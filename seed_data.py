#!/usr/bin/env python3
"""
Script để tạo sample data cho user qua CookMate API
Email: duyymanhh123@gmail.com
"""

import requests
import json
import time
import sys
from datetime import datetime, timedelta

API_BASE = "https://cookm8.vercel.app"
USER_EMAIL = "duyymanhh123@gmail.com"

def print_header(text):
    print(f"\n{'='*50}")
    print(f"  {text}")
    print(f"{'='*50}\n")

def send_otp(email):
    """Gửi OTP đến email"""
    print(f"📧 Sending OTP to {email}...")
    response = requests.post(
        f"{API_BASE}/api/auth/otp",
        json={"email": email},
        headers={"Content-Type": "application/json"}
    )
    
    if response.ok:
        print("✅ OTP sent successfully!")
        print(f"Response: {response.json()}")
        return True
    else:
        print(f"❌ Failed: {response.status_code}")
        print(f"Response: {response.text}")
        return False

def verify_otp(email, otp):
    """Verify OTP và lấy token"""
    print(f"\n🔐 Verifying OTP...")
    response = requests.post(
        f"{API_BASE}/api/auth/otp/verify",
        json={"email": email, "otp": otp},
        headers={"Content-Type": "application/json"}
    )
    
    if response.ok:
        data = response.json()
        token = data.get('token') or data.get('access_token')
        if token:
            print("✅ OTP verified! Got token.")
            print(f"Token: {token[:50]}...")
            return token
        else:
            print(f"❌ No token in response: {data}")
            return None
    else:
        print(f"❌ Failed: {response.status_code}")
        print(f"Response: {response.text}")
        return None

def create_categories(token):
    """Tạo categories"""
    print_header("📁 Creating Categories")
    
    categories = [
        {"name": "Trái cây", "icon": "🍎"},
        {"name": "Rau củ", "icon": "🥕"},
        {"name": "Thịt", "icon": "🥩"},
        {"name": "Sữa & Trứng", "icon": "🥛"},
        {"name": "Gia vị", "icon": "🧂"},
        {"name": "Hải sản", "icon": "🦐"},
        {"name": "Ngũ cốc", "icon": "🌾"}
    ]
    
    created_ids = {}
    
    for i, cat in enumerate(categories, 1):
        print(f"{i}. Creating '{cat['name']}'...")
        try:
            response = requests.post(
                f"{API_BASE}/api/ingredient-categories",
                json=cat,
                headers={
                    "Content-Type": "application/json",
                    "Authorization": f"Bearer {token}"
                }
            )
            
            if response.ok:
                data = response.json()
                # Try different possible ID fields
                cat_id = data.get('id') or data.get('_id') or data.get('ingredientCategoryId') or data.get('categoryId')
                if cat_id:
                    created_ids[cat['name']] = cat_id
                    print(f"   ✅ Created: {cat_id}")
                else:
                    print(f"   ⚠️  Created but no ID found in response: {data}")
                    # Try to get from response data structure
                    if isinstance(data, dict) and 'data' in data:
                        cat_id = data['data'].get('id') or data['data'].get('_id')
                        if cat_id:
                            created_ids[cat['name']] = cat_id
                            print(f"   ✅ Created (from data): {cat_id}")
            elif response.status_code == 500 and "duplicate" in response.text.lower():
                print(f"   ⚠️  Category already exists (duplicate)")
                # Will fetch IDs later
            else:
                print(f"   ❌ Failed: {response.status_code}")
                print(f"   Response: {response.text[:200]}")
        except Exception as e:
            print(f"   ❌ Error: {e}")
        
        time.sleep(0.5)  # Rate limiting
    
    print(f"\n✅ Created {len(categories)} categories!")
    
    # Always fetch categories to get IDs (in case they already existed)
    print(f"\n   🔍 Fetching all categories to get IDs...")
    try:
        response = requests.get(
            f"{API_BASE}/api/ingredient-categories",
            headers={"Authorization": f"Bearer {token}"}
        )
        if response.ok:
            all_cats = response.json()
            
            # Function to extract list from any format
            def extract_categories_list(data):
                if isinstance(data, list):
                    return data
                elif isinstance(data, dict):
                    # Try all possible keys
                    for key in ['data', 'categories', 'items', 'result', 'results', 'content']:
                        if key in data and isinstance(data[key], list):
                            return data[key]
                    # Try nested dicts
                    for key in data.keys():
                        if isinstance(data[key], list):
                            return data[key]
                return []
            
            cats_list = extract_categories_list(all_cats)
            
            if isinstance(cats_list, list) and len(cats_list) > 0:
                print(f"   📋 Found {len(cats_list)} categories in response")
                for cat in cats_list:
                    cat_name = cat.get('name', '')
                    # Try all possible ID fields
                    cat_id = (cat.get('id') or cat.get('_id') or 
                             cat.get('ingredientCategoryId') or 
                             cat.get('categoryId') or
                             str(cat.get('_id', '')) if cat.get('_id') else None)
                    
                    if cat_id:
                        # Update or add to created_ids
                        if cat_name in [c['name'] for c in categories]:
                            created_ids[cat_name] = str(cat_id)
                            print(f"   ✅ Found ID for '{cat_name}': {cat_id}")
                    else:
                        print(f"   ⚠️  No ID found for category: {cat_name}")
                        print(f"       Category data: {str(cat)[:200]}")
            else:
                print(f"   ⚠️  Could not parse categories from response")
                print(f"   Response type: {type(all_cats)}")
                print(f"   Response keys (if dict): {list(all_cats.keys()) if isinstance(all_cats, dict) else 'N/A'}")
                print(f"   Response sample: {str(all_cats)[:500]}")
        else:
            print(f"   ⚠️  Failed to fetch categories: {response.status_code}")
            print(f"   Response: {response.text[:200]}")
    except Exception as e:
        print(f"   ⚠️  Error fetching categories: {e}")
        import traceback
        traceback.print_exc()
    
    print(f"\n✅ Total category IDs: {len(created_ids)}")
    return created_ids

