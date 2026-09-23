using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using GeometryHelper.Geometry;
using GeometryHelper.IfcConvert.Core;
using GeometryHelper.TeklaConvert;

namespace BimCommands.Tekla.ClashCheck
{
    /// <summary>
    /// Chế độ phạm vi quét đối tượng IFC:
    /// - AutoSpatialAllIfc: Tự động lọc không gian tất cả file IFC giao với vùng thép (Phong cách Navisworks).
    /// - SelectedIfcOnly: Chỉ quét các cấu kiện IFC hoặc Part được chọn trực tiếp trên màn hình Tekla.
    /// - SpecificFile: Quét theo một file IFC cụ thể được chọn từ danh sách thả xuống.
    /// </summary>
    public enum IfcScopeMode
    {
        /// <summary>Tự động quét toàn bộ file IFC nằm trong vùng không gian của cốt thép</summary>
        AutoSpatialAllIfc,

        /// <summary>Chỉ quét các đối tượng tham chiếu IFC đang chọn trong Tekla UI</summary>
        SelectedIfcOnly,

        /// <summary>Chỉ quét các đối tượng thuộc một file IFC cụ thể</summary>
        SpecificFile
    }

    /// <summary>
    /// Lớp lưu trữ các thiết lập cấu hình quét va chạm.
    /// </summary>
    public class ClashSettings
    {
        /// <summary>Chỉ quét các thanh thép đang chọn (true) hay toàn bộ thép trong mô hình (false).</summary>
        public bool OnlySelectedRebars { get; set; } = true;

        /// <summary>Chế độ phạm vi quét file IFC.</summary>
        public IfcScopeMode IfcMode { get; set; } = IfcScopeMode.AutoSpatialAllIfc;

        /// <summary>Tên file IFC mục tiêu cần quét (mặc định "ALL" cho tất cả các file).</summary>
        public string TargetIfcFileName { get; set; } = "ALL";

