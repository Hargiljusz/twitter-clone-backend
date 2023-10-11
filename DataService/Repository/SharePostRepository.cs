using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class SharePostRepository : ISharePostRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<SharePost> _sharePosts;

        public SharePostRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _sharePosts = _dbClient.GetSharePostsCollection();
        }
        public async Task Add(SharePost document)
        {
            await _sharePosts.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<SharePost> documents)
        {
            await _sharePosts.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<SharePost>> Find(Expression<Func<SharePost, bool>> filter)
        {
            return await _sharePosts.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<SharePost>> Find(FilterDefinition<SharePost> filter)
        {
            return await _sharePosts.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<SharePost>> FindPageable(Expression<Func<SharePost, bool>> filter, SortDefinition<SharePost> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _sharePosts.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<SharePost>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<SharePost>> FindPageable(FilterDefinition<SharePost> filter, SortDefinition<SharePost> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _sharePosts.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<SharePost>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<SharePost>> GetAll()
        {
            return await _sharePosts.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<SharePost>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _sharePosts.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<SharePost>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<SharePost> GetById(string id)
        {
            var filter = Builders<SharePost>.Filter.Eq(t => t.Id, id);
            var sharePost = await _sharePosts.FindAsync(filter);

            return await sharePost.FirstOrDefaultAsync();
        }

        public async Task<long> GetNumberOfSharedPosForPost(string postID)
        {
            var filter = Builders<SharePost>.Filter.Eq(sh => sh.PostFor, postID);
            var count = await _sharePosts.CountDocumentsAsync(filter);
            return count;
        }

        public IQueryable<SharePost> GetQueryable()
        {
            return _sharePosts.AsQueryable();
        }

        public async Task<bool> Remove(SharePost document)
        {
            var filter = Builders<SharePost>.Filter.Eq(t => t.Id, document.Id);
            var result = await _sharePosts.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<SharePost> documents)
        {
            var filter = Builders<SharePost>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _sharePosts.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<bool> RemoveSharedPostById(string sharePostId)
        {
            var filter = Builders<SharePost>.Filter.Eq(sh => sh.Id, sharePostId);
            var result = await _sharePosts.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<ReplaceOneResult> Update(SharePost document)
        {
            var filter = Builders<SharePost>.Filter.Eq(t => t.Id, document.Id);
            var result = await _sharePosts.ReplaceOneAsync(filter, document);
            return result;
        }
    }
}
