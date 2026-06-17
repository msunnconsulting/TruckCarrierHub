namespace Common.Utility
{
    using Email;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// enum for outgoing email types
    /// </summary>
    public enum TemplateType
    {
        ForgotPassword = 1,
        TestEmail = 2,
        ExceptionSendToMail = 3,
        TestimonialAddMail = 4,
        ContactUsPersonSendToMail = 5,
        ContactUsDetailsSendMailToAdmin = 6,
        BusinessVerificationMail = 7,
        EmailContent = 8,
        BusinessResetPassowdMail = 9,
        QuoteDettail = 10,
        NewReviewAdminNotificationMail = 11,
        NewReviewCompanyNotificationMail = 12,
        CompanyReviewResponeMail = 13
    }

    public class EmailUtility
    {
        public static string GetTemplate(TemplateType templateType)
        {
            string contentTemplate = null;

            switch (templateType)
            {
                case TemplateType.ForgotPassword:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\ForgotPassword.html");
                    break;
                case TemplateType.TestEmail:
                    contentTemplate = Path.Combine(Config.SitePath, @"TestMail\TestMail.html");
                    break;
                case TemplateType.ExceptionSendToMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"ExceptionExtension\ExceptionSentToMail.html");
                    break;
                case TemplateType.TestimonialAddMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailExtention\AddTestimonial.html");
                    break;
                case TemplateType.ContactUsPersonSendToMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailExtention\ContactUsPersonSendToMail.html");
                    break;
                case TemplateType.ContactUsDetailsSendMailToAdmin:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\ContactUsEmail.html");
                    break;
                case TemplateType.BusinessVerificationMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\BusinessVerificationMail.html");
                    break;
                case TemplateType.EmailContent:
                    contentTemplate = Path.Combine(Config.SitePathForAsync, @"EmailTemplates\EmailContent.html");
                    break;
                case TemplateType.BusinessResetPassowdMail:
                    contentTemplate = Path.Combine(Config.SitePathForAsync, @"EmailTemplates\BusinessResetPasswordMail.html");
                    break;
                case TemplateType.QuoteDettail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\QuoteDetails.html");
                    break;
                case TemplateType.NewReviewAdminNotificationMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\AdminNewReviewNotification.html");
                    break;
                case TemplateType.NewReviewCompanyNotificationMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\CompanyNewReviewNotification.html");
                    break;
                case TemplateType.CompanyReviewResponeMail:
                    contentTemplate = Path.Combine(Config.SitePath, @"EmailTemplates\CompanyReviewResponeMail.html");
                    break;

            }
            return contentTemplate;
        }

        public static void Send(string emailAddress, string subject, string fromEmail, string contentTemplate, Dictionary<string, string> replaceValues)
        {
            //fetch email body from email template
            var contentBody = File.ReadAllText(contentTemplate);

            if (Config.GetValue("DevEmail") != null)
            {
                emailAddress = Config.GetValue("DevEmail");
            }

            //Send Email and get true or false
            SMTPClient.Send(emailAddress, subject, contentBody, fromEmail, replaceValues);
        }

        public static void Send(string emailAddress, string subject, string fromEmail, string contentTemplate, Dictionary<string, string> replaceValues, string[] attachmentPath)
        {
            //fetch email body from email template
            var contentBody = File.ReadAllText(contentTemplate);

            //Send Email and get true or false
            SMTPClient.Send(emailAddress, subject, contentBody, fromEmail, replaceValues, attachmentPath);
        }
    }
}
