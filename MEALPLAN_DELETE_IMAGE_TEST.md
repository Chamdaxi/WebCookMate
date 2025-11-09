# Kiểm Tra Tính Năng Xóa Meal Plan và Hiển Thị Ảnh Recipe

## 📋 Tóm Tắt Các Fix Đã Thực Hiện

### 1. ✅ Fix DELETE Meal Plan
- **Vấn đề**: API trả về 404 NotFound vì code gửi `DELETE /meal-plans/{id}` (URL path parameter)
- **Giải pháp**: Sửa `DeleteMealPlanAsync` trong `CookMateApiService.cs` để gửi `DELETE /meal-plans` với body JSON `{"mealPlanId": "..."}` theo đúng API documentation
- **File**: `Services/CookMateApiService.cs` - line 1241-1260
- **Code**:
  ```csharp
  return await DeleteAsync("/meal-plans", new { mealPlanId = mealPlanId });
  ```

### 2. ✅ Fix Recipe Images
- **Vấn đề**: 
  - Placeholder image từ `via.placeholder.com` không load được (network/DNS error)
  - `onerror` handler vẫn dùng external URL
- **Giải pháp**: 
  - Thay thế bằng data URI (SVG base64) - không cần network
  - Fix `onerror` handler để dùng data URI thay vì external URL
- **File**: `Views/Home/MealPlan.cshtml` - line 737, 757
- **Code**:
  ```javascript
  let imageUrl = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTUwIiBoZWlnaHQ9IjE1MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTUwIiBoZWlnaHQ9IjE1MCIgZmlsbD0iI2YwZjBmMCIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iMTQiIGZpbGw9IiM5OTk5OTkiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj5ObyBJbWFnZTwvdGV4dD48L3N2Zz4=';
  if (recipeImage) {
      imageUrl = recipeImage.startsWith('http') ? recipeImage : `https://${recipeImage}`;
  }
  ```

### 3. ✅ Cải Thiện Recipe IDs Parsing
- **Vấn đề**: Recipe IDs có thể là string format (ví dụ: "recipe_stir_fry_001") hoặc số
- **Giải pháp**: 
  - Extract numeric part từ string IDs
  - Thêm logging chi tiết để debug
- **File**: 
  - `Views/Home/MealPlan.cshtml` - line 662-673 (client-side)
  - `Services/CookMateApiService.cs` - line 932-988 (server-side)

## 🧪 Hướng Dẫn Test

### Test 1: Xóa Meal Plan

1. **Mở trang Meal Plan**: http://localhost:5134/Home/MealPlan
2. **Chọn một meal plan** từ calendar (ngày có meal plan sẽ được highlight)
3. **Bấm nút "Xóa kế hoạch"** (màu đỏ)
4. **Xác nhận xóa** trong dialog
5. **Kiểm tra Console (F12)**:
   - `🗑️ Deleting meal plan: { id: "...", name: "..." }`
   - `🗑️ Delete URL: /api/MealPlansApi/...`
   - `🗑️ Delete response status: 200`
   - `✅ Deleted meal plan via CookMate API`
6. **Kiểm tra UI**:
   - Meal plan bị xóa khỏi display
   - Calendar cập nhật (không còn mark trên ngày đó)
   - Hiển thị empty state: "Chưa có kế hoạch bữa ăn cho ngày này"
7. **Kiểm tra Server Logs** (`/tmp/webcookmate_mealplan_fix.log`):
   - `📤 DELETE /meal-plans - Body: {"mealPlanId":"..."}`
   - `✅ DELETE /meal-plans succeeded: 200`

### Test 2: Hiển Thị Recipe Images

1. **Mở trang Meal Plan**: http://localhost:5134/Home/MealPlan
2. **Chọn một meal plan có recipes** (ngày có meal plan)
3. **Kiểm tra hiển thị**:
   - Recipe images hiển thị (nếu có từ API)
   - Nếu không có ảnh, hiển thị placeholder "No Image" (data URI - không cần network)
   - Không còn lỗi `net::ERR_NAME_NOT_RESOLVED` trong Console
4. **Kiểm tra Console (F12)**:
   - `🔄 Loading recipe details for IDs: [...]`
   - `🔄 Recipe IDs type: ... Sample: ...`
   - `🔄 Cleaned recipe IDs: [...]`
   - `✅ Loaded recipe details: [...]` (array of recipes)
   - Không có lỗi load image
5. **Kiểm tra Server Logs**:
   - `🔄 GetRecipeDetailsByStringIdsAsync: Received X recipe IDs`
   - `✅ Parsed '...' -> ...`
   - `📤 GET /recipes/bulk - Parsed RecipeIds: [...]`
   - `✅ Received X recipe details`

### Test 3: Recipe IDs Parsing

1. **Mở Console (F12)**
2. **Chọn một meal plan**
3. **Kiểm tra logs**:
   - `🔄 Recipe IDs type: string Sample: 645872` (hoặc format khác)
   - `🔄 Cleaned recipe IDs: ["645872", ...]`
   - `✅ Parsed '645872' -> 645872` (server-side)
   - `✅ Received X recipe details`

## 🔍 Expected Results

### Delete Meal Plan
- ✅ DELETE request thành công (200 OK)
- ✅ Meal plan bị xóa khỏi database
- ✅ UI cập nhật ngay lập tức
- ✅ Calendar cập nhật (không còn mark)
- ✅ Không có lỗi trong Console

### Recipe Images
- ✅ Recipe images hiển thị (nếu có từ API)
- ✅ Placeholder "No Image" hiển thị nếu không có ảnh (data URI - không cần network)
- ✅ Không còn lỗi `net::ERR_NAME_NOT_RESOLVED`
- ✅ Recipe details được load thành công
- ✅ Recipe titles, time, servings hiển thị đúng

## 📝 Notes

1. **Delete Meal Plan**: API endpoint đúng là `DELETE /meal-plans` với body JSON, không phải URL path parameter
2. **Recipe Images**: Sử dụng data URI để tránh network issues, fallback tự động nếu API không có ảnh
3. **Recipe IDs**: Hỗ trợ cả string và numeric IDs, tự động extract numeric part nếu cần
4. **Logging**: Thêm logging chi tiết để dễ debug

## 🐛 Troubleshooting

### Delete không hoạt động:
- Kiểm tra Console (F12) xem có lỗi gì không
- Kiểm tra Server Logs xem API call có thành công không
- Đảm bảo meal plan ID đúng format

### Images không hiển thị:
- Kiểm tra Console (F12) xem recipe details có được load không
- Kiểm tra Server Logs xem recipe IDs có được parse đúng không
- Đảm bảo placeholder (data URI) hiển thị nếu không có ảnh từ API

