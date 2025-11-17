# TestApp - ASP.NET Core + Angular Application

A full-stack web application built with ASP.NET Core 9.0 backend and Angular frontend, featuring user authentication, role-based permissions, reporting, and application logging.

## 🏗️ Project Architecture

This application follows a **Clean Architecture** pattern with layered separation:

```
┌─────────────────┐
│   Angular SPA   │ ← Client-side UI
└─────────────────┘
         │
         ▼ HTTP Requests
┌─────────────────┐
│ ASP.NET Core API│ ← Web API Layer
│   Controllers   │
└─────────────────┘
         │
         ▼ Dependency Injection
┌─────────────────┐
│   Domain Layer  │ ← Business Logic
│    Services     │
└─────────────────┘
         │
         ▼ Repository Pattern
┌─────────────────┐
│   Data Access   │ ← Database Operations
│   Layer (DAL)   │
└─────────────────┘
         │
         ▼ Database Connections
┌─────────────────┐
│   SQL Server    │ ← Primary Database
│   Databases     │
└─────────────────┘
```

## 📁 Project Structure

```
testapp/
├── testapp.sln                           # Solution file
├── testapp.client/                       # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   │   ├── auth/                 # Authentication components
│   │   │   │   ├── guards/               # Route guards
│   │   │   │   ├── Interceptors/         # HTTP interceptors
│   │   │   │   ├── login/                # Login component
│   │   │   │   ├── Models/               # TypeScript models
│   │   │   │   ├── Module/               # Feature modules
│   │   │   │   └── Services/             # Angular services
│   │   │   └── app.module.ts
│   │   ├── proxy.conf.js                 # Development proxy config
│   │   └── main.ts
│   ├── angular.json                      # Angular CLI config
│   └── package.json                      # Node dependencies
├── testapp.Server/                       # ASP.NET Core Backend
│   ├── Controllers/                      # API Controllers
│   │   ├── AuthController.cs            # Authentication endpoints
│   │   ├── MainReportController.cs      # Report endpoints
│   │   ├── AppLogController.cs          # Logging endpoints
│   │   └── WeatherForecastController.cs # Sample controller
│   ├── Config/
│   │   └── JwtSettings.cs               # JWT configuration
│   ├── Properties/
│   │   └── launchSettings.json          # Launch configuration
│   ├── appsettings.json                 # Main configuration
│   ├── appsettings.Development.json     # Development config
│   ├── Program.cs                        # Application entry point
│   └── testapp.Server.csproj            # Project file
├── testapp.Domain/                       # Domain Layer
│   ├── Interfaces/                      # Service interfaces
│   │   ├── IAuthService.cs
│   │   ├── IMainReportService.cs
│   │   └── IAppLogService.cs
│   ├── Models/                          # DTOs and domain models
│   │   ├── LoginRequestDto.cs
│   │   ├── LoginResponseDto.cs
│   │   ├── ReportFilterDto.cs
│   │   └── UserDto.cs
│   ├── Services/                        # Business logic services
│   │   ├── AuthService.cs
│   │   ├── MainReportService.cs
│   │   └── AppLogService.cs
│   ├── Results/
│   │   └── AuthResult.cs                # Service result wrappers
│   └── Utils/
│       └── PasswordHasher.cs            # Password utilities
├── testapp.DAL/                         # Data Access Layer
│   ├── Context/
│   │   └── DapperDbContext.cs           # Database context
│   ├── Interfaces/                      # Repository interfaces
│   │   ├── IUserRepo.cs
│   │   ├── IMainReportRepo.cs
│   │   ├── IAppLogRepo.cs
│   │   ├── ILoginLogRepo.cs
│   │   └── IRolePermissionRepo.cs
│   ├── Models/                          # Database models
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Permission.cs
│   │   ├── RolePermission.cs
│   │   ├── UserRole.cs
│   │   ├── MainReport.cs
│   │   ├── AppLog.cs
│   │   └── LoginLog.cs
│   ├── Repositories/                    # Repository implementations
│   │   ├── UserRepo.cs
│   │   ├── MainReportRepo.cs
│   │   ├── AppLogRepo.cs
│   │   ├── LoginLogRepo.cs
│   │   ├── RolePermissionRepo.cs
│   │   └── SqlQueries.cs                # SQL query definitions
│   └── testapp.DAL.csproj
├── docker/                              # Docker configuration
│   └── sqlserver/
│       └── init/
│           └── 01-init.sql              # Database initialization
├── scripts/
│   └── deploy.sh                        # Deployment script
├── .github/workflows/
│   └── ci-cd.yml                        # GitHub Actions CI/CD
├── Dockerfile                           # Docker build config
├── docker-compose.yml                   # Local Docker setup
├── nginx.conf                           # Nginx configuration
├── appsettings.Production.json          # Production settings
├── testapp.service                      # Systemd service
├── Jenkinsfile                          # Jenkins pipeline
├── Jenkinsfile.vps                      # VPS deployment pipeline
├── complete-deployment-guide.md         # Deployment guide
└── README.md                            # This file
```

