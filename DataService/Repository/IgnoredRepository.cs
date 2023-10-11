using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Repository
{
    public class IgnoredRepository : IIgnoredRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<Ignored> _ignored;

        public IgnoredRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _ignored = _dbClient.GetIgnoredCollection();
        }

        public async Task Add(Ignored document)
        {
            await _ignored.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<Ignored> documents)
        {
            await _ignored.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<Ignored>> Find(Expression<Func<Ignored, bool>> filter)
        {
            return await _ignored.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Ignored>> Find(FilterDefinition<Ignored> filter)
        {
            return await _ignored.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<Ignored>> FindPageable(Expression<Func<Ignored, bool>> filter, SortDefinition<Ignored> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _ignored.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Ignored>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<Ignored>> FindPageable(FilterDefinition<Ignored> filter, SortDefinition<Ignored> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _ignored.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Ignored>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<Ignored>> GetAll()
        {
            return await _ignored.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<Ignored>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _ignored.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Ignored>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<Ignored> GetById(string id)
        {
            return await _ignored.Find(i => i.Id == id).FirstOrDefaultAsync();
        }
       
        public async Task<PageWrapper<IgnoredPost>> GetIgnoredPostsByUserIdAndSortAndPageable(string userID, SortDefinition<IgnoredPost> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var projection = Builders<Ignored>.Projection.Exclude(x => x.Id).Include(i=>i.IgnoredPosts);

            #region count facet
            var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<IgnoredPost, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<IgnoredPost>()
            }));
            #endregion

            #region data pagination facet
            AggregateFacet<IgnoredPost, IgnoredPost> dataFacet = null;
           if(sort is not null) {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<IgnoredPost, IgnoredPost>.Create(new[]
                        {   PipelineStageDefinitionBuilder.Sort(sort),
                            PipelineStageDefinitionBuilder.Skip<IgnoredPost>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<IgnoredPost>(pageSize),
                        }
                    )
                );
            }
            else
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<IgnoredPost, IgnoredPost>.Create(new[]
                        {   
                            PipelineStageDefinitionBuilder.Skip<IgnoredPost>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<IgnoredPost>(pageSize),
                        }
                    )
                );
            }
            #endregion

            var aggreagation = _ignored.Aggregate()
                  .Match(i => i.UserId == userID)
                  .Project(projection)
                  .Unwind("IgnoredPosts")
                  .ReplaceRoot<IgnoredPost>("$IgnoredPosts")
                  .Facet(countFacet, dataFacet);
            Console.WriteLine(aggreagation.ToString());
            var result = await aggreagation.ToListAsync();

           var count = result.First()
            .Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()
            ?.FirstOrDefault()
            ?.Count ?? 0;

            var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

            var data = result.First()
                .Facets.First(x => x.Name == "data")
                .Output<IgnoredPost>();

            return new PageWrapper<IgnoredPost>(data, pageSize, pageNumber, totalPages);
        }

        public async Task<PageWrapper<IgnoredTag>> GetIgnoredTagsByUserIdAndSortAndPageable(string userID, SortDefinition<IgnoredTag> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var projection = Builders<Ignored>.Projection.Exclude(x => x.Id).Include(i => i.IgnoredTags);

            #region count facet
            var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<IgnoredTag, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<IgnoredTag>()
            }));
            #endregion

            #region data pagination facet
            AggregateFacet<IgnoredTag, IgnoredTag> dataFacet = null;
            if (sort is not null)
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<IgnoredTag, IgnoredTag>.Create(new[]
                        {   PipelineStageDefinitionBuilder.Sort(sort),
                            PipelineStageDefinitionBuilder.Skip<IgnoredTag>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<IgnoredTag>(pageSize),
                        }
                    )
                );
            }
            else
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<IgnoredTag, IgnoredTag>.Create(new[]
                        {
                            PipelineStageDefinitionBuilder.Skip<IgnoredTag>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<IgnoredTag>(pageSize),
                        }
                    )
                );
            }
            #endregion

            var aggreagation = _ignored.Aggregate()
                  .Match(i => i.UserId == userID)
                  .Project(projection)
                  .Unwind("IgnoredTags")
                  .ReplaceRoot<IgnoredTag>("$IgnoredTags")
                  .Facet(countFacet, dataFacet); 
            Console.WriteLine(aggreagation.ToString());
            var result = await aggreagation.ToListAsync();

            var count = result.First()
             .Facets.First(x => x.Name == "count")
             .Output<AggregateCountResult>()
             ?.FirstOrDefault()
             ?.Count ?? 0;

            var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

            var data = result.First()
                .Facets.First(x => x.Name == "data")
                .Output<IgnoredTag>();

            return new PageWrapper<IgnoredTag>(data, pageSize, pageNumber, totalPages);
        }

        public async Task<PageWrapper<IgnoredUser>> GetIgnoredUsersByUserIdAndSortAndPageable(string userID, SortDefinition<IgnoredUser> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var projection = Builders<Ignored>.Projection.Exclude(x => x.Id).Include(i => i.IgnoredUsers);

            #region count facet
            var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<IgnoredUser, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<IgnoredUser>()
            }));
            #endregion

            #region data pagination facet
            AggregateFacet<IgnoredUser, IgnoredUser> dataFacet = null;
            if (sort is not null)
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<IgnoredUser, IgnoredUser>.Create(new[]
                        {   PipelineStageDefinitionBuilder.Sort(sort),
                            PipelineStageDefinitionBuilder.Skip<IgnoredUser>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<IgnoredUser>(pageSize),
                        }
                    )
                );
            }
            else
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<IgnoredUser, IgnoredUser>.Create(new[]
                        {
                            PipelineStageDefinitionBuilder.Skip<IgnoredUser>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<IgnoredUser>(pageSize),
                        }
                    )
                );
            }
            #endregion

            var aggreagation = _ignored.Aggregate()
                  .Match(i => i.UserId == userID)
                  .Project(projection)
                  .Unwind("IgnoredUsers")
                  .ReplaceRoot<IgnoredUser>("$IgnoredUsers")
                  .Facet(countFacet, dataFacet);
            Console.WriteLine(aggreagation.ToString());
            var result = await aggreagation.ToListAsync();

            var count = result.First()
             .Facets.First(x => x.Name == "count")
             .Output<AggregateCountResult>()
             ?.FirstOrDefault()
             ?.Count ?? 0;

            var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

            var data = result.First()
                .Facets.First(x => x.Name == "data")
                .Output<IgnoredUser>();

            return new PageWrapper<IgnoredUser>(data, pageSize, pageNumber, totalPages);
        }

        public IQueryable<Ignored> GetQueryable()
        {
            return _ignored.AsQueryable();
        }

        public async Task<bool> IsContainsPostsIdInIgnoredPostsByUserId(string userId, string ignoredPostId)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId) & Builders<Ignored>.Filter.ElemMatch(i => i.IgnoredPosts, ip => ip.PostId.Equals(ignoredPostId));
            var result = await _ignored.CountDocumentsAsync(filter);
            return result == 1;
        }

        public async Task<bool> IsContainsTagIdInIgnoredTagsByUserId(string userId, string ignoredTagId)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId) & Builders<Ignored>.Filter.ElemMatch(i => i.IgnoredTags, it => it.TagId.Equals(ignoredTagId));
            var result = await _ignored.CountDocumentsAsync(filter);
            return result == 1;
        }

        public async Task<bool> IsContainsTagNameInIgnoredTagsByUserId(string userId, string name)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId) & Builders<Ignored>.Filter.ElemMatch(i => i.IgnoredTags, it => it.Name.Equals(name));
            var result = await _ignored.CountDocumentsAsync(filter);
            return result == 1;
        }

        public async Task<bool> IsContainsUserIdInIgnoredUsersByUserId(string userId, string ignoredUserId)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId) & Builders<Ignored>.Filter.ElemMatch(i => i.IgnoredUsers,iu=>iu.UserId.Equals(ignoredUserId));
            var result = await _ignored.CountDocumentsAsync(filter);
            return result == 1;
        }

        public async Task<UpdateResult> PushNewPostsToIgnoredPostsByUserId(string userId, IgnoredPost ignoredPost)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId);
            var update = Builders<Ignored>.Update.Push(i => i.IgnoredPosts, ignoredPost);
            return await _ignored.UpdateOneAsync(filter,update);
        }

        public async Task<UpdateResult> PushNewTagToIgnoredTagsByUserId(string userId, IgnoredTag ignoredTag)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId);
            var update = Builders<Ignored>.Update.Push(i => i.IgnoredTags, ignoredTag);
            return await _ignored.UpdateOneAsync(filter, update);
        }

        public async Task<UpdateResult> PushNewUserToIgnoredUsersByUserId(string userId, IgnoredUser ignoredUser)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId);
            var update = Builders<Ignored>.Update.Push(i => i.IgnoredUsers, ignoredUser);
            return await _ignored.UpdateOneAsync(filter, update);
        }

        public async Task<bool> Remove(Ignored document)
        {
            var filter = Builders<Ignored>.Filter.Eq(t => t.Id, document.Id);
            var result = await _ignored.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemovePostsFromIgnoredPostsByUserId(string userId, IgnoredPost ignoredPost)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId);
            var update = Builders<Ignored>.Update.PullFilter(i => i.IgnoredPosts, iP=>iP.PostId.Equals(ignoredPost.PostId));
            var result = await _ignored.UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.MatchedCount > 0 && result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<Ignored> documents)
        {
            var filter = Builders<Ignored>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _ignored.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<bool> RemoveTagFromIgnoredTagsByUserId(string userId, IgnoredTag ignoredTag)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId);
            var update = Builders<Ignored>.Update.PullFilter(i => i.IgnoredTags, iP => iP.TagId.Equals(ignoredTag.TagId));
            var result =await _ignored.UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.MatchedCount > 0 && result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveUserFromIgnoredUsersByUserId(string userId, IgnoredUser ignoredUser)
        {
            var filter = Builders<Ignored>.Filter.Eq(i => i.UserId, userId);
            var update = Builders<Ignored>.Update.PullFilter(i => i.IgnoredUsers, iP => iP.UserId.Equals(ignoredUser.UserId));
            var result = await _ignored.UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.MatchedCount > 0 && result.ModifiedCount > 0;
        }

        public async Task<ReplaceOneResult> Update(Ignored document)
        {
            var filter = Builders<Ignored>.Filter.Eq(t => t.Id, document.Id);
            var result = await _ignored.ReplaceOneAsync(filter, document);
            return result;
        }
    }
}
