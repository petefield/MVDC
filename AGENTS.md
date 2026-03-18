# AGENTS.md -- Coding Agent Instructions for MVDC

## Project overview

.NET 10 application for managing a girls' football club (MVGFC). Blazor WASM frontend, ASP.NET Core API backend, Azure Cosmos DB storage, and a standalone NuGet library (`MVDC.FullTime`) that scrapes FA Full-Time for fixtures/results.

## Build, test, and run commands

```bash
# Build entire solution (only includes Api, Web, Shared)
dotnet build

# Build all projects including FullTime library and tests
dotnet build src/MVDC.FullTime/
dotnet build tests/MVDC.FullTime.Tests/

# Run all tests (59 xUnit tests)
dotnet test tests/MVDC.FullTime.Tests/

# Run a single test by fully-qualified name
dotnet test tests/MVDC.FullTime.Tests/ --filter "FullyQualifiedName~FixtureParserTests.Parse_ReturnsSevenFixtures"

# Run all tests in a single test class
dotnet test tests/MVDC.FullTime.Tests/ --filter "FullyQualifiedName~FixtureParserTests"

# Run with verbose output
dotnet test tests/MVDC.FullTime.Tests/ --verbosity normal

# Docker Compose (local dev with Cosmos DB emulator)
docker compose build
docker compose up -d
# Ports: cosmosdb=8081, api=5001, web=5080
```

**Important:** The solution file (`MVDC.slnx`) only includes Api, Web, and Shared. The FullTime library and tests must be built/tested by path. There are no linters or analyzers configured.

## Project structure

```
src/MVDC.Api/           ASP.NET Core Web API (controllers, identity, services)
src/MVDC.Web/           Blazor WebAssembly frontend (pages, auth, layout)
src/MVDC.Shared/        Shared models referenced by both Api and Web
src/MVDC.FullTime/      Standalone NuGet library (FA Full-Time scraper)
src/MVDC.FullTime.Console/  CLI tool for FullTime library
tests/MVDC.FullTime.Tests/  xUnit tests for FullTime library
```

All projects target `net10.0` with `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>`.

## Code style

### Namespaces and usings
- **File-scoped namespaces only**: `namespace MVDC.Api.Controllers;`
- Namespace follows `MVDC.{Project}.{Folder}` pattern
- Rely on implicit usings; add explicit `using` only for non-implicit types
- xUnit requires explicit `using Xunit;` on .NET 10
- No `using static` or global using files

### Naming conventions
| Element | Convention | Example |
|---|---|---|
| Classes | PascalCase | `FixtureParser`, `CosmosRepository` |
| Interfaces | `I` + PascalCase | `IFullTimeClient`, `IRepository<T>` |
| Methods | PascalCase, async suffix | `GetAllAsync`, `SeedAsync` |
| Properties | PascalCase | `public string Name { get; set; }` |
| Private fields | `_camelCase` | `private readonly HttpClient _httpClient;` |
| Constants | PascalCase | `private const string DefaultDateFormat = "dd/MM/yyyy";` |
| Static readonly | PascalCase | `private static readonly TeamConfig[] Teams = [...]` |
| Locals/params | camelCase | `var cosmosClientOptions`, `string html` |

### Formatting
- **4-space indentation** (no tabs)
- **Allman brace style** (opening brace on its own line)
- Explicit access modifiers on all types and members
- `var` used pervasively for local type inference
- Expression-bodied members for single-line methods/properties
- Target-typed `new()` for type-obvious constructions
- C# 12 collection expressions: `[]` for empty/inline arrays

### Types and patterns
- **Records** (`sealed record`) for immutable DTOs: `FormattedFixture`, `FormattedResult`
  - Use `required` and `init` properties: `public required string Date { get; init; }`
- **Classes** for mutable entity models: `Player`, `Team`, `Fixture`
  - Use `{ get; set; }` with defaults; `DocumentType` uses `{ get; init; }`
