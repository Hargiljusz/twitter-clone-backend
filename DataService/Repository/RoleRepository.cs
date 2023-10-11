using DataCommon;
using DataCommon.Models.Documents;
using DataService.Exceptions;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<ApplicationRole> _roles;
        public RoleRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _roles = _dbClient.GetRolesCollection();
        }

        public async Task Add(ApplicationRole document)
        {
            await _roles.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<ApplicationRole> documents)
        {
            await _roles.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<ApplicationRole>> Find(Expression<Func<ApplicationRole, bool>> filter)
        {
            return await _roles.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ApplicationRole>> Find(FilterDefinition<ApplicationRole> filter)
        {
            return await _roles.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<ApplicationRole>> FindPageable(Expression<Func<ApplicationRole, bool>> filter, SortDefinition<ApplicationRole> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _roles.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<ApplicationRole>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<ApplicationRole>> FindPageable(FilterDefinition<ApplicationRole> filter, SortDefinition<ApplicationRole> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _roles.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<ApplicationRole>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<ApplicationRole>> GetAll()
        {
            return await _roles.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<ApplicationRole>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _roles.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<ApplicationRole>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<ApplicationRole> GetById(string id)
        {
            var filter = Builders<ApplicationRole>.Filter.Eq(t => t.Id.ToString(), id);
            var roleCursor = await _roles.FindAsync(filter);

            var role =  await roleCursor.FirstOrDefaultAsync();
            return role;
        }

        public IQueryable<ApplicationRole> GetQueryable()
        {
            return _roles.AsQueryable();
        }

        public async Task<bool> Remove(ApplicationRole document)
        {
            var filter = Builders<ApplicationRole>.Filter.Eq(t => t.Id, document.Id);
            var result = await _roles.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<ApplicationRole> documents)
        {
            var filter = Builders<ApplicationRole>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _roles.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<ReplaceOneResult> Update(ApplicationRole document)
        {
            var filter = Builders<ApplicationRole>.Filter.Eq(t => t.Id, document.Id);
            var result = await _roles.ReplaceOneAsync(filter, document);
            return result;
        }
    }
}
