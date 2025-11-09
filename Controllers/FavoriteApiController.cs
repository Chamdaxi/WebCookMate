using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;
using demo.Models;
using System.Linq;

namespace demo.Controllers
{
    /// <summary>
    /// API Controller để gọi CookMate API cho Favorites
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<FavoriteApiController> _logger;

        public FavoriteApiController(
            CookMateApiService cookMateApi,
            ILogger<FavoriteApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/FavoriteApi
        /// Lấy tất cả favorites từ CookMate API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                _logger.LogInformation("🔄 GET /api/FavoriteApi called");
                
                var (favorites, isSpoonacularLimit) = await _cookMateApi.GetFavoritesAsync();
                
                // Handle null response
                if (favorites == null)
                {
                    _logger.LogWarning("⚠️ GetFavoritesAsync returned null, returning empty list");
                    return Ok(new { 
                        favorites = new List<object>(),
                        message = "Không thể lấy danh sách favorites. Vui lòng thử lại sau.",
                        spoonacularLimit = false,
                        canAdd = true
                    });
                }

                // If favorites list is empty
                if (favorites.Count == 0)
                {
                    if (isSpoonacularLimit)
                    {
                        _logger.LogInformation("✅ Fetched 0 favorites from CookMate API due to Spoonacular limit");
                        return Ok(new { 
                            favorites = new List<object>(),
                            message = "Giới hạn API Spoonacular đã đạt. Favorites có thể đã được lưu nhưng không thể hiển thị chi tiết. Bạn vẫn có thể thêm favorites mới bằng button 'Thêm món yêu thích'. Favorites sẽ được hiển thị khi API limit reset.",
                            spoonacularLimit = true,
                            canAdd = true
                        });
                    }
                    else
                    {
                        _logger.LogInformation("✅ Fetched 0 favorites from CookMate API (no favorites found)");
                        return Ok(new { 
                            favorites = new List<object>(),
                            message = "Chưa có món yêu thích nào. Hãy thêm favorites bằng button 'Thêm món yêu thích' hoặc từ trang Recipes.",
                            spoonacularLimit = false,
                            canAdd = true
                        });
                    }
                }

                _logger.LogInformation($"✅ Fetched {favorites.Count} favorites from CookMate API (Spoonacular limit: {isSpoonacularLimit})");
                
                // Normalize favorites to ensure IDs are consistent
                var normalizedFavorites = favorites.Select(f => new
                {
                    id = f.GetId(),
                    _id = f.GetId(),
                    favoriteId = f.GetId(),
                    recipeId = f.GetRecipeId(),
                    recipe_id = f.GetRecipeId(),
                    userId = f.UserId ?? "",
                    createdAt = f.GetCreatedAt().ToString("o"),
                    created_at = f.GetCreatedAt().ToString("o"),
                    addedAt = f.GetCreatedAt().ToString("o")
                }).ToList();
                
                // Log normalized favorites for debugging
                if (normalizedFavorites.Count > 0)
                {
                    var first = normalizedFavorites[0];
                    _logger.LogInformation($"🔍 First normalized favorite - id: '{first.id}', recipeId: '{first.recipeId}', userId: '{first.userId}'");
                }
                
                // Return favorites with Spoonacular limit flag if applicable
                if (isSpoonacularLimit)
                {
                    return Ok(new { 
                        favorites = normalizedFavorites,
                        message = "Một số favorites có thể không hiển thị đầy đủ chi tiết do giới hạn API Spoonacular.",
                        spoonacularLimit = true,
                        canAdd = true
                    });
                }
                
                return Ok(normalizedFavorites);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"⚠️ Unauthorized: {ex.Message}");
                return StatusCode(401, new { message = "Unauthorized", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting favorites from API: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/FavoriteApi/recipe/{recipeId}
        /// Kiểm tra recipe có phải favorite không
        /// </summary>
        [HttpGet("recipe/{recipeId}")]
        public async Task<IActionResult> CheckFavorite(string recipeId)
        {
            try
            {
                var (favorites, _) = await _cookMateApi.GetFavoritesAsync();
                
                if (favorites == null)
                {
                    return Ok(new { isFavorite = false });
                }

                var isFavorite = favorites.Any(f => f.GetRecipeId() == recipeId);
                
                _logger.LogInformation($"✅ Checked favorite status for recipe {recipeId}: {isFavorite}");
                return Ok(new { isFavorite });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking favorite for recipe {recipeId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// POST: api/FavoriteApi
        /// Thêm recipe vào favorites
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            try
            {
                _logger.LogInformation($"🔄 POST /api/FavoriteApi called - RecipeId: {request.RecipeId}");
                
                if (string.IsNullOrEmpty(request.RecipeId))
                {
                    return BadRequest(new { message = "RecipeId is required" });
                }

                var favorite = await _cookMateApi.AddFavoriteAsync(request.RecipeId);
                
                if (favorite == null)
                {
                    _logger.LogWarning($"⚠️ AddFavoriteAsync returned null for RecipeId: {request.RecipeId}");
                    return StatusCode(500, new { message = "Failed to add favorite", error = "Service returned null" });
                }

                _logger.LogInformation($"✅ Added favorite: Recipe {request.RecipeId}");
                return Ok(favorite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error adding favorite: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/FavoriteApi/recipe/{recipeId}
        /// Xóa recipe khỏi favorites
        /// </summary>
        [HttpDelete("recipe/{recipeId}")]
        public async Task<IActionResult> DeleteFavorite(string recipeId)
        {
            try
            {
                var success = await _cookMateApi.DeleteFavoriteAsync(recipeId);
                
                if (!success)
                {
                    return StatusCode(500, new { message = "Failed to delete favorite" });
                }

                _logger.LogInformation($"✅ Deleted favorite: Recipe {recipeId}");
                return Ok(new { message = "Favorite deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting favorite");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #region Request Models

        public class AddFavoriteRequest
        {
            public string RecipeId { get; set; } = "";
        }

        #endregion
    }
}

