using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs
{
    public class GradeResultDto
    {
        public int GradeId { get; set; }
        public int SubmissionId { get; set; }
        public int MarkerUserId { get; set; }
        public decimal Total { get; set; }
        public string? Status { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
