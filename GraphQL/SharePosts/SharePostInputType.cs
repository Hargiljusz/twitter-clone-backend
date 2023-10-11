using DataService.DTO;

namespace GraphQL.SharePosts
{
    public class SharePostInputType : InputObjectType<SharePostDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<SharePostDTO> descriptor)
        {
            descriptor.Description("Input Type for Share Post");

            descriptor
               .Ignore(sh => sh.Id)
               .Ignore(sh => sh.CreatedAt)
               .Ignore(sh => sh.CreatedByBackendType)
               .Ignore(sh => sh.UpdatedByBackendType)
               .Ignore(sh=>sh.SharedByUser);

            descriptor.Field(t => t.PostFor).Description("Post For");
            descriptor.Field(t => t.SharedByUserId).Description("Shared By UserId");
        }
    }
}
