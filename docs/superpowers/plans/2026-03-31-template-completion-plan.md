# Template Completion Plan

> **Branch:** `template/enhance-sample-modules` (based on `project-one-implementation`)
> **Goal:** Complete the modular monolith template with enhanced sample modules, then cherry-pick into orphan `main`

## Current State

### What's Done
- **Branching:** `main` is an orphan branch (empty, no project-one history). `template/enhance-sample-modules` has all work.
- **SampleOrders enhanced:**
  - Customer aggregate: PublicId, CustomerName value object, CustomerContact child collection, CustomerAddress with shared Address value object, CustomerStatus enum, [SensitiveData] on DOB
  - Order aggregate: OrderLine TPH (ProductLine/CustomLine) with single JSONB `details` column via VersionedJsonConverter, ShippingAddress child entity with Address value object, status machine
  - ProductLineDetails/CustomLineDetails implement IVersionedDetails (SchemaVersion + ExtensionData)
  - Domain events for all state changes
  - 31 domain tests, 27 application tests (58 total, all passing)
- **SampleSales enhanced:**
  - Product/Catalog: PublicId (Guid v7)
  - Zero tests (needs them)
- **Both modules:**
  - Integration events use `Guid Public{Entity}Id`
  - Cache entities use `RefPublicId` with `UseIdentityAlwaysColumn`
  - Cache writer interfaces in Domain layer (not inline in Presentation)
  - All response DTOs use `Guid PublicId`
  - Commands return `Guid PublicId`
  - Clean InitialCreate migrations
- **14 skills** in `.claude/skills/` — all updated to reference sample modules
- **All 58 tests pass, build is clean**

### What's NOT Done

## Phase 1: Remaining Sample Module Work

### Task 1: Add SampleSales Tests
Create domain and application tests for SampleSales (currently zero tests).

**Domain Tests** (`src/Modules/SampleSales/test/Domain.Tests/`):
- `Products/ProductTests.cs` — Create, Update, validation, PublicId, domain events
- `Catalogs/CatalogTests.cs` — Create, Update, AddProduct, RemoveProduct, duplicate prevention

**Application Tests** (`src/Modules/SampleSales/test/Application.Tests/`):
- `Products/CreateProductCommandHandlerTests.cs`
- `Products/CreateProductCommandValidatorTests.cs`
- `Catalogs/CreateCatalogCommandHandlerTests.cs`

Reference patterns: `src/Modules/SampleOrders/test/` has working examples.

### Task 2: Add Shipping Address Endpoint
The domain supports `Order.SetShippingAddress(Address)` but no endpoint exposes it.

Create in `src/Modules/SampleOrders/Presentation/Endpoints/Orders/V1/`:
- `SetShippingAddressEndpoint.cs` — `PUT /orders/{orderId}/shipping-address`
- Request: `SetShippingAddressRequest(string AddressLine1, string? AddressLine2, string City, string State, string PostalCode, string? Country)`

Also need:
- `SetShippingAddressCommand` + handler + validator in Application layer
- Update `OrderResponse` to include shipping address data

### Task 3: Add Customer Contact/Address Endpoints (optional but valuable)
- `POST /customers/{customerId}/contacts` — AddContact
- `POST /customers/{customerId}/addresses` — AddAddress

These demonstrate the nested resource endpoint pattern.

## Phase 2: Strip Project-One Code

### Task 4: Delete Project-One Modules
Remove these directories entirely:
- `src/Modules/Customer/`
- `src/Modules/Sales/`
- `src/Modules/Inventory/`
- `src/Modules/Organization/`
- `src/Modules/Funding/`

### Task 5: Delete Project-One API Hosts
Remove:
- `src/Api/Host.Customer/`
- `src/Api/Host.Sales/`
- `src/Api/Host.Inventory/`
- `src/Api/Host.Organization/`
- `src/Api/Host.Funding/`
- `src/Api/Host/` — this is the combined host that registers all modules. Either delete or strip to only register sample modules.

### Task 6: Clean Up Solution File
The `.sln` file references all project-one `.csproj` files. Remove those references, keep only:
- `src/Common/**` (all common infrastructure)
- `src/Modules/SampleOrders/**`
- `src/Modules/SampleSales/**`
- `src/Api/Host.SampleOrders/`
- `src/Api/Host.SampleSales/`
- `src/Api/Shared/`
- All test projects for sample modules

### Task 7: Remove Project-One References from Common
Check `src/Common/` for any imports or references to project-one modules. These should be generic infrastructure only.

### Task 8: Build and Test
After stripping:
```bash
dotnet build
dotnet test src/Modules/SampleOrders/test/Domain.Tests/
dotnet test src/Modules/SampleOrders/test/Application.Tests/
dotnet test src/Modules/SampleSales/test/Domain.Tests/
dotnet test src/Modules/SampleSales/test/Application.Tests/
```

### Task 9: Clean Up Docs
- Remove `docs/testing/event-flow-integration-testing-strategy.md` (project-one specific)
- Remove `docs/superpowers/specs/2026-03-30-inventory-public-id-design.md` (project-one specific)
- Keep skills in `.claude/skills/`

## Phase 3: Cherry-Pick to Main

### Task 10: Cherry-Pick Clean Template to Orphan Main
```bash
git checkout main
# Cherry-pick or manually copy the clean template files
# Commit as "Initial modular monolith template"
git push origin main
```

The approach: since `main` is an orphan branch, you can't cherry-pick directly. Instead:
1. Checkout `main`
2. Copy all files from `template/enhance-sample-modules` (excluding `.git/`)
3. Stage and commit as the initial template commit
4. Push

Alternative: `git checkout main && git checkout template/enhance-sample-modules -- .` to grab all files.

## Phase 4: Update Skills for Final Template

### Task 11: Final Skill Review
After the template is clean, review all 14 skills to ensure:
- No references to deleted modules remain
- File paths point to files that actually exist in the template
- Examples match the final code

## Key Conventions (for the next Claude instance)

### Code Style
- **Always use curly braces** on if/else — even single-line returns. User explicitly requires this.
- `sealed class` for all entities
- `Guid.CreateVersion7()` not `Guid.NewGuid()`

### Naming
- Integration events: `Guid Public{EntityName}Id`
- Cache entities: `Guid RefPublicId` (short form)
- Error codes: `{Entities}.{ErrorName}` (plural prefix)
- Columns: `snake_case`
- Indexes: `ix_{table}_{column}`

### Architecture
- Modules communicate ONLY through integration events
- Cache writer interfaces live in Domain layer
- Integration event handlers live in Presentation layer
- Commands return `Guid` (PublicId), never `int` (Id)
- Response DTOs never expose `int Id`

### Migrations
- Require `ENCRYPTION_KEY` env var: `MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=`
- SampleOrders context: `OrdersDbContext`
- SampleSales context: `SampleDbContext`

### Git
- NEVER run destructive git commands without user confirmation
- Current working branch: `template/enhance-sample-modules`
- `main` branch is an orphan (empty initial commit, no project-one history)
- `project-one-implementation` is the real project branch (preserved, don't touch)
