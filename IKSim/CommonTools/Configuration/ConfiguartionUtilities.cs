using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CommonTools
{
    /// <summary>
    /// Utilities class for all around configuration functions
    /// </summary>
    public static class ConfiguartionUtilities
    {
        /// <summary>
        /// The function changes hight of property grid rows
        /// </summary>
        /// <param name="pg">the property grid to set</param>
        /// <param name="height">the height of rows to set</param>
        public static void SetPropertyGridRowHeight(PropertyGrid pg, int height)
        {
            // get the grid view
            Control view = (Control)pg.GetType().GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pg);

            // set label width
            FieldInfo fi = view.GetType().GetField("cachedRowHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(view, 25);

            // refresh
            view.Invalidate();
        }

        /// <summary>
        /// the function uses reflection to apply defaut values to obj's properties
        /// </summary>
        /// <param name="obj">object to set default values to</param>
        public static void SetDefaultValues(object obj)
        {
            // search for default value in property attributes
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                var objAttrs = prop.GetCustomAttributes(typeof(ConfigurationPropertyAttribute),true);
                if (objAttrs.Length == 0)
                {
                    continue;
                }
                var attr = objAttrs[0] as ConfigurationPropertyAttribute;
                if (attr == null)
                {
                    continue;
                }
                // set the given default value
                prop.SetValue(obj, attr.DefaultValue,null);
            }
        }
    }
}
