using System.Net;
using System.Net.Mail;
using System.Text;

namespace demo.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOTPEmailAsync(string email, string otpCode)
        {
            try
            {
                // Cấu hình SMTP (sử dụng Gmail)
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) || smtpPassword == "your-16-char-app-password-here")
                {
                    _logger.LogWarning("Email credentials not configured, using demo mode");
                    return await SendDemoEmailAsync(email, otpCode);
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername, "CookMate"),
                    Subject = "Mã OTP đăng nhập CookMate",
                    Body = CreateOTPEmailBody(otpCode),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"OTP email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {email}");
                // Fallback to demo mode
                return await SendDemoEmailAsync(email, otpCode);
            }
        }

        private async Task<bool> SendDemoEmailAsync(string email, string otpCode)
        {
            _logger.LogInformation($"DEMO MODE: OTP {otpCode} would be sent to {email}");
            
            // Simulate email sending delay
            await Task.Delay(1000);
            
            return true;
        }

        private string CreateOTPEmailBody(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea, #764ba2); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-code {{ background: #667eea; color: white; font-size: 32px; font-weight: bold; padding: 20px; text-align: center; border-radius: 10px; margin: 20px 0; letter-spacing: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍳 CookMate</h1>
            <p>Mã OTP đăng nhập của bạn</p>
        </div>
        <div class='content'>
            <h2>Xin chào!</h2>
            <p>Bạn đã yêu cầu mã OTP để đăng nhập vào tài khoản CookMate.</p>
            <p>Mã OTP của bạn là:</p>
            <div class='otp-code'>{otpCode}</div>
            <p><strong>Lưu ý:</strong></p>
            <ul>
                <li>Mã OTP có hiệu lực trong 5 phút</li>
                <li>Không chia sẻ mã này với bất kỳ ai</li>
                <li>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này</li>
            </ul>
        </div>
        <div class='footer'>
            <p>© 2024 CookMate - Nấu ăn thông minh, cuộc sống dễ dàng</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
