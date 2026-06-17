namespace Common.Utility.IP
{
    using System.Net;
    using System.Web;

    /// <summary>
    /// This is a static IPUtil class.
    /// </summary>
    public static class IPUtil
    {
        /// <summary>
        /// This method is for getting request IP
        /// </summary>
        /// <returns>return request IP</returns>
        public static string GetRequestIP()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return null;

            string requestIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (requestIP != null)
                requestIP = requestIP.Split(',')[0].Trim();
            return requestIP;
        }

        public static string GetIpAddress() // Get IP Address
        {
            string ip = "";
            IPHostEntry ipEntry = Dns.GetHostEntry(GetCompCode());
            IPAddress[] addr = ipEntry.AddressList;
            ip = addr[1].ToString();
            return ip;
        }

        public static string GetCompCode() // Get Computer Name
        {
            string strHostName = "";
            strHostName = Dns.GetHostName();
            return strHostName;
        }
    }
}
