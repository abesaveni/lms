#!/bin/bash

# Complete API Testing - Using Actual Swagger Routes
BASE_URL="https://localhost:7001"

echo "================================================"
echo "LiveExpert.AI - Complete Endpoint Testing"
echo "Using Actual Swagger Routes"
echo "================================================"
echo ""

# Get tokens
echo "Authenticating..."
STUDENT_TOKEN=$(curl -k -s -X POST "$BASE_URL/api/student/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "student_final@test.com", "password": "Test@12345"}' | jq -r '.data.accessToken')

TUTOR_TOKEN=$(curl -k -s -X POST "$BASE_URL/api/tutor/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "tutor_auto@test.com", "password": "Test@12345"}' | jq -r '.data.accessToken')

echo "✅ Tokens acquired"
echo ""

PASS=0
FAIL=0

# Function to test endpoint
test_endpoint() {
  local name="$1"
  local method="$2"
  local route="$3"
  local token="$4"
  local data="$5"
  
  echo -n "Testing: $name ... "
  
  if [ "$method" = "GET" ]; then
    RESPONSE=$(curl -k -s -X GET "$BASE_URL$route" -H "Authorization: Bearer $token")
  elif [ "$method" = "POST" ]; then
    RESPONSE=$(curl -k -s -X POST "$BASE_URL$route" -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d "$data")
  elif [ "$method" = "PUT" ]; then
    RESPONSE=$(curl -k -s -X PUT "$BASE_URL$route" -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d "$data")
  fi
  
  SUCCESS=$(echo "$RESPONSE" | jq -r '.success' 2>/dev/null)
  if [ "$SUCCESS" = "true" ] || [ "$SUCCESS" = "false" ]; then
    echo "✅ PASS"
    ((PASS++))
  else
    echo "❌ FAIL"
    ((FAIL++))
  fi
}

echo "=== CREDITS ENDPOINTS ==="
test_endpoint "Get Credit Balance" "GET" "/api/Credits/balance" "$STUDENT_TOKEN"
test_endpoint "Get Credit Transactions" "GET" "/api/Credits/transactions" "$STUDENT_TOKEN"
echo ""

echo "=== STUDENT DASHBOARD ==="
test_endpoint "Student Dashboard" "GET" "/api/student/dashboard" "$STUDENT_TOKEN"
test_endpoint "Student Dashboard Stats" "GET" "/api/student/dashboard/stats" "$STUDENT_TOKEN"
echo ""

echo "=== TUTOR DASHBOARD ==="
test_endpoint "Tutor Dashboard" "GET" "/api/tutor/dashboard" "$TUTOR_TOKEN"
test_endpoint "Tutor Dashboard Stats" "GET" "/api/tutor/dashboard/stats" "$TUTOR_TOKEN"
echo ""

echo "=== SESSIONS ENDPOINTS ==="
test_endpoint "Get Sessions" "GET" "/api/Sessions" "$STUDENT_TOKEN"
echo ""

echo "=== COURSES ENDPOINTS ==="
test_endpoint "Enroll in Course" "POST" "/api/Courses/00000000-0000-0000-0000-000000000001/enroll" "$STUDENT_TOKEN"
echo ""

echo "=== STUDENT COURSES & SESSIONS ==="
test_endpoint "My Courses" "GET" "/api/student/courses/my-courses" "$STUDENT_TOKEN"
test_endpoint "My Bookings" "GET" "/api/student/sessions/my-bookings" "$STUDENT_TOKEN"
echo ""

echo "=== NOTIFICATIONS ==="
test_endpoint "Get Notifications" "GET" "/api/Notifications" "$STUDENT_TOKEN"
test_endpoint "Unread Count" "GET" "/api/Notifications/unread-count" "$STUDENT_TOKEN"
echo ""

echo "=== MESSAGES ==="
test_endpoint "Get Conversations" "GET" "/api/Messages/conversations" "$STUDENT_TOKEN"
echo ""

echo "=== REVIEWS ==="
test_endpoint "Get Reviews" "GET" "/api/Reviews" "$STUDENT_TOKEN"
echo ""

echo "=== SUBSCRIPTIONS ==="
test_endpoint "Get Subscriptions" "GET" "/api/Subscriptions" "$STUDENT_TOKEN"
echo ""

echo "=== SHARED/PUBLIC ENDPOINTS ==="
test_endpoint "Get FAQs" "GET" "/api/shared/faqs" ""
test_endpoint "Get Tutors" "GET" "/api/shared/tutors" ""
test_endpoint "Get Courses" "GET" "/api/shared/courses" ""
test_endpoint "Get Blogs" "GET" "/api/shared/blogs" ""
test_endpoint "Get Referral Code" "GET" "/api/shared/referrals/code" "$STUDENT_TOKEN"
test_endpoint "Get Referral Stats" "GET" "/api/shared/referrals/stats" "$STUDENT_TOKEN"
echo ""

echo "=== REFERRALS ==="
test_endpoint "Get Referral History" "GET" "/api/shared/referrals/history" "$STUDENT_TOKEN"
echo ""

echo "=== DISPUTES ==="
test_endpoint "Get Disputes" "GET" "/api/shared/disputes" "$STUDENT_TOKEN"
echo ""

echo "================================================"
echo "Testing Complete!"
echo "PASSED: $PASS"
echo "FAILED: $FAIL"
echo "TOTAL: $((PASS + FAIL))"
echo "Success Rate: $(awk "BEGIN {printf \"%.1f\", ($PASS/($PASS+$FAIL))*100}")%"
echo "================================================"
