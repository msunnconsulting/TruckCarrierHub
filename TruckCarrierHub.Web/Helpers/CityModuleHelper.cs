using System;
using System.Collections.Generic;
using System.Linq;
using PartnerCarrier.ViewModels.Admin;

namespace PartnerCarrier.Web.Helpers
{
    public static class CityModuleHelper
    {
        // City VM cargo names that differ from the national CgCargoTypeRow labels
        private static readonly Dictionary<string, string> CargoLabelMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Garbage & Refuse",     "Garbage/Refuse/Trash" },
                { "US Mail",              "U.S. Mail" },
                { "Logs, Poles & Lumber", "Logs/Poles/Beams/Lumber" },
                { "Grain, Feed & Hay",    "Grain/Feed/Hay" },
                { "Coal & Coke",          "Coal/Coke" }
            };

        // Returns up to 3 highlight sentences, ranked by extremeness.
        // Any null baseline → that metric's highlights are skipped, no exception.
        public static List<string> BuildHighlights(
            StatisticsCityCompaniesVM city,
            StatisticsCargoVM cargoBaseline,
            StatisticsIndexVM statsBaseline,
            StatisticsFleetOperationsVM fleetBaseline,
            StatisticsNewRegistrationsVM newRegBaseline)
        {
            if (city == null) { return new List<string>(); }

            // Deterministic template variant per city name
            int hash = 0;
            if (city.CityName != null)
            {
                foreach (char c in city.CityName) { hash = hash * 31 + c; }
            }
            hash = Math.Abs(hash);

            // (extremeness, sentence) — higher extremeness = ranked first
            var candidates = new List<Tuple<double, string>>();

            // ── a. Cargo type shares ──────────────────────────────────────────────
            if (cargoBaseline != null
                && cargoBaseline.AllCargoTypes != null
                && cargoBaseline.TotalActiveCompanies > 0
                && city.TopCargoTypes != null
                && city.TopCargoTypes.Count > 0
                && city.TopCargoTypes[0].CompanyCount >= 30)
            {
                var natDict = cargoBaseline.AllCargoTypes
                    .ToDictionary(x => x.Label, x => x.CompanyCount, StringComparer.OrdinalIgnoreCase);

                foreach (var ct in city.TopCargoTypes.Take(5))
                {
                    string natKey = ct.CargoTypeName;
                    if (CargoLabelMap.ContainsKey(natKey)) { natKey = CargoLabelMap[natKey]; }
                    if (!natDict.ContainsKey(natKey)) { continue; }

                    decimal cityPct = ct.PercentOfTotal;
                    decimal natPct  = Math.Round((decimal)natDict[natKey] / cargoBaseline.TotalActiveCompanies * 100m, 1);
                    if (natPct <= 0m) { continue; }

                    double ratio = (double)(cityPct / natPct);
                    if (ratio >= 2.0 || (ratio > 0 && ratio <= 0.5))
                    {
                        double ext = ratio >= 1 ? ratio : 1.0 / ratio;
                        candidates.Add(Tuple.Create(ext, CargoSentence(ct.CargoTypeName, cityPct, natPct, ratio, hash)));
                    }
                }
            }

            // ── b. Owner-operator % ───────────────────────────────────────────────
            if (city.SizeDistribution != null && city.SizeDistribution.TotalReporting >= 20)
            {
                decimal cityOO = city.OwnerOperatorPercent;

                // vs state (already in city VM)
                if (city.StateOwnerOperatorPercent > 0m)
                {
                    double r = (double)(cityOO / city.StateOwnerOperatorPercent);
                    if (r >= 2.0 || (r > 0 && r <= 0.5))
                    {
                        double ext = r >= 1 ? r : 1.0 / r;
                        candidates.Add(Tuple.Create(ext, OOSentence(cityOO, city.StateOwnerOperatorPercent, r, hash, isState: true)));
                    }
                }

                // vs national
                if (fleetBaseline != null && fleetBaseline.ReportingCount > 0)
                {
                    decimal natOO = Math.Round((decimal)fleetBaseline.OwnerOperatorCount / fleetBaseline.ReportingCount * 100m, 1);
                    if (natOO > 0m)
                    {
                        double r = (double)(cityOO / natOO);
                        if (r >= 2.0 || (r > 0 && r <= 0.5))
                        {
                            double ext = r >= 1 ? r : 1.0 / r;
                            candidates.Add(Tuple.Create(ext, OOSentence(cityOO, natOO, r, hash, isState: false)));
                        }
                    }
                }
            }

            // ── c. Large-fleet share (21+ units) ─────────────────────────────────
            if (city.SizeDistribution != null
                && city.SizeDistribution.TotalReporting >= 20
                && fleetBaseline != null
                && fleetBaseline.FleetBuckets != null
                && fleetBaseline.FleetBuckets.Count >= 5
                && fleetBaseline.ReportingCount > 0)
            {
                int cityLarge = city.SizeDistribution.TwentyOneToHundred + city.SizeDistribution.OverHundred;
                decimal cityLargePct = Math.Round((decimal)cityLarge / city.SizeDistribution.TotalReporting * 100m, 1);
                // FleetBuckets[3] = "Large Fleet (21-100)", [4] = "Very Large Fleet (101+)"
                int natLarge = fleetBaseline.FleetBuckets[3].CompanyCount + fleetBaseline.FleetBuckets[4].CompanyCount;
                decimal natLargePct = Math.Round((decimal)natLarge / fleetBaseline.ReportingCount * 100m, 1);

                if (natLargePct > 0m)
                {
                    double r = (double)(cityLargePct / natLargePct);
                    if (r >= 2.0 || (r > 0 && r <= 0.5))
                    {
                        double ext = r >= 1 ? r : 1.0 / r;
                        candidates.Add(Tuple.Create(ext, LargeFleetSentence(cityLargePct, natLargePct, r, hash)));
                    }
                }
            }

            // ── d. Broker share ───────────────────────────────────────────────────
            if (city.TotalActiveCompanies >= 50
                && city.TopAuthorityTypes != null
                && statsBaseline != null
                && (statsBaseline.TotalCompanies + statsBaseline.PureBrokerCount) > 0)
            {
                var brokerEntry = city.TopAuthorityTypes.FirstOrDefault(a => a.AuthorityTypeName == "Broker");
                int cityBrokerCount = brokerEntry != null ? brokerEntry.CompanyCount : 0;
                decimal cityBrokerPct = Math.Round((decimal)cityBrokerCount / city.TotalActiveCompanies * 100m, 1);

                int natTotal = statsBaseline.TotalCompanies + statsBaseline.PureBrokerCount;
                decimal natBrokerPct = Math.Round((decimal)statsBaseline.ActiveBrokers / natTotal * 100m, 1);

                if (natBrokerPct > 0m)
                {
                    if (cityBrokerPct > 0m)
                    {
                        double r = (double)(cityBrokerPct / natBrokerPct);
                        if (r >= 2.0 || r <= 0.5)
                        {
                            double ext = r >= 1 ? r : 1.0 / r;
                            candidates.Add(Tuple.Create(ext, BrokerSentence(cityBrokerPct, natBrokerPct, r, hash)));
                        }
                    }
                    else
                    {
                        // 0 brokers vs non-zero national — always extreme
                        string sentence = "Only " + F1(cityBrokerPct) + "% of companies here hold freight broker authority — a fraction of the national " + F1(natBrokerPct) + "% share.";
                        candidates.Add(Tuple.Create((double)natBrokerPct, sentence));
                    }
                }
            }

            // ── e. Registration growth ────────────────────────────────────────────
            if (city.NewRegistrationsYoYPercent.HasValue
                && newRegBaseline != null
                && newRegBaseline.PrevTotalCount > 0)
            {
                decimal cityGrowth = city.NewRegistrationsYoYPercent.Value;
                decimal natGrowth  = Math.Round(
                    (decimal)(newRegBaseline.TotalNewCount - newRegBaseline.PrevTotalCount)
                    / newRegBaseline.PrevTotalCount * 100m, 1);
                decimal diff = cityGrowth - natGrowth;

                if (Math.Abs(diff) >= 15m)
                {
                    candidates.Add(Tuple.Create((double)Math.Abs(diff), GrowthSentence(cityGrowth, natGrowth, hash)));
                }
            }

            return candidates
                .OrderByDescending(x => x.Item1)
                .Take(3)
                .Select(x => x.Item2)
                .ToList();
        }

