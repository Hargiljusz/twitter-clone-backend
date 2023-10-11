using DataCommon.Models.Utils;
using DataService.DTO;

namespace GraphQL.Tags
{
    public class TagType : ObjectType<TagDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<TagDTO> descriptor)
        {
            descriptor.Description("This type represent Tag");
            descriptor.Field(t => t.CreatedByBackendType).Authorize(new[] {UserRoles.Admin} ).Description("Only For Admin");
            descriptor.Field(t => t.UpdatedByBackendType).Authorize(new[] { UserRoles.Admin}).Description("Only For Admin");
        }
    }
}
