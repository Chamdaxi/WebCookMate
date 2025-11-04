# 🧪 TEST API FLOW - COOKMATE

## ✅ ĐÃ FIX VẤN ĐỀ

### 🐛 Vấn đề ban đầu:
```
JavaScript đang gọi endpoints CŨ (local database):
  ❌ /api/IngredientCategory
  ❌ /api/Ingredient
```

### ✅ Đã fix:
```
JavaScript giờ gọi endpoints MỚI (CookMate API):
  ✅ /api/IngredientCategoryApi → calls CookMate API
  ✅ /api/IngredientApi → calls CookMate API
```

---

## 🔍 KIỂM TRA API FLOW

### 1. Đăng nhập trước

**QUAN TRỌNG**: Phải đăng nhập trước để có JWT token!

```
1. Truy cập: http://localhost:5134
2. Chọn một trong hai:
   - Google OAuth
   - OTP Email
3. Sau khi đăng nhập thành công → Token được lưu vào session
```

---

### 2. Test Pantry Page

#### A. Mở Browser Console (F12)

#### B. Truy cập Pantry
```
http://localhost:5134/Home/Pantry
```

#### C. Xem Console Logs
Bạn sẽ thấy:
```javascript
✅ Loaded categories from CookMate API: X
✅ Loaded ingredients from CookMate API: Y
```

#### D. Xem Network Tab
- Filter: `XHR` hoặc `Fetch`
- Tìm requests:
  - `GET /api/IngredientCategoryApi`
  - `GET /api/IngredientApi`

#### E. Check Request Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Cookie: .AspNetCore.Cookies=...
```

---

### 3. Test với cURL (sau khi đăng nhập)

#### Lấy token từ Browser
```javascript
// Trong Console, chạy:
document.cookie
```

#### Test Categories API
```bash
curl -X GET http://localhost:5134/api/IngredientCategoryApi \
  -H "Cookie: YOUR_COOKIE_HERE" \
  -v
```

**Expected Response**:
```json
[
  {
    "id": "cat_123",
    "name": "Trái cây",
    "icon": "🍎",
    "userId": "user_123"
  }
]
```

#### Test Ingredients API
```bash
curl -X GET http://localhost:5134/api/IngredientApi \
  -H "Cookie: YOUR_COOKIE_HERE" \
  -v
```

**Expected Response**:
```json
[
  {
    "id": "ing_123",
    "name": "Táo",
    "categoryId": "cat_123",
    "quantity": 5,
    "unit": "quả",
    "expireDate": "2025-12-31T00:00:00Z",
    "notes": "Táo xanh Úc",
    "imageUrl": "https://...",
    "userId": "user_123"
  }
]
```

---

### 4. Test Backend Logs

```bash
tail -f /tmp/cookmate.log
```

**Khi load Pantry page, bạn sẽ thấy**:
```
info: demo.Controllers.IngredientCategoryApiController[0]
      ✅ Fetched X categories from CookMate API

info: demo.Controllers.IngredientApiController[0]
      ✅ Fetched Y ingredients from CookMate API
```

---

## 🔧 TROUBLESHOOTING

### Issue 1: 401 Unauthorized
**Nguyên nhân**: Chưa đăng nhập hoặc token hết hạn

**Giải pháp**:
```
1. Clear cookies/session
2. Đăng nhập lại
3. Refresh Pantry page
```

### Issue 2: Không có dữ liệu (categories/ingredients = 0)
**Nguyên nhân**: CookMate API server chưa có data cho user này

**Giải pháp**:
```
1. Vào Pantry
2. Click "Quản lý danh mục" → Thêm categories
3. Click "Thêm nguyên liệu" → Thêm ingredients
4. Data sẽ được lưu trên CookMate API server
5. Refresh → Thấy data
```

### Issue 3: Network Error
**Nguyên nhân**: CookMate API server không phản hồi

**Kiểm tra**:
```bash
# Test API server trực tiếp
curl https://cookm8.vercel.app/api/ingredient-categories

# Expected: {"error": "Unauthorized"} (OK, server hoạt động)
```

### Issue 4: CORS Error
**Nguyên nhân**: Browser block request

**Giải pháp**:
```
Không có vấn đề vì web client gọi qua backend proxy:
Browser → http://localhost:5134/api/IngredientCategoryApi
         → Backend gọi: https://cookm8.vercel.app/api/ingredient-categories
```

---

## 📊 FLOW CHART

### Complete Data Flow

```
USER
  ↓
1. Đăng nhập (Google/OTP)
  ↓
AuthController.GoogleResponse()
  ↓ POST https://cookm8.vercel.app/api/auth/google
CookMate API
  ↓ Response: { token: "JWT_TOKEN", user: {...} }
Session Storage: access_token = JWT_TOKEN
  ↓
Cookie Created
  ↓
2. User truy cập Pantry
  ↓
Browser loads pantry.js
  ↓
loadCategories() → fetch('/api/IngredientCategoryApi')
  ↓
IngredientCategoryApiController.GetCategories()
  ↓
CookMateApiService.GetIngredientCategoriesAsync()
  ↓ GET https://cookm8.vercel.app/api/ingredient-categories
  ↓ Authorization: Bearer JWT_TOKEN
CookMate API
  ↓ Response: [ { id, name, icon, userId }, ... ]
Backend Controller
  ↓ return Ok(categories);
JavaScript
  ↓ allCategories = await response.json()
  ↓ renderCategoryTabs()
  ↓ renderCategoryList()
UI Updated ✅
```

---

## ✅ CHECKLIST

Sau khi test, verify:

- [ ] Đăng nhập thành công (Google hoặc OTP)
- [ ] Browser Console hiển thị: `✅ Loaded categories from CookMate API`
- [ ] Browser Console hiển thị: `✅ Loaded ingredients from CookMate API`
- [ ] Network tab shows: `GET /api/IngredientCategoryApi` → 200 OK
- [ ] Network tab shows: `GET /api/IngredientApi` → 200 OK
- [ ] Request headers có: `Authorization: Bearer ...`
- [ ] Backend logs shows: `✅ Fetched X categories from CookMate API`
- [ ] Backend logs shows: `✅ Fetched Y ingredients from CookMate API`
- [ ] UI hiển thị categories (nếu có data)
- [ ] UI hiển thị ingredients (nếu có data)
- [ ] Có thể thêm category mới
- [ ] Có thể thêm ingredient mới
- [ ] Data được lưu trên CookMate API server (refresh vẫn còn)

---

## 🎯 KẾT LUẬN

**Nếu checklist đều PASS**:
✅ Hệ thống hoạt động HOÀN HẢO!
✅ Tất cả data được lưu trên CookMate API Server!
✅ KHÔNG có local database!

**Nếu không có data**:
- Đó là vì user account mới, chưa có data
- Thêm categories và ingredients mới
- Data sẽ được lưu trên API server
- Lần sau đăng nhập vào cùng account → Thấy data

---

**Date**: 2025-10-29  
**Version**: 2.0.0  
**Status**: ✅ HOÀN CHỈNH - TẤT CẢ CALL COOKMATE API

