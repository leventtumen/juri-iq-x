# Juri-IQ System Startup Script
Write-Host "======================================" -ForegroundColor Cyan
Write-Host " Starting Juri-IQ System" -ForegroundColor Cyan
Write-Host "======================================`n" -ForegroundColor Cyan

# Check if Docker is running
try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Docker is not running"
    }
    Write-Host "Docker is running..." -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Docker is not running!" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Stop and remove existing containers
Write-Host "`nCleaning up existing containers..." -ForegroundColor Yellow
docker-compose down

# Build and start containers
Write-Host "`nBuilding and starting containers..." -ForegroundColor Yellow
docker-compose up --build -d

# Wait for services
Write-Host "`nWaiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Display success message
Write-Host "`n======================================" -ForegroundColor Green
Write-Host " Juri-IQ System Started Successfully!" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Green

Write-Host "Services:" -ForegroundColor Cyan
Write-Host "  Frontend:    http://localhost:8001" -ForegroundColor White
Write-Host "  Backend API: http://localhost:5001" -ForegroundColor White
Write-Host "  Swagger UI:  http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Database:    localhost:5432`n" -ForegroundColor White

Write-Host "Useful Commands:" -ForegroundColor Cyan
Write-Host "  View logs:   docker-compose logs -f" -ForegroundColor White
Write-Host "  Stop system: docker-compose down`n" -ForegroundColor White

Read-Host "Press Enter to exit"
