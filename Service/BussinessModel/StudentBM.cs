using Repository.Models;

namespace Service.BussinessModel;

public class StudentBM
{
    public int Studentid { get; set; }
    public string Studentfullname { get; set; }
    public string Studentroll { get; set; }
    public bool? Isactive { get; set; }
    public DateTime? Createat { get; set; }

    public IReadOnlyList<GroupStudent> GroupStudents { get; set; }
    public IReadOnlyList<Submission> Submissions { get; set; }
}