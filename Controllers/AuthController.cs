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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
