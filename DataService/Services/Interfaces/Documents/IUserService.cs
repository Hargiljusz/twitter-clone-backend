using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.DTO.Utils;
using DataService.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services.Interfaces.Documents
{
    public interface IUserService 
    {
        Task<UserDTO> GetById(string id);
        Task<PageWrapper<UserDTO>> GetAll(int pageSize = 10, int pageNumber = 0);
        Task<bool> BlockUser(string userId, string blockUserId);
        Task<PageWrapper<UserDTO>> GetBlockUserByUserIdPageableSort(string userId, int pageSize = 10, int pageNumber = 0);
        Task<bool> ReportUser(string userId, string reportUserId, string description);
        Task<UserDTO> UpdateDataByUserId(string userId, UpdateUser updateUser);
        Task<IEnumerable<Warning>> GetWarningsByUserId(string userId);
        Task<bool> CreateUser(CreateUser userRegister);
        Task<bool> CreateUser(CreateUser userRegister, Stream mainPhotoStream, string mainPhotoName, Stream backgroundPhotoStream, string backgroundPhotoName);
        Task<UserDTO> RemoveUser(string userId);
        Task<PageWrapper<UserDTO>> SearchPageable(string q, SortDefinition<ApplicationUser> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<bool> BanUserByUserId(string userId, string adminId, string description);
        Task<bool> UnBanUserByUserId(string userId);
        Task<Warning> AddWarning(string adminId, string Description, string userId);
        Task<IEnumerable<Report>> GetReportsByUserId(string userId);
    }
}
