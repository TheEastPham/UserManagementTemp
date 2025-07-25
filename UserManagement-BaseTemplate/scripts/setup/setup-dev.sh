#!/bin/bash

# Genealogy Platform - Development Setup Script
# This script sets up the development environment for Phase 1

set -e

echo "🏗️ Setting up Genealogy Platform - Phase 1 Development Environment"
echo "================================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if Docker is running
echo "📋 Checking prerequisites..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running. Please start Docker Desktop and try again.${NC}"
    exit 1
fi

# Check if .NET 8 SDK is installed
if ! dotnet --version | grep -q "8."; then
    echo -e "${RED}❌ .NET 8 SDK is not installed. Please install .NET 8 SDK and try again.${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Prerequisites check passed${NC}"

# Create environment file if it doesn't exist
if [ ! -f .env ]; then
    echo "📝 Creating .env file..."
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
    echo -e "${GREEN}✅ .env file created${NC}"
fi

# Start infrastructure services
echo "🐳 Starting infrastructure services..."
docker-compose up -d sqlserver redis rabbitmq

# Wait for SQL Server to be ready
echo "⏳ Waiting for SQL Server to be ready..."
sleep 30

# Check if SQL Server is responding
echo "🔍 Checking SQL Server connection..."
for i in {1..10}; do
    if docker exec genealogy-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ SQL Server is ready${NC}"
        break
    fi
    echo "⏳ Waiting for SQL Server... (attempt $i/10)"
    sleep 10
    if [ $i -eq 10 ]; then
        echo -e "${RED}❌ SQL Server failed to start${NC}"
        exit 1
    fi
done

# Build the solution
echo "🔨 Building the solution..."
cd src/Backend
dotnet restore
dotnet build

# Run database migrations
echo "🗄️ Running database migrations..."
cd UserManagement
dotnet ef database update || echo -e "${YELLOW}⚠️ Database migrations may need to be created first${NC}"

echo ""
echo -e "${GREEN}🎉 Development environment setup complete!${NC}"
echo ""
echo "📋 Next steps:"
echo "  1. Navigate to src/Backend/UserManagement"
echo "  2. Run: dotnet run"
echo "  3. Open browser to: https://localhost:5001/swagger"
echo ""
echo "🔧 Infrastructure URLs:"
echo "  • SQL Server: localhost:1433 (sa/YourStrong!Passw0rd)"
echo "  • Redis: localhost:6379"
echo "  • RabbitMQ Management: http://localhost:15672 (admin/admin123)"
echo ""
echo "🐳 Docker commands:"
echo "  • Stop services: docker-compose down"
echo "  • View logs: docker-compose logs -f [service-name]"
echo "  • Restart service: docker-compose restart [service-name]"
