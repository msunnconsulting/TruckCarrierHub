Two related changes: (A) new monthly archive pages `/statistics/new-registrations/{year}/{month}` (e.g. `/statistics/new-registrations/2026/06`), and (B) rename the "New FMCSA Registrations" block under "Explore Key Statistics" on the Statistics landing page to "Latest Registration Activity" with three new links pointing at those pages. No DB schema changes — everything is computed from existing data.

First, study `Views/Home/NewRegistrations.cshtml` and `HomepageService.GetNewRegistrationsData()` (the components and single-pull aggregation pattern to reuse), plus the "Explore Key Statistics" section markup in `Views/Home/Statistics.cshtml`.

## Part A — monthly archive page

Route/controller/view/VM/service:
* Route `StatisticsNewRegistrationsMonth`, url `statistics/new-registrations/{year}/{month}`, placed immediately before the existing `StatisticsNewRegistrations` route. Month is two digits in URLs we generate (`/2026/06`); accept `/2026/6` but 301-redirect it to the zero-padded form (one canonical URL per month).
* `HomeController.NewRegistrationsMonth(int year, int month)`. Valid = a COMPLETED calendar month, from January 2000 through last month. Current month, future months, out-of-range values → 301 redirect to `/statistics/new-registrations` (no 404s — these URLs will get probed).
* `Views/Home/NewRegistrationsMonth.cshtml`, `StatisticsNewRegistrationsMonthVM`, `GetNewRegistrationsMonthData(year, month)` in `#region Statistics`.

Data scope: active US companies with `DateAdded` inside the month (YYYYMMDD integer bounds: `yyyyMM01`–`yyyyMM31`). Company-type classification identical to the main page (Motor Carrier = contains C; Freight Broker = contains B not C; Other if nonzero). One pull of `(EntityType, PhysicalAddressStateCode, PhysicalAddressCity, DateAdded)` for the month plus the previous month (for the badge); aggregate in memory.

Page sections:
1. Breadcrumb `Home > Statistics > New FMCSA Registrations > June 2026`. Hero: H1 "New FMCSA Registrations — June 2026" (em dash convention), one-line intro naming the month, "Data updated / Source: FMCSA" line — same style as the parent page.
2. Three stat cards: Total New Registrations (with "+X% vs May 2026" badge — vs previous month, green/red, hidden if previous month is 0); New Motor Carriers (give its section/card `id="carriers"`); New Freight Brokers (`id="brokers"`).
3. "Registrations by Day" — bar or line chart of daily counts within the month (day-of-month x-axis), derived from `DateAdded % 100`, same SVG chart conventions.
4. Two-column row: "Registrations by Company Type" donut + count/% table; "Top 10 States in June 2026" ranked table (% of month total, bar fills).
5. "Top 10 Cities in June 2026" ranked table.
6. Month navigation row: "← May 2026" and "July 2026 →" links (previous/next month pages; suppress next when it would be the current/incomplete month, suppress previous before Jan 2000). This creates a crawlable archive chain.
7. Link card back to the main page: "Registration Trends — Last 24 Months" → `/statistics/new-registrations`.
8. Footer note: same "active FMCSA (USDOT) records… updated weekly… date the company was added to FMCSA records" wording as the parent page.

Caching (required):
* Key `NewRegistrationsMonthData_v1_{yyyyMM}`, 30-day absolute expiration, `NoSlidingExpiration`, lazy (NOT pre-warmed — do not touch the refresh button's pre-warm list).
* Add prefix `"NewRegistrationsMonthData_"` to `InvalidateStatisticsCache()`.

SEO (required):
* Title: `"New FMCSA Registrations — June 2026 | Truck Carrier Hub"`.
* Meta description (dynamic): `"[N] new trucking companies and freight brokers registered with FMCSA in June 2026 — [C] motor carriers and [B] freight brokers. Top states, top cities, and daily trends."` ≤160 chars validated in C#; truncate at 157 + "..." if over.
* Canonical: `https://truckcarrierhub.com/statistics/new-registrations/2026/06` (zero-padded, no trailing slash).
* JSON-LD WebPage + BreadcrumbList with 4 levels (Home → Statistics → New FMCSA Registrations → June 2026), Newtonsoft JObject pattern.
* Sitemap: in the admin "Generate Sitemaps" logic, add a `sitemap_registrations.xml` listing every completed month from the earliest month with data through last month, referenced from the sitemap index; `<lastmod>` = generation date.

## Part B — landing page section rename + links

In the "Explore Key Statistics" section of `Views/Home/Statistics.cshtml`, change ONLY the "New FMCSA Registrations" block (the other blocks stay untouched):
* Block title → **"Latest Registration Activity"**; keep the existing description text ("Track newly registered trucking companies and freight brokers.").
* Replace the block's current sub-link(s) with exactly three, month computed at render time as the last completed month (July 2026 → June 2026 — use `DateTime.Now` in Razor, never hardcode; these roll forward automatically):
  1. "New Trucking Companies Registered in June 2026" → `/statistics/new-registrations/2026/06#carriers`
  2. "New Freight Brokers Registered in June 2026" → `/statistics/new-registrations/2026/06#brokers`
  3. "Registration Trends — Last 24 Months" → `/statistics/new-registrations`
* No dead `href="#"` left in the block. No VM/cache-key change needed for this part (dates are computed at render, not cached).

Implementation rules:
* Reuse the parent page's components and CSS (`nr-` prefixed or shared) — no new design language; renders in the Bootstrap 3 grid.
* All copy "updated weekly"; all numbers comma-formatted from the ViewModel; percentages 1 decimal.
* URLs `https://truckcarrierhub.com`, no trailing slashes.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* `/statistics/new-registrations/2026/06` rendering all sections with real data; `<head>` excerpt (title, meta description + char count, canonical, JSON-LD).
* Redirect behavior: `/2026/6` → zero-padded 301; current month and junk values → 301 to the main page.
* Month navigation links on the June page (May ←, and confirm July is suppressed while incomplete).
* The renamed landing block with its three links and correctly computed month.
* Cache key format and the new prefix in `InvalidateStatisticsCache()`; confirm the pre-warm list is unchanged.
* The generated `sitemap_registrations.xml` excerpt (first and last entries).
* Cold and warm load time for one month page.
* Exact list of files changed.
