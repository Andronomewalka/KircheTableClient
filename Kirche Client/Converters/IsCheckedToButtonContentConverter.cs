using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kirche_Client.Converters
{
    class IsCheckedToButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Save changes" : "Start editing";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
