# Customization Guide

## Adding New User Fields

1. **Update the Model:**
   ```csharp
   // Models/ApplicationUser.cs
   public string? Department { get; set; }
   ```

2. **Create Migration:**
   ```bash
   dotnet ef migrations add AddDepartmentToUser
   dotnet ef database update
   ```

3. **Update DTOs:**
   ```csharp
   // DTOs/UpdateProfileRequest.cs
   public string? Department { get; set; }
   ```

4. **Update Frontend:**
   ```tsx
   // Add to profile form
   <input name="department" ... />
   ```

## Adding New Roles

1. **Update DataSeeder:**
   ```csharp
   await roleManager.CreateAsync(new IdentityRole("Manager"));
   ```

2. **Update Authorization:**
   ```csharp
   [Authorize(Roles = "Admin,Manager")]
   ```

## Extending Admin Portal

1. **Create new page:** src/Frontend/AdminPortal/src/app/newpage/page.tsx
2. **Add to navigation:** Update components/Navigation.tsx
3. **Add API calls:** Create service in lib/services/

## Database Customization

- Models: Models/
- Migrations: dotnet ef migrations add YourMigrationName
- Seed data: Data/DataSeeder.cs

## Frontend Customization

- Theme: 	ailwind.config.js
- Components: components/
- State: lib/store/
- API: lib/services/
