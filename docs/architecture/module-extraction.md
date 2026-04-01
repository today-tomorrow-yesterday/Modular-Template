# Module Extraction Guide

## Overview

This guide describes how to extract a module from the ModularTemplate monorepo into its own independent repository. The per-module deployment architecture makes extraction straightforward.

## When to Extract

Consider extraction when:
- A module has grown large enough to warrant its own repository
- A team wants full ownership and independent release cycles
- Technology choices need to diverge from the monorepo
- Compliance/security requires code isolation

## Prerequisites

Before extraction, ensure:
- Module has its own dedicated host project (`ModularTemplate.Api.{Module}`)
- Module uses its own database (not sharing tables with other modules)
- All cross-module communication uses integration events (no direct dependencies)
- Module has comprehensive tests

## Extraction Checklist

### Step 1: Create New Repository

```bash
# Create new repo structure
mkdir orders-service
cd orders-service
git init

# Create src folder structure
mkdir -p src/{Orders.Domain,Orders.Application,Orders.Infrastructure,Orders.Presentation,Orders.IntegrationEvents,Orders.Api}
mkdir -p test/{Orders.Domain.Tests,Orders.Application.Tests,Orders.IntegrationTests}
```

### Step 2: Copy Module Code

Copy from monorepo to new repo:

| Source (Monorepo) | Destination (New Repo) |
|-------------------|------------------------|
| `src/Modules/{Module}/Domain/` | `src/{Module}.Domain/` |
| `src/Modules/{Module}/Application/` | `src/{Module}.Application/` |
| `src/Modules/{Module}/Infrastructure/` | `src/{Module}.Infrastructure/` |
| `src/Modules/{Module}/Presentation/` | `src/{Module}.Presentation/` |
| `src/Modules/{Module}/IntegrationEvents/` | `src/{Module}.IntegrationEvents/` |
| `src/Api/Host.{Module}/` | `src/{Module}.Api/` |
| `src/Modules/{Module}/test/` | `test/` |

### Step 3: Handle Common Dependencies

You have three options for Common layer dependencies:

#### Option A: Publish Common as NuGet Packages (Recommended)

Publish shared code as internal NuGet packages:

```xml
<!-- Before (Project Reference) -->
<ProjectReference Include="..\..\Common\ModularTemplate.Common.Domain\..." />

<!-- After (Package Reference) -->
<PackageReference Include="YourOrg.Common.Domain" Version="1.0.0" />
```

Create packages for:
- `YourOrg.Common.Domain` - Base entities, Result pattern
- `YourOrg.Common.Application` - MediatR interfaces, behaviors
- `YourOrg.Common.Infrastructure` - EF Core, outbox/inbox
- `YourOrg.Common.Presentation` - Endpoint interfaces

#### Option B: Copy Common Code (Fork)

Copy the Common code into the new repository:

```bash
cp -r src/Common/* orders-service/src/Common/
```

**Pros:** Full independence, no external dependencies
**Cons:** Code divergence over time, harder to share improvements

#### Option C: Git Submodule

Add Common as a git submodule:

```bash
git submodule add https://github.com/yourorg/modular-template-common.git src/Common
```

**Pros:** Shared updates
**Cons:** Added complexity, submodule management overhead

### Step 4: Update Namespaces (Optional)

If renaming the module:

```bash
# Using find and replace
find src -name "*.cs" -exec sed -i 's/Modules.{Module}/YourNewNamespace/g' {} \;
```

Update:
- Namespace declarations
- Using statements
- Project names in `.csproj` files

### Step 5: Update Project References

Convert project references to package references for Common:

```xml
<ItemGroup>
  <!-- External packages -->
  <PackageReference Include="YourOrg.Common.Domain" Version="1.0.0" />
  <PackageReference Include="YourOrg.Common.Application" Version="1.0.0" />
  <PackageReference Include="YourOrg.Common.Infrastructure" Version="1.0.0" />
  <PackageReference Include="YourOrg.Common.Presentation" Version="1.0.0" />

  <!-- Internal project references -->
  <ProjectReference Include="..\Orders.Domain\Orders.Domain.csproj" />
</ItemGroup>
```

### Step 6: Update CI/CD

Create new GitHub Actions workflows for the extracted repo:

```yaml
# .github/workflows/build.yml
name: Build
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build
```

### Step 7: Update Infrastructure

#### ECR Repository

Create dedicated ECR repository:

```bash
aws ecr create-repository --repository-name orders-api
```

#### ECS Task Definition

Update task definition to use new image:

```json
{
  "containerDefinitions": [{
    "image": "${ECR_REGISTRY}/orders-api:${IMAGE_TAG}"
  }]
}
```

#### Database

- Module already has its own database (per architecture)
- Update connection strings to point to dedicated RDS instance if needed
- Consider moving from shared RDS to dedicated instance

### Step 8: Update Integration Events

#### Publishing Module

The IntegrationEvents package may need to be published to NuGet:

```bash
# Package and publish
dotnet pack src/Orders.IntegrationEvents -c Release
dotnet nuget push Orders.IntegrationEvents.*.nupkg --source internal-feed
```

#### Consuming Modules

Other modules consuming events need to update their package references:

```xml
<PackageReference Include="Orders.IntegrationEvents" Version="1.0.0" />
```

### Step 9: Clean Up Monorepo

After successful extraction and verification:

1. Remove module from monorepo
2. Update monorepo's `docker-compose.yml`
3. Remove from solution file
4. Update any remaining references

```bash
# Remove from solution
dotnet sln ModularTemplate.Api.sln remove src/Modules/{Module}/**/*.csproj
dotnet sln ModularTemplate.Api.sln remove src/Api/Host.{Module}/*.csproj

# Delete module folders
rm -rf src/Modules/{Module}
rm -rf src/Api/Host.{Module}
```

## Post-Extraction Verification

### Functional Testing

1. Run all tests in new repo
2. Deploy to staging environment
3. Verify API endpoints work correctly
4. Verify integration events flow correctly

### Integration Testing

1. Verify cross-module events still work
2. Test end-to-end user workflows
3. Monitor for any missing dependencies

### Performance Testing

1. Baseline performance metrics
2. Compare with pre-extraction metrics
3. Verify no degradation

## Rollback Plan

If extraction fails:

1. Keep monorepo deployment running during transition
2. Route traffic back to monorepo if issues arise
3. Investigate and fix issues before re-attempting

## Timeline Recommendation

| Phase | Duration | Activities |
|-------|----------|------------|
| Preparation | 1 week | Create new repo, copy code, update references |
| Testing | 1-2 weeks | Unit tests, integration tests, staging deployment |
| Shadow Mode | 1 week | Run both in parallel, compare results |
| Cutover | 1 day | Switch traffic, monitor |
| Cleanup | 1 week | Remove from monorepo, documentation |

## Common Issues

### Missing Dependencies

**Symptom:** Build fails with missing type errors
**Solution:** Add missing package references from Common layer

### Integration Event Serialization

**Symptom:** Events not deserializing correctly
**Solution:** Ensure event types are exactly the same (namespace, properties)

### Database Migrations

**Symptom:** EF Core migration history conflicts
**Solution:** Create new migration baseline in extracted repo

### Authentication/Authorization

**Symptom:** Auth tokens not working
**Solution:** Ensure shared auth configuration is correctly replicated

## Related Documentation

- [Module Deployment Architecture](./module-deployment.md)
- [Local Development](../development/local-development.md)
