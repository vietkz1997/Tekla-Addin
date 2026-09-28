using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BimCommands.Tekla.ClashCheck
{
    /// <summary>
    /// Excel-style modern column filter and sort dropdown popup for DataGridView.
    /// Provides search box, distinct value checklist with counters, ascending/descending sort,
    /// and quick clear buttons.
    /// </summary>
    public class ColumnFilterPopup : Form
    {
        private readonly int _columnIndex;
        private readonly string _columnName;
        private readonly List<FilterValueItem> _allItems;
        private readonly HashSet<string> _initiallySelectedValues;

        private TextBox txtSearch;
        private CheckedListBox clbValues;
        private CheckBox chkSelectAll;
        private Button btnSortAsc;
        private Button btnSortDesc;
        private Button btnApply;
        private Button btnClearThis;
        private Button btnClearAll;

        private bool _isUpdatingCheckState = false;

        public event Action<int, bool> SortRequested;
        public event Action<int, HashSet<string>> FilterApplied;
        public event Action<int> FilterCleared;
        public event Action AllFiltersCleared;

        public class FilterValueItem
        {
            public string RawValue { get; set; }
            public string DisplayText { get; set; }
            public int Count { get; set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", DisplayText, Count);
            }
        }

        public ColumnFilterPopup(
            int columnIndex,
            string columnName,
            IEnumerable<string> columnValues,
            HashSet<string> currentFilter)
        {
            _columnIndex = columnIndex;
            _columnName = columnName;
            _initiallySelectedValues = currentFilter != null ? new HashSet<string>(currentFilter) : null;

            // Group distinct values with count
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (columnValues != null)
            {
                foreach (var val in columnValues)
                {
                    string k = val ?? string.Empty;
                    if (!counts.ContainsKey(k)) counts[k] = 0;
                    counts[k]++;
                }
            }

            _allItems = counts
                .OrderBy(kv => kv.Key)
                .Select(kv => new FilterValueItem
                {
                    RawValue = kv.Key,
                    DisplayText = string.IsNullOrEmpty(kv.Key) ? "(Trống / Blank)" : kv.Key,
                    Count = kv.Value
                })
                .ToList();

            InitializeUi();
            PopulateList(string.Empty);
        }

        private void InitializeUi()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(290, 420);
            this.BackColor = Color.FromArgb(24, 28, 36);
            this.ForeColor = Color.FromArgb(226, 232, 240);
            this.KeyPreview = true;

            // Close when clicking outside
            this.Deactivate += (s, e) => { if (!this.IsDisposed) this.Close(); };
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) this.Close();
                else if (e.KeyCode == Keys.Enter && !txtSearch.Focused)
                {
                    ApplyFilter();
                    e.Handled = true;
                }
            };

            // 1. Header Bar
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(30, 35, 45),
                Padding = new Padding(10, 0, 10, 0)
            };

            var lblTitle = new Label
            {
                Text = string.Format("🔍 Lọc: {0}", _columnName),
                ForeColor = Color.FromArgb(147, 197, 253), // Sky blue
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                AutoEllipsis = true,
                Location = new Point(10, 8),
                Size = new Size(230, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnClose = new Button
            {
                Text = "✕",
                Size = new Size(26, 24),
                Location = new Point(255, 6),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnClose);

            // 2. Sort Section
            var pnlSort = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.FromArgb(24, 28, 36),
                Padding = new Padding(8, 4, 8, 4)
            };

            btnSortAsc = CreateFlatButton("▲ Sắp xếp tăng dần (A-Z)", new Point(8, 3), new Size(132, 26), Color.FromArgb(37, 43, 56), Color.FromArgb(226, 232, 240));
            btnSortAsc.Click += (s, e) =>
            {
                SortRequested?.Invoke(_columnIndex, true);
                this.Close();
            };

            btnSortDesc = CreateFlatButton("▼ Sắp xếp giảm dần (Z-A)", new Point(146, 3), new Size(134, 26), Color.FromArgb(37, 43, 56), Color.FromArgb(226, 232, 240));
            btnSortDesc.Click += (s, e) =>
            {
                SortRequested?.Invoke(_columnIndex, false);
                this.Close();
            };

            pnlSort.Controls.Add(btnSortAsc);
            pnlSort.Controls.Add(btnSortDesc);

            // 3. Search Box & Select All
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(24, 28, 36),
                Padding = new Padding(10, 4, 10, 4)
            };

            txtSearch = new TextBox
            {
                Location = new Point(10, 4),
                Size = new Size(270, 24),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };
            // Placeholder hint text
            txtSearch.TextChanged += (s, e) => PopulateList(txtSearch.Text.Trim());

            chkSelectAll = new CheckBox
            {
                Text = "(Chọn tất cả / Select All)",
                Location = new Point(12, 32),
                Size = new Size(260, 20),
                ForeColor = Color.FromArgb(253, 224, 71), // Highlight Yellow
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Checked = true
            };
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;

            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(chkSelectAll);

            // 4. Action Buttons (Bottom)
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                BackColor = Color.FromArgb(30, 35, 45),
                Padding = new Padding(8, 8, 8, 8)
            };

            btnApply = CreateFlatButton("✅ Áp dụng", new Point(8, 8), new Size(88, 30), Color.FromArgb(37, 99, 235), Color.White);
            btnApply.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnApply.Click += (s, e) => ApplyFilter();

            btnClearThis = CreateFlatButton("🔄 Bỏ lọc cột", new Point(102, 8), new Size(88, 30), Color.FromArgb(71, 85, 105), Color.FromArgb(226, 232, 240));
            btnClearThis.Click += (s, e) =>
            {
                FilterCleared?.Invoke(_columnIndex);
                this.Close();
            };

            btnClearAll = CreateFlatButton("🧹 Xóa hết lọc", new Point(196, 8), new Size(86, 30), Color.FromArgb(51, 65, 85), Color.FromArgb(203, 213, 225));
            btnClearAll.Click += (s, e) =>
            {
                AllFiltersCleared?.Invoke();
                this.Close();
            };

            pnlBottom.Controls.Add(btnApply);
            pnlBottom.Controls.Add(btnClearThis);
            pnlBottom.Controls.Add(btnClearAll);

            // 5. Checklist Values (Center)
            clbValues = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.None,
                CheckOnClick = true,
                Font = new Font("Segoe UI", 9F),
                IntegralHeight = false
            };
            clbValues.ItemCheck += (s, e) =>
            {
                if (_isUpdatingCheckState) return;
                this.BeginInvoke(new Action(SyncSelectAllState));
            };

            this.Controls.Add(clbValues);
            this.Controls.Add(pnlSearch);
            this.Controls.Add(pnlSort);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw sleek subtle outer border
            using (var pen = new Pen(Color.FromArgb(75, 85, 99), 1.5f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void PopulateList(string search)
        {
            _isUpdatingCheckState = true;
            clbValues.BeginUpdate();
            clbValues.Items.Clear();

            var filtered = string.IsNullOrEmpty(search)
                ? _allItems
                : _allItems.Where(item => item.DisplayText.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            foreach (var item in filtered)
            {
                bool isChecked;
                if (_initiallySelectedValues == null)
                {
                    isChecked = true;
                }
                else
                {
                    isChecked = _initiallySelectedValues.Contains(item.RawValue);
                }

                clbValues.Items.Add(item, isChecked);
            }

            clbValues.EndUpdate();
            _isUpdatingCheckState = false;
            SyncSelectAllState();
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCheckState) return;
            _isUpdatingCheckState = true;

            bool targetCheck = chkSelectAll.Checked;
            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                clbValues.SetItemChecked(i, targetCheck);
            }

            _isUpdatingCheckState = false;
        }

        private void SyncSelectAllState()
        {
            if (clbValues.Items.Count == 0)
            {
                _isUpdatingCheckState = true;
                chkSelectAll.Checked = false;
                _isUpdatingCheckState = false;
                return;
            }

            int checkedCount = clbValues.CheckedItems.Count;
            _isUpdatingCheckState = true;
            if (checkedCount == clbValues.Items.Count)
            {
                chkSelectAll.CheckState = CheckState.Checked;
            }
            else if (checkedCount == 0)
            {
                chkSelectAll.CheckState = CheckState.Unchecked;
            }
            else
            {
                chkSelectAll.CheckState = CheckState.Indeterminate;
            }
            _isUpdatingCheckState = false;
        }

        private void ApplyFilter()
        {
            var selectedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                if (clbValues.GetItemChecked(i))
                {
                    if (clbValues.Items[i] is FilterValueItem item)
                    {
                        selectedSet.Add(item.RawValue);
                    }
                }
            }

            // If user checked every single item across all items (no filter restriction)
            if (selectedSet.Count == _allItems.Count && string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                FilterCleared?.Invoke(_columnIndex);
            }
            else
            {
                FilterApplied?.Invoke(_columnIndex, selectedSet);
            }

            this.Close();
        }

        private Button CreateFlatButton(string text, Point loc, Size size, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }

    /// <summary>
    /// Custom IComparer to sort DataGridView rows by column values (numeric or textual).
    /// </summary>
    public class RowComparer : System.Collections.IComparer
    {
        private readonly int _colIndex;
        private readonly bool _ascending;

        public RowComparer(int colIndex, bool ascending)
        {
            _colIndex = colIndex;
            _ascending = ascending;
        }

        public int Compare(object x, object y)
        {
            var row1 = (DataGridViewRow)x;
            var row2 = (DataGridViewRow)y;

            string v1 = row1.Cells[_colIndex].Value?.ToString() ?? string.Empty;
            string v2 = row2.Cells[_colIndex].Value?.ToString() ?? string.Empty;

            // Attempt numeric parse for numbers like Rebar ID, Length, Overlap, Size
            if (double.TryParse(v1, out double d1) && double.TryParse(v2, out double d2))
            {
                return _ascending ? d1.CompareTo(d2) : d2.CompareTo(d1);
            }

            return _ascending
                ? string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase)
                : string.Compare(v2, v1, StringComparison.OrdinalIgnoreCase);
        }
    }
}
