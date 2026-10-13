using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TeklaUiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using TeklaPicker = Tekla.Structures.Model.UI.Picker;

namespace BimCommands.Tekla.MyTool
{
    public class RebarShapeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public double Diameter { get; set; } = 10.0;
        public string CurrentShapeCode { get; set; } = "1";
        public List<Point> OriginalPoints { get; set; } = new List<Point>();
        public int OriginalPointCount => OriginalPoints != null ? OriginalPoints.Count : 0;
        public double OriginalLength { get; set; }
        public string RebarType { get; set; } = string.Empty;
        public Reinforcement ModelObject { get; set; }

        // Extracted dimension attributes from original geometry
        public List<double> SegmentLengths { get; set; } = new List<double>();
        public double BoundingWidth { get; set; }
        public double BoundingHeight { get; set; }
        public double DimA { get; set; }
        public double DimB { get; set; }
        public double DimC { get; set; }
        public double DimD { get; set; }
        public Vector LocalAxisU { get; set; } = new Vector(1, 0, 0);
        public Vector LocalAxisV { get; set; } = new Vector(0, 1, 0);
        public Vector LocalNormal { get; set; } = new Vector(0, 0, 1);
        public Point CenterPoint { get; set; } = new Point();

        public string GetSegmentsSummary()
        {
            if (SegmentLengths == null || SegmentLengths.Count == 0) return string.Empty;
            var parts = new List<string>();
            for (int i = 0; i < SegmentLengths.Count; i++)
            {
                parts.Add($"L{i + 1}={SegmentLengths[i]:0}");
            }
            return string.Join(", ", parts);
        }
    }

    public class RebarUndoRecord
    {
        public Reinforcement ModelObject { get; set; }
        public ArrayList BackupPolygons { get; set; } = new ArrayList();
        public ArrayList BackupRadiusValues { get; set; } = new ArrayList();
        public RebarHookData.RebarHookShapeEnum BackupStartHookShape { get; set; }
        public double BackupStartHookAngle { get; set; }
        public double BackupStartHookLength { get; set; }
        public double BackupStartHookRadius { get; set; }
        public RebarHookData.RebarHookShapeEnum BackupEndHookShape { get; set; }
        public double BackupEndHookAngle { get; set; }
        public double BackupEndHookLength { get; set; }
        public double BackupEndHookRadius { get; set; }

        public ArrayList CreateDeepCopyPolygons()
        {
            var result = new ArrayList();
            if (BackupPolygons != null)
            {
                foreach (var item in BackupPolygons)
                {
                    if (item is ArrayList ptList)
                    {
                        var copyPts = new ArrayList();
                        foreach (var pt in ptList)
                        {
                            if (pt is Point p)
                                copyPts.Add(new Point(p.X, p.Y, p.Z));
                        }
                        result.Add(copyPts);
                    }
                }
            }
            return result;
        }

        public ArrayList CreateDeepCopyRadiusValues()
        {
            var result = new ArrayList();
            if (BackupRadiusValues != null)
            {
                foreach (var rad in BackupRadiusValues)
                {
                    if (rad is double d)
                    {
                        result.Add(d);
                    }
                    else if (rad is IConvertible conv)
                    {
                        result.Add(conv.ToDouble(System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Restores this undo record's backed-up state to the associated Tekla reinforcement object.
        /// </summary>
        public bool Restore(out string error)
        {
            return RebarShapeModifier.RestoreRebarFromUndoRecord(this, out error);
        }
    }

    public class ShapeDefinition
    {
        public string ShapeCode { get; set; } = "1";
        public string DisplayName { get; set; } = "Shape 1 - Straight Bar";
        public string Category { get; set; } = "Standard";
        public int ParamCount { get; set; } = 1;
        public string LabelA { get; set; } = "A (Length mm):";
        public string LabelB { get; set; } = "B (Height mm):";
        public string LabelC { get; set; } = "C (Hook mm):";
        public string LabelD { get; set; } = "D (Leg mm):";
        public double DefaultA { get; set; } = 1000.0;
        public double DefaultB { get; set; } = 400.0;
        public double DefaultC { get; set; } = 120.0;
        public double DefaultD { get; set; } = 100.0;
        public bool IsFromModelCatalog { get; set; } = false;

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public static class RebarShapeModifier
    {
        public static event Action<string> LogHandler;

        internal static void Log(string msg)
        {
            try { LogHandler?.Invoke(msg); } catch { }
        }

        private static readonly Dictionary<string, ShapeDefinition> _builtInShapes = new Dictionary<string, ShapeDefinition>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _catalogLock = new object();
        private static List<ShapeDefinition> _cachedCatalogShapes = null;

        static RebarShapeModifier()
        {
            InitializeStandardShapes();
        }

        public static void InvalidateCatalogCache()
        {
            lock (_catalogLock)
            {
                _cachedCatalogShapes = null;
            }
        }

        private static void InitializeStandardShapes()
        {
            AddShape(new ShapeDefinition
            {
                ShapeCode = "1",
                DisplayName = "Shape 1 - Straight Bar",
                Category = "Straight",
                ParamCount = 1,
                LabelA = "A (Length mm):",
                DefaultA = 1000.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "102",
                DisplayName = "Shape 102 - Straight Bar (Coupler/Marker)",
                Category = "Straight",
                ParamCount = 1,
                LabelA = "A (Length mm):",
                DefaultA = 1000.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "2",
                DisplayName = "Shape 2 - L-Bar (90 deg Bend)",
                Category = "Bends",
                ParamCount = 2,
                LabelA = "A (Leg 1 mm):",
                LabelB = "B (Leg 2 mm):",
                DefaultA = 600.0,
                DefaultB = 400.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "12",
                DisplayName = "Shape 12 - Straight with One 90 deg Hook",
                Category = "Bends",
                ParamCount = 2,
                LabelA = "A (Main mm):",
                LabelB = "B (Hook mm):",
                DefaultA = 800.0,
                DefaultB = 200.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "3",
                DisplayName = "Shape 3 - Z-Cranked Bar",
                Category = "Cranked",
                ParamCount = 3,
                LabelA = "A (Leg 1 mm):",
                LabelB = "B (Offset mm):",
                LabelC = "C (Leg 2 mm):",
                DefaultA = 500.0,
                DefaultB = 100.0,
                DefaultC = 500.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "4",
                DisplayName = "Shape 4 - U-Bar / Stirrup Cap (3-Sides)",
                Category = "Bends",
                ParamCount = 3,
                LabelA = "A (Leg 1 mm):",
                LabelB = "B (Base mm):",
                LabelC = "C (Leg 2 mm):",
                DefaultA = 300.0,
                DefaultB = 500.0,
                DefaultC = 300.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "11",
                DisplayName = "Shape 11 - Straight Bar with 180 deg Hooks",
                Category = "Hooks",
                ParamCount = 3,
                LabelA = "A (Main Length mm):",
                LabelB = "B (Hook 1 mm):",
                LabelC = "C (Hook 2 mm):",
                DefaultA = 1200.0,
                DefaultB = 100.0,
                DefaultC = 100.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "14",
                DisplayName = "Shape 14 - U-Bar (Same-Side 90 deg Hooks)",
                Category = "U-Bars",
                ParamCount = 3,
                LabelA = "A (Base mm):",
                LabelB = "B (Leg 1 mm):",
                LabelC = "C (Leg 2 mm):",
                DefaultA = 500.0,
                DefaultB = 300.0,
                DefaultC = 300.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "15",
                DisplayName = "Shape 15 - Z-Bar (Opposite 90 deg Hooks)",
                Category = "Bends",
                ParamCount = 3,
                LabelA = "A (Web mm):",
                LabelB = "B (Flange 1 mm):",
                LabelC = "C (Flange 2 mm):",
                DefaultA = 600.0,
                DefaultB = 250.0,
                DefaultC = 250.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "21",
                DisplayName = "Shape 21 - Standard U-Stirrup",
                Category = "U-Bars",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height 1 mm):",
                LabelC = "C (Height 2 mm):",
                DefaultA = 400.0,
                DefaultB = 500.0,
                DefaultC = 500.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "22",
                DisplayName = "Shape 22 - U-Bar with Inward Hooks",
                Category = "U-Bars",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook In mm):",
                DefaultA = 500.0,
                DefaultB = 400.0,
                DefaultC = 100.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "25",
                DisplayName = "Shape 25 - U-Bar with Outward Hooks",
                Category = "U-Bars",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook Out mm):",
                DefaultA = 500.0,
                DefaultB = 400.0,
                DefaultC = 100.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "31",
                DisplayName = "Shape 31 - L-Bar with 135 deg Hook",
                Category = "Hooks",
                ParamCount = 3,
                LabelA = "A (Leg 1 mm):",
                LabelB = "B (Leg 2 mm):",
                LabelC = "C (Hook 135 mm):",
                DefaultA = 500.0,
                DefaultB = 300.0,
                DefaultC = 120.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "41",
                DisplayName = "Shape 41 - Closed Ring / Welded Link",
                Category = "Closed Ties",
                ParamCount = 2,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                DefaultA = 600.0,
                DefaultB = 400.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "51",
                DisplayName = "Shape 51 - Open U-Stirrup (Cap Tie)",
                Category = "Stirrups",
                ParamCount = 2,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                DefaultA = 600.0,
                DefaultB = 400.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "54",
                DisplayName = "Shape 54 - Closed Stirrup (90 deg Hooks)",
                Category = "Stirrups",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook 90 mm):",
                DefaultA = 600.0,
                DefaultB = 400.0,
                DefaultC = 120.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "55",
                DisplayName = "Shape 55 - Closed Stirrup (90 + 135 deg Hooks)",
                Category = "Stirrups",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook mm):",
                DefaultA = 600.0,
                DefaultB = 400.0,
                DefaultC = 120.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "56",
                DisplayName = "Shape 56 - Closed Stirrup (135 deg Seismic Hooks)",
                Category = "Stirrups",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook 135 mm):",
                DefaultA = 600.0,
                DefaultB = 400.0,
                DefaultC = 120.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "60",
                DisplayName = "Shape 60 - Diamond Stirrup (45 deg Tie)",
                Category = "Stirrups",
                ParamCount = 2,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                DefaultA = 500.0,
                DefaultB = 500.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "62",
                DisplayName = "Shape 62 - Triangular Tie",
                Category = "Stirrups",
                ParamCount = 2,
                LabelA = "A (Base mm):",
                LabelB = "B (Height mm):",
                DefaultA = 500.0,
                DefaultB = 400.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "67",
                DisplayName = "Shape 67 - Circular Hoop / Ring",
                Category = "Circular",
                ParamCount = 1,
                LabelA = "A (Diameter mm):",
                DefaultA = 500.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "72",
                DisplayName = "Shape 72 - Hairpin Spacer Tie",
                Category = "Spacers",
                ParamCount = 3,
                LabelA = "A (Length mm):",
                LabelB = "B (Width mm):",
                LabelC = "C (Hook mm):",
                DefaultA = 600.0,
                DefaultB = 150.0,
                DefaultC = 100.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "74",
                DisplayName = "Shape 74 - Chair Bar Support",
                Category = "Spacers",
                ParamCount = 4,
                LabelA = "A (Top Base mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Leg 1 mm):",
                LabelD = "D (Leg 2 mm):",
                DefaultA = 300.0,
                DefaultB = 150.0,
                DefaultC = 200.0,
                DefaultD = 200.0
            });

            AddShape(new ShapeDefinition
            {
                ShapeCode = "370",
                DisplayName = "Shape 370 - C-Clip Link Tie (End Hooks)",
                Category = "Links",
                ParamCount = 3,
                LabelA = "A (Width mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook mm):",
                DefaultA = 400.0,
                DefaultB = 250.0,
                DefaultC = 100.0
            });
        }

        private static void AddShape(ShapeDefinition s)
        {
            _builtInShapes[s.ShapeCode] = s;
        }

        public static List<ShapeDefinition> GetAllAvailableShapes(Model model)
        {
            lock (_catalogLock)
            {
                if (_cachedCatalogShapes != null && _cachedCatalogShapes.Count > 0)
                {
                    return new List<ShapeDefinition>(_cachedCatalogShapes);
                }

                var result = new List<ShapeDefinition>(_builtInShapes.Values);
                var seenCodes = new HashSet<string>(_builtInShapes.Keys, StringComparer.OrdinalIgnoreCase);

                // Load HDC Korea RebarShapeRules definitions
                try
                {
                    var hdcRules = HdcRebarShapeRulesManager.LoadRules();
                    if (hdcRules != null && hdcRules.Count > 0)
                    {
                        foreach (var kvp in hdcRules)
                        {
                            var rule = kvp.Value;
                            if (seenCodes.Add(rule.ShapeCode))
                            {
                                var shapeDef = CreateShapeDefinitionFromHdcRule(rule);
                                result.Add(shapeDef);
                            }
                            else
                            {
                                // Enrich existing built-in shape with HDC formula details
                                var existing = result.FirstOrDefault(s => string.Equals(s.ShapeCode, rule.ShapeCode, StringComparison.OrdinalIgnoreCase));
                                if (existing != null)
                                {
                                    EnrichShapeWithHdcRule(existing, rule);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Error reading HDC RebarShapeRules: " + ex.Message);
                }

                // Load STShape DWG Catalog (STShapeBlockAll_230926.dwg)
                try
                {
                    var dwgShapes = DwgShapeCatalogManager.LoadDwgShapes();
                    if (dwgShapes != null && dwgShapes.Count > 0)
                    {
                        foreach (var kvp in dwgShapes)
                        {
                            var dwgItem = kvp.Value;
                            if (seenCodes.Add(dwgItem.ShapeCode))
                            {
                                int pCount = Math.Max(1, Math.Min(4, dwgItem.Tags.Count));
                                result.Add(new ShapeDefinition
                                {
                                    ShapeCode = dwgItem.ShapeCode,
                                    DisplayName = $"Shape {dwgItem.ShapeCode} [STShape DWG: {dwgItem.GetTagSummary()}]",
                                    Category = "STShape DWG",
                                    ParamCount = pCount,
                                    LabelA = dwgItem.HasTag("A") ? "A (mm):" : "A (Length mm):",
                                    LabelB = dwgItem.HasTag("B") ? "B (mm):" : "B (Height mm):",
                                    LabelC = dwgItem.HasTag("C") ? "C (mm):" : "C (Hook mm):",
                                    LabelD = dwgItem.HasTag("D") ? "D (mm):" : "D (Leg mm):",
                                    DefaultA = 600.0,
                                    DefaultB = 400.0,
                                    DefaultC = 120.0,
                                    IsFromModelCatalog = true
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Error reading STShape DWG shapes: " + ex.Message);
                }

                try
                {
                    if (model != null && model.GetConnectionStatus())
                    {
                        string modelPath = model.GetInfo().ModelPath;
                        string catFile = Path.Combine(modelPath, "attributes", "_current_.RebarShapeCatalog.dat");

                        if (File.Exists(catFile))
                        {
                            var lines = File.ReadAllLines(catFile);
                            foreach (var line in lines)
                            {
                                if (line.StartsWith("SHAPE:", StringComparison.OrdinalIgnoreCase))
                                {
                                    int colonIdx = line.IndexOf(':');
                                    int semiIdx = line.IndexOfAny(new[] { ' ', ';', '[' }, colonIdx + 1);
                                    string code = semiIdx > 0 ? line.Substring(colonIdx + 1, semiIdx - colonIdx - 1).Trim() : line.Substring(colonIdx + 1).Trim();

                                    if (!string.IsNullOrEmpty(code) && seenCodes.Add(code))
                                    {
                                        result.Add(new ShapeDefinition
                                        {
                                            ShapeCode = code,
                                            DisplayName = $"Shape {code} (Tekla Model Catalog)",
                                            Category = "Model Catalog",
                                            ParamCount = 3,
                                            LabelA = "A (Width/Length mm):",
                                            LabelB = "B (Height mm):",
                                            LabelC = "C (Hook mm):",
                                            DefaultA = 600.0,
                                            DefaultB = 400.0,
                                            DefaultC = 120.0,
                                            IsFromModelCatalog = true
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Error reading model shape catalog: " + ex.Message);
                }

                // Sort shapes: numerical shape code ascending first, then alphanumeric
                result = result
                    .OrderBy(s => int.TryParse(s.ShapeCode?.Trim(), out int n) ? n : 999999)
                    .ThenBy(s => s.ShapeCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                _cachedCatalogShapes = result;
                return new List<ShapeDefinition>(_cachedCatalogShapes);
            }
        }

        private static ShapeDefinition CreateShapeDefinitionFromHdcRule(HdcShapeRule rule)
        {
            var def = new ShapeDefinition
            {
                ShapeCode = rule.ShapeCode,
                DisplayName = $"Shape {rule.ShapeCode} - {rule.GetFormulaSummary()}",
                Category = rule.Category,
                IsFromModelCatalog = true
            };

            EnrichShapeWithHdcRule(def, rule);
            return def;
        }

        private static void EnrichShapeWithHdcRule(ShapeDefinition def, HdcShapeRule rule)
        {
            if (rule == null || def == null) return;

            int count = 0;
            if (rule.Fields.TryGetValue("A", out var fa))
            {
                def.LabelA = !string.IsNullOrEmpty(fa.Formula) ? $"A ({fa.Formula} mm):" : (!string.IsNullOrEmpty(fa.Name) ? $"A ({fa.Name} mm):" : "A (mm):");
                count++;
            }
            if (rule.Fields.TryGetValue("B", out var fb))
            {
                def.LabelB = !string.IsNullOrEmpty(fb.Formula) ? $"B ({fb.Formula} mm):" : (!string.IsNullOrEmpty(fb.Name) ? $"B ({fb.Name} mm):" : "B (mm):");
                count++;
            }
            if (rule.Fields.TryGetValue("C", out var fc))
            {
                def.LabelC = !string.IsNullOrEmpty(fc.Formula) ? $"C ({fc.Formula} mm):" : (!string.IsNullOrEmpty(fc.Name) ? $"C ({fc.Name} mm):" : "C (mm):");
                count++;
            }
            if (rule.Fields.TryGetValue("D", out var fd))
            {
                def.LabelD = !string.IsNullOrEmpty(fd.Formula) ? $"D ({fd.Formula} mm):" : (!string.IsNullOrEmpty(fd.Name) ? $"D ({fd.Name} mm):" : "D (mm):");
                count++;
            }

            if (count > 0)
            {
                def.ParamCount = Math.Max(def.ParamCount, count);
            }

            if (rule.StartHook.HasHook || rule.EndHook.HasHook)
            {
                double defaultHookLen = rule.EndHook.HasHook ? rule.EndHook.Length : rule.StartHook.Length;
                if (defaultHookLen > 0) def.DefaultC = defaultHookLen;
            }
        }

        public static ShapeDefinition GetShapeDefinition(string shapeCode, Model model = null)
        {
            if (string.IsNullOrWhiteSpace(shapeCode)) shapeCode = "1";
            shapeCode = shapeCode.Trim();

            // Try direct exact or numeric match
            if (_builtInShapes.TryGetValue(shapeCode, out var exact)) return exact;

            string cleanedCode = shapeCode;
            if (cleanedCode.StartsWith("Shape", StringComparison.OrdinalIgnoreCase))
            {
                cleanedCode = cleanedCode.Substring(5).Trim();
                int space = cleanedCode.IndexOfAny(new[] { ' ', '-', '(' });
                if (space > 0) cleanedCode = cleanedCode.Substring(0, space).Trim();
            }

            if (_builtInShapes.TryGetValue(cleanedCode, out var matchCleaned)) return matchCleaned;

            // Check if available from HDC RebarShapeRules
            var hdcRule = HdcRebarShapeRulesManager.GetRule(cleanedCode);
            if (hdcRule != null)
            {
                return CreateShapeDefinitionFromHdcRule(hdcRule);
            }

            // Check if available from model catalog
            if (model != null)
            {
                var all = GetAllAvailableShapes(model);
                var found = all.FirstOrDefault(s => string.Equals(s.ShapeCode, cleanedCode, StringComparison.OrdinalIgnoreCase));
                if (found != null) return found;
            }

            // Return generic shape definition for any custom user number
            return new ShapeDefinition
            {
                ShapeCode = cleanedCode,
                DisplayName = $"Shape {cleanedCode} (Custom Shape)",
                Category = "Custom",
                ParamCount = 3,
                LabelA = "A (Width/Length mm):",
                LabelB = "B (Height mm):",
                LabelC = "C (Hook mm):",
                DefaultA = 600.0,
                DefaultB = 400.0,
                DefaultC = 120.0
            };
        }

        public static List<Reinforcement> GetSelectedReinforcements(Model model)
        {
            var result = new List<Reinforcement>();
            var seenIds = new HashSet<int>();

            try
            {
                var selector = new TeklaUiSelector();
                var enumerator = selector.GetSelectedObjects();

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current == null) continue;

                    if (current is Reinforcement rebar)
                    {
                        if (seenIds.Add(rebar.Identifier.ID))
                        {
                            result.Add(rebar);
                        }
                    }
                    else if (current is Part part)
                    {
                        var children = part.GetChildren();
                        while (children.MoveNext())
                        {
                            if (children.Current is Reinforcement childRebar && seenIds.Add(childRebar.Identifier.ID))
                            {
                                result.Add(childRebar);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error selecting objects: " + ex.Message);
            }

            return result;
        }

        public static List<Reinforcement> PickReinforcementsFromModel(string prompt = "Pick reinforcement:")
        {
            var result = new List<Reinforcement>();
            var seenIds = new HashSet<int>();

            try
            {
                var picker = new TeklaPicker();
                var pickedObjects = picker.PickObjects(TeklaPicker.PickObjectsEnum.PICK_N_REINFORCEMENTS, prompt);

                while (pickedObjects.MoveNext())
                {
                    var current = pickedObjects.Current;
                    if (current is Reinforcement rebar)
                    {
                        if (seenIds.Add(rebar.Identifier.ID))
                        {
                            result.Add(rebar);
                        }
                    }
                    else if (current is Part part)
                    {
                        var children = part.GetChildren();
                        while (children.MoveNext())
                        {
                            if (children.Current is Reinforcement childRebar && seenIds.Add(childRebar.Identifier.ID))
                            {
                                result.Add(childRebar);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Picker cancelled or error: " + ex.Message);
            }

            return result;
        }

        public static RebarShapeInfo AnalyzeRebar(Reinforcement rebar)
        {
            return ExtractRebarInfo(rebar);
        }

        public static RebarShapeInfo ExtractRebarInfo(Reinforcement rebar)
        {
            var info = new RebarShapeInfo
            {
                Id = rebar.Identifier.ID,
                Name = rebar.Name ?? "REBAR",
                Grade = rebar.Grade ?? "",
                ModelObject = rebar
            };

            double dia = 0;
            if (rebar.GetReportProperty("DIAMETER", ref dia) && dia > 0)
            {
                info.Diameter = dia;
            }
            else
            {
                info.Diameter = 10.0;
            }

            string shapeCode = "";
            rebar.GetReportProperty("SHAPE", ref shapeCode);
            if (string.IsNullOrEmpty(shapeCode)) rebar.GetReportProperty("SHAPE_INTERNAL", ref shapeCode);
            info.CurrentShapeCode = !string.IsNullOrEmpty(shapeCode) ? shapeCode : "1";

            double len = 0;
            if (rebar.GetReportProperty("LENGTH", ref len))
            {
                info.OriginalLength = len;
            }

            if (rebar is SingleRebar sr)
            {
                info.RebarType = "SingleRebar";
                info.Size = sr.Size ?? "";
                if (sr.Polygon != null && sr.Polygon.Points != null)
                {
                    foreach (Point pt in sr.Polygon.Points)
                    {
                        info.OriginalPoints.Add(new Point(pt.X, pt.Y, pt.Z));
                    }
                }
            }
            else if (rebar is RebarGroup rg)
            {
                info.RebarType = "RebarGroup";
                info.Size = rg.Size ?? "";
                if (rg.Polygons != null && rg.Polygons.Count > 0)
                {
                    var poly = rg.Polygons[0] as Polygon;
                    if (poly != null && poly.Points != null)
                    {
                        foreach (Point pt in poly.Points)
                        {
                            info.OriginalPoints.Add(new Point(pt.X, pt.Y, pt.Z));
                        }
                    }
                }
            }
            else if (rebar is CircleRebarGroup crg)
            {
                info.RebarType = "CircleRebarGroup";
                info.Size = crg.Size ?? "";
                if (crg.Polygon?.Points != null)
                {
                    foreach (Point pt in crg.Polygon.Points)
                    {
                        info.OriginalPoints.Add(new Point(pt.X, pt.Y, pt.Z));
                    }
                }
            }
            else if (rebar is CurvedRebarGroup cvrg)
            {
                info.RebarType = "CurvedRebarGroup";
                info.Size = cvrg.Size ?? "";
                if (cvrg.Polygon?.Points != null)
                {
                    foreach (Point pt in cvrg.Polygon.Points)
                    {
                        info.OriginalPoints.Add(new Point(pt.X, pt.Y, pt.Z));
                    }
                }
            }
            else
            {
                info.RebarType = rebar.GetType().Name;
            }

            // 1. Calculate segment lengths
            if (info.OriginalPoints.Count >= 2)
            {
                double total = 0;
                for (int i = 0; i < info.OriginalPoints.Count - 1; i++)
                {
                    double segLen = Distance.PointToPoint(info.OriginalPoints[i], info.OriginalPoints[i + 1]);
                    if (segLen > 0.5)
                    {
                        double rounded = Math.Round(segLen, 0);
                        info.SegmentLengths.Add(rounded);
                        total += rounded;
                    }
                }
                if (info.OriginalLength <= 0.1) info.OriginalLength = total;
            }

            // 2. Try read official Tekla Bending Schedule dimensions
            double da = 0, db = 0, dc = 0, dd = 0;
            if (rebar.GetReportProperty("DIM_A", ref da) && da > 1.0) info.DimA = Math.Round(da, 0);
            if (rebar.GetReportProperty("DIM_B", ref db) && db > 1.0) info.DimB = Math.Round(db, 0);
            if (rebar.GetReportProperty("DIM_C", ref dc) && dc > 1.0) info.DimC = Math.Round(dc, 0);
            if (rebar.GetReportProperty("DIM_D", ref dd) && dd > 1.0) info.DimD = Math.Round(dd, 0);

            // 3. Fallback from segment lengths if DIM properties are not populated
            if (info.DimA <= 1.0 && info.SegmentLengths.Count >= 1) info.DimA = info.SegmentLengths[0];
            if (info.DimB <= 1.0 && info.SegmentLengths.Count >= 2) info.DimB = info.SegmentLengths[1];
            if (info.DimC <= 1.0 && info.SegmentLengths.Count >= 3) info.DimC = info.SegmentLengths[2];
            if (info.DimD <= 1.0 && info.SegmentLengths.Count >= 4) info.DimD = info.SegmentLengths[3];

            // 4. Calculate accurate coordinate system and bounding dimensions along bar's own primary axis
            var frame = DetermineCoordinateSystem(info.OriginalPoints, "AutoPerpendicular");
            info.CenterPoint = frame.Center;
            info.LocalAxisU = frame.WidthDir;
            info.LocalAxisV = frame.HeightDir;

            if (info.OriginalPoints.Count >= 2)
            {
                double minU = double.MaxValue, maxU = double.MinValue;
                double minV = double.MaxValue, maxV = double.MinValue;
                foreach (var p in info.OriginalPoints)
                {
                    double uVal = (p.X - frame.Center.X) * frame.WidthDir.X + (p.Y - frame.Center.Y) * frame.WidthDir.Y + (p.Z - frame.Center.Z) * frame.WidthDir.Z;
                    double vVal = (p.X - frame.Center.X) * frame.HeightDir.X + (p.Y - frame.Center.Y) * frame.HeightDir.Y + (p.Z - frame.Center.Z) * frame.HeightDir.Z;
                    if (uVal < minU) minU = uVal; if (uVal > maxU) maxU = uVal;
                    if (vVal < minV) minV = vVal; if (vVal > maxV) maxV = vVal;
                }
                info.BoundingWidth = Math.Round(maxU - minU, 0);
                info.BoundingHeight = Math.Round(maxV - minV, 0);
            }

            return info;
        }

        public static (Point Center, Vector WidthDir, Vector HeightDir) GetRebarReferenceFrame(
            Reinforcement rebar,
            string planeMode)
        {
            var info = ExtractRebarInfo(rebar);
            return DetermineCoordinateSystem(info.OriginalPoints, planeMode);
        }

        public static (Point Center, Vector WidthDir, Vector HeightDir) GetRebarReferenceFrame(
            RebarShapeInfo info,
            string planeMode)
        {
            if (info != null && info.OriginalPoints != null && info.OriginalPoints.Count >= 2)
            {
                return DetermineCoordinateSystem(info.OriginalPoints, planeMode);
            }
            if (info?.ModelObject != null)
            {
                return GetRebarReferenceFrame(info.ModelObject, planeMode);
            }
            return (new Point(), new Vector(1, 0, 0), new Vector(0, 0, 1));
        }

        public static (Point Center, Vector WidthDir, Vector HeightDir) DetermineCoordinateSystem(
            List<Point> pts,
            string planeMode)
        {
            Point center = new Point();
            Vector widthDir = new Vector(1, 0, 0);
            Vector heightDir = new Vector(0, 0, 1);

            if (pts != null && pts.Count >= 2)
            {
                double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
                foreach (var p in pts)
                {
                    if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
                    if (p.Z < minZ) minZ = p.Z; if (p.Z > maxZ) maxZ = p.Z;
                }
                center = new Point((minX + maxX) / 2.0, (minY + maxY) / 2.0, (minZ + maxZ) / 2.0);

                if (planeMode == "HorizontalXY")
                {
                    widthDir = new Vector(1, 0, 0);
                    heightDir = new Vector(0, 1, 0);
                }
                else if (planeMode == "VerticalXZ")
                {
                    widthDir = new Vector(1, 0, 0);
                    heightDir = new Vector(0, 0, 1);
                }
                else if (planeMode == "VerticalYZ")
                {
                    widthDir = new Vector(0, 1, 0);
                    heightDir = new Vector(0, 0, 1);
                }
                else // "AutoPerpendicular"
                {
                    // Detect polygon plane normal if polygon has >= 3 vertices
                    Vector planeNormal = new Vector();
                    double maxSegLen = -1;
                    Vector primarySegDir = new Vector(1, 0, 0);

                    // Find primary segment (longest edge) to align widthDir naturally with the bar
                    for (int i = 0; i < pts.Count - 1; i++)
                    {
                        Vector seg = new Vector(pts[i + 1] - pts[i]);
                        double sl = seg.GetLength();
                        if (sl > maxSegLen)
                        {
                            maxSegLen = sl;
                            primarySegDir = seg.GetNormal();
                        }
                    }

                    if (pts.Count >= 3)
                    {
                        for (int i = 0; i < pts.Count - 1; i++)
                        {
                            Vector v1 = new Vector(pts[i + 1] - pts[i]);
                            if (v1.GetLength() < 1.0) continue;
                            for (int j = i + 1; j < pts.Count - 1; j++)
                            {
                                Vector v2 = new Vector(pts[j + 1] - pts[j]);
                                Vector cr = v1.Cross(v2);
                                if (cr.GetLength() > 5.0)
                                {
                                    planeNormal = cr.GetNormal();
                                    break;
                                }
                            }
                            if (planeNormal.GetLength() > 0.5) break;
                        }
                    }

                    if (planeNormal.GetLength() > 0.5)
                    {
                        // Normalize planeNormal so its positive orientation is consistently upward / forward
                        if (planeNormal.Z < -0.01 ||
                            (Math.Abs(planeNormal.Z) <= 0.01 && planeNormal.Y < -0.01) ||
                            (Math.Abs(planeNormal.Z) <= 0.01 && Math.Abs(planeNormal.Y) <= 0.01 && planeNormal.X < -0.01))
                        {
                            planeNormal = planeNormal * -1.0;
                        }

                        // Project primary segment onto the plane
                        double dotVal = primarySegDir.Dot(planeNormal);
                        Vector proj = new Vector(primarySegDir.X - planeNormal.X * dotVal,
                                                 primarySegDir.Y - planeNormal.Y * dotVal,
                                                 primarySegDir.Z - planeNormal.Z * dotVal).GetNormal();
                        if (proj.GetLength() > 0.5)
                        {
                            widthDir = proj;
                            heightDir = planeNormal.Cross(widthDir).GetNormal();
                        }
                        else
                        {
                            Vector zUp = Math.Abs(planeNormal.Z) < 0.707 ? new Vector(0, 0, 1) : new Vector(0, 1, 0);
                            heightDir = new Vector(zUp - (planeNormal * zUp.Dot(planeNormal))).GetNormal();
                            widthDir = heightDir.Cross(planeNormal).GetNormal();
                        }

                        // Consistent height orientation:
                        // If plane is near-horizontal (slab/footing in XY plane with |planeNormal.Z| >= 0.707),
                        // heightDir lies in XY plane -> prefer positive Y (or positive X).
                        // If plane is vertical/inclined (|planeNormal.Z| < 0.707), prefer positive Z.
                        if (Math.Abs(planeNormal.Z) >= 0.707)
                        {
                            if (heightDir.Y < -0.01 || (Math.Abs(heightDir.Y) <= 0.01 && heightDir.X < -0.01))
                            {
                                heightDir = heightDir * -1.0;
                                widthDir = heightDir.Cross(planeNormal).GetNormal();
                            }
                        }
                        else
                        {
                            if (heightDir.Z < -0.01 || (Math.Abs(heightDir.Z) <= 0.01 && heightDir.Y < -0.01))
                            {
                                heightDir = heightDir * -1.0;
                                widthDir = heightDir.Cross(planeNormal).GetNormal();
                            }
                        }
                    }
                    else
                    {
                        // Straight rebar fallback
                        Vector barAxis = primarySegDir;
                        if (barAxis.GetLength() < 0.001) barAxis = new Vector(1, 0, 0);
                        Vector refUp = Math.Abs(barAxis.Z) > 0.85 ? new Vector(0, 1, 0) : new Vector(0, 0, 1);
                        widthDir = barAxis;
                        heightDir = barAxis.Cross(refUp).GetNormal();
                        if (heightDir.GetLength() < 0.1) heightDir = new Vector(0, 0, 1);

                        if (heightDir.Z < -0.01 || (Math.Abs(heightDir.Z) <= 0.01 && heightDir.Y < -0.01))
                        {
                            heightDir = heightDir * -1.0;
                        }
                    }
                }
            }
            else if (pts != null && pts.Count == 1)
            {
                center = pts[0];
            }

            return (center, widthDir, heightDir);
        }

        public static List<Point> GenerateShapePoints(
            string shapeCode,
            Point center,
            Vector widthDir,
            Vector heightDir,
            double a,
            double b,
            double c,
            double d,
            double rebarDia,
            Model model = null,
            string corner = "Auto",
            bool flip = false,
            double rotationAngle = 0.0)
        {
            Vector u = widthDir.GetNormal();
            Vector v = heightDir.GetNormal();
            if (flip)
            {
                v = v * -1.0;
            }

            if (Math.Abs(rotationAngle) > 0.001)
            {
                double rad = rotationAngle * Math.PI / 180.0;
                double cosA = Math.Cos(rad);
                double sinA = Math.Sin(rad);
                // Rotation axis = plane normal (perpendicular to both u and v)
                Vector n = u.Cross(v).GetNormal();
                // Rodrigues' rotation: rigidly rotate both u and v around normal n by angle rad
                // v_rot = v*cos(θ) + (n×v)*sin(θ) + n*(n·v)*(1-cos(θ))
                Vector nCrossU = n.Cross(u);
                double nDotU = n.Dot(u);
                Vector uRotated = new Vector(
                    u.X * cosA + nCrossU.X * sinA + n.X * nDotU * (1.0 - cosA),
                    u.Y * cosA + nCrossU.Y * sinA + n.Y * nDotU * (1.0 - cosA),
                    u.Z * cosA + nCrossU.Z * sinA + n.Z * nDotU * (1.0 - cosA));
                if (uRotated.GetLength() > 0.001)
                {
                    u = uRotated.GetNormal();
                }

                Vector nCrossV = n.Cross(v);
                double nDotV = n.Dot(v);
                Vector vRotated = new Vector(
                    v.X * cosA + nCrossV.X * sinA + n.X * nDotV * (1.0 - cosA),
                    v.Y * cosA + nCrossV.Y * sinA + n.Y * nDotV * (1.0 - cosA),
                    v.Z * cosA + nCrossV.Z * sinA + n.Z * nDotV * (1.0 - cosA));
                if (vRotated.GetLength() > 0.001)
                {
                    v = vRotated.GetNormal();
                }
            }

            string clean = shapeCode.Trim();
            if (clean.StartsWith("Shape", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(5).Trim();
                int sp = clean.IndexOfAny(new[] { ' ', '-', '(' });
                if (sp > 0) clean = clean.Substring(0, sp).Trim();
            }

            switch (clean)
            {
                case "1":
                case "01":
                case "0":
                    return GenerateShape1Points(center, u, a);

                case "102":
                    return GenerateShape1Points(center, u, a);

                case "2":
                case "02":
                    return GenerateShape2Points(center, u, v, a, b);

                case "12":
                case "012":
                    return GenerateShape12Points(center, u, v, a, b);

                case "3":
                case "03":
                    return GenerateShape3Points(center, u, v, a, b, c);

                case "4":
                case "04":
                case "45":
                case "045":
                    return GenerateShape4Points(center, u, v, a, b, c);

                case "11":
                case "011":
                    return GenerateShape11Points(center, u, v, a, b, c);

                case "14":
                case "014":
                    return GenerateShape14Points(center, u, v, a, b, c);

                case "15":
                case "015":
                    return GenerateShape15Points(center, u, v, a, b, c);

                case "21":
                case "021":
                    return GenerateShape21Points(center, u, v, a, b, c);

                case "22":
                case "022":
                    return GenerateShape22Points(center, u, v, a, b, c);

                case "25":
                case "025":
                    return GenerateShape25Points(center, u, v, a, b, c);

                case "31":
                    return GenerateShape31Points(center, u, v, a, b, c);

                case "41":
                    return GenerateShape41Points(center, u, v, a, b, corner);

                case "51":
                    return GenerateShape51Points(center, u, v, a, b);

                case "54":
                    return GenerateShape54Points(center, u, v, a, b, c, corner);

                case "55":
                    return GenerateShape55Points(center, u, v, a, b, c, corner);

                case "56":
                    return GenerateShape56Points(center, u, v, a, b, c, corner);

                case "60":
                    return GenerateShape60Points(center, u, v, a, b);

                case "62":
                    return GenerateShape62Points(center, u, v, a, b);

                case "67":
                    return GenerateShape67Points(center, u, v, a);

                case "72":
                    return GenerateShape72Points(center, u, v, a, b, c);

                case "74":
                    return GenerateShape74Points(center, u, v, a, b, c, d);

                case "370":
                    return GenerateShape370Points(center, u, v, a, b, c);

                default:
                    // If shape has 4 params, default to multi-bend geometry
                    if (d > 1.0)
                    {
                        return GenerateShape74Points(center, u, v, a, b, c, d);
                    }
                    // If shape has 3 params, default to stirrup-like closed/hooked geometry
                    if (c > 1.0)
                    {
                        return GenerateShape56Points(center, u, v, a, b, c, corner);
                    }
                    if (b > 1.0)
                    {
                        return GenerateShape51Points(center, u, v, a, b);
                    }
                    return GenerateShape1Points(center, u, a);
            }
        }

        // =========================================================================
        // GEOMETRIC SHAPE GENERATORS
        // =========================================================================

        public static List<Point> GenerateShape1Points(Point center, Vector u, double lengthA)
        {
            double half = lengthA / 2.0;
            return new List<Point> { center - (u * half), center + (u * half) };
        }

        public static List<Point> GenerateShape2Points(Point center, Vector u, Vector v, double legA, double legB)
        {
            Point corner = center;
            return new List<Point>
            {
                corner + (v * legB),
                corner,
                corner + (u * legA)
            };
        }

        public static List<Point> GenerateShape12Points(Point center, Vector u, Vector v, double mainA, double hookB)
        {
            Point pStart = center - (u * (mainA / 2.0));
            Point pCorner = center + (u * (mainA / 2.0));
            Point pHook = pCorner + (v * hookB);
            return new List<Point> { pStart, pCorner, pHook };
        }

        public static List<Point> GenerateShape3Points(Point center, Vector u, Vector v, double legA, double offsetB, double legC)
        {
            double halfTotalU = (legA + legC) / 2.0;
            double halfOffsetV = offsetB / 2.0;
            Point p1 = center - (u * halfTotalU) - (v * halfOffsetV);
            Point p2 = p1 + (u * legA);
            Point p3 = p2 + (v * offsetB);
            Point p4 = p3 + (u * legC);
            return new List<Point> { p1, p2, p3, p4 };
        }

        public static List<Point> GenerateShape4Points(Point center, Vector u, Vector v, double legA, double baseB, double legC)
        {
            double halfB = baseB / 2.0;
            double halfH = Math.Max(legA, legC) / 2.0;
            Point pBL = center - (u * halfB) - (v * halfH);
            Point pBR = center + (u * halfB) - (v * halfH);
            Point pTL = pBL + (v * legA);
            Point pTR = pBR + (v * legC);

            return new List<Point> { pTL, pBL, pBR, pTR };
        }

        public static List<Point> GenerateShape11Points(Point center, Vector u, Vector v, double mainA, double hookB, double hookC)
        {
            double half = mainA / 2.0;
            Point pL = center - (u * half);
            Point pR = center + (u * half);
            Point pHookL = pL + (v * hookB);
            Point pHookR = pR + (v * hookC);
            return new List<Point> { pHookL, pL, pR, pHookR };
        }

        public static List<Point> GenerateShape14Points(Point center, Vector u, Vector v, double baseA, double legB, double legC)
        {
            double half = baseA / 2.0;
            Point pBL = center - (u * half);
            Point pBR = center + (u * half);
            Point pTL = pBL + (v * legB);
            Point pTR = pBR + (v * legC);
            return new List<Point> { pTL, pBL, pBR, pTR };
        }

        public static List<Point> GenerateShape15Points(Point center, Vector u, Vector v, double webA, double flangeB, double flangeC)
        {
            double half = webA / 2.0;
            Point pBottom = center - (v * half);
            Point pTop = center + (v * half);
            Point pFlange1 = pBottom - (u * flangeB);
            Point pFlange2 = pTop + (u * flangeC);
            return new List<Point> { pFlange1, pBottom, pTop, pFlange2 };
        }

        public static List<Point> GenerateShape21Points(Point center, Vector u, Vector v, double widthA, double heightB, double heightC)
        {
            double hw = widthA / 2.0;
            Point pBL = center - (u * hw);
            Point pBR = center + (u * hw);
            Point pTL = pBL + (v * heightB);
            Point pTR = pBR + (v * heightC);
            return new List<Point> { pTL, pBL, pBR, pTR };
        }

        public static List<Point> GenerateShape22Points(Point center, Vector u, Vector v, double widthA, double heightB, double hookC)
        {
            double hw = widthA / 2.0;
            Point pBL = center - (u * hw);
            Point pBR = center + (u * hw);
            Point pTL = pBL + (v * heightB);
            Point pTR = pBR + (v * heightB);
            Point pHookL = pTL + (u * hookC);
            Point pHookR = pTR - (u * hookC);
            return new List<Point> { pHookL, pTL, pBL, pBR, pTR, pHookR };
        }

        public static List<Point> GenerateShape25Points(Point center, Vector u, Vector v, double widthA, double heightB, double hookC)
        {
            double hw = widthA / 2.0;
            Point pBL = center - (u * hw);
            Point pBR = center + (u * hw);
            Point pTL = pBL + (v * heightB);
            Point pTR = pBR + (v * heightB);
            Point pHookL = pTL - (u * hookC);
            Point pHookR = pTR + (u * hookC);
            return new List<Point> { pHookL, pTL, pBL, pBR, pTR, pHookR };
        }

        public static List<Point> GenerateShape31Points(Point center, Vector u, Vector v, double legA, double legB, double hookC)
        {
            Point pCorner = center;
            Point pTop = pCorner + (v * legB);
            Point pRight = pCorner + (u * legA);
            double cos45 = Math.Cos(Math.PI / 4.0);
            double sin45 = Math.Sin(Math.PI / 4.0);
            Vector hookDir = new Vector(-u.X * cos45 + v.X * sin45, -u.Y * cos45 + v.Y * sin45, -u.Z * cos45 + v.Z * sin45).GetNormal() * hookC;
            Point pHook = pRight + hookDir;
            return new List<Point> { pTop, pCorner, pRight, pHook };
        }

        public static List<Point> GenerateShape51Points(Point center, Vector u, Vector v, double widthA, double heightB)
        {
            double hw = widthA / 2.0;
            double hh = heightB / 2.0;
            Point pTL = center - (u * hw) + (v * hh);
            Point pBL = center - (u * hw) - (v * hh);
            Point pBR = center + (u * hw) - (v * hh);
            Point pTR = center + (u * hw) + (v * hh);
            return new List<Point> { pTL, pBL, pBR, pTR };
        }

        public static List<Point> GenerateClosedStirrupPoints(Point center, Vector u, Vector v, double widthA, double heightB, string corner = "Auto")
        {
            double hw = widthA / 2.0;
            double hh = heightB / 2.0;
            Point cTR = center + (u * hw) + (v * hh);
            Point cTL = center - (u * hw) + (v * hh);
            Point cBL = center - (u * hw) - (v * hh);
            Point cBR = center + (u * hw) - (v * hh);
            string c = (corner ?? "").ToLowerInvariant();
            if (c.Contains("top") && c.Contains("left"))
            {
                return new List<Point> { cTL, cBL, cBR, cTR, cTL };
            }
            if (c.Contains("bot") && c.Contains("left"))
            {
                return new List<Point> { cBL, cBR, cTR, cTL, cBL };
            }
            if (c.Contains("bot") && c.Contains("right"))
            {
                return new List<Point> { cBR, cTR, cTL, cBL, cBR };
            }
            // Default according to STShapeBlockAll_230926.dwg (SSBAR_SHP_56) standard: Top-Right
            return new List<Point> { cTR, cTL, cBL, cBR, cTR };
        }

        public static List<Point> GenerateShape41Points(Point center, Vector u, Vector v, double widthA, double heightB, string corner = "Auto")
        {
            return GenerateClosedStirrupPoints(center, u, v, widthA, heightB, corner);
        }

        public static List<Point> GenerateShape54Points(Point center, Vector u, Vector v, double widthA, double heightB, double hookC, string corner = "Auto")
        {
            return GenerateClosedStirrupPoints(center, u, v, widthA, heightB, corner);
        }

        public static List<Point> GenerateShape55Points(Point center, Vector u, Vector v, double widthA, double heightB, double hookC, string corner = "Auto")
        {
            return GenerateClosedStirrupPoints(center, u, v, widthA, heightB, corner);
        }

        public static List<Point> GenerateShape56Points(Point center, Vector u, Vector v, double widthA, double heightB, double hookC, string corner = "Auto")
        {
            return GenerateClosedStirrupPoints(center, u, v, widthA, heightB, corner);
        }

        public static List<Point> GenerateShape60Points(Point center, Vector u, Vector v, double widthA, double heightB)
        {
            double hw = widthA / 2.0;
            double hh = heightB / 2.0;
            Point pTop = center + (v * hh);
            Point pRight = center + (u * hw);
            Point pBottom = center - (v * hh);
            Point pLeft = center - (u * hw);
            return new List<Point> { pTop, pLeft, pBottom, pRight, pTop };
        }

        public static List<Point> GenerateShape62Points(Point center, Vector u, Vector v, double baseA, double heightB)
        {
            double hw = baseA / 2.0;
            Point pTop = center + (v * (heightB / 2.0));
            Point pBL = center - (u * hw) - (v * (heightB / 2.0));
            Point pBR = center + (u * hw) - (v * (heightB / 2.0));
            return new List<Point> { pTop, pBL, pBR, pTop };
        }

        public static List<Point> GenerateShape67Points(Point center, Vector u, Vector v, double diaA)
        {
            double r = diaA / 2.0;
            // Use 24+ segments for smooth circular hoop recognition in Tekla (32 segments for large hoops D > 800mm)
            int segments = diaA > 800.0 ? 32 : 24;
            var pts = new List<Point>();
            for (int i = 0; i <= segments; i++)
            {
                double angle = (2.0 * Math.PI * i) / segments;
                pts.Add(center + (u * (r * Math.Cos(angle))) + (v * (r * Math.Sin(angle))));
            }
            return pts;
        }

        public static List<Point> GenerateShape72Points(Point center, Vector u, Vector v, double lengthA, double widthB, double hookC)
        {
            double hw = widthB / 2.0;
            double hl = lengthA / 2.0;
            Point pBL = center - (u * hl) - (v * hw);
            Point pBR = center + (u * hl) - (v * hw);
            Point pTR = center + (u * hl) + (v * hw);
            Point pTL = center - (u * hl) + (v * hw);
            return new List<Point> { pBL - (v * hookC), pBL, pBR, pTR, pTL, pTL - (v * hookC) };
        }

        public static List<Point> GenerateShape74Points(Point center, Vector u, Vector v, double topBaseA, double heightB, double legC, double legD)
        {
            double hw = topBaseA / 2.0;
            Point pTopL = center - (u * hw) + (v * (heightB / 2.0));
            Point pTopR = center + (u * hw) + (v * (heightB / 2.0));
            Point pBottomL = pTopL - (v * heightB);
            Point pBottomR = pTopR - (v * heightB);
            Point pLegL = pBottomL - (u * legC);
            Point pLegR = pBottomR + (u * legD);
            return new List<Point> { pLegL, pBottomL, pTopL, pTopR, pBottomR, pLegR };
        }

        public static List<Point> GenerateShape370Points(Point center, Vector u, Vector v, double widthA, double heightB, double hookC)
        {
            double hw = widthA / 2.0;
            double hh = heightB / 2.0;
            Point pTL = center - (u * hw) + (v * hh);
            Point pBL = center - (u * hw) - (v * hh);
            Point pBR = center + (u * hw) - (v * hh);
            Point pTR = center + (u * hw) + (v * hh);

            Point pHookL = pTL + (u * hookC);
            Point pHookR = pTR - (u * hookC);

            return new List<Point> { pHookL, pTL, pBL, pBR, pTR, pHookR };
        }

        public static (double Width, double Height, bool Success) DetectConcreteDimensions(Reinforcement rebar, double cover, Model model = null)
        {
            try
            {
                if (rebar == null) return (300.0, 500.0, false);
                var m = model ?? new global::Tekla.Structures.Model.Model();
                Part part = rebar.Father as Part;
                if (part == null)
                {
                    int fatherId = 0;
                    if (rebar.GetReportProperty("FATHER_ID", ref fatherId) && fatherId > 0)
                    {
                        part = m.SelectModelObject(new global::Tekla.Structures.Identifier(fatherId)) as Part;
                    }
                }
                if (part == null)
                {
                    int mainPartId = 0;
                    if (rebar.GetReportProperty("MAIN_PART.ID", ref mainPartId) && mainPartId > 0)
                    {
                        part = m.SelectModelObject(new global::Tekla.Structures.Identifier(mainPartId)) as Part;
                    }
                }

                if (part != null)
                {
                    double w = 0, h = 0;
                    part.GetReportProperty("WIDTH", ref w);
                    part.GetReportProperty("HEIGHT", ref h);

                    if (w > 0 && h > 0)
                    {
                        double autoA = Math.Max(50.0, w - 2.0 * cover);
                        double autoB = Math.Max(50.0, h - 2.0 * cover);
                        return (autoA, autoB, true);
                    }

                    var solid = part.GetSolid();
                    if (solid != null)
                    {
                        double dx = Math.Abs(solid.MaximumPoint.X - solid.MinimumPoint.X);
                        double dy = Math.Abs(solid.MaximumPoint.Y - solid.MinimumPoint.Y);
                        double dz = Math.Abs(solid.MaximumPoint.Z - solid.MinimumPoint.Z);

                        var dims = new List<double> { dx, dy, dz };
                        dims.Sort();
                        double autoA = Math.Max(50.0, dims[0] - 2.0 * cover);
                        double autoB = Math.Max(50.0, dims[1] - 2.0 * cover);
                        return (autoA, autoB, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Could not detect host dimensions: " + ex.Message);
            }

            return (300.0, 500.0, false);
        }

        public static bool ApplyShapeToRebar(
            Reinforcement rebar,
            List<Point> newPoints,
            double rebarDia,
            string shapeCode,
            double hookLen,
            RebarUndoRecord undoRecord,
            out string error)
        {
            return ApplyPolygonToRebar(rebar, newPoints, rebarDia, shapeCode, hookLen, undoRecord, out error);
        }

        private static void SetHook(RebarHookData hook, RebarHookData.RebarHookShapeEnum shape, double angle, double length, double radius = 0.0)
        {
            if (hook == null) return;
            hook.Shape = shape;
            hook.Angle = angle;
            hook.Length = length;
            hook.Radius = radius;
        }

        private static void ClearHook(RebarHookData hook)
        {
            SetHook(hook, RebarHookData.RebarHookShapeEnum.NO_HOOK, 0.0, 0.0, 0.0);
        }

        private static void ConfigureHooks(RebarHookData startHook, RebarHookData endHook, string sCode, double validHook)
        {
            if (startHook == null || endHook == null) return;

            // 0. Pure polygon shapes MUST NOT have any Tekla hooks (Shape 4, 45, 1, 2, 3, 21, 41, 51, etc.)
            if (!HdcRebarShapeRulesManager.IsNaturallyHookedShape(sCode))
            {
                ClearHook(startHook);
                ClearHook(endHook);
                return;
            }

            // 1. Check if HDC RebarShapeRules defines hooks for this shape
            try
            {
                var rule = HdcRebarShapeRulesManager.GetRule(sCode);
                if (rule != null)
                {
                    if (rule.StartHook != null && rule.StartHook.HasHook)
                    {
                        SetHook(startHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, rule.StartHook.Angle,
                            validHook > 0 ? validHook : rule.StartHook.Length,
                            rule.StartHook.Radius > 0 ? rule.StartHook.Radius : 0.0);
                    }
                    else
                    {
                        ClearHook(startHook);
                    }

                    if (rule.EndHook != null && rule.EndHook.HasHook)
                    {
                        SetHook(endHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, rule.EndHook.Angle,
                            validHook > 0 ? validHook : rule.EndHook.Length,
                            rule.EndHook.Radius > 0 ? rule.EndHook.Radius : 0.0);
                    }
                    else
                    {
                        ClearHook(endHook);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                Log("Error configuring hooks via HDC rules: " + ex.Message);
            }

            // 2. Built-in fallback
            if (sCode == "56")
            {
                SetHook(startHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 135.0, validHook);
                SetHook(endHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 135.0, validHook);
            }
            else if (sCode == "54")
            {
                SetHook(startHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 90.0, validHook);
                SetHook(endHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 90.0, validHook);
            }
            else if (sCode == "55")
            {
                // Shape 55: StartHook=90°, EndHook=135° (matches HDC Korea standard)
                SetHook(startHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 90.0, validHook);
                SetHook(endHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 135.0, validHook);
            }
            else if (sCode == "11")
            {
                SetHook(startHook, RebarHookData.RebarHookShapeEnum.HOOK_180_DEGREES, 180.0, validHook);
                SetHook(endHook, RebarHookData.RebarHookShapeEnum.HOOK_180_DEGREES, 180.0, validHook);
            }
            else if (sCode == "12")
            {
                ClearHook(startHook);
                SetHook(endHook, RebarHookData.RebarHookShapeEnum.CUSTOM_HOOK, 90.0, validHook);
            }
            else
            {
                // For all other shapes, clear leftover hooks
                ClearHook(startHook);
                ClearHook(endHook);
            }
        }

        public static ArrayList ClonePointsArrayList(ArrayList points)
        {
            if (points == null) return new ArrayList();
            var result = new ArrayList();
            foreach (var item in points)
            {
                if (item is Point p)
                    result.Add(new Point(p.X, p.Y, p.Z));
            }
            return result;
        }

        public static ArrayList CloneRadiusValuesArrayList(ArrayList radii)
        {
            if (radii == null) return new ArrayList();
            var result = new ArrayList();
            foreach (var item in radii)
            {
                if (item is double d)
                    result.Add(d);
                else if (item is IConvertible conv)
                    result.Add(conv.ToDouble(System.Globalization.CultureInfo.InvariantCulture));
            }
            return result;
        }

        /// <summary>
        /// Retrieves the StartHook and EndHook data objects for any supported Tekla reinforcement type.
        /// </summary>
        public static bool GetRebarHooks(Reinforcement rebar, out RebarHookData startHook, out RebarHookData endHook)
        {
            if (rebar is SingleRebar sr)
            {
                startHook = sr.StartHook;
                endHook = sr.EndHook;
                return true;
            }
            if (rebar is BaseRebarGroup brg)
            {
                startHook = brg.StartHook;
                endHook = brg.EndHook;
                return true;
            }

            startHook = null;
            endHook = null;
            return false;
        }

        /// <summary>
        /// Backups reinforcement geometry, radius values, and hook settings into an undo record.
        /// </summary>
        private static void BackupRebarState(Reinforcement rebar, RebarUndoRecord undoRecord)
        {
            if (undoRecord == null || rebar == null) return;

            undoRecord.ModelObject = rebar;
            undoRecord.BackupPolygons.Clear();

            // Backup hook configurations
            if (GetRebarHooks(rebar, out var sHook, out var eHook))
            {
                if (sHook != null)
                {
                    undoRecord.BackupStartHookShape = sHook.Shape;
                    undoRecord.BackupStartHookAngle = sHook.Angle;
                    undoRecord.BackupStartHookLength = sHook.Length;
                    undoRecord.BackupStartHookRadius = sHook.Radius;
                }
                if (eHook != null)
                {
                    undoRecord.BackupEndHookShape = eHook.Shape;
                    undoRecord.BackupEndHookAngle = eHook.Angle;
                    undoRecord.BackupEndHookLength = eHook.Length;
                    undoRecord.BackupEndHookRadius = eHook.Radius;
                }
            }

            // Backup radius values
            if (rebar.RadiusValues != null)
            {
                undoRecord.BackupRadiusValues = CloneRadiusValuesArrayList(rebar.RadiusValues);
            }

            // Backup polygon points according to specific rebar class
            if (rebar is SingleRebar sr)
            {
                if (sr.Polygon?.Points != null)
                    undoRecord.BackupPolygons.Add(ClonePointsArrayList(sr.Polygon.Points));
            }
            else if (rebar is RebarGroup rg)
            {
                if (rg.Polygons != null)
                {
                    foreach (Polygon poly in rg.Polygons)
                    {
                        if (poly?.Points != null)
                            undoRecord.BackupPolygons.Add(ClonePointsArrayList(poly.Points));
                    }
                }
            }
            else if (rebar is CircleRebarGroup crg)
            {
                if (crg.Polygon?.Points != null)
                    undoRecord.BackupPolygons.Add(ClonePointsArrayList(crg.Polygon.Points));
            }
            else if (rebar is CurvedRebarGroup cvrg)
            {
                if (cvrg.Polygon?.Points != null)
                    undoRecord.BackupPolygons.Add(ClonePointsArrayList(cvrg.Polygon.Points));
            }
        }

        /// <summary>
        /// Assigns the target polygon geometry to the reinforcement object.
        /// Handles multi-polygon displacement for tapered or multi-polygon RebarGroups.
        /// </summary>
        private static bool ApplyPolygonGeometry(
            Reinforcement rebar,
            Polygon newPolygon,
            List<Point> newPoints,
            out string error)
        {
            error = string.Empty;

            if (rebar is SingleRebar sr)
            {
                sr.Polygon = newPolygon;
                return true;
            }
            if (rebar is CircleRebarGroup crg)
            {
                crg.Polygon = newPolygon;
                return true;
            }
            if (rebar is CurvedRebarGroup cvrg)
            {
                cvrg.Polygon = newPolygon;
                return true;
            }
            if (rebar is RebarGroup rg)
            {
                if (rg.Polygons != null && rg.Polygons.Count > 1)
                {
                    // Preserve relative position for tapered or multi-polygon groups
                    var oldFirstPoly = rg.Polygons[0] as Polygon;
                    Point oldFirstRef = (oldFirstPoly?.Points != null && oldFirstPoly.Points.Count > 0)
                        ? (Point)oldFirstPoly.Points[0]
                        : new Point();

                    var updatedPolys = new ArrayList();
                    for (int pIdx = 0; pIdx < rg.Polygons.Count; pIdx++)
                    {
                        var oldP = rg.Polygons[pIdx] as Polygon;
                        Vector shift = new Vector();
                        if (oldP?.Points != null && oldP.Points.Count > 0)
                        {
                            shift = new Vector((Point)oldP.Points[0] - oldFirstRef);
                        }
                        var shiftedPts = new ArrayList();
                        foreach (var pt in newPoints)
                        {
                            shiftedPts.Add(new Point(pt.X + shift.X, pt.Y + shift.Y, pt.Z + shift.Z));
                        }
                        updatedPolys.Add(new Polygon { Points = shiftedPts });
                    }

                    rg.Polygons.Clear();
                    foreach (Polygon p in updatedPolys) rg.Polygons.Add(p);
                }
                else
                {
                    rg.Polygons.Clear();
                    rg.Polygons.Add(newPolygon);
                }
                return true;
            }

            error = "Unsupported rebar type: " + rebar.GetType().Name;
            return false;
        }

        public static bool ApplyPolygonToRebar(
            Reinforcement rebar,
            List<Point> newPoints,
            double rebarDia,
            string shapeCode,
            double hookLen,
            RebarUndoRecord undoRecord,
            out string error)
        {
            error = string.Empty;
            try
            {
                if (rebar == null)
                {
                    error = "Rebar object is null.";
                    return false;
                }

                if (newPoints == null || newPoints.Count < 2)
                {
                    error = "Polygon requires at least 2 points.";
                    return false;
                }

                // 1. Record undo snapshot
                BackupRebarState(rebar, undoRecord);

                // 2. Configure hooks
                string sCode = shapeCode?.Trim() ?? "";
                double validHook = hookLen > 1.0 ? hookLen : Math.Max(80.0, rebarDia * 10.0);
                if (GetRebarHooks(rebar, out var startHook, out var endHook))
                {
                    ConfigureHooks(startHook, endHook, sCode, validHook);
                }

                // 3. Build polygon and radius list
                var arrPoints = new ArrayList();
                foreach (var pt in newPoints)
                {
                    arrPoints.Add(new Point(pt.X, pt.Y, pt.Z));
                }
                var newPolygon = new Polygon { Points = arrPoints };

                double radius = Math.Max(16.0, rebarDia * 2.0);
                var radList = new ArrayList();
                for (int i = 0; i < newPoints.Count; i++)
                {
                    radList.Add(radius);
                }
                rebar.RadiusValues = radList;

                // 4. Apply geometry
                if (!ApplyPolygonGeometry(rebar, newPolygon, newPoints, out error))
                {
                    return false;
                }

                // 5. Commit changes to Tekla model object
                return rebar.Modify();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Restores reinforcement state (hooks, radius values, and polygon points) from an undo record.
        /// Unifies restoration across SingleRebar, RebarGroup, CircleRebarGroup, and CurvedRebarGroup.
        /// </summary>
        public static bool RestoreRebarFromUndoRecord(RebarUndoRecord record, out string error)
        {
            error = string.Empty;
            if (record == null)
            {
                error = "Undo record is null.";
                return false;
            }

            var rebar = record.ModelObject;
            if (rebar == null)
            {
                error = "ModelObject is not a valid Reinforcement.";
                return false;
            }

            if (record.BackupPolygons == null || record.BackupPolygons.Count == 0)
            {
                error = "Undo record contains no backup polygons.";
                return false;
            }

            try
            {
                // 1. Restore Hook Data
                if (GetRebarHooks(rebar, out var startHook, out var endHook))
                {
                    if (startHook != null)
                    {
                        startHook.Shape = record.BackupStartHookShape;
                        startHook.Angle = record.BackupStartHookAngle;
                        startHook.Length = record.BackupStartHookLength;
                        startHook.Radius = record.BackupStartHookRadius;
                    }
                    if (endHook != null)
                    {
                        endHook.Shape = record.BackupEndHookShape;
                        endHook.Angle = record.BackupEndHookAngle;
                        endHook.Length = record.BackupEndHookLength;
                        endHook.Radius = record.BackupEndHookRadius;
                    }
                }

                // 2. Restore Polygons
                var restoredPolys = record.CreateDeepCopyPolygons();
                if (restoredPolys.Count == 0)
                {
                    error = "No polygons could be restored.";
                    return false;
                }

                if (rebar is SingleRebar sr)
                {
                    sr.Polygon = new Polygon { Points = (ArrayList)restoredPolys[0] };
                }
                else if (rebar is CircleRebarGroup crg)
                {
                    crg.Polygon = new Polygon { Points = (ArrayList)restoredPolys[0] };
                }
                else if (rebar is CurvedRebarGroup cvrg)
                {
                    cvrg.Polygon = new Polygon { Points = (ArrayList)restoredPolys[0] };
                }
                else if (rebar is RebarGroup rg)
                {
                    rg.Polygons.Clear();
                    foreach (ArrayList pts in restoredPolys)
                    {
                        rg.Polygons.Add(new Polygon { Points = pts });
                    }
                }
                else
                {
                    error = "Unsupported rebar type: " + rebar.GetType().Name;
                    return false;
                }

                // 3. Restore Radius Values
                var restoredRadii = record.CreateDeepCopyRadiusValues();
                if (restoredRadii.Count > 0)
                {
                    rebar.RadiusValues = restoredRadii;
                }
                else
                {
                    double dia = 10.0;
                    rebar.GetReportProperty("DIAMETER", ref dia);
                    var defaultRads = new ArrayList();
                    int ptCount = 4;
                    if (restoredPolys.Count > 0 && restoredPolys[0] is ArrayList pts)
                    {
                        ptCount = pts.Count;
                    }
                    for (int i = 0; i < ptCount; i++)
                    {
                        defaultRads.Add(Math.Max(16.0, dia * 2.0));
                    }
                    rebar.RadiusValues = defaultRads;
                }

                // 4. Commit modification to Tekla model object
                return rebar.Modify();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
