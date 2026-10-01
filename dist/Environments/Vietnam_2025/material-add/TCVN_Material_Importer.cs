// =============================================================================
// TCVN Material Importer for Tekla Structures
// Version: 1.0
// Author: BIM Vietnam Automation
// Description: Macro with WinForms GUI to import TCVN standard materials
//              (Concrete, Rebar, Steel) into Tekla Material Catalog (MATDB.BIN)
// Standards: TCVN 5574:2018 (Concrete), TCVN 1651:2018 (Rebar), TCVN 5709 (Steel)
// =============================================================================
//
// References required (auto-resolved by Tekla macro runner):
//   - Tekla.Structures.dll
//   - Tekla.Structures.Catalogs.dll
//   - Tekla.Structures.Model.dll
//   - System.Windows.Forms.dll
//   - System.Drawing.dll
//
// Usage: Run from Tekla Structures > Applications > Macros > Run...
// =============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Model;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            try
            {
                Model model = new Model();
                if (!model.GetConnectionStatus())
                {
                    MessageBox.Show(
                        "Không thể kết nối đến Tekla Structures!\n" +
                        "Hãy đảm bảo Tekla đang mở và có model đang hoạt động.",
                        "Lỗi kết nối",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                TCVNMaterialForm form = new TCVNMaterialForm();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khởi tạo macro:\n" + ex.Message,
                    "TCVN Material Importer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    // =========================================================================
    // Data class representing a material to import
    // =========================================================================
    public class TCVNMaterial
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }         // "Concrete", "Rebar", "Steel"
        public string Standard { get; set; }          // "TCVN 5574:2018", etc.
        public int TypeEnum { get; set; }             // MaterialItemTypeEnum value
        public double ProfileDensity { get; set; }    // kg/m3
        public double ModulusOfElasticity { get; set; } // MPa
        public double PoissonRatio { get; set; }
        public double ThermalDilatation { get; set; } // 1/K
        public bool Selected { get; set; }

        public TCVNMaterial(string name, string desc, string category, string standard,
            int typeEnum, double density, double modulus, double poisson, double thermal)
        {
            Name = name;
            Description = desc;
            Category = category;
            Standard = standard;
            TypeEnum = typeEnum;
            ProfileDensity = density;
            ModulusOfElasticity = modulus;
            PoissonRatio = poisson;
            ThermalDilatation = thermal;
            Selected = true;
        }
    }

    // =========================================================================
    // Main form with material selection and import functionality
    // =========================================================================
    public class TCVNMaterialForm : Form
    {
        private ListView listViewMaterials;
        private Button btnSelectAll;
        private Button btnDeselectAll;
        private Button btnSelectConcrete;
        private Button btnSelectRebar;
        private Button btnSelectSteel;
        private Button btnImport;
        private Button btnClose;
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblTitle;
        private Label lblInfo;
        private CheckBox chkSkipExisting;
        private RichTextBox txtLog;

        private List<TCVNMaterial> materials;

        public TCVNMaterialForm()
        {
            InitializeMaterialData();
            InitializeComponents();
        }

        // =====================================================================
        // TCVN Material Database - All standard materials per TCVN codes
        // =====================================================================
        private void InitializeMaterialData()
        {
            materials = new List<TCVNMaterial>();

            // -----------------------------------------------------------------
            // CONCRETE - TCVN 5574:2018
            // Format: MATERIAL(type, name, desc, density, E, poisson, thermal)
            // Eb (modulus) values from TCVN 5574:2018 Table 6.11
            // -----------------------------------------------------------------
            // Eb values (MPa): B10=19000, B15=24000, B20=27500, B22.5=29000,
            //   B25=30000, B30=32500, B35=34500, B40=36000, B45=37500,
            //   B50=39000, B55=39500, B60=40000
            materials.Add(new TCVNMaterial("B10", "Be tong B10 (M100) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2400.0, 19000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B15", "Be tong B15 (M200) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 24000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B20", "Be tong B20 (M250) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 27500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B22.5", "Be tong B22.5 (M300) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 29000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B25", "Be tong B25 (M350) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 30000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B30", "Be tong B30 (M400) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 32500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B35", "Be tong B35 (M450) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 34500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B40", "Be tong B40 (M500) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 36000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B45", "Be tong B45 (M600) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 37500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B50", "Be tong B50 (M700) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 39000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B55", "Be tong B55 (M750) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 39500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("B60", "Be tong B60 (M800) - TCVN 5574:2018", "Bê tông", "TCVN 5574:2018", 2, 2500.0, 40000.0, 0.20, 1.0E-05));

            // Old Mac notation (still used in some projects)
            materials.Add(new TCVNMaterial("M150", "Be tong Mac 150", "Bê tông", "TCVN (Mac)", 2, 2400.0, 21000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("M200", "Be tong Mac 200", "Bê tông", "TCVN (Mac)", 2, 2500.0, 24000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("M250", "Be tong Mac 250", "Bê tông", "TCVN (Mac)", 2, 2500.0, 27500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("M300", "Be tong Mac 300", "Bê tông", "TCVN (Mac)", 2, 2500.0, 29000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("M350", "Be tong Mac 350", "Bê tông", "TCVN (Mac)", 2, 2500.0, 30000.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("M400", "Be tong Mac 400", "Bê tông", "TCVN (Mac)", 2, 2500.0, 32500.0, 0.20, 1.0E-05));
            materials.Add(new TCVNMaterial("M500", "Be tong Mac 500", "Bê tông", "TCVN (Mac)", 2, 2500.0, 36000.0, 0.20, 1.0E-05));

            // -----------------------------------------------------------------
            // REINFORCING BAR - TCVN 1651:2018
            // Es = 200000 MPa for all grades
            // -----------------------------------------------------------------
            materials.Add(new TCVNMaterial("CB240-T", "Thep tron tron Fy=240MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("CB300-V", "Thep van Fy=300MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("CB400-V", "Thep van Fy=400MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("CB500-V", "Thep van Fy=500MPa - TCVN 1651:2018", "Thép cốt thép", "TCVN 1651:2018", 5, 7850.0, 200000.0, 0.30, 1.2E-05));

            // JIS rebar commonly used in FDI projects in Vietnam
            materials.Add(new TCVNMaterial("SD295", "Thep van SD295 - JIS G3112", "Thép cốt thép", "JIS G3112", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("SD390", "Thep van SD390 - JIS G3112", "Thép cốt thép", "JIS G3112", 5, 7850.0, 200000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("SD490", "Thep van SD490 - JIS G3112", "Thép cốt thép", "JIS G3112", 5, 7850.0, 200000.0, 0.30, 1.2E-05));

            // ASTM rebar sometimes used
            materials.Add(new TCVNMaterial("A615-Gr60", "Rebar ASTM A615 Grade 60", "Thép cốt thép", "ASTM A615", 5, 7850.0, 200000.0, 0.30, 1.2E-05));

            // -----------------------------------------------------------------
            // STRUCTURAL STEEL - TCVN 5709, JIS, GB, ASTM
            // Es = 210000 MPa (206000 per some codes, using 210000 standard)
            // -----------------------------------------------------------------
            materials.Add(new TCVNMaterial("SS400", "Thep ket cau SS400 Fy=245MPa - JIS G3101", "Thép kết cấu", "JIS G3101", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("SM490", "Thep ket cau SM490 Fy=325MPa - JIS G3106", "Thép kết cấu", "JIS G3106", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("SM520", "Thep ket cau SM520 Fy=365MPa - JIS G3106", "Thép kết cấu", "JIS G3106", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("CT3", "Thep ket cau CT3 - TCVN 5709", "Thép kết cấu", "TCVN 5709", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("Q235", "Thep ket cau Q235 Fy=235MPa - GB/T 700", "Thép kết cấu", "GB/T 700", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("Q345", "Thep ket cau Q345 Fy=345MPa - GB/T 1591", "Thép kết cấu", "GB/T 1591", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("Q355", "Thep ket cau Q355 Fy=355MPa - GB/T 1591", "Thép kết cấu", "GB/T 1591", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("S235JR", "Structural Steel S235JR Fy=235MPa - EN 10025", "Thép kết cấu", "EN 10025", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("S275JR", "Structural Steel S275JR Fy=275MPa - EN 10025", "Thép kết cấu", "EN 10025", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("S355JR", "Structural Steel S355JR Fy=355MPa - EN 10025", "Thép kết cấu", "EN 10025", 1, 7850.0, 210000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("A36", "Structural Steel ASTM A36 Fy=250MPa", "Thép kết cấu", "ASTM A36", 1, 7850.0, 200000.0, 0.30, 1.2E-05));
            materials.Add(new TCVNMaterial("A572-Gr50", "Structural Steel ASTM A572 Gr50 Fy=345MPa", "Thép kết cấu", "ASTM A572", 1, 7850.0, 200000.0, 0.30, 1.2E-05));
        }

        // =====================================================================
        // UI Layout
        // =====================================================================
        private void InitializeComponents()
        {
            // Form settings
            this.Text = "TCVN Material Importer - Tekla Structures Vietnam";
            this.Size = new Size(920, 720);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.Icon = SystemIcons.Application;

            // --- Title panel ---
            Panel panelTop = new Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 65;
            panelTop.BackColor = Color.FromArgb(0, 85, 164); // Blue header
            panelTop.Padding = new Padding(12, 8, 12, 8);

            lblTitle = new Label();
            lblTitle.Text = "TCVN MATERIAL IMPORTER";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(15, 8);

            lblInfo = new Label();
            lblInfo.Text = "Import vật liệu tiêu chuẩn Việt Nam (TCVN 5574:2018, TCVN 1651:2018, TCVN 5709)";
            lblInfo.Font = new Font("Segoe UI", 9F);
            lblInfo.ForeColor = Color.FromArgb(200, 220, 255);
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(15, 38);

            panelTop.Controls.Add(lblTitle);
            panelTop.Controls.Add(lblInfo);

            // --- Filter buttons panel ---
            Panel panelFilter = new Panel();
            panelFilter.Dock = DockStyle.Top;
            panelFilter.Height = 50;
            panelFilter.Padding = new Padding(10, 10, 10, 5);

            btnSelectAll = CreateFilterButton("Chọn tất cả", 10);
            btnSelectAll.Click += (s, e) => SetAllChecked(true);

            btnDeselectAll = CreateFilterButton("Bỏ chọn tất cả", 130);
            btnDeselectAll.Click += (s, e) => SetAllChecked(false);

            btnSelectConcrete = CreateFilterButton("Chỉ Bê tông", 275);
            btnSelectConcrete.BackColor = Color.FromArgb(76, 175, 80);
            btnSelectConcrete.Click += (s, e) => SelectByCategory("Bê tông");

            btnSelectRebar = CreateFilterButton("Chỉ Cốt thép", 390);
            btnSelectRebar.BackColor = Color.FromArgb(255, 152, 0);
            btnSelectRebar.Click += (s, e) => SelectByCategory("Thép cốt thép");

            btnSelectSteel = CreateFilterButton("Chỉ Thép KC", 510);
            btnSelectSteel.BackColor = Color.FromArgb(33, 150, 243);
            btnSelectSteel.Click += (s, e) => SelectByCategory("Thép kết cấu");

            panelFilter.Controls.AddRange(new Control[] {
                btnSelectAll, btnDeselectAll, btnSelectConcrete, btnSelectRebar, btnSelectSteel
            });

            // --- ListView (main content) ---
            listViewMaterials = new ListView();
            listViewMaterials.Dock = DockStyle.Fill;
            listViewMaterials.View = View.Details;
            listViewMaterials.FullRowSelect = true;
            listViewMaterials.CheckBoxes = true;
            listViewMaterials.GridLines = true;
            listViewMaterials.Font = new Font("Consolas", 9F);

            listViewMaterials.Columns.Add("", 30);                  // Checkbox column
            listViewMaterials.Columns.Add("Tên vật liệu", 100);
            listViewMaterials.Columns.Add("Loại", 110);
            listViewMaterials.Columns.Add("Tiêu chuẩn", 130);
            listViewMaterials.Columns.Add("Density (kg/m³)", 120);
            listViewMaterials.Columns.Add("E (MPa)", 100);
            listViewMaterials.Columns.Add("Poisson", 70);
            listViewMaterials.Columns.Add("Thermal (1/K)", 100);
            listViewMaterials.Columns.Add("Mô tả", 200);

            PopulateListView();

            // --- Bottom panel ---
            Panel panelBottom = new Panel();
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Height = 175;
            panelBottom.Padding = new Padding(10);

            chkSkipExisting = new CheckBox();
            chkSkipExisting.Text = "Bỏ qua vật liệu đã tồn tại (không ghi đè)";
            chkSkipExisting.Location = new Point(12, 5);
            chkSkipExisting.AutoSize = true;
            chkSkipExisting.Checked = true;
            chkSkipExisting.Font = new Font("Segoe UI", 9F);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(12, 30);
            progressBar.Size = new Size(870, 22);
            progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            lblStatus = new Label();
            lblStatus.Text = "Sẵn sàng. Chọn vật liệu cần import rồi nhấn [Import vào Tekla].";
            lblStatus.Location = new Point(12, 55);
            lblStatus.Size = new Size(870, 20);
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            txtLog = new RichTextBox();
            txtLog.Location = new Point(12, 78);
            txtLog.Size = new Size(680, 90);
            txtLog.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.FromArgb(30, 30, 30);
            txtLog.ForeColor = Color.FromArgb(200, 200, 200);
            txtLog.Font = new Font("Consolas", 8.5F);
            txtLog.WordWrap = false;

            btnImport = new Button();
            btnImport.Text = "Import vào Tekla";
            btnImport.Location = new Point(700, 78);
            btnImport.Size = new Size(180, 40);
            btnImport.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnImport.BackColor = Color.FromArgb(0, 120, 60);
            btnImport.ForeColor = Color.White;
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnImport.Cursor = Cursors.Hand;
            btnImport.Click += BtnImport_Click;

            btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Location = new Point(700, 125);
            btnClose.Size = new Size(180, 35);
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.AddRange(new Control[] {
                chkSkipExisting, progressBar, lblStatus, txtLog, btnImport, btnClose
            });

            // --- Assemble form ---
            this.Controls.Add(listViewMaterials);
            this.Controls.Add(panelFilter);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelBottom);
        }

        private Button CreateFilterButton(string text, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, 10);
            btn.Size = new Size(115, 30);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = Color.FromArgb(55, 55, 55);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void PopulateListView()
        {
            listViewMaterials.Items.Clear();
            foreach (TCVNMaterial mat in materials)
            {
                ListViewItem item = new ListViewItem("");
                item.Checked = mat.Selected;
                item.SubItems.Add(mat.Name);
                item.SubItems.Add(mat.Category);
                item.SubItems.Add(mat.Standard);
                item.SubItems.Add(mat.ProfileDensity.ToString("F1"));
                item.SubItems.Add(mat.ModulusOfElasticity.ToString("F0"));
                item.SubItems.Add(mat.PoissonRatio.ToString("F2"));
                item.SubItems.Add(mat.ThermalDilatation.ToString("E2"));
                item.SubItems.Add(mat.Description);
                item.Tag = mat;

                // Color-code by category
                if (mat.Category == "Bê tông")
                    item.BackColor = Color.FromArgb(232, 245, 233);
                else if (mat.Category == "Thép cốt thép")
                    item.BackColor = Color.FromArgb(255, 243, 224);
                else if (mat.Category == "Thép kết cấu")
                    item.BackColor = Color.FromArgb(227, 242, 253);

                listViewMaterials.Items.Add(item);
            }
        }

        // =====================================================================
        // Filter helpers
        // =====================================================================
        private void SetAllChecked(bool state)
        {
            foreach (ListViewItem item in listViewMaterials.Items)
            {
                item.Checked = state;
            }
        }

        private void SelectByCategory(string category)
        {
            foreach (ListViewItem item in listViewMaterials.Items)
            {
                TCVNMaterial mat = (TCVNMaterial)item.Tag;
                item.Checked = (mat.Category == category);
            }
        }

        // =====================================================================
        // Import logic using Tekla Open API
        // =====================================================================
        private void BtnImport_Click(object sender, EventArgs e)
        {
            List<TCVNMaterial> selectedMaterials = new List<TCVNMaterial>();
            foreach (ListViewItem item in listViewMaterials.Items)
            {
                if (item.Checked)
                {
                    selectedMaterials.Add((TCVNMaterial)item.Tag);
                }
            }

            if (selectedMaterials.Count == 0)
            {
                MessageBox.Show(
                    "Chưa chọn vật liệu nào!\nHãy tick chọn ít nhất 1 vật liệu để import.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Sẽ import " + selectedMaterials.Count + " vật liệu vào Material Catalog.\n\n" +
                (chkSkipExisting.Checked
                    ? "Vật liệu đã tồn tại sẽ được BỎ QUA (không ghi đè)."
                    : "Vật liệu đã tồn tại sẽ được CẬP NHẬT (ghi đè).") +
                "\n\nTiếp tục?",
                "Xác nhận Import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            btnImport.Enabled = false;
            txtLog.Clear();
            progressBar.Maximum = selectedMaterials.Count;
            progressBar.Value = 0;

            int countAdded = 0;
            int countUpdated = 0;
            int countSkipped = 0;
            int countError = 0;

            foreach (TCVNMaterial mat in selectedMaterials)
            {
                try
                {
                    MaterialItem materialItem = new MaterialItem();

                    // Check if material already exists
                    bool exists = materialItem.Select(mat.Name);

                    if (exists && chkSkipExisting.Checked)
                    {
                        LogMessage("[SKIP] " + mat.Name + " - da ton tai, bo qua.", Color.Gray);
                        countSkipped++;
                    }
                    else
                    {
                        // Set material properties
                        materialItem.MaterialName = mat.Name;
                        materialItem.Type = (MaterialItem.MaterialItemTypeEnum)mat.TypeEnum;
                        materialItem.ProfileDensity = mat.ProfileDensity;
                        materialItem.PlateDensity = mat.ProfileDensity;
                        materialItem.ModulusOfElasticity = mat.ModulusOfElasticity;
                        materialItem.PoissonsRatio = mat.PoissonRatio;
                        materialItem.ThermalDilatation = mat.ThermalDilatation;

                        bool success;
                        if (exists)
                        {
                            // Update existing material
                            success = materialItem.Modify();
                            if (success)
                            {
                                LogMessage("[UPDATE] " + mat.Name + " - cap nhat thanh cong.", Color.FromArgb(255, 183, 77));
                                countUpdated++;
                            }
                            else
                            {
                                LogMessage("[ERROR] " + mat.Name + " - khong the cap nhat!", Color.Red);
                                countError++;
                            }
                        }
                        else
                        {
                            // Insert new material
                            success = materialItem.Insert();
                            if (success)
                            {
                                LogMessage("[ADD]    " + mat.Name + " - them moi thanh cong.", Color.FromArgb(129, 199, 132));
                                countAdded++;
                            }
                            else
                            {
                                LogMessage("[ERROR] " + mat.Name + " - khong the them!", Color.Red);
                                countError++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("[ERROR] " + mat.Name + " - " + ex.Message, Color.Red);
                    countError++;
                }

                progressBar.Value++;
                Application.DoEvents();
            }

            // Summary
            string summary = string.Format(
                "Hoàn tất! Thêm mới: {0} | Cập nhật: {1} | Bỏ qua: {2} | Lỗi: {3}",
                countAdded, countUpdated, countSkipped, countError);
            lblStatus.Text = summary;
            LogMessage("", Color.White);
            LogMessage("=== " + summary + " ===", Color.Cyan);

            if (countAdded > 0 || countUpdated > 0)
            {
                LogMessage("Material Catalog da duoc cap nhat. Bat ky model nao mo se su dung catalog moi.", Color.Cyan);
            }

            btnImport.Enabled = true;

            MessageBox.Show(
                summary + "\n\n" +
                (countAdded + countUpdated > 0
                    ? "Catalog vật liệu đã được cập nhật thành công!\n" +
                      "Mở lại model hoặc vào File > Catalogs > Material Catalog để kiểm tra."
                    : "Không có thay đổi nào được thực hiện."),
                "Kết quả Import",
                MessageBoxButtons.OK,
                countError > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void LogMessage(string message, Color color)
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;
            txtLog.AppendText(message + "\n");
            txtLog.ScrollToCaret();
        }
    }
}
