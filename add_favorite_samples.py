#!/usr/bin/env python3
"""
Script để thêm favorites mẫu vào CookMate API
"""

import requests
import json
import time
import sys

# API Base URL
API_BASE = "https://cookm8.vercel.app"

def login_with_google():
    """Đăng nhập bằng Google OAuth"""
    print("=" * 60)
    print("🔐 Đăng nhập bằng Google OAuth")
    print("=" * 60)
    
    # Google user info
    google_user_id = "102881048202682148908"
    email = "duyymanhh123@gmail.com"
    name = "Manh Phạm"
    
    response = requests.post(
        f"{API_BASE}/api/auth/google",
        json={
            "googleUserId": google_user_id,
            "email": email,
            "name": name,
            "avatar": ""
        },
        headers={
            "Content-Type": "application/json"
        }
    )
    
    if response.ok:
        data = response.json()
        token = data.get("token")
        print(f"✅ Đăng nhập thành công!")
        print(f"   Token: {token[:50]}...")
        return token
    else:
        print(f"❌ Đăng nhập thất bại: {response.status_code}")
        print(f"   Response: {response.text}")
        return None

def add_favorite(token, recipe_id):
    """Thêm một recipe vào favorites"""
    try:
        response = requests.post(
            f"{API_BASE}/api/favorites",
            json={"recipeId": str(recipe_id)},
            headers={
                "Content-Type": "application/json",
                "Authorization": f"Bearer {token}"
            }
        )
        
        if response.ok:
            data = response.json()
            print(f"   ✅ Đã thêm Recipe ID {recipe_id} vào favorites")
            return True
        else:
            error_text = response.text
            print(f"   ⚠️  Thất bại: {response.status_code} - {error_text[:100]}")
            return False
    except Exception as e:
        print(f"   ❌ Lỗi: {e}")
        return False

def main():
    """Main function"""
    print("=" * 60)
    print("⭐ THÊM FAVORITES MẪU VÀO COOKMATE API")
    print("=" * 60)
    print()
    
    # Đăng nhập
    token = login_with_google()
    if not token:
        print("❌ Không thể đăng nhập. Vui lòng thử lại.")
        sys.exit(1)
    
    print()
    print("=" * 60)
    print("📝 Thêm Favorites")
    print("=" * 60)
    
    # Danh sách Recipe IDs mẫu (các IDs phổ biến từ Spoonacular)
    # Note: Một số IDs có thể đã tồn tại (duplicate), script sẽ skip chúng
    sample_recipe_ids = [
        "645872",  # Chicken Stir Fry
        "642264",  # Pasta
        "642265",  # Salad
        "642266",  # Soup
        "642267",  # Dessert
        "642268",  # Beef
        "642269",  # Fish
        "642270",  # Vegetarian
        "642271",  # Breakfast
        "642272",  # Lunch
        "642273",  # Dinner
        "642274",  # Snack
        "642275",  # Dessert 2
        "642276",  # Appetizer
    ]
    
    print(f"📋 Sẽ thêm {len(sample_recipe_ids)} favorites mẫu...")
    print()
    
    success_count = 0
    failed_count = 0
    
    for i, recipe_id in enumerate(sample_recipe_ids, 1):
        print(f"{i}. Thêm Recipe ID: {recipe_id}...")
        if add_favorite(token, recipe_id):
            success_count += 1
        else:
            failed_count += 1
        time.sleep(0.5)  # Delay để tránh rate limit
    
    print()
    print("=" * 60)
    print("✅ HOÀN THÀNH")
    print("=" * 60)
    print(f"✅ Thành công: {success_count}")
    print(f"❌ Thất bại: {failed_count}")
    print()
    print("📊 Bây giờ bạn có thể:")
    print("   1. Vào trang FavoriteList: http://localhost:5134/Home/FavoriteList")
    print("   2. Refresh trang để xem các favorites mới được thêm")
    print()

if __name__ == "__main__":
    main()

