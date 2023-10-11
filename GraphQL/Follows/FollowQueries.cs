using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GraphQL.Follows
{
    public class FollowQueries
    {
        public async Task<long> GetFollowersNumber([Service] IFollowService followService,string userId)
        {
            var count = await followService.GetFollowersByUserId(userId);
            return count;
        }

        public async Task<long> GetFollowingNumber([Service] IFollowService followService, string userId)
        {
            var count = await followService.GetFollowingByUserId(userId);
            return count;
        }

        public async Task<PageWrapper<UserDTO>> GetFollowProposition([Service] UserManager<ApplicationUser> userManager, [Service] IFollowService followService, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            var result = await followService.GetListOfPropositionUsersPeageableAndSort(userId, size, page);
            return result;
        }

        public async Task<PageWrapper<UserDTO>> GetMyFollowers([Service] UserManager<ApplicationUser> userManager, [Service] IFollowService followService, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            try { 
                var result = await followService.GetListOfFollowerUsersPeageableAndSort(userId, size, page);
                return result;
            }
            catch (FollowerTransactionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<PageWrapper<UserDTO>> GetMyFollowings([Service] UserManager<ApplicationUser> userManager, [Service] IFollowService followService, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            try
            {
                var result = await followService.GetListOfFollowingUsersPeageableAndSort(userId, size, page);
                return result;
            }
            catch (FollowerTransactionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<CheckFollow> CheckFollow([Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user , [Service] IFollowService followService, string checkUserId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            var result = await followService.CheckFollow(userId, checkUserId);
            return result;
        }
    }

    public class FollowQueriesExtension : ObjectTypeExtension<FollowQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<FollowQueries> descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor.Field(f => f.GetFollowersNumber(default!, default!))
                .Type<LongType>()
                .Description("Get followers number");

            
            descriptor.Field(f => f.GetFollowingNumber(default!, default!))
                .Type<LongType>()
                .Description("Get following number");

            descriptor.Field(f => f.GetFollowProposition(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("Get follow proposition");


            descriptor.Field(f => f.GetMyFollowers(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("Get my followers");


            descriptor.Field(f => f.GetMyFollowings(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("Get my followings");

            descriptor.Field(f => f.CheckFollow(default!, default!, default!, default!))
                .Authorize()
                .Description("Check Follow");
        }
    }
}
