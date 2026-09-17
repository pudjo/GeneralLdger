using Accounting.Domain.Entities;
using Accounting.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Services.Administration
{
    public interface IUserService
    {
       Task<List<UserDTO>> GetAlUserDTO();
        Task<UserDTO> GetUserDTOByID(int id);

        Task<List<User>> GetAlUser();
        Task<UserDTO> UserLogin(LoginDTO loginDTO);
        Task<int> SaveUserAsync(UserDTO userDto);
        Task<bool> DeleteUserAsync(UserDTO userDto);

        event EventHandler? UsersChanged;

    }
}
