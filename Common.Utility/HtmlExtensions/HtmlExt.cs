namespace Common.Utility.Helpers.MVC.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    public static partial class HtmlExtensions
    {


        /// <summary>
        /// Generate checkbox list
        /// </summary>
        /// <param name="name">name using which you want to post selected values</param>
        /// <param name="data">data to bind checkbox list with</param>
        /// <param name="selectedValues">default selected values</param>
        /// <param name="columnwidth">width of one column of list e.g. 20px</param>
        /// <param name="columns">number of columns to be displayed</param>
        /// <returns></returns>
        //public static IHtmlString CheckboxList(this HtmlHelper htmlHelper, string name, string[] data, string[] selectedValues, string columnwidth, int columns)
        //{

        //    List<KeyValuePair<string, string>> lstData = new List<KeyValuePair<string, string>>();

        //    foreach (string str in data)
        //        lstData.Add(new KeyValuePair<string, string>(str, str));

        //    return CheckboxList(htmlHelper, name, lstData, selectedValues.ToList(), columnwidth, columns);
        //}

        /// <summary>
        /// Generate checkbox list
        /// </summary>
        /// <param name="name">name using which you want to post selected values</param>
        /// <param name="data">data to bind checkbox list with</param>
        /// <param name="selectedValues">default selected values</param>
        /// <param name="columnwidth">width of one column of list e.g. 20px</param>
        /// <param name="columns">number of columns to be displayed</param>
        /// <returns></returns>
        //public static IHtmlString CheckboxList<TKey, TValue>(this HtmlHelper htmlHelper, string name, List<KeyValuePair<TKey, TKey>> data, List<TValue> selectedValues, string columnwidth, int columns)
        //{
        //    if (data == null || data.Count == 0)
        //        return MvcHtmlString.Create(string.Empty);

        //    StringBuilder sbCheckboxList = new StringBuilder(string.Empty);
        //    string valueAsString, inputName, inputId, displayLabel;
        //    int col = 1;

        //    sbCheckboxList.Append("<table><thead><tr>");
        //    while (col <= columns)
        //    {
        //        sbCheckboxList.Append("<th style='width:" + columnwidth + "'></th>");
        //        col++;
        //    }
        //    col = 1;
        //    sbCheckboxList.Append("</tr></thead><tbody>");
        //    for (int i = 0; i < data.Count; i++)
        //    {
        //        if (col == 1)
        //            sbCheckboxList.Append("<tr>");

        //        displayLabel = data[i].Key.ToString();
        //        valueAsString = data[i].Value.ToString();
        //        inputName = name;
        //        inputId = name + "[" + i.ToString() + "]";

        //        if (selectedValues != null && selectedValues.Any(m => m.Equals(data[i].Value)))
        //            sbCheckboxList.Append("<td><input id='" + inputId + "' name='" + inputName + "' type='checkbox' value='" + valueAsString + "' checked='checked' />");
        //        else
        //            sbCheckboxList.Append("<td><input id='" + inputId + "' name='" + inputName + "' type='checkbox' value='" + valueAsString + "' />");

        //        sbCheckboxList.Append("&nbsp;<label class='check' for='" + inputId + "'>" + displayLabel + "</label></td>");

        //        if (col == columns)
        //        {
        //            sbCheckboxList.Append("</tr>");
        //            col = 1;
        //        }
        //        else
        //            col++;
        //    }

        //    if (col != 1)
        //    {
        //        // if entire row is not finished, we have to finish it.
        //        while (col <= columns)
        //        {
        //            sbCheckboxList.Append("<td></td>");
        //            col++;
        //        }
        //    }

        //    sbCheckboxList.Append("</tbody></table>");

        //    return MvcHtmlString.Create(sbCheckboxList.ToString());
        //}


        /// <summary>
        /// Generate checkbox list
        /// </summary>
        /// <param name="name">name using which you want to post selected values</param>
        /// <param name="data">data to bind checkbox list with</param>
        /// <param name="selectedValues">default selected values</param>
        /// <param name="columnwidth">width of one column of list e.g. 20px</param>
        /// <param name="columns">number of columns to be displayed</param>
        /// <param name="className">Class to add on input checbox list element</param>
        /// <returns></returns>
        public static IHtmlString CheckboxList<TKey, TValue>(this HtmlHelper htmlHelper, string name, List<KeyValuePair<TKey, TValue>> data, List<TValue> selectedValues, string columnwidth, int columns, string className = null)
        {
            if (data == null || data.Count == 0)
                return MvcHtmlString.Create(string.Empty);

            StringBuilder sbCheckboxList = new StringBuilder(string.Empty);
            string valueAsString, inputName, inputId, displayLabel;
            int col = 1;

            sbCheckboxList.Append("<table width='100%'><thead><tr>");
            while (col <= columns)
            {
                sbCheckboxList.Append("<th style='width:" + columnwidth + "'></th>");
                col++;
            }
            col = 1;
            sbCheckboxList.Append("</tr></thead><tbody>");
            for (int i = 0; i < data.Count; i++)
            {
                if (col == 1)
                    sbCheckboxList.Append("<tr valign='top'>");

                displayLabel = data[i].Key.ToString();
                valueAsString = data[i].Value.ToString();
                inputName = name;
                inputId = name + "[" + i.ToString() + "]";

                if (selectedValues != null && selectedValues.Any(m => m.Equals(data[i].Value)))
                    sbCheckboxList.Append("<td><input id='" + inputId + "' name='" + inputName + "' class='" + className + "' type='checkbox' value='" + valueAsString + "' checked='checked' />");
                else
                    sbCheckboxList.Append("<td><input id='" + inputId + "' name='" + inputName + "' class='" + className + "' type='checkbox' value='" + valueAsString + "' />");

                sbCheckboxList.Append("&nbsp;<label style='display: initial' class='check' for='" + inputId + "'>" + displayLabel + "</label></td>");

                if (col == columns)
                {
                    sbCheckboxList.Append("</tr>");
                    col = 1;
                }
                else
                    col++;
            }

            if (col != 1)
            {
                // if entire row is not finished, we have to finish it.
                while (col <= columns)
                {
                    sbCheckboxList.Append("<td></td>");
                    col++;
                }
            }

            sbCheckboxList.Append("</tbody></table>");

            return MvcHtmlString.Create(sbCheckboxList.ToString());
        }

    }
}
