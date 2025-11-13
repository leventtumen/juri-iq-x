@echo off
echo ========================================
echo JuriIQ Backend System - Rebuild Script
echo ========================================
echo.

REM Check if Docker is running
docker info > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Docker is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo Docker is running...
echo.

REM Stop and remove existing containers
echo Stopping and removing existing containers...
docker-compose down
echo.

REM Remove old images (optional)
set /p REMOVE_IMAGES="Do you want to remove old images? (y/n): "
if /i "%REMOVE_IMAGES%"=="y" (
    echo Removing old images...
    docker-compose down --rmi all
    echo.
)

REM Build and start all services
echo Rebuilding and starting all services...
echo This will take a few minutes...
echo.
docker-compose up --build -d

if %ERRORLEVEL% EQ 0 (
    echo.
    echo ========================================
    echo JuriIQ System Rebuilt Successfully!
    echo ========================================
    echo.
    echo Services are now running.
    echo   - Frontend: http://localhost:8001
    echo   - Backend API: http://localhost:5001
    echo ========================================
    echo.
) else (
    echo.
    echo ERROR: Failed to rebuild services!
    echo.
)

pause
