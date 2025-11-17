ike # TestApp - Full-Stack Web Application

TestApp is a full-stack web application built with ASP.NET Core Web API backend and Angular frontend. It features user authentication, role-based permissions, reporting, logging, and user management.

## Architecture Overview

The application follows Clean Architecture principles with the following layers:

- **testapp.Server**: ASP.NET Core Web API (Presentation Layer)
- **testapp.Domain**: Business Logic Layer (Services, Models, Interfaces)
- **testapp.DAL**: Data Access Layer (Repositories, Models, Database Context)
- **testapp.client**: Angular SPA (User Interface)

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server (Two databases: main DB and reports DB)
- **ORM**: Dapper (Micro-ORM)
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog (with MSSQL sink)
- **Validation**: FluentValidation
- **User Agent Parsing**: UAParser

### Frontend
- **Framework**: Angular 19
- **UI Library**: Angular Material
- **Styling**: Custom SCSS with Tailwind CSS
- **HTTP Client**: Angular HttpClient with interceptors

## Project Structure

```
testapp/
├── testapp.sln                                    # Visual Studio Solution file
├── testapp.client/                                 # Angular Frontend
│   ├── angular.json                                # Angular CLI configuration
│   ├── package.json                                # Node.js dependencies
│   ├── src/
│   │   ├── app/
│   │   │   ├── app-routing.module.ts              # Angular routing
│   │   │   ├── app.module.ts                       # Main Angular module
│   │   │   ├── app.component.*                     # Root component
│   │   │   ├── core/                               # Core functionality
│   │   │   │   ├── auth/                           # Authentication components
│   │   │   │   ├── guards/                         # Route guards
│   │   │   │   ├── Interceptors/                   # HTTP interceptors
│   │   │   │   ├── login/                          # Login component
│   │   │   │   ├── Models/                         # Frontend models
│   │   │   │   ├── Module/                         # Feature modules
│   │   │   │   │   ├── dashboard/                  # Dashboard component
│   │   │   │   │   ├── dashboard-layout/           # Layout with navbar
│   │   │   │   │   ├── report/                     # Reports component
│   │   │   │   │   ├── app-log/                    # Application logs component
│   │   │   │   │   ├── all-user/                   # User management component
│   │   │   │   │   └── add-user/                   # Add user component
│   │   │   │   └── Services/                       # Angular services
│   │   │   │       ├── auth.service.ts             # Authentication service
│   │   │   │       └── login.service.ts            # Login API service
│   │   │   └── styles.css                          # Global styles
│   │   ├── index.html                              # Main HTML file
│   │   ├── main.ts                                 # Angular bootstrap
│   │   ├── proxy.conf.js                           # Development proxy config
│   │   └── styles.css                              # Additional styles
│   └── public/                                     # Static assets
├── testapp.Server/                                 # ASP.NET Core API
│   ├── testapp.Server.csproj                       # Project file
│   ├── Program.cs                                  # Application entry point
│   ├── appsettings.json                            # Configuration
│   ├── Controllers/                                # API Controllers
│   │   ├── AuthController.cs                       # Authentication endpoints
│   │   ├── MainReportController.cs                 # Report endpoints
│   │   ├── AppLogController.cs                     # Logging endpoints
│   │   └── WeatherForecastController.cs            # Sample controller
│   ├── Config/                                     # Configuration classes
│   │   └── JwtSettings.cs                          # JWT configuration
│   └── Properties/                                 # Project properties
├── testapp.Domain/                                 # Business Logic Layer
│   ├── testapp.Domain.csproj                       # Project file
│   ├── Interfaces/                                 # Service interfaces
│   │   ├── IAuthService.cs                         # Authentication interface
│   │   ├── IMainReportService.cs                   # Report service interface
│   │   └── IAppLogService.cs                       # Log service interface
│   ├── Models/                                     # Domain models (DTOs)
│   │   ├── LoginRequestDto.cs                      # Login request
│   │   ├── LoginResponseDto.cs                     # Login response
│   │   ├── RegisterRequestDto.cs                   # Registration request
│   │   ├── ReportFilterDto.cs                      # Report filter
│   │   └── UserDto.cs                              # User data transfer object
│   ├── Results/                                    # Result classes
│   │   └── AuthResult.cs                           # Authentication result
│   ├── Services/                                   # Business logic services
│   │   ├── AuthService.cs                          # Authentication service
│   │   ├── MainReportService.cs                    # Report service
│   │   └── AppLogService.cs                        # Log service
│   └── Utils/                                      # Utility classes
│       └── PasswordHasher.cs                       # Password hashing utility
├── testapp.DAL/                                    # Data Access Layer
│   ├── testapp.DAL.csproj                          # Project file
│   ├── Context/                                    # Database context
│   │   └── DapperDbContext.cs                      # Dapper connection factory
│   ├── Interfaces/                                 # Repository interfaces
│   │   ├── IUserRepo.cs                            # User repository interface
│   │   ├── IRolePermissionRepo.cs                  # Role/permission interface
│   │   ├── IMainReportRepo.cs                      # Report repository interface
│   │   ├── IAppLogRepo.cs                          # Log repository interface
│   │   └── ILoginLogRepo.cs                        # Login log interface
│   ├── Models/                                     # Database models
│   │   ├── User.cs                                 # User entity
│   │   ├── Role.cs                                 # Role entity
│   │   ├── Permission.cs                           # Permission entity
│   │   ├── RolePermission.cs                       # Role-permission mapping
│   │   ├── UserRole.cs                             # User-role mapping
│   │   ├── MainReport.cs                           # Report entity
│   │   ├── AppLog.cs                               # Application log entity
│   │   └── LoginLog.cs                             # Login log entity
│   └── Repositories/                               # Data access implementations
│       ├── UserRepo.cs                             # User repository
│       ├── RolePermissionRepo.cs                   # Role/permission repository
│       ├── MainReportRepo.cs                       # Report repository
│       ├── AppLogRepo.cs                           # Log repository
│       ├── LoginLogRepo.cs                         # Login log repository
│       └── SqlQueries.cs                           # SQL query constants
└── README.md                                        # This file
```

