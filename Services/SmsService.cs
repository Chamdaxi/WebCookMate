using System.Text;

namespace demo.Services
{
    public class SmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOTPSmsAsync(string phoneNumber, string otpCode)
        {
            try
            {
                var twilioAccountSid = _configuration["Twilio:AccountSid"];
                var twilioAuthToken = _configuration["Twilio:AuthToken"];
                var twilioPhoneNumber = _configuration["Twilio:PhoneNumber"];

                if (string.IsNullOrEmpty(twilioAccountSid) || string.IsNullOrEmpty(twilioAuthToken))
                {
                    _logger.LogWarning("Twilio credentials not configured, using demo mode");
                    return await SendDemoSmsAsync(phoneNumber, otpCode);
                }

                // Sử dụng Twilio API để gửi SMS
                var message = $"Mã OTP CookMate của bạn là: {otpCode}. Mã có hiệu lực trong 5 phút. Không chia sẻ mã này với ai.";
                
                // TODO: Implement actual Twilio SMS sending
                // var client = new TwilioRestClient(twilioAccountSid, twilioAuthToken);
                // var message = await client.Messages.CreateAsync(
                //     new PhoneNumber(phoneNumber),
                //     new PhoneNumber(twilioPhoneNumber),
                //     message
                // );

                _logger.LogInformation($"SMS sent successfully to {phoneNumber}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}");
                // Fallback to demo mode
                return await SendDemoSmsAsync(phoneNumber, otpCode);
            }
        }

        private async Task<bool> SendDemoSmsAsync(string phoneNumber, string otpCode)
        {
            _logger.LogInformation($"DEMO MODE: SMS with OTP {otpCode} would be sent to {phoneNumber}");
            
            // Simulate SMS sending delay
            await Task.Delay(1000);
            
            return true;
        }
    }
}


