using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Point = Tekla.Structures.Geometry3d.Point;

namespace BimCommands.Tekla.MyTool
{
    public class ChangeShapeDialog : Form
    {
        private readonly Model _model;
        private readonly object _selectedRebarsLock = new object();
        private List<RebarShapeInfo> _selectedRebars = new List<RebarShapeInfo>();
        private readonly Stack<List<RebarUndoRecord>> _undoStack = new Stack<List<RebarUndoRecord>>();
        private List<ShapeDefinition> _availableShapes = new List<ShapeDefinition>();
        private ShapeDefinition _currentShapeDef;
        private bool _isUpdatingUi = false;

        private List<RebarShapeInfo> GetSelectedRebarsSnapshot()
        {
            lock (_selectedRebarsLock)
            {
                return new List<RebarShapeInfo>(_selectedRebars);
            }
        }

        private int GetSelectedRebarCount()
        {
            lock (_selectedRebarsLock)
            {
                return _selectedRebars.Count;
            }
        }

        private RebarShapeInfo GetFirstSelectedRebar()
        {
            lock (_selectedRebarsLock)
            {
                return _selectedRebars.Count > 0 ? _selectedRebars[0] : null;
            }
        }

        // UI Controls
        private Label lblStatus;
        private GroupBox grpSelect;
        private Button btnGetSelected;
        private Button btnPickIn3D;
        private Label lblRebarDetails;

        private GroupBox grpShape;
        private Label lblShapeCode;
        private TextBox txtShapeNo;
        private ComboBox cboShapeType;
        private Button btnScanModelShapes;

        private Label lblA;
        private NumericUpDown numA;
        private Label lblB;
        private NumericUpDown numB;
        private Label lblC;
        private NumericUpDown numC;
        private Label lblD;
        private NumericUpDown numD;
        private Button btnAutoHook;

        private Label lblPlane;
        private ComboBox cboPlaneMode;
        private Label lblRotate;
        private NumericUpDown numRotation;
        private Button btnRotate90;
        private CheckBox chkFlip;
        private Label lblCorner;
        private ComboBox cboHookCorner;

        private Label lblCover;
        private NumericUpDown numCover;
        private Button btnAutoConcrete;

        private GroupBox grpPreview;
        private Panel pnlCanvas;
        private Label lblFormula;

        private GroupBox grpAction;
        private Button btnApply;
        private Button btnBatchApply;
        private Button btnUndo;

        private TextBox txtLog;

        public ChangeShapeDialog(Model model)
        {
            _model = model ?? new Model();
            InitializeComponent();
            RebarShapeModifier.LogHandler += LogFromOtherThreads;
            HdcRebarShapeRulesManager.LogHandler += LogFromOtherThreads;
            DwgShapeCatalogManager.LogHandler += LogFromOtherThreads;
            LoadShapeCatalog();
            CheckTeklaConnection();
        }

