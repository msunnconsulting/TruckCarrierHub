using System;
using System.Collections.Generic;
using System.Text;
using PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement;
using PartnerCarrier.Infrastructure.Contracts.User;
using PartnerCarrier.ViewModels.Admin;
using PartnerCarrier.ViewModels.User;

namespace PartnerCarrier.Web.Helpers
{
    /// <summary>
    /// Builds sitemap.xml + child sitemap files (pages, states, cities, registration archive)
    /// from scratch every time it runs. Shared by the admin "Generate Sitemaps" action
    /// (Areas/Admin/Controllers/BusinessController.cs) and the automatic regeneration hook in
    /// Global.asax.cs, so both call paths stay in sync and there's exactly one place that knows
    /// the sitemap file names / URL structure.
    ///
    /// Only covers directory pages with 50+ active companies for cities (see
    /// IBusinessMangementService.GetAllCities) to keep the city sitemap from ballooning to the
    /// ~30,000-city size the "City articles" work (CLAUDE.md "What's next" #4) will eventually
    /// need; individual carrier/company pages are intentionally NOT included here (~1.3M pages,
    /// templated/thin at scale per the July 2026 Search Console review).
    /// </summary>
    public static class SitemapGenerator
    {
        private const string BaseUrl = "https://truckcarrierhub.com";

        public class Result
        {
            public int TotalUrlCount { get; set; }
        }