        // ── Sentence builders ─────────────────────────────────────────────────────

        private static string F1(decimal v)
        {
            return v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string MultStr(double r)
        {
            double m = r >= 1 ? r : 1.0 / r;
            return m.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "×";
        }

        private static string CargoSentence(string name, decimal city, decimal nat, double r, int hash)
        {
            if (r >= 2.0)
            {
                switch (hash % 3)
                {
                    case 0:  return name + " haulers make up " + F1(city) + "% of classified carriers here — " + MultStr(r) + " the U.S. share.";
                    case 1:  return F1(city) + "% of local carriers handle " + name + ", vs. " + F1(nat) + "% nationally — a " + MultStr(r) + " concentration.";
                    default: return "About " + F1(city) + "% of companies here specialize in " + name + ", roughly " + MultStr(r) + " the national average of " + F1(nat) + "%.";
                }
            }
            else
            {
                switch (hash % 2)
                {
                    case 0:  return "Only " + F1(city) + "% of carriers here haul " + name + ", below the U.S. share of " + F1(nat) + "%.";
                    default: return name + " represents just " + F1(city) + "% of local cargo activity, vs. the U.S. average of " + F1(nat) + "%.";
                }
            }
        }

        private static string OOSentence(decimal city, decimal baseline, double r, int hash, bool isState)
        {
            string scopeWord  = isState ? "statewide"    : "nationally";
            string scopeNoun  = isState ? "state"        : "national";

            if (r >= 2.0)
            {
                switch (hash % 3)
                {
                    case 0:  return F1(city) + "% of reporting companies here are owner-operators — " + MultStr(r) + " the " + scopeNoun + " average of " + F1(baseline) + "%.";
                    case 1:  return "Owner-operators dominate: " + F1(city) + "% of local fleets are single-truck operations, vs. " + F1(baseline) + "% " + scopeWord + ".";
                    default: return "Nearly " + F1(city) + "% of carriers here run a single truck, well above the " + scopeNoun + " norm of " + F1(baseline) + "%.";
                }
            }
            else
            {
                switch (hash % 2)
                {
                    case 0:  return "Owner-operators are relatively rare here at " + F1(city) + "%, compared to " + F1(baseline) + "% " + scopeWord + ".";
                    default: return "Only " + F1(city) + "% of reporting carriers here are single-truck operators, below the " + scopeNoun + " average of " + F1(baseline) + "%.";
                }
            }
        }

        private static string LargeFleetSentence(decimal city, decimal nat, double r, int hash)
        {
            if (r >= 2.0)
            {
                switch (hash % 2)
                {
                    case 0:  return F1(city) + "% of reporting carriers here run fleets of 21+ trucks — " + MultStr(r) + " the national share of " + F1(nat) + "%.";
                    default: return "Large-fleet operators (21+ units) make up " + F1(city) + "% of local carriers, vs. " + F1(nat) + "% nationally.";
                }
            }
            else
            {
                return "Large fleets (21+ trucks) account for just " + F1(city) + "% of local carriers, vs. " + F1(nat) + "% nationally.";
            }
        }

        private static string BrokerSentence(decimal city, decimal nat, double r, int hash)
        {
            if (r >= 2.0)
            {
                switch (hash % 2)
                {
                    case 0:  return F1(city) + "% of companies here hold freight broker authority — " + MultStr(r) + " the national share of " + F1(nat) + "%.";
                    default: return "Freight brokers represent " + F1(city) + "% of local businesses, vs. " + F1(nat) + "% nationally — a " + MultStr(r) + " concentration.";
                }
            }
            else
            {
                switch (hash % 2)
                {
                    case 0:  return "Only " + F1(city) + "% of companies here hold freight broker authority — a fraction of the national " + F1(nat) + "% share.";
                    default: return "Freight brokers account for just " + F1(city) + "% of the market here, well below the " + F1(nat) + "% national share.";
                }
            }
        }

        private static string GrowthSentence(decimal city, decimal nat, int hash)
        {
            if (city > nat)
            {
                switch (hash % 2)
                {
                    case 0:  return "Registrations grew " + F1(city) + "% year over year, outpacing the national rate of " + F1(nat) + "%.";
                    default: return "New carrier registrations are up " + F1(city) + "% year over year — vs. " + F1(nat) + "% nationally.";
                }
            }
            else
            {
                return "Registration growth slowed to " + F1(city) + "% year over year, compared to " + F1(nat) + "% nationally.";
            }
        }
    }
}
