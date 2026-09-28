using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BimCommands.Tekla.IssueTracker.Forms
{
    public class ImageMarkupForm : Form
    {
        private enum ToolType { Pen, Rectangle, Cloud, Arrow, Text }

        private abstract class DrawAction
        {
            public Color ActionColor;
            public float Thickness;
            public abstract void Draw(Graphics g);
        }

        private class PenAction : DrawAction
        {
            public List<Point> Points = new List<Point>();
            public override void Draw(Graphics g)
            {
                if (Points.Count < 2) return;
                using (Pen p = new Pen(ActionColor, Thickness))
                {
                    p.LineJoin = LineJoin.Round;
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawLines(p, Points.ToArray());
                }
            }
        }

        private class RectAction : DrawAction
        {
            public Rectangle Rect;
            public override void Draw(Graphics g)
            {
                using (Pen p = new Pen(ActionColor, Thickness))
                {
                    g.DrawRectangle(p, Rect);
                }
            }
        }

        private class ArrowAction : DrawAction
        {
            public Point Start;
            public Point End;
            public override void Draw(Graphics g)
            {
                using (Pen p = new Pen(ActionColor, Thickness))
                {
                    using (AdjustableArrowCap cap = new AdjustableArrowCap(5, 5))
                    {
                        p.CustomEndCap = cap;
                        g.DrawLine(p, Start, End);
                    }
                }
            }
        }

        private class CloudAction : DrawAction
        {
            public Rectangle Rect;
            public override void Draw(Graphics g)
            {
                if (Rect.Width < 10 || Rect.Height < 10) return;
                using (Pen p = new Pen(ActionColor, Thickness))
                {
                    // Draw a series of connected arcs to form a revision cloud
                    int r = 16;
                    int x = Rect.X;
                    int y = Rect.Y;
                    int w = Rect.Width;
                    int h = Rect.Height;

                    for (int curX = x; curX < x + w; curX += r)
                        g.DrawArc(p, curX, y - r / 2, r, r, 180, 180);
                    for (int curY = y; curY < y + h; curY += r)
                        g.DrawArc(p, x + w - r / 2, curY, r, r, 270, 180);
                    for (int curX = x + w; curX > x; curX -= r)
                        g.DrawArc(p, curX - r, y + h - r / 2, r, r, 0, 180);
                    for (int curY = y + h; curY > y; curY -= r)
                        g.DrawArc(p, x - r / 2, curY - r, r, r, 90, 180);
                }
            }
        }

        private class TextAction : DrawAction
        {
            public Point Location;
            public string Text;
            public override void Draw(Graphics g)
            {
                if (string.IsNullOrEmpty(Text)) return;
                using (Font font = new Font("Segoe UI", 11f, FontStyle.Bold))
                {
                    SizeF sz = g.MeasureString(Text, font);
                    Rectangle bgRect = new Rectangle(Location.X, Location.Y, (int)sz.Width + 10, (int)sz.Height + 6);
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
                    using (Pen borderPen = new Pen(ActionColor, 1.5f))
                    using (SolidBrush textBrush = new SolidBrush(ActionColor))
                    {
                        g.FillRectangle(bgBrush, bgRect);
                        g.DrawRectangle(borderPen, bgRect);
                        g.DrawString(Text, font, textBrush, Location.X + 5, Location.Y + 3);
                    }
                }
            }
        }

        private Bitmap _baseImage;
        private List<DrawAction> _actions = new List<DrawAction>();
        private DrawAction _currentAction;
        private Point _startPoint;
        private bool _isDrawing = false;

        private ToolType _activeTool = ToolType.Rectangle;
        private Color _activeColor = Color.FromArgb(239, 68, 68); // Red
        private float _activeThickness = 3f;

        private Panel pnlCanvas;
        public Bitmap ResultImage { get; private set; }

        public ImageMarkupForm(Bitmap sourceImage)
        {
            _baseImage = new Bitmap(sourceImage);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "✏️ Chỉnh Sửa & Vẽ Chú Thích Lỗi (Image Markup Editor)";
            Size = new Size(Math.Min(1200, _baseImage.Width + 60), Math.Min(800, _baseImage.Height + 140));
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(241, 245, 249);
            Font = new Font("Segoe UI", 9f);

            // Top Toolbar
            ToolStrip ts = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(6),
                ImageScalingSize = new Size(20, 20)
            };

            ToolStripButton btnRect = new ToolStripButton("🔲 Khung Vuông") { ForeColor = Color.White, Checked = true };
            ToolStripButton btnCloud = new ToolStripButton("☁️ Đám Mây") { ForeColor = Color.White };
            ToolStripButton btnArrow = new ToolStripButton("➡️ Mũi Tên") { ForeColor = Color.White };
            ToolStripButton btnPen = new ToolStripButton("✏️ Bút Vẽ") { ForeColor = Color.White };
            ToolStripButton btnText = new ToolStripButton("🔤 Thêm Chữ") { ForeColor = Color.White };

            btnRect.Click += (s, e) => SetTool(ToolType.Rectangle, btnRect, ts);
            btnCloud.Click += (s, e) => SetTool(ToolType.Cloud, btnCloud, ts);
            btnArrow.Click += (s, e) => SetTool(ToolType.Arrow, btnArrow, ts);
            btnPen.Click += (s, e) => SetTool(ToolType.Pen, btnPen, ts);
            btnText.Click += (s, e) => SetTool(ToolType.Text, btnText, ts);

            ts.Items.Add(btnRect);
            ts.Items.Add(btnCloud);
            ts.Items.Add(btnArrow);
            ts.Items.Add(btnPen);
            ts.Items.Add(btnText);
            ts.Items.Add(new ToolStripSeparator());

            // Colors
            Color[] colors = new Color[]
            {
                Color.FromArgb(239, 68, 68), // Red
                Color.FromArgb(245, 158, 11), // Amber/Yellow
                Color.FromArgb(16, 185, 129), // Emerald
                Color.FromArgb(59, 130, 246), // Blue
                Color.White
            };

            foreach (var col in colors)
            {
                ToolStripButton cBtn = new ToolStripButton("  ")
                {
                    BackColor = col,
                    Margin = new Padding(2)
                };
                cBtn.Click += (s, e) => _activeColor = col;
                ts.Items.Add(cBtn);
            }

            ts.Items.Add(new ToolStripSeparator());

            ToolStripButton btnUndo = new ToolStripButton("↩️ Hoàn tác") { ForeColor = Color.White };
            btnUndo.Click += (s, e) =>
            {
                if (_actions.Count > 0)
                {
                    _actions.RemoveAt(_actions.Count - 1);
                    pnlCanvas.Invalidate();
                }
            };
            ts.Items.Add(btnUndo);

            ToolStripButton btnClear = new ToolStripButton("🗑️ Xóa hết") { ForeColor = Color.White };
            btnClear.Click += (s, e) =>
            {
                _actions.Clear();
                pnlCanvas.Invalidate();
            };
            ts.Items.Add(btnClear);

            ToolStripButton btnSave = new ToolStripButton("💾 LƯU CHÚ THÍCH")
            {
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(15, 0, 0, 0)
            };
            btnSave.Click += (s, e) =>
            {
                ResultImage = RenderFinalImage();
                DialogResult = DialogResult.OK;
                Close();
            };
            ts.Items.Add(btnSave);

            // Canvas Panel inside a Scrollable container
            Panel pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            pnlCanvas = new Panel
            {
                Size = _baseImage.Size,
                Location = new Point(10, 10),
                BackColor = Color.Black
            };
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, pnlCanvas, new object[] { true });

            pnlCanvas.Paint += PnlCanvas_Paint;
            pnlCanvas.MouseDown += PnlCanvas_MouseDown;
            pnlCanvas.MouseMove += PnlCanvas_MouseMove;
            pnlCanvas.MouseUp += PnlCanvas_MouseUp;

            pnlScroll.Controls.Add(pnlCanvas);

            Controls.Add(pnlScroll);
            Controls.Add(ts);
        }

        private void SetTool(ToolType tool, ToolStripButton activeBtn, ToolStrip ts)
        {
            _activeTool = tool;
            foreach (ToolStripItem item in ts.Items)
            {
                if (item is ToolStripButton btn && btn.Tag == null)
                {
                    btn.Checked = (btn == activeBtn);
                }
            }
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw base image
            g.DrawImage(_baseImage, 0, 0);

            // Draw all completed actions
            foreach (var action in _actions)
            {
                action.Draw(g);
            }

            // Draw current in-progress action
            if (_isDrawing && _currentAction != null)
            {
                _currentAction.Draw(g);
            }
        }

        private void PnlCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDrawing = true;
                _startPoint = e.Location;

                switch (_activeTool)
                {
                    case ToolType.Pen:
                        var penAct = new PenAction { ActionColor = _activeColor, Thickness = _activeThickness };
                        penAct.Points.Add(e.Location);
                        _currentAction = penAct;
                        break;
                    case ToolType.Rectangle:
                        _currentAction = new RectAction { ActionColor = _activeColor, Thickness = _activeThickness, Rect = new Rectangle(e.Location, new Size(0, 0)) };
                        break;
                    case ToolType.Cloud:
                        _currentAction = new CloudAction { ActionColor = _activeColor, Thickness = _activeThickness, Rect = new Rectangle(e.Location, new Size(0, 0)) };
                        break;
                    case ToolType.Arrow:
                        _currentAction = new ArrowAction { ActionColor = _activeColor, Thickness = _activeThickness + 1, Start = e.Location, End = e.Location };
                        break;
                    case ToolType.Text:
                        using (var inputDlg = new Form())
                        {
                            inputDlg.Text = "Nhập văn bản ghi chú lỗi";
                            inputDlg.Size = new Size(380, 150);
                            inputDlg.StartPosition = FormStartPosition.CenterParent;
                            inputDlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                            inputDlg.MaximizeBox = false;
                            inputDlg.MinimizeBox = false;

                            TextBox tb = new TextBox { Location = new Point(15, 20), Width = 330, Font = new Font("Segoe UI", 10f) };
                            Button ok = new Button { Text = "Xác nhận", DialogResult = DialogResult.OK, Location = new Point(255, 60), Size = new Size(90, 30) };
                            inputDlg.Controls.Add(tb);
                            inputDlg.Controls.Add(ok);
                            inputDlg.AcceptButton = ok;

                            if (inputDlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(tb.Text))
                            {
                                _actions.Add(new TextAction { ActionColor = _activeColor, Thickness = _activeThickness, Location = e.Location, Text = tb.Text.Trim() });
                                pnlCanvas.Invalidate();
                            }
                        }
                        _isDrawing = false;
                        break;
                }
            }
        }

        private void PnlCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing && _currentAction != null)
            {
                if (_currentAction is PenAction penAct)
                {
                    penAct.Points.Add(e.Location);
                }
                else if (_currentAction is RectAction rectAct)
                {
                    int x = Math.Min(_startPoint.X, e.X);
                    int y = Math.Min(_startPoint.Y, e.Y);
                    int w = Math.Abs(_startPoint.X - e.X);
                    int h = Math.Abs(_startPoint.Y - e.Y);
                    rectAct.Rect = new Rectangle(x, y, w, h);
                }
                else if (_currentAction is CloudAction cloudAct)
                {
                    int x = Math.Min(_startPoint.X, e.X);
                    int y = Math.Min(_startPoint.Y, e.Y);
                    int w = Math.Abs(_startPoint.X - e.X);
                    int h = Math.Abs(_startPoint.Y - e.Y);
                    cloudAct.Rect = new Rectangle(x, y, w, h);
                }
                else if (_currentAction is ArrowAction arrowAct)
                {
                    arrowAct.End = e.Location;
                }
                pnlCanvas.Invalidate();
            }
        }

        private void PnlCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDrawing && _currentAction != null)
            {
                _actions.Add(_currentAction);
                _currentAction = null;
                _isDrawing = false;
                pnlCanvas.Invalidate();
            }
        }

        public Bitmap RenderFinalImage()
        {
            Bitmap bmp = new Bitmap(_baseImage.Width, _baseImage.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(_baseImage, 0, 0);
                foreach (var action in _actions)
                {
                    action.Draw(g);
                }
            }
            return bmp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _baseImage != null)
            {
                _baseImage.Dispose();
                _baseImage = null;
            }
            base.Dispose(disposing);
        }
    }
}
