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
                script {
                    echo '>>> Restoring .NET packages'
                    sh 'dotnet restore testapp.sln'
                    
                    echo '>>> Publishing backend'
                    sh 'dotnet publish testapp.Server -c Release -o /var/www/testapp/api'
                }
            }
        }
        
        stage('Build Angular') {
            steps {
                dir('testapp.client') {
                    script {
                        echo '>>> Installing Angular dependencies'
                        sh 'npm install'
                        
                        echo '>>> Building Angular for production'
                        sh 'npm run build -- --configuration production'
                    }
                }
                
                script {
                    echo '>>> Deploying Angular build to web directory'
                    sh '''
                        rm -rf /var/www/testapp/ui/*
                        cp -r testapp.client/dist/testapp.client/browser/* /var/www/testapp/ui/ 2>/dev/null || \
                        cp -r testapp.client/dist/testapp.client/* /var/www/testapp/ui/
                    '''
                }
            }
        }
        
        stage('Restart API') {
            steps {
                script {
                    echo '>>> Restarting testapp service'
                    sh 'sudo systemctl restart testapp'
                }
            }
        }
    }
    
    post {
        success {
            echo '🎉 Deployment completed successfully!'
        }
        failure {
            echo '❌ Deployment failed — check console logs.'
        }
        always {
            cleanWs(cleanWhenNotBuilt: false,
                    deleteDirs: true,
                    disableDeferredWipeout: true,
                    notFailBuild: true)
        }
    }
}
