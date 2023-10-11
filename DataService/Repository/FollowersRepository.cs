using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DataService.Repository
{
    public class FollowerRepository : IFollowersRepository
    {
        private readonly IDbClient _dbClient;
        private readonly IMongoCollection<Follower> _follower;

        public FollowerRepository(IDbClient dbClient)
        {
            _dbClient = dbClient;
            _follower = _dbClient.GetFollowersCollection();
        }

        public async Task Add(Follower document)
        {
            await _follower.InsertOneAsync(document);
        }

        public async Task AddRange(IEnumerable<Follower> documents)
        {
            await _follower.InsertManyAsync(documents);
        }

        public async Task<IEnumerable<Follower>> Find(Expression<Func<Follower, bool>> filter)
        {
            return await _follower.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Follower>> Find(FilterDefinition<Follower> filter)
        {
            return await _follower.Find(filter).ToListAsync();
        }

        public async Task<PageWrapper<Follower>> FindPageable(Expression<Func<Follower, bool>> filter, SortDefinition<Follower> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _follower.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Follower>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<PageWrapper<Follower>> FindPageable(FilterDefinition<Follower> filter, SortDefinition<Follower> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _follower.Find(filter);

            if (sort is not null)
            {
                queryDocuments = queryDocuments.Sort(sort);
            }

            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Follower>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<IEnumerable<Follower>> GetAll()
        {
            return await _follower.Find(_ => true).ToListAsync();
        }

        public async Task<PageWrapper<Follower>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var queryDocuments = _follower.Find(_ => true);
            var totalItemCount = await queryDocuments.CountDocumentsAsync();
            var pageCount = Convert.ToInt32(Math.Ceiling(totalItemCount / Convert.ToDouble(pageSize)));

            var result = await queryDocuments.Skip(pageNumber * pageSize).Limit(pageSize).ToListAsync();
            return new PageWrapper<Follower>(result, pageSize, pageNumber, pageCount);
        }

        public async Task<Follower> GetById(string id)
        {
            var filter = Builders<Follower>.Filter.Eq(t => t.Id, id);
            var sharePost = await _follower.FindAsync(filter);

            return await sharePost.FirstOrDefaultAsync();
        }

        public async Task<PageWrapper<ApplicationUser>> GetListOfFollowerUsersPeageableAndSort(string userId, SortDefinition<ApplicationUser> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            #region count facet
            var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<ApplicationUser, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<ApplicationUser>()
            }));
            #endregion

            #region data facet
            AggregateFacet<ApplicationUser, ApplicationUser> dataFacet = null;
            if (sort is not null)
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<ApplicationUser, ApplicationUser>.Create(new[]
                        {   PipelineStageDefinitionBuilder.Sort(sort),
                            PipelineStageDefinitionBuilder.Skip<ApplicationUser>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<ApplicationUser>(pageSize),
                        }
                    )
                );
            }
            else
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<ApplicationUser, ApplicationUser>.Create(new[]
                        {
                            PipelineStageDefinitionBuilder.Skip<ApplicationUser>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<ApplicationUser>(pageSize),
                        }
                    )
                );
            }
            #endregion

            var aggreagation = _follower.Aggregate()
                .Match(f => f.To == userId)
                .Lookup<Follower, ApplicationUser, ApplicationUser>(_dbClient.GetUsersCollection(), f => f.From, user => user.Id, user => user)
                .Facet(countFacet, dataFacet);

            var aggreagationResult = await aggreagation.ToListAsync();

            var count = aggreagationResult.First()
              .Facets.First(x => x.Name == "count")
              .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

            var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

            var data = aggreagationResult.First()
                .Facets.First(x => x.Name == "data")
                .Output<ApplicationUser>();

            var users = data.ToList();

            return new PageWrapper<ApplicationUser>(users,pageSize,pageNumber,totalPages);
        }

        public async Task<PageWrapper<ApplicationUser>> GetListOfFollowingUsersPeageableAndSort(string userId, SortDefinition<ApplicationUser> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            #region count facet
            var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<ApplicationUser, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<ApplicationUser>()
            }));
            #endregion

            #region data facet
            AggregateFacet<ApplicationUser, ApplicationUser> dataFacet = null;
            if (sort is not null)
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<ApplicationUser, ApplicationUser>.Create(new[]
                        {   PipelineStageDefinitionBuilder.Sort(sort),
                            PipelineStageDefinitionBuilder.Skip<ApplicationUser>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<ApplicationUser>(pageSize),
                        }
                    )
                );
            }
            else
            {
                dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<ApplicationUser, ApplicationUser>.Create(new[]
                        {
                            PipelineStageDefinitionBuilder.Skip<ApplicationUser>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<ApplicationUser>(pageSize),
                        }
                    )
                );
            }
            #endregion

            var aggreagation = _follower.Aggregate()
                .Match(f => f.From == userId)
                .Lookup<Follower, ApplicationUser, ApplicationUser>(_dbClient.GetUsersCollection(), f => f.To, user => user.Id, user => user)
                .Facet(countFacet, dataFacet);

            var aggreagationResult = await aggreagation.ToListAsync();

            var count = aggreagationResult.First()
              .Facets.First(x => x.Name == "count")
              .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

            var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

            var data = aggreagationResult.First()
                .Facets.First(x => x.Name == "data")
                .Output<ApplicationUser>();

            var users = data.ToList();

            return new PageWrapper<ApplicationUser>(users, pageSize, pageNumber, totalPages);
        }

        public async Task<long> GetNumberOfFollowersByUserId(string userId)
        {
            return await _follower.Find(f => f.To == userId).CountDocumentsAsync();
        }

        public async Task<long> GetNumberOfFollowingsByUserId(string userId)
        {
            return await _follower.Find(f => f.From == userId).CountDocumentsAsync();
        }

        public IQueryable<Follower> GetQueryable()
        {
            return _follower.AsQueryable();
        }

        public async Task<bool> Remove(Follower document)
        {
            var filter = Builders<Follower>.Filter.Eq(t => t.Id, document.Id);
            var result = await _follower.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> RemoveRange(IEnumerable<Follower> documents)
        {
            if(documents == null && documents.Count() == 0)
            {
                return false;
            }
            var filter = Builders<Follower>.Filter.In(t => t.Id, documents.Select(t => t.Id));
            var result = await _follower.DeleteManyAsync(filter);

            return result.DeletedCount == documents.Count();
        }

        public async Task<ReplaceOneResult> Update(Follower document)
        {
            var filter = Builders<Follower>.Filter.Eq(t => t.Id, document.Id);
            var result = await _follower.ReplaceOneAsync(filter, document);
            return result;
        }
    }
}
