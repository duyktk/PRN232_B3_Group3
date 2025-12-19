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
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task CreateUserAsync(UserDto userDto, string requestorRole)
        {
            if (requestorRole != "0") throw new UnauthorizedAccessException("Only Admin can create accounts.");

            var existingUsers = await _unitOfWork.UserRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Username == userDto.Username)) throw new Exception("Username already exists.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var newUser = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Role = userDto.Role,
                Password = passwordHash,
                Createat = DateTime.Now,
                Isactive = true
            };

            await _unitOfWork.UserRepository.CreateAsync(newUser);
        }

        public async Task UpdateUserAsync(UserDto userDto, string requestorRole)
        {
            if (requestorRole != "0") throw new UnauthorizedAccessException("Only Admin can update.");

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userDto.Userid);
            if (user == null) throw new Exception("User does not exist.");

            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Role = userDto.Role;

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
                throw new UnauthorizedAccessException("Only Admin can delete accounts.");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.Isactive = false;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }
            if (requestorRole != "0") throw new UnauthorizedAccessException("Insufficient permissions.");
            if (user != null) { user.Isactive = false; await _unitOfWork.UserRepository.UpdateAsync(user); }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var defaultEmail = _configuration["DefaultAdmin:Email"];
            var defaultPass = _configuration["DefaultAdmin:Password"];

            if (!string.IsNullOrEmpty(defaultEmail) &&
                defaultEmail.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase) &&
                defaultPass == loginDto.Password)
            {
                var adminUser = new User
                {
                    Userid = 0,
                    Username = "SuperAdmin",
                    Email = defaultEmail,
                    Role = "0",
                    Isactive = true
                };

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

            var users = await _unitOfWork.UserRepository.GetAllAsync();

            var user = users.FirstOrDefault(u =>
                u.Email.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase));

            if (user == null) return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);

            if (!isPasswordValid) return null;

            if (user.Isactive == false) throw new Exception("Account is locked.");

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = new UserResponseDto
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
                    new Claim(ClaimTypes.Role, user.Role),
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
