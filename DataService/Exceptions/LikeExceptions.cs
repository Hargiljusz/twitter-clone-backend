﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Exceptions
{
    public class LikeNotFoundException : Exception
    {
        public LikeNotFoundException(string message) : base(message)
        {
        }
    }
}
