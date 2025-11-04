using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using demo.Models;
using demo.Data;

namespace demo.Controllers
{
    // [Authorize] - Tạm thời comment để test
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Chat()
        {
            return View();
        }

        public IActionResult Recipes()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult FavoriteList()
        {
            return View();
        }

        public async Task<IActionResult> ShoppingList()
        {
            var userId = _userManager.GetUserId(User);
            var items = await _context.ShoppingItems
                .Where(i => userId == null || i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            
            ViewBag.ShoppingItems = items;
            return View();
        }

        // Search Recipes API
        [HttpPost]
        public IActionResult SearchRecipes(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
            }

            // Mock recipes data - trong thực tế sẽ lấy từ database hoặc API
            var recipes = new List<Recipe>
            {
                new Recipe { Id = 1, Name = "Phở Bò", Description = "Phở bò truyền thống", ImageUrl = "/images/recipes/phobo.jpeg", Category = "Món chính", Ingredients = new List<string> { "Bánh phở", "Thịt bò", "Hành tây", "Rau thơm", "Gia vị" } },
                new Recipe { Id = 2, Name = "Bánh Mì", Description = "Bánh mì thịt nướng", ImageUrl = "/images/recipes/banhmi.jpg", Category = "Món chính", Ingredients = new List<string> { "Bánh mì", "Thịt nướng", "Pate", "Rau củ", "Gia vị" } },
                new Recipe { Id = 3, Name = "Cơm Tấm", Description = "Cơm tấm sườn nướng", ImageUrl = "/images/recipes/Comtam.jpg", Category = "Món chính", Ingredients = new List<string> { "Cơm tấm", "Sườn nướng", "Trứng", "Bì", "Chả" } },
                new Recipe { Id = 4, Name = "Kho Quẹt", Description = "Kho quẹt tôm thịt", ImageUrl = "/images/recipes/Khoquet.webp", Category = "Món ăn kèm", Ingredients = new List<string> { "Tôm", "Thịt ba chỉ", "Nước mắm", "Đường", "Tỏi", "Ớt" } }
            };

            var filteredRecipes = recipes
                .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           r.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                           r.Ingredients.Any(i => i.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Json(new { success = true, recipes = filteredRecipes });
        }

        // Add item to Shopping List
        [HttpPost]
        public async Task<IActionResult> AddToShoppingList(string name, string? category, string? notes, int quantity = 1, string? unit = null, decimal? price = null)
        {
            var userId = _userManager.GetUserId(User);
            
            var item = new ShoppingItem
            {
                Name = name,
                Category = category,
                Notes = notes,
                Quantity = quantity,
                Unit = unit,
                Price = price,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.ShoppingItems.Add(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào danh sách mua sắm", itemId = item.Id });
        }

        // Add item to Cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int? shoppingItemId, string name, string? category, int quantity = 1, string? unit = null, decimal? price = null)
        {
            var userId = _userManager.GetUserId(User);
            
            // Nếu có shoppingItemId, lấy giá từ shopping item nếu chưa có
            if (shoppingItemId.HasValue && !price.HasValue)
            {
                var shoppingItem = await _context.ShoppingItems.FindAsync(shoppingItemId.Value);
                if (shoppingItem != null && shoppingItem.Price.HasValue)
                {
                    price = shoppingItem.Price.Value;
                }
            }
            
            var cartItem = new CartItem
            {
                Name = name,
                Category = category,
                Quantity = quantity,
                Unit = unit,
                Price = price,
                ShoppingItemId = shoppingItemId,
                UserId = userId,
                AddedAt = DateTime.Now
            };

            _context.CartItems.Add(cartItem);
            
            // Mark shopping item as in cart if exists
            if (shoppingItemId.HasValue)
            {
                var shoppingItem = await _context.ShoppingItems.FindAsync(shoppingItemId.Value);
                if (shoppingItem != null)
                {
                    shoppingItem.InCart = true;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", itemId = cartItem.Id });
        }

        // Get Cart
        public async Task<IActionResult> Cart()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Where(c => (userId == null || c.UserId == userId) && !c.IsPurchased)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();
            
            ViewBag.CartItems = cartItems;
            return View();
        }

        // Delete Shopping Item
        [HttpPost]
        public async Task<IActionResult> DeleteShoppingItem(int id)
        {
            var item = await _context.ShoppingItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy item" });
            }

            _context.ShoppingItems.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa item" });
        }

        // Update Shopping Item
        [HttpPost]
        public async Task<IActionResult> UpdateShoppingItem(int id, string? name, string? category, string? notes, bool? isCompleted)
        {
            var item = await _context.ShoppingItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy item" });
            }

            if (!string.IsNullOrEmpty(name)) item.Name = name;
            if (category != null) item.Category = category;
            if (notes != null) item.Notes = notes;
            if (isCompleted.HasValue) item.IsCompleted = isCompleted.Value;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật item" });
        }

        // Update Cart Item Quantity
        [HttpPost]
        public async Task<IActionResult> UpdateCartItemQuantity(int id, int quantity)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy item trong giỏ hàng" });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật số lượng", item = item });
        }

        // Remove from Cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy item trong giỏ hàng" });
            }

            // Unmark shopping item if exists
            if (item.ShoppingItemId.HasValue)
            {
                var shoppingItem = await _context.ShoppingItems.FindAsync(item.ShoppingItemId.Value);
                if (shoppingItem != null)
                {
                    shoppingItem.InCart = false;
                }
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa khỏi giỏ hàng" });
        }
    }
}
