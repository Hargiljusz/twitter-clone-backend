using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;

namespace GraphQL.Likes
{
    public class LikeQueries
    {
        public async Task<PageWrapper<LikesDTO>> GetLikes([Service] ILikeService likeService,int size = 10, int page = 0)
        {
            var pagedLikes = await likeService.GetAllPageable(size, page);
            return pagedLikes;
        }

        public async Task<LikesDTO> GetLike([Service] ILikeService likeService,string likeId)
        {
            var result = await likeService.GetById(likeId);
            return result; 
        }

        public async Task<long> GetNumberOfLikes([Service] ILikeService likeService, string postId)
        {
            return await likeService.NumberOfLikesForPost(postId);
        }

        public async Task<PageWrapper<LikesDTO>> GetMyLikedPosts([Service] ILikeService likeService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDB = await userManager.FindByEmailAsync(userEmail);
            var userId = userDB.Id.ToString();
            var sort = Builders<DataCommon.Models.Documents.Likes>.Sort.Descending(sh => sh.CreatedAt);
            var result = await likeService.GetAllLikesByUserIdPageableAndSort(userId, sort, size, page);
            return result;
        }
    }

    public class LikeQueriesExtension : ObjectTypeExtension<LikeQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<LikeQueries> descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor.Field(l => l.GetLikes(default!, default!, default!))
                .Authorize()
                .Description("Get All Likes");

            descriptor.Field(l => l.GetLike(default!, default!))
                .Authorize()
                .Description("Get Like By Id");

            descriptor.Field(l => l.GetNumberOfLikes(default!, default!))
              .Type<LongType>()
              .Description("Get Likes Number For Post");

            descriptor.Field(l => l.GetMyLikedPosts(default!, default!, default!, default!, default!))
                .Name("myLikes")
              .Authorize()
              .Description("Get All My Likes");

        }
    }
}
