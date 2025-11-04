using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Category { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        public string? Unit { get; set; }
        
        public decimal? Price { get; set; }
        
        public bool IsPurchased { get; set; } = false;
        
        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        // Foreign key to User
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        // Link to ShoppingItem if added from shopping list
        public int? ShoppingItemId { get; set; }
        public ShoppingItem? ShoppingItem { get; set; }
    }
}

