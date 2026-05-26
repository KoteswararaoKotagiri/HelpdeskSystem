# HelpDesk System

![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=.net)
![Language](https://img.shields.io/badge/Language-C%23-239120?style=flat-square&logo=csharp)
![Database](https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=flat-square&logo=microsoft-sql-server)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

A modern, scalable HelpDesk management system built with ASP.NET Core 9.0, Entity Framework Core, and JWT authentication. This enterprise-grade API provides comprehensive ticket management, user authentication, role-based access control, and departmental organization.

## 📋 Table of Contents

- [Project Description](#project-description)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture Overview](#architecture-overview)
- [Folder Structure](#folder-structure)
- [Prerequisites](#prerequisites)
- [Installation Steps](#installation-steps)
- [Environment Variables & Configuration](#environment-variables--configuration)
- [Database Setup & Migration Commands](#database-setup--migration-commands)
- [Running the Project](#running-the-project)
- [API Endpoints Summary](#api-endpoints-summary)
- [Authentication Details](#authentication-details)
- [Example Request/Response](#example-requestresponse)
- [Error Handling](#error-handling)
- [Logging](#logging)
- [Testing Instructions](#testing-instructions)
- [Deployment Notes](#deployment-notes)
- [Security Best Practices](#security-best-practices)
- [Performance Optimizations](#performance-optimizations)
- [Future Improvements](#future-improvements)
- [Contributing Guidelines](#contributing-guidelines)

## 📝 Project Description

The HelpDesk System is an enterprise-level ticketing and support management platform designed to streamline issue tracking, resolution, and team collaboration. It supports role-based access control with three primary roles: Admin, Support Engineer, and Employee. The system manages tickets across multiple departments with priority and status tracking, SLA management, and audit trails for compliance.

### Key Capabilities:
- **Ticket Lifecycle Management**: Create, track, and resolve support tickets
- **Role-Based Access Control**: Admin, Support Engineer, and Employee roles
- **Department Management**: Organize users and tickets by departments
- **Priority & Status Tracking**: SLA-based priority levels (Low, Medium, High, Critical)
- **Audit Trail**: Track all changes with CreatedBy/ModifiedBy and timestamps
- **JWT Authentication**: Secure token-based authentication

---

## ✨ Features

### Authentication & Authorization
- ✅ User registration with email verification
- ✅ Secure login with JWT token generation
- ✅ Role-based authorization (Admin, Support Engineer, Employee)
- ✅ Token expiration and refresh capability (60-minute default)
- ✅ BCrypt password hashing for enhanced security

### Ticket Management
- ✅ Create and manage support tickets
- ✅ Assign tickets to support engineers
- ✅ Track ticket status (Open, In Progress, Resolved)
- ✅ Categorize tickets by type
- ✅ Set priority levels with SLA hours
- ✅ Define due dates and track completion

### Master Data Management
- ✅ Role management
- ✅ Department management
- ✅ Ticket status configuration
- ✅ Priority level management
- ✅ Ticket category management
- ✅ Color-coded status indicators

### Data Management
- ✅ Soft delete support (IsDeleted flag)
- ✅ Comprehensive audit trails (CreatedBy/ModifiedBy)
- ✅ Database seeding with default values
- ✅ Entity Framework Core migrations

### API Features
- ✅ Swagger/OpenAPI documentation
- ✅ RESTful API design
- ✅ Asynchronous operations
- ✅ Proper HTTP status codes
- ✅ Structured error responses

---

## 🛠 Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | ASP.NET Core | 9.0 |
| **Language** | C# | Latest |
| **Database** | SQL Server | Latest |
| **ORM** | Entity Framework Core | 9.0.16 |
| **Authentication** | JWT Bearer Tokens | OpenID Standard |
| **API Documentation** | Swagger/OpenAPI | 9.0.6 |
| **Password Hashing** | BCrypt.Net-Next | 4.2.0 |
| **Validation** | FluentValidation | 9.3.0 |
| **Mapping** | AutoMapper | 11.0.0 |
| **Mediation** | MediatR | 9.0.0 |
| **Real-time** | SignalR | 1.2.10 |
| **Logging** | Serilog | 9.0.0 |

---

## 🏗 Architecture Overview

The HelpDesk System follows **Clean Architecture** principles with strict separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   Helpdesk.API (Presentation)            │
│         Controllers, DTOs, Middleware, Filters           │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────┴──────────────────────────────────┐
│             Helpdesk.Application (Application)           │
│    Features, DTOs, Interfaces, MediatR Handlers         │
└──────────────────────┬──────────────────────────────────┘
                       │
    ┌──────────────────┼──────────────────┐
    │                  │                  │
┌───▼────────────┐ ┌──▼────────────┐ ┌──▼──────────────────┐
│  Helpdesk.     │ │Helpdesk.      │ │Helpdesk.Persistence│
│  Domain        │ │Infrastructure │ │   (Data Access)    │
│ (Entities,     │ │(Services,Auth)│ │(DbContext, Repos)  │
│  Rules)        │ │               │ │                    │
└────────────────┘ └───────────────┘ └────────────────────┘
                       │
                ┌──────▼──────┐
                │ SQL Server  │
                └─────────────┘
```

### Layer Responsibilities:

**Helpdesk.API**
- REST API controllers
- Request/response handling
- Authentication middleware
- Swagger configuration
- Program.cs (dependency injection, service configuration)

**Helpdesk.Application**
- Business logic
- DTOs (Data Transfer Objects)
- Service interfaces
- Feature organization
- Validation logic

**Helpdesk.Domain**
- Core entities
- Domain rules
- Base entity class
- No external dependencies

**Helpdesk.Infrastructure**
- JWT authentication service
- Password hashing utility
- External service implementations

**Helpdesk.Persistence**
- DbContext configuration
- Entity Framework migrations
- Database seed data
- Repository implementations

---

## 📂 Folder Structure

```
HelpdeskSystem/
├── Helpdesk.API/
│   ├── Controllers/
│   │   ├── AuthController.cs          # Authentication endpoints
│   │   ├── MasterController.cs        # Master data endpoints
│   │   ├── TicketController.cs        # Ticket management (placeholder)
│   │   └── WeatherForecastController.cs
│   ├── Middleware/
│   │   └── MiddlewareMarker.cs        # Custom middleware placeholder
│   ├── Filters/
│   │   └── FiltersMarker.cs           # Action filters placeholder
│   ├── Program.cs                     # Application startup & DI configuration
│   ├── appsettings.json               # Configuration settings
│   ├── appsettings.Development.json   # Development-specific settings
│   └── Helpdesk.API.csproj
│
├── Helpdesk.Domain/
│   ├── Entities/
│   │   ├── Identity/
│   │   │   └── User.cs                # User entity with role & department
│   │   ├── Masters/
│   │   │   ├── Role.cs                # Role definition
│   │   │   ├── Department.cs          # Department organization
│   │   │   ├── Permission.cs          # Permission definition
│   │   │   ├── TicketCategory.cs      # Ticket categories
│   │   │   ├── TicketStatus.cs        # Ticket status options
│   │   │   └── TicketPriority.cs      # Priority levels with SLA
│   │   └── Tickets/
│   │       └── Ticket.cs              # Ticket entity
│   ├── Common/
│   │   └── BaseEntity.cs              # Base class with audit fields
│   └── Helpdesk.Domain.csproj
│
├── Helpdesk.Application/
│   ├── Interfaces/
│   │   └── IJwtService.cs             # JWT token generation
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── DTOs/
│   │   │   │   ├── RegisterRequestDto.cs
│   │   │   │   ├── LoginRequestDto.cs
│   │   │   │   └── LoginResponseDto.cs
│   │   │   ├── Queries/
│   │   │   │   └── QueriesMarker.cs
│   │   │   └── Commands/
│   │   └── Masters/
│   │       └── MastersMarker.cs
│   └── Helpdesk.Application.csproj
│
├── Helpdesk.Infrastructure/
│   ├── Authentication/
│   │   ├── JwtService.cs              # JWT token generation implementation
│   │   ├── JwtSettings.cs             # JWT configuration class
│   │   └── PasswordHasher.cs          # BCrypt password hashing
│   ├── Services/
│   │   └── ServicesMarker.cs          # Service implementations placeholder
│   └── Helpdesk.Infrastructure.csproj
│
├── Helpdesk.Persistence/
│   ├── Contexts/
│   │   └── HelpdeskDbContext.cs       # Entity Framework DbContext
│   ├── Migrations/
│   │   ├── 20260520163658_InitialCreate.cs
│   │   ├── 20260520163658_InitialCreate.Designer.cs
│   │   └── HelpdeskDbContextModelSnapshot.cs
│   ├── Seed/
│   │   └── DbInitializer.cs           # Database seeding logic
│   └── Helpdesk.Persistence.csproj
│
├── HelpdeskSystem/ (Legacy/Sample project)
│   ├── Controllers/
│   │   └── WeatherForecastController.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── HelpdeskSystem.sln                 # Visual Studio Solution
├── .gitignore
└── .gitattributes
```

---

## 📋 Prerequisites

### System Requirements
- **Operating System**: Windows, macOS, or Linux
- **RAM**: Minimum 4 GB (8 GB recommended)
- **Storage**: 2 GB free space

### Required Software
- **.NET SDK 9.0** or later ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **SQL Server 2019 or later** (LocalDB, Express, or full version)
- **Visual Studio 2022** (Community or Professional) OR **VS Code**
- **Git** for version control
- **Postman** or **Insomnia** for API testing (optional)

### Installation Verification

```bash
# Verify .NET installation
dotnet --version

# Verify SQL Server is running (Windows)
sqlcmd -S . -Q "SELECT @@VERSION"

# Or for SQL Server LocalDB
SqlLocalDB info
```

---

## 🚀 Installation Steps

### 1. Clone the Repository

```bash
git clone https://github.com/KoteswararaoKotagiri/HelpdeskSystem.git
cd HelpdeskSystem
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Configure Database Connection

Edit `Helpdesk.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=HelpdeskDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Connection String Options:**

| Environment | Connection String |
|------------|-------------------|
| **LocalDB** | `Server=(localdb)\mssqllocaldb;Database=HelpdeskDb;Integrated Security=true;` |
| **Local Instance** | `Server=.;Database=HelpdeskDb;Trusted_Connection=True;TrustServerCertificate=True` |
| **Express Edition** | `Server=.\SQLEXPRESS;Database=HelpdeskDb;Integrated Security=true;` |
| **Remote Server** | `Server=192.168.1.100;Database=HelpdeskDb;User Id=sa;Password=YourPassword;` |

### 4. Apply Database Migrations

```bash
cd Helpdesk.API
dotnet ef database update --project ../Helpdesk.Persistence
```

This command:
- Creates the HelpdeskDb database
- Applies all migrations
- Seeds initial data (Roles, Departments, Statuses, Priorities)

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run the Application

```bash
cd Helpdesk.API
dotnet run
```

The API will start at: `https://localhost:7000` or `http://localhost:5000`

Access Swagger UI: `https://localhost:7000/swagger`

---

## ⚙️ Environment Variables & Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=HelpdeskDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Key": "THIS_IS_SUPER_SECRET_KEY_123456789",
    "Issuer": "HelpdeskAPI",
    "Audience": "HelpdeskClient",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### JWT Configuration

| Setting | Purpose | Value |
|---------|---------|-------|
| **Key** | Secret key for token signing (min 32 chars) | `THIS_IS_SUPER_SECRET_KEY_123456789` |
| **Issuer** | Token issuer identifier | `HelpdeskAPI` |
| **Audience** | Intended token audience | `HelpdeskClient` |
| **ExpiryMinutes** | Token validity duration | `60` |

⚠️ **IMPORTANT**: Change the JWT Key in production to a strong, unique value!

---

## 🗄️ Database Setup & Migration Commands

### Initial Setup

```bash
# Navigate to Helpdesk.API directory
cd Helpdesk.API

# Create initial migration (if needed)
dotnet ef migrations add InitialCreate --project ../Helpdesk.Persistence

# Update database with migration
dotnet ef database update --project ../Helpdesk.Persistence
```

### Common Database Commands

```bash
# View pending migrations
dotnet ef migrations list --project ../Helpdesk.Persistence

# Revert last migration
dotnet ef database update <PreviousMigrationName> --project ../Helpdesk.Persistence

# Drop database
dotnet ef database drop --project ../Helpdesk.Persistence

# Generate script for migrations
dotnet ef migrations script --output migration.sql --project ../Helpdesk.Persistence

# Add new migration
dotnet ef migrations add <MigrationName> --project ../Helpdesk.Persistence
```

### Database Schema

**Core Tables**:
- **Users**: User accounts with roles and departments
- **Roles**: User roles (Admin, Support Engineer, Employee)
- **Departments**: Organizational departments
- **Tickets**: Support tickets with status, priority, and category
- **TicketStatuses**: Status definitions (Open, In Progress, Resolved)
- **TicketPriorities**: Priority levels (Low, Medium, High, Critical)
- **TicketCategories**: Ticket classifications

### Seed Data

Default seed data automatically inserted on first run:

**Roles**: Admin, Support Engineer, Employee

**Departments**: IT Support, Human Resources, Finance

**Ticket Statuses**: Open, In Progress, Resolved

**Ticket Priorities**: Low, Medium, High, Critical

---

## ▶️ Running the Project

### Using Visual Studio 2022

1. Open `HelpdeskSystem.sln`
2. Set `Helpdesk.API` as startup project
3. Press `F5` or click **Run**
4. Navigate to `https://localhost:7000/swagger`

### Using .NET CLI

```bash
cd Helpdesk.API
dotnet run
```

---

## 📡 API Endpoints Summary

### Authentication Endpoints

#### 1. **User Registration**
```
POST /api/auth/register
Content-Type: application/json

Request Body:
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "roleId": "guid-of-role",
  "departmentId": "guid-of-department"
}

Response: 200 OK
{
  "message": "User registered successfully."
}
```

#### 2. **User Login**
```
POST /api/auth/login
Content-Type: application/json

Request Body:
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-05-26T15:30:00Z",
  "userName": "John Doe",
  "role": "Support Engineer"
}
```

### Master Data Endpoints

#### 3. **Get Roles**
```
GET /api/master/roles
Authorization: Bearer {token}
```

#### 4. **Get Departments**
```
GET /api/master/departments
Authorization: Bearer {token}
```

#### 5. **Get Ticket Statuses**
```
GET /api/master/ticket-statuses
Authorization: Bearer {token}
```

#### 6. **Get Ticket Priorities**
```
GET /api/master/ticket-priorities
Authorization: Bearer {token}
```

---

## 🔐 Authentication Details

### JWT Token Structure

Tokens include claims for user identification, role, and expiration:
- `sub`: User ID
- `email`: User email
- `role`: User role
- `FirstName`: User's first name
- `exp`: Expiration time
- `iss`: Issuer (HelpdeskAPI)
- `aud`: Audience (HelpdeskClient)

### Token Usage

Include the token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🧪 Testing Instructions

### Manual Testing with Swagger

1. Open `https://localhost:7000/swagger`
2. Click on Auth endpoints
3. Execute requests directly in UI

### Testing with cURL

```bash
# Register
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"test@test.com","password":"Test123!","roleId":"role-id","departmentId":"dept-id"}'

# Login
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}'

# Access protected endpoint
curl -X GET "https://localhost:7000/api/master/roles" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 🚀 Deployment Notes

### Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Update `appsettings.Production.json`
- [ ] Change JWT Key to strong, random value
- [ ] Configure CORS policy
- [ ] Set connection string to production database
- [ ] Enable HTTPS only
- [ ] Configure logging to file/cloud
- [ ] Run database migrations
- [ ] Test all endpoints

---

## 🔒 Security Best Practices

### Implemented

✅ Password Hashing with BCrypt  
✅ JWT Token-based Authentication  
✅ HTTPS Enforcement  
✅ SQL Injection Prevention via EF Core  
✅ Authorization on Protected Endpoints  

### Recommended Additions

- Global exception handling middleware
- Rate limiting
- Input validation with FluentValidation
- CORS configuration
- Data encryption for sensitive fields
- Security headers

---

## ⚡ Performance Optimizations

### Current Implementation
- Asynchronous operations (async/await)
- Entity Framework Core with SQL Server
- JWT stateless authentication

### Recommended Improvements
- Database query optimization with projections
- Redis caching for frequently accessed data
- Pagination for list endpoints
- Database indexes on frequently queried columns
- Async database connection pooling
- Gzip compression for responses

---

## 🔮 Future Improvements

### High Priority
1. Complete Ticket Management CRUD operations
2. User management endpoints
3. Advanced search and filtering
4. Role-based access control implementation
5. Global error handling middleware

### Medium Priority
6. Reporting and analytics dashboard
7. Email notification system
8. File attachment support
9. Comprehensive audit trail
10. Integration capabilities (Slack, Teams, etc.)

### Low Priority
11. Mobile application
12. AI-powered ticket classification
13. Chatbot integration
14. Knowledge base/FAQ system

---

## 👥 Contributing Guidelines

### Code Standards

- Use PascalCase for classes, methods, and properties
- Use _camelCase for private fields
- Add XML documentation comments to public members
- One class per file
- Organize using statements alphabetically

### Pull Request Process

1. Fork the repository
2. Create feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -am 'Add new feature'`
4. Push to branch: `git push origin feature/your-feature`
5. Create Pull Request with description

---

**Project Created**: May 2026  
**Last Updated**: May 26, 2026  
**Status**: Active Development

For the latest updates, visit the [GitHub Repository](https://github.com/KoteswararaoKotagiri/HelpdeskSystem)
