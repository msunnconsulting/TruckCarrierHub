namespace PartnerCarrier.ViewModels.Admin
{
    using Common.Utility;
    using Common.Utility.ViewModels;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class AdminUserVM
    {
        public long AdminID { get; set; }

        public string LoginName { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }

    }

    public class AddEditAdminUserVM
    {
        public long? AdminID { get; set; }

        [DisplayName("Name")]
        [Required(ErrorMessage = "Please Enter Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter E-mail")]
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please Enter Valid E-mail Address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("Password")]
        [Required(ErrorMessage = "Please Enter Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password & Confirm Password does'nt match")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }

        public PageSortPara pageSortPara { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string OldPassword { get; set; }

        public bool IsDataMigrationLogEnabled { get; set; }
    }
    public class DeleteRecordByUSDOTNumberVM
    {
        [Required(ErrorMessage = "Please Enter USDOT Number")]
        public string USDOTNumber { get; set; }
    }

    public class CommonUpdateRecordVM
    {
        public UpdateRecordVM updateRecordVM { get; set; }

        public UpdateRecorVMSession updateRecorVMSession { get; set; }
    }

    public class UpdateRecordVM
    {
        [Required(ErrorMessage = "Please enter number of record to be process.")]
        [RegularExpression(Patterns.OnlyNumbers, ErrorMessage = "Please Enter only number.")]
        public int NumberOfRecord { get; set; }
    }

    public class ErrorDetailVM
    {
        public int USDOTNumber { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class GoogleGeoCodeResponse
    {
        public string status { get; set; }
        public results[] results { get; set; }
    }

    public class results
    {
        public string formatted_address { get; set; }
        public geometry geometry { get; set; }
        public string[] types { get; set; }
        public address_component[] address_components { get; set; }
    }

    public class geometry
    {
        public string location_type { get; set; }
        public location location { get; set; }
        public bounds bounds { get; set; }
        public viewport viewport { get; set; }
    }

    public class location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class bounds
    {
        public northeast northeast { get; set; }
        public southwest southwest { get; set; }
    }

    public class viewport
    {
        public northeast northeast { get; set; }
        public southwest southwest { get; set; }
    }

    public class northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class address_component
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public string[] types { get; set; }
    }

    public class UpdateRecorVMSession
    {
        public int TotalRecord { get; set; }

        public int UpdatedRecord { get; set; }

        public List<ErrorDetailVM> ErrorDetail { get; set; }

        public int FailedWithError { get; set; }
    }

    public class UpdateRecordProgressInfo
    {
        public bool IsInProgress { get; set; } // indicates if process is running or not
        public int RecordsToProcess { get; set; } // total records to process        
        public int RecordsSuccessful { get; set; } // records processsed so far successfully                
        public List<ErrorDetailVM> Errors { get; set; } // records failed so far
        public int RecordsCouldNotFetch { get; set; } // records couldn't be fetched from db
        public int RecordFailedDuringLatlng { get; set; } //record failed when getting lat lng
        //check if user enter records are enouugh to update or not
        //example: user entered 500 records but only 400 records are available to update from PreTransportCompanyTable
        //in this case CheckIsRecordsEnoughToUpdate = false
        // when value of this field is false, then in UI we show a message notifying the same to user
        public bool CheckIsRecordsEnoughToUpdate { get; set; }
        public int TotalCountOfAvailableRecordToUpdate { get; set; } //get count how many records are remaining to update
        public int TotalRecordCount { get; set; }
        //if any error occurs during updating record then store into this list variable
        //now when second time query execute then skip those records
        public List<int?> RecordsFailDuringUpdate { get; set; }
    }

    public class ProcessRecordsVM
    {
        [Required(ErrorMessage = "Please enter number of record to be process.")]
        [RegularExpression(Patterns.OnlyNumbers, ErrorMessage = "Please Enter only number.")]
        public int RecordsToProcess { get; set; }

        public bool IsDataMigrationLogEnabled { get; set; }
    }

    public class BusinessOrWaitingForApprovalVM
    {
        public string WebsiteName { get; set; }

        public bool? CommunicationApproved { get; set; }

        public bool? EmailVerified { get; set; }

        public int USDOTNumber { get; set; }

        public string CompanyEmailAddress { get; set; }

        public bool? WebsiteApproved { get; set; }
    }


    public class CommonBusinessAndPagelistVM
    {
        public PagedList<BusinessListVM> PagedListVM { get; set; }

        public int BusinessID { get; set; }

        [Display(Name = "Website")]
        [RegularExpression(Patterns.website, ErrorMessage = "Please enter valid Website")]
        [MaxLength(255, ErrorMessage = "Website cannot be longer than 255 characters.")]
        public string WebsiteName { get; set; }


        [Display(Name = "Contact Email")]
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email address")]
        [MaxLength(255, ErrorMessage = "{0} cannot be longer than 255 characters.")]
        public string BusinessContactEmail { get; set; }

        public bool? CommunicationApproved { get; set; }

        public bool? EmailVerified { get; set; }

        public int USDOTNumber { get; set; }

        public string CompanyEmailAddress { get; set; }

        public bool? WebsiteApproved { get; set; }
    }

    public class CleanCompanyEveryMonthVM
    {
        public int USDOTNumber { get; set; }
        public string CityName { get; set; }
        public string StateCode { get; set; }
    }

    public class CommonCleanCompanyEveryMonthAndPagelistVM
    {
        public bool IsAllCheck { get; set; }
        public string CommaSeparatedCheckUSDOTNumber { get; set; }
        public PagedList<CleanCompanyEveryMonthVM> PagedListVM { get; set; }
    }
}
