using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.ResponseModel;

namespace PRN232_B3_Group3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _service;
        private readonly IExamExportService _exportService;

        public ExamController(IExamService service, IExamExportService exportService)
        {
            _service = service;
            _exportService = exportService;
        }

        [HttpGet("export/{examId}")]
        public async Task<IActionResult> ExportExamScores(int examId)
        {
            try
            {
                // Lấy thông tin kỳ thi
                var exam = await _service.GetByIdAsync(examId, includeDetails: true);
                if (exam == null)
                    return NotFound(ApiResponse<string>.FailResponse($"Exam with ID {examId} not found"));

                var fileBytes = await _exportService.ExportExamScoresToExcelAsync(examId);

                // Lấy tên kỳ thi và ngày thi
                var fileName = $"{exam.Examname}_Marking_Sheet.xlsx";

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse(
                    $"Error exporting exam: {ex.Message}"));
            }
        }
    }
}
