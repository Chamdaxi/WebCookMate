using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace demo.Controllers
{
    /// <summary>
    /// Auth Controller - ONLY Google OAuth
    /// Calls CookMate API Server: https://cookm8.vercel.app
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthController> _logger;
        private const string API_BASE_URL = "https://cookm8.vercel.app";

        public AuthController(
            IHttpClientFactory httpClientFactory,
            ILogger<AuthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        #region Google OAuth → CookMate API

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse"),
                Items = { { "prompt", "select_account" } }
            };
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                
                if (!result.Succeeded)
                {
                    _logger.LogError("Google authentication failed");
                    TempData["Error"] = "Google authentication failed";
                    return RedirectToAction("Login");
                }

                var claims = result.Principal.Claims.ToList();
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var googleUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email not found in Google response");
                    TempData["Error"] = "Email not found in Google response";
                    return RedirectToAction("Login");
                }

                _logger.LogInformation($"📧 Google auth successful: {email}");

                // CALL CookMate API Server
                var httpClient = _httpClientFactory.CreateClient();
                var requestData = new
                {
                    googleUserId = googleUserId ?? email,
                    email = email,
                    name = name ?? email.Split('@')[0],
                    avatar = picture ?? ""
                };

                var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🌐 Calling CookMate API: POST {API_BASE_URL}/api/auth/google");
                _logger.LogInformation($"📤 Request: {json}");
                
                var response = await httpClient.PostAsync($"{API_BASE_URL}/api/auth/google", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Response status: {response.StatusCode}");
                _logger.LogInformation($"📥 Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<CookMateAuthResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Token))
                    {
                        // Lưu JWT token vào session
                        HttpContext.Session.SetString("access_token", apiResponse.Token);
                        HttpContext.Session.SetString("user_email", apiResponse.User?.Email ?? email);
                        HttpContext.Session.SetString("user_name", apiResponse.User?.Name ?? name ?? "");
                        HttpContext.Session.SetString("user_id", apiResponse.User?.Id ?? "");

                        _logger.LogInformation($"✅ Authentication successful!");
                        _logger.LogInformation($"🔑 Token: {apiResponse.Token.Substring(0, Math.Min(50, apiResponse.Token.Length))}...");
                        _logger.LogInformation($"👤 User: {apiResponse.User?.Email} (ID: {apiResponse.User?.Id})");
                        
                        // Create local cookie
                        var userClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, email),
                            new Claim(ClaimTypes.Name, name ?? email),
                            new Claim("access_token", apiResponse.Token),
                            new Claim("user_id", apiResponse.User?.Id ?? "")
                        };

                        var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme, 
                            claimsPrincipal);

                        TempData["Success"] = $"Chào mừng {name}! Đăng nhập thành công.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogError($"❌ API response missing token: {responseContent}");
                        TempData["Error"] = "Failed to get authentication token from server";
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    _logger.LogError($"❌ API call failed: {response.StatusCode} - {responseContent}");
                    TempData["Error"] = $"Authentication failed: {response.StatusCode}";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during Google authentication");
                TempData["Error"] = "Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại.";
                return RedirectToAction("Login");
            }
        }

        #endregion

        #region OTP Authentication → CookMate API

        [HttpGet]
        public IActionResult OTPLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return Json(new { success = false, message = "Email là bắt buộc" });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var requestData = new { email = request.Email };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🌐 Sending OTP to: {request.Email}");

                var response = await httpClient.PostAsync($"{API_BASE_URL}/api/auth/otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Lưu email vào session
                    HttpContext.Session.SetString("otp_email", request.Email);

                    _logger.LogInformation($"✅ OTP sent successfully to {request.Email}");
                    return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn" });
                }
                else
                {
                    _logger.LogError($"❌ Failed to send OTP: {responseContent}");
                    return Json(new { success = false, message = "Không thể gửi OTP. Vui lòng thử lại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
                {
                    return Json(new { success = false, message = "Email và OTP là bắt buộc" });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var requestData = new { email = request.Email, otp = request.Otp };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🌐 Verifying OTP for: {request.Email}");

                var response = await httpClient.PostAsync($"{API_BASE_URL}/api/auth/otp/verify", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<CookMateAuthResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Token))
                    {
                        // Lưu JWT token vào session
                        HttpContext.Session.SetString("access_token", apiResponse.Token);
                        HttpContext.Session.SetString("user_email", apiResponse.User?.Email ?? request.Email);
                        HttpContext.Session.SetString("user_name", apiResponse.User?.Name ?? request.Email.Split('@')[0]);
                        HttpContext.Session.SetString("user_id", apiResponse.User?.Id ?? "");

                        _logger.LogInformation($"✅ OTP verified successfully for {request.Email}");

                        // Create local cookie
                        var userClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, request.Email),
                            new Claim(ClaimTypes.Name, apiResponse.User?.Name ?? request.Email),
                            new Claim("access_token", apiResponse.Token),
                            new Claim("user_id", apiResponse.User?.Id ?? "")
                        };

                        var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                        // Clear OTP session
                        HttpContext.Session.Remove("otp_email");

                        return Json(new { 
                            success = true, 
                            message = "Đăng nhập thành công!",
                            redirectUrl = Url.Action("Index", "Home")
                        });
                    }
                }

                return Json(new { success = false, message = "Mã OTP không đúng hoặc đã hết hạn" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xác thực OTP" });
            }
        }

        #endregion

        [HttpGet]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Sign out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("👋 User logged out");
            TempData["Success"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        #region Request/Response Models

        public class SendOTPRequest
        {
            public string Email { get; set; } = "";
        }

        public class VerifyOTPRequest
        {
            public string Email { get; set; } = "";
            public string Otp { get; set; } = "";
        }

        private class CookMateAuthResponse
        {
            public string Token { get; set; } = "";
            public string Message { get; set; } = "";
            public UserInfo? User { get; set; }
        }

        private class UserInfo
        {
            public string Id { get; set; } = "";
            public string Email { get; set; } = "";
            public string Name { get; set; } = "";
            public string Avatar { get; set; } = "";
            public List<string>? DietaryPreferences { get; set; }
        }

        #endregion
    }
}
