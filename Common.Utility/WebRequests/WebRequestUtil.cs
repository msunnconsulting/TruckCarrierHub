namespace Common.Utility.WebRequests
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// This is a sealed WebRequestUtil class.
    /// </summary>
    public sealed class WebRequestUtil
    {
        /// <summary>
        /// Gets raw html for a URL
        /// </summary>
        /// <param name="url">URL of page to download html for</param>
        /// <param name="cookies">cookies to be passed to request</param>
        /// <param name="useUserAgent">indicates if user agent should be used for request or not</param>
        /// <returns>returns stream reader object</returns>
        public static string DownloadPage(string url, CookieContainer cookies, bool useUserAgent)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.AllowAutoRedirect = true;

            if (useUserAgent)
                webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; InfoPath.2; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";

            if (cookies != null)
                webRequest.CookieContainer = cookies;

            HttpWebResponse rsp = webRequest.GetResponse() as HttpWebResponse;

            StreamReader responseReader = new StreamReader(rsp.GetResponseStream());
            string responseData = responseReader.ReadToEnd();
            responseReader.Close();
            rsp.Close();
            return responseData;
        }

        /// <summary>
        /// Downloads raw HTML of a URL by posting data to it.
        /// </summary>
        /// <param name="url">URL of page to download html for</param>
        /// <param name="cookies">cookies to be passed to request</param>
        /// <param name="postData">query string type data to be posted</param>
        /// <returns>returns stream reader object</returns>
        public static string DownloadPage(string url, CookieContainer cookies, string postData)
        {
            return DownloadPage(url, cookies, postData, "application/x-www-form-urlencoded");
        }

        /// <summary>
        /// Method for Download Page.
        /// </summary>
        /// <param name="url">URL of page to download html for</param>
        /// <param name="cookies">cookies to be passed to request</param>
        /// <param name="postData">query string type data to be posted</param>
        /// <param name="contentType">request content type</param>
        /// <returns>returns Stream Reader Object</returns>
        public static string DownloadPage(string url, CookieContainer cookies, string postData, string contentType)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; InfoPath.2; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            webRequest.CookieContainer = cookies;
            webRequest.AllowAutoRedirect = true;
            //// write the form values into the request message
            StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
            requestWriter.Write(postData);
            requestWriter.Close();

            HttpWebResponse rsp = webRequest.GetResponse() as HttpWebResponse;

            StreamReader responseReader = new StreamReader(rsp.GetResponseStream());
            string responseData = responseReader.ReadToEnd();
            responseReader.Close();
            rsp.Close();
            return responseData;
        }

        /// <summary>
        /// Method for Download Page.
        /// </summary>
        /// <param name="url">URL of page to download html for</param>
        /// <param name="cookies">cookies to be passed to request</param>
        /// <param name="postData">query string type data to be posted</param>
        /// <param name="contentType">request content type</param>
        /// <returns>returns Stream Reader Object</returns>
        public static async Task<string> DownloadPageAsync(string url, CookieContainer cookies = null, string postData = null, string contentType = null)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; InfoPath.2; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            webRequest.CookieContainer = cookies;
            //webRequest.AllowAutoRedirect = true;
            ////// write the form values into the request message
            StreamWriter requestWriter = new StreamWriter(await webRequest.GetRequestStreamAsync());
            requestWriter.Write(postData);
            requestWriter.Close();

            HttpWebResponse htppWebResponse = await webRequest.GetResponseAsync() as HttpWebResponse;
            StreamReader responseReader = new StreamReader(htppWebResponse.GetResponseStream());
            string responseData = await responseReader.ReadToEndAsync();
            responseReader.Close();
            htppWebResponse.Close();
            return responseData;
        }
    }
}
