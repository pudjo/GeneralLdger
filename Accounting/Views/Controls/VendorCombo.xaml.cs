using Accounting.Services.ERP;
using Accounting.ViewModels.Controls;
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

namespace Accounting.Views.Controls
{
    /// <summary>
    /// Interaction logic for VendorCombo.xaml
    /// </summary>
    public partial class VendorCombo : UserControl
    {
        internal VendorComboViewModel _vm { get; } = new VendorComboViewModel();

        public static readonly DependencyProperty SelectedVendorIdProperty =
    DependencyProperty.Register(
        nameof(SelectedVendorId),
        typeof(int?),
        typeof(VendorCombo),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, // <--- SANGAT PENTING
            OnSelectedVendorIdChanged));
        /*
        public int? SelectedVendorId
        {
            get => (int?)GetValue(SelectedVendorIdProperty);
            set => SetValue(SelectedVendorIdProperty, value);
        }
        */
        public int? SelectedVendorId
        {
            get => (int?)GetValue(SelectedVendorIdProperty);
            set => SetValue(SelectedVendorIdProperty, value);
        }


        //================

        public static readonly DependencyProperty VendorServiceProperty =
            DependencyProperty.Register(nameof(VendorService),
                typeof(IVendorService),
                typeof(VendorCombo),
                new PropertyMetadata(null, OnVendorServiceChanged));

        public IVendorService? VendorService
        {
            get => (VendorService?)GetValue(VendorServiceProperty);
            set => SetValue(VendorServiceProperty, value);
        }




        public VendorCombo()
        {

            InitializeComponent();

            LayoutRoot.DataContext = _vm;
            // Merekam perubahan dari VM internal untuk dikirim ke DependencyProperty
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_vm.SelectedVendorId))
                {
                    if (SelectedVendorId != _vm.SelectedVendorId)
                    {
                        SelectedVendorId = _vm.SelectedVendorId;
                    }
                }
            };
            this.DataContextChanged += VendorCombo_DataContextChanged;

        }
        private void VendorCombo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryInitService();
        }

        private void TryInitService()
        {
            if (VendorService != null)
            {
                _vm.SetService(VendorService);
            }
        }
        /*
        private void VendorCombo_Loaded(object? sender, RoutedEventArgs e)
        {
            
            _vm.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == nameof(_vm.SelectedVendorId))
                {
                    if (SelectedVendorId != _vm.SelectedVendorId)
                        SelectedVendorId = _vm.SelectedVendorId;
                }
            };

            // Pastikan jika VendorService diset via DataContext atau Property, VM tetap memanggil SetService
            if (VendorService != null)
            {
                _vm.SetService(VendorService);
            }
        }
        */
        private static void OnVendorServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VendorCombo control && e.NewValue is IVendorService svc)
            {
                // Use the control's owned VM instance to set service
                control._vm.SetService(svc);
            }
        }

        /*private static void OnSelectedVendorIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VendorCombo control)
            {
                control._vm.SelectedVendorId = (int?)e.NewValue;
            }
        }*/
        private static void OnSelectedVendorIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (VendorCombo)d;
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
                SelectedVendorId = (int?)InnerComboBox.SelectedValue;
            }
            else
            {
                SelectedVendorId = null;
            }
        }
    }
}
