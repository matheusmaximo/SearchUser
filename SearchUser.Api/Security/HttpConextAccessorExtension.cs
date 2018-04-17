using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SearchUser.Api
{
    public static class IHttpContextAccessorExtension
    {
        /// <summary>
        /// Http Context extension to get user Id
        /// </summary>
        /// <param name="httpContextAccessor">Http context</param>
        /// <returns>Logged in user Id</returns>
        public static string CurrentUser(this IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
