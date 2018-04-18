using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using SearchUser.Entities.Models;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            // Data initialize
            // Users
            var telephones = new Collection<Telephone>
            {
                new Telephone { Number = "+353834209690" },
                new Telephone { Number = "+353834211002" },
                new Telephone { Number = "+5585988861982" }
            };
            var id = "79bfe381-050d-4cd4-9cd7-64b3a68d8faf";
            var applicationUser = new ApplicationUser { Id = id, Name = "Matheus", UserName = "matheusmaximo@gmail.com", Email = "matheusmaximo@gmail.com", Telephones = telephones };

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var existentUser = userManager.Users.Include(m => m.Telephones).FirstOrDefault(_user => _user.Id.Equals(id));
            if (existentUser != null)
            {
                await userManager.DeleteAsync(existentUser);
            }
            await userManager.CreateAsync(applicationUser, "Passw0rd!");
        }
    }
}
