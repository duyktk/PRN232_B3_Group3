using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface ISubmissionService
    {
        Task<Submission> GetByIdAsync(int id);
        Task<Submission> FindOrCreateSubmissionAsync(int examId, int studentId, string fileUrl, string studentRoll = null);

    }
}
