using Accounting.Views.Menu;
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

namespace Accounting.Views
{
    /// <summary>
    /// Interaction logic for UserControlMenuItem.xaml
    /// </summary>
  public partial class UserControlMenuItem :UserControl
  {
    public UserControlMenuItem(ItemMenu itemMenu)
    {
        InitializeComponent();

        ExpanderMenu.Visibility = itemMenu.SubItems == null ? Visibility.Collapsed : Visibility.Visible;
        ListViewItemMenu.Visibility = itemMenu.SubItems == null ? Visibility.Visible : Visibility.Collapsed;

        this.DataContext = itemMenu;
    }
   private void OnMenuItemClick(object sender, MouseButtonEventArgs e)
   {
            var item = sender as ListViewItem;
            if (item != null && item.DataContext is SubItem subItem)
            {
                // Jalankan command yang ada di SubItem
                if (subItem.OpenScreenCommand != null && subItem.OpenScreenCommand.CanExecute(subItem.Screen))
                {
                    subItem.OpenScreenCommand.Execute(subItem.Screen);
                }
            }
        }


    }
}
