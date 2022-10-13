using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DanTheMan827.Modulation.Views
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(bool))
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value != null && value.GetType() == typeof(string))
            {
                return !string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
         object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(bool))
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value != null && value.GetType() == typeof(string))
            {
                return !string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (parameter != null && parameter.GetType() == typeof(bool))
            {
                return (bool)parameter ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }
    }
}