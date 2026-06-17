

namespace Common.Utility.Extensions
{
    using EnumUtil;
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using ViewModels;

    /// <summary>
    /// 
    /// </summary>
    public static partial class SortLinkExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="onSorting">js function to be called when sort link is clicked</param>
        /// <param name="linkText">This is the text which will be displayed before Up-Down sort arrow as a header</param>
        /// <param name="columnExpression">This is unique identification text for the current column </param>
        /// <param name="action">action parameter used to generate url for the sort link</param>
        /// <param name="controller">controller parameter used to generate url for the sort link</param>
        /// <param name="routeValues">routeValues used to generate url for the sort link</param>
        /// <param name="sorting">sorting information which is used to find out current sorting details</param>
        /// <param name="pagination">optional pagination parameter, if supplied autoamtically add pagination parameter 'p' to the user for the sort link</param>        
        /// <returns>Rendered anchor tag along with Up-Down arrow next to current sort link</returns>
        public static MvcHtmlString SortLink(this HtmlHelper helper, string onSorting, string linkText, string columnExpression, string action, string controller, object routeValues, Sorting sorting, Pagination pagination = null)
        {
            return GenerateSortLink(helper, onSorting, linkText, columnExpression, action, controller, routeValues, sorting, pagination);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="linkText">This is the text which will be displayed before Up-Down sort arrow as a header</param>
        /// <param name="columnExpression">This is unique identification text for the current column </param>
        /// <param name="action">action parameter used to generate url for the sort link</param>
        /// <param name="controller">controller parameter used to generate url for the sort link</param>
        /// <param name="routeValues">routeValues used to generate url for the sort link</param>
        /// <param name="sorting">sorting information which is used to find out current sorting details</param>
        /// <param name="pagination">optional pagination parameter, if supplied autoamtically add pagination parameter 'p' to the user for the sort link</param>        
        /// <returns>Rendered anchor tag along with Up-Down arrow next to current sort link</returns>
        public static MvcHtmlString SortLink(this HtmlHelper helper, string linkText, string columnExpression, string action, string controller, object routeValues, Sorting sorting, Pagination pagination = null)
        {
            return GenerateSortLink(helper, null, linkText, columnExpression, action, controller, routeValues, sorting, pagination);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="onSorting">js function to be called when sort link is clicked</param>
        /// <param name="linkText">This is the text which will be displayed before Up-Down sort arrow as a header</param>
        /// <param name="columnExpression">This is unique identification text for the current column </param>
        /// <param name="action">action parameter used to generate url for the sort link</param>
        /// <param name="controller">controller parameter used to generate url for the sort link</param>
        /// <param name="routeValues">routeValues used to generate url for the sort link</param>
        /// <param name="sorting">sorting information which is used to find out current sorting details</param>
        /// <param name="pagination">optional pagination parameter, if supplied autoamtically add pagination parameter 'p' to the user for the sort link</param>        
        /// <returns>Rendered anchor tag along with Up-Down arrow next to current sort link</returns>
        private static MvcHtmlString GenerateSortLink(HtmlHelper helper, string onSorting, string linkText, string columnExpression, string action, string controller, object routeValues, Sorting sorting, Pagination pagination = null)
        {
            if (onSorting != null)
                onSorting = onSorting.Trim();

            if (linkText == null)
                throw new ArgumentNullException("linkText");

            if (columnExpression == null)
                throw new ArgumentNullException("columnExpression");

            if (action == null)
                throw new ArgumentNullException("action");

            if (controller == null)
                throw new ArgumentNullException("controller");

            if (sorting == null)
                throw new ArgumentNullException("sorting");

            TagBuilder a = new TagBuilder("a");

            SortingDirection newSortingDirection = GetNewSortDirection(columnExpression, sorting);

            IDictionary<string, object> routeValueDic = routeValues.ToDictionary();
            if (pagination != null)
                routeValueDic.MergeKey("p", pagination.PageIndex);

            routeValueDic.MergeKey("se", columnExpression);
            routeValueDic.MergeKey("sd", newSortingDirection.ToName());

            // se, sd, p, anything else
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));

            if (sorting.SortExpression == columnExpression)
                linkText += " <i class='" + (newSortingDirection == SortingDirection.Asc ? "glyphicon glyphicon-sort-by-attributes-alt" : "glyphicon glyphicon-sort-by-attributes") + "'></i>";

            if (!string.IsNullOrEmpty(onSorting))
                a.Attributes.Add("onclick", onSorting + "('" + url + "')");
            else
                a.Attributes.Add("href", url);

            a.InnerHtml = linkText;

            return MvcHtmlString.Create(a.ToString());
        }

        /// <summary>
        /// Returns sort direction for currently rendering column. So when this column is clicked, sort direction will be the one returned from this method
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="sorting"></param>
        /// <returns></returns>
        private static SortingDirection GetNewSortDirection(string columnName, Sorting sorting)
        {
            if (columnName == sorting.SortExpression)
            {
                if (sorting.SortDirection == SortingDirection.Asc)
                    return SortingDirection.Desc;
                else
                    return SortingDirection.Asc;
            }
            else
                return SortingDirection.Asc;
        }
    }
}
