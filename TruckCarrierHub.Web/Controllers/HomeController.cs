

namespace PartnerCarrier.Web.Controllers
{
    using Common.Utility;
    using Common.Utility.Extensions;
    using Common.Utility.Logger;
    using Common.Utility.ViewModels;
    using Common.Utility.WebRequests;
    using Filters;
    using Infrastructure;
    using Infrastructure.Contracts.Admin.AdminManagement;
    using PartnerCarrier.Infrastructure.Contracts.User;
    using PartnerCarrier.ViewModels.User;
    using PartnerCarrier.Web.SessionManager;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using ViewModels.Admin;
    using static ViewModels.Admin.common;

    public class HomeController : BaseController
    {
        #region Private Variables
        private readonly IHomepageService _homepageService;
        private readonly IBusinessMangementService _businessMangementService;
        #endregion

        #region constructor
        public HomeController(IHomepageService homepageService, IBusinessMangementService businessMangementService)
        {
            _homepageService = homepageService;
            _businessMangementService = businessMangementService;
        }
        #endregion

        #region Homepage
        /// <summary>
        /// show homepage
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            StateVM stateVM = new StateVM();
            var pageName = "Homepage";
            //set page title
            stateVM.PageTitle = _homepageService.GetPageTitle(pageName, null, null);
            //set page description
            stateVM.PageDescription = _homepageService.GetPageDescription(pageName, null, null);
            stateVM.MetaDescription = _homepageService.GetPlainHomeMetaDescription();
            //set homepage article text (separate from PageDescription)
            stateVM.HomeArticle = _homepageService.GetHomeArticle();

            //Get Global Hire value from Admin table
            int GlobalHire = _businessMangementService.GetGlobalHiring().GlobalHire;
            int ReviewFilterValue = _businessMangementService.GetReviewsFilter().SelectedFilterValue;

            bool isHiringCheckboxIsChecked = SessionManager.Instance.IsHiringCheckboxIsChecked;
            bool isReviewsFilterCheckboxIsChecked = SessionManager.Instance.IsReviewsFilterCheckboxIsChecked;

            // Get outbound banner for Homepage
            stateVM.OutboundBanner = _homepageService.GetOutboundBanner((byte)OutboundBannerPageLevelEnum.Homepage);

            //get US state list based on Hiring
            //when Checkbox is check on homepage it take only Hiring States
            stateVM.USStates = _homepageService.GetUsStates(isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);
            //get Canada State List based on Hiring
            //when Checkbox is check on homepage it take only Hiring States
            stateVM.CAStates = _homepageService.GetCaStates(isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);

            stateVM.GetAQuoteVM = new GetAQuoteVM();
            stateVM.GetAQuoteVM.ListLoad = _homepageService.GetDropDownListForLoadType();

            var hpStats = _homepageService.GetStatisticsData();
            stateVM.HpTotalCompanies      = hpStats.TotalCompanies;
            stateVM.HpActiveBrokers       = hpStats.ActiveBrokers;
            stateVM.HpNewRegistrations12m = hpStats.NewRegistrations12MonthsCount;
            stateVM.HpCitiesCount         = hpStats.CitiesCount;
            stateVM.HpLastDataUpdate      = hpStats.LastDataUpdate;

            try
            {
                var hpNow       = DateTime.Now;
                var hpLastMonth = new DateTime(hpNow.Year, hpNow.Month, 1).AddMonths(-1);
                stateVM.HpMonthTeaser = _homepageService.GetNewRegistrationsMonthData(hpLastMonth.Year, hpLastMonth.Month);
            }
            catch
            {
                stateVM.HpMonthTeaser = null;
            }

            return View(stateVM);
        }

        #endregion

        #region State Page
        /// <summary>
        /// show city list for selected state
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult State(string stateCode)
        {
            CityVM cityVM = new CityVM();
            var pageName = "Statepage";
            cityVM.StateName = _homepageService.GetStateName(stateCode);
            //set page Description
            cityVM.PageDescription = _homepageService.GetPageDescription(pageName, cityVM.StateName, null);
            //set page title
            cityVM.PageTitle = _homepageService.GetPageTitle(pageName, cityVM.StateName, null);

            bool isHiringCheckboxIsChecked = SessionManager.Instance.IsHiringCheckboxIsChecked;
            bool isReviewsFilterCheckboxIsChecked = SessionManager.Instance.IsReviewsFilterCheckboxIsChecked;

            //Get Global Hire value from Admin table
            int GlobalHire = _businessMangementService.GetGlobalHiring().GlobalHire;
            int ReviewFilterValue = _businessMangementService.GetReviewsFilter().SelectedFilterValue;

            //get city list for selected state, then derive popular cities from it in memory (no second query)
            cityVM.Cities = _homepageService.GetCityList(stateCode, isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);
            cityVM.popularcities = cityVM.Cities.OrderByDescending(c => c.CompanyCount).Take(10).ToList();

            // Get outbound banner for State page
            cityVM.OutboundBanner = _homepageService.GetOutboundBanner((byte)OutboundBannerPageLevelEnum.State);

            //Binding Load Type for State page
            cityVM.GetAQuoteVM = new GetAQuoteVM();
            cityVM.GetAQuoteVM.ListLoad = _homepageService.GetDropDownListForLoadType();

            try
            {
                var stateModule = _homepageService.GetStateCompaniesData(stateCode);
                if (stateModule != null)
                {
                    ViewModels.Admin.StatisticsFleetOperationsVM smFleet  = null;
                    ViewModels.Admin.StatisticsNewRegistrationsVM smNewReg = null;
                    try { smFleet  = _homepageService.GetFleetOperationsData(); }      catch { }
                    try { smNewReg = _homepageService.GetNewRegistrationsData("12m"); } catch { }
                    ViewBag.StateModule           = stateModule;
                    ViewBag.StateModuleHighlights = PartnerCarrier.Web.Helpers.StateModuleHelper.BuildHighlights(
                        stateModule, smFleet, smNewReg);
                }
            }
            catch { }

            return View(cityVM);
        }
        #endregion

        #region City Page



