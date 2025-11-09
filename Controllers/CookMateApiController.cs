using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using demo.Models;
using demo.Data;
using demo.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace demo.Controllers
{
    /// <summary>
    /// CookMate API Controller - Compatible with https://cookm8.vercel.app/api-docs
    /// </summary>
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class CookMateApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly ILogger<CookMateApiController> _logger;

        public CookMateApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            TokenService tokenService,
            ILogger<CookMateApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        #region Authentication Endpoints

        /// <summary>
        /// Google OAuth Authentication
        /// POST /api/auth/google
        /// </summary>
        [HttpPost("auth/google")]
        public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new { message = "ID token is required" });
                }

                // Validate Google token and extract user info
                var payload = await ValidateGoogleTokenAsync(request.IdToken);
                if (payload == null)
                {
                    return Unauthorized(new { message = "Invalid Google token" });
                }

                var email = payload.Email;
                var name = payload.Name;

                // Find or create user
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name ?? email.Split('@')[0],
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return StatusCode(500, new { message = "Failed to create user" });
                    }
                }

                // Sign in user with Cookie-based authentication (for Web UI)
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                // Generate JWT token (for API clients)
                var token = _tokenService.GenerateJwtToken(user);

                return Ok(new
                {
                    access_token = token,
                    token_type = "Bearer",
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.FullName,
                        avatar = ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google authentication failed");
                return StatusCode(500, new { message = "Authentication failed" });
            }
        }

        /// <summary>
        /// Email/Password Login
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Sign in user with Cookie-based authentication (for Web UI)
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                // Generate JWT token (for API clients)
                var token = _tokenService.GenerateJwtToken(user);

                return Ok(new
                {
                    access_token = token,
                    token_type = "Bearer",
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.FullName,
                        avatar = ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, new { message = "Login failed" });
            }
        }

        /// <summary>
        /// Register new user
        /// POST /api/auth/register
        /// </summary>
        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.Name ?? request.Email.Split('@')[0],
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Failed to create user", errors = result.Errors });
                }

                // Sign in user with Cookie-based authentication (for Web UI)
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                // Generate JWT token (for API clients)
                var token = _tokenService.GenerateJwtToken(user);

                return Ok(new
                {
                    access_token = token,
                    token_type = "Bearer",
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.FullName,
                        avatar = ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                return StatusCode(500, new { message = "Registration failed" });
            }
        }

        #endregion

        #region User Profile Endpoints

        /// <summary>
        /// Get current user profile
        /// GET /api/user/profile
        /// </summary>
        [HttpGet("user/profile")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.FullName,
                    avatar = "",
                    phone = user.PhoneNumber ?? "",
                    createdAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user profile");
                return StatusCode(500, new { message = "Failed to get profile" });
            }
        }

        /// <summary>
        /// Update user profile
        /// PUT /api/user/profile
        /// </summary>
        [HttpPut("user/profile")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Update user info
                if (!string.IsNullOrEmpty(request.Name))
                    user.FullName = request.Name;
                if (!string.IsNullOrEmpty(request.Phone))
                    user.PhoneNumber = request.Phone;
                // Avatar not supported in current model

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Failed to update profile" });
                }

                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.FullName,
                    avatar = "",
                    phone = user.PhoneNumber ?? ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user profile");
                return StatusCode(500, new { message = "Failed to update profile" });
            }
        }

        #endregion

        #region Ingredient Endpoints

        /// <summary>
        /// Get all ingredients (pantry)
        /// GET /api/ingredients
        /// </summary>
        [HttpGet("ingredients")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        public async Task<IActionResult> GetIngredients()
        {
            try
            {
                var ingredients = await _context.Ingredients
                    .Include(i => i.Category)
                    .OrderBy(i => i.Name)
                    .Select(i => new
                    {
                        id = i.Id,
                        name = i.Name,
                        quantity = i.Quantity,
                        unit = i.Unit,
                        categoryId = i.CategoryId,
                        category = i.Category != null ? new
                        {
                            id = i.Category.Id,
                            name = i.Category.Name,
                            icon = i.Category.Icon
                        } : null,
                        expiryDate = i.ExpiryDate,
                        createdAt = i.CreatedAt,
                        notes = i.Notes,
                        userId = i.UserId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = ingredients,
                    count = ingredients.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ingredients");
                return StatusCode(500, new { message = "Failed to get ingredients" });
            }
        }

        /// <summary>
        /// Add new ingredient
        /// POST /api/ingredients
        /// </summary>
        // DEPRECATED: This endpoint is replaced by IngredientApiController
        // [HttpPost("ingredients")]
        // [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        [Obsolete("Use /api/IngredientApi instead")]
        public async Task<IActionResult> AddIngredient_OLD([FromBody] CreateIngredientRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var ingredient = new Ingredient
                {
                    Name = request.Name,
                    Quantity = request.Quantity,
                    Unit = request.Unit,
                    CategoryId = request.CategoryId.ToString(),
                    ExpiryDate = request.ExpiryDate,
                    CreatedAt = DateTime.UtcNow,
                    Notes = request.Notes,
                    UserId = userId
                };

                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();

                var category = await _context.IngredientCategories.FindAsync(request.CategoryId.ToString());

                return CreatedAtAction(nameof(GetIngredients), new { id = ingredient.Id }, new
                {
                    id = ingredient.Id,
                    name = ingredient.Name,
                    quantity = ingredient.Quantity,
                    unit = ingredient.Unit,
                    categoryId = ingredient.CategoryId,
                    category = category != null ? new
                    {
                        id = category.Id,
                        name = category.Name,
                        icon = category.Icon
                    } : null,
                    expiryDate = ingredient.ExpiryDate,
                    createdAt = ingredient.CreatedAt,
                    notes = ingredient.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add ingredient");
                return StatusCode(500, new { message = "Failed to add ingredient" });
            }
        }

        /// <summary>
        /// Update ingredient
        /// PUT /api/ingredients/{id}
        /// </summary>
        [HttpPut("ingredients/{id}")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] UpdateIngredientRequest request)
        {
            try
            {
                var ingredient = await _context.Ingredients.FindAsync(id);
                if (ingredient == null)
                {
                    return NotFound(new { message = "Ingredient not found" });
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.Name))
                    ingredient.Name = request.Name;
                if (request.Quantity.HasValue)
                    ingredient.Quantity = request.Quantity.Value;
                if (!string.IsNullOrEmpty(request.Unit))
                    ingredient.Unit = request.Unit;
                if (request.CategoryId.HasValue)
                    ingredient.CategoryId = request.CategoryId.Value.ToString();
                if (request.ExpiryDate.HasValue)
                    ingredient.ExpiryDate = request.ExpiryDate;
                if (!string.IsNullOrEmpty(request.Notes))
                    ingredient.Notes = request.Notes;

                ingredient.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var category = await _context.IngredientCategories.FindAsync(ingredient.CategoryId);

                return Ok(new
                {
                    id = ingredient.Id,
                    name = ingredient.Name,
                    quantity = ingredient.Quantity,
                    unit = ingredient.Unit,
                    categoryId = ingredient.CategoryId,
                    category = category != null ? new
                    {
                        id = category.Id,
                        name = category.Name,
                        icon = category.Icon
                    } : null,
                    expiryDate = ingredient.ExpiryDate,
                    createdAt = ingredient.CreatedAt,
                    updatedAt = ingredient.UpdatedAt,
                    notes = ingredient.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update ingredient");
                return StatusCode(500, new { message = "Failed to update ingredient" });
            }
        }

        /// <summary>
        /// Delete ingredient
        /// DELETE /api/ingredients/{id}
        /// </summary>
        [HttpDelete("ingredients/{id}")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            try
            {
                var ingredient = await _context.Ingredients.FindAsync(id);
                if (ingredient == null)
                {
                    return NotFound(new { message = "Ingredient not found" });
                }

                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Ingredient deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete ingredient");
                return StatusCode(500, new { message = "Failed to delete ingredient" });
            }
        }

        /// <summary>
        /// Get ingredient categories
        /// GET /api/ingredients/categories
        /// </summary>
        [HttpGet("ingredients/categories")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.IngredientCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        icon = c.Icon,
                        description = c.Description
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = categories,
                    count = categories.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get categories");
                return StatusCode(500, new { message = "Failed to get categories" });
            }
        }

        #endregion

        #region Helper Methods

        private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { "YOUR_GOOGLE_CLIENT_ID" } // Replace with actual client ID
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

    #region Request/Response Models

    public class GoogleAuthRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
    }

    public class CreateIngredientRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateIngredientRequest
    {
        public string? Name { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class GoogleJsonWebSignature
    {
        public class Payload
        {
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Picture { get; set; } = string.Empty;
        }

        public class ValidationSettings
        {
            public string[] Audience { get; set; } = Array.Empty<string>();
        }

        public static Task<Payload?> ValidateAsync(string idToken, ValidationSettings settings)
        {
            // Demo implementation - in production, use Google.Apis.Auth package
            return Task.FromResult<Payload?>(new Payload
            {
                Email = "demo@example.com",
                Name = "Demo User",
                Picture = ""
            });
        }
    }

    #endregion
}

