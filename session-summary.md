# Truck Carrier Hub — session summary (checkpoint, July 8, 2026)

Working setup: Cowork (desktop Claude) reads the codebase, writes specs and Claude Code
prompts, and reviews results; Claude Code implements and builds. Claude Code sessions are
per-directory — always start them from `C:\aspnet4\TruckCarrierHub` so CLAUDE.md is loaded.

## Project facts (stable)

ASP.NET MVC 5 on .NET Framework 4.6.1 (not Core), five projects (Web, Infrastructure,
ViewModels, Database/SSDT, Common.Utility). Hand-rolled ADO.NET in most of the codebase, EF6
in HomepageService.cs. Database and some namespaces still named "PartnerCarrier" — intentional
debt, leave alone. SQL Server Express: no SQL Agent (external triggers only), 10GB per-DB cap,
1GB buffer pool. Duplicate filenames gotcha (`_Layout.cshtml` ×3 — only
`Themes/Bootstrap/Views/Shared/` is live; `Index.cshtml` ×4). `CompanyInformationVM` and other
Company/review VMs live in `TruckCarrierHub.ViewModels\User\CompanyVM.cs`. Statistics VMs live
in `TruckCarrierHub.ViewModels\Admin\BusinessVM.cs`. `DateAdded`/`DateLastChanged` are YYYYMMDD
integers. `EntityType` is a semicolon-separated code list: C Carrier, B Broker, S Shipper,
F Freight Forwarder, I Intermodal Equipment Provider, T Cargo Tank.

## Standing rules (now also in CLAUDE.md)

Every new heavy/public page (specs AND implementations) must include:
- Caching: ViewModel in `HttpRuntime.Cache`, 30-day absolute expiration, versioned key prefix,
  prefix registered in `HomepageService.InvalidateStatisticsCache()`, explicit pre-warm vs lazy
  decision.
- SEO: title, plain-text meta description ≤160 chars, canonical URL, JSON-LD
  (WebPage + BreadcrumbList minimum), cross-links from related pages.
- Data-update copy is always "updated weekly" (copy sweep completed — no "monthly" remains).
- **Entity classification (site-wide)**: Broker = EntityType contains "B" (hybrids "B;C" count
  as both broker AND carrier — never "B and not C" for broker counts). Carrier = contains "C".
  Pure broker = EntityType exactly 'B'. "Trucking companies" (statistics + homepage band) =
  active minus pure brokers. Disclose overlaps where both counts appear. Directory listing
  counts stay all-active. Full text in CLAUDE.md.

## Completed in earlier sessions

- Layout, Company, City, State page redesigns; homepage meta description fix
  (`GetPlainHomeMetaDescription()`); City canonical/pagination bug fix (self-canonicalizing
  paginated pages, "| Page N" titles); claim banner only for Status='A'; CSS-based
  Read More/Read Less.
- City articles/descriptions generation system (Claude API, claude-sonnet-4-6) with locked
  generation rules (size-tier language, no exact counts, median not average, length caps by
  tier, cargo/fleet disclosure rules). Batch generation ongoing by tier.
- City/State dynamic meta descriptions computed in controller. Article renders on page 1 only.
- FMCSA sync service: designed, deliberately paused — manual monthly process continues
  (`fmcsa-sync-business-rules.md`). MOTUS migration coming sometime in 2026: keep FMCSA field
  names isolated when the sync service is eventually built.
- City name corrections: `city_name_corrections.csv` (20,002 exact), `city_name_pattern_rules.txt`
  (55 wildcard rules); Tier A/B corrections applied; ghost cities deleted.
- Performance fixes: EF change-tracker leak in UpdateRecordFromPreMainTable (Detach +
  TryGetObjectStateEntry), City page N+1 ratings batched, DB-side sort/pagination, Update
  Cities double-count fix (physical address only, Status='A').
- County data: `McmisCountyCodes` (3,144 rows), `PhysicalAddressCountyCode` backfilled
  (1,263,604 rows); future syncs import PHY_CNTY directly.
- SEO: three-file static sitemaps (cities only where Article IS NOT NULL, lastmod from
  LastRenewedDate, admin "Generate Sitemaps" button), robots.txt, JSON-LD on Homepage/State/
  City/Company pages, active-only counts everywhere.
- Statistics section: `/statistics` landing, `/statistics/active-companies`,
  `/statistics/state/{code}`, `/statistics/city/{code}/{city}` — public controller actions on
  HomeController (routes in RouteConfig), views in `Views/Home/`, service methods in
  `HomepageService.cs` `#region Statistics`, Admin menu + navbar visibility setting.
  US-only (Canada excluded). `TotalNumberOfPowerUnits` for fleet math, >50,000 outlier filter.

## This session (July 6–7, 2026)

