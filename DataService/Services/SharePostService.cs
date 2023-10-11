using AutoMapper;
using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class SharePostService : ISharePostService
    {
        private readonly IMapper _mapper;
        private readonly ISharePostRepository _sharePostRepository;
        private readonly IUserService _userService;
        private readonly IPostRepository _postRepository;

        public SharePostService(IMapper mapper, ISharePostRepository sharePostRepository, IUserService userService, IPostRepository postRepository)
        {
            _mapper = mapper;
            _sharePostRepository = sharePostRepository;
            _userService = userService;
            _postRepository = postRepository;
        }

        public async Task<SharePostDTO> Add(SharePostDTO document)
        {
            var sharePost = _mapper.Map<SharePost>(document);
            sharePost.CreatedAt = DateTime.Now;
            await _sharePostRepository.Add(sharePost);
            await UpdateShareNumberInPost(sharePost.PostFor);
            return _mapper.Map<SharePostDTO>(sharePost);
        }

        public async Task<IEnumerable<SharePostDTO>> AddRange(IEnumerable<SharePostDTO> documents)
        {
            var sharePosts = _mapper.Map<IEnumerable<SharePost>>(documents).ToList();
            sharePosts.ForEach(sh=>sh.CreatedAt = DateTime.Now);
            await _sharePostRepository.AddRange(sharePosts);
            sharePosts.ForEach(async sp => await UpdateShareNumberInPost(sp.PostFor));
            return _mapper.Map<IEnumerable<SharePostDTO>>(sharePosts);
        }

        public async Task<IEnumerable<SharePostDTO>> GetAll()
        {
            var result = await _sharePostRepository.GetAll();
            var sharePostsDTO = _mapper.Map<IEnumerable<SharePostDTO>>(result).ToList();
            sharePostsDTO.ForEach(async sp => sp.SharedByUser = await getUserById(sp.SharedByUserId));
            return sharePostsDTO;
        }

        public async Task<PageWrapper<SharePostDTO>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var result = await _sharePostRepository.GetAllPageable(pageSize,pageNumber);
            var sharePostsDTO = _mapper.Map<PageWrapper<SharePostDTO>>(result);
            sharePostsDTO.Content.ToList().ForEach(async sp => sp.SharedByUser = await getUserById(sp.SharedByUserId));
            return sharePostsDTO;
        }

        public async Task<PageWrapper<SharePostDTO>> GetAllSharedPostByUserIdPageableAndSort(string userId, SortDefinition<SharePost> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var filter = Builders<SharePost>.Filter.Eq(sh=>sh.SharedByUserId, userId);
            var result = await _sharePostRepository.FindPageable(filter, sort, pageSize, pageNumber);
            var sharePostsDTO = _mapper.Map<PageWrapper<SharePostDTO>>(result);
            //sharePostsDTO.Content.ToList().ForEach(async sp => sp.SharedByUser = await getUserById(sp.SharedByUserId));
            return sharePostsDTO;
        }

        public async Task<SharePostDTO> GetById(string id)
        {
            var result = await _sharePostRepository.GetById(id);

            if(result is null)
            {
                throw new SharePostNotFoundException("Share post not found");
            }
            var sharePostDTO = _mapper.Map<SharePostDTO>(result);
            sharePostDTO.SharedByUser = await getUserById(sharePostDTO.SharedByUserId);
            return sharePostDTO;
        }

        public async Task<long> NumberOfSharedPosForPost(string postId)
        {
            var count = await _sharePostRepository.GetNumberOfSharedPosForPost(postId);
            return count;
        }

        public async Task<bool> Remove(SharePostDTO document)
        {
            var sharePost = _mapper.Map<SharePost>(document);

            var result = await _sharePostRepository.Remove(sharePost);
            await UpdateShareNumberInPost(document.PostFor);
            return result;
        }

        public async Task<bool> RemoveRange(IEnumerable<SharePostDTO> documents)
        {
            var sharePosts = _mapper.Map<IEnumerable<SharePost>>(documents);
            var result = await _sharePostRepository.RemoveRange(sharePosts);
            return result;
        }

        public async Task<bool> RemoveSharedPostById(string sharePostId)
        {
            return await _sharePostRepository.RemoveSharedPostById(sharePostId);
        }

        public async Task<bool> RemoveSharedPostByUserIdAndPostId(string userId, string postId)
        {
            var filter1 = Builders<SharePost>.Filter.Eq(sp => sp.SharedByUserId, userId);
            var filter2 = Builders<SharePost>.Filter.Eq(sp => sp.PostFor, postId);

            var sharePost = (await _sharePostRepository.Find(filter1 & filter2)).FirstOrDefault();

            if (sharePost is null) throw new PostNotFoundException("Post Not Found");
            var removeResult = await _sharePostRepository.Remove(sharePost);
            await UpdateShareNumberInPost(sharePost.PostFor);
            return removeResult;
        }

        public async Task<SharePostDTO> Update(string id, SharePostDTO document)
        {
            document.Id = id;
            var sharePost = _mapper.Map<SharePost>(document);
            var result = await _sharePostRepository.Update(sharePost);


            if(result.MatchedCount == 0)
            {
                throw new SharePostNotFoundException("Share post not found");
            }
            var sharePostDTO = _mapper.Map<SharePostDTO>(sharePost);
            sharePostDTO.SharedByUser = await getUserById(sharePostDTO.SharedByUserId);
            return sharePostDTO;
        }

        private async Task<UserDTO> getUserById(string userId)
        {
            return await _userService.GetById(userId);
        }

        private async Task UpdateShareNumberInPost(string postId)
        {
            var postTask = _postRepository.GetById(postId);
            var shareNumberTask = this.NumberOfSharedPosForPost(postId);
            await Task.WhenAll(postTask, shareNumberTask);
            var post = postTask.Result;
            var shareNumber = shareNumberTask.Result;
            post.ShareNumber = shareNumber;
            await _postRepository.Update(post);

        }
    }
}
