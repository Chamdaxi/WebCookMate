#!/usr/bin/env python3
"""
Test script để xem API response format
"""

import requests
import json
import sys

API_BASE = "https://cookm8.vercel.app"
USER_EMAIL = "duyymanhh123@gmail.com"

def verify_otp(otp_code):
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

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python3 test_api_response.py <OTP_CODE>")
        sys.exit(1)
    
    otp_code = sys.argv[1]
    print(f"🔐 Verifying OTP: {otp_code}")
    
    token = verify_otp(otp_code)
    if not token:
        print("❌ Invalid OTP")
        sys.exit(1)
    
    print("✅ OTP verified!\n")
    
    # Test categories API
    print("="*60)
    print("📁 TESTING CATEGORIES API")
    print("="*60)
    
    headers = {"Authorization": f"Bearer {token}"}
    response = requests.get(f"{API_BASE}/api/ingredient-categories", headers=headers)
    
    print(f"\nStatus Code: {response.status_code}")
    print(f"Response Type: {type(response.json())}")
    print(f"\nFull Response:")
    print(json.dumps(response.json(), indent=2, ensure_ascii=False))
    
    # Test structure
    data = response.json()
    if isinstance(data, list):
        print(f"\n✅ Response is a LIST with {len(data)} items")
        if len(data) > 0:
            print(f"\nFirst item structure:")
            print(json.dumps(data[0], indent=2, ensure_ascii=False))
    elif isinstance(data, dict):
        print(f"\n✅ Response is a DICT")
        print(f"Keys: {list(data.keys())}")
        if 'data' in data:
            print(f"Data type: {type(data['data'])}")
            if isinstance(data['data'], list) and len(data['data']) > 0:
                print(f"\nFirst item in data:")
                print(json.dumps(data['data'][0], indent=2, ensure_ascii=False))

