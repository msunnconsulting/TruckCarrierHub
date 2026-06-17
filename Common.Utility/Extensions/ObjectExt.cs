namespace Common.Utility.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Utility class that provides extension methods for Object
    /// </summary>
    public static partial class ObjectExt
    {
        /// <summary>
        /// This method is for set property value
        /// </summary>
        /// <param name="obj">This is an Extention method for object.</param>
        /// <param name="propertyName">Enter property name for which you want to set value</param>
        /// <param name="value">Enter value object that set to property which you pass as a parameter</param>
        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            PropertyInfo prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (null != prop && prop.CanWrite)
            {
                if (prop.PropertyType == value.GetType())
                    prop.SetValue(obj, value, null);
                else
                {
                    string typeName = prop.PropertyType.Name.ToLower();
                    if (typeName.Contains("nullable") || typeName.Contains("int32"))
                    {
                        if (value != DBNull.Value)
                        {
                            typeName = prop.PropertyType.GenericTypeArguments[0].Name.ToLower();
                        }


                        if (value == DBNull.Value || value == null)
                        {
                            prop.SetValue(obj, null, null);
                            return;
                        }
                    }

                    //// if property type is not same as value type, we have to convert it.
                    switch (typeName)
                    {
                        case "byte":
                            value = Convert.ToByte(value);
                            break;
                        case "char":
                            value = Convert.ToChar(value);
                            break;
                        case "boolean":
                            bool returnValue;
                            string strBool = value.ToString().Trim();
                            if (bool.TryParse(strBool, out returnValue))
                                value = returnValue;
                            else if (strBool == "1")
                                value = true;
                            else if (strBool == "0")
                                value = false;
                            //// else - don't do anyhthing, let's try type conversion, it would throw execption for invalid value
                            break;
                        case "datetime":
                            value = Convert.ToDateTime(value);
                            break;
                        case "decimal":
                            value = Convert.ToDecimal(value);
                            break;
                        case "double":
                            value = Convert.ToDouble(value);
                            break;
                        case "int16":
                            value = Convert.ToInt16(value);
                            break;
                        case "int32":
                            value = Convert.ToInt32(value);
                            break;
                        case "int64":
                            value = Convert.ToInt64(value);
                            break;
                        case "sbyte":
                            value = Convert.ToSByte(value);
                            break;
                        case "single":
                            value = Convert.ToSingle(value);
                            break;
                        case "string":
                            value = Convert.ToString(value);
                            break;
                        case "uint16":
                            value = Convert.ToUInt16(value);
                            break;
                        case "uint32":
                            value = Convert.ToUInt32(value);
                            break;
                        case "uint64":
                            value = Convert.ToUInt64(value);
                            break;
                        default:
                            throw new Exception("Type conversion not supported for " + prop.PropertyType.Name);
                    }

                    prop.SetValue(obj, value, null);
                }
            }
            else
                throw new Exception("Property Cannot Be Set");
        }

        /// <summary>
        /// This is method for convert data to dictionary
        /// </summary>
        /// <param name="data">pass object</param>
        /// <returns>return dictionary object</returns>
        public static IDictionary<string, object> ToDictionary(this object data)
        {
            if (data == null)
                return new Dictionary<string, object>();
            else if (data.GetType() == typeof(IDictionary<string, object>) || data.GetType() == typeof(Dictionary<string, object>))
                return (IDictionary<string, object>)data;

            BindingFlags publicAttributes = BindingFlags.Public | BindingFlags.Instance;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (PropertyInfo property in
                     data.GetType().GetProperties(publicAttributes))
            {
                if (property.CanRead)
                {
                    dictionary.Add(property.Name, property.GetValue(data, null));
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Returns the typename of the underlaying data type without namespace
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string TypeName(this object obj)
        {
            string typeName = obj.GetType().Name;
            if (typeName.ToLower().Contains("nullable"))
            {
                typeName = obj.GetType().GenericTypeArguments[0].Name;
            }
            return typeName;
        }

        /// <summary>
        /// Returns true, if obj.ToString() is suppose to actually return the value of the object as string. Returns false if that is not possible
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsTostringable(this object obj)
        {
            //The primitive types are .
            if (obj == null) return false;
            return obj.GetType().IsBuiltInType();
        }

        /// <summary>
        /// Converts object to string (currently support class or primitive types or string only), for any other types, it would return default result of .toString()
        /// </summary>
        /// <param name="obj">pass object</param>
        /// <returns>returns object of stream builder with append properties</returns>
        public static string ToObjString(this object obj)
        {
            if (obj == null) return string.Empty;

            if (obj.IsTostringable()) return obj.ToString();


            var buffer = new StringBuilder();
            buffer.Append(string.Format("{0} ", obj.GetType().Name));
            buffer.AppendLine(" { ");

            var properties = obj.GetType().GetProperties();
            foreach (var p in properties)
            {
                buffer.AppendLine(string.Format(" {0} = {1} ", p.Name, p.GetValue(obj, null).ToObjString()));
            }

            buffer.AppendLine(" } ");
            return buffer.ToString();
        }

        /// <summary>
        /// converts object of premetivetype or string to given enum. If any other type, it would throw error. If value is out of range for enum, it would use return default value of enum
        /// </summary>
        /// <typeparam name="T">Enter enum Entity in which you want to convert this entity</typeparam>
        /// <param name="obj"></param>
        /// <returns>return enum</returns>
        public static T ToEnum<T>(this object obj)
        {
            if (!typeof(T).IsEnum)
                throw new Exception("Type " + typeof(T).TypeName() + " is not enum");

            if (!obj.IsTostringable())
                throw new Exception("Type " + obj.TypeName() + " cannot be converted to enum");

            try
            {
                T res = (T)Enum.Parse(typeof(T), obj.ToString());
                if (!Enum.IsDefined(typeof(T), res)) return default(T);
                return res;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
