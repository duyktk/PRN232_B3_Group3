using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class StudentDto
{
    public int Studentid { get; set; }
    
    [Required(ErrorMessage = "Tên sinh viên không được để trống")]
    [StringLength(200, ErrorMessage = "Tên sinh viên không được quá 200 ký tự")]
    public string Studentfullname { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "MSSV không được để trống")]
    [StringLength(50, ErrorMessage = "MSSV không được quá 50 ký tự")]
    public string Studentroll { get; set; } = string.Empty;
    
    public bool? Isactive { get; set; }
}

public class StudentCreateDto
{
    [Required(ErrorMessage = "Tên sinh viên không được để trống")]
    [StringLength(200, ErrorMessage = "Tên sinh viên không được quá 200 ký tự")]
    public string Studentfullname { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "MSSV không được để trống")]
    [StringLength(50, ErrorMessage = "MSSV không được quá 50 ký tự")]
    public string Studentroll { get; set; } = string.Empty;
    
    public bool? Isactive { get; set; }
}

public class StudentUpdateDto
{
    public int Studentid { get; set; }
    
    [Required(ErrorMessage = "Tên sinh viên không được để trống")]
    [StringLength(200, ErrorMessage = "Tên sinh viên không được quá 200 ký tự")]
    public string Studentfullname { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "MSSV không được để trống")]
    [StringLength(50, ErrorMessage = "MSSV không được quá 50 ký tự")]
    public string Studentroll { get; set; } = string.Empty;
    
    public bool? Isactive { get; set; }
}

public class StudentResponseDto
{
    public int Studentid { get; set; }
    public string Studentfullname { get; set; } = string.Empty;
    public string Studentroll { get; set; } = string.Empty;
    public bool? Isactive { get; set; }
    public DateTime? Createat { get; set; }
    public List<string> GroupNames { get; set; } = new List<string>();
}

