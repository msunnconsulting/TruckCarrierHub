Add directory links to every ranked Top States / Top Cities table across the statistics section, using ONE convention everywhere: the place NAME keeps its current link (statistics drill-down), and the COUNT cell becomes a link to the corresponding DIRECTORY page. Currently the statistics section has almost no links back to the directory (only the two CTA banners on the state/city stats pages) while the directory now links heavily into statistics — this closes the loop and points internal links at the state/city directory pages, which are the ones with the Search Console indexing problem.

The convention:
* State rows: count cell links to `/XX` (the state directory page). Example: "Texas | 15,638" — "Texas" keeps linking to `/statistics/state/TX`, "15,638" links to `/TX`.
* City rows: count cell links to `/XX/CITY-NAME` (the city directory page; spaces → dashes, exactly the format the existing city-stats CTA generates).
* The count link is styled as a normal table link (the site's link blue), with `title="Browse trucking companies in …"` for clarity. No new columns, no icons, no layout changes.
* **Uppercase rule — non-negotiable: every directory URL uses UPPERCASE state codes and UPPERCASE city segments (`/TX/HOUSTON`, never `/tx/houston`). Three lowercase bugs have already been fixed in these views (`.ToLower()` on link parameters) — do not reintroduce it. Grep your own diff for `ToLower` in link-generating code before finishing.**

Apply to every ranked place table in these views (grep for the ranked-table patterns to make sure none are missed — `ac-ranked-table` and its per-page equivalents):
* `Statistics.cshtml` (landing): Top States and Top Cities tables.
* `ActiveCompanies.cshtml`: Top 10 States, Top 10 Cities, and the "Top 10 States by New Registrations" table if present.
* `ActiveBrokers.cshtml`: Top States, Top Cities, and the Carrier-to-Broker Ratio by State table (count cells for carriers and brokers can both link to the same `/XX`; if that reads oddly, link only the broker count).
* `NewRegistrations.cshtml`: Top 10 States, Top 10 Cities.
* `NewRegistrationsMonth.cshtml` + `NewRegistrationsMonthCarriers.cshtml` + `NewRegistrationsMonthBrokers.cshtml`: their Top States / Top Cities tables (the directory link goes to the full city/state listing even though the row is about one month's registrations — intended).
* `FleetOperations.cshtml`: Top 10 States by Total Power Units and Average Fleet Size by State (the power-unit/average numbers are NOT counts of companies — for these two tables link the STATE NAME's existing behavior as-is and instead make the reporting-companies count link to `/XX` if a count column exists; if no company-count column exists, add the directory link on the state name's row as a small "browse →" suffix ONLY in these two tables — the one permitted exception to the no-new-elements rule).
* `CargoStatistics.cshtml`: no place tables — verify and skip.
* `StateCompanies.cshtml`: Top Counties table has no directory equivalent — skip counties; Top Cities table gets city directory links on counts.
* `CityCompanies.cshtml`: no place tables besides companies (already linked) — verify and skip.
* Where a table has "% of total" cells, those stay unlinked.

Also verify (no changes expected, just confirm): the two CTA banners on StateCompanies/CityCompanies still point to uppercase directory URLs, and the "View all …" expanders' extra rows get the same count links as the top-10 rows (the expander tables must not be forgotten).

Implementation rules:
* View-layer only — no VM, service, route, or cache changes (all tables already have the state code / city name in their row data; if any table lacks the state code needed to build the URL, tell me rather than adding VM fields silently).
* Links are plain `<a>` in the existing cells; keep sort/alignment/bar-fill rendering intact.
* No nofollow, no target=_blank.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* One rendered row example (HTML) from each modified view — name link + count link with their hrefs.
* Grep proof: zero `ToLower` in any href-generating expression across the statistics views.
* A count of directory links added per page (rough numbers fine) — the landing page alone should gain ~20.
* Confirmation the expander (View all states/cities) rows carry the links too.
* Exact list of files changed.
