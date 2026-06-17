

namespace Common.Utility.MVC
{
    using System.Web.Mvc;

    /// <summary>
    /// 
    /// </summary>
    public static partial class TagBuilderExt
    {

        /// <summary>
        /// if attribute already exist in the tag builder, it appends the new value prefixed by space to existing value, if attribute doesn't exist, then it just add it
        /// </summary>
        /// <param name="tagBuilder">current tag builder</param>
        /// <param name="key">attribute whose value need to be merged</param>
        /// <param name="value">value to be set for attribute</param>        
        public static void MergeAttributeByConcat(this TagBuilder tagBuilder, string key, string value)
        {
            if (tagBuilder.Attributes.ContainsKey(key))
                tagBuilder.Attributes[key] = tagBuilder.Attributes[key] + " " + value;
            else
                tagBuilder.Attributes.Add(key, value);
        }
    }
}
