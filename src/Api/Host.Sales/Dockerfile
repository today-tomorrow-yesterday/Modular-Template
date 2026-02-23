# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Rtl.Core.Api.sln ./
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Api/Host.Sales/*.csproj ./src/Api/Host.Sales/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/Sales/Domain/*.csproj ./src/Modules/Sales/Domain/
COPY src/Modules/Sales/Application/*.csproj ./src/Modules/Sales/Application/
COPY src/Modules/Sales/Infrastructure/*.csproj ./src/Modules/Sales/Infrastructure/
COPY src/Modules/Sales/Presentation/*.csproj ./src/Modules/Sales/Presentation/
COPY src/Modules/Sales/IntegrationEvents/*.csproj ./src/Modules/Sales/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host.Sales/Rtl.Core.Api.Sales.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host.Sales/Rtl.Core.Api.Sales.csproj \
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

ENTRYPOINT ["dotnet", "Rtl.Core.Api.Sales.dll"]
