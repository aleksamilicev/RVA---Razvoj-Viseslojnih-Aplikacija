// RVA.Client.Converters/BoolToSaveTextConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace RVA.Client.Converters
{
    public class BoolToSaveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditMode)
                return isEditMode ? "Update" : "Save";
            return "Save";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}