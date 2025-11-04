using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class ShoppingItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Category { get; set; }
        
        public string? Notes { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        public string? Unit { get; set; } // kg, g, l, ml, pcs, etc.
        
        public decimal? Price { get; set; } // Giá tiền
        
        public bool IsCompleted { get; set; } = false;
        
        public bool InCart { get; set; } = false; // Đã thêm vào giỏ hàng
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign key to User
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}

