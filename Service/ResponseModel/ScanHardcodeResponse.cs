using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ResponseModel
{
    public class ScanHardcodeResponse
    {
        public bool HasAppSettings { get; set; }
        public bool HasConnectionStringInAppSettings { get; set; }
        public int? Point { get; set; }
        public string? ReasonWhyZero { get; set; }
        public bool IsPassed { get; set; }
    }

}
