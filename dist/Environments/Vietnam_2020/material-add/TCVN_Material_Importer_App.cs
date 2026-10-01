// =============================================================================
// Project: TCVN Material Importer Standalone Executable for Tekla Structures
// Standards: TCVN 5574:2018 (Concrete), TCVN 1651:2018 (Rebar), TCVN 5709 (Steel)
// Author: BIM Automation Engineer
// Compatible with C# 5.0+ and .NET Framework 4.5+ / 4.7.2 / 4.8
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Model;

namespace TeklaMaterialImporter
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // Set up dynamic assembly resolver to locate Tekla Structures assemblies
            AppDomain.CurrentDomain.AssemblyResolve += ResolveTeklaAssemblies;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static System.Reflection.Assembly ResolveTeklaAssemblies(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";

            // 1. Check current application directory
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string localPath = Path.Combine(appDir, assemblyName);
            if (File.Exists(localPath)) return System.Reflection.Assembly.LoadFrom(localPath);

            // 2. Check XSBIN environment variable
            string xsBin = Environment.GetEnvironmentVariable("XSBIN");
            if (!string.IsNullOrEmpty(xsBin))
            {
                string p1 = Path.Combine(xsBin, assemblyName);
                if (File.Exists(p1)) return System.Reflection.Assembly.LoadFrom(p1);

                string p2 = Path.Combine(xsBin, "plugins", assemblyName);
                if (File.Exists(p2)) return System.Reflection.Assembly.LoadFrom(p2);
            }

            // 3. Check running Tekla Structures process path
            try
            {
                Process[] processes = Process.GetProcessesByName("TeklaStructures");
                if (processes.Length > 0 && processes[0].MainModule != null)
                {
                    string teklaBin = Path.GetDirectoryName(processes[0].MainModule.FileName);
                    if (!string.IsNullOrEmpty(teklaBin))
                    {
                        string candidate1 = Path.Combine(teklaBin, assemblyName);
                        if (File.Exists(candidate1)) return System.Reflection.Assembly.LoadFrom(candidate1);

                        string candidate2 = Path.Combine(teklaBin, "plugins", assemblyName);
                        if (File.Exists(candidate2)) return System.Reflection.Assembly.LoadFrom(candidate2);
                    }
                }
            }
            catch { }

            // 4. Fallback search across standard installation paths
            string[] probePaths = new string[]
            {
                @"C:\TeklaStructures\2020.0\nt\bin\plugins",
                @"C:\TeklaStructures\2020.0\nt\bin",
                @"C:\TeklaStructures\2021.0\nt\bin\plugins",
                @"C:\TeklaStructures\2022.0\bin",
                @"C:\TeklaStructures\2023.0\bin",
                @"C:\TeklaStructures\2024.0\bin",
                @"C:\TeklaStructures\2025.0\bin",
                @"C:\Program Files\Tekla Structures\2025.0\bin",
                @"C:\Program Files\Trimble\Tekla Structures\2025.0\bin"
            };

            foreach (string probe in probePaths)
            {
                string target = Path.Combine(probe, assemblyName);
                if (File.Exists(target))
                {
                    try { return System.Reflection.Assembly.LoadFrom(target); } catch { }
                }
            }

            return null;
        }
    }

    // =========================================================================
    // Material Model
    // =========================================================================
    public class TCVNMaterialItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Standard { get; set; }
        public int TypeEnum { get; set; }
        public double Density { get; set; }
        public double Modulus { get; set; }
        public double Poisson { get; set; }
        public double Thermal { get; set; }
        public bool Selected { get; set; }

        public TCVNMaterialItem(string name, string desc, string category, string standard,
            int typeEnum, double density, double modulus, double poisson, double thermal)
        {
            Name = name;
            Description = desc;
            Category = category;
            Standard = standard;
            TypeEnum = typeEnum;
            Density = density;
            Modulus = modulus;
            Poisson = poisson;
            Thermal = thermal;
            Selected = true;
        }
    }

    // =========================================================================
    // Main Form
    // =========================================================================
    public class MainForm : Form
    {
        private List<TCVNMaterialItem> allMaterials;
        private ListView lvMaterials;
        private TextBox txtSearch;
        private Label lblCount;
        private CheckBox chkSkipExisting;
        private ProgressBar progressBar;
        private Label lblStatus;
        private RichTextBox rtbLog;
        private Button btnImport;
        private Button btnBackup;
        private Button btnCheckConnection;
        private Label lblConnection;

        public MainForm()
        {
            InitializeData();
            BuildUI();
            CheckTeklaConnection(false);
        }

        private void InitializeData()
        {
            allMaterials = new List<TCVNMaterialItem>();

            // --- 1. CONCRETE - TCVN 5574:2018 ---
            // Modulus Eb (MPa) per Table 6.11
            allMaterials.Add(new TCVNMaterialItem("B10", "Be tong B10 (M100) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2400.0, 19000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B15", "Be tong B15 (M200) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 24000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B20", "Be tong B20 (M250) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 27500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B22.5", "Be tong B22.5 (M300) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 29000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B25", "Be tong B25 (M350) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 30000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B30", "Be tong B30 (M400) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 32500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B35", "Be tong B35 (M450) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 34500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B40", "Be tong B40 (M500) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 36000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B45", "Be tong B45 (M600) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 37500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B50", "Be tong B50 (M700) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 39000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B55", "Be tong B55 (M750) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 39500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("B60", "Be tong B60 (M800) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 40000.0, 0.20, 1.0E-05));

            // Legacy Mac
            allMaterials.Add(new TCVNMaterialItem("M150", "Be tong Mac 150", "Bê tông", "TCVN (Mac)", 2, 2400.0, 21000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("M200", "Be tong Mac 200", "Bê tông", "TCVN (Mac)", 2, 2500.0, 24000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("M250", "Be tong Mac 250", "Bê tông", "TCVN (Mac)", 2, 2500.0, 27500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("M300", "Be tong Mac 300", "Bê tông", "TCVN (Mac)", 2, 2500.0, 29000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("M350", "Be tong Mac 350", "Bê tông", "TCVN (Mac)", 2, 2500.0, 30000.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("M400", "Be tong Mac 400", "Bê tông", "TCVN (Mac)", 2, 2500.0, 32500.0, 0.20, 1.0E-05));
            allMaterials.Add(new TCVNMaterialItem("M500", "Be tong Mac 500", "Bê tông", "TCVN (Mac)", 2, 2500.0, 36000.0, 0.20, 1.0E-05));

            // --- 2. REBAR - TCVN 1651:2018 & JIS & ASTM ---
            allMaterials.Add(new TCVNMaterialItem("CB240-T", "Thep tron tron Fy=240MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("CB300-V", "Thep van Fy=300MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("CB400-V", "Thep van Fy=400MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("CB500-V", "Thep van Fy=500MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("SD295", "Thep van SD295 - JIS G3112", "Thép cốt thép", "JIS G3112", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("SD390", "Thep van SD390 - JIS G3112", "Thép cốt thép", "JIS G3112", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("SD490", "Thep van SD490 - JIS G3112", "Thép cốt thép", "JIS G3112", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("A615-Gr60", "Rebar ASTM A615 Grade 60", "Thép cốt thép", "ASTM A615", 5, 7850.0, 200000.0, 0.30, 1.2E-05));

            // --- 3. STRUCTURAL STEEL ---
            allMaterials.Add(new TCVNMaterialItem("SS400", "Thep ket cau SS400 Fy=245MPa - JIS G3101", "Thép kết cấu", "JIS G3101", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("SM490", "Thep ket cau SM490 Fy=325MPa - JIS G3106", "Thép kết cấu", "JIS G3106", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("SM520", "Thep ket cau SM520 Fy=365MPa - JIS G3106", "Thép kết cấu", "JIS G3106", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("CT3", "Thep ket cau CT3 - TCVN 5709", "Thép kết cấu", "TCVN 5709", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("Q235", "Thep ket cau Q235 Fy=235MPa - GB/T 700", "Thép kết cấu", "GB/T 700", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("Q345", "Thep ket cau Q345 Fy=345MPa - GB/T 1591", "Thép kết cấu", "GB/T 1591", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("Q355", "Thep ket cau Q355 Fy=355MPa - GB/T 1591", "Thép kết cấu", "GB/T 1591", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("S235JR", "Structural Steel S235JR Fy=235MPa - EN 10025", "Thép kết cấu", "EN 10025", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("S275JR", "Structural Steel S275JR Fy=275MPa - EN 10025", "Thép kết cấu", "EN 10025", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("S355JR", "Structural Steel S355JR Fy=355MPa - EN 10025", "Thép kết cấu", "EN 10025", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("A36", "Structural Steel ASTM A36 Fy=250MPa", "Thép kết cấu", "ASTM A36", 1, 7850.0, 200000.0, 0.30, 1.2E-05));
            allMaterials.Add(new TCVNMaterialItem("A572-Gr50", "Structural Steel ASTM A572 Gr50 Fy=345MPa", "Thép kết cấu", "ASTM A572", 1, 7850.0, 200000.0, 0.30, 1.2E-05));
        }

        private void BuildUI()
        {
            this.Text = "TCVN Material Importer v1.2 - Tekla Structures";
            this.Size = new Size(1000, 750);
            this.MinimumSize = new Size(880, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.Icon = SystemIcons.Application;

            // Header Panel
            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 72;
            topPanel.BackColor = Color.FromArgb(16, 75, 142);

            Label title = new Label();
            title.Text = "TCVN MATERIAL IMPORTER";
            title.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(16, 10);
            title.AutoSize = true;

            Label subTitle = new Label();
            subTitle.Text = "Tự động import vật liệu TCVN (Bê tông TCVN 5574, Cốt thép TCVN 1651, Thép KC) vào MATDB.BIN";
            subTitle.Font = new Font("Segoe UI", 9F);
            subTitle.ForeColor = Color.FromArgb(215, 232, 255);
            subTitle.Location = new Point(18, 42);
            subTitle.AutoSize = true;

            lblConnection = new Label();
            lblConnection.Text = "Đang kiểm tra kết nối Tekla...";
            lblConnection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblConnection.ForeColor = Color.Yellow;
            lblConnection.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblConnection.Location = new Point(650, 18);
            lblConnection.Size = new Size(320, 20);
            lblConnection.TextAlign = ContentAlignment.MiddleRight;

            btnCheckConnection = new Button();
            btnCheckConnection.Text = "Kiểm tra kết nối";
            btnCheckConnection.Font = new Font("Segoe UI", 8F);
            btnCheckConnection.Size = new Size(110, 25);
            btnCheckConnection.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCheckConnection.Location = new Point(860, 40);
            btnCheckConnection.BackColor = Color.FromArgb(40, 100, 170);
            btnCheckConnection.ForeColor = Color.White;
            btnCheckConnection.FlatStyle = FlatStyle.Flat;
            btnCheckConnection.Click += delegate(object s, EventArgs e) { CheckTeklaConnection(true); };

            topPanel.Controls.AddRange(new Control[] { title, subTitle, lblConnection, btnCheckConnection });

            // Action & Filter Panel
            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 48;
            filterPanel.BackColor = Color.FromArgb(245, 247, 250);
            filterPanel.Padding = new Padding(10, 8, 10, 8);

            Button btnAll = CreateButton("Chọn tất cả", 12, 10, 95, Color.FromArgb(70, 80, 95), delegate(object s, EventArgs e) { SetAllSelection(true); });
            Button btnNone = CreateButton("Bỏ chọn", 112, 10, 85, Color.FromArgb(100, 110, 125), delegate(object s, EventArgs e) { SetAllSelection(false); });
            Button btnConc = CreateButton("Chỉ Bê tông", 202, 10, 105, Color.FromArgb(46, 125, 50), delegate(object s, EventArgs e) { FilterByCategory("Bê tông"); });
            Button btnRebar = CreateButton("Chỉ Cốt thép", 312, 10, 110, Color.FromArgb(230, 115, 0), delegate(object s, EventArgs e) { FilterByCategory("Thép cốt thép"); });
            Button btnSteel = CreateButton("Chỉ Thép KC", 427, 10, 105, Color.FromArgb(25, 118, 210), delegate(object s, EventArgs e) { FilterByCategory("Thép kết cấu"); });

            Label lblSearch = new Label();
            lblSearch.Text = "Tìm:";
            lblSearch.Location = new Point(545, 14);
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(580, 11);
            txtSearch.Size = new Size(180, 23);
            txtSearch.TextChanged += delegate(object s, EventArgs e) { ApplySearchFilter(); };

            lblCount = new Label();
            lblCount.Text = "Đang chọn: 39/39";
            lblCount.Location = new Point(770, 14);
            lblCount.AutoSize = true;
            lblCount.ForeColor = Color.DarkSlateGray;

            filterPanel.Controls.AddRange(new Control[] { btnAll, btnNone, btnConc, btnRebar, btnSteel, lblSearch, txtSearch, lblCount });

            // Main ListView
            lvMaterials = new ListView();
            lvMaterials.Dock = DockStyle.Fill;
            lvMaterials.View = View.Details;
            lvMaterials.CheckBoxes = true;
            lvMaterials.FullRowSelect = true;
            lvMaterials.GridLines = true;
            lvMaterials.Font = new Font("Consolas", 9F);

            lvMaterials.Columns.Add("", 32);
            lvMaterials.Columns.Add("Tên vật liệu", 100);
            lvMaterials.Columns.Add("Nhóm loại", 110);
            lvMaterials.Columns.Add("Tiêu chuẩn", 130);
            lvMaterials.Columns.Add("K.Lượng (kg/m³)", 125);
            lvMaterials.Columns.Add("E (MPa)", 95);
            lvMaterials.Columns.Add("Poisson", 70);
            lvMaterials.Columns.Add("Thermal (1/K)", 105);
            lvMaterials.Columns.Add("Mô tả kỹ thuật", 240);

            lvMaterials.ItemChecked += delegate(object s, ItemCheckedEventArgs e)
            {
                TCVNMaterialItem item = e.Item.Tag as TCVNMaterialItem;
                if (item != null) item.Selected = e.Item.Checked;
                UpdateCountLabel();
            };

            // Bottom Panel
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 220;
            bottomPanel.BackColor = Color.FromArgb(248, 249, 251);
            bottomPanel.Padding = new Padding(12);

            chkSkipExisting = new CheckBox();
            chkSkipExisting.Text = "Bỏ qua vật liệu đã tồn tại trong catalog (tránh ghi đè thiết lập cũ)";
            chkSkipExisting.Location = new Point(14, 8);
            chkSkipExisting.AutoSize = true;
            chkSkipExisting.Checked = true;
            chkSkipExisting.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(14, 32);
            progressBar.Size = new Size(955, 18);
            progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            lblStatus = new Label();
            lblStatus.Text = "Sẵn sàng nhập vật liệu vào catalog Tekla.";
            lblStatus.Location = new Point(14, 54);
            lblStatus.Size = new Size(650, 20);
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(30, 41, 59);

            rtbLog = new RichTextBox();
            rtbLog.Location = new Point(14, 78);
            rtbLog.Size = new Size(740, 130);
            rtbLog.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            rtbLog.BackColor = Color.FromArgb(24, 28, 36);
            rtbLog.ForeColor = Color.FromArgb(226, 232, 240);
            rtbLog.Font = new Font("Consolas", 8.5F);
            rtbLog.ReadOnly = true;
            rtbLog.WordWrap = false;

            btnBackup = new Button();
            btnBackup.Text = "Sao lưu MATDB.BIN";
            btnBackup.Location = new Point(765, 78);
            btnBackup.Size = new Size(205, 34);
            btnBackup.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnBackup.BackColor = Color.FromArgb(90, 105, 120);
            btnBackup.ForeColor = Color.White;
            btnBackup.FlatStyle = FlatStyle.Flat;
            btnBackup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBackup.Cursor = Cursors.Hand;
            btnBackup.Click += BtnBackup_Click;

            btnImport = new Button();
            btnImport.Text = "Import vào Tekla";
            btnImport.Location = new Point(765, 118);
            btnImport.Size = new Size(205, 48);
            btnImport.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnImport.BackColor = Color.FromArgb(34, 139, 34);
            btnImport.ForeColor = Color.White;
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnImport.Cursor = Cursors.Hand;
            btnImport.Click += BtnImport_Click;

            Button btnExit = new Button();
            btnExit.Text = "Đóng";
            btnExit.Location = new Point(765, 172);
            btnExit.Size = new Size(205, 34);
            btnExit.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += delegate(object s, EventArgs e) { this.Close(); };

            bottomPanel.Controls.AddRange(new Control[] {
                chkSkipExisting, progressBar, lblStatus, rtbLog, btnBackup, btnImport, btnExit
            });

            this.Controls.Add(lvMaterials);
            this.Controls.Add(filterPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);

            RefreshListView(allMaterials);
        }

        private Button CreateButton(string text, int x, int y, int width, Color bg, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 28);
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private void RefreshListView(IEnumerable<TCVNMaterialItem> source)
        {
            lvMaterials.BeginUpdate();
            lvMaterials.Items.Clear();

            foreach (TCVNMaterialItem mat in source)
            {
                ListViewItem lvi = new ListViewItem("");
                lvi.Checked = mat.Selected;
                lvi.SubItems.Add(mat.Name);
                lvi.SubItems.Add(mat.Category);
                lvi.SubItems.Add(mat.Standard);
                lvi.SubItems.Add(mat.Density.ToString("F1"));
                lvi.SubItems.Add(mat.Modulus.ToString("F0"));
                lvi.SubItems.Add(mat.Poisson.ToString("F2"));
                lvi.SubItems.Add(mat.Thermal.ToString("E2"));
                lvi.SubItems.Add(mat.Description);
                lvi.Tag = mat;

                if (mat.Category == "Bê tông") lvi.BackColor = Color.FromArgb(236, 253, 243);
                else if (mat.Category == "Thép cốt thép") lvi.BackColor = Color.FromArgb(254, 243, 226);
                else if (mat.Category == "Thép kết cấu") lvi.BackColor = Color.FromArgb(239, 246, 255);

                lvMaterials.Items.Add(lvi);
            }

            lvMaterials.EndUpdate();
            UpdateCountLabel();
        }

        private void SetAllSelection(bool isChecked)
        {
            foreach (ListViewItem item in lvMaterials.Items)
            {
                item.Checked = isChecked;
                TCVNMaterialItem mat = item.Tag as TCVNMaterialItem;
                if (mat != null) mat.Selected = isChecked;
            }
            UpdateCountLabel();
        }

        private void FilterByCategory(string cat)
        {
            foreach (ListViewItem item in lvMaterials.Items)
            {
                TCVNMaterialItem mat = item.Tag as TCVNMaterialItem;
                if (mat != null)
                {
                    bool match = mat.Category.Equals(cat, StringComparison.OrdinalIgnoreCase);
                    item.Checked = match;
                    mat.Selected = match;
                }
            }
            UpdateCountLabel();
        }

        private void ApplySearchFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(keyword))
            {
                RefreshListView(allMaterials);
                return;
            }

            List<TCVNMaterialItem> filtered = allMaterials.FindAll(delegate(TCVNMaterialItem m)
            {
                return m.Name.ToLowerInvariant().Contains(keyword) ||
                       m.Category.ToLowerInvariant().Contains(keyword) ||
                       m.Standard.ToLowerInvariant().Contains(keyword) ||
                       m.Description.ToLowerInvariant().Contains(keyword);
            });

            RefreshListView(filtered);
        }

        private void UpdateCountLabel()
        {
            int selected = 0;
            foreach (TCVNMaterialItem m in allMaterials)
            {
                if (m.Selected) selected++;
            }
            lblCount.Text = string.Format("Đang chọn: {0}/{1}", selected, allMaterials.Count);
        }

        private bool CheckTeklaConnection(bool showMessage)
        {
            try
            {
                Model model = new Model();
                bool connected = model.GetConnectionStatus();
                if (connected)
                {
                    ModelInfo info = model.GetInfo();
                    string modelName = (info != null) ? info.ModelName : "Đang mở";
                    string modelPath = (info != null) ? info.ModelPath : "";
                    lblConnection.Text = string.Format("Đã kết nối Tekla: [{0}]", modelName);
                    lblConnection.ForeColor = Color.LightGreen;
                    if (showMessage)
                    {
                        MessageBox.Show(
                            string.Format("Kết nối Tekla Structures thành công!\nModel hiện tại: {0}\nĐường dẫn: {1}", modelName, modelPath),
                            "Tekla Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return true;
                }
                else
                {
                    lblConnection.Text = "Chưa kết nối Tekla (Tekla chưa mở model)";
                    lblConnection.ForeColor = Color.Orange;
                    if (showMessage)
                    {
                        MessageBox.Show(
                            "Không tìm thấy model Tekla đang hoạt động.\nVui lòng khởi động Tekla Structures và mở một dự án trước khi Import!",
                            "Chưa kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                lblConnection.Text = "Lỗi kết nối API Tekla";
                lblConnection.ForeColor = Color.Salmon;
                if (showMessage)
                {
                    MessageBox.Show(string.Format("Lỗi kết nối Open API:\n{0}", ex.Message),
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string envDir = Environment.GetEnvironmentVariable("XSDATADIR");
                string matDbPath = null;

                if (!string.IsNullOrEmpty(envDir))
                {
                    string candidate = Path.Combine(envDir, "environments", "vietnam", "General", "profil", "MATDB.BIN");
                    if (File.Exists(candidate)) matDbPath = candidate;
                }

                if (matDbPath == null)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "Chọn file MATDB.BIN cần sao lưu";
                        ofd.Filter = "Tekla Material DB (MATDB.BIN)|MATDB.BIN|All Files (*.*)|*.*";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            matDbPath = ofd.FileName;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(matDbPath) && File.Exists(matDbPath))
                {
                    string backupPath = matDbPath + ".bak_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Copy(matDbPath, backupPath, true);
                    Log(string.Format("[BACKUP] Đã sao lưu MATDB.BIN tại: {0}", backupPath), Color.LightGreen);
                    MessageBox.Show(string.Format("Đã tạo file sao lưu an toàn:\n{0}", backupPath),
                        "Sao lưu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("[LỖI SAO LƯU] {0}", ex.Message), Color.Red);
                MessageBox.Show(string.Format("Không thể tạo bản sao lưu:\n{0}", ex.Message),
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (!CheckTeklaConnection(false))
            {
                MessageBox.Show("Vui lòng mở Tekla Structures và tạo/mở một Model trước khi nhấn Import!",
                    "Chưa kết nối Tekla", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<TCVNMaterialItem> targets = allMaterials.FindAll(delegate(TCVNMaterialItem m) { return m.Selected; });
            if (targets.Count == 0)
            {
                MessageBox.Show("Bạn chưa tick chọn vật liệu nào để import!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string modeMsg = chkSkipExisting.Checked ? "BỎ QUA nếu đã tồn tại" : "CẬP NHẬT/GHI ĐÈ nếu đã tồn tại";
            DialogResult dialogResult = MessageBox.Show(
                string.Format("Sẽ tiến hành import {0} vật liệu TCVN vào Tekla Material Catalog.\n\nChế độ: {1}\n\nBạn có muốn tiếp tục?", targets.Count, modeMsg),
                "Xác nhận Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult != DialogResult.Yes) return;

            btnImport.Enabled = false;
            progressBar.Value = 0;
            progressBar.Maximum = targets.Count;
            rtbLog.Clear();

            int added = 0;
            int modified = 0;
            int skipped = 0;
            int failed = 0;

            Log(string.Format("=== BẮT ĐẦU IMPORT VẬT LIỆU TCVN ({0:HH:mm:ss}) ===", DateTime.Now), Color.Cyan);

            foreach (TCVNMaterialItem mat in targets)
            {
                try
                {
                    MaterialItem item = new MaterialItem();
                    bool exists = item.Select(mat.Name);

                    if (exists && chkSkipExisting.Checked)
                    {
                        Log(string.Format("[SKIP]   '{0}' đã có trong catalog -> Bỏ qua.", mat.Name), Color.Gray);
                        skipped++;
                    }
                    else
                    {
                        item.MaterialName = mat.Name;
                        item.Type = (MaterialItem.MaterialItemTypeEnum)mat.TypeEnum;
                        item.ProfileDensity = mat.Density;
                        item.PlateDensity = mat.Density;
                        item.ModulusOfElasticity = mat.Modulus;
                        item.PoissonsRatio = mat.Poisson;
                        item.ThermalDilatation = mat.Thermal;

                        if (exists)
                        {
                            if (item.Modify())
                            {
                                Log(string.Format("[UPDATE] '{0}' cập nhật thành công.", mat.Name), Color.FromArgb(255, 183, 77));
                                modified++;
                            }
                            else
                            {
                                Log(string.Format("[FAIL]   '{0}' không thể cập nhật (Modify trả về False).", mat.Name), Color.OrangeRed);
                                failed++;
                            }
                        }
                        else
                        {
                            if (item.Insert())
                            {
                                Log(string.Format("[ADD]    '{0}' ({1}) đã thêm vào catalog.", mat.Name, mat.Category), Color.FromArgb(129, 199, 132));
                                added++;
                            }
                            else
                            {
                                Log(string.Format("[FAIL]   '{0}' không thể thêm (Insert trả về False).", mat.Name), Color.OrangeRed);
                                failed++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(string.Format("[ERROR]  '{0}' phát sinh ngoại lệ: {1}", mat.Name, ex.Message), Color.Red);
                    failed++;
                }

                progressBar.Value++;
                Application.DoEvents();
            }

            string resultSummary = string.Format("Hoàn tất: Thêm mới: {0} | Cập nhật: {1} | Bỏ qua: {2} | Lỗi: {3}", added, modified, skipped, failed);
            lblStatus.Text = resultSummary;
            Log(resultSummary, Color.Cyan);
            Log("=== Vui lòng mở File > Catalogs > Material catalog để xem các mác mới ===", Color.Yellow);

            btnImport.Enabled = true;

            MessageBox.Show(
                string.Format("{0}\n\nCác mác vật liệu TCVN đã được cập nhật trực tiếp vào cơ sở dữ liệu Tekla!", resultSummary),
                "Kết quả hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Log(string message, Color color)
        {
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }
    }
}
