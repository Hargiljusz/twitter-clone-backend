using DataService.DTO;

namespace GraphQL.Posts
{
    public class PostInputType : InputObjectType<PostDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<PostDTO> descriptor)
        {
            descriptor.Description("Input Type for Post");

            descriptor
               .Ignore(sh => sh.Id)
               .Ignore(sh => sh.CreatedAt)
               .Ignore(sh => sh.CreatedByBackendType)
               .Ignore(sh => sh.UpdatedByBackendType)
               .Ignore(sh => sh.CreateByUser);

            descriptor.Field(t => t.PostFor).Description("Post For");
            descriptor.Field(t => t.CreateByUserId).Description("Create By UserId");
        }
    }
}
