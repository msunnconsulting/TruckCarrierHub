namespace Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This is a Format class.
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// convert date to specific format
        /// </summary>
        /// <param name="dateToFormat">Date to be formatted.</param>
        /// <param name="formatSpecifier">Result format</param>
        /// <returns>string of Date</returns>
        public static string DateToString(DateTime? dateToFormat, string formatSpecifier)
        {
            if (dateToFormat.HasValue)
                return dateToFormat.Value.ToString(formatSpecifier, CultureInfo.CreateSpecificCulture("en-US"));
            else
                return string.Empty;
        }

        /// <summary>
        /// Converts string to Date. It uses supplied date format as base format of the string.
        /// </summary>
        /// <param name="stringToConvert">enter string to convert into date</param>
        /// <param name="formatSpecifier">enter format</param>
        /// <returns>returns date in date time</returns>
        public static DateTime? StringToDate(string stringToConvert, string formatSpecifier)
        {
            if (string.IsNullOrEmpty(stringToConvert))
                return null;
            else
            {
                try
                {
                    return DateTime.ParseExact(stringToConvert, formatSpecifier, CultureInfo.CreateSpecificCulture("en-US"));
                }
                catch
                {
                    return DateFromSQLString(stringToConvert);
                }
            }
        }

        /// <summary>
        /// Converts a Number to specific formatted string with decimal places specified.
        /// </summary>
        /// <param name="number">Enter number</param>
        /// <param name="decimalPlaces">Enter Decimal places</param>
        /// <returns>returns number into string</returns>
        public static string NumberToString(string number, int decimalPlaces)
        {
            if (string.IsNullOrEmpty(number.Trim()))
                number = "0";

            number = Convert.ToDouble(number).ToString("N" + decimalPlaces.ToString());

            return number;
        }

        /// <summary>
        /// Converts a Number to amount format with thousand separator and with currency symbol prefixed.
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <param name="currencySymbol">Enter symbol of currency</param>
        /// <returns>returns number in amount</returns>
        public static string NumberToAmount(string number, string currencySymbol)
        {
            currencySymbol = Convert.ToString(currencySymbol);

            if (!string.IsNullOrEmpty(number.Trim()) && number != "0")
                number = currencySymbol + string.Format("{0:#,###}", Convert.ToDouble(number));
            else
                number = currencySymbol + "0";

            return number;
        }

        /// <summary>
        /// this method converts bytes to mega bytes
        /// </summary>
        /// <param name="bytes">Enter bytes</param>
        /// <returns>returns bytes in mega bytes</returns>
        public static double ToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        /// <summary>
        /// Converts the pascal word to sentence.
        /// </summary>
        /// <param name="pascalWord">The pascal word.</param>
        /// <returns>new set with join</returns>
        public static string PascalWordToSentence(string pascalWord)
        {
            if (pascalWord == null)
                return string.Empty;
            string[] keepLowerCased = new[] { "am", "is", "are", "were", "of", "the" };

            string baseConvertedString = Regex.Replace(pascalWord, "([A-Z]+|[0-9]+)", " $1").TrimStart();

            string[] words = baseConvertedString.Split(' ');
            List<string> newSet = new List<string>();
            for (int i = 0; i < words.Length; i++)
            {
                if (i > 0 && keepLowerCased.Contains(words[i], StringComparer.OrdinalIgnoreCase))
                    newSet.Add(words[i].ToLower());
                else
                    newSet.Add(words[i]);
            }

            return string.Join(" ", newSet);
        }

        /// <summary>
        /// Converts date as string came from SQL Server to Date.
        /// </summary>
        /// <param name="stringToConvert">enter string to convert</param>
        /// <returns>returns date in date time</returns>
        private static DateTime? DateFromSQLString(string stringToConvert)
        {
            if (string.IsNullOrEmpty(stringToConvert))
                return null;
            else
            {
                try
                {
                    return DateTime.ParseExact(stringToConvert, "dd/MM/yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
                }
                catch
                {
                    DateTime objDate;
                    if (DateTime.TryParse(stringToConvert, out objDate))
                        return objDate;
                    else
                        return null;
                }
            }
        }
    }
}