### Refresh Statistics Cache button — verified working
Already fully implemented end-to-end (view `Areas/Admin/Views/Statistics/ManageStatistics.cshtml`,
POST `/admin/business/refresh-statistics-cache` on BusinessController, invalidate + pre-warm +
timestamp JSON). Tested by user. Pre-warms: statistics landing, Active Companies, Active Brokers.
State/city caches remain lazy. Note: `Admin/StatisticsController` duplicates the
Settings/ManageStatistics routes that BusinessController also serves — harmless redundancy.

### Active Freight Brokers page — built and live
`/statistics/active-brokers` from user's mockup, styled to match Active Companies page (not the
mockup's green theme). Spec: `active-brokers-page-spec.md`; prompt:
`active-brokers-claude-code-prompt.md`. Route `StatisticsActiveBrokers`,
`HomeController.ActiveBrokers()`, `Views/Home/ActiveBrokers.cshtml`,
`StatisticsActiveBrokersVM`, `HomepageService.GetActiveBrokersData()`, cache key
`ActiveBrokersData_v1` (prefix registered in invalidator, pre-warmed by refresh button).
Approved deviations from mockup (census data limits):
- Property/Non-Property broker cards → Broker Only / Broker + Carrier (EntityType-based)
- Broker Authority Types donut → Broker Entity Types donut (EntityType combinations)
- "vs last year" badge dropped (no historical snapshots kept)
- Top 10 by MC Number → Longest-Registered Freight Brokers (earliest DateAdded)
- "Weekly updated" → "Updated monthly" (later reversed: sync cadence is weekly, all copy says "updated weekly")

### Perf fix 1 — refresh cache slowness (prompt: `refresh-cache-perf-fix-prompt.md`)
Root cause: `GetActiveBrokersData()` ran ~12 queries all filtered `EntityType LIKE '%B%'` with
no covering index → repeated clustered-index scans of the wide table. Fixed: new
`IX_Stats_Status_EntityType` index; method refactored to ~3 queries (single in-memory pull of
active US broker rows, aggregates computed in C#). Also fixed: Canadian brokers leaking into
the raw-SQL monthly/age/longest queries (missing US filter); silent `catch {}` removed.

### Perf fix 2 — State/City page cold loads (prompt: `state-city-stats-perf-fix-prompt.md`)
Root cause: `GetStateCompaniesData()` ~18 queries, `GetCityCompaniesData()` ~26, several on
columns no index covered (IccDocketNumber1Prefix, drivers total, DateLastChanged, EntityType).
Fixed: `IX_Stats_Status_State_City` replaced with 6-column INCLUDE
(TotalNumberOfPowerUnits, DateAdded, DateLastChanged, EntityType, IccDocketNumber1Prefix,
NNDriversGrandTotalInterstateAndIntrastate); state method → ~5 queries (single CASE-based
aggregate pass), city method → ~6 (single pull of city rows, buckets/median/ages/chart
computed in memory). Cargo 30-column SUM stays a single residual query (cargo columns
deliberately NOT indexed — 10GB cap). User confirmed all pages fast.

### Index inventory — SSDT now matches intent (verified in TransportCompany.sql)
Previously the five stats indexes existed only on the live DB (project drift). Now scripted:
- Legacy: IX_Address, IX_ZIP, IX_MC, IX_TrucksAndTractors, IX_SortRelevance
- IX_Stats_Status_State (Status, State) INCLUDE (City, PowerUnits, DateAdded, CountyCode)
- IX_Stats_Status_State_City (Status, State, City) INCLUDE (PowerUnits, DateAdded,
  DateLastChanged, EntityType, IccDocketNumber1Prefix, NNDriversGrandTotal…)
- IX_Stats_Status_DateAdded (Status, DateAdded) INCLUDE (State, City)
- IX_Stats_Status_PowerUnits (Status, PowerUnits) INCLUDE (State, City)
- IX_Stats_Status_State_County (Status, State, CountyCode) INCLUDE (PowerUnits, DateAdded)
- IX_Stats_Status_EntityType (Status, State) INCLUDE (EntityType, City, DateAdded,
  DateLastChanged) — partially overlaps State_City; kept because it's narrower (faster broker
  scans); first candidate to drop if the 10GB cap gets tight.
Live-DB parity assumed from applied scripts but not independently re-verified — quick SSMS
check if ever in doubt: `SELECT name FROM sys.indexes WHERE object_id =
OBJECT_ID('dbo.TransportCompany')`.

### Decisions
- Cache warming: keep current status (top 3 pages pre-warmed, state/city lazy). Fallback plan
  if needed later: background warm 50 states + top ~100 cities via
  `HostingEnvironment.QueueBackgroundWorkItem` after refresh, plus a Windows Task Scheduler
  warmer for app-pool-recycle cache wipes (recycles clear `HttpRuntime.Cache` ~daily).
