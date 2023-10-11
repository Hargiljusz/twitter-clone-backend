using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class LikesRepositiry : ILikesRepositiry
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<Likes> _likes;

        public LikesRepositiry(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _likes = _dbClient.GetLikesCollection();
        }

        public async Task Add(Likes document)
        {
            await _likes.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<Likes> documents)
        {
            await _likes.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<Likes>> Find(Expression<Func<Likes, bool>> filter)
        {
            return await _likes.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Likes>> Find(FilterDefinition<Likes> filter)
        {
            return await _likes.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<Likes>> FindPageable(Expression<Func<Likes, bool>> filter, SortDefinition<Likes> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _likes.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Likes>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<Likes>> FindPageable(FilterDefinition<Likes> filter, SortDefinition<Likes> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _likes.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Likes>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<Likes>> GetAll()
        {
            return await _likes.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<Likes>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _likes.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Likes>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<Likes> GetById(string id)
        {
            var filter = Builders<Likes>.Filter.Eq(t => t.Id, id);
            var sharePost = await _likes.FindAsync(filter);

            return await sharePost.FirstOrDefaultAsync();
        }

        public async Task<long> GetNumberOfLikesForPost(string postId)
        {
            var filter = Builders<Likes>.Filter.Eq(t => t.PostFor, postId);
            var result = await _likes.CountDocumentsAsync(filter);
            return result;
        }

        public IQueryable<Likes> GetQueryable()
        {
            return _likes.AsQueryable();
        }

        public async Task<bool> Remove(Likes document)
        {
            var filter = Builders<Likes>.Filter.Eq(t => t.Id, document.Id);
            var result = await _likes.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveByPost(string postId)
        {
            var filter = Builders<Likes>.Filter.Eq(t => t.Id, postId);
            var result = await _likes.DeleteManyAsync(filter);
            return result.DeletedCount > 0; ;
        }


        public async Task<bool> RemoveRange(IEnumerable<Likes> documents)
        {

            var filter = Builders<Likes>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _likes.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<ReplaceOneResult> Update(Likes document)
        {
            var filter = Builders<Likes>.Filter.Eq(t => t.Id, document.Id);
            var result = await _likes.ReplaceOneAsync(filter, document);
            return result;
        }
    }
}
