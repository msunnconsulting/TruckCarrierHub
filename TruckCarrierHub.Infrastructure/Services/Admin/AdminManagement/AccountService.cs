namespace PartnerCarrier.Infrastructure.Services.Admin.AdminManagement
{
    using Common.Utility;
    using Common.Utility.Logger;
    using PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement;
    using PartnerCarrier.Infrastructure.Database;
    using PartnerCarrier.ViewModels.Admin;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using static PartnerCarrier.ViewModels.Admin.common;

    public class AccountService : IAccountService
    {

        #region  private variables

        private readonly PartnerCarrier_DevEntities db;
        #endregion

        #region constructor
        public AccountService(PartnerCarrier_DevEntities _db)
        {
            db = _db;
        }
        #endregion

        /// <summary>
        ///Admin User Login 
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        public SignInStatus Login(LoginInfoVM loginInfoVM)
        {
            var adminUserInfo = (from user in db.AdminUsers
                                 where user.Email == loginInfoVM.Email
                                 select new LoggedInUserVM
                                 {
                                     Name = user.Name,
                                     EmailAddress = user.Email,
                                     UserId = user.Id,
                                     RoleId = user.RoleId,
                                     IsActive = user.IsActive,
                                     PasswordHash = user.PasswordHash,
                                     PasswordSalt = user.PasswordSalt,
                                 }).FirstOrDefault();
            //if admin Email And Password does not Exist 
            if (adminUserInfo == null || (adminUserInfo != null && adminUserInfo.PasswordHash != PasswordGenerator.GetHashedPassword(adminUserInfo.PasswordSalt, loginInfoVM.Password)))
            {
                return SignInStatus.Failure;
            }
            // verify if user is active
            if (!adminUserInfo.IsActive)
            {
                return SignInStatus.Inactive;
            }
            //Get Role Name from Role id from user role enum.
            adminUserInfo.Role = (UserRole)adminUserInfo.RoleId;

            //return the user information on success
            FormsAuthService.Instance.LogIn(adminUserInfo, Config.GetValue("AdminLoginAuthenticationName"), redirectAfterLogin: false);
            ////return log;
            return SignInStatus.Success;
        }

        /// <summary>
        /// setup login info
        /// </summary>
        /// <param name="loginInfoVM"></param>
        /// <returns></returns>
        public SignInStatus SetupLogin(LoginInfoVM loginInfoVM)
        {
            LoggedInUserVM userInfo = new LoggedInUserVM();

            //get username from web config
            userInfo.EmailAddress = Config.GetValue("username");

            //get saltpassword from web config
            userInfo.PasswordSalt = Config.GetValue("saltpassword");

            //get hashpassword from web config
            userInfo.PasswordHash = Config.GetValue("hashpassword");

            //if user Email And Password from web.config file does not Exist or not valid then it will return signin status failure 
            if (userInfo.PasswordSalt == null || (userInfo.EmailAddress != loginInfoVM.Email) || (userInfo.PasswordHash != null && userInfo.PasswordHash != PasswordGenerator.GetHashedPassword(userInfo.PasswordSalt, loginInfoVM.Password)))
            {
                return SignInStatus.Failure;
            }

            //Get Role Name from Role id from user role enum.
            userInfo.RoleId = (int)UserRole.SetupAdmin;
            userInfo.Role = UserRole.SetupAdmin;

            //return the user information on success and stored in cookie
            FormsAuthService.Instance.LogIn(userInfo, Config.GetValue("SetupLoginAuthenticationName"), redirectAfterLogin: false);

            ////return log;
            return SignInStatus.Success;
        }

        /// <summary>
        /// For User Edit Profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EditProfileVM EditAdminProfile()
        {
            var adminUserid = FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName")).UserId;

            var adminUserInfo = (from user in db.AdminUsers.AsNoTracking()
                                 where user.Id == adminUserid
                                 select new EditProfileVM
                                 {
                                     Id = user.Id,
                                     Name = user.Name,
                                     Email = user.Email,
                                 }).FirstOrDefault();
            return adminUserInfo;
        }

        /// <summary>
        /// Update Admin User Profile Information
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProfile(EditProfileVM editProfileVM)
        {
            AdminUser adminUser = new AdminUser();
            adminUser.Id = editProfileVM.Id;
            adminUser.Name = editProfileVM.Name;
            adminUser.Email = editProfileVM.Email;
            db.AdminUsers.UpdatePartial(db, adminUser, true, "Name", "Email");
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <param name="model"></param>
        public void ChangePassword(ChangePasswordVM changePasswordVM)
        {
            var loggedInUser = FormsAuthService.Instance.LoggedInUser(Config.GetValue("AdminLoginAuthenticationName"));
            var adminUserDetail = (from user in db.AdminUsers
                                   where user.Id == loggedInUser.UserId
                                   select user).SingleOrDefault();

            if (adminUserDetail.PasswordHash != PasswordGenerator.GetHashedPassword(adminUserDetail.PasswordSalt, changePasswordVM.OldPassword))
            {
                throw new BusinessException("400", "Invalid current password");
            }
            adminUserDetail.Id = loggedInUser.UserId;
            adminUserDetail.PasswordHash = PasswordGenerator.GetHashedPassword(adminUserDetail.PasswordSalt, changePasswordVM.ConfirmPassword);
            db.AdminUsers.UpdatePartial(db, adminUserDetail, true, "PasswordHash");

        }

        /// <summary>
        /// Account Forgot Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            //Create Token for Forgot Password
            forgotPasswordVM.ForgotPasswordKey = Guid.NewGuid().ToString();

            var adminUser = (from userDetail in db.AdminUsers
                             where userDetail.Email == forgotPasswordVM.Email
                             select userDetail).FirstOrDefault();

            if (adminUser == null) throw new BusinessException("400", "Supplied email doesn't exist.");

            try
            {
                Dictionary<string, string> replacevalues = new Dictionary<string, string>();
                replacevalues.Add("{resetPasswordLink}", Config.SiteURL + "admin/account/reset-password/" + forgotPasswordVM.ForgotPasswordKey.ToString());
                replacevalues.Add("{UserName}", adminUser.Email);

                //set forgot password Key
                adminUser.ForgotPasswordKey = forgotPasswordVM.ForgotPasswordKey;

                db.AdminUsers.UpdatePartial(db, adminUser, true, "ForgotPasswordKey");

                EmailUtility.Send(forgotPasswordVM.Email, "Reset Password", AppSettings.FromEmail, EmailUtility.GetTemplate(TemplateType.ForgotPassword), replacevalues);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Log(ex);
                throw new BusinessException("400", "Sending email failed. Please try again later or contact system administrator.");
            }

        }

        /// <summary>
        /// Account Reset Password
        /// </summary>
        /// <param name="code"></param>
        /// <param name="resetPwdVM"></param>
        /// <returns></returns>
        public void ResetPassword(string code, ResetPasswordVM resetPasswordVM)
        {
            //get admin user Detail By ForgotPasswordKey
            var adminUserInfo = (from userDetail in db.AdminUsers
                                 where userDetail.ForgotPasswordKey == code
                                 select userDetail).FirstOrDefault();


            if (adminUserInfo != null)
            {
                if (adminUserInfo.PasswordHash == null || adminUserInfo.PasswordSalt == null)
                {
                    adminUserInfo.PasswordSalt = PasswordGenerator.GetSalt();
                    adminUserInfo.PasswordHash = PasswordGenerator.GetHashedPassword(adminUserInfo.PasswordSalt, resetPasswordVM.ConfirmPassword);
                }
                else
                {
                    //Reset Password Here
                    adminUserInfo.PasswordHash = PasswordGenerator.GetHashedPassword(adminUserInfo.PasswordSalt, resetPasswordVM.ConfirmPassword);
                }


                adminUserInfo.ForgotPasswordKey = null;
                db.AdminUsers.UpdatePartial(db, adminUserInfo, true, "PasswordHash", "ForgotPasswordKey");
            }
            else
                throw new BusinessException("400", "Reset password link is expired.");
        }
    }
}
