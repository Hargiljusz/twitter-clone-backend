
using AspNetCore.Identity.MongoDbCore.Models;
using DataCommon.Models.Utils;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace DataCommon.Models.Documents
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<ObjectId>
    {
        public string Name { get; set; }
        public string Surename { get; set; }
        public string AboutMe { get; set; }
        public string Photo { get; set; }
        public string BackgroundPhoto { get; set; }
        public string Nick { get; set; }
        public BackendType CreatedByBackendType { get; set; }
        public BackendType UpdatedByBackendType { get; set; }
        public IEnumerable<Warning> Warnings { get; set; } = Enumerable.Empty<Warning>();
        public IEnumerable<Report> Reports { get; set; } = Enumerable.Empty<Report>();
        public BanInfo BanInfo{ get; set; }

       
    }
    public record BanInfo(string AdminId, DateTime StartDate,DateTime EndDate, string Description);
    public record Warning(string AdminId, DateTime Date, string Description);
    public record Report(string UserId, DateTime Date, string Description);
}
