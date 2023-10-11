using AutoMapper;
using DataCommon;
using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;
        private readonly IIgnoredRepository _ignoredRepository;
        private readonly IDbClient _dbClient;

        public TagService(ITagRepository tagRepository, IMapper mapper, IIgnoredRepository ignoredRepository, IDbClient dbClient)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
            _ignoredRepository = ignoredRepository;
            this._dbClient = dbClient;
        }

        public async Task<TagDTO> Add(TagDTO document)
        {
            var tag = _mapper.Map<DataCommon.Models.Documents.Tag>(document);
            tag.CreatedAt = DateTime.Now;

            var isExist =  (await _tagRepository.Find(t => t.Name == document.Name)).FirstOrDefault();
            if (isExist is not null)
            {
                throw new TagExistException("Tag with this contetnt exist");
            }

            await _tagRepository.Add(tag);
            var tagDTO = _mapper.Map<TagDTO>(tag);
            return tagDTO;
        }

        public async Task<IEnumerable<TagDTO>> AddRange(IEnumerable<TagDTO> documents)
        {
            var tags = _mapper.Map<IEnumerable<DataCommon.Models.Documents.Tag>>(documents).ToList();
            tags.ForEach(t => t.CreatedAt = DateTime.Now);
            await _tagRepository.AddRange(tags);
            var tagsDTO = _mapper.Map<IEnumerable<TagDTO>>(tags);
            return tagsDTO;
        }

        public async Task<bool> DeleteTagByName(string name)
        {
            var tag = (await _tagRepository.Find(t => t.Name == name))
                .FirstOrDefault();

            if (tag is null)
            {
                throw new TagNotFoundException("Tag not found");
            }

            var result = await _tagRepository.Remove(tag);
            return result;
        }
        public async Task<IEnumerable<TagDTO>> GetAll()
        {
            var documents = await _tagRepository.GetAll();
            var tagsDTO = _mapper.Map<IEnumerable<TagDTO>>(documents);
            return tagsDTO;
        }

        public async Task<PageWrapper<TagDTO>> GetAllIgnoredTagsForUserPageableAndSort(string userId, SortDefinition<DataCommon.Models.Documents.Tag> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var ignores = await _ignoredRepository.Find(i=>i.UserId == userId);
            var ignore = ignores.FirstOrDefault();

            if(ignore is null)
            {
                throw new UserNotFoundException("Ignore for this user not found");
            }

            var ignoredTagsId = ignore.IgnoredTags.Select(it=>it.TagId);

            var filter =Builders<DataCommon.Models.Documents.Tag>.Filter.In(t => t.Id, ignoredTagsId);

            var countTask =  _dbClient.GetTagsCollection().Find(filter).CountDocumentsAsync();
            var tagsTask =  _tagRepository.FindPageable(filter,sort,pageSize,pageNumber);

            await Task.WhenAll(countTask, tagsTask);

            var tagsDTO = _mapper.Map<IEnumerable<TagDTO>>(tagsTask.Result.Content);

            var totalPageCount = Convert.ToInt32(Math.Ceiling(countTask.Result / (double)pageSize));

            return new PageWrapper<TagDTO>(
                tagsDTO,
                pageSize,
                pageNumber,
                totalPageCount);
        }

        public async Task<PageWrapper<TagDTO>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var documents = await _tagRepository.GetAllPageable(pageSize,pageNumber);
            var pagedTagsDTO = _mapper.Map<PageWrapper<TagDTO>>(documents);
            return pagedTagsDTO;
        }

        public async Task<PageWrapper<TagDTO>> GetAllPageableAndSort(int pageSize = 10, int pageNumber = 0, MongoDB.Driver.SortDefinition<DataCommon.Models.Documents.Tag> sort = null)
        {
            var documents = await _tagRepository.FindPageable(_ => true, sort, pageSize, pageNumber);
            var pagedTagsDTO = _mapper.Map<PageWrapper<TagDTO>>(documents);
            return pagedTagsDTO;
        }

        public async Task<TagDTO> GetById(string id)
        {
            var document = await _tagRepository.GetById(id);

            if(document is null)
            {
                throw new TagNotFoundException("Tag not found");
            }

            var tagDTO = _mapper.Map<TagDTO>(document);
            return tagDTO;
        }

        public async Task<PageWrapper<TagDTO>> GetPopularTagsInThis(TimeDuration timeDuration, int pageSize = 10, int pageNumber = 0, MongoDB.Driver.SortDefinition<DataCommon.Models.Documents.Tag> sort = null)
        {
            var postCollection = _dbClient.GetPostCollection();
            var timeDurations = DateTime.Now.AddDays(- ((int) timeDuration));

            var aggregate = postCollection.Aggregate()
                .Match(p => p.CreatedAt >= timeDurations)
                .Project(p => p.Tags);

            var aggregateResult = await aggregate.ToListAsync();

            var tagsAll = aggregateResult
                .SelectMany(e => e)
                .GroupBy(t => t)
                .Select(t => new
                {
                    Tag = t.Key,
                    Count = t.Count()
                })
                .OrderByDescending(t => t.Count)
                .ToList();


            var tags = tagsAll
                .Skip(pageSize * pageNumber)
                .Take(pageSize)
                .ToList();


            var mongoClient = _dbClient.GetDatabase().Client;

            using (var session =await mongoClient.StartSessionAsync())
            {

                session.StartTransaction();
                try
                {
                    var tasks = new List<Task<TagDTO>>();
                    tags.ForEach(tag => tasks.Add(this.GetTagByName(tag.Tag.TagId)));

                    await Task.WhenAll(tasks);
                    var tagsDTO = tasks.Select(t=>t.Result);
                    var totalPages = Convert.ToInt32(Math.Ceiling(tagsAll.Count / (double)pageSize));

                    await session.CommitTransactionAsync();
                    return new PageWrapper<TagDTO>(tagsDTO,pageSize,pageNumber, totalPages);
                }catch(Exception)
                {
                    await session.AbortTransactionAsync();
                    throw new TagTransactionException("Transaction Error");
                }
            }
        }

        public async Task<TagDTO> GetTagByName(string name)
        {
            var tag = (await _tagRepository.Find(t => t.Name == name))
                .FirstOrDefault();

            if (tag is null)
            {
                throw new TagNotFoundException("Tag not found");
            }

            var tagDTO = _mapper.Map<TagDTO>(tag);

           return tagDTO;
        }

        public async Task<bool> IgnoreTagForUser(string name, string userId)
        {
            var isContainTag = await _ignoredRepository.IsContainsTagNameInIgnoredTagsByUserId(userId, name);
            if (isContainTag)
            {
                return true;
            }
            var tags = await _tagRepository.Find(t=>t.Name == name);
            var tag = tags.FirstOrDefault();

            if(tag is null)
            {
                return false;
            }

            var ignoredTag = new IgnoredTag(tag.Id,tag.Name,DateTime.Now);
            var result = await _ignoredRepository.PushNewTagToIgnoredTagsByUserId(userId, ignoredTag);
            return true;
        }

        public async Task<bool> IgnoreTagIdForUser(string tagId, string userId)
        {
            var isContainTag = await _ignoredRepository.IsContainsTagIdInIgnoredTagsByUserId(userId, tagId);
            if (isContainTag)
            {
                return true;
            }
            var tags = await _tagRepository.Find(t => t.Id == tagId);
            var tag = tags.FirstOrDefault();

            if (tag is null)
            {
                return false;
            }

            var ignoredTag = new IgnoredTag(tag.Id, tag.Name, DateTime.Now);
            var result = await _ignoredRepository.PushNewTagToIgnoredTagsByUserId(userId, ignoredTag);
            return true;
        }

        public async Task<bool> Remove(TagDTO document)
        {
            var tag = _mapper.Map<DataCommon.Models.Documents.Tag>(document);
            var result = await _tagRepository.Remove(tag);
            return result;
        }

        public async Task<bool> RemoveRange(IEnumerable<TagDTO> documents)
        {
            var tags = _mapper.Map<IEnumerable<DataCommon.Models.Documents.Tag>>(documents);
            var result = await _tagRepository.RemoveRange(tags);
            return result;
        }

        public async Task<PageWrapper<TagDTO>> SearchPageable(string q, int pageSize = 10, int pageNumber = 0)
        {
            string pattern = @"\b" + q + @"\w+";
            var rx = new Regex(pattern,RegexOptions.IgnoreCase);
            var filter = Builders<DataCommon.Models.Documents.Tag>.Filter.Regex(t=>t.Name, new BsonRegularExpression(rx));
            var tags = await _tagRepository.FindPageable(filter, null, pageSize, pageNumber);
            var pageTagDTO = _mapper.Map<PageWrapper<TagDTO>>(tags);
            return pageTagDTO;
        }

        public async Task<bool> UnignoreTagForUser(string name, string userId)
        {
            var isContainTag = await _ignoredRepository.IsContainsTagNameInIgnoredTagsByUserId(userId, name);
            if (!isContainTag)
            {
                throw new IgnoredTagNotFoundException($"Ignore tag name: [{name}], for this user not found. User id: [{userId}]");
            }
            var tags = await _tagRepository.Find(t => t.Name == name);
            var tag = tags.FirstOrDefault();

            if (tag == null)
            {
                throw new TagNotFoundException($"Tag not found for this name: [{name}]");
            }
            var ignoredTag = new IgnoredTag(tag.Id, tag.Name, DateTime.Now);
            return await _ignoredRepository.RemoveTagFromIgnoredTagsByUserId(userId,ignoredTag);
        }

        public async Task<bool> UnignoreTagIdForUser(string tagId, string userId)
        {
            var isContainTag = await _ignoredRepository.IsContainsTagNameInIgnoredTagsByUserId(userId, tagId);
            if (!isContainTag)
            {
                throw new IgnoredTagNotFoundException($"Ignore tag id: [{tagId}], for this user not found. User id: [{userId}]");
            }
            var tags = await _tagRepository.Find(t => t.Id == tagId);
            var tag = tags.FirstOrDefault();

            if (tag == null)
            {
                throw new TagNotFoundException($"Tag not found for this id: [{tagId}]");
            }
            var ignoredTag = new IgnoredTag(tag.Id, tag.Name, DateTime.Now);
            return await _ignoredRepository.RemoveTagFromIgnoredTagsByUserId(userId, ignoredTag);
        }

        public async Task<TagDTO> Update(string id, TagDTO document)
        {
            document.Id = id;
            var tag = _mapper.Map<DataCommon.Models.Documents.Tag>(document);
            var result = await _tagRepository.Update(tag);

            if(result.MatchedCount == 0)
            {
                throw new TagNotFoundException("Tag not found");
            }

            return _mapper.Map<TagDTO>(tag);
        }
    }
}
