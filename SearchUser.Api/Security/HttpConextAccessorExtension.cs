using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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
