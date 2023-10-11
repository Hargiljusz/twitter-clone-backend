using DataService.DTO;
using DataService.Utils;

namespace GraphQL.Users
{
    public class PageWrapperUserType : ObjectType<PageWrapper<UserDTO>>
    {
        protected override void Configure(IObjectTypeDescriptor<PageWrapper<UserDTO>> descriptor)
        {
            descriptor.Description("Conteins paged users");
        }
    }
}
