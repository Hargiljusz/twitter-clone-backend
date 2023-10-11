using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestAPI.Controllers
{
    [Route("api/follow")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        #region CRUD
        // GET: api/<FollowController>
        [HttpGet]
        public async Task<ActionResult<PageWrapper<FollowerDTO>>> Get(int pageSize = 10, int pageNumber = 0)
        {
            var result = await _followService.GetAllPageable(pageSize, pageNumber);
            return Ok(result);
        }

        // GET api/<FollowController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FollowerDTO>> GetById(string id)
        {
            try
            {
                var result = await _followService.GetById(id);
                return Ok(result);
            }catch(FollowerNotFoundExceptions ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/<FollowController>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<FollowerDTO>> Post([FromBody] FollowCreate value)
        {
            var followerDTO = new FollowerDTO
            {
                To = value.To,
                From = value.From,
            };
            var result = await _followService.Add(followerDTO);
            return CreatedAtAction(nameof(GetById) , new {id = result.Id},result);
        }

        // PUT api/<FollowController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<FollowerDTO>> Put(string id, [FromBody] FollowUpdate value)
        {
            var followerDTO = new FollowerDTO
            {
                Id = value.Id,
                To = value.To,
                From = value.From,
            };

            try
            {
                var result = await _followService.Update(id,followerDTO);
                return Ok(result);
            }catch(FollowerNotFoundExceptions ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE api/<FollowController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles =UserRoles.Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var followDTO = await _followService.GetById(id);
                var removeResult = await _followService.Remove(followDTO);
                return removeResult ? NoContent() : Conflict();
            }
            catch (FollowerNotFoundExceptions ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion

        [HttpPut("unfollow")]
        [Authorize]
        public async Task<IActionResult> Unfollow([FromServices] UserManager<ApplicationUser> userManager, string followUserId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();

            try
            {
                var result = await _followService.UnFollowUser(userId, followUserId);
                return NoContent();
            }catch(FollowerNotFoundExceptions ex)
            {
                return NotFound($"Follow for thi users doesn't exist. \n{ex.Source}");
            }
        }

        [HttpPut("follow")]
        [Authorize]
        public async Task<IActionResult> Follow([FromServices] UserManager<ApplicationUser> userManager, string followUserId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();

            try
            {
                var result = await _followService.FollowUser(userId, followUserId);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("followersNumber")]
        [AllowAnonymous]
        public async Task<ActionResult<CountF>> GetFollowersNumber(string userId)
        {
            var count = await _followService.GetFollowersByUserId(userId);
            return Ok(new CountF(count));
        }

        [HttpGet("followeringNumber")]
        [AllowAnonymous]
        public async Task<ActionResult<CountF>> GetFollowingNumber(string userId)
        {
            var count = await _followService.GetFollowingByUserId(userId);
            return Ok(new CountF(count));
        }

        [HttpGet("check")]
        [Authorize]
        public async Task<ActionResult<CheckFollow>> GetFollowProposition([FromServices] UserManager<ApplicationUser> userManager, string checkUserId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            var result = await _followService.CheckFollow(userId, checkUserId);
            return Ok(result);
        }

        [HttpGet("myFollowers")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<UserDTO>>> GetMyFollowers([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();

            try
            {
                var result = await _followService.GetListOfFollowerUsersPeageableAndSort(userId, pageSize, pageNumber);
                return Ok(result);
            }catch(FollowerTransactionException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("myFollowings")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<UserDTO>>> GetMyFollowings([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();

            try
            {
                var result = await _followService.GetListOfFollowingUsersPeageableAndSort(userId, pageSize, pageNumber);
                return Ok(result);
            }catch(FollowerTransactionException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("myPropositions")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<UserDTO>>> GetMyPropositions([FromServices] UserManager<ApplicationUser> userManager, int pageSize = 10, int pageNumber = 0)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();

            try
            {
                var result = await _followService.GetListOfPropositionUsersPeageableAndSort(userId, pageSize, pageNumber);
                return Ok(result);
            }catch(FollowerTransactionException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
    public record FollowCreate(string From,string To);
    public record FollowUpdate(string Id,string From, string To);
    public record CountF(long Count);
}
