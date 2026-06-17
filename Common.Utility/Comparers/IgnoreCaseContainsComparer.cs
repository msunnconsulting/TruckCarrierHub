namespace Common.Utility.Comparers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This utility class allows to use contains operation on LINQ string
    /// </summary>
    public class IgnoreCaseContainsComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// this method checks if x contains y or not
        /// </summary>
        /// <param name="x">Enter string x</param>
        /// <param name="y">Enter string y</param>
        /// <returns>returns element of x that contains y</returns>
        public bool Equals(string x, string y)
        {
            x = x.ToLower();
            y = y.ToLower();

            return x.Contains(y);
        }

        /// <summary>
        /// This is a method for get hash code
        /// </summary>
        /// <param name="obj">Enter string object for which you want to get hash code</param>
        /// <returns>returns hash code</returns>
        public int GetHashCode(string obj)
        {
            throw new NotImplementedException();
        }
    }
}
