using Accounting.ViewModels.Inventory;
using Accounting.Views.Inventory;
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
    /// Interaction logic for ProductListDialogWindow.xaml
    /// </summary>
    public partial class ProductListDialogWindow : Window
    {
        public ProductListDialogWindow(ProductListView view)
        {
            InitializeComponent();
            Content = view;

        }
        // Optionally expose the viewmodel for callers:
        public ProductListViewModel? ViewModel => (Content as ProductListView)?.DataContext as ProductListViewModel;
        // set host content and wire the hosted VM RequestClose event
        public void SetHostedView(System.Windows.FrameworkElement hosted)
        {
            Content = hosted;
            if (hosted?.DataContext is ProductListViewModel vm)
            {
                // make sure dialog is in selection mode
                vm.IsSelectionMode = true;

                void Vm_RequestClose(object? s, EventArgs e)
                {
                    vm.RequestClose -= Vm_RequestClose;

                    // set DialogResult true only when a product was selected
                    if (vm.SelectedProduct != null)
                        this.DialogResult = true;
                    else
                        this.DialogResult = false;
                }

                vm.RequestClose += Vm_RequestClose;
            }
        }


    }
}
