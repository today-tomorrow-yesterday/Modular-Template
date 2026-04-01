# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY ModularTemplate.Api.sln ./
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Api/Host.SampleOrders/*.csproj ./src/Api/Host.SampleOrders/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/SampleOrders/Domain/*.csproj ./src/Modules/SampleOrders/Domain/
COPY src/Modules/SampleOrders/Application/*.csproj ./src/Modules/SampleOrders/Application/
COPY src/Modules/SampleOrders/Infrastructure/*.csproj ./src/Modules/SampleOrders/Infrastructure/
COPY src/Modules/SampleOrders/Presentation/*.csproj ./src/Modules/SampleOrders/Presentation/
COPY src/Modules/SampleOrders/IntegrationEvents/*.csproj ./src/Modules/SampleOrders/IntegrationEvents/
COPY src/Modules/SampleSales/IntegrationEvents/*.csproj ./src/Modules/SampleSales/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host.SampleOrders/ModularTemplate.Api.SampleOrders.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host.SampleOrders/ModularTemplate.Api.SampleOrders.csproj \
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

ENTRYPOINT ["dotnet", "ModularTemplate.Api.SampleOrders.dll"]
