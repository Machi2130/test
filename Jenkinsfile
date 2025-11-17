pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = "/tmp/.dotnet"
        NODE_VERSION = '18'  // Specify Node.js version
    }

    stages {
        stage('Checkout') {
            steps {
                // Checkout the source code
                checkout scm
            }
        }

        stage('Setup .NET') {
            steps {
                // Ensure .NET 9.0 is installed
                sh 'dotnet --version'
                sh 'dotnet tool install --global dotnet-ef --version 9.0.0'
            }
        }

        stage('Setup Node.js') {
            steps {
                // Install Node.js if not available
                sh 'curl -fsSL https://deb.nodesource.com/setup_${NODE_VERSION}.x | sudo -E bash -'
                sh 'sudo apt-get install -y nodejs'
                sh 'node --version'
                sh 'npm --version'
            }
        }

        stage('Restore Dependencies') {
            parallel {
                stage('Restore .NET Dependencies') {
                    steps {
                        // Restore .NET dependencies
                        sh 'dotnet restore testapp.sln'
                    }
                }
                stage('Install Node.js Dependencies') {
                    steps {
                        // Install Angular dependencies
                        dir('testapp.client') {
                            sh 'npm ci'
                        }
                    }
                }
            }
        }

        stage('Build Frontend') {
            steps {
                // Build Angular application for production
                dir('testapp.client') {
                    sh 'npm run build --prod'
                }
            }
        }

        stage('Build Backend') {
            steps {
                // Build the .NET solution
                sh 'dotnet build testapp.sln --configuration Release'
            }
        }

        stage('Run Tests') {
            steps {
                // Run .NET tests (if any)
                sh 'dotnet test testapp.sln --configuration Release --no-build'
            }
        }

        stage('Publish Backend') {
            steps {
                // Publish the .NET application
                sh 'dotnet publish testapp.Server/testapp.Server.csproj -c Release -o ./publish --no-build'
            }
        }

        stage('Archive Artifacts') {
            steps {
                // Archive the published application
                archiveArtifacts artifacts: 'publish/**', fingerprint: true
            }
        }

        stage('Deploy') {
            steps {
                // Deployment steps (customize based on your environment)
                // Example: Copy to deployment server or use Docker
                sh '''
                echo "Deployment step - customize based on your infrastructure"
                # Example commands:
                # scp -r publish/* user@server:/path/to/app
                # docker build -t testapp .
                # docker run -d -p 80:80 testapp
                '''
            }
        }
    }

    post {
        always {
            // Clean up workspace
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