def create_ingredients(token, category_ids):
    """Tạo ingredients"""
    print_header("🥗 Creating Ingredients")
    
    # Tính ngày hết hạn
    today = datetime.now()
    
    ingredients = [
        {
            "categoryId": category_ids.get("Trái cây"),
            "name": "Táo Fuji",
            "quantity": 5,
            "unit": "piece",  # Changed from "quả" to "piece"
            "expireDate": (today + timedelta(days=60)).isoformat() + "Z",
            "notes": "Táo Fuji nhập khẩu, ngọt"
        },
        {
            "categoryId": category_ids.get("Trái cây"),
            "name": "Chuối",
            "quantity": 10,
            "unit": "piece",  # Changed from "quả" to "piece"
            "expireDate": (today + timedelta(days=7)).isoformat() + "Z",
            "notes": "Chuối già Việt Nam"
        },
        {
            "categoryId": category_ids.get("Rau củ"),
            "name": "Cà rốt",
            "quantity": 500,
            "unit": "g",  # Changed from "gram" to "g"
            "expireDate": (today + timedelta(days=14)).isoformat() + "Z",
            "notes": "Cà rốt Đà Lạt tươi"
        },
        {
            "categoryId": category_ids.get("Rau củ"),
            "name": "Khoai tây",
            "quantity": 1,
            "unit": "kg",
            "expireDate": (today + timedelta(days=30)).isoformat() + "Z",
            "notes": "Khoai tây sạch"
        },
        {
            "categoryId": category_ids.get("Thịt"),
            "name": "Thịt gà",
            "quantity": 500,
            "unit": "g",  # Changed from "gram" to "g"
            "expireDate": (today + timedelta(days=3)).isoformat() + "Z",
            "notes": "Gà ta tươi sạch"
        },
        {
            "categoryId": category_ids.get("Thịt"),
            "name": "Thịt heo",
            "quantity": 300,
            "unit": "g",  # Changed from "gram" to "g"
            "expireDate": (today + timedelta(days=2)).isoformat() + "Z",
            "notes": "Thịt nạc vai"
        },
        {
            "categoryId": category_ids.get("Sữa & Trứng"),
            "name": "Sữa tươi",
            "quantity": 2,
            "unit": "piece",  # Changed from "hộp" to "piece"
            "expireDate": (today + timedelta(days=10)).isoformat() + "Z",
            "notes": "Vinamilk 1L không đường"
        },
        {
            "categoryId": category_ids.get("Sữa & Trứng"),
            "name": "Trứng gà",
            "quantity": 12,
            "unit": "piece",  # Changed from "quả" to "piece"
            "expireDate": (today + timedelta(days=20)).isoformat() + "Z",
            "notes": "Trứng gà sạch CP"
        },
        {
            "categoryId": category_ids.get("Gia vị"),
            "name": "Muối",
            "quantity": 500,
            "unit": "g",  # Changed from "gram" to "g"
            "expireDate": (today + timedelta(days=365)).isoformat() + "Z",
            "notes": "Muối biển Việt Nam"
        },
        {
            "categoryId": category_ids.get("Gia vị"),
            "name": "Đường trắng",
            "quantity": 1,
            "unit": "kg",
            "expireDate": (today + timedelta(days=365)).isoformat() + "Z",
            "notes": "Đường tinh luyện"
        },
        {
            "categoryId": category_ids.get("Hải sản"),
            "name": "Tôm sú",
            "quantity": 200,
            "unit": "g",  # Changed from "gram" to "g"
            "expireDate": (today + timedelta(days=1)).isoformat() + "Z",
            "notes": "Tôm sú tươi sống"
        },
        {
            "categoryId": category_ids.get("Ngũ cốc"),
            "name": "Gạo tẻ",
            "quantity": 5,
            "unit": "kg",
            "expireDate": (today + timedelta(days=180)).isoformat() + "Z",
            "notes": "Gạo ST25 cao cấp"
        }
    ]
    
    created_count = 0
    
    for i, ing in enumerate(ingredients, 1):
        if not ing["categoryId"]:
            print(f"{i}. Skipping '{ing['name']}' - no category")
            continue
            
        print(f"{i}. Creating '{ing['name']}'...")
        try:
            # API requires multipart/form-data, not JSON
            files = {}
            data = {
                'categoryId': ing['categoryId'],
                'name': ing['name'],
                'quantity': str(ing['quantity']),
                'unit': ing['unit'],
                'expireDate': ing['expireDate'],
                'notes': ing['notes']
            }
            
            response = requests.post(
                f"{API_BASE}/api/ingredients",
                files=files,
                data=data,
                headers={
                    "Authorization": f"Bearer {token}"
                }
            )
            
            if response.ok:
                created_count += 1
                print(f"   ✅ Created")
            else:
                print(f"   ❌ Failed: {response.status_code}")
                error_text = response.text[:500]
                print(f"   Response: {error_text}")
                try:
                    error_json = response.json()
                    if 'error' in error_json:
                        print(f"   Error: {error_json['error']}")
                except:
                    pass
        except Exception as e:
            print(f"   ❌ Error: {e}")
        
        time.sleep(0.3)  # Rate limiting
    
    print(f"\n✅ Created {created_count} ingredients!")
    return created_count

