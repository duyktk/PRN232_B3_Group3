using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ExamService : IExamService
    {
        private readonly UnitOfWork _unitOfWork;

        public ExamService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Exam> GetByIdAsync(int id, bool includeDetails = true)
        => await _unitOfWork.GetExamByIdAsync(id, includeDetails = true);
    }
}
