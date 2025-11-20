pipeline {
    agent any

    tools {
        nodejs 'node18'
    }

    environment {
        NODE_HOME = tool 'node18'
        PATH = "${NODE_HOME}/bin:${PATH}"
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "true"
        DOTNET_CLI_TELEMETRY_OPTOUT = "true"

        MAIN_DOMAIN = "maaain.duckdns.org"
        DEV_DOMAIN  = "testsdev.duckdns.org"
        PRATHA_DOMAIN = "devtests.duckdns.org"
    }

    stages {

        stage('Detect Branch & Assign Domain') {
            steps {
                script {
                    branch = env.BRANCH_NAME

                    isMain = (branch == "main")
                    isDev  = (branch == "dev")
                    isPratha = (branch.toLowerCase().startsWith("prathamesh"))

                    if (isMain) {
                        DEPLOY_DOMAIN = MAIN_DOMAIN
                        DEPLOY_PATH   = "/var/www/testapp"
                        SERVICE_NAME  = "testapp"
                        SERVICE_PORT  = "5000"
                        DB_NAME       = "gusto_prod"

                    } else if (isDev) {
                        DEPLOY_DOMAIN = DEV_DOMAIN
                        DEPLOY_PATH   = "/var/www/testapp-dev"
                        SERVICE_NAME  = "testapp-dev"
                        SERVICE_PORT  = "5001"
                        DB_NAME       = "gusto_dev"

                    } else if (isPratha) {
                        DEPLOY_DOMAIN = PRATHA_DOMAIN
                        DEPLOY_PATH   = "/var/www/testapp-prathamesh"
                        SERVICE_NAME  = "testapp-prathamesh"
                        SERVICE_PORT  = "5002"
                        DB_NAME       = "gusto_prathamesh"

                    } else {
                        // All other feature branches → testsdev.duckdns.org
                        DEPLOY_DOMAIN = DEV_DOMAIN
                        safe = branch.toLowerCase().replaceAll(/[^a-z0-9]/, "-")
                        DEPLOY_PATH   = "/var/www/testapp-${safe}"
                        SERVICE_NAME  = "testapp-${safe}"
                        SERVICE_PORT  = "${5100 + Math.abs(safe.hashCode()) % 100}"
                        DB_NAME       = "gusto_${safe}"
                    }

                    echo """
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Branch:        ${branch}
Domain:        ${DEPLOY_DOMAIN}
Service Name:  ${SERVICE_NAME}
Service Port:  ${SERVICE_PORT}
Deploy Path:   ${DEPLOY_PATH}
Database Name: ${DB_NAME}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
"""
                }
            }
        }

        stage('Checkout Code') {
            steps {
                checkout scm
            }
        }

        stage('Stop Old Service') {
            steps {
                script {
                    sh """
                        sudo systemctl stop ${SERVICE_NAME} || true
                        sudo pkill -9 -f ${SERVICE_NAME}.dll || true
                        sleep 1
                    """
                }
            }
        }

        stage('Build Backend (.NET)') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {

                    sh "dotnet restore testapp.sln"

                    sh "dotnet publish testapp.Server -c Release -o publish_temp"

                    sh """
                        cat > publish_temp/appsettings.Production.json <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Urls": "http://localhost:${SERVICE_PORT}",
  "ConnectionStrings": {
    "DefaultConnection": "Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};Encrypt=True;TrustServerCertificate=True;"
  }
}
EOF
                    """
                }
            }
        }

        stage('Deploy Backend') {
            steps {
                script {
                    sh """
                        sudo mkdir -p ${DEPLOY_PATH}/api
                        sudo rm -rf ${DEPLOY_PATH}/api/*
                        sudo cp -r publish_temp/* ${DEPLOY_PATH}/api/
                        sudo chown -R www-data:www-data ${DEPLOY_PATH}/api
                    """
                }
            }
        }

        stage('Build Angular UI') {
            steps {
                dir("testapp.client") {
                    sh "npm install"

                    sh """
                        cat > src/environments/environment.prod.ts <<EOF
export const environment = {
    production: true,
    apiUrl: "https://${DEPLOY_DOMAIN}/api"
};
EOF
                    """

                    sh "npm run build -- --configuration production"
                }
            }
        }

        stage('Deploy Angular UI') {
            steps {
                script {
                    sh """
                        sudo mkdir -p ${DEPLOY_PATH}/ui
                        sudo rm -rf ${DEPLOY_PATH}/ui/*

                        if [ -d testapp.client/dist/testapp.client/browser ]; then
                           sudo cp -r testapp.client/dist/testapp.client/browser/* ${DEPLOY_PATH}/ui/
                        else
                           sudo cp -r testapp.client/dist/testapp.client/* ${DEPLOY_PATH}/ui/
                        fi

                        sudo chown -R www-data:www-data ${DEPLOY_PATH}/ui
                    """
                }
            }
        }

        stage('Create Systemd Service') {
            steps {
                script {
                    sh """
                        sudo tee /etc/systemd/system/${SERVICE_NAME}.service > /dev/null <<EOF
[Unit]
Description=Branch Deployment: ${SERVICE_NAME}
After=network.target

[Service]
WorkingDirectory=${DEPLOY_PATH}/api
ExecStart=/usr/bin/dotnet ${DEPLOY_PATH}/api/testapp.Server.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
EOF

                        sudo systemctl daemon-reload
                        sudo systemctl enable ${SERVICE_NAME}
                    """
                }
            }
        }

        stage('Configure Nginx') {
            steps {
                script {
                    sh """
                        sudo tee /etc/nginx/sites-available/${SERVICE_NAME} > /dev/null <<EOF
server {
    listen 80;
    server_name ${DEPLOY_DOMAIN};

    root ${DEPLOY_PATH}/ui;
    index index.html;

    location / {
        try_files \$uri \$uri/ /index.html;
    }

    location ^~ /api/ {
        proxy_pass http://127.0.0.1:${SERVICE_PORT};

        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

                        sudo ln -sf /etc/nginx/sites-available/${SERVICE_NAME} /etc/nginx/sites-enabled/${SERVICE_NAME}
                        sudo nginx -t
                        sudo systemctl reload nginx
                    """
                }
            }
        }

        stage('Start Service') {
            steps {
                sh """
                    sudo systemctl start ${SERVICE_NAME}
                    sleep 3
                """
            }
        }

    }

    post {
        success {
            echo "🎉 Deployment Successful → http://${DEPLOY_DOMAIN}"
        }
        failure {
            echo "❌ Deployment Failed"
        }
        always {
            cleanWs()
        }
    }
}
