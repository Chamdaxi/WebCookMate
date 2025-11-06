# ✅ CHECKLIST KHI MỞ LẠI PROJECT

## 📋 KIỂM TRA TRƯỚC KHI CHẠY LẠI

### 1. ✅ .NET SDK
```bash
dotnet --version
# Phải là 8.0.415 hoặc tương thích
# Nếu không có, cài đặt: https://dotnet.microsoft.com/download
```

### 2. ✅ Dependencies
```bash
cd /Users/phamtau/WebCookmate
dotnet restore
# Đảm bảo tất cả packages được restore
```

### 3. ✅ Build Project
```bash
dotnet build
# Phải build thành công, không có lỗi
```

### 4. ✅ Chạy Web
```bash
dotnet run
# Web sẽ chạy tại: http://localhost:5134
```

## 🔧 CÁC FILE QUAN TRỌNG ĐÃ ĐƯỢC LƯU

- ✅ `demo.csproj` - Dependencies đã được lưu
- ✅ `global.json` - .NET SDK version đã được cấu hình
- ✅ `Program.cs` - Cấu hình services và middleware
- ✅ `Services/CookMateApiService.cs` - API service
- ✅ `Controllers/` - Tất cả controllers
- ✅ `Views/` - Tất cả views
- ✅ `wwwroot/css/` - CSS files
- ✅ `wwwroot/js/` - JavaScript files

## ⚠️ LƯU Ý KHI MỞ LẠI

### 1. Port 5134
- Nếu port 5134 đã bị chiếm, kill process:
```bash
lsof -ti:5134 | xargs kill -9
```

### 2. API Server
- API server: `https://cookm8.vercel.app`
- Đảm bảo API server đang hoạt động
- Không cần cấu hình thêm, đã hardcode trong code

### 3. Session/Cookies
- Session được lưu trong memory (mặc định)
- Khi restart server, session sẽ bị mất
- User cần đăng nhập lại

### 4. Database
- Không sử dụng local database
- Tất cả data lưu trên API server
- Không cần migrate database

## 🚀 CÁCH CHẠY LẠI NHANH

```bash
# 1. Di chuyển vào thư mục project
cd /Users/phamtau/WebCookmate

# 2. Kill process cũ (nếu có)
lsof -ti:5134 | xargs kill -9 2>/dev/null || true

# 3. Restore dependencies (nếu cần)
dotnet restore

# 4. Build project
dotnet build

# 5. Chạy web
dotnet run
```

## 📝 CÁC TÍNH NĂNG ĐÃ HOÀN THIỆN

- ✅ Đăng nhập Google
- ✅ Đăng nhập OTP
- ✅ Trang Home với Meal Plan và Recipe Today
- ✅ Trang Pantry (Quản lý nguyên liệu)
- ✅ Trang Favorites (Món yêu thích)
- ✅ Trang Meal Plans (Kế hoạch bữa ăn)
- ✅ Trang Recipes (Công thức nấu ăn)
- ✅ Trang Shopping List (Danh sách mua sắm)
- ✅ Trang User Profile (Thông tin cá nhân)
- ✅ Xóa tài khoản

## 🔗 LINKS QUAN TRỌNG

- Web UI: http://localhost:5134
- API Server: https://cookm8.vercel.app
- API Docs: https://cookm8.vercel.app/api-docs

## ❓ NẾU GẶP LỖI

1. **Lỗi .NET SDK**: Kiểm tra `global.json` và cài đặt đúng version
2. **Lỗi Port**: Kill process đang chiếm port 5134
3. **Lỗi Build**: Chạy `dotnet clean` và `dotnet restore` lại
4. **Lỗi API**: Kiểm tra kết nối internet và API server

