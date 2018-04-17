using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SearchUser.Entities.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchUser.Api.Persistence
{
    /// <summary>
    /// Application Database Context class
    /// </summary>
    public class SearchUserDbContext : IdentityDbContext<ApplicationUser>
    {
        public SearchUserDbContext(DbContextOptions<SearchUserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Fill fields CreateOn and LastUpdateOn
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var AddedEntities = ChangeTracker.Entries().Where(E => E.State == EntityState.Added).ToList();
            AddedEntities.ForEach(e =>
            {
                e.Property("CreatedOn").CurrentValue = DateTime.Now;
            });

            var EditedEntities = ChangeTracker.Entries().Where(E => E.State == EntityState.Modified).ToList();
            EditedEntities.ForEach(e =>
            {
                e.Property("LastUpdatedOn").CurrentValue = DateTime.Now;
            });

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
