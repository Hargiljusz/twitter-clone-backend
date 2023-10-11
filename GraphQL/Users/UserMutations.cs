using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.DTO.Utils;
using DataService.Exceptions;
using DataService.Services.Interfaces.Documents;
using GraphQL.TypesUtils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GraphQL.Users
{
    public class UserMutations
    {
        public async Task<UserDTO> Put([Service] IUserService userService, string id,UpdateUser input)
        {
            var result = await userService.UpdateDataByUserId(id, input);
            return result;
        }

        public async Task<ResultBool> Delete([Service] IUserService userService, string id)
        {
            try
            {
                var result = await userService.RemoveUser(id);
                return new ResultBool(result is not null, DateTime.Now);
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (DeleteUserException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        public async Task<ResultBool> ReportUser([Service] UserManager<ApplicationUser> userManager, [Service] IUserService userService, ClaimsPrincipal user, string reportUserId, string description)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            try
            {
                var result = await userService.ReportUser(userId, reportUserId, description);
                return new ResultBool(result, DateTime.Now);
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        
        public async Task<ResultBool> BlockUser([Service] UserManager<ApplicationUser> userManager, [Service] IUserService userService, ClaimsPrincipal user, string blockUserId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();

            try
            {
                var result = await userService.BlockUser(userId, blockUserId);
                return new ResultBool(result, DateTime.Now);
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<ResultBool> BanUser([Service] UserManager<ApplicationUser> userManager, [Service] IUserService userService, ClaimsPrincipal user, string banUserId, string description)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var adminUserId = userDb.Id.ToString();
            try
            {
                var result = await userService.BanUserByUserId(adminUserId, banUserId, description);
                return new ResultBool(result, DateTime.Now);
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<ResultBool> AddWarning([Service] UserManager<ApplicationUser> userManager, [Service] IUserService userService, ClaimsPrincipal user, string warningUserId, string description)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var adminUserId = userDb.Id.ToString();
            try
            {
                var result = await userService.AddWarning(adminUserId, description, warningUserId);
                return new ResultBool(result is not null, DateTime.Now);
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<ResultBool> UnBanUser([Service] IUserService userService, string userId)
        {
            try
            {
                var result = await userService.UnBanUserByUserId(userId);
                return new ResultBool(result, DateTime.Now);
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }

    public class UserMutationsExtension : ObjectTypeExtension<UserMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<UserMutations> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);
            descriptor.Field(u => u.Put(default!, default!, default!))
                .Name("updateUser")
                .Description("Update User By Id");

            descriptor.Field(u => u.Delete(default!, default!))
                .Name("removeUser")
                .Description("Remove User By Id");

            descriptor.Field(u => u.ReportUser(default!, default!, default!, default!, default!))
                .Authorize()
                .UseMutationConvention()
                .Description("Report User");

            descriptor.Field(u => u.BlockUser(default!, default!, default!, default!))
                .Authorize()
                .Description("Block User");

            descriptor.Field(u => u.BanUser(default!, default!, default!, default!, default!))
                .Authorize(new[] {UserRoles.Admin} )
                .UseMutationConvention()
                .Description("Ban  User");

            descriptor.Field(u => u.UnBanUser(default!, default!))
                .Authorize(new[] { UserRoles.Admin })
                .UseMutationConvention()
                .Description("Unban  User");

            descriptor.Field(u => u.AddWarning(default!, default!, default!, default!, default!))
                .Authorize(new[] { UserRoles.Admin })
                .UseMutationConvention()
                .Description("Add Warning for User");
        }
    }
}
