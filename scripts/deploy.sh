l#!/bin/bash

# TestApp Deployment Script
# This script handles the deployment of the TestApp application

set -e  # Exit on any error

echo "Starting TestApp deployment..."

# Configuration
APP_NAME="testapp"
DEPLOY_DIR="/opt/testapp"
BACKUP_DIR="/opt/testapp_backups"
SERVICE_NAME="testapp.service"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
if [[ $EUID -eq 0 ]]; then
   log_error "This script should not be run as root"
   exit 1
fi

# Create backup if deployment directory exists
if [ -d "$DEPLOY_DIR" ]; then
    TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
    BACKUP_PATH="$BACKUP_DIR/$APP_NAME_$TIMESTAMP"

    log_info "Creating backup at $BACKUP_PATH"
    mkdir -p "$BACKUP_DIR"
    cp -r "$DEPLOY_DIR" "$BACKUP_PATH"
fi

# Stop the service if it's running
if systemctl is-active --quiet $SERVICE_NAME; then
    log_info "Stopping $SERVICE_NAME service"
    sudo systemctl stop $SERVICE_NAME
fi

# Create deployment directory
log_info "Creating deployment directory"
sudo mkdir -p "$DEPLOY_DIR"
sudo chown $USER:$USER "$DEPLOY_DIR"

# Copy published application
log_info "Copying application files"
cp -r publish/* "$DEPLOY_DIR/"

# Set proper permissions
log_info "Setting permissions"
sudo chown -R www-data:www-data "$DEPLOY_DIR"
sudo chmod -R 755 "$DEPLOY_DIR"

# Update database connection strings if needed
# Note: Update appsettings.json with production values before deployment

# Start the service
log_info "Starting $SERVICE_NAME service"
sudo systemctl start $SERVICE_NAME

# Check if service started successfully
if systemctl is-active --quiet $SERVICE_NAME; then
    log_info "Service started successfully"
else
    log_error "Failed to start service"
    exit 1
fi

# Clean up old backups (keep last 5)
log_info "Cleaning up old backups"
cd "$BACKUP_DIR" && ls -t | tail -n +6 | xargs -r rm -rf

log_info "Deployment completed successfully!"

# Health check
sleep 5
if curl -f http://localhost:5000/health > /dev/null 2>&1; then
    log_info "Health check passed"
else
    log_warn "Health check failed - please verify the application"
fi
