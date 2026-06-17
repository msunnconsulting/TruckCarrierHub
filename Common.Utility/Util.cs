namespace Common.Utility
{
    using System;

    /// <summary>
    /// this is a static Util Class.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// This is a constant field for Long Date Format
        /// </summary>
        public const string LongDateFormat = "{0:dddd, MMM dd, yyyy}";

        /// <summary>
        /// This is a constant field for Short Time Format
        /// </summary>
        public const string ShortTimeFormat = "{0:hh:mm tt}";

        /// <summary>
        /// This is a constant field for Long Date Time Format
        /// </summary>
        public const string LongDateTimeFormat = "{0:F}";

        /// <summary>
        /// This is a constant field for Short Date Time Format
        /// </summary>
        public const string ShortDateTimeFormat = "{0:MM/dd/yyyy HH:mm tt}";

        /// <summary>
        /// This is a method for Get Random alphanumeric string
        /// </summary>
        /// <param name="length">Enter length</param>
        /// <returns>returns random string of alphanumeric</returns>
        public static string GetRandomAlphaNumericString(int length)
        {
            return GetRandomString(length, "abcdefghijkmnopqrstuvwxyz0123456789");
        }

        /// <summary>
        /// This is a method to getting a random string
        /// </summary>
        /// <param name="length">Enter length</param>
        /// <param name="allowedChars">Enter allowed characters</param>
        /// <returns>returns random string</returns>
        public static string GetRandomString(int length, string allowedChars)
        {
            char[] chars = new char[length];
            Random rd = new Random();

            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}
