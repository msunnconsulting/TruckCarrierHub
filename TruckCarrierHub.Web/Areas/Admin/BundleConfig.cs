using System.Web.Optimization;

namespace PartnerCarrier.Web.Areas.Admin
{
    internal static class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        internal static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery-area").Include(
                                 "~/Themes/Bootstrap/plugins/jquery-1.10.2/jquery-{version}.js",
                                 "~/Themes/Bootstrap/plugins/jquery.validate-1.11.1/jquery.validate.js",
                                 "~/Themes/Bootstrap/plugins/microsoft.jQuery.unobtrusive.validation-3.2.3/jquery.validate.unobtrusive.js",
                                 "~/Scripts/jquery.unobtrusive-ajax.min.js",
                                 "~/Themes/Bootstrap/plugins/jquery.ui-1.9.2/jquery-ui.min.js"

                        ));
            bundles.Add(new ScriptBundle("~/Areas/Admin/bundles/common-ajax-area").Include(
               //"~/Scripts/Common.AJAX.js",
               "~/Areas/Admin/Themes/Plugins/Common.AJAX.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-Area").Include(
                       "~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/dist/js/bootstrap.js",
                      "~/Themes/Bootstrap/plugins/respond-1.2.0/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css-Area").Include(
                 "~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/dist/css/bootstrap.css",
                    "~/Themes/Bootstrap/plugins/fontAwesome-4.7.0/css/font-awesome.min.css",
                    "~/Themes/Bootstrap/plugins/jquery.ui-1.9.2/css/jquery-ui.css",
                    "~/Areas/Admin/Themes/Bootstrap/CSS/site.css"
                    ));
        }
    }
}
