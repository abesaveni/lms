#!/bin/bash

# Define colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting Clean and Rebuild process for LiveExpert Backend...${NC}"

# Navigate to Backend directory
cd "LMS-Backend" || { echo -e "${RED}LMS-Backend directory not found!${NC}"; exit 1; }

echo "Cleaning solution..."
dotnet clean

echo "Removing bin and obj directories explicitly..."
find . -type d -name "bin" -exec rm -rf {} +
find . -type d -name "obj" -exec rm -rf {} +

echo -e "${GREEN}Clean complete.${NC}"

echo "Restoring dependencies..."
dotnet restore

echo "Building project..."
dotnet build

echo -e "${GREEN}Build complete. You can now restart the backend server.${NC}"
