using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace demo.Services
{
    public interface ICookMateApiService
    {
        Task<ApiResponse> RequestAccountRecoveryAsync(string email);
        Task<ApiResponse> VerifyAccountRecoveryOTPAsync(string email, string otp);
    }

    public class CookMateApiService : ICookMateApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public CookMateApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["CookMateApi:BaseUrl"] ?? "https://cookm8.vercel.app";
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<ApiResponse> RequestAccountRecoveryAsync(string email)
        {
            try
            {
                var requestBody = new { email };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/recover-account", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Message = "Mã OTP đã được gửi đến email của bạn.",
                        Data = responseContent
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Không thể gửi yêu cầu khôi phục. Vui lòng thử lại.",
                        Error = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Lỗi kết nối đến server. Vui lòng thử lại sau.",
                    Error = ex.Message
                };
            }
        }

        public async Task<ApiResponse> VerifyAccountRecoveryOTPAsync(string email, string otp)
        {
            try
            {
                var requestBody = new { email, otp };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/verify-otp", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Message = "Xác minh OTP thành công. Tài khoản đã được khôi phục.",
                        Data = responseContent
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Mã OTP không đúng hoặc đã hết hạn.",
                        Error = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Lỗi kết nối đến server. Vui lòng thử lại sau.",
                    Error = ex.Message
                };
            }
        }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Data { get; set; }
        public string? Error { get; set; }
    }
}

