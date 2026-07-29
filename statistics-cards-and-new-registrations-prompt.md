Two related changes on the Statistics section: (A) reshape the top stat-card row on the Statistics landing page (`/statistics`) from six cards to four bigger ones, first three fully clickable; (B) build the new sub-page `/statistics/new-registrations` from the approved mockup, which card 3 links to. The "Explore Key Statistics" section below the landing cards stays EXACTLY as it is — do not touch it.

First, study: `Views/Home/Statistics.cshtml` (stat-card row markup, `.stat-card-*` CSS, the dead `href="#"` chevrons on cards 3–6), `Views/Home/ActiveBrokers.cshtml` and `Views/Home/CityCompanies.cshtml` (component patterns to clone: hero, cards, donuts, line chart with range param, choropleth, ranked tables, `@section AdditionalMeta`), `HomepageService.GetActiveBrokersData()` (single-pull in-memory aggregation + caching pattern), and `StatisticsIndexVM` / `StatisticsActiveBrokersVM` in `BusinessVM.cs`.

## Part A — landing page stat-card row

* Keep card 1 (Active Trucking Companies) and card 2 (Active Freight Brokers) with their current numbers and text.
* Card 3 becomes **New FMCSA Registrations**: number = new registrations in the last 12 months (active US companies, `DateAdded` within the last 12 calendar months — YYYYMMDD integer pattern, covered by `IX_Stats_Status_DateAdded`); label "New FMCSA Registrations"; description "Track newly registered trucking companies and freight brokers."
* DELETE the "Cities Covered" and "Median Fleet Size" cards (markup only — leave VM fields in place, other consumers may exist).
* Keep the "FMCSA Data Updates" card as card 4, non-clickable — remove its dead `href="#"` chevron.
* Cards 1, 2, 3 become fully clickable block-level `<a>` (chevron stays as a visual cue inside), hrefs: `/statistics/active-companies`, `/statistics/active-brokers`, `/statistics/new-registrations`. No `href="#"` anywhere in the row afterward.
* Make the four cards larger to fill the row (adjust `.stat-card-col` flex basis from `1 1 140px`); must still wrap acceptably on small screens.
* VM/service: add the last-12-months count to `StatisticsIndexVM`, compute in `GetStatisticsData()`, and **bump cache key `StatisticsData_v4` → `StatisticsData_v5`** (VM shape changes; stale v4 must not hit the new view). The `StatisticsData_` prefix in `InvalidateStatisticsCache()` covers it — verify, don't assume.

## Part B — new page `/statistics/new-registrations` (per mockup)

Clone existing statistics-page conventions (shared `statistics-page.css`, `nr-` CSS prefix, server-rendered SVG donuts, the SVG line chart, the topojson choropleth). Route `StatisticsNewRegistrations`, url `statistics/new-registrations` (+ optional `range` query param), `HomeController.NewRegistrations(string range = "24m")`, `Views/Home/NewRegistrations.cshtml`, `StatisticsNewRegistrationsVM`, `GetNewRegistrationsData(range)` in `#region Statistics`.

Data scope and definitions:
* Population: active US companies (`Status='A'`, US state codes) — consistent with all statistics pages. (Inactive records have their addresses scrubbed, so historical registration counts by state/city are only possible for active companies; this is the accepted trade-off.)
* All date math on the YYYYMMDD `DateAdded` integer.
* Company-type classification (mutually exclusive, sums to total): **Motor Carrier** = EntityType contains C; **Freight Broker** = contains B but not C; **Other** = neither (only render "Other" rows/segments if nonzero). Split EntityType on `;` in memory like the brokers page.
* Follow the single-pull technique: ONE query fetches `(EntityType, PhysicalAddressStateCode, PhysicalAddressCity, DateAdded)` for the selected range window plus the previous same-length window (for the trend badges); compute everything below in memory. Separate queries only where noted.

Range selector: 12M / 24M / 36M / All toggle on the chart card (mockup top-right). Implement as `?range=` reloads like the City statistics page does — not client-side. Default `24m`. "All" derives its start from min DateAdded (like the city page). The stat cards, donut, tables, map, and calendar all follow the selected range where labeled "(Last 24 Months)" in the mockup — labels must reflect the active range.

Page sections, top to bottom (mirror the mockup layout):
1. Breadcrumb `Home > Statistics > New FMCSA Registrations`. Hero: left — H1 "New FMCSA Registrations", intro paragraph, "Data updated: [date] · Source: FMCSA (as of [date])" line; right — the four stat cards:
   * "New Registrations (Last N Months)" — total for range.
   * "New Motor Carriers (Last N Months)".
   * "New Freight Brokers (Last N Months)".
   * "Average per Month (Last N Months)" — 1 decimal or whole number, match mockup.
   * Each card gets the mockup's "+X% vs previous N months" badge: compare the range window against the preceding equal-length window (both from `DateAdded`, both active-only). Green for positive, red for negative, hide if previous window is 0. Note: because only currently-active companies are counted, older windows undercount slightly (companies that since went inactive) — accepted; do not add disclaimers to the UI.
