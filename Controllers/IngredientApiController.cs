using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace demo.Controllers
{
    /// <summary>
    /// API Controller để gọi CookMate API cho Ingredients
    /// Thay thế cho local database operations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<IngredientApiController> _logger;

        public IngredientApiController(
            CookMateApiService cookMateApi,
            ILogger<IngredientApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/IngredientApi
        /// Lấy tất cả ingredients từ CookMate API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetIngredients()
        {
            try
            {
                _logger.LogInformation("🔄 GET /api/IngredientApi called");
                
                var ingredients = await _cookMateApi.GetIngredientsAsync();
                
                if (ingredients == null)
                {
                    _logger.LogWarning("⚠️ GetIngredientsAsync returned null");
                    return StatusCode(500, new { message = "Failed to fetch ingredients from API", error = "Service returned null" });
                }

                _logger.LogInformation($"✅ Fetched {ingredients.Count} ingredients from CookMate API");
                return Ok(ingredients);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"⚠️ Unauthorized: {ex.Message}");
                return StatusCode(401, new { message = "Unauthorized", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting ingredients from API: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/IngredientApi
        /// Thêm ingredient mới qua CookMate API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddIngredient([FromForm] AddIngredientRequest request)
        {
            try
            {
                // Tạo FormData để upload (bao gồm image nếu có)
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(request.CategoryId), "categoryId");
                formData.Add(new StringContent(request.Name), "name");
                formData.Add(new StringContent(request.Quantity.ToString()), "quantity");
                formData.Add(new StringContent(request.Unit), "unit");
                
                if (request.ExpireDate.HasValue)
                {
                    formData.Add(new StringContent(request.ExpireDate.Value.ToString("o")), "expireDate");
                }
                
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    formData.Add(new StringContent(request.Notes), "notes");
                }

                // Upload image if provided
                if (request.Image != null && request.Image.Length > 0)
                {
                    var streamContent = new StreamContent(request.Image.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.Image.ContentType);
                    formData.Add(streamContent, "image", request.Image.FileName);
                }

                var ingredient = await _cookMateApi.AddIngredientAsync(formData);
                
                if (ingredient == null)
                {
                    return StatusCode(500, new { message = "Failed to add ingredient" });
                }

                _logger.LogInformation($"✅ Added ingredient: {ingredient.Name}");
                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ingredient");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// PUT: api/IngredientApi
        /// Cập nhật ingredient qua CookMate API
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateIngredient([FromForm] UpdateIngredientRequest request)
        {
            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(request.IngredientId), "ingredientId");
                formData.Add(new StringContent(request.Name), "name");
                formData.Add(new StringContent(request.CategoryId), "categoryId");
                formData.Add(new StringContent(request.Quantity.ToString()), "quantity");
                formData.Add(new StringContent(request.Unit), "unit");
                
                if (request.ExpireDate.HasValue)
                {
                    formData.Add(new StringContent(request.ExpireDate.Value.ToString("o")), "expireDate");
                }
                
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    formData.Add(new StringContent(request.Notes), "notes");
                }

                if (request.Image != null && request.Image.Length > 0)
                {
                    var streamContent = new StreamContent(request.Image.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.Image.ContentType);
                    formData.Add(streamContent, "image", request.Image.FileName);
                }

                var ingredient = await _cookMateApi.UpdateIngredientAsync(formData);
                
                if (ingredient == null)
                {
                    return StatusCode(500, new { message = "Failed to update ingredient" });
                }

                _logger.LogInformation($"✅ Updated ingredient: {ingredient.Name}");
                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ingredient");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// DELETE: api/IngredientApi/{id}
        /// Xóa ingredient qua CookMate API
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(string id)
        {
            try
            {
                var success = await _cookMateApi.DeleteIngredientAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, new { message = "Failed to delete ingredient" });
                }

                _logger.LogInformation($"✅ Deleted ingredient: {id}");
                return Ok(new { message = "Ingredient deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ingredient");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #region Request Models

        public class AddIngredientRequest
        {
            public string CategoryId { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = "";
            public DateTime? ExpireDate { get; set; }
            public string? Notes { get; set; }
            public IFormFile? Image { get; set; }
        }

        public class UpdateIngredientRequest
        {
            public string IngredientId { get; set; } = "";
            public string CategoryId { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = "";
            public DateTime? ExpireDate { get; set; }
            public string? Notes { get; set; }
            public IFormFile? Image { get; set; }
        }

        #endregion
    }
}

