using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace jxlgui.wpf.Converter
{
    public class VisiblityInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueBool = (bool) value;
            return valueBool ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}