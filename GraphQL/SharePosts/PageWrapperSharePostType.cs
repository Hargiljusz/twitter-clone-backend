using DataService.DTO;
using DataService.Utils;

namespace GraphQL.SharePosts
{
    public class PageWrapperSharePostType : ObjectType<PageWrapper<SharePostDTO>>
    {
        protected override void Configure(IObjectTypeDescriptor<PageWrapper<SharePostDTO>> descriptor)
        {
            descriptor.Description("Conteins paged share post");
        }
    }
}
