using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using demo.Services;

namespace WebCookmate.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho tất cả actions trong controller
    public class HomeController : Controller
    {
        private readonly CookMateApiService _cookMateApi;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CookMateApiService cookMateApi, ILogger<HomeController> logger)
        {
            _cookMateApi = cookMateApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Call CookMate API to get user profile
                var profile = await _cookMateApi.GetProfileAsync();
                if (profile != null)
                {
                    ViewBag.UserName = profile.Name;
                    ViewBag.UserEmail = profile.Email;
                }

                // Get meal plans from API
                var mealPlans = await _cookMateApi.GetMealPlansAsync();
                ViewBag.MealPlans = mealPlans ?? new List<CookMateApiService.MealPlan>();

                // Get today's recipes
                var recipes = await _cookMateApi.GetTodayRecipesAsync();
                ViewBag.Recipes = recipes ?? new List<CookMateApiService.Recipe>();

                _logger.LogInformation($"✅ Loaded home data from CookMate API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home data");
                ViewBag.MealPlans = new List<CookMateApiService.MealPlan>();
                ViewBag.Recipes = new List<CookMateApiService.Recipe>();
            }
            
            return View();
        }

        public IActionResult Chat()
        {
            return View();
        }

        public async Task<IActionResult> Recipes()
        {
            try
            {
                // Get recipes from CookMate API
                var recipes = await _cookMateApi.GetRecipesAsync(page: 1, limit: 20);
                ViewBag.Recipes = recipes ?? new List<CookMateApiService.Recipe>();
                
                _logger.LogInformation($"✅ Loaded {recipes?.Count ?? 0} recipes from API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipes");
                ViewBag.Recipes = new List<CookMateApiService.Recipe>();
            }
            
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult FavoriteList()
        {
            return View();
        }

        public IActionResult Pantry()
        {
            return View();
        }

        public async Task<IActionResult> MealPlan()
        {
            try
            {
                // Get meal plans from CookMate API
                var mealPlans = await _cookMateApi.GetMealPlansAsync();
                ViewBag.MealPlans = mealPlans ?? new List<CookMateApiService.MealPlan>();
                
                _logger.LogInformation($"✅ Loaded {mealPlans?.Count ?? 0} meal plans from API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading meal plans");
                ViewBag.MealPlans = new List<CookMateApiService.MealPlan>();
            }
            
            return View();
        }

        public async Task<IActionResult> ShoppingList()
        {
            try
            {
                // Get shopping list from CookMate API
                var shoppingItems = await _cookMateApi.GetShoppingListAsync();
                ViewBag.ShoppingItems = shoppingItems ?? new List<CookMateApiService.ShoppingItem>();
                
                _logger.LogInformation($"✅ Loaded {shoppingItems?.Count ?? 0} shopping items from API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shopping list");
                ViewBag.ShoppingItems = new List<CookMateApiService.ShoppingItem>();
            }
            
            return View();
        }
    }
}
