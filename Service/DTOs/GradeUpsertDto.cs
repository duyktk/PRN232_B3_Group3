using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs
{
    public class GradeUpsertDto
    {
        public int SubmissionId { get; set; }

        public decimal Q1 { get; set; }
        public decimal Q2 { get; set; }
        public decimal Q3 { get; set; }
        public decimal Q4 { get; set; }
        public decimal Q5 { get; set; }
        public decimal Q6 { get; set; }

        public string? Status { get; set; }

        public List<GradeDetailUpsertDto> Details { get; set; } = new();
    }

    public class GradeDetailUpsertDto
    {
        public string Qcode { get; set; } = default!;
        public string Subcode { get; set; } = default!;
        public decimal Point { get; set; }
        public string? Note { get; set; }
    }
}