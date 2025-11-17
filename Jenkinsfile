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
    }

    stages {

        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/Machi2130/test'
            }
        }

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

        stage('Build Backend') {
            steps {
                withCredentials([
                    string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'),
                    string(credentialsId: 'DB_USER', variable: 'DB_USER'),
                    string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD')
                ]) {

                    script {
                        echo ">>> Restoring .NET packages"
                        sh 'dotnet restore testapp.sln'

                        echo ">>> Publishing .NET backend to TEMP folder"
                        sh 'dotnet publish testapp.Server -c Release -o publish_temp -maxcpucount:1 /p:UseSharedCompilation=false'

                        echo ">>> Writing appsettings.Production.json"
                        sh '''
                            cat > publish_temp/appsettings.Production.json <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "PLACEHOLDER_CONNECTION_STRING"
  }
}
EOF
                        '''

                        sh """
                            CONNECTION_STRING="Server=${DB_SERVER};Database=gusto;User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;Encrypt=True;"
                            sed -i "s|PLACEHOLDER_CONNECTION_STRING|${CONNECTION_STRING}|g" publish_temp/appsettings.Production.json
                        """

                        echo ">>> Copying backend to /var/www/testapp/api"
                        sh '''
                            sudo rm -rf /var/www/testapp/api/*
                            sudo cp -r publish_temp/* /var/www/testapp/api/
                            sudo chown -R www-data:www-data /var/www/testapp/api
                            echo "✔ Backend deployed"
                        '''
                    }
                }
            }
        }

        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    script {
                        echo ">>> Installing Angular dependencies"
                        sh 'npm install'

                        echo ">>> Building Angular"
                        sh 'npm run build -- --configuration production'
                    }
                }

                script {
                    echo ">>> Deploying Angular UI"
                    sh '''
                        sudo rm -rf /var/www/testapp/ui/*
                        cp -r testapp.client/dist/testapp.client/browser/* /var/www/testapp/ui/ || \
                        cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/

                        sudo chown -R www-data:www-data /var/www/testapp/ui
                        sudo chmod -R 755 /var/www/testapp/ui
                        echo "✔ UI deployed"
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
                        echo ">>> Running EF Migrations"
                        sh '''
                            cd /var/www/testapp/api

                            export ConnectionStrings__DefaultConnection="Server='${DB_SERVER}';Database=gusto;User Id='${DB_USER}';Password='${DB_PASSWORD}';TrustServerCertificate=True;Encrypt=True;"

                            if [ -f testapp.Server.dll ]; then
                                dotnet ef database update --no-build || echo "No migrations found"
                            fi

                            echo "✔ Migration step complete"
                        '''
                    }
                }
            }
        }

        stage('Start Service') {
            steps {
                script {
                    echo ">>> Starting testapp service"
                    sh '''
                        sudo systemctl start testapp
                        sleep 5
                        echo "✔ Service started"
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "🎉 Deployment successful!"
        }

        failure {
            echo "❌ Deployment failed"
            sh 'sudo systemctl start testapp || true'
        }

        always {
            cleanWs()
        }
    }
}
