using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IZipExtractService
    {
        Task<string> ExtractZipToTempAsync(
            Stream zipStream,
            string fileName,
            string tempRoot
        );
    }
}
