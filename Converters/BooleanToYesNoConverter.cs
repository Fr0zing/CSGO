using System;
using System.Globalization;
using System.Windows.Data;

namespace CSGOCheatDetector.Converters
{
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return boolean ? "Yes vac ban" : "No vac ban";
            }
            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string str && str.Equals("Yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}
