using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.RequestModel
{
    public class FileUploadRequest
    {
        public IFormFile? File { get; set; }
        public string? Prefix { get; set; }
        public int? ExamId { get; set; }
    }
}
