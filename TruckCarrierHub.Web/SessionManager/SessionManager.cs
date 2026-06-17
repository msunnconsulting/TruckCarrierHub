namespace PartnerCarrier.Web.SessionManager
{
    using System.Web;

    public class SessionManager
    {
        public static readonly SessionManager _instance = new SessionManager();

        public static SessionManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool IsHiringCheckboxIsChecked
        {
            get
            {
                if (HttpContext.Current.Session["IsHiringCheckboxIsChecked"] != null)
                {
                    return (bool)HttpContext.Current.Session["IsHiringCheckboxIsChecked"];
                }
                return false;
            }
            set { HttpContext.Current.Session["IsHiringCheckboxIsChecked"] = value; }
        }

        public bool IsReviewsFilterCheckboxIsChecked
        {
            get
            {
                if (HttpContext.Current.Session["IsReviewsFilterCheckboxIsChecked"] != null)
                {
                    return (bool)HttpContext.Current.Session["IsReviewsFilterCheckboxIsChecked"];
                }
                return false;
            }
            set { HttpContext.Current.Session["IsReviewsFilterCheckboxIsChecked"] = value; }
        }
    }
}