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
        public string City { get; set; }
        public string CityURL { get; set; }
        public int NoCompanies { get; set; }
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
