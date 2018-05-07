using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchUser.Api.Manager;
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
        private readonly ILogger<AccountController> logger;

        #region Public methods
        /// <summary>
        /// Execute find user
        /// </summary>
        /// <param name="id">User id to find. Must match with signed in user</param>
        /// <returns>Returns [<see cref="IActionResult"/>] with httpStatusCode response and if success a [<see cref="SignedInUserViewModel"/>] json object</returns>
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult FindUser(string id)
        {
            logger.LogDebug("GetUser.id = " + id);

            // Compare user id with given token user id
            if (id != signInManager.GetCurrentUser()) { return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Unauthorized" }); }

            // Try to find user by id
            var user = signInManager.UserManager.Users.Include(m => m.Telephones).FirstOrDefault(_user => _user.Id.Equals(id));
            if (user == null) { return StatusCode(StatusCodes.Status404NotFound, null); }

            // Verify last login time
            if(!signInManager.SessionIsValid(user))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Invalid Session" });
            }

            // Return found user
            return StatusCode(StatusCodes.Status200OK, mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
        }

        /// <summary>
        /// Execute signin
        /// </summary>
        /// <param name="loginDto">[<see cref="LoginViewModel"/>]</param>
        /// <returns>Returns [<see cref="ObjectResult"/>] with httpStatusCode response and if success a [<see cref="SignedInUserViewModel"/>] json object</returns>
        [HttpPost]
        public async Task<ObjectResult> Signin([FromBody]LoginViewModel loginDto)
        {
            logger.LogDebug("SigninUser.email = " + loginDto.Email);

            // Try to find user with the given email
            var user = signInManager.UserManager.Users.SingleOrDefault(r => r.Email == loginDto.Email);
            if (user == null) { return StatusCode(StatusCodes.Status404NotFound, new { Message = "Invalid user and / or password" }); }
            
            // Check given password
            var result = await signInManager.PasswordSignInAsync(user.Email ?? string.Empty, loginDto.Password ?? string.Empty, false, false);

            // If not succeed, report error
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Invalid user and / or password" });
            }

            // Execute signin operations
            await signInManager.SignInAsync(user, false);

            // Return JSON object
            return StatusCode(StatusCodes.Status202Accepted, mapper.Map<ApplicationUser, SignedInUserViewModel>(user));
        }

        /// <summary>
        /// Execute signup
        /// </summary>
        /// <param name="loginDto">[<see cref="UserViewModel"/>]</param>
        /// <returns>Returns [<see cref="ObjectResult"/>] with httpStatusCode response and if success a [<see cref="SignedInUserViewModel"/>] json object</returns>
        [HttpPost]
        public async Task<ObjectResult> Signup([FromBody]UserViewModel userDto)
        {
            logger.LogDebug("SignupUser.email = " + userDto.Email);

            // Map from dto to entity
            var user = mapper.Map<UserViewModel, ApplicationUser>(userDto);

            // Try to create new user
            var result = await signInManager.UserManager.CreateAsync(user, userDto.Password);

            // Check if suceed
            if (!result.Succeeded)
            {
                // Report errors
                return StatusCode(StatusCodes.Status400BadRequest, result.Errors.ToArray());
            }

            // Request sign in
            await signInManager.SignInAsync(user, false);

            // Return mapped object
            var signedInUserDto = mapper.Map<ApplicationUser, SignedInUserViewModel>(user);
            return StatusCode(StatusCodes.Status201Created, signedInUserDto);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="mapper">[<see cref="IMapper"/>]</param>
        /// <param name="signInManager">[<see cref="ApplicationSignInManager"/>]</param>
        /// <param name="logger">[<see cref="ILogger"/>]</param>
        public AccountController(IMapper mapper, ApplicationSignInManager signInManager, ILogger<AccountController> logger)
        {
            this.mapper = mapper;
            this.signInManager = signInManager;
            this.logger = logger;
        }
        #endregion
    }
}