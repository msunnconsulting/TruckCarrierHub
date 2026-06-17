namespace PartnerCarrier.Web.Areas.Admin.Controllers
{
    using Common.Utility;
    using Common.Utility.Logger;
    using Common.Utility.ViewModels;
    using Filters;
    using Infrastructure.Contracts.Admin.AdminManagement;
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ViewModels.Admin;

    [RouteArea("admin", AreaPrefix = "admin")]
    [RoutePrefix("user")]
    [AuthorizeRole(UserRole.Admin)]
    public class AdminUsersController : BaseController
    {
        #region private variable
        private readonly IAdminManagementService _adminManagementService;
        private static object lockUpdateRecordMethod = new object();
        #endregion

        #region constructor
        public AdminUsersController(IAdminManagementService adminManagementService)
        {
            _adminManagementService = adminManagementService;
        }
        #endregion

        #region Admin User List
        /// <summary>
        /// Get Admin user list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        public ActionResult AdminUserList(PageSortPara ps)
        {
            return View("~/Areas/Admin/Views/AdminUsers/AdminUserList.cshtml", ps);
        }

        /// <summary>
        /// Admin User List Partial
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        [Route("list-partial")]
        public ActionResult AdminUserListPartial(PageSortPara ps, string searchText)
        {
            try
            {
                var adminUsers = _adminManagementService.AdminUserList(ps, searchText);
                return PartialView("~/Areas/Admin/Views/AdminUsers/AdminUserListPartial.cshtml", adminUsers);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion


        #region Add/Update admin User
        /// <summary>
        /// Create Adminuser GET method
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("add")]
        public ActionResult CreateAdminUser()
        {
            AddEditAdminUserVM createCountyUserVM = new AddEditAdminUserVM();
            return View("AddeditAdminUser", createCountyUserVM);
        }

        /// <summary>
        /// Select admin User for edit action 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("edit/{id}")]
        public ActionResult EditAdminUser(int id, PageSortPara ps)
        {
            AddEditAdminUserVM addEditAdminUserVM = new AddEditAdminUserVM();
            //service call for get admin user by id.
            addEditAdminUserVM = _adminManagementService.EditAdminById(id);
            addEditAdminUserVM.pageSortPara = ps;
            return View("AddeditAdminUser", addEditAdminUserVM);
        }

        /// <summary>
        /// Add/ Update admin User 
        /// </summary>
        /// <param name="addEditAdminUserVM"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUpdateAdminUser(AddEditAdminUserVM addEditAdminUserVM)
        {

            if (addEditAdminUserVM.AdminID.HasValue)
            {
                ModelState.Remove("Password");
            }
            if (!ModelState.IsValid)
            {
                ReturnModelStateErrors();
            }
            try
            {
                _adminManagementService.AddUpdateAdmin(addEditAdminUserVM);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion

        #region Delete Admin User
        /// <summary>
        /// Delete admin user by its admin Id
        /// </summary>
        /// <param name="AdminID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("delete")]
        public ActionResult DeleteAdminUser(int AdminID)
        {
            try
            {
                _adminManagementService.DeleteAdminById(AdminID);
            }
            catch (Exception ex)
            {
                ReturnExceptionResult(ex);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete Record By USDOT Number
        /// <summary>
        /// delete record by USDOT number
        /// after that Submit This Number into deleterecord table
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("delete-record")]
        public ActionResult DeleteRecordByUSDOTNumber()
        {
            return View("~/Areas/Admin/Views/AdminUsers/DeleteRecordByUSDOTNumber.cshtml");
        }

        /// <summary>
        /// delete record by Usdot number post method
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("delete-record")]
        public ActionResult DeleteRecordByUSDOTNumber(int USDOTNumber)
        {
            try
            {
                _adminManagementService.DeleteRecordByUSDOTNumber(USDOTNumber);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion

        #region Update Record From PreMain Table
        /// <summary>
        /// Update Record From PreMain Table to main table and other table
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("update-record")]
        public ActionResult UpdateRecordFromPreMainTable()
        {
            return View("~/Areas/Admin/Views/AdminUsers/UpdateRecordFromPreTransportCompanyTable.cshtml");
        }

        /// <summary>
        /// Execute this method in asynchronous operation
        /// update record from pretransportCompany table to transport company table
        /// during process running make static variable isProcessInRunning=true
        /// </summary>
        /// <param name="commonUpdateRecordVM"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("update-record")]
        public ActionResult UpdateRecordFromPreMainTableToMain(ProcessRecordsVM commonUpdateRecordVM)
        {
            try
            {
                _adminManagementService.EnableDisableDataMigrationLog(commonUpdateRecordVM);
                //start process in new thread operation, this will execute in asynchronous
                lock (lockUpdateRecordMethod)
                {
                    var updateRecordProgress = _adminManagementService.GetUpdateRecordProgressInfo();
                    if (!updateRecordProgress.IsInProgress)
                    {
                        AppLogger.Instance.Log("Starting Update Record Process ...", LogType.Info, null, commonUpdateRecordVM.IsDataMigrationLogEnabled);
                        Task.Factory.StartNew(() =>
                        {
                            _adminManagementService.UpdateRecordFromPreMainTable(commonUpdateRecordVM);
                        });
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        throw new BusinessException(new ServerMessage("UpdateProcessAlreadyStarted", "Only one process can be started at a time. Someone already started the process. Please try reloading the page to see the progress", MessageType.Info, true));
                    }
                }
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// on click of cancel button just set isProcess = false
        /// and it will cancel update record process.
        /// </summary>
        /// <returns></returns>
        [Route("cancel-update")]
        [HttpPost]
        public ActionResult CancelUpdateProcess()
        {
            try
            {

                return Json(_adminManagementService.CancelUpdateProcess(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// every 1 second this method will be called 
        /// return total record value and updated record count every second
        /// after process complete change value isProcessRunning true to false
        /// </summary>
        /// <returns></returns>
        [Route("process-progress")]
        [HttpGet]
        public ActionResult UpdaterecordProgress()
        {
            try
            {
                return Json(_adminManagementService.GetUpdateRecordProgressInfo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        #endregion

        #region Clean Company Every Month
        /// <summary>
        /// Get Business List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("clean-company-monthly-list")]
        public ActionResult CleanCompanyEveryMonthList()
        {
            return View("~/Areas/Admin/Views/AdminUsers/CleanCompanyEveryMonthList.cshtml");
        }

        /// <summary>
        /// Business Search List Partial
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        [Route("clean-company-monthly-list-partial")]
        public ActionResult CleanCompanyEveryMonthListPartial(PageSortPara ps, CommonCleanCompanyEveryMonthAndPagelistVM commonCleanCompanyEveryMonthAndPagelistVM)
        {
            try
            {
                commonCleanCompanyEveryMonthAndPagelistVM.PagedListVM = _adminManagementService.GetCleanMonthlyCompanyList(ps);
                return PartialView("~/Areas/Admin/Views/AdminUsers/CleanCompanyEveryMonthListPartial.cshtml", commonCleanCompanyEveryMonthAndPagelistVM);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Clean all the companies which is available in grid
        /// Clean All companies which city having only one company.
        /// </summary>
        /// <returns></returns>
        [Route("clean-all-company-every-month")]
        public ActionResult ClearAllCompanyEveryMonth()
        {
            try
            {
                _adminManagementService.ClearAllCompanyEveryMonth();
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        /// <summary>
        /// Clean Only Selected companies which selected from grid
        /// </summary>
        /// <returns></returns>
        [Route("clean-selected-company-every-month")]
        public ActionResult ClearSelectedCompanyEveryMonth(string DeleteSeletedUSDOTNumber)
        {
            try
            {
                //check here if user selected usdotnumber it means it delete only selected records from the database
                //else it will Clean All companies which city having only one company.
                if (string.IsNullOrEmpty(DeleteSeletedUSDOTNumber))
                {
                    _adminManagementService.ClearAllCompanyEveryMonth();
                }
                else
                {
                    _adminManagementService.ClearSelectedCompanyEveryMonth(DeleteSeletedUSDOTNumber);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        #endregion

        #region Delete Reviews By USDOT Number
        /// <summary>
        /// delete record by USDOT number
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("delete-reviews")]
        public ActionResult DeleteReviewsByUSDOTNumber()
        {
            return View("~/Areas/Admin/Views/AdminUsers/DeleteReviewsByUSDOTNumber.cshtml");

        }

        /// <summary>
        /// delete reviews by Usdot number post method
        /// </summary>
        /// <param name="USDOTNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("delete-reviews")]
        public ActionResult DeleteReviewsByUSDOTNumber(long USDOTNumber)
        {
            try
            {
                _adminManagementService.DeleteReviewsByUSDOTNumber(USDOTNumber);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }
        #endregion
    }
}