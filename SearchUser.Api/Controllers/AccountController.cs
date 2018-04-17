using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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

        // GET api/finduser/id
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult FindUser(string id)
        {
            logger.LogDebug("GetUser.id = " + id);
            var user = context.Users.Include(m => m.Telephones).FirstOrDefault(_user => _user.Id.Equals(id));
            if (user == null) { return NotFound(); }

            return new ObjectResult(mapper.Map<ApplicationUser, UserViewModel>(user));
        }

        // POST api/signin
        [HttpPost]
        public void Signin([FromBody]LoginViewModel loginDto)
        {
            logger.LogDebug("SigninUser.email = " + loginDto.Email);
        }

        // POST api/signup
        [HttpPost]
        public async Task<object> Signup([FromBody]UserViewModel userDto)
        {
            var user = mapper.Map<UserViewModel, ApplicationUser>(userDto);

            logger.LogDebug("SignupUser.id = " + user.Id + ", SignupUser.email = " + user.Email);
            user.CreatedOn = DateTime.Now;
            user.LastUpdatedOn = DateTime.Now;
            var result = await userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, false);
                user.Token = GenerateJwtToken(user.Email, user);
                return user;
            }

            return StatusCode(StatusCodes.Status400BadRequest, result.Errors.ToArray());
        }

        #region Constructor
        public AccountController(SearchUserDbContext context, IMapper mapper, ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }
        #endregion

        #region Private methods
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