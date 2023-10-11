using DataCommon.Models.Utils;
using DataService.DTO;

namespace GraphQL.Users
{
    public class UserType : ObjectType<UserDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<UserDTO> descriptor)
        {
            descriptor.Description("This type represent User");
            descriptor.Field(u => u.RolesDTO).Name("roles");
            descriptor.Field(x => x.CreatedByBackendType).Authorize(new[] { UserRoles.Admin });
            descriptor.Field(x => x.UpdatedByBackendType).Authorize(new[] { UserRoles.Admin });
            descriptor.Field("sayHello")
                //.Authorize(new[] { UserRoles.Admin })
                .Resolve(ctx =>
                {
                    var user = ctx.Parent<UserDTO>();
                    var q = new UserQueries();

                    return q.Greeting(user.UserName);
                });
        }
    }
}
