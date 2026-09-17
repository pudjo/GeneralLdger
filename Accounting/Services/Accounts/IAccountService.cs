using Accounting.DTO;
using Accounting.Repositories.SQLLite.Master;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Accounts
{
    internal interface IAccountService
    {
        Task<List<AccountDTO>> GetTreeAccounts();
        Task<List<AccountDTO>> GetAccountsDTOAsync();      
        Task<int> SaveAccountAsync(AccountDTO accountDto);
        Task<bool> DeleteAccountAsync(string code);
        Task<int> GetChildrenCount(string code);


        event EventHandler? AccountsChanged;
        Task<string> ExportAccountsToExcelAsync(string filePath);
        
        Task<int> ImportAccountsFromExcelAsync(string filePath);


    }
}
