# Hướng Dẫn Test Chức Năng Pantry

## 📋 Dữ Liệu Test Mẫu

### 1. Test Thêm Category (Danh Mục)

#### Dữ liệu test mẫu:
- **Tên danh mục**: `Trái cây`
- **Icon**: `🍎` hoặc `fa-apple`

- **Tên danh mục**: `Rau củ`
- **Icon**: `🥕` hoặc `fa-carrot`

- **Tên danh mục**: `Thịt`
- **Icon**: `🥩` hoặc `fa-drumstick-bite`

- **Tên danh mục**: `Sữa & Trứng`
- **Icon**: `🥛` hoặc `fa-mug-hot`

- **Tên danh mục**: `Gia vị`
- **Icon**: `🧂` hoặc `fa-pepper-hot`

- **Tên danh mục**: `Hải sản`
- **Icon**: `🦐` hoặc `fa-fish`

- **Tên danh mục**: `Ngũ cốc`
- **Icon**: `🌾` hoặc `fa-wheat-awn`

#### Cách test:
1. Mở trang Pantry
2. Click nút "Quản Lý Danh Mục"
3. Điền tên danh mục và icon
4. Click "Thêm Danh Mục"
5. Kiểm tra xem danh mục có xuất hiện trong danh sách không

---

### 2. Test Thêm Ingredient (Nguyên Liệu)

#### Dữ liệu test mẫu:

**Nguyên liệu 1: Táo**
- **Tên**: `Táo`
- **Danh mục**: Chọn "Trái cây" (phải tạo category trước)
- **Số lượng**: `5`
- **Đơn vị**: `quả`
- **Ngày hết hạn**: `2025-12-31` (tùy chọn)
- **Ghi chú**: `Táo xanh Úc` (tùy chọn)

**Nguyên liệu 2: Thịt heo**
- **Tên**: `Thịt heo`
- **Danh mục**: Chọn "Thịt"
- **Số lượng**: `1`
- **Đơn vị**: `kg`
- **Ngày hết hạn**: `2025-01-15`
- **Ghi chú**: `Thịt ba chỉ` (tùy chọn)

**Nguyên liệu 3: Cà rốt**
- **Tên**: `Cà rốt`
- **Danh mục**: Chọn "Rau củ"
- **Số lượng**: `500`
- **Đơn vị**: `g`
- **Ngày hết hạn**: `2025-01-10`
- **Ghi chú**: `Cà rốt tươi` (tùy chọn)

**Nguyên liệu 4: Sữa tươi**
- **Tên**: `Sữa tươi`
- **Danh mục**: Chọn "Sữa & Trứng"
- **Số lượng**: `1`
- **Đơn vị**: `lít`
- **Ngày hết hạn**: `2025-01-05`
- **Ghi chú**: `Sữa tươi không đường` (tùy chọn)

**Nguyên liệu 5: Trứng gà**
- **Tên**: `Trứng gà`
- **Danh mục**: Chọn "Sữa & Trứng"
- **Số lượng**: `10`
- **Đơn vị**: `quả`
- **Ngày hết hạn**: `2025-01-08`
- **Ghi chú**: `Trứng gà ta` (tùy chọn)

#### Cách test:
1. Mở trang Pantry
2. Click nút "Thêm Nguyên Liệu"
3. Điền đầy đủ các trường bắt buộc (*):
   - Tên Nguyên Liệu
   - Danh Mục (phải chọn một category)
   - Số Lượng
   - Đơn Vị
4. Điền các trường tùy chọn:
   - Ngày Hết Hạn
   - Ghi Chú
5. Click "Thêm Nguyên Liệu"
6. Kiểm tra xem nguyên liệu có xuất hiện trong danh sách không

---

### 3. Test Sửa Ingredient

#### Cách test:
1. Tìm một nguyên liệu trong danh sách
2. Click nút "Sửa" (icon bút chì)
3. Thay đổi thông tin (ví dụ: số lượng, ngày hết hạn)
4. Click "Cập Nhật"
5. Kiểm tra xem thông tin có được cập nhật không

---

### 4. Test Xóa Ingredient

#### Cách test:
1. Tìm một nguyên liệu trong danh sách
2. Click nút "Xóa" (icon thùng rác)
3. Xác nhận xóa
4. Kiểm tra xem nguyên liệu có bị xóa khỏi danh sách không

---

### 5. Test Sửa Category

#### Cách test:
1. Mở modal "Quản Lý Danh Mục"
2. Tìm một category trong danh sách
3. Click nút "Sửa" (icon bút chì)
4. Thay đổi tên hoặc icon
5. Click "Cập Nhật"
6. Kiểm tra xem category có được cập nhật không

---

### 6. Test Xóa Category

#### Cách test:
1. Mở modal "Quản Lý Danh Mục"
2. Tìm một category trong danh sách
3. Click nút "Xóa" (icon thùng rác)
4. Xác nhận xóa
5. Kiểm tra xem category có bị xóa khỏi danh sách không

---

## ⚠️ Lưu Ý Khi Test

1. **Phải tạo Category trước khi thêm Ingredient**: 
   - Ingredient cần một category để thuộc về
   - Nếu chưa có category, tạo category trước

2. **Các trường bắt buộc**:
   - Tên Nguyên Liệu
   - Danh Mục
   - Số Lượng
   - Đơn Vị

3. **Format Icon**:
   - Có thể dùng emoji: `🍎`, `🥕`, `🥩`
   - Hoặc Font Awesome icon class: `fa-apple`, `fa-carrot`, `fa-drumstick-bite`

4. **Format Ngày Hết Hạn**:
   - Sử dụng date picker trong form
   - Format: `YYYY-MM-DD`

5. **Lỗi thường gặp**:
   - Nếu thêm category bị lỗi "Missing required fields": Kiểm tra xem đã điền đầy đủ `name` và `icon` chưa
   - Nếu thêm ingredient bị lỗi: Kiểm tra xem đã chọn category chưa và điền đầy đủ các trường bắt buộc

---

## 🔍 Kiểm Tra Logs

Nếu có lỗi, kiểm tra terminal logs để xem:
- Request body gửi đi
- Response từ API
- Error message chi tiết

Logs sẽ hiển thị:
- `📤 POST ... - Body: {...}` - Request gửi đi
- `✅ POST ... succeeded` - Thành công
- `❌ POST ... failed` - Thất bại với error message

---

## ✅ Checklist Test

- [ ] Thêm category thành công
- [ ] Sửa category thành công
- [ ] Xóa category thành công
- [ ] Thêm ingredient thành công
- [ ] Sửa ingredient thành công
- [ ] Xóa ingredient thành công
- [ ] Hiển thị danh sách ingredients đúng
- [ ] Filter và search hoạt động đúng
- [ ] Sort hoạt động đúng
- [ ] Hiển thị thông báo thành công/lỗi đúng

---

## 🚀 Sau Khi Test Xong

Nếu tất cả các test case đều pass, chức năng Pantry đã hoạt động đúng!

Nếu có lỗi, ghi lại:
- Test case nào bị lỗi
- Error message
- Steps để reproduce
- Screenshot (nếu có)

