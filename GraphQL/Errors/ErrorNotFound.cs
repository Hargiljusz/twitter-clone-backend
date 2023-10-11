using DataService.Exceptions;

namespace GraphQL.Errors
{
    public class ErrorNotFound : IErrorFilter
    {
        private static List<Type> NotFoundExceptions = new List<Type>()
        {
            typeof(TagNotFoundException),
            typeof(SharePostNotFoundException),
            typeof(PostNotFoundException),
            typeof(LikeNotFoundException),
            typeof(UserNotFoundException)
        };
        public IError OnError(IError error)
        {
            var ex = error.Exception!;
            if (ex is not null && NotFoundExceptions.Any(ef=>ef == ex.GetType())){
                return ErrorBuilder.New()
                    .SetCode("404")
                    .SetMessage(ex.Message)
                    .SetExtension("TimeStamp", DateTime.Now)
                    .SetExtension ("ErrorType","NotFound")
                    .Build();
            }
            return error;
        }
    }
}
