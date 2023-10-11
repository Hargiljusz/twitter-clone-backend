using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.DTO.Utils;
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
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        #region CRD
        // GET: api/<UserController>
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<PageWrapper<UserDTO>>> Get(int pageSize = 10, int pageNumber = 0)
        {
            var result = await _userService.GetAll(pageSize, pageNumber);
            return Ok(result);
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetById(string id)
        {
            var result = await _userService.GetById(id);
            return Ok(result);
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> Put(string id, [FromBody] UpdateUser value)
        {
            try
            {
                var result = await _userService.UpdateDataByUserId(id, value);
                return Ok(result);
            }
            catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _userService.RemoveUser(id);
                return NoContent();
            }catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(DeleteUserException ex)
            {
                return Conflict(ex.Message);
            }

        }
        #endregion

        [Authorize]
        [HttpPut("report")]
        public async Task<ActionResult> ReportUser([FromBody] ReportUser reportUser)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();
            try
            {
                var result = await _userService.ReportUser(userId, reportUser.ReportUserId, reportUser.Description);
                return result ? NoContent() : Conflict();
            }catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("report")]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports(string userId)
        {
            try
            {
                var result = await _userService.GetReportsByUserId(userId);
                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<PageWrapper<UserDTO>>> Search(string q, int pageSize = 10, int pageNumber = 0)
        {
            var sort = Builders<ApplicationUser>.Sort.Ascending(u=>u.Surename).Ascending(u=>u.Name);
            var result = await _userService.SearchPageable(q,sort,pageSize,pageNumber);
            return Ok(result);
        }

        [HttpPut("block")]
        [Authorize]
        public async Task<ActionResult> BlockUser(string blockUserId)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            var userId = user.Id.ToString();

            try
            {
                var result = await _userService.BlockUser(userId, blockUserId);
                return result ? NoContent() : Conflict();
            }catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("ban")]
        [Authorize(Roles =UserRoles.Admin)]
        public async Task<ActionResult> BanUser([FromBody] BanUser banUser)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            var adminUserId = user.Id.ToString();
            try
            {
                var result = await _userService.BanUserByUserId(adminUserId, banUser.BanUserId, banUser.Description);
                return result ? NoContent() : Conflict();
            }catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("unban")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> UnBanUser(string userId)
        {
            try
            {
                var result = await _userService.UnBanUserByUserId(userId);
                return result ? NoContent() : Conflict();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpPut("warning")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> AddWarning([FromBody] WarningUser warningUser)
        {
            var userEmail = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(userEmail);
            var adminUserId = user.Id.ToString();
            try
            {
                var result = await _userService.AddWarning(adminUserId,warningUser.Description,warningUser.WarningUserId);
                return result is not null ? NoContent() : Conflict();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("warning")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Warning>>> GetWarnings(string userId)
        {
            try
            {
                var result = await _userService.GetWarningsByUserId(userId);
                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("block/list/{userId}")]
        [Authorize]
        public async Task<ActionResult<PageWrapper<UserDTO>>> GetBlockUserList(string userId, int pageSize = 10, int pageNumber = 0)
        {
            try
            {
                var result = await _userService.GetBlockUserByUserIdPageableSort(userId, pageSize, pageNumber);

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(UserTransactionException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
    public record ReportUser(string ReportUserId,string Description);
    public record WarningUser(string WarningUserId, string Description);
    public record BanUser(string BanUserId, string Description);
}
