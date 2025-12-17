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

        public async Task<Submission> GetByIdAsync(int id)
        => await _unitOfWork.GetSubmissionByIdAsync(id);

        public async Task<Submission> FindOrCreateSubmissionAsync(int examId, int studentId, string fileUrl, string studentRoll = null)
        {
            var repo = _unitOfWork.SubmissionRepository;
            var existing = _unitOfWork.SubmissionRepository.GetAll()
                .FirstOrDefault(s => s.Examid == examId && s.Studentid == studentId);

            if (existing != null)
            {
                bool needsUpdate = false;

                // Update FileUrl if provided
                if (!string.IsNullOrWhiteSpace(fileUrl) && existing.Fileurl != fileUrl)
                {
                    existing.Fileurl = fileUrl;
                    needsUpdate = true;
                }

                // Update Solution with studentRoll if provided
                if (!string.IsNullOrWhiteSpace(studentRoll) && existing.Solution != studentRoll)
                {
                    existing.Solution = studentRoll;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    existing.Updateat = DateTime.UtcNow;
                    repo.Update(existing);
                    await _unitOfWork.SaveAsync();
                }

                return existing;
            }

            // Create new submission
            var newSubmission = new Submission
            {
                Examid = examId,
                Studentid = studentId,
                Fileurl = fileUrl,
                Solution = studentRoll,
                Createat = DateTime.UtcNow
            };

            await _unitOfWork.SubmissionRepository.CreateAsync(newSubmission);

            return newSubmission;
        }
    }
}
