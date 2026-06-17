namespace Common.Utility.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Utility class that provides extension methods for Dictionary
    /// </summary>
    public static partial class DictionaryExt
    {
        /// <summary>
        /// This method merges 2nd dictionary into current dictionary. If key already exist in first dictionary, it would just replace its value
        /// </summary>
        /// <param name="dic1"></param>
        /// <param name="dic2"></param>        
        public static void Merge(this IDictionary<string, object> dic1, IDictionary<string, object> dic2)
        {
            if (dic1 == null)
                dic1 = new Dictionary<string, object>();

            if (dic2 != null)
            {
                foreach (var pair in dic2)
                    dic1[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// This method merges 2nd dictionary into current dictionary. If key already exist and value is of any primitive type or string, it concat values by concatChar. else it would just set new value to the supplied key
        /// </summary>
        /// <param name="dic1"></param>
        /// <param name="dic2"></param>
        /// <param name="concatChar">Concat character by which new value will be concated</param>
        public static void Merge(this IDictionary<string, object> dic1, IDictionary<string, object> dic2, string concatChar)
        {
            if (dic1 == null)
                dic1 = new Dictionary<string, object>();

            if (dic2 != null)
            {
                foreach (var pair in dic2)
                    dic1.MergeKey(pair.Key, pair.Value, concatChar);
            }
        }

        /// <summary>
        /// If key already exist it would just set new value to the supplied key
        /// </summary>
        /// <param name="dictionary">this is extention method for IDictionary. Enter dictionary in which you want to add key and value.</param>
        /// <param name="key">Enter key to be merged</param>
        /// <param name="value">Enter value for key</param>        
        public static void MergeKey(this IDictionary<string, object> dictionary, string key, object value)
        {
            dictionary[key] = value;
        }

        /// <summary>
        /// If key already exist and value is of any primitive type or string, it concat values by concatChar. else it would just set new value to the supplied key
        /// </summary>
        /// <param name="dictionary">this is extention method for IDictionary. Enter dictionary in which you want to add key and value.</param>
        /// <param name="key">Enter key to be merged</param>
        /// <param name="value">Enter value for key</param>
        /// <param name="concatChar">Concat character by which new value will be concated</param>        
        public static void MergeKey(this IDictionary<string, object> dictionary, string key, object value, string concatChar)
        {
            if (dictionary.ContainsKey(key) && dictionary[key].IsTostringable())
                dictionary[key] = dictionary[key].ToString() + concatChar + value.ToString();
            else
                dictionary[key] = value;
        }
    }
}
