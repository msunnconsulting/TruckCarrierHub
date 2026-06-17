using Common.Utility.FormAuthentication;

namespace PartnerCarrier.Infrastructure
{
    using PartnerCarrier.ViewModels.Admin;
    using static PartnerCarrier.ViewModels.Admin.common;

    //using static ViewModels.common;

    public sealed class FormsAuthService
    {

        private static readonly FormsAuthenticationUtil<LoggedInUserVM, UserRole, int> instance = new FormsAuthenticationUtil<LoggedInUserVM, UserRole, int>();

        private FormsAuthService() { }

        public static FormsAuthenticationUtil<LoggedInUserVM, UserRole, int> Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
