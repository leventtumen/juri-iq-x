@echo off
echo ======================================
echo  Starting Juri-IQ System
echo ======================================
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo Docker is running...
echo.

REM Stop and remove existing containers
echo Cleaning up existing containers...
docker-compose down

echo.
echo Building and starting containers...
docker-compose up --build -d

echo.
echo Waiting for services to be ready...
timeout /t 30 /nobreak >nul

echo.
echo ======================================
echo  Juri-IQ System Started Successfully!
echo ======================================
echo.
echo Services:
echo   Frontend:  http://localhost:8001
echo   Backend API: http://localhost:5001
echo   Swagger UI: http://localhost:5001/swagger
echo   Database: localhost:5432
echo.
echo To view logs: docker-compose logs -f
echo To stop: docker-compose down
echo.

pause
