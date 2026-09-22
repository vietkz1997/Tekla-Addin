# Tekla-Addin - Structural Modeling & Clash Detection Toolkit

Bộ công cụ mở rộng (Add-in / External Tools) dành cho **Tekla Structures**, cung cấp các giải pháp tự động hóa mô hình kết cấu, phân tích hình học nâng cao và kiểm tra va chạm vật lý siêu tốc giữa cốt thép (Rebar) và cấu kiện tham chiếu IFC.

---

## 🚀 Các Phân Hệ Chính (Core Modules)

### 1. 🔍 Clash-check (Rebar vs IFC Hard Clash Detector)
Công cụ kiểm tra va chạm độ chính xác cao giữa cốt thép Tekla Structures và các mô hình IFC tham chiếu (ReferenceModelObject):
- **Lõi hình học 3D Brep & Exact Geometry:** Tích hợp bộ thư viện GeometryHelper kết hợp với Xbim.Geometry.Engine cho phép giải mã chính xác hình dạng 3D của các cấu kiện kết cấu thép, bản mã, bu lông và dầm/cột phức tạp trong IFC.
- **Tối ưu hóa không gian đa cấp:**
  - **3D Spatial R-Tree & BVH (Bounding Volume Hierarchy):** Giảm thiểu tối đa phạm vi tìm kiếm hình học từ O(N x M) xuống còn O(N log M).
  - **Sweep-and-Prune & Envelope Filter:** Tự động phát hiện và loại bỏ các vỏ bao tổng thể (compound envelopes) chứa nhiều chi tiết lá, triệt tiêu hoàn toàn hiện tượng báo va chạm giả (false positives) trong khoảng không giữa các bản cánh dầm/cột.
- **Bộ lọc thông minh tiền xử lý (Pre-filtering & Skip Names):**
  - Đọc danh sách cấu kiện phụ trợ cần lược bỏ trực tiếp từ cấu hình giao diện (Bolt assembly, SAFETY_BAR, LUG, LADDER, v.v.).
  - Bỏ qua giải mã hình học ngay từ khâu nạp file IFC thông qua IfcConvertOptions.AddSkipNames().
  - Tra cứu metadata O(1) để lập tức bỏ qua hơn 15.000+ cấu kiện bu lông/phụ trợ mà không cần gọi Tekla COM IPC, giảm hơn 90% thời gian xử lý tổng thể.
- **Xử lý đa luồng song song (Multi-Threaded Parallel Processing):** Tận dụng tối đa số nhân CPU (Parallel.ForEach) cho giai đoạn tính toán va chạm phân đoạn hình học.
- **Tương tác trực quan 3D với Tekla:**
  - Tự động điều hướng góc nhìn camera Tekla (Tekla.Structures.Model.UI.View) tới vị trí va chạm khi chọn dòng kết quả.
  - Highlight màu sắc trực quan (đỏ/vàng) trên mô hình 3D.
  - Xuất báo cáo danh sách va chạm chi tiết (tọa độ va chạm, độ lẹm Overlap mm, mã cấu kiện IFC, thông số thép).

### 2. 📐 Rebar-error (Rebar Modeling QA/QC Inspector)
Công cụ kiểm tra chất lượng mô hình cốt thép tự động:
- Phân tích hình học đường tâm cốt thép (Centerline geometry & segments).
- Rà soát các lỗi tạo hình: bán kính uốn không đạt chuẩn, chiều dài neo/nối không hợp lệ, phân đoạn cốt thép trùng lặp hoặc tự cắt chính mình.
- Báo cáo và định vị tức thời vị trí lỗi trong mô hình kết cấu.

### 3. 🛠️ My-tool (Ribbon & Launcher Hub)
- Giao diện trung tâm tích hợp trực tiếp vào thanh công cụ Tekla Structures hoặc chạy độc lập.
- Quản lý và khởi chạy nhanh các tính năng mở rộng hỗ trợ kỹ sư kết cấu & detailer.

---

## 📁 Cấu Trúc Dự Án (Repository Structure)

```text
Tekla-Addin/
│
├── .gitignore                      # Cấu hình lọc file rác biên dịch, IDE và cache
├── README.md                       # Tài liệu hướng dẫn dự án
├── TeklaAddin.slnx                 # Solution format XML thế hệ mới
│
├── src/                            # Thư mục mã nguồn (Source Code)
│   ├── ClashCheck/                 # Module kiểm tra va chạm Rebar vs IFC
│   │   ├── ClashCheck.csproj       # Project .NET Framework 4.8 x64
│   │   ├── ClashDetector.cs        # Lõi phát hiện va chạm, BVH Tree & lọc cấu kiện
│   │   ├── IfcGeometryBridge.cs    # Cầu nối trích xuất hình học IFC qua GeometryHelper
│   │   ├── XbimGeometryManager.cs  # Quản lý metadata & cache hình học IFC/xBIM
│   │   ├── MainForm.cs             # Giao diện điều khiển, danh sách kết quả & 3D view
│   │   └── ...
│   │
│   ├── RebarError/                 # Module kiểm tra lỗi tạo hình cốt thép
│   │   ├── RebarError.csproj       # Project .NET Framework 4.8 x64
│   │   ├── RebarGeometryAnalyzer.cs# Phân tích hình học & đường tâm cốt thép
│   │   ├── RebarErrorInfo.cs       # Cấu trúc dữ liệu lỗi cốt thép
│   │   └── ...
│   │
│   └── MyTool/                     # Module thanh công cụ Launcher / SplitSlab
│       ├── MyTool.csproj           # Project .NET Framework 4.8 x64
│       └── BimCommands.Tekla.MyTool/
│
├── libs/                           # Thư viện phụ thuộc bên thứ ba (Dependencies)
│   ├── GeometryHelper/             # Bộ DLL lõi hình học không gian đa giác & IFC/Tekla
│   ├── Xbim/                       # Bộ DLL xBIM đọc mô hình IFC & Geometry Engine native
│   └── Framework/                  # Các assembly Microsoft.Extensions, System.*
│
├── release/                        # Thư mục chứa gói phát hành sẵn sàng chạy
│   ├── Clash-check.exe             # File thực thi công cụ kiểm tra va chạm
│   ├── MyTool.exe                  # File thực thi bộ công cụ tổng hợp
│   ├── Rebar-error.exe             # File thực thi kiểm tra lỗi cốt thép
│   └── *.dll                       # Các DLL phụ thuộc cần thiết khi triển khai
│
└── docs/                           # Tài liệu kỹ thuật
    └── GEOMETRY_HELPER_GUIDE.md
```

