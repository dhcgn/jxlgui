using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using jxlgui.converter;

namespace jxlgui.wpf.Converter;

public class JobStatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var converterValue = (Job.JobStateEnum) value;

        switch (converterValue)
        {
            case Job.JobStateEnum.Done: return Brushes.LightGreen;
            case Job.JobStateEnum.Working: return Brushes.Yellow;
            case Job.JobStateEnum.Pending: return Brushes.Beige;
            case Job.JobStateEnum.Error: return Brushes.OrangeRed;
            default:
                return Brushes.Green;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}