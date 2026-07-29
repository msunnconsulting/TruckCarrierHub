Build a new public statistics page: Active Freight Brokers at `/statistics/active-brokers`. A full spec exists at `active-brokers-page-spec.md` in the solution root — this prompt is the authoritative summary; consult the spec if anything here is ambiguous.

First, study these four existing pieces and follow their patterns exactly rather than inventing new approaches:

* `TruckCarrierHub.Web/Views/Home/ActiveCompanies.cshtml` — page structure, hero, stat cards, choropleth, donuts, line chart, ranked tables, `@section AdditionalMeta` SEO block
* `HomeController.ActiveCompanies()` — controller shape
* `HomepageService.GetActiveCompaniesData()` (in `#region Statistics`) — query patterns, caching pattern
* `StatisticsActiveCompaniesVM` in `TruckCarrierHub.ViewModels/Admin/BusinessVM.cs` — ViewModel shape; reuse `StateStatVM`, `CityStatVM`, `MonthStatVM` where they fit

**Important: the page's visual style, colors, fonts, hero image, map, and chart implementations must match the existing Active Trucking Companies page.** The mockup for this page used a green theme — ignore the green, use the same palette and components as ActiveCompanies.cshtml. Use shared `~/Content/statistics-page.css`; prefix page-local CSS classes `ab-` instead of `ac-`. Hero uses `~/Content/images/truck.png`. Choropleth uses `topojson-client.min.js` + `~/Content/us-states-110m.json` filling an empty `<svg id="abMapSvg" viewBox="0 0 960 600">`, same blue intensity scale and hover tooltips as the carriers map. Donuts are server-rendered SVG arcs. The registrations chart is the same SVG trends-chart implementation.

Files to create/modify:

* `RouteConfig.cs` — add route `StatisticsActiveBrokers`, url `statistics/active-brokers`, defaults `Home.ActiveBrokers`, placed next to the existing `StatisticsActiveCompanies` route (before generic routes)
* `HomeController` — new `ActiveBrokers()` action in `#region Statistics`
* `Views/Home/ActiveBrokers.cshtml` — new view
* `BusinessVM.cs` — new `StatisticsActiveBrokersVM`
* `IHomepageService.cs` + `HomepageService.cs` — new `GetActiveBrokersData()`
* `HomepageService.InvalidateStatisticsCache()` — see caching below
* `BusinessController.RefreshStatisticsCache()` + `Areas/Admin/Views/Statistics/ManageStatistics.cshtml` — see caching below
* Statistics landing page + ActiveCompanies.cshtml — see cross-links below

Data definitions:

* Broker = `Status == "A"` AND `EntityType.Contains("B")` AND physical address state is a US state (`States.CountryCode == "US"`). Canada excluded, consistent with the other statistics pages. This matches the existing `vm.ActiveBrokers` query in `GetStatisticsData()`.
* Carrier (for the ratio table) = same filters but `EntityType.Contains("C")`.
* `EntityType` is a semicolon-separated code list: C Carrier, B Broker, S Shipper, F Freight Forwarder, I Intermodal Equipment Provider, T Cargo Tank. For the entity-types donut, split on `;` in memory — the city statistics page already does this (authority-types code in `HomepageService`, ~line 4292).
* `DateAdded` and `DateLastChanged` are YYYYMMDD integers — use the established integer-arithmetic patterns, never string dates.
* MC Number = `IccDocketNumber1Prefix` + `IccDocketNumberFirst`.

Page sections, top to bottom:

1. Breadcrumb `Home > Statistics > Active Freight Brokers`, then hero: H1 "Active Freight Brokers", subtitle "U.S. Statistics Overview", short intro paragraph, truck image — same `stats-hero` layout as ActiveCompanies.
2. Five stat cards:
   * "Active Freight Brokers" — total count, sublabel "Across the U.S.". No year-over-year badge (we keep no historical snapshots — do not fabricate one).
   * "New FMCSA Registrations (Last 24 Months)" — count, sublabel "Avg. [N] per month".
   * "Broker Only" — % of brokers whose EntityType contains B but not C, count as sublabel.
   * "Broker + Carrier" — % whose EntityType contains both B and C, count as sublabel.
   * "Data as of" — max `DateLastChanged` formatted "MMMM d, yyyy", sublabel "Updated monthly". Never the word "weekly" anywhere on this page.
3. "Top States by Active Freight Brokers" — choropleth left, Top 10 table right (State, Active Freight Brokers, % of U.S. Total, bar fill), "View all 50 states" expander — same behavior as the carriers page.
4. Three-column row:
   * "Top Cities by Active Freight Brokers" — top 10 table (City, State, Active Brokers).
   * "Broker Entity Types" donut — segments: Broker only; Broker + Carrier; Broker + Freight Forwarder; Broker + Carrier + FF; Other combinations. Center label: total broker count. Footnote: "Based on FMCSA census entity types."
   * "New Broker Registrations (Last 24 Months)" — monthly line chart from `DateAdded`, brokers only, with Total / Average per month / Best month / Lowest month summary row.
