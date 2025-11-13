@echo off
echo ========================================
echo JuriIQ Backend System - View Logs
echo ========================================
echo.
echo Choose a service to view logs:
echo   1. All services
echo   2. Backend API
echo   3. Scheduler
echo   4. Frontend
echo   5. PostgreSQL
echo.

set /p CHOICE="Enter your choice (1-5): "

if "%CHOICE%"=="1" (
    docker-compose logs -f
) else if "%CHOICE%"=="2" (
    docker-compose logs -f backend
) else if "%CHOICE%"=="3" (
    docker-compose logs -f scheduler
) else if "%CHOICE%"=="4" (
    docker-compose logs -f frontend
) else if "%CHOICE%"=="5" (
    docker-compose logs -f postgres
) else (
    echo Invalid choice!
    pause
    exit /b 1
)
