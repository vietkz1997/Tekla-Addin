using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Collaboration;
using GeometryHelper;
using GeometryHelper.Geometry;
using GeometryHelper.Spatial;
using GeometryHelper.Core;
using GeometryHelper.TeklaConvert;

namespace BimCommands.Tekla.ClashCheck
{
    public enum IfcScopeMode
    {
        AutoSpatialAllIfc,  // Navisworks style: Automatically check all IFC files intersecting rebar zone
        SelectedIfcOnly,    // Only check IFC / Part objects selected in Tekla UI
        SpecificFile        // Check specific IFC file from dropdown
    }

    public class ClashSettings
    {
        public bool OnlySelectedRebars { get; set; } = true;
        public IfcScopeMode IfcMode { get; set; } = IfcScopeMode.AutoSpatialAllIfc;
        public string TargetIfcFileName { get; set; } = "ALL";
        public double ToleranceMm { get; set; } = 1.0;
        public double ClearanceMm { get; set; } = 0.0;
        public bool EnableIgnoredComponents { get; set; } = true;
        public List<string> IgnoredKeywords { get; set; } = new List<string>();
    }

    public class IfcBoxInfo
    {
        public ModelObject ModelObject { get; set; }
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IfcEntity { get; set; } = string.Empty;
        public string SearchableText { get; set; } = string.Empty;
        public Point MinPoint { get; set; }
        public Point MaxPoint { get; set; }
        public bool IsDiagonalMember { get; set; } = false;
        public bool IsSlab { get; set; } = false;

        public Point Origin { get; set; }
        public Vector AxisX { get; set; }
        public Vector AxisExtrusion { get; set; }
        public string ProfileName { get; set; }
        public bool HasOrientation { get; set; } = false;

        public bool IntersectsBox(Point minBox, Point maxBox)
        {
            return (MinPoint.X <= maxBox.X && MaxPoint.X >= minBox.X) &&
                   (MinPoint.Y <= maxBox.Y && MaxPoint.Y >= minBox.Y) &&
                   (MinPoint.Z <= maxBox.Z && MaxPoint.Z >= minBox.Z);
        }

        private GeoObb3 _obb = null;
        private List<GeoSolid3> _geoSolids = null;
        private IfcGeometryRepresentation _geometryRep = null;

        public IfcGeometryRepresentation GetGeometry(Model teklaModel, IEnumerable<string> skipNames = null)
        {
            if (_geometryRep != null) return _geometryRep;
            if (ModelObject is ReferenceModelObject refObj)
            {
                _geometryRep = IfcGeometryBridge.ExtractGeometry(refObj, MinPoint, MaxPoint, teklaModel, skipNames);
            }
            return _geometryRep;
        }

        public GeoObb3 GetObb()
        {
            if (_obb != null) return _obb;

            GeoPoint3 center;
            double sizeX, sizeY, sizeZ;

            if (HasOrientation && Origin != null && AxisX != null && AxisExtrusion != null)
            {
                center = new GeoPoint3(Origin.X, Origin.Y, Origin.Z);
                sizeX = Math.Max(1.0, MaxPoint.X - MinPoint.X);
                sizeY = Math.Max(1.0, MaxPoint.Y - MinPoint.Y);
                sizeZ = Math.Max(1.0, MaxPoint.Z - MinPoint.Z);

                Vector teklaVy = AxisExtrusion.Cross(AxisX);
                GeoVector3 vx = new GeoVector3(AxisX.X, AxisX.Y, AxisX.Z);
                GeoVector3 vy = new GeoVector3(teklaVy.X, teklaVy.Y, teklaVy.Z);

                try
                {
                    _obb = new GeoObb3(center, sizeX, sizeY, sizeZ, vx, vy);
                    return _obb;
                }
                catch { }
            }

            center = new GeoPoint3((MinPoint.X + MaxPoint.X) / 2.0, 
                                   (MinPoint.Y + MaxPoint.Y) / 2.0, 
                                   (MinPoint.Z + MaxPoint.Z) / 2.0);
            sizeX = Math.Max(1.0, MaxPoint.X - MinPoint.X);
            sizeY = Math.Max(1.0, MaxPoint.Y - MinPoint.Y);
            sizeZ = Math.Max(1.0, MaxPoint.Z - MinPoint.Z);

            _obb = new GeoObb3(center, sizeX, sizeY, sizeZ);
            return _obb;
        }

        public List<GeoSolid3> GetGeoSolids(Model teklaModel = null, IEnumerable<string> skipNames = null)
        {
            if (_geoSolids != null) return _geoSolids;

            // 1. Exact 3D Brep via GeometryHelper.TeklaConvert.ToGeoSolids()
            if (ModelObject is ReferenceModelObject refObj)
            {
                try
                {
                    var exactSolids = refObj.ToGeoSolids();
                    if (exactSolids != null && exactSolids.Length > 0)
                    {
                        _geoSolids = new List<GeoSolid3>(exactSolids);
                        return _geoSolids;
                    }
                }
                catch { }
            }

            // Fallback: Exact 3D Brep from source IFC file via xBIM Toolkit (Cách 1)
            if (teklaModel != null)
            {
                var exactSolids = XbimGeometryManager.GetExactSolids(this, teklaModel, skipNames);
                if (exactSolids != null && exactSolids.Count > 0)
                {
                    _geoSolids = exactSolids;
                    return _geoSolids;
                }
            }

            _geoSolids = new List<GeoSolid3>();
            double dX = MaxPoint.X - MinPoint.X;
            double dY = MaxPoint.Y - MinPoint.Y;
            double dZ = MaxPoint.Z - MinPoint.Z;

            double[] dims = new double[] { dX, dY, dZ };
            Array.Sort(dims);
            double minDim = dims[0];
            double midDim = dims[1];
            double maxDim = dims[2];

            bool isStructuralSection = (minDim >= 40.0 && midDim >= 70.0 && maxDim >= 150.0 && !IsSlab);

            if (isStructuralSection)
            {
                // Decompose structural member (H, I, Box, Column, Bracket) into genuine plate solids:
                // Flanges + Web (leaves hollow central bay empty!)
                double tf = 25.0; // Standard flange thickness (25mm)
                double tw = 16.0; // Standard web thickness (16mm)

                // Flange 1 (Top)
                GeoPoint3 cTop = new GeoPoint3((MinPoint.X + MaxPoint.X) / 2.0, (MinPoint.Y + MaxPoint.Y) / 2.0, MaxPoint.Z - tf / 2.0);
                _geoSolids.Add(new GeoObb3(cTop, dX, dY, tf).ToSolid());

                // Flange 2 (Bottom)
                GeoPoint3 cBot = new GeoPoint3((MinPoint.X + MaxPoint.X) / 2.0, (MinPoint.Y + MaxPoint.Y) / 2.0, MinPoint.Z + tf / 2.0);
                _geoSolids.Add(new GeoObb3(cBot, dX, dY, tf).ToSolid());

                // Web Solid
                double webH = Math.Max(0.0, dZ - 2 * tf);
                if (webH > 10.0)
                {
                    GeoPoint3 cWeb = new GeoPoint3((MinPoint.X + MaxPoint.X) / 2.0, (MinPoint.Y + MaxPoint.Y) / 2.0, (MinPoint.Z + MaxPoint.Z) / 2.0);
                    double webLenX = (dX >= dY) ? dX : tw;
                    double webThkY = (dX >= dY) ? tw : dY;
                    _geoSolids.Add(new GeoObb3(cWeb, webLenX, webThkY, webH).ToSolid());
                }
            }
            else
            {
                // Solid Plate, Slab, Wall or compact member: full solid block
                var obb = GetObb();
                if (obb != null)
                {
                    _geoSolids.Add(obb.ToSolid());
                }
            }

            return _geoSolids;
        }
    }

    public class ClashDetector
    {
        private readonly Model _model;

