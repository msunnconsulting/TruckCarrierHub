namespace PartnerCarrier.Web.Controllers
{
    using Common.Utility;
    using Common.Utility.Logger;
    using Infrastructure.Contracts.User;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web.Mvc;
    using ViewModels.User;

    public class ContactUsController : BaseController
    {
        #region Private Data member
        private readonly IContactUsService _IContactUsService;
        #endregion

        #region Constructor
        public ContactUsController(IContactUsService contactUsService)
        {
            _IContactUsService = contactUsService;
        }
        #endregion

        #region
        /// <summary>
        /// Contact Us Page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ContactUs()
        {
            return View();
        }

        /// <summary>
        /// Contact Us Detail send by email to admin
        /// </summary>
        /// <param name="contactUsVM"></param>
        /// <returns></returns>
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        public ActionResult ContactUs(ContactUsVM contactUsVM)
        {
            try
            {
                _IContactUsService.SendContactUsDetailsToAdminEmail(contactUsVM);
                //show alert successful message after submit information
                TempData["SuccesMessageAfterSubmitInformation"] = "Your information submitted successfully.";
                //clear information from model
                ModelState.Clear();
            }
            catch (Exception ex)
            {
                //if any error occurs then create a log in applog text
                AppLogger.Instance.Log(ex);
                //set error message in viewbag
                TempData["EmailErrorMesaage"] = "Something went wrong,please try after some time.";
                ModelState.Clear();
            }
            //return view
            return View();
        }
        #endregion

        #region Google Captcha Validate Attribute
        public class ValidateGoogleCaptchaAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                const string urlToPost = "https://www.google.com/recaptcha/api/siteverify";
                string secretKey = Config.GetValue("GoogleRecaptchaSecretKey");
                var captchaResponse = filterContext.HttpContext.Request.Form["g-recaptcha-response"];

                if (string.IsNullOrWhiteSpace(captchaResponse)) AddErrorAndRedirectToGetAction(filterContext);

                var validateResult = ValidateFromGoogle(urlToPost, secretKey, captchaResponse);
                if (!validateResult.Success) AddErrorAndRedirectToGetAction(filterContext);

                base.OnActionExecuting(filterContext);
            }

            private static void AddErrorAndRedirectToGetAction(ActionExecutingContext filterContext)
            {
                filterContext.Controller.TempData["InvalidCaptcha"] = "Invalid Captcha !";
                filterContext.Result = new ViewResult
                {
                    ViewName = "ContactUs",
                    ViewData = filterContext.Controller.ViewData,
                    TempData = filterContext.Controller.TempData
                };
            }

            private static ReCaptchaResponse ValidateFromGoogle(string urlToPost, string secretKey, string captchaResponse)
            {
                var postData = "secret=" + secretKey + "&response=" + captchaResponse;

                var request = (HttpWebRequest)WebRequest.Create(urlToPost);
                request.Method = "POST";
                request.ContentLength = postData.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    streamWriter.Write(postData);

                string result;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                        result = reader.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<ReCaptchaResponse>(result);
            }
        }

        internal class ReCaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("challenge_ts")]
            public string ValidatedDateTime { get; set; }

            [JsonProperty("hostname")]
            public string HostName { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
        #endregion
    }
}