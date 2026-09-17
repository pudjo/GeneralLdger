using Accounting.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.ViewModels.Controls
{
    public class DateRangePickerViewModel : BaseViewModel
    {
        private bool _isMonthlyMode = true;
        private int _selectedMonthIndex =   DateTime.Now.Month - 1; // ComboBox index dimulai dari 0
        private int _selectedYear = AppSession.SelectedYear;
        private DateTime _firstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        private DateTime _endDate = new DateTime(DateTime.Now.Year, 12, 31);
        private List<string> _monthList;



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ==================== PROPERTIES ====================

        /// <summary>
        /// Property untuk mode Monthly atau Daily
        /// </summary>
        public bool IsMonthlyMode
        {
            get => _isMonthlyMode;
            set
            {
                if (_isMonthlyMode != value)
                {
                    _isMonthlyMode = value;
                    OnPropertyChanged(); // Memicu UI untuk memperbarui Visibility StackPanel
                }
            }
        }
        /// <summary>
        /// Property untuk mode Daily (inverse dari IsMonthlyMode)
        /// </summary>
        public bool IsDailyMode => !IsMonthlyMode;

        /// <summary>
        /// Property untuk index bulan yang dipilih di ComboBox
        /// </summary>
        public int SelectedMonthIndex
        {
            get => _selectedMonthIndex;
            set
            {
                if (_selectedMonthIndex != value)
                {
                    _selectedMonthIndex = value;
                    OnPropertyChanged(nameof(SelectedMonthIndex));
                }
            }
        }

        /// <summary>
        /// Property untuk tahun yang dipilih
        /// </summary>
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged(nameof(SelectedYear));
                }
            }
        }

        /// <summary>
        /// Property untuk tanggal pertama (Daily mode)
        /// </summary>
        public DateTime FirstDate
        {
            get => _firstDate;
            set
            {
                if (_firstDate != value)
                {
                    _firstDate = value;
                    OnPropertyChanged(nameof(FirstDate));
                }
            }
        }

        /// <summary>
        /// Property untuk tanggal akhir (Daily mode)
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }

        /// <summary>
        /// Property untuk daftar nama bulan
        /// </summary>
        public List<string> MonthList
        {
            get => _monthList;
            set
            {
                if (_monthList != value)
                {
                    _monthList = value;
                    OnPropertyChanged(nameof(MonthList));
                }
            }
        }

        // ==================== CONSTRUCTOR ====================

        public DateRangePickerViewModel()
        {
            InitializeMonthList();
        }

        // ==================== PUBLIC METHODS ====================

        /// <summary>
        /// Mendapatkan tanggal pertama range yang dipilih
        /// Jika Monthly Mode: tanggal pertama bulan
        /// Jika Daily Mode: FirstDate yang dipilih
        /// </summary>
        public DateTime GetFirstDate()
        {
            if (IsMonthlyMode)
            {
                // Kembalikan tanggal pertama bulan yang dipilih
                // SelectedMonthIndex + 1 karena index dimulai dari 0 tapi month dimulai dari 1
                int month = SelectedMonthIndex + 1;

                return new DateTime(SelectedYear, month, 1);
            }
            else
            {
                // Kembalikan FirstDate dari Daily mode
                return FirstDate.Date; // .Date untuk menghilangkan time component
            }
        }

        /// <summary>
        /// Mendapatkan tanggal akhir range yang dipilih
        /// Jika Monthly Mode: tanggal akhir bulan
        /// Jika Daily Mode: EndDate yang dipilih
        /// </summary>
        public DateTime GetEndDate()
        {
            if (IsMonthlyMode)
            {
                // Kembalikan tanggal akhir bulan yang dipilih
                int year = SelectedYear;
                int month = SelectedMonthIndex + 1; // SelectedMonthIndex dimulai dari 0
                int lastDay = DateTime.DaysInMonth(year, month);
                return new DateTime(year, month, lastDay);
            }
            else
            {
                // Kembalikan EndDate dari Daily mode
                return EndDate.Date; // .Date untuk menghilangkan time component
            }
        }

        // ==================== PRIVATE METHODS ====================

        /// <summary>
        /// Inisialisasi list bulan-bulan dalam tahun
        /// </summary>
        private void InitializeMonthList()
        {
            MonthList = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                //hanya memancing 
                var date = new DateTime(DateTime.Now.Year, i, 1);
                MonthList.Add(date.ToString("MMMM")); // Contoh: "January 2025"
            }
        }

        public void Reset()
        {
            InitializeMonthList();

            _isMonthlyMode = false;
        _selectedMonthIndex = DateTime.Now.Month - 1; // ComboBox index dimulai dari 0
        _selectedYear = AppSession.SelectedYear;
        _firstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        _endDate = new DateTime(DateTime.Now.Year, 12, 31);
        OnPropertyChanged(nameof(IsMonthlyMode));
            OnPropertyChanged(nameof(FirstDate));
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(SelectedMonthIndex));
        }

    }
}
