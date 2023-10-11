using DataCommon.Models.Documents;
using DataService.DTO.Utils;
using DataService.Services.Interfaces.Documents;
using GraphQL.Errors.Exceptions;
using GraphQL.TypesUtils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GraphQL.Auth
{
    public class AuthMutations
    {
        private const string RefrestTokenName = "refreshToken";
        public async Task<LoginResponse> Login(
            [Service]IUserService userService, 
            [Service] UserManager<ApplicationUser> userManager, 
            [Service] IAuthService authService, 
            [Service] IConfiguration config,
            [Service] IHttpContextAccessor httpContextAccessor,
            LoginModel input )
        {
            var user = await userManager.FindByEmailAsync(input.Email);

            if (user == null)
            {
                throw new UserNotFoundException("User Not Found");
            }
            if (!user.EmailConfirmed)
            {
                throw new EmailNotConfirmedException("Email Not Confirmed");
            }

            var passwordCorrect = await userManager.CheckPasswordAsync(user, input.Password);
            if (!passwordCorrect)
            {
                throw new PasswordsInccoretException("Passwords Inccoret");
            }
            var jwt = await authService.GenerateJWT(user);
            var refreshToken = authService.GenerateRefreshToken(user);
            var userRoles = await userManager.GetRolesAsync(user);

            SetCookie(RefrestTokenName, refreshToken, Int32.Parse(config["Jwt:ExpRefreshTokenHours"]) * 60, httpContextAccessor.HttpContext);
            return  new LoginResponse(jwt, userRoles, user.CreatedOn, user.Id.ToString(), user.Email, user.UserName, user.Nick);
        }


        public async Task<ResultBool> Register(
            [Service] IUserService userService,
            [Service] UserManager<ApplicationUser> userManager,
            CreateUser user,
            IFile  mainPhoto,
            IFile bgPhoto)
        {
            var userExist = await userManager.FindByEmailAsync(user.Email);
            if (userExist is not null)
            {
                throw new UserExistException("User Exist");
            }

            var result = await userService.CreateUser(user,mainPhoto.OpenReadStream(),mainPhoto.Name,bgPhoto.OpenReadStream(),bgPhoto.Name);
            if (result)
            {
                //var user = await userManager.FindByEmailAsync(input.Email);
                //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return new ResultBool(true,DateTime.Now);
            }
            else
            {
                throw new RegisterErrorException("User creation failed! Please check user details and try again.");
            }
        }

        public ResultBool Logout([Service] IHttpContextAccessor ctx)
        {
            ctx.HttpContext!.Response.Cookies.Delete(RefrestTokenName);
            return new ResultBool(true,DateTime.Now);
        }

        public async Task<LoginResponse> RefreshToken(
            [Service] IHttpContextAccessor ctx,
            [Service] IAuthService authService,
            [Service] UserManager<ApplicationUser> userManager)
        {
            var Request = ctx.HttpContext!.Request;
            var refresh_token = Request.Cookies[RefrestTokenName];
            if (refresh_token == null)
            {
                throw new Exception("Cookie is not set.");
            }
            var validate = authService.ValidRefreshToken(refresh_token);

            if (validate is null)
            {
                throw new Exception();
            }
            var email = validate.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            var user = await userManager.FindByEmailAsync(email);

            var jwt = await authService.GenerateJWT(user);
            var refreshToken = authService.GenerateRefreshToken(user);
            var userRoles = await userManager.GetRolesAsync(user);
            return new LoginResponse(jwt, userRoles, user.CreatedOn, user.Id.ToString(), user.Email, user.UserName, user.Nick);
        }


        private static void SetCookie(string key, string value, int? expireTime, Microsoft.AspNetCore.Http.HttpContext? httpCtx)
        {
            CookieOptions option = new()
            {
                HttpOnly = true,
                Path = "/"
            };

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            httpCtx!.Response.Cookies.Append(key, value, option);
        }
    }
    public class AuthMutationsExtension : ObjectTypeExtension<AuthMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<AuthMutations> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor.Field(a => a.Login(default!,default!,default!,default!,default!,default!))
                .Description("Sing In");

            descriptor.Field(a => a.Register(default!, default!, default!, default!, default!))
                .UseMutationConvention()
                .Description("Sing Up");

            descriptor.Field(a => a.Logout(default!))
                .Description("Logout");

            descriptor.Field(a => a.RefreshToken(default!, default!, default!))
                .Description("Refresh Token");
        }
    }
    public record LoginModel(string Email, string Password);
    public record LoginResponse(string JWT, IList<string> Roles, DateTime CreationTime, string UserId, string Email, string Username, string Nick);

   
}
