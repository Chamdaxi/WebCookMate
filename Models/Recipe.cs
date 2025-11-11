using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? Ingredients { get; set; } // JSON string of ingredients array
        
        public string? Category { get; set; }
        
        public int? CookingTime { get; set; } // in minutes
        
        public string? Difficulty { get; set; } // Easy, Medium, Hard
        
        public string? UserId { get; set; } // User who posted the recipe
        public ApplicationUser? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}

