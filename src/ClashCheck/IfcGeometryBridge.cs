using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Tekla.Structures.Model;
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
    }
}
