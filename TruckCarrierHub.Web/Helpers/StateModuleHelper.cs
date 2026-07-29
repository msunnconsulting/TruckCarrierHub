using System;
using System.Collections.Generic;
using System.Linq;
using PartnerCarrier.ViewModels.Admin;

namespace PartnerCarrier.Web.Helpers
{
    public static class StateModuleHelper
    {
        // Returns up to 2 highlight sentences, ranked by extremeness.
        // Any null baseline → that metric's highlights are skipped, no exception.
        public static List<string> BuildHighlights(
            StatisticsStateCompaniesVM state,
            StatisticsFleetOperationsVM fleetBaseline,
            StatisticsNewRegistrationsVM newRegBaseline)
        {
            if (state == null) { return new List<string>(); }

            // Deterministic template variant per state code
            int hash = 0;
            if (state.StateCode != null)
            {
                foreach (char c in state.StateCode) { hash = hash * 31 + c; }
            }
            hash = Math.Abs(hash);

            var candidates = new List<Tuple<double, string>>();

            var sd        = state.SizeDistribution;
            int reporting = sd != null ? sd.TotalReporting : 0;

            // ── a. Owner-operator share (1-unit carriers / reporting) ─────────
            if (sd != null && reporting >= 30 && fleetBaseline != null && fleetBaseline.ReportingCount > 0)
            {
                decimal stateOO = Math.Round((decimal)sd.OneUnit / reporting * 100m, 1);
                decimal natOO   = Math.Round((decimal)fleetBaseline.OwnerOperatorCount / fleetBaseline.ReportingCount * 100m, 1);
                if (natOO > 0m)
                {
                    double r = (double)(stateOO / natOO);
                    if (r >= 1.5 || (r > 0 && r <= 0.67))
                    {
                        double ext = r >= 1 ? r : 1.0 / r;
                        candidates.Add(Tuple.Create(ext, OOSentence(stateOO, natOO, r, hash)));
                    }
                }
            }

            // ── b. Large-fleet share (21+ units / reporting) ─────────────────
            if (sd != null && reporting >= 30
                && fleetBaseline != null
                && fleetBaseline.FleetBuckets != null && fleetBaseline.FleetBuckets.Count >= 5
                && fleetBaseline.ReportingCount > 0)
            {
                int stateLarge    = sd.TwentyOneToHundred + sd.OverHundred;
                decimal stateLargePct = Math.Round((decimal)stateLarge / reporting * 100m, 1);
                int natLarge      = fleetBaseline.FleetBuckets[3].CompanyCount + fleetBaseline.FleetBuckets[4].CompanyCount;
                decimal natLargePct   = Math.Round((decimal)natLarge / fleetBaseline.ReportingCount * 100m, 1);
                if (natLargePct > 0m)
                {
                    double r = (double)(stateLargePct / natLargePct);
                    if (r >= 1.5 || (r > 0 && r <= 0.67))
                    {
                        double ext = r >= 1 ? r : 1.0 / r;
                        candidates.Add(Tuple.Create(ext, LargeFleetSentence(stateLargePct, natLargePct, r, hash)));
                    }
                }
            }

            // ── c. Average fleet size (total PU / reporting) ─────────────────
            if (sd != null && reporting >= 30 && state.TotalPowerUnits > 0
                && fleetBaseline != null && fleetBaseline.ReportingCount > 0 && fleetBaseline.TotalPowerUnits > 0)
            {
                decimal stateAvg = Math.Round((decimal)state.TotalPowerUnits / reporting, 2);
                decimal natAvg   = Math.Round((decimal)fleetBaseline.TotalPowerUnits / fleetBaseline.ReportingCount, 2);
                if (natAvg > 0m)
                {
                    double r = (double)(stateAvg / natAvg);
                    if (r >= 1.5 || (r > 0 && r <= 0.67))
                    {
                        double ext = r >= 1 ? r : 1.0 / r;
                        candidates.Add(Tuple.Create(ext, AvgFleetSentence(stateAvg, natAvg, r, hash)));
                    }
                }
            }

            // ── d. Registration growth (last 12 vs prior 12 months) ──────────
            if (state.NewPriorRegistrations12 > 0
                && newRegBaseline != null && newRegBaseline.PrevTotalCount > 0)
            {
                decimal stateGrowth = Math.Round(
                    (decimal)(state.NewRegistrations12 - state.NewPriorRegistrations12)
                    / state.NewPriorRegistrations12 * 100m, 1);
                decimal natGrowth = Math.Round(
                    (decimal)(newRegBaseline.TotalNewCount - newRegBaseline.PrevTotalCount)
                    / newRegBaseline.PrevTotalCount * 100m, 1);
                decimal diff = stateGrowth - natGrowth;
                if (Math.Abs(diff) >= 10m)
                {
                    candidates.Add(Tuple.Create((double)Math.Abs(diff), GrowthSentence(stateGrowth, natGrowth, hash)));
                }
            }

            return candidates
                .OrderByDescending(x => x.Item1)
                .Take(2)
                .Select(x => x.Item2)
                .ToList();
        }

