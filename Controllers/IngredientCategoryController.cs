using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;

namespace demo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IngredientCategoryController> _logger;

        public IngredientCategoryController(
            ApplicationDbContext context,
            ILogger<IngredientCategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/IngredientCategory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientCategory>>> GetCategories()
        {
            try
            {
                var categories = await _context.IngredientCategories
                    .Include(c => c.Ingredients)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ingredient categories");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/IngredientCategory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientCategory>> GetCategory(string id)
        {
            try
            {
                var category = await _context.IngredientCategories
                    .Include(c => c.Ingredients)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound("Category not found");
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting category {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/IngredientCategory
        [HttpPost]
        public async Task<ActionResult<IngredientCategory>> CreateCategory([FromBody] IngredientCategory category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if category with same name already exists
                var existingCategory = await _context.IngredientCategories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                {
                    return Conflict("Category with this name already exists");
                }

                category.Id = Guid.NewGuid().ToString();
                category.CreatedAt = DateTime.Now;
                category.UpdatedAt = null;

                _context.IngredientCategories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created ingredient category: {category.Name}");

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ingredient category");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/IngredientCategory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] IngredientCategory category)
        {
            try
            {
                if (id != category.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingCategory = await _context.IngredientCategories.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound("Category not found");
                }

                // Check if another category with same name exists
                var duplicateCategory = await _context.IngredientCategories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id);

                if (duplicateCategory != null)
                {
                    return Conflict("Another category with this name already exists");
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.Icon = category.Icon;
                existingCategory.Color = category.Color;
                existingCategory.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated ingredient category: {category.Name}");

                return Ok(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/IngredientCategory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var category = await _context.IngredientCategories
                    .Include(c => c.Ingredients)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound("Category not found");
                }

                // Check if category has ingredients
                if (category.Ingredients.Any())
                {
                    return BadRequest($"Cannot delete category. It has {category.Ingredients.Count} ingredient(s). Please delete or move the ingredients first.");
                }

                _context.IngredientCategories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted ingredient category: {category.Name}");

                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}


