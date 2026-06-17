namespace Common.Utility.Extensions
{
    using System;
    using System.Collections;

    /// <summary>
    /// Utility class that provides extension methods for ArrayList
    /// </summary>
    public static partial class ArrayListExt
    {
        /// <summary>
        /// Converts array list to separated list of values in single string. If value is null, it would be skipped
        /// </summary>
        /// <param name="arrList">Extention method for arraylist</param>
        /// <param name="separator">Enter separator which you want to saperate list in string</param>
        /// <returns>string of array list with separator</returns>
        public static string ToString(this ArrayList arrList, string separator)
        {
            string returnValue = string.Empty;
            foreach (object strItem in arrList)
            {
                if (strItem == null)
                    continue;
                else if (strItem.IsTostringable())
                    returnValue += strItem.ToString() + separator;
                else
                    throw new Exception("value " + strItem.ToObjString() + " can not be converted to string");
            }

            return returnValue.Trim(separator.ToCharArray());
        }
    }
}
