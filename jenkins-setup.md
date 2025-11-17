# Jenkins CI/CD Setup for TestApp on VPS

## Prerequisites
- VPS with Jenkins installed and running
- VPS with Nginx installed and running
- VPS with SQL Server running (or access to remote SQL Server)
- Git repository (GitHub/GitLab/Bitbucket)

## Step 1: Configure Jenkins

### 1.1 Install Required Jenkins Plugins
1. Go to Jenkins Dashboard → Manage Jenkins → Manage Plugins
2. Install these plugins:
   - **Git** (for source code checkout)
   - **Pipeline** (for declarative pipelines)
   - **GitHub Integration** or **GitLab** (for webhooks)
   - **SSH Agent** (for deployment)
   - **Credentials Binding** (for secure credentials)

### 1.2 Configure Global Tools
1. Go to Manage Jenkins → Global Tool Configuration
2. Configure .NET SDK:
   - Name: `.NET 9.0`
   - Version: `9.0.x`
   - Installation: `Install automatically`
3. Configure Node.js:
   - Name: `Node.js 18`
   - Version: `18.x`
   - Installation: `Install automatically`

### 1.3 Add Credentials
1. Go to Manage Jenkins → Manage Credentials
2. Add these credentials:
   - **Git Repository Credentials**: Username/password or SSH key for your repo
   - **SSH Credentials**: For deploying to your VPS (if needed)
   - **Database Credentials**: For SQL Server connection

## Step 2: Create Jenkins Pipeline Job

### 2.1 Create New Pipeline Job
1. Click "New Item" → "Pipeline"
2. Name: `testapp-cicd`
3. Select "Pipeline" and click OK

### 2.2 Configure Pipeline
1. In the job configuration:
   - **Definition**: Pipeline script from SCM
   - **SCM**: Git
   - **Repository URL**: `https://github.com/yourusername/testapp.git` (or your repo URL)
   - **Credentials**: Select your git credentials
   - **Branch Specifier**: `*/main`
   - **Script Path**: `Jenkinsfile`

### 2.3 Configure Build Triggers
1. Check "GitHub hook trigger for GITScm polling" (if using GitHub)
2. Or configure webhook URL for other Git providers

## Step 3: Update Jenkinsfile for VPS Environment

The current Jenkinsfile needs modifications for your VPS setup:
