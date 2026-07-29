Add a "Trucking in {City}" data module to the directory City pages (`Views/Home/City.cshtml`) for cities with 50+ active companies. It replaces the role of generated articles going forward: facts as structured data plus COMPUTED "what stands out" highlights — no generated prose anywhere. Everything renders server-side from already-cached ViewModels; the module must never break or slow the page.

First, study: `Views/Home/City.cshtml` + `HomeController.City()` (structure; where the article block renders; note the article renders on page 1 only — the module follows the same rule), `GetCityCompaniesData()` / `GetStateCompaniesData()` / `GetStatisticsData()` / `GetNewRegistrationsData()` / `GetCargoData()` (the cached VMs that supply city numbers and state/national baselines), and the homepage band markup/CSS (`.hpb-card`, `.hpb-icon` in `modern-theme.css`) — the module reuses that card styling for visual consistency.

Data and gating:
* In `HomeController.City()`, page 1 only: call `GetCityCompaniesData(stateCode, city, "24m")` inside a try/catch. If it returns null, throws, or `TotalActiveCompanies < 50` → no module, render nothing, page unaffected. Same defensive pattern as the homepage teaser.
* Baselines come from the cached statistics VMs (national: `GetStatisticsData()`, `GetCargoData()`, `GetNewRegistrationsData()`; state: `GetStateCompaniesData(stateCode)`) — all 30-day cached and mostly pre-warmed; zero heavy queries added. If any baseline VM is unavailable, skip only the highlights that need it.
* All module numbers use the all-active base (directory context — matches the listing counts on the same page). No trucking-companies-minus-brokers math here.

Module layout (place below the article block if present, above the company list; render on page 1 only):
1. Heading: `<h2>Trucking in {CityName}, {StateCode}</h2>` (title-cased city).
2. Four compact stat cards (homepage-band styling, icon circles):
   * Active Companies (count; sub: "% of {StateName} total" from the city VM).
   * Median Fleet Size (sub: "of companies reporting fleet data").
   * New Registrations — Last 24 Months (city VM field; sub: "avg N/month").
   * Owner-Operator Share (city VM percent; sub: "of companies reporting fleet size").
   Omit any card whose underlying data is missing/zero-sample rather than showing 0/N/A.
3. Top cargo types — a compact 5-row list (cargo name, % of companies) from the city VM. Only if the city has ≥30 companies with cargo classifications; footnote "companies may select multiple cargo types".
4. **"What stands out" — the computed highlights (the heart of the module):**
   * Candidate metrics, each compared as a ratio city-vs-baseline:
     a. Each of the city's top cargo shares vs the NATIONAL share of that cargo type (state cargo baselines don't exist — national only). Min 30 classified companies.
     b. Owner-operator % vs state and national. Min 20 reporting.
     c. Large-fleet share (21+ units) vs national. Min 20 reporting.
     d. Broker share (any-B companies as % of active) vs national. Min 50 active.
     e. Registration growth: city last-12-months vs previous-12-months (from the city VM's monthly rows), compared against the national growth rate over the same windows. Min 30 registrations across the 24 months.
   * A highlight qualifies when the ratio is ≥2.0 or ≤0.5 (for growth: city growth exceeds national by ≥15 percentage points either direction). Rank qualifying highlights by how extreme the ratio is; render at most 3, each as one plain sentence with the number and the multiple, e.g.:
     - "Fresh produce haulers make up 42% of classified carriers here — 6.2× the U.S. share."
     - "Registrations grew 31% year over year, versus 8% nationally."
     - "Only 0.4% of companies are freight brokers — a fraction of the 2.9% U.S. share."
   * Sentences are ASSEMBLED from data (string templates with slots), not generated — but write 2–3 template variants per metric and pick by city-name hash so adjacent cities don't all share identical sentence openers.
   * If nothing qualifies, omit the section entirely — facts-only modules are fine.
5. Footer links row: "Full {City} trucking statistics →" to `/statistics/city/{ST}/{CITY}` (uppercase state, URL-formatted city exactly as the statistics pages generate it) and "{StateName} statistics →" to `/statistics/state/{ST}`. If a growth highlight rendered, it links to `/statistics/new-registrations`.

Implementation rules:
* Styling: reuse `.hpb-card` / `.hpb-icon` classes (add module-specific classes to modern-theme.css only where needed); must render correctly in the Bootstrap 3 grid at mobile widths.
* Pagination pages (p=2+) and map view: no module. The article block is untouched — module coexists below it where articles exist.
* No new caching, no new service data methods if avoidable — prefer computing highlight ratios in a small helper (view model builder or controller helper) from the existing VMs. If a helper class is needed, put it in the Web project, not Infrastructure.
* All percentages 1 decimal; counts comma-formatted; every percentage labels its base; "updated weekly" if freshness copy is used.
* No changes to sitemaps, article generation, or the admin Renew City Content feature in this task.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The module rendered for three very different cities: LOS ANGELES CA (huge), FRESNO CA (distinctive cargo — expect a produce highlight), and a ~60-company city (sparse — expect omitted sections), plus one <50 city with NO module.
* A page-2 URL of a module city proving the module is absent there.
* The exact highlight sentences produced for Fresno and Birmingham AL — they must not be identical in structure.
* Cold + warm City page load time for a module city (the module must add ~0 on warm).
* Exact list of files changed.
