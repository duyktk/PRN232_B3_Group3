using Service.BussinessModel;
using Service.DTOs;

namespace Service;

public interface IStudentService_
{
    Task<StudentResponseDto?> GetByIdAsync(int id, bool includeDetails = true);

    Task<PagedResult<StudentResponseDto>> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        string? searchName = null,
        string? searchMssv = null,
        string? searchClass = null);

    Task<StudentResponseDto> CreateAsync(StudentCreateDto model);

    Task<StudentResponseDto> UpdateAsync(int id, StudentUpdateDto model);

    Task<bool> DeleteAsync(int id);
    
    // Task<List<StudentResponseDto>> SearchAsync(
    //     string? searchName = null,
    //     string? searchMssv = null,
    //     string? searchClass = null);
}