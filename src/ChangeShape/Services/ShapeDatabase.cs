using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ChangeShape.Models;

namespace ChangeShape.Services
{
    public static class ShapeDatabase
    {
        private static readonly List<RebarShapeDefinition> _shapes = new();

        static ShapeDatabase()
        {
            InitializeShapes();
        }

        public static IReadOnlyList<RebarShapeDefinition> AllShapes => _shapes;

        public static RebarShapeDefinition? GetShape(string shapeId)
        {
            return _shapes.FirstOrDefault(s => string.Equals(s.ShapeId, shapeId, StringComparison.OrdinalIgnoreCase))
                   ?? _shapes.FirstOrDefault(s => s.ShapeId == "56");
        }

        private static void InitializeShapes()
        {
            // =========================================================================
            // SHAPE 56 - ĐAI KÍN MÓC 135° TIÊU CHUẨN (CLOSED STIRRUP)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "56",
                Name = "Shape 56 - Đai Kín 135° Hai Bên",
                Category = "Đai Kín (Stirrup)",
                Description = "Đai kín dầm / cột tiêu chuẩn SIDD-SHOP Hàn Quốc, 2 tai móc 135 độ.",
                FormulaDisplay = "L = 2×(A + B) + 2×C",
                ParamCount = 3,
                ParamNames = new[] { "A (Rộng mm)", "B (Cao mm)", "C (Móc 135° mm)" },
                DefaultValues = new[] { 600.0, 400.0, 120.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => 2.0 * (a + b) + 2.0 * c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) =>
                {
                    // Center at origin. Top-Right corner has the two 135-deg hooks.
                    double hw = a / 2.0;
                    double hh = b / 2.0;
                    double cos45 = Math.Cos(Math.PI / 4.0);
                    double sin45 = Math.Sin(Math.PI / 4.0);

                    var pts = new List<Point>
                    {
                        // Hook 1 start (inside Top-Right)
                        new Point(hw - c * cos45, hh - c * sin45),
                        // Corner Top-Right
                        new Point(hw, hh),
                        // Corner Top-Left
                        new Point(-hw, hh),
                        // Corner Bottom-Left
                        new Point(-hw, -hh),
                        // Corner Bottom-Right
                        new Point(hw, -hh),
                        // Back to Top-Right
                        new Point(hw, hh),
                        // Hook 2 end
                        new Point(hw - c * sin45, hh - c * cos45)
                    };
                    return pts;
                }
            });

