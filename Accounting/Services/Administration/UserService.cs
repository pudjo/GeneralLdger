using Accounting.Domain.Entities;
using Accounting.DTO;
using Accounting.IRepositories;
using Accounting.Services.Administration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Transforms;


namespace Accounting.Services.Administration
{
    public class UserService: IUserService
    {
        private IUserRepository repo;
        private readonly ILogger<UserService> logger;

        public event EventHandler? UsersChanged;

        public UserService(IUserRepository _repo, 
            ILogger<UserService> _logger)
        {
            repo = _repo;
            logger = _logger;
        }

        private void OnUsersChanged() => UsersChanged?.Invoke(this, EventArgs.Empty);

        public async Task<List<UserDTO>> GetAlUserDTO()
        {
            try
            {
                List<User> lstUser = await repo.GetAllAsync();

                List<UserDTO> lstUserDTO = lstUser.Select(current => new UserDTO
                {
                    Id = current.Id,
                    UserID= current.UserID,
                    Name = current.Name,
                    HandPhoneNumber = current.HandPhoneNumber,
                    Email = current.Email,
                    Status = current.Status,


                }).ToList();
            
                     return lstUserDTO;
        }
         catch (Exception exp)
            {
                // Mencatat error secara detail beserta Stack Trace-nya
                logger.LogError(exp, "Error in GetAllUser ");
                throw;
            }
        }
        public async Task<UserDTO> GetUserDTOByID(int id)
        {
            try
            {
                List<User> lstUser = await repo.GetAllAsync();
                List<UserDTO> lstUserDTO = lstUser.Select(current => new UserDTO
                {
                    Id = current.Id,
                    UserID = current.UserID,
                    Name = current.Name,
                    HandPhoneNumber = current.HandPhoneNumber,
                    Email = current.Email,
                    Status = current.Status,


                }).ToList();
                UserDTO userDTO = lstUserDTO.FirstOrDefault(x => x.Id == id);
                if(userDTO == null)
                {
                    logger.LogError($"User dengan ID {id} tidak ditemukan.");
                    return null;
                }
                return userDTO;
            }
            catch (Exception exp)
            {
                // Mencatat error secara detail beserta Stack Trace-nya
                logger.LogError(exp, "01.Kesalahan dalam Mengambil data semua User");
                throw;
            }
        }
        public async Task<List<User>> GetAlUser()
        {
            try
            {
                List<User> lstUser = await repo.GetAllAsync();

                return lstUser;
            }
            catch (Exception exp)
            {
                // Mencatat error secara detail beserta Stack Trace-nya
                logger.LogError(exp, "Error in GetAllUser ");
                throw;
            }
        }

        public async Task<UserDTO> UserLogin(LoginDTO loginDTO)
        {
            try
            {
                logger.LogError("Masuk Get All Async");
                List<User> lstUser = await repo.GetAllAsync();
                User user = new User();
                if (loginDTO.UserID== null)
                {
                 
                    return null;
                }
                logger.LogError("Get User");
                user = lstUser.FirstOrDefault(x=> x.UserID.Trim().ToLower()== loginDTO.UserID.Trim().ToLower());
                

                if (user != null) {
                    logger.LogError("User ketemu");
                    UserDTO userDTO = new UserDTO
                    {
                        Id = user.Id,
                        UserID = user.UserID,
                        Name = user.Name,
                        HandPhoneNumber = user.HandPhoneNumber,
                        Email = user.Email,
                        Status = user.Status,
                        Password = user.Password
                    };
                   
                    if (userDTO.Status != 0)
                    {
                        if (string.IsNullOrEmpty(loginDTO.Password) )
                        {
                           // Error = "Password harus diisi";
                            return null;
                        } else
                        {
                            if (userDTO.Password != loginDTO.Password)
                            {
                             //   Error= "Password salah.";
                                return null;
                            } 
                        }

                    }
                    logger.LogError("Keluar");
                    return userDTO;

                } else
                {
                    

                    logger.LogError("Pengguna tidak ditemukan");
                    return null;
                }
               

            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show($"Looping berjalan! Memproses Jurnal pertama dengan RefNo: {exp.Message}");
                // Mencatat error secara detail beserta Stack Trace-nya
                logger.LogError(exp.Message, "Error in UserLogin ");
                throw;
            }
        }
        public async Task<int> SaveUserAsync(UserDTO userDto)
        {
            //  Validete the incoming data
            User user = new User
            {
                Id = userDto.Id,
                Name = userDto.Name,
                UserID = userDto.UserID,
                HandPhoneNumber = userDto.HandPhoneNumber,
                Email = userDto.Email,
                Status = userDto.Status ,
                Password= userDto.Password,

            }; //
            int result = 0;
            if (userDto.Id == 0)
            {
                result =await repo.CreateAsync(user);
            }
            else
            {
                result =await repo.UpdateAsync(user);
            }
            if (result > 0)
            {
                OnUsersChanged();
                return result;
            }else
            {
                return 0;
            }

        }
        public async Task<bool> DeleteUserAsync(UserDTO userDto)
        {
            try
            {
                //  Validete the incoming data
                User user = new User
                {
                    Id = userDto.Id,
                    Name = userDto.Name,
                    UserID = userDto.UserID,
                    HandPhoneNumber = userDto.HandPhoneNumber,
                    Email = userDto.Email,
                    Status = userDto.Status,
                    Password = userDto.Password,

                }; //
                bool result = false;
                if (userDto.Id != 0)
                {
                    result= await repo.DeleteAsync(userDto.Id);
                    if (result) OnUsersChanged();
                }
                return result ;
            }
            catch
            {
                return false;

            }
            

        }

    }
}
