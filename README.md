# infotrack.scraper

A .NET 10 Web API + React/TypeScript SPA that extracts solicitor contact details by
location from public conveyancing listings and turns them into a standard report.

> **Status: scaffold.** The full stack is wired and runnable, but no scraping or domain
> logic exists yet. See [CONTEXT.md](CONTEXT.md) for the domain glossary and
> [docs/adr/0001-scaffold-architecture.md](docs/adr/0001-scaffold-architecture.md) for
> the architecture.

## Stack

| Concern  | Technology |
|----------|------------|
| API      | ASP.NET Core 10 minimal API, OpenAPI + Scalar |
| Data     | Postgres 17 (`Npgsql`) |
| Logging  | Serilog → Console + Seq |
| SPA      | React 19 + Vite + TypeScript |
| Tests    | xunit + `Microsoft.AspNetCore.Mvc.Testing` + AwesomeAssertions |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/) (for building/testing locally)
- Docker + Docker Compose v2 (for the containerised stack)
- Node 22+ (only if running the SPA outside Docker)

## Run everything with Docker (recommended)

A single command brings up Postgres, Seq, the API, and the SPA:

```bash
docker compose up -d --build
```

Then:

| Service        | URL                                   |
|----------------|---------------------------------------|
| API health     | http://localhost:5070/health          |
| API reference  | http://localhost:5070/scalar          |
| OpenAPI spec   | http://localhost:5070/openapi/v1.json |
| Seq (logs) UI  | http://localhost:5341                 |
| SPA            | http://localhost:5173                 |

Stop the stack (add `-v` to also remove the Postgres/Seq volumes):

```bash
docker compose down
```

Or bring up individual concerns by combining specific files:

```bash
docker compose -f docker-compose.infrastructure.yml -f docker-compose.logging.yml -f docker-compose.app.yml -f docker-compose.fe.yml up -d --build
```

## Build and test

```bash
dotnet build      # build the solution
dotnet test       # run the test suite (placeholder + OpenAPI smoke test)
```

## How to verify it works

1. `dotnet build` — succeeds with no warnings.
2. `dotnet test` — all tests pass.
3. `docker compose up -d --build`, then:
   - `curl http://localhost:5070/health` returns `{"status":"healthy"}` (proves the API
     reached Postgres).
   - Open http://localhost:5070/scalar — the API reference loads.
   - Hit the API a few times, then open http://localhost:5341 — request-logging events
     appear in Seq.
   - Open http://localhost:5173 — the SPA loads.
4. `docker compose down` — everything stops cleanly.

## Run the API or SPA outside Docker (optional)

```bash
# API — needs Postgres + Seq running (e.g. the two infra compose files above).
dotnet run --project Infotrack.Scraper

# SPA — Vite dev server on http://localhost:5173
cd infotrack-ui && npm install && npm run dev
```

The API's connection string and Seq URL live in
[Infotrack.Scraper/appsettings.json](Infotrack.Scraper/appsettings.json) and default to
`localhost`. In Docker they are overridden via environment variables in
`docker-compose.app.yml`.
