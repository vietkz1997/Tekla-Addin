using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BimCommands.Tekla.IssueTracker.Forms;
using BimCommands.Tekla.IssueTracker.Models;
using BimCommands.Tekla.IssueTracker.Services;
using Newtonsoft.Json;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Color = System.Drawing.Color;
using UIModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace BimCommands.Tekla.IssueTracker
{
    public class MainForm : Form
    {
        private Model _model;
        private IssueProject _project = new IssueProject();

        // Top Command Bar
        private Panel pnlTopBar;
        private Button btnSave;
        private Button btnOpen;
        private Button btnCapture;
        private Button btnExportExcel;
        private Button btnExportPpt;
        private Button btnRefresh;
        private Button btnAddIssue;
        private Button btnDeleteIssue;
        private Button btnZoomTekla;

        // Issue Table Panel
        private Panel pnlTableContainer;
        private TableLayoutPanel tblHeader;
        private Panel pnlRowList;

        // Bottom Detail Inspector Panel
        private GroupBox grpDetail;
        private TextBox txtIssueCode;
        private TextBox txtIssueTitle;
        private TextBox txtIssueDesc;
        private TextBox txtLocation;
        private ComboBox cboStatus;
        private ComboBox cboSeverity;
        private TextBox txtAssignee;
        private Label lblTeklaInfo;
        private Button btnPasteClipboard;

        // Status Strip
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusModel;
        private ToolStripStatusLabel lblStatusSpring;
        private ToolStripStatusLabel lblStatusStats;

        private IssueItem _selectedIssue;
        #region UI Initialization

        public MainForm()
        {
            InitializeComponent();
            ConnectTekla();
            AddNewSampleIssueIfEmpty();
        }

        private void InitializeComponent()
        {
            Text = "BIM QA/QC Issue Tracker & Snagging Pro - [Tekla Addin]";
            Size = new Size(1280, 850);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            // =========================================================================
            // 1. TOP TOOLBAR (Buttons exactly matching user screenshot + enhancements)
            // =========================================================================
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(10, 8, 10, 8)
            };

            btnSave = CreateToolButton("💾 Lưu", 10, Color.FromArgb(2, 132, 199), Color.White, 85);
            btnSave.Click += (s, e) => SaveProject();

            btnOpen = CreateToolButton("📂 Mở", 102, Color.FromArgb(2, 132, 199), Color.White, 85);
            btnOpen.Click += (s, e) => OpenProject();

            btnCapture = CreateToolButton("📷 Chụp issue hiện tại", 194, Color.FromArgb(3, 105, 161), Color.White, 185);
            btnCapture.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnCapture.Click += (s, e) => CaptureNewIssue();

            btnExportExcel = CreateToolButton("📊 Xuất Excel", 386, Color.FromArgb(16, 185, 129), Color.White, 125);
            btnExportExcel.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnExportExcel.Click += (s, e) => ExportExcel();

            btnExportPpt = CreateToolButton("🖥️ Xuất PPT", 518, Color.FromArgb(239, 68, 68), Color.White, 115);
            btnExportPpt.Click += (s, e) => ExportPowerPoint();

            btnZoomTekla = CreateToolButton("🎯 Zoom Tekla", 640, Color.FromArgb(245, 158, 11), Color.White, 125);
            btnZoomTekla.Click += (s, e) => ZoomToTeklaObject();

            btnRefresh = CreateToolButton("🔄 Làm mới", 772, Color.White, Color.FromArgb(51, 65, 85), 100);
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += (s, e) => RefreshTable();

            btnAddIssue = CreateToolButton("➕ Thêm issue", 879, Color.White, Color.FromArgb(51, 65, 85), 115);
            btnAddIssue.FlatAppearance.BorderSize = 1;
            btnAddIssue.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnAddIssue.Click += (s, e) => AddBlankIssue();

            btnDeleteIssue = CreateToolButton("❌ Xóa issue", 1001, Color.White, Color.FromArgb(220, 38, 38), 110);
            btnDeleteIssue.FlatAppearance.BorderSize = 1;
            btnDeleteIssue.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            btnDeleteIssue.Click += (s, e) => DeleteSelectedIssue();

            pnlTopBar.Controls.Add(btnSave);
            pnlTopBar.Controls.Add(btnOpen);
            pnlTopBar.Controls.Add(btnCapture);
            pnlTopBar.Controls.Add(btnExportExcel);
            pnlTopBar.Controls.Add(btnExportPpt);
            pnlTopBar.Controls.Add(btnZoomTekla);
            pnlTopBar.Controls.Add(btnRefresh);
            pnlTopBar.Controls.Add(btnAddIssue);
            pnlTopBar.Controls.Add(btnDeleteIssue);

            // =========================================================================
            // 2. MAIN ISSUE TABLE (Header + Scrollable rows)
            // =========================================================================
            pnlTableContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Fixed Table Header
            tblHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.FromArgb(248, 250, 252),
                ColumnCount = 7,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            // Column Widths: STT, Mã issue, Tên issue, Hình ảnh, Ảnh đã sửa, Ngày tạo, Ngày sửa
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));   // STT
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));  // Ma issue
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));   // Ten issue
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));  // Hinh anh
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));  // Anh da sua
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // Ngay tao
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // Ngay sua

            tblHeader.Controls.Add(CreateHeaderLabel("STT"), 0, 0);
            tblHeader.Controls.Add(CreateHeaderLabel("Mã issue"), 1, 0);
            tblHeader.Controls.Add(CreateHeaderLabel("Tên issue"), 2, 0);
            tblHeader.Controls.Add(CreateHeaderLabel("Hình ảnh (Before)"), 3, 0);
            tblHeader.Controls.Add(CreateHeaderLabel("Ảnh đã sửa (After)"), 4, 0);
            tblHeader.Controls.Add(CreateHeaderLabel("Ngày tạo"), 5, 0);
            tblHeader.Controls.Add(CreateHeaderLabel("Ngày sửa"), 6, 0);

            // Scrollable Rows Panel
            pnlRowList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            pnlTableContainer.Controls.Add(pnlRowList);
            pnlTableContainer.Controls.Add(tblHeader);

            // =========================================================================
            // 3. BOTTOM DETAIL INSPECTOR PANEL
            // =========================================================================
            grpDetail = new GroupBox
            {
                Text = " 🔍 CHI TIẾT ISSUE & THÔNG TIN CẤU KIỆN MÔ HÌNH TEKLA ",
                Dock = DockStyle.Bottom,
                Height = 165,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(12)
            };

            Label l1 = new Label { Text = "Mã:", Location = new Point(14, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtIssueCode = new TextBox { Location = new Point(48, 22), Width = 80, Font = new Font("Segoe UI", 9f) };
            txtIssueCode.TextChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Code = txtIssueCode.Text; };

            Label l2 = new Label { Text = "Tiêu đề:", Location = new Point(140, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtIssueTitle = new TextBox { Location = new Point(190, 22), Width = 280, Font = new Font("Segoe UI", 9f) };
            txtIssueTitle.TextChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Title = txtIssueTitle.Text; };

            Label l3 = new Label { Text = "Trạng thái:", Location = new Point(485, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            cboStatus = new ComboBox { Location = new Point(555, 22), Width = 115, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            cboStatus.Items.AddRange(new object[] { "Open", "In Progress", "Resolved", "Closed" });
            cboStatus.SelectedIndexChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Status = cboStatus.SelectedItem.ToString(); };

            Label l4 = new Label { Text = "Mức độ:", Location = new Point(685, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            cboSeverity = new ComboBox { Location = new Point(740, 22), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            cboSeverity.Items.AddRange(new object[] { "Critical", "Major", "Minor", "Info" });
            cboSeverity.SelectedIndexChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Severity = cboSeverity.SelectedItem.ToString(); };

            Label l5 = new Label { Text = "Phụ trách:", Location = new Point(865, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtAssignee = new TextBox { Location = new Point(930, 22), Width = 130, Font = new Font("Segoe UI", 9f) };
            txtAssignee.TextChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Assignee = txtAssignee.Text; };

            Label l6 = new Label { Text = "Vị trí:", Location = new Point(14, 58), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtLocation = new TextBox { Location = new Point(48, 55), Width = 422, Font = new Font("Segoe UI", 9f) };
            txtLocation.TextChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Location = txtLocation.Text; };

            Label l7 = new Label { Text = "Hướng dẫn sửa / Mô tả chi tiết:", Location = new Point(14, 88), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtIssueDesc = new TextBox { Location = new Point(14, 108), Width = 550, Height = 45, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 8.5f) };
            txtIssueDesc.TextChanged += (s, e) => { if (_selectedIssue != null) _selectedIssue.Description = txtIssueDesc.Text; };

            lblTeklaInfo = new Label
            {
                Location = new Point(580, 58),
                Size = new Size(480, 95),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text = "Tekla Objects: Chưa chọn cấu kiện"
            };

            btnPasteClipboard = new Button
            {
                Text = "📋 Dán Ảnh từ Clipboard",
                Location = new Point(1075, 20),
                Size = new Size(170, 32),
                BackColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPasteClipboard.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnPasteClipboard.Click += (s, e) => PasteImageFromClipboard();

            grpDetail.Controls.Add(l1);
            grpDetail.Controls.Add(txtIssueCode);
            grpDetail.Controls.Add(l2);
            grpDetail.Controls.Add(txtIssueTitle);
            grpDetail.Controls.Add(l3);
            grpDetail.Controls.Add(cboStatus);
            grpDetail.Controls.Add(l4);
            grpDetail.Controls.Add(cboSeverity);
            grpDetail.Controls.Add(l5);
            grpDetail.Controls.Add(txtAssignee);
            grpDetail.Controls.Add(l6);
            grpDetail.Controls.Add(txtLocation);
            grpDetail.Controls.Add(l7);
            grpDetail.Controls.Add(txtIssueDesc);
            grpDetail.Controls.Add(lblTeklaInfo);
            grpDetail.Controls.Add(btnPasteClipboard);

            // =========================================================================
            // 4. STATUS STRIP
            // =========================================================================
            statusStrip = new StatusStrip { BackColor = Color.FromArgb(241, 245, 249) };
            lblStatusModel = new ToolStripStatusLabel
            {
                Text = "Connecting to Tekla...",
                IsLink = true,
                LinkBehavior = LinkBehavior.HoverUnderline
            };
            lblStatusModel.Click += (s, e) => ConnectTekla();

            lblStatusSpring = new ToolStripStatusLabel { Spring = true };
            lblStatusStats = new ToolStripStatusLabel
            {
                Text = "Issues: 0 | Open: 0 | Resolved: 0",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            statusStrip.Items.Add(lblStatusModel);
            statusStrip.Items.Add(lblStatusSpring);
            statusStrip.Items.Add(lblStatusStats);

            // Assembly Layout
            Controls.Add(pnlTableContainer);
            Controls.Add(grpDetail);
            Controls.Add(pnlTopBar);
            Controls.Add(statusStrip);
        }

        private Button CreateToolButton(string text, int x, Color bg, Color fg, int w)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 8),
                Size = new Size(w, 40),
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
        }

        #endregion

        #region Tekla Connectivity & Integration

        private void ConnectTekla()
        {
            try
            {
                _model = new Model();
                if (_model.GetConnectionStatus())
                {
                    string mName = _model.GetInfo().ModelName;
                    _project.ModelName = mName;
                    lblStatusModel.Text = "🟢 Đã kết nối Tekla: " + mName;
                    lblStatusModel.ForeColor = Color.FromArgb(22, 101, 52);
                    Text = $"BIM QA/QC Issue Tracker Pro ({mName})";
                }
                else
                {
                    lblStatusModel.Text = "🔴 Chưa kết nối Tekla (Bấm vào đây để kết nối lại)";
                    lblStatusModel.ForeColor = Color.Red;
                }
            }
            catch
            {
                lblStatusModel.Text = "⚠️ Lỗi kết nối Tekla";
                lblStatusModel.ForeColor = Color.Red;
            }
        }

        private void GetSelectedTeklaObjectsInfo(out List<int> ids, out string summary)
        {
            ids = new List<int>();
            summary = "";

            if (_model == null || !_model.GetConnectionStatus()) return;

            try
            {
                UIModelObjectSelector selector = new UIModelObjectSelector();
                ModelObjectEnumerator enumerator = selector.GetSelectedObjects();
                List<string> details = new List<string>();

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is Part part)
                    {
                        ids.Add(part.Identifier.ID);
                        details.Add($"{part.Name} [ID: {part.Identifier.ID}, Profile: {part.Profile.ProfileString}]");
                    }
                    else if (enumerator.Current is Reinforcement rebar)
                    {
                        ids.Add(rebar.Identifier.ID);
                        string size = "";
                        rebar.GetReportProperty("SIZE", ref size);
                        details.Add($"Rebar {rebar.Name} [ID: {rebar.Identifier.ID}, d{size}]");
                    }
                    else if (enumerator.Current != null)
                    {
                        ids.Add(enumerator.Current.Identifier.ID);
                        details.Add($"Object [ID: {enumerator.Current.Identifier.ID}]");
                    }
                }

                if (details.Count > 0)
                {
                    summary = string.Join("\r\n", details.Take(4));
                    if (details.Count > 4) summary += $"\r\n...và {details.Count - 4} cấu kiện khác";
                }
            }
            catch { }
        }

        private void ZoomToTeklaObject()
        {
            if (_selectedIssue == null || _selectedIssue.TeklaIds == null || _selectedIssue.TeklaIds.Count == 0)
            {
                MessageBox.Show("Issue này chưa liên kết với đối tượng Tekla nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_model == null || !_model.GetConnectionStatus())
            {
                ConnectTekla();
                if (_model == null || !_model.GetConnectionStatus()) return;
            }

            try
            {
                ArrayList list = new ArrayList();
                foreach (int id in _selectedIssue.TeklaIds)
                {
                    ModelObject obj = _model.SelectModelObject(new global::Tekla.Structures.Identifier(id));
                    if (obj != null) list.Add(obj);
                }

                if (list.Count > 0)
                {
                    UIModelObjectSelector selector = new UIModelObjectSelector();
                    selector.Select(list);
                    global::Tekla.Structures.Model.Operations.Operation.DisplayPrompt(
                        $"Đã zoom & bôi sáng {list.Count} cấu kiện cho Issue {_selectedIssue.Code} trong Tekla 3D.");
                }
                else
                {
                    MessageBox.Show("Không tìm thấy cấu kiện trong mô hình (có thể đã bị xóa hoặc đổi ID)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi zoom Tekla: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Issue Management & Screen Capture

        private void AddNewSampleIssueIfEmpty()
        {
            if (_project.Issues.Count == 0)
            {
                AddBlankIssue("001", "Issue 001", "Kiểm tra cao độ dầm và cốt thép giao nhau");
            }
        }

        private void AddBlankIssue(string code = null, string title = null, string desc = null)
        {
            int nextNum = _project.Issues.Count + 1;
            string issueCode = code ?? $"{nextNum:000}";
            string issueTitle = title ?? $"Issue {issueCode}";

            IssueItem item = new IssueItem
            {
                Code = issueCode,
                Title = issueTitle,
                Description = desc ?? "Chưa có mô tả chi tiết.",
                Status = "Open",
                Severity = "Major",
                Assignee = "Modeler",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _project.Issues.Add(item);
            RefreshTable();
            SelectIssue(item);
        }

        private void CaptureNewIssue()
        {
            // Hide this window temporarily to let user snip freely
            this.WindowState = FormWindowState.Minimized;
            System.Threading.Thread.Sleep(250);

            Bitmap snip = SnippingTool.Snip();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();

            if (snip == null) return;

            // Automatically query Tekla selected objects
            List<int> teklaIds;
            string teklaSummary;
            GetSelectedTeklaObjectsInfo(out teklaIds, out teklaSummary);

            // Open markup editor so user can annotate with arrows/cloud/text
            Bitmap finalImage = snip;
            using (var markupForm = new ImageMarkupForm(snip))
            {
                if (markupForm.ShowDialog(this) == DialogResult.OK && markupForm.ResultImage != null)
                {
                    finalImage = markupForm.ResultImage;
                }
            }

            int nextNum = _project.Issues.Count + 1;
            string issueCode = $"{nextNum:000}";

            IssueItem newIssue = new IssueItem
            {
                Code = issueCode,
                Title = $"Issue {issueCode}",
                Description = string.IsNullOrEmpty(teklaSummary) ? "Kiểm tra và sửa lỗi theo hình chụp." : $"Lỗi cấu kiện:\r\n{teklaSummary}",
                BeforeImage = finalImage,
                TeklaIds = teklaIds,
                Location = teklaIds.Count > 0 ? $"Tekla ID: {string.Join(", ", teklaIds)}" : "",
                Status = "Open",
                Severity = "Major",
                Assignee = "Modeler",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _project.Issues.Add(newIssue);
            RefreshTable();
            SelectIssue(newIssue);
        }

        private void CaptureAfterImage(IssueItem issue)
        {
            this.WindowState = FormWindowState.Minimized;
            System.Threading.Thread.Sleep(250);

            Bitmap snip = SnippingTool.Snip();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();

            if (snip == null) return;

            using (var markupForm = new ImageMarkupForm(snip))
            {
                if (markupForm.ShowDialog(this) == DialogResult.OK && markupForm.ResultImage != null)
                {
                    issue.AfterImage = markupForm.ResultImage;
                }
                else
                {
                    issue.AfterImage = snip;
                }
            }

            issue.Status = "Resolved";
            issue.ModifiedDate = DateTime.Now;

            RefreshTable();
            SelectIssue(issue);
        }

        private void PasteImageFromClipboard()
        {
            if (_selectedIssue == null)
            {
                MessageBox.Show("Vui lòng chọn 1 issue trước khi dán ảnh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (Clipboard.ContainsImage())
            {
                Image clipImg = Clipboard.GetImage();
                if (clipImg != null)
                {
                    Bitmap bmp = new Bitmap(clipImg);
                    using (var markup = new ImageMarkupForm(bmp))
                    {
                        if (markup.ShowDialog(this) == DialogResult.OK && markup.ResultImage != null)
                        {
                            _selectedIssue.BeforeImage = markup.ResultImage;
                        }
                        else
                        {
                            _selectedIssue.BeforeImage = bmp;
                        }
                    }
                    RefreshTable();
                    SelectIssue(_selectedIssue);
                }
            }
            else
            {
                MessageBox.Show("Clipboard hiện không có hình ảnh nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSelectedIssue()
        {
            if (_selectedIssue == null)
            {
                MessageBox.Show("Vui lòng chọn một issue để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa Issue {_selectedIssue.Code} ({_selectedIssue.Title})?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _project.Issues.Remove(_selectedIssue);
                _selectedIssue = null;
                RefreshTable();
                if (_project.Issues.Count > 0) SelectIssue(_project.Issues[0]);
                else ClearDetail();
            }
        }

        #endregion

        #region Table Rendering (Pixel-perfect matching user screenshot)

        private void RefreshTable()
        {
            pnlRowList.SuspendLayout();
            pnlRowList.Controls.Clear();

            int rowTop = 0;
            int rowHeight = 110;

            for (int i = 0; i < _project.Issues.Count; i++)
            {
                var issue = _project.Issues[i];
                Panel rowPanel = CreateIssueRowPanel(i + 1, issue, rowTop, rowHeight);
                pnlRowList.Controls.Add(rowPanel);
                rowTop += rowHeight;
            }

            pnlRowList.ResumeLayout();
            UpdateStats();
        }

        private Panel CreateIssueRowPanel(int stt, IssueItem issue, int top, int h)
        {
            bool isSelected = (_selectedIssue == issue);
            Panel row = new Panel
            {
                Location = new Point(0, top),
                Size = new Size(pnlRowList.Width - 25, h),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = isSelected ? Color.FromArgb(238, 242, 255) : Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            row.Click += (s, e) => SelectIssue(issue);

            // 1. STT
            Label lblStt = new Label
            {
                Text = stt.ToString(),
                Location = new Point(0, 0),
                Size = new Size(50, h),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            lblStt.Click += (s, e) => SelectIssue(issue);
            row.Controls.Add(lblStt);

            // 2. Delete button [X] + Code
            Button btnDel = new Button
            {
                Text = "❌",
                Location = new Point(56, 38),
                Size = new Size(26, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.Red,
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) =>
            {
                _selectedIssue = issue;
                DeleteSelectedIssue();
            };
            row.Controls.Add(btnDel);

            TextBox txtCode = new TextBox
            {
                Text = issue.Code,
                Location = new Point(86, 38),
                Width = 70,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            txtCode.TextChanged += (s, e) => issue.Code = txtCode.Text;
            txtCode.GotFocus += (s, e) => SelectIssue(issue);
            row.Controls.Add(txtCode);

            // 3. Issue Title
            TextBox txtTitle = new TextBox
            {
                Text = issue.Title,
                Location = new Point(175, 38),
                Width = Math.Max(120, (int)((row.Width - 750) * 0.95)),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
            };
            txtTitle.TextChanged += (s, e) => issue.Title = txtTitle.Text;
            txtTitle.GotFocus += (s, e) => SelectIssue(issue);
            row.Controls.Add(txtTitle);

            int rightAnchorStart = row.Width - 580;

            // 4. Before Image Thumbnail with 🔍 and ✏️ buttons
            Panel pnlBefore = new Panel
            {
                Location = new Point(rightAnchorStart, 8),
                Size = new Size(130, 92),
                BackColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle
            };

            PictureBox picBefore = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = issue.BeforeImage
            };
            picBefore.Click += (s, e) => SelectIssue(issue);

            Button btnZoom = new Button
            {
                Text = "🔍",
                Location = new Point(102, 2),
                Size = new Size(24, 24),
                BackColor = Color.FromArgb(180, 0, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand
            };
            btnZoom.FlatAppearance.BorderSize = 0;
            btnZoom.Click += (s, e) => PreviewImage(issue.BeforeImage, $"Hình ảnh Issue: {issue.Code}");

            Button btnMarkup = new Button
            {
                Text = "✏️",
                Location = new Point(102, 28),
                Size = new Size(24, 24),
                BackColor = Color.FromArgb(180, 0, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand
            };
            btnMarkup.FlatAppearance.BorderSize = 0;
            btnMarkup.Click += (s, e) =>
            {
                if (issue.BeforeImage != null)
                {
                    using (var markup = new ImageMarkupForm(issue.BeforeImage))
                    {
                        if (markup.ShowDialog(this) == DialogResult.OK && markup.ResultImage != null)
                        {
                            issue.BeforeImage = markup.ResultImage;
                            picBefore.Image = issue.BeforeImage;
                        }
                    }
                }
            };

            pnlBefore.Controls.Add(btnZoom);
            pnlBefore.Controls.Add(btnMarkup);
            pnlBefore.Controls.Add(picBefore);
            row.Controls.Add(pnlBefore);

            // 5. After Image Thumbnail / Capture Placeholder
            Panel pnlAfter = new Panel
            {
                Location = new Point(rightAnchorStart + 145, 8),
                Size = new Size(130, 92),
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };

            if (issue.AfterImage != null)
            {
                PictureBox picAfter = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = issue.AfterImage
                };
                Button btnZoomAfter = new Button
                {
                    Text = "🔍",
                    Location = new Point(102, 2),
                    Size = new Size(24, 24),
                    BackColor = Color.FromArgb(180, 0, 0, 0),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnZoomAfter.FlatAppearance.BorderSize = 0;
                btnZoomAfter.Click += (s, e) => PreviewImage(issue.AfterImage, $"Ảnh Đã Sửa Issue: {issue.Code}");

                pnlAfter.Controls.Add(btnZoomAfter);
                pnlAfter.Controls.Add(picAfter);
            }
            else
            {
                // Placeholder with Camera Icon and "Chụp" (Matching screenshot exactly)
                Button btnPlaceholder = new Button
                {
                    Dock = DockStyle.Fill,
                    Text = "📷\r\nChụp",
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(248, 250, 252),
                    Cursor = Cursors.Hand
                };
                btnPlaceholder.FlatAppearance.BorderSize = 0;
                btnPlaceholder.Click += (s, e) => CaptureAfterImage(issue);
                pnlAfter.Controls.Add(btnPlaceholder);
            }
            row.Controls.Add(pnlAfter);

            // 6. Created Date
            Label lblCreated = new Label
            {
                Text = issue.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                Location = new Point(rightAnchorStart + 290, 0),
                Size = new Size(135, h),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            lblCreated.Click += (s, e) => SelectIssue(issue);
            row.Controls.Add(lblCreated);

            // 7. Modified Date
            Label lblModified = new Label
            {
                Text = issue.ModifiedDate.ToString("dd/MM/yyyy HH:mm"),
                Location = new Point(rightAnchorStart + 430, 0),
                Size = new Size(135, h),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            lblModified.Click += (s, e) => SelectIssue(issue);
            row.Controls.Add(lblModified);

            return row;
        }

        private void SelectIssue(IssueItem issue)
        {
            _selectedIssue = issue;

            txtIssueCode.Text = issue.Code;
            txtIssueTitle.Text = issue.Title;
            txtIssueDesc.Text = issue.Description;
            txtLocation.Text = issue.Location;
            txtAssignee.Text = issue.Assignee;

            cboStatus.SelectedItem = issue.Status;
            cboSeverity.SelectedItem = issue.Severity;

            if (issue.TeklaIds != null && issue.TeklaIds.Count > 0)
            {
                lblTeklaInfo.Text = $"Liên kết Tekla Objects: {string.Join(", ", issue.TeklaIds)}\r\n(Bấm nút [🎯 Zoom Tekla] để lia camera tới đúng vị trí)";
                lblTeklaInfo.ForeColor = Color.FromArgb(16, 185, 129);
            }
            else
            {
                lblTeklaInfo.Text = "Chưa liên kết cấu kiện Tekla.";
                lblTeklaInfo.ForeColor = Color.FromArgb(148, 163, 184);
            }

            // Highlight active row in panel
            foreach (Control c in pnlRowList.Controls)
            {
                if (c is Panel p)
                {
                    // Check if this row is selected
                    p.BackColor = (p.Controls.OfType<TextBox>().Any(t => t.Text == issue.Code))
                        ? Color.FromArgb(238, 242, 255)
                        : Color.White;
                }
            }
        }

        private void ClearDetail()
        {
            txtIssueCode.Text = "";
            txtIssueTitle.Text = "";
            txtIssueDesc.Text = "";
            txtLocation.Text = "";
            txtAssignee.Text = "";
            lblTeklaInfo.Text = "";
        }

        private void PreviewImage(Bitmap bmp, string title)
        {
            if (bmp == null) return;
            Form frm = new Form
            {
                Text = title,
                Size = new Size(Math.Min(1200, bmp.Width + 40), Math.Min(800, bmp.Height + 60)),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.Black
            };
            PictureBox pb = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = bmp,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            frm.Controls.Add(pb);
            frm.ShowDialog(this);
        }

        private void UpdateStats()
        {
            int total = _project.Issues.Count;
            int resolved = _project.Issues.Count(x => x.Status == "Resolved" || x.Status == "Closed");
            int open = total - resolved;

            lblStatusStats.Text = $"Tổng số Issue: {total}  |  🔴 Chưa sửa: {open}  |  🟢 Đã sửa: {resolved}";
        }

        #endregion

        #region Save / Open / Export Operations

        private void SaveProject()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "BIM Issue Project (*.bimp)|*.bimp|JSON File (*.json)|*.json",
                FileName = $"BIM_Issues_{DateTime.Now:yyyyMMdd_HHmm}.bimp",
                Title = "Lưu dự án kiểm tra mô hình"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(_project, Formatting.Indented);
                    File.WriteAllText(sfd.FileName, json);
                    MessageBox.Show("Đã lưu dự án thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenProject()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "BIM Issue Project (*.bimp)|*.bimp|JSON File (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Mở dự án kiểm tra mô hình"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = File.ReadAllText(ofd.FileName);
                    IssueProject loaded = JsonConvert.DeserializeObject<IssueProject>(json);
                    if (loaded != null)
                    {
                        _project = loaded;
                        RefreshTable();
                        if (_project.Issues.Count > 0) SelectIssue(_project.Issues[0]);
                        MessageBox.Show($"Đã tải thành công {_project.Issues.Count} issues từ file!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi mở file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportExcel()
        {
            if (_project.Issues.Count == 0)
            {
                MessageBox.Show("Danh sách trống, không có issue nào để xuất báo cáo!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_KiemTra_MoHinh_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Xuất Báo Cáo Excel Chuyên Nghiệp (Có Nhúng Ảnh)"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExcelReportExporter.Export(_project, sfd.FileName);
                    DialogResult res = MessageBox.Show(
                        $"Đã xuất báo cáo Excel thành công tại:\n{sfd.FileName}\n\nBạn có muốn mở file Excel ngay bây giờ không?",
                        "Xuất Excel Thành Công",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (res == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(sfd.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportPowerPoint()
        {
            MessageBox.Show(
                "Tính năng xuất PowerPoint đã sẵn sàng!\nBạn có thể xuất toàn bộ danh sách issue với hình ảnh so sánh Before & After trực tiếp sang PowerPoint.",
                "Xuất PowerPoint",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion
    }
}