        private static string F1(decimal v)
        {
            return v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string MultStr(double r)
        {
            double m = r >= 1 ? r : 1.0 / r;
            return m.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "×";
        }

        private static string OOSentence(decimal state, decimal nat, double r, int hash)
        {
            if (r >= 1.5)
            {
                switch (hash % 2)
                {
                    case 0:  return F1(state) + "% of reporting carriers here are owner-operators — " + MultStr(r) + " the U.S. average of " + F1(nat) + "%.";
                    default: return "Owner-operators account for " + F1(state) + "% of reporting fleets, vs. " + F1(nat) + "% nationally.";
                }
            }
            else
            {
                switch (hash % 2)
                {
                    case 0:  return "Owner-operators are less common here at " + F1(state) + "%, compared to " + F1(nat) + "% nationally.";
                    default: return "Only " + F1(state) + "% of reporting carriers run a single truck, below the U.S. average of " + F1(nat) + "%.";
                }
            }
        }

        private static string LargeFleetSentence(decimal state, decimal nat, double r, int hash)
        {
            if (r >= 1.5)
            {
                switch (hash % 2)
                {
                    case 0:  return F1(state) + "% of reporting carriers run fleets of 21+ trucks — " + MultStr(r) + " the national share of " + F1(nat) + "%.";
                    default: return "Large-fleet operators (21+ units) make up " + F1(state) + "% of carriers here, vs. " + F1(nat) + "% nationally.";
                }
            }
            else
            {
                return "Large fleets (21+ trucks) account for just " + F1(state) + "% of carriers here, vs. " + F1(nat) + "% nationally.";
            }
        }

        private static string AvgFleetSentence(decimal state, decimal nat, double r, int hash)
        {
            if (r >= 1.5)
            {
                switch (hash % 2)
                {
                    case 0:  return "Carriers here average " + F1(state) + " power units per company — " + MultStr(r) + " the national average of " + F1(nat) + ".";
                    default: return "The average fleet here runs " + F1(state) + " units, well above the U.S. average of " + F1(nat) + ".";
                }
            }
            else
            {
                switch (hash % 2)
                {
                    case 0:  return "Average fleet size here is " + F1(state) + " units, below the national average of " + F1(nat) + ".";
                    default: return "Carriers here average " + F1(state) + " power units — under the U.S. average of " + F1(nat) + ".";
                }
            }
        }

        private static string GrowthSentence(decimal state, decimal nat, int hash)
        {
            if (state > nat)
            {
                switch (hash % 2)
                {
                    case 0:  return "New carrier registrations grew " + F1(state) + "% year over year, outpacing the national rate of " + F1(nat) + "%.";
                    default: return "Registrations are up " + F1(state) + "% year over year — vs. " + F1(nat) + "% nationally.";
                }
            }
            else
            {
                return "Registration growth slowed to " + F1(state) + "% year over year, compared to " + F1(nat) + "% nationally.";
            }
        }
    }
}
