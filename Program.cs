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
        // Xóa database cũ nếu có và tạo lại với schema mới
        context.Database.EnsureDeleted();
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
        
        // Seed recipes mẫu nếu chưa có
        if (!context.Recipes.Any())
        {
            var sampleRecipes = new List<demo.Models.Recipe>
            {
                new demo.Models.Recipe
                {
                    Name = "Phở Bò",
                    Description = "Phở bò truyền thống Việt Nam với nước dùng đậm đà, thịt bò mềm và bánh phở tươi ngon",
                    ImageUrl = "/images/recipes/phobo.jpeg",
                    Category = "Món chính",
                    CookingTime = 60,
                    Difficulty = "Medium",
                    Ingredients = "[\"Bánh phở\", \"Thịt bò\", \"Hành tây\", \"Rau thơm\", \"Gia vị phở\", \"Quế\", \"Hồi\", \"Gừng\"]",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new demo.Models.Recipe
                {
                    Name = "Bánh Mì",
                    Description = "Bánh mì thịt nướng giòn rụm với pate thơm, thịt nướng đậm đà và rau củ tươi ngon",
                    ImageUrl = "/images/recipes/banhmi.jpg",
                    Category = "Món chính",
                    CookingTime = 20,
                    Difficulty = "Easy",
                    Ingredients = "[\"Bánh mì\", \"Thịt nướng\", \"Pate\", \"Rau củ\", \"Gia vị\", \"Ớt\", \"Ngò\"]",
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new demo.Models.Recipe
                {
                    Name = "Cơm Tấm",
                    Description = "Cơm tấm sườn nướng thơm lừng với sườn heo nướng vàng, trứng ốp la, bì, chả",
                    ImageUrl = "/images/recipes/Comtam.jpg",
                    Category = "Món chính",
                    CookingTime = 45,
                    Difficulty = "Medium",
                    Ingredients = "[\"Cơm tấm\", \"Sườn nướng\", \"Trứng\", \"Bì\", \"Chả\", \"Dưa chua\", \"Nước mắm pha\"]",
                    CreatedAt = DateTime.Now.AddDays(-7)
                },
                new demo.Models.Recipe
                {
                    Name = "Kho Quẹt",
                    Description = "Kho quẹt tôm thịt đậm đà, béo ngậy với tôm tươi và thịt ba chỉ, ăn kèm cơm nóng",
                    ImageUrl = "/images/recipes/Khoquet.webp",
                    Category = "Món ăn kèm",
                    CookingTime = 40,
                    Difficulty = "Easy",
                    Ingredients = "[\"Tôm\", \"Thịt ba chỉ\", \"Nước mắm\", \"Đường\", \"Tỏi\", \"Ớt\", \"Hành lá\"]",
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new demo.Models.Recipe
                {
                    Name = "Bún Bò Huế",
                    Description = "Bún bò Huế cay nồng với nước dùng đậm đà, thịt bò mềm, chả cua và rau sống",
                    ImageUrl = "/images/recipes/today-suggestion.jpg",
                    Category = "Món chính",
                    CookingTime = 90,
                    Difficulty = "Hard",
                    Ingredients = "[\"Bún\", \"Thịt bò\", \"Chả cua\", \"Rau sống\", \"Hành tây\", \"Sả\", \"Ớt\", \"Mắm ruốc\"]",
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };
            
            context.Recipes.AddRange(sampleRecipes);
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
