using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo.Models
{
    public class Favorite
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public string RecipeId { get; set; } // ID of the favorited recipe

        [Required]
        public string RecipeName { get; set; }

        public string RecipeDescription { get; set; } = string.Empty;

        public string RecipeCategory { get; set; } = string.Empty;

        public string RecipeImage { get; set; } = string.Empty;

        public int CookingTime { get; set; } // in minutes

        public string Difficulty { get; set; } = string.Empty; // easy, medium, hard

        public double Rating { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}

