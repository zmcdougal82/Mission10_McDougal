@echo off
cd %~dp0ClientApp

echo Installing npm packages...
call npm install

echo Building React app...
call npm run build

echo React app built successfully!
