

namespace PartnerCarrier.Infrastructure.Services.Admin.AdminManagement
{
    using Common.Utility;
    using Common.Utility.Logger;
    using Common.Utility.ViewModels;
    using Contracts.Admin.AdminManagement;
    using Database;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ViewModels.Admin;
    using ViewModels.User;
    using static ViewModels.Admin.common;

    public class BusinessMangementService : IBusinessMangementService
    {
        private readonly PartnerCarrier_DevEntities db;

        private static List<SentEmailsProgressInfoVM> lstSentEmailsProgressInfoVM = null;
        private readonly HttpClient _httpClient;


        public BusinessMangementService(PartnerCarrier_DevEntities _db)
        {
            db = _db;
            db.Database.CommandTimeout = 300;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Get emails list by EmailID sort order Ascending
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public PagedList<EmailsVM> GetEmailList(PageSortPara ps)
        {
            int pageSize = 5000;
            PageSortPara.Init(ps, "EmailID", SortingDirection.Asc);
            var emailList = (from business in db.Emails.AsNoTracking()
                             orderby business.EmailID
                             select new EmailsVM
                             {
                                 EmailID = business.EmailID,
                                 Content = business.Content,
                                 CreatedDate = business.CreatedDate,
                                 EmailSent = business.EmailsSents.Count(),
                                 LastDateSent = business.EmailsSents.OrderByDescending(x => x.SentDate).Select(x => x.SentDate).FirstOrDefault(),
                                 LinkNeeded = business.LinkNeeded,
                                 Subject = business.Subject,
                                 UpdatedDate = business.UpdatedDate,
                             });

            return emailList.SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
        }

        /// <summary>
        /// Get email by email id
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        public EmailsVM GetEmailByEmailId(int emailId)
        {
            return (from business in db.Emails.AsNoTracking()
                    where business.EmailID == emailId
                    select new EmailsVM
                    {
                        EmailID = business.EmailID,
                        Content = business.Content,
                        CreatedDate = business.CreatedDate,
                        EmailSent = 0,
                        LinkNeeded = business.LinkNeeded,
                        Subject = business.Subject,
                        UpdatedDate = business.UpdatedDate,
                    }).FirstOrDefault();
        }

        /// <summary>
        /// Save email details
        /// </summary>
        /// <param name="emailsVM"></param>
        /// <returns></returns>
        public long SaveEmail(EmailsVM emailsVM)
        {
            Email email = new Email();
            email.Content = emailsVM.Content;
            email.UpdatedDate = DateTime.Now;
            email.Subject = emailsVM.Subject;
            email.LinkNeeded = emailsVM.LinkNeeded;

            if (emailsVM.EmailID.HasValue)
            {
                email.EmailID = emailsVM.EmailID.Value;
                return db.Emails.UpdatePartial(db, email, true, "Content", "UpdatedDate", "Subject", "LinkNeeded").EmailID;

            }
            else
            {
                email.CreatedDate = DateTime.Now;
                return db.Emails.Insert(db, email).EmailID;
            }
        }

        /// <summary>
        /// Get Email Details For Send Mail By EmailId
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        public SendEmailsVM GetEmailDetailsForSendMailByEmailId(int emailId)
        {
            var emailList = (from emails in db.Emails.AsNoTracking()
                             where emails.EmailID == emailId
                             select new SendEmailsVM
                             {
                                 EmailID = emails.EmailID,
                                 RealTimeCounter = emails.EmailsSents.Count(),
                                 Subject = emails.Subject,
                                 LinkNeeded = emails.LinkNeeded

                             }).FirstOrDefault();
            return emailList;
        }

        /// <summary>
        /// Send email by filtered criteria.
        /// </summary>
        /// <param name="sendEmailsVM"></param>
        public void SendEmails(SendEmailsVM sendEmailsVM)
        {

            GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).IsInProgress = true;
            GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).TotalMailToSent = sendEmailsVM.EmailsToSend;
            GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).MailSentFailed = 0;
            GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).MailSentSuccessful = 0;
            GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).RealTimeCounter = 0;
            GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).TotalRecordCountForMail = 0;
            using (var dbCopyData = new PartnerCarrier_DevEntities(true, null, true, null, null))
            {
                dbCopyData.Database.CommandTimeout = 360;
                //get all record from transport table where emails are not null
                var lstCompany = dbCopyData.TransportCompanies.Where(a => a.EmailAddress != null).Select(companies => new CompanyForSendMailVM
                {
                    LegalName = companies.LegalName,
                    DoingBusinessAsName = companies.DoingBusinessAsName,
                    PhysicalAddressCity = companies.PhysicalAddressCity,
                    PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                    PhysicalAddressStreet = companies.PhysicalAddressStreet,
                    PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                    USDOTNumber = companies.USDOTNumber,
                    OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                    CellPhoneNumber = companies.CellPhoneNumber,
                    City = companies.PhysicalAddressCity,
                    StateCode = companies.PhysicalAddressStateCode,
                    TruckOrTractor = companies.TrucksAndTractors,
                    TotalNumberOfTrucks = companies.TotalNumberOfTrucks,
                    CompanyName = companies.CompanyName,
                    intDateAdded = companies.DateAdded,
                    intDateUpdated = companies.DateLastChanged,
                    CompanyEmailAddress = companies.EmailAddress,
                }).AsQueryable();

                if (sendEmailsVM.MinNoOfTrucks.HasValue && sendEmailsVM.MaxNoOfTrucks.HasValue)
                {
                    lstCompany = lstCompany.Where(x => x.TotalNumberOfTrucks >= sendEmailsVM.MinNoOfTrucks && x.TotalNumberOfTrucks <= sendEmailsVM.MaxNoOfTrucks);
                }
                else
                {
                    if (sendEmailsVM.MinNoOfTrucks.HasValue)
                    {
                        lstCompany = lstCompany.Where(x => x.TotalNumberOfTrucks >= sendEmailsVM.MinNoOfTrucks);
                    }
                    if (sendEmailsVM.MaxNoOfTrucks.HasValue)
                    {
                        lstCompany = lstCompany.Where(x => x.TotalNumberOfTrucks <= sendEmailsVM.MaxNoOfTrucks);
                    }
                }

                if (sendEmailsVM.ToCompaniesAddedAfterTheDate.HasValue) //if ToCompaniesAddedAfterTheDate has value then date into int "yyyyMMdd" format and then search, In database date is stored in int format
                {
                    var intDate = Convert.ToInt64(sendEmailsVM.ToCompaniesAddedAfterTheDate.Value.ToString("yyyyMMdd"));
                    lstCompany = lstCompany.Where(x => x.intDateAdded > intDate);
                }
                if (sendEmailsVM.ToCompaniesUpdatedAfterTheDate.HasValue) //if ToCompaniesUpdatedAfterTheDate has value then date into int "yyyyMMdd" format and then search, In database date is stored in int format
                {
                    var intDate = Convert.ToInt64(sendEmailsVM.ToCompaniesUpdatedAfterTheDate.Value.ToString("yyyyMMdd"));
                    lstCompany = lstCompany.Where(x => x.intDateUpdated > intDate);
                }
                if (!string.IsNullOrEmpty(sendEmailsVM.State) && sendEmailsVM.State != "--Select--") // If State has value then search state code 
                {
                    lstCompany = lstCompany.Where(x => x.StateCode == sendEmailsVM.State);
                }
                if (!string.IsNullOrEmpty(sendEmailsVM.City) && sendEmailsVM.City != "--Select--") // If City has value then search city from transport table
                {
                    lstCompany = lstCompany.Where(x => x.City == sendEmailsVM.City);
                }
                ///Join with business table for check that user unsubscribed newsletter 
                ///CommunicationApproved == false means unsubscribed newsletter 
                lstCompany = (from transportTbl in lstCompany
                              join busineses in dbCopyData.Businesses on transportTbl.USDOTNumber equals busineses.USDOTNumber into businessTable
                              from business in businessTable.DefaultIfEmpty()
                              where (business == null || business.CommunicationApproved == true)
                              select transportTbl);

                //Get Email already sent to user list by EmailId
                var emailSent = dbCopyData.EmailsSents.Where(x => x.EmailID == sendEmailsVM.EmailID);

                //No need to send email to user who has already mail sent 
                //emailsent == null means still pending to mail
                lstCompany = (from p in lstCompany
                              join emailsent in emailSent on p.USDOTNumber equals emailsent.USDOTNumber into lstEmailsent
                              from emailsent in lstEmailsent.DefaultIfEmpty()
                              where emailsent == null
                              select p);
                var totalEmailSent = emailSent.Count(); // Store sent email count in global variable for display real time counter in Send email page.

                var lstOfMailReadyToSent = lstCompany.Take(sendEmailsVM.EmailsToSend).ToList(); // Take only those records for number of  EmailsToSend.

                if (lstOfMailReadyToSent.Count() < sendEmailsVM.EmailsToSend)
                {
                    GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).CheckIsRecordsEnoughToUpdate = false;
                }
                else
                {
                    GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).CheckIsRecordsEnoughToUpdate = true;
                }

                //Fetch email record by email id for use subject, content and LinkNeeded value.
                var emailContent = (from emails in dbCopyData.Emails.AsNoTracking()
                                    where emails.EmailID == sendEmailsVM.EmailID.Value
                                    select new EmailsVM
                                    {
                                        EmailID = emails.EmailID,
                                        Content = emails.Content,
                                        CreatedDate = emails.CreatedDate,
                                        EmailSent = 0,
                                        LinkNeeded = emails.LinkNeeded,
                                        Subject = emails.Subject,
                                        UpdatedDate = emails.UpdatedDate,
                                    }).FirstOrDefault();

                //Total mail to send 
                GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).TotalRecordCountForMail = lstOfMailReadyToSent.Count();


                //iterate loop for send email one by one
                foreach (var item in lstOfMailReadyToSent)
                {

                    if (!GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).IsInProgress) break;//If IsInProgress == false then break process

                    string emailContents = emailContent.Content;

                    ///If LinkNeeded == true then replace "your page" word from content with Company url
                    if (emailContent.LinkNeeded)
                    {
                        var siteFullURL = GenerateCompanyUrlByUSDOTNumber(item, sendEmailsVM.SiteURL); //By company detail create company url
                        string anchorTagForFullSiteUrl = "<a href='" + siteFullURL + "'>" + siteFullURL + "</a>";
                        emailContents = emailContents.Replace("your page", anchorTagForFullSiteUrl);
                    }

                    //Replace value for email template
                    Dictionary<string, string> replacevalues = new Dictionary<string, string>();
                    replacevalues.Add("{emailTableContent}", emailContents);
                    replacevalues.Add("{userEmail}", item.CompanyEmailAddress);
                    replacevalues.Add("{unsubscribeURL}", sendEmailsVM.SiteURL + "email/unsubscribe/" + item.USDOTNumber);
                    try
                    {
                        //Send email 
                        EmailUtility.Send(item.CompanyEmailAddress, emailContent.Subject, AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.EmailContent), replacevalues);
                        GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).MailSentSuccessful = GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).MailSentSuccessful + 1; //if mail is sent then increase successful counter by 1
                        GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).RealTimeCounter = totalEmailSent = totalEmailSent + 1;  //if mail is sent then increase RealTime Counter by 1
                        //if mail is sent then log it into EmailSent with EmailID and USDOTNumber
                        var emailSents = new EmailsSent()
                        {
                            EmailID = emailContent.EmailID.Value,
                            USDOTNumber = item.USDOTNumber,
                            SentDate = DateTime.Now
                        };
                        dbCopyData.EmailsSents.Insert(dbCopyData, emailSents);
                    }
                    catch (Exception ex)
                    {
                        GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).MailSentFailed = GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).MailSentFailed + 1;//if mail is not sent then increase MailSentFailed counter  by 1
                        //Log mail failed exception
                        AppLogger.Instance.Log("EmailId: " + sendEmailsVM.EmailID.Value + ", USDOTNumber: " + item.USDOTNumber + ", EmailAddress: " + item.CompanyEmailAddress);
                        AppLogger.Instance.Log(ex);

                    }
                    //If we have only one mail to send then do not wait for thread their should not be any delay for sending first email.
                    //Mail functionality only wait when mail to send is greater then or equalt to 2
                    if (lstOfMailReadyToSent.Count > 1)
                    {
                        //Sleep for 60 second as per client requirement ("I’m not going to send too many emails. You may pause 60 seconds between emails.") 
                        //But as per discuss with Amit sir, Get second's value from web.config file and set into sleep method while sending mail.
                        var SendMailSleep = Convert.ToInt32(Config.GetValue("SendMailSleep"));
                        var SendMailSleepInMilisecond = (SendMailSleep * 1000);
                        Thread.Sleep(SendMailSleepInMilisecond);
                    }
                }
                GetSentEmailsProgressInfo(sendEmailsVM.EmailID.Value).IsInProgress = false; // Once process is done then set IsInProgress = false
            }
        }

        /// <summary>
        /// Get Sent Emails Progress Info by email id because at a time user can send multiple emails from send email page.
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        public SentEmailsProgressInfoVM GetSentEmailsProgressInfo(int emailId)
        {
            SentEmailsProgressInfoVM sentEmailsProgressInfoVM = null;//Declare SentEmailsProgressInfoVM  object as null

            if (lstSentEmailsProgressInfoVM == null) // if global declared variable for list of SentEmailsProgressInfoVM (lstSentEmailsProgressInfoVM) is null then 
            {
                lstSentEmailsProgressInfoVM = new List<SentEmailsProgressInfoVM>(); // initialize lstSentEmailsProgressInfoVM variable
                sentEmailsProgressInfoVM = new SentEmailsProgressInfoVM(); // Now initialize SentEmailsProgressInfoVM view model and set emailId in emailID field and add into lstSentEmailsProgressInfoVM variable
                sentEmailsProgressInfoVM.EmailID = emailId;
                lstSentEmailsProgressInfoVM.Add(sentEmailsProgressInfoVM);

            }
            else
            {
                sentEmailsProgressInfoVM = lstSentEmailsProgressInfoVM.Where(x => x.EmailID == emailId).FirstOrDefault(); // if lstSentEmailsProgressInfoVM is not null then get view model object by emailID
                if (sentEmailsProgressInfoVM == null) // If sentEmailsProgressInfoVM == null then it means for current emailID no send email process is InProgress. So now initialize SentEmailsProgressInfoVM view model and set emailId in emailID field and add into lstSentEmailsProgressInfoVM variable
                {
                    sentEmailsProgressInfoVM = new SentEmailsProgressInfoVM();
                    sentEmailsProgressInfoVM.EmailID = emailId;
                    lstSentEmailsProgressInfoVM.Add(sentEmailsProgressInfoVM);
                }
            }
            return sentEmailsProgressInfoVM;
        }

        /// <summary>
        /// Stop Sent Emails Progress Info
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        public SentEmailsProgressInfoVM StopSentEmailsProgressInfo(int emailId)
        {
            GetSentEmailsProgressInfo(emailId).IsInProgress = false;
            return GetSentEmailsProgressInfo(emailId);
        }

        /// <summary>
        /// Generate Company Url By USDOTNumber
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        private string GenerateCompanyUrlByUSDOTNumber(CompanyForSendMailVM company, string sitreURL)
        {
            var state = company.PhysicalAddressStateCode;

            return sitreURL + state + "/USDOT-" + @company.USDOTNumber;
        }

        /// <summary>
        /// Unsubscribe email by USDOTNumber
        /// </summary>
        /// <param name="USDOTNumber"></param>
        public void UnSubscribeEmail(int USDOTNumber)
        {
            var bussinessDetails = db.Businesses.Where(x => x.USDOTNumber == USDOTNumber).FirstOrDefault(); //get business record by USDOTNumber
            if (bussinessDetails == null) // if business record is null then we need to insert record for unSubscribe so set website = null and set other required column value
            {
                bussinessDetails = new Business();
                bussinessDetails.USDOTNumber = USDOTNumber;
                bussinessDetails.EmailVerified = true;
                bussinessDetails.CommunicationApproved = false;
                bussinessDetails.Website = "";
                bussinessDetails.CreatedDate = DateTime.Now;
                bussinessDetails.NowHiring = false;
                db.Businesses.Insert(db, bussinessDetails);
            }
            else
            {
                //otherwise update only CommunicationApproved = false and updated date
                bussinessDetails.CommunicationApproved = false;
                bussinessDetails.UpdatedDate = DateTime.Now;
                db.Businesses.UpdatePartial(db, bussinessDetails, true, "CommunicationApproved", "UpdatedDate");
            }
        }

        /// <summary>
        /// Save Success Stories Details to Admin table
        /// </summary>
        /// <param name="successStoriesVM"></param>
        public void SaveSuccessStories(SuccessStoriesVM successStoriesVM)
        {
            //Update Record in Admin for Success Stories
            db.Database.ExecuteSqlCommand("UPDATE Admin SET SuccessStories = '" + successStoriesVM.Content + "', SuccessStoryPublished = '" + successStoriesVM.SuccessStoryPublished + "'");
        }

        /// <summary>
        /// Get Success Story from Admin Table
        /// </summary>
        /// <returns></returns>
        public SuccessStoriesVM GetSuccessStory()
        {
            //Get Success Story  Record from Admin table
            SuccessStoriesVM successStoriesVM = new SuccessStoriesVM();
            successStoriesVM.Content = db.Database.SqlQuery<string>("select SuccessStories from Admin").FirstOrDefault();
            successStoriesVM.SuccessStoryPublished = db.Database.SqlQuery<bool>("select SuccessStoryPublished from Admin").FirstOrDefault();
            return successStoriesVM;
        }

        //Get Global Hiring
        public GlobalHiringVM GetGlobalHiring()
        {
            //Get Global Hiring from Admin table
            GlobalHiringVM globalHiringVM = new GlobalHiringVM();
            int globalHiring = db.Database.SqlQuery<int>("select GlobalHiring from Admin").FirstOrDefault();
            globalHiringVM.GlobalHire = globalHiring;

            return globalHiringVM;
        }

        /// <summary>
        /// Save Global Hiring
        /// </summary>
        /// <param name="globalHiringVM"></param>
        public void SaveGlobalHiring(GlobalHiringVM globalHiringVM)
        {
            //Update Record in Admin for Success Stories
            db.Database.ExecuteSqlCommand("UPDATE Admin SET GlobalHiring = '" + globalHiringVM.GlobalHire + "'");
        }


        /// <summary>
        /// Get All Business List
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public PagedList<BusinessListVM> GetBusinessSearchList(PageSortPara ps, BusinessSearchVM businessSearchVM)
        {
            int pageSize = 15;
            ps.p = ((!ps.p.HasValue) || ps.p == 0) ? 1 : Convert.ToInt32(ps.p);
            ps.se = String.IsNullOrEmpty(ps.se) ? "USDOTNumber" : ps.se;
            ps.sd = String.IsNullOrEmpty(ps.sd) ? "Asc" : ps.sd;

            var businessOrWaitingForApprovalList = (from bussines in db.Businesses.AsNoTracking()
                                                    orderby bussines.USDOTNumber
                                                    select new BusinessListVM
                                                    {
                                                        USDOTNumber = bussines.USDOTNumber,
                                                        Website = bussines.Website,
                                                        EmailVerified = bussines.EmailVerified,
                                                        WebsiteApproved = bussines.WebsiteApproved,
                                                        CommunicationApproved = bussines.CommunicationApproved,
                                                        CreatedDate = bussines.CreatedDate,
                                                        UpdatedDate = bussines.UpdatedDate,
                                                        VerificationKey = bussines.VerificationKey,
                                                        PasswordHash = bussines.PasswordHash,
                                                        PasswordSalt = bussines.PasswordSalt,
                                                        BusinessContactEmail = bussines.BusinessContactEmail,
                                                        JobContactEmail = bussines.JobContactEmail,
                                                        JobContactPhone = bussines.JobContactPhone,
                                                        JobContactSMS = bussines.JobContactSMS,
                                                        NowHiring = bussines.NowHiring,
                                                        ForgotPasswordKey = bussines.ForgotPasswordKey
                                                    }).AsQueryable();

            var sortOrder = ps.se + "_" + ps.sd;
            switch (sortOrder)
            {

                case "USDOTNumber_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.USDOTNumber);
                    break;
                case "USDOTNumber_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.USDOTNumber);
                    break;
                case "WebsiteName_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.Website);
                    break;
                case "WebsiteName_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.Website);
                    break;
                case "EmailVerified_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.EmailVerified);
                    break;
                case "EmailVerified_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.EmailVerified);
                    break;
                case "WebsiteApproved_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.WebsiteApproved);
                    break;
                case "WebsiteApproved_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.WebsiteApproved);
                    break;
                case "BusinessContactEmail_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.BusinessContactEmail);
                    break;
                case "BusinessContactEmail_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.BusinessContactEmail);
                    break;
                case "CommunicationApproved_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.CommunicationApproved);
                    break;
                case "CommunicationApproved_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.CommunicationApproved);
                    break;
            }

            //If BusinessContactEmail is not string NULLOREMPTY then check contains with BusinessContactEmail field
            if (!string.IsNullOrEmpty(businessSearchVM.BusinessContactEmail))
            {
                businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.Where(t => t.BusinessContactEmail.Contains(businessSearchVM.BusinessContactEmail));
            }

            //If USDOTNumber has value then check exact USDOTNumber
            if (businessSearchVM.USDOTNumber.HasValue)
            {
                businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.Where(t => t.USDOTNumber == businessSearchVM.USDOTNumber.Value);
            }
            //If UpdatedAfter has value then get all data which are updated after selected date
            if (businessSearchVM.UpdatedAfter.HasValue)
            {
                businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.Where(t => DbFunctions.TruncateTime(t.UpdatedDate) > DbFunctions.TruncateTime(businessSearchVM.UpdatedAfter));
            }

            //If default check box is selected then no need to write query.
            //If "only records with approved websites" checkbox is checked, it means "businessSearchVM.ApprovedWebsite == Approved" so get all approved records
            if (businessSearchVM.ApprovedWebsite == "Approved")
            {
                businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.Where(t => t.WebsiteApproved == true);
            }
            //If "only records with not approved websites" checkbox is checked, it means "businessSearchVM.ApprovedWebsite == NotApproved" so get all business data which is WebsiteApproved== null or WebsiteApproved == false
            else if (businessSearchVM.ApprovedWebsite == "NotApproved")
            {
                businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.Where(t => t.WebsiteApproved != true && (t.Website != null && t.Website != "")); // t.WebsiteApproved != true means all records which  WebsiteApproved  == false or WebsiteApproved == null //Get NotApproved with which has WebsiteName not empty.
            }
            PagedList<BusinessListVM> allBusinessOrWaitingForApprovalList = businessOrWaitingForApprovalList.AsQueryable().SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
            return allBusinessOrWaitingForApprovalList;
        }

        /// <summary>
        /// Delete Business by BusinessId
        /// </summary>
        public void DeleteBusinessById(int businessId)
        {
            //Delete Record from Business table
            Business business = new Business();
            business.USDOTNumber = businessId;
            db.Businesses.Delete(db, business);
        }

        /// <summary>
        /// Update Business Details
        /// </summary>
        /// <param name="businessSearchVM"></param>
        public void UpdateBusiness(BusinessListVM businessSearchVM)
        {
            var businessupdate = (from busines in db.Businesses.AsNoTracking()
                                  where busines.USDOTNumber == businessSearchVM.USDOTNumber
                                  select busines).FirstOrDefault();
            if (businessupdate != null)
            {
                Business business = new Business();
                business.USDOTNumber = businessSearchVM.USDOTNumber;
                business.Website = businessSearchVM.Website;
                business.EmailVerified = businessSearchVM.EmailVerified;
                business.WebsiteApproved = businessSearchVM.WebsiteApproved;
                business.CommunicationApproved = businessSearchVM.CommunicationApproved;
                business.BusinessContactEmail = businessSearchVM.BusinessContactEmail;
                business.UpdatedDate = DateTime.Now;

                db.Businesses.UpdatePartial(db, business, true, "USDOTNumber", "Website", "EmailVerified", "WebsiteApproved", "CommunicationApproved", "BusinessContactEmail", "UpdatedDate");

            }
        }

        /// <summary>
        /// Business Reset Password Mail Send
        /// </summary>
        /// <param name="usDOTNumber"></param>
        public void BussinessResetPasswordMailSend(BusinessForgotPasswordVM businessForgotPasswordVM)
        {


            //Initialize transaction
            var transactionBusiness = db.Database.BeginTransaction();
            try
            {
                var transportCompany = new BusinessEmailAndUSDOTNumberVM();

                //This method is used from Manage bussiness and Reset password from user login page.
                // So when this method called from user login page then IsEmailAddressCheck == true means check email addess
                // And when this method is called from Manage Business page then we have passed USDOTNumber so IsEmailAddressCheck == false.
                if (businessForgotPasswordVM.IsEmailAddressCheck)
                {

                    transportCompany = (from transport in db.TransportCompanies
                                        where transport.EmailAddress == businessForgotPasswordVM.Email
                                        select new BusinessEmailAndUSDOTNumberVM { USDOTNumber = transport.USDOTNumber, EmailAddress = transport.EmailAddress }).FirstOrDefault();
                }
                else
                {
                    transportCompany = (from transport in db.TransportCompanies
                                        where transport.USDOTNumber == businessForgotPasswordVM.USDOTNumber
                                        select new BusinessEmailAndUSDOTNumberVM { USDOTNumber = transport.USDOTNumber, EmailAddress = transport.EmailAddress }).FirstOrDefault();
                }


                if (transportCompany == null)
                {
                    throw new BusinessException("EmailNotFound", "Email Address is not exist.");
                }

                Business business = new Business();
                business.ForgotPasswordKey = Guid.NewGuid().ToString();
                if (businessForgotPasswordVM.USDOTNumber.HasValue)
                {
                    business.USDOTNumber = businessForgotPasswordVM.USDOTNumber.Value;
                }
                else
                {
                    business.USDOTNumber = transportCompany.USDOTNumber.Value;
                }

                //set replace values for email
                Dictionary<string, string> replacevalues = new Dictionary<string, string>();

                //now we have to replace all these values in contactus.html page
                replacevalues.Add("{VerifyLink}", Config.SiteURL + "reset-password/" + business.ForgotPasswordKey);

                //Apply try catch for knowing the exact which exception is fired and display error message based on exception
                try
                {
                    EmailUtility.Send(transportCompany.EmailAddress, "Reset Password", AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.BusinessResetPassowdMail), replacevalues);
                }
                catch (Exception ex)
                {
                    throw new BusinessException("MailSendingFailed", "Email sending failed. Please try again later.");
                }
                db.Businesses.UpdatePartial(db, business, true, "ForgotPasswordKey");
                //Commit transaction
                transactionBusiness.Commit();
            }
            catch (Exception ex)
            {
                //Rollback transaction
                transactionBusiness.Rollback();
                throw ex;
            }
        }

        /// <summary>
        /// Check Reset Password Key Is Valid or not
        /// </summary>
        /// <param name="resetPasswordKey"></param>
        /// <returns></returns>
        public BusinessVM CheckResetPasswordKeyIsValid(Guid? resetPasswordKey)
        {
            var strGuid = resetPasswordKey.Value.ToString();
            var bussinessData = db.Businesses.Where(x => x.ForgotPasswordKey == strGuid).FirstOrDefault(); // check guid is exist in business table or not. If not exist then throw exception
            if (bussinessData != null) // If guid is exist in business table then get BusinessName and LegalName for display in view
            {
                var bussinessVM = db.TransportCompanies.Where(x => x.USDOTNumber == bussinessData.USDOTNumber).Select(x => new BusinessVM
                {
                    DoingBusinessAsName = x.DoingBusinessAsName,
                    LegalName = x.LegalName,
                    USDOTNumber = x.USDOTNumber
                }).FirstOrDefault();

                return bussinessVM;
            }
            else
            {
                throw new BusinessException("ResetPassword", "Password has been reset already.");
            }
        }


        /// <summary>
        /// Business Reset Password
        /// </summary>
        /// <param name="businessVM"></param>
        public void BussinessResetPassword(BusinessVM businessVM)
        {

            var business = (from transport in db.Businesses
                            where transport.USDOTNumber == businessVM.USDOTNumber
                            select transport).FirstOrDefault();

            if (business != null)
            {
                if (string.IsNullOrEmpty(business.PasswordHash) || string.IsNullOrEmpty(business.PasswordSalt))
                {
                    business.PasswordSalt = PasswordGenerator.GetSalt();
                    business.PasswordHash = PasswordGenerator.GetHashedPassword(business.PasswordSalt, businessVM.ConfirmPassword);
                }
                else
                {
                    //Reset Password Here
                    business.PasswordHash = PasswordGenerator.GetHashedPassword(business.PasswordSalt, businessVM.ConfirmPassword);
                }

                business.PasswordHash = PasswordGenerator.GetHashedPassword(business.PasswordSalt, businessVM.Password);
                business.ForgotPasswordKey = null;
                business.InvalidLoginAttempt = 0;
                db.SaveChanges();
            }
            else
                throw new BusinessException("400", "Reset password link is expired.");
        }

        public common.SignInStatus BusinessLogin(BusinessLoginVM businessLoginVM)
        {
            var userInfo = (from user in db.TransportCompanies
                            join business in db.Businesses on user.USDOTNumber equals business.USDOTNumber
                            where user.EmailAddress == businessLoginVM.Email
                            select new LoggedInUserVM
                            {
                                Email = user.EmailAddress,
                                PasswordSalt = business.PasswordSalt,
                                PasswordHash = business.PasswordHash,
                                USDOTNumber = user.USDOTNumber,
                                IsActive = business.EmailVerified ?? false
                            }).FirstOrDefault();
            //if user Email And Password does not Exist 
            if (userInfo == null || string.IsNullOrEmpty(userInfo.PasswordSalt))
            {
                return SignInStatus.Failure;
            }
            if (userInfo.PasswordHash != PasswordGenerator.GetHashedPassword(userInfo.PasswordSalt, businessLoginVM.Password))
            {
                var bussinesRecordForTooManyAttamptForCheck = db.Businesses.Where(x => x.USDOTNumber == userInfo.USDOTNumber).FirstOrDefault();

                bussinesRecordForTooManyAttamptForCheck.InvalidLoginAttempt = bussinesRecordForTooManyAttamptForCheck.InvalidLoginAttempt == null ? 1 : bussinesRecordForTooManyAttamptForCheck.InvalidLoginAttempt + 1;
                db.SaveChanges();
                if (bussinesRecordForTooManyAttamptForCheck.InvalidLoginAttempt > 3)
                {
                    return SignInStatus.TooManyAttempt;
                }
                else
                {
                    return SignInStatus.Failure;
                }
            }
            // verify if user is active
            if (!userInfo.IsActive)
            {
                return SignInStatus.Inactive;
            }

            //Get Role Name from Role id from user role enum.
            userInfo.Role = UserRole.BusinessUser;
            userInfo.RoleId = (int)UserRole.BusinessUser;

            //return the user information on success
            FormsAuthService.Instance.LogIn(userInfo, Config.GetValue("BusinessLoginAuthenticationName"), redirectAfterLogin: false);


            var bussinesRecordForTooManyAttampt = db.Businesses.Where(x => x.USDOTNumber == userInfo.USDOTNumber).FirstOrDefault();
            if (bussinesRecordForTooManyAttampt.InvalidLoginAttempt > 3)
            {
                return SignInStatus.TooManyAttempt;
            }
            else
            {
                bussinesRecordForTooManyAttampt.InvalidLoginAttempt = 0;
                db.SaveChanges();
            }


            ////return log;
            return SignInStatus.Success;
        }

        /// <summary>
        /// Save details of Get A Quote Control to show on pages which user selected checkboxes only
        /// </summary>
        /// <param name="manageGetAQuoteVM"></param>
        public void SaveGetAQuoteToShow(ManageGetAQuoteVM manageGetAQuoteVM)
        {
            foreach (var item in manageGetAQuoteVM.GetAQuoteToShowList)
            {
                GetAQuoteToShow getAQuoteToShow = new GetAQuoteToShow();

                getAQuoteToShow.Id = item.Id;
                getAQuoteToShow.Name = item.Name;
                getAQuoteToShow.ControlToShow = item.ControlToShow;

                db.GetAQuoteToShows.UpdatePartial(db, getAQuoteToShow, true, "ControlToShow");
            }
        }

        /// <summary>
        /// Get Checkbox list for Show Get A Quote control
        /// </summary>
        /// <returns></returns>
        public List<GetAQuoteToShowVM> GetCheckboxListForShowGetAQuoteControl()
        {
            return (from getAQuoteToShows in db.GetAQuoteToShows
                    select new GetAQuoteToShowVM
                    {
                        Id = getAQuoteToShows.Id,
                        Name = getAQuoteToShows.Name,
                        ControlToShow = getAQuoteToShows.ControlToShow
                    }).ToList();
        }

        /// <summary>
        /// Get Checkbox list for Load Type
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, int>> BindCheckboxListForLoadType()
        {
            var lists = (from getAQuoteToShows in db.LoadTypes
                         select new LoadTypeVM
                         {
                             Id = getAQuoteToShows.Id,
                             LoadName = getAQuoteToShows.Name,
                             LoadDescription = getAQuoteToShows.Description
                         }).ToList();

            //Initialize list for store load content value id and loadname 
            var list = new List<KeyValuePair<string, int>>();

            //Iterate loop and add 1 by 1 item from enum sidebar list into key and value pair list to display in view
            for (int i = 0; i < lists.Count; i++)
            {
                list.Add(new KeyValuePair<string, int>(lists[i].LoadName, Convert.ToInt32(lists[i].Id)));
            }

            //Return list
            return list;
        }

        /// <summary>
        /// Bind Checkbox list for State From and TO
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> BindCheckboxListForState()
        {
            var lists = (from getAQuoteToShows in db.States
                         select new StateVM
                         {
                             StateCode = getAQuoteToShows.StateCode,
                             State = getAQuoteToShows.State1,
                         }).ToList();

            //Initialize list for store sidebar content value id and name 
            var list = new List<KeyValuePair<string, string>>();

            //Iterate loop and add 1 by 1 item from enum sidebar list into key and value pair list to display in view
            for (int i = 0; i < lists.Count; i++)
            {
                list.Add(new KeyValuePair<string, string>(lists[i].State, lists[i].StateCode));
            }

            //Return list
            return list;
        }

        public void SaveCarrierDetails(CarrierVM carrierVM)
        {
            //Initialize transaction
            var transactionCarrier = db.Database.BeginTransaction();
            try
            {
                Carrier carrier = new Carrier();
                carrier.USDOTNumber = carrierVM.USDOTNumber;
                carrier.CompanyName = carrierVM.CompanyName;
                carrier.ContactEmail1 = carrierVM.ContactEmail1;
                carrier.ContactEmail2 = carrierVM.ContactEmail2;
                carrier.ContactPerson1 = carrierVM.ContactPerson1;
                carrier.ContactPerson2 = carrierVM.ContactPerson2;
                carrier.ContactPhone1 = carrierVM.ContactPhone1;
                carrier.ContactPhone2 = carrierVM.ContactPhone2;
                carrier.Website = carrierVM.Website;
                carrier.CarrierActive = carrierVM.CarrierActive;
                carrier.MaxQuotesPerMonth = carrierVM.MaxQuotesPerMonth;

                if (!carrierVM.Id.HasValue)
                {
                    carrierVM.Id = db.Carriers.Insert(db, carrier).Id;

                    if (carrierVM.SelectedPickupStateCode != null)
                    {
                        foreach (var selectedState in carrierVM.SelectedPickupStateCode)
                        {
                            Carrier_State_From carrier_State_From = new Carrier_State_From();
                            carrier_State_From.CarrierId = carrierVM.Id.Value;
                            carrier_State_From.StateCode = selectedState;

                            db.Carrier_State_From.Insert(db, carrier_State_From);
                        }
                    }

                    if (carrierVM.SelectedDeliveryStateCode != null)
                    {
                        foreach (var selectedState in carrierVM.SelectedDeliveryStateCode)
                        {
                            Carrier_State_To carrier_State_To = new Carrier_State_To();
                            carrier_State_To.CarrierId = carrierVM.Id.Value;
                            carrier_State_To.StateCode = selectedState;

                            db.Carrier_State_To.Insert(db, carrier_State_To);
                        }
                    }

                    if (carrierVM.SelectedLoadTypeList != null)
                    {
                        foreach (var selectedLoadType in carrierVM.SelectedLoadTypeList)
                        {
                            Carrier_LoadType carrier_LoadType = new Carrier_LoadType();
                            carrier_LoadType.CarrierId = carrierVM.Id.Value;
                            carrier_LoadType.LoadTypeID = selectedLoadType;

                            db.Carrier_LoadType.Insert(db, carrier_LoadType);
                        }
                    }
                }
                else
                {
                    carrier.Id = carrierVM.Id.Value;
                    carrierVM.Id = db.Carriers.UpdatePartial(db, carrier, false, "Id").Id;


                    if (carrierVM.SelectedPickupStateCode != null)
                    {

                        var carrierSelectedPickupStateCode = (from carrier_StateFrom in db.Carrier_State_From
                                                              where carrier_StateFrom.CarrierId == carrierVM.Id
                                                              select carrier_StateFrom).ToList();
                        db.Carrier_State_From.RemoveRange(carrierSelectedPickupStateCode);
                        db.SaveChanges();
                        foreach (var selectedState in carrierVM.SelectedPickupStateCode)
                        {
                            Carrier_State_From carrier_State_From = new Carrier_State_From();
                            carrier_State_From.CarrierId = carrierVM.Id.Value;
                            carrier_State_From.StateCode = selectedState;

                            db.Carrier_State_From.Insert(db, carrier_State_From);
                        }
                    }

                    if (carrierVM.SelectedDeliveryStateCode != null)
                    {
                        var carrierselectedStateCode = (from carrier_StateTo in db.Carrier_State_To
                                                        where carrier_StateTo.CarrierId == carrierVM.Id
                                                        select carrier_StateTo).ToList();

                        db.Carrier_State_To.RemoveRange(carrierselectedStateCode);
                        db.SaveChanges();
                        foreach (var selectedState in carrierVM.SelectedDeliveryStateCode)
                        {
                            Carrier_State_To carrier_State_To = new Carrier_State_To();
                            carrier_State_To.CarrierId = carrierVM.Id.Value;
                            carrier_State_To.StateCode = selectedState;

                            db.Carrier_State_To.Insert(db, carrier_State_To);
                        }
                    }

                    if (carrierVM.SelectedLoadTypeList != null)
                    {

                        var carrierselectedLoadType = (from carrier_LoadType in db.Carrier_LoadType
                                                       where carrier_LoadType.CarrierId == carrierVM.Id
                                                       select carrier_LoadType).ToList();

                        db.Carrier_LoadType.RemoveRange(carrierselectedLoadType);
                        db.SaveChanges();
                        foreach (var selectedLoadType in carrierVM.SelectedLoadTypeList)
                        {
                            Carrier_LoadType carrier_LoadType = new Carrier_LoadType();
                            carrier_LoadType.CarrierId = carrierVM.Id.Value;
                            carrier_LoadType.LoadTypeID = selectedLoadType;

                            db.Carrier_LoadType.Insert(db, carrier_LoadType);
                        }
                    }
                }
                //Commit transaction
                transactionCarrier.Commit();
            }
            catch (Exception ex)
            {
                //Rollback transaction
                transactionCarrier.Rollback();
                throw ex;
            }
        }


        /// <summary>
        /// Get Carrier Details at edit time by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CarrierVM GetCarrierDetailsById(int id)
        {
            //Get carrier details by it's id
            var carrierDetail = (from carrier in db.Carriers.AsNoTracking()
                                 where carrier.Id == id
                                 select new CarrierVM
                                 {
                                     Id = carrier.Id,
                                     USDOTNumber = carrier.USDOTNumber,
                                     CompanyName = carrier.CompanyName,
                                     CarrierActive = carrier.CarrierActive,
                                     ContactPhone1 = carrier.ContactPhone1,
                                     ContactPhone2 = carrier.ContactPhone2,
                                     ContactEmail1 = carrier.ContactEmail1,
                                     ContactEmail2 = carrier.ContactEmail2,
                                     ContactPerson1 = carrier.ContactPerson1,
                                     ContactPerson2 = carrier.ContactPerson2,
                                     Website = carrier.Website,
                                     MaxQuotesPerMonth = carrier.MaxQuotesPerMonth,
                                 }).FirstOrDefault();

            //Bind selected Load Type list
            carrierDetail.SelectedLoadTypeList = (from carrier_LoadType in db.Carrier_LoadType.AsNoTracking()
                                                  where carrier_LoadType.CarrierId == id
                                                  select carrier_LoadType.LoadTypeID).ToList();

            //Bind Selected StateCode for From(Pickup side)
            carrierDetail.SelectedPickupStateCode = (from carrier_State_From in db.Carrier_State_From.AsNoTracking()
                                                     where carrier_State_From.CarrierId == id
                                                     select carrier_State_From.StateCode).ToList();

            //Bind Selected StateCode for To(Delivery side)
            carrierDetail.SelectedDeliveryStateCode = (from carrier_State_To in db.Carrier_State_To.AsNoTracking()
                                                       where carrier_State_To.CarrierId == id
                                                       select carrier_State_To.StateCode).ToList();

            return carrierDetail;
        }


        public PagedList<CarrierVM> GetCarrierList(PageSortPara ps, ManageCarrierVM manageCarrierVM)
        {
            int pageSize = 15;
            ps.p = ((!ps.p.HasValue) || ps.p == 0) ? 1 : Convert.ToInt32(ps.p);
            ps.se = String.IsNullOrEmpty(ps.se) ? "USDOTNumber" : ps.se;
            ps.sd = String.IsNullOrEmpty(ps.sd) ? "Asc" : ps.sd;

            var carrierlist = (from carrier in db.Carriers.AsNoTracking()
                               select new CarrierVM
                               {
                                   Id = carrier.Id,
                                   CarrierActive = carrier.CarrierActive,
                                   USDOTNumber = carrier.USDOTNumber,
                                   Website = carrier.Website,
                                   ContactEmail1 = carrier.ContactEmail1,
                                   ContactPhone1 = carrier.ContactPhone1,
                                   CompanyName = carrier.CompanyName
                               }).AsQueryable();

            var sortOrder = ps.se + "_" + ps.sd;
            switch (sortOrder)
            {

                case "CompanyName_Asc":
                    carrierlist = carrierlist.OrderBy(s => s.CompanyName);
                    break;
                case "CompanyName_Desc":
                    carrierlist = carrierlist.OrderByDescending(s => s.CompanyName);
                    break;
                case "ContactPhone1_Asc":
                    carrierlist = carrierlist.OrderBy(s => s.ContactPhone1);
                    break;
                case "ContactPhone1_Desc":
                    carrierlist = carrierlist.OrderByDescending(s => s.ContactPhone1);
                    break;
                case "ContactEmail1_Asc":
                    carrierlist = carrierlist.OrderBy(s => s.ContactEmail1);
                    break;
                case "ContactEmail1_Desc":
                    carrierlist = carrierlist.OrderByDescending(s => s.ContactEmail1);
                    break;
                case "Website_Asc":
                    carrierlist = carrierlist.OrderBy(s => s.Website);
                    break;
                case "Website_Desc":
                    carrierlist = carrierlist.OrderByDescending(s => s.Website);
                    break;
                default:
                    carrierlist = carrierlist.OrderByDescending(s => s.USDOTNumber);
                    break;

            }

            //Search on Company Name 
            if (!string.IsNullOrEmpty(manageCarrierVM.CompanyName))
            {
                carrierlist = carrierlist.Where(t => t.CompanyName.Contains(manageCarrierVM.CompanyName));
            }

            //Search For Email Address
            if (!string.IsNullOrEmpty(manageCarrierVM.EmailAddess))
            {
                carrierlist = carrierlist.Where(t => t.ContactEmail1.Contains(manageCarrierVM.EmailAddess));
            }
            //Search for Phone Number
            if (!string.IsNullOrEmpty(manageCarrierVM.PhoneNumber))
            {
                carrierlist = carrierlist.Where(t => t.ContactPhone1.Contains(manageCarrierVM.PhoneNumber));
            }

            PagedList<CarrierVM> allcarrierlist = carrierlist.AsQueryable().SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
            return allcarrierlist;
        }

        /// <summary>
        /// Activate and InActivate the carrier by it's id
        /// </summary>
        /// <param name="carrierId"></param>
        public void ActiveOrInActiveCarrierById(int carrierId)
        {
            //Get carrier details by it's id
            var carrierDetail = (from carrierinfo in db.Carriers.AsNoTracking()
                                 where carrierinfo.Id == carrierId
                                 select carrierinfo).FirstOrDefault();
            //If active then make it deactive and if deactive then make it active
            carrierDetail.CarrierActive = !carrierDetail.CarrierActive;

            db.Carriers.UpdatePartial(db, carrierDetail, true, "CarrierActive");
        }

        #region Outbound Link Management

        /// <summary>
        /// Get all outbound links
        /// </summary>
        /// <returns></returns>
        public List<OutboundLinkVM> GetOutBoundLinks(bool sortByNumber)
        {
            var outboundLinksList = (from outboundLink in db.OutboundLinks
                                     select new OutboundLinkVM
                                     {
                                         Id = outboundLink.Id,
                                         URLTitle = outboundLink.URLTitle,
                                         URL = outboundLink.URL,
                                         Number = outboundLink.Number,
                                         IsFollow = outboundLink.IsFollow,
                                         Comment = outboundLink.Comment,
                                     }).ToList();
            if (sortByNumber)
            {
                return outboundLinksList.OrderBy(s => s.Number).ToList();
            }
            else
            {
                return outboundLinksList;
            }
        }

        /// <summary>
        /// Get outbound link detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OutboundLinkVM GetOutboundLinkDataById(int id)
        {
            var outboundLinksList = (from outboundLink in db.OutboundLinks
                                     where outboundLink.Id == id
                                     select new OutboundLinkVM
                                     {
                                         Id = outboundLink.Id,
                                         URLTitle = outboundLink.URLTitle,
                                         URL = outboundLink.URL,
                                         Number = outboundLink.Number,
                                         IsFollow = outboundLink.IsFollow,
                                         Comment = outboundLink.Comment,
                                     }).FirstOrDefault();
            return outboundLinksList;
        }

        /// <summary>
        /// Delete outboundLink by Id
        /// </summary>
        public void DeleteOutboundLinkById(int id)
        {
            //Delete Record from outbound link table
            OutboundLink outboundLink = new OutboundLink();
            outboundLink.Id = id;
            db.OutboundLinks.Delete(db, outboundLink);
        }


        /// <summary>
        /// Save outbound link
        /// </summary>
        public void SaveOutboundLink(OutboundLinkVM outboundLinkDetail)
        {
            OutboundLink outboundLink = new OutboundLink();
            if (outboundLinkDetail.Id.HasValue)
            {
                outboundLink.Id = outboundLinkDetail.Id.Value;
                outboundLink.URLTitle = outboundLinkDetail.URLTitle;
                outboundLink.URL = outboundLinkDetail.URL;
                outboundLink.Number = outboundLinkDetail.Number;
                outboundLink.IsFollow = outboundLinkDetail.IsFollow;
                outboundLink.Comment = outboundLinkDetail.Comment;
                db.OutboundLinks.Update(db, outboundLink);
            }
            else
            {
                outboundLink.URLTitle = outboundLinkDetail.URLTitle;
                outboundLink.URL = outboundLinkDetail.URL;
                outboundLink.Number = outboundLinkDetail.Number;
                outboundLink.IsFollow = outboundLinkDetail.IsFollow;
                outboundLink.Comment = outboundLinkDetail.Comment;
                db.OutboundLinks.Insert(db, outboundLink);
            }
        }

        /// <summary>
        /// get total of outbound links
        /// </summary>
        public int GetTotalOutboundLink()
        {
            return db.OutboundLinks.Count();
        }

        #endregion

        #region Outbound Banner Management

        /// <summary>
        /// Get the list of all outbound banners from the database.
        /// </summary>
        /// <returns>List of outbound banners</returns>
        public List<OutboundBannerDataModel> GetOutBoundBanners()
        {

            // Query the database to retrieve Outbound Banners
            List<OutboundBannerDataModel> outboundBannerList = (from OutboundBanner in db.OutboundBanners
                                                                select new OutboundBannerDataModel
                                                                {
                                                                    Id = OutboundBanner.Id,
                                                                    PageLevel = (OutboundBannerPageLevelEnum)OutboundBanner.PageLevel,
                                                                    IsShow = OutboundBanner.IsShow,
                                                                    OriginalFileName = OutboundBanner.OriginalFileName,
                                                                    FileName = OutboundBanner.FileName,
                                                                    URL = OutboundBanner.URL,
                                                                    IsFollow = OutboundBanner.IsFollow,
                                                                    AltText = OutboundBanner.AltText,
                                                                    TitleText = OutboundBanner.TitleText
                                                                }).ToList();

            // Return the list of outbound banners
            return outboundBannerList;
        }

        /// <summary>
        /// Save outbound banners to the database. This method updates existing banners
        /// and handles the upload of new images, including deletion of previous images.
        /// </summary>
        /// <param name="outboundBanners">List of banners to be saved.</param>
        public void SaveOutboundBanner(List<OutboundBannerDataModel> outboundBanners)
        {
            try
            {
                var outboundBannerPath = Config.SitePath + Config.GetValue("OutboundBannerFilePath");
                if (!Directory.Exists(outboundBannerPath))
                {
                    Directory.CreateDirectory(outboundBannerPath);
                }
                foreach (var outboundBannerModel in outboundBanners)
                {
                    // Find the existing banner in the database based on its ID
                    var existingOutboundBanner = db.OutboundBanners.Find(outboundBannerModel.Id);

                    if (existingOutboundBanner != null)
                    {
                        // Update properties of the existing banner with values from the provided model
                        existingOutboundBanner.PageLevel = (byte)outboundBannerModel.PageLevel;
                        existingOutboundBanner.IsShow = outboundBannerModel.IsShow;

                        if (outboundBannerModel.IsShow)
                        {
                            existingOutboundBanner.URL = outboundBannerModel.URL;
                            existingOutboundBanner.IsFollow = outboundBannerModel.IsFollow;
                            existingOutboundBanner.AltText = outboundBannerModel.AltText;
                            existingOutboundBanner.TitleText = outboundBannerModel.TitleText;
                        }

                        // When ImageFile is available
                        if (outboundBannerModel.ImageFile != null && outboundBannerModel.ImageFile.ContentLength > 0)
                        {
                            var fileExtension = Path.GetExtension(outboundBannerModel.ImageFile.FileName).ToLower();

                            if (fileExtension != ".jpeg" && fileExtension != ".jpg" && fileExtension != ".png")
                            {
                                string errorMessage = "Please select a valid image file (jpg, jpeg, png)";
                                throw new Exception(errorMessage);
                            }

                            // Delete the previous file if it exists
                            if (!string.IsNullOrEmpty(existingOutboundBanner.FileName))
                            {
                                var previousFilePath = Path.Combine(Config.SitePath + Config.GetValue("OutboundBannerFilePath"), existingOutboundBanner.FileName);

                                // Check if the previous file exists before attempting deletion
                                if (File.Exists(previousFilePath))
                                {
                                    File.Delete(previousFilePath);
                                }
                            }

                            // Save the new image and update FileName and OriginalFileName
                            var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(outboundBannerModel.ImageFile.FileName);
                            var filePath = Path.Combine(Config.SitePath + Config.GetValue("OutboundBannerFilePath"), newFileName);

                            // Save the new image to the server
                            outboundBannerModel.ImageFile.SaveAs(filePath);

                            // Update banner properties with new image information
                            existingOutboundBanner.FileName = newFileName;
                            existingOutboundBanner.OriginalFileName = outboundBannerModel.ImageFile.FileName;
                        }
                        else if (!string.IsNullOrEmpty(outboundBannerModel.FileName))
                        {
                            // When ImageFile is not available, but FileName is provided
                            // Update FileName and OriginalFileName (For Update, No updation for image)
                            existingOutboundBanner.FileName = outboundBannerModel.FileName;
                            existingOutboundBanner.OriginalFileName = outboundBannerModel.OriginalFileName;
                        }

                        // Mark the record as modified
                        db.Entry(existingOutboundBanner).State = EntityState.Modified;
                    }
                }

                // Save changes to the database outside the loop after all modifications
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region City Articles

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CityArticlesAvailabilityVM GetCityArticleAvailabilityDetails()
        {
            CityArticlesAvailabilityVM cityArticlesAvailability = new CityArticlesAvailabilityVM();
            cityArticlesAvailability.IsCityArticlesAllowed = db.Database.SqlQuery<bool>("select CityArticlesAllowed from Admin").FirstOrDefault();
            cityArticlesAvailability.NumberOfWords = db.Database.SqlQuery<int>("select NumberOfWords from Admin").FirstOrDefault();
            return cityArticlesAvailability;
        }

        /// <summary>
        /// Save city articles availability details.
        /// </summary>
        public void SaveCityArticlesAvailability(CityArticlesAvailabilityVM availabilityVM)
        {
            //Update Record in Admin for City Articles Availability
            db.Database.ExecuteSqlCommand("UPDATE Admin SET  CityArticlesAllowed = '" + availabilityVM.IsCityArticlesAllowed + "', NumberOfWords = '" + availabilityVM.NumberOfWords + "'");
        }

        public List<SelectListVM> GetCountries()
        {
            var contryList = (from cities in db.Cities.AsNoTracking()
                              select new SelectListVM
                              {
                                  Text = (cities.CountryCode == null ? string.Empty : cities.CountryCode == "US" ? "USA" : "Canada"),
                                  Value = cities.CountryCode
                              }).Distinct().ToList();
            return contryList;
        }

        public List<SelectListVM> GetStates(string countryCode)
        {
            var contryList = (from cities in db.Cities.AsNoTracking()
                              join state in db.States on cities.StateCode equals state.StateCode
                              where cities.CountryCode == countryCode
                              select new SelectListVM
                              {
                                  Value = cities.StateCode,
                                  Text = state.State1
                              }).Distinct().ToList();
            return contryList;
        }

        public List<SelectListVM> GetCities(string countryCode, string stateCode)
        {
            var contryList = (from cities in db.Cities.AsNoTracking()
                              where cities.CountryCode == countryCode && cities.StateCode == stateCode
                              select new SelectListVM
                              {
                                  Text = cities.CityName,
                                  Value = cities.CityName
                              }).Distinct().OrderBy(a => a.Text).ToList();
            return contryList;
        }

        /// <summary>
        /// Retrieves the count of cities that match the specified criteria for creating an article.
        /// </summary>
        /// <param name="selecteCitiesCriteria">An object containing the criteria for selecting cities.</param>
        /// <returns>The count of cities that meet the specified criteria.</returns>
        public Int64 GetSelectedCitiesForCreateArticle(SelectedCitiesForCreateArticleModel selecteCitiesCriteria)
        {
            var count = GetSelectedCitiesFromSelectionCriteria(selecteCitiesCriteria).Count;

            // Return the count of the selected cities
            return count; // Return the total number of cities that match the criteria
        }

        /// <summary>
        /// Retrieves a list of selected cities based on the provided selection criteria.
        /// </summary>
        /// <param name="selecteCitiesCriteria">The criteria used to filter the cities.</param>
        /// <returns>A list of cities that match the selection criteria.</returns>
        private List<City> GetSelectedCitiesFromSelectionCriteria(SelectedCitiesForCreateArticleModel selecteCitiesCriteria)
        {
            // Query the Cities table from the database without tracking changes
            var seletedCities = (from cities in db.Cities.AsNoTracking()
                                     // Apply filtering criteria based on the provided selection criteria
                                 where ((selecteCitiesCriteria.CountryCode != null && cities.CountryCode == selecteCitiesCriteria.CountryCode) || selecteCitiesCriteria.CountryCode == null) &&
                                       ((selecteCitiesCriteria.StateCode != null && cities.StateCode == selecteCitiesCriteria.StateCode) || selecteCitiesCriteria.StateCode == null) &&
                                       ((selecteCitiesCriteria.CityName != null && cities.CityName == selecteCitiesCriteria.CityName) || selecteCitiesCriteria.CityName == null) &&
                                       ((cities.NumberOfCompanies >= selecteCitiesCriteria.FromNumberCompany && cities.NumberOfCompanies <= selecteCitiesCriteria.ToNumberCompany) || (selecteCitiesCriteria.FromNumberCompany == null && selecteCitiesCriteria.ToNumberCompany == null)) &&
                                       ((selecteCitiesCriteria.OnlyAffectNullArticleCities == true && cities.Article == null) || (selecteCitiesCriteria.OnlyAffectNullArticleCities == false))
                                 // Select the CityName of the filtered cities
                                 select cities).ToList();
            // Return the list of selected cities
            return seletedCities;
        }

        /// <summary>
        /// Creates and saves an article for selected cities based on the provided prompt text.
        /// </summary>
        /// <param name="selecteCitiesCriteria">The criteria for selecting cities to create articles for.</param>
        /// <param name="promptText">The text prompt used to generate the article.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a success message or an error message if the prompt text is invalid.</returns>
        public async Task<string> CreateAndSaveArticleToCityAsync(SelectedCitiesForCreateArticleModel selecteCitiesCriteria, string promptText)
        {
            // Validate the prompt text to ensure it is not null or empty
            if (string.IsNullOrEmpty(promptText))
            {
                return "danger:Prompt text is required.";
            }


            // Retrieve the selected cities from the provided selection criteria
            var selectedCities = GetSelectedCitiesFromSelectionCriteria(selecteCitiesCriteria);

            //Get Auth details for Open AI
            OpenAIAuthDetails aIAuthDetails = new OpenAIAuthDetails()
            {
                APIKey = Config.GetValue("OpenAIAPIKey"),
                SystemContent = Config.GetValue("OpenAISystemContent"),
                Model = Config.GetValue("OpenAIModel"),
            };

            // Iterate through each selected city to update its article
            foreach (var city in selectedCities)
            {
                // Asynchronously get the ChatGPT response based on the provided prompt text
                var cityArticleResponse = await GetChatGptResponseAsync(promptText, city.CityName, city.StateCode, aIAuthDetails);

                // If city article is not get from OpenAI then not save article and show error message
                if (!cityArticleResponse.Status)
                {
                    return "danger:" + cityArticleResponse.ResponseText;
                }

                // Assign the generated article response to the city's Article property
                city.Article = cityArticleResponse.ResponseText;

                // Mark the city as partially updated in the database
                db.Cities.MarkAsPartialUpdated(db, city, true, "Article");

                // Save the changes to the database asynchronously
                await db.SaveChangesAsync();

                // Mark the partial update as completed for the city
                db.Cities.MarkPartialUpdateCompleted(db, city);

                // Note: If there are additional async operations to be performed, they should be awaited here
            }

            // Return a success message upon completion of the operation
            return "Success";
        }

        /// <summary>
        /// Asynchronously retrieves a response from the OpenAI API based on a user-provided prompt.
        /// The method replaces placeholders in the prompt with the actual city name and state name,
        /// prepares the request, and handles the API response.
        /// </summary>
        /// <param name="promptText">The prompt text containing placeholders for city and state.</param>
        /// <param name="cityName">The name of the city to replace the "XXXXX" placeholder.</param>
        /// <param name="stateCode">The state code used to fetch the corresponding state name.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a string
        /// indicating the success or failure of the API call, along with the generated response or an error message.</returns>
        private async Task<OpenAIResponse> GetChatGptResponseAsync(string promptText, string cityName, string stateCode, OpenAIAuthDetails aIAuthDetails)
        {
            OpenAIResponse aIResponse = new OpenAIResponse();
            try
            {
                // Replace placeholder "XXXXX" in the promptText with the actual cityName
                if (promptText.Contains("XXXXX"))
                {
                    promptText = promptText.Replace("XXXXX", cityName);
                }

                // Replace placeholder "YYYYY" in the promptText with the actual state name
                if (promptText.Contains("YYYYY"))
                {
                    // Fetch the state name from the database using the provided stateCode
                    string stateName = (await db.States.FirstOrDefaultAsync(s => s.StateCode == stateCode)).State1;
                    promptText = promptText.Replace("YYYYY", stateName);
                }

                // Prepare the request body for the OpenAI API
                var requestBody = new
                {
                    model = aIAuthDetails.Model, // Specify the model to use (can be changed to gpt-4 or others)
                    messages = new List<object>
            {
                new { role = "system", content = aIAuthDetails.SystemContent }, // System message to set context
                new { role = "user", content = promptText } // User message containing the prompt
            },
                    temperature = 0.7 // Set the randomness of the output (0.0 = deterministic, 1.0 = more random)
                };

                // Serialize the request body to JSON format
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Set up the HTTP headers for the request
                _httpClient.DefaultRequestHeaders.Clear();
                // TODO: replace API key with the actual OpenAI API key
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {aIAuthDetails.APIKey} ");

                // Send the POST request to the OpenAI API
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                // Check if the response was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the response into a ChatGptResponse object
                    var gptResponse = JsonConvert.DeserializeObject<ChatGptResponse>(responseBody);

                    aIResponse.Status = true;
                    aIResponse.ResponseText = gptResponse?.Choices[0]?.Message?.Content;
                    // Return the successful response content prefixed with "success:"
                    return aIResponse;
                }
                else
                {
                    aIResponse.Status = false;
                    aIResponse.ResponseText = "Could not get response from OpenAI.";
                    // Return an error message if the response was not successful
                    return aIResponse;
                }
            }
            catch (Exception ex)
            {
                aIResponse.Status = false;
                aIResponse.ResponseText = ex.Message;
                // Return an error message if an exception occurs
                return aIResponse;
            }
        }

        /// <summary>
        /// Allows articles for cities in a specified country and state by updating the CityArticleAllowed property.
        /// </summary>
        /// <param name="countryCode">The country code to filter cities.</param>
        /// <param name="stateCode">The state code to filter cities.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a string indicating success or an error message.</returns>
        public async Task<string> AllowArticleForSelectedState(string countryCode, string stateCode)
        {
            try
            {
                // Retrieve a list of cities that match the specified country and state codes,
                // and have a non-null Article property.
                var recordToUpdate = await db.Cities
                    .Where(city => city.CountryCode == countryCode && city.StateCode == stateCode && city.Article != null)
                    .ToListAsync();

                // Iterate through the retrieved cities and set the CityArticleAllowed property to true.
                foreach (var city in recordToUpdate)
                {
                    city.CityArticleAllowed = true;
                }

                // Save the changes to the database asynchronously.
                await db.SaveChangesAsync();
                if (recordToUpdate.Count > 0)
                {
                    // Return a success message.
                    return "Success : " + recordToUpdate.Count + " cities are updated.";

                }
                else
                {
                    // Return a error message.
                    return "Danger : No cities have been updated.";

                }

            }
            catch (Exception ex)
            {
                // In case of an exception, return the exception message.
                return "Danger : " + ex.Message;
            }
        }

        /// <summary>
        /// Retrieves the article for a specified city based on the country code, state code, and city name.
        /// </summary>
        /// <param name="countryCode">The country code to filter the city.</param>
        /// <param name="stateCode">The state code to filter the city.</param>
        /// <param name="cityName">The name of the city to retrieve the article for.</param>
        /// <returns>The article associated with the specified city, or null if not found.</returns>
        public string GetArticleForSelectedCity(string countryCode, string stateCode, string cityName)
        {
            // Use a LINQ query to find the article for the specified city.
            var city = GetSelectedCity(countryCode, stateCode, cityName);

            // Return the found article, or null if no article exists for the specified city.
            if (city != null)
            {
                return city.Article;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a city object based on the specified country code, state code, and city name.
        /// </summary>
        /// <param name="countryCode">The country code to filter the city.</param>
        /// <param name="stateCode">The state code to filter the city.</param>
        /// <param name="cityName">The name of the city to retrieve.</param>
        /// <returns>The city object that matches the specified criteria, or null if not found.</returns>
        private City GetSelectedCity(string countryCode, string stateCode, string cityName)
        {
            // Use a LINQ query to find the city that matches the specified country, state, and city name.
            var selectedCity = (from cities in db.Cities
                                where cities.CountryCode == countryCode
                                      && cities.StateCode == stateCode
                                      && cities.CityName == cityName
                                select cities).FirstOrDefault();

            // Return the found city object, or null if no matching city exists.
            return selectedCity;
        }

        /// <summary>
        /// Allows articles for a specified city by updating the CityArticleAllowed property.
        /// </summary>
        /// <param name="countryCode">The country code to identify the city.</param>
        /// <param name="stateCode">The state code to identify the city.</param>
        /// <param name="cityName">The name of the city to allow the article for.</param>
        public string AllowArticleForSelectedCity(string countryCode, string stateCode, string cityName)
        {
            try
            {
                // Retrieve the selected city based on the provided country, state, and city name.
                var selectedCity = GetSelectedCity(countryCode, stateCode, cityName);

                // Check if the city was found.
                if (selectedCity != null)
                {
                    // Set the CityArticleAllowed property to true to allow articles for this city.
                    selectedCity.CityArticleAllowed = true;

                    // Update the city in the database, specifically the CityArticleAllowed property.
                    db.Cities.UpdatePartial(db, selectedCity, true, "CityArticleAllowed");
                }
                return "Success : City artcle has been enabled for " + selectedCity.CityName;
            }
            catch (Exception ex)
            {
                return "Danger : " + ex.Message;
            }
        }

        /// <summary>
        /// Sets the article for a specified city by updating the Article property.
        /// </summary>
        /// <param name="countryCode">The country code to identify the city.</param>
        /// <param name="stateCode">The state code to identify the city.</param>
        /// <param name="cityName">The name of the city to set the article for.</param>
        /// <param name="article">The article content to be set for the city.</param>
        public string SetArticleForSelectedCity(string countryCode, string stateCode, string cityName, string article)
        {
            try
            {
                // Retrieve the selected city based on the provided country, state, and city name.
                var selectedCity = GetSelectedCity(countryCode, stateCode, cityName);

                // Check if the city was found.
                if (selectedCity != null)
                {
                    // Set the Article property to the provided article content.
                    selectedCity.Article = article;

                    // Update the city in the database, specifically the Article property.
                    db.Cities.UpdatePartial(db, selectedCity, true, "Article");
                }

                if (!string.IsNullOrEmpty(article))
                {
                    return "Success : The city article has been successfully updated.";
                }
                else
                {
                    return "Success : The city article has been successfully removed.";

                }

            }
            catch (Exception ex)
            {
                return "Danger : " + ex.Message;
            }

        }

        /// <summary>
        /// Retrieves a list of cities for managing city articles based on the specified country code, state code, and article allowance status.
        /// </summary>
        /// <param name="countryCode">The country code to filter the cities.</param>
        /// <param name="stateCode">The state code to filter the cities.</param>
        /// <param name="isAllowed">Optional parameter to filter cities based on whether articles are allowed.</param>
        /// <returns>A list of SelectListVM objects representing the cities.</returns>
        public List<SelectListVM> GetCitiesForManageCityArticle(string countryCode, string stateCode, bool? isAllowed)
        {
            // Retrieve the list of cities based on the provided country and state codes, using AsNoTracking for better performance.
            var cityList = (from cities in db.Cities.AsNoTracking()
                            where cities.CountryCode == countryCode && cities.StateCode == stateCode && cities.Article != null
                            select cities).Distinct();

            // If isAllowed has a value, filter the city list based on the CityArticleAllowed property.
            if (isAllowed.HasValue)
            {
                cityList = cityList.Where(city => city.CityArticleAllowed == isAllowed.Value);
            }

            // Project the filtered city list into a list of SelectListVM objects.
            var returnList = cityList.Select(city => new SelectListVM()
            {
                Text = city.CityName,  // The display text for the select list.
                Value = city.CityName  // The value for the select list.
            }).OrderBy(a => a.Text).ToList();

            // Return the list of SelectListVM objects.
            return returnList;
        }

        /// <summary>
        /// Checks if a city article is available for the specified city.
        /// </summary>
        /// <param name="countryCode">The country code to identify the city.</param>
        /// <param name="stateCode">The state code to identify the city.</param>
        /// <param name="cityName">The name of the city to check for an article.</param>
        /// <returns>True if the city article is available, false otherwise.</returns>
        public bool IsCityArticleAvailable(string countryCode, string stateCode, string cityName)
        {
            // Retrieve the selected city based on the provided country, state, and city name.
            var selectedCity = GetSelectedCity(countryCode, stateCode, cityName);

            // Return true if the city exists and its Article property is not null or empty.
            return selectedCity != null && !string.IsNullOrEmpty(selectedCity.Article);
        }

        /// <summary>
        /// Retrieves a list of cities for managing cities, excluding cities that already exist in the Cities table.
        /// </summary>
        /// <returns>A list of ManageCityListVM objects representing the cities.</returns>
        public List<ManageCityListVM> GetCityListForManageCities()
        {
            // Perform a left join between TransportCompanies and States tables based on the state code.
            var result = (from tc in db.TransportCompanies
                          join s in db.States on tc.PhysicalAddressStateCode equals s.StateCode into stateGroup
                          from s in stateGroup.DefaultIfEmpty()
                              // Group the results by country code, state code, city, and state name.
                          group tc by new
                          {
                              s.CountryCode,
                              tc.PhysicalAddressStateCode,
                              tc.PhysicalAddressCity,
                              s.State1
                          } into g
                          // Filter out groups where a city with the same country code, state code, and city name already exists in the Cities table.
                          where !db.Cities.Any(c =>
                              (c.CountryCode) == (g.Key.CountryCode) &&
                              c.StateCode == g.Key.PhysicalAddressStateCode &&
                              c.CityName == g.Key.PhysicalAddressCity)
                          // Project the filtered groups into ManageCityListVM objects.
                          select new ManageCityListVM
                          {
                              // Determine the country based on the country code.
                              Country = g.Key.CountryCode == "US" ? "US" : "Canada",
                              // Set the state name.
                              State = g.Key.State1,
                              // Set the city name.
                              City = g.Key.PhysicalAddressCity,
                              // Generate the city URL by combining the state code and city name.
                              CityURL = g.Key.PhysicalAddressStateCode + "/" + g.Key.PhysicalAddressCity.Replace(" ", "-"),
                              // Set the number of companies in the city.
                              NoCompanies = g.Count()
                          }).OrderBy(c => c.Country).ThenBy(s => s.State).ThenBy(c => c.City).ToList();
            return result;
        }

        /// <summary>
        /// Finishes the update of city information based on transport companies.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a message indicating success or failure.</returns>
        public async Task<string> FinishCityUpdate()
        {
            try
            {
                // Check for invalid state codes among active companies only.
                var invalidStateCodes = await db.TransportCompanies
                    .Where(c => c.Status == "A" && !db.States.Any(s => s.StateCode == c.PhysicalAddressStateCode))
                    .ToListAsync();

                // If there are invalid state codes, return an error message.
                if (invalidStateCodes.Any())
                {
                    return "danger : There are StateCodes in Company that do not exist in State.";
                }

                // Count active companies by physical address city only.
                // Grouping on TransportCompanies alone (no States join) avoids row
                // multiplication if States ever has duplicate StateCode entries.
                var cityCounts = await db.TransportCompanies
                    .Where(tc => tc.Status == "A")
                    .GroupBy(tc => new { tc.PhysicalAddressStateCode, tc.PhysicalAddressCity })
                    .Select(g => new
                    {
                        StateCode = g.Key.PhysicalAddressStateCode,
                        CityName = g.Key.PhysicalAddressCity,
                        NumberOfCompanies = g.Count()
                    })
                    .ToListAsync();

                // Resolve CountryCode per StateCode in memory, deduplicating States rows.
                var countryByState = (await db.States.ToListAsync())
                    .GroupBy(s => s.StateCode)
                    .ToDictionary(g => g.Key, g => g.First().CountryCode, StringComparer.OrdinalIgnoreCase);

                var allCities = cityCounts.Select(c => new
                {
                    CountryCode = countryByState.ContainsKey(c.StateCode) ? countryByState[c.StateCode] : (string)null,
                    c.StateCode,
                    c.CityName,
                    c.NumberOfCompanies
                }).ToList();

                // Load all existing Cities rows once; use a dictionary for O(1) lookup.
                var existingCities = await db.Cities.ToListAsync();
                var existingByKey = existingCities.ToDictionary(
                    c => c.StateCode + "|" + c.CityName,
                    StringComparer.OrdinalIgnoreCase);

                // Set of city keys that currently have active companies.
                var activeCityKeys = new HashSet<string>(
                    allCities.Select(c => c.StateCode + "|" + c.CityName),
                    StringComparer.OrdinalIgnoreCase);

                // Update existing rows and insert new ones.
                foreach (var city in allCities)
                {
                    var key = city.StateCode + "|" + city.CityName;
                    if (existingByKey.TryGetValue(key, out var existingCity))
                    {
                        // Update the count if it has changed; leave Article untouched.
                        if (existingCity.NumberOfCompanies != city.NumberOfCompanies)
                            existingCity.NumberOfCompanies = city.NumberOfCompanies;
                    }
                    else
                    {
                        // New city/spelling — create a row with no article.
                        db.Cities.Add(new City()
                        {
                            CountryCode = city.CountryCode,
                            StateCode = city.StateCode,
                            CityName = city.CityName,
                            NumberOfCompanies = city.NumberOfCompanies,
                            Article = null,
                            CityArticleAllowed = false
                        });
                    }
                }

                // Remove orphan rows: cities that exist in the table but have no
                // active companies today (physical address only).
                foreach (var orphan in existingCities.Where(c => !activeCityKeys.Contains(c.StateCode + "|" + c.CityName)))
                {
                    db.Cities.Remove(orphan);
                }

                // Save all changes to the database.
                await db.SaveChangesAsync();

                return "success : Cities are updated successfully.";
            }
            catch (Exception ex)
            {
                // Return an error message if an exception occurs.
                return "danger : " + ex.Message;
            }
        }
        #endregion

        #region Company Reviews

        //Get Global Hiring
        public ManageReviewsVM GetReviewsFilter()
        {
            //Get Global Hiring from Admin table
            ManageReviewsVM manageReviewsVM = new ManageReviewsVM();
            int filterValue = db.Database.SqlQuery<int>("select ReviewsFilter from Admin").FirstOrDefault();
            manageReviewsVM.SelectedFilterValue = filterValue;

            return manageReviewsVM;
        }

        /// <summary>   
        /// Saves the selected reviews filter preference into Admin table.
        /// </summary>
        /// <param name="globalHiringVM"></param>
        public void SaveReviewsFilter(ManageReviewsVM manageReviewsVM)
        {
            db.Database.ExecuteSqlCommand("UPDATE Admin SET ReviewsFilter = '" + manageReviewsVM.SelectedFilterValue + "'");
        }

        /// <summary>
        /// Retrieves the global reviews filter value for company reviews.
        /// </summary>
        /// <returns></returns>
        public ManageReviewsVM GetCompanyReviewsFilterValue()
        {
            //Get company reviews filter value from Admin table
            ManageReviewsVM reviewFilter = new ManageReviewsVM();
            int globalHiring = db.Database.SqlQuery<int>("select ReviewsFilter from Admin").FirstOrDefault();
            reviewFilter.SelectedFilterValue = globalHiring;

            return reviewFilter;
        }

        #endregion
    }
}
