using DataCommon.Models.Documents;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DataCommon
{
    public partial class MongoDbClient : IDbClient
    {
        private readonly IMongoCollection<Follower> _followers;
        private readonly IMongoCollection<Likes> _likes;
        private readonly IMongoCollection<Post> _post;
        private readonly IMongoCollection<SharePost> _sharePosts;
        private readonly IMongoCollection<Models.Documents.Tag> _tags;
        private readonly IMongoCollection<ApplicationUser> _users;
        private readonly IMongoCollection<ApplicationRole> _roles;
        private readonly IMongoCollection<Ignored> _ignored;
        private readonly IMongoDatabase _database;

        public MongoDbClient(IConfiguration configuration)
        {
            var mongoConfiguration = configuration.GetSection("MongoDB").Get<MongoDatabaseSettings>();
            MongoClientSettings mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConfiguration.ConnectionURI);
            mongoClientSettings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

            mongoClientSettings.WriteConcern = WriteConcern.WMajority;
            mongoClientSettings.ReadPreference = ReadPreference.PrimaryPreferred;

            var mongoClient = new MongoClient(mongoClientSettings);
            var database = mongoClient.GetDatabase(mongoConfiguration.DatabaseName);

            _database = database;

            _followers = database.GetCollection<Follower>(IDbClient.FollowerCollectionName);
            _likes = database.GetCollection<Likes>(IDbClient.LikesCollectionName);
            _post = database.GetCollection<Post>(IDbClient.PostsCollectionName);
            _sharePosts = database.GetCollection<SharePost>(IDbClient.SharePostsCollectionName);
            _tags = database.GetCollection<Models.Documents.Tag>(IDbClient.TagCollectionName);
            _users = database.GetCollection<ApplicationUser>(IDbClient.UsersCollectionName);
            _roles = database.GetCollection<ApplicationRole>(IDbClient.RolesCollectionName);
            _ignored = database.GetCollection<Ignored>(IDbClient.IgnoredCollectionName);

            if (mongoConfiguration.CreateIndexes) { CreateIndexes(); }
        }

        public async Task DropAllCollections()
        {
            await _database.DropCollectionAsync(IDbClient.FollowerCollectionName);
            await _database.DropCollectionAsync(IDbClient.LikesCollectionName);
            await _database.DropCollectionAsync(IDbClient.PostsCollectionName);
            await _database.DropCollectionAsync(IDbClient.SharePostsCollectionName);
            await _database.DropCollectionAsync(IDbClient.TagCollectionName);
            await _database.DropCollectionAsync("Users");
            await _database.DropCollectionAsync("Roles");
        }

        public IMongoDatabase GetDatabase() => _database;
        public IMongoCollection<Follower> GetFollowersCollection() => _followers;
        public IMongoCollection<Likes> GetLikesCollection() => _likes;
        public IMongoCollection<Post> GetPostCollection() => _post;
        public IMongoCollection<ApplicationRole> GetRolesCollection() => _roles;
        public IMongoCollection<SharePost> GetSharePostsCollection() => _sharePosts;
        public IMongoCollection<Models.Documents.Tag> GetTagsCollection() => _tags;
        public IMongoCollection<ApplicationUser> GetUsersCollection() => _users;
        public IMongoCollection<Ignored> GetIgnoredCollection() => _ignored;

    }
}
