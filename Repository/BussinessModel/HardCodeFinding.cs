using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.BussinessModel
{
    public class HardCodeFinding
    {
        public string FileName { get; set; } = null!;
        public int LineNumber { get; set; }
        public string Preview { get; set; } = null!;
    }
}
