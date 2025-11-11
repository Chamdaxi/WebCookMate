using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}


