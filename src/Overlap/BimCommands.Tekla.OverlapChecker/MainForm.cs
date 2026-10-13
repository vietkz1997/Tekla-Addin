using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeometryHelper;
using GeometryHelper.Clash;
using GeometryHelper.Enums;
using GeometryHelper.Geometry;
using GeometryHelper.TeklaConvert;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Color = System.Drawing.Color;
using TeklaPoint = global::Tekla.Structures.Geometry3d.Point;
using TeklaAabb = global::Tekla.Structures.Geometry3d.AABB;
using TeklaPicker = Tekla.Structures.Model.UI.Picker;
using UIModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using View = System.Windows.Forms.View;

namespace BimCommands.Tekla.OverlapChecker
{
    public class MainForm : Form
    {
        #region Nested Data Structures

        private class ObjectGeoData
        {
            public ModelObject Obj;
            public int Id;
            public string Guid;
            public string TypeName;
            public string Name;
            public string Profile;
            public string Material;
            public string ClassStr;
            public int Phase;
            public string PartMark;
            public string AssemblyMark;
            public double Length;
            public double Volume;
            public bool HasMark;

            public TeklaPoint Min;
            public TeklaPoint Max;
            public TeklaPoint Center;
            public double SizeX;
            public double SizeY;
            public double SizeZ;
            public double Radius;

            // GeometryHelper 3D geometric solid representation
            public GeoSolid3 Solid;
            public List<GeoSolid3> RebarPipes;
            public GeoAabb3 Box;
            public bool IsRebar;

            // Rebar centerline cache for precision rebar overlap checking
            public List<TeklaPoint> RebarPoints;
        }

        private enum RetentionMode
        {
            SmartNumberedFirst, // Prefer object with drawing/part mark
            OlderObjectFirst,   // Keep smaller ID (created earlier)
            NewerObjectFirst,   // Keep larger ID (created later)
            PreserveMainPhase   // Keep object in primary phase
        }

        #endregion

        #region Fields

        private Model _model;
        private List<OverlapItem> _allOverlapList = new List<OverlapItem>();
        private List<OverlapItem> _displayedOverlapList = new List<OverlapItem>();
        private BackgroundWorker _bgWorker;
        private List<ControlPolycurve> _drawnPolycurves = new List<ControlPolycurve>();

        // UI Components - Header & Dashboard
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlStats;
        private Label lblStatScanned;
        private Label lblStatDuplicates;
        private Label lblStatRebars;
        private Label lblStatParts;

        // UI Components - Settings
        private GroupBox grpSettings;
        private Label lblTolerance;
        private NumericUpDown numTolerance;
        private RadioButton radExactDuplicate;
        private RadioButton radClashIntersection;
        private ComboBox cboRetentionRule;
        private Label lblRetention;
        private CheckBox chkAutoSelectTekla;

        // UI Components - Actions
        private GroupBox grpActions;
        private Button btnPickObjects;
        private Button btnFromSelection;
        private Button btnCancelScan;
        private Button btnHighlightTekla;
        private Button btnDrawClash3D;
        private Button btnClearDraw3D;
        private Button btnDeleteDuplicates;
        private Button btnQuarantineClass;
        private Button btnExportCsv;
        private Button btnReconnect;

        // UI Components - Filter & Search Bar
        private Panel pnlFilter;
        private TextBox txtSearch;
        private ComboBox cboFilterType;
        private ComboBox cboFilterOverlap;
        private ComboBox cboFilterClass;
        private ComboBox cboFilterPhase;
        private ComboBox cboFilterRatio;
        private CheckBox chkSelectAll;
        private Label lblFilterSummary;
        private ContextMenuStrip ctxResults;

        // UI Components - Results Split Container
        private SplitContainer splitResults;
        private ListView lstResults;

        // UI Components - Side-by-Side Inspector
        private GroupBox grpInspector;
        private ListView lstInspector;
        private Button btnSwapSelected;
        private Label lblInspectorTip;

        // UI Components - Status & Progress
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusModel;
        private ToolStripProgressBar prgScan;
        private ToolStripStatusLabel lblStatusSpring;
        private ToolStripStatusLabel lblStatusCount;

        #endregion

        #region Constructor & Initialization

        public MainForm()
        {
            InitializeComponent();
            SetupBackgroundWorker();
            ConnectTekla();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            ClearDrawn3D();
            base.OnFormClosing(e);
        }

