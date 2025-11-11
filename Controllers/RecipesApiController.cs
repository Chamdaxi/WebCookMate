using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace demo.Controllers
{
    /// <summary>
    /// Recipes API Controller - Proxy to CookMate API
    /// Endpoints: /api/RecipesApi
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecipesApiController : ControllerBase
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<RecipesApiController> _logger;

        public RecipesApiController(
            CookMateApiService cookMateApi,
            ILogger<RecipesApiController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/RecipesApi
        /// Get recipes with optional search, filter, and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecipes(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? difficulty = null,
            [FromQuery] int? maxTime = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            try
            {
                _logger.LogInformation($"🔄 GET /api/RecipesApi - Search: {search}, Category: {category}, Difficulty: {difficulty}, MaxTime: {maxTime}, Page: {page}, Limit: {limit}");

                // Get recipes from CookMate API
                var recipes = await _cookMateApi.GetRecipesAsync(page: page, limit: limit);

                if (recipes == null || recipes.Count == 0)
                {
                    _logger.LogWarning("⚠️ No recipes found from API");
                    return Ok(new { recipes = new List<object>(), total = 0, page = page, limit = limit });
                }

                // Apply client-side filtering (since API might not support all filters)
                var filteredRecipes = recipes.AsEnumerable();

                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    filteredRecipes = filteredRecipes.Where(r =>
                        (r.Title?.ToLower().Contains(searchLower) ?? false) ||
                        (r.Summary?.ToLower().Contains(searchLower) ?? false) ||
                        (r.DishTypes?.Any(dt => dt?.ToLower().Contains(searchLower) ?? false) ?? false)
                    );
                }

                // Category filter (map to dishTypes)
                if (!string.IsNullOrEmpty(category))
                {
                    filteredRecipes = filteredRecipes.Where(r =>
                        r.DishTypes?.Any(dt => dt?.ToLower().Contains(category.ToLower()) ?? false) ?? false
                    );
                }

                // Difficulty filter (map to readyInMinutes)
                if (!string.IsNullOrEmpty(difficulty))
                {
                    filteredRecipes = difficulty.ToLower() switch
                    {
                        "easy" => filteredRecipes.Where(r => (r.ReadyInMinutes ?? 0) <= 30),
                        "medium" => filteredRecipes.Where(r => (r.ReadyInMinutes ?? 0) > 30 && (r.ReadyInMinutes ?? 0) <= 60),
                        "hard" => filteredRecipes.Where(r => (r.ReadyInMinutes ?? 0) > 60),
                        _ => filteredRecipes
                    };
                }

                // Max time filter
                if (maxTime.HasValue && maxTime.Value > 0)
                {
                    filteredRecipes = filteredRecipes.Where(r => (r.ReadyInMinutes ?? 0) <= maxTime.Value);
                }

                var recipesList = filteredRecipes.ToList();

                // Normalize recipes for consistent response format
                var normalizedRecipes = recipesList.Select(r => new
                {
                    id = r.Id?.ToString() ?? "",
                    Id = r.Id?.ToString() ?? "",
                    title = r.Title ?? "Recipe",
                    Title = r.Title ?? "Recipe",
                    name = r.Title ?? "Recipe", // For UI compatibility
                    summary = r.Summary ?? "",
                    Summary = r.Summary ?? "",
                    description = r.Summary ?? "", // For UI compatibility
                    image = r.Image ?? "",
                    Image = r.Image ?? "",
                    readyInMinutes = r.ReadyInMinutes ?? 30,
                    ReadyInMinutes = r.ReadyInMinutes ?? 30,
                    time = r.ReadyInMinutes ?? 30, // For UI compatibility
                    servings = r.Servings ?? 4,
                    Servings = r.Servings ?? 4,
                    rating = r.SpoonacularScore ?? 0,
                    SpoonacularScore = r.SpoonacularScore ?? 0,
                    difficulty = GetDifficultyFromTime(r.ReadyInMinutes ?? 30), // Map time to difficulty
                    category = r.DishTypes?.FirstOrDefault()?.ToLower() ?? "lunch", // Default category
                    tags = r.DishTypes ?? new List<string>(),
                    Tags = r.DishTypes ?? new List<string>(),
                    dishTypes = r.DishTypes ?? new List<string>(),
                    ingredients = new List<string>(), // API might not have ingredients in list
                    extendedIngredients = r.ExtendedIngredients ?? new List<CookMateApiService.ExtendedIngredient>()
                }).ToList();

                _logger.LogInformation($"✅ Returned {normalizedRecipes.Count} recipes");

                return Ok(new
                {
                    recipes = normalizedRecipes,
                    total = normalizedRecipes.Count,
                    page = page,
                    limit = limit,
                    hasMore = recipes.Count >= limit
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting recipes");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/RecipesApi/search?query=...
        /// Search recipes by query
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchRecipes(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            try
            {
                _logger.LogInformation($"🔄 GET /api/RecipesApi/search - Query: {query}, Page: {page}, Limit: {limit}");

                // Use GetRecipes with search parameter
                return await GetRecipes(search: query, page: page, limit: limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error searching recipes");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/RecipesApi/{id}
        /// Get recipe details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(string id)
        {
            try
            {
                _logger.LogInformation($"🔄 GET /api/RecipesApi/{id}");

                // Get recipe details using bulk endpoint
                var recipes = await _cookMateApi.GetRecipeDetailsByStringIdsAsync(new List<string> { id });

                if (recipes == null || recipes.Count == 0)
                {
                    return NotFound(new { message = "Recipe not found" });
                }

                var recipe = recipes.First();
                var normalizedRecipe = new
                {
                    id = recipe.Id?.ToString() ?? "",
                    title = recipe.Title ?? "Recipe",
                    name = recipe.Title ?? "Recipe",
                    summary = recipe.Summary ?? "",
                    description = recipe.Summary ?? "",
                    image = recipe.Image ?? "",
                    readyInMinutes = recipe.ReadyInMinutes ?? 30,
                    time = recipe.ReadyInMinutes ?? 30,
                    servings = recipe.Servings ?? 4,
                    rating = recipe.SpoonacularScore ?? 0,
                    difficulty = GetDifficultyFromTime(recipe.ReadyInMinutes ?? 30),
                    category = recipe.DishTypes?.FirstOrDefault()?.ToLower() ?? "lunch",
                    tags = recipe.DishTypes ?? new List<string>(),
                    ingredients = recipe.ExtendedIngredients?.Select(ing => ing.Name ?? "").ToList() ?? new List<string>(),
                    extendedIngredients = recipe.ExtendedIngredients ?? new List<CookMateApiService.ExtendedIngredient>(),
                    instructions = recipe.Instructions ?? "",
                    dishTypes = recipe.DishTypes ?? new List<string>()
                };

                return Ok(normalizedRecipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting recipe {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Helper: Map cooking time to difficulty level
        /// </summary>
        private string GetDifficultyFromTime(int minutes)
        {
            return minutes switch
            {
                <= 30 => "easy",
                <= 60 => "medium",
                _ => "hard"
            };
        }
    }
}

