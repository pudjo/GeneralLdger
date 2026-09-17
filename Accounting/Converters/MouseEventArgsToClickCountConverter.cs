using System;

using System.Globalization;

using System.Windows.Data;
using System.Windows.Input;

namespace Accounting.Converters
{
    internal class MouseEventArgsToClickCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MouseButtonEventArgs e)
            {
                return e.ClickCount;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
