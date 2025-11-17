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
                sh """
                echo >>> Restoring .NET packages
                dotnet restore testapp.sln

                echo >>> Publishing backend
                dotnet publish testapp.Server -c Release -o /var/www/testapp/api
                """
            }
        }

        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    sh """
                    echo >>> Installing Angular dependencies
                    npm install

                    echo >>> Building Angular for production
                    ng build --configuration production
                    """
                }

                sh """
                rm -rf /var/www/testapp/ui/*
                cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/
                """
            }
        }

        stage('Restart API') {
            steps {
                sh 'sudo systemctl restart testapp'
            }
        }
    }

    post {
        success {
            echo "🎉 Deployment completed successfully!"
        }
        failure {
            echo "❌ Deployment failed — check console logs."
        }
    }
}
