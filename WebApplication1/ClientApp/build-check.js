const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Check if the build directory exists
const buildDir = path.join(__dirname, 'build');
if (!fs.existsSync(buildDir)) {
  console.log('React app build directory not found. Building the React app...');
  
  try {
    // Install dependencies if node_modules doesn't exist
    if (!fs.existsSync(path.join(__dirname, 'node_modules'))) {
      console.log('Installing dependencies...');
      execSync('npm install', { stdio: 'inherit', cwd: __dirname });
    }
    
    // Build the React app
    console.log('Building the React app...');
    execSync('npm run build', { stdio: 'inherit', cwd: __dirname });
    
    console.log('React app built successfully!');
  } catch (error) {
    console.error('Error building the React app:', error.message);
    process.exit(1);
  }
} else {
  console.log('React app build directory found. Skipping build.');
}
