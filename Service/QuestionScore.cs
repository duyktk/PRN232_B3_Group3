using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class QuestionScore
    {
        public string Qcode { get; set; }  // Mã câu hỏi
        public Dictionary<string, decimal> SubScores { get; set; } // Các tiêu chí nhỏ và điểm tối đa
        public decimal TotalScore => SubScores?.Values.Sum() ?? 0;

        public string DisplayName => Qcode switch
        {
            "Q1" => "Login",
            "Q2" => "List All",
            "Q3" => "Create function",
            "Q4" => "Update",
            "Q5" => "Search",
            "Q6" => "Delete",
            _ => Qcode ?? string.Empty
        };
    }
}
