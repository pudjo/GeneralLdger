using Accounting.DTO;
using Accounting.Services.Inventory;
using Accounting.ViewModels.Controls;
using Accounting.Views.AR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Accounting.Views.Controls
{
    /// <summary>
    /// Interaction logic for ProductCombo.xaml
    /// </summary>
    public partial class ProductCombo : UserControl
    {
        private readonly ProductComboViewModel _vm = new ProductComboViewModel();

        // editable textbox inside ComboBox (PART_EditableTextBox)
        private TextBox? _editableTextBox;
        // flag that indicates the last input was user typing (text input/paste)
        private bool _userTyping;

        public static readonly DependencyProperty SelectedProductIdProperty =
            DependencyProperty.Register(nameof(SelectedProductId),
                typeof(int?),
                typeof(ProductCombo),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedProductIdChanged));

        public int? SelectedProductId
        {
            get => (int?)GetValue(SelectedProductIdProperty);
            set => SetValue(SelectedProductIdProperty, value);
        }

        public static readonly DependencyProperty ProductServiceProperty =
            DependencyProperty.Register(nameof(ProductService),
                typeof(IProductService),
                typeof(ProductCombo),
                new PropertyMetadata(null, OnProductServiceChanged));

        public IProductService? ProductService
        {
            get => (IProductService?)GetValue(ProductServiceProperty);
            set => SetValue(ProductServiceProperty, value);
        }

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(ProductCombo),
                new PropertyMetadata(null));

        public ICommand? SearchCommand
        {
            get => (ICommand?)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static readonly DependencyProperty SearchCommandParameterProperty =
            DependencyProperty.Register(nameof(SearchCommandParameter), typeof(object), typeof(ProductCombo),
                new PropertyMetadata(null));

        public object? SearchCommandParameter
        {
            get => GetValue(SearchCommandParameterProperty);
            set => SetValue(SearchCommandParameterProperty, value);
        }

        public ProductCombo()
        {
            InitializeComponent();

            LayoutRoot.DataContext = _vm;

            // VM -> DP sync
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_vm.SelectedProductId))
                {
                    if (SelectedProductId != _vm.SelectedProductId)
                        SelectedProductId = _vm.SelectedProductId;
                }
            };

            this.DataContextChanged += PtoductCombo_DataContextChanged;

            // hook to locate the inner editable textbox when ComboBox template has been applied
            cmbProduct.Loaded += CmbProduct_Loaded;
            cmbProduct.SelectionChanged += CmbProduct_SelectionChanged;
        }

        private void CmbProduct_Loaded(object? sender, RoutedEventArgs e)
        {
            // try find the editable text box part
            _editableTextBox = cmbProduct.Template.FindName("PART_EditableTextBox", cmbProduct) as TextBox;
            if (_editableTextBox != null)
            {
                _editableTextBox.TextChanged -= OnComboTextChanged;
                _editableTextBox.TextChanged += OnComboTextChanged;

                _editableTextBox.PreviewTextInput -= EditableTextBox_PreviewTextInput;
                _editableTextBox.PreviewTextInput += EditableTextBox_PreviewTextInput;

                _editableTextBox.PreviewKeyDown -= EditableTextBox_PreviewKeyDown;
                _editableTextBox.PreviewKeyDown += EditableTextBox_PreviewKeyDown;

                DataObject.RemovePastingHandler(_editableTextBox, OnEditableTextBoxPasting);
                DataObject.AddPastingHandler(_editableTextBox, OnEditableTextBoxPasting);
            }
            else
            {
                // fallback: attach TextChanged to ComboBox if editable textbox not found
                cmbProduct.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnComboTextChanged));
            }
        }

        private void EditableTextBox_PreviewTextInput(object? sender, TextCompositionEventArgs e)
        {
            // user typed text (character input) -> enable filtering
            _userTyping = true;
        }

        private void OnEditableTextBoxPasting(object? sender, DataObjectPastingEventArgs e)
        {
            // treat paste as user typing
            _userTyping = true;
        }

        private void EditableTextBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            // Navigation keys -> not user typing; do not filter when text changes as a result
            switch (e.Key)
            {
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.PageDown:
                case Key.PageUp:
                case Key.Tab:
                case Key.Enter:
                case Key.Escape:
                    _userTyping = false;
                    break;

                case Key.V:
                    // Ctrl+V paste -> treat as typing
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        _userTyping = true;
                    break;

                default:
                    // do not change flag here; PreviewTextInput will set _userTyping = true for character input
                    break;
            }
        }

        private void CmbProduct_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // When selection is changed via keyboard/mouse selection, ensure VM and DP reflect it
            if (cmbProduct.SelectedValue is int id)
            {
                _vm.SelectedProductId = id;
                SelectedProductId = id;
            }
            else
            {
                _vm.SelectedProductId = null;
                SelectedProductId = null;
            }

            // selection from dropdown should not be treated as user typing for subsequent TextChanged
            _userTyping = false;
        }

        private void OnComboTextChanged(object sender, TextChangedEventArgs e)
        {
            // Only apply filter when user actually typed or pasted text.
            if (!_userTyping) return;

            var tb = sender as TextBox;
            var text = tb?.Text ?? string.Empty;

            ICollectionView view = CollectionViewSource.GetDefaultView(cmbProduct.ItemsSource);
            if (view == null) return;

            if (string.IsNullOrWhiteSpace(text))
            {
                view.Filter = null; // show all
            }
            else
            {
                view.Filter = item =>
                {
                    if (item is ProductDTO p)
                    {
                        var name = p.Name ?? string.Empty;
                        var code = p.Code ?? string.Empty;
                        return name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
                               || code.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    // if item is KeyValuePair<int,string> fallback
                    var prop = item.GetType().GetProperty("Value");
                    if (prop != null)
                    {
                        var val = prop.GetValue(item, null)?.ToString();
                        return val != null && val.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    return item?.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                };
            }

            if (!cmbProduct.IsDropDownOpen)
                cmbProduct.IsDropDownOpen = true;
        }

        private void PtoductCombo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryInitService();
        }

        private void TryInitService()
        {
            if (ProductService != null)
            {
                _vm.SetService(ProductService);
            }
        }

        private static void OnProductServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProductCombo control && e.NewValue is IProductService svc)
            {
                control._vm.SetService(svc);
            }
            else if (d is ProductCombo ctrl && e.NewValue == null)
            {
                ctrl._vm.SetService(null);
            }
        }

        private static void OnSelectedProductIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProductCombo control)
            {
                control._vm.SelectedProductId = (int?)e.NewValue;
                control.SelectedProductId = control._vm.SelectedProductId;
            }
        }

        
        private void SetSelectedProductIdFromVm(int id)
        {
            void apply()
            {
                _vm.SelectedProductId = id;
                SelectedProductId = id;

                if (this.DataContext is Accounting.DTO.AR.PenjualanDetailDTO row)
                {
                    row.ProductId = id;
                    var found = _vm.Products.FirstOrDefault(p => p.Id == id);
                    if (found != null)
                    {
                        row.ProductName = found.Name ?? string.Empty;
                        row.SellingPrice = found.CurrentSellingPrice;
                    }
                }
            }

            if (Application.Current?.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(apply);
            else
                apply();
        }
        private void BtnJenis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sp = App.ServiceProvider ?? throw new InvalidOperationException("ServiceProvider not available");

                // resolve a JenisTreeViewModel from DI if available, otherwise construct
                var jenisVm = sp.GetService(typeof(Accounting.ViewModels.Inventory.JenisTreeViewModel)) as Accounting.ViewModels.Inventory.JenisTreeViewModel
                              ?? ActivatorUtilities.CreateInstance<Accounting.ViewModels.Inventory.JenisTreeViewModel>(sp);

                var picker = ActivatorUtilities.CreateInstance(sp, typeof(Accounting.Views.Inventory.JenisPickerWindow)) as Window
                             ?? new Accounting.Views.Inventory.JenisPickerWindow();

                picker.DataContext = jenisVm;
                picker.Owner = Window.GetWindow(this) ?? Application.Current?.MainWindow;

                var result = picker.ShowDialog();
                if (result == true && jenisVm.Selected != null)
                {
                    var jenisId = jenisVm.Selected.ID;
                    // tell VM to filter by jenis
                    _vm.SetJenis(jenisId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Open Jenis picker failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

            }
}
