using DataCommon.Models.Documents;
using MongoDB.Driver;

namespace DataCommon
{
    public interface IDbClient
    {
        public const string TagCollectionName = "Tags";
        public const string SharePostsCollectionName = "SharePosts";
        public const string PostsCollectionName = "Posts";
        public const string LikesCollectionName = "Likes";
        public const string FollowerCollectionName = "Follower";
        public const string RolesCollectionName = "Roles";
        public const string UsersCollectionName = "Users";
        public const string IgnoredCollectionName = "Ignored";
        public static List<string> CollectionNames => new List<string>
        {
            TagCollectionName, SharePostsCollectionName, PostsCollectionName, LikesCollectionName,FollowerCollectionName, RolesCollectionName, UsersCollectionName,IgnoredCollectionName
        };
        IMongoCollection<Models.Documents.Tag> GetTagsCollection();
        IMongoCollection<SharePost> GetSharePostsCollection();
        IMongoCollection<Post> GetPostCollection();
        IMongoCollection<Likes> GetLikesCollection();
        IMongoCollection<Follower> GetFollowersCollection();
        IMongoCollection<ApplicationUser> GetUsersCollection();
        IMongoCollection<ApplicationRole> GetRolesCollection();
        IMongoCollection<Ignored> GetIgnoredCollection();
        IMongoDatabase GetDatabase();
        Task DropAllCollections();


    }
}
