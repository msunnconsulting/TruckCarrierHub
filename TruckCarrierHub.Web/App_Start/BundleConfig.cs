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

            // CssRewriteUrlTransform is required on every file here: StyleBundle does NOT rewrite
            // relative url(...) references by default, so without it each file's relative image/
            // font paths (e.g. jquery-ui's "images/ui-bg_flat_75_ffffff_40x100.png") resolve
            // against the bundle's own virtual path (~/Content/css-frontend) instead of the
            // source file's real folder, producing 404s like /Content/images/ui-bg_flat_75_ffffff_40x100.png
            // (confirmed via Search Console — real file lives under
            // Themes/Bootstrap/plugins/jquery.ui-1.9.2/css/images/).
            bundles.Add(new StyleBundle("~/Content/css-frontend")
                    .Include("~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/dist/css/bootstrap.css", new CssRewriteUrlTransform())
                    .Include("~/Themes/Bootstrap/plugins/bootstrap-3.3.7-custom/bootstrap-override.css", new CssRewriteUrlTransform())
                    .Include("~/Themes/Bootstrap/plugins/fontAwesome-4.7.0/css/font-awesome.min.css", new CssRewriteUrlTransform())
                    .Include("~/Themes/Bootstrap/plugins/jquery.ui-1.9.2/css/jquery-ui.css", new CssRewriteUrlTransform())
                    .Include("~/Themes/Bootstrap/plugins/jquery.tagsinput-1.3.3/css/jquery.tagsinput.css", new CssRewriteUrlTransform())
                    .Include("~/Themes/Bootstrap/CSS/Site.css", new CssRewriteUrlTransform()));
            bundles.Add(new ScriptBundle("~/bundles/common-ajax-frontend").Include(
               "~/Scripts/Common.AJAX.js"
               ));
        }
    }
}
