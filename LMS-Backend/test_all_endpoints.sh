#!/bin/bash

# COMPREHENSIVE API TESTING - ALL 119 ENDPOINTS
BASE_URL="https://localhost:7001"

echo "=========================================================="
echo "LiveExpert.AI - COMPLETE API TESTING (All 119 Endpoints)"
echo "=========================================================="
echo ""

PASS=0
FAIL=0
TOTAL=0

# Get authentication tokens
echo "🔐 Authenticating..."
STUDENT_TOKEN=$(curl -k -s -X POST "$BASE_URL/api/student/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "student_final@test.com", "password": "Test@12345"}' | jq -r '.data.accessToken' 2>/dev/null)

TUTOR_TOKEN=$(curl -k -s -X POST "$BASE_URL/api/tutor/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "tutor_auto@test.com", "password": "Test@12345"}' | jq -r '.data.accessToken' 2>/dev/null)

if [ -z "$STUDENT_TOKEN" ] || [ "$STUDENT_TOKEN" = "null" ]; then
  echo "❌ Failed to get student token"
  exit 1
fi

if [ -z "$TUTOR_TOKEN" ] || [ "$TUTOR_TOKEN" = "null" ]; then
  echo "❌ Failed to get tutor token"
  exit 1
fi

echo "✅ Tokens acquired"
echo ""

# Test function
test_endpoint() {
  local name="$1"
  local method="$2"
  local route="$3"
  local token="$4"
  local data="$5"
  local expected_status="${6:-200}"
  
  ((TOTAL++))
  echo -n "[$TOTAL] Testing: $name ... "
  
  if [ "$method" = "GET" ]; then
    if [ -z "$token" ]; then
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X GET "$BASE_URL$route")
    else
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X GET "$BASE_URL$route" -H "Authorization: Bearer $token")
    fi
  elif [ "$method" = "POST" ]; then
    if [ -z "$token" ]; then
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X POST "$BASE_URL$route" -H "Content-Type: application/json" -d "$data")
    else
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X POST "$BASE_URL$route" -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d "$data")
    fi
  elif [ "$method" = "PUT" ]; then
    if [ -z "$token" ]; then
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X PUT "$BASE_URL$route" -H "Content-Type: application/json" -d "$data")
    else
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X PUT "$BASE_URL$route" -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d "$data")
    fi
  elif [ "$method" = "DELETE" ]; then
    if [ -z "$token" ]; then
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X DELETE "$BASE_URL$route")
    else
      RESPONSE=$(curl -k -s -w "\n%{http_code}" -X DELETE "$BASE_URL$route" -H "Authorization: Bearer $token")
    fi
  fi
  
  STATUS=$(echo "$RESPONSE" | tail -n1)
  BODY=$(echo "$RESPONSE" | head -n-1)
  
  # Check if status matches expected or is a success code
  if [ "$STATUS" = "$expected_status" ] || [ "$STATUS" = "200" ] || [ "$STATUS" = "201" ] || [ "$STATUS" = "204" ]; then
    echo "✅ PASS (HTTP $STATUS)"
    ((PASS++))
  else
    echo "❌ FAIL (HTTP $STATUS)"
    ((FAIL++))
  fi
}

echo "=========================================================="
echo "CATEGORY 1: ADDITIONAL FEATURES (5 endpoints)"
echo "=========================================================="
test_endpoint "Get FAQs" "GET" "/api/shared/faqs" ""
test_endpoint "Create Support Ticket" "POST" "/api/shared/support/ticket" "$STUDENT_TOKEN" '{"subject":"Test","message":"Test message"}'
test_endpoint "Get Support Tickets" "GET" "/api/shared/support/tickets" "$STUDENT_TOKEN"
test_endpoint "Get Contact Subjects" "GET" "/api/ContactMessages/subjects" ""
test_endpoint "Create Contact Message" "POST" "/api/ContactMessages" "" '{"name":"Test","email":"test@test.com","subject":"Test","message":"Test"}'
echo ""

