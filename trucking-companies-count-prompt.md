Redefine the "Active Trucking Companies" number across the statistics section and homepage band: trucking companies = active US companies MINUS pure brokers, where **pure broker = `EntityType = 'B'` exactly** (string equality, not LIKE). One number everywhere. The rule is recorded in CLAUDE.md — read the entity-classification section first. Directory listing counts (state/city directory pages) are explicitly OUT of scope — brokers are listed in the directory, those counts stay all-active.

Changes:

1. `GetStatisticsData()` (`HomepageService.cs`):
   * `TotalCompanies` becomes: count(Status='A', US states) − count(Status='A', US states, EntityType='B'). Compute via one extra cheap count or a CASE in an existing pass — do not add a table scan.
   * Add a VM field for the excluded pure-broker count (the subtrahend) — consumers below need it for disclosure lines.
   * **Bump cache key `StatisticsData_v5` → `_v6`** (semantics + shape change).
   * This automatically fixes: the statistics landing level-1 card, the homepage band card 1 (`HpTotalCompanies`), and the homepage `{NUS:N0}`… — NO: `{NUS:N0}`/`{NCA:N0}` in `GetHomeCountryCounts()` stay ALL-active (the intro sentence says "trucking companies and freight brokers" combined, which is the all-active population). Do not change that helper.
2. Active Companies page (`GetActiveCompaniesData()` + `ActiveCompanies.cshtml`):
   * The ENTIRE page adopts the trucking-companies base: add `AND NOT (EntityType = 'B')` (null-safe: `(EntityType IS NULL OR EntityType != 'B')`) to every query in the method — headline total, top states, top cities, choropleth, fleet buckets, cargo donut, monthly registrations trend, owner-operator split. The page is about trucking companies; brokers have their own page.
   * Headline card gets a sub-line: "Excludes [N] broker-only companies — see Active Freight Brokers" (linked), using the excluded count scoped the same way.
   * Meta description keeps working (it quotes the total — now the new one).
   * **Bump `ActiveCompaniesData_v4` → `_v5`.**
3. Fleet & Operations (`GetFleetOperationsData()`) and Cargo (`GetCargoData()`):
   * Their "Total Active Companies" stat cards and every "% of all active companies" denominator switch to the trucking-companies base: add the same null-safe exclusion to their aggregates. (Practical effect on fleet/cargo numbers is small — pure brokers report no power units and few cargo flags — but the bases must reconcile.)
   * **Bump `FleetOperationsData_` and `CargoData_` key versions.**
4. Homepage band (`Views/Home/Index.cshtml`):
   * Card 1 number comes from the updated `TotalCompanies` — verify the binding, no logic change needed.
   * Replace the footnote text with: "Companies holding both carrier and broker authority are counted in both figures." (the old brokers-are-included-in-companies note is no longer true).
5. Active Brokers page: NO changes to its counts (broker = contains B stays). Optionally verify its "Broker + Carrier" card already communicates the mirror side — it does; leave it.
6. New-registrations pages: NO changes — their carrier/broker split follows the contains-C / contains-B rule already implemented; the monthly totals remain all-registrations. Do not touch.

Implementation rules:
* Exclusion predicate everywhere: `(EntityType IS NULL OR EntityType != 'B')` in SQL, `tc.EntityType != "B"`-with-null-guard in LINQ — exact equality with 'B', never LIKE '%B%'.
* Every changed percentage must still label its base population; disclosure lines comma-formatted, only rendered when the excluded count > 0.
* Verify all bumped prefixes are still covered by `InvalidateStatisticsCache()` via StartsWith.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, clear the statistics cache, then show me:
* The pure-broker count (`EntityType='B'`, active, US) from SQL, and the reconciliation: old total − pure brokers = new total, shown on all four surfaces (homepage band, statistics landing, Active Companies headline, Fleet & Cargo total cards) — same number everywhere.
* The Active Companies headline sub-line rendered with the linked brokers reference.
* The homepage band with its new footnote; confirm the intro sentence's `{NUS:N0}` number is UNCHANGED (all-active by design).
* New cache key strings and prefix coverage.
* Exact list of files changed.
