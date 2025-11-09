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
                
                // Normalize meal plans - ensure Id and Date are properly set
                var normalizedMealPlans = mealPlans.Select(plan => new
                {
                    id = plan.GetId(),
                    name = plan.Name ?? "",
                    recipeIds = plan.RecipeIds ?? new List<string>(),
                    notes = plan.Notes ?? "",
                    date = plan.Date ?? "",
                    userId = plan.UserId ?? ""
                }).ToList();
                
                _logger.LogInformation($"✅ Normalized {normalizedMealPlans.Count} meal plans for response");
                return Ok(normalizedMealPlans);
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
                    Date = request.Date.ToString("o") // Convert DateTime to ISO 8601 string
                };

                var mealPlan = await _cookMateApi.CreateMealPlanAsync(apiRequest);
                
                if (mealPlan == null)
                {
                    return StatusCode(500, new { message = "Failed to create meal plan" });
                }

                _logger.LogInformation($"✅ Created meal plan: {mealPlan.Name}");
                
                // Normalize response
                var normalizedMealPlan = new
                {
                    id = mealPlan.GetId(),
                    name = mealPlan.Name ?? "",
                    recipeIds = mealPlan.RecipeIds ?? new List<string>(),
                    notes = mealPlan.Notes ?? "",
                    date = mealPlan.Date ?? "",
                    userId = mealPlan.UserId ?? ""
                };
                
                return Ok(normalizedMealPlan);
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
                // Validate request
                if (request == null)
                {
                    _logger.LogWarning("⚠️ UpdateMealPlan: Request is null");
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.MealPlanId))
                {
                    _logger.LogWarning("⚠️ UpdateMealPlan: MealPlanId is empty");
                    return BadRequest(new { message = "MealPlanId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("⚠️ UpdateMealPlan: Name is empty");
                    return BadRequest(new { message = "Name is required" });
                }

                if (request.RecipeIds == null || request.RecipeIds.Count == 0)
                {
                    _logger.LogWarning("⚠️ UpdateMealPlan: RecipeIds is empty");
                    return BadRequest(new { message = "At least one RecipeId is required" });
                }

                _logger.LogInformation($"🔄 UpdateMealPlan: ID={request.MealPlanId}, Name={request.Name}, Date={request.Date}, RecipeIds={string.Join(",", request.RecipeIds)}");

                // Handle date - ensure it's valid
                DateTime dateToUse;
                if (request.Date == default(DateTime))
                {
                    _logger.LogWarning("⚠️ UpdateMealPlan: Date is default, using current date");
                    dateToUse = DateTime.UtcNow;
                }
                else
                {
                    dateToUse = request.Date;
                }

                string dateString = dateToUse.ToString("o"); // ISO 8601 format

                var apiRequest = new CookMateApiService.UpdateMealPlanRequest
                {
                    MealPlanId = request.MealPlanId,
                    Name = request.Name,
                    RecipeIds = request.RecipeIds,
                    Notes = request.Notes ?? "",
                    Date = dateString
                };

                _logger.LogInformation($"📤 Calling CookMateApiService.UpdateMealPlanAsync with Date: {dateString}");
                var mealPlan = await _cookMateApi.UpdateMealPlanAsync(apiRequest);
                
                if (mealPlan == null)
                {
                    _logger.LogError("❌ UpdateMealPlanAsync returned null");
                    return StatusCode(500, new { message = "Failed to update meal plan. API returned null." });
                }

                _logger.LogInformation($"✅ Updated meal plan: {mealPlan.Name} (ID: {mealPlan.GetId()})");
                
                // Normalize response
                var normalizedMealPlan = new
                {
                    id = mealPlan.GetId(),
                    name = mealPlan.Name ?? "",
                    recipeIds = mealPlan.RecipeIds ?? new List<string>(),
                    notes = mealPlan.Notes ?? "",
                    date = mealPlan.Date ?? "",
                    userId = mealPlan.UserId ?? ""
                };
                
                return Ok(normalizedMealPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating meal plan");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
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
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("⚠️ DeleteMealPlan: ID is empty");
                    return BadRequest(new { message = "Meal plan ID is required" });
                }

                _logger.LogInformation($"🔄 DeleteMealPlan: ID={id}");
                var success = await _cookMateApi.DeleteMealPlanAsync(id);
                
                if (!success)
                {
                    _logger.LogError($"❌ DeleteMealPlanAsync returned false for ID: {id}");
                    return StatusCode(500, new { message = "Failed to delete meal plan. API returned false." });
                }

                _logger.LogInformation($"✅ Deleted meal plan: {id}");
                return Ok(new { message = "Meal plan deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error deleting meal plan: {id}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/MealPlansApi/recipes?ids=1,2,3
        /// Lấy recipe details từ recipe IDs
        /// </summary>
        [HttpGet("recipes")]
        public async Task<IActionResult> GetRecipeDetails([FromQuery] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    return BadRequest(new { message = "Recipe IDs are required" });
                }

                var recipeIds = ids.Split(',')
                    .Select(id => id.Trim())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (recipeIds.Count == 0)
                {
                    return BadRequest(new { message = "At least one recipe ID is required" });
                }

                _logger.LogInformation($"🔄 GetRecipeDetails: RecipeIds={string.Join(", ", recipeIds)}");
                var recipes = await _cookMateApi.GetRecipeDetailsByStringIdsAsync(recipeIds);

                if (recipes == null)
                {
                    return StatusCode(500, new { message = "Failed to fetch recipe details from API" });
                }

                _logger.LogInformation($"✅ Fetched {recipes.Count} recipe details");
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting recipe details");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
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
            [System.ComponentModel.DataAnnotations.Required]
            public string MealPlanId { get; set; } = "";
            
            [System.ComponentModel.DataAnnotations.Required]
            public string Name { get; set; } = "";
            
            [System.ComponentModel.DataAnnotations.Required]
            public List<string> RecipeIds { get; set; } = new List<string>();
            
            public string? Notes { get; set; }
            
            [System.ComponentModel.DataAnnotations.Required]
            public DateTime Date { get; set; }
        }

        #endregion
    }
}

