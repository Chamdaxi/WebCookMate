using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using demo.Models;

namespace demo.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ShoppingItem> ShoppingItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Favorite - unique constraint on UserId and RecipeId
            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.RecipeId })
                .IsUnique();
            
            // Reaction - unique constraint on UserId, RecipeId, and Type
            builder.Entity<Reaction>()
                .HasIndex(r => new { r.UserId, r.RecipeId, r.Type })
                .IsUnique();
            
            // Comment relationships
            builder.Entity<Comment>()
                .HasOne(c => c.Recipe)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Favorite relationships
            builder.Entity<Favorite>()
                .HasOne(f => f.Recipe)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Reaction relationships
            builder.Entity<Reaction>()
                .HasOne(r => r.Recipe)
                .WithMany(rec => rec.Reactions)
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Recipe relationships
            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
