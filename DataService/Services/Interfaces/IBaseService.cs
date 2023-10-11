using DataService.Utils;

namespace DataService.Services.Interfaces
{
    public interface IBaseService<T>
    {
        Task<T> GetById(string id);
        Task<IEnumerable<T>> GetAll();
        Task<PageWrapper<T>> GetAllPageable(int pageSize = 10, int pageNumber = 0);
        Task<T> Add(T document);
        Task<IEnumerable<T>> AddRange(IEnumerable<T> documents);
        Task<T> Update(string id,T document);
        Task<bool> Remove(T document);
        Task<bool> RemoveRange(IEnumerable<T> documents);
    }
}
