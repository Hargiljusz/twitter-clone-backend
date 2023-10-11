namespace GraphQL.Errors.Exceptions
{
    public interface IAuthException
    {

    }

    public class UserNotFoundException : Exception, IAuthException
    {
        public UserNotFoundException(string? message) : base(message)
        {
        }
    }
    public class PasswordsInccoretException : Exception, IAuthException
    {
        public PasswordsInccoretException(string? message) : base(message)
        {
        }
    }
    public class EmailNotConfirmedException : Exception, IAuthException
    {
        public EmailNotConfirmedException(string? message) : base(message)
        {
        }
    }
    public class UserExistException : Exception, IAuthException
    {
        public UserExistException(string? message) : base(message)
        {
        }
    }
    public class RegisterErrorException : Exception, IAuthException
    {
        public RegisterErrorException(string? message) : base(message)
        {
        }
    }

}
