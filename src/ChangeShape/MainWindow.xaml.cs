using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChangeShape.Models;
using ChangeShape.Services;

namespace ChangeShape
{
    public partial class MainWindow : Window
    {
        private readonly AutoCadBridge _cadBridge = new();
        private readonly PresetManager _presetManager = new();
        private RebarShapeDefinition? _currentShape;
        private CadRebarTag _currentTag = new();
        private bool _isUpdatingUi = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadShapeCatalog();
            LoadPresets();
            CheckCadStatus();

            // Default selection: Shape 56
            SelectShapeById("56");
        }

        private void CheckCadStatus()
        {
            bool isCadActive = _cadBridge.IsAutoCadAvailable();
            if (isCadActive)
            {
                CadStatusBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6DA95"));
                CadStatusDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6DA95"));
                TxtCadStatus.Text = "AutoCAD Đã Kết Nối";
                TxtCadStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6DA95"));
                TxtStatusMessage.Text = "🟢 Đã tìm thấy AutoCAD! Sẵn sàng tương tác.";
            }
            else
            {
                CadStatusBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EED49F"));
                CadStatusDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EED49F"));
                TxtCadStatus.Text = "Chờ Kết Nối CAD";
                TxtCadStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EED49F"));
                TxtStatusMessage.Text = "🟡 Chưa kết nối AutoCAD. Bạn vẫn có thể tính toán, vẽ thử và dùng Clipboard bình thường.";
            }
        }

        private void LoadShapeCatalog(string filter = "")
        {
            var shapes = ShapeDatabase.AllShapes;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim().ToLowerInvariant();
                shapes = shapes.Where(s =>
                    s.ShapeId.ToLowerInvariant().Contains(f) ||
                    s.Name.ToLowerInvariant().Contains(f) ||
                    s.Category.ToLowerInvariant().Contains(f) ||
                    s.Description.ToLowerInvariant().Contains(f)).ToList();
            }

