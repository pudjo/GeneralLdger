

namespace Accounting.Domain.Entities
{
    public class Pejabat
    {
        public int Id { get; set; }
        public int IdJabatan { get; set; }  
        public string Nama { get; set; }
        public int IDAnggota { get; set; }
        public string NamaJabatan { get; set; }

        public DateTime TanggalAktiv { get; set; }

    }
}
