# 🎉 CookMate API Integration - HOÀN CHỈNH

## ✅ Đã Hoàn Thành

### 1. **Authentication - Hoàn toàn sử dụng CookMate API**

#### 🌐 Google OAuth Login
- **Endpoint**: `POST https://cookm8.vercel.app/api/auth/google`
- **Request**:
  ```json
  {
    "googleUserId": "123456789",
    "email": "user@gmail.com",
    "name": "User Name",
    "avatar": "https://..."
  }
  ```
- **Response**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "message": "Login successful",
    "user": {
      "id": "abc123",
      "email": "user@gmail.com",
      "name": "User Name",
      "avatar": "...",
      "dietaryPreferences": []
    }
  }
  ```
- **Controller**: `AuthController.GoogleResponse()` 
- **Flow**:
  1. User clicks "Đăng nhập với Google"
  2. Google OAuth → Get user info
  3. Call CookMate API `/api/auth/google`
  4. Nhận JWT token
  5. Lưu token vào session + tạo cookie
  6. Redirect đến Home

#### 🔐 OTP Email Login
- **Endpoint 1**: `POST https://cookm8.vercel.app/api/auth/otp`
  - **Request**:
    ```json
    {
      "email": "user@example.com"
    }
    ```
  - **Response**:
    ```json
    {
      "message": "OTP sent to email"
    }
    ```

