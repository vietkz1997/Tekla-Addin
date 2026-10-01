using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitQtoTool
{
    public class AppRibbon : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                string tabName = "BIM QTO PRO";
                
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    // Ignore if tab already exists
                }

                RibbonPanel panel = application.CreateRibbonPanel(tabName, "Bóc tách & Va chạm");

                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = System.IO.Path.GetDirectoryName(assemblyPath);
                string iconPath = System.IO.Path.Combine(assemblyDir, "RevitQtoTool-40x40.png");

                var buttonData = new PushButtonData(
                    "btnExportBoq",
                    "Xuất BOQ &\nKiểm tra Va chạm",
                    assemblyPath,
                    "RevitQtoTool.Commands.ExportBoqCommand"
                )
                {
                    ToolTip = "Trích xuất khối lượng Bê tông, Ván khuôn, Cốt thép và kiểm tra va chạm cấu kiện trùng lặp.",
                    LongDescription = "Hỗ trợ tự động trừ giao cắt Cột - Dầm - Sàn - Vách, bóc tách cốt thép chuẩn TCVN và xuất file Excel thông minh."
                };

                if (System.IO.File.Exists(iconPath))
                {
                    try
                    {
                        buttonData.LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
                    }
                    catch
                    {
                        // Fallback gracefully if image cannot be decoded
                    }
                }

                panel.AddItem(buttonData);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ribbon Error", ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