        private void InitializeComponent()
        {
            this.Text = "Change-Shape Studio - Tekla Rebar Shape Modifier";
            this.Size = new Size(640, 920);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.FromArgb(248, 249, 250);

            int pad = 12;
            int w = this.ClientSize.Width - 2 * pad;
            int curY = pad;

            // Header Status Banner
            lblStatus = new Label
            {
                Location = new System.Drawing.Point(pad, curY),
                Size = new Size(w, 28),
                BackColor = Color.FromArgb(220, 252, 231),
                ForeColor = Color.FromArgb(22, 101, 52),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Text = "Tekla Structures: Checking connection..."
            };
            this.Controls.Add(lblStatus);
            curY += 34;

            // SECTION 1: SELECTION
            grpSelect = new GroupBox
            {
                Text = "1. Select Reinforcement to Modify",
                Location = new System.Drawing.Point(pad, curY),
                Size = new Size(w, 95),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            btnGetSelected = new Button
            {
                Text = "Get Selected Rebars",
                Location = new System.Drawing.Point(12, 24),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(238, 242, 255),
                ForeColor = Color.FromArgb(67, 56, 202),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGetSelected.Click += BtnGetSelected_Click;
            grpSelect.Controls.Add(btnGetSelected);

            btnPickIn3D = new Button
            {
                Text = "Pick on 3D View",
                Location = new System.Drawing.Point(180, 24),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(254, 243, 199),
                ForeColor = Color.FromArgb(146, 64, 14),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPickIn3D.Click += BtnPickIn3D_Click;
            grpSelect.Controls.Add(btnPickIn3D);

            lblRebarDetails = new Label
            {
                Text = "No rebar selected. Select rebar in Tekla model or click Pick.",
                Location = new System.Drawing.Point(12, 64),
                Size = new Size(w - 24, 22),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 85, 99)
            };
            grpSelect.Controls.Add(lblRebarDetails);
            this.Controls.Add(grpSelect);
            curY += 102;

            // SECTION 2: SHAPE & DIMENSIONS
            grpShape = new GroupBox
            {
                Text = "2. Target Shape & Dimensions (mm)",
                Location = new System.Drawing.Point(pad, curY),
                Size = new Size(w, 235),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            lblShapeCode = new Label
            {
                Text = "Shape Code / No:",
                Location = new System.Drawing.Point(12, 26),
                Size = new Size(115, 22),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            grpShape.Controls.Add(lblShapeCode);

            txtShapeNo = new TextBox
            {
                Location = new System.Drawing.Point(130, 23),
                Size = new Size(68, 26),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                Text = "56"
            };
            txtShapeNo.TextChanged += TxtShapeNo_TextChanged;
            txtShapeNo.KeyDown += TxtShapeNo_KeyDown;
            grpShape.Controls.Add(txtShapeNo);

            cboShapeType = new ComboBox
            {
                Location = new System.Drawing.Point(204, 23),
                Size = new Size(256, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            cboShapeType.SelectedIndexChanged += CboShapeType_SelectedIndexChanged;
            grpShape.Controls.Add(cboShapeType);

            btnScanModelShapes = new Button
            {
                Text = "Scan Model",
                Location = new System.Drawing.Point(468, 22),
                Size = new Size(120, 27),
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnScanModelShapes.Click += (s, e) => { RebarShapeModifier.InvalidateCatalogCache(); LoadShapeCatalog(); Log("Scanned all available shapes from Tekla model catalog."); };
            grpShape.Controls.Add(btnScanModelShapes);

            // Row 1 Dimensions: A & B
            lblA = new Label { Text = "A (Width mm):", Location = new System.Drawing.Point(12, 60), Size = new Size(100, 20), Font = new Font("Segoe UI", 8.5F) };
            numA = new NumericUpDown { Location = new System.Drawing.Point(115, 58), Size = new Size(80, 22), Minimum = 10, Maximum = 50000, Value = 600, Increment = 10 };
            numA.ValueChanged += Param_ValueChanged;
            grpShape.Controls.Add(lblA);
            grpShape.Controls.Add(numA);

            lblB = new Label { Text = "B (Height mm):", Location = new System.Drawing.Point(210, 60), Size = new Size(100, 20), Font = new Font("Segoe UI", 8.5F) };
            numB = new NumericUpDown { Location = new System.Drawing.Point(310, 58), Size = new Size(80, 22), Minimum = 0, Maximum = 50000, Value = 400, Increment = 10 };
            numB.ValueChanged += Param_ValueChanged;
            grpShape.Controls.Add(lblB);
            grpShape.Controls.Add(numB);

            // Row 2 Dimensions: C & D
            lblC = new Label { Text = "C (Hook mm):", Location = new System.Drawing.Point(12, 92), Size = new Size(100, 20), Font = new Font("Segoe UI", 8.5F) };
            numC = new NumericUpDown { Location = new System.Drawing.Point(115, 90), Size = new Size(80, 22), Minimum = 0, Maximum = 10000, Value = 120, Increment = 5 };
            numC.ValueChanged += Param_ValueChanged;
            grpShape.Controls.Add(lblC);
            grpShape.Controls.Add(numC);

            btnAutoHook = new Button
            {
                Text = "10d",
                Location = new System.Drawing.Point(200, 89),
                Size = new Size(42, 24),
                BackColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            btnAutoHook.Click += BtnAutoHook_Click;
            grpShape.Controls.Add(btnAutoHook);

            lblD = new Label { Text = "D (Ext mm):", Location = new System.Drawing.Point(255, 92), Size = new Size(80, 20), Font = new Font("Segoe UI", 8.5F) };
            numD = new NumericUpDown { Location = new System.Drawing.Point(340, 90), Size = new Size(75, 22), Minimum = 0, Maximum = 10000, Value = 100, Increment = 5 };
            numD.ValueChanged += Param_ValueChanged;
            grpShape.Controls.Add(lblD);
            grpShape.Controls.Add(numD);

            // Row 3: Plane Orientation, Rotate Angle, Flip & Hook Corner
            lblPlane = new Label { Text = "Bending Plane:", Location = new System.Drawing.Point(12, 126), Size = new Size(82, 20), Font = new Font("Segoe UI", 8.5F) };
            cboPlaneMode = new ComboBox
            {
                Location = new System.Drawing.Point(95, 123),
                Size = new Size(130, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5F)
            };
            cboPlaneMode.Items.AddRange(new object[] {
                "Auto Perpendicular",
                "Horizontal (XY Plane)",
                "Vertical Beam (XZ)",
                "Vertical Column (YZ)"
            });
            cboPlaneMode.SelectedIndex = 0;
            grpShape.Controls.Add(lblPlane);
            grpShape.Controls.Add(cboPlaneMode);

            lblRotate = new Label { Text = "Rotate (°):", Location = new System.Drawing.Point(232, 126), Size = new Size(58, 20), Font = new Font("Segoe UI", 8.5F) };
            numRotation = new NumericUpDown
            {
                Location = new System.Drawing.Point(292, 123),
                Size = new Size(55, 22),
                Minimum = -360,
                Maximum = 360,
                Value = 0,
                Increment = 90,
                DecimalPlaces = 0,
                Font = new Font("Segoe UI", 8.5F)
            };
            numRotation.ValueChanged += (s, e) => { pnlCanvas.Invalidate(); };
            grpShape.Controls.Add(lblRotate);
            grpShape.Controls.Add(numRotation);

            btnRotate90 = new Button
            {
                Text = "⟳ 90°",
                Location = new System.Drawing.Point(350, 122),
                Size = new Size(46, 24),
                BackColor = Color.FromArgb(238, 242, 255),
                ForeColor = Color.FromArgb(67, 56, 202),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRotate90.Click += (s, e) =>
            {
                decimal current = numRotation.Value;
                decimal next = ((current + 90) % 360 + 360) % 360;
                numRotation.Value = next;
            };
            grpShape.Controls.Add(btnRotate90);

            chkFlip = new CheckBox
            {
                Text = "Flip 180°",
                Location = new System.Drawing.Point(402, 124),
                Size = new Size(80, 22),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Cursor = Cursors.Hand
            };
            chkFlip.CheckedChanged += (s, e) => { pnlCanvas.Invalidate(); };
            grpShape.Controls.Add(chkFlip);

            lblCorner = new Label { Text = "Hook:", Location = new System.Drawing.Point(488, 126), Size = new Size(40, 20), Font = new Font("Segoe UI", 8.5F) };
            cboHookCorner = new ComboBox
            {
                Location = new System.Drawing.Point(530, 123),
                Size = new Size(68, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5F)
            };
            cboHookCorner.Items.AddRange(new object[] {
                "TR",
                "BR",
                "BL",
                "TL"
            });
            cboHookCorner.SelectedIndex = 0;
            cboHookCorner.SelectedIndexChanged += (s, e) => pnlCanvas.Invalidate();
            grpShape.Controls.Add(lblCorner);
            grpShape.Controls.Add(cboHookCorner);

            // Sub-Panel: Host concrete auto-fit
            var pnlHost = new Panel
            {
                Location = new System.Drawing.Point(12, 158),
                Size = new Size(w - 24, 65),
                BackColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblHostTitle = new Label
            {
                Text = "Auto-Fit Beam / Column Concrete Section",
                Location = new System.Drawing.Point(8, 6),
                Size = new Size(300, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            pnlHost.Controls.Add(lblHostTitle);

            lblCover = new Label { Text = "Cover (mm):", Location = new System.Drawing.Point(8, 30), Size = new Size(75, 20), Font = new Font("Segoe UI", 8.5F) };
            numCover = new NumericUpDown { Location = new System.Drawing.Point(85, 28), Size = new Size(55, 22), Minimum = 10, Maximum = 200, Value = 30 };
            pnlHost.Controls.Add(lblCover);
            pnlHost.Controls.Add(numCover);

            btnAutoConcrete = new Button
            {
                Text = "Auto-Fit Host Dimensions",
                Location = new System.Drawing.Point(150, 27),
                Size = new Size(180, 26),
                BackColor = Color.FromArgb(220, 252, 231),
                ForeColor = Color.FromArgb(22, 101, 52),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAutoConcrete.Click += BtnAutoConcrete_Click;
            pnlHost.Controls.Add(btnAutoConcrete);

            grpShape.Controls.Add(pnlHost);
            this.Controls.Add(grpShape);
            curY += 245;

            // SECTION 3: 2D LIVE PREVIEW
            grpPreview = new GroupBox
            {
                Text = "3. 2D Real-time Geometry Preview",
                Location = new System.Drawing.Point(pad, curY),
                Size = new Size(w, 180),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            pnlCanvas = new Panel
            {
                Location = new System.Drawing.Point(12, 22),
                Size = new Size(w - 24, 125),
                BackColor = Color.FromArgb(26, 27, 38)
            };
            pnlCanvas.Paint += PnlCanvas_Paint;
            grpPreview.Controls.Add(pnlCanvas);

            lblFormula = new Label
            {
                Text = "L = 2*(A + B) + 2*C",
                Location = new System.Drawing.Point(12, 152),
                Size = new Size(w - 24, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235)
            };
            grpPreview.Controls.Add(lblFormula);
            this.Controls.Add(grpPreview);
            curY += 190;

            // SECTION 4: ACTIONS
            grpAction = new GroupBox
            {
                Text = "4. Apply Changes to Tekla Model",
                Location = new System.Drawing.Point(pad, curY),
                Size = new Size(w, 75),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            btnApply = new Button
            {
                Text = "APPLY SHAPE CHANGE",
                Location = new System.Drawing.Point(12, 22),
                Size = new Size(200, 42),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApply.Click += BtnApply_Click;
            grpAction.Controls.Add(btnApply);

            btnBatchApply = new Button
            {
                Text = "BATCH APPLY (MULTI-SELECT)",
                Location = new System.Drawing.Point(220, 22),
                Size = new Size(230, 42),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchApply.Click += BtnBatchApply_Click;
            grpAction.Controls.Add(btnBatchApply);

            btnUndo = new Button
            {
                Text = "UNDO",
                Location = new System.Drawing.Point(460, 22),
                Size = new Size(125, 42),
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(75, 85, 99),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUndo.Click += BtnUndo_Click;
            grpAction.Controls.Add(btnUndo);

            this.Controls.Add(grpAction);
            curY += 85;

            // SECTION 5: ACTIVITY LOG
            txtLog = new TextBox
            {
                Location = new System.Drawing.Point(pad, curY),
                Size = new Size(w, 130),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 8.5F),
                ForeColor = Color.FromArgb(55, 65, 81)
            };
            this.Controls.Add(txtLog);

            Log("Change-Shape Studio initialized successfully.");
        }

        private void CheckTeklaConnection()
        {
            try
            {
                if (_model.GetConnectionStatus())
                {
                    var info = _model.GetInfo();
                    lblStatus.Text = $"Tekla Structures Connected: {info.ModelName}";
                    lblStatus.BackColor = Color.FromArgb(220, 252, 231);
                    lblStatus.ForeColor = Color.FromArgb(22, 101, 52);
                    Log($"Connected to Tekla Model: {info.ModelName}");
                }
                else
                {
                    lblStatus.Text = "Tekla Structures: Not Connected. Please open a model.";
                    lblStatus.BackColor = Color.FromArgb(254, 226, 226);
                    lblStatus.ForeColor = Color.FromArgb(153, 27, 27);
                    Log("Warning: Tekla model not connected.");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Tekla Structures: Connection error.";
                Log("Connection error: " + ex.Message);
            }
        }

        private void LoadShapeCatalog()
        {
            _availableShapes = RebarShapeModifier.GetAllAvailableShapes(_model);
            cboShapeType.Items.Clear();

            foreach (var shape in _availableShapes)
            {
                cboShapeType.Items.Add(shape);
            }

            string xmlPath = HdcRebarShapeRulesManager.LoadedXmlPath;
            if (!string.IsNullOrEmpty(xmlPath))
            {
                Log($"Loaded {cboShapeType.Items.Count} shapes from HDC Rules: {System.IO.Path.GetFileName(xmlPath)}");
                Log($"Rules source: {xmlPath}");
            }
            else
            {
                Log($"Loaded {cboShapeType.Items.Count} shapes from built-in and catalog.");
            }

            string dwgSource = DwgShapeCatalogManager.LoadedSourcePath;
            if (!string.IsNullOrEmpty(dwgSource))
            {
                Log($"STShape DWG Blocks synced: {System.IO.Path.GetFileName(dwgSource)}");
            }

            // Default to Shape 56
            var def56 = _availableShapes.FirstOrDefault(s => s.ShapeCode == "56") ?? _availableShapes.FirstOrDefault();
            if (def56 != null)
            {
                cboShapeType.SelectedItem = def56;
                SetCurrentShape(def56);
            }
        }

        private void CboShapeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            if (cboShapeType.SelectedItem is ShapeDefinition shape)
            {
                SetCurrentShape(shape);
            }
        }

        private void CboShapeType_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            string typed = cboShapeType.Text;
            var match = RebarShapeModifier.GetShapeDefinition(typed, _model);
            if (match != null && match != _currentShapeDef)
            {
                SetCurrentShape(match);
            }
        }

        private void TxtShapeNo_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            string typed = txtShapeNo.Text.Trim();
            if (string.IsNullOrEmpty(typed)) return;

            var match = _availableShapes?.FirstOrDefault(s => s.ShapeCode.Equals(typed, StringComparison.OrdinalIgnoreCase))
                     ?? RebarShapeModifier.GetShapeDefinition(typed, _model);

            if (match != null && match != _currentShapeDef)
            {
                SetCurrentShape(match);
            }
        }

        private void TxtShapeNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string typed = txtShapeNo.Text.Trim();
                if (!string.IsNullOrEmpty(typed))
                {
                    var match = _availableShapes?.FirstOrDefault(s => s.ShapeCode.Equals(typed, StringComparison.OrdinalIgnoreCase))
                             ?? RebarShapeModifier.GetShapeDefinition(typed, _model);
                    if (match != null)
                    {
                        SetCurrentShape(match);
                    }
                }

                if (numA.Enabled)
                {
                    numA.Focus();
                    numA.Select(0, numA.Text.Length);
                }
            }
        }

        private void SetCurrentShape(ShapeDefinition shape)
        {
            if (shape == null) return;
            if (_isUpdatingUi) return;
            try
            {
                _isUpdatingUi = true;
                _currentShapeDef = shape;

                if (cboShapeType.SelectedItem != shape)
                {
                    cboShapeType.SelectedItem = shape;
                }
                if (txtShapeNo != null && txtShapeNo.Text != shape.ShapeCode)
                {
                    txtShapeNo.Text = shape.ShapeCode;
                }

                var hdcRule = HdcRebarShapeRulesManager.GetRule(shape.ShapeCode);

                lblA.Text = shape.LabelA;
                lblB.Text = shape.LabelB;
                lblC.Text = shape.LabelC;
                lblD.Text = shape.LabelD;

                int paramCount = shape.ParamCount;
                if (hdcRule != null && hdcRule.Fields.Count > 0)
                {
                    int validFields = hdcRule.Fields.Keys.Count(k => k == "A" || k == "B" || k == "C" || k == "D");
                    if (validFields > 0)
                    {
                        paramCount = Math.Max(paramCount, Math.Min(4, validFields));
                    }
                }

                string sClean = shape.ShapeCode.Trim();
                if (sClean == "1" || sClean == "102") paramCount = 1;
                else if (sClean == "2") paramCount = 2;
                else if (sClean == "3" || sClean == "4" || sClean == "45") paramCount = 3;

                numA.Enabled = paramCount >= 1;
                numB.Enabled = paramCount >= 2;
                numC.Enabled = paramCount >= 3;
                btnAutoHook.Enabled = (sClean == "56" || sClean == "54" || sClean == "55" || sClean == "11" || sClean == "12" || sClean == "14");
                numD.Enabled = paramCount >= 4;

                // Smart Mapping from selected bar if available, else preserve user input or default
                var orig = GetFirstSelectedRebar();
                if (orig != null)
                {
                    string sCode = shape.ShapeCode;
                    if (sCode == "1" || sCode == "102")
                    {
                        double targetA = orig.OriginalLength > 0 ? orig.OriginalLength : (orig.DimA > 0 ? orig.DimA : (double)numA.Value);
                        numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)targetA));
                    }
                    else if (sCode == "2" || sCode == "12")
                    {
                        if (orig.DimA > 0) numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)orig.DimA));
                        if (orig.DimB > 0) numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)orig.DimB));
                    }
                    else if (sCode == "4" || sCode == "45")
                    {
                        if (orig.DimA > 0) numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)orig.DimA));
                        if (orig.DimB > 0) numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)orig.DimB));
                        if (orig.DimC > 0) numC.Value = Math.Max(numC.Minimum, Math.Min(numC.Maximum, (decimal)orig.DimC));
                    }
                    else if (sCode == "54" || sCode == "55" || sCode == "56" || sCode == "41")
                    {
                        if (orig.BoundingWidth > 0) numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)orig.BoundingWidth));
                        if (orig.BoundingHeight > 0) numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)orig.BoundingHeight));
                        numC.Value = (decimal)Math.Max(100.0, orig.Diameter * 10.0);
                    }
                    else
                    {
                        if (orig.DimA > 0) numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)orig.DimA));
                        if (orig.DimB > 0) numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)orig.DimB));
                        if (orig.DimC > 0) numC.Value = Math.Max(numC.Minimum, Math.Min(numC.Maximum, (decimal)orig.DimC));
                        if (orig.DimD > 0) numD.Value = Math.Max(numD.Minimum, Math.Min(numD.Maximum, (decimal)orig.DimD));
                    }
                }
                else
                {
                    numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)shape.DefaultA));
                    numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)shape.DefaultB));
                    numC.Value = Math.Max(numC.Minimum, Math.Min(numC.Maximum, (decimal)shape.DefaultC));
                    numD.Value = Math.Max(numD.Minimum, Math.Min(numD.Maximum, (decimal)shape.DefaultD));
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }

            UpdatePreview();
        }

        private void Param_ValueChanged(object sender, EventArgs e)
        {
            if (!_isUpdatingUi)
            {
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            pnlCanvas.Invalidate();

            double a = (double)numA.Value;
            double b = (double)numB.Value;
            double c = (double)numC.Value;
            double d = (double)numD.Value;

            string code = _currentShapeDef != null ? _currentShapeDef.ShapeCode : "1";

            // Check if HDC RebarShapeRules rule exists for exact formula text
            var hdcRule = HdcRebarShapeRulesManager.GetRule(code);
            if (hdcRule != null)
            {
                lblFormula.Text = $"{hdcRule.GetCutLengthFormulaText(a, b, c, d)} | {hdcRule.GetFormulaSummary()}";
                return;
            }

            double cutLength = 0;

            if (code == "1" || code == "102")
            {
                cutLength = a;
                lblFormula.Text = $"L = A = {cutLength:N0} mm";
            }
            else if (code == "2" || code == "12")
            {
                cutLength = a + b;
                lblFormula.Text = $"L = A + B = {cutLength:N0} mm";
            }
            else if (code == "3" || code == "4" || code == "45")
            {
                cutLength = a + b + c;
                lblFormula.Text = $"L = A + B + C = {cutLength:N0} mm";
            }
            else if (code == "11")
            {
                cutLength = a + b + c;
                lblFormula.Text = $"L = A + Hook1 + Hook2 = {cutLength:N0} mm";
            }
            else if (code == "14" || code == "15" || code == "21")
            {
                cutLength = a + b + c;
                lblFormula.Text = $"L = A + B + C = {cutLength:N0} mm";
            }
            else if (code == "22" || code == "25")
            {
                cutLength = a + 2.0 * b + 2.0 * c;
                lblFormula.Text = $"L = A + 2*B + 2*C = {cutLength:N0} mm";
            }
            else if (code == "31")
            {
                cutLength = a + b + c;
                lblFormula.Text = $"L = A + B + C = {cutLength:N0} mm";
            }
            else if (code == "41")
            {
                cutLength = 2.0 * (a + b);
                lblFormula.Text = $"L = 2*(A + B) = {cutLength:N0} mm";
            }
            else if (code == "51")
            {
                cutLength = a + 2.0 * b;
                lblFormula.Text = $"L = A + 2*B = {cutLength:N0} mm";
            }
            else if (code == "56" || code == "54" || code == "55")
            {
                cutLength = 2.0 * (a + b) + 2.0 * c;
                lblFormula.Text = $"L = 2*(A + B) + 2*C = 2*({a:0} + {b:0}) + 2*{c:0} = {cutLength:N0} mm";
            }
            else if (code == "60")
            {
                cutLength = 4.0 * Math.Sqrt((a / 2.0) * (a / 2.0) + (b / 2.0) * (b / 2.0));
                lblFormula.Text = $"L = 4 * sqrt((A/2)^2 + (B/2)^2) = {cutLength:N0} mm";
            }
            else if (code == "62")
            {
                cutLength = a + 2.0 * Math.Sqrt((a / 2.0) * (a / 2.0) + b * b);
                lblFormula.Text = $"L = A + 2 * sqrt((A/2)^2 + B^2) = {cutLength:N0} mm";
            }
            else if (code == "67")
            {
                cutLength = Math.PI * a;
                lblFormula.Text = $"L = π * D = {cutLength:N0} mm";
            }
            else if (code == "72")
            {
                cutLength = 2.0 * a + b + 2.0 * c;
                lblFormula.Text = $"L = 2*A + B + 2*C = {cutLength:N0} mm";
            }
            else if (code == "74")
            {
                cutLength = a + 2.0 * b + c + d;
                lblFormula.Text = $"L = A + 2*B + C + D = {cutLength:N0} mm";
            }
            else if (code == "370")
            {
                cutLength = a + 2.0 * b + 2.0 * c;
                lblFormula.Text = $"L = A + 2*B + 2*C = {cutLength:N0} mm";
            }
            else
            {
                cutLength = (b > 1.0 ? 2.0 * (a + b) : a) + (c > 1.0 ? 2.0 * c : 0.0) + (d > 1.0 ? d : 0.0);
                lblFormula.Text = $"Total Length L = {cutLength:N0} mm";
            }
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            string code = _currentShapeDef != null ? _currentShapeDef.ShapeCode : cboShapeType.Text.Trim();
            double a = (double)numA.Value;
            double b = (double)numB.Value;
            double c = (double)numC.Value;
            double d = (double)numD.Value;
            int cornerIdx = cboHookCorner != null ? cboHookCorner.SelectedIndex : 0;
            bool isFlipped = chkFlip != null && chkFlip.Checked;
            double rot = numRotation != null ? (double)numRotation.Value : 0.0;

            ShapePreviewRenderer.Render(
                e.Graphics,
                pnlCanvas.Width,
                pnlCanvas.Height,
                code,
                a,
                b,
                c,
                d,
                cornerIdx,
                isFlipped,
                rot);
        }

        private void BtnGetSelected_Click(object sender, EventArgs e)
        {
            try
            {
                btnGetSelected.Enabled = false;
                btnPickIn3D.Enabled = false;
                var rebars = RebarShapeModifier.GetSelectedReinforcements(_model);
                ProcessSelectedRebars(rebars);
            }
            catch (Exception ex)
            {
                Log($"Error getting selected rebars: {ex.Message}");
            }
            finally
            {
                btnGetSelected.Enabled = true;
                btnPickIn3D.Enabled = true;
            }
        }

        private void BtnPickIn3D_Click(object sender, EventArgs e)
        {
            try
            {
                btnPickIn3D.Enabled = false;
                btnGetSelected.Enabled = false;
                Log("Waiting for you to pick rebar in 3D Tekla model (middle click to finish)...");
                var rebars = RebarShapeModifier.PickReinforcementsFromModel("Pick rebar to change shape, middle click to finish");
                ProcessSelectedRebars(rebars);
            }
            catch (Exception ex)
            {
                Log($"Error picking rebar in 3D: {ex.Message}");
            }
            finally
            {
                btnPickIn3D.Enabled = true;
                btnGetSelected.Enabled = true;
            }
        }

        private void ProcessSelectedRebars(List<Reinforcement> rebars)
        {
            var parsedRebars = new List<RebarShapeInfo>();
            if (rebars != null && rebars.Count > 0)
            {
                foreach (var r in rebars)
                {
                    var info = RebarShapeModifier.AnalyzeRebar(r);
                    if (info != null) parsedRebars.Add(info);
                }
            }

            RebarShapeInfo first;
            int count;
            lock (_selectedRebarsLock)
            {
                _selectedRebars.Clear();
                _selectedRebars.AddRange(parsedRebars);
                count = _selectedRebars.Count;
                first = count > 0 ? _selectedRebars[0] : null;
            }

            if (count == 0)
            {
                lblRebarDetails.Text = "No valid reinforcement found in selection.";
                Log("Selected objects could not be resolved as reinforcement.");
                return;
            }

            string segsText = first.GetSegmentsSummary();
            string dimDetails = !string.IsNullOrEmpty(segsText) ? $" | Legs: [{segsText}]" : "";
            string boxDetails = (first.BoundingWidth > 0 && first.BoundingHeight > 0) ? $" | Box: {first.BoundingWidth:0}x{first.BoundingHeight:0}" : "";
            lblRebarDetails.Text = $"Selected: {count} bar(s) | ID: {first.Id} | Shape: {first.CurrentShapeCode} | Size: {first.Size} (D{first.Diameter:0}){dimDetails}{boxDetails}";
            Log($"Loaded {count} rebar(s). ID {first.Id}, Shape: {first.CurrentShapeCode}, Diameter: D{first.Diameter:0}, L={first.OriginalLength:0} mm {dimDetails}.");

            AutoLoadOriginalRebarToUi(first);
        }

        private void AutoLoadOriginalRebarToUi(RebarShapeInfo barInfo)
        {
            if (barInfo == null) return;
            try
            {
                _isUpdatingUi = true;

                // 1. Try match shape code in available catalog or create definition
                string origCode = barInfo.CurrentShapeCode?.Trim() ?? "1";
                var matchShape = _availableShapes?.FirstOrDefault(s => s.ShapeCode.Equals(origCode, StringComparison.OrdinalIgnoreCase))
                              ?? RebarShapeModifier.GetShapeDefinition(origCode, _model);

                if (matchShape != null)
                {
                    _currentShapeDef = matchShape;
                    if (cboShapeType.SelectedItem != matchShape) cboShapeType.SelectedItem = matchShape;
                    if (txtShapeNo != null) txtShapeNo.Text = matchShape.ShapeCode;

                    lblA.Text = matchShape.LabelA;
                    lblB.Text = matchShape.LabelB;
                    lblC.Text = matchShape.LabelC;
                    lblD.Text = matchShape.LabelD;

                    int paramCount = matchShape.ParamCount;
                    var hdcRule = HdcRebarShapeRulesManager.GetRule(matchShape.ShapeCode);
                    if (hdcRule != null && hdcRule.Fields.Count > 0)
                    {
                        int validFields = hdcRule.Fields.Keys.Count(k => k == "A" || k == "B" || k == "C" || k == "D");
                        if (validFields > 0)
                        {
                            paramCount = Math.Max(paramCount, Math.Min(4, validFields));
                        }
                    }

                    string sClean = matchShape.ShapeCode.Trim();
                    if (sClean == "1" || sClean == "102") paramCount = 1;
                    else if (sClean == "2") paramCount = 2;
                    else if (sClean == "3" || sClean == "4" || sClean == "45") paramCount = 3;

                    numA.Enabled = paramCount >= 1;
                    numB.Enabled = paramCount >= 2;
                    numC.Enabled = paramCount >= 3;
                    btnAutoHook.Enabled = (sClean == "56" || sClean == "54" || sClean == "55" || sClean == "11" || sClean == "12" || sClean == "14");
                    numD.Enabled = paramCount >= 4;
                }

                // 2. Populate dimensions accurately from the original bar
                if (barInfo.DimA > 0)
                    numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)barInfo.DimA));
                else if (barInfo.BoundingWidth > 0)
                    numA.Value = Math.Max(numA.Minimum, Math.Min(numA.Maximum, (decimal)barInfo.BoundingWidth));

                if (barInfo.DimB > 0)
                    numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)barInfo.DimB));
                else if (barInfo.BoundingHeight > 0)
                    numB.Value = Math.Max(numB.Minimum, Math.Min(numB.Maximum, (decimal)barInfo.BoundingHeight));

                if (barInfo.DimC > 0)
                    numC.Value = Math.Max(numC.Minimum, Math.Min(numC.Maximum, (decimal)barInfo.DimC));
                else
                    numC.Value = (decimal)Math.Max(100.0, barInfo.Diameter * 10.0);

                if (barInfo.DimD > 0)
                    numD.Value = Math.Max(numD.Minimum, Math.Min(numD.Maximum, (decimal)barInfo.DimD));
            }
            finally
            {
                _isUpdatingUi = false;
            }

            UpdatePreview();
        }

        private void BtnAutoHook_Click(object sender, EventArgs e)
        {
            var first = GetFirstSelectedRebar();
            double dia = first != null ? first.Diameter : 10.0;
            numC.Value = (decimal)Math.Max(100.0, dia * 10.0);
            Log($"Auto calculated 10d hook for bar D{dia:0} = {numC.Value} mm");
        }

        private void BtnAutoConcrete_Click(object sender, EventArgs e)
        {
            var first = GetFirstSelectedRebar();
            if (first == null)
            {
                MessageBox.Show("Please select a rebar before auto-fitting host dimensions!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double cover = (double)numCover.Value;
            var (w, h, ok) = RebarShapeModifier.DetectConcreteDimensions(first.ModelObject, cover, _model);

            if (ok)
            {
                numA.Value = (decimal)w;
                numB.Value = (decimal)h;
                Log($"Auto-detected host concrete section: A={w:0} mm, B={h:0} mm (Cover={cover:0} mm)");
            }
            else
            {
                Log("Could not find host concrete member. Keeping current dimensions.");
            }
        }

        private string GetPlaneModeString()
        {
            switch (cboPlaneMode.SelectedIndex)
            {
                case 1: return "HorizontalXY";
                case 2: return "VerticalXZ";
                case 3: return "VerticalYZ";
                default: return "AutoPerpendicular";
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            var snapshot = GetSelectedRebarsSnapshot();
            if (snapshot.Count == 0)
            {
                MessageBox.Show("Please select at least one rebar first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteShapeChange(snapshot);
        }

        private void BtnBatchApply_Click(object sender, EventArgs e)
        {
            try
            {
                btnBatchApply.Enabled = false;
                Log("Please pick / window-select rebars in Tekla to apply this Shape (middle click to finish)...");
                var rebars = RebarShapeModifier.PickReinforcementsFromModel("Pick or window-select rebars to apply this Shape:");
                if (rebars != null && rebars.Count > 0)
                {
                    var list = new List<RebarShapeInfo>();
                    foreach (var r in rebars)
                    {
                        var info = RebarShapeModifier.AnalyzeRebar(r);
                        if (info != null) list.Add(info);
                    }
                    ExecuteShapeChange(list);
                }
            }
            catch (Exception ex)
            {
                Log($"Error in batch apply: {ex.Message}");
            }
            finally
            {
                btnBatchApply.Enabled = true;
            }
        }

        private void ExecuteShapeChange(List<RebarShapeInfo> rebarsToChange)
        {
            if (rebarsToChange == null || rebarsToChange.Count == 0) return;

            double a = (double)numA.Value;
            double b = (double)numB.Value;
            double c = (double)numC.Value;
            double d = (double)numD.Value;
            string targetShapeCode = _currentShapeDef != null ? _currentShapeDef.ShapeCode : cboShapeType.Text.Trim();
            string planeMode = GetPlaneModeString();

            int successCount = 0;
            var batchUndo = new List<RebarUndoRecord>();

            foreach (var info in rebarsToChange)
            {
                var frame = RebarShapeModifier.GetRebarReferenceFrame(info, planeMode);

                string corner = "TopRight"; // Index 0 = "TR" (Top-Right)
                if (cboHookCorner != null)
                {
                    if (cboHookCorner.SelectedIndex == 1) corner = "BottomRight";
                    else if (cboHookCorner.SelectedIndex == 2) corner = "BottomLeft";
                    else if (cboHookCorner.SelectedIndex == 3) corner = "TopLeft";
                }

                bool flip = chkFlip != null && chkFlip.Checked;
                double rotAngle = numRotation != null ? (double)numRotation.Value : 0.0;
                var newPts = RebarShapeModifier.GenerateShapePoints(
                    targetShapeCode,
                    frame.Center,
                    frame.WidthDir,
                    frame.HeightDir,
                    a,
                    b,
                    c,
                    d,
                    info.Diameter,
                    _model,
                    corner,
                    flip,
                    rotAngle);

                if (newPts != null && newPts.Count >= 2)
                {
                    var undo = new RebarUndoRecord();
                    if (RebarShapeModifier.ApplyShapeToRebar(info.ModelObject, newPts, info.Diameter, targetShapeCode, c, undo, out string err))
                    {
                        successCount++;
                        batchUndo.Add(undo);
                    }
                    else
                    {
                        Log($"Error modifying rebar ID {info.Id}: {err}");
                    }
                }
            }

            if (successCount > 0)
            {
                _undoStack.Push(batchUndo);
                _model.CommitChanges();

                // Validate and report Tekla recognized shape post-commit
                int verifiedShapeCount = 0;
                var unrecognizedItems = new List<string>();

                foreach (var undo in batchUndo)
                {
                    if (undo.ModelObject == null) continue;

                    string recognized = "";
                    undo.ModelObject.GetReportProperty("SHAPE", ref recognized);
                    if (string.IsNullOrEmpty(recognized))
                    {
                        undo.ModelObject.GetReportProperty("SHAPE_INTERNAL", ref recognized);
                    }

                    recognized = recognized?.Trim() ?? "";
                    bool matches = string.Equals(recognized, targetShapeCode, StringComparison.OrdinalIgnoreCase)
                                || (int.TryParse(recognized, out int rCode) && int.TryParse(targetShapeCode, out int tCode) && rCode == tCode);

                    if (matches)
                    {
                        verifiedShapeCount++;
                    }
                    else if (!string.IsNullOrEmpty(recognized))
                    {
                        unrecognizedItems.Add($"ID {undo.ModelObject.Identifier.ID}: {recognized}");
                    }
                    else
                    {
                        unrecognizedItems.Add($"ID {undo.ModelObject.Identifier.ID}: unmapped");
                    }
                }

                if (verifiedShapeCount == successCount)
                {
                    Log($"Successfully modified {successCount} rebar(s) to Shape {targetShapeCode} (A={a:0}, B={b:0}, C={c:0}). Verified: 100% recognized as Shape {targetShapeCode} by Tekla.");
                }
                else
                {
                    string details = unrecognizedItems.Count > 0 ? " [" + string.Join(", ", unrecognizedItems.Take(3)) + (unrecognizedItems.Count > 3 ? "..." : "") + "]" : "";
                    Log($"Modified {successCount} rebar(s). Tekla recognition: {verifiedShapeCount}/{successCount} bars verified as Shape {targetShapeCode}{details}.");
                }

                // Refresh UI with latest committed properties
                RefreshSelectedRebarsUi();
            }
            else
            {
                Log("Modification failed for the selected rebar(s).");
            }
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            if (_undoStack.Count == 0)
            {
                Log("No action history to undo.");
                return;
            }

            var batch = _undoStack.Pop();
            int undoCount = 0;

            foreach (var record in batch)
            {
                if (record.Restore(out string err))
                {
                    undoCount++;
                }
                else if (!string.IsNullOrEmpty(err))
                {
                    Log($"Undo warning: {err}");
                }
            }

            if (undoCount > 0)
            {
                _model.CommitChanges();
                Log($"Undone successfully: Restored {undoCount} rebar(s) to previous shape!");

                // Refresh selected rebars state in UI
                RefreshSelectedRebarsUi();
            }
        }

        private void RefreshSelectedRebarsUi()
        {
            var currentSnapshot = GetSelectedRebarsSnapshot();
            if (currentSnapshot.Count > 0)
            {
                var refreshed = new List<RebarShapeInfo>();
                foreach (var info in currentSnapshot)
                {
                    if (info.ModelObject != null)
                    {
                        var refreshedInfo = RebarShapeModifier.AnalyzeRebar(info.ModelObject);
                        if (refreshedInfo != null) refreshed.Add(refreshedInfo);
                    }
                }
                lock (_selectedRebarsLock)
                {
                    _selectedRebars = refreshed;
                }
                var first = GetFirstSelectedRebar();
                if (first != null)
                {
                    string segsText = first.GetSegmentsSummary();
                    string dimDetails = !string.IsNullOrEmpty(segsText) ? $" | Legs: [{segsText}]" : "";
                    string boxDetails = (first.BoundingWidth > 0 && first.BoundingHeight > 0) ? $" | Box: {first.BoundingWidth:0}x{first.BoundingHeight:0}" : "";
                    lblRebarDetails.Text = $"Selected: {refreshed.Count} bar(s) | ID: {first.Id} | Shape: {first.CurrentShapeCode} | Size: {first.Size} (D{first.Diameter:0}){dimDetails}{boxDetails}";
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            RebarShapeModifier.LogHandler -= LogFromOtherThreads;
            HdcRebarShapeRulesManager.LogHandler -= LogFromOtherThreads;
            DwgShapeCatalogManager.LogHandler -= LogFromOtherThreads;
            base.OnFormClosed(e);
        }

        private void LogFromOtherThreads(string msg)
        {
            if (txtLog != null && !txtLog.IsDisposed)
            {
                if (txtLog.InvokeRequired)
                {
                    try { txtLog.BeginInvoke(new Action<string>(Log), msg); } catch { }
                }
                else
                {
                    Log(msg);
                }
            }
        }

        private void Log(string msg)
        {
            if (txtLog == null || txtLog.IsDisposed) return;
            string line = $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n";
            txtLog.AppendText(line);
        }
    }
}
