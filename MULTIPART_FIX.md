# Multipart Form-Data Fix

## Vấn Đề

API server trả về lỗi: `{"error":"Failed to parse body as FormData."}` với status 500.

## Nguyên Nhân

Boundary format không đúng với Python requests và Postman:
- **Trước**: `----WebKitFormBoundary{Guid}` (có prefix `----WebKitFormBoundary`)
- **Sau**: `{Guid}` (32 hex characters, không có prefix)

## Giải Pháp

### 1. Boundary Format
Python requests tạo boundary đơn giản: 32 hex characters (từ Guid, không có dashes)
```csharp
// ❌ SAI (trước)
var boundary = $"----WebKitFormBoundary{Guid.NewGuid().ToString("N")[..24]}";

// ✅ ĐÚNG (sau)
var boundaryValue = Guid.NewGuid().ToString("N"); // 32 hex chars, no dashes
```

### 2. Multipart Body Format
Format đúng theo RFC 2046:
```
--boundary\r\n
Content-Disposition: form-data; name="key"\r\n
\r\n
value\r\n
--boundary\r\n
Content-Disposition: form-data; name="key2"; filename="file.jpg"\r\n
Content-Type: image/jpeg\r\n
\r\n
[file bytes]\r\n
--boundary--\r\n
```

### 3. Content-Type Header
```csharp
request.Content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundaryValue}");
```

## Test

1. Test Add Ingredient với boundary format mới
2. Kiểm tra logs để xem body preview và response
3. So sánh với Python requests format

## Kết Quả Mong Đợi

- Boundary: 32 hex characters (ví dụ: `f95dc406338aace50714be51634fa5f2`)
- Content-Type: `multipart/form-data; boundary={boundary}`
- Body format: Đúng theo RFC 2046
- API server có thể parse được multipart body

