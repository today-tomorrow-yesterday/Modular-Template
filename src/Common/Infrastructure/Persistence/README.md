# Data Infrastructure

## DbContext Architecture

This folder contains `ModuleDbContext<TContext>` - the **abstract base class** that all module DbContexts inherit from. It provides common infrastructure (outbox, inbox, audit tables).

Each **module owns its own concrete DbContext** inheriting from this base:
```
src/Modules/{ModuleName}/.../Persistence/{ModuleName}DbContext.cs : ModuleDbContext<T>
```

Why per-module DbContexts?
- Modules are isolated bounded contexts with their own data
- Teams can evolve schemas independently
- Modules can be extracted to microservices without shared database coupling

## What's in This Folder?

This folder contains **shared data infrastructure** used by all modules:

| File | Purpose |
|------|---------|
| `DbConnectionFactory<TModule>.cs` | Creates module-specific database connections for Dapper queries |
| `ModuleDbContext.cs` | Base DbContext for all modules with common infrastructure (outbox, inbox, audit) |
| `ReadRepository.cs` | Generic read-only repository base class with expression-based ID lookup |
| `Repository.cs` | Generic repository base class |

### Extension Methods in Auditing/Configurations/

These are **static extension methods** (not traditional EF `IEntityTypeConfiguration` classes) that modules call in their entity configurations:

| File | Purpose |
|------|---------|
| `AuditableEntityConfiguration.cs` | Extension method to configure audit fields (CreatedAtUtc, ModifiedAtUtc, etc.) |
| `SoftDeletableEntityConfiguration.cs` | Extension method to configure soft delete fields and query filters |

Usage in module entity configurations:
```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ConfigureAuditProperties();      // From AuditableEntityConfiguration
        builder.ConfigureSoftDeleteProperties(); // From SoftDeletableEntityConfiguration
    }
}
```

## Database Connection Factory

### Why Generic?

We use `IDbConnectionFactory<TModule>` (generic) instead of a shared `IDbConnectionFactory`:

```csharp
// Each module has its own connection factory
IDbConnectionFactory<ISampleOrdersModule>   // SampleOrders module connections
IDbConnectionFactory<ISampleSalesModule>    // SampleSales module connections
```

**Benefits:**
- **Module Isolation**: Each module can have its own database
- **DI Resolution**: The container resolves the correct factory per module
- **Consistency**: All components (jobs, handlers) use the same pattern

### Registration

Register in each module's setup:

```csharp
services.AddModuleDataSource<ISampleOrdersModule>(databaseConnectionString);
```

### Usage in Handlers

```csharp
public class ProcessOutboxJob(
    IDbConnectionFactory<ISampleOrdersModule> dbConnectionFactory,
    // ...
) : ProcessOutboxJobBase<ISampleOrdersModule>(dbConnectionFactory, ...)
```

## Database Configuration Options

### Single Database (Default)
All modules share one database with separate schemas:
```
PostgreSQL: rtlcore
├── Schema: samplesales
├── Schema: sampleorders
├── Schema: customer
└── Schema: sales
```

### Multiple Databases
Each module can have its own database via config:
```json
{
  "Modules": {
    "SampleOrders": {
      "ConnectionStrings": {
        "Database": "Host=localhost;Database=rtlcore_sampleorders;..."
      }
    }
  }
}
```

## EF Core Migration Quick Reference

Run from solution root. Replace `{Module}` with your module name.

**Create Migration:**
```bash
dotnet ef migrations add <Name> \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host
```

**Apply Migration:**
```bash
dotnet ef database update \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host
```

**Remove Last Migration:**
```bash
dotnet ef migrations remove \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host
```

**Generate SQL Script (Production):**
```bash
dotnet ef migrations script \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --idempotent \
  --output migrations.sql
```

For module-specific migration details, check each module's `Persistence/` folder.
