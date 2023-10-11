using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Exceptions
{
    public class TagNotFoundException : Exception
    {
        public TagNotFoundException(string message) : base(message)
        {
        }
      
    }
    public class IgnoredTagNotFoundException : Exception
    {
        public IgnoredTagNotFoundException(string message) : base(message)
        {
        }
    }

    public class TagExistException : Exception
    {
        public TagExistException(string message) : base(message)
        {
        }
    }

    public class TagTransactionException : Exception
    {
        public TagTransactionException(string message) : base(message)
        {
        }
    }
}