- Workflow: specs/prompts written by Cowork into solution-root .md files; Claude Code
  implements. Prompt format: task up front, "study existing pattern X first", exact literal
  values, implementation-rules list, verification section. Never ask Claude Code to measure
  "before" timings on known-slow paths — it can burn 30+ minutes.
- Repo hygiene: ~747 files show line-ending-only churn in git — do not stage/commit them.

## Session July 7–8, 2026 (continued) — Statistics section buildout

### New FMCSA Registrations page — built and refined
`/statistics/new-registrations` (prompt: `statistics-cards-and-new-registrations-prompt.md`, fixes:
`new-registrations-fixes-prompt.md`). Range selector Last 12/24/36/48 Months via `?range=`
(no "All" — `range=all` and junk values fall back to 24m). Hero stat cards with "+X% vs previous
N months" badges (active-only windows, slight survivorship undercount in older windows —
accepted, no UI disclaimer). Three-series monthly chart (All/Carriers/Brokers), company-type
donut (Motor Carrier = has C; Freight Broker = has B not C; Other), top states/cities, choropleth,
monthly calendar. "Active Companies by Registration Age" card (renamed from Registration Age
Distribution): all active companies, not range-scoped — has "All active companies" pill, subtitle,
explanatory footnote; caption under range bar names it as the exception. Footer: registration
date = FMCSA record-add date (NOT operating-authority wording). Cache
`NewRegistrationsData_v1_{range}`, 24m variant pre-warmed, others lazy. Landing stat card links
with `?range=12m` so displayed 12-month number matches the opened view.

### Monthly registration archive — 3 pages per month
`/statistics/new-registrations/{yyyy}/{MM}` overview + `/carriers` + `/brokers` child pages
(prompts: `monthly-registrations-archive-prompt.md`, `monthly-type-pages-prompt.md`). Valid =
completed months Jan 2000 → last month; non-padded/invalid/current-month URLs 301. Child pages:
type-only daily chart, data-driven intro, top states/cities; zero-registration months render a
zero-state (no 404). Month switcher at BOTH top (under hero) and bottom of all three page types.
All three pages render from ONE cached month VM: `NewRegistrationsMonthData_v3_{yyyyMM}`, lazy,
prefix registered. `sitemap_registrations.xml` lists all three URLs per completed month.
No DB storage cost — computed from existing DateAdded/EntityType data.

### Statistics landing page — two-level card layout (prompt: `statistics-two-level-cards-prompt.md`)
- Level 1 (three cards): Active Trucking Companies (clickable), Active Freight Brokers
  (clickable), FMCSA Data Updates (not clickable; future: swaps to "Data and Reports").
- Level 2 (three topic cards): New FMCSA Registrations (clickable, 12-month count, two monthly
  sub-links to last completed month's carriers/brokers pages — computed at render, rolls
  forward); Fleet and Operations (inert, bullet list: fleet sizes / owner-operators / power
  units / drivers); Cargo and Equipment (inert, bullets: cargo types / commodities / hazmat /
  equipment mix). Inert cards await their future pages (optional-URL pattern).
- "Explore Key Statistics" section REMOVED entirely (Market Rankings, Latest Registration
  Activity, etc.). "Registration Trends" third link removed by choice — archive-index page
  (`/statistics/new-registrations/archive`) noted as future option.
- Typography scaled for 3-across cards (nums 2.3em, labels 1.25em, descs 1.15em, padding up).
  Level-2 icons unified to the SAME `stat-icon-circle sic-*` component as level 1 after a long
  hunt — em-based sizing differences were unfixable reliably; glyphs pinned via
  `.stat-icon-circle .fa { font-size: 24px }`.

