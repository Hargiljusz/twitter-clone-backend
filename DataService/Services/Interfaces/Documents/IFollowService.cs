using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services.Interfaces.Documents
{
    public interface IFollowService :IBaseService<FollowerDTO>
    {
        /// <summary>
        /// Kogo obserwuje dane userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<long> GetFollowingByUserId(string userId);

        /// <summary>
        /// Kto obserwuje dane userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<long> GetFollowersByUserId(string userId);
        Task<FollowerDTO> FollowUser(string myUserId, string followUserId);
        Task<FollowerDTO> UnFollowUser(string myUserId, string followUserId);
        Task<PageWrapper<UserDTO>> GetListOfFollowingUsersPeageableAndSort(string userId, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<UserDTO>> GetListOfFollowerUsersPeageableAndSort(string userId, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<UserDTO>> GetListOfPropositionUsersPeageableAndSort(string userId, int pageSize = 10, int pageNumber = 0);
        Task<CheckFollow> CheckFollow(string myUserId, string fcheckUserId);
    }
}
