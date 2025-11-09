# Test Plan: Thêm Ingredient

## ✅ Đã Sửa Các Lỗi

1. **Build Errors**: Đã sửa lỗi `Ingredient` type not found
2. **Logging**: Đã thêm logging chi tiết cho request/response
3. **Error Handling**: Đã cải thiện error handling và messages
4. **ExpireDate Format**: Đã sửa format từ "o" sang "yyyy-MM-dd"

## 🧪 Test Steps

### Bước 1: Restart Web Application

```bash
# Dừng web hiện tại (Ctrl+C nếu đang chạy)
cd /Users/phamtau/WebCookmate
export PATH="$HOME/.dotnet:$PATH"
dotnet run
```

### Bước 2: Mở Browser và Login

1. Mở browser: `http://localhost:5134`
2. Đăng nhập bằng OTP hoặc Google
3. Đảm bảo đã đăng nhập thành công

### Bước 3: Test Thêm Ingredient

1. **Mở trang Pantry**: Navigate đến `/Home/Pantry`
2. **Click "Thêm Nguyên Liệu"**: Mở modal form
3. **Điền form**:
   - **Tên**: `Táo`
   - **Danh mục**: Chọn một category có sẵn (ví dụ: "Trái cây")
   - **Số lượng**: `5`
   - **Đơn vị**: Chọn "Quả"
   - **Ngày hết hạn**: `2025-12-31` (optional)
   - **Ghi chú**: `táo xanh úc` (optional)
4. **Click "Thêm Nguyên Liệu"**: Submit form

### Bước 4: Kiểm Tra Kết Quả

#### ✅ Success Case:
- Ingredient được thêm thành công
- Hiển thị thông báo "Đã thêm nguyên liệu mới"
- Ingredient xuất hiện trong danh sách
- Modal đóng lại
- Page refresh và hiển thị ingredient mới

#### ❌ Error Cases:

**Case 1: Validation Error (400)**
- Kiểm tra browser console: Không có error
- Kiểm tra terminal: `BadRequest` với message cụ thể
- Form hiển thị error message

**Case 2: API Error (500)**
- Kiểm tra browser console: `500 Internal Server Error`
- Kiểm tra terminal logs:
  - `📥 Received AddIngredient request:` - Request details
  - `📤 FormData fields:` - FormData được gửi
  - `📥 Response status:` - Status code từ API
  - `📥 Response body:` - Response body từ API
  - `❌ POST /ingredients failed:` - Error message
  - `❌ API Error message:` - Parsed error message

**Case 3: Unauthorized (401)**
- Kiểm tra terminal: `❌ Unauthorized: Token may be invalid or expired`
- Browser hiển thị: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."

### Bước 5: Kiểm Tra Terminal Logs

#### Expected Logs (Success):

```
🔄 POST /api/IngredientApi - Name: Táo, CategoryId: 690195402c834dc8e8d13517, Quantity: 5
📥 Received AddIngredient request:
   Name: 'Táo'
   CategoryId: '690195402c834dc8e8d13517'
   Quantity: 5
   Unit: 'quả'
   ExpireDate: '2025-12-31'
   Notes: 'táo xanh úc'
   Has Image: False
   Parsed ExpireDate: 2025-12-31
📤 Calling CookMate API to add ingredient...
🔑 Using token: eyJhbGciOiJIUzI1NiIs...
📤 POST https://cookm8.vercel.app/api/ingredients
📤 FormData fields:
  categoryId: [String]
  name: [String]
  quantity: [String]
  unit: [String]
  expireDate: [String]
  notes: [String]
📥 Response status: 200
📥 Response body: {"id":"...","name":"Táo",...}
✅ POST /ingredients succeeded: 200
✅ Added ingredient: Táo (ID: ...)
```

#### Expected Logs (Error):

```
📥 Received AddIngredient request:
   Name: 'Táo'
   CategoryId: '690195402c834dc8e8d13517'
   ...
📤 POST https://cookm8.vercel.app/api/ingredients
📤 FormData fields:
  ...
📥 Response status: 400
📥 Response body: {"message":"Invalid categoryId"}
❌ POST /ingredients failed: 400
❌ Response body: {"message":"Invalid categoryId"}
❌ API Error message: Invalid categoryId
⚠️ AddIngredientAsync returned null - API call failed
```

## 🔍 Debug Checklist

Nếu test fail, kiểm tra:

- [ ] **Token có tồn tại**: Kiểm tra `🔑 Using token:` trong logs
- [ ] **Request data đúng**: Kiểm tra `📥 Received AddIngredient request:` 
- [ ] **FormData format đúng**: Kiểm tra `📤 FormData fields:`
- [ ] **API response**: Kiểm tra `📥 Response status:` và `📥 Response body:`
- [ ] **Error message**: Kiểm tra `❌ API Error message:`

## 📝 Test Results

Sau khi test, ghi lại kết quả:

- [ ] Test thành công
- [ ] Test thất bại - Lý do: ___________
- [ ] Terminal logs: ___________
- [ ] Browser console logs: ___________

## 🐛 Known Issues

Nếu gặp các lỗi sau, tham khảo cách fix:

1. **"Category ID không được nhận diện"**
   - Đã sửa: Normalize category ID trong JavaScript và C#

2. **"ExpireDate format không đúng"**
   - Đã sửa: Format từ "o" sang "yyyy-MM-dd"

3. **"API trả về 500 nhưng không biết lý do"**
   - Đã sửa: Thêm logging chi tiết để xem error message từ API

## ✅ Next Steps

Sau khi test thành công:
1. Test các trường hợp edge cases (empty fields, invalid data)
2. Test update ingredient
3. Test delete ingredient
4. Test với image upload

