namespace Common.Utility.Extensions
{
    using System;
    using System.Linq;

    /// <summary>
    /// Utility class that provides extension methods for Type
    /// </summary>
    public static partial class TypeExt
    {
        /// <summary>
        /// This is a method for getting attribute of specific Entity
        /// </summary>
        /// <typeparam name="T">Enter Entity from which you want to get attribute</typeparam>
        /// <param name="type">Enter type for attribute</param>
        /// <returns>return attribute</returns>
        public static T GetAttribute<T>(this Type type) where T : class
        {
            return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// This method checks that attribute is exist or not
        /// </summary>
        /// <typeparam name="T">Enter Entity for which you want to check that attribute is exist or not</typeparam>
        /// <param name="type">Enter type for Attribute</param>
        /// <returns>indicates that attribute is exist or not</returns>
        public static bool AttributeExists<T>(this Type type) where T : class
        {
            var attribute = type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            return !(attribute == null);
        }

        /// <summary>
        /// Returns if a type is a built in type or not. 
        /// Built in types are : DateTime, String, Guid, Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double,Binary, DateTimeOffset, Decimal and Single
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBuiltInType(this Type type)
        {
            string typeName = type.Name;
            if (typeName.ToLower().Contains("nullable"))
            {
                typeName = type.GenericTypeArguments[0].Name;
            }

            typeName = typeName.ToLower();

            //http://thedatafarm.com/data-access/sql-server-2008-data-types-and-entity-framework-4/
            string[] builtInTypes = new string[] { "String", "Int32", "Int64", "Boolean", "DateTime", "Byte"
                , "SByte", "Int16", "UInt16", "UInt32", "UInt64", "IntPtr", "UIntPtr", "Char", "Double", "Single", "Guid"
                , "Binary", "DateTimeOffset", "Decimal", "Time"
            };

            return builtInTypes.Any(m => m.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
