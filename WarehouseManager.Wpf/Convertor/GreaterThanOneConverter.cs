using System;
using System.Globalization;
using System.Windows.Data;

namespace WarehouseManager.Wpf.Convertor
{
    public class GreaterThanOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем на null сначала
            if (value == null)
                return false;
            
            // Используем базовый тип int в pattern matching (не int?)
            // Это работает и для обычных int, и для boxed int? со значениями
            if (value is int intValue)
                return intValue > 1;
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

