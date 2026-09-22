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
        private List<GraphicPolyLine> _activeHighlights = new List<GraphicPolyLine>();
        private CancellationTokenSource _cts;

        // Các thành phần điều khiển giao diện (UI Controls)
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

        /// <summary>
        /// Tải danh sách từ khóa bỏ qua đã lưu từ cấu hình hoặc trả về danh sách mặc định.
        /// </summary>
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

        /// <summary>
        /// Tải danh sách từ khóa chỉ quét đã lưu từ cấu hình (OnlyNames).
        /// </summary>
        private static string LoadSavedOnlyKeywords()
        {
            try
            {
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
                        return content;
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
        /// Hàm khởi tạo MainForm, thiết lập giao diện và kết nối Tekla Open API.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
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
                Height = 152,
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
            btnScan = CreateFlatButton("⚡ Quét va chạm", new DrawPoint(15, 60), new DrawSize(140, 78), DrawColor.FromArgb(37, 99, 235), DrawColor.White);
            btnScan.Font = new DrawFont("Segoe UI", 10F, FontStyle.Bold);
            btnScan.Click += async (s, e) => await StartClashCheckAsync();

            btnStop = CreateFlatButton("⏹ Dừng", new DrawPoint(160, 60), new DrawSize(68, 78), DrawColor.FromArgb(75, 85, 99), DrawColor.White);
            btnStop.Enabled = false;
            btnStop.Click += (s, e) => _cts?.Cancel();

            btnZoomSelect = CreateFlatButton("🔍 Zoom & Chọn", new DrawPoint(234, 60), new DrawSize(145, 36), DrawColor.FromArgb(13, 148, 136), DrawColor.White);
            btnZoomSelect.Click += (s, e) => ZoomToSelectedClash();

            btnHighlight = CreateFlatButton("📍 Đánh dấu 3D", new DrawPoint(385, 60), new DrawSize(115, 36), DrawColor.FromArgb(147, 51, 234), DrawColor.White);
            btnHighlight.Click += (s, e) => HighlightClashesInTekla();

            btnClearHighlight = CreateFlatButton("🧹 Xóa 3D", new DrawPoint(506, 60), new DrawSize(80, 36), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
            btnClearHighlight.Click += (s, e) => ClearHighlights();

            btnExportCsv = CreateFlatButton("📊 Xuất báo cáo Excel/CSV", new DrawPoint(234, 102), new DrawSize(352, 36), DrawColor.FromArgb(16, 185, 129), DrawColor.White);
            btnExportCsv.Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold);
            btnExportCsv.Click += (s, e) => ExportToCsv();

            // Row 2 - Cột bộ lọc 1: Lược bỏ cấu kiện IFC (SkipNames)
            chkIgnoreFilter = new CheckBox
            {
                Text = "Lược bỏ (SkipNames):",
                Checked = true,
                ForeColor = DrawColor.FromArgb(147, 197, 253),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new DrawPoint(600, 48),
                AutoSize = true
            };

            btnResetIgnore = CreateFlatButton("↺ Mặc định", new DrawPoint(830, 45), new DrawSize(70, 22), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
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

            // Row 2 - Cột bộ lọc 2: Chỉ quét cấu kiện IFC chỉ định (OnlyNames)
            chkOnlyFilter = new CheckBox
            {
                Text = "Chỉ quét (OnlyNames):",
                Checked = false,
                ForeColor = DrawColor.FromArgb(134, 239, 172),
                Font = new DrawFont("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new DrawPoint(920, 48),
                AutoSize = true
            };

            btnClearOnly = CreateFlatButton("✖ Xóa trắng", new DrawPoint(1150, 45), new DrawSize(70, 22), DrawColor.FromArgb(51, 65, 85), DrawColor.FromArgb(203, 213, 225));
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
            filterTooltip.SetToolTip(chkIgnoreFilter, "Bật/tắt bỏ qua các cấu kiện phụ IFC khi quét va chạm với thép (IfcConvertOptions.AddSkipNames)");
            filterTooltip.SetToolTip(txtIgnoreKeywords, "Nhập danh sách tên/từ khóa cấu kiện IFC cần lược bỏ (mỗi dòng 1 từ khóa hoặc phân tách bằng dấu phẩy).\nVí dụ:\nBolt assembly\nSAFETY_BAR\nLUG\nLADDER\nSAFETY_HOOK\nVBRACE\nWELD_COUPLER(10)\nCHECK_COUPLER(10)");
            filterTooltip.SetToolTip(btnResetIgnore, "Khôi phục danh sách từ khóa bỏ qua mặc định");

            filterTooltip.SetToolTip(chkOnlyFilter, "Bật/tắt bộ lọc CHỈ quét các cấu kiện IFC chỉ định (IfcConvertOptions.AddOnlyNames)");
            filterTooltip.SetToolTip(txtOnlyKeywords, "Nhập danh sách tên/từ khóa cấu kiện IFC chỉ định bắt buộc quét (mỗi dòng 1 từ khóa hoặc phân tách bằng dấu phẩy).\nNếu kích hoạt, chỉ những cấu kiện khớp với từ khóa mới được trích xuất và quét va chạm.\nVí dụ:\nBEAM\nCOLUMN\nSLAB\nWALL\nPIPE");
            filterTooltip.SetToolTip(btnClearOnly, "Xóa trắng danh sách cấu kiện chỉ định để quét tất cả");

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

        /// <summary>
        /// Phương thức trợ giúp tạo nút bấm phẳng với phong cách giao diện Dark Theme hiện đại.
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
        /// Kết nối với mô hình Tekla Structures hiện hành thông qua Tekla Open API.
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
                    lblTeklaStatus.Text = string.Format("● Đã kết nối: {0}", info.ModelName);
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(34, 197, 94); // Màu xanh lá biểu thị đã kết nối thành công

                    // Nạp danh sách các file IFC tham chiếu vào ComboBox
                    PopulateIfcComboBox();
                }
                else
                {
                    lblTeklaStatus.Text = "● Chưa kết nối Tekla";
                    lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68); // Màu đỏ cảnh báo
                }
            }
            catch (Exception ex)
            {
                lblTeklaStatus.Text = "● Lỗi kết nối Tekla";
                lblTeklaStatus.ForeColor = DrawColor.FromArgb(239, 68, 68);
                lblStatusText.Text = "Lỗi khởi tạo Tekla API: " + ex.Message;
            }
        }

        /// <summary>
        /// Nạp các lựa chọn chế độ quét và danh sách các file IFC tham chiếu vào ComboBox.
        /// </summary>
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

            // Lưu cài đặt bộ lọc từ khóa vào file cấu hình
            try
            {
                File.WriteAllText(FilterSettingsFile, txtIgnoreKeywords.Text);
                File.WriteAllText(OnlyFilterSettingsFile, txtOnlyKeywords.Text);
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
                IgnoredKeywords = ParseKeywords(txtIgnoreKeywords.Text),
                EnableOnlyComponents = chkOnlyFilter.Checked,
                OnlyKeywords = ParseKeywords(txtOnlyKeywords.Text)
            };

            // 1. Thu thập danh sách đối tượng người dùng đang chọn trên Tekla UI (thực hiện siêu nhanh trên UI thread)
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
                    MessageBox.Show(this, "Bạn chưa chọn thanh thép hoặc cấu kiện bê tông nào trong mô hình Tekla!\n\nMẹo: Bạn có thể chọn trực tiếp Thanh thép hoặc Dầm/Cột bê tông trên màn hình 3D để kiểm tra va chạm.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetUiState();
                    return;
                }
            }

            lblStatusText.Text = "Đang khởi tạo tác vụ quét va chạm nền...";
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

                    // 1. Phân tích cốt thép trong luồng nền (Tránh tuyệt đối đơ/treo giao diện Tekla & Add-in)
                    safeUpdateStatus("Đang phân tích cấu trúc thép và tính toán phạm vi không gian 3D...");

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
                                selectedObstacles.Add(obj);
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
                        safeUpdateStatus("Không tìm thấy thanh thép nào để quét.");
                        return new List<ClashResultItem>();
                    }

                    safeUpdateStatus(string.Format("Đã phân tích {0} thép. Đang quét cấu kiện IFC lân cận...", targetRebars.Count));

                    // 2. Quét thu thập vật thể cản trở IFC trong không gian (Broad-phase)
                    var obstacles = _detector.CollectObstacles(zoneMin, zoneMax, settings, selectedObstacles, safeUpdateStatus);

                    if (obstacles.Count == 0)
                    {
                        safeUpdateStatus("Không tìm thấy cấu kiện IFC nào trong vùng quét (hoặc đã bị lược bỏ bởi bộ lọc).");
                        return new List<ClashResultItem>();
                    }

                    safeUpdateStatus(string.Format("Tìm thấy {0} cấu kiện IFC trong vùng thép. Đang tính toán va chạm hình học đa luồng...", obstacles.Count));

                    // 3. Tính toán va chạm song song đa luồng với bộ điều tiết cập nhật UI (Throttling 80ms)
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
                                        lblStatusText.Text = string.Format("Đang kiểm tra: {0}/{1} thép ({2}%)", curr, max, pct);
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

            // Lớp lọc an toàn cuối cùng: Loại bỏ cấu kiện theo IgnoredKeywords
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
            }

            // Lớp lọc an toàn cuối cùng: Chỉ giữ cấu kiện theo OnlyKeywords
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

            // Đổ dữ liệu vào bảng DataGridView
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

        /// <summary>
        /// Khôi phục trạng thái sẵn sàng của các nút điều khiển trên giao diện.
        /// </summary>
        private void ResetUiState()
        {
            btnScan.Enabled = true;
            btnStop.Enabled = false;
            progressBar.Visible = false;
        }

        /// <summary>
        /// Định dạng màu sắc các ô trong bảng DataGridView dựa trên mức độ nghiêm trọng của va chạm.
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
                        fg = DrawColor.FromArgb(239, 68, 68); // Màu đỏ: va chạm nghiêm trọng
                        break;
                    case ClashSeverity.Medium:
                        fg = DrawColor.FromArgb(249, 115, 22); // Màu cam: va chạm trung bình
                        break;
                    default:
                        fg = DrawColor.FromArgb(234, 179, 8); // Màu vàng: va chạm nhẹ / khoảng hở
                        break;
                }
                e.CellStyle.ForeColor = fg;
                e.CellStyle.Font = new DrawFont("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        /// <summary>
        /// Tự động thu phóng (Zoom &amp; Focus) màn hình 3D Tekla Structures đến vị trí va chạm đang chọn
        /// và chọn (select) đối tượng thanh thép bị va chạm.
        /// </summary>
        private void ZoomToSelectedClash()
        {
            if (dgvClashes.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn một dòng va chạm trong bảng!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Xóa hình vẽ đánh dấu DrawClashBox cũ trước khi Zoom & vẽ hộp mới
            ClearHighlights();

            try
            {
                var objsToSelect = new ArrayList();
                TeklaPoint minPt = new TeklaPoint(double.MaxValue, double.MaxValue, double.MaxValue);
                TeklaPoint maxPt = new TeklaPoint(double.MinValue, double.MinValue, double.MinValue);

                foreach (DataGridViewRow row in dgvClashes.SelectedRows)
                {
                    var c = row.Tag as ClashResultItem;
                    if (c == null) continue;

                    // Chỉ chọn duy nhất đối tượng thanh thép va chạm
                    if (c.RebarObject != null) objsToSelect.Add(c.RebarObject);

                    if (c.ClashPoint != null)
                    {
                        // Zoom cận cảnh vào phạm vi khối hộp va chạm ClashPoint (bán kính 100mm)
                        minPt.X = Math.Min(minPt.X, c.ClashPoint.X - 100.0);
                        minPt.Y = Math.Min(minPt.Y, c.ClashPoint.Y - 100.0);
                        minPt.Z = Math.Min(minPt.Z, c.ClashPoint.Z - 100.0);

                        maxPt.X = Math.Max(maxPt.X, c.ClashPoint.X + 100.0);
                        maxPt.Y = Math.Max(maxPt.Y, c.ClashPoint.Y + 100.0);
                        maxPt.Z = Math.Max(maxPt.Z, c.ClashPoint.Z + 100.0);
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

                    // Vẽ khối hộp lập phương nổi bật màu vàng cam tại vị trí va chạm đang chọn
                    var selectedItem = dgvClashes.SelectedRows[0].Tag as ClashResultItem;
                    if (selectedItem != null && selectedItem.ClashPoint != null)
                    {
                        var drawer = new GraphicsDrawer();
                        var yellowWire = new TeklaColor(1.0, 0.8, 0.0);
                        var yellowFill = new TeklaColor(1.0, 0.8, 0.0, 0.4);
                        var yellowLabel = new TeklaColor(1.0, 1.0, 0.2);
                        DrawClashBox(drawer, selectedItem, 30.0, yellowWire, yellowFill, yellowLabel, true);
                    }

                    lblStatusText.Text = string.Format("Đã Zoom & Chọn va chạm #{0} trên mô hình Tekla!", ((ClashResultItem)dgvClashes.SelectedRows[0].Tag).Index);
                }
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Lỗi khi Zoom trong Tekla: " + ex.Message;
            }
        }

        /// <summary>
        /// Vẽ một khối hộp lập phương 3D hoàn chỉnh (khung viền 12 cạnh + bề mặt bán trong suốt + nhãn chỉ dẫn)
        /// tại tọa độ ClashPoint của một kết quả va chạm.
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

            // 1. Xác định tọa độ 8 đỉnh của hình hộp lập phương (Cube Box) bao quanh điểm va chạm
            var p0 = new TeklaPoint(p.X - h, p.Y - h, p.Z - h);
            var p1 = new TeklaPoint(p.X + h, p.Y - h, p.Z - h);
            var p2 = new TeklaPoint(p.X + h, p.Y + h, p.Z - h);
            var p3 = new TeklaPoint(p.X - h, p.Y + h, p.Z - h);

            var p4 = new TeklaPoint(p.X - h, p.Y - h, p.Z + h);
            var p5 = new TeklaPoint(p.X + h, p.Y - h, p.Z + h);
            var p6 = new TeklaPoint(p.X + h, p.Y + h, p.Z + h);
            var p7 = new TeklaPoint(p.X - h, p.Y + h, p.Z + h);

            // 2. Vẽ 12 cạnh viền của hình hộp lập phương bằng GraphicPolyLine nét đậm (Width = 3)
            var bottomLoop = new ArrayList { p0, p1, p2, p3, p0 };
            var topLoop = new ArrayList { p4, p5, p6, p7, p4 };
            var side0 = new ArrayList { p0, p4 };
            var side1 = new ArrayList { p1, p5 };
            var side2 = new ArrayList { p2, p6 };
            var side3 = new ArrayList { p3, p7 };
            var diag1 = new ArrayList { p4, p6 }; // Dấu chéo mặt trên để dễ nhận diện từ góc nhìn trên cao
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
                if (trackHighlight) _activeHighlights.Add(gLine);
                drawer.DrawPolyLine(gLine);
            }

            // 3. Vẽ bề mặt 3D của khối lập phương bán trong suốt (Mesh Surfaces)
            try
            {
                var mesh = new Mesh();
                int i0 = mesh.AddPoint(p0);
                int i1 = mesh.AddPoint(p1);
                int i2 = mesh.AddPoint(p2);
                int i3 = mesh.AddPoint(p3);
                int i4 = mesh.AddPoint(p4);
                int i5 = mesh.AddPoint(p5);
                int i6 = mesh.AddPoint(p6);
                int i7 = mesh.AddPoint(p7);

                // 6 mặt lập phương (12 tam giác)
                mesh.AddTriangle(i0, i2, i1); mesh.AddTriangle(i0, i3, i2); // Mặt đáy (Z-)
                mesh.AddTriangle(i4, i5, i6); mesh.AddTriangle(i4, i6, i7); // Mặt đỉnh (Z+)
                mesh.AddTriangle(i0, i1, i5); mesh.AddTriangle(i0, i5, i4); // Mặt trước (Y-)
                mesh.AddTriangle(i3, i6, i2); mesh.AddTriangle(i3, i7, i6); // Mặt sau (Y+)
                mesh.AddTriangle(i0, i4, i7); mesh.AddTriangle(i0, i7, i3); // Mặt trái (X-)
                mesh.AddTriangle(i1, i2, i6); mesh.AddTriangle(i1, i6, i5); // Mặt phải (X+)

                drawer.DrawMeshSurface(mesh, fillColor);
            }
            catch { }

            // 4. Vẽ nhãn văn bản chỉ dẫn số thứ tự và độ lấn va chạm ngay phía trên hình hộp
            try
            {
                var labelPos = new TeklaPoint(p.X, p.Y, p.Z + h + 25.0);
                drawer.DrawText(labelPos, string.Format("#{0} [{1:F1}mm]", c.Index, c.OverlapMm), labelColor);
            }
            catch { }
        }

        /// <summary>
        /// Vẽ hình hộp lập phương 3D màu đỏ nổi bật tại tất cả các điểm va chạm trong không gian mô hình Tekla bằng GraphicsDrawer.
        /// Bao gồm khung viền 12 cạnh nét đậm, bề mặt lập phương 3D bán trong suốt và nhãn văn bản định danh.
        /// </summary>
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
                var redWireColor = new TeklaColor(1.0, 0.0, 0.0);       // Màu đỏ rực cho các cạnh viền khung
                var redFillColor = new TeklaColor(1.0, 0.1, 0.1, 0.35); // Màu đỏ bán trong suốt (35%) cho bề mặt khối
                var labelColor = new TeklaColor(1.0, 0.9, 0.2);         // Màu vàng nổi bật cho nhãn văn bản chỉ dẫn
                double h = 30.0; // Nửa chiều dài cạnh (tạo khối hộp lập phương kích thước 60x60x60 mm nhỏ gọn, vừa vặn)
                var drawer = new GraphicsDrawer();

                foreach (var c in _currentClashes)
                {
                    DrawClashBox(drawer, c, h, redWireColor, redFillColor, labelColor, true);
                }

                lblStatusText.Text = string.Format("Đã đánh dấu {0} điểm va chạm bằng hình hộp lập phương 3D màu đỏ!", _currentClashes.Count);
            }
            catch (Exception ex)
            {
                lblStatusText.Text = "Lỗi khi vẽ đánh dấu 3D: " + ex.Message;
            }
        }

        /// <summary>
        /// Xóa bỏ toàn bộ các đường vẽ đánh dấu 3D trên màn hình Tekla Structures.
        /// </summary>
        private void ClearHighlights()
        {
            try
            {
                _activeHighlights.Clear();
                var viewEnum = ViewHandler.GetVisibleViews();
                while (viewEnum.MoveNext())
                {
                    ViewHandler.RedrawView(viewEnum.Current);
                }
            }
            catch { }
        }

        /// <summary>
        /// Xuất toàn bộ danh sách kết quả va chạm ra file định dạng Excel/CSV với bảng mã UTF-8.
        /// </summary>
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

        /// <summary>
        /// Xử lý sự kiện khi đóng Form: giải phóng bộ nhớ đệm hình học của IfcGeometryBridge.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { IfcGeometryBridge.ClearCache(); } catch { }
            base.OnFormClosing(e);
        }
    }
}

