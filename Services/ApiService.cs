using System.Text.Json;
using System.Text;

namespace demo.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = "https://cookm8.vercel.app";
        }

        // OTP Authentication Methods
        public async Task<ApiResponse<object>> SendOTPAsync(string email)
        {
            try
            {
                Console.WriteLine($"Gửi OTP đến {email} (Demo mode)...");
                
                // Tạo mock OTP vì API CookMate không có endpoint OTP
                var otp = new Random().Next(100000, 999999).ToString();
                var mockResponse = new Dictionary<string, object>
                {
                    { "otp", otp },
                    { "email", email },
                    { "expires_in", 300 },
                    { "message", "OTP sent successfully (Demo mode)" }
                };
                
                Console.WriteLine($"Demo OTP cho {email}: {otp}");
                
                return new ApiResponse<object> { 
                    Success = true, 
                    Data = mockResponse, 
                    Message = "OTP sent successfully (Demo mode)" 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendOTP Error: {ex.Message}");
                return new ApiResponse<object> { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<ApiResponse<AuthResult>> VerifyOTPAsync(string email, string otp)
        {
            try
            {
                Console.WriteLine($"Verify OTP cho {email} (Demo mode)...");
                
                // Demo verification - accept any 6-digit OTP
                if (otp.Length == 6 && otp.All(char.IsDigit))
                {
                    var mockAuthResult = new AuthResult
                    {
                        AccessToken = $"demo_token_{Guid.NewGuid():N}",
                        User = new UserInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            Email = email,
                            FullName = email.Split('@')[0]
                        }
                    };
                    
                    Console.WriteLine($"Demo verification thành công cho {email}");
                    
                    return new ApiResponse<AuthResult> { 
                        Success = true, 
                        Data = mockAuthResult, 
                        Message = "OTP verified successfully (Demo mode)" 
                    };
                }
                else
                {
                    return new ApiResponse<AuthResult> { 
                        Success = false, 
                        Message = "Invalid OTP format (Demo mode)" 
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VerifyOTP Error: {ex.Message}");
                return new ApiResponse<AuthResult> { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        // Google OAuth Methods
        public async Task<ApiResponse<AuthResult>> GoogleLoginAsync(string googleToken)
        {
            try
            {
                var requestData = new { token = googleToken };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/google", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResult>(responseContent);
                    return new ApiResponse<AuthResult> { Success = true, Data = result, Message = "Google login successful" };
                }
                else
                {
                    return new ApiResponse<AuthResult> { Success = false, Message = "Google login failed" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResult> { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        // User Profile Methods
        public async Task<ApiResponse<UserProfile>> GetUserProfileAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var response = await _httpClient.GetAsync($"{_baseUrl}/user/profile");
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<UserProfile>(responseContent);
                    return new ApiResponse<UserProfile> { Success = true, Data = result };
                }
                else
                {
                    return new ApiResponse<UserProfile> { Success = false, Message = "Failed to get user profile" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserProfile> { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        // Meal Planning Methods
        public async Task<ApiResponse<List<MealPlan>>> GetMealPlansAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var response = await _httpClient.GetAsync($"{_baseUrl}/meal-plans");
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<List<MealPlan>>(responseContent);
                    return new ApiResponse<List<MealPlan>> { Success = true, Data = result };
                }
                else
                {
                    return new ApiResponse<List<MealPlan>> { Success = false, Message = "Failed to get meal plans" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<MealPlan>> { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        // Recipe Methods
        public async Task<ApiResponse<List<Recipe>>> GetRecipesAsync(string accessToken, string category = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var url = $"{_baseUrl}/recipes";
                if (!string.IsNullOrEmpty(category))
                {
                    url += $"?category={category}";
                }
                
                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<List<Recipe>>(responseContent);
                    return new ApiResponse<List<Recipe>> { Success = true, Data = result };
                }
                else
                {
                    return new ApiResponse<List<Recipe>> { Success = false, Message = "Failed to get recipes" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Recipe>> { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }

    // Response Models
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AuthResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }

    public class UserProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MealPlan
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<Meal> Meals { get; set; } = new List<Meal>();
    }

    public class Meal
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // breakfast, lunch, dinner, snack
        public List<string> Ingredients { get; set; } = new List<string>();
        public string Instructions { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class Recipe
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new List<string>();
        public string Instructions { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int CookTime { get; set; }
        public int PrepTime { get; set; }
        public int Servings { get; set; }
        public int Views { get; set; }
        public int Cooksnaps { get; set; }
    }
}
