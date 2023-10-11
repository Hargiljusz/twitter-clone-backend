using AutoMapper;
using DataCommon;
using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.DTO.Utils;
using DataService.Exceptions;
using DataService.Repository;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataService.Services
{
    //add exceptions
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IIgnoredRepository _ignoredRepository;
        private readonly IFollowersRepository _followersRepository;
        private readonly IDbClient _dbClient;
        private readonly IFileService _fileService;

        public UserService(UserManager<ApplicationUser> userManager, IMapper mapper, IUserRepository userRepository, IIgnoredRepository ignoredRepository, IFollowersRepository followersRepository, IDbClient dbClient, IFileService fileService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userRepository = userRepository;
            _ignoredRepository = ignoredRepository;
            _followersRepository = followersRepository;
            _dbClient = dbClient;
            _fileService = fileService;
        }

        public async Task<Warning> AddWarning(string adminId, string Description,string userId)
        {
            var warning = new Warning(adminId, DateTime.Now, Description);
            var result = await _userRepository.PushWarning(userId, warning);
            if(result.MatchedCount == 0 || result.ModifiedCount == 0)
            {
                throw new UserNotFoundException("User not found for this Id");
            }
            return warning;
        }

        public async Task<bool> BanUserByUserId(string userId, string adminId, string description)
        {
            var result = await _userRepository.BanUserById(userId,adminId,description);
            if (result.MatchedCount == 0 || result.ModifiedCount == 0)
            {
                throw new UserNotFoundException("User not found for this Id");
            }
            return result.ModifiedCount == 1;
        }

        public async Task<bool> BlockUser(string userId, string blockUserId)
        {
            IgnoredUser ignoredUser = new(blockUserId, DateTime.Now);
            var result = await _ignoredRepository.PushNewUserToIgnoredUsersByUserId(userId, ignoredUser);

            if(result.MatchedCount == 0 || result.ModifiedCount == 0)
            {
                throw new UserNotFoundException("User not found for this Id");
            }
           
            var filter1 = Builders<Follower>.Filter.Eq(f=>f.From,userId) & Builders<Follower>.Filter.Eq(f=>f.To,blockUserId);
            
            var filter2 = Builders<Follower>.Filter.Eq(f => f.From, blockUserId) & Builders<Follower>.Filter.Eq(f => f.To, userId);

            var filter = Builders<Follower>.Filter.Or(filter1,filter2);
            var follows = await _followersRepository.Find(filter);

            var removeResult = await _followersRepository.RemoveRange(follows);
            return true;
        }

        public async Task<bool> CreateUser(CreateUser userRegister)
        {
            if(userRegister.Password != userRegister.PasswordConfirm)
            {
                throw new PasswordNotCorrectException("Passwords are not the same");
            }

            ApplicationUser user = new() 
            {
                Email = userRegister.Email,
                UserName = userRegister.UserName,
                Nick = userRegister.Nick,
                PhoneNumber = userRegister.PhoneNumber,
                Name = userRegister.Name,
                Surename = userRegister.Surename,
                AboutMe = userRegister.AboutMe,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            };

            var resultCreate = await _userManager.CreateAsync(user,userRegister.Password);
            var resultRole =await _userManager.AddToRoleAsync(user, UserRoles.User);

            if(resultCreate.Succeeded & resultRole.Succeeded)
            {
                await _ignoredRepository.Add(new Ignored
                {
                    CreatedAt = DateTime.Now,
                    UserId = user.Id.ToString()
                });
            }

            return resultCreate.Succeeded & resultRole.Succeeded;
        }

        public async Task<bool> CreateUser(CreateUser userRegister, Stream mainPhotoStream, string mainPhotoName, Stream backgroundPhotoStream, string backgroundPhotoName)
        {
            if (userRegister.Password != userRegister.PasswordConfirm)
            {
                throw new PasswordNotCorrectException("Passwords are not the same");
            }

            ApplicationUser user = new()
            {
                Email = userRegister.Email,
                UserName = userRegister.UserName,
                Nick = userRegister.Nick,
                PhoneNumber = userRegister.PhoneNumber,
                Name = userRegister.Name,
                Surename = userRegister.Surename,
                AboutMe = userRegister.AboutMe,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            };

            var resultCreate = await _userManager.CreateAsync(user, userRegister.Password);
            var resultRole = await _userManager.AddToRoleAsync(user, UserRoles.User);

            if (resultCreate.Succeeded & resultRole.Succeeded)
            {
                await _ignoredRepository.Add(new Ignored
                {
                    CreatedAt = DateTime.Now,
                    UserId = user.Id.ToString()
                });
            }

            var saveMainPhotoTask =  _fileService.SaveFile(mainPhotoStream, user.Id.ToString(), mainPhotoName);
            var savebackgroundPhotoTask =  _fileService.SaveFile(backgroundPhotoStream, user.Id.ToString(), backgroundPhotoName);
            await Task.WhenAll(saveMainPhotoTask, savebackgroundPhotoTask);

            user.Photo = saveMainPhotoTask.Result;
            user.BackgroundPhoto = savebackgroundPhotoTask.Result;

            await _userRepository.Update(user);
            return resultCreate.Succeeded & resultRole.Succeeded;
        }

        public async Task<PageWrapper<UserDTO>> GetAll(int pageSize = 10, int pageNumber = 0)
        {
            var users = await _userRepository.FindPageable(_ => true,null,pageSize,pageNumber);
            var usersDTO = await _appliacationUsersToUsersDTO(users.Content.ToList());

            return new PageWrapper<UserDTO>(usersDTO,users.PageSize,users.PageNumber,users.TotalPageCount);
        }

        public async Task<PageWrapper<UserDTO>> GetBlockUserByUserIdPageableSort(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var mongoClient = _dbClient.GetDatabase().Client;

            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var ignored = _dbClient.GetIgnoredCollection();
                    var projection = Builders<Ignored>.Projection.Exclude(i => i.Id).Include(i => i.IgnoredUsers);

                    var aggregate = ignored.Aggregate()
                      .Match(l => l.UserId == userId)
                      .SortByDescending(i => i.CreatedAt)
                      .Project<IEnumerable<IgnoredUser>>(projection);

                    var aggreagationResult = await aggregate.ToListAsync();

                    var ignoredUsers = aggreagationResult.FirstOrDefault();
                    if (ignoredUsers is null)
                    {
                        throw new UserNotFoundException("User not found");
                    }

                    var ignoredUsersId = ignoredUsers.Select(iu => iu.UserId);
                    var totalPages = Convert.ToInt32(Math.Ceiling(ignoredUsersId.Count() / (double)pageSize));

                    var ignoredUsersIdPage = ignoredUsersId.Skip(pageSize * pageNumber).Take(pageSize);


                    var tasks = new List<Task<ApplicationUser>>();
                    ignoredUsersIdPage.ToList().ForEach(id => tasks.Add(_userManager.FindByIdAsync(id)));
                    await Task.WhenAll(tasks);
                    var users = tasks.Select(t => t.Result).ToList();
                    var usersDTO = await _appliacationUsersToUsersDTO(users);

                    await session.CommitTransactionAsync();

                    return new PageWrapper<UserDTO>(usersDTO, pageSize, pageNumber, totalPages);
                }catch(Exception ex)
                {
                    await session.AbortTransactionAsync();
                    if(ex is UserNotFoundException)
                    {
                        throw;
                    }
                    throw new UserTransactionException("User Transaction Error");
                }
            }
        }

        public async Task<UserDTO> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                throw new UserNotFoundException("User not found for this Id");
            }

            var userDTO = await _appliacationUserToUserDTO(user);
            return userDTO;
        }

        public async Task<IEnumerable<Report>> GetReportsByUserId(string userId)
        {
            var result = await _userRepository.GetById(userId);

            if(result is null)
            {
                throw new UserNotFoundException("User not found for this Id");
            }

            return result.Reports;
        }

        public async Task<IEnumerable<Warning>> GetWarningsByUserId(string userId)
        {
            var result = await _userRepository.GetById(userId);

            if (result is null)
            {
                throw new UserNotFoundException("User not found for this Id");
            }

            return result.Warnings;
        }

        //For ADMIN
        public async Task<UserDTO> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var userDTO = await _appliacationUserToUserDTO(user);

            if (user is null)
            {
                throw new UserNotFoundException("User not found for this Id");
            }
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new DeleteUserException("Remove user excepption");
            }

            return userDTO;
        }

        public async Task<bool> ReportUser(string userId, string reportUserId,string description)
        {
            Report report = new(userId,DateTime.Now,description);
            var result = await _userRepository.PushReport(reportUserId, report);
            if (result.MatchedCount == 0 || result.ModifiedCount == 0)
            {
                throw new UserNotFoundException("User not found for this Id");
            }
            return result.ModifiedCount == 1;
        }

        public async Task<PageWrapper<UserDTO>> SearchPageable(string q, SortDefinition<ApplicationUser> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            string pattern = @"\b" + q + @"\w+";
            var rx = new Regex(pattern, RegexOptions.IgnoreCase);

            var filterBuilder = Builders<ApplicationUser>.Filter;
            var filter1 = filterBuilder.Regex(u => u.Nick, new BsonRegularExpression(rx));
            var filter2 = filterBuilder.Regex(u => u.UserName, new BsonRegularExpression(rx));
            var filter3 = filterBuilder.Regex(u => u.Name, new BsonRegularExpression(rx));
            var filter4 = filterBuilder.Regex(u => u.Surename, new BsonRegularExpression(rx));
            var filter5 = filterBuilder.Regex(u => u.Email, new BsonRegularExpression(rx));
           
            var filter =filterBuilder.Or( filter1 , filter2 , filter3 ,filter4 , filter5);
            var users = await _userRepository.FindPageable(
                filter
                ,sort
                ,pageSize
                ,pageNumber);
            var listOfUsers = users.Content.ToList();

            var result = await _appliacationUsersToUsersDTO(listOfUsers);

            return new PageWrapper<UserDTO>(result,users.PageSize,users.PageNumber,users.TotalPageCount);
        }

        public async Task<bool> UnBanUserByUserId(string userId)
        {
            var result = await _userRepository.UnBanUserById(userId);
            if (result.MatchedCount == 0 || result.ModifiedCount == 0)
            {
                throw new UserNotFoundException("User not found for this Id");
            }
            return result.ModifiedCount == 1;
        }

        public async Task<UserDTO> UpdateDataByUserId(string userId, UpdateUser updateUser)
        {
            ApplicationUser user = new()
            {
                UserName = updateUser.UserName,
                Nick = updateUser.Nick,
                PhoneNumber = updateUser.PhoneNumber,
                Name = updateUser.Name,
                Surename = updateUser.Surename,
                AboutMe = updateUser.AboutMe
            };
            var updateResult =await _userRepository.Update(user);

            if(updateResult.MatchedCount == 0)
            {
                throw new UserNotFoundException("User not found for this Id");
            }

            var result = await _userRepository.GetById(userId);
            return await _appliacationUserToUserDTO(result);
        }


        private async Task<List<UserDTO>> _appliacationUsersToUsersDTO(List<ApplicationUser> listOfUsers)
        {
            var tasks = listOfUsers.Select(u => _appliacationUserToUserDTO(u)).ToList();
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToList();
        }


        private async Task<UserDTO> _appliacationUserToUserDTO(ApplicationUser user)
        {
            var rolse = await _userManager.GetRolesAsync(user);
            var userDTO = _mapper.Map<UserDTO>(user);
            
            userDTO.RolesDTO = rolse.ToList();
            return userDTO;
        }
    }
}