echo "=========================================================="
echo "CATEGORY 2: ADMIN (9 endpoints)"
echo "=========================================================="
test_endpoint "Get All Users" "GET" "/api/Admin/users" "$STUDENT_TOKEN"
test_endpoint "Get Pending Tutors" "GET" "/api/Admin/tutors/pending" "$STUDENT_TOKEN"
test_endpoint "Approve Tutor" "PUT" "/api/Admin/tutors/00000000-0000-0000-0000-000000000001/approve" "$STUDENT_TOKEN"
test_endpoint "Reject Tutor" "PUT" "/api/Admin/tutors/00000000-0000-0000-0000-000000000001/reject" "$STUDENT_TOKEN"
test_endpoint "Deactivate User" "PUT" "/api/Admin/users/00000000-0000-0000-0000-000000000001/deactivate" "$STUDENT_TOKEN"
test_endpoint "Activate User" "PUT" "/api/Admin/users/00000000-0000-0000-0000-000000000001/activate" "$STUDENT_TOKEN"
test_endpoint "Get Admin Statistics" "GET" "/api/Admin/statistics" "$STUDENT_TOKEN"
test_endpoint "Get Admin Sessions" "GET" "/api/Admin/sessions" "$STUDENT_TOKEN"
test_endpoint "Get Admin Payments" "GET" "/api/Admin/payments" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 3: ADMIN AUTH (5 endpoints)"
echo "=========================================================="
test_endpoint "Admin Login" "POST" "/api/admin/login" "" '{"email":"admin@test.com","password":"Admin@123"}'
test_endpoint "Create Admin User" "POST" "/api/admin/users/create-admin" "$STUDENT_TOKEN" '{"username":"admin2","email":"admin2@test.com","password":"Admin@123"}'
test_endpoint "Admin Forgot Password" "POST" "/api/admin/forgot-password" "" '{"email":"admin@test.com"}'
test_endpoint "Admin Reset Password" "POST" "/api/admin/reset-password" "" '{"token":"test","newPassword":"NewPass@123"}'
test_endpoint "Admin Refresh Token" "POST" "/api/admin/refresh-token" "" '{"refreshToken":"test"}'
echo ""