        /// <summary>Danh sách các file IFC mục tiêu cần quét (hỗ trợ chọn đồng thời nhiều file IFC).</summary>
        public HashSet<string> TargetIfcFileNames { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Kiểm tra xem tên file có khớp với danh sách file IFC mục tiêu hay không.</summary>
        public bool MatchesIfcFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            if (TargetIfcFileNames == null || TargetIfcFileNames.Count == 0 || TargetIfcFileNames.Contains("ALL"))
                return true;

            foreach (var target in TargetIfcFileNames)
            {
                if (string.Equals(target, "ALL", StringComparison.OrdinalIgnoreCase)) return true;
                if (fileName.Equals(target, StringComparison.OrdinalIgnoreCase) ||
                    fileName.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    target.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Dung sai độ lấn tối thiểu để ghi nhận va chạm (đơn vị: mm, mặc định 1.0mm).</summary>
        public double ToleranceMm { get; set; } = 1.0;

        /// <summary>Khoảng hở an toàn xung quanh thép cần kiểm tra (Clearance mm, mặc định 0.0mm).</summary>
        public double ClearanceMm { get; set; } = 0.0;

        /// <summary>Bật/tắt bộ lọc tự động loại trừ các cấu kiện phụ IFC (bu lông, thang, tai móc, giằng...).</summary>
        public bool EnableIgnoredComponents { get; set; } = true;

        /// <summary>Danh sách từ khóa tên cấu kiện cần lược bỏ (SkipNames).</summary>
        public List<string> IgnoredKeywords { get; set; } = new List<string>();

        /// <summary>Bật/tắt bộ lọc chỉ quét các cấu kiện chỉ định (OnlyNames).</summary>
        public bool EnableOnlyComponents { get; set; } = false;

        /// <summary>Danh sách từ khóa tên cấu kiện bắt buộc phải quét (OnlyNames).</summary>
        public List<string> OnlyKeywords { get; set; } = new List<string>();
    }

    /// <summary>
    /// Bộ máy phát hiện va chạm độ chính xác cao giữa Cốt thép Tekla và Cấu kiện tham chiếu IFC.
    /// Áp dụng mô hình tinh gọn sử dụng trực tiếp IfcProductGeometry và cây chỉ mục không gian BVH.
    /// </summary>
    public class ClashDetector
    {
        private readonly Model _model;

        /// <summary>
        /// Khởi tạo bộ phát hiện va chạm với mô hình Tekla Structures hiện hành.
        /// </summary>
        /// <param name="model">Đối tượng Model của Tekla Open API.</param>
        public ClashDetector(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Lấy toàn bộ danh sách các mô hình tham chiếu (ReferenceModel) đang được chèn trong Tekla Model.
        /// </summary>
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
                System.Diagnostics.Debug.WriteLine("Lỗi khi lấy danh sách ReferenceModel: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Kiểm tra xem một cấu kiện có thuộc diện bỏ qua trong kiểm tra va chạm hay không (dùng làm bộ lọc an toàn UI).
        /// </summary>
        public static bool IsIgnoredComponent(string fullSearchableText, IEnumerable<string> customKeywords = null)
        {
            if (string.IsNullOrWhiteSpace(fullSearchableText)) return false;

            string target = fullSearchableText.ToUpperInvariant();
            string normTarget = target.Replace("_", "").Replace("-", "").Replace(" ", "").Replace("\t", "").Replace("/", "").Replace("\\", "");

            var keywordsList = new List<string>();
            if (customKeywords != null)
            {
                foreach (var kw in customKeywords)
                {
                    if (!string.IsNullOrWhiteSpace(kw)) keywordsList.Add(kw.Trim());
                }
            }

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

                if (kwUpper.Contains("BOLT") || normKw.Contains("BOLT") || 
                    kwUpper.Contains("STUD") || normKw.Contains("STUD") || 
                    kwUpper.Contains("FASTENER") || normKw.Contains("FASTENER"))
                {
                    if (target.Contains("BOLT") || 
                        target.Contains("IFCMECHANICALFASTENER") || 
                        target.Contains("MECHANICALFASTENER") ||
                        target.Contains("STUD") ||
                        normTarget.Contains("BOLT") || 
                        normTarget.Contains("STUD") ||
                        normTarget.Contains("FASTENER"))
                    {
                        return true;
                    }
                }

                if (target.Contains(kwUpper) || normTarget.Contains(normKw))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra xem một cấu kiện có khớp với danh sách từ khóa chỉ định quét hay không (OnlyNames).
        /// </summary>
        public static bool MatchesOnlyComponent(string fullSearchableText, IEnumerable<string> onlyKeywords)
        {
            if (onlyKeywords == null) return true;

            var list = new List<string>();
            foreach (var k in onlyKeywords)
            {
                if (!string.IsNullOrWhiteSpace(k)) list.Add(k.Trim());
            }

            if (list.Count == 0) return true;
            if (string.IsNullOrWhiteSpace(fullSearchableText)) return false;

            string target = fullSearchableText.ToUpperInvariant();
            string normTarget = target.Replace("_", "").Replace("-", "").Replace(" ", "").Replace("\t", "").Replace("/", "").Replace("\\", "");

            foreach (var rawKw in list)
            {
                string kwUpper = rawKw.ToUpperInvariant();
                string normKw = kwUpper.Replace("_", "").Replace("-", "").Replace(" ", "").Replace("\t", "").Replace("/", "").Replace("\\", "");
                if (string.IsNullOrEmpty(normKw)) continue;

                if (target.Contains(kwUpper) || normTarget.Contains(normKw))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Thu thập danh sách các cấu kiện cản trở tiềm năng từ ReferenceModelObject trong Tekla,
        /// trích xuất hình học B-Rep chuẩn xác 100% bằng extension method ToIfcGeometries() của GeometryHelper.
        /// </summary>
        public List<IfcTargetObject> CollectObstacles(
            Point rebarZoneMin, 
            Point rebarZoneMax, 
            ClashSettings settings, 
            List<ModelObject> userSelectedObstacles,
            Action<string> onStatusUpdate = null)
        {
            var refObjs = new List<ReferenceModelObject>();
            var addedIds = new HashSet<long>();

            Action<ReferenceModelObject> addRefObj = (ro) =>
            {
                if (ro != null && addedIds.Add(ro.Identifier.ID))
                {
                    refObjs.Add(ro);
                }
            };

            // Trường hợp A: Người dùng chọn trực tiếp vật thể tham chiếu IFC trên mô hình Tekla
            if (settings.IfcMode == IfcScopeMode.SelectedIfcOnly || (userSelectedObstacles != null && userSelectedObstacles.Count > 0))
            {
                onStatusUpdate?.Invoke("Đang thu thập các cấu kiện IFC được chọn...");
                foreach (var obj in userSelectedObstacles)
                {
                    if (obj is ReferenceModel refM)
                    {
                        var ch = refM.GetChildren();
                        while (ch.MoveNext())
                        {
                            if (ch.Current is ReferenceModelObject ro) addRefObj(ro);
                        }
                    }
                    else if (obj is ReferenceModelObject refObj)
                    {
                        addRefObj(refObj);
                    }
                }
            }
            else
            {
                // Trường hợp B: Quét vùng không gian 3D BoundingBox xung quanh cốt thép
                double buffer = 500.0 + settings.ClearanceMm;
                Point searchMin = new Point(rebarZoneMin.X - buffer, rebarZoneMin.Y - buffer, rebarZoneMin.Z - buffer);
                Point searchMax = new Point(rebarZoneMax.X + buffer, rebarZoneMax.Y + buffer, rebarZoneMax.Z + buffer);

                onStatusUpdate?.Invoke("Đang quét cấu kiện IFC trong phạm vi không gian cốt thép...");
                try
                {
                    var boxEnum = _model.GetModelObjectSelector().GetObjectsByBoundingBox(searchMin, searchMax);
                    while (boxEnum.MoveNext())
                    {
                        if (boxEnum.Current is ReferenceModelObject refObj)
                        {
                            if (settings.IfcMode == IfcScopeMode.SpecificFile && settings.TargetIfcFileNames != null && settings.TargetIfcFileNames.Count > 0)
                            {
                                string fileName = string.Empty;
                                try
                                {
                                    var parent = refObj.GetReferenceModel();
                                    if (parent != null) fileName = Path.GetFileName(parent.Filename);
                                }
                                catch { }

                                if (!string.IsNullOrEmpty(fileName) && !settings.MatchesIfcFile(fileName))
                                {
                                    continue;
                                }
                            }
                            addRefObj(refObj);
                        }
                    }
                }
                catch { }

                // Dự phòng nếu selector không trả về đối tượng
                if (refObjs.Count == 0)
                {
                    var refModels = GetReferenceModels();
                    foreach (var refModel in refModels)
                    {
                        if (refModel == null) continue;
                        string fileName = Path.GetFileName(refModel.Filename ?? string.Empty);
                        if (settings.IfcMode == IfcScopeMode.SpecificFile &&
                            settings.TargetIfcFileNames != null &&
                            settings.TargetIfcFileNames.Count > 0 &&
                            !settings.MatchesIfcFile(fileName))
                        {
                            continue;
                        }

                        var children = refModel.GetChildren();
                        while (children.MoveNext())
                        {
                            if (children.Current is ReferenceModelObject refObj)
                            {
                                addRefObj(refObj);
                            }
                        }
                    }
                }
            }

            if (refObjs.Count == 0) return new List<IfcTargetObject>();

            onStatusUpdate?.Invoke(string.Format("Đang nạp hình học B-Rep {0} cấu kiện IFC qua ToIfcGeometries...", refObjs.Count));

            // Thiết lập cấu hình IfcConvertOptions tích hợp SkipNames và OnlyNames
            var opts = new IfcConvertOptions
            {
                CoordinateSpace = CoordinateSpace.Global,
                TargetUnit = LengthUnit.Millimeters,
                ApplyVoids = false,
                TessellateNonPlanarFaces = true
            };

            if (settings.EnableIgnoredComponents && settings.IgnoredKeywords != null)
            {
                foreach (var k in settings.IgnoredKeywords)
                {
                    if (!string.IsNullOrWhiteSpace(k)) opts.AddSkipNames(k.Trim());
                }
            }

            if (settings.EnableOnlyComponents && settings.OnlyKeywords != null)
            {
                foreach (var k in settings.OnlyKeywords)
                {
                    if (!string.IsNullOrWhiteSpace(k)) opts.AddOnlyNames(k.Trim());
                }
            }

            // Gọi trích xuất sạch sẽ qua GeometryHelper.TeklaConvert
            return IfcGeometryBridge.ExtractIfcTargets(refObjs, opts);
        }

        /// <summary>
        /// Cấu trúc nội bộ lưu trữ dữ liệu trích xuất của cốt thép để xử lý song song đa luồng an toàn.
        /// </summary>
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
            public GeoPolyline3[] Polylines;
            public GeoAabb3[] PolylineAabbs;
        }

        /// <summary>
        /// Thuật toán kiểm tra va chạm cứng (Hard Clash Detection) chuẩn phong cách Navisworks.
        /// Sử dụng quy trình đường ống 2 pha đa luồng (Parallel Pipeline) kết hợp lọc AABB siêu tốc và cắt B-Rep chính xác 100%.
        /// </summary>
        public List<ClashResultItem> DetectClashes(
            List<Reinforcement> rebars, 
            List<IfcTargetObject> obstacles, 
            ClashSettings settings,
            Action<int, int> progressCallback = null,
            System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            var rawClashes = new System.Collections.Concurrent.ConcurrentBag<ClashResultItem>();
            if (rebars == null || obstacles == null || rebars.Count == 0 || obstacles.Count == 0)
                return new List<ClashResultItem>();

            // Xây dựng cây chỉ mục không gian BVH cho các cấu kiện IFC mục tiêu
            var bvhTree = new ObstacleBvhTree(obstacles);
            int totalRebars = rebars.Count;

            // Pha 1: Trích xuất tim thép (Centerline polylines) và tính sẵn AABB
            var rebarItems = new List<RebarExtractData>(totalRebars);
            for (int rIdx = 0; rIdx < totalRebars; rIdx++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rebar = rebars[rIdx];
                if (rebar == null) continue;

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

                // Lấy danh sách tim thép
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
                        try { rebarPolylines.Add(new GeoPolyline3(gPts)); } catch { }
                    }
                }

                if (rebarPolylines.Count == 0) continue;

                var polyArr = rebarPolylines.ToArray();
                var polyAabbs = new GeoAabb3[polyArr.Length];
                for (int p = 0; p < polyArr.Length; p++)
                {
                    polyAabbs[p] = polyArr[p].GetAabb();
                }

                // Tính bounding box tổng của cốt thép
                GeoAabb3 rAabb = polyAabbs[0];
                for (int p = 1; p < polyAabbs.Length; p++)
                {
                    rAabb = rAabb.Union(polyAabbs[p]);
                }

                Point rMin = new Point(rAabb.Min.X, rAabb.Min.Y, rAabb.Min.Z);
                Point rMax = new Point(rAabb.Max.X, rAabb.Max.Y, rAabb.Max.Z);

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
                    Polylines = polyArr,
                    PolylineAabbs = polyAabbs
                });
            }

            int validRebars = rebarItems.Count;
            if (validRebars == 0) return new List<ClashResultItem>();

            // Pha 2: Tính toán va chạm song song đa luồng (Tận dụng 100% tất cả các lõi CPU)
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

                var candidateTargets = new List<IfcTargetObject>();
                bvhTree.Query(expRMin, expRMax, candidateTargets);

                if (candidateTargets.Count > 0)
                {
                    foreach (var target in candidateTargets)
                    {
                        if (target == null || !target.HasSolids) continue;

                        // Lọc sớm SkipNames: Bỏ qua tính toán hình học cho cấu kiện bị bỏ qua
                        if (settings.EnableIgnoredComponents && settings.IgnoredKeywords != null && settings.IgnoredKeywords.Count > 0)
                        {
                            if (IsIgnoredComponent(target.EntityName, settings.IgnoredKeywords) ||
                                (!string.IsNullOrEmpty(target.IfcType) && IsIgnoredComponent(target.IfcType, settings.IgnoredKeywords)))
                            {
                                continue;
                            }
                        }

                        // Lọc sớm OnlyNames: Bỏ qua nếu không khớp danh sách cấu kiện chỉ định
                        if (settings.EnableOnlyComponents && settings.OnlyKeywords != null && settings.OnlyKeywords.Count > 0)
                        {
                            bool matchName = MatchesOnlyComponent(target.EntityName, settings.OnlyKeywords);
                            bool matchType = !string.IsNullOrEmpty(target.IfcType) && MatchesOnlyComponent(target.IfcType, settings.OnlyKeywords);
                            if (!matchName && !matchType)
                            {
                                continue;
                            }
                        }

                        double maxOverlap = 0.0;
                        Point bestClashPt = null;

                        for (int pIdx = 0; pIdx < rb.Polylines.Length; pIdx++)
                        {
                            var poly = rb.Polylines[pIdx];
                            var polyAabb = rb.PolylineAabbs[pIdx];

                            // Kiểm tra va chạm 2 bước qua IfcGeometryBridge (AABB filter -> B-Rep split)
                            if (IfcGeometryBridge.TestPolylineVsIfcTarget(poly, rb.Radius, settings.ClearanceMm, settings.ToleranceMm, target, out double polyOverlap, out Point clashPt, polyAabb))
                            {
                                if (polyOverlap > maxOverlap)
                                {
                                    maxOverlap = polyOverlap;
                                    bestClashPt = clashPt;
                                }
                            }
                        }

                        if (maxOverlap >= settings.ToleranceMm && bestClashPt != null)
                        {
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
                                IfcObjectId = target.Id,
                                IfcFileName = target.FileName,
                                IfcEntityName = target.EntityName,
                                IfcObject = target.ModelObject,
                                OverlapMm = Math.Round(maxOverlap, 1),
                                ClashPoint = bestClashPt,
                                MinPoint = new Point(Math.Min(rb.Min.X, target.BoundingBox.Min.X), Math.Min(rb.Min.Y, target.BoundingBox.Min.Y), Math.Min(rb.Min.Z, target.BoundingBox.Min.Z)),
                                MaxPoint = new Point(Math.Max(rb.Max.X, target.BoundingBox.Max.X), Math.Max(rb.Max.Y, target.BoundingBox.Max.Y), Math.Max(rb.Max.Z, target.BoundingBox.Max.Z))
                            });
                        }
                    }
                }

                int finished = System.Threading.Interlocked.Increment(ref processedCount);
                progressCallback?.Invoke(finished, validRebars);
            });

