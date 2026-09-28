using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BimCommands.EnvironmentConverter.Models;

namespace BimCommands.EnvironmentConverter.UI
{
    public class MappingDialog : Form
    {
        private DataGridView dgvRules;
        private Button btnAddRule;
        private Button btnResetDefault;
        private Button btnOk;
        private Button btnCancel;
        private Label lblHeader;

        public List<MaterialMappingRule> CurrentRules { get; private set; }

        public MappingDialog(List<MaterialMappingRule> initialRules)
        {
            CurrentRules = initialRules ?? new List<MaterialMappingRule>();
            InitializeComponent();
            PopulateGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Bảng Ánh Xạ Vật Liệu & Mác Thép (Korea → Vietnam)";
            this.Size = new Size(720, 520);
            this.MinimumSize = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(245, 247, 250);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(24, 43, 73),
                Padding = new Padding(12)
            };

            lblHeader = new Label
            {
                Text = "CẤU HÌNH ÁNH XẠ VẬT LIỆU & MÁC THÉP (MAPPING RULES)\nTùy chỉnh mác gốc từ Korea sang mác tiêu chuẩn Vietnam (TCVN)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
            topPanel.Controls.Add(lblHeader);
            this.Controls.Add(topPanel);

            // DataGridView
            dgvRules = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Setup columns
            var colEnabled = new DataGridViewCheckBoxColumn
            {
                Name = "colEnabled",
                HeaderText = "Bật",
                FillWeight = 30
            };
            var colCategory = new DataGridViewTextBoxColumn
            {
                Name = "colCategory",
                HeaderText = "Phân loại",
                ReadOnly = true,
                FillWeight = 60
            };
            var colSource = new DataGridViewTextBoxColumn
            {
                Name = "colSource",
                HeaderText = "Mác gốc (Korea)",
                ReadOnly = true,
                FillWeight = 80
            };
            var colTarget = new DataGridViewTextBoxColumn
            {
                Name = "colTarget",
                HeaderText = "Mác đích (Vietnam/TCVN)",
                FillWeight = 90
            };
            var colDesc = new DataGridViewTextBoxColumn
            {
                Name = "colDesc",
                HeaderText = "Ghi chú",
                ReadOnly = true,
                FillWeight = 110
            };

            dgvRules.Columns.AddRange(colEnabled, colCategory, colSource, colTarget, colDesc);
            this.Controls.Add(dgvRules);
            dgvRules.BringToFront();

            // Bottom Panel
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            btnResetDefault = new Button
            {
                Text = "Mặc định Korea → VN",
                Location = new Point(12, 10),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 8.5F)
            };
            btnResetDefault.Click += (s, e) =>
            {
                CurrentRules = Core.MaterialMapper.GetDefaultKoreaToVietnamRules();
                PopulateGrid();
            };

            btnAddRule = new Button
            {
                Text = "Thêm dòng mới",
                Location = new Point(180, 10),
                Size = new Size(110, 30),
                Font = new Font("Segoe UI", 8.5F)
            };
            btnAddRule.Click += BtnAddRule_Click;

            btnOk = new Button
            {
                Text = "Áp dụng",
                Location = new Point(510, 10),
                Size = new Size(95, 30),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button
            {
                Text = "Đóng",
                Location = new Point(615, 10),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            bottomPanel.Controls.Add(btnResetDefault);
            bottomPanel.Controls.Add(btnAddRule);
            bottomPanel.Controls.Add(btnOk);
            bottomPanel.Controls.Add(btnCancel);
            this.Controls.Add(bottomPanel);
        }

        private void PopulateGrid()
        {
            dgvRules.Rows.Clear();
            foreach (var r in CurrentRules)
            {
                string catName = r.Category == MappingCategory.Rebar ? "Cốt thép (Rebar)" :
                                 r.Category == MappingCategory.SteelPart ? "Thép kết cấu" : "Bê tông";
                int idx = dgvRules.Rows.Add(r.IsEnabled, catName, r.SourceValue, r.TargetValue, r.Description);
                dgvRules.Rows[idx].Tag = r;
            }
        }

        private void BtnAddRule_Click(object sender, EventArgs e)
        {
            string source = ShowInputPrompt("Nhập tên mác gốc cần ánh xạ (ví dụ: SD400, SS400):", "Thêm quy tắc mới", "");
            if (string.IsNullOrWhiteSpace(source)) return;

            string target = ShowInputPrompt("Nhập tên mác đích sau chuyển đổi (ví dụ: CB400-V, Q235B):", "Thêm quy tắc mới", source);
            if (string.IsNullOrWhiteSpace(target)) return;

            var newRule = new MaterialMappingRule(MappingCategory.Rebar, source.Trim(), target.Trim(), "Người dùng thêm thủ công");
            CurrentRules.Insert(0, newRule);
            PopulateGrid();
        }

        private static string ShowInputPrompt(string text, string caption, string defaultValue)
        {
            using (var prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 160;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = caption;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                var textLabel = new Label() { Left = 16, Top = 16, Text = text, AutoSize = true };
                var textBox = new TextBox() { Left = 16, Top = 42, Width = 350, Text = defaultValue };
                var confirmation = new Button() { Text = "OK", Left = 210, Width = 75, Top = 80, DialogResult = DialogResult.OK };
                var cancel = new Button() { Text = "Hủy", Left = 290, Width = 75, Top = 80, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            // Sync values from grid back to CurrentRules
            for (int i = 0; i < dgvRules.Rows.Count; i++)
            {
                var row = dgvRules.Rows[i];
                if (row.Tag is MaterialMappingRule rule)
                {
                    rule.IsEnabled = Convert.ToBoolean(row.Cells["colEnabled"].Value);
                    rule.TargetValue = Convert.ToString(row.Cells["colTarget"].Value)?.Trim();
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
