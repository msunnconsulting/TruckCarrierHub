namespace Common.Utility.ViewModels
{
    using Extensions;
    using System;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// A JsonResult which allows us to send rendered view as html wrapped in RenderedViewResult object
    /// </summary>
    public class JsonViewResult : JsonResult
    {
        /// <summary>
        /// Creates an insrance of JsonViewResult
        /// </summary>
        /// <param name="renderedView">html of the view to be sent</param>
        /// <param name="updateTarget">selector of the target for redneredView</param>
        /// <param name="mode">indicates if renderedView should be inserted before, after, replaced or just content should be replaced</param>
        /// <param name="success"></param>
        /// <param name="continueOnFailure"></param>
        /// <param name="messages"></param>
        public JsonViewResult(string renderedView, bool success = true, bool continueOnFailure = false, params Toast[] messages)
        {
            if (renderedView == null)
                throw new ArgumentNullException("renderedView");

            this.Data = new APIResult<string>
            {
                Response = renderedView,
                IsSuccess = success,
                AutoShowMessages = messages.ToList().Where(m => m.Show == true).ToList(),
                ManualShowMessages = messages.ToList().Where(m => m.Show == false).ToList()
            };
            this.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller">current controller instance</param>
        /// <param name="viewName">viewname</param>
        /// <param name="model">model to render view</param>
        /// <param name="updateTarget">selector of the target for redneredView</param>
        /// <param name="mode">indicates if renderedView should be inserted before, after, replaced or just content should be replaced</param>
        /// <param name="success"></param>
        /// <param name="continueOnFailure"></param>
        /// <param name="messages"></param>
        public JsonViewResult(Controller controller, string viewName, object model, bool success = true, bool continueOnFailure = false, params Toast[] messages)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            if (viewName == null)
                throw new ArgumentNullException("viewpath");

            this.Data = new APIResult<string>
            {
                Response = controller.RenderPartialViewToString(viewName, model),
                IsSuccess = success,
                AutoShowMessages = messages.ToList().Where(m => m.Show == true).ToList(),
                ManualShowMessages = messages.ToList().Where(m => m.Show == false).ToList(),
            };
            this.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

        }
    }
}