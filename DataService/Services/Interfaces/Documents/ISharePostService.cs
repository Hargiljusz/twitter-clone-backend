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
    public interface ISharePostService : IBaseService<SharePostDTO>
    {
        Task<PageWrapper<SharePostDTO>> GetAllSharedPostByUserIdPageableAndSort(string userId,SortDefinition<SharePost> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<long> NumberOfSharedPosForPost(string postId);
        Task<bool> RemoveSharedPostById(string sharePostId);
        Task<bool> RemoveSharedPostByUserIdAndPostId(string userId, string postId);
    }
}
