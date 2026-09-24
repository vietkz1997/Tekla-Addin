using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Color = System.Drawing.Color;
using TeklaPoint = global::Tekla.Structures.Geometry3d.Point;
using TeklaPicker = Tekla.Structures.Model.UI.Picker;
using UIModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using View = System.Windows.Forms.View;

namespace BimCommands.Tekla.OverlapChecker
{
    public class MainForm : Form
    {
        private class ObjectGeoData
        {
            public ModelObject Obj;
            public int Id;
            public string TypeName;
            public string Name;
            public string Profile;
            public TeklaPoint Min;
            public TeklaPoint Max;
            public TeklaPoint Center;
            public double SizeX;
            public double SizeY;
            public double SizeZ;
            public double Radius;
        }

        private Model _model;
        private List<OverlapItem> _overlapList = new List<OverlapItem>();

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;

        private GroupBox grpSettings;
        private Label lblTolerance;
        private NumericUpDown numTolerance;
        private RadioButton radExactDuplicate;
        private RadioButton radClashIntersection;
        private CheckBox chkAutoSelectTekla;

        private GroupBox grpActions;
        private Button btnPickObjects;
        private Button btnFromSelection;
        private Button btnHighlightTekla;
        private Button btnDeleteDuplicates;
        private Button btnReconnect;

        private GroupBox grpResults;
        private ListView lstResults;
        private Label lblSummary;
        private CheckBox chkSelectAll;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusModel;
        private ToolStripStatusLabel lblStatusSpring;
        private ToolStripStatusLabel lblStatusCount;

        public MainForm()
        {
            InitializeComponent();
            ConnectTekla();
        }

