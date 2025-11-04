using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo.Models
{
    public class Ingredient
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // UserId is optional - if null, ingredient is shared/global
        public string? UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty; // kg, g, ml, l, pieces, etc.

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual IngredientCategory? Category { get; set; }
    }
}

