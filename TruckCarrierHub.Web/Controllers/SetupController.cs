namespace PartnerCarrier.Web.Controllers
{
    using Common.Utility;
    using Common.Utility.ADO.DAL;
    using Common.Utility.DbUpdator;
    using Infrastructure;
    using Infrastructure.Contracts.Setup;
    using Infrastructure.Database;
    using Infrastructure.Services.Admin.AdminManagement;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using ViewModels.Admin;
    using ViewModels.Setup;
    using Web.Filters;


    [RoutePrefix("setup")]
    [EnableSetup]
    public class SetupController : BaseController
    {
        #region Private Variables
        private readonly PartnerCarrier_DevEntities db;
        private readonly ADODbContext adoDb;
        private readonly ISqlManagerService _sqlManagerService;
        private readonly AccountService _accountService;
        #endregion


        public SetupController(PartnerCarrier_DevEntities dba, ISqlManagerService sqlManagerService, AccountService accountService) : base()
        {
            _sqlManagerService = sqlManagerService;
            _accountService = accountService;
            db = dba;
            adoDb = new ADODbContext(db.Database.Connection.ConnectionString);
        }

        /// <summary>
        /// login for setup
        /// </summary>
        /// <returns></returns>
        [Route("login")]
        [HttpGet]
        public ActionResult LogIn(string returnUrl)
        {

            // if user is already logged in, take him to upgradeDb page
            if (User.Identity.IsAuthenticated)
            {
                //if logged in user is setupadmin then it will return view for Upgrade Db page
                //otherwise return back to login page
                if (FormsAuthService.Instance.LoggedInUser(Config.GetValue("SetupLoginAuthenticationName")).Role == ViewModels.Admin.UserRole.SetupAdmin)
                    return View("~/Views/Setup/UpgradeDb.cshtml");
                else
                {
                    FormsAuthService.Instance.LogOut(Config.GetValue("SetupLoginAuthenticationName"), false);
                    HttpContext.ClearError();
                    return View("~/Views/Setup/LogIn.cshtml");
                }
            }

            //to return request URL
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = returnUrl.Contains("LogOff") ? null : returnUrl;
            }
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Setup/LogIn.cshtml");
        }

        /// <summary>
        /// log in for setup
        /// </summary>
        /// <param name="loginInfoVM"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public ActionResult LogIn(LoginInfoVM loginInfoVM, string returnUrl)
        {
            try
            {
                //store return url for if login failed then it viewbag will be null so.
                ViewBag.ReturnUrl = returnUrl;

                //model validation
                if (!ModelState.IsValid) return ReturnModelStateErrors();

                //Call Service Login Function
                var result = _accountService.SetupLogin(loginInfoVM);
                switch (result)
                {
                    //If successfully login
                    case common.SignInStatus.Success:
                        return Json("/setup/upgrade-db", JsonRequestBehavior.AllowGet);
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

        [Route("upgrade-db")]
        [HttpGet]
        [AuthorizeRole(ViewModels.Admin.UserRole.SetupAdmin)]
        public ActionResult UpgradeDb()
        {
            return View("~/Views/Setup/UpgradeDb.cshtml");
        }

        [HttpPost]
        [Route("upgrade-db")]

        public ActionResult SubmitUpgradeDb()
        {
            try
            {
                DbUpdator.Instance.UpgradeDatabase();
                ViewBag.SuccessMessage = "Upgrade database successfully.";

            }
            catch (Exception ex)
            {
                StringBuilder sbLog = new StringBuilder();

                int errorMessageCount = 1;
                do
                {
                    sbLog.Append("Message :- ".PadRight(19));
                    sbLog.Append(ex.Message);
                    if (errorMessageCount > 1 && ex.Message != null)
                    {
                        sbLog.Append(Environment.NewLine);
                        sbLog.Append("Message(" + errorMessageCount + ") : ".PadRight(19));
                        sbLog.Append(ex.Message);
                    }
                    if (ex.StackTrace != null)
                    {
                        if (errorMessageCount > 1)
                            sbLog.Append("StackTrace(" + errorMessageCount + ") : ".PadRight(19));

                        sbLog.AppendLine(ex.StackTrace);
                    }
                    errorMessageCount++;
                    ex = ex.InnerException;
                }
                while (ex != null);


                //string message = string.Format("<b>Message:</b> {0}<br /><br />", ex.Message);
                //message += string.Format("<b>StackTrace:</b> {0}<br /><br />", ex.StackTrace.Replace(Environment.NewLine, string.Empty));
                ModelState.AddModelError(string.Empty, sbLog.ToString());
            }
            return View("~/Views/Setup/UpgradeDb.cshtml");
        }

        /// <summary> 
        /// Get:// Open SQL Manager
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("sql")]
        [AuthorizeRole(ViewModels.Admin.UserRole.SetupAdmin)]
        public ActionResult SqlSample()
        {
            ViewBag.ConnectionString = db.Database.Connection.ConnectionString;
            return View("~/Views/Setup/SqlSample.cshtml");
        }

        /// <summary>
        /// Post:// Execute Query Here
        /// </summary>
        /// <param name="submitButton"></param>
        /// <param name="query"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sql")]

        public ActionResult SqlSample(string query, string dbConnectionString)
        {
            //add Connection In ViewBag
            ViewBag.ConnectionString = dbConnectionString;
            try
            {
                //check type of query
                bool checkTypeOfQuery = _sqlManagerService.checkTypeOfQuery(query);
                if (checkTypeOfQuery)
                {
                    //call method of DDL Query Like Insert/Update/Delete
                    return (ExecuteQuery(query, dbConnectionString));
                }
                else
                {
                    return (SelectQueryResult(query, dbConnectionString));
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "UserEnterNullOrAnythingValueQueryErrorMessage")
                {
                    ViewBag.QueryErrorMessage = "Please enter query";

                    return View("~/Views/Setup/SqlSample.cshtml");
                }
                throw ex;
            }
        }

        /// <summary>
        ///Select Query 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private ActionResult SelectQueryResult(string query, string dbConnectionString)
        {
            try
            {
                //Store Query in ViewBag to Show In The input Box AFter Execution
                ViewBag.Query = query;
                //Separate Multiple No. Of Queries and Store In List
                List<string> saperatedQuery = query.Split(';').ToList();
                //Store separated QueryList In ViewBag
                ViewBag.TotalQuery = saperatedQuery;
                SqlQueryVM result = new SqlQueryVM();
                //Call Service method,it will return Dataset
                result.ExecuteSelectQueryDataSet = _sqlManagerService.SelectQueryResult(query, dbConnectionString);

                return View("~/Views/Setup/SqlSample.cshtml", result);
            }
            catch (Exception ex)
            {
                //store Query Error Message
                ViewBag.QueryErrorMessage = ex.Message;
            }

            return View("~/Views/Setup/SqlSample.cshtml");
        }

        /// <summary>
        /// CRUD
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private ActionResult ExecuteQuery(string query, string dbConnectionString)
        {
            try
            {
                //store query in ViewBag
                ViewBag.Query = query;
                SqlQueryVM resultant = new SqlQueryVM();
                //Call Service method, It will return Int
                resultant.ExecuteQueryResult = _sqlManagerService.ExecuteQuery(query, dbConnectionString);
                //resultant.ExecuteQueryResult = db.Database.ExecuteSqlCommand(query);
                //If User CreateStore Procedure Then Result Will Be In Negative Value,
                //For That Show Message Below
                if (resultant.ExecuteQueryResult < 0)
                {
                    //Show Store Procedure Successful Message
                    ViewBag.SpExecuteSuccessfulMessage = "Command(s) completed successfully.";
                }
                //Insert/Update/Delete
                else
                {
                    //Show Message That How Many Rows Affected
                    ViewBag.NoOfRows = resultant.ExecuteQueryResult;
                }

                return View("~/Views/Setup/SqlSample.cshtml", resultant);
            }
            catch (Exception ex)
            {
                //Show Query Error Message
                ViewBag.QueryErrorMessage = ex.Message;
            }

            return View("~/Views/Setup/SqlSample.cshtml");
        }

        /// <summary>
        /// Test Multiple Email
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("test-email")]

        public ActionResult SendEmailToMultiple()
        {

            return View("~/Views/Setup/SendEmailToMultiple.cshtml");
        }

        [HttpPost]
        [Route("test-email")]

        public ActionResult SendEmailToMultiple(string email)
        {
            try
            {
                //call service method
                bool sendEmail = _sqlManagerService.SendEmailProcess(email);
                ViewBag.EmailSendSuccessfullyMessage = "Successfully sent";

                return View("~/Views/Setup/SendEmailToMultiple.cshtml");
            }
            catch (Exception)
            {
                //ViewBag.EmailSendingFailedMessage = "Email sending failed";
                return View("~/Views/Setup/SendEmailToMultiple.cshtml");


            }
        }

        [HttpGet]
        [Route("test-exception-email")]

        public ActionResult TestExceptionEmail()
        {
            return View("~/Views/Setup/TestExceptionEmail.cshtml");
        }

        [HttpPost]
        [Route("test-exception-email")]

        public ActionResult TestExceptionEmail(string ex)
        {
            throw new Exception();
        }

        //[HttpGet]
        //[Route("encrypt-password")]
        //public ActionResult encryptPassword()
        //{
        //    _sqlManagerService.EncryptPassword();

        //    return RedirectToAction("Login", "Account");
        //}

        #region Logout
        // POST: /Account/LogOff
        [HttpPost]
        [Route("log-off")]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //log out current logged in user
            FormsAuthService.Instance.LogOut(Config.GetValue("SetupLoginAuthenticationName"), false);
            HttpContext.ClearError();
            return RedirectToAction("login", "setup");
        }

        #endregion

        [AllowAnonymous]
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