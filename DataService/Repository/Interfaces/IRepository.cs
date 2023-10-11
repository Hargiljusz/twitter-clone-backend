using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(string id);
        Task<IEnumerable<T>> GetAll();
        Task<PageWrapper<T>> GetAllPageable(int pageSize = 10, int pageNumber = 0);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> Find(FilterDefinition<T> filter);
        Task<PageWrapper<T>> FindPageable(Expression<Func<T, bool>> filter, SortDefinition<T> sort = null, int pageSize = 10, int pageNumber = 0);
        Task<PageWrapper<T>> FindPageable(FilterDefinition<T> filter, SortDefinition<T> sort = null, int pageSize = 10, int pageNumber = 0);
        Task Add(T document);
        Task AddRange(IEnumerable<T> documents);
        Task<ReplaceOneResult> Update(T document);
        Task<bool> Remove(T document);
        Task<bool> RemoveRange(IEnumerable<T> documents);
        IQueryable<T> GetQueryable();
    }
}