            LstShapes.ItemsSource = shapes;
        }

        private void LoadPresets()
        {
            LstPresets.ItemsSource = _presetManager.Presets;
        }

        private void SelectShapeById(string shapeId)
        {
            var shape = ShapeDatabase.GetShape(shapeId);
            if (shape != null)
            {
                LstShapes.SelectedItem = shape;
                SetCurrentShape(shape);
            }
        }

        private void SetCurrentShape(RebarShapeDefinition shape)
        {
            _currentShape = shape;
            _isUpdatingUi = true;

            TxtSelectedShapeTitle.Text = shape.Name;
            TxtShapeCategory.Text = shape.Category;
            TxtShapeDescription.Text = shape.Description;

            // Parameter visibility and labels
            LblParamA.Text = shape.ParamNames.Length > 0 ? shape.ParamNames[0] : "A (mm):";
            LblParamB.Text = shape.ParamNames.Length > 1 ? shape.ParamNames[1] : "B (mm):";
            LblParamC.Text = shape.ParamNames.Length > 2 ? shape.ParamNames[2] : "C (mm):";

            PanelParamA.Visibility = shape.ParamCount >= 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelParamB.Visibility = shape.ParamCount >= 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelParamC.Visibility = shape.ParamCount >= 3 ? Visibility.Visible : Visibility.Collapsed;

            if (shape.ParamCount >= 4)
            {
                PanelExtendedParams.Visibility = Visibility.Visible;
                PanelParamD.Visibility = Visibility.Visible;
                LblParamD.Text = shape.ParamNames.Length > 3 ? shape.ParamNames[3] : "D (mm):";

                PanelParamE.Visibility = shape.ParamCount >= 5 ? Visibility.Visible : Visibility.Collapsed;
                if (shape.ParamCount >= 5)
                    LblParamE.Text = shape.ParamNames.Length > 4 ? shape.ParamNames[4] : "E (mm):";
            }
            else
            {
                PanelExtendedParams.Visibility = Visibility.Collapsed;
            }

            // Fill default values if empty or changing shape
            if (shape.DefaultValues.Length > 0 && string.IsNullOrWhiteSpace(TxtParamA.Text))
                TxtParamA.Text = shape.DefaultValues[0].ToString(CultureInfo.InvariantCulture);
            if (shape.DefaultValues.Length > 1 && string.IsNullOrWhiteSpace(TxtParamB.Text))
                TxtParamB.Text = shape.DefaultValues[1].ToString(CultureInfo.InvariantCulture);
            if (shape.DefaultValues.Length > 2 && string.IsNullOrWhiteSpace(TxtParamC.Text))
                TxtParamC.Text = shape.DefaultValues[2].ToString(CultureInfo.InvariantCulture);

            _isUpdatingUi = false;

            UpdateCalculationsAndPreview();
        }

        private (double A, double B, double C, double D, double E) GetCurrentParamValues()
        {
            double.TryParse(TxtParamA.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double a);
            double.TryParse(TxtParamB.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double b);
            double.TryParse(TxtParamC.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double c);
            double.TryParse(TxtParamD.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double d);
            double.TryParse(TxtParamE.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double e);

            return (a, b, c, d, e);
        }

        private void UpdateCalculationsAndPreview()
        {
            if (_isUpdatingUi || _currentShape == null) return;

            var (a, b, c, d, e) = GetCurrentParamValues();

            // Calculate Cutting Length L
            double totalL = _currentShape.CalculateLength(a, b, c, d, e);
            TxtTotalLengthValue.Text = $"{totalL:N0} mm";

            // Formula expression display
            if (_currentShape.ShapeId == "56" || _currentShape.ShapeId == "54" || _currentShape.ShapeId == "55")
            {
                TxtFormulaExpr.Text = $"L = 2×({a:0} + {b:0}) + 2×{c:0} = {totalL:0} mm";
            }
            else if (_currentShape.ShapeId == "1")
            {
                TxtFormulaExpr.Text = $"L = A = {totalL:0} mm";
            }
            else if (_currentShape.ShapeId == "2")
            {
                TxtFormulaExpr.Text = $"L = {a:0} + {b:0} = {totalL:0} mm";
            }
            else if (_currentShape.ShapeId == "51")
            {
                TxtFormulaExpr.Text = $"L = {a:0} + 2×{b:0} = {totalL:0} mm";
            }
            else
            {
                TxtFormulaExpr.Text = $"{_currentShape.FormulaDisplay} = {totalL:0} mm";
            }

            // Update MText Preview
            string tagParamString = _currentShape.FormatTagParams(a, b, c, d, e);
            string fullMText = _currentTag.GenerateMText(_currentShape.ShapeId, totalL, tagParamString);
            TxtPreviewMText.Text = fullMText;

            // Redraw 2D Canvas
            RedrawCanvas(a, b, c, d, e);
        }

        #region LIVE 2D CANVAS DRAWING ENGINE

        private void RedrawCanvas(double a, double b, double c, double d, double e)
        {
            if (_currentShape == null) return;

            RebarCanvas.Children.Clear();

            double canvasWidth = RebarCanvas.ActualWidth;
            double canvasHeight = RebarCanvas.ActualHeight;

            if (canvasWidth <= 20 || canvasHeight <= 20) return;

            // Generate raw millimeter points
            var rawPoints = _currentShape.GeneratePoints(a, b, c, d, e);
            if (rawPoints.Count < 2) return;

            // Compute bounding box
            double minX = rawPoints.Min(p => p.X);
            double maxX = rawPoints.Max(p => p.X);
            double minY = rawPoints.Min(p => p.Y);
            double maxY = rawPoints.Max(p => p.Y);

            double shapeW = Math.Max(maxX - minX, 10.0);
            double shapeH = Math.Max(maxY - minY, 10.0);

            // Fit into canvas with padding
            double padding = 55.0;
            double availW = canvasWidth - 2.0 * padding;
            double availH = canvasHeight - 2.0 * padding;

            double scale = Math.Min(availW / shapeW, availH / shapeH);

            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;

            // Transformation from model mm to canvas screen coordinates (Y inverted)
            Point ModelToCanvas(Point pt)
            {
                double sx = canvasWidth / 2.0 + (pt.X - centerX) * scale;
                double sy = canvasHeight / 2.0 - (pt.Y - centerY) * scale;
                return new Point(sx, sy);
            }

            // Draw Rebar Polyline Path
            var screenPoints = rawPoints.Select(ModelToCanvas).ToList();

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = screenPoints[0],
                IsClosed = false
            };

            for (int i = 1; i < screenPoints.Count; i++)
            {
                pathFigure.Segments.Add(new LineSegment(screenPoints[i], true));
            }
            pathGeometry.Figures.Add(pathFigure);

            var path = new Path
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8AADF4")),
                StrokeThickness = 5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };
            RebarCanvas.Children.Add(path);

            // Draw Dimension Callouts (A, B, C)
            DrawDimensionAnnotations(a, b, c, screenPoints, ModelToCanvas, scale);
        }

        private void DrawDimensionAnnotations(double a, double b, double c, List<Point> screenPoints, Func<Point, Point> toScreen, double scale)
        {
            if (_currentShape == null) return;

            // Dimension for Width A (Bottom or Top)
            if (_currentShape.ParamCount >= 1 && a > 0)
            {
                double bottomY = screenPoints.Max(p => p.Y);
                double minX = screenPoints.Min(p => p.X);
                double maxX = screenPoints.Max(p => p.X);

                DrawDimLine(new Point(minX, bottomY + 18), new Point(maxX, bottomY + 18), $"A = {a:0} mm", "#A6DA95");
            }

            // Dimension for Height B (Left or Right)
            if (_currentShape.ParamCount >= 2 && b > 0)
            {
                double rightX = screenPoints.Max(p => p.X);
                double minY = screenPoints.Min(p => p.Y);
                double maxY = screenPoints.Max(p => p.Y);

                DrawDimLine(new Point(rightX + 18, maxY), new Point(rightX + 18, minY), $"B = {b:0} mm", "#F5A97F");
            }

            // Dimension for Hook C (for Stirrup Shape 56, 54, 55)
            if ((_currentShape.ShapeId == "56" || _currentShape.ShapeId == "54" || _currentShape.ShapeId == "55") && c > 0)
            {
                // Hook tip point
                Point hookTip = screenPoints[0];
                var badge = CreateTextBadge($"C = {c:0} (móc 135°)", "#EED49F");
                Canvas.SetLeft(badge, hookTip.X - 50);
                Canvas.SetTop(badge, hookTip.Y - 26);
                RebarCanvas.Children.Add(badge);
            }
        }

        private void DrawDimLine(Point p1, Point p2, string label, string colorHex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));

            var line = new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = brush,
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 3, 2 }
            };
            RebarCanvas.Children.Add(line);

            // Tick marks
            RebarCanvas.Children.Add(new Line { X1 = p1.X - 4, Y1 = p1.Y - 4, X2 = p1.X + 4, Y2 = p1.Y + 4, Stroke = brush, StrokeThickness = 1.5 });
            RebarCanvas.Children.Add(new Line { X1 = p2.X - 4, Y1 = p2.Y - 4, X2 = p2.X + 4, Y2 = p2.Y + 4, Stroke = brush, StrokeThickness = 1.5 });

            // Label badge in middle
            Point mid = new Point((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
            var badge = CreateTextBadge(label, colorHex);
            Canvas.SetLeft(badge, mid.X - 35);
            Canvas.SetTop(badge, mid.Y - 10);
            RebarCanvas.Children.Add(badge);
        }

        private Border CreateTextBadge(string text, string colorHex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E2030")),
                BorderBrush = brush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 1, 4, 1),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = brush
                }
            };
        }

        private void RebarCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var (a, b, c, d, ep) = GetCurrentParamValues();
            RedrawCanvas(a, b, c, d, ep);
        }

        #endregion

        #region EVENT HANDLERS

        private void LstShapes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstShapes.SelectedItem is RebarShapeDefinition shape)
            {
                SetCurrentShape(shape);
            }
        }

        private void TxtSearchShape_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearchShape.Text;
            TxtSearchPlaceholder.Visibility = string.IsNullOrEmpty(query) ? Visibility.Visible : Visibility.Collapsed;
            LoadShapeCatalog(query);
        }

        private void ParamInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculationsAndPreview();
        }

        private void CadPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentTag.Prefix = TxtCadPrefix.Text;
            _currentTag.Suffix = TxtCadSuffix.Text;
            UpdateCalculationsAndPreview();
        }

        private void LstPresets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstPresets.SelectedItem is PresetModel preset)
            {
                SelectShapeById(preset.ShapeId);

                _isUpdatingUi = true;
                if (preset.Values.Count > 0) TxtParamA.Text = preset.Values[0].ToString(CultureInfo.InvariantCulture);
                if (preset.Values.Count > 1) TxtParamB.Text = preset.Values[1].ToString(CultureInfo.InvariantCulture);
                if (preset.Values.Count > 2) TxtParamC.Text = preset.Values[2].ToString(CultureInfo.InvariantCulture);
                _isUpdatingUi = false;

                UpdateCalculationsAndPreview();
                TxtStatusMessage.Text = $"⭐ Đã nạp mẫu: {preset.Name}";
            }
        }

        private void BtnRefreshCad_Click(object sender, RoutedEventArgs e)
        {
            CheckCadStatus();
        }

        private void ChkTopMost_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            TxtStatusMessage.Text = "📌 Cửa sổ đã được ghim nổi trên AutoCAD.";
        }

        private void ChkTopMost_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            TxtStatusMessage.Text = "Đã bỏ ghim cửa sổ.";
        }

        #endregion

        #region CAD ACTIONS & HELPERS

        private void BtnPickFromCad_Click(object sender, RoutedEventArgs e)
        {
            TxtStatusMessage.Text = "⏳ Đang chuyển sang AutoCAD, hãy click chọn thanh MText...";
            var tag = _cadBridge.PickMTextFromCad(out string error);

            if (tag != null)
            {
                _currentTag = tag;
                TxtCadPrefix.Text = tag.Prefix;
                TxtCadSuffix.Text = tag.Suffix;

                // Sync diameter in dropdown if recognized
                int dia = tag.Diameter;
                if (dia == 10) CboRebarDia.SelectedIndex = 0;
                else if (dia == 12) CboRebarDia.SelectedIndex = 1;
                else if (dia == 16) CboRebarDia.SelectedIndex = 2;
                else if (dia == 20) CboRebarDia.SelectedIndex = 3;

                // If user selected rebar #1, they typically want to convert to Shape 56
                // Preserve current shape if already set, or adopt existing params
                if (tag.CurrentParams.Count > 0 && tag.CurrentShapeId != "1")
                {
                    _isUpdatingUi = true;
                    if (tag.CurrentParams.Count > 0) TxtParamA.Text = tag.CurrentParams[0].ToString(CultureInfo.InvariantCulture);
                    if (tag.CurrentParams.Count > 1) TxtParamB.Text = tag.CurrentParams[1].ToString(CultureInfo.InvariantCulture);
                    if (tag.CurrentParams.Count > 2) TxtParamC.Text = tag.CurrentParams[2].ToString(CultureInfo.InvariantCulture);
                    _isUpdatingUi = false;
                }

                UpdateCalculationsAndPreview();
                TxtStatusMessage.Text = $"✅ Đã nhận diện thanh thép từ CAD: {tag.Prefix} L={tag.CurrentLength:0} (Shape cũ: {tag.CurrentShapeId})";
            }
            else if (!string.IsNullOrEmpty(error))
            {
                TxtStatusMessage.Text = $"⚠️ {error}";
            }
        }

        private void BtnApplyToCad_Click(object sender, RoutedEventArgs e)
        {
            if (_currentShape == null) return;

            string fullText = TxtPreviewMText.Text;
            bool success = _cadBridge.UpdateMTextInCad(_currentTag.EntityHandle, fullText, out string error);

            if (success)
            {
                TxtStatusMessage.Text = $"🚀 Thành công! Đã đổi thanh thép sang Shape {_currentShape.ShapeId} với L={TxtTotalLengthValue.Text}!";
            }
            else
            {
                // Fallback to Clipboard
                Clipboard.SetText(fullText);
                TxtStatusMessage.Text = $"📋 Không thể ghi trực tiếp ({error}). Đã copy mã vào Clipboard!";
            }
        }

        private void BtnBatchApply_Click(object sender, RoutedEventArgs e)
        {
            if (_currentShape == null) return;

            var (a, b, c, d, ep) = GetCurrentParamValues();
            double totalL = _currentShape.CalculateLength(a, b, c, d, ep);
            string paramStr = _currentShape.FormatTagParams(a, b, c, d, ep);

            TxtStatusMessage.Text = "⏳ Hãy quét chọn các MText trên AutoCAD...";
            int count = _cadBridge.BatchUpdateMTextsInCad(_currentShape.ShapeId, totalL, paramStr, out string error);

            if (count > 0)
            {
                TxtStatusMessage.Text = $"⚡ Tuyệt vời! Đã cập nhật hàng loạt {count} thanh MText sang Shape {_currentShape.ShapeId}!";
            }
            else if (!string.IsNullOrEmpty(error))
            {
                TxtStatusMessage.Text = $"⚠️ {error}";
            }
        }

        private void BtnDrawPolyline_Click(object sender, RoutedEventArgs e)
        {
            if (_currentShape == null) return;

            var (a, b, c, d, ep) = GetCurrentParamValues();
            var pts = _currentShape.GeneratePoints(a, b, c, d, ep);

            bool isClosed = _currentShape.ShapeId == "56" || _currentShape.ShapeId == "54" || _currentShape.ShapeId == "55";
            TxtStatusMessage.Text = "⏳ Hãy click chọn điểm đặt hình vẽ trên AutoCAD...";
            bool success = _cadBridge.DrawPolylineInCad(pts, isClosed, out string error);

            if (success)
            {
                TxtStatusMessage.Text = $"✏️ Đã vẽ đường Polyline hình học Shape {_currentShape.ShapeId} vào CAD!";
            }
            else
            {
                TxtStatusMessage.Text = $"⚠️ {error}";
            }
        }

        private void BtnCopyMText_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TxtPreviewMText.Text);
            TxtStatusMessage.Text = "📋 Đã sao chép nội dung MText vào Clipboard!";
        }

        private void BtnPickParamA_Click(object sender, RoutedEventArgs e)
        {
            TxtStatusMessage.Text = "⏳ Đang chuyển sang CAD, click chọn 2 điểm để đo A...";
            double? dist = _cadBridge.MeasureDistanceInCad("Đo kích thước A (Rộng)", out string error);
            if (dist.HasValue)
            {
                TxtParamA.Text = dist.Value.ToString(CultureInfo.InvariantCulture);
                TxtStatusMessage.Text = $"📏 Đã lấy số đo A từ CAD: {dist.Value:0} mm";
            }
        }

        private void BtnPickParamB_Click(object sender, RoutedEventArgs e)
        {
            TxtStatusMessage.Text = "⏳ Đang chuyển sang CAD, click chọn 2 điểm để đo B...";
            double? dist = _cadBridge.MeasureDistanceInCad("Đo kích thước B (Cao)", out string error);
            if (dist.HasValue)
            {
                TxtParamB.Text = dist.Value.ToString(CultureInfo.InvariantCulture);
                TxtStatusMessage.Text = $"📏 Đã lấy số đo B từ CAD: {dist.Value:0} mm";
            }
        }

        private void BtnHookHelper_Click(object sender, RoutedEventArgs e)
        {
            // Auto hook: 10 * diameter
            int dia = 10;
            if (CboRebarDia.SelectedItem is ComboBoxItem item)
            {
                string text = item.Content.ToString() ?? "D10";
                int.TryParse(text.Replace("D", ""), out dia);
            }
            double hook = Math.Max(100, dia * 10);
            TxtParamC.Text = hook.ToString(CultureInfo.InvariantCulture);
            TxtStatusMessage.Text = $"⚡ Đã tính móc 135° theo chuẩn {dia}d = {hook:0} mm";
        }

        private void BtnApplyConcreteCalc_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtConcreteW.Text, out double cw) &&
                double.TryParse(TxtConcreteH.Text, out double ch) &&
                double.TryParse(TxtCover.Text, out double cover))
            {
                int dia = 10;
                if (CboRebarDia.SelectedItem is ComboBoxItem item)
                {
                    string text = item.Content.ToString() ?? "D10";
                    int.TryParse(text.Replace("D", ""), out dia);
                }

                var (a, b, c) = PresetManager.CalculateFromConcreteSection(cw, ch, cover, dia);

                _isUpdatingUi = true;
                TxtParamA.Text = a.ToString(CultureInfo.InvariantCulture);
                TxtParamB.Text = b.ToString(CultureInfo.InvariantCulture);
                TxtParamC.Text = c.ToString(CultureInfo.InvariantCulture);
                _isUpdatingUi = false;

                UpdateCalculationsAndPreview();
                TxtStatusMessage.Text = $"💡 Đã tính đai từ bê tông {cw:0}×{ch:0} (Cover={cover:0}): A={a:0}, B={b:0}, C={c:0}";
            }
        }

        private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            if (_currentShape == null) return;

            var (a, b, c, d, ep) = GetCurrentParamValues();
            string presetName = $"Mẫu {_currentShape.ShapeId} ({a:0}×{b:0}×{c:0})";

            var values = new List<double> { a, b, c };
            _presetManager.SaveCustomPreset(presetName, _currentShape.ShapeId, values);
            LoadPresets();

            TxtStatusMessage.Text = $"⭐ Đã lưu mẫu mới: {presetName}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnApplyToCad_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                BtnRefreshCad_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                BtnPickFromCad_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                BtnDrawPolyline_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        #endregion
    }
}