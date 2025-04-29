// Panel/Folder/FolderPermissionConverters.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Convertit un booléen en chaîne de caractères en fonction du paramètre
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string options)
                {
                    string[] parts = options.Split('|');
                    if (parts.Length >= 2)
                    {
                        return boolValue ? parts[0] : parts[1];
                    }
                }
                return boolValue ? "Vrai" : "Faux";
            }
            return "Inconnu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit un booléen en couleur en fonction du paramètre
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string options)
                {
                    string[] parts = options.Split('|');
                    if (parts.Length >= 2)
                    {
                        string colorName = boolValue ? parts[0] : parts[1];
                        return (SolidColorBrush)new BrushConverter().ConvertFromString(colorName);
                    }
                }
                return boolValue ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}