2. Two-column row:
   * "New Registrations Over Time" — monthly line chart with THREE series: All Registrations, Motor Carriers, Freight Brokers (extend the existing single-series SVG chart pattern to three lines + legend, colors from the site palette). Footnote: "Shows monthly new registrations by FMCSA registration date."
   * "Registrations by Company Type" — donut (Motor Carrier / Freight Broker / Other-if-nonzero) with center total, plus the side table with Count and % columns. Footnote "Based on FMCSA registration data."
3. Two-column row: "Top 10 States by New Registrations" and "Top 10 Cities by New Registrations" — ranked tables with % of total and bar fills; "View all states" / "View all cities" expanders (same behavior as the other statistics pages).
4. Three-column row:
   * "Registration Age Distribution — By length of time since registration" — donut over ALL active US companies (not just the range cohort): buckets 0–6 months, 6–12 months, 1–2 years, 2–5 years, 5+ years, center = total active count, side legend with counts and %. (The mockup's center number is inconsistent with its own buckets — this definition is the correct one.) One extra grouped pass over `DateAdded` for all active US companies.
   * "New Registrations by State" — choropleth for the selected range, same topojson implementation, hover counts, Less→More legend.
   * "Monthly Registration Calendar" — table of the 6 most recent months: Month, All Registrations, Motor Carriers, Freight Brokers; "View full N-month calendar" expander revealing all months of the range. Derived from the same in-memory monthly data as the chart.
5. Footer note: "Statistics are based on active FMCSA (USDOT) records as of [date] and are updated weekly. Registration date reflects the date the company was added to FMCSA records." + "Source: FMCSA". (Deliberate correction from the mockup: `DateAdded` is the FMCSA census add date, NOT the operating-authority grant date — do not use the mockup's "operating authority" wording.)

Caching (required):
* Cache key `NewRegistrationsData_v1_{range}` (one entry per range variant), 30-day absolute expiration, `NoSlidingExpiration`.
* Add prefix `"NewRegistrationsData_"` to `InvalidateStatisticsCache()`.
* Pre-warm ONLY the default `24m` variant: add `GetNewRegistrationsData("24m")` to `BusinessController.RefreshStatisticsCache()`; other ranges cache lazily. Update the admin success message and ManageStatistics.cshtml description.

SEO (required — same `@section AdditionalMeta` pattern):
* Title: `"New FMCSA Registrations Statistics — U.S. | Truck Carrier Hub"` (em dash via `ConvertFromUtf32(0x2014)`).
* Meta description (dynamic, comma-formatted default-range total): `"New FMCSA registration statistics — [N] trucking companies and freight brokers registered in the last 24 months. Monthly trends, top states, top cities, and company types."` ≤160 chars validated in C#; truncate at 157 + "..." if over. Plain text only.
* Canonical: `https://truckcarrierhub.com/statistics/new-registrations` — WITHOUT the range parameter (all range variants canonicalize to the default page; title/meta do not vary by range).
* JSON-LD WebPage + BreadcrumbList (Home → Statistics → New FMCSA Registrations), Newtonsoft JObject pattern.
* Cross-links: add a "New FMCSA Registrations" card to the Related Statistics rows on BOTH the Active Trucking Companies and Active Freight Brokers pages. Do NOT touch the landing page's "Explore Key Statistics" section.

Implementation rules:
* All copy says "updated weekly" — never "monthly".
* All numbers from the ViewModel, comma-formatted; no hardcoded values; percentages 1 decimal.
* Match the mockup's layout but the site's existing palette/typography (modern-theme.css + statistics-page.css conventions), not the mockup's exact colors.
* URLs use `https://truckcarrierhub.com`, no trailing slashes.
* Synchronous EF/raw-SQL style; no new packages; must render in the Bootstrap 3 grid.
* Do not stage or commit line-ending-churned files; commit only files you actually edit.

After implementing, build with zero errors, then show me:
* Rendered landing 4-card row: three block `<a>` cards with correct hrefs, fourth non-clickable, zero `href="#"` in the row.
* The new page rendering all sections with real data at `range=24m`, plus a load of `?range=12m` proving the toggle works and labels update.
* Rendered `<head>`: title, meta description with character count, canonical (no range param), JSON-LD.
* Cache proof: `StatisticsData_v5` and `NewRegistrationsData_v1_24m` keys in code, both prefixes in `InvalidateStatisticsCache()`, pre-warm list now four pages (24m only for this page), admin message updated.
* Cold and warm load times for `/statistics` and `/statistics/new-registrations` (24m).
* The two Related Statistics cross-links on the carriers and brokers pages.
* Exact list of files changed.
