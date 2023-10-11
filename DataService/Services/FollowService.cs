
using AutoMapper;
using DataCommon;
using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class FollowService : IFollowService
    {
        private readonly IMapper _mapper;
        private readonly IFollowersRepository _followersRepositor;
        private readonly IDbClient _dbClient;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowService(IMapper mapper, IFollowersRepository followersRepositor, IDbClient dbClient, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _followersRepositor = followersRepositor;
            _dbClient = dbClient;
            _userManager = userManager;
        }

        public async Task<FollowerDTO> Add(FollowerDTO document)
        {
            var follow = _mapper.Map<Follower>(document);
            await _followersRepositor.Add(follow);
            return _mapper.Map<FollowerDTO>(follow);
        }

        public async Task<IEnumerable<FollowerDTO>> AddRange(IEnumerable<FollowerDTO> documents)
        {
            var follows = _mapper.Map<IEnumerable<Follower>>(documents);
            await _followersRepositor.AddRange(follows);
            return _mapper.Map<IEnumerable<FollowerDTO>>(follows);
        }

        public async Task<CheckFollow> CheckFollow(string userId, string checkUserId)
        {
            var isExistFrom = (await _followersRepositor.Find(f => f.From == userId && f.To == checkUserId)).FirstOrDefault();
            var isExistTo = (await _followersRepositor.Find(f => f.From == checkUserId && f.To == userId)).FirstOrDefault();

            var checkFollow = new CheckFollow() { UserId = userId, CheckUserId =  checkUserId, IsFollowing = true,IsFollower = true};
            if (isExistFrom is null)
            {
                checkFollow.IsFollowing = false;
            }

            if (isExistTo is null)
            {
                checkFollow.IsFollower = false;
            }
            return checkFollow;
        }

        public async Task<FollowerDTO> FollowUser(string myUserId, string followUserId)
        {
            var follow = new Follower(myUserId, followUserId, DateTime.Now, BackendType.RestAPI);
            var checkFollowId = _userManager.FindByIdAsync(followUserId);
            var checkMyId = _userManager.FindByIdAsync(myUserId);

            await Task.WhenAll(checkFollowId, checkMyId);

            if (checkFollowId.Result is null )
            {
                throw new UserNotFoundException($"User Not Found For Id: {followUserId}");
            }

            if(checkMyId.Result is null)
            {
                throw new UserNotFoundException($"User Not Found For Id: {myUserId}");
            }

            await _followersRepositor.Add(follow);
            return _mapper.Map<FollowerDTO>(follow);
        }

        public async Task<IEnumerable<FollowerDTO>> GetAll()
        {
           var result = await _followersRepositor.GetAll();
           var followsDTO = _mapper.Map<IEnumerable<FollowerDTO>>(result);
           return followsDTO;
        }

        public async Task<PageWrapper<FollowerDTO>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var result = await _followersRepositor.GetAllPageable(pageSize, pageNumber);
            var followPageable = _mapper.Map<PageWrapper<FollowerDTO>>(result);
            return followPageable;
        }

        public async Task<FollowerDTO> GetById(string id)
        {
            var result = await _followersRepositor.GetById(id);

            if (result is null)
            {
                throw new FollowerNotFoundExceptions("Follow not found");
            }
            var follow = _mapper.Map<FollowerDTO>(result);
            return follow;
        }

        public async Task<long> GetFollowersByUserId(string userId)
        {
            return await _followersRepositor.GetNumberOfFollowersByUserId(userId);
        }

        public async Task<long> GetFollowingByUserId(string userId)
        {
           return await _followersRepositor.GetNumberOfFollowingsByUserId(userId);
        }

        //wez kto mnie obserwuje
        public async Task<PageWrapper<UserDTO>> GetListOfFollowerUsersPeageableAndSort(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var mongoClient = _dbClient.GetDatabase().Client;

            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {

                    #region getIds
                    var projection = Builders<Follower>.Projection.Exclude(f => f.Id).Include(f => f.From);

                    var followerCollection = _dbClient.GetFollowersCollection();

                    #region count facet
                    var countFacet = AggregateFacet.Create("count",
                    PipelineDefinition<string, AggregateCountResult>.Create(new[]
                    {
                PipelineStageDefinitionBuilder.Count<string>()
                    }));
                    #endregion


                    #region data facet
                    AggregateFacet<string, string> dataFacet = AggregateFacet.Create("data",
                            PipelineDefinition<string, string>.Create(new[]
                                {
                            PipelineStageDefinitionBuilder.Skip<string>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<string>(pageSize),
                                }
                            )
                        );
                    #endregion

                    var aggregate = followerCollection.Aggregate()
                        .Match(f => f.To == userId)
                        .SortByDescending(f => f.CreatedAt)
                        .Project<string>(projection)
                        .Facet(countFacet, dataFacet);

                    var aggreagationResult = await aggregate.ToListAsync();
                    var count = aggreagationResult.First()
                      .Facets.First(x => x.Name == "count")
                      .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

                    var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

                    var data = aggreagationResult.First()
                        .Facets.First(x => x.Name == "data")
                        .Output<string>().ToList();
                    #endregion

                    var tasks = new List<Task<ApplicationUser>>();
                    data.ToList().ForEach(id => tasks.Add(_userManager.FindByIdAsync(id)));
                    await Task.WhenAll(tasks);
                    var users = tasks.Select(t => t.Result);
                    var userDTO = _mapper.Map<IEnumerable<UserDTO>>(users);

                    await session.CommitTransactionAsync();

                    return new PageWrapper<UserDTO>(userDTO, pageSize, pageNumber, totalPages);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw new FollowerTransactionException("Follower transaction error");
                }
            }
        }

        //wez moje obserwacje
        public async Task<PageWrapper<UserDTO>> GetListOfFollowingUsersPeageableAndSort(string userId, int pageSize = 10, int pageNumber = 0)
        {var mongoClient = _dbClient.GetDatabase().Client;

            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    #region getIds
                    var projection = Builders<Follower>.Projection.Exclude(f => f.Id).Include(f => f.To);

                    var followerCollection = _dbClient.GetFollowersCollection();

                    #region count facet
                    var countFacet = AggregateFacet.Create("count",
                    PipelineDefinition<string, AggregateCountResult>.Create(new[]
                    {
                        PipelineStageDefinitionBuilder.Count<string>()
                    }));
                    #endregion


                    #region data facet
                    AggregateFacet<string, string> dataFacet = AggregateFacet.Create("data",
                            PipelineDefinition<string, string>.Create(new[]
                                {
                            PipelineStageDefinitionBuilder.Skip<string>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<string>(pageSize),
                                }
                            )
                        );
                    #endregion

                    var aggregate = followerCollection.Aggregate()
                        .Match(f => f.From == userId)
                        .SortByDescending(f => f.CreatedAt)
                        .Project<string>(projection)
                        .Facet(countFacet, dataFacet);

                    var aggreagationResult = await aggregate.ToListAsync();
                    var count = aggreagationResult.First()
                      .Facets.First(x => x.Name == "count")
                      .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

                    var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

                    var data = aggreagationResult.First()
                        .Facets.First(x => x.Name == "data")
                        .Output<string>().ToList();
                    #endregion

                    var tasks = new List<Task<ApplicationUser>>();
                    data.ToList().ForEach(id => tasks.Add(_userManager.FindByIdAsync(id)));
                    await Task.WhenAll(tasks);
                    var users = tasks.Select(t => t.Result);
                    var userDTO = _mapper.Map<IEnumerable<UserDTO>>(users);

                    await session.CommitTransactionAsync();

                    return new PageWrapper<UserDTO>(userDTO, pageSize, pageNumber, totalPages);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw new FollowerTransactionException("Follower transaction error");
                }
            }
        }

        public async Task<PageWrapper<UserDTO>> GetListOfPropositionUsersPeageableAndSort(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var mongoClient = _dbClient.GetDatabase().Client;

            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {

                    var follwerCollection = _dbClient.GetFollowersCollection();
                    var projection = Builders<Follower>.Projection.Exclude(f => f.Id).Include(f => f.To);
                    var aggregate1 = follwerCollection.Aggregate()
                        .Match(f => f.From == userId)
                        .SortByDescending(f => f.CreatedAt)
                        .Project<string>(projection);

                    var followingsId = await aggregate1.ToListAsync();


                    var aggreagation2 = follwerCollection.Aggregate()
                        .Match(f => f.From == userId)
                        .SortByDescending(f => f.CreatedAt)
                        .Project<string>(projection);
                    var tasks = new List<Task<List<string>>>();
                    followingsId.ForEach(id =>
                    {
                        tasks.Add(getFollowingsById(id));
                    });

                    await Task.WhenAll(tasks);

                    var propositionalIds = tasks
                        .Select(t => t.Result)
                        .SelectMany(f => f)
                        .Distinct() // remove duplicates
                        .ToList();

                    propositionalIds.RemoveAll(id => followingsId.Contains(id)); // remove id which user is following

                    var pagedPropositionalIds = propositionalIds.Skip(pageSize * pageNumber).Take(pageSize);

                    var totalPages = Convert.ToInt32(Math.Ceiling(propositionalIds.Count() / (double)pageSize));

                    var tasks2 = new List<Task<ApplicationUser>>();
                    pagedPropositionalIds.ToList().ForEach(id => tasks2.Add(_userManager.FindByIdAsync(id)));

                    await Task.WhenAll(tasks2);
                    var users = tasks2.Select(t => t.Result);
                    var usersDTO = _mapper.Map<IEnumerable<UserDTO>>(users);

                    await session.CommitTransactionAsync();

                    return new PageWrapper<UserDTO>(usersDTO, pageSize, pageNumber, totalPages);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw new FollowerTransactionException("Follower transaction error");
                }
            }
        }

        public async Task<bool> Remove(FollowerDTO document)
        {
            var follower = _mapper.Map<Follower>(document);
            var result = await _followersRepositor.Remove(follower);
            return result;
        }

        public async Task<bool> RemoveRange(IEnumerable<FollowerDTO> documents)
        {
            var followers = _mapper.Map<IEnumerable<Follower>>(documents);
            var result = await _followersRepositor.RemoveRange(followers);
            return result;
        }

        public async Task<FollowerDTO> UnFollowUser(string myUserId, string followUserId)
        {
            var filter = Builders<Follower>.Filter.Eq(f=>f.From,myUserId) & Builders<Follower>.Filter.Eq(f=>f.To,followUserId);
            var result = await _followersRepositor.Find(filter);
            var follow = result.FirstOrDefault();

            if(follow is null)
            {
                throw new FollowerNotFoundExceptions("Follow not found");
            }

            await _followersRepositor.Remove(follow);
            return _mapper.Map<FollowerDTO>(follow);
        }

        public async Task<FollowerDTO> Update(string id, FollowerDTO document)
        {
            document.Id = id;
            var follower = _mapper.Map<Follower>(document);
            var result = await _followersRepositor.Update(follower);

            if (result.MatchedCount == 0)
            {
                throw new FollowerNotFoundExceptions("Follow not found");
            }
            return _mapper.Map<FollowerDTO>(follower);
        }

        private async Task<List<string>> getFollowingsById(string userId)
        {
            var follwerCollection = _dbClient.GetFollowersCollection();
            var projection = Builders<Follower>.Projection.Exclude(f => f.Id).Include(f => f.To);
            return await follwerCollection.Aggregate()
                .Match(f => f.From == userId)
                .SortByDescending(f => f.CreatedAt)
                .Project<string>(projection).ToListAsync();
        }


    }
}
