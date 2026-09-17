using System.Globalization;
using System.Windows.Data;

namespace Accounting.Converters
{
    public class InverseBooleanConverter: IValueConverter
    {
       
        // Mengubah True jadi False, dan sebaliknya
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }

        // Kebalikan saat data dikirim balik dari UI ke ViewModel
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }
      }
}
