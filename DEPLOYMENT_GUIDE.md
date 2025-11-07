# Juri-IQ Backend System - Deployment Guide

## Overview
Juri-IQ is a comprehensive AI-driven document processing and search system with secure user authentication, subscription management, and automated document analysis.

## System Architecture

### Components
1. **PostgreSQL Database** - Primary data store
2. **Backend API** - ASP.NET Core 8.0 Web API (Port 5001)
3. **Quartz Scheduler** - Document processing service
4. **Frontend** - Static HTML/CSS/JS with Nginx (Port 8001)

### Technology Stack
- **.NET 8.0** - Backend framework
- **PostgreSQL 15** - Database
- **Dapper** - Micro-ORM
- **Quartz.NET** - Job scheduling
- **JWT** - Authentication
- **Docker** - Containerization
- **Nginx** - Frontend web server

## Prerequisites

### Windows Environment
- Docker Desktop for Windows (latest version)
- Git (for cloning repository)
- At least 4GB RAM available
- Ports 5001, 5432, and 8001 must be free

## Quick Start

### Option 1: Using Batch File (Recommended)
1. Navigate to the project root directory
2. Double-click `start-juriiq.bat`
3. Wait for all services to start (approximately 30 seconds)
4. Access the application at http://localhost:8001

### Option 2: Using PowerShell
1. Open PowerShell in the project root directory
2. Run: `.\start-juriiq.ps1`
3. Wait for services to initialize
4. Access the application at http://localhost:8001

### Option 3: Manual Docker Compose
```bash
# Navigate to project directory
cd /path/to/juri-iq

# Start all services
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Service URLs

| Service | URL | Description |
|---------|-----|-------------|
| Frontend | http://localhost:8001 | Main application UI |
| Backend API | http://localhost:5001 | REST API endpoints |
| Swagger UI | http://localhost:5001/swagger | API documentation |
| Database | localhost:5432 | PostgreSQL database |

## Default Credentials

### Admin Account
- **Email:** admin@test.com
- **Password:** Pass!2345

This account is automatically created during database initialization.

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Documents
- `GET /api/documents/{id}` - Get document by ID
- `GET /api/documents/search?query=...` - Search documents
- `GET /api/documents` - Get all documents

### Health Check
- `GET /health` - API health status

## Database Schema

The database is automatically initialized with the following tables:
- `users` - User accounts
- `user_devices` - Device tracking
- `login_attempts` - Security tracking
- `documents` - Document metadata
- `document_keywords` - Extracted keywords
- `document_statistics` - Document statistics
- `search_history` - User search history
- `user_bookmarks` - User bookmarks
- `document_views` - View analytics

## Document Processing

### Supported Formats
- PDF (.pdf)
- Microsoft Word (.doc, .docx)
- Word Template (.dot)
- Plain Text (.txt)

### Processing Workflow
1. Place documents in `documents_to_process/` folder
2. Scheduler runs every hour (or on startup)
3. Documents are processed and analyzed
4. Successfully processed files move to `documents_done/`
5. Failed files move to `documents_failed/`

### Initial Document Processing
On first startup, all documents in `documents_to_process/` are automatically processed.

## Subscription Types

### Simple Subscription
- Single device login
- Access to all documents
- Search and bookmark features

### Pro Subscription
- Up to 4 concurrent devices
- All Simple features
- Priority access

## Security Features

### Authentication
- JWT-based authentication
- BCrypt password hashing
- Automatic token expiration (24 hours)
- Secure password requirements

### Security Rules
1. **Login Attempt Tracking** - Failed login attempts are logged
2. **Device Limit Enforcement** - Based on subscription type
3. **Account Blocking** - Automatic blocking for multiple concurrent device violations
4. **Token Validation** - All protected endpoints require valid JWT

## Development

### Project Structure
```
webapp/
├── backend/
│   ├── JuriIQ.API/          # Web API project
│   ├── JuriIQ.Core/         # Domain models and interfaces
│   ├── JuriIQ.Infrastructure/ # Data access and services
│   ├── JuriIQ.AI/           # AI processing (future)
│   ├── JuriIQ.Scheduler/    # Quartz scheduler
│   ├── database_schema.sql  # Database initialization
│   ├── Dockerfile.api       # API container
│   └── Dockerfile.scheduler # Scheduler container
├── webapp/                   # Frontend files
│   ├── index.html
│   ├── css/
│   ├── js/
│   └── Dockerfile           # Frontend container
├── documents_to_process/    # Input folder
├── documents_done/          # Processed files
├── documents_failed/        # Failed files
├── docker-compose.yml       # Docker orchestration
├── start-juriiq.bat        # Windows batch startup
└── start-juriiq.ps1        # PowerShell startup
```

### Building Projects
```bash
# Build solution
cd backend
dotnet build

