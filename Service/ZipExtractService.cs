using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ZipExtractService : IZipExtractService
    {
        public async Task<string> ExtractZipToTempAsync(
            Stream zipStream,
            string fileName,
            string tempRoot)
        {
            if (!Path.GetExtension(fileName)
                .Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only .zip files are supported.");
            }

            Directory.CreateDirectory(tempRoot);

            string zipPath = Path.Combine(tempRoot, fileName);

            using (var fs = new FileStream(zipPath, FileMode.Create))
            {
                await zipStream.CopyToAsync(fs);
            }

            ZipFile.ExtractToDirectory(zipPath, tempRoot);

            return ResolveProjectRoot(tempRoot);
        }

        private string ResolveProjectRoot(string extractedPath)
        {
            // Tìm appsettings.json ở MỌI CẤP
            var appsettingsFile = Directory
                .GetFiles(extractedPath, "appsettings.json", SearchOption.AllDirectories)
                .FirstOrDefault(f =>
                    Path.GetFileName(f)
                        .Equals("appsettings.json", StringComparison.OrdinalIgnoreCase));

            if (appsettingsFile != null)
            {
                // Trả về folder chứa appsettings.json
                return Path.GetDirectoryName(appsettingsFile)!;
            }

            // fallback: nếu không tìm thấy thì return root
            return extractedPath;
        }

    }
}