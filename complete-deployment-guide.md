# Complete TestApp Deployment Guide
## Docker Testing + VPS Production Setup

This guide provides a complete walkthrough for deploying your TestApp full-stack application. First, we'll test everything locally using Docker, then deploy to your VPS with Jenkins CI/CD.

---

## 🐳 Part 1: Local Docker Testing

### Step 1: Prerequisites
- Docker installed on your local machine
- Docker Compose installed
- Git repository cloned locally

### Step 2: Local Directory Setup
```bash
# Clone your repository (replace with your repo URL)
git clone https://github.com/yourusername/testapp.git
cd testapp

# Create necessary directories
mkdir -p docker/nginx
mkdir -p docker/sqlserver
```

### Step 3: Create Docker Environment Files

#### docker-compose.yml (for local testing)
```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: testapp-sqlserver
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourStrong!Passw0rd
      MSSQL_PID: Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./docker/sqlserver/init:/docker-entrypoint-initdb.d
    networks:
      - testapp-network

  testapp:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: testapp-app
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5000
      ConnectionStrings__DefaultConnection: Server=sqlserver,1433;Database=testapp;User ID=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;
      ConnectionStrings__ReportConnection: Server=sqlserver,1433;Database=testapp_reports;User ID=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;
    ports:
      - "5000:5000"
    depends_on:
      - sqlserver
    networks:
      - testapp-network
    volumes:
      - ./testapp.Server/appsettings.Development.json:/app/appsettings.Development.json:ro

  nginx:
    image: nginx:alpine
    container_name: testapp-nginx
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/conf.d/default.conf:ro
    depends_on:
      - testapp
    networks:
      - testapp-network

volumes:
  sqlserver_data:

networks:
  testapp-network:
    driver: bridge
```

