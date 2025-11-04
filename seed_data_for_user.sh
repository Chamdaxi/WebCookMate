#!/bin/bash

# Script để tạo sample data cho user qua CookMate API
# Email: duyymanhh123@gmail.com

echo "=========================================="
echo "🌱 SEED DATA FOR USER"
echo "=========================================="
echo ""

API_BASE="https://cookm8.vercel.app"

# Bước 1: Đăng nhập bằng OTP để lấy token
echo "📧 Step 1: Send OTP to duyymanhh123@gmail.com"
echo ""

SEND_OTP_RESPONSE=$(curl -s -X POST "$API_BASE/api/auth/otp" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "duyymanhh123@gmail.com"
  }')

echo "Response: $SEND_OTP_RESPONSE"
echo ""
echo "⏳ Vui lòng:"
echo "   1. Check email duyymanhh123@gmail.com"
echo "   2. Lấy mã OTP (6 chữ số)"
echo "   3. Nhập vào đây:"
echo ""
read -p "Nhập OTP: " OTP_CODE

echo ""
echo "🔐 Step 2: Verify OTP and get token..."
echo ""

VERIFY_RESPONSE=$(curl -s -X POST "$API_BASE/api/auth/otp/verify" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"duyymanhh123@gmail.com\",
    \"otp\": \"$OTP_CODE\"
  }")

echo "Response: $VERIFY_RESPONSE"
echo ""

# Extract token từ response
TOKEN=$(echo $VERIFY_RESPONSE | jq -r '.token // .access_token // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    echo "❌ Không lấy được token! Vui lòng thử lại."
    exit 1
fi

echo "✅ Got token: ${TOKEN:0:50}..."
echo ""

# Bước 2: Tạo Categories
echo "=========================================="
echo "📁 Step 3: Creating Categories..."
echo "=========================================="
echo ""

# Category 1: Trái cây
echo "1. Creating 'Trái cây' category..."
CAT1_RESPONSE=$(curl -s -X POST "$API_BASE/api/ingredient-categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Trái cây",
    "icon": "🍎"
  }')
CAT1_ID=$(echo $CAT1_RESPONSE | jq -r '.id // ._id // empty')
echo "   ✅ Created: $CAT1_ID"

# Category 2: Rau củ
echo "2. Creating 'Rau củ' category..."
CAT2_RESPONSE=$(curl -s -X POST "$API_BASE/api/ingredient-categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Rau củ",
    "icon": "🥕"
  }')
CAT2_ID=$(echo $CAT2_RESPONSE | jq -r '.id // ._id // empty')
echo "   ✅ Created: $CAT2_ID"

# Category 3: Thịt
echo "3. Creating 'Thịt' category..."
CAT3_RESPONSE=$(curl -s -X POST "$API_BASE/api/ingredient-categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Thịt",
    "icon": "🥩"
  }')
CAT3_ID=$(echo $CAT3_RESPONSE | jq -r '.id // ._id // empty')
echo "   ✅ Created: $CAT3_ID"

# Category 4: Sữa & Trứng
echo "4. Creating 'Sữa & Trứng' category..."
CAT4_RESPONSE=$(curl -s -X POST "$API_BASE/api/ingredient-categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Sữa & Trứng",
    "icon": "🥛"
  }')
CAT4_ID=$(echo $CAT4_RESPONSE | jq -r '.id // ._id // empty')
echo "   ✅ Created: $CAT4_ID"

# Category 5: Gia vị
echo "5. Creating 'Gia vị' category..."
CAT5_RESPONSE=$(curl -s -X POST "$API_BASE/api/ingredient-categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Gia vị",
    "icon": "🧂"
  }')
CAT5_ID=$(echo $CAT5_RESPONSE | jq -r '.id // ._id // empty')
echo "   ✅ Created: $CAT5_ID"

echo ""
echo "✅ Created 5 categories!"
echo ""

# Bước 3: Tạo Ingredients (cần FormData, dùng JSON thay thế)
echo "=========================================="
echo "🥗 Step 4: Creating Ingredients..."
echo "=========================================="
echo ""

# Ingredient 1: Táo
if [ ! -z "$CAT1_ID" ]; then
    echo "1. Creating 'Táo'..."
    curl -s -X POST "$API_BASE/api/ingredients" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"categoryId\": \"$CAT1_ID\",
        \"name\": \"Táo\",
        \"quantity\": 5,
        \"unit\": \"quả\",
        \"expireDate\": \"2025-12-31T00:00:00Z\",
        \"notes\": \"Táo Fuji nhập khẩu\"
      }" > /dev/null
    echo "   ✅ Created Táo"
fi

