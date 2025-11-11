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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            
            // Get most favorited recipes
            var recipes = await _context.Recipes
                .Include(r => r.Favorites)
                .Include(r => r.Reactions)
                .Include(r => r.Comments)
                .Include(r => r.User)
                .ToListAsync();
            
            var mostFavoritedRecipes = recipes
                .Select(r => new {
                    Recipe = r,
                    FavoriteCount = r.Favorites.Count,
                    ReactionCount = r.Reactions.Count,
                    CommentCount = r.Comments.Count,
                    IsFavorited = userId != null && r.Favorites.Any(f => f.UserId == userId),
                    UserReaction = userId != null ? r.Reactions.FirstOrDefault(re => re.UserId == userId) : null
                })
                .OrderByDescending(x => x.FavoriteCount)
                .ThenByDescending(x => x.ReactionCount)
                .Take(10)
                .ToList();
            
            ViewBag.MostFavoritedRecipes = mostFavoritedRecipes;
            ViewBag.CurrentUserId = userId;
            
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

        public async Task<IActionResult> FavoriteList(string? searchQuery = null, string? sortBy = "newest")
        {
            var userId = _userManager.GetUserId(User);
            var favoritesQuery = _context.Favorites
                .Include(f => f.Recipe)
                    .ThenInclude(r => r.Favorites)
                .Include(f => f.Recipe)
                    .ThenInclude(r => r.Reactions)
                .Include(f => f.Recipe)
                    .ThenInclude(r => r.Comments)
                .Include(f => f.Recipe)
                    .ThenInclude(r => r.User)
                .Where(f => userId == null || f.UserId == userId);
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                favoritesQuery = favoritesQuery.Where(f => 
                    f.Recipe.Name.Contains(searchQuery) ||
                    (f.Recipe.Description != null && f.Recipe.Description.Contains(searchQuery)) ||
                    (f.Recipe.Category != null && f.Recipe.Category.Contains(searchQuery)));
            }
            
            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "mostfavorited":
                    favoritesQuery = favoritesQuery.OrderByDescending(f => f.Recipe.Favorites.Count);
                    break;
                case "mostreacted":
                    favoritesQuery = favoritesQuery.OrderByDescending(f => f.Recipe.Reactions.Count);
                    break;
                case "name":
                    favoritesQuery = favoritesQuery.OrderBy(f => f.Recipe.Name);
                    break;
                case "newest":
                default:
                    favoritesQuery = favoritesQuery.OrderByDescending(f => f.AddedAt);
                    break;
            }
            
            var favoritesList = await favoritesQuery.ToListAsync();
            
            var favorites = favoritesList
                .Select(f => new {
                    Favorite = f,
                    Recipe = f.Recipe,
                    FavoriteCount = f.Recipe.Favorites.Count,
                    ReactionCount = f.Recipe.Reactions.Count,
                    CommentCount = f.Recipe.Comments.Count
                })
                .ToList();
            
            ViewBag.Favorites = favorites;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentUserId = userId;
            return View();
        }
        
        // Add to Favorites
        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int recipeId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }
            
            // Check if recipe exists
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy công thức" });
            }
            
            // Check if already favorited
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);
            
            if (existingFavorite != null)
            {
                return Json(new { success = false, message = "Đã có trong danh sách yêu thích" });
            }
            
            var favorite = new Favorite
            {
                UserId = userId,
                RecipeId = recipeId,
                AddedAt = DateTime.Now
            };
            
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích" });
        }
        
        // Remove from Favorites
        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(int recipeId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }
            
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);
            
            if (favorite == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trong danh sách yêu thích" });
            }
            
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích" });
        }
        
        // Toggle Reaction
        [HttpPost]
        public async Task<IActionResult> ToggleReaction(int recipeId, string reactionType)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }
            
            // Validate reaction type
            var validTypes = new[] { "like", "love", "wow", "haha", "sad", "angry" };
            if (!validTypes.Contains(reactionType.ToLower()))
            {
                return Json(new { success = false, message = "Loại cảm xúc không hợp lệ" });
            }
            
            var reaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId && r.Type == reactionType);
            
            if (reaction != null)
            {
                // Remove reaction
                _context.Reactions.Remove(reaction);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã bỏ cảm xúc", isAdded = false });
            }
            else
            {
                // Add reaction - remove other reactions from this user for this recipe first
                var existingReactions = await _context.Reactions
                    .Where(r => r.UserId == userId && r.RecipeId == recipeId)
                    .ToListAsync();
                _context.Reactions.RemoveRange(existingReactions);
                
                var newReaction = new Reaction
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    Type = reactionType.ToLower(),
                    CreatedAt = DateTime.Now
                };
                
                _context.Reactions.Add(newReaction);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã thêm cảm xúc", isAdded = true });
            }
        }
        
        // Get Reactions for Recipe
        [HttpGet]
        public async Task<IActionResult> GetReactions(int recipeId)
        {
            var allReactions = await _context.Reactions
                .Include(r => r.User)
                .Where(r => r.RecipeId == recipeId)
                .ToListAsync();
            
            var reactions = allReactions
                .GroupBy(r => r.Type)
                .Select(g => new {
                    type = g.Key,
                    count = g.Count()
                })
                .ToList();
            
            var userId = _userManager.GetUserId(User);
            var userReaction = userId != null 
                ? allReactions.FirstOrDefault(r => r.UserId == userId)?.Type
                : null;
            
            return Json(new { 
                success = true, 
                reactions = reactions,
                userReaction = userReaction
            });
        }
        
        // Get Comments for Recipe
        [HttpGet]
        public async Task<IActionResult> GetComments(int recipeId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.RecipeId == recipeId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new {
                    id = c.Id,
                    content = c.Content,
                    userName = c.User != null ? (c.User.FullName ?? c.User.UserName) : "Anonymous",
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    userId = c.UserId
                })
                .ToListAsync();
            
            return Json(new { success = true, comments = comments });
        }
        
        // Add Comment
        [HttpPost]
        public async Task<IActionResult> AddComment(int recipeId, string content)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }
            
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung bình luận" });
            }
            
            // Check if recipe exists
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy công thức" });
            }
            
            var comment = new Comment
            {
                RecipeId = recipeId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };
            
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            
            var user = await _userManager.FindByIdAsync(userId);
            return Json(new { 
                success = true, 
                message = "Đã thêm bình luận",
                comment = new {
                    id = comment.Id,
                    content = comment.Content,
                    userName = user != null ? (user.FullName ?? user.UserName) : "Anonymous",
                    createdAt = comment.CreatedAt,
                    userId = comment.UserId
                }
            });
        }
        
        // Delete Comment
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }
            
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận" });
            }
            
            // Only allow user to delete their own comments
            if (comment.UserId != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này" });
            }
            
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Đã xóa bình luận" });
        }
        
        // Post Recipe (Upload/Add Recipe)
        [HttpPost]
        public async Task<IActionResult> PostRecipe(string name, string? description, string? imageUrl, string? ingredients, string? category, int? cookingTime, string? difficulty)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }
            
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Vui lòng nhập tên món ăn" });
            }
            
            var recipe = new Recipe
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                ImageUrl = imageUrl?.Trim(),
                Ingredients = ingredients?.Trim(),
                Category = category?.Trim(),
                CookingTime = cookingTime,
                Difficulty = difficulty?.Trim(),
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            
            return Json(new { 
                success = true, 
                message = "Đã đăng món ăn thành công",
                recipeId = recipe.Id
            });
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

        // Get Most Favorited Recipes
        [HttpGet]
        public async Task<IActionResult> GetMostFavoritedRecipes(int limit = 10)
        {
            var userId = _userManager.GetUserId(User);
            
            var recipesList = await _context.Recipes
                .Include(r => r.Favorites)
                .Include(r => r.Reactions)
                .Include(r => r.Comments)
                .Include(r => r.User)
                .ToListAsync();
            
            var recipes = recipesList
                .Select(r => {
                    var userReaction = userId != null 
                        ? r.Reactions.FirstOrDefault(re => re.UserId == userId) 
                        : null;
                    
                    return new {
                        id = r.Id,
                        name = r.Name,
                        description = r.Description,
                        imageUrl = r.ImageUrl,
                        category = r.Category,
                        cookingTime = r.CookingTime,
                        difficulty = r.Difficulty,
                        favoriteCount = r.Favorites.Count,
                        reactionCount = r.Reactions.Count,
                        commentCount = r.Comments.Count,
                        createdAt = r.CreatedAt,
                        userName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Anonymous",
                        isFavorited = userId != null && r.Favorites.Any(f => f.UserId == userId),
                        userReaction = userReaction != null ? userReaction.Type : null
                    };
                })
                .OrderByDescending(r => r.favoriteCount)
                .ThenByDescending(r => r.reactionCount)
                .Take(limit)
                .ToList();
            
            return Json(new { success = true, recipes = recipes });
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
                new Recipe { Id = 1, Name = "Phở Bò", Description = "Phở bò truyền thống", ImageUrl = "/images/recipes/phobo.jpeg", Category = "Món chính", Ingredients = "[\"Bánh phở\", \"Thịt bò\", \"Hành tây\", \"Rau thơm\", \"Gia vị\"]" },
                new Recipe { Id = 2, Name = "Bánh Mì", Description = "Bánh mì thịt nướng", ImageUrl = "/images/recipes/banhmi.jpg", Category = "Món chính", Ingredients = "[\"Bánh mì\", \"Thịt nướng\", \"Pate\", \"Rau củ\", \"Gia vị\"]" },
                new Recipe { Id = 3, Name = "Cơm Tấm", Description = "Cơm tấm sườn nướng", ImageUrl = "/images/recipes/Comtam.jpg", Category = "Món chính", Ingredients = "[\"Cơm tấm\", \"Sườn nướng\", \"Trứng\", \"Bì\", \"Chả\"]" },
                new Recipe { Id = 4, Name = "Kho Quẹt", Description = "Kho quẹt tôm thịt", ImageUrl = "/images/recipes/Khoquet.webp", Category = "Món ăn kèm", Ingredients = "[\"Tôm\", \"Thịt ba chỉ\", \"Nước mắm\", \"Đường\", \"Tỏi\", \"Ớt\"]" }
            };

            var filteredRecipes = recipes
                .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           r.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                           (r.Ingredients != null && r.Ingredients.Contains(query, StringComparison.OrdinalIgnoreCase)))
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
