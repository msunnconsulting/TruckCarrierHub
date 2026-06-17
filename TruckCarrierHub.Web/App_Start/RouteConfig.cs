namespace PartnerCarrier.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
           name: "Search",
           url: "search",
           defaults: new { controller = "Home", action = "Search" },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
               name: "SubmitGetAQuote",
               url: "SubmitGetAQuotePage",
               defaults: new { controller = "Home", action = "SubmitGetAQuotePage", getAQuoteVM = UrlParameter.Optional },
             namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
                name: "SendEmailsForQuote",
                url: "SendEmailsForQuote/{quoteId}",
                defaults: new { controller = "Home", action = "SendEmailsForQuote", quoteId = UrlParameter.Optional },
              namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
                name: "OnClickEditReview",
                url: "on-edit-review",
                defaults: new { controller = "Home", action = "EditCompanyReview", reviewId = UrlParameter.Optional },
                namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
            );

            routes.MapRoute(
                name: "OnClickEditResponse",
                url: "on-edit-review-response",
                defaults: new { controller = "Home", action = "EditReviewResponse", responseId = UrlParameter.Optional },
                namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
            );

            routes.MapRoute(
                name: "OnClickReplyReview",
                url: "on-add-review-reply",
                defaults: new { controller = "Home", action = "OnClickAddReviewReply" },
                namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
            );

            routes.MapRoute(
               name: "OnAddEditReviewResponse",
               url: "add-edit-review-response",
               defaults: new { controller = "Home", action = "AddEditReviewResponse" },
               namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
           );

            routes.MapRoute(
                name: "OnClickAddReview",
                url: "on-add-review",
                defaults: new { controller = "Home", action = "OnClickAddReview", companyId = UrlParameter.Optional },
                namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
            );

            routes.MapRoute(
                name: "OnsubmitReview",
                url: "on-submit-review",
                defaults: new { controller = "Home", action = "AddCompanyReview", companyId = UrlParameter.Optional },
                namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
            );

            routes.MapRoute(
                name: "GetReviews",
                url: "get-reviews",
                defaults: new { controller = "Home", action = "GetReviewsList", companyId = UrlParameter.Optional },
                namespaces: new string[] { "PartnerCarrier.Web.Controllers" }
            );

            routes.MapRoute(
             name: "GetAQuote",
             url: "getaquote/{loadType}",
             defaults: new { controller = "Home", action = "GetAQuotePage", loadType = UrlParameter.Optional },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "GetCheckboxListFromLocationType",
             url: "GetCheckboxListFromLocationType/{pickupLocationTypeId}",
             defaults: new { controller = "Home", action = "GetCheckboxListFromLocationType", pickupLocationTypeId = UrlParameter.Optional },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "PrivacyNotice",
             url: "privacy",
             defaults: new { controller = "Home", action = "DisplayPrivacyNotice" },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
            name: "SuccessStory",
            url: "success-story",
            defaults: new { controller = "Home", action = "DisplaySuccessStory" },
          namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "TermsOfUse",
             url: "terms",
             defaults: new { controller = "Home", action = "DisplayTermsOfUse" },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "ContactUS",
              url: "contact_us",
              defaults: new { controller = "ContactUs", action = "ContactUs" },
            namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "SearchAutoComplete",
              url: "searchautocomplete",
              defaults: new { controller = "Home", action = "SearchAutoComplete" },
            namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "SearchAutoCompleteCity",
              url: "searchautocompletecity",
              defaults: new { controller = "Home", action = "SearchAutoCompleteCity" },
            namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "StoreIsCheckCheckBoxValue",
             url: "storeischeckcheckboxvalue",
             defaults: new { controller = "Home", action = "StoreIsCheckCheckBoxValue" },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
            name: "StoreReviewsFilterCheckBoxValue",
            url: "storereviewsfiltercheckboxvalue",
            defaults: new { controller = "Home", action = "StoreReviewsFilterCheckBoxValue" },
            namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
            name: "Error",
            url: "error",
            defaults: new { controller = "Home", action = "Error" },
          namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
               name: "EmailUnsubscribe",
               url: "email/unsubscribe/{USDOTNumber}",
               defaults: new { controller = "Home", action = "Unsubscribe", USDOTNumber = UrlParameter.Optional },
               namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "UnSubscribed-ThankYouPage",
              url: "unsubscribed-sucessfully",
              defaults: new { controller = "Home", action = "UnSubscribeThankYouPage" },
              namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "QuoteSubmited-ConfirmationPage",
              url: "quote-submited-sucessfully",
              defaults: new { controller = "Home", action = "QuoteSubmitConfirmationPage" },
              namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "Business",
              url: "business/{USDOTNumber}",
              defaults: new { controller = "Home", action = "Business", USDOTNumber = UrlParameter.Optional },
              namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "CreateAccount",
             url: "create-account/{USDOTNumber}",
             defaults: new { controller = "Home", action = "CreateAccount", USDOTNumber = UrlParameter.Optional },
             namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "ChangePassword",
             url: "change-password/{USDOTNumber}",
             defaults: new { controller = "Home", action = "ChangePassword", USDOTNumber = UrlParameter.Optional },
             namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
               name: "State",
               url: "{stateCode}",
               defaults: new { controller = "Home", action = "State", stateCode = UrlParameter.Optional },
             namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "EmailVerify",
              url: "verify/{verifyLinkGUID}",
              defaults: new { controller = "Home", action = "EmailVerify", verifyLinkGUID = UrlParameter.Optional },
            namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "CompanyNewFormat",
              url: "{statecode}/USDOT-{usdotnumber}",
              defaults: new { controller = "Home", action = "Company", stateCode = UrlParameter.Optional, city = UrlParameter.Optional, companylegalname = UrlParameter.Optional, usdotnumber = UrlParameter.Optional },
              namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
              name: "City",
              url: "{stateCode}/{city}",
              defaults: new { controller = "Home", action = "City", stateCode = UrlParameter.Optional, city = UrlParameter.Optional },
            namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
             name: "Company",
             url: "{statecode}/{city}/{companyNameForUrl}-USDOT-{usdotnumber}",
             defaults: new { controller = "Home", action = "Company", stateCode = UrlParameter.Optional, city = UrlParameter.Optional, companylegalname = UrlParameter.Optional, usdotnumber = UrlParameter.Optional },
           namespaces: new string[] { "PartnerCarrier.Web.Controllers" });

            routes.MapRoute(
            name: "CityFilter",
            url: "{stateCode}/{city}/{filter1}/{filter2}/{filter3}/{filter4}/{filter5}/{filter6}/{filter7}/{filter8}/{filter9}",
            defaults: new
            {
                controller = "Home",
                action = "City",
                stateCode = UrlParameter.Optional,
                city = UrlParameter.Optional,
                filter1 = UrlParameter.Optional,
                filter2 = UrlParameter.Optional,
                filter3 = UrlParameter.Optional,
                filter4 = UrlParameter.Optional,
                filter5 = UrlParameter.Optional,
                filter6 = UrlParameter.Optional,
                filter7 = UrlParameter.Optional,
                filter8 = UrlParameter.Optional,
                filter9 = UrlParameter.Optional
            },
          namespaces: new string[] { "PartnerCarrier.Web.Controllers" });



        }
    }
}
