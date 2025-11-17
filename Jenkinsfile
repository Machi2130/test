pipeline {
    agent any

    tools {
        nodejs 'node18'
    }

    environment {
        NODE_HOME = tool 'node18'
        PATH = "${NODE_HOME}/bin:${PATH}"
        DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    }

    stages {

        /* ---------------------------------------------------
            1. CHECKOUT CODE
        --------------------------------------------------- */
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/Machi2130/test'
            }
        }

        /* ---------------------------------------------------
            2. STOP RUNNING SERVICE
        --------------------------------------------------- */
        stage('Stop Service') {
            steps {
                script {
                    echo ">>> Stopping testapp service"
                    sh '''
                        sudo systemctl stop testapp || true
                        sleep 2
                        sudo pkill -9 -f testapp.Server.dll || true
                        sleep 2
                        echo "✔ Service stopped"
                    '''
                }
            }
        }

        /* ---------------------------------------------------
            3. BUILD BACKEND (.NET 9)
        --------------------------------------------------- */
        stage('Build Backend') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {
                    script {
                        echo ">>> Restoring .NET packages"
                        sh "dotnet restore testapp.sln"

                        echo ">>> Publishing .NET backend to TEMP folder"
                        sh "dotnet publish testapp.Server -c Release -o publish_temp"

                        echo ">>> Deploying backend via rsync"
                        sh '''
                            sudo rsync -a --delete publish_temp/ /var/www/testapp/api/
                            sudo chown -R www-data:www-data /var/www/testapp
                            sudo chmod -R 775 /var/www/testapp
                        '''

                        echo ">>> Writing Production config"
                        sh """
                            CONNECTION_STRING=\"Server=${DB_SERVER};Database=gusto;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;\"

                            cat > /var/www/testapp/api/appsettings.Production.json << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "PLACEHOLDER"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
EOF

                            sudo sed -i "s|PLACEHOLDER|${CONNECTION_STRING}|" /var/www/testapp/api/appsettings.Production.json
                            sudo chown www-data:www-data /var/www/testapp/api/appsettings.Production.json
                            sudo chmod 640 /var/www/testapp/api/appsettings.Production.json
                        """

                        echo "✔ Backend deployment done"
                    }
                }
            }
        }

        /* ---------------------------------------------------
            4. BUILD ANGULAR FRONTEND
        --------------------------------------------------- */
        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    script {
                        echo ">>> Installing Angular dependencies"
                        sh "npm install"

                        echo ">>> Building Angular"
                        sh "npm run build -- --configuration production"
                    }
                }

                script {
                    echo ">>> Deploying Angular to /var/www/testapp/ui"
                    sh '''
                        sudo rm -rf /var/www/testapp/ui/*
                        
                        if [ -d "testapp.client/dist/testapp.client/browser" ]; then
                            sudo cp -r testapp.client/dist/testapp.client/browser/* /var/www/testapp/ui/
                        else
                            sudo cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/
                        fi

                        sudo chown -R www-data:www-data /var/www/testapp/ui
                        sudo chmod -R 755 /var/www/testapp/ui

                        echo "✔ Angular deployed"
                    '''
                }
            }
        }

        /* ---------------------------------------------------
            5. OPTIONAL: EF MIGRATIONS
        --------------------------------------------------- */
        stage('Database Migration') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {
                    script {
                        echo ">>> Running EF migrations"
                        sh """
                            cd /var/www/testapp/api

                            export ConnectionStrings__DefaultConnection=\"Server=${DB_SERVER};Database=gusto;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;\"

                            dotnet ef database update --no-build || echo 'ℹ No migrations to apply'
                        """
                    }
                }
            }
        }

        /* ---------------------------------------------------
            6. START SERVICE
        --------------------------------------------------- */
        stage('Start Service') {
            steps {
                script {
                    echo ">>> Starting service"
                    sh '''
                        sudo systemctl start testapp
                        sleep 4
                        
                        echo ">>> Checking service..."
                        sudo systemctl status testapp --no-pager

                        echo ">>> Testing API..."
                        timeout 10 bash -c 'until curl -sf http://127.0.0.1:6000/health || true; do sleep 1; done'

                        echo "✔ API online"
                    '''
                }
            }
        }
    }

    /* ---------------------------------------------------
        POST ACTIONS
    --------------------------------------------------- */
    post {
        success {
            echo "🎉 Deployment successful!"
        }
        failure {
            echo "❌ Deployment failed"
            sh "sudo systemctl start testapp || true"
        }
        always {
            cleanWs()
        }
    }
}
