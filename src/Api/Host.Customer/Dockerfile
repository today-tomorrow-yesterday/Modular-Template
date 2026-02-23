# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Rtl.Core.Api.sln ./
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Api/Host.Customer/*.csproj ./src/Api/Host.Customer/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/Customer/Domain/*.csproj ./src/Modules/Customer/Domain/
COPY src/Modules/Customer/Application/*.csproj ./src/Modules/Customer/Application/
COPY src/Modules/Customer/Infrastructure/*.csproj ./src/Modules/Customer/Infrastructure/
COPY src/Modules/Customer/Presentation/*.csproj ./src/Modules/Customer/Presentation/
COPY src/Modules/Customer/IntegrationEvents/*.csproj ./src/Modules/Customer/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host.Customer/Rtl.Core.Api.Customer.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host.Customer/Rtl.Core.Api.Customer.csproj \
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

ENTRYPOINT ["dotnet", "Rtl.Core.Api.Customer.dll"]
