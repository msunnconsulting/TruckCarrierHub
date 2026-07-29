Restructure the Statistics landing page (`/statistics`) into a two-level card layout, and remove the "Explore Key Statistics" section entirely. View-layer work only — the ViewModel already has every number needed; no service or cache-key changes.

First, study the current `Views/Home/Statistics.cshtml`: the top stat-card row (`.stat-cards-wrap`, currently four cards), the "Explore Key Statistics" section markup (including the "Latest Registration Activity" block with its computed-month links — that computation logic gets reused), and the `.stat-card-*` CSS.

Level 1 — top stat-card row, exactly three cards:
* Keep "Active Trucking Companies" (clickable, whole card links to `/statistics/active-companies`) and "Active Freight Brokers" (clickable → `/statistics/active-brokers`) exactly as they are.
* Keep "FMCSA Data Updates" (the date card) as the third card, non-clickable, no chevron.
* REMOVE the "New FMCSA Registrations" card from this row (it moves to level 2).
* Adjust card sizing so three cards fill the row.

Level 2 — new row directly below level 1, three topic cards. Build ONE card pattern with an optional URL (block `<a>` with hover affordance when set, plain `<div>` when null):
* Card 1 — "New FMCSA Registrations" (clickable → `/statistics/new-registrations?range=12m`, matching its number):
  * Big number: the existing 12-month count from the VM (the field the removed level-1 card displays today).
  * Description: "Track newly registered trucking companies and freight brokers."
  * Below the description, two small sub-links for the last completed month, computed at render time from `DateTime.Now` exactly like the current "Latest Registration Activity" block does (July 2026 → June 2026, rolls forward automatically, never hardcoded):
    * "New Trucking Companies — June 2026" → `/statistics/new-registrations/2026/06/carriers`
    * "New Freight Brokers — June 2026" → `/statistics/new-registrations/2026/06/brokers`
  * These sub-links must remain real `<a>` tags inside the card; if the whole card is one `<a>`, nested anchors are invalid HTML — structure the card so the header/number area is the link and the sub-links are separate anchors (no nested `<a>`).
* Card 2 — "Fleet and Operations", NOT clickable (URL = null; the page doesn't exist yet). Description: "Fleet sizes, owner-operators, power units, and drivers." No chevron, no hover affordance, no `href="#"`.
* Card 3 — "Cargo and Equipment", NOT clickable. Description: "Cargo types, commodities, hazmat, and equipment mix." Same rules.
* Visual hierarchy: level 2 cards read as navigation/topic cards, slightly different from the level-1 number cards (e.g. icon + title + description) but same design language (`.stat-card-v2` conventions, site palette).

Remove the "Explore Key Statistics" section entirely:
* The section heading, subheading, and ALL blocks — Market Rankings, Latest Registration Activity, Fleet & Operations, and any others — including every sub-link (several are `href="#"` placeholders). The monthly links live on in level-2 card 1; nothing else from the section survives.
* After removal, grep the page: zero `href="#"` anywhere, and no orphaned CSS classes left referenced only by deleted markup (remove page-local styles that are now unused; leave shared stylesheet classes alone).

Implementation rules:
* No ViewModel, service, route, or cache changes — this is Statistics.cshtml (and page-local CSS) only.
* SEO metadata on the page is untouched.
* Numbers comma-formatted from the VM; copy "updated weekly"; Bootstrap 3 grid compatibility.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, then show me:
* The rendered page: three level-1 cards (two clickable), three level-2 cards (one clickable with working month sub-links, two inert), Explore section gone.
* Grep proof: no `href="#"` remains in Statistics.cshtml.
* The two month sub-link hrefs rendered for the current date (should point at last completed month, zero-padded).
* Confirmation that no VM/service/cache files were touched.
