namespace Common.Utility.Extensions
{
    using System;
    using System.Web;

    /// <summary>
    /// Utility class that provides extension methods for HttpRequest
    /// </summary>
    public static partial class HttpRequestExt
    {
        /// <summary>
        /// Request values from submitted form data.
        /// </summary>
        /// /// <param name="request">this is extention method for HTTP Request.Enter Http Request.</param>
        /// <param name="clientId">client id of the control or key of the form data submitted</param>
        /// <returns> value of request.form</returns>
        public static string GetValue(this HttpRequest request, string clientId)
        {
            if (request == null) return null;

            if (request.Form[clientId.Replace('_', '$')] != null)
                return request.Form[clientId.Replace('_', '$')].ToString();
            else if (request.Form[clientId] != null)
                return request.Form[clientId].ToString();
            return null;
        }

        /// <summary>
        /// Returns true or false based on if request is an ajax request or not
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return (request["X-Requested-With"] == "XMLHttpRequest") || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest"));
        }
    }
}
