# CookMate API Documentation

API tương thích với chuẩn [CookMate API](https://cookm8.vercel.app/api-docs)

**Base URL:** `http://localhost:5134/api`

---

## 🔐 Hybrid Authentication

API này hỗ trợ **2 loại authentication**:

1. **Cookie-based** (cho Web UI) - Tự động tạo session sau khi login
2. **JWT Bearer Token** (cho API clients) - Trả về `access_token` trong response

**Khi gọi API từ Web UI:** Session cookie sẽ được tự động tạo, không cần thêm token vào header.

**Khi gọi API từ client khác:** Sử dụng JWT token trong header `Authorization: Bearer {token}`.

---

## Authentication Endpoints

### 1. Google OAuth Login
**Endpoint:** `POST /api/auth/google`

**Request Body:**
```json
{
  "idToken": "string"
}
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "user": {
    "id": "string",
    "email": "string",
    "name": "string",
    "avatar": ""
  }
}
```

---

### 2. Email/Password Login
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "user": {
    "id": "string",
    "email": "user@example.com",
    "name": "User Name",
    "avatar": ""
  }
}
```

---

### 3. Register New User
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "password123",
  "name": "New User"
}
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "user": {
    "id": "string",
    "email": "newuser@example.com",
    "name": "New User",
    "avatar": ""
  }
}
```

---

## User Profile Endpoints

### 4. Get User Profile
**Endpoint:** `GET /api/user/profile`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "id": "string",
  "email": "user@example.com",
  "name": "User Name",
  "avatar": "",
  "phone": "+1234567890",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

### 5. Update User Profile
**Endpoint:** `PUT /api/user/profile`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Request Body:**
```json
{
  "name": "Updated Name",
  "phone": "+9876543210",
  "avatar": ""
}
```

**Response:**
```json
{
  "id": "string",
  "email": "user@example.com",
  "name": "Updated Name",
  "avatar": "",
  "phone": "+9876543210"
}
```

---

## Ingredient (Pantry) Endpoints

### 6. Get All Ingredients
**Endpoint:** `GET /api/ingredients`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "data": [
    {
      "id": "string",
      "name": "Cà chua",
      "quantity": 2.5,
      "unit": "kg",
      "categoryId": "string",
      "category": {
        "id": "string",
        "name": "Rau củ",
        "icon": "🥕"
      },
      "expiryDate": "2024-12-31",
      "createdAt": "2024-01-01T00:00:00Z",
      "notes": "Organic",
      "userId": "string"
    }
  ],
  "count": 1
}
```

---

### 7. Add New Ingredient
**Endpoint:** `POST /api/ingredients`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Request Body:**
```json
{
  "name": "Cà chua",
  "quantity": 2.5,
  "unit": "kg",
  "categoryId": 1,
  "expiryDate": "2024-12-31",
  "purchaseDate": "2024-01-01",
  "notes": "Organic"
}
```

**Response:**
```json
{
  "id": "string",
  "name": "Cà chua",
  "quantity": 2.5,
  "unit": "kg",
  "categoryId": "1",
  "category": {
    "id": "1",
    "name": "Rau củ",
    "icon": "🥕"
  },
  "expiryDate": "2024-12-31",
  "createdAt": "2024-01-01T00:00:00Z",
  "notes": "Organic"
}
```

---

### 8. Update Ingredient
**Endpoint:** `PUT /api/ingredients/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Request Body:**
```json
{
  "name": "Cà chua bi",
  "quantity": 3.0,
  "unit": "kg",
  "categoryId": 1,
  "expiryDate": "2024-12-31",
  "notes": "Updated organic"
}
```

**Response:**
```json
{
  "id": "string",
  "name": "Cà chua bi",
  "quantity": 3.0,
  "unit": "kg",
  "categoryId": "1",
  "category": {
    "id": "1",
    "name": "Rau củ",
    "icon": "🥕"
  },
  "expiryDate": "2024-12-31",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "notes": "Updated organic"
}
```

---

### 9. Delete Ingredient
**Endpoint:** `DELETE /api/ingredients/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "message": "Ingredient deleted successfully"
}
```

---

### 10. Get Ingredient Categories
**Endpoint:** `GET /api/ingredients/categories`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "data": [
    {
      "id": "1",
      "name": "Rau củ",
      "icon": "🥕",
      "description": "Vegetables"
    }
  ],
  "count": 1
}
```

---

## Testing API với cURL

### Login và lấy token:
```bash
curl -X POST http://localhost:5134/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'
```

### Sử dụng token để gọi API:
```bash
curl -X GET http://localhost:5134/api/user/profile \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Lấy danh sách nguyên liệu:
```bash
curl -X GET http://localhost:5134/api/ingredients \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## JWT Configuration

JWT settings được cấu hình trong `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "CookMate_Super_Secret_Key_2024_MinLength32Chars!!!",
    "Issuer": "CookMate",
    "Audience": "CookMateUsers",
    "ExpirationMinutes": "1440"
  }
}
```

Token có hiệu lực **24 giờ** (1440 phút).

---

## Error Responses

Tất cả endpoint đều trả về error theo format:

```json
{
  "message": "Error description"
}
```

**HTTP Status Codes:**
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `404` - Not Found
- `500` - Internal Server Error

---

## Notes

- Tất cả API endpoints (trừ auth) yêu cầu **Bearer token** trong header
- Token được trả về sau khi login/register thành công
- Sử dụng token trong header: `Authorization: Bearer {token}`
- API tương thích với chuẩn CookMate API: https://cookm8.vercel.app/api-docs

