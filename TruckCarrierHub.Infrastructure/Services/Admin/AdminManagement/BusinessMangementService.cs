

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
        private static CityRenewProgressVM _cityRenewProgress = new CityRenewProgressVM { Message = "Idle" };
        private readonly HttpClient _httpClient;


        public BusinessMangementService(PartnerCarrier_DevEntities _db)
        {
            db = _db;
            db.Database.CommandTimeout = 300;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(240) };
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
                catch (Exception)
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
        public List<ManageCityListVM> GetAllCities() => GetAllCities(0, 0);

        public List<ManageCityListVM> GetAllCities(int minCompanies, int maxCompanies, DateTime? lastRenewedBefore = null)
        {
            bool hasFilter = minCompanies > 0 || maxCompanies > 0 || lastRenewedBefore.HasValue;

            IQueryable<City> query = db.Cities;
            if (minCompanies > 0) query = query.Where(c => c.NumberOfCompanies >= minCompanies);
            if (maxCompanies > 0) query = query.Where(c => c.NumberOfCompanies <= maxCompanies);
            if (lastRenewedBefore.HasValue)
            {
                var dateThreshold = lastRenewedBefore.Value;
                query = query.Where(c => c.LastRenewedDate == null || c.LastRenewedDate < dateThreshold);
            }
            var cities = query.ToList();
            var stateByCode = db.States.ToList()
                .GroupBy(s => s.StateCode)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Live active counts only when a filter is applied — the result set is bounded so
            // the GROUP BY is cheap. The no-filter initial page load (all cities) skips this
            // to avoid holding the ASP.NET session lock for 30-60 s and blocking AJAX requests.
            var liveCountDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (hasFilter && cities.Count > 0)
            {
                var stateCodes = cities.Select(c => c.StateCode).Distinct().ToList();
                var liveCounts = db.TransportCompanies
                    .Where(tc => tc.Status == "A" && stateCodes.Contains(tc.PhysicalAddressStateCode))
                    .GroupBy(tc => new { tc.PhysicalAddressStateCode, tc.PhysicalAddressCity })
                    .Select(g => new { StateCode = g.Key.PhysicalAddressStateCode, CityName = g.Key.PhysicalAddressCity, Count = g.Count() })
                    .ToList();
                foreach (var x in liveCounts)
                {
                    var key = (x.StateCode ?? "") + "|" + (x.CityName ?? "");
                    if (liveCountDict.ContainsKey(key))
                        liveCountDict[key] += x.Count;
                    else
                        liveCountDict[key] = x.Count;
                }
            }

            return cities.Select(c =>
            {
                var state = stateByCode.ContainsKey(c.StateCode) ? stateByCode[c.StateCode] : null;
                int noCompanies;
                if (hasFilter)
                {
                    var liveKey = (c.StateCode ?? "") + "|" + (c.CityName ?? "");
                    liveCountDict.TryGetValue(liveKey, out noCompanies);
                }
                else
                {
                    noCompanies = c.NumberOfCompanies ?? 0;
                }
                return new ManageCityListVM
                {
                    Country = c.CountryCode == "US" ? "US" : "Canada",
                    State = state != null ? state.State1 : c.StateCode,
                    StateCode = c.StateCode,
                    City = c.CityName,
                    CityURL = c.StateCode + "/" + c.CityName.Replace(" ", "-"),
                    NoCompanies = noCompanies,
                    Description = c.Description,
                    Article = c.Article,
                    LastRenewedDate = c.LastRenewedDate
                };
            })
            .OrderBy(c => c.Country)
            .ThenBy(c => c.State)
            .ThenBy(c => c.City)
            .ToList();
        }

        public string SaveCityContent(SaveCityContentVM vm)
        {
            var city = db.Cities.FirstOrDefault(c => c.StateCode == vm.StateCode && c.CityName == vm.CityName);
            if (city == null)
                return "error:City not found";
            city.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
            city.Article = string.IsNullOrWhiteSpace(vm.Article) ? null : vm.Article.Trim();
            city.LastRenewedDate = DateTime.Now;
            db.SaveChanges();
            return "success:Saved";
        }

        public CityRenewProgressVM GetCityRenewProgress()
        {
            return _cityRenewProgress;
        }

        public void StartCityRenew(StartCityRenewVM request)
        {
            _cityRenewProgress.IsInProgress = true;
            _cityRenewProgress.Processed = 0;
            _cityRenewProgress.Total = 0;
            _cityRenewProgress.Message = "Starting...";
            try
            {
                var aiAuth = new OpenAIAuthDetails
                {
                    APIKey = Config.GetValue("OpenAIAPIKey"),
                    SystemContent = Config.GetValue("OpenAISystemContent"),
                    Model = Config.GetValue("OpenAIModel")
                };

                using (var dbCtx = new PartnerCarrier_DevEntities(true, null, true, null, null))
                {
                    dbCtx.Database.CommandTimeout = 360;

                    var urlSet = new HashSet<string>(
                        request.CityURLs ?? new List<string>(),
                        StringComparer.OrdinalIgnoreCase);

                    var cities = dbCtx.Cities.ToList()
                        .Where(c => urlSet.Contains(c.StateCode + "/" + c.CityName.Replace(" ", "-")))
                        .Where(c => !request.LastRenewedBefore.HasValue
                                    || c.LastRenewedDate == null
                                    || c.LastRenewedDate < request.LastRenewedBefore)
                        .ToList();
                    _cityRenewProgress.Total = cities.Count;

                    var statesAll = dbCtx.States.ToList()
                        .GroupBy(s => s.StateCode)
                        .Select(g => g.First())
                        .ToList();
                    var stateByCode = statesAll.ToDictionary(s => s.StateCode, s => s.State1, StringComparer.OrdinalIgnoreCase);
                    var countryByStateCode = statesAll.ToDictionary(s => s.StateCode, s => s.CountryCode, StringComparer.OrdinalIgnoreCase);

                    // Fleet benchmarks — computed once, reused across all cities in the batch
                    // National medians are split by country so US cities compare against US carriers
                    // and Canadian cities compare against Canadian carriers.
                    double? usNationalMedian = null;
                    double? caNationalMedian = null;
                    var stateFleetAvgs = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        var benchmarkRows = dbCtx.TransportCompanies
                            .Where(c => c.Status == "A" && c.TrucksAndTractors != null && c.TrucksAndTractors > 0 && c.TrucksAndTractors <= 50000)
                            .Select(c => new { c.PhysicalAddressStateCode, TT = c.TrucksAndTractors })
                            .ToList();
                        var validRows = benchmarkRows
                            .Where(x => x.TT.HasValue)
                            .Select(x => new {
                                StateCode = x.PhysicalAddressStateCode ?? "",
                                Fleet = (double)x.TT.Value,
                                CountryCode = countryByStateCode.ContainsKey(x.PhysicalAddressStateCode ?? "")
                                    ? countryByStateCode[x.PhysicalAddressStateCode ?? ""] : null
                            })
                            .ToList();
                        if (validRows.Count > 0)
                        {
                            var usRows = validRows.Where(x => x.CountryCode == "US").Select(x => x.Fleet).OrderBy(x => x).ToList();
                            if (usRows.Count > 0) usNationalMedian = usRows[usRows.Count / 2];
                            var caRows = validRows.Where(x => x.CountryCode != "US" && x.CountryCode != null).Select(x => x.Fleet).OrderBy(x => x).ToList();
                            if (caRows.Count > 0) caNationalMedian = caRows[caRows.Count / 2];
                            foreach (var sg in validRows.GroupBy(x => x.StateCode, StringComparer.OrdinalIgnoreCase))
                            {
                                var ss = sg.Select(x => x.Fleet).OrderBy(x => x).ToList();
                                stateFleetAvgs[sg.Key] = ss[ss.Count / 2];
                            }
                        }
                    }
                    catch { /* benchmarks are optional; continue without */ }

                    foreach (var city in cities)
                    {
                        _cityRenewProgress.Message = "Generating: " + city.CityName + ", " + city.StateCode;

                        var companies = dbCtx.TransportCompanies
                            .Where(c => c.Status == "A"
                                && c.PhysicalAddressStateCode == city.StateCode
                                && c.PhysicalAddressCity == city.CityName)
                            .ToList();

                        if (companies.Count > 0)
                        {
                            var stateName = stateByCode.ContainsKey(city.StateCode)
                                ? stateByCode[city.StateCode]
                                : city.StateCode;

                            double? cityStateAvg = null;
                            if (city.StateCode != null && stateFleetAvgs.ContainsKey(city.StateCode))
                                cityStateAvg = stateFleetAvgs[city.StateCode];

                            string regionLabel = string.Equals(city.CountryCode, "US", StringComparison.OrdinalIgnoreCase)
                                ? "state" : "province";
                            double? cityNationalMedian = regionLabel == "state" ? usNationalMedian : caNationalMedian;
                            var displayCityName = ToTitleCaseCityName(city.CityName);
                            var cityCountPhrase = CompanyCountPhrase(companies.Count);
                            var prompt = BuildCityContentPrompt(displayCityName, stateName, companies, cityStateAvg, cityNationalMedian, regionLabel);
                            var aiResult = CallOpenAISync(prompt, aiAuth);

                            if (aiResult.Status && !string.IsNullOrEmpty(aiResult.ResponseText))
                            {
                                var cleaned = StripCodeFences(aiResult.ResponseText);
                                string resolvedDesc = null;
                                string resolvedArticle = null;
                                try
                                {
                                    var parsed = JsonConvert.DeserializeObject<CityContentResponse>(cleaned);
                                    var desc = parsed?.Description?.Trim();
                                    resolvedArticle = string.IsNullOrWhiteSpace(parsed?.Article) ? null : parsed.Article.Trim();
                                    var descReason = ValidateDescription(desc);
                                    if (descReason != null)
                                    {
                                        var retryPrompt = "Your previous description was rejected because " + descReason + ". "
                                            + "Previous attempt: \"" + (desc ?? "") + "\". "
                                            + "Rewrite as a single complete sentence ending with a period, strictly within 175 characters. "
                                            + "The sentence MUST open with '" + displayCityName + ", " + stateName + " has " + cityCountPhrase + " active trucking companies' — use '" + cityCountPhrase + "' verbatim, never 'about', 'around', 'few', or any other wording. "
                                            + "Use 'has', never 'hosts'. Use 'median fleet size', never 'average fleet size'. "
                                            + "Include the full state name '" + stateName + "', never abbreviate. "
                                            + "Shorten by trimming the cargo list or dropping the fleet clause — never by abbreviating cargo labels or the size-tier phrase. "
                                            + "Return only: {\"description\": \"...rewritten text...\"}";
                                        var retryResult = CallOpenAISync(retryPrompt, aiAuth);
                                        if (retryResult.Status && !string.IsNullOrEmpty(retryResult.ResponseText))
                                        {
                                            var retryParsed = JsonConvert.DeserializeObject<CityContentResponse>(StripCodeFences(retryResult.ResponseText));
                                            var retryDesc = retryParsed?.Description?.Trim();
                                            if (IsValidDescription(retryDesc))
                                                resolvedDesc = retryDesc;
                                            else
                                                resolvedDesc = IsAcceptableFallbackDescription(desc) ? desc
                                                             : IsAcceptableFallbackDescription(retryDesc) ? retryDesc
                                                             : null;
                                        }
                                        else
                                        {
                                            resolvedDesc = IsAcceptableFallbackDescription(desc) ? desc : null;
                                        }
                                    }
                                    else
                                    {
                                        resolvedDesc = desc;
                                    }
                                }
                                catch
                                {
                                    // JSON parse failed; resolvedDesc and resolvedArticle remain null
                                }
                                if (resolvedDesc != null)
                                {
                                    city.Description = resolvedDesc;
                                    city.Article = resolvedArticle;
                                    city.LastRenewedDate = DateTime.Now;
                                    dbCtx.SaveChanges();
                                }
                            }
                            else if (!aiResult.Status)
                            {
                                _cityRenewProgress.Message = "AI error (" + city.CityName + "): "
                                    + (aiResult.ResponseText ?? "").Substring(0,
                                        Math.Min(120, (aiResult.ResponseText ?? "").Length));
                            }
                        }

                        _cityRenewProgress.Processed++;
                        _cityRenewProgress.Message = "Processed " + _cityRenewProgress.Processed
                            + " of " + _cityRenewProgress.Total;
                    }

                    _cityRenewProgress.Message = "Complete — " + _cityRenewProgress.Processed
                        + " of " + _cityRenewProgress.Total + " cities updated";
                }
            }
            catch (Exception ex)
            {
                _cityRenewProgress.Message = "Error: " + ex.Message;
            }
            finally
            {
                _cityRenewProgress.IsInProgress = false;
            }
        }

        private static string ToTitleCaseCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName)) return cityName;
            var ti = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
            var words = ti.ToTitleCase(cityName.ToLowerInvariant()).Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                var w = words[i];
                // Fix "Mc" prefix: "Mccalla" → "McCalla"
                if (w.Length > 2 && w[0] == 'M' && w[1] == 'c' && char.IsLower(w[2]))
                    words[i] = "Mc" + char.ToUpper(w[2]) + w.Substring(3);
            }
            return string.Join(" ", words);
        }

        private static string CompanyCountPhrase(int count)
        {
            if (count <= 20)    return "a small number of";
            if (count <= 50)    return "dozens of";
            if (count <= 200)   return NearlyOrOver(count, 25);
            if (count <= 500)   return "hundreds of";
            if (count <= 2000)  return NearlyOrOver(count, 50);
            if (count <= 10000) return NearlyOrOver(count, 500);
            return NearlyOrOver(count, 1000);
        }

        private static string NearlyOrOver(int count, int step)
        {
            int rounded = (int)Math.Round((double)count / step) * step;
            if (rounded == 0) rounded = step;
            if (count < rounded && (double)count / rounded >= 0.97)
                return "nearly " + rounded.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
            int floor = (count / step) * step;
            if (floor == 0) floor = step;
            return "over " + floor.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        }

        private string BuildCityContentPrompt(
            string cityName, string stateName, List<TransportCompany> companies,
            double? stateAvgFleet, double? nationalAvgFleet, string regionLabel = "state")
        {
            int count = companies.Count;
            string countPhrase = CompanyCountPhrase(count);

            // Entity type breakdown
            var entityTypeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "C", "Carrier" }, { "S", "Shipper Only" }, { "B", "Broker" },
                { "R", "Registrant" }, { "F", "Freight Forwarder" },
                { "I", "Intermodal Equipment Provider" }, { "T", "Cargo Tank" }
            };
            var allCodes = companies
                .Where(c => !string.IsNullOrEmpty(c.EntityType))
                .SelectMany(c => c.EntityType.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();
            int companiesWithMultiple = companies
                .Count(c => !string.IsNullOrEmpty(c.EntityType) && c.EntityType.Contains(';'));
            var entityTypePluralNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "C", "Carriers" }, { "S", "Shippers Only" }, { "B", "Brokers" },
                { "R", "Registrants" }, { "F", "Freight Forwarders" },
                { "I", "Intermodal Equipment Providers" }, { "T", "Cargo Tanks" }
            };
            // Canonical display order: Carrier, Broker, Shipper Only, Freight Forwarder, IEP, then others alphabetically.
            var entityTypeOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "C", 0 }, { "B", 1 }, { "S", 2 }, { "F", 3 }, { "I", 4 }
            };
            var entityGroups = allCodes
                .GroupBy(code => code, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g =>
                {
                    string singularName, pluralName;
                    if (!entityTypeNames.TryGetValue(g.Key, out singularName)) singularName = g.Key;
                    if (!entityTypePluralNames.TryGetValue(g.Key, out pluralName)) pluralName = singularName + "s";
                    double pctExact = count > 0 ? 100.0 * g.Count() / count : 0.0;
                    int pctInt = (int)Math.Round(pctExact);
                    if (pctInt == 99 && pctExact < 99.5) pctInt = 98;
                    string pctStr = pctInt == 0 && g.Count() > 0 ? pctExact.ToString("F1") : pctInt.ToString();
                    int sortOrder;
                    if (!entityTypeOrder.TryGetValue(g.Key, out sortOrder)) sortOrder = 100;
                    return new { SingularName = singularName, PluralName = pluralName, PctStr = pctStr, SortOrder = sortOrder };
                })
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.SingularName)
                .Select(e => new { e.SingularName, e.PluralName, e.PctStr })
                .ToList();
            // When any type is 100%, additional types are subsets of companies holding multiple
            // registrations simultaneously — keep only the 100% entry so the sentence doesn't
            // imply separate non-overlapping groups (e.g. "Carriers 100%, Shippers Only 8%").
            if (entityGroups.Any(e => e.PctStr == "100"))
                entityGroups = entityGroups.Where(e => e.PctStr == "100").ToList();
            var entityLines = entityGroups.Select(e => e.SingularName + " " + e.PctStr + "%").ToList();
            string regSentence;
            if (entityGroups.Count == 0)
                regSentence = "";
            else if (entityGroups.Count == 1)
                regSentence = entityGroups[0].PluralName + " represent " + entityGroups[0].PctStr + "% of registered companies.";
            else
            {
                var regParts = entityGroups
                    .Select((e, i) => i == 0
                        ? e.PluralName + " represent " + e.PctStr + "% of registered companies"
                        : e.PluralName + " " + e.PctStr + "%")
                    .ToList();
                regSentence = string.Join(", ", regParts.Take(regParts.Count - 1)) + ", and " + regParts.Last() + ".";
            }

            // Cargo types
            var cargoCounts = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("general freight",             companies.Count(c => c.CargoTransportedAGeneralFreight != null)),
                new KeyValuePair<string, int>("household goods",             companies.Count(c => c.CargoTransportedBHouseholdGoods != null)),
                new KeyValuePair<string, int>("motor vehicles",              companies.Count(c => c.CargoTransportedDMotorVehicles != null)),
                new KeyValuePair<string, int>("building materials",          companies.Count(c => c.CargoTransportedGBuildingMaterials != null)),
                new KeyValuePair<string, int>("machinery and heavy objects", companies.Count(c => c.CargoTransportedIMachineryLargeObjects != null)),
                new KeyValuePair<string, int>("fresh produce",               companies.Count(c => c.CargoTransportedJFreshProduce != null)),
                new KeyValuePair<string, int>("liquids and gases",           companies.Count(c => c.CargoTransportedKLiquidsGases != null)),
                new KeyValuePair<string, int>("intermodal containers",       companies.Count(c => c.CargoTransportedLIintermodalContainers != null)),
                new KeyValuePair<string, int>("oilfield equipment",          companies.Count(c => c.CargoTransportedNOilfieldEquipment != null)),
                new KeyValuePair<string, int>("grain, feed, and hay",        companies.Count(c => c.CargoTransportedPGrainFeedHay != null)),
                new KeyValuePair<string, int>("chemicals",                   companies.Count(c => c.CargoTransportedUChemicals != null)),
                new KeyValuePair<string, int>("dry bulk",                    companies.Count(c => c.CargoTransportedVCommoditiesDryBulk != null)),
                new KeyValuePair<string, int>("refrigerated food",           companies.Count(c => c.CargoTransportedWRefrigeratedFood != null)),
            };
            int companiesWithAnyCargo = companies.Count(c =>
                c.CargoTransportedAGeneralFreight != null ||
                c.CargoTransportedBHouseholdGoods != null ||
                c.CargoTransportedDMotorVehicles != null ||
                c.CargoTransportedGBuildingMaterials != null ||
                c.CargoTransportedIMachineryLargeObjects != null ||
                c.CargoTransportedJFreshProduce != null ||
                c.CargoTransportedKLiquidsGases != null ||
                c.CargoTransportedLIintermodalContainers != null ||
                c.CargoTransportedNOilfieldEquipment != null ||
                c.CargoTransportedPGrainFeedHay != null ||
                c.CargoTransportedUChemicals != null ||
                c.CargoTransportedVCommoditiesDryBulk != null ||
                c.CargoTransportedWRefrigeratedFood != null);

            double coveragePctExact = count > 0 ? 100.0 * companiesWithAnyCargo / count : 0.0;
            int coveragePct = (int)Math.Round(coveragePctExact);
            if (coveragePct == 99 && coveragePctExact < 99.5) coveragePct = 98;

            var topCargo = cargoCounts
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv =>
                {
                    double pctOfClassifiedExact = companiesWithAnyCargo > 0
                        ? 100.0 * kv.Value / companiesWithAnyCargo : 0.0;
                    int pctOfClassified = (int)Math.Round(pctOfClassifiedExact);
                    if (pctOfClassified == 99 && pctOfClassifiedExact < 99.5) pctOfClassified = 98;
                    return new { Label = kv.Key, Pct = pctOfClassified };
                })
                .ToList();

            // Description cargo selection:
            // If GF is the top type: GF first, then top 2 non-GF by company count.
            // If GF is not the top type: top 3 types by company count regardless of whether GF is among them.
            var sortedAllCargo = cargoCounts
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .ToList();
            var descriptionCargo = new List<string>();
            if (sortedAllCargo.Count > 0)
            {
                if (sortedAllCargo[0].Key == "general freight")
                {
                    descriptionCargo.Add("general freight");
                    var nonGf = sortedAllCargo.Where(kv => kv.Key != "general freight").ToList();
                    // Top 2 non-GF types, extended to include any additional tied at position 2
                    var distinctiveSelected = nonGf.Count <= 2
                        ? nonGf
                        : nonGf.Take(2)
                                .Concat(nonGf.Skip(2).TakeWhile(kv => kv.Value == nonGf[1].Value))
                                .ToList();
                    distinctiveSelected.Select(kv => kv.Key).ToList().ForEach(t => descriptionCargo.Add(t));
                }
                else
                {
                    // Top 3 types, extended to include any additional tied at position 3
                    var topSelected = sortedAllCargo.Count <= 3
                        ? sortedAllCargo
                        : sortedAllCargo.Take(3)
                                        .Concat(sortedAllCargo.Skip(3).TakeWhile(kv => kv.Value == sortedAllCargo[2].Value))
                                        .ToList();
                    topSelected.Select(kv => kv.Key).ToList().ForEach(t => descriptionCargo.Add(t));
                }
            }

            // Fleet computation
            var fleetSizes = companies
                .Where(c => c.TrucksAndTractors.HasValue && c.TrucksAndTractors.Value > 0 && c.TrucksAndTractors.Value <= 50000)
                .Select(c => c.TrucksAndTractors.Value)
                .OrderBy(x => x)
                .ToList();
            int ownerOps   = fleetSizes.Count(x => x <= 3);
            int midSized   = fleetSizes.Count(x => x >= 4 && x <= 20);
            int largeSized = fleetSizes.Count(x => x > 20);
            int? medianFleet = null;
            int fleetCoveragePct = 0;
            if (fleetSizes.Count > 0)
            {
                medianFleet = fleetSizes[fleetSizes.Count / 2];
                fleetCoveragePct = (int)Math.Round(100.0 * fleetSizes.Count / count);
            }

            bool outlierFleet = fleetSizes.Count > 0 && medianFleet.HasValue && medianFleet.Value > 0
                && fleetSizes.Max() > 20 * medianFleet.Value
                && fleetSizes.Max() > 50000;

            // Article length tier
            int paragraphs;
            string wordLimit;
            if      (count >= 2001) { paragraphs = 4; wordLimit = "800"; }
            else if (count >= 501)  { paragraphs = 4; wordLimit = "600"; }
            else if (count >= 201)  { paragraphs = 4; wordLimit = "500"; }
            else if (count >= 51)   { paragraphs = 3; wordLimit = "400"; }
            else if (count >= 11)   { paragraphs = 2; wordLimit = "300"; }
            else                    { paragraphs = 1; wordLimit = "150"; }

            string countryLabel = regionLabel == "state" ? "US" : "Canada";

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Write a JSON object with exactly two fields — \"description\" and \"article\" — for the trucking directory page for " + cityName + ", " + stateName + ".");
            sb.AppendLine("Return ONLY raw JSON. No markdown, no code fences, no preamble. Format: {\"description\":\"...\",\"article\":\"...\"}");
            sb.AppendLine();

            // INPUT DATA
            sb.AppendLine("INPUT DATA (use only this data — invent nothing):");
            sb.AppendLine();
            sb.AppendLine("City: " + cityName + ", " + stateName);
            sb.AppendLine("Country: " + countryLabel);
            sb.AppendLine("Size tier phrase: " + countPhrase + " — use this exact phrase verbatim in both description and article; never substitute 'about', 'around', 'approximately', 'few', 'numerous', or the raw number (" + count + ").");
            if (entityLines.Any())
                sb.AppendLine("Registration types: " + string.Join(", ", entityLines));
            sb.AppendLine("Multiple registrations: " + (companiesWithMultiple > 0 ? "yes" : "no"));
            sb.AppendLine("Cargo data coverage: " + coveragePct + "% of companies have cargo classifications on file with FMCSA");
            if (topCargo.Any())
                sb.AppendLine("Top cargo types: " + string.Join(", ", topCargo.Select(c => c.Label + ": " + c.Pct + "%")));
            sb.AppendLine("Fleet data coverage: " + (fleetSizes.Count > 0 ? fleetCoveragePct + "%" : "no data"));
            if (medianFleet.HasValue)
                sb.AppendLine("Median fleet size: " + medianFleet.Value + " " + (medianFleet.Value == 1 ? "vehicle" : "vehicles"));
            if (fleetSizes.Count > 0)
            {
                int ownerOpPct = (int)Math.Round(100.0 * ownerOps   / fleetSizes.Count);
                int midPct     = (int)Math.Round(100.0 * midSized   / fleetSizes.Count);
                int largePct   = (int)Math.Round(100.0 * largeSized / fleetSizes.Count);
                sb.AppendLine("Fleet scale (of reporting companies): owner-operator-scale (1–3 vehicles): " + ownerOpPct + "%, mid-size (4–20 vehicles): " + midPct + "%, large (21+): " + largePct + "%");
            }
            sb.AppendLine("Outlier fleet: " + (outlierFleet ? "yes" : "no"));
            if (stateAvgFleet.HasValue)
            {
                int stateMedVal = (int)Math.Round(stateAvgFleet.Value);
                sb.AppendLine(stateName + " median fleet size: " + stateMedVal + " " + (stateMedVal == 1 ? "vehicle" : "vehicles"));
            }
            if (nationalAvgFleet.HasValue)
            {
                int natMedVal = (int)Math.Round(nationalAvgFleet.Value);
                sb.AppendLine(countryLabel + " national median fleet size: " + natMedVal + " " + (natMedVal == 1 ? "vehicle" : "vehicles"));
            }
            sb.AppendLine("Article length tier: " + paragraphs + " paragraph" + (paragraphs == 1 ? "" : "s") + ", " + wordLimit + " words maximum");
            sb.AppendLine();

            // DESCRIPTION RULES
            // Shorter label forms used only in the description — article uses full labels unchanged
            var descriptionLabelMap = new Dictionary<string, string>
            {
                { "machinery and heavy objects", "machinery" },
                { "grain, feed, and hay",        "grain and feed" },
            };
            var descCargoLabels = descriptionCargo
                .Select(t => descriptionLabelMap.ContainsKey(t) ? descriptionLabelMap[t] : t)
                .ToList();
            // Build description sentence entirely in C# — every word hardcoded, nothing left to model
            string descFleetClause = medianFleet.HasValue
                ? ", with a median fleet size of " + medianFleet.Value + " " + (medianFleet.Value == 1 ? "vehicle" : "vehicles")
                : "";
            string descBase = cityName + ", " + stateName + " has " + countPhrase + " active trucking companies";
            string targetDescription;
            if (descCargoLabels.Count == 0)
            {
                targetDescription = descBase + descFleetClause + ".";
            }
            else
            {
                // Full cargo phrase
                string cargoAll = descCargoLabels.Count == 1 ? descCargoLabels[0]
                    : descCargoLabels.Count == 2 ? descCargoLabels[0] + " and " + descCargoLabels[1]
                    : string.Join(", ", descCargoLabels.Take(descCargoLabels.Count - 1)) + ", and " + descCargoLabels.Last();
                // Cargo phrase with general freight dropped
                var distinctiveLabels = descCargoLabels.Where(l => l != "general freight").ToList();
                string cargoNoGf = distinctiveLabels.Count == 0 ? ""
                    : distinctiveLabels.Count == 1 ? distinctiveLabels[0]
                    : distinctiveLabels.Count == 2 ? distinctiveLabels[0] + " and " + distinctiveLabels[1]
                    : string.Join(", ", distinctiveLabels.Take(distinctiveLabels.Count - 1)) + ", and " + distinctiveLabels.Last();
                // Single most-distinctive type
                string cargoOne = distinctiveLabels.Count > 0 ? distinctiveLabels[0] : descCargoLabels[0];
                // Try candidates in shortening order; pick first that fits 175 chars.
                // Rule: drop general freight only if keeping it would exceed 175 — never before.
                var candidates = new List<string>
                {
                    descBase + " specializing in " + cargoAll     + descFleetClause + ".",
                    descBase + " transporting "    + cargoAll     + descFleetClause + ".",
                };
                // Before dropping GF, try GF + first distinctive only (drops extra distinctives first)
                if (descCargoLabels.Contains("general freight") && distinctiveLabels.Count >= 2)
                {
                    string gfPlusOne = "general freight and " + distinctiveLabels[0];
                    if (gfPlusOne != cargoAll)
                    {
                        candidates.Add(descBase + " specializing in " + gfPlusOne  + descFleetClause + ".");
                        candidates.Add(descBase + " transporting "    + gfPlusOne  + descFleetClause + ".");
                    }
                }
                if (!string.IsNullOrEmpty(cargoNoGf) && cargoNoGf != cargoAll)
                {
                    candidates.Add(descBase + " specializing in " + cargoNoGf  + descFleetClause + ".");
                    candidates.Add(descBase + " transporting "    + cargoNoGf  + descFleetClause + ".");
                }
                if (cargoOne != cargoAll)
                {
                    candidates.Add(descBase + " specializing in " + cargoOne   + descFleetClause + ".");
                    candidates.Add(descBase + " transporting "    + cargoOne   + descFleetClause + ".");
                }
                candidates.Add(descBase + descFleetClause + ".");
                targetDescription = candidates.FirstOrDefault(c => c.Length <= 175) ?? candidates.Last();
            }

            sb.AppendLine("DESCRIPTION RULES:");
            sb.AppendLine("Write this exact sentence verbatim: \"" + targetDescription + "\"");
            sb.AppendLine();

            // ARTICLE RULES
            sb.AppendLine("ARTICLE RULES:");
            sb.AppendLine("Write " + paragraphs + " paragraph" + (paragraphs == 1 ? "" : "s") + ", " + wordLimit + " words maximum. Use only the data above. Separate each paragraph with a blank line (two newlines).");
            sb.AppendLine("Opening sentence: \"" + cityName + ", " + stateName + " is home to " + countPhrase + " active trucking companies.\" — '" + countPhrase + "' verbatim, never paraphrased. This is the first sentence of paragraph 1 — do not insert a blank line after it.");

            // Registration paragraph — embed exact sentence built in C#, model copies verbatim
            string regParaText = !string.IsNullOrEmpty(regSentence)
                ? "Write this exact sentence verbatim: \"" + regSentence + "\""
                : "State the percentage for each registration type.";
            if (companiesWithMultiple > 0)
                regParaText += " Add one sentence: \"A small number of companies hold multiple simultaneous registrations, indicating a degree of operational flexibility.\"";
            regParaText += " Then stop. Never explain what entity types mean. Never say carriers transport goods. Never say brokers coordinate shipments. Never comment on what the percentages imply.";

            // Cargo paragraph — each type gets its own semicolon-separated entry, no grouping by percentage
            string flowingCargoSentence = "";
            if (topCargo.Any())
            {
                var flowParts = topCargo
                    .Select((c, i) => i == 0
                        ? c.Pct + "% of classified companies transport " + c.Label
                        : c.Pct + "% transport " + c.Label)
                    .ToList();
                flowingCargoSentence = flowParts.Count == 1
                    ? flowParts[0] + "."
                    : string.Join("; ", flowParts.Take(flowParts.Count - 1)) + "; and " + flowParts.Last() + ".";
            }
            var cargoParaSb = new StringBuilder();
            cargoParaSb.Append("Write this exact sentence verbatim: \"" + coveragePct + "% of companies have cargo classifications on file with FMCSA.\"");
            if (!string.IsNullOrEmpty(flowingCargoSentence))
                cargoParaSb.Append(" Then write this exact sentence verbatim: \"" + flowingCargoSentence + "\" Then stop.");
            cargoParaSb.Append(" Do not summarize, interpret, or characterize the cargo mix.");
            string cargoParaText = cargoParaSb.ToString();

            // Fleet scale paragraph — embed exact sentence templates with live data
            int ownerOpPctF = 0, midPctF = 0, largePctF = 0;
            string fleetScaleText;
            if (fleetSizes.Count > 0 && medianFleet.HasValue)
            {
                ownerOpPctF = (int)Math.Round(100.0 * ownerOps   / fleetSizes.Count);
                midPctF     = (int)Math.Round(100.0 * midSized   / fleetSizes.Count);
                largePctF   = (int)Math.Round(100.0 * largeSized / fleetSizes.Count);
                var fsb = new StringBuilder();
                fsb.AppendLine("Write these three sentences in order — never combine into one:");
                fsb.AppendLine("\"" + fleetCoveragePct + "% of companies have reported fleet data.\"");
                fsb.AppendLine("\"Among these, the median fleet size is " + medianFleet.Value + " " + (medianFleet.Value == 1 ? "vehicle" : "vehicles") + ".\"");
                string scaleBreakdownSentence = largePctF == 0
                    ? "In terms of scale, " + ownerOpPctF + "% operate on an owner-operator-scale with 1–3 vehicles and " + midPctF + "% are mid-size with 4–20 vehicles, with no large-fleet operations reported."
                    : "In terms of scale, " + ownerOpPctF + "% operate on an owner-operator-scale with 1–3 vehicles, " + midPctF + "% are mid-size with 4–20 vehicles, and " + largePctF + "% are large with 21 or more vehicles.";
                fsb.Append("\"" + scaleBreakdownSentence + "\"");
                if (outlierFleet)
                    fsb.Append(" Then add: \"One company reports a significantly larger fleet, standing out clearly in a market dominated by small operators.\"");
                fsb.Append(" Then stop. No interpretation.");
                fleetScaleText = fsb.ToString();
            }
            else
            {
                fleetScaleText = "No fleet data available — omit fleet scale statements.";
            }

            // Fleet comparison paragraph — opening sentence built in C# with fixed format
            string fleetCompareText;
            if (medianFleet.HasValue && (stateAvgFleet.HasValue || nationalAvgFleet.HasValue))
            {
                int cityMed = medianFleet.Value;
                var fcb = new StringBuilder();
                // Build the fixed-format opening verbatim: "The median fleet size in [city] is [comparison] the [state] median of [N] vehicle(s)..."
                fcb.Append("Write this exact opening verbatim: \"The median fleet size in " + cityName + " is ");
                if (stateAvgFleet.HasValue && nationalAvgFleet.HasValue)
                {
                    int stateMed = (int)Math.Round(stateAvgFleet.Value);
                    int natMed  = (int)Math.Round(nationalAvgFleet.Value);
                    string stateComp = cityMed > stateMed ? "above" : cityMed < stateMed ? "below" : "equal to";
                    string natComp   = cityMed > natMed   ? "above" : cityMed < natMed   ? "below" : "equal to";
                    if (stateComp == natComp)
                        fcb.Append(stateComp + " the " + stateName + " median of " + stateMed + " " + (stateMed == 1 ? "vehicle" : "vehicles") + " and the " + countryLabel + " national median of " + natMed + " " + (natMed == 1 ? "vehicle" : "vehicles") + ".");
                    else
                        fcb.Append(stateComp + " the " + stateName + " median of " + stateMed + " " + (stateMed == 1 ? "vehicle" : "vehicles") + " and " + natComp + " the " + countryLabel + " national median of " + natMed + " " + (natMed == 1 ? "vehicle" : "vehicles") + ".");
                }
                else if (stateAvgFleet.HasValue)
                {
                    int stateMed = (int)Math.Round(stateAvgFleet.Value);
                    string stateComp = cityMed > stateMed ? "above" : cityMed < stateMed ? "below" : "equal to";
                    fcb.Append(stateComp + " the " + stateName + " median of " + stateMed + " " + (stateMed == 1 ? "vehicle" : "vehicles") + ".");
                }
                else
                {
                    int natMed = (int)Math.Round(nationalAvgFleet.Value);
                    string natComp = cityMed > natMed ? "above" : cityMed < natMed ? "below" : "equal to";
                    fcb.Append(natComp + " the " + countryLabel + " national median of " + natMed + " " + (natMed == 1 ? "vehicle" : "vehicles") + ".");
                }
                fcb.Append("\" Use 'median' never 'average' or 'mean'. Use exact numbers.");
                if (count >= 2000 && fleetSizes.Count > 0)
                {
                    string largeFleetClause = largePctF == 0
                        ? "and no large-fleet operations"
                        : "vs. " + largePctF + "% at large-fleet scale";
                    fcb.Append(" Also state: \"" + ownerOpPctF + "% of reporting companies operate at owner-operator-scale " + largeFleetClause + ".\"");
                };
                fcb.Append(" Then stop.");
                fleetCompareText = fcb.ToString();
            }
            else
            {
                fleetCompareText = "No comparison data available — omit fleet comparison.";
            }

            // Assemble paragraph instructions
            if (paragraphs == 1)
            {
                sb.AppendLine("1 paragraph covering registration types, cargo, and fleet:");
                sb.AppendLine("Registration: " + regParaText);
                sb.AppendLine("Cargo: " + cargoParaText);
                if (fleetSizes.Count > 0)
                    sb.AppendLine("Fleet: " + fleetScaleText);
            }
            else if (paragraphs == 2)
            {
                sb.AppendLine("Paragraph 1 — Registration types + cargo:");
                sb.AppendLine("Registration: " + regParaText);
                sb.AppendLine("Cargo: " + cargoParaText);
                sb.AppendLine("Paragraph 2 — Fleet scale + fleet comparison:");
                if (fleetSizes.Count > 0)
                    sb.AppendLine("Fleet scale: " + fleetScaleText);
                sb.AppendLine("Fleet comparison: " + fleetCompareText);
            }
            else if (paragraphs == 3)
            {
                sb.AppendLine("Paragraph 1 — Registration types:");
                sb.AppendLine(regParaText);
                sb.AppendLine("Paragraph 2 — Cargo:");
                sb.AppendLine(cargoParaText);
                sb.AppendLine("Paragraph 3 — Fleet scale + fleet comparison:");
                sb.AppendLine(fleetScaleText);
                sb.AppendLine(fleetCompareText);
            }
            else
            {
                sb.AppendLine("Paragraph 1 — Registration types:");
                sb.AppendLine(regParaText);
                sb.AppendLine("Paragraph 2 — Cargo:");
                sb.AppendLine(cargoParaText);
                sb.AppendLine("Paragraph 3 — Fleet scale and distribution:");
                sb.AppendLine(fleetScaleText);
                sb.AppendLine("Paragraph 4 — Fleet comparison to " + regionLabel + " and national medians:");
                sb.AppendLine(fleetCompareText);
            }
            sb.AppendLine();
            sb.AppendLine("End every paragraph on a data point — never on an editorial summary or characterization.");
            sb.AppendLine();

            // ABSOLUTE BANS
            sb.AppendLine("ABSOLUTE BANS — never use any of these in any output:");
            sb.AppendLine("Words/phrases: vibrant, thriving, bustling, dynamic, hub, gateway, crucial, vital, significant, nestled, testament to, plays a key role, staggering, remarkable, interestingly, surprisingly, impressively, boasts, robust, agile, notably, clearly, heavily reliant on, concentrated presence, demonstrates, underscores, highlights, reveals, paints a picture.");
            sb.AppendLine("Entity-type explanations (any variation): 'directly involved in the transportation of goods', 'rather than coordinating or arranging for transportation', 'companies that own and operate their own trucks', 'direct transportation services', 'coordinating shipments', 'forwarding freight', 'physically transport', 'as opposed to arranging', 'market focused on direct transportation.'");
            sb.AppendLine("Cargo: Never use 'vehicles' alone — always 'motor vehicles.' Never abbreviate or paraphrase cargo labels — 'general freight' not 'general cargo', 'oilfield equipment' not 'oilfield gear', 'building materials' not 'building supplies', 'intermodal containers' not 'intermodal loads' or 'intermodal units', 'machinery and heavy objects' not 'heavy machinery' or 'heavy equipment.'");
            sb.AppendLine("Size/count language: 'about X', 'around X', 'approximately X', 'few', 'numerous', 'several' as count approximations — always use the size tier phrase '" + countPhrase + "'.");
            sb.AppendLine("Editorializing: 'could imply', 'might suggest', 'given the right circumstances', 'operators can flourish', 'highly competitive', 'competing for business', 'room for growth', 'favorable conditions', 'it can be inferred that', 'this suggests that' as a sentence opener, 'this indicates that' as a sentence opener, 'this demonstrates that' as a sentence opener, 'agile operators', 'small nimble operations', 'corporate operations.'");
            sb.AppendLine("Comparisons: Any comparison not grounded in the exact " + regionLabel + " and national medians provided — never 'more fragmented than other markets', 'unusually small', 'compared to other cities', 'than might be found elsewhere.'");
            sb.AppendLine("Endings: Never end any paragraph on an editorial summary or characterization.");

            return sb.ToString();
        }

        private OpenAIResponse CallOpenAISync(string prompt, OpenAIAuthDetails aiAuth)
        {
            var result = new OpenAIResponse();
            try
            {
                var body = new
                {
                    model = aiAuth.Model,
                    messages = new List<object>
                    {
                        new { role = "system", content = aiAuth.SystemContent ?? "" },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.5
                };
                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add(
                    "Authorization", "Bearer " + (aiAuth.APIKey ?? "").Trim());

                var httpResponse = _httpClient
                    .PostAsync("https://api.openai.com/v1/chat/completions", requestContent)
                    .GetAwaiter().GetResult();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseBody = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var gptResponse = JsonConvert.DeserializeObject<ChatGptResponse>(responseBody);
                    result.Status = true;
                    result.ResponseText = gptResponse?.Choices != null && gptResponse.Choices.Count > 0
                        ? gptResponse.Choices[0]?.Message?.Content
                        : null;
                }
                else
                {
                    var errorBody = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    result.Status = false;
                    result.ResponseText = "HTTP " + (int)httpResponse.StatusCode + ": " + errorBody;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ResponseText = ex.Message;
            }
            return result;
        }

        public CityContentPreviewVM PreviewCityContent(string stateCode, string cityName)
        {
            var result = new CityContentPreviewVM();
            try
            {
                var companies = db.TransportCompanies
                    .Where(c => c.Status == "A"
                        && c.PhysicalAddressStateCode == stateCode
                        && c.PhysicalAddressCity == cityName)
                    .ToList();
                result.CompanyCount = companies.Count;
                if (companies.Count == 0)
                {
                    result.Error = "No active companies found with PhysicalAddressCity = '" + cityName + "'";
                    return result;
                }
                var stateName = db.States.FirstOrDefault(s => s.StateCode == stateCode)?.State1 ?? stateCode;
                var cityEntity = db.Cities.FirstOrDefault(c => c.StateCode == stateCode && c.CityName == cityName);
                string regionLabel = string.Equals(cityEntity?.CountryCode, "US", StringComparison.OrdinalIgnoreCase)
                    ? "state" : "province";

                // Fleet benchmarks for comparison (computed per preview; acceptable cost)
                // National median is scoped to the same country as the city being previewed.
                double? nationalAvgFleet = null;
                double? stateAvgFleet = null;
                try
                {
                    bool isCityUS = string.Equals(cityEntity?.CountryCode, "US", StringComparison.OrdinalIgnoreCase);
                    var countryByStateCode = db.States.ToList()
                        .GroupBy(s => s.StateCode)
                        .ToDictionary(g => g.Key, g => g.First().CountryCode, StringComparer.OrdinalIgnoreCase);
                    var benchmarkRows = db.TransportCompanies
                        .Where(c => c.Status == "A" && c.TrucksAndTractors != null && c.TrucksAndTractors > 0 && c.TrucksAndTractors <= 50000)
                        .Select(c => new { c.PhysicalAddressStateCode, TT = c.TrucksAndTractors })
                        .ToList();
                    var validRows = benchmarkRows
                        .Where(x => x.TT.HasValue)
                        .Select(x => new {
                            StateCode = x.PhysicalAddressStateCode ?? "",
                            Fleet = (double)x.TT.Value,
                            CountryCode = countryByStateCode.ContainsKey(x.PhysicalAddressStateCode ?? "")
                                ? countryByStateCode[x.PhysicalAddressStateCode ?? ""] : null
                        })
                        .ToList();
                    if (validRows.Count > 0)
                    {
                        var nationalRows = isCityUS
                            ? validRows.Where(x => x.CountryCode == "US").Select(x => x.Fleet).OrderBy(x => x).ToList()
                            : validRows.Where(x => x.CountryCode != "US" && x.CountryCode != null).Select(x => x.Fleet).OrderBy(x => x).ToList();
                        if (nationalRows.Count > 0)
                            nationalAvgFleet = nationalRows[nationalRows.Count / 2];
                        var stateRows = validRows
                            .Where(x => string.Equals(x.StateCode, stateCode, StringComparison.OrdinalIgnoreCase))
                            .Select(x => x.Fleet).OrderBy(x => x).ToList();
                        if (stateRows.Count > 0)
                            stateAvgFleet = stateRows[stateRows.Count / 2];
                    }
                }
                catch { /* benchmarks optional */ }

                var aiAuth = new OpenAIAuthDetails
                {
                    APIKey = Config.GetValue("OpenAIAPIKey"),
                    SystemContent = Config.GetValue("OpenAISystemContent"),
                    Model = Config.GetValue("OpenAIModel")
                };
                var displayCityName = ToTitleCaseCityName(cityName);
                var cityCountPhrase = CompanyCountPhrase(companies.Count);
                var prompt = BuildCityContentPrompt(displayCityName, stateName, companies, stateAvgFleet, nationalAvgFleet, regionLabel);
                var aiResult = CallOpenAISync(prompt, aiAuth);
                if (!aiResult.Status)
                {
                    result.Error = aiResult.ResponseText;
                    return result;
                }
                var cleaned = StripCodeFences(aiResult.ResponseText ?? "");
                try
                {
                    var parsed = JsonConvert.DeserializeObject<CityContentResponse>(cleaned);
                    var desc = parsed?.Description?.Trim();
                    var descReason = ValidateDescription(desc);
                    if (descReason != null)
                    {
                        var retryPrompt = "Your previous description was rejected because " + descReason + ". "
                            + "Previous attempt: \"" + (desc ?? "") + "\". "
                            + "Rewrite as a single complete sentence ending with a period, strictly within 175 characters. "
                            + "The sentence MUST open with '" + displayCityName + ", " + stateName + " has " + cityCountPhrase + " active trucking companies' — use '" + cityCountPhrase + "' verbatim, never 'about', 'around', 'few', or any other wording. "
                            + "Use 'has', never 'hosts'. Use 'median fleet size', never 'average fleet size'. "
                            + "Include the full state name '" + stateName + "', never abbreviate. "
                            + "Shorten by trimming the cargo list or dropping the fleet clause — never by abbreviating cargo labels or the size-tier phrase. "
                            + "Return only: {\"description\": \"...rewritten text...\"}";
                        var retryResult = CallOpenAISync(retryPrompt, aiAuth);
                        if (retryResult.Status && !string.IsNullOrEmpty(retryResult.ResponseText))
                        {
                            var retryParsed = JsonConvert.DeserializeObject<CityContentResponse>(StripCodeFences(retryResult.ResponseText));
                            var retryDesc = retryParsed?.Description?.Trim();
                            if (IsValidDescription(retryDesc))
                                result.Description = retryDesc;
                            else
                                result.Description = IsAcceptableFallbackDescription(desc) ? desc
                                                   : IsAcceptableFallbackDescription(retryDesc) ? retryDesc
                                                   : null;
                        }
                        else
                        {
                            result.Description = IsAcceptableFallbackDescription(desc) ? desc : null;
                        }
                    }
                    else
                    {
                        result.Description = desc;
                    }
                    result.Article = parsed?.Article?.Trim();
                }
                catch
                {
                    result.Article = cleaned;
                }
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }

        private static string ValidateDescription(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "description is empty";
            if (s.Length > 175) return "it is " + s.Length + " characters (limit 175)";
            if (!s.EndsWith(".") && !s.EndsWith("?")) return "it does not end with a period";
            if (s.Contains(". ")) return "it contains multiple sentences — must be a single sentence";
            var lower = s.ToLowerInvariant();
            if (lower.Contains(" hosts ") || lower.StartsWith("hosts ")) return "it uses 'hosts' — must use 'has'";
            if (lower.Contains("average fleet") || lower.Contains("mean fleet")) return "it says 'average fleet' or 'mean fleet' — must say 'median fleet'";
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"\babout\s+[\d,]") ||
                System.Text.RegularExpressions.Regex.IsMatch(lower, @"\baround\s+[\d,]"))
                return "it uses 'about' or 'around' before a number — must use the size-tier phrase ('nearly', 'over', 'a small number of', etc.)";
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"\bfew\b"))
                return "it uses 'few' — must use 'a small number of'";
            if (lower.Contains("trucking firms") || lower.Contains("truck firms"))
                return "it uses 'firms' — must use 'companies'";
            return null;
        }
        private static bool IsValidDescription(string s) => ValidateDescription(s) == null;

        // Relaxed check used as a fallback when strict validation fails after retry.
        // Accepts descriptions up to 185 chars but enforces the hard quality rules.
        private static bool IsAcceptableFallbackDescription(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.Length > 185) return false;
            if (!s.EndsWith(".") && !s.EndsWith("?")) return false;
            var lower = s.ToLowerInvariant();
            if (lower.Contains(" hosts ") || lower.StartsWith("hosts ")) return false;
            if (lower.Contains("average fleet") || lower.Contains("mean fleet")) return false;
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"\babout\s+[\d,]") ||
                System.Text.RegularExpressions.Regex.IsMatch(lower, @"\baround\s+[\d,]")) return false;
            if (System.Text.RegularExpressions.Regex.IsMatch(lower, @"\bfew\b")) return false;
            return true;
        }

        private static string StripCodeFences(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var t = text.Trim();
            if (t.StartsWith("```"))
            {
                var nl = t.IndexOf('\n');
                if (nl >= 0) t = t.Substring(nl + 1).Trim();
                if (t.EndsWith("```")) t = t.Substring(0, t.Length - 3).Trim();
            }
            // Strip any preamble before the opening brace
            var brace = t.IndexOf('{');
            if (brace > 0) t = t.Substring(brace);
            return t.Trim();
        }

        private class CityContentResponse
        {
            public string Description { get; set; }
            public string Article { get; set; }
        }

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
                            Description = null,
                            LastRenewedDate = null
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

                // Cache the total active company count in Admin so the homepage
                // can read it without running COUNT(*) on the full TransportCompany table.
                int totalActiveCompanies = allCities.Sum(c => c.NumberOfCompanies);
                await db.Database.ExecuteSqlCommandAsync(
                    "UPDATE Admin SET NumberOfCompanies = {0}", totalActiveCompanies);

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

        #region Statistics Nav

        public ShowStatisticsNavVM GetShowStatisticsNav()
        {
            try
            {
                bool show = db.Database.SqlQuery<bool>("SELECT ShowStatisticsNav FROM Admin").FirstOrDefault();
                return new ShowStatisticsNavVM { ShowStatisticsNav = show };
            }
            catch
            {
                return new ShowStatisticsNavVM { ShowStatisticsNav = false };
            }
        }

        public void SaveShowStatisticsNav(ShowStatisticsNavVM vm)
        {
            db.Database.ExecuteSqlCommand("UPDATE Admin SET ShowStatisticsNav = " + (vm.ShowStatisticsNav ? "1" : "0"));
        }

        #endregion
    }
}