echo "=========================================================="
echo "CATEGORY 4: ADMIN DASHBOARD (1 endpoint)"
echo "=========================================================="
test_endpoint "Admin Dashboard" "GET" "/api/admin/dashboard" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 5: ADMIN ENHANCEMENTS (5 endpoints)"
echo "=========================================================="
test_endpoint "Get Audit Logs" "GET" "/api/admin/audit-logs" "$STUDENT_TOKEN"
test_endpoint "Get Pending KYC" "GET" "/api/admin/kyc/pending" "$STUDENT_TOKEN"
test_endpoint "Approve KYC" "PUT" "/api/admin/kyc/00000000-0000-0000-0000-000000000001/approve" "$STUDENT_TOKEN"
test_endpoint "Reject KYC" "PUT" "/api/admin/kyc/00000000-0000-0000-0000-000000000001/reject" "$STUDENT_TOKEN"
test_endpoint "Get Monthly Reports" "GET" "/api/admin/reports/monthly" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 6: BANK ACCOUNTS (3 endpoints)"
echo "=========================================================="
test_endpoint "Create Bank Account" "POST" "/api/bank-accounts" "$TUTOR_TOKEN" '{"accountNumber":"123456","ifscCode":"TEST0001","accountHolderName":"Test"}'
test_endpoint "Get Bank Accounts" "GET" "/api/bank-accounts" "$TUTOR_TOKEN"
test_endpoint "Delete Bank Account" "DELETE" "/api/bank-accounts/00000000-0000-0000-0000-000000000001" "$TUTOR_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 7: BLOGS (5 endpoints)"
echo "=========================================================="
test_endpoint "Get Blogs" "GET" "/api/shared/blogs" ""
test_endpoint "Get Blog by ID" "GET" "/api/shared/blogs/00000000-0000-0000-0000-000000000001" ""
test_endpoint "Create Blog" "POST" "/api/admin/blogs" "$STUDENT_TOKEN" '{"title":"Test Blog","content":"Test content"}'
test_endpoint "Update Blog" "PUT" "/api/admin/blogs/00000000-0000-0000-0000-000000000001" "$STUDENT_TOKEN" '{"title":"Updated","content":"Updated"}'
test_endpoint "Delete Blog" "DELETE" "/api/admin/blogs/00000000-0000-0000-0000-000000000001" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 8: CALENDAR (3 endpoints)"
echo "=========================================================="
test_endpoint "Connect Calendar" "POST" "/api/tutor/calendar/connect" "$TUTOR_TOKEN" '{"provider":"google","accessToken":"test"}'
test_endpoint "Get Calendar Status" "GET" "/api/tutor/calendar/status" "$TUTOR_TOKEN"
test_endpoint "Disconnect Calendar" "POST" "/api/tutor/calendar/disconnect" "$TUTOR_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 9: CAMPAIGNS (3 endpoints)"
echo "=========================================================="
test_endpoint "Create WhatsApp Campaign" "POST" "/api/admin/campaigns/whatsapp" "$STUDENT_TOKEN" '{"name":"Test","message":"Test"}'
test_endpoint "Get Campaigns" "GET" "/api/admin/campaigns" "$STUDENT_TOKEN"
test_endpoint "Get Campaign Stats" "GET" "/api/admin/campaigns/00000000-0000-0000-0000-000000000001/stats" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 10: COURSES (2 endpoints)"
echo "=========================================================="
test_endpoint "Create Course" "POST" "/api/Courses" "$TUTOR_TOKEN" '{"title":"Test Course","description":"Test"}'
test_endpoint "Enroll in Course" "POST" "/api/Courses/00000000-0000-0000-0000-000000000001/enroll" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 11: CREDITS (4 endpoints)"
echo "=========================================================="
test_endpoint "Get Credit Balance" "GET" "/api/Credits/balance" "$STUDENT_TOKEN"
test_endpoint "Purchase Credits" "POST" "/api/Credits/purchase" "$STUDENT_TOKEN" '{"amount":100,"creditsAmount":10}'
test_endpoint "Verify Purchase" "POST" "/api/Credits/verify-purchase" "$STUDENT_TOKEN" '{"orderId":"test","paymentId":"test","signature":"test"}'
test_endpoint "Get Transactions" "GET" "/api/Credits/transactions" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 12: DISPUTES (3 endpoints)"
echo "=========================================================="
test_endpoint "Create Dispute" "POST" "/api/shared/disputes" "$STUDENT_TOKEN" '{"sessionId":"00000000-0000-0000-0000-000000000001","reason":"Test"}'
test_endpoint "Get Disputes" "GET" "/api/shared/disputes" "$STUDENT_TOKEN"
test_endpoint "Respond to Dispute" "POST" "/api/shared/disputes/00000000-0000-0000-0000-000000000001/respond" "$STUDENT_TOKEN" '{"response":"Test response"}'
echo ""

