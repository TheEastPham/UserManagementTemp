# Use the official .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test

# Set working directory
WORKDIR /tests

# Copy test project files
COPY tests/IntegrationTests/UserManagement.IntegrationTests/*.csproj ./tests/IntegrationTests/UserManagement.IntegrationTests/

# Copy source code dependencies with correct structure
COPY src/Backend/UserManagement/UserManagement.API/ ./src/Backend/UserManagement/UserManagement.API/
COPY src/Backend/UserManagement/UserManagement.Domain/ ./src/Backend/UserManagement/UserManagement.Domain/
COPY src/Backend/UserManagement/UserManagement.EFCore/ ./src/Backend/UserManagement/UserManagement.EFCore/

# Restore dependencies
RUN dotnet restore ./tests/IntegrationTests/UserManagement.IntegrationTests/

# Copy all test files
COPY tests/IntegrationTests/UserManagement.IntegrationTests/ ./tests/IntegrationTests/UserManagement.IntegrationTests/

# Copy all source files
COPY src/ ./src/

# Build the test project
RUN dotnet build ./tests/IntegrationTests/UserManagement.IntegrationTests/ --no-restore

# Run the tests
ENTRYPOINT ["dotnet", "test", "./tests/IntegrationTests/UserManagement.IntegrationTests/", "--no-build", "--verbosity", "normal", "--logger", "console"]
