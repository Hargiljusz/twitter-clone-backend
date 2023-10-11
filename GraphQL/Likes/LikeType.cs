using DataCommon.Models.Utils;
using DataService.DTO;

namespace GraphQL.Likes
{
    public class LikeType : ObjectType<LikesDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<LikesDTO> descriptor)
        {
            descriptor.Description("Like Type");
            descriptor.Field(l => l.LikedByUser).Ignore();
            descriptor.Field(l => l.CreatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
            descriptor.Field(l => l.UpdatedByBackendType).Authorize(new[] { UserRoles.Admin }).Description("Only For Admin");
        }
    }
}
