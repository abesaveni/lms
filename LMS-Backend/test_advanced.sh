#!/bin/bash

# Advanced API Testing - Complex Workflows
BASE_URL="https://localhost:7001"

echo "=============================================="
echo "LiveExpert.AI - Advanced Workflow Testing"
echo "=============================================="
echo ""

# Setup: Get tokens
echo "Setting up test accounts..."
STUDENT_TOKEN=$(curl -k -s -X POST "$BASE_URL/api/student/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "student_final@test.com", "password": "Test@12345"}' | jq -r '.data.accessToken')

TUTOR_TOKEN=$(curl -k -s -X POST "$BASE_URL/api/tutor/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "tutor_auto@test.com", "password": "Test@12345"}' | jq -r '.data.accessToken')

echo "✅ Tokens acquired"
echo ""

# Test 1: Get Credit Balance
echo "1. Testing Get Credit Balance..."
CREDITS=$(curl -k -s -X GET "$BASE_URL/api/student/credits/balance" \
  -H "Authorization: Bearer $STUDENT_TOKEN")
SUCCESS=$(echo "$CREDITS" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  BALANCE=$(echo "$CREDITS" | jq -r '.data.availableCredits' 2>/dev/null)
  echo "✅ Get Credit Balance: PASSED (Balance: $BALANCE credits)"
else
  echo "❌ Get Credit Balance: FAILED"
  echo "$CREDITS"
fi
echo ""

# Test 2: Get Tutor Profile
echo "2. Testing Get Tutor Profile..."
TUTOR_PROFILE=$(curl -k -s -X GET "$BASE_URL/api/tutor/profile" \
  -H "Authorization: Bearer $TUTOR_TOKEN")
SUCCESS=$(echo "$TUTOR_PROFILE" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  echo "✅ Get Tutor Profile: PASSED"
else
  echo "❌ Get Tutor Profile: FAILED"
  echo "$TUTOR_PROFILE"
fi
echo ""

# Test 3: Update Tutor Profile
echo "3. Testing Update Tutor Profile..."
UPDATE_PROFILE=$(curl -k -s -X PUT "$BASE_URL/api/tutor/profile" \
  -H "Authorization: Bearer $TUTOR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "bio": "Experienced tutor with 5+ years of teaching",
    "hourlyRate": 50.00,
    "subjects": ["Mathematics", "Physics"],
    "languages": ["English", "Hindi"]
  }')
SUCCESS=$(echo "$UPDATE_PROFILE" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  echo "✅ Update Tutor Profile: PASSED"
else
  echo "❌ Update Tutor Profile: FAILED"
  echo "$UPDATE_PROFILE"
fi
echo ""

# Test 4: Create Session (Tutor)
echo "4. Testing Create Session..."
CREATE_SESSION=$(curl -k -s -X POST "$BASE_URL/api/tutor/sessions" \
  -H "Authorization: Bearer $TUTOR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Mathematics Basics",
    "description": "Learn fundamental mathematics concepts",
    "subjectId": "00000000-0000-0000-0000-000000000001",
    "scheduledAt": "2025-01-15T10:00:00Z",
    "duration": 60,
    "maxStudents": 10,
    "creditsRequired": 10
  }')
SUCCESS=$(echo "$CREATE_SESSION" | jq -r '.success' 2>/dev/null)
SESSION_ID=$(echo "$CREATE_SESSION" | jq -r '.data.id' 2>/dev/null)
if [ "$SUCCESS" = "true" ] && [ "$SESSION_ID" != "null" ]; then
  echo "✅ Create Session: PASSED (Session ID: $SESSION_ID)"
else
  echo "❌ Create Session: FAILED"
  echo "$CREATE_SESSION"
fi
echo ""

# Test 5: Get Available Sessions
echo "5. Testing Get Available Sessions..."
SESSIONS=$(curl -k -s -X GET "$BASE_URL/api/student/sessions/available" \
  -H "Authorization: Bearer $STUDENT_TOKEN")
SUCCESS=$(echo "$SESSIONS" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  COUNT=$(echo "$SESSIONS" | jq -r '.data | length' 2>/dev/null)
  echo "✅ Get Available Sessions: PASSED ($COUNT sessions found)"
else
  echo "❌ Get Available Sessions: FAILED"
  echo "$SESSIONS"
fi
echo ""

# Test 6: Book Session (if session was created)
if [ ! -z "$SESSION_ID" ] && [ "$SESSION_ID" != "null" ]; then
  echo "6. Testing Book Session..."
  BOOK_SESSION=$(curl -k -s -X POST "$BASE_URL/api/student/sessions/$SESSION_ID/book" \
    -H "Authorization: Bearer $STUDENT_TOKEN")
  SUCCESS=$(echo "$BOOK_SESSION" | jq -r '.success' 2>/dev/null)
  if [ "$SUCCESS" = "true" ]; then
    echo "✅ Book Session: PASSED"
  else
    echo "❌ Book Session: FAILED"
    echo "$BOOK_SESSION"
  fi
else
  echo "6. Skipping Book Session (no session ID)"
fi
echo ""

# Test 7: Get Student Sessions
echo "7. Testing Get Student Sessions..."
STUDENT_SESSIONS=$(curl -k -s -X GET "$BASE_URL/api/student/sessions" \
  -H "Authorization: Bearer $STUDENT_TOKEN")
SUCCESS=$(echo "$STUDENT_SESSIONS" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  COUNT=$(echo "$STUDENT_SESSIONS" | jq -r '.data | length' 2>/dev/null)
  echo "✅ Get Student Sessions: PASSED ($COUNT sessions)"
else
  echo "❌ Get Student Sessions: FAILED"
  echo "$STUDENT_SESSIONS"
fi
echo ""

# Test 8: Get Tutor Sessions
echo "8. Testing Get Tutor Sessions..."
TUTOR_SESSIONS=$(curl -k -s -X GET "$BASE_URL/api/tutor/sessions" \
  -H "Authorization: Bearer $TUTOR_TOKEN")
SUCCESS=$(echo "$TUTOR_SESSIONS" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  COUNT=$(echo "$TUTOR_SESSIONS" | jq -r '.data | length' 2>/dev/null)
  echo "✅ Get Tutor Sessions: PASSED ($COUNT sessions)"
else
  echo "❌ Get Tutor Sessions: FAILED"
  echo "$TUTOR_SESSIONS"
fi
echo ""

# Test 9: Get Categories
echo "9. Testing Get Categories..."
CATEGORIES=$(curl -k -s -X GET "$BASE_URL/api/shared/categories")
SUCCESS=$(echo "$CATEGORIES" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  COUNT=$(echo "$CATEGORIES" | jq -r '.data | length' 2>/dev/null)
  echo "✅ Get Categories: PASSED ($COUNT categories)"
else
  echo "❌ Get Categories: FAILED"
  echo "$CATEGORIES"
fi
echo ""

# Test 10: Get Subjects
echo "10. Testing Get Subjects..."
SUBJECTS=$(curl -k -s -X GET "$BASE_URL/api/shared/subjects")
SUCCESS=$(echo "$SUBJECTS" | jq -r '.success' 2>/dev/null)
if [ "$SUCCESS" = "true" ]; then
  COUNT=$(echo "$SUBJECTS" | jq -r '.data | length' 2>/dev/null)
  echo "✅ Get Subjects: PASSED ($COUNT subjects)"
else
  echo "❌ Get Subjects: FAILED"
  echo "$SUBJECTS"
fi
echo ""

echo "=============================================="
echo "Advanced Testing Complete!"
echo "=============================================="
