using System.Web.Optimization;

namespace PartnerCarrier.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery-frontend").Include(
                          "~/Themes/Bootstrap/plugins/jquery-1.10.2/jquery-{version}.js",
                          "~/Themes/Bootstrap/plugins/jquery.validate-1.11.1/jquery.validate.js",
                          "~/Themes/Bootstrap/plugins/microsoft.jQuery.unobtrusive.validation-3.2.3/jquery.validate.unobtrusive.js",
                          "~/Themes/Bootstrap/plugins/jquery.ui-1.9.2/jquery-ui.min.js",
                          "~/Themes/Bootstrap/plugins/jquery.tagsinput-1.3.3/jquery.tagsinput.js"
                          , "~/Scripts/jquery.unobtrusive-ajax.min.js"
                         ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                         "~/Themes/Bootstrap/plugins/modernizr-2.6.2/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-frontend").Include(
                      "~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/dist/js/bootstrap.js",
                      "~/Themes/Bootstrap/plugins/respond-1.2.0/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css-frontend").Include(
                    "~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/dist/css/bootstrap.css",
                    "~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/bootstrap-override.css",
                    "~/Themes/Bootstrap/plugins/fontAwesome-4.7.0/css/font-awesome.min.css",
                    "~/Themes/Bootstrap/plugins/jquery.ui-1.9.2/css/jquery-ui.css",
                                      "~/Themes/Bootstrap/plugins/jquery.tagsinput-1.3.3/css/jquery.tagsinput.css",
                      "~/Themes/Bootstrap/CSS/Site.css"

                    ));
            bundles.Add(new ScriptBundle("~/bundles/common-ajax-frontend").Include(
               "~/Scripts/Common.AJAX.js"
               ));
        }
    }
}
