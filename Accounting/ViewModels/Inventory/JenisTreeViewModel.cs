using Accounting.DTO;
using Accounting.IRepositories.Inventory;
using Accounting.Services.Inventory;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting.ViewModels.Inventory
{
    internal class JenisTreeViewModel : BaseViewModel
    {
        private readonly IJenisService _service;
        private readonly IJenisQueryRepository _queryRepo;
        private readonly ILogger<JenisTreeViewModel> _logger;

        private ObservableCollection<JenisDTO> _treeItems = new();
        public ObservableCollection<JenisDTO> TreeItems
        {
            get => _treeItems;
            set => SetProperty(ref _treeItems, value);
        }

        private JenisDTO _selected;
        public JenisDTO Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand AddCommand { get; }
        public IRelayCommand<JenisDTO> EditCommand { get; }
        public IRelayCommand<JenisDTO> DeleteCommand { get; }

        public IRelayCommand<JenisDTO> AddChildCommand { get; } // new

        

        private async void OnAddChild(JenisDTO parent)
        {
            try
            {
                /*
                var newDto = new JenisDTO
                {
                    ParentID = parent?.ID
                };

                var vm = new JenisDetailViewModel(newDto, _service, _queryRepo, _logger);
                var win = new Accounting.Views.Inventory.JenisDetailWindow
                {
                    DataContext = vm,
                    Owner = Application.Current?.MainWindow
                };

                vm.RequestClose += async (s, saved) =>
                {
                    win.DialogResult = saved;
                    win.Close();
                    if (saved) await LoadAsync();
                };

                win.ShowDialog();
                */
                var isSaved = OpenDetailWindow(new JenisDTO { ParentID = parent?.ID });
                if (isSaved)
                {
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add child Jenis failed");
            }
        }
        public JenisTreeViewModel(IJenisService service, IJenisQueryRepository queryRepo, ILogger<JenisTreeViewModel> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _queryRepo = queryRepo ?? throw new ArgumentNullException(nameof(queryRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            AddChildCommand = new RelayCommand<JenisDTO>(OnAddChild);
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand<JenisDTO>(OnEdit);
            DeleteCommand = new RelayCommand<JenisDTO>(OnDelete);

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var all = await _queryRepo.GetAllAsync().ConfigureAwait(false);
                // build tree: roots = ParentID null
                var roots = all.Where(x => x.ParentID == null).ToList();
                foreach (var r in roots)
                    r.Children = GetChildren(all, r.ID);

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    TreeItems = new ObservableCollection<JenisDTO>(roots);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load Jenis tree failed");
            }
        }

        private System.Collections.Generic.List<JenisDTO> GetChildren(System.Collections.Generic.List<JenisDTO> all, int parentId)
        {
            var children = all.Where(x => x.ParentID == parentId).ToList();
            foreach (var c in children)
                c.Children = GetChildren(all, c.ID);
            return children;
        }

        private bool OpenDetailWindow(JenisDTO dto)
        {
            bool isSaved = false;
            try
            {
                var vm = new JenisDetailViewModel(dto, _service, _queryRepo, _logger);
                var win = new Accounting.Views.Inventory.JenisDetailWindow
                {
                    DataContext = vm,
                    Owner = Application.Current?.MainWindow
                };

                vm.RequestClose += (s, saved) =>
                {
                    isSaved = saved;
                    win.DialogResult = saved;
                    win.Close();
                };

                win.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Open detail window failed");
            }

            return isSaved;
        }
        private async void OnAdd()
        {
            var isSaved = OpenDetailWindow(new JenisDTO());
            if (isSaved)
            {
                await LoadAsync();
            }

            /*   var vm = new JenisDetailViewModel(new JenisDTO(), _service, _queryRepo, _logger);
            var win = new Accounting.Views.Inventory.JenisDetailWindow { DataContext = vm, Owner = Application.Current?.MainWindow };
           
           
            
            vm.RequestClose += async (s, saved) => 
            { win.Close(); if (saved) await LoadAsync(); };
            win.ShowDialog();*/
        }

        private async void OnEdit(JenisDTO dto)
        {

            if (dto == null) return;

            // Buat salinan DTO baru agar jika dibatalkan di form, data di treeview tidak ikut berubah
            var cloneDto = new JenisDTO
            {
                ID = dto.ID,
                Kode = dto.Kode,
                Nama = dto.Nama,
                ParentID = dto.ParentID,
                ParentNama = dto.ParentNama
            };

            var isSaved = OpenDetailWindow(cloneDto);
            if (isSaved)
            {
                await LoadAsync();
            }
            /*
            if (dto == null) return;
            var vm = new JenisDetailViewModel(dto, _service, _queryRepo, _logger);
            var win = new Accounting.Views.Inventory.JenisDetailWindow { DataContext = vm, Owner = Application.Current?.MainWindow };
            vm.RequestClose += async (s, saved) => { win.Close(); if (saved) await LoadAsync(); };
            win.ShowDialog();*/
        }

        private async void OnDelete(JenisDTO dto)
        {
            if (dto == null) return;
            // optional: confirm
            var ok = System.Windows.MessageBox.Show($"Hapus jenis '{dto.Nama}' ?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                await _service.DeleteAsync(dto.ID).ConfigureAwait(false);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Jenis failed");
            }
        }
    }

}
