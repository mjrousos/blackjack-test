# Blackjack

A browser-based, single-player Blackjack game built with .NET 10 and Blazor Server. Players compete against a computer dealer using virtual chips, with full account management, persistent game history, and a casino-inspired UI.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
  - [Project Layout](#project-layout)
  - [Layer Overview](#layer-overview)
  - [Key Design Decisions](#key-design-decisions)
- [Prerequisites](#prerequisites)
- [Build & Run](#build--run)
  - [Docker Compose (recommended)](#docker-compose-recommended)
  - [Local Development (without Docker)](#local-development-without-docker)
- [Running Tests](#running-tests)
- [Game Rules & Configuration](#game-rules--configuration)

---

## Features

- Register/login with email and password (ASP.NET Core Identity)
- Play Blackjack against a computer dealer (Hit, Stand, Double Down, Split)
- 6-deck shoe with configurable bet limits ($5 minimum, $500 maximum)
- Starting balance of $1,000 virtual chips; free refill when balance runs low
- Dashboard showing current balance and game statistics
- Paginated game history with hand details and outcomes
- Responsive, casino-themed design that works on desktop, tablet, and mobile
- Customizable color themes: Classic Casino (green), Midnight Blue (navy), and Royal Purple

---

## Architecture

### Project Layout

```
Blackjack/
├── src/
│   ├── Blackjack.Domain/          # Core game logic — no external dependencies
│   │   ├── Models/                # Card, Hand, Shoe, GameState, GameSettings, …
│   │   └── Services/              # BlackjackGame (state machine), IGameService
│   ├── Blackjack.Infrastructure/  # Data access and identity
│   │   ├── Data/                  # EF Core DbContext (AppDbContext)
│   │   ├── Identity/              # ASP.NET Core Identity stores / setup
│   │   └── Repositories/         # IGameRepository implementation (SQL Server)
│   └── Blackjack.Web/             # Blazor Server application
│       ├── Pages/                 # Game.razor, Dashboard.razor, History.razor
│       ├── Layout/                # MainLayout + scoped CSS
│       ├── Controllers/           # AccountController (login/register/logout via MVC)
│       ├── Services/              # GameSessionService (per-circuit game state)
│       └── wwwroot/               # Static assets (CSS, card images)
├── tests/
│   ├── Blackjack.Domain.Tests/    # xUnit unit tests for game logic (116 tests)
│   ├── Blackjack.Infrastructure.Tests/ # xUnit integration tests with EF InMemory (13 tests)
│   ├── Blackjack.Web.Tests/       # bUnit component tests (46 tests)
│   └── Blackjack.E2E.Tests/       # Playwright end-to-end tests (requires Docker)
├── Dockerfile
├── docker-compose.yml
└── Blackjack.slnx
```

### Layer Overview

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `Blackjack.Domain` | Pure game rules: shuffling, dealing, scoring, state transitions. No I/O or framework dependencies. |
| **Infrastructure** | `Blackjack.Infrastructure` | EF Core data model, SQL Server migrations, Identity configuration, repository implementations. |
| **Web** | `Blackjack.Web` | Blazor Server UI, MVC auth endpoints, per-circuit `GameSessionService` that bridges the domain service with persistence. |

Authentication pages (login, register, logout) are implemented as MVC Razor views rather than Blazor components because ASP.NET Core Identity requires full HTTP request/response cycles to set authentication cookies, which are not available over a SignalR connection.

### Key Design Decisions

- **`BlackjackGame` state machine** — The domain service is a self-contained state machine with states `WaitingForBet`, `PlayerTurn`, `DealerTurn`, and `Resolved`. It enforces valid action sequences and throws if an invalid action is attempted.
- **`GameSessionService`** — A Blazor-scoped service that wraps `BlackjackGame`, hydrates it from the database on first access, and persists completed rounds. This keeps the domain layer free of persistence concerns.
- **CSS isolation** — Each `.razor` component has a paired `.razor.css` file for scoped styles. The isolation bundle (`Blackjack.Web.styles.css`) is referenced explicitly in `App.razor`.
- **6-deck shoe** — Configurable via `GameSettings`; the shoe is re-shuffled automatically when fewer than 25% of cards remain.

---

## Prerequisites

| Tool | Purpose |
|---|---|
| [.NET 10 SDK (preview)](https://dotnet.microsoft.com/download/dotnet/10.0) | Build and run locally |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | Run with Docker Compose (includes SQL Server) |

---

## Build & Run

### Docker Compose (recommended)

This is the simplest way to run the full stack. Docker Compose starts both a SQL Server 2022 container and the web application.

```bash
# Clone the repo
git clone https://github.com/mjrousos/Blackjack.git
cd Blackjack

# Build and start all services
docker compose up --build
```

The application will be available at **http://localhost:8080**.

SQL Server data is persisted in the `sqlserver-data` Docker volume between runs.

To stop and remove containers:

```bash
docker compose down
```

To also remove the database volume:

```bash
docker compose down -v
```

### Local Development (without Docker)

You need a SQL Server instance (local or remote) and the .NET 10 SDK.

1. **Set the connection string** — update `src/Blackjack.Web/appsettings.Development.json` or set an environment variable:

   ```bash
   export ConnectionStrings__DefaultConnection="Server=localhost;Database=BlackjackDb;User Id=sa;Password=<your-password>;TrustServerCertificate=true"
   ```

2. **Apply database migrations** — the application runs migrations automatically on startup, so no manual step is needed. If you want to apply them manually:

   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project src/Blackjack.Infrastructure --startup-project src/Blackjack.Web
   ```

3. **Build and run:**

   ```bash
   dotnet build Blackjack.slnx
   dotnet run --project src/Blackjack.Web
   ```

   The application will be available at the URL printed in the console (typically **http://localhost:5000** or **https://localhost:5001**).

---

## Running Tests

Unit and integration tests do not require Docker or a running database:

```bash
# Run all non-E2E tests
dotnet test Blackjack.slnx --filter "Category!=E2E"
```

End-to-end tests use Playwright and require a running application:

```bash
# Start the app with Docker Compose first
docker compose up --build -d

# Set the application URL
export APP_URL=http://localhost:8080

# Run E2E tests
dotnet test tests/Blackjack.E2E.Tests
```

---

## Game Rules & Configuration

The game follows standard casino Blackjack rules:

- Dealer hits on soft 16, stands on soft 17
- Blackjack pays 3:2
- Double down is available on any first two cards
- Splitting is available when both cards share the same rank
- No insurance or surrender

Default configuration (editable in `appsettings.json` under the `GameSettings` section):

| Setting | Default |
|---|---|
| Number of decks | 6 |
| Minimum bet | $5 |
| Maximum bet | $500 |
| Starting balance | $1,000 |
| Reshuffle threshold | 25% of shoe remaining |

---

## Themes

The game includes three color themes that can be switched at any time using the 🎨 button in the top navigation bar:

| Theme | Description |
|---|---|
| **Classic Casino** | Traditional green felt with gold accents (default) |
| **Midnight Blue** | Deep navy background with ice-blue accents |
| **Royal Purple** | Rich purple felt with rose-gold accents |

Your theme preference is saved in the browser and persists across sessions, including on the login and registration pages. No account or server-side changes are needed — themes are applied entirely on the client side via CSS custom properties and `localStorage`.
