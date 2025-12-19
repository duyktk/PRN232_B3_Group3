using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Service.DTOs;
using Service.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PRN232_B3_Group3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "0")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Helper to get Role from current Token
        private string GetCurrentRole()
        {
            // Real logic: Get Claim Role from Token
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return role ?? "-1"; // Return -1 if role not found
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
                return Ok(new { message = "User created successfully" });
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
            if (id != userDto.Userid) return BadRequest("ID does not match");

            try
            {
                var role = GetCurrentRole();
                await _userService.UpdateUserAsync(userDto, role);
                return Ok(new { message = "Update successful" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex) // Catch other logic errors (such as user not found)
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
                return Ok(new { message = "User deleted (hidden) successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }
    }
}
