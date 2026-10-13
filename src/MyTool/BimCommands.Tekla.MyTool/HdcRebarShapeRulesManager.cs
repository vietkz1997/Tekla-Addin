using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace BimCommands.Tekla.MyTool
{
    public class HdcFieldDefinition
    {
        public string FieldName { get; set; } = string.Empty; // e.g. "A", "B", "C", "D", "TD", "R"
        public string Name { get; set; } = string.Empty;      // e.g. "L1", "L4", "EHLA", "SHLA", "R1"
        public string Formula { get; set; } = string.Empty;   // e.g. "round(EHLA,1)", "2*R1", "180-A1*180/PI"
        public string ValueType { get; set; } = string.Empty;
        public string OperandType { get; set; } = string.Empty; // "LegLength", "BendingRadius", etc.
        public int OperandIndex { get; set; } = -1;
    }

    public class HdcHookDefinition
    {
        public string HookType { get; set; } = string.Empty; // e.g. "CustomHook"
        public double Angle { get; set; } = 0.0;             // e.g. 135, 90, 180
        public double Radius { get; set; } = 20.0;           // e.g. 20, 30, 57.5
        public double Length { get; set; } = 80.0;           // e.g. 80, 230, 60
        public bool HasHook => Math.Abs(Angle) > 0.01;
    }

    public class HdcShapeRule
    {
        public string ShapeCode { get; set; } = string.Empty;
        public string Topology { get; set; } = string.Empty;
        public int StraightItemCount { get; set; } = 0;
        public HdcHookDefinition StartHook { get; set; } = new HdcHookDefinition();
        public HdcHookDefinition EndHook { get; set; } = new HdcHookDefinition();
        public Dictionary<string, HdcFieldDefinition> Fields { get; set; } = new Dictionary<string, HdcFieldDefinition>(StringComparer.OrdinalIgnoreCase);

        public string Category
        {
            get
            {
                if (ShapeCode == "1" || ShapeCode == "102") return "Straight";
                if (ShapeCode == "2" || ShapeCode == "12" || ShapeCode == "3" || ShapeCode == "4") return "Bends";
                if (ShapeCode == "11" || ShapeCode == "14" || ShapeCode == "15") return "Hooks";
                if (ShapeCode == "53" || ShapeCode == "54" || ShapeCode == "55" || ShapeCode == "56" || ShapeCode == "60") return "Stirrups";
                if (ShapeCode == "72" || ShapeCode == "370") return "Links / Spacers";
                if (ShapeCode.StartsWith("TS", StringComparison.OrdinalIgnoreCase)) return "HDC Special";
                return "HDC Korea";
            }
        }

        public string GetFieldDescription(string fieldName)
        {
            if (Fields.TryGetValue(fieldName, out var def))
            {
                if (!string.IsNullOrEmpty(def.Formula))
                    return $"{fieldName}={def.Formula}";
                if (!string.IsNullOrEmpty(def.Name))
                    return $"{fieldName}={def.Name}";
            }
            return fieldName;
        }

        public string GetFormulaSummary()
        {
            var parts = new List<string>();
            foreach (var k in new[] { "A", "B", "C", "D", "E" })
            {
                if (Fields.TryGetValue(k, out var f))
                {
                    if (!string.IsNullOrEmpty(f.Formula))
                        parts.Add($"{k}={f.Formula}");
                    else if (!string.IsNullOrEmpty(f.Name))
                        parts.Add($"{k}={f.Name}");
                    else
                        parts.Add(k);
                }
            }

            string hookStr = string.Empty;
            if (StartHook.HasHook && EndHook.HasHook)
            {
                if (Math.Abs(StartHook.Angle - EndHook.Angle) < 1.0)
                    hookStr = $"[Hooks {StartHook.Angle:0}° L={StartHook.Length:0}]";
                else
                    hookStr = $"[SH {StartHook.Angle:0}° L={StartHook.Length:0} | EH {EndHook.Angle:0}° L={EndHook.Length:0}]";
            }
            else if (StartHook.HasHook)
            {
                hookStr = $"[SH {StartHook.Angle:0}° L={StartHook.Length:0}]";
            }
            else if (EndHook.HasHook)
            {
                hookStr = $"[EH {EndHook.Angle:0}° L={EndHook.Length:0}]";
            }

            string fieldsStr = parts.Count > 0 ? string.Join(", ", parts) : "A, B";
            return $"HDC Korea: {fieldsStr} {hookStr}".Trim();
        }

        public double CalculateCutLength(double a, double b, double c, double d, double dia = 10.0)
        {
            string code = ShapeCode;
            if (code == "1" || code == "102") return a;
            if (code == "2" || code == "12") return a + b;
            if (code == "3" || code == "4") return a + b + c;
            if (code == "11") return a + b + c + d;
            if (code == "14") return a + b + c + d;
            if (code == "15") return a + b + c + d + (c > 0 ? c : 0);
            if (code == "53") return a + 2.0 * b;
            if (code == "54" || code == "55" || code == "56") return 2.0 * (a + b) + 2.0 * c;
            if (code == "60") return 4.0 * Math.Sqrt((a / 2.0) * (a / 2.0) + (b / 2.0) * (b / 2.0)) + 2.0 * c;
            if (code == "72") return 2.0 * a + b + 2.0 * c;
            if (code == "370") return a + 2.0 * b + 2.0 * c;

            // Generic fallback
            double total = a;
            if (b > 1.0) total += (StraightItemCount >= 4 ? 2.0 * b : b);
            if (c > 1.0) total += (EndHook.HasHook ? 2.0 * c : c);
            if (d > 1.0) total += d;
            return total;
        }

        public string GetCutLengthFormulaText(double a, double b, double c, double d)
        {
            string code = ShapeCode;
            double len = CalculateCutLength(a, b, c, d);

            if (code == "1" || code == "102")
                return $"L = A = {len:N0} mm";
            if (code == "2" || code == "12")
                return $"L = A + B = {a:0} + {b:0} = {len:N0} mm";
            if (code == "3" || code == "4")
                return $"L = A + B + C = {a:0} + {b:0} + {c:0} = {len:N0} mm";
            if (code == "11" || code == "14")
                return $"L = A + B + C + D = {a:0} + {b:0} + {c:0} + {d:0} = {len:N0} mm";
            if (code == "54" || code == "55" || code == "56")
                return $"L = 2*(A + B) + 2*C = 2*({a:0} + {b:0}) + 2*{c:0} = {len:N0} mm";
            if (code == "72")
                return $"L = 2*A + B + 2*C = 2*{a:0} + {b:0} + 2*{c:0} = {len:N0} mm";
            if (code == "370")
                return $"L = A + 2*B + 2*C = {a:0} + 2*{b:0} + 2*{c:0} = {len:N0} mm";

            return $"L = Total Length = {len:N0} mm";
        }
    }

    public static class HdcRebarShapeRulesManager
    {
        public static event Action<string> LogHandler;

        internal static void Log(string msg)
        {
            try { LogHandler?.Invoke(msg); } catch { }
        }

        private static readonly object _syncLock = new object();
        private static volatile Dictionary<string, HdcShapeRule> _rulesCache = null;
        private static string _loadedXmlPath = string.Empty;

        public static string LoadedXmlPath => _loadedXmlPath;

        public static string FindRulesXmlPath()
        {
            // 1. Try reading Tekla Structures environment variables (XS_FIRM, XS_PROJECT)
            try
            {
                Type settingsType = Type.GetType("Tekla.Structures.TeklaStructuresSettings, Tekla.Structures");
                if (settingsType != null)
                {
                    var getAdvOptMethod = settingsType.GetMethod("GetAdvancedOption", new[] { typeof(string), typeof(string).MakeByRefType() });
                    if (getAdvOptMethod != null)
                    {
                        string CheckSetting(string optName)
                        {
                            object[] args = new object[] { optName, string.Empty };
                            getAdvOptMethod.Invoke(null, args);
                            return args[1] as string;
                        }

                        string firm = CheckSetting("XS_FIRM");
                        if (!string.IsNullOrEmpty(firm))
                        {
                            string p1 = Path.Combine(firm, "RebarShapeRules.xml");
                            if (File.Exists(p1)) return p1;
                            string p2 = Path.Combine(firm, "HDC", "RebarShapeRules.xml");
                            if (File.Exists(p2)) return p2;
                        }

                        string project = CheckSetting("XS_PROJECT");
                        if (!string.IsNullOrEmpty(project))
                        {
                            string p1 = Path.Combine(project, "RebarShapeRules.xml");
                            if (File.Exists(p1)) return p1;
                            string p2 = Path.Combine(project, "HDC", "RebarShapeRules.xml");
                            if (File.Exists(p2)) return p2;
                        }
                    }
                }
            }
            catch
            {
                // Tekla Open API might not be connected in standalone/test mode
            }

            // 2. Application folder
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RebarShapeRules.xml");
            if (File.Exists(localPath)) return localPath;

            // 3. Current working directory
            string cwdPath = Path.Combine(Directory.GetCurrentDirectory(), "RebarShapeRules.xml");
            if (File.Exists(cwdPath)) return cwdPath;

            // 4. Dynamic scan across drives (C:, D:, E:) for any installed TeklaStructures version
            string[] searchDrives = new[] { @"C:\TeklaStructures", @"D:\TeklaStructures", @"E:\TeklaStructures" };
            string[] relativeSubPaths = new[]
            {
                @"Environments\Korea\roles\bimpartners\HDC\RebarShapeRules.xml",
                @"Environments\Korea\system\RebarShapeRules.xml",
                @"Environments\common\system\RebarShapeRules.xml"
            };

            foreach (var drive in searchDrives)
            {
                if (Directory.Exists(drive))
                {
                    try
                    {
                        var versionDirs = Directory.GetDirectories(drive);
                        // Sort descending so newer versions are checked first
                        Array.Sort(versionDirs, (a, b) => string.Compare(b, a, StringComparison.OrdinalIgnoreCase));
                        foreach (var verDir in versionDirs)
                        {
                            foreach (var sub in relativeSubPaths)
                            {
                                string candidate = Path.Combine(verDir, sub);
                                if (File.Exists(candidate)) return candidate;
                            }
                        }
                    }
                    catch { }
                }
            }

            return null;
        }

        public static Dictionary<string, HdcShapeRule> LoadRules(string specificPath = null)
        {
            // Fast-path: Lock-free read if cache is already loaded and no specific path is requested
            if (string.IsNullOrEmpty(specificPath))
            {
                var cached = _rulesCache;
                if (cached != null && cached.Count > 0)
                {
                    return cached;
                }
            }

            lock (_syncLock)
            {
                // Double-checked locking check inside lock
                if (string.IsNullOrEmpty(specificPath))
                {
                    if (_rulesCache != null && _rulesCache.Count > 0)
                    {
                        return _rulesCache;
                    }
                }

                var result = ParseRulesInternal(specificPath);

                if (string.IsNullOrEmpty(specificPath))
                {
                    _rulesCache = result;
                }
                return result;
            }
        }

        public static void ReloadRules(string specificPath = null)
        {
            lock (_syncLock)
            {
                _rulesCache = null;
                LoadRules(specificPath);
            }
        }

        private static Dictionary<string, HdcShapeRule> ParseRulesInternal(string specificPath)
        {
            var result = new Dictionary<string, HdcShapeRule>(StringComparer.OrdinalIgnoreCase);
            string path = !string.IsNullOrEmpty(specificPath) ? specificPath : FindRulesXmlPath();

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(path);
                    _loadedXmlPath = path;

                    var defNodes = doc.SelectNodes("//BendingShapeDefinition");
                    if (defNodes != null)
                    {
                        foreach (XmlNode defNode in defNodes)
                        {
                            string code = defNode.Attributes?["ShapeCode"]?.Value;
                            if (string.IsNullOrWhiteSpace(code)) continue;
                            code = code.Trim();

                            // If multiple variants exist for the same ShapeCode, keep the most comprehensive one (with hooks & fields)
                            bool isNew = !result.TryGetValue(code, out var existingRule);

                            var rule = new HdcShapeRule
                            {
                                ShapeCode = code,
                                Topology = defNode.Attributes?["Topology"]?.Value ?? string.Empty
                            };

                            // Straight items
                            var straightNodes = defNode.SelectNodes(".//StraightItem");
                            rule.StraightItemCount = straightNodes != null ? straightNodes.Count : 0;

                            // Hooks
                            var shNode = defNode.SelectSingleNode(".//StartHook");
                            if (shNode != null)
                            {
                                rule.StartHook = new HdcHookDefinition
                                {
                                    HookType = shNode.Attributes?["Type"]?.Value ?? string.Empty,
                                    Angle = ParseDouble(shNode.Attributes?["Angle"]?.Value, 0.0),
                                    Radius = ParseDouble(shNode.Attributes?["Radius"]?.Value, 20.0),
                                    Length = ParseDouble(shNode.Attributes?["Length"]?.Value, 80.0)
                                };
                            }

                            var ehNode = defNode.SelectSingleNode(".//EndHook");
                            if (ehNode != null)
                            {
                                rule.EndHook = new HdcHookDefinition
                                {
                                    HookType = ehNode.Attributes?["Type"]?.Value ?? string.Empty,
                                    Angle = ParseDouble(ehNode.Attributes?["Angle"]?.Value, 0.0),
                                    Radius = ParseDouble(ehNode.Attributes?["Radius"]?.Value, 20.0),
                                    Length = ParseDouble(ehNode.Attributes?["Length"]?.Value, 80.0)
                                };
                            }

                            // Fields
                            var fieldNodes = defNode.SelectNodes(".//ScheduleFieldDefinition");
                            if (fieldNodes != null)
                            {
                                foreach (XmlNode fn in fieldNodes)
                                {
                                    string fName = fn.Attributes?["FieldName"]?.Value;
                                    if (string.IsNullOrEmpty(fName)) continue;

                                    var fieldDef = new HdcFieldDefinition
                                    {
                                        FieldName = fName,
                                        Name = fn.Attributes?["Name"]?.Value ?? string.Empty,
                                        Formula = fn.Attributes?["Formula"]?.Value ?? string.Empty,
                                        ValueType = fn.Attributes?["ValueType"]?.Value ?? string.Empty
                                    };

                                    var opNode = fn.SelectSingleNode(".//RuleOperand");
                                    if (opNode != null)
                                    {
                                        fieldDef.OperandType = opNode.Attributes?["Type"]?.Value ?? string.Empty;
                                        fieldDef.OperandIndex = ParseInt(opNode.Attributes?["Index"]?.Value, -1);
                                    }

                                    if (!rule.Fields.ContainsKey(fName))
                                    {
                                        rule.Fields[fName] = fieldDef;
                                    }
                                }
                            }

                            if (isNew)
                            {
                                result[code] = rule;
                            }
                            else if (IsNaturallyHookedShape(code))
                            {
                                // For stirrups/ties with hooks, prefer the definition with hooks
                                if ((rule.StartHook.HasHook || rule.EndHook.HasHook) && (!existingRule.StartHook.HasHook && !existingRule.EndHook.HasHook))
                                {
                                    result[code] = rule;
                                }
                                else if (rule.Fields.Count > existingRule.Fields.Count)
                                {
                                    result[code] = rule;
                                }
                            }
                            else
                            {
                                // For pure polygon shapes (Shape 4, 45, 1, 2, 3, 21, 41, 51...), PREFER NO-HOOK DEFINITIONS!
                                bool newHasHook = rule.StartHook.HasHook || rule.EndHook.HasHook;
                                bool existingHasHook = existingRule.StartHook.HasHook || existingRule.EndHook.HasHook;

                                if (!newHasHook && existingHasHook)
                                {
                                    result[code] = rule;
                                }
                                else if (!newHasHook && !existingHasHook)
                                {
                                    // Prefer clean standard fields (e.g. A=L1, B=L2, C=L3) over formulas containing SHLA/EHLA
                                    bool newHasShla = rule.Fields.Values.Any(f => f.Formula.IndexOf("SHLA", StringComparison.OrdinalIgnoreCase) >= 0 || f.Formula.IndexOf("EHLA", StringComparison.OrdinalIgnoreCase) >= 0);
                                    bool existingHasShla = existingRule.Fields.Values.Any(f => f.Formula.IndexOf("SHLA", StringComparison.OrdinalIgnoreCase) >= 0 || f.Formula.IndexOf("EHLA", StringComparison.OrdinalIgnoreCase) >= 0);

                                    if (!newHasShla && existingHasShla)
                                    {
                                        result[code] = rule;
                                    }
                                    else if (rule.Fields.Count >= existingRule.Fields.Count && !newHasShla)
                                    {
                                        result[code] = rule;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error loading RebarShapeRules.xml: {ex.Message}");
                }
            }

            // Sanitize pure polygon shapes (ensure NO hooks and clean fields)
            SanitizePolygonRules(result);

            // If XML was not found or empty, load fallback HDC standard definitions
            if (result.Count == 0)
            {
                LoadFallbackHdcRules(result);
            }

            return result;
        }

        public static bool IsNaturallyHookedShape(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            string c = code.Trim();
            if (c.StartsWith("Shape", StringComparison.OrdinalIgnoreCase))
                c = c.Substring(5).Trim();
            return c == "56" || c == "54" || c == "55" || c == "11" || c == "12" || c == "14" || c == "15";
        }

        private static void SanitizePolygonRules(Dictionary<string, HdcShapeRule> dict)
        {
            var purePolygonCodes = new[] { "1", "01", "2", "02", "3", "03", "4", "04", "21", "021", "41", "041", "45", "045", "51", "051", "60", "060", "62", "062", "67", "067" };
            foreach (var code in purePolygonCodes)
            {
                if (dict.TryGetValue(code, out var rule))
                {
                    rule.StartHook = new HdcHookDefinition();
                    rule.EndHook = new HdcHookDefinition();
                    rule.Fields.Remove("SHLA");
                    rule.Fields.Remove("EHLA");
                    rule.Fields.Remove("RX");
                    rule.Fields.Remove("TD");
                    rule.Fields.Remove("R");
                    rule.Fields.Remove("D");

                    string clean = code.TrimStart('0');
                    if (string.IsNullOrEmpty(clean)) clean = code;

                    if (clean == "1")
                    {
                        rule.Fields["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L1" };
                    }
                    else if (clean == "2")
                    {
                        rule.Fields["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L1" };
                        rule.Fields["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L2" };
                    }
                    else if (clean == "3" || clean == "4" || clean == "45" || clean == "62")
                    {
                        rule.Fields["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L1" };
                        rule.Fields["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L2" };
                        rule.Fields["C"] = new HdcFieldDefinition { FieldName = "C", Name = "L3" };
                    }
                    else if (clean == "21" || clean == "41" || clean == "51")
                    {
                        rule.Fields["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L1" };
                        rule.Fields["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L2" };
                    }
                    else if (clean == "60")
                    {
                        rule.Fields["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L1" };
                        rule.Fields["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L2" };
                    }
                    else if (clean == "67")
                    {
                        rule.Fields["A"] = new HdcFieldDefinition { FieldName = "A", Name = "D" };
                    }
                }
            }
        }

        public static HdcShapeRule GetRule(string shapeCode)
        {
            var all = LoadRules();
            if (string.IsNullOrWhiteSpace(shapeCode)) return null;
            shapeCode = shapeCode.Trim();

            if (all.TryGetValue(shapeCode, out var rule)) return rule;

            // Strip "Shape" prefix if present
            if (shapeCode.StartsWith("Shape", StringComparison.OrdinalIgnoreCase))
            {
                string clean = shapeCode.Substring(5).Trim();
                int space = clean.IndexOfAny(new[] { ' ', '-', '(' });
                if (space > 0) clean = clean.Substring(0, space).Trim();
                if (all.TryGetValue(clean, out rule)) return rule;
            }

            return null;
        }

        private static void LoadFallbackHdcRules(Dictionary<string, HdcShapeRule> dict)
        {
            // Shape 56: HDC Stirrup with 135 deg hooks
            dict["56"] = new HdcShapeRule
            {
                ShapeCode = "56",
                StartHook = new HdcHookDefinition { Angle = 135, Radius = 20, Length = 80, HookType = "CustomHook" },
                EndHook = new HdcHookDefinition { Angle = 135, Radius = 20, Length = 80, HookType = "CustomHook" },
                StraightItemCount = 8,
                Fields = new Dictionary<string, HdcFieldDefinition>(StringComparer.OrdinalIgnoreCase)
                {
                    ["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L4" },
                    ["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L3" },
                    ["C"] = new HdcFieldDefinition { FieldName = "C", Formula = "round(EHLA,1)" },
                    ["TD"] = new HdcFieldDefinition { FieldName = "TD", Formula = "2*R1" }
                }
            };

            // Shape 55: HDC Stirrup with 90 + 135 deg hooks
            dict["55"] = new HdcShapeRule
            {
                ShapeCode = "55",
                StartHook = new HdcHookDefinition { Angle = 90, Radius = 20, Length = 60, HookType = "CustomHook" },
                EndHook = new HdcHookDefinition { Angle = 135, Radius = 20, Length = 60, HookType = "CustomHook" },
                StraightItemCount = 7,
                Fields = new Dictionary<string, HdcFieldDefinition>(StringComparer.OrdinalIgnoreCase)
                {
                    ["A"] = new HdcFieldDefinition { FieldName = "A", Name = "L4", Formula = "L2" },
                    ["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L3", Formula = "L3" },
                    ["C"] = new HdcFieldDefinition { FieldName = "C", Formula = "round(EHLA,1)" }
                }
            };

            // Shape 11: C-Shape with 90 deg hooks
            dict["11"] = new HdcShapeRule
            {
                ShapeCode = "11",
                StartHook = new HdcHookDefinition { Angle = 90, Radius = 57.5, Length = 233.4, HookType = "CustomHook" },
                EndHook = new HdcHookDefinition { Angle = 90, Radius = 57.5, Length = 233.4, HookType = "CustomHook" },
                StraightItemCount = 4,
                Fields = new Dictionary<string, HdcFieldDefinition>(StringComparer.OrdinalIgnoreCase)
                {
                    ["A"] = new HdcFieldDefinition { FieldName = "A", Formula = "round(EHLA,1)" },
                    ["B"] = new HdcFieldDefinition { FieldName = "B", Name = "L3" },
                    ["C"] = new HdcFieldDefinition { FieldName = "C", Name = "L2" },
                    ["D"] = new HdcFieldDefinition { FieldName = "D", Formula = "round(SHLA,1)" }
                }
            };
        }

        private static double ParseDouble(string str, double defaultVal)
        {
            if (string.IsNullOrWhiteSpace(str)) return defaultVal;
            if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                return v;
            return defaultVal;
        }

        private static int ParseInt(string str, int defaultVal)
        {
            if (string.IsNullOrWhiteSpace(str)) return defaultVal;
            if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out int v))
                return v;
            return defaultVal;
        }
    }
}
