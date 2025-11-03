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

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApiService apiService, OTPService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _apiService = apiService;
            _otpService = otpService;
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

            // Tạo user claims
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name ?? email),
                new Claim(ClaimTypes.NameIdentifier, email)
            };

            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            // Đăng nhập user với Identity scheme
            await HttpContext.SignInAsync("Identity.Application", userPrincipal);

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
            // Tạm thời comment để test không cần đăng nhập
            // if (!User.Identity.IsAuthenticated)
            // {
            //     return RedirectToAction("Login");
            // }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Tạo user demo để test nếu chưa đăng nhập
                user = new ApplicationUser
                {
                    UserName = "test@example.com",
                    Email = "test@example.com",
                    FullName = "Test User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };
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

        // Delete Account Actions
        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            // Tạm thời comment để test không cần đăng nhập
            // if (!User.Identity.IsAuthenticated)
            // {
            //     return RedirectToAction("Login");
            // }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Tạo user demo để test nếu chưa đăng nhập
                user = new ApplicationUser
                {
                    UserName = "test@example.com",
                    Email = "test@example.com",
                    FullName = "Test User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteAccount(string password)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu nếu user có mật khẩu
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword && !string.IsNullOrWhiteSpace(password))
            {
                var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
                if (!passwordCheck.Succeeded)
                {
                    TempData["Error"] = "Mật khẩu không đúng. Vui lòng thử lại.";
                    return View("DeleteAccount", user);
                }
            }

            // Xóa tài khoản
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["Success"] = "Tài khoản đã được xóa thành công.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa tài khoản: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return View("DeleteAccount", user);
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