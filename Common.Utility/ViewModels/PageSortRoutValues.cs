namespace Common.Utility.ViewModels
{
    using EnumUtil;
    using Extensions;

    /// <summary>
    /// 
    /// </summary>
    public class PageSortPara
    {
        /// <summary>
        /// 
        /// </summary>
        public int? p { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string se { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string sd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SortingDirection sortDirection
        {
            get
            {
                if (sd == null)
                    return SortingDirection.Asc;
                return sd.ToEnum<SortingDirection>();
            }
        }

        /// <summary>
        /// Initialize ps to default values for p, se and sd
        /// </summary>
        /// <param name="ps">instance to be initialized</param>
        /// <param name="se">default sort expression</param>
        /// <param name="sd">default sort direction</param>
        public static void Init(PageSortPara ps, string se, string sd)
        {
            // Setup Default page sort values
            if (ps == null)
            {
                ps = new PageSortPara()
                {
                    p = 1,
                    sd = sd,
                    se = se
                };
                return;
            }

            if (!ps.p.HasValue)
                ps.p = 1;

            if (ps.sd == null)
                ps.sd = sd;

            if (ps.se == null)
                ps.se = se;
        }

        /// <summary>
        /// Initialize ps to default values for p, se and sd
        /// </summary>
        /// <param name="ps">instance to be initialized</param>
        /// <param name="se">default sort expression</param>
        /// <param name="sd">default sort direction</param>
        public static void Init(PageSortPara ps, string se, SortingDirection sd)
        {
            Init(ps, se, sd.ToName());
        }
    }
}
