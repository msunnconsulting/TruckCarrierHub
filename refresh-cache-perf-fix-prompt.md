The admin "Refresh Statistics Cache" button now runs for over 2 minutes. Root cause: `HomepageService.GetActiveBrokersData()` (added for the new Active Brokers page, pre-warmed by `BusinessController.RefreshStatisticsCache()`) issues roughly 12 separate queries, nearly all filtered by `EntityType LIKE '%B%'`. No index covers `EntityType`, so each query scans the full clustered index of `TransportCompany` (163 columns, millions of rows) — on SQL Express with its 1GB buffer pool that means repeated disk scans. Fix it in one pass with three changes.

First, read `HomepageService.GetActiveBrokersData()` end to end (in `#region Statistics`) so you know every query it currently runs, and read one of the existing `IX_Stats_*` index definitions on the live database (they are NOT in the SSDT project — see change 3) so the new index follows the same naming and shape conventions.

Change 1 — covering index:

* Create `IX_Stats_Status_EntityType` on `dbo.TransportCompany`: key `(Status, PhysicalAddressStateCode)`, `INCLUDE (EntityType, PhysicalAddressCity, DateAdded, DateLastChanged)`.
* Keep the INCLUDE list exactly that — do not add LegalName or the MC columns; the one TOP-10 query that needs them retrieves 10 rows via key lookup, which is fine.
* Add it to the SSDT project (`TruckCarrierHub.Database/dbo/Tables/TransportCompany.sql`) AND give me a standalone `CREATE INDEX` script to run manually on the live database, since SSDT publish is not part of the normal workflow here.
* `LIKE '%B%'` stays non-sargable — that is expected. The win is that the scan runs over a narrow index that fits in memory instead of the multi-GB clustered index.

Change 2 — refactor `GetActiveBrokersData()` to ~3 queries:

* Replace the separate queries for: total broker count, new-in-24-months count, broker-only count, broker+carrier count, max DateLastChanged, top-10 states, all-states map counts, top-10 cities, entity-type strings, monthly registrations, and age distribution — with ONE query that pulls all active US brokers' `(EntityType, PhysicalAddressStateCode, PhysicalAddressCity, DateAdded, DateLastChanged)` into memory (roughly 50–60k rows, five small columns), then compute all of those numbers from that list in C#.
* Keep as separate queries only: the carrier counts for the Carrier-to-Broker ratio table, and the TOP-10 longest-registered brokers SQL.
* Fix a correctness bug while you are in there: the three raw-SQL queries (monthly registrations, age distribution, longest-registered brokers) filter only `Status='A' AND EntityType LIKE '%B%'` — they are missing the US-states filter that all the EF queries apply, so Canadian brokers currently leak into those three sections. After the refactor, monthly and age come from the in-memory list (which is US-filtered); add the US filter to the longest-registered SQL explicitly.
* The computed results must be identical to what the current code produces for US data — same bucket boundaries, same rounding (`Math.Round(..., 1)` / `(..., 2)` as now), same YYYYMMDD integer arithmetic. The only intentional output changes are the Canadian-leak fixes above.
* Remove the silent `catch { }` around the longest-registered query — let it throw; the admin refresh action already reports errors.

Change 3 — script the missing live-DB indexes into SSDT:

* The five statistics covering indexes created last session exist only on the live database and were never added to the SSDT project: `IX_Stats_Status_State`, `IX_Stats_Status_State_City`, `IX_Stats_Status_DateAdded`, `IX_Stats_Status_PowerUnits`, `IX_Stats_Status_State_County`. Ask me for their exact definitions (I can script them from SSMS) or generate them from the query patterns in `GetStatisticsData()` / `GetActiveCompaniesData()` / `GetStateCompaniesData()` and show me for confirmation before committing. The SSDT project must end up matching the live database.

Implementation rules:

* Do not change the cache key (`ActiveBrokersData_v1`), the 30-day expiration, the `InvalidateStatisticsCache` prefixes, or the pre-warm call list — only the internals of `GetActiveBrokersData()` and the indexes.
* Do not change `GetStatisticsData()` or `GetActiveCompaniesData()`.
* No new NuGet packages, no async rewrite — keep the synchronous pattern.
* EF6 embeds the `"A"` status literal as a constant; do not switch to parameterized status values, it would change plan reuse behavior.

After implementing, build the solution with zero errors, then measure and show me:

* Cold `GetActiveBrokersData()` time before the index, after the index, and after the refactor (clear cache between runs via the admin button or app restart).
* Total "Refresh Statistics Cache" button time end to end — target is back in the 20–60 second range.
* A side-by-side of key numbers before/after refactor (total brokers, broker-only %, broker+carrier %, top state, average age) confirming they match, with a note quantifying how much the monthly-chart and age numbers moved due to the Canadian-leak fix.
* The final index definition as it appears in the SSDT project, plus the manual script you ran on the live DB.
