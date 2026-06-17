namespace Common.Utility
{
    public class AppSettings
    {
        public static string FromEmail
        {
            get
            {
                return Config.GetValue("FromEmail");
            }
        }

        public static string ErrorPageUrl
        {
            get
            {
                return Config.SiteURL + "error";
            }
        }

        public static string SetupErrorPageUrl
        {
            get
            {
                return Config.SiteURL + "Setup/Error";
            }
        }

        public static string AdminErrorPageUrl
        {
            get
            {
                return Config.SiteURL + "Admin/Account/Error";
            }
        }

    }
}
