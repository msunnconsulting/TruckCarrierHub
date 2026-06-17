
/*
<!-- Usage in razor (note @model): -->
@using BootstrapSupport
@model IPagedList

@Html.Pager(Model.PageIndex,
            Model.TotalPages,
            x => Url.Action("Index", new {page = x}),
            " pagination-right")

// Index action on the HomeController from the sample project:
public ActionResult Index(int page = 1)
{
    var pageSize = 3;
    var homeInputModels = _models;
    return View(homeInputModels.ToPagedList(page, pageSize));
}
*/

namespace Common.Utility.Extensions
{
    using EnumUtil;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using ViewModels;

    /// <summary>
    /// 
    /// </summary>
    public static partial class PagerExt
    {

        private static TagBuilder WrapInListItem(string text)
        {
            var li = new TagBuilder("li");
            li.SetInnerText(text);
            return li;
        }

        private static TagBuilder WrapInListItem(TagBuilder inner, PagedListRenderOptions options, params string[] classes)
        {
            var li = new TagBuilder("li");
            foreach (var @class in classes)
                li.AddCssClass(@class);
            if (options.FunctionToTransformEachPageLink != null)
                return options.FunctionToTransformEachPageLink(li, inner);
            li.InnerHtml = inner.ToString();
            return li;
        }

        private static TagBuilder First(PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {
            const int targetPageNumber = 1;
            var first = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.LinkToFirstPageFormat, targetPageNumber)
            };

