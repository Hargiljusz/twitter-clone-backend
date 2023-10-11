using DataService.DTO;
using DataService.Utils;

namespace GraphQL.Likes
{
    public class PageWrapperLikeType : ObjectType<PageWrapper<LikesDTO>>
    {
        protected override void Configure(IObjectTypeDescriptor<PageWrapper<LikesDTO>> descriptor)
        {
            descriptor.Description("Conteins paged  post");
        }
    }
}
