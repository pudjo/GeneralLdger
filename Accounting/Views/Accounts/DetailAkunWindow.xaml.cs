using Accounting.DTO;
using Accounting.ViewModels.Akun;
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

namespace Accounting.Views.Account
{
    /// <summary>
    /// Interaction logic for DetailAkunWindow.xaml
    /// </summary>
    public partial class DetailAkunWindow : Window
    {
   
        public DetailAkunWindow(AccountDTO data, bool isNew = false)
        {
            InitializeComponent();

            this.DataContext = new DetailAkunViewModel(data, isNew); 
            //this.DataContext = data;

        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Menutup modal dialog
        }

    }
}

