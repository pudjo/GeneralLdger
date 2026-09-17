
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.ViewModels;
using System.Windows.Controls;

namespace Accounting.Views
{
    /// <summary>
    /// Interaction logic for JurnalImportView.xaml
    /// </summary>
    public partial class JurnalImportView : UserControl
    {
        public JurnalImportView()
        {
            InitializeComponent();
            this.DataContext = new JurnalImportViewModel();
        }
        private void dgJurnalHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgJurnalHeader.SelectedItem is JurnalDTO headerTerpilih)
            {
                // 1. Cari baris di Data Mentah yang NoBukti-nya sama
                var itemTujuan = dgDataMentah.Items.Cast<JurnalImport>()
                    .FirstOrDefault(x => x.NoBukti == headerTerpilih.RefNo);

                if (itemTujuan != null)
                {
                    // 2. Sorot barisnya (Selected)
                    dgDataMentah.SelectedItem = itemTujuan;

                    // 3. Otomatis geser scrollbar ke baris tersebut
                    dgDataMentah.ScrollIntoView(itemTujuan);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void dgDataMentah_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Pastikan statusnya adalah Commit (user selesai mengedit, bukan membatalkan/Cancel)
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Beri jeda sangat singkat agar WPF selesai menulis nilai baru dari Cell ke properti Object
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (this.DataContext is JurnalImportViewModel vm)
                    {
                        // Kita panggil fungsi refresh via method internal atau pemicu di ViewModel
                        vm.RefreshHeaderDetail();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