### Active Companies page — population labeling fixes (done directly, not via prompt)
Top card (1,263,573 all active) vs 1,144,827 (reporting `TotalNumberOfPowerUnits > 0`) confusion
resolved: "Total" rows renamed "Total reporting fleet size", exclusion footnotes added
(~118.7k / 9.4% don't report power units), and top "Owner-Operator %" card denominator aligned
to reporting companies (matches donut; sublabel "of companies reporting fleet size").

### Copy cadence reversal
Site updates WEEKLY (user confirmed) — reversed the earlier "monthly" rule. CLAUDE.md standing
rules updated; ActiveBrokers page fixed. All future copy: "updated weekly".

### Ops/process
- `claude-code-recovery.md` created: resume steps (`cd /d C:\aspnet4\TruckCarrierHub`,
  `claude -c`), cmd-vs-PowerShell quoting, status-first message, git churn warnings, stuck-session
  checklist, never measure "before" timings.
- Claude Code stall diagnosed mid-session (30 min): was reading + fighting 747 line-ending-churned
  files; steered with "skip before-timings, commit only edited files".
- Directory/search perf prompt (`directory-search-perf-fix-prompt.md`) DRAFTED — covers USDOT/MC
  int-vs-string scan bug, autocomplete via Cities table, city-URL resolution, IX_CompanyName,
  SSDT drift (Cities/McmisCountyCodes tables + CompanyName column missing). Status: not yet
  confirmed run — check before assuming directory search is fixed.

## Session July 8, 2026 (later) — homepage evolution, definitions, polish

### Homepage repositioned as "directory + data platform"
- Statistics band: 4 cards (Active U.S. Trucking Companies → active-companies; Active U.S.
  Freight Brokers → active-brokers; New FMCSA Registrations 12mo → new-registrations?range=12m;
  U.S. Cities Covered, unlinked). Numbers ride the cached StatisticsIndexVM — zero query cost
  warm. Styled to MATCH the statistics pages (white cards, colored icon circles) on the site's
  paper-50 background via `.hp-stats-strip` wrapper (no borders — they read as a black line).
  Footnote: "Companies holding both carrier and broker authority are counted in both figures."
- Monthly report teaser: auto-rolling "New: [Month] FMCSA Registration Report" card →
  the month archive page; renders nothing if month VM unavailable (homepage never breaks).
- Intro article: rendered in a white card (`.homepage-page-decription` styled), first paragraph
  = HomePageDescription as lede; tightened line-heights per user taste.
- Dynamic placeholders in `Admin.HomePageDescription` (meta description + on-page lede):
  `{N:N0}` total active (US+CA, from Admin.NumberOfCompanies), `{NUS:N0}` / `{NCA:N0}` per
  country (GetHomeCountryCounts, cached `HomeCountryCounts_v1`, prefix registered). Template in
  use: "Search {NUS:N0} active US and {NCA:N0} Canadian trucking companies and freight brokers.
  Statistics, registration reports, jobs, freight quotes — updated weekly." (~160 chars).
  NOTE: the "Truck Driver Jobs" popover replacement no longer fires (phrase absent from new
  description) — deliberate. HomeArticle = 5-paragraph platform pitch with internal links.
- New homepage title recommended: "Trucking Company Directory & Industry Statistics |
  Truck Carrier Hub".

### Entity-definition changes (see standing rules)
- Broker = contains B everywhere (was B-and-not-C in registrations code — fixed via
  `broker-definition-change-prompt.md`; company-type donuts now Carrier only/Broker only/Both/
  Other; overlap notes shown when nonzero; month cache → v4, range cache → v2).
- Trucking companies = active minus pure brokers (EntityType='B' exactly) — one number across
  homepage band, statistics landing, Active Companies page (whole page base), Fleet & Cargo
  total cards (`trucking-companies-count-prompt.md`; StatisticsData → v6, ActiveCompaniesData
  → v5, Fleet/Cargo bumped). {NUS}/{NCA} intro counts stay all-active by design (sentence says
  "companies and freight brokers"). Directory counts untouched.
- Data finding RESOLVED (July 9): pure-broker registrations ran 330–460/month for 10 months,
  then May 2026 = 56 and June 2026 = 0 — the FMCSA MOTUS transition (registration systems
  offline May 14; URS/L&I retired; brokers now register via Motus). July recovering (58 in
  first 9 days). Upstream effect, NOT a sync bug; nothing to fix. June's brokers archive page
  correctly shows the zero-state. Watch weekly syncs for full recovery to ~400/month.
  MOTUS status: Phase II live May 14, 2026; MCMIS remains the back end — census extract still
  flows; keep FMCSA field mappings isolated for the eventual sync service anyway.

### Fixes and polish (mostly direct edits)
- EF BIGINT materialization error in Fleet aggregates (SUM(int) → int vs long DTO props):
  all long-mapped SQL columns now CAST AS BIGINT. RULE: any long DTO prop needs BIGINT cast.
- Mojibake ("2â€“3 Types"): `<globalization fileEncoding="utf-8" .../>` added to Web.config
  (Claude Code writes BOM-less UTF-8 .cshtml); label also uses – escapes.
- modern-theme.css cache-busting: `?v=<file-ticks>` on the layout link.
- Cargo page: "Other" cargo type removed everywhere; expander "View all cargo types";
  first-click toggle bug fixed (style.display vs CSS-hidden).
- Fleet page: Equipment Breakdown section removed (only 4 usable columns; user pulled it);
  landing Fleet card bullets: Fleet Size Distribution / Owner-Operators / Operational Scope /
  Hazmat Carriers. US-filter rider applied to fleet aggregates.
- Active Companies page: "New FMCSA Registrations This Month" card → last completed month vs
  month before (named months, e.g. "in June … vs May"); VM fields still named
  NewThisMonth/NewLastMonth (naming debt).
- Company links: Houston fleet table /business/{id} → /{ST}/USDOT-{id}; brokers page
  Longest-Registered links .ToLower() → .ToUpper(). All other links already uppercase from DB.
- City stats hero: county title-cased; "Located in X County." hidden when county name equals
  city name (LA case).
- robots.txt: Disallow /Admin/, /create-account/, /change-password/, /business/, /verify/,
  /email/unsubscribe/, /error, both autocomplete endpoints. Claim links already rel=nofollow.
- Statistics landing: card 3 links with ?range=12m (number/window match); "Registration
  Trends" third link removed (archive index page deferred).

### Production readiness (as of this checkpoint)
Restore production DB from local (covers indexes/tables/Admin fields). MUST before deploy:
set `<compilation debug="false">`; rebuild; re-verify Refresh Statistics Cache end-to-end
(BIGINT fix never re-confirmed); confirm `directory-search-perf-fix-prompt.md` was actually
run. After deploy: regenerate sitemaps, resubmit in Search Console, enable Statistics nav
toggle, press Refresh Statistics Cache, smoke-test homepage/statistics/monthly page/range
variant/USDOT search.

## Session July 8–9, 2026 — city data module replaces articles, hiring consistency

### City articles RETIRED, data module built (prompt: `city-data-module-prompt.md`)
Decision: generated city articles were near-identical templates (identical sentences across
cities) — not worth the API cost or hallucination risk. Distinctiveness is COMPUTABLE.
- Article block removed from directory City pages entirely (Read More toggle + truncation too).
  `Cities.Article` column, generation pipeline, and admin Renew City Content are now orphaned —
  retire later. `Cities.Description` STILL FEEDS the city meta description tag (keep).
- Sitemap still keyed on `Article IS NOT NULL` — deliberately NOT changed yet; revisit after
  observing indexing (future criterion: cities with 50+ companies).
- New module on City pages (50+ active companies, page 1, list view only): 4 stat cards
  (homepage-band styling), top-5 cargo list (≥30 classified), computed "what stands out"
  highlights (city vs state/national baselines, ratio ≥2× or ≤0.5×, max 3, template variants
  picked by city-name hash), footer links to the city + state statistics pages (the
  "New registrations" third link was removed — uniform 2 links). All data from cached VMs;
  fails silently; all-active base (directory context).
- Hard-won edge cases: pagination and List/Map toggle are AJAX (no server re-render), so the
  module is ALWAYS rendered in page-1 HTML (hidden via inline style when the page loads in map
  mode — ViewBag.CityModuleHidden) and shown/hidden client-side in MapView/MapToListView/
  OnPageChange (`.cm-wrap`). "pos-…" URL = map view; "pos_lst-…" = list view WITH map bounds
  (module renders). search-filter-on-company.js now cache-busted like modern-theme.css.
- City page header: H1 title-cased; "N active companies found" line shows ONLY when the module
  doesn't render (small cities, pages 2+); module has no own heading.
- City stats hero: county line hidden when county name equals city name.

### Hiring indicator restored + consistency (prompt: `restore-hiring-indicator-prompt.md`, run)
- Company page Hiring section: every trailer/driver type now shows explicit state — "Flatbed ✓"
  (green) vs "Tanker — N/A" (muted). Was rendering the full catalog with no indication.
- "Effective hiring" rule (site-wide): hiring = `Business.NowHiring = 1` AND at least one
  trailer OR driver type selected. Company page shows "Not currently hiring" (no contacts)
  otherwise. List rows show "Hiring : <driver types>" + green Now Hiring badge, or "N/A".
- Save path already derives NowHiring from type selections (can't create bad state); legacy
  inconsistent rows fixed with one-time UPDATE (Business.NowHiring=0 where no junction rows) —
  run on any DB restore/production copy that predates the cleanup.

## Session July 10, 2026 — the partnercarrier.com discovery (MAJOR SEO finding)

Google's URL Inspection showed a referring link from partnercarrier.com (the pre-rebrand
domain). Investigation revealed: **partnercarrier.com was still live and serving a complete,
database-backed, old-brand copy of the entire site** — hosted on an expired SmarterASP "002"
account (server 154.53.51.126) that kept running for years after the rebrand and even after
its expiry. Google had been crawling two full copies of the 1.3M-page directory the whole time
— a plausible major contributor to the 128,700 "Crawled – currently not indexed" problem. The
domain migration from the 2022-era rebrand was simply never completed.

Fix applied (all verified working):
1. GoDaddy: partnercarrier.com nameservers switched from the orphaned site4now (SmarterASP)
   zone to GoDaddy defaults; A records @ and www → 154.53.33.43 (production VPS).
2. Production web.config: legacy-domain rule added FIRST in <rewrite> — any
   (www.)partnercarrier.com host 301s single-hop to https://truckcarrierhub.com/{path}.
3. IIS: partnercarrier.com + www bindings (port 80) added to the TruckCarrierHub site.
   **301 verified working.**
4. win-acme (Let's Encrypt) cert issued for partnercarrier.com + www, 443 bindings created,
   auto-renew scheduled. **HTTPS 301 verified working too.** Migration technically complete.
5. Search Console Change of Address ACCEPTED July 10, 2026 — "This site is currently moving:
   partnercarrier.com → truckcarrierhub.com". Google keeps the move active ~180 days; do NOT
   remove the redirects, bindings, or cert during (or after) that window.
Pending: SmarterASP — confirm full deletion of the expired 002 account (site files, database —
contains pre-rebrand business accounts/password hashes — and its site4now DNS zone). Watch
partnercarrier.com's indexed pages drain into truckcarrierhub over the coming weeks in Search
Console, alongside the new-content cohorts. Web.config merge discipline + redirect test URLs:
`production-rewrite-rules.md`.

Also noted: build output DLLs still named PartnerCarrier.* (AssemblyName in csproj) —
cosmetic, internal-only, consistent with the intentional-debt decision; leave alone.

## Session July 11, 2026 — state module, sync follow-ups, directory UX bugs

### State data module (prompt: `state-data-module-prompt.md`, implemented)
Directory State pages now have the module (4 cards: Active Companies w/ % of U.S., Power
Units, New Registrations 12mo w/ prior-12 badge, Owner-Operator share; "what stands out" vs
national with tighter 1.5×/0.67× thresholds; links to state stats + national overview).
`GetStateCompaniesData` gained the two registration fields → cache key `StateCompaniesData_v2_`.
Canadian provinces: no module (stats are US-only). Order fixed to match homepage: module first,
state article below. Internal-linking lattice complete: homepage → state → city all feed the
statistics section; statistics count-links feed back.

### Finish City Update (Beaverlodge case)
New city BEAVERLODGE (AB) missing from directory after sync → root cause: Cities table not
yet rebuilt. Direct GET of `/admin/business/finish-cities-update` returned success and fixed
it — the function works; the BUTTON gave no feedback. Button hardened: disabled while running,
live elapsed counter, 10-min timeout with honest message, always renders success/danger alert,
splits response on first colon only. NOTE: the method still aborts wholesale if ANY active
company has a state code missing from States (incl. NULL) — diagnostic SQL in chat history;
possible future soften-to-skip change.

### Directory UX bugs fixed (all verified live in browser)
- GRAND RAPIDS showed as "GRAND-RAPIDS" in state city lists: the URL-building code MUTATED
  the shared model objects (`city.CityName = ...Replace(" ","-")`), and since the perf work
  made popular-cities a slice of the same list, the mutation leaked into the alphabetical
  list. Both loops now use a local URL segment. LESSON: never mutate model objects in views.
- Browser Back was dead after filtering: legacy JS re-applied the last filter from
  localStorage on EVERY page load (filters followed users across pages/sessions forever, and
  Back → reload → re-apply → same page). **BEHAVIOR CHANGE: filters now live in the URL
  only** — localStorage restore removed; popstate handler now uses location.reload();
  pageshow(persisted) reloads city pages restored from bfcache.
- Ghost spinner + self-scrolling when returning to state pages: `#al` loader shown before
  navigating away persisted in the bfcache snapshot. Site-wide fix in the shared layout:
  pageshow handler hides #al on every page show.
- Hiring line confirmed rendering on city rows ("HIRING N/A" / driver types).

### Chrome-extension debugging workflow (established)
Claude connects to the user's Chrome via the Claude in Chrome extension for live repro/verify
(user enables it on demand, disables after — disabling is sufficient, no need to uninstall).
Screenshots after scrolling sometimes render zoomed/offset (capture artifact — ignore) and
captures occasionally time out (retry with a wait).

### Production deployment + "all statistics sub-pages error" incident (RESOLVED)
First production deploy of the statistics work. Symptom: `/statistics` landing and all
directory pages worked, but EVERY statistics sub-page (active-brokers, active-companies,
state/XX, fleet-operations, new-registrations, monthly pages) instantly showed the generic
"Error." view; nothing in app-log.txt, no Event Viewer 3005 (the one found was an unrelated
phpinfo scanner probe). Root cause: 10 new `Views\Home\*.cshtml` files plus
`Content\statistics-page.css`, `Content\us-states-110m.json`, `Scripts\topojson-client.min.js`,
and `robots.txt` were on disk but NOT in TruckCarrierHub.Web.csproj — MSBuild publish only
ships csproj-listed files, so production had new DLLs but no new views ("view not found" →
HandleErrorAttribute → instant Error view, nothing logged). Locally invisible because IIS
Express serves views from the source folder. Fixed: all 14 entries added to the csproj
(verified all .cs files were already in), user rebuilt/republished, pages confirmed working.
Rule added to CLAUDE.md standing rules: every new file must get a csproj entry, verify with a
disk-vs-csproj diff. Side note: robots.txt had never been publishing — the updated Disallow
list only reached production with this fix.

## Session July 12, 2026 — post-deploy fixes, homepage data-freshness card, dead-code cleanup

### csproj incident closed
User republished after the csproj fix (see July 11 section) — all statistics sub-pages
confirmed working on production. Production robots.txt verified correct (first time it ever
shipped via publish). Sitemap line still to be added when sitemaps are regenerated:
`Sitemap: https://truckcarrierhub.com/sitemap.xml`.

### Homepage "FMCSA Data Updates" card (5th band card)
User wanted the old "database last updated" indication back. First attempt: site-wide footer
line (service method + layout + CSS) — user didn't like it, FULLY ROLLED BACK (no trace left;
a `GetLastDataUpdateDate()` service method was added and then removed). Final version: 5th
card on the homepage stats band matching the Statistics landing card — gold calendar icon,
date, "FMCSA Data Updates", "Data is updated weekly to ensure accuracy." Implementation:
`StateVM.HpLastDataUpdate` (string) ← `hpStats.LastDataUpdate` in HomeController.Index (zero
extra queries — homepage already calls GetStatisticsData()); card renders only if date
non-empty. All five hpb cards made smaller to fit 5-up: card padding 16/15/14, icons 52→46px
(fa 24→21px), `.hpb-num` 1.9→1.7em; new `.hpb-num-date` (1.25em, line-height 1.5 so labels
align across cards) and `.hpb-desc` classes. Mobile unchanged (50% wrap).

### City articles/descriptions fully retired from public site
Confirmed: `Cities.Article` no longer rendered publicly (city data module replaced it);
`Cities.Description` fully dead (computed meta description replaced it). Dead code REMOVED:
the two per-request queries in HomeController city action (GetPageDescription "Citypage" call
+ GetNumberOfWordsAllowedByAdmin — 2 wasted queries per city page view), the "Citypage" branch
inside GetPageDescription (explanatory comment left in place), orphaned GetCityMetaDescription
+ GetNumberOfWordsAllowedByAdmin methods + their IHomepageService entries, and unused
`CompanyVM.NumberOfWordsForCityArticle`. KEPT: the Cities columns themselves, admin tooling
(Manage Cities / City Articles screens), and `States.StateArticle` (still rendered on State
pages). SQL provided to null out Cities.Article/Description with a backup table
(`Cities_ArticleBackup_20260712`) — user planned to run it; keep the backup table until the
auto-generated city articles project ships (it will likely write into Cities.Article again).

### Cowork organization
User created a "Truck Carrier Hub" Cowork project (points at C:\aspnet4\TruckCarrierHub).
Regular chats can be moved into projects via the chat dropdown; Cowork sessions (different
sidebar icon) currently cannot — not a problem, continuity lives in this file + CLAUDE.md,
which new project sessions pick up automatically. New sessions should be started from inside
the project.

### Search Console error triage — search-icon fix, and a real bug found along the way
User asked about two GSC "Other error" entries on the homepage: an image
(`ui-bg_flat_75_ffffff_40x100.png`) and a script (`googletagmanager.com/gtag/js`). The gtag
one is benign (Google's own renderer sandbox occasionally fails to fetch its own analytics
script; not fixable or actionable from this codebase — same root category as the earlier
AdSense CORS "abg_config" error). The image one was real: `BundleConfig.cs`'s
`~/Content/css-frontend` StyleBundle (bootstrap.css, bootstrap-override.css, font-awesome,
jquery-ui.css, tagsinput, Site.css) never had `CssRewriteUrlTransform` on any Include, so
relative `url(...)` refs inside those files resolved against the bundle's own virtual path
instead of each source file's real folder — 404s in production only (bundling is disabled
locally since `Web.config` has `debug="true"`, so this was invisible in dev). Added the
transform to all six Includes; fixed jquery-ui's image and Font Awesome's fonts, but Bootstrap's
glyphicon `@font-face` stayed broken even after a full rebuild+republish+app-pool-recycle —
root cause never identified. Bypassed by hardcoding the real absolute path directly into
`bootstrap.css`'s font-face block instead of depending on the transform for that one file.

