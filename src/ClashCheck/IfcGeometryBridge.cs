using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using GeometryHelper;
using GeometryHelper.Core;
using GeometryHelper.Geometry;
using GeometryHelper.IfcConvert.Core;
using GeometryHelper.IfcConvert.Models;
using GeometryHelper.TeklaConvert;

namespace BimCommands.Tekla.ClashCheck
{
    /// <summary>
    /// Đối tượng đích IFC đại diện cho cấu kiện tham chiếu trong mô hình Tekla,
    /// chứa thông tin nhận diện đối tượng và khối hình học B-Rep chuẩn xác 100% từ GeometryHelper.
    /// </summary>
    public class IfcTargetObject
    {
        /// <summary>Đối tượng gốc trong Tekla Structures (ReferenceModelObject hoặc Part/Item).</summary>
        public ModelObject ModelObject { get; set; }

        /// <summary>Mã định danh ID của đối tượng trong Tekla.</summary>
        public long Id { get; set; }

        /// <summary>Tên file IFC chứa cấu kiện này.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Tên sản phẩm IFC (ví dụ: BEAM_W12x26, H400x200).</summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>Kiểu phần tử IFC (ví dụ: IfcBeam, IfcColumn, IfcPipeSegment).</summary>
        public string IfcType { get; set; } = string.Empty;

        /// <summary>Mã IFC GlobalId (GUID 22 ký tự duy nhất toàn cầu).</summary>
        public string GlobalId { get; set; } = string.Empty;

        /// <summary>Hộp bao không gian AABB chính xác của cấu kiện (dùng cho lọc nhanh AABB O(1)).</summary>
        public GeoAabb3 BoundingBox { get; set; } = GeoAabb3.Empty;

        /// <summary>Mảng các khối B-Rep chính xác 100% của cấu kiện từ GeometryHelper.</summary>
        public GeoSolid3[] Solids { get; set; } = new GeoSolid3[0];

        /// <summary>Kiểm tra cấu kiện có khối B-Rep hợp lệ để kiểm tra va chạm hay không.</summary>
        public bool HasSolids => Solids != null && Solids.Length > 0;
    }

    /// <summary>
    /// Cầu nối trích xuất và kiểm tra va chạm hình học tinh gọn giữa Tekla ReferenceModelObject
    /// và các khối hình học không gian GeometryHelper, sử dụng trực tiếp IfcProductGeometry.
    /// </summary>
    public static class IfcGeometryBridge
    {
        /// <summary>Dung sai dung sai chung tái sử dụng cho các phép toán va chạm để triệt tiêu cấp phát rác GC.</summary>
        private static readonly Tolerance S_ClashTolerance = new Tolerance(0.01, 0.01, Math.PI / 180.0, 0.01);

        /// <summary>Bộ nhớ đệm lưu trữ hình học đã trích xuất theo ID của ReferenceModelObject để tránh giải mã lặp lại.</summary>
        private static readonly ConcurrentDictionary<long, IfcTargetObject> _targetCache = new ConcurrentDictionary<long, IfcTargetObject>();

        /// <summary>
        /// Xóa sạch bộ nhớ đệm hình học của các cấu kiện IFC đã xử lý.
        /// </summary>
        public static void ClearCache()
        {
            _targetCache.Clear();
        }

