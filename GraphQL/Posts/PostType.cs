using DataCommon.Models.Utils;
using DataService.DTO;
using GraphQL.Users;

namespace GraphQL.Posts
{
    public class PostType : ObjectType<PostDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<PostDTO> descriptor)
        {
            descriptor.Description("Post Type");
            descriptor.Field(p => p.CreateByUser).Type<UserType>();
            descriptor.Field(p => p.CreatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
            descriptor.Field(p => p.UpdatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
        }
    }
}