- **`sealed`** on concrete classes not designed for inheritance
- **`static partial class`** for source-generated regex via `[GeneratedRegex]`
- Data annotations for validation: `[Required]`, `[EmailAddress]`, `[Phone]`, `[MinLength]`

### Null handling
- Nullable reference types enabled everywhere
- Use `is null` / `is not null` (not `== null`)
- Guard with `?? throw new ArgumentNullException(nameof(param))`
- `string.Empty` as default for non-nullable string properties

### Async/await
- All async methods return `Task`/`Task<T>` with `Async` suffix
- `CancellationToken` parameter on all async methods (with `= default` in library code)
- Controllers accept `CancellationToken cancellationToken` as action parameter
- Do NOT use `ConfigureAwait(false)` (ASP.NET Core convention)
- `Task.WhenAll` for parallel loading in Blazor pages

### Error handling
- Pattern-matched exceptions: `catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)`
- Structured logging with `ILogger` and message templates (not string interpolation)
- Graceful degradation: non-critical failures log warnings and continue
- HTTP status codes: `NotFound()`, `BadRequest()`, `Unauthorized()`, `Conflict()`, `NoContent()`, `Ok()`, `CreatedAtAction()`
- Blazor pages wrap API calls in try/catch with dismissible Bootstrap alert error messages

### API controller conventions
- Inherit `ControllerBase` (not `Controller`)
- Attributes: `[ApiController]`, `[Route("api/[controller]")]`
- Class-level `[Authorize]`, with `[AllowAnonymous]` on public read endpoints
- Role-based: `[Authorize(Roles = "Admin,Coach")]` for writes, `[Authorize(Roles = "Admin")]` for deletes
- Consistent CRUD pattern: `GetAll`, `GetById`, `Create`, `Update`, `Delete`
- Update checks `id != entity.Id` -> `BadRequest()`
- Create returns `CreatedAtAction(nameof(GetById), ...)`

### Blazor conventions
- `EditForm` with `DataAnnotationsValidator` and `ValidationSummary` for form validation
- `InputText`, `InputDate`, `InputNumber`, `InputSelect`, `InputTextArea` components (not raw HTML inputs)
- `@inject` for services, `@code { }` for logic
- `<PageTitle>` on every page
- JS interop for `confirm()` dialogs via `IJSRuntime`
- Error display via Bootstrap dismissible alerts

### Dependency injection
- Constructor injection only
- `IRepository<T>` registered as scoped per entity type via factory
- `CosmosClient` as singleton
- No third-party DI container

## Test conventions

- **Framework:** xUnit (2.9.3) with `[Fact]` attributes (no `[Theory]` used)
- **Mocking:** NSubstitute -- `Substitute.For<T>()`, `.Returns()`, `.Received()`
- **Assertions:** xUnit built-in only (`Assert.Equal`, `Assert.NotEmpty`, etc.)
- **File naming:** `{ClassUnderTest}Tests.cs` mirroring source folder structure
- **Method naming:** `MethodName_Scenario_ExpectedBehavior` with underscores
- **Setup:** Constructor-based (field-level readonly instances, no `[SetUp]`)
- **Test data:** HTML files in `tests/MVDC.FullTime.Tests/Examples/`, loaded via `File.ReadAllText`
- **Helper methods:** `private static` for test data construction

## Database

- Azure Cosmos DB with single container `Items`, partition key `/id`
- `documentType` field discriminates entity types (camelCase serialization)
- Deterministic IDs where appropriate (SHA256 hash for fixtures)
- Generic repository: `IRepository<T>` with `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync` (upsert), `DeleteAsync`

## Docker

- Multi-stage builds with non-root `appuser` for both images
- API image: `aspnet:10.0` (Ubuntu) -- use `useradd` (not `adduser`)
- Web image: `nginx:alpine` -- use `adduser -D -H` (Alpine syntax)
- Cosmos DB emulator: `mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`
