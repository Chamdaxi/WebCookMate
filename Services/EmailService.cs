using System.Diagnostics;
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
                var smtpPort = _configuration["Email:SmtpPort"] ?? "587";
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("❌ EMAIL CREDENTIALS NOT CONFIGURED!");
                    throw new Exception("Email credentials are required");
                }

                _logger.LogInformation($"📧 Sending OTP via Python script to {email}");

                // FIX: Sử dụng Python script để gửi email (fix lỗi SSL trên macOS Sequoia)
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "send_email.py");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/python3",
                    Arguments = $"\"{scriptPath}\" \"{smtpHost}\" \"{smtpPort}\" \"{smtpUsername}\" \"{smtpPassword}\" \"{email}\" \"{otpCode}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation($"✅ OTP email sent SUCCESSFULLY to {email} - Output: {output}");
                    return true;
                }
                else
                {
                    _logger.LogError($"❌ Python script failed with exit code {process.ExitCode}. Error: {error}");
                    throw new Exception($"Failed to send email via Python script: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ FAILED to send OTP email to {email}: {ex.Message}");
                throw;
            }
        }
    }
}
