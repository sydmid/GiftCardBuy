# GiftCardBuy

Production-grade Persian (fa-IR, RTL) digital gift-card commerce platform built with .NET 10. Integrates Gift-i-Card's 3-step procurement API (`/giftcard/buy`, `/giftcard/confirm`, `/giftcard/retrieve`) with idempotent transactional outbox fulfillment, at-rest code encryption, and decoupled Orchard Core CMS.

## Architecture

Modular monolith comprising:
- `src/GiftStore.Web`: ASP.NET Core 10 Razor Pages storefront (RTL/fa-IR) and back-office UI.
- `src/GiftStore.Application`: CQRS/Vertical slice handlers, order state machine, outbox orchestration.
- `src/GiftStore.Domain`: Domain entities, value objects, and events (Catalog, Orders, Payments, Codes).
- `src/GiftStore.Infrastructure`: EF Core 10 (PostgreSQL), Redis distributed cache, Quartz.NET background workers, ASP.NET Core Data Protection encryption.
- `src/GiftStore.Gifticard`: Typed API adapter for Gift-i-Card with resilient HTTP client and in-memory fake.
- `src/GiftStore.Payment`: Provider-neutral payment gateway abstraction with fake gateway and Zarinpal adapter.
- `src/GiftStore.Cms`: Decoupled Orchard Core CMS host with cached client and webhook invalidation.
- `src/GiftStore.AppHost` & `src/GiftStore.ServiceDefaults`: .NET Aspire orchestration and OpenTelemetry observability.

## Prerequisites

- .NET 10 SDK
- Docker & Docker Compose
- PostgreSQL 17 & Redis 7 (or run via Docker Compose)

## Configuration

Copy `.env.example` to `.env` and set environment variables:

```bash
cp .env.example .env
```

Key configuration sections in `appsettings.json`:
- `ConnectionStrings`: PostgreSQL (`DefaultConnection`) and Redis connection strings.
- `Gifticard`: API credentials (`ApiToken`, `ConsumerKey`, `ConsumerSecret`), environment (`Sandbox`/`Production`), and `UseFakeInDevelopment`.
- `Payment`: Selected gateway provider (`Fake` or `Zarinpal`), callback URL.
- `Cms`: Base URL for decoupled Orchard Core instance and webhook secret.

## Running Locally

### Docker Compose (Recommended)

Starts PostgreSQL, Redis, Orchard Core CMS, and the Web Storefront:

```bash
docker-compose up -d --build
```

- **Storefront**: `http://localhost:7001`
- **CMS Admin**: `http://localhost:5050/admin`
- **Health Checks**: `http://localhost:7001/health`, `http://localhost:7001/alive`, `http://localhost:7001/ready`

### .NET CLI

```bash
# Restore dependencies
dotnet restore

# Run Web application
dotnet run --project src/GiftStore.Web

# Or run via Aspire AppHost
dotnet run --project src/GiftStore.AppHost
```

## Running Tests

```bash
dotnet test
```

Test suites included:
- `tests/GiftStore.Domain.Tests`: Order state transitions, price rules, and margin calculations.
- `tests/GiftStore.Application.Tests`: Payment verification idempotency and checkout logic.
- `tests/GiftStore.Gifticard.IntegrationTests`: 3-step procurement workflow against supplier contract.
- `tests/GiftStore.Infrastructure.Tests`: PII and sensitive voucher code redaction.
- `tests/GiftStore.Web.E2ETests`: Razor pages RTL markup verification.
