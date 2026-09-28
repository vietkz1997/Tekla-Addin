using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Color = System.Drawing.Color;
using TeklaPoint = global::Tekla.Structures.Geometry3d.Point;
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
        private Button btnDeleteDuplicates;
        private Button btnQuarantineClass;
        private Button btnExportCsv;
        private Button btnReconnect;

        // UI Components - Filter & Search Bar
        private Panel pnlFilter;
        private TextBox txtSearch;
        private ComboBox cboFilterType;
        private ComboBox cboFilterOverlap;
        private CheckBox chkSelectAll;
        private Label lblFilterSummary;

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
            Text = "Tekla Overlap & Duplicate Checker Pro - [My-tool]";
            Size = new Size(1120, 820);
            MinimumSize = new Size(960, 680);
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
                Text = "⚡ TEKLA OVERLAP & DUPLICATE CHECKER PRO",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(16, 10),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "High-performance spatial grid detection, smart retention rules, and safe model cleanup",
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

            lblStatScanned = CreateMetricCard("📦 Scanned: 0", Color.FromArgb(30, 41, 59), 14);
            lblStatDuplicates = CreateMetricCard("🔴 Duplicates: 0", Color.FromArgb(185, 28, 28), 210);
            lblStatRebars = CreateMetricCard("🔩 Rebars: 0", Color.FromArgb(2, 132, 199), 420);
            lblStatParts = CreateMetricCard("🏗️ Parts: 0", Color.FromArgb(217, 119, 6), 610);

            pnlStats.Controls.Add(lblStatScanned);
            pnlStats.Controls.Add(lblStatDuplicates);
            pnlStats.Controls.Add(lblStatRebars);
            pnlStats.Controls.Add(lblStatParts);

            // =========================================================================
            // 3. Settings Group
            // =========================================================================
            grpSettings = new GroupBox
            {
                Text = " ⚙️ DETECTION CRITERIA & RETENTION STRATEGY ",
                Location = new Point(14, 120),
                Size = new Size(1076, 78),
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
            cboRetentionRule.Items.Add("⭐ Smart: Keep Numbered / Drawing Part");
            cboRetentionRule.Items.Add("⏱️ Keep Older Object (Smaller ID)");
            cboRetentionRule.Items.Add("🆕 Keep Newer Object (Larger ID)");
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
                Text = " 🎯 SCAN & MODEL MANAGEMENT ACTIONS ",
                Location = new Point(14, 204),
                Size = new Size(1076, 75),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnPickObjects = CreateActionButton("🎯  PICK WINDOW", 14, 22, 140, Color.FromArgb(37, 99, 235), Color.White);
            btnPickObjects.Click += (s, e) => StartScan(true);

            btnFromSelection = CreateActionButton("⚡  FROM SELECTION", 160, 22, 155, Color.FromArgb(16, 185, 129), Color.White);
            btnFromSelection.Click += (s, e) => StartScan(false);

            btnCancelScan = CreateActionButton("⛔  CANCEL", 321, 22, 95, Color.FromArgb(100, 116, 139), Color.White);
            btnCancelScan.Enabled = false;
            btnCancelScan.Click += (s, e) => CancelScan();

            btnHighlightTekla = CreateActionButton("👁️  HIGHLIGHT 3D", 422, 22, 135, Color.FromArgb(245, 158, 11), Color.White);
            btnHighlightTekla.Click += (s, e) => HighlightDuplicatesInTekla();

            btnDeleteDuplicates = CreateActionButton("🗑️  DELETE DUPLICATES", 563, 22, 165, Color.FromArgb(239, 68, 68), Color.White);
            btnDeleteDuplicates.Click += (s, e) => DeleteDuplicates();

            btnQuarantineClass = CreateActionButton("🛡️  QUARANTINE CLASS", 734, 22, 160, Color.FromArgb(147, 51, 234), Color.White);
            btnQuarantineClass.Click += (s, e) => QuarantineDuplicates();

            btnExportCsv = CreateActionButton("📊  EXPORT CSV", 900, 22, 120, Color.FromArgb(15, 118, 110), Color.White);
            btnExportCsv.Click += (s, e) => ExportToCsv();

            btnReconnect = new Button
            {
                Text = "🔄",
                Location = new Point(1026, 22),
                Size = new Size(38, 38),
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
            grpActions.Controls.Add(btnDeleteDuplicates);
            grpActions.Controls.Add(btnQuarantineClass);
            grpActions.Controls.Add(btnExportCsv);
            grpActions.Controls.Add(btnReconnect);

            // =========================================================================
            // 5. Search & Filter Bar
            // =========================================================================
            pnlFilter = new Panel
            {
                Location = new Point(14, 285),
                Size = new Size(1076, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkSelectAll = new CheckBox
            {
                Text = "Select All",
                Location = new Point(4, 8),
                AutoSize = true,
                Checked = true,
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
                Text = "🔍 Search:",
                Location = new Point(110, 9),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            txtSearch = new TextBox
            {
                Location = new Point(180, 6),
                Width = 220,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            Label lblTypeFilter = new Label
            {
                Text = "Type:",
                Location = new Point(415, 9),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            cboFilterType = new ComboBox
            {
                Location = new Point(460, 6),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterType.Items.AddRange(new object[] { "All Object Types", "Rebar Only", "Part (Beam/Col/Slab)" });
            cboFilterType.SelectedIndex = 0;
            cboFilterType.SelectedIndexChanged += (s, e) => ApplyFilter();

            cboFilterOverlap = new ComboBox
            {
                Location = new Point(610, 6),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            cboFilterOverlap.Items.AddRange(new object[] { "All Overlap Modes", "100% Duplicate", "Volume Overlap" });
            cboFilterOverlap.SelectedIndex = 0;
            cboFilterOverlap.SelectedIndexChanged += (s, e) => ApplyFilter();

            lblFilterSummary = new Label
            {
                Text = "No scan performed yet.",
                Location = new Point(780, 9),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            pnlFilter.Controls.Add(chkSelectAll);
            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(lblTypeFilter);
            pnlFilter.Controls.Add(cboFilterType);
            pnlFilter.Controls.Add(cboFilterOverlap);
            pnlFilter.Controls.Add(lblFilterSummary);

            // =========================================================================
            // 6. Split Container: Results List & Side-by-Side Inspector
            // =========================================================================
            splitResults = new SplitContainer
            {
                Location = new Point(14, 325),
                Size = new Size(1076, 420),
                Orientation = Orientation.Horizontal,
                SplitterDistance = 270,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Main Results List
            lstResults = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            lstResults.Columns.Add("Duplicate ID (Delete)", 140);
            lstResults.Columns.Add("Kept ID (Original)", 130);
            lstResults.Columns.Add("Object Type", 110);
            lstResults.Columns.Add("Name", 120);
            lstResults.Columns.Add("Profile", 110);
            lstResults.Columns.Add("Mark Diff", 90);
            lstResults.Columns.Add("Center (X, Y, Z)", 140);
            lstResults.Columns.Add("Overlap Mode", 110);
            lstResults.Columns.Add("Retention Reason", 200);

            lstResults.SelectedIndexChanged += LstResults_SelectedIndexChanged;
            lstResults.DoubleClick += LstResults_DoubleClick;

            splitResults.Panel1.Controls.Add(lstResults);

            // Inspector Panel
            grpInspector = new GroupBox
            {
                Text = " 🔍 SIDE-BY-SIDE PROPERTY INSPECTOR (COMPARE KEPT VS DUPLICATE) ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            btnSwapSelected = new Button
            {
                Text = "🔄  SWAP KEPT ⮂ DUPLICATE",
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
                Text = "💡 Select any duplicate item above to inspect detailed differences. Click [SWAP] if you wish to keep the other object instead.",
                Location = new Point(235, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            lstInspector = new ListView
            {
                Location = new Point(12, 58),
                Size = new Size(1050, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };

            lstInspector.Columns.Add("Property", 140);
            lstInspector.Columns.Add("✅ ORIGINAL (TO BE KEPT)", 430);
            lstInspector.Columns.Add("❌ DUPLICATE (TO BE REMOVED)", 430);

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
                    Text = $"Tekla Overlap & Duplicate Checker Pro ({modelName}) - [My-tool]";
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

        #region High-Performance Scanning Engine (Spatial Grid)

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
                    lblStatusCount.Text = "👉 Pick window area in Tekla 3D view (Middle-click to finish)...";
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

            if (list.Count < 2)
            {
                MessageBox.Show(
                    "At least 2 objects are required to check for duplicates!",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Prepare UI state for scanning
            btnPickObjects.Enabled = false;
            btnFromSelection.Enabled = false;
            btnCancelScan.Enabled = true;
            prgScan.Visible = true;
            prgScan.Value = 0;
            lblStatusCount.Text = $"Analyzing {list.Count} objects using Spatial Grid Acceleration...";

            RetentionMode retMode = RetentionMode.SmartNumberedFirst;
            if (cboRetentionRule.SelectedIndex == 1) retMode = RetentionMode.OlderObjectFirst;
            else if (cboRetentionRule.SelectedIndex == 2) retMode = RetentionMode.NewerObjectFirst;

            ScanWorkerArgs args = new ScanWorkerArgs
            {
                Objects = list,
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

            // Phase 1: Feature Extraction
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

                if (i % 200 == 0 || i == total - 1)
                {
                    int pct = (int)((i + 1) * 30.0 / total);
                    _bgWorker.ReportProgress(pct, $"Extracting geometry {i + 1}/{total}...");
                }
            }

            // Phase 2: Spatial Grid Partitioning (O(N log N))
            double cellSize = 1200.0; // 1.2m default cell
            var grid = new Dictionary<long, List<ObjectGeoData>>();

            long HashCell(int cx, int cy, int cz)
            {
                unchecked
                {
                    long h = 17;
                    h = h * 31 + cx;
                    h = h * 31 + cy;
                    h = h * 31 + cz;
                    return h;
                }
            }

            for (int i = 0; i < geoList.Count; i++)
            {
                var g = geoList[i];
                int minX = (int)Math.Floor(g.Min.X / cellSize);
                int maxX = (int)Math.Floor(g.Max.X / cellSize);
                int minY = (int)Math.Floor(g.Min.Y / cellSize);
                int maxY = (int)Math.Floor(g.Max.Y / cellSize);
                int minZ = (int)Math.Floor(g.Min.Z / cellSize);
                int maxZ = (int)Math.Floor(g.Max.Z / cellSize);

                // Add to overlapping cells
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            long key = HashCell(x, y, z);
                            if (!grid.TryGetValue(key, out var cellBucket))
                            {
                                cellBucket = new List<ObjectGeoData>();
                                grid[key] = cellBucket;
                            }
                            cellBucket.Add(g);
                        }
                    }
                }
            }

            // Phase 3: Candidate Collision Checking
            List<OverlapItem> results = new List<OverlapItem>();
            HashSet<int> duplicateIds = new HashSet<int>();
            HashSet<long> testedPairs = new HashSet<long>();

            long PairKey(int idA, int idB)
            {
                int min = Math.Min(idA, idB);
                int max = Math.Max(idA, idB);
                return ((long)min << 32) | (uint)max;
            }

            int processedCount = 0;
            int totalGeos = geoList.Count;

            foreach (var g in geoList)
            {
                if (_bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                processedCount++;
                if (processedCount % 100 == 0 || processedCount == totalGeos)
                {
                    int pct = 30 + (int)(processedCount * 70.0 / totalGeos);
                    _bgWorker.ReportProgress(pct, $"Checking collisions {processedCount}/{totalGeos}...");
                }

                if (duplicateIds.Contains(g.Id)) continue;

                // Lookup cells
                int minX = (int)Math.Floor(g.Min.X / cellSize);
                int maxX = (int)Math.Floor(g.Max.X / cellSize);
                int minY = (int)Math.Floor(g.Min.Y / cellSize);
                int maxY = (int)Math.Floor(g.Max.Y / cellSize);
                int minZ = (int)Math.Floor(g.Min.Z / cellSize);
                int maxZ = (int)Math.Floor(g.Max.Z / cellSize);

                HashSet<ObjectGeoData> candidates = new HashSet<ObjectGeoData>();
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            long key = HashCell(x, y, z);
                            if (grid.TryGetValue(key, out var bucket))
                            {
                                foreach (var other in bucket)
                                {
                                    if (other.Id != g.Id) candidates.Add(other);
                                }
                            }
                        }
                    }
                }

                foreach (var other in candidates)
                {
                    if (duplicateIds.Contains(other.Id)) continue;

                    long pKey = PairKey(g.Id, other.Id);
                    if (testedPairs.Contains(pKey)) continue;
                    testedPairs.Add(pKey);

                    if (CheckIsOverlap(g, other, tolerance, exactOnly))
                    {
                        // Decide which object to keep and which to delete based on Smart Retention
                        ObjectGeoData kept;
                        ObjectGeoData dup;
                        string reason;

                        DecideRetention(g, other, retention, out kept, out dup, out reason);

                        duplicateIds.Add(dup.Id);

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
                            CenterPoint = dup.Center,
                            CenterStr = $"{dup.Center.X:0}, {dup.Center.Y:0}, {dup.Center.Z:0}",
                            OverlapType = exactOnly ? "100% Duplicate" : "Volume Overlap",
                            RetentionReason = reason
                        };

                        results.Add(item);
                        break;
                    }
                }
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

            lblStatScanned.Text = $"📦 Scanned: {totalScanned}";
            lblStatDuplicates.Text = $"🔴 Duplicates: {_allOverlapList.Count}";
            lblStatRebars.Text = $"🔩 Rebars: {rebarsCount}";
            lblStatParts.Text = $"🏗️ Parts: {partsCount}";

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

        #region Smart Retention & Overlap Algorithms

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
                    if (!a.HasMark && b.HasMark)
                    {
                        kept = b;
                        dup = a;
                        reason = $"Kept ID {b.Id} (Has Mark: {b.PartMark})";
                        return;
                    }
                    // If both or neither has mark, fall back to older ID
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

        private bool CheckIsOverlap(ObjectGeoData a, ObjectGeoData b, double tol, bool exactOnly)
        {
            double dist = Distance(a.Center, b.Center);
            if (dist > a.Radius + b.Radius + tol)
            {
                return false;
            }

            // Quick type compatibility check
            bool aIsRebar = a.TypeName.Contains("Rebar") || a.TypeName.Contains("Reinforcement");
            bool bIsRebar = b.TypeName.Contains("Rebar") || b.TypeName.Contains("Reinforcement");
            if (aIsRebar != bIsRebar) return false;

            if (exactOnly)
            {
                if (a.TypeName != b.TypeName) return false;
                if (dist > tol) return false;

                double diffX = Math.Abs(a.SizeX - b.SizeX);
                double diffY = Math.Abs(a.SizeY - b.SizeY);
                double diffZ = Math.Abs(a.SizeZ - b.SizeZ);
                if (diffX > tol || diffY > tol || diffZ > tol)
                {
                    return false;
                }

                // If Beam-like: compare start & end points
                if (a.Obj is Beam beamA && b.Obj is Beam beamB)
                {
                    double d1 = Distance(beamA.StartPoint, beamB.StartPoint);
                    double d2 = Distance(beamA.EndPoint, beamB.EndPoint);
                    double d3 = Distance(beamA.StartPoint, beamB.EndPoint);
                    double d4 = Distance(beamA.EndPoint, beamB.StartPoint);

                    bool sameDir = d1 <= tol && d2 <= tol;
                    bool revDir = d3 <= tol && d4 <= tol;
                    if (!sameDir && !revDir)
                    {
                        return false;
                    }
                }

                // If Rebar: compare centerline polygon points
                if (aIsRebar && a.RebarPoints != null && b.RebarPoints != null)
                {
                    if (a.RebarPoints.Count != b.RebarPoints.Count) return false;
                    for (int i = 0; i < a.RebarPoints.Count; i++)
                    {
                        if (Distance(a.RebarPoints[i], b.RebarPoints[i]) > tol)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // Volume Clash Mode (AABB intersection with radius limit)
            bool overlapX = a.Min.X <= b.Max.X + tol && a.Max.X >= b.Min.X - tol;
            bool overlapY = a.Min.Y <= b.Max.Y + tol && a.Max.Y >= b.Min.Y - tol;
            bool overlapZ = a.Min.Z <= b.Max.Z + tol && a.Max.Z >= b.Min.Z - tol;

            if (overlapX && overlapY && overlapZ)
            {
                return dist <= Math.Max(a.Radius, b.Radius);
            }

            return false;
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

                if (obj is Part part)
                {
                    name = part.Name;
                    profile = part.Profile.ProfileString;
                    material = part.Material.MaterialString;
                    classStr = part.Class;
                    Solid solid = part.GetSolid();
                    if (solid != null)
                    {
                        minPt = solid.MinimumPoint;
                        maxPt = solid.MaximumPoint;
                    }
                }
                else if (obj is Reinforcement rebar)
                {
                    name = rebar.Name;
                    material = rebar.Grade;
                    classStr = rebar.Class.ToString();

                    string size = "";
                    rebar.GetReportProperty("SIZE", ref size);
                    profile = rebar.Grade + " d" + size;

                    Solid solid = rebar.GetSolid();
                    if (solid != null)
                    {
                        minPt = solid.MinimumPoint;
                        maxPt = solid.MaximumPoint;
                    }

                    // Extract rebar polygon centerline points
                    try
                    {
                        var rebarGeos = rebar.GetRebarGeometries(true);
                        if (rebarGeos != null && rebarGeos.Count > 0 && rebarGeos[0] is RebarGeometry rGeo && rGeo.Shape != null)
                        {
                            rebarPts = new List<TeklaPoint>();
                            foreach (var pt in rGeo.Shape.Points)
                            {
                                if (pt is TeklaPoint p) rebarPts.Add(p);
                            }
                        }
                    }
                    catch { }
                }

                if (minPt == null || maxPt == null) return null;

                // Extract standard report properties
                try
                {
                    obj.GetReportProperty("PHASE", ref phase);
                    obj.GetReportProperty("PART_POS", ref partPos);
                    obj.GetReportProperty("ASSEMBLY_POS", ref assPos);
                    obj.GetReportProperty("LENGTH", ref length);
                    obj.GetReportProperty("VOLUME", ref volume);
                }
                catch { }

                bool hasMark = !string.IsNullOrWhiteSpace(partPos) && partPos != "?" && partPos != "0";

                TeklaPoint center = new TeklaPoint(
                    (minPt.X + maxPt.X) / 2.0,
                    (minPt.Y + maxPt.Y) / 2.0,
                    (minPt.Z + maxPt.Z) / 2.0);

                double szX = Math.Abs(maxPt.X - minPt.X);
                double szY = Math.Abs(maxPt.Y - minPt.Y);
                double szZ = Math.Abs(maxPt.Z - minPt.Z);
                double radius = Math.Sqrt(szX * szX + szY * szY + szZ * szZ) / 2.0;

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

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLowerInvariant();
            int typeFilter = cboFilterType.SelectedIndex;
            int overlapFilter = cboFilterOverlap.SelectedIndex;

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

                string markStatus = (item.OriginalMark == item.DuplicateMark) ? "Same" : $"{item.OriginalMark} vs {item.DuplicateMark}";
                lvi.SubItems.Add(markStatus);
                lvi.SubItems.Add(item.CenterStr);
                lvi.SubItems.Add(item.OverlapType);
                lvi.SubItems.Add(item.RetentionReason);

                lvi.ForeColor = Color.FromArgb(185, 28, 28);
                lstResults.Items.Add(lvi);
            }
            lstResults.EndUpdate();

            lblFilterSummary.Text = $"Showing: {_displayedOverlapList.Count} / {_allOverlapList.Count} duplicates";
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
        }

        private void PopulateInspector(OverlapItem item)
        {
            lstInspector.BeginUpdate();
            lstInspector.Items.Clear();

            void AddRow(string prop, string origVal, string dupVal)
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

            AddRow("ID", item.OriginalId.ToString(), item.DuplicateId.ToString());
            AddRow("GUID", item.OriginalGuid, item.DuplicateGuid);
            AddRow("Name", item.OriginalName, item.DuplicateName);
            AddRow("Profile", item.OriginalProfile, item.DuplicateProfile);
            AddRow("Material", item.OriginalMaterial, item.DuplicateMaterial);
            AddRow("Class", item.OriginalClass, item.DuplicateClass);
            AddRow("Phase", item.OriginalPhase.ToString(), item.DuplicatePhase.ToString());
            AddRow("Part Mark", item.OriginalMark, item.DuplicateMark);
            AddRow("Length (mm)", $"{item.OriginalLength:0.0}", $"{item.DuplicateLength:0.0}");
            AddRow("Retention Logic", item.RetentionReason, "Marked for Deletion");

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
            lvi.SubItems[8].Text = item.RetentionReason;

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

                global::Tekla.Structures.Model.Operations.Operation.DisplayPrompt(
                    $"Selected Duplicate ID {item.DuplicateId} and Kept ID {item.OriginalId} in Tekla.");
            }
        }

        #endregion

        #region Actions: Highlight, Delete, Quarantine, Export

        private void HighlightDuplicatesInTekla()
        {
            if (_allOverlapList.Count == 0) return;

            ArrayList listToHighlight = new ArrayList();
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                if (lstResults.Items[i].Checked && i < _displayedOverlapList.Count)
                {
                    var dup = _displayedOverlapList[i].DuplicateObject;
                    if (dup != null) listToHighlight.Add(dup);
                }
            }

            if (listToHighlight.Count == 0)
            {
                MessageBox.Show("No items are checked to highlight!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Highlight error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteDuplicates()
        {
            List<OverlapItem> toDelete = new List<OverlapItem>();
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                if (lstResults.Items[i].Checked && i < _displayedOverlapList.Count)
                {
                    toDelete.Add(_displayedOverlapList[i]);
                }
            }

            if (toDelete.Count == 0)
            {
                MessageBox.Show("Please check at least one duplicate object in the list to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to permanently delete {toDelete.Count} duplicate object(s) from Tekla Structures?\n\n" +
                $"✅ All corresponding original/kept objects will be preserved.\n" +
                $"💡 Tip: You can also use [QUARANTINE CLASS] to isolate them before deleting.",
                "Confirm Delete Duplicates",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

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

            _model.CommitChanges();
            MessageBox.Show(
                $"Successfully deleted {deletedCount} duplicate object(s) from Tekla model!",
                "Delete Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ApplyFilter();
            lblStatusCount.Text = $"Deleted {deletedCount} duplicate object(s).";
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
                        part.Class = "99"; // Class 99 is standard quarantine/gray
                        part.Modify();
                        changedCount++;
                    }
                    else if (item.DuplicateObject is Reinforcement rebar)
                    {
                        rebar.Class = 99;
                        rebar.Modify();
                        changedCount++;
                    }
                }
                catch { }
            }

            _model.CommitChanges();
            MessageBox.Show(
                $"Quarantined {changedCount} duplicate object(s) to Class 99!\n\nYou can now inspect them visually in 3D.",
                "Quarantine Complete",
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
                sb.AppendLine("DuplicateId,KeptId,TypeName,Name,Profile,Material,OriginalMark,DuplicateMark,Center_X,Center_Y,Center_Z,OverlapType,RetentionReason");

                foreach (var item in _allOverlapList)
                {
                    sb.AppendLine(
                        $"{item.DuplicateId},{item.OriginalId},\"{item.TypeName}\",\"{item.DuplicateName}\",\"{item.DuplicateProfile}\"," +
                        $"\"{item.DuplicateMaterial}\",\"{item.OriginalMark}\",\"{item.DuplicateMark}\"," +
                        $"{item.CenterPoint.X:0.0},{item.CenterPoint.Y:0.0},{item.CenterPoint.Z:0.0}," +
                        $"\"{item.OverlapType}\",\"{item.RetentionReason}\"");
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Exported {_allOverlapList.Count} records to:\n{sfd.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private double Distance(TeklaPoint a, TeklaPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        #endregion
    }
}
