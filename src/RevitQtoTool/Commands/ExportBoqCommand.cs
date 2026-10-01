using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitQtoTool.Views;

namespace RevitQtoTool.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportBoqCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData?.Application;
                UIDocument uidoc = uiapp?.ActiveUIDocument;
                Document doc = uidoc?.Document;

                if (doc == null)
                {
                    TaskDialog.Show("Thông báo", "Vui lòng mở một dự án Revit trước khi chạy công cụ.");
                    return Result.Cancelled;
                }

                if (doc.IsFamilyDocument)
                {
                    TaskDialog.Show("Thông báo", "Công cụ bóc tách khối lượng chỉ áp dụng cho môi trường Dự án (Project), không áp dụng cho Family Editor.");
                    return Result.Cancelled;
                }

                var window = new QtoWindow(uiapp);
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Lỗi thực thi", $"Không thể mở công cụ BOQ: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}
