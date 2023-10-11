using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<Likes> _likes;
        private readonly IMongoCollection<SharePost> _sharePost;

        public PostRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _posts = _dbClient.GetPostCollection();
            _likes = _dbClient.GetLikesCollection();
            _sharePost = _dbClient.GetSharePostsCollection();
        }
        public async Task Add(Post document)
        {
            await _posts.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<Post> documents)
        {
            await _posts.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<Post>> Find(Expression<Func<Post, bool>> filter)
        {
            return await _posts.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Post>> Find(FilterDefinition<Post> filter)
        {
            return await _posts.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<Post>> FindPageable(Expression<Func<Post, bool>> filter, SortDefinition<Post> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _posts.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            } 

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Post>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<Post>> FindPageable(FilterDefinition<Post> filter, SortDefinition<Post> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _posts.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Post>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<Post>> GetAll()
        {
            return await _posts.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<Post>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _posts.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Post>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<Post> GetById(string id)
        {
            var filter = Builders<Post>.Filter.Eq(t => t.Id, id);
            var post = await _posts.FindAsync(filter);

            return await post.FirstOrDefaultAsync();
        }

        public IQueryable<Post> GetQueryable()
        {
            return _posts.AsQueryable();
        }

        public async Task<bool> Remove(Post document)
        {
            var filter = Builders<Post>.Filter.Eq(t => t.Id, document.Id);
            var result = await _posts.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemovePostById(string postId)
        {
            var filter = Builders<Post>.Filter.Eq(t => t.Id, postId);
            var result = await _posts.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<Post> documents)
        {
            var filter = Builders<Post>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _posts.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<ReplaceOneResult> Update(Post document)
        {
            var filter = Builders<Post>.Filter.Eq(t => t.Id, document.Id);
            var result = await _posts.ReplaceOneAsync(filter, document);
            return result;
        }

       
    }
}
