using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using SearchUser.Entities.Models;
using System.Collections.ObjectModel;
using SearchUser.Api.Persistence;
using Microsoft.AspNetCore.Identity;

namespace SearchUser.Api.Initializer
{
    /// <summary>
    /// Database Initializer for development and test data
    /// </summary>
    public class DatabaseInitializer
    {
        /// <summary>
        /// Create data and ensure migrations
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider</param>
        /// <returns>Task threading</returns>
        public async Task InitializeDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService<SearchUserDbContext>();
            context.Database.EnsureCreated();

            // Users
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            // Data initialize
            var telephones = new Collection<Telephone>
            {
                new Telephone { Number = "+353834209690" },
                new Telephone { Number = "+353834211002" },
                new Telephone { Number = "+5585988861982" }
            };
            var applicationUser = new ApplicationUser { Id = "79bfe381-050d-4cd4-9cd7-64b3a68d8faf", Name = "Matheus", UserName = "matheusmaximo@gmail.com", Email = "matheusmaximo@gmail.com", Telephones = telephones };
            await userManager.CreateAsync(applicationUser, "Passw0rd!");

            await context.SaveChangesAsync();
        }
    }
}
