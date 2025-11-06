using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;
using demo.Models;

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
                
                var favorites = await _cookMateApi.GetFavoritesAsync();
                
                // Return empty list instead of error if null (allows UI to work)
                if (favorites == null)
                {
                    _logger.LogWarning("⚠️ GetFavoritesAsync returned null, returning empty list");
                    return Ok(new List<object>());
                }

                _logger.LogInformation($"✅ Fetched {favorites.Count} favorites from CookMate API");
                return Ok(favorites);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Spoonacular API limit"))
            {
                // Spoonacular limit reached - favorites exist but cannot fetch details
                _logger.LogWarning($"⚠️ Spoonacular API limit: {ex.Message}");
                return StatusCode(503, new { 
                    message = "Không thể tải danh sách món yêu thích", 
                    error = "Spoonacular API limit reached",
                    details = "Món yêu thích của bạn đã được lưu nhưng không thể hiển thị chi tiết do giới hạn API. Vui lòng thử lại sau.",
                    code = "SPOONACULAR_LIMIT"
                });
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
                var favorites = await _cookMateApi.GetFavoritesAsync();
                
                if (favorites == null)
                {
                    return Ok(new { isFavorite = false });
                }

                var isFavorite = favorites.Any(f => f.RecipeId == recipeId);
                
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

