#!/usr/bin/env python3
"""
Script nhanh để thêm meal plan cho hôm nay
"""
import requests
import json
from datetime import datetime

API_BASE = "https://cookm8.vercel.app"
USER_EMAIL = "duyymanhh123@gmail.com"

def main():
    import sys
    
    # Get OTP from command line argument or prompt
    if len(sys.argv) > 1:
        otp = sys.argv[1].strip()
        print(f"🔑 Using OTP from command line: {otp}")
    else:
        print("🔑 Vui lòng nhập OTP code để đăng nhập:")
        otp = input("OTP (6 digits): ").strip()
    
    # Verify OTP
    print("\n🔐 Verifying OTP...")
    response = requests.post(
        f"{API_BASE}/api/auth/otp/verify",
        json={"email": USER_EMAIL, "otp": otp}
    )
    
    if not response.ok:
        print(f"❌ OTP verification failed: {response.status_code}")
        print(response.text)
        return
    
    data = response.json()
    token = data.get('token') or data.get('access_token')
    if not token:
        print("❌ No token in response")
        return
    
    print("✅ Logged in successfully!")
    
    # Get existing recipes
    print("\n📋 Getting recipes...")
    recipes_resp = requests.get(
        f"{API_BASE}/api/recipes/search?query=chicken",
        headers={"Authorization": f"Bearer {token}"}
    )
    
    recipe_ids = []
    if recipes_resp.ok:
        recipes_data = recipes_resp.json()
        if isinstance(recipes_data, list):
            recipe_ids = [str(r.get('id', '')) for r in recipes_data[:3] if r.get('id')]
        elif isinstance(recipes_data, dict):
            # Try different keys
            for key in ['recipes', 'data', 'items', 'results']:
                if key in recipes_data and isinstance(recipes_data[key], list):
                    recipe_ids = [str(r.get('id', '')) for r in recipes_data[key][:3] if r.get('id')]
                    break
    
    if not recipe_ids:
        recipe_ids = ["642264", "642265", "642266"]  # Fallback
    
    print(f"   Using recipe IDs: {recipe_ids}")
    
    # Create meal plan for today
    today = datetime.now().replace(hour=0, minute=0, second=0, microsecond=0)
    meal_plan = {
        "name": "Kế hoạch bữa ăn hôm nay",
        "recipeIds": recipe_ids,
        "notes": "Bữa sáng, trưa và tối cho ngày hôm nay",
        "date": today.isoformat() + "Z"
    }
    
    print(f"\n📅 Creating meal plan for today ({today.strftime('%Y-%m-%d')})...")
    print(f"   Name: {meal_plan['name']}")
    print(f"   Recipes: {', '.join(recipe_ids)}")
    
    response = requests.post(
        f"{API_BASE}/api/meal-plans",
        json=meal_plan,
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {token}"
        }
    )
    
    if response.ok:
        print("✅ Meal plan created successfully!")
        print(f"   Response: {response.json()}")
    else:
        print(f"❌ Failed: {response.status_code}")
        print(f"   Response: {response.text}")
        # Check if it's a duplicate
        if response.status_code == 409 or "duplicate" in response.text.lower():
            print("\n⚠️  Meal plan for today already exists!")
            print("   (This is OK - data is already there)")

if __name__ == "__main__":
    main()

