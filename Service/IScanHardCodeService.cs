using Microsoft.AspNetCore.Http;
using Service.RequestModel;
using Service.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IScanHardCodeService
    {
        public Task<ScanHardcodeResponse> ScanHardCodeFromZipAsync(IFormFile file);
    }
}
