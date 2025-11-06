using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using demo.Models;

namespace demo.Data
{
    // Note: ApplicationDbContext is not used in current setup (no local database)
    // Keeping for compatibility with old code that may reference it
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // These DbSets are not used - all data comes from external API
        // public DbSet<Favorite> Favorites { get; set; }
        // public DbSet<IngredientCategory> IngredientCategories { get; set; }
        // public DbSet<Ingredient> Ingredients { get; set; }
    }
}
