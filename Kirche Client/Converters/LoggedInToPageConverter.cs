using Kirche_Client.Views;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kirche_Client.Converters
{
    class LoggedInToPageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return new TabControlView();
            else
                return new LoginView();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
