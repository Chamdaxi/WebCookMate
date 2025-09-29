using Microsoft.AspNetCore.Mvc;

namespace WebCookmate.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
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

        public IActionResult MealPlan()
        {
            return View();
        }
    }
}
