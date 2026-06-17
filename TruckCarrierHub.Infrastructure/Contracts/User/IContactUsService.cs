namespace PartnerCarrier.Infrastructure.Contracts.User
{
    using ViewModels.User;
    public interface IContactUsService
    {
        /// <summary>
        /// Send Contact Us details by email to admin
        /// </summary>
        /// <param name="contactUSVM"></param>
        void SendContactUsDetailsToAdminEmail(ContactUsVM contactUSVM);
    }
}
