using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace OpenWrap.Windows.Framework
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const string trueText = "Yes";
            const string falseText = "No";

            if ((value != null) && (value.GetType() == typeof(bool)))
            {
                bool boolValue = (bool)value;

                return boolValue ? trueText : falseText;
            }

            return falseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            IEnumerable<string> trueValues = new[] { "yes", "y", "true" };
            return trueValues.ContainsNoCase(value.ToString());
        }
    }
}
