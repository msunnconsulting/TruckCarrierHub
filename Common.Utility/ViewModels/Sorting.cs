namespace Common.Utility.ViewModels
{
    using EnumUtil;

    /// <summary>
    /// 
    /// </summary>
    public class Sorting
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string SortExpression;

        /// <summary>
        /// 
        /// </summary>
        public readonly SortingDirection SortDirection;

        /// <summary>
        /// 
        /// </summary>
        public string SortDirectionString { get { return this.SortDirection.ToName(); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        public Sorting(string sortExpression, SortingDirection sortDirection)
        {
            this.SortExpression = sortExpression;
            this.SortDirection = sortDirection;
        }
    }
}
