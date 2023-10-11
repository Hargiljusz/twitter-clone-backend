using DataCommon.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Surename { get; set; }
        public string AboutMe { get; set; }
        public string Photo { get; set; }
        public string BackgroundPhoto { get; set; }
        public string Nick { get; set; }
        public List<string> RolesDTO { get; set; } = Enumerable.Empty<string>().ToList();
        public BackendType CreatedByBackendType { get; set; }
        public BackendType UpdatedByBackendType { get; set; }

    }
}
