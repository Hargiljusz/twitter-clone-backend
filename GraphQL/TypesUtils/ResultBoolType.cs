namespace GraphQL.TypesUtils
{
    public record ResultBool(bool StatusResult, DateTime DateTime);

    public class ResultBoolType : ObjectType<ResultBool>
    {
        protected override void Configure(IObjectTypeDescriptor<ResultBool> descriptor)
        {
            descriptor.Description("Represent result: bool");
        }
    }
}
