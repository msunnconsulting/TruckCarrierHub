Four fixes from a full UI review of the statistics section and directory pages. Items 1–3 are targeted bugs; item 4 is an investigation with a fix.

1. Active Companies page — the 12-month registrations badge compares mismatched windows:
* The card shows "New FMCSA Registrations Last 12 Months … ▲71.3% vs same period 2025". The 71.3% is inflated because it compares the trailing 12 months against `NewSamePeriodLastYear`, which is only January-of-last-year through this-month-of-last-year (~7 months) — see the `acThisYearStart/acLastYearStart/acLastYearEnd` parameters in `GetActiveCompaniesData()` and the `last12Pct` computation at the top of `ActiveCompanies.cshtml`.
* Fix: compare trailing 12 months (months 1–12 back) against the PRIOR 12 months (months 13–24 back). Add the prior-12 window to the single-pass aggregate SQL (same YYYYMMDD integer pattern), expose it on the VM, and compute the badge from it. Keep the "New This Year" / YTD fields for whatever else uses them.
* The month-over-month badge on the "in June" card was already fixed — do not touch it.
* Bump `ActiveCompaniesData_` cache key version (VM shape changes).

2. Company list rows — the "Hiring : …" line is missing:
* `restore-hiring-indicator-prompt.md` specified each row in `CompanyListPartial.cshtml` shows `<strong>Hiring : </strong>` with driver-type names for effectively-hiring companies and "N/A" otherwise. Only the badge path appears implemented — non-hiring rows show nothing. Add the line per the original spec (the row VM already carries `NowHiring` and `CompanyDriverType`).

3. "Other" cargo type consistency:
* "Other" was removed from the Cargo Statistics page but still appears in the Active Companies page's cargo donut/table and in the city data module's Top Cargo Types list (`City.cshtml` module section — its data comes from the city VM's TopCargoTypes).
* Remove "Other" from both: filter it out of the Active Companies cargo list in `GetActiveCompaniesData()` (bump already happening in item 1) and out of the city module's top-5 (filter in the view or where TopCargoTypes is built — but do NOT change the city statistics page itself, which legitimately shows a Top 10 including Other; only the module's compact list drops it).
* If removing it from the module leaves fewer than 5 rows, show the next cargo type instead (i.e., take top 5 AFTER excluding Other).

4. Statistics landing page (`/statistics`) freezes the browser renderer:
* During review, scrolling the landing page froze the tab twice (30+ seconds unresponsive, screenshot capture timed out). The lower half of the page (registration trend chart, state directory grid, or another script) is likely hammering the main thread — possibly a resize/scroll handler loop or an unbounded redraw.
* Investigate: open the page, open DevTools Performance/Console, scroll through the full page, find what's consuming the main thread. Fix the cause (debounce the handler, remove the loop, or defer heavy work). If it turns out to be an external script (ads/GTM), tell me rather than fixing around it.
* Verify after: the full page scrolls smoothly top to bottom with no long tasks over ~200ms in the Performance trace.

Implementation rules:
* No public copy may contain internal column/field names (a sweep just fixed several — don't add new ones).
* Directory links: uppercase state codes and city segments, always.
* Comma-formatted numbers, percentages 1 decimal, bases labeled.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The corrected 12-month badge value with the two window totals it compares (SQL + rendered).
* A Birmingham row rendering "Hiring : N/A" and a hiring company's row with driver types + badge.
* The Active Companies cargo table and the Birmingham module cargo list, both without "Other".
* What was freezing the landing page and the fix; before/after description of the Performance trace.
* Exact list of files changed.
