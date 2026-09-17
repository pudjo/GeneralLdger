using Accounting.Domain;
using Accounting.ViewModels.UserVM;
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

namespace Accounting.Views.UserView
{
    /// <summary>
    /// Interaction logic for CreatePasswordView.xaml
    /// </summary>
    public partial class CreatePasswordView : Window
    {
        public CreatePasswordView()
        {
            InitializeComponent();
            
            this.DataContext = new CreatePasswordViewModel(AppSession.CurrentUser);
        }
    }
}
