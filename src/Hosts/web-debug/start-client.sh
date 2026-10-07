#!/usr/bin/env bash
# Start the Angular Budget Client with watch mode on the host machine
# Prerequisites: Node.js and npm must be installed

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Starting Budget Client..."

# Navigate to client directory
cd "$SCRIPT_DIR/../../Controllers/NVs.Budget.Controllers.Web.Client/budget-client"

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    echo "node_modules not found. Installing dependencies..."
    npm install
    echo "Dependencies installed successfully!"
fi

# Set environment
export NODE_ENV=development

echo
echo "Environment Configuration:"
echo "  Development Server: http://localhost:4200"
echo "  Watch Mode: Enabled"
echo
echo "Starting Angular dev server..."
echo "----------------------------------------"
echo

# Start Angular dev server with watch
npm start
