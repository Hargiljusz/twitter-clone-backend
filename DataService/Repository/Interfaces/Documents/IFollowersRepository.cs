using DataCommon.Models.Documents;
using DataService.Utils;
using MongoDB.Driver;

namespace DataService.Repository.Interfaces.Documents
{
    public interface IFollowersRepository : IRepository<Follower>
    {
        Task<long> GetNumberOfFollowingsByUserId(string userId);//kogo obserwuje
        Task<long> GetNumberOfFollowersByUserId(string userId);//kto mnie obserwuje
       }
}
