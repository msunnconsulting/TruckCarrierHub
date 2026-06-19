namespace PartnerCarrier.Infrastructure.Services.Admin.AdminManagement
{
    using Common.Utility;
    using Common.Utility.Logger;
    using Common.Utility.ViewModels;
    using Omu.ValueInjecter;
    using Infrastructure.Contracts.Admin.AdminManagement;
    using Infrastructure.Database;
    using ViewModels.Admin;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Web.Script.Serialization;
    using System.Threading;
    using System.Web;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;

    public class AdminManagementService : IAdminManagementService
    {
        #region private variable 

        private readonly PartnerCarrier_DevEntities db;
        private static UpdateRecordProgressInfo UpdateRecordProgressInfo = null;

        #endregion

        #region constructor
        public AdminManagementService(PartnerCarrier_DevEntities _db)
        {
            db = _db;
            db.Database.CommandTimeout = 300;
        }
        #endregion

        #region Admin User List with Pagination sorting searching
        /// <summary>
        /// Get AdminUser List By Login Name
        /// </summary>
        /// <returns></returns>
        public PagedList<AdminUserVM> AdminUserList(PageSortPara ps, string searchText)
        {
            int pageSize = 5;
            ps.p = ((!ps.p.HasValue) || ps.p == 0) ? 1 : Convert.ToInt32(ps.p);
            ps.se = String.IsNullOrEmpty(ps.se) ? "Name" : ps.se;
            ps.sd = String.IsNullOrEmpty(ps.sd) ? "Asc" : ps.sd;
            var adminList = (from user in db.AdminUsers.AsNoTracking()
                             where user.RoleId == (int)1
                             orderby user.Name ascending
                             select new AdminUserVM
                             {
                                 AdminID = user.Id,
                                 Name = user.Name,
                                 Email = user.Email,
                                 IsActive = user.IsActive,
                                 LoginName = user.Name
                             }).AsQueryable();
            var sortOrder = ps.se + "_" + ps.sd;
            switch (sortOrder)
            {

                case "Name_Asc":
                    adminList = adminList.OrderBy(s => s.LoginName);
                    break;
                case "Name_Desc":
                    adminList = adminList.OrderByDescending(s => s.LoginName);
                    break;
                case "Email_Asc":
                    adminList = adminList.OrderBy(s => s.Email);
                    break;
                case "Email_Desc":
                    adminList = adminList.OrderByDescending(s => s.Email);
                    break;
            }
            //Search
            if (!string.IsNullOrEmpty(searchText))
            {
                adminList = adminList.Where(t => t.Name.Contains(searchText));
            }
            PagedList<AdminUserVM> allUsers = adminList.AsQueryable().SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
            return allUsers;
        }
        #endregion

        #region Select Admin  By AdminId for edit action
        /// <summary>
        /// Select admin for edit action by AdminID
        /// </summary>
        public AddEditAdminUserVM EditAdminById(long? adminId)
        {
            var adminInfo = (from adminTable in db.AdminUsers
                             where adminTable.Id == adminId
                             select new AddEditAdminUserVM
                             {
                                 AdminID = adminTable.Id,
                                 Name = adminTable.Name,
                                 Email = adminTable.Email,
                                 IsDataMigrationLogEnabled = adminTable.IsDataMigrationLogEnabled
                             }).FirstOrDefault();
            return adminInfo;
        }
        #endregion

        #region Add/Update Admin
        /// <summary>
        /// Add/Update Admin
        /// </summary>
        public void AddUpdateAdmin(AddEditAdminUserVM addEditAdminUserVM)
        {
            //check admin aleady exist 
            if (!addEditAdminUserVM.AdminID.HasValue)
            {
                CheckAdminDuplicate(addEditAdminUserVM);
            }
            else // check admin already exist in update time
            {
                CheckAdminDuplicateInUpdateAction(addEditAdminUserVM);
            }
            //parameter
            AdminUser adminInfo = new AdminUser();
            adminInfo.Name = addEditAdminUserVM.Name;
            adminInfo.Email = addEditAdminUserVM.Email;
            //if admin id is null then insert
            if (!addEditAdminUserVM.AdminID.HasValue)
            {
                adminInfo.PasswordSalt = PasswordGenerator.GetSalt();
                adminInfo.RoleId = (int)UserRole.Admin;
                adminInfo.PasswordHash = PasswordGenerator.GetHashedPassword(adminInfo.PasswordSalt, addEditAdminUserVM.Password);
                db.AdminUsers.Insert(db, adminInfo);
            }
            //update admin details 
            else
            {
                adminInfo.Id = addEditAdminUserVM.AdminID.Value;
                db.AdminUsers.UpdatePartial(db, adminInfo, true, "Name", "Email");
            }
        }
        #endregion

        #region Delete Admin
        /// <summary>
        /// Delete Admin By  AdminId
        /// </summary>
        public void DeleteAdminById(int adminId)
        {
            AdminUser admin = new AdminUser();
            admin.Id = adminId;
            db.AdminUsers.Delete(db, admin);
        }
        #endregion

        #region check Is Admin Duplicate
        /// <summary>
        /// Check if Admin user is exist or not  by its username
        /// if Admin exist then exception occurs
        /// </summary>
        /// <param name="addEditAdminUserVM"></param>
        public void CheckAdminDuplicate(AddEditAdminUserVM addEditAdminUserVM)
        {
            var checkEmailIsDuplicate = (from admin in db.AdminUsers
                                         where admin.Email == addEditAdminUserVM.Email
                                         select admin).Any();

            if (checkEmailIsDuplicate)
            {
                throw new BusinessException("400", "Admin user already exist.", autoShow: true);
            }
        }

        public void CheckAdminDuplicateInUpdateAction(AddEditAdminUserVM addEditAdminUserVM)
        {
            var checkEmailIsDuplicate = (from admin in db.AdminUsers
                                         where admin.Email == addEditAdminUserVM.Email && admin.Id != addEditAdminUserVM.AdminID
                                         select admin).Any();

            if (checkEmailIsDuplicate)
            {
                throw new BusinessException("400", "Admin user already exist.", autoShow: true);
            }
        }


        #endregion

        #region Delete Record By USDOT Number
        /// <summary>
        /// Delete Record By USDOT number 
        /// After Delete this record add this Information in delete record table
        /// </summary>
        /// <param name="USDOTNumber"></param>
        public void DeleteRecordByUSDOTNumber(int USDOTNumber)
        {
            //check if usdotNumber exist or not in TranspportCompany Table
            //if return false then throw exception
            var checkIsUSDOTNumberExist = (from companies in db.TransportCompanies
                                           where companies.USDOTNumber == USDOTNumber
                                           select companies).Any();


            if (checkIsUSDOTNumberExist)
            {
                var transactionBusiness = db.Database.BeginTransaction();
                try
                {
                    //Check business is exist or not on current USDOTNumber
                    var isBusinessExistOrNot = (from busines in db.Businesses where busines.USDOTNumber == USDOTNumber select busines).Any();

                    // Delete ReviewReplies
                    var replies = db.ReviewReplies.Where(r => r.CompanyUSDOT == USDOTNumber || r.Review.ReviewerUSDOT == USDOTNumber).ToList();
                    db.ReviewReplies.RemoveRange(replies);

                    // Delete Reviews
                    var reviews = db.Reviews.Where(r => r.CompanyUSDOT == USDOTNumber || r.ReviewerUSDOT == USDOTNumber).ToList();
                    db.Reviews.RemoveRange(reviews);

                    //If USDOTNumber is exist in business table then delete business record.
                    if (isBusinessExistOrNot)
                    {
                        //Now delete this records by usdotnumber from business table
                        Business business = new Business();
                        //set USDOTNumber
                        business.USDOTNumber = USDOTNumber;
                        db.Businesses.Delete(db, business);
                    }

                    //Now delete this records by usdotnumber
                    TransportCompany record = new TransportCompany();
                    //set ID
                    record.USDOTNumber = USDOTNumber;
                    db.TransportCompanies.Delete(db, record);

                    //after successful deletion add this USDOTNumber in the DeletedRecord table
                    DeletedRecord deletedRecord = new DeletedRecord();
                    deletedRecord.USDOTNumber = USDOTNumber;
                    deletedRecord.RecordDeletedBy = (int)FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName")).UserId;
                    deletedRecord.DeletedOn = DateTime.Now;
                    //insert values
                    db.DeletedRecords.Insert(db, deletedRecord);
                    transactionBusiness.Commit();
                }
                catch (Exception ex)
                {

                    //Rollback transaction
                    transactionBusiness.Rollback();
                    throw ex;
                }

            }
            else
            {
                throw new BusinessException("404", "USDOT number does not exist.", true);
            }
        }
        #endregion

        #region UpdateRecordFromPreTransportCompanyTable

        public void UpdateRecordFromPreMainTable(ProcessRecordsVM vm)
        {
            // initialize the progress information
            UpdateRecordProgressInfo = GetUpdateRecordProgressInfo();
            UpdateRecordProgressInfo.RecordsSuccessful = 0;
            UpdateRecordProgressInfo.RecordsToProcess = vm.RecordsToProcess; // number of records user wants to process, this value never changes during the process
            UpdateRecordProgressInfo.IsInProgress = true;
            UpdateRecordProgressInfo.RecordsCouldNotFetch = 0;
            UpdateRecordProgressInfo.RecordFailedDuringLatlng = 0;
            UpdateRecordProgressInfo.CheckIsRecordsEnoughToUpdate = true;
            UpdateRecordProgressInfo.TotalCountOfAvailableRecordToUpdate = 0;
            UpdateRecordProgressInfo.TotalRecordCount = vm.RecordsToProcess;
            UpdateRecordProgressInfo.Errors.Clear();
            UpdateRecordProgressInfo.RecordsFailDuringUpdate = new List<int?>();

            //do not db dispose in thread
            using (var dbCopyData = new PartnerCarrier_DevEntities(true, null, true, null, null))
            {
                bool isLogEnable = vm.IsDataMigrationLogEnabled; //for Enable/Disable log
                int currentRecordIndex = 0; // take this to get the currently updated record number during running process
                var currentRecordUSDOTNumber = 0;
                var objCtx = ((IObjectContextAdapter)dbCopyData).ObjectContext;

                try
                {


                    int? isForeignkeyExists; // take variable for check foreign key is exists or not..

                    ///using sql connection check foreign key is exist or not in business table by constraint name.
                    ///IF USDOTNumber is exist in main Transport table then we delete that record from main transport table.
                    ///but USDOTNumber is also exist in Business table if user create his job so we need to drop foreign key from Business table 
                    ///Once whole process will complete then we set foreign key back.
                    using (SqlConnection connection = new SqlConnection(dbCopyData.Database.Connection.ConnectionString))
                    {
                        SqlCommand command = new SqlCommand(@"SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Business_TransportCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[Business]')", connection);
                        connection.Open(); // Open the connection
                        isForeignkeyExists = (int?)command.ExecuteScalar(); // If foreign key exists then it will return 1 otherwise return null
                        connection.Close(); // Close the connection
                    }
                    if (isForeignkeyExists == 1)
                        dbCopyData.Database.ExecuteSqlCommand("ALTER TABLE Business DROP Constraint FK_Business_TransportCompany"); // if foreign key exists then DROP foreign key FK_Business_TransportCompany constraint

                    DateTime startTime = DateTime.Now;
                    AppLogger.Instance.Log("Process Started :  " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), LogType.Info, null, isLogEnable);

                    var nextRecordsToSkip = 0;

                    // Example : if records to process entered by user = 20, then Take 20 only from db, instead of firing query to take 50.
                    var nextRecordsToTake = 50;
                    if (vm.RecordsToProcess < nextRecordsToTake)
                        nextRecordsToTake = vm.RecordsToProcess;

                    var preTransportCompanyReocrds = new List<PreTransportCompany>();
                    //get list of cargotypes from cargotype table
                    var listCargoTypes = (from list in dbCopyData.CargoTypes
                                          select list).ToList();
                    //get list of servicetype from servicetype table
                    var listServiceTypes = (from list in dbCopyData.ServiceTypes
                                            select list).ToList();
                    //check how many record are remaining to update
                    UpdateRecordProgressInfo.TotalCountOfAvailableRecordToUpdate = (from preCompany in dbCopyData.PreTransportCompanies.AsNoTracking()

                                                                                    join company in dbCopyData.TransportCompanies.AsNoTracking() on preCompany.USDOTNumber equals company.USDOTNumber into joinedCompanies
                                                                                    from matchedCompany in joinedCompanies.DefaultIfEmpty()

                                                                                    join deletedRecord in dbCopyData.DeletedRecords.AsNoTracking() on preCompany.USDOTNumber equals deletedRecord.USDOTNumber into joinedDeletedRecords
                                                                                    from matchedDeletedRecord in joinedDeletedRecords.DefaultIfEmpty()

                                                                                    where
                                                                                    (
                                                                                      matchedDeletedRecord == null // USDOT must not be deleted
                                                                                      && (matchedCompany == null // USDOT not already in main
                                                                                      || preCompany.DateLastChanged != matchedCompany.DateLastChanged) // USDOT exist in main but date is different in premain and main
                                                                                    )
                                                                                    select preCompany).Count();
                    AppLogger.Instance.Log(string.Format("Remaining record to be process count :{0}", UpdateRecordProgressInfo.TotalCountOfAvailableRecordToUpdate), LogType.Info, null, isLogEnable);

                    //if records user want to process is more than actually records available for process, then change records to process to match that. We can't process more records than how many records are available to process
                    if (UpdateRecordProgressInfo.TotalCountOfAvailableRecordToUpdate < vm.RecordsToProcess)
                    {
                        UpdateRecordProgressInfo.RecordsToProcess = UpdateRecordProgressInfo.TotalCountOfAvailableRecordToUpdate;
                        vm.RecordsToProcess = UpdateRecordProgressInfo.TotalCountOfAvailableRecordToUpdate;
                        UpdateRecordProgressInfo.CheckIsRecordsEnoughToUpdate = false;
                    }
                    do
                    {
                        try
                        {
                            AppLogger.Instance.Log(string.Format("Fetch Records Started, Skip:{0}, Take:{1}", nextRecordsToSkip, nextRecordsToTake), LogType.Info, null, isLogEnable);
                            //Get Those record from PreTransportCompany  which has not in TransportCompany and also dateLastchanged is not equal to PreTransportCompany table of each Usdotnumber using left join
                            //also get record which is not in deleteRecord    
                            preTransportCompanyReocrds = (from preCompany in dbCopyData.PreTransportCompanies.AsNoTracking()

                                                          join company in dbCopyData.TransportCompanies.AsNoTracking() on preCompany.USDOTNumber equals company.USDOTNumber into joinedCompanies
                                                          from matchedCompany in joinedCompanies.DefaultIfEmpty()

                                                          join deletedRecord in dbCopyData.DeletedRecords.AsNoTracking() on preCompany.USDOTNumber equals deletedRecord.USDOTNumber into joinedDeletedRecords
                                                          from matchedDeletedRecord in joinedDeletedRecords.DefaultIfEmpty()

                                                              //join recordsDuringUpdate in UpdateRecordProgressInfo.RecordsFailDuringUpdate on preCompany.USDOTNumber equals recordsDuringUpdate into joinedFailedRecords
                                                              //from matchedFailedRecords in joinedFailedRecords.DefaultIfEmpty()

                                                          where
                                                          (
                                                           UpdateRecordProgressInfo.RecordsFailDuringUpdate.Contains(preCompany.USDOTNumber) == false &&
                                                            matchedDeletedRecord == null // USDOT must not be deleted
                                                            && (matchedCompany == null // USDOT not already in main
                                                            || preCompany.DateLastChanged != matchedCompany.DateLastChanged) // USDOT exist in main but date is different in premain and main
                                                          )
                                                          orderby preCompany.USDOTNumber
                                                          select preCompany).Take(nextRecordsToTake).ToList();
                            AppLogger.Instance.Log(string.Format("Fetch Records Ended, Skip:{0}, Take:{1}", nextRecordsToSkip, nextRecordsToTake), LogType.Info, null, isLogEnable);
                        }
                        catch (Exception ex)
                        {
                            //if error occurs during query execution then process stopped and display error
                            AppLogger.Instance.Log(string.Format("Error In Fetch Records, Skip:{0}, Take:{1}", nextRecordsToSkip, nextRecordsToTake), LogType.Info, null);
                            AppLogger.Instance.Log(ex);

                            UpdateRecordProgressInfo.RecordsCouldNotFetch = UpdateRecordProgressInfo.RecordsToProcess - (UpdateRecordProgressInfo.RecordsSuccessful + UpdateRecordProgressInfo.Errors.Count);
                            UpdateRecordProgressInfo.IsInProgress = false;

                            // to break the do while loop
                            nextRecordsToSkip = UpdateRecordProgressInfo.RecordsToProcess;
                        }

                        if (UpdateRecordProgressInfo.IsInProgress)
                        {
                            //create list object for TransportCompany table
                            TransportCompany transportCompany = new TransportCompany();
                            for (var i = 0; i < preTransportCompanyReocrds.Count; i++)               //craete loop in which i< joinequery result record
                            {
                                if (!UpdateRecordProgressInfo.IsInProgress) break; // If user click on Cancel button then we set UpdateRecordProgressInfo.IsInProgress = false but if process is in iterate loop then it will not check for process is in progress or not? so when iterate loop start we just check that process is in progress or not, If not then break iterate loop.
                                currentRecordIndex++;
                                try
                                {
                                    currentRecordUSDOTNumber = preTransportCompanyReocrds[i].USDOTNumber;
                                    //now check if record is exist in main table 
                                    var transportCompanysExistInMain = (from records in dbCopyData.TransportCompanies.AsNoTracking()
                                                                        where records.USDOTNumber == currentRecordUSDOTNumber
                                                                        select records).FirstOrDefault();

                                    // if USDOTNUmber already exist in main table, delete it and re-insert it, else just insert it.
                                    if (transportCompanysExistInMain != null)
                                    {
                                        AppLogger.Instance.Log(string.Format("Starting - Delete Record From Db - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);
                                        var deletedCompany = new TransportCompany { USDOTNumber = currentRecordUSDOTNumber };
                                        dbCopyData.TransportCompanies.Delete(dbCopyData, deletedCompany);
                                        if (objCtx.ObjectStateManager.TryGetObjectStateEntry(deletedCompany, out _))
                                            dbCopyData.TransportCompanies.Detach(dbCopyData, deletedCompany);
                                        AppLogger.Instance.Log(string.Format("Ending - Delete Record From Db - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    }

                                    //Now Insert PreTransportCompany’s record into TransportCompany
                                    transportCompany = new TransportCompany();
                                    transportCompany.InjectFrom(preTransportCompanyReocrds[i]);

                                    //set Company Name                                
                                    transportCompany.CompanyName = !string.IsNullOrEmpty(preTransportCompanyReocrds[i].DoingBusinessAsName) ? preTransportCompanyReocrds[i].DoingBusinessAsName : preTransportCompanyReocrds[i].LegalName;
                                    AppLogger.Instance.Log(string.Format("Set CompanName {0} - USDOTNumber: {1}, RecordIndex: {2}", transportCompany.CompanyName, currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    AppLogger.Instance.Log(string.Format("Starting - Set CargoType & Service Type - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);
                                    AddUpdateRecordInMainCargoTypeAndServiceType(transportCompany, listCargoTypes, listServiceTypes);
                                    AppLogger.Instance.Log(string.Format("Ending   - Set CargoType & Service Type - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    AppLogger.Instance.Log(string.Format("Starting - Sort Calculation - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);
                                    CalculateSortRelavance(transportCompany);
                                    AppLogger.Instance.Log(string.Format("Ending   - Sort Calculation - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    AppLogger.Instance.Log(string.Format("Starting - Lat/Long Calculation - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    //To avoid recalculating of Main.Latitude and Main.Longitude when updating existing company and address didn't change.
                                    //To avoid unnecessary request to Geocoding requests to Google Maps and also speed up update process by avoiding pause between API calls
                                    //By comparing preMain with Main Table with below 4 fileds PhysicalAddressNationality, PhysicalAddressStateCode, PhysicalAddressCity, PhysicalAddressStreet if this fields are same then no need to Geocoding requests to Google Map.
                                    //So by this way we can reduces the unnecessary request.
                                    if (transportCompanysExistInMain != null)
                                    {
                                        if (preTransportCompanyReocrds[i].PhysicalAddressNationality != transportCompanysExistInMain.PhysicalAddressNationality ||
                                        preTransportCompanyReocrds[i].PhysicalAddressStateCode != transportCompanysExistInMain.PhysicalAddressStateCode ||
                                        preTransportCompanyReocrds[i].PhysicalAddressCity != transportCompanysExistInMain.PhysicalAddressCity ||
                                        preTransportCompanyReocrds[i].PhysicalAddressStreet != transportCompanysExistInMain.PhysicalAddressStreet)
                                        {
                                            CalculateLattitudeAndLongitude(transportCompany, isLogEnable);
                                        }
                                        else
                                        {
                                            //if Latitude & Longtitude is already in memory then no need to request calculating latitude & longitude
                                            //else is not in memory then we request to calculate Latitude & Longtitude here
                                            if (transportCompanysExistInMain.Latitude != null || transportCompanysExistInMain.Longitude == null)
                                            {
                                                transportCompany.Latitude = transportCompanysExistInMain.Latitude;
                                                transportCompany.Longitude = transportCompanysExistInMain.Longitude;
                                            }
                                            else
                                            {
                                                CalculateLattitudeAndLongitude(transportCompany, isLogEnable);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        CalculateLattitudeAndLongitude(transportCompany, isLogEnable);
                                    }

                                    AppLogger.Instance.Log(string.Format("Ending   - Lat/Long Calculation - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    AppLogger.Instance.Log(string.Format("Starting - Add Record To Db - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);
                                    dbCopyData.TransportCompanies.Insert(dbCopyData, transportCompany);
                                    if (objCtx.ObjectStateManager.TryGetObjectStateEntry(transportCompany, out _))
                                        dbCopyData.TransportCompanies.Detach(dbCopyData, transportCompany);
                                    AppLogger.Instance.Log(string.Format("Ending   - Add Record To Db - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null, isLogEnable);

                                    UpdateRecordProgressInfo.RecordsSuccessful++;
                                }

                                catch (BusinessException ex)
                                {
                                    //if error occurs due to some reason then create log but do not stop update process
                                    var errorInfo = ex.ErrorMessages[0];
                                    AppLogger.Instance.Log(errorInfo.message, LogType.Info, null, isLogEnable);
                                    //AppLogger.Instance.Log(ex);

                                    ErrorDetailVM errorDetailVM = new ErrorDetailVM();
                                    errorDetailVM.USDOTNumber = transportCompany.USDOTNumber;
                                    errorDetailVM.ErrorMessage = errorInfo.message;
                                    // update info in progres
                                    UpdateRecordProgressInfo.Errors.Add(errorDetailVM);
                                    UpdateRecordProgressInfo.RecordsFailDuringUpdate.Add(transportCompany.USDOTNumber);
                                }
                                catch (Exception ex)
                                {
                                    //if error occurs due to some reason then create log but do not stop update process
                                    AppLogger.Instance.Log(string.Format("Error While Updating Record - USDOTNumber: {0}, RecordIndex: {1}", currentRecordUSDOTNumber, currentRecordIndex), LogType.Info, null);
                                    AppLogger.Instance.Log(ex);

                                    ErrorDetailVM errorDetailVM = new ErrorDetailVM();
                                    errorDetailVM.USDOTNumber = transportCompany.USDOTNumber;
                                    errorDetailVM.ErrorMessage = string.Format("Error While Updating Record - USDOTNumber: {0}, RecordIndex: {1}, Error Message: {2}", currentRecordUSDOTNumber, currentRecordIndex, ex.Message);
                                    // update info in progress
                                    UpdateRecordProgressInfo.Errors.Add(errorDetailVM);
                                    UpdateRecordProgressInfo.RecordsFailDuringUpdate.Add(transportCompany.USDOTNumber);
                                }
                            }
                            //change value of skip records
                            nextRecordsToSkip += nextRecordsToTake;
                            var remainingRecordToBeProcess = vm.RecordsToProcess - nextRecordsToSkip;
                            AppLogger.Instance.Log(string.Format("Remaining records to be process : {0}", remainingRecordToBeProcess), LogType.Info, null, isLogEnable);
                            if (remainingRecordToBeProcess <= nextRecordsToTake && remainingRecordToBeProcess > 0)
                            {
                                nextRecordsToTake = remainingRecordToBeProcess;
                                AppLogger.Instance.Log(string.Format("Next Record To Take after check how many records are remaining to update from user entered : {0}", nextRecordsToTake), LogType.Info, null, isLogEnable);
                            }
                        }
                        if (!UpdateRecordProgressInfo.IsInProgress) break; // If user click on Cancel button then we set UpdateRecordProgressInfo.IsInProgress = false but if process is in iterate loop then it will not check for process is in progress or not? so when iterate loop start we just check that process is in progress or not, If not then break iterate loop.
                    }
                    while (nextRecordsToSkip < UpdateRecordProgressInfo.RecordsToProcess); // process stops when there's no more records to process
                    UpdateRecordProgressInfo.IsInProgress = false;
                    AppLogger.Instance.Log("Process Ended :  " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), LogType.Info, null, isLogEnable);


                }
                finally
                {
                    int? isForeignkeyExists; // take variable for check foreign key is exists or not..

                    ///using sql connection check foreign key is exist or not in business table by constraint name.
                    using (SqlConnection connections = new SqlConnection(dbCopyData.Database.Connection.ConnectionString))
                    {
                        connections.Open(); // Open the connection
                        SqlCommand command = new SqlCommand(@"SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Business_TransportCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[Business]')", connections);
                        isForeignkeyExists = (int?)command.ExecuteScalar(); // If foreign key exists then it will return 1 otherwise return null
                        connections.Close();// Close the connection
                    }
                    if (isForeignkeyExists == null)
                        dbCopyData.Database.ExecuteSqlCommand("ALTER TABLE Business ADD CONSTRAINT FK_Business_TransportCompany FOREIGN KEY (USDOTNumber)REFERENCES TransportCompany(USDOTNumber)"); // if foreign key doesn't exists then add foreign key FK_Business_TransportCompany constraint
                }
            }
        }
        #endregion

        #region Add/Update Record In MainCargoType And ServiceType
        /// <summary>
        /// Update Cargotype and ServiceType Table of mainTransportCompany table
        /// </summary>
        /// <param name="transportCompany"></param>
        public void AddUpdateRecordInMainCargoTypeAndServiceType(TransportCompany transportCompany, List<CargoType> listCargoTypes, List<ServiceType> listServiceTypes)
        {
            try
            {
                //first check all condition as required and set values in TransportCompany_CargoType and TransportCompany_ServiceType Table
                if (transportCompany.CargoTransportedAGeneralFreight == "X")
                {

                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 1));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 1));
                }
                if (transportCompany.CargoTransportedBHouseholdGoods == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 2));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 1));
                }
                if (transportCompany.CargoTransportedCMetalSheetsCoilsRolls == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 3));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.CargoTransportedDMotorVehicles == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 4));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 6));
                }
                if (transportCompany.CargoTransportedFLogsPolesBeamsLumber == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 6));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.CargoTransportedGBuildingMaterials == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 7));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.CargoTransportedHMobileHomes == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 8));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 7));
                }
                if (transportCompany.CargoTransportedIMachineryLargeObjects == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 9));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.CargoTransportedJFreshProduce == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 10));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 3));
                }
                if (transportCompany.CargoTransportedKLiquidsGases == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 11));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 8));
                }
                if (transportCompany.CargoTransportedLIintermodalContainers == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 12));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 9));
                }
                if (transportCompany.CargoTransportedNOilfieldEquipment == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 14));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.CargoTransportedOLivestock == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 15));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 10));
                }
                if (transportCompany.CargoTransportedPGrainFeedHay == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 16));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 4));
                }
                if (transportCompany.CargoTransportedQCoalCoke == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 17));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 11));
                }
                if (transportCompany.CargoTransportedRMeat == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 18));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 3));
                }
                if (transportCompany.CargoTransportedTUSMail == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 20));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 1));
                }
                if (transportCompany.CargoTransportedUChemicals == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 21));
                    //transportCompany.ServiceTypes.Add(null);
                }
                if (transportCompany.CargoTransportedVCommoditiesDryBulk == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 22));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 4));
                }
                if (transportCompany.CargoTransportedWRefrigeratedFood == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 23));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 3));
                }
                if (transportCompany.CargoTransportedXBeverages == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 24));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 1));
                }
                if (transportCompany.CargoTransportedYPaperProducts == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 25));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 1));
                }
                if (transportCompany.CargoTransportedZUtility == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 26));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 1));
                }
                if (transportCompany.CargoTransportedAAFarmSupplies == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 27));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.CargoTransportedBBConstruction == "X")
                {
                    transportCompany.CargoTypes.Add(listCargoTypes.Single(m => m.CargoNumber == 28));
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 2));
                }
                if (transportCompany.HazmatIndicator == "Y")
                {
                    //transportCompany.CargoTypes.Add(null);
                    transportCompany.ServiceTypes.Add(listServiceTypes.Single(m => m.ServiceTypeNumber == 5));
                }
            }
            catch (Exception ex)
            {
                throw new BusinessException("400", string.Format("Error Setting Cargotype and Servicetype For USDOTNumber: {0}, Error Message:  {1}", transportCompany.USDOTNumber, ex.Message), true);
            }
        }
        #endregion

        #region Calculate Sort Relavance
        /// <summary>
        /// Calculate SortRelavance  
        /// first get business years of this company
        /// set truckandtractors point
        /// set hazmatIndicatorPoint point
        /// check company has flatbed service and then set point as required
        /// check company has reefer service and then set point as required
        /// calculate sortRelevance
        /// </summary>
        public void CalculateSortRelavance(TransportCompany transportCompany)
        {
            //variables
            int businessYearPoint = 0;
            int truckAndTractorsPoint = 0;
            int hazmatIndicatorPoint = 0;
            int flatbedPoint = 0;
            int reeferPoint = 0;
            TimeSpan totalTimeSpan;
            double years = 0;
            try
            {
                //get TruckandTractors From logic which has given
                transportCompany.TrucksAndTractors = transportCompany.TotalNumberOfPowerUnits;   //Arkady April 5 2026    transportCompany.NNEquipmentUnitsOwnedTruck + transportCompany.NNEquipmentUnitsOwnedTractor + transportCompany.NNEquipmentUnitsTermLeasedTruck + transportCompany.NNEquipmentUnitsTermLeasedTractor;
                DateTime dateAdded;
                //we are getting dateAdded in int,therefore first we have to convert it into datetime and get year
                if (DateTime.TryParseExact(transportCompany.DateAdded.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateAdded))
                {
                    totalTimeSpan = DateTime.Now - dateAdded;
                    //get year difference from between current year and year from dateAdded
                    years = Convert.ToInt32(totalTimeSpan.TotalDays / 365.25);  // 365 1/4 days per year
                }
                //now set businessYear point by check these conditions
                if (years < 1)
                {
                    businessYearPoint = -5;
                }
                else if (years >= 1 && years < 2)
                {
                    businessYearPoint = -3;
                }
                else if (years >= 2 && years < 5)
                {
                    businessYearPoint = 0;
                }
                else if (years >= 5 && years < 8)
                {
                    businessYearPoint = 5;
                }
                else if (years >= 8 && years < 10)
                {
                    businessYearPoint = 7;
                }
                else if (years >= 10)
                {
                    businessYearPoint = 10;
                }
                //now set truckAndTractorsPoint  by applying these conditions
                if (transportCompany.TrucksAndTractors == 1)
                {
                    truckAndTractorsPoint = -10;
                }
                else if (transportCompany.TrucksAndTractors >= 2 && transportCompany.TrucksAndTractors < 5)
                {
                    truckAndTractorsPoint = -5;
                }
                else if (transportCompany.TrucksAndTractors >= 5 && transportCompany.TrucksAndTractors < 10)
                {
                    truckAndTractorsPoint = 0;
                }
                else if (transportCompany.TrucksAndTractors >= 10 && transportCompany.TrucksAndTractors < 15)
                {
                    truckAndTractorsPoint = 2;
                }
                else if (transportCompany.TrucksAndTractors >= 15 && transportCompany.TrucksAndTractors < 20)
                {
                    truckAndTractorsPoint = 5;
                }
                else if (transportCompany.TrucksAndTractors >= 20 && transportCompany.TrucksAndTractors < 30)
                {
                    truckAndTractorsPoint = 10;
                }
                else if (transportCompany.TrucksAndTractors >= 30 && transportCompany.TrucksAndTractors < 50)
                {
                    truckAndTractorsPoint = 20;
                }
                else if (transportCompany.TrucksAndTractors >= 50 && transportCompany.TrucksAndTractors < 75)
                {
                    truckAndTractorsPoint = 30;
                }
                else if (transportCompany.TrucksAndTractors >= 75 && transportCompany.TrucksAndTractors < 100)
                {
                    truckAndTractorsPoint = 40;
                }
                else if (transportCompany.TrucksAndTractors >= 100)
                {
                    truckAndTractorsPoint = 50;
                }
                //set HazmatIndicatorpoint 
                if (transportCompany.HazmatIndicator == "Y")
                {
                    hazmatIndicatorPoint = 10;
                }
                //check if company has flatbed or not then set point as required
                if (transportCompany.ServiceTypes.Where(x => x.Service_Type == "Flatbed").Any())
                {
                    flatbedPoint = 10;
                }
                //check if company has reefer or not then set point as required
                if (transportCompany.ServiceTypes.Where(x => x.Service_Type == "Reefer").Any())
                {
                    reeferPoint = 5;
                }
                //finally calculate sortRelevance
                transportCompany.SortRelevance = businessYearPoint + truckAndTractorsPoint + hazmatIndicatorPoint + flatbedPoint + reeferPoint;
            }
            catch (Exception ex)
            {
                throw new BusinessException("400", string.Format("Error While Calculating Sort relevance For USDOTNumber: {0}, Error Message:  {1}", transportCompany.USDOTNumber, ex.Message), true);
            }
        }
        #endregion

        #region Get Latitude and longitude for each company
        /// <summary>
        /// Send GeoCode GoogleApi request and
        /// get Latitude and longitude from the response
        /// here, we have set Time Delay of 2 seconds after on google api response comes.
        /// after getting latitude and longitude from response
        /// set into our transport company table for each record
        /// </summary>
        /// <param name="transportCompany"></param>
        public void CalculateLattitudeAndLongitude(TransportCompany transportCompany, bool isLogEnable)
        {
            string CompanyAddressWithUsDotNumber = null;
            try
            {
                //google api geocode request is not support characters like # into address so if address contains then remove it 
                var fullAddress = (transportCompany.PhysicalAddressStreet + "," + transportCompany.PhysicalAddressCity + "," + transportCompany.PhysicalAddressStateCode + "," + transportCompany.PhysicalAddressZipCode).Replace("#", "");
                //create request url with address
                //get API Key from web.config file
                var googleAPIKey = Config.GetValue("GoogleAPIKeyForLatLong");
                var url = string.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}&key=" + googleAPIKey, fullAddress.Replace(" ", "+"));
                CompanyAddressWithUsDotNumber = "UsDotNumber=" + transportCompany.USDOTNumber + " And FullAddress=" + fullAddress;
                //get response from url, which will return in json formatr
                var response = new System.Net.WebClient().DownloadString(url);
                AppLogger.Instance.Log(string.Format("Response From Google Api For latitude and longitude: {0}", response), LogType.Info, null, isLogEnable);
                //set Time Delay of 2 seconds after on google api response comes  
                Thread.Sleep(500);
                //Convert Json data to our viewmodel object
                GoogleGeoCodeResponse googleGeoCodeResponse = new JavaScriptSerializer().Deserialize<GoogleGeoCodeResponse>(response);
                foreach (var companyAddress in googleGeoCodeResponse.results)
                {
                    //set latitude from response
                    transportCompany.Latitude = companyAddress.geometry.location.lat;
                    //set longitude from response
                    transportCompany.Longitude = companyAddress.geometry.location.lng;
                }
            }
            catch (Exception ex)
            {
                //if error occurs due to some reason then create log but do not stop update process
                AppLogger.Instance.Log(string.Format("Error While Getting Latlng on This Address {0}, For USDOTNumber:{1} ", CompanyAddressWithUsDotNumber, transportCompany.USDOTNumber));
                AppLogger.Instance.Log(ex);

                ErrorDetailVM errorDetailVM = new ErrorDetailVM();
                errorDetailVM.USDOTNumber = transportCompany.USDOTNumber;
                errorDetailVM.ErrorMessage = string.Format("Error While Getting Latlng on This Address {0}, For USDOTNumber:{1}, Error Message: {2}", CompanyAddressWithUsDotNumber, transportCompany.USDOTNumber, ex.Message);
                // update info in progres
                UpdateRecordProgressInfo.Errors.Add(errorDetailVM);
                UpdateRecordProgressInfo.RecordsFailDuringUpdate.Add(transportCompany.USDOTNumber);
                UpdateRecordProgressInfo.RecordFailedDuringLatlng++;
            }
        }
        #endregion

        #region Updated Record Information
        /// <summary>
        /// Get Update Record Progress Information
        /// </summary>
        /// <returns></returns>
        public UpdateRecordProgressInfo GetUpdateRecordProgressInfo()
        {
            if (UpdateRecordProgressInfo == null)
            {
                UpdateRecordProgressInfo = new UpdateRecordProgressInfo()
                {
                    Errors = new List<ErrorDetailVM>()
                };
            }
            return UpdateRecordProgressInfo;
        }
        #endregion

        /// <summary>
        /// Cancel Update record process
        /// </summary>
        /// <returns></returns>
        public UpdateRecordProgressInfo CancelUpdateProcess()
        {
            if (UpdateRecordProgressInfo == null)
            {
                UpdateRecordProgressInfo = new UpdateRecordProgressInfo()
                {
                    Errors = new List<ErrorDetailVM>()
                };
            }
            else
            {
                // UpdateRecordProgressInfo is not null means current process is running so we need to just set  UpdateRecordProgressInfo.IsInProgress = false; and return object.
                UpdateRecordProgressInfo.IsInProgress = false;
            }
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName")); //Get LoggedIn User
            var adminUsers = db.AdminUsers.Where(x => x.Id == loggedInUser.UserId).FirstOrDefault(); // Get Admin user by logged in user for get IsDataMigrationLogEnabled value for logged in App-log.txt or not.
            AppLogger.Instance.Log("Process Canceled :  " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), LogType.Info, null, adminUsers.IsDataMigrationLogEnabled);

            return UpdateRecordProgressInfo;
        }

        /// <summary>
        /// Admin can enable disable log for the process of updating record by check checkbox
        /// set values in session
        /// </summary>
        /// <param name="commonUpdateRecordVM"></param>
        public void EnableDisableDataMigrationLog(ProcessRecordsVM commonUpdateRecordVM)
        {
            AdminUser user = new AdminUser();
            //get admin user id
            user.Id = (int)FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName")).UserId;
            user.IsDataMigrationLogEnabled = commonUpdateRecordVM.IsDataMigrationLogEnabled;
            //update value
            db.AdminUsers.UpdatePartial(db, user, true, "IsDataMigrationLogEnabled");
            //set value in the session
            HttpContext.Current.Session["IsDataLogEnabled"] = commonUpdateRecordVM.IsDataMigrationLogEnabled;


        }

        /// <summary>
        /// Get  Business/Waiting For Approval List
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public PagedList<BusinessOrWaitingForApprovalVM> GetBusinessOrWatingForApprovalList(PageSortPara ps)
        {
            int pageSize = 15;
            ps.p = ((!ps.p.HasValue) || ps.p == 0) ? 1 : Convert.ToInt32(ps.p);
            ps.se = String.IsNullOrEmpty(ps.se) ? "USDOTNumber" : ps.se;
            ps.sd = String.IsNullOrEmpty(ps.sd) ? "Asc" : ps.sd;

            var businessOrWaitingForApprovalList = (from businesses in db.Businesses.AsNoTracking()
                                                    where businesses.WebsiteApproved == null
                                                    orderby businesses.USDOTNumber
                                                    select new BusinessOrWaitingForApprovalVM
                                                    {
                                                        //BusinessID = businesses.Id,
                                                        USDOTNumber = businesses.USDOTNumber,
                                                        WebsiteName = businesses.Website,
                                                        EmailVerified = businesses.EmailVerified,
                                                        CommunicationApproved = businesses.CommunicationApproved,
                                                        WebsiteApproved = businesses.WebsiteApproved
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
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.WebsiteName);
                    break;
                case "WebsiteName_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.WebsiteName);
                    break;
                case "EmailVerified_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.EmailVerified);
                    break;
                case "EmailVerified_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.EmailVerified);
                    break;
                case "CommunicationApproved_Asc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderBy(s => s.CommunicationApproved);
                    break;
                case "CommunicationApproved_Desc":
                    businessOrWaitingForApprovalList = businessOrWaitingForApprovalList.OrderByDescending(s => s.CommunicationApproved);
                    break;
            }

            PagedList<BusinessOrWaitingForApprovalVM> allBusinessOrWaitingForApprovalList = businessOrWaitingForApprovalList.AsQueryable().SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
            return allBusinessOrWaitingForApprovalList;

        }

        /// <summary>
        /// Approve Website Request by ApproveID
        /// </summary>
        /// <param name="approvedId"></param>
        /// <returns></returns>
        public void ApproveWebsite(int approvedId)
        {
            var approved = (from busines in db.Businesses.AsNoTracking()
                            where busines.USDOTNumber == approvedId
                            select busines).FirstOrDefault();
            if (approved != null)
            {
                Business business = new Business();

                business.USDOTNumber = approvedId;
                business.WebsiteApproved = true;
                business.UpdatedDate = DateTime.Now;

                db.Businesses.UpdatePartial(db, business, true, "WebsiteApproved", "UpdatedDate");

            }

        }

        /// <summary>
        /// List for Clean Company Every Month
        /// Get List of all the company which city having only one company
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public PagedList<CleanCompanyEveryMonthVM> GetCleanMonthlyCompanyList(PageSortPara ps)
        {
            //set default page size
            int pageSize = 70;
            //If pageSortPara is null then initialize pageSortPara values
            PageSortPara.Init(ps, "PhysicalAddressCity", SortingDirection.Asc);


            var cleanCompanyMonthlyList = db.Database.SqlQuery<CleanCompanyEveryMonthVM>("SELECT T1.USDOTNumber, T1.PhysicalAddressCity As CityName, T1.PhysicalAddressStateCode as StateCode FROM TransportCompany T1 inner join TransportCompany T2 on T2.PhysicalAddressStateCode = T1.PhysicalAddressStateCode and T2.PhysicalAddressCity = T1.PhysicalAddressCity GROUP BY T1.PhysicalAddressCity,T1.PhysicalAddressStateCode, T1.USDOTNumber HAVING COUNT(T1.usdotnumber) = 1 ORDER BY T1.PhysicalAddressStateCode, T1.PhysicalAddressCity").AsQueryable();

            return cleanCompanyMonthlyList.SelectByPaging((int)ps.p, pageSize, ps.se, ps.sortDirection);
        }


        /// <summary>
        /// This method will clean all the records which city having only one company.
        /// delete record first from child table businesss then after delete from Transpport table records.
        /// </summary>
        public void ClearAllCompanyEveryMonth()
        {
            var transactionClearAllCompanyEveryMonth = db.Database.BeginTransaction();
            try
            {
                db.Database.ExecuteSqlCommand("Delete from Business where UsDOTNumber In (SELECT T1.USDOTNumber FROM TransportCompany T1 inner join TransportCompany T2 on T2.PhysicalAddressStateCode = T1.PhysicalAddressStateCode and T2.PhysicalAddressCity = T1.PhysicalAddressCity GROUP BY T1.PhysicalAddressCity,T1.PhysicalAddressStateCode, T1.USDOTNumber HAVING COUNT(T1.usdotnumber) = 1)");
                db.Database.ExecuteSqlCommand("Delete from TransportCompany where UsDOTNumber In(SELECT T1.USDOTNumber FROM TransportCompany T1 inner join TransportCompany T2 on T2.PhysicalAddressStateCode = T1.PhysicalAddressStateCode and T2.PhysicalAddressCity = T1.PhysicalAddressCity GROUP BY T1.PhysicalAddressCity,T1.PhysicalAddressStateCode, T1.USDOTNumber HAVING COUNT(T1.usdotnumber) = 1)");

                transactionClearAllCompanyEveryMonth.Commit();
            }
            catch (Exception ex)
            {
                //Rollback transaction
                transactionClearAllCompanyEveryMonth.Rollback();
                throw ex;
            }

        }

        /// <summary>
        /// Delete method will delete only selected company from Business then it delete from TransportCompany table.
        /// </summary>
        public void ClearSelectedCompanyEveryMonth(string USDOTNumber)
        {
            var transactionClearSelectedCompanyEveryMonth = db.Database.BeginTransaction();
            try
            {
                db.Database.ExecuteSqlCommand("Delete from Business where USDOTNumber In (" + USDOTNumber + ")");
                db.Database.ExecuteSqlCommand("Delete from TransportCompany where USDOTNumber In (" + USDOTNumber + ")");

                transactionClearSelectedCompanyEveryMonth.Commit();
            }
            catch (Exception ex)
            {
                //Rollback transaction
                transactionClearSelectedCompanyEveryMonth.Rollback();
                throw ex;
            }
        }

        /// <summary>
        /// Deletes all reviews and their replies for a given company identified by USDOTNumber.
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <exception cref="BusinessException"></exception>
        public void DeleteReviewsByUSDOTNumber(long USDOTNumber)
        {
            //check if usdotNumber exist or not in TranspportCompany Table
            //if return false then throw exception
            var checkIsUSDOTNumberExist = (from companies in db.TransportCompanies
                                           where companies.USDOTNumber == USDOTNumber
                                           select companies).Any();
            if (!checkIsUSDOTNumberExist)
                throw new BusinessException("404", "USDOT number does not exist.", true);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Get all review IDs for the company
                    var reviewIds = db.Reviews
                                      .Where(r => r.CompanyUSDOT == USDOTNumber)
                                      .Select(r => r.Id)
                                      .ToList();

                    if (reviewIds.Any())
                    {
                        // Delete replies for those reviews
                        var repliesToDelete = db.ReviewReplies
                                                .Where(rr => reviewIds.Contains(rr.ReviewId));
                        db.ReviewReplies.RemoveRange(repliesToDelete);

                        // Delete reviews for the company
                        var reviewsToDelete = db.Reviews
                                                .Where(r => r.CompanyUSDOT == USDOTNumber);
                        db.Reviews.RemoveRange(reviewsToDelete);

                        db.SaveChanges();
                    } else
                    {
                        throw new BusinessException("404", "No reviews exist for USDOT number.", true);
                    }

                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Rollback transaction if error
                    transaction.Rollback();
                    throw;
                }
            }

        }

    }
}


