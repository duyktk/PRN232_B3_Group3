using Repository.BussinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Repository
{
    public class ScanRepository : IScanRepository
    {
        private readonly Regex _connRegex = new Regex(
            @"(Data\s*Source|Server|Initial\s*Catalog|Database)\s*=",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private readonly string[] _allowedExtensions = { ".cs", ".json", ".config" };

        public async Task<bool> HasAppsettingsAsync(string projectPath)
        {
            var exists = Directory
                .GetFiles(projectPath, "appsettings.json", SearchOption.TopDirectoryOnly)
                .Any();

            return await Task.FromResult(exists);
        }

       
        public async Task<bool> HasConnectionStringInAppsettingsAsync(string projectPath)
        {
            var path = Directory
                .GetFiles(projectPath, "appsettings.json", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            if (path == null)
                return false;

            var json = await File.ReadAllTextAsync(path);

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.EnumerateObject()
                .Any(p => p.Name.Equals("ConnectionStrings", StringComparison.OrdinalIgnoreCase));
        }

        
        public async Task<HardCodeFinding?> FindHardcodedConnectionStringAsync(string projectPath)
        {
            var files = Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                // skip extension
                if (!_allowedExtensions.Contains(Path.GetExtension(file)))
                    continue;

                // skip appsettings.json
                if (Path.GetFileName(file)
                    .Equals("appsettings.json", StringComparison.OrdinalIgnoreCase))
                    continue;

                // skip bin / obj
                if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") ||
                    file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                    continue;

                string[] lines;
                try
                {
                    lines = await File.ReadAllLinesAsync(file);
                }
                catch
                {
                    continue; // file đang bị lock hoặc lỗi encoding
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    if (_connRegex.IsMatch(lines[i]))
                    {
                        return new HardCodeFinding
                        {
                            FileName = Path.GetFileName(file),
                            LineNumber = i + 1,
                            Preview = lines[i].Trim()
                        };
                    }
                }
            }

            return null;
        }
    }
}
