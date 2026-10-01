using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using RevitQtoTool.Models;

namespace RevitQtoTool.ViewModels
{
    public class MainQtoViewModel : INotifyPropertyChanged
    {
        private readonly object _revitContext;

        public ObservableCollection<SelectionItem> Levels { get; set; } = new ObservableCollection<SelectionItem>();
        public ObservableCollection<SelectionItem> Categories { get; set; } = new ObservableCollection<SelectionItem>();
        public ObservableCollection<BoqItem> ExtractedItems { get; set; } = new ObservableCollection<BoqItem>();
        public ObservableCollection<RebarBoqItem> ExtractedRebars { get; set; } = new ObservableCollection<RebarBoqItem>();
        public ObservableCollection<OverlapIssueItem> OverlapIssues { get; set; } = new ObservableCollection<OverlapIssueItem>();

        private bool _includeLinks;
        public bool IncludeLinks
        {
            get => _includeLinks;
            set { _includeLinks = value; OnPropertyChanged(); }
        }

        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set { _progressPercentage = value; OnPropertyChanged(); }
        }

        private string _statusMessage = "Sẵn sàng làm việc";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public int TotalElements => ExtractedItems.Count;
        public double TotalVolumeM3 => Math.Round(ExtractedItems.Sum(x => x.NetVolumeM3), 2);
        public double TotalFormworkM2 => Math.Round(ExtractedItems.Sum(x => x.FormworkAreaM2), 2);
        public double TotalRebarTons => Math.Round(ExtractedRebars.Sum(x => x.TotalWeightKg) / 1000.0, 3);
        public int TotalOverlapIssues => OverlapIssues.Count;

        public double RebarD10Kg => Math.Round(ExtractedRebars.Where(r => r.DiameterMm <= 10.01).Sum(r => r.TotalWeightKg), 1);
        public double RebarD18Kg => Math.Round(ExtractedRebars.Where(r => r.DiameterMm > 10.01 && r.DiameterMm <= 18.01).Sum(r => r.TotalWeightKg), 1);
        public double RebarDOver18Kg => Math.Round(ExtractedRebars.Where(r => r.DiameterMm > 18.01).Sum(r => r.TotalWeightKg), 1);

        public ICommand ScanModelCommand { get; }
        public ICommand ScanOverlapsCommand { get; }
        public ICommand AutoJoinAllCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ToggleAllLevelsCommand { get; }

