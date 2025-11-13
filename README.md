# JuriIQ Backend System

**JuriIQ** is a comprehensive AI-powered legal case management and search platform built with .NET 8, PostgreSQL, and modern NLP technologies.

## Features

### Core Functionality
- **User Authentication & Authorization**: JWT-based secure authentication
- **Subscription Management**: Simple (1 device) and Pro (4 devices) subscription tiers
- **Device Tracking**: Multi-device login detection and management
- **Security Features**:
  - Failed login attempt tracking and temporary blocking
  - Account blacklisting for policy violations
  - Device limit enforcement

### Document Management
- **Automated Document Processing**: Quartz.NET scheduler for batch processing
- **Multi-Format Support**: PDF, DOCX, DOC, TXT files
- **AI-Powered Analysis**:
  - Automatic document summarization
  - Keyword extraction with relevance scoring
  - NLP-based similarity calculations

### Search & Discovery
- **Advanced Search**: Full-text search with PostgreSQL's Turkish language support
- **Relation Percentage**: NLP-based similarity scoring for search results
- **Filters**: Document type, court name, date range filtering
- **Related Documents**: AI-powered document relationship detection

### User Features
- **Bookmarking**: Save important documents with personal notes
- **Search History**: Track last 10 searches automatically
- **Dashboard**: Personalized user dashboard with recent activity

### Admin Panel
- **User Management**: View all users and their subscription status
- **Device Monitoring**: Track user devices and login activity
- **System Overview**: Monitor system health and usage

## Technology Stack

### Backend
- **.NET 8**: Modern C# web framework
- **ASP.NET Core Web API**: RESTful API implementation
- **Dapper**: Lightweight micro-ORM for database access
- **BCrypt.NET**: Secure password hashing
- **JWT**: Token-based authentication

### Database
- **PostgreSQL 16**: Relational database with full-text search
- **Turkish Language Support**: Native Turkish text search and analysis

### Scheduler
- **Quartz.NET**: Enterprise-grade job scheduling
- **Background Processing**: Automated document pipeline

### AI/NLP
- **PdfPig**: PDF text extraction
- **DocumentFormat.OpenXml**: Word document processing
- **Custom NLP**: Turkish language keyword extraction and similarity

### DevOps
- **Docker & Docker Compose**: Containerized deployment
- **Nginx**: Frontend web server with API proxying
- **Serilog**: Structured logging

## Project Structure

```
juri-iq-x/
├── src/
│   ├── JuriIQ.sln                    # Solution file
│   ├── JuriIQ.Api/                   # Web API project
│   ├── JuriIQ.Core/                  # Domain entities and interfaces
│   ├── JuriIQ.Infrastructure/        # Data access and services
│   ├── JuriIQ.Scheduler/             # Quartz.NET background processor
│   └── JuriIQ.AI/                    # AI/NLP document processing
├── webapp/                           # Frontend (HTML/CSS/JS)
├── database/                         # Database scripts
│   └── scripts/
│       ├── 01_init_schema.sql       # Database schema
│       └── 02_seed_data.sql         # Initial data
├── documents_to_process/             # Input folder for new documents
├── documents_done/                   # Successfully processed documents
├── documents_failed/                 # Failed document processing
├── docker-compose.yml                # Docker orchestration
├── start-juriiq.bat                 # Windows startup script
├── start-juriiq.ps1                 # PowerShell startup script
└── README.md                        # This file
```

## Quick Start

### Prerequisites
- **Docker Desktop** for Windows
- **Git** for cloning the repository
- **Windows 10/11** or **Windows Server 2019+**

### Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd juri-iq-x
   ```

2. **Start the system**:
   - Double-click `start-juriiq.bat`, or
   - Run `./start-juriiq.ps1` in PowerShell

3. **Access the application**:
   - Frontend: http://localhost:8001
   - API: http://localhost:5001
   - API Documentation: http://localhost:5001/swagger

4. **Default Admin Login**:
   - Email: `admin@test.com`
   - Password: `Pass!2345`

## Usage

### Adding Documents for Processing

1. Place documents (.pdf, .docx, .doc, .txt) in the `documents_to_process/` folder
2. Documents are processed automatically:
   - On system startup
   - Daily at 2:00 AM (configurable)
3. Processed documents move to `documents_done/`
4. Failed documents move to `documents_failed/`

### API Endpoints

See [SETUP.md](./SETUP.md) for detailed setup instructions.

Key endpoints:
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/documents/search` - Search documents
- `GET /api/documents/{id}` - Get document details
- `POST /api/documents/{id}/bookmark` - Bookmark document
- `GET /api/documents/bookmarks` - Get user bookmarks
- `GET /api/documents/history` - Get search history

### Management Scripts

- `start-juriiq.bat` - Start all services
- `stop-juriiq.bat` - Stop all services
- `rebuild-juriiq.bat` - Rebuild and restart
- `view-logs.bat` - View container logs

## Configuration

### Database Connection
Edit `src/JuriIQ.Api/appsettings.json` and `src/JuriIQ.Scheduler/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=postgres;Port=5432;Database=juriiq;Username=juriiq_user;Password=juriiq_pass"
}
```

### JWT Settings
```json
"JwtSettings": {
  "SecretKey": "YourSecretKey",
  "Issuer": "JuriIQ",
  "Audience": "JuriIQ",
  "ExpirationMinutes": 1440
}
```

### Document Processing Schedule
```json
"DocumentProcessing": {
  "ScheduleCron": "0 0 2 * * ?"  // 2 AM daily
}
```

## Development

### Building Locally

```bash
cd src
dotnet restore
dotnet build
```

### Running Without Docker

1. Install PostgreSQL locally
2. Update connection strings in appsettings.json
3. Run migrations: `psql -U postgres -d juriiq -f database/scripts/01_init_schema.sql`
4. Start API: `cd src/JuriIQ.Api && dotnet run`
5. Start Scheduler: `cd src/JuriIQ.Scheduler && dotnet run`
6. Serve frontend with any web server

## Troubleshooting

### Docker Issues
- Ensure Docker Desktop is running
- Check Docker has sufficient memory (4GB+ recommended)
- Run `docker-compose logs` to view errors

### Database Connection Issues
- Verify PostgreSQL container is healthy: `docker ps`
- Check logs: `docker-compose logs postgres`
- Ensure port 5432 is not in use by another service

### Document Processing Issues
- Check scheduler logs: `docker-compose logs scheduler`
- Verify file permissions on document folders
- Ensure documents are valid formats

## License

Copyright © 2025 JuriIQ. All rights reserved.

## Support

For issues and questions, please create an issue in the repository or contact the development team