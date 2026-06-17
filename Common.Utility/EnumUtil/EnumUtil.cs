namespace Common.Utility.EnumUtil
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web.UI.WebControls;

    /// <summary>
    /// This class provides utility method for Enums
    /// </summary>
    public static class EnumUtil
    {
        /// <summary>
        /// this method converts enum to list that can be bound to a control as Name-Value pair.
        /// </summary>
        /// <typeparam name="T">Type of enum to be converted to list</typeparam>
        /// <returns>Returns list of Name-Value pairs for entire enum</returns>        
        public static List<ListItem> ToList<T>() where T : struct
        {
            List<ListItem> itemList = new List<ListItem>();
            if (typeof(T).IsEnum)
            {
                foreach (Enum record in Enum.GetValues(typeof(T)))
                    itemList.Add(new ListItem(record.ToName(), Convert.ToInt32(record).ToString()));
            }
            return itemList.OrderBy(m => m.Text).ToList();
        }
    }

    /// <summary>
    /// This is the attribute which can assign Description to an Enum value
    /// </summary>
    public class EnumDescription : DescriptionAttribute
    {
        private string description;

        /// <summary>
        /// Description of the enum value
        /// </summary>
        public override string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// Constructor that is used to initialize description for the enum value
        /// </summary>
        /// <param name="description">description for the enum value</param>
        public EnumDescription(string description)
        {
            this.description = description;
        }
    }

    /// <summary>
    /// This is the attribute which can assign Name to an Enum value
    /// </summary>
    public class EnumName : DescriptionAttribute
    {
        private string name;

        /// <summary>
        /// Name of the enum value
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Constructor that is used to initialize Name for the enum value
        /// </summary>
        /// <param name="name">name for the enum value</param>
        public EnumName(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// This is extension class using which we enable ToName and ToDescription extension methods of Enum
    /// </summary>
    public static partial class EnumExt
    {
        /// <summary>
        /// This method returns name of the enum value. If no Name attribute given, then it gives value.toString()
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToName(this Enum value)
        {
            var attribute = value.GetAttribute<EnumName>();
            return attribute == null ? value.ToString() : attribute.Name;
        }

        /// <summary>
        /// This method returns description of the enum value. If no Description attribute given, then it gives value.toString()
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum value)
        {
            var attribute = value.GetAttribute<EnumDescription>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// This extension method is returns metatdata on EnumValue setup using attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            if (attributes != null && attributes.Length > 0)
                return (T)attributes[0];
            else
                return null;
        }
    }

}