        /// <param name="homepageService">Used for GetAllStates() and GetEarliestRegistrationYearMonth().</param>
        /// <param name="businessMangementService">Used for GetAllCities(50, 0) - active companies only.</param>
        /// <param name="mapPath">Resolves a virtual path ("~/sitemap.xml") to a physical path.
        /// Pass Controller.Server.MapPath from a controller, or HostingEnvironment.MapPath from
        /// Global.asax/background code - keeps this class independent of either context.</param>
        public static Result GenerateAll(
            IHomepageService homepageService,
            IBusinessMangementService businessMangementService,
            Func<string, string> mapPath)
        {
            var now = DateTime.Today;
            string genDate = now.ToString("yyyy-MM-dd");
            int totalCount = 0;

            // 1) Homepage + core statistics section pages
            // Priority reflects click-depth from the homepage consistently across all four
            // sitemaps: 1.0 home, 0.9 one click away (/statistics, /{StateCode}), 0.8 two
            // clicks (/statistics/{report}, /{StateCode}/{City}), 0.7 three clicks
            // (/statistics/state/{Code}), 0.6 four clicks (/statistics/city/{Code}/{City},
            // monthly registration archive), 0.5 five clicks (registration carriers/brokers).
            var corePages = new[]
            {
                Tuple.Create(BaseUrl + "/", "1.0"),
                Tuple.Create(BaseUrl + "/statistics", "0.9"),
                Tuple.Create(BaseUrl + "/interactive-map", "0.9"),
                Tuple.Create(BaseUrl + "/reviews", "0.9"),
                Tuple.Create(BaseUrl + "/truck-driver-jobs", "0.9"),
                Tuple.Create(BaseUrl + "/statistics/active-companies", "0.8"),
                Tuple.Create(BaseUrl + "/statistics/active-brokers", "0.8"),
                Tuple.Create(BaseUrl + "/statistics/fleet-operations", "0.8"),
                Tuple.Create(BaseUrl + "/statistics/cargo", "0.8"),
                Tuple.Create(BaseUrl + "/statistics/new-registrations", "0.8"),
            };
            var pagesSb = StartUrlset();
            foreach (var p in corePages)
                AppendUrl(pagesSb, p.Item1, genDate, "weekly", p.Item2);
            EndUrlset(pagesSb);
            totalCount += corePages.Length;
            WriteFile(mapPath, "~/sitemap_pages.xml", pagesSb);

            // 2) State directory pages (/{StateCode}) + state statistics pages
            // (/statistics/state/{StateCode}) - same GetAllStates() list covers both, since
            // StateCompanies() renders for every state with no extra gating.
            var states = homepageService.GetAllStates() ?? new List<StateVM>();
            var statesSb = StartUrlset();
            int stateCount = 0;
            foreach (var s in states)
            {
                if (string.IsNullOrEmpty(s.StateCode)) continue;
                AppendUrl(statesSb, BaseUrl + "/" + s.StateCode, genDate, "weekly", "0.9");
                AppendUrl(statesSb, BaseUrl + "/statistics/state/" + s.StateCode, genDate, "weekly", "0.7");
                stateCount += 2;
            }
            EndUrlset(statesSb);
            totalCount += stateCount;
            WriteFile(mapPath, "~/sitemap_states.xml", statesSb);

            // 3) City directory pages (/{StateCode}/{CityName}) + city statistics pages
            // (/statistics/city/{StateCode}/{CityName}), both limited to cities with 50+
            // active companies - CityCompanies() returns null (redirects) below that same
            // threshold, so this list is exactly right for both URL shapes. CityURL is
            // already "{StateCode}/{CityName-with-dashes}", which is also the exact path
            // segment the statistics route expects.
            var cities = businessMangementService.GetAllCities(50, 0) ?? new List<ManageCityListVM>();
            var citiesSb = StartUrlset();
            int cityCount = 0;
            foreach (var c in cities)
            {
                if (string.IsNullOrEmpty(c.CityURL)) continue;
                AppendUrl(citiesSb, BaseUrl + "/" + c.CityURL, genDate, "weekly", "0.8");
                AppendUrl(citiesSb, BaseUrl + "/statistics/city/" + c.CityURL, genDate, "weekly", "0.6");
                cityCount += 2;
            }
            EndUrlset(citiesSb);
            totalCount += cityCount;
            WriteFile(mapPath, "~/sitemap_cities.xml", citiesSb);

            // 4) Monthly new-registrations archive (existing logic, unchanged behavior)
            int earliestYM = homepageService.GetEarliestRegistrationYearMonth();
            var lastComplete = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            int lastYM = lastComplete.Year * 100 + lastComplete.Month;
            var regSb = StartUrlset();
            int regCount = 0;
            int curYM = earliestYM;
            while (curYM <= lastYM)
            {
                int yr = curYM / 100;
                int mo = curYM % 100;
                string moStr = mo.ToString("D2");
                string monthUrl = BaseUrl + "/statistics/new-registrations/" + yr + "/" + moStr;
                AppendUrl(regSb, monthUrl, genDate, "monthly", "0.6");
                AppendUrl(regSb, monthUrl + "/carriers", genDate, "monthly", "0.5");
                AppendUrl(regSb, monthUrl + "/brokers", genDate, "monthly", "0.5");
                regCount += 3;
                mo++;
                if (mo > 12) { mo = 1; yr++; }
                curYM = yr * 100 + mo;
            }
            EndUrlset(regSb);
            totalCount += regCount;
            WriteFile(mapPath, "~/sitemap_registrations.xml", regSb);

            // 5) Master sitemap index referencing all four child sitemaps
            var idx = new StringBuilder();
            idx.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            idx.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (var file in new[] { "sitemap_pages.xml", "sitemap_states.xml", "sitemap_cities.xml", "sitemap_registrations.xml" })
            {
                idx.Append("  <sitemap>\n    <loc>").Append(BaseUrl).Append("/").Append(file);
                idx.Append("</loc>\n    <lastmod>").Append(genDate).Append("</lastmod>\n  </sitemap>\n");
            }
            idx.AppendLine("</sitemapindex>");
            WriteFile(mapPath, "~/sitemap.xml", idx);

            return new Result { TotalUrlCount = totalCount };
        }

        private static StringBuilder StartUrlset()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            return sb;
        }

        private static void EndUrlset(StringBuilder sb)
        {
            sb.AppendLine("</urlset>");
        }

        private static void AppendUrl(StringBuilder sb, string url, string lastmod, string changefreq, string priority)
        {
            sb.Append("  <url>\n    <loc>").Append(XmlEscape(url));
            sb.Append("</loc>\n    <lastmod>").Append(lastmod);
            sb.Append("</lastmod>\n    <changefreq>").Append(changefreq);
            sb.Append("</changefreq>\n    <priority>").Append(priority);
            sb.Append("</priority>\n  </url>\n");
        }

        private static string XmlEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static void WriteFile(Func<string, string> mapPath, string virtualPath, StringBuilder sb)
        {
            string physicalPath = mapPath(virtualPath);
            System.IO.File.WriteAllText(physicalPath, sb.ToString(), Encoding.UTF8);
        }
    }
}
