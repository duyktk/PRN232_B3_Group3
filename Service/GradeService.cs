using Repository;
using Repository.Models;
using Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class GradeService : IGradeService
    {
        private readonly UnitOfWork _unitOfWork;

        public GradeService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private const decimal MAX_TOTAL = 10.0m;

       
        public async Task<GradeResultDto> CreateGradeAsync(int myUserId, string myRole, int submissionId, GradeUpsertDto dto)
        {
            ValidateMainScores(dto);

            // Ensure submission exists
            var submission = await _unitOfWork.GetSubmissionByIdAsync(submissionId);
            if (submission == null)
                throw new Exception($"SubmissionId {submissionId} not found.");

            // Check existing grade by submissionId (same style as SubmissionService)
            var gradeRepo = _unitOfWork.GradeRepository;
            var existingGrade = gradeRepo.GetAll()
                .FirstOrDefault(g => g.SubmissionId == submissionId);

            if (existingGrade != null)
                throw new Exception("This submission already has a grade. Use UPDATE.");

            // Create grade
            var newGrade = new Grade
            {
                SubmissionId = submissionId,
                Q1 = dto.Q1,
                Q2 = dto.Q2,
                Q3 = dto.Q3,
                Q4 = dto.Q4,
                Q5 = dto.Q5,
                Q6 = dto.Q6,
                Totalscore = CalcTotal(dto),
                Status = dto.Status ?? "GRADED",
                Marker = myUserId,
                Createat = DateTime.UtcNow,
                Updateat = DateTime.UtcNow
            };

            await gradeRepo.CreateAsync(newGrade);
            await _unitOfWork.SaveAsync();

            // Create details after we have GradeId
            if (dto.Details != null && dto.Details.Any())
            {
                foreach (var d in dto.Details)
                {
                    await _unitOfWork.GradedetailRepository.CreateAsync(new Gradedetail
                    {
                        Gradeid = newGrade.Gradeid,
                        Qcode = d.Qcode,
                        Subcode = d.Subcode,
                        Point = d.Point,
                        Note = d.Note,
                        Createat = DateTime.UtcNow,
                        Updateat = DateTime.UtcNow
                    });
                }
                await _unitOfWork.SaveAsync();
            }

            // Return response format you want
            return new GradeResultDto
            {
                GradeId = newGrade.Gradeid,
                SubmissionId = submissionId,
                MarkerUserId = myUserId,
                Total = newGrade.Totalscore ?? 0,
                Status = newGrade.Status,
                UpdateAt = newGrade.Updateat
            };
        }

        // PUT behavior: update only if exist + role rules
        public async Task<GradeResultDto> UpdateGradeAsync(int myUserId, string myRole, int submissionId, GradeUpsertDto dto)
        {
            ValidateMainScores(dto);

            var submission = await _unitOfWork.GetSubmissionByIdAsync(submissionId);
            if (submission == null)
                throw new Exception($"SubmissionId {submissionId} not found.");

            var myRoleId = ParseRoleIdOrThrow(myRole, "Your role must be numeric (e.g. '2','3','4').");

            var gradeRepo = _unitOfWork.GradeRepository;
            var detailRepo = _unitOfWork.GradedetailRepository;

            // Load existing grade
            var existingGrade = gradeRepo.GetAll()
                .FirstOrDefault(g => g.SubmissionId == submissionId);

            if (existingGrade == null)
                throw new Exception("This submission has no grade yet. Use CREATE.");

            // Rule: if you are the current marker => cannot update again
            if (existingGrade.Marker.HasValue && existingGrade.Marker.Value == myUserId)
                throw new Exception("You already graded this submission. You cannot update again.");

            // Need current marker role from User.Role string (no DB change)
            int currentMarkerRoleId = await GetMarkerRoleIdAsync(existingGrade.Marker);

            // Rule: must be strictly higher
            if (myRoleId <= currentMarkerRoleId)
                throw new Exception($"Not allowed. Your role ({myRoleId}) must be higher than current marker role ({currentMarkerRoleId}).");

            // Update grade (override)
            existingGrade.Q1 = dto.Q1;
            existingGrade.Q2 = dto.Q2;
            existingGrade.Q3 = dto.Q3;
            existingGrade.Q4 = dto.Q4;
            existingGrade.Q5 = dto.Q5;
            existingGrade.Q6 = dto.Q6;
            existingGrade.Totalscore = CalcTotal(dto);
            existingGrade.Status = dto.Status ?? existingGrade.Status ?? "GRADED";
            existingGrade.Marker = myUserId;
            existingGrade.Updateat = DateTime.UtcNow;

            gradeRepo.Update(existingGrade);
            await _unitOfWork.SaveAsync();

            // Replace details: remove old details of this grade, add new
            var oldDetails = detailRepo.GetAll()
                .Where(d => d.Gradeid == existingGrade.Gradeid)
                .ToList();

            if (oldDetails.Any())
            {
                foreach (var od in oldDetails)
                    detailRepo.Remove(od);

                await _unitOfWork.SaveAsync();
            }

            if (dto.Details != null && dto.Details.Any())
            {
                foreach (var d in dto.Details)
                {
                    await detailRepo.CreateAsync(new Gradedetail
                    {
                        Gradeid = existingGrade.Gradeid,
                        Qcode = d.Qcode,
                        Subcode = d.Subcode,
                        Point = d.Point,
                        Note = d.Note,
                        Createat = DateTime.UtcNow,
                        Updateat = DateTime.UtcNow
                    });
                }
                await _unitOfWork.SaveAsync();
            }

            return new GradeResultDto
            {
                GradeId = existingGrade.Gradeid,
                SubmissionId = submissionId,
                MarkerUserId = myUserId,
                Total = existingGrade.Totalscore ?? 0,
                Status = existingGrade.Status,
                UpdateAt = existingGrade.Updateat
            };
        }

        private static decimal CalcTotal(GradeUpsertDto dto)
        {
            var total = dto.Q1 + dto.Q2 + dto.Q3 + dto.Q4 + dto.Q5 + dto.Q6;
            return total;
        }

        private static void ValidateMainScores(GradeUpsertDto dto)
        {
            if (dto.Q1 < 0 || dto.Q1 > 1.0m) throw new Exception("Q1 must be between 0 and 1.0");
            if (dto.Q2 < 0 || dto.Q2 > 2.0m) throw new Exception("Q2 must be between 0 and 2.0");
            if (dto.Q3 < 0 || dto.Q3 > 1.0m) throw new Exception("Q3 must be between 0 and 1.0");
            if (dto.Q4 < 0 || dto.Q4 > 2.5m) throw new Exception("Q4 must be between 0 and 2.5");
            if (dto.Q5 < 0 || dto.Q5 > 2.0m) throw new Exception("Q5 must be between 0 and 2.0");
            if (dto.Q6 < 0 || dto.Q6 > 1.5m) throw new Exception("Q6 must be between 0 and 1.5");

            var total = dto.Q1 + dto.Q2 + dto.Q3 + dto.Q4 + dto.Q5 + dto.Q6;
            if (total < 0 || total > MAX_TOTAL) throw new Exception("Total must be between 0 and 10.0");
        }

        private static int ParseRoleIdOrThrow(string? roleText, string message)
        {
            if (string.IsNullOrWhiteSpace(roleText) || !int.TryParse(roleText, out var roleId))
                throw new Exception(message);
            return roleId;
        }

        private async Task<int> GetMarkerRoleIdAsync(int? markerUserId)
        {
            if (!markerUserId.HasValue)
                throw new Exception("Existing grade has no marker user.");

            var marker = await _unitOfWork.GetUserByIdAsync(markerUserId.Value);
            if (marker == null)
                throw new Exception("Marker user not found.");

            return ParseRoleIdOrThrow(marker.Role, "Marker role is invalid (must be numeric string).");
        }
    }
}