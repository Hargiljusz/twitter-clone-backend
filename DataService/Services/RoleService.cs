using AutoMapper;
using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Repository;
using DataService.Services.Interfaces.Documents;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly RoleRepository _roleRepository;

        public RoleService(RoleManager<ApplicationRole> roleManager, IMapper mapper, RoleRepository roleRepository)
        {
            _roleManager = roleManager;
            _mapper = mapper;
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDTO>> GetAll()
        {
            var roles = await _roleRepository.Find(_ => true);
            var rolesDTO = _mapper.Map<IEnumerable<RoleDTO>>(roles);
            return rolesDTO;
        }

        public async Task<RoleDTO> GetById(string id)
        {
            var role =  await _roleManager.FindByIdAsync(id);

            if (role is null)
            {
                throw new RoleNotFoundException("Role not found");
            }

            var roleDTO = _mapper.Map<RoleDTO>(role);
            return roleDTO;
        }
    }
}
