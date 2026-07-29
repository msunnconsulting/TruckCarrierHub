Build the Fleet & Operations statistics page at `/statistics/fleet-operations`, v1 = the mockup's Overview view ONLY — no tabs, no "Explore This Section" sidebar, no Filters panel, no CSV download, no methodology link, no "vs last month" badges. Also make the "Fleet and Operations" level-2 card on the Statistics landing page clickable to it.

First, study `Views/Home/ActiveBrokers.cshtml` (page pattern: hero, stat cards, donuts, ranked lists, `@section AdditionalMeta`), `HomepageService.GetActiveBrokersData()` and the refactored `GetStateCompaniesData()` (the single-pass raw-SQL aggregate technique — reuse it), and the level-2 card markup in `Views/Home/Statistics.cshtml` (optional-URL pattern).

Data scope: active US companies (`Status='A'`, US state codes). Equipment data is LIMITED to four columns — `NNEquipmentUnitsOwnedTruck`, `NNEquipmentUnitsOwnedTractor`, `NNEquipmentUnitsTermLeasedTruck`, `NNEquipmentUnitsTermLeasedTractor`. Do not use trailer, motorcoach, bus, van, limo, or trip-leased columns anywhere. Fleet size = `TotalNumberOfPowerUnits` (exclude > 50,000 outliers, established convention). "Reporting" = `TotalNumberOfPowerUnits > 0` — every percentage on the page must state its base ("of all active companies" vs "of companies reporting fleet size"), following the labeling convention just established on the Active Companies page.

Query plan (performance matters — SQL Express):
* ONE raw-SQL single-pass aggregate over `Status='A'` US companies computing: total companies; total power units; the four equipment-column sums; owner-operator count (`TotalNumberOfPowerUnits = 1`); reporting count (> 0); the five fleet-size bucket counts AND per-bucket power-unit sums; interstate carrier count and their power-unit sum (`OperationCarrierInterstate` flag — check the actual stored values before writing the CASE, don't assume 'X'/'Y'); hazmat carrier count and power-unit sum (`HazmatIndicator`, same caveat); max `DateLastChanged`. One clustered scan, acceptable cold, cached 30 days.
* Grouped-by-state queries (covered by `IX_Stats_Status_State`): power units by state (top 10), average fleet size by state (top 10, reporting companies only, minimum 100 reporting companies per state so tiny states don't dominate).

Page sections, top to bottom:
1. Breadcrumb `Home > Statistics > Fleet & Operations`. Hero: H1 "Fleet & Operations", intro ("Analyze how trucking companies operate across the U.S. — fleet composition, owner-operator statistics, equipment ownership, and operating patterns."), the "Data Updated / Updated weekly" info box, truck image — same hero conventions as the other statistics pages.
2. Four stat cards (NO trend badges): Total Active Companies (sub: "100% of active motor carriers"); Total Power Units; Average Fleet Size (power units ÷ reporting companies, 2 decimals; sub: "per company reporting fleet size"); Total Trucks & Tractors (sum of the four equipment columns; sub: "owned and term-leased").
3. Two-column row: "Fleet Size Distribution" donut (five buckets, 1 = Owner-Operator labeled as in the mockup) + "Top 10 States by Total Power Units" horizontal bar list with counts and "View all states" expander.
4. Three highlight cards: Owner-Operator Companies (count, "% of companies reporting fleet size", their total power units); Interstate Carriers (count, "% of all active companies", power units); HazMat Carriers (count, "% of all active companies", power units).
5. Two-column row: "Trucks & Tractors by Ownership" donut — four segments: Owned Trucks, Owned Tractors, Leased Trucks, Leased Tractors, with counts and %, center = total; footnote: "FMCSA census reports owned and term-leased trucks and tractors; other equipment categories are not included." + "Interstate vs Intrastate" donut by power units (Interstate / Intrastate only), footnote naming the FMCSA operation flags as the source.
6. "Average Fleet Size by State (Top 10)" bar list (reporting companies only; note the minimum-companies threshold in a footnote).
7. "Fleet Size Distribution Table": rows = the five buckets + Total; columns = Companies, % of Companies, Power Units, % of Power Units. NO trailer columns. Total row labeled "Total reporting fleet size"; footnote gives the count and % of active companies excluded for not reporting power units (same wording pattern as the Active Companies page).
8. Related Statistics card row: Statistics landing, Active Trucking Companies, Active Freight Brokers, New FMCSA Registrations.
9. Footer note: "All data is based on active FMCSA (USDOT) records as of [date] and is updated weekly." + Source: FMCSA.

Routing/VM/service: route `StatisticsFleetOperations`, url `statistics/fleet-operations`, with the other statistics routes; `HomeController.FleetOperations()`; `Views/Home/FleetOperations.cshtml`; `StatisticsFleetOperationsVM` in `BusinessVM.cs`; `GetFleetOperationsData()` in `#region Statistics`.

Caching (required): key `FleetOperationsData_v1`, 30-day absolute, `NoSlidingExpiration`; add prefix `"FleetOperationsData_"` to `InvalidateStatisticsCache()`; add to the pre-warm calls in `RefreshStatisticsCache()` and update the admin success message + ManageStatistics.cshtml description.

SEO (required): title `"Fleet & Operations Statistics — U.S. Trucking | Truck Carrier Hub"` (em dash convention); dynamic meta description ≤160 chars C#-validated, e.g. `"U.S. trucking fleet statistics — [PU] power units across [N] active companies. Fleet sizes, owner-operators, equipment ownership, interstate operations."`; canonical `https://truckcarrierhub.com/statistics/fleet-operations`; JSON-LD WebPage + BreadcrumbList; cross-links: set the URL on the landing page's "Fleet and Operations" level-2 card (it becomes clickable via the existing optional-URL pattern — keep its bullet list), and add a Fleet & Operations card to the Related Statistics rows on the Active Companies, Active Brokers, and New Registrations pages.

Implementation rules:
* All copy "updated weekly"; numbers comma-formatted from the VM; percentages 1 decimal; every percentage labels its base population.
* Site palette and existing components — not the mockup's exact colors; Bootstrap 3 grid; no new packages; synchronous style.
* Do not build tabs, filters, sub-pages, CSV export, or methodology content — v1 is this single page.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The page rendering every section with real data; rendered `<head>` (title, meta + char count, canonical, JSON-LD).
* The actual distinct values found in `OperationCarrierInterstate` and `HazmatIndicator` and the CASE logic used.
* Cache key in code, prefix registered, pre-warm list now including this page, admin message updated.
* Cold and warm load times; total Refresh Statistics Cache button time (must stay well under 60s).
* The landing card now clickable + the three Related Statistics cross-links.
* Exact list of files changed.
