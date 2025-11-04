# 🌱 HƯỚNG DẪN TẠO DATA CHO 3 TRANG

## 📧 Email: duyymanhh123@gmail.com

---

## 🚀 CÁCH NHANH NHẤT: Script Python

### Bước 1: Cài đặt dependencies
```bash
pip3 install requests
```

### Bước 2: Chạy script
```bash
cd /Users/phamtau/WebCookmate
python3 seed_data.py
```

### Bước 3: Nhập OTP
Script sẽ:
1. Gửi OTP đến email `duyymanhh123@gmail.com`
2. Yêu cầu bạn nhập OTP code (6 chữ số)
3. Tự động tạo **TẤT CẢ DATA** cho 3 trang

---

## 📊 DATA SẼ ĐƯỢC TẠO

### 1. 📁 PANTRY (Tủ lạnh)

#### Categories (7):
| Icon | Name |
|------|------|
| 🍎 | Trái cây |
| 🥕 | Rau củ |
| 🥩 | Thịt |
| 🥛 | Sữa & Trứng |
| 🧂 | Gia vị |
| 🦐 | Hải sản |
| 🌾 | Ngũ cốc |

#### Ingredients (12):
| Name | Category | Quantity | Unit | Expire Date | Notes |
|------|----------|----------|------|-------------|-------|
| Táo Fuji | Trái cây | 5 | quả | +60 days | Táo Fuji nhập khẩu, ngọt |
| Chuối | Trái cây | 10 | quả | +7 days | Chuối già Việt Nam |
| Cà rốt | Rau củ | 500 | gram | +14 days | Cà rốt Đà Lạt tươi |
| Khoai tây | Rau củ | 1 | kg | +30 days | Khoai tây sạch |
| Thịt gà | Thịt | 500 | gram | +3 days | Gà ta tươi sạch |
| Thịt heo | Thịt | 300 | gram | +2 days | Thịt nạc vai |
| Sữa tươi | Sữa & Trứng | 2 | hộp | +10 days | Vinamilk 1L không đường |
| Trứng gà | Sữa & Trứng | 12 | quả | +20 days | Trứng gà sạch CP |
| Muối | Gia vị | 500 | gram | +365 days | Muối biển Việt Nam |
| Đường trắng | Gia vị | 1 | kg | +365 days | Đường tinh luyện |
| Tôm sú | Hải sản | 200 | gram | +1 day | Tôm sú tươi sống |
| Gạo tẻ | Ngũ cốc | 5 | kg | +180 days | Gạo ST25 cao cấp |

---

### 2. ⭐ FAVORITES (Yêu thích)

**5 Favorite Recipes**:
- Script tự động:
  1. Lấy recipe IDs từ API (`/api/recipes/today`)
  2. Nếu không có → Dùng demo IDs: `642264`, `642265`, `642266`, `642267`, `642268`
  3. Add vào favorites

**Sau khi tạo**:
- Vào **FavoriteList** page → Thấy 5 recipes yêu thích
- Có thể xóa, thêm favorites mới

---

### 3. 📅 MEAL PLANS (Kế hoạch bữa ăn)

**3 Meal Plans**:

| Tên | Ngày | Recipe IDs | Notes |
|-----|------|------------|-------|
| Thực đơn tuần này | Ngày mai | 642264, 642265, 642266 | Kế hoạch ăn uống lành mạnh |
| Bữa ăn cuối tuần | +5 ngày | 642267, 642268 | Thưởng thức món ngon cuối tuần |
| Kế hoạch tập gym | +3 ngày | 642264, 642266 | Thực đơn giàu protein |

**Sau khi tạo**:
- Vào **MealPlan** page
- Click vào ngày có meal plan (highlight màu tím + icon 📅)
- Xem chi tiết meal plan
- Có thể sửa, xóa meal plans

---

## 🧪 TEST SAU KHI TẠO DATA

### 1. Pantry Page
```
http://localhost:5134/Home/Pantry
```
**Expected**:
- ✅ 7 categories hiển thị (tabs và list)
- ✅ 12 ingredients hiển thị
- ✅ Có thể filter, search, sort
- ✅ Có thể CRUD categories và ingredients