        /// <summary>
        /// Trích xuất danh sách IfcTargetObject từ danh sách ReferenceModelObject thông qua
        /// extension method chính thức ToIfcGeometries() của GeometryHelper.TeklaConvert.
        /// Tự động áp dụng các bộ lọc SkipNames và OnlyNames được thiết lập trong IfcConvertOptions.
        /// </summary>
        public static List<IfcTargetObject> ExtractIfcTargets(
            IEnumerable<ReferenceModelObject> refObjs,
            IfcConvertOptions options = null)
        {
            var result = new List<IfcTargetObject>();
            if (refObjs == null) return result;

            var uncachedList = new List<ReferenceModelObject>();
            var guidMap = new Dictionary<string, Queue<ReferenceModelObject>>(StringComparer.OrdinalIgnoreCase);

            foreach (var ro in refObjs)
            {
                if (ro == null) continue;
                long id = ro.Identifier.ID;
                if (_targetCache.TryGetValue(id, out var cached))
                {
                    result.Add(cached);
                }
                else
                {
                    uncachedList.Add(ro);
                    string guid = ro.GetIfcGuid();
                    if (!string.IsNullOrEmpty(guid))
                    {
                        if (!guidMap.TryGetValue(guid, out var q))
                        {
                            q = new Queue<ReferenceModelObject>();
                            guidMap[guid] = q;
                        }
                        q.Enqueue(ro);
                    }
                }
            }

            if (uncachedList.Count == 0) return result;

            if (options == null)
            {
                options = new IfcConvertOptions
                {
                    CoordinateSpace = CoordinateSpace.Global,
                    TargetUnit = LengthUnit.Millimeters,
                    ApplyVoids = false,
                    TessellateNonPlanarFaces = true
                };
            }

            try
            {
                // Gọi extension method chính thức của GeometryHelper.TeklaConvert trích xuất hàng loạt theo lô
                IReadOnlyList<IfcProductGeometry> ifcGeoms = uncachedList.ToIfcGeometries(options);
                if (ifcGeoms != null)
                {
                    var matchedIds = new HashSet<long>();
                    int fallbackIdx = 0;

                    foreach (var geom in ifcGeoms)
                    {
                        if (geom == null || geom.IsEmpty) continue;

                        ReferenceModelObject matchedObj = null;
                        if (!string.IsNullOrEmpty(geom.GlobalId) && guidMap.TryGetValue(geom.GlobalId, out var q) && q.Count > 0)
                        {
                            matchedObj = q.Dequeue();
                        }
                        else
                        {
                            // Fallback: match by sequence for items without GUID or unmatched GlobalId
                            while (fallbackIdx < uncachedList.Count && matchedIds.Contains(uncachedList[fallbackIdx].Identifier.ID))
                            {
                                fallbackIdx++;
                            }
                            if (fallbackIdx < uncachedList.Count)
                            {
                                matchedObj = uncachedList[fallbackIdx++];
                            }
                        }

                        if (matchedObj != null)
                        {
                            matchedIds.Add(matchedObj.Identifier.ID);
                        }

                        string fileName = string.Empty;
                        if (matchedObj != null)
                        {
                            try
                            {
                                var parentRef = matchedObj.GetReferenceModel();
                                if (parentRef != null) fileName = System.IO.Path.GetFileName(parentRef.Filename);
                            }
                            catch { }
                        }

                        var solids = (geom.Solids != null && geom.Solids.Count > 0)
                            ? geom.Solids.ToArray()
                            : new GeoSolid3[0];

                        var target = new IfcTargetObject
                        {
                            ModelObject = matchedObj,
                            Id = matchedObj != null ? matchedObj.Identifier.ID : 0,
                            FileName = fileName,
                            EntityName = !string.IsNullOrEmpty(geom.Name) ? geom.Name : (matchedObj != null ? "IFC_OBJECT" : "UNKNOWN"),
                            IfcType = geom.IfcType ?? string.Empty,
                            GlobalId = geom.GlobalId ?? string.Empty,
                            BoundingBox = geom.BoundingBox,
                            Solids = solids
                        };

                        if (matchedObj != null)
                        {
                            _targetCache[matchedObj.Identifier.ID] = target;
                        }
                        result.Add(target);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi trích xuất ToIfcGeometries: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra va chạm toàn diện siêu tốc giữa đường tim thép đa đoạn (GeoPolyline3) và cấu kiện IFC:
        /// - Bước 1: Kiểm tra giao cắt hộp bao AABB (BoundingBox) O(1) < 1 nano-giây.
        /// - Bước 2: Kiểm tra đâm xuyên B-Rep chuẩn xác 100% bằng TrySplitBy của GeometryHelper.
        /// - Bước 3: Kiểm tra vi phạm khoảng hở an toàn (Clearance Check) nếu được thiết lập > 0.
        /// </summary>
        /// <summary>
        /// Thuật toán kiểm tra va chạm hình học chuẩn xác giữa một thanh thép (dạng polyline + bán kính r)
        /// và một cấu kiện cản trở (chứa các khối B-Rep GeoSolid3).
        /// Hỗ trợ chuẩn hóa Navisworks: phân biệt rõ ràng giữa tiếp giáp bề mặt rất nhỏ (end-point touch / kissing surface)
        /// và va chạm đâm xuyên thực thể (hard penetration).
        /// </summary>
        public static bool TestPolylineVsIfcTarget(
            GeoPolyline3 rebarPolyline,
            double rebarRadius,
            double clearance,
            double tolerance,
            IfcTargetObject target,
            out double overlap,
            out Point clashPt,
            GeoAabb3 polyAabb)
        {
            overlap = 0.0;
            clashPt = null;
            if (rebarPolyline == null || target == null || !target.HasSolids) return false;

            double effRadius = rebarRadius + clearance;
            double effectiveTolerance = Math.Max(tolerance, 0.0);

            // BƯỚC 1: Lọc nhanh Hộp bao AABB siêu tốc (O(1))
            if (!target.BoundingBox.IsEmpty && !polyAabb.IsEmpty)
            {
                if (!polyAabb.Expand(effRadius).CollidesWith(target.BoundingBox))
                {
                    return false;
                }
            }

            // BƯỚC 2: Kiểm tra đâm xuyên ruột thực thể (Internal Penetration qua B-Rep Split)
            bool splitSuccess = false;
            GeoPolyline3[] inside = null;
            GeoPolyline3[] outside = null;

            try
            {
                if (target.Solids.Length == 1)
                {
                    splitSuccess = rebarPolyline.TrySplitBy(target.Solids[0], out inside, out outside, S_ClashTolerance);
                }
                else
                {
                    splitSuccess = rebarPolyline.TrySplitBy(target.Solids, out inside, out outside, S_ClashTolerance);
                }
            }
            catch { }

            if (splitSuccess && inside != null && inside.Length > 0)
            {
                double maxInsideLen = 0.0;
                GeoPoint3 bestInsidePt = inside[0].StartPoint;
                bool isTruePenetration = false;

                foreach (var piece in inside)
                {
                    if (piece == null || piece.Length <= 0.0) continue;

                    GeoPoint3 midPt;
                    if (piece.VertexCount >= 2)
                    {
                        var p0 = piece[0];
                        var p1 = piece[piece.VertexCount - 1];
                        midPt = new GeoPoint3((p0.X + p1.X) / 2.0, (p0.Y + p1.Y) / 2.0, (p0.Z + p1.Z) / 2.0);
                    }
                    else
                    {
                        midPt = piece.StartPoint;
                    }

                    // Kiểm tra vị trí của điểm giữa đối với các khối Solid
                    bool insideSolid = false;
                    foreach (var solid in target.Solids)
                    {
                        if (solid == null) continue;
                        try
                        {
                            var loc = solid.Locate(midPt, S_ClashTolerance);
                            if (loc == GeometryHelper.Enums.PointLocation.Inside)
                            {
                                insideSolid = true;
                                break;
                            }
                        }
                        catch { }
                    }

                    // Bỏ qua tiếp xúc bề mặt rất nhỏ: chỉ ghi nhận khi đoạn cắt trong ruột có chiều dài đáng kể (>= tolerance và >= 0.5mm)
                    if (insideSolid || piece.Length >= Math.Max(0.5, effectiveTolerance))
                    {
                        if (piece.Length > maxInsideLen)
                        {
                            maxInsideLen = piece.Length;
                            bestInsidePt = midPt;
                            isTruePenetration = true;
                        }
                    }
                }

                // Nếu là đâm xuyên thực sự và chiều sâu đâm xuyên vượt qua ngưỡng dung sai (>= tolerance và >= 0.5mm)
                // Trường hợp tiếp giáp 0.01mm (maxInsideLen = 0.01mm < tolerance): BỎ QUA HOÀN TOÀN!
                if (isTruePenetration && maxInsideLen >= effectiveTolerance && maxInsideLen >= 0.5)
                {
                    double actualDepth = Math.Min(maxInsideLen, rebarRadius * 2.0);
                    overlap = Math.Round(actualDepth, 2);
                    clashPt = new Point(bestInsidePt.X, bestInsidePt.Y, bestInsidePt.Z);
                    return true;
                }
            }

            // BƯỚC 3: Kiểm tra tiếp xúc mặt bên (Lateral / Radial Skin Contact)
            // Chỉ xét khi có vi phạm khoảng hở hoặc thân thanh thép tiếp xúc lún vào mặt bên
            double maxLateralPenetration = 0.0;
            Point bestLateralContactPt = null;

            for (int eIdx = 0; eIdx < rebarPolyline.EdgeCount; eIdx++)
            {
                var edge = rebarPolyline.GetEdgeAt(eIdx);
                double eMinX = Math.Min(edge.StartPoint.X, edge.EndPoint.X) - effRadius;
                double eMaxX = Math.Max(edge.StartPoint.X, edge.EndPoint.X) + effRadius;
                double eMinY = Math.Min(edge.StartPoint.Y, edge.EndPoint.Y) - effRadius;
                double eMaxY = Math.Max(edge.StartPoint.Y, edge.EndPoint.Y) + effRadius;
                double eMinZ = Math.Min(edge.StartPoint.Z, edge.EndPoint.Z) - effRadius;
                double eMaxZ = Math.Max(edge.StartPoint.Z, edge.EndPoint.Z) + effRadius;

                foreach (var solid in target.Solids)
                {
                    if (solid == null) continue;
                    var sAabb = solid.GetAabb();
                    if (!sAabb.IsEmpty)
                    {
                        if (eMinX > sAabb.Max.X || eMaxX < sAabb.Min.X ||
                            eMinY > sAabb.Max.Y || eMaxY < sAabb.Min.Y ||
                            eMinZ > sAabb.Max.Z || eMaxZ < sAabb.Min.Z)
                        {
                            continue;
                        }
                    }

                    double dist = Distance3.DistanceTo(edge, solid, S_ClashTolerance);
                    if (dist <= effRadius)
                    {
                        double dStart = solid.DistanceTo(edge.StartPoint);
                        double dEnd = solid.DistanceTo(edge.EndPoint);

                        // QUAN TRỌNG: Kiểm tra xem điểm gần nhất có phải là đầu mút thanh thép hay không!
                        // Nếu điểm gần nhất là StartPoint hoặc EndPoint (chênh lệch <= 0.05mm so với dist):
                        // Đây là trường hợp ĐẦU MÚT thanh thép hướng vào mặt phẳng!
                        // Với đầu mút phẳng (flat end cut), nếu tim thép chưa vào ruột (đã xét ở Bước 2),
                        // thì độ lún dọc trục = 0 (chỉ là tiếp xúc đầu mút 0.01mm hoặc cách 0.01mm) => BỎ QUA!
                        bool isEndPointContact = Math.Abs(dStart - dist) <= 0.05 || Math.Abs(dEnd - dist) <= 0.05;
                        if (isEndPointContact)
                        {
                            // Đầu mút tiếp giáp bề mặt rất nhỏ (<= 0.01mm) -> BỎ QUA HOÀN TOÀN!
                            continue;
                        }

                        // Điểm gần nhất nằm ở THÂN THANH THÉP (Tiếp xúc mặt bên):
                        double lateralPenetration = effRadius - dist;
                        if (lateralPenetration > maxLateralPenetration)
                        {
                            maxLateralPenetration = lateralPenetration;

                            GeoPoint3 candidatePt = new GeoPoint3(
                                (edge.StartPoint.X + edge.EndPoint.X) / 2.0,
                                (edge.StartPoint.Y + edge.EndPoint.Y) / 2.0,
                                (edge.StartPoint.Z + edge.EndPoint.Z) / 2.0);

                            bestLateralContactPt = new Point(candidatePt.X, candidatePt.Y, candidatePt.Z);
                        }
                    }
                }
            }

            // Với tiếp xúc mặt bên:
            // Bỏ qua nếu độ lún mặt bên rất nhỏ (< tolerance hoặc < 0.5mm khi không đo khoảng hở)
            double minLateralThreshold = (clearance <= 0.0 && effectiveTolerance < 0.5) ? 0.5 : effectiveTolerance;
            if (maxLateralPenetration >= minLateralThreshold && bestLateralContactPt != null)
            {
                overlap = Math.Round(maxLateralPenetration, 2);
                clashPt = bestLateralContactPt;
                return true;
            }

            return false;
        }
    }
}
