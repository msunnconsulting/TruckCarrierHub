using PartnerCarrier.ViewModels.Admin;
using static PartnerCarrier.ViewModels.Admin.common;

namespace PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement
{
    public interface IAccountService
    {
        /// <summary>
        /// Setup Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        SignInStatus SetupLogin(LoginInfoVM loginInfoVM);

        SignInStatus Login(LoginInfoVM loginInfoVM);

        EditProfileVM EditAdminProfile();

        void UpdateProfile(EditProfileVM editProfileVM);

        void ChangePassword(ChangePasswordVM changePasswordVM);

        void ForgotPassword(ForgotPasswordVM forgotPasswordVM);

        void ResetPassword(string code, ResetPasswordVM resetPasswordVM);
    }
}
