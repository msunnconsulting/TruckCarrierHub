namespace Common.Utility.RegEx
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// enum for URLOption
    /// </summary>
    public enum UrlOptions
    {
        /// <summary>
        /// must provide protocol for url
        /// </summary>
        RequireProtocol,

        /// <summary>
        /// protocol for url is optional
        /// </summary>
        OptionalProtocol,

        /// <summary>
        /// protocol must not be supplied
        /// </summary>
        DisallowProtocol
    }

    /// <summary>
    /// This is a static RegExUtil class.
    /// </summary>
    public static class RegExUtil
    {
        /// <summary>
        /// This is a constant field for Email
        /// </summary>
        public const string Email = @"^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$";

        /// <summary>
        /// This is a constant field for Digits
        /// </summary>
        public const string Digits = @"^\d*$";

        /// <summary>
        /// This is a constant field for Strong Password
        /// </summary>
        public const string StrongPassword = @"(?=^.{8,}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$";

        /// <summary>
        /// This is a constant field for Phone number validation
        /// </summary>
        public const string PhoneNumber = "\\+?[-()\\d]+";

        ////this is date format for which we need a regular expression. date format must use yyyy to indicate year, mm to indicate month & dd to indicate day. Example : yyyy-mm-dd or yyyy/mm/dd etc

        /// <summary>
        /// this method converts passed date format to a regular expression that can match dates in that format. min date and max date valid value would be .net compatible only.
        /// </summary>
        /// <param name="dateFormat">Enter Date Format to convert it into dotnet date format</param>
        /// <returns>returns the regular expression matching a date for date format supplied.</returns>
        public static string DotNetDate(string dateFormat)
        {
            dateFormat = dateFormat.ToLower();
            ////if (dateFormat.Contains("/"))
            ////    dateFormat = dateFormat.Replace("/", "/.");

            ////DateTime.MinValue : January 1, 0001
            ////DateTime.MaxValue : December 31, 9999       

            string year, month, day;
            year = @"(([0-9][0-9][0-9][1-9])|([1-9][0-9][0-9][0-9])|([0-9][1-9][0-9][0-9])|([0-9][0-9][1-9][0-9]))";
            month = @"(0[1-9]|1[012])";
            day = "(0[1-9]|[12][0-9]|3[01])";

            string hour, minute, seconds, ampm;
            hour = @"(0[0-9]|1[01])";
            minute = @"(0[0-9]|[10-59])";
            seconds = "(0[0-9]|[10-59])";
            ampm = "(am|pm)";

            dateFormat = @"^" + dateFormat + "$";
            dateFormat = dateFormat.Replace("yyyy", year);
            dateFormat = dateFormat.Replace("mm", month);
            dateFormat = dateFormat.Replace("dd", day);
            dateFormat = dateFormat.Replace("hh", hour);
            dateFormat = dateFormat.Replace("mm", minute);
            dateFormat = dateFormat.Replace("ss", seconds);
            dateFormat = dateFormat.Replace("tt", ampm);

            return dateFormat;
        }

        /// <summary>
        /// This is method for convert time into 24 hour format
        /// </summary>
        /// <param name="timeFormat">Enter time format to convert it into 24 hours time format </param>
        /// <returns>returns time format in 24 hours</returns>
        public static string TimeIn24HourFormat(string timeFormat)
        {
            timeFormat = timeFormat.ToLower();

            string hour, minute, seconds, ampm;
            hour = @"(0[0-9]|1[01])";
            minute = @"(0[0-9]|[10-59])";
            seconds = "(0[0-9]|[10-59])";
            ampm = "(am|pm)";
            timeFormat = @"^" + timeFormat + "$";
            timeFormat = timeFormat.Replace("hh", hour);
            timeFormat = timeFormat.Replace("mm", minute);
            timeFormat = timeFormat.Replace("ss", seconds);
            timeFormat = timeFormat.Replace("tt", ampm);

            return timeFormat;
        }

        ////in SQLDATE method date format is for which we need a regular expression. date format must use yyyy to indicate year, mm to indicate month and dd to indicate day. Example : yyyy-mm-dd or yyyy/mm/dd etc.

        /// <summary>
        /// converts passed date format to a regular expression that can match dates in that format. min date and max date valid value would be SQL server 2005+ compatible only.
        /// </summary>
        /// <param name="dateFormat">Enter Date Format to convert it into SQL Date format</param>
        /// <returns>Returns the regular expression matching a date for date format supplied.</returns>
        public static string SQLDate(string dateFormat)
        {
            dateFormat = dateFormat.ToLower();

            ////DateTime.MinValue : January 1, 1753
            ////DateTime.MaxValue : December 31, 9999
            //// http://stackoverflow.com/questions/548353/finding-max-possible-date-in-ms-sql-server-2005

            string year, month, day;
            year = @"(175[3-9]|17[6-9][0-9]|1[89][0-9][0-9]|[2-9][0-9][0-9][0-9])";
            month = @"(0[1-9]|1[012])";
            day = "(0[1-9]|[12][0-9]|3[01])";
            dateFormat = @"^" + dateFormat + "$";
            dateFormat = dateFormat.Replace("yyyy", year);
            dateFormat = dateFormat.Replace("mm", month);
            dateFormat = dateFormat.Replace("dd", day);

            return dateFormat;
        }

        /// <summary>
        /// converts passed date format to a regular expression that can match dates in that format. there's no restriction about min date and max date valid value.
        /// </summary>
        /// <param name="dateFormat">date format for which we need a regular expression. date format must use yyyy to indicate year, mm to indicate month and dd to indicate day. Example : yyyy-mm-dd or yyyy/mm/dd etc</param>
        /// <returns>Returns the regular expression matching a date for date format supplied.</returns>
        public static string AnyDate(string dateFormat)
        {
            dateFormat = dateFormat.ToLower();

            string year, month, day;
            year = @"(\d{4})";
            month = @"(\d{2})";
            day = @"(\d{2})";
            dateFormat = @"^" + dateFormat + "$";
            dateFormat = dateFormat.Replace("yyyy", year);
            dateFormat = dateFormat.Replace("mm", month);
            dateFormat = dateFormat.Replace("dd", day);

            return dateFormat;
        }

        // TODO : Regular Expression for DateTime
        //// below method returns regular expression for Integer value with specific length based on minimum Length & maximum Length specified.

        /// <summary>
        /// Generates and return a regular expression for Integer value with specific length based on minLength and maxLength specified.
        /// Both the length parameters are optional, but none of them can have a negative value.
        /// </summary>
        /// <param name="minLength">Enter Minimum length for the integer.by default its 0</param>
        /// <param name="maxLength">Enter Maximum length for the integer. by default its 0.Length 0 will be treated as infinite.</param>
        /// <returns>returns integer value</returns>
        public static string Integer(int minLength = 0, int maxLength = 0)
        {
            if (minLength < 0 || maxLength < 0)
                throw new Exception("minLength and maxLength must not be negative value.");

            if (minLength == 0 && maxLength == 0)
                return @"^[+-]{0,1}\d*$";
            else if (maxLength > 0 && minLength > 0)
                return @"^[+-]{0,1}\d{" + minLength + "," + maxLength + "}$";
            else if (minLength > 0 && maxLength == 0)
                return @"^[+-]{0,1}\d{" + minLength + ",}$";
            else if (maxLength > 0 && minLength == 0)
                return @"^[+-]{0,1}\d{0," + maxLength + "}$";
            else
                throw new Exception("Imposissible.");
        }

        /// <summary>
        /// Generates and return a regular expression for Integer value with exact length. Length must not be negative or 0.
        /// </summary>
        /// <param name="length">Length for the integer. Must be a positive value</param>
        /// <returns>A regular expression for Integer value with exact length.</returns>
        public static string Integer(int length)
        {
            if (length <= 0)
                throw new Exception("length must be a positive value.");

            return @"^\d{" + length + "}$";
        }

        //// below method Generates and return a regular expression for Alpha value(A to Z or a to z) with specific length based on minLength & maxLength specified Both the length parameters are optional, but none of them can have a negative value.
        //// returns a regular expression for Alpha value with specific length based on minLength & maxLength specified

        /// <summary>
        /// this is a Alpha method for regular expression
        /// </summary>
        /// <param name="minLength">Enter Minimum length for the Alpha. by default its 0</param>
        /// <param name="maxLength">Enter Maximum length for the Alpha and Length 0 will be treated as infinite</param>
        /// <returns>returns regular expression for alpha</returns>
        public static string Alpha(int minLength = 0, int maxLength = 0)
        {
            if (minLength < 0 || maxLength < 0)
                throw new Exception("minLength and maxLength must not be negative value.");

            if (minLength == 0 && maxLength == 0)
                return @"^[a-zA-Z]*$";
            else if (maxLength > 0 && minLength > 0)
                return @"^[a-zA-Z]{" + minLength + "," + maxLength + "}$";
            else if (minLength > 0 && maxLength == 0)
                return @"^[a-zA-Z]{" + minLength + ",}$";
            else if (maxLength > 0 && minLength == 0)
                return @"^[a-zA-Z]{0," + maxLength + "}$";
            else
                throw new Exception("Imposissible.");
        }

        /// <summary>
        /// Generates and return a regular expression for Alpha value(A to Z or a to z) with exact length. Length must not be negative or 0.
        /// </summary>
        /// <param name="length">Length for the Alpha. Must be a positive value</param>
        /// <returns>A regular expression for Alpha value with exact length.</returns>
        public static string Alpha(int length)
        {
            if (length <= 0)
                throw new Exception("length must be a positive value.");

            return @"^[a-zA-Z]{" + length + "}$";
        }

        //// below method Generates and return a regular expression for AlphaNumeric value(A to Z or a to z or 0 to 9) with specific length based on minLength & maxLength specified. Both the length parameters are optional, but none of them can have a negative value.
        //// returns A regular expression for AlphaNumeric value with specific length based on minLength & maxLength specified.

        /// <summary>
        /// this is a method for regular expression for alpha numeric
        /// </summary>
        /// <param name="minLength">Enter Minimum length for the AlphaNumeric. by default its 0</param>
        /// <param name="maxLength">Enter Maximum length for the AlphaNumeric. by default its 0 Length 0 will be treated as infinite.</param>
        /// <returns>returns regular expression for alpha numeric</returns>
        public static string AlphaNumeric(int minLength = 0, int maxLength = 0)
        {
            if (minLength < 0 || maxLength < 0)
                throw new Exception("minLength and maxLength must not be negative value.");

            if (minLength == 0 && maxLength == 0)
                return @"^[a-zA-Z0-9]*$";
            else if (maxLength > 0 && minLength > 0)
                return @"^[a-zA-Z0-9]{" + minLength + "," + maxLength + "}$";
            else if (minLength > 0 && maxLength == 0)
                return @"^[a-zA-Z0-9]{" + minLength + ",}$";
            else if (maxLength > 0 && minLength == 0)
                return @"^[a-zA-Z0-9]{0," + maxLength + "}$";
            else
                throw new Exception("Imposissible.");
        }

        /// <summary>
        /// Generates and return a regular expression for AlphaNumeric value(A to Z or a to z or 0 to 9) with exact length. Length must not be negative or 0.
        /// </summary>
        /// <param name="length">Length for the AlphaNumeric. Must be a positive value</param>
        /// <returns>A regular expression for AlphaNumeric value with exact length.</returns>
        public static string AlphaNumeric(int length)
        {
            if (length <= 0)
                throw new Exception("length must be a positive value.");

            return @"^[a-zA-Z0-9]{" + length + "}$";
        }

        /// <summary>
        /// Generates and return a regular expression for Numeric (Decimal) value. This would also accept integer number with max length = maxDigitsBeforeDecimal.
        /// </summary>
        /// <param name="minDigitsBeforeDecimal">Minimum number of digits allowed before decimal place.</param>
        /// <param name="maxDigitsBeforeDecimal">Maximum number of digits allowed before decimal place.</param>
        /// <param name="minDigitsAfterDecimal">Minimum number of digits allowed after decimal place.</param>
        /// <param name="maxDigitsAfterDecimal">Maximum number of digits allowed after decimal place.</param>
        /// <returns>>A regular expression for Numeric value.</returns>
        public static string Numeric(bool hasOptionalSign = true, int minDigitsBeforeDecimal = 0, int maxDigitsBeforeDecimal = 29, int minDigitsAfterDecimal = 0, int maxDigitsAfterDecimal = 2)
        {
            if (maxDigitsBeforeDecimal < 0 || maxDigitsAfterDecimal < 0 || minDigitsBeforeDecimal < 0 || minDigitsAfterDecimal < 0)
                throw new Exception("Parameter cannot be a negative value");

            if (maxDigitsBeforeDecimal > 29 || maxDigitsAfterDecimal > 29 || minDigitsBeforeDecimal > 29 || minDigitsAfterDecimal > 29)
                throw new Exception("max digits can't be more than 29");

            string beforeDecimal, afterDecimal, sign;

            beforeDecimal = string.Format(@"\d{{0},{1}}", minDigitsBeforeDecimal, maxDigitsBeforeDecimal);
            afterDecimal = string.Format(@"\d{{0},{1}}", minDigitsAfterDecimal, maxDigitsAfterDecimal);

            if (hasOptionalSign)
                sign = "[+-]{0,1}";
            else
                sign = "[+-]";
            return string.Format(@"^{0}{1}(\.{2})?$", sign, beforeDecimal, afterDecimal);
        }

        /// <summary>
        /// This is a method for match value as a string with reg
        /// </summary>
        /// <param name="value">Enter object of value</param>
        /// <param name="pattern">Enter pattern</param>
        /// <param name="options">Enter option for reg</param>
        /// <returns>returns true or false for is match or not</returns>
        public static bool IsMatch(object value, string pattern, RegexOptions options = (RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {
            if (value == null) return false;
            Regex reg = new Regex(pattern, options);

            string valueAsString = Convert.ToString(value);
            MatchCollection matches = reg.Matches(valueAsString, 0);

            return matches.Count > 0 && matches.Cast<Match>().Any(x => x.Length == valueAsString.Length);
        }

        /// <summary>
        /// This is a method for get url in reg
        /// </summary>
        /// <param name="urlOptions">Enter url option</param>
        /// <returns>returns url in reg</returns>
        public static string Url(UrlOptions urlOptions = UrlOptions.OptionalProtocol)
        {
            string pattern = null;
            switch (urlOptions)
            {
                case UrlOptions.RequireProtocol:
                    pattern = @"^(https?|ftp):\/\/(((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";
                    break;
                case UrlOptions.OptionalProtocol:
                    pattern = @"^((https?|ftp):\/\/)?(((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";
                    break;
                case UrlOptions.DisallowProtocol:
                    pattern = @"^(((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("urlOptions");
            }

            return pattern;
        }


    }
}
