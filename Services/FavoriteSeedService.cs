using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;

namespace demo.Services
{
    public class FavoriteSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoriteSeedService> _logger;

        public FavoriteSeedService(ApplicationDbContext context, ILogger<FavoriteSeedService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tự động thêm món ăn yêu thích mẫu nếu user chưa có món nào
        /// </summary>
        public async Task<bool> EnsureUserHasFavorites(string userId)
        {
            try
            {
                // Kiểm tra xem user đã có món ăn yêu thích chưa
                var existingFavorites = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .CountAsync();

                if (existingFavorites > 0)
                {
                    // User đã có món ăn yêu thích, không cần seed
                    return false;
                }

                // Thêm 8 món ăn mẫu
                var sampleFavorites = new List<Favorite>
                {
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "1",
                        RecipeName = "Phở Bò",
                        RecipeDescription = "Món phở truyền thống Việt Nam",
                        RecipeCategory = "lunch",
                        RecipeImage = "",
                        CookingTime = 60,
                        Difficulty = "medium",
                        Rating = 4.8,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "2",
                        RecipeName = "Bánh Mì",
                        RecipeDescription = "Bánh mì Việt Nam với nhân đa dạng",
                        RecipeCategory = "breakfast",
                        RecipeImage = "",
                        CookingTime = 20,
                        Difficulty = "easy",
                        Rating = 4.6,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "1",
                        RecipeName = "Kho Quẹt",
                        RecipeDescription = "Mắm kho quẹt đậm đà hương vị",
                        RecipeCategory = "lunch",
                        RecipeImage = "",
                        CookingTime = 45,
                        Difficulty = "medium",
                        Rating = 4.5,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "2",
                        RecipeName = "Cơm Tấm",
                        RecipeDescription = "Cơm tấm sườn bì chả",
                        RecipeCategory = "lunch",
                        RecipeImage = "",
                        CookingTime = 40,
                        Difficulty = "easy",
                        Rating = 4.5,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "1",
                        RecipeName = "Bún Bò Huế",
                        RecipeDescription = "Bún bò Huế cay nồng đặc trưng miền Trung",
                        RecipeCategory = "dinner",
                        RecipeImage = "",
                        CookingTime = 90,
                        Difficulty = "medium",
                        Rating = 4.9,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "1",
                        RecipeName = "Pizza Margherita",
                        RecipeDescription = "Pizza cổ điển với phô mai mozzarella",
                        RecipeCategory = "lunch",
                        RecipeImage = "",
                        CookingTime = 30,
                        Difficulty = "easy",
                        Rating = 4.4,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "1",
                        RecipeName = "Gỏi Cuốn",
                        RecipeDescription = "Gỏi cuốn tươi mát với rau thơm",
                        RecipeCategory = "appetizer",
                        RecipeImage = "",
                        CookingTime = 20,
                        Difficulty = "easy",
                        Rating = 4.5,
                        AddedAt = DateTime.UtcNow
                    },
                    new Favorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        RecipeId = "1",
                        RecipeName = "Tiramisu",
                        RecipeDescription = "Tiramisu ngọt ngào phong cách Ý",
                        RecipeCategory = "lunch",
                        RecipeImage = "",
                        CookingTime = 45,
                        Difficulty = "medium",
                        Rating = 4.8,
                        AddedAt = DateTime.UtcNow
                    }
                };

                await _context.Favorites.AddRangeAsync(sampleFavorites);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Đã tự động thêm 8 món ăn yêu thích mẫu cho user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Lỗi khi seed món ăn yêu thích cho user {userId}");
                return false;
            }
        }
    }
}

