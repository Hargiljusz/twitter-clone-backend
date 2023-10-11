using DataCommon.Models.Documents;

namespace DataService.Repository.Interfaces.Documents
{
    public interface ILikesRepositiry : IRepository<Likes>
    {
        Task<bool> RemoveByPost(string postId);
        Task<long> GetNumberOfLikesForPost(string postId);
    }
}
