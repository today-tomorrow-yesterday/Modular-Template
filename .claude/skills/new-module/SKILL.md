---
name: scaffold-new-module
description: Use when creating a new module from scratch - scaffolds directory structure, projects, DbContext, module registration, and wires everything up
---

# Scaffold New Module

Creates a complete new module with all layers, wired into the solution and API host.

## Inputs

Before starting, confirm with the user:
- **Module name** (e.g., `Billing`) — PascalCase, singular
- **Schema name** (e.g., `billing`) — lowercase, used for DB schema and HiLo sequence
- **First aggregate** (e.g., `Invoice`) — the initial entity to scaffold

## Step 1: Create Project Structure

Create these directories and `.csproj` files under `src/Modules/{Module}/`:

### Domain Project

**File:** `src/Modules/{Module}/Domain/Modules.{Module}.Domain.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\..\Common\Domain\ModularTemplate.Domain.csproj" />
  </ItemGroup>
</Project>
```

**File:** `src/Modules/{Module}/Domain/I{Module}Module.cs`
```csharp
namespace Modules.{Module}.Domain;

/// <summary>
/// Marker interface for {Module} module.
/// Used for module-specific dependency injection registrations.
/// </summary>
public interface I{Module}Module;
```

### Application Project

**File:** `src/Modules/{Module}/Application/Modules.{Module}.Application.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.1.1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Common\Application\ModularTemplate.Application.csproj" />
    <ProjectReference Include="..\Domain\Modules.{Module}.Domain.csproj" />
    <ProjectReference Include="..\IntegrationEvents\Modules.{Module}.IntegrationEvents.csproj" />
  </ItemGroup>
</Project>
```

**File:** `src/Modules/{Module}/Application/AssemblyReference.cs`
```csharp
using System.Reflection;

namespace Modules.{Module}.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
```

### IntegrationEvents Project

**File:** `src/Modules/{Module}/IntegrationEvents/Modules.{Module}.IntegrationEvents.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\..\Common\Application\ModularTemplate.Application.csproj" />
  </ItemGroup>
</Project>
```

### Presentation Project

**File:** `src/Modules/{Module}/Presentation/Modules.{Module}.Presentation.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\..\Common\Presentation\ModularTemplate.Presentation.csproj" />
    <ProjectReference Include="..\Application\Modules.{Module}.Application.csproj" />
  </ItemGroup>
</Project>
```

**File:** `src/Modules/{Module}/Presentation/AssemblyReference.cs`
```csharp
using System.Reflection;

namespace Modules.{Module}.Presentation;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
```

**File:** `src/Modules/{Module}/Presentation/Endpoints/{Module}ModuleEndpoints.cs`
```csharp
using Asp.Versioning.Builder;
using ModularTemplate.Presentation.Endpoints;

namespace Modules.{Module}.Presentation.Endpoints;

public sealed class {Module}ModuleEndpoints : IModuleEndpoints
{
    public void MapEndpoints(WebApplication app, ApiVersionSet apiVersionSet)
    {
        // Endpoint groups registered here — see SampleOrders for pattern
    }
}
```

### Infrastructure Project

**File:** `src/Modules/{Module}/Infrastructure/Modules.{Module}.Infrastructure.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\..\Common\Infrastructure\ModularTemplate.Infrastructure.csproj" />
    <ProjectReference Include="..\Application\Modules.{Module}.Application.csproj" />
    <ProjectReference Include="..\Presentation\Modules.{Module}.Presentation.csproj" />
  </ItemGroup>
</Project>
```

**File:** `src/Modules/{Module}/Infrastructure/Persistence/Schemas.cs`
```csharp
namespace Modules.{Module}.Infrastructure.Persistence;

internal static class Schemas
{
    internal const string {Module} = "{schema}";
}
```

**File:** `src/Modules/{Module}/Infrastructure/Persistence/{Module}DbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Modules.{Module}.Domain;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure.Persistence;

namespace Modules.{Module}.Infrastructure.Persistence;

public sealed class {Module}DbContext(DbContextOptions<{Module}DbContext> options)
    : ModuleDbContext<{Module}DbContext>(options), IUnitOfWork<I{Module}Module>
{
    protected override string Schema => Schemas.{Module};

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof({Module}DbContext).Assembly);
    }
}
```

**File:** `src/Modules/{Module}/Infrastructure/{Module}Module.cs`
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.{Module}.Domain;
using Modules.{Module}.Infrastructure.Persistence;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure;
using ModularTemplate.Infrastructure.EventBus;
using ModularTemplate.Infrastructure.Inbox.Job;
using ModularTemplate.Infrastructure.Outbox.Job;
using ModularTemplate.Infrastructure.Persistence;

