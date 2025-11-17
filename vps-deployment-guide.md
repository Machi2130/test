# Complete VPS Deployment Guide for TestApp

## Overview
This guide will help you set up a complete CI/CD pipeline that automatically deploys your TestApp from GitHub to your VPS whenever you push code to the main branch.

## Architecture
```
GitHub Repo (main branch)
    ↓ (push triggers webhook)
Jenkins Server
    ↓ (builds and tests)
VPS Server
    ├── Nginx (reverse proxy on port 80/443)
    ├── TestApp .NET Core (Kestrel on port 5000)
    └── SQL Server (database)
```

## Prerequisites Checklist
- [ ] VPS with Ubuntu/Debian
- [ ] Jenkins installed and running on VPS
- [ ] SQL Server installed and running on VPS
- [ ] Nginx installed on VPS
- [ ] GitHub repository created
- [ ] Domain name (optional, for HTTPS)

## Step 1: VPS Software Installation

### Install Required Software

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 9.0 SDK
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

# Install Git
sudo apt install -y git

# Verify installations
dotnet --version
node --version
npm --version
nginx -v
git --version
```

## Step 2: Configure Jenkins

### 2.1 Install Jenkins Plugins
1. Go to Jenkins Dashboard → Manage Jenkins → Manage Plugins
2. Install these plugins:
   - Git
   - Pipeline
   - GitHub Integration
   - Credentials Binding

### 2.2 Configure Global Tools
1. Manage Jenkins → Global Tool Configuration
2. Add .NET SDK:
   - Name: `.NET 9.0`
   - Version: `9.0.x`
   - Install automatically: ✅
3. Add Node.js:
   - Name: `Node.js 18`
   - Version: `18.x`
   - Install automatically: ✅

### 2.3 Create Jenkins Job
1. New Item → Pipeline
2. Name: `testapp-cicd`
3. Configure:
   - **Definition**: Pipeline script from SCM
   - **SCM**: Git
   - **Repository URL**: `https://github.com/yourusername/testapp.git`
   - **Credentials**: Add your GitHub credentials
   - **Branch Specifier**: `*/main`
   - **Script Path**: `Jenkinsfile.vps`
4. Build Triggers: ✅ GitHub hook trigger for GITScm polling

## Step 3: Configure GitHub Webhook

### 3.1 GitHub Repository Settings
1. Go to your GitHub repo → Settings → Webhooks
2. Add webhook:
   - **Payload URL**: `http://your-jenkins-server:8080/github-webhook/`
   - **Content type**: `application/json`
   - **Events**: `Just the push event`
   - **Active**: ✅

## Step 4: VPS Directory Setup

```bash
# Create application directories
sudo mkdir -p /var/www/testapp
sudo mkdir -p /var/www/testapp_backups

# Set proper ownership
sudo chown -R www-data:www-data /var/www/testapp
sudo chown -R www-data:www-data /var/www/testapp_backups

# Create log directories
sudo mkdir -p /var/log/testapp
sudo chown www-data:www-data /var/log/testapp
```

## Step 5: Configure Nginx

```bash
# Copy nginx configuration
sudo cp nginx.conf /etc/nginx/sites-available/testapp

# Enable the site
sudo ln -s /etc/nginx/sites-available/testapp /etc/nginx/sites-enabled/

# Remove default nginx site (optional)
sudo rm /etc/nginx/sites-enabled/default

# Test configuration
sudo nginx -t

# Reload nginx
sudo systemctl reload nginx
```

## Step 6: Configure Systemd Service

```bash
# Copy service file
sudo cp testapp.service /etc/systemd/system/

# Reload systemd
sudo systemctl daemon-reload

# Enable service (starts on boot)
sudo systemctl enable testapp.service
```

## Step 7: Database Configuration

### Update appsettings.json for Production

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=testapp;User ID=sa;Password=YourStrongPassword;Encrypt=True;TrustServerCertificate=True;",
    "ReportConnection": "Server=localhost,1433;Database=testapp_reports;User ID=sa;Password=YourStrongPassword;Encrypt=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "Your256BitSecretKeyHereMakeItVeryLongAndSecure",
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

### Create Databases

```bash
# Connect to SQL Server
sqlcmd -S localhost -U sa -P YourPassword

# Create databases
CREATE DATABASE testapp;
CREATE DATABASE testapp_reports;
GO

# Exit
QUIT
```

## Step 8: SSL Certificate (Optional but Recommended)

```bash
# Install certbot
sudo apt install -y certbot python3-certbot-nginx

# Get certificate (replace with your domain)
sudo certbot --nginx -d yourdomain.com

# Test renewal
sudo certbot renew --dry-run
```

## Step 9: First Deployment

### 9.1 Push Code to GitHub

```bash
# Add all files
git add .

# Commit
git commit -m "Initial CI/CD setup for VPS deployment"

# Push to main (this triggers Jenkins pipeline)
git push origin main
```

### 9.2 Monitor Jenkins Build
1. Go to Jenkins dashboard
2. Click on `testapp-cicd` job
3. Watch the build progress
4. Check console output for any errors

