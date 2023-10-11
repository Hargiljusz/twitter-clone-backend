using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO.Utils
{
    public class UpdateUser
    {
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Surename { get; set; }
        public string AboutMe { get; set; }
        public string Photo { get; set; }
        public string BackgroundPhoto { get; set; }
        public string Nick { get; set; }
    }
}