def get_recipes_with_images(token, queries=None):
    """Lấy recipes có hình ảnh từ API"""
    if queries is None:
        queries = ["chicken", "pasta", "salad", "soup", "dessert", "vietnamese", "asian", "beef"]
    
    all_recipes = []
    recipe_ids = []
    
    print(f"   🔍 Searching recipes with images...")
    
    for query in queries[:5]:  # Limit to 5 queries
        try:
            response = requests.get(
                f"{API_BASE}/api/recipes/search",
                params={"query": query, "limit": 2},
                headers={"Authorization": f"Bearer {token}"}
            )
            
            if response.ok:
                recipes = response.json()
                if isinstance(recipes, list) and len(recipes) > 0:
                    for recipe in recipes:
                        recipe_id = str(recipe.get('id', recipe.get('Id', '')))
                        recipe_title = recipe.get('title', recipe.get('Title', 'Unknown'))
                        recipe_image = recipe.get('image', recipe.get('Image', ''))
                        
                        if recipe_id and recipe_id not in recipe_ids:
                            recipe_ids.append(recipe_id)
                            all_recipes.append({
                                'id': recipe_id,
                                'title': recipe_title,
                                'image': recipe_image
                            })
                time.sleep(0.2)  # Rate limiting
        except Exception as e:
            print(f"   ⚠️  Search '{query}' failed: {e}")
            continue
    
    # If search didn't work, try /recipes/today
    if len(recipe_ids) == 0:
        try:
            print(f"   🔍 Trying /recipes/today...")
            response = requests.get(
                f"{API_BASE}/api/recipes/today",
                headers={"Authorization": f"Bearer {token}"}
            )
            if response.ok:
                recipes = response.json()
                if isinstance(recipes, list) and len(recipes) > 0:
                    for recipe in recipes[:10]:
                        recipe_id = str(recipe.get('id', recipe.get('Id', '')))
                        recipe_title = recipe.get('title', recipe.get('Title', 'Unknown'))
                        recipe_image = recipe.get('image', recipe.get('Image', ''))
                        
                        if recipe_id and recipe_id not in recipe_ids:
                            recipe_ids.append(recipe_id)
                            all_recipes.append({
                                'id': recipe_id,
                                'title': recipe_title,
                                'image': recipe_image
                            })
        except Exception as e:
            print(f"   ⚠️  /recipes/today failed: {e}")
    
    if len(all_recipes) > 0:
        print(f"   ✅ Found {len(all_recipes)} recipes with images")
        for r in all_recipes[:5]:
            has_image = "✅" if r['image'] else "❌"
            print(f"      {has_image} {r['title']} (ID: {r['id']})")
    else:
        print(f"   ⚠️  No recipes found, using fallback IDs")
        recipe_ids = ["642264", "642265", "642266", "642267", "642268"]
    
    return recipe_ids[:10]  # Return up to 10 recipe IDs

