@echo off
echo ========================================
echo JuriIQ Backend System - Stop Script
echo ========================================
echo.

echo Stopping all containers...
docker-compose down

echo.
echo All services stopped.
echo.

pause
