using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;
using System.Security.Claims;

namespace demo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IngredientController> _logger;

        public IngredientController(
            ApplicationDbContext context,
            ILogger<IngredientController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: api/Ingredient
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients()
        {
            try
            {
                // Return ALL ingredients (shared for all users)
                var ingredients = await _context.Ingredients
                    .Include(i => i.Category)
                    .OrderBy(i => i.Name)
                    .ToListAsync();

                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ingredients");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Ingredient/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Ingredient>> GetIngredient(string id)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var ingredient = await _context.Ingredients
                    .Include(i => i.Category)
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

                if (ingredient == null)
                {
                    return NotFound("Ingredient not found");
                }

                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ingredient {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Ingredient/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredientsByCategory(string categoryId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var ingredients = await _context.Ingredients
                    .Include(i => i.Category)
                    .Where(i => i.UserId == userId && i.CategoryId == categoryId)
                    .OrderBy(i => i.Name)
                    .ToListAsync();

                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ingredients for category {categoryId}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Ingredient
        [HttpPost]
        public async Task<ActionResult<Ingredient>> CreateIngredient([FromBody] Ingredient ingredient)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify category exists
                var categoryExists = await _context.IngredientCategories.AnyAsync(c => c.Id == ingredient.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest("Invalid category ID");
                }

                ingredient.Id = Guid.NewGuid().ToString();
                ingredient.UserId = userId;
                ingredient.CreatedAt = DateTime.Now;
                ingredient.UpdatedAt = null;

                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();

                // Load category for response
                await _context.Entry(ingredient).Reference(i => i.Category).LoadAsync();

                _logger.LogInformation($"Created ingredient: {ingredient.Name} for user {userId}");

                return CreatedAtAction(nameof(GetIngredient), new { id = ingredient.Id }, ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ingredient");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Ingredient/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIngredient(string id, [FromBody] Ingredient ingredient)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                if (id != ingredient.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingIngredient = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

                if (existingIngredient == null)
                {
                    return NotFound("Ingredient not found");
                }

                // Verify category exists
                var categoryExists = await _context.IngredientCategories.AnyAsync(c => c.Id == ingredient.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest("Invalid category ID");
                }

                existingIngredient.Name = ingredient.Name;
                existingIngredient.CategoryId = ingredient.CategoryId;
                existingIngredient.Quantity = ingredient.Quantity;
                existingIngredient.Unit = ingredient.Unit;
                existingIngredient.ExpiryDate = ingredient.ExpiryDate;
                existingIngredient.Notes = ingredient.Notes;
                existingIngredient.ImageUrl = ingredient.ImageUrl;
                existingIngredient.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Load category for response
                await _context.Entry(existingIngredient).Reference(i => i.Category).LoadAsync();

                _logger.LogInformation($"Updated ingredient: {ingredient.Name} for user {userId}");

                return Ok(existingIngredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating ingredient {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Ingredient/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(string id)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var ingredient = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

                if (ingredient == null)
                {
                    return NotFound("Ingredient not found");
                }

                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted ingredient: {ingredient.Name} for user {userId}");

                return Ok(new { message = "Ingredient deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ingredient {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Ingredient/expiring
        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetExpiringIngredients([FromQuery] int days = 7)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var expiryDate = DateTime.Now.AddDays(days);

                var ingredients = await _context.Ingredients
                    .Include(i => i.Category)
                    .Where(i => i.UserId == userId && i.ExpiryDate != null && i.ExpiryDate <= expiryDate)
                    .OrderBy(i => i.ExpiryDate)
                    .ToListAsync();

                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring ingredients");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

