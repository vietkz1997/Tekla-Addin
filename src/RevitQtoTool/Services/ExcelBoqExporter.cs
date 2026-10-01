using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using RevitQtoTool.Models;

namespace RevitQtoTool.Services
{
    public class ExcelBoqExporter
    {
        public void ExportBoqToExcel(string filePath, IEnumerable<BoqItem> items, IEnumerable<RebarBoqItem> rebarItems = null, string projectName = "DỰ ÁN XÂY DỰNG")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Export path cannot be empty.", nameof(filePath));

            var itemList = items?.ToList() ?? new List<BoqItem>();
            var rebars = rebarItems?.ToList() ?? new List<RebarBoqItem>();

            using (var workbook = new XLWorkbook())
            {
                // ==================== SHEET 1: BE TONG & VAN KHUON ====================
                var ws = workbook.Worksheets.Add("BOQ_Betong_Vankhuon");
                ws.ShowGridLines = true;

                // 1. Title & Info
                ws.Cell("B2").Value = "BẢNG TỔNG HỢP KHỐI LƯỢNG BÊ TÔNG & VÁN KHUÔN";
                ws.Cell("B2").Style.Font.Bold = true;
                ws.Cell("B2").Style.Font.FontSize = 16;
                ws.Cell("B2").Style.Font.FontColor = XLColor.FromHtml("#1E293B");

                ws.Cell("B3").Value = $"Dự án: {projectName.ToUpper()}";
                ws.Cell("B3").Style.Font.Italic = true;
                ws.Cell("B3").Style.Font.FontSize = 11;

                ws.Cell("B4").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} | Xuất tự động từ Revit QTO Tool";
                ws.Cell("B4").Style.Font.FontSize = 10;
                ws.Cell("B4").Style.Font.FontColor = XLColor.Gray;

