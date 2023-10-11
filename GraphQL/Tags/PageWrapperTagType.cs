using DataService.DTO;
using DataService.Utils;

namespace GraphQL.Tags
{
    public class PageWrapperTagType : ObjectType<PageWrapper<TagDTO>>
    {
        protected override void Configure(IObjectTypeDescriptor<PageWrapper<TagDTO>> descriptor)
        {
            descriptor.Description("Conteins paged tags");
        }
    }
}