            // =========================================================================
            // SHAPE 54 - ĐAI KÍN MÓC VUÔNG 90°
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "54",
                Name = "Shape 54 - Đai Kín Móc 90°",
                Category = "Đai Kín (Stirrup)",
                Description = "Đai kín móc vuông 90 độ ở góc trên.",
                FormulaDisplay = "L = 2×(A + B) + 2×C",
                ParamCount = 3,
                ParamNames = new[] { "A (Rộng mm)", "B (Cao mm)", "C (Móc 90° mm)" },
                DefaultValues = new[] { 500.0, 400.0, 100.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => 2.0 * (a + b) + 2.0 * c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) =>
                {
                    double hw = a / 2.0;
                    double hh = b / 2.0;
                    return new List<Point>
                    {
                        new Point(hw, hh - c),
                        new Point(hw, hh),
                        new Point(-hw, hh),
                        new Point(-hw, -hh),
                        new Point(hw, -hh),
                        new Point(hw, hh),
                        new Point(hw - c, hh)
                    };
                }
            });

            // =========================================================================
            // SHAPE 55 - ĐAI KÍN MÓC 135° MỘT BÊN
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "55",
                Name = "Shape 55 - Đai Kín 135° Một Bên",
                Category = "Đai Kín (Stirrup)",
                Description = "Đai kín móc 135 độ kết hợp 90 độ.",
                FormulaDisplay = "L = 2×(A + B) + 2×C",
                ParamCount = 3,
                ParamNames = new[] { "A (Rộng mm)", "B (Cao mm)", "C (Móc mm)" },
                DefaultValues = new[] { 500.0, 400.0, 120.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => 2.0 * (a + b) + 2.0 * c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) =>
                {
                    double hw = a / 2.0;
                    double hh = b / 2.0;
                    return new List<Point>
                    {
                        new Point(hw - c * 0.707, hh - c * 0.707),
                        new Point(hw, hh),
                        new Point(-hw, hh),
                        new Point(-hw, -hh),
                        new Point(hw, -hh),
                        new Point(hw, hh),
                        new Point(hw - c, hh)
                    };
                }
            });

            // =========================================================================
            // SHAPE 1 - THANH THẲNG TIÊU CHUẨN (STRAIGHT BAR)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "1",
                Name = "Shape 1 - Thẳng Tiêu Chuẩn",
                Category = "Thẳng (Straight)",
                Description = "Thanh thép thẳng tiêu chuẩn, không có dòng mã màu đỏ.",
                FormulaDisplay = "L = A",
                ParamCount = 1,
                ParamNames = new[] { "A (Chiều dài L mm)" },
                DefaultValues = new[] { 4000.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(-a / 2.0, 0),
                    new Point(a / 2.0, 0)
                }
            });

            // =========================================================================
            // SHAPE 102 - THẲNG NỐI ĐẦU (A//)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "102",
                Name = "Shape 102 - Thẳng Đánh Dấu Đầu Nối",
                Category = "Thẳng (Straight)",
                Description = "Thanh thép thẳng có ký hiệu nối chồng ở đầu mút (//).",
                FormulaDisplay = "L = A",
                ParamCount = 1,
                ParamNames = new[] { "A (Chiều dài L mm)" },
                DefaultValues = new[] { 6000.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(-a / 2.0, 0),
                    new Point(a / 2.0, 0)
                }
            });

            // =========================================================================
            // SHAPE 2 - CHỮ L (L-BAR)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "2",
                Name = "Shape 2 - Uốn Chữ L",
                Category = "Chữ L / Bẻ Móc",
                Description = "Thanh uốn vuông góc 90 độ 1 đầu (cạnh dài A, bẻ móc B).",
                FormulaDisplay = "L = A + B",
                ParamCount = 2,
                ParamNames = new[] { "A (Cạnh ngang mm)", "B (Cạnh đứng mm)" },
                DefaultValues = new[] { 1500.0, 400.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + b,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(0, b),
                    new Point(0, 0),
                    new Point(a, 0)
                }
            });

            // =========================================================================
            // SHAPE 3 - CHỮ L BÁN KÍNH R (L-BAR WITH RADIUS)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "3",
                Name = "Shape 3 - Chữ L Bo Góc R",
                Category = "Chữ L / Bẻ Móc",
                Description = "Thanh uốn chữ L có bán kính uốn R tiêu chuẩn.",
                FormulaDisplay = "L = A + B (Góc R)",
                ParamCount = 3,
                ParamNames = new[] { "A (Cạnh ngang mm)", "B (Cạnh đứng mm)", "R (Bán kính bo mm)" },
                DefaultValues = new[] { 1500.0, 400.0, 45.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + b,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(0, b),
                    new Point(0, 0),
                    new Point(a, 0)
                }
            });

            // =========================================================================
            // SHAPE 51 - CHỮ U ĐỐI XỨNG (U-BAR)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "51",
                Name = "Shape 51 - Thép Chữ U Đối Xứng",
                Category = "Chữ U (U-Bar)",
                Description = "Thép chữ U hai tai đối xứng (đáy A, hai cánh B).",
                FormulaDisplay = "L = A + 2×B",
                ParamCount = 2,
                ParamNames = new[] { "A (Đáy ngang mm)", "B (Hai cánh đứng mm)" },
                DefaultValues = new[] { 400.0, 500.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + 2.0 * b,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(-a / 2.0, b),
                    new Point(-a / 2.0, 0),
                    new Point(a / 2.0, 0),
                    new Point(a / 2.0, b)
                }
            });

            // =========================================================================
            // SHAPE 4 - CHỮ Z LỆCH (Z-BAR)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "4",
                Name = "Shape 4 - Thép Chữ Z",
                Category = "Chữ Z",
                Description = "Thép chữ Z uốn 2 góc ngược chiều nhau.",
                FormulaDisplay = "L = A + B + C",
                ParamCount = 3,
                ParamNames = new[] { "A (Đoạn 1 mm)", "B (Đoạn 2 mm)", "C (Đoạn 3 mm)" },
                DefaultValues = new[] { 300.0, 600.0, 300.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + b + c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(-b / 2.0 - a, 0),
                    new Point(-b / 2.0, 0),
                    new Point(b / 2.0, 0),
                    new Point(b / 2.0, -c)
                }
            });

            // =========================================================================
            // SHAPE 60 - THÉP ĐAI MŨ / MÓC CẨU ÂM
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "60",
                Name = "Shape 60 - Đai Mũ / Cẩu Âm",
                Category = "Đặc Biệt",
                Description = "Thép đai mũ 5 đoạn đối xứng (L = 2A + B + 2C).",
                FormulaDisplay = "L = 2×A + B + 2×C",
                ParamCount = 3,
                ParamNames = new[] { "A (Đoạn đứng mm)", "B (Đoạn ngang mm)", "C (Chân bẻ mm)" },
                DefaultValues = new[] { 300.0, 400.0, 150.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => 2.0 * a + b + 2.0 * c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(-b / 2.0 - c, -a),
                    new Point(-b / 2.0, -a),
                    new Point(-b / 2.0, 0),
                    new Point(b / 2.0, 0),
                    new Point(b / 2.0, -a),
                    new Point(b / 2.0 + c, -a)
                }
            });

            // =========================================================================
            // SHAPE 62 - CHỮ U NGƯỢC
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "62",
                Name = "Shape 62 - Chữ U Ngược Có Móc",
                Category = "Chữ U (U-Bar)",
                Description = "Thép uốn chữ U úp ngược kèm móc xéo 45 độ.",
                FormulaDisplay = "L = A + B + C",
                ParamCount = 3,
                ParamNames = new[] { "A (Ngang mm)", "B (Móc trái mm)", "C (Móc phải mm)" },
                DefaultValues = new[] { 600.0, 300.0, 200.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + b + c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(0, 0),
                    new Point(0, b),
                    new Point(a, b),
                    new Point(a - c * 0.707, b - c * 0.707)
                }
            });

            // =========================================================================
            // SHAPE 370 - GIẰNG CHỐNG UỐN C-CLIP (C-BAR)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "370",
                Name = "Shape 370 - C-Clip / Giằng",
                Category = "Đặc Biệt",
                Description = "Thanh giằng chữ C kẹp sườn dầm/vách (L = A + B + 2C).",
                FormulaDisplay = "L = A + B + 2×C",
                ParamCount = 3,
                ParamNames = new[] { "A (Cạnh chính mm)", "B (Cạnh phụ mm)", "C (Hai móc mm)" },
                DefaultValues = new[] { 600.0, 200.0, 100.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + b + 2.0 * c,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(-a / 2.0, c),
                    new Point(-a / 2.0, 0),
                    new Point(a / 2.0, 0),
                    new Point(a / 2.0, b),
                    new Point(a / 2.0 - c, b)
                }
            });

            // =========================================================================
            // SHAPE 12 - THÉP UỐN 4 ĐOẠN (4-SEGMENT BAR)
            // =========================================================================
            _shapes.Add(new RebarShapeDefinition
            {
                ShapeId = "12",
                Name = "Shape 12 - Uốn 4 Đoạn Liên Tục",
                Category = "Đặc Biệt",
                Description = "Thanh thép uốn 4 phân đoạn A, B, C, D.",
                FormulaDisplay = "L = A + B + C + D",
                ParamCount = 4,
                ParamNames = new[] { "A (Đoạn 1 mm)", "B (Đoạn 2 mm)", "C (Đoạn 3 mm)", "D (Đoạn 4 mm)" },
                DefaultValues = new[] { 300.0, 800.0, 400.0, 300.0 },
                CalculateLengthFunc = (a, b, c, d, e, f, g) => a + b + c + d,
                GeneratePointsFunc = (a, b, c, d, e, f, g) => new List<Point>
                {
                    new Point(0, a),
                    new Point(0, 0),
                    new Point(b, 0),
                    new Point(b, -c),
                    new Point(b + d, -c)
                }
            });
        }
    }
}
