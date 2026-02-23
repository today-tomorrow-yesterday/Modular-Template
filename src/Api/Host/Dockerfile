# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Rtl.Core.Api.sln ./
COPY src/Api/Host/*.csproj ./src/Api/Host/
COPY src/Api/Shared/*.csproj ./src/Api/Shared/
COPY src/Common/Domain/*.csproj ./src/Common/Domain/
COPY src/Common/Application/*.csproj ./src/Common/Application/
COPY src/Common/Infrastructure/*.csproj ./src/Common/Infrastructure/
COPY src/Common/Presentation/*.csproj ./src/Common/Presentation/
COPY src/Modules/Fees/Domain/*.csproj ./src/Modules/Fees/Domain/
COPY src/Modules/Fees/Application/*.csproj ./src/Modules/Fees/Application/
COPY src/Modules/Fees/Infrastructure/*.csproj ./src/Modules/Fees/Infrastructure/
COPY src/Modules/Fees/Presentation/*.csproj ./src/Modules/Fees/Presentation/
COPY src/Modules/Fees/IntegrationEvents/*.csproj ./src/Modules/Fees/IntegrationEvents/
COPY src/Modules/Product/Domain/*.csproj ./src/Modules/Product/Domain/
COPY src/Modules/Product/Application/*.csproj ./src/Modules/Product/Application/
COPY src/Modules/Product/Infrastructure/*.csproj ./src/Modules/Product/Infrastructure/
COPY src/Modules/Product/Presentation/*.csproj ./src/Modules/Product/Presentation/
COPY src/Modules/Product/IntegrationEvents/*.csproj ./src/Modules/Product/IntegrationEvents/
COPY src/Modules/SampleOrders/Domain/*.csproj ./src/Modules/SampleOrders/Domain/
COPY src/Modules/SampleOrders/Application/*.csproj ./src/Modules/SampleOrders/Application/
COPY src/Modules/SampleOrders/Infrastructure/*.csproj ./src/Modules/SampleOrders/Infrastructure/
COPY src/Modules/SampleOrders/Presentation/*.csproj ./src/Modules/SampleOrders/Presentation/
COPY src/Modules/SampleOrders/IntegrationEvents/*.csproj ./src/Modules/SampleOrders/IntegrationEvents/
COPY src/Modules/SampleSales/Domain/*.csproj ./src/Modules/SampleSales/Domain/
COPY src/Modules/SampleSales/Application/*.csproj ./src/Modules/SampleSales/Application/
COPY src/Modules/SampleSales/Infrastructure/*.csproj ./src/Modules/SampleSales/Infrastructure/
COPY src/Modules/SampleSales/Presentation/*.csproj ./src/Modules/SampleSales/Presentation/
COPY src/Modules/SampleSales/IntegrationEvents/*.csproj ./src/Modules/SampleSales/IntegrationEvents/
COPY src/Modules/Organization/Domain/*.csproj ./src/Modules/Organization/Domain/
COPY src/Modules/Organization/Application/*.csproj ./src/Modules/Organization/Application/
COPY src/Modules/Organization/Infrastructure/*.csproj ./src/Modules/Organization/Infrastructure/
COPY src/Modules/Organization/Presentation/*.csproj ./src/Modules/Organization/Presentation/
COPY src/Modules/Organization/IntegrationEvents/*.csproj ./src/Modules/Organization/IntegrationEvents/
COPY src/Modules/Customer/Domain/*.csproj ./src/Modules/Customer/Domain/
COPY src/Modules/Customer/Application/*.csproj ./src/Modules/Customer/Application/
COPY src/Modules/Customer/Infrastructure/*.csproj ./src/Modules/Customer/Infrastructure/
COPY src/Modules/Customer/Presentation/*.csproj ./src/Modules/Customer/Presentation/
COPY src/Modules/Customer/IntegrationEvents/*.csproj ./src/Modules/Customer/IntegrationEvents/
COPY src/Modules/Inventory/Domain/*.csproj ./src/Modules/Inventory/Domain/
COPY src/Modules/Inventory/Application/*.csproj ./src/Modules/Inventory/Application/
COPY src/Modules/Inventory/Infrastructure/*.csproj ./src/Modules/Inventory/Infrastructure/
COPY src/Modules/Inventory/Presentation/*.csproj ./src/Modules/Inventory/Presentation/
COPY src/Modules/Inventory/IntegrationEvents/*.csproj ./src/Modules/Inventory/IntegrationEvents/
COPY src/Modules/Sales/Domain/*.csproj ./src/Modules/Sales/Domain/
COPY src/Modules/Sales/Application/*.csproj ./src/Modules/Sales/Application/
COPY src/Modules/Sales/Infrastructure/*.csproj ./src/Modules/Sales/Infrastructure/
COPY src/Modules/Sales/Presentation/*.csproj ./src/Modules/Sales/Presentation/
COPY src/Modules/Sales/IntegrationEvents/*.csproj ./src/Modules/Sales/IntegrationEvents/
COPY src/Modules/Funding/Domain/*.csproj ./src/Modules/Funding/Domain/
COPY src/Modules/Funding/Application/*.csproj ./src/Modules/Funding/Application/
COPY src/Modules/Funding/Infrastructure/*.csproj ./src/Modules/Funding/Infrastructure/
COPY src/Modules/Funding/Presentation/*.csproj ./src/Modules/Funding/Presentation/
COPY src/Modules/Funding/IntegrationEvents/*.csproj ./src/Modules/Funding/IntegrationEvents/

# Restore dependencies
RUN dotnet restore src/Api/Host/Rtl.Core.Api.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish src/Api/Host/Rtl.Core.Api.csproj \
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

ENTRYPOINT ["dotnet", "Rtl.Core.Api.dll"]