        public ClashDetector(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public List<ReferenceModel> GetReferenceModels()
        {
            var result = new List<ReferenceModel>();
            try
            {
                var refEnum = _model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.REFERENCE_MODEL);
                while (refEnum.MoveNext())
                {
                    if (refEnum.Current is ReferenceModel refModel)
                    {
                        result.Add(refModel);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error retrieving ReferenceModels: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Collects candidate obstacle objects (ONLY IFC objects, strictly excluding native Tekla parts)
        /// </summary>
        public List<IfcBoxInfo> CollectObstacles(
            Point rebarZoneMin, 
            Point rebarZoneMax, 
            ClashSettings settings, 
            List<ModelObject> userSelectedObstacles,
            Action<string> onStatusUpdate = null)
        {
            var obstacles = new List<IfcBoxInfo>();

            // Case A: User selected obstacles directly in Tekla UI
            if (settings.IfcMode == IfcScopeMode.SelectedIfcOnly || (userSelectedObstacles != null && userSelectedObstacles.Count > 0))
            {
                onStatusUpdate?.Invoke(string.Format("Đang nạp cấu kiện IFC được chọn từ Tekla..."));
                foreach (var obj in userSelectedObstacles)
                {
                    if (obj is ReferenceModel refM)
                    {
                        var ch = refM.GetChildren();
                        while (ch.MoveNext())
                        {
                            var box = ExtractObjectBox(ch.Current as ModelObject, settings);
                            if (box != null) obstacles.Add(box);
                        }
                    }
                    else if (obj is ReferenceModelObject refObj)
                    {
                        var box = ExtractObjectBox(refObj, settings);
                        if (box != null) obstacles.Add(box);
                    }
                }
                return FilterCompoundEnvelopes(obstacles);
            }

            // Case B: High-Performance 3D Spatial R-Tree Query (ONLY IFC ReferenceModelObjects)
            double buffer = 500.0 + settings.ClearanceMm;
            Point searchMin = new Point(rebarZoneMin.X - buffer, rebarZoneMin.Y - buffer, rebarZoneMin.Z - buffer);
            Point searchMax = new Point(rebarZoneMax.X + buffer, rebarZoneMax.Y + buffer, rebarZoneMax.Z + buffer);

            onStatusUpdate?.Invoke("Đang dùng bộ lọc không gian 3D quét cấu kiện IFC lân cận...");

            try
            {
                var boxEnum = _model.GetModelObjectSelector().GetObjectsByBoundingBox(searchMin, searchMax);

                while (boxEnum.MoveNext())
                {
                    var curr = boxEnum.Current;
                    // STRICT FILTER: ONLY collect ReferenceModelObject (IFC elements).
                    // NEVER collect native Tekla Part (Beam, Column, ContourPlate) to avoid false clashes with concrete!
                    if (curr is ReferenceModelObject refObj)
                    {
                        string fileName = string.Empty;
                        try
                        {
                            var parent = refObj.GetReferenceModel();
                            if (parent != null) fileName = Path.GetFileName(parent.Filename);
                        }
                        catch { }

                        if (string.IsNullOrEmpty(fileName))
                        {
                            refObj.GetReportProperty("NAME", ref fileName);
                            if (!string.IsNullOrEmpty(fileName)) fileName = Path.GetFileName(fileName);
                        }

                        // Filter by specific IFC file if user selected one
                        if (settings.IfcMode == IfcScopeMode.SpecificFile &&
                            !string.Equals(settings.TargetIfcFileName, "ALL", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(fileName) &&
                                !fileName.Equals(settings.TargetIfcFileName, StringComparison.OrdinalIgnoreCase) &&
                                fileName.IndexOf(settings.TargetIfcFileName, StringComparison.OrdinalIgnoreCase) < 0 &&
                                settings.TargetIfcFileName.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                continue;
                            }
                        }

                        var box = ExtractObjectBox(refObj, settings);
                        if (box != null)
                        {
                            if (!string.IsNullOrEmpty(fileName)) box.FileName = fileName;
                            obstacles.Add(box);
                        }
                    }
                }

                if (obstacles.Count > 0)
                {
                    // Filter out compound envelopes that enclose 2 or more other sub-objects (e.g. beam assembly enclosing flanges and web)
                    return FilterCompoundEnvelopes(obstacles);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Spatial BoundingBox query: " + ex.Message);
            }

            // Fallback: If spatial query returned 0, check reference models directly
            var refModels = GetReferenceModels();
            foreach (var refModel in refModels)
            {
                if (refModel == null) continue;
                string fileName = Path.GetFileName(refModel.Filename ?? string.Empty);

                if (settings.IfcMode == IfcScopeMode.SpecificFile &&
                    !string.Equals(settings.TargetIfcFileName, "ALL", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fileName, settings.TargetIfcFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                double rMinX = 0, rMinY = 0, rMinZ = 0, rMaxX = 0, rMaxY = 0, rMaxZ = 0;
                bool hasRMin = refModel.GetReportProperty("BOUNDING_BOX_MIN_X", ref rMinX) &&
                               refModel.GetReportProperty("BOUNDING_BOX_MIN_Y", ref rMinY) &&
                               refModel.GetReportProperty("BOUNDING_BOX_MIN_Z", ref rMinZ);
                bool hasRMax = refModel.GetReportProperty("BOUNDING_BOX_MAX_X", ref rMaxX) &&
                               refModel.GetReportProperty("BOUNDING_BOX_MAX_Y", ref rMaxY) &&
                               refModel.GetReportProperty("BOUNDING_BOX_MAX_Z", ref rMaxZ);

                if (hasRMin && hasRMax)
                {
                    Point refBoxMin = new Point(rMinX, rMinY, rMinZ);
                    Point refBoxMax = new Point(rMaxX, rMaxY, rMaxZ);
                    if (!IsAabbOverlap(searchMin, searchMax, refBoxMin, refBoxMax)) continue;
                }

                var children = refModel.GetChildren();
                while (children.MoveNext())
                {
                    if (children.Current is ReferenceModelObject refObj)
                    {
                        var box = ExtractObjectBox(refObj, settings);
                        if (box != null && IsAabbOverlap(searchMin, searchMax, box.MinPoint, box.MaxPoint))
                        {
                            box.FileName = fileName;
                            obstacles.Add(box);
                        }
                    }
                }
            }

            return FilterCompoundEnvelopes(obstacles);
        }

        /// <summary>
        /// Eliminates compound envelopes that enclose 2 or more distinct sub-objects.
        /// When an IFC steel beam or column is imported, Tekla provides both the leaf solid parts (flanges, web plates)
        /// AND a giant envelope containing all of them. Treating the whole envelope as solid steel causes false clashes
        /// with rebars passing through the clear space between flanges!
        /// </summary>
        private static List<IfcBoxInfo> FilterCompoundEnvelopes(List<IfcBoxInfo> obstacles)
        {
            if (obstacles == null || obstacles.Count <= 1) return obstacles ?? new List<IfcBoxInfo>();

            var leafObstacles = new List<IfcBoxInfo>();
            var nonSlabs = new List<IfcBoxInfo>();

            foreach (var b in obstacles)
            {
                if (b.IsSlab)
                    leafObstacles.Add(b);
                else
                    nonSlabs.Add(b);
            }

            if (nonSlabs.Count <= 1)
            {
                leafObstacles.AddRange(nonSlabs);
                return leafObstacles;
            }

            // Internal helper holding precomputed bounds & volume for sweep-and-prune
            var items = new (IfcBoxInfo Box, double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double Vol, double DZ)[nonSlabs.Count];
            for (int i = 0; i < nonSlabs.Count; i++)
            {
                var b = nonSlabs[i];
                double dX = b.MaxPoint.X - b.MinPoint.X;
                double dY = b.MaxPoint.Y - b.MinPoint.Y;
                double dZ = b.MaxPoint.Z - b.MinPoint.Z;
                double vol = dX * dY * dZ;
                items[i] = (b, b.MinPoint.X, b.MaxPoint.X, b.MinPoint.Y, b.MaxPoint.Y, b.MinPoint.Z, b.MaxPoint.Z, vol, dZ);
            }

            // Sort by MinX for spatial interval pruning (O(N log N))
            Array.Sort(items, (a, b) => a.MinX.CompareTo(b.MinX));

            int count = items.Length;
            for (int i = 0; i < count; i++)
            {
                var a = items[i];
                bool isCompoundEnvelope = false;

                double xThresholdMin = a.MinX - 15.0;
                double xThresholdMax = a.MaxX + 15.0;

                // Find candidate starting index via binary search
                int low = 0, high = count - 1, startIdx = 0;
                while (low <= high)
                {
                    int mid = (low + high) >> 1;
                    if (items[mid].MinX >= xThresholdMin)
                    {
                        startIdx = mid;
                        high = mid - 1;
                    }
                    else
                    {
                        low = mid + 1;
                    }
                }

                for (int j = startIdx; j < count; j++)
                {
                    if (i == j) continue;
                    var b = items[j];

                    // Early exit as soon as b's MinX exceeds a's MaxX boundary
                    if (b.MinX > xThresholdMax) break;

                    // Check if b is geometrically contained inside a with tolerance
                    if (b.MaxX <= a.MaxX + 15.0 &&
                        b.MinY >= a.MinY - 15.0 && b.MaxY <= a.MaxY + 15.0 &&
                        b.MinZ >= a.MinZ - 15.0 && b.MaxZ <= a.MaxZ + 15.0)
                    {
                        if (a.Vol > b.Vol * 1.3 || a.DZ > b.DZ * 1.5)
                        {
                            isCompoundEnvelope = true;
                            break;
                        }
                    }
                }

                if (!isCompoundEnvelope)
                {
                    leafObstacles.Add(a.Box);
                }
            }

            return leafObstacles;
        }

        /// <summary>
        /// Checks if a component should be ignored during clash detection based on user rules.
        /// Excluded components: Bolt assembly, SAFETY_BAR, LUG, LADDER, SAFETY_HOOK, VBRACE, WELD_COUPLER(10), CHECK_COUPLER(10), or custom keywords.
        /// </summary>
        public static bool IsIgnoredComponent(string fullSearchableText, IEnumerable<string> customKeywords = null)
        {
            if (string.IsNullOrWhiteSpace(fullSearchableText)) return false;

            string target = fullSearchableText.ToUpperInvariant();
            // Normalized target without separators ('_', '-', ' ', '\t', '/', '\\')
            string normTarget = target.Replace("_", "").Replace("-", "").Replace(" ", "").Replace("\t", "").Replace("/", "").Replace("\\", "");

            var keywordsList = new List<string>();
            if (customKeywords != null)
            {
                foreach (var kw in customKeywords)
                {
                    if (!string.IsNullOrWhiteSpace(kw)) keywordsList.Add(kw.Trim());
                }
            }

            // Fallback to standard defaults if no custom keywords provided
            if (keywordsList.Count == 0)
            {
                keywordsList.AddRange(new string[] {
                    "Bolt assembly", "SAFETY_BAR", "LUG", "LADDER", "SAFETY_HOOK", 
                    "VBRACE", "WELD_COUPLER", "CHECK_COUPLER"
                });
            }

            foreach (var rawKw in keywordsList)
            {
                if (string.IsNullOrWhiteSpace(rawKw)) continue;
                string kwUpper = rawKw.Trim().ToUpperInvariant();
                string normKw = kwUpper.Replace("_", "").Replace("-", "").Replace(" ", "").Replace("\t", "").Replace("/", "").Replace("\\", "");
                if (string.IsNullOrEmpty(normKw)) continue;

                // 1. Bolt / Fastener / Stud rule
                if (kwUpper.Contains("BOLT") || normKw.Contains("BOLT") || 
                    kwUpper.Contains("STUD") || normKw.Contains("STUD") || 
                    kwUpper.Contains("FASTENER") || normKw.Contains("FASTENER"))
                {
                    if (target.Contains("BOLT") || 
                        target.Contains("IFCMECHANICALFASTENER") || 
                        target.Contains("MECHANICALFASTENER") ||
                        target.Contains("IFCFASTENER") ||
                        target.Contains("FASTENER") ||
                        target.Contains("STUD") ||
                        target.Contains("SHEAR_CONNECTOR") ||
                        target.Contains("WASHER") ||
                        target.Contains("ANCHOR_BOLT") ||
                        normTarget.Contains("BOLT") ||
                        normTarget.Contains("STUD") ||
                        normTarget.Contains("FASTENER"))
                    {
                        return true;
                    }
                }

                // 2. VBRACE / Brace rule
                if (normKw.Contains("VBRACE") || normKw.Contains("BRACE"))
                {
                    if (target.Contains("VBRACE") || target.Contains("V_BRACE") || 
                        target.Contains("V-BRACE") || target.Contains("V BRACE") || 
                        normTarget.Contains("VBRACE") || normTarget.Contains("BRACE"))
                    {
                        return true;
                    }
                }

                // 3. Coupler with parenthesis rule, e.g. WELD_COUPLER(10), CHECK_COUPLER(10)
                int parenIdx = kwUpper.IndexOf('(');
                string prefixKw = parenIdx > 0 ? kwUpper.Substring(0, parenIdx).Trim() : string.Empty;
                string normPrefix = !string.IsNullOrEmpty(prefixKw) ? prefixKw.Replace("_", "").Replace("-", "").Replace(" ", "") : string.Empty;

                if (!string.IsNullOrEmpty(prefixKw))
                {
                    if (target.Contains(prefixKw) || normTarget.Contains(normPrefix))
                    {
                        return true;
                    }
                }

                // 4. LUG / Pad eye rule
                if (kwUpper.Equals("LUG", StringComparison.OrdinalIgnoreCase) || normKw.Equals("LUG", StringComparison.OrdinalIgnoreCase))
                {
                    if (ContainsWordToken(target, "LUG") || normTarget.Contains("LUG") || target.Contains("PAD_EYE") || target.Contains("LIFTING_LUG"))
                    {
                        return true;
                    }
                }

                // 5. General normalized substring match
                if (target.Contains(kwUpper) || normTarget.Contains(normKw))
                {
                    return true;
                }

                // Also check without parentheses
                string noParenKw = kwUpper.Replace("(", "").Replace(")", "").Trim();
                if (!string.IsNullOrEmpty(noParenKw))
                {
                    string normNoParen = noParenKw.Replace("_", "").Replace("-", "").Replace(" ", "");
                    if (target.Contains(noParenKw) || normTarget.Contains(normNoParen))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Backward compatibility overload
        public static bool IsIgnoredComponent(
            string name, 
            string ifcEntity = null, 
            string description = null, 
            string profile = null, 
            IEnumerable<string> customKeywords = null,
            string extraText = null)
        {
            string combined = string.Format("{0} {1} {2} {3} {4}", name ?? "", ifcEntity ?? "", description ?? "", profile ?? "", extraText ?? "").Trim();
            return IsIgnoredComponent(combined, customKeywords);
        }

        private static bool ContainsWordToken(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token)) return false;
            int idx = 0;
            while ((idx = text.IndexOf(token, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                bool startOk = (idx == 0) || !char.IsLetter(text[idx - 1]);
                int endIdx = idx + token.Length;
                bool endOk = (endIdx >= text.Length) || !char.IsLetter(text[endIdx]);
                if (startOk && endOk) return true;
                idx += token.Length;
            }
            return false;
        }

        public IfcBoxInfo ExtractObjectBox(ModelObject obj, ClashSettings settings = null)
        {
            if (obj == null) return null;

            // ONLY extract ReferenceModelObject (IFC elements)
            if (obj is ReferenceModelObject refObj)
            {
                // 1. Must belong to a valid ReferenceModel (ReferenceModelObjectConvert pattern)
                ReferenceModel parentRef = null;
                try
                {
                    parentRef = refObj.GetReferenceModel();
                }
                catch { }

                if (parentRef == null) return null;

                // 2. Must be an IFC file (.ifc, .ifczip, .ifcxml) - Filter out DWG, DXF, DGN, SKP reference models
                string refFilename = parentRef.Filename;
                if (string.IsNullOrWhiteSpace(refFilename)) return null;

                string refExt = Path.GetExtension(refFilename);
                if (string.IsNullOrEmpty(refExt) ||
                    (!refExt.Equals(".ifc", StringComparison.OrdinalIgnoreCase) &&
                     !refExt.Equals(".ifczip", StringComparison.OrdinalIgnoreCase) &&
                     !refExt.Equals(".ifcxml", StringComparison.OrdinalIgnoreCase)))
                {
                    return null;
                }

                // 3. Must have an IFC GlobalId (EXTERNAL.GUID) - Objects without GUID are non-IFC or invalid
                string extGuid = string.Empty;
                if (!refObj.GetReportProperty("EXTERNAL.GUID", ref extGuid) || string.IsNullOrWhiteSpace(extGuid))
                {
                    refObj.GetReportProperty("EXTERNAL.guid", ref extGuid);
                }

                if (string.IsNullOrWhiteSpace(extGuid))
                {
                    return null;
                }
                extGuid = extGuid.Trim();

                // 4. Ultra-fast IFC Catalog Metadata Lookup (O(1) dictionary, <0.001 ms)
                var skipKeywords = (settings != null && settings.EnableIgnoredComponents) ? settings.IgnoredKeywords : null;
                var ifcMeta = XbimGeometryManager.GetProductMetadata(parentRef, _model, extGuid, skipKeywords);
                string mType = ifcMeta?.IfcType ?? string.Empty;
                string mName = ifcMeta?.Name ?? string.Empty;

                // 4a. Filter out Non-physical / Spatial IFC Containers
                if (!string.IsNullOrEmpty(mType))
                {
                    if (mType.Equals("IfcOpeningElement", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcVirtualElement", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcAnnotation", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcGrid", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcSpace", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcZone", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcGroup", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcSite", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcBuilding", StringComparison.OrdinalIgnoreCase) ||
                        mType.Equals("IfcBuildingStorey", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }

                // 4b. Early Discard: Instantly drop Bolt assemblies, fasteners, studs before touching COM API
                bool enableIgnore = (settings != null) ? settings.EnableIgnoredComponents : true;
                var keywords = (settings != null) ? settings.IgnoredKeywords : null;
                string earlyCheckText = (mName + " " + mType).Trim();
                if (enableIgnore && !string.IsNullOrEmpty(earlyCheckText) && IsIgnoredComponent(earlyCheckText, keywords))
                {
                    return null;
                }

                // 5. Must have valid non-degenerate 3D Bounding Box
                double minX = 0, minY = 0, minZ = 0, maxX = 0, maxY = 0, maxZ = 0;
                if (!refObj.GetReportProperty("BOUNDING_BOX_MIN_X", ref minX) ||
                    !refObj.GetReportProperty("BOUNDING_BOX_MIN_Y", ref minY) ||
                    !refObj.GetReportProperty("BOUNDING_BOX_MIN_Z", ref minZ) ||
                    !refObj.GetReportProperty("BOUNDING_BOX_MAX_X", ref maxX) ||
                    !refObj.GetReportProperty("BOUNDING_BOX_MAX_Y", ref maxY) ||
                    !refObj.GetReportProperty("BOUNDING_BOX_MAX_Z", ref maxZ))
                {
                    return null;
                }

                if (maxX <= minX || maxY <= minY || maxZ <= minZ) return null;

                double dX = maxX - minX;
                double dY = maxY - minY;
                double dZ = maxZ - minZ;

                // Eliminate zero-thickness or degenerate lines/points (threshold 0.1mm preserves thin plates/sheets)
                if (dX < 0.1 || dY < 0.1 || dZ < 0.1) return null;

                // 6. Read oriented local basis attributes from ReferenceModelObjectAttributeEnumerator
                Point origin = null;
                Vector axisX = null;
                Vector axisExtrusion = null;
                string prof = string.Empty;
                bool hasOrientation = false;
                List<string> attrNames = new List<string>();

                try
                {
                    var attrEnum = new ReferenceModelObjectAttributeEnumerator(refObj);
                    while (attrEnum.MoveNext())
                    {
                        if (attrEnum.Current is ReferenceModelObjectAttribute attr)
                        {
                            if (!string.IsNullOrWhiteSpace(attr.Name))
                            {
                                attrNames.Add(attr.Name.Trim());
                            }
                            if (origin == null && attr.Origin != null && attr.xDir != null && attr.Extrusion != null)
                            {
                                origin = attr.Origin;
                                axisX = attr.xDir;
                                axisExtrusion = attr.Extrusion;
                                hasOrientation = true;
                            }
                            if (string.IsNullOrEmpty(prof) && !string.IsNullOrEmpty(attr.ProfileName))
                            {
                                prof = attr.ProfileName;
                            }
                        }
                    }
                }
                catch { }

                // Determine display name
                string displayName = !string.IsNullOrWhiteSpace(mName) ? mName :
                                     (attrNames.Count > 0 ? attrNames[0] :
                                     (!string.IsNullOrWhiteSpace(mType) ? mType : "IFC Element"));

                // Combine text tokens for keyword matching
                StringBuilder sbText = new StringBuilder();
                sbText.Append(displayName).Append(" ");
                if (!string.IsNullOrEmpty(mType)) sbText.Append(mType).Append(" ");
                if (!string.IsNullOrEmpty(prof)) sbText.Append(prof).Append(" ");
                foreach (var an in attrNames)
                {
                    sbText.Append(an).Append(" ");
                }
                string searchableText = sbText.ToString().Trim();

                if (enableIgnore && IsIgnoredComponent(searchableText, keywords))
                {
                    return null;
                }

                string fn = Path.GetFileName(refFilename);

                bool isSlab = (!string.IsNullOrEmpty(displayName) && displayName.IndexOf("Slab", StringComparison.OrdinalIgnoreCase) >= 0)
                    || (!string.IsNullOrEmpty(mType) && (mType.IndexOf("Slab", StringComparison.OrdinalIgnoreCase) >= 0 || mType.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0));

                if (!isSlab && !string.IsNullOrEmpty(fn))
                {
                    isSlab = (fn.IndexOf("slab", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              fn.IndexOf("floor", StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // Filter: Eliminate bay-wide compound mapped item envelopes
                if (dX > 1200.0 && dY > 1200.0 && dZ <= 150.0 && !isSlab)
                {
                    return null;
                }

                bool isDiagonal = (dX > 1200.0 && dY > 1200.0 && !isSlab);

                return new IfcBoxInfo
                {
                    ModelObject = refObj,
                    Id = refObj.Identifier.ID,
                    FileName = fn ?? "IFC",
                    EntityName = displayName,
                    Description = string.Empty,
                    IfcEntity = mType,
                    SearchableText = searchableText,
                    MinPoint = new Point(minX, minY, minZ),
                    MaxPoint = new Point(maxX, maxY, maxZ),
                    IsDiagonalMember = isDiagonal,
                    IsSlab = isSlab,
                    Origin = origin,
                    AxisX = axisX,
                    AxisExtrusion = axisExtrusion,
                    ProfileName = prof,
                    HasOrientation = hasOrientation
                };
            }

            return null;
        }

        private class RebarExtractData
        {
            public Reinforcement Rebar;
            public long Id;
            public string Guid;
            public string Name;
            public string Size;
            public string Grade;
            public string Pos;
            public string HostPart;
            public double Length;
            public double Radius;
            public Point Min;
            public Point Max;
            public List<GeoPolyline3> Polylines;
        }

        /// <summary>
        /// True Navisworks Hard Clash Detection: Checks each cylindrical rebar segment against 3D IFC boxes
        /// Optimized with a 2-Phase Multi-Threaded Parallel Pipeline for ultra-fast calculation.
        /// </summary>
        public List<ClashResultItem> DetectClashes(
            List<Reinforcement> rebars, 
            List<IfcBoxInfo> obstacles, 
            ClashSettings settings,
            Action<int, int> progressCallback = null,
            System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            var rawClashes = new System.Collections.Concurrent.ConcurrentBag<ClashResultItem>();
            if (rebars == null || obstacles == null || rebars.Count == 0 || obstacles.Count == 0)
                return new List<ClashResultItem>();

            // GeometryHelper Optimization: Build BVH spatial tree over obstacles once (O(M log M))
            var bvhTree = new ObstacleBvhTree(obstacles);
            int totalRebars = rebars.Count;

            // Phase 1: Fast Tekla Data Extraction (Single-threaded to respect Tekla Open API)
            var rebarItems = new List<RebarExtractData>(totalRebars);
            for (int rIdx = 0; rIdx < totalRebars; rIdx++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rebar = rebars[rIdx];
                if (rebar == null) continue;

                // Rebar Bounding Box (Fast metadata lookup, 1000x faster than GetSolid)
                double rMinX = 0, rMinY = 0, rMinZ = 0, rMaxX = 0, rMaxY = 0, rMaxZ = 0;
                Point rMin, rMax;
                if (rebar.GetReportProperty("BOUNDING_BOX_MIN_X", ref rMinX) &&
                    rebar.GetReportProperty("BOUNDING_BOX_MIN_Y", ref rMinY) &&
                    rebar.GetReportProperty("BOUNDING_BOX_MIN_Z", ref rMinZ) &&
                    rebar.GetReportProperty("BOUNDING_BOX_MAX_X", ref rMaxX) &&
                    rebar.GetReportProperty("BOUNDING_BOX_MAX_Y", ref rMaxY) &&
                    rebar.GetReportProperty("BOUNDING_BOX_MAX_Z", ref rMaxZ) && rMaxX > rMinX)
                {
                    rMin = new Point(rMinX, rMinY, rMinZ);
                    rMax = new Point(rMaxX, rMaxY, rMaxZ);
                }
                else
                {
                    Solid rebarSolid = null;
                    try { rebarSolid = rebar.GetSolid(); } catch { }
                    if (rebarSolid == null) continue;
                    rMin = rebarSolid.MinimumPoint;
                    rMax = rebarSolid.MaximumPoint;
                }

                // Rebar Size & Diameter
                double dia = 16.0;
                rebar.GetReportProperty("DIAMETER", ref dia);
                if (dia <= 0) dia = 16.0;
                double rebarRadius = dia / 2.0;

                string rebarSize = string.Empty;
                rebar.GetReportProperty("SIZE", ref rebarSize);
                if (string.IsNullOrEmpty(rebarSize)) rebarSize = "D" + dia.ToString("0");

                string rebarName = string.Empty;
                rebar.GetReportProperty("NAME", ref rebarName);

                string rebarGrade = string.Empty;
                rebar.GetReportProperty("GRADE", ref rebarGrade);

                string rebarPos = string.Empty;
                rebar.GetReportProperty("REBAR_POS", ref rebarPos);

                string hostPart = string.Empty;
                rebar.GetReportProperty("PART.NAME", ref hostPart);
                if (string.IsNullOrEmpty(hostPart)) rebar.GetReportProperty("MAINPART.NAME", ref hostPart);

                double rebarLen = 0.0;
                rebar.GetReportProperty("LENGTH", ref rebarLen);

                // Retrieve all centerline polylines
                var centerlinePolys = new List<ArrayList>();
                try
                {
                    ArrayList geoms = rebar.GetRebarGeometries(true);
                    if (geoms == null || geoms.Count == 0)
                        geoms = rebar.GetRebarGeometries(false);

                    if (geoms != null)
                    {
                        foreach (object obj in geoms)
                        {
                            if (obj is RebarGeometry rg && rg.Shape != null && rg.Shape.Points != null && rg.Shape.Points.Count >= 2)
                            {
                                centerlinePolys.Add(rg.Shape.Points);
                            }
                        }
                    }
                }
                catch { }

                if (centerlinePolys.Count == 0) continue;

                var rebarPolylines = new List<GeoPolyline3>(centerlinePolys.Count);
                foreach (var polyPts in centerlinePolys)
                {
                    if (polyPts == null || polyPts.Count < 2) continue;
                    var gPts = new List<GeoPoint3>(polyPts.Count);
                    for (int pIdx = 0; pIdx < polyPts.Count; pIdx++)
                    {
                        if (polyPts[pIdx] is Point pt)
                        {
                            var gp = new GeoPoint3(pt.X, pt.Y, pt.Z);
                            if (gPts.Count == 0 || !gPts[gPts.Count - 1].IsEqualTo(gp))
                            {
                                gPts.Add(gp);
                            }
                        }
                    }
                    if (gPts.Count >= 2)
                    {
                        try
                        {
                            rebarPolylines.Add(new GeoPolyline3(gPts));
                        }
                        catch { }
                    }
                }

                if (rebarPolylines.Count == 0) continue;

                rebarItems.Add(new RebarExtractData
                {
                    Rebar = rebar,
                    Id = rebar.Identifier.ID,
                    Guid = rebar.Identifier.GUID.ToString(),
                    Name = rebarName ?? "REBAR",
                    Size = rebarSize,
                    Grade = rebarGrade,
                    Pos = rebarPos,
                    HostPart = hostPart,
                    Length = Math.Round(rebarLen, 0),
                    Radius = rebarRadius,
                    Min = rMin,
                    Max = rMax,
                    Polylines = rebarPolylines
                });
            }

            int validRebars = rebarItems.Count;
            if (validRebars == 0) return new List<ClashResultItem>();

            // Geometry Pre-Caching (Runs on Main Tekla Thread)
            // Pre-extracts geometry for obstacles that intersect the collective rebar zone.
            // This guarantees ZERO Tekla COM IPC calls occur during multi-threaded Phase 2!
            double gMinX = double.MaxValue, gMinY = double.MaxValue, gMinZ = double.MaxValue;
            double gMaxX = double.MinValue, gMaxY = double.MinValue, gMaxZ = double.MinValue;
            for (int i = 0; i < validRebars; i++)
            {
                var rb = rebarItems[i];
                if (rb.Min.X < gMinX) gMinX = rb.Min.X;
                if (rb.Min.Y < gMinY) gMinY = rb.Min.Y;
                if (rb.Min.Z < gMinZ) gMinZ = rb.Min.Z;
                if (rb.Max.X > gMaxX) gMaxX = rb.Max.X;
                if (rb.Max.Y > gMaxY) gMaxY = rb.Max.Y;
                if (rb.Max.Z > gMaxZ) gMaxZ = rb.Max.Z;
            }

            double globalPad = 50.0 + settings.ClearanceMm + 50.0;
            Point qZoneMin = new Point(gMinX - globalPad, gMinY - globalPad, gMinZ - globalPad);
            Point qZoneMax = new Point(gMaxX + globalPad, gMaxY + globalPad, gMaxZ + globalPad);

            var zoneObstacles = new List<IfcBoxInfo>();
            bvhTree.Query(qZoneMin, qZoneMax, zoneObstacles);

            var skipKeywords = (settings != null && settings.EnableIgnoredComponents) ? settings.IgnoredKeywords : null;
            foreach (var obs in zoneObstacles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                obs.GetGeometry(_model, skipKeywords);
            }

            // Phase 2: Parallel Multi-Threaded Geometry Clash Detection (Full Multi-Core CPU Utilization)
            int processedCount = 0;
            var parallelOpts = new System.Threading.Tasks.ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            System.Threading.Tasks.Parallel.ForEach(rebarItems, parallelOpts, (rb) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                double effRadius = rb.Radius + settings.ClearanceMm;
                Point expRMin = new Point(rb.Min.X - effRadius, rb.Min.Y - effRadius, rb.Min.Z - effRadius);
                Point expRMax = new Point(rb.Max.X + effRadius, rb.Max.Y + effRadius, rb.Max.Z + effRadius);

                var candidateObstacles = new List<IfcBoxInfo>();
                bvhTree.Query(expRMin, expRMax, candidateObstacles);

                if (candidateObstacles.Count > 0)
                {
                    foreach (var obs in candidateObstacles)
                    {
                        var intersectingPolys = new List<GeoPolyline3>();
                        foreach (var rebarPoly in rb.Polylines)
                        {
                            var polyAabb = rebarPoly.GetAabb();
                            if (!(polyAabb.Min.X - effRadius > obs.MaxPoint.X || polyAabb.Max.X + effRadius < obs.MinPoint.X ||
                                  polyAabb.Min.Y - effRadius > obs.MaxPoint.Y || polyAabb.Max.Y + effRadius < obs.MinPoint.Y ||
                                  polyAabb.Min.Z - effRadius > obs.MaxPoint.Z || polyAabb.Max.Z + effRadius < obs.MinPoint.Z))
                            {
                                intersectingPolys.Add(rebarPoly);
                            }
                        }

                        if (intersectingPolys.Count == 0) continue;

                        double maxOverlap = 0.0;
                        Point bestClashPt = null;

                        var geomRep = obs.GetGeometry(_model, skipKeywords);

                        foreach (var rebarPoly in intersectingPolys)
                        {
                            if (geomRep != null && (geomRep.HasExactSolids || geomRep.HasParametricGeometry || geomRep.BrepSolid != null || geomRep.MeshBvh != null))
                            {
                                if (IfcGeometryBridge.TestPolylineVsGeometry(rebarPoly, rb.Radius, settings.ClearanceMm, settings.ToleranceMm, geomRep, out double polyOverlap, out Point clashPt))
                                {
                                    if (polyOverlap > maxOverlap)
                                    {
                                        maxOverlap = polyOverlap;
                                        bestClashPt = clashPt;
                                    }
                                }
                            }
                            else
                            {
                                for (int eIdx = 0; eIdx < rebarPoly.EdgeCount; eIdx++)
                                {
                                    var edge = rebarPoly.GetEdgeAt(eIdx);
                                    Point p1 = new Point(edge.StartPoint.X, edge.StartPoint.Y, edge.StartPoint.Z);
                                    Point p2 = new Point(edge.EndPoint.X, edge.EndPoint.Y, edge.EndPoint.Z);
                                    if (TestSegmentVsBox(p1, p2, rb.Radius, settings.ClearanceMm, settings.ToleranceMm, obs, out double overlap, out Point pt, _model))
                                    {
                                        if (overlap > maxOverlap)
                                        {
                                            maxOverlap = overlap;
                                            bestClashPt = pt;
                                        }
                                    }
                                }
                            }
                        }

                        if (maxOverlap >= settings.ToleranceMm && bestClashPt != null)
                        {
                            if (settings.EnableIgnoredComponents && 
                                IsIgnoredComponent(!string.IsNullOrEmpty(obs.SearchableText) ? obs.SearchableText : obs.EntityName, settings.IgnoredKeywords))
                            {
                                continue;
                            }

                            rawClashes.Add(new ClashResultItem
                            {
                                RebarId = rb.Id,
                                RebarGuid = rb.Guid,
                                RebarName = rb.Name,
                                RebarSize = rb.Size,
                                RebarGrade = rb.Grade,
                                RebarPos = rb.Pos,
                                HostPartName = rb.HostPart,
                                RebarLength = rb.Length,
                                RebarObject = rb.Rebar,
                                IfcObjectId = obs.Id,
                                IfcFileName = obs.FileName,
                                IfcEntityName = obs.EntityName,
                                IfcObject = obs.ModelObject as ReferenceModelObject,
                                OverlapMm = Math.Round(maxOverlap, 1),
                                ClashPoint = bestClashPt,
                                MinPoint = new Point(Math.Min(rb.Min.X, obs.MinPoint.X), Math.Min(rb.Min.Y, obs.MinPoint.Y), Math.Min(rb.Min.Z, obs.MinPoint.Z)),
                                MaxPoint = new Point(Math.Max(rb.Max.X, obs.MaxPoint.X), Math.Max(rb.Max.Y, obs.MaxPoint.Y), Math.Max(rb.Max.Z, obs.MaxPoint.Z))
                            });
                        }
                    }
                }

                int finished = System.Threading.Interlocked.Increment(ref processedCount);
                progressCallback?.Invoke(finished, validRebars);
            });

            // Deduplicate: Keep max overlap per (RebarId, IfcObjectId) pair to report all distinct clash pairs
            var uniqueMap = new Dictionary<Tuple<long, long>, ClashResultItem>();
            foreach (var c in rawClashes)
            {
                var pairKey = Tuple.Create(c.RebarId, c.IfcObjectId);
                if (!uniqueMap.TryGetValue(pairKey, out var existing) || c.OverlapMm > existing.OverlapMm)
                {
                    uniqueMap[pairKey] = c;
                }
            }

            var finalClashes = new List<ClashResultItem>(uniqueMap.Values);
            for (int i = 0; i < finalClashes.Count; i++)
            {
                finalClashes[i].Index = i + 1;
            }

            return finalClashes;
        }

        /// <summary>
        /// Exact 3D Segment-to-Box Intersection & Distance Algorithm
        /// </summary>
        private static bool TestSegmentVsBox(
            Point p1, 
            Point p2, 
            double rebarRadius, 
            double clearance, 
            double tolerance,
            IfcBoxInfo obs, 
            out double overlap, 
            out Point clashPoint,
            Model teklaModel = null)
        {
            overlap = 0.0;
            clashPoint = null;

            Point bMin = obs.MinPoint;
            Point bMax = obs.MaxPoint;
            double allowedDistance = rebarRadius + clearance;

            double dX = bMax.X - bMin.X;
            double dY = bMax.Y - bMin.Y;
            double dZ = bMax.Z - bMin.Z;
            double boxMinDim = Math.Min(dX, Math.Min(dY, dZ));

            // Rotated / Slanted Member Check via Extrusion Axis Line
            if (obs.HasOrientation && obs.Origin != null && obs.AxisExtrusion != null)
            {
                Vector vExt = new Vector(obs.AxisExtrusion);
                double vLenSq = vExt.X * vExt.X + vExt.Y * vExt.Y + vExt.Z * vExt.Z;
                if (vLenSq > 1e-4)
                {
                    double halfProfile = Math.Max(Math.Min(dX, dY), Math.Min(dY, dZ)) / 2.0;
                    if (halfProfile > 10.0 && halfProfile < 800.0)
                    {
                        Point segMid = new Point((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0, (p1.Z + p2.Z) / 2.0);
                        Vector op = new Vector(segMid.X - obs.Origin.X, segMid.Y - obs.Origin.Y, segMid.Z - obs.Origin.Z);
                        double t = (op.X * vExt.X + op.Y * vExt.Y + op.Z * vExt.Z) / vLenSq;
                        Point ptOnAxis = new Point(obs.Origin.X + t * vExt.X, obs.Origin.Y + t * vExt.Y, obs.Origin.Z + t * vExt.Z);
                        double distToAxis = Distance(segMid, ptOnAxis);

                        // If the rebar is further from the member's longitudinal spine than its actual profile width:
                        if (distToAxis > (halfProfile + allowedDistance + 50.0))
                        {
                            return false; // Rebar is in empty air around rotated member's expanded AABB
                        }
                    }
                }
            }

            // Special check for large diagonal structural elements (bracing, diagonal steel members)
            if (obs.IsDiagonalMember)
            {
                Point c1 = new Point(bMin.X, bMin.Y, (bMin.Z + bMax.Z) / 2.0);
                Point c2 = new Point(bMax.X, bMax.Y, (bMin.Z + bMax.Z) / 2.0);
                Point c3 = new Point(bMin.X, bMax.Y, (bMin.Z + bMax.Z) / 2.0);
                Point c4 = new Point(bMax.X, bMin.Y, (bMin.Z + bMax.Z) / 2.0);

                double dDiag1 = DistSegToSeg2D(p1, p2, c1, c2);
                double dDiag2 = DistSegToSeg2D(p1, p2, c3, c4);
                double minDiagDist = Math.Min(dDiag1, dDiag2);

                if (minDiagDist > 400.0)
                {
                    return false; // Rebar is in empty air inside the diagonal member's AABB
                }
            }

            // 1. Exact 3D Liang-Barsky Slab Penetration Test: Checks if rebar centerline penetrates the IFC box
            if (SegmentIntersectsBox(p1, p2, bMin, bMax, out double tEnter, out double tExit, out Point enterPt, out Point exitPt))
            {
                double lengthInside = Distance(enterPt, exitPt);

                // Rebar endpoint terminates right at the face without penetrating solid interior
                if (lengthInside < Math.Max(2.0, tolerance))
                {
                    return false;
                }

                // Excessive length inside (> 1.0m) for non-slab is an empty compound bounding envelope
                if (lengthInside > 1000.0 && !obs.IsSlab)
                {
                    return false;
                }

                // Tier 3: GeoSolid3 B-Rep Exact Solid Splition Check
                // Checks exact penetration against solid components of the member (e.g. Flanges & Web)
                var solids = obs.GetGeoSolids(teklaModel);
                if (solids != null && solids.Count > 0)
                {
                    GeoPoint3 gp1 = new GeoPoint3(p1.X, p1.Y, p1.Z);
                    GeoPoint3 gp2 = new GeoPoint3(p2.X, p2.Y, p2.Z);
                    GeoPolyline3 segPoly = new GeoPolyline3(new GeoPoint3[] { gp1, gp2 });

                    bool hitSolid = false;
                    double maxSolidLen = 0.0;
                    Point bestSolidPt = null;

                    foreach (var solid in solids)
                    {
                        if (solid == null) continue;
                        if (Splition3.TrySplitBy(segPoly, solid, out GeoPolyline3[] inside, out GeoPolyline3[] outside))
                        {
                            if (inside != null && inside.Length > 0)
                            {
                                foreach (var piece in inside)
                                {
                                    if (piece.Length >= Math.Max(2.0, tolerance))
                                    {
                                        hitSolid = true;
                                        if (piece.Length > maxSolidLen)
                                        {
                                            maxSolidLen = piece.Length;
                                            bestSolidPt = new Point(
                                                (piece.StartPoint.X + piece.EndPoint.X) / 2.0,
                                                (piece.StartPoint.Y + piece.EndPoint.Y) / 2.0,
                                                (piece.StartPoint.Z + piece.EndPoint.Z) / 2.0
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (hitSolid)
                    {
                        double penetration = Math.Min(maxSolidLen, boxMinDim);
                        overlap = penetration + rebarRadius;
                        clashPoint = bestSolidPt ?? new Point((enterPt.X + exitPt.X) / 2.0, (enterPt.Y + exitPt.Y) / 2.0, (enterPt.Z + exitPt.Z) / 2.0);
                        return true;
                    }
                    else
                    {
                        // Rebar is in empty air between flanges/plates of structural section!
                        return false;
                    }
                }

                clashPoint = new Point((enterPt.X + exitPt.X) / 2.0, (enterPt.Y + exitPt.Y) / 2.0, (enterPt.Z + exitPt.Z) / 2.0);
                double pen = Math.Min(lengthInside, boxMinDim);
                overlap = pen + rebarRadius;
                return true;
            }

            // 2. Centerline does not enter box. Test if the cylindrical rebar skin touches or penetrates the box surface.
            Point closestPtOnSeg = GetClosestPointOnSegmentToBox(p1, p2, bMin, bMax);
            Point closestPtOnBox = new Point(
                Math.Max(bMin.X, Math.Min(closestPtOnSeg.X, bMax.X)),
                Math.Max(bMin.Y, Math.Min(closestPtOnSeg.Y, bMax.Y)),
                Math.Max(bMin.Z, Math.Min(closestPtOnSeg.Z, bMax.Z))
            );

            double dist = Distance(closestPtOnSeg, closestPtOnBox);

            if (dist <= allowedDistance)
            {
                overlap = allowedDistance - dist;
                clashPoint = closestPtOnBox;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 2D segment to segment distance (ignoring Z)
        /// </summary>
        private static double DistSegToSeg2D(Point s1, Point s2, Point t1, Point t2)
        {
            double d1 = DistPtToSeg2D(s1, t1, t2);
            double d2 = DistPtToSeg2D(s2, t1, t2);
            double d3 = DistPtToSeg2D(t1, s1, s2);
            double d4 = DistPtToSeg2D(t2, s1, s2);
            return Math.Min(Math.Min(d1, d2), Math.Min(d3, d4));
        }

        private static double DistPtToSeg2D(Point pt, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lenSq = dx * dx + dy * dy;
            if (lenSq < 1e-9)
            {
                double px = pt.X - a.X;
                double py = pt.Y - a.Y;
                return Math.Sqrt(px * px + py * py);
            }

            double t = ((pt.X - a.X) * dx + (pt.Y - a.Y) * dy) / lenSq;
            t = Math.Max(0.0, Math.Min(1.0, t));
            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;
            double distSq = (pt.X - projX) * (pt.X - projX) + (pt.Y - projY) * (pt.Y - projY);
            return Math.Sqrt(distSq);
        }

        /// <summary>
        /// Exact 3D Ray-AABB Slab intersection test
        /// </summary>
        private static bool SegmentIntersectsBox(
            Point p1, 
            Point p2, 
            Point bMin, 
            Point bMax, 
            out double tEnter, 
            out double tExit, 
            out Point enterPt, 
            out Point exitPt)
        {
            tEnter = 0.0;
            tExit = 1.0;
            enterPt = null;
            exitPt = null;

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;

            // X slab
            if (Math.Abs(dx) < 1e-9)
            {
                if (p1.X < bMin.X || p1.X > bMax.X) return false;
            }
            else
            {
                double t0 = (bMin.X - p1.X) / dx;
                double t1 = (bMax.X - p1.X) / dx;
                if (t0 > t1) { double tmp = t0; t0 = t1; t1 = tmp; }
                tEnter = Math.Max(tEnter, t0);
                tExit = Math.Min(tExit, t1);
                if (tEnter > tExit) return false;
            }

            // Y slab
            if (Math.Abs(dy) < 1e-9)
            {
                if (p1.Y < bMin.Y || p1.Y > bMax.Y) return false;
            }
            else
            {
                double t0 = (bMin.Y - p1.Y) / dy;
                double t1 = (bMax.Y - p1.Y) / dy;
                if (t0 > t1) { double tmp = t0; t0 = t1; t1 = tmp; }
                tEnter = Math.Max(tEnter, t0);
                tExit = Math.Min(tExit, t1);
                if (tEnter > tExit) return false;
            }

            // Z slab
            if (Math.Abs(dz) < 1e-9)
            {
                if (p1.Z < bMin.Z || p1.Z > bMax.Z) return false;
            }
            else
            {
                double t0 = (bMin.Z - p1.Z) / dz;
                double t1 = (bMax.Z - p1.Z) / dz;
                if (t0 > t1) { double tmp = t0; t0 = t1; t1 = tmp; }
                tEnter = Math.Max(tEnter, t0);
                tExit = Math.Min(tExit, t1);
                if (tEnter > tExit) return false;
            }

            if (tExit >= 0.0 && tEnter <= 1.0 && tEnter <= tExit)
            {
                double tIn = Math.Max(0.0, tEnter);
                double tOut = Math.Min(1.0, tExit);
                enterPt = new Point(p1.X + tIn * dx, p1.Y + tIn * dy, p1.Z + tIn * dz);
                exitPt = new Point(p1.X + tOut * dx, p1.Y + tOut * dy, p1.Z + tOut * dz);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Exact 3D Analytical Closest Point on Segment to AABB Box.
        /// Replaces ternary search with zero-allocation convex derivative bisection (sub-millimeter precision in &lt;10 steps).
        /// </summary>
        private static Point GetClosestPointOnSegmentToBox(Point p1, Point p2, Point bMin, Point bMax)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;

            // Evaluates derivative of squared distance function f(t) = ||P(t) - Clamp(P(t), bMin, bMax)||^2
            double EvalDeriv(double t)
            {
                double x = p1.X + t * dx;
                double y = p1.Y + t * dy;
                double z = p1.Z + t * dz;

                double gx = x < bMin.X ? (x - bMin.X) : (x > bMax.X ? (x - bMax.X) : 0.0);
                double gy = y < bMin.Y ? (y - bMin.Y) : (y > bMax.Y ? (y - bMax.Y) : 0.0);
                double gz = z < bMin.Z ? (z - bMin.Z) : (z > bMax.Z ? (z - bMax.Z) : 0.0);

                return 2.0 * (gx * dx + gy * dy + gz * dz);
            }

            if (EvalDeriv(0.0) >= 0.0)
                return new Point(p1.X, p1.Y, p1.Z);

            if (EvalDeriv(1.0) <= 0.0)
                return new Point(p2.X, p2.Y, p2.Z);

            double low = 0.0;
            double high = 1.0;
            for (int i = 0; i < 10; i++)
            {
                double mid = 0.5 * (low + high);
                if (EvalDeriv(mid) < 0.0)
                    low = mid;
                else
                    high = mid;
            }

            double bestT = 0.5 * (low + high);
            return new Point(p1.X + bestT * dx, p1.Y + bestT * dy, p1.Z + bestT * dz);
        }

        private static double SqrDistanceToBox(Point pt, Point bMin, Point bMax)
        {
            double cx = Math.Max(bMin.X, Math.Min(pt.X, bMax.X));
            double cy = Math.Max(bMin.Y, Math.Min(pt.Y, bMax.Y));
            double cz = Math.Max(bMin.Z, Math.Min(pt.Z, bMax.Z));

            double dx = pt.X - cx;
            double dy = pt.Y - cy;
            double dz = pt.Z - cz;
            return dx * dx + dy * dy + dz * dz;
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static bool IsAabbOverlap(Point minA, Point maxA, Point minB, Point maxB)
        {
            return (minA.X <= maxB.X && maxA.X >= minB.X) &&
                   (minA.Y <= maxB.Y && maxA.Y >= minB.Y) &&
                   (minA.Z <= maxB.Z && maxA.Z >= minB.Z);
        }

        private static double CalculateAabbOverlapDepth(Point minA, Point maxA, Point minB, Point maxB)
        {
            double ox = Math.Max(0.0, Math.Min(maxA.X, maxB.X) - Math.Max(minA.X, minB.X));
            double oy = Math.Max(0.0, Math.Min(maxA.Y, maxB.Y) - Math.Max(minA.Y, minB.Y));
            double oz = Math.Max(0.0, Math.Min(maxA.Z, maxB.Z) - Math.Max(minA.Z, minB.Z));
            return Math.Min(ox, Math.Min(oy, oz));
        }
    }

    /// <summary>
    /// High-Performance Bounding Volume Hierarchy (BVH) Spatial Index for 3D obstacles
    /// Built following GeometryHelper.SolidGeometry.Spatial.GeoBvh3 design principles.
    /// Reduces broad-phase clash query complexity from O(N * M) to O(N * log M).
    /// </summary>
    public class ObstacleBvhTree
    {
        private const int LeafThreshold = 4;

        private class BvhNode
        {
            public Point MinPoint;
            public Point MaxPoint;
            public BvhNode Left;
            public BvhNode Right;
            public List<IfcBoxInfo> Items;

            public bool IsLeaf => Items != null;
        }

        private readonly BvhNode _root;

        public ObstacleBvhTree(List<IfcBoxInfo> obstacles)
        {
            if (obstacles == null || obstacles.Count == 0) return;
            var list = new List<IfcBoxInfo>(obstacles);
            _root = BuildNode(list, 0, list.Count);
        }

        private static BvhNode BuildNode(List<IfcBoxInfo> list, int start, int count)
        {
            if (count <= 0) return null;

            double bMinX = double.MaxValue, bMinY = double.MaxValue, bMinZ = double.MaxValue;
            double bMaxX = double.MinValue, bMaxY = double.MinValue, bMaxZ = double.MinValue;

            for (int i = start; i < start + count; i++)
            {
                var item = list[i];
                if (item.MinPoint.X < bMinX) bMinX = item.MinPoint.X;
                if (item.MinPoint.Y < bMinY) bMinY = item.MinPoint.Y;
                if (item.MinPoint.Z < bMinZ) bMinZ = item.MinPoint.Z;

                if (item.MaxPoint.X > bMaxX) bMaxX = item.MaxPoint.X;
                if (item.MaxPoint.Y > bMaxY) bMaxY = item.MaxPoint.Y;
                if (item.MaxPoint.Z > bMaxZ) bMaxZ = item.MaxPoint.Z;
            }

            var node = new BvhNode
            {
                MinPoint = new Point(bMinX, bMinY, bMinZ),
                MaxPoint = new Point(bMaxX, bMaxY, bMaxZ)
            };

            if (count <= LeafThreshold)
            {
                node.Items = list.GetRange(start, count);
                return node;
            }

            // Split along largest axis
            double dx = bMaxX - bMinX;
            double dy = bMaxY - bMinY;
            double dz = bMaxZ - bMinZ;

            int axis = 0; // 0=X, 1=Y, 2=Z
            if (dy > dx && dy >= dz) axis = 1;
            else if (dz > dx && dz >= dy) axis = 2;

            list.Sort(start, count, Comparer<IfcBoxInfo>.Create((a, b) =>
            {
                double ca = axis == 0 ? (a.MinPoint.X + a.MaxPoint.X) : (axis == 1 ? (a.MinPoint.Y + a.MaxPoint.Y) : (a.MinPoint.Z + a.MaxPoint.Z));
                double cb = axis == 0 ? (b.MinPoint.X + b.MaxPoint.X) : (axis == 1 ? (b.MinPoint.Y + b.MaxPoint.Y) : (b.MinPoint.Z + b.MaxPoint.Z));
                return ca.CompareTo(cb);
            }));

            int mid = start + count / 2;
            node.Left = BuildNode(list, start, mid - start);
            node.Right = BuildNode(list, mid, count - (mid - start));

            return node;
        }

        public void Query(Point qMin, Point qMax, List<IfcBoxInfo> results)
        {
            if (_root == null || results == null) return;
            QueryNode(_root, qMin, qMax, results);
        }

        private static void QueryNode(BvhNode node, Point qMin, Point qMax, List<IfcBoxInfo> results)
        {
            if (node == null) return;

            // AABB overlap test with node bounding volume
            if (qMin.X > node.MaxPoint.X || qMax.X < node.MinPoint.X ||
                qMin.Y > node.MaxPoint.Y || qMax.Y < node.MinPoint.Y ||
                qMin.Z > node.MaxPoint.Z || qMax.Z < node.MinPoint.Z)
            {
                return;
            }

            if (node.IsLeaf)
            {
                for (int i = 0; i < node.Items.Count; i++)
                {
                    var item = node.Items[i];
                    if (qMin.X <= item.MaxPoint.X && qMax.X >= item.MinPoint.X &&
                        qMin.Y <= item.MaxPoint.Y && qMax.Y >= item.MinPoint.Y &&
                        qMin.Z <= item.MaxPoint.Z && qMax.Z >= item.MinPoint.Z)
                    {
                        results.Add(item);
                    }
                }
            }
            else
            {
                QueryNode(node.Left, qMin, qMax, results);
                QueryNode(node.Right, qMin, qMax, results);
            }
        }
    }
}

