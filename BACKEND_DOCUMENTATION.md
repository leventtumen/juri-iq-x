# Juri-IQ Backend System Documentation

## Overview

The Juri-IQ Backend System is a secure, scalable Flask-based API that provides document processing, AI-powered search, user authentication, subscription management, and administrative functionality for the Juri-IQ legal document portal.

## Architecture

### Technology Stack
- **Backend Framework**: Flask 2.3.3
- **Database**: SQLAlchemy with SQLite (development) / PostgreSQL (production)
- **Authentication**: JWT (JSON Web Tokens)
- **AI/NLP**: spaCy, NLTK, scikit-learn
- **Document Processing**: PyPDF2, python-docx, python-magic
- **Task Scheduling**: APScheduler
- **Security**: bcrypt, Flask-Limiter

### Project Structure
```
backend/
├── app.py                 # Main application entry point
├── requirements.txt       # Python dependencies
├── config/
│   └── config.py         # Configuration settings
├── models/
│   ├── __init__.py
│   ├── database.py       # Database initialization
│   ├── user.py          # User model
│   ├── subscription.py  # Subscription model
│   ├── device.py        # Device model
│   ├── document.py      # Document models
│   ├── search_history.py
│   └── bookmark.py
├── routes/
│   ├── auth.py          # Authentication endpoints
│   ├── documents.py     # Document endpoints
│   ├── search.py        # Search endpoints
│   ├── bookmarks.py     # Bookmark endpoints
│   ├── admin.py         # Admin endpoints
│   └── profile.py       # User profile endpoints
└── services/
    ├── document_processor.py  # Document processing service
    └── scheduler.py            # Task scheduling service
```

## Installation & Setup

### Prerequisites
- Python 3.8 or higher
- Node.js 16 or higher (for frontend)
- pip (Python package manager)

### Windows Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/leventtumen/juri-iq-x.git
   cd juri-iq-x
   ```

2. **Run the startup script:**
   ```bash
   start-juri-iq.bat
   ```

   This script will:
   - Create the project directory at `C:\projects\project-juri-iq`
   - Set up Python virtual environment
   - Install all dependencies
   - Download required NLP models
   - Initialize the database
   - Start both backend (port 5000) and frontend (port 8000) services

### Manual Installation

1. **Set up virtual environment:**
   ```bash
   cd backend
   python -m venv venv
   venv\Scripts\activate
   ```

2. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   python -m spacy download en_core_web_sm
   ```

3. **Set environment variables (optional):**
   ```bash
   set SECRET_KEY=your-secret-key
   set JWT_SECRET_KEY=your-jwt-secret
   set DATABASE_URL=sqlite:///juris_iq.db
   ```

4. **Initialize database:**
   ```bash
   python -c "from app import create_app; app = create_app()"
   ```

5. **Start the server:**
   ```bash
   python app.py
   ```

## API Documentation

### Base URL
```
http://localhost:5000/api
```

### Authentication Endpoints

#### Login
```
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

#### Register
```
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "subscription_type": "simple" | "pro"
}
```

#### Logout
```
POST /auth/logout
Authorization: Bearer <token>
```

#### Get Current User
```
GET /auth/me
Authorization: Bearer <token>
```

### Document Endpoints

#### Get Documents
```
GET /documents/?page=1&per_page=20&file_type=pdf&processed=true
Authorization: Bearer <token>
```

#### Get Document Details
```
GET /documents/{document_id}
Authorization: Bearer <token>
```

#### Search Documents with NLP
```
POST /search/query
Authorization: Bearer <token>
Content-Type: application/json

{
  "query": "legal contract terms",
  "page": 1,
  "per_page": 20,
  "similarity_threshold": 0.1
}
```

#### Get Search History
```
GET /search/history
Authorization: Bearer <token>
```

### Bookmark Endpoints

#### Create Bookmark
```
POST /bookmarks/
Authorization: Bearer <token>
Content-Type: application/json

