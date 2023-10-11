using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
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
    [Route("api/tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        #region CRUD
        // GET: api/<TagController>
        [HttpGet]
        public async Task<ActionResult<PageWrapper<TagDTO>>> Get(int pageSize = 10, int pageNumber = 0)
        {
            var pagedTags = await _tagService.GetAllPageableAndSort(pageSize, pageNumber);
            return Ok(pagedTags);
        }

        // GET api/<TagController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagDTO>> GetById(string id)
        {
            try {
                var tag = await _tagService.GetById(id);
                return Ok(tag);
            }
            catch(TagNotFoundException tEx){
                return NotFound(tEx.Message);
            }
           
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<TagDTO>> GetByNameAsync(string name)
        {
            try
            {
                var tag = await _tagService.GetTagByName(name);
                return Ok(tag);
            }
            catch (TagNotFoundException tEx)
            {
                return NotFound(tEx.Message);
            }
        }

        // POST api/<TagController>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TagDTO>> Post([FromBody] TagDTO value)
        {
            try
            {
                var result = await _tagService.Add(value);
                return CreatedAtAction(nameof(GetById),new { id = result.Id},result);
            }catch(TagExistException tEEx)
            {
                return Conflict(tEEx.Message);
            }
        }

        // PUT api/<TagController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<TagDTO>> Put(string id, [FromBody] TagDTO value)
        {
            try
            {
                var result = await _tagService.Update(id,value);
                return  Ok(result);
            }
            catch (TagNotFoundException tEx)
            {
                return NotFound(tEx.Message);
            }
        }

        // DELETE api/<TagController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles =UserRoles.Admin)]
        public async Task<IActionResult> DeleteById(string id)
        {
            try
            {
               var tag = await _tagService.GetById(id);
               var result = await _tagService.Remove(tag);
               return result ? NoContent() : Conflict();
            }
            catch (TagNotFoundException tEx)
            {
                return NotFound(tEx.Message);
            }
        }

        [HttpDelete("name/{name}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteByName(string name)
        {
            try
            {
                var tag = await _tagService.GetTagByName(name);
                var result = await _tagService.Remove(tag);
                return result ? NoContent() : Conflict();
            }
            catch (TagNotFoundException tEx)
            {
                return NotFound(tEx.Message);
            }
        }
        #endregion

        [HttpGet("search")]
        public async Task<ActionResult<PageWrapper<TagDTO>>> Search(string query, int pageSize = 10, int pageNumber = 0)
        {
            var tag = await _tagService.SearchPageable(query,pageSize,pageNumber);
            return Ok(tag);
        }

        [HttpGet("popular")]
        public async Task<ActionResult<PageWrapper<TagDTO>>> GetPopularTags(int pageSize = 10, int pageNumber = 0)
        {
            try
            {
                var tag = await _tagService.GetPopularTagsInThis(TimeDuration.WEEK, pageSize, pageNumber);
                return Ok(tag);
            }catch(TagTransactionException ex)
            {
                return Conflict(ex.Message);
            }
        }

        //[HttpGet("me/popular")]
        //[Authorize]
        //public async Task<ActionResult<PageWrapper<TagDTO>>> GetPopularTagsForMe([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        //{
        //    //var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        //    //var user = await userManager.FindByEmailAsync(userEmail);
        //    //try
        //    //{
        //    //    var tag = await _tagService.GetPopularTagsByUserIdPageableSort(user.Id.ToString(), pageSize, pageNumber);
        //    //    return Ok(tag);
        //    //}
        //    //catch(UserNotFoundException ex)
        //    //{
        //    //    return NotFound(ex.Message);
        //    //}
        //    await Task.Delay(1000);
        //    return StatusCode(StatusCodes.Status501NotImplemented);
        //}

        [HttpPut("me/ignore")]
        [Authorize]
        public async Task<IActionResult> IgnoreTag([FromServices] UserManager<ApplicationUser> userManager,string tag)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var result = await _tagService.IgnoreTagForUser(tag, user.Id.ToString());
            return result ? NoContent() : Conflict();
        }

        [HttpDelete("me/ignore")]
        [Authorize]
        public async Task<IActionResult> UnignoreTag([FromServices] UserManager<ApplicationUser> userManager, string tag)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var result = await _tagService.UnignoreTagForUser(tag, user.Id.ToString());
            return result ? NoContent() : Conflict();
        }

        [HttpGet("me/ignore")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<TagDTO>>> GetMyIgnoreTags([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0) {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var sort = Builders<DataCommon.Models.Documents.Tag>.Sort.Ascending(t => t.Name);
            var result = await _tagService.GetAllIgnoredTagsForUserPageableAndSort(user.Id.ToString(),sort,pageSize,pageNumber);
            return result;
        }
    }
}
