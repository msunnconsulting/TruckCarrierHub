

namespace PartnerCarrier.Web.Areas.Admin.Controllers
{
    using Common.Utility;
    using Common.Utility.ViewModels;
    using Filters;
    using Infrastructure.Contracts.User;
    using PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ViewModels.Admin;

    [RouteArea("admin", AreaPrefix = "admin")]
    [RoutePrefix("business")]
    [AuthorizeRole(UserRole.Admin)]
    public class BusinessController : BaseController
    {

        private readonly IHomepageService _homepageService;
        private readonly IBusinessMangementService _businessMangementService;
        private static object lockSendMailMethod = new object();
        private static object lockCityRenew = new object();

        public BusinessController(IBusinessMangementService businessMangementService, IHomepageService homepageService)
        {
            _businessMangementService = businessMangementService;
            _homepageService = homepageService;
        }


        /// <summary>
        /// Get list of emails for edit and ready for send.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("email")]
        public ActionResult UpdateSendEmail(PageSortPara ps)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var emailList = _businessMangementService.GetEmailList(ps);
                    return PartialView("_PartialUpdateSendEmailList", emailList);
                }
                catch (Exception ex)
                {

                    return ReturnExceptionResult(ex);
                }

            }

            return View(ps);
        }

        /// <summary>
        /// Add/edit email 
        /// </summary>
        /// <param name="emailID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("create-email")]
        [Route("edit-email/{emailID}")]
        public ActionResult AddEditEmail(int? emailID)
        {
            EmailsVM emailsVM = new EmailsVM();
            if (emailID.HasValue)
            {
                emailsVM = _businessMangementService.GetEmailByEmailId(emailID.Value);
            }

            return View(emailsVM);
        }

        /// <summary>
        /// Save Email
        /// </summary>
        /// <param name="emailsVM"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save-email")]
        [ValidateInput(false)]
        public ActionResult SaveEmail(EmailsVM emailsVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();

            try
            {
                _businessMangementService.SaveEmail(emailsVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }


        /// <summary>
        /// Send Emails
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("send-emails/{emailId}")]
        public ActionResult SendEmails(int emailId)
        {

            var emailList = _businessMangementService.GetEmailDetailsForSendMailByEmailId(emailId);
            emailList.lstStates = _homepageService.GetAllStates();
            return View(emailList);
        }

        /// <summary>
        /// Send Emails
        /// </summary>
        /// <param name="sendEmailsVM"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("send-email")]
        public ActionResult SendEmails(SendEmailsVM sendEmailsVM)
        {
            try
            {

                sendEmailsVM.SiteURL = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
                lock (lockSendMailMethod)
                {
                    var updateRecordProgress = _businessMangementService.GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value);

                    if (!updateRecordProgress.IsInProgress)
                    {
                        Task.Factory.StartNew(() => { _businessMangementService.SendEmails(sendEmailsVM); });
                    }


                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

        }

        /// <summary>
        /// Get Sent Email Progress
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        [Route("email-sent-progress/{emailId}")]
        public ActionResult GetSentEmailProgress(int emailId)
        {
            return Json(_businessMangementService.GetSentEmailsProgressInfo(emailId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Stop Sent Email Progress
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        [Route("stop-email-sent-progress/{emailId}")]
        public ActionResult StopSentEmailProgress(int emailId)
        {
            return Json(_businessMangementService.StopSentEmailsProgressInfo(emailId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/edit email 
        /// </summary>
        /// <param name="emailID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("update-success-stories")]
        public ActionResult UpdateSuccessStories()
        {
            SuccessStoriesVM successStorieVM = new SuccessStoriesVM();
            successStorieVM = _businessMangementService.GetSuccessStory();
            return View(successStorieVM);
        }

        [HttpGet]
        [Route("manage-global-hiring")]
        public ActionResult ManageGlobalHiring()
        {
            GlobalHiringVM globalHiringVM = new GlobalHiringVM();

            globalHiringVM = _businessMangementService.GetGlobalHiring();

            return View(globalHiringVM);
        }

        [Route("save-global-hiring")]
        public ActionResult SaveGlobalHiring(GlobalHiringVM globalHiringVM)
        {

            try
            {
                _businessMangementService.SaveGlobalHiring(globalHiringVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return ReturnExceptionResult(ex);
            }

        }

        /// <summary>
        /// Manage get a quote page which returns view with selected checkbox for Get a Quote control to show.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("manage-get-a-quote")]
        public ActionResult ManageGetAQuote()
        {
            ManageGetAQuoteVM manageGetAQuoteVM = new ManageGetAQuoteVM();
            manageGetAQuoteVM.GetAQuoteToShowList = _businessMangementService.GetCheckboxListForShowGetAQuoteControl();
            return View(manageGetAQuoteVM);
        }

        /// <summary>
        /// Save Checkboxes which user submited for Show Get A Quote control to show
        /// </summary>
        /// <param name="manageGetAQuoteVM"></param>
        /// <returns></returns>
        [Route("save-getaquote-to-show")]
        public ActionResult SaveGetAQuoteToShow(ManageGetAQuoteVM manageGetAQuoteVM)
        {
            try
            {
                _businessMangementService.SaveGetAQuoteToShow(manageGetAQuoteVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Save Email
        /// </summary>
        /// <param name="emailsVM"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save-success-stories")]
        [ValidateInput(false)]
        public ActionResult SaveSuccessStories(SuccessStoriesVM successStoriesVM)
        {
            try
            {
                //Save Success Stories
                _businessMangementService.SaveSuccessStories(successStoriesVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }


        /// <summary>
        /// Get Cities By State Code
        /// </summary>
        /// <param name="stateCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-cities-by-state-code/{stateCode}")]
        public ActionResult GetCitiesByStateCode(string stateCode)
        {
            try
            {
                //Get Global Hire value from Admin table
                int GlobalHire = _businessMangementService.GetGlobalHiring().GlobalHire;

                //Here we pass default false in GetCityList method because we need all the cities
                //If we pass true then we will get only hiring cities so we had pass only false default
                //In Front End Functionality for Hiring checkbox handle we pass default false
                var lstCities = _homepageService.GetCityList(stateCode, false, GlobalHire);
                return Json(lstCities, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }



        /// <summary>
        /// Get Business List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("manage-business")]
        public ActionResult BusinessSearchList()
        {
            return View("~/Areas/Admin/Views/Business/BusinessSearchList.cshtml");
        }

        /// <summary>
        /// Business Search List Partial
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        [Route("manage-business-partial")]
        public ActionResult BusinessSearchListPartial(PageSortPara ps, BusinessSearchVM businessSearchVM)
        {
            try
            {

                CommonBusinessAndPagelistVM commonBusinessAndPagelistVM = new CommonBusinessAndPagelistVM();
                //Get Business Search List here
                commonBusinessAndPagelistVM.PagedListVM = _businessMangementService.GetBusinessSearchList(ps, businessSearchVM);

                return PartialView("~/Areas/Admin/Views/Business/BusinessSearchListPartial.cshtml", commonBusinessAndPagelistVM);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }




        /// <summary>
        /// Delete business by its business Id
        /// </summary>
        /// <param name="AdminID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("delete-business")]
        public ActionResult DeleteBusiness(int BusinessId)
        {
            try
            {
                //Delete Business from business table
                _businessMangementService.DeleteBusinessById(BusinessId);
            }
            catch (Exception ex)
            {
                ReturnExceptionResult(ex);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Update Business Details
        /// </summary>
        /// <param name="businessSearchVM"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save")]
        //Post : Save Business
        public ActionResult EditBusiness(BusinessListVM businessSearchVM, PageSortPara ps)
        {
            //check model state is valid or not
            if (!ModelState.IsValid) return ReturnModelStateErrors();
            try
            {
                //service call for Update Business
                _businessMangementService.UpdateBusiness(businessSearchVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Business Reset Password
        /// </summary>
        /// <param name="usDOTNumberForResetPassword"></param>
        /// <returns></returns>
        [Route("business-reset-password")]
        public ActionResult BusinessResetPassword(int usDOTNumberForResetPassword)
        {
            try
            {
                BusinessForgotPasswordVM businessForgotPasswordVM = new BusinessForgotPasswordVM();
                businessForgotPasswordVM.USDOTNumber = usDOTNumberForResetPassword;
                businessForgotPasswordVM.IsEmailAddressCheck = false;
                _businessMangementService.BussinessResetPasswordMailSend(businessForgotPasswordVM);

                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Add/edit carrier 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("create-carrier")]
        [Route("edit-carrier/{id}")]
        public ActionResult AddEditCarrier(int? id)
        {
            CarrierVM carrierVM = new CarrierVM();
            if (id.HasValue)
            {
                //Get Detail at edit time 
                carrierVM = _businessMangementService.GetCarrierDetailsById(id.Value);
            }
            carrierVM.LoadTypeList = _businessMangementService.BindCheckboxListForLoadType();
            carrierVM.StateList = _businessMangementService.BindCheckboxListForState();

            return View(carrierVM);
        }

        /// <summary>
        /// Save Carrier Details
        /// </summary>
        /// <param name="carrierVM"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save-carrier")]
        public ActionResult SaveCarrierDetails(CarrierVM carrierVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();

            try
            {
                _businessMangementService.SaveCarrierDetails(carrierVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Get Carrier List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("manage-carrier")]
        public ActionResult ManagerCarrier()
        {
            return View("~/Areas/Admin/Views/Business/ManageCarrier.cshtml");
        }

        /// <summary>
        /// Business Search List Partial
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        [Route("carrier-list-partial")]
        public ActionResult CarrierListPartial(PageSortPara ps, ManageCarrierVM manageCarrierVM)
        {
            try
            {
                CommonCarrierAndPagelistVM commonCarrierAndPagelistVM = new CommonCarrierAndPagelistVM();
                ////Get Carrier Search List here
                commonCarrierAndPagelistVM.PagedListVM = _businessMangementService.GetCarrierList(ps, manageCarrierVM);

                return PartialView("~/Areas/Admin/Views/Business/CarrierListPartial.cshtml", commonCarrierAndPagelistVM);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }


        /// <summary>
        /// Activate or Inactivate the carrier by it carrierId
        /// </summary>
        /// <param name="carrierId">based on carrierId do active or inactive the carrier</param>
        /// <returns></returns>
        [HttpPost]
        [Route("activate-inactive-carrier")]
        public ActionResult ActivateInActivateCarrier(int carrierId)
        {
            try
            {
                //Make Active or Inactive Carrier by it's carrierId
                _businessMangementService.ActiveOrInActiveCarrierById(carrierId);

            }
            catch (Exception ex)
            {
                ReturnExceptionResult(ex);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }


        #region Outbound Link Management

        /// <summary>
        /// Get outbound links list
        /// </summary>
        /// <returns>list of links</returns>
        [HttpGet]
        [Route("outbound-list")]
        public ActionResult OutboundLinkList()
        {
            try
            {
                BaseOutboundLinkVM outboundLink = new BaseOutboundLinkVM();
                outboundLink.List = _businessMangementService.GetOutBoundLinks(false);
                return View(outboundLink);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Get outbound detail by id
        /// </summary>
        /// <param name="id">id for which outbound link detail</param>
        /// <returns></returns>
        [HttpGet]
        [Route("outbound-detail/{id}")]
        public ActionResult GetOutboundLinkDetail(int id)
        {
            try
            {
                OutboundLinkVM OutboundLinkVM = new OutboundLinkVM();
                OutboundLinkVM = _businessMangementService.GetOutboundLinkDataById(id);
                return Json(OutboundLinkVM, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Delete outbound link
        /// </summary>
        /// <param name="id">id to be deleted</param>
        /// <returns></returns>
        [HttpPost]
        [Route("delete-outbound-link/{id}")]
        public ActionResult DeleteOutboundLink(int id)
        {
            try
            {
                //Delete outbound link from OutboundLink by Id
                _businessMangementService.DeleteOutboundLinkById(id);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Save outbound link
        /// </summary>
        /// <param name="outboundLink">outboundLink to be saved</param>
        /// <returns></returns>
        [HttpPost]
        [Route("save-outbound-link")]
        public ActionResult SaveOutboundLink(OutboundLinkVM outboundLink)
        {
            try
            {
                if (!outboundLink.Id.HasValue)
                {
                    var totalLink = _businessMangementService.GetTotalOutboundLink();
                    if (totalLink >= 5)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                // Save outbound link
                _businessMangementService.SaveOutboundLink(outboundLink);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

        }

        #endregion

        #region Outbound Banner Management


        /// <summary>
        /// Get outbound banner list
        /// </summary>
        /// <returns>list of banner</returns>
        [HttpGet]
        [Route("outbound-banner-list")]
        public ActionResult OutboundBannerList()
        {
            try
            {
                BaseOutboundBannerVM outBoundBanner = new BaseOutboundBannerVM
                {
                    OutboundBannerList = _businessMangementService.GetOutBoundBanners()
                };

                return View(outBoundBanner);

            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

        }

        /// <summary>
        /// Save outbound Banner
        /// </summary>
        /// <param name="outboundBanners">outbound banners to be saved</param>
        /// <returns></returns>
        [HttpPost]
        [Route("save-outbound-banner")]
        public ActionResult SaveOutboundBanner(List<OutboundBannerDataModel> OutboundBannerList)
        {
            try
            {

                // Save outbound link
                _businessMangementService.SaveOutboundBanner(OutboundBannerList);

                return RedirectToAction("outbound-banner-list");
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

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


        #endregion

        #region City Articles


        #region Availability

        /// <summary>
        /// To make city articles available for viewers.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("city-articles-availability")]
        public ActionResult CityArticlesAvailability()
        {
            try
            {
                CityArticlesAvailabilityVM availabilityVM = new CityArticlesAvailabilityVM();
                availabilityVM = _businessMangementService.GetCityArticleAvailabilityDetails();
                return View(availabilityVM);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

        }

        /// <summary>
        /// To save city articles availability for viewers.
        /// </summary>
        /// <param name="OutboundBannerList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save-city-articles-availability")]
        public ActionResult SaveCityArticlesAvailability(CityArticlesAvailabilityVM availabilityVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Return the view with validation errors
                    return View(availabilityVM);
                }

                _businessMangementService.SaveCityArticlesAvailability(availabilityVM);

            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
            return Json(true, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Create City Article

        /// <summary>
        /// Create city article
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("create-city-article")]
        public ActionResult CreateCityArticle()
        {
            try
            {
                CreatCityArticlesVM data = new CreatCityArticlesVM();
                data.CountryList = _businessMangementService.GetCountries();
                data.StateList = new List<SelectListVM>();
                data.CityList = new List<SelectListVM>();

                return View(data);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-coutry-list")]
        public ActionResult GetCountryList()
        {
            try
            {
                var contryList = _businessMangementService.GetCountries();
                return Json(contryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-state-list/{countryCode}")]
        public ActionResult GetStateList(string countryCode)
        {
            try
            {
                var contryList = _businessMangementService.GetStates(countryCode);
                return Json(contryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-city-list/{countryCode}/{stateCode}")]
        public ActionResult GetCityList(string countryCode, string stateCode)
        {
            try
            {
                var contryList = _businessMangementService.GetCities(countryCode, stateCode);
                return Json(contryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-selected-cities-for-create-article")]
        public ActionResult GetSelectedCitiesForCreateArticle(SelectedCitiesForCreateArticleModel selecteCitiesCriteria)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ReturnModelStateErrors();
                }
                var selectedCitiesCount = _businessMangementService.GetSelectedCitiesForCreateArticle(selecteCitiesCriteria);
                return Json(selectedCitiesCount, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        [Route("create-article")]
        public async Task<ActionResult> CreateArticleAsync(SelectedCitiesForCreateArticleModel selectCitiesCriteria, string promptText)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ReturnModelStateErrors();
                }
                var saveArticlesResponse = await _businessMangementService.CreateAndSaveArticleToCityAsync(selectCitiesCriteria, promptText);
                return Json(saveArticlesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion

        #region Manage City Article

        /// <summary>
        /// To make city articles available for viewers.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("manage-city-articles")]
        public ActionResult ManageCityArticles()
        {
            try
            {
                ManageCityArticleVM data = new ManageCityArticleVM();
                data.CountryList = _businessMangementService.GetCountries();
                data.StateList = new List<SelectListVM>();
                data.CityList = new List<SelectListVM>();
                data.ShowCitiesWithArticle = "All";

                return View(data);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        [Route("allow-article-for-selected-state")]
        public async Task<ActionResult> AllowArticleForSelectedState(string countryCode, string stateCode)
        {
            try
            {
                var allowArticleResponse = await _businessMangementService.AllowArticleForSelectedState(countryCode, stateCode);
                return Json(allowArticleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-article-for-selected-city/{countryCode}/{stateCode}/{cityName}")]
        public ActionResult GetArticleForSelectedCity(string countryCode, string stateCode, string cityName)
        {
            try
            {
                var article = _businessMangementService.GetArticleForSelectedCity(countryCode, stateCode, cityName);
                return Json(article, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        [Route("allow-article-for-seleceted-city")]
        public ActionResult AllowArticleForSelectedCity(string countryCode, string stateCode, string cityName)
        {
            try
            {
                var response = _businessMangementService.AllowArticleForSelectedCity(countryCode, stateCode, cityName);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        [Route("set-article-for-seleceted-city/{countryCode}/{stateCode}/{cityName}")]
        public ActionResult SetArticleForSelectedCity(string countryCode, string stateCode, string cityName, string article)
        {
            try
            {
                var updateArticleResponse = _businessMangementService.SetArticleForSelectedCity(countryCode, stateCode, cityName, article);
                return Json(updateArticleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        [Route("remove-article-for-seleceted-city")]
        public ActionResult RemoveArticleForSelectedCity(string countryCode, string stateCode, string cityName)
        {
            try
            {
                var removeArticleResponse = _businessMangementService.SetArticleForSelectedCity(countryCode, stateCode, cityName, null);
                return Json(removeArticleResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-city-list-for-manage-city-article/{countryCode}/{stateCode}/{showCitiesWith}")]
        public ActionResult GetCityListForManageCityArticle(string countryCode, string stateCode, string showCitiesWith)
        {
            try
            {
                Nullable<bool> isAllowed = null;
                switch (showCitiesWith)
                {
                    case "Allowed":
                        isAllowed = true;
                        break;
                    case "NotAllowed":
                        isAllowed = false;
                        break;

                }
                var contryList = _businessMangementService.GetCitiesForManageCityArticle(countryCode, stateCode, isAllowed);
                return Json(contryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("is-city-article-available/{countryCode}/{stateCode}/{cityName}")]
        public ActionResult IsCityArticleAvailable(string countryCode, string stateCode, string cityName)
        {
            try
            {
                var contryList = _businessMangementService.IsCityArticleAvailable(countryCode, stateCode, cityName);
                return Json(contryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion

        #region Manage Cities

        /// <summary>
        /// Create city article
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("manage-cities")]
        public ActionResult ManageCities()
        {
            try
            {
                ManageCitiesVM data = new ManageCitiesVM();
                data.List = _businessMangementService.GetAllCities();
                return View(data);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("get-city-list-for-manage-city")]
        public ActionResult GetCityListForManageCities()
        {
            try
            {
                var cityList = _businessMangementService.GetCityListForManageCities();
                return Json(cityList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("finish-cities-update")]
        public async Task<ActionResult> FinishCitiesUpdate()
        {
            try
            {
                var response = await _businessMangementService.FinishCityUpdate();
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpPost]
        [Route("save-city-content")]
        public ActionResult SaveCityContent(SaveCityContentVM vm)
        {
            try
            {
                var result = _businessMangementService.SaveCityContent(vm);
                var parts = result.Split(new[] { ':' }, 2);
                return Json(new { success = parts[0] == "success", message = parts.Length > 1 ? parts[1] : "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("city-renew-progress")]
        public ActionResult GetCityRenewProgress()
        {
            return Json(_businessMangementService.GetCityRenewProgress(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("start-city-renew")]
        public ActionResult StartCityRenew(StartCityRenewVM request)
        {
            lock (lockCityRenew)
            {
                if (!_businessMangementService.GetCityRenewProgress().IsInProgress)
                {
                    Task.Factory.StartNew(() => { _businessMangementService.StartCityRenew(request); });
                }
            }
            return Json(new { started = true });
        }

        #endregion

        #endregion

        #region Review

        [HttpGet]
        [Route("manage-reviews")]
        public ActionResult ManageReviews()
        {
            ManageReviewsVM manageReviewsVM = _businessMangementService.GetReviewsFilter();

            return View(manageReviewsVM);
        }

        [Route("save-reviews-filter")]
        public ActionResult SaveReviewsFilter(ManageReviewsVM manageReviewsVM)
        {

            try
            {
                _businessMangementService.SaveReviewsFilter(manageReviewsVM);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return ReturnExceptionResult(ex);
            }

        }

        #endregion

    }
}