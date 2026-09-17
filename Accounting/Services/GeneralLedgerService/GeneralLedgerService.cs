using Accounting.Domain;
using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Services.Accounts;
using ControlzEx.Standard;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Accounting.Services.GeneralLedgerService
{
    internal class GeneralLedgerService 
    {
        private string error;
        private IGeneralLedgerQueryRepository generalLedgerQueryRepository;
        private IAccountQueryRepository accountQueryRepository;
        private IAccountService accountService;
        


        public GeneralLedgerService(IGeneralLedgerQueryRepository _generalLedgerQueryRepository,
            IAccountService _accountService 
            )
        {
            generalLedgerQueryRepository = _generalLedgerQueryRepository;
            accountService = _accountService;

        }
        public string Error
        {
            set { error = value; }
            get { return error; }
        }
        public async Task<ObservableCollection<GeneralLedgerDTO>> GetGeneralLedgers(DateTime start, DateTime end, string accountCode = "")
        {
            try
            {

                string trimmedAccountCode= accountCode.Replace(".","");
                if (!string.IsNullOrEmpty(accountCode)) {
                    trimmedAccountCode = trimmedAccountCode.Trim().TrimEnd('0');
                }


                

                ObservableCollection < GeneralLedgerDTO >gl = await generalLedgerQueryRepository.GetGeneralLedgers(start, end, trimmedAccountCode);
                List<AccountDTO> lstAccount = await accountService.GetAccountsDTOAsync();

                var accountDict = lstAccount
                                .Where(a => a.Id != null)
                                .ToDictionary(a => a.Id.Trim().ToLower(), a => a.Name);

                // 2. Saat melakukan pencarian di dalam loop, terapkan juga Trim() dan ToLower()
                foreach (var ledger in gl)
                {
                    // Lakukan Trim dan ToLower pada AccountCode yang dicari
                    string searchKey = ledger.AccountCode?.Trim().ToLower();

                    if (searchKey != null && accountDict.TryGetValue(searchKey, out string accountName))
                    {
                        ledger.AccountName = accountName;
                    }
                    else
                    {
                        ledger.AccountName = "Akun Tidak ada "; // Penanganan jika tidak ketemu / blank
                    }
                }

                return gl;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new ObservableCollection<GeneralLedgerDTO>();
            }

        }

        
        public async Task<List<GeneralLedgerLaporanDTO>> GetBalanceSheet(DateTime end)
        {
            try
            {
                List<GeneralLedgerLaporanDTO> lstLaporan = new List<GeneralLedgerLaporanDTO>();

                // terdiri dari GL dan saldoaal
                // start selalu awaltahun 
                ObservableCollection<SaldoAwalDTO> lstsaldoAwal = await GetSaldoAwal();
                ObservableCollection<GeneralLedgerDTO> generalLedgerDTOs = new ObservableCollection<GeneralLedgerDTO>();
                DateTime start;
                start= new DateTime(AppSession.SelectedYear, 1, 1);
                generalLedgerDTOs = new ObservableCollection<GeneralLedgerDTO>();
                generalLedgerDTOs= await generalLedgerQueryRepository.GetGeneralLedgers(start, end, "");

                //ObservableCollection
                List<AccountDTO> lstAccount = await accountService.GetAccountsDTOAsync();


                decimal saldoawal = 0;
                decimal saldo = 0;

                foreach (AccountDTO ac in lstAccount)
                {
                    saldoawal = 0;
                    saldo = 0;

                    string accountId = ac.Id?.Trim() ?? "";
                    //if(ac.Id.StartsWith("11.9") )
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        if (ac.Root == 1)
                        {
                            // Ambil 1 karakter pertama (misal: "1")
                            string prefix = accountId.Length >= 1 ? accountId.Substring(0, 1) : accountId;
                           
                            saldoawal = lstsaldoAwal
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => ac.Debet * x.Debet - ac.Debet * x.Credit);

                            saldo = generalLedgerDTOs
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => ac.Debet * x.Debet - ac.Debet * x.Credit);
                        }
                        else if (ac.Root == 2)
                        {
                            
                            // Ambil 2 karakter pertama (misal: "11")
                            string prefix = accountId.Length >= 2 ? accountId.Substring(0, 2) : accountId;
                            if (prefix == "11")
                            {
                                prefix = prefix;
                            }
                            
                                saldoawal = lstsaldoAwal
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => ac.Debet * x.Debet - ac.Debet * x.Credit);
                            
                                saldo = generalLedgerDTOs
                                    .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                    .Sum(x => ac.Debet * x.Debet - ac.Debet * x.Credit);
                            
                        }
                        else if (ac.Root == 3)
                        {
                            // Ambil 4 karakter pertama atau sesuai kebutuhan struktur akun Anda (misal: "11.1")
                            string prefix = accountId.Length >= 4 ? accountId.Substring(0, 4) : accountId;

                            saldoawal = lstsaldoAwal
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => ac.Debet * x.Debet - ac.Debet * x.Credit);

                            saldo = generalLedgerDTOs
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => ac.Debet * x.Debet - ac.Debet * x.Credit);
                        }
                    }

                    GeneralLedgerLaporanDTO gl = new GeneralLedgerLaporanDTO
                    {
                        AccountCode = ac.Id,
                        AccountName = ac.Name,
                        CurrentAmount = saldo,
                        BeforeAmount = saldoawal,
                        Root = ac.Root
                    };
                    if (ac.Root < 4)
                    {
                        lstLaporan.Add(gl);
                    }

                    //keika masuk ke code '2.....
                    if (ac.Id.Substring(0, 1) == "2" && ac.Root == 1)
                    {

                        saldoawal = 0;
                        saldo = 0;
                        saldoawal = lstsaldoAwal
                            .Where(x => !string.IsNullOrEmpty(x.AccountCode) &&
                            x.AccountCode.Trim().StartsWith("1"))
                            .Sum(x =>  x.Debet - x.Credit);

                        saldo = generalLedgerDTOs
                            .Where(x => !string.IsNullOrEmpty(x.AccountCode)
                            && (x.AccountCode.Trim().StartsWith("1") ))
                            .Sum(x =>  x.Debet -  x.Credit);


                        GeneralLedgerLaporanDTO jumlahaset = new GeneralLedgerLaporanDTO
                        {
                            AccountCode = "19999999",
                            AccountName = "JUMLAH ASET",
                            CurrentAmount = saldo,
                            BeforeAmount = saldoawal,
                            Root = 1
                        };

                        lstLaporan.Add(jumlahaset);
                    }

                }

                saldoawal = lstsaldoAwal
                        .Where(x => !string.IsNullOrEmpty(x.AccountCode) 
                        && (x.AccountCode.Trim().StartsWith("2") || x.AccountCode.Trim().StartsWith("3")))
                        .Sum(x => -1 * x.Debet - (-1 * x.Credit));

                    saldo = generalLedgerDTOs
                        .Where(x => !string.IsNullOrEmpty(x.AccountCode)
                        && (x.AccountCode.Trim().StartsWith("2") || x.AccountCode.Trim().StartsWith("3")))
                        .Sum(x => -1 * x.Debet - (-1 * x.Credit));

                
                GeneralLedgerLaporanDTO jumkewajianekuitas = new GeneralLedgerLaporanDTO
                {
                    AccountCode = "39999999",
                    AccountName = "JUMLAH KEWAJIBAN DAN EKUITAS",
                    CurrentAmount = saldo,
                    BeforeAmount = saldoawal,
                    Root = 1
                };
                lstLaporan.Add(jumkewajianekuitas);
                return lstLaporan.OrderBy(x => x.AccountCode).ToList();

            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new List<GeneralLedgerLaporanDTO>();
            }

        }

        
        private async Task<ObservableCollection<SaldoAwalDTO>> GetSaldoAwal()
        {
            try
            {
                ObservableCollection<SaldoAwalDTO> saldoAwals = new ObservableCollection<SaldoAwalDTO>();
                SaldoAwalReadService saldoAwalReadService = App.ServiceProvider.GetRequiredService<SaldoAwalReadService>(); ;
                List<SaldoAwalDTO> dataList = await saldoAwalReadService.GetOnYear(AppSession.SelectedYear);
                // Bungkus List tersebut ke dalam ObservableCollection baru, lalu return
                return new ObservableCollection<SaldoAwalDTO>(dataList);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new ObservableCollection<SaldoAwalDTO>();

            }
        }

        public async Task<List<GeneralLedgerLaporanDTO>> GetLabaRugi(DateTime start,
            DateTime end)
        {
            try
            {
                List<GeneralLedgerLaporanDTO> lstLaporan = new List<GeneralLedgerLaporanDTO>();

                // terdiri dari GL dan saldoaal
                // start selalu awaltahun 
                ObservableCollection<SaldoAwalDTO> lstsaldoAwal = await GetSaldoAwal();
                ObservableCollection<GeneralLedgerDTO> generalLedgerDTOs = new ObservableCollection<GeneralLedgerDTO>();
                generalLedgerDTOs = new ObservableCollection<GeneralLedgerDTO>();
                generalLedgerDTOs = await generalLedgerQueryRepository.GetLabaRugi(start, end, "");

                //ObservableCollection
                List<AccountDTO> lstAccount = await accountService.GetAccountsDTOAsync();

                decimal jumlahPendapatan = 0;
                decimal jumlahBeban = 0;
                foreach (AccountDTO ac in lstAccount)
                {
                    decimal saldoawal = 0;
                    decimal saldo = 0;
                    string accountId = ac.Id?.Trim() ?? "";

                    if (!string.IsNullOrEmpty(accountId))
                    {
                        if (ac.Root == 1)
                        {
                            // Ambil 1 karakter pertama (misal: "1")
                            string prefix = accountId.Length >= 1 ? accountId.Substring(0, 1) : accountId;                            
                            saldo = generalLedgerDTOs
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => ac.Debet* x.Debet - ac.Debet * x.Credit);

                            if (prefix == "2" && ac.Root == 1)
                    {
                                jumlahPendapatan = generalLedgerDTOs
                                    .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith("1"))
                                    .Sum(x => x.Debet - x.Credit);
                                GeneralLedgerLaporanDTO jmlpendapatan = new GeneralLedgerLaporanDTO
                                {
                                    AccountCode = "49999999",
                                    AccountName = "JUMLAH PENDAPATAN",
                                    CurrentAmount = saldo,
                                    BeforeAmount = saldoawal,
                                    Root = 1
                                };
                                lstLaporan.Add(jmlpendapatan);
                            }
                        }
                        else if (ac.Root == 2)
                        {
                            // Ambil 2 karakter pertama (misal: "11")
                            string prefix = accountId.Length >= 2 ? accountId.Substring(0, 2) : accountId;

                            saldo = generalLedgerDTOs
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => x.Debet - x.Credit);
                        }
                        else if (ac.Root == 3)
                        {
                            // Ambil 4 karakter pertama atau sesuai kebutuhan struktur akun Anda (misal: "11.1")
                            string prefix = accountId.Length >= 4 ? accountId.Substring(0, 4) : accountId;

                            
                            saldo = generalLedgerDTOs
                                .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith(prefix))
                                .Sum(x => x.Debet - x.Credit);
                        }
                    }

                    GeneralLedgerLaporanDTO gl = new GeneralLedgerLaporanDTO
                    {
                        AccountCode = ac.Id,
                        AccountName = ac.Name,
                        CurrentAmount = saldo,
                        BeforeAmount = saldoawal,
                        Root = ac.Root
                    };

                    if (ac.Root < 4)
                    {
                        lstLaporan.Add(gl);
                    }

                       
                }
                jumlahBeban = generalLedgerDTOs
                                    .Where(x => !string.IsNullOrEmpty(x.AccountCode) && x.AccountCode.Trim().StartsWith("1"))
                                    .Sum(x => -1* x.Debet - (-1 *  x.Credit));
                GeneralLedgerLaporanDTO beban = new GeneralLedgerLaporanDTO
                {
                    AccountCode = "59999999",
                    AccountName = "JUMLAH BELANJA",
                    CurrentAmount = jumlahBeban,
                    BeforeAmount = 0,
                    Root = 1
                };
                lstLaporan.Add(beban);

                GeneralLedgerLaporanDTO surplusdefisit = new GeneralLedgerLaporanDTO
                {
                    AccountCode = "79999999",
                    AccountName = "JUMLAH BELANJA",
                    CurrentAmount = jumlahBeban-jumlahBeban,
                    BeforeAmount = 0,
                    Root = 1
                };
                lstLaporan.Add(surplusdefisit);

                return lstLaporan.OrderBy(x => x.AccountCode).ToList();

            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return new List<GeneralLedgerLaporanDTO>();
            }

        }
        private async Task<ObservableCollection<GeneralLedgerLaporanDTO>> ProcessGLPerlevelAsync(int level, List<GeneralLedgerDTO> lstGL,
            List<SaldoAwalDTO>lstSaldoAwal)
        {
            string idRekening = "";
            try
            {
                int lenspasi = 0;
                int lenKode = 7;
                switch (level)
                {
                    case 1:
                        lenKode = 1;
                        lenspasi = 0;
                        break;
                    case 2:
                        lenKode = 2;
                        lenspasi = 10;
                        break;
                    case 3:
                        lenKode = 3;
                        lenspasi = 20;
                        break;
                }

                //Dapat accon code yang levelnya sesuai dengan parameter level, lalu group by account code dengan substring sesuai dengan level, lalu sum harga perolehan dan count jumlahnya
                var lstAccountCode = (await accountQueryRepository.GetAllAsync()).FindAll(x => x.Root == level);
                // 2. Proyeksikan data GL ke bentuk standar penampung sementara
                var glMapped = lstGL.Select(x => new
                {
                    AccountCode = x.AccountCode?.Trim().Replace(".", "") ?? "",
                    DebetMutasi = x.Debet,
                    CreditMutasi = x.Credit,
                    DebetAwal = 0m,
                    CreditAwal = 0m
                });

                // Proyeksikan data Saldo Awal ke bentuk standar yang sama
                //    (Sesuaikan nama properti AccountCode/Debit/Credit sesuai DTO SaldoAwal Anda)
                var saldoAwalMapped = lstSaldoAwal.Select(x => new
                {
                    AccountCode = x.AccountCode?.Trim().Replace(".", "") ?? "",
                    DebetMutasi = 0m,
                    CreditMutasi = 0m,
                    DebetAwal = x.Debet, // atau x.SALDOAWALDebet
                    CreditAwal = x.Credit // atau x.SALDOAWALCredit
                });

                // 4. Gabungkan kedua list (Concat)
                var combinedList = glMapped.Concat(saldoAwalMapped);

                // 5. Lakukan GroupBy berdasarkan panjang karakter (lenKode) dari hasil gabungan
                var lstJumlah = combinedList
                    .Where(x => x.AccountCode.Length >= lenKode) // Antisipasi jika ada kode yang terlalu pendek
                    .GroupBy(x => x.AccountCode.Substring(0, lenKode))
                    .Select(g => new
                    {
                        AccountCode = g.Key,

                        // Saldo Awal kelompok tersebut
                        BeginningAmountDebet = g.Sum(y => y.DebetAwal),
                        BeginningAmountCredit = g.Sum(y => y.CreditAwal),

                        // Mutasi berjalan kelompok tersebut
                        CurrentAmountDebet = g.Sum(y => y.DebetMutasi),
                        CurrentAmountCredit = g.Sum(y => y.CreditMutasi),

                        // BONUS: Anda juga bisa langsung menghitung Saldo Akhir di sini jika dibutuhkan
                        // (Contoh jika saldo normal debet: Awal + Mutasi Debet - Mutasi Kredit)
                    })
                    .ToList();


                List<GeneralLedgerLaporanDTO> lstReport = (from t in lstAccountCode
                                             join j in lstJumlah
                                             on t.Id.Replace(".", "").Substring(0, lenKode) equals j.AccountCode.Replace(".", "").Trim().Substring(0, lenKode)
                                             select new GeneralLedgerLaporanDTO
                                             {
                                                 AccountCode = t.Id,
                                                 Root= level,
                                                 AccountName =  t.Name,
                                                 CurrentAmount=(t.Debet * j.CurrentAmountDebet) - (t.Debet * j.CurrentAmountCredit),
                                                 BeforeAmount = (t.Debet * j.BeginningAmountDebet) - (t.Debet * j.BeginningAmountCredit),



                                             }).ToList<GeneralLedgerLaporanDTO>();

                            return new ObservableCollection<GeneralLedgerLaporanDTO> (lstReport);
            }
            catch (Exception ex)
            {
                return null;

            }

        }
        private async Task<ObservableCollection<GeneralLedgerLaporanDTO>> ProcessGLPerlevelAsyncLama(int level, List<GeneralLedgerDTO> lst,
            List<SaldoAwalDTO> lstSaldoAwal)
        {
            string idRekening = "";
            try
            {
                int lenspasi = 0;
                int lenKode = 7;
                switch (level)
                {
                    case 1:
                        lenKode = 1;
                        lenspasi = 0;
                        break;
                    case 2:
                        lenKode = 2;
                        lenspasi = 10;
                        break;
                    case 3:
                        lenKode = 3;
                        lenspasi = 20;
                        break;
                }

                //Dapat accon code yang levelnya sesuai dengan parameter level, lalu group by account code dengan substring sesuai dengan level, lalu sum harga perolehan dan count jumlahnya
                var lstAccountCode = (await accountQueryRepository.GetAllAsync()).FindAll(x => x.Root == level);

                var lstJumlah = lst
                         .GroupBy(
                          c => c.AccountCode.Trim().Replace(".", "").Substring(0, lenKode))


                .Select(x => new
                {
                    AccountCode = x.Key,
                    CurrentAmoutDebet = x.Sum(y => y.Debet),
                    CurrentAmoutCredit = x.Sum(y => y.Credit),
                }).ToList();



                List<GeneralLedgerLaporanDTO> lstReport = (from t in lstAccountCode
                                                           join j in lstJumlah
                                                           on t.Id.Replace(".", "").Substring(0, lenKode) equals j.AccountCode.Replace(".", "").Trim().Substring(0, lenKode)
                                                           select new GeneralLedgerLaporanDTO
                                                           {
                                                               AccountCode = t.Id,
                                                               Root = level,
                                                               AccountName = t.Name,
                                                               CurrentAmount = (t.Debet * j.CurrentAmoutDebet) - (t.Debet * j.CurrentAmoutCredit),




                                                           }).ToList<GeneralLedgerLaporanDTO>();

                return new ObservableCollection<GeneralLedgerLaporanDTO>(lstReport);
            }
            catch (Exception ex)
            {
                return null;

            }

        }

    }
}
