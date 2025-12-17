using Repository.BussinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IScanRepository
    {
        Task<bool> HasAppsettingsAsync(string projectPath);
        Task<bool> HasConnectionStringInAppsettingsAsync(string projectPath);
        Task<HardCodeFinding?> FindHardcodedConnectionStringAsync(string projectPath);

    }
}
