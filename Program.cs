using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using demo.Data;
using demo.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Thêm services cho MVC
builder.Services.AddControllersWithViews();

// Thêm Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
});

var app = builder.Build();

// Tự động tạo database nếu chưa tồn tại và seed dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        // Seed dữ liệu mẫu nếu chưa có
        if (!context.ShoppingItems.Any())
        {
            var sampleItems = new List<demo.Models.ShoppingItem>
            {
                new demo.Models.ShoppingItem
                {
                    Name = "Thịt bò",
                    Category = "meat",
                    Notes = "Thịt bò tươi, khoảng 500g",
                    ImageUrl = "/images/recipes/today-suggestion.jpg",
                    Quantity = 500,
                    Unit = "g",
                    Price = 150000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Bánh mì",
                    Category = "grains",
                    Notes = "Bánh mì tươi, 10 ổ",
                    ImageUrl = "/images/recipes/banhmi.jpg",
                    Quantity = 10,
                    Unit = "ổ",
                    Price = 50000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Gạo tấm",
                    Category = "grains",
                    Notes = "Gạo tấm thơm, túi 5kg",
                    ImageUrl = "/images/recipes/Comtam.jpg",
                    Quantity = 5,
                    Unit = "kg",
                    Price = 120000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Tôm tươi",
                    Category = "meat",
                    Notes = "Tôm tươi, khoảng 300g",
                    ImageUrl = "/images/recipes/Khoquet.webp",
                    Quantity = 300,
                    Unit = "g",
                    Price = 120000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Bánh phở",
                    Category = "grains",
                    Notes = "Bánh phở tươi, 1kg",
                    ImageUrl = "/images/recipes/phobo.jpeg",
                    Quantity = 1,
                    Unit = "kg",
                    Price = 30000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Rau thơm",
                    Category = "vegetables",
                    Notes = "Rau thơm các loại: húng, ngò, hành",
                    ImageUrl = null,
                    Quantity = 1,
                    Unit = "bó",
                    Price = 10000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Sữa tươi",
                    Category = "dairy",
                    Notes = "Sữa tươi, hộp 1L",
                    ImageUrl = null,
                    Quantity = 1,
                    Unit = "lít",
                    Price = 35000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                },
                new demo.Models.ShoppingItem
                {
                    Name = "Nước mắm",
                    Category = "spices",
                    Notes = "Nước mắm Phú Quốc, chai 500ml",
                    ImageUrl = null,
                    Quantity = 1,
                    Unit = "chai",
                    Price = 45000,
                    IsCompleted = false,
                    InCart = false,
                    CreatedAt = DateTime.Now
                }
            };
            
            context.ShoppingItems.AddRange(sampleItems);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Log error if needed
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

// Cấu hình pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Tắt HTTPS redirection để tránh lỗi
app.UseStaticFiles(); // Hỗ trợ static files (CSS, JS, images)

app.UseRouting();

// Thêm Session middleware
app.UseSession();

// Thêm Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình routing cho MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
