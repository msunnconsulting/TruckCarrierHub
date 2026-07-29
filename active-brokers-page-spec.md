# Active Freight Brokers page — implementation spec

Implementation prompt for `/statistics/active-brokers`. Based on the approved mockup
(Active Freight Brokers) and the existing Active Trucking Companies page. Where the two
disagree, this spec wins — see "Deviations from mockup" at the end.

## Route & files

- URL: `/statistics/active-brokers`
- RouteConfig.cs: add route `StatisticsActiveBrokers`, url `statistics/active-brokers`,
  `Home.ActiveBrokers` — next to the existing `StatisticsActiveCompanies` route (order matters,
  before generic routes).
- Controller: `HomeController.ActiveBrokers()` — same shape as `ActiveCompanies()`.
- View: `Views/Home/ActiveBrokers.cshtml`.
- ViewModel: `StatisticsActiveBrokersVM` in `TruckCarrierHub.ViewModels\Admin\BusinessVM.cs`
  (statistics VMs live there, alongside `StatisticsActiveCompaniesVM`). Reuse `StateStatVM`,
  `CityStatVM`, `MonthStatVM` etc. where they fit.
- Service: `GetActiveBrokersData()` on `IHomepageService` / `HomepageService.cs`, in the
  `#region Statistics`, following `GetActiveCompaniesData()` patterns exactly.

## Style — match the Active Trucking Companies page, not the mockup's palette

The mockup uses a green theme; **ignore that**. Style, colors, fonts, images, map, donuts,
tables, and cards must match the existing Active Trucking Companies page
(`Views/Home/ActiveCompanies.cshtml`):

- Shared stylesheet `~/Content/statistics-page.css`; copy the `ac-*` class conventions
  (rename prefix `ab-*` for page-local styles to avoid collisions).
- Hero: same `stats-hero` layout, `~/Content/images/truck.png` image, same typography.
- Choropleth: same implementation — `topojson-client.min.js` + `~/Content/us-states-110m.json`,
  JS fills an empty `<svg id="abMapSvg" viewBox="0 0 960 600">`, hover tooltips, same blue
  intensity scale as the carriers map.
- Donuts: server-rendered SVG arcs with center label, same sizes/legend layout.
- Line chart: same SVG trends-chart implementation as "New FMCSA Registrations (Last 24 Months)".
- Ranked tables: `ac-ranked-table` pattern with rank number, count, % and bar fill.
- Design system rules from `Content/modern-theme.css` apply (Archivo headings, IBM Plex Sans
  body, `.data-tag` IBM Plex Mono for USDOT/MC numbers).

## Data definitions

**Broker** = `Status = 'A'` AND `EntityType` contains code `B` AND physical address state is a
US state (`States.CountryCode = 'US'`), consistent with other statistics pages (Canada excluded).
`EntityType` is a semicolon-separated code list: `C` Carrier, `B` Broker, `S` Shipper,
`F` Freight Forwarder, `I` Intermodal Equipment Provider, `T` Cargo Tank. In SQL/EF use
`EntityType.Contains("B")` (matches the existing `vm.ActiveBrokers` query in
`GetStatisticsData()`); for the combination donut, split on `;` in memory like the city-page
authority-types code does (HomepageService ~line 4292).

`DateAdded` is a YYYYMMDD `int` — use the established `CONVERT`/integer-arithmetic patterns,
never string dates.

## Page sections (top to bottom)

1. **Breadcrumb + hero** — Home > Statistics > Active Freight Brokers. H1 "Active Freight
   Brokers", subtitle "U.S. Statistics Overview", short intro paragraph, truck image.

2. **Stat cards (5)**
   - Active Freight Brokers — total count. No "vs last year" badge (no historical snapshots).
   - New FMCSA Registrations (Last 24 Months) — count + "Avg. N per month".
   - Broker Only — % of brokers whose EntityType contains B but not C (count as sublabel).
   - Broker + Carrier — % whose EntityType contains both B and C (count as sublabel).
   - Data as of — max `DateLastChanged` (YYYYMMDD → formatted). Sublabel **"Updated monthly"**
     — never "weekly", site-wide copy decision.

3. **Top States by Active Freight Brokers** — choropleth (left) + Top 10 table with
   % of U.S. total and bar fills (right). "View all 50 states" expander, same behavior as the
   carriers page.

