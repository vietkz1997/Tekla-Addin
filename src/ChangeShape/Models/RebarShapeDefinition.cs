using System;
using System.Collections.Generic;
using System.Windows;

namespace ChangeShape.Models
{
    /// <summary>
    /// Represents a 2D rebar shape definition matching SIDD-SHOP / Korean CAD standards.
    /// </summary>
    public class RebarShapeDefinition
    {
        public string ShapeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FormulaDisplay { get; set; } = string.Empty;
        public int ParamCount { get; set; } = 3;
        public string[] ParamNames { get; set; } = Array.Empty<string>();
        public double[] DefaultValues { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Calculate total cutting length L (mm) based on parameters A..G.
        /// </summary>
        public Func<double, double, double, double, double, double, double, double> CalculateLengthFunc { get; set; } = (a, b, c, d, e, f, g) => a + b + c;

        /// <summary>
        /// Generates 2D polyline vertices for preview canvas and CAD export.
        /// </summary>
        public Func<double, double, double, double, double, double, double, List<Point>> GeneratePointsFunc { get; set; } = (a, b, c, d, e, f, g) => new List<Point>();

        public double CalculateLength(double A, double B, double C, double D = 0, double E = 0, double F = 0, double G = 0)
        {
            return CalculateLengthFunc(A, B, C, D, E, F, G);
        }

        public List<Point> GeneratePoints(double A, double B, double C, double D = 0, double E = 0, double F = 0, double G = 0)
        {
            return GeneratePointsFunc(A, B, C, D, E, F, G);
        }

        /// <summary>
        /// Formats parameter string for line 2 of rebar tag (e.g. "56,600,400,120").
        /// </summary>
        public string FormatTagParams(double A, double B, double C, double D = 0, double E = 0, double F = 0, double G = 0)
        {
            if (ShapeId == "1")
            {
                return $"{Math.Round(A)}";
            }
            if (ShapeId == "102")
            {
                return $"{Math.Round(A)}//";
            }

            var parts = new List<string>();
            if (ParamCount >= 1) parts.Add(Math.Round(A).ToString());
            if (ParamCount >= 2) parts.Add(Math.Round(B).ToString());
            if (ParamCount >= 3) parts.Add(Math.Round(C).ToString());
            if (ParamCount >= 4) parts.Add(Math.Round(D).ToString());
            if (ParamCount >= 5) parts.Add(Math.Round(E).ToString());
            if (ParamCount >= 6) parts.Add(Math.Round(F).ToString());
            if (ParamCount >= 7) parts.Add(Math.Round(G).ToString());

            return string.Join(",", parts);
        }
    }
}
