# 0001 — Scaffold architecture and conventions

Status: accepted

## Context

Infotrack.Scraper starts as a small, focused service: a .NET Web API paired with a
single-page app that will eventually scrape solicitor contact details and produce a
report. Before any domain logic exists we need a runnable skeleton that establishes
the stack, the local run story, and the project conventions.

## Decision

- **.NET 10 minimal API** for the backend. The surface area is small and request-scoped;
  minimal-API route handlers keep the composition root (`Program.cs`) readable without
  the ceremony of controllers. `public partial class Program` is exposed so integration
  tests can boot the app through `WebApplicationFactory`.
- **OpenAPI + Scalar** for an always-available, browsable API reference during
  development. The built-in `Microsoft.AspNetCore.OpenApi` emits the spec; `Scalar`
  renders it. Both are wired only in the Development environment.
- **Postgres** as the datastore, reached through `Npgsql`. The scaffold wires the
  connection and exposes a `/health` endpoint that proves connectivity, but contains no
  queries or schema yet.
- **Serilog → Console + Seq** for structured, queryable diagnostics. A two-stage
  bootstrap logger captures startup failures; the full logger reads from configuration
  so the Seq endpoint can be overridden per environment.
- **React + TypeScript (Vite)** for the SPA. TypeScript is chosen up front for type
  safety as the UI grows beyond the scaffold.
- **Split `docker-compose` files joined by a root `include:`** — one file per concern
  (infrastructure / logging / api / fe). Each file is runnable on its own, and
  `docker compose up` brings up the whole stack with one command. This keeps the data
  and logging dependencies usable independently during local development.

## Consequences

- The stack runs with minimum configuration: `docker compose up` is the single entry
  point, satisfying the "clone and run" requirement.
- Adding the database client (`Npgsql`) and a UI framework now means the first domain
  feature has no infrastructure yak-shaving to do.
- The split-compose layout means contributors must know which file (or the root) to run;
  this is documented in the README.
- Reversing the minimal-API or split-compose choices later would touch the composition
  root and the entire compose layout, which is why the shape is recorded here.