        private void InitializeComponent()
        {
            Text = "Tekla Overlap Checker - Duplicate Object Detector [My-tool]";
            Size = new Size(840, 700);
            MinimumSize = new Size(820, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            TopMost = true;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            // 1. Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            lblTitle = new Label
            {
                Text = "🔍 TEKLA OVERLAP CHECKER",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(16, 10),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Scan, highlight, and remove duplicate or overlapping parts in Tekla Structures",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(18, 36),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Settings Group
            grpSettings = new GroupBox
            {
                Text = " ⚙️ TOLERANCE & DETECTION SETTINGS ",
                Location = new Point(14, 75),
                Size = new Size(796, 75),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTolerance = new Label
            {
                Text = "Position Tolerance (mm):",
                Location = new Point(16, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            numTolerance = new NumericUpDown
            {
                Location = new Point(175, 25),
                Width = 70,
                Minimum = 0.1m,
                Maximum = 500m,
                Value = 2.0m,
                DecimalPlaces = 1,
                TextAlign = HorizontalAlignment.Right,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            radExactDuplicate = new RadioButton
            {
                Text = "Exact Duplicate Parts (100% Geometry)",
                Location = new Point(265, 26),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            radClashIntersection = new RadioButton
            {
                Text = "Clash / Volume Overlap",
                Location = new Point(530, 26),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Black
            };

            chkAutoSelectTekla = new CheckBox
            {
                Text = "Auto-highlight duplicate objects in Tekla 3D view after scanning",
                Location = new Point(18, 52),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            grpSettings.Controls.Add(lblTolerance);
            grpSettings.Controls.Add(numTolerance);
            grpSettings.Controls.Add(radExactDuplicate);
            grpSettings.Controls.Add(radClashIntersection);
            grpSettings.Controls.Add(chkAutoSelectTekla);

            // 3. Actions Group
            grpActions = new GroupBox
            {
                Text = " 🎯 SCAN & ACTION CONTROLS ",
                Location = new Point(14, 158),
                Size = new Size(796, 75),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnPickObjects = new Button
            {
                Text = "🎯  PICK WINDOW",
                Location = new Point(14, 23),
                Size = new Size(150, 38),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPickObjects.FlatAppearance.BorderSize = 0;
            btnPickObjects.Click += (s, e) => ScanOverlap(true);

            btnFromSelection = new Button
            {
                Text = "⚡  FROM SELECTION",
                Location = new Point(170, 23),
                Size = new Size(160, 38),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFromSelection.FlatAppearance.BorderSize = 0;
            btnFromSelection.Click += (s, e) => ScanOverlap(false);

            btnHighlightTekla = new Button
            {
                Text = "👁️  HIGHLIGHT 3D",
                Location = new Point(336, 23),
                Size = new Size(150, 38),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnHighlightTekla.FlatAppearance.BorderSize = 0;
            btnHighlightTekla.Click += (s, e) => HighlightDuplicatesInTekla();

            btnDeleteDuplicates = new Button
            {
                Text = "🗑️  DELETE DUPLICATES",
                Location = new Point(492, 23),
                Size = new Size(170, 38),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDeleteDuplicates.FlatAppearance.BorderSize = 0;
            btnDeleteDuplicates.Click += (s, e) => DeleteDuplicates();

            btnReconnect = new Button
            {
                Text = "🔄  RECONNECT",
                Location = new Point(668, 23),
                Size = new Size(115, 38),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReconnect.FlatAppearance.BorderSize = 1;
            btnReconnect.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnReconnect.Click += (s, e) => ConnectTekla();

            grpActions.Controls.Add(btnPickObjects);
            grpActions.Controls.Add(btnFromSelection);
            grpActions.Controls.Add(btnHighlightTekla);
            grpActions.Controls.Add(btnDeleteDuplicates);
            grpActions.Controls.Add(btnReconnect);

            // 4. Results Group
            grpResults = new GroupBox
            {
                Text = " 📋 DUPLICATE OBJECTS LIST ",
                Location = new Point(14, 240),
                Size = new Size(796, 370),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            chkSelectAll = new CheckBox
            {
                Text = "Select All",
                Location = new Point(16, 24),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            chkSelectAll.CheckedChanged += (s, e) =>
            {
                foreach (ListViewItem item in lstResults.Items)
                {
                    item.Checked = chkSelectAll.Checked;
                }
            };

            lblSummary = new Label
            {
                Text = "No scan performed yet. (Double-click any item to zoom and select in Tekla)",
                Location = new Point(115, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            lstResults = new ListView
            {
                Location = new Point(14, 50),
                Size = new Size(768, 305),
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            lstResults.Columns.Add("Duplicate ID", 100);
            lstResults.Columns.Add("Object Type", 110);
            lstResults.Columns.Add("Name", 110);
            lstResults.Columns.Add("Profile", 110);
            lstResults.Columns.Add("Kept ID", 90);
            lstResults.Columns.Add("Center (X, Y, Z)", 140);
            lstResults.Columns.Add("Overlap Type", 110);

            lstResults.DoubleClick += (s, e) =>
            {
                if (lstResults.SelectedItems.Count > 0)
                {
                    int index = lstResults.SelectedItems[0].Index;
                    if (index >= 0 && index < _overlapList.Count)
                    {
                        var dupObj = _overlapList[index].DuplicateObject;
                        if (dupObj != null)
                        {
                            ArrayList modelObjects = new ArrayList { dupObj };
                            UIModelObjectSelector selector = new UIModelObjectSelector();
                            selector.Select(modelObjects);
                            global::Tekla.Structures.Model.Operations.Operation.DisplayPrompt($"Selected duplicate object ID {dupObj.Identifier.ID} in Tekla.");
                        }
                    }
                }
            };

            grpResults.Controls.Add(chkSelectAll);
            grpResults.Controls.Add(lblSummary);
            grpResults.Controls.Add(lstResults);

            // 5. Status Strip
            statusStrip = new StatusStrip { BackColor = Color.FromArgb(241, 245, 249) };
            lblStatusModel = new ToolStripStatusLabel
            {
                Text = "Connecting to Tekla...",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                IsLink = true,
                LinkBehavior = LinkBehavior.HoverUnderline
            };
            lblStatusModel.Click += (s, e) => ConnectTekla();

            lblStatusSpring = new ToolStripStatusLabel { Spring = true };

            lblStatusCount = new ToolStripStatusLabel
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            statusStrip.Items.Add(lblStatusModel);
            statusStrip.Items.Add(lblStatusSpring);
            statusStrip.Items.Add(lblStatusCount);

            Controls.Add(grpResults);
            Controls.Add(grpActions);
            Controls.Add(grpSettings);
            Controls.Add(pnlHeader);
            Controls.Add(statusStrip);
        }

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
                    Text = "Tekla Overlap Checker (" + modelName + ") - [My-tool]";
                    lblStatusCount.Text = "Connected to Tekla 2020. Ready to scan.";
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
                        "1. Tekla Structures 2020 is running.\n" +
                        "2. A model (.db1) is open in Tekla.\n" +
                        "3. Click [🔄 RECONNECT] on the toolbar.",
                        "Tekla Not Connected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        private void ScanOverlap(bool pickWindow)
        {
            if (!EnsureTeklaConnected()) return;

            List<ModelObject> list = new List<ModelObject>();
            try
            {
                if (pickWindow)
                {
                    TeklaPicker picker = new TeklaPicker();
                    lblStatusCount.Text = "Waiting for user to pick area in Tekla 3D view...";
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
                lblStatusCount.Text = "Scan selection cancelled.";
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

            lblStatusCount.Text = $"Analyzing {list.Count} objects...";
            statusStrip.Refresh();

            double tolerance = (double)numTolerance.Value;
            bool exactOnly = radExactDuplicate.Checked;
            _overlapList = DetectOverlaps(list, tolerance, exactOnly);

            lstResults.Items.Clear();
            foreach (OverlapItem overlap in _overlapList)
            {
                ListViewItem item = new ListViewItem(overlap.DuplicateId.ToString());
                item.Checked = true;
                item.SubItems.Add(overlap.TypeName);
                item.SubItems.Add(overlap.Name);
                item.SubItems.Add(overlap.Profile);
                item.SubItems.Add(overlap.OriginalId.ToString());
                item.SubItems.Add(overlap.CenterStr);
                item.SubItems.Add(overlap.OverlapType);
                item.ForeColor = Color.FromArgb(185, 28, 28);
                lstResults.Items.Add(item);
            }

            lblSummary.Text = $"Scanned: {list.Count} objects | Detected: {_overlapList.Count} duplicate objects";
            lblStatusCount.Text = $"Detected {_overlapList.Count} duplicate objects.";

            if (_overlapList.Count > 0 && chkAutoSelectTekla.Checked)
            {
                HighlightDuplicatesInTekla();
            }
            else if (_overlapList.Count == 0)
            {
                MessageBox.Show(
                    "Great! No duplicate objects detected in the selected area.",
                    "Scan Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private List<OverlapItem> DetectOverlaps(List<ModelObject> objects, double tolerance, bool exactOnly)
        {
            List<OverlapItem> list = new List<OverlapItem>();
            HashSet<int> duplicateIds = new HashSet<int>();
            List<ObjectGeoData> geoList = new List<ObjectGeoData>();

            foreach (ModelObject obj in objects)
            {
                ObjectGeoData geo = ExtractGeoData(obj);
                if (geo != null)
                {
                    geoList.Add(geo);
                }
            }

            int count = geoList.Count;
            for (int i = 0; i < count; i++)
            {
                ObjectGeoData a = geoList[i];
                if (duplicateIds.Contains(a.Id)) continue;

                for (int j = i + 1; j < count; j++)
                {
                    ObjectGeoData b = geoList[j];
                    if (!duplicateIds.Contains(b.Id) && CheckIsOverlap(a, b, tolerance, exactOnly))
                    {
                        duplicateIds.Add(b.Id);
                        OverlapItem item = new OverlapItem
                        {
                            IsSelected = true,
                            OriginalObject = a.Obj,
                            DuplicateObject = b.Obj,
                            OriginalId = a.Id,
                            DuplicateId = b.Id,
                            TypeName = b.TypeName,
                            Name = b.Name,
                            Profile = b.Profile,
                            CenterPoint = b.Center,
                            CenterStr = $"{b.Center.X:0}, {b.Center.Y:0}, {b.Center.Z:0}",
                            OverlapType = exactOnly ? "100% Duplicate" : "Volume Overlap"
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        private bool CheckIsOverlap(ObjectGeoData a, ObjectGeoData b, double tol, bool exactOnly)
        {
            double dist = Distance(a.Center, b.Center);
            if (dist > a.Radius + b.Radius + tol)
            {
                return false;
            }

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
                return true;
            }

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
                string typeName = obj.GetType().Name;
                string name = "";
                string profile = "";
                TeklaPoint minPt = null;
                TeklaPoint maxPt = null;

                if (obj is Part part)
                {
                    name = part.Name;
                    profile = part.Profile.ProfileString;
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
                    string size = "";
                    string grade = "";
                    rebar.GetReportProperty("SIZE", ref size);
                    rebar.GetReportProperty("GRADE", ref grade);
                    profile = grade + " d" + size;
                    Solid solid = rebar.GetSolid();
                    if (solid != null)
                    {
                        minPt = solid.MinimumPoint;
                        maxPt = solid.MaximumPoint;
                    }
                }

                if (minPt == null || maxPt == null) return null;

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
                    TypeName = typeName,
                    Name = name,
                    Profile = profile,
                    Min = minPt,
                    Max = maxPt,
                    Center = center,
                    SizeX = szX,
                    SizeY = szY,
                    SizeZ = szZ,
                    Radius = radius
                };
            }
            catch
            {
                return null;
            }
        }

        private void HighlightDuplicatesInTekla()
        {
            if (_overlapList.Count == 0) return;

            ArrayList listToHighlight = new ArrayList();
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                if (lstResults.Items[i].Checked && i < _overlapList.Count)
                {
                    listToHighlight.Add(_overlapList[i].DuplicateObject);
                }
            }

            if (listToHighlight.Count == 0) return;

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
                if (lstResults.Items[i].Checked && i < _overlapList.Count)
                {
                    toDelete.Add(_overlapList[i]);
                }
            }

            if (toDelete.Count == 0)
            {
                MessageBox.Show(
                    "Please check at least one duplicate object in the list to delete!",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete {toDelete.Count} duplicate object(s) from Tekla Structures?\n\n(Original objects will be safely preserved)",
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

            lstResults.Items.Clear();
            _overlapList.Clear();
            lblSummary.Text = $"Deleted {deletedCount} duplicate object(s). Ready for new scan.";
            lblStatusCount.Text = $"Deleted {deletedCount} duplicate object(s).";
        }

        private double Distance(TeklaPoint a, TeklaPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
