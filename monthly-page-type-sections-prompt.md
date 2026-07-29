Fix on the monthly registration archive pages (`/statistics/new-registrations/{year}/{month}`): the two landing-page links ("New Trucking Companies Registered in June 2026" → `#carriers`, "New Freight Brokers Registered in June 2026" → `#brokers`) currently anchor to small stat cards near the top, so both links appear to open the same page. Give each anchor a real, visually distinct section. One page per month stays — do NOT create separate carrier/broker pages.

Changes on `Views/Home/NewRegistrationsMonth.cshtml` + its VM/service:

1. REMOVE the combined "Top 10 States" and "Top 10 Cities" tables (the combined split is already on the donut, and combined tables live on the main new-registrations page).
2. ADD two full-width sections, after the donut row:
   * `id="carriers"` — section heading "New Motor Carriers in [Month Year]" with the carrier count as a headline number, then a two-column row: "Top 10 States" and "Top 10 Cities" tables computed over MOTOR CARRIERS only (EntityType contains C), with % of the month's carrier total and bar fills.
   * `id="brokers"` — same structure: "New Freight Brokers in [Month Year]", broker count headline, top states + top cities over FREIGHT BROKERS only (EntityType contains B, not C).
   * Both sections derive from the SAME single data pull the page already makes — extra in-memory groupings only, no new queries.
   * Move the `id="carriers"` / `id="brokers"` attributes off the stat cards and onto these sections; add `scroll-margin-top` (or equivalent offset) so anchored jumps don't hide the heading under the navbar.
3. The three stat cards at the top stay as they are (minus the id attributes).
4. VM shape changes → bump the cache key `NewRegistrationsMonthData_v1_` → `NewRegistrationsMonthData_v2_` (prefix in `InvalidateStatisticsCache()` matches by StartsWith, so the registered prefix still covers it — verify).
5. If a month has zero brokers (possible in early-2000s months), the brokers section renders its heading with "No freight broker registrations recorded in [Month Year]." instead of empty tables — same for carriers.
6. No changes to the landing-page links, routes, redirects, SEO metadata, or sitemap — anchors keep the same names.

After implementing, build with zero errors, then show me:
* The June 2026 page with both sections rendering distinct carrier/broker tables with real data.
* Clicking each landing-page link scrolls to the correct visible section heading (screenshot or description of anchor behavior).
* The new cache key format, and confirmation the old combined tables are gone.
* An early month (e.g. 2001/03) rendering the zero-broker fallback if applicable.
