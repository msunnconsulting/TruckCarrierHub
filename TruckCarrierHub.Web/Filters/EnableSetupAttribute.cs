namespace PartnerCarrier.Web.Filters
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class EnableSetupAttribute : AuthorizeAttribute
    {

        //EnableSetupAttribute.cs
        //use for developers this file allow to developer to use setup controller functionality.
        //setup functionality not allowed on production page.


        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            //return true/false from web.config file
            string enableSetup = ConfigurationManager.AppSettings["EnableSetup"];
            bool isEnableSetup = Convert.ToBoolean(enableSetup);
            //if enableSetup is true then it will return true other wise it return false.
            if (isEnableSetup) authorize = true;

            return authorize;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //if enableSetup value is false then it will return to error page.
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                                                { "action", "Error" },
                                                { "controller", "Account" },
                                                { "msg", "errNotFound"}
                                            });
        }
    }
}