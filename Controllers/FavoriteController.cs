using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: api/Favorite
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var favorites = await _context.Favorites
                                            .Where(f => f.UserId == userId)
                                            .ToListAsync();
            return Ok(new { success = true, data = favorites });
        }

        // GET: api/Favorite/check/{recipeId}
        [HttpGet("check/{recipeId}")]
        public async Task<IActionResult> CheckFavorite(string recipeId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var isFavorite = await _context.Favorites
                                            .AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId);
            return Ok(new { isFavorite = isFavorite });
        }

        // GET: api/Favorite/count
        [HttpGet("count")]
        public async Task<IActionResult> GetFavoriteCount()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var count = await _context.Favorites
                                        .CountAsync(f => f.UserId == userId);
            return Ok(new { count = count });
        }

        // POST: api/Favorite
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] Favorite favorite)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            favorite.UserId = userId;
            favorite.Id = Guid.NewGuid().ToString(); // Ensure a new ID is generated

            // Check if already favorited
            if (await _context.Favorites.AnyAsync(f => f.UserId == userId && f.RecipeId == favorite.RecipeId))
            {
                return Conflict(new { message = "Recipe already in favorites." });
            }

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFavorites), new { id = favorite.Id }, new { success = true, data = favorite });
        }

        // DELETE: api/Favorite/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(string id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (favorite == null)
            {
                return NotFound(new { message = "Favorite not found." });
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Favorite removed successfully." });
        }

        // DELETE: api/Favorite/recipe/{recipeId}
        [HttpDelete("recipe/{recipeId}")]
        public async Task<IActionResult> DeleteFavoriteByRecipeId(string recipeId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == userId);
            if (favorite == null)
            {
                return NotFound(new { message = "Favorite not found for this recipe." });
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Favorite removed successfully by recipe ID." });
        }
    }
}

