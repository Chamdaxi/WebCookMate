using demo.Data;
using demo.Models;
using Microsoft.EntityFrameworkCore;

namespace demo.Services
{
    public class IngredientSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IngredientSeedService> _logger;

        public IngredientSeedService(ApplicationDbContext context, ILogger<IngredientSeedService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task EnsureGlobalSampleIngredients()
        {
            try
            {
                // Check if global sample ingredients already exist (UserId = null means global/shared)
                var existingCount = await _context.Ingredients.CountAsync(i => i.UserId == null);
                if (existingCount > 0)
                {
                    _logger.LogInformation($"Global sample ingredients already seeded ({existingCount} ingredients)");
                    return;
                }

                // Get categories
                var categories = await _context.IngredientCategories.ToDictionaryAsync(c => c.Name, c => c.Id);

                var sampleIngredients = new List<Ingredient>
                {
                    new Ingredient
                    {
                        UserId = null, // Global ingredient, visible to all users
                        Name = "Cà chua",
                        CategoryId = categories.GetValueOrDefault("Rau củ", categories.First().Value),
                        Quantity = 1.5m,
                        Unit = "kg",
                        ExpiryDate = DateTime.Now.AddDays(5),
                        Notes = "Tươi ngon",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Cá hồi",
                        CategoryId = categories.GetValueOrDefault("Thịt & Hải sản", categories.First().Value),
                        Quantity = 500,
                        Unit = "g",
                        ExpiryDate = DateTime.Now.AddDays(2),
                        Notes = "Bảo quản trong ngăn đá",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Sữa tươi",
                        CategoryId = categories.GetValueOrDefault("Sữa & Trứng", categories.First().Value),
                        Quantity = 2,
                        Unit = "lít",
                        ExpiryDate = DateTime.Now.AddDays(7),
                        Notes = "Vinamilk 100%",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Gạo ST25",
                        CategoryId = categories.GetValueOrDefault("Ngũ cốc", categories.First().Value),
                        Quantity = 5,
                        Unit = "kg",
                        ExpiryDate = DateTime.Now.AddDays(180),
                        Notes = "Gạo thơm ngon nhất thế giới",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Táo Fuji",
                        CategoryId = categories.GetValueOrDefault("Trái cây", categories.First().Value),
                        Quantity = 10,
                        Unit = "quả",
                        ExpiryDate = DateTime.Now.AddDays(10),
                        Notes = "Nhập khẩu Nhật Bản",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Dầu ô liu",
                        CategoryId = categories.GetValueOrDefault("Gia vị", categories.First().Value),
                        Quantity = 500,
                        Unit = "ml",
                        ExpiryDate = DateTime.Now.AddDays(365),
                        Notes = "Extra virgin",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Bánh mì",
                        CategoryId = categories.GetValueOrDefault("Đồ khô", categories.First().Value),
                        Quantity = 2,
                        Unit = "ổ",
                        ExpiryDate = DateTime.Now.AddDays(3),
                        Notes = "Bánh mì Sài Gòn",
                        CreatedAt = DateTime.Now
                    },
                    new Ingredient
                    {
                        UserId = null,
                        Name = "Trứng gà",
                        CategoryId = categories.GetValueOrDefault("Sữa & Trứng", categories.First().Value),
                        Quantity = 12,
                        Unit = "quả",
                        ExpiryDate = DateTime.Now.AddDays(14),
                        Notes = "Trứng gà ta sạch",
                        CreatedAt = DateTime.Now
                    }
                };

                await _context.Ingredients.AddRangeAsync(sampleIngredients);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully seeded {sampleIngredients.Count} global sample ingredients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error seeding global sample ingredients");
            }
        }
    }
}

