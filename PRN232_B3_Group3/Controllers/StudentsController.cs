using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs;

namespace PRN232_B3_Group3.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentsController : ControllerBase
{
    private readonly IStudentService_ _studentService;

    public StudentsController(IStudentService_ studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="includeDetails">Include group details</param>
    /// <returns>Student details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] bool includeDetails = true)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id, includeDetails);
            if (student == null)
            {
                return NotFound(new { message = $"Không tìm thấy sinh viên với ID {id}" });
            }
            return Ok(student);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get paginated list of students with search filters
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchName">Search by student name</param>
    /// <param name="searchMssv">Search by student ID (MSSV)</param>
    /// <param name="searchClass">Search by class/group name</param>
    /// <returns>Paginated student list</returns>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] string? searchMssv = null,
        [FromQuery] string? searchClass = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            var result = await _studentService.GetPagedAsync(
                pageNumber, 
                pageSize, 
                searchName, 
                searchMssv, 
                searchClass);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Create a new student
    /// </summary>
    /// <param name="model">Student creation data</param>
    /// <returns>Created student</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudentCreateDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var student = await _studentService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = student.Studentid }, student);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="model">Student update data</param>
    /// <returns>Updated student</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Studentid)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            var student = await _studentService.UpdateAsync(id, model);
            return Ok(student);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete a student (soft delete if has related data)
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _studentService.DeleteAsync(id);
            return Ok(new { message = "Xóa sinh viên thành công", deleted = result });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }
}

