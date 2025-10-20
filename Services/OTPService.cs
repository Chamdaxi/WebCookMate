using System.Collections.Concurrent;

namespace demo.Services
{
    public class OTPService
    {
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        private readonly ILogger<OTPService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        // In-memory storage for OTP (trong production nên dùng Redis hoặc Database)
        private readonly ConcurrentDictionary<string, OTPData> _otpStorage = new();

        public OTPService(EmailService emailService, SmsService smsService, ILogger<OTPService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OTPResult> SendOTPAsync(string email, string? phoneNumber = null)
        {
            try
            {
                // Tạo OTP 6 chữ số
                var otpCode = GenerateOTP();
                
                // Lưu OTP vào session
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    session.SetString($"otp_{email}", otpCode);
                    session.SetString($"otp_time_{email}", DateTime.Now.ToString("o"));
                    session.SetString($"otp_attempts_{email}", "0");
                }

                // Chỉ gửi OTP qua email
                var emailSent = await _emailService.SendOTPEmailAsync(email, otpCode);

                _logger.LogInformation($"OTP {otpCode} sent to {email} (Email: {emailSent})");

                return new OTPResult
                {
                    Success = emailSent,
                    Message = emailSent 
                        ? "Mã OTP đã được gửi đến email của bạn!" 
                        : "Có lỗi xảy ra khi gửi OTP!",
                    OTPKey = email, // Sử dụng email làm key
                    OTPCode = otpCode // Chỉ trả về trong demo mode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending OTP to {email}");
                return new OTPResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi OTP!"
                };
            }
        }

        public async Task<OTPVerificationResult> VerifyOTPAsync(string email, string otpCode)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                {
                    return new OTPVerificationResult
                    {
                        Success = false,
                        Message = "Session không khả dụng!"
                    };
                }

                // Lấy OTP từ session
                var storedOtp = session.GetString($"otp_{email}");
                var otpTimeStr = session.GetString($"otp_time_{email}");
                var attemptsStr = session.GetString($"otp_attempts_{email}");

                if (string.IsNullOrEmpty(storedOtp))
                {
                    return new OTPVerificationResult
                    {
                        Success = false,
                        Message = "OTP không tồn tại hoặc đã hết hạn!"
                    };
                }

                // Kiểm tra thời gian hết hạn (5 phút)
                if (DateTime.TryParse(otpTimeStr, out var otpTime))
                {
                    if (DateTime.Now.Subtract(otpTime).TotalMinutes > 5)
                    {
                        // Xóa OTP hết hạn
                        session.Remove($"otp_{email}");
                        session.Remove($"otp_time_{email}");
                        session.Remove($"otp_attempts_{email}");
                        
                        return new OTPVerificationResult
                        {
                            Success = false,
                            Message = "OTP đã hết hạn!"
                        };
                    }
                }

                // Kiểm tra số lần thử
                var attempts = int.TryParse(attemptsStr, out var attemptCount) ? attemptCount : 0;
                if (attempts >= 3)
                {
                    // Xóa OTP sau khi thử quá nhiều lần
                    session.Remove($"otp_{email}");
                    session.Remove($"otp_time_{email}");
                    session.Remove($"otp_attempts_{email}");
                    
                    return new OTPVerificationResult
                    {
                        Success = false,
                        Message = "Bạn đã nhập sai OTP quá nhiều lần. Vui lòng gửi lại OTP mới!"
                    };
                }

                // Tăng số lần thử
                attempts++;
                session.SetString($"otp_attempts_{email}", attempts.ToString());

                if (storedOtp != otpCode)
                {
                    return new OTPVerificationResult
                    {
                        Success = false,
                        Message = $"Mã OTP không đúng! Bạn còn {3 - attempts} lần thử."
                    };
                }

                // OTP đúng - xóa khỏi session
                session.Remove($"otp_{email}");
                session.Remove($"otp_time_{email}");
                session.Remove($"otp_attempts_{email}");

                _logger.LogInformation($"OTP verified successfully for {email}");

                return new OTPVerificationResult
                {
                    Success = true,
                    Message = "Xác thực OTP thành công!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying OTP for {email}");
                return new OTPVerificationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xác thực OTP!"
                };
            }
        }

        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Cleanup expired OTPs
        public void CleanupExpiredOTPs()
        {
            var expiredKeys = _otpStorage.Where(x => x.Value.ExpiresAt < DateTime.Now).Select(x => x.Key).ToList();
            foreach (var key in expiredKeys)
            {
                _otpStorage.TryRemove(key, out _);
            }
        }
    }

    public class OTPData
    {
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
    }

    public class OTPResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? OTPKey { get; set; }
        public string? OTPCode { get; set; } // Chỉ dùng trong demo mode
    }

    public class OTPVerificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
