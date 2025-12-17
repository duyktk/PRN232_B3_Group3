using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IExamService
    {
        Task<Exam> GetByIdAsync(int id, bool includeDetails = true);
    }
}