5. Three-column row:
   * "Carrier-to-Broker Ratio by State" — top 10 states by broker count. Columns: State, Active Carriers, Active Brokers, Ratio formatted "N.N : 1" (carriers ÷ brokers).
   * "Broker Age Distribution" — donut from `DateAdded`, buckets 0–2, 3–5, 6–10, 11–20, 20+ years; "Average Age: N.N years" below. Empty buckets: omit the segment, keep the legend row with "—", never "0%".
   * "Longest-Registered Freight Brokers" — top 10 by earliest `DateAdded`, filtered to non-empty LegalName and MC number. Columns: Broker Name (linked to its company page), MC Number (rendered with `.data-tag`), Headquarters ("City, ST"), Since (year from DateAdded). This deliberately replaces the mockup's "Top 10 by MC Number" — the census has no broker size metric, so registration age is the ranking.
6. "Related Statistics" card row — link only to pages that exist: Active Trucking Companies and the Statistics landing page.
7. Footer note: "All data is based on active FMCSA (USDOT) records as of [Data as of date] and is updated monthly." with "Source: FMCSA".

Caching (required):

* Cache the whole ViewModel in `HttpRuntime.Cache`, key `ActiveBrokersData_v1`, 30-day absolute expiration, `NoSlidingExpiration` — identical pattern to `GetActiveCompaniesData()`.
* Add prefix `"ActiveBrokersData_"` to the prefixes array in `HomepageService.InvalidateStatisticsCache()`. Without this the admin Refresh Statistics Cache button will not clear the page.
* Add `_homepageService.GetActiveBrokersData();` to the pre-warm calls in `BusinessController.RefreshStatisticsCache()`, and update the success message and the description text in `ManageStatistics.cshtml` to mention the brokers page.
* `EntityType.Contains("B")` compiles to a non-sargable `LIKE '%B%'` and is not covered by the existing `IX_Stats_*` indexes. Time the cold load. If it is slow, propose a covering index on `(Status, PhysicalAddressStateCode) INCLUDE (EntityType, ...)` as an SSDT script in TruckCarrierHub.Database — but show me the timing first, do not add it preemptively.

SEO metadata (required — same `@section AdditionalMeta` pattern as ActiveCompanies.cshtml):

* Title: `"Active Freight Brokers Statistics — U.S. | Truck Carrier Hub"` (em dash via `ConvertFromUtf32(0x2014)`, same as existing pages)
* Meta description (dynamic, `[TotalActiveBrokers]` comma-formatted): `"U.S. freight broker statistics and analytics — [TotalActiveBrokers] active freight brokers. Top states, top cities, registration trends, entity types, and FMCSA data."` Hard limit 160 characters — validate in C# before rendering; if over, truncate at 157 + "...". Plain text only, no HTML.
* Canonical: `<link rel="canonical" href="https://truckcarrierhub.com/statistics/active-brokers" />`
* JSON-LD (serialize with Newtonsoft JObject like ActiveCompanies.cshtml, not string concatenation):

```json
{
  "@context": "https://schema.org",
  "@type": "WebPage",
  "name": "Active Freight Brokers Statistics — U.S.",
  "description": "[same as meta description]",
  "url": "https://truckcarrierhub.com/statistics/active-brokers",
  "breadcrumb": {
    "@type": "BreadcrumbList",
    "itemListElement": [
      {"@type": "ListItem", "position": 1, "name": "Home", "item": "https://truckcarrierhub.com"},
      {"@type": "ListItem", "position": 2, "name": "Statistics", "item": "https://truckcarrierhub.com/statistics"},
      {"@type": "ListItem", "position": 3, "name": "Active Freight Brokers", "item": "https://truckcarrierhub.com/statistics/active-brokers"}
    ]
  }
}
```

* Cross-links: add this page to the Statistics landing page's "Explore Key Statistics" grid (replace the matching placeholder box if one exists) and to the Related Statistics row on the Active Trucking Companies page.

Implementation rules applying everywhere:

* Follow ActiveCompanies.cshtml patterns exactly — do not invent new chart, table, card, or metadata approaches
* All numbers computed from real data in the service layer — no hardcoded values; dynamic numbers comma-formatted
* All URLs use `https://truckcarrierhub.com`, no trailing slashes, state codes uppercase
* No exact-count language problems here (this is a statistics page, exact counts are fine), but all copy says "updated monthly" — never "weekly"
* Layout must render correctly inside the existing Bootstrap 3 grid; do not upgrade or work around it

After implementing, build the solution and confirm zero errors, then show me:

* The rendered HTML `<head>` for `/statistics/active-brokers` — title, meta description (with character count), canonical, JSON-LD
* The list of prefixes in `InvalidateStatisticsCache()` proving `ActiveBrokersData_` is registered
* The pre-warm call list in `RefreshStatisticsCache()`
* Cold-load time vs second-load time for the page, confirming the 30-day cache hit
* Confirmation the choropleth, both donuts, the line chart, and all four tables render with real data, and that the two cross-links are in place
