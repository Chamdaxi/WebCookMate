using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using demo.Models;
using demo.Data;
using demo.Services;

namespace demo.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ICookMateApiService _apiService;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ICookMateApiService apiService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _apiService = apiService;
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

            // Kiểm tra tài khoản đã bị xóa
            if (user.IsDeleted)
            {
                TempData["Error"] = "Tài khoản của bạn đã bị xóa. Vui lòng khôi phục tài khoản để tiếp tục sử dụng.";
                TempData["DeletedEmail"] = email;
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

            // Send email OTP
            try
            {
                _emailSender.SendEmail(email, "Mã khôi phục tài khoản CookMate", $"Mã OTP của bạn là: {otp}. Mã có hiệu lực trong 5 phút.");
                TempData["Success"] = $"Mã OTP đã được gửi đến {email}.";
            }
            catch
            {
                TempData["Error"] = "Không gửi được email. Vui lòng thử lại sau.";
                return RedirectToAction("Login");
            }
            
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

        // Forgot password - request email
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Vui lòng nhập email";
                return View();
            }

            // Generate OTP for recovery
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("RecoveryOTP", otp);
            HttpContext.Session.SetString("RecoveryEmail", email);
            HttpContext.Session.SetString("RecoveryOTPTime", DateTime.Now.AddMinutes(10).ToString());

            try
            {
                _emailSender.SendEmail(email, "Mã khôi phục mật khẩu CookMate", $"Mã OTP khôi phục của bạn là: {otp}. Mã có hiệu lực trong 10 phút.");
                TempData["Success"] = "Đã gửi OTP khôi phục. Vui lòng kiểm tra email.";
            }
            catch
            {
                TempData["Error"] = "Không gửi được email. Vui lòng thử lại.";
                return View();
            }

            return RedirectToAction("VerifyRecoveryOTP");
        }

        [HttpGet]
        public IActionResult VerifyRecoveryOTP()
        {
            var email = HttpContext.Session.GetString("RecoveryEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            return View();
        }

        [HttpPost]
        public IActionResult VerifyRecoveryOTP(string otp)
        {
            var stored = HttpContext.Session.GetString("RecoveryOTP");
            var email = HttpContext.Session.GetString("RecoveryEmail");
            var timeStr = HttpContext.Session.GetString("RecoveryOTPTime");

            if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên khôi phục đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            if (DateTime.Now > DateTime.Parse(timeStr ?? DateTime.Now.ToString()))
            {
                TempData["Error"] = "Mã OTP đã hết hạn.";
                HttpContext.Session.Remove("RecoveryOTP");
                HttpContext.Session.Remove("RecoveryEmail");
                HttpContext.Session.Remove("RecoveryOTPTime");
                return RedirectToAction("ForgotPassword");
            }

            if (otp != stored)
            {
                TempData["Error"] = "Mã OTP không đúng.";
                return View();
            }

            // Mark verified and proceed to reset password
            HttpContext.Session.SetString("RecoveryVerified", "true");
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var verified = HttpContext.Session.GetString("RecoveryVerified");
            var email = HttpContext.Session.GetString("RecoveryEmail");
            if (verified != "true" || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("RecoveryEmail");
            var verified = HttpContext.Session.GetString("RecoveryVerified");
            if (verified != "true" || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu không hợp lệ hoặc không khớp.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return View();
            }

            // For demo we remove old password if exists, then set new
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                var remove = await _userManager.RemovePasswordAsync(user);
                if (!remove.Succeeded)
                {
                    TempData["Error"] = "Không thể thiết lập mật khẩu mới.";
                    return View();
                }
            }
            var add = await _userManager.AddPasswordAsync(user, newPassword);
            if (!add.Succeeded)
            {
                TempData["Error"] = string.Join("; ", add.Errors.Select(e => e.Description));
                return View();
            }

            // Clear recovery session
            HttpContext.Session.Remove("RecoveryOTP");
            HttpContext.Session.Remove("RecoveryEmail");
            HttpContext.Session.Remove("RecoveryOTPTime");
            HttpContext.Session.Remove("RecoveryVerified");

            TempData["Success"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
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

        [HttpGet]
        public IActionResult TestRecovery()
        {
            return View();
        }

        // Xóa tài khoản (soft delete)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Soft delete - chỉ đánh dấu là đã xóa
            user.DeletedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["Success"] = "Tài khoản của bạn đã được xóa. Bạn có thể khôi phục tài khoản trong vòng 30 ngày.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Không thể xóa tài khoản. Vui lòng thử lại.";
            return RedirectToAction("Index", "Home");
        }

        // Yêu cầu khôi phục tài khoản
        [HttpGet]
        public IActionResult RequestAccountRecovery()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestAccountRecovery(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Vui lòng nhập email";
                return View();
            }

            // Gọi API từ cookm8.vercel.app để yêu cầu khôi phục tài khoản
            var apiResponse = await _apiService.RequestAccountRecoveryAsync(email);
            
            if (apiResponse.Success)
            {
                // Lưu email vào session để sử dụng ở bước verify
                HttpContext.Session.SetString("AccountRecoveryEmail", email);
                TempData["Success"] = apiResponse.Message;
                return RedirectToAction("VerifyAccountRecoveryOTP");
            }
            else
            {
                TempData["Error"] = apiResponse.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult VerifyAccountRecoveryOTP()
        {
            var email = HttpContext.Session.GetString("AccountRecoveryEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("RequestAccountRecovery");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAccountRecoveryOTP(string otp)
        {
            var email = HttpContext.Session.GetString("AccountRecoveryEmail");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên đã hết hạn. Vui lòng yêu cầu khôi phục lại.";
                return RedirectToAction("RequestAccountRecovery");
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                TempData["Error"] = "Vui lòng nhập mã OTP";
                return View();
            }

            // Gọi API từ cookm8.vercel.app để xác minh OTP và khôi phục tài khoản
            var apiResponse = await _apiService.VerifyAccountRecoveryOTPAsync(email, otp);
            
            if (apiResponse.Success)
            {
                // Nếu API trả về thành công, cập nhật local database
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && user.IsDeleted)
                {
                    // Restore account - remove DeletedAt
                    user.DeletedAt = null;
                    await _userManager.UpdateAsync(user);
                }

                // Clear session
                HttpContext.Session.Remove("AccountRecoveryEmail");

                TempData["Success"] = apiResponse.Message;
                return RedirectToAction("Login");
            }
            else
            {
                TempData["Error"] = apiResponse.Message;
                return View();
            }
        }
    }
}
