using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using GraphQL.TypesUtils;

namespace GraphQL.Likes
{
    public class LikeMutations
    {
        public async Task<LikesDTO> AddLike([Service] ILikeService likeService,LikesDTO input)
        {
            var result = await likeService.Add(input);
            return result;
        }
        public async Task<LikesDTO> UpdateLike([Service] ILikeService likeService,string likeId ,LikesDTO input)
        {
            input.Id = likeId;
            var result = await likeService.Update(likeId, input);
            return result;
        }

        public async Task<ResultBool> DeleteLike([Service] ILikeService likeService, string likeId)
        {
            var result = await likeService.DeleteById(likeId);
            return new ResultBool(result, DateTime.Now);
        }
        public async Task<ResultBool> DeleteLike([Service] ILikeService likeService, string userId,string postId)
        {
            var removeResult = await likeService.DeleteByUserIdAndPostId(userId, postId);
            return new ResultBool(removeResult, DateTime.Now);
        }
    }

    public class LikeMutationsExtension : ObjectTypeExtension<LikeMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<LikeMutations> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor.Field(l => l.AddLike(default!, default!))
                .Authorize()
                .Description("Create new Like");


            descriptor.Field(l => l.UpdateLike(default!, default!, default!))
                .Authorize()
                .Description("Update Like By Id");

            descriptor.Field(l => l.DeleteLike(default!, default!))
                .Authorize()
                .Description("Delete Like By Id");

            descriptor.Field(l => l.DeleteLike(default!, default!, default!))
                .Authorize()
                .Description("Delete Like By Id");

        }
    }
}
