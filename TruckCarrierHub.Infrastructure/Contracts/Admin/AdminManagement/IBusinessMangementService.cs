using Common.Utility.ViewModels;
using PartnerCarrier.ViewModels.Admin;
using PartnerCarrier.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement
{
    public interface IBusinessMangementService
    {
        PagedList<EmailsVM> GetEmailList(PageSortPara ps);
        long SaveEmail(EmailsVM emailsVM);
        EmailsVM GetEmailByEmailId(int emailId);
        SendEmailsVM GetEmailDetailsForSendMailByEmailId(int emailId);
        void SendEmails(SendEmailsVM sendEmailsVM);
        void UnSubscribeEmail(int USDOTNumber);
        SentEmailsProgressInfoVM GetSentEmailsProgressInfo(int emailId);
        SentEmailsProgressInfoVM StopSentEmailsProgressInfo(int emailId);
        void SaveSuccessStories(SuccessStoriesVM successStoriesVM);
        SuccessStoriesVM GetSuccessStory();
        PagedList<BusinessListVM> GetBusinessSearchList(PageSortPara ps, BusinessSearchVM businessSearchVM);
        void DeleteBusinessById(int businessId);
        void UpdateBusiness(BusinessListVM businessSearchVM);
        void BussinessResetPasswordMailSend(BusinessForgotPasswordVM businessForgotPasswordVM);
        BusinessVM CheckResetPasswordKeyIsValid(Guid? resetPasswordKey);
        void BussinessResetPassword(BusinessVM businessVM);

        /// <summary>
        /// Bind Global Hiring
        /// </summary>
        /// <returns></returns>
        GlobalHiringVM GetGlobalHiring();

        /// <summary>
        /// Save Global Hiring 
        /// </summary>
        /// <param name="globalHiringVM"></param>
        void SaveGlobalHiring(GlobalHiringVM globalHiringVM);
        common.SignInStatus BusinessLogin(BusinessLoginVM businessLoginVM);
        void SaveGetAQuoteToShow(ManageGetAQuoteVM manageGetAQuoteVM);
        List<GetAQuoteToShowVM> GetCheckboxListForShowGetAQuoteControl();
        List<KeyValuePair<string, int>> BindCheckboxListForLoadType();
        void SaveCarrierDetails(CarrierVM carrierVM);
        List<KeyValuePair<string, string>> BindCheckboxListForState();
        CarrierVM GetCarrierDetailsById(int value);
        PagedList<CarrierVM> GetCarrierList(PageSortPara ps, ManageCarrierVM manageCarrierVM);
        void ActiveOrInActiveCarrierById(int carrierId);
        List<OutboundLinkVM> GetOutBoundLinks(bool sortByNumber);
        OutboundLinkVM GetOutboundLinkDataById(int id);
        void DeleteOutboundLinkById(int id);
        void SaveOutboundLink(OutboundLinkVM data);
        int GetTotalOutboundLink();
        List<OutboundBannerDataModel> GetOutBoundBanners();
        void SaveOutboundBanner(List<OutboundBannerDataModel> data);
        void SaveCityArticlesAvailability(CityArticlesAvailabilityVM data);
        CityArticlesAvailabilityVM GetCityArticleAvailabilityDetails();
        List<SelectListVM> GetCountries();
        List<SelectListVM> GetStates(string countryCode);
        List<SelectListVM> GetCities(string countryCode, string stateCode);
        Int64 GetSelectedCitiesForCreateArticle(SelectedCitiesForCreateArticleModel selecteCitiesCriteria);
        Task<string> CreateAndSaveArticleToCityAsync(SelectedCitiesForCreateArticleModel selecteCitiesCriteria, string promptText);
        Task<string> AllowArticleForSelectedState(string countryCode, string stateCode);
        string GetArticleForSelectedCity(string countryCode, string stateCode, string cityName);
        string AllowArticleForSelectedCity(string countryCode, string stateCode, string cityName);
        string SetArticleForSelectedCity(string countryCode, string stateCode, string cityName, string article);
        List<SelectListVM> GetCitiesForManageCityArticle(string countryCode, string stateCode, bool? isAllowed);
        bool IsCityArticleAvailable(string countryCode, string stateCode, string cityName);
        List<ManageCityListVM> GetAllCities();
        List<ManageCityListVM> GetCityListForManageCities();
        Task<string> FinishCityUpdate();
        ManageReviewsVM GetReviewsFilter();
        void SaveReviewsFilter(ManageReviewsVM manageReviewsVM);
        ManageReviewsVM GetCompanyReviewsFilterValue();
    }
}
