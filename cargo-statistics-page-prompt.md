Build the Cargo Statistics page at `/statistics/cargo` — cargo only, NO equipment content anywhere. Also rename the "Cargo and Equipment" level-2 card on the Statistics landing page to "Cargo Statistics" and make it clickable. Plus one small consistency rider on the Fleet & Operations service (below).

First, study: `Views/Home/FleetOperations.cshtml` (newest page — clone its conventions), the city page's cargo SQL in `HomepageService.GetCityCompaniesData()` (the 30 `CargoTransported*` columns, their `='X'` flag convention, and the exact display-label list "General Freight", "Household Goods", "Metal: Sheets & Coils", etc. — reuse those labels verbatim), and the level-2 card markup in `Views/Home/Statistics.cshtml`.

Data scope: active US companies (`Status='A'` AND physical state in US state codes — do NOT repeat the Fleet page's mistake of skipping the US filter). Fleet size = `TotalNumberOfPowerUnits`, 1–50,000 range.

**EF materialization rule (a cast error already bit us today): any DTO property declared `long` MUST have its SQL column wrapped `CAST(... AS BIGINT)` — EF `SqlQuery` will not widen Int32 to Int64. Counts that fit int can stay int; power-unit sums are `long` + BIGINT.**

Query plan — ONE single-pass raw-SQL aggregate over active US companies computing:
* Per cargo type (30 types): company count; power-unit sum (1–50,000 only); count of companies in that cargo group reporting power units (> 0) — for average fleet size per cargo.
* Specialization: per-row selected-type count via a summed CASE expression over the 30 flags, bucketed: exactly 1, 2–3, 4–6, 7+; plus count of companies with zero cargo flags.
* Totals: active company count, companies with ≥1 cargo type, sum of all selections (for average types per company), max `DateLastChanged`.
One clustered scan, cached 30 days. No other heavy queries.

Page sections, top to bottom:
1. Breadcrumb `Home > Statistics > Cargo Statistics`; hero H1 "Cargo Statistics", intro ("Explore what U.S. trucking companies haul — cargo categories, specialization, and how fleet size varies by cargo type."), Data Updated / Updated weekly box, image per page conventions.
2. Four stat cards (no trend badges):
   * General Freight Carriers — count, sub "% of all active companies".
   * Most Common Specialized Cargo — name + count of the top type excluding General Freight.
   * Average Cargo Types per Company — 1 decimal, sub "among companies reporting cargo types".
   * Data as of — max DateLastChanged, sub "Updated weekly".
3. "Cargo Types by Company Count" — top 10 as horizontal bars (count + % of all active companies), "View all 30 cargo types" expander revealing the full ranked table (Cargo Type, Companies, % of Active Companies, bar fill). REQUIRED footnote: "Companies may select multiple cargo types, so percentages sum to more than 100%." Do NOT render this as a donut — it is not parts-of-a-whole.
4. "Cargo Specialization" donut — buckets: 1 cargo type, 2–3, 4–6, 7+; center = companies reporting at least one cargo type. Footnote states the count and % of active companies with no cargo types reported (excluded from the donut).
5. "Average Fleet Size by Cargo Type" — table over the top 10 cargo types by company count: Cargo Type, Companies, Average Fleet Size (power-unit sum ÷ companies in that cargo group reporting power units, 2 decimals). Footnote: average is per company reporting fleet size within each cargo group; outliers above 50,000 power units excluded.
6. Related Statistics card row: Statistics landing, Active Trucking Companies, Active Freight Brokers, New FMCSA Registrations, Fleet & Operations.
7. Footer note: "All data is based on active FMCSA (USDOT) records as of [date] and is updated weekly." + Source: FMCSA. No equipment mentions anywhere.

Routing/VM/service: route `StatisticsCargo`, url `statistics/cargo`, with the other statistics routes; `HomeController.Cargo()`; `Views/Home/CargoStatistics.cshtml`; `StatisticsCargoVM` in `BusinessVM.cs`; `GetCargoData()` in `#region Statistics`.

Caching (required): key `CargoData_v1`, 30-day absolute, `NoSlidingExpiration`; prefix `"CargoData_"` added to `InvalidateStatisticsCache()`; add `GetCargoData()` to the pre-warm calls in `RefreshStatisticsCache()`; update the admin success message + ManageStatistics.cshtml description.

SEO (required): title `"Cargo Statistics — U.S. Trucking | Truck Carrier Hub"`; dynamic meta description ≤160 chars C#-validated, e.g. `"U.S. trucking cargo statistics — what [N] active companies haul: cargo types, specialization, and fleet size by cargo, from FMCSA data."`; canonical `https://truckcarrierhub.com/statistics/cargo`; JSON-LD WebPage + BreadcrumbList; cross-links: add a Cargo Statistics card to the Related Statistics rows on Active Companies, Active Brokers, New Registrations, and Fleet & Operations pages.

Landing page card (`Views/Home/Statistics.cshtml`):
* Rename "Cargo and Equipment" → "Cargo Statistics"; keep the `sic-orange` icon circle (swap the icon to `fa-cubes` or similar if `fa-cube` reads as equipment).
* Bullets become: Cargo Types / Specialization / Fleet Size by Cargo / General Freight.
* Card becomes clickable → `/statistics/cargo` via the existing optional-URL pattern (same structure as the Fleet & Operations card).

Rider — Fleet & Operations base-consistency fix: `GetFleetOperationsData()`'s three raw-SQL queries filter only `Status='A'` (no US filter), while its Total Active Companies count IS US-filtered — mixed bases on one page. Add the US-state filter to all three queries (join or IN-list against US state codes, matching how other methods do it) and bump the cache key `FleetOperationsData_v1` → `_v2`. Report how much the headline numbers move.

Implementation rules:
* All copy "updated weekly"; comma-formatted numbers; percentages 1 decimal; every percentage labels its base population.
* Site palette/components, Bootstrap 3 grid, synchronous style, no new packages.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The page rendering all sections with real data; the full 30-type table expanded; rendered `<head>` (title, meta + char count, canonical, JSON-LD).
* Verification SQL + results for two spot checks: General Freight count and one mid-table cargo type, matching the page.
* Cache key + prefix + pre-warm list updated; total Refresh Statistics Cache button time (now warms six datasets — must stay well under 60s).
* The renamed clickable landing card and the four Related Statistics cross-links.
* Fleet & Operations numbers before/after the US-filter rider.
* Exact list of files changed.
