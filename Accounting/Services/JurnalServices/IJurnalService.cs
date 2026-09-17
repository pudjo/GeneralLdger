using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.JurnalServices
{
    internal interface IJurnalService
    {
        Task<int> CreateBunc(List<JurnalImport> listJurnalImport
                );
        Task<int> Simpan(JurnalDTO jurnalDTO);
        Task<bool> DeleteBunc(List<JurnalDTO> listjurnalDTO);
        event EventHandler?JurnalChanged;
    }
    }


