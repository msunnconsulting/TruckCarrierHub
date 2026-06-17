namespace PartnerCarrier.Web.Filters
{
    using Common.Utility;
    using Infrastructure;
    using PartnerCarrier.ViewModels.Admin;
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        protected new readonly UserRole[] Roles;

        public AuthorizeRoleAttribute(params UserRole[] Roles)
        {
            this.Roles = Roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {



            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }



            var role = Roles[0];

            if (role == UserRole.Admin)
            {
                if (!FormsAuthService.Instance.IsAuthenticated(Config.GetValue("AdminLoginAuthenticationName")))
                    return false;

                var user = FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName"));
                if (!this.Roles.Any(m => m == user.Role))
                    return false;


            }
            else if (role == UserRole.BusinessUser)
            {
                if (!FormsAuthService.Instance.IsAuthenticated(Config.GetValue("BusinessLoginAuthenticationName")))
                    return false;
                var user = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));
                if (!this.Roles.Any(m => m == user.Role))
                    return false;
            }
            else
            {

                if (!FormsAuthService.Instance.IsAuthenticated(Config.GetValue("SetupLoginAuthenticationName")))
                    return false;
                var user = FormsAuthService.Instance.LoggedInUser(Config.GetValue("SetupLoginAuthenticationName"));
                if (!this.Roles.Any(m => m == user.Role))
                    return false;
            }
            return true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //Here we check condition that for what HandleUnauthorizedRequest called like user is not logged in or Unauthorized logged in.
            var role = Roles[0];

            bool isAuthenticated = false;

            if (role == UserRole.Admin)
            {
                isAuthenticated = FormsAuthService.Instance.IsAuthenticated(Config.GetValue("AdminLoginAuthenticationName"));

            }
            else if (role == UserRole.BusinessUser)
            {
                isAuthenticated = FormsAuthService.Instance.IsAuthenticated(Config.GetValue("BusinessLoginAuthenticationName"));
            }
            else
            {
                isAuthenticated = FormsAuthService.Instance.IsAuthenticated(Config.GetValue("SetupLoginAuthenticationName"));

            }
            //if user is authenticated or not.
            if (!isAuthenticated)
            {
                string lastUrl = null;
                //Check request is get or post 
                //if request is post then no need to last url.
                //if request is get then get last url and after  login redirect last url page.
                if (HttpContext.Current.Request.HttpMethod == "GET")
                    lastUrl = filterContext.HttpContext.Request.RawUrl;

                //Check request is ajax request or not 
                //if ajax request then we have to pass response status code 
                //on response status code it will redirect from common.js if ajax request.
                //otherwise redirect from here.
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    filterContext.HttpContext.Response.StatusCode = 403;
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            Error = "NotAuthorized",
                            LogOnUrl = urlHelper.Action("Login", "Account", lastUrl)


                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    if (lastUrl.Contains("setup"))
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                                                { "action", "Login" },
                                                { "controller", "Setup" }
                                            });
                    }
                    if (lastUrl.Contains("admin"))
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                                                { "action", "Login" },
                                                { "controller", "Account" },
                                                { "returnUrl", lastUrl}
                                            });
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                                                { "action", "BusinessLogin" },
                                                { "controller", "Home" },
                                                { "returnUrl", lastUrl}
                                            });
                    }
                }
            }
            else
            {
                var user = FormsAuthService.Instance.LoggedInUser(Config.GetValue("SetupLoginAuthenticationName"));

                if (user.Role == UserRole.SetupAdmin)
                {
                    FormsAuthService.Instance.LogOut(Config.GetValue("SetupLoginAuthenticationName"), false);
                    HttpContext.Current.ClearError();

                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                                                { "action", "LogIn" },
                                                { "controller", "Account" }
                                            });
                }
                else
                {
                    //if user is doing unauthorized access.

                    //Check request is ajax request or not 
                    //if ajax request then we have to pass response status code 
                    //on response status code it will redirect from common.js if ajax request.
                    //otherwise redirect from here.
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        var urlHelper = new UrlHelper(filterContext.RequestContext);
                        filterContext.HttpContext.Response.StatusCode = 403;
                        filterContext.Result = new JsonResult
                        {
                            Data = new
                            {
                                Error = "Access Denied",
                                LogOnUrl = urlHelper.Action("Error", "Account", new { }, "")


                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{
                                                { "action", "Error" },
                                                { "controller", "Account" },
                                                { "msg", "errAcc"}
                                            });
                    }
                }
            }
        }
    }
}
