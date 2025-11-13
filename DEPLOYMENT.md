# JuriIQ Deployment Guide

This guide covers deploying JuriIQ to production environments.

## Pre-Deployment Checklist

Before deploying to production:

### Security
- [ ] Change JWT secret key to a strong, random value
- [ ] Change database password
- [ ] Review and update CORS settings
- [ ] Disable Swagger UI in production
- [ ] Enable HTTPS/TLS certificates
- [ ] Configure firewall rules
- [ ] Set up secure key management (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Enable audit logging
- [ ] Configure rate limiting
- [ ] Review and update authentication settings

### Configuration
- [ ] Update connection strings
- [ ] Configure email service (if applicable)
- [ ] Set up monitoring and alerting
- [ ] Configure backup schedules
- [ ] Set up log aggregation
- [ ] Configure CDN for static assets
- [ ] Update environment variables
- [ ] Configure SSL certificates

### Infrastructure
- [ ] Size compute resources appropriately
- [ ] Set up load balancing (if needed)
- [ ] Configure auto-scaling rules
- [ ] Set up database replication
- [ ] Configure disaster recovery
- [ ] Set up health checks
- [ ] Configure network security groups

## Docker Production Configuration

### Update docker-compose for Production

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    restart: always
    environment:
      POSTGRES_DB: ${DB_NAME}
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/scripts:/docker-entrypoint-initdb.d
    networks:
      - juriiq-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER} -d ${DB_NAME}"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ./src
      dockerfile: JuriIQ.Api/Dockerfile
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:5001;http://+:5000
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/cert.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
      - JwtSettings__SecretKey=${JWT_SECRET}
    volumes:
      - ./documents_to_process:/app/documents_to_process
      - ./documents_done:/app/documents_done
      - ./documents_failed:/app/documents_failed
      - ./certs:/app/certs:ro
      - backend_logs:/app/logs
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - juriiq-network

  scheduler:
    build:
      context: ./src
      dockerfile: JuriIQ.Scheduler/Dockerfile
    restart: always
    environment:
      - DOTNET_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
    volumes:
      - ./documents_to_process:/app/documents_to_process
      - ./documents_done:/app/documents_done
      - ./documents_failed:/app/documents_failed
      - scheduler_logs:/app/logs
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - juriiq-network

  frontend:
    build:
      context: ./webapp
      dockerfile: Dockerfile
    restart: always
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - ./certs:/etc/nginx/certs:ro
      - ./nginx-prod.conf:/etc/nginx/conf.d/default.conf:ro
    depends_on:
      - backend
    networks:
      - juriiq-network

  # Nginx Reverse Proxy (Optional - for SSL termination)
  nginx:
    image: nginx:alpine
    restart: always
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./certs:/etc/nginx/certs:ro
    depends_on:
      - backend
      - frontend
    networks:
      - juriiq-network

networks:
  juriiq-network:
    driver: bridge

volumes:
  postgres_data:
  backend_logs:
  scheduler_logs:
```

### Environment Variables

Create `.env` file:

```env
# Database
DB_NAME=juriiq_prod
DB_USER=juriiq_user
DB_PASSWORD=<strong-password-here>

# JWT
JWT_SECRET=<strong-secret-key-here>

# SSL Certificate
CERT_PASSWORD=<certificate-password-here>
```

**Important**: Never commit `.env` file to source control!

### SSL/TLS Configuration

#### Option 1: Let's Encrypt

```bash
# Install certbot
apt-get install certbot

# Get certificate
certbot certonly --standalone -d yourdomain.com

# Copy certificates
cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem ./certs/
cp /etc/letsencrypt/live/yourdomain.com/privkey.pem ./certs/
```

#### Option 2: Self-Signed (Development Only)

```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ./certs/privkey.pem \
  -out ./certs/fullchain.pem
```

## Windows Server Deployment

### Prerequisites

1. Windows Server 2019 or later
2. Docker Desktop for Windows Server
3. .NET 8 SDK (for local development)

### Installation Steps

1. **Install Docker Desktop**:
   - Download from Docker website
   - Follow installation wizard
   - Configure resources (CPU, Memory)

2. **Clone Repository**:
   ```powershell
   cd C:\inetpub\
   git clone <repository-url> juriiq
   cd juriiq
   ```

3. **Configure Environment**:
   - Copy `.env.example` to `.env`
   - Edit `.env` with production values
   - Update `docker-compose.prod.yml`

4. **Start Services**:
   ```powershell
   docker-compose -f docker-compose.prod.yml up -d
   ```

5. **Configure Windows Firewall**:
   ```powershell
   New-NetFirewallRule -DisplayName "JuriIQ HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
   New-NetFirewallRule -DisplayName "JuriIQ HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
   ```

6. **Set up Windows Service** (Optional):
   Use NSSM to run Docker Compose as a Windows Service:
   ```powershell
   nssm install JuriIQ "docker-compose" "-f docker-compose.prod.yml up"
   nssm set JuriIQ AppDirectory C:\inetpub\juriiq
   nssm start JuriIQ
   ```

## Cloud Deployment

### Azure

#### Using Azure Container Instances

```bash
# Login to Azure
az login

