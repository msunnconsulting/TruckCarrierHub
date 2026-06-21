namespace PartnerCarrier.Infrastructure.Contracts.User
{
    using Common.Utility.ViewModels;
    using PartnerCarrier.ViewModels.Admin;
    using System;
    using System.Collections.Generic;
    using ViewModels.User;

    public interface IHomepageService
    {
        List<StateVM> GetUsStates(bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsCheckboxIsChecked, int SelectedReviewsFilterValue);

        List<StateVM> GetCaStates(bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsCheckboxIsChecked, int SelectedReviewsFilterValue);

        List<StateVM> GetAllStates();

        List<CityVM> GetCityList(string state, bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked = false, int ReviewFilterValue = 0);

        List<CityVM> GetPopularCities(string state, bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked, int ReviewFilterValue);

        CompanyInformationVM GetCompanyInformation(int usdotnumber, string companyURL);

        string GetPageTitle(string pageName, string state, string city);

        string GetPageDescription(string pageName, string state, string city, int? pageNum = null);

        string GetCityMetaDescription(string state, string city);

        string GetPlainHomeMetaDescription();

        string GetHomeArticle();

        string GetStateName(string stateCode);

        string SetPageTitleForCompanyInformation(int usdotnumber);

        string SetPageDescriptionForCompanyInformation(int usdotnumber);

        List<SearchVM> GetSearchResultAutoComplete(string searchText, string selectedValue);

        PagedList<CompanyVM> GetCompanyListFromSearch(string searchText, string selectedDropdownValue, PageSortPara ps, bool isHiringCheckboxIsChecked, int GlobalHire);

        List<KeyValuePair<string, string>> GetServiceTypes();

        List<KeyValuePair<string, string>> GetCargoTypes();

        List<KeyValuePair<string, string>> GetTrailerTypesForFilter();

        List<KeyValuePair<string, string>> GetDriverTypesForFilter();

        PagedList<CompanyVM> GetCompanyListFromFilter(SearchFilterVM searchFilterVM, string stateCode, string cityName, PageSortPara ps, bool isForMapView, bool? isRestrictResultToCity, bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked, int ReviewFilterValue);

        SearchFilterVM CheckFilterValueType(string filter, SearchFilterVM searchFilterVM);

        SearchFilterVM CheckFilterValuesNullOrNot(string filter1, string filter2, string filter3, string filter4, string filter5, string filter6, string filter7, string filter8, string filter9);

        string CreateNewUrl(string state, string city, string SearchText, string filter1, string filter2, string filter3, string filter4, string filter5, string filter6, string filter7, string filter8, string filter9);

        string GetCityNameFromCompanyList(List<CompanyVM> companyList);

        string GetStateCodeFromCompanyList(List<CompanyVM> companyList);

        /// <summary>
        /// Save Account Details
        /// </summary>
        /// <param name="businessVM"></param>
        void SaveAccountDetails(BusinessVM businessVM);

        /// <summary>
        /// Get Company Email Address
        /// </summary>
        /// <param name="uSDOTNumber"></param>
        /// <returns></returns>
        BusinessVM GetCompanyEmailAddress(int? USDOTNumber);

        BusinessVM GetBusinessDetailsByUSDOTNumber(int? usdotnumber);

        /// <summary>
        /// Verify Email Address
        /// </summary>
        /// <param name="verifyLinkGUID"></param>
        int VerifyEmail(Guid verifyLinkGUID);

        CompanyVM GenerateURLAfterSaveBusiness(int uSDOTNumber);

        /// <summary>
        /// Get Trailer Types
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<string, int>> GetTrailerTypes();

        /// <summary>
        /// Get Driver Type
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<string, int>> GetDriverTypes();

        /// <summary>
        /// Change Password
        /// </summary>
        /// <param name="businessVM"></param>
        void ChangePassword(BusinessVM businessVM);

        /// <summary>
        /// Save Company Business Details
        /// </summary>
        /// <param name="businessVM"></param>
        void SaveCompanyBusinessDetails(BusinessVM businessVM);
        List<SearchVM> GetSearchResultAutoCompleteCity(string id, string selectedValue);

        /// <summary>
        /// Get Load Types for binding dropdown
        /// </summary>
        /// <returns></returns>
        List<LoadTypeVM> GetDropDownListForLoadType();
        List<LocationTypeVM> GetDropDropdownForPickupLocationType(string loadType);
        List<LocationTypeVM> GetDropdownForDeliveryLocationType(string loadType);
        List<SpecialHandlingVM> GetCheckBoxList(string location, string loadType, int LocationTypeId);
        List<SpecialHandlingVM> GetCheckBoxListFromLocationType(int locationTypeId, string locationType, string loadType);
        List<LoadClassVM> GetDropDownForLoadClass();
        List<LoadItemTypeVM> GetDropDownForLoadItemType();
        GetAQuoteVM submitGetAQuoteDetails(GetAQuoteVM getAQuoteVM);
        List<LoadContainerTypeVM> GetDropDownForLoadContainerType();
        List<LoadContainerLengthVM> GetDropDownForLoadContainerLength();
        List<RefrigerationVM> GetDropDownForRefrigerationType(string loadType);
        string GetCityNameFromURLCityName(string city);
        List<LoadTruckTypeVM> GetDropDownForTruckType(string loadType);
        List<LoadInfoVM> GetDropDownForLoadInfo();
        List<TemperatureVM> GetDropdownForTemperatureType();
        void SendEmailsForQuote(int quoteId);
        OutboundBannerDataModel GetOutboundBanner(byte pageLevel);
        int GetNumberOfWordsAllowedByAdmin();
        CompanyRatingVM GetCompanyRating(int usdotnumber);
        void AddCompanyReview(AddCompanyReviewVM addCompanyReviewVM);
        AddCompanyReviewVM GetReviewDetailsById(int reviewId);
        void UpdateCompanyReview(int reviewId, AddCompanyReviewVM addCompanyReviewVM);
        CompanyReviewSummaryVM GetReviewsList(int companyUSDOT, int sortOption = 0);
        CompanyReviewerNames GetCompanyandReviewerName(int companyUSDOT, int reviewerId = 0, int rUSDOTNumber = 0);
        void AddUpdateReviewResponse(AddEditReviewReplyVM addEditReviewReplyVM);
        AddEditReviewReplyVM GetResponseDetailsById(int responseId);
    }
}
