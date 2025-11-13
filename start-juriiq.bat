@echo off
echo ========================================
echo JuriIQ Backend System - Startup Script
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

REM Stop any existing containers
echo Stopping existing containers...
docker-compose down
echo.

REM Build and start all services
echo Building and starting all services...
echo This may take a few minutes on first run...
echo.
docker-compose up --build -d

if %ERRORLEVEL% EQ 0 (
    echo.
    echo ========================================
    echo JuriIQ System Started Successfully!
    echo ========================================
    echo.
    echo Services:
    echo   - Frontend: http://localhost:8001
    echo   - Backend API: http://localhost:5001
    echo   - API Documentation: http://localhost:5001/swagger
    echo   - PostgreSQL: localhost:5432
    echo.
    echo Default Admin Credentials:
    echo   - Email: admin@test.com
    echo   - Password: Pass!2345
    echo.
    echo To view logs, run: docker-compose logs -f
    echo To stop services, run: stop-juriiq.bat
    echo ========================================
    echo.
) else (
    echo.
    echo ERROR: Failed to start services!
    echo Check the error messages above.
    echo.
)

pause