namespace Modules.{Module}.Infrastructure;

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
        services.AddModuleDbContext<{Module}DbContext>(databaseConnectionString, Schemas.{Module});

        // Register repositories here:
        // services.AddScoped<I{Entity}Repository, {Entity}Repository>();

        services.AddScoped<IUnitOfWork<I{Module}Module>>(sp => sp.GetRequiredService<{Module}DbContext>());

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);
        services.AddDomainEventHandlers(Application.AssemblyReference.Assembly);

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection("Messaging:Outbox"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<InboxOptions>()
            .Bind(configuration.GetSection("Messaging:Inbox"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
```

### Test Projects

**File:** `src/Modules/{Module}/test/Domain.Tests/Modules.{Module}.Domain.Tests.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Domain\Modules.{Module}.Domain.csproj" />
  </ItemGroup>
</Project>
```

**File:** `src/Modules/{Module}/test/Application.Tests/Modules.{Module}.Application.Tests.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.1.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Domain\Modules.{Module}.Domain.csproj" />
    <ProjectReference Include="..\..\Application\Modules.{Module}.Application.csproj" />
  </ItemGroup>
</Project>
```

## Step 2: Add to Solution

```bash
dotnet sln add src/Modules/{Module}/Domain/Modules.{Module}.Domain.csproj
dotnet sln add src/Modules/{Module}/Application/Modules.{Module}.Application.csproj
dotnet sln add src/Modules/{Module}/IntegrationEvents/Modules.{Module}.IntegrationEvents.csproj
dotnet sln add src/Modules/{Module}/Infrastructure/Modules.{Module}.Infrastructure.csproj
dotnet sln add src/Modules/{Module}/Presentation/Modules.{Module}.Presentation.csproj
dotnet sln add src/Modules/{Module}/test/Domain.Tests/Modules.{Module}.Domain.Tests.csproj
dotnet sln add src/Modules/{Module}/test/Application.Tests/Modules.{Module}.Application.Tests.csproj
```

## Step 3: Wire Into API Host

### Combined Host (`src/Api/Host/Program.cs`)

Add using statements:
```csharp
using Modules.{Module}.Infrastructure;
using Modules.{Module}.Infrastructure.Persistence;
using {Module}Application = Modules.{Module}.Application.AssemblyReference;
```

Add to `AddCommonApplication` array:
```csharp
{Module}Application.Assembly,
```

Add module registration:
```csharp
.Add{Module}Module(builder.Configuration, builder.Environment,
    DatabaseMigrationExtensions.GetModuleConnectionString(builder.Configuration, "{Module}", databaseConnectionString))
```

Add to `ApplyMigrations`:
```csharp
("{Module}", typeof({Module}DbContext)),
```

Add to `AddModuleConfiguration` array:
```csharp
"{Module}",
```

### Combined Host ModuleExtensions (`src/Api/Host/Extensions/ModuleExtensions.cs`)

Add using:
```csharp
using Modules.{Module}.Presentation.Endpoints;
```

Add to `GetModuleEndpoints()` array:
```csharp
new {Module}ModuleEndpoints(),
```

### Combined Host .csproj (`src/Api/Host/ModularTemplate.Api.csproj`)

Add project reference:
```xml
<ProjectReference Include="..\..\Modules\{Module}\Infrastructure\Modules.{Module}.Infrastructure.csproj" />
```

## Step 4: Generate Initial Migration

```bash
export ENCRYPTION_KEY="MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI="

dotnet ef migrations add InitialCreate \
  --project src/Modules/{Module}/Infrastructure \
  --startup-project src/Api/Host \
  --context {Module}DbContext \
  --output-dir Persistence/Migrations
```

## Step 5: Build and Verify

```bash
dotnet build
dotnet test src/Modules/{Module}/test/Domain.Tests/
dotnet test src/Modules/{Module}/test/Application.Tests/
```

## Next Steps

After the module is scaffolded, use these skills to add features:
- `domain-modeling` — add the first aggregate root entity
- `endpoint` — add CRUD endpoints
- `integration-event` — add cross-module events
- `ef-core-patterns` — configure EF for the entity

## Checklist

- [ ] All 5 projects created (Domain, Application, IntegrationEvents, Infrastructure, Presentation)
- [ ] 2 test projects created (Domain.Tests, Application.Tests)
- [ ] `I{Module}Module` marker interface in Domain
- [ ] `AssemblyReference` in Application and Presentation
- [ ] `{Module}DbContext` extends `ModuleDbContext<T>` and implements `IUnitOfWork<I{Module}Module>`
- [ ] `Schemas` class with module schema constant
- [ ] `{Module}Module` static class with `Add{Module}Module` extension method
- [ ] `{Module}ModuleEndpoints` class implementing `IModuleEndpoints`
- [ ] All projects added to solution
- [ ] Module registered in combined Host (Program.cs, ModuleExtensions.cs, .csproj)
- [ ] Initial migration generated
- [ ] Build succeeds, tests pass
