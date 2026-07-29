Add a "Trucking in {StateName}" data module to the directory State pages (`Views/Home/State.cshtml`, route `/{stateCode}`), mirroring the city data module: structured facts + computed highlights + links into the statistics section. No prose generation. Everything from cached VMs; never break the page.

First, study: the city module implementation (`Views/Home/City.cshtml` module section + `CityModuleHelper` in the Web project + `HomeController.City()`'s defensive pattern), `GetStateCompaniesData()` / `StatisticsStateCompaniesVM`, and the national-baseline VMs (`GetStatisticsData()`, `GetFleetOperationsData()`).

Data and gating:
* In `HomeController.State()`: call `GetStateCompaniesData(stateCode)` in a try/catch. It returns null for Canadian provinces (US-only by design) and on any failure — in those cases render nothing. No company-count threshold needed for US states, they're all large enough.
* National baselines from the cached `GetStatisticsData()` / `GetFleetOperationsData()` VMs; skip any highlight whose baseline is unavailable.
* Base population: all-active (directory context), same as the state stats page itself.

New VM fields (one query): add to `GetStateCompaniesData()` a single extra count pass for new registrations — last 12 months and the prior 12 months for that state (`DateAdded` YYYYMMDD windows; covered by `IX_Stats_Status_DateAdded` which includes the state code). **Bump the cache key `StateCompaniesData_v1_` → `_v2_`.**

Module layout (directly under the page heading, above the popular-cities/city-list sections; coexists with the state article if one renders — module goes below the article, same as cities):
1. `<h2>` heading "Trucking in {StateName}" — or omit the heading if the page's H1 already reads equivalently; match whatever heading logic the city page ended up with (no redundant stacking — the city page lesson).
2. Four stat cards (same `hpb-card`/icon-circle components as the city module):
   * Active Companies (sub: "% of U.S. total")
   * Active Power Units (sub: "across reporting companies")
   * New Registrations — Last 12 Months (sub: "+/-X% vs prior 12 months" badge, green/red, hidden if prior window is 0)
   * Owner-Operator Share (OneUnit ÷ TotalReporting; sub: "of companies reporting fleet size")
3. **"What stands out"** — computed highlights, state vs NATIONAL baselines. Candidates:
   a. Owner-operator share vs national (min 500 reporting)
   b. Large-fleet share (21+ units) vs national
   c. Average fleet size (power units ÷ reporting) vs national
   d. Registration growth (12m vs prior 12m) vs the national growth rate over the same windows
   * States cluster closer to the mean than cities, so qualification thresholds are tighter: ratio ≥1.5 or ≤0.67 (growth: ±10 percentage points vs national). Render at most 2, ranked by extremity, with the same assembled-sentence approach and 2–3 template variants per metric (picked by state-code hash). If nothing qualifies, omit the section.
4. Footer links: "Full {StateName} trucking statistics →" to `/statistics/state/{XX}` and "U.S. trucking statistics →" to `/statistics`. Uppercase state codes in URLs — standing rule.

Implementation rules:
* Reuse the city module's CSS classes; add state-specific ones only if unavoidable.
* Defensive rendering identical to the city module: any missing piece silently omits that piece, never an error.
* Comma-formatted numbers, percentages 1 decimal, bases labeled; "updated weekly" if freshness copy appears.
* The state page has no pagination or map toggle — no visibility JS needed.
* Do not touch the state statistics page, the city module, or `GetStateCompaniesData()` beyond the two new fields.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, clear the statistics cache, then show me:
* The module rendered for a big state (TX), a small state (VT), and a distinctive one (check ND or IA for large-fleet/agriculture deviations — expect a highlight).
* A Canadian province page (/ON) rendering with NO module and no errors.
* The exact highlight sentences for two states — different structure, per the variant rule.
* Cache key `_v2_` in code and prefix coverage confirmed.
* Warm State page load time unchanged; note the one-time cold cost per state.
* Exact list of files changed.
