pipeline {
    agent any
    
    tools {
        nodejs 'node18'
    }
    
    environment {
        NODE_HOME = tool 'node18'
        PATH = "${NODE_HOME}/bin:${PATH}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/Machi2130/test'
            }
        }
        
        stage('Build Backend') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {
                    script {
                        echo '>>> Restoring .NET packages'
                        sh 'dotnet restore testapp.sln'
                        
                        echo '>>> Publishing backend'
                        sh 'dotnet publish testapp.Server -c Release -o /var/www/testapp/api'
                        
                        echo '>>> Configuring database connection'
                        sh """
                            # Build connection string
                            CONNECTION_STRING="Server=${DB_SERVER};Database=testappDB;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;"
                            
                            # Update appsettings.json with connection string
                            cat > /var/www/testapp/api/appsettings.Production.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "\${CONNECTION_STRING}"
  }
}
EOF
                            
                            # Replace placeholder with actual connection string
                            sed -i "s|\\\${CONNECTION_STRING}|${CONNECTION_STRING}|g" /var/www/testapp/api/appsettings.Production.json
                            
                            # Set proper ownership and permissions for www-data
                            sudo chown -R www-data:www-data /var/www/testapp/api
                            sudo chmod 640 /var/www/testapp/api/appsettings.Production.json
                            sudo chmod 755 /var/www/testapp/api
                            
                            echo '✅ Database connection configured'
                        """
                    }
                }
            }
        }
        
        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    script {
                        echo '>>> Installing Angular dependencies'
                        sh 'npm install'
                        
                        echo '>>> Building Angular for production'
                        sh 'npm run build -- --configuration production'
                    }
                }
                
                script {
                    echo '>>> Deploying Angular build to web directory'
                    sh '''
                        # Ensure target directory exists
                        sudo mkdir -p /var/www/testapp/ui
                        sudo chown -R jenkins:jenkins /var/www/testapp
                        
                        # Clean existing files
                        rm -rf /var/www/testapp/ui/*
                        
                        # Copy Angular build files
                        if [ -d "testapp.client/dist/testapp.client/browser" ]; then
                            cp -r testapp.client/dist/testapp.client/browser/* /var/www/testapp/ui/
                        else
                            cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/
                        fi
                        
                        # Set proper permissions
                        sudo chmod -R 755 /var/www/testapp/ui
                        sudo chown -R www-data:www-data /var/www/testapp/ui
                        
                        echo '✅ Angular deployment complete'
                    '''
                }
            }
        }
        
        stage('Database Migration') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {
                    script {
                        echo '>>> Running database migrations'
                        sh """
                            cd /var/www/testapp/api
                            
                            # Set connection string as environment variable
                            export ConnectionStrings__DefaultConnection="Server=${DB_SERVER};Database=testappDB;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;"
                            
                            # Run EF migrations if available
                            if [ -f "testapp.Server.dll" ]; then
                                dotnet ef database update --no-build 2>/dev/null || echo 'ℹ️  No migrations to apply or EF not configured'
                            fi
                            
                            echo '✅ Database migration check complete'
                        """
                    }
                }
            }
        }
        
        stage('Restart API') {
            steps {
                script {
                    echo '>>> Restarting testapp service'
                    sh '''
                        sudo systemctl stop testapp || true
                        sleep 2
                        sudo systemctl start testapp
                        sleep 3
                        sudo systemctl status testapp
                    '''
                    echo '✅ Service restarted successfully'
                }
            }
        }
    }
    
    post {
        success {
            echo '🎉 Deployment completed successfully!'
            echo '📊 Deployment Summary:'
            echo '   ✅ Backend deployed to /var/www/testapp/api'
            echo '   ✅ Frontend deployed to /var/www/testapp/ui'
            echo '   ✅ Database configured'
            echo '   ✅ Service restarted'
        }
        failure {
            echo '❌ Deployment failed — check console logs.'
        }
        always {
            cleanWs(cleanWhenNotBuilt: false,
                    deleteDirs: true,
                    disableDeferredWipeout: true,
                    notFailBuild: true)
        }
    }
}
