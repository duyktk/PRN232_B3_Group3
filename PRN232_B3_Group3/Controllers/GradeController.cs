using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs;
using System.Security.Claims;

namespace PRN232_B3_Group3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] GradeUpsertDto dto)
        {
            try
            {
                int myUserId = GetMyUserId();
                string myRole = GetMyRole();

                var result = await _gradeService.CreateGradeAsync(myUserId, myRole, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] GradeUpsertDto dto)
        {
            try
            {
                int myUserId = GetMyUserId();
                string myRole = GetMyRole();

                var result = await _gradeService.UpdateGradeAsync(myUserId, myRole, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        private int GetMyUserId()
        {
            var idStr = User.FindFirstValue("userid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id)) throw new Exception("Cannot read userid from token.");
            return id;
        }

        private string GetMyRole()
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            if (string.IsNullOrWhiteSpace(role)) throw new Exception("Cannot read role from token.");
            return role;
        }
    }
}
