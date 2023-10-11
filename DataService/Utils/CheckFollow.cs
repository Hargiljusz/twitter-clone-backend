using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Utils
{
    public class CheckFollow
    {
        public string UserId { get; set; }
        public string CheckUserId { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFollower { get; set; }

    }
}
