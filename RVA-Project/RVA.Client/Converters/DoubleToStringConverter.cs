using System;
using System.Globalization;
using System.Windows.Data;

namespace RVA.Client.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return doubleValue.ToString("F6", CultureInfo.InvariantCulture);
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return result;
            return 0.0;
        }
    }
}