## Build Flow

### Development Build

1. **Restore Dependencies**:
   ```bash
   # Restore .NET dependencies
   dotnet restore testapp.sln

   # Install Node.js dependencies
   cd testapp.client
   npm install
   cd ..
   ```

2. **Build Solution**:
   ```bash
   # Build the entire solution
   dotnet build testapp.sln
   ```

3. **Run Application**:
   ```bash
   # Run the server (includes Angular dev server via SpaProxy)
   dotnet run --project testapp.Server
   ```

### Production Build

1. **Build Angular Frontend**:
   ```bash
   cd testapp.client
   npm run build --prod
   cd ..
   ```

2. **Publish .NET Application**:
   ```bash
   dotnet publish testapp.Server -c Release -o ./publish
   ```

## Data Flow

### Authentication Flow
1. User submits login credentials from Angular client
2. Request proxied to `/api/auth/login` endpoint
3. `AuthController.Login()` receives request
4. Controller calls `IAuthService.AuthenticateAsync()`
5. `AuthService` validates credentials using `IUserRepo`
6. Password verification using `PasswordHasher.Verify()`
7. User roles and permissions retrieved via `IRolePermissionRepo`
8. JWT token generated with user claims
9. Login attempt logged via `ILoginLogRepo`
10. Response with token and user data sent back to client
11. Client stores token in localStorage

### Report Retrieval Flow
1. Authenticated user requests reports from Angular client
2. Request proxied to `/api/MainReport/filter` or `/api/MainReport/all`
3. `MainReportController` receives request
4. Controller calls `IMainReportService.GetReportsByDateRangeAsync()`
5. `MainReportService` logs the request and converts dates to IST
6. Service calls `IMainReportRepo.GetReportsByDateRangeAsync()`
7. Repository executes SQL query using Dapper on ReportConnection
8. Data returned up the chain to client

### Logging Flow
1. Application events trigger logging in services
2. Serilog configured to write to MSSQL database
3. Logs stored in AppLogs table via MSSqlServer sink
4. Logs can be retrieved via `AppLogController` endpoints

### Database Connections
- **DefaultConnection**: Main application database (user management, roles, permissions, login logs)
- **ReportConnection**: Reports database (main reports, application logs)

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/all` - Get all users

### Reports
- `GET /api/MainReport/all` - Get all reports
- `POST /api/MainReport/filter` - Get reports by date range

### Application Logs
- `GET /api/AppLog/allLogs` - Get all application logs
- `GET /api/AppLog/daterange` - Get logs by date range
- `GET /api/AppLog/{id}` - Get log by ID

## Configuration

### Database Connections (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=103.154.233.202,1433;Database=gusto;User ID=sa;Password=#Machi@5500;Encrypt=True;TrustServerCertificate=True;",
    "ReportConnection": "Server=GANESH\\SQLEXPRESS;Database=reportDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "Issuer": "GustoServer",
    "Audience": "GustoClient",
    "SecretKey": "ReplaceWithASecureRandomStringOfAtLeast32Chars!",
    "ExpiryMinutes": 20
  }
}
```

## Development Setup

1. **Prerequisites**:
   - .NET 9.0 SDK
   - Node.js 18+
   - SQL Server (with two databases configured)

2. **Clone and Setup**:
   ```bash
   git clone <repository-url>
   cd testapp
   dotnet restore
   cd testapp.client
   npm install
   cd ..
   ```

3. **Configure Database**:
   - Ensure SQL Server is running
   - Update connection strings in `appsettings.json`
   - Run database migrations if needed

4. **Run in Development**:
   ```bash
   dotnet run --project testapp.Server
   ```
   - Angular dev server will start automatically on port 61311
   - API will be available on https://localhost:7274

## Security Features

- JWT-based authentication
- Role-based authorization
- Password hashing with salt
- HTTPS enforcement
- CORS configuration
- Request logging with user agent parsing
- IP address tracking

## Logging

- Structured logging with Serilog
- MSSQL database sink for persistent storage
- Request/response logging
- User activity tracking
- Error logging with context

## Deployment

1. Build the application for production
2. Configure production database connections
3. Set secure JWT secret key
4. Deploy to web server (IIS, Nginx, etc.)
5. Configure SSL certificates
6. Set up database backups

## Contributing

1. Follow the Clean Architecture principles
2. Use dependency injection
3. Write unit tests for new features
4. Update documentation for API changes
5. Follow naming conventions

## License

[Add license information here]
