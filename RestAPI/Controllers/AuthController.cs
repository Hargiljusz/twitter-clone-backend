using DataCommon.Models.Documents;
using DataService.DTO.Utils;
using DataService.Services.Interfaces.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RestAPI.Controllers
{
    [Authorize]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;
        private IConfiguration _config;
        private const string RefrestTokenName = "refreshToken";


        public AuthController(IUserService userService, UserManager<ApplicationUser> userManager, IAuthService authService, IConfiguration config)
        {
            _userService = userService;
            _userManager = userManager;
            _authService = authService;
            _config = config;
        }

        [HttpPost("singIn")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);

            if (user == null)
            {
                return Unauthorized();
            }
            if (!user.EmailConfirmed)
            {
                return Forbid();
            }

            var passwordCorrect = await _userManager.CheckPasswordAsync(user, loginModel.Password);
            if (!passwordCorrect)
            {
                return Forbid();
            }
            var jwt = await _authService.GenerateJWT(user);
            var refreshToken = _authService.GenerateRefreshToken(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            SetCookie(RefrestTokenName, refreshToken, Int32.Parse(_config["Jwt:ExpRefreshTokenHours"]) * 60);
            return Ok(new LoginResponse(jwt,userRoles,user.CreatedOn,user.Id.ToString(),user.Email,user.UserName,user.Nick));
        }

        [HttpPost("singUp")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromForm]CreateUserWithPhoto createUserWithPhoto)
        {   
            
            CreateUser newUser = createUserWithPhoto.createUser;
            IFormFile mainPhoto = createUserWithPhoto.MainPhoto;
            IFormFile bgPhoto = createUserWithPhoto.BgPhoto;


            var userExist = await _userManager.FindByEmailAsync(newUser.Email);
            if (userExist is not null)
            {
                return Conflict("User exist");
            }

            var result = await _userService.CreateUser(newUser,mainPhoto.OpenReadStream(),mainPhoto.FileName,bgPhoto.OpenReadStream(),bgPhoto.FileName);
            if (result)
            {
                //var user = await _userManager.FindByEmailAsync(newUser.Email);
                //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return NoContent();
            }
            else
            {
                return Conflict("User creation failed! Please check user details and try again.");
            }
        }

        [HttpDelete("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            Response.Cookies.Delete(RefrestTokenName, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTime.Now.AddDays(-1)
            });

            return NoContent();
        }

        [HttpGet("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> RefreshToken()
        {

            var refresh_token = Request.Cookies[RefrestTokenName];
            if (refresh_token == null)
            {
                return BadRequest(new  { Status = "Cookie is not set.", Message = "Error cookie!" });
            }
            var validate = _authService.ValidRefreshToken(refresh_token);

            if(validate is null)
            {
                return Unauthorized();
            }
            var email = validate.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);

            var jwt = await _authService.GenerateJWT(user);
            var refreshToken = _authService.GenerateRefreshToken(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            return Ok(new LoginResponse(jwt, userRoles, user.CreatedOn, user.Id.ToString(), user.Email, user.UserName, user.Nick));
        }


        private void SetCookie(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();
            option.HttpOnly = true;
            option.Path = "/";

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            Response.Cookies.Append(key, value, option);
        }
        
    }
    public record LoginModel(string Email, string Password);
    public record LoginResponse(string JWT, IList<string> Roles, DateTime CreationTime, string UserId, string Email, string Username, string Nick);
    public record CreateUserWithPhoto()
    {
        public CreateUser createUser { get; set; }
        public IFormFile MainPhoto { get; set; }
        public IFormFile BgPhoto { get; set; }
    }
}
