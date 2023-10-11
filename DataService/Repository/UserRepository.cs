using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<ApplicationUser> _users;
        private readonly IMongoCollection<Ignored> _ignored;

        public UserRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _users = _dbClient.GetUsersCollection();
            _ignored = _dbClient.GetIgnoredCollection();
        }

        public async Task Add(ApplicationUser document)
        {
            await _users.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<ApplicationUser> documents)
        {
            await _users.InsertManyAsync(documents);
        }

        public async Task<UpdateResult> BanUserById(string userId, string adminId, string description)
        {
            var objectId = new ObjectId(userId);
            var update = Builders<ApplicationUser>.Update.Set(au=>au.BanInfo,new BanInfo(adminId,DateTime.Now,DateTime.MaxValue,description));
            var filter = Builders<ApplicationUser>.Filter.Eq(au=>au.Id, objectId);
            var result = await _users.UpdateOneAsync(filter, update);
            return result;
        }

        public async Task<IEnumerable<ApplicationUser>> Find(Expression<Func<ApplicationUser, bool>> filter)
        {
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> Find(FilterDefinition<ApplicationUser> filter)
        {
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<ApplicationUser>> FindPageable(Expression<Func<ApplicationUser, bool>> filter, SortDefinition<ApplicationUser> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _users.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<ApplicationUser>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<ApplicationUser>> FindPageable(FilterDefinition<ApplicationUser> filter, SortDefinition<ApplicationUser> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _users.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<ApplicationUser>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAll()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<ApplicationUser>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _users.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<ApplicationUser>(result, pageSize, pageNumber, pageCount);
        }

        

        public async Task<ApplicationUser> GetById(string id)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(t => t.Id.ToString(), id);
            var user = await _users.FindAsync(filter);

            return await user.FirstOrDefaultAsync();
        }

        public IQueryable<ApplicationUser> GetQueryable()
        {
            return _users.AsQueryable();
        }

        public async Task<UpdateResult> PushReport(string userId, Report report)
        {
            var objectId = new ObjectId(userId);
            var filter = Builders<ApplicationUser>.Filter.Eq(i => i.Id, objectId);
            var update = Builders<ApplicationUser>.Update.Push(i => i.Reports, report);

            var result = await _users.UpdateOneAsync(filter, update);
            return result;
        }

        public async Task<UpdateResult> PushWarning(string userId, Warning warning)
        {
            var objectId = new ObjectId(userId);
            var filter = Builders<ApplicationUser>.Filter.Eq(i => i.Id, objectId);
            var update = Builders<ApplicationUser>.Update.Push(i => i.Warnings, warning);
           
            var result = await _users.UpdateOneAsync(filter, update);
            return result;
        }

        public async Task<bool> Remove(ApplicationUser document)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(t => t.Id, document.Id);
            var result = await _users.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<ApplicationUser> documents)
        {
            var filter = Builders<ApplicationUser>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _users.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<UpdateResult> UnBanUserById(string userId)
        {
            var objectId = new ObjectId(userId);
            var update = Builders<ApplicationUser>.Update.Set(au => au.BanInfo, null);
            var filter = Builders<ApplicationUser>.Filter.Eq(au => au.Id, objectId);
            var result = await _users.UpdateOneAsync(filter, update);
            return result;
        }

        public async Task<UpdateResult> Update(ApplicationUser document)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(t => t.Id, document.Id);
            var update = Builders<ApplicationUser>.Update
                .Set(au => au.UserName, document.UserName)
                .Set(au => au.Nick, document.Nick)
                .Set(au => au.PhoneNumber, document.PhoneNumber)
                .Set(au => au.Name, document.Name)
                .Set(au => au.Surename, document.Surename)
                .Set(au => au.AboutMe, document.AboutMe)
                .Set(au=>au.Photo,document.Photo)
                .Set(au => au.BackgroundPhoto, document.BackgroundPhoto);
                var result = await _users.UpdateOneAsync(filter, update);
            return result;
        }

        /// <summary>
        /// Method not suported
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        [Obsolete]
        Task<ReplaceOneResult> IRepository<ApplicationUser>.Update(ApplicationUser document)
        {
            throw new NotSupportedException();
        }
    }
}
