using Common.Utility;
using Common.Utility.ExceptionExtension;
using Common.Utility.Logger;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PartnerCarrier.Web.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        protected RedirectToRouteResult RedirectToHomePage()
        {
            return RedirectToAction("list", "user");
        }

        protected string RedirectToHomePageJsonURL()
        {
            return "admin/user/list";
        }

        protected ActionResult ReturnModelStateErrors()
        {

            var allErrors = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray();
            var modelStateErrors = new ServerMessage[allErrors.Length];
            for (int error = 0; error < allErrors.Length; error++)
            {
                Response.StatusCode = 400;
                modelStateErrors[error] = new ServerMessage("400", allErrors[error], MessageType.Error, true);
            }
            var modelStateExceptions = new BusinessException(modelStateErrors);
            return Json(modelStateExceptions.ErrorMessages, JsonRequestBehavior.AllowGet);
        }

        protected ActionResult ReturnExceptionResult(Exception ex)
        {


            Response.StatusCode = 500;
            if (ex is BusinessException)
            {
                // handles any errors to be notified to user intensionally due violation of any business logic.
                // for example : duplicate email found, user is not active, invalid credentials etc.
                Response.StatusCode = 400;
                var businesException = (BusinessException)ex;
                foreach (var item in businesException.ErrorMessages)
                {
                    AppLogger.Instance.Log(item.message);
                }

                return Json(businesException.ErrorMessages, JsonRequestBehavior.AllowGet);

            }
            else
            {
                // log any unhandled exception occurred in service or controller for debugging purpose
                // and also send user notification about something went wrong
                AppLogger.Instance.Log(ex);
                ExceptionLogging.ExceptionSendToMail(ex);
                return Json(new ServerMessage("UnhandledError", "Some unhandled error occurred on server. Please try again later or contact site administrator", MessageType.Error, true), JsonRequestBehavior.AllowGet);
            }

        }
    }
}