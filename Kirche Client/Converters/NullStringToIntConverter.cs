using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kirche_Client.Converters
{
    class NullStringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int cur;
            if(!int.TryParse((value as string), out cur))
                return null;

            return value;
        }
    }
}
