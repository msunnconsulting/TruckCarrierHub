namespace Common.Utility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class DataKeyValueException : Exception
    {
        /// <summary>
        /// gives error messages along with key as field which gave error and relevant message for that field
        /// </summary>
        public Dictionary<string, string> DataErrors;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public DataKeyValueException(string key, string value)
        {
            this.DataErrors = new Dictionary<string, string>();
            this.DataErrors.Add(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errors"></param>
        public DataKeyValueException(Dictionary<string, string> errors)
        {
            if (errors == null)
                this.DataErrors = new Dictionary<string, string>();
            else
                this.DataErrors = errors;
        }
    }
}
