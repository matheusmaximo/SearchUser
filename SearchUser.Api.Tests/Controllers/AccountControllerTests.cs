using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SearchUser.Api.Controllers;
using SearchUser.Api.Manager;
using SearchUser.Api.Persistence;
using SearchUser.Entities.Models;
using SearchUser.Entities.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace SearchUser.Api.Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task TestSignIn()
        {
            IMapper mapper = null;
            ILogger<AccountController> logger = null;
            var signInManager = GetMockSignInManager();

            // Arrange
            var controller = new AccountController(mapper, signInManager.Object, logger);

            // Act
            var loginDto = new LoginViewModel { Email = "matheusmaximo@gmail.com", Password = "Passw0rd!" };
            IActionResult actionResult = await controller.Signin(loginDto);

            // Assert
            Assert.NotNull(actionResult);
            CreatedResult result = actionResult as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            Assert.NotNull(result.Value);
            Assert.IsType<SignedInUserViewModel>(result.Value);

            SignedInUserViewModel userViewModel = result.Value as SignedInUserViewModel;
            Assert.NotNull(userViewModel.Token);
        }

        [Fact]
        public async Task TestSignUp()
        {
            IMapper mapper = null;
            var signInManager = GetMockSignInManager();
            ILogger<AccountController> logger = null;

            // Arrange
            var controller = new AccountController(mapper, signInManager.Object, logger);

            // Act
            var signupDto = new UserViewModel
            {
                Name = "Matheus",
                Email = "matheusmaximo@gmail.com",
                Password = "Passw0rd!",
                Telephones = new Collection<TelephoneViewModel>
                {
                    new TelephoneViewModel{ Number = "+5585988861982"},
                    new TelephoneViewModel{ Number = "+353834209690"},
                }
            };
            IActionResult actionResult = await controller.Signup(signupDto);

            // Assert
            Assert.NotNull(actionResult);
            CreatedResult result = actionResult as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            Assert.NotNull(result.Value);
            Assert.IsType<SignedInUserViewModel>(result.Value);

            SignedInUserViewModel userViewModel = result.Value as SignedInUserViewModel;
            Assert.NotNull(userViewModel.Id);
            Assert.NotNull(userViewModel.Token);
        }

        [Fact]
        public void TestFindUser()
        {
            IMapper mapper = null;
            var signInManager = GetMockSignInManager();
            ILogger<AccountController> logger = null;

            // Arrange
            var controller = new AccountController(mapper, signInManager.Object, logger);

            // Act
            var id = "";
            IActionResult actionResult = controller.FindUser(id);

            // Assert
            Assert.NotNull(actionResult);
            CreatedResult result = actionResult as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            Assert.NotNull(result.Value);
            Assert.IsType<SignedInUserViewModel>(result.Value);

            SignedInUserViewModel userViewModel = result.Value as SignedInUserViewModel;
            Assert.NotNull(userViewModel.Token);
        }

        #region Private methods
        /// <summary>
        /// Creates a mock SignInManager class
        /// </summary>
        /// <returns>Mocked SignInManager object</returns>
        private Mock<ApplicationSignInManager> GetMockSignInManager()
        {
            var mockUsrMgr = GetMockUserManager();
            return new Mock<ApplicationSignInManager>(mockUsrMgr.Object);
        }

        /// <summary>
        /// Creates a mock for UserManager
        /// </summary>
        /// <returns>Mocked UserManager object</returns>
        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            return new Mock<UserManager<ApplicationUser>>();
        }
        #endregion
    }
}
