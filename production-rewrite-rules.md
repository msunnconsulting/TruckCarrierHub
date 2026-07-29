# Production web.config — hand-maintained rewrite rules (DO NOT LOSE ON DEPLOY)

The production server's `Web.config` contains a `<rewrite>` block inside `<system.webServer>`
that exists ONLY on the server — it is NOT in the repo's Web.config or any transform file
(localhost must not redirect to https, so it stays out of the dev config).

**When deploying: never overwrite the production web.config wholesale. Merge changes, or
re-apply this block after deploy and verify the redirects (test URLs below).**

The full block as deployed July 10, 2026:

```xml
<rewrite>
  <rules>
    <!--  <rule name="Enforce canonical hostname" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTP_HOST}" negate="true" pattern="^www\.truckcarrierhub\.com$" />
      </conditions>
      <action type="Redirect" url="https://truckcarrierhub.com/{R:0}" redirectType="Permanent" />
    </rule>-->
    <rule name="Redirect legacy partnercarrier.com" stopProcessing="true">
      <match url="^(.*)$" />
      <conditions>
        <add input="{HTTP_HOST}" pattern="^(www\.)?partnercarrier\.com$" />
      </conditions>
      <action type="Redirect" url="https://truckcarrierhub.com/{R:1}" redirectType="Permanent" />
    </rule>
    <rule name="HTTP to HTTPS redirect" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="off" ignoreCase="true" />
      </conditions>
      <action type="Redirect" redirectType="Permanent" url="https://{HTTP_HOST}/{R:1}" />
    </rule>
    <rule name="site" stopProcessing="true">
      <match url="^(.*)$" />
      <conditions>
        <add input="{HTTP_HOST}" pattern="^www\.truckcarrierhub\.com$" />
      </conditions>
      <action type="Redirect" url="https://truckcarrierhub.com/{R:1}" redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
```

Why the legacy rule matters: partnercarrier.com (the pre-rebrand domain) served a full
duplicate of the site for ~4 years via an expired hosting account. Its DNS now points at the
production server; the IIS site has bindings for partnercarrier.com + www (ports 80 and 443,
Let's Encrypt cert via win-acme, auto-renewing). This rule turns all of that traffic into
single-hop 301s. Removing it resurrects the duplicate site instantly.

Post-deploy verification (all must 301 in ONE hop to the matching truckcarrierhub.com URL):
- http://partnercarrier.com/PA/USDOT-359682
- https://partnercarrier.com/PA/USDOT-359682
- http://www.partnercarrier.com/

---

# Web.config differences — local vs production (merge checklist)

Local and production Web.config are DIFFERENT FILES maintained separately. Never copy one
over the other. At every deploy, reconcile using this list:

**Must be carried local → production (added July 2026):**
- `<system.web>` → `<globalization fileEncoding="utf-8" requestEncoding="utf-8"
  responseEncoding="utf-8"/>` — fixes mojibake (â€“) from BOM-less UTF-8 .cshtml files that
  Claude Code creates. Without it, en dashes and ✓ marks garble on production.

**Production-only, must SURVIVE every deploy (never in local):**
- The full `<rewrite>` block above (HTTPS enforcement, www-stripping, partnercarrier.com
  legacy 301). Removing it resurrects the duplicate old-domain site.
- `<compilation debug="false">` — local runs debug="true"; production must not
  (performance, bundling, timeout behavior).

**Environment-specific, different by design (reconcile, don't sync):**
- Connection strings (local: NEWHP16INCH\MSSQLSERVER1 / PartnerCarrier_New; production: its
  own SQL instance).
- Any appSettings that differ (SMTP, API keys, site URL, reCAPTCHA, etc.) — production values
  win on production.

After every deploy run the three redirect test URLs above, plus load one statistics page and
one directory city page containing an en dash to confirm the globalization line survived.
