using DataCommon.Models.Documents;

namespace DataService.Repository.Interfaces.Documents
{
    public interface ISharePostRepository : IRepository<SharePost>
    {
        Task<long> GetNumberOfSharedPosForPost(string postID);
        Task<bool> RemoveSharedPostById(string sharePostId);
    }
}
