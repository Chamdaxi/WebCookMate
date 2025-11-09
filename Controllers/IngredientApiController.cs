using Microsoft.AspNetCore.Mvc;
using demo.Services;
using System.Text.Json;

namespace demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<IngredientApiController> _logger;

        public IngredientApiController(CookMateApiService cookMateApi, ILogger<IngredientApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

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
                    return StatusCode(500, new { message = "Không thể lấy danh sách nguyên liệu" });
                }

                _logger.LogInformation($"✅ Fetched {ingredients.Count} ingredients from CookMate API");
                
                // Normalize ingredients - ensure Id is populated from _id if needed
                var normalizedIngredients = ingredients.Select(ing => new
                {
                    id = ing.GetId(), // Use GetId() to get ID from any field
                    name = ing.Name ?? "",
                    categoryId = ing.CategoryId ?? "",
                    quantity = ing.Quantity,
                    unit = ing.Unit ?? "piece",
                    expireDate = ing.ExpireDate ?? ing.ExpiryDate,
                    expiryDate = ing.ExpireDate ?? ing.ExpiryDate, // Support both field names
                    notes = ing.Notes ?? "",
                    imageUrl = ing.ImageUrl ?? "",
                    userId = ing.UserId ?? "",
                    createdAt = ing.CreatedAt
                }).ToList();
                
                _logger.LogInformation($"✅ Normalized {normalizedIngredients.Count} ingredients for response");
                if (normalizedIngredients.Count > 0)
                {
                    _logger.LogInformation($"🔍 First normalized ingredient - id: '{normalizedIngredients[0].id}', name: '{normalizedIngredients[0].name}', categoryId: '{normalizedIngredients[0].categoryId}'");
                }
                
                return Ok(normalizedIngredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ingredients");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddIngredient([FromForm] AddIngredientRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Tên nguyên liệu là bắt buộc" });
                }
                
                if (string.IsNullOrWhiteSpace(request.CategoryId))
                {
                    return BadRequest(new { message = "Danh mục là bắt buộc" });
                }
                
                if (request.Quantity <= 0)
                {
                    return BadRequest(new { message = "Số lượng phải lớn hơn 0" });
                }
                
                if (string.IsNullOrWhiteSpace(request.Unit))
                {
                    return BadRequest(new { message = "Đơn vị là bắt buộc" });
                }

                // Call API service
                var ingredient = await _cookMateApi.AddIngredientAsync(
                    categoryId: request.CategoryId,
                    name: request.Name,
                    quantity: request.Quantity,
                    unit: request.Unit,
                    expireDate: request.ExpireDate,
                    notes: request.Notes,
                    image: request.Image
                );

                if (ingredient == null)
                {
                    return StatusCode(500, new { 
                        message = "Không thể thêm nguyên liệu. Vui lòng thử lại sau.",
                        error = "API returned null"
                    });
                }

                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ingredient");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateIngredient([FromForm] UpdateIngredientRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IngredientId))
                {
                    return BadRequest(new { message = "ID nguyên liệu là bắt buộc" });
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Tên nguyên liệu là bắt buộc" });
                }

                // Call API service with individual parameters (uses raw multipart body)
                var ingredient = await _cookMateApi.UpdateIngredientAsync(
                    ingredientId: request.IngredientId,
                    categoryId: request.CategoryId,
                    name: request.Name,
                    quantity: request.Quantity,
                    unit: request.Unit,
                    expireDate: request.ExpireDate,
                    notes: request.Notes,
                    image: request.Image
                );

                if (ingredient == null)
                {
                    return StatusCode(500, new { message = "Không thể cập nhật nguyên liệu. Vui lòng thử lại sau." });
                }

                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ingredient");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "ID nguyên liệu là bắt buộc" });
                }
                
                var success = await _cookMateApi.DeleteIngredientAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, new { message = "Không thể xóa nguyên liệu. Vui lòng thử lại." });
                }

                return Ok(new { message = "Đã xóa nguyên liệu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ingredient");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        #region Request Models

        public class AddIngredientRequest
        {
            public string CategoryId { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = "";
            public string? ExpireDate { get; set; }
            public string? Notes { get; set; }
            public IFormFile? Image { get; set; }
        }

        public class UpdateIngredientRequest
        {
            public string IngredientId { get; set; } = "";
            public string? CategoryId { get; set; }
            public string Name { get; set; } = "";
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = "";
            public string? ExpireDate { get; set; }
            public string? Notes { get; set; }
            public IFormFile? Image { get; set; }
        }

        #endregion
    }
}
