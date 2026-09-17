using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Accounting.Converters
{
    public class DetailsToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Binding used with RelativeSource AncestorType=DataGridRow passes DataGridRow as value
            if (value is DataGridRow row)
            {
                return (row.GetIndex() + 1).ToString();
            }

            // Fallback: try to find index if value is the item (not expected here)
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

}
