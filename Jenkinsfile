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

                    echo "---- Branch: ${branch} ----"
                    echo "Domain: ${env.DEPLOY_DOMAIN}"
                    echo "Service: ${env.SERVICE_NAME} on ${env.SERVICE_PORT}"
                    echo "Deploy path: ${env.DEPLOY_PATH}"
                    echo "DB name: ${env.DB_NAME}"
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
                    sh '''
                        set -e
                        sudo systemctl stop ${SERVICE_NAME} || true
                        sudo pkill -9 -f ${SERVICE_NAME}.dll || true
                        sleep 1
                    '''
                }
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
                        // restore and publish single-threaded to avoid MSBuild child-node crashes
                        sh '''
                            set -e
                            dotnet restore testapp.sln
                            dotnet publish testapp.Server -c Release -o publish_temp -maxcpucount:1 /p:UseSharedCompilation=false
                        '''

                        // write production appsettings using shell env vars (safer)
                        sh '''
                            set -e
                            mkdir -p publish_temp
                            cat > publish_temp/appsettings.Production.json <<'APPSETTINGS'
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
APPSETTINGS
                        '''
                    }
                }
            }
        }

        stage('Deploy Backend') {
            steps {
                script {
                    sh '''
                        set -e
                        sudo mkdir -p ${DEPLOY_PATH}/api
                        sudo rm -rf ${DEPLOY_PATH}/api/*
                        sudo cp -r publish_temp/* ${DEPLOY_PATH}/api/
                        sudo chown -R www-data:www-data ${DEPLOY_PATH}/api
                    '''
                }
            }
        }

        stage('Build Angular UI (if present)') {
            steps {
                script {
                    sh '''
                        set -e
                        if [ -d "testapp.client" ]; then
                            echo "Found frontend: testapp.client — building..."
                            cd testapp.client

                            # install dependencies (local @angular/cli will be used if present)
                            npm ci || npm install

                            # ensure environment folder exists (works for Angular v12..v17+)
                            mkdir -p src/environments

                            # write a lean environment.prod that most Angular setups will use
                            cat > src/environments/environment.prod.ts <<'ENV'
export const environment = {
  production: true,
  apiUrl: "https://${DEPLOY_DOMAIN}/api"
};
ENV

                            # run the build script (uses local CLI)
                            npm run build -- --configuration production || npm run build
                            cd -
                        else
                            echo "Frontend folder testapp.client not found — skipping UI build"
                        fi
                    '''
                }
            }
        }

        stage('Deploy Angular UI (if built)') {
            steps {
                script {
                    sh '''
                        set -e
                        if [ -d "testapp.client" ] && [ -d "testapp.client/dist" ]; then
                            sudo mkdir -p ${DEPLOY_PATH}/ui
                            sudo rm -rf ${DEPLOY_PATH}/ui/*
                            if [ -d testapp.client/dist/testapp.client/browser ]; then
                                sudo cp -r testapp.client/dist/testapp.client/browser/* ${DEPLOY_PATH}/ui/
                            else
                                # pick the first dist folder
                                first_dist=$(ls -1 testapp.client/dist | head -n1)
                                sudo cp -r testapp.client/dist/${first_dist}/* ${DEPLOY_PATH}/ui/ || true
                            fi
                            sudo chown -R www-data:www-data ${DEPLOY_PATH}/ui
                            sudo chmod -R 755 ${DEPLOY_PATH}/ui
                        else
                            echo "No built UI to deploy"
                        fi
                    '''
                }
            }
        }

        stage('Create Systemd Service') {
            steps {
                script {
                    sh '''
                        set -e
                        sudo tee /etc/systemd/system/${SERVICE_NAME}.service > /dev/null <<'SERVICE'
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
SERVICE

                        sudo systemctl daemon-reload
                        sudo systemctl enable ${SERVICE_NAME} || true
                    '''
                }
            }
        }

        stage('Configure Nginx') {
            steps {
                script {
                    sh '''
                        set -e
                        sudo tee /etc/nginx/sites-available/${SERVICE_NAME} > /dev/null <<'NG'
server {
    listen 80;
    server_name ${DEPLOY_DOMAIN};

    root ${DEPLOY_PATH}/ui;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }

    location ^~ /api/ {
        proxy_pass http://127.0.0.1:${SERVICE_PORT};
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 300s;
        proxy_send_timeout 300s;
    }

    client_max_body_size 50M;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
}
NG

                        sudo ln -sf /etc/nginx/sites-available/${SERVICE_NAME} /etc/nginx/sites-enabled/${SERVICE_NAME}
                        sudo nginx -t
                        sudo systemctl reload nginx
                    '''
                }
            }
        }

        stage('Start Service') {
            steps {
                script {
                    sh '''
                        set -e
                        sudo systemctl start ${SERVICE_NAME} || true
                        sleep 3
                    '''
                }
            }
        }

        stage('Health Check (quick)') {
            steps {
                script {
                    sh '''
                        set +e
                        for i in 1 2 3 4 5; do
                            curl -sSf http://127.0.0.1:${SERVICE_PORT}/ || break
                            sleep 1
                        done
                        set -e
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "🎉 Deployment successful → http://${env.DEPLOY_DOMAIN}"
            // Clean workspace only on success so failed builds keep files for debugging
            cleanWs()
        }

        failure {
            echo "❌ Deployment failed — workspace kept for inspection"
            // do not clean workspace so you can inspect logs and files
        }
    }
}
