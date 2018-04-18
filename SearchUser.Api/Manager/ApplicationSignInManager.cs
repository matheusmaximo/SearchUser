using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SearchUser.Entities.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SearchUser.Api.Manager
{
    /// <summary>
    /// Custom application signin manager class
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Custom ApplicationSignInManager constructor
        /// </summary>
        public ApplicationSignInManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes, IConfiguration configuration)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
            this.contextAccessor = contextAccessor;
            this.configuration = configuration;
        }

        /// <summary>
        /// Get user Id from current Jwt token logged user
        /// </summary>
        /// <returns>Logged in user Id</returns>
        public string GetCurrentUser()
        {
            return contextAccessor.CurrentUser();
        }

        /// <summary>
        /// Validate user session
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>True when sesion was created less than {configuration:Jwt:ExpireMinutes} ago</returns>
        public bool SessionIsValid(ApplicationUser user)
        {
            return (user.LastLoginOn.HasValue && (user.LastLoginOn.Value.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"])).Ticks > DateTime.Now.Ticks));
        }

        /// <summary>
        /// Extension method for Identity Core SignInManager SignInAsync method
        /// </summary>
        public override async Task SignInAsync(ApplicationUser user, bool isPersistent, string authenticationMethod = null)
        {
            await base.SignInAsync(user, isPersistent, authenticationMethod);

            // Register sign in
            user.LastLoginOn = DateTime.Now;
            await this.UserManager.UpdateAsync(user);

            // Request token
            user.Token = GenerateJwtToken(user.Email, user);
        }

        /// <summary>
        /// Creates a Jwt Token
        /// </summary>
        /// <param name="email">Email connecting</param>
        /// <param name="user">User model connecting</param>
        /// <returns>User token</returns>
        private string GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.Ticks.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Issuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