#### Dockerfile (multi-stage build)
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["testapp.Server/testapp.Server.csproj", "testapp.Server/"]
COPY ["testapp.DAL/testapp.DAL.csproj", "testapp.DAL/"]
COPY ["testapp.Domain/testapp.Domain.csproj", "testapp.Domain/"]
RUN dotnet restore "testapp.Server/testapp.Server.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/testapp.Server"
RUN dotnet build "testapp.Server.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "testapp.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Angular build stage
FROM node:18-alpine AS angular-build
WORKDIR /app
COPY testapp.client/package*.json ./
RUN npm ci
COPY testapp.client/ ./
RUN npm run build --prod

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=angular-build /app/dist/testapp.client ./wwwroot
EXPOSE 5000
ENTRYPOINT ["dotnet", "testapp.Server.dll"]
```

#### docker/sqlserver/init/01-init.sql
```sql
CREATE DATABASE testapp;
CREATE DATABASE testapp_reports;
GO
```

### Step 4: Update nginx.conf for Docker
```nginx
server {
    listen 80;
    server_name localhost;

    location / {
        proxy_pass http://testapp:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # API routes
    location /api/ {
        proxy_pass http://testapp:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Static files
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        proxy_pass http://testapp:5000;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

### Step 5: Test Locally with Docker

```bash
# Build and start all services
docker-compose up -d

# Wait for SQL Server to initialize (about 30 seconds)
sleep 30

# Check if services are running
docker-compose ps

# Check logs
docker-compose logs sqlserver
docker-compose logs testapp
docker-compose logs nginx

# Test the application
curl http://localhost/
curl http://localhost/api/auth/all

# Access the application in browser
# http://localhost/
```

### Step 6: Test Database Connection

```bash
# Connect to SQL Server container
docker exec -it testapp-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd

# Check databases
SELECT name FROM sys.databases;
GO

# Exit
QUIT
```

### Step 7: Clean Up Docker Test

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (data) if you want to start fresh
docker-compose down -v

# Remove images if needed
docker-compose down --rmi all
```

---

## 🖥️ Part 2: VPS Production Setup

### Step 8: VPS Prerequisites
- Ubuntu/Debian VPS
- Root or sudo access
- Domain name (optional)

### Step 9: VPS Software Installation

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 9.0
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-9.0

# Install Node.js 18
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install Nginx
sudo apt install -y nginx

# Install SQL Server (if not already installed)
# Follow Microsoft's official guide for Ubuntu
```

### Step 10: VPS Directory Setup

```bash
# Create application directories
sudo mkdir -p /var/www/testapp
sudo mkdir -p /var/www/testapp_backups
sudo mkdir -p /var/log/testapp

# Set ownership
sudo chown -R www-data:www-data /var/www/testapp
sudo chown -R www-data:www-data /var/www/testapp_backups
sudo chown www-data:www-data /var/log/testapp
```

### Step 11: Configure Nginx on VPS

```bash
# Copy nginx configuration
sudo cp nginx.conf /etc/nginx/sites-available/testapp

# Enable site
sudo ln -s /etc/nginx/sites-available/testapp /etc/nginx/sites-enabled/

# Remove default site
sudo rm /etc/nginx/sites-enabled/default

# Test configuration
sudo nginx -t

# Reload nginx
sudo systemctl reload nginx
```

### Step 12: Configure Systemd Service

```bash
# Copy service file
sudo cp testapp.service /etc/systemd/system/

# Reload systemd
sudo systemctl daemon-reload

# Enable service
sudo systemctl enable testapp.service
```

### Step 13: Database Setup on VPS

```bash
# Create databases (adjust connection details)
sqlcmd -S localhost -U sa -P YourPassword -Q "
CREATE DATABASE testapp;
CREATE DATABASE testapp_reports;
GO
"
```

### Step 14: Update Production Configuration

Create `/var/www/testapp/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=testapp;User ID=sa;Password=YourActualPassword;Encrypt=True;TrustServerCertificate=True;",
    "ReportConnection": "Server=localhost,1433;Database=testapp_reports;User ID=sa;Password=YourActualPassword;Encrypt=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "Your256BitSecretKeyHereMakeItVeryLongAndSecure1234567890123456789012345678901234567890",
    "Issuer": "testapp",
    "Audience": "testapp-users",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🚀 Part 3: Jenkins CI/CD Setup

### Step 15: Install and Configure Jenkins

```bash
# Install Jenkins (if not already installed)
wget -q -O - https://pkg.jenkins.io/debian-stable/jenkins.io.key | sudo apt-key add -
sudo sh -c 'echo deb http://pkg.jenkins.io/debian-stable binary/ > /etc/apt/sources.list.d/jenkins.list'
sudo apt update
sudo apt install -y jenkins

# Start Jenkins
sudo systemctl start jenkins
sudo systemctl enable jenkins

# Get initial admin password
sudo cat /var/lib/jenkins/secrets/initialAdminPassword
```

### Step 16: Jenkins Configuration

1. **Access Jenkins**: `http://your-vps-ip:8080`
2. **Install Plugins**:
   - Git
   - Pipeline
   - GitHub Integration
   - Credentials Binding

3. **Configure Global Tools**:
   - .NET SDK 9.0 (install automatically)
   - Node.js 18 (install automatically)

4. **Create Pipeline Job**:
   - Name: `testapp-cicd`
   - Pipeline script from SCM
   - Git repository URL: `https://github.com/yourusername/testapp.git`
   - Script Path: `Jenkinsfile.vps`
   - Build Triggers: GitHub hook trigger

### Step 17: GitHub Webhook Setup

1. **Repository Settings** → **Webhooks**
2. **Add webhook**:
   - Payload URL: `http://your-jenkins-server:8080/github-webhook/`
   - Content type: `application/json`
   - Events: `Push`
   - Active: ✅

### Step 18: First Deployment

```bash
# Commit all files
git add .
git commit -m "Complete CI/CD setup with Docker testing and VPS deployment"

# Push to trigger deployment
git push origin main
```

### Step 19: Monitor Deployment

```bash
# Check Jenkins build status
# Go to Jenkins dashboard → testapp-cicd → Console Output

# Check service status
sudo systemctl status testapp.service

# Check application logs
sudo journalctl -u testapp.service -f

# Check nginx logs
sudo tail -f /var/log/nginx/testapp_access.log
sudo tail -f /var/log/nginx/testapp_error.log
```

---

## 🔧 Part 4: Troubleshooting

### Docker Issues
```bash
# Check container logs
docker-compose logs

# Rebuild containers
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Check container resource usage
docker stats
```

### VPS Issues
```bash
# Check service status
sudo systemctl status testapp.service
sudo systemctl status nginx
sudo systemctl status jenkins

# Restart services
sudo systemctl restart testapp.service
sudo systemctl restart nginx

# Check permissions
sudo chown -R www-data:www-data /var/www/testapp

# Test nginx configuration
sudo nginx -t
```

### Database Issues
```bash
# Test SQL Server connection
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"

# Check database existence
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT name FROM sys.databases"
```

### Jenkins Issues
```bash
# Check Jenkins logs
sudo tail -f /var/log/jenkins/jenkins.log

# Restart Jenkins
sudo systemctl restart jenkins

# Check Jenkins status
sudo systemctl status jenkins
```

---

## 📋 Part 5: File Checklist

Ensure you have these files in your repository:

- [ ] `Dockerfile` (multi-stage build)
- [ ] `docker-compose.yml` (local testing)
- [ ] `Jenkinsfile.vps` (Jenkins pipeline)
- [ ] `nginx.conf` (web server config)
- [ ] `testapp.service` (systemd service)
- [ ] `complete-deployment-guide.md` (this guide)
- [ ] `docker/sqlserver/init/01-init.sql` (database init)

---

## 🎯 Quick Commands Reference

### Docker Testing
```bash
# Start local environment
docker-compose up -d

# Stop and cleanup
docker-compose down -v

# View logs
docker-compose logs -f
```

### VPS Deployment
```bash
# Check services
sudo systemctl status testapp.service
sudo systemctl status nginx

# View logs
sudo journalctl -u testapp.service -f
sudo tail -f /var/log/nginx/error.log

# Restart services
sudo systemctl restart testapp.service
sudo systemctl reload nginx
```

### Jenkins
```bash
# Restart Jenkins
sudo systemctl restart jenkins

# Check status
sudo systemctl status jenkins
```

---

## 🚀 Final Steps

1. **Test locally with Docker** (Part 1)
2. **Set up VPS** (Part 2)
3. **Configure Jenkins** (Part 3)
4. **Deploy and monitor** (Parts 3-4)
5. **Set up SSL** (optional but recommended)

Your application will be automatically deployed whenever you push to the main branch!

**Access URLs:**
- **Local Docker**: `http://localhost/`
- **VPS Production**: `http://your-vps-ip/` or `https://yourdomain.com/`
