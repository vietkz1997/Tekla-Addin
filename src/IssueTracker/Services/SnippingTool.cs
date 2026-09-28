using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BimCommands.Tekla.IssueTracker.Services
{
    public class SnippingTool : Form
    {
        private Bitmap _fullScreenshot;
        private Point _startPoint;
        private Rectangle _selectRect;
        private bool _isSelecting = false;
        public Bitmap CapturedImage { get; private set; }

        public static Bitmap Snip()
        {
            using (var tool = new SnippingTool())
            {
                if (tool.ShowDialog() == DialogResult.OK)
                {
                    return tool.CapturedImage;
                }
            }
            return null;
        }

        public SnippingTool()
        {
            // Capture all screens combined
            int left = SystemInformation.VirtualScreen.Left;
            int top = SystemInformation.VirtualScreen.Top;
            int width = SystemInformation.VirtualScreen.Width;
            int height = SystemInformation.VirtualScreen.Height;

            _fullScreenshot = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(_fullScreenshot))
            {
                g.CopyFromScreen(left, top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(left, top);
            Size = new Size(width, height);
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Draw dimmed full screenshot
            g.DrawImage(_fullScreenshot, 0, 0);

            // Draw semi-transparent dark overlay
            using (SolidBrush darkBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                g.FillRectangle(darkBrush, 0, 0, Width, Height);
            }

            if (_isSelecting && _selectRect.Width > 0 && _selectRect.Height > 0)
            {
                // Clear the selected area to reveal the original crisp screenshot
                g.SetClip(_selectRect);
                g.DrawImage(_fullScreenshot, 0, 0);
                g.ResetClip();

                // Draw bright red border and dimension callout
                using (Pen pen = new Pen(Color.FromArgb(239, 68, 68), 2f))
                {
                    pen.DashStyle = DashStyle.Solid;
                    g.DrawRectangle(pen, _selectRect);
                }

                // Draw size badge
                string dimText = $"{_selectRect.Width} x {_selectRect.Height} px";
                using (Font font = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.White))
                using (Brush badgeBg = new SolidBrush(Color.FromArgb(220, 15, 23, 42)))
                {
                    SizeF sz = g.MeasureString(dimText, font);
                    Rectangle badgeRect = new Rectangle(_selectRect.Left, _selectRect.Bottom + 4, (int)sz.Width + 10, (int)sz.Height + 4);
                    g.FillRectangle(badgeBg, badgeRect);
                    g.DrawString(dimText, font, textBrush, badgeRect.X + 5, badgeRect.Y + 2);
                }
            }
            else if (!_isSelecting)
            {
                // Instruction hint in the top-center
                string hint = "📸 Kéo thả chuột để chọn vùng chụp màn hình  |  Nhấn [ESC] để hủy";
                using (Font font = new Font("Segoe UI", 11f, FontStyle.Bold))
                using (Brush badgeBg = new SolidBrush(Color.FromArgb(220, 15, 23, 42)))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    SizeF sz = g.MeasureString(hint, font);
                    int hx = (Width - (int)sz.Width) / 2;
                    Rectangle hintRect = new Rectangle(hx - 15, 20, (int)sz.Width + 30, (int)sz.Height + 10);
                    g.FillRectangle(badgeBg, hintRect);
                    using (Pen bPen = new Pen(Color.FromArgb(59, 130, 246), 2f))
                    {
                        g.DrawRectangle(bPen, hintRect);
                    }
                    g.DrawString(hint, font, textBrush, hx, 25);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _startPoint = e.Location;
                _selectRect = new Rectangle(e.Location, new Size(0, 0));
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isSelecting)
            {
                int x = Math.Min(_startPoint.X, e.X);
                int y = Math.Min(_startPoint.Y, e.Y);
                int w = Math.Abs(_startPoint.X - e.X);
                int h = Math.Abs(_startPoint.Y - e.Y);
                _selectRect = new Rectangle(x, y, w, h);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                if (_selectRect.Width > 5 && _selectRect.Height > 5)
                {
                    CapturedImage = new Bitmap(_selectRect.Width, _selectRect.Height);
                    using (Graphics g = Graphics.FromImage(CapturedImage))
                    {
                        g.DrawImage(_fullScreenshot,
                            new Rectangle(0, 0, _selectRect.Width, _selectRect.Height),
                            _selectRect,
                            GraphicsUnit.Pixel);
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Invalidate();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _fullScreenshot != null)
            {
                _fullScreenshot.Dispose();
                _fullScreenshot = null;
            }
            base.Dispose(disposing);
        }
    }
}
