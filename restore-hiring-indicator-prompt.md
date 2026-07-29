Restore the hiring indicator on company list rows (city directory pages / `CompanyListPartial.cshtml`). It was lost in the City/Company redesign: each row used to show what a company is hiring for — an indication when the company is hiring, "N/A" when not.

First, study: `Views/Home/CompanyListPartial.cshtml` (current row layout: Trucks/Tractors, Drivers, USDOT, MC, phone, rating — the hiring line goes alongside these), `GetCompanyListFromFilter()` in `HomepageService.cs` (the row projection and the batched-ratings pattern — the same batching approach applies here), `Business.NowHiring`, and the company↔`DriverTypes` relationship (`CompanyDriverType` usage in `GetCompanyInformation` shows how driver-type names are read per company).

Implementation:
1. Row data: add to the list-row `CompanyVM` population (city listing path in `GetCompanyListFromFilter`):
   * `NowHiring` (left-join `Business` by USDOTNumber; false when no Business record).
   * For hiring companies only: the company's driver-type names (the positions hired for).
   * **No N+1**: after the page of ~70 rows is materialized, fetch NowHiring flags and driver types for the visible USDOT numbers in at most two batched queries (same pattern as the batched ratings fix). Do not join per row inside the paged query and do not disturb the DB-side sort/Skip/Take.
2. Markup in `CompanyListPartial.cshtml`, one line per row in the details area, matching the existing `<strong>Label : </strong>value` style:
   * IMPORTANT — "effective hiring" rule (already applied on the Company page): a company counts as hiring only when `Business.NowHiring = 1` AND it has at least one trailer type OR driver type selected (junction rows in `TransportCompany_TrailerType` / `TransportCompany_DriverType`). `NowHiring` alone with no type selections renders as NOT hiring.
   * Effectively hiring: `<strong>Hiring : </strong>Company Drivers, Owner Operators` (comma-joined driver-type names; if only trailer types are selected, show those instead) plus a small green "Now Hiring" badge next to the company name.
   * Not effectively hiring (including NowHiring with zero type selections): `<strong>Hiring : </strong>N/A`, no badge.
3. The badge: small pill using the site's `--verified-600` green per the modern-theme design system; keep it subtle (this list is dense).
4. Scope: the city listing path. If the same partial renders search results through a different service path, make sure those rows don't throw when the new fields are null — default to "N/A".

After implementing, build with zero errors, then show me:
* A city page row for a company with `NowHiring = 1` (find one via `SELECT TOP 5 USDOTNumber FROM Business WHERE NowHiring = 1`) showing the badge and driver types, and a normal row showing "Hiring : N/A".
* The "Companies Now Hiring" checkbox flow still works (it filters to hiring companies — all visible rows should then show the badge).
* Confirmation of the batched queries (show the two SQL statements) and that page load time is unchanged.
* Exact list of files changed.
