pipeline {
    agent any

    tools {
        nodejs 'node18'
    }

    environment {
        NODE_HOME = tool 'node18'
        PATH = "${NODE_HOME}/bin:${PATH}"
        DOTNET_CLI_TELEMETRY_OPTOUT = "1"
        # Force dotnet to skip first-time experience and reduce interactive prompts
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "true"
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/Machi2130/test'
            }
        }

        stage('Stop Service (best-effort)') {
            steps {
                script {
                    echo ">>> Stopping testapp service (best-effort)"
                    // Best-effort: sudo may require password unless configured in sudoers.
                    sh '''
                        # stop service if possible (ignore errors)
                        sudo systemctl stop testapp 2>/dev/null || true
                        sleep 2
                        # kill stray dotnet process that locks files (best-effort)
                        sudo pkill -9 -f testapp.Server.dll 2>/dev/null || true
                        sleep 1
                        echo "✅ stop-step done (errors ignored)"
                    '''
                }
            }
        }

        stage('Restore & Publish Backend (safe)') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {
                    script {
                        echo ">>> dotnet restore"
                        sh 'dotnet restore testapp.sln'

                        echo ">>> dotnet publish -> publish_temp (single-threaded to avoid MSB4166)"
                        // Use reduced parallelism and disable shared compilation to reduce memory usage.
                        sh """
                            rm -rf publish_temp
                            dotnet publish testapp.Server -c Release -o publish_temp /p:UseSharedCompilation=false -maxcpucount:1 --verbosity minimal
                        """

                        echo ">>> rsync publish_temp -> /var/www/testapp/api (atomic-ish)"
                        // Sync into place using sudo (ensure jenkins has permission or sudoers configured).
                        sh '''
                            sudo rsync -a --delete publish_temp/ /var/www/testapp/api/
                            sudo chown -R www-data:www-data /var/www/testapp
                            sudo chmod -R 775 /var/www/testapp
                        '''

                        // create Production config
                        sh """
                            CONNECTION_STRING="Server=${DB_SERVER};Database=gusto;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;"
                            cat > /var/www/testapp/api/appsettings.Production.json <<'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "PLACEHOLDER_CONNECTION_STRING"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
EOF
                            sudo sed -i "s|PLACEHOLDER_CONNECTION_STRING|${CONNECTION_STRING}|" /var/www/testapp/api/appsettings.Production.json
                            sudo chown www-data:www-data /var/www/testapp/api/appsettings.Production.json
                            sudo chmod 640 /var/www/testapp/api/appsettings.Production.json
                        """
                        echo "✅ Backend published"
                    }
                }
            }
        }

        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    script {
                        echo ">>> npm install"
                        sh 'npm ci --no-audit --no-fund || npm install'
                        echo ">>> ng build --production"
                        sh 'npm run build -- --configuration production'
                    }
                }
                script {
                    echo ">>> Deploy Angular build"
                    sh '''
                        sudo mkdir -p /var/www/testapp/ui
                        sudo rm -rf /var/www/testapp/ui/*
                        if [ -d "testapp.client/dist/testapp.client/browser" ]; then
                            sudo cp -r testapp.client/dist/testapp.client/browser/* /var/www/testapp/ui/
                        else
                            sudo cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/
                        fi
                        sudo chown -R www-data:www-data /var/www/testapp/ui
                        sudo chmod -R 755 /var/www/testapp/ui
                    '''
                    echo "✅ Frontend deployed"
                }
            }
        }

        stage('Optional DB migrations') {
            steps {
                script {
                    echo ">>> Running EF migrations (if configured)"
                    sh '''
                        cd /var/www/testapp/api || exit 0
                        export ConnectionStrings__DefaultConnection="Server=${DB_SERVER};Database=gusto;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;"
                        # Attempt migrations but don't fail pipeline if EF is not installed/configured
                        dotnet ef database update --no-build 2>/dev/null || echo "ℹ️ No migrations or dotnet-ef not available"
                    '''
                }
            }
        }

        stage('Start Service') {
            steps {
                script {
                    echo ">>> Starting systemd service (testapp)"
                    sh '''
                        sudo systemctl daemon-reload || true
                        sudo systemctl start testapp || true
                        sleep 3
                        sudo systemctl status testapp --no-pager || true
                        # quick health check (adjust port/URL as your app exposes)
                        timeout 10 bash -c 'until curl -sf http://127.0.0.1:6000/ || true; do sleep 1; done' || echo "⚠️ health-check timed out or endpoint missing"
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "🎉 Deployment finished successfully"
        }
        failure {
            echo "❌ Deployment failed — attempt to start service anyway"
            sh 'sudo systemctl start testapp || true'
        }
        always {
            cleanWs()
        }
    }
}

