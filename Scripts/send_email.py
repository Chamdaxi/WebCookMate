#!/usr/bin/env python3
import smtplib
import ssl
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
import sys

def send_otp_email(smtp_host, smtp_port, username, password, to_email, otp_code):
    """Send OTP email using Python's smtplib (works on macOS Sequoia)"""
    
    # Create message
    message = MIMEMultipart("alternative")
    message["Subject"] = "Mã OTP đăng nhập CookMate"
    message["From"] = f"CookMate <{username}>"
    message["To"] = to_email
    
    # HTML body
    html_body = f"""
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea, #764ba2); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-code {{ background: #667eea; color: white; font-size: 32px; font-weight: bold; padding: 20px; text-align: center; border-radius: 10px; margin: 20px 0; letter-spacing: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍳 CookMate</h1>
            <p>Mã OTP đăng nhập của bạn</p>
        </div>
        <div class='content'>
            <h2>Xin chào!</h2>
            <p>Bạn đã yêu cầu mã OTP để đăng nhập vào tài khoản CookMate.</p>
            <p>Mã OTP của bạn là:</p>
            <div class='otp-code'>{otp_code}</div>
            <p><strong>Lưu ý:</strong></p>
            <ul>
                <li>Mã OTP có hiệu lực trong 5 phút</li>
                <li>Không chia sẻ mã này với bất kỳ ai</li>
                <li>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này</li>
            </ul>
        </div>
        <div class='footer'>
            <p>© 2024 CookMate - Nấu ăn thông minh, cuộc sống dễ dàng</p>
        </div>
    </div>
</body>
</html>"""
    
    part = MIMEText(html_body, "html")
    message.attach(part)
    
    # Create secure SSL context
    context = ssl.create_default_context()
    
    try:
        # Connect and send
        with smtplib.SMTP(smtp_host, smtp_port) as server:
            server.starttls(context=context)
            server.login(username, password)
            server.sendmail(username, to_email, message.as_string())
        
        print(f"SUCCESS: OTP email sent to {to_email}")
        return 0
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        return 1

if __name__ == "__main__":
    if len(sys.argv) != 7:
        print("Usage: send_email.py <smtp_host> <smtp_port> <username> <password> <to_email> <otp_code>", file=sys.stderr)
        sys.exit(1)
    
    smtp_host = sys.argv[1]
    smtp_port = int(sys.argv[2])
    username = sys.argv[3]
    password = sys.argv[4]
    to_email = sys.argv[5]
    otp_code = sys.argv[6]
    
    sys.exit(send_otp_email(smtp_host, smtp_port, username, password, to_email, otp_code))