        /// <summary>
        /// show company list for selected city
        /// show 20 companies on each page
        /// for map view whenever someone types the URL in browser again, it should directly load map that's set to exactly that bound
        /// at that time this method calls two times, first time does not load the map and in 2nd request we can load map using ajax request
        /// </summary>
        /// <returns></returns>
        public ActionResult City(string stateCode, string city, string SearchText, string filter1, string filter2, string filter3, string filter4, string filter5, string filter6, string filter7, string filter8, string filter9, PageSortPara ps, bool? isRestrictResultToCity)
        {
            //Get CityName from Url if cityname found return CityName else throw exception 404
            var cityNameFromDb = _homepageService.GetCityNameFromURLCityName(city, stateCode);

            if (cityNameFromDb == null)
            {
                //Generate city not found log message and log into log file.
                var cityNotFoundLogMessage = Config.SiteURL + stateCode + "/" + city + " - City Doesn't exist in our system";
                AppLogger.Instance.Log(cityNotFoundLogMessage, LogType.Info, null, true);
                throw new HttpException(404, "Page Not Found");
            }

            // we have matched cityName from Db that match the city name in URL, use it in further code
            city = cityNameFromDb;

            stateCode = stateCode.Trim();
            //Get Global Hire value from Admin table
            int GlobalHire = _businessMangementService.GetGlobalHiring().GlobalHire;
            int ReviewFilterValue = _businessMangementService.GetReviewsFilter().SelectedFilterValue;
            CompanyVM companyVM = new CompanyVM();
            //call function for check filter is null or not and after set values of filter into searchfilterVM object
            companyVM.SearchFilter = new SearchFilterVM();
            companyVM.hdnCityName = city;

            companyVM.SearchFilter = _homepageService.CheckFilterValuesNullOrNot(filter1, filter2, filter3, filter4, filter5, filter6, filter7, filter8, filter9);
            var pageName = "Citypage";
            //set page title
            companyVM.StateName = _homepageService.GetStateName(stateCode);
            companyVM.PageTitle = _homepageService.GetPageTitle(pageName, companyVM.StateName, city);
            //set statecode to set in url
            companyVM.StateCode = stateCode;
            //create new Url if user search or filter anything
            var newUrl = _homepageService.CreateNewUrl(stateCode, city, SearchText, filter1, filter2, filter3, filter4, filter5, filter6, filter7, filter8, filter9); ;
            //check if request is ajax or not
            isRestrictResultToCity = isRestrictResultToCity.HasValue ? isRestrictResultToCity : true;
            if (Request.IsAjaxRequest())
            {
                try
                {
                    companyVM.SearchFilter.SearchText = SearchText;
                    //set updated cityname from searchtextbox
                    if (!string.IsNullOrEmpty(SearchText) && SearchText.Contains(","))
                    {
                        //get city
                        companyVM.City = SearchText.Split(',')[0];

                        //get state
                        companyVM.StateCode = SearchText.Split(',')[1].Trim();
                    }
                    //get servicetype checkbox list from servicetype table of database  set into view
                    companyVM.SearchFilter.ServiceTypeList = _homepageService.GetServiceTypes();
                    //get cargotype checkbox list from cargotype table of database and set into view
                    companyVM.SearchFilter.CargoTypeList = _homepageService.GetCargoTypes();
                    //Binding LoadType for City Page
                    companyVM.GetAQuoteVM = new GetAQuoteVM();
                    companyVM.GetAQuoteVM.ListLoad = _homepageService.GetDropDownListForLoadType();

                    //get trailertype checkbox list from trailertype table of database  set into view
                    companyVM.SearchFilter.TrailerTypeList = _homepageService.GetTrailerTypesForFilter();

                    //get drivertype checkbox list from drivertype table of database and set into view
                    companyVM.SearchFilter.DriverTypeList = _homepageService.GetDriverTypesForFilter();

                    bool isHiringCheckboxIsChecked = SessionManager.Instance.IsHiringCheckboxIsChecked;
                    bool isReviewsFilterCheckboxIsChecked = SessionManager.Instance.IsReviewsFilterCheckboxIsChecked;

                    //check here if any filter contains map position or not
                    //if it contains boundry values then get those companies which are coming under this region(position values)
                    if ((!string.IsNullOrEmpty(filter1) && filter1.Contains("pos") && !filter1.Contains("lst")) ||
                                   (!string.IsNullOrEmpty(filter2) && filter2.Contains("pos") && !filter2.Contains("lst")) ||
                                   (!string.IsNullOrEmpty(filter3) && filter3.Contains("pos")) && !filter3.Contains("lst") ||
                                   (!string.IsNullOrEmpty(filter4) && filter4.Contains("pos")) && !filter4.Contains("lst") ||
                                   (!string.IsNullOrEmpty(filter5) && filter5.Contains("pos")) && !filter5.Contains("lst") ||
                                   (!string.IsNullOrEmpty(filter6) && filter6.Contains("pos")) && !filter6.Contains("lst") ||
                                   (!string.IsNullOrEmpty(filter7) && filter7.Contains("pos")) && !filter7.Contains("lst") ||
                                   (!string.IsNullOrEmpty(filter8) && filter8.Contains("pos")) && !filter8.Contains("lst") ||
                                   (!string.IsNullOrEmpty(filter9) && filter9.Contains("pos")) && !filter9.Contains("lst"))
                    {
                        //get companies under given position
                        companyVM.Companies = _homepageService.GetCompanyListFromFilter(companyVM.SearchFilter, stateCode, city, ps, true, isRestrictResultToCity.Value, isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);
                        companyVM.NewUrl = newUrl;
                        companyVM.City = city.Replace("-", " ");
                        companyVM.hdnCityName = city;
                        return Json(companyVM, JsonRequestBehavior.AllowGet);
                    }
                    //if user click on list button from map view
                    else if ((!string.IsNullOrEmpty(filter1)) && filter1.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter2)) && filter2.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter3)) && filter3.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter4)) && filter4.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter5)) && filter5.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter6)) && filter6.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter7)) && filter7.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter8)) && filter8.Contains("lst") ||
                        (!string.IsNullOrEmpty(filter9)) && filter9.Contains("lst"))
                    {
                        //get companies under given position
                        companyVM.Companies = _homepageService.GetCompanyListFromFilter(companyVM.SearchFilter, stateCode, city, ps, false, true, isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);
                        companyVM.NewUrl = newUrl;
                        var totalCompanies = companyVM.Companies.Pagination.TotalCount;
                        //return json with partial view and also send new updated url into response
                        return Json(new { NewUrl = "/" + newUrl, companyVM = companyVM, TotalCompanies = totalCompanies, AjaxReturn = ConvertViewToString("~/Views/Home/CompanyListPartial.cshtml", companyVM) });
                    }
                    else
                    {
                        //get company list
                        companyVM.Companies = _homepageService.GetCompanyListFromFilter(companyVM.SearchFilter, stateCode, city, ps, false, false, isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);
                        var totalCompanies = companyVM.Companies.Pagination.TotalCount;
                        //return json with partial view and also send new updated url into response
                        return Json(new { NewUrl = "/" + newUrl, companyVM = companyVM, TotalCompanies = totalCompanies, AjaxReturn = ConvertViewToString("~/Views/Home/CompanyListPartial.cshtml", companyVM) });
                    }
                }
                catch (Exception ex)
                {
                    return ReturnExceptionResult(ex);
                }
            }
            else
            {
                //we are storing StateCode, City and search values temporarily here and after generating url again we set it to same variable
                //While setting url it will clear that values so we assign same values again to variables
                var tempState = stateCode;
                var tempCity = city;
                var tempSearchText = SearchText;
                //create new Url if user search or filter anything
                newUrl = _homepageService.CreateNewUrl(stateCode, city, SearchText, filter1, filter2, filter3, filter4, filter5, filter6, filter7, filter8, filter9);
                stateCode = tempState;
                city = tempCity;
                SearchText = tempSearchText;
            }
            //if user enter url into other tab or browser 
            //then check if it is for map or not
            if ((!string.IsNullOrEmpty(filter1) && (filter1.Contains("pos") || filter1.Contains("lst"))) ||
                (!string.IsNullOrEmpty(filter2) && (filter2.Contains("pos") || filter2.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter3) && (filter3.Contains("pos") || filter3.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter4) && (filter4.Contains("pos") || filter4.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter5) && (filter5.Contains("pos") || filter5.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter6) && (filter6.Contains("pos") || filter6.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter7) && (filter7.Contains("pos") || filter7.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter8) && (filter8.Contains("pos") || filter8.Contains("lst"))) ||
               (!string.IsNullOrEmpty(filter9) && (filter9.Contains("pos") || filter9.Contains("lst")))
               )
            {
                //if this url for map then in the first request do nothing with database
            }
            else       // if user go to city page after state page then get companies from database
            {
                bool isHiringCheckboxIsChecked = SessionManager.Instance.IsHiringCheckboxIsChecked;
                bool isReviewsFilterCheckboxIsChecked = SessionManager.Instance.IsReviewsFilterCheckboxIsChecked;

                //get all companies of selected city
                companyVM.Companies = _homepageService.GetCompanyListFromFilter(companyVM.SearchFilter, stateCode, city, ps, false, false, isHiringCheckboxIsChecked, GlobalHire, isReviewsFilterCheckboxIsChecked, ReviewFilterValue);
            }
            companyVM.hdnCityName = city;
            //set city to set in url
            companyVM.City = city.Replace("-", " ");

            //get cargotype checkbox list from cargotype table of database and set into view
            companyVM.SearchFilter.CargoTypeList = _homepageService.GetCargoTypes();

            //get servicetype checkbox list from servicetype table of database  set into view
            companyVM.SearchFilter.ServiceTypeList = _homepageService.GetServiceTypes();

            //get trailertype checkbox list from trailertype table of database  set into view
            companyVM.SearchFilter.TrailerTypeList = _homepageService.GetTrailerTypesForFilter();

            //get drivertype checkbox list from drivertype table of database and set into view
            companyVM.SearchFilter.DriverTypeList = _homepageService.GetDriverTypesForFilter();

            // Get outbound banner for Homepage
            companyVM.OutboundBanner = _homepageService.GetOutboundBanner((byte)OutboundBannerPageLevelEnum.City);

            //Binding LoadType for City Page
            companyVM.GetAQuoteVM = new GetAQuoteVM();
            companyVM.GetAQuoteVM.ListLoad = _homepageService.GetDropDownListForLoadType();

            // City data module — page 1 only, non-map requests
            {
                var pageParam = Request.QueryString["p"];
                bool isPage1 = string.IsNullOrEmpty(pageParam) || pageParam == "1";
                var filterArr = new[] { filter1, filter2, filter3, filter4, filter5, filter6, filter7, filter8, filter9 };
                // "pos-..." = map view: module renders but starts hidden (client toggle shows it on List)
                bool isMapRequest = System.Array.Exists(filterArr, f => f != null && f.StartsWith("pos-", StringComparison.OrdinalIgnoreCase));
                ViewBag.CityModuleHidden = isMapRequest;
                if (isPage1)
                {
                    try
                    {
                        var cityModule = _homepageService.GetCityCompaniesData(stateCode, city, "24m");
                        if (cityModule != null && cityModule.TotalActiveCompanies >= 50)
                        {
                            ViewModels.Admin.StatisticsCargoVM          cargoBaseline  = null;
                            ViewModels.Admin.StatisticsIndexVM           statsBaseline  = null;
                            ViewModels.Admin.StatisticsFleetOperationsVM fleetBaseline  = null;
                            ViewModels.Admin.StatisticsNewRegistrationsVM newRegBaseline = null;
                            try { cargoBaseline  = _homepageService.GetCargoData(); }            catch { }
                            try { statsBaseline  = _homepageService.GetStatisticsData(); }       catch { }
                            try { fleetBaseline  = _homepageService.GetFleetOperationsData(); }  catch { }
                            try { newRegBaseline = _homepageService.GetNewRegistrationsData("12m"); } catch { }
                            ViewBag.CityModule = cityModule;
                            ViewBag.CityModuleHighlights = PartnerCarrier.Web.Helpers.CityModuleHelper.BuildHighlights(
                                cityModule, cargoBaseline, statsBaseline, fleetBaseline, newRegBaseline);
                        }
                    }
                    catch { }
                }
            }

            return View(companyVM);
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
        #endregion

        #region Company Information Page
        /// <summary>
        /// display company information from database
        /// set page title as required
        /// set page description as required
        /// </summary>
        /// <returns></returns>
        public ActionResult Company(string statecode, string city, string companyNameForUrl, int usdotnumber)
        {
            //create company URl here to enter proper app-log if company not found
            //var companyURL = statecode + "/" + companyNameForUrl + "-USDOT-" + usdotnumber; Arkady Feb 23, 2024
            var companyURL = statecode + "/USDOT-" + usdotnumber; // New format of company url
            CompanyInformationVM companyInformationVM = new CompanyInformationVM();
            //get company information
            companyInformationVM = _homepageService.GetCompanyInformation(usdotnumber, companyURL);

            // If city and company name are present in url then redirect to new url 
            if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(companyNameForUrl))
            {
                // Get the base URL of the current request
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

                // Construct the new URL for redirection
                string newUrl = GenerateCompanyNewUrl(companyInformationVM.PhysicalAddressStateCode, usdotnumber, baseUrl);

                // If details does not match, permanentally  redirect it to new URL.
                return RedirectPermanent(newUrl);
            }

            // If URL state and actula compant state not match then redirect to new url
            if (!string.IsNullOrEmpty(statecode) && !string.IsNullOrEmpty(companyInformationVM.PhysicalAddressStateCode) && statecode != companyInformationVM.PhysicalAddressStateCode)
            {
                // Get the base URL of the current request
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

                // Construct the new URL for redirection
                string newUrl = GenerateCompanyNewUrl(companyInformationVM.PhysicalAddressStateCode, usdotnumber, baseUrl);

                // If details does not match, permanentally  redirect it to new URL.
                return RedirectPermanent(newUrl);
            }

            companyInformationVM.StateName = _homepageService.GetStateName(statecode);
            //set browser title
            companyInformationVM.PageTitle = _homepageService.SetPageTitleForCompanyInformation(usdotnumber);
            //set page description
            companyInformationVM.PageDescription = _homepageService.SetPageDescriptionForCompanyInformation(usdotnumber);

            // Get outbound banner for Company page
            companyInformationVM.OutboundBanner = _homepageService.GetOutboundBanner((byte)OutboundBannerPageLevelEnum.Company);

            //Binding LoadType for COmpany Page
            companyInformationVM.GetAQuoteVM = new GetAQuoteVM();
            companyInformationVM.GetAQuoteVM.ListLoad = _homepageService.GetDropDownListForLoadType();

            // Get ratings for company using USDOTNumber
            var companyRating = _homepageService.GetCompanyRating(usdotnumber);

            companyInformationVM.AverageRating = companyRating.AverageRating;
            companyInformationVM.TotalReviews = companyRating.TotalReviews;


            //return company information
            return View(companyInformationVM);
        }
        #endregion

        #region Search 
        /// <summary>
        /// user can search companies by city , usdot number and mc number
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="selectedValue"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Search(string searchText, string selectedValue, PageSortPara ps)
        {
            try
            {
                int GlobalHire = _businessMangementService.GetGlobalHiring().GlobalHire;
                bool isHiringCheckboxIsChecked = SessionManager.Instance.IsHiringCheckboxIsChecked;

                var info = _homepageService.GetSearchRedirectInfo(searchText, selectedValue, isHiringCheckboxIsChecked, GlobalHire);

                // No match - either the city/company/USDOT genuinely doesn't exist, or (when the
                // hiring checkbox is on) it exists but has zero companies currently marked Now
                // Hiring. Previously this fell through to info[0] on an empty list, which threw
                // an unhandled IndexOutOfRangeException: every single "no results" search
                // returned an HTTP 500 (read client-side as a generic error) AND fired an
                // exception-report email to the site admin. Return a clean null result instead
                // so callers can show an accurate, specific message.
                if (info == null || info.Count == 0)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                var row = info[0];

                if (string.IsNullOrEmpty(selectedValue) || selectedValue == "City")
                    return Json("/" + row.PhysicalAddressStateCode + "/" + row.PhysicalAddressCity, JsonRequestBehavior.AllowGet);
                else
                    return Json(GenerateCompanyNewUrl(row.PhysicalAddressStateCode, row.USDOTNumber, ""), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// City search used only by the Truck Driver Jobs page. Checks whether a city has at
        /// least one Now Hiring company and, if so, returns that city's normal URL - same as
        /// Search() above, but deliberately passes isHiringCheckboxIsChecked: true as a literal
        /// argument instead of reading/writing SessionManager.Instance.IsHiringCheckboxIsChecked.
        /// This must have zero effect on the navbar's Companies Now Hiring checkbox or on any
        /// other page's filtering; it only answers "does this city have a hiring company right
        /// now" and hands back a plain city URL that renders using whatever the real checkbox
        /// state already is.
        /// </summary>
        [HttpPost]
        public ActionResult SearchHiringCity(string searchText)
        {
            try
            {
                int GlobalHire = _businessMangementService.GetGlobalHiring().GlobalHire;

                var info = _homepageService.GetSearchRedirectInfo(searchText, "City", true, GlobalHire);

                if (info == null || info.Count == 0)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                var row = info[0];
                return Json("/" + row.PhysicalAddressStateCode + "/" + row.PhysicalAddressCity, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// AutoComplete textbox after adding 3 character in the search textbox
        /// this method execute after user enter  character in search textbox
        /// </summary>
        /// <param name="id"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchAutoComplete(AutoCompleteSearchRequestVM AutoCompleteSearchRequestVM)
        {
            try
            {
                var searchResultList = new List<SearchVM>();
                if (AutoCompleteSearchRequestVM.SelectedValue == "City")
                    //get search result from seleted ropdown value
                    searchResultList = _homepageService.GetSearchResultAutoComplete(AutoCompleteSearchRequestVM.SelctedSearchPrefix, AutoCompleteSearchRequestVM.SelectedValue);
                else if (AutoCompleteSearchRequestVM.SelectedValue == "Company Name")
                {
                    searchResultList = _homepageService.GetSearchResultAutoComplete(AutoCompleteSearchRequestVM.SelctedSearchPrefix, AutoCompleteSearchRequestVM.SelectedValue);
                }
                return Json(searchResultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// AutoComplete textbox after adding 3 character in the search textbox
        /// this method execute after user enter  character in search textbox
        /// </summary>
        /// <param name="id"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchAutoCompleteCity(AutoCompleteSearchRequestVM AutoCompleteSearchRequestVM)
        {
            try
            {
                //get search result from seleted ropdown value
                var searchResultList = _homepageService.GetSearchResultAutoCompleteCity(AutoCompleteSearchRequestVM.SelctedSearchPrefix, AutoCompleteSearchRequestVM.SelectedValue);
                return Json(searchResultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        #endregion

        #region Privacy and Terms of use page
        /// <summary>
        /// Display Privacy Notice Page
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplayPrivacyNotice()
        {
            return View("PrivacyNoticePage");
        }

        /// <summary>
        /// display terms of use page
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplayTermsOfUse()
        {
            return View("TermsOfUsePage");
        }
        #endregion

        #region Convert View To string
        /// <summary>
        /// Convert View To string 
        /// this required after filter in company list,
        /// after filter applied we are returning JSON with partial view, for that we will have to convert partial view to string
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Convert View To string 
        /// this required after filter in company list,
        /// after filter applied we are returning JSON with partial view, for that we will have to convert partial view to string
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        private string ConvertViewToStringForReviews(string viewName)
        {
            var physicalPath = viewName.StartsWith("~") ? Server.MapPath(viewName) : viewName;
            if (!System.IO.File.Exists(physicalPath))
                throw new FileNotFoundException($"HTML file not found: {physicalPath}");
            return System.IO.File.ReadAllText(physicalPath);
        }

        #endregion

        #region Business Login

        [Route("login")]
        public ActionResult BusinessLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [Route("login")]
        public ActionResult BusinessLogin(BusinessLoginVM businessLoginVM, string returnUrl)
        {
            try
            {
                SignInStatus result = _businessMangementService.BusinessLogin(businessLoginVM);
                switch (result)
                {
                    //If successfully login
                    case common.SignInStatus.Success:
                        //return RedirectToLocal(returnUrl);
                        var redirectUrl = ReturnRedirectToLocalURL(returnUrl);
                        return Json(redirectUrl, JsonRequestBehavior.AllowGet);
                    //if user Inactive , Then It shows message
                    case common.SignInStatus.Inactive:
                        throw new BusinessException("400", "Your account is inactive, please contact site administrator.");
                    case common.SignInStatus.TooManyAttempt:
                        throw new BusinessException("400", "You entered wrong password too many times. Please contact to Administrator.");
                    //If user Does not exist
                    case common.SignInStatus.Failure:
                    default:
                        throw new BusinessException("400", "Invalid login attempt.");
                }
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }


        // POST: /Account/LogOff
        [HttpPost]
        [AuthorizeRole(UserRole.BusinessUser)]
        [ValidateAntiForgeryToken]
        [Route("log-out")]
        public ActionResult LogOff()
        {
            FormsAuthService.Instance.LogOut(Config.GetValue("BusinessLoginAuthenticationName"), false);
            HttpContext.ClearError();
            return RedirectToAction("Index", "Home");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToHomePage();
        }
        //for return url after json  login.        
        private string ReturnRedirectToLocalURL(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            return RedirectToBusinessLogineJsonURL();
        }

        #endregion

        [Route("forgot-password")]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("forgot-password")]
        public ActionResult ForgotPassword(BusinessForgotPasswordVM businessForgotPasswordVM)
        {
            try
            {
                businessForgotPasswordVM.IsEmailAddressCheck = true;
                _businessMangementService.BussinessResetPasswordMailSend(businessForgotPasswordVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

        }

        [Route("business-forgot-password-success")]
        public ActionResult BusinessForgotPasswordSuccess()
        {
            ViewBag.SuccesMessageAfteEmailVerified = "Reset password link is sent in mail please reset your password from mail. Click <a href='/'>here</a> to go to home page";
            return View("BusinessResetPasswordSuccess");
        }

        [Route("edit-business-profile")]
        public ActionResult EditBusinessProfile()
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));
            BusinessVM businessVM = new BusinessVM();
            businessVM = _homepageService.GetBusinessDetailsByUSDOTNumber(loggedInUser.USDOTNumber);
            //Get Trailer Type and Driver Type 
            businessVM.TrailerTypeList = _homepageService.GetTrailerTypes();
            businessVM.DriverTypeList = _homepageService.GetDriverTypes();
            return View("CompanyBusiness", businessVM);
        }

        #endregion

        /// <summary>
        /// Get Business By USDOTNumber
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Business(int? USDOTNumber)
        {
            BusinessVM businessVM = new BusinessVM();
            businessVM = _homepageService.GetCompanyEmailAddress(USDOTNumber);

            //Get Trailer Type and Driver Type 
            businessVM.TrailerTypeList = _homepageService.GetTrailerTypes();
            businessVM.DriverTypeList = _homepageService.GetDriverTypes();
            return View("CompanyBusiness", businessVM);
        }

        /// <summary>
        /// Create Account
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <returns></returns>
        public ActionResult CreateAccount(int? USDOTNumber)
        {
            BusinessVM businessVM = new BusinessVM();
            businessVM = _homepageService.GetCompanyEmailAddress(USDOTNumber);
            return View("CreateAccount", businessVM);
        }

        /// <summary>
        /// Save Business Details
        /// </summary>
        /// <param name="businessVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateAccount(BusinessVM businessVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();
            var url = new CompanyVM();
            try
            {
                //Save Create Account Details 
                _homepageService.SaveAccountDetails(businessVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Get Company Business
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CompanyBusiness(int? USDOTNumber)
        {
            BusinessVM businessVM = new BusinessVM();
            businessVM = _homepageService.GetCompanyEmailAddress(USDOTNumber);
            return View(businessVM);
        }

        /// <summary>
        /// Change Password Get method
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChangePassword(int USDOTNumber)
        {
            BusinessVM businessVM = new BusinessVM();
            businessVM = _homepageService.GetCompanyEmailAddress(USDOTNumber);
            return View(businessVM);
        }

        /// <summary>
        /// Change Password 
        /// </summary>
        /// <param name="businessVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangePassword(BusinessVM businessVM)
        {
            try
            {
                _homepageService.ChangePassword(businessVM);

                var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));
                if (loggedInUser == null) // if logged in user in null then send USDOTNumber in response for redirect on previous page
                    return Json(businessVM.USDOTNumber, JsonRequestBehavior.AllowGet);
                else
                    return Json("", JsonRequestBehavior.AllowGet); // If loggedInUser has value then just send blank in response fore redirection using logged in user value.
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Save Business Details
        /// </summary>
        /// <param name="businessVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Business(BusinessVM businessVM)
        {
            if (!ModelState.IsValid && ModelState.IsValidField("WebsiteName") && ModelState.IsValidField("Password") && ModelState.IsValidField("ConfirmPassword")) return ReturnModelStateErrors();
            var url = new CompanyVM();
            try
            {
                var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

                _homepageService.SaveCompanyBusinessDetails(businessVM);

                if (loggedInUser == null)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(loggedInUser, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [Route("business-saved-successfully")]
        public ActionResult BusinessSavedSuccessfully()
        {
            return View();
        }

        /// <summary>
        /// Display Account Create Successfull Message
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("account-create-successfully")]
        public ActionResult AccountCreatedSuccessfully()
        {
            return View();
        }

        /// <summary>
        /// Verify link by email
        /// </summary>
        /// <param name="verifyLinkGUID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EmailVerify(Guid? verifyLinkGUID)
        {
            try
            {
                var uSDOTNumber = _homepageService.VerifyEmail(verifyLinkGUID.Value);
                ViewBag.USDOTNumber = uSDOTNumber;
                var sitePath = Config.SiteURL + "business/" + uSDOTNumber;

                //show alert successful message after submit information
                ViewBag.SuccesMessageAfteEmailVerified = " Your account was successfully created, thank you for using TruckCarrierHub.com \n Click <a href=" + sitePath + ">here</a> to manage your account and post your job openings";
            }
            catch (Exception ex)
            {
                ReturnExceptionResult(ex);
                //set error message in viewbag
                ViewBag.EmailVerifiedFailureErrorMesaage = " Sorry, something went wrong. Please contact us for more details.";
            }
            return View();
        }

        [HttpGet]
        [Route("email/unsubscribe/{USDOTNumber}")]
        public ActionResult Unsubscribe(int USDOTNumber)
        {
            _businessMangementService.UnSubscribeEmail(USDOTNumber);

            return RedirectToAction("UnSubscribeThankYouPage");
        }

        [Route("unsubscribed-sucessfully")]
        public ActionResult UnSubscribeThankYouPage()
        {
            return View();

        }

        /// <summary>
        /// display terms of use page
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplaySuccessStory()
        {
            //Get Success Story here
            var successStory = _businessMangementService.GetSuccessStory();
            return View("SuccessStoryPage", successStory);
        }

        #region Error Page
        /// <summary>
        /// Error Page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Error()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["msg"]))
                SetErrorPageInfo(Request.QueryString["msg"].ToString());
            else
                SetErrorPageInfo(null);
            return View("Error_Page");

        }

        /// <summary>
        /// sets error text to be set for the label
        /// </summary>
        /// <param name="strMessageType">Message type passed in the query-string</param>        
        public void SetErrorPageInfo(string messageType)
        {
            var errorTitle = "Unhandled Error Occurred";
            var errorMsg = "Unhandled Error Occurred,Please Contact Site Administrator.";
            if (messageType != null)
            {
                // check for the message type and return the text according to that
                switch (messageType)
                {
                    case "errAcc":
                        {
                            errorTitle = "Unauthorized Access";
                            errorMsg = "You are not authorized to access the page you are trying to access";
                            break;
                        }
                    case "errHtml":
                        {
                            errorTitle = "Dangerous Script Entered";
                            errorMsg = "You have used HTML tags in your input text.";
                            break;
                        }
                    case "errNotFound":
                        {
                            errorTitle = "404. Page Not Found";
                            errorMsg = "Page you are looking for is not found";
                            break;
                        }
                    case "BadReq":
                        {
                            errorTitle = "400. Bad Request";
                            errorMsg = "Bad Request or Invalid URL entered";
                            break;
                        }
                    case "countyDbConnectionError":
                        {
                            errorTitle = "Connection Error";
                            errorMsg = "Unable to connect with county database";
                            break;
                        }
                }
            }

            ViewBag.ErrorTitle = errorTitle;
            ViewBag.ErrorMessage = errorMsg;
        }
        #endregion


        #region Business

        /// <summary>
        /// Business password Verify
        /// </summary>
        /// <param name="resetPasswordKey"></param>
        /// <returns></returns>
        [Route("reset-password/{resetPasswordKey}")]
        public ActionResult BusinessResetPasswordVerify(Guid? resetPasswordKey)
        {
            var businessResetPasswordVM = _businessMangementService.CheckResetPasswordKeyIsValid(resetPasswordKey);
            businessResetPasswordVM.ResetPasswordKey = resetPasswordKey.ToString();
            return View(businessResetPasswordVM);
        }

        [HttpPost]
        [Route("save-reset-password")]
        public ActionResult ResetPasswordSubmit(BusinessVM businessVM)
        {
            if (!ModelState.IsValid && ModelState.IsValidField("WebsiteName") && ModelState.IsValidField("Password") && ModelState.IsValidField("ConfirmPassword")) return ReturnModelStateErrors();

            try
            {
                _businessMangementService.BussinessResetPassword(businessVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [Route("business-reset-password-success")]
        public ActionResult BusinessResetPasswordSuccess()
        {
            ViewBag.SuccesMessageAfteEmailVerified = " Your password was successfully updated, \n Click <a href='/'>here</a> to go home page";
            return View();
        }

        #endregion

        [HttpPost]
        public ActionResult StoreIsCheckCheckBoxValue(bool isHiringCheckboxCheck)
        {
            try
            {
                if (isHiringCheckboxCheck)
                {
                    Session["IsHiringCheckboxIsChecked"] = true;
                }
                else
                {
                    Session["IsHiringCheckboxIsChecked"] = false;
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        public ActionResult StoreReviewsFilterCheckBoxValue(bool isReveiewsCheckboxCheck)
        {
            try
            {
                if (isReveiewsCheckboxCheck)
                {
                    Session["IsReviewsFilterCheckboxIsChecked"] = true;
                }
                else
                {
                    Session["IsReviewsFilterCheckboxIsChecked"] = false;
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        public ActionResult GetAQuotePage(GetAQuoteVM getAQuoteVM)
        {
            //Binding dropdowns to view model
            getAQuoteVM.ListLoad = _homepageService.GetDropDownListForLoadType();
            getAQuoteVM.ListPickupLocationType = _homepageService.GetDropDropdownForPickupLocationType(getAQuoteVM.LoadType);
            getAQuoteVM.ListDeliveryLocationType = _homepageService.GetDropdownForDeliveryLocationType(getAQuoteVM.LoadType);
            var PickupLocationTypeId = getAQuoteVM.ListPickupLocationType.FirstOrDefault().Id;
            var DeliveryLocationTypeId = getAQuoteVM.ListDeliveryLocationType.FirstOrDefault().Id;

            getAQuoteVM.PickupSpecialHandlingList = _homepageService.GetCheckBoxList("Pickup", getAQuoteVM.LoadType, PickupLocationTypeId);
            getAQuoteVM.DeliverySpecialHandlingList = _homepageService.GetCheckBoxList("Delivery", getAQuoteVM.LoadType, DeliveryLocationTypeId);

            if (getAQuoteVM.LoadType == "LTL" || getAQuoteVM.LoadType == "FTL/Rail")
            {
                getAQuoteVM.LoadItemTypeList = _homepageService.GetDropDownForLoadItemType();
                getAQuoteVM.RefrigerationList = _homepageService.GetDropDownForRefrigerationType(getAQuoteVM.LoadType);
                if (getAQuoteVM.LoadType == "FTL/Rail")
                {
                    getAQuoteVM.TemperatureList = _homepageService.GetDropdownForTemperatureType();
                    getAQuoteVM.LoadInfoList = _homepageService.GetDropDownForLoadInfo();
                }
                else
                {
                    getAQuoteVM.LoadClassList = _homepageService.GetDropDownForLoadClass();
                }
            }
            if (getAQuoteVM.LoadType == "Flatbed" || getAQuoteVM.LoadType == "FTL/Rail")
            {
                getAQuoteVM.TruckTypeList = _homepageService.GetDropDownForTruckType(getAQuoteVM.LoadType);
            }
            if (getAQuoteVM.LoadType == "Container")
            {
                getAQuoteVM.LoadContainerTypeList = _homepageService.GetDropDownForLoadContainerType();
                getAQuoteVM.LoadContainerLengthList = _homepageService.GetDropDownForLoadContainerLength();
            }

            ModelState.Clear();
            getAQuoteVM.PickupDate = DateTime.Now; //we set current date as default date.

            if (Request.IsAjaxRequest())
            {
                var url = "getaquote?loadType=" + getAQuoteVM.LoadType + "&PickupLocation=" + getAQuoteVM.PickupLocation + "&DeliveryLocation=" + getAQuoteVM.DeliveryLocation + "&OriginURL=" + getAQuoteVM.OriginURL;
                return Json(new { url = url, pickupCity = getAQuoteVM.PickupLocation, deliveryCity = getAQuoteVM.DeliveryLocation, originURL = getAQuoteVM.OriginURL }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View("~/Views/Home/GetAQuotePage.cshtml", getAQuoteVM);
            }
        }

        /// <summary>
        /// Submit Get a quote details
        /// </summary>
        /// <param name="getAQuoteVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitGetAQuotePage(GetAQuoteVM getAQuoteVM)
        {
            //gettting string date then parse to Exact date format and set to view model property after converting to date format
            getAQuoteVM.PickupDate = DateTime.ParseExact(getAQuoteVM.StringPickupDate, "MM/dd/yyyy", null);

            if (getAQuoteVM.LoadType == "FTL/Rail" || getAQuoteVM.LoadType == "Container")
            {
                ModelState.Remove("getAQuoteVM.LoadInformationVM.DimentionLength");
                ModelState.Remove("getAQuoteVM.LoadInformationVM.DimentionWidth");
                ModelState.Remove("getAQuoteVM.LoadInformationVM.DimentionHeight");
            }
            if (!ModelState.IsValid) return ReturnModelStateErrors();

            //Send Email based on User details match to those carriers
            getAQuoteVM = _homepageService.submitGetAQuoteDetails(getAQuoteVM);

            var urlForCallsenEmailForQuote = Config.SiteURL + $"SendEmailsForQuote/" + getAQuoteVM.QuoteId;
            var _ = Task.Run(() => WebRequestUtil.DownloadPageAsync(urlForCallsenEmailForQuote));

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// We created this seperate method to send email to carrier by quote id seperately for maintaining response and mail sending side by side.
        /// </summary>
        /// <param name="quoteId">quote Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendEmailsForQuote(int quoteId)
        {
            _homepageService.SendEmailsForQuote(quoteId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [Route("quote-submited-sucessfully")]
        public ActionResult QuoteSubmitConfirmationPage()
        {
            return View();
        }

        public ActionResult GetCheckboxListFromLocationType(int locationTypeId, string locationType, string loadType)
        {
            return Json(_homepageService.GetCheckBoxListFromLocationType(locationTypeId, locationType, loadType), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get banner image file from the server.
        /// </summary>
        /// <param name="fileName">The name of the banner image file.</param>
        /// <returns>Banner image.</returns>
        [HttpGet]
        [Route("get-banner-image")]
        public ActionResult GetBannerImage(string fileName)
        {
            try
            {
                // Combine the site path and the configured outbound banner file path to get the full file path
                string outboundBannerFilePath = Path.Combine(Config.SitePath, Config.GetValue("OutboundBannerFilePath"));
                string bannerImagePath = Path.Combine(outboundBannerFilePath, fileName);

                // Return the banner image file as a file result with MIME type "image/jpg"
                return File(bannerImagePath, "image/jpg");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Constructs the new URL for a company based on the state code and USDOT number.
        /// </summary>
        /// <param name="stateCode">The state code of the company.</param>
        /// <param name="dotNumber">The USDOT number of the company.</param>
        /// <param name="baseUrl">Base url</param>
        /// <returns>The new URL for the company.</returns>
        private string GenerateCompanyNewUrl(string stateCode, int dotNumber, string baseUrl)
        {
            // Construct and return the new URL for the company
            return baseUrl + "/" + stateCode + "/USDOT-" + dotNumber;
        }

        #region Company Review

        /// <summary>
        /// Displays the form for adding a new company review.
        /// </summary>
        /// <param name="companyUSDOT"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddCompanyReview(int companyUSDOT)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            AddCompanyReviewVM data = new AddCompanyReviewVM();
            data.CompanyUSDOT = companyUSDOT;
            data.ReviewerUSDOT = (int)loggedInUser.USDOTNumber;

            return PartialView("_AddReview", data);
        }

        /// <summary>
        /// To add a new company review.
        /// </summary>
        /// <param name="addCompanyReviewVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCompanyReview(AddCompanyReviewVM addCompanyReviewVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();

            try
            {
                _homepageService.AddCompanyReview(addCompanyReviewVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Returns the edit view for an existing company review.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public ActionResult EditCompanyReview(int reviewId)
        {
            var review = _homepageService.GetReviewDetailsById(reviewId);
            review.IsReview = true;
            return PartialView("_WriteReviewPartial", review);
        }

        /// <summary>
        /// Updates an existing company review.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="addCompanyReviewVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateCompanyReview(int reviewId, AddCompanyReviewVM addCompanyReviewVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();

            try
            {
                _homepageService.UpdateCompanyReview(reviewId, addCompanyReviewVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Retrieves the list of reviews for a company, optionally sorted.
        /// </summary>
        /// <param name="companyUSDOT"></param>
        /// <param name="sortDir"></param>
        /// <returns></returns>
        public ActionResult GetReviewsList(int companyUSDOT, int sortDir = 0)
        {
            var data = _homepageService.GetReviewsList(companyUSDOT, sortDir);
            return PartialView("_ReviewsSummaryPartial", data);
        }

        /// <summary>
        /// Returns the add review view for a logged-in user, or a sign-in prompt if not logged in.
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public ActionResult OnClickAddReview(int companyId)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            if (loggedInUser != null)
            {


                var model = new AddCompanyReviewVM
                {
                    CompanyUSDOT = companyId,
                    ReviewerUSDOT = (int)loggedInUser.USDOTNumber
                };

                var data = _homepageService.GetCompanyandReviewerName(model.CompanyUSDOT, 0, model.ReviewerUSDOT);

                model.CompanyName = data.CompanyName;
                model.ReviewerName = data.ReviewerName;

                return PartialView("_WriteReviewPartial", model);
            }
            else
            {
                return PartialView("~/Views/Shared/_SignInPartial.cshtml");
            }
        }

        /// <summary>
        /// Returns the add reply view for a review, or a sign-in prompt if not logged in.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public ActionResult OnClickAddReviewReply(int companyId, int reviewId)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            if (loggedInUser != null)
            {
                var model = new AddEditReviewReplyVM
                {
                    CompanyUSDOT = companyId,
                    ReviewId = reviewId,
                };

                var data = _homepageService.GetCompanyandReviewerName(model.CompanyUSDOT, reviewId, 0);

                model.CompanyName = data.CompanyName;
                model.ReviewerName = data.ReviewerName;

                return PartialView("_AddEditReviewResponePartial", model);
            }
            else
            {
                return PartialView("~/Views/Shared/_SignInPartial.cshtml");
            }
        }

        /// <summary>
        /// Adds or updates a review reply.
        /// </summary>
        /// <param name="addCompanyReviewVM"></param>
        /// <returns></returns>
        public ActionResult AddEditReviewResponse(AddEditReviewReplyVM addCompanyReviewVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();

            try
            {
                _homepageService.AddUpdateReviewResponse(addCompanyReviewVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Returns the edit view for an existing review reply.
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        public ActionResult EditReviewResponse(int responseId)
        {
            var response = _homepageService.GetResponseDetailsById(responseId);
            return PartialView("_AddEditReviewResponePartial", response);
        }

        #endregion

        #region Interactive Map

        /// <summary>
        /// Landing/instruction page for the interactive map feature (which already exists as
        /// the Map toggle on the City page). Gives the feature its own crawlable, linkable URL
        /// with a city search box; on submit, reuses the existing /search + /searchautocomplete
        /// endpoints (selectedValue hardcoded to "City") and appends ?view=map to the redirect
        /// target so City.cshtml's init script (see the "view=map" block near the bottom of that
        /// view) opens straight into Map view instead of the default List view.
        /// Also loads a small, capped (default page size, ~70 rows) live company list for
        /// Washington, DC to render an embedded example map, so users and Google can see what
        /// the map view looks like before picking a city themselves. This is a light,
        /// uncached query (same shape as the City page's default list query) - if it fails for
        /// any reason we log and fall back to the page with no example map rather than 500ing
        /// the whole landing page, since the example map is illustrative, not core content.
        /// </summary>
        [HttpGet]
        public ActionResult InteractiveMap()
        {
            ViewBag.Title = "Interactive Map " + System.Char.ConvertFromUtf32(0x2014) + " Search Trucking Companies by City | Truck Carrier Hub";

            List<CompanyVM> dcCompanies = new List<CompanyVM>();
            try
            {
                var dcCityName = _homepageService.GetCityNameFromURLCityName("WASHINGTON", "DC") ?? "WASHINGTON";
                var searchFilterVM = new SearchFilterVM();
                var ps = new PageSortPara { p = 1 };
                var result = _homepageService.GetCompanyListFromFilter(searchFilterVM, "DC", dcCityName, ps, false, true, false, 0, false, 0);
                if (result != null && result.Items != null)
                {
                    dcCompanies = result.Items.Where(c => c.Latitude.HasValue && c.Longitude.HasValue).ToList();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Log("InteractiveMap: failed to load DC example map companies - " + ex.Message, LogType.Error, null, true);
                dcCompanies = new List<CompanyVM>();
            }

            return View(dcCompanies);
        }

        /// <summary>
        /// AJAX endpoint backing the /interactive-map example map's pan/zoom refresh. Unlike the
        /// City page's equivalent (GetCompaniesAfterZoomOrDrag), this map isn't scoped to one
        /// city, so it reuses GetCompanyListFromFilter with an empty city/state and only
        /// BoundrieValues set: isRestrictResultToCity=true (server default when unset) plus a
        /// non-zero ZoomLevel together skip the exact city/state match branch inside that method
        /// and fall through to the lat/lng bounding-box filter only - the same mechanism the City
        /// page's own pan/zoom AJAX call relies on. Capped at 300 markers since a zoomed-out view
        /// can span a huge area; the map only ever needs to render what's currently on screen.
        /// </summary>
        [HttpPost]
        public ActionResult InteractiveMapBounds(double swLat, double swLng, double neLat, double neLng)
        {
            try
            {
                var searchFilterVM = new SearchFilterVM
                {
                    ZoomLevel = 10,
                    BoundrieValues = new BoundrieValuesVM
                    {
                        Southwest = new PartnerCarrier.ViewModels.User.southwest { lat = swLat, lng = swLng },
                        Northeast = new PartnerCarrier.ViewModels.User.northeast { lat = neLat, lng = neLng }
                    }
                };
                var ps = new PageSortPara { p = 1 };
                var result = _homepageService.GetCompanyListFromFilter(searchFilterVM, "", "", ps, true, true, false, 0, false, 0);

                var items = (result != null && result.Items != null)
                    ? result.Items
                        .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                        .Take(300)
                        .Select(c => new
                        {
                            Name = c.DoingBusinessAsName ?? c.LegalName,
                            Lat = c.Latitude,
                            Lng = c.Longitude,
                            USDOT = c.USDOTNumber,
                            State = c.StateCode,
                            Phone = c.OfficeTelephoneNumber,
                            Status = c.Status
                        })
                        .ToList()
                    : null;

                return Json(items, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Log("InteractiveMapBounds: failed to load companies for bounds - " + ex.Message, LogType.Error, null, true);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Landing/instruction page for the review system - explains both writing a review
        /// (search by USDOT number -> land on the company page -> submit a rating, logged-in
        /// business accounts only) and responding to a review as a company owner (log in first,
        /// then open your own company's page). No ViewModel/DB query backs this page - it's
        /// static instructional content plus a client-side USDOT search box.
        /// </summary>
        [HttpGet]
        public ActionResult Reviews()
        {
            ViewBag.Title = "Trucking Company Reviews " + System.Char.ConvertFromUtf32(0x2014) + " Read & Write Reviews | Truck Carrier Hub";
            return View();
        }

        /// <summary>
        /// Landing/instruction page for the "Now Hiring" feature. Deliberately does NOT frame
        /// this as a job board with individual postings - there is no job-posting table, just a
        /// NowHiring boolean flag on the Business record surfaced as a filter on city listings.
        /// Copy explains both sides: companies flip the flag from their business profile after
        /// logging in, and job seekers filter city results down to hiring companies only. The
        /// city search box on this page pre-sets the hiring filter in session (reusing the same
        /// /storeischeckcheckboxvalue endpoint the navbar checkbox uses) before redirecting via
        /// /search, so the destination city page opens already filtered to hiring companies.
        /// </summary>
        [HttpGet]
        public ActionResult TruckDriverJobs()
        {
            ViewBag.Title = "Truck Driver Jobs " + System.Char.ConvertFromUtf32(0x2014) + " Find Trucking Companies That Are Hiring | Truck Carrier Hub";
            return View();
        }

        #endregion

        #region Statistics

        [HttpGet]
        public ActionResult Statistics()
        {
            var vm = _homepageService.GetStatisticsData();
            ViewBag.Title = "Trucking Industry Statistics & Analytics | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult ActiveCompanies()
        {
            var vm = _homepageService.GetActiveCompaniesData();
            ViewBag.Title = "Active Trucking Companies Statistics " + System.Char.ConvertFromUtf32(0x2014) + " U.S. | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult ActiveBrokers()
        {
            var vm = _homepageService.GetActiveBrokersData();
            ViewBag.Title = "Active Freight Brokers Statistics " + System.Char.ConvertFromUtf32(0x2014) + " U.S. | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult StateCompanies(string stateCode)
        {
            if (string.IsNullOrWhiteSpace(stateCode))
                return RedirectToAction("ActiveCompanies");
            var vm = _homepageService.GetStateCompaniesData(stateCode.ToUpper());
            if (vm == null)
                return RedirectToAction("ActiveCompanies");
            ViewBag.Title = vm.StateName + " Trucking Statistics & Analytics | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult CityCompanies(string stateCode, string cityName, string range = "24m")
        {
            if (string.IsNullOrWhiteSpace(stateCode) || string.IsNullOrWhiteSpace(cityName))
                return RedirectToAction("ActiveCompanies");
            string dbCityName = cityName.Replace("-", " ").ToUpper();
            var vm = _homepageService.GetCityCompaniesData(stateCode.ToUpper(), dbCityName, range);
            if (vm == null)
                return Redirect("/statistics/state/" + stateCode.ToUpper());
            var ti = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            string cityDisplay = ti.ToTitleCase(dbCityName.ToLower());
            ViewBag.Title = cityDisplay + ", " + stateCode.ToUpper() + " Trucking Statistics & Analytics | Truck Carrier Hub";
            return View(vm);
        }

        private ActionResult ValidateMonthRoute(int year, int month, out DateTime requested)
        {
            requested = DateTime.MinValue;
            if (year < 2000 || year > 9999 || month < 1 || month > 12)
                return RedirectPermanent("/statistics/new-registrations");
            var now2         = DateTime.Today;
            var lastComplete = new DateTime(now2.Year, now2.Month, 1).AddMonths(-1);
            requested        = new DateTime(year, month, 1);
            if (requested > lastComplete)
                return RedirectPermanent("/statistics/new-registrations");
            return null;
        }

        [HttpGet]
        public ActionResult NewRegistrationsMonthCarriers(int year = 0, int month = 0)
        {
            DateTime requested;
            var redirect = ValidateMonthRoute(year, month, out requested);
            if (redirect != null) return redirect;
            var rawMonth = RouteData.Values["month"] as string;
            if (rawMonth != null && rawMonth.Length == 1)
                return RedirectPermanent(string.Format("/statistics/new-registrations/{0}/{1:D2}/carriers", year, month));
            var vm = _homepageService.GetNewRegistrationsMonthData(year, month);
            string mdash = System.Char.ConvertFromUtf32(0x2014);
            ViewBag.Title = "New Motor Carriers " + mdash + " " + requested.ToString("MMMM yyyy") + " | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult NewRegistrationsMonthBrokers(int year = 0, int month = 0)
        {
            DateTime requested;
            var redirect = ValidateMonthRoute(year, month, out requested);
            if (redirect != null) return redirect;
            var rawMonth = RouteData.Values["month"] as string;
            if (rawMonth != null && rawMonth.Length == 1)
                return RedirectPermanent(string.Format("/statistics/new-registrations/{0}/{1:D2}/brokers", year, month));
            var vm = _homepageService.GetNewRegistrationsMonthData(year, month);
            string mdash = System.Char.ConvertFromUtf32(0x2014);
            ViewBag.Title = "New Freight Brokers " + mdash + " " + requested.ToString("MMMM yyyy") + " | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult NewRegistrationsMonth(int year = 0, int month = 0)
        {
            DateTime requested;
            var redirect = ValidateMonthRoute(year, month, out requested);
            if (redirect != null) return redirect;
            var rawMonth = RouteData.Values["month"] as string;
            if (rawMonth != null && rawMonth.Length == 1)
                return RedirectPermanent(string.Format("/statistics/new-registrations/{0}/{1:D2}", year, month));
            var vm = _homepageService.GetNewRegistrationsMonthData(year, month);
            string mdash     = System.Char.ConvertFromUtf32(0x2014);
            ViewBag.Title    = "New FMCSA Registrations " + mdash + " " + requested.ToString("MMMM yyyy") + " | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult NewRegistrations(string range = "24m")
        {
            string mdash = System.Char.ConvertFromUtf32(0x2014);
            var allowedRanges = new[] { "12m", "24m", "36m", "48m" };
            bool validRange = false;
            foreach (var r in allowedRanges) { if (r == range) { validRange = true; break; } }
            if (!validRange) range = "24m";
            var vm = _homepageService.GetNewRegistrationsData(range);
            ViewBag.Title = "New FMCSA Registrations " + mdash + " U.S. Trucking & Broker Data | Truck Carrier Hub";
            return View(vm);
        }

        [HttpGet]
        public ActionResult FleetOperations()
        {
            string mdash = System.Char.ConvertFromUtf32(0x2014);
            ViewBag.Title = "Fleet & Operations Statistics " + mdash + " U.S. Trucking | Truck Carrier Hub";
            var vm = _homepageService.GetFleetOperationsData();
            return View(vm);
        }

        [HttpGet]
        public ActionResult Cargo()
        {
            string mdash = System.Char.ConvertFromUtf32(0x2014);
            ViewBag.Title = "Cargo Statistics " + mdash + " U.S. Trucking | Truck Carrier Hub";
            var vm = _homepageService.GetCargoData();
            return View("CargoStatistics", vm);
        }

        #endregion

    }
}