using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DrawColor = System.Drawing.Color;
using DrawFont = System.Drawing.Font;
using DrawPoint = System.Drawing.Point;
using DrawSize = System.Drawing.Size;
using SysTask = System.Threading.Tasks.Task;
using TeklaColor = Tekla.Structures.Model.UI.Color;
using TeklaPoint = Tekla.Structures.Geometry3d.Point;
using TeklaUiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace BimCommands.RebarErrorChecker
{
    public class MainForm : Form
    {
        private Model _model;
        private List<RebarErrorInfo> _currentErrors = new List<RebarErrorInfo>();
        private List<GraphicPolyLine> _activeHighlights = new List<GraphicPolyLine>();

        // UI Controls
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblTeklaStatus;

        private Panel controlPanel;
        private Button btnCheckSelected;
        private Button btnScanAll;
        private Button btnZoomSelect;
        private Button btnHighlight;
        private Button btnClearHighlight;
        private Button btnExportCsv;

        private GroupBox grpSettings;
        private NumericUpDown numSearchRadius;
        private NumericUpDown numAngleTol;
        private NumericUpDown numOffsetTol;
        private CheckBox chkSimulateSplice;
        private CheckBox chkCountMismatch;
        private CheckBox chkCorrupted;

        private DataGridView dgvErrors;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusText;
        private ToolStripProgressBar progressBar;
        private ToolStripStatusLabel lblCountText;

        public MainForm()
        {
            InitializeComponent();
            ConnectTekla();
        }

        private void InitializeComponent()
        {
            this.Text = "Tekla Rebar Error Checker - [BimCommands]";
            this.Size = new DrawSize(1100, 720);
            this.MinimumSize = new DrawSize(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DrawColor.FromArgb(24, 28, 36);
            this.ForeColor = DrawColor.White;
            this.Font = new DrawFont("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            // 1. Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                Padding = new Padding(15, 10, 15, 10)
            };

            lblTitle = new Label
            {
                Text = "TEKLA REBAR ERROR CHECKER",
                Font = new DrawFont("Segoe UI", 13F, System.Drawing.FontStyle.Bold),
                ForeColor = DrawColor.FromArgb(56, 189, 248),
                AutoSize = true,
                Location = new DrawPoint(15, 12)
            };

            lblSubtitle = new Label
            {
                Text = "Phát hiện & chẩn đoán lỗi: \"Rebar splice could not be created. Check rebar type and placements\"",
                Font = new DrawFont("Segoe UI", 8.5F),
                ForeColor = DrawColor.FromArgb(156, 163, 175),
                AutoSize = true,
                Location = new DrawPoint(16, 40)
            };

            lblTeklaStatus = new Label
            {
                Text = "Tekla: Checking...",
                Font = new DrawFont("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                ForeColor = DrawColor.Yellow,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new DrawPoint(850, 24)
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblTeklaStatus);

            // 2. Control Panel & Settings
            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 135,
                BackColor = DrawColor.FromArgb(30, 35, 45),
                Padding = new Padding(15, 8, 15, 8)
            };

            // Buttons
            btnCheckSelected = CreateModernButton("Kiểm tra Thép Đang Chọn", DrawColor.FromArgb(37, 99, 235), new DrawPoint(15, 12), new DrawSize(200, 36));
            btnCheckSelected.Click += (s, e) => RunAnalysis(true);

            btnScanAll = CreateModernButton("Quét Toàn Bộ Model", DrawColor.FromArgb(79, 70, 229), new DrawPoint(225, 12), new DrawSize(180, 36));
            btnScanAll.Click += (s, e) => RunAnalysis(false);

            btnZoomSelect = CreateModernButton("Zoom & Chọn Lỗi", DrawColor.FromArgb(16, 149, 193), new DrawPoint(415, 12), new DrawSize(150, 36));
            btnZoomSelect.Click += (s, e) => ZoomToSelectedError();

            btnHighlight = CreateModernButton("Tô Màu Highlight", DrawColor.FromArgb(217, 119, 6), new DrawPoint(575, 12), new DrawSize(140, 36));
            btnHighlight.Click += (s, e) => HighlightErrorsInTekla();

            btnClearHighlight = CreateModernButton("Xóa Highlight", DrawColor.FromArgb(75, 85, 99), new DrawPoint(725, 12), new DrawSize(110, 36));
            btnClearHighlight.Click += (s, e) => ClearHighlights();

            btnExportCsv = CreateModernButton("Xuất File CSV", DrawColor.FromArgb(5, 150, 105), new DrawPoint(845, 12), new DrawSize(120, 36));
            btnExportCsv.Click += (s, e) => ExportToCsv();

            controlPanel.Controls.AddRange(new Control[] {
                btnCheckSelected, btnScanAll, btnZoomSelect, btnHighlight, btnClearHighlight, btnExportCsv
            });

            // Settings Group
            grpSettings = new GroupBox
            {
                Text = " Thiết Lập Phạm Vi Quét Lỗi Splice ",
                ForeColor = DrawColor.FromArgb(209, 213, 219),
                Location = new DrawPoint(15, 55),
                Size = new DrawSize(1050, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblRad = new Label { Text = "Bán kính tìm kiếm (mm):", AutoSize = true, Location = new DrawPoint(15, 28) };
            numSearchRadius = new NumericUpDown { Minimum = 100, Maximum = 10000, Value = 1500, Increment = 100, Location = new DrawPoint(165, 25), Width = 70, BackColor = DrawColor.FromArgb(40, 46, 58), ForeColor = DrawColor.White };

            Label lblModeInfo = new Label 
            { 
                Text = "⚡ Chế độ: Tự động phát hiện 100% các cặp thép bị 'Lỗi tạo Splice (Tekla Warning)' và phân tích nguyên nhân hình học (Ngược hướng rải / Lệch số thanh / Lệch tâm)", 
                AutoSize = true, 
                ForeColor = DrawColor.FromArgb(52, 211, 153), 
                Location = new DrawPoint(255, 28),
                Font = new DrawFont("Segoe UI", 9F, System.Drawing.FontStyle.Regular)
            };

            numAngleTol = new NumericUpDown { Value = 3, Visible = false };
            numOffsetTol = new NumericUpDown { Value = 40, Visible = false };
            chkSimulateSplice = new CheckBox { Checked = true, Visible = false };
            chkCountMismatch = new CheckBox { Checked = true, Visible = false };
            chkCorrupted = new CheckBox { Checked = false, Visible = false };

            grpSettings.Controls.AddRange(new Control[] {
                lblRad, numSearchRadius, lblModeInfo
            });

            controlPanel.Controls.Add(grpSettings);

            // 3. DataGridView Setup
            dgvErrors = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = DrawColor.FromArgb(20, 24, 32),
                GridColor = DrawColor.FromArgb(45, 52, 65),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            dgvErrors.ColumnHeadersDefaultCellStyle.BackColor = DrawColor.FromArgb(30, 41, 59);
            dgvErrors.ColumnHeadersDefaultCellStyle.ForeColor = DrawColor.FromArgb(241, 245, 249);
            dgvErrors.ColumnHeadersDefaultCellStyle.Font = new DrawFont("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvErrors.ColumnHeadersHeight = 35;
            dgvErrors.RowTemplate.Height = 28;
            dgvErrors.DefaultCellStyle.BackColor = DrawColor.FromArgb(24, 28, 36);
            dgvErrors.DefaultCellStyle.ForeColor = DrawColor.FromArgb(226, 232, 240);
            dgvErrors.DefaultCellStyle.SelectionBackColor = DrawColor.FromArgb(51, 65, 85);
            dgvErrors.DefaultCellStyle.SelectionForeColor = DrawColor.White;

            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColIndex", HeaderText = "STT", FillWeight = 25 });
            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColCategory", HeaderText = "Phân Loại Lỗi", FillWeight = 70 });
            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColIds", HeaderText = "ID Đối Tượng (Rebar)", FillWeight = 55 });
            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColSizes", HeaderText = "Size Thép", FillWeight = 45 });
            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColDetails", HeaderText = "Chi Tiết Sai Lệch / Nguyên Nhân Gây Lỗi", FillWeight = 160 });
            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColLocation", HeaderText = "Vị Trí Tọa Độ", FillWeight = 75 });

            dgvErrors.CellDoubleClick += (s, e) => ZoomToSelectedError();
            dgvErrors.RowPrePaint += DgvErrors_RowPrePaint;

            // 4. Status Strip
            statusStrip = new StatusStrip
            {
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(209, 213, 219)
            };

            lblStatusText = new ToolStripStatusLabel { Text = "Sẵn sàng", Spring = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            progressBar = new ToolStripProgressBar { Visible = false, Width = 150 };
            lblCountText = new ToolStripStatusLabel { Text = "0 lỗi tìm thấy" };

            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusText, progressBar, lblCountText });

            // Add all to Form
            this.Controls.Add(dgvErrors);
            this.Controls.Add(controlPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(statusStrip);
        }

        private Button CreateModernButton(string text, DrawColor bg, DrawPoint pos, DrawSize size)
        {
            var btn = new Button
            {
                Text = text,
                Location = pos,
                Size = size,
                BackColor = bg,
                ForeColor = DrawColor.White,
                FlatStyle = FlatStyle.Flat,
                Font = new DrawFont("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(bg, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = bg;
            return btn;
        }

        private void ConnectTekla()
        {
            try
            {
                _model = new Model();
                if (_model.GetConnectionStatus())
                {
                    string modelName = _model.GetInfo().ModelName;
                    lblTeklaStatus.Text = "Tekla: Kết nối [" + modelName + "]";
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(34, 197, 94);
                }
                else
                {
                    lblTeklaStatus.Text = "Tekla: Chưa kết nối!";
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68);
                }
            }
            catch (Exception ex)
            {
                lblTeklaStatus.Text = "Tekla: Lỗi kết nối";
                lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68);
                lblStatusText.Text = "Lỗi kết nối Tekla: " + ex.Message;
            }
        }

        private async void RunAnalysis(bool fromSelection)
        {
            if (_model == null || !_model.GetConnectionStatus())
            {
                ConnectTekla();
                if (_model == null || !_model.GetConnectionStatus())
                {
                    MessageBox.Show(this, "Không thể kết nối với Tekla Structures. Vui lòng mở mô hình trước!", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            btnCheckSelected.Enabled = false;
            btnScanAll.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;
            lblStatusText.Text = fromSelection ? "Đang đọc các thanh thép đang chọn..." : "Đang quét các thanh thép trong toàn bộ mô hình...";

            var settings = new AnalysisSettings
            {
                SearchRadius = (double)numSearchRadius.Value,
                AngleTolerance = (double)numAngleTol.Value,
                OffsetTolerance = (double)numOffsetTol.Value,
                CheckSpliceSimulation = chkSimulateSplice.Checked,
                CheckBarCountMismatch = chkCountMismatch.Checked,
                CheckCorruptedBars = chkCorrupted.Checked
            };

            List<Reinforcement> targetRebars = new List<Reinforcement>();

            try
            {
                if (fromSelection)
                {
                    var selector = new TeklaUiSelector();
                    var selectedObjects = selector.GetSelectedObjects();
                    while (selectedObjects.MoveNext())
                    {
                        Reinforcement rebar = selectedObjects.Current as Reinforcement;
                        if (rebar != null)
                        {
                            targetRebars.Add(rebar);
                        }
                    }

                    if (targetRebars.Count == 0)
                    {
                        MessageBox.Show(this, "Bạn chưa chọn thanh thép nào trong mô hình Tekla!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnCheckSelected.Enabled = true;
                        btnScanAll.Enabled = true;
                        progressBar.Visible = false;
                        lblStatusText.Text = "Sẵn sàng";
                        return;
                    }
                }
                else
                {
                    ModelObject.ModelObjectEnum[] types = new ModelObject.ModelObjectEnum[]
                    {
                        ModelObject.ModelObjectEnum.REBARGROUP,
                        ModelObject.ModelObjectEnum.SINGLEREBAR,
                        ModelObject.ModelObjectEnum.CURVED_REBARGROUP,
                        ModelObject.ModelObjectEnum.CIRCLE_REBARGROUP
                    };

                    foreach (var t in types)
                    {
                        var rebarEnum = _model.GetModelObjectSelector().GetAllObjectsWithType(t);
                        while (rebarEnum.MoveNext())
                        {
                            Reinforcement rebar = rebarEnum.Current as Reinforcement;
                            if (rebar != null)
                            {
                                targetRebars.Add(rebar);
                            }
                        }
                    }
                }

                lblStatusText.Text = string.Format("Đang phân tích {0} đối tượng thép...", targetRebars.Count);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                List<RebarErrorInfo> errors = null;
                await SysTask.Run(() =>
                {
                    errors = RebarGeometryAnalyzer.AnalyzeRebars(targetRebars, settings, (curr, max) =>
                    {
                        if (max > 0)
                        {
                            int pct = (int)((curr * 100.0) / max);
                            if (pct > 100) pct = 100;
                            this.Invoke((Action)(() => progressBar.Value = pct));
                        }
                    });
                });
                sw.Stop();

                _currentErrors = errors ?? new List<RebarErrorInfo>();
                PopulateGrid(_currentErrors);

                lblStatusText.Text = string.Format("Hoàn thành kiểm tra trong {0:F1} giây!", sw.Elapsed.TotalSeconds);
                lblCountText.Text = string.Format("{0} lỗi phát hiện", _currentErrors.Count);

                if (_currentErrors.Count == 0)
                {
                    MessageBox.Show(this, "Tuyệt vời! Không phát hiện lỗi placement hoặc splice nào trong các thanh thép đã quét.", "Kết Quả Kiểm Tra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Đã xảy ra lỗi trong quá trình quét: " + ex.Message, "Lỗi Thực Thi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusText.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnCheckSelected.Enabled = true;
                btnScanAll.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void PopulateGrid(List<RebarErrorInfo> errors)
        {
            dgvErrors.Rows.Clear();
            for (int i = 0; i < errors.Count; i++)
            {
                var err = errors[i];
                err.Index = i + 1;

                string sizes = err.BarSize1;
                if (!string.IsNullOrEmpty(err.BarSize2) && err.BarSize2 != err.BarSize1)
                {
                    sizes = err.BarSize1 + " / " + err.BarSize2;
                }

                int rowIdx = dgvErrors.Rows.Add(
                    err.Index,
                    err.CategoryDisplayName,
                    err.ObjectIdsString,
                    sizes,
                    err.ErrorDetails,
                    err.LocationString
                );

                dgvErrors.Rows[rowIdx].Tag = err;
            }
        }

        private void DgvErrors_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvErrors.Rows.Count) return;
            var err = dgvErrors.Rows[e.RowIndex].Tag as RebarErrorInfo;
            if (err == null) return;

            DrawColor catColor = DrawColor.FromArgb(226, 232, 240);
            switch (err.Category)
            {
                case RebarErrorCategory.SpliceFailed:
                    catColor = DrawColor.FromArgb(239, 68, 68); // Red
                    break;
                case RebarErrorCategory.AngleDeviation:
                    catColor = DrawColor.FromArgb(249, 115, 22); // Orange
                    break;
                case RebarErrorCategory.EccentricityOffset:
                    catColor = DrawColor.FromArgb(234, 179, 8); // Yellow-Amber
                    break;
                case RebarErrorCategory.BarCountMismatch:
                    catColor = DrawColor.FromArgb(56, 189, 248); // Cyan
                    break;
                case RebarErrorCategory.EndGapTooLarge:
                    catColor = DrawColor.FromArgb(168, 85, 247); // Purple
                    break;
                case RebarErrorCategory.CorruptedRebar:
                    catColor = DrawColor.FromArgb(244, 63, 94); // Rose
                    break;
                case RebarErrorCategory.OrphanSplice:
                    catColor = DrawColor.FromArgb(148, 163, 184); // Gray
                    break;
            }

            dgvErrors.Rows[e.RowIndex].Cells["ColCategory"].Style.ForeColor = catColor;
            dgvErrors.Rows[e.RowIndex].Cells["ColCategory"].Style.Font = new DrawFont("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        }

        private void ZoomToSelectedError()
        {
            if (dgvErrors.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn một dòng lỗi trong bảng kết quả!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var objsToSelect = new ArrayList();
                TeklaPoint minAABB = new TeklaPoint(double.MaxValue, double.MaxValue, double.MaxValue);
                TeklaPoint maxAABB = new TeklaPoint(double.MinValue, double.MinValue, double.MinValue);

                foreach (DataGridViewRow row in dgvErrors.SelectedRows)
                {
                    var err = row.Tag as RebarErrorInfo;
                    if (err == null) continue;

                    if (err.ModelObject1 != null) objsToSelect.Add(err.ModelObject1);
                    if (err.ModelObject2 != null) objsToSelect.Add(err.ModelObject2);

                    if (err.MinPoint != null && err.MaxPoint != null)
                    {
                        minAABB.X = Math.Min(minAABB.X, err.MinPoint.X);
                        minAABB.Y = Math.Min(minAABB.Y, err.MinPoint.Y);
                        minAABB.Z = Math.Min(minAABB.Z, err.MinPoint.Z);

                        maxAABB.X = Math.Max(maxAABB.X, err.MaxPoint.X);
                        maxAABB.Y = Math.Max(maxAABB.Y, err.MaxPoint.Y);
                        maxAABB.Z = Math.Max(maxAABB.Z, err.MaxPoint.Z);
                    }
                }

                if (objsToSelect.Count > 0)
                {
                    // Select in Tekla
                    var selector = new TeklaUiSelector();
                    selector.Select(objsToSelect);

                    // Zoom to Bounding Box
                    if (minAABB.X < double.MaxValue && maxAABB.X > double.MinValue)
                    {
                        AABB aabb = new AABB(minAABB, maxAABB);
                        ViewHandler.ZoomToBoundingBox(aabb);
                    }
                    lblStatusText.Text = string.Format("Đã Zoom & Chọn {0} đối tượng lỗi trong Tekla!", objsToSelect.Count);
                }
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Lỗi khi Zoom trong Tekla: " + ex.Message;
            }
        }

        private void HighlightErrorsInTekla()
        {
            if (_currentErrors.Count == 0)
            {
                MessageBox.Show(this, "Không có lỗi nào để tô màu!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ClearHighlights();

            try
            {
                foreach (var err in _currentErrors)
                {
                    if (err.MinPoint == null || err.MaxPoint == null) continue;

                    // Draw wireframe bounding box around error pair
                    var color = new TeklaColor(1.0, 0.0, 0.0); // Red
                    if (err.Category == RebarErrorCategory.AngleDeviation)
                        color = new TeklaColor(1.0, 0.5, 0.0); // Orange
                    else if (err.Category == RebarErrorCategory.EccentricityOffset)
                        color = new TeklaColor(1.0, 1.0, 0.0); // Yellow

                    AddBoxHighlight(err.MinPoint, err.MaxPoint, color);
                }

                lblStatusText.Text = string.Format("Đã tô màu Highlight {0} vị trí lỗi trong Tekla View!", _currentErrors.Count);
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Lỗi khi tô màu Highlight: " + ex.Message;
            }
        }

        private void AddBoxHighlight(TeklaPoint pMin, TeklaPoint pMax, TeklaColor col)
        {
            TeklaPoint p1 = new TeklaPoint(pMin.X, pMin.Y, pMin.Z);
            TeklaPoint p2 = new TeklaPoint(pMax.X, pMin.Y, pMin.Z);
            TeklaPoint p3 = new TeklaPoint(pMax.X, pMax.Y, pMin.Z);
            TeklaPoint p4 = new TeklaPoint(pMin.X, pMax.Y, pMin.Z);

            TeklaPoint p5 = new TeklaPoint(pMin.X, pMin.Y, pMax.Z);
            TeklaPoint p6 = new TeklaPoint(pMax.X, pMin.Y, pMax.Z);
            TeklaPoint p7 = new TeklaPoint(pMax.X, pMax.Y, pMax.Z);
            TeklaPoint p8 = new TeklaPoint(pMin.X, pMax.Y, pMax.Z);

            DrawSegment(p1, p2, col);
            DrawSegment(p2, p3, col);
            DrawSegment(p3, p4, col);
            DrawSegment(p4, p1, col);

            DrawSegment(p5, p6, col);
            DrawSegment(p6, p7, col);
            DrawSegment(p7, p8, col);
            DrawSegment(p8, p5, col);

            DrawSegment(p1, p5, col);
            DrawSegment(p2, p6, col);
            DrawSegment(p3, p7, col);
            DrawSegment(p4, p8, col);
        }

        private void DrawSegment(TeklaPoint a, TeklaPoint b, TeklaColor col)
        {
            var pts = new ArrayList { a, b };
            var poly = new PolyLine(pts);
            var gPoly = new GraphicPolyLine();
            gPoly.PolyLine = poly;
            gPoly.Color = col;
            gPoly.Width = 2;
            _activeHighlights.Add(gPoly);
        }

        private void ClearHighlights()
        {
            _activeHighlights.Clear();
            var views = ViewHandler.GetVisibleViews();
            while (views.MoveNext())
            {
                ViewHandler.RedrawView(views.Current);
            }
            lblStatusText.Text = "Đã xóa toàn bộ Highlight trên Tekla Views.";
        }

        private void ExportToCsv()
        {
            if (_currentErrors.Count == 0)
            {
                MessageBox.Show(this, "Danh sách rỗng, không có dữ liệu để xuất!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = string.Format("Tekla_Rebar_Errors_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now)
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("STT,PhanLoaiLoi,ID_Thep1,ID_Thep2,SizeThep,ChiTietSaiLech,ToaDo_X,ToaDo_Y,ToaDo_Z");
                    foreach (var err in _currentErrors)
                    {
                        sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6:F1}\",\"{7:F1}\",\"{8:F1}\"",
                            err.Index,
                            err.CategoryDisplayName,
                            err.Id1,
                            err.Id2 > 0 ? err.Id2.ToString() : "",
                            err.BarSize1,
                            err.ErrorDetails.Replace("\"", "'"),
                            err.CenterPoint.X,
                            err.CenterPoint.Y,
                            err.CenterPoint.Z
                        ));
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show(this, "Xuất báo cáo CSV thành công:\n" + sfd.FileName, "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Lỗi khi lưu file CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
