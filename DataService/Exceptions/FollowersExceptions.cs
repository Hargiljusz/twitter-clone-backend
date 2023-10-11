using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Exceptions
{
    public class FollowerNotFoundExceptions : Exception
    {
        public FollowerNotFoundExceptions(string message) : base(message)
        {
        }
    }

    public class FollowerTransactionException : Exception
    {
        public FollowerTransactionException(string message) : base(message)
        {
        }
    }
}
