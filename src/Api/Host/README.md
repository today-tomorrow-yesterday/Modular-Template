# API Projects

## Overview

This folder contains multiple API host projects. Understanding when and why to use each one will help you navigate local development.

## Projects

| Project | Purpose |
|---------|---------|
| `Rtl.Core.Api` | **Main monolith API** - runs all modules together. Use this for production and most development. |
| `Rtl.Core.Api.{Module}` | **Dedicated module APIs** - runs a single module in isolation. Use these for local development only. |
| `Rtl.Core.Api.Shared` | Shared extensions and configuration used by all API hosts. |

## Why Dedicated Module APIs Exist

The dedicated API projects (Customer, Fees, Organization, Product, Sales, SampleOrders, SampleSales) exist to give you flexibility during local development:

- **Faster startup** - Load only the module you're working on
- **Isolated debugging** - Focus on one module without noise from others
- **Database flexibility** - Test different database configurations per module

These are **not deployed to production**. In production, only `Rtl.Core.Api` runs with all modules loaded.

## Database Configuration Options

During local development, you have flexibility in how you structure your databases:

### Option 1: Single Database, Multiple Schemas (Recommended)
All modules share one database but each module has its own schema (e.g., `sampleorders`, `customer`, `sales`). This is the default configuration and mirrors production.

```
Database: Rtl.Core
├── Schema: samplesales
├── Schema: sampleorders
├── Schema: organization
├── Schema: customer
├── Schema: sales
├── Schema: fees
└── Schema: products
```

### Option 2: Separate Databases Per Module
Each module gets its own database. Useful when you want complete isolation or are testing module extraction.

```
Database: Rtl.Core_SampleSales
Database: Rtl.Core_SampleOrders
Database: Rtl.Core_Organization
Database: Rtl.Core_Customer
Database: Rtl.Core_Sales
Database: Rtl.Core_Fees
Database: Rtl.Core_Products
```

Configure this in each module's `appsettings.Development.json` by changing the connection string.

## Quick Start

**Running the full monolith:**
```bash
dotnet run --project src/Api/Host
```

**Running a single module:**
```bash
dotnet run --project src/Api/Host.SampleOrders
```

## When to Use What

| Scenario | Use This |
|----------|----------|
| Normal development | `Rtl.Core.Api` |
| Working on one module only | `Rtl.Core.Api.{Module}` |
| Running integration tests | `Rtl.Core.Api` |
| Production deployment | `Rtl.Core.Api` |