            if (pagination.PageIndex == 1)
                return WrapInListItem(first, options);


            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (linkPage == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));

                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    first.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    first.Attributes.Add("href", url);
            }



            return WrapInListItem(first, options);
        }

        private static TagBuilder Previous(PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {
            var targetPageNumber = pagination.PageIndex - 1;
            var previous = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.LinkToPreviousPageFormat, targetPageNumber)
            };
            previous.Attributes["rel"] = "prev";

            if (!pagination.HasPreviousPage)
                return WrapInListItem(previous, options, "disabled");

            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (targetPageNumber == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    previous.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    previous.Attributes.Add("href", url);
            }

            return WrapInListItem(previous, options);
        }

        private static TagBuilder Page(int i, PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {
            var format = options.FunctionToDisplayEachPageNumber
                ?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));
            var targetPageNumber = i;
            var page = new TagBuilder("a");
            page.SetInnerText(format(targetPageNumber));

            if (i == pagination.PageIndex)
                return WrapInListItem(page, options, "active");

            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {

                string url = string.Empty;
                if (targetPageNumber == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }



                if (!string.IsNullOrEmpty(onPager))
                    page.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    page.Attributes.Add("href", url);
            }


            return WrapInListItem(page, options);
        }

        private static TagBuilder Next(PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {
            var targetPageNumber = pagination.PageIndex + 1;
            var next = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.LinkToNextPageFormat, targetPageNumber)
            };
            next.Attributes["rel"] = "next";

            if (!pagination.HasNextPage)
                return WrapInListItem(next, options, "disabled");

            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (targetPageNumber == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    next.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    next.Attributes.Add("href", url);
            }

            return WrapInListItem(next, options);
        }

        private static TagBuilder Last(PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {
            var targetPageNumber = pagination.TotalCount;
            var last = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.LinkToLastPageFormat, targetPageNumber)
            };

            if (pagination.PageIndex == pagination.TotalPages)
                return WrapInListItem(last, options, "disabled");

            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (targetPageNumber == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    last.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    last.Attributes.Add("href", url);
            }

            return WrapInListItem(last, options);
        }

        private static TagBuilder PageCountAndLocationText(PagedListRenderOptions options, Pagination pagination)
        {
            var text = new TagBuilder("a");
            text.SetInnerText(string.Format(options.PageCountAndCurrentLocationFormat, pagination.PageIndex, pagination.TotalPages));

            return WrapInListItem(text, options, "disabled");
        }

        private static TagBuilder ItemSliceAndTotalText(PagedListRenderOptions options, Pagination pagination)
        {
            var text = new TagBuilder("a");


            var FirstItemOnPage = ((pagination.PageIndex - 1) * pagination.PageSize + 1);
            var TotalItemCount = pagination.TotalCount;
            var numberOfLastItemOnPage = (FirstItemOnPage + pagination.PageSize - 1);
            var LastItemOnPage = numberOfLastItemOnPage > TotalItemCount
                                    ? TotalItemCount
                                    : numberOfLastItemOnPage;


            text.SetInnerText(string.Format(options.ItemSliceAndTotalFormat, FirstItemOnPage, LastItemOnPage, TotalItemCount));

            return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
        }

        private static TagBuilder Ellipses(PagedListRenderOptions options, Pagination pagination)
        {
            var a = new TagBuilder("a")
            {
                InnerHtml = options.EllipsesFormat
            };

            return WrapInListItem(a, options, "disabled");
        }


        private static TagBuilder PreviousEllipsis(PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {


            const int targetPageNumber = 1;
            var first = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.EllipsesFormat, targetPageNumber)
            };

            //if (pagination.PageIndex == 1)
            //    return WrapInListItem(first, options);


            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (linkPage == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    first.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    first.Attributes.Add("href", url);
            }



            return WrapInListItem(first, options);

            //var targetPageNumber = firstPageToDisplay - 1;//list.PageNumber - 1;
            //var previous = new TagBuilder("a")
            //{
            //    InnerHtml = string.Format(options.EllipsesFormat, targetPageNumber)
            //};
            //previous.Attributes["rel"] = "prev";

            //if (!list.HasPreviousPage)
            //    return WrapInListItem(previous, options, "PagedList-skipToPrevious", "disabled");

            //previous.Attributes["href"] = generatePageUrl(targetPageNumber);
            //return WrapInListItem(previous, options, "PagedList-skipToPrevious");
        }
        private static TagBuilder NextEllipsis(PagedListRenderOptions options, UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {


            const int targetPageNumber = 1;
            var first = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.EllipsesFormat, targetPageNumber)
            };

            //if (pagination.PageIndex == 1)
            //    return WrapInListItem(first, options);


            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (linkPage == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    first.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    first.Attributes.Add("href", url);
            }



            return WrapInListItem(first, options);

            //var targetPageNumber = lastPageToDisplay + 1;// list.PageNumber  +1;
            //var next = new TagBuilder("a")
            //{
            //    InnerHtml = string.Format(options.EllipsesFormat, targetPageNumber)
            //};
            //next.Attributes["rel"] = "next";

            //if (!list.HasNextPage)
            //    return WrapInListItem(next, options, "PagedList-skipToNext", "disabled");

            //next.Attributes["href"] = generatePageUrl(targetPageNumber);
            //return WrapInListItem(next, options, "PagedList-skipToNext");
        }


        /// <summary>
        /// Returns a Rendered bootstrap based standard pagination bar HTML, it makes page link to redirect to new URL
        /// </summary>
        /// <param name="helper">The html helper</param>
        /// <param name="action">action parameter used to generate url for the page link</param>
        /// <param name="controller">controller parameter used to generate url for the page link</param>
        /// <param name="routeValues">routeValues used to generate url for the page link</param>
        /// <param name="pagination">autoamtically add pagination parameter 'p' to the user for the sort link based on current pageindex</param>        
        /// <param name="sorting">optional sorting para, if supplied, it is used to find out current sorting details and add to url</param>
        /// <param name="additionalPagerCssClass">Additional classes for the navigation div (e.g. "pagination-right pagination-mini")</param>
        /// <returns>Returns a Rendered bootstrap based standard pagination bar HTML</returns>
        public static MvcHtmlString Pager(this HtmlHelper helper, string action, string controller, object routeValues, Pagination pagination, Sorting sorting = null, string additionalPagerCssClass = "", PagedListRenderOptions options = null)
        {
            return GeneratePagination(helper, action, controller, routeValues, pagination, sorting, additionalPagerCssClass, "", options);
        }

        /// <summary>
        /// Returns a Rendered bootstrap based standard pagination bar HTML, it makes page link to call a javascript function, which we can utilize to implement pagination with ajax
        /// </summary>
        /// <param name="helper">The html helper</param>
        /// <param name="onPager">name of the javascript function which will be called when someone click a page link, parameter of this js function will be the resultant url</param>
        /// <param name="action">action parameter used to generate url for the page link</param>
        /// <param name="controller">controller parameter used to generate url for the page link</param>
        /// <param name="routeValues">routeValues used to generate url for the page link</param>
        /// <param name="pagination">autoamtically add pagination parameter 'p' to the user for the sort link based on current pageindex</param>        
        /// <param name="sorting">optional sorting para, if supplied, it is used to find out current sorting details and add to url</param>
        /// <param name="additionalPagerCssClass">Additional classes for the navigation div (e.g. "pagination-right pagination-mini")</param>
        /// <returns>Returns a Rendered bootstrap based standard pagination bar HTML</returns>
        public static MvcHtmlString Pager(this HtmlHelper helper, string onPager, string action, string controller, object routeValues, Pagination pagination, Sorting sorting = null, string additionalPagerCssClass = "", PagedListRenderOptions options = null)
        {
            return GeneratePagination(helper, action, controller, routeValues, pagination, sorting, additionalPagerCssClass, onPager, options);
        }

        private static MvcHtmlString GeneratePagination(HtmlHelper helper, string action, string controller, object routeValues, Pagination pagination, Sorting sorting = null, string additionalPagerCssClass = "", string onPager = null, PagedListRenderOptions options = null)
        {
            if (onPager != null)
                onPager = onPager.Trim();

            if (action == null)
                throw new ArgumentNullException("action");

            if (controller == null)
                throw new ArgumentNullException("controller");

            if (pagination == null)
                throw new ArgumentNullException("pagination");

            if (pagination.TotalPages <= 1)
                return MvcHtmlString.Empty;

            if (options == null)
            {
                options = new PagedListRenderOptions();
            }

            IDictionary<string, object> routeValueDic = routeValues.ToDictionary();
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);


            if (options.Display == PagedListDisplayMode.Never || (options.Display == PagedListDisplayMode.IfNeeded && pagination.TotalPages <= 1))
                return null;

            var listItemLinks = new List<TagBuilder>();

            //calculate start and end of range of page numbers
            var firstPageToDisplay = 1;
            var lastPageToDisplay = pagination.TotalPages;

            var pageNumbersToDisplay = lastPageToDisplay;
            if (options.MaximumPageNumbersToDisplay.HasValue && pagination.TotalPages > options.MaximumPageNumbersToDisplay)
            {
                // cannot fit all pages into pager
                var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;
                firstPageToDisplay = pagination.PageIndex - maxPageNumbersToDisplay / 2;
                if (firstPageToDisplay < 1)
                    firstPageToDisplay = 1;
                pageNumbersToDisplay = maxPageNumbersToDisplay;
                lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;
                if (lastPageToDisplay > pagination.TotalPages)
                    firstPageToDisplay = pagination.TotalPages - maxPageNumbersToDisplay + 1;
            }

            //first
            if (options.DisplayLinkToFirstPage == PagedListDisplayMode.Always || (options.DisplayLinkToFirstPage == PagedListDisplayMode.IfNeeded && firstPageToDisplay > 1))
                listItemLinks.Add(First(options, urlHelper, "1", 1, action, controller, routeValueDic, pagination, sorting, onPager));

            //previous
            if (options.DisplayLinkToPreviousPage == PagedListDisplayMode.Always || (options.DisplayLinkToPreviousPage == PagedListDisplayMode.IfNeeded && pagination.PageIndex != 1))
                listItemLinks.Add(Previous(options, urlHelper, (pagination.PageIndex - 1).ToString(), pagination.PageIndex - 1, action, controller, routeValueDic, pagination, sorting, onPager));

            //text
            if (options.DisplayPageCountAndCurrentLocation)
                listItemLinks.Add(PageCountAndLocationText(options, pagination));

            //text
            if (options.DisplayItemSliceAndTotal)
                listItemLinks.Add(ItemSliceAndTotalText(options, pagination));



            //page
            if (options.DisplayLinkToIndividualPages)
            {
                //if there are previous page numbers not displayed, show an ellipsis
                if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
                    listItemLinks.Add(PreviousEllipsis(options, urlHelper, "...", (firstPageToDisplay - 1), action, controller, routeValueDic, pagination, sorting, onPager));
                //listItemLinks.Add(Ellipses(options,pagination));


                foreach (var i in Enumerable.Range(firstPageToDisplay, pageNumbersToDisplay))
                {
                    //show delimiter between page numbers
                    if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
                        listItemLinks.Add(WrapInListItem(options.DelimiterBetweenPageNumbers));

                    //show page number link
                    listItemLinks.Add(Page(i, options, urlHelper, i.ToString(), i, action, controller, routeValueDic, pagination, sorting, onPager));
                }

                //if there are subsequent page numbers not displayed, show an ellipsis
                if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && (firstPageToDisplay + pageNumbersToDisplay - 1) < pagination.TotalPages) //list.PageCount
                    listItemLinks.Add(NextEllipsis(options, urlHelper, "...", (firstPageToDisplay + pageNumbersToDisplay), action, controller, routeValueDic, pagination, sorting, onPager));
                //listItemLinks.Add(Ellipses(options, pagination));
            }

            //next
            if (options.DisplayLinkToNextPage == PagedListDisplayMode.Always || (options.DisplayLinkToNextPage == PagedListDisplayMode.IfNeeded && pagination.PageIndex != pagination.TotalPages))
                listItemLinks.Add(Next(options, urlHelper, (pagination.PageIndex + 1).ToString(), (pagination.PageIndex + 1), action, controller, routeValueDic, pagination, sorting, onPager));

            //last
            if (options.DisplayLinkToLastPage == PagedListDisplayMode.Always || (options.DisplayLinkToLastPage == PagedListDisplayMode.IfNeeded && lastPageToDisplay < pagination.TotalPages))
                listItemLinks.Add(Last(options, urlHelper, pagination.TotalPages.ToString(), pagination.TotalPages, action, controller, routeValueDic, pagination, sorting, onPager));

            if (listItemLinks.Any())
            {
                //append class to first item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToFirstListItemInPager))
                    listItemLinks.First().AddCssClass(options.ClassToApplyToFirstListItemInPager);

                //append class to last item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToLastListItemInPager))
                    listItemLinks.Last().AddCssClass(options.ClassToApplyToLastListItemInPager);

                //append classes to all list item links
                foreach (var li in listItemLinks)
                    foreach (var c in options.LiElementClasses ?? Enumerable.Empty<string>())
                        li.AddCssClass(c);
            }

            //collapse all of the list items into one big string
            var listItemLinksString = listItemLinks.Aggregate(
                new StringBuilder(),
                (sb, listItem) => sb.Append(listItem.ToString()),
                sb => sb.ToString()
                );


            var ul = new TagBuilder("ul")
            {
                InnerHtml = listItemLinksString
            };
            foreach (var c in options.UlElementClasses ?? Enumerable.Empty<string>())
                ul.AddCssClass(c);

            var outerDiv = new TagBuilder("div");
            foreach (var c in options.ContainerDivClasses ?? Enumerable.Empty<string>())
                outerDiv.AddCssClass(c);
            outerDiv.InnerHtml = ul.ToString() /*+ "<span class='margin-top-25-left-20'> Showing " + pagination.TotalCount + " Items</span>"*/;

            return new MvcHtmlString(outerDiv.ToString());

        }
        private static TagBuilder GetPageLink(UrlHelper urlHelper, string linkText, int linkPage, string action, string controller, IDictionary<string, object> routeValueDic, Pagination pagination, Sorting sorting, string onPager)
        {
            TagBuilder a = new TagBuilder("a");
            a.SetInnerText(linkText);
            if (linkPage >= 1 && linkPage <= pagination.TotalPages && pagination.PageIndex != linkPage)
            {
                string url = string.Empty;
                if (linkPage == 1)
                {
                    //url = urlHelper.Action(action, controller);
                    routeValueDic.Remove("p");
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                else
                {
                    routeValueDic.MergeKey("p", linkPage);
                    if (sorting != null)
                    {
                        routeValueDic.MergeKey("se", sorting.SortExpression);
                        routeValueDic.MergeKey("sd", sorting.SortDirection.ToName());
                    }
                    url = urlHelper.Action(action, controller, new RouteValueDictionary(routeValueDic));
                }
                if (!string.IsNullOrEmpty(onPager))
                    a.Attributes.Add("onclick", onPager + "('" + url + "')");
                else
                    a.Attributes.Add("href", url);
            }
            return a;
        }




    }



}