                // 2. Table Headers
                int headerRow = 6;
                string[] headers = new[]
                {
                    "STT", "Revit ID", "Hạng mục (Category)", "Tiết diện / Family Type",
                    "Tầng (Level)", "Mác vật liệu", "Thể tích Bê tông (m³)", "Diện tích Ván khuôn (m²)", "Ghi chú"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = ws.Cell(headerRow, col + 2);
                    cell.Value = headers[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
                }
                ws.Row(headerRow).Height = 26;

                // 3. Data Rows & Grouping
                int currentRow = headerRow + 1;
                int itemIndex = 1;
                var levelGroups = itemList.GroupBy(x => x.LevelName).OrderBy(g => g.Key);

                foreach (var group in levelGroups)
                {
                    int groupStartRow = currentRow;

                    foreach (var item in group)
                    {
                        ws.Cell(currentRow, 2).Value = itemIndex++;
                        ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(currentRow, 3).Value = item.ElementId;
                        ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(currentRow, 4).Value = item.CategoryName;
                        ws.Cell(currentRow, 5).Value = item.TypeName;
                        ws.Cell(currentRow, 6).Value = item.LevelName;
                        ws.Cell(currentRow, 7).Value = item.MaterialName;

                        var volCell = ws.Cell(currentRow, 8);
                        volCell.Value = item.NetVolumeM3;
                        volCell.Style.NumberFormat.Format = "#,##0.000";

                        var formCell = ws.Cell(currentRow, 9);
                        formCell.Value = item.FormworkAreaM2;
                        formCell.Style.NumberFormat.Format = "#,##0.00";

                        ws.Cell(currentRow, 10).Value = item.Comments;

                        ws.Range(currentRow, 2, currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(currentRow, 2, currentRow, 10).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(currentRow, 2, currentRow, 10).Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                        ws.Range(currentRow, 2, currentRow, 10).Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");

                        currentRow++;
                    }

                    int groupEndRow = currentRow - 1;

                    // Level Subtotal
                    int subtotalRow = currentRow;
                    ws.Cell(subtotalRow, 4).Value = $"TỔNG CỘNG {group.Key.ToUpper()}";
                    ws.Range(subtotalRow, 4, subtotalRow, 7).Merge();
                    ws.Cell(subtotalRow, 4).Style.Font.Bold = true;

                    ws.Cell(subtotalRow, 8).FormulaA1 = $"=SUBTOTAL(9, H{groupStartRow}:H{groupEndRow})";
                    ws.Cell(subtotalRow, 8).Style.NumberFormat.Format = "#,##0.000";
                    ws.Cell(subtotalRow, 8).Style.Font.Bold = true;

                    ws.Cell(subtotalRow, 9).FormulaA1 = $"=SUBTOTAL(9, I{groupStartRow}:I{groupEndRow})";
                    ws.Cell(subtotalRow, 9).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(subtotalRow, 9).Style.Font.Bold = true;

                    var subtotalRange = ws.Range(subtotalRow, 2, subtotalRow, 10);
                    subtotalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDF2F7");
                    subtotalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    subtotalRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");

                    ws.Rows(groupStartRow, groupEndRow).Group();
                    ws.Rows(groupStartRow, groupEndRow).Collapse();

                    currentRow++;
                }

                // Grand Total
                if (itemList.Any())
                {
                    int grandTotalRow = currentRow;
                    ws.Cell(grandTotalRow, 2).Value = "TỔNG TOÀN DỰ ÁN";
                    ws.Range(grandTotalRow, 2, grandTotalRow, 7).Merge();
                    ws.Cell(grandTotalRow, 2).Style.Font.Bold = true;
                    ws.Cell(grandTotalRow, 2).Style.Font.FontSize = 12;
                    ws.Cell(grandTotalRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(grandTotalRow, 8).FormulaA1 = $"=SUBTOTAL(9, H7:H{grandTotalRow - 1})";
                    ws.Cell(grandTotalRow, 8).Style.NumberFormat.Format = "#,##0.000";
                    ws.Cell(grandTotalRow, 8).Style.Font.Bold = true;
                    ws.Cell(grandTotalRow, 8).Style.Font.FontSize = 12;

                    ws.Cell(grandTotalRow, 9).FormulaA1 = $"=SUBTOTAL(9, I7:I{grandTotalRow - 1})";
                    ws.Cell(grandTotalRow, 9).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(grandTotalRow, 9).Style.Font.Bold = true;
                    ws.Cell(grandTotalRow, 9).Style.Font.FontSize = 12;

                    var grandTotalRange = ws.Range(grandTotalRow, 2, grandTotalRow, 10);
                    grandTotalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#C7D2FE");
                    grandTotalRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    grandTotalRange.Style.Border.BottomBorder = XLBorderStyleValues.Double;

                    ws.Columns(2, 10).AdjustToContents(headerRow, grandTotalRow);
                }

                ws.Column(2).Width = 7;
                ws.Column(3).Width = 11;

                // ==================== SHEET 2: COT THEP (REBAR) ====================
                if (rebars.Any())
                {
                    var wsRebar = workbook.Worksheets.Add("BOQ_CotThep");
                    wsRebar.ShowGridLines = true;

                    wsRebar.Cell("B2").Value = "BẢNG TỔNG HỢP KHỐI LƯỢNG CỐT THÉP (TCVN)";
                    wsRebar.Cell("B2").Style.Font.Bold = true;
                    wsRebar.Cell("B2").Style.Font.FontSize = 15;
                    wsRebar.Cell("B2").Style.Font.FontColor = XLColor.FromHtml("#92400E");

                    string[] rebarHeaders = new[]
                    {
                        "STT", "Revit ID", "Cấu kiện chủ", "Tầng", "Mác thép",
                        "Đường kính", "Nhóm Φ (TCVN)", "Số thanh", "Chiều dài (m)", "Tổng KL (kg)"
                    };

                    int rHeadRow = 5;
                    for (int col = 0; col < rebarHeaders.Length; col++)
                    {
                        var cell = wsRebar.Cell(rHeadRow, col + 2);
                        cell.Value = rebarHeaders[col];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D97706");
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    int rRow = rHeadRow + 1;
                    int rIdx = 1;
                    foreach (var r in rebars)
                    {
                        wsRebar.Cell(rRow, 2).Value = rIdx++;
                        wsRebar.Cell(rRow, 3).Value = r.ElementId;
                        wsRebar.Cell(rRow, 4).Value = r.HostCategory;
                        wsRebar.Cell(rRow, 5).Value = r.LevelName;
                        wsRebar.Cell(rRow, 6).Value = r.RebarType;
                        wsRebar.Cell(rRow, 7).Value = $"Φ{r.DiameterMm}";
                        wsRebar.Cell(rRow, 8).Value = r.DiameterGroup;
                        wsRebar.Cell(rRow, 9).Value = r.Quantity;

                        var lenCell = wsRebar.Cell(rRow, 10);
                        lenCell.Value = r.TotalLengthM;
                        lenCell.Style.NumberFormat.Format = "#,##0.00";

                        var wtCell = wsRebar.Cell(rRow, 11);
                        wtCell.Value = r.TotalWeightKg;
                        wtCell.Style.NumberFormat.Format = "#,##0.00";

                        wsRebar.Range(rRow, 2, rRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        wsRebar.Range(rRow, 2, rRow, 11).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        wsRebar.Range(rRow, 2, rRow, 11).Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                        rRow++;
                    }

                    // Grand total rebar
                    wsRebar.Cell(rRow, 2).Value = "TỔNG CỘNG THÉP (KG)";
                    wsRebar.Range(rRow, 2, rRow, 10).Merge();
                    wsRebar.Cell(rRow, 2).Style.Font.Bold = true;

                    wsRebar.Cell(rRow, 11).FormulaA1 = $"=SUM(K{rHeadRow + 1}:K{rRow - 1})";
                    wsRebar.Cell(rRow, 11).Style.NumberFormat.Format = "#,##0.00";
                    wsRebar.Cell(rRow, 11).Style.Font.Bold = true;

                    var rebarTotalRange = wsRebar.Range(rRow, 2, rRow, 11);
                    rebarTotalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                    rebarTotalRange.Style.Border.BottomBorder = XLBorderStyleValues.Double;

                    wsRebar.Columns(2, 11).AdjustToContents();
                }

                workbook.SaveAs(filePath);
            }

            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Cannot open Excel]: {ex.Message}");
            }
        }
    }
}
