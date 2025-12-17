using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Models;
using Service.BussinessModel;
using Service.DTOs;

namespace Service;

public class StudentService : IStudentService_
{
    private readonly UnitOfWork _unitOfWork;

    public StudentService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Helper: Map từ Entity sang DTO để trả về
    private StudentResponseDto MapToDto(Student student, bool includeDetails = true)
    {
        var dto = new StudentResponseDto
        {
            Studentid = student.Studentid,
            Studentfullname = student.Studentfullname,
            Studentroll = student.Studentroll,
            Isactive = student.Isactive,
            Createat = student.Createat,
            GroupNames = new List<string>()
        };

        if (includeDetails && student.GroupStudents != null)
        {
            dto.GroupNames = student.GroupStudents
                .Where(gs => gs.Group != null)
                .Select(gs => gs.Group.Groupname)
                .ToList();
        }

        return dto;
    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id, bool includeDetails = true)
    {
        var query = _unitOfWork.StudentRepository.GetDbSet().AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(s => s.GroupStudents)
                    .ThenInclude(gs => gs.Group);
        }

        var student = await query.FirstOrDefaultAsync(s => s.Studentid == id);

        if (student == null)
        {
            return null;
        }

        return MapToDto(student, includeDetails);
    }

    public async Task<PagedResult<StudentResponseDto>> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        string? searchName = null,
        string? searchMssv = null,
        string? searchClass = null)
    {
        var query = _unitOfWork.StudentRepository.GetDbSet()
            .Include(s => s.GroupStudents)
                .ThenInclude(gs => gs.Group)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            searchName = searchName.Trim();
            query = query.Where(s => s.Studentfullname.Contains(searchName));
        }

        if (!string.IsNullOrWhiteSpace(searchMssv))
        {
            searchMssv = searchMssv.Trim();
            query = query.Where(s => s.Studentroll.Contains(searchMssv));
        }

        if (!string.IsNullOrWhiteSpace(searchClass))
        {
            searchClass = searchClass.Trim();
            query = query.Where(s => s.GroupStudents.Any(gs => gs.Group.Groupname.Contains(searchClass)));
        }

        var totalCount = await query.CountAsync();

        var students = await query
            .OrderBy(s => s.Studentid)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = students.Select(s => MapToDto(s, true)).ToList();

        return new PagedResult<StudentResponseDto>(items, totalCount, pageNumber, pageSize);
    }
    
    public async Task<StudentResponseDto> CreateAsync(StudentCreateDto model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (string.IsNullOrWhiteSpace(model.Studentfullname))
        {
            throw new ArgumentException("Tên sinh viên không được để trống");
        }

        if (string.IsNullOrWhiteSpace(model.Studentroll))
        {
            throw new ArgumentException("MSSV không được để trống");
        }

        // Check if MSSV already exists
        var existingStudent = await _unitOfWork.StudentRepository.GetDbSet()
            .FirstOrDefaultAsync(s => s.Studentroll == model.Studentroll);

        if (existingStudent != null)
        {
            throw new InvalidOperationException($"MSSV {model.Studentroll} đã tồn tại");
        }

        var student = new Student
        {
            Studentfullname = model.Studentfullname,
            Studentroll = model.Studentroll,
            Isactive = model.Isactive ?? true,
            Createat = DateTime.Now
        };

        await _unitOfWork.StudentRepository.CreateAsync(student);

        var result = await GetByIdAsync(student.Studentid, false);
        return result ?? throw new InvalidOperationException("Failed to retrieve created student");
    }

    public async Task<StudentResponseDto> UpdateAsync(int id, StudentUpdateDto model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (id != model.Studentid)
        {
            throw new ArgumentException("ID không khớp");
        }

        var existingStudent = await _unitOfWork.StudentRepository.GetDbSet()
            .FirstOrDefaultAsync(s => s.Studentid == id);

        if (existingStudent == null)
        {
            throw new InvalidOperationException($"Không tìm thấy sinh viên với ID {id}");
        }

        // Check if new MSSV already exists (if changed)
        if (existingStudent.Studentroll != model.Studentroll)
        {
            var duplicateStudent = await _unitOfWork.StudentRepository.GetDbSet()
                .FirstOrDefaultAsync(s => s.Studentroll == model.Studentroll && s.Studentid != id);

            if (duplicateStudent != null)
            {
                throw new InvalidOperationException($"MSSV {model.Studentroll} đã tồn tại");
            }
        }

        existingStudent.Studentfullname = model.Studentfullname;
        existingStudent.Studentroll = model.Studentroll;
        existingStudent.Isactive = model.Isactive;

        await _unitOfWork.StudentRepository.UpdateAsync(existingStudent);

        var result = await GetByIdAsync(id, false);
        return result ?? throw new InvalidOperationException("Failed to retrieve updated student");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _unitOfWork.StudentRepository.GetDbSet()
            .Include(s => s.GroupStudents)
            .Include(s => s.Submissions)
            .FirstOrDefaultAsync(s => s.Studentid == id);

        if (student == null)
        {
            throw new InvalidOperationException($"Không tìm thấy sinh viên với ID {id}");
        }

        // Check if student has any submissions or groups
        if (student.Submissions?.Any() == true || student.GroupStudents?.Any() == true)
        {
            // Soft delete - just mark as inactive
            student.Isactive = false;
            await _unitOfWork.StudentRepository.UpdateAsync(student);
            return true;
        }

        // Hard delete if no related data
        return await _unitOfWork.StudentRepository.RemoveAsync(student);
    }
}