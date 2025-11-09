# Fix: API Unit Enum Values

## Vấn Đề

API server chỉ chấp nhận unit values bằng **tiếng Anh**:
- ✅ Valid: `kg`, `g`, `l`, `ml`, `piece`
- ❌ Invalid: `quả`, `cái`, `củ`, `trái`, `lít` (tiếng Việt)

Error từ API:
```json
{"error":"ingredient validation failed: unit: `quả` is not a valid enum value for path `unit`."}
```

## Giải Pháp

### 1. Form HTML
Đổi tất cả unit options sang tiếng Anh:
```html
<select id="ingredientUnit">
    <option value="kg">Kilogram (kg)</option>
    <option value="g">Gram (g)</option>
    <option value="l">Lít (l)</option>
    <option value="ml">Mililít (ml)</option>
    <option value="piece" selected>Cái/Quả/Trái/Củ/Bó/Gói/Ổ/Lon/Chai/Hộp (piece)</option>
</select>
```

### 2. JavaScript Unit Mapping
Thêm unit mapping trong `handleIngredientSubmit()`:
```javascript
// API chỉ chấp nhận unit bằng tiếng Anh: kg, g, l, ml, piece
const unitMap = {
    'kg': 'kg',
    'g': 'g',
    'lít': 'l',
    'l': 'l',
    'ml': 'ml',
    'cái': 'piece',
    'củ': 'piece',
    'quả': 'piece',
    'trái': 'piece',
    'bó': 'piece',
    'gói': 'piece',
    'ổ': 'piece',
    'lon': 'piece',
    'chai': 'piece',
    'hộp': 'piece',
    'piece': 'piece'
};
const mappedUnit = unitMap[unit.toLowerCase()] || 'piece';
formData.append('unit', mappedUnit);
```

### 3. Display Unit
Khi hiển thị, giữ nguyên unit từ API (tiếng Anh).

## API Valid Unit Values

Theo API documentation và Postman collection:
- `kg` - Kilogram
- `g` - Gram  
- `l` - Liter (NOT `lít`)
- `ml` - Milliliter
- `piece` - Piece (for items like fruits, packages, etc.)

## Test

1. Form chỉ có options với value tiếng Anh
2. JavaScript maps Vietnamese units → English units
3. API nhận được unit values hợp lệ
4. Không còn lỗi validation

## Status

✅ Form đã được cập nhật
✅ JavaScript unit mapping đã được thêm
✅ Web đã restart với code mới

