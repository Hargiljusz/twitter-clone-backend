using DataService.DTO;

namespace GraphQL.Tags
{
    public class TagInputType : InputObjectType<TagDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<TagDTO> descriptor)
        {
            descriptor
                .Ignore(t => t.Id)
                .Ignore(t => t.CreatedAt)
                .Ignore(t => t.CreatedByBackendType)
                .Ignore(t => t.UpdatedByBackendType);

            descriptor.Field(t => t.Name).Description("Tag Name");

            descriptor.Description("Input Type for Tag");
        }
    }

}
