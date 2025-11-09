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
                
                // Debug: Log first category structure
                if (categories.Count > 0)
                {
                    var firstCat = categories[0];
                    _logger.LogInformation($"🔍 First category - Id: '{firstCat.GetId()}', Name: '{firstCat.Name}', Icon: '{firstCat.Icon}'");
                }
                
                // Return categories with normalized ID field
                var responseCategories = categories.Select(c => new
                {
                    id = c.GetId(),
                    name = c.Name,
                    icon = c.Icon,
                    userId = c.UserId
                }).ToList();
                
                return Ok(responseCategories);
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
                _logger.LogInformation($"🔄 POST /api/IngredientCategoryApi - Name: {request.Name}, Icon: {request.Icon}");
                
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Tên danh mục là bắt buộc" });
                }
                
                if (string.IsNullOrWhiteSpace(request.Icon))
                {
                    return BadRequest(new { message = "Icon là bắt buộc" });
                }
                
                var apiRequest = new CookMateApiService.CreateCategoryRequest
                {
                    Name = request.Name.Trim(),
                    Icon = request.Icon.Trim()
                };

                var category = await _cookMateApi.CreateCategoryAsync(apiRequest);
                
                if (category == null)
                {
                    _logger.LogWarning("⚠️ CreateCategoryAsync returned null");
                    return StatusCode(500, new { message = "Không thể tạo danh mục. Vui lòng kiểm tra logs để xem chi tiết lỗi." });
                }

                _logger.LogInformation($"✅ Created category: {category.Name}, ID: {category.GetId()}");
                
                // Return normalized response
                var responseCategory = new
                {
                    id = category.GetId(),
                    name = category.Name,
                    icon = category.Icon,
                    userId = category.UserId
                };
                
                return Ok(responseCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
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
                _logger.LogInformation($"🔄 PUT /api/IngredientCategoryApi - CategoryId: {request.CategoryId}, Name: {request.Name}");
                
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.CategoryId))
                {
                    return BadRequest(new { message = "ID danh mục là bắt buộc" });
                }
                
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Tên danh mục là bắt buộc" });
                }
                
                if (string.IsNullOrWhiteSpace(request.Icon))
                {
                    return BadRequest(new { message = "Icon là bắt buộc" });
                }
                
                var apiRequest = new CookMateApiService.UpdateCategoryRequest
                {
                    IngredientCategoryId = request.CategoryId.Trim(),
                    Name = request.Name.Trim(),
                    Icon = request.Icon.Trim()
                };

                var category = await _cookMateApi.UpdateCategoryAsync(apiRequest);
                
                if (category == null)
                {
                    _logger.LogWarning("⚠️ UpdateCategoryAsync returned null");
                    return StatusCode(500, new { message = "Không thể cập nhật danh mục. Vui lòng thử lại." });
                }

                _logger.LogInformation($"✅ Updated category: {category.Name}");
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
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
                _logger.LogInformation($"🔄 DELETE /api/IngredientCategoryApi/{id}");
                
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID danh mục là bắt buộc" });
                }
                
                var success = await _cookMateApi.DeleteCategoryAsync(id);
                
                if (!success)
                {
                    _logger.LogWarning($"⚠️ DeleteCategoryAsync returned false for id: {id}");
                    return StatusCode(500, new { message = "Không thể xóa danh mục. Vui lòng thử lại." });
                }

                _logger.LogInformation($"✅ Deleted category: {id}");
                return Ok(new { message = "Đã xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
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

