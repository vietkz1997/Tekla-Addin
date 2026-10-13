using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BimCommands.Tekla.MyTool
{
    public class DwgShapeItem
    {
        public string ShapeCode { get; set; } = string.Empty;
        public string BlockName => $"SSBAR_SHP_{ShapeCode}";
        public List<string> Tags { get; set; } = new List<string>();
        public int LineCount { get; set; } = 0;
        public int ArcCount { get; set; } = 0;
        public string HookPosition { get; set; } = "Top-Right"; // Standard in STShape blocks
        public string Description { get; set; } = string.Empty;

        public bool HasTag(string tag)
        {
            return Tags != null && Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
        }

        public string GetTagSummary()
        {
            if (Tags == null || Tags.Count == 0) return "A";
            var cleanTags = Tags.Where(t => !t.StartsWith("COUPLER", StringComparison.OrdinalIgnoreCase) && t != "T1" && t != "O1").ToList();
            return string.Join(", ", cleanTags);
        }
    }

    public static class DwgShapeCatalogManager
    {
        public static event Action<string> LogHandler;

        internal static void Log(string msg)
        {
            try { LogHandler?.Invoke(msg); } catch { }
        }

        private static readonly object _syncLock = new object();
        private static Dictionary<string, DwgShapeItem> _dwgShapesCache = null;
        private static string _loadedDwgOrJsonPath = string.Empty;

        public static string LoadedSourcePath => _loadedDwgOrJsonPath;

        public static string FindDwgOrJsonPath()
        {
            // 1. Check DWG file directly
            string dwgPath = @"c:\Users\BIM\Documents\Github\Tekla-Addin\STShapeBlockAll_230926.dwg";
            if (File.Exists(dwgPath)) return dwgPath;

            // 2. Check JSON data extracted from DWG
            string jsonLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dwg_shapes.json");
            if (File.Exists(jsonLocal)) return jsonLocal;

            string jsonCwd = Path.Combine(Directory.GetCurrentDirectory(), "dwg_shapes.json");
            if (File.Exists(jsonCwd)) return jsonCwd;

            string jsonWorkspace = @"d:\VIET\00.INFORMATION\00LISP\cad\dwg_shapes.json";
            if (File.Exists(jsonWorkspace)) return jsonWorkspace;

            return null;
        }

        public static Dictionary<string, DwgShapeItem> LoadDwgShapes()
        {
            lock (_syncLock)
            {
                if (_dwgShapesCache != null && _dwgShapesCache.Count > 0)
                {
                    return _dwgShapesCache;
                }

                var result = new Dictionary<string, DwgShapeItem>(StringComparer.OrdinalIgnoreCase);
                string path = FindDwgOrJsonPath();

                // If JSON is available, load rapidly from JSON
                string jsonFile = @"d:\VIET\00.INFORMATION\00LISP\cad\dwg_shapes.json";
                if (!File.Exists(jsonFile))
                {
                    jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dwg_shapes.json");
                }

                if (File.Exists(jsonFile))
                {
                    try
                    {
                        string jsonText = File.ReadAllText(jsonFile);
                        _loadedDwgOrJsonPath = jsonFile;
                        ParseJsonShapes(jsonText, result);
                    }
                    catch (Exception ex)
                    {
                        Log("Error reading dwg_shapes.json: " + ex.Message);
                    }
                }

                if (result.Count == 0)
                {
                    // Fallback popular standard shapes from STShapeBlockAll
                    PopulateStandardStShapes(result);
                }

                _dwgShapesCache = result;
                return _dwgShapesCache;
            }
        }

        public static DwgShapeItem GetShape(string shapeCode)
        {
            var dict = LoadDwgShapes();
            if (string.IsNullOrWhiteSpace(shapeCode)) return null;
            shapeCode = shapeCode.Trim();

            if (dict.TryGetValue(shapeCode, out var item)) return item;

            if (shapeCode.StartsWith("Shape", StringComparison.OrdinalIgnoreCase))
            {
                string clean = shapeCode.Substring(5).Trim();
                int sp = clean.IndexOfAny(new[] { ' ', '-', '(' });
                if (sp > 0) clean = clean.Substring(0, sp).Trim();
                if (dict.TryGetValue(clean, out item)) return item;
            }

            return null;
        }

        private static void ParseJsonShapes(string json, Dictionary<string, DwgShapeItem> dict)
        {
            // Simple robust regex parse for json array of objects without heavy third-party dependencies
            var regexObj = new Regex(@"\{[^{}]*""ShapeCode""\s*:\s*""([^""]+)""[^{}]*\}", RegexOptions.Singleline);
            var matches = regexObj.Matches(json);

            foreach (Match m in matches)
            {
                string block = m.Value;
                string code = m.Groups[1].Value.Trim();

                var item = new DwgShapeItem { ShapeCode = code };

                // Extract LineCount
                var mLine = Regex.Match(block, @"""LineCount""\s*:\s*(\d+)");
                if (mLine.Success && int.TryParse(mLine.Groups[1].Value, out int lc)) item.LineCount = lc;

                // Extract ArcCount
                var mArc = Regex.Match(block, @"""ArcCount""\s*:\s*(\d+)");
                if (mArc.Success && int.TryParse(mArc.Groups[1].Value, out int ac)) item.ArcCount = ac;

                // Extract Tags
                var mTags = Regex.Match(block, @"""Tags""\s*:\s*\[([^\]]*)\]");
                if (mTags.Success)
                {
                    var tagMatches = Regex.Matches(mTags.Groups[1].Value, @"""([^""]+)""");
                    foreach (Match tm in tagMatches)
                    {
                        item.Tags.Add(tm.Groups[1].Value);
                    }
                }

                if (code == "56" || code == "55" || code == "54")
                {
                    item.HookPosition = "Top-Right";
                }

                dict[code] = item;
            }
        }

        private static void PopulateStandardStShapes(Dictionary<string, DwgShapeItem> dict)
        {
            dict["1"] = new DwgShapeItem { ShapeCode = "1", Tags = new List<string> { "A" }, LineCount = 1 };
            dict["2"] = new DwgShapeItem { ShapeCode = "2", Tags = new List<string> { "A", "B" }, LineCount = 2 };
            dict["4"] = new DwgShapeItem { ShapeCode = "4", Tags = new List<string> { "A", "B", "C" }, LineCount = 3 };
            dict["11"] = new DwgShapeItem { ShapeCode = "11", Tags = new List<string> { "A", "B", "C", "D" }, LineCount = 4 };
            dict["14"] = new DwgShapeItem { ShapeCode = "14", Tags = new List<string> { "A", "B", "C", "D", "R1" }, LineCount = 4, ArcCount = 1 };
            dict["54"] = new DwgShapeItem { ShapeCode = "54", Tags = new List<string> { "A", "B", "C" }, LineCount = 6, HookPosition = "Top-Right" };
            dict["55"] = new DwgShapeItem { ShapeCode = "55", Tags = new List<string> { "A", "B", "C" }, LineCount = 7, HookPosition = "Top-Right" };
            dict["56"] = new DwgShapeItem { ShapeCode = "56", Tags = new List<string> { "A", "B", "C" }, LineCount = 8, HookPosition = "Top-Right" };
            dict["72"] = new DwgShapeItem { ShapeCode = "72", Tags = new List<string> { "A", "B", "C", "D", "E" }, LineCount = 5, ArcCount = 2 };
            dict["370"] = new DwgShapeItem { ShapeCode = "370", Tags = new List<string> { "A", "B", "C" }, LineCount = 4 };
        }
    }
}
