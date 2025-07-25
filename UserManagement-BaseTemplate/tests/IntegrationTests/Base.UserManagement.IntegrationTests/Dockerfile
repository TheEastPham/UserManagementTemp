# Use the official .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test

# Set working directory
WORKDIR /tests

# Copy test project files
COPY tests/IntegrationTests/Base.UserManagement.IntegrationTests/*.csproj ./tests/IntegrationTests/Base.UserManagement.IntegrationTests/

# Copy source code dependencies with correct structure
COPY src/Backend/UserManagement/Base.UserManagement.API/ ./src/Backend/UserManagement/Base.UserManagement.API/
COPY src/Backend/UserManagement/Base.UserManagement.Domain/ ./src/Backend/UserManagement/Base.UserManagement.Domain/
COPY src/Backend/UserManagement/Base.UserManagement.EFCore/ ./src/Backend/UserManagement/Base.UserManagement.EFCore/

# Restore dependencies
RUN dotnet restore ./tests/IntegrationTests/Base.UserManagement.IntegrationTests/

# Copy all test files
COPY tests/IntegrationTests/Base.UserManagement.IntegrationTests/ ./tests/IntegrationTests/Base.UserManagement.IntegrationTests/

# Copy all source files
COPY src/ ./src/

# Build the test project
RUN dotnet build ./tests/IntegrationTests/Base.UserManagement.IntegrationTests/ --no-restore

# Run the tests
ENTRYPOINT ["dotnet", "test", "./tests/IntegrationTests/Base.UserManagement.IntegrationTests/", "--no-build", "--verbosity", "normal", "--logger", "console"]
