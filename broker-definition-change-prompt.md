Site-wide classification change in the statistics code: a freight broker is ANY company whose `EntityType` contains "B" — including hybrids like "B;C" that also hold carrier authority. The current "B and not C" logic undercounts brokers (it produced "0 freight brokers" on the homepage teaser). Carrier stays "contains C". Because the two now overlap, disclose the overlap wherever both counts appear together; donuts that must sum to 100% switch to explicit exclusive segments. The rule is recorded in CLAUDE.md — read it first.

Find every occurrence of the old pattern (search `Contains("B") && !` and `NOT LIKE '%C%'` variants in `HomepageService.cs`) and fix each site as follows:

1. `GetNewRegistrationsData()` (the range page `/statistics/new-registrations`):
   * `NewBrokerCount` (and `PrevBrokerCount`) = contains B. `NewMotorCarrierCount` = contains C (unchanged). Add a new VM field for the overlap: count of rows containing both B and C.
   * Stat cards: broker card gets footnote-style sub-text "[X] hold both carrier and broker authority" if X > 0 (comma-formatted).
   * The three-series chart: the Freight Brokers series = any B (recompute `MonthlyBroker`).
   * "Registrations by Company Type" donut: switch segments to Motor Carrier only / Freight Broker only / Both / Other — exclusive, sums to total. The side table shows those four rows PLUS a separate summary line under the table: "Total motor carriers (any): [N] · Total freight brokers (any): [M]".
2. `GetNewRegistrationsMonthData()` (monthly archive):
   * `BrokerCount` / `PrevBrokerCount` = any B; `MotorCarrierCount` = any C; add the both-count to the VM.
   * Month overview stat cards: same "both" sub-text treatment.
   * The brokers child page (`/{yyyy}/{MM}/brokers`) now covers any-B rows: its counts, daily chart, top states/cities all recompute from `EntityType.Contains("B")`. The carriers child page stays any-C. A hybrid company appears on both child pages — intended; add one line to each child page intro: "Includes companies that hold both carrier and broker authority." Only add it when the both-count > 0.
   * Bump the month cache key `NewRegistrationsMonthData_v3_` → `_v4_` (VM shape + semantics changed; stale cached months must not mix definitions).
3. Homepage teaser (`Views/Home/Index.cshtml`): no logic change needed once the VM is fixed, but adjust the sentence to tolerate overlap: "[T] companies registered last month — [C] motor carriers and [B] freight brokers." stays accurate since the two may exceed T; append " (some hold both authorities)" only when the both-count > 0.
4. `GetActiveBrokersData()` — already uses contains-B for the broker total; VERIFY nothing in it uses B-and-not-C except the intentional "Broker Only" vs "Broker + Carrier" split cards and the entity-types donut, which stay as they are (they disclose the split explicitly — that is the pattern the rest of the site is now adopting).
5. Landing page (`Statistics.cshtml`) and anything else reading these VMs: verify displayed numbers still bind to the right fields; the level-2 card's 12-month count is the total (unchanged).
6. Range-page cache key `NewRegistrationsData_v1_` → `_v2_` (same reason as the month bump). Both new prefixes are already covered by the registered prefix strings — verify via StartsWith, don't assume.

Implementation rules:
* Classification is exactly: broker = `EntityType != null && EntityType.Contains("B")`; carrier = contains "C"; both = contains both. Case handling: match existing code (values are stored uppercase — verify with one query before assuming).
* Do not touch the Active Companies, Fleet & Operations, Cargo, State, or City statistics pages — they don't classify brokers.
* All copy additions comma-formatted, "updated weekly" convention, no hardcoded numbers.
* Do not stage or commit line-ending-churned files.

After implementing, build with zero errors, clear the statistics cache, then show me:
* The June 2026 numbers before/after: total, carriers, brokers, both — from SQL and from the rendered pages, matching.
* The homepage teaser sentence rendered with real numbers (brokers must no longer read 0 if any-B June registrations exist).
* The reworked company-type donut with its four segments and the any-total summary line.
* Both child pages for June with the overlap sentence when applicable.
* New cache key strings and prefix coverage confirmation.
* Exact list of files changed.
