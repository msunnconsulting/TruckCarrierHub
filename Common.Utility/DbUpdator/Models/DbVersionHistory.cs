namespace Common.Utility.DbUpdator.Models
{
    using System;
    internal partial class DbVersionHistory
    {
        public long Id { get; set; }
        public Nullable<long> OldMajor { get; set; }
        public Nullable<long> OldMinor { get; set; }
        public Nullable<long> OldBuild { get; set; }
        public long NewMajor { get; set; }
        public long NewMinor { get; set; }
        public long NewBuild { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
