using OfficeOpenXml;
using OfficeOpenXml.Style;
using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ExamExportService : IExamExportService
    {
        private readonly UnitOfWork _examRepository;

        public ExamExportService(UnitOfWork repository)
        {
            _examRepository = repository;
        }

        public async Task<byte[]> ExportExamScoresToExcelAsync(int examId)
        {
            var exam = await _examRepository.GetExamWithFullDataAsync(examId);
            ExcelPackage.License.SetNonCommercialPersonal("GROUP3");
            if (exam == null)
                throw new Exception($"Exam with ID {examId} not found");

            using var package = new ExcelPackage();
            CreateExamWorksheet(package, exam);
            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportMultipleExamsToExcelAsync(List<int> examIds)
        {
            using var package = new ExcelPackage();

            foreach (var examId in examIds)
            {
                var exam = await _examRepository.GetExamWithFullDataAsync(examId);
                if (exam != null)
                {
                    CreateExamWorksheet(package, exam);
                }
            }

            return package.GetAsByteArray();
        }

        private void CreateExamWorksheet(ExcelPackage package, Exam exam)
        {
            var sheetName = SanitizeSheetName("Marking Sheet");
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            SetupWorksheetColumns(worksheet);
            CreateHeaders(worksheet);
            FillStudentData(worksheet, exam);

            // Auto-fit rows
            //worksheet.Cells.AutoFitColumns();

            // Freeze panes
            worksheet.View.FreezePanes(4, 7);
        }

        private string SanitizeSheetName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Sheet1";

            // Remove invalid characters
            var invalidChars = new[] { '\\', '/', '?', '*', '[', ']' };
            foreach (var c in invalidChars)
            {
                name = name.Replace(c.ToString(), "");
            }

            // Limit to 31 characters
            if (name.Length > 31)
                name = name.Substring(0, 31);

            return name;
        }

        private void SetupWorksheetColumns(ExcelWorksheet worksheet)
        {
            worksheet.Column(1).Width = 5;
            worksheet.Column(2).Width = 10;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 25;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 12;

            // Score columns G->V
            for (int i = 8; i <= 21; i++)
            {
                worksheet.Column(i).Width = 8;
            }
            worksheet.Cells.AutoFitColumns();
            // Cột V rộng hơn
            worksheet.Column(22).Width = 15;
            worksheet.Column(23).Width = 8;
            worksheet.Column(24).Width = 15;
        }

        private void CreateHeaders(ExcelWorksheet worksheet)
        {
            // ===== ROW 1: Main Headers =====
            int col = 7;

            foreach (var question in MaxScoresData.MaxScores)
            {
                int startCol = col;
                int endCol = col + question.SubScores.Count - 1;

                string headerText = $"{question.DisplayName} ({question.TotalScore})";
                Color headerColor = GetHeaderColor(question.Qcode);

                CreateMergedHeader(worksheet, 1, startCol, endCol, headerText, headerColor);
                col = endCol + 1;
            }

            // Total
            CreateMergedHeader(worksheet, 1, 23, 23, "Total", Color.Yellow, 2);
            col++;

            // Comment merge row 1 + 2
            CreateMergedHeader(worksheet, 1, 24, 24,
                "Trường hợp 0 điểm, bắt buộc ghi chú lý do ở đây", Color.Yellow, 2);
            // Tăng chiều cao row 1
            worksheet.Row(1).Height = 20;
            // ===== ROW 2: Sub-headers with scores =====
            col = 7;
            foreach (var question in MaxScoresData.MaxScores)
            {
                Color colColor = GetHeaderColor(question.Qcode); // màu Q1-Q6
                foreach (var subScore in question.SubScores)
                {
                    var cell = worksheet.Cells[2, col];
                    cell.Value = $"{subScore.Key}\n({subScore.Value})";
                    cell.Style.Font.Size = 9;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.WrapText = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(colColor); // tô màu
                    SetBorder(cell);

                    col++;
                }
            }

            // Total row 2
            worksheet.Cells[2, 23].Value = "Total";
            //worksheet.Cells[2, 23].Style.Font.Bold = true;
            worksheet.Cells[2, 23].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 23].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            SetBorder(worksheet.Cells[2, 23]);

            // Comment row 2
            worksheet.Cells[2, 24].Value = "Comment";
            worksheet.Cells[2, 24].Style.Font.Bold = true;
            worksheet.Cells[2, 24].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 24].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            SetBorder(worksheet.Cells[2, 24]);

            // ===== ROW 3: Max scores và basic headers =====
            var basicHeaders = new Dictionary<int, string>
            {
                {1, "No"}, {2, "Group"}, {3, "Roll"}, {4, "FullName"},
                {5, "Solution"}, {6, "Marker"}
            };

            foreach (var kvp in basicHeaders)
            {
                var cell = worksheet.Cells[3, kvp.Key];
                cell.Value = kvp.Value;
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.White);
                SetBorder(cell);
            }

            col = 7;
            foreach (var question in MaxScoresData.MaxScores)
            {
                Color colColor = GetHeaderColor(question.Qcode); // màu Q1-Q6
                foreach (var subScore in question.SubScores)
                {
                    var cell = worksheet.Cells[3, col];
                    cell.Value = subScore.Value;
                    cell.Style.Font.Bold = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(colColor); // tô màu
                    cell.Style.Numberformat.Format = "0.00";
                    SetBorder(cell);

                    col++;
                }
            }
            worksheet.Cells[3, 1, 3, 24].AutoFilter = true;
            // Total max score row 3
            worksheet.Cells[3, 23].Value = MaxScoresData.GetTotalMaxScore();
            worksheet.Cells[3, 23].Style.Font.Bold = true;
            worksheet.Cells[3, 23].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[3, 23].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[3, 23].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[3, 23].Style.Fill.BackgroundColor.SetColor(Color.White);
            worksheet.Cells[3, 23].Style.Numberformat.Format = "0.00";
            SetBorder(worksheet.Cells[3, 23]);

            // Comment row 3
            worksheet.Cells[3, 24].Value = "Comment";
            worksheet.Cells[3, 24].Style.Font.Bold = true;
            worksheet.Cells[3, 24].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[3, 24].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[3, 24].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[3, 24].Style.Fill.BackgroundColor.SetColor(Color.White);
            SetBorder(worksheet.Cells[3, 24]);
        }

        private Color GetHeaderColor(string qcode)
        {
            return qcode switch
            {
                "Q1" => Color.FromArgb(217, 225, 242),
                "Q2" => Color.FromArgb(255, 242, 204),
                "Q3" => Color.FromArgb(217, 225, 242),
                "Q4" => Color.FromArgb(252, 228, 214),
                "Q5" => Color.FromArgb(226, 239, 218),
                "Q6" => Color.FromArgb(252, 228, 214),
                _ => Color.White
            };
        }

        private void FillStudentData(ExcelWorksheet worksheet, Exam exam)
        {
            int row = 4;
            int index = 1;

            var sortedSubmissions = exam.Submissions
                .Where(s => s.Student != null)
                .OrderBy(s => s.Student.GroupStudents.FirstOrDefault()?.Group?.Groupname ?? "")
                .ThenBy(s => s.Student.Studentroll ?? "")
                .ToList();

            foreach (var submission in sortedSubmissions)
            {
                var student = submission.Student;

                // Skip nếu student null
                if (student == null) continue;

                // Lấy groupname từ GroupStudents
                var groupName = student.GroupStudents.FirstOrDefault()?.Group?.Groupname ?? "";

                // Lấy Grade (chỉ có 1 Grade per Submission)
                var grade = submission.Grades?.FirstOrDefault();

                // Basic info
                worksheet.Cells[row, 1].Value = index++;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 2].Value = groupName;
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 3].Value = student.Studentroll ?? "";
                worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 4].Value = student.Studentfullname ?? "";
                worksheet.Cells[row, 5].Value = submission.Solution ?? "";
                worksheet.Cells[row, 6].Value = grade?.MarkerNavigation?.Username ?? "";
                worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Fill detail scores từ GradeDetails
                if (grade?.Gradedetails != null)
                {
                    FillDetailScores(worksheet, row, grade.Gradedetails);
                }

                // Total score từ Grade.TotalScore hoặc tính từ Q1-Q6
                decimal totalScore = grade?.Totalscore ??
                    ((grade?.Q1 ?? 0) + (grade?.Q2 ?? 0) + (grade?.Q3 ?? 0) +
                     (grade?.Q4 ?? 0) + (grade?.Q5 ?? 0) + (grade?.Q6 ?? 0));

                worksheet.Cells[row, 23].Value = totalScore;
                worksheet.Cells[row, 23].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 23].Style.Font.Bold = true;
                worksheet.Cells[row, 23].Style.Font.Color.SetColor(Color.Blue);
                worksheet.Cells[row, 23].Style.Numberformat.Format = "0.00";

                // Comment từ Submission.Comment
                if (!string.IsNullOrWhiteSpace(submission.Comment))
                {
                    worksheet.Cells[row, 24].Value = submission.Comment;
                }

                // Apply borders
                for (int c = 1; c <= 24; c++)
                {
                    SetBorder(worksheet.Cells[row, c]);
                }

                // Highlight nếu điểm thấp
                if (totalScore == 0)
                {
                    // Tô đỏ nhạt nếu = 0
                    for (int c = 1; c <= 24; c++)
                    {
                        worksheet.Cells[row, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, c].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }
                }
                //else if (totalScore < MaxScoresData.GetTotalMaxScore() * 0.5m)
                //{
                //    // Tô vàng nhạt nếu < 50%
                //    for (int c = 1; c <= 24; c++)
                //    {
                //        worksheet.Cells[row, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //        worksheet.Cells[row, c].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 235, 156));
                //    }
                //}

                row++;
            }
        }

        private void FillDetailScores(ExcelWorksheet worksheet, int row, ICollection<Gradedetail> gradeDetails)
        {
            if (gradeDetails == null || !gradeDetails.Any()) return;

            int col = 7;

            foreach (var question in MaxScoresData.MaxScores)
            {
                Color colColor = GetHeaderColor(question.Qcode); // màu theo Qcode

                foreach (var subScoreKvp in question.SubScores)
                {
                    string subKey = subScoreKvp.Key;
                    decimal maxScore = subScoreKvp.Value;

                    decimal actualScore = 0;

                    // Tìm GradeDetail theo QCode và SubCode
                    var detail = gradeDetails.FirstOrDefault(gd =>
                        gd.Qcode == question.Qcode && gd.Subcode == subKey);

                    if (detail != null)
                    {
                        actualScore = detail.Point ?? 0;
                    }

                    var cell = worksheet.Cells[row, col];
                    cell.Value = actualScore;
                    cell.Style.Numberformat.Format = "0.00";
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(colColor); // set màu theo Q

                    col++;
                }
            }
        }

        private void CreateMergedHeader(ExcelWorksheet ws, int startRow, int fromCol, int toCol, string text, Color bgColor, int rowSpan = 1)
        {
            var range = ws.Cells[startRow, fromCol, startRow + rowSpan - 1, toCol];
            range.Merge = true;
            range.Value = text;
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 10;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(bgColor);
            range.Style.WrapText = true;
            SetBorder(range);
        }

        private void SetBorder(ExcelRange range)
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Top.Color.SetColor(Color.Black);
            range.Style.Border.Left.Color.SetColor(Color.Black);
            range.Style.Border.Right.Color.SetColor(Color.Black);
            range.Style.Border.Bottom.Color.SetColor(Color.Black);
        }
    }
}
