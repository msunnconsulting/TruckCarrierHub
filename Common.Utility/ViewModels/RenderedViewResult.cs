namespace Common.Utility.ViewModels
{
    using EnumUtil;

    /// <summary>
    /// indicates if renderedView should be inserted before, after, replaced or just content should be replaced
    /// </summary>
    public enum InsertionMode
    {
        /// <summary>
        /// Replace the content of element.
        /// </summary>
        [EnumName("replace")]
        Replace = 0,

        /// <summary>
        /// Insert before the element.
        /// </summary>
        [EnumName("before")]
        InsertBefore = 1,

        /// <summary>
        /// Insert after the element.
        /// </summary>
        [EnumName("after")]
        InsertAfter = 2,

        /// <summary>
        /// Replace the entire element.
        /// </summary>
        [EnumName("replace-with")]
        ReplaceWith = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public class RenderedViewResult<T> : APIResult<T>
    {
        /// <summary>
        /// selector of the target for redneredView
        /// </summary>
        public string UpdateTarget;

        /// <summary>
        /// indicates if renderedView should be inserted before, after, replaced or just content should be replaced
        /// </summary>
        public string InsertionMode;
    }
}