            // Khử trùng lặp va chạm (Deduplication): Giữ lại độ lấn lớn nhất cho mỗi cặp (RebarId, IfcObjectId)
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
    }

    /// <summary>
    /// Cây chỉ mục không gian BVH (Bounding Volume Hierarchy) hiệu năng cao cho các cấu kiện IFC 3D.
    /// Giảm độ phức tạp thuật toán tìm kiếm sơ bộ (Broad-phase) từ O(N * M) xuống O(N * log M).
    /// </summary>
    public class ObstacleBvhTree
    {
        private const int LeafThreshold = 4;

        /// <summary>Nút trong cây BVH.</summary>
        private class BvhNode
        {
            public Point MinPoint;
            public Point MaxPoint;
            public BvhNode Left;
            public BvhNode Right;
            public List<IfcTargetObject> Items;

            public bool IsLeaf => Items != null;
        }

        private readonly BvhNode _root;

        /// <summary>
        /// Khởi tạo và xây dựng cây BVH từ danh sách cấu kiện cản trở IFC.
        /// </summary>
        public ObstacleBvhTree(List<IfcTargetObject> obstacles)
        {
            if (obstacles == null || obstacles.Count == 0) return;
            var list = new List<IfcTargetObject>(obstacles);
            _root = BuildNode(list, 0, list.Count);
        }

