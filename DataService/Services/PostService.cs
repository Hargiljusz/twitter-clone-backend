using AutoMapper;
using DataCommon;
using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class PostService : IPostService
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly IIgnoredRepository _ignoredRepository;
        private readonly ISharePostService _sharePostService;
        private readonly IDbClient _dbClient;
        private readonly ITagRepository _tagRepository;
        private readonly ILikesRepositiry _likesRepositiry;
        private readonly ISharePostRepository _sharePostRepository;
        private readonly IFileService _fileService;
        private readonly IUserRepository _userRepository;

        public PostService(IMapper mapper, IPostRepository postRepository, IUserService userService, IIgnoredRepository ignoredRepository, ISharePostService sharePostService, IDbClient dbClient, ITagRepository tagRepository, ISharePostRepository sharePostRepository, ILikesRepositiry likesRepositiry, IFileService fileService, IUserRepository userRepository)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _userService = userService;
            _ignoredRepository = ignoredRepository;
            _sharePostService = sharePostService;
            _dbClient = dbClient;
            _tagRepository = tagRepository;
            _sharePostRepository = sharePostRepository;
            _likesRepositiry = likesRepositiry;
            _fileService = fileService;
            _userRepository = userRepository;
        }

        public async Task<PostDTO> Add(PostDTO document, string userId, List<PostFileWrapper> postFiles = null)
        {
            var post = _mapper.Map<Post>(document);
            post.CreatedAt = DateTime.Now;
            post.Tags = getTags(document.Content);
            post.Multimedia = new PostMultimedia();

            //save files
            if(postFiles!= null && postFiles.Count > 0)
            {
                var filePathsTasks = postFiles.Select(pf =>
                {
                    return _fileService.SaveFile(pf.FileStream, userId, pf.FileName);
                })
                              .ToList();
                await Task.WhenAll(filePathsTasks);
                var filePaths = filePathsTasks.Select(fpt => fpt.Result).ToList();
                post.Multimedia.Files = filePaths;
            }
            await _postRepository.Add(post);
            return _mapper.Map<PostDTO>(post);
        }

        public async Task<IEnumerable<PostDTO>> AddRange(IEnumerable<PostDTO> documents)
        {
            var posts = _mapper.Map<IEnumerable<Post>>(documents);
            posts = posts.Select(p =>
            {
                p.CreatedAt = DateTime.Now;
                p.Tags = getTags(p.Content);
                return p;
            });
            await _postRepository.AddRange(posts);
            return _mapper.Map<IEnumerable<PostDTO>>(posts);
        }

        public async Task<bool> DeleteByPostId(string postId)
        {
            return await _postRepository.RemovePostById(postId);
        }

        public async Task<PageWrapper<PostDTO>> Feed(string userId, int pageNumber, int pageSize)
        {
            var mongoClient = _dbClient.GetDatabase().Client;

            #region facet
            var countFacet = AggregateFacet.Create("count",
               PipelineDefinition<Post, AggregateCountResult>.Create(new[]
               {
                    PipelineStageDefinitionBuilder.Count<Post>()
               }));

            var dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<Post, Post>.Create(new[]
                        {
                            PipelineStageDefinitionBuilder.Skip<Post>(pageNumber * pageSize),
                            PipelineStageDefinitionBuilder.Limit<Post>(pageSize),
                        }
                    )
             );
            #endregion

            using (var session = await mongoClient.StartSessionAsync())
            {
                try
                {
                    session.StartTransaction();
                    var followers = await _dbClient.GetFollowersCollection().Find(f => f.From == userId).Project(f => f.To).ToListAsync();
                    var ignorePostsId = (await _dbClient.GetIgnoredCollection().Find(i => i.UserId == userId).Project(i => i.IgnoredPosts).ToListAsync()).FirstOrDefault().Select(i => i.PostId);
                    var ignoreTags = (await _dbClient.GetIgnoredCollection().Find(i => i.UserId == userId).Project(i => i.IgnoredTags).ToListAsync()).FirstOrDefault().Select(i => i.Name).ToList();

                    followers = followers.Prepend(userId).ToList();


                    var filter1 = Builders<Post>.Filter.In(p => p.CreateByUserId, followers);
                    var filter2 = Builders<Post>.Filter.Nin(p => p.Id, ignorePostsId);

                    //INFO trzeba było utwrzoyć klase wrapper, ponieważ Bson serializer nie radził sobie z samym stringiem jako typ
                    var filter3 = Builders<Post>.Filter.ElemMatch(p => p.Tags, Builders<TagItem>.Filter.In(t => t.TagId, ignoreTags));
                    var filter4 = Builders<Post>.Filter.Where(p => p.PostFor == "");

                    var aggreagation = _dbClient.GetPostCollection().Aggregate()
                                        .Match(filter1 & filter2 & !filter3 & filter4)
                                        .SortByDescending(p => p.CreatedAt)
                                        .Facet(countFacet, dataFacet);

                    var aggreagationResult = await aggreagation.ToListAsync();

                    var count = aggreagationResult.First()
                      .Facets.First(x => x.Name == "count")
                      .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

                    var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

                    var data = aggreagationResult.First()
                        .Facets.First(x => x.Name == "data")
                        .Output<Post>();

                    await session.CommitTransactionAsync();
                    var posts = data.ToList();
                    var postsDTO = _mapper.Map<List<PostDTO>>(posts).ToList();

                    //TODO check is liked or shared
                    postsDTO = await CheckLikesAndSharesByUser(postsDTO, userId);

                    #region userDataForPost
                    var tasks = postsDTO.Select(p => {
                        return new
                        {
                            postId = p.Id,
                            task = getUserById(p.CreateByUserId)
                        };
                    });

                    await Task.WhenAll(tasks.Select(p => p.task));

                    postsDTO.ForEach(p =>
                    {
                        p.CreateByUser = tasks.First(t => t.postId == p.Id).task.Result;
                    });
                    #endregion

                    return new PageWrapper<PostDTO>(postsDTO, pageSize, pageNumber, totalPages);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await session.AbortTransactionAsync();
                    throw;
                }
            }

        }

        public async Task<List<PostDTO>> CheckLikesAndSharesByUser(List<PostDTO> postsDTO, string userId)
        {
            var tasks = postsDTO.Select(p =>
            {
                return new
                {
                    PostId = p.Id,
                    TaskLike = _likesRepositiry.Find(l => l.LikedByUserId == userId && l.PostFor == p.Id),
                    TaskSharePost = _sharePostRepository.Find(sh => sh.SharedByUserId == userId && sh.PostFor == p.Id)
                };
            });



            await Task.WhenAll(Task.WhenAll(tasks.Select(t => t.TaskLike)), Task.WhenAll(tasks.Select(t => t.TaskSharePost)));

            return postsDTO.Select(p =>
                            {
                                var temp = tasks.First(t => t.PostId == p.Id);
                                p.IsLiked = temp.TaskLike.Result.FirstOrDefault() != null;
                                p.IsShared = temp.TaskSharePost.Result.FirstOrDefault() != null;
                                return p;
                            }).
                            ToList();
        }
        public async Task<List<PostDTO>> CheckLikesAndSharesByUserEmail(List<PostDTO> postsDTO, string email)
        {
            var user = (await _userRepository.Find(u => u.Email == email)).FirstOrDefault();
            var userId = user.Id.ToString();
            var tasks = postsDTO.Select(p =>
            {
                return new
                {
                    PostId = p.Id,
                    TaskLike = _likesRepositiry.Find(l => l.LikedByUserId == userId && l.PostFor == p.Id),
                    TaskSharePost = _sharePostRepository.Find(sh => sh.SharedByUserId == userId && sh.PostFor == p.Id)
                };
            });



            await Task.WhenAll(Task.WhenAll(tasks.Select(t => t.TaskLike)), Task.WhenAll(tasks.Select(t => t.TaskSharePost)));

            return postsDTO.Select(p =>
            {
                var temp = tasks.First(t => t.PostId == p.Id);
                p.IsLiked = temp.TaskLike.Result.FirstOrDefault() != null;
                p.IsShared = temp.TaskSharePost.Result.FirstOrDefault() != null;
                return p;
            }).
                            ToList();
        }
        public async Task<IEnumerable<PostDTO>> GetAll()
        {
            var posts = await _postRepository.GetAll();
            var result = _mapper.Map<IEnumerable<PostDTO>>(posts).ToList();
            var uniqueUserID = result.Select(p => p.CreateByUserId).Distinct().ToList();

            var usersDTO = await getUserDTOAsync(uniqueUserID);
            result.ToList()
                 .ForEach(post =>
                 {
                     post.CreateByUser = usersDTO.FirstOrDefault(u => u.userId == post.CreateByUserId).user;
                 });

            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var postsPageable = await _postRepository.GetAllPageable(pageSize, pageNumber);
            var result = _mapper.Map<PageWrapper<PostDTO>>(postsPageable);

            var uniqueUserID = result.Content.Select(p => p.CreateByUserId).Distinct().ToList();

            var usersDTO =  await getUserDTOAsync(uniqueUserID);
            result.Content.ToList()
                 .ForEach(post =>
                 {
                     post.CreateByUser = usersDTO.FirstOrDefault(u => u.userId == post.CreateByUserId).user;
                 });

            return result;
        }

        private async Task<List<(string userId, UserDTO user)>> getUserDTOAsync(List<string> userIDs)
        {
            
            var temp = userIDs.Select(id =>
            {
                return (id, getUserById(id));
            }).ToList();

            await Task.WhenAll(temp.Select(p => p.Item2));

            return temp.Select(p => (p.id, p.Item2.Result)).ToList();
        }

        public async Task<PageWrapper<PostDTO>> GetAllPostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.CreateByUserId, userId);
            var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
            var posts = await _postRepository.FindPageable(filter, sort, pageSize, pageNumber);
            var result = _mapper.Map<IEnumerable<PostDTO>>(posts.Content).ToList();

            result = await CheckLikesAndSharesByUser(result, userId);
            var user = await getUserById(userId);

            result.ForEach(p => p.CreateByUser = user);
            return new PageWrapper<PostDTO>(result, pageSize, pageNumber, posts.TotalPageCount);
        }

        public async Task<PageWrapper<PostDTO>> GetAllSubpostForPostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.CreateByUserId, userId) & Builders<Post>.Filter.Ne(p => p.PostFor, String.Empty);
            var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
            var posts = await _postRepository.FindPageable(filter, sort, pageSize, pageNumber);
            var result = _mapper.Map<IEnumerable<PostDTO>>(posts.Content).ToList();

            result = await CheckLikesAndSharesByUser(result, userId);

            #region userDataForPost
            var tasks = result.Select(p => {
                return new
                {
                    postId = p.Id,
                    task = getUserById(p.CreateByUserId)
                };
            });

            await Task.WhenAll(tasks.Select(p => p.task));

            result.ForEach(p =>
            {
                p.CreateByUser = tasks.First(t => t.postId == p.Id).task.Result;
            });
            #endregion

            return new PageWrapper<PostDTO>(result, pageSize, pageNumber, posts.TotalPageCount);
        }

        public async Task<PostDTO> GetById(string id)
        {
            var post = await _postRepository.GetById(id);
            

            if (post is null)
            {
                throw new PostNotFoundException("Post not found");
            }

            var result = _mapper.Map<PostDTO>(post);

            var user = await getUserById(result.CreateByUserId);

            result.CreateByUser = user;
            

            return result;
        }

        public async Task<PageWrapper<PostDTO>> GetLikedPostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var mongoClient = _dbClient.GetDatabase().Client;

            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {

                    var likes = _dbClient.GetLikesCollection();

                    var sort = Builders<Likes>.Sort.Descending(l => l.CreatedAt);

                    #region facet
                    var countFacet = AggregateFacet.Create("count",
                    PipelineDefinition<Likes, AggregateCountResult>.Create(new[]
                    {
                        PipelineStageDefinitionBuilder.Count<Likes>()
                    }));

                    var dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<Likes, Likes>.Create(new List<IPipelineStageDefinition>
                    {
                      PipelineStageDefinitionBuilder.Skip<Likes>(pageNumber * pageSize),
                      PipelineStageDefinitionBuilder.Limit<Likes>(pageSize)
                    }));

                    #endregion

                    var aggregate = likes.Aggregate()
                      .Match(l => l.LikedByUserId == userId)
                      .Sort(sort)
                      .Facet(countFacet, dataFacet);

                    var aggreagationResult = await aggregate.ToListAsync();

                    var count = aggreagationResult.First()
                      .Facets.First(x => x.Name == "count")
                      .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

                    var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

                    var likedPost = aggreagationResult.First()
                        .Facets.First(x => x.Name == "data")
                        .Output<Likes>()
                        .ToList();

                    var tasks = new List<Task<Post>>();

                    likedPost.ForEach(like => tasks.Add(_postRepository.GetById(like.PostFor)));

                    await Task.WhenAll(tasks);
                    var posts = tasks.Select(task => task.Result).ToList();
                    var postsDTO = _mapper.Map<IEnumerable<PostDTO>>(posts).ToList();

                    await session.CommitTransactionAsync();

                    postsDTO = await CheckLikesAndSharesByUser(postsDTO, userId);

                    #region userDataForPost
                    var userTasks = postsDTO.Select(p => {
                        return new
                        {
                            postId = p.Id,
                            task = getUserById(p.CreateByUserId)
                        };
                    });

                    await Task.WhenAll(userTasks.Select(p => p.task));

                    postsDTO.ForEach(p =>
                    {
                        p.CreateByUser = userTasks.First(t => t.postId == p.Id).task.Result;
                    });
                    #endregion

                    return new PageWrapper<PostDTO>(postsDTO, pageSize, pageNumber, totalPages);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw new PostTransactionException("Post - Transaction error");
                }
            }
        }

        public async Task<PageWrapper<PostDTO>> GetNewestPostByTag(string tag, int pageSize = 10, int pageNumber = 0)
        {
            var filter = Builders<Post>.Filter.ElemMatch(p => p.Tags, tagItem => tagItem.TagId == tag);
            var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
            var postsCollection = _dbClient.GetPostCollection();


            var aggreagate = postsCollection.Aggregate()
                .Match(filter)
                .Sort(sort)
                .Skip(pageSize * pageNumber)
                .Limit(pageSize);

            var newestPostsByTag = await aggreagate.ToListAsync();

            var newestPostsDTOByTag = _mapper.Map<List<PostDTO>>(newestPostsByTag);


            #region userDataForPost
            var userTasks = newestPostsDTOByTag.Select(p => {
                return new
                {
                    postId = p.Id,
                    task = getUserById(p.CreateByUserId)
                };
            });

            await Task.WhenAll(userTasks.Select(p => p.task));

            newestPostsDTOByTag.ForEach(p =>
            {
                p.CreateByUser = userTasks.First(t => t.postId == p.Id).task.Result;
            });
            #endregion

            return new PageWrapper<PostDTO>(newestPostsDTOByTag, pageSize, pageNumber, Int32.MaxValue);
        }

        public async Task<PageWrapper<PostDTO>> GetPopularPostByTag(string tag, int pageSize = 10, int pageNumber = 0)
        {
            var filter = Builders<Post>.Filter.ElemMatch(p => p.Tags, tagItem => tagItem.TagId == tag);
            var sort = Builders<Post>.Sort.Descending(p => p.LikeNumber).Descending(p => p.ShareNumber);
            var postsCollection = _dbClient.GetPostCollection();


            var aggreagate = postsCollection.Aggregate()
                .Match(filter)
                .Sort(sort)
                .Skip(pageSize * pageNumber)
                .Limit(pageSize);

            var newestPostsByTag = await aggreagate.ToListAsync();

            var newestPostsDTOByTag = _mapper.Map<List<PostDTO>>(newestPostsByTag);

            #region userDataForPost
            var userTasks = newestPostsDTOByTag.Select(p => {
                return new
                {
                    postId = p.Id,
                    task = getUserById(p.CreateByUserId)
                };
            });

            await Task.WhenAll(userTasks.Select(p => p.task));

            newestPostsDTOByTag.ForEach(p =>
            {
                p.CreateByUser = userTasks.First(t => t.postId == p.Id).task.Result;
            });
            #endregion

            return new PageWrapper<PostDTO>(newestPostsDTOByTag, pageSize, pageNumber, Int32.MaxValue);
        }

        public async Task<PageWrapper<PostDTO>> GetSharePostByUserIdPageableSortByDate(string userId, int pageSize = 10, int pageNumber = 0)
        { var mongoClient = _dbClient.GetDatabase().Client;

            using (var session = await mongoClient.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var sharePosts = _dbClient.GetSharePostsCollection();

                    var sort = Builders<SharePost>.Sort.Descending(l => l.CreatedAt);
                    var projection = Builders<SharePost>.Projection.Exclude(l => l.Id).Include(l => l.PostFor);

                    #region facet
                    var countFacet = AggregateFacet.Create("count",
                    PipelineDefinition<SharePost, AggregateCountResult>.Create(new[]
                    {
                PipelineStageDefinitionBuilder.Count<SharePost>()
                    }));

                    var dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<SharePost, SharePost>.Create(new List<IPipelineStageDefinition>
                    {
                      PipelineStageDefinitionBuilder.Skip<SharePost>(pageNumber * pageSize),
                      PipelineStageDefinitionBuilder.Limit<SharePost>(pageSize)
                    }));

                    #endregion

                    var aggregate = sharePosts.Aggregate()
                      .Match(l => l.SharedByUserId == userId)
                      .Sort(sort)
                      .Facet(countFacet, dataFacet);

                    var aggreagationResult = await aggregate.ToListAsync();

                    var count = aggreagationResult.First()
                      .Facets.First(x => x.Name == "count")
                      .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

                    var totalPages = Convert.ToInt32(Math.Ceiling(count / (double)pageSize));

                    var sharedPosts = aggreagationResult.First()
                        .Facets.First(x => x.Name == "data")
                        .Output<SharePost>()
                        .ToList();

                    var tasks = new List<Task<Post>>();

                    sharedPosts.ForEach(sharedPost => tasks.Add(_postRepository.GetById(sharedPost.PostFor)));

                    await Task.WhenAll(tasks);
                    var posts = tasks.Select(task => task.Result).ToList();
                    var postsDTO = _mapper.Map<IEnumerable<PostDTO>>(posts).ToList();


                    postsDTO = await CheckLikesAndSharesByUser(postsDTO, userId);

                    #region userDataForPost
                    var userTasks = postsDTO.Select(p => {
                        return new
                        {
                            postId = p.Id,
                            task = getUserById(p.CreateByUserId)
                        };
                    });

                    await Task.WhenAll(userTasks.Select(p => p.task));

                    postsDTO.ForEach(p =>
                    {
                        p.CreateByUser = userTasks.First(t => t.postId == p.Id).task.Result;
                    });
                    #endregion

                    await session.CommitTransactionAsync();

                    return new PageWrapper<PostDTO>(postsDTO, pageSize, pageNumber, totalPages);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw new PostTransactionException("Post - Transaction error");
                }
            }
        }

        public async Task<PageWrapper<PostDTO>> GetSubpostsForPostByIdSortNewestAndPageable(string postId, int pageSize = 10, int pageNumber = 0)
        {
            var builderFilter = Builders<Post>.Filter;
            var filter = builderFilter.Eq(p => p.PostFor, postId);
            var sort = Builders<Post>.Sort.Descending(p => p.CreatedAt);
            var posts = await _postRepository.FindPageable(filter, sort, pageSize, pageNumber);
            var result = _mapper.Map<IEnumerable<PostDTO>>(posts.Content).ToList();

            var userTasks = result.Select(p => {
                return new
                {
                    postId = p.Id,
                    task = getUserById(p.CreateByUserId)
                };
            });

            await Task.WhenAll(userTasks.Select(p => p.task));

            result.ForEach(p =>
            {
                p.CreateByUser = userTasks.First(t => t.postId == p.Id).task.Result;
            });
            return new PageWrapper<PostDTO>(result, pageSize, pageNumber, posts.TotalPageCount);
        }

        public async Task<bool> IgnoreAllPosts(string userId, string ignoreUserId)
        {
            var result = await _userService.BlockUser(userId, ignoreUserId);
            return result;
        }

        public async Task<bool> IgnorePostByPostId(string userId, string postId)
        {
            IgnoredPost ignoredPost = new(postId, DateTime.Now);
            var result = await _ignoredRepository.PushNewPostsToIgnoredPostsByUserId(userId, ignoredPost);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> Remove(PostDTO document)
        {
            var post = _mapper.Map<Post>(document);
            var result = await _postRepository.Remove(post);
            return result;
        }

        public async Task<bool> RemoveRange(IEnumerable<PostDTO> documents)
        {
            var posts = _mapper.Map<IEnumerable<Post>>(documents);
            var result = await _postRepository.RemoveRange(posts);
            return result;
        }

        public async Task<bool> RepostPost(string userId, string postId)
        {
            var post = (await _postRepository.Find(p => p.Id == postId)).FirstOrDefault();
            if (post is null)
            {
                throw new PostNotFoundException($"Post not found for id: {postId}");
            }
            SharePostDTO sharePostDTO = new SharePostDTO(userId, postId, DateTime.Now, DataCommon.Models.Utils.BackendType.RestAPI);
            var result = await _sharePostService.Add(sharePostDTO);
            return result is not null;
        }

        public async Task<PostDTO> Update(string id, PostDTO document)
        {
            document.Id = id;
            var postDB = await _postRepository.GetById(id);
            postDB.Content = document.Content;
            postDB.Tags = getTags(document.Content);
            var result = await _postRepository.Update(postDB);

            if (result.MatchedCount is 0)
            {
                throw new PostNotFoundException("Post not found");
            }
            return _mapper.Map<PostDTO>(postDB);
        }
        private async Task<UserDTO> getUserById(string userId)
        {
            return await _userService.GetById(userId);
        }

        private List<TagItem> getTags(string postContent)
        {
            Regex rx = new Regex(@"\#\w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var collection = rx.Matches(postContent);
            var result = collection.Select(tag => tag.Value.Replace("#", String.Empty)).ToList();

            result.ForEach(async tag =>
            {
                var tagExist = (await _tagRepository.Find(t => t.Name == tag)).FirstOrDefault();
                if (tagExist is null)
                {
                    var tagDb = new DataCommon.Models.Documents.Tag { Name = tag };
                    await _tagRepository.Add(tagDb);
                }
            });

            return result.Select(t => new TagItem(t, DateTime.Now)).ToList();
        }

        [Obsolete]
        public Task<PostDTO> Add(PostDTO document)
        {
            throw new NotImplementedException();
        }

        public async Task<PostDTO> GetByIdWhenRequestAuthenticated(string postId, string userEmail)
        {
            var post = await this.GetById(postId); 
            var  result= await CheckLikesAndSharesByUserEmail(new List<PostDTO> { post }, userEmail);
            return result.FirstOrDefault();
        }

       
        //private PostDTO MapPostToPostDTO(Post post)
        //{
        //    var postDTO = _mapper.Map<PostDTO>(post);
        //    postDTO.MultimediaDTO = post.Multimedia;
        //    return null;
        //}
    }
    public record Temp (string IngoredTagId);
    public record PostFileWrapper(Stream FileStream,string FileName);
}
