using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using demo.Services;

namespace demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ApiService _apiService;

        public ApiController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet("user/profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("access_token");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Access token not found");
                }

                var response = await _apiService.GetUserProfileAsync(accessToken);
                if (response.Success)
                {
                    return Ok(response.Data);
                }
                else
                {
                    return BadRequest(response.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("meal-plans")]
        [Authorize]
        public async Task<IActionResult> GetMealPlans()
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("access_token");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Access token not found");
                }

                var response = await _apiService.GetMealPlansAsync(accessToken);
                if (response.Success)
                {
                    return Ok(response.Data);
                }
                else
                {
                    return BadRequest(response.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("recipes")]
        [Authorize]
        public async Task<IActionResult> GetRecipes([FromQuery] string category = null)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("access_token");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Access token not found");
                }

                var response = await _apiService.GetRecipesAsync(accessToken, category);
                if (response.Success)
                {
                    return Ok(response.Data);
                }
                else
                {
                    return BadRequest(response.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "Bearer token authentication successful!",
                user = User.Identity.Name,
                timestamp = DateTime.Now
            });
        }
    }
}




