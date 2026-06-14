# Infotrack.Scraper

A .NET Web API + React SPA that extracts solicitor contact details by location from
public conveyancing listings and turns the results into a standard report. This file
is the canonical glossary for the project — definitions only, no implementation detail.

> The scaffold is in place; none of the domain terms below are implemented yet. They
> are recorded so the language is settled before the scraping logic is built.

## Language

**Solicitor**:
A legal services provider returned by a conveyancing listing search — the entity whose
name, location, and contact details we extract.
_Avoid_: lawyer, firm, provider, vendor

**Location**:
A user-selectable place that scopes a search (e.g. London, Birmingham, Leeds). The SPA
lets the user adjust the list of locations to search.
_Avoid_: city, region, area

**ConveyancingSearch**:
A single submission against the listing site for one Location, producing the raw set of
Solicitors found for that Location.
_Avoid_: query, lookup, scrape job

**Report**:
The standardised layout derived from the Solicitors gathered across one or more
ConveyancingSearches — the "data turned into insight" deliverable.
_Avoid_: results, output, summary

## Example dialogue

> "Run a ConveyancingSearch for Leeds and Manchester."
> "Done — Leeds returned 12 Solicitors, Manchester 9. Want me to build the Report across
> both Locations, or keep them separate?"
