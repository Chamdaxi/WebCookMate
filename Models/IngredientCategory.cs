using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class IngredientCategory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; } // Font Awesome icon class

        [MaxLength(20)]
        public string? Color { get; set; } // Hex color code

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}