# Ingredient 2: Cà rốt
if [ ! -z "$CAT2_ID" ]; then
    echo "2. Creating 'Cà rốt'..."
    curl -s -X POST "$API_BASE/api/ingredients" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"categoryId\": \"$CAT2_ID\",
        \"name\": \"Cà rốt\",
        \"quantity\": 3,
        \"unit\": \"củ\",
        \"expireDate\": \"2025-11-15T00:00:00Z\",
        \"notes\": \"Cà rốt Đà Lạt\"
      }" > /dev/null
    echo "   ✅ Created Cà rốt"
fi

# Ingredient 3: Thịt gà
if [ ! -z "$CAT3_ID" ]; then
    echo "3. Creating 'Thịt gà'..."
    curl -s -X POST "$API_BASE/api/ingredients" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"categoryId\": \"$CAT3_ID\",
        \"name\": \"Thịt gà\",
        \"quantity\": 500,
        \"unit\": \"gram\",
        \"expireDate\": \"2025-11-05T00:00:00Z\",
        \"notes\": \"Gà ta sạch\"
      }" > /dev/null
    echo "   ✅ Created Thịt gà"
fi

# Ingredient 4: Sữa tươi
if [ ! -z "$CAT4_ID" ]; then
    echo "4. Creating 'Sữa tươi'..."
    curl -s -X POST "$API_BASE/api/ingredients" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"categoryId\": \"$CAT4_ID\",
        \"name\": \"Sữa tươi\",
        \"quantity\": 1,
        \"unit\": \"hộp\",
        \"expireDate\": \"2025-11-10T00:00:00Z\",
        \"notes\": \"Vinamilk 1L\"
      }" > /dev/null
    echo "   ✅ Created Sữa tươi"
fi

# Ingredient 5: Trứng gà
if [ ! -z "$CAT4_ID" ]; then
    echo "5. Creating 'Trứng gà'..."
    curl -s -X POST "$API_BASE/api/ingredients" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"categoryId\": \"$CAT4_ID\",
        \"name\": \"Trứng gà\",
        \"quantity\": 10,
        \"unit\": \"quả\",
        \"expireDate\": \"2025-11-20T00:00:00Z\",
        \"notes\": \"Trứng gà sạch\"
      }" > /dev/null
    echo "   ✅ Created Trứng gà"
fi

# Ingredient 6: Muối
if [ ! -z "$CAT5_ID" ]; then
    echo "6. Creating 'Muối'..."
    curl -s -X POST "$API_BASE/api/ingredients" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"categoryId\": \"$CAT5_ID\",
        \"name\": \"Muối\",
        \"quantity\": 1,
        \"unit\": \"gói\",
        \"expireDate\": \"2026-12-31T00:00:00Z\",
        \"notes\": \"Muối biển Việt Nam\"
      }" > /dev/null
    echo "   ✅ Created Muối"
fi

echo ""
echo "✅ Created 6+ ingredients!"
echo ""

# Bước 4: Tạo Favorites (cần recipe IDs thực tế)
echo "=========================================="
echo "⭐ Step 5: Creating Favorites..."
echo "=========================================="
echo ""

echo "1. Adding recipe to favorites..."
curl -s -X POST "$API_BASE/api/favorites" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "recipeId": "recipe_001"
  }' > /dev/null
echo "   ✅ Added recipe_001"

echo "2. Adding another recipe..."
curl -s -X POST "$API_BASE/api/favorites" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "recipeId": "recipe_002"
  }' > /dev/null
echo "   ✅ Added recipe_002"

echo ""
echo "✅ Created favorites!"
echo ""

# Bước 5: Verify data
echo "=========================================="
echo "✅ VERIFICATION"
echo "=========================================="
echo ""

echo "📊 Checking created data..."
echo ""

echo "Categories:"
curl -s -X GET "$API_BASE/api/ingredient-categories" \
  -H "Authorization: Bearer $TOKEN" | jq '.[] | {id, name, icon}' 2>/dev/null

echo ""
echo "Ingredients:"
curl -s -X GET "$API_BASE/api/ingredients" \
  -H "Authorization: Bearer $TOKEN" | jq '.[] | {id, name, quantity, unit}' 2>/dev/null | head -30

echo ""
echo "Favorites:"
curl -s -X GET "$API_BASE/api/favorites" \
  -H "Authorization: Bearer $TOKEN" | jq '.[] | {id, recipeId}' 2>/dev/null

echo ""
echo "=========================================="
echo "🎉 DONE! Data đã được tạo thành công!"
echo "=========================================="
echo ""
echo "📝 Giờ bạn có thể:"
echo "   1. Đăng nhập web: http://localhost:5134"
echo "   2. Dùng email: duyymanhh123@gmail.com"
echo "   3. Vào Pantry → Thấy categories + ingredients"
echo "   4. Vào Favorites → Thấy favorite recipes"
echo ""
echo "🔑 Token (save nếu cần):"
echo "   $TOKEN"
echo ""

