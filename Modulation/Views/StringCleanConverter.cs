using DanTheMan827.Modulation.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DanTheMan827.Modulation.Views
{
    public class StringCleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return HelperMethods.CleanString(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
