using Common.Utility.ViewModels;
using PartnerCarrier.ViewModels.Admin;

namespace PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement
{
    public interface IAdminManagementService
    {
        PagedList<AdminUserVM> AdminUserList(PageSortPara ps, string searchText);

        AddEditAdminUserVM EditAdminById(long? adminId);

        void AddUpdateAdmin(AddEditAdminUserVM addEditAdminUserVM);

        void DeleteAdminById(int adminId);

        void CheckAdminDuplicate(AddEditAdminUserVM addEditAdminUserVM);

        void DeleteRecordByUSDOTNumber(int USDOTNumber);

        UpdateRecordProgressInfo GetUpdateRecordProgressInfo();

        UpdateRecordProgressInfo CancelUpdateProcess();

        void UpdateRecordFromPreMainTable(ProcessRecordsVM vm);

        void EnableDisableDataMigrationLog(ProcessRecordsVM commonUpdateRecordVM);

        PagedList<BusinessOrWaitingForApprovalVM> GetBusinessOrWatingForApprovalList(PageSortPara ps);

        void ApproveWebsite(int approvedId);

        /// <summary>
        /// Get List of all the company which city having only one company
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        PagedList<CleanCompanyEveryMonthVM> GetCleanMonthlyCompanyList(PageSortPara ps);

        /// <summary>
        /// Clean All Company Every Month grid data will be deleted from database.
        /// Clear all companies moethod delete all the companies from business then from transportcompanies table
        /// </summary>
        void ClearAllCompanyEveryMonth();

        /// <summary>
        /// Clear only selected data will be delete which user selected from Clean company every month grid
        /// </summary>
        /// <param name="deleteSeletedUSDOTNumber"></param>
        void ClearSelectedCompanyEveryMonth(string deleteSeletedUSDOTNumber);

        /// <summary>
        /// Deletes all reviews and their replies for a given company identified by USDOTNumber.
        /// </summary>
        /// <param name="USDOTNumber"></param>
        void DeleteReviewsByUSDOTNumber(long USDOTNumber);
    }
}