- **Endpoint 2**: `POST https://cookm8.vercel.app/api/auth/otp/verify`
  - **Request**:
    ```json
    {
      "email": "user@example.com",
      "otp": "123456"
    }
    ```
  - **Response**:
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "message": "Login successful",
      "user": { ... }
    }
    ```

- **Controller**: 
  - `AuthController.SendOTP()` 
  - `AuthController.VerifyOTP()`
  
- **Flow**:
  1. User nhập email → Call `/api/auth/otp`
  2. Nhận OTP qua email
  3. Nhập OTP → Call `/api/auth/otp/verify`
  4. Nhận JWT token
  5. Lưu token vào session + tạo cookie
  6. Redirect đến Home

### 2. **Token Management**

- **JWT Token từ API** được lưu vào:
  - `HttpContext.Session["access_token"]`
  - `HttpContext.Session["user_email"]`
  - `HttpContext.Session["user_name"]`
  - `HttpContext.Session["user_id"]`

- **Cookie Authentication**:
  - Cookie-based session cho Web UI
  - Cookie chứa Claims: Email, Name, access_token, user_id

### 3. **Removed Local Database/Identity**

✅ **Đã xóa hoàn toàn**:
- ❌ Local SQLite database
- ❌ Entity Framework Core
- ❌ ASP.NET Core Identity
- ❌ Local user management
- ❌ Email/Password login
- ❌ Local data seeding services

✅ **Chỉ giữ lại**:
- Cookie Authentication (cho Web UI session)
- Google OAuth (delegate sang CookMate API)
- Session Management

---

## 📋 TODO: Các chức năng cần integrate với CookMate API

### ❌ 1. User Profile API
- **Endpoint**: `GET /api/user/profile`
- **Header**: `Authorization: Bearer {token}`
- **Cần update**: `HomeController`, User profile pages

### ❌ 2. Ingredients/Pantry API
- **Endpoints**:
  - `GET /api/ingredients` - Lấy danh sách nguyên liệu
  - `POST /api/ingredients` - Thêm nguyên liệu
  - `PUT /api/ingredients/:id` - Sửa nguyên liệu
  - `DELETE /api/ingredients/:id` - Xóa nguyên liệu
- **Cần update**: `IngredientController`, Pantry pages

### ❌ 3. Ingredient Categories API
- **Endpoints**:
  - `GET /api/ingredient-categories` - Lấy danh mục
  - `POST /api/ingredient-categories` - Thêm danh mục
- **Cần update**: `IngredientCategoryController`

### ❌ 4. Recipes API
- **Endpoints**: (cần check API docs)
  - `GET /api/recipes` - Lấy công thức
  - `POST /api/recipes` - Tạo công thức
- **Cần update**: Recipe controllers/pages

### ❌ 5. Favorites API
- **Endpoints**: (cần check API docs)
  - `GET /api/favorites` - Lấy yêu thích
  - `POST /api/favorites` - Thêm yêu thích
- **Cần update**: `FavoriteController`

---

## 🔧 Hướng Dẫn Sử Dụng

### 1. Chạy Web Application
```bash
cd /Users/phamtau/WebCookmate
dotnet run --urls "http://localhost:5134"
```

### 2. Truy cập
- **URL**: http://localhost:5134
- **Login**: Chỉ có Google OAuth và OTP Email

### 3. Test Login Flow

#### Google Login:
1. Click "Đăng nhập với Google"
2. Chọn tài khoản Google
3. → Tự động call CookMate API
4. → Lưu token
5. → Redirect về Home

#### OTP Login:
1. Click "Đăng nhập bằng OTP (Email)"
2. Nhập email → Click "Gửi mã OTP"
3. Check email → Nhập OTP (6 chữ số)
4. Click "Xác thực OTP"
5. → Tự động call CookMate API
6. → Lưu token
7. → Redirect về Home

### 4. Xem Logs
```bash
tail -f /tmp/cookmate.log
```

Logs sẽ hiển thị:
- 🌐 API calls đến CookMate API
- 📥 API responses
- ✅ Successful authentication
- ❌ Errors

---

## 📂 File Structure

### Controllers
- ✅ `AuthController.cs` - **HOÀN CHỈNH** (Google + OTP → CookMate API)
- ⏳ `HomeController.cs` - Cần update để call API
- ⏳ `IngredientController.cs` - Cần update để call API
- ⏳ `IngredientCategoryController.cs` - Cần update để call API
- ⏳ `FavoriteController.cs` - Cần update để call API

### Views
- ✅ `Views/Auth/Login.cshtml` - Hiển thị Google + OTP buttons
- ✅ `Views/Auth/OTPLogin.cshtml` - **MỚI** - OTP flow UI
- ⏳ `Views/Home/*` - Cần update
- ⏳ `Views/Home/Pantry.cshtml` - Cần update

### Services
- ⏳ `ApiService.cs` - Cần refactor để call CookMate API đầy đủ
- ❌ `IngredientSeedService.cs` - **DEPRECATED** (không cần nữa)
- ❌ `FavoriteSeedService.cs` - **DEPRECATED**

### Configuration
- ✅ `Program.cs` - **ĐÃ CLEAN** - Chỉ Cookie + Google OAuth
- ✅ `appsettings.json` - Google ClientId/Secret

---

## 🧪 Test Checklist

### Authentication ✅
- [x] Google Login → Call API → Get token
- [x] OTP Send → Call API → Email sent
- [x] OTP Verify → Call API → Get token
- [x] Token lưu vào session
- [x] Cookie tạo cho Web UI
- [x] Logout → Clear session + cookie

### Data Operations ⏳
- [ ] Load user profile từ API
- [ ] Load ingredients từ API
- [ ] Add/Edit/Delete ingredients qua API
- [ ] Load categories từ API
- [ ] Load recipes từ API
- [ ] Manage favorites qua API

---

## 🚀 Next Steps

1. **Tạo `CookMateApiService.cs`**
   - Centralized service để call tất cả CookMate API endpoints
   - Tự động thêm `Authorization: Bearer {token}` header
   - Handle errors, retries, timeouts

2. **Update Controllers**
   - Replace local database calls với API calls
   - Sử dụng `CookMateApiService`

3. **Test từng chức năng**
   - Login → Load data → CRUD operations
   - Verify data được lưu trên CookMate API server

4. **Remove deprecated code**
   - Xóa seeding services
   - Xóa local database models (nếu không cần)

---

## 📞 CookMate API Documentation

**URL**: https://cookm8.vercel.app/api-docs

**Implemented Endpoints**:
- ✅ `POST /api/auth/google` - Google OAuth
- ✅ `POST /api/auth/otp` - Send OTP
- ✅ `POST /api/auth/otp/verify` - Verify OTP

**TODO Endpoints** (cần research từ API docs):
- User profile
- Ingredients
- Categories
- Recipes
- Favorites
- Meal plans
- ...

---

## 💡 Tips

- **Token expiration**: Cần implement refresh token logic (nếu API hỗ trợ)
- **Error handling**: Cần handle 401 Unauthorized → Redirect login
- **Network errors**: Retry logic cho API calls
- **Loading states**: UI cần loading spinners khi call API
- **Caching**: Consider caching API responses để giảm calls

---

**Last Updated**: 2025-10-29  
**Status**: Authentication HOÀN CHỈNH ✅ | Data Operations TODO ⏳

