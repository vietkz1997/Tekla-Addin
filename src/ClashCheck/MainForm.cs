using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SysTask = System.Threading.Tasks.Task;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using DrawColor = System.Drawing.Color;
using DrawFont = System.Drawing.Font;
using DrawPoint = System.Drawing.Point;
using DrawSize = System.Drawing.Size;
using TeklaColor = Tekla.Structures.Model.UI.Color;
using TeklaPoint = Tekla.Structures.Geometry3d.Point;
using TeklaUiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace BimCommands.Tekla.ClashCheck
{
    public class MainForm : Form
    {
        private Model _model;
        private ClashDetector _detector;
        private List<ClashResultItem> _currentClashes = new List<ClashResultItem>();
        private List<GraphicPolyLine> _activeHighlights = new List<GraphicPolyLine>();
        private CancellationTokenSource _cts;

        // UI Controls
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblTeklaStatus;

        private Panel controlPanel;
        private RadioButton rbRebarAll;
        private RadioButton rbRebarSelected;
        private ComboBox cboIfcFiles;
        private NumericUpDown numTolerance;
        private NumericUpDown numClearance;
        private CheckBox chkIgnoreFilter;
        private TextBox txtIgnoreKeywords;
        private Button btnResetIgnore;
        private const string DefaultIgnoredList = "Bolt assembly\r\nSAFETY_BAR\r\nLUG\r\nLADDER\r\nSAFETY_HOOK\r\nVBRACE\r\nWELD_COUPLER(10)\r\nCHECK_COUPLER(10)";
        private static string FilterSettingsFile
        {
            get
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string candidate = Path.Combine(baseDir, "clash_ignored_filters.txt");
                    if (File.Exists(candidate)) return candidate;

                    string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tekla_ClashCheck");
                    if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
                    string appDataPath = Path.Combine(appData, "clash_ignored_filters.txt");
                    if (File.Exists(appDataPath)) return appDataPath;

                    return candidate;
                }
                catch
                {
                    return "clash_ignored_filters.txt";
                }
            }
        }
        private Button btnScan;
        private Button btnStop;
        private Button btnZoomSelect;
        private Button btnHighlight;
        private Button btnClearHighlight;
        private Button btnExportCsv;

        private DataGridView dgvClashes;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusText;
        private ToolStripProgressBar progressBar;
        private ToolStripStatusLabel lblCountText;

        private static string LoadSavedIgnoredKeywords()
        {
            try
            {
                if (File.Exists(FilterSettingsFile))
                {
                    string content = File.ReadAllText(FilterSettingsFile).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        if (!content.Contains("\n") && content.Contains(","))
                        {
                            var parts = content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var lines = new List<string>();
                            foreach (var p in parts)
                            {
                                string t = p.Trim();
                                if (!string.IsNullOrEmpty(t)) lines.Add(t);
                            }
                            return string.Join("\r\n", lines.ToArray());
                        }
                        return content;
                    }
                }
            }
            catch { }
            return DefaultIgnoredList;
        }

        private static List<string> ParseKeywords(string text)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return list;
            string[] parts = text.Split(new char[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                string t = p.Trim();
                if (!string.IsNullOrEmpty(t)) list.Add(t);
            }
            return list;
        }

        public MainForm()
        {
            InitializeComponent();
            ConnectTekla();
        }

        private void InitializeComponent()
        {
            this.Text = "Tekla Clash Check (Rebar vs IFC) - [My-tool]";
            this.Size = new DrawSize(1180, 750);
            this.MinimumSize = new DrawSize(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DrawColor.FromArgb(24, 28, 36);
            this.ForeColor = DrawColor.White;
            this.Font = new DrawFont("Segoe UI", 9F, FontStyle.Regular);

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
                Text = "CLASH CHECK: THÉP VÀ CẤU KIỆN IFC",
                Font = new DrawFont("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = DrawColor.FromArgb(56, 189, 248),
                AutoSize = true,
                Location = new DrawPoint(15, 12)
            };

            lblSubtitle = new Label
            {
                Text = "Kiểm tra va chạm & hở an toàn giữa Cốt thép (Tekla Model) và Cấu kiện tham chiếu IFC (Navisworks Style)",
                Font = new DrawFont("Segoe UI", 8.5F),
                ForeColor = DrawColor.FromArgb(156, 163, 175),
                AutoSize = true,
                Location = new DrawPoint(16, 40)
            };

            lblTeklaStatus = new Label
            {
                Text = "● Đang kết nối Tekla...",
                Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DrawColor.FromArgb(234, 179, 8),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new DrawPoint(880, 24)
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblTeklaStatus);

            // 2. Control Panel
            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 138,
                BackColor = DrawColor.FromArgb(30, 35, 45),
                Padding = new Padding(15, 10, 15, 10)
            };

            // Group 1: Phạm vi Thép
            Label lblScope = new Label
            {
                Text = "Phạm vi Thép:",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(15, 14),
                AutoSize = true
            };
            rbRebarSelected = new RadioButton
            {
                Text = "🎯 Thép đang chọn",
                Checked = true,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new DrawSize(125, 28),
                Location = new DrawPoint(105, 8),
                Cursor = Cursors.Hand
            };
            rbRebarAll = new RadioButton
            {
                Text = "🌐 Toàn bộ thép",
                Checked = false,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new DrawSize(115, 28),
                Location = new DrawPoint(238, 8),
                Cursor = Cursors.Hand
            };

            Action updateScopeStyles = () =>
            {
                if (rbRebarSelected.Checked)
                {
                    rbRebarSelected.BackColor = DrawColor.FromArgb(37, 99, 235); // Active Blue
                    rbRebarSelected.ForeColor = DrawColor.White;
                    rbRebarSelected.FlatAppearance.BorderColor = DrawColor.FromArgb(96, 165, 250);
                    rbRebarSelected.FlatAppearance.BorderSize = 1;
                    rbRebarSelected.Font = new DrawFont(this.Font.FontFamily, 8.5F, FontStyle.Bold);

                    rbRebarAll.BackColor = DrawColor.FromArgb(30, 41, 59); // Inactive Dark Slate
                    rbRebarAll.ForeColor = DrawColor.FromArgb(148, 163, 184);
                    rbRebarAll.FlatAppearance.BorderColor = DrawColor.FromArgb(51, 65, 85);
                    rbRebarAll.FlatAppearance.BorderSize = 1;
                    rbRebarAll.Font = new DrawFont(this.Font.FontFamily, 8.5F, FontStyle.Regular);
                }
                else
                {
                    rbRebarAll.BackColor = DrawColor.FromArgb(37, 99, 235); // Active Blue
                    rbRebarAll.ForeColor = DrawColor.White;
                    rbRebarAll.FlatAppearance.BorderColor = DrawColor.FromArgb(96, 165, 250);
                    rbRebarAll.FlatAppearance.BorderSize = 1;
                    rbRebarAll.Font = new DrawFont(this.Font.FontFamily, 8.5F, FontStyle.Bold);

                    rbRebarSelected.BackColor = DrawColor.FromArgb(30, 41, 59); // Inactive Dark Slate
                    rbRebarSelected.ForeColor = DrawColor.FromArgb(148, 163, 184);
                    rbRebarSelected.FlatAppearance.BorderColor = DrawColor.FromArgb(51, 65, 85);
                    rbRebarSelected.FlatAppearance.BorderSize = 1;
                    rbRebarSelected.Font = new DrawFont(this.Font.FontFamily, 8.5F, FontStyle.Regular);
                }
            };

            rbRebarSelected.CheckedChanged += (s, e) => updateScopeStyles();
            rbRebarAll.CheckedChanged += (s, e) => updateScopeStyles();
            updateScopeStyles();

            // Group 2: File IFC
            Label lblIfc = new Label
            {
                Text = "File IFC:",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(365, 14),
                AutoSize = true
            };
            cboIfcFiles = new ComboBox
            {
                Location = new DrawPoint(425, 10),
                Width = 295,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.White,
                FlatStyle = FlatStyle.Flat
            };

            // Group 3: Tolerance & Clearance
            Label lblTol = new Label
            {
                Text = "Dung sai (mm):",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(730, 14),
                AutoSize = true
            };
            numTolerance = new NumericUpDown
            {
                Location = new DrawPoint(825, 11),
                Width = 60,
                Minimum = 0,
                Maximum = 500,
                Value = 1,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.White
            };

            Label lblClearance = new Label
            {
                Text = "Khoảng hở (mm):",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(900, 14),
                AutoSize = true
            };
            numClearance = new NumericUpDown
            {
                Location = new DrawPoint(1010, 11),
                Width = 60,
                Minimum = 0,
                Maximum = 500,
                Value = 0,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.White
            };

            // Row 2: Action Buttons
            btnScan = CreateFlatButton("⚡ Quét va chạm", new DrawPoint(15, 60), new DrawSize(150, 48), DrawColor.FromArgb(37, 99, 235), DrawColor.White);
            btnScan.Font = new DrawFont("Segoe UI", 9.5F, FontStyle.Bold);
            btnScan.Click += async (s, e) => await StartClashCheckAsync();

            btnStop = CreateFlatButton("⏹ Dừng", new DrawPoint(175, 60), new DrawSize(80, 48), DrawColor.FromArgb(75, 85, 99), DrawColor.White);
            btnStop.Enabled = false;
            btnStop.Click += (s, e) => _cts?.Cancel();

            btnZoomSelect = CreateFlatButton("🔍 Zoom & Chọn (Focus)", new DrawPoint(265, 60), new DrawSize(170, 48), DrawColor.FromArgb(13, 148, 136), DrawColor.White);
            btnZoomSelect.Click += (s, e) => ZoomToSelectedClash();

            btnHighlight = CreateFlatButton("📍 Đánh dấu 3D", new DrawPoint(445, 60), new DrawSize(130, 48), DrawColor.FromArgb(147, 51, 234), DrawColor.White);
            btnHighlight.Click += (s, e) => HighlightClashesInTekla();

            btnClearHighlight = CreateFlatButton("🧹 Xóa 3D", new DrawPoint(585, 60), new DrawSize(90, 48), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
            btnClearHighlight.Click += (s, e) => ClearHighlights();

            btnExportCsv = CreateFlatButton("📊 Xuất Excel/CSV", new DrawPoint(685, 60), new DrawSize(140, 48), DrawColor.FromArgb(16, 185, 129), DrawColor.White);
            btnExportCsv.Click += (s, e) => ExportToCsv();

            // Row 2: Vị trí ô màu đỏ (Lược bỏ cấu kiện IFC - to lên, hỗ trợ xuống dòng multiline)
            chkIgnoreFilter = new CheckBox
            {
                Text = "Lược bỏ cấu kiện IFC:",
                Checked = true,
                ForeColor = DrawColor.FromArgb(147, 197, 253),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new DrawPoint(835, 46),
                AutoSize = true
            };

            btnResetIgnore = CreateFlatButton("↺ Mặc định", new DrawPoint(1070, 43), new DrawSize(75, 22), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
            btnResetIgnore.Font = new DrawFont("Segoe UI", 7.5F);
            btnResetIgnore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnResetIgnore.Click += (s, e) => { txtIgnoreKeywords.Text = DefaultIgnoredList; };

            txtIgnoreKeywords = new TextBox
            {
                Location = new DrawPoint(835, 68),
                Width = 310,
                Height = 58,
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new DrawFont("Segoe UI", 8.5F),
                Text = LoadSavedIgnoredKeywords()
            };
            chkIgnoreFilter.CheckedChanged += (s, e) => txtIgnoreKeywords.Enabled = chkIgnoreFilter.Checked;

            ToolTip filterTooltip = new ToolTip();
            filterTooltip.SetToolTip(chkIgnoreFilter, "Bật/tắt bỏ qua các cấu kiện phụ IFC khi check va chạm với thép");
            filterTooltip.SetToolTip(txtIgnoreKeywords, "Nhập danh sách tên cấu kiện IFC cần lược bỏ (xuống dòng bằng Enter hoặc phân tách bằng dấu phẩy).\nVí dụ:\nBolt assembly\nSAFETY_BAR\nLUG\nLADDER\nSAFETY_HOOK\nVBRACE\nWELD_COUPLER(10)\nCHECK_COUPLER(10)");
            filterTooltip.SetToolTip(btnResetIgnore, "Khôi phục danh sách từ khóa mặc định");

            controlPanel.Controls.Add(lblScope);
            controlPanel.Controls.Add(rbRebarSelected);
            controlPanel.Controls.Add(rbRebarAll);
            controlPanel.Controls.Add(lblIfc);
            controlPanel.Controls.Add(cboIfcFiles);
            controlPanel.Controls.Add(lblTol);
            controlPanel.Controls.Add(numTolerance);
            controlPanel.Controls.Add(lblClearance);
            controlPanel.Controls.Add(numClearance);
            controlPanel.Controls.Add(btnScan);
            controlPanel.Controls.Add(btnStop);
            controlPanel.Controls.Add(btnZoomSelect);
            controlPanel.Controls.Add(btnHighlight);
            controlPanel.Controls.Add(btnClearHighlight);
            controlPanel.Controls.Add(btnExportCsv);
            controlPanel.Controls.Add(chkIgnoreFilter);
            controlPanel.Controls.Add(btnResetIgnore);
            controlPanel.Controls.Add(txtIgnoreKeywords);

            // 3. DataGridView
            dgvClashes = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = DrawColor.FromArgb(24, 28, 36),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                GridColor = DrawColor.FromArgb(45, 52, 65),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                RowTemplate = { Height = 28 },
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            };

            dgvClashes.ColumnHeadersDefaultCellStyle.BackColor = DrawColor.FromArgb(30, 35, 45);
            dgvClashes.ColumnHeadersDefaultCellStyle.ForeColor = DrawColor.FromArgb(148, 163, 184);
            dgvClashes.ColumnHeadersDefaultCellStyle.Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold);
            dgvClashes.ColumnHeadersHeight = 35;
            dgvClashes.DefaultCellStyle.BackColor = DrawColor.FromArgb(24, 28, 36);
            dgvClashes.DefaultCellStyle.SelectionBackColor = DrawColor.FromArgb(37, 99, 235);
            dgvClashes.DefaultCellStyle.SelectionForeColor = DrawColor.White;

            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColIndex", HeaderText = "#", Width = 45 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarId", HeaderText = "ID Thép", Width = 95 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarName", HeaderText = "Tên Thép", Width = 110 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarSize", HeaderText = "Kích thước", Width = 85 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarGrade", HeaderText = "Mác thép", Width = 85 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarPos", HeaderText = "Số hiệu (Pos)", Width = 100 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColHostPart", HeaderText = "Cấu kiện (Part)", Width = 120 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColIfcName", HeaderText = "Cấu kiện IFC", Width = 140 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColLength", HeaderText = "Chiều dài (mm)", Width = 100 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColOverlap", HeaderText = "Độ lấn (mm)", Width = 95 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColSeverity", HeaderText = "Mức độ", Width = 90 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColCoord", HeaderText = "Tọa độ va chạm (X, Y, Z)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            dgvClashes.CellFormatting += DgvClashes_CellFormatting;
            dgvClashes.CellDoubleClick += (s, e) => ZoomToSelectedClash();

            // 4. Status Strip
            statusStrip = new StatusStrip
            {
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(156, 163, 175)
            };

            lblStatusText = new ToolStripStatusLabel
            {
                Text = "Sẵn sàng kiểm tra.",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            progressBar = new ToolStripProgressBar
            {
                Width = 220,
                Visible = false,
                Style = ProgressBarStyle.Continuous
            };

            lblCountText = new ToolStripStatusLabel
            {
                Text = "0 va chạm",
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                BorderStyle = Border3DStyle.Etched
            };

            statusStrip.Items.Add(lblStatusText);
            statusStrip.Items.Add(progressBar);
            statusStrip.Items.Add(lblCountText);

            // Assemble Form
            this.Controls.Add(dgvClashes);
            this.Controls.Add(controlPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(statusStrip);
        }

        private Button CreateFlatButton(string text, DrawPoint loc, DrawSize size, DrawColor bg, DrawColor fg)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void ConnectTekla()
        {
            try
            {
                _model = new Model();
                if (_model.GetConnectionStatus())
                {
                    _detector = new ClashDetector(_model);
                    var info = _model.GetInfo();
                    lblTeklaStatus.Text = string.Format("● Đã kết nối: {0}", info.ModelName);
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(34, 197, 94); // Green

                    // Populate IFC ComboBox
                    PopulateIfcComboBox();
                }
                else
                {
                    lblTeklaStatus.Text = "● Chưa kết nối Tekla";
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68); // Red
                }
            }
            catch (Exception ex)
            {
                lblTeklaStatus.Text = "● Lỗi kết nối Tekla";
                lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68);
                lblStatusText.Text = "Lỗi khởi tạo Tekla API: " + ex.Message;
            }
        }

        private void PopulateIfcComboBox()
        {
            cboIfcFiles.Items.Clear();
            cboIfcFiles.Items.Add("⭐ Tự động lọc tất cả file IFC (Navisworks Auto)");
            cboIfcFiles.Items.Add("🎯 Chỉ cấu kiện IFC / Part đang chọn trong Tekla");

            if (_detector == null) return;
            var refModels = _detector.GetReferenceModels();
            foreach (var r in refModels)
            {
                string fn = Path.GetFileName(r.Filename ?? string.Empty);
                if (!string.IsNullOrEmpty(fn) && !cboIfcFiles.Items.Contains(fn))
                {
                    cboIfcFiles.Items.Add(fn);
                }
            }
            if (cboIfcFiles.Items.Count > 0)
                cboIfcFiles.SelectedIndex = 0;
        }

        private async SysTask StartClashCheckAsync()
        {
            if (_model == null || !_model.GetConnectionStatus())
            {
                MessageBox.Show(this, "Chưa kết nối được với Tekla Structures!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cts = new CancellationTokenSource();
            btnScan.Enabled = false;
            btnStop.Enabled = true;
            progressBar.Visible = true;
            progressBar.Value = 0;
            lblStatusText.Text = "Đang chuẩn bị dữ liệu...";
            dgvClashes.Rows.Clear();
            _currentClashes.Clear();
            ClearHighlights();

            IfcScopeMode ifcMode = IfcScopeMode.AutoSpatialAllIfc;
            if (cboIfcFiles.SelectedIndex == 1)
                ifcMode = IfcScopeMode.SelectedIfcOnly;
            else if (cboIfcFiles.SelectedIndex > 1)
                ifcMode = IfcScopeMode.SpecificFile;

            try
            {
                File.WriteAllText(FilterSettingsFile, txtIgnoreKeywords.Text);
            }
            catch { }

            var settings = new ClashSettings
            {
                OnlySelectedRebars = rbRebarSelected.Checked,
                IfcMode = ifcMode,
                TargetIfcFileName = cboIfcFiles.SelectedIndex > 1 ? cboIfcFiles.SelectedItem.ToString() : "ALL",
                ToleranceMm = (double)numTolerance.Value,
                ClearanceMm = (double)numClearance.Value,
                EnableIgnoredComponents = chkIgnoreFilter.Checked,
                IgnoredKeywords = ParseKeywords(txtIgnoreKeywords.Text)
            };

            // 1. Collect Rebars & Obstacles (Navisworks Selection Sets)
            List<Reinforcement> targetRebars = new List<Reinforcement>();
            List<ModelObject> selectedObstacles = new List<ModelObject>();

            TeklaPoint zoneMin = new TeklaPoint(double.MaxValue, double.MaxValue, double.MaxValue);
            TeklaPoint zoneMax = new TeklaPoint(double.MinValue, double.MinValue, double.MinValue);

            HashSet<long> addedRebarIds = new HashSet<long>();
            Action<Reinforcement> addRebar = (r) =>
            {
                if (r == null || !addedRebarIds.Add(r.Identifier.ID)) return;
                targetRebars.Add(r);
                double bMinX = 0, bMinY = 0, bMinZ = 0, bMaxX = 0, bMaxY = 0, bMaxZ = 0;
                if (r.GetReportProperty("BOUNDING_BOX_MIN_X", ref bMinX) &&
                    r.GetReportProperty("BOUNDING_BOX_MIN_Y", ref bMinY) &&
                    r.GetReportProperty("BOUNDING_BOX_MIN_Z", ref bMinZ) &&
                    r.GetReportProperty("BOUNDING_BOX_MAX_X", ref bMaxX) &&
                    r.GetReportProperty("BOUNDING_BOX_MAX_Y", ref bMaxY) &&
                    r.GetReportProperty("BOUNDING_BOX_MAX_Z", ref bMaxZ) && bMaxX > bMinX)
                {
                    zoneMin.X = Math.Min(zoneMin.X, bMinX);
                    zoneMin.Y = Math.Min(zoneMin.Y, bMinY);
                    zoneMin.Z = Math.Min(zoneMin.Z, bMinZ);

                    zoneMax.X = Math.Max(zoneMax.X, bMaxX);
                    zoneMax.Y = Math.Max(zoneMax.Y, bMaxY);
                    zoneMax.Z = Math.Max(zoneMax.Z, bMaxZ);
                }
                else
                {
                    try
                    {
                        var s = r.GetSolid();
                        if (s != null)
                        {
                            zoneMin.X = Math.Min(zoneMin.X, s.MinimumPoint.X);
                            zoneMin.Y = Math.Min(zoneMin.Y, s.MinimumPoint.Y);
                            zoneMin.Z = Math.Min(zoneMin.Z, s.MinimumPoint.Z);

                            zoneMax.X = Math.Max(zoneMax.X, s.MaximumPoint.X);
                            zoneMax.Y = Math.Max(zoneMax.Y, s.MaximumPoint.Y);
                            zoneMax.Z = Math.Max(zoneMax.Z, s.MaximumPoint.Z);
                        }
                    }
                    catch { }
                }
            };

            if (settings.OnlySelectedRebars)
            {
                var selector = new TeklaUiSelector();
                var selectedObjects = selector.GetSelectedObjects();
                while (selectedObjects.MoveNext())
                {
                    var obj = selectedObjects.Current;
                    if (obj is Reinforcement rebar)
                    {
                        addRebar(rebar);
                    }
                    else if (obj is Part part)
                    {
                        try
                        {
                            var partRebars = part.GetReinforcements();
                            while (partRebars.MoveNext())
                            {
                                if (partRebars.Current is Reinforcement pr) addRebar(pr);
                            }
                        }
                        catch { }
                    }
                    else if (obj is Assembly assy)
                    {
                        try
                        {
                            // Collect rebars from main part first
                            var mpObj = assy.GetMainPart();
                            if (mpObj is Reinforcement mainRebar)
                            {
                                addRebar(mainRebar);
                            }
                            else if (mpObj is Part mainPart)
                            {
                                var mainRebars = mainPart.GetReinforcements();
                                while (mainRebars.MoveNext())
                                {
                                    if (mainRebars.Current is Reinforcement mpr) addRebar(mpr);
                                }
                            }

                            // Then collect from secondary parts
                            var sec = assy.GetSecondaries();
                            if (sec != null)
                            {
                                foreach (object item in sec)
                                {
                                    if (item is Reinforcement ar) addRebar(ar);
                                    else if (item is Part ap)
                                    {
                                        var apr = ap.GetReinforcements();
                                        while (apr.MoveNext())
                                        {
                                            if (apr.Current is Reinforcement pr) addRebar(pr);
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    else if (obj is ReferenceModelObject || obj is ReferenceModel)
                    {
                        selectedObstacles.Add(obj);
                    }
                }

                if (targetRebars.Count == 0)
                {
                    MessageBox.Show(this, "Bạn chưa chọn thanh thép hoặc cấu kiện bê tông nào trong mô hình Tekla!\n\nMẹo: Bạn có thể chọn trực tiếp Thanh thép hoặc Dầm/Cột bê tông trên màn hình 3D để kiểm tra va chạm.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetUiState();
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
                        if (rebarEnum.Current is Reinforcement rebar)
                        {
                            targetRebars.Add(rebar);
                            double bMinX = 0, bMinY = 0, bMinZ = 0, bMaxX = 0, bMaxY = 0, bMaxZ = 0;
                            if (rebar.GetReportProperty("BOUNDING_BOX_MIN_X", ref bMinX) &&
                                rebar.GetReportProperty("BOUNDING_BOX_MIN_Y", ref bMinY) &&
                                rebar.GetReportProperty("BOUNDING_BOX_MIN_Z", ref bMinZ) &&
                                rebar.GetReportProperty("BOUNDING_BOX_MAX_X", ref bMaxX) &&
                                rebar.GetReportProperty("BOUNDING_BOX_MAX_Y", ref bMaxY) &&
                                rebar.GetReportProperty("BOUNDING_BOX_MAX_Z", ref bMaxZ) && bMaxX > bMinX)
                            {
                                zoneMin.X = Math.Min(zoneMin.X, bMinX);
                                zoneMin.Y = Math.Min(zoneMin.Y, bMinY);
                                zoneMin.Z = Math.Min(zoneMin.Z, bMinZ);

                                zoneMax.X = Math.Max(zoneMax.X, bMaxX);
                                zoneMax.Y = Math.Max(zoneMax.Y, bMaxY);
                                zoneMax.Z = Math.Max(zoneMax.Z, bMaxZ);
                            }
                            else
                            {
                                try
                                {
                                    var s = rebar.GetSolid();
                                    if (s != null)
                                    {
                                        zoneMin.X = Math.Min(zoneMin.X, s.MinimumPoint.X);
                                        zoneMin.Y = Math.Min(zoneMin.Y, s.MinimumPoint.Y);
                                        zoneMin.Z = Math.Min(zoneMin.Z, s.MinimumPoint.Z);

                                        zoneMax.X = Math.Max(zoneMax.X, s.MaximumPoint.X);
                                        zoneMax.Y = Math.Max(zoneMax.Y, s.MaximumPoint.Y);
                                        zoneMax.Z = Math.Max(zoneMax.Z, s.MaximumPoint.Z);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }

            lblStatusText.Text = string.Format("Đang phân tích {0} thép & lọc cấu kiện IFC lân cận...", targetRebars.Count);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            List<ClashResultItem> results = null;

            try
            {
                results = await SysTask.Run(() =>
                {
                    // 1. Spatial Obstacle Collection (Navisworks BVH / Bounding Filter)
                    var obstacles = _detector.CollectObstacles(zoneMin, zoneMax, settings, selectedObstacles, (status) =>
                    {
                        this.Invoke(new Action(() => { lblStatusText.Text = status; }));
                    });

                    this.Invoke(new Action(() =>
                    {
                        if (obstacles.Count == 0)
                        {
                            lblStatusText.Text = "Không tìm thấy cấu kiện IFC nào trong vùng quét (hoặc tất cả đã bị loại bởi bộ lọc từ khóa).";
                        }
                        else
                        {
                            lblStatusText.Text = string.Format("Tìm thấy {0} cấu kiện IFC trong vùng thép. Đang tính toán va chạm hình học...", obstacles.Count);
                        }
                    }));

                    // 2. Exact Navisworks Segment-to-Box Hard Clash Detection with UI Throttling
                    var progressSw = System.Diagnostics.Stopwatch.StartNew();
                    return _detector.DetectClashes(targetRebars, obstacles, settings, (curr, max) =>
                    {
                        if (max > 0 && (curr == max || progressSw.ElapsedMilliseconds >= 80))
                        {
                            progressSw.Restart();
                            int pct = (int)((curr * 100.0) / max);
                            if (pct > 100) pct = 100;
                            try
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    if (progressBar.Visible) progressBar.Value = pct;
                                    lblStatusText.Text = string.Format("Đang kiểm tra: {0}/{1} thép ({2}%)", curr, max, pct);
                                }));
                            }
                            catch { }
                        }
                    }, _cts.Token);
                }, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                lblStatusText.Text = "Đã dừng quét va chạm bởi người dùng.";
                ResetUiState();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi trong quá trình quét: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUiState();
                return;
            }

            sw.Stop();
            _currentClashes = results ?? new List<ClashResultItem>();

            // Final safety filter: Guarantee no ignored component slips through to UI
            if (settings.EnableIgnoredComponents && settings.IgnoredKeywords != null && settings.IgnoredKeywords.Count > 0)
            {
                var filtered = new List<ClashResultItem>();
                foreach (var c in _currentClashes)
                {
                    if (ClashDetector.IsIgnoredComponent(c.IfcEntityName, settings.IgnoredKeywords))
                        continue;
                    filtered.Add(c);
                }
                _currentClashes = filtered;

                for (int i = 0; i < _currentClashes.Count; i++)
                {
                    _currentClashes[i].Index = i + 1;
                }
            }

            // Populate Grid
            dgvClashes.SuspendLayout();
            foreach (var c in _currentClashes)
            {
                int rowIdx = dgvClashes.Rows.Add(
                    c.Index,
                    c.RebarId,
                    c.RebarName,
                    c.RebarSize,
                    c.RebarGrade,
                    c.RebarPos,
                    c.HostPartName,
                    !string.IsNullOrEmpty(c.IfcEntityName) ? c.IfcEntityName : c.IfcFileName,
                    c.RebarLength > 0 ? c.RebarLength.ToString("F0") : "-",
                    c.OverlapMm.ToString("F1"),
                    c.Severity.ToString(),
                    c.ClashPointDisplay
                );
                dgvClashes.Rows[rowIdx].Tag = c;
            }
            dgvClashes.ResumeLayout();

            lblCountText.Text = string.Format("{0} va chạm", _currentClashes.Count);
            lblStatusText.Text = string.Format("Hoàn tất quét! Phát hiện {0} va chạm trong {1:F1} giây.", _currentClashes.Count, sw.Elapsed.TotalSeconds);
            ResetUiState();
        }

        private void ResetUiState()
        {
            btnScan.Enabled = true;
            btnStop.Enabled = false;
            progressBar.Visible = false;
        }

        private void DgvClashes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvClashes.Rows.Count) return;
            var item = dgvClashes.Rows[e.RowIndex].Tag as ClashResultItem;
            if (item == null) return;

            if (dgvClashes.Columns[e.ColumnIndex].Name == "ColSeverity" || dgvClashes.Columns[e.ColumnIndex].Name == "ColOverlap")
            {
                DrawColor fg;
                switch (item.Severity)
                {
                    case ClashSeverity.Severe:
                        fg = DrawColor.FromArgb(239, 68, 68); // Red
                        break;
                    case ClashSeverity.Medium:
                        fg = DrawColor.FromArgb(249, 115, 22); // Orange
                        break;
                    default:
                        fg = DrawColor.FromArgb(234, 179, 8); // Yellow
                        break;
                }
                e.CellStyle.ForeColor = fg;
                e.CellStyle.Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private void ZoomToSelectedClash()
        {
            if (dgvClashes.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn một dòng va chạm trong bảng!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var objsToSelect = new ArrayList();
                TeklaPoint minPt = new TeklaPoint(double.MaxValue, double.MaxValue, double.MaxValue);
                TeklaPoint maxPt = new TeklaPoint(double.MinValue, double.MinValue, double.MinValue);

                foreach (DataGridViewRow row in dgvClashes.SelectedRows)
                {
                    var c = row.Tag as ClashResultItem;
                    if (c == null) continue;

                    // Strictly select the clashing rebar object
                    if (c.RebarObject != null) objsToSelect.Add(c.RebarObject);

                    if (c.ClashPoint != null)
                    {
                        double pad = 350.0;
                        minPt.X = Math.Min(minPt.X, c.ClashPoint.X - pad);
                        minPt.Y = Math.Min(minPt.Y, c.ClashPoint.Y - pad);
                        minPt.Z = Math.Min(minPt.Z, c.ClashPoint.Z - pad);

                        maxPt.X = Math.Max(maxPt.X, c.ClashPoint.X + pad);
                        maxPt.Y = Math.Max(maxPt.Y, c.ClashPoint.Y + pad);
                        maxPt.Z = Math.Max(maxPt.Z, c.ClashPoint.Z + pad);
                    }
                    else if (c.MinPoint != null && c.MaxPoint != null)
                    {
                        minPt.X = Math.Min(minPt.X, c.MinPoint.X - 200.0);
                        minPt.Y = Math.Min(minPt.Y, c.MinPoint.Y - 200.0);
                        minPt.Z = Math.Min(minPt.Z, c.MinPoint.Z - 200.0);

                        maxPt.X = Math.Max(maxPt.X, c.MaxPoint.X + 200.0);
                        maxPt.Y = Math.Max(maxPt.Y, c.MaxPoint.Y + 200.0);
                        maxPt.Z = Math.Max(maxPt.Z, c.MaxPoint.Z + 200.0);
                    }
                }

                if (objsToSelect.Count > 0)
                {
                    var selector = new TeklaUiSelector();
                    selector.Select(objsToSelect);

                    if (minPt.X < double.MaxValue && maxPt.X > double.MinValue)
                    {
                        AABB aabb = new AABB(minPt, maxPt);
                        ViewHandler.ZoomToBoundingBox(aabb);
                    }
                    lblStatusText.Text = string.Format("Đã Zoom & Chọn va chạm #{0} trên mô hình Tekla!", ((ClashResultItem)dgvClashes.SelectedRows[0].Tag).Index);
                }
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Lỗi khi Zoom trong Tekla: " + ex.Message;
            }
        }

        private void HighlightClashesInTekla()
        {
            if (_currentClashes.Count == 0)
            {
                MessageBox.Show(this, "Không có va chạm nào để đánh dấu!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ClearHighlights();

            try
            {
                var redColor = new TeklaColor(1.0, 0.0, 0.0);
                double markerSize = 250.0;

                foreach (var c in _currentClashes)
                {
                    if (c.ClashPoint == null) continue;

                    // Draw 3D Cross Marker at clash location
                    TeklaPoint p = c.ClashPoint;

                    var pListX = new ArrayList { new TeklaPoint(p.X - markerSize, p.Y, p.Z), new TeklaPoint(p.X + markerSize, p.Y, p.Z) };
                    var pListY = new ArrayList { new TeklaPoint(p.X, p.Y - markerSize, p.Z), new TeklaPoint(p.X, p.Y + markerSize, p.Z) };
                    var pListZ = new ArrayList { new TeklaPoint(p.X, p.Y, p.Z - markerSize), new TeklaPoint(p.X, p.Y, p.Z + markerSize) };

                    var polyX = new GraphicPolyLine { Color = redColor, Width = 3, PolyLine = new PolyLine(pListX) };
                    var polyY = new GraphicPolyLine { Color = redColor, Width = 3, PolyLine = new PolyLine(pListY) };
                    var polyZ = new GraphicPolyLine { Color = redColor, Width = 3, PolyLine = new PolyLine(pListZ) };

                    _activeHighlights.Add(polyX);
                    _activeHighlights.Add(polyY);
                    _activeHighlights.Add(polyZ);
                }

                var drawer = new GraphicsDrawer();
                foreach (var g in _activeHighlights)
                {
                    drawer.DrawPolyLine(g);
                }

                lblStatusText.Text = string.Format("Đã đánh dấu {0} điểm va chạm bằng dấu X màu đỏ trên 3D!", _currentClashes.Count);
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Lỗi khi vẽ đánh dấu 3D: " + ex.Message;
            }
        }

        private void ClearHighlights()
        {
            try
            {
                if (_activeHighlights.Count > 0)
                {
                    _activeHighlights.Clear();
                    _model?.CommitChanges();
                    var viewEnum = ViewHandler.GetVisibleViews();
                    while (viewEnum.MoveNext())
                    {
                        ViewHandler.RedrawView(viewEnum.Current);
                    }
                }
            }
            catch { }
        }

        private void ExportToCsv()
        {
            if (_currentClashes.Count == 0)
            {
                MessageBox.Show(this, "Không có dữ liệu để xuất!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = string.Format("Clash_Report_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("STT,ID_Thep,Ten_Thep,Kich_Thuoc,Mac_Thep,So_Hieu_Pos,Cau_Kien_Part,Cau_Kien_IFC,Chieu_Dai_mm,Do_Lan_mm,Muc_Do,Toa_Do_X,Toa_Do_Y,Toa_Do_Z");

                        foreach (var c in _currentClashes)
                        {
                            string cx = c.ClashPoint != null ? c.ClashPoint.X.ToString("F1") : "";
                            string cy = c.ClashPoint != null ? c.ClashPoint.Y.ToString("F1") : "";
                            string cz = c.ClashPoint != null ? c.ClashPoint.Z.ToString("F1") : "";

                            sb.AppendLine(string.Format("{0},{1},\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",{8},{9},{10},{11},{12},{13}",
                                c.Index,
                                c.RebarId,
                                c.RebarName,
                                c.RebarSize,
                                c.RebarGrade,
                                c.RebarPos,
                                c.HostPartName,
                                !string.IsNullOrEmpty(c.IfcEntityName) ? c.IfcEntityName : c.IfcFileName,
                                c.RebarLength,
                                c.OverlapMm.ToString("F1"),
                                c.Severity,
                                cx, cy, cz
                            ));
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show(this, "Đã xuất báo cáo thành công ra file:\n" + sfd.FileName, "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { XbimGeometryManager.Clear(); } catch { }
            base.OnFormClosing(e);
        }
    }
}
