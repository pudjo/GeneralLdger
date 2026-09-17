using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DTO
{
         
    internal class JurnalDetailDTO : BaseViewModel
    {
        public int Id { get; set; }
        public int IdHeader { get; set; }
        public string NoBukti { get; set; }

        private string _noAkun;
        public string NoAkun
        {
            get => _noAkun;
            set => SetProperty(ref _noAkun, value); // Ini yang memicu UI update
        }

        private string _namaAkun;
        public string NamaAkun
        {
            get => _namaAkun;
            set => SetProperty(ref _namaAkun, value);
        }
        
        
        public decimal Debet { get; set; }
        public decimal Kredit { get; set; }

       
    }
}
