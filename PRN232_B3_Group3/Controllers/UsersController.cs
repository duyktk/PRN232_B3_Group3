using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Service.DTOs;
using Service.Interfaces;
using System.Security.Claims;

namespace PRN232_B3_Group3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Helper lấy Role từ Token hiện tại
        private string GetCurrentRole()
        {
            // Nếu chưa làm authen, tạm thời return "0" để test quyền Admin
             return "0"; 

            // Logic thật: Lấy Claim Role từ Token
            //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            //return role ?? "-1"; // Trả về -1 nếu không tìm thấy role
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDto userDto)
        {
            try
            {
                var role = GetCurrentRole();
                await _userService.CreateUserAsync(userDto, role);
                return Ok(new { message = "Tạo user thành công" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Userid) return BadRequest("ID không khớp");

            try
            {
                var role = GetCurrentRole();
                await _userService.UpdateUserAsync(userDto, role);
                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex) // Bắt các lỗi logic khác (như không tìm thấy user)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var role = GetCurrentRole();
                await _userService.DeleteUserAsync(id, role);
                return Ok(new { message = "Xóa (ẩn) user thành công" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }
    }
}
