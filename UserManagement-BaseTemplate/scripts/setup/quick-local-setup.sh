#!/bin/bash
# ========================================================================
# Quick Local Development Setup Script
# ========================================================================
# This script provides a quick way to set up local development environment
# with database migrations similar to production workflow

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

function print_colored() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

function show_help() {
    print_colored $BLUE "🗄️ Quick Local Development Setup"
    print_colored $BLUE "================================="
    echo ""
    print_colored $YELLOW "USAGE:"
    echo "  ./quick-local-setup.sh [OPTIONS]"
    echo ""
    print_colored $YELLOW "OPTIONS:"
    echo "  --init                 Initialize database with migrations"
    echo "  --reset                Reset database completely"
    echo "  --migrate              Run pending migrations"
    echo "  --seed                 Seed database with test data"
    echo "  --docker-sql           Use Docker SQL Server (default: LocalDB)"
    echo "  --help                 Show this help message"
    echo ""
    print_colored $YELLOW "EXAMPLES:"
    echo "  ./quick-local-setup.sh --init"
    echo "  ./quick-local-setup.sh --reset --seed"
    echo "  ./quick-local-setup.sh --migrate --docker-sql"
    exit 0
}

# Default values
INIT_DB=false
RESET_DB=false
MIGRATE_DB=false
SEED_DATA=false
USE_DOCKER_SQL=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --init)
            INIT_DB=true
            shift
            ;;
        --reset)
            RESET_DB=true
            shift
            ;;
        --migrate)
            MIGRATE_DB=true
            shift
            ;;
        --seed)
            SEED_DATA=true
            shift
            ;;
        --docker-sql)
            USE_DOCKER_SQL=true
            shift
            ;;
        --help)
            show_help
            ;;
        *)
            print_colored $RED "❌ Unknown option: $1"
            show_help
            ;;
    esac
done

print_colored $BLUE "🚀 Quick Local Development Setup"
print_colored $BLUE "================================="
echo ""

# If no specific action is specified, do a full initialization
if [[ "$INIT_DB" == false && "$RESET_DB" == false && "$MIGRATE_DB" == false ]]; then
    INIT_DB=true
fi

# ========================================================================
# 1. Prerequisites Check
# ========================================================================
print_colored $YELLOW "📋 Checking prerequisites..."

# Check if .NET 8 SDK is installed
if ! dotnet --version | grep -q "8\."; then
    print_colored $RED "❌ .NET 8 SDK is not installed. Please install .NET 8 SDK and try again."
    exit 1
fi

# Check if Docker is running (if using Docker SQL)
if [[ "$USE_DOCKER_SQL" == true ]]; then
    if ! docker info > /dev/null 2>&1; then
        print_colored $RED "❌ Docker is not running. Please start Docker and try again."
        exit 1
    fi
fi

# Check if EF Core tools are installed
if ! dotnet ef --version > /dev/null 2>&1; then
    print_colored $YELLOW "⚠️ Installing EF Core tools..."
    dotnet tool install --global dotnet-ef
    if [[ $? -ne 0 ]]; then
        print_colored $RED "❌ Failed to install EF Core tools"
        exit 1
    fi
fi

print_colored $GREEN "✅ Prerequisites check passed"

# ========================================================================
# 2. Environment Setup
# ========================================================================
print_colored $YELLOW "🔧 Setting up environment..."

# Create .env file if it doesn't exist
if [[ ! -f .env ]]; then
    print_colored $YELLOW "📝 Creating .env file..."
    cat > .env << EOL
# Database Configuration
DB_PASSWORD=YourStrong!Passw0rd
REDIS_PASSWORD=YourRedisPassword123
RABBITMQ_PASSWORD=YourRabbitPassword123

# JWT Configuration
JWT_SECRET_KEY=YourVeryLongSecretKeyThatShouldBeAtLeast32CharactersLongForDev!
JWT_ISSUER=Genealogy.UserManagement.Dev
JWT_AUDIENCE=Genealogy.Client.Dev

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ALLOWED_ORIGINS=http://localhost:3000,https://localhost:3001,http://localhost:5173
EOL
    print_colored $GREEN "✅ .env file created"
fi

# ========================================================================
# 3. Infrastructure Setup
# ========================================================================
if [[ "$USE_DOCKER_SQL" == true ]]; then
    print_colored $YELLOW "🐳 Starting Docker infrastructure..."
    
    # Start Docker services
    docker-compose up -d sqlserver redis rabbitmq
    
    # Wait for SQL Server to be ready
    print_colored $YELLOW "⏳ Waiting for SQL Server to be ready..."
    sleep 15
    
    # Test SQL Server connection
    for i in {1..30}; do
        if docker exec $(docker-compose ps -q sqlserver) /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT 1" > /dev/null 2>&1; then
            print_colored $GREEN "✅ SQL Server is ready"
            break
        fi
        if [[ $i -eq 30 ]]; then
            print_colored $RED "❌ SQL Server failed to start"
            exit 1
        fi
        sleep 2
    done
    
    CONNECTION_STRING="Server=localhost,1433;Database=GenealogyUserManagement_Dev;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
