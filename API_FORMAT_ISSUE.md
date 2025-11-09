# Vấn Đề: "Failed to parse body as FormData"

## Lỗi Hiện Tại
API CookMate (Node.js/Express) trả về: `{"error":"Failed to parse body as FormData."}` với status 500.

## Nguyên Nhân
C# `HttpClient` với `MultipartFormDataContent` tạo multipart/form-data format **KHÔNG TƯƠNG THÍCH** với Node.js multer/busboy parser.

## So Sánh

### Python requests (✅ Hoạt động)
```python
response = requests.post(
    url,
    files={},  # Empty dict
    data={
        'categoryId': '...',
        'name': '...',
        'quantity': '5',
        'unit': 'quả',
        'expireDate': '2025-09-23T14:06:15.378Z',
        'notes': '...'
    },
    headers={"Authorization": f"Bearer {token}"}
)
```

### C# HttpClient (❌ Không hoạt động)
```csharp
var formData = new MultipartFormDataContent();
formData.Add(new StringContent(categoryId), "categoryId");
formData.Add(new StringContent(name), "name");
// ... etc
var response = await client.PostAsync(url, formData);
```

## Vấn Đề Cụ Thể

1. **Boundary Format**: C# tạo boundary khác với Python requests
2. **Content-Disposition Headers**: Format có thể khác
3. **Encoding**: Có thể có vấn đề về encoding

## Giải Pháp Có Thể

### Option 1: Tạo Raw Multipart Body Manually
Tạo multipart/form-data body manually để match Python requests format.

### Option 2: Dùng Library Khác
- Flurl.Http
- RestSharp
- Refit

### Option 3: Kiểm Tra API Server
Có thể API server có bug hoặc config sai multer/busboy.

## Test với Postman
Postman collection **CŨNG DÙNG formdata mode** và có thể hoạt động. Vậy vấn đề là gì?

Có thể:
- Postman tạo format khác với C#
- API server có vấn đề với một số client

## Next Steps

1. Test với Postman để xác nhận API hoạt động
2. So sánh raw HTTP request từ Postman vs C#
3. Thử tạo raw multipart body manually
4. Hoặc dùng library khác (Flurl, RestSharp)

