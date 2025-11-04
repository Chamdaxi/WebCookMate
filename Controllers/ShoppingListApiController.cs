using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace demo.Controllers
{
    /// <summary>
    /// API Controller để gọi CookMate API cho Shopping List
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingListApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<ShoppingListApiController> _logger;

        public ShoppingListApiController(
            CookMateApiService cookMateApi,
            ILogger<ShoppingListApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/ShoppingListApi
        /// Lấy tất cả shopping items từ CookMate API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetShoppingList()
        {
            try
            {
                var items = await _cookMateApi.GetShoppingListAsync();
                
                if (items == null)
                {
                    return StatusCode(500, new { message = "Failed to fetch shopping list from API" });
                }

                _logger.LogInformation($"✅ Fetched {items.Count} shopping items from CookMate API");
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shopping list from API");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// POST: api/ShoppingListApi
        /// Thêm item vào shopping list
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddShoppingItemRequest request)
        {
            try
            {
                var apiRequest = new CookMateApiService.CreateShoppingItemRequest
                {
                    Name = request.Name,
                    Notes = request.Notes,
                    Status = request.Status ?? "active"
                };

                var item = await _cookMateApi.AddShoppingItemAsync(apiRequest);
                
                if (item == null)
                {
                    return StatusCode(500, new { message = "Failed to add shopping item" });
                }

                _logger.LogInformation($"✅ Added shopping item: {item.Name}");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding shopping item");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// PUT: api/ShoppingListApi
        /// Cập nhật shopping item
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateShoppingItemRequest request)
        {
            try
            {
                var apiRequest = new CookMateApiService.UpdateShoppingItemRequest
                {
                    ShoppingItemId = request.ShoppingItemId,
                    Name = request.Name,
                    Notes = request.Notes,
                    Status = request.Status ?? "active"
                };

                var item = await _cookMateApi.UpdateShoppingItemAsync(apiRequest);
                
                if (item == null)
                {
                    return StatusCode(500, new { message = "Failed to update shopping item" });
                }

                _logger.LogInformation($"✅ Updated shopping item: {item.Name}");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shopping item");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// DELETE: api/ShoppingListApi/{id}
        /// Xóa shopping item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                var success = await _cookMateApi.DeleteShoppingItemAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, new { message = "Failed to delete shopping item" });
                }

                _logger.LogInformation($"✅ Deleted shopping item: {id}");
                return Ok(new { message = "Shopping item deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shopping item");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #region Request Models

        public class AddShoppingItemRequest
        {
            public string Name { get; set; } = "";
            public string? Notes { get; set; }
            public string? Status { get; set; }
        }

        public class UpdateShoppingItemRequest
        {
            public string ShoppingItemId { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Notes { get; set; }
            public string? Status { get; set; }
        }

        #endregion
    }
}

