Add two new blocks to the homepage: a live statistics band and an auto-rolling monthly report teaser, placed between the search/hero area and the existing content (intro article / state lists). No other homepage changes. This must not slow the homepage down — zero new heavy queries (details below).

First, study: `Views/Home/Index.cshtml` and `HomeController.Index()` (current structure, where the search bar ends and the article/state lists begin), `StateVM` (the homepage ViewModel), `Content/modern-theme.css` (the homepage's design system — navy `--ink-900`, amber `--signal-500`, Archivo headings, IBM Plex Sans body; read the CLAUDE.md design-system section), and `StatisticsIndexVM` / `GetStatisticsData()` / `GetNewRegistrationsMonthData()` in `HomepageService.cs`.

**Critical style rule: the homepage uses the modern-theme design system, NOT the statistics section's blue card styles. Build these blocks with modern-theme variables and typography so they look native to the homepage. Do not copy `stat-card-v2` / statistics-page CSS.**

Block 1 — statistics band (four compact cards in a row):
* Card 1: count of active U.S. trucking companies → whole card links to `/statistics/active-companies`. Label explicitly "Active U.S. Trucking Companies" — the homepage intro text quotes a larger US+Canada number, so the U.S. qualifier prevents the two from looking contradictory.
* Card 2: active freight brokers → links to `/statistics/active-brokers`. Label "Active U.S. Freight Brokers".
* Card 3: new registrations last 12 months → links to `/statistics/new-registrations?range=12m`. Label "New FMCSA Registrations (12 mo)".
* Card 4: U.S. cities covered — NOT linked, no hover affordance.
* All four numbers come from the already-cached `StatisticsIndexVM` via `GetStatisticsData()` (fields exist: total companies, ActiveBrokers, the 12-month registrations count, CitiesCount). Call it from `Index()` and expose the four numbers on `StateVM` (or a small nested object). `GetStatisticsData()` is 30-day cached and pre-warmed by the admin refresh button — this adds zero query cost to a warm homepage.
* Comma-format numbers; small muted "Statistics →" style link cue on the three linked cards.

Block 2 — monthly report teaser (single full-width card below the band):
* Content: "New: [Month Year] FMCSA Registration Report" + one line: "[T] companies registered last month — [C] motor carriers and [B] freight brokers. See top states, cities, and daily trends." + chevron. Whole card links to `/statistics/new-registrations/{yyyy}/{MM}` (zero-padded).
* Month = last completed calendar month, computed at render from `DateTime.Now` — rolls forward automatically, never hardcoded.
* Numbers from `GetNewRegistrationsMonthData(year, month)` — the same cached month VM the archive pages use (`NewRegistrationsMonthData_v3_*`). If it's cold, this is one fast index-covered query; acceptable. If the call returns null or throws, render NOTHING (no empty teaser, no error) — the homepage must never break because of this block. Wrap defensively.
* Style: quietly prominent — amber/`--signal-500` accent per the design system's one-accent rule, not a loud banner.

Placement: band directly after the hero/search section, teaser directly under the band, then the existing article and state lists continue untouched. Must sit correctly in the Bootstrap 3 grid and look right at mobile widths (cards wrap 2×2, teaser stacks).

Implementation rules:
* `HomeController.Index()` gains at most the `GetStatisticsData()` call and the month-VM call — no new service methods, no new caching, no VM shape changes to `StatisticsIndexVM`.
* No SEO metadata changes (title/description were just redone). No changes to the search bar, article rendering, state lists, or checkbox filters.
* All copy "updated weekly" if any data-freshness wording is used.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The homepage rendering both blocks with real data; screenshots or HTML at desktop and ~375px mobile width.
* The three band links and the teaser link (correct zero-padded month for the current date).
* Proof of the graceful-degradation path: temporarily simulate a null month VM and show the homepage rendering without the teaser and without errors.
* Homepage warm load time before/after (should be indistinguishable).
* Exact list of files changed.
