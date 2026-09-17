using Accounting.DTO.AR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Accounting.Converters
{
    public class DetailsToProductNamesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<PenjualanDetailDTO> details)
            {
                var names = details.Select(d =>
                {
                    // PenjualanDetailDTO.ProductName is currently an int in DTO.
                    // If it represents a name in the future, this will still work when adjusted.
                    
                        return d.ProductName.ToString();
                    //return d.ProductId.ToString();
                });

                return string.Join(", ", names);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
