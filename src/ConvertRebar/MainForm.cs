using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;


namespace BimCommands.ConvertRebar
{
    public class MainForm : Form
    {
        private Model _model;
        private RebarConverter _converter;
        private readonly List<RebarConversionRecord> _records = new List<RebarConversionRecord>();
        private readonly Stack<List<RebarConversionRecord>> _undoStack = new Stack<List<RebarConversionRecord>>();

        // UI Controls
        private Panel _pnlHeader;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Panel _pnlDiagram;
        private Label _lblDiagram;
        
        private Panel _pnlActions;
        private Button _btnConvertSelected;
        private Button _btnPickRebars;
        private Button _btnScanSelected;
        private Button _btnUndo;
        private Button _btnReconnect;

        private CheckBox _chkOnlySixPoints;
        private CheckBox _chkAutoCommit;

        private DataGridView _grid;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private ToolStripStatusLabel _lblModelInfo;

        public MainForm()
        {
            InitializeComponent();
            ConnectTekla();
        }

        private void InitializeComponent()
        {
            Text = "Convert-rebar (6 Points ➔ 4 Points) - [My-tool]";
            Size = new Size(950, 650);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 250, 252); // Light Slate
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            // 1. Header Panel
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(15, 23, 42), // Dark Slate
                Padding = new Padding(15, 10, 15, 10)
            };

