using Accounting.DTO;
using Accounting.ViewModels.AR;
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

namespace Accounting.Views.AR
{
    /// <summary>
    /// Interaction logic for CustomerListView.xaml
    /// </summary>
    public partial class CustomerListView : UserControl
    {
        internal CustomerListView()
        {
            InitializeComponent();

            this.DataContext = App.ServiceProvider.GetRequiredService<CustomerListViewModel>();

        }


        private void Vm_ShowDetailRequested(object? sender, ContactDTO dto)
        {
            if (dto == null) return;

            // Replace with navigation to a proper detail form as needed.
            var msg = $"Customer Id: {dto.Id}\nName: {dto.Name}\nAddress: {dto.Address}\nPhone: {dto.Phone}\nTelephone: {dto.Company}";
            MessageBox.Show(msg, "Customer Detail", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}
