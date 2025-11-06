using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace demo.Services
{
    /// <summary>
    /// Service tổng hợp để call tất cả CookMate API endpoints
    /// Base URL: https://cookm8.vercel.app
    /// </summary>
    public class CookMateApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CookMateApiService> _logger;
        private const string API_BASE_URL = "https://cookm8.vercel.app/api";

        public CookMateApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CookMateApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        #region Helper Methods

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            
            // Lấy token từ session
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("⚠️ HttpContext is null in CreateAuthenticatedClient");
                return client;
            }
            
            var token = httpContext.Session.GetString("access_token");
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("⚠️ No access token found in session");
                throw new UnauthorizedAccessException("No access token found. Please login again.");
            }
            
            _logger.LogInformation($"🔑 Using token: {token.Substring(0, Math.Min(20, token.Length))}...");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"GET {endpoint} failed: {response.StatusCode}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling GET {endpoint}");
                return default;
            }
        }

        private async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation($"📤 POST {API_BASE_URL}{endpoint} - Body: {json}");
                var response = await client.PostAsync($"{API_BASE_URL}{endpoint}", content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ POST {endpoint} succeeded: {response.StatusCode}");
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"❌ POST {endpoint} failed: {response.StatusCode} - {responseContent}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error calling POST {endpoint}");
                return default;
            }
        }

        private async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PutAsync($"{API_BASE_URL}{endpoint}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"PUT {endpoint} failed: {response.StatusCode}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling PUT {endpoint}");
                return default;
            }
        }

        private async Task<bool> DeleteAsync(string endpoint, object? data = null)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{API_BASE_URL}{endpoint}");
                
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                
                var response = await client.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"DELETE {endpoint} failed: {response.StatusCode}");
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling DELETE {endpoint}");
                return false;
            }
        }

        #endregion

        #region Profile APIs

        public async Task<UserProfile?> GetProfileAsync()
        {
            return await GetAsync<UserProfile>("/profile");
        }

        public async Task<UserProfile?> UpdateProfileAsync(UpdateProfileRequest request)
        {
            return await PutAsync<UserProfile>("/profile", request);
        }

        public async Task<bool> DeleteAccountAsync()
        {
            return await DeleteAsync("/profile/delete");
        }

        #endregion

        #region Ingredient Category APIs

        public async Task<List<IngredientCategory>?> GetIngredientCategoriesAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/ingredient-categories");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<IngredientCategory>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<List<IngredientCategory>>(dataElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        // Try other common keys
                        foreach (var key in new[] { "ingredientCategories", "categories", "items", "results" })
                        {
                            if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<IngredientCategory>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    
                    _logger.LogWarning($"Unexpected response format for categories: {content.Substring(0, Math.Min(200, content.Length))}");
                    return new List<IngredientCategory>();
                }
                
                _logger.LogError($"GET /ingredient-categories failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ingredient categories");
                return null;
            }
        }

        public async Task<IngredientCategory?> CreateCategoryAsync(CreateCategoryRequest request)
        {
            return await PostAsync<IngredientCategory>("/ingredient-categories", request);
        }

        public async Task<IngredientCategory?> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            return await PutAsync<IngredientCategory>("/ingredient-categories", request);
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            return await DeleteAsync("/ingredient-categories", new { ingredientCategoryId = categoryId });
        }

        #endregion

        #region Ingredient APIs

        public async Task<List<Ingredient>?> GetIngredientsAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/ingredients");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<Ingredient>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<List<Ingredient>>(dataElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        // Try other common keys
                        foreach (var key in new[] { "ingredients", "items", "results" })
                        {
                            if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<Ingredient>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    
                    _logger.LogWarning($"Unexpected response format for ingredients: {content.Substring(0, Math.Min(200, content.Length))}");
                    return new List<Ingredient>();
                }
                
                _logger.LogError($"GET /ingredients failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ingredients");
                return null;
            }
        }

        public async Task<Ingredient?> AddIngredientAsync(MultipartFormDataContent formData)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ingredients", formData);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Ingredient>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"Add ingredient failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ingredient");
                return null;
            }
        }

        public async Task<Ingredient?> UpdateIngredientAsync(MultipartFormDataContent formData)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var request = new HttpRequestMessage(HttpMethod.Put, $"{API_BASE_URL}/ingredients")
                {
                    Content = formData
                };
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Ingredient>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"Update ingredient failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ingredient");
                return null;
            }
        }

        public async Task<bool> DeleteIngredientAsync(string ingredientId)
        {
            return await DeleteAsync("/ingredients", new { ingredientId });
        }

        #endregion

        #region Recipe APIs

        public async Task<List<Recipe>?> GetRecipesAsync(int? page = null, int? limit = null)
        {
            var endpoint = "/recipes";
            if (page.HasValue && limit.HasValue)
            {
                endpoint += $"?page={page}&limit={limit}";
            }
            return await GetAsync<List<Recipe>>(endpoint);
        }

        public async Task<List<Recipe>?> GetTodayRecipesAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/recipes/today");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<Recipe>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get recipes property (common format)
                        foreach (var key in new[] { "recipes", "data", "items", "results", "content" })
                        {
                            if (jsonDoc.TryGetProperty(key, out var recipesElement) && recipesElement.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<Recipe>>(recipesElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    
                    _logger.LogWarning($"Unexpected response format for recipes/today: {content.Substring(0, Math.Min(200, content.Length))}");
                    return new List<Recipe>();
                }
                
                // If 400 BadRequest or other client errors, return empty list instead of null
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning($"GET /recipes/today returned 400 BadRequest - returning empty list");
                    return new List<Recipe>();
                }
                
                _logger.LogError($"GET /recipes/today failed: {response.StatusCode}");
                return new List<Recipe>(); // Return empty list instead of null
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling GET /recipes/today");
                return new List<Recipe>(); // Return empty list instead of null
            }
        }

        public async Task<List<Recipe>?> GetRecipeDetailsAsync(List<int> recipeIds)
        {
            return await PostAsync<List<Recipe>>("/recipes/bulk", new { recipeIds });
        }

        #endregion

        #region Favorites APIs

        public async Task<List<Favorite>?> GetFavoritesAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/favorites");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<Favorite>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<List<Favorite>>(dataElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        // Try other common keys
                        foreach (var key in new[] { "favorites", "items", "results" })
                        {
                            if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<Favorite>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    
                    _logger.LogWarning($"Unexpected response format for favorites: {content.Substring(0, Math.Min(200, content.Length))}");
                    return new List<Favorite>();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"GET /favorites failed: {response.StatusCode} - {errorContent}");
                
                // If it's a Spoonacular API limit error, throw a specific exception
                // This allows the controller to return a proper error message to the frontend
                if (errorContent.Contains("daily points limit") || errorContent.Contains("402"))
                {
                    _logger.LogWarning("⚠️ Spoonacular API limit reached - favorites exist in database but cannot fetch details");
                    throw new InvalidOperationException("Spoonacular API limit reached. Favorites are saved but cannot display details. Please try again tomorrow.");
                }
                
                return null;
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException (for Spoonacular limit) so controller can handle it
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites");
                return null;
            }
        }

        public async Task<Favorite?> AddFavoriteAsync(string recipeId)
        {
            try
            {
                _logger.LogInformation($"🔄 POST /favorites - RecipeId: {recipeId}");
                var result = await PostAsync<Favorite>("/favorites", new { recipeId });
                if (result == null)
                {
                    _logger.LogWarning($"⚠️ PostAsync returned null for RecipeId: {recipeId}");
                }
                else
                {
                    _logger.LogInformation($"✅ Successfully added favorite: RecipeId {recipeId}");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error in AddFavoriteAsync for RecipeId: {recipeId}");
                throw;
            }
        }

        public async Task<bool> DeleteFavoriteAsync(string recipeId)
        {
            return await DeleteAsync("/favorites", new { recipeId });
        }

        #endregion

        #region Shopping List APIs

        public async Task<List<ShoppingItem>?> GetShoppingListAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/shopping");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<ShoppingItem>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<List<ShoppingItem>>(dataElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        // Try other common keys
                        foreach (var key in new[] { "shopping", "items", "results" })
                        {
                            if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<ShoppingItem>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    
                    _logger.LogWarning($"Unexpected response format for shopping list: {content.Substring(0, Math.Min(200, content.Length))}");
                    return new List<ShoppingItem>();
                }
                
                _logger.LogError($"GET /shopping failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shopping list");
                return null;
            }
        }

        public async Task<ShoppingItem?> AddShoppingItemAsync(CreateShoppingItemRequest request)
        {
            return await PostAsync<ShoppingItem>("/shopping", request);
        }

        public async Task<ShoppingItem?> UpdateShoppingItemAsync(UpdateShoppingItemRequest request)
        {
            return await PutAsync<ShoppingItem>("/shopping", request);
        }

        public async Task<bool> DeleteShoppingItemAsync(string shoppingItemId)
        {
            return await DeleteAsync("/shopping", new { shoppingItemId });
        }

        #endregion

        #region Meal Plan APIs

        public async Task<List<MealPlan>?> GetMealPlansAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/meal-plans");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<MealPlan>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<List<MealPlan>>(dataElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        // Try other common keys
                        foreach (var key in new[] { "mealPlans", "meal_plans", "items", "results" })
                        {
                            if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                            {
                                return JsonSerializer.Deserialize<List<MealPlan>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }
                    }
                    
                    _logger.LogWarning($"Unexpected response format for meal plans: {content.Substring(0, Math.Min(200, content.Length))}");
                    return new List<MealPlan>();
                }
                
                _logger.LogError($"GET /meal-plans failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meal plans");
                return null;
            }
        }

        public async Task<MealPlan?> CreateMealPlanAsync(CreateMealPlanRequest request)
        {
            return await PostAsync<MealPlan>("/meal-plans", request);
        }

        public async Task<MealPlan?> UpdateMealPlanAsync(UpdateMealPlanRequest request)
        {
            return await PutAsync<MealPlan>("/meal-plans", request);
        }

        public async Task<bool> DeleteMealPlanAsync(string mealPlanId)
        {
            return await DeleteAsync("/meal-plans", new { mealPlanId });
        }

        #endregion

        #region Notifications APIs

        public async Task<List<Notification>?> GetNotificationsAsync()
        {
            return await GetAsync<List<Notification>>("/notifications");
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
        {
            return await PutAsync<bool>("/notifications/mark-read", new { notificationId });
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            return await DeleteAsync("/notifications", new { notificationId });
        }

        #endregion

        #region Models

        public class UserProfile
        {
            public string Id { get; set; } = "";
            public string Email { get; set; } = "";
            public string Name { get; set; } = "";
            public string Avatar { get; set; } = "";
            public List<string> DietaryPreferences { get; set; } = new();
        }

        public class UpdateProfileRequest
        {
            public string? Avatar { get; set; }
            public string? Name { get; set; }
            public List<string>? DietaryPreferences { get; set; }
        }

        public class IngredientCategory
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
            public string UserId { get; set; } = "";
        }

        public class CreateCategoryRequest
        {
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public class UpdateCategoryRequest
        {
            public string IngredientCategoryId { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public class Ingredient
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string CategoryId { get; set; } = "";
            public decimal Quantity { get; set; }
            public string Unit { get; set; } = "";
            public DateTime? ExpireDate { get; set; }
            public string Notes { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string UserId { get; set; } = "";
        }

        public class Recipe
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Image { get; set; } = "";
            public int ReadyInMinutes { get; set; }
            public int Servings { get; set; }
        }

        public class Favorite
        {
            public string Id { get; set; } = "";
            public string RecipeId { get; set; } = "";
            public string UserId { get; set; } = "";
            public DateTime CreatedAt { get; set; }
        }

        public class ShoppingItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Notes { get; set; } = "";
            public string Status { get; set; } = ""; // "active" or "draft"
            public string UserId { get; set; } = "";
        }

        public class CreateShoppingItemRequest
        {
            public string Name { get; set; } = "";
            public string? Notes { get; set; }
            public string Status { get; set; } = "active";
        }

        public class UpdateShoppingItemRequest
        {
            public string ShoppingItemId { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Notes { get; set; }
            public string Status { get; set; } = "active";
        }

        public class MealPlan
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public List<string> RecipeIds { get; set; } = new();
            public string Notes { get; set; } = "";
            public DateTime Date { get; set; }
            public string UserId { get; set; } = "";
        }

        public class CreateMealPlanRequest
        {
            public string Name { get; set; } = "";
            public List<string> RecipeIds { get; set; } = new();
            public string? Notes { get; set; }
            public DateTime Date { get; set; }
        }

        public class UpdateMealPlanRequest
        {
            public string MealPlanId { get; set; } = "";
            public string Name { get; set; } = "";
            public List<string> RecipeIds { get; set; } = new();
            public string? Notes { get; set; }
            public DateTime Date { get; set; }
        }

        public class Notification
        {
            public string Id { get; set; } = "";
            public string Message { get; set; } = "";
            public bool IsRead { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        #endregion
    }
}

