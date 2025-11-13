# JuriIQ Backend System - PowerShell Startup Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "JuriIQ Backend System - Startup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "Docker is running..." -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "ERROR: Docker is not running!" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Stop any existing containers
Write-Host "Stopping existing containers..." -ForegroundColor Yellow
docker-compose down
Write-Host ""

# Build and start all services
Write-Host "Building and starting all services..." -ForegroundColor Yellow
Write-Host "This may take a few minutes on first run..." -ForegroundColor Yellow
Write-Host ""

docker-compose up --build -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "JuriIQ System Started Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Services:" -ForegroundColor Cyan
    Write-Host "  - Frontend: http://localhost:8001" -ForegroundColor White
    Write-Host "  - Backend API: http://localhost:5001" -ForegroundColor White
    Write-Host "  - API Documentation: http://localhost:5001/swagger" -ForegroundColor White
    Write-Host "  - PostgreSQL: localhost:5432" -ForegroundColor White
    Write-Host ""
    Write-Host "Default Admin Credentials:" -ForegroundColor Cyan
    Write-Host "  - Email: admin@test.com" -ForegroundColor White
    Write-Host "  - Password: Pass!2345" -ForegroundColor White
    Write-Host ""
    Write-Host "Useful Commands:" -ForegroundColor Cyan
    Write-Host "  - View logs: docker-compose logs -f" -ForegroundColor White
    Write-Host "  - Stop services: ./stop-juriiq.ps1" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "ERROR: Failed to start services!" -ForegroundColor Red
    Write-Host "Check the error messages above." -ForegroundColor Yellow
    Write-Host ""
}

Read-Host "Press Enter to exit"
