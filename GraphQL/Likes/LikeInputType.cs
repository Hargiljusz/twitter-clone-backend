using DataService.DTO;

namespace GraphQL.Likes
{
    public class LikeInputType : InputObjectType<LikesDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<LikesDTO> descriptor)
        {
            descriptor.Description("Input Type For Like");

            descriptor.Field(l=>l.LikedByUser).Ignore();

            descriptor
               .Ignore(l => l.Id)
               .Ignore(l => l.CreatedAt)
               .Ignore(l => l.CreatedByBackendType)
               .Ignore(l => l.UpdatedByBackendType);

            descriptor.Field(t => t.PostFor).Description("Post For");
            descriptor.Field(t => t.LikedByUserId).Description("Create By UserId");
        }
    }
}
