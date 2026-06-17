namespace Common.Utility.Extensions
{
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Utility class that provides extension methods for PropertyInfo
    /// </summary>
    public static partial class PropertyInfoExt
    {
        /// <summary>
        /// This is a method for getting attribute of specific entity
        /// </summary>
        /// <typeparam name="T">Enter entity from which you want to get attribute</typeparam>
        /// <param name="propertyInfo">enter property information</param>
        /// <returns>returns attributes</returns>
        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : class
        {
            return propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// this method checks that attribute is exist or not
        /// </summary>
        /// <typeparam name="T">Enter Entity for which you want to check attribute is exist or not</typeparam>
        /// <param name="propertyInfo">Enter Property info</param>
        /// <returns>indicates that attribute is exist or not</returns>
        public static bool AttributeExists<T>(this PropertyInfo propertyInfo) where T : class
        {
            var attribute = propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            return !(attribute == null);
        }
    }
}
