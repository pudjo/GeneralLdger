using Accounting.Services.ERP;
using Accounting.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Accounting.Views.Controls
{
    /// <summary>
    /// Interaction logic for CustomerCombo.xaml
    /// </summary>
    public partial class CustomerCombo : UserControl
    {
  


        
        internal CustomerComboViewModel _vm { get; } = new CustomerComboViewModel();
        
       /* public static readonly DependencyProperty SelectedCustomerIdProperty =
            DependencyProperty.Register(nameof(SelectedCustomerId), 
                typeof(int?),
                typeof(CustomerCombo),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedCustomerIdChanged));
       */
        public static readonly DependencyProperty SelectedCustomerIdProperty =
    DependencyProperty.Register(
        nameof(SelectedCustomerId),
        typeof(int?),
        typeof(CustomerCombo),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, // <--- SANGAT PENTING
            OnSelectedCustomerIdChanged));
        /*
        public int? SelectedCustomerId
        {
            get => (int?)GetValue(SelectedCustomerIdProperty);
            set => SetValue(SelectedCustomerIdProperty, value);
        }
        */
        public int? SelectedCustomerId
        {
            get => (int?)GetValue(SelectedCustomerIdProperty);
            set => SetValue(SelectedCustomerIdProperty, value);
        }


        //================

        public static readonly DependencyProperty CustomerServiceProperty =
            DependencyProperty.Register(nameof(CustomerService),
                typeof(ICustomerService), 
                typeof(CustomerCombo),
                new PropertyMetadata(null, OnCustomerServiceChanged));

        public ICustomerService? CustomerService
        {
            get => (ICustomerService?)GetValue(CustomerServiceProperty);
            set => SetValue(CustomerServiceProperty, value);
        }

      


        public CustomerCombo()
        {

            InitializeComponent();

            LayoutRoot.DataContext = _vm;
            // Merekam perubahan dari VM internal untuk dikirim ke DependencyProperty
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_vm.SelectedCustomerId))
                {
                    if (SelectedCustomerId != _vm.SelectedCustomerId)
                    {
                        SelectedCustomerId = _vm.SelectedCustomerId;
                    }
                }
            };
            this.DataContextChanged += CustomerCombo_DataContextChanged;

        }
        private void CustomerCombo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryInitService();
        }

        private void TryInitService()
        {
            if (CustomerService != null)
            {
                _vm.SetService(CustomerService);
            }
        }
        /*
        private void CustomerCombo_Loaded(object? sender, RoutedEventArgs e)
        {
            
            _vm.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == nameof(_vm.SelectedCustomerId))
                {
                    if (SelectedCustomerId != _vm.SelectedCustomerId)
                        SelectedCustomerId = _vm.SelectedCustomerId;
                }
            };

            // Pastikan jika CustomerService diset via DataContext atau Property, VM tetap memanggil SetService
            if (CustomerService != null)
            {
                _vm.SetService(CustomerService);
            }
        }
        */
        private static void OnCustomerServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomerCombo control && e.NewValue is ICustomerService svc)
            {
                // Use the control's owned VM instance to set service
                control._vm.SetService(svc);
            }
        }

        /*private static void OnSelectedCustomerIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomerCombo control)
            {
                control._vm.SelectedCustomerId = (int?)e.NewValue;
            }
        }*/
        private static void OnSelectedCustomerIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomerCombo)d;
            // Sinkronisasi pilihan ComboBox internal jika nilai dari luar (ViewModel) berubah
            if (control.InnerComboBox != null)
            {
                control.InnerComboBox.SelectedValue = e.NewValue;
            }
        }
        private void InnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update DependencyProperty agar ViewModel mendapat nilai baru
            if (InnerComboBox.SelectedValue != null)
            {
                SelectedCustomerId = (int?)InnerComboBox.SelectedValue;
            }
            else
            {
                SelectedCustomerId = null;
            }
        }

    }
}