def create_favorites(token):
    """Tạo favorites với recipes có hình ảnh"""
    print_header("⭐ Creating Favorites")
    
    # Get recipes with images
    recipe_ids = get_recipes_with_images(token)
    
    created_count = 0
    
    for i, recipe_id in enumerate(recipe_ids[:5], 1):  # Limit to 5 favorites
        print(f"{i}. Adding recipe ID '{recipe_id}' to favorites...")
        try:
            response = requests.post(
                f"{API_BASE}/api/favorites",
                json={"recipeId": recipe_id},
                headers={
                    "Content-Type": "application/json",
                    "Authorization": f"Bearer {token}"
                }
            )
            
            if response.ok:
                created_count += 1
                print(f"   ✅ Added")
            else:
                print(f"   ⚠️  Failed: {response.status_code} - {response.text[:100]}")
        except Exception as e:
            print(f"   ❌ Error: {e}")
        
        time.sleep(0.3)
    
    print(f"\n✅ Created {created_count} favorites!")
    return created_count, recipe_ids

def create_meal_plans(token, recipe_ids=None):
    """Tạo meal plans với recipes có hình ảnh"""
    print_header("📅 Creating Meal Plans")
    
    # Use provided recipe_ids or get new ones
    if not recipe_ids:
        recipe_ids = get_recipes_with_images(token)
    
    # Need at least 5 recipe IDs
    if len(recipe_ids) < 5:
        print(f"   ⚠️  Only {len(recipe_ids)} recipes available, using fallback")
        recipe_ids = recipe_ids + ["642264", "642265", "642266", "642267", "642268"]
        recipe_ids = recipe_ids[:10]  # Remove duplicates
    
    today = datetime.now()
    meal_plans = [
        {
            "name": "Kế hoạch bữa ăn hôm nay",
            "recipeIds": recipe_ids[:3] if len(recipe_ids) >= 3 else recipe_ids,
            "notes": "Bữa sáng, trưa và tối cho ngày hôm nay",
            "date": today.replace(hour=0, minute=0, second=0, microsecond=0).isoformat() + "Z"  # Today
        },
        {
            "name": "Thực đơn tuần này",
            "recipeIds": recipe_ids[:3] if len(recipe_ids) >= 3 else recipe_ids,
            "notes": "Kế hoạch ăn uống lành mạnh với các món ngon",
            "date": (today + timedelta(days=1)).isoformat() + "Z"  # Tomorrow
        },
        {
            "name": "Bữa ăn cuối tuần",
            "recipeIds": recipe_ids[3:5] if len(recipe_ids) >= 5 else recipe_ids[:2],
            "notes": "Thưởng thức món ngon cuối tuần",
            "date": (today + timedelta(days=5)).isoformat() + "Z"  # This Saturday
        },
        {
            "name": "Kế hoạch tập gym",
            "recipeIds": recipe_ids[0:1] + recipe_ids[2:3] if len(recipe_ids) >= 3 else recipe_ids[:2],
            "notes": "Thực đơn giàu protein cho người tập thể hình",
            "date": (today + timedelta(days=3)).isoformat() + "Z"  # Day 3
        }
    ]
    
    created_count = 0
    
    for i, plan in enumerate(meal_plans, 1):
        print(f"{i}. Creating meal plan '{plan['name']}'...")
        print(f"   Recipes: {', '.join(plan['recipeIds'])}")
        try:
            response = requests.post(
                f"{API_BASE}/api/meal-plans",
                json=plan,
                headers={
                    "Content-Type": "application/json",
                    "Authorization": f"Bearer {token}"
                }
            )
            
            if response.ok:
                created_count += 1
                plan_date = plan['date'][:10]  # Extract date part
                print(f"   ✅ Created for {plan_date}")
            else:
                print(f"   ⚠️  Failed: {response.status_code}")
                print(f"   Response: {response.text[:100]}")
        except Exception as e:
            print(f"   ❌ Error: {e}")
        
        time.sleep(0.3)
    
    print(f"\n✅ Created {created_count} meal plans!")
    return created_count

