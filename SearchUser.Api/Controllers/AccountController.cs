using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SearchUser.Api.Persistence;
using SearchUser.Entities.Models;
using SearchUser.Entities.ViewModel;

namespace SearchUser.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[action]")]
    public class AccountController : Controller
    {
        private readonly SearchUserDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        // GET api/finduser/id
        [Authorize]
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult FindUser(string id)
        {
            logger.LogDebug("GetUser.id = " + id);

            // Compare user id with given token user id
            if (id != httpContextAccessor.CurrentUser()) { return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Unauthorized" }); }

            // Try to find user by id
            var user = context.Users.Include(m => m.Telephones).FirstOrDefault(_user => _user.Id.Equals(id));
            if (user == null) { return NotFound(); }

            // Verify last login time
            if(!user.LastLoginOn.HasValue || (user.LastLoginOn.Value.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"])).Ticks < DateTime.Now.Ticks))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Invalid Session" });
            }

            // Return found user
            return new ObjectResult(mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
        }

        // POST api/signin
        [HttpPost]
        public async Task<object> Signin([FromBody]LoginViewModel loginDto)
        {
            logger.LogDebug("SigninUser.email = " + loginDto.Email);

            // Try to find user with the given email
            var user = userManager.Users.SingleOrDefault(r => r.Email == loginDto.Email);
            if (user == null) { return NotFound(new { Message = "Invalid user and / or password" }); }

            // Check given password
            var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, false, false);

            // If not succeed, report error
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Invalid user and / or password" });
            }

            // Execute signin operations
            await ExecuteSignIn(user);

            // Return JSON object
            return new ObjectResult(mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
        }

        // POST api/signup
        [HttpPost]
        public async Task<object> Signup([FromBody]UserViewModel userDto)
        {
            // Map from dto to entity
            var user = mapper.Map<UserViewModel, ApplicationUser>(userDto);

            // Try to create new user
            logger.LogDebug("SignupUser.id = " + user.Id + ", SignupUser.email = " + user.Email);
            var result = await userManager.CreateAsync(user, userDto.Password);

            // Check if suceed
            if (result.Succeeded)
            {
                await ExecuteSignIn(user);

                // Return mapped object
                return new ObjectResult(mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
            }

            // Report errors
            return StatusCode(StatusCodes.Status400BadRequest, result.Errors.ToArray());
        }

        #region Constructor
        public AccountController(SearchUserDbContext context, IMapper mapper, ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Execute signIn operations
        /// </summary>
        /// <param name="user">User to signin</param>
        /// <returns>The System.Threading.Tasks.Task that represents the asynchronous operation,
        /// containing the Microsoft.AspNetCore.Identity.IdentityResult of the operation.</returns>
        private async Task<ApplicationUser> ExecuteSignIn(ApplicationUser user)
        {
            // Request sign in
            await signInManager.SignInAsync(user, false);

            // Register sign in
            user.LastLoginOn = DateTime.Now;
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Request token
            user.Token = GenerateJwtToken(user.Email, user);

            return user;
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
        #endregion
    }
}