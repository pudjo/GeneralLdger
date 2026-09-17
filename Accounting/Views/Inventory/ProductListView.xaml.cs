
using Accounting.ViewModels.Inventory;
using System.Windows;
using System.Windows.Controls;

namespace Accounting.Views.Inventory
{ 
    /// <summary>
    /// Interaction logic for ProductListView.xaml
    /// </summary>
    public partial class ProductListView : UserControl
    {
        public ProductListView(ProductListViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            // When VM requests close, close window. Caller can read vm.SelectedProduct after dialog ends.
          //  vm.RequestClose += Vm_RequestClose;
        }
        
        /*   private void Vm_RequestClose(object? sender, EventArgs e)
           {
               // If selection made, return true so callers using ShowDialog can check DialogResult
               if (DataContext is ProductListViewModel vm && vm.SelectedProduct != null)
                   this.DialogResult = true;
               else
                   this.DialogResult = false;

               // Close the window (DialogResult setter closes window automatically)
               // If DialogResult set above, window closes; otherwise call Close to be safe
               if (!this.IsActive) return;
               if (!this.DialogResult.HasValue)
                   this.Close();
           }*/
    }
}
