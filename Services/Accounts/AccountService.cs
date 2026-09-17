using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace Accounting.Services.Accounts
{
    public class AccountService: IAccountService
    {

        private IAccountRepository repo;
        private IAccountQueryRepository accountQueryRepository;
        private readonly ILogger<AccountService> _logger;

        public event EventHandler? AccountsChanged;
        private void OnAccountsChanged() => AccountsChanged?.Invoke(this, EventArgs.Empty);

        public AccountService(IAccountRepository _repo, IAccountQueryRepository _accountQueryRepository, ILogger<AccountService> logger) {
            repo = _repo;
            accountQueryRepository = _accountQueryRepository;
            _logger = logger;
        }

        
        public async Task<List<AccountDTO>> GetTreeAccounts()
        {
            try
            {

                   var lstAccount = await GetAccountsDTOAsync();
                    List<AccountDTO> lst = lstAccount.ToList();
                    List<AccountDTO> lstReturned = new List<AccountDTO>();
                    foreach (AccountDTO account in lst)
                    {
                        if (string.IsNullOrEmpty(account.IdParent) || account.IdParent.Trim() == "0")
                        {
                            // Recusively call it's children
                            account.Children = GetChildren(lst, account.Id);
                            lstReturned.Add(account);
                        }

                    }
                    _logger.LogInformation("Get data from database..");

                    return lstReturned;

                
            }
            catch (Exception exp)
            {
          
                _logger.LogError(exp, "Error in GetTreeAccounts  IdParent: {Id}", "Root");
                throw;
            }
        }
        public async Task<int> GetChildrenCount(string idParent)
        {
            try
            {
                var lstAccount = await GetAccountsDTOAsync();
                List<AccountDTO> lst = lstAccount.ToList();
                var matches = lst.Where(a => a.IdParent == idParent && a.Id != idParent).ToList();
                return matches.Count;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Error in GetChildrenCount  IdParent: {Id}", idParent);
                throw;
            }
        }
        private List<AccountDTO> GetChildren(List<AccountDTO> lst, string idParent)
        {
            try
            {
                List<AccountDTO> lstChildren = new List<AccountDTO>();
                // Ambil hanya akun yang memang anak dari idParent ini
                //var matches = lst.Where(a => a.IdParent == idParent).ToList();
                var matches = lst.Where(a => a.IdParent == idParent && a.Id != idParent).ToList();

                foreach (AccountDTO account in matches)
                {
                    account.Children = GetChildren(lst, account.Id);
                    lstChildren.Add(account);
                }
                return lstChildren;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp.Message, "Error in GetChildren  IdParent: {Id}", idParent);
                return null;
            }
        }

        public async Task<List<AccountDTO>> GetAccountsDTOAsync()
        {
            try
            {
                IEnumerable<Account> rawAccounts = await accountQueryRepository.GetAllAsync();
                List<Account> accountList = rawAccounts.ToList();

                List<AccountDTO> dtoList = accountList.Select(current => new AccountDTO
                {
                    Id = current.Id,
                    Name = current.Name,
                    Root = current.Root,
                    IdParent = current.IdParent,
                    Leaf = current.Leaf,
                    Debet = current.Debet,
                    ParentName = accountList.FirstOrDefault(parent => parent.Id == current.IdParent)?.Name
                                 ?? "Root / Tanpa Parent" // Jika tidak ketemu / null, beri teks default
                }).ToList();

                return dtoList;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Error in GetAccountsDTOAsync");
                return null;
            }
        }

        public async Task<int> SaveAccountAsync(AccountDTO accountDto)
        {
            try
            {
                string message = "";
                // 1. TUGAS SERVICE: Validasi Aturan Bisnis (Akuntansi)
                if (accountDto.Id == accountDto.IdParent)
                {
                    message = "Akun tidak boleh menunjuk dirinya sendiri sebagai Parent!";
                    _logger.LogError(message);
                    return 0;
                }

                if (string.IsNullOrEmpty(accountDto.Name))
                {
                    message = "Nama akun wajib diisi";
                    _logger.LogError(message);
                    return 0;

                }

                Account accountEntity = new Account
                {
                    Id = accountDto.Id,
                    Name = accountDto.Name,
                    Root = accountDto.Root,
                    IdParent = accountDto.IdParent,
                    Leaf = accountDto.Leaf,
                    Debet = accountDto.BoolDebet == true ? 1 : -1,

                };
                int rowsAffected = 0;
                var existingAccount = await accountQueryRepository.GetByIdAsync(accountEntity.Id);

                if (existingAccount == null)
                {
                    // Jika belum ada, lakukan Insert (Create)
                    rowsAffected = await repo.CreateAsync(accountEntity);
                }
                else
                {
                    // Jika sudah ada, lakukan Update
                    rowsAffected = await repo.UpdateAsync(accountEntity);
                }


                if (rowsAffected > 0)
                {
                    OnAccountsChanged();
                    return rowsAffected;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Error in SaveAccountAsync for Account ID: {Id}", accountDto.Id);
                return 0;
            }
        }

        public async Task<bool> DeleteAccountAsync(string code)
        {
            try
            {
                string message = "";
                // 1. TUGAS SERVICE: Validasi Aturan Bisnis (Akuntansi)
                if (await repo.DeleteAsync(code) ==true) { 
                    OnAccountsChanged();
                    return true ;
                } return false;
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Error in SaveAccountAsync for Account ID: {Id}", code);
                return false ;
            }
        }

        public Task<int> CountChildrenAsync(string code)
        {
            throw new NotImplementedException();
        }
        public async Task<string>   ExportAccountsToExcelAsync(string filePath)
        {
            try
            {
                var accounts = await GetAccountsDTOAsync();
                if (accounts == null) accounts = new List<AccountDTO>();

                // Pastikan direktori tujuan ada
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Jalankan operasi ClosedXML secara asynchronous / background task
                await Task.Run(() =>
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Accounts");

                        // Header kolom
                        worksheet.Cell(1, 1).Value = "Id";
                        worksheet.Cell(1, 2).Value = "Name";
                        worksheet.Cell(1, 3).Value = "Root";
                        worksheet.Cell(1, 4).Value = "IdParent";
                        worksheet.Cell(1, 5).Value = "Leaf";
                        worksheet.Cell(1, 6).Value = "Debet";
                        worksheet.Cell(1, 7).Value = "ParentName";

                        // Styling Header (Opsional agar terlihat profesional)
                        var headerRange = worksheet.Range("A1:G1");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                        // Masukkan data baris demi baris
                        int row = 2;
                        foreach (var a in accounts)
                        {
                            worksheet.Cell(row, 1).Value = a.Id ?? string.Empty;
                            worksheet.Cell(row, 2).Value = a.Name ?? string.Empty;
                            worksheet.Cell(row, 3).Value = a.Root;
                            worksheet.Cell(row, 4).Value = a.IdParent ?? string.Empty;
                            worksheet.Cell(row, 5).Value = a.Leaf;
                            worksheet.Cell(row, 6).Value = a.Debet;
                            worksheet.Cell(row, 7).Value = a.ParentName ?? string.Empty;
                            row++;
                        }

                        // Otomatis sesuaikan lebar kolom agar rapi
                        worksheet.Columns().AdjustToContents();

                        // Simpan workbook ke file path
                        workbook.SaveAs(filePath);
                    }
                });

                return filePath;
            }
            catch (Exception ex){
                _logger.LogError("Salah export " + ex.Message);
                return "";
            }
        }

        /// <summary>
        /// Import akun dari file Excel (.xlsx). Kolom: Id, Name, Root, IdParent, Leaf, Debet, ParentName
        /// Returns number of successfully saved records.
        /// </summary>
        public async Task<int> ImportAccountsFromExcelAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return 0;

                int saved = 0;

                await Task.Run(async () =>
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        // Ambil worksheet pertama
                        var worksheet = workbook.Worksheets.Worksheet(1);

                        // Ambil baris terakhir yang terisi data
                        var rows = worksheet.RowsUsed();
                        bool isHeader = true;

                        foreach (var row in rows)
                        {
                            // Lewati baris pertama (Header)
                            if (isHeader)
                            {
                                isHeader = false;
                                continue;
                            }

                            try
                            {
                                // Membaca cell berdasarkan nomor kolom (1-based index)
                                var id = row.Cell(1).GetValue<string>();
                                var name = row.Cell(2).GetValue<string>();
                                var rootStr = row.Cell(3).GetValue<string>();
                                var idParent = row.Cell(4).GetValue<string>();
                                var leafStr = row.Cell(5).GetValue<string>();
                                var debetStr = row.Cell(6).GetValue<string>();
                                var parentName = row.Cell(7).GetValue<string>();

                                var dto = new AccountDTO
                                {
                                    Id = id,
                                    Name = name,
                                    Root = int.TryParse(rootStr, out var r) ? r : 0,
                                    IdParent = string.IsNullOrWhiteSpace(idParent) ? null : idParent,
                                    Leaf = int.TryParse(leafStr, out var lf) ? lf : 0,
                                    Debet = int.TryParse(debetStr, out var db) ? db : 1,
                                    ParentName = string.IsNullOrWhiteSpace(parentName) ? null : parentName
                                };

                                // Panggil fungsi simpan asynchronously di dalam task
                                var rowsAffected = await SaveAccountAsync(dto);
                                if (rowsAffected > 0)
                                {
                                    lock (this) { saved++; }
                                }
                            }
                            catch
                            {
                                // Abaikan baris yang error/malformed, lanjut ke baris berikutnya
                            }
                        }
                    }
                });

                if (saved > 0) OnAccountsChanged();
                return saved;

            }
            catch (Exception ex)
            {
                _logger.LogError("Salah import " + ex.Message);
                return 0;
            }
        }

}
}
