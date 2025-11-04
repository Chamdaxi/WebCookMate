# 🍽️ HƯỚNG DẪN TẠO DATA VỚI RECIPES CÓ HÌNH ẢNH

## 📧 Email: duyymanhh123@gmail.com

---

## 🎯 MỤC TIÊU

Script sẽ tự động tạo data cho 3 trang:
1. **Pantry** - Categories + Ingredients
2. **Favorites** - Recipes với hình ảnh
3. **Meal Plans** - Meal plans với recipes có hình ảnh

---

## 🔍 CÁCH LẤY RECIPES VỚI HÌNH ẢNH

### Function: `get_recipes_with_images()`

Script sẽ tự động:

1. **Search Recipes** với nhiều keywords:
   - `chicken`, `pasta`, `salad`, `soup`, `dessert`
   - `vietnamese`, `asian`, `beef`
   - Mỗi query tìm 2 recipes → Tối đa 10 recipes

2. **API Endpoints**:
   ```
   GET /api/recipes/search?query={keyword}&limit=2
   ```
   - Headers: `Authorization: Bearer {token}`
   - Response: Array of recipes với `id`, `title`, `image`

3. **Fallback**:
   - Nếu search không có kết quả → Dùng `/api/recipes/today`
   - Nếu vẫn không có → Dùng fallback IDs: `642264-642268`

4. **Validation**:
   - Chỉ lấy recipes có `image` (hình ảnh)
   - Loại bỏ duplicates
   - Trả về tối đa 10 recipe IDs

---

## 📊 DATA FLOW

```
┌─────────────────────────────────────────┐
│  1. Search Recipes (Multiple Queries)   │
│     → Get recipes with images           │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  2. Create Favorites                    │
│     → Use first 5 recipes with images   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  3. Create Meal Plans                   │
│     → Use same recipes from favorites   │
│     → Distribute across 3 meal plans    │
└─────────────────────────────────────────┘
```

---

## 🚀 CHẠY SCRIPT

```bash
cd /Users/phamtau/WebCookmate
python3 seed_data.py
```

### Output Example:

```
==================================================
  🔍 Searching recipes with images...
==================================================

   🔍 Searching recipes with images...
   ✅ Found 8 recipes with images
      ✅ Chicken Curry (ID: 642264)
      ✅ Pasta Carbonara (ID: 642265)
      ✅ Caesar Salad (ID: 642266)
      ✅ Vietnamese Pho (ID: 642267)
      ✅ Beef Steak (ID: 642268)

==================================================
  ⭐ Creating Favorites
==================================================

1. Adding recipe ID '642264' to favorites...
   ✅ Added
2. Adding recipe ID '642265' to favorites...
   ✅ Added
...

✅ Created 5 favorites!

==================================================
  📅 Creating Meal Plans
==================================================

1. Creating meal plan 'Thực đơn tuần này'...
   Recipes: 642264, 642265, 642266
   ✅ Created for 2025-10-30
2. Creating meal plan 'Bữa ăn cuối tuần'...
   Recipes: 642267, 642268
   ✅ Created for 2025-11-03
3. Creating meal plan 'Kế hoạch tập gym'...
   Recipes: 642264, 642266
   ✅ Created for 2025-11-01

✅ Created 3 meal plans!
```

---

## ✅ KẾT QUẢ SAU KHI TẠO

### 1. 📁 Pantry Page
- ✅ 7 Categories
- ✅ 12 Ingredients
- ✅ Có thể upload hình ảnh cho ingredients

### 2. ⭐ Favorites Page
- ✅ **5 Favorite Recipes** với hình ảnh
- ✅ Mỗi recipe hiển thị:
  - Tên món ăn
  - Hình ảnh
  - Thời gian nấu
  - Rating
  - Tags

### 3. 📅 Meal Plans Page
- ✅ **3 Meal Plans** với recipes có hình ảnh:
  - **Thực đơn tuần này** (ngày mai) - 3 recipes
  - **Bữa ăn cuối tuần** (+5 ngày) - 2 recipes
  - **Kế hoạch tập gym** (+3 ngày) - 2 recipes
- ✅ Calendar highlight ngày có meal plans
- ✅ Click ngày → Xem recipes với hình ảnh

---

## 🎨 UI HIỂN THỊ

### Favorites List:
```
┌─────────────────────────────────────┐
│  ⭐ [Hình ảnh] Chicken Curry        │
│     ⏱️ 45 min  ⭐ 4.5               │
├─────────────────────────────────────┤
│  ⭐ [Hình ảnh] Pasta Carbonara      │
│     ⏱️ 30 min  ⭐ 4.8               │
├─────────────────────────────────────┤
│  ⭐ [Hình ảnh] Caesar Salad         │
│     ⏱️ 15 min  ⭐ 4.2               │
└─────────────────────────────────────┘
```

### Meal Plans:
```
Calendar:
  29  30  31   1   2   3   4
  📅  📅       📅
  
Detail View:
  📅 Thực đơn tuần này (30 Oct)
  
  Breakfast:
    🍳 [Hình ảnh] Chicken Curry
  Lunch:
    🍝 [Hình ảnh] Pasta Carbonara
  Dinner:
    🥗 [Hình ảnh] Caesar Salad
```

---

## 🔧 TROUBLESHOOTING

### Issue 1: Không tìm thấy recipes
**Nguyên nhân**: API `/recipes/search` không có kết quả hoặc không có hình ảnh

**Giải pháp**:
- Script tự động fallback sang `/recipes/today`
- Nếu vẫn không có → Dùng fallback IDs
- Có thể cần update keywords trong `get_recipes_with_images()`

### Issue 2: Recipes không có hình ảnh
**Nguyên nhân**: API trả về recipes nhưng không có field `image`

**Giải pháp**:
- Script hiển thị status khi search (✅ có image, ❌ không có)
- Chỉ add recipes có `image` vào favorites/meal plans
- Nếu không có image → Vẫn add nhưng sẽ không hiển thị hình

### Issue 3: Meal Plans không hiển thị recipes
**Nguyên nhân**: Recipe IDs không hợp lệ hoặc API không trả về recipe details

**Giải pháp**:
- Verify meal plans đã được tạo (check API response)
- Verify recipe IDs trong meal plan match với favorites
- Check browser console để xem có errors không

---

## 📝 RECIPE API STRUCTURE

### Search Response:
```json
[
  {
    "id": 642264,
    "title": "Chicken Curry",
    "image": "https://spoonacular.com/recipeImages/642264-556x370.jpg",
    "readyInMinutes": 45,
    "servings": 4,
    "summary": "Delicious chicken curry..."
  }
]
```

### Used in:
- **Favorites**: `recipeId` field = recipe `id`
- **Meal Plans**: `recipeIds` array = array of recipe `id`s

---

## ✅ CHECKLIST

Sau khi chạy script:

- [ ] Pantry: 7 categories + 12 ingredients visible
- [ ] Favorites: 5 recipes visible với hình ảnh
- [ ] Meal Plans: 3 meal plans visible trên calendar
- [ ] Meal Plans: Click ngày → Recipes hiển thị với hình ảnh
- [ ] All images load correctly (không broken)
- [ ] Recipes clickable → Show details (nếu có)

---

**Date**: 2025-10-29  
**Email**: duyymanhh123@gmail.com  
**Status**: ✅ READY - RECIPES WITH IMAGES

