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
            bool hiringFilter  = isHiringCheckboxIsChecked  && GlobalHire       == (int)GLobalHiring.HomeStateAndCityPage;
            bool reviewsFilter = isReviewsFilterCheckboxIsChecked && ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage;

            // Fast path: no filters — sum active counts from Cities (already active-only, much faster than scanning TransportCompany)
            if (!hiringFilter && !reviewsFilter)
            {
                return (from city in db.Cities
                        join state in db.States on city.StateCode equals state.StateCode
                        where state.CountryCode == "US"
                        group city by new { state.CountryCode, state.State1, state.StateCode } into g
                        orderby g.Key.State1
                        select new StateVM
                        {
                            CountryCode = g.Key.CountryCode,
                            State       = g.Key.State1,
                            StateCode   = g.Key.StateCode,
                            StateCount  = g.Sum(c => c.NumberOfCompanies) ?? 0
                        })
                        .ToList();
            }

            // Filter path: join TransportCompanies for hiring/reviews filters
            var query = from state in db.States
                        join transport in db.TransportCompanies
                            on state.StateCode equals transport.PhysicalAddressStateCode
                        where transport.Status == "A"
                        select new { state, transport };

            if (hiringFilter)
            {
                query = query.Join(db.Businesses,
                                   t => t.transport.USDOTNumber,
                                   b => b.USDOTNumber,
                                   (t, b) => new { t.state, t.transport, business = b })
                             .Where(x => x.business.NowHiring)
                             .Select(x => new { x.state, x.transport });
            }

            if (reviewsFilter)
            {
                query = query.Join(db.Reviews,
                                   t => t.transport.USDOTNumber,
                                   r => r.CompanyUSDOT,
                                   (t, r) => new { t.state, t.transport });
            }

            return query
                .Where(x => x.state.CountryCode == "US")
                .GroupBy(x => new { x.state.CountryCode, x.state.State1, x.state.StateCode })
                .OrderBy(g => g.Key.State1)
                .Select(g => new StateVM
                {
                    CountryCode = g.Key.CountryCode,
                    State       = g.Key.State1,
                    StateCode   = g.Key.StateCode,
                    StateCount  = g.Select(x => x.transport.USDOTNumber).Distinct().Count()
                })
                .ToList();
        }


        #endregion

        #region CA state list
        /// <summary>
        /// Get CA State List
        /// </summary>
        /// <returns></returns>
        public List<StateVM> GetCaStates(bool isHiringCheckboxIsChecked, int GlobalHire, bool isReviewsFilterCheckboxIsChecked, int ReviewFilterValue)
        {
            bool hiringFilter  = isHiringCheckboxIsChecked  && GlobalHire       == (int)GLobalHiring.HomeStateAndCityPage;
            bool reviewsFilter = isReviewsFilterCheckboxIsChecked && ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage;

            // Fast path: no filters — sum active counts from Cities (already active-only, much faster than scanning TransportCompany)
            if (!hiringFilter && !reviewsFilter)
            {
                return (from city in db.Cities
                        join state in db.States on city.StateCode equals state.StateCode
                        where state.CountryCode == "CA"
                        group city by new { state.CountryCode, state.State1, state.StateCode } into g
                        orderby g.Key.State1
                        select new StateVM
                        {
                            CountryCode = g.Key.CountryCode,
                            State       = g.Key.State1,
                            StateCode   = g.Key.StateCode,
                            StateCount  = g.Sum(c => c.NumberOfCompanies) ?? 0
                        })
                        .ToList();
            }

            // Filter path: join TransportCompanies for hiring/reviews filters
            var query = from st in db.States
                        join tc in db.TransportCompanies on st.StateCode equals tc.PhysicalAddressStateCode into tcJoin
                        from tc in tcJoin.DefaultIfEmpty()
                        join b in db.Businesses on tc.USDOTNumber equals b.USDOTNumber into bJoin
                        from b in bJoin.DefaultIfEmpty()
                        join r in db.Reviews on tc.USDOTNumber equals r.CompanyUSDOT into rJoin
                        from r in rJoin.DefaultIfEmpty()
                        where st.CountryCode == "CA" && (tc == null || tc.Status == "A")
                        select new { st, tc, b, r };

            if (hiringFilter)
                query = query.Where(x => x.b != null && x.b.NowHiring);

            if (reviewsFilter)
                query = query.Where(x => x.r != null);

            return query
                .GroupBy(x => new { x.st.CountryCode, x.st.State1, x.st.StateCode })
                .OrderBy(g => g.Key.State1)
                .Select(g => new StateVM
                {
                    CountryCode = g.Key.CountryCode,
                    State       = g.Key.State1,
                    StateCode   = g.Key.StateCode,
                    StateCount  = g.Select(x => x.tc != null ? x.tc.USDOTNumber : 0).Distinct().Count()
                })
                .ToList();
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
            bool hiringFilter  = isHiringCheckboxIsChecked       && (GlobalHire       == (int)GLobalHiring.HomeStateAndCityPage || GlobalHire       == (int)GLobalHiring.StateAndCityPage);
            bool reviewsFilter = isReviewsFilterCheckboxIsChecked && (ReviewFilterValue == (int)GLobalHiring.HomeStateAndCityPage || ReviewFilterValue == (int)GLobalHiring.StateAndCityPage);

            // Fast path: no filters — read Cities table (active-only, already aggregated, ordered by CityName via UQ index)
            if (!hiringFilter && !reviewsFilter)
            {
                return db.Cities
                    .Where(c => c.StateCode == state)
                    .OrderBy(c => c.CityName)
                    .Select(c => new CityVM
                    {
                        CityName = c.CityName,
                        StateCode = state,
                        CompanyCount = c.NumberOfCompanies ?? 0
                    })
                    .ToList();
            }

            // Filter path: keep TransportCompany GroupBy
            var query = db.TransportCompanies
                .Where(tc => tc.PhysicalAddressStateCode == state && tc.Status == "A");

            if (hiringFilter)
            {
                query = from tc in query
                        join b in db.Businesses on tc.USDOTNumber equals b.USDOTNumber
                        where b.NowHiring == true
                        select tc;
            }

            if (reviewsFilter)
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

            return query
                .Where(tc => !string.IsNullOrEmpty(tc.PhysicalAddressCity))
                .GroupBy(tc => tc.PhysicalAddressCity)
                .OrderBy(g => g.Key)
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
                .Where(tc => tc.PhysicalAddressStateCode == state && tc.Status == "A");

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
                // Title-case the city name (stored uppercase) so the title tag reads
                // "Birmingham Alabama ..." in search results, not "BIRMINGHAM Alabama ...".
                var titleCity = string.IsNullOrEmpty(city)
                    ? city
                    : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(city.ToLower());
                pagetitle = titleCity + " " + state + " " + pagetitle;
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

                //Get Count of total active companies (cached in Admin.NumberOfCompanies by FinishCityUpdate)
                var countOfTotalCompanies = db.Database.SqlQuery<int>("SELECT ISNULL(NumberOfCompanies, 0) FROM Admin").FirstOrDefault();
                pageDescription = pageDescription.Replace("more than 1.8 million", countOfTotalCompanies.ToString("#,##0"));
                pageDescription = pageDescription.Replace("{N:N0}", countOfTotalCompanies.ToString("#,##0"));
                var homeCounts = GetHomeCountryCounts();
                pageDescription = pageDescription.Replace("{NUS:N0}", homeCounts.Item1.ToString("#,##0"));
                pageDescription = pageDescription.Replace("{NCA:N0}", homeCounts.Item2.ToString("#,##0"));
            }
            if (pageName == "Statepage")
            {
                var StateArticle = db.Database.SqlQuery<string>("SELECT StateArticle FROM States WHERE State = @p0", state).FirstOrDefault();
                if (StateArticle != null)
                {
                    pageDescription = StateArticle;
                }
            }
            // NOTE: city articles (Cities.Article) are no longer rendered publicly —
            // the city data module replaced them (July 2026). Admin tooling still
            // reads/writes the column; future auto-generated articles may reuse it.
            return pageDescription;
        }

        /// <summary>
        /// Active company counts by country (US, CA) for the homepage
        /// {NUS:N0} / {NCA:N0} placeholders. US count uses the same
        /// definition as the statistics pages (Status='A', physical state
        /// in the country's state codes). Cached 30 days; the admin
        /// "Refresh Statistics Cache" button clears it via the
        /// HomeCountryCounts_ prefix.
        /// </summary>
        private Tuple<int, int> GetHomeCountryCounts()
        {
            const string cacheKey = "HomeCountryCounts_v1";
            var cached = HttpRuntime.Cache[cacheKey] as Tuple<int, int>;
            if (cached != null) { return cached; }

            var usCodes = db.States.Where(s => s.CountryCode == "US").Select(s => s.StateCode).ToList();
            var caCodes = db.States.Where(s => s.CountryCode == "CA").Select(s => s.StateCode).ToList();
            int usCount = db.TransportCompanies.Count(tc => tc.Status == "A" && usCodes.Contains(tc.PhysicalAddressStateCode));
            int caCount = db.TransportCompanies.Count(tc => tc.Status == "A" && caCodes.Contains(tc.PhysicalAddressStateCode));

            var result = Tuple.Create(usCount, caCount);
            HttpRuntime.Cache.Insert(cacheKey, result, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);
            return result;
        }

        public string GetPlainHomeMetaDescription()
        {
            var countOfDriverJobs = db.Database.SqlQuery<int>("select count(*) from Business where NowHiring!=0").FirstOrDefault();
            var description = db.Database.SqlQuery<string>("select HomePageDescription from Admin").FirstOrDefault() ?? "";
            var plainCount = countOfDriverJobs.ToString() + " Truck Driver Jobs";
            description = description.Replace("Truck Driver Jobs", plainCount);
            var countOfTotalCompanies = db.Database.SqlQuery<int>("SELECT ISNULL(NumberOfCompanies, 0) FROM Admin").FirstOrDefault();
            description = description.Replace("more than 1.8 million", countOfTotalCompanies.ToString("#,##0"));
            description = description.Replace("{N:N0}", countOfTotalCompanies.ToString("#,##0"));
            var plainHomeCounts = GetHomeCountryCounts();
            description = description.Replace("{NUS:N0}", plainHomeCounts.Item1.ToString("#,##0"));
            description = description.Replace("{NCA:N0}", plainHomeCounts.Item2.ToString("#,##0"));
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
                // Exact name matches first (so small cities like ALLEN, SD aren't pushed
                // out of the list by bigger prefix matches like ALLENTOWN), then by size.
                searchList = (from city in db.Cities
                              where city.CityName.StartsWith(searchText)
                              orderby (city.CityName == searchText ? 0 : 1), city.NumberOfCompanies descending
                              select new SearchVM
                              {
                                  Value = city.CityName + ", " + city.StateCode
                              })
                              .Take(15)
                              .ToList();
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
            // Exact name matches first, then by size — see GetSearchResultAutoComplete.
            return (from city in db.Cities
                    where city.CityName.StartsWith(searchText)
                    orderby (city.CityName == searchText ? 0 : 1), city.NumberOfCompanies descending
                    select new SearchVM
                    {
                        Value = city.CityName + ", " + city.StateCode
                    })
                    .Take(15)
                    .ToList();
        }

        public List<SearchRedirectResult> GetSearchRedirectInfo(string searchText, string selectedDropdownValue, bool isHiringCheckboxIsChecked, int GlobalHire)
        {
            string city = "";
            string state = "";
            IQueryable<SearchRedirectResult> query;

            if (string.IsNullOrEmpty(selectedDropdownValue) || selectedDropdownValue == "City")
            {
                if (searchText.Contains(","))
                {
                    city = searchText.Split(',')[0];
                    state = searchText.Split(',')[1].Trim();
                }

                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    query = from companies in db.TransportCompanies
                            where companies.Business.TransportCompany.PhysicalAddressCity == city
                               && companies.Business.TransportCompany.PhysicalAddressStateCode == state
                               && companies.Business.NowHiring == true
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
                else
                {
                    query = from companies in db.TransportCompanies
                            where companies.PhysicalAddressCity == city && companies.PhysicalAddressStateCode == state
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
            }
            else if (selectedDropdownValue == "USDOT Number")
            {
                searchText = searchText.Trim();
                if (!int.TryParse(searchText, out int usdot))
                    throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);

                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    query = from companies in db.TransportCompanies
                            where companies.Business.USDOTNumber == usdot && companies.Business.NowHiring == true
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
                else
                {
                    query = from companies in db.TransportCompanies
                            where companies.USDOTNumber == usdot
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
            }
            else if (selectedDropdownValue == "Company Name")
            {
                searchText = searchText.Trim();
                var companyName = searchText.Split(',')[0];

                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    query = from companies in db.TransportCompanies
                            where companies.Business.TransportCompany.CompanyName == companyName && companies.Business.NowHiring == true
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
                else
                {
                    query = from companies in db.TransportCompanies
                            where companies.CompanyName == companyName
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };

                    var searchTextSplits = searchText.Split(',');
                    if (searchTextSplits.Length > 1)
                    {
                        city = searchTextSplits[1].Trim();
                        query = query.Where(c => c.PhysicalAddressCity == city);
                    }
                    if (searchTextSplits.Length > 2)
                    {
                        state = searchTextSplits[2].Trim();
                        query = query.Where(c => c.PhysicalAddressStateCode == state);
                    }
                }
            }
            else // MC Number
            {
                searchText = searchText.Trim();
                if (!int.TryParse(searchText, out int mc))
                    throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);

                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    query = from companies in db.TransportCompanies
                            where companies.Business.TransportCompany.IccDocketNumberFirst == mc && companies.Business.NowHiring == true
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
                else
                {
                    query = from companies in db.TransportCompanies
                            where companies.IccDocketNumberFirst == mc
                            select new SearchRedirectResult
                            {
                                USDOTNumber = companies.USDOTNumber,
                                PhysicalAddressCity = companies.PhysicalAddressCity,
                                PhysicalAddressStateCode = companies.PhysicalAddressStateCode
                            };
                }
            }

            var results = query.Take(2).ToList();
            if (results.Count == 0)
                throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);
            return results;
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
                searchText = searchText.Trim();
                if (!int.TryParse(searchText, out int usdot))
                    throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);

                //If user check checkbox for Hiring then it returns only hiring companies with USDOT Number
                //else it gives  the comapny which match USDOT Number
                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    companyList = (from companies in db.TransportCompanies
                                   where companies.Business.USDOTNumber == usdot && companies.Business.NowHiring == true
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
                    companyList = (from companies in db.TransportCompanies
                                   where companies.USDOTNumber == usdot
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
                searchText = searchText.Trim();
                if (!int.TryParse(searchText, out int mc))
                    throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);

                if (isHiringCheckboxIsChecked && GlobalHire != (int)GLobalHiring.NotToShow)
                {
                    companyList = (from companies in db.TransportCompanies
                                   where companies.Business.TransportCompany.IccDocketNumberFirst == mc && companies.Business.NowHiring == true
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
                    companyList = (from companies in db.TransportCompanies
                                   where companies.IccDocketNumberFirst == mc
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
            PagedList<CompanyVM> allCompanies = companyList.SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
            if (allCompanies.Pagination.TotalCount == 0)
            {
                throw new BusinessException("404", "No trucking company matched your search criteria.", autoShow: true);
            }
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

        public string GetCityNameFromURLCityName(string urlCityName, string stateCode = null)
        {
            if (!urlCityName.Contains("-"))
                return urlCityName;

            var spaceName = urlCityName.Replace("-", " ");
            IQueryable<City> stateQuery = stateCode != null
                ? db.Cities.Where(c => c.StateCode == stateCode)
                : db.Cities;

            // case1: dash→space seek against Cities (UQ index seek when stateCode provided)
            string cityNameFromDb = stateQuery.Where(c => c.CityName == spaceName).Select(c => c.CityName).FirstOrDefault();

            if (cityNameFromDb == null)
            {
                // case2: exact match — city itself contains dashes (e.g. "WINSTON-SALEM")
                cityNameFromDb = stateQuery.Where(c => c.CityName == urlCityName).Select(c => c.CityName).FirstOrDefault();
            }

            if (cityNameFromDb == null)
            {
                // case3 fallback (rare): load state cities in memory, match by computed replace
                var stateCities = stateQuery.Select(c => c.CityName).ToList();
                cityNameFromDb = stateCities.FirstOrDefault(n => n != null && n.Replace(" ", "-") == urlCityName);
            }

            return cityNameFromDb;
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

            // Restrict to active companies only — Status == "A"
            lstCompanies = lstCompanies.Where(a => a.Status == "A").AsQueryable();

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

            // Batch NowHiring flags — one SELECT IN query for the whole page.
            var hiringByUsdot = db.Businesses
                .Where(b => usdotNumbers.Contains(b.USDOTNumber))
                .Select(b => new { b.USDOTNumber, b.NowHiring })
                .ToDictionary(x => x.USDOTNumber, x => x.NowHiring);

            // Batch driver-type names — one JOIN query for the whole page.
            var driverTypesByUsdot = (from tc in db.TransportCompanies
                                      where usdotNumbers.Contains(tc.USDOTNumber)
                                      from dt in tc.DriverTypes
                                      select new { tc.USDOTNumber, dt.DriverName })
                                     .AsEnumerable()
                                     .GroupBy(x => x.USDOTNumber)
                                     .ToDictionary(g => g.Key, g => g.Select(x => x.DriverName.Trim()).ToList());

            var pageItems = pageEntities.Select(companies =>
            {
                CompanyRatingVM rating;
                ratingsByUsdot.TryGetValue(companies.USDOTNumber, out rating);
                bool nowHiring;
                hiringByUsdot.TryGetValue(companies.USDOTNumber, out nowHiring);
                System.Collections.Generic.List<string> driverTypes;
                driverTypesByUsdot.TryGetValue(companies.USDOTNumber, out driverTypes);
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
                    AverageRating = rating != null ? rating.AverageRating : 0,
                    NowHiring = nowHiring,
                    CompanyDriverType = driverTypes ?? new System.Collections.Generic.List<string>()
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
                catch (Exception)
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

            // Get reviews — one row per review, no joins to avoid duplication from
            // TransportCompanies or ReviewReplies having multiple rows per key.
            var rawReviews = db.Reviews
                .Where(r => r.CompanyUSDOT == companyUSDOT)
                .ToList();

            var reviewerUsdots = rawReviews.Select(r => r.ReviewerUSDOT).Distinct().ToList();
            var reviewerNames = db.TransportCompanies
                .Where(tc => reviewerUsdots.Contains(tc.USDOTNumber))
                .Select(tc => new { tc.USDOTNumber, tc.LegalName })
                .ToList()
                .GroupBy(tc => tc.USDOTNumber)
                .ToDictionary(g => g.Key, g => g.First().LegalName);

            var reviewIds = rawReviews.Select(r => r.Id).ToList();
            var repliesByReviewId = db.ReviewReplies
                .Where(rr => reviewIds.Contains(rr.ReviewId))
                .ToList()
                .GroupBy(rr => rr.ReviewId)
                .ToDictionary(g => g.Key, g => g.First());

            var reviewsWithReplies = rawReviews.Select(r =>
            {
                string reviewerName;
                reviewerNames.TryGetValue(r.ReviewerUSDOT, out reviewerName);
                ReviewReply reply;
                repliesByReviewId.TryGetValue(r.Id, out reply);
                return new ReviewWithReplyVM
                {
                    ReviewId    = r.Id,
                    ReponseId   = reply != null ? reply.Id : 0,
                    Rating      = r.Rating,
                    Comment     = r.Comment,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate,
                    ReviewerUSDOT  = r.ReviewerUSDOT,
                    ReviewerName   = reviewerName,
                    ReplyText      = reply != null ? reply.ReplyText    : null,
                    ReplyCreatedDate = reply != null ? reply.CreatedDate : null,
                    ReplyUpdateDate  = reply != null ? reply.UpdatedDate : null,
                };
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

        #region Statistics

        public void InvalidateStatisticsCache()
        {
            var prefixes = new[] { "StatisticsData_", "ActiveCompaniesData_", "ActiveBrokersData_", "StateCompaniesData_", "CityCompaniesData_", "NewRegistrationsData_", "NewRegistrationsMonthData_", "FleetOperationsData_", "CargoData_", "HomeCountryCounts_" };
            var keysToRemove = new System.Collections.Generic.List<string>();
            var enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key as string;
                if (key == null) { continue; }
                foreach (var prefix in prefixes)
                {
                    if (key.StartsWith(prefix)) { keysToRemove.Add(key); break; }
                }
            }
            foreach (var key in keysToRemove) { HttpRuntime.Cache.Remove(key); }
        }

        public StatisticsIndexVM GetStatisticsData()
        {
            const string cacheKey = "StatisticsData_v6";
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsIndexVM;
            if (cached != null) { return cached; }

            var vm = new StatisticsIndexVM();

            // Get US state codes once; used as IN-filter on every query below
            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();

            vm.PureBrokerCount = db.TransportCompanies
                .Count(tc => tc.Status == "A" && tc.EntityType == "B" && usStateCodes.Contains(tc.PhysicalAddressStateCode));
            vm.TotalCompanies = db.TransportCompanies
                .Count(tc => tc.Status == "A" && usStateCodes.Contains(tc.PhysicalAddressStateCode)) - vm.PureBrokerCount;

            vm.StatesCount = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.PhysicalAddressStateCode != null && tc.PhysicalAddressStateCode != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Select(tc => tc.PhysicalAddressStateCode)
                .Distinct()
                .Count();

            vm.CitiesCount = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.PhysicalAddressCity != null && tc.PhysicalAddressCity != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Select(tc => new { tc.PhysicalAddressCity, tc.PhysicalAddressStateCode })
                .Distinct()
                .Count();

            double? avgFleet = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.TrucksAndTractors != null && tc.TrucksAndTractors > 0 && tc.TrucksAndTractors < 10000
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Average(tc => (double?)tc.TrucksAndTractors);
            vm.AvgFleetSize = avgFleet.HasValue ? (int)avgFleet.Value : 0;

            int? lastChanged = db.TransportCompanies
                .Where(tc => tc.DateLastChanged != null && tc.DateLastChanged > 19000101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Max(tc => tc.DateLastChanged);
            if (lastChanged.HasValue)
            {
                int y = lastChanged.Value / 10000;
                int m = (lastChanged.Value / 100) % 100;
                int d = lastChanged.Value % 100;
                try { vm.LastDataUpdate = new DateTime(y, m, d).ToString("MMMM d, yyyy"); } catch { vm.LastDataUpdate = ""; }
            }

            int today = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + DateTime.Today.Day;
            int monthStart = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + 1;
            int yearStart = DateTime.Today.Year * 10000 + 101;

            vm.NewThisMonth = db.TransportCompanies
                .Count(tc => tc.DateAdded >= monthStart && tc.DateAdded <= today
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode));

            vm.NewThisYear = db.TransportCompanies
                .Count(tc => tc.DateAdded >= yearStart && tc.DateAdded <= today
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode));

            {
                var nr12PrevMonth  = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                int nr12EndInt     = nr12PrevMonth.Year * 10000 + nr12PrevMonth.Month * 100 + 31;
                var nr12StartDate  = nr12PrevMonth.AddMonths(-11);
                int nr12StartInt   = nr12StartDate.Year * 10000 + nr12StartDate.Month * 100 + 1;
                vm.NewRegistrations12MonthsCount = db.TransportCompanies
                    .Count(tc => tc.Status == "A"
                              && tc.DateAdded != null && tc.DateAdded >= nr12StartInt && tc.DateAdded <= nr12EndInt
                              && usStateCodes.Contains(tc.PhysicalAddressStateCode));
            }

            var topStateRaw = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.PhysicalAddressStateCode != null && tc.PhysicalAddressStateCode != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => tc.PhysicalAddressStateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(15)
                .ToList();

            var topStateCodes = topStateRaw.Select(x => x.StateCode).ToList();
            var stateNames = db.States
                .Where(s => topStateCodes.Contains(s.StateCode))
                .ToDictionary(s => s.StateCode, s => s.State1);

            vm.TopStates = topStateRaw.Select(x => new StateStatVM
            {
                StateCode = x.StateCode,
                StateName = stateNames.ContainsKey(x.StateCode) ? stateNames[x.StateCode] : x.StateCode,
                Count = x.Count
            }).ToList();

            vm.TopCities = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.PhysicalAddressCity != null && tc.PhysicalAddressCity != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => new { tc.PhysicalAddressCity, tc.PhysicalAddressStateCode })
                .Select(g => new { City = g.Key.PhysicalAddressCity, StateCode = g.Key.PhysicalAddressStateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList()
                .Select(x => new CityStatVM { City = x.City, StateCode = x.StateCode, Count = x.Count })
                .ToList();

            vm.RegistrationsByYear = db.TransportCompanies
                .Where(tc => tc.DateAdded != null && tc.DateAdded > 19500101 && tc.DateAdded < 20500101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => tc.DateAdded / 10000)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Year)
                .ToList()
                .Select(x => new YearStatVM { Year = x.Year ?? 0, Count = x.Count })
                .ToList();

            vm.ActiveBrokers = db.TransportCompanies
                .Count(tc => tc.Status == "A"
                          && tc.EntityType.Contains("B")
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode));

            var allStateCountsRaw = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.PhysicalAddressStateCode != null && tc.PhysicalAddressStateCode != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => tc.PhysicalAddressStateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .ToDictionary(x => x.StateCode, x => x.Count);

            vm.AllUsStates = db.States
                .Where(s => s.CountryCode == "US")
                .OrderBy(s => s.State1)
                .Select(s => new StateStatVM { StateCode = s.StateCode, StateName = s.State1, Count = 0 })
                .ToList();

            foreach (var st in vm.AllUsStates)
            {
                if (allStateCountsRaw.ContainsKey(st.StateCode))
                    st.Count = allStateCountsRaw[st.StateCode];
            }

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        public StatisticsNewRegistrationsVM GetNewRegistrationsData(string range = "24m")
        {
            string normalizedRange = (range == "12m" || range == "36m" || range == "48m") ? range : "24m";
            string cacheKey = "NewRegistrationsData_v2_" + normalizedRange;
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsNewRegistrationsVM;
            if (cached != null) return cached;

            var vm = new StatisticsNewRegistrationsVM();
            vm.Range = normalizedRange;

            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();

            var now           = DateTime.Today;
            var prevMonthDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            int rangeEndInt   = prevMonthDate.Year * 10000 + prevMonthDate.Month * 100 + 31;

            int rangeMonths, rangeStartInt, queryStartInt;
            int prevStartInt = 0;

            rangeMonths   = normalizedRange == "12m" ? 12 : normalizedRange == "36m" ? 36 : normalizedRange == "48m" ? 48 : 24;
            var startDate = prevMonthDate.AddMonths(-(rangeMonths - 1));
            rangeStartInt = startDate.Year * 10000 + startDate.Month * 100 + 1;
            var prevStartDate = startDate.AddMonths(-rangeMonths);
            prevStartInt  = prevStartDate.Year * 10000 + prevStartDate.Month * 100 + 1;
            queryStartInt = prevStartInt;

            vm.RangeMonths = Math.Max(rangeMonths, 1);
            vm.RangeStart  = new DateTime(rangeStartInt / 10000, (rangeStartInt / 100) % 100, 1);
            vm.RangeEnd    = prevMonthDate;

            var rawRows = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.DateAdded != null
                          && tc.DateAdded >= queryStartInt && tc.DateAdded <= rangeEndInt
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Select(tc => new {
                    tc.EntityType,
                    StateCode = tc.PhysicalAddressStateCode,
                    City      = tc.PhysicalAddressCity,
                    tc.DateAdded
                })
                .ToList();

            var currentRows = rawRows.Where(r => r.DateAdded >= rangeStartInt).ToList();
            var prevRows    = rawRows.Where(r => r.DateAdded < rangeStartInt).ToList();

            Func<string, bool> isMC     = et => et != null && et.Contains("C");
            Func<string, bool> isBroker = et => et != null && et.Contains("B");
            Func<string, bool> isBoth   = et => et != null && et.Contains("B") && et.Contains("C");

            vm.TotalNewCount        = currentRows.Count;
            vm.NewMotorCarrierCount = currentRows.Count(r => isMC(r.EntityType));
            vm.NewBrokerCount       = currentRows.Count(r => isBroker(r.EntityType));
            vm.NewBothCount         = currentRows.Count(r => isBoth(r.EntityType));
            vm.NewOtherCount        = vm.TotalNewCount - vm.NewMotorCarrierCount - vm.NewBrokerCount + vm.NewBothCount;
            vm.AvgPerMonth          = vm.RangeMonths > 0 ? Math.Round((decimal)vm.TotalNewCount / vm.RangeMonths, 1) : 0m;
            vm.EntityMCCount        = vm.NewMotorCarrierCount - vm.NewBothCount;
            vm.EntityBrokerCount    = vm.NewBrokerCount - vm.NewBothCount;
            vm.EntityBothCount      = vm.NewBothCount;
            vm.EntityOtherCount     = vm.NewOtherCount;

            if (prevRows != null)
            {
                vm.PrevTotalCount        = prevRows.Count;
                vm.PrevMotorCarrierCount = prevRows.Count(r => isMC(r.EntityType));
                vm.PrevBrokerCount       = prevRows.Count(r => isBroker(r.EntityType));
                vm.PrevBothCount         = prevRows.Count(r => isBoth(r.EntityType));
            }

            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                     "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            int chartMonths = Math.Min(vm.RangeMonths, 60);
            var monthsInRange = new System.Collections.Generic.List<int>();
            for (int mi = chartMonths - 1; mi >= 0; mi--)
            {
                var d2 = prevMonthDate.AddMonths(-mi);
                monthsInRange.Add(d2.Year * 100 + d2.Month);
            }

            var byMonthAll = currentRows
                .GroupBy(r => (r.DateAdded ?? 0) / 100)
                .ToDictionary(g => g.Key, g => g.Count());
            var byMonthMC = currentRows.Where(r => isMC(r.EntityType))
                .GroupBy(r => (r.DateAdded ?? 0) / 100)
                .ToDictionary(g => g.Key, g => g.Count());
            var byMonthBroker = currentRows.Where(r => isBroker(r.EntityType))
                .GroupBy(r => (r.DateAdded ?? 0) / 100)
                .ToDictionary(g => g.Key, g => g.Count());

            vm.MonthlyAll = monthsInRange.Select(ym => new NrMonthlyRow
            {
                Label     = monthNames[(ym % 100) - 1] + " '" + (ym / 100 % 100).ToString("D2"),
                YearMonth = ym,
                Count     = byMonthAll.ContainsKey(ym) ? byMonthAll[ym] : 0
            }).ToList();

            vm.MonthlyMC = monthsInRange.Select(ym => new NrMonthlyRow
            {
                Label     = monthNames[(ym % 100) - 1] + " '" + (ym / 100 % 100).ToString("D2"),
                YearMonth = ym,
                Count     = byMonthMC.ContainsKey(ym) ? byMonthMC[ym] : 0
            }).ToList();

            vm.MonthlyBroker = monthsInRange.Select(ym => new NrMonthlyRow
            {
                Label     = monthNames[(ym % 100) - 1] + " '" + (ym / 100 % 100).ToString("D2"),
                YearMonth = ym,
                Count     = byMonthBroker.ContainsKey(ym) ? byMonthBroker[ym] : 0
            }).ToList();

            var topStatesRaw = currentRows
                .Where(r => r.StateCode != null && r.StateCode != "")
                .GroupBy(r => r.StateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var topStateCodes2 = topStatesRaw.Select(x => x.StateCode).ToList();
            var stateNameDict2 = db.States
                .Where(s => topStateCodes2.Contains(s.StateCode))
                .ToDictionary(s => s.StateCode, s => s.State1);

            int totalCurrent = vm.TotalNewCount > 0 ? vm.TotalNewCount : 1;
            vm.TopStates = topStatesRaw.Select(x => new NrStateRow
            {
                StateCode      = x.StateCode,
                StateName      = stateNameDict2.ContainsKey(x.StateCode) ? stateNameDict2[x.StateCode] : x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / totalCurrent * 100.0
            }).ToList();

            var topCitiesRaw = currentRows
                .Where(r => r.City != null && r.City != "" && r.StateCode != null)
                .GroupBy(r => new { r.City, r.StateCode })
                .Select(g => new { CityName = g.Key.City, StateCode = g.Key.StateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            vm.TopCities = topCitiesRaw.Select(x => new NrCityRow
            {
                CityName       = x.CityName,
                StateCode      = x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / totalCurrent * 100.0
            }).ToList();

            var stateCountDict = currentRows
                .Where(r => r.StateCode != null && r.StateCode != "")
                .GroupBy(r => r.StateCode)
                .ToDictionary(g => g.Key, g => g.Count());

            var allUsStatesForMap = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => new { s.StateCode, StateName = s.State1 })
                .ToList();

            vm.AllStatesForMap = allUsStatesForMap.Select(s => new StateMapRow
            {
                StateCode    = s.StateCode,
                StateName    = s.StateName,
                CompanyCount = stateCountDict.ContainsKey(s.StateCode) ? stateCountDict[s.StateCode] : 0
            }).ToList();

            var allDateAdded = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.DateAdded != null && tc.DateAdded > 19000101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Select(tc => tc.DateAdded)
                .ToList();

            var today2           = DateTime.Today;
            int sixMonthsAgo    = today2.AddMonths(-6).Year  * 10000 + today2.AddMonths(-6).Month  * 100 + today2.AddMonths(-6).Day;
            int twelveMonthsAgo = today2.AddMonths(-12).Year * 10000 + today2.AddMonths(-12).Month * 100 + today2.AddMonths(-12).Day;
            int twoYearsAgo     = today2.AddYears(-2).Year   * 10000 + today2.AddYears(-2).Month   * 100 + today2.AddYears(-2).Day;
            int fiveYearsAgo    = today2.AddYears(-5).Year   * 10000 + today2.AddYears(-5).Month   * 100 + today2.AddYears(-5).Day;

            var ageDist = new NrAgeDistribution();
            foreach (int? da in allDateAdded)
            {
                int dv = da ?? 0;
                if (dv <= 0) continue;
                ageDist.Total++;
                if      (dv >= sixMonthsAgo)    ageDist.ZeroToSixMonths++;
                else if (dv >= twelveMonthsAgo) ageDist.SixToTwelveMonths++;
                else if (dv >= twoYearsAgo)     ageDist.OneToTwoYears++;
                else if (dv >= fiveYearsAgo)    ageDist.TwoToFiveYears++;
                else                            ageDist.FivePlusYears++;
            }
            vm.AgeDistribution = ageDist;

            int? lastChanged2 = db.TransportCompanies
                .Where(tc => tc.DateLastChanged != null && tc.DateLastChanged > 19000101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Max(tc => tc.DateLastChanged);
            if (lastChanged2.HasValue)
            {
                int y2 = lastChanged2.Value / 10000;
                int m3 = (lastChanged2.Value / 100) % 100;
                int d3 = lastChanged2.Value % 100;
                try { vm.LastDataUpdate = new DateTime(y2, m3, d3); } catch { vm.LastDataUpdate = null; }
            }

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        public StatisticsNewRegistrationsMonthVM GetNewRegistrationsMonthData(int year, int month)
        {
            string cacheKey = "NewRegistrationsMonthData_v4_" + year.ToString() + month.ToString("D2");
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsNewRegistrationsMonthVM;
            if (cached != null) return cached;

            var vm = new StatisticsNewRegistrationsMonthVM();
            vm.Year      = year;
            vm.Month     = month;
            var monthDate = new DateTime(year, month, 1);
            vm.MonthName  = monthDate.ToString("MMMM yyyy");

            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();

            int currStartInt = year * 10000 + month * 100 + 1;
            int currEndInt   = year * 10000 + month * 100 + 31;
            var prevDate     = monthDate.AddMonths(-1);
            int prevStartInt = prevDate.Year * 10000 + prevDate.Month * 100 + 1;
            vm.PrevYear  = prevDate.Year;
            vm.PrevMonth = prevDate.Month;
            vm.HasPrev   = prevDate.Year >= 2000;

            var nextDate      = monthDate.AddMonths(1);
            var now2          = DateTime.Today;
            var lastComplete2 = new DateTime(now2.Year, now2.Month, 1).AddMonths(-1);
            vm.NextYear  = nextDate.Year;
            vm.NextMonth = nextDate.Month;
            vm.HasNext   = nextDate <= lastComplete2;

            var rawRows = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.DateAdded != null
                          && tc.DateAdded >= prevStartInt && tc.DateAdded <= currEndInt
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Select(tc => new {
                    tc.EntityType,
                    StateCode = tc.PhysicalAddressStateCode,
                    City      = tc.PhysicalAddressCity,
                    tc.DateAdded
                })
                .ToList();

            var currentRows = rawRows.Where(r => r.DateAdded >= currStartInt).ToList();
            var prevRows    = rawRows.Where(r => r.DateAdded < currStartInt).ToList();

            Func<string, bool> isMC     = et => et != null && et.Contains("C");
            Func<string, bool> isBroker = et => et != null && et.Contains("B");
            Func<string, bool> isBoth   = et => et != null && et.Contains("B") && et.Contains("C");

            var carrierRows = currentRows.Where(r => isMC(r.EntityType)).ToList();
            var brokerRows  = currentRows.Where(r => isBroker(r.EntityType)).ToList();
            var bothCount   = currentRows.Count(r => isBoth(r.EntityType));

            vm.TotalCount        = currentRows.Count;
            vm.MotorCarrierCount = carrierRows.Count;
            vm.BrokerCount       = brokerRows.Count;
            vm.BothCount         = bothCount;
            vm.OtherCount        = vm.TotalCount - vm.MotorCarrierCount - vm.BrokerCount + bothCount;

            vm.PrevTotalCount        = prevRows.Count;
            vm.PrevMotorCarrierCount = prevRows.Count(r => isMC(r.EntityType));
            vm.PrevBrokerCount       = prevRows.Count(r => isBroker(r.EntityType));
            vm.PrevBothCount         = prevRows.Count(r => isBoth(r.EntityType));

            var byDay = currentRows
                .Where(r => r.DateAdded != null)
                .GroupBy(r => (r.DateAdded ?? 0) % 100)
                .ToDictionary(g => g.Key, g => g.Count());

            int daysInMonth = DateTime.DaysInMonth(year, month);
            vm.DailyAll = new System.Collections.Generic.List<NrDailyRow>();
            for (int d = 1; d <= daysInMonth; d++)
            {
                vm.DailyAll.Add(new NrDailyRow { Day = d, Count = byDay.ContainsKey(d) ? byDay[d] : 0 });
            }

            var byDayCarrier = carrierRows
                .Where(r => r.DateAdded != null)
                .GroupBy(r => (r.DateAdded ?? 0) % 100)
                .ToDictionary(g => g.Key, g => g.Count());
            var byDayBroker = brokerRows
                .Where(r => r.DateAdded != null)
                .GroupBy(r => (r.DateAdded ?? 0) % 100)
                .ToDictionary(g => g.Key, g => g.Count());
            vm.DailyCarriers = new System.Collections.Generic.List<NrDailyRow>();
            vm.DailyBrokers  = new System.Collections.Generic.List<NrDailyRow>();
            for (int dc = 1; dc <= daysInMonth; dc++)
            {
                vm.DailyCarriers.Add(new NrDailyRow { Day = dc, Count = byDayCarrier.ContainsKey(dc) ? byDayCarrier[dc] : 0 });
                vm.DailyBrokers.Add(new NrDailyRow  { Day = dc, Count = byDayBroker.ContainsKey(dc)  ? byDayBroker[dc]  : 0 });
            }

            var topStatesRaw = currentRows
                .Where(r => r.StateCode != null && r.StateCode != "")
                .GroupBy(r => r.StateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var carrierStatesRaw = carrierRows
                .Where(r => r.StateCode != null && r.StateCode != "")
                .GroupBy(r => r.StateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var brokerStatesRaw = brokerRows
                .Where(r => r.StateCode != null && r.StateCode != "")
                .GroupBy(r => r.StateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var allNeededCodes = topStatesRaw.Select(x => x.StateCode)
                .Union(carrierStatesRaw.Select(x => x.StateCode))
                .Union(brokerStatesRaw.Select(x => x.StateCode))
                .Distinct().ToList();
            var stateNameDict3 = db.States
                .Where(s => allNeededCodes.Contains(s.StateCode))
                .ToDictionary(s => s.StateCode, s => s.State1);

            int totalC3       = vm.TotalCount > 0 ? vm.TotalCount : 1;
            int carrierTotal3 = vm.MotorCarrierCount > 0 ? vm.MotorCarrierCount : 1;
            int brokerTotal3  = vm.BrokerCount > 0 ? vm.BrokerCount : 1;

            vm.TopStates = topStatesRaw.Select(x => new NrStateRow
            {
                StateCode      = x.StateCode,
                StateName      = stateNameDict3.ContainsKey(x.StateCode) ? stateNameDict3[x.StateCode] : x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / totalC3 * 100.0
            }).ToList();

            vm.CarrierTopStates = carrierStatesRaw.Select(x => new NrStateRow
            {
                StateCode      = x.StateCode,
                StateName      = stateNameDict3.ContainsKey(x.StateCode) ? stateNameDict3[x.StateCode] : x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / carrierTotal3 * 100.0
            }).ToList();

            vm.BrokerTopStates = brokerStatesRaw.Select(x => new NrStateRow
            {
                StateCode      = x.StateCode,
                StateName      = stateNameDict3.ContainsKey(x.StateCode) ? stateNameDict3[x.StateCode] : x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / brokerTotal3 * 100.0
            }).ToList();

            vm.CarrierTopState = (vm.CarrierTopStates != null && vm.CarrierTopStates.Count > 0) ? vm.CarrierTopStates[0].StateName : "";
            vm.BrokerTopState  = (vm.BrokerTopStates  != null && vm.BrokerTopStates.Count  > 0) ? vm.BrokerTopStates[0].StateName  : "";

            var topCitiesRaw3 = currentRows
                .Where(r => r.City != null && r.City != "" && r.StateCode != null)
                .GroupBy(r => new { r.City, r.StateCode })
                .Select(g => new { CityName = g.Key.City, StateCode = g.Key.StateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var carrierCitiesRaw = carrierRows
                .Where(r => r.City != null && r.City != "" && r.StateCode != null)
                .GroupBy(r => new { r.City, r.StateCode })
                .Select(g => new { CityName = g.Key.City, StateCode = g.Key.StateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var brokerCitiesRaw = brokerRows
                .Where(r => r.City != null && r.City != "" && r.StateCode != null)
                .GroupBy(r => new { r.City, r.StateCode })
                .Select(g => new { CityName = g.Key.City, StateCode = g.Key.StateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            vm.TopCities = topCitiesRaw3.Select(x => new NrCityRow
            {
                CityName       = x.CityName,
                StateCode      = x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / totalC3 * 100.0
            }).ToList();

            vm.CarrierTopCities = carrierCitiesRaw.Select(x => new NrCityRow
            {
                CityName       = x.CityName,
                StateCode      = x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / carrierTotal3 * 100.0
            }).ToList();

            vm.BrokerTopCities = brokerCitiesRaw.Select(x => new NrCityRow
            {
                CityName       = x.CityName,
                StateCode      = x.StateCode,
                Count          = x.Count,
                PercentOfTotal = (double)x.Count / brokerTotal3 * 100.0
            }).ToList();

            int? lastChanged3 = db.TransportCompanies
                .Where(tc => tc.DateLastChanged != null && tc.DateLastChanged > 19000101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Max(tc => tc.DateLastChanged);
            if (lastChanged3.HasValue)
            {
                int y3 = lastChanged3.Value / 10000;
                int m3 = (lastChanged3.Value / 100) % 100;
                int d3 = lastChanged3.Value % 100;
                try { vm.LastDataUpdate = new DateTime(y3, m3, d3); } catch { vm.LastDataUpdate = null; }
            }

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        public int GetEarliestRegistrationYearMonth()
        {
            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();
            int? minDate = db.TransportCompanies
                .Where(tc => tc.Status == "A" && tc.DateAdded != null && tc.DateAdded >= 20000101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Min(tc => tc.DateAdded);
            int dv = minDate ?? 20000101;
            int yr = dv / 10000;
            int mo = (dv / 100) % 100;
            if (mo < 1 || mo > 12) mo = 1;
            return yr * 100 + mo;
        }

        public StatisticsActiveCompaniesVM GetActiveCompaniesData()
        {
            const string cacheKey = "ActiveCompaniesData_v6";
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsActiveCompaniesVM;
            if (cached != null) { return cached; }

            var vm = new StatisticsActiveCompaniesVM();

            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();

            vm.PureBrokerCount = db.TransportCompanies
                .Count(tc => tc.Status == "A" && tc.EntityType == "B" && usStateCodes.Contains(tc.PhysicalAddressStateCode));
            vm.TotalActiveCompanies = db.TransportCompanies
                .Count(tc => tc.Status == "A" && (tc.EntityType == null || tc.EntityType != "B") && usStateCodes.Contains(tc.PhysicalAddressStateCode));

            vm.StatesRepresented = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && (tc.EntityType == null || tc.EntityType != "B")
                          && tc.PhysicalAddressStateCode != null && tc.PhysicalAddressStateCode != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Select(tc => tc.PhysicalAddressStateCode)
                .Distinct()
                .Count();

            vm.CitiesCount = db.Database.SqlQuery<int>(
                "SELECT COUNT(DISTINCT PhysicalAddressCity) FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND PhysicalAddressCity IS NOT NULL AND PhysicalAddressCity != ''"
            ).FirstOrDefault();

            var topStateRaw = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && (tc.EntityType == null || tc.EntityType != "B")
                          && tc.PhysicalAddressStateCode != null && tc.PhysicalAddressStateCode != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => tc.PhysicalAddressStateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var topStateCodes = topStateRaw.Select(x => x.StateCode).ToList();
            var stateNamesTop = db.States
                .Where(s => topStateCodes.Contains(s.StateCode))
                .ToDictionary(s => s.StateCode, s => s.State1);

            if (topStateRaw.Any())
            {
                var topState = topStateRaw.First();
                vm.TopStateCode = topState.StateCode;
                vm.TopStateCount = topState.Count;
                vm.TopStateName = stateNamesTop.ContainsKey(topState.StateCode) ? stateNamesTop[topState.StateCode] : topState.StateCode;
            }

            vm.TopStates = topStateRaw.Select(x => new StateStatRow
            {
                StateCode = x.StateCode,
                StateName = stateNamesTop.ContainsKey(x.StateCode) ? stateNamesTop[x.StateCode] : x.StateCode,
                CompanyCount = x.Count,
                PercentOfTotal = vm.TotalActiveCompanies > 0
                    ? Math.Round((decimal)x.Count / vm.TotalActiveCompanies * 100, 1)
                    : 0
            }).ToList();

            var topCitiesRaw = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && (tc.EntityType == null || tc.EntityType != "B")
                          && tc.PhysicalAddressCity != null && tc.PhysicalAddressCity != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => new { tc.PhysicalAddressCity, tc.PhysicalAddressStateCode })
                .Select(g => new { City = g.Key.PhysicalAddressCity, StateCode = g.Key.PhysicalAddressStateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            vm.TopCities = topCitiesRaw.Select(x => new CityStatRow
            {
                CityName = x.City,
                StateCode = x.StateCode,
                CompanyCount = x.Count,
                PercentOfTotal = vm.TotalActiveCompanies > 0
                    ? Math.Round((decimal)x.Count / vm.TotalActiveCompanies * 100, 2)
                    : 0
            }).ToList();

            var allStateCountsRaw = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && (tc.EntityType == null || tc.EntityType != "B")
                          && tc.PhysicalAddressStateCode != null && tc.PhysicalAddressStateCode != ""
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => tc.PhysicalAddressStateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .ToList();

            var allStateCountsDict = allStateCountsRaw.ToDictionary(x => x.StateCode, x => x.Count);
            var allStateNameDict = db.States
                .Where(s => s.CountryCode == "US")
                .ToDictionary(s => s.StateCode, s => s.State1);

            vm.AllStatesForMap = usStateCodes.Select(sc => new StateMapRow
            {
                StateCode = sc,
                StateName = allStateNameDict.ContainsKey(sc) ? allStateNameDict[sc] : sc,
                CompanyCount = allStateCountsDict.ContainsKey(sc) ? allStateCountsDict[sc] : 0
            }).OrderBy(x => x.StateName).ToList();

            int? lastChanged = db.TransportCompanies
                .Where(tc => tc.DateLastChanged != null && tc.DateLastChanged > 19000101
                          && usStateCodes.Contains(tc.PhysicalAddressStateCode))
                .Max(tc => tc.DateLastChanged);
            if (lastChanged.HasValue)
            {
                int y = lastChanged.Value / 10000;
                int m = (lastChanged.Value / 100) % 100;
                int d = lastChanged.Value % 100;
                try { vm.LastDataUpdate = new DateTime(y, m, d); } catch { vm.LastDataUpdate = null; }
            }

            // All new aggregate stats in ONE SQL query — avoids 20+ individual round trips
            var acNow = DateTime.Now;
            int acThisMonthStart = acNow.Year * 10000 + acNow.Month * 100 + 1;
            int acThisMonthEnd   = acNow.Year * 10000 + acNow.Month * 100 + 31;
            var acLastM          = acNow.AddMonths(-1);
            int acLastMonthStart = acLastM.Year * 10000 + acLastM.Month * 100 + 1;
            int acLastMonthEnd   = acLastM.Year * 10000 + acLastM.Month * 100 + 31;
            var acPrevM          = acNow.AddMonths(-2);
            int acPrevMonthStart = acPrevM.Year * 10000 + acPrevM.Month * 100 + 1;
            int acPrevMonthEnd   = acPrevM.Year * 10000 + acPrevM.Month * 100 + 31;
            int acThisYearStart  = acNow.Year * 10000 + 101;
            int acLastYearStart  = (acNow.Year - 1) * 10000 + 101;
            int acLastYearEnd    = (acNow.Year - 1) * 10000 + acNow.Month * 100 + 31;
            var acPrior12        = acNow.AddMonths(-24);
            int acPrior12Start   = acPrior12.Year * 10000 + acPrior12.Month * 100 + 1;
            var acPrior12EndM    = acNow.AddMonths(-13);
            int acPrior12End     = acPrior12EndM.Year * 10000 + acPrior12EndM.Month * 100 + 31;

            var acAggSql = @"SELECT
                SUM(CASE WHEN OperationCarrierInterstate='A' THEN 1 ELSE 0 END) AS Interstate,
                SUM(CASE WHEN TotalNumberOfPowerUnits = 1 THEN 1 ELSE 0 END) AS OneUnit,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 2 AND 5 THEN 1 ELSE 0 END) AS TwoToFive,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 6 AND 20 THEN 1 ELSE 0 END) AS SixToTwenty,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 21 AND 100 THEN 1 ELSE 0 END) AS TwentyOneToHundred,
                SUM(CASE WHEN TotalNumberOfPowerUnits > 100 THEN 1 ELSE 0 END) AS OverHundred,
                SUM(CASE WHEN TotalNumberOfPowerUnits > 0 THEN 1 ELSE 0 END) AS TotalReporting,
                SUM(CASE WHEN DateAdded >= @p0 AND DateAdded <= @p1 THEN 1 ELSE 0 END) AS NewThisMonth,
                SUM(CASE WHEN DateAdded >= @p2 AND DateAdded <= @p3 THEN 1 ELSE 0 END) AS NewLastMonth,
                SUM(CASE WHEN DateAdded >= @p4 AND DateAdded <= @p5 THEN 1 ELSE 0 END) AS NewThisYear,
                SUM(CASE WHEN DateAdded >= @p6 AND DateAdded <= @p7 THEN 1 ELSE 0 END) AS NewSamePeriodLastYear,
                SUM(CASE WHEN DateAdded >= @p8 AND DateAdded <= @p9 THEN 1 ELSE 0 END) AS NewPrior12Months
            FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B')";

            var acAgg = db.Database.SqlQuery<AcAggregateCounts>(acAggSql,
                acLastMonthStart, acLastMonthEnd,
                acPrevMonthStart, acPrevMonthEnd,
                acThisYearStart,  acThisMonthEnd,
                acLastYearStart,  acLastYearEnd,
                acPrior12Start,   acPrior12End
            ).FirstOrDefault() ?? new AcAggregateCounts();

            vm.NewThisMonth          = acAgg.NewThisMonth;
            vm.NewLastMonth          = acAgg.NewLastMonth;
            vm.NewThisYear           = acAgg.NewThisYear;
            vm.NewSamePeriodLastYear = acAgg.NewSamePeriodLastYear;
            vm.NewPrior12Months      = acAgg.NewPrior12Months;
            vm.InterstateCarriersCount = acAgg.Interstate;
            vm.InterstateCarriersPct   = vm.TotalActiveCompanies > 0
                ? Math.Round((decimal)acAgg.Interstate / vm.TotalActiveCompanies * 100, 1)
                : 0;

            int acSdTotal = acAgg.TotalReporting;
            vm.MedianFleetSize = 1;
            if (acSdTotal > 0)
            {
                int acMid = acSdTotal / 2;
                if (acAgg.OneUnit >= acMid) vm.MedianFleetSize = 1;
                else if (acAgg.OneUnit + acAgg.TwoToFive >= acMid) vm.MedianFleetSize = 2;
                else if (acAgg.OneUnit + acAgg.TwoToFive + acAgg.SixToTwenty >= acMid) vm.MedianFleetSize = 6;
                else if (acAgg.OneUnit + acAgg.TwoToFive + acAgg.SixToTwenty + acAgg.TwentyOneToHundred >= acMid) vm.MedianFleetSize = 21;
                else vm.MedianFleetSize = 101;
            }
            vm.SizeDistribution = new CompanySizeDistribution
            {
                OneUnit            = acAgg.OneUnit,
                TwoToFive          = acAgg.TwoToFive,
                SixToTwenty        = acAgg.SixToTwenty,
                TwentyOneToHundred = acAgg.TwentyOneToHundred,
                OverHundred        = acAgg.OverHundred,
                TotalReporting     = acSdTotal,
                MedianFleetSize    = vm.MedianFleetSize
            };

            vm.TopCargoTypes = new List<CargoTypeRow>();
            try
            {
                var acCargoSql = @"SELECT
                    SUM(CASE WHEN CargoTransportedAGeneralFreight='X' THEN 1 ELSE 0 END) AS GenFreight,
                    SUM(CASE WHEN CargoTransportedBHouseholdGoods='X' THEN 1 ELSE 0 END) AS HouseholdGoods,
                    SUM(CASE WHEN CargoTransportedCMetalSheetsCoilsRolls='X' THEN 1 ELSE 0 END) AS Metal,
                    SUM(CASE WHEN CargoTransportedDMotorVehicles='X' THEN 1 ELSE 0 END) AS MotorVehicles,
                    SUM(CASE WHEN CargoTransportedEDriveawayTowaway='X' THEN 1 ELSE 0 END) AS Driveaway,
                    SUM(CASE WHEN CargoTransportedFLogsPolesBeamsLumber='X' THEN 1 ELSE 0 END) AS Lumber,
                    SUM(CASE WHEN CargoTransportedGBuildingMaterials='X' THEN 1 ELSE 0 END) AS BuildingMat,
                    SUM(CASE WHEN CargoTransportedHMobileHomes='X' THEN 1 ELSE 0 END) AS MobileHomes,
                    SUM(CASE WHEN CargoTransportedIMachineryLargeObjects='X' THEN 1 ELSE 0 END) AS Machinery,
                    SUM(CASE WHEN CargoTransportedJFreshProduce='X' THEN 1 ELSE 0 END) AS FreshProduce,
                    SUM(CASE WHEN CargoTransportedKLiquidsGases='X' THEN 1 ELSE 0 END) AS Liquids,
                    SUM(CASE WHEN CargoTransportedLIintermodalContainers='X' THEN 1 ELSE 0 END) AS Intermodal,
                    SUM(CASE WHEN CargoTransportedMPassengers='X' THEN 1 ELSE 0 END) AS Passengers,
                    SUM(CASE WHEN CargoTransportedNOilfieldEquipment='X' THEN 1 ELSE 0 END) AS Oilfield,
                    SUM(CASE WHEN CargoTransportedOLivestock='X' THEN 1 ELSE 0 END) AS Livestock,
                    SUM(CASE WHEN CargoTransportedPGrainFeedHay='X' THEN 1 ELSE 0 END) AS Grain,
                    SUM(CASE WHEN CargoTransportedQCoalCoke='X' THEN 1 ELSE 0 END) AS Coal,
                    SUM(CASE WHEN CargoTransportedRMeat='X' THEN 1 ELSE 0 END) AS Meat,
                    SUM(CASE WHEN CargoTransportedSGarbageRefuseTrash='X' THEN 1 ELSE 0 END) AS Garbage,
                    SUM(CASE WHEN CargoTransportedTUSMail='X' THEN 1 ELSE 0 END) AS USMail,
                    SUM(CASE WHEN CargoTransportedUChemicals='X' THEN 1 ELSE 0 END) AS Chemicals,
                    SUM(CASE WHEN CargoTransportedVCommoditiesDryBulk='X' THEN 1 ELSE 0 END) AS DryBulk,
                    SUM(CASE WHEN CargoTransportedWRefrigeratedFood='X' THEN 1 ELSE 0 END) AS RefrigFood,
                    SUM(CASE WHEN CargoTransportedXBeverages='X' THEN 1 ELSE 0 END) AS Beverages,
                    SUM(CASE WHEN CargoTransportedYPaperProducts='X' THEN 1 ELSE 0 END) AS Paper,
                    SUM(CASE WHEN CargoTransportedZUtility='X' THEN 1 ELSE 0 END) AS Utility,
                    SUM(CASE WHEN CargoTransportedAAFarmSupplies='X' THEN 1 ELSE 0 END) AS FarmSupplies,
                    SUM(CASE WHEN CargoTransportedBBConstruction='X' THEN 1 ELSE 0 END) AS Construction,
                    SUM(CASE WHEN CargoTransportedCCWaterWell='X' THEN 1 ELSE 0 END) AS WaterWell,
                    SUM(CASE WHEN CargoTransportedDDOther='X' THEN 1 ELSE 0 END) AS Other
                FROM TransportCompany WHERE Status='A'";
                var acCargo = db.Database.SqlQuery<CargoCounts>(acCargoSql).FirstOrDefault();
                if (acCargo != null)
                {
                    var acRawList = new List<KeyValuePair<string, int>>
                    {
                        new KeyValuePair<string, int>("General Freight",           acCargo.GenFreight),
                        new KeyValuePair<string, int>("Household Goods",           acCargo.HouseholdGoods),
                        new KeyValuePair<string, int>("Building Materials",        acCargo.BuildingMat),
                        new KeyValuePair<string, int>("Motor Vehicles",            acCargo.MotorVehicles),
                        new KeyValuePair<string, int>("Refrigerated Food",         acCargo.RefrigFood),
                        new KeyValuePair<string, int>("Chemicals",                 acCargo.Chemicals),
                        new KeyValuePair<string, int>("Liquids & Gases",           acCargo.Liquids),
                        new KeyValuePair<string, int>("Fresh Produce",             acCargo.FreshProduce),
                        new KeyValuePair<string, int>("Construction",              acCargo.Construction),
                        new KeyValuePair<string, int>("Metal: Sheets & Coils",    acCargo.Metal),
                        new KeyValuePair<string, int>("Intermodal Containers",     acCargo.Intermodal),
                        new KeyValuePair<string, int>("Machinery & Large Objects", acCargo.Machinery),
                        new KeyValuePair<string, int>("Dry Bulk Commodities",      acCargo.DryBulk),
                        new KeyValuePair<string, int>("Paper Products",            acCargo.Paper),
                        new KeyValuePair<string, int>("Beverages",                 acCargo.Beverages),
                        new KeyValuePair<string, int>("Livestock",                 acCargo.Livestock)
                    };
                    vm.TopCargoTypes = acRawList
                        .Where(x => x.Value > 0)
                        .OrderByDescending(x => x.Value)
                        .Take(8)
                        .Select(x => new CargoTypeRow
                        {
                            CargoTypeName  = x.Key,
                            CompanyCount   = x.Value,
                            PercentOfTotal = vm.TotalActiveCompanies > 0
                                ? Math.Round((decimal)x.Value / vm.TotalActiveCompanies * 100, 1)
                                : 0
                        }).ToList();
                }
            }
            catch { }

            // Monthly registrations — ONE GROUP BY query instead of 12 individual queries
            vm.MonthlyRegistrations = new List<MonthlyRegistrationRow>();
            int acMoStart = acNow.AddMonths(-24).Year * 10000 + acNow.AddMonths(-24).Month * 100 + 1;
            int acLast12Start = acNow.AddMonths(-12).Year * 10000 + acNow.AddMonths(-12).Month * 100 + 1;
            int acMoCurStart = acThisMonthStart;
            var acMoSql = "SELECT DateAdded/100 AS YearMonth, COUNT(*) AS Cnt FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND DateAdded >= @p0 AND DateAdded < @p1 GROUP BY DateAdded/100 ORDER BY DateAdded/100";
            var acMoRows = db.Database.SqlQuery<AcMonthlyCount>(acMoSql, acMoStart, acMoCurStart).ToList();
            var acMoDict = acMoRows.ToDictionary(x => x.YearMonth, x => x.Cnt);
            for (int i = 24; i >= 1; i--)
            {
                var mDate = acNow.AddMonths(-i);
                int ym = mDate.Year * 100 + mDate.Month;
                vm.MonthlyRegistrations.Add(new MonthlyRegistrationRow
                {
                    Year  = mDate.Year,
                    Month = mDate.Month,
                    Count = acMoDict.ContainsKey(ym) ? acMoDict[ym] : 0,
                    Label = mDate.ToString("MMM yy")
                });
            }

            vm.TopCompaniesByPowerUnits = new List<TopCompanyRow>();
            try
            {
                var topCompSql = @"SELECT TOP 10 LegalName AS CompanyName, TotalNumberOfPowerUnits AS PowerUnits, PhysicalAddressCity AS City, PhysicalAddressStateCode AS StateCode
                    FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND TotalNumberOfPowerUnits > 0 ORDER BY TotalNumberOfPowerUnits DESC";
                vm.TopCompaniesByPowerUnits = db.Database.SqlQuery<TopCompanyRow>(topCompSql).ToList();
            }
            catch { }

            vm.TopStatesByNewRegistrations = new List<StateNewRegRow>();
            try
            {
                var stateNewRegSql = @"SELECT TOP 10 PhysicalAddressStateCode AS StateCode, COUNT(*) AS NewRegistrations FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND DateAdded >= @p0 AND DateAdded < @p1 AND PhysicalAddressStateCode IS NOT NULL AND PhysicalAddressStateCode != '' GROUP BY PhysicalAddressStateCode ORDER BY COUNT(*) DESC";
                var stateNewRegRaw = db.Database.SqlQuery<AcStateNewReg>(stateNewRegSql, acLast12Start, acThisMonthStart).ToList();
                int totalNewReg12 = stateNewRegRaw.Sum(x => x.NewRegistrations);
                vm.TopStatesByNewRegistrations = stateNewRegRaw.Select(x => new StateNewRegRow
                {
                    StateCode       = x.StateCode,
                    StateName       = allStateNameDict.ContainsKey(x.StateCode) ? allStateNameDict[x.StateCode] : x.StateCode,
                    NewRegistrations = x.NewRegistrations,
                    PercentOfTotal  = totalNewReg12 > 0 ? Math.Round((decimal)x.NewRegistrations / totalNewReg12 * 100, 1) : 0
                }).ToList();
            }
            catch { }

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        private class FoMainAgg
        {
            public int TotalCount { get; set; }
            public long PowerUnitsSum { get; set; }
            public long OwnedTrucks { get; set; }
            public long OwnedTractors { get; set; }
            public long LeasedTrucks { get; set; }
            public long LeasedTractors { get; set; }
            public int OwnerOperators { get; set; }
            public int Reporting { get; set; }
            public int Bucket1Count { get; set; }
            public int Bucket2Count { get; set; }
            public int Bucket3Count { get; set; }
            public int Bucket4Count { get; set; }
            public int Bucket5Count { get; set; }
            public long Bucket1PU { get; set; }
            public long Bucket2PU { get; set; }
            public long Bucket3PU { get; set; }
            public long Bucket4PU { get; set; }
            public long Bucket5PU { get; set; }
            public int InterstateCount { get; set; }
            public long InterstatePU { get; set; }
            public int IntrastateCount { get; set; }
            public long IntrastatePU { get; set; }
            public int HazmatCount { get; set; }
            public long HazmatPU { get; set; }
            public int? MaxDLC { get; set; }
        }

        private class FoStatePURow
        {
            public string PhysicalAddressStateCode { get; set; }
            public long PU { get; set; }
            public int Reporting { get; set; }
        }

        public StatisticsFleetOperationsVM GetFleetOperationsData()
        {
            const string cacheKey = "FleetOperationsData_v3";
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsFleetOperationsVM;
            if (cached != null) { return cached; }

            var vm = new StatisticsFleetOperationsVM();

            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();

            string foUsIn = "'" + string.Join("','", usStateCodes) + "'";

            vm.TotalActiveCompanies = db.TransportCompanies
                .Count(tc => tc.Status == "A" && (tc.EntityType == null || tc.EntityType != "B") && usStateCodes.Contains(tc.PhysicalAddressStateCode));

            var foAgg = db.Database.SqlQuery<FoMainAgg>(@"SELECT
                COUNT(*) AS TotalCount,
                CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS PowerUnitsSum,
                CAST(ISNULL(SUM(CASE WHEN NNEquipmentUnitsOwnedTruck    > 0 THEN CAST(NNEquipmentUnitsOwnedTruck    AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS OwnedTrucks,
                CAST(ISNULL(SUM(CASE WHEN NNEquipmentUnitsOwnedTractor  > 0 THEN CAST(NNEquipmentUnitsOwnedTractor  AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS OwnedTractors,
                CAST(ISNULL(SUM(CASE WHEN NNEquipmentUnitsTermLeasedTruck    > 0 THEN CAST(NNEquipmentUnitsTermLeasedTruck    AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS LeasedTrucks,
                CAST(ISNULL(SUM(CASE WHEN NNEquipmentUnitsTermLeasedTractor  > 0 THEN CAST(NNEquipmentUnitsTermLeasedTractor  AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS LeasedTractors,
                SUM(CASE WHEN TotalNumberOfPowerUnits = 1 THEN 1 ELSE 0 END) AS OwnerOperators,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS Reporting,
                SUM(CASE WHEN TotalNumberOfPowerUnits = 1 THEN 1 ELSE 0 END) AS Bucket1Count,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 2 AND 5 THEN 1 ELSE 0 END) AS Bucket2Count,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 6 AND 20 THEN 1 ELSE 0 END) AS Bucket3Count,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 21 AND 100 THEN 1 ELSE 0 END) AS Bucket4Count,
                SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 101 AND 50000 THEN 1 ELSE 0 END) AS Bucket5Count,
                CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits = 1 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS Bucket1PU,
                CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 2 AND 5 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS Bucket2PU,
                CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 6 AND 20 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS Bucket3PU,
                CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 21 AND 100 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS Bucket4PU,
                CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 101 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS Bucket5PU,
                SUM(CASE WHEN OperationCarrierInterstate='A' THEN 1 ELSE 0 END) AS InterstateCount,
                CAST(ISNULL(SUM(CASE WHEN OperationCarrierInterstate='A' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS InterstatePU,
                SUM(CASE WHEN (OperationCarrierIntrastateHazmat='B' OR OperationCarrierIntrastateNonHazmat='C') AND (OperationCarrierInterstate IS NULL OR OperationCarrierInterstate='') THEN 1 ELSE 0 END) AS IntrastateCount,
                CAST(ISNULL(SUM(CASE WHEN (OperationCarrierIntrastateHazmat='B' OR OperationCarrierIntrastateNonHazmat='C') AND (OperationCarrierInterstate IS NULL OR OperationCarrierInterstate='') AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS IntrastatePU,
                SUM(CASE WHEN HazmatIndicator='Y' THEN 1 ELSE 0 END) AS HazmatCount,
                CAST(ISNULL(SUM(CASE WHEN HazmatIndicator='Y' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS HazmatPU,
                MAX(CASE WHEN DateLastChanged > 19000101 THEN DateLastChanged ELSE NULL END) AS MaxDLC
            FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND PhysicalAddressStateCode IN (" + foUsIn + ")").First();

            vm.TotalPowerUnits    = foAgg.PowerUnitsSum;
            vm.ReportingCount     = foAgg.Reporting;
            vm.NonReportingCount  = vm.TotalActiveCompanies - foAgg.Reporting;
            vm.AvgFleetSize       = foAgg.Reporting > 0 ? Math.Round((double)foAgg.PowerUnitsSum / foAgg.Reporting, 2) : 0;
            vm.OwnedTrucks        = foAgg.OwnedTrucks;
            vm.OwnedTractors      = foAgg.OwnedTractors;
            vm.LeasedTrucks       = foAgg.LeasedTrucks;
            vm.LeasedTractors     = foAgg.LeasedTractors;
            vm.TotalTrucksAndTractors = foAgg.OwnedTrucks + foAgg.OwnedTractors + foAgg.LeasedTrucks + foAgg.LeasedTractors;
            vm.OwnerOperatorCount = foAgg.OwnerOperators;
            vm.InterstateCount    = foAgg.InterstateCount;
            vm.InterstatePowerUnits = foAgg.InterstatePU;
            vm.IntrastateCount    = foAgg.IntrastateCount;
            vm.IntrastatePowerUnits = foAgg.IntrastatePU;
            vm.HazmatCount        = foAgg.HazmatCount;
            vm.HazmatPowerUnits   = foAgg.HazmatPU;

            if (foAgg.MaxDLC.HasValue)
            {
                int y = foAgg.MaxDLC.Value / 10000; int m2 = (foAgg.MaxDLC.Value / 100) % 100; int d2 = foAgg.MaxDLC.Value % 100;
                try { vm.LastDataUpdate = new DateTime(y, m2, d2); } catch { vm.LastDataUpdate = null; }
            }

            vm.FleetBuckets = new System.Collections.Generic.List<FoFleetBucket>
            {
                new FoFleetBucket { Label = "Owner-Operator (1 unit)",  CompanyCount = foAgg.Bucket1Count, PowerUnitsSum = foAgg.Bucket1PU },
                new FoFleetBucket { Label = "Small Fleet (2–5)",   CompanyCount = foAgg.Bucket2Count, PowerUnitsSum = foAgg.Bucket2PU },
                new FoFleetBucket { Label = "Medium Fleet (6–20)", CompanyCount = foAgg.Bucket3Count, PowerUnitsSum = foAgg.Bucket3PU },
                new FoFleetBucket { Label = "Large Fleet (21–100)",CompanyCount = foAgg.Bucket4Count, PowerUnitsSum = foAgg.Bucket4PU },
                new FoFleetBucket { Label = "Very Large Fleet (101+)",  CompanyCount = foAgg.Bucket5Count, PowerUnitsSum = foAgg.Bucket5PU }
            };

            var allStateNameDict = db.States.Where(s => s.CountryCode == "US").ToDictionary(s => s.StateCode, s => s.State1);

            var foPURows = db.Database.SqlQuery<FoStatePURow>("SELECT TOP 10 PhysicalAddressStateCode," +
                " CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS PU," +
                " SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS Reporting" +
                " FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND PhysicalAddressStateCode IN (" + foUsIn + ")" +
                " GROUP BY PhysicalAddressStateCode ORDER BY PU DESC").ToList();

            vm.TopStatesByPowerUnits = foPURows.Select(r => new FoStateRow
            {
                StateCode = r.PhysicalAddressStateCode,
                StateName = allStateNameDict.ContainsKey(r.PhysicalAddressStateCode) ? allStateNameDict[r.PhysicalAddressStateCode] : r.PhysicalAddressStateCode,
                TotalPowerUnits = r.PU,
                ReportingCount  = r.Reporting,
                AvgFleetSize    = r.Reporting > 0 ? Math.Round((double)r.PU / r.Reporting, 2) : 0
            }).ToList();

            var foAvgRows = db.Database.SqlQuery<FoStatePURow>("SELECT TOP 10 PhysicalAddressStateCode," +
                " CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END), 0) AS BIGINT) AS PU," +
                " SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS Reporting" +
                " FROM TransportCompany WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND PhysicalAddressStateCode IN (" + foUsIn + ")" +
                " GROUP BY PhysicalAddressStateCode" +
                " HAVING SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) >= 100" +
                " ORDER BY CAST(ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN TotalNumberOfPowerUnits ELSE 0 END), 0) AS FLOAT) / NULLIF(SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END), 0) DESC").ToList();

            vm.TopStatesByAvgFleet = foAvgRows.Select(r => new FoStateRow
            {
                StateCode = r.PhysicalAddressStateCode,
                StateName = allStateNameDict.ContainsKey(r.PhysicalAddressStateCode) ? allStateNameDict[r.PhysicalAddressStateCode] : r.PhysicalAddressStateCode,
                TotalPowerUnits = r.PU,
                ReportingCount  = r.Reporting,
                AvgFleetSize    = r.Reporting > 0 ? Math.Round((double)r.PU / r.Reporting, 2) : 0
            }).ToList();

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        private class CgMainAgg
        {
            public int TotalCount { get; set; }
            public int GenFreightCount { get; set; }     public long GenFreightPU { get; set; }     public int GenFreightRep { get; set; }
            public int HouseholdGoodsCount { get; set; } public long HouseholdGoodsPU { get; set; } public int HouseholdGoodsRep { get; set; }
            public int MetalCount { get; set; }          public long MetalPU { get; set; }          public int MetalRep { get; set; }
            public int MotorVehiclesCount { get; set; }  public long MotorVehiclesPU { get; set; }  public int MotorVehiclesRep { get; set; }
            public int DriveawayCount { get; set; }      public long DriveawayPU { get; set; }      public int DriveawayRep { get; set; }
            public int LumberCount { get; set; }         public long LumberPU { get; set; }         public int LumberRep { get; set; }
            public int BuildingMatCount { get; set; }    public long BuildingMatPU { get; set; }    public int BuildingMatRep { get; set; }
            public int MobileHomesCount { get; set; }    public long MobileHomesPU { get; set; }    public int MobileHomesRep { get; set; }
            public int MachineryCount { get; set; }      public long MachineryPU { get; set; }      public int MachineryRep { get; set; }
            public int FreshProduceCount { get; set; }   public long FreshProducePU { get; set; }   public int FreshProduceRep { get; set; }
            public int LiquidsCount { get; set; }        public long LiquidsPU { get; set; }        public int LiquidsRep { get; set; }
            public int IntermodalCount { get; set; }     public long IntermodalPU { get; set; }     public int IntermodalRep { get; set; }
            public int PassengersCount { get; set; }     public long PassengersPU { get; set; }     public int PassengersRep { get; set; }
            public int OilfieldCount { get; set; }       public long OilfieldPU { get; set; }       public int OilfieldRep { get; set; }
            public int LivestockCount { get; set; }      public long LivestockPU { get; set; }      public int LivestockRep { get; set; }
            public int GrainCount { get; set; }          public long GrainPU { get; set; }          public int GrainRep { get; set; }
            public int CoalCount { get; set; }           public long CoalPU { get; set; }           public int CoalRep { get; set; }
            public int MeatCount { get; set; }           public long MeatPU { get; set; }           public int MeatRep { get; set; }
            public int GarbageCount { get; set; }        public long GarbagePU { get; set; }        public int GarbageRep { get; set; }
            public int USMailCount { get; set; }         public long USMailPU { get; set; }         public int USMailRep { get; set; }
            public int ChemicalsCount { get; set; }      public long ChemicalsPU { get; set; }      public int ChemicalsRep { get; set; }
            public int DryBulkCount { get; set; }        public long DryBulkPU { get; set; }        public int DryBulkRep { get; set; }
            public int RefrigFoodCount { get; set; }     public long RefrigFoodPU { get; set; }     public int RefrigFoodRep { get; set; }
            public int BeveragesCount { get; set; }      public long BeveragesPU { get; set; }      public int BeveragesRep { get; set; }
            public int PaperCount { get; set; }          public long PaperPU { get; set; }          public int PaperRep { get; set; }
            public int UtilityCount { get; set; }        public long UtilityPU { get; set; }        public int UtilityRep { get; set; }
            public int FarmSuppliesCount { get; set; }   public long FarmSuppliesPU { get; set; }   public int FarmSuppliesRep { get; set; }
            public int ConstructionCount { get; set; }   public long ConstructionPU { get; set; }   public int ConstructionRep { get; set; }
            public int WaterWellCount { get; set; }      public long WaterWellPU { get; set; }      public int WaterWellRep { get; set; }
            public int OtherCount { get; set; }          public long OtherPU { get; set; }          public int OtherRep { get; set; }
            public int ZeroCargo { get; set; }
            public int ExactlyOne { get; set; }
            public int TwoToThree { get; set; }
            public int FourToSix { get; set; }
            public int SevenPlus { get; set; }
            public int TotalSelections { get; set; }
            public int? MaxDLC { get; set; }
        }

        public StatisticsCargoVM GetCargoData()
        {
            const string cacheKey = "CargoData_v2";
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsCargoVM;
            if (cached != null) { return cached; }

            var vm = new StatisticsCargoVM();

            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();

            string cgUsIn = "'" + string.Join("','", usStateCodes) + "'";

            string cgSql = @"WITH R AS (
    SELECT TotalNumberOfPowerUnits, DateLastChanged,
        CargoTransportedAGeneralFreight, CargoTransportedBHouseholdGoods,
        CargoTransportedCMetalSheetsCoilsRolls, CargoTransportedDMotorVehicles,
        CargoTransportedEDriveawayTowaway, CargoTransportedFLogsPolesBeamsLumber,
        CargoTransportedGBuildingMaterials, CargoTransportedHMobileHomes,
        CargoTransportedIMachineryLargeObjects, CargoTransportedJFreshProduce,
        CargoTransportedKLiquidsGases, CargoTransportedLIintermodalContainers,
        CargoTransportedMPassengers, CargoTransportedNOilfieldEquipment,
        CargoTransportedOLivestock, CargoTransportedPGrainFeedHay,
        CargoTransportedQCoalCoke, CargoTransportedRMeat,
        CargoTransportedSGarbageRefuseTrash, CargoTransportedTUSMail,
        CargoTransportedUChemicals, CargoTransportedVCommoditiesDryBulk,
        CargoTransportedWRefrigeratedFood, CargoTransportedXBeverages,
        CargoTransportedYPaperProducts, CargoTransportedZUtility,
        CargoTransportedAAFarmSupplies, CargoTransportedBBConstruction,
        CargoTransportedCCWaterWell, CargoTransportedDDOther,
        (CASE WHEN CargoTransportedAGeneralFreight='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedBHouseholdGoods='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedCMetalSheetsCoilsRolls='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedDMotorVehicles='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedEDriveawayTowaway='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedFLogsPolesBeamsLumber='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedGBuildingMaterials='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedHMobileHomes='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedIMachineryLargeObjects='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedJFreshProduce='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedKLiquidsGases='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedLIintermodalContainers='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedMPassengers='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedNOilfieldEquipment='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedOLivestock='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedPGrainFeedHay='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedQCoalCoke='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedRMeat='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedSGarbageRefuseTrash='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedTUSMail='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedUChemicals='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedVCommoditiesDryBulk='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedWRefrigeratedFood='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedXBeverages='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedYPaperProducts='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedZUtility='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedAAFarmSupplies='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedBBConstruction='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedCCWaterWell='X' THEN 1 ELSE 0 END
        +CASE WHEN CargoTransportedDDOther='X' THEN 1 ELSE 0 END) AS CC
    FROM TransportCompany
    WHERE Status='A' AND (EntityType IS NULL OR EntityType != 'B') AND PhysicalAddressStateCode IN (" + cgUsIn + @")
)
SELECT COUNT(*) AS TotalCount,
SUM(CASE WHEN CargoTransportedAGeneralFreight='X' THEN 1 ELSE 0 END) AS GenFreightCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedAGeneralFreight='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS GenFreightPU,
SUM(CASE WHEN CargoTransportedAGeneralFreight='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS GenFreightRep,
SUM(CASE WHEN CargoTransportedBHouseholdGoods='X' THEN 1 ELSE 0 END) AS HouseholdGoodsCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedBHouseholdGoods='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS HouseholdGoodsPU,
SUM(CASE WHEN CargoTransportedBHouseholdGoods='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS HouseholdGoodsRep,
SUM(CASE WHEN CargoTransportedCMetalSheetsCoilsRolls='X' THEN 1 ELSE 0 END) AS MetalCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedCMetalSheetsCoilsRolls='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS MetalPU,
SUM(CASE WHEN CargoTransportedCMetalSheetsCoilsRolls='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS MetalRep,
SUM(CASE WHEN CargoTransportedDMotorVehicles='X' THEN 1 ELSE 0 END) AS MotorVehiclesCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedDMotorVehicles='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS MotorVehiclesPU,
SUM(CASE WHEN CargoTransportedDMotorVehicles='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS MotorVehiclesRep,
SUM(CASE WHEN CargoTransportedEDriveawayTowaway='X' THEN 1 ELSE 0 END) AS DriveawayCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedEDriveawayTowaway='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS DriveawayPU,
SUM(CASE WHEN CargoTransportedEDriveawayTowaway='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS DriveawayRep,
SUM(CASE WHEN CargoTransportedFLogsPolesBeamsLumber='X' THEN 1 ELSE 0 END) AS LumberCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedFLogsPolesBeamsLumber='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS LumberPU,
SUM(CASE WHEN CargoTransportedFLogsPolesBeamsLumber='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS LumberRep,
SUM(CASE WHEN CargoTransportedGBuildingMaterials='X' THEN 1 ELSE 0 END) AS BuildingMatCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedGBuildingMaterials='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS BuildingMatPU,
SUM(CASE WHEN CargoTransportedGBuildingMaterials='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS BuildingMatRep,
SUM(CASE WHEN CargoTransportedHMobileHomes='X' THEN 1 ELSE 0 END) AS MobileHomesCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedHMobileHomes='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS MobileHomesPU,
SUM(CASE WHEN CargoTransportedHMobileHomes='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS MobileHomesRep,
SUM(CASE WHEN CargoTransportedIMachineryLargeObjects='X' THEN 1 ELSE 0 END) AS MachineryCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedIMachineryLargeObjects='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS MachineryPU,
SUM(CASE WHEN CargoTransportedIMachineryLargeObjects='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS MachineryRep,
SUM(CASE WHEN CargoTransportedJFreshProduce='X' THEN 1 ELSE 0 END) AS FreshProduceCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedJFreshProduce='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS FreshProducePU,
SUM(CASE WHEN CargoTransportedJFreshProduce='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS FreshProduceRep,
SUM(CASE WHEN CargoTransportedKLiquidsGases='X' THEN 1 ELSE 0 END) AS LiquidsCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedKLiquidsGases='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS LiquidsPU,
SUM(CASE WHEN CargoTransportedKLiquidsGases='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS LiquidsRep,
SUM(CASE WHEN CargoTransportedLIintermodalContainers='X' THEN 1 ELSE 0 END) AS IntermodalCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedLIintermodalContainers='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS IntermodalPU,
SUM(CASE WHEN CargoTransportedLIintermodalContainers='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS IntermodalRep,
SUM(CASE WHEN CargoTransportedMPassengers='X' THEN 1 ELSE 0 END) AS PassengersCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedMPassengers='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS PassengersPU,
SUM(CASE WHEN CargoTransportedMPassengers='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS PassengersRep,
SUM(CASE WHEN CargoTransportedNOilfieldEquipment='X' THEN 1 ELSE 0 END) AS OilfieldCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedNOilfieldEquipment='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS OilfieldPU,
SUM(CASE WHEN CargoTransportedNOilfieldEquipment='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS OilfieldRep,
SUM(CASE WHEN CargoTransportedOLivestock='X' THEN 1 ELSE 0 END) AS LivestockCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedOLivestock='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS LivestockPU,
SUM(CASE WHEN CargoTransportedOLivestock='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS LivestockRep,
SUM(CASE WHEN CargoTransportedPGrainFeedHay='X' THEN 1 ELSE 0 END) AS GrainCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedPGrainFeedHay='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS GrainPU,
SUM(CASE WHEN CargoTransportedPGrainFeedHay='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS GrainRep,
SUM(CASE WHEN CargoTransportedQCoalCoke='X' THEN 1 ELSE 0 END) AS CoalCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedQCoalCoke='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS CoalPU,
SUM(CASE WHEN CargoTransportedQCoalCoke='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS CoalRep,
SUM(CASE WHEN CargoTransportedRMeat='X' THEN 1 ELSE 0 END) AS MeatCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedRMeat='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS MeatPU,
SUM(CASE WHEN CargoTransportedRMeat='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS MeatRep,
SUM(CASE WHEN CargoTransportedSGarbageRefuseTrash='X' THEN 1 ELSE 0 END) AS GarbageCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedSGarbageRefuseTrash='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS GarbagePU,
SUM(CASE WHEN CargoTransportedSGarbageRefuseTrash='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS GarbageRep,
SUM(CASE WHEN CargoTransportedTUSMail='X' THEN 1 ELSE 0 END) AS USMailCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedTUSMail='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS USMailPU,
SUM(CASE WHEN CargoTransportedTUSMail='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS USMailRep,
SUM(CASE WHEN CargoTransportedUChemicals='X' THEN 1 ELSE 0 END) AS ChemicalsCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedUChemicals='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS ChemicalsPU,
SUM(CASE WHEN CargoTransportedUChemicals='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS ChemicalsRep,
SUM(CASE WHEN CargoTransportedVCommoditiesDryBulk='X' THEN 1 ELSE 0 END) AS DryBulkCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedVCommoditiesDryBulk='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS DryBulkPU,
SUM(CASE WHEN CargoTransportedVCommoditiesDryBulk='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS DryBulkRep,
SUM(CASE WHEN CargoTransportedWRefrigeratedFood='X' THEN 1 ELSE 0 END) AS RefrigFoodCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedWRefrigeratedFood='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS RefrigFoodPU,
SUM(CASE WHEN CargoTransportedWRefrigeratedFood='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS RefrigFoodRep,
SUM(CASE WHEN CargoTransportedXBeverages='X' THEN 1 ELSE 0 END) AS BeveragesCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedXBeverages='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS BeveragesPU,
SUM(CASE WHEN CargoTransportedXBeverages='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS BeveragesRep,
SUM(CASE WHEN CargoTransportedYPaperProducts='X' THEN 1 ELSE 0 END) AS PaperCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedYPaperProducts='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS PaperPU,
SUM(CASE WHEN CargoTransportedYPaperProducts='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS PaperRep,
SUM(CASE WHEN CargoTransportedZUtility='X' THEN 1 ELSE 0 END) AS UtilityCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedZUtility='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS UtilityPU,
SUM(CASE WHEN CargoTransportedZUtility='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS UtilityRep,
SUM(CASE WHEN CargoTransportedAAFarmSupplies='X' THEN 1 ELSE 0 END) AS FarmSuppliesCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedAAFarmSupplies='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS FarmSuppliesPU,
SUM(CASE WHEN CargoTransportedAAFarmSupplies='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS FarmSuppliesRep,
SUM(CASE WHEN CargoTransportedBBConstruction='X' THEN 1 ELSE 0 END) AS ConstructionCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedBBConstruction='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS ConstructionPU,
SUM(CASE WHEN CargoTransportedBBConstruction='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS ConstructionRep,
SUM(CASE WHEN CargoTransportedCCWaterWell='X' THEN 1 ELSE 0 END) AS WaterWellCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedCCWaterWell='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS WaterWellPU,
SUM(CASE WHEN CargoTransportedCCWaterWell='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS WaterWellRep,
SUM(CASE WHEN CargoTransportedDDOther='X' THEN 1 ELSE 0 END) AS OtherCount,
CAST(ISNULL(SUM(CASE WHEN CargoTransportedDDOther='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN CAST(TotalNumberOfPowerUnits AS BIGINT) ELSE 0 END),0) AS BIGINT) AS OtherPU,
SUM(CASE WHEN CargoTransportedDDOther='X' AND TotalNumberOfPowerUnits BETWEEN 1 AND 50000 THEN 1 ELSE 0 END) AS OtherRep,
SUM(CASE WHEN CC=0 THEN 1 ELSE 0 END) AS ZeroCargo,
SUM(CASE WHEN CC=1 THEN 1 ELSE 0 END) AS ExactlyOne,
SUM(CASE WHEN CC BETWEEN 2 AND 3 THEN 1 ELSE 0 END) AS TwoToThree,
SUM(CASE WHEN CC BETWEEN 4 AND 6 THEN 1 ELSE 0 END) AS FourToSix,
SUM(CASE WHEN CC>=7 THEN 1 ELSE 0 END) AS SevenPlus,
SUM(CC) AS TotalSelections,
MAX(CASE WHEN DateLastChanged>19000101 THEN DateLastChanged ELSE NULL END) AS MaxDLC
FROM R";

            var cgAgg = db.Database.SqlQuery<CgMainAgg>(cgSql).First();

            vm.TotalActiveCompanies = cgAgg.TotalCount;
            vm.CompaniesWithNoCargo = cgAgg.ZeroCargo;
            vm.CompaniesWithAnyCargo = cgAgg.TotalCount - cgAgg.ZeroCargo;
            vm.TotalCargoSelections = cgAgg.TotalSelections;
            vm.AvgCargoTypesPerCompany = vm.CompaniesWithAnyCargo > 0
                ? Math.Round((double)cgAgg.TotalSelections / vm.CompaniesWithAnyCargo, 1)
                : 0;

            if (cgAgg.MaxDLC.HasValue)
            {
                int cy = cgAgg.MaxDLC.Value / 10000; int cm = (cgAgg.MaxDLC.Value / 100) % 100; int cd = cgAgg.MaxDLC.Value % 100;
                try { vm.LastDataUpdate = new DateTime(cy, cm, cd); } catch { vm.LastDataUpdate = null; }
            }

            var cgRaw = new[]
            {
                new { Label = "General Freight",           Count = cgAgg.GenFreightCount,     PU = cgAgg.GenFreightPU,     Rep = cgAgg.GenFreightRep },
                new { Label = "Household Goods",           Count = cgAgg.HouseholdGoodsCount, PU = cgAgg.HouseholdGoodsPU, Rep = cgAgg.HouseholdGoodsRep },
                new { Label = "Metal: Sheets & Coils",     Count = cgAgg.MetalCount,          PU = cgAgg.MetalPU,          Rep = cgAgg.MetalRep },
                new { Label = "Motor Vehicles",            Count = cgAgg.MotorVehiclesCount,  PU = cgAgg.MotorVehiclesPU,  Rep = cgAgg.MotorVehiclesRep },
                new { Label = "Driveaway/Towaway",         Count = cgAgg.DriveawayCount,      PU = cgAgg.DriveawayPU,      Rep = cgAgg.DriveawayRep },
                new { Label = "Logs/Poles/Beams/Lumber",   Count = cgAgg.LumberCount,         PU = cgAgg.LumberPU,         Rep = cgAgg.LumberRep },
                new { Label = "Building Materials",        Count = cgAgg.BuildingMatCount,    PU = cgAgg.BuildingMatPU,    Rep = cgAgg.BuildingMatRep },
                new { Label = "Mobile Homes",              Count = cgAgg.MobileHomesCount,    PU = cgAgg.MobileHomesPU,    Rep = cgAgg.MobileHomesRep },
                new { Label = "Machinery & Large Objects", Count = cgAgg.MachineryCount,      PU = cgAgg.MachineryPU,      Rep = cgAgg.MachineryRep },
                new { Label = "Fresh Produce",             Count = cgAgg.FreshProduceCount,   PU = cgAgg.FreshProducePU,   Rep = cgAgg.FreshProduceRep },
                new { Label = "Liquids & Gases",           Count = cgAgg.LiquidsCount,        PU = cgAgg.LiquidsPU,        Rep = cgAgg.LiquidsRep },
                new { Label = "Intermodal Containers",     Count = cgAgg.IntermodalCount,     PU = cgAgg.IntermodalPU,     Rep = cgAgg.IntermodalRep },
                new { Label = "Passengers",                Count = cgAgg.PassengersCount,     PU = cgAgg.PassengersPU,     Rep = cgAgg.PassengersRep },
                new { Label = "Oilfield Equipment",        Count = cgAgg.OilfieldCount,       PU = cgAgg.OilfieldPU,       Rep = cgAgg.OilfieldRep },
                new { Label = "Livestock",                 Count = cgAgg.LivestockCount,      PU = cgAgg.LivestockPU,      Rep = cgAgg.LivestockRep },
                new { Label = "Grain/Feed/Hay",            Count = cgAgg.GrainCount,          PU = cgAgg.GrainPU,          Rep = cgAgg.GrainRep },
                new { Label = "Coal/Coke",                 Count = cgAgg.CoalCount,           PU = cgAgg.CoalPU,           Rep = cgAgg.CoalRep },
                new { Label = "Meat",                      Count = cgAgg.MeatCount,           PU = cgAgg.MeatPU,           Rep = cgAgg.MeatRep },
                new { Label = "Garbage/Refuse/Trash",      Count = cgAgg.GarbageCount,        PU = cgAgg.GarbagePU,        Rep = cgAgg.GarbageRep },
                new { Label = "U.S. Mail",                 Count = cgAgg.USMailCount,         PU = cgAgg.USMailPU,         Rep = cgAgg.USMailRep },
                new { Label = "Chemicals",                 Count = cgAgg.ChemicalsCount,      PU = cgAgg.ChemicalsPU,      Rep = cgAgg.ChemicalsRep },
                new { Label = "Dry Bulk Commodities",      Count = cgAgg.DryBulkCount,        PU = cgAgg.DryBulkPU,        Rep = cgAgg.DryBulkRep },
                new { Label = "Refrigerated Food",         Count = cgAgg.RefrigFoodCount,     PU = cgAgg.RefrigFoodPU,     Rep = cgAgg.RefrigFoodRep },
                new { Label = "Beverages",                 Count = cgAgg.BeveragesCount,      PU = cgAgg.BeveragesPU,      Rep = cgAgg.BeveragesRep },
                new { Label = "Paper Products",            Count = cgAgg.PaperCount,          PU = cgAgg.PaperPU,          Rep = cgAgg.PaperRep },
                new { Label = "Utility",                   Count = cgAgg.UtilityCount,        PU = cgAgg.UtilityPU,        Rep = cgAgg.UtilityRep },
                new { Label = "Farm Supplies",             Count = cgAgg.FarmSuppliesCount,   PU = cgAgg.FarmSuppliesPU,   Rep = cgAgg.FarmSuppliesRep },
                new { Label = "Construction",              Count = cgAgg.ConstructionCount,   PU = cgAgg.ConstructionPU,   Rep = cgAgg.ConstructionRep },
                new { Label = "Water Well",                Count = cgAgg.WaterWellCount,      PU = cgAgg.WaterWellPU,      Rep = cgAgg.WaterWellRep }
            };

            vm.AllCargoTypes = cgRaw.Select(r => new CgCargoTypeRow
            {
                Label          = r.Label,
                CompanyCount   = r.Count,
                PowerUnitsSum  = r.PU,
                ReportingWithPU = r.Rep,
                AvgFleetSize   = r.Rep > 0 ? Math.Round((double)r.PU / r.Rep, 2) : 0
            }).OrderByDescending(x => x.CompanyCount).ToList();

            vm.SpecBuckets = new List<CargoSpecBucket>
            {
                new CargoSpecBucket { Label = "1 Cargo Type",  Count = cgAgg.ExactlyOne },
                new CargoSpecBucket { Label = "2–3 Types", Count = cgAgg.TwoToThree },
                new CargoSpecBucket { Label = "4–6 Types", Count = cgAgg.FourToSix },
                new CargoSpecBucket { Label = "7+ Types",      Count = cgAgg.SevenPlus }
            };

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        public StatisticsActiveBrokersVM GetActiveBrokersData()
        {
            const string cacheKey = "ActiveBrokersData_v1";
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsActiveBrokersVM;
            if (cached != null) { return cached; }

            var vm = new StatisticsActiveBrokersVM();

            var usStateCodes = db.States
                .Where(s => s.CountryCode == "US")
                .Select(s => s.StateCode)
                .ToList();
            var usStateSet = new System.Collections.Generic.HashSet<string>(usStateCodes, StringComparer.OrdinalIgnoreCase);

            var now = DateTime.Now;
            int currentYear = now.Year;
            var last24Date = now.AddMonths(-24);
            int last24StartInt = last24Date.Year * 10000 + last24Date.Month * 100 + 1;
            var prevMonthDate  = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            int prevMonthEndInt = prevMonthDate.Year * 10000 + prevMonthDate.Month * 100 + 31;

            // Query 1 — pull all active broker rows (6 narrow columns) into memory.
            // IX_Stats_Status_EntityType covers (Status, PhysicalAddressStateCode) INCLUDE
            // (EntityType, PhysicalAddressCity, DateAdded, DateLastChanged), so this scan
            // touches the narrow index rather than the 163-column clustered index.
            // USDOTNumber (PK, INT) is included so Query 3 can do a targeted PK lookup.
            var abAllBrokers = db.Database.SqlQuery<AbBrokerRow>(
                "SELECT USDOTNumber, EntityType, PhysicalAddressStateCode, PhysicalAddressCity, DateAdded, DateLastChanged" +
                " FROM TransportCompany WHERE Status='A' AND EntityType LIKE '%B%'"
            ).ToList();

            // Apply US-state filter in memory (excludes Canadian rows — fixes the leak).
            var abBrokers = abAllBrokers
                .Where(r => r.PhysicalAddressStateCode != null
                         && usStateSet.Contains(r.PhysicalAddressStateCode))
                .ToList();

            // ── scalar aggregates ────────────────────────────────────────────────
            vm.TotalActiveBrokers = abBrokers.Count;

            vm.New24MonthsCount = abBrokers.Count(r =>
                r.DateAdded.HasValue
                && r.DateAdded.Value >= last24StartInt
                && r.DateAdded.Value <= prevMonthEndInt);
            vm.AvgNewPerMonth = vm.New24MonthsCount > 0
                ? Math.Round((decimal)vm.New24MonthsCount / 24, 1) : 0;

            vm.BrokerOnlyCount = abBrokers.Count(r =>
                !string.IsNullOrEmpty(r.EntityType) && !r.EntityType.Contains("C"));
            vm.BrokerOnlyPct = vm.TotalActiveBrokers > 0
                ? Math.Round((decimal)vm.BrokerOnlyCount / vm.TotalActiveBrokers * 100, 1) : 0;

            vm.BrokerAndCarrierCount = abBrokers.Count(r =>
                !string.IsNullOrEmpty(r.EntityType) && r.EntityType.Contains("C"));
            vm.BrokerAndCarrierPct = vm.TotalActiveBrokers > 0
                ? Math.Round((decimal)vm.BrokerAndCarrierCount / vm.TotalActiveBrokers * 100, 1) : 0;

            int maxDlc = abBrokers
                .Where(r => r.DateLastChanged.HasValue && r.DateLastChanged.Value > 19000101)
                .Select(r => r.DateLastChanged.Value)
                .DefaultIfEmpty(0).Max();
            if (maxDlc > 19000101)
            {
                int ly = maxDlc / 10000; int lm = (maxDlc / 100) % 100; int ld = maxDlc % 100;
                try { vm.LastDataUpdate = new DateTime(ly, lm, ld); } catch { vm.LastDataUpdate = null; }
            }

            // ── state name lookup ────────────────────────────────────────────────
            var allStateNameDict = db.States
                .Where(s => s.CountryCode == "US")
                .ToDictionary(s => s.StateCode, s => s.State1);

            // ── top states + map ─────────────────────────────────────────────────
            var allStateCounts = abBrokers
                .Where(r => !string.IsNullOrEmpty(r.PhysicalAddressStateCode))
                .GroupBy(r => r.PhysicalAddressStateCode)
                .Select(g => new { StateCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var allStateCountsDict = allStateCounts.ToDictionary(x => x.StateCode, x => x.Count);

            vm.TopStates = allStateCounts.Take(10).Select(x => new StateStatRow
            {
                StateCode = x.StateCode,
                StateName = allStateNameDict.ContainsKey(x.StateCode) ? allStateNameDict[x.StateCode] : x.StateCode,
                CompanyCount = x.Count,
                PercentOfTotal = vm.TotalActiveBrokers > 0
                    ? Math.Round((decimal)x.Count / vm.TotalActiveBrokers * 100, 1) : 0
            }).ToList();

            vm.AllStatesForMap = usStateCodes.Select(sc => new StateMapRow
            {
                StateCode = sc,
                StateName = allStateNameDict.ContainsKey(sc) ? allStateNameDict[sc] : sc,
                CompanyCount = allStateCountsDict.ContainsKey(sc) ? allStateCountsDict[sc] : 0
            }).OrderBy(x => x.StateName).ToList();

            // ── top cities ───────────────────────────────────────────────────────
            var topCitiesRaw = abBrokers
                .Where(r => !string.IsNullOrEmpty(r.PhysicalAddressCity))
                .GroupBy(r => new { r.PhysicalAddressCity, r.PhysicalAddressStateCode })
                .Select(g => new { g.Key.PhysicalAddressCity, g.Key.PhysicalAddressStateCode, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            vm.TopCities = topCitiesRaw.Select(x => new CityStatRow
            {
                CityName = x.PhysicalAddressCity,
                StateCode = x.PhysicalAddressStateCode,
                CompanyCount = x.Count,
                PercentOfTotal = vm.TotalActiveBrokers > 0
                    ? Math.Round((decimal)x.Count / vm.TotalActiveBrokers * 100, 2) : 0
            }).ToList();

            // ── entity type combinations ─────────────────────────────────────────
            int etBrokerOnly = 0, etBrokerCarrier = 0, etBrokerFF = 0, etBrokerCarrierFF = 0, etOther = 0;
            foreach (var broker in abBrokers)
            {
                var et = broker.EntityType;
                if (string.IsNullOrEmpty(et)) { continue; }
                var codes = new System.Collections.Generic.HashSet<string>(
                    et.Split(';').Select(c => c.Trim()).Where(c => c.Length > 0));
                bool hasB = codes.Contains("B");
                bool hasC = codes.Contains("C");
                bool hasF = codes.Contains("F");
                if (!hasB) { continue; }
                if (hasC && hasF) { etBrokerCarrierFF++; }
                else if (hasC) { etBrokerCarrier++; }
                else if (hasF) { etBrokerFF++; }
                else if (codes.Count == 1) { etBrokerOnly++; }
                else { etOther++; }
            }
            vm.EntityTypes = new List<BrokerEntityTypeRow>();
            if (etBrokerOnly > 0)     vm.EntityTypes.Add(new BrokerEntityTypeRow { Name = "Broker Only",                  Count = etBrokerOnly,     Pct = vm.TotalActiveBrokers > 0 ? Math.Round((decimal)etBrokerOnly     / vm.TotalActiveBrokers * 100, 1) : 0 });
            if (etBrokerCarrier > 0)  vm.EntityTypes.Add(new BrokerEntityTypeRow { Name = "Broker + Carrier",             Count = etBrokerCarrier,  Pct = vm.TotalActiveBrokers > 0 ? Math.Round((decimal)etBrokerCarrier  / vm.TotalActiveBrokers * 100, 1) : 0 });
            if (etBrokerFF > 0)       vm.EntityTypes.Add(new BrokerEntityTypeRow { Name = "Broker + Freight Forwarder",   Count = etBrokerFF,       Pct = vm.TotalActiveBrokers > 0 ? Math.Round((decimal)etBrokerFF       / vm.TotalActiveBrokers * 100, 1) : 0 });
            if (etBrokerCarrierFF > 0)vm.EntityTypes.Add(new BrokerEntityTypeRow { Name = "Broker + Carrier + FF",        Count = etBrokerCarrierFF,Pct = vm.TotalActiveBrokers > 0 ? Math.Round((decimal)etBrokerCarrierFF / vm.TotalActiveBrokers * 100, 1) : 0 });
            if (etOther > 0)          vm.EntityTypes.Add(new BrokerEntityTypeRow { Name = "Other",                        Count = etOther,          Pct = vm.TotalActiveBrokers > 0 ? Math.Round((decimal)etOther          / vm.TotalActiveBrokers * 100, 1) : 0 });

            // ── monthly registrations (last 24 months) ───────────────────────────
            var abMoDict = abBrokers
                .Where(r => r.DateAdded.HasValue
                         && r.DateAdded.Value >= last24StartInt
                         && r.DateAdded.Value <= prevMonthEndInt)
                .GroupBy(r => r.DateAdded.Value / 100)
                .ToDictionary(g => g.Key, g => g.Count());

            vm.MonthlyRegistrations = new List<MonthlyRegistrationRow>();
            for (int i = 24; i >= 1; i--)
            {
                var mDate = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                int ym = mDate.Year * 100 + mDate.Month;
                vm.MonthlyRegistrations.Add(new MonthlyRegistrationRow
                {
                    Year  = mDate.Year,
                    Month = mDate.Month,
                    Count = abMoDict.ContainsKey(ym) ? abMoDict[ym] : 0,
                    Label = mDate.ToString("MMM yy")
                });
            }

            // ── age distribution ─────────────────────────────────────────────────
            int abYear2  = (currentYear - 2)  * 10000;
            int abYear5  = (currentYear - 5)  * 10000;
            int abYear10 = (currentYear - 10) * 10000;
            int abYear20 = (currentYear - 20) * 10000;

            int ageZ2 = 0, age35 = 0, age610 = 0, age1120 = 0, age20p = 0;
            double ageSumYears = 0.0; int ageCount = 0;
            foreach (var broker in abBrokers)
            {
                if (!broker.DateAdded.HasValue || broker.DateAdded.Value <= 19000101) { continue; }
                int da = broker.DateAdded.Value;
                if      (da >= abYear2)  { ageZ2++;   }
                else if (da >= abYear5)  { age35++;   }
                else if (da >= abYear10) { age610++;  }
                else if (da >= abYear20) { age1120++; }
                else                     { age20p++;  }
                ageSumYears += (double)(currentYear - da / 10000);
                ageCount++;
            }
            vm.AgeDistribution = new CompanyAgeDistribution
            {
                ZeroToTwo       = ageZ2,
                ThreeToFive     = age35,
                SixToTen       = age610,
                ElevenToTwenty  = age1120,
                OverTwenty      = age20p,
                AverageAgeYears = ageCount > 0 ? Math.Round((decimal)(ageSumYears / ageCount), 1) : 0
            };

            // ── carrier-to-broker ratio — Query 2 ───────────────────────────────
            // Broker state counts come from the in-memory list; carrier counts require a DB query.
            var abTop10Codes = allStateCounts.Take(10).Select(x => x.StateCode).ToList();
            var abCarrierCounts = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.EntityType.Contains("C")
                          && abTop10Codes.Contains(tc.PhysicalAddressStateCode))
                .GroupBy(tc => tc.PhysicalAddressStateCode)
                .Select(g => new { StateCode = g.Key, CarrierCount = g.Count() })
                .ToList();
            var abCarrierDict = abCarrierCounts.ToDictionary(x => x.StateCode, x => x.CarrierCount);

            vm.CarrierToBrokerRatioByState = allStateCounts.Take(10).Select(x => new BrokerCarrierRatioRow
            {
                StateCode    = x.StateCode,
                StateName    = allStateNameDict.ContainsKey(x.StateCode) ? allStateNameDict[x.StateCode] : x.StateCode,
                BrokerCount  = x.Count,
                CarrierCount = abCarrierDict.ContainsKey(x.StateCode) ? abCarrierDict[x.StateCode] : 0,
                Ratio        = x.Count > 0
                    ? Math.Round((decimal)(abCarrierDict.ContainsKey(x.StateCode) ? abCarrierDict[x.StateCode] : 0) / x.Count, 1) : 0
            }).ToList();

            // ── longest-registered brokers — Query 3 ────────────────────────────
            // Candidates are sourced from the already-US-filtered abBrokers list
            // (so no Canadian leak), sorted by earliest DateAdded in memory.
            // Then a tiny PK-lookup (USDOTNumber IN ...) fetches the LegalName/MC
            // columns for up to 100 candidates. PK lookups are O(log n) — instant.
            var abLongestCandidates = abBrokers
                .Where(r => r.DateAdded.HasValue && r.DateAdded.Value > 19000101)
                .OrderBy(r => r.DateAdded.Value)
                .Take(100)
                .Select(r => r.USDOTNumber)
                .ToList();

            vm.LongestRegisteredBrokers = db.TransportCompanies
                .Where(tc => abLongestCandidates.Contains(tc.USDOTNumber)
                          && tc.LegalName != null && tc.LegalName != "")
                .OrderBy(tc => tc.DateAdded)
                .Take(10)
                .ToList()
                .Select(tc => new LongestRegisteredBrokerRow
                {
                    LegalName   = tc.LegalName,
                    USDOTNumber = tc.USDOTNumber,
                    City        = tc.PhysicalAddressCity,
                    StateCode   = tc.PhysicalAddressStateCode,
                    SinceYear   = tc.DateAdded.HasValue ? tc.DateAdded.Value / 10000 : 0
                }).ToList();

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        public StatisticsStateCompaniesVM GetStateCompaniesData(string stateCode)
        {
            stateCode = stateCode?.ToUpper()?.Trim();
            if (string.IsNullOrEmpty(stateCode)) return null;

            string cacheKey = "StateCompaniesData_v2_" + stateCode;
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsStateCompaniesVM;
            if (cached != null) { return cached; }

            var stateEntity = db.States.FirstOrDefault(s => s.StateCode == stateCode && s.CountryCode == "US");
            if (stateEntity == null) return null;

            var vm = new StatisticsStateCompaniesVM();
            vm.StateCode = stateCode;
            vm.StateName = stateEntity.State1;

            var scNow          = DateTime.Now;
            var sc12StartDt    = scNow.AddMonths(-12);
            int scLast12Start  = sc12StartDt.Year * 10000 + sc12StartDt.Month * 100 + 1;
            var sc12EndDt      = scNow.AddMonths(-1);
            int scLast12End    = sc12EndDt.Year * 10000 + sc12EndDt.Month * 100 + 31;
            var scPrior12Dt    = scNow.AddMonths(-24);
            int scPrior12Start = scPrior12Dt.Year * 10000 + scPrior12Dt.Month * 100 + 1;
            var scPrior12EndDt = scNow.AddMonths(-13);
            int scPrior12End   = scPrior12EndDt.Year * 10000 + scPrior12EndDt.Month * 100 + 31;

            var agg = db.Database.SqlQuery<ScStateAggregate>(@"
                SELECT
                    COUNT(*) AS TotalCount,
                    SUM(CASE WHEN IccDocketNumber1Prefix='MC' THEN 1 ELSE 0 END) AS MCCount,
                    ISNULL(SUM(CASE WHEN TotalNumberOfPowerUnits > 0 THEN TotalNumberOfPowerUnits ELSE 0 END), 0) AS PowerUnitsSum,
                    ISNULL(SUM(CASE WHEN NNDriversGrandTotalInterstateAndIntrastate > 0 THEN NNDriversGrandTotalInterstateAndIntrastate ELSE 0 END), 0) AS DriversSum,
                    SUM(CASE WHEN TotalNumberOfPowerUnits =  1               THEN 1 ELSE 0 END) AS OneUnit,
                    SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN  2 AND  5  THEN 1 ELSE 0 END) AS TwoToFive,
                    SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN  6 AND 20  THEN 1 ELSE 0 END) AS SixToTwenty,
                    SUM(CASE WHEN TotalNumberOfPowerUnits BETWEEN 21 AND 100 THEN 1 ELSE 0 END) AS TwentyOneToHundred,
                    SUM(CASE WHEN TotalNumberOfPowerUnits >  100             THEN 1 ELSE 0 END) AS OverHundred,
                    SUM(CASE WHEN TotalNumberOfPowerUnits >    0             THEN 1 ELSE 0 END) AS TotalReporting,
                    MAX(CASE WHEN DateLastChanged > 19000101 THEN DateLastChanged ELSE NULL END) AS MaxDLC,
                    SUM(CASE WHEN DateAdded >= @p1 AND DateAdded <= @p2 THEN 1 ELSE 0 END) AS NewReg12,
                    SUM(CASE WHEN DateAdded >= @p3 AND DateAdded <= @p4 THEN 1 ELSE 0 END) AS NewPriorReg12
                FROM TransportCompany
                WHERE Status='A' AND PhysicalAddressStateCode=@p0",
                stateCode, scLast12Start, scLast12End, scPrior12Start, scPrior12End).First();

            vm.TotalActiveCompanies = agg.TotalCount;
            vm.ActiveMCNumbers      = agg.MCCount;
            vm.TotalPowerUnits      = agg.PowerUnitsSum;
            vm.TotalDrivers         = agg.DriversSum;

            int usTotalCount = db.TransportCompanies.Count(tc => tc.Status == "A");
            vm.PercentOfUSTotal = usTotalCount > 0
                ? Math.Round((decimal)vm.TotalActiveCompanies / usTotalCount * 100, 2)
                : 0;

            vm.SizeDistribution = new CompanySizeDistribution
            {
                OneUnit            = agg.OneUnit,
                TwoToFive          = agg.TwoToFive,
                SixToTwenty        = agg.SixToTwenty,
                TwentyOneToHundred = agg.TwentyOneToHundred,
                OverHundred        = agg.OverHundred,
                TotalReporting     = agg.TotalReporting
            };

            vm.NewRegistrations12    = agg.NewReg12;
            vm.NewPriorRegistrations12 = agg.NewPriorReg12;

            var countyNameDict = new Dictionary<int, string>();
            var countyRows = db.Database.SqlQuery<CountyNameResult>(
                "SELECT Id, StateCode, CountyCode, CountyName FROM McmisCountyCodes WHERE StateCode = @p0",
                stateCode).ToList();
            countyNameDict = countyRows.ToDictionary(x => x.CountyCode, x => x.CountyName);

            var ccRows = db.Database.SqlQuery<ScCountyCityRow>(
                "SELECT PhysicalAddressCountyCode, PhysicalAddressCity FROM TransportCompany WHERE Status='A' AND PhysicalAddressStateCode=@p0",
                stateCode).ToList();

            var countyGroups = ccRows
                .Where(r => r.PhysicalAddressCountyCode.HasValue)
                .GroupBy(r => r.PhysicalAddressCountyCode.Value)
                .Select(g => new { CountyCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            vm.TopCounties = countyGroups.Take(10).Select(x => new CountyStatRow
            {
                CountyCode = x.CountyCode,
                CountyName = countyNameDict.ContainsKey(x.CountyCode)
                    ? countyNameDict[x.CountyCode]
                    : "County " + x.CountyCode,
                CompanyCount = x.Count,
                PercentOfState = vm.TotalActiveCompanies > 0
                    ? Math.Round((decimal)x.Count / vm.TotalActiveCompanies * 100, 2)
                    : 0
            }).ToList();

            vm.AllCountiesForMap = countyGroups.Select(x => new CountyMapRow
            {
                CountyCode = x.CountyCode,
                CountyName = countyNameDict.ContainsKey(x.CountyCode)
                    ? countyNameDict[x.CountyCode]
                    : "County " + x.CountyCode,
                CompanyCount = x.Count
            }).ToList();

            vm.TopCities = ccRows
                .Where(r => r.PhysicalAddressCity != null && r.PhysicalAddressCity != "")
                .GroupBy(r => r.PhysicalAddressCity)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .Select(x => new StateCityStatRow
                {
                    CityName = x.City,
                    CompanyCount = x.Count,
                    PercentOfState = vm.TotalActiveCompanies > 0
                        ? Math.Round((decimal)x.Count / vm.TotalActiveCompanies * 100, 2)
                        : 0
                }).ToList();

            if (agg.MaxDLC.HasValue)
            {
                int y = agg.MaxDLC.Value / 10000;
                int m = (agg.MaxDLC.Value / 100) % 100;
                int d = agg.MaxDLC.Value % 100;
                try { vm.LastDataUpdate = new DateTime(y, m, d); } catch { vm.LastDataUpdate = null; }
            }

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        public StatisticsCityCompaniesVM GetCityCompaniesData(string stateCode, string cityName, string range = "24m")
        {
            stateCode = stateCode?.ToUpper()?.Trim();
            cityName = cityName?.ToUpper()?.Trim();
            if (string.IsNullOrEmpty(stateCode) || string.IsNullOrEmpty(cityName)) { return null; }
            if (range != "4y" && range != "8y" && range != "all") { range = "24m"; }

            string cacheKey = "CityCompaniesData_v2_" + stateCode + "_" + cityName + "_" + range;
            var cached = HttpRuntime.Cache[cacheKey] as StatisticsCityCompaniesVM;
            if (cached != null) { return cached; }

            var stateEntity = db.States.FirstOrDefault(s => s.StateCode == stateCode && s.CountryCode == "US");
            if (stateEntity == null) { return null; }

            var vm = new StatisticsCityCompaniesVM();
            vm.StateCode = stateCode;
            vm.CityName = cityName;
            vm.StateName = stateEntity.State1;

            var cityRows = db.Database.SqlQuery<CcCityRow>(
                "SELECT TotalNumberOfPowerUnits, DateAdded, DateLastChanged, EntityType FROM TransportCompany WHERE Status='A' AND PhysicalAddressStateCode=@p0 AND PhysicalAddressCity=@p1",
                stateCode, cityName).ToList();

            vm.TotalActiveCompanies = cityRows.Count;
            if (vm.TotalActiveCompanies == 0) { return null; }

            int cityReporting = cityRows.Count(r => r.TotalNumberOfPowerUnits.HasValue && r.TotalNumberOfPowerUnits.Value > 0);
            int cityOO        = cityRows.Count(r => r.TotalNumberOfPowerUnits == 1);

            var fleetSizes = cityRows
                .Where(r => r.TotalNumberOfPowerUnits.HasValue && r.TotalNumberOfPowerUnits.Value > 0)
                .Select(r => r.TotalNumberOfPowerUnits.Value)
                .OrderBy(x => x)
                .ToList();
            if (fleetSizes.Any())
            {
                int mid = fleetSizes.Count / 2;
                vm.MedianFleetSize = fleetSizes.Count % 2 == 0
                    ? (fleetSizes[mid - 1] + fleetSizes[mid]) / 2
                    : fleetSizes[mid];
            }

            var now = DateTime.Now;

            int cutoff24Int = now.AddMonths(-24).Year * 10000 + now.AddMonths(-24).Month * 100 + 1;
            vm.NewRegistrationsLast24Months = cityRows.Count(r => r.DateAdded.HasValue && r.DateAdded.Value >= cutoff24Int && r.DateAdded.Value > 0);
            vm.AvgNewPerMonth = Math.Round((decimal)vm.NewRegistrationsLast24Months / 24.0m, 1);

            int monthsBack;
            string rangeLabel;
            switch (range)
            {
                case "4y":
                    monthsBack = 47; rangeLabel = "Last 4 Years"; break;
                case "8y":
                    monthsBack = 95; rangeLabel = "Last 8 Years"; break;
                case "all":
                    var validForMin = cityRows.Where(r => r.DateAdded.HasValue && r.DateAdded.Value > 10000000).ToList();
                    if (validForMin.Any())
                    {
                        int minRaw = validForMin.Min(r => r.DateAdded.Value);
                        int minY = minRaw / 10000;
                        int minM = (minRaw / 100) % 100;
                        if (minM < 1) minM = 1; if (minM > 12) minM = 12;
                        monthsBack = (now.Year - minY) * 12 + (now.Month - minM);
                        if (monthsBack < 23) monthsBack = 23;
                    }
                    else { monthsBack = 23; }
                    rangeLabel = "All Data Available"; break;
                default:
                    monthsBack = 23; rangeLabel = "Last 24 Months"; break;
            }
            vm.SelectedRange = range;
            vm.RangeLabel    = rangeLabel;

            int chartCutoffInt = now.AddMonths(-monthsBack).Year * 10000 + now.AddMonths(-monthsBack).Month * 100 + 1;
            var monthlyDict = cityRows
                .Where(r => r.DateAdded.HasValue && r.DateAdded.Value >= chartCutoffInt && r.DateAdded.Value > 0)
                .GroupBy(r => r.DateAdded.Value / 100)
                .ToDictionary(g => g.Key, g => g.Count());
            var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            vm.MonthlyRegistrations = new List<MonthlyRegistrationRow>();
            for (int i = monthsBack; i >= 0; i--)
            {
                var d = now.AddMonths(-i);
                int ym = d.Year * 100 + d.Month;
                int cnt = monthlyDict.ContainsKey(ym) ? monthlyDict[ym] : 0;
                vm.MonthlyRegistrations.Add(new MonthlyRegistrationRow
                {
                    Year = d.Year, Month = d.Month, Count = cnt,
                    Label = monthNames[d.Month] + " " + d.Year
                });
            }

            if (vm.MonthlyRegistrations.Any())
            {
                var best = vm.MonthlyRegistrations.OrderByDescending(x => x.Count).First();
                vm.BestMonthLabel = best.Label;
                vm.BestMonthCount = best.Count;
                var completedMonths = vm.MonthlyRegistrations
                    .Where(x => !(x.Year == now.Year && x.Month == now.Month)).ToList();
                if (completedMonths.Any())
                {
                    var lowest = completedMonths.OrderBy(x => x.Count).First();
                    vm.LowestMonthLabel = lowest.Label;
                    vm.LowestMonthCount = lowest.Count;
                }
            }

            int mrCount = vm.MonthlyRegistrations.Count;
            if (mrCount >= 24)
            {
                int last12 = vm.MonthlyRegistrations.Skip(mrCount - 12).Sum(x => x.Count);
                int prev12 = vm.MonthlyRegistrations.Skip(mrCount - 24).Take(12).Sum(x => x.Count);
                if (prev12 > 0)
                    vm.NewRegistrationsYoYPercent = Math.Round((decimal)(last12 - prev12) / prev12 * 100, 1);
            }

            vm.SizeDistribution = new CompanySizeDistribution
            {
                OneUnit            = cityRows.Count(r => r.TotalNumberOfPowerUnits == 1),
                TwoToFive          = cityRows.Count(r => r.TotalNumberOfPowerUnits >= 2 && r.TotalNumberOfPowerUnits <= 5),
                SixToTwenty        = cityRows.Count(r => r.TotalNumberOfPowerUnits >= 6 && r.TotalNumberOfPowerUnits <= 20),
                TwentyOneToHundred = cityRows.Count(r => r.TotalNumberOfPowerUnits >= 21 && r.TotalNumberOfPowerUnits <= 100),
                OverHundred        = cityRows.Count(r => r.TotalNumberOfPowerUnits > 100),
                TotalReporting     = cityReporting,
                MedianFleetSize    = vm.MedianFleetSize
            };

            int currentYear = now.Year;
            int year2  = (currentYear - 2)  * 10000;
            int year5  = (currentYear - 5)  * 10000;
            int year10 = (currentYear - 10) * 10000;
            int year20 = (currentYear - 20) * 10000;

            vm.AgeDistribution = new CompanyAgeDistribution
            {
                ZeroToTwo      = cityRows.Count(r => r.DateAdded.HasValue && r.DateAdded.Value > 0 && r.DateAdded.Value >= year2),
                ThreeToFive    = cityRows.Count(r => r.DateAdded.HasValue && r.DateAdded.Value > 0 && r.DateAdded.Value >= year5  && r.DateAdded.Value < year2),
                SixToTen       = cityRows.Count(r => r.DateAdded.HasValue && r.DateAdded.Value > 0 && r.DateAdded.Value >= year10 && r.DateAdded.Value < year5),
                ElevenToTwenty = cityRows.Count(r => r.DateAdded.HasValue && r.DateAdded.Value > 0 && r.DateAdded.Value >= year20 && r.DateAdded.Value < year10),
                OverTwenty     = cityRows.Count(r => r.DateAdded.HasValue && r.DateAdded.Value > 0 && r.DateAdded.Value < year20)
            };
            var withDates = cityRows.Where(r => r.DateAdded.HasValue && r.DateAdded.Value > 0).ToList();
            if (withDates.Any())
            {
                double avgAge = withDates.Average(r => (double)(currentYear - r.DateAdded.Value / 10000));
                vm.AgeDistribution.AverageAgeYears = Math.Round((decimal)avgAge, 1);
            }

            var authorityMapping = new Dictionary<string, string>
            {
                { "C", "Carrier" }, { "B", "Broker" }, { "S", "Shipper" },
                { "F", "Freight Forwarder" }, { "I", "Intermodal Equipment Provider" }, { "T", "Cargo Tank" }
            };
            var authCounts = new Dictionary<string, int>();
            foreach (var row in cityRows.Where(r => r.EntityType != null && r.EntityType != ""))
            {
                foreach (var part in row.EntityType.Split(';'))
                {
                    var code = part.Trim();
                    if (authorityMapping.ContainsKey(code))
                    {
                        if (!authCounts.ContainsKey(code)) { authCounts[code] = 0; }
                        authCounts[code]++;
                    }
                }
            }
            vm.TopAuthorityTypes = authCounts
                .OrderByDescending(x => x.Value)
                .Select(x => new AuthorityTypeRow
                {
                    AuthorityTypeName = authorityMapping[x.Key],
                    CompanyCount = x.Value,
                    PercentOfTotal = vm.TotalActiveCompanies > 0
                        ? Math.Round((decimal)x.Value / vm.TotalActiveCompanies * 100, 1)
                        : 0
                }).ToList();

            var stateAgg = db.Database.SqlQuery<CcStateAggregate>(@"
                SELECT
                    COUNT(*) AS StateTotal,
                    SUM(CASE WHEN TotalNumberOfPowerUnits > 0 THEN 1 ELSE 0 END) AS StateReporting,
                    SUM(CASE WHEN TotalNumberOfPowerUnits = 1 THEN 1 ELSE 0 END) AS StateOO
                FROM TransportCompany
                WHERE Status='A' AND PhysicalAddressStateCode=@p0", stateCode).First();

            vm.PercentOfStateTotal = stateAgg.StateTotal > 0
                ? Math.Round((decimal)vm.TotalActiveCompanies / stateAgg.StateTotal * 100, 2)
                : 0;
            vm.OwnerOperatorPercent = cityReporting > 0
                ? Math.Round((decimal)cityOO / cityReporting * 100, 1)
                : 0;
            vm.StateOwnerOperatorPercent = stateAgg.StateReporting > 0
                ? Math.Round((decimal)stateAgg.StateOO / stateAgg.StateReporting * 100, 1)
                : 0;

            var cargoSql = @"SELECT
                    SUM(CASE WHEN CargoTransportedAGeneralFreight='X' THEN 1 ELSE 0 END) AS GenFreight,
                    SUM(CASE WHEN CargoTransportedBHouseholdGoods='X' THEN 1 ELSE 0 END) AS HouseholdGoods,
                    SUM(CASE WHEN CargoTransportedCMetalSheetsCoilsRolls='X' THEN 1 ELSE 0 END) AS Metal,
                    SUM(CASE WHEN CargoTransportedDMotorVehicles='X' THEN 1 ELSE 0 END) AS MotorVehicles,
                    SUM(CASE WHEN CargoTransportedEDriveawayTowaway='X' THEN 1 ELSE 0 END) AS Driveaway,
                    SUM(CASE WHEN CargoTransportedFLogsPolesBeamsLumber='X' THEN 1 ELSE 0 END) AS Lumber,
                    SUM(CASE WHEN CargoTransportedGBuildingMaterials='X' THEN 1 ELSE 0 END) AS BuildingMat,
                    SUM(CASE WHEN CargoTransportedHMobileHomes='X' THEN 1 ELSE 0 END) AS MobileHomes,
                    SUM(CASE WHEN CargoTransportedIMachineryLargeObjects='X' THEN 1 ELSE 0 END) AS Machinery,
                    SUM(CASE WHEN CargoTransportedJFreshProduce='X' THEN 1 ELSE 0 END) AS FreshProduce,
                    SUM(CASE WHEN CargoTransportedKLiquidsGases='X' THEN 1 ELSE 0 END) AS Liquids,
                    SUM(CASE WHEN CargoTransportedLIintermodalContainers='X' THEN 1 ELSE 0 END) AS Intermodal,
                    SUM(CASE WHEN CargoTransportedMPassengers='X' THEN 1 ELSE 0 END) AS Passengers,
                    SUM(CASE WHEN CargoTransportedNOilfieldEquipment='X' THEN 1 ELSE 0 END) AS Oilfield,
                    SUM(CASE WHEN CargoTransportedOLivestock='X' THEN 1 ELSE 0 END) AS Livestock,
                    SUM(CASE WHEN CargoTransportedPGrainFeedHay='X' THEN 1 ELSE 0 END) AS Grain,
                    SUM(CASE WHEN CargoTransportedQCoalCoke='X' THEN 1 ELSE 0 END) AS Coal,
                    SUM(CASE WHEN CargoTransportedRMeat='X' THEN 1 ELSE 0 END) AS Meat,
                    SUM(CASE WHEN CargoTransportedSGarbageRefuseTrash='X' THEN 1 ELSE 0 END) AS Garbage,
                    SUM(CASE WHEN CargoTransportedTUSMail='X' THEN 1 ELSE 0 END) AS USMail,
                    SUM(CASE WHEN CargoTransportedUChemicals='X' THEN 1 ELSE 0 END) AS Chemicals,
                    SUM(CASE WHEN CargoTransportedVCommoditiesDryBulk='X' THEN 1 ELSE 0 END) AS DryBulk,
                    SUM(CASE WHEN CargoTransportedWRefrigeratedFood='X' THEN 1 ELSE 0 END) AS RefrigFood,
                    SUM(CASE WHEN CargoTransportedXBeverages='X' THEN 1 ELSE 0 END) AS Beverages,
                    SUM(CASE WHEN CargoTransportedYPaperProducts='X' THEN 1 ELSE 0 END) AS Paper,
                    SUM(CASE WHEN CargoTransportedZUtility='X' THEN 1 ELSE 0 END) AS Utility,
                    SUM(CASE WHEN CargoTransportedAAFarmSupplies='X' THEN 1 ELSE 0 END) AS FarmSupplies,
                    SUM(CASE WHEN CargoTransportedBBConstruction='X' THEN 1 ELSE 0 END) AS Construction,
                    SUM(CASE WHEN CargoTransportedCCWaterWell='X' THEN 1 ELSE 0 END) AS WaterWell,
                    SUM(CASE WHEN CargoTransportedDDOther='X' THEN 1 ELSE 0 END) AS Other
                FROM TransportCompany
                WHERE Status='A' AND PhysicalAddressStateCode=@p0 AND PhysicalAddressCity=@p1";

            var cargo = db.Database.SqlQuery<CargoCounts>(cargoSql, stateCode, cityName).FirstOrDefault();
            if (cargo != null)
            {
                var rawList = new List<KeyValuePair<string, int>>
                {
                    new KeyValuePair<string, int>("General Freight", cargo.GenFreight),
                    new KeyValuePair<string, int>("Intermodal Containers", cargo.Intermodal),
                    new KeyValuePair<string, int>("Building Materials", cargo.BuildingMat),
                    new KeyValuePair<string, int>("Motor Vehicles", cargo.MotorVehicles),
                    new KeyValuePair<string, int>("Fresh Produce", cargo.FreshProduce),
                    new KeyValuePair<string, int>("Household Goods", cargo.HouseholdGoods),
                    new KeyValuePair<string, int>("Other", cargo.Other),
                    new KeyValuePair<string, int>("Paper Products", cargo.Paper),
                    new KeyValuePair<string, int>("Beverages", cargo.Beverages),
                    new KeyValuePair<string, int>("Refrigerated Food", cargo.RefrigFood),
                    new KeyValuePair<string, int>("Garbage & Refuse", cargo.Garbage),
                    new KeyValuePair<string, int>("Construction", cargo.Construction),
                    new KeyValuePair<string, int>("Liquids & Gases", cargo.Liquids),
                    new KeyValuePair<string, int>("Passengers", cargo.Passengers),
                    new KeyValuePair<string, int>("Driveaway/Towaway", cargo.Driveaway),
                    new KeyValuePair<string, int>("Dry Bulk Commodities", cargo.DryBulk),
                    new KeyValuePair<string, int>("Meat", cargo.Meat),
                    new KeyValuePair<string, int>("Metal: Sheets & Coils", cargo.Metal),
                    new KeyValuePair<string, int>("Chemicals", cargo.Chemicals),
                    new KeyValuePair<string, int>("Machinery & Large Objects", cargo.Machinery),
                    new KeyValuePair<string, int>("Utility", cargo.Utility),
                    new KeyValuePair<string, int>("Farm Supplies", cargo.FarmSupplies),
                    new KeyValuePair<string, int>("US Mail", cargo.USMail),
                    new KeyValuePair<string, int>("Logs, Poles & Lumber", cargo.Lumber),
                    new KeyValuePair<string, int>("Grain, Feed & Hay", cargo.Grain),
                    new KeyValuePair<string, int>("Oilfield Equipment", cargo.Oilfield),
                    new KeyValuePair<string, int>("Livestock", cargo.Livestock),
                    new KeyValuePair<string, int>("Mobile Homes", cargo.MobileHomes),
                    new KeyValuePair<string, int>("Coal & Coke", cargo.Coal),
                    new KeyValuePair<string, int>("Water Well", cargo.WaterWell)
                };

                vm.TopCargoTypes = rawList
                    .Where(x => x.Value > 0)
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .Select(x => new CargoTypeRow
                    {
                        CargoTypeName = x.Key,
                        CompanyCount = x.Value,
                        PercentOfTotal = vm.TotalActiveCompanies > 0
                            ? Math.Round((decimal)x.Value / vm.TotalActiveCompanies * 100, 1)
                            : 0
                    }).ToList();
            }
            if (vm.TopCargoTypes == null) { vm.TopCargoTypes = new List<CargoTypeRow>(); }

            vm.TopCompaniesByFleetSize = db.TransportCompanies
                .Where(tc => tc.Status == "A"
                          && tc.PhysicalAddressStateCode == stateCode
                          && tc.PhysicalAddressCity == cityName
                          && tc.TotalNumberOfPowerUnits > 0)
                .OrderByDescending(tc => tc.TotalNumberOfPowerUnits)
                .Take(10)
                .Select(tc => new CityCompanyRow
                {
                    LegalName = tc.LegalName ?? tc.CompanyName,
                    USDOTNumber = tc.USDOTNumber,
                    FleetSize = tc.TotalNumberOfPowerUnits ?? 0
                })
                .ToList();

            var countyName = db.Database.SqlQuery<string>(@"
                    SELECT TOP 1 m.CountyName
                    FROM McmisCountyCodes m
                    WHERE m.StateCode = @p0 AND m.CountyCode = (
                        SELECT TOP 1 PhysicalAddressCountyCode
                        FROM TransportCompany
                        WHERE Status='A' AND PhysicalAddressStateCode=@p0 AND PhysicalAddressCity=@p1
                          AND PhysicalAddressCountyCode IS NOT NULL
                        GROUP BY PhysicalAddressCountyCode
                        ORDER BY COUNT(*) DESC
                    )", stateCode, cityName).FirstOrDefault();
            vm.CountyName = countyName;

            var validDlc = cityRows
                .Where(r => r.DateLastChanged.HasValue && r.DateLastChanged.Value > 19000101)
                .Select(r => r.DateLastChanged.Value)
                .ToList();
            if (validDlc.Any())
            {
                int maxDlc = validDlc.Max();
                int ly = maxDlc / 10000;
                int lm = (maxDlc / 100) % 100;
                int ld = maxDlc % 100;
                try { vm.LastDataUpdate = new DateTime(ly, lm, ld); } catch { vm.LastDataUpdate = null; }
            }

            HttpRuntime.Cache.Insert(cacheKey, vm, null,
                DateTime.Now.AddDays(30), System.Web.Caching.Cache.NoSlidingExpiration);

            return vm;
        }

        #endregion

    }

    internal class CountyNameResult
    {
        public int Id { get; set; }
        public string StateCode { get; set; }
        public int CountyCode { get; set; }
        public string CountyName { get; set; }
    }

    internal class AcStateNewReg
    {
        public string StateCode { get; set; }
        public int NewRegistrations { get; set; }
    }

    internal class AcAggregateCounts
    {
        public int Interstate { get; set; }
        public int OneUnit { get; set; }
        public int TwoToFive { get; set; }
        public int SixToTwenty { get; set; }
        public int TwentyOneToHundred { get; set; }
        public int OverHundred { get; set; }
        public int TotalReporting { get; set; }
        public int NewThisMonth { get; set; }
        public int NewLastMonth { get; set; }
        public int NewThisYear { get; set; }
        public int NewSamePeriodLastYear { get; set; }
        public int NewPrior12Months { get; set; }
    }

    internal class AcMonthlyCount
    {
        public int YearMonth { get; set; }
        public int Cnt { get; set; }
    }

    internal class AbAgeResult
    {
        public int ZeroToTwo { get; set; }
        public int ThreeToFive { get; set; }
        public int SixToTen { get; set; }
        public int ElevenToTwenty { get; set; }
        public int OverTwenty { get; set; }
        public double AverageAgeYears { get; set; }
    }

    internal class AbBrokerRow
    {
        public int USDOTNumber { get; set; }
        public string EntityType { get; set; }
        public string PhysicalAddressStateCode { get; set; }
        public string PhysicalAddressCity { get; set; }
        public int? DateAdded { get; set; }
        public int? DateLastChanged { get; set; }
    }

    internal class CargoCounts
    {
        public int GenFreight { get; set; }
        public int HouseholdGoods { get; set; }
        public int Metal { get; set; }
        public int MotorVehicles { get; set; }
        public int Driveaway { get; set; }
        public int Lumber { get; set; }
        public int BuildingMat { get; set; }
        public int MobileHomes { get; set; }
        public int Machinery { get; set; }
        public int FreshProduce { get; set; }
        public int Liquids { get; set; }
        public int Intermodal { get; set; }
        public int Passengers { get; set; }
        public int Oilfield { get; set; }
        public int Livestock { get; set; }
        public int Grain { get; set; }
        public int Coal { get; set; }
        public int Meat { get; set; }
        public int Garbage { get; set; }
        public int USMail { get; set; }
        public int Chemicals { get; set; }
        public int DryBulk { get; set; }
        public int RefrigFood { get; set; }
        public int Beverages { get; set; }
        public int Paper { get; set; }
        public int Utility { get; set; }
        public int FarmSupplies { get; set; }
        public int Construction { get; set; }
        public int WaterWell { get; set; }
        public int Other { get; set; }
    }

    internal class ScStateAggregate
    {
        public int TotalCount { get; set; }
        public int MCCount { get; set; }
        public int PowerUnitsSum { get; set; }
        public int DriversSum { get; set; }
        public int OneUnit { get; set; }
        public int TwoToFive { get; set; }
        public int SixToTwenty { get; set; }
        public int TwentyOneToHundred { get; set; }
        public int OverHundred { get; set; }
        public int TotalReporting { get; set; }
        public int? MaxDLC { get; set; }
        public int NewReg12 { get; set; }
        public int NewPriorReg12 { get; set; }
    }

    internal class ScCountyCityRow
    {
        public int? PhysicalAddressCountyCode { get; set; }
        public string PhysicalAddressCity { get; set; }
    }

    internal class CcCityRow
    {
        public int? TotalNumberOfPowerUnits { get; set; }
        public int? DateAdded { get; set; }
        public int? DateLastChanged { get; set; }
        public string EntityType { get; set; }
    }

    internal class CcStateAggregate
    {
        public int StateTotal { get; set; }
        public int StateReporting { get; set; }
        public int StateOO { get; set; }
    }
}
