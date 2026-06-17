using Common.Utility;
using Common.Utility.ExceptionExtension;
using Common.Utility.Logger;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using PartnerCarrier.MyRazorViewEngines;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PartnerCarrier.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AppLogger.Instance.Log("Application Started...", LogType.Info);

            //RouteTable.Routes.MapMvcAttributeRoutes();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Register our customer view engine to control T2 and TBag views and over ridding
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new MyRazorViewEngine());

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
        }

        /// <summary>
        /// Handles the Error event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Application_Error(object sender, EventArgs e)
        {

            // identify Area Name to which the Current request is made for redirect on error page.
            var areaName = HttpContext.Current.Request.RequestContext.RouteData.DataTokens["area"];
            string errorUrl = string.Empty;

            //if area name is not null and area name is admin then it will redirect on admin error page other wise redirect on front end error page.
            if (areaName != null && areaName.ToString().ToLower() == "admin")
                errorUrl = AppSettings.AdminErrorPageUrl;
            else
                errorUrl = AppSettings.ErrorPageUrl;
            var exception = Server.GetLastError();

            try
            {
                HttpException httpException = null;
                if (exception is HttpException)
                    httpException = (HttpException)exception;

                var isUnhandled = false;

                if (exception.Message.IndexOf("A potentially dangerous Request") != -1)
                {
                    HttpContext.Current.Response.Redirect(errorUrl + "?msg=errHtml");
                }
                else if (httpException != null)
                {
                    var httpStatusCode = httpException.GetHttpCode();
                    if (httpStatusCode == 404)
                    {
                        HttpContext.Current.Server.ClearError();
                        HttpContext.Current.Response.Redirect(errorUrl + "?msg=errNotFound", false);
                    }
                    else if (httpStatusCode == 400)
                    {
                        HttpContext.Current.Server.ClearError();
                        HttpContext.Current.Response.Redirect(errorUrl + "?msg=BadReq", false);
                    }
                    else if (httpStatusCode == 403)
                    {
                        HttpContext.Current.Server.ClearError();
                        HttpContext.Current.Response.Redirect("/Account/Login", false);
                    }
                    else if (httpStatusCode == 401)
                    {
                        HttpContext.Current.Server.ClearError();
                        HttpContext.Current.Response.Redirect(errorUrl + "?msg=errAcc", false);
                    }
                    else
                    {
                        isUnhandled = true;
                    }
                }
                else
                {
                    isUnhandled = true;
                }


                if (isUnhandled)
                {
                    AppLogger.Instance.Log(exception);

                    //Getting Config Key for Sending Mail.
                    var exceptionMailSend = Config.GetValue("SendExceptionMail");

                    //If key value is true then only mail will be send otherwise not send
                    if (exceptionMailSend == "true")
                    {
                        ExceptionLogging.ExceptionSendToMail(exception);
                    }
                    Server.ClearError();
                    HttpContext.Current.Response.Redirect(errorUrl, true);
                }
            }
            catch
            {
                AppLogger.Instance.Log(exception);
                ExceptionLogging.ExceptionSendToMail(exception);
                HttpContext.Current.Server.ClearError();
                HttpContext.Current.Response.Redirect(errorUrl);
            }
        }
    }
}
