using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BimCommands.Tekla.ClashCheck
{
    /// <summary>
    /// Phân loại mức độ nghiêm trọng của va chạm dựa trên độ lấn (overlap):
    /// - Nhẹ (Minor): &lt; 10mm (chạm mép hoặc vi phạm khoảng hở an toàn).
    /// - Trung bình (Medium): 10mm - 50mm.
    /// - Nghiêm trọng (Severe): &gt; 50mm (cắt sâu xuyên qua cấu kiện).
    /// </summary>
    public enum ClashSeverity
    {
        /// <summary>Va chạm nhẹ hoặc vi phạm khoảng hở (&lt; 10mm)</summary>
        Minor,

        /// <summary>Va chạm mức độ trung bình (10mm - 50mm)</summary>
        Medium,

        /// <summary>Va chạm nghiêm trọng, đâm xuyên sâu (&gt; 50mm)</summary>
        Severe
    }

    /// <summary>
    /// Đối tượng lưu trữ thông tin chi tiết một điểm va chạm giữa thanh thép và cấu kiện IFC tham chiếu.
    /// Bao gồm đầy đủ thông số kỹ thuật của thanh thép, thông tin cấu kiện IFC và tọa độ va chạm 3D.
    /// </summary>
    public class ClashResultItem
    {
        /// <summary>Số thứ tự của va chạm trong danh sách kết quả (bắt đầu từ 1).</summary>
        public int Index { get; set; }
        
        #region Thông tin Cốt thép (Tekla Rebar)

        /// <summary>Mã định danh số nguyên (Identifier ID) của thanh thép trong mô hình Tekla.</summary>
        public long RebarId { get; set; }

        /// <summary>Chuỗi GUID định danh toàn cục của đối tượng thép.</summary>
        public string RebarGuid { get; set; } = string.Empty;

        /// <summary>Tên quy cách của thép (ví dụ: REBAR, TIE, STIRRUP).</summary>
        public string RebarName { get; set; } = string.Empty;

        /// <summary>Kích thước / đường kính danh định của thép (ví dụ: D16, D20, D25).</summary>
        public string RebarSize { get; set; } = string.Empty;

        /// <summary>Mác thép / cấp độ bền (ví dụ: CB400-V, SD390, Gr60).</summary>
        public string RebarGrade { get; set; } = string.Empty;

        /// <summary>Số hiệu đánh số vị trí (Position / Mark) của thép sau khi Numbering.</summary>
        public string RebarPos { get; set; } = string.Empty;

        /// <summary>Tên của cấu kiện bê tông chứa thanh thép (Host Part / Main Part).</summary>
        public string HostPartName { get; set; } = string.Empty;

        /// <summary>Chiều dài tổng thể của thanh thép theo thiết kế (đơn vị: mm).</summary>
        public double RebarLength { get; set; }

        /// <summary>Tham chiếu đến đối tượng Reinforcement gốc trong Tekla Open API để hỗ trợ chọn và highlight.</summary>
        public Reinforcement RebarObject { get; set; }

        #endregion

        #region Thông tin Cấu kiện IFC (Reference Model Object)

        /// <summary>Mã ID của cấu kiện IFC trong mô hình Tekla.</summary>
        public long IfcObjectId { get; set; }

        /// <summary>Mã GlobalId gốc trong file IFC (EXTERNAL.GUID).</summary>
        public string IfcGuid { get; set; } = string.Empty;

        /// <summary>Tên file IFC nguồn chứa cấu kiện này (ví dụ: MEP.ifc, Structure_Steel.ifc).</summary>
        public string IfcFileName { get; set; } = string.Empty;

        /// <summary>Tên thực thể hoặc loại cấu kiện IFC (ví dụ: IfcBeam, IfcColumn, IfcPipeSegment).</summary>
        public string IfcEntityName { get; set; } = string.Empty;

        /// <summary>Tham chiếu đến đối tượng gốc trong Tekla Open API (ReferenceModelObject hoặc Part/Item).</summary>
        public ModelObject IfcObject { get; set; }

        #endregion

        #region Chi tiết Va chạm (Clash Details)

        /// <summary>
        /// Độ sâu chồng lấn lớn nhất giữa thanh thép và cấu kiện IFC (đơn vị: mm).
        /// Bao gồm cả bán kính danh định của thép và độ xuyên tâm.
        /// </summary>
        public double OverlapMm { get; set; }

        /// <summary>Tọa độ tâm điểm va chạm không gian 3D (World Coordinates).</summary>
        public Point ClashPoint { get; set; }

        /// <summary>Tọa độ góc dưới cực tiểu của hộp bao chung (AABB) giữa thép và vật cản.</summary>
        public Point MinPoint { get; set; }

        /// <summary>Tọa độ góc trên cực đại của hộp bao chung (AABB) giữa thép và vật cản.</summary>
        public Point MaxPoint { get; set; }
        
        /// <summary>
        /// Tự động tính toán mức độ nghiêm trọng dựa trên độ lấn OverlapMm.
        /// </summary>
        public ClashSeverity Severity
        {
            get
            {
                if (OverlapMm > 50.0) return ClashSeverity.Severe;
                if (OverlapMm > 10.0) return ClashSeverity.Medium;
                return ClashSeverity.Minor;
            }
        }

        /// <summary>
        /// Chuỗi định dạng hiển thị tọa độ va chạm (X, Y, Z) để trình bày trên DataGridView.
        /// </summary>
        public string ClashPointDisplay
        {
            get
            {
                if (ClashPoint == null) return "N/A";
                return string.Format("({0:F1}, {1:F1}, {2:F1})", ClashPoint.X, ClashPoint.Y, ClashPoint.Z);
            }
        }

        #endregion
    }
}