Second layer, found by testing live in Chrome: even with the font file loading correctly, the
search-icon (`glyphicon-search`, used site-wide in the nav bar, homepage, and City page filter)
and the account-menu gear icon (`glyphicon-cog`) still didn't render. Cause: `modern-theme.css`
line 40 (`body, p, div, span, ... { font-family: var(--font-body) !important; }`) includes
`span`, and glyphicons render on `<span class="glyphicon ...">` — the `!important` silently
overrode Bootstrap's own `.glyphicon:before { font-family: 'Glyphicons Halflings' }` rule.
Same category of bug as the button-color `!important`/specificity issue already in this file's
history, just not caught for this element. Fixed with a targeted `.glyphicon:before` override
(also `!important`, higher specificity) appended to `modern-theme.css`.

Also found while chasing why the bootstrap.css fix wouldn't stick in production even after two
full rebuild+republish cycles: **ASP.NET's bundle cache for `~/Content/css-frontend` did not
pick up the file change even after an IIS app-pool recycle** (raw file confirmed correctly
updated on disk each time; the bundle kept serving old content regardless). Root cause not
pinned down (kernel-mode HTTP.sys output caching surviving app-pool-only recycles is the leading
theory — worth an `iisreset` instead of just a recycle if this happens again). Final fix routes
around the bundle entirely: the glyphicon font-face override lives in `modern-theme.css`, which
loads as a plain `<link>` right after the bundle and has its own cache-busting
(`?v=<file-ticks>`), so it wins regardless of what the bundle does.

