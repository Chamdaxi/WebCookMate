using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;

namespace demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SeedController> _logger;

        public SeedController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SeedController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("favorites")]
        public async Task<IActionResult> SeedFavorites()
        {
            try
            {
                // Lấy user hiện tại (hoặc user đầu tiên trong database)
                var userId = User?.Identity?.Name;
                ApplicationUser? user = null;

                if (!string.IsNullOrEmpty(userId))
                {
                    user = await _userManager.FindByNameAsync(userId);
                }

                // Nếu không tìm thấy user đang đăng nhập, lấy user đầu tiên
                if (user == null)
                {
                    user = await _context.Users.FirstOrDefaultAsync();
                }

                if (user == null)
                {
                    return BadRequest(new { message = "Không tìm thấy user nào trong hệ thống!" });
                }

                // Xóa các favorites cũ của user này (nếu có)
                var existingFavorites = await _context.Favorites
                    .Where(f => f.UserId == user.Id)
                    .ToListAsync();
                
                if (existingFavorites.Any())
                {
                    _context.Favorites.RemoveRange(existingFavorites);
                    await _context.SaveChangesAsync();
                }

                // Tạo danh sách món ăn yêu thích mẫu
                var sampleFavorites = new List<Favorite>
                {
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "1",
                        RecipeName = "Phở Bò",
                        RecipeDescription = "Món phở bò truyền thống Việt Nam",
                        RecipeCategory = "lunch",
                        RecipeImage = "/images/recipes/phobo.jpeg",
                        CookingTime = 45,
                        Difficulty = "medium",
                        Rating = 4.8,
                        AddedAt = DateTime.Now.AddDays(-7)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "2",
                        RecipeName = "Bánh Mì",
                        RecipeDescription = "Bánh mì thịt nướng đặc sản Sài Gòn",
                        RecipeCategory = "breakfast",
                        RecipeImage = "/images/recipes/banhmi.jpg",
                        CookingTime = 15,
                        Difficulty = "easy",
                        Rating = 4.6,
                        AddedAt = DateTime.Now.AddDays(-6)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "3",
                        RecipeName = "Kho Quẹt",
                        RecipeDescription = "Kho quẹt miền Tây với thịt và trứng",
                        RecipeCategory = "lunch",
                        RecipeImage = "/images/recipes/Khoquet.webp",
                        CookingTime = 25,
                        Difficulty = "easy",
                        Rating = 4.5,
                        AddedAt = DateTime.Now.AddDays(-5)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "4",
                        RecipeName = "Cơm Tấm",
                        RecipeDescription = "Cơm tấm sườn nướng đặc sản miền Nam",
                        RecipeCategory = "lunch",
                        RecipeImage = "/images/recipes/phobo.jpeg",
                        CookingTime = 30,
                        Difficulty = "medium",
                        Rating = 4.5,
                        AddedAt = DateTime.Now.AddDays(-4)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "5",
                        RecipeName = "Bún Bò Huế",
                        RecipeDescription = "Bún bò Huế cay nồng đặc sản xứ Huế",
                        RecipeCategory = "lunch",
                        RecipeImage = "/images/recipes/banhmi.jpg",
                        CookingTime = 120,
                        Difficulty = "hard",
                        Rating = 4.9,
                        AddedAt = DateTime.Now.AddDays(-3)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "6",
                        RecipeName = "Pizza Margherita",
                        RecipeDescription = "Pizza Ý cổ điển với cà chua và mozzarella",
                        RecipeCategory = "dinner",
                        RecipeImage = "/images/recipes/Khoquet.webp",
                        CookingTime = 40,
                        Difficulty = "medium",
                        Rating = 4.4,
                        AddedAt = DateTime.Now.AddDays(-2)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "10",
                        RecipeName = "Gỏi Cuốn",
                        RecipeDescription = "Gỏi cuốn tôm thịt tươi mát",
                        RecipeCategory = "snack",
                        RecipeImage = "/images/recipes/phobo.jpeg",
                        CookingTime = 20,
                        Difficulty = "easy",
                        Rating = 4.5,
                        AddedAt = DateTime.Now.AddDays(-1)
                    },
                    new Favorite
                    {
                        UserId = user.Id,
                        RecipeId = "11",
                        RecipeName = "Tiramisu",
                        RecipeDescription = "Tiramisu Ý với cà phê và mascarpone",
                        RecipeCategory = "dessert",
                        RecipeImage = "/images/recipes/banhmi.jpg",
                        CookingTime = 50,
                        Difficulty = "medium",
                        Rating = 4.8,
                        AddedAt = DateTime.Now
                    }
                };

                // Thêm vào database
                await _context.Favorites.AddRangeAsync(sampleFavorites);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Đã thêm {sampleFavorites.Count} món ăn yêu thích cho user {user.Email}");

                return Ok(new
                {
                    message = $"Đã thêm thành công {sampleFavorites.Count} món ăn yêu thích!",
                    userId = user.Id,
                    userEmail = user.Email,
                    count = sampleFavorites.Count,
                    favorites = sampleFavorites.Select(f => new
                    {
                        name = f.RecipeName,
                        category = f.RecipeCategory,
                        rating = f.Rating
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi seed favorites");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

