using DataCommon.Models.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataService.Repository.IgnoredRepository;

namespace DataService.Repository.Interfaces.Documents
{
    public interface IIgnoredRepository: IRepository<Ignored>
    {
        #region Ignored Tag
        Task<PageWrapper<IgnoredTag>> GetIgnoredTagsByUserIdAndSortAndPageable(string userID, SortDefinition<IgnoredTag> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<bool> IsContainsTagIdInIgnoredTagsByUserId(string userId,string ignoredTagId);
        Task<bool> IsContainsTagNameInIgnoredTagsByUserId(string userId, string name);
        Task<UpdateResult> PushNewTagToIgnoredTagsByUserId(string userId, IgnoredTag ignoredTag);
        Task<bool> RemoveTagFromIgnoredTagsByUserId(string userId, IgnoredTag ignoredTag);
        #endregion

        #region Ignored User
        Task<PageWrapper<IgnoredUser>> GetIgnoredUsersByUserIdAndSortAndPageable(string userID, SortDefinition<IgnoredUser> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<bool> IsContainsUserIdInIgnoredUsersByUserId(string userId, string ignoredUserId);
        Task<UpdateResult> PushNewUserToIgnoredUsersByUserId(string userId, IgnoredUser ignoredUser);
        Task<bool> RemoveUserFromIgnoredUsersByUserId(string userId, IgnoredUser ignoredUser);
        #endregion

        #region Ignored Post
        Task<PageWrapper<IgnoredPost>> GetIgnoredPostsByUserIdAndSortAndPageable(string userID, SortDefinition<IgnoredPost> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<bool> IsContainsPostsIdInIgnoredPostsByUserId(string userId, string ignoredPostId);
        Task<UpdateResult> PushNewPostsToIgnoredPostsByUserId(string userId, IgnoredPost ignoredPost);
        Task<bool> RemovePostsFromIgnoredPostsByUserId(string userId, IgnoredPost ignoredPost);
        #endregion
    }
}
