using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using demo.Models;

namespace demo.Controllers
{
    public class SimpleOTPController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SimpleOTPController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Hiển thị trang OTP Login
        public IActionResult Index()
        {
            return View();
        }

        // Gửi OTP
        [HttpPost]
        public IActionResult SendOTP(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Vui lòng nhập email" });
                }

                // Tạo OTP ngẫu nhiên 6 chữ số
                var otp = new Random().Next(100000, 999999).ToString();
                
                // Lưu OTP vào session
                HttpContext.Session.SetString("otp", otp);
                HttpContext.Session.SetString("otp_email", email);
                HttpContext.Session.SetString("otp_time", DateTime.Now.ToString());

                Console.WriteLine($"OTP cho {email}: {otp}");

                return Json(new { 
                    success = true, 
                    message = $"Mã OTP: {otp} (Demo mode)",
                    otp = otp 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendOTP Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi OTP" });
            }
        }

        // Xác thực OTP
        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otpCode)
        {
            try
            {
                if (string.IsNullOrEmpty(otpCode))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã OTP" });
                }

                // Lấy thông tin từ session
                var storedOtp = HttpContext.Session.GetString("otp");
                var email = HttpContext.Session.GetString("otp_email");
                var otpTimeStr = HttpContext.Session.GetString("otp_time");

                if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "OTP đã hết hạn, vui lòng gửi lại" });
                }

                // Kiểm tra thời gian hết hạn (5 phút)
                if (DateTime.TryParse(otpTimeStr, out var otpTime))
                {
                    if (DateTime.Now.Subtract(otpTime).TotalMinutes > 5)
                    {
                        // Xóa session
                        HttpContext.Session.Remove("otp");
                        HttpContext.Session.Remove("otp_email");
                        HttpContext.Session.Remove("otp_time");
                        
                        return Json(new { success = false, message = "OTP đã hết hạn, vui lòng gửi lại" });
                    }
                }

                // Kiểm tra OTP
                if (storedOtp != otpCode)
                {
                    return Json(new { success = false, message = "Mã OTP không đúng" });
                }

                // OTP đúng - tạo hoặc tìm user
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
                HttpContext.Session.Remove("otp");
                HttpContext.Session.Remove("otp_email");
                HttpContext.Session.Remove("otp_time");

                return Json(new { 
                    success = true, 
                    message = "Đăng nhập thành công!",
                    redirectUrl = Url.Action("Index", "Home")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VerifyOTP Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xác thực OTP" });
            }
        }
    }
}


