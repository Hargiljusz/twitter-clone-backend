using DataCommon.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services.Interfaces.Documents
{
    public interface IAuthService
    {
        Task<string> GenerateJWT(ApplicationUser user);
        string GenerateRefreshToken(ApplicationUser user);
        ClaimsPrincipal ValidRefreshToken(string refreshToken);
    }
}