else
    print_colored $YELLOW "🏠 Using LocalDB..."
    CONNECTION_STRING="Server=(localdb)\\mssqllocaldb;Database=GenealogyUserManagement_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
fi

# ========================================================================
# 4. Database Operations
# ========================================================================
cd src/Backend/UserManagement

# Set connection string environment variable
export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"

if [[ "$RESET_DB" == true ]]; then
    print_colored $YELLOW "🗑️ Resetting database..."
    dotnet ef database drop --force --verbose
    print_colored $GREEN "✅ Database reset completed"
fi

if [[ "$INIT_DB" == true ]]; then
    print_colored $YELLOW "🏗️ Initializing database..."
    
    # Check if migrations exist
    if [[ ! -d "Migrations" ]]; then
        print_colored $YELLOW "📝 Creating initial migration..."
        dotnet ef migrations add InitialCreate --verbose
        if [[ $? -ne 0 ]]; then
            print_colored $RED "❌ Failed to create initial migration"
            exit 1
        fi
        print_colored $GREEN "✅ Initial migration created"
    fi
    
    # Apply migrations
    print_colored $YELLOW "⏳ Applying migrations..."
    dotnet ef database update --verbose
    if [[ $? -ne 0 ]]; then
        print_colored $RED "❌ Failed to apply migrations"
        exit 1
    fi
    print_colored $GREEN "✅ Database initialized successfully"
fi

if [[ "$MIGRATE_DB" == true ]]; then
    print_colored $YELLOW "⏳ Running migrations..."
    dotnet ef database update --verbose
    if [[ $? -ne 0 ]]; then
        print_colored $RED "❌ Failed to run migrations"
        exit 1
    fi
    print_colored $GREEN "✅ Migrations applied successfully"
fi

# ========================================================================
# 5. Seed Data
# ========================================================================
if [[ "$SEED_DATA" == true ]]; then
    print_colored $YELLOW "🌱 Seeding database with test data..."
    
    # Create seed SQL script
    cat > temp_seed.sql << 'EOL'
-- Development seed data
USE GenealogyUserManagement_Dev;

-- Insert default roles
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, Description, CreatedAt, IsActive)
    VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', 'System Super Administrator', GETUTCDATE(), 1);
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'FamilyAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, Description, CreatedAt, IsActive)
    VALUES (NEWID(), 'FamilyAdmin', 'FAMILYADMIN', 'Family Administrator', GETUTCDATE(), 1);
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Member')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, Description, CreatedAt, IsActive)
    VALUES (NEWID(), 'Member', 'MEMBER', 'Family Member', GETUTCDATE(), 1);
END

PRINT 'Development seed data completed';
EOL
    
    # Execute seed script
    if [[ "$USE_DOCKER_SQL" == true ]]; then
        docker exec -i $(docker-compose ps -q sqlserver) /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd < temp_seed.sql
    else
        # For LocalDB, we might need to use alternative approach
        print_colored $YELLOW "⚠️ Seed data feature requires manual setup for LocalDB"
    fi
    
    # Clean up
    rm -f temp_seed.sql
    
    print_colored $GREEN "✅ Database seeded successfully"
fi

# ========================================================================
# 6. Summary and Next Steps
# ========================================================================
cd ../../..

print_colored $GREEN ""
print_colored $GREEN "🎉 Local Development Setup Complete!"
print_colored $GREEN "====================================="
print_colored $GREEN ""
print_colored $BLUE "📋 Summary:"
print_colored $BLUE "  Database: GenealogyUserManagement_Dev"
print_colored $BLUE "  Environment: Development"
if [[ "$USE_DOCKER_SQL" == true ]]; then
    print_colored $BLUE "  Connection: Docker SQL Server (localhost:1433)"
else
    print_colored $BLUE "  Connection: LocalDB"
fi
print_colored $BLUE ""
print_colored $YELLOW "🚀 Next Steps:"
print_colored $YELLOW "  1. cd src/Backend/UserManagement"
print_colored $YELLOW "  2. dotnet run"
print_colored $YELLOW "  3. Open browser: https://localhost:5001/swagger"
print_colored $YELLOW ""
print_colored $YELLOW "🔧 Useful Commands:"
print_colored $YELLOW "  Add migration:       dotnet ef migrations add YourMigrationName"
print_colored $YELLOW "  Update database:     dotnet ef database update"
print_colored $YELLOW "  List migrations:     dotnet ef migrations list"
print_colored $YELLOW "  Generate script:     dotnet ef migrations script"
print_colored $YELLOW ""
print_colored $YELLOW "🐳 Docker Commands (if using Docker SQL):"
print_colored $YELLOW "  View logs:           docker-compose logs -f sqlserver"
print_colored $YELLOW "  Stop services:       docker-compose down"
print_colored $YELLOW "  Restart services:    docker-compose restart"
print_colored $YELLOW ""

print_colored $GREEN "✅ Setup completed successfully!"
