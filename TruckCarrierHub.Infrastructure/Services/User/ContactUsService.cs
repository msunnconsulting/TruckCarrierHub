namespace PartnerCarrier.Infrastructure.Services.User
{
    using Common.Utility;
    using Contracts.User;
    using Database;
    using System.Collections.Generic;
    using ViewModels.User;

    public class ContactUsService : IContactUsService
    {
        #region
        private readonly PartnerCarrier_DevEntities db;
        #endregion

        #region
        public ContactUsService(PartnerCarrier_DevEntities dba)
        {
            db = dba;
        }
        #endregion

        #region
        /// <summary>
        /// Send Contact Us Details to admin from Contact Us Page
        /// first get all required values
        /// after that replace all values in contactusEmail.html page
        /// send mail to admin.
        /// </summary>
        /// <param name="contactUSVM"></param>
        public void SendContactUsDetailsToAdminEmail(ContactUsVM contactUSVM)
        {
            //set replace values for email
            Dictionary<string, string> replacevalues = new Dictionary<string, string>();

            //IF phone number and Message is null or empty set N/A 
            if (string.IsNullOrEmpty(contactUSVM.Phone))
            {
                contactUSVM.Phone = "N/A";
            }
            if (string.IsNullOrEmpty(contactUSVM.Message))
            {
                contactUSVM.Message = "N/A";
            }
            //now we have to replace all these values in contactus.html page
            replacevalues.Add("{contactName}", contactUSVM.Name);
            replacevalues.Add("{email}", contactUSVM.Email);
            replacevalues.Add("{phone}", contactUSVM.Phone);
            replacevalues.Add("{subject}", contactUSVM.Subject);
            replacevalues.Add("{message}", contactUSVM.Message);
            string content = EmailUtility.GetTemplate(TemplateType.ContactUsDetailsSendMailToAdmin);
            //get admin mail from our web.config file
            var adminEmail = Config.GetValue("AdminNotificationEmail");
            EmailUtility.Send(adminEmail, contactUSVM.Subject, AppSettings.FromEmail, content, replacevalues);
        }
        #endregion

    }
}
