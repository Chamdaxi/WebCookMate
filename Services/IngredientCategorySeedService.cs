using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;

namespace demo.Services
{
    public class IngredientCategorySeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IngredientCategorySeedService> _logger;

        public IngredientCategorySeedService(ApplicationDbContext context, ILogger<IngredientCategorySeedService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedDefaultCategories()
        {
            try
            {
                // Check if categories already exist
                var existingCount = await _context.IngredientCategories.CountAsync();
                if (existingCount > 0)
                {
                    _logger.LogInformation($"Ingredient categories already seeded ({existingCount} categories)");
                    return;
                }

                var categories = new List<IngredientCategory>
                {
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Rau Củ",
                        Description = "Các loại rau củ tươi",
                        Icon = "fa-carrot",
                        Color = "#10b981",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Thịt & Hải Sản",
                        Description = "Thịt các loại và hải sản",
                        Icon = "fa-fish",
                        Color = "#ef4444",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Trái Cây",
                        Description = "Các loại trái cây tươi",
                        Icon = "fa-apple-alt",
                        Color = "#f59e0b",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Gia Vị",
                        Description = "Gia vị và nước sốt",
                        Icon = "fa-pepper-hot",
                        Color = "#dc2626",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Sữa & Trứng",
                        Description = "Các sản phẩm từ sữa và trứng",
                        Icon = "fa-egg",
                        Color = "#fbbf24",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Ngũ Cốc",
                        Description = "Gạo, bột mì, bột các loại",
                        Icon = "fa-bread-slice",
                        Color = "#d97706",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Đồ Uống",
                        Description = "Nước ngọt, nước ép, rượu bia",
                        Icon = "fa-glass-water",
                        Color = "#3b82f6",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Đồ Đông Lạnh",
                        Description = "Thực phẩm đông lạnh",
                        Icon = "fa-snowflake",
                        Color = "#06b6d4",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Đồ Khô",
                        Description = "Đậu, hạt, mì gói",
                        Icon = "fa-box",
                        Color = "#8b5cf6",
                        CreatedAt = DateTime.Now
                    },
                    new IngredientCategory
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Khác",
                        Description = "Nguyên liệu khác",
                        Icon = "fa-ellipsis-h",
                        Color = "#6b7280",
                        CreatedAt = DateTime.Now
                    }
                };

                await _context.IngredientCategories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Đã tạo {categories.Count} danh mục nguyên liệu mẫu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi seed danh mục nguyên liệu");
            }
        }
    }
}


