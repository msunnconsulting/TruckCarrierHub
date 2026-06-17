namespace Common.Utility.Extensions
{
    using System;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utility class that provides extension methods for String
    /// </summary>
    public static partial class StringExt
    {
        /// <summary>
        /// This is a method for separate  words from string
        /// </summary>
        /// <param name="value">Enter value</param>
        /// <returns>returns words</returns>
        public static string ToSeparatedWords(this string value)
        {
            if (value == null)
                return string.Empty;
            else
                return Regex.Replace(value, "([A-Z][a-z])", " $1").Trim();
        }

        /// <summary>
        /// This is a method for remove whitespace from string
        /// </summary>
        /// <param name="value">Enter value</param>
        /// <returns>returns words</returns>
        public static string ToRemoveWhiteSpaceFromdWords(this string value)
        {
            if (value == null)
                return string.Empty;
            else
                return Regex.Replace(value, @"\s+", "");
        }



        /// <summary>
        /// Merge two comma separated lists into one comma separated list.
        /// </summary>
        /// <param name="list1">Enter List1 object</param>
        /// <param name="list2">Enter List2 object</param>
        /// <returns>string of list1 and list2</returns>
        public static string MergeWithCommaSeperatedList(this string list1, string list2)
        {
            if (string.IsNullOrEmpty(list1))
                return list2;
            else if (string.IsNullOrEmpty(list2))
                return list1;

            string retrunValue = list1;

            string[] arrList2 = list2.Split(',');

            foreach (string strValue in arrList2)
            {
                if (!retrunValue.Contains(strValue))
                    retrunValue += "," + strValue;
            }

            return retrunValue;
        }

        /// <summary>
        /// When passed a string having key value pairs separated by Separator and key and value are also separated by one more separator, it will generate a name value collection for this key value and return.
        /// </summary>
        /// <param name="keyValueList">string having key value pairs in form of string</param>
        /// <param name="keyValueSeperator">separator used in string to separate key and value</param>
        /// <param name="newKeySeperator">separator used in string to separate key-value pairs</param>
        /// <returns>Name Value Collection Object</returns>
        public static NameValueCollection ToKeyValueCollection(this string keyValueList, string keyValueSeperator, string newKeySeperator)
        {
            NameValueCollection keyValues = new NameValueCollection();

            string[] arrSeperator = new string[1] { newKeySeperator };

            string[] arrKeyValues = keyValueList.Split(arrSeperator, StringSplitOptions.None);

            string[] arrKeyVal;
            arrSeperator = new string[1] { keyValueSeperator };

            foreach (string keyValue in arrKeyValues)
            {
                arrKeyVal = keyValue.Split(arrSeperator, StringSplitOptions.None);
                keyValues.Add(arrKeyVal[0], arrKeyVal[1]);
            }

            return keyValues;
        }
    }
}