#!/bin/bash

# Navigate to the ClientApp directory
cd "$(dirname "$0")/ClientApp"

# Install dependencies
npm install

# Build the React app
npm run build

echo "React app built successfully!"
