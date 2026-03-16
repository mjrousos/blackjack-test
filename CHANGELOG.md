# Changelog

All notable changes to the Blackjack project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - 2026-03-05

Initial release of the Blackjack web application.

### Added

- Single-player Blackjack game against a computer dealer built with Blazor Server and .NET 10
- Core game actions: Hit, Stand, Double Down, and Split
- 6-deck shoe with configurable shuffle and bet limits ($5 min, $500 max)
- Starting balance of $1,000 virtual chips with free refill when balance is low
- User registration and login with ASP.NET Core Identity
- Player dashboard showing current balance and game statistics
- Paginated game history with hand details and outcomes
- Responsive, casino-themed UI for desktop, tablet, and mobile
- SQL Server persistence via Entity Framework Core
- Docker Compose support for containerized deployment
- Unit tests for domain game logic (xUnit)
- Integration tests for data access layer (EF Core InMemory)
- Component tests for Blazor UI (bUnit)
- End-to-end tests with Playwright

[1.0.0]: https://github.com/mjrousos/Blackjack/commits/425230e
