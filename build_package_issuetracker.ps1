Add-Type -AssemblyName System.Drawing

$distDir = "dist\IssueTracker_Tekla2020"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

# 1. Tạo Icon IssueTracker 40x40
$iconPath = Join-Path $distDir "IssueTracker-40x40.png"
$bmp = New-Object System.Drawing.Bitmap(40, 40)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Background rounded box
$brushBg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(15, 23, 42)) # Slate 900
$g.FillRectangle($brushBg, 0, 0, 40, 40)

# Camera / Target icon symbol
$penCamera = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(56, 189, 248), 2) # Sky blue
$g.DrawRectangle($penCamera, 6, 12, 28, 20)
$g.FillRectangle((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(56, 189, 248))), 14, 7, 12, 5)

# Center lens
$penLens = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(239, 68, 68), 2) # Red alert
$g.DrawEllipse($penLens, 14, 16, 12, 12)

# Check badge
$checkPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(34, 197, 94), 2)
$g.DrawLine($checkPen, 17, 22, 20, 25)
$g.DrawLine($checkPen, 20, 25, 25, 18)

$g.Dispose()
$bmp.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

# 2. Sao chép binaries từ release
$filesToCopy = @(
    "AppIssueTracker.exe",
    "AppIssueTracker.exe.config",
    "IssueTracker.exe",
    "EPPlus.dll",
    "EPPlus.Interfaces.dll",
    "EPPlus.System.Drawing.dll",
    "Microsoft.IO.RecyclableMemoryStream.dll",
    "Newtonsoft.Json.dll",
    "Tekla.Structures.dll",
    "Tekla.Structures.Model.dll",
    "Tekla.Structures.Catalogs.dll",
    "Tekla.Structures.Datatype.dll",
    "Tekla.Structures.Plugins.dll"
)

foreach ($f in $filesToCopy) {
    $src = Join-Path "release" $f
    if (Test-Path $src) {
        Copy-Item $src (Join-Path $distDir $f) -Force
    }
}

$extraTekla = @("Tekla.Structures.Drawing.dll", "Tekla.Structures.Dialog.dll")
foreach ($f in $extraTekla) {
    $src = Join-Path "dist\Overlap" $f
    if (Test-Path $src) {
        Copy-Item $src (Join-Path $distDir $f) -Force
    }
}

# 3. Tạo CaiDat_Ribbon.bat
$batContent = @"
@echo off
chcp 65001 >nul
echo ======================================================================
echo    DANG KY CONG CU BIM QA/QC ISSUE TRACKER VAO TEKLA STRUCTURES
echo ======================================================================
powershell -ExecutionPolicy Bypass -File "%~dp0CaiDat_Ribbon.ps1"
echo.
pause
"@
[System.IO.File]::WriteAllText((Join-Path $distDir "CaiDat_Ribbon.bat"), $batContent, [System.Text.Encoding]::UTF8)

# 4. Sao chép và tinh chỉnh CaiDat_Ribbon.ps1 từ Overlap
$overlapPs1 = Join-Path "dist\Overlap" "CaiDat_Ribbon.ps1"
if (Test-Path $overlapPs1) {
    $c = [System.IO.File]::ReadAllText($overlapPs1, [System.Text.Encoding]::UTF8)
    $c = $c.Replace("Overlap.exe", "AppIssueTracker.exe")
    $c = $c.Replace("Overlap-40x40.png", "IssueTracker-40x40.png")
    $c = $c.Replace("Bim.Commands.OverlapChecker", "Bim.Commands.IssueTracker")
    $c = $c.Replace("Tekla Overlap &amp; Duplicate Checker Pro", "BIM QA/QC Issue Tracker &amp; Excel Exporter")
    $c = $c.Replace("Overlap-Checker", "Issue-Tracker")
    [System.IO.File]::WriteAllText((Join-Path $distDir "CaiDat_Ribbon.ps1"), $c, [System.Text.Encoding]::UTF8)
}

# 5. Hướng dẫn sử dụng
$guide = @"
========================================================================================
   HƯỚNG DẪN SỬ DỤNG: BIM QA/QC ISSUE TRACKER & EXCEL SNAGGING REPORT
   Dành cho Kỹ sư Kiểm tra Mô hình (QC/Lead) và Nhân viên Mô hình (Modeler/Detailer)
========================================================================================

