using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Repositories.SQLLite;
using System.Windows;

namespace Accounting.Services.JurnalServices
{
    internal class JurnalService:IJurnalService
    {
        IJournalRepository jurnalRepository;
        IUnitOfWork unitOfWork;

        public event EventHandler? JurnalChanged;

        public JurnalService(IUnitOfWork _unitOfWork,
            IJournalRepository _jurnalRepository) {
            unitOfWork = _unitOfWork;
            jurnalRepository = _jurnalRepository;

        }
        private void OnJurnalChanged() => JurnalChanged?.Invoke(this, EventArgs.Empty);

        public async Task<int> CreateBunc(
            List<JurnalImport> listJurnalImport
          )
        {
            try
            {
                
                List<GeneralLedger> listGL= new List<GeneralLedger>();
                listGL = MapJurnalImportToGeneralLedger(listJurnalImport);

                decimal? totaldebet = listGL.Sum(x => x.Debet);
                decimal? totalcredit = listGL.Sum(x => x.Credit);
                if (totaldebet != null)
                {
                    MessageBox.Show($"Dari Service{totaldebet} - {totalcredit}");

                }


                await unitOfWork.BeginTransactionAsync();
                int lastID=await unitOfWork.JournalRepository.CreateBunc( listGL);
                if (lastID > 0)
                {
                        await unitOfWork.CommitAsync();

                }
                      else await  unitOfWork.RollbackAsync();
                return lastID;
                
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private List<Jurnal> MapDtoToJurnalLINQ(List<JurnalDTO> listDto)
        {
            if (listDto is null) return []; // Fitur .NET 8: sintaks 'is null' dan collection expression '[]'

            return listDto.Select(dto => new Jurnal
            {
                Id = dto.Id,
                JournalDate = dto.JournalDate,
                RefNo = dto.RefNo,
                Description = dto.Description,
                Status = dto.Status,

                // Memetakan total nominal
                DebetAmount = dto.TotalDebet,
                CreditAmount = dto.TotalCredit,

                // Fitur .NET 8: Mengambil tahun dengan efisien
                AccountPeriode = dto.JournalDate.Year,
                DCRT = DateTime.Now,
                ReferenceType = 0,
                IDCRT = 1,
                Source = 0,

                // Konversi list detail di dalam objek
                Detail = dto.Detail.Select(detailDto => new JurnalDetail
                {
                    Id = 0,
                    JournalHeaderId = dto.Id,
                    AccountCode = detailDto.AccountCode,
                    Debet = detailDto.Debet,
                    Credit = detailDto.Credit
                }).ToList()
            }).ToList();
        }
        private List<Jurnal> MapDtoToJurnalManual(List<JurnalDTO> listDto)
        {
            if (listDto is null) return [];

            List<Jurnal> listJurnal = []; // Bersih, tidak perlu 'new List<Jurnal>()'

            foreach (var dto in listDto)
            {
                // Cukup tulis new() karena tipe datanya sudah jelas dari struktur List
                Jurnal jurnalBaru = new()
                {
                    Id = dto.Id,
                    JournalDate = dto.JournalDate,
                    RefNo = dto.RefNo,
                    Description = dto.Description,
                    Status = dto.Status,
                    DebetAmount = dto.TotalDebet,
                    CreditAmount = dto.TotalCredit,
                    AccountPeriode = dto.JournalDate.Year,
                    DCRT = DateTime.Now
                };

                foreach (var detailDto in dto.Detail)
                {
                    jurnalBaru.Detail.Add(new()
                    {
                        Id = 0,
                        JournalHeaderId = dto.Id,
                        AccountCode = detailDto.AccountCode,
                        Debet = detailDto.Debet,
                        Credit = detailDto.Credit
                    });
                }

                listJurnal.Add(jurnalBaru);
            }

            return listJurnal;
        }
        private List<GeneralLedger> MapJurnalImportToGeneralLedger(List<JurnalImport> listJI)
        {
            if (listJI is null) return [];

            List<GeneralLedger> listGL= []; // Bersih, tidak perlu 'new List<Jurnal>()'

            foreach (var ji in listJI)
            {
                // Cukup tulis new() karena tipe datanya sudah jelas dari struktur List
                GeneralLedger gl = new()
                {
                  
                    TrxDate= ji.Tanggal,
                    RefNo = ji.NoBukti,
                    Description = ji.Keterangan,
                    Status = 0,
                    Credit = ji.Kredit ?? 0m,
                    Debet = ji.Debit ?? 0m,
                    AccountCode= ji.NoAkun,
                    Source =1,
                    
                };

                listGL.Add(gl);
            }

            return listGL;
        }


        public async Task<int> Simpan(JurnalDTO jurnalDTO)
        {
            // Pemicu awal transaksi dimulai di sini
            await unitOfWork.BeginTransactionAsync();

            try
            {
                // Mapping dari DTO ke Entity Jurnal (Header)
                Jurnal jurnal = new()
                {
                    Id = jurnalDTO.Id,
                    JournalDate = jurnalDTO.JournalDate,
                    RefNo = jurnalDTO.RefNo,
                    Description = jurnalDTO.Description,
                    Status = jurnalDTO.Status,
                    DebetAmount = jurnalDTO.TotalDebet,
                    CreditAmount = jurnalDTO.TotalCredit,
                    AccountPeriode = jurnalDTO.JournalDate.Year,
                    ReferenceType = 0,
                    IDCRT = 1,
                    Source = 0,
                    DCRT = DateTime.Now,

                    // Mapping Detail (Mengakomodasi input multi-baris)
                    Detail = jurnalDTO.Detail.Select(detailDto => new JurnalDetail
                    {
                        Id = 0,
                        JournalHeaderId = jurnalDTO.Id,
                        AccountCode = detailDto.AccountCode,
                        Debet = detailDto.Debet,
                        Credit = detailDto.Credit
                    }).ToList()
                };

                int headerResult = 0;


                // 1. Simpan atau Update Jurnal Header & Detail lewat UnitOfWork
                if (jurnal.Id == 0)
                {
                    headerResult = await unitOfWork.JournalRepository.CreateAsync(jurnal);
                }
                else
                {
                    headerResult = await unitOfWork.JournalRepository.UpdateAsync(jurnal);
                }

                // Jika simpan jurnal gagal, langsung batalkan
                if (headerResult <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return 0;
                }
                await unitOfWork.CommitAsync();
                OnJurnalChanged();
                return headerResult;
            }
            catch (Exception ex)
            {
                // Jika ada error di tengah jalan, kembalikan database ke kondisi semula
                await unitOfWork.RollbackAsync();

                // Catat log error ex disini jika ada logger
                return 0;
            }
        }
        public async Task<bool> DeleteBunc(List<JurnalDTO> listjurnalDTO
            
                        )
        {
            try
            {
                
                
                if (listjurnalDTO != null)
                {
                    await unitOfWork.BeginTransactionAsync();
                    bool  ret = await unitOfWork.JournalRepository.DeleteBunc(listjurnalDTO);
                    if (ret  == true )
                    {
                        await unitOfWork.CommitAsync();

                    }
                    else await unitOfWork.RollbackAsync();
                    return ret;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false ;
            }
        }


    }
}
