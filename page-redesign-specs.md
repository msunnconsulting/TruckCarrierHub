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
