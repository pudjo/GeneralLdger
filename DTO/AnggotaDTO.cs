

internal class AnggotaDTO
{
    public int Id { get; set; }
    public string Nama { get; set; }
    public string NIK { get; set; }
    public int NamaAgama { get; set; } // Diasumsikan sebagai ID Agama
    public string Alamat { get; set; }
    public string NoTelepon { get; set; }
    public string JenisKelamin { get; set; }
    public string NamaJenisKelamin { get; set; }
}

internal static class AnggotaDataInitializer
{
    public static List<AnggotaDTO> GetSampleAnggota()
    {
        return new List<AnggotaDTO>
        {
            new AnggotaDTO
            {
                Id = 1,
                Nama = "Budi Santoso",
                NIK = "3171012345670001",
                NamaAgama = 1,
                Alamat = "Jl. Merdeka No. 10, Jakarta",
                NoTelepon = "081234567890",
                JenisKelamin = "L",
                NamaJenisKelamin = "Laki-laki"
            },
            new AnggotaDTO
            {
                Id = 2,
                Nama = "Siti Aminah",
                NIK = "3171012345670002",
                NamaAgama = 1,
                Alamat = "Jl. Mawar No. 5, Bandung",
                NoTelepon = "081345678901",
                JenisKelamin = "P",
                NamaJenisKelamin = "Perempuan"
            },
            new AnggotaDTO
            {
                Id = 3,
                Nama = "Michael Wijaya",
                NIK = "3171012345670003",
                NamaAgama = 2,
                Alamat = "Jl. Diponegoro No. 22, Surabaya",
                NoTelepon = "081456789012",
                JenisKelamin = "L",
                NamaJenisKelamin = "Laki-laki"
            },
            new AnggotaDTO
            {
                Id = 4,
                Nama = "Dewi Lestari",
                NIK = "3171012345670004",
                NamaAgama = 3,
                Alamat = "Jl. Gajah Mada No. 15, Yogyakarta",
                NoTelepon = "081567890123",
                JenisKelamin = "P",
                NamaJenisKelamin = "Perempuan"
            },
            new AnggotaDTO
            {
                Id = 5,
                Nama = "I Putu Gede",
                NIK = "3171012345670005",
                NamaAgama = 4,
                Alamat = "Jl. Sunset Road No. 88, Bali",
                NoTelepon = "081678901234",
                JenisKelamin = "L",
                NamaJenisKelamin = "Laki-laki"
            },
            new AnggotaDTO
            {
                Id = 6,
                Nama = "linawati",
                NIK = "3171012345670006",
                NamaAgama = 5,
                Alamat = "Jl. Pemuda No. 3, Semarang",
                NoTelepon = "081789012345",
                JenisKelamin = "P",
                NamaJenisKelamin = "Perempuan"
            },
            new AnggotaDTO
            {
                Id = 7,
                Nama = "Ahmad Fauzi",
                NIK = "3171012345670007",
                NamaAgama = 1,
                Alamat = "Jl. Sudirman No. 45, Medan",
                NoTelepon = "081890123456",
                JenisKelamin = "L",
                NamaJenisKelamin = "Laki-laki"
            },
            new AnggotaDTO
            {
                Id = 8,
                Nama = "Christina Natalia",
                NIK = "3171012345670008",
                NamaAgama = 2,
                Alamat = "Jl. Asia Afrika No. 12, Bandung",
                NoTelepon = "081901234567",
                JenisKelamin = "P",
                NamaJenisKelamin = "Perempuan"
            },
            new AnggotaDTO
            {
                Id = 9,
                Nama = "Rian Hidayat",
                NIK = "3171012345670009",
                NamaAgama = 1,
                Alamat = "Jl. Urip Sumoharjo No. 7, Makassar",
                NoTelepon = "082123456789",
                JenisKelamin = "L",
                NamaJenisKelamin = "Laki-laki"
            },
            new AnggotaDTO
            {
                Id = 10,
                Nama = "Siti Rahma",
                NIK = "3171012345670010",
                NamaAgama = 1,
                Alamat = "Jl. Ahmad Yani No. 100, Palembang",
                NoTelepon = "082234567890",
                JenisKelamin = "P",
                NamaJenisKelamin = "Perempuan"
            }
        };
    }
}