{
  "document_id": 1,
  "notes": "Important case about contract law"
}
```

#### Get Bookmarks
```
GET /bookmarks/?page=1&per_page=20
Authorization: Bearer <token>
```

#### Delete Bookmark
```
DELETE /bookmarks/{bookmark_id}
Authorization: Bearer <token>
```

### Profile Endpoints

#### Get Profile
```
GET /profile/
Authorization: Bearer <token>
```

#### Get Subscription Info
```
GET /profile/subscription
Authorization: Bearer <token>
```

#### Get Devices
```
GET /profile/devices
Authorization: Bearer <token>
```

### Admin Endpoints

#### Get Dashboard
```
GET /admin/dashboard
Authorization: Bearer <admin_token>
```

#### Get Users
```
GET /admin/users?page=1&per_page=20&is_active=true
Authorization: Bearer <admin_token>
```

#### Toggle User Status
```
POST /admin/users/{user_id}/toggle-status
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "action": "activate" | "deactivate" | "blacklist" | "unblacklist"
}
```

## Security Features

### Authentication & Authorization
- JWT-based authentication with 24-hour token expiration
- Device registration and tracking
- Subscription-based device limits
- Admin role-based access control

### Rate Limiting
- 200 requests per day per user
- 50 requests per hour per user
- Login attempt limiting (5 attempts, 5-minute timeout)

### Device Management
- Simple subscription: 1 device limit
- Pro subscription: 4 device limit
- Automatic blacklisting for exceeding device limits
- Device fingerprinting using User-Agent and IP

### Input Validation
- SQL injection prevention through SQLAlchemy ORM
- XSS protection through input sanitization
- File type validation for document uploads

## Document Processing

### Supported Formats
- PDF (.pdf)
- Microsoft Word (.doc, .docx)
- Word Template (.dot)
- Plain Text (.txt)

### AI Processing Features
- **Text Extraction**: Automatic text extraction from all supported formats
- **Summarization**: AI-powered document summarization
- **Keyword Extraction**: Automatic keyword and entity extraction
- **Similarity Scoring**: NLP-based document similarity calculation
- **Search Indexing**: Full-text search with relevance ranking

### Processing Pipeline
1. File type detection using magic numbers
2. Text extraction using format-specific parsers
3. AI analysis using spaCy NLP pipeline
4. Keyword and entity extraction
5. Summary generation
6. Database storage with full-text indexing

## Subscription Management

### Subscription Types
- **Simple**: 1 device, basic features
- **Pro**: 4 devices, advanced features

### Device Limits Enforcement
- Real-time device tracking
- Automatic logout for inactive devices
- Blacklist protection for subscription violations

## Administrative Features

### User Management
- View all users with subscription status
- Activate/deactivate user accounts
- Blacklist/unblacklist users
- Manage user subscriptions
- View user device history

### System Monitoring
- Real-time dashboard with system statistics
- Document processing metrics
- User activity tracking
- Device usage analytics

### Document Management
- Manual document processing trigger
- Processing statistics by file type
- System health monitoring

## Maintenance & Operations

### Daily Scheduled Tasks
- Document processing for new/updated files
- Device cleanup (inactive devices older than 30 days)
- Database optimization

### Service Management

#### Restart Services
```bash
# Stop running processes
# Close backend and frontend command windows

# Start services again
start-juri-iq.bat
```

#### Rebuild After Updates
```bash
# 1. Pull latest changes
git pull origin main

# 2. Update dependencies (if requirements.txt changed)
cd backend
venv\Scripts\activate
pip install -r requirements.txt

# 3. Restart services
start-juri-iq.bat
```

#### Database Migration
```bash
cd backend
venv\Scripts\activate
python -c "from app import create_app; app = create_app(); from models.database import db; db.create_all()"
```

### Troubleshooting

#### Common Issues

1. **Port Already in Use**
   ```bash
   # Find process using port
   netstat -ano | findstr :5000
   # Kill process
   taskkill /PID <PID> /F
   ```

2. **spaCy Model Not Found**
   ```bash
   python -m spacy download en_core_web_sm
   ```

3. **Database Connection Error**
   ```bash
   # Recreate database
   del backend\juris_iq.db
   python -c "from app import create_app; app = create_app()"
   ```

4. **Document Processing Fails**
   - Check file permissions
   - Verify antiword installation for .doc files
   - Check disk space

#### Logs
- Application logs: Console output
- Error logs: Check console for stack traces
- Database logs: SQLAlchemy debug mode (if enabled)

## Production Deployment

### Environment Variables
```bash
set SECRET_KEY=your-production-secret-key
set JWT_SECRET_KEY=your-production-jwt-secret
set DATABASE_URL=postgresql://user:pass@localhost/juris_iq
set RATELIMIT_STORAGE_URL=redis://localhost:6379
```

### Security Considerations
- Use HTTPS in production
- Change default secret keys
- Enable database connection pooling
- Set up reverse proxy (nginx)
- Configure firewall rules
- Regular security updates

### Scaling
- Database: PostgreSQL for production
- Caching: Redis for rate limiting and sessions
- Load Balancer: Multiple backend instances
- File Storage: Cloud storage for documents

## Support & Contact

For technical support or questions:
- Check the troubleshooting section above
- Review the API documentation
- Contact the development team

## License

This software is proprietary and confidential. All rights reserved.