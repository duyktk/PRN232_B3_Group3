using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Models;
using Service.DTOs;

namespace Service.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int id);

        // requestorRole: Role của người đang thực hiện hành động (Admin hay GV...)
        Task CreateUserAsync(UserDto userDto, string requestorRole);
        Task UpdateUserAsync(UserDto userDto, string requestorRole);
        Task DeleteUserAsync(int id, string requestorRole);

        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    }
}
