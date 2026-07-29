using Common.Utility;
using Common.Utility.ViewModels;
using PartnerCarrier.ViewModels.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PartnerCarrier.ViewModels.Admin
{
    public class EmailsVM
    {
        public int? EmailID { get; set; }
        [Required(ErrorMessage = "Please Enter Subject")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Please Enter Email Content")]
        public string Content { get; set; }
        public bool LinkNeeded { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? EmailSent { get; set; }
        public DateTime? LastDateSent { get; set; }

    }
    public class EmailSentVM
    {
        public int USDOTNumber { get; set; }
        public int EmailId { get; set; }
    }

    public class SendEmailsVM
    {
        public int? EmailID { get; set; }
        public bool LinkNeeded { get; set; }
        public string Subject { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        [Required(ErrorMessage = "Please enter numbers of Emails to send")]
        [RegularExpression(Patterns.OnlyNumbers, ErrorMessage = "Please enter only numeric value")]
        public int EmailsToSend { get; set; }
        [RegularExpression(Patterns.OnlyNumbers, ErrorMessage = "Please enter only numeric value")]
        public int? RealTimeCounter { get; set; }
        [RegularExpression(Patterns.OnlyNumbers, ErrorMessage = "Please enter only numeric value")]
        public int? MinNoOfTrucks { get; set; }
        [RegularExpression(Patterns.OnlyNumbers, ErrorMessage = "Please enter only numeric value")]
        public int? MaxNoOfTrucks { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Please select valid date")]
        public DateTime? ToCompaniesAddedAfterTheDate { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Please select valid date")]
        public DateTime? ToCompaniesUpdatedAfterTheDate { get; set; }
        public List<StateVM> lstStates { get; set; }

        public string SiteURL { get; set; }
    }

    public class BusinessListVM
    {
        public int USDOTNumber { get; set; }
        public string Website { get; set; }
        public bool? EmailVerified { get; set; }
        public bool? WebsiteApproved { get; set; }
        public bool? CommunicationApproved { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? VerificationKey { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string BusinessContactEmail { get; set; }
        public string JobContactEmail { get; set; }
        public string JobContactPhone { get; set; }
        public string JobContactSMS { get; set; }
        public bool NowHiring { get; set; }
        public string ForgotPasswordKey { get; set; }

    }

    public class BusinessSearchVM
    {
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid USDOTNumber")]
        public int? USDOTNumber { get; set; }
        public string BusinessContactEmail { get; set; }
        public DateTime? UpdatedAfter { get; set; }
        public string ApprovedWebsite { get; set; }
    }

    public class SentEmailsProgressInfoVM
    {
        public int EmailID { get; set; }
        public int TotalMailToSent { get; set; }
        public bool IsInProgress { get; set; }
        public int MailSentSuccessful { get; set; }
        public int MailSentFailed { get; set; }
        public int TotalRecordCountForMail { get; set; } // How many mail will be sent (Count of fetched record for send mail)
        public int RealTimeCounter { get; set; }
        public bool CheckIsRecordsEnoughToUpdate { get; set; }
    }

    public class BusinessResetPasswordVM
    {
        public Guid? ResetPasswordKey { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class SuccessStoriesVM
    {
        [Display(Name = "Success Stories")]
        public string Content { get; set; }

        public bool SuccessStoryPublished { get; set; }
    }

    public class ManageGetAQuoteVM
    {
        public List<GetAQuoteToShowVM> GetAQuoteToShowList { get; set; }

        public bool? IsSelected { get; set; }
    }

    public class GetAQuoteToShowVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool ControlToShow { get; set; }
    }

    public class GlobalHiringVM
    {
        public GLobalHiring GlobalHiring { get; set; }

        public int GlobalHire { get; set; }

    }

    public enum GLobalHiring
    {
        [Display(Name = "Not to show at all")]
        NotToShow = 0,

        [Display(Name = "Show on City page")]
        CityPage = 1,

        [Display(Name = "Show on State/Province page and City page")]
        StateAndCityPage = 2,

        [Display(Name = "Show on Homepage, State/Province page and City page")]
        HomeStateAndCityPage = 3
    }

    public class BusinessLoginVM
    {
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email")]
        [Required(ErrorMessage = "Email Address is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }

    public class BusinessForgotPasswordVM
    {
        [Required(ErrorMessage = "Email Address is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public int? USDOTNumber { get; set; }

        public bool IsEmailAddressCheck { get; set; }

    }

    public class BusinessEmailAndUSDOTNumberVM
    {
        public string EmailAddress { get; set; }
        public int? USDOTNumber { get; set; }
    }

    public class CarrierVM
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid USDOTNumber")]
        public Nullable<int> USDOTNumber { get; set; }

        [Required(ErrorMessage = "Company Name field is required")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Contact Person 1 field is required")]
        public string ContactPerson1 { get; set; }

        [Required(ErrorMessage = "Contact Email 1 field is required")]

        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email address")]
        [MaxLength(255, ErrorMessage = "{0} cannot be longer than 255 characters.")]
        public string ContactEmail1 { get; set; }

        [Required(ErrorMessage = "Contact Phone 1 field is required")]
        public string ContactPhone1 { get; set; }

        [Required(ErrorMessage = "Contact Person 2 field is required")]
        public string ContactPerson2 { get; set; }

        [Required(ErrorMessage = "Contact Email 2 field is required")]
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email address")]
        [MaxLength(255, ErrorMessage = "{0} cannot be longer than 255 characters.")]
        public string ContactEmail2 { get; set; }

        [Required(ErrorMessage = "Contact Phone 2 field is required")]
        public string ContactPhone2 { get; set; }
        [Required(ErrorMessage = "Carrier Active field is required")]
        public bool CarrierActive { get; set; }
        public string Website { get; set; }
        public Nullable<int> MaxQuotesPerMonth { get; set; }

        public List<KeyValuePair<string, int>> LoadTypeList { get; set; }
        public List<int> SelectedLoadTypeList { get; set; }

        public List<KeyValuePair<string, string>> StateList { get; set; }

        [Required(ErrorMessage = "Pickup State Code field is required")]
        public List<string> SelectedPickupStateCode { get; set; }

        [Required(ErrorMessage = "Delivery State Code field is required")]
        public List<string> SelectedDeliveryStateCode { get; set; }
    }

    public class ManageCarrierVM
    {
        public string CompanyName { get; set; }
        public string EmailAddess { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class CommonCarrierAndPagelistVM
    {
        public PagedList<CarrierVM> PagedListVM { get; set; }

        public int? Id { get; set; }

        public string CompanyName { get; set; }
        public string ContactPerson1 { get; set; }
        public string ContactEmail1 { get; set; }
        public string ContactPhone1 { get; set; }
        public bool CarrierActive { get; set; }
    }


    public class OutboundLinkVM : OutboundLinkDataModel
    {
        public long? Id { get; set; }

    }

    public class OutboundLinkDataModel
    {
        [Required(ErrorMessage = "Please enter url title")]
        [MaxLength(150, ErrorMessage = "Cannot be longer than 150 characters")]
        public string URLTitle { get; set; }
        [Required(ErrorMessage = "Please enter url ")]
        [MaxLength(2048, ErrorMessage = "Cannot be longer than 2048 characters")]
        public string URL { get; set; }
        [Required(ErrorMessage = "Please enter number")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter only numeric value")]
        public int Number { get; set; }
        [Required(ErrorMessage = "Please enter comment")]
        //[MaxLength(250,ErrorMessage = "Cannot be longer than 250 characters")]
        public string Comment { get; set; }

        public bool IsFollow { get; set; }

    }
    public class BaseOutboundLinkVM
    {
        public List<OutboundLinkVM> List { get; set; }

        public OutboundLinkVM Data { get; set; }

    }

    public class OutboundBannerDataModel
    {
        public long? Id { get; set; }
        public OutboundBannerPageLevelEnum PageLevel { get; set; }
        public bool IsShow { get; set; } // Show banner or not    

        public string OriginalFileName { get; set; } // To save original file name
        public string FileName { get; set; } // To save generic id of uploaded banner

        [Required(ErrorMessage = "Please select an image")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase ImageFile { get; set; }

        [Required(ErrorMessage = "Please enter url")]
        public string URL { get; set; } = string.Empty;

        public bool IsFollow { get; set; }

        [Required(ErrorMessage = "Please enter Alt text")]
        [MaxLength(256, ErrorMessage = "Alt text cannot be longer than 256 characters.")]
        public string AltText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Title text")]
        [MaxLength(256, ErrorMessage = "Title text cannot be longer than 256 characters.")]
        public string TitleText { get; set; } = string.Empty;
    }

    public class BaseOutboundBannerVM
    {
        public List<OutboundBannerDataModel> OutboundBannerList { get; set; }

    }

    #region City Articles
    public class CityArticlesAvailabilityVM
    {
        public bool IsCityArticlesAllowed { get; set; } // Show/Hide city articles

        [Required(ErrorMessage = "Please enter number of words to display")]
        [Range(50, int.MaxValue, ErrorMessage = "The number of words must be greater than or equal to 50")]
        public int NumberOfWords { get; set; } // Stores number of words display in city articles
    }

    public class CreatCityArticlesVM : CityArticleFilterVM
    {
        public string NumberOfCompaniesFrom { get; set; }
        public string NumberOfCompaniesTo { get; set; }
        public bool IsCitiesWithoutArticles { get; set; }
        public int NumberOfCitiesSelected { get; set; }
        [Required(ErrorMessage = "Please Enter chatGPT prompt")]
        public string Prompt { get; set; }

    }

    public class SelectListVM
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class SelectedCitiesForCreateArticleModel : IValidatableObject
    {
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public string CityName { get; set; }
        public int? FromNumberCompany { get; set; }
        public int? ToNumberCompany { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public bool OnlyAffectNullArticleCities { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if only one of FromNumberCompany or ToNumberCompany is set
            if (FromNumberCompany != null && ToNumberCompany == null)
            {
                yield return new ValidationResult("ToNumberCompany is required when FromNumberCompany has a value.", new[] { nameof(ToNumberCompany) });
            }

            if (FromNumberCompany == null && ToNumberCompany != null)
            {
                yield return new ValidationResult("FromNumberCompany is required when ToNumberCompany has a value.", new[] { nameof(FromNumberCompany) });
            }
        }
    }

    public class ManageCityArticleVM : CityArticleFilterVM
    {
        public string ShowCitiesWithArticle { get; set; }
        [Required(ErrorMessage = "Please enter city article.")]
        public string CityArticle { get; set; }
    }

    public class CityArticleFilterVM
    {
        public List<SelectListVM> CountryList { get; set; }
        public List<SelectListVM> StateList { get; set; }
        public List<SelectListVM> CityList { get; set; }
        public string Country { get; set; } = String.Empty;
        public string State { get; set; } = String.Empty;
        public string City { get; set; } = String.Empty;
    }

    public class ManageCityListVM
    {
        public string Country { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string City { get; set; }
        public string CityURL { get; set; }
        public int NoCompanies { get; set; }
        public string Description { get; set; }
        public string Article { get; set; }
        public DateTime? LastRenewedDate { get; set; }
    }

    // Response Model (to represent the OpenAI API response)
    public class ChatGptResponse
    {
        public List<Choice> Choices { get; set; }

        public class Choice
        {
            public Message Message { get; set; }
        }

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }

    public class ManageCitiesVM
    {
        public List<ManageCityListVM> List { get; set; }
    }

    public class SaveCityContentVM
    {
        public string StateCode { get; set; }
        public string CityName { get; set; }
        public string Description { get; set; }
        public string Article { get; set; }
    }

    public class CityRenewProgressVM
    {
        public bool IsInProgress { get; set; }
        public int Processed { get; set; }
        public int Total { get; set; }
        public string Message { get; set; }
    }

    public class StartCityRenewVM
    {
        public int MinCompanies { get; set; }
        public int MaxCompanies { get; set; }
        public List<string> CityURLs { get; set; }
        public DateTime? LastRenewedBefore { get; set; }
    }

    public class CityContentPreviewVM
    {
        public bool Success { get; set; }
        public string Description { get; set; }
        public string Article { get; set; }
        public int CompanyCount { get; set; }
        public string Error { get; set; }
    }

    public class OpenAIAuthDetails
    {
        public string Model { get; set; }
        public string SystemContent { get; set; }
        public string APIKey { get; set; }
    }

    public class OpenAIResponse
    {
        public bool Status { get; set; }
        public string ResponseText { get; set; }
    }
    #endregion

    #region Statistics
    public class ShowStatisticsNavVM
    {
        public bool ShowStatisticsNav { get; set; }
    }

    public class StatisticsIndexVM
    {
        public int TotalCompanies { get; set; }
        public int PureBrokerCount { get; set; }
        public int ActiveBrokers { get; set; }
        public int StatesCount { get; set; }
        public int CitiesCount { get; set; }
        public int AvgFleetSize { get; set; }
        public string LastDataUpdate { get; set; }
        public int NewThisMonth { get; set; }
        public int NewThisYear { get; set; }
        public int NewRegistrations12MonthsCount { get; set; }
        public List<StateStatVM> TopStates { get; set; }
        public List<CityStatVM> TopCities { get; set; }
        public List<YearStatVM> RegistrationsByYear { get; set; }
        public List<StateStatVM> AllUsStates { get; set; }
    }

    public class StateStatVM
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int Count { get; set; }
    }

    public class CityStatVM
    {
        public string City { get; set; }
        public string StateCode { get; set; }
        public int Count { get; set; }
    }

    public class YearStatVM
    {
        public int Year { get; set; }
        public int Count { get; set; }
    }

    public class StatisticsActiveCompaniesVM
    {
        public int TotalActiveCompanies { get; set; }
        public int StatesRepresented { get; set; }
        public string TopStateName { get; set; }
        public string TopStateCode { get; set; }
        public int TopStateCount { get; set; }
        public DateTime? LastDataUpdate { get; set; }
        public List<StateStatRow> TopStates { get; set; }
        public List<CityStatRow> TopCities { get; set; }
        public List<StateMapRow> AllStatesForMap { get; set; }
        public int NewThisMonth { get; set; }
        public int NewLastMonth { get; set; }
        public int NewThisYear { get; set; }
        public int NewSamePeriodLastYear { get; set; }
        public int NewPrior12Months { get; set; }
        public int InterstateCarriersCount { get; set; }
        public decimal InterstateCarriersPct { get; set; }
        public int MedianFleetSize { get; set; }
        public CompanySizeDistribution SizeDistribution { get; set; }
        public List<CargoTypeRow> TopCargoTypes { get; set; }
        public List<MonthlyRegistrationRow> MonthlyRegistrations { get; set; }
        public int CitiesCount { get; set; }
        public int PureBrokerCount { get; set; }
        public List<TopCompanyRow> TopCompaniesByPowerUnits { get; set; }
        public List<StateNewRegRow> TopStatesByNewRegistrations { get; set; }
    }

    public class TopCompanyRow
    {
        public string CompanyName { get; set; }
        public int PowerUnits { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
    }

    public class StateNewRegRow
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int NewRegistrations { get; set; }
        public decimal PercentOfTotal { get; set; }
    }

    public class StateStatRow
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int CompanyCount { get; set; }
        public decimal PercentOfTotal { get; set; }
    }

    public class CityStatRow
    {
        public string CityName { get; set; }
        public string StateCode { get; set; }
        public int CompanyCount { get; set; }
        public decimal PercentOfTotal { get; set; }
    }

    public class StateMapRow
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int CompanyCount { get; set; }
    }

    public class StatisticsStateCompaniesVM
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int TotalActiveCompanies { get; set; }
        public int ActiveMCNumbers { get; set; }
        public int TotalPowerUnits { get; set; }
        public int TotalDrivers { get; set; }
        public decimal PercentOfUSTotal { get; set; }
        public DateTime? LastDataUpdate { get; set; }
        public List<CountyStatRow> TopCounties { get; set; }
        public List<StateCityStatRow> TopCities { get; set; }
        public List<CountyMapRow> AllCountiesForMap { get; set; }
        public CompanySizeDistribution SizeDistribution { get; set; }
        public int NewRegistrations12 { get; set; }
        public int NewPriorRegistrations12 { get; set; }
    }

    public class CountyStatRow
    {
        public string CountyName { get; set; }
        public int CountyCode { get; set; }
        public int CompanyCount { get; set; }
        public decimal PercentOfState { get; set; }
    }

    public class StateCityStatRow
    {
        public string CityName { get; set; }
        public int CompanyCount { get; set; }
        public decimal PercentOfState { get; set; }
    }

    public class CountyMapRow
    {
        public string CountyName { get; set; }
        public int CountyCode { get; set; }
        public int CompanyCount { get; set; }
    }

    public class CompanySizeDistribution
    {
        public int OneUnit { get; set; }
        public int TwoToFive { get; set; }
        public int SixToTwenty { get; set; }
        public int TwentyOneToHundred { get; set; }
        public int OverHundred { get; set; }
        public int TotalReporting { get; set; }
        public int MedianFleetSize { get; set; }
    }

    public class StatisticsCityCompaniesVM
    {
        public string CityName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public string CountyName { get; set; }
        public int TotalActiveCompanies { get; set; }
        public decimal PercentOfStateTotal { get; set; }
        public int NewRegistrationsLast24Months { get; set; }
        public decimal AvgNewPerMonth { get; set; }
        public decimal? NewRegistrationsYoYPercent { get; set; }
        public int MedianFleetSize { get; set; }
        public decimal OwnerOperatorPercent { get; set; }
        public decimal StateOwnerOperatorPercent { get; set; }
        public List<MonthlyRegistrationRow> MonthlyRegistrations { get; set; }
        public string BestMonthLabel { get; set; }
        public int BestMonthCount { get; set; }
        public string LowestMonthLabel { get; set; }
        public int LowestMonthCount { get; set; }
        public CompanySizeDistribution SizeDistribution { get; set; }
        public List<CargoTypeRow> TopCargoTypes { get; set; }
        public CompanyAgeDistribution AgeDistribution { get; set; }
        public List<AuthorityTypeRow> TopAuthorityTypes { get; set; }
        public List<CityCompanyRow> TopCompaniesByFleetSize { get; set; }
        public DateTime? LastDataUpdate { get; set; }
        public string SelectedRange { get; set; }
        public string RangeLabel { get; set; }
    }

    public class MonthlyRegistrationRow
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public string Label { get; set; }
    }

    public class CargoTypeRow
    {
        public string CargoTypeName { get; set; }
        public int CompanyCount { get; set; }
        public decimal PercentOfTotal { get; set; }
    }

    public class CompanyAgeDistribution
    {
        public int ZeroToTwo { get; set; }
        public int ThreeToFive { get; set; }
        public int SixToTen { get; set; }
        public int ElevenToTwenty { get; set; }
        public int OverTwenty { get; set; }
        public decimal AverageAgeYears { get; set; }
    }

    public class AuthorityTypeRow
    {
        public string AuthorityTypeName { get; set; }
        public int CompanyCount { get; set; }
        public decimal PercentOfTotal { get; set; }
    }

    public class CityCompanyRow
    {
        public string LegalName { get; set; }
        public int USDOTNumber { get; set; }
        public int FleetSize { get; set; }
    }

    public class StatisticsActiveBrokersVM
    {
        public int TotalActiveBrokers { get; set; }
        public int New24MonthsCount { get; set; }
        public decimal AvgNewPerMonth { get; set; }
        public int BrokerOnlyCount { get; set; }
        public decimal BrokerOnlyPct { get; set; }
        public int BrokerAndCarrierCount { get; set; }
        public decimal BrokerAndCarrierPct { get; set; }
        public DateTime? LastDataUpdate { get; set; }
        public List<StateStatRow> TopStates { get; set; }
        public List<StateMapRow> AllStatesForMap { get; set; }
        public List<CityStatRow> TopCities { get; set; }
        public List<BrokerEntityTypeRow> EntityTypes { get; set; }
        public List<MonthlyRegistrationRow> MonthlyRegistrations { get; set; }
        public CompanyAgeDistribution AgeDistribution { get; set; }
        public List<BrokerCarrierRatioRow> CarrierToBrokerRatioByState { get; set; }
        public List<LongestRegisteredBrokerRow> LongestRegisteredBrokers { get; set; }
    }

    public class BrokerEntityTypeRow
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Pct { get; set; }
    }

    public class BrokerCarrierRatioRow
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int CarrierCount { get; set; }
        public int BrokerCount { get; set; }
        public decimal Ratio { get; set; }
    }

    public class LongestRegisteredBrokerRow
    {
        public string LegalName { get; set; }
        public int USDOTNumber { get; set; }
        public string MCPrefix { get; set; }
        public int? MCNumber { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public int SinceYear { get; set; }
    }
    public class StatisticsNewRegistrationsVM
    {
        public string Range { get; set; }
        public int RangeMonths { get; set; }
        public DateTime RangeStart { get; set; }
        public DateTime RangeEnd { get; set; }
        public int TotalNewCount { get; set; }
        public int NewMotorCarrierCount { get; set; }
        public int NewBrokerCount { get; set; }
        public int NewOtherCount { get; set; }
        public decimal AvgPerMonth { get; set; }
        public int PrevTotalCount { get; set; }
        public int PrevMotorCarrierCount { get; set; }
        public int PrevBrokerCount { get; set; }
        public List<NrMonthlyRow> MonthlyAll { get; set; }
        public List<NrMonthlyRow> MonthlyMC { get; set; }
        public List<NrMonthlyRow> MonthlyBroker { get; set; }
        public int NewBothCount { get; set; }
        public int PrevBothCount { get; set; }
        public int EntityMCCount { get; set; }
        public int EntityBrokerCount { get; set; }
        public int EntityBothCount { get; set; }
        public int EntityOtherCount { get; set; }
        public List<NrStateRow> TopStates { get; set; }
        public List<NrCityRow> TopCities { get; set; }
        public NrAgeDistribution AgeDistribution { get; set; }
        public List<StateMapRow> AllStatesForMap { get; set; }
        public DateTime? LastDataUpdate { get; set; }
    }

    public class NrMonthlyRow
    {
        public string Label { get; set; }
        public int YearMonth { get; set; }
        public int Count { get; set; }
    }

    public class NrStateRow
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int Count { get; set; }
        public double PercentOfTotal { get; set; }
    }

    public class NrCityRow
    {
        public string CityName { get; set; }
        public string StateCode { get; set; }
        public int Count { get; set; }
        public double PercentOfTotal { get; set; }
    }

    public class NrAgeDistribution
    {
        public int ZeroToSixMonths { get; set; }
        public int SixToTwelveMonths { get; set; }
        public int OneToTwoYears { get; set; }
        public int TwoToFiveYears { get; set; }
        public int FivePlusYears { get; set; }
        public int Total { get; set; }
    }

    public class NrDailyRow
    {
        public int Day { get; set; }
        public int Count { get; set; }
    }

    public class StatisticsNewRegistrationsMonthVM
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int TotalCount { get; set; }
        public int MotorCarrierCount { get; set; }
        public int BrokerCount { get; set; }
        public int OtherCount { get; set; }
        public int BothCount { get; set; }
        public int PrevTotalCount { get; set; }
        public int PrevMotorCarrierCount { get; set; }
        public int PrevBrokerCount { get; set; }
        public int PrevBothCount { get; set; }
        public int PrevYear { get; set; }
        public int PrevMonth { get; set; }
        public bool HasPrev { get; set; }
        public int NextYear { get; set; }
        public int NextMonth { get; set; }
        public bool HasNext { get; set; }
        public List<NrDailyRow> DailyAll { get; set; }
        public List<NrStateRow> TopStates { get; set; }
        public List<NrCityRow> TopCities { get; set; }
        public List<NrStateRow> CarrierTopStates { get; set; }
        public List<NrCityRow> CarrierTopCities { get; set; }
        public List<NrStateRow> BrokerTopStates { get; set; }
        public List<NrCityRow> BrokerTopCities { get; set; }
        public List<NrDailyRow> DailyCarriers { get; set; }
        public List<NrDailyRow> DailyBrokers { get; set; }
        public string CarrierTopState { get; set; }
        public string BrokerTopState { get; set; }
        public DateTime? LastDataUpdate { get; set; }
    }

    public class FoFleetBucket
    {
        public string Label { get; set; }
        public int CompanyCount { get; set; }
        public long PowerUnitsSum { get; set; }
    }

    public class FoStateRow
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public long TotalPowerUnits { get; set; }
        public double AvgFleetSize { get; set; }
        public int ReportingCount { get; set; }
    }

    public class StatisticsFleetOperationsVM
    {
        public int TotalActiveCompanies { get; set; }
        public long TotalPowerUnits { get; set; }
        public double AvgFleetSize { get; set; }
        public int ReportingCount { get; set; }
        public int NonReportingCount { get; set; }
        public long TotalTrucksAndTractors { get; set; }
        public long OwnedTrucks { get; set; }
        public long OwnedTractors { get; set; }
        public long LeasedTrucks { get; set; }
        public long LeasedTractors { get; set; }
        public int OwnerOperatorCount { get; set; }
        public int InterstateCount { get; set; }
        public long InterstatePowerUnits { get; set; }
        public int IntrastateCount { get; set; }
        public long IntrastatePowerUnits { get; set; }
        public int HazmatCount { get; set; }
        public long HazmatPowerUnits { get; set; }
        public List<FoFleetBucket> FleetBuckets { get; set; }
        public List<FoStateRow> TopStatesByPowerUnits { get; set; }
        public List<FoStateRow> TopStatesByAvgFleet { get; set; }
        public DateTime? LastDataUpdate { get; set; }
    }

    public class CgCargoTypeRow
    {
        public string Label { get; set; }
        public int CompanyCount { get; set; }
        public long PowerUnitsSum { get; set; }
        public int ReportingWithPU { get; set; }
        public double AvgFleetSize { get; set; }
    }

    public class CargoSpecBucket
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class StatisticsCargoVM
    {
        public int TotalActiveCompanies { get; set; }
        public int CompaniesWithAnyCargo { get; set; }
        public int CompaniesWithNoCargo { get; set; }
        public int TotalCargoSelections { get; set; }
        public double AvgCargoTypesPerCompany { get; set; }
        public List<CgCargoTypeRow> AllCargoTypes { get; set; }
        public List<CargoSpecBucket> SpecBuckets { get; set; }
        public DateTime? LastDataUpdate { get; set; }
    }

    #endregion

    #region Company Reviews
    public class ManageReviewsVM
    {
        public ManageReviewEnum SelectedFilter { get; set; }

        public int SelectedFilterValue { get; set; }

    }

    public enum ManageReviewEnum
    {
        [Display(Name = "Not to show at all")]
        NotToShow = 0,

        [Display(Name = "Show on City page")]
        CityPage = 1,

        [Display(Name = "Show on State/Province page and City page")]
        StateAndCityPage = 2,

        [Display(Name = "Show on Homepage, State/Province page and City page")]
        HomeStateAndCityPage = 3
    }
    public class DeleteReviewsByUSDOTNumberVM
    {
        [Required(ErrorMessage = "Please Enter USDOT Number")]
        public string USDOTNumber { get; set; }
    }

    #endregion
}
