using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SysTask = System.Threading.Tasks.Task;
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
    /// <summary>
    /// Giao diện chính của công cụ Kiểm tra Va chạm Thép và Cấu kiện IFC (Tekla Clash Check).
    /// Cung cấp giao diện đồ họa Dark Theme hiện đại, trực quan, hỗ trợ tương tác 3D hai chiều với Tekla Structures:
    /// - Quét va chạm song song đa luồng tốc độ cao (Navisworks Style).
    /// - Tự động thu phóng (Zoom &amp; Focus) đến vị trí va chạm trên mô hình 3D.
    /// - Đánh dấu điểm va chạm bằng dấu X 3D trực quan.
    /// - Xuất báo cáo va chạm chi tiết ra định dạng Excel/CSV UTF-8.
    /// </summary>
    public class MainForm : Form
    {
        private Model _model;
        private ClashDetector _detector;
        private List<ClashResultItem> _currentClashes = new List<ClashResultItem>();
        private readonly List<int> _activeHighlights = new List<int>();
        private readonly object _highlightLock = new object();
        private CancellationTokenSource _cts;

        // Các thành phần điều khiển giao diện (UI Controls)
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblTeklaStatus;

        private Panel controlPanel;
        private RadioButton rbRebarAll;
        private RadioButton rbRebarSelected;
        private Button btnIfcSelect;
        private readonly List<string> _availableIfcFiles = new List<string>();
        private readonly HashSet<string> _selectedIfcFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _ifcSelectedPartsOnly = false;
        private ToolTip _ifcTooltip;
        private NumericUpDown numTolerance;
        private NumericUpDown numClearance;
        private CheckBox chkIgnoreFilter;
        private TextBox txtIgnoreKeywords;
        private Button btnResetIgnore;
        private const string DefaultIgnoredList = "Bolt assembly\r\nSAFETY_BAR\r\nLUG\r\nLADDER\r\nSAFETY_HOOK\r\nVBRACE\r\nWELD_COUPLER\r\nCHECK_COUPLER";

        private CheckBox chkOnlyFilter;
        private TextBox txtOnlyKeywords;
        private Button btnClearOnly;

        /// <summary>
        /// Đường dẫn file lưu trữ danh sách từ khóa bỏ qua (SkipNames) trên máy tính người dùng.
        /// </summary>
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

        /// <summary>
        /// Đường dẫn file lưu trữ danh sách từ khóa chỉ định quét (OnlyNames) trên máy tính người dùng.
        /// </summary>
        private static string OnlyFilterSettingsFile
        {
            get
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string candidate = Path.Combine(baseDir, "clash_only_filters.txt");
                    if (File.Exists(candidate)) return candidate;

                    string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tekla_ClashCheck");
                    if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
                    string appDataPath = Path.Combine(appData, "clash_only_filters.txt");
                    if (File.Exists(appDataPath)) return appDataPath;

                    return candidate;
                }
                catch
                {
                    return "clash_only_filters.txt";
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

        // Context Menu chuột phải cho DataGridView kết quả va chạm
        private ContextMenuStrip _clashContextMenu;
        private ToolStripMenuItem _menuItemHideRow;
        private ToolStripMenuItem _menuItemUnhideAll;
        private ToolStripMenuItem _menuItemZoom;
        private ToolStripMenuItem _menuItemCopy;

        /// <summary>
        /// Tải danh sách từ khóa bỏ qua đã lưu từ cấu hình hoặc trả về danh sách mặc định.
        /// </summary>
        private static string LoadSavedIgnoredKeywords()
        {
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.SkipNames))
                {
                    return Properties.Settings.Default.SkipNames;
                }
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
                        return content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                    }
                }
            }
            catch { }
            return DefaultIgnoredList;
        }

        /// <summary>
        /// Tải danh sách từ khóa chỉ quét đã lưu từ cấu hình (OnlyNames).
        /// </summary>
        private static string LoadSavedOnlyKeywords()
        {
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.OnlyNames))
                {
                    return Properties.Settings.Default.OnlyNames;
                }
                if (File.Exists(OnlyFilterSettingsFile))
                {
                    string content = File.ReadAllText(OnlyFilterSettingsFile).Trim();
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
                        return content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// Phân tích chuỗi từ khóa nhập vào thành danh sách các chuỗi con riêng biệt.
        /// </summary>
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

        /// <summary>
        /// Hàm khởi tạo MainForm, thiết lập giao diện, nạp cấu hình đã lưu và kết nối Tekla Open API.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            ConnectTekla();
        }

        private void InitializeComponent()
        {
            this.Text = "Tekla Clash Check (Rebar vs IFC) - [My-tool]";
            this.Size = new DrawSize(1280, 780);

            this.MinimumSize = new DrawSize(1100, 650);
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
                Text = "CLASH CHECK: REBAR VS IFC",
                Font = new DrawFont("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = DrawColor.FromArgb(56, 189, 248),
                AutoSize = true,
                Location = new DrawPoint(15, 12)
            };

            lblSubtitle = new Label
            {
                Text = "Clash & clearance check between Rebars (Tekla Model) and Reference IFC Objects (Navisworks Style)",
                Font = new DrawFont("Segoe UI", 8.5F),
                ForeColor = DrawColor.FromArgb(156, 163, 175),
                AutoSize = true,
                Location = new DrawPoint(16, 40)
            };

            lblTeklaStatus = new Label
            {
                Text = "● Connecting to Tekla...",
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
                Height = 152,
                BackColor = DrawColor.FromArgb(30, 35, 45),
                Padding = new Padding(15, 10, 15, 10)
            };

            // Group 1: Rebar Scope
            Label lblScope = new Label
            {
                Text = "Rebar Scope:",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(15, 14),
                AutoSize = true
            };
            rbRebarSelected = new RadioButton
            {
                Text = "🎯 Selected Rebars",
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
                Text = "🌐 All Rebars",
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

            // Group 2: IFC Model
            Label lblIfc = new Label
            {
                Text = "IFC Model:",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(365, 14),
                AutoSize = true
            };
            _ifcTooltip = new ToolTip();
            btnIfcSelect = new Button
            {
                Location = new DrawPoint(435, 10),
                Width = 285,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new DrawFont("Segoe UI", 8.5F),
                Text = "⭐ All IFC Models (Navisworks Auto)  ▼",
                Cursor = Cursors.Hand
            };
            btnIfcSelect.FlatAppearance.BorderColor = DrawColor.FromArgb(51, 65, 85);
            btnIfcSelect.Click += (s, e) => ShowIfcSelectionDropdown();
            _ifcTooltip.SetToolTip(btnIfcSelect, "Click to open selector and select multiple IFC files");

            // Group 3: Tolerance & Clearance
            Label lblTol = new Label
            {
                Text = "Tolerance (mm):",
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
                Text = "Clearance (mm):",
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Location = new DrawPoint(895, 14),
                AutoSize = true
            };
            numClearance = new NumericUpDown
            {
                Location = new DrawPoint(995, 11),
                Width = 60,
                Minimum = 0,
                Maximum = 500,
                Value = 0,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.White
            };

            // Row 2: Action Buttons
            btnScan = CreateFlatButton("⚡ Run Clash Check", new DrawPoint(15, 60), new DrawSize(140, 78), DrawColor.FromArgb(37, 99, 235), DrawColor.White);
            btnScan.Font = new DrawFont("Segoe UI", 10F, FontStyle.Bold);
            btnScan.Click += async (s, e) => await StartClashCheckAsync();

            btnStop = CreateFlatButton("⏹ Stop", new DrawPoint(160, 60), new DrawSize(68, 78), DrawColor.FromArgb(75, 85, 99), DrawColor.White);
            btnStop.Enabled = false;
            btnStop.Click += (s, e) => _cts?.Cancel();

            btnZoomSelect = CreateFlatButton("🔍 Zoom Selected", new DrawPoint(234, 60), new DrawSize(145, 36), DrawColor.FromArgb(13, 148, 136), DrawColor.White);
            btnZoomSelect.Click += (s, e) => ZoomToSelectedClash();

            btnHighlight = CreateFlatButton("📍 Highlight 3D", new DrawPoint(385, 60), new DrawSize(115, 36), DrawColor.FromArgb(147, 51, 234), DrawColor.White);
            btnHighlight.Click += (s, e) => HighlightClashesInTekla();

            btnClearHighlight = CreateFlatButton("🧹 Clear 3D", new DrawPoint(506, 60), new DrawSize(80, 36), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
            btnClearHighlight.Click += (s, e) => ClearHighlights(clearSelection: true);

            btnExportCsv = CreateFlatButton("📊 Export Report (Excel/CSV)", new DrawPoint(234, 102), new DrawSize(352, 36), DrawColor.FromArgb(16, 185, 129), DrawColor.White);
            btnExportCsv.Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold);
            btnExportCsv.Click += (s, e) => ExportToCsv();

            // Row 2 - Filter Column 1: Skip IFC components (SkipNames)
            chkIgnoreFilter = new CheckBox
            {
                Text = "Skip Filter (SkipNames):",
                Checked = true,
                ForeColor = DrawColor.FromArgb(147, 197, 253),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new DrawPoint(600, 48),
                AutoSize = true
            };

            btnResetIgnore = CreateFlatButton("↺ Default", new DrawPoint(830, 45), new DrawSize(70, 22), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
            btnResetIgnore.Font = new DrawFont("Segoe UI", 7.5F);
            btnResetIgnore.Click += (s, e) => { txtIgnoreKeywords.Text = DefaultIgnoredList; };

            txtIgnoreKeywords = new TextBox
            {
                Location = new DrawPoint(600, 70),
                Width = 300,
                Height = 68,
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new DrawFont("Segoe UI", 8.5F),
                Text = LoadSavedIgnoredKeywords()
            };
            chkIgnoreFilter.CheckedChanged += (s, e) => txtIgnoreKeywords.Enabled = chkIgnoreFilter.Checked;

            // Row 2 - Filter Column 2: Only scan designated IFC components (OnlyNames)
            chkOnlyFilter = new CheckBox
            {
                Text = "Only Filter (OnlyNames):",
                Checked = false,
                ForeColor = DrawColor.FromArgb(134, 239, 172),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new DrawPoint(920, 48),
                AutoSize = true
            };

            btnClearOnly = CreateFlatButton("✖ Clear", new DrawPoint(1150, 45), new DrawSize(70, 22), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
            btnClearOnly.Font = new DrawFont("Segoe UI", 7.5F);
            btnClearOnly.Click += (s, e) => { txtOnlyKeywords.Text = string.Empty; };

            txtOnlyKeywords = new TextBox
            {
                Location = new DrawPoint(920, 70),
                Width = 300,
                Height = 68,
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new DrawFont("Segoe UI", 8.5F),
                Enabled = false,
                Text = LoadSavedOnlyKeywords()
            };
            chkOnlyFilter.CheckedChanged += (s, e) => txtOnlyKeywords.Enabled = chkOnlyFilter.Checked;

            ToolTip filterTooltip = new ToolTip();
            filterTooltip.SetToolTip(chkIgnoreFilter, "Enable/disable skipping auxiliary IFC objects when clash checking with rebar (IfcConvertOptions.AddSkipNames)");
            filterTooltip.SetToolTip(txtIgnoreKeywords, "Enter list of IFC object names/keywords to skip (one keyword per line or comma separated).\nExample:\nBolt assembly\nSAFETY_BAR\nLUG\nLADDER\nSAFETY_HOOK\nVBRACE\nWELD_COUPLER(10)\nCHECK_COUPLER(10)");
            filterTooltip.SetToolTip(btnResetIgnore, "Reset to default skip keywords");

            filterTooltip.SetToolTip(chkOnlyFilter, "Enable/disable ONLY checking designated IFC objects (IfcConvertOptions.AddOnlyNames)");
            filterTooltip.SetToolTip(txtOnlyKeywords, "Enter list of designated IFC object names/keywords to scan (one keyword per line or comma separated).\nWhen enabled, only objects matching these keywords will be extracted and clash checked.\nExample:\nBEAM\nCOLUMN\nSLAB\nWALL\nPIPE");
            filterTooltip.SetToolTip(btnClearOnly, "Clear designated objects list to check all");

            controlPanel.Controls.Add(lblScope);
            controlPanel.Controls.Add(rbRebarSelected);
            controlPanel.Controls.Add(rbRebarAll);
            controlPanel.Controls.Add(lblIfc);
            controlPanel.Controls.Add(btnIfcSelect);
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
            controlPanel.Controls.Add(chkOnlyFilter);
            controlPanel.Controls.Add(btnClearOnly);
            controlPanel.Controls.Add(txtOnlyKeywords);

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
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarId", HeaderText = "Rebar ID", Width = 95 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarName", HeaderText = "Rebar Name", Width = 110 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarSize", HeaderText = "Size", Width = 85 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarGrade", HeaderText = "Grade", Width = 85 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRebarPos", HeaderText = "Pos (Mark)", Width = 100 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColHostPart", HeaderText = "Host Part", Width = 120 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColIfcName", HeaderText = "IFC Entity", Width = 140 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColLength", HeaderText = "Length (mm)", Width = 100 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColOverlap", HeaderText = "Overlap (mm)", Width = 95 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColSeverity", HeaderText = "Severity", Width = 90 });
            dgvClashes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColCoord", HeaderText = "Clash Point (X, Y, Z)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            dgvClashes.CellFormatting += DgvClashes_CellFormatting;
            dgvClashes.CellDoubleClick += (s, e) => ZoomToSelectedClash();
            SetupClashesContextMenu();

            // 4. Status Strip
            statusStrip = new StatusStrip
            {
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(156, 163, 175)
            };

            lblStatusText = new ToolStripStatusLabel
            {
                Text = "Ready.",
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
                Text = "0 clashes",
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

        /// <summary>
        /// Helper method to create a flat button with modern dark theme styling.
        /// </summary>
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

        /// <summary>
        /// Connect to current Tekla Structures model via Tekla Open API.
        /// </summary>
        private void ConnectTekla()
        {
            try
            {
                _model = new Model();
                if (_model.GetConnectionStatus())
                {
                    _detector = new ClashDetector(_model);
                    var info = _model.GetInfo();
                    lblTeklaStatus.Text = string.Format("● Connected: {0}", info.ModelName);
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(34, 197, 94); // Green indicates connected

                    // Populate referenced IFC models into selection list
                    PopulateIfcComboBox();
                }
                else
                {
                    lblTeklaStatus.Text = "● Not connected to Tekla";
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68); // Red warning
                }
            }
            catch (Exception ex)
            {
                lblTeklaStatus.Text = "● Tekla connection error";
                lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68);
                lblStatusText.Text = "Tekla API initialization error: " + ex.Message;
            }
        }

        /// <summary>
        /// Cập nhật nhãn văn bản và tooltip hiển thị trên nút chọn file IFC dựa trên các lựa chọn hiện tại.
        /// </summary>
        private void UpdateIfcButtonDisplay()
        {
            if (_ifcSelectedPartsOnly)
            {
                btnIfcSelect.Text = "🎯 Selected IFC Objects Only  ▼";
                btnIfcSelect.ForeColor = DrawColor.FromArgb(96, 165, 250);
                _ifcTooltip.SetToolTip(btnIfcSelect, "Mode: Check selected IFC objects or Parts directly in Tekla model");
            }
            else if (_selectedIfcFiles.Count == 0)
            {
                btnIfcSelect.Text = "⭐ All IFC Models (Navisworks Auto)  ▼";
                btnIfcSelect.ForeColor = DrawColor.White;
                _ifcTooltip.SetToolTip(btnIfcSelect, "Mode: Automatically scan all intersecting IFC files in rebar bounding area");
            }
            else if (_selectedIfcFiles.Count == 1)
            {
                string singleFile = _selectedIfcFiles.First();
                btnIfcSelect.Text = singleFile + "  ▼";
                btnIfcSelect.ForeColor = DrawColor.FromArgb(134, 239, 172);
                _ifcTooltip.SetToolTip(btnIfcSelect, "Selected 1 IFC file:\n• " + singleFile);
            }
            else
            {
                btnIfcSelect.Text = string.Format("☑ Selected: {0} IFC files  ▼", _selectedIfcFiles.Count);
                btnIfcSelect.ForeColor = DrawColor.FromArgb(134, 239, 172);
                _ifcTooltip.SetToolTip(btnIfcSelect, string.Format("Selected {0} IFC files:\n• {1}", _selectedIfcFiles.Count, string.Join("\n• ", _selectedIfcFiles)));
            }
        }

        /// <summary>
        /// Show modern dropdown with search and checkboxes for selecting one or multiple IFC files.
        /// </summary>
        private void ShowIfcSelectionDropdown()
        {
            var dropDown = new ToolStripDropDown
            {
                AutoClose = true,
                DropShadowEnabled = true,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };

            dropDown.Closing += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    e.Cancel = true;
                }
            };

            var pnlHost = new Panel
            {
                Width = 380,
                Height = 420,
                BackColor = DrawColor.FromArgb(24, 28, 36),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            // 1. Scan Mode (Auto vs Selected)
            var rbAuto = new RadioButton
            {
                Text = "⭐ Auto scan all IFC files (Navisworks Auto)",
                Checked = !_ifcSelectedPartsOnly && _selectedIfcFiles.Count == 0,
                Location = new DrawPoint(10, 10),
                AutoSize = true,
                ForeColor = DrawColor.White,
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            var rbSelectedParts = new RadioButton
            {
                Text = "🎯 Selected IFC objects / Parts in Tekla only",
                Checked = _ifcSelectedPartsOnly,
                Location = new DrawPoint(10, 32),
                AutoSize = true,
                ForeColor = DrawColor.FromArgb(147, 197, 253),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            var rbCustom = new RadioButton
            {
                Text = "📁 Custom select specific IFC files below:",
                Checked = !_ifcSelectedPartsOnly && _selectedIfcFiles.Count > 0,
                Location = new DrawPoint(10, 54),
                AutoSize = true,
                ForeColor = DrawColor.FromArgb(134, 239, 172),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // 2. Search box
            var txtSearch = new TextBox
            {
                Location = new DrawPoint(10, 80),
                Width = 358,
                Height = 24,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new DrawFont("Segoe UI", 8.5F)
            };

            // 3. Action buttons
            var btnSelectAll = new Button
            {
                Text = "☑ Select All",
                Location = new DrawPoint(10, 108),
                Size = new DrawSize(90, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = DrawColor.FromArgb(37, 99, 235),
                ForeColor = DrawColor.White,
                Font = new DrawFont("Segoe UI", 7.5F),
                Cursor = Cursors.Hand
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;

            var btnClearAll = new Button
            {
                Text = "☐ Deselect All",
                Location = new DrawPoint(105, 108),
                Size = new DrawSize(90, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = DrawColor.FromArgb(51, 65, 85),
                ForeColor = DrawColor.FromArgb(203, 213, 225),
                Font = new DrawFont("Segoe UI", 7.5F),
                Cursor = Cursors.Hand
            };
            btnClearAll.FlatAppearance.BorderSize = 0;

            var lblSelectedCount = new Label
            {
                Text = string.Format("Selected: {0} files", _selectedIfcFiles.Count),
                Location = new DrawPoint(205, 112),
                AutoSize = true,
                ForeColor = DrawColor.FromArgb(148, 163, 184),
                Font = new DrawFont("Segoe UI", 8F)
            };

            // 4. CheckedListBox
            var clbFiles = new CheckedListBox
            {
                Location = new DrawPoint(10, 136),
                Width = 358,
                Height = 235,
                CheckOnClick = true,
                BackColor = DrawColor.FromArgb(18, 22, 29),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new DrawFont("Segoe UI", 8.5F)
            };

            Action refreshList = () =>
            {
                clbFiles.BeginUpdate();
                clbFiles.Items.Clear();
                string filter = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
                foreach (var fn in _availableIfcFiles)
                {
                    if (string.IsNullOrEmpty(filter) || fn.ToLowerInvariant().Contains(filter))
                    {
                        bool isChecked = _selectedIfcFiles.Contains(fn);
                        clbFiles.Items.Add(fn, isChecked);
                    }
                }
                clbFiles.EndUpdate();
                lblSelectedCount.Text = string.Format("Selected: {0} / {1} files", _selectedIfcFiles.Count, _availableIfcFiles.Count);
            };

            refreshList();

            txtSearch.TextChanged += (s, e) => refreshList();

            clbFiles.ItemCheck += (s, e) =>
            {
                string fn = clbFiles.Items[e.Index].ToString();
                if (e.NewValue == CheckState.Checked)
                {
                    _selectedIfcFiles.Add(fn);
                    rbCustom.Checked = true;
                    _ifcSelectedPartsOnly = false;
                }
                else
                {
                    _selectedIfcFiles.Remove(fn);
                }
                lblSelectedCount.Text = string.Format("Selected: {0} / {1} files", _selectedIfcFiles.Count, _availableIfcFiles.Count);
                UpdateIfcButtonDisplay();
            };

            btnSelectAll.Click += (s, e) =>
            {
                foreach (var fn in _availableIfcFiles) _selectedIfcFiles.Add(fn);
                rbCustom.Checked = true;
                _ifcSelectedPartsOnly = false;
                refreshList();
                UpdateIfcButtonDisplay();
            };

            btnClearAll.Click += (s, e) =>
            {
                _selectedIfcFiles.Clear();
                refreshList();
                UpdateIfcButtonDisplay();
            };

            rbAuto.CheckedChanged += (s, e) =>
            {
                if (rbAuto.Checked)
                {
                    _ifcSelectedPartsOnly = false;
                    _selectedIfcFiles.Clear();
                    refreshList();
                    UpdateIfcButtonDisplay();
                }
            };

            rbSelectedParts.CheckedChanged += (s, e) =>
            {
                if (rbSelectedParts.Checked)
                {
                    _ifcSelectedPartsOnly = true;
                    _selectedIfcFiles.Clear();
                    refreshList();
                    UpdateIfcButtonDisplay();
                }
            };

            // 5. Apply Button
            var btnApply = new Button
            {
                Text = "✓ Apply",
                Location = new DrawPoint(10, 380),
                Width = 358,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = DrawColor.FromArgb(16, 185, 129),
                ForeColor = DrawColor.White,
                Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += (s, e) =>
            {
                UpdateIfcButtonDisplay();
                dropDown.Close();
            };

            pnlHost.Controls.Add(rbAuto);
            pnlHost.Controls.Add(rbSelectedParts);
            pnlHost.Controls.Add(rbCustom);
            pnlHost.Controls.Add(txtSearch);
            pnlHost.Controls.Add(btnSelectAll);
            pnlHost.Controls.Add(btnClearAll);
            pnlHost.Controls.Add(lblSelectedCount);
            pnlHost.Controls.Add(clbFiles);
            pnlHost.Controls.Add(btnApply);

            var host = new ToolStripControlHost(pnlHost)
            {
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                AutoSize = false,
                Size = pnlHost.Size
            };

            dropDown.Items.Add(host);
            dropDown.Show(btnIfcSelect, new DrawPoint(0, btnIfcSelect.Height + 2));
        }

        /// <summary>
        /// Nạp danh sách các file IFC tham chiếu từ mô hình Tekla vào danh sách lựa chọn.
        /// </summary>
        private void PopulateIfcComboBox()
        {
            _availableIfcFiles.Clear();
            if (_detector == null) return;
            var refModels = _detector.GetReferenceModels();
            foreach (var r in refModels)
            {
                string fn = Path.GetFileName(r.Filename ?? string.Empty);
                if (!string.IsNullOrEmpty(fn) && !_availableIfcFiles.Contains(fn))
                {
                    _availableIfcFiles.Add(fn);
                }
            }
            UpdateIfcButtonDisplay();
        }

        /// <summary>
        /// Bắt đầu quy trình kiểm tra va chạm bất đồng bộ (Asynchronous Clash Check):
        /// 1. Thu thập danh sách cốt thép mục tiêu theo tùy chọn (đang chọn hay toàn bộ).
        /// 2. Thu thập các cấu kiện IFC cản trở trong vùng không gian (Broad-phase).
        /// 3. Tính toán va chạm song song đa luồng tận dụng toàn bộ lõi CPU (Narrow-phase).
        /// 4. Đổ dữ liệu kết quả trực quan lên bảng DataGridView.
        /// </summary>
        private async SysTask StartClashCheckAsync()
        {
            if (_model == null || !_model.GetConnectionStatus())
            {
                MessageBox.Show(this, "Cannot connect to Tekla Structures!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cts = new CancellationTokenSource();
            btnScan.Enabled = false;
            btnStop.Enabled = true;
            progressBar.Visible = true;
            progressBar.Value = 0;
            lblStatusText.Text = "Preparing data...";
            dgvClashes.Rows.Clear();
            _currentClashes.Clear();
            ClearHighlights();

            IfcScopeMode ifcMode = IfcScopeMode.AutoSpatialAllIfc;
            if (_ifcSelectedPartsOnly)
            {
                ifcMode = IfcScopeMode.SelectedIfcOnly;
            }
            else if (_selectedIfcFiles.Count > 0)
            {
                ifcMode = IfcScopeMode.SpecificFile;
            }

            // Save current settings to Properties.Settings
            SaveSettings();

            var settings = new ClashSettings
            {
                OnlySelectedRebars = rbRebarSelected.Checked,
                IfcMode = ifcMode,
                TargetIfcFileName = _selectedIfcFiles.Count == 1 ? _selectedIfcFiles.First() : (_selectedIfcFiles.Count > 1 ? string.Join(";", _selectedIfcFiles) : "ALL"),
                TargetIfcFileNames = new HashSet<string>(_selectedIfcFiles, StringComparer.OrdinalIgnoreCase),
                ToleranceMm = (double)numTolerance.Value,
                ClearanceMm = (double)numClearance.Value,
                EnableIgnoredComponents = chkIgnoreFilter.Checked,
                IgnoredKeywords = ParseKeywords(txtIgnoreKeywords.Text),
                EnableOnlyComponents = chkOnlyFilter.Checked,
                OnlyKeywords = ParseKeywords(txtOnlyKeywords.Text)
            };

            // 1. Collect user selected objects on Tekla UI
            var selectedObjectsList = new List<ModelObject>();
            if (settings.OnlySelectedRebars)
            {
                var selector = new TeklaUiSelector();
                var selectedObjects = selector.GetSelectedObjects();
                while (selectedObjects.MoveNext())
                {
                    if (selectedObjects.Current is ModelObject mo)
                        selectedObjectsList.Add(mo);
                }

                if (selectedObjectsList.Count == 0)
                {
                    MessageBox.Show(this, "No rebars or host parts selected in Tekla model!\n\nTip: You can select Rebars or Concrete Beams/Columns directly in the 3D view to check clashes.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetUiState();
                    return;
                }
            }

            lblStatusText.Text = "Initializing background clash detection task...";
            var sw = System.Diagnostics.Stopwatch.StartNew();
            List<ClashResultItem> results = null;

            Action<string> safeUpdateStatus = (status) =>
            {
                try
                {
                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() => { lblStatusText.Text = status; }));
                    }
                }
                catch { }
            };

            try
            {
                results = await SysTask.Run(() =>
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    // 1. Analyze rebars in background thread
                    safeUpdateStatus("Analyzing rebar geometry and computing 3D bounding range...");

                    var targetRebars = new List<Reinforcement>();
                    var selectedObstacles = new List<ModelObject>();
                    var zoneMin = new TeklaPoint(double.MaxValue, double.MaxValue, double.MaxValue);
                    var zoneMax = new TeklaPoint(double.MinValue, double.MinValue, double.MinValue);
                    var addedRebarIds = new HashSet<long>();

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
                        foreach (var obj in selectedObjectsList)
                        {
                            _cts.Token.ThrowIfCancellationRequested();
                            if (obj is Reinforcement rebar)
                            {
                                addRebar(rebar);
                            }
                            else if (obj is Part part)
                            {
                                try
                                {
                                    var partRebars = part.GetReinforcements();
                                    bool hasPartRebar = false;
                                    while (partRebars.MoveNext())
                                    {
                                        if (partRebars.Current is Reinforcement pr)
                                        {
                                            addRebar(pr);
                                            hasPartRebar = true;
                                        }
                                    }

                                    // Chỉ coi Part không có thép là vật cản KHI VÀ CHỈ KHI người dùng đang bật chế độ "Selected IFC objects / Parts in Tekla only"
                                    if (!hasPartRebar && settings.IfcMode == IfcScopeMode.SelectedIfcOnly)
                                    {
                                        selectedObstacles.Add(part);
                                    }
                                }
                                catch { }
                            }
                            else if (obj is Assembly assy)
                            {
                                try
                                {
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
                                if (settings.IfcMode == IfcScopeMode.SelectedIfcOnly)
                                {
                                    selectedObstacles.Add(obj);
                                }
                            }
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
                            _cts.Token.ThrowIfCancellationRequested();
                            var rebarEnum = _model.GetModelObjectSelector().GetAllObjectsWithType(t);
                            while (rebarEnum.MoveNext())
                            {
                                if (rebarEnum.Current is Reinforcement rebar)
                                {
                                    addRebar(rebar);
                                }
                            }
                        }
                    }

                    if (targetRebars.Count == 0)
                    {
                        safeUpdateStatus("No rebars found to check.");
                        return new List<ClashResultItem>();
                    }

                    safeUpdateStatus(string.Format("Analyzed {0} rebars. Collecting nearby IFC objects...", targetRebars.Count));

                    // 2. Collect IFC obstacle objects in spatial range (Broad-phase)
                    var obstacles = _detector.CollectObstacles(zoneMin, zoneMax, settings, selectedObstacles, safeUpdateStatus);

                    if (obstacles.Count == 0)
                    {
                        safeUpdateStatus("No IFC objects found in the scan range (or filtered out by skip rules).");
                        return new List<ClashResultItem>();
                    }

                    safeUpdateStatus(string.Format("Found {0} IFC objects in rebar area. Computing multithreaded clash geometry...", obstacles.Count));

                    // 3. Compute clashes in parallel with UI throttling (80ms)
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
                                if (!this.IsDisposed && this.IsHandleCreated)
                                {
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        if (progressBar.Visible) progressBar.Value = pct;
                                        lblStatusText.Text = string.Format("Checking: {0}/{1} rebars ({2}%)", curr, max, pct);
                                    }));
                                }
                            }
                            catch { }
                        }
                    }, _cts.Token);
                }, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                lblStatusText.Text = "Clash check was cancelled by user.";
                ResetUiState();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error during clash check: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUiState();
                return;
            }

            sw.Stop();
            _currentClashes = results ?? new List<ClashResultItem>();

            // Filter pass: Remove components matching IgnoredKeywords
            if (settings.EnableIgnoredComponents && settings.IgnoredKeywords != null && settings.IgnoredKeywords.Count > 0)
            {
                var filtered = new List<ClashResultItem>();
                foreach (var c in _currentClashes)
                {
                    string searchable = c.IfcEntityName ?? string.Empty;
                    if (c.IfcObject != null)
                    {
                        try
                        {
                            string teklaName = string.Empty;
                            c.IfcObject.GetReportProperty("NAME", ref teklaName);
                            string teklaDesc = string.Empty;
                            c.IfcObject.GetReportProperty("DESCRIPTION", ref teklaDesc);
                            searchable = $"{searchable} {teklaName} {teklaDesc}".Trim();
                        }
                        catch { }
                    }
                    if (ClashDetector.IsIgnoredComponent(searchable, settings.IgnoredKeywords))
                        continue;
                    filtered.Add(c);
                }
                _currentClashes = filtered;
            }

            // Filter pass: Only keep components matching OnlyKeywords
            if (settings.EnableOnlyComponents && settings.OnlyKeywords != null && settings.OnlyKeywords.Count > 0)
            {
                var filtered = new List<ClashResultItem>();
                foreach (var c in _currentClashes)
                {
                    string searchable = (c.IfcEntityName + " " + c.IfcFileName).Trim();
                    if (!ClashDetector.MatchesOnlyComponent(searchable, settings.OnlyKeywords))
                        continue;
                    filtered.Add(c);
                }
                _currentClashes = filtered;
            }

            for (int i = 0; i < _currentClashes.Count; i++)
            {
                _currentClashes[i].Index = i + 1;
            }

            // Populate DataGridView
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

            UpdateClashCountStatus();
            lblStatusText.Text = string.Format("Scan completed! Detected {0} clashes in {1:F1} seconds.", _currentClashes.Count, sw.Elapsed.TotalSeconds);
            ResetUiState();
        }

        /// <summary>
        /// Restore UI button states after scan completes or stops.
        /// </summary>
        private void ResetUiState()
        {
            btnScan.Enabled = true;
            btnStop.Enabled = false;
            progressBar.Visible = false;
        }

        /// <summary>
        /// Initialize Context Menu (ContextMenuStrip) and keyboard shortcuts for the clash grid.
        /// Allows users to hide reviewed rows, unhide all, zoom to clash, or copy clash details.
        /// </summary>
        private void SetupClashesContextMenu()
        {
            _clashContextMenu = new ContextMenuStrip
            {
                BackColor = DrawColor.FromArgb(30, 35, 45),
                ForeColor = DrawColor.FromArgb(226, 232, 240),
                ShowImageMargin = false
            };

            _menuItemHideRow = new ToolStripMenuItem("👁️ Hide this row (Reviewed)       [Key H / Delete]")
            {
                ForeColor = DrawColor.FromArgb(253, 224, 71), // Highlight Yellow
                Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold)
            };
            _menuItemHideRow.Click += (s, e) => HideSelectedClashRows();

            _menuItemZoom = new ToolStripMenuItem("🔍 Zoom & Select in Tekla")
            {
                ForeColor = DrawColor.FromArgb(147, 197, 253)
            };
            _menuItemZoom.Click += (s, e) => ZoomToSelectedClash();

            _menuItemCopy = new ToolStripMenuItem("📋 Copy Clash Details (Clipboard)")
            {
                ForeColor = DrawColor.FromArgb(203, 213, 225)
            };
            _menuItemCopy.Click += (s, e) => CopySelectedClashInfo();

            _menuItemUnhideAll = new ToolStripMenuItem("🔄 Unhide All Rows")
            {
                ForeColor = DrawColor.FromArgb(134, 239, 172)
            };
            _menuItemUnhideAll.Click += (s, e) => UnhideAllClashRows();

            _clashContextMenu.Items.Add(_menuItemHideRow);
            _clashContextMenu.Items.Add(_menuItemZoom);
            _clashContextMenu.Items.Add(new ToolStripSeparator());
            _clashContextMenu.Items.Add(_menuItemUnhideAll);
            _clashContextMenu.Items.Add(new ToolStripSeparator());
            _clashContextMenu.Items.Add(_menuItemCopy);

            dgvClashes.ContextMenuStrip = _clashContextMenu;

            // Handle CellMouseDown: Right-clicking any cell selects that row immediately
            dgvClashes.CellMouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    if (!dgvClashes.Rows[e.RowIndex].Selected)
                    {
                        dgvClashes.ClearSelection();
                        dgvClashes.Rows[e.RowIndex].Selected = true;
                    }
                    try
                    {
                        dgvClashes.CurrentCell = dgvClashes.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
                    }
                    catch { }

                    // Update hidden rows count on the menu item
                    int hiddenCount = GetHiddenRowCount();
                    _menuItemUnhideAll.Enabled = hiddenCount > 0;
                    _menuItemUnhideAll.Text = hiddenCount > 0
                        ? string.Format("🔄 Unhide All Rows ({0} rows)", hiddenCount)
                        : "🔄 Unhide All Rows";
                }
            };

            // Support keyboard shortcuts H or Delete to quickly hide rows while reviewing
            dgvClashes.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.H)
                {
                    HideSelectedClashRows();
                    e.Handled = true;
                }
            };
        }

        /// <summary>
        /// Count number of clash rows currently hidden in the grid.
        /// </summary>
        private int GetHiddenRowCount()
        {
            int count = 0;
            foreach (DataGridViewRow r in dgvClashes.Rows)
            {
                if (!r.Visible) count++;
            }
            return count;
        }

        /// <summary>
        /// Update clash count status on the status strip (visible vs total and hidden).
        /// </summary>
        private void UpdateClashCountStatus()
        {
            int total = dgvClashes.Rows.Count;
            int hidden = GetHiddenRowCount();
            int visible = total - hidden;

            if (hidden > 0)
            {
                lblCountText.Text = string.Format("{0} remaining / {1} total ({2} hidden)", visible, total, hidden);
            }
            else
            {
                lblCountText.Text = string.Format("{0} clashes", total);
            }
        }

        /// <summary>
        /// Hide currently selected clash rows (marked as reviewed).
        /// Automatically moves selection to the next visible row for smooth review workflow.
        /// </summary>
        private void HideSelectedClashRows()
        {
            if (dgvClashes.SelectedRows.Count == 0) return;

            int lastSelectedIndex = -1;
            var rowsToHide = new List<DataGridViewRow>();
            foreach (DataGridViewRow r in dgvClashes.SelectedRows)
            {
                rowsToHide.Add(r);
                if (r.Index > lastSelectedIndex) lastSelectedIndex = r.Index;
            }

            dgvClashes.CurrentCell = null; // Avoid InvalidOperationException when hiding current cell

            foreach (var r in rowsToHide)
            {
                r.Visible = false;
            }

            // Find and select next visible row
            if (lastSelectedIndex >= 0)
            {
                for (int i = lastSelectedIndex + 1; i < dgvClashes.Rows.Count; i++)
                {
                    if (dgvClashes.Rows[i].Visible)
                    {
                        dgvClashes.Rows[i].Selected = true;
                        try { dgvClashes.CurrentCell = dgvClashes.Rows[i].Cells[0]; } catch { }
                        break;
                    }
                }
            }

            UpdateClashCountStatus();
        }

        /// <summary>
        /// Unhide all clash rows that were previously hidden.
        /// </summary>
        private void UnhideAllClashRows()
        {
            dgvClashes.SuspendLayout();
            foreach (DataGridViewRow r in dgvClashes.Rows)
            {
                r.Visible = true;
            }
            dgvClashes.ResumeLayout();
            UpdateClashCountStatus();
            lblStatusText.Text = "All clash rows unhidden.";
        }

        /// <summary>
        /// Copy detailed information of selected clash rows to Clipboard.
        /// </summary>
        private void CopySelectedClashInfo()
        {
            if (dgvClashes.SelectedRows.Count == 0) return;
            var sb = new StringBuilder();
            foreach (DataGridViewRow r in dgvClashes.SelectedRows)
            {
                var item = r.Tag as ClashResultItem;
                if (item != null)
                {
                    sb.AppendLine(string.Format("#{0}\tRebar: {1} (ID:{2}, {3})\tEntity: {4}\tOverlap: {5} mm\tSeverity: {6}\tCoord: {7}",
                        item.Index, item.RebarName, item.RebarId, item.RebarSize, item.IfcEntityName, item.OverlapMm, item.Severity, item.ClashPointDisplay));
                }
            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
                lblStatusText.Text = "Clash details copied to Clipboard!";
            }
        }

        /// <summary>
        /// Format cell colors in DataGridView based on clash severity.
        /// </summary>
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
                        fg = DrawColor.FromArgb(239, 68, 68); // Red: severe clash
                        break;
                    case ClashSeverity.Medium:
                        fg = DrawColor.FromArgb(249, 115, 22); // Orange: medium clash
                        break;
                    default:
                        fg = DrawColor.FromArgb(234, 179, 8); // Yellow: minor clash / clearance
                        break;
                }
                e.CellStyle.ForeColor = fg;
                e.CellStyle.Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        /// <summary>
        /// Automatically zoom and focus Tekla Structures 3D view to the selected clash position
        /// and select the clashing rebar object.
        /// Temporarily switches WorkPlane to Global for accurate AABB calculation, then restores original WorkPlane.
        /// </summary>
        private void ZoomToSelectedClash()
        {
            if (dgvClashes.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Please select a clash row from the table!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Clear previous highlight boxes before zooming and drawing new box
            ClearHighlights();

            TransformationPlane originalPlane = null;
            WorkPlaneHandler wph = null;

            try
            {
                var model = _model ?? new Model();
                wph = model.GetWorkPlaneHandler();
                if (wph != null)
                {
                    // 1. Save current WorkPlane (Local)
                    originalPlane = wph.GetCurrentTransformationPlane();
                    // 2. Temporarily switch to Global WorkPlane for accurate AABB
                    wph.SetCurrentTransformationPlane(new TransformationPlane());
                }

                var objsToSelect = new ArrayList();
                TeklaPoint minPt = new TeklaPoint(double.MaxValue, double.MaxValue, double.MaxValue);
                TeklaPoint maxPt = new TeklaPoint(double.MinValue, double.MinValue, double.MinValue);

                foreach (DataGridViewRow row in dgvClashes.SelectedRows)
                {
                    var c = row.Tag as ClashResultItem;
                    if (c == null) continue;

                    // Select only the clashing rebar
                    if (c.RebarObject != null)
                    {
                        objsToSelect.Add(c.RebarObject);

                        // Obtain exact AABB of rebar in Global WorkPlane
                        try
                        {
                            var solid = c.RebarObject.GetSolid();
                            if (solid != null && solid.MinimumPoint != null && solid.MaximumPoint != null)
                            {
                                minPt.X = Math.Min(minPt.X, solid.MinimumPoint.X);
                                minPt.Y = Math.Min(minPt.Y, solid.MinimumPoint.Y);
                                minPt.Z = Math.Min(minPt.Z, solid.MinimumPoint.Z);

                                maxPt.X = Math.Max(maxPt.X, solid.MaximumPoint.X);
                                maxPt.Y = Math.Max(maxPt.Y, solid.MaximumPoint.Y);
                                maxPt.Z = Math.Max(maxPt.Z, solid.MaximumPoint.Z);
                            }
                        }
                        catch { }
                    }

                    if (c.ClashPoint != null)
                    {
                        // Zoom close to ClashPoint bounding range (radius 150mm)
                        minPt.X = Math.Min(minPt.X, c.ClashPoint.X - 150.0);
                        minPt.Y = Math.Min(minPt.Y, c.ClashPoint.Y - 150.0);
                        minPt.Z = Math.Min(minPt.Z, c.ClashPoint.Z - 150.0);

                        maxPt.X = Math.Max(maxPt.X, c.ClashPoint.X + 150.0);
                        maxPt.Y = Math.Max(maxPt.Y, c.ClashPoint.Y + 150.0);
                        maxPt.Z = Math.Max(maxPt.Z, c.ClashPoint.Z + 150.0);
                    }
                    else if (c.MinPoint != null && c.MaxPoint != null)
                    {
                        minPt.X = Math.Min(minPt.X, c.MinPoint.X);
                        minPt.Y = Math.Min(minPt.Y, c.MinPoint.Y);
                        minPt.Z = Math.Min(minPt.Z, c.MinPoint.Z);

                        maxPt.X = Math.Max(maxPt.X, c.MaxPoint.X);
                        maxPt.Y = Math.Max(maxPt.Y, c.MaxPoint.Y);
                        maxPt.Z = Math.Max(maxPt.Z, c.MaxPoint.Z);
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

                    // Draw yellow highlighted cube box at selected clash position (while in Global)
                    var selectedItem = dgvClashes.SelectedRows[0].Tag as ClashResultItem;
                    if (selectedItem != null && selectedItem.ClashPoint != null)
                    {
                        var drawer = new GraphicsDrawer();
                        var yellowWire = new TeklaColor(1.0, 0.8, 0.0);
                        var yellowFill = new TeklaColor(1.0, 0.8, 0.0, 0.4);
                        var yellowLabel = new TeklaColor(1.0, 1.0, 0.2);
                        DrawClashBox(drawer, selectedItem, 30.0, yellowWire, yellowFill, yellowLabel, true);
                    }

                    lblStatusText.Text = string.Format("Zoomed & selected clash #{0} in Tekla model!", ((ClashResultItem)dgvClashes.SelectedRows[0].Tag).Index);
                }
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Error zooming in Tekla: " + ex.Message;
            }
            finally
            {
                // 3. Restore original Local WorkPlane
                if (wph != null && originalPlane != null)
                {
                    try
                    {
                        wph.SetCurrentTransformationPlane(originalPlane);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Draw complete 3D cube box (12 wireframe edges + translucent faces + label)
        /// at ClashPoint coordinate of a clash result.
        /// </summary>
        private void DrawClashBox(
            GraphicsDrawer drawer,
            ClashResultItem c,
            double h,
            TeklaColor wireColor,
            TeklaColor fillColor,
            TeklaColor labelColor,
            bool trackHighlight = true)
        {
            if (drawer == null || c == null || c.ClashPoint == null) return;
            TeklaPoint p = c.ClashPoint;

            // 1. Determine 8 vertices of cube box around clash point
            var p0 = new TeklaPoint(p.X - h, p.Y - h, p.Z - h);
            var p1 = new TeklaPoint(p.X + h, p.Y - h, p.Z - h);
            var p2 = new TeklaPoint(p.X + h, p.Y + h, p.Z - h);
            var p3 = new TeklaPoint(p.X - h, p.Y + h, p.Z - h);

            var p4 = new TeklaPoint(p.X - h, p.Y - h, p.Z + h);
            var p5 = new TeklaPoint(p.X + h, p.Y - h, p.Z + h);
            var p6 = new TeklaPoint(p.X + h, p.Y + h, p.Z + h);
            var p7 = new TeklaPoint(p.X - h, p.Y + h, p.Z + h);

            // 2. Draw 12 cube edges with GraphicPolyLine (Width = 3)
            var bottomLoop = new ArrayList { p0, p1, p2, p3, p0 };
            var topLoop = new ArrayList { p4, p5, p6, p7, p4 };
            var side0 = new ArrayList { p0, p4 };
            var side1 = new ArrayList { p1, p5 };
            var side2 = new ArrayList { p2, p6 };
            var side3 = new ArrayList { p3, p7 };
            var diag1 = new ArrayList { p4, p6 }; // Top face diagonal for visibility from top view
            var diag2 = new ArrayList { p5, p7 };

            var wireSegments = new[] { bottomLoop, topLoop, side0, side1, side2, side3, diag1, diag2 };
            foreach (var pts in wireSegments)
            {
                var gLine = new GraphicPolyLine
                {
                    Color = wireColor,
                    Width = 3,
                    PolyLine = new PolyLine(pts)
                };
                int id = drawer.DrawPolyLine(gLine);
                if (trackHighlight && id > 0)
                {
                    lock (_highlightLock)
                    {
                        _activeHighlights.Add(id);
                    }
                }
            }
        }

        /// <summary>
        /// Highlight all clash points in Tekla 3D model space with red 3D cube boxes using GraphicsDrawer.
        /// Temporarily switches WorkPlane to Global for accurate 3D drawing, then restores original WorkPlane.
        /// </summary>
        private void HighlightClashesInTekla()
        {
            if (_currentClashes.Count == 0)
            {
                MessageBox.Show(this, "No clashes to highlight!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ClearHighlights();

            TransformationPlane originalPlane = null;
            WorkPlaneHandler wph = null;

            try
            {
                var model = _model ?? new Model();
                wph = model.GetWorkPlaneHandler();
                if (wph != null)
                {
                    // 1. Save current WorkPlane (Local)
                    originalPlane = wph.GetCurrentTransformationPlane();
                    // 2. Temporarily switch to Global WorkPlane for accurate drawing
                    wph.SetCurrentTransformationPlane(new TransformationPlane());
                }

                var redWireColor = new TeklaColor(1.0, 0.0, 0.0);       // Red wireframe
                var redFillColor = new TeklaColor(1.0, 0.1, 0.1, 0.35); // Translucent red
                var labelColor = new TeklaColor(1.0, 0.9, 0.2);         // Highlight yellow
                double h = 30.0; // Half edge length (creates 60x60x60 mm cube box)
                var drawer = new GraphicsDrawer();

                foreach (var c in _currentClashes)
                {
                    DrawClashBox(drawer, c, h, redWireColor, redFillColor, labelColor, true);
                }

                lblStatusText.Text = string.Format("Highlighted {0} clash points with 3D red boxes!", _currentClashes.Count);
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Error drawing 3D highlights: " + ex.Message;
            }
            finally
            {
                // 3. Restore original Local WorkPlane
                if (wph != null && originalPlane != null)
                {
                    try
                    {
                        wph.SetCurrentTransformationPlane(originalPlane);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Clear all temporary 3D graphics markings in Tekla Structures using GraphicsDrawer.
        /// If clearSelection is true (user explicitly clicked "Clear 3D"), also deselects objects and redraws views.
        /// </summary>
        private void ClearHighlights(bool clearSelection = false)
        {
            try
            {
                // 1. Snapshot and remove all tracked temporary 3D graphic IDs
                ArrayList idsToRemove = null;
                lock (_highlightLock)
                {
                    if (_activeHighlights.Count > 0)
                    {
                        idsToRemove = new ArrayList(_activeHighlights);
                        _activeHighlights.Clear();
                    }
                }

                if (idsToRemove != null && idsToRemove.Count > 0)
                {
                    try
                    {
                        var drawer = new GraphicsDrawer();
                        // Batch removal with ArrayList (compatible with Tekla OpenAPI COM/C++ layer)
                        try
                        {
                            drawer.RemoveTemporaryGraphicsObjects(idsToRemove);
                        }
                        catch { }

                        // Fallback individual removal for safety
                        foreach (var idObj in idsToRemove)
                        {
                            try
                            {
                                if (idObj is int id)
                                {
                                    drawer.RemoveTemporaryGraphicsObject(id);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                // 2. Clear selected objects in Tekla UI ONLY if user clicked Clear 3D button!
                // NEVER deselect during StartClashCheckAsync because user explicitly selected rebars to check!
                if (clearSelection)
                {
                    try
                    {
                        var selector = new TeklaUiSelector();
                        selector.Select(new ArrayList());
                    }
                    catch { }
                }

                // 3. Force Tekla Structures to redraw all visible model views
                // This is MANDATORY when removing temporary graphics or clearing selection to update display buffer
                if (clearSelection || (idsToRemove != null && idsToRemove.Count > 0))
                {
                    try
                    {
                        var viewEnum = ViewHandler.GetVisibleViews();
                        if (viewEnum != null)
                        {
                            while (viewEnum.MoveNext())
                            {
                                if (viewEnum.Current != null)
                                {
                                    ViewHandler.RedrawView(viewEnum.Current);
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        ViewHandler.RedrawWorkplane();
                    }
                    catch { }
                }

                if (clearSelection && lblStatusText != null)
                {
                    lblStatusText.Text = "Cleared 3D highlights and model selections.";
                }
            }
            catch (Exception ex)
            {
                if (lblStatusText != null)
                {
                    lblStatusText.Text = "Clear 3D: " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Export clash detection results to Excel/CSV file with UTF-8 encoding.
        /// </summary>
        private void ExportToCsv()
        {
            if (_currentClashes.Count == 0)
            {
                MessageBox.Show(this, "No data to export!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        sb.AppendLine("No,Rebar_ID,Rebar_Name,Size,Grade,Pos_Mark,Host_Part,IFC_Entity,Length_mm,Overlap_mm,Severity,Coord_X,Coord_Y,Coord_Z");

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
                        MessageBox.Show(this, "Report successfully exported to:\n" + sfd.FileName, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Nạp toàn bộ cài đặt từ Properties.Settings khi mở Form.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var s = Properties.Settings.Default;

                // 1. Phạm vi kiểm tra cốt thép
                if (s.OnlySelectedRebars)
                {
                    rbRebarSelected.Checked = true;
                    rbRebarAll.Checked = false;
                }
                else
                {
                    rbRebarSelected.Checked = false;
                    rbRebarAll.Checked = true;
                }

                // 2. Dung sai & Khoảng hở
                if (s.Tolerance >= numTolerance.Minimum && s.Tolerance <= numTolerance.Maximum)
                {
                    numTolerance.Value = s.Tolerance;
                }
                if (s.Clearance >= numClearance.Minimum && s.Clearance <= numClearance.Maximum)
                {
                    numClearance.Value = s.Clearance;
                }

                // 3. Bộ lọc SkipNames
                chkIgnoreFilter.Checked = s.EnableSkipNames;
                txtIgnoreKeywords.Enabled = s.EnableSkipNames;
                if (!string.IsNullOrEmpty(s.SkipNames))
                {
                    txtIgnoreKeywords.Text = s.SkipNames;
                }

                // 4. Bộ lọc OnlyNames
                chkOnlyFilter.Checked = s.EnableOnlyNames;
                txtOnlyKeywords.Enabled = s.EnableOnlyNames;
                if (!string.IsNullOrEmpty(s.OnlyNames))
                {
                    txtOnlyKeywords.Text = s.OnlyNames;
                }

                // 5. Cấu hình file IFC đã chọn
                _ifcSelectedPartsOnly = s.IfcSelectedPartsOnly;
                _selectedIfcFiles.Clear();
                if (!string.IsNullOrEmpty(s.SelectedIfcFiles))
                {
                    var files = s.SelectedIfcFiles.Split(new char[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var f in files)
                    {
                        string trimmed = f.Trim();
                        if (!string.IsNullOrEmpty(trimmed)) _selectedIfcFiles.Add(trimmed);
                    }
                }
                UpdateIfcButtonDisplay();

                // 6. Kích thước và trạng thái cửa sổ Form
                if (s.WindowWidth >= this.MinimumSize.Width && s.WindowHeight >= this.MinimumSize.Height)
                {
                    this.Size = new DrawSize(s.WindowWidth, s.WindowHeight);
                }
                if (s.WindowState == (int)FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi nạp Properties.Settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Lưu toàn bộ cài đặt của Form vào Properties.Settings khi tắt Form hoặc bắt đầu quét.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                var s = Properties.Settings.Default;

                // 1. Phạm vi cốt thép
                s.OnlySelectedRebars = rbRebarSelected.Checked;

                // 2. Dung sai & Khoảng hở
                s.Tolerance = numTolerance.Value;
                s.Clearance = numClearance.Value;

                // 3. Bộ lọc SkipNames
                s.EnableSkipNames = chkIgnoreFilter.Checked;
                s.SkipNames = txtIgnoreKeywords.Text;

                // 4. Bộ lọc OnlyNames
                s.EnableOnlyNames = chkOnlyFilter.Checked;
                s.OnlyNames = txtOnlyKeywords.Text;

                // 5. Cấu hình file IFC
                s.IfcSelectedPartsOnly = _ifcSelectedPartsOnly;
                s.SelectedIfcFiles = string.Join(";", _selectedIfcFiles);

                // 6. Kích thước và trạng thái Form
                if (this.WindowState == FormWindowState.Normal)
                {
                    s.WindowWidth = this.Width;
                    s.WindowHeight = this.Height;
                    s.WindowState = 0;
                }
                else if (this.WindowState == FormWindowState.Maximized)
                {
                    s.WindowState = (int)FormWindowState.Maximized;
                }

                // Ghi vĩnh viễn vào user.config thông qua .NET Settings Provider
                s.Save();

                // Lưu kèm ra file text dự phòng để tương thích ngược
                try
                {
                    File.WriteAllText(FilterSettingsFile, txtIgnoreKeywords.Text);
                    File.WriteAllText(OnlyFilterSettingsFile, txtOnlyKeywords.Text);
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lưu Properties.Settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi đóng Form: lưu cài đặt vào Properties.Settings và giải phóng bộ nhớ đệm hình học của IfcGeometryBridge.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { SaveSettings(); } catch { }
            try { ClearHighlights(); } catch { }
            try { IfcGeometryBridge.ClearCache(); } catch { }
            base.OnFormClosing(e);
        }
    }
}

