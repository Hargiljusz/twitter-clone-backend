using DataService.DTO;
using DataService.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services.Interfaces.Documents
{
    public interface IPostService: IBaseService<PostDTO>
    {
        Task<PostDTO> Add(PostDTO document, string userId, List<PostFileWrapper> postFiles = null);
        /// <summary>
        /// Blokuje wszystkie posty użytkownika, co równa się z dodaniem użytkownika do black listy
        /// </summary>
        /// <param name="userId">Id uzytkownika</param>
        /// <returns>bool - true jeśli udalo sie dodac, false nie</returns>
        Task<bool> IgnoreAllPosts(string userId,string ignoreUserId);
        Task<bool> IgnorePostByPostId(string userId,string postId);
        Task<PageWrapper<PostDTO>> GetAllPostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<PostDTO>> GetAllSubpostForPostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<PostDTO>> GetLikedPostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0);

        Task<PageWrapper<PostDTO>> GetSharePostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<PostDTO>> GetNewestPostByTag(string tag, int pageSize = 10, int pageNumber = 0);

        /// <summary>
        /// Zwraca wszystkie popularne posty z ostatniego tygodnia
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task<PageWrapper<PostDTO>> GetPopularPostByTag(string tag, int pageSize = 10, int pageNumber = 0);
        Task<bool> RepostPost(string userId, string postId);
        Task<bool> DeleteByPostId(string postId);
        Task<PageWrapper<PostDTO>> Feed(string userId, int pageNumber, int pageSize);
        Task<PageWrapper<PostDTO>> GetSubpostsForPostByIdSortNewestAndPageable(string postId, int pageSize = 10, int pageNumber = 0);

        Task<PostDTO> GetByIdWhenRequestAuthenticated(string postId, string userEmail);

    }
}
