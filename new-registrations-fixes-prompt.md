Three small fixes on the New FMCSA Registrations page (`/statistics/new-registrations`). No layout redesign — targeted changes only.

Fix 1 — replace the "All Time" range option with "Last 48 Months":
* The range filter bar at the top currently offers Last 12 / 24 / 36 Months / All Time (`?range=12m|24m|36m|all`). Replace `all` with `48m` (link label "Last 48 Months", monthsBack = 47, same pattern as the other ranges). Remove the min-DateAdded "all" logic from `GetNewRegistrationsData()`.
* Unknown or legacy `range` values (including `all` from old bookmarks/crawled URLs) must fall back to the default `24m` — no error, no redirect loop.
* Cache: the `48m` variant caches as `NewRegistrationsData_v1_48m`, lazy (only `24m` stays pre-warmed). The old `_all` cache entries die naturally via the prefix invalidation — nothing to migrate.
* The "+X% vs previous N months" badges must work for 48m (previous window = months 49–96 back).

Fix 2 — make the range scope obvious:
* Directly under the range filter bar, add a small muted caption: "The selected date range applies to all statistics on this page except Active Companies by Registration Age." (font/color per the page's existing footnote style).
* Every range-scoped card already shows "(Last N Months)" in its title — verify all of them update with the selected range, including the Monthly Registration Calendar and the choropleth.

Fix 3 — rename and explain the Age Distribution card:
* Title: "Registration Age Distribution" → **"Active Companies by Registration Age"**.
* Subtitle (replaces "By length of time since registration"): "How long ago currently active companies first registered with FMCSA".
* Add a small neutral pill/tag in the card header: "All active companies" — visually distinct from the "(Last N Months)" labels on the other cards.
* Donut center: the total active US company count with label "Active Companies" (verify that's what it already shows; fix if it shows the range cohort total).
* Footnote at the bottom of the card (replaces "Based on FMCSA registration date."): "Includes all active U.S. companies regardless of the date range selected above. Age is measured from the date the company was added to FMCSA records."

Implementation rules:
* Exact copy strings as given above; adjust only if a string breaks the card layout, and tell me if so.
* No changes to any other page; canonical stays `https://truckcarrierhub.com/statistics/new-registrations` without the range param.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The filter bar rendering 12M / 24M / 36M / 48M with the caption below it, and `?range=48m` loading with all "(Last 48 Months)" labels correct.
* `?range=all` falling back to the 24m view without error.
* The renamed age card: new title, subtitle, pill, center label, footnote.
* Confirmation the pre-warm list still contains only the `24m` variant for this page.
