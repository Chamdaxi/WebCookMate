# Hướng dẫn cấu hình Email cho CookMate

## Cấu hình Email SMTP

Để sử dụng chức năng khôi phục tài khoản qua email, bạn cần cấu hình thông tin SMTP trong file `appsettings.Development.json`:

### 1. Cấu hình Gmail (Khuyến nghị)

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "SenderEmail": "your_email@gmail.com",
    "SenderName": "CookMate",
    "Username": "your_email@gmail.com",
    "Password": "your_app_password"
  }
}
```

**Lưu ý quan trọng:**
- Thay `your_email@gmail.com` bằng email Gmail của bạn
- Thay `your_app_password` bằng App Password của Gmail (không phải mật khẩu thường)
- Để tạo App Password: Google Account → Security → 2-Step Verification → App passwords

### 2. Cấu hình Outlook/Hotmail

```json
{
  "Email": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "SenderEmail": "your_email@outlook.com",
    "SenderName": "CookMate",
    "Username": "your_email@outlook.com",
    "Password": "your_password"
  }
}
```

### 3. Cấu hình Yahoo Mail

```json
{
  "Email": {
    "SmtpServer": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "SenderEmail": "your_email@yahoo.com",
    "SenderName": "CookMate",
    "Username": "your_email@yahoo.com",
    "Password": "your_app_password"
  }
}
```

## Cách sử dụng chức năng khôi phục tài khoản

### Quy trình khôi phục:

1. **Truy cập trang đăng nhập** (`/Auth/Login`)
2. **Click "Quên mật khẩu?"** trong modal đăng nhập email
3. **Nhập email** và click "Gửi mã OTP"
4. **Kiểm tra email** để nhận mã OTP 6 chữ số
5. **Nhập mã OTP** vào trang xác thực
6. **Đặt mật khẩu mới** sau khi xác thực thành công

### Tính năng bảo mật:

- ✅ Mã OTP có thời hạn 10 phút
- ✅ Mã OTP chỉ sử dụng được 1 lần
- ✅ Kiểm tra độ mạnh mật khẩu mới
- ✅ Xác nhận mật khẩu để tránh nhập sai
- ✅ Session tự động hết hạn sau khi hoàn thành

### Xử lý lỗi thường gặp:

1. **"Không gửi được email"**
   - Kiểm tra cấu hình SMTP
   - Đảm bảo App Password đúng (với Gmail)
   - Kiểm tra kết nối internet

2. **"Mã OTP đã hết hạn"**
   - Yêu cầu gửi lại mã OTP mới
   - Kiểm tra thời gian hệ thống

3. **"Mã OTP không đúng"**
   - Kiểm tra lại mã từ email
   - Đảm bảo nhập đúng 6 chữ số

## Testing

Để test chức năng:

1. Chạy ứng dụng: `dotnet run`
2. Truy cập: `http://localhost:5000/Auth/Login`
3. Click "Login with Email"
4. Click "Quên mật khẩu?"
5. Nhập email test và kiểm tra hộp thư

## Lưu ý bảo mật

- Không commit file `appsettings.Development.json` có chứa mật khẩu thật
- Sử dụng biến môi trường cho production
- Cân nhắc sử dụng dịch vụ email chuyên nghiệp như SendGrid, Mailgun
