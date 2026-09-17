using Accounting.DTO;
using Accounting.ViewModels.Inventory;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Accounting.Views.Inventory
{
    /// <summary>
    /// Interaction logic for JenisTreeView.xaml
    /// </summary>
    public partial class JenisTreeView : UserControl
    {
        public JenisTreeView()
        {
            InitializeComponent();
        }
     

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is JenisTreeViewModel vm)
            {
                vm.Selected = e.NewValue as JenisDTO;
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem tvi)
            {
                tvi.IsSelected = true;
            }
        }

        // Menu handlers: read the item from MenuItem.Tag and forward to VM commands
        private void MenuItem_AddChild_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = menu?.Tag as JenisDTO;
            if (DataContext is JenisTreeViewModel vm && item != null)
            {
                if (vm.AddChildCommand.CanExecute(item))
                    vm.AddChildCommand.Execute(item);
            }
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = menu?.Tag as JenisDTO;
            if (DataContext is JenisTreeViewModel vm && item != null)
            {
                if (vm.EditCommand.CanExecute(item))
                    vm.EditCommand.Execute(item);
            }
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = menu?.Tag as JenisDTO;
            if (DataContext is JenisTreeViewModel vm && item != null)
            {
                if (vm.DeleteCommand.CanExecute(item))
                    vm.DeleteCommand.Execute(item);
            }
        }
    }
}

