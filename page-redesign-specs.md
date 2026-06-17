# Page redesign specs — Company.cshtml and City.cshtml

These describe the layout already agreed on (mocked up and approved before the move to
Claude Code). Build against this structure rather than designing the layout from scratch;
the design system itself (colors, type, component conventions) is in CLAUDE.md.

## Company.cshtml (carrier detail page)

Top to bottom:

1. **Breadcrumb**: Home / State / City / Company name.
2. **Claim banner** — shown only when the company has no matching `Business` record (i.e.
   unclaimed, which is true for ~99.7% of carriers). An info-colored bar: "This listing hasn't
   been claimed yet. Are you the owner?" with a "Claim this business" button. This is a
   deliberate growth/content-quality lever, not just decoration, make it the first thing
   visible, not buried text like the current version.
3. **Header row**: company name as the page heading, an Active/Inactive status badge next to
   it, a "registered since [year]" line, and on the right side the existing star rating /
   review-count display (or "Be the first to write a review" if none yet).
4. **Stats grid** — four small metric cards in a row: USDOT number, MC number,
   Trucks/Tractors, Drivers. Use the `.data-tag` monospace style from the design system for
   the actual numbers.
5. **Two-column section** below the stats grid:
   - Left column (wider): "Contact & location" card. Small location visual area (a map
     embed if feasible, otherwise a simple placeholder with coordinates), then address,
     phone, email, and website as a clean label/value list.
   - Right column (narrower), stacked: a "Safety snapshot" card, and a "Reviews" card.
     - Safety snapshot: only ~2% of carriers have FMCSA safety rating data. When present,
       show it. When absent (the common case), show something like "No FMCSA safety rating
       on file. Most small and mid-size carriers aren't formally rated unless they've
       undergone a compliance review." — not an empty gap, and not alarming.
     - Reviews: existing review/star system and reply functionality, just restyled.
6. Preserve everything currently in Company.cshtml functionally: the canonical-URL sort-param
   stripping logic, the outbound banner ad slot, the claim/login flow, the review JS hooks.
   This is a visual and structural pass, not a feature rewrite.

## City.cshtml (city listing page)

Top to bottom:

1. **Breadcrumb**: Home / State / City.
2. **Header row**: "Trucking companies in [City], [ST]" as the heading, a result count below
   it, and the existing List/Map toggle on the right (restyled, same underlying function).
3. **City intro text** — the existing City Article block (admin-curated, truncated with a
   "Read more" expand). Keep this prominent; it's the most SEO-valuable content on the page.
4. **Filter bar** — same existing filters (entity type, cargo, trucks/tractors, service type,
   trailer type, driver type, now hiring, sort), restyled as a compact row of buttons rather
   than the current sprawling form. Functionality unchanged.
5. **Company list** — one card per company: name, status badge, entity type, trucks/tractors
   count, USDOT number. Same data as today, restyled to match the card pattern used elsewhere.
6. **Pagination** at the bottom.

Important bug fix to make at the same time, not a separate pass: the canonical tag currently
strips everything after `/STATE/CITY` from the path, which also strips the `?p=` pagination
parameter, so every page beyond page 1 incorrectly canonicalizes to page 1. Page number needs
to be preserved (or each paginated page should self-canonicalize) — see CLAUDE.md for the
confirmed example (Birmingham, AL, 15 pages, all currently collapsing to page 1).

## State.cshtml (state directory page)

Top to bottom:

1. **Breadcrumb**: Home / State name. Unchanged from current.
2. **Header**: `<h1>Trucking Companies in [StateName]</h1>` with a two-stat block below it
   (total cities and total companies), derived from `Model.Cities` at render time
   (`Model.Cities.Count` and `Model.Cities.Sum(c => c.CompanyCount)`). Style the stat values
   in the `.font-data` monospace style, similar in spirit to the Company.cshtml stats grid
   but with just two metrics.
3. **State article** — strip HTML from `Model.PageDescription`, truncate to 100 words, show
   "Read more / Read less" toggle using the same pattern as City.cshtml (`data-fulltext`
   attribute + click handler). Show unconditionally if the description is non-empty; there
   is no per-page article-allowed flag on the state page.
4. **Get A Quote widget** — same `IsShowOnStatePage` admin on/off flag, same
   `_GetAQuoteControl` partial, same `GetAQuoteVM` data. Visual pass only.
5. **Outbound banner** — same `IsShow` / `IsFollow` / nofollow logic, centered,
   `max-width: 100%; max-height: 200px`. Visual pass only.
6. **Most popular cities** — replace the current plain-text link list with a responsive card
   grid (3 columns desktop / 2 tablet / 1 mobile). Each card links to the city page, shows
   the city name and company count. Preserve the existing `onClick="$('#al').show();"` on
   every link.
7. **A-Z jump navigation** — same 26 letter anchors A–Z in a single flex-wrap row (no
   split into two separate divs). Style each as a compact pill button matching the
   `.dropdown-toggle` pill style already built for City.cshtml's filter bar
   (same background, border, border-radius, padding). Current `btn btn-default btn-pager`
   Bootstrap buttons replaced.
8. **A-Z city directory** — same Razor loop (`for char 'A' to 'Z'`, filter `Model.Cities`
   by starting letter). Replace `<h4 class="h2backcolor well">CITIES AND TOWNS BEGINNING
   WITH (X)</h4>` with a lightweight section header: large letter + small "Cities and towns"
   label + right-aligned back-to-top arrow. Preserve `id="@charater"` anchor on the header
   for jump-nav targeting. City links keep the same href, `onClick`, and
   `@cityName Trucking Companies (@city.CompanyCount)` text. Same 3-col / 2-col / 1-col
   responsive grid.
9. Preserve exactly: hidden field `#fromStatePage`, `.up-arrow-citypage` scroll-to-top
   click handler, and the `#al` loader-hide `setTimeout`. No canonical fix needed (page
   is not paginated). No query restructuring needed (performance already confirmed fine).
