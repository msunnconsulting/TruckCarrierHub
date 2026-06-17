namespace Common.Utility.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        public Sorting Sorting { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Pagination Pagination { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        public PagedList(int pageIndex, int pageSize, int totalCount, string sortExpression, SortingDirection sortDirection)
        {
            this.Pagination = new Pagination(pageIndex, pageSize, totalCount);
            this.Sorting = new Sorting(sortExpression, sortDirection);
            this.Items = new List<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="sorting"></param>
        public PagedList(Pagination pagination, Sorting sorting)
        {
            this.Pagination = pagination;
            this.Sorting = sorting;
            this.Items = new List<T>();
        }
    }
}
