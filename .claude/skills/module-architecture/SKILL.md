---
name: module-architecture
description: Use when adding a new module, understanding module structure, or making decisions about module boundaries - covers the modular monolith project layout, DI registration, and cross-module communication patterns
---

# Module Architecture

This project is a modular monolith. Each module is a bounded context with its own database schema, domain, application layer, infrastructure, and presentation layer. Modules communicate exclusively through integration events — never direct references to another module's domain or application layer.

## Module Folder Structure

```
src/Modules/{Module}/
  Domain/                        # Entities, value objects, events, repository interfaces
    {Aggregate}/
      {Entity}.cs
      I{Entity}Repository.cs
      Events/
        {Entity}{Action}DomainEvent.cs
      Enums/
      Errors/
    I{Module}Module.cs           # Module marker interface for IUnitOfWork<T>
  Application/                   # Commands, queries, handlers, event handlers
    {Feature}/
      {CommandOrQuery}/
        {Command}Command.cs
        {Command}CommandHandler.cs
        {Command}CommandValidator.cs  # FluentValidation
      EventHandlers/
        {DomainEvent}Handler.cs      # Domain → integration event mapping
    AssemblyReference.cs
  Infrastructure/                # EF Core, repositories, module registration
    Persistence/
      {Module}DbContext.cs
      Configurations/
        {Entity}Configuration.cs
      Repositories/
        {Entity}Repository.cs
      Migrations/
      README.md                  # Migration commands
      Schemas.cs
    Seeding/
      {Module}Seeder.cs
      Fakers/
    EventBus/
      ProcessSqsJob.cs
    Outbox/
      ProcessOutboxJob.cs
    Inbox/
      ProcessInboxJob.cs
    {Module}Module.cs            # DI registration entry point
  IntegrationEvents/             # Public contracts — other modules reference this project
    {Event}IntegrationEvent.cs
  Presentation/                  # Endpoints, integration event handlers (consumers)
    Endpoints/
      V1/{Feature}/
        {Endpoint}Endpoint.cs
    IntegrationEvents/
      {SourceModule}/
        {Event}IntegrationEventHandler.cs
    AssemblyReference.cs
  test/
    Application.Tests/           # Unit tests
    Domain.Tests/                # Domain unit tests
    Integration/                 # Integration tests (see testing skills)
```

## Module Registration

**File:** `src/Modules/{Module}/Infrastructure/{Module}Module.cs`

```csharp
public static class {Module}Module
{
    public static IServiceCollection Add{Module}Module(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<I{Module}Module>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<{Module}DbContext>(databaseConnectionString, Schemas.{Schema});

        // Repository registrations
        services.AddScoped<I{Entity}Repository, {Entity}Repository>();
        // Cache writer registrations
        services.AddScoped<I{Cache}Writer, {Cache}Repository>();

        services.AddScoped<IUnitOfWork<I{Module}Module>>(
            sp => sp.GetRequiredService<{Module}DbContext>());

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);
        services.AddDomainEventHandlers(Application.AssemblyReference.Assembly);

        services.AddSqsPolling<ProcessSqsJob>(environment);

        // Outbox + Inbox configuration
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection("Messaging:Outbox"))
            .ValidateDataAnnotations().ValidateOnStart();
        services.ConfigureOptions<ConfigureProcessOutboxJob<ProcessOutboxJob>>();

        services.AddOptions<InboxOptions>()
            .Bind(configuration.GetSection("Messaging:Inbox"))
            .ValidateDataAnnotations().ValidateOnStart();
        services.ConfigureOptions<ConfigureProcessInboxJob<ProcessInboxJob>>();

        return services;
    }
}
```

**Registered in Host:** `src/Api/Host/Program.cs`
```csharp
builder.Services.Add{Module}Module(configuration, environment, connectionString);
```

## Cross-Module Communication Rules

1. **Integration events are the ONLY cross-module communication mechanism**
2. Module A's Presentation layer references Module B's IntegrationEvents project (contracts only)
3. NEVER reference another module's Domain or Application project
4. Integration events carry `Guid Public{Entity}Id` — never `int Id`
5. Consumer modules store received data in cache tables (`ICacheProjection`)

## Module Marker Interface

```csharp
// In Domain/
public interface I{Module}Module;
```

Used for `IUnitOfWork<I{Module}Module>` — scopes the unit of work to the module's DbContext.

## Database Schema Convention

Each module owns one or more Postgres schemas:
- Main schema: lowercase module name (e.g., `customers`, `sales`, `inventories`)
- Cache schema: `cache` (for cross-module cache projections)
- Messaging schema: `messaging` (outbox/inbox, shared infrastructure)
- CDC schema: `cdc` (for CDC reference data)
- Packages schema: `packages` (Sales-specific, for package lines TPH)

Defined in `Infrastructure/Persistence/Schemas.cs`:
```csharp
internal static class Schemas
{
    public const string {Name} = "{lowercase}";
    public const string Cache = "cache";
}
```

## Adding a New Module Checklist

- [ ] Create folder structure under `src/Modules/{Module}/`
- [ ] Create 5 projects: Domain, Application, Infrastructure, IntegrationEvents, Presentation
- [ ] Create `I{Module}Module` marker interface in Domain
- [ ] Create `{Module}DbContext` extending `ModuleDbContext<T>`
- [ ] Create `{Module}Module.cs` static class with `Add{Module}Module()`
- [ ] Create `Schemas.cs` with schema constants
- [ ] Create `AssemblyReference.cs` in Application and Presentation
- [ ] Create Outbox/Inbox job classes inheriting from base jobs
- [ ] Create `ProcessSqsJob` in EventBus folder
- [ ] Register in Host `Program.cs`
- [ ] Add connection string configuration in `appsettings.json`
- [ ] Generate initial migration
