using DataCommon.Models.Documents;
using DataService.Services.Interfaces.Documents;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthService(IConfiguration config,UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }
        public async Task<string> GenerateJWT(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var jwtHandler = new JwtSecurityTokenHandler();
            var duration = Double.Parse(_config["Jwt:ExpJWTMin"]);
            var claims = await getClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(duration),
                SigningCredentials = credentials
            };
            var token = jwtHandler.CreateToken(tokenDescriptor);
            var jwt = jwtHandler.WriteToken(token);

            return jwt;
        }
        public string GenerateRefreshToken(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:RefreshKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var jwtHandler = new JwtSecurityTokenHandler();
            var duration = Double.Parse(_config["Jwt:ExpRefreshTokenHours"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddHours(duration),
                SigningCredentials = credentials
            };
            var token = jwtHandler.CreateToken(tokenDescriptor);
            var refreshToken = jwtHandler.WriteToken(token);
            return refreshToken;

        }
        public ClaimsPrincipal ValidRefreshToken(string refreshToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            try
            {
                ClaimsPrincipal validate = jwtHandler.ValidateToken(refreshToken, GetRefreshTokenValidationParameters(), out securityToken);
                return validate;
            }
            catch
            {
                return null;
            }

        }
        private async Task<List<Claim>> getClaims(ApplicationUser user)
        {
            var roles = await getRoles(user);
            List<Claim> claims = new List<Claim>{
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            roles.ForEach(r => claims.Add(new Claim(ClaimTypes.Role, r)));
            return claims;
        }
        private async Task<List<string>> getRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }
        private TokenValidationParameters GetRefreshTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:RefreshKey"])),
                ClockSkew = TimeSpan.FromMinutes(10)// 10 minut tolerancji
            };
        }

    }
}
