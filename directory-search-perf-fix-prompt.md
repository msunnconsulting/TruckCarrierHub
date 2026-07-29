Speed up the public directory pages and search: homepage, state pages (`/{stateCode}`), city pages (`/{stateCode}/{city}`), the search box (all four dropdown options: City, Company Name, USDOT Number, MC Number), and both autocomplete endpoints. The slowness comes from non-sargable predicates, full scans of TransportCompany where the small `Cities` table already has the answer, missing indexes, and redundant query executions. Behavior must stay identical — including that search currently matches inactive companies too (no Status filter in search); do NOT add one.

First, study these before changing anything:
* `HomepageService.cs`: `GetUsStates`/`GetCaStates` (the no-filter "fast path" reading from `Cities` — this is the pattern to reuse), `GetCityList`, `GetPopularCities`, `GetSearchResultAutoComplete`, `GetSearchResultAutoCompleteCity`, `GetCompanyListFromSearch`, `GetCityNameFromURLCityName`.
* `HomeController.Search()` — note that its only outputs are redirect URLs (it never renders the company list).
* The `Cities` table via the EF `City` entity: (CountryCode, StateCode, CityName, NumberOfCompanies, Article, Description, LastRenewedDate). Active-only counts, refreshed monthly by the admin "Update Cities" process.

Change 1 — kill the string-conversion scans in `GetCompanyListFromSearch()`:
* USDOT branch: `(companies.USDOTNumber + "") == searchText` string-converts the INT clustered primary key — full scan. Replace with `int.TryParse(searchText, out n)` in C#, then `companies.USDOTNumber == n` (instant PK seek). If parse fails, return the "no company matched" BusinessException without querying.
* MC branch: `(companies.IccDocketNumberFirst + "") == searchText` — same anti-pattern; it defeats the existing `IX_MC` index. Same fix: parse to int, compare as int, seek.
* Apply to both the hiring-filter and non-filter variants of each branch.

Change 2 — stop executing the search query three times, and stop fetching a page to build a URL:
* `GetCompanyListFromSearch` runs `companyList.Count()` for the empty check, then `SelectByPaging` runs a count and a page fetch. `HomeController.Search()` then uses only the city/state of the first row (City option) or the first USDOT (other options) to build a redirect URL.
* Add a lean service method for the Search action that returns just what it needs (e.g. TOP 2 of `USDOTNumber, PhysicalAddressCity, PhysicalAddressStateCode` — 2 rows so existing first-match semantics are preserved deterministically with the same ORDER BY as today). Keep `GetCompanyListFromSearch` itself for any other callers, but fix its double Count (single execution via the paging helper only).

Change 3 — autocomplete:
* City autocomplete (`GetSearchResultAutoComplete` "City" branch and `GetSearchResultAutoCompleteCity`): currently `StartsWith` + `Distinct` over ALL TransportCompany rows (active + inactive) with NO row limit. Rewrite to query the `Cities` table: `CityName.StartsWith(text)`, project "CityName, StateCode", order by NumberOfCompanies descending, `Take(15)`. This changes results to active cities only — intended and correct (inactive-only cities 404 anyway via the city page).
* Company Name autocomplete: keep querying TransportCompany (`CompanyName.StartsWith`), keep `Take(50)`, but it needs the index from Change 5 to be fast. Do not change its result shape.

Change 4 — city-name resolution on the City page (`GetCityNameFromURLCityName`):
* Currently up to 3 sequential TransportCompany queries; the third (`PhysicalAddressCity.Replace(" ", "-") == urlCityName`) computes Replace on every row — guaranteed full scan, and it runs for every URL where the first two miss, including every crawler 404 probe.
* Rewrite against the `Cities` table: one query `WHERE CityName == dashesReplacedBySpaces OR CityName == originalUrlValue`; if no hit, fetch the state's city list (small) and match `CityName.Replace(" ", "-") == urlCityName` in memory. Accept the method's current signature; if adding the stateCode parameter is trivial at the call site, add it and filter by state — it narrows the search and fixes cross-state false matches.
* Preserve exact return semantics: same casing/value returned as today (city names are stored uppercase in both tables — verify with one query before assuming).

Change 5 — indexes + SSDT drift (script into `TruckCarrierHub.Database` AND provide manual scripts for the live DB):
* NEW `IX_CompanyName` ON `dbo.TransportCompany (CompanyName)` INCLUDE `(PhysicalAddressCity, PhysicalAddressStateCode)` — serves Company Name autocomplete (LIKE 'x%' is sargable) and exact-match search.
* The `CompanyName` column exists in the live DB and EF model but is MISSING from `TransportCompany.sql` in SSDT — script its exact definition from the live DB (check whether it is a computed column before assuming) and add it.
* The `Cities` and `McmisCountyCodes` tables are entirely missing from the SSDT project — script both from the live DB, including any existing indexes/PKs. If `Cities` has no index on `(StateCode, CityName)`, add one (INCLUDE `NumberOfCompanies`): it serves the state page, autocomplete, and city-name resolution.

Change 6 — state page query reduction (`GetCityList` + `GetPopularCities`, no-filter path only):
* Both currently group the state's TransportCompany rows on every request — two aggregate scans. For the no-filter path (both checkboxes off), read from `Cities` instead: `WHERE StateCode == state`, project CityName/StateCode/NumberOfCompanies, order by CityName. Popular cities = top 10 by NumberOfCompanies of the SAME in-memory list — zero extra queries. This mirrors the existing `GetUsStates` fast-path pattern and gives identical numbers (Update Cities maintains them: physical address, Status='A').
* Keep the hiring/reviews filter paths exactly as they are (they need TransportCompany joins), but in `State()` the controller calls both methods — for the filtered path too, derive popular cities from the full city list in memory instead of running the second grouped query.

Implementation rules:
* No behavior changes beyond those explicitly stated (city autocomplete active-only, Take(15) limit). Search must still match inactive companies. Result ordering, casing, and URL formats stay identical.
* Keep the synchronous EF/LINQ style of the file; no new packages.
* Homepage: no changes needed (it already uses the Cities fast path) — do not touch it.
* Do not stage or commit the line-ending-churned files; commit only files you actually edit.

After implementing, build with zero errors, then measure and show me (after-timings only — do not measure the old slow paths):
* Search round-trip for each of the four dropdown options (use a real USDOT, MC number, company name, and city).
* Autocomplete response time for a 3-letter prefix, City and Company Name modes, and confirm City mode returns ≤15 rows.
* City page load for a city whose name contains a dash (e.g. WINSTON-SALEM, NC) and one 404 probe (nonexistent city with a dash) — both should be milliseconds for the resolution step.
* State page load for TX (no filters) and confirm the city list + popular cities numbers match the Cities table values.
* The final SSDT additions: CompanyName column definition, IX_CompanyName, Cities and McmisCountyCodes table scripts, and any new Cities index — plus the manual scripts run on the live DB.
