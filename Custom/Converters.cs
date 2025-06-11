// Custom/Converters.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SERGamesLauncher_V31
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1.0 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Paramètre pour déterminer la condition (ex: "gt1" pour greater than 1)
            string condition = parameter as string;

            if (condition == "gt1" && value is int count)
            {
                return count > 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}