#!/bin/bash

# LiveExpert.AI Application Test Script
# This script tests the application end-to-end

echo "🧪 LiveExpert.AI Application Test Suite"
echo "========================================"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counters
PASSED=0
FAILED=0

# Function to test API endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local expected_status=$4
    local description=$5
    
    if [ -z "$data" ]; then
        response=$(curl -s -w "\n%{http_code}" -X $method "http://localhost:5128/api$endpoint" \
            -H "Content-Type: application/json" 2>/dev/null)
    else
        response=$(curl -s -w "\n%{http_code}" -X $method "http://localhost:5128/api$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data" 2>/dev/null)
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" == "$expected_status" ]; then
        echo -e "${GREEN}✅ PASS${NC}: $description"
        ((PASSED++))
        return 0
    else
        echo -e "${RED}❌ FAIL${NC}: $description (Expected $expected_status, got $http_code)"
        ((FAILED++))
        return 1
    fi
}

# Check if backend is running
echo "📡 Checking Backend Status..."
if curl -s http://localhost:5128/health > /dev/null 2>&1 || curl -s http://localhost:5128/swagger > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Backend is running${NC}"
else
    echo -e "${RED}❌ Backend is not running${NC}"
    echo "Please start the backend first:"
    echo "  cd LMS-Backend/src/LiveExpert.API && dotnet run"
    exit 1
fi

echo ""
echo "🧪 Testing API Endpoints..."
echo ""

# Test 1: Health Check
test_endpoint "GET" "/health" "" "200" "Health Check"

# Test 2: Swagger Available
if curl -s http://localhost:5128/swagger > /dev/null 2>&1; then
    echo -e "${GREEN}✅ PASS${NC}: Swagger Documentation Available"
    ((PASSED++))
else
    echo -e "${RED}❌ FAIL${NC}: Swagger Documentation Not Available"
    ((FAILED++))
fi

# Test 3: Public Settings (should work without auth)
test_endpoint "GET" "/admin/settings/public" "" "200" "Public Settings Endpoint"

# Test 4: Login (will fail without valid credentials, but endpoint should exist)
test_endpoint "POST" "/auth/login" '{"email":"test@test.com","password":"test"}' "401" "Login Endpoint (Unauthorized Expected)"

echo ""
echo "📊 Test Results:"
echo "================"
echo -e "${GREEN}Passed: $PASSED${NC}"
echo -e "${RED}Failed: $FAILED${NC}"
echo ""

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed!${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠️  Some tests failed. Check the output above.${NC}"
    exit 1
fi


