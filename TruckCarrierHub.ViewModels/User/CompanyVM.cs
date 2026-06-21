namespace PartnerCarrier.ViewModels.User
{
    using Common.Utility;
    using Common.Utility.ViewModels;
    using ExpressiveAnnotations.Attributes;
    using PartnerCarrier.ViewModels.Admin;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class CompanyVM
    {
        public string PageTitle { get; set; }

        public string PageDescription { get; set; }

        public string City { get; set; }

        public string StateCode { get; set; }

        public List<string> Services { get; set; }

        public List<string> CompanyCargoType { get; set; }

        public List<string> CompanyTrailerType { get; set; }

        public List<string> CompanyDriverType { get; set; }

        public string EntityType { get; set; }

        public int? TruckOrTractor { get; set; }

        public int? TotalNumberOfTrucks { get; set; }

        public int? SortRelevance { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string LegalName { get; set; }

        public string CompanyName { get; set; }

        public string DoingBusinessAsName { get; set; }

        public string PhysicalAddressStreet { get; set; }

        public string PhysicalAddressCity { get; set; }

        public string PhysicalAddressStateCode { get; set; }

        public string PhysicalAddressZipCode { get; set; }

        public int USDOTNumber { get; set; }

        public int? IccDocketNumberFirst { get; set; }

        public string OfficeTelephoneNumber { get; set; }

        public string CellPhoneNumber { get; set; }

        public PageSortPara ps { get; set; }

        public PagedList<CompanyVM> Companies { get; set; }

        public SearchFilterVM SearchFilter { get; set; }

        public string NewUrl { get; set; }

        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public string StateName { get; set; }

        public int? NNDriversGrandTotalInterstateAndIntrastate { get; set; }

        public string CompanyEmailAddress { get; set; }

        public string companyNameForUrl { get; set; }

        public GetAQuoteVM GetAQuoteVM { get; set; }

        public List<LoadTypeVM> ListLoad { get; set; }

        public string hdnCityName { get; set; }

        public string Status { get; set; }
        public OutboundBannerDataModel OutboundBanner { get; set; }
        public int NumberOfWordsForCityArticle { get; set; }


        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
    public class BusinessVM
    {
        [Display(Name = "Website")]
        [RegularExpression(Patterns.website, ErrorMessage = "Please enter valid Website")]
        [MaxLength(255, ErrorMessage = "Website cannot be longer than 255 characters.")]
        public string WebsiteName { get; set; }

        public bool? CommunicationApproved { get; set; }

        public int USDOTNumber { get; set; }

        public string CompanyEmailAddress { get; set; }

        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email")]
        public string BusinessContactEmail { get; set; }

        [Required]
        [DisplayName("Password")]
        [RegularExpression(Patterns.PasswordValidation, ErrorMessage = "* Password must be at least 6 characters long.<br/>* Password must have at least one digit.<br/>* Password must have at least one uppercase ('A'-'Z').<br/>* Password must have at least one lowercase ('a'-'z')")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Confirm Password must be match with Password")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string ResetPasswordKey { get; set; }

        public string DoingBusinessAsName { get; set; }

        public string LegalName { get; set; }

        public List<KeyValuePair<string, int>> TrailerTypeList { get; set; }
        public List<int> SelectedTrailerTypes { get; set; }

        public List<KeyValuePair<string, int>> DriverTypeList { get; set; }
        public List<int> SelectedDriverTypes { get; set; }

        public string filter { get; set; }

        public bool IsNowHiring { get; set; }

        [RequiredIf("JobContactEmail == null && JobContactPhone == null && JobContactSMS == null && IsNowHiring == true", ErrorMessage = "At least Job Contact Email is required")]
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email")]
        public string JobContactEmail { get; set; }

        [RequiredIf("JobContactEmail == null && JobContactSMS == null && JobContactPhone == null && IsNowHiring == true", ErrorMessage = "At least Job Contact Phone is required")]
        public string JobContactPhone { get; set; }

        [RequiredIf("JobContactEmail == null && JobContactPhone == null && JobContactSMS == null && IsNowHiring == true", ErrorMessage = "At least Job Contact SMS is required")]
        public string JobContactSMS { get; set; }
    }

    public class EmailverifyVM
    {
        public int USDOTNumber { get; set; }
    }

    public class CompanyInformationVM
    {
        public string StateName { get; set; }

        public int USDOTNumber { get; set; }

        public string StateCode { get; set; }

        public string CityName { get; set; }

        public int? IccDocketNumberFirst { get; set; }

        public string PhysicalAddressCity { get; set; }

        public string PhysicalAddressStateCode { get; set; }

        public string EntityType { get; set; }

        public string DoingBusinessAsName { get; set; }

        public string LegalName { get; set; }

        public string HazmatIndicator { get; set; }

        public List<ServiceTypeVM> ServiceType { get; set; }

        public string PhysicalAddressStreet { get; set; }

        public string PhysicalAddressZipCode { get; set; }

        public string MailingAddressStreet { get; set; }

        public string MailingAddressCity { get; set; }

        public string MailingAddressStateCode { get; set; }

        public string MailingAddressZipCode { get; set; }

        public string OfficeTelephoneNumber { get; set; }

        public string CellPhoneNumber { get; set; }

        public string OfficeFaxPhoneNumber { get; set; }

        public int? DateAdded { get; set; }

        public string OperationCarrierInterstate { get; set; }

        public string OperationCarrierIntrastateHazmat { get; set; }

        public string OperationCarrierIntrastateNonHazmat { get; set; }

        public string CargoType { get; set; }

        public int? TrucksAndTractors { get; set; }

        public List<string> Services { get; set; }

        public List<string> CompanyCargoType { get; set; }

        public List<string> TrailerType { get; set; }

        public List<string> DriverType { get; set; }

        public List<string> CompanyTrailerType { get; set; }

        public List<string> CompanyDriverType { get; set; }

        public string Entity { get; set; }

        public string DateInString { get; set; }

        public string ServiceInCommaSaperated { get; set; }

        public string CargoInCommaSaperated { get; set; }

        public int? NNDriversGrandTotalInterstateAndIntrastate { get; set; }

        public string PageTitle { get; set; }

        public string PageDescription { get; set; }

        public string CompanysEmailAddress { get; set; }

        public string CompanysWebsiteName { get; set; }

        public bool? WebsiteApproved { get; set; }

        public int UsDOtNum { get; set; }

        public string BusinessContactEmail { get; set; }

        public string JobContactEmail { get; set; }
        public string JobContactPhone { get; set; }
        public string JobContactSMS { get; set; }
        public bool NowHiring { get; set; }

        public GetAQuoteVM GetAQuoteVM { get; set; }
        public List<LoadTypeVM> ListLoad { get; set; }

        public string CompanyName { get; set; }

        public string Status { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public OutboundBannerDataModel OutboundBanner { get; set; }

        public bool CanWriteReview { get; set; } = true;
    }

    public class ServiceTypeVM
    {
        public string ServiceType { get; set; }

        public int ServiceNumber { get; set; }

        public int USDOTNumber { get; set; }

        public string ServiceTypeForUrl { get; set; }

    }

    public class CargoTypeVM
    {
        public int CargoNumber { get; set; }

        public string CargoName { get; set; }

        public string CargoNameForUrl { get; set; }

        public int USDOTNumber { get; set; }
    }

    public class TrailerTypeVM
    {
        public int TrailerNumber { get; set; }

        public string TrailerName { get; set; }

        public string TrailerNameForUrl { get; set; }
    }

    public class DriverTypeVM
    {
        public int DriverNumber { get; set; }

        public string DriverName { get; set; }

        public string DriverNameForUrl { get; set; }
    }

    public class SearchVM
    {
        public int Id { get; set; }
        public int USDOTNumber { get; set; }
        public int MCNumber { get; set; }
        public string CityName { get; set; }
        public string Value { get; set; }
        public string CompanyName { get; set; }
    }

    public class SearchFilterVM
    {
        public string SearchText { get; set; }

        public List<string> EntityTypeList { get; set; }
        public List<string> SelectedEntityTypes { get; set; }

        public List<KeyValuePair<string, string>> CargoTypeList { get; set; }
        public List<string> SelectedCargoTypes { get; set; }

        public string SortBy { get; set; }

        public BoundrieValuesVM BoundrieValues { get; set; }

        public List<KeyValuePair<string, string>> ServiceTypeList { get; set; }
        public List<string> SelectedServiceTypes { get; set; }

        public List<string> TrucksOrTractor { get; set; }
        public List<string> SelectedTruckOrTractor { get; set; }

        public int ZoomLevel { get; set; }

        public List<KeyValuePair<string, string>> TrailerTypeList { get; set; }
        public List<string> SelectedTrailerTypes { get; set; }

        public List<KeyValuePair<string, string>> DriverTypeList { get; set; }
        public List<string> SelectedDriverTypes { get; set; }



    }

    public class TruckOrTractorVM
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }

    public class CompanyAddressVM
    {
        public string FullAddress { get; set; }

        public string LegalName { get; set; }

        public string CompanyBusinessName { get; set; }

        public string formatted_address { get; set; }


        public string Phone { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

    }

    public class GoogleGeoCodeResponse
    {
        public string status { get; set; }
        public results[] results { get; set; }
    }

    public class results
    {
        public string formatted_address { get; set; }
        public geometry geometry { get; set; }
        public string[] types { get; set; }
        public address_component[] address_components { get; set; }
    }

    public class geometry
    {
        public string location_type { get; set; }
        public location location { get; set; }
        public bounds bounds { get; set; }
        public viewport viewport { get; set; }
    }

    public class location
    {
        public string lat { get; set; }
        public string lng { get; set; }
    }

    public class bounds
    {
        public northeast northeast { get; set; }
        public southwest southwest { get; set; }
    }

    public class viewport
    {
        public northeast northeast { get; set; }
        public southwest southwest { get; set; }
    }

    public class northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class southwest
    {
        public double? lat { get; set; }
        public double lng { get; set; }
    }

    public class address_component
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public string[] types { get; set; }
    }

    public class southeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class northwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class BoundrieValuesVM
    {
        public northeast Northeast { get; set; }

        public northwest Northwest { get; set; }

        public southeast Southeast { get; set; }

        public southwest Southwest { get; set; }

    }

    public class CommonLatLongVM
    {
        public double? lat { get; set; }

        public double? lng { get; set; }
    }

    public class CompanyForSendMailVM
    {
        public string PageTitle { get; set; }

        public string PageDescription { get; set; }

        public string City { get; set; }

        public string StateCode { get; set; }

        public List<string> Services { get; set; }

        public List<string> CompanyCargoType { get; set; }

        public string EntityType { get; set; }

        public int? TruckOrTractor { get; set; }

        public int? TotalNumberOfTrucks { get; set; }

        public int? SortRelevance { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string LegalName { get; set; }

        public string CompanyName { get; set; }

        public string DoingBusinessAsName { get; set; }

        public string PhysicalAddressStreet { get; set; }

        public string PhysicalAddressCity { get; set; }

        public string PhysicalAddressStateCode { get; set; }

        public string PhysicalAddressZipCode { get; set; }

        public int USDOTNumber { get; set; }

        public int? IccDocketNumberFirst { get; set; }

        public string OfficeTelephoneNumber { get; set; }

        public string CellPhoneNumber { get; set; }

        public PageSortPara ps { get; set; }

        public PagedList<CompanyVM> Companies { get; set; }

        public SearchFilterVM SearchFilter { get; set; }

        public string NewUrl { get; set; }

        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public string StateName { get; set; }

        public int? NNDriversGrandTotalInterstateAndIntrastate { get; set; }

        public string CompanyEmailAddress { get; set; }

        public string companyNameForUrl { get; set; }

        public DateTime? DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }

        public int? intDateAdded { get; set; }
        public int? intDateUpdated { get; set; }

    }

    public class CompanyRatingVM
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class AddCompanyReviewVM
    {
        public int? ReviewId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company USDOT Number is required.")]
        public int CompanyUSDOT { get; set; }

        [Required(ErrorMessage = "Reviewer USDOT Number is required.")]
        public int ReviewerUSDOT { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [StringLength(1200, ErrorMessage = "Comment cannot exceed 1200 characters.")]
        [MaxWordCount(120, ErrorMessage = "Comment cannot exceed 120 words.")]
        public string Comment { get; set; }
        public bool IsReview { get; set; } = true;
    }

    public class CompanyReviewSummaryVM
    {
        public int CompanyUSDOT { get; set; }
        public string CompanyName { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingsBreakdown { get; set; }
        public bool CanWriteReview { get; set; } = true;
        public List<ReviewWithReplyVM> Reviews { get; set; }
    }
    public class ReviewWithReplyVM
    {
        public int ReviewId { get; set; } = 0;
        public int ReponseId { get; set; } = 0;
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int ReviewerUSDOT { get; set; }
        public string ReviewerName { get; set; }
        public string ReplyText { get; set; }
        public DateTime? ReplyCreatedDate { get; set; }
        public DateTime? ReplyUpdateDate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanReply { get; set; }
        public string UpdatedOn { get; set; }
        public string ReplyUpdatedOn { get; set; }
    }


    public enum ReviewSortOption
    {
        [Display(Name = "Newest")]
        Newest = 0,

        [Display(Name = "Highest Rating")]
        HighestRating = 1,

        [Display(Name = "Lowest Rating")]
        LowestRating = 2,
    }

    public class CompanyReviewerNames
    {
        public string CompanyName { get; set; }
        public string ReviewerName { get; set; }
    }

    public class AddEditReviewReplyVM
    {
        public int Id { get; set; } = 0;
        public int ReviewId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company USDOT Number is required.")]
        public int CompanyUSDOT { get; set; }

        [Required(ErrorMessage = "Reply text is required.")]
        [StringLength(1200, ErrorMessage = "Reply text cannot exceed 1200 characters.")]
        [MaxWordCount(120, ErrorMessage = "Reply text cannot exceed 120 words.")]
        public string Response { get; set; }
    }

}
