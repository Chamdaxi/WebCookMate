using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using demo.Models;
using demo.Data;
using demo.Services;
using System.Text.Json;
using System.Security.Claims;

namespace demo.Controllers
{
    public class AuthController : Controller
    {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApiService _apiService;
    private readonly OTPService _otpService;
    private readonly FavoriteSeedService _favoriteSeedService;

        public AuthController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ApiService apiService, 
            OTPService otpService,
            FavoriteSeedService favoriteSeedService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _apiService = apiService;
            _otpService = otpService;
            _favoriteSeedService = favoriteSeedService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Google OAuth Actions
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            // Redirect trực tiếp đến Google với prompt=select_account để hiển thị danh sách tài khoản
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
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
            {
                TempData["Error"] = "Google authentication failed";
                return RedirectToAction("Login");
            }

            var claims = result.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email not found in Google response";
                return RedirectToAction("Login");
            }

            // Tìm hoặc tạo user trong database
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Tạo user mới
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name ?? email.Split('@')[0],
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "Không thể tạo tài khoản";
                    return RedirectToAction("Login");
                }
            }

            // Đăng nhập user bằng SignInManager
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Tự động thêm món ăn yêu thích mẫu nếu user chưa có món nào
            await _favoriteSeedService.EnsureUserHasFavorites(user.Id);

            // Redirect đến trang chủ
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Tự động thêm món ăn yêu thích mẫu nếu user chưa có món nào
                await _favoriteSeedService.EnsureUserHasFavorites(user.Id);
                
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Email hoặc mật khẩu không đúng";
            return View();
        }

        [HttpGet]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Auth/UserProfile")]
        public async Task<IActionResult> UserProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(string fullName, string email, string phoneNumber)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Cập nhật thông tin
            user.FullName = fullName;
            user.Email = email;
            user.NormalizedEmail = email.ToUpper();
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("UserProfile");
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return View(user);
            }
        }

        // OTP Login Actions
        [HttpGet]
        public IActionResult OTPLogin()
        {
            // Redirect to RealOTP instead
            return RedirectToAction("RealOTP");
        }

        // GET: Auth/SimpleOTP - Trang OTP đơn giản mới
        public IActionResult SimpleOTP()
        {
            return View();
        }

        // GET: Auth/RealOTP - Trang OTP thực sự với Email/SMS
        public IActionResult RealOTP()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOTP(string email, string? phoneNumber = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Vui lòng nhập email" });
                }

                // Sử dụng OTP Service thực sự
                var otpResult = await _otpService.SendOTPAsync(email, phoneNumber);
                
                if (otpResult.Success)
                {
                    // Lưu email vào session để verify sau
                    HttpContext.Session.SetString("otp_email", email);
                    
                    // Chỉ trả về otpCode nếu thực sự là demo mode
                    if (!string.IsNullOrEmpty(otpResult.OTPCode) && otpResult.Message.Contains("Demo"))
                    {
                        return Json(new { 
                            success = true, 
                            message = $"Mã OTP: {otpResult.OTPCode} (Demo mode)",
                            otpCode = otpResult.OTPCode
                        });
                    }
                    else
                    {
                        return Json(new { 
                            success = true, 
                            message = otpResult.Message
                        });
                    }
                }
                else
                {
                    return Json(new { success = false, message = otpResult.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendOTP Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi OTP!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otpCode)
        {
            try
            {
                if (string.IsNullOrEmpty(otpCode))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã OTP" });
                }

                // Lấy email từ session
                var email = HttpContext.Session.GetString("otp_email");
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Vui lòng gửi OTP trước" });
                }

                // Sử dụng OTP Service để verify
                var verifyResult = await _otpService.VerifyOTPAsync(email, otpCode);
                
                if (verifyResult.Success)
                {
                    // Tìm hoặc tạo user
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        // Tạo user mới
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            FullName = email.Split('@')[0],
                            EmailConfirmed = true,
                            CreatedAt = DateTime.Now
                        };

                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            return Json(new { success = false, message = "Không thể tạo tài khoản" });
                        }
                    }

                    // Đăng nhập user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Tự động thêm món ăn yêu thích mẫu nếu user chưa có món nào
                    await _favoriteSeedService.EnsureUserHasFavorites(user.Id);

                    // Xóa session OTP
                    HttpContext.Session.Remove("otp_email");

                    return Json(new { 
                        success = true, 
                        message = "Đăng nhập thành công!",
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }
                else
                {
                    return Json(new { success = false, message = verifyResult.Message });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xác thực OTP" });
            }
        }
    }
}