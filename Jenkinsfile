pipeline {
    agent any

    tools {
        nodejs 'node18'   // Jenkins NodeJS installation name
    }

    environment {
        ASPNETCORE_ENVIRONMENT = "Production"
    }

    stages {

        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/Machi2130/test'
            }
        }

        stage('Build Backend') {
            steps {
                sh """
                    echo ">>> Restoring .NET packages"
                    dotnet restore testapp.sln

                    echo ">>> Publishing backend"
                    dotnet publish testapp.Server -c Release -o /var/www/testapp/api
                """
            }
        }

        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    sh """
                        echo ">>> Installing Angular dependencies"
                        npm install

                        echo ">>> Building Angular for production"
                        ng build --configuration production
                    """
                }

                sh """
                    echo ">>> Clearing old UI files"
                    rm -rf /var/www/testapp/ui/*

                    echo ">>> Copying new Angular build"
                    cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/
                """
            }
        }

        stage('Restart API') {
            steps {
                sh "sudo systemctl restart testapp"
            }
        }
    }

    post {
        success {
            echo "🚀 Deployment successful!"
        }
        failure {
            echo "❌ Deployment failed — check console logs."
        }
    }
}
