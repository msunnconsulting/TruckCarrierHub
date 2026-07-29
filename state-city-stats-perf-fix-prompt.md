The State (`/statistics/state/{code}`) and City (`/statistics/city/{code}/{city}`) statistics pages are very slow on first load (cold cache). The 30-day caching works — the problem is cold-load cost: `GetStateCompaniesData()` runs ~18 separate queries and `GetCityCompaniesData()` ~26, and several filter or aggregate columns that no index covers, so they scan the wide clustered index of `TransportCompany` repeatedly. Fix with one covering index change plus a query-consolidation refactor of both methods. Do not touch `GetStatisticsData()`, `GetActiveCompaniesData()`, or `GetActiveBrokersData()`.

First, read both methods end to end in `HomepageService.cs` (`#region Statistics`) and script the current definition of `IX_Stats_Status_State_City` from the live database so you know its exact keys/includes before replacing it.

Change 1 — covering index:

* Replace `IX_Stats_Status_State_City` with a version that keeps keys `(Status, PhysicalAddressStateCode, PhysicalAddressCity)` and adds `INCLUDE (TotalNumberOfPowerUnits, DateAdded, DateLastChanged, EntityType, IccDocketNumber1Prefix, NNDriversGrandTotalInterstateAndIntrastate)`. This single index then covers every non-cargo query on both pages (state queries seek on the first two key columns).
* Do NOT include the 30 `CargoTransported*` columns — the cargo query stays a residual key-lookup cost, acceptable because it is one query per city.
* Provide it as: updated SSDT definition in `TruckCarrierHub.Database/dbo/Tables/TransportCompany.sql` AND a standalone `DROP_EXISTING`/`CREATE` script to run manually on the live database. Mind the 10GB SQL Express cap — report the index size after creation (`sp_spaceused` or `sys.dm_db_partition_stats`).

Change 2 — refactor `GetStateCompaniesData()` to ~5 queries:

* One raw-SQL aggregate over `WHERE Status='A' AND PhysicalAddressStateCode=@p0` computing in a single pass: total active companies, active MC numbers (`IccDocketNumber1Prefix='MC'`), power-units sum, drivers sum, the six fleet-size buckets (`CASE WHEN` sums: =1, 2–5, 6–20, 21–100, >100, >0), and max `DateLastChanged` (>19000101).
* Keep: the US total count (one covered count), the county grouping — but compute Top 10 counties and the all-counties map from ONE grouped query in memory instead of the current two identical group-bys — the top-cities grouping, and the small `McmisCountyCodes` lookup.
* Results must be numerically identical to today — same rounding, same bucket boundaries, same YYYYMMDD integer arithmetic.

Change 3 — refactor `GetCityCompaniesData()` to ~6 queries:

* ONE query pulls all active rows for the city with just `(TotalNumberOfPowerUnits, DateAdded, DateLastChanged, EntityType)` into memory (typically a few hundred to ~20k rows). From that list compute in C#: total count, city reporting count, owner-operator count and %, median fleet size, the five size buckets, new-registrations-last-24-months, the monthly registrations chart for the selected range (including the "all" range minimum date — no extra query), the five age buckets, average age, entity/authority-type counts, and max DateLastChanged.
* One raw-SQL aggregate for the state-level numbers (state total, state reporting, state owner-operator count) in a single pass — same technique as Change 2.
* Keep as separate queries: the 30-column cargo SUM (already one query), Top 10 companies by fleet size (make it select only the columns the row needs — it currently materializes entire 163-column entities via `ToList()` before projecting), and the county-name lookup.
* The YoY percent, best/lowest month, and range-label logic stay exactly as they are — they already operate on the in-memory monthly list.
* Results must be numerically identical to today for every range value ("24m", "4y", "8y", "all").

Implementation rules:

* Cache keys, 30-day expirations, `InvalidateStatisticsCache` prefixes, lazy-load behavior, and method signatures all stay unchanged.
* Keep the existing style: raw SQL via `db.Database.SqlQuery<T>` with `@p0/@p1` parameters, synchronous, no new packages.
* Remove the silent `catch { }` blocks around the cargo and county-name queries — let errors surface; wrap nothing new in try/catch.
* Null-safety: `TotalNumberOfPowerUnits`, `DateAdded`, `DateLastChanged`, `EntityType` are all nullable — the in-memory computations must treat null exactly as the current per-query filters do (e.g. `DateAdded > 0` guards, `> 19000101` for DateLastChanged).

After implementing, build with zero errors, then measure and show me:

* Cold-load time before and after for: one large state (TX), one small state (VT), one large city (HOUSTON, TX), one small city (pick any with <50 companies) — clear cache between runs.
* Second-load times confirming the cache still hits.
* A side-by-side of key numbers before/after for TX and HOUSTON (total, MC numbers, drivers, median fleet, owner-op %, age buckets, top county, best month) proving identical output.
* The new index definition and its size on disk.
