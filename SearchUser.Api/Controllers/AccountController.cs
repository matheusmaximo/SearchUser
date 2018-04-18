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
using SearchUser.Api.Manager;
using SearchUser.Api.Persistence;
using SearchUser.Entities.Models;
using SearchUser.Entities.ViewModel;

namespace SearchUser.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[action]")]
    public class AccountController : Controller
    {
        private readonly IMapper mapper;
        private readonly ApplicationSignInManager signInManager;

        #region Public methods
        // GET api/finduser/id
        [Authorize]
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult FindUser(string id)
        {
            signInManager.Logger.LogDebug("GetUser.id = " + id);

            // Compare user id with given token user id
            if (id != signInManager.GetCurrentUser()) { return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Unauthorized" }); }

            // Try to find user by id
            var user = signInManager.UserManager.Users.Include(m => m.Telephones).FirstOrDefault(_user => _user.Id.Equals(id));
            if (user == null) { return NotFound(); }

            // Verify last login time
            if(!signInManager.SessionIsValid(user))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Invalid Session" });
            }

            // Return found user
            return new ObjectResult(mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
        }

        // POST api/signin
        [HttpPost]
        public async Task<ObjectResult> Signin([FromBody]LoginViewModel loginDto)
        {
            signInManager.Logger.LogDebug("SigninUser.email = " + loginDto.Email);

            // Try to find user with the given email
            var user = signInManager.UserManager.Users.SingleOrDefault(r => r.Email == loginDto.Email);
            if (user == null) { return NotFound(new { Message = "Invalid user and / or password" }); }
            
            // Check given password
            var result = await signInManager.PasswordSignInAsync(user.Email, loginDto.Password, false, false);

            // If not succeed, report error
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Invalid user and / or password" });
            }

            // Execute signin operations
            await signInManager.SignInAsync(user, false);

            // Return JSON object
            return new ObjectResult(mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
        }

        // POST api/signup
        [HttpPost]
        public async Task<ObjectResult> Signup([FromBody]UserViewModel userDto)
        {
            // Map from dto to entity
            var user = mapper.Map<UserViewModel, ApplicationUser>(userDto);

            // Try to create new user
            signInManager.Logger.LogDebug("SignupUser.email = " + user.Email);
            var result = await signInManager.UserManager.CreateAsync(user, userDto.Password);

            // Check if suceed
            if (result.Succeeded)
            {
                // Request sign in
                await signInManager.SignInAsync(user, false);

                // Return mapped object
                return new ObjectResult(mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
            }

            // Report errors
            return StatusCode(StatusCodes.Status400BadRequest, result.Errors.ToArray());
        }
        #endregion

        #region Constructor
        public AccountController(IMapper mapper, ApplicationSignInManager signInManager)
        {
            this.mapper = mapper;
            this.signInManager = signInManager;
        }
        #endregion
    }
}