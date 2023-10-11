using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Exceptions
{
    public class PostNotFoundException : Exception
    {
        public PostNotFoundException(string message) : base(message)
        {
        }
    }
    public class PostTransactionException : Exception
    {
        public PostTransactionException(string message) : base(message)
        {
        }
    }
}