echo "=========================================================="
echo "CATEGORY 13: MESSAGES (5 endpoints)"
echo "=========================================================="
test_endpoint "Get Conversations" "GET" "/api/Messages/conversations" "$STUDENT_TOKEN"
test_endpoint "Create Conversation" "POST" "/api/Messages/conversations" "$STUDENT_TOKEN" '{"participantId":"00000000-0000-0000-0000-000000000001"}'
test_endpoint "Get Messages" "GET" "/api/Messages/conversations/00000000-0000-0000-0000-000000000001/messages" "$STUDENT_TOKEN"
test_endpoint "Send Message" "POST" "/api/Messages/conversations/00000000-0000-0000-0000-000000000001/messages" "$STUDENT_TOKEN" '{"content":"Test message"}'
test_endpoint "Mark as Read" "PUT" "/api/Messages/messages/00000000-0000-0000-0000-000000000001/read" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 14: NOTIFICATIONS (5 endpoints)"
echo "=========================================================="
test_endpoint "Get Notifications" "GET" "/api/Notifications" "$STUDENT_TOKEN"
test_endpoint "Get Unread Count" "GET" "/api/Notifications/unread-count" "$STUDENT_TOKEN"
test_endpoint "Mark Notification Read" "PUT" "/api/Notifications/00000000-0000-0000-0000-000000000001/read" "$STUDENT_TOKEN"
test_endpoint "Mark All Read" "PUT" "/api/Notifications/read-all" "$STUDENT_TOKEN"
test_endpoint "Delete Notification" "DELETE" "/api/Notifications/00000000-0000-0000-0000-000000000001" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 15: PROFILES (2 endpoints)"
echo "=========================================================="
test_endpoint "Get Student Profile" "GET" "/api/shared/students/00000000-0000-0000-0000-000000000001/profile" ""
test_endpoint "Complete Tutor Profile" "POST" "/api/shared/tutor/profile/complete" "$TUTOR_TOKEN" '{"bio":"Test","hourlyRate":50}'
echo ""

echo "=========================================================="
echo "CATEGORY 16: REFERRALS (3 endpoints)"
echo "=========================================================="
test_endpoint "Get Referral Code" "GET" "/api/shared/referrals/code" "$STUDENT_TOKEN"
test_endpoint "Get Referral Stats" "GET" "/api/shared/referrals/stats" "$STUDENT_TOKEN"
test_endpoint "Get Referral History" "GET" "/api/shared/referrals/history" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 17: REVIEWS (3 endpoints)"
echo "=========================================================="
test_endpoint "Create Review" "POST" "/api/Reviews" "$STUDENT_TOKEN" '{"sessionId":"00000000-0000-0000-0000-000000000001","rating":5,"comment":"Great!"}'
test_endpoint "Get Reviews" "GET" "/api/Reviews" "$STUDENT_TOKEN"
test_endpoint "Respond to Review" "POST" "/api/Reviews/00000000-0000-0000-0000-000000000001/respond" "$TUTOR_TOKEN" '{"response":"Thank you!"}'
echo ""

echo "=========================================================="
echo "CATEGORY 18: SESSIONS (9 endpoints)"
echo "=========================================================="
test_endpoint "Create Session" "POST" "/api/Sessions" "$TUTOR_TOKEN" '{"title":"Test Session","scheduledAt":"2025-02-01T10:00:00Z","duration":60}'
test_endpoint "Get Sessions" "GET" "/api/Sessions" "$STUDENT_TOKEN"
test_endpoint "Get Session by ID" "GET" "/api/Sessions/00000000-0000-0000-0000-000000000001" "$STUDENT_TOKEN"
test_endpoint "Update Session" "PUT" "/api/Sessions/00000000-0000-0000-0000-000000000001" "$TUTOR_TOKEN" '{"title":"Updated"}'
test_endpoint "Delete Session" "DELETE" "/api/Sessions/00000000-0000-0000-0000-000000000001" "$TUTOR_TOKEN"
test_endpoint "Book Session" "POST" "/api/Sessions/00000000-0000-0000-0000-000000000001/book" "$STUDENT_TOKEN"
test_endpoint "Cancel Booking" "POST" "/api/Sessions/00000000-0000-0000-0000-000000000001/cancel-booking" "$STUDENT_TOKEN"
test_endpoint "Mark Attendance" "POST" "/api/Sessions/00000000-0000-0000-0000-000000000001/mark-attendance" "$TUTOR_TOKEN" '{"studentId":"00000000-0000-0000-0000-000000000001","present":true}'
test_endpoint "Get Meeting Link" "GET" "/api/Sessions/00000000-0000-0000-0000-000000000001/meeting-link" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 19: SETTINGS (4 endpoints)"
echo "=========================================================="
test_endpoint "Get Settings" "GET" "/api/admin/settings" "$STUDENT_TOKEN"
test_endpoint "Update Settings" "PUT" "/api/admin/settings" "$STUDENT_TOKEN" '{"key":"value"}'
test_endpoint "Get API Keys" "GET" "/api/admin/settings/api-keys" "$STUDENT_TOKEN"
test_endpoint "Create API Key" "POST" "/api/admin/settings/api-keys" "$STUDENT_TOKEN" '{"name":"Test Key"}'
echo ""