# Run API locally
cd JuriIQ.API
dotnet run

# Run Scheduler locally
cd JuriIQ.Scheduler
dotnet run
```

### Running Tests
```bash
cd backend
dotnet test
```

## Configuration

### Database Connection
Edit `docker-compose.yml` to modify database settings:
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres_db;Port=5432;Database=juriiq;Username=postgres;Password=yourpassword
```

### JWT Configuration
Edit `backend/JuriIQ.API/appsettings.json`:
```json
{
  "Jwt": {
    "Secret": "YourSecretKey",
    "Issuer": "JuriIQ",
    "ExpireMinutes": "1440"
  }
}
```

### Scheduler Configuration
Modify cron schedule in `JuriIQ.Scheduler/Program.cs`:
```csharp
.WithCronSchedule("0 0 * * * ?") // Every hour
```

## Troubleshooting

### Docker Issues
**Problem:** Docker is not running
**Solution:** Start Docker Desktop

**Problem:** Port already in use
**Solution:** 
```bash
docker-compose down
# Or change ports in docker-compose.yml
```

### Database Issues
**Problem:** Database connection failed
**Solution:** 
```bash
# Check if database container is running
docker ps

# View database logs
docker logs juriiq_postgres

# Restart database
docker-compose restart postgres_db
```

### API Issues
**Problem:** 401 Unauthorized responses
**Solution:** 
- Verify JWT token is valid
- Check token expiration
- Re-login to get new token

**Problem:** 500 Internal Server Error
**Solution:**
```bash
# View API logs
docker logs juriiq_backend

# Check database connection
docker exec -it juriiq_postgres psql -U postgres -d juriiq -c "SELECT 1;"
```

## Maintenance

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend_api
docker-compose logs -f scheduler
docker-compose logs -f frontend
docker-compose logs -f postgres_db
```

### Restart Services
```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart backend_api
```

### Database Backup
```bash
docker exec juriiq_postgres pg_dump -U postgres juriiq > backup.sql
```

### Database Restore
```bash
docker exec -i juriiq_postgres psql -U postgres juriiq < backup.sql
```

### Clean Rebuild
```bash
# Stop and remove all containers
docker-compose down -v

# Rebuild and start
docker-compose up --build -d
```

## Performance Optimization

### Database Indexes
All necessary indexes are created automatically. For custom queries, add indexes as needed.

### Document Processing
- Process documents in batches during off-peak hours
- Adjust scheduler frequency based on document volume
- Monitor `documents_failed/` for processing issues

## Security Best Practices

1. **Change Default Passwords**
   - Update database password in `docker-compose.yml`
   - Update JWT secret in `appsettings.json`

2. **Use HTTPS in Production**
   - Configure SSL certificates
   - Update frontend proxy settings

3. **Regular Backups**
   - Schedule automatic database backups
   - Store backups securely off-site

4. **Monitor Logs**
   - Review authentication logs regularly
   - Monitor for suspicious activity

5. **Update Dependencies**
   - Keep Docker images updated
   - Update .NET packages regularly

## Support

For issues or questions:
1. Check the logs: `docker-compose logs -f`
2. Review this documentation
3. Check API documentation: http://localhost:5001/swagger

## Version History

### Version 1.0.0 (Current)
- Initial release
- Core authentication system
- Document processing pipeline
- REST API endpoints
- Admin panel
- Docker containerization
