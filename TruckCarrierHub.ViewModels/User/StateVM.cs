namespace PartnerCarrier.ViewModels.User
{
    using PartnerCarrier.ViewModels.Admin;
    using System.Collections.Generic;

    public class StateVM
    {
        public List<StateVM> CAStates { get; set; }

        public List<StateVM> USStates { get; set; }

        public string CountryCode { get; set; }

        public string State { get; set; }

        public string StateCode { get; set; }

        public int CityCount { get; set; }

        public int StateCount { get; set; }

        public string PageTitle { get; set; }

        public string PageDescription { get; set; }

        public string MetaDescription { get; set; }

        public string HomeArticle { get; set; }

        public string SearchFor { get; set; }

        public GetAQuoteVM GetAQuoteVM { get; set; }

        public OutboundBannerDataModel OutboundBanner { get; set; }

        // Homepage statistics band — populated from GetStatisticsData() (30-day cached)
        public int HpTotalCompanies { get; set; }
        public int HpActiveBrokers { get; set; }
        public int HpNewRegistrations12m { get; set; }
        public int HpCitiesCount { get; set; }
        public string HpLastDataUpdate { get; set; }

        // Homepage monthly teaser — null means render nothing (graceful degradation)
        public StatisticsNewRegistrationsMonthVM HpMonthTeaser { get; set; }
    }
}
