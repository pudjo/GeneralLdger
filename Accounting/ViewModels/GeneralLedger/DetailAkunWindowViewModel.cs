using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.Services.GeneralLedgerService;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Accounting.ViewModels.GeneralLedger
{
    public class DetailAkunWindowViewModel : BaseViewModel
    {
        private readonly CashFlowService _service;

        public DetailAkunWindowViewModel(CashFlowService service, CashFlowItemDTO item)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            CashFlowItem = item ?? new CashFlowItemDTO();
            // ensure Children collection not null
            if (CashFlowItem.Children == null) CashFlowItem.Children = new ObservableCollection<CashFlowItemDTO>(); 
            SelectedChild = null;
        }

        public CashFlowItemDTO CashFlowItem { get; set; }

        private CashFlowItemDTO _selectedChild;
        public CashFlowItemDTO SelectedChild
        {
            get => _selectedChild;
            set { _selectedChild = value; OnPropertyChanged(); }
        }

        // Events
        public Action RequestClose;
     
        // NEW: callback to request refresh on parent view
        public Action RequestRefresh;


        // Commands
        public ICommand AddChildCommand => new RelayCommand(() =>
        {
            var child = new CashFlowItemDTO
            {
                Code = "",
                Name = "",
                ParentCode = CashFlowItem.Code,
                ParentName = CashFlowItem.Name,
                GroupType = CashFlowItem.GroupType
            };
            CashFlowItem.Children.Add(child);
            SelectedChild = child;
            OnPropertyChanged(nameof(CashFlowItem));
        });

        public ICommand UpdateCommand => new RelayCommand(() =>
        {
            // In this simple implementation Update is a no-op because fields are two-way bound.
            // Keep this command for UI parity; could be used to trigger validation or UI state.
            MessageBox.Show("Perubahan sudah diterapkan ke bidang yang dipilih. Klik Simpan untuk menyimpan ke database.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        });

        public ICommand SaveCommand => new RelayCommand(async () =>
        {
            try
            {
                // Jika ada child terpilih, simpan child; else simpan utama
                if (SelectedChild != null)
                {
                    await SaveOneAsync(SelectedChild);
                }
                else
                {
                    await SaveOneAsync(CashFlowItem);
                }

                MessageBox.Show("Data berhasil disimpan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        public ICommand DeleteCommand => new RelayCommand(async () =>
        {
            if (SelectedChild == null)
            {
                MessageBox.Show("Pilih item anak yang akan dihapus.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var toDelete = SelectedChild;

            var result = MessageBox.Show($"Hapus item '{toDelete.Name}' ?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Try call delete on service if available
                var svcType = _service.GetType();
                var methodByDto = svcType.GetMethod("DeleteCashFlowItemAsync", new Type[] { typeof(CashFlowItemDTO) });
                var methodByCode = svcType.GetMethod("DeleteCashFlowItemAsync", new Type[] { typeof(string) });

                bool persisted = false;

                if (methodByDto != null)
                {
                    var task = (Task<bool>)methodByDto.Invoke(_service, new object[] { toDelete })!;
                    persisted = await task;
                }
                else if (methodByCode != null)
                {
                    var task = (Task<bool>)methodByCode.Invoke(_service, new object[] { toDelete.Code })!;
                    persisted = await task;
                }
                else
                {
                    // service does not implement delete; still remove from UI and inform user
                    persisted = false;
                }

                // Remove from UI collection
                var removed = CashFlowItem.Children.Remove(toDelete);
                SelectedChild = null;
                OnPropertyChanged(nameof(CashFlowItem));

                // Notify parent to refresh (whether or not persisted)
                RequestRefresh?.Invoke();

                if (!persisted)
                {
                    MessageBox.Show("Item dihapus dari tampilan. Implementasikan metode delete pada service/repository untuk menyimpan penghapusan ke database.",
                                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal melakukan delete di service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        public ICommand CloseCommand => new RelayCommand(() =>
        {
            RequestClose?.Invoke();
        });

        private async Task SaveOneAsync(CashFlowItemDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            await _service.SaveCashFlowItemAsync(dto);
            bool success = await _service.SaveCashFlowItemAsync(dto);
            if (success)
            {
                // Refresh TreeView secara total dari database
                var freshTree = await _service.GetTreeCashFlowItems();

            }
        }

    }
}
