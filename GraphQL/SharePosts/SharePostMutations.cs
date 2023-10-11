using DataService.DTO;
using DataService.Services.Interfaces.Documents;
using GraphQL.TypesUtils;

namespace GraphQL.SharePosts
{
    public class SharePostMutations
    {
        public async Task<SharePostDTO> AddSharePost([Service] ISharePostService sharePostService, SharePostDTO input)
        {
            var result = await sharePostService.Add(input);
            return result;
        }

        public async Task<SharePostDTO> UpdateSharePost([Service] ISharePostService sharePostService,string id, SharePostDTO input)
        {
            input.Id = id;
            var result = await sharePostService.Update(id,input);
            return result;
        }

        public async Task<ResultBool> DeleteSharePost([Service] ISharePostService sharePostService, string id)
        {
            var result = await sharePostService.RemoveSharedPostById(id);
            return new ResultBool(result,DateTime.Now);
        }

        public async Task<ResultBool> DeleteSharePost([Service] ISharePostService sharePostService, string userId, string postId)
        {
            var removeResult = await sharePostService.RemoveSharedPostByUserIdAndPostId(userId, postId);
            return new ResultBool(removeResult, DateTime.Now);
        }
    }

    public class SharePostMutationsExtension : ObjectTypeExtension<SharePostMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<SharePostMutations> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor.Field(sh => sh.AddSharePost(default!, default!))
                .Authorize()
                .Description("Create new Share Post");

            descriptor.Field(sh => sh.UpdateSharePost(default!, default!, default!))
                .Authorize()
                .Description("Update existing Share Post");

            descriptor.Field(sh => sh.DeleteSharePost(default!, default!))
                .Authorize()
                .Description("Delete Share Post By Id");

            descriptor.Field(sh => sh.DeleteSharePost(default!, default!, default!))
               .Authorize()
               .Description("Delete Share Post By Id");
        }
    }
}
