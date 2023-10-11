using DataCommon.Models.Documents;
using DataService.Utils;
using MongoDB.Driver;

namespace DataService.Repository.Interfaces.Documents
{
    public interface ITagRepository : IRepository<DataCommon.Models.Documents.Tag>
    {
        
    }
}
