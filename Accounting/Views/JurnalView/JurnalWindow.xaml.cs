using Accounting.ViewModels.Akun;
using Accounting.ViewModels.Jurnal;
using Microsoft.Extensions.DependencyInjection;
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

namespace Accounting.Views.JurnalView
{
    /// <summary>
    /// Interaction logic for Jurnal.xaml
    /// </summary>
    public partial class JurnalWindow : UserControl
    {
        public JurnalWindow()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider.GetRequiredService<JurnalViewModel>();

        }
    }
}