        /// <summary>
        /// Xây dựng nút cây BVH đệ quy theo trục không gian có độ trải rộng lớn nhất.
        /// </summary>
        private static BvhNode BuildNode(List<IfcTargetObject> list, int start, int count)
        {
            if (count <= 0) return null;

            double bMinX = double.MaxValue, bMinY = double.MaxValue, bMinZ = double.MaxValue;
            double bMaxX = double.MinValue, bMaxY = double.MinValue, bMaxZ = double.MinValue;

            for (int i = start; i < start + count; i++)
            {
                var item = list[i];
                var box = item.BoundingBox;
                if (!box.IsEmpty)
                {
                    if (box.Min.X < bMinX) bMinX = box.Min.X;
                    if (box.Min.Y < bMinY) bMinY = box.Min.Y;
                    if (box.Min.Z < bMinZ) bMinZ = box.Min.Z;

                    if (box.Max.X > bMaxX) bMaxX = box.Max.X;
                    if (box.Max.Y > bMaxY) bMaxY = box.Max.Y;
                    if (box.Max.Z > bMaxZ) bMaxZ = box.Max.Z;
                }
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

            // Chia đôi theo trục có kích thước lớn nhất
            double dx = bMaxX - bMinX;
            double dy = bMaxY - bMinY;
            double dz = bMaxZ - bMinZ;

            int axis = 0; // 0=X, 1=Y, 2=Z
            if (dy > dx && dy >= dz) axis = 1;
            else if (dz > dx && dz >= dy) axis = 2;

            list.Sort(start, count, Comparer<IfcTargetObject>.Create((a, b) =>
            {
                double ca = axis == 0 ? (a.BoundingBox.Min.X + a.BoundingBox.Max.X) : (axis == 1 ? (a.BoundingBox.Min.Y + a.BoundingBox.Max.Y) : (a.BoundingBox.Min.Z + a.BoundingBox.Max.Z));
                double cb = axis == 0 ? (b.BoundingBox.Min.X + b.BoundingBox.Max.X) : (axis == 1 ? (b.BoundingBox.Min.Y + b.BoundingBox.Max.Y) : (b.BoundingBox.Min.Z + b.BoundingBox.Max.Z));
                return ca.CompareTo(cb);
            }));

            int mid = start + count / 2;
            node.Left = BuildNode(list, start, mid - start);
            node.Right = BuildNode(list, mid, count - (mid - start));

            return node;
        }

        /// <summary>
        /// Truy vấn tìm tất cả các cấu kiện IFC có hộp biên giao cắt với vùng hộp hỏi (qMin, qMax).
        /// </summary>
        public void Query(Point qMin, Point qMax, List<IfcTargetObject> results)
        {
            if (_root == null || results == null) return;
            QueryNode(_root, qMin, qMax, results);
        }

        /// <summary>
        /// Duyệt đệ quy cây BVH kiểm tra chồng lấn AABB.
        /// </summary>
        private static void QueryNode(BvhNode node, Point qMin, Point qMax, List<IfcTargetObject> results)
        {
            if (node == null) return;

            // Kiểm tra chồng lấn với hộp bao của nút BVH
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
                    var box = item.BoundingBox;
                    if (!box.IsEmpty)
                    {
                        if (qMin.X <= box.Max.X && qMax.X >= box.Min.X &&
                            qMin.Y <= box.Max.Y && qMax.Y >= box.Min.Y &&
                            qMin.Z <= box.Max.Z && qMax.Z >= box.Min.Z)
                        {
                            results.Add(item);
                        }
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