Confirmed live in Chrome (fetched `/Content/css-frontend` directly, checked computed
`font-family` on `.glyphicon-search::before`, screenshot of the rendered search button).

### Publish profile audit and cleanup
While debugging why the bootstrap.css deploy wasn't taking effect, discovered the project had
**four publish profiles** in `TruckCarrierHub.Web/Properties/PublishProfiles/`, targeting three
different local folders, with no clear indication which was live:
- `ProductionProfile` → `C:\Publish\NewProductionAug31_2020` — no publish history, files frozen
  2017–2020. Dead.
- `PartnerCarrierProductionProfile` → `C:\Publish\Production` — last published Nov 18, 2025. Dead.
- `FolderProfile` → `C:\Publish\Production` (same folder as above) but build config **Debug** —
  last published Oct 11, 2025. Dead, and was the most dangerous of the four: same target folder
  as another profile, wrong config, would've silently shipped a debug build (bundling disabled)
  if ever picked by mistake.
- `FolderProfile1` → `C:\Publish\TruckCarrierHub`, Release config — confirmed via its
  `.pubxml.user` history log as the one actually in use (publish recorded today, file list
  matches this session's edits exactly). **This is the real one.**
Deleted the three dead profiles (both `.pubxml` and `.pubxml.user` for each). Left
`FolderProfile1` un-renamed — renaming means delete+recreate, which would wipe its
`.pubxml.user` publish-history log for no real benefit now that it's the only profile left.
Underlying deploy flow is still local-folder publish + a separate manual step to get files onto
the actual production server (RDP/FTP, not automated) — that manual step is what caused the
earlier confusion in this session (files replaced at the wrong path, `bootstrap-override.css`
mixup). Consider moving to direct Web Deploy (MSDeploy straight to the server) to remove that
manual step entirely, if the production server's Web Management Service can be enabled for it.

## Files worth bringing into a new chat
- `session-summary.md` (this file)
- `CLAUDE.md` (project context + standing rules)
- `fmcsa-sync-business-rules.md`, `city_name_corrections.csv`, `city_name_pattern_rules.txt`
- Specs/prompts: `active-brokers-page-spec.md`, `active-brokers-claude-code-prompt.md`,
  `refresh-cache-perf-fix-prompt.md`, `state-city-stats-perf-fix-prompt.md`,
  `page-redesign-specs.md`

## Where things stand / next
Statistics section: landing (two-level cards), Active Companies, Active Brokers, State, City,
New Registrations (+ range filters), and monthly archive (3 pages/month, back to Jan 2000) all
built, fast, cached, with SEO metadata. DEPLOYED TO PRODUCTION July 11, 2026 (after the csproj
incident above; sub-pages confirmed working July 12). Remaining post-deploy checklist:
NowHiring legacy cleanup UPDATE on production DB, regenerate sitemaps on production (+ add
Sitemap line to robots.txt), Refresh Statistics Cache on production, rotate the exposed OpenAI
key + sa password / create non-sa SQL login, run the Cities.Article/Description null-out SQL,
deploy the July 12 changes (homepage 5th card + dead-code cleanup — all in existing files, no
csproj changes needed). Roughly in order:
1. Run/verify `directory-search-perf-fix-prompt.md` (homepage/state/city/search speed) if not
   already done
2. Fleet and Operations page + Cargo and Equipment page — the two inert level-2 landing cards
   are waiting for them (make clickable when built)
3. "Data and Reports" concept — replaces the Last Updated card on level 1 when ready
4. Registration archive index page (`/statistics/new-registrations/archive`) — deferred option
5. City article batch generation continues by tier
6. Deployment to production; after deploy, watch Search Console on the new monthly archive
   pages before multiplying page types further
7. Longer term: FMCSA Socrata sync service (paused), MOTUS migration watch, ~128,700
   "Crawled – currently not indexed" content-quality work
