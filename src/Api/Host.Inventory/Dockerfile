# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Rtl.Core.Api.sln ./
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Api/Host.Inventory/*.csproj ./src/Api/Host.Inventory/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/Inventory/Domain/*.csproj ./src/Modules/Inventory/Domain/
COPY src/Modules/Inventory/Application/*.csproj ./src/Modules/Inventory/Application/
COPY src/Modules/Inventory/Infrastructure/*.csproj ./src/Modules/Inventory/Infrastructure/
COPY src/Modules/Inventory/Presentation/*.csproj ./src/Modules/Inventory/Presentation/
COPY src/Modules/Inventory/IntegrationEvents/*.csproj ./src/Modules/Inventory/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host.Inventory/Rtl.Core.Api.Inventory.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host.Inventory/Rtl.Core.Api.Inventory.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Rtl.Core.Api.Inventory.dll"]