### 2. FavoriteList Page
```
http://localhost:5134/Home/FavoriteList
```
**Expected**:
- ✅ 5 favorite recipes hiển thị
- ✅ Có thể xóa favorites
- ✅ Search và filter hoạt động

### 3. Meal Plan Page
```
http://localhost:5134/Home/MealPlan
```
**Expected**:
- ✅ Calendar hiển thị
- ✅ Ngày có meal plan → Highlight màu tím + icon 📅
- ✅ Click ngày → Xem meal plan details
- ✅ Có thể sửa, xóa meal plans

---

## 📋 SCRIPT OUTPUT EXAMPLE

```
==================================================
  🌱 SEED DATA FOR COOKMATE USER
==================================================

Email: duyymanhh123@gmail.com
API: https://cookm8.vercel.app

📧 Sending OTP to duyymanhh123@gmail.com...
✅ OTP sent successfully!

📧 Check your email and enter the OTP code:
OTP (6 digits): 123456

🔐 Verifying OTP...
✅ OTP verified! Got token.

==================================================
  📁 Creating Categories
==================================================

1. Creating 'Trái cây'...
   ✅ Created: cat_001
...

✅ Created 7 categories!

==================================================
  🥗 Creating Ingredients
==================================================

1. Creating 'Táo Fuji'...
   ✅ Created
...

✅ Created 12 ingredients!

==================================================
  ⭐ Creating Favorites
==================================================

   📋 Found 5 recipes from API
1. Adding recipe '642264' to favorites...
   ✅ Added
...

✅ Created 5 favorites!

==================================================
  📅 Creating Meal Plans
==================================================

1. Creating meal plan 'Thực đơn tuần này'...
   ✅ Created for 2025-10-30
2. Creating meal plan 'Bữa ăn cuối tuần'...
   ✅ Created for 2025-11-03
3. Creating meal plan 'Kế hoạch tập gym'...
   ✅ Created for 2025-11-01

✅ Created 3 meal plans!

==================================================
  ✅ VERIFICATION
==================================================

📁 Categories:
   ✅ Total: 7
   - 🍎 Trái cây
   - 🥕 Rau củ
   ...

🥗 Ingredients:
   ✅ Total: 12
   - Táo Fuji: 5 quả
   - Chuối: 10 quả
   ...

⭐ Favorites:
   ✅ Total: 5
   - Recipe ID: 642264
   ...

📅 Meal Plans:
   ✅ Total: 3
   - Thực đơn tuần này (2025-10-30)
   ...

==================================================
  🎉 DONE!
==================================================
```

---

## 🔧 TROUBLESHOOTING

### Issue 1: OTP không nhận được
**Giải pháp**:
- Check spam folder
- Đợi 1-2 phút
- Thử gửi lại bằng cách chạy script lại

### Issue 2: Recipe IDs không hợp lệ
**Nguyên nhân**: CookMate API không có recipes hoặc IDs không đúng

**Giải pháp**:
- Script sẽ dùng demo IDs
- Nếu vẫn fail → Có thể API chưa có recipes
- Favorites và Meal Plans vẫn có thể tạo, nhưng recipe IDs cần đúng

### Issue 3: Meal plan date không match calendar
**Nguyên nhân**: Calendar hiển thị tháng hiện tại, meal plans có thể ở tháng khác

**Giải pháp**:
- Meal plans được tạo cho các ngày sắp tới (tomorrow, +3 days, +5 days)
- Check calendar tháng hiện tại và tháng sau
- Hoặc update script để tạo meal plans cho các ngày trong tháng hiện tại

---

## 📝 NOTES

- **Data persistence**: Tất cả data được lưu trên CookMate API Server
- **User-specific**: Data chỉ hiển thị cho user đã đăng nhập
- **Refresh**: Sau khi tạo data, refresh trang để thấy updates

---

## ✅ CHECKLIST

Sau khi chạy script, verify:

- [ ] Pantry: 7 categories visible
- [ ] Pantry: 12 ingredients visible
- [ ] Favorites: 5 favorites visible
- [ ] Meal Plans: Calendar có 3 ngày highlighted
- [ ] Meal Plans: Click ngày → Xem meal plan details
- [ ] All data persists sau khi refresh
- [ ] All data visible khi đăng nhập lại

---

**Date**: 2025-10-29  
**Email**: duyymanhh123@gmail.com  
**Status**: ✅ READY TO RUN

