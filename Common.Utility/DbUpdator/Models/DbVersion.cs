
namespace Common.Utility.DbUpdator.Models
{
    using System;

    internal sealed class DbVersion
    {
        public long Id { get; set; }
        public long Major { get; set; }
        public long Minor { get; set; }
        public long Build { get; set; }
        public DateTime DateUpdated { get; set; }
        public string Version
        {
            get
            {
                return Major.ToString() + "." + Minor.ToString() + "." + Build.ToString();
            }
        }
    }
}
