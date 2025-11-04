@echo off
echo ========================================
echo  JurisIQ Frontend - Local Server
echo ========================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if %errorlevel% == 0 (
    echo Python found! Starting server...
    echo.
    echo Server will start on: http://localhost:8000
    echo.
    echo Press Ctrl+C to stop the server
    echo.
    echo Opening browser in 3 seconds...
    timeout /t 3 /nobreak >nul
    start http://localhost:8000/login.html
    python -m http.server 8000
    goto :end
)

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% == 0 (
    echo Node.js found! Starting server...
    echo.
    echo Server will start on: http://localhost:8000
    echo.
    echo Press Ctrl+C to stop the server
    echo.
    echo Opening browser in 3 seconds...
    timeout /t 3 /nobreak >nul
    start http://localhost:8000/login.html
    npx http-server -p 8000
    goto :end
)

REM Neither Python nor Node.js found
echo ========================================
echo  ERROR: No server found!
echo ========================================
echo.
echo Neither Python nor Node.js is installed on your system.
echo.
echo Please install one of the following:
echo.
echo Option 1: Python
echo   Download from: https://www.python.org/downloads/
echo.
echo Option 2: Node.js
echo   Download from: https://nodejs.org/
echo.
echo OR
echo.
echo Option 3: Use Visual Studio Code + Live Server (RECOMMENDED)
echo   See HOW_TO_RUN_LOCALLY.md for instructions
echo.
echo For now, you can double-click any HTML file to view it in your browser.
echo.
pause
goto :end

:end