        public MainQtoViewModel(object revitContext = null)
        {
            _revitContext = revitContext;

            ScanModelCommand = new RelayCommand(_ => ExecuteScanModel(), _ => !IsBusy);
            ScanOverlapsCommand = new RelayCommand(_ => ExecuteScanOverlaps(), _ => !IsBusy);
            AutoJoinAllCommand = new RelayCommand(_ => ExecuteAutoJoinAll(), _ => !IsBusy && OverlapIssues.Any());
            ExportExcelCommand = new RelayCommand(_ => ExecuteExportExcel(), _ => !IsBusy && (ExtractedItems.Any() || ExtractedRebars.Any()));
            ToggleAllLevelsCommand = new RelayCommand(param => ToggleAll(Levels, Convert.ToBoolean(param)));

            if (_revitContext != null)
            {
                InitializeFromRevitSafe(_revitContext);
            }
            else
            {
                InitializeDemoData();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeFromRevitSafe(object context)
        {
            try
            {
                if (context is Autodesk.Revit.UI.UIApplication uiapp)
                {
                    var doc = uiapp.ActiveUIDocument?.Document;
                    if (doc != null)
                    {
                        Categories.Add(new SelectionItem { Name = "Cột (Columns)", CategoryId = (long)Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns, IsSelected = true });
                        Categories.Add(new SelectionItem { Name = "Dầm (Framing)", CategoryId = (long)Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming, IsSelected = true });
                        Categories.Add(new SelectionItem { Name = "Sàn (Floors)", CategoryId = (long)Autodesk.Revit.DB.BuiltInCategory.OST_Floors, IsSelected = true });
                        Categories.Add(new SelectionItem { Name = "Vách (Walls)", CategoryId = (long)Autodesk.Revit.DB.BuiltInCategory.OST_Walls, IsSelected = true });
                        Categories.Add(new SelectionItem { Name = "Móng (Foundations)", CategoryId = (long)Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFoundation, IsSelected = true });

                        var levels = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                            .OfClass(typeof(Autodesk.Revit.DB.Level))
                            .Cast<Autodesk.Revit.DB.Level>()
                            .OrderBy(l => l.Elevation);

                        foreach (var lvl in levels)
                        {
                            Levels.Add(new SelectionItem { Name = lvl.Name, IsSelected = true });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowDialog("Lỗi Revit", ex.Message);
            }
        }

        private void InitializeDemoData()
        {
            StatusMessage = "Chế độ Standalone (.exe) - Xem trước giao diện & Dữ liệu mô phỏng";

            Categories.Add(new SelectionItem { Name = "Cột (Columns)", CategoryId = -2001330, IsSelected = true });
            Categories.Add(new SelectionItem { Name = "Dầm (Framing)", CategoryId = -2001320, IsSelected = true });
            Categories.Add(new SelectionItem { Name = "Sàn (Floors)", CategoryId = -2000032, IsSelected = true });
            Categories.Add(new SelectionItem { Name = "Vách (Walls)", CategoryId = -2000011, IsSelected = true });
            Categories.Add(new SelectionItem { Name = "Móng (Foundations)", CategoryId = -2001300, IsSelected = true });

            string[] sampleLevels = new[] { "Tầng Hầm (Basement)", "Tầng 1 (Ground Floor)", "Tầng 2 (Level 2)", "Tầng 3 (Level 3)", "Tầng Mái (Roof)" };
            foreach (var lvl in sampleLevels)
            {
                Levels.Add(new SelectionItem { Name = lvl, IsSelected = true });
            }

            ExtractedItems.Add(new BoqItem { ElementId = 101230, CategoryName = "Structural Columns", FamilyName = "Cột Bê Tông", TypeName = "C600x600", LevelName = "Tầng 1 (Ground Floor)", MaterialName = "Bê tông B30", NetVolumeM3 = 1.440, FormworkAreaM2 = 9.60, Comments = "Cột trục A1" });
            ExtractedItems.Add(new BoqItem { ElementId = 101235, CategoryName = "Structural Framing", FamilyName = "Dầm Bê Tông", TypeName = "B300x600", LevelName = "Tầng 1 (Ground Floor)", MaterialName = "Bê tông B30", NetVolumeM3 = 1.080, FormworkAreaM2 = 7.20, Comments = "Dầm D1" });
            ExtractedItems.Add(new BoqItem { ElementId = 101240, CategoryName = "Floors", FamilyName = "Sàn Kết Cấu", TypeName = "Slab 150mm", LevelName = "Tầng 1 (Ground Floor)", MaterialName = "Bê tông B25", NetVolumeM3 = 7.500, FormworkAreaM2 = 50.00, Comments = "Sàn ô S1" });
            ExtractedItems.Add(new BoqItem { ElementId = 101250, CategoryName = "Walls", FamilyName = "Vách Bê Tông", TypeName = "Wall 250mm", LevelName = "Tầng Hầm (Basement)", MaterialName = "Bê tông B30", NetVolumeM3 = 6.250, FormworkAreaM2 = 50.00, Comments = "Vách hầm V1" });

            ExtractedRebars.Add(new RebarBoqItem { ElementId = 205100, HostCategory = "Structural Columns", LevelName = "Tầng 1 (Ground Floor)", RebarType = "CB400-V", DiameterMm = 20, Quantity = 8, SingleLengthM = 4.2, TotalLengthM = 33.6, TotalWeightKg = 82.86 });
            ExtractedRebars.Add(new RebarBoqItem { ElementId = 205105, HostCategory = "Structural Columns", LevelName = "Tầng 1 (Ground Floor)", RebarType = "CB300-V", DiameterMm = 10, Quantity = 25, SingleLengthM = 2.1, TotalLengthM = 52.5, TotalWeightKg = 32.36 });
            ExtractedRebars.Add(new RebarBoqItem { ElementId = 205110, HostCategory = "Structural Framing", LevelName = "Tầng 1 (Ground Floor)", RebarType = "CB400-V", DiameterMm = 16, Quantity = 6, SingleLengthM = 6.5, TotalLengthM = 39.0, TotalWeightKg = 61.54 });

            NotifySummaryChanged();
        }

        private void ExecuteScanModel()
        {
            if (_revitContext == null)
            {
                ShowDialog("Thông báo", "Bạn đang chạy ở chế độ Standalone. Dữ liệu mô phỏng đã được nạp sẵn để xem thử.\nĐể bóc tách mô hình thật, hãy mở công cụ từ Ribbon trong Autodesk Revit.");
                return;
            }

            IsBusy = true;
            ExtractedItems.Clear();
            ExtractedRebars.Clear();
            ProgressPercentage = 0;
            StatusMessage = "Đang quét danh sách tài liệu...";

            try
            {
                ExecuteScanModelSafe(_revitContext);
            }
            catch (Exception ex)
            {
                ShowDialog("Lỗi", $"Bóc tách thất bại: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExecuteScanModelSafe(object context)
        {
            var uiapp = context as Autodesk.Revit.UI.UIApplication;
            var doc = uiapp?.ActiveUIDocument?.Document;
            if (doc == null) return;

            var collector = new Services.RevitConcreteCollector();
            var formworkCalc = new Services.RevitFormworkCalculator();
            var rebarCollector = new Services.RevitRebarCollector();

            var targetDocs = new List<Autodesk.Revit.DB.Document> { doc };
            if (IncludeLinks)
            {
                var links = Services.RevitLinkHelper.GetLoadedLinks(doc);
                foreach (var link in links)
                {
                    if (link.LinkDoc != null && !targetDocs.Any(d => d.Title == link.LinkDoc.Title))
                    {
                        targetDocs.Add(link.LinkDoc);
                    }
                }
            }

            var selectedLevelNames = new HashSet<string>(Levels.Where(l => l.IsSelected).Select(l => l.Name));
            var tempConcreteList = new List<BoqItem>();

            foreach (var d in targetDocs)
            {
                var items = collector.CollectConcreteElements(d);
                tempConcreteList.AddRange(items.Where(i => selectedLevelNames.Contains(i.LevelName)));
            }

            int totalConcrete = tempConcreteList.Count;
            for (int i = 0; i < totalConcrete; i++)
            {
                var item = tempConcreteList[i];
                var owningDoc = targetDocs.FirstOrDefault(d => d.GetElement(new Autodesk.Revit.DB.ElementId((long)item.ElementId)) != null) ?? doc;
                var elem = owningDoc.GetElement(new Autodesk.Revit.DB.ElementId((long)item.ElementId));

                if (elem != null)
                {
                    var neighbors = formworkCalc.GetNearbyIntersectingElements(owningDoc, elem);
                    item.FormworkAreaM2 = formworkCalc.CalculateNetFormworkArea(owningDoc, elem, neighbors);
                }

                ExtractedItems.Add(item);
                ProgressPercentage = (int)(((i + 1) / (double)(totalConcrete + 1)) * 70);
                StatusMessage = $"Đang tính ván khuôn: {i + 1}/{totalConcrete} ({item.CategoryName})";
            }

            StatusMessage = "Đang trích xuất khối lượng cốt thép...";
            foreach (var d in targetDocs)
            {
                var rebars = rebarCollector.CollectRebars(d);
                foreach (var r in rebars)
                {
                    if (selectedLevelNames.Contains(r.LevelName) || r.LevelName == "Unassigned")
                    {
                        ExtractedRebars.Add(r);
                    }
                }
            }

            ProgressPercentage = 100;
            StatusMessage = $"Hoàn tất: {ExtractedItems.Count} cấu kiện BT, {ExtractedRebars.Count} nhóm thép ({TotalRebarTons:N2} tấn).";
            NotifySummaryChanged();
        }

        private void ExecuteScanOverlaps()
        {
            if (_revitContext == null)
            {
                ShowDialog("Thông báo", "Tính năng quét va chạm mô hình yêu cầu mở trong Autodesk Revit.");
                return;
            }

            IsBusy = true;
            OverlapIssues.Clear();
            ProgressPercentage = 10;
            StatusMessage = "Đang kiểm tra giao cắt & trùng lặp...";

            try
            {
                ExecuteScanOverlapsSafe(_revitContext);
            }
            catch (Exception ex)
            {
                ShowDialog("Lỗi", $"Kiểm tra va chạm thất bại: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExecuteScanOverlapsSafe(object context)
        {
            var uiapp = context as Autodesk.Revit.UI.UIApplication;
            var doc = uiapp?.ActiveUIDocument?.Document;
            if (doc == null) return;

            var overlapDetector = new Services.RevitOverlapDetector();
            var issues = overlapDetector.DetectOverlaps(doc);
            foreach (var issue in issues)
            {
                OverlapIssues.Add(issue);
            }

            ProgressPercentage = 100;
            StatusMessage = $"Phát hiện {OverlapIssues.Count} vị trí trùng lặp / giao lấn thể tích.";
            OnPropertyChanged(nameof(TotalOverlapIssues));
        }

        private void ExecuteAutoJoinAll()
        {
            if (_revitContext == null) return;
            if (!OverlapIssues.Any()) return;

            ExecuteAutoJoinAllSafe(_revitContext);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExecuteAutoJoinAllSafe(object context)
        {
            var uiapp = context as Autodesk.Revit.UI.UIApplication;
            var doc = uiapp?.ActiveUIDocument?.Document;
            if (doc == null) return;

            var overlapDetector = new Services.RevitOverlapDetector();
            int fixedCount = 0;
            foreach (var issue in OverlapIssues.Where(x => !x.IsJoined).ToList())
            {
                if (overlapDetector.AutoJoinElements(doc, issue.ElementIdA, issue.ElementIdB))
                {
                    issue.IsJoined = true;
                    issue.IssueType = "Giao cắt (Đã Join)";
                    fixedCount++;
                }
            }

            ShowDialog("Auto-Join", $"Đã tự động Join thành công {fixedCount} cặp cấu kiện giao nhau!");
            OnPropertyChanged(nameof(TotalOverlapIssues));
        }

        private void ExecuteExportExcel()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Lưu Bảng Khối Lượng BOQ",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BOQ_TongHop_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsBusy = true;
                    StatusMessage = "Đang xuất file Excel đa Sheet...";

                    string projectTitle = "DỰ ÁN XÂY DỰNG";
                    if (_revitContext != null)
                    {
                        projectTitle = GetRevitDocTitle(_revitContext);
                    }

                    var exporter = new Services.ExcelBoqExporter();
                    exporter.ExportBoqToExcel(saveFileDialog.FileName, ExtractedItems, ExtractedRebars, projectTitle);

                    StatusMessage = "Xuất Excel thành công!";
                    ShowDialog("Thành công", $"File đã xuất thành công:\n{saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    ShowDialog("Lỗi", $"Không thể lưu file: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string GetRevitDocTitle(object context)
        {
            try
            {
                var uiapp = context as Autodesk.Revit.UI.UIApplication;
                return uiapp?.ActiveUIDocument?.Document?.Title ?? "DỰ ÁN XÂY DỰNG";
            }
            catch { return "DỰ ÁN XÂY DỰNG"; }
        }

        private void ShowDialog(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void ToggleAll(ObservableCollection<SelectionItem> list, bool selectAll)
        {
            foreach (var item in list) item.IsSelected = selectAll;
        }

        private void NotifySummaryChanged()
        {
            OnPropertyChanged(nameof(TotalElements));
            OnPropertyChanged(nameof(TotalVolumeM3));
            OnPropertyChanged(nameof(TotalFormworkM2));
            OnPropertyChanged(nameof(TotalRebarTons));
            OnPropertyChanged(nameof(TotalOverlapIssues));
            OnPropertyChanged(nameof(RebarD10Kg));
            OnPropertyChanged(nameof(RebarD18Kg));
            OnPropertyChanged(nameof(RebarDOver18Kg));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SelectionItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public long CategoryId { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
    }
}
