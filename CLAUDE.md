# Infotrack.Scraper

## Project overview

A .NET 10 Web API + React/TypeScript SPA that will extract solicitor contact details by
location from public conveyancing listings and turn them into a standard report. This
repository is currently a **scaffold** — the stack is wired end-to-end but no scraping or
domain logic exists yet. See [CONTEXT.md](CONTEXT.md) for the domain glossary and
[docs/adr/0001-scaffold-architecture.md](docs/adr/0001-scaffold-architecture.md) for the
shape and reasoning.

## Stack

- **Runtime:** .NET 10 (current LTS)
- **API:** ASP.NET Core minimal APIs, OpenAPI + Scalar
- **Data:** Postgres 17 via `Npgsql`
- **Logging:** Serilog → Console + Seq (`datalust/seq`)
- **SPA:** React 19 + Vite + TypeScript
- **Tests:** xunit + `Microsoft.AspNetCore.Mvc.Testing` + AwesomeAssertions
- **Functional helpers:** `CSharpFunctionalExtensions` (API and test projects)

## Layout

```
Infotrack.Scraper.slnx
Infotrack.Scraper/            # API: Program.cs, appsettings*, Infotrack.Scraper.csproj
Test.Infotrack.Scraper/      # xunit tests
infotrack-ui/                # Vite + React + TS SPA (+ Dockerfile, nginx.conf)
Dockerfile                   # multi-stage build for the API
docker-compose.yml           # root; `include:`s the four files below
docker-compose.infrastructure.yml   # Postgres
docker-compose.logging.yml          # Seq
docker-compose.app.yml              # API
docker-compose.fe.yml               # SPA (nginx)
```

## Build, run, test

```bash
dotnet build                 # build the solution
dotnet test                  # run the test suite
dotnet run --project Infotrack.Scraper   # run the API locally (needs Postgres + Seq)

docker compose up -d --build # run the whole stack (Postgres + Seq + API + SPA)
docker compose down          # stop it (add -v to drop volumes)
```

Endpoints (Docker): API `http://localhost:5070` (`/health`, `/scalar`,
`/openapi/v1.json`), Seq UI `http://localhost:5341`, SPA `http://localhost:5173`.

## Conventions

- Minimal-API route handlers (`app.MapGet/Post/...`) — no controllers. Keep `Program.cs`
  thin; extract `IServiceCollection` extension methods as it grows.
- Return `TypedResults`/`Results.*` so OpenAPI infers response types.
- Nullable reference types are enabled — don't suppress nullability warnings.
- Log through Serilog; the Seq sink URL comes from configuration
  (`Serilog:WriteTo:1:Args:serverUrl`) and is overridden in `docker-compose.app.yml`.
- No third-party libraries for the eventual scraping logic — structure it by hand
  (per the task brief). Keep the domain language aligned with `CONTEXT.md`.
