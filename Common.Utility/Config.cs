namespace Common.Utility
{
    using System;
    using System.Configuration;
    using System.Web;

    /// <summary>
    /// Class used to fetch some default information from AppConfig section. Also gives 2 very important properties that gives us Root URL and Path of the website.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// static variable for property SiteURL
        /// </summary>
        private static string siteURL;

        /// <summary> 
        /// static variable for property SitePath
        /// </summary>
        private static string sitePath;

        /// <summary>
        /// Gets the Root URL of the website.
        /// </summary>
        public static string SiteURL
        {
            get
            {
                if (string.IsNullOrEmpty(siteURL))
                {
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.Url.Query))
                        siteURL = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.Query, string.Empty);
                    else
                        siteURL = HttpContext.Current.Request.Url.AbsoluteUri;
                    if (HttpContext.Current.Request.Url.AbsolutePath != "/")
                        siteURL = siteURL.Replace(HttpContext.Current.Request.Url.AbsolutePath, string.Empty) + HttpContext.Current.Request.ApplicationPath;

                    if (!string.IsNullOrEmpty(siteURL))
                    {
                        if (!siteURL.EndsWith("/"))
                            siteURL += "/";
                    }
                    else
                        siteURL = string.Empty;
                }

                return siteURL;
            }
        }

        public static string SitePathForAsync
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// Gets the Root physical path of the website.
        /// </summary>
        public static string SitePath
        {
            get
            {
                sitePath = string.Concat(HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath), @"\");
                if (!string.IsNullOrEmpty(sitePath))
                {
                    if (!sitePath.EndsWith("\\"))
                        sitePath += "\\";
                }
                else
                    sitePath = string.Empty;
                return sitePath;
            }
        }

        /// <summary>
        /// Gets a value from config file and return it for the supplied key
        /// </summary>
        /// <param name="key">Key for which value need to be read from web.config and return</param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            return Convert.ToString(ConfigurationManager.AppSettings[key]);
        }
    }
}
