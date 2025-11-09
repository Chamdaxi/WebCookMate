# Fix: API Field Names và Values bằng Tiếng Anh

## Vấn Đề

API CookMate sử dụng **tiếng Anh** cho tất cả field names và values:
- Field names: `categoryId`, `name`, `quantity`, `unit`, `expireDate`, `notes`, `image`
- Unit values: `piece`, `kg`, `g`, `l`, `ml` (KHÔNG phải `quả`, `cái`, `lít`)

## Giải Pháp

### 1. Field Order (Thứ Tự Fields)

**Vấn đề**: Dictionary trong C# không đảm bảo thứ tự, nhưng API/multer có thể nhạy cảm với thứ tự fields.

**Fix**: Đổi từ `Dictionary<string, string>` sang `List<(string key, string value)>` để đảm bảo thứ tự đúng như Postman.

#### POST /ingredients (Add Ingredient)
```csharp
// Postman order: categoryId, name, quantity, unit, expireDate, notes, image
var fields = new List<(string key, string value)>
{
    ("categoryId", categoryId),
    ("name", name),
    ("quantity", quantity.ToString()),
    ("unit", unit) // API expects English: piece, kg, g, l, ml
};

if (!string.IsNullOrWhiteSpace(formattedExpireDate))
{
    fields.Add(("expireDate", formattedExpireDate));
}

if (!string.IsNullOrWhiteSpace(notes))
{
    fields.Add(("notes", notes));
}
```

#### PUT /ingredients (Update Ingredient)
```csharp
// Postman order: ingredientId, name, categoryId, quantity, unit, expireDate, notes, image
var fields = new List<(string key, string value)>
{
    ("ingredientId", ingredientId),
    ("name", name)
};

if (!string.IsNullOrWhiteSpace(categoryId))
{
    fields.Add(("categoryId", categoryId));
}

fields.Add(("quantity", quantity.ToString()));
fields.Add(("unit", unit)); // API expects English: piece, kg, g, l, ml

if (!string.IsNullOrWhiteSpace(formattedExpireDate))
{
    fields.Add(("expireDate", formattedExpireDate));
}

if (!string.IsNullOrWhiteSpace(notes))
{
    fields.Add(("notes", notes));
}
```

### 2. Unit Values (Giá Trị Đơn Vị)

**Vấn đề**: Form HTML sử dụng tiếng Việt (`quả`, `cái`, `lít`), nhưng API chỉ chấp nhận tiếng Anh.

**Fix**: 
1. **Form HTML**: Chỉ hiển thị options với giá trị tiếng Anh
2. **JavaScript**: Unit mapping từ tiếng Việt sang tiếng Anh (nếu cần)

```html
<select id="ingredientUnit">
    <option value="kg">Kilogram (kg)</option>
    <option value="g">Gram (g)</option>
    <option value="l">Lít (l)</option>
    <option value="ml">Mililít (ml)</option>
    <option value="piece" selected>Cái/Quả/Trái/Củ/Bó/Gói/Ổ/Lon/Chai/Hộp (piece)</option>
</select>
```

```javascript
// Unit mapping trong handleIngredientSubmit()
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

### 3. Multipart Form Data Format

**Vấn đề**: API trả về lỗi "Failed to parse body as FormData" khi format multipart không đúng.

**Fix**: 
- Sử dụng `CreateMultipartFormDataBody()` để tạo raw multipart body
- Đảm bảo field order đúng như Postman
- Boundary format: 32 hex characters (no dashes)
- Content-Type header: `multipart/form-data; boundary={boundaryValue}`

### 4. DELETE Endpoint

**Format**: JSON body với `ingredientId`
```json
{
    "ingredientId": "68f4854afbf73e07ae919bbb"
}
```

**Code**:
```csharp
public async Task<bool> DeleteIngredientAsync(string ingredientId)
{
    var success = await DeleteAsync("/ingredients", new { ingredientId = ingredientId });
    return success;
}
```

## API Valid Values

### Unit Enum Values
- `kg` - Kilogram
- `g` - Gram
- `l` - Liter (NOT `lít`)
- `ml` - Milliliter
- `piece` - Piece (for items like fruits, packages, etc.)

### Field Names
- `categoryId` - Category ID (MongoDB ObjectId)
- `name` - Ingredient name
- `quantity` - Quantity (number as string)
- `unit` - Unit (enum: kg, g, l, ml, piece)
- `expireDate` - Expiry date (ISO 8601: `2025-09-23T14:06:15.378Z`)
- `notes` - Notes (optional)
- `image` - Image file (optional)

## Testing

1. ✅ Field order đã được fix (List thay vì Dictionary)
2. ✅ Unit mapping đã được thêm vào JavaScript
3. ✅ Form HTML chỉ có options tiếng Anh
4. ✅ Multipart format đã được cập nhật
5. ✅ DELETE endpoint format đã đúng

## Status

✅ **Code đã được cập nhật**
✅ **Build thành công**
✅ **Web đã restart với code mới**

## Next Steps

1. Test Add Ingredient với unit `piece`
2. Test Update Ingredient
3. Test Delete Ingredient
4. Kiểm tra logs để verify multipart format

