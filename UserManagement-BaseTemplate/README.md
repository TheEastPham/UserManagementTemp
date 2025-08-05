﻿# Project UserManagement Template

This is a ready-to-use template for web applications with:
- âœ… User Management System (.NET Core)
- âœ… Admin Portal (Next.js)
- âœ… Authentication & Authorization
- âœ… Docker Infrastructure
- âœ… Database Migrations

## Quick Start

1. **Clone this template:**
   ```bash
   git clone <this-repo> MyNewProject
   cd MyNewProject
   ```

2. **Rename project:**
   ```powershell
   .\scripts\setup-new-project.ps1 -ProjectName "MyProject" -CompanyName "MyCompany"
   ```

3. **Setup environment:**
   ```bash
   cp .env.template .env
   # Edit .env with your configurations
   ```

4. **Start infrastructure:**
   ```bash
   docker-compose up -d
   .\scripts\setup-database.ps1
   ```

5. **Run application:**
   ```bash
   # Backend
   cd src/Backend
   dotnet run

   # Frontend  
   cd src/Frontend/AdminPortal
   npm install
   npm run dev
   ```

## Features

### ðŸ” Authentication
- JWT token authentication
- Role-based authorization
- External providers (Google, Facebook)
- Password reset & email confirmation

### ðŸ‘¥ User Management
- User registration/login
- Profile management
- Role assignment
- User activity tracking

### ðŸŽ› Admin Portal
- User management dashboard
- Role & permission management
- System monitoring
- Responsive design

### ðŸ³ Infrastructure
- Docker Compose setup
- SQL Server database
- Redis caching
- Health checks

## Customization

See docs/CUSTOMIZATION.md for detailed instructions on:
- Adding new user fields
- Creating new roles
- Extending the admin portal
- Adding new features

## Tech Stack

**Backend:**
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Identity
- JWT Authentication

**Frontend:**
- Next.js 14+ (App Router)
- React 18
- TypeScript
- Tailwind CSS
- Zustand (State Management)

**Infrastructure:**
- Docker & Docker Compose
- SQL Server
- Redis
- Nginx (for production)

## Support

For questions and support, please check the documentation in the docs/ folder.
