using Accounting.Domain;
using Accounting.Domain.Enum;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.ViewModels.AR;
using Accounting.ViewModels.ERP;
using Accounting.ViewModels.Inventory;
using Accounting.Views;
using Accounting.Views.AR;
using Accounting.Views.ERP;
using Accounting.Views.GeneralLedger;
using Accounting.Views.JurnalView;
using Accounting.Views.Koperasi;
using Accounting.Views.Menu;
using Accounting.Views.UserView;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;

namespace Accounting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ContentRendered += MainWindow_ContentRendered;

            var viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;


            // Buat perintah (Command) untuk mengganti screen
            var changeScreenCommand = new RelayCommand<object>(screen =>
            {
                viewModel.SelectedItem = screen;
            });
            MainRibbon.Background= (Brush)Application.Current.FindResource("MaterialDesignPaper");
            var menuDasboard = new List<SubItem>();
            menuDasboard.Add(new SubItem("Dashboard", new HomeView(), changeScreenCommand));
            var menuMaster = new List<SubItem>();
            var menuInventory = new List<SubItem>();

            menuMaster.Add(new SubItem("Perusahaan", new CompanySettingView(), changeScreenCommand));
            menuMaster.Add(new SubItem("Kode Akun", new AccountView(), changeScreenCommand));
            menuMaster.Add(new SubItem("Item Arus Kas", new CashFlowView(), changeScreenCommand));
            

            var menuKoperasi = new List<SubItem>();
            menuKoperasi.Add(new SubItem("Anggota", new AnggotaView(), changeScreenCommand));

            var menuAdministrasi = new List<SubItem>();
            menuAdministrasi.Add(new SubItem("User", new UserListView(), changeScreenCommand));

            var menuAkuntansi = new List<SubItem>();
            menuAkuntansi.Add(new SubItem("Input Saldo Awal", new InputSaldoAwalView(), changeScreenCommand));
            menuAkuntansi.Add(new SubItem("Import Excell Jurnal", new JurnalImportView(), changeScreenCommand));
            menuAkuntansi.Add(new SubItem("Input Jurnal", new JurnalWindow(), changeScreenCommand));

            var menuLaporan = new List<SubItem>();
            menuLaporan.Add(new SubItem("Buku Besar", new GeneralLedgerView(), changeScreenCommand));
            menuLaporan.Add(new SubItem("Neraca", new LaporanView(1), changeScreenCommand));
            menuLaporan.Add(new SubItem("Laba Rugi", new LabaRugi(), changeScreenCommand));

            var menuAP = new List<SubItem>();
            var menuAR = new List<SubItem>();

            var penjualanVM = App.ServiceProvider.GetRequiredService<Accounting.ViewModels.AR.ListPenjualanViewModel>();
            var penjualanView = new Accounting.Views.AR.ListPenjualanView(penjualanVM);

            menuAR.Add(new SubItem("Daftar Customer", new CustomerListView(), changeScreenCommand));
            menuAR.Add(new SubItem("Penjualan", penjualanView, changeScreenCommand));

            try
            {
                var factory = App.ServiceProvider.GetRequiredService<Func<ContactType, ContactListViewModel>>();

                // Inisialisasi Vendor (AP)
                if (factory != null)
                {
                    var vendorVm = factory(ContactType.Vendor);
                    var vendorView = new ContactListView { DataContext = vendorVm };
                    menuAP.Add(new SubItem("Daftar Vendor", vendorView, changeScreenCommand));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Menu AP: {ex.Message}");
            }

            var productVm = App.ServiceProvider.GetRequiredService<Accounting.ViewModels.Inventory.ProductListViewModel>();
            var productView = App.ServiceProvider.GetRequiredService<Accounting.Views.Inventory.ProductListView>();
            menuInventory.Add(new SubItem("Daftar Produk", productView, changeScreenCommand));

            var kategoryVm = App.ServiceProvider.GetRequiredService<KategoriListViewModel>();
            var kategoryView = new Accounting.Views.Inventory.KategoriListView { DataContext = kategoryVm };

            var jenisVm = App.ServiceProvider.GetRequiredService<JenisTreeViewModel>();
            var jenisView = new Accounting.Views.Inventory.JenisTreeView { DataContext = jenisVm };

            var subkategoryVm = App.ServiceProvider.GetRequiredService<SubKategoriListViewModel>();
            var subkategoryView = new Accounting.Views.Inventory.SubKategoriListView { DataContext = subkategoryVm };

            var priceListVm = App.ServiceProvider.GetRequiredService<ProductPriceViewModel>();
            var priceListView = new Accounting.Views.Inventory.ProductPriceListView { DataContext = priceListVm };

            //menuInventory.Add(new SubItem("Daftar Kategory", kategoryView, changeScreenCommand));
            //menuInventory.Add(new SubItem("Daftar Sub Kategory", subkategoryView, changeScreenCommand));
            menuInventory.Add(new SubItem("Jenis Barang", jenisView, changeScreenCommand));
            menuInventory.Add(new SubItem("Daftar Produk", productView, changeScreenCommand));
            menuInventory.Add(new SubItem("Setting Harga Jual", priceListView, changeScreenCommand));

            // choose simple default icons (replace as needed)
            var item1 = new ItemMenu("Dashboard", menuDasboard, PackIconKind.ViewDashboard);
            var itemMaster = new ItemMenu("Master", menuMaster, PackIconKind.Database);
            var itemAdm = new ItemMenu("Administrasi", menuAdministrasi, PackIconKind.Account);
            var itemInventory = new ItemMenu("Inventory", menuInventory, PackIconKind.Cube);
            var itemAkuntansi = new ItemMenu("Akuntansi", menuAkuntansi, PackIconKind.CurrencyUsd);
            var itemLaporan = new ItemMenu("Laporan", menuLaporan, PackIconKind.FileDocument);
            var itemAKoperasi = new ItemMenu("Koperasi", menuKoperasi, PackIconKind.AccountMultiple);
            var itemAP = new ItemMenu("A P ", menuAP, PackIconKind.Truck);
            var itemAR = new ItemMenu("A R ", menuAR, PackIconKind.Sale);

            // Buat Ribbon tabs dari ItemMenu
            void AddRibbonTab(ItemMenu item)
            {
                var tab = new RibbonTab { Header = item.Header ?? string.Empty };
                var group = new RibbonGroup { Header = item.Header ?? string.Empty };

                void ProcessMenuSubItems(IEnumerable<SubItem> subItems)
                {
                    foreach (var s in subItems)
                    {
                        // Gunakan Button standar WPF agar isi kustom (StackPanel) dirender sempurna
                        var btn = new System.Windows.Controls.Button
                        {
                            Width = 90,
                            Height = 65,
                            Margin = new Thickness(4, 2, 4, 2),
                            Padding = new Thickness(4),
                            ToolTip = s.Name,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Transparent
                        };

                        // Susun ikon di atas, teks di bawah (Vertical) agar rapi layaknya Ribbon modern
                        var contentPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        var icon = new PackIcon
                        {
                            Kind = item.Icon,
                            Width = 24,
                            Height = 24,
                            Margin = new Thickness(0, 0, 0, 4),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = (Brush)Application.Current.FindResource("MaterialDesignBody")
                        };

                        var text = new System.Windows.Controls.TextBlock
                        {
                            Text = s.Name,
                            FontSize = 12,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = (Brush)Application.Current.FindResource("MaterialDesignBody")
                        };

                        contentPanel.Children.Add(icon);
                        contentPanel.Children.Add(text);
                        btn.Content = contentPanel;

                        btn.Click += (sender, e) =>
                        {
                            if (s.OpenScreenCommand != null && s.OpenScreenCommand.CanExecute(s.Screen))
                                s.OpenScreenCommand.Execute(s.Screen);
                            else
                                viewModel.SelectedItem = s.Screen;
                        };

                        group.Items.Add(btn);
                    }
                }

                if (item.SubItems != null)
                {
                    ProcessMenuSubItems(item.SubItems);
                }
                else if (item.Screen != null)
                {
                    // Jika menu utama berdiri sendiri tanpa sub-item
                    var dummySub = new SubItem(item.Header, item.Screen, null);
                    ProcessMenuSubItems(new[] { dummySub });
                }

                tab.Items.Add(group);
                MainRibbon.Items.Add(tab);
            }
            //AddRibbonTab(item1);
            AddRibbonTab(itemAdm);
            AddRibbonTab(itemMaster);
           // AddRibbonTab(itemInventory);
           // AddRibbonTab(itemAP);
            //AddRibbonTab(itemAR);
            AddRibbonTab(itemAkuntansi);
            if (AppSession.CurrentUser.UserID.Trim().ToUpper() == "KOPERASI")
            {
                AddRibbonTab(itemAKoperasi);
            }
            AddRibbonTab(itemLaporan);

            // ensure first tab selected and layout refreshed
            if (MainRibbon.Items.Count > 0)
            {
                MainRibbon.SelectedIndex = 0;
            }
            MainRibbon.UpdateLayout();
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            var popup = this.FindName("TutorialPopup") as Popup;
            if (popup != null)
            {
                popup.IsOpen = true;
            }
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("TutorialPopup") as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }

    }
}