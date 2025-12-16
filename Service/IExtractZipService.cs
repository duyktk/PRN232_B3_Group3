using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IExtractZipService
    {
        Task<IReadOnlyList<string>> UploadArchiveAsync(Stream contentStream, string fileName, string contentType, string prefix, int examId, CancellationToken cancellationToken = default);
        Task<FileDownloadResult> DownloadFileAsync(string fileLocation, CancellationToken cancellationToken = default);
    }
}
