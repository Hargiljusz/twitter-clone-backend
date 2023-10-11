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
    public interface ILikeService : IBaseService<LikesDTO>
    {
        Task<PageWrapper<LikesDTO>> GetAllLikesByUserIdPageableAndSort(string userId, SortDefinition<Likes> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<long> NumberOfLikesForPost(string postId);
        Task<bool> DeleteById(string postId);
        Task<bool> DeleteByUserIdAndPostId(string userId, string postId);
    }
}
