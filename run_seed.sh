#!/bin/bash
# Script helper để chạy seed data

echo "=========================================="
echo "🌱 TẠO DỮ LIỆU MẪU CHO COOKMATE"
echo "=========================================="
echo ""
echo "📧 Email: duyymanhh123@gmail.com"
echo ""
echo "⚠️  Script sẽ:"
echo "   1. Gửi OTP đến email của bạn"
echo "   2. Yêu cầu nhập OTP code"
echo "   3. Tự động tạo data cho 3 trang:"
echo "      - Pantry (7 categories, 12 ingredients)"
echo "      - Favorites (5 recipes với hình ảnh)"
echo "      - Meal Plans (3 meal plans với recipes)"
echo ""
echo "🚀 Đang bắt đầu..."
echo ""

python3 seed_data.py

