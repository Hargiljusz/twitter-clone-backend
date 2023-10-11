using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services.Interfaces.Documents
{
    public interface IRoleService
    {
        Task<RoleDTO> GetById(string id);
        Task<IEnumerable<RoleDTO>> GetAll();
    }
}
