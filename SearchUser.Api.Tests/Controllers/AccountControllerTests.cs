using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SearchUser.Api.Controllers;
using SearchUser.Api.Manager;
using SearchUser.Api.Mapping;
using SearchUser.Api.Persistence;
using SearchUser.Entities.Models;
using SearchUser.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SearchUser.Api.Tests.Controllers
{
    public class AccountControllerTests
    {
        public SearchUserDbContext Context { get; }
        public ServiceProvider ServiceProvider { get; }
        public IConfiguration Configuration { get; }
        public IMapper Mapper { get; }

        // Test data
        private static readonly string testUserId = "79bfe381-050d-4cd4-9cd7-64b3a68d8faf";
        private static readonly string testUserName = "Matheus";
        private static readonly string testUserEmail = "matheusmaximo@gmail.com";
        private static readonly string testUserPassword = "Passw0rd!";

        [Theory]
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "matheusmaximo@gmail.com", "Passw0rd!", StatusCodes.Status202Accepted)] // All correct
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "matheusmaximoERROR@gmail.com", "Passw0rd!", StatusCodes.Status404NotFound)] // Email wrong
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "", "Passw0rd!", StatusCodes.Status404NotFound)] // Email empty
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", null, "Passw0rd!", StatusCodes.Status404NotFound)] // Email null
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "matheusmaximo@gmail.com", "Passw0rd", StatusCodes.Status401Unauthorized)] // Password wrong
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "matheusmaximo@gmail.com", "", StatusCodes.Status401Unauthorized)] // Password empty
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "matheusmaximo@gmail.com", null, StatusCodes.Status401Unauthorized)] // Password null
        public async Task TestSignIn(string userId = null, string userEmail = null, string userPassword = null, int statusCodeExpected = StatusCodes.Status200OK)
        {
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.Ticks.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Sub, userEmail ?? testUserEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = this.ServiceProvider;

            var logger = this.ServiceProvider.GetRequiredService<ILogger<AccountController>>();
            var applicationSignInManager = this.ServiceProvider.GetRequiredService<ApplicationSignInManager>();
            applicationSignInManager.Context = httpContext;
            var controller = new AccountController(this.Mapper, applicationSignInManager, logger);
            controller.ControllerContext.HttpContext = httpContext;

            // Act
            var loginDto = new LoginViewModel { Email = userEmail, Password = userPassword };
            ObjectResult result = await controller.Signin(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(statusCodeExpected, result.StatusCode);

            if (statusCodeExpected == StatusCodes.Status202Accepted)
            {
                Assert.NotNull(result.Value);
                Assert.IsType<SignedInUserViewModel>(result.Value);

                SignedInUserViewModel userViewModel = result.Value as SignedInUserViewModel;
                Assert.NotNull(userViewModel.Id);
                Assert.NotNull(userViewModel.Token);
            }
        }

        [Theory]
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8faf", "79bfe381-050d-4cd4-9cd7-64b3a68d8faf", StatusCodes.Status200OK)] // All correct
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8fa", "79bfe381-050d-4cd4-9cd7-64b3a68d8faf", StatusCodes.Status401Unauthorized)] // Different id
        [InlineData("79bfe381-050d-4cd4-9cd7-64b3a68d8fa", "79bfe381-050d-4cd4-9cd7-64b3a68d8fa", StatusCodes.Status404NotFound)] // Wrong id
        public void TestFindUser(string userId = null, string jwtUserId = null, int statusCodeExpected = StatusCodes.Status200OK)
        {
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.Ticks.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Sub, testUserEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, jwtUserId)
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = this.ServiceProvider;

            var logger = this.ServiceProvider.GetRequiredService<ILogger<AccountController>>();
            var applicationSignInManager = this.ServiceProvider.GetRequiredService<ApplicationSignInManager>();
            applicationSignInManager.Context = httpContext;

            var controller = new AccountController(this.Mapper, applicationSignInManager, logger);
            controller.ControllerContext.HttpContext = httpContext;

            // Act
            IActionResult actionResult = controller.FindUser(userId);

            Assert.NotNull(actionResult);
            Assert.IsType<ObjectResult>(actionResult);

            var objectResult = actionResult as ObjectResult;
            Assert.Equal(statusCodeExpected, objectResult.StatusCode);

            switch (statusCodeExpected)
            {
                case StatusCodes.Status200OK:
                    Assert.NotNull(objectResult.Value);
                    Assert.IsType<SignedInUserViewModel>(objectResult.Value);

                    SignedInUserViewModel userViewModel = objectResult.Value as SignedInUserViewModel;
                    Assert.NotNull(userViewModel.Id);
                    Assert.Equal(userId, userViewModel.Id);
                    break;
                case StatusCodes.Status401Unauthorized:
                    Assert.NotNull(objectResult.Value);

                    dynamic value401 = objectResult.Value;
                    var message = value401?.GetType().GetProperty("Message")?.GetValue(value401, null);
                    Assert.Equal("Unauthorized", message);
                    break;
                case StatusCodes.Status404NotFound:
                    Assert.Null(objectResult.Value);
                    break;
            }
        }

        [Fact]
        public async Task TestSignUp()
        {
            var logger = this.ServiceProvider.GetRequiredService<ILogger<AccountController>>();
            var applicationSignInManager = this.ServiceProvider.GetRequiredService<ApplicationSignInManager>();
            var controller = new AccountController(this.Mapper, applicationSignInManager, logger);

            var userDto = new UserViewModel { Name = "Test", Email = "Test", Password = "Test", Telephones = {
                    new TelephoneViewModel{ Number = "123456789" }
                } };
            ObjectResult result = await controller.Signup(userDto);
        }

        #region Private methods
        /// <summary>
        /// Create a user for tests
        /// </summary>
        private async void CreateTestUser()
        {
            // Arrange
            var telephones = new Collection<Telephone>
            {
                new Telephone { Number = "+353834209690" },
                new Telephone { Number = "+353834211002" },
                new Telephone { Number = "+5585988861982" }
            };
            var user = new ApplicationUser { Id = testUserId, Name = testUserName, UserName = testUserEmail, Email = testUserEmail, Telephones = telephones, LastLoginOn = DateTime.Now };
            var userManager = this.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var userManagerResult = await userManager.CreateAsync(user, testUserPassword);
            Assert.True(userManagerResult.Succeeded);
        }
        #endregion

        #region Constructor
        public AccountControllerTests()
        {
            var services = new ServiceCollection();

            // Configuration
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Jwt:Key", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjYzNjU5NjU5OTM1NDA1MDM2Niwic3ViIjoibWF0aGV1c21heGltb0BnbWFpbC5jb20iLCJqdGkiOiI2ZWNmNzg2Yy05NjliLTQzMmEtODViYi04NzE5OTcxZDY5YTQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijc5YmZlMzgxLTA1MGQtNGNkNC05Y2Q3LTY0YjNhNjhkOGZhZiIsImV4cCI6MTUyNDA2MTMzNSwiaXNzIjoiaHR0cHM6Ly9naXRodWIuY29tL21hdGhldXNtYXhpbW8iLCJhdWQiOiJodHRwczovL2dpdGh1Yi5jb20vbWF0aGV1c21heGltbyJ9.T_ZB-ECHKyhR77PjT7-Uioh6EsKX_apVsRjV51nnADg" },
                { "Jwt:ExpireMinutes", "1" },
                { "Jwt:Issuer", "http://githubtest.com/matheusmaximo" }
            });
            services.AddSingleton<IConfiguration>(configurationBuilder.Build());

            // DbContext
            services.AddDbContext<SearchUserDbContext>(options => options.UseInMemoryDatabase("SearchUserTest"));

            // Identity
            services.AddMvc();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<SearchUserDbContext>()
                .AddSignInManager<ApplicationSignInManager>()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options => { options.User.RequireUniqueEmail = true; });

            // Jwt
            services.AddApplicationSecurity(this.Configuration);

            // Create HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Load properties
            this.ServiceProvider = services.BuildServiceProvider();
            this.Configuration = this.ServiceProvider.GetRequiredService<IConfiguration>();
            this.Context = this.ServiceProvider.GetRequiredService<SearchUserDbContext>();

            var config = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
            this.Mapper = config.CreateMapper();

            CreateTestUser();
        }
        #endregion
    }
}
