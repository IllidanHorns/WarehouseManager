using System;
using System.Globalization;
using System.Windows.Data;

namespace WarehouseManager.Wpf.Convertor
{
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue.ToString(culture);
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && int.TryParse(stringValue, out int result))
                return result;
            return 0;
        }
    }
}

