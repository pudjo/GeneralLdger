

namespace Accounting.Domain.Entities
{
    internal class CompanySetting 
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
        public string NoHP { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NPWP { get; set; } = string.Empty;

        public int Bentuk { set; get; }
        public int Jenis { set; get; }
        public string NoAkta { set; get; }
        public byte[] Logo { get; set; }
    }
}