---

## 💻 Yêu Cầu Hệ Thống (Prerequisites)

- **Hệ điều hành:** Windows 10 / Windows 11 (x64).
- **Phần mềm:** Tekla Structures 2020 trở lên (đã kiểm tra tương thích Tekla Structures 2020 / 2021 / 2022).
- **Nền tảng phát triển:**
  - .NET Framework 4.8 Developer Pack.
  - Visual Studio 2019 / 2022 (với Workload *.NET desktop development*) hoặc .NET SDK (hỗ trợ dotnet build).
- **Thư viện phụ thuộc:**
  - Microsoft Visual C++ 2015-2022 Redistributable (x64) (yêu cầu bởi Xbim.Geometry.Engine64.dll).
  - Tekla Structures Open API (Tekla.Structures.dll, Tekla.Structures.Model.dll).

---

## 🔨 Hướng Dẫn Biên Dịch (Build Instructions)

### Cách 1: Sử dụng Command Line (.NET CLI)

1. Mở PowerShell hoặc Terminal tại thư mục gốc của repository:
   ```powershell
   cd c:\Users\BIM\Documents\Github\Tekla-Addin
   ```
2. Biên dịch toàn bộ Solution ở chế độ Release:
   ```powershell
   # Mặc định (Tekla 2025):
   dotnet build TeklaAddin.slnx -c Release

   # Hoặc chuyển đổi linh hoạt sang phiên bản khác (2020, 2025, 2026):
   dotnet build TeklaAddin.slnx -c Release /p:TeklaVersion=2020
   dotnet build TeklaAddin.slnx -c Release /p:TeklaVersion=2026
   ```
   *(Hoặc biên dịch từng module riêng lẻ, ví dụ: dotnet build src/ClashCheck/ClashCheck.csproj -c Release)*
3. Các file thực thi (Clash-check.exe, MyTool.exe, Rebar-error.exe) và các DLL cần thiết sẽ được tự động xuất ra thư mục `release/`.

### Cách 2: Sử dụng Visual Studio 2022

1. Mở file `TeklaAddin.slnx` trong Visual Studio 2022 (v17.10 trở lên).
2. Đổi phiên bản Tekla mong muốn trong file `Directory.Build.props` (dòng `<TeklaVersion>2025</TeklaVersion>`).
3. Chọn menu **Build > Build Solution** (hoặc nhấn Ctrl + Shift + B).

---

## 📖 Hướng Dẫn Sử Dụng (Quick Start)

1. **Khởi động Tekla Structures:** Mở mô hình công trình cần kiểm tra.
2. **Chạy công cụ:** Chạy file elease/Clash-check.exe (hoặc khởi chạy từ MyTool.exe hoặc Rebar-error.exe).
3. **Cấu hình phạm vi kiểm tra:**
   - **Thép cần kiểm tra:** Chọn các thanh thép trực tiếp trên mô hình Tekla (hoặc chọn toàn bộ mô hình).
   - **Mô hình IFC:** Chọn quét tự động các file IFC giao cắt với vùng thép (AutoSpatialAllIfc) hoặc chọn file IFC cụ thể.
   - **Dung sai va chạm (Tolerance):** Đặt độ lẹm tối thiểu để tính là va chạm (mặc định: 1.0 mm).
   - **Khoảng hở an toàn (Clearance):** Đặt khoảng cách an toàn mong muốn (mặc định: 0.0 mm).
   - **Lược bỏ cấu kiện IFC:** Tích chọn ☑ Lược bỏ cấu kiện IFC: và tùy chỉnh danh sách từ khóa (ví dụ: Bolt assembly, SAFETY_BAR, LUG, LADDER) để tối ưu hóa thời gian tính toán.
4. **Bấm "Quét Va Chạm":** Theo dõi tiến trình phân tích trên thanh trạng thái.
5. **Kiểm tra kết quả:** Nhấp chuột vào từng dòng va chạm trên bảng để camera Tekla tự động zoom và highlight cấu kiện va chạm trên màn hình 3D.

---

## 📜 Bản Quyền & Tác Quyền (License)

Dự án được xây dựng và phát triển nội bộ phục vụ công tác tối ưu hóa quy trình BIM / Detailing kết cấu thép & bê tông cốt thép trên nền tảng Tekla Structures Open API.
