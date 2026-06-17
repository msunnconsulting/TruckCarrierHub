# Truck Carrier Hub — project context

## What this is
A directory of 1.3M+ US/Canada trucking companies (truckcarrierhub.com), built on FMCSA public
data. Features: company/city/state listings, interactive map, job postings, reviews, freight
quote requests ("Get a Quote").

## Stack
- ASP.NET MVC 5 on .NET Framework 4.6.1 — not .NET Core, despite using Razor views.
- Mixed data access: hand-rolled ADO.NET in most of the codebase (Common.Utility/ADO.DAL),
  but EF6 (`db.Database.SqlQuery<T>` and some LINQ) in HomepageService.cs specifically.
- SQL Server (Express locally). Database is internally named "PartnerCarrier_New" —
  that's the site's original name before it was rebranded to Truck Carrier Hub. Same for
  several namespaces (`PartnerCarrier.Web.*`). This is intentional debt: leave it alone
  unless a much bigger refactor is already underway, the rename touches too many connection
  strings for the benefit it gives.
- Solution projects: Common.Utility, TruckCarrierHub.Database (SSDT, raw table scripts, no EF
  migrations), TruckCarrierHub.Infrastructure, TruckCarrierHub.ViewModels, TruckCarrierHub.Web.

## Gotchas — confirm exact path before editing anything
- `_Layout.cshtml` exists in 3 places. Only one is live:
  - `TruckCarrierHub.Web/Themes/Bootstrap/Views/Shared/_Layout.cshtml` — ACTIVE, used by `_ViewStart.cshtml`.
  - `TruckCarrierHub.Web/Views/Shared/_Layout.cshtml` — dead, entirely commented out, ignore it.
  - `Areas/Admin/Views/Shared/_Layout.cshtml` — separate admin area layout, unrelated.
- `Index.cshtml` exists in `Views/Home/` (the real homepage) and 3 more times under `Areas/Admin`.
- SQL Server Express has no SQL Agent (scheduled jobs need an external trigger, e.g. Windows
  Task Scheduler calling a console app, not a SQL job) and a 10GB per-database cap.

## Design system — Content/modern-theme.css
Loaded from the shared layout, so it applies site-wide. Deliberately keeps Bootstrap 3's grid
(`.row`, `.col-md-*`) intact and only restyles chrome/components, since most pages still depend
on that grid and a full Bootstrap version upgrade was never in scope.
- Colors: `--ink-900` (#0F2236, navy — primary/nav), `--signal-500` (#D98F2B, amber — the one
  accent color), `--verified-600` (green — active/trustworthy status), `--paper-50` (warm
  off-white background), `--steel-*` (borders, secondary text).
- Type: Archivo for headings, IBM Plex Sans for body, IBM Plex Mono via the `.data-tag` class
  for official record numbers (USDOT, MC, phone, zip) — meant to read like a stamped manifest.
- Hard-won lesson from this file already: when a button's active/inactive state is toggled by
  JS adding/removing one class (e.g. `btn-primary`) while a base class (`btn-default`) never
  gets removed, do NOT write a separate CSS rule for the base class on that same element — it
  ties in specificity with the active-state rule and wins by source order, silently cancelling
  it. This exact bug happened twice on the City page's List/Map toggle.

## Already done
- `_Layout.cshtml` redesigned (navbar, footer, search bar) with the new design system.
- Homepage `HomeArticle` feature: new `Admin.HomeArticle` column wired through
  `IHomepageService.cs` → `HomepageService.cs` → `StateVM.cs` → `HomeController.cs` →
  `Index.cshtml`, rendered in its own block, separate from the existing `PageDescription`
  field. Don't merge those two: `PageDescription` is also reused for the homepage's
  `<meta name="description">` tag and contains a dynamically-built "Truck Driver Jobs" count
  with an embedded popover link, it has to stay structurally distinct.
- Several rounds of CSS bugs already fixed in modern-theme.css: font-family not overriding
  legacy styles, buttons staying colored after a click (`:focus` vs `:focus-visible`), the
  List/Map toggle conflict above, and the homepage text inheriting the wrong font/size from
  its wrapping `<h2>` (used purely for SEO weight, not as a real heading).

## Navigation / search conventions
- **Never glob or read under `\obj`, `\bin`, or `.vs\`** — those are build/IDE artefacts. All real source is under the five named project folders and `packages\`.
- `CompanyInformationVM` (and all other Company + review ViewModels) live in `TruckCarrierHub.ViewModels\User\CompanyVM.cs`. Go there directly; don't search for it.

## What's next, roughly in order
1. **Company.cshtml redesign** (carrier detail page). Preserve all existing functionality:
   claim-listing flow, review/star rating system, the canonical-URL sort-param stripping logic,
   USDOT/MC/truck/driver stats, the outbound banner ad slot. Add a "safety snapshot" section
   with a graceful empty state, only ~2% of carriers have FMCSA safety rating data, so this
   can't assume the data exists.
2. **City.cshtml redesign**. Same approach, plus a real bug fix: the canonical tag currently
   strips everything after `/STATE/CITY` from the URL path, including the `?p=` pagination
   parameter, so every paginated page (page 2, 3, ...) incorrectly declares itself a duplicate
   of page 1 even though it shows entirely different companies. Confirmed live on
   `truckcarrierhub.com/AL/BIRMINGHAM` (15 pages, all canonicalizing to page 1).
3. **FMCSA Socrata sync service** — a separate console/worker project, not inside
   TruckCarrierHub.Web. Pulls from data.transportation.gov (Socrata SoQL API; call the REST
   endpoints directly with HttpClient rather than the abandoned SODA.NET SDK, last released
   2019). Must include the inactive-company scrub as part of the same update that flips
   `Status` to `'I'`: null out physical/mailing address, office phone, cell phone, fax, and
   zero out Latitude/Longitude. This is currently done by hand with a SQL script; it needs to
   become automatic.
4. **City articles** — first cross-check existing curated articles (e.g. Birmingham, AL) for
   unverified factual claims about specific named companies. Longer term: auto-generate for
   roughly 30,000 cities, grounded in real per-city stats (carrier count, cargo/entity-type
   mix, fleet size) rather than generic prose, with a monthly regeneration option triggered
   from the Admin interface.

## Known SEO issues (from Search Console, not yet addressed in code)
- ~128,700 pages "Crawled – currently not indexed": templated/duplicate content at scale is
  the root cause, not a technical block, Google crawls everything, it just doesn't think most
  individual carrier pages are differentiated enough to index.
- 868 pages "Duplicate without user-selected canonical": largely the City pagination bug above.
- Homepage meta description currently contains leaked HTML (a popover-trigger `<a>` tag gets
  embedded via `GetPageDescription`'s "Truck Driver Jobs" replacement logic) and needs to be
  trimmed to a short plain-text summary, separate from the on-page version.