I. GIỚI THIỆU
Ứng dụng được thiết kế chuyên biệt cho việc kiểm tra chất lượng mô hình BIM (Tekla Structures,
AutoCAD, Navisworks, Revit...). Cho phép:
- Chụp ảnh màn hình trực tiếp vị trí lỗi bằng công cụ Snipping Tool tích hợp.
- Khoanh mây (Revision Cloud), vẽ mũi tên, đóng hộp đỏ, ghi chú Text trực quan ngay trên ảnh.
- Tự động bắt mã ID cấu kiện Tekla đang chọn, hỗ trợ nút bấm Zoom trực tiếp đến cấu kiện.
- Quản lý quy trình Before / After: Chụp ảnh hiện trạng lỗi và nhân viên chụp ảnh đối chứng sau khi sửa.
- Xuất file Báo cáo Excel chuyên nghiệp (EPPlus): Tự động nhúng ảnh độ nét cao vào ô, kẻ khung, 
  thống kê thẻ KPI dashboard (Tổng lỗi, Chưa sửa, Đang sửa, Đã sửa, Tỷ lệ hoàn thành),
  canh lề in chuẩn khổ A4 Landscape.

II. CÁC TÍNH NĂNG VÀ NÚT CHỨC NĂNG CHÍNH
1. [💾 Lưu]: Lưu toàn bộ danh sách issue hiện tại thành file dự án (.bimissue) định dạng JSON.
2. [📂 Mở]: Mở lại file dự án đã lưu để tiếp tục cập nhật tiến độ kiểm tra.
3. [📷 Chụp issue hiện tại]:
   - Tự động ẩn cửa sổ ứng dụng để không che màn hình.
   - Hiện màn hình mờ kéo chuột chụp vùng mong muốn (có hiển thị kích thước px thời gian thực).
   - Tự động mở trình Markup để vẽ khoanh mây đỏ, mũi tên chỉ dẫn, gõ chữ ghi chú.
   - Nhấn "Hoàn Tất" -> Tự động thêm dòng Issue mới vào bảng!
4. [📊 Xuất Excel]:
   - Tạo file Excel định dạng .xlsx hoàn chỉnh.
   - Tự động mở file Excel ngay khi xuất xong để kiểm tra trước khi gửi nhân viên.
5. [🔄 Làm mới]: Tạo một phiên làm việc mới hoàn toàn sạch sẽ.
6. [➕ Thêm issue]: Tạo thủ công dòng issue mới.
7. [❌ Xóa issue]: Xóa dòng issue đang chọn khỏi danh sách.
8. [🎯 Zoom Tekla]: Nhấp đúp vào dòng hoặc bấm nút Zoom để Tekla tự động chọn và phóng to cấu kiện có ID tương ứng!

III. QUY TRÌNH PHỐI HỢP GIỮA NGƯỜI CHECK VÀ NHÂN VIÊN SỬA
Bước 1: Trưởng nhóm / QC mở mô hình Tekla, kiểm tra lỗi (đụng độ thép, sai profile, hở liên kết...).
Bước 2: Bấm [📷 Chụp issue hiện tại] -> Kéo vùng lỗi -> Khoanh mây / ghi chú -> Bấm [Hoàn Tất].
Bước 3: Nhập Tên issue, Hướng dẫn sửa, Tên nhân viên sửa (Assignee).
Bước 4: Bấm [📊 Xuất Excel] -> Gửi file Excel này cho nhân viên qua Zalo / Teams / Email.
Bước 5: Nhân viên mở file Excel hoặc mở ứng dụng -> Sửa mô hình -> Bấm nút [📷 Chụp] ở cột "Ảnh đã sửa" để lưu bằng chứng đối chứng đã sửa xong.
Bước 6: Trạng thái tự động chuyển thành "Resolved" (Đã sửa) với màu xanh lá cây -> Báo cáo hoàn tất!

IV. TRIỂN KHAI VÀO TEKLA STRUCTURES
- Cách 1: Chạy trực tiếp file "AppIssueTracker.exe" hoặc "IssueTracker.exe".
- Cách 2: Nhấp chuột phải vào file "CaiDat_Ribbon.bat" -> Chọn "Run as administrator" để đăng ký nút bấm lên thanh Ribbon của Tekla Structures 2020.

========================================================================================
"@
[System.IO.File]::WriteAllText((Join-Path $distDir "Huong_Dan_Su_Dung.txt"), $guide, [System.Text.Encoding]::UTF8)

# 6. Nén thành file ZIP
$zipPath = "dist\IssueTracker_Tekla2020.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$distDir\*" -DestinationPath $zipPath -Force

# 7. Đẩy sang C:\tekla\My-tool nếu thư mục tồn tại
$cMyTool = "C:\tekla\My-tool"
if (Test-Path $cMyTool) {
    Copy-Item "$distDir\*" $cMyTool -Recurse -Force
    Write-Host "[THÀNH CÔNG] Đã đồng bộ sang $cMyTool" -ForegroundColor Green
}

Write-Host "Hoàn tất đóng gói: $zipPath" -ForegroundColor Cyan
Write-Host "Kích thước ZIP: $((Get-Item $zipPath).Length / 1MB) MB"
