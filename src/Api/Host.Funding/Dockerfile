# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Rtl.Core.Api.sln ./
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Api/Host.Funding/*.csproj ./src/Api/Host.Funding/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/Funding/Domain/*.csproj ./src/Modules/Funding/Domain/
COPY src/Modules/Funding/Application/*.csproj ./src/Modules/Funding/Application/
COPY src/Modules/Funding/Infrastructure/*.csproj ./src/Modules/Funding/Infrastructure/
COPY src/Modules/Funding/Presentation/*.csproj ./src/Modules/Funding/Presentation/
COPY src/Modules/Funding/IntegrationEvents/*.csproj ./src/Modules/Funding/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host.Funding/Rtl.Core.Api.Funding.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host.Funding/Rtl.Core.Api.Funding.csproj \
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

ENTRYPOINT ["dotnet", "Rtl.Core.Api.Funding.dll"]
