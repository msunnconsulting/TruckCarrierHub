namespace PartnerCarrier.Web.Areas.Admin
{
    using System.Web.Mvc;
    using System.Web.Routing;

    internal static class RouteConfig
    {
        internal static void RegisterRoutes(AreaRegistrationContext context)
        {

            RouteTable.Routes.MapMvcAttributeRoutes();
            //add routes
            context.MapRoute(
                "Admin_default",
                "admin/{controller}/{action}/{id}",
                new { controller = "Account", action = "Login", id = UrlParameter.Optional },
                new string[] { "PartnerCarrier.Web.Areas.Admin.Controllers" }
            );
        }
    }
}
