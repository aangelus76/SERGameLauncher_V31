// Panel/Path/BoolToPathTypeConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace SERGamesLauncher_V31
{
    public class BoolToPathTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUrl)
            {
                return isUrl ? "URL" : "Local";
            }
            return "Local";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue.Equals("URL", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}