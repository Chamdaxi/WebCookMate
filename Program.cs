using Microsoft.AspNetCore.Authentication.Cookies;
using demo.Services;

var builder = WebApplication.CreateBuilder(args);

// ================================
// COOKMATE WEB - CALLS API SERVER
// Base URL: https://cookm8.vercel.app
// ================================

// MVC with JSON camelCase serialization
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Session để lưu JWT token
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
});

// Cookie Authentication (KHÔNG dùng Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    })
    .AddGoogle(options =>
    {
        options.ClientId = "398405054635-uiiigmv8ri287l68neg028n2ol41rlcn.apps.googleusercontent.com";
        options.ClientSecret = "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";
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        
        // SSL fix for macOS
        var socketsHandler = new System.Net.Http.SocketsHttpHandler
        {
            UseCookies = false,
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            }
        };
        options.BackchannelHttpHandler = socketsHandler;
    });

// Services
builder.Services.AddHttpContextAccessor(); // Cần cho CookMateApiService
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<CookMateApiService>(); // Service chính để call tất cả API

// Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // MUST be before Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "google-callback",
    pattern: "signin-google",
    defaults: new { controller = "Auth", action = "GoogleResponse" });

Console.WriteLine("===========================================");
Console.WriteLine("🚀 CookMate Web - Calling API Server");
Console.WriteLine("📍 Web UI: http://localhost:5134");
Console.WriteLine("🌐 API Server: https://cookm8.vercel.app");
Console.WriteLine("===========================================");

app.Run();
