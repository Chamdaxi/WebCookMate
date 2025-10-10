using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using demo.Models;
using demo.Data;

namespace demo.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
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

        [HttpPost]
        public IActionResult SendOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập email";
                return RedirectToAction("Login");
            }

            // Generate OTP (6 digits)
            var otp = new Random().Next(100000, 999999).ToString();
            
            // Store OTP in session (in real app, store in database with expiration)
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTPEmail", email);
            HttpContext.Session.SetString("OTPTime", DateTime.Now.AddMinutes(5).ToString());

            // In real app, send email here
            TempData["Success"] = $"Mã OTP đã được gửi đến {email}. Mã OTP: {otp} (Demo)";
            
            return RedirectToAction("VerifyOTP");
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            var email = HttpContext.Session.GetString("OTPEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otp)
        {
            var storedOTP = HttpContext.Session.GetString("OTP");
            var email = HttpContext.Session.GetString("OTPEmail");
            var otpTime = HttpContext.Session.GetString("OTPTime");

            if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên OTP đã hết hạn";
                return RedirectToAction("Login");
            }

            if (DateTime.Now > DateTime.Parse(otpTime ?? DateTime.Now.ToString()))
            {
                TempData["Error"] = "Mã OTP đã hết hạn";
                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("OTPEmail");
                HttpContext.Session.Remove("OTPTime");
                return RedirectToAction("Login");
            }

            if (otp != storedOTP)
            {
                TempData["Error"] = "Mã OTP không đúng";
                return View();
            }

            // OTP verified successfully
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create new user if doesn't exist
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = email.Split('@')[0] // Use email prefix as name
                };
                await _userManager.CreateAsync(user);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            
            // Clear OTP session
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTPEmail");
            HttpContext.Session.Remove("OTPTime");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateTestUser()
        {
            // Tạo user test
            var testUser = new ApplicationUser
            {
                UserName = "test@example.com",
                Email = "test@example.com",
                FullName = "Test User",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(testUser, "Test123!");
            if (result.Succeeded)
            {
                TempData["Success"] = "Test user created successfully! Email: test@example.com, Password: Test123!";
            }
            else
            {
                TempData["Error"] = "Failed to create test user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> CreateManhUser()
        {
            // Tạo user manh123@gmail.com với password đơn giản
            var manhUser = new ApplicationUser
            {
                UserName = "manh123@gmail.com",
                Email = "manh123@gmail.com",
                FullName = "Manh User",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(manhUser, "123456");
            if (result.Succeeded)
            {
                TempData["Success"] = "Manh user created successfully! Email: manh123@gmail.com, Password: 123456";
            }
            else
            {
                TempData["Error"] = "Failed to create manh user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Auth/UserProfile")]
        public async Task<IActionResult> UserProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Không thể đăng nhập với Google";
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Nếu user chưa tồn tại, tạo user mới
            var user = new ApplicationUser
            {
                UserName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                FullName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? 
                          info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value?.Split('@')[0]
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["Error"] = "Không thể tạo tài khoản với Google";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Auth/EditProfile")]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }

        [HttpPost]
        [Route("Auth/EditProfile")]
        public async Task<IActionResult> EditProfile(string fullName, string email, string phoneNumber)
        {
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
    }
}
