using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace WebCookmate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Get access token from session
            var accessToken = HttpContext.Session.GetString("access_token");
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Get meal plans and recipes from API
                var mealPlansResponse = await _apiService.GetMealPlansAsync(accessToken);
                var recipesResponse = await _apiService.GetRecipesAsync(accessToken);
                
                ViewBag.MealPlans = mealPlansResponse.Success ? mealPlansResponse.Data : new List<MealPlan>();
                ViewBag.Recipes = recipesResponse.Success ? recipesResponse.Data : new List<Recipe>();
            }
            else
            {
                ViewBag.MealPlans = new List<MealPlan>();
                ViewBag.Recipes = new List<Recipe>();
            }
            
            return View();
        }

        public IActionResult Chat()
        {
            return View();
        }

        public IActionResult Recipes()
        {
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

        public async Task<IActionResult> MealPlan()
        {
            // Get access token from session
            var accessToken = HttpContext.Session.GetString("access_token");
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Get meal plans from API
                var mealPlansResponse = await _apiService.GetMealPlansAsync(accessToken);
                ViewBag.MealPlans = mealPlansResponse.Success ? mealPlansResponse.Data : new List<MealPlan>();
            }
            else
            {
                ViewBag.MealPlans = new List<MealPlan>();
            }
            
            return View();
        }


    }
}
