namespace PartnerCarrier.Web.Areas.Admin.Controllers
{
    using Filters;
    using Common.Utility;
    using PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement;
    using System;
    using System.Web.Mvc;
    using ViewModels.Admin;

    [RouteArea("admin", AreaPrefix = "admin")]
    [RoutePrefix("statistics")]
    [AuthorizeRole(UserRole.Admin)]
    public class StatisticsController : BaseController
    {
        private readonly IBusinessMangementService _businessMangementService;

        public StatisticsController(IBusinessMangementService businessMangementService)
        {
            _businessMangementService = businessMangementService;
        }

        [HttpGet]
        [Route("settings")]
        public ActionResult Settings()
        {
            var vm = _businessMangementService.GetShowStatisticsNav();
            return View(vm);
        }

        [Route("save-settings")]
        public ActionResult SaveSettings(ShowStatisticsNavVM vm)
        {
            try
            {
                _businessMangementService.SaveShowStatisticsNav(vm);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ReturnExceptionResult(ex);
            }
        }

        [HttpGet]
        [Route("manage")]
        public ActionResult ManageStatistics()
        {
            return View();
        }
    }
}
