pipeline {
    agent any

    environment {
        DOTNET_ROOT = tool(name: 'dotnet9', type: 'dotnet')
        PATH = "${DOTNET_ROOT}:${PATH}"
        NODEJS = tool(name: 'node18', type: 'nodejs')
        PATH = "${NODEJS}/bin:${PATH}"
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
                    string(credentialsId: 'db-server', variable: 'DB_SERVER'),
                    string(credentialsId: 'db-user',   variable: 'DB_USER'),
                    string(credentialsId: 'db-pass',   variable: 'DB_PASSWORD')
                ]) {

                    script {
                        echo ">>> Restoring .NET"
                        sh "dotnet restore testapp.sln"

                        echo ">>> Publishing backend to TEMP folder"
                        sh """
                            rm -rf publish_temp
                            mkdir publish_temp
                            dotnet publish testapp.Server -c Release -o publish_temp
                        """

                        echo ">>> Updating Production config"
                        sh """
                            sed -i "s|PLACEHOLDER_CONNECTION_STRING|Server=$DB_SERVER;Database=gusto;User Id=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=True;Encrypt=True;|g" publish_temp/appsettings.Production.json
                        """
                    }
                }
            }
        }

        stage('Build Angular') {
            steps {
                dir("testapp.client") {
                    echo ">>> Installing Angular dependencies"
                    sh "npm install"

                    echo ">>> Building Angular"
                    sh "npm run build -- --configuration production"
                }

                script {
                    echo ">>> Copy Angular dist to workspace"
                    sh """
                        rm -rf ng_temp
                        mkdir ng_temp
                        cp -r testapp.client/dist/testapp.client/browser/* ng_temp/
                    """
                }
            }
        }

        stage('Deploy Backend') {
            steps {
                script {
                    echo ">>> Stopping API"
                    sh "sudo systemctl stop testapp || true"

                    echo ">>> Deploying backend"
                    sh """
                        sudo rsync -a --delete publish_temp/ /var/www/testapp/api/
                        sudo chown -R jenkins:www-data /var/www/testapp
                        sudo find /var/www/testapp -type d -exec chmod 775 {} +
                        sudo find /var/www/testapp -type f -exec chmod 664 {} +
                    """

                    echo ">>> Starting API"
                    sh "sudo systemctl start testapp"
                }
            }
        }

        stage('Database Migration') {
            steps {
                script {
                    echo ">>> Running migrations"

                    sh """
                        cd /var/www/testapp/api
                        export ConnectionStrings__DefaultConnection="Server=$DB_SERVER;Database=gusto;User Id=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=True;Encrypt=True;"
                        if [ -f testapp.Server.dll ]; then
                            dotnet ef database update || echo "ℹ️  EF not configured or no migrations"
                        fi
                    """
                }
            }
        }

        stage('Deploy Angular') {
            steps {
                script {
                    echo ">>> Deploying Angular UI"
                    sh """
                        sudo rsync -a --delete ng_temp/ /var/www/testapp/ui/
                        sudo chown -R jenkins:www-data /var/www/testapp/ui
                        sudo chmod -R 755 /var/www/testapp/ui
                    """
                    echo "✓ Angular deployment complete"
                }
            }
        }
    }

    post {
        always {
            echo "Build complete (no workspace cleanup)"
        }
    }
}
