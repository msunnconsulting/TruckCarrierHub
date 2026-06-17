using PartnerCarrier.ViewModels.Admin;
using System.Collections.Generic;

namespace PartnerCarrier.ViewModels.User
{
    public class CityVM
    {
        public string PageTitle { get; set; }

        public string StateName { get; set; }

        public string PageDescription { get; set; }

        public string CityName { get; set; }

        public List<CityVM> Cities { get; set; }

        public string StateCode { get; set; }

        public int CompanyCount { get; set; }

        public List<CityVM> popularcities { get; set; }

        public GetAQuoteVM GetAQuoteVM { get; set; }

        public List<LoadTypeVM> ListLoad { get; set; }
        public OutboundBannerDataModel OutboundBanner { get; set; }
    }
}