echo "=========================================================="
echo "CATEGORY 20: STUDENT AUTH (8 endpoints)"
echo "=========================================================="
test_endpoint "Student Register" "POST" "/api/student/register" "" '{"username":"newstudent","email":"new@test.com","password":"Test@123","phoneNumber":"+1234567890","firstName":"Test","lastName":"User"}'
test_endpoint "Verify Email" "POST" "/api/student/verify-email" "" '{"token":"test"}'
test_endpoint "Verify WhatsApp" "POST" "/api/student/verify-whatsapp" "" '{"phoneNumber":"+1234567890","otp":"123456"}'
test_endpoint "Forgot Password" "POST" "/api/student/forgot-password" "" '{"email":"student_final@test.com"}'
test_endpoint "Reset Password" "POST" "/api/student/reset-password" "" '{"token":"test","newPassword":"NewPass@123"}'
test_endpoint "Change Password" "POST" "/api/student/change-password" "$STUDENT_TOKEN" '{"currentPassword":"Test@12345","newPassword":"NewPass@123"}'
test_endpoint "Student Logout" "POST" "/api/student/logout" "$STUDENT_TOKEN"
test_endpoint "Refresh Token" "POST" "/api/student/refresh-token" "" '{"refreshToken":"test"}'
echo ""

echo "=========================================================="
echo "CATEGORY 21: STUDENT COURSES & SESSIONS (2 endpoints)"
echo "=========================================================="
test_endpoint "Get My Courses" "GET" "/api/student/courses/my-courses" "$STUDENT_TOKEN"
test_endpoint "Get My Bookings" "GET" "/api/student/sessions/my-bookings" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 22: STUDENT DASHBOARD (2 endpoints)"
echo "=========================================================="
test_endpoint "Student Dashboard" "GET" "/api/student/dashboard" "$STUDENT_TOKEN"
test_endpoint "Student Dashboard Stats" "GET" "/api/student/dashboard/stats" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 23: STUDENT QUIZZES (4 endpoints)"
echo "=========================================================="
test_endpoint "Start Quiz" "POST" "/api/student/quizzes/00000000-0000-0000-0000-000000000001/start" "$STUDENT_TOKEN"
test_endpoint "Submit Quiz" "POST" "/api/student/quizzes/attempts/00000000-0000-0000-0000-000000000001/submit" "$STUDENT_TOKEN" '{"answers":[]}'
test_endpoint "Get Quiz Attempts" "GET" "/api/student/quizzes/00000000-0000-0000-0000-000000000001/attempts" "$STUDENT_TOKEN"
test_endpoint "Get Quiz Results" "GET" "/api/student/quizzes/attempts/00000000-0000-0000-0000-000000000001/results" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 24: SUBSCRIPTIONS (4 endpoints)"
echo "=========================================================="
test_endpoint "Subscribe" "POST" "/api/Subscriptions/subscribe" "$STUDENT_TOKEN" '{"planId":"00000000-0000-0000-0000-000000000001"}'
test_endpoint "Verify Subscription" "POST" "/api/Subscriptions/verify" "$STUDENT_TOKEN" '{"orderId":"test","paymentId":"test","signature":"test"}'
test_endpoint "Get Subscriptions" "GET" "/api/Subscriptions" "$STUDENT_TOKEN"
test_endpoint "Cancel Subscription" "DELETE" "/api/Subscriptions" "$STUDENT_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 25: TUTOR AUTH (7 endpoints)"
echo "=========================================================="
test_endpoint "Tutor Register" "POST" "/api/tutor/register" "" '{"username":"newtutor","email":"newtutor@test.com","password":"Test@123","phoneNumber":"+1234567891","firstName":"Test","lastName":"Tutor"}'
test_endpoint "Tutor Verify Email" "POST" "/api/tutor/verify-email" "" '{"token":"test"}'
test_endpoint "Tutor Verify WhatsApp" "POST" "/api/tutor/verify-whatsapp" "" '{"phoneNumber":"+1234567891","otp":"123456"}'
test_endpoint "Tutor Forgot Password" "POST" "/api/tutor/forgot-password" "" '{"email":"tutor_auto@test.com"}'
test_endpoint "Tutor Change Password" "POST" "/api/tutor/change-password" "$TUTOR_TOKEN" '{"currentPassword":"Test@12345","newPassword":"NewPass@123"}'
test_endpoint "Tutor Logout" "POST" "/api/tutor/logout" "$TUTOR_TOKEN"
test_endpoint "Tutor Refresh Token" "POST" "/api/tutor/refresh-token" "" '{"refreshToken":"test"}'
echo ""

