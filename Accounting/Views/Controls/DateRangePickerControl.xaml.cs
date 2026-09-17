using Accounting.ViewModels.Controls;
using System.Windows.Controls;

namespace Accounting.Views.Controls
{
    /// <summary>
    /// Interaction logic for DateRangePickerControl.xaml
    /// </summary>
    public partial class DateRangePickerControl : UserControl
    {
        public DateRangePickerControl()
        {
            InitializeComponent();

            this.DataContext = new DateRangePickerViewModel();
        }

        public DateRangePickerViewModel ViewModel => this.DataContext as DateRangePickerViewModel;
    }
}
