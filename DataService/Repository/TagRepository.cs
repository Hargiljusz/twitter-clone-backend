using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<DataCommon.Models.Documents.Tag> _tags;
        private readonly IMongoCollection<Ignored> _ignored;

        public TagRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _tags = _dbClient.GetTagsCollection();
            _ignored = _dbClient.GetIgnoredCollection();
        }

        public async Task Add(DataCommon.Models.Documents.Tag document)
        {
            await _tags.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<DataCommon.Models.Documents.Tag> documents)
        {
            await _tags.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<DataCommon.Models.Documents.Tag>> Find(Expression<Func<DataCommon.Models.Documents.Tag, bool>> filter)
        {
            return await _tags.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<DataCommon.Models.Documents.Tag>> Find(FilterDefinition<DataCommon.Models.Documents.Tag> filter)
        {
            return await _tags.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<DataCommon.Models.Documents.Tag>> FindPageable(Expression<Func<DataCommon.Models.Documents.Tag, bool>> filter, SortDefinition<DataCommon.Models.Documents.Tag> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments =  _tags.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<DataCommon.Models.Documents.Tag>(result,pageSize,pageNumber,pageCount);
        }

        public async Task<PageWrapper<DataCommon.Models.Documents.Tag>> FindPageable(FilterDefinition<DataCommon.Models.Documents.Tag> filter, SortDefinition<DataCommon.Models.Documents.Tag> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _tags.Find(filter);

            if(sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<DataCommon.Models.Documents.Tag>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<DataCommon.Models.Documents.Tag>> GetAll()
        {
            return await _tags.Find(_ => true).ToListAsync();
        }


        public async Task<PageWrapper<DataCommon.Models.Documents.Tag>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _tags.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<DataCommon.Models.Documents.Tag>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<DataCommon.Models.Documents.Tag> GetById(string id)
        {
            var filter = Builders<DataCommon.Models.Documents.Tag>.Filter.Eq(t => t.Id, id);
            var tag = await _tags.FindAsync(filter);

            return await tag.FirstOrDefaultAsync();

        }

        public IQueryable<DataCommon.Models.Documents.Tag> GetQueryable()
        {
            return _tags.AsQueryable();
        }

        public async Task<bool> Remove(DataCommon.Models.Documents.Tag document)
        {
            var filter = Builders<DataCommon.Models.Documents.Tag>.Filter.Eq(t => t.Id, document.Id);
            var result = await _tags.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<DataCommon.Models.Documents.Tag> documents)
        {
            var filter = Builders<DataCommon.Models.Documents.Tag>.Filter.In(t => t.Id, documents.Select(t=>t.Id));
            var result = await _tags.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<ReplaceOneResult> Update(DataCommon.Models.Documents.Tag document)
        {
            var filter = Builders<DataCommon.Models.Documents.Tag>.Filter.Eq(t => t.Id, document.Id);
            var result = await _tags.ReplaceOneAsync(filter,document);
            return result;
        }
    }
    public record TagList(List<DataCommon.Models.Documents.Tag> Tags);
}
