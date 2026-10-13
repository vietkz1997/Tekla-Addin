using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChangeShape.Models
{
    /// <summary>
    /// Represents a parsed MText rebar tag from AutoCAD.
    /// Example format:
    /// Line 1: 2-HD25 6200 @200
    /// Line 2: {\H0.8x;\C1;56,600,400,120}
    /// </summary>
    public class CadRebarTag
    {
        public string EntityHandle { get; set; } = string.Empty;
        public string RawMText { get; set; } = string.Empty;
        public string Prefix { get; set; } = "1-HD20";
        public string Suffix { get; set; } = "";
        public double CurrentLength { get; set; } = 4000;
        public string CurrentShapeId { get; set; } = "1";
        public List<double> CurrentParams { get; set; } = new List<double>();
        public int Diameter { get; set; } = 20;
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Parses raw MText string from AutoCAD.
        /// </summary>
        public static CadRebarTag Parse(string rawText, string handle = "")
        {
            var tag = new CadRebarTag
            {
                EntityHandle = handle,
                RawMText = rawText ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(rawText))
                return tag;

            // Normalize line breaks
            string text = rawText.Replace("\r\n", "\\P").Replace("\n", "\\P");
            string line1 = text;
            string line2 = "";

            int pPos = text.IndexOf("\\P", StringComparison.OrdinalIgnoreCase);
            if (pPos >= 0)
            {
                line1 = text.Substring(0, pPos);
                line2 = text.Substring(pPos + 2);
            }

            // Parse Line 1: [Prefix] [Length] [Suffix]
            // e.g. "2-HD25 6200 @200" or "12-D16 3450" or "HD20 4000"
            var parts = line1.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
            {
                tag.Prefix = parts[0];

                // Detect quantity and diameter from prefix (e.g., 2-HD25, HD20, 4-D16)
                var diaMatch = Regex.Match(tag.Prefix, @"(\d+)?[-_]?[A-Za-z]+(\d+)");
                if (diaMatch.Success)
                {
                    if (!string.IsNullOrEmpty(diaMatch.Groups[1].Value) && int.TryParse(diaMatch.Groups[1].Value, out int qty))
                        tag.Quantity = qty;

                    if (!string.IsNullOrEmpty(diaMatch.Groups[2].Value) && int.TryParse(diaMatch.Groups[2].Value, out int dia))
                        tag.Diameter = dia;
                }
            }

            if (parts.Length >= 2)
            {
                if (double.TryParse(parts[1], out double len))
                {
                    tag.CurrentLength = len;
                }
                else
                {
                    tag.Prefix = parts[0] + " " + parts[1];
                }
            }

            if (parts.Length >= 3)
            {
                tag.Suffix = string.Join(" ", parts, 2, parts.Length - 2);
            }

            // Parse Line 2: {\H0.8x;\C1;56,600,400,120} or 56,600,400,120
            if (!string.IsNullOrWhiteSpace(line2))
            {
                // Strip formatting tags like {\H0.8x;\C1; ... }
                string cleanLine2 = Regex.Replace(line2, @"\{?\\H[^;]*;\\C\d+;", "");
                cleanLine2 = cleanLine2.Replace("{", "").Replace("}", "").Trim();

                var shapeTokens = cleanLine2.Split(new[] { ',', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (shapeTokens.Length >= 1)
                {
                    tag.CurrentShapeId = shapeTokens[0].Trim();
                    for (int i = 1; i < shapeTokens.Length; i++)
                    {
                        if (double.TryParse(shapeTokens[i], out double val))
                        {
                            tag.CurrentParams.Add(val);
                        }
                    }
                }
            }
            else
            {
                // Default is Shape 1 (Straight bar)
                tag.CurrentShapeId = "1";
                tag.CurrentParams.Add(tag.CurrentLength);
            }

            return tag;
        }

        /// <summary>
        /// Formats full MText string for AutoCAD with new shape and dimensions.
        /// </summary>
        public string GenerateMText(string newShapeId, double newTotalLength, string tagParamString)
        {
            string line1 = $"{Prefix} {Math.Round(newTotalLength)}";
            if (!string.IsNullOrWhiteSpace(Suffix))
            {
                line1 += $" {Suffix}";
            }

            if (newShapeId == "1")
            {
                // Shape 1 (straight) only has Line 1 per standard
                return line1;
            }

            // Line 2 format: \P{\H0.8x;\C1;56,600,400,120}
            return $"{line1}\\P{{\\H0.8x;\\C1;{newShapeId},{tagParamString}}}";
        }
    }
}
