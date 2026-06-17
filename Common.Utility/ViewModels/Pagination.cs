namespace Common.Utility.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        public Pagination(int pageIndex, int pageSize, int totalCount)
        {
            if (pageSize == 0)
                pageSize = 1;
            this.TotalCount = totalCount;
            this.TotalPages = totalCount / pageSize;

            if (totalCount % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex; // 1 based page index
        }

        /// <summary>
        /// 1 based page index
        /// </summary>
        public int PageIndex { get; set; } // 1 based page index

        /// <summary>
        /// 0 based page index
        /// </summary>
        public int PageIndex0Based { get { return PageIndex - 1; } } // 0 based page index

        /// <summary>
        /// 
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int Skip { get { return (this.PageIndex - 1) * this.PageSize; } }

        /// <summary>
        /// 
        /// </summary>
        public int Take { get { return this.PageSize; } }

        /// <summary>
        /// 
        /// </summary>
        public bool HasPreviousPage { get { return (PageIndex > 1); } }

        /// <summary>
        /// 
        /// </summary>
        public bool HasNextPage { get { return (PageIndex < TotalPages); } }
    }
}
