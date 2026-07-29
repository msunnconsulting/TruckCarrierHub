Restructure the monthly registration archive: split the carrier/broker sections of `/statistics/new-registrations/{year}/{month}` into standalone child pages `/statistics/new-registrations/{year}/{month}/carriers` and `/statistics/new-registrations/{year}/{month}/brokers`. Three pages per month total; each must be differentiated enough that they don't read as near-duplicates.

First, study the current `Views/Home/NewRegistrationsMonth.cshtml` (it already contains the `id="carriers"` / `id="brokers"` sections with type-scoped top-10 tables — that content MOVES to the child pages) and `GetNewRegistrationsMonthData()` + `StatisticsNewRegistrationsMonthVM`.

## Page structure

Month overview page (existing, slimmed):
* Keeps: hero, three stat cards, "Registrations by Day" (all registrations), company-type donut, month prev/next navigation, footer.
* The two in-page type sections are REMOVED and replaced by two link cards: "New Motor Carriers in June 2026 →" and "New Freight Brokers in June 2026 →" pointing to the child pages (reuse the Related Statistics card style).

Carrier child page (`.../carriers`):
* H1 "New Motor Carriers — June 2026"; breadcrumb `Home > Statistics > New FMCSA Registrations > June 2026 > Motor Carriers`.
* Intro sentence built from real data: carrier count, share of the month's total registrations, top state (e.g. "4,686 new motor carriers registered with FMCSA in June 2026 — 88.1% of all new registrations that month, led by Texas.").
* Stat cards: New Motor Carriers (with vs-previous-month badge), Share of All New Registrations, Top State.
* "Carrier Registrations by Day" chart — CARRIER-ONLY daily counts (this differentiates the page from the overview; data comes from the same pull).
* Top 10 States and Top 10 Cities tables (carrier-only — move from the current section).
* Cross-links: back to the June 2026 overview, sideways to the brokers page, and to the main `/statistics/new-registrations`.

Broker child page (`.../brokers`): mirror of the carrier page with broker-only data (EntityType contains B, not C). For months with zero brokers, render the page with a zero-state message ("No freight broker registrations recorded in [Month Year].") instead of empty charts — do not 404 or redirect.

## Routing, caching, SEO

* Routes: `statistics/new-registrations/{year}/{month}/carriers` and `.../brokers`, registered immediately before the existing month route. Same validation/redirect rules as the month page (completed months only, zero-padded canonical form, invalid → 301 to `/statistics/new-registrations`).
* Caching: all three pages render from the SAME cached month VM — extend `StatisticsNewRegistrationsMonthVM` with the per-type daily series and any missing per-type fields, computed from the existing single data pull (no new queries). **Bump the cache key to `NewRegistrationsMonthData_v3_`** (shape changes). One cache entry per month serves all three pages; no new prefixes, no pre-warm changes.
* SEO per child page:
  * Titles: `"New Motor Carriers — June 2026 | Truck Carrier Hub"` / `"New Freight Brokers — June 2026 | Truck Carrier Hub"`.
  * Dynamic meta descriptions (≤160 chars, C#-validated), e.g. `"[N] new motor carriers registered with FMCSA in June 2026. Top states, top cities, and daily registration trends."` — brokers equivalent.
  * Canonicals: the zero-padded child URLs, no trailing slash.
  * JSON-LD WebPage + 5-level BreadcrumbList.
* Sitemap: `sitemap_registrations.xml` now lists all three URLs per completed month.
* Landing page ("Latest Registration Activity" block): repoint link 1 to `/statistics/new-registrations/2026/06/carriers` and link 2 to `.../brokers` (computed month, never hardcoded). Link 3 (trends) unchanged.

Implementation rules:
* Reuse existing components/CSS; Bootstrap 3 grid; "updated weekly" copy; comma-formatted numbers; no new packages.
* No changes to the main `/statistics/new-registrations` page.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* All three June 2026 pages rendering with real data — overview slimmed with the two link cards, each child page with its type-only daily chart and tables.
* `<head>` excerpts for both child pages (title, meta + char count, canonical, JSON-LD).
* Redirect checks on a child URL: non-padded month, current month, junk values.
* A zero-broker early month rendering the zero-state (find one, or state that none exists).
* The updated landing links, sitemap excerpt showing 3 URLs for one month, cache key `_v3_` in code.
* Exact list of files changed.
