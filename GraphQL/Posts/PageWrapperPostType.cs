using DataService.DTO;
using DataService.Utils;

namespace GraphQL.Posts
{
    public class PageWrapperPostType : ObjectType<PageWrapper<PostDTO>>
    {
        protected override void Configure(IObjectTypeDescriptor<PageWrapper<PostDTO>> descriptor)
        {
            descriptor.Description("Conteins paged post");
        }
    }
}
