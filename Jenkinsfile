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

        /* ───────────────────────────────────────────────────────────────
           DETECT BRANCH → DOMAIN → SERVICE → DB
        ────────────────────────────────────────────────────────────────*/
        stage('Detect Branch & Assign Domain') {
            steps {
                script {
                    def branch = env.BRANCH_NAME ?: 'unknown'
                    def isMain = (branch == "main")
                    def isDev = (branch == "dev")
                    def isPratha = branch.toLowerCase().startsWith("prathamesh")

                    if (isMain) {
                        env.DEPLOY_DOMAIN = MAIN_DOMAIN
                        env.DEPLOY_PATH = "/var/www/testapp"
                        env.SERVICE_NAME = "testapp"
                        env.SERVICE_PORT = "5000"
                        env.DB_NAME = "gusto_prod"

                    } else if (isDev) {
                        env.DEPLOY_DOMAIN = DEV_DOMAIN
                        env.DEPLOY_PATH = "/var/www/testapp-dev"
                        env.SERVICE_NAME = "testapp-dev"
                        env.SERVICE_PORT = "5001"
                        env.DB_NAME = "gusto_dev"

                    } else if (isPratha) {
                        env.DEPLOY_DOMAIN = PRATHA_DOMAIN
                        env.DEPLOY_PATH = "/var/www/testapp-prathamesh"
                        env.SERVICE_NAME = "testapp-prathamesh"
                        env.SERVICE_PORT = "5002"
                        env.DB_NAME = "gusto_prathamesh"

                    } else {
                        def safe = branch.toLowerCase().replaceAll(/[^a-z0-9]/, "-")
                        env.DEPLOY_DOMAIN = DEV_DOMAIN
                        env.DEPLOY_PATH = "/var/www/testapp-${safe}"
                        env.SERVICE_NAME = "testapp-${safe}"
                        env.SERVICE_PORT = "${5100 + Math.abs(safe.hashCode()) % 100}"
                        env.DB_NAME = "gusto_${safe}"
                    }

                    echo """
────────────────────────────────────────────
BRANCH      : ${branch}
DOMAIN      : ${env.DEPLOY_DOMAIN}
SERVICE     : ${env.SERVICE_NAME}
PORT        : ${env.SERVICE_PORT}
DB          : ${env.DB_NAME}
────────────────────────────────────────────
"""
                }
            }
        }

        /* ───────────────────────────────────────────────────────────────
           CHECKOUT CODE
        ────────────────────────────────────────────────────────────────*/
        stage('Checkout Code') {
            steps { checkout scm }
        }

        /* ───────────────────────────────────────────────────────────────
           CLEAN WORKSPACE (fixes TAR/Angular errors)
        ────────────────────────────────────────────────────────────────*/
        stage('Clean Old Files') {
            steps {
                sh """
                    rm -rf testapp.client/node_modules
                    rm -rf testapp.client/package-lock.json
                    rm -rf publish_temp
                """
            }
        }

        /* ───────────────────────────────────────────────────────────────
           BUILD BACKEND + FRONTEND
        ────────────────────────────────────────────────────────────────*/
        stage('Build Application') {
            parallel {

                /* ------- Backend -------- */
                stage('Build Backend (.NET)') {
                    steps {
                        sh """
                            dotnet restore testapp.sln
                            dotnet publish testapp.Server -c Release -o publish_temp
                        """
                    }
                }

                /* ------- Angular UI -------- */
                stage('Build Frontend (Angular)') {
                    steps {
                        sh """
                            if [ -d "testapp.client" ]; then
                                cd testapp.client

                                # Full clean and reinstall (fixes TAR_ENTRY_ERROR)
                                rm -rf node_modules package-lock.json
                                npm install

                                # No global ng needed
                                npx --yes @angular/cli@latest build --configuration production

                                cd -
                            fi
                        """
                    }
                }
            }
        }

        /* ───────────────────────────────────────────────────────────────
           DEPLOY BACKEND + FRONTEND
        ────────────────────────────────────────────────────────────────*/
        stage('Deploy Application') {
            parallel {

                /* ------- API -------- */
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

                /* ------- UI -------- */
                stage('Deploy Frontend') {
                    steps {
                        sh """
                            if [ -d "testapp.client/dist" ]; then
                                sudo mkdir -p ${env.DEPLOY_PATH}/ui
                                sudo rm -rf ${env.DEPLOY_PATH}/ui/*

                                # Smart dist detection
                                DIST_PATH=$(find testapp.client/dist -type d -maxdepth 2 | grep browser | head -1)

                                sudo cp -r $DIST_PATH/* ${env.DEPLOY_PATH}/ui/

                                sudo chown -R www-data:www-data ${env.DEPLOY_PATH}/ui
                                sudo chmod -R 755 ${env.DEPLOY_PATH}/ui
                            fi
                        """
                    }
                }
            }
        }

        /* ───────────────────────────────────────────────────────────────
           SYSTEMD + NGINX CONFIGURATION
        ────────────────────────────────────────────────────────────────*/
        stage('Configure Services') {
            parallel {

                /* ------- Systemd -------- */
                stage('Setup Systemd') {
                    steps {
                        script {
                            writeFile file: "service.tmp", text: """[Unit]
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

                            sh """
                                sudo cp service.tmp /etc/systemd/system/${env.SERVICE_NAME}.service
                                sudo systemctl daemon-reload
                                sudo systemctl enable ${env.SERVICE_NAME}
                            """
                        }
                    }
                }

                /* ------- Nginx -------- */
                stage('Setup Nginx') {
                    steps {
                        script {
                            writeFile file: "nginx.tmp", text: """
server {
    listen 80;
    server_name ${env.DEPLOY_DOMAIN};

    root ${env.DEPLOY_PATH}/ui;
    index index.html;

    location / {
        try_files \$uri \$uri/ /index.html;
    }

    location ~ ^/api/ {
        proxy_pass http://127.0.0.1:${env.SERVICE_PORT};
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }

    client_max_body_size 50M;
}
"""

                            sh """
                                sudo cp nginx.tmp /etc/nginx/sites-available/${env.SERVICE_NAME}
                                sudo ln -sf /etc/nginx/sites-available/${env.SERVICE_NAME} /etc/nginx/sites-enabled/${env.SERVICE_NAME}
                                sudo nginx -t
                                sudo systemctl reload nginx
                            """
                        }
                    }
                }
            }
        }

        /* ───────────────────────────────────────────────────────────────
           START & VERIFY
        ────────────────────────────────────────────────────────────────*/
        stage('Start Service') {
            steps {
                sh """
                    sudo systemctl start ${env.SERVICE_NAME}
                    sleep 5
                """
            }
        }

        stage('Verify Deployment') {
            steps {
                sh """
                    sudo systemctl status ${env.SERVICE_NAME} --no-pager || true
                    curl -f http://127.0.0.1:${env.SERVICE_PORT}/api || echo 'API starting...'
                """
            }
        }
    }

    post {
        success {
            echo "✅ DEPLOYED SUCCESSFULLY → http://${env.DEPLOY_DOMAIN}"
            cleanWs()
        }
        failure {
            echo "❌ Deployment failed"
            cleanWs()      // Ensures future builds don’t break
        }
    }
}
