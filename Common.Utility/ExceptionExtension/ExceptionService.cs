using System;
using System.Collections.Generic;
namespace Common.Utility.ExceptionExtension
{
    public class ExceptionService : IExceptionService
    {
        public void ExceptionSendToMail(Exception ex)
        {
            string exceptionNotifyEmails = Config.GetValue("ExceptionNotifyEmails");
            if (!string.IsNullOrEmpty(exceptionNotifyEmails))
            {
                Dictionary<string, string> replacevalues = new Dictionary<string, string>();
                replacevalues.Add("{loginLink}", Config.SiteURL + "Account/Login");
                replacevalues.Add("{exMessage}", ex.Message.ToString());
                replacevalues.Add("{exDescription}", ex.StackTrace.ToString());
                EmailUtility.Send(exceptionNotifyEmails, " Exception error mail", AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.ExceptionSendToMail), replacevalues);
            }
        }
    }
}
