using Microsoft.AspNetCore.Mvc;
using demo.Services;

namespace demo.Controllers
{
    public class TestOTPController : Controller
    {
        private readonly OTPService _otpService;

        public TestOTPController(OTPService otpService)
        {
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOTP(string email, string? phoneNumber = null)
        {
            try
            {
                var result = await _otpService.SendOTPAsync(email, phoneNumber);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string email, string otpCode)
        {
            try
            {
                var result = await _otpService.VerifyOTPAsync(email, otpCode);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}