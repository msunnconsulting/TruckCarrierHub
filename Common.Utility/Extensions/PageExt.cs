namespace Common.Utility.Extensions
{
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// Utility class that provides extension methods for Web Page
    /// </summary>
    public static partial class PageExt
    {
        /// <summary>
        /// Adds meta tag to the page.
        /// </summary>
        /// <param name="currentPage">Page to which meta tags need to be added.</param>
        /// <param name="name">name of the meta tag. e.g : Keyword, Description.</param>
        /// <param name="value">value of the meta tag.</param>
        public static void AddMetaTag(this Page currentPage, string name, string value)
        {
            HtmlMeta meta = new HtmlMeta();
            meta.Name = name;
            meta.Content = value;
            currentPage.Header.Controls.Add(meta);
        }
    }
}
