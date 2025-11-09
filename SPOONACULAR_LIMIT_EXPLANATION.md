# Giải thích về Giới hạn Spoonacular API

## Vấn đề

API server CookMate (`https://cookm8.vercel.app`) sử dụng **Spoonacular API** để lấy thông tin chi tiết về recipes (tên, ảnh, mô tả, thời gian nấu, v.v.).

### Giới hạn Spoonacular API:
- **Plan miễn phí**: 50 points mỗi ngày
- **Reset**: Sau 24 giờ (tính từ lần đầu sử dụng trong ngày)
- **Khi đạt giới hạn**: API trả về lỗi 402 (Payment Required) với message "Your daily points limit of 50 has been reached"

## Tại sao bị giới hạn?

1. **Spoonacular API** là dịch vụ trả phí, có giới hạn cho plan miễn phí
2. Mỗi lần lấy thông tin recipe chi tiết tốn **1-2 points**
3. Khi load favorites, API server cần fetch thông tin chi tiết cho **tất cả favorites** → tốn nhiều points
4. Nếu có nhiều favorites (ví dụ: 15 favorites) → tốn ~15-30 points
5. Sau vài lần load → đạt giới hạn 50 points/ngày

## Giải pháp đã implement

### 1. **Cache Favorites trong localStorage**
- Khi load favorites thành công, lưu vào `localStorage`
- Khi gặp Spoonacular limit, hiển thị favorites từ cache
- Cache có hiệu lực **24 giờ**

### 2. **Hiển thị Favorites với thông tin cơ bản**
- Ngay cả khi không có chi tiết từ Spoonacular, vẫn hiển thị favorites với:
  - Recipe ID
  - Tên mặc định: "Recipe #[ID]"
  - Ảnh placeholder
  - Thông tin cơ bản khác

### 3. **Thông báo rõ ràng cho người dùng**
- Hiển thị notification khi đang dùng cache
- Giải thích về giới hạn API và thời gian reset (24 giờ)

## Cách khắc phục

### Ngắn hạn:
1. **Đợi 24 giờ** - Giới hạn sẽ reset tự động
2. **Sử dụng cache** - Favorites đã load trước đó sẽ được hiển thị từ cache
3. **Thêm favorites mới** - Vẫn có thể thêm favorites mới (chỉ lưu recipeId, không cần fetch chi tiết)

### Dài hạn:
1. **Upgrade Spoonacular plan** - Trả phí để có nhiều points hơn
2. **Tối ưu API calls** - Chỉ fetch chi tiết khi cần thiết (lazy loading)
3. **Sử dụng cache server-side** - Cache recipe details ở server để giảm API calls

## Lưu ý

- **Favorites vẫn được lưu** trong database, chỉ là không thể hiển thị chi tiết khi Spoonacular limit
- **Có thể thêm favorites mới** bất cứ lúc nào
- **Cache sẽ tự động update** khi load favorites thành công
- **Giới hạn reset sau 24 giờ** (tính từ lần đầu sử dụng trong ngày)

## Code changes

Đã thêm các functions:
- `cacheFavorites()` - Lưu favorites vào localStorage
- `getCachedFavorites()` - Lấy favorites từ cache
- `processFavoritesArray()` - Xử lý và render favorites (có thể từ API hoặc cache)
- Auto-clear cache khi thêm/xóa favorites

