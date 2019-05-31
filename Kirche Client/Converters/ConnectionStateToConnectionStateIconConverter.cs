using Kirche_Client.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kirche_Client.Converters
{
    class ConnectionStateToConnectionStateIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "Connected")
                return "CheckCircle";

            else if ((string)value == "Connecting" || (string)value == "Reconnecting")
                return "DotsHorizontalCircle";

            else
                return "CloseCircle";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
