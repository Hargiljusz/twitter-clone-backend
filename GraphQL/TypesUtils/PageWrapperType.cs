using DataService.Utils;

namespace GraphQL.TypesUtils
{
    public class PageWrapperType<TypeContent> : ObjectType<PageWrapper<TypeContent>> where TypeContent : class, IOutputType
    {
        protected override void Configure(IObjectTypeDescriptor<PageWrapper<TypeContent>> descriptor)
        {
            descriptor.Name($"PageWrapper_{typeof(TypeContent).Name}");

            descriptor
                 .Field(pw => pw.Content)
                 .Type<ListType<TypeContent>>();

            descriptor.Field(pw => pw.PageSize);
            descriptor.Field(pw => pw.PageNumber);
            descriptor.Field(pw => pw.TotalPageCount);
        }
    }
}
