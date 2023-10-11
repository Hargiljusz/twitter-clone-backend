using DataCommon.Models.Utils;
using DataService.DTO;

namespace GraphQL.Follows
{
    public class FollowType : ObjectType<FollowerDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<FollowerDTO> descriptor)
        {
            descriptor.Name("Follow");
            descriptor.Description("This type represent Follow");
            descriptor.Field(f => f.CreatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
            descriptor.Field(f => f.UpdatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
        }
    }
}
