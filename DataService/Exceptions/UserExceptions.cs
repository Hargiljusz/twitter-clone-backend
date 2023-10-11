using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }

    public class PasswordNotCorrectException : Exception
    {
        public PasswordNotCorrectException(string message) : base(message)
        {
        }
    }
    public class DeleteUserException : Exception
    {
        public DeleteUserException(string message) : base(message)
        {
        }
    }
    public class UserTransactionException : Exception
    {
        public UserTransactionException(string message) : base(message)
        {
        }
    }
}
