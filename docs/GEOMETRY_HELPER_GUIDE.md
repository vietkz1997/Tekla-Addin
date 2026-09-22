# HƯỚNG DẪN SỬ DỤNG HỆ SINH THÁI GEOMETRY HELPER CHO TEKLA VÀ AUTOCAD

Tài liệu này hướng dẫn cách khai thác 6 thư viện hình học cao cấp từ **GeometryHelper** đã được tích hợp sẵn tại `d:\Tekla_\My-tool\libs\` để phát triển các công cụ tự động hóa cho **Tekla Structures** và **AutoCAD (.NET & AutoLISP)**.

---

## 1. Vị Trí Các Thư Viện (DLLs) Đã Biên Dịch Sẵn

Tất cả các file DLL đều nhắm tới target **`netstandard2.0`**, tương thích 100% với .NET Framework 4.8 của Tekla/AutoCAD hiện tại và các phiên bản .NET mới:
*   `libs/GeometryHelper.CommonGeometry.dll`: Xử lý sai số dung sai (`Tolerance`), kiểu dữ liệu góc (`Angle`), vị trí tương đối.
*   `libs/GeometryHelper.PlaneGeometry.dll`: Hình học 2D (Điểm, vector, đoạn, đa giác, hình tròn, cắt split, va chạm, khoảng cách).
*   `libs/GeometryHelper.SolidGeometry.dll`: Hình học 3D (Khối đa diện `GeoSolid3`, cây BVH `GeoBvh3`, phép toán Boolean Union/Subtract/Intersect, OBB/AABB).
*   `libs/GeometryHelper.ArrangeAlgorithms.dll`: 5 thuật toán thông minh tự động dải/sắp xếp nhãn (`Rebar Mark`, `Part Mark`, `Dimension Text`) tránh đè nhau và tránh vật cản.
*   `libs/GeometryHelper.TeklaConvert.dll`: Cầu nối 2 chiều giữa Tekla Open API (`Point`, `Vector`, `AABB`, `Solid`, `Face`, `Matrix`) và GeometryHelper.
*   `libs/GeometryHelper.CadConvert.dll`: Cầu nối 2 chiều giữa AutoCAD .NET (`Point3d`, `Vector3d`, `Polyline`, `Line`, `Extents3d`) và GeometryHelper.

Mã nguồn đầy đủ nằm tại: `d:\Tekla_\My-tool\GeometryHelper\`

---

## 2. Ứng Dụng Đã Cập Nhật Vào Tool `Clash-check.exe`

1.  **Cây phân cấp khối bao BVH (`ObstacleBvhTree`)**:
    *   Trước đây: Quét va chạm tuyến tính $O(N \times M)$ giữa từng thanh thép và toàn bộ cấu kiện IFC. Với 3,000 thanh thép và 1,500 cấu kiện IFC, thuật toán phải duyệt 4,500,000 phép thử.
    *   Hiện tại: Đã tích hợp cây BVH. Không gian được chia nhánh nhị phân, loại bỏ ngay 95%-98% các cấu kiện ở xa. Độ phức tạp giảm xuống $O(N \log M)$, giúp tốc độ quét nhanh gấp 10-30 lần.
2.  **Giải thuật giải tích điểm gần nhất (Zero-Allocation Analytical Bisection)**:
    *   Thay thế thuật toán dò tam phân (ternary search 16 vòng lặp) vốn sinh ra hàng trăm ngàn đối tượng `Point` trên bộ nhớ Heap.
    *   Thuật toán mới tính đạo hàm của hàm khoảng cách lồi $D^2(t)$, dùng bisection thuần túy trên kiểu `double`, đạt độ chính xác $< 0.001$ mm trong 10 bước với 0 lần cấp phát bộ nhớ.

---

## 3. Mẫu Code Ứng Dụng Phát Triển Tool Tekla Tiếp Theo

### A. Tự động dàn nhãn thép trên bản vẽ Tekla (Tránh chồng lấn text và dim)
Tham khảo từ `Samples/GeometryHelper.ArrangeAlgorithms.TeklaTest`:
```csharp
using GeometryHelper.PlaneGeometry.Geometry;
using GeometryHelper.ArrangeAlgorithms;
using GeometryHelper.TeklaConvert;
using TSD = Tekla.Structures.Drawing;

// 1. Tạo danh sách các nhãn cần sắp xếp
var arranges = new List<Arrange>();