        private void SetupBackgroundWorker()
        {
            _bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _bgWorker.DoWork += BgWorker_DoWork;
            _bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            _bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void InitializeComponent()
        {
            Text = "Tekla Overlap & Duplicate Checker Pro - [GeometryHelper Powered]";
            Size = new Size(1140, 840);
            MinimumSize = new Size(980, 680);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            TopMost = true;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            // =========================================================================
            // 1. Header Panel
            // =========================================================================
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            lblTitle = new Label
            {
                Text = "🎯 TEKLA OVERLAP & DUPLICATE CHECKER PRO",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(16, 10),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "GeometryHelper BVH clash engine, true 3D solid overlap, smart retention rules, and safe model cleanup",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(18, 36),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // =========================================================================
            // 2. Metrics Dashboard Panel
            // =========================================================================
            pnlStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(14, 6, 14, 6)
            };

            lblStatScanned = CreateMetricCard("🔍 Scanned: 0", Color.FromArgb(30, 41, 59), 14);
            lblStatDuplicates = CreateMetricCard("⚠️ Overlaps: 0", Color.FromArgb(220, 38, 38), 240);
            lblStatRebars = CreateMetricCard("🔩 Rebars: 0", Color.FromArgb(37, 99, 235), 470);
            lblStatParts = CreateMetricCard("🏗️ Parts: 0", Color.FromArgb(16, 185, 129), 690);

            pnlStats.Controls.Add(lblStatScanned);
            pnlStats.Controls.Add(lblStatDuplicates);
            pnlStats.Controls.Add(lblStatRebars);
            pnlStats.Controls.Add(lblStatParts);

            // =========================================================================
            // 3. Settings & Rules Configuration
            // =========================================================================
            grpSettings = new GroupBox
            {
                Text = " ⚙️ DETECTION CRITERIA & RETENTION STRATEGY ",
                Location = new Point(14, 120),
                Size = new Size(1096, 78),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTolerance = new Label
            {
                Text = "Tolerance (mm):",
                Location = new Point(16, 26),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            numTolerance = new NumericUpDown
            {
                Location = new Point(116, 23),
                Width = 60,
                Minimum = 0.1m,
                Maximum = 500m,
                Value = 2.0m,
                DecimalPlaces = 1,
                TextAlign = HorizontalAlignment.Right,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            radExactDuplicate = new RadioButton
            {
                Text = "Exact 100% Geometry",
                Location = new Point(190, 24),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            radClashIntersection = new RadioButton
            {
                Text = "Volume Clash / Overlap",
                Location = new Point(355, 24),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            lblRetention = new Label
            {
                Text = "Keep Rule:",
                Location = new Point(530, 26),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            cboRetentionRule = new ComboBox
            {
                Location = new Point(600, 23),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboRetentionRule.Items.Add("🌟 Smart: Keep Numbered / Drawing Part");
            cboRetentionRule.Items.Add("🏛️ Keep Older Object (Smaller ID)");
            cboRetentionRule.Items.Add("🚀 Keep Newer Object (Larger ID)");
            cboRetentionRule.SelectedIndex = 0;

            chkAutoSelectTekla = new CheckBox
            {
                Text = "Auto-highlight 3D",
                Location = new Point(880, 25),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            grpSettings.Controls.Add(lblTolerance);
            grpSettings.Controls.Add(numTolerance);
            grpSettings.Controls.Add(radExactDuplicate);
            grpSettings.Controls.Add(radClashIntersection);
            grpSettings.Controls.Add(lblRetention);
            grpSettings.Controls.Add(cboRetentionRule);
            grpSettings.Controls.Add(chkAutoSelectTekla);

            // =========================================================================
            // 4. Action Controls Group
            // =========================================================================
            grpActions = new GroupBox
            {
                Text = " 🚀 SCAN & MODEL MANAGEMENT ACTIONS ",
                Location = new Point(14, 204),
                Size = new Size(1096, 75),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnPickObjects = CreateActionButton("🎯 PICK WINDOW", 12, 22, 125, Color.FromArgb(37, 99, 235), Color.White);
            btnPickObjects.Click += (s, e) => StartScan(true);

            btnFromSelection = CreateActionButton("⚡ SELECTION", 142, 22, 125, Color.FromArgb(16, 185, 129), Color.White);
            btnFromSelection.Click += (s, e) => StartScan(false);

            btnCancelScan = CreateActionButton("⛔ CANCEL", 272, 22, 85, Color.FromArgb(100, 116, 139), Color.White);
            btnCancelScan.Enabled = false;
            btnCancelScan.Click += (s, e) => CancelScan();

            btnHighlightTekla = CreateActionButton("👁️ SELECT 3D", 362, 22, 110, Color.FromArgb(245, 158, 11), Color.White);
            btnHighlightTekla.Click += (s, e) => HighlightDuplicatesInTekla();

            btnDrawClash3D = CreateActionButton("✏️ DRAW 3D", 477, 22, 105, Color.FromArgb(139, 92, 246), Color.White);
            btnDrawClash3D.Click += (s, e) => DrawSelectedOrAllClashes();

            btnClearDraw3D = CreateActionButton("🧹 CLEAR 3D", 587, 22, 95, Color.FromArgb(107, 114, 128), Color.White);
            btnClearDraw3D.Click += (s, e) => ClearDrawn3D();

            btnDeleteDuplicates = CreateActionButton("🗑️ DELETE", 687, 22, 125, Color.FromArgb(239, 68, 68), Color.White);
            btnDeleteDuplicates.Click += (s, e) => DeleteDuplicates();

            btnQuarantineClass = CreateActionButton("🛡️ QUARANTINE", 817, 22, 130, Color.FromArgb(147, 51, 234), Color.White);
            btnQuarantineClass.Click += (s, e) => QuarantineDuplicates();

            btnExportCsv = CreateActionButton("📊 EXPORT", 952, 22, 95, Color.FromArgb(15, 118, 110), Color.White);
            btnExportCsv.Click += (s, e) => ExportToCsv();

            btnReconnect = new Button
            {
                Text = "🔄",
                Location = new Point(1052, 22),
                Size = new Size(36, 38),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReconnect.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnReconnect.Click += (s, e) => ConnectTekla();

            grpActions.Controls.Add(btnPickObjects);
            grpActions.Controls.Add(btnFromSelection);
            grpActions.Controls.Add(btnCancelScan);
            grpActions.Controls.Add(btnHighlightTekla);
            grpActions.Controls.Add(btnDrawClash3D);
            grpActions.Controls.Add(btnClearDraw3D);
            grpActions.Controls.Add(btnDeleteDuplicates);
            grpActions.Controls.Add(btnQuarantineClass);
            grpActions.Controls.Add(btnExportCsv);
            grpActions.Controls.Add(btnReconnect);

            // =========================================================================
            // 5. Search & Multi-Filter Bar
            // =========================================================================
            pnlFilter = new Panel
            {
                Location = new Point(14, 285),
                Size = new Size(1096, 72),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkSelectAll = new CheckBox
            {
                Text = "All",
                Location = new Point(4, 8),
                AutoSize = true,
                Checked = false,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            chkSelectAll.CheckedChanged += (s, e) =>
            {
                foreach (ListViewItem item in lstResults.Items)
                {
                    item.Checked = chkSelectAll.Checked;
                }
            };

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(56, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            txtSearch = new TextBox
            {
                Location = new Point(106, 5),
                Width = 180,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            Label lblFilterType = new Label
            {
                Text = "Type:",
                Location = new Point(296, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            cboFilterType = new ComboBox
            {
                Location = new Point(336, 5),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterType.Items.AddRange(new object[] { "All Types", "Rebar Only", "Part Only" });
            cboFilterType.SelectedIndex = 0;
            cboFilterType.SelectedIndexChanged += (s, e) => ApplyFilter();

            Label lblFilterOverlap = new Label
            {
                Text = "Mode:",
                Location = new Point(466, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            cboFilterOverlap = new ComboBox
            {
                Location = new Point(512, 5),
                Width = 135,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterOverlap.Items.AddRange(new object[] { "All Modes", "100% Duplicate", "Volume Overlap" });
            cboFilterOverlap.SelectedIndex = 0;
            cboFilterOverlap.SelectedIndexChanged += (s, e) => ApplyFilter();

            lblFilterSummary = new Label
            {
                Text = "No scan performed yet.",
                Location = new Point(660, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            // Row 2 Filters: Class, Phase, Overlap %, Reset
            Label lblFilterClass = new Label
            {
                Text = "Class:",
                Location = new Point(4, 42),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            cboFilterClass = new ComboBox
            {
                Location = new Point(50, 39),
                Width = 110,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterClass.Items.Add("All Classes");
            cboFilterClass.SelectedIndex = 0;
            cboFilterClass.SelectedIndexChanged += (s, e) => ApplyFilter();

            Label lblFilterPhase = new Label
            {
                Text = "Phase:",
                Location = new Point(170, 42),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            cboFilterPhase = new ComboBox
            {
                Location = new Point(220, 39),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterPhase.Items.Add("All Phases");
            cboFilterPhase.SelectedIndex = 0;
            cboFilterPhase.SelectedIndexChanged += (s, e) => ApplyFilter();

            Label lblFilterRatio = new Label
            {
                Text = "Overlap %:",
                Location = new Point(330, 42),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            cboFilterRatio = new ComboBox
            {
                Location = new Point(400, 39),
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterRatio.Items.AddRange(new object[] { "All Overlap %", ">= 95% (Exact)", "50% - 95%", "< 50% (Minor)" });
            cboFilterRatio.SelectedIndex = 0;
            cboFilterRatio.SelectedIndexChanged += (s, e) => ApplyFilter();

            Button btnResetFilter = new Button
            {
                Text = "Reset Filters",
                Location = new Point(540, 37),
                Size = new Size(95, 26),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnResetFilter.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnResetFilter.Click += (s, e) =>
            {
                txtSearch.Text = "";
                cboFilterType.SelectedIndex = 0;
                cboFilterOverlap.SelectedIndex = 0;
                if (cboFilterClass.Items.Count > 0) cboFilterClass.SelectedIndex = 0;
                if (cboFilterPhase.Items.Count > 0) cboFilterPhase.SelectedIndex = 0;
                cboFilterRatio.SelectedIndex = 0;
                ApplyFilter();
            };

            pnlFilter.Controls.Add(chkSelectAll);
            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(lblFilterType);
            pnlFilter.Controls.Add(cboFilterType);
            pnlFilter.Controls.Add(lblFilterOverlap);
            pnlFilter.Controls.Add(cboFilterOverlap);
            pnlFilter.Controls.Add(lblFilterSummary);
            pnlFilter.Controls.Add(lblFilterClass);
            pnlFilter.Controls.Add(cboFilterClass);
            pnlFilter.Controls.Add(lblFilterPhase);
            pnlFilter.Controls.Add(cboFilterPhase);
            pnlFilter.Controls.Add(lblFilterRatio);
            pnlFilter.Controls.Add(cboFilterRatio);
            pnlFilter.Controls.Add(btnResetFilter);

            // =========================================================================
            // 6. Results & Side-by-Side Inspector (SplitContainer)
            // =========================================================================
            splitResults = new SplitContainer
            {
                Orientation = Orientation.Horizontal,
                Location = new Point(14, 362),
                Size = new Size(1096, 415),
                SplitterDistance = 250,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(203, 213, 225)
            };

            // Context Menu for Right-Click on Results
            ctxResults = new ContextMenuStrip();
            var mnuDelete = new ToolStripMenuItem("Delete Duplicate (Del)", null, (s, e) => DeleteDuplicates());
            mnuDelete.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            mnuDelete.ForeColor = Color.FromArgb(220, 38, 38);

            var mnuSwap = new ToolStripMenuItem("Swap Kept <-> Duplicate (S)", null, (s, e) => BtnSwapSelected_Click(s, e));
            var mnuQuarantine = new ToolStripMenuItem("Quarantine to Class 99", null, (s, e) => QuarantineDuplicates());
            var mnuSelect3D = new ToolStripMenuItem("Select in Tekla 3D", null, (s, e) => HighlightDuplicatesInTekla());
            var mnuZoom = new ToolStripMenuItem("Zoom to Clash", null, (s, e) => LstResults_DoubleClick(s, e));
            var mnuDraw3D = new ToolStripMenuItem("Draw 3D Clash Box", null, (s, e) => DrawSelectedOrAllClashes());

            var mnuCheckSelected = new ToolStripMenuItem("Check Selected Rows", null, (s, e) =>
            {
                foreach (int idx in lstResults.SelectedIndices)
                {
                    lstResults.Items[idx].Checked = true;
                }
            });
            var mnuUncheckSelected = new ToolStripMenuItem("Uncheck Selected Rows", null, (s, e) =>
            {
                foreach (int idx in lstResults.SelectedIndices)
                {
                    lstResults.Items[idx].Checked = false;
                }
            });

            ctxResults.Items.Add(mnuDelete);
            ctxResults.Items.Add(new ToolStripSeparator());
            ctxResults.Items.Add(mnuSwap);
            ctxResults.Items.Add(mnuQuarantine);
            ctxResults.Items.Add(new ToolStripSeparator());
            ctxResults.Items.Add(mnuSelect3D);
            ctxResults.Items.Add(mnuZoom);
            ctxResults.Items.Add(mnuDraw3D);
            ctxResults.Items.Add(new ToolStripSeparator());
            ctxResults.Items.Add(mnuCheckSelected);
            ctxResults.Items.Add(mnuUncheckSelected);

            // Main Results ListView
            lstResults = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true,
                GridLines = true,
                ContextMenuStrip = ctxResults,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                BackColor = Color.White
            };

            lstResults.Columns.Add("Dup ID", 75);
            lstResults.Columns.Add("Keep ID", 75);
            lstResults.Columns.Add("Type", 95);
            lstResults.Columns.Add("Name", 100);
            lstResults.Columns.Add("Profile", 95);
            lstResults.Columns.Add("Material", 80);
            lstResults.Columns.Add("Overlap Mode", 110);
            lstResults.Columns.Add("Clash Vol", 90);
            lstResults.Columns.Add("Overlap %", 75);
            lstResults.Columns.Add("Keep Reason", 220);

            lstResults.SelectedIndexChanged += LstResults_SelectedIndexChanged;
            lstResults.DoubleClick += LstResults_DoubleClick;
            lstResults.KeyDown += LstResults_KeyDown;

            splitResults.Panel1.Controls.Add(lstResults);

            // Side-by-Side Inspector Panel
            grpInspector = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = " 🔍 SIDE-BY-SIDE PROPERTY INSPECTOR (COMPARE KEPT vs DUPLICATE) ",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                BackColor = Color.FromArgb(248, 250, 252)
            };

            btnSwapSelected = new Button
            {
                Text = "🔄 SWAP KEPT ⮂ DUPLICATE",
                Location = new Point(12, 22),
                Size = new Size(210, 30),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSwapSelected.FlatAppearance.BorderSize = 0;
            btnSwapSelected.Click += BtnSwapSelected_Click;

            lblInspectorTip = new Label
            {
                Text = "💡 Select any duplicate item above to inspect differences. Double-click to zoom and draw 3D clash in Tekla.",
                Location = new Point(235, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            lstInspector = new ListView
            {
                Location = new Point(12, 58),
                Size = new Size(1070, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            lstInspector.Columns.Add("Property", 150);
            lstInspector.Columns.Add("🟢 ORIGINAL (TO BE KEPT)", 450);
            lstInspector.Columns.Add("🔴 DUPLICATE (TO BE REMOVED)", 450);

            grpInspector.Controls.Add(btnSwapSelected);
            grpInspector.Controls.Add(lblInspectorTip);
            grpInspector.Controls.Add(lstInspector);

            splitResults.Panel2.Controls.Add(grpInspector);

            // =========================================================================
            // 7. Status Strip
            // =========================================================================
            statusStrip = new StatusStrip { BackColor = Color.FromArgb(241, 245, 249) };

            lblStatusModel = new ToolStripStatusLabel
            {
                Text = "Connecting to Tekla...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                IsLink = true,
                LinkBehavior = LinkBehavior.HoverUnderline
            };
            lblStatusModel.Click += (s, e) => ConnectTekla();

            prgScan = new ToolStripProgressBar
            {
                Width = 140,
                Visible = false,
                Style = ProgressBarStyle.Continuous
            };

            lblStatusSpring = new ToolStripStatusLabel { Spring = true };

            lblStatusCount = new ToolStripStatusLabel
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            statusStrip.Items.Add(lblStatusModel);
            statusStrip.Items.Add(prgScan);
            statusStrip.Items.Add(lblStatusSpring);
            statusStrip.Items.Add(lblStatusCount);

            // Add all controls to Form
            Controls.Add(splitResults);
            Controls.Add(pnlFilter);
            Controls.Add(grpActions);
            Controls.Add(grpSettings);
            Controls.Add(pnlStats);
            Controls.Add(pnlHeader);
            Controls.Add(statusStrip);
        }

        private Label CreateMetricCard(string text, Color fgColor, int x)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = fgColor
            };
        }

        private Button CreateActionButton(string text, int x, int y, int w, Color bg, Color fg)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, 38),
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        #endregion

        #region Tekla Connection

        private void ConnectTekla()
        {
            try
            {
                _model = new Model();
                if (_model.GetConnectionStatus())
                {
                    string modelName = _model.GetInfo().ModelName;
                    lblStatusModel.Text = "🟢 Connected: " + modelName;
                    lblStatusModel.ForeColor = Color.FromArgb(22, 101, 52);
                    Text = $"Tekla Overlap & Duplicate Checker Pro ({modelName}) - [GeometryHelper Powered]";
                    lblStatusCount.Text = "Connected to Tekla Structures. Ready to scan.";
                }
                else
                {
                    lblStatusModel.Text = "🔴 Not connected to Tekla (Click to reconnect)";
                    lblStatusModel.ForeColor = Color.Red;
                    lblStatusCount.Text = "Tekla model not found. Open Tekla Structures and open a model (.db1).";
                }
            }
            catch (Exception ex)
            {
                lblStatusModel.Text = "⚠️ Connection error: " + ex.Message;
                lblStatusModel.ForeColor = Color.Red;
            }
        }

        private bool EnsureTeklaConnected()
        {
            if (_model == null || !_model.GetConnectionStatus())
            {
                ConnectTekla();
                if (_model == null || !_model.GetConnectionStatus())
                {
                    MessageBox.Show(
                        "Cannot connect to Tekla Structures!\n\n" +
                        "Please verify:\n" +
                        "1. Tekla Structures is currently open.\n" +
                        "2. A 3D model (.db1) is open.\n" +
                        "3. Click [🔄 RECONNECT] on the toolbar.",
                        "Tekla Not Connected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region High-Performance Scanning Engine (GeometryHelper Clash3.Find)

        private struct ScanWorkerArgs
        {
            public List<ModelObject> Objects;
            public double Tolerance;
            public bool ExactOnly;
            public RetentionMode Retention;
        }

        private void StartScan(bool pickWindow)
        {
            if (!EnsureTeklaConnected()) return;
            if (_bgWorker.IsBusy) return;

            List<ModelObject> list = new List<ModelObject>();
            try
            {
                if (pickWindow)
                {
                    TeklaPicker picker = new TeklaPicker();
                    lblStatusCount.Text = "🎯 Pick window area in Tekla 3D view (Middle-click to finish)...";
                    statusStrip.Refresh();
                    ModelObjectEnumerator enumerator = picker.PickObjects(
                        TeklaPicker.PickObjectsEnum.PICK_N_OBJECTS,
                        "Pick window area containing objects to check for duplicates (Middle-click to finish):");
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != null)
                        {
                            list.Add(enumerator.Current);
                        }
                    }
                }
                else
                {
                    UIModelObjectSelector selector = new UIModelObjectSelector();
                    ModelObjectEnumerator enumerator = selector.GetSelectedObjects();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != null)
                        {
                            list.Add(enumerator.Current);
                        }
                    }
                    if (list.Count == 0)
                    {
                        MessageBox.Show(
                            "No objects currently selected in Tekla!\n\nPlease select objects in the model first or use 'PICK WINDOW'.",
                            "Information",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch
            {
                lblStatusCount.Text = "Selection cancelled.";
                return;
            }

            // Expand any RebarSets into their constituent reinforcements
            var expandedList = new List<ModelObject>();
            foreach (var obj in list)
            {
                if (obj is RebarSet rSet)
                {
                    var enumRebars = rSet.GetReinforcements();
                    while (enumRebars != null && enumRebars.MoveNext())
                    {
                        if (enumRebars.Current != null) expandedList.Add(enumRebars.Current);
                    }
                }
                else
                {
                    expandedList.Add(obj);
                }
            }

            if (expandedList.Count < 2)
            {
                MessageBox.Show(
                    "At least 2 objects are required to check for duplicates!",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Prepare UI state for scanning
            ClearDrawn3D();
            btnPickObjects.Enabled = false;
            btnFromSelection.Enabled = false;
            btnCancelScan.Enabled = true;
            prgScan.Visible = true;
            prgScan.Value = 0;
            lblStatusCount.Text = $"Analyzing {expandedList.Count} objects with GeometryHelper 3D Engine...";

            RetentionMode retMode = RetentionMode.SmartNumberedFirst;
            if (cboRetentionRule.SelectedIndex == 1) retMode = RetentionMode.OlderObjectFirst;
            else if (cboRetentionRule.SelectedIndex == 2) retMode = RetentionMode.NewerObjectFirst;

            ScanWorkerArgs args = new ScanWorkerArgs
            {
                Objects = expandedList,
                Tolerance = (double)numTolerance.Value,
                ExactOnly = radExactDuplicate.Checked,
                Retention = retMode
            };

            _bgWorker.RunWorkerAsync(args);
        }

        private void CancelScan()
        {
            if (_bgWorker.IsBusy)
            {
                _bgWorker.CancelAsync();
                lblStatusCount.Text = "Cancelling scan...";
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ScanWorkerArgs args = (ScanWorkerArgs)e.Argument;
            List<ModelObject> rawObjects = args.Objects;
            double tolerance = args.Tolerance;
            bool exactOnly = args.ExactOnly;
            RetentionMode retention = args.Retention;

            // Phase 1: Feature & Solid Extraction
            List<ObjectGeoData> geoList = new List<ObjectGeoData>();
            int total = rawObjects.Count;
            for (int i = 0; i < total; i++)
            {
                if (_bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                ObjectGeoData geo = ExtractGeoData(rawObjects[i]);
                if (geo != null)
                {
                    geoList.Add(geo);
                }

                if (i % 100 == 0 || i == total - 1)
                {
                    int pct = (int)((i + 1) * 35.0 / total);
                    _bgWorker.ReportProgress(pct, $"Extracting 3D solids {i + 1}/{total}...");
                }
            }

            // Phase 2: Index Solids for GeometryHelper Clash3.Find
            var solidsToCheck = new List<GeoSolid3>();
            var solidOwners = new List<ObjectGeoData>();

            for (int i = 0; i < geoList.Count; i++)
            {
                var g = geoList[i];
                if (g.Solid != null)
                {
                    solidsToCheck.Add(g.Solid);
                    solidOwners.Add(g);
                }
                else if (g.RebarPipes != null && g.RebarPipes.Count > 0)
                {
                    foreach (var pipe in g.RebarPipes)
                    {
                        solidsToCheck.Add(pipe);
                        solidOwners.Add(g);
                    }
                }
            }

            long PairKey(int idA, int idB)
            {
                int min = Math.Min(idA, idB);
                int max = Math.Max(idA, idB);
                return ((long)min << 32) | (uint)max;
            }

            var pairMap = new Dictionary<long, (ObjectGeoData A, ObjectGeoData B, double TotalVol, List<GeoSolid3> Solids, ClashKind Kind)>();

            if (solidsToCheck.Count >= 2)
            {
                _bgWorker.ReportProgress(40, $"Running multi-core BVH clash check on {solidsToCheck.Count} bodies...");

                ClashOptions clashOptions = new ClashOptions(
                    clearance: tolerance,
                    includeTouching: false, // Disregard surface touching contacts to prevent false positives
                    maxDegreeOfParallelism: Environment.ProcessorCount
                );

                ClashResult[] rawClashes = Clash3.Find(solidsToCheck, clashOptions);

                if (_bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (rawClashes != null)
                {
                    foreach (var c in rawClashes)
                    {
                        if (c == null) continue;
                        var ownerA = solidOwners[c.First];
                        var ownerB = solidOwners[c.Second];

                        if (ownerA.Id == ownerB.Id) continue;
                        if (ownerA.IsRebar != ownerB.IsRebar) continue;
                        // Structural types must match (e.g. Beam with Beam, Plate with Plate)
                        if (!string.Equals(ownerA.TypeName, ownerB.TypeName, StringComparison.OrdinalIgnoreCase)) continue;

                        long pKey = PairKey(ownerA.Id, ownerB.Id);
                        if (!pairMap.TryGetValue(pKey, out var existing))
                        {
                            var solids = new List<GeoSolid3>();
                            if (c.Overlaps != null) solids.AddRange(c.Overlaps);
                            pairMap[pKey] = (ownerA, ownerB, c.Volume, solids, c.Kind);
                        }
                        else
                        {
                            double newVol = existing.TotalVol + c.Volume;
                            if (c.Overlaps != null) existing.Solids.AddRange(c.Overlaps);
                            pairMap[pKey] = (existing.A, existing.B, newVol, existing.Solids, c.Kind == ClashKind.Hard ? ClashKind.Hard : existing.Kind);
                        }
                    }
                }
            }

            // Phase 3: Fallback check for objects without solid bodies (e.g. unmeshed rebars/parts)
            // using spatial centerline or bounding box matching
            var unconverted = geoList.Where(g => g.Solid == null && (g.RebarPipes == null || g.RebarPipes.Count == 0)).ToList();
            if (unconverted.Count > 0)
            {
                for (int i = 0; i < unconverted.Count; i++)
                {
                    var a = unconverted[i];
                    for (int j = i + 1; j < unconverted.Count; j++)
                    {
                        var b = unconverted[j];
                        if (a.Id == b.Id || a.IsRebar != b.IsRebar) continue;
                        if (!string.Equals(a.TypeName, b.TypeName, StringComparison.OrdinalIgnoreCase)) continue;

                        long pKey = PairKey(a.Id, b.Id);
                        if (pairMap.ContainsKey(pKey)) continue;

                        if (CheckFallbackOverlap(a, b, tolerance, exactOnly))
                        {
                            // Calculate approximate intersection volume between bounding boxes
                            double interMinX = Math.Max(a.Min.X, b.Min.X);
                            double interMaxX = Math.Min(a.Max.X, b.Max.X);
                            double interMinY = Math.Max(a.Min.Y, b.Min.Y);
                            double interMaxY = Math.Min(a.Max.Y, b.Max.Y);
                            double interMinZ = Math.Max(a.Min.Z, b.Min.Z);
                            double interMaxZ = Math.Min(a.Max.Z, b.Max.Z);

                            double interAabbVol = 0;
                            if (interMaxX > interMinX && interMaxY > interMinY && interMaxZ > interMinZ)
                            {
                                interAabbVol = (interMaxX - interMinX) * (interMaxY - interMinY) * (interMaxZ - interMinZ);
                            }

                            double aabbVolA = Math.Max(1.0, a.SizeX * a.SizeY * a.SizeZ);
                            double approxOverlapVol;
                            if (a.Volume > 0 && b.Volume > 0)
                            {
                                double ratio = Math.Min(1.0, interAabbVol / aabbVolA);
                                approxOverlapVol = Math.Min(a.Volume, b.Volume) * ratio;
                            }
                            else
                            {
                                approxOverlapVol = interAabbVol;
                            }

                            pairMap[pKey] = (a, b, approxOverlapVol, new List<GeoSolid3>(), ClashKind.Hard);
                        }
                    }
                }
            }

            // Phase 4: Construct OverlapItems & Decide Retention
            List<OverlapItem> results = new List<OverlapItem>();
            HashSet<int> duplicateIds = new HashSet<int>();

            int pairIndex = 0;
            int totalPairs = pairMap.Count;

            foreach (var entry in pairMap.Values)
            {
                pairIndex++;
                if (pairIndex % 50 == 0 || pairIndex == totalPairs)
                {
                    int pct = 70 + (int)(pairIndex * 30.0 / Math.Max(1, totalPairs));
                    _bgWorker.ReportProgress(pct, $"Classifying overlap results {pairIndex}/{totalPairs}...");
                }

                var a = entry.A;
                var b = entry.B;

                // Chain duplicate prevention: skip if either object was already handled/deleted in another duplicate pair
                if (duplicateIds.Contains(a.Id) || duplicateIds.Contains(b.Id))
                {
                    continue;
                }

                double overlapVol = entry.TotalVol;
                double volA = a.Volume > 0 ? a.Volume : (a.Solid != null ? a.Solid.Volume : 1.0);
                double volB = b.Volume > 0 ? b.Volume : (b.Solid != null ? b.Solid.Volume : 1.0);
                double minVol = Math.Min(volA, volB);
                double overlapRatio = minVol > 0 ? Math.Min(1.0, overlapVol / minVol) : 0.0;

                double centerDist = Distance(a.Center, b.Center);

                // High-fidelity exact duplicate logic (requiring >= 95% volume overlap or aligned bounding box & profile)
                bool isGeometricMatch = centerDist <= tolerance &&
                    Math.Abs(a.SizeX - b.SizeX) <= tolerance &&
                    Math.Abs(a.SizeY - b.SizeY) <= tolerance &&
                    Math.Abs(a.SizeZ - b.SizeZ) <= tolerance;

                bool isProfileMatch = string.Equals(a.Profile, b.Profile, StringComparison.OrdinalIgnoreCase);

                bool isExactDuplicate = (overlapRatio >= 0.95 && (isProfileMatch || a.IsRebar)) ||
                    (isGeometricMatch && (isProfileMatch || a.IsRebar));

                // For rebars, also check centerline agreement
                if (!isExactDuplicate && a.IsRebar && a.RebarPoints != null && b.RebarPoints != null && a.RebarPoints.Count == b.RebarPoints.Count)
                {
                    bool match = true;
                    for (int p = 0; p < a.RebarPoints.Count; p++)
                    {
                        if (Distance(a.RebarPoints[p], b.RebarPoints[p]) > tolerance)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) isExactDuplicate = true;
                }

                if (exactOnly && !isExactDuplicate)
                {
                    continue;
                }

                string overlapType = isExactDuplicate ? "100% Duplicate" : "Volume Overlap";

                ObjectGeoData kept;
                ObjectGeoData dup;
                string reason;
                DecideRetention(a, b, retention, out kept, out dup, out reason);
                duplicateIds.Add(dup.Id);

                TeklaPoint clashCenter = new TeklaPoint(
                    (dup.Center.X + kept.Center.X) / 2.0,
                    (dup.Center.Y + kept.Center.Y) / 2.0,
                    (dup.Center.Z + kept.Center.Z) / 2.0);

                OverlapItem item = new OverlapItem
                {
                    IsSelected = true,
                    OriginalObject = kept.Obj,
                    DuplicateObject = dup.Obj,
                    OriginalId = kept.Id,
                    DuplicateId = dup.Id,
                    OriginalGuid = kept.Guid,
                    DuplicateGuid = dup.Guid,
                    TypeName = dup.TypeName,
                    OriginalName = kept.Name,
                    DuplicateName = dup.Name,
                    OriginalProfile = kept.Profile,
                    DuplicateProfile = dup.Profile,
                    OriginalMaterial = kept.Material,
                    DuplicateMaterial = dup.Material,
                    OriginalClass = kept.ClassStr,
                    DuplicateClass = dup.ClassStr,
                    OriginalPhase = kept.Phase,
                    DuplicatePhase = dup.Phase,
                    OriginalMark = kept.PartMark,
                    DuplicateMark = dup.PartMark,
                    OriginalLength = kept.Length,
                    DuplicateLength = dup.Length,
                    OriginalVolume = kept.Volume,
                    DuplicateVolume = dup.Volume,
                    CenterPoint = clashCenter,
                    CenterStr = $"{clashCenter.X:0}, {clashCenter.Y:0}, {clashCenter.Z:0}",
                    MinPoint = new TeklaPoint(Math.Min(a.Min.X, b.Min.X), Math.Min(a.Min.Y, b.Min.Y), Math.Min(a.Min.Z, b.Min.Z)),
                    MaxPoint = new TeklaPoint(Math.Max(a.Max.X, b.Max.X), Math.Max(a.Max.Y, b.Max.Y), Math.Max(a.Max.Z, b.Max.Z)),
                    ClashVolume = overlapVol,
                    OverlapRatio = isExactDuplicate ? 1.0 : overlapRatio,
                    ClashSolids = entry.Solids,
                    OverlapType = overlapType,
                    RetentionReason = reason
                };

                results.Add(item);
            }

            e.Result = new Tuple<List<OverlapItem>, int>(results, rawObjects.Count);
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgScan.Value = Math.Min(100, Math.Max(0, e.ProgressPercentage));
            lblStatusCount.Text = e.UserState?.ToString() ?? "Processing...";
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPickObjects.Enabled = true;
            btnFromSelection.Enabled = true;
            btnCancelScan.Enabled = false;
            prgScan.Visible = false;

            if (e.Cancelled)
            {
                lblStatusCount.Text = "Scan operation cancelled by user.";
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show("Scan error: " + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusCount.Text = "Scan error.";
                return;
            }

            var tuple = (Tuple<List<OverlapItem>, int>)e.Result;
            _allOverlapList = tuple.Item1;
            int totalScanned = tuple.Item2;

            // Update Metrics Cards
            int rebarsCount = _allOverlapList.Count(x => x.TypeName.Contains("Rebar") || x.TypeName.Contains("Reinforcement"));
            int partsCount = _allOverlapList.Count - rebarsCount;

            lblStatScanned.Text = $"🔍 Scanned: {totalScanned}";
            lblStatDuplicates.Text = $"⚠️ Overlaps: {_allOverlapList.Count}";
            lblStatRebars.Text = $"🔩 Rebars: {rebarsCount}";
            lblStatParts.Text = $"🏗️ Parts: {partsCount}";

            PopulateDynamicFilterOptions();
            ApplyFilter();

            if (_allOverlapList.Count > 0 && chkAutoSelectTekla.Checked)
            {
                HighlightDuplicatesInTekla();
            }
            else if (_allOverlapList.Count == 0)
            {
                MessageBox.Show(
                    "Great news! No duplicate or overlapping objects detected in the selected model area.",
                    "Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Smart Retention & Fallback Algorithms

        private void DecideRetention(ObjectGeoData a, ObjectGeoData b, RetentionMode mode, out ObjectGeoData kept, out ObjectGeoData dup, out string reason)
        {
            switch (mode)
            {
                case RetentionMode.SmartNumberedFirst:
                    if (a.HasMark && !b.HasMark)
                    {
                        kept = a;
                        dup = b;
                        reason = $"Kept ID {a.Id} (Has Mark: {a.PartMark})";
                        return;
                    }
                    if (b.HasMark && !a.HasMark)
                    {
                        kept = b;
                        dup = a;
                        reason = $"Kept ID {b.Id} (Has Mark: {b.PartMark})";
                        return;
                    }
                    if (a.Id < b.Id)
                    {
                        kept = a;
                        dup = b;
                        reason = $"Kept ID {a.Id} (Created earlier)";
                    }
                    else
                    {
                        kept = b;
                        dup = a;
                        reason = $"Kept ID {b.Id} (Created earlier)";
                    }
                    return;

                case RetentionMode.NewerObjectFirst:
                    if (a.Id > b.Id)
                    {
                        kept = a;
                        dup = b;
                        reason = $"Kept ID {a.Id} (Newer object)";
                    }
                    else
                    {
                        kept = b;
                        dup = a;
                        reason = $"Kept ID {b.Id} (Newer object)";
                    }
                    return;

                case RetentionMode.OlderObjectFirst:
                default:
                    if (a.Id < b.Id)
                    {
                        kept = a;
                        dup = b;
                        reason = $"Kept ID {a.Id} (Created earlier)";
                    }
                    else
                    {
                        kept = b;
                        dup = a;
                        reason = $"Kept ID {b.Id} (Created earlier)";
                    }
                    return;
            }
        }

        private bool CheckFallbackOverlap(ObjectGeoData a, ObjectGeoData b, double tol, bool exactOnly)
        {
            double dist = Distance(a.Center, b.Center);
            if (dist > a.Radius + b.Radius + tol) return false;

            if (exactOnly)
            {
                if (!string.Equals(a.TypeName, b.TypeName, StringComparison.OrdinalIgnoreCase)) return false;
                if (!string.Equals(a.Profile, b.Profile, StringComparison.OrdinalIgnoreCase)) return false;
                if (dist > tol) return false;

                double diffX = Math.Abs(a.SizeX - b.SizeX);
                double diffY = Math.Abs(a.SizeY - b.SizeY);
                double diffZ = Math.Abs(a.SizeZ - b.SizeZ);
                return diffX <= tol && diffY <= tol && diffZ <= tol;
            }

            bool overlapX = a.Min.X <= b.Max.X + tol && a.Max.X >= b.Min.X - tol;
            bool overlapY = a.Min.Y <= b.Max.Y + tol && a.Max.Y >= b.Min.Y - tol;
            bool overlapZ = a.Min.Z <= b.Max.Z + tol && a.Max.Z >= b.Min.Z - tol;

            return overlapX && overlapY && overlapZ && dist <= Math.Max(a.Radius, b.Radius);
        }

        private ObjectGeoData ExtractGeoData(ModelObject obj)
        {
            try
            {
                int id = obj.Identifier.ID;
                string guid = obj.Identifier.GUID.ToString();
                string typeName = obj.GetType().Name;
                string name = "";
                string profile = "";
                string material = "";
                string classStr = "";
                int phase = 0;
                string partPos = "";
                string assPos = "";
                double length = 0;
                double volume = 0;
                TeklaPoint minPt = null;
                TeklaPoint maxPt = null;
                List<TeklaPoint> rebarPts = null;
                GeoSolid3 geoSolid = null;
                List<GeoSolid3> rebarPipes = null;
                bool isRebar = false;

                if (obj is Part part)
                {
                    name = part.Name;
                    profile = part.Profile.ProfileString;
                    material = part.Material.MaterialString;
                    classStr = part.Class;
                    try
                    {
                        var s = part.GetSolid();
                        if (s != null)
                        {
                            minPt = s.MinimumPoint;
                            maxPt = s.MaximumPoint;
                            if (s.TryToGeoSolid3(out GeoSolid3 parsedSolid))
                            {
                                geoSolid = parsedSolid;
                            }
                        }
                    }
                    catch { }
                }
                else if (obj is Reinforcement rebar)
                {
                    isRebar = true;
                    name = rebar.Name;
                    material = rebar.Grade;
                    classStr = rebar.Class.ToString();

                    string size = "";
                    rebar.GetReportProperty("SIZE", ref size);
                    profile = rebar.Grade + " d" + size;

                    try
                    {
                        var s = rebar.GetSolid();
                        if (s != null)
                        {
                            minPt = s.MinimumPoint;
                            maxPt = s.MaximumPoint;
                        }
                    }
                    catch { }

                    // Extract rebar geometries with GeometryHelper TeklaConvert
                    try
                    {
                        var rebarGeos = rebar.GetRebarGeometriesWithoutClashes(true);
                        if (rebarGeos != null)
                        {
                            rebarPipes = new List<GeoSolid3>();
                            foreach (RebarGeometry rGeo in rebarGeos.OfType<RebarGeometry>())
                            {
                                try
                                {
                                    var arc = rGeo.ToGeoPolylineArc3();
                                    double barRad = rGeo.ToBarRadius();
                                    if (barRad > 0 && arc.Length > 0)
                                    {
                                        var pipe = GeoSolid3.Pipe(arc, barRad, 1.0);
                                        rebarPipes.Add(pipe);
                                    }
                                }
                                catch { }

                                if (rebarPts == null && rGeo.Shape != null)
                                {
                                    rebarPts = new List<TeklaPoint>();
                                    foreach (var pt in rGeo.Shape.Points)
                                    {
                                        if (pt is TeklaPoint p) rebarPts.Add(p);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                if (minPt == null || maxPt == null)
                {
                    if (geoSolid != null)
                    {
                        var aabb = geoSolid.GetAabb();
                        minPt = new TeklaPoint(aabb.Min.X, aabb.Min.Y, aabb.Min.Z);
                        maxPt = new TeklaPoint(aabb.Max.X, aabb.Max.Y, aabb.Max.Z);
                    }
                    else
                    {
                        return null;
                    }
                }

                try
                {
                    obj.GetReportProperty("PHASE", ref phase);
                    obj.GetReportProperty("PART_POS", ref partPos);
                    obj.GetReportProperty("ASSEMBLY_POS", ref assPos);
                    obj.GetReportProperty("LENGTH", ref length);
                    obj.GetReportProperty("VOLUME", ref volume);
                }
                catch { }

                if (volume <= 0 && geoSolid != null)
                {
                    volume = geoSolid.Volume;
                }

                bool hasMark = !string.IsNullOrWhiteSpace(partPos) && partPos != "?" && partPos != "0";

                TeklaPoint center = new TeklaPoint(
                    (minPt.X + maxPt.X) / 2.0,
                    (minPt.Y + maxPt.Y) / 2.0,
                    (minPt.Z + maxPt.Z) / 2.0);

                double szX = Math.Abs(maxPt.X - minPt.X);
                double szY = Math.Abs(maxPt.Y - minPt.Y);
                double szZ = Math.Abs(maxPt.Z - minPt.Z);
                double radius = Math.Sqrt(szX * szX + szY * szY + szZ * szZ) / 2.0;

                GeoAabb3 box = geoSolid != null ? geoSolid.GetAabb() :
                    (rebarPipes != null && rebarPipes.Count > 0 ?
                        GeoAabb3.FromPoints(rebarPipes.SelectMany(p => new[] { p.GetAabb().Min, p.GetAabb().Max })) :
                        new GeoAabb3(new GeoPoint3(minPt.X, minPt.Y, minPt.Z), new GeoPoint3(maxPt.X, maxPt.Y, maxPt.Z)));

                return new ObjectGeoData
                {
                    Obj = obj,
                    Id = id,
                    Guid = guid,
                    TypeName = typeName,
                    Name = name,
                    Profile = profile,
                    Material = material,
                    ClassStr = classStr,
                    Phase = phase,
                    PartMark = partPos,
                    AssemblyMark = assPos,
                    Length = length,
                    Volume = volume,
                    HasMark = hasMark,
                    Min = minPt,
                    Max = maxPt,
                    Center = center,
                    SizeX = szX,
                    SizeY = szY,
                    SizeZ = szZ,
                    Radius = radius,
                    Solid = geoSolid,
                    RebarPipes = rebarPipes,
                    Box = box,
                    IsRebar = isRebar,
                    RebarPoints = rebarPts
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region UI Filtering & Selection

        private void PopulateDynamicFilterOptions()
        {
            try
            {
                // Populate unique Classes
                if (cboFilterClass != null)
                {
                    cboFilterClass.Items.Clear();
                    cboFilterClass.Items.Add("All Classes");
                    var uniqueClasses = _allOverlapList
                        .Select(x => x.DuplicateClass)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                    foreach (var cls in uniqueClasses)
                    {
                        cboFilterClass.Items.Add($"Class {cls}");
                    }
                    cboFilterClass.SelectedIndex = 0;
                }

                // Populate unique Phases
                if (cboFilterPhase != null)
                {
                    cboFilterPhase.Items.Clear();
                    cboFilterPhase.Items.Add("All Phases");
                    var uniquePhases = _allOverlapList
                        .Select(x => x.DuplicatePhase)
                        .Distinct()
                        .OrderBy(p => p)
                        .ToList();
                    foreach (var ph in uniquePhases)
                    {
                        cboFilterPhase.Items.Add($"Phase {ph}");
                    }
                    cboFilterPhase.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLowerInvariant();
            int typeFilter = cboFilterType.SelectedIndex;
            int overlapFilter = cboFilterOverlap.SelectedIndex;
            string selectedClass = (cboFilterClass != null && cboFilterClass.SelectedIndex > 0) ? cboFilterClass.SelectedItem.ToString().Replace("Class ", "").Trim() : "";
            int selectedPhase = -1;
            if (cboFilterPhase != null && cboFilterPhase.SelectedIndex > 0)
            {
                string phStr = cboFilterPhase.SelectedItem.ToString().Replace("Phase ", "").Trim();
                int.TryParse(phStr, out selectedPhase);
            }
            int ratioFilter = (cboFilterRatio != null) ? cboFilterRatio.SelectedIndex : 0;

            _displayedOverlapList = _allOverlapList.Where(item =>
            {
                // Keyword match
                if (!string.IsNullOrEmpty(keyword))
                {
                    bool match = item.DuplicateId.ToString().Contains(keyword) ||
                                 item.OriginalId.ToString().Contains(keyword) ||
                                 item.DuplicateName.ToLowerInvariant().Contains(keyword) ||
                                 item.DuplicateProfile.ToLowerInvariant().Contains(keyword) ||
                                 item.DuplicateMark.ToLowerInvariant().Contains(keyword);
                    if (!match) return false;
                }

                // Type filter
                if (typeFilter == 1) // Rebar
                {
                    if (!item.TypeName.Contains("Rebar") && !item.TypeName.Contains("Reinforcement")) return false;
                }
                else if (typeFilter == 2) // Part
                {
                    if (item.TypeName.Contains("Rebar") || item.TypeName.Contains("Reinforcement")) return false;
                }

                // Overlap mode filter
                if (overlapFilter == 1 && !item.OverlapType.Contains("100%")) return false;
                if (overlapFilter == 2 && !item.OverlapType.Contains("Volume")) return false;

                // Class filter
                if (!string.IsNullOrEmpty(selectedClass))
                {
                    if (item.DuplicateClass != selectedClass && item.OriginalClass != selectedClass) return false;
                }

                // Phase filter
                if (selectedPhase >= 0)
                {
                    if (item.DuplicatePhase != selectedPhase && item.OriginalPhase != selectedPhase) return false;
                }

                // Ratio filter
                if (ratioFilter == 1) // >= 95%
                {
                    if (item.OverlapRatio < 0.95) return false;
                }
                else if (ratioFilter == 2) // 50% - 95%
                {
                    if (item.OverlapRatio < 0.50 || item.OverlapRatio >= 0.95) return false;
                }
                else if (ratioFilter == 3) // < 50%
                {
                    if (item.OverlapRatio >= 0.50) return false;
                }

                return true;
            }).ToList();

            // Populate ListView
            lstResults.BeginUpdate();
            lstResults.Items.Clear();

            foreach (var item in _displayedOverlapList)
            {
                ListViewItem lvi = new ListViewItem(item.DuplicateId.ToString());
                lvi.Checked = item.IsSelected;
                lvi.SubItems.Add(item.OriginalId.ToString());
                lvi.SubItems.Add(item.TypeName);
                lvi.SubItems.Add(item.DuplicateName);
                lvi.SubItems.Add(item.DuplicateProfile);
                lvi.SubItems.Add(item.DuplicateMaterial);
                lvi.SubItems.Add(item.OverlapType);
                lvi.SubItems.Add(item.ClashVolumeFormatted);
                lvi.SubItems.Add(item.OverlapPercentageFormatted);
                lvi.SubItems.Add(item.RetentionReason);

                if (item.OverlapType.Contains("100%"))
                {
                    lvi.ForeColor = Color.FromArgb(185, 28, 28);
                }
                else
                {
                    lvi.ForeColor = Color.FromArgb(30, 41, 59);
                }

                lstResults.Items.Add(lvi);
            }
            lstResults.EndUpdate();

            lblFilterSummary.Text = $"Showing: {_displayedOverlapList.Count} / {_allOverlapList.Count} overlaps";
            lblStatusCount.Text = $"Found {_allOverlapList.Count} duplicate items ({_displayedOverlapList.Count} shown).";
        }

        private void LstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstResults.SelectedIndices.Count == 0)
            {
                lstInspector.Items.Clear();
                return;
            }

            int index = lstResults.SelectedIndices[0];
            if (index < 0 || index >= _displayedOverlapList.Count) return;

            OverlapItem item = _displayedOverlapList[index];
            PopulateInspector(item);

            // Auto-highlight in Tekla 3D when navigating rows with Up/Down arrow keys or mouse
            if (chkAutoSelectTekla.Checked && item.DuplicateObject != null)
            {
                try
                {
                    ArrayList list = new ArrayList { item.DuplicateObject };
                    if (item.OriginalObject != null) list.Add(item.OriginalObject);
                    UIModelObjectSelector selector = new UIModelObjectSelector();
                    selector.Select(list);
                }
                catch { }
            }
        }

        private void LstResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                DeleteDuplicates();
            }
            else if (e.KeyCode == Keys.S && (e.Modifiers == Keys.None || e.Modifiers == Keys.Control))
            {
                e.Handled = true;
                BtnSwapSelected_Click(sender, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                LstResults_DoubleClick(sender, EventArgs.Empty);
            }
        }

        private List<OverlapItem> GetTargetItems()
        {
            List<OverlapItem> targets = new List<OverlapItem>();

            // 1. First priority: items with checked checkboxes
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                if (lstResults.Items[i].Checked && i < _displayedOverlapList.Count)
                {
                    targets.Add(_displayedOverlapList[i]);
                }
            }

            // 2. If NO checkbox is checked, fallback to currently selected row(s) (Up/Down arrow navigation or Mouse)
            if (targets.Count == 0 && lstResults.SelectedIndices.Count > 0)
            {
                foreach (int selIdx in lstResults.SelectedIndices)
                {
                    if (selIdx >= 0 && selIdx < _displayedOverlapList.Count)
                    {
                        targets.Add(_displayedOverlapList[selIdx]);
                    }
                }
            }

            return targets;
        }

        private void PopulateInspector(OverlapItem item)
        {
            lstInspector.BeginUpdate();
            lstInspector.Items.Clear();

            void AddRow(string prop, string dupVal, string origVal)
            {
                ListViewItem row = new ListViewItem(prop);
                row.SubItems.Add(origVal ?? "");
                row.SubItems.Add(dupVal ?? "");
                if (origVal != dupVal)
                {
                    row.BackColor = Color.FromArgb(254, 242, 242);
                    row.Font = new Font(lstInspector.Font, FontStyle.Bold);
                }
                lstInspector.Items.Add(row);
            }

            AddRow("ID", item.DuplicateId.ToString(), item.OriginalId.ToString());
            AddRow("GUID", item.DuplicateGuid, item.OriginalGuid);
            AddRow("Object Type", item.TypeName, item.TypeName);
            AddRow("Name", item.DuplicateName, item.OriginalName);
            AddRow("Profile", item.DuplicateProfile, item.OriginalProfile);
            AddRow("Material", item.DuplicateMaterial, item.OriginalMaterial);
            AddRow("Class", item.DuplicateClass, item.OriginalClass);
            AddRow("Phase", item.DuplicatePhase.ToString(), item.OriginalPhase.ToString());
            AddRow("Part Mark", item.DuplicateMark, item.OriginalMark);
            AddRow("Length (mm)", $"{item.DuplicateLength:0.0}", $"{item.OriginalLength:0.0}");
            AddRow("Volume (cm³)", $"{item.DuplicateVolume / 1e3:0.0}", $"{item.OriginalVolume / 1e3:0.0}");
            AddRow("3D Clash Volume", item.ClashVolumeFormatted, item.ClashVolumeFormatted);
            AddRow("Overlap Ratio", item.OverlapPercentageFormatted, item.OverlapPercentageFormatted);
            AddRow("Overlap Type", item.OverlapType, item.OverlapType);
            AddRow("Clash Center", item.CenterStr, item.CenterStr);
            AddRow("Retention Logic", "Marked for Deletion", item.RetentionReason);

            lstInspector.EndUpdate();
        }

        private void BtnSwapSelected_Click(object sender, EventArgs e)
        {
            if (lstResults.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Please select an item in the list above to swap!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = lstResults.SelectedIndices[0];
            if (index < 0 || index >= _displayedOverlapList.Count) return;

            OverlapItem item = _displayedOverlapList[index];
            item.Swap();

            // Refresh Inspector and ListView row
            PopulateInspector(item);
            ListViewItem lvi = lstResults.Items[index];
            lvi.Text = item.DuplicateId.ToString();
            lvi.SubItems[1].Text = item.OriginalId.ToString();
            lvi.SubItems[3].Text = item.DuplicateName;
            lvi.SubItems[4].Text = item.DuplicateProfile;
            lvi.SubItems[5].Text = item.DuplicateMaterial;
            lvi.SubItems[9].Text = item.RetentionReason;

            global::Tekla.Structures.Model.Operations.Operation.DisplayPrompt($"Swapped duplicate pair. Kept: ID {item.OriginalId}, Duplicate to delete: ID {item.DuplicateId}");
        }

        private void LstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.SelectedIndices.Count == 0) return;

            int index = lstResults.SelectedIndices[0];
            if (index < 0 || index >= _displayedOverlapList.Count) return;

            OverlapItem item = _displayedOverlapList[index];
            if (item.DuplicateObject != null)
            {
                ArrayList list = new ArrayList { item.DuplicateObject };
                if (item.OriginalObject != null) list.Add(item.OriginalObject);

                UIModelObjectSelector selector = new UIModelObjectSelector();
                selector.Select(list);

                // Zoom to clash location
                try
                {
                    double pad = 400.0;
                    var min = new TeklaPoint(item.MinPoint.X - pad, item.MinPoint.Y - pad, item.MinPoint.Z - pad);
                    var max = new TeklaPoint(item.MaxPoint.X + pad, item.MaxPoint.Y + pad, item.MaxPoint.Z + pad);
                    ViewHandler.ZoomToBoundingBox(new TeklaAabb(min, max));
                }
                catch { }

                // Draw 3D clash boundary in Tekla view
                DrawClashInTekla(item);

                global::Tekla.Structures.Model.Operations.Operation.DisplayPrompt(
                    $"Zoomed & Selected Duplicate ID {item.DuplicateId} (Kept ID {item.OriginalId}) in Tekla. Clash Volume: {item.ClashVolumeFormatted}.");
            }
        }

        #endregion

        #region Actions (Highlight, Draw 3D, Delete, Quarantine, Export)

        private void HighlightDuplicatesInTekla()
        {
            if (_allOverlapList.Count == 0) return;

            List<OverlapItem> targets = GetTargetItems();
            ArrayList listToHighlight = new ArrayList();
            foreach (var item in targets)
            {
                if (item.DuplicateObject != null) listToHighlight.Add(item.DuplicateObject);
            }

            if (listToHighlight.Count == 0)
            {
                MessageBox.Show("No items are selected or checked to highlight!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                UIModelObjectSelector selector = new UIModelObjectSelector();
                selector.Select(listToHighlight);
                lblStatusCount.Text = $"Highlighted {listToHighlight.Count} duplicate object(s) in Tekla 3D.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Selection error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDrawn3D()
        {
            try
            {
                if (_drawnPolycurves.Count > 0)
                {
                    foreach (var p in _drawnPolycurves)
                    {
                        try { p.Delete(); } catch { }
                    }
                    _drawnPolycurves.Clear();
                    if (_model != null && _model.GetConnectionStatus())
                    {
                        _model.CommitChanges();
                    }
                }
                lblStatusCount.Text = "Cleared 3D temporary drawings from Tekla view.";
            }
            catch { }
        }

        private void DrawClashInTekla(OverlapItem item)
        {
            try
            {
                ClearDrawn3D();
                if (item.ClashSolids != null && item.ClashSolids.Count > 0)
                {
                    foreach (var solid in item.ClashSolids)
                    {
                        var polycurves = solid.DrawToTekla(ControlObjectColorEnum.RED, ControlObjectColorEnum.WHITE);
                        if (polycurves != null) _drawnPolycurves.AddRange(polycurves);
                    }
                }
                else
                {
                    // Draw bounding box if exact boolean solid is not cached
                    var aabb = new GeoAabb3(
                        new GeoPoint3(item.MinPoint.X, item.MinPoint.Y, item.MinPoint.Z),
                        new GeoPoint3(item.MaxPoint.X, item.MaxPoint.Y, item.MaxPoint.Z));
                    var corners = aabb.GetCorners();
                    if (corners.Length >= 8)
                    {
                        var pts1 = new[] { corners[0], corners[1], corners[2], corners[3] }.Select(p => new TeklaPoint(p.X, p.Y, p.Z));
                        var pts2 = new[] { corners[4], corners[5], corners[6], corners[7] }.Select(p => new TeklaPoint(p.X, p.Y, p.Z));
                        var poly1 = pts1.DrawPolyline(true, ControlObjectColorEnum.RED);
                        var poly2 = pts2.DrawPolyline(true, ControlObjectColorEnum.RED);
                        if (poly1 != null) _drawnPolycurves.Add(poly1);
                        if (poly2 != null) _drawnPolycurves.Add(poly2);
                    }
                }

                if (_drawnPolycurves.Count > 0 && _model != null && _model.GetConnectionStatus())
                {
                    _model.CommitChanges();
                }
            }
            catch { }
        }

        private void DrawSelectedOrAllClashes()
        {
            if (_displayedOverlapList.Count == 0)
            {
                MessageBox.Show("No overlaps to draw. Please scan first!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ClearDrawn3D();

            var toDraw = new List<OverlapItem>();
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                if (lstResults.Items[i].Checked && i < _displayedOverlapList.Count)
                {
                    toDraw.Add(_displayedOverlapList[i]);
                }
            }

            if (toDraw.Count == 0 && lstResults.SelectedIndices.Count > 0)
            {
                int selIdx = lstResults.SelectedIndices[0];
                if (selIdx >= 0 && selIdx < _displayedOverlapList.Count)
                {
                    toDraw.Add(_displayedOverlapList[selIdx]);
                }
            }

            if (toDraw.Count == 0) toDraw.AddRange(_displayedOverlapList);

            foreach (var item in toDraw)
            {
                if (item.ClashSolids != null && item.ClashSolids.Count > 0)
                {
                    foreach (var solid in item.ClashSolids)
                    {
                        var polycurves = solid.DrawToTekla(ControlObjectColorEnum.RED, ControlObjectColorEnum.WHITE);
                        if (polycurves != null) _drawnPolycurves.AddRange(polycurves);
                    }
                }
                else
                {
                    var aabb = new GeoAabb3(
                        new GeoPoint3(item.MinPoint.X, item.MinPoint.Y, item.MinPoint.Z),
                        new GeoPoint3(item.MaxPoint.X, item.MaxPoint.Y, item.MaxPoint.Z));
                    var corners = aabb.GetCorners();
                    if (corners.Length >= 8)
                    {
                        var pts1 = new[] { corners[0], corners[1], corners[2], corners[3] }.Select(p => new TeklaPoint(p.X, p.Y, p.Z));
                        var pts2 = new[] { corners[4], corners[5], corners[6], corners[7] }.Select(p => new TeklaPoint(p.X, p.Y, p.Z));
                        var poly1 = pts1.DrawPolyline(true, ControlObjectColorEnum.RED);
                        var poly2 = pts2.DrawPolyline(true, ControlObjectColorEnum.RED);
                        if (poly1 != null) _drawnPolycurves.Add(poly1);
                        if (poly2 != null) _drawnPolycurves.Add(poly2);
                    }
                }
            }

            if (_drawnPolycurves.Count > 0)
            {
                if (_model != null && _model.GetConnectionStatus())
                {
                    _model.CommitChanges();
                }
                lblStatusCount.Text = $"Drew {_drawnPolycurves.Count} 3D clash boundary curves in Tekla view.";
            }
        }

        private void DeleteDuplicates()
        {
            if (!EnsureTeklaConnected()) return;

            List<OverlapItem> toDelete = GetTargetItems();

            if (toDelete.Count == 0)
            {
                MessageBox.Show("Please select or check at least one duplicate item in the list to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to permanently delete {toDelete.Count} duplicate object(s) from Tekla Structures?\n\n" +
                $"✓ All corresponding original/kept objects will be preserved.\n" +
                $"💡 Tip: You can also use [QUARANTINE CLASS] to isolate them before deleting.",
                "Confirm Permanent Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            int deletedCount = 0;
            foreach (OverlapItem item in toDelete)
            {
                try
                {
                    if (item.DuplicateObject != null && item.DuplicateObject.Delete())
                    {
                        deletedCount++;
                        _allOverlapList.Remove(item);
                    }
                }
                catch { }
            }

            if (_model != null && _model.GetConnectionStatus())
            {
                _model.CommitChanges();
            }
            ClearDrawn3D();

            ApplyFilter();

            // Refresh Metrics
            int rebarsCount = _allOverlapList.Count(x => x.TypeName.Contains("Rebar") || x.TypeName.Contains("Reinforcement"));
            int partsCount = _allOverlapList.Count - rebarsCount;
            lblStatDuplicates.Text = $"⚠️ Overlaps: {_allOverlapList.Count}";
            lblStatRebars.Text = $"🔩 Rebars: {rebarsCount}";
            lblStatParts.Text = $"🏗️ Parts: {partsCount}";

            MessageBox.Show(
                $"Successfully deleted {deletedCount} duplicate object(s) from the Tekla model.",
                "Deletion Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void QuarantineDuplicates()
        {
            List<OverlapItem> toQuarantine = new List<OverlapItem>();
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                if (lstResults.Items[i].Checked && i < _displayedOverlapList.Count)
                {
                    toQuarantine.Add(_displayedOverlapList[i]);
                }
            }

            if (toQuarantine.Count == 0)
            {
                MessageBox.Show("Please check at least one duplicate object in the list to quarantine!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int changedCount = 0;
            foreach (OverlapItem item in toQuarantine)
            {
                try
                {
                    if (item.DuplicateObject is Part part)
                    {
                        part.Class = "99";
                        if (part.Modify()) changedCount++;
                    }
                    else if (item.DuplicateObject is Reinforcement rebar)
                    {
                        rebar.Class = 99;
                        if (rebar.Modify()) changedCount++;
                    }
                }
                catch { }
            }

            if (_model != null && _model.GetConnectionStatus())
            {
                _model.CommitChanges();
            }

            MessageBox.Show(
                $"Successfully moved {changedCount} duplicate object(s) to Quarantine Class 99.\n\n" +
                $"You can now visually inspect them in Tekla 3D views using view color representation.",
                "Quarantine Applied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ExportToCsv()
        {
            if (_allOverlapList.Count == 0)
            {
                MessageBox.Show("No scan results to export!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV Spreadsheet (*.csv)|*.csv",
                FileName = $"Tekla_Duplicates_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Export Duplicate Objects Report"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DuplicateId,KeptId,TypeName,Name,Profile,Material,OriginalMark,DuplicateMark,Center_X,Center_Y,Center_Z,OverlapType,ClashVolume_mm3,OverlapPercentage,RetentionReason");

                foreach (var item in _allOverlapList)
                {
                    sb.AppendLine(
                        $"{item.DuplicateId},{item.OriginalId},\"{item.TypeName}\",\"{item.DuplicateName}\"," +
                        $"\"{item.DuplicateProfile}\",\"{item.DuplicateMaterial}\",\"{item.OriginalMark}\",\"{item.DuplicateMark}\"," +
                        $"{item.CenterPoint.X:0.0},{item.CenterPoint.Y:0.0},{item.CenterPoint.Z:0.0}," +
                        $"\"{item.OverlapType}\",{item.ClashVolume:0.0},{item.OverlapRatio * 100.0:0.0},\"{item.RetentionReason}\"");
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show(
                    $"Report successfully exported to:\n{sfd.FileName}",
                    "Export Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static double Distance(TeklaPoint a, TeklaPoint b)
        {
            if (a == null || b == null) return double.MaxValue;
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        #endregion
    }
}
