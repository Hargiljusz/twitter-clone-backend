using GraphQL.Errors.Exceptions;

namespace GraphQL.Errors
{
    public class AuthError : IErrorFilter
    {
        public IError OnError(IError error)
        {
            var ex = error.Exception!;
            if (ex is not null && ex is IAuthException)
            {
                var code = ex is UserNotFoundException ? "404" : "409";
                return ErrorBuilder.New()
                    .SetCode(code)
                    .SetMessage(ex.Message)
                    .SetExtension("TimeStamp", DateTime.Now)
                    .SetExtension("ErrorType", "Auth")
                    .Build();
            }
            return error;
        }
    }
}
