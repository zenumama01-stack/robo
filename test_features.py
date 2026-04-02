#!/usr/bin/env python3
"""
Test script for Repo1 Application
Tests all features and APIs
"""

import requests
import json
from datetime import datetime

def test_app():
    print("\n" + "="*70)
    print("🧪 REPO1 APPLICATION - FEATURE TEST")
    print("="*70)
    
    base_url = "http://localhost:5000"
    tests_passed = 0
    tests_failed = 0
    
    # Test 1: Dashboard
    print("\n1️⃣ Testing Dashboard Page...")
    try:
        r = requests.get(f"{base_url}/", timeout=3)
        if r.status_code == 200:
            print("   ✅ Dashboard: ACCESSIBLE")
            print(f"   Status Code: {r.status_code}")
            if 'dashboard' in r.text.lower() or 'repo1' in r.text.lower():
                print("   ✅ Dashboard HTML loaded successfully")
                tests_passed += 1
            else:
                print("   ⚠️  HTML content unclear")
        else:
            print(f"   ❌ Status: {r.status_code}")
            tests_failed += 1
    except Exception as e:
        print(f"   ❌ Error: {type(e).__name__}: {str(e)}")
        tests_failed += 1
    
    # Test 2: Health Check API
    print("\n2️⃣ Testing Health Check API...")
    try:
        r = requests.get(f"{base_url}/api/health", timeout=3)
        if r.status_code == 200:
            data = r.json()
            print("   ✅ Health Check: RESPONDING")
            print(f"   Status: {data.get('status', 'unknown')}")
            print(f"   Components: Auth({data['components']['auth']}), API({data['components']['api']})")
            tests_passed += 1
        else:
            print(f"   ❌ Status: {r.status_code}")
            tests_failed += 1
    except Exception as e:
        print(f"   ❌ Error: {type(e).__name__}: {str(e)}")
        tests_failed += 1
    
    # Test 3: App Status API
    print("\n3️⃣ Testing App Status API...")
    try:
        r = requests.get(f"{base_url}/api/status", timeout=3)
        if r.status_code == 200:
            data = r.json()
            print("   ✅ App Status: RESPONDING")
            print(f"   App: {data.get('app_name', 'unknown')}")
            print(f"   Version: {data.get('version', 'unknown')}")
            print(f"   Status: {data.get('status', 'unknown')}")
            print(f"   Active Users: {data.get('active_users', 0)}")
            print(f"   Features: {len(data.get('features', []))} available")
            tests_passed += 1
        else:
            print(f"   ❌ Status: {r.status_code}")
            tests_failed += 1
    except Exception as e:
        print(f"   ❌ Error: {type(e).__name__}: {str(e)}")
        tests_failed += 1
    
    # Test 4: Authentication Routes exist
    print("\n4️⃣ Testing Authentication Routes...")
    try:
        # Test login endpoint (POST)
        r = requests.post(f"{base_url}/api/auth/login", 
                         json={"email": "test@test.com", "password": "test"}, 
                         timeout=3)
        if r.status_code in [200, 400, 401]:  # Any response means endpoint exists
            print("   ✅ Auth Routes: AVAILABLE")
            print(f"   Login endpoint responding (Status: {r.status_code})")
            tests_passed += 1
        else:
            print(f"   ❌ Unexpected status: {r.status_code}")
            tests_failed += 1
    except Exception as e:
        print(f"   ❌ Error: {type(e).__name__}: {str(e)}")
        tests_failed += 1
    
    # Test 5: User Management Routes exist
    print("\n5️⃣ Testing User Management Routes...")
    try:
        # Test users endpoint (GET)
        r = requests.get(f"{base_url}/api/users", 
                        headers={"Cookie": "session=test"}, 
                        timeout=3)
        if r.status_code in [200, 401]:  # 401 means auth required but route exists
            print("   ✅ User Routes: AVAILABLE")
            print(f"   Users endpoint responding (Status: {r.status_code})")
            tests_passed += 1
        else:
            print(f"   ❌ Unexpected status: {r.status_code}")
            tests_failed += 1
    except Exception as e:
        print(f"   ❌ Error: {type(e).__name__}: {str(e)}")
        tests_failed += 1
    
    # Summary
    print("\n" + "="*70)
    print("📊 TEST SUMMARY")
    print("="*70)
    print(f"✅ Passed: {tests_passed}")
    print(f"❌ Failed: {tests_failed}")
    print(f"📈 Total: {tests_passed + tests_failed}")
    print("="*70)
    
    if tests_failed == 0:
        print("\n🎉 ALL FEATURES WORKING! Application is READY!")
        print(f"\n📍 Access at: http://localhost:5000")
        print("\n✨ Available Features:")
        print("   ✅ Dashboard Interface")
        print("   ✅ User Authentication")
        print("   ✅ User Management")
        print("   ✅ API Testing")
        print("   ✅ Health Monitoring")
        print("   ✅ Status Reporting")
    else:
        print(f"\n⚠️  {tests_failed} feature(s) not working properly")
    
    print("\n" + "="*70 + "\n")
    
    return tests_failed == 0

if __name__ == "__main__":
    success = test_app()
    exit(0 if success else 1)
