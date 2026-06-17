using Common.Utility;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PartnerCarrier.ViewModels.Admin
{
    public class AccountVM
    {

    }

    public class common
    {
        public class LoggedInUserVM : Common.Utility.FormAuthentication.LoggedInUser<UserRole, int>
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public long UserId { get; set; }

            public string Email { get; set; }

            public string Password { get; set; }

            public string PasswordHash { get; set; }

            public string PasswordSalt { get; set; }

            public bool IsActive { get; set; }

            public string LoginName { get; set; }

            public UserRole Role { get; set; }

            public byte RoleId { get; set; }
            public int? USDOTNumber { get; set; }
        }
        public enum SignInStatus
        {
            //Sign in was successful
            Success = 0,
            //Sign in failed
            Failure = 3,
            //If User is inactive
            Inactive = 4,
            //If user enter wrong password more than 3 times
            TooManyAttempt = 5
        }
    }
    public enum UserRole
    {
        Admin = 1,
        User = 2,
        SetupAdmin = 3,
        BusinessUser = 4

    }

    public class LoginInfoVM
    {
        [Required(ErrorMessage = "Please enter password")]
        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayName("Remember me?")]
        public bool RememberMe { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? AdminID { get; set; }

        [Required]
        [DisplayName("Email")]
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter email")]
        public string Email { get; set; }

        public string LoginName { get; set; }

        public UserRole Role { get; set; }
    }

    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        [DisplayName("Email")]
        public string Email { get; set; }
        public string ForgotPasswordKey { get; set; }
    }

    public class ResetPasswordVM
    {
        [Required]
        [DisplayName("New Password")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm Password must be match with New Password")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class EditProfileVM
    {
        public long Id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("Email")]
        public string Email { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Current Password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        [RegularExpression(Patterns.PasswordValidation, ErrorMessage = "* Password must be at least 8 characters long.<br/>* Password must have at least special character or digit.<br/>* Password must have at least one uppercase ('A'-'Z').<br/>* Password must have at least one lowercase ('a'-'z')")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The New Password and confirm Password does not match!!")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        public string ConfirmPassword { get; set; }
    }
}
