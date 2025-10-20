using System.Text.Json;

namespace demo.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TokenService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            // In a real application, you would get this from a secure storage
            // For now, we'll return null to indicate no token
            return null;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync("https://cookm8.vercel.app/api/user/profile");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<object?> GetUserProfileAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync("https://cookm8.vercel.app/api/user/profile");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<object>(content);
                }
            }
            catch
            {
                // Handle error
            }
            
            return null;
        }
    }
}




