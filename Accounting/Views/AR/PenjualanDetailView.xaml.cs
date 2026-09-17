using Accounting.ViewModels.AR;
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
using System.Windows.Threading;

namespace Accounting.Views.AR
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PenjualanDetailView : Window
    {
        public PenjualanDetailView(PenjualanDetailViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.RequestClose += Vm_RequestClose;
            vm.OperationCompleted += Vm_OperationCompleted;
        }

        private void Vm_OperationCompleted(object? sender, (bool Success, string Message) e)
        {
            if (!e.Success)
            {
                MessageBox.Show(e.Message, "Operation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(e.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Vm_RequestClose(object? sender, EventArgs e)
        {
            if (this.IsVisible) this.Close();
        }

        // show row number in header if desired (alternative uses converter)
        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }


        // When a cell enters edit mode, select all text and attach handlers
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            // Try to get the editing TextBox
            if (e.EditingElement is TextBox tb)
            {
                tb.SelectAll();
                tb.Focus();

                // numeric validation for Quantity column
                if (e.Column.Header?.ToString() == "Jumlah")
                {
                    tb.PreviewTextInput -= Tb_PreviewTextInput;
                    tb.PreviewTextInput += Tb_PreviewTextInput;
                    tb.KeyDown -= Tb_KeyDown;
                    tb.KeyDown += Tb_KeyDown;
                }
                else
                {
                    tb.PreviewTextInput -= Tb_PreviewTextInput;
                    tb.KeyDown -= Tb_KeyDown;
                }
            }
            else
            {
                // If editing element is a ContentPresenter (e.g. ProductCombo), we still want to focus the inner control.
                // Delay to let template apply, then try to focus inner textbox if exists.
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    var cell = e.EditingElement;
                    var txt = FindVisualChild<TextBox>(cell);
                    if (txt != null)
                    {
                        txt.SelectAll();
                        txt.Focus();

                        if (e.Column.Header?.ToString() == "Jumlah")
                        {
                            txt.PreviewTextInput -= Tb_PreviewTextInput;
                            txt.PreviewTextInput += Tb_PreviewTextInput;
                            txt.KeyDown -= Tb_KeyDown;
                            txt.KeyDown += Tb_KeyDown;
                        }
                    }
                }));
            }
        }

        // After a product cell edit ends, move focus to Harga Jual cell and begin edit
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // If we just edited Product column, move to Harga Jual cell
            if (e.Column.Header?.ToString() == "Product")
            {
                // schedule after commit
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var dg = dgDetails;
                    // column index of Harga Jual — find by header
                    var targetCol = dg.Columns.FirstOrDefault(c => c.Header?.ToString() == "Jumlah");
                    if (targetCol != null)
                    {
                        dg.CurrentCell = new DataGridCellInfo(e.Row.Item, targetCol);
                        dg.BeginEdit();
                    }
                }), DispatcherPriority.Background);
            }
        }

        // Validate numeric input (allow digits and decimal separator)
        private void Tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var ch = e.Text;
            // allow digits and one dot or comma
            if (!char.IsDigit(ch, 0) && ch != "." && ch != ",")
            {
                e.Handled = true;
                return;
            }

            // optional: prevent multiple decimal separators
            var tb = sender as TextBox;
            if (tb != null)
            {
                var text = tb.Text;
                if ((ch == "." || ch == ",") && (text.Contains(".") || text.Contains(",")))
                {
                    e.Handled = true;
                }
            }
        }

        // Handle Enter on Quantity: commit edit and add new line
        private void Tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var dg = dgDetails;
                // commit current edit
                dg.CommitEdit(DataGridEditingUnit.Cell, true);
                dg.CommitEdit(DataGridEditingUnit.Row, true);

                // Add a new detail line via VM command
                if (DataContext is PenjualanDetailViewModel vm)
                {
                    vm.AddDetailLineCommand.Execute(null);

                    // select last row and begin editing its Product cell
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var lastIndex = dg.Items.Count - 1;
                        if (lastIndex >= 0)
                        {
                            dg.SelectedIndex = lastIndex;
                            dg.ScrollIntoView(dg.Items[lastIndex]);
                            var prodCol = dg.Columns.FirstOrDefault(c => c.Header?.ToString() == "Product");
                            if (prodCol != null)
                            {
                                dg.CurrentCell = new DataGridCellInfo(dg.Items[lastIndex], prodCol);
                                dg.BeginEdit();
                            }
                        }
                    }), DispatcherPriority.Background);
                }

                e.Handled = true;
            }
        }

        // helper to find TextBox in cell template
        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T found) return found;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }


    }
}