echo "=========================================================="
echo "CATEGORY 26: TUTOR COURSES (3 endpoints)"
echo "=========================================================="
test_endpoint "Publish Course" "PUT" "/api/tutor/courses/00000000-0000-0000-0000-000000000001/publish" "$TUTOR_TOKEN"
test_endpoint "Unpublish Course" "PUT" "/api/tutor/courses/00000000-0000-0000-0000-000000000001/unpublish" "$TUTOR_TOKEN"
test_endpoint "Get Course Students" "GET" "/api/tutor/courses/00000000-0000-0000-0000-000000000001/students" "$TUTOR_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 27: TUTOR DASHBOARD (2 endpoints)"
echo "=========================================================="
test_endpoint "Tutor Dashboard" "GET" "/api/tutor/dashboard" "$TUTOR_TOKEN"
test_endpoint "Tutor Dashboard Stats" "GET" "/api/tutor/dashboard/stats" "$TUTOR_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 28: TUTOR QUIZZES (4 endpoints)"
echo "=========================================================="
test_endpoint "Create Quiz" "POST" "/api/tutor/quizzes" "$TUTOR_TOKEN" '{"title":"Test Quiz","courseId":"00000000-0000-0000-0000-000000000001"}'
test_endpoint "Get Quiz" "GET" "/api/tutor/quizzes/00000000-0000-0000-0000-000000000001" "$TUTOR_TOKEN"
test_endpoint "Update Quiz" "PUT" "/api/tutor/quizzes/00000000-0000-0000-0000-000000000001" "$TUTOR_TOKEN" '{"title":"Updated Quiz"}'
test_endpoint "Delete Quiz" "DELETE" "/api/tutor/quizzes/00000000-0000-0000-0000-000000000001" "$TUTOR_TOKEN"
echo ""

echo "=========================================================="
echo "CATEGORY 29: PUBLIC ENDPOINTS (3 endpoints)"
echo "=========================================================="
test_endpoint "Get Public Tutors" "GET" "/api/shared/tutors" ""
test_endpoint "Get Public Courses" "GET" "/api/shared/courses" ""
test_endpoint "Health Check" "GET" "/health" ""
echo ""

echo "=========================================================="
echo "FINAL RESULTS"
echo "=========================================================="
echo "Total Endpoints Tested: $TOTAL"
echo "Passed: $PASS"
echo "Failed: $FAIL"
PERCENTAGE=$((PASS * 100 / TOTAL))
echo "Success Rate: $PERCENTAGE%"
echo "=========================================================="

if [ $FAIL -eq 0 ]; then
  echo "🎉 ALL TESTS PASSED! 100% SUCCESS!"
else
  echo "⚠️  Some tests failed. Review the output above."
fi
