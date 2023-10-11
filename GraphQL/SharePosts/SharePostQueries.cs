using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;

namespace GraphQL.SharePosts
{
    public class SharePostQueries 
    {
        public Task<PageWrapper<SharePostDTO>> GetSharePosts([Service] ISharePostService sharePostService, int size = 10, int page = 0)
        {
            var result = sharePostService.GetAllPageable(size,page);
            return result;
        }

        public Task<SharePostDTO> GetSharePost([Service] ISharePostService sharePostService, string postId)
        {
            var result = sharePostService.GetById(postId);
            return result;
        }

        public async Task<long> GetNumberOfShares([Service] ISharePostService sharePostService, string postId)
        {
            var result = await sharePostService.NumberOfSharedPosForPost(postId);
            return result;
        }

        public async Task<PageWrapper<SharePostDTO>> GetMySharedPosts([Service] ISharePostService sharePostService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var sort = Builders<SharePost>.Sort.Descending(sh => sh.CreatedAt);
            var result = await sharePostService.GetAllSharedPostByUserIdPageableAndSort(userDb.Id.ToString(), sort, size, page);
            return result;
        }
    }

    public class SharePostQueriesExtension : ObjectTypeExtension<SharePostQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<SharePostQueries> descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor.Field(q => q.GetSharePosts(default!, default!, default!))
                .Authorize()
                .Description("Get All Share Post");

            descriptor.Field(q => q.GetSharePost(default!, default!))
                .Name("myShares")
                .Authorize()
                .Description("Get Share Post");

            descriptor.Field(q => q.GetNumberOfShares(default!, default!))
                .Type<LongType>()
                .Description("Get Number od Shares for Posts");

            descriptor.Field(q => q.GetMySharedPosts(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("Get My Share Posts");
        }
    }

}
