using DataCommon.Models.Documents;
using DataService.Utils;

namespace DataService.Repository.Interfaces.Documents
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<bool> RemovePostById(string postId);
    }
}
