using System;
using System.Drawing;
using System.IO;
using BimCommands.Tekla.IssueTracker.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BimCommands.Tekla.IssueTracker.Services
{
    public static class ExcelReportExporter
    {
        public static void Export(IssueProject project, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("BIM_Issue_Report");
                ws.View.ShowGridLines = true;

                // =====================================================================
                // 1. BANNER & PROJECT METADATA HEADER
                // =====================================================================
                ws.Cells["A1:K1"].Merge = true;
                ws.Cells["A1"].Value = "BÁO CÁO KIỂM TRA CHẤT LƯỢNG MÔ HÌNH BIM & BẢN VẼ (QA/QC SNAGGING REPORT)";
                ws.Cells["A1"].Style.Font.Size = 16;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Color.SetColor(Color.White);
                ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 23, 42)); // Slate 900
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(1).Height = 40;

                // Metadata Info Box
                ws.Cells["A2:B2"].Merge = true;
                ws.Cells["A2"].Value = "Tên Dự Án:";
                ws.Cells["A2"].Style.Font.Bold = true;
                ws.Cells["C2:E2"].Merge = true;
                ws.Cells["C2"].Value = project.ProjectName;

                ws.Cells["F2"].Value = "Người Kiểm Tra:";
                ws.Cells["F2"].Style.Font.Bold = true;
                ws.Cells["G2:H2"].Merge = true;
                ws.Cells["G2"].Value = project.Auditor;

                ws.Cells["I2"].Value = "Ngày Báo Cáo:";
                ws.Cells["I2"].Style.Font.Bold = true;
                ws.Cells["J2:K2"].Merge = true;
                ws.Cells["J2"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                ws.Cells["A3:B3"].Merge = true;
                ws.Cells["A3"].Value = "Mô Hình Tekla:";
                ws.Cells["A3"].Style.Font.Bold = true;
                ws.Cells["C3:E3"].Merge = true;
                ws.Cells["C3"].Value = string.IsNullOrEmpty(project.ModelName) ? "Tekla Structures 3D Model" : project.ModelName;

                // Statistics
                int totalCount = project.Issues.Count;
                int openCount = 0;
                int inProgressCount = 0;
                int resolvedCount = 0;

                foreach (var issue in project.Issues)
                {
                    if (issue.Status == "Resolved" || issue.Status == "Closed") resolvedCount++;
                    else if (issue.Status == "In Progress") inProgressCount++;
                    else openCount++;
                }

                // KPI Dashboard Cards
                void FormatKpi(string range, string title, string val, Color bg, Color fg)
                {
                    ws.Cells[range].Merge = true;
                    ws.Cells[range].Value = $"{title}: {val}";
                    ws.Cells[range].Style.Font.Bold = true;
                    ws.Cells[range].Style.Font.Size = 10;
                    ws.Cells[range].Style.Font.Color.SetColor(fg);
                    ws.Cells[range].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[range].Style.Fill.BackgroundColor.SetColor(bg);
                    ws.Cells[range].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[range].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                FormatKpi("A4:B4", "📦 TỔNG SỐ LỖI", totalCount.ToString(), Color.FromArgb(241, 245, 249), Color.FromArgb(30, 41, 59));
                FormatKpi("C4:D4", "🔴 CHƯA SỬA", openCount.ToString(), Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28));
                FormatKpi("E4:F4", "🟡 ĐANG SỬA", inProgressCount.ToString(), Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9));
                FormatKpi("G4:H4", "🟢 ĐÃ SỬA", resolvedCount.ToString(), Color.FromArgb(220, 252, 231), Color.FromArgb(21, 128, 61));

                double completionRate = totalCount > 0 ? (resolvedCount * 100.0 / totalCount) : 0;
                FormatKpi("I4:K4", "📈 TIẾN ĐỘ HOÀN THÀNH", $"{completionRate:0.0}%", Color.FromArgb(224, 231, 255), Color.FromArgb(67, 56, 202));
                ws.Row(4).Height = 26;

                // =====================================================================
                // 2. TABLE HEADERS (Row 6)
                // =====================================================================
                int headerRow = 6;
                string[] headers = new string[]
                {
                    "STT",
                    "Mã Issue",
                    "Nội Dung Lỗi & Hướng Dẫn Sửa",
                    "Vị Trí / Cấu Kiện (Tekla ID)",
                    "Mức Độ",
                    "ẢNH HIỆN TRẠNG (BEFORE)",
                    "ẢNH ĐÃ SỬA (AFTER)",
                    "Trạng Thái",
                    "Người Sửa",
                    "Ngày Tạo",
                    "Ngày Sửa"
                };

                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = ws.Cells[headerRow, c + 1];
                    cell.Value = headers[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 10;
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 41, 59)); // Slate 800
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(203, 213, 225));
                }
                ws.Row(headerRow).Height = 32;

                // Set Column Widths
                ws.Column(1).Width = 7;   // STT
                ws.Column(2).Width = 14;  // Ma Issue
                ws.Column(3).Width = 36;  // Noi dung
                ws.Column(4).Width = 24;  // Vi tri Tekla ID
                ws.Column(5).Width = 14;  // Muc do
                ws.Column(6).Width = 38;  // Hinh anh Before
                ws.Column(7).Width = 38;  // Hinh anh After
                ws.Column(8).Width = 15;  // Trang thai
                ws.Column(9).Width = 18;  // Nguoi sua
                ws.Column(10).Width = 18; // Ngay tao
                ws.Column(11).Width = 18; // Ngay sua

                // =====================================================================
                // 3. DATA ROWS & EMBEDDED PICTURES
                // =====================================================================
                int currentRow = headerRow + 1;
                int imageIndex = 1;

                for (int i = 0; i < project.Issues.Count; i++)
                {
                    var issue = project.Issues[i];
                    ws.Row(currentRow).Height = 140; // Ample row height for crisp picture display

                    // 1. STT
                    ws.Cells[currentRow, 1].Value = i + 1;
                    ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // 2. Code
                    ws.Cells[currentRow, 2].Value = issue.Code;
                    ws.Cells[currentRow, 2].Style.Font.Bold = true;
                    ws.Cells[currentRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // 3. Title & Description
                    string fullDesc = string.IsNullOrEmpty(issue.Description)
                        ? issue.Title
                        : $"{issue.Title}\r\n\r\n👉 Hướng dẫn: {issue.Description}";
                    ws.Cells[currentRow, 3].Value = fullDesc;
                    ws.Cells[currentRow, 3].Style.WrapText = true;

                    // 4. Location & Tekla IDs
                    string teklaIdStr = (issue.TeklaIds != null && issue.TeklaIds.Count > 0)
                        ? $"Tekla IDs: {string.Join(", ", issue.TeklaIds)}"
                        : "";
                    string locStr = string.IsNullOrEmpty(issue.Location)
                        ? teklaIdStr
                        : $"{issue.Location}\r\n{teklaIdStr}".Trim();
                    ws.Cells[currentRow, 4].Value = locStr;
                    ws.Cells[currentRow, 4].Style.WrapText = true;

                    // 5. Severity
                    ws.Cells[currentRow, 5].Value = issue.Severity;
                    ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    if (issue.Severity == "Critical")
                    {
                        ws.Cells[currentRow, 5].Style.Font.Bold = true;
                        ws.Cells[currentRow, 5].Style.Font.Color.SetColor(Color.FromArgb(185, 28, 28));
                    }

                    // 6. Before Image Embedding
                    if (issue.BeforeImage != null)
                    {
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                issue.BeforeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ms.Position = 0;
                                string picName = $"BeforePic_{imageIndex++}";
                                var picture = ws.Drawings.AddPicture(picName, ms);
                                picture.SetPosition(currentRow - 1, 6, 5, 6); // row, rowOffset, col, colOffset
                                picture.SetSize(220, 160);
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        ws.Cells[currentRow, 6].Value = "(Không có ảnh)";
                        ws.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    // 7. After Image Embedding
                    if (issue.AfterImage != null)
                    {
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                issue.AfterImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ms.Position = 0;
                                string picName = $"AfterPic_{imageIndex++}";
                                var picture = ws.Drawings.AddPicture(picName, ms);
                                picture.SetPosition(currentRow - 1, 6, 6, 6);
                                picture.SetSize(220, 160);
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        ws.Cells[currentRow, 7].Value = "Chưa cập nhật ảnh đã sửa\r\n(Nhân viên chụp dán vào đây)";
                        ws.Cells[currentRow, 7].Style.WrapText = true;
                        ws.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[currentRow, 7].Style.Font.Italic = true;
                        ws.Cells[currentRow, 7].Style.Font.Color.SetColor(Color.FromArgb(148, 163, 184));
                    }

                    // 8. Status
                    var statusCell = ws.Cells[currentRow, 8];
                    statusCell.Value = issue.Status;
                    statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    statusCell.Style.Font.Bold = true;
                    if (issue.Status == "Resolved" || issue.Status == "Closed")
                    {
                        statusCell.Style.Font.Color.SetColor(Color.FromArgb(21, 128, 61));
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 252, 231));
                    }
                    else if (issue.Status == "In Progress")
                    {
                        statusCell.Style.Font.Color.SetColor(Color.FromArgb(180, 83, 9));
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 243, 199));
                    }
                    else
                    {
                        statusCell.Style.Font.Color.SetColor(Color.FromArgb(185, 28, 28));
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 226, 226));
                    }

                    // 9. Assignee
                    ws.Cells[currentRow, 9].Value = issue.Assignee;
                    ws.Cells[currentRow, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // 10. Created Date
                    ws.Cells[currentRow, 10].Value = issue.CreatedDate.ToString("dd/MM/yyyy HH:mm");
                    ws.Cells[currentRow, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // 11. Modified Date
                    ws.Cells[currentRow, 11].Value = issue.ModifiedDate.ToString("dd/MM/yyyy HH:mm");
                    ws.Cells[currentRow, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Apply standard vertical centering and thin borders
                    for (int c = 1; c <= headers.Length; c++)
                    {
                        ws.Cells[currentRow, c].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[currentRow, c].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(226, 232, 240));
                    }

                    currentRow++;
                }

                // =====================================================================
                // 4. PRINT SETUP (A4 Landscape, Fit to Page Width)
                // =====================================================================
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.FitToPage = true;
                ws.PrinterSettings.FitToWidth = 1;
                ws.PrinterSettings.FitToHeight = 0;
                ws.PrinterSettings.RepeatRows = ws.Cells["6:6"];

                // Save to file
                FileInfo fi = new FileInfo(filePath);
                package.SaveAs(fi);
            }
        }
    }
}
