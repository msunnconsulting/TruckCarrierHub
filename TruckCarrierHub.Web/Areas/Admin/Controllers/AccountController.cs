namespace PartnerCarrier.Web.Areas.Admin.Controllers
{
    using Common.Utility;
    using Infrastructure;
    using Infrastructure.Contracts.Admin.AdminManagement;
    using System;
    using System.Web.Mvc;
    using ViewModels.Admin;
    using Web.Filters;

    [RouteArea("admin", AreaPrefix = "admin")]
    [RoutePrefix("account")]
    public class AccountController : BaseController
    {

        #region Private variable
        private readonly IAccountService _accountService;
        #endregion

        #region Constructor
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        #endregion

        [Route]
        public ActionResult Index()
        {
            return View("~/Areas/Admin/Views/Account/Index.cshtml");
        }


        /// <summary>
        /// Login GEt Method
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            var adminUserid = FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName"));

            // if user is already logged in, take him to home page
            if (adminUserid != null)
            {
                return RedirectToHomePage();
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = returnUrl.Contains("LogOff") ? null : returnUrl;
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Login Post
        /// </summary>
        /// <param name="loginInfoVM"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LogIn(LoginInfoVM loginInfoVM, string returnUrl)
        {
            try
            {
                //store return url for if login failed then it viewbag will be null so.
                ViewBag.ReturnUrl = returnUrl;
                if (!ModelState.IsValid) return ReturnModelStateErrors();

                //Call Service Login Function
                var result = _accountService.Login(loginInfoVM);
                switch (result)
                {
                    //If successfully login
                    case common.SignInStatus.Success:
                        //return RedirectToLocal(returnUrl);
                        var redirectUrl = ReturnRedirectToLocalURL(returnUrl);
                        return Json(redirectUrl, JsonRequestBehavior.AllowGet);
                    //if user Inactive , Then It shows message
                    case common.SignInStatus.Inactive:
                        throw new BusinessException("400", "Your account is inactive, please contact site administrator.");
                    //If user Does not exist
                    case common.SignInStatus.Failure:
                    default:
                        throw new BusinessException("400", "Invalid login attempt.");
                }
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        #region Logout
        /// <summary>
        /// Admin User Logout
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            FormsAuthService.Instance.LogOut(Config.GetValue("AdminLoginAuthenticationName"), false);
            HttpContext.ClearError();
            return RedirectToAction("Login", "Account");
        }

        #endregion

        #region Edit Profile
        /// <summary>
        /// Edit Admin Profile By LoggedIn User Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AuthorizeRole(UserRole.Admin)]
        [Route("edit-profile")]
        public ActionResult EditProfile()
        {
            var userInfo = _accountService.EditAdminProfile();
            return View(userInfo);
        }

        /// <summary>
        /// For Update Profile Info For LoggedIn User Admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("edit-profile")]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(EditProfileVM editAdminUserVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();
            try
            {
                _accountService.UpdateProfile(editAdminUserVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion

        #region Change Password
        /// <summary>
        /// change password by loggedIn User ID
        /// </summary>
        /// <returns></returns>
        // GET: /Account/ChangePassword
        [AuthorizeRole(UserRole.Admin)]
        [Route("change-password")]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [AuthorizeRole(UserRole.Admin)]
        [ValidateAntiForgeryToken]
        [Route("change-password")]
        public ActionResult ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();
            try
            {
                _accountService.ChangePassword(changePasswordVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion

        #region Forgot-password
        /// <summary>
        /// forgot password
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("forgot-password")]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("forgot-password")]
        public ActionResult ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            if (!ModelState.IsValid) return ReturnModelStateErrors();
            try
            {
                _accountService.ForgotPassword(forgotPasswordVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }

        }


        #endregion

        #region Reset password
        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        [Route("reset-password/{code}")]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password/{code}")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetAccountPassword(string code, ResetPasswordVM resetPasswordVM)
        {

            if (!ModelState.IsValid) return ReturnModelStateErrors();
            try
            {
                _accountService.ResetPassword(code, resetPasswordVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        [Route("reset-password-confirmation")]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();

        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToHomePage();
        }
        //for return url after json  login.        
        private string ReturnRedirectToLocalURL(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            return RedirectToHomePageJsonURL();
        }

        #endregion

        /// <summary>
        /// Error Page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("error")]
        public ActionResult Error()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["msg"]))
                SetErrorPageInfo(Request.QueryString["msg"].ToString());
            else
                SetErrorPageInfo(null);
            return View("Error_Page");

        }

        /// <summary>
        /// sets error text to be set for the label
        /// </summary>
        /// <param name="strMessageType">Message type passed in the query-string</param>        
        public void SetErrorPageInfo(string messageType)
        {
            var errorTitle = "Unhandled Error Occurred";
            var errorMsg = "Unhandled Error Occurred,Please Contact Site Administrator.";
            if (messageType != null)
            {
                // check for the message type and return the text according to that
                switch (messageType)
                {
                    case "errAcc":
                        {
                            errorTitle = "Unauthorized Access";
                            errorMsg = "You are not authorized to access the page you are trying to access";
                            break;
                        }
                    case "errHtml":
                        {
                            errorTitle = "Dangerous Script Entered";
                            errorMsg = "You have used HTML tags in your input text.";
                            break;
                        }
                    case "errNotFound":
                        {
                            errorTitle = "404. Page Not Found";
                            errorMsg = "Page you are looking for is not found";
                            break;
                        }
                    case "BadReq":
                        {
                            errorTitle = "400. Bad Request";
                            errorMsg = "Bad Request or Invalid URL entered";
                            break;
                        }
                    case "countyDbConnectionError":
                        {
                            errorTitle = "Connection Error";
                            errorMsg = "Unable to connect with county database";
                            break;
                        }
                }
            }

            ViewBag.ErrorTitle = errorTitle;
            ViewBag.ErrorMessage = errorMsg;
        }
    }
}