using Accounting.ViewModels.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Accounting.Views.AR
{
    /// <summary>
    /// Interaction logic for CustomerEntryView.xaml
    /// </summary>
    public partial class CustomerEntryView : UserControl
    {
        public CustomerEntryView(CustomerEntryViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.RequestClose += Vm_RequestClose;
            vm.OperationCompleted += Vm_OperationCompleted;
        }

        private void Vm_OperationCompleted(object? sender, (bool Success, string Message) e)
        {
            if (!e.Success)
            {
                MessageBox.Show(e.Message, "Operation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(e.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Vm_RequestClose(object? sender, System.EventArgs e)
        {
            // If this Window hosts itself, close it
            var win = Window.GetWindow(this);
            if (win != null && win.Content == this)
            {
                win.Close();
                return;
            }

            // Otherwise nothing to do (host may handle)
        }


    }
}
