
namespace Common.Utility.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// 
    /// </summary>
    public static partial class ControllerExt
    {
        /// <summary>
        /// Renders partial view as string
        /// </summary>
        /// <param name="controller">current controller instance</param>
        /// <param name="partialPath">viewname or path</param>
        /// <param name="model">model to be passed to view</param>
        /// <returns>returns rendered partial view as string</returns>
        public static string RenderPartialViewToString(this ControllerBase controller, string partialPath, object model)
        {
            if (string.IsNullOrEmpty(partialPath))
                partialPath = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialPath);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                // copy model state items to the html helper 
                foreach (var item in viewContext.Controller.ViewData.ModelState)
                    if (!viewContext.ViewData.ModelState.Keys.Contains(item.Key))
                    {
                        viewContext.ViewData.ModelState.Add(item);

                    }
                try
                {
                    viewResult.View.Render(viewContext, sw);
                }
                catch (Exception)
                {

                    throw;
                }



                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Returns array of toasts based on model state errors
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="showAutomatically">true means, error will be shown in toastr by global ajax handler, false means programmer will handle it on its own</param>
        /// <returns></returns>        
        public static Toast[] GetModelStateErrors(this Controller controller, bool showAutomatically)
        {
            List<Toast> lstErrors = new List<Toast>();

            controller.ModelState.Where(m => m.Value.Errors.Count > 0)
                .ToList().ForEach(
                k =>
                {
                    k.Value.Errors.ToList().ForEach(
                    e =>
                    {
                        lstErrors.Add(new Toast(e.ErrorMessage, ToastType.Error) { Show = showAutomatically, MessageFor = k.Key });
                    }
                    );
                }
                );

            return lstErrors.ToArray();
        }
    }
}