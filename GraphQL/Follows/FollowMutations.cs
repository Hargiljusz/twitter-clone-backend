using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services.Interfaces.Documents;
using GraphQL.TypesUtils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GraphQL.Follows
{
    public class FollowMutations
    {
        public async Task<FollowerDTO> Follow([Service] UserManager<ApplicationUser> userManager, [Service] IFollowService followService, ClaimsPrincipal user, string followUserId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            var result = await followService.FollowUser(userId, followUserId);

            return result;
        }

        public async Task<FollowerDTO> Unfollow([Service] UserManager<ApplicationUser> userManager, [Service] IFollowService followService, ClaimsPrincipal user, string followUserId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            try
            {
                var result = await followService.UnFollowUser(userId, followUserId);

                return result;
            }
            catch (FollowerNotFoundExceptions ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }

    public class FollowMutationsExtension : ObjectTypeExtension<FollowMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<FollowMutations> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor.Field(f => f.Follow(default!, default!, default!, default!))
                .Authorize()
                .Description("Follow user")
                .UseMutationConvention();

            descriptor.Field(f => f.Unfollow(default!, default!, default!, default!))
                .Authorize()
                .Description("Unfollow user")
                .UseMutationConvention();
        }
    }
}
