using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestAPI.Controllers
{
    [Route("api/like")]
    [ApiController]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }


        #region CRUD
        // GET: api/<LikeController>
        [HttpGet]
        public async Task<ActionResult<PageWrapper<LikesDTO>>> Get(int pageSize = 10, int pageNumber = 0)
        {
            var pagedLikes = await _likeService.GetAllPageable(pageSize, pageNumber);
            return Ok(pagedLikes);
        }

        // GET api/<LikeController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LikesDTO>> GetById(string id)
        {
            try
            {
                var like = await _likeService.GetById(id);
                return Ok(like);
            }
            catch (LikeNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/<LikeController>
        [HttpPost]
        public async Task<ActionResult<LikesDTO>> Post([FromBody] LikeCreate likeCreate)
        {
            LikesDTO likesDTO = new()
            {
                PostFor = likeCreate.PostFor,
                LikedByUserId = likeCreate.UserId
            };
            var result = await _likeService.Add(likesDTO);
            return CreatedAtAction(nameof(GetById),new { id = result.Id},result);

        }

        // PUT api/<LikeController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<LikesDTO>> Put(string id, [FromBody] LikeUpdate likeUpdate)
        {
            LikesDTO likesDTO = new()
            {
                Id = likeUpdate.Id,
                PostFor = likeUpdate.PostFor,
                LikedByUserId = likeUpdate.UserId
            };
            try
            {
                var result = await _likeService.Update(id,likesDTO);
                return Ok(result);
            }catch(LikeNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE api/<LikeController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var like = await _likeService.GetById(id);
                var removeResult = await _likeService.Remove(like);
                return removeResult ? NoContent() : Conflict();
            }
            catch (LikeNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion

        [HttpGet("likeNumber/{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<LikesNumber>> GetNumberOfLikes(string postId)
        {
            var result = await _likeService.NumberOfLikesForPost(postId);
            return Ok(new LikesNumber(result));
        }

        [HttpGet("me")]
        public async Task<ActionResult<PageWrapper<LikesDTO>>> GetMyLikedPosts([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var sort = Builders<Likes>.Sort.Descending(sh => sh.CreatedAt);
            var result = await _likeService.GetAllLikesByUserIdPageableAndSort(user.Id.ToString(),sort,pageSize,pageNumber);
            return Ok(result);
        }
        [HttpDelete("user")]
        public async Task<IActionResult> DeleteByUserIdAndPostId(string userId,string postId)
        {
            try
            {
                var removeResult = await _likeService.DeleteByUserIdAndPostId(userId, postId);
                return removeResult ? NoContent() : Conflict();
            }
            catch (LikeNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
    public record LikeCreate(string UserId,string PostFor);
    public record LikeUpdate(string Id,string UserId, string PostFor);
    public record LikesNumber(long count);
}
