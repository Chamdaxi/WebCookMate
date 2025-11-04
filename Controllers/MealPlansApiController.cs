using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace demo.Controllers
{
    /// <summary>
    /// API Controller để gọi CookMate API cho Meal Plans
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MealPlansApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<MealPlansApiController> _logger;

        public MealPlansApiController(
            CookMateApiService cookMateApi,
            ILogger<MealPlansApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/MealPlansApi
        /// Lấy tất cả meal plans từ CookMate API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMealPlans()
        {
            try
            {
                var mealPlans = await _cookMateApi.GetMealPlansAsync();
                
                if (mealPlans == null)
                {
                    return StatusCode(500, new { message = "Failed to fetch meal plans from API" });
                }

                _logger.LogInformation($"✅ Fetched {mealPlans.Count} meal plans from CookMate API");
                return Ok(mealPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meal plans from API");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// POST: api/MealPlansApi
        /// Tạo meal plan mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMealPlan([FromBody] CreateMealPlanRequest request)
        {
            try
            {
                var apiRequest = new CookMateApiService.CreateMealPlanRequest
                {
                    Name = request.Name,
                    RecipeIds = request.RecipeIds ?? new List<string>(),
                    Notes = request.Notes,
                    Date = request.Date
                };

                var mealPlan = await _cookMateApi.CreateMealPlanAsync(apiRequest);
                
                if (mealPlan == null)
                {
                    return StatusCode(500, new { message = "Failed to create meal plan" });
                }

                _logger.LogInformation($"✅ Created meal plan: {mealPlan.Name}");
                return Ok(mealPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating meal plan");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// PUT: api/MealPlansApi
        /// Cập nhật meal plan
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateMealPlan([FromBody] UpdateMealPlanRequest request)
        {
            try
            {
                var apiRequest = new CookMateApiService.UpdateMealPlanRequest
                {
                    MealPlanId = request.MealPlanId,
                    Name = request.Name,
                    RecipeIds = request.RecipeIds ?? new List<string>(),
                    Notes = request.Notes,
                    Date = request.Date
                };

                var mealPlan = await _cookMateApi.UpdateMealPlanAsync(apiRequest);
                
                if (mealPlan == null)
                {
                    return StatusCode(500, new { message = "Failed to update meal plan" });
                }

                _logger.LogInformation($"✅ Updated meal plan: {mealPlan.Name}");
                return Ok(mealPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meal plan");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// DELETE: api/MealPlansApi/{id}
        /// Xóa meal plan
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMealPlan(string id)
        {
            try
            {
                var success = await _cookMateApi.DeleteMealPlanAsync(id);
                
                if (!success)
                {
                    return StatusCode(500, new { message = "Failed to delete meal plan" });
                }

                _logger.LogInformation($"✅ Deleted meal plan: {id}");
                return Ok(new { message = "Meal plan deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meal plan");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #region Request Models

        public class CreateMealPlanRequest
        {
            public string Name { get; set; } = "";
            public List<string>? RecipeIds { get; set; }
            public string? Notes { get; set; }
            public DateTime Date { get; set; }
        }

        public class UpdateMealPlanRequest
        {
            public string MealPlanId { get; set; } = "";
            public string Name { get; set; } = "";
            public List<string>? RecipeIds { get; set; }
            public string? Notes { get; set; }
            public DateTime Date { get; set; }
        }

        #endregion
    }
}