# Create resource group
az group create --name juriiq-rg --location eastus

# Create container group
az container create \
  --resource-group juriiq-rg \
  --file docker-compose.azure.yml
```

#### Using Azure App Service

```bash
# Create App Service Plan
az appservice plan create \
  --name juriiq-plan \
  --resource-group juriiq-rg \
  --is-linux --sku B1

# Create Web App
az webapp create \
  --resource-group juriiq-rg \
  --plan juriiq-plan \
  --name juriiq-api \
  --deployment-container-image-name juriiq/backend:latest
```

### AWS

#### Using ECS

```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name juriiq-cluster

# Register task definition
aws ecs register-task-definition --cli-input-json file://task-definition.json

# Create service
aws ecs create-service \
  --cluster juriiq-cluster \
  --service-name juriiq-service \
  --task-definition juriiq-task:1 \
  --desired-count 2
```

## Monitoring & Logging

### Application Insights (Azure)

Add to `appsettings.Production.json`:

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### Serilog Configuration

```json
{
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/juriiq-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq-server:5341"
        }
      }
    ]
  }
}
```

### Health Checks

Configure health check endpoints in `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck("document_processor", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
```

## Backup Strategy

### Database Backups

#### Automated Daily Backup

```bash
# Create backup script
cat > backup-db.sh << 'EOF'
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR=/backups
docker-compose exec -T postgres pg_dump -U juriiq_user juriiq | gzip > $BACKUP_DIR/juriiq_$DATE.sql.gz
find $BACKUP_DIR -name "juriiq_*.sql.gz" -mtime +30 -delete
EOF

chmod +x backup-db.sh

# Add to crontab
crontab -e
# Add: 0 2 * * * /path/to/backup-db.sh
```

### Document Backups

```bash
# Sync to cloud storage
aws s3 sync ./documents_done s3://juriiq-documents/done/
aws s3 sync ./documents_to_process s3://juriiq-documents/pending/
```

## Scaling

### Horizontal Scaling

Use Docker Swarm or Kubernetes for orchestration:

```yaml
# docker-compose.scale.yml
services:
  backend:
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
      restart_policy:
        condition: on-failure
```

Deploy:
```bash
docker stack deploy -c docker-compose.scale.yml juriiq
```

### Load Balancing

Configure Nginx load balancer:

```nginx
upstream backend_servers {
    least_conn;
    server backend1:5001;
    server backend2:5001;
    server backend3:5001;
}

server {
    listen 443 ssl;
    server_name api.juriiq.com;

    location / {
        proxy_pass http://backend_servers;
    }
}
```

## Maintenance

### Update Procedure

1. **Backup everything**:
   ```bash
   ./backup-db.sh
   cp -r documents_done documents_done_backup
   ```

2. **Pull latest code**:
   ```bash
   git pull origin main
   ```

3. **Update database** (if needed):
   ```bash
   docker-compose exec postgres psql -U juriiq_user -d juriiq -f /path/to/migration.sql
   ```

4. **Rebuild and restart**:
   ```bash
   docker-compose -f docker-compose.prod.yml up --build -d
   ```

5. **Verify**:
   ```bash
   docker-compose ps
   curl https://api.juriiq.com/health
   ```

### Rollback Procedure

```bash
# Restore previous Docker images
docker-compose -f docker-compose.prod.yml down
git checkout <previous-commit>
docker-compose -f docker-compose.prod.yml up -d

# Restore database
gunzip < backup.sql.gz | docker-compose exec -T postgres psql -U juriiq_user juriiq
```

## Performance Tuning

### PostgreSQL

```sql
-- Increase connection pool
ALTER SYSTEM SET max_connections = 200;

-- Enable query caching
ALTER SYSTEM SET shared_buffers = '2GB';
ALTER SYSTEM SET effective_cache_size = '6GB';

-- Optimize for SSD
ALTER SYSTEM SET random_page_cost = 1.1;
```

### .NET Application

```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxRequestBodySize": 52428800
    }
  }
}
```

## Security Best Practices

1. **Regular Updates**: Keep Docker, OS, and dependencies updated
2. **Least Privilege**: Run containers as non-root user
3. **Network Segmentation**: Use Docker networks to isolate services
4. **Secrets Management**: Use Docker secrets or external vaults
5. **Monitoring**: Set up intrusion detection and log analysis
6. **Backups**: Test restore procedures regularly
7. **SSL/TLS**: Use strong cipher suites
8. **Rate Limiting**: Implement API rate limiting
9. **Input Validation**: Validate all user inputs
10. **Regular Security Audits**: Scan for vulnerabilities

## Support

For deployment assistance:
- Review logs: `docker-compose logs -f`
- Check health endpoints
- Contact DevOps team
- Review Azure/AWS documentation
