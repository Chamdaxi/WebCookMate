using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;
using demo.Services;

var builder = WebApplication.CreateBuilder(args);

// Thêm services cho MVC
builder.Services.AddControllersWithViews();

// Thêm Data Protection cho Google OAuth
builder.Services.AddDataProtection();

// Thêm Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

// Thêm Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình password
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    
    // Cấu hình user
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    
    // Cấu hình cookie cho Google OAuth
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

// Thêm Google Authentication - FIX cho .NET 8 + macOS Sequoia
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "398405054635-uiiigmv8ri287l68neg028n2ol41rlcn.apps.googleusercontent.com";
        options.ClientSecret = "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";
        
        // Cấu hình redirect URI
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
        
        // Cấu hình scope
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        
        // Cấu hình cookie cho Google OAuth
        options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        
        // FIX SSL cho .NET 8 + macOS Sequoia: Sử dụng SocketsHttpHandler thay vì HttpClientHandler
        var socketsHandler = new System.Net.Http.SocketsHttpHandler
        {
            UseCookies = false,
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                // Bypass SSL certificate validation cho development
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            }
        };
        options.BackchannelHttpHandler = socketsHandler;
    });

    // Thêm HttpClient và Services
    builder.Services.AddHttpClient<TokenService>();
    builder.Services.AddScoped<TokenService>();
    
    builder.Services.AddHttpClient<ApiService>();
    builder.Services.AddScoped<ApiService>();
    
    // OTP Services
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<EmailService>();
    builder.Services.AddScoped<SmsService>();
    builder.Services.AddScoped<OTPService>();
    
    // Favorite Seed Service - Tự động thêm món ăn yêu thích mẫu cho user mới
    builder.Services.AddScoped<FavoriteSeedService>();

// Cấu hình HTTP cho development (tạm thời)
// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenLocalhost(5134, listenOptions =>
//     {
//         listenOptions.UseHttps();
//     });
// });

var app = builder.Build();

// Cấu hình HTTPS cho development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Tắt HTTPS redirection tạm thời
app.UseStaticFiles(); // Hỗ trợ static files (CSS, JS, images)

app.UseRouting();

// Thêm Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// Thêm Session middleware
app.UseSession();

// Thêm Bearer Token middleware - đã xóa hoàn toàn để test Google OAuth
// app.UseMiddleware<BearerTokenMiddleware>();

// Cấu hình routing cho MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Thêm routing cho Google OAuth
app.MapControllerRoute(
    name: "google-oauth",
    pattern: "signin-google",
    defaults: new { controller = "Auth", action = "GoogleResponse" });

app.Run();
