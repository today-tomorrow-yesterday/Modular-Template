# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Rtl.Core.Api.sln ./
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Api/Host.Organization/*.csproj ./src/Api/Host.Organization/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/Organization/Domain/*.csproj ./src/Modules/Organization/Domain/
COPY src/Modules/Organization/Application/*.csproj ./src/Modules/Organization/Application/
COPY src/Modules/Organization/Infrastructure/*.csproj ./src/Modules/Organization/Infrastructure/
COPY src/Modules/Organization/Presentation/*.csproj ./src/Modules/Organization/Presentation/
COPY src/Modules/Organization/IntegrationEvents/*.csproj ./src/Modules/Organization/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host.Organization/Rtl.Core.Api.Organization.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host.Organization/Rtl.Core.Api.Organization.csproj \
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

ENTRYPOINT ["dotnet", "Rtl.Core.Api.Organization.dll"]
