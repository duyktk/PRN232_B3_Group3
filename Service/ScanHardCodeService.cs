using Microsoft.AspNetCore.Http;
using Repository;
using Service.RequestModel;
using Service.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ScanHardCodeService : IScanHardCodeService
    {
        private readonly IScanRepository _scanRepository;

        private readonly IZipExtractService _zipExtractService;

        public ScanHardCodeService(IScanRepository scanRepository, IZipExtractService zipExtractService)
        {
            _scanRepository = scanRepository;
            _zipExtractService = zipExtractService;
        }

        public async Task<ScanHardcodeResponse> ScanHardCodeFromZipAsync(IFormFile zipFile)
        {
            string tempRoot = null!;
            try
            {
                tempRoot = Path.Combine(
                    Path.GetTempPath(),
                    "scan_" + Guid.NewGuid()
                );

                string projectPath =
                    await _zipExtractService.ExtractZipToTempAsync(
                        zipFile.OpenReadStream(),
                        zipFile.FileName,
                        tempRoot
                    );

                return await SCanHardCodeAsync(new ScanHardcodeRequest
                {
                    ProjectPath = projectPath
                });
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempRoot) && Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }


        public async Task<ScanHardcodeResponse> SCanHardCodeAsync(ScanHardcodeRequest request)
        {
            string projectPath = request.ProjectPath;

            bool hasAppSettings =
                await _scanRepository.HasAppsettingsAsync(projectPath);

            if (!hasAppSettings)
            {
                return new ScanHardcodeResponse
                {
                    HasAppSettings = false,
                    HasConnectionStringInAppSettings = false,
                    Point = 0,
                    ReasonWhyZero = "No appsettings.json file found.",
                    IsPassed = false
                };
            }

            bool hasConnectionString =
                await _scanRepository.HasConnectionStringInAppsettingsAsync(projectPath);

            if (!hasConnectionString)
            {
                return new ScanHardcodeResponse
                {
                    HasAppSettings = true,
                    HasConnectionStringInAppSettings = false,
                    Point = 0,
                    ReasonWhyZero = "No connection string found in appsettings.json.",
                    IsPassed = false
                };
            }

            var finding =
                await _scanRepository.FindHardcodedConnectionStringAsync(projectPath);

            if (finding != null)
            {
                return new ScanHardcodeResponse
                {
                    HasAppSettings = true,
                    HasConnectionStringInAppSettings = true,
                    Point = 0,
                    ReasonWhyZero =
                        $"Hardcoded ConnectionString found in '{finding.FileName}' " +
                        $"(line {finding.LineNumber})",
                    IsPassed = false
                };
            }

            return new ScanHardcodeResponse
            {
                HasAppSettings = true,
                HasConnectionStringInAppSettings = true,
                Point = null,
                ReasonWhyZero = null,
                IsPassed = true
            };
        }

    }

}
