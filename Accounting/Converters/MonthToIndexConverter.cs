using System;
using System.Globalization;
using System.Windows.Data;

namespace Accounting.Converters
{
    public class MonthToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int month && month >= 1 && month <= 12)
            {
                return month - 1; // Convert month (1-12) ke index (0-11)
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index && index >= 0 && index < 12)
            {
                return index + 1; // Convert index (0-11) ke month (1-12)
            }
            return 1;
        }
    }
}