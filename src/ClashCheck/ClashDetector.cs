using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using GeometryHelper.Clash;
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

        /// <summary>Whether to include surface-touching contacts without penetration (Touch clash). Default is false to prevent false positives.</summary>
        public bool IncludeTouching { get; set; } = false;

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

                // Với từ khóa ngắn (<= 3 ký tự như "LUG"), so khớp nguyên từ để tránh loại trừ nhầm các từ như "PLUG", "SLUG"
                if (kwUpper.Length <= 3)
                {
                    var tokens = target.Split(new char[] { '_', '-', ' ', '.', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        if (token.Equals(kwUpper, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (target.Contains(kwUpper) || normTarget.Contains(normKw))
                    {
                        return true;
                    }
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
        /// Thu thập đệ quy tất cả các nút lá (leaf objects có chứa hình học B-Rep) của cây ReferenceModelObject.
        /// Đồng thời lọc sơ bộ theo hộp bao AABB để tăng tốc độ xử lý tối đa.
        /// </summary>
        private static void CollectLeafReferenceModelObjects(
            ReferenceModelObject parent,
            List<ReferenceModelObject> list,
            HashSet<long> visitedIds,
            Point boxMin = null,
            Point boxMax = null)
        {
            if (parent == null || !visitedIds.Add(parent.Identifier.ID)) return;

            // KIỂM TRA SỚM HỘP BAO (EARLY HIERARCHICAL BOUNDING BOX PRUNING):
            // Nếu node này (tầng, khối nhà, nhóm cụm IFC) đã có tọa độ Bounding Box
            // và nằm hoàn toàn NGOÀI phạm vi vùng thép (boxMin -> boxMax):
            // -> TOÀN BỘ CÁC CẤU KIỆN CON BÊN DƯỚI CŨNG NẰM NGOÀI! CẮT TỈA (SKIP) CẢ CÂY CON NGAY TỨC THÌ!
            if (boxMin != null && boxMax != null)
            {
                double minX = 0, minY = 0, minZ = 0, maxX = 0, maxY = 0, maxZ = 0;
                if (parent.GetReportProperty("BOUNDING_BOX_MIN_X", ref minX) &&
                    parent.GetReportProperty("BOUNDING_BOX_MIN_Y", ref minY) &&
                    parent.GetReportProperty("BOUNDING_BOX_MIN_Z", ref minZ) &&
                    parent.GetReportProperty("BOUNDING_BOX_MAX_X", ref maxX) &&
                    parent.GetReportProperty("BOUNDING_BOX_MAX_Y", ref maxY) &&
                    parent.GetReportProperty("BOUNDING_BOX_MAX_Z", ref maxZ) && maxX > minX)
                {
                    if (maxX < boxMin.X || minX > boxMax.X ||
                        maxY < boxMin.Y || minY > boxMax.Y ||
                        maxZ < boxMin.Z || minZ > boxMax.Z)
                    {
                        return; // Toàn bộ nhánh cây này nằm ngoài vùng thép -> Bỏ qua lập tức!
                    }
                }
            }

            var children = parent.GetChildren();
            bool hasChild = false;
            while (children.MoveNext())
            {
                if (children.Current is ReferenceModelObject chObj)
                {
                    hasChild = true;
                    CollectLeafReferenceModelObjects(chObj, list, visitedIds, boxMin, boxMax);
                }
            }

            if (!hasChild)
            {
                list.Add(parent);
            }
        }

        /// <summary>
        /// Thu thập danh sách các cấu kiện cản trở tiềm năng từ ReferenceModelObject (IFC) và Tekla Part/Item,
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
            var directTargets = new List<IfcTargetObject>();
            var addedIds = new HashSet<long>();

            double buffer = 500.0 + settings.ClearanceMm;
            Point searchMin = new Point(rebarZoneMin.X - buffer, rebarZoneMin.Y - buffer, rebarZoneMin.Z - buffer);
            Point searchMax = new Point(rebarZoneMax.X + buffer, rebarZoneMax.Y + buffer, rebarZoneMax.Z + buffer);

            // Trường hợp A: Người dùng chọn trực tiếp vật thể tham chiếu IFC hoặc cấu kiện Tekla trên mô hình
            if (settings.IfcMode == IfcScopeMode.SelectedIfcOnly)
            {
                onStatusUpdate?.Invoke("Collecting selected obstacle objects...");
                if (userSelectedObstacles != null)
                {
                    foreach (var obj in userSelectedObstacles)
                    {
                        if (obj is Part part)
                        {
                            try
                            {
                                var s = part.GetSolid();
                                if (s != null && SolidConvert.TryToGeoSolid3(s, out GeoSolid3 partSolid))
                                {
                                    directTargets.Add(new IfcTargetObject
                                    {
                                        ModelObject = part,
                                        Id = part.Identifier.ID,
                                        FileName = "Tekla Model",
                                        EntityName = !string.IsNullOrEmpty(part.Name) ? part.Name : "Tekla Part",
                                        IfcType = part.GetType().Name,
                                        GlobalId = part.Identifier.GUID.ToString(),
                                        BoundingBox = partSolid.GetAabb(),
                                        Solids = new GeoSolid3[] { partSolid }
                                    });
                                }
                            }
                            catch { }
                        }
                        else if (obj is ReferenceModel refM)
                        {
                            var ch = refM.GetChildren();
                            while (ch.MoveNext())
                            {
                                if (ch.Current is ReferenceModelObject ro)
                                {
                                    CollectLeafReferenceModelObjects(ro, refObjs, addedIds);
                                }
                            }
                        }
                        else if (obj is ReferenceModelObject refObj)
                        {
                            CollectLeafReferenceModelObjects(refObj, refObjs, addedIds);
                        }
                    }
                }
            }
            else
            {
                // Trường hợp B: Quét cấu kiện trong file IFC (SpecificFile hoặc AutoSpatialAllIfc)
                onStatusUpdate?.Invoke("Querying spatial index for IFC obstacles in rebar bounding range...");

                // 1. Tận dụng trực tiếp Spatial Index C++ của Tekla qua GetObjectsByBoundingBox (O(log N))
                // Chỉ thu thập ReferenceModelObject thuộc file IFC được chọn, KHÔNG quét Part gốc của Tekla (coupler, anchor...)
                try
                {
                    var boxEnum = _model.GetModelObjectSelector().GetObjectsByBoundingBox(searchMin, searchMax);
                    while (boxEnum.MoveNext())
                    {
                        if (boxEnum.Current is ReferenceModelObject rmo)
                        {
                            if (addedIds.Add(rmo.Identifier.ID))
                            {
                                bool matchFile = true;
                                if (settings.IfcMode == IfcScopeMode.SpecificFile &&
                                    settings.TargetIfcFileNames != null &&
                                    settings.TargetIfcFileNames.Count > 0)
                                {
                                    try
                                    {
                                        var rm = rmo.GetReferenceModel();
                                        string fileName = rm != null ? Path.GetFileName(rm.Filename ?? string.Empty) : string.Empty;
                                        matchFile = settings.MatchesIfcFile(fileName);
                                    }
                                    catch { }
                                }

                                if (matchFile)
                                {
                                    refObjs.Add(rmo);
                                }
                            }
                        }
                    }
                }
                catch { }

                // 2. Cơ chế dự phòng (Fallback): Chỉ khi Spatial Index không trả về ReferenceModelObject nào
                // (ví dụ công tắc chọn Reference Model trong thanh công cụ Tekla bị người dùng tắt),
                // mới duyệt đệ quy cây đối tượng có cắt tỉa cành cha.
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
                            if (children.Current is ReferenceModelObject rootRo)
                            {
                                CollectLeafReferenceModelObjects(rootRo, refObjs, addedIds, searchMin, searchMax);
                            }
                        }
                    }
                }
            }

            var allTargets = new List<IfcTargetObject>();
            if (directTargets.Count > 0)
            {
                allTargets.AddRange(directTargets);
            }

            if (refObjs.Count > 0)
            {
                onStatusUpdate?.Invoke(string.Format("Loading B-Rep geometries for {0} IFC objects...", refObjs.Count));

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
                var ifcTargets = IfcGeometryBridge.ExtractIfcTargets(refObjs, opts);
                if (ifcTargets != null && ifcTargets.Count > 0)
                {
                    allTargets.AddRange(ifcTargets);
                }
            }

            return allTargets;
        }
        /// <summary>
        /// Cấu trúc nội bộ lưu trữ dữ liệu trích xuất của cốt thép để xử lý va chạm và mapping kết quả.
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
        }

        /// <summary>
        /// Thuật toán kiểm tra va chạm tối ưu hiệu năng cao sử dụng GeometryHelper.Clash.Clash3.Find
        /// kết hợp ClashBar (tim thép dạng GeoPolylineArc3 + bán kính thực tế) và các khối GeoSolid3 từ IFC.
        /// Tốc độ xử lý song song đa luồng tối đa, đo lường chính xác thể tích, độ sâu và chiều dài ngập.
        /// </summary>
        public List<ClashResultItem> DetectClashes(
            List<Reinforcement> rebars, 
            List<IfcTargetObject> obstacles, 
            ClashSettings settings,
            Action<int, int> progressCallback = null,
            System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (rebars == null || obstacles == null || rebars.Count == 0 || obstacles.Count == 0)
                return new List<ClashResultItem>();

            cancellationToken.ThrowIfCancellationRequested();

            // Pha 1: Trích xuất các thanh thép thành ClashBar (giữ nguyên cung tròn GeoPolylineArc3 và bán kính)
            var bars = new List<ClashBar>();
            var barOwners = new List<RebarExtractData>();
            int totalRebars = rebars.Count;

            for (int rIdx = 0; rIdx < totalRebars; rIdx++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rebar = rebars[rIdx];
                if (rebar == null) continue;

                double dia = 16.0;
                rebar.GetReportProperty("DIAMETER", ref dia);
                if (dia <= 0) dia = 16.0;

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

                var rebarData = new RebarExtractData
                {
                    Rebar = rebar,
                    Id = rebar.Identifier.ID,
                    Guid = rebar.Identifier.GUID.ToString(),
                    Name = !string.IsNullOrEmpty(rebarName) ? rebarName : "REBAR",
                    Size = rebarSize,
                    Grade = rebarGrade,
                    Pos = rebarPos,
                    HostPart = hostPart,
                    Length = Math.Round(rebarLen, 0),
                    Radius = dia / 2.0
                };

                ArrayList geometries = null;
                try
                {
                    geometries = rebar.GetRebarGeometriesWithoutClashes(true);
                    if (geometries == null || geometries.Count == 0)
                    {
                        geometries = rebar.GetRebarGeometries(true);
                    }
                }
                catch { }

                if (geometries != null)
                {
                    foreach (object geomObj in geometries)
                    {
                        if (geomObj is RebarGeometry rg)
                        {
                            try
                            {
                                var polyArc = rg.ToGeoPolylineArc3();
                                double barRad = rg.ToBarRadius();
                                if (barRad <= 0) barRad = rebarData.Radius;

                                bars.Add(new ClashBar(polyArc, barRad));
                                barOwners.Add(rebarData);
                            }
                            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                            {
                                // Bỏ qua thanh đơn lẻ không xác định được hình học hoặc bán kính
                            }
                        }
                    }
                }

                progressCallback?.Invoke(rIdx + 1, totalRebars);
            }

            if (bars.Count == 0) return new List<ClashResultItem>();

            // Pha 2: Gom toàn bộ các khối GeoSolid3 từ cấu kiện cản trở mục tiêu
            cancellationToken.ThrowIfCancellationRequested();
            var bodies = new List<GeoSolid3>();
            var bodyOwners = new List<IfcTargetObject>();

            foreach (var target in obstacles)
            {
                if (target == null || !target.HasSolids) continue;

                // Lọc sớm theo từ khóa SkipNames
                if (settings.EnableIgnoredComponents && settings.IgnoredKeywords != null && settings.IgnoredKeywords.Count > 0)
                {
                    if (IsIgnoredComponent(target.EntityName, settings.IgnoredKeywords) ||
                        (!string.IsNullOrEmpty(target.IfcType) && IsIgnoredComponent(target.IfcType, settings.IgnoredKeywords)))
                    {
                        continue;
                    }
                }

                // Lọc sớm theo từ khóa OnlyNames
                if (settings.EnableOnlyComponents && settings.OnlyKeywords != null && settings.OnlyKeywords.Count > 0)
                {
                    bool matchName = MatchesOnlyComponent(target.EntityName, settings.OnlyKeywords);
                    bool matchType = !string.IsNullOrEmpty(target.IfcType) && MatchesOnlyComponent(target.IfcType, settings.OnlyKeywords);
                    if (!matchName && !matchType) continue;
                }

                foreach (var solid in target.Solids)
                {
                    if (solid != null && solid.Faces != null && solid.Faces.Count > 0)
                    {
                        bodies.Add(solid);
                        bodyOwners.Add(target);
                    }
                }
            }

            if (bodies.Count == 0) return new List<ClashResultItem>();

            // Pha 3: Chạy bộ máy va chạm Clash3.Find cực nhanh từ GeometryHelper (tự động đa luồng & dựng BVH nội bộ)
            cancellationToken.ThrowIfCancellationRequested();
            var clashOptions = new ClashOptions(
                clearance: settings.ClearanceMm,
                includeTouching: settings.IncludeTouching,
                maxDegreeOfParallelism: Environment.ProcessorCount,
                minimumDepth: settings.ToleranceMm,
                minimumVolume: 0.0
            );

            ClashResult[] clashes = Clash3.Find(bars, bodies, clashOptions);
            if (clashes == null || clashes.Length == 0) return new List<ClashResultItem>();

            // Pha 4: Chuyển đổi kết quả ClashResult[] sang ClashResultItem và khử trùng lặp theo (RebarId, IfcObjectId)
            var uniqueMap = new Dictionary<Tuple<long, long>, ClashResultItem>();

            foreach (var clash in clashes)
            {
                if (clash == null || clash.Kind == ClashKind.Unresolved || clash.Error != null) continue;
                if (clash.First < 0 || clash.First >= barOwners.Count) continue;
                if (clash.Second < 0 || clash.Second >= bodyOwners.Count) continue;

                // If touching faces are not requested, ignore ClashKind.Touch
                if (clash.Kind == ClashKind.Touch && !settings.IncludeTouching)
                {
                    continue;
                }

                var rb = barOwners[clash.First];
                var target = bodyOwners[clash.Second];

                double depth = clash.Depth;
                double lengthInside = clash.LengthInside;
                double overlap = depth > 0 ? depth : (lengthInside > 0 ? lengthInside : 0.0);

                if (clash.Kind == ClashKind.Clearance && settings.ClearanceMm > 0)
                {
                    overlap = Math.Max(0.0, settings.ClearanceMm - clash.Distance);
                }

                // Nếu là va chạm Hard mà overlap bé hơn dung sai tối thiểu, bỏ qua
                if (clash.Kind == ClashKind.Hard && overlap < settings.ToleranceMm)
                {
                    continue;
                }

                Point clashPt = clash.Location != null 
                    ? new Point(clash.Location.X, clash.Location.Y, clash.Location.Z) 
                    : new Point(0, 0, 0);

                var item = new ClashResultItem
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
                    IfcGuid = target.GlobalId,
                    IfcFileName = target.FileName,
                    IfcEntityName = target.EntityName,
                    IfcObject = target.ModelObject,
                    ClashType = clash.Kind.ToString(),
                    OverlapMm = Math.Round(overlap, 1),
                    VolumeMm3 = Math.Round(clash.Volume, 1),
                    LengthInsideMm = Math.Round(clash.LengthInside, 1),
                    ContactAreaMm2 = Math.Round(clash.ContactArea, 1),
                    DistanceMm = Math.Round(clash.Distance, 1),
                    ClashPoint = clashPt,
                    MinPoint = !target.BoundingBox.IsEmpty
                        ? new Point(target.BoundingBox.Min.X, target.BoundingBox.Min.Y, target.BoundingBox.Min.Z)
                        : clashPt,
                    MaxPoint = !target.BoundingBox.IsEmpty
                        ? new Point(target.BoundingBox.Max.X, target.BoundingBox.Max.Y, target.BoundingBox.Max.Z)
                        : clashPt
                };

                var pairKey = Tuple.Create(item.RebarId, item.IfcObjectId);
                if (!uniqueMap.TryGetValue(pairKey, out var existing) || item.OverlapMm > existing.OverlapMm || (item.OverlapMm == existing.OverlapMm && item.VolumeMm3 > existing.VolumeMm3))
                {
                    uniqueMap[pairKey] = item;
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
}
