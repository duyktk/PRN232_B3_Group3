using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository;
using Repository.Models;
using Service.DTOs;
using Service.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Service
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public UserService(UnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        // Helper: Map từ Entity sang DTO để trả về
        private UserResponseDto MapToDto(User user)
        {
            return new UserResponseDto
            {
                Userid = user.Userid,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Isactive = user.Isactive,
                Createat = user.Createat
            };
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            // Convert list User -> list UserResponseDto
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task CreateUserAsync(UserDto userDto, string requestorRole)
        {
            if (requestorRole != "0") throw new UnauthorizedAccessException("Chỉ Admin mới được tạo tài khoản.");

            var existingUsers = await _unitOfWork.UserRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Username == userDto.Username)) throw new Exception("Username đã tồn tại.");

            // HASH PASSWORD TẠI ĐÂY
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var newUser = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Role = userDto.Role,
                Password = passwordHash, // Lưu pass đã mã hóa
                Createat = DateTime.Now,
                Isactive = true
            };

            await _unitOfWork.UserRepository.CreateAsync(newUser);
        }

        public async Task UpdateUserAsync(UserDto userDto, string requestorRole)
        {
            if (requestorRole != "0") throw new UnauthorizedAccessException("Chỉ Admin mới được sửa.");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userDto.Userid);
            if (user == null) throw new Exception("User không tồn tại.");

            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Role = userDto.Role;

            // Nếu có nhập password mới thì Hash lại
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            await _unitOfWork.UserRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id, string requestorRole)
        {
            if (requestorRole != "0")
            {
                throw new UnauthorizedAccessException("Chỉ Admin mới được xóa tài khoản.");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user != null)
            {
                // Cách 1: Xóa vĩnh viễn (Hard Delete)
                // await _unitOfWork.UserRepository.RemoveAsync(user);

                // Cách 2: Xóa mềm (Soft Delete) - Khuyên dùng vì entity có Isactive
                user.Isactive = false;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }
            if (requestorRole != "0") throw new UnauthorizedAccessException("Quyền hạn không đủ.");
            if (user != null) { user.Isactive = false; await _unitOfWork.UserRepository.UpdateAsync(user); }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // -----------------------------------------------------------------------
            // BƯỚC 1: KIỂM TRA DEFAULT ADMIN TRONG APPSETTINGS TRƯỚC
            // -----------------------------------------------------------------------
            var defaultEmail = _configuration["DefaultAdmin:Email"];
            var defaultPass = _configuration["DefaultAdmin:Password"];

            // So sánh (bỏ qua viết hoa thường ở email)
            if (!string.IsNullOrEmpty(defaultEmail) &&
                defaultEmail.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase) &&
                defaultPass == loginDto.Password) // So sánh pass thô luôn (vì trong config là "123")
            {
                // Tạo một User "ảo" để sinh Token
                var adminUser = new User
                {
                    Userid = 0, // ID đặc biệt cho admin hệ thống
                    Username = "SuperAdmin",
                    Email = defaultEmail,
                    Role = "0", // Role 0 là Admin
                    Isactive = true
                };

                // Cấp Token luôn
                var adminToken = GenerateJwtToken(adminUser);

                return new LoginResponseDto
                {
                    Token = adminToken,
                    User = new UserResponseDto
                    {
                        Userid = adminUser.Userid,
                        Username = adminUser.Username,
                        Role = adminUser.Role,
                        Email = adminUser.Email
                    }
                };
            }

            // -----------------------------------------------------------------------
            // BƯỚC 2: NẾU KHÔNG PHẢI DEFAULT ADMIN THÌ TÌM TRONG DATABASE NHƯ CŨ
            // -----------------------------------------------------------------------
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            // Tìm user trong DB
            var user = users.FirstOrDefault(u =>
                u.Email.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase));

            if (user == null) return null;

            // Check pass hash (BCrypt)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);

            if (!isPasswordValid) return null;

            if (user.Isactive == false) throw new Exception("Tài khoản đã bị khóa.");

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = new UserResponseDto // MapToDto(user)
                {
                    Userid = user.Userid,
                    Username = user.Username,
                    Role = user.Role,
                    Email = user.Email
                }
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng: Role để phân quyền
                new Claim("Email", user.Email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(jwtSettings["TokenExpirationHours"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