            _lblTitle = new Label
            {
                Text = "⚡ CONVERT-REBAR (6 POINTS ➔ 4 POINTS)",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248), // Cyan
                AutoSize = true,
                Location = new Point(14, 10)
            };

            _lblSubtitle = new Label
            {
                Text = "Chuyển đổi hình dạng cốt thép Tekla (SingleRebar & RebarGroup) từ 6 điểm thành 4 điểm (Cắt bỏ 2 chân)",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184), // Slate 400
                AutoSize = true,
                Location = new Point(16, 36)
            };

            _pnlHeader.Controls.Add(_lblTitle);
            _pnlHeader.Controls.Add(_lblSubtitle);

            // 2. Diagram Banner Panel
            _pnlDiagram = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(241, 245, 249), // Slate 100
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 6, 15, 6)
            };

            _lblDiagram = new Label
            {
                Text = "SƠ ĐỒ BIẾN ĐỔI:   [P0] ─── [P1] ─── [P2] ─── [P3] ─── [P4] ─── [P5] (6 điểm)   ➔   [P1] ─── [P2] ─── [P3] ─── [P4] (4 điểm hình chữ U)\nQuy tắc: Cắt bỏ hoàn toàn 2 chân móc ngoài cùng (P0 và P5), giữ nguyên thân U và bán kính uốn chuẩn.",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59), // Slate 800
                Dock = DockStyle.Fill
            };
            _pnlDiagram.Controls.Add(_lblDiagram);

            // 3. Actions & Options Panel
            _pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };

            _btnConvertSelected = CreateStyledButton("⚡ Chuyển đổi thép đang chọn", 15, 10, 240, 36, Color.FromArgb(37, 99, 235), Color.White);
            _btnConvertSelected.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnConvertSelected.Click += (s, e) => ExecuteConversionFromSelection();

            _btnPickRebars = CreateStyledButton("👆 Chọn thép trên 3D (Pick)", 265, 10, 200, 36, Color.FromArgb(5, 150, 105), Color.White);
            _btnPickRebars.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnPickRebars.Click += (s, e) => ExecuteConversionFromPick();

            _btnScanSelected = CreateStyledButton("🔍 Quét kiểm tra (Scan)", 475, 10, 160, 36, Color.FromArgb(71, 85, 105), Color.White);
            _btnScanSelected.Click += (s, e) => ExecuteScanOnly();

            _btnUndo = CreateStyledButton("↩️ Hoàn tác (Undo)", 645, 10, 150, 36, Color.FromArgb(217, 119, 6), Color.White);
            _btnUndo.Enabled = false;
            _btnUndo.Click += (s, e) => ExecuteUndo();

            // Options Checkboxes
            _chkOnlySixPoints = new CheckBox
            {
                Text = "Chỉ chuyển đổi thép có đúng 6 điểm (Bỏ qua loại khác)",
                Location = new Point(18, 54),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            _chkAutoCommit = new CheckBox
            {
                Text = "Tự động CommitChanges()",
                Location = new Point(360, 54),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            _btnReconnect = CreateStyledButton("🔄 Kết nối lại", 725, 10, 130, 36, Color.FromArgb(241, 245, 249), Color.FromArgb(71, 85, 105));
            _btnReconnect.Click += (s, e) => ConnectTekla();

            _pnlActions.Controls.Add(_btnConvertSelected);
            _pnlActions.Controls.Add(_btnPickRebars);
            _pnlActions.Controls.Add(_btnScanSelected);
            _pnlActions.Controls.Add(_btnUndo);
            _pnlActions.Controls.Add(_btnReconnect);
            _pnlActions.Controls.Add(_chkOnlySixPoints);
            _pnlActions.Controls.Add(_chkAutoCommit);

            // 4. DataGridView
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 28 }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Index", HeaderText = "#", FillWeight = 25 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Rebar ID", FillWeight = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên thép", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "Size", FillWeight = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Grade", HeaderText = "Mác", FillWeight = 50 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Loại Rebar", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "OrigPts", HeaderText = "Pts Cũ", FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NewPts", HeaderText = "Pts Mới", FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái xử lý", FillWeight = 160 });

            _grid.CellDoubleClick += Grid_CellDoubleClick;
            _grid.CellFormatting += Grid_CellFormatting;

            // Context Menu for Grid
            var ctxMenu = new ContextMenuStrip();
            var mnuZoom = ctxMenu.Items.Add("🔍 Zoom & Chọn thép trên Tekla");
            mnuZoom.Click += (s, e) => ZoomSelectedGridRow();
            var mnuConvertRow = ctxMenu.Items.Add("⚡ Chuyển đổi riêng dòng này");
            mnuConvertRow.Click += (s, e) => ConvertSelectedGridRow();
            _grid.ContextMenuStrip = ctxMenu;

            // 5. Status Strip
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(241, 245, 249) };
            _lblStatus = new ToolStripStatusLabel { Text = "Sẵn sàng", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _lblModelInfo = new ToolStripStatusLabel
            {
                Text = "Chưa kết nối Tekla (Click để kết nối)",
                ForeColor = Color.DarkGray,
                IsLink = true,
                LinkBehavior = LinkBehavior.HoverUnderline
            };
            _lblModelInfo.Click += (s, e) => ConnectTekla();

            _statusStrip.Items.Add(_lblStatus);
            _statusStrip.Items.Add(_lblModelInfo);

            // Add controls to Form
            Controls.Add(_grid);
            Controls.Add(_pnlActions);
            Controls.Add(_pnlDiagram);
            Controls.Add(_pnlHeader);
            Controls.Add(_statusStrip);
        }

        private Button CreateStyledButton(string text, int x, int y, int w, int h, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
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
                    _converter = new RebarConverter(_model);
                    string modelName = _model.GetInfo().ModelName;
                    _lblModelInfo.Text = $"🟢 Tekla Model: {modelName}";
                    _lblModelInfo.ForeColor = Color.FromArgb(5, 150, 105);
                    _lblStatus.Text = "Đã kết nối Tekla Structures 2020 thành công. Hãy chọn thép trên mô hình rồi bấm Chuyển đổi.";
                }
                else
                {
                    _converter = null;
                    _lblModelInfo.Text = "🔴 Chưa kết nối Tekla (Click để thử lại)";
                    _lblModelInfo.ForeColor = Color.FromArgb(220, 38, 38);
                    _lblStatus.Text = "Chưa kết nối Tekla: Hãy chắc chắn Tekla Structures 2020 đang chạy và ĐÃ MỞ MÔ HÌNH (.db1), sau đó click [🔄 Kết nối lại].";
                }
            }
            catch (Exception ex)
            {
                _converter = null;
                _lblModelInfo.Text = "⚠️ Lỗi kết nối (Click để thử lại)";
                _lblModelInfo.ForeColor = Color.FromArgb(220, 38, 38);
                _lblStatus.Text = "Lỗi kết nối Tekla: " + ex.Message;
            }
        }

        private bool EnsureTeklaConnected()
        {
            if (_converter == null || _model == null || !_model.GetConnectionStatus())
            {
                ConnectTekla();
                if (_converter == null || _model == null || !_model.GetConnectionStatus())
                {
                    MessageBox.Show(
                        "Chưa kết nối được với Tekla Structures!\n\n" +
                        "Vui lòng kiểm tra:\n" +
                        "1. Phần mềm Tekla Structures 2020 đang chạy.\n" +
                        "2. Bạn đã MỞ MỘT MÔ HÌNH (.db1) trong Tekla.\n" +
                        "3. Sau khi mở mô hình xong, bấm nút [🔄 Kết nối lại] trên thanh công cụ.",
                        "Chưa kết nối Tekla Structures",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        private void ExecuteConversionFromSelection()
        {
            if (!EnsureTeklaConnected()) return;

            var rebars = _converter.GetSelectedReinforcements();
            if (rebars.Count == 0)
            {
                MessageBox.Show("Vui lòng quét chọn các thanh thép hoặc dầm/cột chứa thép trong mô hình Tekla trước!",
                    "Chưa chọn đối tượng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ProcessAndConvertRebars(rebars);
        }

        private void ExecuteConversionFromPick()
        {
            if (!EnsureTeklaConnected()) return;

            try
            {
                _lblStatus.Text = "Đang chờ người dùng pick chọn thép trên Tekla 3D...";
                var rebars = _converter.PickReinforcementsFromModel("Chọn các thanh thép cần chuyển đổi, sau đó click chuột giữa để hoàn tất");
                if (rebars.Count == 0)
                {
                    _lblStatus.Text = "Đã hủy thao tác chọn.";
                    return;
                }

                ProcessAndConvertRebars(rebars);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi khi chọn thép: " + ex.Message;
            }
        }

        private void ExecuteScanOnly()
        {
            if (!EnsureTeklaConnected()) return;

            var rebars = _converter.GetSelectedReinforcements();
            if (rebars.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn các thanh thép trong Tekla trước khi quét!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _records.Clear();
            _grid.Rows.Clear();

            int idx = 1;
            int readyCount = 0;
            foreach (var r in rebars)
            {
                var rec = _converter.AnalyzeRebar(r);
                if (rec != null)
                {
                    _records.Add(rec);
                    _grid.Rows.Add(idx++, rec.Id, rec.Name, rec.Size, rec.Grade, rec.RebarType, rec.OriginalPointCount, rec.NewPointCount, rec.Status);
                    if (rec.OriginalPointCount == 6) readyCount++;
                }
            }

            _lblStatus.Text = $"Quét hoàn tất {rebars.Count} thanh/nhóm thép. Phát hiện {readyCount} thép 6 điểm sẵn sàng chuyển đổi.";
        }

        private void ProcessAndConvertRebars(List<Reinforcement> rebars)
        {
            _records.Clear();
            _grid.Rows.Clear();

            var convertedInThisRun = new List<RebarConversionRecord>();
            int idx = 1;
            int successCount = 0;
            int skippedCount = 0;

            bool onlySix = _chkOnlySixPoints.Checked;

            foreach (var r in rebars)
            {
                var rec = _converter.AnalyzeRebar(r);
                if (rec == null) continue;

                if (rec.OriginalPointCount == 6 || !onlySix)
                {
                    bool ok = _converter.ConvertRebar(rec, onlySix);
                    if (ok)
                    {
                        successCount++;
                        convertedInThisRun.Add(rec);
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                else
                {
                    skippedCount++;
                }

                _records.Add(rec);
                _grid.Rows.Add(idx++, rec.Id, rec.Name, rec.Size, rec.Grade, rec.RebarType, rec.OriginalPointCount, rec.NewPointCount, rec.Status);
            }

            if (_chkAutoCommit.Checked && successCount > 0)
            {
                _model.CommitChanges();
            }

            if (convertedInThisRun.Count > 0)
            {
                _undoStack.Push(convertedInThisRun);
                _btnUndo.Enabled = true;
            }

            _lblStatus.Text = $"Hoàn tất: Đã chuyển đổi thành công {successCount} thép (6 điểm ➔ 4 điểm). Bỏ qua: {skippedCount}.";

            if (successCount > 0)
            {
                MessageBox.Show($"Đã chuyển đổi thành công {successCount} thanh/nhóm thép từ 6 điểm thành 4 điểm (Cắt 2 chân)!",
                    "Chuyển đổi thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExecuteUndo()
        {
            if (_undoStack.Count == 0) return;

            var lastBatch = _undoStack.Pop();
            int undoCount = 0;

            foreach (var rec in lastBatch)
            {
                bool ok = _converter.UndoRecord(rec);
                if (ok) undoCount++;
            }

            if (_chkAutoCommit.Checked && undoCount > 0)
            {
                _model.CommitChanges();
            }

            // Refresh grid status
            for (int i = 0; i < _records.Count; i++)
            {
                _grid.Rows[i].Cells["NewPts"].Value = _records[i].NewPointCount;
                _grid.Rows[i].Cells["Status"].Value = _records[i].Status;
            }

            _btnUndo.Enabled = _undoStack.Count > 0;
            _lblStatus.Text = $"Đã hoàn tác (Undo) thành công {undoCount} thanh/nhóm thép về hình dạng ban đầu.";
        }

        private void ZoomSelectedGridRow()
        {
            if (_grid.CurrentRow == null) return;
            int rowIndex = _grid.CurrentRow.Index;
            if (rowIndex >= 0 && rowIndex < _records.Count)
            {
                var rec = _records[rowIndex];
                _converter?.ZoomToRebar(rec.ModelObject);
            }
        }

        private void ConvertSelectedGridRow()
        {
            if (_grid.CurrentRow == null) return;
            int rowIndex = _grid.CurrentRow.Index;
            if (rowIndex >= 0 && rowIndex < _records.Count)
            {
                var rec = _records[rowIndex];
                bool ok = _converter.ConvertRebar(rec, _chkOnlySixPoints.Checked);
                if (ok)
                {
                    if (_chkAutoCommit.Checked) _model.CommitChanges();

                    _grid.Rows[rowIndex].Cells["NewPts"].Value = rec.NewPointCount;
                    _grid.Rows[rowIndex].Cells["Status"].Value = rec.Status;
                    _undoStack.Push(new List<RebarConversionRecord> { rec });
                    _btnUndo.Enabled = true;
                    _lblStatus.Text = $"Đã chuyển đổi thành công Rebar ID {rec.Id}.";
                }
                else
                {
                    _grid.Rows[rowIndex].Cells["Status"].Value = rec.Status;
                }
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _records.Count)
            {
                var rec = _records[e.RowIndex];
                _converter?.ZoomToRebar(rec.ModelObject);
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _records.Count)
            {
                var rec = _records[e.RowIndex];
                if (_grid.Columns[e.ColumnIndex].Name == "Status")
                {
                    e.CellStyle.ForeColor = rec.StatusColor;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (_grid.Columns[e.ColumnIndex].Name == "OrigPts" && rec.OriginalPointCount == 6)
                {
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.ForeColor = Color.FromArgb(37, 99, 235); // Blue
                }
                else if (_grid.Columns[e.ColumnIndex].Name == "NewPts" && rec.NewPointCount == 4)
                {
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105); // Green
                }
            }
        }
    }
}
