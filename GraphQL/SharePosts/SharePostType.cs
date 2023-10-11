using DataCommon.Models.Utils;
using DataService.DTO;

namespace GraphQL.SharePosts
{
    public class SharePostType : ObjectType<SharePostDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<SharePostDTO> descriptor)
        {
            descriptor.Description("Share post Type");
            descriptor.Field(sh => sh.SharedByUser).Ignore();
            descriptor.Field(sh => sh.CreatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
            descriptor.Field(sh => sh.UpdatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
        }
    }
}
