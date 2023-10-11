using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RestAPI.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        #region CRUD
        // GET: api/<PostController>
        [HttpGet]
        public async Task<ActionResult<PageWrapper<PostDTO>>> Get(int pageSize = 10, int pageNumber = 0)
        {
            var result = await _postService.GetAllPageable(pageSize, pageNumber);
            return Ok(result);
        }

        // GET api/<PostController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDTO>> GetById(string id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var email = User.FindFirstValue(ClaimTypes.Email);
                    return Ok(await _postService.GetByIdWhenRequestAuthenticated(id, email));
                }
                return Ok(await _postService.GetById(id));
            }
            catch (PostNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/<PostController>
        [HttpPost]
        [Authorize]
        //TODO add files
        public async Task<ActionResult<PostDTO>> Post([FromForm] PostCreate value, [FromServices] UserManager<ApplicationUser> userManager)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);

            var post = new PostDTO
            {
                Content = value.Contetnt,
                PostFor = String.IsNullOrEmpty(value.PostFor)? "": value.PostFor,
                CreateByUserId = value.CreateByUserId
            };

            var postFilesList = new List<PostFileWrapper>();
            if(value.Files != null && value.Files.Length > 0)
            {
                postFilesList = value.Files.Select(f => new PostFileWrapper(f.OpenReadStream(), f.FileName)).ToList();
            }
            
            var result = await _postService.Add(post,user.Id.ToString(), postFilesList);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT api/<PostController>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<PostDTO>> Put(string id, [FromBody] PostUpdate value)
        {
            var post = new PostDTO
            {
                Id = value.Id,
                Content = value.Contetnt,
                PostFor = value.PostFor,
                CreateByUserId = value.CreateByUserId
            };
            var result = await _postService.Update(id, post);
            return Ok(result);
        }

        // DELETE api/<PostController>/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var post = await _postService.GetById(id);
                var removeResult = await _postService.Remove(post);
                return removeResult ? NoContent() : Conflict();
            } catch (PostNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion

        [HttpPut("ignore/allPosts")]
        [Authorize]
        public async Task<IActionResult> IgnoreAllPosts([FromServices] UserManager<ApplicationUser> userManager, string ignoreUserId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            var result = await _postService.IgnoreAllPosts(userId, ignoreUserId);
            return result ? NoContent() : NotFound();
        }

        [HttpPut("ignore")]
        [Authorize]
        public async Task<IActionResult> IgnorePost([FromServices] UserManager<ApplicationUser> userManager, string postId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            var result = await _postService.IgnorePostByPostId(userId, postId);
            return result ? NoContent() : NotFound();
        }

        [HttpPut("repost")]
        [Authorize]
        public async Task<IActionResult> Respost([FromServices] UserManager<ApplicationUser> userManager, string postId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            var result = await _postService.RepostPost(userId, postId);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<PostDTO>>> Feed([FromServices]UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            
            var feed = await _postService.Feed(user.Id.ToString(),pageNumber,pageSize);
            return Ok(feed);
        }

        [HttpGet("liked")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<PostDTO>>> MyLikedPosts([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            try
            {
                var result = await _postService.GetLikedPostByUserIdPageableSortByDate(userId, pageSize, pageNumber);
                return Ok(result);
            }catch(PostTransactionException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("shares")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<PostDTO>>> MySharedPosts([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            try
            {
                var result = await _postService.GetSharePostByUserIdPageableSortByDate(userId, pageSize, pageNumber);
                return Ok(result);
            }
            catch(PostTransactionException ex)
            {
                return Conflict(ex.Message);
            }
            
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<PostDTO>>> GetMyPosts([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            var result = await _postService.GetAllPostByUserIdPageableSortByDate(userId, pageSize, pageNumber);
            return Ok(result);
        }

        [HttpGet("subpost/user")]
        public async Task<ActionResult<PageWrapper<PostDTO>>> GetSubPostForUser(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var result = await _postService.GetAllSubpostForPostByUserIdPageableSortByDate(userId, pageSize, pageNumber);
            return Ok(result);
        }

        [HttpGet("subpost/{postId}")]
        public async Task<ActionResult<PageWrapper<PostDTO>>> GetSubpostForPost(string postId, int pageSize = 10, int pageNumber = 0)
        {
            var result = await _postService.GetSubpostsForPostByIdSortNewestAndPageable(postId,pageSize,pageNumber);
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                result.Content = await (_postService as PostService).CheckLikesAndSharesByUserEmail(result.Content.ToList(), email);
            }
            return Ok(result);

           

        }

        [HttpGet("newest")]
        public async Task<ActionResult<PageWrapper<PostDTO>>> GetNewestPostByTag(string tag, int pageSize = 10, int pageNumber = 0)
        {
            var result = await _postService.GetNewestPostByTag(tag, pageSize, pageNumber);
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                result.Content = await ((PostService)_postService).CheckLikesAndSharesByUserEmail(result.Content.ToList(),email);
            }
            
            return Ok(result);
        }

        [HttpGet("popular")]
        public async Task<ActionResult<PageWrapper<PostDTO>>> GetPopularPostByTag(string tag, int pageSize = 10, int pageNumber = 0)
        {
            var result = await _postService.GetPopularPostByTag(tag,pageSize,pageNumber);
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                result.Content = await ((PostService)_postService).CheckLikesAndSharesByUserEmail(result.Content.ToList(), email);
            }
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PageWrapper<PostDTO>>> GetPostByUserId(string userId, int pageSize = 10, int pageNumber = 0)
        {
            var result = await _postService.GetAllPostByUserIdPageableSortByDate(userId, pageSize, pageNumber);
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                result.Content = await ((PostService)_postService).CheckLikesAndSharesByUserEmail(result.Content.ToList(), email);
            }
            return Ok(result);
        }

    }
    public record PostCreate(string Contetnt, string PostFor,string CreateByUserId)
    {
        public IFormFile[] Files{ get; set; }
    };
    public record PostUpdate(string Id,string Contetnt, string PostFor,string CreateByUserId);
}
