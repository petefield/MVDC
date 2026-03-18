# Copilot Instructions for MVDC

## Project Overview

**MVDC** (Mole Valley District Club) is a .NET 10 application for managing **Mole Valley Girls Football Club (MVGFC)**. It consists of a Blazor WebAssembly frontend, an ASP.NET Core API backend, Azure Cosmos DB for storage, and a standalone NuGet library (`MVDC.FullTime`) for scraping FA Full-Time fixture data.

## Architecture

```
Browser (Blazor WASM) → REST API calls → ASP.NET Core API → Azure Cosmos DB
                                                          → FA Full-Time website (scraping)
nginx (Docker) → serves Blazor static files
```

> The Blazor WASM app runs entirely in the browser. nginx only serves static files — all API calls go directly from the browser to the API on port 5001.

## Project Structure

```
MVDC/
├── src/
│   ├── MVDC.Api/          # ASP.NET Core Web API (.NET 10)
│   │   ├── Controllers/   # 8 API controllers
│   │   ├── Identity/      # CosmosUserStore, CosmosRoleStore
│   │   ├── Services/      # CosmosRepository<T>, FullTimeFixtureSeeder
│   │   └── Program.cs     # DI, Cosmos init, seeding, middleware
│   ├── MVDC.Web/          # Blazor WebAssembly frontend (.NET 10)
│   │   ├── Auth/          # JWT auth state provider + handler
│   │   ├── Pages/         # 10 Blazor pages
│   │   └── Program.cs
│   ├── MVDC.Shared/       # Shared models (referenced by Api + Web)
│   │   └── Models/        # Player, Coach, Team, Fixture, Parent, etc.
│   ├── MVDC.FullTime/     # Standalone NuGet library for FA Full-Time scraping
│   │   ├── Parsers/       # TeamParser, FixtureParser, ResultParser
│   │   ├── Formatters/    # FixtureFormatter, ResultFormatter
│   │   ├── Division.cs    # Main facade class
│   │   └── FullTimeClient.cs
│   └── MVDC.FullTime.Console/  # CLI tool for FullTime library
└── tests/
    └── MVDC.FullTime.Tests/    # 59 xUnit tests
        └── Examples/           # HTML fixture files for parser tests
```

> **Note:** `MVDC.slnx` only includes Api, Web, and Shared. The FullTime library and tests must be built/tested by path.

## Build Commands

```bash
# Build solution (Api, Web, Shared)
dotnet build

# Build FullTime library separately
dotnet build src/MVDC.FullTime/

# Build tests
dotnet build tests/MVDC.FullTime.Tests/
```

## Test Commands

```bash
# Run all tests
dotnet test tests/MVDC.FullTime.Tests/

# Run a specific test class
dotnet test tests/MVDC.FullTime.Tests/ --filter "FullyQualifiedName~FixtureParserTests"

# Verbose output
dotnet test tests/MVDC.FullTime.Tests/ --verbosity normal
```

Tests use xUnit and NSubstitute. HTML fixtures for parser tests live in `tests/MVDC.FullTime.Tests/Examples/` and are loaded via `File.ReadAllText`.

## Data Model

All entities are stored in a single Cosmos DB container (`Items`) with partition key `/id`. A `documentType` field discriminates between entity types.

| Entity | Document Type | ID Strategy |
|--------|--------------|-------------|
| Player | `Player` | GUID |
| Coach | `Coach` | GUID |
| Team | `Team` | Deterministic slug (`mv-u9-blacks`) |
| Parent | `Parent` | GUID |
| Fixture | `Fixture` | SHA256 deterministic hash |
| MatchReport | `MatchReport` | GUID |
| PlayerAvailability | `PlayerAvailability` | GUID |
| ApplicationUser | `User` | GUID (Identity-managed) |

### Repository Pattern

All entities use a generic `IRepository<T>` backed by `CosmosRepository<T>`:

```csharp
GetAllAsync(CancellationToken)         → IEnumerable<T>
GetByIdAsync(string id, CT)            → T?
CreateAsync(T item, CT)                → T
UpdateAsync(string id, T item, CT)     → T       // uses UpsertItemAsync
DeleteAsync(string id, CT)             → void
```

## Authentication and Authorization

- JWT Bearer tokens (HMAC-SHA256, 24h expiry by default)
- Three roles: **Admin**, **Coach**, **Parent** (Parent is the default role for new registrations)
- JWT claims: `NameIdentifier`, `Email`, `Name`, `Role`
- `JwtAuthenticationStateProvider` provides auth state in Blazor WASM
- Token stored in `localStorage`

### Role Permissions

| Role | Read (own) | Read (all) | Create/Update | Delete | Register Users |
|------|------------|------------|---------------|--------|----------------|
| Admin | Yes | Yes | Yes | Yes | Yes |
| Coach | Yes | Yes | Yes | No | No |
| Parent | Yes | Yes | No | No | No |

### Parent–Player Relationship

`Parent.PlayerIds` (List\<string\>) links parents to their children (players). A parent's email matches `ApplicationUser.Email` to identify which `Parent` record belongs to the logged-in user.

## Coding Conventions

- **C# / .NET 10** — use modern C# features (records, nullable reference types, pattern matching)
- **Blazor pages** use `EditForm` with `DataAnnotationsValidator` for forms
- **Error display** uses Bootstrap dismissible alerts
- **Delete confirmations** use JS `confirm()` via `IJSRuntime`
- **API controllers** use `[ApiController]` and return standard HTTP status codes (`200`, `201 Created` with `Location` header, `204 No Content`, `400`, `401`, `403`, `404`, `409 Conflict`)
- **Models** in `MVDC.Shared` are annotated with `[Required]`, `[EmailAddress]`, `[Phone]` data annotations
- **Fixture IDs** are a SHA256 hash of `{date}|{home}|{away}|{groupKey}`, truncated to 16 hex characters

## UI / Branding

| Element | Value |
|---------|-------|
| Primary accent | `#00a14b` (MVGFC green) |
| Heading font | Oswald (uppercase) |
| Body font | Open Sans |
| Content width | Max 1080px |

Match the [MVGFC website](https://www.mvgfc.co.uk/) branding for any new UI components.

## Public API Endpoints (no auth required)

- `GET /api/fixtures`, `GET /api/fixtures/{id}`
- `GET /api/matchreports`, `GET /api/matchreports/{id}`
- `POST /api/auth/login`

## Docker / Local Development

```bash
# Start everything (Cosmos DB emulator, API, Web)
docker compose build && docker compose up -d
```

| Service | URL |
|---------|-----|
| Web (Blazor) | http://localhost:5080 |
| API | http://localhost:5001 |
| Cosmos DB | https://localhost:8081 |

Default admin: `admin@mvgfc.co.uk` / `Admin123!`
