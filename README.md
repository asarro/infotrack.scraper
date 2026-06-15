# Infotrack Scraper

A tool that collects solicitor contact details from public conveyancing listing sites and
surfaces them through a searchable web UI. A background worker scrapes listings on a
schedule and stores results in Postgres; the API serves reads straight from the database.

## What it does

1. A **background worker** scrapes [solicitors.com](https://www.solicitors.com) for
   conveyancing solicitors across configured locations and stores the results in Postgres.
2. The **API** exposes `/conveyancing/solicitors?location=<name>` — it reads from the
   database and returns a `503 Retry-After: 60` while the first scrape pass is still
   running ("warming up").
3. The **SPA** lets you pick a location and browse the collected solicitors.

## Prerequisites

| Tool | Version |
|------|---------|
| Docker + Docker Compose v2 | any recent |
| .NET 10 SDK | only for local builds/tests |
| Node 22+ | only if running the SPA outside Docker |

---

## Quick start (Docker — recommended)

The stack has four compose files. Combine them with `-f` flags to bring up exactly what
you need.

### Spin up the full stack

```bash
docker compose \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.app.yml \
  -f docker-compose.fe.yml \
  up -d --build
```

| Service | URL | What you'll find |
|---------|-----|-----------------|
| SPA | http://localhost:5173 | Search UI — pick a location and browse solicitors |
| API health | http://localhost:5070/health | `{"status":"healthy"}` once Postgres is up |
| API reference | http://localhost:5070/scalar | Interactive Scalar docs for all endpoints |
| OpenAPI spec | http://localhost:5070/openapi/v1.json | Machine-readable spec |
| Seq (logs) | http://localhost:5341 | Structured log viewer |

> **First run:** the background worker needs a minute or two to complete its first scrape
> pass. Until then, the `/conveyancing/solicitors` endpoint returns `503` — this is
> expected. The SPA will show a "warming up" message. Refresh after ~60 seconds.

### Useful sub-stacks

Bring up only infrastructure (Postgres + Seq) while running the API locally:

```bash
docker compose \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.logging.yml \
  up -d
```

Bring up everything except the SPA (API dev without a frontend container):

```bash
docker compose \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.app.yml \
  up -d --build
```

### Tear down

```bash
# Stop containers, keep volumes (data survives restart)
docker compose \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.app.yml \
  -f docker-compose.fe.yml \
  down

# Stop AND wipe all data (Postgres + Seq volumes)
docker compose \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.app.yml \
  -f docker-compose.fe.yml \
  down -v
```

---

## Verify it's working

1. **API is up:** `curl http://localhost:5070/health` → `{"status":"healthy"}`
2. **Scraper warmed up:** open the SPA at http://localhost:5173, select a location — you
   should see a list of solicitors (not a "warming up" banner).
3. **Logs flowing:** hit the API a few times then check http://localhost:5341 — structured
   request events should appear in Seq.
4. **Interactive docs:** open http://localhost:5070/scalar and try
   `GET /conveyancing/locations` to see configured locations, then
   `GET /conveyancing/solicitors?location=london` to query results.

---

## Build and test locally

```bash
dotnet build   # build the solution
dotnet test    # run the test suite
```

Tests use `Microsoft.AspNetCore.Mvc.Testing` and hit a real Postgres instance. Make sure
the infrastructure stack is running before running tests locally.

---

## Run the API or SPA outside Docker

If you want faster iteration without rebuilding containers:

```bash
# 1. Start just Postgres + Seq
docker compose \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.logging.yml \
  up -d

# 2. Run the API (connects to localhost Postgres + Seq by default)
dotnet run --project Infotrack.Scraper

# 3. (Optional) Run the SPA dev server
cd infotrack-ui && npm install && npm run dev
```

Connection strings and the Seq URL are in
[Infotrack.Scraper/appsettings.json](Infotrack.Scraper/appsettings.json) and default to
`localhost`. In Docker they are overridden via environment variables in
`docker-compose.app.yml`.

---

## Scraping engine — how rules work

The scraper is config-driven. You describe *where* fields live in the HTML using a JSON
rules file; no C# changes are needed to target a new site or adjust existing selectors.

### How a rules file is structured

Rules files live in [Infotrack.Scraper/Resources/](Infotrack.Scraper/Resources/) and are
named `<site>.rules.json`. The existing file for solicitors.com is a good reference:

```jsonc
// Infotrack.Scraper/Resources/solicitors.com.rules.json
{
  // Selector for the repeating result block — every match becomes one Solicitor record.
  "ContainerSelector": ".result-item",

  "Fields": [
    {
      // "Field" must be one of: Name | Phone | Address | Description | Website
      "Field": "Name",
      // ".classname" matches by CSS class; a bare word (e.g. "a", "p") matches by tag.
      "Selector": ".h2",
      // Stop extracting text when this child selector is first encountered.
      "StopAt": ".greentick"
    },
    {
      "Field": "Phone",
      "Selector": ".phone-block",
      // Drill into a child element before extracting text.
      "ChildSelector": "a"
    },
    {
      "Field": "Address",
      "Selector": ".link-map",
      "ChildSelector": "address"
    },
    {
      "Field": "Description",
      "Selector": "p"
    },
    {
      "Field": "Website",
      "Selector": "a",
      // Read an HTML attribute value instead of the element's text content.
      "Attribute": "href",
      // Only match elements where this attribute equals this value.
      "AttributeFilter": { "Name": "target", "Value": "_blank" }
    }
  ]
}
```

### Field options at a glance

| Option | Required | Description |
|--------|----------|-------------|
| `Field` | yes | Which Solicitor field to populate (`Name`, `Phone`, `Address`, `Description`, `Website`) |
| `Selector` | yes | `.classname` to match by CSS class, or a bare tag name (`a`, `p`, `address`) |
| `ChildSelector` | no | After the outer element is found, drill into this child selector before extracting |
| `Attribute` | no | Read this HTML attribute's value (e.g. `href`) instead of the element's inner text |
| `AttributeFilter` | no | Narrow the selector to elements where `Name` attribute equals `Value` |
| `StopAt` | no | Truncate the extracted text at the first occurrence of this child selector |

### How to add a new target site

**1. Create the rules file**

Add `Infotrack.Scraper/Resources/<yoursite>.rules.json` following the structure above.
Inspect the target site's HTML (browser DevTools → Elements) to find the right selectors.

**2. Register the site in `appsettings.json`**

Add an entry to the `TargetSites` array:

```json
{
  "Name": "yoursite.com",
  "TargetUrl": "https://www.yoursite.com/conveyancing",
  "Locations": ["London", "Birmingham"],
  "ParsingRulesFile": "yoursite.com.rules.json"
}
```

`TargetUrl` is the base listing URL; the scraper appends each `Location` as a query
parameter when making requests.

**3. Verify**

Restart the API (`dotnet run` or `docker compose ... up -d --build`) and check Seq at
http://localhost:5341 for scrape log events. Once the worker completes its first pass,
`GET /conveyancing/solicitors?location=london` will include results from your new site.

---

## Stack

| Concern | Technology |
|---------|------------|
| API | ASP.NET Core 10 minimal API, OpenAPI + Scalar |
| Data | Postgres 17 (`Npgsql`) |
| Logging | Serilog → Console + Seq |
| SPA | React 19 + Vite + TypeScript |
| Tests | xunit + `Microsoft.AspNetCore.Mvc.Testing` + AwesomeAssertions |

## Further reading

- [CONTEXT.md](CONTEXT.md) — domain glossary (Solicitor, Location, ConveyancingSearch, Report)
- [docs/adr/0001-scaffold-architecture.md](docs/adr/0001-scaffold-architecture.md) — architecture decisions