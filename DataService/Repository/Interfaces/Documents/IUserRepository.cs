using DataCommon.Models.Documents;
using DataService.Utils;
using MongoDB.Driver;

namespace DataService.Repository.Interfaces.Documents
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<UpdateResult> BanUserById(string userId,string adminId,string description);
        Task<UpdateResult> UnBanUserById(string userId);
        new Task<UpdateResult> Update(ApplicationUser document);
        Task<UpdateResult> PushWarning(string userId,Warning warning);
        Task<UpdateResult> PushReport(string userId, Report report);
    }
}