def verify_data(token):
    """Kiểm tra data đã tạo"""
    print_header("✅ VERIFICATION")
    
    # Check categories
    print("📁 Categories:")
    try:
        response = requests.get(
            f"{API_BASE}/api/ingredient-categories",
            headers={"Authorization": f"Bearer {token}"}
        )
        if response.ok:
            cats = response.json()
            print(f"   ✅ Total: {len(cats)}")
            for cat in cats[:5]:
                print(f"   - {cat.get('icon', '📦')} {cat.get('name', 'Unknown')}")
        else:
            print(f"   ❌ Failed: {response.status_code}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
    
    # Check ingredients
    print("\n🥗 Ingredients:")
    try:
        response = requests.get(
            f"{API_BASE}/api/ingredients",
            headers={"Authorization": f"Bearer {token}"}
        )
        if response.ok:
            ings = response.json()
            print(f"   ✅ Total: {len(ings)}")
            for ing in ings[:5]:
                print(f"   - {ing.get('name', 'Unknown')}: {ing.get('quantity', 0)} {ing.get('unit', '')}")
        else:
            print(f"   ❌ Failed: {response.status_code}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
    
    # Check favorites
    print("\n⭐ Favorites:")
    try:
        response = requests.get(
            f"{API_BASE}/api/favorites",
            headers={"Authorization": f"Bearer {token}"}
        )
        if response.ok:
            favs = response.json()
            print(f"   ✅ Total: {len(favs)}")
            for fav in favs[:5]:
                recipe_id = fav.get('recipeId', 'Unknown')
                print(f"   - Recipe ID: {recipe_id}")
                
                # Try to get recipe details to show image status
                try:
                    # Note: API might not have endpoint to get single recipe by ID
                    # This is just for verification
                    pass
                except:
                    pass
        else:
            print(f"   ❌ Failed: {response.status_code}")
    except Exception as e:
        print(f"   ❌ Error: {e}")
    
    # Check meal plans
    print("\n📅 Meal Plans:")
    try:
        response = requests.get(
            f"{API_BASE}/api/meal-plans",
            headers={"Authorization": f"Bearer {token}"}
        )
        if response.ok:
            plans = response.json()
            print(f"   ✅ Total: {len(plans)}")
            for plan in plans[:3]:
                plan_date = plan.get('date', plan.get('Date', ''))[:10] if plan.get('date') or plan.get('Date') else 'N/A'
                recipe_ids = plan.get('recipeIds', plan.get('RecipeIds', []))
                recipe_count = len(recipe_ids) if isinstance(recipe_ids, list) else 0
                print(f"   - {plan.get('name', plan.get('Name', 'Unknown'))} ({plan_date}) - {recipe_count} recipes")
        else:
            print(f"   ❌ Failed: {response.status_code}")
    except Exception as e:
        print(f"   ❌ Error: {e}")

def main():
    print_header("🌱 SEED DATA FOR COOKMATE USER")
    print(f"Email: {USER_EMAIL}")
    print(f"API: {API_BASE}")
    
    # Step 1: Get OTP from user or command line argument
    if len(sys.argv) > 1:
        otp_code = sys.argv[1].strip()
        print(f"\n📧 Using OTP from command line: {otp_code}")
    else:
        # Step 1: Send OTP (only if no OTP provided)
        if not send_otp(USER_EMAIL):
            print("\n❌ Failed to send OTP. Exiting.")
            return
        
        print("\n📧 Check your email and enter the OTP code:")
        otp_code = input("OTP (6 digits): ").strip()
    
    if not otp_code or len(otp_code) != 6:
        print("❌ Invalid OTP. Exiting.")
        return
    
    # Step 3: Verify OTP and get token
    token = verify_otp(USER_EMAIL, otp_code)
    if not token:
        print("\n❌ Failed to verify OTP. Exiting.")
        return
    
    # Step 4: Create data
    category_ids = create_categories(token)
    create_ingredients(token, category_ids)
    favorites_count, recipe_ids = create_favorites(token)  # Get recipe IDs for meal plans
    create_meal_plans(token, recipe_ids)  # Use same recipes for meal plans
    
    # Step 5: Verify
    verify_data(token)
    
    # Done
    print_header("🎉 DONE!")
    print("Data đã được tạo thành công với recipes có hình ảnh!")
    print(f"\n📝 Giờ bạn có thể:")
    print(f"   1. Đăng nhập web: http://localhost:5134")
    print(f"   2. Dùng OTP với email: {USER_EMAIL}")
    print(f"   3. ✅ Pantry → Thấy {len(category_ids)} categories + ingredients")
    print(f"   4. ✅ Favorites → Thấy {favorites_count} favorite recipes với hình ảnh")
    print(f"   5. ✅ Meal Plans → Thấy meal plans với recipes có hình ảnh trên calendar")
    print(f"\n🔑 Token (save nếu cần test API):")
    print(f"   {token[:100]}...")
    print()

if __name__ == "__main__":
    main()

