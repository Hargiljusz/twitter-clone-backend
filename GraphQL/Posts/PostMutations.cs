using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using GraphQL.TypesUtils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GraphQL.Posts
{
    public class PostMutations
    {
        public async Task<PostDTO> AddPost([Service] IPostService postService, PostCreate input)
        {
            var post = new PostDTO
            {
                Content = input.Contetnt,
                PostFor = input.PostFor,
                CreateByUserId = input.CreateByUserId
            };
            var result = await postService.Add(post,null);
            return result;
        }
        public async Task<PostDTO> AddPostWithFile([Service] IPostService postService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, PostCreate postCreate, List<IFile>? files = null)
        {
            var post = new PostDTO
            {
                Content = postCreate.Contetnt,
                PostFor = postCreate.PostFor,
                CreateByUserId = postCreate.CreateByUserId
            };

            if(files is not null)
            {
                var postFiles = files.Select(f => new PostFileWrapper(f.OpenReadStream(), f.Name)).ToList();
                var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                var userDb = await userManager.FindByEmailAsync(userEmail);
                var userId = userDb.Id.ToString();
                var newPost = await postService.Add(post,userId,postFiles);
                return newPost;
            }

            var result = await postService.Add(post);
            return result;
        }

        public async Task<PostDTO> UpdatePost([Service] IPostService postService,string postId, PostDTO input)
        {
            input.Id = postId;
            var result = await postService.Update(postId,input);
            return result;
        }

        public async Task<ResultBool> DeletePost([Service] IPostService postService, string postId)
        {
            var result = await postService.DeleteByPostId(postId);
            return new ResultBool(result,DateTime.Now);
        }

        public async Task<ResultBool> IgnoreAllPosts([Service] IPostService postService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, string ignoreUserId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            var result = await postService.IgnoreAllPosts(userId, ignoreUserId);
            return new ResultBool(result, DateTime.Now);
        }

        public async Task<ResultBool> IgnorePost([Service] IPostService postService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, string postId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            var result = await postService.IgnorePostByPostId(userId, postId);
            return new ResultBool(result, DateTime.Now);
        }

        public async Task<ResultBool> Respost([Service] IPostService postService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, string postId)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var userId = userDb.Id.ToString();
            var result = await postService.RepostPost(userId, postId);
            return new ResultBool(result, DateTime.Now);
        }

    }

    public class PostMutationsExtension : ObjectTypeExtension<PostMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<PostMutations> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor.Field(p => p.AddPost(default!, default!))
                .Description("Create new Post");

            descriptor.Field(p => p.UpdatePost(default!, default!, default!))
                .Description("Update Post By Id");

            descriptor.Field(p => p.DeletePost(default!, default!))
                .Description("Delete Post By Id");

            descriptor.Field(p => p.IgnoreAllPosts(default!, default!, default!, default!))
                .Authorize()
                .Description("Ignore post author");

            descriptor.Field(p => p.IgnorePost(default!, default!, default!, default!))
                .Authorize()
                .Description("Ignore post");

            descriptor.Field(p => p.Respost(default!, default!, default!, default!))
                .Authorize()
                .Description("Repost post");

            descriptor.Field(p => p.AddPostWithFile(default!, default!, default!, default!, default!))
                .Authorize()
                .UseMutationConvention()
                .Description("Add new post with files");
        }
    }

    public record PostCreate(string Contetnt, string PostFor, string CreateByUserId);
}
