pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/Machi2130/test'
            }
        }

        stage('Build Backend') {
            steps {
                sh 'dotnet restore testapp.sln'
                sh 'dotnet publish testapp.Server -c Release -o /var/www/testapp/api'
            }
        }

        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    sh 'npm install'
                    sh 'ng build --configuration production'
                }
                sh 'rm -rf /var/www/testapp/ui/*'
                sh 'cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/'
            }
        }

        stage('Restart API') {
            steps {
                sh 'sudo systemctl restart testapp'
            }
        }
    }
}
