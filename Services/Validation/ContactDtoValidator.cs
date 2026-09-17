using Accounting.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Validation
{

    internal class ContactDtoValidator : AbstractValidator<ContactDTO>
    {
        public ContactDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kode wajib diisi.")
                .MaximumLength(50).WithMessage("Kode maksimal 50 karakter.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nama wajib diisi.")
                .MaximumLength(200).WithMessage("Nama maksimal 200 karakter.");

            RuleFor(x => x.Phone)
                .MaximumLength(50).WithMessage("Nomor HP maksimal 50 karakter.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Format email tidak valid.");

            RuleFor(x => x.Address)
                .MaximumLength(1000).WithMessage("Alamat terlalu panjang.");
        }
    }
}