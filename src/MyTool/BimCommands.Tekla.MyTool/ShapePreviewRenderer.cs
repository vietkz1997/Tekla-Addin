using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BimCommands.Tekla.MyTool
{
    /// <summary>
    /// Dedicated renderer for 2D rebar shape previewing on Windows Forms canvas.
    /// Handles geometry calculation, transformations (rotation/flip), and GDI+ rendering.
    /// </summary>
    public static class ShapePreviewRenderer
    {
        public static void Render(
            Graphics g,
            int width,
            int height,
            string shapeCode,
            double a,
            double b,
            double c,
            double d,
            int cornerIdx = 0,
            bool isFlipped = false,
            double rotAngle = 0.0)
        {
            if (g == null || width <= 0 || height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            double cx = width / 2.0;
            double cy = height / 2.0;

            string code = !string.IsNullOrEmpty(shapeCode) ? shapeCode.Trim() : "1";

            using (var pen = new Pen(Color.FromArgb(96, 165, 250), 3.0f))
            using (var font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.FromArgb(226, 232, 240)))
            {
                double maxDim = Math.Max(a, Math.Max(b, 400.0));
                double scale = Math.Min((width - 80) / maxDim, (height - 50) / maxDim);
                scale = Math.Max(0.05, Math.Min(0.4, scale));

                if (code == "56" || code == "54" || code == "55" || code == "41")
                {
                    RenderClosedHoop(g, pen, font, textBrush, cx, cy, code, a, b, c, scale, cornerIdx);
                }
                else if (code == "51")
                {
                    RenderUBarWithFlanges(g, pen, font, textBrush, cx, cy, a, b, scale);
                }
                else if (code == "21" || code == "14")
                {
                    if (Math.Abs(rotAngle) > 0.001 || isFlipped)
                    {
                        RenderRotatableUBar(g, pen, font, textBrush, cx, cy, a, b, a, scale, isFlipped, rotAngle);
                    }
                    else
                    {
                        RenderStandardUBar(g, pen, font, textBrush, cx, cy, a, b, scale);
                    }
                }
                else if (code == "22")
                {
                    RenderHookedUBar(g, pen, font, textBrush, cx, cy, a, b, c, scale);
                }
                else if (code == "25")
                {
                    RenderOutwardHookedUBar(g, pen, font, textBrush, cx, cy, a, b, c, scale);
                }
                else if (code == "15")
                {
                    RenderZBar(g, pen, font, textBrush, cx, cy, a, b, c, scale);
                }
                else if (code == "3")
                {
                    RenderOpenTieBar(g, pen, font, textBrush, cx, cy, a, b, c, scale);
                }
                else if (code == "4" || code == "45")
                {
                    RenderRotatableUBar(g, pen, font, textBrush, cx, cy, a, b, c, scale, isFlipped, rotAngle);
                }
                else if (code == "11")
                {
                    RenderStraightWithEndHooks(g, pen, font, textBrush, cx, cy, a, scale);
                }
                else if (code == "31")
                {
                    RenderLBarWithHook(g, pen, font, textBrush, cx, cy, a, b, c, scale);
                }
                else if (code == "60")
                {
                    RenderDiamondHoop(g, pen, font, textBrush, cx, cy, a, b, scale);
                }
                else if (code == "62")
                {
                    RenderTriangleHoop(g, pen, font, textBrush, cx, cy, a, b, scale);
                }
                else if (code == "67")
                {
                    RenderCircularHoop(g, pen, font, textBrush, cx, cy, a, scale);
                }
                else if (code == "72")
                {
                    RenderSquareCap(g, pen, font, textBrush, cx, cy, a, b, scale);
                }
                else if (code == "74")
                {
                    RenderHatBar(g, pen, font, textBrush, cx, cy, a, b, c, d, scale);
                }
                else if (code == "2" || code == "12")
                {
                    RenderLBar(g, pen, font, textBrush, cx, cy, a, b, scale);
                }
                else if (code == "1" || code == "102")
                {
                    RenderStraightBar(g, pen, font, textBrush, cx, cy, a, scale, width);
                }
                else if (code == "370")
                {
                    RenderOverlappingRing(g, pen, font, textBrush, cx, cy, a, b, c, scale);
                }
                else
                {
                    RenderGenericShape(g, pen, font, textBrush, cx, cy, code, a, b, scale, width, height);
                }
            }
        }

        private static void RenderClosedHoop(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, string code, double a, double b, double c, double scale, int cornerIdx)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawRectangle(pen, x, y, (float)sw, (float)sh);

            if (code == "56")
            {
                float hookLen = (float)(c * scale);
                float hd = hookLen * 0.707f;
                if (cornerIdx == 0) // Top-Right (Standard STShape DWG)
                {
                    g.DrawLine(pen, x + (float)sw, y, x + (float)sw - hd, y + hd);
                    g.DrawLine(pen, x + (float)sw - 4, y, x + (float)sw - hd - 4, y + hd);
                }
                else if (cornerIdx == 1) // Bottom-Right
                {
                    g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw - hd, y + (float)sh - hd);
                    g.DrawLine(pen, x + (float)sw - 4, y + (float)sh, x + (float)sw - hd - 4, y + (float)sh - hd);
                }
                else if (cornerIdx == 2) // Bottom-Left
                {
                    g.DrawLine(pen, x, y + (float)sh, x + hd, y + (float)sh - hd);
                    g.DrawLine(pen, x + 4, y + (float)sh, x + hd + 4, y + (float)sh - hd);
                }
                else // Top-Left
                {
                    g.DrawLine(pen, x, y, x + hd, y + hd);
                    g.DrawLine(pen, x + 4, y, x + hd + 4, y + hd);
                }
            }

            g.DrawString($"A={a:0}", font, textBrush, (float)(x - 30), (float)(cy - 8));
            g.DrawString($"B={b:0}", font, textBrush, (float)(cx - 20), (float)(y + sh + 4));
            if (code == "56" || code == "54" || code == "55")
                g.DrawString($"C={c:0}", font, textBrush, (float)(x + sw - 20), (float)(y - 18));
        }

        private static void RenderUBarWithFlanges(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw, y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 20), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
        }

        private static void RenderStandardUBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw, y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 20), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
        }

        private static void RenderHookedUBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            double sc = c * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, (float)(x + sc), y, x, y);
            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw, y);
            g.DrawLine(pen, x + (float)sw, y, (float)(x + sw - sc), y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
            g.DrawString($"C={c:0}", font, textBrush, (float)(cx - 15), (float)(y - 16));
        }

        private static void RenderOutwardHookedUBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            double sc = c * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, (float)(x - sc), y, x, y);
            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw, y);
            g.DrawLine(pen, x + (float)sw, y, (float)(x + sw + sc), y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
            g.DrawString($"C={c:0}", font, textBrush, (float)(x - sc - 25), (float)(y - 16));
        }

        private static void RenderZBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale)
        {
            double sw = b * scale;
            double sh = a * scale;
            double sc = c * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, (float)(x - sc), y + (float)sh, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x, y);
            g.DrawLine(pen, x, y, (float)(x + sc), y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(x + 5), (float)(cy - 8));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - sc), (float)(y + sh + 4));
        }

        private static void RenderOpenTieBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            double sc = c * scale;
            float x = (float)(cx - (sw + sc) / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw, y);
            g.DrawLine(pen, x + (float)sw, y, x + (float)(sw + sc), y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(x + sw / 2.0 - 15), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x + sw + 4), (float)(cy - 8));
            g.DrawString($"C={c:0}", font, textBrush, (float)(x + sw + sc / 2.0 - 10), (float)(y - 16));
        }

        /// <summary>
        /// Transforms 2D points by rotating around a given center point by an arbitrary angle using GDI+ Matrix.
        /// Allows continuous arbitrary angle rotation (0° to 360°) for any polygon or label array.
        /// </summary>
        public static void TransformPointsByRotation(PointF[] points, PointF center, float angleDegrees)
        {
            if (points == null || points.Length == 0) return;
            using (var matrix = new Matrix())
            {
                matrix.RotateAt(angleDegrees, center);
                matrix.TransformPoints(points);
            }
        }

        private static void RenderRotatableUBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale, bool isFlipped, double rotAngle)
        {
            double sw = b * scale;
            double shA = a * scale;
            double shC = c * scale;
            double halfH = Math.Max(shA, shC) / 2.0;

            // Normalize angle to [0, 360) and handle horizontal flip
            double effAngle = ((rotAngle % 360.0) + 360.0) % 360.0;
            if (isFlipped) effAngle = (effAngle + 180.0) % 360.0;

            // 1. Define base geometry at 0° (U-bar facing up, bottom web centered at cy + halfH)
            PointF[] barPoints = new PointF[]
            {
                new PointF((float)(cx - sw / 2.0), (float)(cy + halfH - shA)), // Top-Left (end of leg A)
                new PointF((float)(cx - sw / 2.0), (float)(cy + halfH)),       // Bottom-Left (corner A-B)
                new PointF((float)(cx + sw / 2.0), (float)(cy + halfH)),       // Bottom-Right (corner B-C)
                new PointF((float)(cx + sw / 2.0), (float)(cy + halfH - shC))  // Top-Right (end of leg C)
            };

            // 2. Define dimension label anchor points at 0°
            PointF[] labelPoints = new PointF[]
            {
                new PointF((float)(cx - sw / 2.0 - 24.0), (float)(cy + halfH - shA / 2.0)), // Dim A
                new PointF((float)cx, (float)(cy + halfH + 16.0)),                          // Dim B
                new PointF((float)(cx + sw / 2.0 + 24.0), (float)(cy + halfH - shC / 2.0))  // Dim C
            };

            // 3. Apply System.Drawing.Drawing2D.Matrix rotation around center (cx, cy)
            PointF centerPt = new PointF((float)cx, (float)cy);
            TransformPointsByRotation(barPoints, centerPt, (float)effAngle);
            TransformPointsByRotation(labelPoints, centerPt, (float)effAngle);

            // 4. Draw rotated continuous polyline
            g.DrawLines(pen, barPoints);

            // 5. Draw upright dimension text labels at transformed anchor positions
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString($"A={a:0}", font, textBrush, labelPoints[0].X, labelPoints[0].Y, sf);
                g.DrawString($"B={b:0}", font, textBrush, labelPoints[1].X, labelPoints[1].Y, sf);
                g.DrawString($"C={c:0}", font, textBrush, labelPoints[2].X, labelPoints[2].Y, sf);
            }
        }

        private static void RenderStraightWithEndHooks(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double scale)
        {
            double sw = a * scale;
            float x1 = (float)(cx - sw / 2.0);
            float x2 = (float)(cx + sw / 2.0);
            g.DrawLine(pen, x1, (float)cy, x2, (float)cy);
            g.DrawArc(pen, x1 - 16, (float)(cy - 12), 16, 24, 90, 180);
            g.DrawArc(pen, x2, (float)(cy - 12), 16, 24, 270, 180);
            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 20), (float)(cy + 8));
        }

        private static void RenderLBarWithHook(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            float hd = (float)(c * scale * 0.707);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw - hd, y + (float)sh - hd);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
        }

        private static void RenderDiamondHoop(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            PointF[] pts = new PointF[]
            {
                new PointF((float)cx, (float)(cy - sh / 2.0)),
                new PointF((float)(cx + sw / 2.0), (float)cy),
                new PointF((float)cx, (float)(cy + sh / 2.0)),
                new PointF((float)(cx - sw / 2.0), (float)cy)
            };
            g.DrawPolygon(pen, pts);
            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(cy + sh / 2.0 + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(cx + sw / 2.0 + 4), (float)(cy - 8));
        }

        private static void RenderTriangleHoop(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            PointF[] pts = new PointF[]
            {
                new PointF((float)cx, (float)(cy - sh / 2.0)),
                new PointF((float)(cx + sw / 2.0), (float)(cy + sh / 2.0)),
                new PointF((float)(cx - sw / 2.0), (float)(cy + sh / 2.0))
            };
            g.DrawPolygon(pen, pts);
            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(cy + sh / 2.0 + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(cx - 45), (float)(cy - 8));
        }

        private static void RenderCircularHoop(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double scale)
        {
            double dia = a * scale;
            g.DrawEllipse(pen, (float)(cx - dia / 2.0), (float)(cy - dia / 2.0), (float)dia, (float)dia);
            g.DrawString($"D={a:0}", font, textBrush, (float)(cx - 15), (float)(cy + dia / 2.0 + 4));
        }

        private static void RenderSquareCap(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, x, y, x + (float)sw, y);
            g.DrawLine(pen, x + (float)sw, y, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x, y + (float)sh);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y - 16));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x + sw + 4), (float)(cy - 8));
        }

        private static void RenderHatBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double d, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, (float)(x - c * scale), y + (float)sh, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x, y);
            g.DrawLine(pen, x, y, x + (float)sw, y);
            g.DrawLine(pen, x + (float)sw, y, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, (float)(x + sw + d * scale), y + (float)sh);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y - 16));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
        }

        private static void RenderLBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x - 45), (float)(cy - 8));
        }

        private static void RenderStraightBar(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double scale, int width)
        {
            double sw = Math.Min(a * scale, width - 60);
            float x1 = (float)(cx - sw / 2.0);
            float x2 = (float)(cx + sw / 2.0);
            g.DrawLine(pen, x1, (float)cy, x2, (float)cy);
            g.DrawString($"L={a:0}", font, textBrush, (float)(cx - 20), (float)(cy + 8));
        }

        private static void RenderOverlappingRing(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, double a, double b, double c, double scale)
        {
            double sw = a * scale;
            double sh = b * scale;
            double sc = c * scale;
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);

            g.DrawLine(pen, (float)(x + sc), y, x, y);
            g.DrawLine(pen, x, y, x, y + (float)sh);
            g.DrawLine(pen, x, y + (float)sh, x + (float)sw, y + (float)sh);
            g.DrawLine(pen, x + (float)sw, y + (float)sh, x + (float)sw, y);
            g.DrawLine(pen, x + (float)sw, y, (float)(x + sw - sc), y);

            g.DrawString($"A={a:0}", font, textBrush, (float)(cx - 15), (float)(y + sh + 4));
            g.DrawString($"B={b:0}", font, textBrush, (float)(x + sw + 4), (float)(cy - 8));
        }

        private static void RenderGenericShape(
            Graphics g, Pen pen, Font font, Brush textBrush,
            double cx, double cy, string code, double a, double b, double scale, int width, int height)
        {
            double sw = Math.Min(a * scale, width - 80);
            double sh = Math.Min(b * scale, height - 50);
            float x = (float)(cx - sw / 2.0);
            float y = (float)(cy - sh / 2.0);
            g.DrawRectangle(pen, x, y, (float)sw, (float)sh);
            g.DrawString($"Shape {code}", font, textBrush, (float)(cx - 30), (float)(cy - 6));
        }
    }
}