4. **Three-column row**
   - Top Cities by Active Freight Brokers — top 10 table, "View all cities" link if the
     carriers page has an equivalent; otherwise omit the link.
   - **Broker Entity Types** donut (replaces mockup's "Broker Authority Types") — segments:
     Broker only, Broker + Carrier, Broker + Freight Forwarder, Broker + Carrier + FF, Other
     combinations. Center: total broker count. Note under the donut: based on FMCSA census
     entity types.
   - New Broker Registrations (Last 24 Months) — monthly line chart from `DateAdded`, brokers
     only, with Total / Average per month / Best month / Lowest month summary row.

5. **Three-column row**
   - Carrier-to-Broker Ratio by State — top 10 states **by broker count**, columns: State,
     Active Carriers, Active Brokers, Ratio ("N.N : 1", carriers ÷ brokers). Carrier =
     `EntityType.Contains("C")`, same US/active filters.
   - Broker Age Distribution — donut from `DateAdded`: 0–2, 3–5, 6–10, 11–20, 20+ years.
     "Average Age: N.N years" below. Zero buckets: omit segment, keep legend row with "—"
     (no "0%" phrasing).
   - **Longest-Registered Freight Brokers** (replaces mockup's "Top 10 by MC Number" — census
     has no broker size metric) — top 10 by earliest `DateAdded`, columns: Broker Name (link
     to company page), MC Number (`IccDocketNumber1Prefix` + `IccDocketNumberFirst`, `.data-tag`
     style), Headquarters (city, state), Since (year). Filter: non-empty LegalName and MC number.

6. **Related Statistics** — same card row as other stats pages: Active Trucking Companies,
   Statistics landing, and the planned sub-pages (only link pages that exist).

7. **Footer note** — "All data is based on active FMCSA (USDOT) records as of {Data as of date}
   and is updated monthly. Source: FMCSA."

## Caching — required, this is a heavy page

Same regime as the other statistics pages:

- Cache the whole VM in `HttpRuntime.Cache`, key **`ActiveBrokersData_v1`**, 30-day absolute
  expiration, `NoSlidingExpiration`.
- **Add prefix `"ActiveBrokersData_"` to the prefixes array in
  `HomepageService.InvalidateStatisticsCache()`** — otherwise the admin Refresh button won't
  clear it.
- Add `GetActiveBrokersData()` to the pre-warm calls in
  `BusinessController.RefreshStatisticsCache()` and extend the admin success message to mention
  the brokers page. Update the description text on ManageStatistics.cshtml accordingly.
- Performance: `EntityType` is not covered by the existing `IX_Stats_*` indexes.
  `Contains("B")` compiles to `LIKE '%B%'` (non-sargable). Test cold-load time; if slow, add a
  covering index, e.g. `IX_Stats_Status_State_EntityType` on `(Status, PhysicalAddressStateCode)
  INCLUDE (EntityType, PhysicalAddressCity, DateAdded, DateLastChanged, IccDocketNumber1Prefix,
  IccDocketNumberFirst, LegalName)` — verify with the actual query plan before adding (SSDT
  script in TruckCarrierHub.Database, plus run manually on the live DB).

## Meta tags / SEO — required

Same pattern as `ActiveCompanies.cshtml` (`@section AdditionalMeta`):

- `<title>`: "Active Freight Brokers Statistics — U.S. | Truck Carrier Hub" (em dash via
  `ConvertFromUtf32(0x2014)`).
- Meta description: dynamic, plain text (no HTML), ≤160 chars, e.g. "U.S. freight broker
  statistics — {N:N0} active brokers across 50 states. Top states, top cities, registration
  trends, entity types, and FMCSA data." Truncate at 157 + "..." if over.
- Canonical: `https://truckcarrierhub.com/statistics/active-brokers`.
- JSON-LD: WebPage + BreadcrumbList (Home → Statistics → Active Freight Brokers), Newtonsoft
  JObject pattern from ActiveCompanies.cshtml.
- Cross-linking: add this page to the Statistics landing page's "Explore Key Statistics" grid
  (there's likely a placeholder card already) and to the Related Statistics row on the Active
  Trucking Companies page.

## Deviations from mockup (approved)

| Mockup | Build instead | Why |
|---|---|---|
| Property / Non-Property Brokers cards | Broker Only / Broker + Carrier cards | Broker authority sub-types come from FMCSA licensing data we don't import; census has EntityType only |
| Broker Authority Types donut | Broker Entity Types donut (EntityType combinations) | Same |
| "↑ 4.2% vs last year" badge | Omit | No historical snapshots kept |
| Top 10 Freight Brokers by MC Number | Longest-Registered Freight Brokers (earliest DateAdded) | No broker size metric in census; registration age is the only honest ranking |
| "Weekly updated" / "updated weekly" | "Updated monthly" | Data syncs monthly; site-wide copy decision |
| Green color theme | Site design system / carriers-page palette | Consistency with existing statistics pages |
