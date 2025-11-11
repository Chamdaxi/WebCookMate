using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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
            
            // Clear any existing headers to avoid conflicts
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // DO NOT set Content-Type for multipart/form-data - let HttpClient set it automatically with boundary
            // Setting it manually will break the multipart boundary

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
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                var json = JsonSerializer.Serialize(data, jsonOptions);
                _logger.LogInformation($"📤 POST {API_BASE_URL}{endpoint}");
                _logger.LogInformation($"📤 Request body: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{API_BASE_URL}{endpoint}", content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"📥 Response status: {response.StatusCode}");
                _logger.LogInformation($"📥 Response body (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ POST {endpoint} succeeded: {response.StatusCode}");
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"❌ POST {endpoint} failed: {response.StatusCode}");
                _logger.LogError($"❌ Response body: {responseContent}");
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
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                var json = JsonSerializer.Serialize(data, jsonOptions);
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
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    };
                    var json = JsonSerializer.Serialize(data, jsonOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    _logger.LogInformation($"📤 DELETE {API_BASE_URL}{endpoint} - Body: {json}");
                }
                else
                {
                    _logger.LogInformation($"📤 DELETE {API_BASE_URL}{endpoint}");
                }
                
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ DELETE {endpoint} succeeded: {response.StatusCode}");
                    return true;
                }
                
                _logger.LogError($"❌ DELETE {endpoint} failed: {response.StatusCode} - {responseContent}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error calling DELETE {endpoint}");
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
                    _logger.LogInformation($"📦 Raw API response (first 500 chars): {content.Substring(0, Math.Min(500, content.Length))}");
                    
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    List<IngredientCategory>? result = null;
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    // Handle both array and wrapped object formats
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        _logger.LogInformation("📦 Response is an array");
                        result = JsonSerializer.Deserialize<List<IngredientCategory>>(content, options);
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        _logger.LogInformation("📦 Response is an object, checking for data property...");
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            _logger.LogInformation("📦 Found 'data' property with array");
                            result = JsonSerializer.Deserialize<List<IngredientCategory>>(dataElement.GetRawText(), options);
                        }
                        else
                        {
                            // Try other common keys
                            foreach (var key in new[] { "ingredientCategories", "categories", "items", "results" })
                            {
                                if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                                {
                                    _logger.LogInformation($"📦 Found '{key}' property with array");
                                    result = JsonSerializer.Deserialize<List<IngredientCategory>>(itemsElement.GetRawText(), options);
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (result != null && result.Count > 0)
                    {
                        _logger.LogInformation($"✅ Parsed {result.Count} categories");
                        
                        // Normalize categories - ensure Id is populated from any field
                        foreach (var cat in result)
                        {
                            // If Id is empty, try to get from other fields
                            if (string.IsNullOrEmpty(cat.Id))
                            {
                                cat.Id = cat.GetId();
                            }
                        }
                        
                        var firstCat = result[0];
                        _logger.LogInformation($"🔍 First parsed category - Id: '{firstCat.Id}', _Id: '{firstCat._Id}', IngredientCategoryId: '{firstCat.IngredientCategoryId}', Name: '{firstCat.Name}', Icon: '{firstCat.Icon}'");
                        
                        // Check if ID is still empty after normalization
                        if (string.IsNullOrEmpty(firstCat.Id))
                        {
                            _logger.LogWarning($"⚠️ First category has empty ID after normalization! Checking raw JSON structure...");
                            // Try to manually extract ID from raw JSON
                            if (jsonDoc.ValueKind == JsonValueKind.Array && jsonDoc.GetArrayLength() > 0)
                            {
                                var firstElement = jsonDoc[0];
                                var keys = firstElement.EnumerateObject().Select(p => p.Name).ToList();
                                _logger.LogInformation($"🔍 Raw first element keys: {string.Join(", ", keys)}");
                                foreach (var prop in firstElement.EnumerateObject())
                                {
                                    var valueStr = prop.Value.ValueKind == JsonValueKind.String 
                                        ? prop.Value.GetString() 
                                        : prop.Value.GetRawText();
                                    _logger.LogInformation($"   - {prop.Name}: {valueStr}");
                                }
                            }
                        }
                        
                        return result;
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
            try
            {
                _logger.LogInformation($"📤 POST /ingredient-categories - Name: {request.Name}, Icon: {request.Icon}");
                var result = await PostAsync<IngredientCategory>("/ingredient-categories", request);
                
                if (result == null)
                {
                    _logger.LogWarning("⚠️ CreateCategoryAsync returned null");
                }
                else
                {
                    _logger.LogInformation($"✅ CreateCategoryAsync succeeded - Id: '{result.GetId()}', Name: '{result.Name}'");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in CreateCategoryAsync");
                return null;
            }
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

        /// <summary>
        /// Create raw multipart/form-data body manually to match Python requests/Postman format
        /// This ensures compatibility with Node.js multer/busboy parser
        /// Format: Each part starts with --boundary, ends with \r\n
        /// Final closing: --boundary--\r\n
        /// </summary>
        private byte[] CreateMultipartFormDataBody(
            string boundary,
            List<(string key, string value)> fields,
            Dictionary<string, (byte[] bytes, string fileName, string contentType)>? files = null)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var crlf = Encoding.UTF8.GetBytes("\r\n");
                
                // Add form fields first (text fields)
                // Order matters: Postman sends fields in the order they appear in the formdata array
                // Format: --boundary\r\nContent-Disposition: form-data; name="key"\r\n\r\nvalue\r\n
                foreach (var field in fields)
                {
                    // Write boundary line: --boundary\r\n
                    var boundaryLine = Encoding.UTF8.GetBytes($"--{boundary}\r\n");
                    ms.Write(boundaryLine, 0, boundaryLine.Length);
                    
                    // Write Content-Disposition header
                    var contentDisposition = Encoding.UTF8.GetBytes($"Content-Disposition: form-data; name=\"{field.key}\"\r\n");
                    ms.Write(contentDisposition, 0, contentDisposition.Length);
                    
                    // Empty line (CRLF) to separate headers from content
                    ms.Write(crlf, 0, crlf.Length);
                    
                    // Write field value (as UTF-8 bytes)
                    var fieldValue = Encoding.UTF8.GetBytes(field.value);
                    ms.Write(fieldValue, 0, fieldValue.Length);
                    
                    // CRLF after field value
                    ms.Write(crlf, 0, crlf.Length);
                }
                
                // Add file fields (if any)
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        // Format: --boundary\r\nContent-Disposition: form-data; name="key"; filename="filename"\r\nContent-Type: type\r\n\r\n[bytes]\r\n
                        var boundaryLine = Encoding.UTF8.GetBytes($"--{boundary}\r\n");
                        ms.Write(boundaryLine, 0, boundaryLine.Length);
                        
                        // Escape filename if it contains special characters
                        var safeFileName = file.Value.fileName.Replace("\"", "\\\"");
                        var contentDisposition = Encoding.UTF8.GetBytes($"Content-Disposition: form-data; name=\"{file.Key}\"; filename=\"{safeFileName}\"\r\n");
                        ms.Write(contentDisposition, 0, contentDisposition.Length);
                        
                        var contentType = Encoding.UTF8.GetBytes($"Content-Type: {file.Value.contentType}\r\n");
                        ms.Write(contentType, 0, contentType.Length);
                        
                        ms.Write(crlf, 0, crlf.Length); // Empty line before file content
                        
                        ms.Write(file.Value.bytes, 0, file.Value.bytes.Length);
                        ms.Write(crlf, 0, crlf.Length);
                    }
                }
                
                // Add closing boundary: --boundary--\r\n
                var closingBoundary = Encoding.UTF8.GetBytes($"--{boundary}--\r\n");
                ms.Write(closingBoundary, 0, closingBoundary.Length);
                
                return ms.ToArray();
            }
        }

        public async Task<Ingredient?> AddIngredientAsync(
            string categoryId,
            string name,
            decimal quantity,
            string unit,
            string? expireDate = null,
            string? notes = null,
            IFormFile? image = null)
        {
            try
            {
                // Get token from session
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("⚠️ HttpContext is null in AddIngredientAsync");
                    return null;
                }
                
                var token = httpContext.Session.GetString("access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No access token found in session");
                    throw new UnauthorizedAccessException("No access token found. Please login again.");
                }
                
                _logger.LogInformation($"🔑 Using token: {token.Substring(0, Math.Min(20, token.Length))}...");
                
                var url = $"{API_BASE_URL}/ingredients";
                
                // Prepare expireDate in ISO 8601 format (match Postman: 2025-09-23T14:06:15.378Z)
                string? formattedExpireDate = null;
                if (!string.IsNullOrWhiteSpace(expireDate))
                {
                    if (DateTime.TryParse(expireDate, out var parsedDate))
                    {
                        var utcDate = parsedDate.ToUniversalTime();
                        formattedExpireDate = utcDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
                        _logger.LogInformation($"📅 ExpireDate (ISO 8601): {formattedExpireDate}");
                    }
                    else
                    {
                        // If already in ISO format, use as-is
                        formattedExpireDate = expireDate;
                        _logger.LogInformation($"📅 ExpireDate (using as-is): {expireDate}");
                    }
                }
                
                // Read image bytes if provided
                byte[]? imageBytes = null;
                string? imageFileName = null;
                string? imageContentType = null;
                if (image != null && image.Length > 0)
                {
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                    imageFileName = image.FileName ?? "image.jpg";
                    imageContentType = image.ContentType ?? "image/jpeg";
                    _logger.LogInformation($"📷 Image prepared: {imageFileName} ({imageBytes.Length} bytes, Content-Type: {imageContentType})");
                }
                
                // Create form fields in EXACT order as Postman (order matters for multipart/form-data)
                // Postman order: categoryId, name, quantity, unit, expireDate, notes, image
                var fields = new List<(string key, string value)>
                {
                    ("categoryId", categoryId),
                    ("name", name),
                    ("quantity", quantity.ToString()),
                    ("unit", unit) // API expects English: piece, kg, g, l, ml
                };
                
                if (!string.IsNullOrWhiteSpace(formattedExpireDate))
                {
                    fields.Add(("expireDate", formattedExpireDate));
                }
                
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    fields.Add(("notes", notes));
                }
                
                // Generate boundary - Python requests uses simple hex string (32 chars)
                // Example: "f95dc406338aace50714be51634fa5f2"
                // We'll use a similar format: 32 hex characters (from Guid)
                var boundaryValue = Guid.NewGuid().ToString("N"); // 32 hex chars, no dashes
                
                // Prepare files dictionary
                Dictionary<string, (byte[] bytes, string fileName, string contentType)>? files = null;
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    files = new Dictionary<string, (byte[] bytes, string fileName, string contentType)>
                    {
                        { "image", (imageBytes, imageFileName!, imageContentType!) }
                    };
                }
                
                // Create raw multipart body using boundary value
                var bodyBytes = CreateMultipartFormDataBody(boundaryValue, fields, files);
                
                // Decode first part of body to verify format (for debugging)
                var bodyText = Encoding.UTF8.GetString(bodyBytes, 0, Math.Min(300, bodyBytes.Length));
                _logger.LogInformation($"📤 POST {url}");
                _logger.LogInformation($"   Fields: {string.Join(", ", fields.Select(f => f.key))}");
                _logger.LogInformation($"   Body size: {bodyBytes.Length} bytes");
                _logger.LogInformation($"   Boundary: {boundaryValue}");
                _logger.LogInformation($"   Body preview (first 300 chars): {bodyText.Replace("\r", "\\r").Replace("\n", "\\n")}");
                
                // Create HttpClient and send request
                var client = CreateAuthenticatedClient();
                
                // Don't let HttpClient modify the request - create raw request
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                
                // Create ByteArrayContent with the raw body
                var content = new ByteArrayContent(bodyBytes);
                
                // Set Content-Type header BEFORE assigning to request
                // This prevents HttpClient from modifying it
                content.Headers.Remove("Content-Type"); // Remove any default
                content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundaryValue}");
                
                request.Content = content;
                
                // Log the exact Content-Type being sent
                var contentTypeValue = request.Content.Headers.GetValues("Content-Type").FirstOrDefault();
                _logger.LogInformation($"📤 Content-Type header: {contentTypeValue}");
                
                // Send request
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"📥 Response: {(int)response.StatusCode} {response.StatusCode}");
                _logger.LogInformation($"📥 Body (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var ingredient = JsonSerializer.Deserialize<Ingredient>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (ingredient != null)
                        {
                            _logger.LogInformation($"✅ Added ingredient: {ingredient.Name} (ID: {ingredient.Id})");
                        }
                        
                        return ingredient;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, $"❌ Failed to deserialize response: {responseContent}");
                        return null;
                    }
                }
                
                // Log error
                _logger.LogError($"❌ POST /ingredients failed: {response.StatusCode}");
                _logger.LogError($"❌ Response: {responseContent}");
                
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "❌ Unauthorized: Token may be invalid or expired");
                throw; // Re-throw to be handled by controller
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error adding ingredient");
                _logger.LogError($"❌ Exception type: {ex.GetType().Name}");
                _logger.LogError($"❌ Exception message: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<Ingredient?> UpdateIngredientAsync(
            string ingredientId,
            string? categoryId,
            string name,
            decimal quantity,
            string unit,
            string? expireDate = null,
            string? notes = null,
            IFormFile? image = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("⚠️ HttpContext is null in UpdateIngredientAsync");
                    return null;
                }
                
                var token = httpContext.Session.GetString("access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No access token found in session");
                    throw new UnauthorizedAccessException("No access token found. Please login again.");
                }
                
                var url = $"{API_BASE_URL}/ingredients";
                
                // Prepare expireDate in ISO 8601 format
                string? formattedExpireDate = null;
                if (!string.IsNullOrWhiteSpace(expireDate))
                {
                    if (DateTime.TryParse(expireDate, out var parsedDate))
                    {
                        var utcDate = parsedDate.ToUniversalTime();
                        formattedExpireDate = utcDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
                    }
                    else
                    {
                        formattedExpireDate = expireDate;
                    }
                }
                
                // Read image bytes if provided
                byte[]? imageBytes = null;
                string? imageFileName = null;
                string? imageContentType = null;
                if (image != null && image.Length > 0)
                {
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                    imageFileName = image.FileName ?? "image.jpg";
                    imageContentType = image.ContentType ?? "image/jpeg";
                }
                
                // Create form fields in EXACT order as Postman (order matters for multipart/form-data)
                // Postman order: ingredientId, name, categoryId, quantity, unit, expireDate, notes, image
                var fields = new List<(string key, string value)>
                {
                    ("ingredientId", ingredientId),
                    ("name", name)
                };
                
                if (!string.IsNullOrWhiteSpace(categoryId))
                {
                    fields.Add(("categoryId", categoryId));
                }
                
                fields.Add(("quantity", quantity.ToString()));
                fields.Add(("unit", unit)); // API expects English: piece, kg, g, l, ml
                
                if (!string.IsNullOrWhiteSpace(formattedExpireDate))
                {
                    fields.Add(("expireDate", formattedExpireDate));
                }
                
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    fields.Add(("notes", notes));
                }
                
                // Generate boundary - same format as POST (32 hex chars, like Python requests)
                var boundaryValue = Guid.NewGuid().ToString("N"); // 32 hex chars, no dashes
                
                // Prepare files dictionary
                Dictionary<string, (byte[] bytes, string fileName, string contentType)>? files = null;
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    files = new Dictionary<string, (byte[] bytes, string fileName, string contentType)>
                    {
                        { "image", (imageBytes, imageFileName!, imageContentType!) }
                    };
                }
                
                // Create raw multipart body
                var bodyBytes = CreateMultipartFormDataBody(boundaryValue, fields, files);
                
                var bodyText = Encoding.UTF8.GetString(bodyBytes, 0, Math.Min(300, bodyBytes.Length));
                _logger.LogInformation($"📤 PUT {url}");
                _logger.LogInformation($"   Fields: {string.Join(", ", fields.Select(f => f.key))}");
                _logger.LogInformation($"   Body size: {bodyBytes.Length} bytes");
                _logger.LogInformation($"   Boundary: {boundaryValue}");
                _logger.LogInformation($"   Body preview (first 300 chars): {bodyText.Replace("\r", "\\r").Replace("\n", "\\n")}");
                
                var client = CreateAuthenticatedClient();
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                
                // Create ByteArrayContent with the raw body
                var content = new ByteArrayContent(bodyBytes);
                
                // Set Content-Type header BEFORE assigning to request
                content.Headers.Remove("Content-Type"); // Remove any default
                content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundaryValue}");
                
                request.Content = content;
                
                // Log the exact Content-Type being sent
                var contentTypeValue = request.Content.Headers.GetValues("Content-Type").FirstOrDefault();
                _logger.LogInformation($"📤 Content-Type header: {contentTypeValue}");
                
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"📥 Response: {(int)response.StatusCode} {response.StatusCode}");
                _logger.LogInformation($"📥 Body (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ PUT /ingredients succeeded: {response.StatusCode}");
                    return JsonSerializer.Deserialize<Ingredient>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogError($"❌ PUT /ingredients failed: {response.StatusCode} - {responseContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating ingredient");
                return null;
            }
        }

        public async Task<bool> DeleteIngredientAsync(string ingredientId)
        {
            try
            {
                _logger.LogInformation($"📤 DELETE {API_BASE_URL}/ingredients - ingredientId: {ingredientId}");
                var success = await DeleteAsync("/ingredients", new { ingredientId = ingredientId });
                
                if (success)
                {
                    _logger.LogInformation($"✅ DELETE /ingredients succeeded");
                }
                else
                {
                    _logger.LogError($"❌ DELETE /ingredients failed");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting ingredient");
                return false;
            }
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

        public async Task<List<Recipe>?> GetRecipeDetailsByStringIdsAsync(List<string> recipeIds)
        {
            try
            {
                _logger.LogInformation($"🔄 GetRecipeDetailsByStringIdsAsync: Received {recipeIds.Count} recipe IDs");
                _logger.LogInformation($"   Raw IDs: {string.Join(", ", recipeIds)}");
                
                // Convert string IDs to int IDs
                var intIds = new List<int>();
                foreach (var id in recipeIds)
                {
                    var cleanedId = id?.Trim() ?? "";
                    if (string.IsNullOrEmpty(cleanedId))
                    {
                        _logger.LogWarning($"⚠️ Empty recipe ID found, skipping");
                        continue;
                    }
                    
                    // Try to extract numeric part if ID contains non-numeric characters
                    if (int.TryParse(cleanedId, out var intId))
                    {
                        intIds.Add(intId);
                        _logger.LogInformation($"   ✅ Parsed '{cleanedId}' -> {intId}");
                    }
                    else
                    {
                        // Try to extract numeric part from string like "recipe_stir_fry_001" or "645872"
                        var numericMatch = System.Text.RegularExpressions.Regex.Match(cleanedId, @"\d+");
                        if (numericMatch.Success && int.TryParse(numericMatch.Value, out var extractedId))
                        {
                            intIds.Add(extractedId);
                            _logger.LogInformation($"   ✅ Extracted '{cleanedId}' -> {extractedId}");
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ Cannot parse recipe ID as int: '{cleanedId}'");
                        }
                    }
                }

                if (intIds.Count == 0)
                {
                    _logger.LogWarning("⚠️ No valid recipe IDs to fetch after parsing");
                    return new List<Recipe>();
                }

                _logger.LogInformation($"📤 GET /recipes/bulk - Parsed RecipeIds: {string.Join(", ", intIds)}");
                var recipes = await GetRecipeDetailsAsync(intIds);
                _logger.LogInformation($"✅ Received {recipes?.Count ?? 0} recipe details");
                return recipes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting recipe details by string IDs");
                return null;
            }
        }

        #endregion

        #region Favorites APIs

        public async Task<(List<Favorite>? Favorites, bool IsSpoonacularLimit)> GetFavoritesAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"{API_BASE_URL}/favorites");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"📦 GET /favorites response (first 500 chars): {content.Substring(0, Math.Min(500, content.Length))}");
                    
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    // Handle both array and wrapped object formats
                    List<Favorite>? favorites = null;
                    if (jsonDoc.ValueKind == JsonValueKind.Array)
                    {
                        favorites = JsonSerializer.Deserialize<List<Favorite>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        _logger.LogInformation($"✅ Parsed favorites as array: {favorites?.Count ?? 0} items");
                    }
                    else if (jsonDoc.ValueKind == JsonValueKind.Object)
                    {
                        // Try to get data property
                        if (jsonDoc.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            favorites = JsonSerializer.Deserialize<List<Favorite>>(dataElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            _logger.LogInformation($"✅ Parsed favorites from 'data' property: {favorites?.Count ?? 0} items");
                        }
                        // Try other common keys
                        else
                        {
                            foreach (var key in new[] { "favorites", "items", "results" })
                            {
                                if (jsonDoc.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                                {
                                    favorites = JsonSerializer.Deserialize<List<Favorite>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });
                                    _logger.LogInformation($"✅ Parsed favorites from '{key}' property: {favorites?.Count ?? 0} items");
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (favorites != null && favorites.Count > 0)
                    {
                        // Log first favorite for debugging
                        var first = favorites[0];
                        _logger.LogInformation($"🔍 First favorite - Id: '{first.GetId()}', RecipeId: '{first.GetRecipeId()}', UserId: '{first.UserId}', CreatedAt: '{first.GetCreatedAt()}'");
                        return (favorites, false);
                    }
                    
                    _logger.LogWarning($"⚠️ Unexpected response format for favorites or empty array: {content.Substring(0, Math.Min(200, content.Length))}");
                    return (new List<Favorite>(), false);
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"GET /favorites returned {response.StatusCode} - {errorContent.Substring(0, Math.Min(500, errorContent.Length))}");
                
                // Check if it's a Spoonacular API limit error
                bool isSpoonacularLimit = errorContent.Contains("daily points limit") || 
                                         errorContent.Contains("402") || 
                                         response.StatusCode == System.Net.HttpStatusCode.PaymentRequired ||
                                         errorContent.Contains("Spoonacular") ||
                                         errorContent.Contains("limit");
                
                if (isSpoonacularLimit)
                {
                    _logger.LogWarning("⚠️ Spoonacular API limit detected - attempting to extract favorites from response");
                    
                    // Try to parse favorites from error response
                    // Some APIs might return favorites list even with limit error
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        
                        // Check if error response contains favorites data
                        if (errorJson.TryGetProperty("favorites", out var favsElement) && favsElement.ValueKind == JsonValueKind.Array)
                        {
                            var favorites = JsonSerializer.Deserialize<List<Favorite>>(favsElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            if (favorites != null && favorites.Count > 0)
                            {
                                _logger.LogInformation($"✅ Found {favorites.Count} favorites in error response (Spoonacular limit)");
                                return (favorites, true);
                            }
                        }
                        
                        // Check if error response contains favoriteItems (MongoDB collection name)
                        if (errorJson.TryGetProperty("favoriteItems", out var favItemsElement) && favItemsElement.ValueKind == JsonValueKind.Array)
                        {
                            var favorites = JsonSerializer.Deserialize<List<Favorite>>(favItemsElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            if (favorites != null && favorites.Count > 0)
                            {
                                _logger.LogInformation($"✅ Found {favorites.Count} favorites in error response 'favoriteItems' (Spoonacular limit)");
                                return (favorites, true);
                            }
                        }
                        
                        // Check other common keys
                        foreach (var key in new[] { "data", "items", "results" })
                        {
                            if (errorJson.TryGetProperty(key, out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                            {
                                var favorites = JsonSerializer.Deserialize<List<Favorite>>(itemsElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                                if (favorites != null && favorites.Count > 0)
                                {
                                    _logger.LogInformation($"✅ Found {favorites.Count} favorites in error response key '{key}' (Spoonacular limit)");
                                    return (favorites, true);
                                }
                            }
                        }
                        
                        // Try to extract favorites from error message if it contains JSON
                        // Some APIs might embed favorites data in error message
                        if (errorContent.Contains("favoriteItems") || errorContent.Contains("\"recipe\""))
                        {
                            _logger.LogInformation("🔍 Error response may contain favorites data, attempting to extract...");
                            // This is a fallback - if the error message itself contains JSON with favorites
                            try
                            {
                                // Look for JSON arrays in the error message
                                var jsonMatch = System.Text.RegularExpressions.Regex.Match(errorContent, @"\[.*?\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                                if (jsonMatch.Success)
                                {
                                    var possibleFavorites = JsonSerializer.Deserialize<List<Favorite>>(jsonMatch.Value, new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });
                                    if (possibleFavorites != null && possibleFavorites.Count > 0)
                                    {
                                        _logger.LogInformation($"✅ Extracted {possibleFavorites.Count} favorites from error message (Spoonacular limit)");
                                        return (possibleFavorites, true);
                                    }
                                }
                            }
                            catch (Exception extractEx)
                            {
                                _logger.LogWarning($"⚠️ Could not extract favorites from error message: {extractEx.Message}");
                            }
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning($"⚠️ Could not parse favorites from error response: {parseEx.Message}");
                    }
                    
                    // If we can't get favorites from error response, return empty list with Spoonacular limit flag
                    // This allows UI to show proper message and still function
                    _logger.LogWarning("⚠️ Spoonacular API limit reached - returning empty list (favorites may exist but cannot be fetched)");
                    return (new List<Favorite>(), true); // Return empty list with Spoonacular limit flag
                }
                
                // For other errors, log and return null
                _logger.LogError($"GET /favorites failed: {response.StatusCode} - {errorContent.Substring(0, Math.Min(200, errorContent.Length))}");
                return (null, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites");
                return (null, false);
            }
        }

        public async Task<Favorite?> AddFavoriteAsync(string recipeId)
        {
            try
            {
                _logger.LogInformation($"🔄 POST /favorites - RecipeId: {recipeId}");
                
                var client = CreateAuthenticatedClient();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                var json = JsonSerializer.Serialize(new { recipeId }, jsonOptions);
                _logger.LogInformation($"📤 POST {API_BASE_URL}/favorites");
                _logger.LogInformation($"📤 Request body: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{API_BASE_URL}/favorites", content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"📥 Response status: {response.StatusCode}");
                _logger.LogInformation($"📥 Response body (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ POST /favorites succeeded: {response.StatusCode}");
                    
                    // Try to parse as Favorite object first
                    try
                    {
                        var favorite = JsonSerializer.Deserialize<Favorite>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (favorite != null && !string.IsNullOrEmpty(favorite.GetId()))
                        {
                            _logger.LogInformation($"✅ Successfully parsed favorite object: RecipeId {recipeId}");
                            return favorite;
                        }
                    }
                    catch (JsonException)
                    {
                        // If deserialization fails, check if it's a message response
                        _logger.LogInformation("ℹ️ Response is not a Favorite object, checking for message...");
                    }
                    
                    // Check if response is a message object (e.g., {"message": "Favorite added successfully"})
                    try
                    {
                        var jsonDoc = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (jsonDoc.ValueKind == JsonValueKind.Object)
                        {
                            // If API returns success message, create a minimal Favorite object
                            // The favorite was added successfully, we just don't have the full object
                            // We'll fetch it later when loading favorites list
                            _logger.LogInformation($"✅ API returned success message for RecipeId {recipeId}, creating minimal Favorite object");
                            
                            return new Favorite
                            {
                                RecipeId = recipeId,
                                Recipe_Id = recipeId,
                                UserId = "", // Will be set by API
                                CreatedAt = DateTime.UtcNow,
                                Created_At = DateTime.UtcNow
                            };
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning($"⚠️ Could not parse response: {parseEx.Message}");
                    }
                    
                    // If we can't parse, still return a minimal Favorite to indicate success
                    _logger.LogInformation($"✅ Creating minimal Favorite object for RecipeId {recipeId}");
                    return new Favorite
                    {
                        RecipeId = recipeId,
                        Recipe_Id = recipeId,
                        UserId = "",
                        CreatedAt = DateTime.UtcNow,
                        Created_At = DateTime.UtcNow
                    };
                }
                
                _logger.LogError($"❌ POST /favorites failed: {response.StatusCode}");
                _logger.LogError($"❌ Response body: {responseContent}");
                return null;
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
            try
            {
                if (string.IsNullOrWhiteSpace(mealPlanId))
                {
                    _logger.LogError("❌ DeleteMealPlanAsync: mealPlanId is null or empty");
                    return false;
                }

                _logger.LogInformation($"📤 DELETE {API_BASE_URL}/meal-plans");
                _logger.LogInformation($"   MealPlanId: {mealPlanId}");
                
                // API expects DELETE /meal-plans with JSON body containing mealPlanId
                return await DeleteAsync("/meal-plans", new { mealPlanId = mealPlanId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error calling DELETE /meal-plans for mealPlanId: {mealPlanId}");
                return false;
            }
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
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("_id")]
            public string? _Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("ingredientCategoryId")]
            public string? IngredientCategoryId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("categoryId")]
            public string? CategoryId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string Name { get; set; } = "";
            
            [System.Text.Json.Serialization.JsonPropertyName("icon")]
            public string Icon { get; set; } = "";
            
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public string UserId { get; set; } = "";
            
            // Computed property to get ID from any field
            public string GetId()
            {
                return Id ?? _Id ?? IngredientCategoryId ?? CategoryId ?? "";
            }
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
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("_id")]
            public string? _Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("ingredientId")]
            public string? IngredientId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("categoryId")]
            public string? CategoryId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("quantity")]
            public decimal Quantity { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("unit")]
            public string? Unit { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("expireDate")]
            public string? ExpireDate { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("expiryDate")]
            public string? ExpiryDate { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("notes")]
            public string? Notes { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("imageUrl")]
            public string? ImageUrl { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public string? UserId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
            public string? CreatedAt { get; set; }
            
            // Computed property to get ID from any field
            public string GetId()
            {
                return Id ?? _Id ?? IngredientId ?? "";
            }
        }

        public class Recipe
        {
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public int? Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("title")]
            public string? Title { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("image")]
            public string? Image { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("readyInMinutes")]
            public int? ReadyInMinutes { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("servings")]
            public int? Servings { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("summary")]
            public string? Summary { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("spoonacularScore")]
            public double? SpoonacularScore { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("dishTypes")]
            public List<string>? DishTypes { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("extendedIngredients")]
            public List<ExtendedIngredient>? ExtendedIngredients { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("instructions")]
            public string? Instructions { get; set; }
        }
        
        public class ExtendedIngredient
        {
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public int? Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("amount")]
            public double? Amount { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("unit")]
            public string? Unit { get; set; }
        }

        public class Favorite
        {
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("_id")]
            public string? _Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("recipeId")]
            public string? RecipeId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("recipe_id")]
            public string? Recipe_Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public string? UserId { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
            public DateTime? CreatedAt { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("created_at")]
            public DateTime? Created_At { get; set; }
            
            // Computed property to get ID
            public string GetId()
            {
                return Id ?? _Id ?? "";
            }
            
            // Computed property to get RecipeId
            public string GetRecipeId()
            {
                return RecipeId ?? Recipe_Id ?? "";
            }
            
            // Computed property to get CreatedAt
            public DateTime GetCreatedAt()
            {
                return CreatedAt ?? Created_At ?? DateTime.UtcNow;
            }
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
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("_id")]
            public string? _Id { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("recipeIds")]
            public List<string>? RecipeIds { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("notes")]
            public string? Notes { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("date")]
            public string? Date { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public string? UserId { get; set; }
            
            // Computed property to get ID
            public string GetId()
            {
                return Id ?? _Id ?? "";
            }
        }

        public class CreateMealPlanRequest
        {
            public string Name { get; set; } = "";
            public List<string> RecipeIds { get; set; } = new();
            public string? Notes { get; set; }
            public string Date { get; set; } = ""; // ISO 8601 string format
        }

        public class UpdateMealPlanRequest
        {
            [System.Text.Json.Serialization.JsonPropertyName("mealPlanId")]
            public string MealPlanId { get; set; } = "";
            public string Name { get; set; } = "";
            public List<string> RecipeIds { get; set; } = new();
            public string? Notes { get; set; }
            public string Date { get; set; } = ""; // ISO 8601 string format
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

