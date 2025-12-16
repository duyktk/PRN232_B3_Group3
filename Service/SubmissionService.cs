using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SubmissionService : ISubmissionService
    {
        private readonly UnitOfWork _unitOfWork;

        public SubmissionService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Submission> GetByIdAsync(int id, bool includeDetails = true)
        => await _unitOfWork.GetSubmissionByIdAsync(id, includeDetails = true);
    }
}
