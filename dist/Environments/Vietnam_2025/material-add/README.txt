================================================================================
  TCVN MATERIAL IMPORTER - Hướng dẫn sử dụng
  Phiên bản: 1.0
  Tác giả: BIM Vietnam Automation
================================================================================

1. GIỚI THIỆU
   -----------
   Macro C# với giao diện đồ họa (WinForms) giúp tự động import vật liệu 
   theo tiêu chuẩn Việt Nam vào Material Catalog (MATDB.BIN) của Tekla Structures.

   Tiêu chuẩn hỗ trợ:
   - TCVN 5574:2018 - Bê tông (B10 đến B60, M150 đến M500)
   - TCVN 1651:2018 - Thép cốt thép (CB240-T, CB300-V, CB400-V, CB500-V)
   - TCVN 5709      - Thép kết cấu (CT3)
   - JIS G3101/3106 - Thép kết cấu (SS400, SM490, SM520)
   - JIS G3112      - Thép cốt thép (SD295, SD390, SD490)
   - GB/T 700/1591  - Thép kết cấu (Q235, Q345, Q355)
   - EN 10025       - Thép kết cấu (S235JR, S275JR, S355JR)
   - ASTM           - Thép kết cấu & cốt thép (A36, A572-Gr50, A615-Gr60)

2. CÀI ĐẶT
   --------
   Cách 1: Copy thủ công
     - Copy thư mục "material-add" vào:
       C:\TeklaStructures\<version>\Environments\Vietnam\
     - Hoặc copy file TCVN_Material_Importer.cs vào thư mục macros:
       C:\TeklaStructures\<version>\Environments\Vietnam\General\macros\modeling\

   Cách 2: Dùng file install_vietnam_environment.bat (đã có sẵn)
     - Chạy file .bat, macro sẽ được cài đặt tự động.

3. CÁCH SỬ DỤNG
   -------------
   Bước 1: Mở Tekla Structures với Environment Vietnam
   Bước 2: Mở model bất kỳ (macro cần kết nối đến model đang hoạt động)
   Bước 3: Vào menu: Applications > Macros > Run...
   Bước 4: Chọn file TCVN_Material_Importer.cs
   Bước 5: Giao diện sẽ hiện ra với danh sách vật liệu TCVN
   Bước 6: Tick chọn vật liệu cần import (hoặc dùng nút lọc nhanh)
   Bước 7: Nhấn nút [Import vào Tekla]
   Bước 8: Xác nhận → Chờ import hoàn tất
   Bước 9: Kiểm tra: File > Catalogs > Material Catalog

4. TÍNH NĂNG
   ---------
   - Giao diện WinForms với bảng danh sách vật liệu có màu sắc phân loại
   - Nút lọc nhanh: Chỉ Bê tông / Chỉ Cốt thép / Chỉ Thép KC
   - Tùy chọn: Bỏ qua vật liệu đã tồn tại hoặc ghi đè
   - Thanh tiến trình (progress bar)
   - Log chi tiết từng vật liệu (thêm mới / cập nhật / bỏ qua / lỗi)
   - Tự động phát hiện vật liệu trùng tên

5. DANH SÁCH VẬT LIỆU
   --------------------
   [Bê tông - 19 loại]
     B10, B15, B20, B22.5, B25, B30, B35, B40, B45, B50, B55, B60
     M150, M200, M250, M300, M350, M400, M500

   [Thép cốt thép - 8 loại]
     CB240-T, CB300-V, CB400-V, CB500-V
     SD295, SD390, SD490
     A615-Gr60

   [Thép kết cấu - 12 loại]
     SS400, SM490, SM520, CT3
     Q235, Q345, Q355
     S235JR, S275JR, S355JR
     A36, A572-Gr50

   Tổng cộng: 39 vật liệu

6. LƯU Ý
   ------
   - Macro cần Tekla đang mở và có model đang hoạt động
   - Sau khi import, mở lại Material Catalog để thấy vật liệu mới
   - Các thuộc tính cơ học (Fy, Fu) không lưu trong MATDB.BIN
   - Nếu cần thêm vật liệu mới, sửa hàm InitializeMaterialData() trong file .cs
   - Backup file MATDB.BIN trước khi chạy nếu lo ngại mất dữ liệu

================================================================================
