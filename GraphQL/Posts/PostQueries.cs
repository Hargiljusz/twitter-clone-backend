using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GraphQL.Posts
{
    public class PostQueries
    {
        public async Task<PageWrapper<PostDTO>> GetPosts([Service] IPostService postService,int size = 10, int page = 0)
        {
            var result = await postService.GetAllPageable(size, page);
            return result;
        }

        public async Task<PostDTO> GetPost([Service] IPostService postService, ClaimsPrincipal user, string postId)
        {
            var result = await postService.GetById(postId);
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var userEmail = user.FindFirstValue(ClaimTypes.Email);
                var temp = (await((PostService)postService).CheckLikesAndSharesByUserEmail(new List<PostDTO> { result }, userEmail)).FirstOrDefault();
                result = temp!;
            }
            return result;
        }

        public async Task<PageWrapper<PostDTO>> Feed([Service] IPostService postService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var feed = await postService.Feed(userDb.Id.ToString(),page,size);
            
            return feed!;
        }

        public async Task<PageWrapper<PostDTO>> MyLikedPosts([Service] IPostService postService,[Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            try
            {
                var result = await postService.GetLikedPostByUserIdPageableSortByDate(userId, size, page);
                return result;
            }
            catch (PostTransactionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<PageWrapper<PostDTO>> MySharedPosts([Service] IPostService postService,[Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            try
            {
                var result = await postService.GetSharePostByUserIdPageableSortByDate(userId, size, page);
                return result;
            }
            catch (PostTransactionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        public async Task<PageWrapper<PostDTO>> GetMyPosts([Service] IPostService postService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int size = 10, int page = 0)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDB = await userManager.FindByEmailAsync(userEmail);
            var userId = userDB.Id.ToString();
            var result = await postService.GetAllPostByUserIdPageableSortByDate(userId, size, page);
            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetSubPostForUser([Service] IPostService postService, string userId, int size = 10, int page = 0)
        {
            var result = await postService.GetAllSubpostForPostByUserIdPageableSortByDate(userId, size, page);
            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetSubpostForPost([Service] IPostService postService, ClaimsPrincipal user, string postId, int size = 10, int page = 0)
        {
            var result = await postService.GetSubpostsForPostByIdSortNewestAndPageable(postId, size, page);

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                result.Content = await ((PostService)postService).CheckLikesAndSharesByUserEmail( result.Content.ToList(), user.FindFirstValue(ClaimTypes.Email));
            }
            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetNewestPostByTag([Service] IPostService postService, ClaimsPrincipal user, string tag, int size = 10, int page = 0)
        {
            var result = await postService.GetNewestPostByTag(tag, size, page);
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                result.Content = await ((PostService)postService).CheckLikesAndSharesByUserEmail(result.Content.ToList(), user.FindFirstValue(ClaimTypes.Email));
            }
            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetPopularPostByTag([Service] IPostService postService, ClaimsPrincipal user, string tag, int size = 10, int page = 0)
        {
            var result = await postService.GetPopularPostByTag(tag, size, page);
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                result.Content = await ((PostService)postService).CheckLikesAndSharesByUserEmail(result.Content.ToList(), user.FindFirstValue(ClaimTypes.Email));
            }
            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetPostByUserId([Service] IPostService postService,ClaimsPrincipal user, string userId, int pageSize = 10, int pageNumber = 0)
        {
            var result = await postService.GetAllPostByUserIdPageableSortByDate(userId, pageSize, pageNumber);
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                result.Content = await ((PostService)postService).CheckLikesAndSharesByUserEmail(result.Content.ToList(), user.FindFirstValue(ClaimTypes.Email));
            }
            return result;
        }
    }

    public class PostQueriesExtension : ObjectTypeExtension<PostQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<PostQueries> descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor.Field(p=>p.GetPosts(default!,default!,default!))
                .Description("Get All Posts");

            descriptor.Field(p => p.GetPost(default!, default!, default!))
                .Description("Get Post By Id");

            descriptor.Field(p => p.Feed(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("Get Feed");

            descriptor.Field(p => p.MyLikedPosts(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("My Liked Posts");


            descriptor.Field(p => p.MySharedPosts(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("My Shared (Reposted) Posts");


            descriptor.Field(p => p.GetMyPosts(default!, default!, default!, default!, default!))
                .Authorize()
                .Description("Get My Posts");

            descriptor.Field(p => p.GetSubPostForUser(default!, default!, default!, default!))
                .Description("Get all created subpost by user");


            descriptor.Field(p => p.GetSubpostForPost(default!, default!, default!, default!, default!))
                .Description("Get all subpost for post");

            descriptor.Field(p => p.GetNewestPostByTag(default!, default!,default!, default!, default!))
                .Description("Get all newest post by tag");

            descriptor.Field(p => p.GetPopularPostByTag(default!, default!,default!, default!, default!))
                .Description("Get all popular post by tag");

            descriptor.Field(p => p.GetPostByUserId(default!, default!, default!, default!, default!))
                .Description("Get post by user id");

        }
    }
}
