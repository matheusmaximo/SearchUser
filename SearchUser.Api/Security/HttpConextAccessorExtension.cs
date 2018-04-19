using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SearchUser.Api
{
    public static class IHttpContextExtension
    {
        /// <summary>
        /// Http Context extension to get user Id
        /// </summary>
        /// <param name="httpContext">Http context</param>
        /// <returns>Logged in user Id</returns>
        public static string CurrentUser(this HttpContext httpContext)
        {
            return httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
