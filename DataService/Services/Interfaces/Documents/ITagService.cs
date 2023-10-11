using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Utils;
using MongoDB.Driver;

namespace DataService.Services.Interfaces.Documents
{
    public interface ITagService : IBaseService<TagDTO>
    {
        Task<TagDTO> GetTagByName(string name);
        Task<bool> DeleteTagByName(string name);
        Task<PageWrapper<TagDTO>> SearchPageable(string q, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<TagDTO>> GetAllPageableAndSort(int pageSize = 10, int pageNumber = 0, SortDefinition<DataCommon.Models.Documents.Tag> sort = null);
        //Task<PageWrapper<TagDTO>> GetPopularTagsByUserIdPageableSort(string userId, int pageSize = 10, int pageNumber = 0, SortDefinition<DataCommon.Models.Documents.Tag> sort = null);
        Task<PageWrapper<TagDTO>> GetPopularTagsInThis(TimeDuration timeDuration, int pageSize = 10, int pageNumber = 0, SortDefinition<DataCommon.Models.Documents.Tag> sort = null);
        Task<bool> IgnoreTagForUser(string name, string userId);
        Task<bool> IgnoreTagIdForUser(string tagId, string userId);
        Task<bool> UnignoreTagForUser(string name, string userId);
        Task<bool> UnignoreTagIdForUser(string tagId, string userId);
        Task<PageWrapper<TagDTO>> GetAllIgnoredTagsForUserPageableAndSort(string userId, SortDefinition<DataCommon.Models.Documents.Tag> sort = null, int pageSize = 10, int pageNumber = 0);
    }
}
