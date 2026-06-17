using Common.Utility;
using System.Web;
using System.Web.Mvc;

namespace PartnerCarrier.MyRazorViewEngines
{
    public class MyRazorViewEngine : RazorViewEngine
    {
        public MyRazorViewEngine() : base()
        {


            AreaViewLocationFormats = new[] {
                "~/Areas/{2}/Themes/%1/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Themes/%1/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Themes/%1/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Themes/%1/Views/Shared/{0}.vbhtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            AreaMasterLocationFormats = new[] {
                "~/Areas/{2}/Themes/%1/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Themes/%1/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Themes/%1/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Themes/%1/Views/Shared/{0}.vbhtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };

            AreaPartialViewLocationFormats = new[] {
                "~/Areas/{2}/Themes/%1/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Themes/%1/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Themes/%1/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Themes/%1/Views/Shared/{0}.vbhtml",
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.vbhtml"
            };


            ViewLocationFormats = new[] {
                "~/Themes/%1/Views/{1}/{0}.cshtml",
                "~/Themes/%1/Views/{1}/{0}.vbhtml",
                "~/Themes/%1/Views/Shared/{0}.cshtml",
                "~/Themes/%1/Views/Shared/{0}.vbhtml",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            MasterLocationFormats = new[] {
                "~/Themes/%1/Views/{1}/{0}.cshtml",
                "~/Themes/%1/Views/{1}/{0}.vbhtml",
                "~/Themes/%1/Views/Shared/{0}.cshtml",
                "~/Themes/%1/Views/Shared/{0}.vbhtml",



                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"

            };

            PartialViewLocationFormats = new[] {
                "~/Themes/%1/Views/{1}/{0}.cshtml",
                "~/Themes/%1/Views/{1}/{0}.vbhtml",
                "~/Themes/%1/Views/Shared/{0}.cshtml",
                "~/Themes/%1/Views/Shared/{0}.vbhtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };




        }





        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {

            var areaName = HttpContext.Current.Request.RequestContext.RouteData.DataTokens["area"];
            string theme = string.Empty;
            if (areaName != null && areaName.ToString() == "Admin")
                theme = Config.GetValue("AdminThemeName");
            else
                theme = Config.GetValue("FrontEndThemeName");

            return base.CreatePartialView(controllerContext, partialPath.Replace("%1", theme));
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {


            var areaName = HttpContext.Current.Request.RequestContext.RouteData.DataTokens["area"];
            string theme = string.Empty;
            if (areaName != null && areaName.ToString() == "Admin")
                theme = Config.GetValue("AdminThemeName");
            else
                theme = Config.GetValue("FrontEndThemeName");


            var viewPathWithTheme = viewPath.Replace("%1", theme);
            var masterPathWithTheme = masterPath.Replace("%1", theme);


            return base.CreateView(controllerContext, viewPathWithTheme, masterPathWithTheme);
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            var areaName = HttpContext.Current.Request.RequestContext.RouteData.DataTokens["area"];
            string theme = string.Empty;
            if (areaName != null && areaName.ToString() == "Admin")
                theme = Config.GetValue("AdminThemeName");
            else
                theme = Config.GetValue("FrontEndThemeName");

            return base.FileExists(controllerContext, virtualPath.Replace("%1", theme));
        }

    }
}