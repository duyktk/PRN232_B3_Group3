using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.RequestModel;
using Service.ResponseModel;

namespace PRN232_B3_Group3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private static readonly string[] AllowedExtensions = new[] { ".zip", ".rar" };
        private const long MaxFileSizeBytes = 1024L * 1024 * 1024; // 1 GB
        private readonly IExtractZipService _fileStorageService;
        private readonly ISubmissionService _submissionService;
        private readonly IScanHardCodeService _scanHardCodeService;
        public FilesController(IExtractZipService fileStorageService, IScanHardCodeService scanHardCodeService)
        {
            _fileStorageService = fileStorageService;
            _scanHardCodeService = scanHardCodeService;
        }

        [HttpPost("upload-archive")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadArchive([FromForm] FileUploadRequest request, CancellationToken cancellationToken)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.FailResponse("File is required."));
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return BadRequest(ApiResponse<string>.FailResponse("File is too large (max 1GB)."));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                return BadRequest(ApiResponse<string>.FailResponse("Only .zip or .rar files are allowed."));
            }

            await using var stream = file.OpenReadStream();
            var urls = await _fileStorageService.UploadArchiveAsync(
                stream,
                file.FileName,
                file.ContentType ?? string.Empty,
                request.Prefix ?? string.Empty,
                request.ExamId ?? 0,
                cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResponse(urls, "Uploaded"));
        }

        [HttpGet("submission/{submissionId:int}/download")]
        public async Task<IActionResult> DownloadSubmissionFile(int submissionId, CancellationToken cancellationToken)
        {
            var submission = await _submissionService.GetByIdAsync(submissionId);
            if (submission == null)
            {
                return NotFound(ApiResponse<string>.FailResponse($"Submission '{submissionId}' was not found."));
            }

            if (string.IsNullOrWhiteSpace(submission.Fileurl))
            {
                return BadRequest(ApiResponse<string>.FailResponse("Submission does not have an associated file."));
            }

            try
            {
                var download = await _fileStorageService.DownloadFileAsync(submission.Fileurl, cancellationToken);
                HttpContext.Response.RegisterForDispose(download);

                var suggestedName = download.FileName;
                if (!string.IsNullOrWhiteSpace(submission.Student?.Studentroll))
                {
                    var ext = Path.GetExtension(download.FileName);
                    suggestedName = $"{submission.Student.Studentroll}_submission{ext}";
                }

                return File(download.Stream, download.ContentType, suggestedName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(ApiResponse<string>.FailResponse("Stored file could not be found."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.FailResponse("Failed to download submission file."));
            }
        }

        [HttpPost("scan-hard-code-zip")]
        public async Task<IActionResult> ScanHardCodeZip(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Zip file is required.");

            var result = await _scanHardCodeService
                .ScanHardCodeFromZipAsync(file);

            return Ok(ApiResponse<ScanHardcodeResponse>
                .SuccessResponse(result, "Scan completed"));
        }
    }
}
