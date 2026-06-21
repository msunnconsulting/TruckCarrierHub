namespace PartnerCarrier.Infrastructure.Services.User
{
    using Common.Utility;
    using Common.Utility.LinqConditionalOperators;
    using Common.Utility.Logger;
    using Common.Utility.ViewModels;
    using Infrastructure.Contracts.User;
    using Infrastructure.Database;
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    using PartnerCarrier.ViewModels.Admin;
    using PartnerCarrier.ViewModels.User;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Web;
    using northeast = ViewModels.User.northeast;
    using southwest = ViewModels.User.southwest;

    public class HomepageService : IHomepageService
    {
        #region  private variable 
        private readonly PartnerCarrier_DevEntities db;
        #endregion

        #region Constructor
        public HomepageService(PartnerCarrier_DevEntities dba)
        {
            db = dba;
            db.Database.CommandTimeout = 300;
        }
        #endregion

        #region Us state list
        /// <summary>
        /// Get Us State List
        /// </summary>
        /// <returns></returns>
        public List<StateVM> GetUsStates(bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked, int ReviewFilterValue)
        {
            // Base query: join States and TransportCompanies
            var query = from state in db.States
                        join transport in db.TransportCompanies
                            on state.StateCode equals transport.PhysicalAddressStateCode
                        select new { state, transport };

            // Apply Hiring filter if required
            if (isHiringCheckboxIsChecked && GlobalHire == (int)GLobalHiring.HomeStateAndCityPage)
            {
                query = query.Join(db.Businesses,
                                   t => t.transport.USDOTNumber,
                                   b => b.USDOTNumber,
                                   (t, b) => new { t.state, t.transport, business = b })
                             .Where(x => x.business.NowHiring)
                             .Select(x => new { x.state, x.transport });
            }

            // Apply Reviews filter if required
            if (isReviewsFilterCheckboxIsChecked && ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage)
            {
                query = query.Join(db.Reviews,
                                   t => t.transport.USDOTNumber,
                                   r => r.CompanyUSDOT,
                                   (t, r) => new { t.state, t.transport });
            }

            // Group by state and count distinct companies
            var result = query
                .Where(x => x.state.CountryCode == "US")
                .GroupBy(x => new { x.state.CountryCode, x.state.State1, x.state.StateCode })
                .OrderBy(g => g.Key.State1)
                .Select(g => new StateVM
                {
                    CountryCode = g.Key.CountryCode,
                    State = g.Key.State1,
                    StateCode = g.Key.StateCode,
                    StateCount = g.Select(x => x.transport.USDOTNumber).Distinct().Count()
                })
                .ToList();

            return result;
        }


        #endregion

        #region CA state list
        /// <summary>
        /// Get CA State List
        /// </summary>
        /// <returns></returns>
        public List<StateVM> GetCaStates(bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked, int ReviewFilterValue)
        {
            // Base query: join States -> TransportCompanies -> Businesses -> Reviews
            var query = from st in db.States
                        join tc in db.TransportCompanies on st.StateCode equals tc.PhysicalAddressStateCode into tcJoin
                        from tc in tcJoin.DefaultIfEmpty()
                        join b in db.Businesses on tc.USDOTNumber equals b.USDOTNumber into bJoin
                        from b in bJoin.DefaultIfEmpty()
                        join r in db.Reviews on tc.USDOTNumber equals r.CompanyUSDOT into rJoin
                        from r in rJoin.DefaultIfEmpty()
                        where st.CountryCode == "CA"
                        select new { st, tc, b, r };

            // Apply Hiring filter if checkbox is checked
            if (isHiringCheckboxIsChecked && GlobalHire == (int)GLobalHiring.HomeStateAndCityPage)
            {
                query = query.Where(x => x.b != null && x.b.NowHiring);
            }

            // Apply Reviews filter if checkbox is checked
            if (isReviewsFilterCheckboxIsChecked && ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage)
            {
                query = query.Where(x => x.r != null);
            }

            // Group by state and count distinct companies
            var result = query
                .GroupBy(x => new { x.st.CountryCode, x.st.State1, x.st.StateCode })
                .OrderBy(g => g.Key.State1)
                .Select(g => new StateVM
                {
                    CountryCode = g.Key.CountryCode,
                    State = g.Key.State1,
                    StateCode = g.Key.StateCode,
                    StateCount = g.Select(x => x.tc != null ? x.tc.USDOTNumber : 0)
                                  .Distinct()
                                  .Count()
                })
                .ToList();

            return result;
        }

        public List<StateVM> GetAllStates()
        {
            return (from states in db.States
                    select new StateVM
                    {
                        CountryCode = states.CountryCode,
                        State = states.State1,
                        StateCode = states.StateCode,
                    }).ToList();
        }

        /// <summary>
        /// Get All LoadTypes for binding dropdown
        /// </summary>
        /// <returns></returns>
        public List<LoadTypeVM> GetDropDownListForLoadType()
        {
            return (from loadType in db.LoadTypes
                    select new LoadTypeVM
                    {
                        Id = loadType.Id,
                        LoadName = loadType.Name,
                        LoadDescription = loadType.Description,
                    }).ToList();
        }

        /// <summary>
        /// Get All Pickup Location Type for binding dropdown
        /// </summary>
        /// <returns></returns>
        public List<LocationTypeVM> GetDropDropdownForPickupLocationType(string loadType)
        {
            return (from locationType in db.LocationTypes
                    where locationType.Location == "Pickup" && locationType.LoadType.Name == loadType
                    select new LocationTypeVM
                    {
                        Id = locationType.Id,
                        Location = locationType.Location,
                        Name = locationType.Name,
                    }).ToList();
        }

        /// <summary>
        /// Get All Delivery Location Type for binding dropdown
        /// </summary>
        /// <returns></returns>
        public List<LocationTypeVM> GetDropdownForDeliveryLocationType(string loadType)
        {
            return (from locationType in db.LocationTypes
                    where locationType.Location == "Delivery" && locationType.LoadType.Name == loadType
                    select new LocationTypeVM
                    {
                        Id = locationType.Id,
                        Location = locationType.Location,
                        Name = locationType.Name,
                    }).ToList();
        }
        #endregion

        #region  City List
        /// <summary>
        /// Get distinct City List For Selected State 
        /// </summary>
        /// <returns></returns>
        public List<CityVM> GetCityList(string state, bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked = false, int ReviewFilterValue = 0)
        {
            // Base query to select all active complies
            var query = db.TransportCompanies
                .Where(tc => tc.PhysicalAddressStateCode == state /* && tc.Status == "A" Arkady May 31 2026 */);

            // If Hiring now checkbox checked then fiter compaies accordingly
            if (isHiringCheckboxIsChecked && (GlobalHire == (int)GLobalHiring.HomeStateAndCityPage || GlobalHire == (int)GLobalHiring.StateAndCityPage))
            {
                query = from tc in query
                        join b in db.Businesses on tc.USDOTNumber equals b.USDOTNumber
                        where b.NowHiring == true
                        select tc;
            }

            // If reviews checkbox checked then filter companies and gets companies having at leat one review
            if (isReviewsFilterCheckboxIsChecked && (ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage || ReviewFilterValue == (int)GLobalHiring.StateAndCityPage))
            {
                var reviewedCompanies =
                    from r in db.Reviews
                    group r by r.CompanyUSDOT into g
                    select g.Key.ToString();

                query =
                    from tc in query
                    where reviewedCompanies.Contains(tc.USDOTNumber.ToString())
                    select tc;
            }

            // Group and return
            return query
                .Where(tc => !string.IsNullOrEmpty(tc.PhysicalAddressCity))
                .GroupBy(tc => tc.PhysicalAddressCity)
                  //               .OrderByDescending(g => g.Count())
                  .OrderBy(g => g.Key) // <-- CHANGED: Sorts alphabetically by City Name (g.Key)  
                .Select(g => new CityVM
                {
                    CityName = g.Key,
                    StateCode = state,
                    CompanyCount = g.Count()
                })
                .ToList();
        }


        #endregion

        #region Get Popular cities 
        /// <summary>
        /// Get 10 most popular cites which has most number of companies
        /// first get companies group by cities set order by descending 
        /// then get first 10 cities
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<CityVM> GetPopularCities(string state, bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked = false, int ReviewFilterValue = 0)
        {
            // Base query to select all active complies
            var query = db.TransportCompanies
                .Where(tc => tc.PhysicalAddressStateCode == state /* && tc.Status == "A" Arkady May 31 2026 */);

            // If Hiring now checkbox checked then fiter compaies accordingly
            if (isHiringCheckboxIsChecked && (GlobalHire == (int)GLobalHiring.HomeStateAndCityPage || GlobalHire == (int)GLobalHiring.StateAndCityPage))
            {
                query = from tc in query
                        join b in db.Businesses on tc.USDOTNumber equals b.USDOTNumber
                        where b.NowHiring == true
                        select tc;
            }

            // If reviews checkbox checked then filter companies and gets companies having at leat one review
            if (isReviewsFilterCheckboxIsChecked && (ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage || ReviewFilterValue == (int)GLobalHiring.StateAndCityPage))
            {
                var reviewedCompanies =
                    from r in db.Reviews
                    group r by r.CompanyUSDOT into g
                    select g.Key.ToString();

                query =
                    from tc in query
                    where reviewedCompanies.Contains(tc.USDOTNumber.ToString())
                    select tc;
            }

            // Group and return
            return query
                .Where(tc => !string.IsNullOrEmpty(tc.PhysicalAddressCity))
                .GroupBy(tc => tc.PhysicalAddressCity)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new CityVM
                {
                    CityName = g.Key,
                    StateCode = state,
                    CompanyCount = g.Count()
                })
                .ToList();
        }

        #endregion

        #region Get Companyinformation
        /// <summary>
        /// get company information by usdotnumber
        /// </summary>
        /// <param name="usdotnumber"></param>
        /// <returns></returns>
        public CompanyInformationVM GetCompanyInformation(int usdotnumber, string companyURL)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            //check company exist or not from USDOT NUmber
            var checkIsCompanyExist = (from company in db.TransportCompanies
                                       where company.USDOTNumber == usdotnumber
                                       select company).Any();
            if (!checkIsCompanyExist)
            {

                var actualMessage = Config.SiteURL + companyURL + " - USDOT Number Doesn't exist in our system";
                AppLogger.Instance.Log(actualMessage, LogType.Info, null, true);
                throw new HttpException(404, "Page Not Found");
            }

            DateTime date;
            List<string> entityList = new List<string>();

            var companyInfo = (from company in db.TransportCompanies
                               join busineess in db.Businesses on company.USDOTNumber equals busineess.USDOTNumber into joinBusinessRecords
                               from matchedBusiness in joinBusinessRecords.DefaultIfEmpty()
                               where company.USDOTNumber == usdotnumber
                               select new CompanyInformationVM
                               {
                                   LegalName = company.LegalName,
                                   DoingBusinessAsName = company.DoingBusinessAsName,
                                   PhysicalAddressCity = company.PhysicalAddressCity,
                                   MailingAddressCity = company.MailingAddressCity,
                                   PhysicalAddressStreet = company.PhysicalAddressStreet,
                                   MailingAddressStreet = company.MailingAddressStreet,
                                   MailingAddressZipCode = company.MailingAddressZipCode,
                                   PhysicalAddressZipCode = company.PhysicalAddressZipCode,
                                   PhysicalAddressStateCode = company.PhysicalAddressStateCode,
                                   MailingAddressStateCode = company.MailingAddressStateCode,
                                   CellPhoneNumber = company.CellPhoneNumber,
                                   OfficeTelephoneNumber = company.OfficeTelephoneNumber,
                                   IccDocketNumberFirst = company.IccDocketNumberFirst,
                                   OfficeFaxPhoneNumber = company.OfficeFaxPhoneNumber,
                                   USDOTNumber = company.USDOTNumber,
                                   HazmatIndicator = company.HazmatIndicator,
                                   EntityType = company.EntityType,
                                   DateAdded = company.DateAdded,
                                   OperationCarrierInterstate = company.OperationCarrierInterstate,
                                   OperationCarrierIntrastateHazmat = company.OperationCarrierIntrastateHazmat,
                                   OperationCarrierIntrastateNonHazmat = company.OperationCarrierIntrastateNonHazmat,
                                   NNDriversGrandTotalInterstateAndIntrastate = company.NNDriversGrandTotalInterstateAndIntrastate,
                                   TrucksAndTractors = company.TrucksAndTractors,
                                   Services = (from service in company.ServiceTypes
                                               select service.Service_Type.Trim()).ToList(),
                                   CompanyCargoType = (from cargo in company.CargoTypes
                                                       select cargo.CargoName.Trim()).ToList(),

                                   TrailerType = (from trailer in db.TrailerTypes
                                                  select trailer.TrailerName.Trim()).ToList(),
                                   DriverType = (from driver in db.DriverTypes
                                                 select driver.DriverName.Trim()).ToList(),

                                   CompanyTrailerType = (from trailer in company.TrailerTypes
                                                         select trailer.TrailerName.Trim()).ToList(),
                                   CompanyDriverType = (from driver in company.DriverTypes
                                                        select driver.DriverName.Trim()).ToList(),
                                   CompanysWebsiteName = matchedBusiness == null ? null : matchedBusiness.Website,
                                   CompanysEmailAddress = company.EmailAddress,
                                   WebsiteApproved = matchedBusiness == null ? null : matchedBusiness.WebsiteApproved,
                                   UsDOtNum = matchedBusiness == null ? 0 : matchedBusiness.USDOTNumber,
                                   BusinessContactEmail = matchedBusiness == null ? null : matchedBusiness.BusinessContactEmail,
                                   JobContactEmail = matchedBusiness == null ? null : matchedBusiness.JobContactEmail,
                                   JobContactPhone = matchedBusiness == null ? null : matchedBusiness.JobContactPhone,
                                   JobContactSMS = matchedBusiness == null ? null : matchedBusiness.JobContactSMS,
                                   NowHiring = matchedBusiness == null ? false : matchedBusiness.NowHiring,
                                   CompanyName = company.CompanyName,
                                   Status = company.Status
                               }).FirstOrDefault();

            //set entity type as required
            if (companyInfo.EntityType.Contains("C"))
            {
                entityList.Add("Carrier");

            }
            if (companyInfo.EntityType.Contains("T"))
            {
                entityList.Add("Cargo Tank");
            }
            if (companyInfo.EntityType.Contains("S"))
            {
                entityList.Add("Shipper");
            }
            if (companyInfo.EntityType.Contains("B"))
            {
                entityList.Add("Broker");
            }
            if (companyInfo.EntityType.Contains("F"))
            {
                entityList.Add("Freight Forwarder");
            }
            if (companyInfo.EntityType.Contains("R"))
            {
                entityList.Add("Registrant");
            }
            if (string.IsNullOrEmpty(companyInfo.EntityType))
            {
                entityList.Add("N/A");
            }
            companyInfo.Entity = String.Join(", ", entityList);
            //if company has services then set into commasaperated
            if (companyInfo.Services.Count > 0)
            {
                companyInfo.ServiceInCommaSaperated = String.Join(", ", companyInfo.Services);
            }
            else
            { companyInfo.ServiceInCommaSaperated = "N/A"; }
            //if company has cargotypes then set into comma saperated
            if (companyInfo.CompanyCargoType.Count > 0)
            {
                companyInfo.CargoInCommaSaperated = String.Join(", ", companyInfo.CompanyCargoType);
            }
            else
            { companyInfo.CargoInCommaSaperated = "N/A"; }
            //convert date int to string and set as required
            if (DateTime.TryParseExact(companyInfo.DateAdded.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                companyInfo.DateInString = date.ToString("MMMM dd yyyy");
            }

            // CanWriteReview logic
            if (loggedInUser != null)
            {
                if (loggedInUser.USDOTNumber == usdotnumber)
                {
                    companyInfo.CanWriteReview = false;
                }
            }

            return companyInfo;
        }
        #endregion

        #region PageTitle
        /// <summary>
        /// Get PageTitle  by page Name from admin table
        /// set into browser title
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public string GetPageTitle(string pageName, string state, string city)
        {
            string pagetitle = "";
            if (pageName == "Homepage")
            {
                pagetitle = db.Database.SqlQuery<string>("select HomePageTitle from Admin").FirstOrDefault();
            }
            if (pageName == "Statepage")
            {
                pagetitle = db.Database.SqlQuery<string>("select StatePageTitle from Admin").FirstOrDefault();
                pagetitle = state + pagetitle;
            }
            if (pageName == "Citypage")
            {
                pagetitle = db.Database.SqlQuery<string>("select CityPageTitle from Admin").FirstOrDefault();
                pagetitle = city + " " + state + " " + pagetitle;
            }
            return pagetitle;
        }
        #endregion

        #region Page Description
        /// <summary>
        /// Get Page Description Name by page Name from admin table
        /// set into meta tag
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public string GetPageDescription(string pageName, string state, string city, int? pageNum)
        {
            string pageDescription = "";
            if (pageName == "Homepage")
            {
                //Get Count of driver jobs
                var countOfDriverJobs = db.Database.SqlQuery<int>("select count(*) from Business where NowHiring!=0").FirstOrDefault();
                //Get home page descriptions
                pageDescription = db.Database.SqlQuery<string>("select HomePageDescription from Admin").FirstOrDefault();
                //Replace Truck driver jobs string with no. of jobs count string
                var newReplaceStringWithDriverJobCounts = countOfDriverJobs.ToString() + " Truck Driver Jobs";
                var finalLink = "<a tabindex='0' role='button' data-toggle='popover' data-html='true' data-trigger='focus' data-container='body' data-placement='bottom' data-content='To see only hiring companies please select \"Companies Now Hiring\" checkbox in the top-right corner of the page. <br />Trucking Company Owner / Operator ? Click the Login link in the top-right corner of the page to create your account on TruckCarrierHub.com and post your trucking jobs free of charge.' class='link-font-size'> " + newReplaceStringWithDriverJobCounts + "</a>";
                pageDescription = pageDescription.Replace("Truck Driver Jobs", finalLink);

                //Get Count of total companies
                var countOfTotalCompanies = db.Database.SqlQuery<int>("select count (*) from TransportCompany").FirstOrDefault();
                pageDescription = pageDescription.Replace("more than 1.8 million", countOfTotalCompanies.ToString("#,##0"));
            }
            if (pageName == "Statepage")
            {
                var StateArticle = db.Database.SqlQuery<string>("SELECT StateArticle FROM States WHERE State = @p0", state).FirstOrDefault();
                if (StateArticle != null)
                {
                    pageDescription = StateArticle;
                }
                else
                {
                    pageDescription = db.Database.SqlQuery<string>("select StatePageDescription from Admin").FirstOrDefault();
                    pageDescription = state + pageDescription;
                }
            }
            if (pageName == "Citypage")
            {
                var stateCode = db.States.FirstOrDefault(s => s.State1.ToLower() == state.ToLower()).StateCode;
                var selectedCity = db.Cities.FirstOrDefault(s => s.CityName == city && s.StateCode == stateCode);
                if (selectedCity != null && selectedCity.Article != null)
                {
                    pageDescription = selectedCity.Article;
                }
            }
            return pageDescription;
        }

        public string GetCityMetaDescription(string state, string city)
        {
            var stateCode = db.States.FirstOrDefault(s => s.State1.ToLower() == state.ToLower()).StateCode;
            var selectedCity = db.Cities.FirstOrDefault(s => s.CityName == city && s.StateCode == stateCode);
            if (selectedCity != null && selectedCity.Description != null)
                return selectedCity.Description;
            var fallback = db.Database.SqlQuery<string>("select CityPageDescription from Admin").FirstOrDefault();
            return city + " " + state + " " + fallback;
        }

        public string GetPlainHomeMetaDescription()
        {
            var countOfDriverJobs = db.Database.SqlQuery<int>("select count(*) from Business where NowHiring!=0").FirstOrDefault();
            var description = db.Database.SqlQuery<string>("select HomePageDescription from Admin").FirstOrDefault() ?? "";
            var plainCount = countOfDriverJobs.ToString() + " Truck Driver Jobs";
            description = description.Replace("Truck Driver Jobs", plainCount);
            var countOfTotalCompanies = db.Database.SqlQuery<int>("select count (*) from TransportCompany").FirstOrDefault();
            description = description.Replace("more than 1.8 million", countOfTotalCompanies.ToString("#,##0"));
            return description;
        }

        /// <summary>
        /// Get the homepage article text (Admin.HomeArticle), shown on the
        /// homepage separately from PageDescription, which has its own
        /// dynamic job-count content and is also reused for the meta
        /// description tag.
        /// </summary>
        public string GetHomeArticle()
        {
            return db.Database.SqlQuery<string>("select HomeArticle from Admin").FirstOrDefault();
        }

        /// <summary>
        /// get State Name From State Code
        /// purpose: Set Name in Page Title and Page Description
        /// </summary>
        /// <param name="stateCode"></param>
        /// <returns></returns>
        public string GetStateName(string stateCode)
        {
            var stateName = (from states in db.States
                             where states.StateCode == stateCode.Trim()
                             select states.State1).FirstOrDefault();

            if (string.IsNullOrEmpty(stateName))
            {
                var actualMessage = Config.SiteURL + stateCode + " - State Doesn't exist in our system";
                AppLogger.Instance.Log(actualMessage, LogType.Info, null, true);
                throw new HttpException(404, "Page Not Found");
            }
            return stateName;
        }
        #endregion

        #region Page Title for Company Information page
        /// <summary>
        /// set page title for comapny information page
        /// </summary>
        /// <param name="usdotNumber"></param>
        /// <returns></returns>
        public string SetPageTitleForCompanyInformation(int usdotNumber)
        {
            string pageTitle = "";
            var companyInfo = (from companies in db.TransportCompanies
                               where companies.USDOTNumber == usdotNumber
                               select companies).First();
            if (!string.IsNullOrEmpty(companyInfo.DoingBusinessAsName))
            {
                pageTitle = companyInfo.DoingBusinessAsName + ",";
            }
            else
            {
                pageTitle = companyInfo.LegalName + ",";
            }
            pageTitle += " USDOT " + companyInfo.USDOTNumber + ", ";
            if (companyInfo.IccDocketNumberFirst.HasValue)
            {
                pageTitle += "MC Number " + companyInfo.IccDocketNumberFirst + ", ";
            }
            pageTitle += companyInfo.PhysicalAddressCity + ", " + companyInfo.PhysicalAddressStateCode + ",";
            if (companyInfo.EntityType == "C" || companyInfo.EntityType == "T")
            {
                pageTitle += " Trucking Company";
            }
            else
            {
                pageTitle += " Freight Broker";
            }
            return pageTitle;
        }
        #endregion

        #region Set Page Description For Company Information Page
        /// <summary>
        /// set page description for company information page
        /// first get service types of selected company
        /// set all service types in comma saperated
        /// then set page description required
        /// </summary>
        /// <param name="usdotnumber"></param>
        /// <returns></returns>
        public string SetPageDescriptionForCompanyInformation(int usdotnumber)
        {
            string pageDescription = "";
            List<string> servicTypes = new List<string>();
            string servicTypeCommaSaperated = "";
            //get company nformation by usdotNumber
            var companyInfo = (from companies in db.TransportCompanies
                               where companies.USDOTNumber == usdotnumber
                               select companies).First();
            //now get all services of this company
            companyInfo.ServiceTypes = GetAllserviceOfselectedCompany(usdotnumber);
            if (companyInfo.ServiceTypes.Count > 0)
            {
                foreach (var item in companyInfo.ServiceTypes)
                {
                    servicTypes.Add(item.Service_Type);
                }
                servicTypeCommaSaperated = String.Join(", ", servicTypes);
            }

            if (!string.IsNullOrEmpty(companyInfo.DoingBusinessAsName))
            {
                pageDescription = companyInfo.DoingBusinessAsName + ", ";
            }
            pageDescription += companyInfo.LegalName + " is a ";
            if (companyInfo.HazmatIndicator == "Y")
            {
                pageDescription += "Hazmat certified ";
            }
            pageDescription += "freight shipping ";
            if (companyInfo.EntityType == "C" || companyInfo.EntityType == "T")
            {
                pageDescription += "Trucking Company";
            }
            else
            {
                pageDescription += "Broker";
            }
            pageDescription += " from " + companyInfo.PhysicalAddressCity + ", " + companyInfo.PhysicalAddressStateCode + ".";
            pageDescription += " Company USDOT number is " + companyInfo.USDOTNumber;
            if (companyInfo.IccDocketNumberFirst.HasValue)
            {
                pageDescription += " and docket number is " + companyInfo.IccDocketNumberFirst;
            }
            pageDescription += ". Transportation Services provided: " + servicTypeCommaSaperated;
            return pageDescription;
        }
        #endregion

        #region Service Tpes
        /// <summary>
        /// Get All Services of selected Company
        /// </summary>
        /// <param name="usdotnumber"></param>
        /// <returns></returns>
        public List<ServiceType> GetAllserviceOfselectedCompany(int usdotnumber)
        {
            var services = db.TransportCompanies.Where(r => r.USDOTNumber == usdotnumber).SelectMany(x => x.ServiceTypes).Distinct().ToList();
            return services;
        }
        #endregion

        #region CargoTypes
        /// <summary>
        /// Get All Cargo Types Of Selected Company
        /// </summary>
        /// <param name="usdotnumber"></param>
        /// <returns></returns>
        public List<CargoType> GetAllCargoOfselectedCompany(int usdotnumber)
        {
            var cargolist = db.TransportCompanies.Where(r => r.USDOTNumber == usdotnumber).SelectMany(x => x.CargoTypes).Distinct().ToList();
            return cargolist;
        }
        #endregion

        #region Search Results
        /// <summary>
        /// Get Result autocomplete in searchtextbox
        /// this method will be execute after user enter 3 character in search textbox 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        public List<SearchVM> GetSearchResultAutoComplete(string searchText, string selectedValue)
        {
            var searchList = new List<SearchVM>();
            if (selectedValue == "City")
            {
                //get city list
                searchList = (from companies in db.TransportCompanies
                              where companies.PhysicalAddressCity.StartsWith(searchText)
                              select new SearchVM
                              {
                                  Value = companies.PhysicalAddressCity + ", " + companies.PhysicalAddressStateCode
                              }).Distinct().ToList();
            }
            else if (selectedValue == "Company Name")
            {
                //get company list list
                searchList = (from companies in db.TransportCompanies
                              where companies.CompanyName.StartsWith(searchText)
                              orderby companies.CompanyName
                              select new SearchVM
                              {
                                  Value = companies.CompanyName + ", " + companies.PhysicalAddressCity + ", " + companies.PhysicalAddressStateCode
                              }).Distinct().Take(50).ToList();
            }

            return searchList;
        }
        /// <summary>
        /// Get Result autocomplete in searchtextbox
        /// this method will be execute after user enter 3 character in search textbox 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        public List<SearchVM> GetSearchResultAutoCompleteCity(string searchText, string selectedValue)
        {
            //get city list
            var cityList = (from transportcompanies in db.TransportCompanies
                            join state in db.States on transportcompanies.PhysicalAddressStateCode equals state.StateCode
                            where transportcompanies.PhysicalAddressCity.StartsWith(searchText)
                            select new SearchVM
                            {
                                Value = transportcompanies.PhysicalAddressCity + ", " + transportcompanies.PhysicalAddressStateCode
                            }).Distinct().ToList();
            return cityList;
        }

        /// <summary>
        /// Get Company List From Serach TextBox
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="selectedDropdownValue"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public PagedList<CompanyVM> GetCompanyListFromSearch(string searchText, string selectedDropdownValue, PageSortPara ps, bool isHiringCheckboxIsChecked, int GlobalHire)
        {
            //set pagesize
            int pageSize = 70;
            string city = "";
            string state = "";
            ps.p = ((!ps.p.HasValue) || ps.p == 0) ? 1 : Convert.ToInt32(ps.p);
            ps.se = String.IsNullOrEmpty(ps.se) ? "LegalName" : ps.se;
            ps.sd = String.IsNullOrEmpty(ps.sd) ? "Asc" : ps.sd;
            IQueryable<CompanyVM> companyList;
            //now check user selected value
            if (string.IsNullOrEmpty(selectedDropdownValue) || selectedDropdownValue == "City")
            {
                //If is global hiring checkbox is checked then it returns only Hiring companies and and check global hire 
                //else it returns all  the companies
                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    if (searchText.Contains(","))
                    {
                        //get city
                        city = searchText.Split(',')[0];
                        //get state
                        state = searchText.Split(',')[1].Trim();
                    }

                    companyList = (from companies in db.TransportCompanies
                                   where (companies.Business.TransportCompany.PhysicalAddressCity == city && companies.Business.TransportCompany.PhysicalAddressStateCode == state) && companies.Business.NowHiring == true
                                   orderby companies.LegalName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });
                }
                else
                {
                    if (searchText.Contains(","))
                    {
                        //get city
                        city = searchText.Split(',')[0];
                        //get state
                        state = searchText.Split(',')[1].Trim();
                    }
                    companyList = (from companies in db.TransportCompanies
                                   where companies.PhysicalAddressCity == city && companies.PhysicalAddressStateCode == state
                                   orderby companies.LegalName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                       StateCode = companies.PhysicalAddressStateCode
                                   });


                }

            }
            //if user search USDOT Number
            else if (selectedDropdownValue == "USDOT Number")
            {
                //If user check checkbox for Hiring then it returns only hiring companies with USDOT Number 
                //else it gives  the comapny which match USDOT Number
                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    searchText = searchText.Trim();
                    companyList = (from companies in db.TransportCompanies
                                   where (companies.Business.USDOTNumber + "") == searchText && companies.Business.NowHiring == true
                                   orderby companies.LegalName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });

                }
                else
                {
                    searchText = searchText.Trim();
                    companyList = (from companies in db.TransportCompanies
                                   where (companies.USDOTNumber + "") == searchText
                                   orderby companies.LegalName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });
                }

            }
            else if (selectedDropdownValue == "Company Name")
            {
                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    searchText = searchText.Trim();
                    var companyName = searchText.Split(',')[0];
                    companyList = (from companies in db.TransportCompanies
                                   where (companies.Business.TransportCompany.CompanyName) == companyName && companies.Business.NowHiring == true
                                   orderby companies.CompanyName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });
                }
                else
                {
                    searchText = searchText.Trim();
                    var companyName = searchText.Split(',')[0];
                    companyList = (from companies in db.TransportCompanies
                                   where (companies.CompanyName) == companyName
                                   orderby companies.CompanyName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });

                    var searchTextSplits = searchText.Split(',');
                    if (searchTextSplits.Count() > 1)
                    {
                        city = searchTextSplits[1].Trim();
                        companyList = companyList.Where(companies => companies.PhysicalAddressCity == city);
                    }
                    if (searchTextSplits.Count() > 2)
                    {
                        state = searchTextSplits[2].Trim();
                        companyList = companyList.Where(companies => companies.PhysicalAddressStateCode == state);
                    }
                }
            }
            //if user search MC Number
            else
            {
                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    searchText = searchText.Trim();
                    companyList = (from companies in db.TransportCompanies
                                   where (companies.Business.TransportCompany.IccDocketNumberFirst + "") == searchText && companies.Business.NowHiring == true
                                   orderby companies.LegalName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });
                }
                else
                {
                    searchText = searchText.Trim();
                    companyList = (from companies in db.TransportCompanies
                                   where (companies.IccDocketNumberFirst + "") == searchText
                                   orderby companies.LegalName
                                   select new CompanyVM
                                   {
                                       LegalName = companies.LegalName,
                                       DoingBusinessAsName = companies.DoingBusinessAsName,
                                       PhysicalAddressCity = companies.PhysicalAddressCity,
                                       PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                                       PhysicalAddressStreet = companies.PhysicalAddressStreet,
                                       PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                                       USDOTNumber = companies.USDOTNumber,
                                       IccDocketNumberFirst = companies.IccDocketNumberFirst,
                                       OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                                       CellPhoneNumber = companies.CellPhoneNumber,
                                       City = companies.PhysicalAddressCity,
                                   });
                }
            }
            if (companyList.Count() <= 0)
            {
                throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);
            }
            PagedList<CompanyVM> allCompanies = companyList.AsQueryable().SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
            return allCompanies;
        }
        #endregion

        /// <summary>
        ///Get all service Types which is avilable in the database 
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetServiceTypes()
        {
            var availableServiceTypes = (from service in db.ServiceTypes
                                         select new ServiceTypeVM
                                         {
                                             ServiceType = service.Service_Type.Trim(),
                                             ServiceNumber = service.ServiceTypeNumber,
                                             ServiceTypeForUrl = service.ServiceTypeForUrl
                                         }).ToList();
            var list = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < availableServiceTypes.Count; i++)
            {
                list.Add(new KeyValuePair<string, string>(availableServiceTypes[i].ServiceType, availableServiceTypes[i].ServiceTypeForUrl));
            }
            return list;
        }

        /// <summary>
        ///Get all Cargo Types which is avilable in the database 
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetCargoTypes()
        {
            var availableCargoTypes = (from cargo in db.CargoTypes
                                       select new CargoTypeVM
                                       {
                                           CargoName = cargo.CargoName.Trim(),
                                           CargoNumber = cargo.CargoNumber,
                                           CargoNameForUrl = cargo.CargoNameForUrl
                                       }).ToList();
            var list = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < availableCargoTypes.Count; i++)
            {
                list.Add(new KeyValuePair<string, string>(availableCargoTypes[i].CargoName, availableCargoTypes[i].CargoNameForUrl));
            }
            return list;
        }

        /// <summary>
        ///Get all Trailer Types which is avilable in the database  to bind dropdown for filter 
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetTrailerTypesForFilter()
        {
            var availableTrailerTypes = (from trailerType in db.TrailerTypes
                                         select new TrailerTypeVM
                                         {
                                             TrailerName = trailerType.TrailerName.Trim(),
                                             TrailerNumber = trailerType.TrailerNumber,
                                             TrailerNameForUrl = trailerType.TrailerNameForUrl
                                         }).ToList();
            var list = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < availableTrailerTypes.Count; i++)
            {
                list.Add(new KeyValuePair<string, string>(availableTrailerTypes[i].TrailerName, availableTrailerTypes[i].TrailerNameForUrl));
            }
            return list;
        }

        /// <summary>
        ///Get all Driver Types which is avilable in the database  to bind dropdown for filter 
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetDriverTypesForFilter()
        {
            var availableTrailerTypes = (from trailerType in db.DriverTypes
                                         select new DriverTypeVM
                                         {
                                             DriverName = trailerType.DriverName.Trim(),
                                             DriverNumber = trailerType.DriverNumber,
                                             DriverNameForUrl = trailerType.DriverNameForUrl
                                         }).ToList();
            var list = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < availableTrailerTypes.Count; i++)
            {
                list.Add(new KeyValuePair<string, string>(availableTrailerTypes[i].DriverName, availableTrailerTypes[i].DriverNameForUrl));
            }
            return list;
        }

        /// <summary>
        ///Get all Trailer Types which is avilable in the database 
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, int>> GetTrailerTypes()
        {
            var availableTrailerTypes = (from trailerType in db.TrailerTypes
                                         select new TrailerTypeVM
                                         {
                                             TrailerName = trailerType.TrailerName.Trim(),
                                             TrailerNumber = trailerType.TrailerNumber,
                                             TrailerNameForUrl = trailerType.TrailerNameForUrl
                                         }).ToList();
            var list = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < availableTrailerTypes.Count; i++)
            {
                list.Add(new KeyValuePair<string, int>(availableTrailerTypes[i].TrailerName, availableTrailerTypes[i].TrailerNumber));
            }
            return list;
        }

        /// <summary>
        ///Get all Driver Types which is avilable in the database 
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, int>> GetDriverTypes()
        {
            var availableDriverTypes = (from driverType in db.DriverTypes
                                        select new DriverTypeVM
                                        {
                                            DriverName = driverType.DriverName.Trim(),
                                            DriverNumber = driverType.DriverNumber,
                                            DriverNameForUrl = driverType.DriverNameForUrl
                                        }).ToList();
            var list = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < availableDriverTypes.Count; i++)
            {
                list.Add(new KeyValuePair<string, int>(availableDriverTypes[i].DriverName, availableDriverTypes[i].DriverNumber));
            }
            return list;
        }

        public string GetCityNameFromURLCityName(string urlCityName)
        {
            // this function is needed because in URL the city name is having "-" and url doesn't accept that.
            // to make it clean url, we repalced space with "-" when creating url
            // so when we want to search based on city name, we have to do reverse replace...and that's what we are trying to do in this fucntion

            if (!urlCityName.Contains("-"))
            {
                // if url doesn't have "-" in it, nothing to be done, just return it.                 
                return urlCityName;
            }
            else
            {
                string cityNameFromDb = null;
                // case1 - compare in db after replacing "-"
                // reverse "-" to " "...because thats what we do when generating URL
                var replaceCityNameDashWithSpace = urlCityName.Replace("-", " ");

                cityNameFromDb = (from a in db.TransportCompanies
                                  where a.PhysicalAddressCity == replaceCityNameDashWithSpace
                                  select a.PhysicalAddressCity).FirstOrDefault();

                if (cityNameFromDb == null)
                {
                    // case2 - compare in db without replacing "-", for the city name, which itself has "-" in it. example "Saint-David"
                    cityNameFromDb = (from a in db.TransportCompanies
                                      where a.PhysicalAddressCity == urlCityName
                                      select a.PhysicalAddressCity).FirstOrDefault();
                }

                if (cityNameFromDb == null)
                {
                    cityNameFromDb = (from a in db.TransportCompanies
                                      where a.PhysicalAddressCity.Replace(" ", "-") == urlCityName
                                      select a.PhysicalAddressCity).FirstOrDefault();
                }

                return cityNameFromDb;
            }
        }

        /// <summary>
        /// Get Company List 
        /// </summary>
        /// <param name="searchFilterVM">Company filter options value</param>
        /// <param name="stateCode">State code </param>
        /// <param name="cityName">City name</param>
        /// <param name="ps">Page sort parameter values</param>
        /// <param name="isForMapView">when isForMapView - return all the matched company records without pagination</param>
        /// <param name="isRestrictResultToCity">bool isRestrictResultToCity = false; // if this is true, then company result should be restricted specific to a city, else it should be restricted to the google map region based on the map co-ordinates passed</param>
        /// <param name="isHiringCheckboxIsChecked">For check hiring checkbox is checked or not</param>
        /// <param name="globalHire">From which page hiring company request came</param>
        /// <returns>List of companies</returns>
        public PagedList<CompanyVM> GetCompanyListFromFilter(SearchFilterVM searchFilterVM, string stateCode, string cityName, PageSortPara ps, bool isForMapView, bool? isRestrictResultToCity, bool isHiringCheckboxIsChecked, int globalHire, bool isReviewsFilterCheckboxIsChecked, int ReviewFilterValue)
        {
            //set default page size
            int pageSize = 70;

            //take global variable for set city and state
            //If user apply city filter or user direct came on city page then display all companies which are in searched city.
            //City may be duplicate so for more sure we are getting state also
            string city = "";
            string state = "";

            //If pageSortPara is null then initialize pageSortPara values
            PageSortPara.Init(ps, "LegalName", SortingDirection.Asc);

            //If user search city from search box from HomePage or from navbar 
            if (!string.IsNullOrEmpty(searchFilterVM.SearchText) && searchFilterVM.SearchText.Contains(","))
            {
                //If user apply search then search textbox contains "CityName, State"
                //so we split city and state by ',' and store in variable

                //get city from searchText
                city = searchFilterVM.SearchText.Split(',')[0];
                //get state from searchText
                state = searchFilterVM.SearchText.Split(',')[1].Trim();
            }
            else
            {
                //if user came on city page from state page then it cityName and state pass as parameter
                /*
                eg.
                    /AL/BAYOU-LA-BATRE
                    AL - StateCode so we have directly store "AL" in state variable
                    BAYOU-LA-BATRE - CityName, but city name contains space so we replace space with "-" in url, So for correct city name we have replace "-" with space
                 */
                //remove '-' from city name which is coming from url
                //here we get city replaced with space wherever cityname having '-' so no need to do replace now
                city = cityName;
                state = stateCode;
            }

            //Take global variable for generate query according filters
            IQueryable<TransportCompany> lstCompanies;

            //If user want to show only hiring companies list then get data from business table because we are storing company is hiring or not in Business table.
            //In navbar there is a checkbox named "Check if you'd like to see only hiring companies" if check box is checked then "isHiringCheckboxIsChecked " value is true and "GlobalHire" variable contains from which page its came from.
            if (isHiringCheckboxIsChecked && (globalHire == (int)GLobalHiring.CityPage || globalHire == (int)GLobalHiring.HomeStateAndCityPage || globalHire == (int)GLobalHiring.StateAndCityPage))
            {
                //Get company list using business table
                //We are storing company is hiring or not in Business table so apply left join business table with TransportCompany using USDDOTNumber 
                //An we are fetching only hiring company by city and state.
                lstCompanies = (from business in db.Businesses
                                join companies in db.TransportCompanies on business.USDOTNumber equals companies.USDOTNumber into matchedBusinessRecord
                                from matchedcompany in matchedBusinessRecord.DefaultIfEmpty()
                                where matchedcompany.PhysicalAddressCity == city && matchedcompany.PhysicalAddressStateCode == state && business.NowHiring == true
                                select matchedcompany).AsQueryable();
            }
            else
            {
                lstCompanies = db.TransportCompanies.Select(companies => companies).AsQueryable();
            }

            //Check if zoomLevel is 0 or isMapView is false then get all city and state data
            if (!isForMapView && !isRestrictResultToCity.Value || isForMapView && !isRestrictResultToCity.Value || searchFilterVM.ZoomLevel == 0)
            {
                lstCompanies = lstCompanies.Where(a => a.PhysicalAddressCity == city && a.PhysicalAddressStateCode == state).AsQueryable();
            }
            //if position/boundary values are available then display
            //here check if any city has only one company then do not apply boundary in map view direct display that single company in center.
            if (searchFilterVM.BoundrieValues != null)
            {
                if (lstCompanies.Count() > 1)
                {
                    lstCompanies = lstCompanies.Where(a => a.Latitude >= searchFilterVM.BoundrieValues.Southwest.lat &&
                      a.Latitude <= searchFilterVM.BoundrieValues.Northeast.lat &&
                      a.Longitude >= searchFilterVM.BoundrieValues.Southwest.lng &&
                      a.Longitude <= searchFilterVM.BoundrieValues.Northeast.lng).AsQueryable();
                }
            }

            //if user search within truckOrTractor 
            //Check TruckOrTractor search variable is not null and its count is not zero
            if (searchFilterVM.SelectedTruckOrTractor != null && searchFilterVM.SelectedTruckOrTractor.Count > 0)
            {
                //Take variable for store min and max one by one in list.
                //set maximum and minimum values in the created object
                /*
                 "searchFilterVM.SelectedTruckOrTractor" this contains selected TruckOrTractor min and max value which are selected from filter area.
                 "Min-Max"
                 So split min and max value using "-" and store in view model
                 */
                /*
                 Expression<Func<TransportCompany, bool>> 
                 using this function we are creating dynamic query for apply filter on MinimumTruckOrTractor and MaximumTruckOrTractor
                */
                Expression<Func<TransportCompany, bool>> whereExpression = null;
                foreach (var truckTracktor in searchFilterVM.SelectedTruckOrTractor)
                {
                    //get minimum number of trucks
                    var getMinimumTruckOrTractor = Convert.ToInt32(truckTracktor.Split('-')[0]);
                    //get maximum number of trucks
                    var getMaximumTruckOrTractor = Convert.ToInt32(truckTracktor.Split('-')[1]);
                    Expression<Func<TransportCompany, bool>> e1 = u => u.TrucksAndTractors >= getMinimumTruckOrTractor;
                    Expression<Func<TransportCompany, bool>> andExpression = e1.And(u => u.TrucksAndTractors <= getMaximumTruckOrTractor);
                    whereExpression = whereExpression == null ? andExpression : whereExpression.Or(andExpression);
                }
                lstCompanies = lstCompanies.Where(whereExpression);
            }
            //if user search with selected ServiceTypes
            //Check user select ServiceType or not 
            if (searchFilterVM.SelectedServiceTypes != null && searchFilterVM.SelectedServiceTypes.Count > 0)
            {
                //if user select ServiceType then using selected service type get matched service type company
                lstCompanies = (from company in lstCompanies
                                from services in company.ServiceTypes.Select(x => x.ServiceTypeForUrl)
                                join selected in searchFilterVM.SelectedServiceTypes on services equals selected
                                select company).Distinct().AsQueryable();


            }
            //check if user search selected CargoTypes
            //check user select CargoType or not
            if (searchFilterVM.SelectedCargoTypes != null && searchFilterVM.SelectedCargoTypes.Count > 0)
            {
                //if user select CargoType then using selected cargo type get matched cargo type company
                lstCompanies = (from company in lstCompanies
                                from cargo in company.CargoTypes.Select(x => x.CargoNameForUrl)
                                join selected in searchFilterVM.SelectedCargoTypes on cargo equals selected
                                select company).Distinct().AsQueryable();
            }
            //Check if user search selected TrailerType
            //check user select TrailerTypes  or not
            if (searchFilterVM.SelectedTrailerTypes != null && searchFilterVM.SelectedTrailerTypes.Count > 0)
            {
                //if user select TrailerType then using selected Trailer type get matched Trailer type company
                lstCompanies = (from company in lstCompanies
                                from trailer in company.TrailerTypes.Select(x => x.TrailerNameForUrl)
                                join selected in searchFilterVM.SelectedTrailerTypes on trailer equals selected
                                select company).Distinct().AsQueryable();


            }
            //Check if User Search Selected Driver Type
            //check user select DriverTypes  or not
            if (searchFilterVM.SelectedDriverTypes != null && searchFilterVM.SelectedDriverTypes.Count > 0)
            {
                //if user select DriverType then using selected Driver type get matched Driver type company
                lstCompanies = (from company in lstCompanies
                                from driver in company.DriverTypes.Select(x => x.DriverNameForUrl)
                                join selected in searchFilterVM.SelectedDriverTypes on driver equals selected
                                select company).Distinct().AsQueryable();
            }
            //check if user search selected EntityType
            if (searchFilterVM.SelectedEntityTypes != null && searchFilterVM.SelectedEntityTypes.Count > 0)
            {
                lstCompanies = lstCompanies.Where(a => searchFilterVM.SelectedEntityTypes.Any(b => a.EntityType.Contains(b))).AsQueryable();
            }

            if (isReviewsFilterCheckboxIsChecked && (ReviewFilterValue == (int)ManageReviewEnum.CityPage || ReviewFilterValue == (int)ManageReviewEnum.HomeStateAndCityPage || ReviewFilterValue == (int)ManageReviewEnum.StateAndCityPage))
            {
                lstCompanies = (from company in lstCompanies
                                join review in db.Reviews on company.USDOTNumber equals review.CompanyUSDOT
                                group review by company into g
                                where g.Any()
                                select g.Key).Distinct().AsQueryable();
            }

            // Sort at the database level so the ORDER BY lands in SQL and Skip/Take
            // fetches only the current page's rows rather than the entire city's result set.
            // (SortRelevance/CompanyName/TrucksAndTractors are TransportCompany entity fields.)
            if (!string.IsNullOrEmpty(searchFilterVM.SortBy))
            {
                if (searchFilterVM.SortBy == "Relevance")
                    lstCompanies = lstCompanies.OrderByDescending(a => a.SortRelevance);
                else if (searchFilterVM.SortBy == "AlphabeticOrder")
                    lstCompanies = lstCompanies.OrderBy(a => a.CompanyName);
                else if (searchFilterVM.SortBy == "NumberOfTrucks")
                    lstCompanies = lstCompanies.OrderByDescending(a => a.TrucksAndTractors);
                //Before we display orderbydescending as "TotalNumberOfTrucks" now we made change here to orderbydescending "TrucksAndTractors"
                /*As per issue of ticket number  #192
                 * When we perform Sort on "Sort by number of trucks". Company with 6 trucks is above company with 5500 trucks so to display "Number of trucks" display proper on city page when perform "Sort by Number of trucks"
                 */
                else
                    lstCompanies = lstCompanies.OrderByDescending(a => a.SortRelevance);
            }
            else
            {
                //if user not select any sorting option then apply default sorting on "SortRelevance"
                lstCompanies = lstCompanies.OrderByDescending(a => a.SortRelevance);
            }

            // One COUNT(*) query for pagination metadata — must happen before Skip/Take.
            int totalCount = lstCompanies.Count();

            // For map view, collapse everything into a single "page" so the caller gets all pins.
            if (isForMapView)
            {
                pageSize = totalCount >= 1 ? totalCount : 1;
            }

            // Skip/Take in SQL — only the current page's rows cross the wire.
            int pageIndex = (int)ps.p;
            if (pageIndex <= 0) { pageIndex = 1; }
            int skipRows = (pageIndex - 1) * pageSize;

            List<TransportCompany> pageEntities = lstCompanies.Skip(skipRows).Take(pageSize).ToList();

            // If the requested page is beyond the last page (stale bookmark), fall back to page 1.
            if (pageEntities.Count == 0 && pageIndex != 1 && totalCount > 0)
            {
                pageIndex = 1;
                pageEntities = lstCompanies.Skip(0).Take(pageSize).ToList();
            }

            // Batch rating query scoped to just this page's companies — not the whole city.
            var usdotNumbers = pageEntities.Select(c => c.USDOTNumber).ToList();
            var ratingsByUsdot = db.Reviews
                .Where(r => usdotNumbers.Contains(r.CompanyUSDOT))
                .GroupBy(r => r.CompanyUSDOT)
                .Select(g => new { USDOTNumber = g.Key, TotalReviews = g.Count(), RawAverage = g.Average(r => r.Rating) })
                .AsEnumerable()
                .ToDictionary(
                    x => x.USDOTNumber,
                    x => new CompanyRatingVM
                    {
                        TotalReviews = x.TotalReviews,
                        AverageRating = Math.Round(x.RawAverage, 1, MidpointRounding.AwayFromZero)
                    }
                );

            var pageItems = pageEntities.Select(companies =>
            {
                CompanyRatingVM rating;
                ratingsByUsdot.TryGetValue(companies.USDOTNumber, out rating);
                return new CompanyVM
                {
                    LegalName = companies.LegalName,
                    DoingBusinessAsName = companies.DoingBusinessAsName,
                    PhysicalAddressCity = companies.PhysicalAddressCity,
                    PhysicalAddressStateCode = companies.PhysicalAddressStateCode,
                    PhysicalAddressStreet = companies.PhysicalAddressStreet,
                    PhysicalAddressZipCode = companies.PhysicalAddressZipCode,
                    USDOTNumber = companies.USDOTNumber,
                    IccDocketNumberFirst = companies.IccDocketNumberFirst,
                    OfficeTelephoneNumber = companies.OfficeTelephoneNumber,
                    CellPhoneNumber = companies.CellPhoneNumber,
                    City = companies.PhysicalAddressCity,
                    StateCode = companies.PhysicalAddressStateCode,
                    TruckOrTractor = companies.TrucksAndTractors,
                    EntityType = companies.EntityType,
                    TotalNumberOfTrucks = companies.TotalNumberOfTrucks,
                    SortRelevance = companies.SortRelevance,
                    Latitude = companies.Latitude,
                    Longitude = companies.Longitude,
                    CompanyName = companies.CompanyName,
                    NNDriversGrandTotalInterstateAndIntrastate = companies.NNDriversGrandTotalInterstateAndIntrastate,
                    Status = companies.Status,
                    TotalReviews = rating != null ? rating.TotalReviews : 0,
                    AverageRating = rating != null ? rating.AverageRating : 0
                };
            }).ToList();

            var pagedResult = new PagedList<CompanyVM>(pageIndex, pageSize, totalCount, ps.se, ps.sortDirection);
            pagedResult.Items = pageItems;
            return pagedResult;

        }

        /// <summary>
        /// check filter values are null or not
        /// retuen  SearchFilterVM object
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <param name="filter3"></param>
        /// <param name="filter4"></param>
        /// <param name="filter5"></param>
        /// <param name="filter6"></param>
        /// <param name="filter7"></param>
        /// <param name="filter8"></param>
        /// <param name="filter9"></param>
        /// <returns></returns>
        public SearchFilterVM CheckFilterValuesNullOrNot(string filter1, string filter2, string filter3, string filter4, string filter5, string filter6, string filter7, string filter8, string filter9)
        {
            //create object
            SearchFilterVM values = new SearchFilterVM();
            if (!string.IsNullOrEmpty(filter1))
            {
                values = CheckFilterValueType(filter1, values);
            }
            if (!string.IsNullOrEmpty(filter2))
            {
                values = CheckFilterValueType(filter2, values);
            }
            if (!string.IsNullOrEmpty(filter3))
            {
                values = CheckFilterValueType(filter3, values);
            }
            if (!string.IsNullOrEmpty(filter4))
            {
                values = CheckFilterValueType(filter4, values);
            }
            if (!string.IsNullOrEmpty(filter5))
            {
                values = CheckFilterValueType(filter5, values);
            }
            if (!string.IsNullOrEmpty(filter6))
            {
                values = CheckFilterValueType(filter6, values);
            }
            if (!string.IsNullOrEmpty(filter7))
            {
                values = CheckFilterValueType(filter7, values);
            }
            if (!string.IsNullOrEmpty(filter8))
            {
                values = CheckFilterValueType(filter8, values);
            }
            if (!string.IsNullOrEmpty(filter9))
            {
                values = CheckFilterValueType(filter9, values);
            }
            return values;
        }

        /// <summary>
        /// Check filter type
        /// remove filter type name from the string 
        /// convart commasaperated value to list
        /// set values 
        /// return SearchFilterVM object with searchable values
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public SearchFilterVM CheckFilterValueType(string filter, SearchFilterVM searchFilterVM)
        {
            //create object
            //SearchFilterVM searchFilterVM = new SearchFilterVM();
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter.Contains("entity"))
                {
                    var entityValues = filter.Replace("entity-", "");
                    searchFilterVM.SelectedEntityTypes = entityValues.Split(',').Distinct().ToList();
                }
                if (filter.Contains("cargo"))
                {
                    var cargoValues = filter.Replace("cargo-", "");
                    searchFilterVM.SelectedCargoTypes = cargoValues.Split(',').ToList();
                }
                if (filter.Contains("trailer"))
                {
                    var trailerValues = filter.Replace("trailer-", "");
                    searchFilterVM.SelectedTrailerTypes = trailerValues.Split(',').ToList();
                }
                if (filter.Contains("driver"))
                {
                    var driverValues = filter.Replace("driver-", "");
                    searchFilterVM.SelectedDriverTypes = driverValues.Split(',').ToList();
                }
                if (filter.Contains("service"))
                {
                    var serviceValues = filter.Replace("service-", "");
                    searchFilterVM.SelectedServiceTypes = serviceValues.Split(',').ToList();
                }
                if (filter.Contains("truckortractor"))
                {
                    var truckortractorValues = filter.Replace("truckortractor-", "");
                    searchFilterVM.SelectedTruckOrTractor = truckortractorValues.Split(',').ToList();
                }
                if (filter.Contains("sortby"))
                {
                    var sortByValues = filter.Replace("sortby-", "");
                    searchFilterVM.SortBy = sortByValues;
                }
                if (filter.Contains("pos") || filter.Contains("pos_lst"))
                {
                    var values = "";
                    if (filter.Contains("pos_lst-"))
                    {
                        values = filter.Replace("pos_lst-", "");
                    }
                    else
                    {
                        //remove mapview name from string
                        values = filter.Replace("pos-", "");
                    }
                    //now split values with , and get into list
                    var allLatlongValues = values.Split(',').ToList();
                    //set values in our boundry alues object
                    searchFilterVM.BoundrieValues = new BoundrieValuesVM();
                    searchFilterVM.BoundrieValues.Southwest = new southwest();
                    searchFilterVM.BoundrieValues.Southwest.lat = Convert.ToDouble(allLatlongValues[0]);
                    searchFilterVM.BoundrieValues.Southwest.lng = Convert.ToDouble(allLatlongValues[1]);
                    //set northeast values
                    searchFilterVM.BoundrieValues.Northeast = new northeast();
                    searchFilterVM.BoundrieValues.Northeast.lat = Convert.ToDouble(allLatlongValues[2]);
                    searchFilterVM.BoundrieValues.Northeast.lng = Convert.ToDouble(allLatlongValues[3]);
                    if (allLatlongValues.Count > 4)
                    {
                        searchFilterVM.ZoomLevel = Convert.ToInt32(allLatlongValues[4]);
                    }
                }
            }
            return searchFilterVM;
        }

        /// <summary>
        /// Create a new Url with applied filter values after successfull Update Filter in city page and get companies
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <param name="filter3"></param>
        /// <param name="filter4"></param>
        /// <param name="filter5"></param>
        /// <returns></returns>
        public string CreateNewUrl(string state, string city, string SearchText, string filter1, string filter2, string filter3, string filter4, string filter5, string filter6, string filter7, string filter8, string filter9)
        {
            var url = "";
            //get city and state
            if (!string.IsNullOrEmpty(SearchText) && SearchText.Contains(","))
            {
                //get city
                city = SearchText.Split(',')[0];
                //get state
                state = SearchText.Split(',')[1].Trim();
            }
            if (!string.IsNullOrEmpty(state))
            {
                if (state.Contains(" "))
                {
                    state = state.Replace(" ", "-");
                }
                url += state;
            }
            if (!string.IsNullOrEmpty(city))
            {
                if (city.Contains(" "))
                {
                    city = city.Replace(" ", "-");
                }
                url += "/" + city;
            }
            if (!string.IsNullOrEmpty(filter1))
            {
                if (filter1.Contains(" "))
                {
                    filter1 = filter1.Replace(" ", "-");
                }
                url += "/" + filter1;
            }
            if (!string.IsNullOrEmpty(filter2))
            {
                if (filter2.Contains(" "))
                {
                    filter2 = filter2.Replace(" ", "-");
                }
                url += "/" + filter2;
            }
            if (!string.IsNullOrEmpty(filter3))
            {
                if (filter3.Contains(" "))
                {
                    filter3 = filter3.Replace(" ", "-");
                }
                url += "/" + filter3;
            }
            if (!string.IsNullOrEmpty(filter4))
            {
                if (filter4.Contains(" "))
                {
                    filter4 = filter4.Replace(" ", "-");
                }
                url += "/" + filter4;
            }
            if (!string.IsNullOrEmpty(filter5))
            {
                if (filter5.Contains(" "))
                {
                    filter5 = filter5.Replace(" ", "-");
                }
                url += "/" + filter5;
            }
            if (!string.IsNullOrEmpty(filter6))
            {
                if (filter6.Contains(" "))
                {
                    filter6 = filter6.Replace(" ", "-");
                }
                url += "/" + filter6;
            }
            if (!string.IsNullOrEmpty(filter7))
            {
                if (filter7.Contains(" "))
                {
                    filter7 = filter7.Replace(" ", "-");
                }
                url += "/" + filter7;
            }
            if (!string.IsNullOrEmpty(filter8))
            {
                if (filter8.Contains(" "))
                {
                    filter8 = filter8.Replace(" ", "-");
                }
                url += "/" + filter8;
            }
            if (!string.IsNullOrEmpty(filter9))
            {
                if (filter9.Contains(" "))
                {
                    filter9 = filter9.Replace(" ", "-");
                }
                url += "/" + filter9;
            }
            return url;
        }

        /// <summary>
        /// Get City Name From Company List
        /// this will cal when user enter USDOTNumber or MC Number in searchtext box from homepage
        /// </summary>
        /// <param name="companyList"></param>
        /// <returns></returns>
        public string GetCityNameFromCompanyList(List<CompanyVM> companyList)
        {
            var cityName = (from companies in companyList
                            select companies.PhysicalAddressCity).First().Replace(" ", "-");
            return cityName;
        }

        /// <summary>
        /// Get Statecode from company list
        /// this will cal when user enter USDOTNumber or MC Number in searchtext box from homepage
        /// </summary>
        /// <param name="companyList"></param>
        /// <returns></returns>
        public string GetStateCodeFromCompanyList(List<CompanyVM> companyList)
        {
            var stateCode = (from companies in companyList
                             select companies.PhysicalAddressStateCode).First();
            return stateCode;
        }

        /// <summary>
        /// after geting all values 
        /// set into boundry object
        /// </summary>
        /// <param name="latLongValues"></param>
        /// <returns></returns>
        public BoundrieValuesVM SetLatLongValues(string latLongValues)
        {
            //remove mapview name from string
            var values = latLongValues.Replace("pos-", "");
            //now split values with , and get into list
            var allLatlongValues = values.Split(',').ToList();
            //now we have created a viewModel for boundries 
            //set values in that
            BoundrieValuesVM boundrieValuesVM = new BoundrieValuesVM();
            boundrieValuesVM.Southwest = new southwest();
            boundrieValuesVM.Southwest.lat = Convert.ToDouble(allLatlongValues[0]);
            boundrieValuesVM.Southwest.lng = Convert.ToDouble(allLatlongValues[1]);
            boundrieValuesVM.Northeast = new northeast();
            boundrieValuesVM.Northeast.lat = Convert.ToDouble(allLatlongValues[2]);
            boundrieValuesVM.Northeast.lng = Convert.ToDouble(allLatlongValues[3]);
            //return boundry values
            return boundrieValuesVM;
        }

        /// <summary>
        /// Save The Business Details
        /// </summary>
        /// <param name="businessVM"></param>
        public void SaveAccountDetails(BusinessVM businessVM)
        {
            //Initialize transaction
            var transactionBusiness = db.Database.BeginTransaction();
            try
            {
                //Insert record in Business table
                Business business = new Business();

                //System generated random number to store in Business table and random generated number will be use to display on Email verify link
                Guid randomUniqueNumberGUID = Guid.NewGuid();

                business.USDOTNumber = businessVM.USDOTNumber;
                business.VerificationKey = randomUniqueNumberGUID;
                business.CreatedDate = DateTime.Now;
                business.UpdatedDate = DateTime.Now;

                business.WebsiteApproved = null;
                business.CommunicationApproved = true;
                business.PasswordSalt = PasswordGenerator.GetSalt();
                business.PasswordHash = PasswordGenerator.GetHashedPassword(business.PasswordSalt, businessVM.Password);
                business.EmailVerified = null;
                business.Website = "";
                business.BusinessContactEmail = null;
                business.JobContactEmail = null;
                business.JobContactPhone = null;
                business.JobContactSMS = null;
                business.NowHiring = false;
                business.ForgotPasswordKey = null;

                db.Businesses.Insert(db, business);

                //set replace values for email
                Dictionary<string, string> replacevalues = new Dictionary<string, string>();

                //now we have to replace all these values in contactus.html page
                replacevalues.Add("{VerifyLink}", Config.SiteURL + "verify/" + randomUniqueNumberGUID);

                string content = EmailUtility.GetTemplate(TemplateType.BusinessVerificationMail);

                //Apply try catch for knowing the exact which exception is fired and display error message based on exception
                try
                {
                    EmailUtility.Send(businessVM.CompanyEmailAddress, "Business Verification", AppSettings.FromEmail, content, replacevalues);
                }
                catch (Exception ex)
                {
                    throw new BusinessException("MailSendingFailed", "Email sending failed. Please try again later.");
                }
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
        /// Save The Business Details
        /// </summary>
        /// <param name="businessVM"></param>
        public void SaveCompanyBusinessDetails(BusinessVM businessVM)
        {
            //Initialize transaction
            var transactionBusiness = db.Database.BeginTransaction();
            try
            {
                //Get Business record by UsDotNumber and Update record 
                var getBusinessDetailstoUpdate = (from business in db.Businesses
                                                  where business.USDOTNumber == businessVM.USDOTNumber
                                                  select business).FirstOrDefault();

                getBusinessDetailstoUpdate.Website = businessVM.WebsiteName;
                getBusinessDetailstoUpdate.BusinessContactEmail = businessVM.BusinessContactEmail;
                getBusinessDetailstoUpdate.JobContactEmail = businessVM.JobContactEmail;
                getBusinessDetailstoUpdate.JobContactPhone = businessVM.JobContactPhone;
                getBusinessDetailstoUpdate.JobContactSMS = businessVM.JobContactSMS;

                //If at least one Listbox is select from Trailer type or Driver type then insert Now Hiring True
                //else insert false in now hiring field
                if (businessVM.SelectedTrailerTypes != null && businessVM.SelectedTrailerTypes.Count > 0 || businessVM.SelectedDriverTypes != null && businessVM.SelectedDriverTypes.Count > 0)
                {
                    getBusinessDetailstoUpdate.NowHiring = true;
                }
                else
                {
                    getBusinessDetailstoUpdate.NowHiring = false;
                }
                db.Businesses.UpdatePartial(db, getBusinessDetailstoUpdate, true, "Website", "BusinessContactEmail", "JobContactEmail", "JobContactPhone", "JobContactSMS", "NowHiring");


                var trailerTypes = (from list in db.TrailerTypes
                                    select list).ToList();
                //get list of servicetype from servicetype table
                var driverType = (from list in db.DriverTypes
                                  select list).ToList();



                var transportCompany = db.TransportCompanies.Where(x => x.USDOTNumber == businessVM.USDOTNumber).FirstOrDefault();

                if (businessVM.SelectedTrailerTypes != null && businessVM.SelectedTrailerTypes.Count > 0)
                {
                    db.Database.ExecuteSqlCommand("Delete From  TransportCompany_TrailerType where USDOTNumber=" + businessVM.USDOTNumber);
                    foreach (var item in businessVM.SelectedTrailerTypes)
                    {
                        transportCompany.TrailerTypes.Add(trailerTypes.Single(x => x.TrailerNumber == item));
                    }
                }
                else
                {
                    db.Database.ExecuteSqlCommand("Delete From  TransportCompany_TrailerType where USDOTNumber=" + businessVM.USDOTNumber);
                    //Delete from TransportCompany_TrailerTypes table
                }


                if (businessVM.SelectedDriverTypes != null && businessVM.SelectedDriverTypes.Count > 0)
                {
                    db.Database.ExecuteSqlCommand("Delete From  TransportCompany_DriverType where USDOTNumber=" + businessVM.USDOTNumber);

                    foreach (var item in businessVM.SelectedDriverTypes)
                    {

                        transportCompany.DriverTypes.Add(driverType.Single(x => x.DriverNumber == item));
                    }
                }
                else
                {
                    db.Database.ExecuteSqlCommand("Delete From  TransportCompany_DriverType where USDOTNumber=" + businessVM.USDOTNumber);
                    //Delete from TransportCompany_DriverTypes table
                }

                db.SaveChanges();
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
        /// Change Password for Business
        /// </summary>
        /// <param name="businessVM"></param>
        public void ChangePassword(BusinessVM businessVM)
        {

            var changePassword = (from business in db.Businesses
                                  where business.USDOTNumber == businessVM.USDOTNumber
                                  select business).SingleOrDefault();

            if (changePassword.PasswordHash != PasswordGenerator.GetHashedPassword(changePassword.PasswordSalt, businessVM.Password))
            {
                throw new BusinessException("400", "Invalid current password");
            }
            changePassword.USDOTNumber = businessVM.USDOTNumber;
            changePassword.PasswordHash = PasswordGenerator.GetHashedPassword(changePassword.PasswordSalt, businessVM.ConfirmPassword);
            db.Businesses.UpdatePartial(db, changePassword, true, "PasswordHash");
        }

        /// <summary>
        /// Get Companies Email address by USDOT number
        /// </summary>
        /// <param name="usdotnumber"></param>
        /// <returns></returns>
        public BusinessVM GetCompanyEmailAddress(int? usdotnumber)
        {
            var companyEmailAddress = (from company in db.TransportCompanies
                                       where company.USDOTNumber == usdotnumber
                                       select new BusinessVM
                                       {
                                           CompanyEmailAddress = company.EmailAddress,
                                           USDOTNumber = company.USDOTNumber,
                                           DoingBusinessAsName = company.DoingBusinessAsName,
                                           LegalName = company.LegalName
                                       }).FirstOrDefault();

            if (companyEmailAddress == null)
            {
                throw new HttpException(404, "Page not found");
            }

            return companyEmailAddress;
        }


        public BusinessVM GetBusinessDetailsByUSDOTNumber(int? usdotnumber)
        {
            var companyEmailAddress = (from company in db.TransportCompanies
                                       join business in db.Businesses on company.USDOTNumber equals business.USDOTNumber
                                       where company.USDOTNumber == usdotnumber
                                       select new BusinessVM
                                       {
                                           CompanyEmailAddress = company.EmailAddress,
                                           USDOTNumber = company.USDOTNumber,
                                           DoingBusinessAsName = company.DoingBusinessAsName,
                                           LegalName = company.LegalName,
                                           BusinessContactEmail = business.BusinessContactEmail,
                                           CommunicationApproved = business.CommunicationApproved,
                                           JobContactEmail = business.JobContactEmail,
                                           JobContactPhone = business.JobContactPhone,
                                           JobContactSMS = business.JobContactSMS,
                                           SelectedDriverTypes = company.DriverTypes.Select(x => x.DriverNumber).ToList(),
                                           SelectedTrailerTypes = company.TrailerTypes.Select(x => x.TrailerNumber).ToList(),
                                           WebsiteName = business.Website
                                       }).FirstOrDefault();

            if (companyEmailAddress == null)
            {
                throw new HttpException(404, "Page not found");
            }

            return companyEmailAddress;
        }

        /// <summary>
        /// Email verify when user click link from their email to verify business
        /// </summary>
        /// <param name="verifyLinkGUID"></param>
        public int VerifyEmail(Guid verifyLinkGUID)
        {
            try
            {
                var verify = (from business in db.Businesses.AsNoTracking()
                              where business.VerificationKey == verifyLinkGUID && business.EmailVerified == null
                              select business).FirstOrDefault();


                if (verify.EmailVerified == null)
                {
                    Business business = new Business();
                    business.USDOTNumber = verify.USDOTNumber;
                    business.EmailVerified = true;
                    business.VerificationKey = null;
                    business.UpdatedDate = DateTime.Now;

                    db.Businesses.UpdatePartial(db, business, true, "EmailVerified", "VerificationKey", "UpdatedDate");
                }

                return verify.USDOTNumber;
            }
            catch (Exception)
            {
                throw new HttpException(404, "Sorry, something went wrong. Please contact us for more details.");
            }
        }

        public CompanyVM GenerateURLAfterSaveBusiness(int uSDOTNumber)
        {
            var companyNameForUrl = "";

            var companyDetail = (from company in db.TransportCompanies
                                 where company.USDOTNumber == uSDOTNumber
                                 select new CompanyVM
                                 {
                                     CompanyEmailAddress = company.EmailAddress,
                                     USDOTNumber = company.USDOTNumber,
                                     DoingBusinessAsName = company.DoingBusinessAsName,
                                     LegalName = company.LegalName,
                                     City = company.PhysicalAddressCity,
                                     StateName = company.PhysicalAddressStateCode,
                                 }).FirstOrDefault();

            if (companyDetail.LegalName.Contains(" "))
            {
                companyNameForUrl = companyDetail.LegalName.Replace(" ", "-");
            }
            if (!string.IsNullOrEmpty(companyDetail.DoingBusinessAsName) && companyDetail.DoingBusinessAsName.Contains(" "))
            {
                companyNameForUrl = companyDetail.DoingBusinessAsName.Replace(" ", "-");
            }
            companyNameForUrl = companyNameForUrl.Replace("+", "-"); // Arkady
            companyNameForUrl = companyNameForUrl.Replace("&", "-"); // Arkady
            companyNameForUrl = companyNameForUrl.Replace(".-", "-"); // Arkady Sep 15 2019
            if (companyDetail.City.Contains(" "))
            {
                companyDetail.City = companyDetail.City.Replace(" ", "-");
            }

            companyDetail.companyNameForUrl = companyNameForUrl;

            return companyDetail;
        }

        /// <summary>
        /// Bind default checkbox list to show on UI for Pickup and Delivery
        /// </summary>
        /// <param name="location">location indicate it's pickup or delivery side</param>
        /// <param name="loadType">load type indicate it's type of load which used selected</param>
        /// <param name="LocationTypeId"> Location Type Id from pickup or delivery side</param>
        /// <returns></returns>
        public List<SpecialHandlingVM> GetCheckBoxList(string location, string loadType, int LocationTypeId)
        {
            if (location.ToLower() == "pickup")
            {
                return (from specialHandling in db.SpecialHandlings
                        where specialHandling.LocationType.Location == location && specialHandling.LocationType.Id == LocationTypeId && specialHandling.LoadType.Name == loadType
                        select new SpecialHandlingVM
                        {
                            Id = specialHandling.Id,
                            Name = specialHandling.Name,
                            Title = specialHandling.Title,
                            LocationTypeId = specialHandling.LocationTypeId
                        }).ToList();
            }
            else
            {
                return (from specialHandling in db.SpecialHandlings
                        where specialHandling.LocationType.Location == location && specialHandling.LocationType.Id == LocationTypeId && specialHandling.LoadType.Name == loadType
                        select new SpecialHandlingVM
                        {
                            Id = specialHandling.Id,
                            Name = specialHandling.Name,
                            Title = specialHandling.Title,
                            LocationTypeId = specialHandling.LocationTypeId
                        }).ToList();
            }
        }

        /// <summary>
        /// Bind Checkboxes list based on location type and load type when changing Location type from dropdown
        /// </summary>
        /// <param name="locationTypeId"></param>
        /// <param name="locationType"></param>
        /// <param name="loadType"></param>
        /// <returns></returns>
        public List<SpecialHandlingVM> GetCheckBoxListFromLocationType(int locationTypeId, string locationType, string loadType)
        {
            var specialHandlingList = (from specialHandling in db.SpecialHandlings
                                       where specialHandling.LocationType.Location == locationType && specialHandling.LocationType.Id == locationTypeId && specialHandling.LoadType.Name == loadType
                                       select new SpecialHandlingVM
                                       {
                                           Id = specialHandling.Id,
                                           Name = specialHandling.Name,
                                           Title = specialHandling.Title,
                                           LocationTypeId = specialHandling.LocationTypeId
                                       }).ToList();

            return specialHandlingList;
        }

        /// <summary>
        /// Dropdown for Load Class
        /// </summary>
        /// <returns></returns>
        public List<LoadClassVM> GetDropDownForLoadClass()
        {
            return (from loadclass in db.LoadClasses
                    select new LoadClassVM
                    {
                        Id = loadclass.Id,
                        Name = loadclass.Name
                    }).ToList();
        }
        /// <summary>
        /// Dropdown for LoadItemType
        /// </summary>
        /// <returns></returns>
        public List<LoadItemTypeVM> GetDropDownForLoadItemType()
        {
            return (from loadItemType in db.LoadItemTypes
                    select new LoadItemTypeVM
                    {
                        Id = loadItemType.Id,
                        LoadItemType = loadItemType.LoadItemType1
                    }).ToList();
        }

        /// <summary>
        /// Submit Get a quote details and Send Email based on User details match to those carriers
        /// </summary>
        /// <param name="getAQuoteVM"quote and load Details></param>
        public GetAQuoteVM submitGetAQuoteDetails(GetAQuoteVM getAQuoteVM)
        {
            //Initialize transaction
            var transactionGetAQuote = db.Database.BeginTransaction();
            try
            {
                Quote quoteDetails = new Quote();

                quoteDetails.CreatedDate = DateTime.Now;
                quoteDetails.PickupDate = getAQuoteVM.PickupDate;
                if (string.IsNullOrEmpty(getAQuoteVM.OriginURL))
                {
                    quoteDetails.OriginURL = Config.SiteURL;
                }
                else
                {
                    quoteDetails.OriginURL = getAQuoteVM.OriginURL;
                }
                quoteDetails.LoadTypeId = db.LoadTypes.Where(a => a.Name == getAQuoteVM.LoadType).Select(b => b.Id).FirstOrDefault();
                if (!string.IsNullOrEmpty(getAQuoteVM.PickupLocation) && !string.IsNullOrEmpty(getAQuoteVM.DeliveryLocation))
                {
                    quoteDetails.FromState = getAQuoteVM.PickupLocation.Split(',')[1].Trim();
                    quoteDetails.ToState = getAQuoteVM.DeliveryLocation.Split(',')[1].Trim();
                    quoteDetails.FromCity = getAQuoteVM.PickupLocation.Split(',')[0].Trim();
                    quoteDetails.ToCity = getAQuoteVM.DeliveryLocation.Split(',')[0].Trim();
                }
                quoteDetails.FromLocationTypeId = Convert.ToInt32(getAQuoteVM.PickupLocationType);
                quoteDetails.ToLocationTypeId = Convert.ToInt32(getAQuoteVM.DeliveryLocationType);
                quoteDetails.IsFlexible = getAQuoteVM.IsFlexible;
                if (!string.IsNullOrEmpty(getAQuoteVM.Temperature))
                {
                    quoteDetails.Temperature = Convert.ToDecimal(getAQuoteVM.Temperature);
                }
                else
                {
                    quoteDetails.Temperature = null;
                }
                quoteDetails.TemperatureId = getAQuoteVM.TemperatureId > 0 ? getAQuoteVM.TemperatureId : new Nullable<Int32>();
                quoteDetails.RefrigerationId = getAQuoteVM.RefrigerationId;
                quoteDetails.LoadDetailDescription = getAQuoteVM.LoadDetailsDescription;
                quoteDetails.ShipperCompanyName = getAQuoteVM.CompanyName;
                quoteDetails.ShipperFirstName = getAQuoteVM.FirstName;
                quoteDetails.ShipperLastName = getAQuoteVM.LastName;
                quoteDetails.ShipperEmail = getAQuoteVM.EmailAddress;
                quoteDetails.ShipperPhone = getAQuoteVM.Phone;

                //Inserted Id of Quote detail
                var quoteDetailId = db.Quotes.Insert(db, quoteDetails).Id;

                if (!string.IsNullOrEmpty(getAQuoteVM.selectedSpecialHandlingIds))
                {
                    //Create list of interger to store list of selected Pickup handlings id.
                    List<int> PickupSelectedHandlingList = new List<int>();
                    //Split comma separated selectedSpecialHandlingIds and store to list of pickup special handlings id
                    PickupSelectedHandlingList = getAQuoteVM.selectedSpecialHandlingIds.Split(',').Select(int.Parse).ToList();

                    //Iterate pickup special handling id to store one by one id into Quote Special Handlings Location table.
                    foreach (var pickupselectedHandlingId in PickupSelectedHandlingList)
                    {
                        Quote_SpecialHandling_Location quoteSpecialLocation = new Quote_SpecialHandling_Location();

                        quoteSpecialLocation.QuoteId = quoteDetailId;
                        quoteSpecialLocation.SpecialHandlingId = pickupselectedHandlingId;
                        db.Quote_SpecialHandling_Location.Insert(db, quoteSpecialLocation);
                    }
                }

                if (!string.IsNullOrEmpty(getAQuoteVM.selectedDeliverySpecialHandlingIds))
                {
                    //Create list of interger to store list of selected Deleivery handlings id.
                    List<int> DeliverySelectedHandlingList = new List<int>();
                    //Split comma separated selectedSpecialHandlingIds and store to list of pickup special handlings id
                    DeliverySelectedHandlingList = getAQuoteVM.selectedDeliverySpecialHandlingIds.Split(',').Select(int.Parse).ToList();

                    //Iterate delivery special handling id to store one by one id into Quote Special Handlings Location table.
                    foreach (var deliverySelectedHandlingId in DeliverySelectedHandlingList)
                    {
                        Quote_SpecialHandling_Location quoteSpecialLocation = new Quote_SpecialHandling_Location();

                        quoteSpecialLocation.QuoteId = quoteDetailId;
                        quoteSpecialLocation.SpecialHandlingId = deliverySelectedHandlingId;
                        db.Quote_SpecialHandling_Location.Insert(db, quoteSpecialLocation);
                    }
                }

                if (getAQuoteVM.ListOfLoadInformationVM != null)
                {
                    //Inser Details into Load table
                    foreach (var loadDetail in getAQuoteVM.ListOfLoadInformationVM)
                    {
                        Load loadDetails = new Load();

                        loadDetails.GoodsDescription = loadDetail.GoodDescription;
                        loadDetails.DimentionLength = loadDetail.DimentionLength;
                        loadDetails.DimentionWidth = loadDetail.DimentionWidth;
                        loadDetails.DimentionHeight = loadDetail.DimentionHeight;
                        loadDetails.NoOfItems = loadDetail.NumberOfItem;
                        loadDetails.LoadStatusTypeId = loadDetail.LoadStatusTypeId;
                        loadDetails.LoadItemTypeId = loadDetail.LoadItemTypeId;
                        loadDetails.TotalWeight = loadDetail.WeightPerItem;
                        loadDetails.LoadClassId = loadDetail.ClassTypeId;
                        loadDetails.IsHasmat_ = loadDetail.IsHazmat;
                        loadDetails.IsNonStackable_ = loadDetail.IsNonStackable;
                        loadDetails.LoadContainerTypeId = loadDetail.LoadContainerLengthId;
                        loadDetails.NoOfContainers = loadDetail.NoOfContainers;
                        loadDetails.LoadTruckTypeId = loadDetail.TruckTypeId;
                        loadDetails.LoadInfoId = loadDetail.LoadInfoId;

                        db.Loads.Insert(db, loadDetails);
                    }
                }
                else
                {
                    Load loadDetails = new Load();

                    loadDetails.GoodsDescription = getAQuoteVM.LoadInformationVM.GoodDescription;
                    loadDetails.DimentionLength = getAQuoteVM.LoadInformationVM.DimentionLength;
                    loadDetails.DimentionWidth = getAQuoteVM.LoadInformationVM.DimentionWidth;
                    loadDetails.DimentionHeight = getAQuoteVM.LoadInformationVM.DimentionHeight;
                    loadDetails.NoOfItems = getAQuoteVM.LoadInformationVM.NumberOfItem;
                    loadDetails.LoadStatusTypeId = getAQuoteVM.LoadInformationVM.LoadStatusTypeId;
                    loadDetails.LoadItemTypeId = getAQuoteVM.LoadInformationVM.LoadItemTypeId;
                    loadDetails.TotalWeight = getAQuoteVM.LoadInformationVM.WeightPerItem;
                    loadDetails.LoadClassId = getAQuoteVM.LoadInformationVM.ClassTypeId;
                    loadDetails.IsHasmat_ = getAQuoteVM.LoadInformationVM.IsHazmat;
                    loadDetails.IsNonStackable_ = getAQuoteVM.LoadInformationVM.IsNonStackable;
                    loadDetails.LoadContainerTypeId = getAQuoteVM.LoadInformationVM.LoadContainerLengthId;
                    loadDetails.NoOfContainers = getAQuoteVM.LoadInformationVM.NoOfContainers;
                    loadDetails.LoadTruckTypeId = getAQuoteVM.LoadInformationVM.TruckTypeId;
                    loadDetails.LoadInfoId = getAQuoteVM.LoadInformationVM.LoadInfoId;

                    db.Loads.Insert(db, loadDetails);
                }

                //Commit transaction
                transactionGetAQuote.Commit();
                //Create pdf programatically
                createQuoteDetailPDF(getAQuoteVM, quoteDetailId);
                getAQuoteVM.QuoteId = quoteDetailId;
                return getAQuoteVM;
            }
            catch (Exception ex)
            {
                //Rollback transaction
                transactionGetAQuote.Rollback();
                throw ex;
            }
        }

        /// <summary>
        /// Send Emails to Carrier based on quoted
        /// </summary>
        /// <param name="quoteId"></param>
        public void SendEmailsForQuote(int quoteId)
        {
            var quoteDetails = db.Quotes.Where(a => a.Id == quoteId).FirstOrDefault();

            var loadTypeId = quoteDetails.LoadTypeId;
            var fromState = quoteDetails.FromState.Trim();
            var toState = quoteDetails.ToState.Trim();

            //get count of mail sent to Carrierrs for check MaxQuotePerMonth
            var countOfMailSent = (from carriers in db.Carriers
                                   join carrientsentto in db.QuoteSents on carriers.Id equals carrientsentto.CarrierId
                                   join carriierLoad in db.Carrier_LoadType on carriers.Id equals carriierLoad.CarrierId
                                   join carriierStateFrom in db.Carrier_State_From on carriers.Id equals carriierStateFrom.CarrierId
                                   join carriierStateTo in db.Carrier_State_To on carriers.Id equals carriierStateTo.CarrierId
                                   where carriierLoad.LoadTypeID == loadTypeId && carriierStateFrom.StateCode == fromState && carriierStateTo.StateCode == toState && carriers.CarrierActive == true
                                   select carrientsentto.CarrierId).Count();

            //Get List of carrier whome to send mail based on Load Type, From and To are matched
            var listofsendEmailToCarrier = (from carrier in db.Carriers
                                            join carriierLoad in db.Carrier_LoadType on carrier.Id equals carriierLoad.CarrierId
                                            join carriierStateFrom in db.Carrier_State_From on carrier.Id equals carriierStateFrom.CarrierId
                                            join carriierStateTo in db.Carrier_State_To on carrier.Id equals carriierStateTo.CarrierId
                                            where (carriierLoad.LoadTypeID == loadTypeId && carriierStateFrom.StateCode == fromState && carriierStateTo.StateCode == toState && carrier.CarrierActive == true) &&
                                            (carrier.MaxQuotesPerMonth == null || carrier.MaxQuotesPerMonth >= countOfMailSent)
                                            select new
                                            {
                                                ContactEmail1 = carrier.ContactEmail1,
                                                ContactEmail2 = carrier.ContactEmail2,
                                                CarrierId = carrier.Id,
                                            }).ToList();

            //Get and create path of pdf from web directorey 
            var getaquotefilepath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/PDF"), "getaquote.pdf");
            //Create attachment of pdf files
            string[] attachmentPDF = new string[] { getaquotefilepath };

            //Create email subject as per new change
            var emailSubject = Config.SiteURL.EndsWith("/") ? Config.SiteURL.Substring(0, Config.SiteURL.Length - 1) : Config.SiteURL;
            emailSubject += " - " + "Quote #" + quoteId.ToString() + " - Details";

            //Replace value for email template
            Dictionary<string, string> replacevalues = new Dictionary<string, string>();
            replacevalues.Add("{emailHeader}", emailSubject);

            //Before sending emails to any carrier first email should be sent to quote@truckcarrierhub.com. quote will be receive email every time if any carrier exist or not.
            EmailUtility.Send("quotes@truckcarrierhub.com", emailSubject, AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.QuoteDettail), replacevalues, attachmentPDF);

            //iterate loop for send email one by one to carriers
            //Before sending mail to each and every carrier we have set fix delay time to adjust for mail not sending to Spam folder.
            foreach (var item in listofsendEmailToCarrier)
            {
                try
                {
                    //Sleep for 60 second as per client requirement ("I’m not going to send too many emails. You may pause 60 seconds between emails.") 
                    //But as per discuss with Amit sir, Get second's value from web.config file and set into sleep method while sending mail.//15 in set in web config currently
                    var SendMailSleep = Convert.ToInt32(Config.GetValue("SendMailSleep"));
                    var SendMailSleepInMilisecond = (SendMailSleep * 1000);
                    Thread.Sleep(SendMailSleepInMilisecond);

                    //Send email to carrier
                    EmailUtility.Send(item.ContactEmail1, emailSubject, AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.QuoteDettail), replacevalues, attachmentPDF);

                    //if mail is sent then log it into EmailSent table with EmailID and CarrierId
                    var quoteSent = new QuoteSent()
                    {
                        CarrierId = item.CarrierId,
                        QuoteID = quoteId,
                        SentDate = DateTime.Now
                    };
                    db.QuoteSents.Insert(db, quoteSent);
                }
                catch (Exception ex)
                {
                    //Log mail failed exception
                    AppLogger.Instance.Log("Mail Sending Fail:  QuoteId: " + quoteId + ", Company Email Address: " + item.ContactEmail1);
                    AppLogger.Instance.Log(ex);

                }
            }
        }

        /// <summary>
        /// Create PDF programattically to send on email to carriers based on quote
        /// </summary>
        /// <param name="getAQuoteVM"></param>
        /// <param name="quoteDetailId"></param>
        private void createQuoteDetailPDF(GetAQuoteVM getAQuoteVM, int quoteDetailId)
        {
            var doc1 = new Document();
            //use a variable to let my code fit across the page...

            //string path = Server.MapPath("PDFs");
            //Get and create path of pdf in web directorey 
            var newpaths = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/PDF"), "getaquote.pdf");

            //create PDF path check if directory is exist or not if not exist then creat directory else use same directory if already exist.
            var path = Config.SitePath + "PDF";
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
                System.IO.Directory.CreateDirectory(path);

            PdfWriter.GetInstance(doc1, new FileStream(newpaths, FileMode.Create));

            doc1.Open();

            Font header = new Font(Font.FontFamily.TIMES_ROMAN, 15f, Font.BOLD, BaseColor.BLACK);
            Font headerofColumn = new Font(Font.FontFamily.TIMES_ROMAN, 12f, Font.BOLD, BaseColor.BLACK);

            BaseFont bf = BaseFont.CreateFont(
                        BaseFont.TIMES_ROMAN,
                        BaseFont.CP1252,
                        BaseFont.EMBEDDED);
            Font font = new Font(bf, 18);

            PdfPTable quoteTable = new PdfPTable(2);
            quoteTable.DefaultCell.Padding = 4;

            //create pdf title to replace in pdf document as per new change
            var pdfTitle = Config.SiteURL.EndsWith("/") ? Config.SiteURL.Substring(0, Config.SiteURL.Length - 1) : Config.SiteURL;
            pdfTitle += " - " + "Quote #" + quoteDetailId.ToString() + " - Details";

            PdfPCell quoteHeaderCell = new PdfPCell(new Phrase(pdfTitle, font));
            PdfPCell shipperInfoHeaderCell = new PdfPCell(new Phrase("Shipper Information", header));

            quoteHeaderCell.Border = Rectangle.NO_BORDER;
            quoteHeaderCell.Colspan = 2;
            quoteHeaderCell.Padding = 5;
            shipperInfoHeaderCell.Colspan = 2;
            quoteHeaderCell.HorizontalAlignment = 1;
            shipperInfoHeaderCell.Padding = 5;
            shipperInfoHeaderCell.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right

            quoteTable.AddCell(quoteHeaderCell);
            quoteTable.AddCell(shipperInfoHeaderCell);

            quoteTable.AddCell(new Phrase("First Name", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.FirstName);

            quoteTable.AddCell(new Phrase("Last Name", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.LastName);

            quoteTable.AddCell(new Phrase("Email Address", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.EmailAddress);

            quoteTable.AddCell(new Phrase("Phone", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.Phone);

            quoteTable.AddCell(new Phrase("Company Name", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.CompanyName);

            quoteTable.AddCell(new Phrase("Pickup City", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.PickupLocation);

            quoteTable.AddCell(new Phrase("Delivery City", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.DeliveryLocation);

            quoteTable.AddCell(new Phrase("Pickup Location Type", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.PickupLocationTypeValue);

            quoteTable.AddCell(new Phrase("Pickup Special Handlings", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.selectedSpecialHandlingValues);

            quoteTable.AddCell(new Phrase("Delivery Location Type", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.DeliveryLocationTypeValue);

            quoteTable.AddCell(new Phrase("Delivery Special Handlings", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.selectedDeliverySpecialHandlingValue);

            quoteTable.AddCell(new Phrase("Load Type", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.LoadType.ToString());

            quoteTable.AddCell(new Phrase("Pickup Date", headerofColumn));
            quoteTable.AddCell(getAQuoteVM.StringPickupDate);

            if (getAQuoteVM.LoadType != "LTL")
            {
                quoteTable.AddCell(new Phrase("I am flexible", headerofColumn));
                if (getAQuoteVM.IsFlexible)
                {
                    quoteTable.AddCell("Yes");
                }
                else
                {
                    quoteTable.AddCell("No");
                }
                quoteTable.AddCell(new Phrase("Detailed Load Description", headerofColumn));
                quoteTable.AddCell(getAQuoteVM.LoadDetailsDescription);
            }

            if (getAQuoteVM.LoadType != "Flatbed" && getAQuoteVM.LoadType != "Container")
            {
                quoteTable.AddCell(new Phrase("Refrigeration / Temp control", headerofColumn));
                quoteTable.AddCell(getAQuoteVM.RefrigerationType);
                if (getAQuoteVM.LoadType != "LTL")
                {
                    if (getAQuoteVM.RefrigerationType == "Exact temperature")
                    {
                        quoteTable.AddCell(new Phrase("Temperature", headerofColumn));
                        quoteTable.AddCell(getAQuoteVM.Temperature + " " + getAQuoteVM.TemperatureType);
                    }
                }
            }

            quoteTable.TotalWidth = 550f;
            quoteTable.LockedWidth = true;
            //relative col widths in proportions - 1/3 and 2/3
            float[] quotewidths = new float[] { 4f, 6f };
            quoteTable.SetWidths(quotewidths);

            doc1.Add(quoteTable);

            if (getAQuoteVM.ListOfLoadInformationVM != null)
            {
                PdfPTable loadTable = new PdfPTable(7);

                loadTable.DefaultCell.Padding = 5;
                PdfPCell balckcell = new PdfPCell(new Phrase(" "));
                balckcell.Border = Rectangle.NO_BORDER;
                balckcell.Colspan = 7;

                PdfPCell LoadInfoHeaderCell = new PdfPCell(new Phrase("Load Information", header));
                LoadInfoHeaderCell.Border = Rectangle.NO_BORDER;
                LoadInfoHeaderCell.Colspan = 7;
                LoadInfoHeaderCell.HorizontalAlignment = 0;
                LoadInfoHeaderCell.Padding = 5;

                loadTable.TotalWidth = 550f;
                //fix the absolute width of the table
                loadTable.LockedWidth = true;
                float[] widths = new float[] { 5f, 2f, 3f, 2f, 2f, 2f, 2f };
                loadTable.SetWidths(widths);
                loadTable.AddCell(balckcell);
                loadTable.AddCell(LoadInfoHeaderCell);

                loadTable.AddCell(new Phrase("Goods description", headerofColumn));
                loadTable.AddCell(new Phrase("Number of items", headerofColumn));
                loadTable.AddCell(new Phrase("Dimention", headerofColumn));
                loadTable.AddCell(new Phrase("Weight Per Item", headerofColumn));
                loadTable.AddCell(new Phrase("Class (USA)", headerofColumn));
                loadTable.AddCell(new Phrase("Hazmat?", headerofColumn));
                loadTable.AddCell(new Phrase("Non-Stackable?", headerofColumn));
                foreach (var loadInformation in getAQuoteVM.ListOfLoadInformationVM)
                {
                    loadTable.AddCell(loadInformation.GoodDescription);
                    loadTable.AddCell(loadInformation.NumberOfItem.ToString() + " " + loadInformation.LoadItemType);
                    loadTable.AddCell(loadInformation.DimentionLength.ToString() + " " + loadInformation.DimentionWidth.ToString() + " " + loadInformation.DimentionHeight.ToString() + " IN");
                    loadTable.AddCell(loadInformation.WeightPerItem.ToString() + " LB");
                    loadTable.AddCell(loadInformation.ClassType);
                    if (loadInformation.IsHazmat)
                    {
                        loadTable.AddCell("Yes");
                    }
                    else
                    {
                        loadTable.AddCell("No");
                    }
                    if (loadInformation.IsNonStackable)
                    {
                        loadTable.AddCell("Yes");
                    }
                    else
                    {
                        loadTable.AddCell("No");
                    }
                }
                doc1.Add(loadTable);
            }
            else
            {
                if (getAQuoteVM.LoadType == "FTL/Rail")
                {
                    PdfPTable loadTable = new PdfPTable(6);
                    loadTable.DefaultCell.Padding = 5;

                    PdfPCell LoadInfoHeaderCell = new PdfPCell(new Phrase("Load Information", header));
                    LoadInfoHeaderCell.Colspan = 6;
                    LoadInfoHeaderCell.HorizontalAlignment = 0;
                    LoadInfoHeaderCell.Border = Rectangle.NO_BORDER;
                    loadTable.TotalWidth = 550f;
                    //fix the absolute width of the table
                    loadTable.LockedWidth = true;
                    float[] widths = new float[] { 5f, 2f, 3f, 2f, 2f, 2f };
                    loadTable.SetWidths(widths);
                    loadTable.AddCell(LoadInfoHeaderCell);

                    loadTable.AddCell(new Phrase("Goods description", headerofColumn));
                    loadTable.AddCell(new Phrase("Number of items", headerofColumn));
                    loadTable.AddCell(new Phrase("Total weight", headerofColumn));
                    loadTable.AddCell(new Phrase("Truck Type", headerofColumn));
                    loadTable.AddCell(new Phrase("Load Info", headerofColumn));
                    loadTable.AddCell(new Phrase("Hazmat?", headerofColumn));

                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.GoodDescription);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.NumberOfItem.ToString() + " " + getAQuoteVM.LoadInformationVM.LoadItemType);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.WeightPerItem.ToString() + " LB");
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.TruckType);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.LoadInfo);
                    if (getAQuoteVM.LoadInformationVM.IsHazmat)
                    {
                        loadTable.AddCell("Yes");
                    }
                    else
                    {
                        loadTable.AddCell("No");
                    }
                    doc1.Add(loadTable);
                }
                else if (getAQuoteVM.LoadType == "Container")
                {
                    PdfPTable loadTable = new PdfPTable(5);
                    loadTable.DefaultCell.Padding = 5;
                    PdfPCell LoadInfoHeaderCell = new PdfPCell(new Phrase("Load Information", header));
                    LoadInfoHeaderCell.Colspan = 5;
                    LoadInfoHeaderCell.HorizontalAlignment = 0;
                    LoadInfoHeaderCell.Border = Rectangle.NO_BORDER;

                    loadTable.TotalWidth = 550f;
                    //fix the absolute width of the table
                    loadTable.LockedWidth = true;
                    float[] widths = new float[] { 5f, 2f, 3f, 2f, 2f };
                    loadTable.SetWidths(widths);
                    loadTable.AddCell(LoadInfoHeaderCell);

                    loadTable.AddCell(new Phrase("Goods description", headerofColumn));
                    loadTable.AddCell(new Phrase("Type", headerofColumn));
                    loadTable.AddCell(new Phrase("# AND LENGTH OF CONTAINERS", headerofColumn));
                    loadTable.AddCell(new Phrase("Total Weight Per Container", headerofColumn));
                    loadTable.AddCell(new Phrase("Hazmat?", headerofColumn));

                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.GoodDescription);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.LoadStatusType);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.NoOfContainers.ToString() + " " + getAQuoteVM.LoadInformationVM.LoadContainerLength);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.WeightPerItem.ToString());
                    if (getAQuoteVM.LoadInformationVM.IsHazmat)
                    {
                        loadTable.AddCell("Yes");
                    }
                    else
                    {
                        loadTable.AddCell("No");
                    }
                    doc1.Add(loadTable);
                }
                else
                {
                    PdfPTable loadTable = new PdfPTable(6);
                    loadTable.DefaultCell.Padding = 5;
                    PdfPCell LoadInfoHeaderCell = new PdfPCell(new Phrase("Load Information", header));
                    LoadInfoHeaderCell.Colspan = 6;
                    LoadInfoHeaderCell.HorizontalAlignment = 0;
                    LoadInfoHeaderCell.Border = Rectangle.NO_BORDER;
                    loadTable.TotalWidth = 550f;
                    //fix the absolute width of the table
                    loadTable.LockedWidth = true;
                    float[] widths = new float[] { 5f, 2f, 3f, 2f, 2f, 2f };
                    loadTable.SetWidths(widths);
                    loadTable.AddCell(LoadInfoHeaderCell);

                    loadTable.AddCell(new Phrase("Goods description", headerofColumn));
                    loadTable.AddCell(new Phrase("Weight", headerofColumn));
                    loadTable.AddCell(new Phrase("Dimention", headerofColumn));
                    loadTable.AddCell(new Phrase("Truck Type", headerofColumn));
                    loadTable.AddCell(new Phrase("Hazmat?", headerofColumn));
                    loadTable.AddCell(new Phrase("Non-Stackable?", headerofColumn));

                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.GoodDescription);
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.WeightPerItem.ToString() + " LB");
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.DimentionLength.ToString() + " " + getAQuoteVM.LoadInformationVM.DimentionWidth.ToString() + " " + getAQuoteVM.LoadInformationVM.DimentionHeight.ToString() + " IN");
                    loadTable.AddCell(getAQuoteVM.LoadInformationVM.TruckType);
                    if (getAQuoteVM.LoadInformationVM.IsHazmat)
                    {
                        loadTable.AddCell("Yes");
                    }
                    else
                    {
                        loadTable.AddCell("No");
                    }
                    if (getAQuoteVM.LoadInformationVM.IsNonStackable)
                    {
                        loadTable.AddCell("Yes");
                    }
                    else
                    {
                        loadTable.AddCell("No");
                    }
                    doc1.Add(loadTable);
                }
            }

            doc1.Close();
            doc1.Dispose();
        }

        /// <summary>
        /// Dropdown for Load Container Type
        /// </summary>
        /// <returns></returns>
        public List<LoadContainerTypeVM> GetDropDownForLoadContainerType()
        {
            return (from loadContainerType in db.LoadContainerTypes
                    select new LoadContainerTypeVM
                    {
                        Id = loadContainerType.Id,
                        StatusType = loadContainerType.StatusType
                    }).ToList();
        }

        /// <summary>
        /// Dropdown for Container Length
        /// </summary>
        /// <returns></returns>
        public List<LoadContainerLengthVM> GetDropDownForLoadContainerLength()
        {
            return (from loadContainerLength in db.LoadContainerLengths
                    select new LoadContainerLengthVM
                    {
                        Id = loadContainerLength.Id,
                        LengthOfContainer = loadContainerLength.LengthOfContainer
                    }).ToList();
        }

        /// <summary>
        /// Dropdown for RefrigerationType
        /// </summary>
        /// <param name="loadType"></param>
        /// <returns></returns>
        public List<RefrigerationVM> GetDropDownForRefrigerationType(string loadType)
        {
            return (from refrigerationType in db.QuoteRefrigerations
                    where refrigerationType.LoadType.Name == loadType
                    select new RefrigerationVM
                    {
                        Id = refrigerationType.Id,
                        RefrigerationType = refrigerationType.RefrigerationType,
                    }).ToList();
        }
        /// <summary>
        /// Dropdown for Truck Type
        /// </summary>
        /// <param name="loadType"></param>
        /// <returns></returns>
        public List<LoadTruckTypeVM> GetDropDownForTruckType(string loadType)
        {
            return (from truckType in db.LoadTruckTypes
                    where truckType.LoadType.Name == loadType
                    select new LoadTruckTypeVM
                    {
                        Id = truckType.Id,
                        TruckType = truckType.TruckType,
                    }).ToList();
        }

        /// <summary>
        /// Dropdown for LoadInfo
        /// </summary>
        /// <returns></returns>
        public List<LoadInfoVM> GetDropDownForLoadInfo()
        {
            return (from loadInfo in db.LoadInfoes
                    select new LoadInfoVM
                    {
                        Id = loadInfo.Id,
                        LoadInfoType = loadInfo.LoadInfoType,
                    }).ToList();
        }

        /// <summary>
        /// Dropdown for Temperature Type
        /// </summary>
        /// <returns></returns>
        public List<TemperatureVM> GetDropdownForTemperatureType()
        {
            return (from temperatureType in db.QuoteTemperatures
                    select new TemperatureVM
                    {
                        Id = temperatureType.Id,
                        TemperatureType = temperatureType.TemperatureType,
                    }).ToList();
        }

        /// <summary>
        /// To get outbound banner for Homepage.
        /// </summary>
        /// <param name="pageLevel"></param>
        /// <returns>Outbound Banner</returns>
        public OutboundBannerDataModel GetOutboundBanner(byte pageLevel)
        {
            // Query the database to retrieve an outbound for homepage.

            return (from outboundBanner in db.OutboundBanners
                    where outboundBanner.PageLevel == pageLevel
                    select new OutboundBannerDataModel
                    {
                        Id = outboundBanner.Id,
                        PageLevel = (OutboundBannerPageLevelEnum)outboundBanner.PageLevel,
                        IsShow = outboundBanner.IsShow,
                        OriginalFileName = outboundBanner.OriginalFileName,
                        FileName = outboundBanner.FileName,
                        URL = outboundBanner.URL,
                        IsFollow = outboundBanner.IsFollow,
                        AltText = outboundBanner.AltText,
                        TitleText = outboundBanner.TitleText,
                    }).FirstOrDefault();
        }


        public int GetNumberOfWordsAllowedByAdmin()
        {
            return db.Database.SqlQuery<int>("select NumberOfWords from Admin").FirstOrDefault();
        }

        #region Company Reviews

        /// <summary>
        /// To get the company ratings and review count using USDOTNumber.
        /// </summary>
        /// <param name="usdotnumber"></param>
        /// <returns></returns>
        public CompanyRatingVM GetCompanyRating(int usdotnumber)
        {
            var reviews = db.Reviews
                .Where(r => r.CompanyUSDOT == usdotnumber);

            int totalReviews = reviews.Count();

            double averageRating = 0;
            if (totalReviews > 0)
            {
                var rawAverage = reviews.Average(r => r.Rating);
                averageRating = Math.Round(rawAverage, 1, MidpointRounding.AwayFromZero);
            }

            return new CompanyRatingVM
            {
                TotalReviews = totalReviews,
                AverageRating = averageRating
            };
        }

        /// <summary>
        /// Adds a new company review from a reviewer.
        /// Validates the logged-in user, prevents duplicate/self-reviews, 
        /// saves the review, and sends notifications (admin + company).
        /// </summary>
        /// <param name="addCompanyReviewVM">Review details submitted by the user</param>
        /// <exception cref="BusinessException"></exception>
        public void AddCompanyReview(AddCompanyReviewVM addCompanyReviewVM)
        {
            // Get logged-in user from authentication service
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            // Check if this reviewer already reviewed the same company
            var existingReview = db.Reviews.FirstOrDefault(r => r.CompanyUSDOT == addCompanyReviewVM.CompanyUSDOT && r.ReviewerUSDOT == addCompanyReviewVM.ReviewerUSDOT);

            // Validation rules
            if (loggedInUser == null || loggedInUser.USDOTNumber == null) throw new BusinessException("Unauthorized", "You must be logged in to add a review.");

            if (addCompanyReviewVM.CompanyUSDOT == (int)loggedInUser.USDOTNumber) throw new BusinessException("Forbidden", "You cannot review your own company.");

            if (existingReview != null && addCompanyReviewVM.ReviewId == null) throw new BusinessException("DouplicateReview", "You already reviewed this company. Please update your review instead.");

            try
            {
                if (addCompanyReviewVM.ReviewId == null)
                {
                    // Create new review entity
                    var review = new Review
                    {
                        CompanyUSDOT = addCompanyReviewVM.CompanyUSDOT,
                        ReviewerUSDOT = addCompanyReviewVM.ReviewerUSDOT,
                        Rating = addCompanyReviewVM.Rating,
                        Comment = addCompanyReviewVM.Comment,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    // Insert and persist review
                    db.Reviews.Insert(db, review);
                    db.SaveChanges();


                    // Fetch company and reviewer info for email notifications
                    var reviewedCompany = db.TransportCompanies.FirstOrDefault(c => c.USDOTNumber == addCompanyReviewVM.CompanyUSDOT);
                    var reviewerCompany = db.TransportCompanies.FirstOrDefault(c => c.USDOTNumber == addCompanyReviewVM.ReviewerUSDOT);

                    //  Admin Notification Email
                    var adminEmailHeader = $"USDOT {addCompanyReviewVM.ReviewerUSDOT} posted {addCompanyReviewVM.Rating}-star review to USDOT {addCompanyReviewVM.CompanyUSDOT}";

                    var adminReplace = new Dictionary<string, string>
                    {
                        { "{ReviewerUSDOT}", addCompanyReviewVM.ReviewerUSDOT.ToString() },
                        { "{CompanyUSDOT}", addCompanyReviewVM.CompanyUSDOT.ToString() },
                        { "{Rating}", addCompanyReviewVM.Rating.ToString() },
                        { "{emailHeader}", adminEmailHeader }
                    };

                    var adminEmail = Config.GetValue("AdminNotificationEmail");

                    EmailUtility.Send(
                        adminEmail,
                        adminEmailHeader,
                        AppSettings.FromEmail,
                        EmailUtility.GetTemplate(TemplateType.NewReviewAdminNotificationMail),
                        adminReplace
                    );

                    // Reviewed Company Notification
                    if (!string.IsNullOrEmpty(reviewedCompany.CompanyRepresentativeOne) && !string.IsNullOrEmpty(reviewedCompany.EmailAddress))
                    {
                        var companyEmailHeader = $"You got {addCompanyReviewVM.Rating}-star review from {reviewerCompany.CompanyName} (USDOT {addCompanyReviewVM.ReviewerUSDOT})";
                        var companyPageUrl = $"/{reviewedCompany.PhysicalAddressStateCode}/USDOT-{reviewedCompany.USDOTNumber}?reviewId={review.Id}&open=reviews";
                        var encodedReturnUrl = HttpUtility.UrlEncode(companyPageUrl);

                        var replyLink = $"{Config.SiteURL}login?returnUrl={encodedReturnUrl}";


                        var companyReplace = new Dictionary<string, string>
                        {
                            { "{CompanyName}", reviewedCompany?.CompanyName ?? "Company" },
                            { "{ReviewerName}", reviewerCompany?.CompanyName ?? "Reviewer" },
                            { "{ReviewerUSDOT}", addCompanyReviewVM.ReviewerUSDOT.ToString() },
                            { "{Rating}", addCompanyReviewVM.Rating.ToString() },
                            { "{ReviewComment}", addCompanyReviewVM.Comment ?? string.Empty },
                            { "{ReplyLink}", replyLink },
                            { "{emailHeader}", companyEmailHeader }
                        };

                        EmailUtility.Send(
                            reviewedCompany.EmailAddress,
                            companyEmailHeader,
                            AppSettings.FromEmail,
                            EmailUtility.GetTemplate(TemplateType.NewReviewCompanyNotificationMail),
                            companyReplace
                        );
                    }


                }
                else
                {
                    UpdateCompanyReview((int)addCompanyReviewVM.ReviewId, addCompanyReviewVM);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// To get the details of a review by its Id.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public AddCompanyReviewVM GetReviewDetailsById(int reviewId)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            if (loggedInUser == null || loggedInUser.USDOTNumber == null) throw new BusinessException("", "You must be logged in to add a review.");

            var review = db.Reviews.FirstOrDefault(r => r.Id == reviewId && r.ReviewerUSDOT == loggedInUser.USDOTNumber);

            if (review == null)
                throw new BusinessException("", "Review not found.");

            var data = GetCompanyandReviewerName(review.CompanyUSDOT, review.Id, 0);


            return new AddCompanyReviewVM
            {
                ReviewId = reviewId,
                CompanyUSDOT = review.CompanyUSDOT,
                ReviewerUSDOT = review.ReviewerUSDOT,
                Comment = review.Comment,
                Rating = review.Rating,
                CompanyName = data.CompanyName,
                ReviewerName = data.ReviewerName
            };

        }

        /// <summary>
        /// To updates an existing company review.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="addCompanyReviewVM"></param>
        /// <exception cref="BusinessException"></exception>
        public void UpdateCompanyReview(int reviewId, AddCompanyReviewVM addCompanyReviewVM)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            if (loggedInUser == null || loggedInUser.USDOTNumber == null) throw new BusinessException("401", "You must be logged in to add a review.");

            if (addCompanyReviewVM.CompanyUSDOT == (int)loggedInUser.USDOTNumber) throw new BusinessException("400", "You cannot review your own company.");

            try
            {
                var review = db.Reviews.FirstOrDefault(r => r.Id == reviewId && r.ReviewerUSDOT == loggedInUser.USDOTNumber);

                if (review == null)
                    throw new BusinessException("404", "Review not found or you are not authorized to update it.");

                review.Rating = addCompanyReviewVM.Rating;
                review.Comment = addCompanyReviewVM.Comment?.Trim();
                review.UpdatedDate = DateTime.Now;

                db.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// To get reviews (and replies, if available) for a given company,
        /// builds a summary including average rating, rating breakdown, and sorted reviews.
        /// </summary>
        /// <param name="companyUSDOT"></param>
        /// <param name="sortOption"></param>
        /// <returns></returns>
        public CompanyReviewSummaryVM GetReviewsList(int companyUSDOT, int sortOption = 0)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            // Get company info
            var company = db.TransportCompanies
                           .Where(c => c.USDOTNumber == companyUSDOT)
                           .Select(c => new { c.CompanyName })
                           .FirstOrDefault();

            // Get reviews with replies
            var reviewsWithReplies = (from r in db.Reviews
                                      join b in db.Businesses on r.ReviewerUSDOT equals b.USDOTNumber
                                      join tc in db.TransportCompanies on r.ReviewerUSDOT equals tc.USDOTNumber
                                      join rr in db.ReviewReplies on r.Id equals rr.ReviewId into replyGroup
                                      from rr in replyGroup.DefaultIfEmpty()
                                      where r.CompanyUSDOT == companyUSDOT
                                      select new ReviewWithReplyVM
                                      {
                                          ReviewId = r.Id,
                                          ReponseId = rr.Id == null ? 0 : rr.Id,
                                          Rating = r.Rating,
                                          Comment = r.Comment,
                                          CreatedDate = (DateTime?)r.CreatedDate,
                                          UpdatedDate = (DateTime?)r.UpdatedDate,
                                          ReviewerUSDOT = b.USDOTNumber,
                                          ReviewerName = tc.LegalName,
                                          ReplyText = rr.ReplyText,
                                          ReplyCreatedDate = (DateTime?)rr.CreatedDate,
                                          ReplyUpdateDate = (DateTime?)rr.UpdatedDate,
                                      }).ToList();

            foreach (var review in reviewsWithReplies)
            {
                review.CanEdit = loggedInUser != null && review.ReviewerUSDOT == loggedInUser.USDOTNumber;
                review.CanReply = loggedInUser != null && companyUSDOT == loggedInUser.USDOTNumber;

                // Pick the correct date (UpdatedDate if available, otherwise CreatedDate)
                var dateToCompare = review.UpdatedDate ?? review.CreatedDate;
                if (dateToCompare != null)
                {
                    review.UpdatedOn = GetHumanReadableDateDiff(dateToCompare.Value);
                }

                // Pick the correct date (UpdatedDate if available, otherwise CreatedDate)
                if (review.ReplyUpdateDate != null || review.ReplyCreatedDate != null)
                {
                    var replyDateToCompare = review.ReplyUpdateDate ?? review.ReplyCreatedDate;
                    if (replyDateToCompare != null)
                    {
                        review.ReplyUpdatedOn = GetHumanReadableDateDiff(replyDateToCompare.Value);
                    }
                }
            }

            if (!reviewsWithReplies.Any())
            {
                return new CompanyReviewSummaryVM
                {
                    CompanyUSDOT = companyUSDOT,
                    CompanyName = company?.CompanyName ?? "Unknown Company",
                    AverageRating = 0,
                    TotalReviews = 0,
                    RatingsBreakdown = new Dictionary<int, int>
                    {
                        { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
                    },
                    Reviews = new List<ReviewWithReplyVM>()
                };
            }

            // Ratings breakdown
            var ratingsBreakdown = reviewsWithReplies
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all rating levels exist
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingsBreakdown.ContainsKey(i))
                    ratingsBreakdown[i] = 0;
            }

            // Apply sorting
            IEnumerable<ReviewWithReplyVM> sortedReviews;
            switch (sortOption)
            {
                case 1: // Highest Rating
                    sortedReviews = reviewsWithReplies.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedDate);
                    break;

                case 2: // Lowest Rating
                    sortedReviews = reviewsWithReplies.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedDate);
                    break;

                default: // Newest
                    sortedReviews = reviewsWithReplies.OrderByDescending(r => r.CreatedDate);
                    break;
            }

            // CanWriteReview logic
            bool canWrite = true;
            if (loggedInUser != null)
            {
                // Owner can not review own company and if user has reviewed a company once thet can not add second review.
                if (loggedInUser.USDOTNumber == companyUSDOT || reviewsWithReplies.Any(r => r.ReviewerUSDOT == loggedInUser.USDOTNumber))
                {
                    canWrite = false;
                }
            }

            return new CompanyReviewSummaryVM
            {
                CompanyUSDOT = companyUSDOT,
                CompanyName = company?.CompanyName ?? "Unknown Company",
                AverageRating = Math.Round(reviewsWithReplies.Average(r => r.Rating), 1),
                TotalReviews = reviewsWithReplies.Count(),
                RatingsBreakdown = ratingsBreakdown.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value),
                CanWriteReview = canWrite,
                Reviews = sortedReviews.ToList()
            };
        }

        /// <summary>
        /// To get legal names of both the company being reviewed and the reviewer company based on their USDOT numbers.
        /// </summary>
        /// <param name="companyUSDOT"></param>
        /// <param name="reviewerUSDOT"></param>
        /// <returns></returns>
        public CompanyReviewerNames GetCompanyandReviewerName(int companyUSDOT, int reviewId = 0, int rUSDOTnumber = 0)
        {
            // Determine reviewer USDOT
            int reviewerUSDOTnumber = 0;
            if (reviewId > 0)
            {
                reviewerUSDOTnumber = db.Reviews
                    .Where(r => r.Id == reviewId)
                    .Select(r => r.ReviewerUSDOT)
                    .FirstOrDefault();
            }

            // Build list of USDOTs to fetch (distinct)
            var usdotNumbers = new List<int> { companyUSDOT };

            if (rUSDOTnumber > 0)
                usdotNumbers.Add(rUSDOTnumber);
            else if (reviewerUSDOTnumber > 0)
                usdotNumbers.Add(reviewerUSDOTnumber);

            usdotNumbers = usdotNumbers.Distinct().ToList();

            // Fetch companies by USDOT list
            var companies = db.TransportCompanies
                              .Where(c => usdotNumbers.Contains(c.USDOTNumber))
                              .ToList();

            var companyName = companies.FirstOrDefault(c => c.USDOTNumber == companyUSDOT)?.LegalName;
            var reviewerName = companies.FirstOrDefault(c => c.USDOTNumber == (rUSDOTnumber > 0 ? rUSDOTnumber : reviewerUSDOTnumber))?.LegalName;

            return new CompanyReviewerNames
            {
                CompanyName = companyName,
                ReviewerName = reviewerName
            };
        }

        /// <summary>
        /// To Add a new review response or updates an existing one.
        /// </summary>
        /// <param name="addEditReviewReplyVM"></param>
        /// <exception cref="BusinessException"></exception>
        public void AddUpdateReviewResponse(AddEditReviewReplyVM addEditReviewReplyVM)
        {
            // Get logged-in user from authentication service
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            var review = db.Reviews.FirstOrDefault(r => r.Id == addEditReviewReplyVM.ReviewId && r.ReviewerUSDOT == loggedInUser.USDOTNumber);

            var existingReviewReply = false;

            if (addEditReviewReplyVM.Id == 0)
            {
                existingReviewReply = db.ReviewReplies.Any(r => r.ReviewId == addEditReviewReplyVM.ReviewId && r.CompanyUSDOT == loggedInUser.USDOTNumber);
            }

            // Ensure only logged-in users with a USDOT number can post/update a response
            if (loggedInUser == null || loggedInUser.USDOTNumber == null) throw new BusinessException("Unauthorized", "You must be logged in to add a review.");

            if (existingReviewReply) throw new BusinessException("DouplicateResponse", "You already responded to this review. Please update your response instead.");

            try
            {
                if (addEditReviewReplyVM.Id == 0)
                {
                    // Add new response
                    var response = new ReviewReply
                    {
                        ReviewId = addEditReviewReplyVM.ReviewId,
                        CompanyUSDOT = addEditReviewReplyVM.CompanyUSDOT,
                        ReplyText = addEditReviewReplyVM.Response,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    // Insert new entity
                    db.ReviewReplies.Insert(db, response);
                    db.SaveChanges();

                    var reviewe = db.Reviews.FirstOrDefault(r => r.Id == response.ReviewId).ReviewerUSDOT;
                    var reviewedCompany = db.TransportCompanies.FirstOrDefault(c => c.USDOTNumber == addEditReviewReplyVM.CompanyUSDOT);
                    var reviewerCompany = db.TransportCompanies.FirstOrDefault(c => c.USDOTNumber == reviewe);

                    if (!string.IsNullOrEmpty(reviewerCompany.CompanyRepresentativeOne))
                    {
                        var emailHeader = $"You got response to your review from {reviewedCompany.CompanyName} (USDOT {reviewedCompany.USDOTNumber})";

                        var replacements = new Dictionary<string, string>
                        {
                            { "{ReviewerCompanyName}", reviewerCompany?.CompanyName ?? "Reviewer" },
                            { "{ReviewedCompanyName}", reviewedCompany?.CompanyName ?? "Company" },
                            { "{ReviewedUSDOT}", reviewedCompany?.USDOTNumber.ToString() ?? "0" },
                            { "{ReplyText}", response.ReplyText ?? string.Empty },
                            { "{ReviewedCompanyRepresentativeOne}", reviewedCompany?.CompanyRepresentativeOne },
                            { "{emailHeader}", emailHeader }
                        };


                        EmailUtility.Send(
                            reviewerCompany.EmailAddress,
                            emailHeader,
                            AppSettings.FromEmail,
                            EmailUtility.GetTemplate(TemplateType.CompanyReviewResponeMail),
                            replacements
                        );
                    }

                }
                else
                {
                    // Update existing response
                    var response = db.ReviewReplies.FirstOrDefault(r => r.Id == addEditReviewReplyVM.Id);

                    if (response == null)
                        throw new BusinessException("404", "Response not found or you are not authorized to update it.");

                    response.ReplyText = addEditReviewReplyVM.Response;
                    response.UpdatedDate = DateTime.Now;

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// To get the details of a review by its Id.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public AddEditReviewReplyVM GetResponseDetailsById(int responseId)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("BusinessLoginAuthenticationName"));

            if (loggedInUser == null || loggedInUser.USDOTNumber == null) throw new BusinessException("", "You must be logged in to add a review.");

            var response = db.ReviewReplies.FirstOrDefault(r => r.Id == responseId);

            if (response == null)
                throw new BusinessException("", "Response not found.");

            var data = GetCompanyandReviewerName(response.CompanyUSDOT, response.ReviewId, 0);


            return new AddEditReviewReplyVM
            {
                Id = responseId,
                ReviewId = response.ReviewId,
                CompanyName = data.CompanyName,
                ReviewerName = data.ReviewerName,
                CompanyUSDOT = response.CompanyUSDOT,
                Response = response.ReplyText
            };

        }

        /// <summary>
        /// Returns a human-readable time difference string
        /// </summary>
        private string GetHumanReadableDateDiff(DateTime dateTime)
        {
            var ts = DateTime.Now - dateTime;

            if (ts.TotalSeconds < 60)
                return "Just now";
            if (ts.TotalMinutes < 60)
                return $"{(int)ts.TotalMinutes} min ago";
            if (ts.TotalHours < 24)
                return $"{(int)ts.TotalHours} hour{(ts.TotalHours >= 2 ? "s" : "")} ago";
            if (ts.TotalDays < 30)
                return $"{(int)ts.TotalDays} day{(ts.TotalDays >= 2 ? "s" : "")} ago";
            if (ts.TotalDays < 365)
                return $"{(int)(ts.TotalDays / 30)} month{(ts.TotalDays / 30 >= 2 ? "s" : "")} ago";

            return $"{(int)(ts.TotalDays / 365)} year{(ts.TotalDays / 365 >= 2 ? "s" : "")} ago";
        }

        #endregion

    }
}
