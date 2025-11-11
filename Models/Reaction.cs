using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class Reaction
    {
        public int Id { get; set; }
        
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        [Required]
        public string Type { get; set; } = string.Empty; // like, love, wow, haha, sad, angry
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}