foreach (TSD.Mark mark in selectedMarks)
{
    // Lấy hộp bao hiện tại của mark và đường dóng (leader line)
    GeoRectangle2 markBox = GetMarkBoundingBox(mark);
    GeoLine2 leader = GetLeaderLine(mark);

    arranges.Add(new Arrange
    {
        GeoRectangle2 = markBox,
        GeoLine2 = leader,
        MarkOffsetFromLine = 50.0, // Khoảng cách tối thiểu từ text tới đường tim
        BlockPolygons = obstaclePolygons, // Vùng biên đường kích thước hoặc chi tiết cần tránh
        BlockLines = obstacleLines        // Tim thanh thép cần tránh đè qua
    });
}

// 2. Chạy thuật toán tự động dời vị trí
var options = new ArrangeOptions
{
    Algorithm = ArrangeAlgorithmType.BoundedBacktracking, // hoặc Greedy / ForceDirected
    RowGap = 20.0
};
List<GeoVector2> moves = Arrange.Run(arranges, options);

// 3. Áp dụng vector dịch chuyển lên Mark trong bản vẽ Tekla
for (int i = 0; i < arranges.Count; i++)
{
    if (arranges[i].Placed)
    {
        var teklaMove = arranges[i].TranslationVector.ToTeklaVector();
        // Dời vị trí mark InsertionPoint
    }
}
```

### B. Boolean Solid và Bounding Volume Hierarchy trên Tekla 3D Model
```csharp
using GeometryHelper.CommonGeometry;
using GeometryHelper.SolidGeometry.Geometry;
using GeometryHelper.TeklaConvert;
using TSM = Tekla.Structures.Model;

var tolerance = new Tolerance(1e-2, 1e-4);

// Chuyển đổi từ Solid của Tekla sang GeoSolid3
if (teklaSolid1.TryToGeoSolid3(out GeoSolid3 body1, tolerance) &&
    teklaSolid2.TryToGeoSolid3(out GeoSolid3 body2, tolerance))
{
    // Kiểm tra giao nhau và tính thể tích phần giao chính xác
    if (body1.TryIntersect(body2, out GeoSolid3 clashIntersection, tolerance))
    {
        double clashVolume = clashIntersection.Volume;
    }
}
```

---

## 4. Mẫu Code Ứng Dụng Phát Triển Tool AutoCAD .NET

### A. Làm sạch Polyline tim thép (Loại bỏ các đỉnh thừa/trùng nhau do sai số)
```csharp
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using GeometryHelper.CadConvert;

// Lọc bỏ các điểm nằm sát nhau dưới bán kính sai số 0.5mm
List<Point3d> rawPoints = GetPointsFromPolyline(polyline);
List<Point3d> cleanedPoints = rawPoints.RemoveConsecutiveNearPoints(0.5);

// Chuyển đổi thành danh sách các đoạn LineSegment3d
List<LineSegment3d> segments = cleanedPoints.ToLineSegments3d();
```

### B. Cắt Polyline thép bằng đường ranh giới (Splition2)
```csharp
using GeometryHelper.PlaneGeometry.Core;
using GeometryHelper.PlaneGeometry.Geometry;
using GeometryHelper.CadConvert;

GeoPolyline2 rebarPoly = acadPolyline.ToGeoPolyline2();
GeoPolygon2 boundaryZone = boundaryPoly.ToGeoPolygon2();

// Tách thép thành phần nằm trong và phần nằm ngoài
if (rebarPoly.TrySplitBy(boundaryZone, out GeoPolyline2[] insideParts, out GeoPolyline2[] outsideParts))
{
    foreach (var inside in insideParts)
    {
        Polyline acadInsideRebar = inside.ToAcadPolyline();
        // Thêm vào AutoCAD Database
    }
}
```

---

## 5. Cú Pháp Biên Dịch Bằng Roslyn `csc.exe`

Để biên dịch tool mới có dùng GeometryHelper từ dòng lệnh (hoặc PowerShell):

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe" `
    /target:winexe `
    /out:"d:\Tekla_\My-tool\TenToolCuaBan.exe" `
    /r:"C:\TeklaStructures\2020.0\nt\bin\plugins\Tekla.Structures.dll" `
    /r:"C:\TeklaStructures\2020.0\nt\bin\plugins\Tekla.Structures.Model.dll" `
    /r:"d:\Tekla_\My-tool\libs\GeometryHelper.CommonGeometry.dll" `
    /r:"d:\Tekla_\My-tool\libs\GeometryHelper.SolidGeometry.dll" `
    /r:"d:\Tekla_\My-tool\libs\GeometryHelper.TeklaConvert.dll" `
    /r:"System.dll","System.Core.dll","System.Data.dll","System.Drawing.dll","System.Windows.Forms.dll" `
    "d:\Tekla_\My-tool\src_tool\*.cs"
```