## 🔄 Data Flow

### Authentication Flow
```
1. User submits login form (Angular)
2. Angular service sends POST /api/auth/login
3. AuthController receives request
4. AuthService validates credentials
5. UserRepo queries database for user
6. PasswordHasher verifies password
7. JWT token generated and returned
8. LoginLogRepo records login attempt
9. Angular stores JWT token
10. Subsequent requests include JWT in Authorization header
```

### Report Data Flow
```
1. User requests reports (Angular)
2. Angular service sends GET/POST to /api/mainreport
3. MainReportController receives request
4. MainReportService processes business logic
5. MainReportRepo queries report database
6. Data returned through layers
7. JSON response sent to Angular
8. Angular displays data in UI
```

### Logging Flow
```
1. Application events trigger logging
2. Serilog configured to write to database
3. AppLogService handles log operations
4. AppLogRepo inserts into AppLogs table
5. Logs can be retrieved via /api/applog endpoints
```

## 🗄️ Database Schema

### Primary Database (DefaultConnection)
- **Users**: User accounts and basic info
- **Roles**: User roles (Admin, User, etc.)
- **Permissions**: System permissions
- **RolePermissions**: Role-permission mappings
- **UserRoles**: User-role assignments
- **LoginLogs**: Authentication attempt logs

### Report Database (ReportConnection)
- **MainReports**: Report data and analytics

### Logging Database (Configured in Serilog)
- **AppLogs**: Application event logs

## 🚀 How to Run

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+
- SQL Server (local or remote)
- Docker (optional for local testing)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd testapp
   ```

2. **Setup databases**
   - Create two SQL Server databases: `gusto` and `reportDB`
   - Update connection strings in `testapp.Server/appsettings.json`

3. **Run the backend**
   ```bash
   cd testapp.Server
   dotnet run
   ```
   Backend will start on https://localhost:7274

4. **Run the frontend**
   ```bash
   cd testapp.client
   npm install
   npm start
   ```
   Frontend will start on https://localhost:61311

### Docker Development (Recommended)

**Note**: Docker is not currently installed on your system. To use Docker for local development:

1. Install Docker Desktop
2. Run the application:
   ```bash
   docker compose up -d
   ```
   - SQL Server: localhost:1433
   - Application: localhost:5000
   - Nginx: localhost:80

## 🔧 Configuration

### Connection Strings
Update `appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=gusto;User ID=sa;Password=YOUR_PASSWORD;",
    "ReportConnection": "Server=YOUR_SERVER;Database=reportDB;User ID=sa;Password=YOUR_PASSWORD;"
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "Your256BitSecretKeyHere",
    "Issuer": "testapp",
    "Audience": "testapp-users",
    "ExpiryMinutes": 60
  }
}
```

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/all` - Get all users

### Reports
- `GET /api/mainreport/all` - Get all reports
- `POST /api/mainreport/filter` - Filter reports by date

### Logging
- `GET /api/applog/allLogs` - Get all application logs
- `GET /api/applog/daterange` - Get logs by date range
- `GET /api/applog/{id}` - Get log by ID

## 🏷️ Tags and Technologies

### Backend Tags
- `ASP.NET Core 9.0` - Web framework
- `C# 12` - Programming language
- `JWT Authentication` - Token-based auth
- `Dapper` - Micro ORM
- `SQL Server` - Database
- `Serilog` - Logging framework
- `Dependency Injection` - IoC container
- `Clean Architecture` - Software design pattern

### Frontend Tags
- `Angular 19` - Frontend framework
- `TypeScript` - Programming language
- `Angular Material` - UI components
- `Tailwind CSS` - Utility-first CSS
- `RxJS` - Reactive programming

### DevOps Tags
- `Docker` - Containerization
- `Docker Compose` - Multi-container orchestration
- `Nginx` - Web server
- `GitHub Actions` - CI/CD
- `Jenkins` - CI/CD pipeline
- `Systemd` - Service management

### Architecture Tags
- `REST API` - API design
- `Repository Pattern` - Data access pattern
- `Service Layer` - Business logic layer
- `DTO Pattern` - Data transfer objects
- `Role-Based Access Control` - Authorization
- `Microservices Ready` - Scalable architecture

## 🚀 Deployment

### Production Deployment
1. Use `appsettings.Production.json` for production settings
2. Run `dotnet publish` to create deployment package
3. Deploy to VPS with Nginx reverse proxy
4. Use systemd for service management

### CI/CD
- GitHub Actions workflow in `.github/workflows/ci-cd.yml`
- Jenkins pipelines for automated deployment
- Docker-based build and deployment

See `complete-deployment-guide.md` for detailed deployment instructions.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.