## Step 10: Verify Deployment

### Check Services

```bash
# Check systemd service
sudo systemctl status testapp.service

# Check nginx
sudo systemctl status nginx

# Check application logs
sudo journalctl -u testapp.service -f

# Check nginx logs
sudo tail -f /var/log/nginx/testapp_access.log
sudo tail -f /var/log/nginx/testapp_error.log
```

### Test Application

```bash
# Test health endpoint
curl http://localhost/health

# Test main application
curl http://localhost/

# Test API endpoints
curl http://localhost/api/auth/all
```

## Step 11: Access Your Application

- **Local**: `http://localhost/`
- **External**: `http://your-vps-ip/`
- **Domain**: `https://yourdomain.com/` (if SSL configured)

## Troubleshooting

### Common Issues

1. **Build Fails**
   - Check Jenkins console output
   - Verify .NET and Node.js versions
   - Check file permissions

2. **Deployment Fails**
   ```bash
   # Check permissions
   sudo chown -R www-data:www-data /var/www/testapp

   # Check service status
   sudo systemctl status testapp.service

   # Check logs
   sudo journalctl -u testapp.service -n 50
   ```

3. **Database Connection Issues**
   ```bash
   # Test SQL Server connection
   sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"

   # Check if SQL Server is running
   sudo systemctl status mssql-server
   ```

4. **Nginx Issues**
   ```bash
   # Test configuration
   sudo nginx -t

   # Check nginx status
   sudo systemctl status nginx

   # Check logs
   sudo tail -f /var/log/nginx/error.log
   ```

### Logs to Monitor

- **Application Logs**: `sudo journalctl -u testapp.service -f`
- **Nginx Access**: `sudo tail -f /var/log/nginx/testapp_access.log`
- **Nginx Error**: `sudo tail -f /var/log/nginx/testapp_error.log`
- **Jenkins Build**: Jenkins Dashboard → Job → Console Output

## Security Hardening

### Firewall Configuration

```bash
# Install ufw
sudo apt install -y ufw

# Allow SSH, HTTP, HTTPS
sudo ufw allow ssh
sudo ufw allow 80
sudo ufw allow 443

# Enable firewall
sudo ufw enable
```

### SSL/TLS Configuration

```bash
# Update nginx config for SSL
sudo nano /etc/nginx/sites-available/testapp

# Add SSL configuration:
server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    # ... rest of config
}
```

## Backup Strategy

### Application Backups
- Jenkins pipeline creates backups automatically before deployment
- Keeps last 3 backups in `/var/www/testapp_backups/`

### Database Backups

```bash
# Create backup script
sudo nano /usr/local/bin/backup-databases.sh

# Add content:
#!/bin/bash
BACKUP_DIR="/var/backups/sqlserver"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

mkdir -p $BACKUP_DIR

sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE testapp TO DISK = '$BACKUP_DIR/testapp_$TIMESTAMP.bak'"
sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE testapp_reports TO DISK = '$BACKUP_DIR/testapp_reports_$TIMESTAMP.bak'"

# Keep only last 7 days
find $BACKUP_DIR -name "*.bak" -mtime +7 -delete

# Make executable
sudo chmod +x /usr/local/bin/backup-databases.sh

# Add to cron for daily backups
sudo crontab -e

# Add line:
0 2 * * * /usr/local/bin/backup-databases.sh
```

## Monitoring Setup

### Health Checks

```bash
# Install monitoring tools
sudo apt install -y htop iotop ncdu

# Create health check script
sudo nano /usr/local/bin/health-check.sh

# Add content:
#!/bin/bash
# Check if application is responding
if curl -f http://localhost/health > /dev/null 2>&1; then
    echo "✅ Application is healthy"
    exit 0
else
    echo "❌ Application is not responding"
    exit 1
fi

# Make executable
sudo chmod +x /usr/local/bin/health-check.sh

# Test
/usr/local/bin/health-check.sh
```

### Log Rotation

```bash
# Configure logrotate for application logs
sudo nano /etc/logrotate.d/testapp

# Add content:
/var/log/testapp/*.log {
    daily
    missingok
    rotate 7
    compress
    notifempty
    create 0644 www-data www-data
    postrotate
        systemctl reload testapp.service
    endscript
}
```

## Next Steps

1. **Test the Pipeline**: Make a small change and push to trigger deployment
2. **Configure Monitoring**: Set up alerts for failures
3. **SSL Certificate**: Get HTTPS working
4. **Domain Setup**: Point domain to your VPS
5. **Backup Automation**: Ensure regular database backups
6. **Security Audit**: Review firewall and permissions
7. **Performance Tuning**: Monitor resource usage and optimize

## Support

If you encounter issues:

1. Check the Jenkins build logs
2. Review systemd service logs: `sudo journalctl -u testapp.service`
3. Check nginx logs: `sudo tail -f /var/log/nginx/error.log`
4. Verify database connectivity
5. Ensure all permissions are correct

Your CI/CD pipeline is now ready! Every push to the main branch will automatically trigger a build, test, and deployment to your VPS.
