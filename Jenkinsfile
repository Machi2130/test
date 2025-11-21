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
                    def branch = env.BRANCH_NAME ?: 'unknown'
                    def isMain = (branch == "main")
                    def isDev  = (branch == "dev")
                    def isPratha = branch.toLowerCase().startsWith("prathamesh")

                    if (isMain) {
                        env.DEPLOY_DOMAIN = MAIN_DOMAIN
                        env.DEPLOY_PATH   = "/var/www/testapp"
                        env.SERVICE_NAME  = "testapp"
                        env.SERVICE_PORT  = "5000"
                        env.DB_NAME       = "gusto_prod"

                    } else if (isDev) {
                        env.DEPLOY_DOMAIN = DEV_DOMAIN
                        env.DEPLOY_PATH   = "/var/www/testapp-dev"
                        env.SERVICE_NAME  = "testapp-dev"
                        env.SERVICE_PORT  = "5001"
                        env.DB_NAME       = "gusto_dev"

                    } else if (isPratha) {
                        env.DEPLOY_DOMAIN = PRATHA_DOMAIN
                        env.DEPLOY_PATH   = "/var/www/testapp-prathamesh"
                        env.SERVICE_NAME  = "testapp-prathamesh"
                        env.SERVICE_PORT  = "5002"
                        env.DB_NAME       = "gusto_prathamesh"

                    } else {
                        def safe = branch.toLowerCase().replaceAll(/[^a-z0-9]/, "-")
                        env.DEPLOY_DOMAIN = DEV_DOMAIN
                        env.DEPLOY_PATH   = "/var/www/testapp-${safe}"
                        env.SERVICE_NAME  = "testapp-${safe}"
                        env.SERVICE_PORT  = "${5100 + Math.abs(safe.hashCode()) % 100}"
                        env.DB_NAME       = "gusto_${safe}"
                    }
                }
            }
        }

        stage('Checkout Code') {
            steps { checkout scm }
        }

        stage('Stop Old Service') {
            steps {
                sh """
                    sudo systemctl stop ${env.SERVICE_NAME} || true
                    sudo pkill -9 -f ${env.SERVICE_NAME}.dll || true
                    sleep 2
                """
            }
        }

        stage('Build Backend (.NET)') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER',   variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {
                    script {
                        sh """
                            dotnet restore testapp.sln
                            dotnet publish testapp.Server -c Release -o publish_temp
                        """

                        def connString = "Server=${env.DB_SERVER};Database=${env.DB_NAME};User Id=${env.DB_USER};Password=${env.DB_PASSWORD};Encrypt=True;TrustServerCertificate=True;"

                        writeFile file: 'publish_temp/appsettings.Production.json', text: """{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Urls": "http://localhost:${env.SERVICE_PORT}",
  "ConnectionStrings": {
    "DefaultConnection": "${connString}"
  }
}"""
                    }
                }
            }
        }

        stage('Deploy Backend') {
            steps {
                sh """
                    sudo mkdir -p ${env.DEPLOY_PATH}/api
                    sudo rm -rf ${env.DEPLOY_PATH}/api/*
                    sudo cp -r publish_temp/* ${env.DEPLOY_PATH}/api/
                    sudo chown -R www-data:www-data ${env.DEPLOY_PATH}/api
                """
            }
        }

        stage('Build Angular UI') {
            steps {
                sh """
                    if [ -d "testapp.client" ]; then
                        cd testapp.client

                        rm -rf node_modules package-lock.json
                        npm install

                        cat > src/environments/environment.prod.ts <<EOF
export const environment = {
  production: true,
  apiUrl: "https://${env.DEPLOY_DOMAIN}/api"
};
EOF

                        npx --yes @angular/cli@latest build --configuration production
                        cd -
                    fi
                """
            }
        }

        stage('Deploy Angular UI') {
            steps {
                sh """
                    if [ -d "testapp.client/dist" ]; then
                        sudo mkdir -p ${env.DEPLOY_PATH}/ui
                        sudo rm -rf ${env.DEPLOY_PATH}/ui/*

                        DIST_PATH=\$(find testapp.client/dist -type d -name browser | head -1)
                        sudo cp -r \$DIST_PATH/* ${env.DEPLOY_PATH}/ui/

                        sudo chown -R www-data:www-data ${env.DEPLOY_PATH}/ui
                        sudo chmod -R 755 ${env.DEPLOY_PATH}/ui
                    fi
                """
            }
        }

        stage('Create Systemd Service') {
            steps {
                script {
                    def serviceContent = """[Unit]
Description=${env.SERVICE_NAME}
After=network.target

[Service]
WorkingDirectory=${env.DEPLOY_PATH}/api
ExecStart=/usr/bin/dotnet ${env.DEPLOY_PATH}/api/testapp.Server.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
"""

                    writeFile file: 'systemd.service', text: serviceContent

                    sh """
                        sudo cp systemd.service /etc/systemd/system/${env.SERVICE_NAME}.service
                        sudo systemctl daemon-reload
                        sudo systemctl enable ${env.SERVICE_NAME}
                    """
                }
            }
        }

        stage('Configure Nginx') {
            steps {
                script {
                    def nginxConfig = """server {
    listen 80;
    server_name ${env.DEPLOY_DOMAIN};

    root ${env.DEPLOY_PATH}/ui;
    index index.html;

    location / {
        try_files \\$uri \\$uri/ /index.html;
    }

    location ^~ /api/ {
        proxy_pass http://127.0.0.1:${env.SERVICE_PORT};
        proxy_http_version 1.1;
        proxy_set_header Host \\$host;
        proxy_set_header X-Real-IP \\$remote_addr;
        proxy_set_header X-Forwarded-For \\$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \\$scheme;
        proxy_set_header Upgrade \\$http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 300s;
        proxy_send_timeout 300s;
    }

    client_max_body_size 50M;
}
"""

                    writeFile file: "nginx-site.conf", text: nginxConfig

                    sh """
                        sudo cp nginx-site.conf /etc/nginx/sites-available/${env.SERVICE_NAME}
                        sudo ln -sf /etc/nginx/sites-available/${env.SERVICE_NAME} /etc/nginx/sites-enabled/${env.SERVICE_NAME}
                        sudo nginx -t
                        sudo systemctl reload nginx
                    """
                }
            }
        }

        stage('Start Service') {
            steps {
                sh """
                    sudo systemctl start ${env.SERVICE_NAME}
                    sleep 5
                """
            }
        }

        stage('Health Check') {
            steps {
                sh """
                    curl -f http://127.0.0.1:${env.SERVICE_PORT}/api || echo "API booting..."
                """
            }
        }
    }

    post {
        success {
            echo "🎉 DEPLOYMENT SUCCESSFUL — http://${env.DEPLOY_DOMAIN}"
            cleanWs()
        }
        failure {
            echo "❌ Deployment Failed"
        }
    }
}
