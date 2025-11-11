using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}


