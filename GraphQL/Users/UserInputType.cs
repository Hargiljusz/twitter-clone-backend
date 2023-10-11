using DataService.DTO;

namespace GraphQL.Users
{
    public class UserInputType : InputObjectType<UserDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UserDTO> descriptor)
        {
            descriptor.Description("Input Type for User");

            descriptor
                .Ignore(t => t.Id)
                //.Ignore(t => t.CreatedAt)
                .Ignore(t => t.CreatedByBackendType)
                .Ignore(t => t.UpdatedByBackendType);

            descriptor.Field(u => u.RolesDTO).Name("Roles");
        }
    }
}
