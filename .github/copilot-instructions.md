# Copilot Instructions — Blackjack

## Quick Reference

```bash
# Build
dotnet build

# Run (Docker Compose — includes SQL Server)
docker compose up --build        # http://localhost:8080

# Run (local — requires .NET 10 SDK, external SQL Server)
dotnet run --project src/Blackjack.Web

# Test
dotnet test                                    # all tests
dotnet test tests/Blackjack.Domain.Tests       # unit tests (fast)
dotnet test tests/Blackjack.Infrastructure.Tests  # EF InMemory integration
dotnet test tests/Blackjack.Web.Tests          # bUnit component tests
dotnet test tests/Blackjack.E2E.Tests          # Playwright (requires Docker running)
```

## Architecture

Three-layer clean architecture. See [README.md](../README.md#architecture) for full details.

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `Blackjack.Domain` | Pure game logic — no external dependencies. `BlackjackGame` is a state machine (`WaitingForBet` → `Dealing` → `PlayerTurn` → `DealerTurn` → `Resolved`). |
| **Infrastructure** | `Blackjack.Infrastructure` | EF Core + SQL Server, ASP.NET Core Identity, repository implementations. |
| **Web** | `Blackjack.Web` | Blazor Server UI (`InteractiveServer` render mode), MVC auth endpoints, `GameSessionService` (per-circuit). |

**Key boundary rule**: Domain has zero external dependencies. All I/O lives in Infrastructure. Web bridges the two via `GameSessionService`.

Auth pages (login/register/logout) use MVC Razor views, not Blazor, because Identity cookies require full HTTP request/response (unavailable over SignalR).

## Conventions

### C# Style
- **.NET 10**, nullable reference types enabled, implicit usings enabled
- **Records** for immutable value types (`Card`, `GameSettings`)
- **Switch expressions** and pattern matching over if/else chains
- **Expression-bodied members** for simple property forwarding
- **Constructor validation** — throw `ArgumentOutOfRangeException` / `InvalidOperationException` early
- **Async at I/O boundaries only** — repositories are `async Task<T>`; domain game actions are synchronous (pure logic)

### Blazor
- Render mode: `@rendermode RenderMode.InteractiveServer`
- **CSS isolation**: each `.razor` component has a paired `.razor.css` file
- **Parameter binding**: `[Parameter]` attributes; computed CSS classes via properties
- Event handlers: `@onclick` for sync actions; async lifecycle methods for I/O

### Dependency Injection
- Constructor injection throughout
- Infrastructure registers services via `AddBlackjackInfrastructure(connectionString)` extension method
- `GameSessionService` is registered as scoped (one per Blazor circuit)

### Database
- SQL Server 2022 via EF Core 10
- `BlackjackDbContext` extends `IdentityDbContext<ApplicationUser>`
- Decimal precision `(18, 2)` for monetary values
- `GameResult` stored as string via value conversion
- `db.Database.EnsureCreated()` in Development only (no migrations checked in)

## Testing

| Project | Framework | Pattern |
|---------|-----------|---------|
| `Domain.Tests` | xUnit + FluentAssertions | Seeded `Random` for deterministic shuffling; helper methods to create games in specific states |
| `Infrastructure.Tests` | xUnit + EF Core InMemory | Repository tests without SQL Server |
| `Web.Tests` | xUnit + bUnit | `Render<Component>(p => p.Add(...))`, DOM assertions via `cut.Find()` |
| `E2E.Tests` | xUnit + Playwright | `PlaywrightFixture` launches headless Chromium; `CreateAuthenticatedPageAsync()` for auth; unique emails per test via `Guid` |

When writing tests:
- Use FluentAssertions (`.Should()`) for assertions
- Use `[Theory]` / `[InlineData]` for parameterized tests
- Domain tests should not require any I/O — use seeded `Random` for reproducibility
- E2E tests must wait for Blazor circuit: `WaitForLoadStateAsync(NetworkIdle)` before interacting

## Pitfalls

- **State machine enforcement**: `BlackjackGame` throws if actions are called in the wrong state. Always check `State` or `AvailableActions` before calling game methods.
- **Blazor SignalR**: UI updates happen over SignalR — don't assume HTTP semantics. Auth requires MVC endpoints.
- **E2E tests require Docker**: The Playwright tests run against the Docker Compose environment. Start `docker compose up` before running them.
- **Shoe reshuffling**: The shoe auto-reshuffles at 25% remaining cards. Tests using seeded randomness must account for this.
