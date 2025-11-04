using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace demo.Controllers
{
    /// <summary>
    /// API Controller để gọi CookMate API cho Ingredient Categories
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientCategoryApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<IngredientCategoryApiController> _logger;

        public IngredientCategoryApiController(
            CookMateApiService cookMateApi,
            ILogger<IngredientCategoryApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/IngredientCategoryApi
        /// Lấy tất cả categories từ CookMate API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                _logger.LogInformation("🔄 GET /api/IngredientCategoryApi called");
                
                var categories = await _cookMateApi.GetIngredientCategoriesAsync();
                
                if (categories == null)
                {
                    _logger.LogWarning("⚠️ GetIngredientCategoriesAsync returned null");
                    return StatusCode(500, new { message = "Failed to fetch categories from API", error = "Service returned null" });
                }

                _logger.LogInformation($"✅ Fetched {categories.Count} categories from CookMate API");
                return Ok(categories);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"⚠️ Unauthorized: {ex.Message}");
                return StatusCode(401, new { message = "Unauthorized", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting categories from API: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/IngredientCategoryApi
        /// Tạo category mới qua CookMate API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var apiRequest = new CookMateApiService.CreateCategoryRequest
                {
                    Name = request.Name,
                    Icon = request.Icon
                };

                var category = await _cookMateApi.CreateCategoryAsync(apiRequest);
                
                if (category == null)
                {
                    return StatusCode(500, new { message = "Failed to create category" });
                }

                _logger.LogInformation($"✅ Created category: {category.Name}");
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// PUT: api/IngredientCategoryApi
        /// Cập nhật category qua CookMate API
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var apiRequest = new CookMateApiService.UpdateCategoryRequest
                {
                    IngredientCategoryId = request.CategoryId,
                    Name = request.Name,
                    Icon = request.Icon
                };

                var category = await _cookMateApi.UpdateCategoryAsync(apiRequest);
                
                if (category == null)
                {
                    return StatusCode(500, new { message = "Failed to update category" });
                }

                _logger.LogInformation($"✅ Updated category: {category.Name}");
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// DELETE: api/IngredientCategoryApi/{id}
        /// Xóa category qua CookMate API
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var success = await _cookMateApi.DeleteCategoryAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, new { message = "Failed to delete category" });
                }

                _logger.LogInformation($"✅ Deleted category: {id}");
                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #region Request Models

        public class CreateCategoryRequest
        {
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public class UpdateCategoryRequest
        {
            public string CategoryId { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        #endregion
    }
}

