using AutoMapper;
using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using MongoDB.Driver;

namespace DataService.Services
{
    public class LikeService : ILikeService
    {
        private readonly IMapper _mapper;
        private readonly ILikesRepositiry _likesRepository;
        private readonly IUserService _userService;
        private readonly IPostRepository _postRepository;


        public LikeService(IMapper mapper, ILikesRepositiry likesRepository, IUserService userService, IPostRepository postRepository)
        {
            _mapper = mapper;
            _likesRepository = likesRepository;
            _userService = userService;
            _postRepository = postRepository;
        }

        public async Task<LikesDTO> Add(LikesDTO document)
        {
            var like = _mapper.Map<Likes>(document);
            like.CreatedAt = DateTime.Now;
            await _likesRepository.Add(like);
            await UpdateLikeNumberInPost(like.PostFor);
            return _mapper.Map<LikesDTO>(like);
        }

        public async Task<IEnumerable<LikesDTO>> AddRange(IEnumerable<LikesDTO> documents)
        {
            var likes = _mapper.Map<IEnumerable<Likes>>(documents).ToList();
            likes.ForEach(sh => sh.CreatedAt = DateTime.Now);
            await _likesRepository.AddRange(likes);
            likes.ForEach(async l=> await UpdateLikeNumberInPost(l.PostFor));
            return _mapper.Map<IEnumerable<LikesDTO>>(likes);
        }

        public async Task<bool> DeleteById(string postId)
        {
            var like = await _likesRepository.GetById(postId);
            var result = await _likesRepository.RemoveByPost(postId);
            await UpdateLikeNumberInPost(like.PostFor);
            return result;
        }

        public async Task<bool> DeleteByUserIdAndPostId(string userId, string postId)
        {
            var filter1 = Builders<Likes>.Filter.Eq(l => l.LikedByUserId, userId);
            var filter2 = Builders<Likes>.Filter.Eq(l => l.PostFor, postId);

            var like = (await _likesRepository.Find(filter1 & filter2)).FirstOrDefault();
            if(like == null)
            {
                throw new LikeNotFoundException("Like not found");
            }
            var deleteResult = await _likesRepository.Remove(like);
            await UpdateLikeNumberInPost(like.PostFor);
            return deleteResult;
        }

        public async Task<IEnumerable<LikesDTO>> GetAll()
        {
            var result = await _likesRepository.GetAll();
            var likesDTO = _mapper.Map<IEnumerable<LikesDTO>>(result);
            likesDTO.ToList().ForEach(async l =>
            {
                l.LikedByUser = await getUserById(l.LikedByUserId);
            });
            return likesDTO;
        }

        public async Task<PageWrapper<LikesDTO>> GetAllLikesByUserIdPageableAndSort(string userId, SortDefinition<Likes> sort = null, int pageSize = 10, int pageNumber = 0)
        {
            var filter = Builders<Likes>.Filter.Eq(l=>l.LikedByUserId,userId);
            var likes = await _likesRepository.FindPageable(filter,sort,pageSize,pageNumber);;
            var likesDTO = _mapper.Map<PageWrapper<LikesDTO>>(likes);
            //var user = await getUserById(userId);
            //likesDTO.Content.ToList().ForEach(l=>l.LikedByUser = user);
            return likesDTO;
        }

        public async Task<PageWrapper<LikesDTO>> GetAllPageable(int pageSize = 10, int pageNumber = 0)
        {
            var documents = await _likesRepository.GetAllPageable( pageSize, pageNumber);
            var pagedLikesDTO = _mapper.Map<PageWrapper<LikesDTO>>(documents);
            pagedLikesDTO.Content.ToList().ForEach(async l =>
            {
                l.LikedByUser = await getUserById(l.LikedByUserId);
            });
            return pagedLikesDTO;
        }

        public async Task<LikesDTO> GetById(string id)
        {
            var like = await _likesRepository.GetById(id);
            if(like is null)
            {
                throw new LikeNotFoundException("Like not found");
            }
            var likeDTO = _mapper.Map<LikesDTO>(like);
            likeDTO.LikedByUser = await getUserById(likeDTO.LikedByUserId);
            return likeDTO;
        }

        public async Task<long> NumberOfLikesForPost(string postId)
        {
            return await _likesRepository.GetNumberOfLikesForPost(postId);
        }

        public async Task<bool> Remove(LikesDTO document)
        {
            var like = _mapper.Map<Likes>(document);
            var result = await _likesRepository.Remove(like);
            return result;
        }

        public async Task<bool> RemoveRange(IEnumerable<LikesDTO> documents)
        {
            var likes = _mapper.Map<IEnumerable<Likes>>(documents);
            var result = await _likesRepository.RemoveRange(likes);
            return result;
        }

        public async Task<LikesDTO> Update(string id, LikesDTO document)
        {
            document.Id = id;
            var like = _mapper.Map<Likes>(document);
            var result =await _likesRepository.Update(like);

            if (result.MatchedCount == 0)
            {
                throw new LikeNotFoundException("Like not found");
            }

            return _mapper.Map<LikesDTO>(like);
        }

        private async Task<UserDTO> getUserById(string userId)
        {
            return await _userService.GetById(userId);
        }
        private async Task UpdateLikeNumberInPost(string postId)
        {
            var postTask = _postRepository.GetById(postId);
            var likeNumberTask = this.NumberOfLikesForPost(postId);
            await Task.WhenAll(postTask, likeNumberTask);
            var post = postTask.Result;
            var likeNumber = likeNumberTask.Result;
            post.LikeNumber = likeNumber;
            await _postRepository.Update(post);

        }
    }
}
