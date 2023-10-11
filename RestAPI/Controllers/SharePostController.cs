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
    [Route("api/sharePost")]
    [ApiController]
    [Authorize]
    public class SharePostController : ControllerBase
    {
        private readonly ISharePostService _sharePostService;

        public SharePostController(ISharePostService sharePostService)
        {
            _sharePostService = sharePostService;
        }
        #region CRUD
        // GET: api/<SharePostController>
        [HttpGet]
        public async Task<ActionResult<PageWrapper<SharePostDTO>>> Get(int pageSize = 10, int pageNumber = 0)
        {
            var result = await _sharePostService.GetAllPageable(pageSize, pageNumber);
            return Ok(result);
        }

        // GET api/<SharePostController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SharePostDTO>> GetById(string id)
        {
            try
            {
                var sharePostDTO = await _sharePostService.GetById(id);
                return Ok(sharePostDTO);
            } catch (SharePostNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/<SharePostController>
        [HttpPost]
        public async Task<ActionResult<SharePostDTO>> Post([FromBody] SharePostCreate value)
        {
            var sharePostDTO = new SharePostDTO()
            {
                SharedByUserId = value.SharedByUserId,
                PostFor = value.PostFor
            };
            var result = await _sharePostService.Add(sharePostDTO);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT api/<SharePostController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<SharePostDTO>> Put(string id, [FromBody] SharePostUpdate value)
        {
            var sharePost = new SharePostDTO
            {
                Id = value.Id,
                PostFor = value.PostFor,
                SharedByUserId = value.SharedByUserId
            };
            try
            {
                var result = await _sharePostService.Update(id, sharePost);
                return Ok(result);
            } catch (SharePostNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE api/<SharePostController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _sharePostService.RemoveSharedPostById(id);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("user")]
        public async Task<IActionResult> DeleteByUserAndPost(string userId,string postId)
        {
            var result = await _sharePostService.RemoveSharedPostByUserIdAndPostId(userId,postId);
            return result ? NoContent() : NotFound();
        }
        #endregion

        [HttpGet("shareNumber/{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<SharePostNumber>> GetNumberOfShares(string postId){
            var result = await _sharePostService.NumberOfSharedPosForPost(postId);
            return Ok(new SharePostNumber(result));
        }

        [HttpGet("me")]
        public async Task<ActionResult<PageWrapper<SharePostDTO>>> GetMySharedPosts([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var sort = Builders<SharePost>.Sort.Descending(sh=>sh.CreatedAt);
            var result = await _sharePostService.GetAllSharedPostByUserIdPageableAndSort(user.Id.ToString(), sort, pageSize, pageNumber);
            return Ok(result);
        }

    }
    public record SharePostCreate(string PostFor, string SharedByUserId);
    public record SharePostUpdate(string Id,string PostFor,string SharedByUserId);
    public record SharePostNumber(long SharePostCount);
}
