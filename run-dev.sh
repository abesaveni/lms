#!/bin/bash

# LiveExpert.AI - Multi-environment Starter Script

# 🎨 Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

echo -e "${BLUE}🚀 Starting LiveExpert.AI Integrated Application...${NC}"

# Kill any existing processes
echo -e "${YELLOW}🛑 Stopping any existing processes...${NC}"
pkill -f "dotnet.*LiveExpert.API" 2>/dev/null
pkill -f "vite" 2>/dev/null
sleep 2

# 1. Start Backend
echo -e "${GREEN}📡 Starting Backend (.NET)...${NC}"
cd "$SCRIPT_DIR/LMS-Backend/src/LiveExpert.API"
dotnet run > /tmp/liveexpert-backend.log 2>&1 &
BACKEND_PID=$!
echo -e "${GREEN}   Backend PID: $BACKEND_PID${NC}"

# 2. Wait for backend to initialize
echo -e "${YELLOW}⏳ Waiting for backend to initialize...${NC}"
for i in {1..10}; do
    if curl -s http://localhost:5128/health > /dev/null 2>&1; then
        echo -e "${GREEN}   ✓ Backend is ready!${NC}"
        break
    fi
    sleep 1
    echo -e "${YELLOW}   Waiting... ($i/10)${NC}"
done

# 3. Start Frontend
echo -e "${GREEN}💻 Starting Frontend (Vite)...${NC}"
cd "$SCRIPT_DIR/LMS-Frontend"
npm run dev > /tmp/liveexpert-frontend.log 2>&1 &
FRONTEND_PID=$!
echo -e "${GREEN}   Frontend PID: $FRONTEND_PID${NC}"

# Wait a moment for frontend to start
sleep 3

echo -e "${BLUE}✅ Both applications are starting!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}📍 Backend:  http://localhost:5128${NC}"
echo -e "${BLUE}📍 Frontend: http://localhost:5173${NC}"
echo -e "${BLUE}📍 Health:   http://localhost:5128/health${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${YELLOW}📋 Logs:${NC}"
echo -e "   Backend:  tail -f /tmp/liveexpert-backend.log"
echo -e "   Frontend: tail -f /tmp/liveexpert-frontend.log"
echo -e "${YELLOW}Press Ctrl+C to stop both applications.${NC}"

# Handle exit
cleanup() {
    echo -e "\n${YELLOW}🛑 Stopping applications...${NC}"
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    pkill -f "dotnet.*LiveExpert.API" 2>/dev/null
    pkill -f "vite" 2>/dev/null
    echo -e "${GREEN}✅ Applications stopped.${NC}"
    exit 0
}

trap cleanup INT TERM

# Wait for processes
wait
