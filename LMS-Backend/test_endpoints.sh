#!/bin/bash

# Comprehensive API Endpoint Testing Script
BASE_URL="https://localhost:7001"
STUDENT_TOKEN=""
TUTOR_TOKEN=""

echo "========================================="
echo "LiveExpert.AI API Comprehensive Testing"
echo "========================================="
echo ""

# Test 1: Student Registration
echo "1. Testing Student Registration..."
STUDENT_REG=$(curl -k -s -X POST "$BASE_URL/api/student/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_student_auto",
    "email": "student_auto@test.com",
    "password": "Test@12345",
    "phoneNumber": "+919876543298",
    "firstName": "Auto",
    "lastName": "Student"
  }')
echo "$STUDENT_REG" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Student Registration: PASSED"
else
  echo "❌ Student Registration: FAILED"
  echo "$STUDENT_REG"
fi
echo ""

# Test 2: Student Login
echo "2. Testing Student Login..."
STUDENT_LOGIN=$(curl -k -s -X POST "$BASE_URL/api/student/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student_final@test.com",
    "password": "Test@12345"
  }')
STUDENT_TOKEN=$(echo "$STUDENT_LOGIN" | jq -r '.data.accessToken' 2>/dev/null)
if [ ! -z "$STUDENT_TOKEN" ] && [ "$STUDENT_TOKEN" != "null" ]; then
  echo "✅ Student Login: PASSED"
  echo "   Token: ${STUDENT_TOKEN:0:50}..."
else
  echo "❌ Student Login: FAILED"
  echo "$STUDENT_LOGIN"
fi
echo ""

# Test 3: Student Dashboard
echo "3. Testing Student Dashboard..."
DASHBOARD=$(curl -k -s -X GET "$BASE_URL/api/student/dashboard" \
  -H "Authorization: Bearer $STUDENT_TOKEN")
echo "$DASHBOARD" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Student Dashboard: PASSED"
else
  echo "❌ Student Dashboard: FAILED"
  echo "$DASHBOARD"
fi
echo ""

# Test 4: Student Profile
echo "4. Testing Student Profile..."
PROFILE=$(curl -k -s -X GET "$BASE_URL/api/student/profile" \
  -H "Authorization: Bearer $STUDENT_TOKEN")
echo "$PROFILE" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Student Profile: PASSED"
else
  echo "❌ Student Profile: FAILED"
  echo "$PROFILE"
fi
echo ""

# Test 5: Tutor Registration
echo "5. Testing Tutor Registration..."
TUTOR_REG=$(curl -k -s -X POST "$BASE_URL/api/tutor/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test_tutor_auto",
    "email": "tutor_auto@test.com",
    "password": "Test@12345",
    "phoneNumber": "+919876543297",
    "firstName": "Auto",
    "lastName": "Tutor"
  }')
echo "$TUTOR_REG" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Tutor Registration: PASSED"
else
  echo "❌ Tutor Registration: FAILED"
  echo "$TUTOR_REG"
fi
echo ""

# Test 6: Tutor Login
echo "6. Testing Tutor Login..."
TUTOR_LOGIN=$(curl -k -s -X POST "$BASE_URL/api/tutor/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "tutor001@test.com",
    "password": "Test@12345"
  }')
TUTOR_TOKEN=$(echo "$TUTOR_LOGIN" | jq -r '.data.accessToken' 2>/dev/null)
if [ ! -z "$TUTOR_TOKEN" ] && [ "$TUTOR_TOKEN" != "null" ]; then
  echo "✅ Tutor Login: PASSED"
  echo "   Token: ${TUTOR_TOKEN:0:50}..."
else
  echo "❌ Tutor Login: FAILED"
  echo "$TUTOR_LOGIN"
fi
echo ""

# Test 7: Tutor Dashboard
echo "7. Testing Tutor Dashboard..."
TUTOR_DASH=$(curl -k -s -X GET "$BASE_URL/api/tutor/dashboard" \
  -H "Authorization: Bearer $TUTOR_TOKEN")
echo "$TUTOR_DASH" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Tutor Dashboard: PASSED"
else
  echo "❌ Tutor Dashboard: FAILED"
  echo "$TUTOR_DASH"
fi
echo ""

# Test 8: Get Tutors (Public)
echo "8. Testing Get Tutors List..."
TUTORS=$(curl -k -s -X GET "$BASE_URL/api/shared/tutors")
echo "$TUTORS" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Get Tutors: PASSED"
else
  echo "❌ Get Tutors: FAILED"
  echo "$TUTORS"
fi
echo ""

# Test 9: Get Courses (Public)
echo "9. Testing Get Courses List..."
COURSES=$(curl -k -s -X GET "$BASE_URL/api/shared/courses")
echo "$COURSES" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Get Courses: PASSED"
else
  echo "❌ Get Courses: FAILED"
  echo "$COURSES"
fi
echo ""

# Test 10: Get FAQs (Public)
echo "10. Testing Get FAQs..."
FAQS=$(curl -k -s -X GET "$BASE_URL/api/shared/faqs")
echo "$FAQS" | jq -r '.success' > /dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "✅ Get FAQs: PASSED"
else
  echo "❌ Get FAQs: FAILED"
  echo "$FAQS"
fi
echo ""

# Test 11: Health Check
echo "11. Testing Health Check..."
HEALTH=$(curl -k -s -X GET "$BASE_URL/health")
if [ ! -z "$HEALTH" ]; then
  echo "✅ Health Check: PASSED"
else
  echo "❌ Health Check: FAILED"
fi
echo ""

echo "========================================="
echo "Testing Complete!"
echo "========================================="
