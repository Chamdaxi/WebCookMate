#!/usr/bin/env python3
"""
Script để kiểm tra data trên API server
"""

import requests
import sys

API_BASE = "https://cookm8.vercel.app"
USER_EMAIL = "duyymanhh123@gmail.com"

def check_otp(otp_code):
    """Verify OTP và lấy token"""
    response = requests.post(
        f"{API_BASE}/api/auth/otp/verify",
        json={"email": USER_EMAIL, "otp": otp_code},
        headers={"Content-Type": "application/json"}
    )
    
    if response.ok:
        data = response.json()
        token = data.get('token') or data.get('access_token')
        return token
    return None

def check_data(token):
    """Kiểm tra data trên API"""
    headers = {"Authorization": f"Bearer {token}"}
    
    print("\n" + "="*50)
    print("📊 KIỂM TRA DỮ LIỆU")
    print("="*50 + "\n")
    
    # Check Categories
    print("📁 Categories:")
    try:
        response = requests.get(f"{API_BASE}/api/ingredient-categories", headers=headers)
        if response.ok:
            data = response.json()
            # Handle different response formats
            if isinstance(data, list):
                cats = data
            elif isinstance(data, dict):
                cats = data.get('data') or data.get('categories') or []
                if not isinstance(cats, list):
                    cats = []
            else:
                cats = []
            
            print(f"   ✅ Total: {len(cats)}")
            for i, cat in enumerate(cats, 1):
                if i > 7:
                    break
                if isinstance(cat, dict):
                    cat_id = cat.get('id') or cat.get('_id') or cat.get('ingredientCategoryId', 'N/A')
                    print(f"   {i}. {cat.get('icon', '📦')} {cat.get('name', 'Unknown')} (ID: {cat_id})")
                else:
                    print(f"   {i}. {cat}")
        else:
            print(f"   ❌ Failed: {response.status_code}")
            print(f"   Response: {response.text[:200]}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
        import traceback
        traceback.print_exc()
    
    # Check Ingredients
    print("\n🥗 Ingredients:")
    try:
        response = requests.get(f"{API_BASE}/api/ingredients", headers=headers)
        if response.ok:
            data = response.json()
            # Handle different response formats
            if isinstance(data, list):
                ings = data
            elif isinstance(data, dict):
                ings = data.get('data') or data.get('ingredients') or []
                if not isinstance(ings, list):
                    ings = []
            else:
                ings = []
            
            print(f"   ✅ Total: {len(ings)}")
            for i, ing in enumerate(ings, 1):
                if i > 12:
                    break
                if isinstance(ing, dict):
                    print(f"   {i}. {ing.get('name', 'Unknown')}: {ing.get('quantity', 0)} {ing.get('unit', '')}")
                else:
                    print(f"   {i}. {ing}")
        else:
            print(f"   ❌ Failed: {response.status_code}")
            print(f"   Response: {response.text[:200]}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
        import traceback
        traceback.print_exc()
    
    # Check Favorites
    print("\n⭐ Favorites:")
    try:
        response = requests.get(f"{API_BASE}/api/favorites", headers=headers)
        if response.ok:
            favs = response.json()
            print(f"   ✅ Total: {len(favs)}")
            for i, fav in enumerate(favs, 1):
                if i > 5:
                    break
                print(f"   {i}. Recipe ID: {fav.get('recipeId', 'Unknown')}")
        else:
            print(f"   ❌ Failed: {response.status_code}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
    
    # Check Meal Plans
    print("\n📅 Meal Plans:")
    try:
        response = requests.get(f"{API_BASE}/api/meal-plans", headers=headers)
        if response.ok:
            plans = response.json()
            print(f"   ✅ Total: {len(plans)}")
            for i, plan in enumerate(plans, 1):
                if i > 3:
                    break
                plan_date = plan.get('date', plan.get('Date', ''))[:10] if plan.get('date') or plan.get('Date') else 'N/A'
                print(f"   {i}. {plan.get('name', plan.get('Name', 'Unknown'))} ({plan_date})")
        else:
            print(f"   ❌ Failed: {response.status_code}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
    
    print("\n" + "="*50 + "\n")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python3 check_data.py <OTP_CODE>")
        sys.exit(1)
    
    otp_code = sys.argv[1]
    print(f"🔐 Verifying OTP: {otp_code}")
    
    token = check_otp(otp_code)
    if token:
        print("✅ OTP verified!")
        check_data(token)
    else:
        print("❌ Invalid OTP")

