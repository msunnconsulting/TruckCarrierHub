namespace Common.Utility.Extensions
{
    /// <summary>
    /// Utility class that provides extension methods for Int
    /// </summary>
    public static partial class IntExt
    {
        /// <summary>
        /// Create an ordinal number from any number
        /// e.g. 1 becomes 1st and 22 becomes twenty second
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <returns>Ordinal value as string</returns>
        public static string ToOrdinalNumber(this int number)
        {
            ////0 remains just 0
            if (number == 0) return "0th";

            ////handle 11-13 seperately because the next stage
            ////would create 113rd instead of 113th
            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }
            ////append the correct ordinal val
            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
            }

            return number + "th";
        }
    }
}
