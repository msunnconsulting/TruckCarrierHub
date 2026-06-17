namespace Common.Utility.FormAuthentication
{
    using System;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Web.Security;

    /// <summary>
    /// This class allows complete FormsAuthentication utility methods
    /// </summary>
    /// <typeparam name="UserType">class which you want to store in cookie as user info</typeparam>
    /// <typeparam name="RoleType">type of role (long, int, enum etc.). By default a user object can have more than one role of this type</typeparam>
    /// <typeparam name="IdType">type of Id (long, int, string) - id means unique pk of user object</typeparam>
    public class FormsAuthenticationUtil<UserType, RoleType, IdType>
        where UserType : LoggedInUser<RoleType, IdType>
    {

        /// <summary>
        /// Returns true if user is authenticated, returns false if user is not authenticated
        /// </summary>
        public bool IsAuthenticated(string cookiesName)
        {
            return GetLoggedInUserFromCookie(cookiesName) != null;
        }

        /// <summary>
        /// Reads cookie and generate object of UserType you have supplied and return it back.
        /// </summary>        
        /// <returns>returns User of Given User Type</returns>
        public UserType LoggedInUser(string cookiesName)
        {

            return GetLoggedInUserFromCookie(cookiesName);
        }

        /// <summary>
        /// It generates forms authentication cookie and inject it into response. Basically it logs in the user and store the user object in cookie.
        /// </summary>        
        /// <param name="user">Enter user for login</param>
        /// <param name="timeoutInMinutes">Enter Time out in minutes for user session, it would override rememberme + web.config setting</param>
        /// <param name="redirectAfterLogin">indicates that do you want to redirect user to specific url or not, default is YES</param>
        /// <param name="redirectToUrl">Enter url where You want to redirect after login, Default NULL, which means it would redirect to ReturnURL in querystring or if that not found, then the DefaultUrl configured in forms auth in web.config</param>
        public void LogIn(UserType user, string cookiesName, double? timeoutInMinutes = null, bool redirectAfterLogin = true, string redirectToUrl = null)
        {
            //// Create the authentication ticket with custom user data.
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string userData = serializer.Serialize(user);

            //// if remember me, then cookies is stored for 1 year,
            // else stored for 50 minutes - same as forms auth time out in web.config
            if (!timeoutInMinutes.HasValue)
                timeoutInMinutes = user.RememberMe ? 365 * 24 * 60 : 50;

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    user.Id.ToString(),
                    DateTime.Now,
                    DateTime.Now.AddMinutes(timeoutInMinutes.Value),
                    user.RememberMe,
                    userData,
                    FormsAuthentication.FormsCookiePath);

            //// Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            //// Create the cookie.
            HttpCookie authCookie = new HttpCookie(cookiesName, encTicket);
            authCookie.Expires = DateTime.Now.AddMinutes(timeoutInMinutes.Value);
            authCookie.HttpOnly = true;

            HttpResponse response = HttpContext.Current.Response;
            response.Cookies.Add(authCookie);

            if (redirectAfterLogin)
                RedirectFromLoginPage(redirectToUrl);
        }

        ///// <summary>
        ///// This method accepts data and consider forms auth cookie already exist, and it changes data of that cookie and submit in response.
        ///// </summary>
        ///// <param name="userName">Username using which auth cookie is generated</param>
        ///// <param name="data">data which needs to be updated in the auth cookie</param>
        //public void SetAuthenticationData(string userName, object data)
        //{
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    string authData = serializer.Serialize(data);

        //    //get authentication cookie and add siteid to ticket for future reference
        //    HttpCookie cookie = FormsAuthentication.GetAuthCookie(userName, false);
        //    var ticket = FormsAuthentication.Decrypt(cookie.Value);
        //    var newticket = new FormsAuthenticationTicket(ticket.Version,
        //                                                  ticket.Name,
        //                                                  ticket.IssueDate,
        //                                                  ticket.Expiration,
        //                                                  false,
        //                                                  authData,
        //                                                  ticket.CookiePath);

        //    cookie.Value = FormsAuthentication.Encrypt(newticket);
        //    HttpContext.Current.Response.Cookies.Set(cookie);
        //}

        /// <summary>
        /// This method is used to redirect user from login page after successful login
        /// </summary>
        /// <param name="redirectToUrl">Enter url where You want to redirect to, Default NULL which means it would redirect to ReturnURL in querystring or if that not found, then the DefaultUrl configured in forms auth in web.config</param>
        public void RedirectFromLoginPage(string redirectToUrl = null)
        {
            HttpResponse response = HttpContext.Current.Response;
            if (redirectToUrl != null)
                response.Redirect(redirectToUrl);

            HttpRequest request = HttpContext.Current.Request;
            if (request.QueryString["ReturnUrl"] != null && !string.IsNullOrWhiteSpace(request.QueryString["ReturnUrl"].ToString()))
            {
                response.Redirect("~/" + HttpUtility.UrlDecode(request.QueryString["ReturnUrl"].ToString()));
            }
            else
            {
                response.Redirect(FormsAuthentication.DefaultUrl);
            }
        }

        /// <summary>
        /// Logout the current user and redirect to login url specified for forms authentication in web.config
        /// If session is available, it also abandon the session
        /// </summary>
        /// <param name="redirectAfterLogout">true indicates response is redirected after logout to login url, false means no direction.</param>
        public void LogOut(string cookiesName, bool redirectAfterLogout = true)
        {
            //// Delete the authentication ticket and sign out.
            FormsAuthentication.SignOut();

            //// Clear authentication cookie.
            HttpCookie cookie = new HttpCookie(cookiesName, string.Empty);
            cookie.Expires = DateTime.Now.AddYears(-1);

            HttpResponse response = HttpContext.Current.Response;
            response.Cookies.Add(cookie);

            if (redirectAfterLogout)
                response.Redirect(FormsAuthentication.LoginUrl);
        }

        /// <summary>
        /// This method is to reset user data stored during authentication process.
        /// </summary>        
        /// <param name="user">Enter user data which needs to be reset</param>
        public void ResetLoggedInUser(UserType user, string cookiesName)
        {
            HttpResponse response = HttpContext.Current.Response;

            ////// Clear existing authentication cookie.
            //HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, string.Empty);
            //cookie.Expires = DateTime.Now.AddYears(-1);            
            //response.Cookies.Add(cookie);

            //// Set authentication cookie with new values
            //// Create the authentication ticket with custom user data.
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string userData = serializer.Serialize(user);

            double? timeoutInMinutes = user.RememberMe ? 365 * 24 * 60 : 50;

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                  1,
                  user.Id.ToString(),
                  DateTime.Now,
                  DateTime.Now.AddMinutes(timeoutInMinutes.Value),
                  user.RememberMe,
                  userData,
                  FormsAuthentication.FormsCookiePath);

            //// Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            //// Create the cookie.
            HttpCookie authCookie = new HttpCookie(cookiesName, encTicket);
            authCookie.Expires = DateTime.Now.AddMinutes(timeoutInMinutes.Value);
            authCookie.HttpOnly = true;

            response.Cookies.Add(authCookie);


        }

        #region Private Methods



        /// <summary>
        /// This method is for getting logged in user info from cookie
        /// </summary>        
        /// <returns>returns user</returns>
        private static UserType GetLoggedInUserFromCookie(string cookiesName)
        {
            //if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.User.Identity.Name != null)
            //{
            HttpCookie cookie;
            FormsAuthenticationTicket ticket;

            cookie = HttpContext.Current.Request.Cookies[cookiesName];
            if (cookie == null) return null;

            ticket = FormsAuthentication.Decrypt(cookie.Value);
            if (ticket == null || ticket.UserData == null) return null;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            UserType user = (UserType)serializer.Deserialize(ticket.UserData, typeof(UserType));
            return user;
            //}

            return null;
        }

        #endregion
    }
}
