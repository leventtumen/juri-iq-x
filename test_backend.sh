#!/bin/bash

echo "🧪 Testing Juri-IQ Backend API..."
echo "=================================="

BACKEND_URL="http://localhost:5001"

# Test 1: Health Check
echo "📍 Testing Health Check..."
curl -s "$BACKEND_URL/api/health" | jq . || echo "Health check response: $(curl -s $BACKEND_URL/api/health)"

echo ""

# Test 2: Admin Login
echo "🔐 Testing Admin Login..."
LOGIN_RESPONSE=$(curl -s -X POST "$BACKEND_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Pass!2345",
    "deviceId": "test-device-123",
    "deviceName": "Test Device",
    "deviceType": "Web"
  }')

echo "Login Response:"
echo "$LOGIN_RESPONSE" | jq . || echo "$LOGIN_RESPONSE"

# Extract token for next tests
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.token // empty')

if [ -n "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
    echo "✅ Login successful! Token extracted."
    
    echo ""
    echo "👥 Testing Admin Endpoints..."
    
    # Test 3: Get Users (Admin Only)
    echo "📊 Getting all users..."
    curl -s -X GET "$BACKEND_URL/api/admin/users" \
      -H "Authorization: Bearer $TOKEN" | jq . || echo "Users response received"
    
    echo ""
    echo "📈 Getting system statistics..."
    curl -s -X GET "$BACKEND_URL/api/admin/stats" \
      -H "Authorization: Bearer $TOKEN" | jq . || echo "Stats response received"
      
    echo ""
    echo "📄 Testing Document Endpoints..."
    
    # Test 4: Get Documents
    echo "📋 Getting documents..."
    curl -s -X GET "$BACKEND_URL/api/documents" \
      -H "Authorization: Bearer $TOKEN" | jq . || echo "Documents response received"
      
else
    echo "❌ Login failed or token not found"
fi

echo ""
echo "🌐 Backend is running on: $BACKEND_URL"
echo "📖 Swagger UI available at: $BACKEND_URL/swagger"
echo "🎯 Frontend available at: https://3000-e446688f-6183-4216-8c14-c55df0457436.proxy.daytona.works"
echo ""
echo "✅ Backend testing complete!"