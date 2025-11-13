# JuriIQ Setup Guide

This guide provides detailed instructions for setting up the JuriIQ backend system.

## System Requirements

### Hardware
- **CPU**: 2+ cores recommended
- **RAM**: 4GB minimum, 8GB recommended
- **Disk**: 10GB free space minimum
- **Network**: Internet connection for Docker image downloads

### Software
- **Operating System**: Windows 10/11, Windows Server 2019+
- **Docker Desktop**: Latest version ([Download](https://www.docker.com/products/docker-desktop))
- **Git**: For version control ([Download](https://git-scm.com/downloads))

## Step-by-Step Installation

### 1. Install Docker Desktop

1. Download Docker Desktop from https://www.docker.com/products/docker-desktop
2. Run the installer
3. Restart your computer when prompted
4. Launch Docker Desktop
5. Wait for Docker to start (whale icon in system tray should be steady)

**Configure Docker Resources**:
1. Right-click Docker icon → Settings
2. Go to Resources → Advanced
3. Set:
   - CPUs: 2 or more
   - Memory: 4GB or more
   - Disk: 20GB or more
4. Click "Apply & Restart"

### 2. Clone the Repository

```bash
git clone <repository-url>
cd juri-iq-x
```

Or download as ZIP and extract.

### 3. Configuration (Optional)

#### Database Configuration
Edit `docker-compose.yml` if you want to change database credentials:
```yaml
postgres:
  environment:
    POSTGRES_DB: juriiq
    POSTGRES_USER: juriiq_user
    POSTGRES_PASSWORD: your_password_here
```

**Important**: If you change database credentials, also update:
- `src/JuriIQ.Api/appsettings.json`
- `src/JuriIQ.Scheduler/appsettings.json`

#### JWT Secret Key
Edit `src/JuriIQ.Api/appsettings.json`:
```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
}
```

⚠️ **Important**: Change this in production!

#### Port Configuration
If ports 5001 or 8001 are in use, edit `docker-compose.yml`:
```yaml
backend:
  ports:
    - "5001:5001"  # Change first port to available port

frontend:
  ports:
    - "8001:8001"  # Change first port to available port
```

### 4. Start the System

#### Option A: Using Batch Script (Recommended)
Double-click `start-juriiq.bat`

#### Option B: Using PowerShell
```powershell
./start-juriiq.ps1
```

#### Option C: Using Docker Compose Directly
```bash
docker-compose up --build -d
```

### 5. Verify Installation

1. **Check running containers**:
   ```bash
   docker ps
   ```
   You should see 4 containers running:
   - juriiq-postgres
   - juriiq-backend
   - juriiq-scheduler
   - juriiq-frontend

2. **Check logs**:
   ```bash
   docker-compose logs -f
   ```

3. **Access services**:
   - Frontend: http://localhost:8001
   - API: http://localhost:5001/api/health
   - Swagger: http://localhost:5001/swagger

4. **Test login**:
   - Go to http://localhost:8001/login.html
   - Email: `admin@test.com`
   - Password: `Pass!2345`

## Post-Installation

### Add Sample Documents

1. Navigate to the `documents_to_process/` folder
2. Add some .pdf, .docx, or .txt files
3. Wait for processing (check logs: `docker-compose logs scheduler`)
4. Processed documents will appear in `documents_done/`

### Create Additional Users

Use the registration page or API:
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"user@example.com\",
    \"password\": \"Password123!\",
    \"firstName\": \"John\",
    \"lastName\": \"Doe\"
  }"
```

## Updating the System

### Update Code
```bash
git pull origin main
./rebuild-juriiq.bat
```

### Update Database Schema
1. Add new SQL scripts to `database/scripts/`
2. Run: `docker-compose exec postgres psql -U juriiq_user -d juriiq -f /docker-entrypoint-initdb.d/your-script.sql`

## Backup and Restore

### Backup Database
```bash
docker-compose exec postgres pg_dump -U juriiq_user juriiq > backup.sql
```

### Restore Database
```bash
docker-compose exec -T postgres psql -U juriiq_user juriiq < backup.sql
```

### Backup Documents
Copy the following folders:
- `documents_done/`
- `documents_to_process/` (if needed)

## Uninstallation

### Stop and Remove Containers
```bash
docker-compose down
```

### Remove All Data (Optional)
```bash
docker-compose down -v  # Removes volumes (database data)
```

### Remove Images (Optional)
```bash
docker-compose down --rmi all
```

## Troubleshooting

### Port Already in Use
Error: `Bind for 0.0.0.0:5001 failed: port is already allocated`

**Solution**: Change port in `docker-compose.yml`

### Docker Not Running
Error: `Cannot connect to the Docker daemon`

**Solution**: Start Docker Desktop

### Database Connection Failed
Error: `Could not connect to PostgreSQL`

**Solutions**:
1. Wait 30 seconds for PostgreSQL to initialize
2. Check PostgreSQL logs: `docker-compose logs postgres`
3. Restart: `docker-compose restart postgres`

### Out of Disk Space
Error: `no space left on device`

**Solutions**:
1. Clean Docker: `docker system prune -a`
2. Increase Docker disk limit in Docker Desktop settings

### Permission Denied (Windows)
Error: `Permission denied` when accessing folders

**Solutions**:
1. Run Docker Desktop as Administrator
2. Check folder permissions in Windows Explorer

## Security Checklist

Before deploying to production:

- [ ] Change JWT secret key
- [ ] Change database password
- [ ] Enable HTTPS/TLS
- [ ] Configure firewall rules
- [ ] Set strong admin password
- [ ] Review and restrict CORS settings
- [ ] Enable Docker security features
- [ ] Set up regular backups
- [ ] Configure log rotation
- [ ] Disable Swagger in production

## Next Steps

- Read [API.md](./API.md) for API documentation
- Read [DEPLOYMENT.md](./DEPLOYMENT.md) for production deployment
- Configure scheduled document processing times
- Set up monitoring and alerting
- Integrate with your organization's authentication system

## Support

For help with setup:
1. Check logs: `docker-compose logs`
2. Review troubleshooting section above
3. Create an issue in the repository
4. Contact the development team
