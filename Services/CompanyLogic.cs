using Accounting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Applications
{
    internal class CompanyLogic
    {

        public CompanySetting GetCompany()
        {
            CompanySetting company = new CompanySetting()
            {

                Id = 1,
                Nama = "Koperasi W N",
                Alamat = "Jalan AAAA",
                Description = "Perusahaan Koperasi Karyawan"
            };
            return company;

        }

    }
}
