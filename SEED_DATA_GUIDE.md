# 🌱 HƯỚNG DẪN TẠO DATA CHO USER

## 📧 Email: duyymanhh123@gmail.com

---

## 🚀 CÁCH NHANH NHẤT: Qua Web UI

### Bước 1: Đăng nhập
```
1. Mở: http://localhost:5134
2. Click "Đăng nhập bằng OTP (Email)"
3. Nhập email: duyymanhh123@gmail.com
4. Check email → Nhập OTP
5. ✅ Đăng nhập thành công!
```

### Bước 2: Tạo Categories
```
1. Vào: Pantry
2. Click "Quản lý danh mục"
3. Thêm các categories:
   - 🍎 Trái cây
   - 🥕 Rau củ
   - 🥩 Thịt
   - 🥛 Sữa & Trứng
   - 🧂 Gia vị
   - 🦐 Hải sản
   - 🌾 Ngũ cốc
```

### Bước 3: Tạo Ingredients
```
1. Ở trang Pantry
2. Click "Thêm nguyên liệu"
3. Điền thông tin:
   - Danh mục: Chọn từ dropdown
   - Tên: VD: "Táo Fuji"
   - Số lượng: 5
   - Đơn vị: quả
   - Hạn sử dụng: 31/12/2025
   - Ghi chú: "Táo nhập khẩu"
4. Save
5. Lặp lại với các ingredients khác
```

### Bước 4: Tạo Favorites
```
1. Vào: Recipes (khi có)
2. Click ❤️ trên recipe thích
3. Vào: FavoriteList → Xem danh sách
```

---

## 🐍 CÁCH TỰ ĐỘNG: Script Python

### Yêu cầu:
- Python 3.6+
- Thư viện `requests`

### Cài đặt dependencies:
```bash
pip3 install requests
```

### Chạy script:
```bash
cd /Users/phamtau/WebCookmate
python3 seed_data.py
```

### Script sẽ:
1. Gửi OTP đến email `duyymanhh123@gmail.com`
2. Yêu cầu bạn nhập OTP từ email
3. Tự động tạo:
   - ✅ 7 Categories
   - ✅ 12 Ingredients với đầy đủ thông tin
   - ✅ 3 Favorites (demo)

### Ví dụ output:
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
2. Creating 'Rau củ'...
   ✅ Created: cat_002
...

✅ Created 7 categories!

==================================================
  🥗 Creating Ingredients
==================================================

1. Creating 'Táo Fuji'...
   ✅ Created
2. Creating 'Chuối'...
   ✅ Created
...

✅ Created 12 ingredients!

==================================================
  ✅ VERIFICATION
==================================================

📁 Categories:
   ✅ Total: 7
   - 🍎 Trái cây
   - 🥕 Rau củ
   - 🥩 Thịt
   - 🥛 Sữa & Trứng
   - 🧂 Gia vị

🥗 Ingredients:
   ✅ Total: 12
   - Táo Fuji: 5 quả
   - Chuối: 10 quả
   - Cà rốt: 500 gram
   - Khoai tây: 1 kg
   - Thịt gà: 500 gram

⭐ Favorites:
   ✅ Total: 3

==================================================
  🎉 DONE!
==================================================
```

---

## 📋 SAMPLE DATA ĐƯỢC TẠO

### Categories (7):
| Icon | Name | 
|------|------|
| 🍎 | Trái cây |
| 🥕 | Rau củ |
| 🥩 | Thịt |
| 🥛 | Sữa & Trứng |
| 🧂 | Gia vị |
| 🦐 | Hải sản |
| 🌾 | Ngũ cốc |

### Ingredients (12):
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

### Favorites (3):
- Recipe: Chicken Curry
- Recipe: Phở Bò
- Recipe: Bánh Mì

---

## 🧪 VERIFY DATA

### Sau khi tạo, kiểm tra:

1. **Đăng nhập web**:
   ```
   http://localhost:5134
   Email: duyymanhh123@gmail.com
   ```

2. **Vào Pantry**:
   - ✅ Thấy 7 categories
   - ✅ Thấy 12 ingredients
   - ✅ Có thể filter, search, sort

3. **Vào Favorites**:
   - ✅ Thấy danh sách favorites
   - ✅ Có thể xóa, thêm mới

4. **Check Console logs**:
   ```
   F12 → Console:
   ✅ Loaded categories from CookMate API: 7
   ✅ Loaded ingredients from CookMate API: 12
   ```

5. **Check Network**:
   ```
   F12 → Network → XHR:
   ✅ GET /api/IngredientCategoryApi → 200 OK
   ✅ GET /api/IngredientApi → 200 OK
   ```

---

## 🔧 TROUBLESHOOTING

### Issue 1: OTP không nhận được
**Giải pháp**:
- Check spam folder
- Đợi 1-2 phút
- Thử gửi lại

### Issue 2: Script lỗi "Unauthorized"
**Nguyên nhân**: Token hết hạn hoặc OTP sai

**Giải pháp**:
- Chạy lại script
- Nhập đúng OTP code (6 chữ số)

### Issue 3: Không thấy data trên web
**Giải pháp**:
- Hard refresh: Cmd+Shift+R
- Clear cache
- Logout → Login lại

### Issue 4: Categories created nhưng ingredients failed
**Nguyên nhân**: Category IDs không match

**Giải pháp**:
- Kiểm tra category IDs từ API
- Update script với IDs đúng

---

## 📝 NOTES

- Data được lưu trên **CookMate API Server** (https://cookm8.vercel.app)
- **KHÔNG LƯU LOCAL** → Đăng nhập bất kỳ đâu đều thấy data
- Mỗi user có data riêng → Privacy
- Recipes là public → Tất cả users thấy

---

## 🎯 NEXT STEPS

Sau khi có data:

1. **Test CRUD operations**:
   - Edit ingredient
   - Delete category
   - Add new items

2. **Test Search/Filter**:
   - Search ingredients
   - Filter by category
   - Sort by name/date

3. **Test Favorites**:
   - Add recipes to favorites
   - View favorites list
   - Remove from favorites

4. **Test UI/UX**:
   - Responsive design
   - Loading states
   - Error handling

---

**Date**: 2025-10-29  
**Email**: duyymanhh123@gmail.com  
**Status**: ✅ READY TO SEED

