Add-Type -AssemblyName System.Drawing

$distDir = "dist\RevitQtoTool"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

$binDir = Join-Path $distDir "Bin"
if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir -Force | Out-Null
}

$revit2026Dir = Join-Path $distDir "Revit_2026"
if (-not (Test-Path $revit2026Dir)) {
    New-Item -ItemType Directory -Path $revit2026Dir -Force | Out-Null
}

$revit2024Dir = Join-Path $distDir "Revit_2024"
if (-not (Test-Path $revit2024Dir)) {
    New-Item -ItemType Directory -Path $revit2024Dir -Force | Out-Null
}

# 1. Tạo Icon RevitQtoTool 40x40
$iconPath = Join-Path $distDir "RevitQtoTool-40x40.png"
$bmp = New-Object System.Drawing.Bitmap(40, 40)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Background dark navy
$brushBg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(30, 41, 59))
$g.FillRectangle($brushBg, 0, 0, 40, 40)

# Concrete building blocks (Cyan & Orange accents)
$penCube = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(56, 189, 248), 2)
$g.DrawRectangle($penCube, 7, 10, 16, 22)

$penCube2 = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(245, 158, 11), 2)
$g.DrawRectangle($penCube2, 17, 6, 16, 22)

# Excel Green badge check
$brushBadge = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(34, 197, 94))
$g.FillEllipse($brushBadge, 22, 22, 14, 14)

$penCheck = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 2)
$g.DrawLine($penCheck, 25, 29, 28, 32)
$g.DrawLine($penCheck, 28, 32, 33, 26)

$g.Dispose()
$bmp.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Copy-Item $iconPath (Join-Path $binDir "RevitQtoTool-40x40.png") -Force
Copy-Item $iconPath (Join-Path $revit2026Dir "RevitQtoTool-40x40.png") -Force
Copy-Item $iconPath (Join-Path $revit2024Dir "RevitQtoTool-40x40.png") -Force

# 2. Sao chép binaries cho Revit 2026 (.NET 8.0)
$releaseNet8 = "src\RevitQtoTool\bin\Release\net8.0-windows"
if (Test-Path $releaseNet8) {
    Write-Host "-> Sao chep binaries cho Revit 2026 / 2025 (.NET 8.0)..." -ForegroundColor Cyan
    Get-ChildItem -Path $releaseNet8 -Filter "*.*" | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $revit2026Dir $_.Name) -Force
    }
    # Copy file thực thi độc lập .exe ra thư mục gốc dist
    Copy-Item (Join-Path $releaseNet8 "RevitQtoTool.exe") (Join-Path $distDir "RevitQtoTool.exe") -Force
    if (Test-Path (Join-Path $releaseNet8 "RevitQtoTool.dll")) {
        Copy-Item (Join-Path $releaseNet8 "RevitQtoTool.dll") (Join-Path $distDir "RevitQtoTool.dll") -Force
    }
    if (Test-Path (Join-Path $releaseNet8 "RevitQtoTool.runtimeconfig.json")) {
        Copy-Item (Join-Path $releaseNet8 "RevitQtoTool.runtimeconfig.json") (Join-Path $distDir "RevitQtoTool.runtimeconfig.json") -Force
    }
    if (Test-Path (Join-Path $releaseNet8 "RevitQtoTool.deps.json")) {
        Copy-Item (Join-Path $releaseNet8 "RevitQtoTool.deps.json") (Join-Path $distDir "RevitQtoTool.deps.json") -Force
    }
    if (Test-Path (Join-Path $releaseNet8 "ClosedXML.dll")) {
        Copy-Item (Join-Path $releaseNet8 "ClosedXML.dll") (Join-Path $distDir "ClosedXML.dll") -Force
        Copy-Item (Join-Path $releaseNet8 "DocumentFormat.OpenXml.dll") (Join-Path $distDir "DocumentFormat.OpenXml.dll") -Force
        Copy-Item (Join-Path $releaseNet8 "ExcelNumberFormat.dll") (Join-Path $distDir "ExcelNumberFormat.dll") -Force
        Copy-Item (Join-Path $releaseNet8 "Irony.dll") (Join-Path $distDir "Irony.dll") -Force
        Copy-Item (Join-Path $releaseNet8 "SixLabors.Fonts.dll") (Join-Path $distDir "SixLabors.Fonts.dll") -Force
        Copy-Item (Join-Path $releaseNet8 "XLParser.dll") (Join-Path $distDir "XLParser.dll") -Force
    }
}

# 3. Sao chép binaries cho Revit 2020-2024 (.NET Framework 4.8)
$releaseNet48 = "src\RevitQtoTool\bin\Release\net48"
if (Test-Path $releaseNet48) {
    Write-Host "-> Sao chep binaries cho Revit 2020-2024 (.NET Framework 4.8)..." -ForegroundColor Cyan
    Get-ChildItem -Path $releaseNet48 -Filter "*.*" | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $revit2024Dir $_.Name) -Force
        Copy-Item $_.FullName (Join-Path $binDir $_.Name) -Force
    }
    if (Test-Path (Join-Path $revit2024Dir "RevitQtoTool.exe")) {
        Copy-Item (Join-Path $revit2024Dir "RevitQtoTool.exe") (Join-Path $revit2024Dir "RevitQtoTool.dll") -Force
    }
}

# 4. Tạo CaiDat.bat
$batContent = @"
@echo off
chcp 65001 >nul
title Cai dat Add-in Revit QTO va Clash Pro cho Revit 2026
echo ======================================================================
echo    BAT DAU CAI DAT ADD-IN REVIT QTO & CLASH PRO (HO TRO REVIT 2026)
echo ======================================================================
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0CaiDat_Addin.ps1"
echo.
pause
"@
[System.IO.File]::WriteAllText((Join-Path $distDir "CaiDat.bat"), $batContent, [System.Text.Encoding]::UTF8)

# 5. Tạo CaiDat_Addin.ps1 thông minh hỗ trợ Revit 2026
$ps1Lines = @(
    "[Console]::OutputEncoding = [System.Text.Encoding]::UTF8",
    "Clear-Host",
    "Write-Host '==========================================================' -ForegroundColor Cyan",
    "Write-Host '   CAI DAT ADD-IN REVIT QTO & CLASH PRO (HO TRO REVIT 2026)' -ForegroundColor Green",
    "Write-Host '==========================================================' -ForegroundColor Cyan",
    "",
    "`$CurrentDir = Split-Path -Parent `$MyInvocation.MyCommand.Path",
    "",
    "`$RevitVersions = @('2026', '2025', '2024', '2023', '2022', '2021', '2020')",
    "`$InstalledCount = 0",
    "",
    "foreach (`$ver in `$RevitVersions) {",
    "    `$AddinDir = Join-Path `$env:APPDATA ""Autodesk\Revit\Addins\`$ver""",
    "    ",
    "    # Tu dong tao thu muc Addins cho Revit 2026 neu may dang dung Revit 2026",
    "    if (`$ver -eq '2026' -and -not (Test-Path `$AddinDir)) {",
    "        `$revit2026Exe = ""C:\Program Files\Autodesk\Revit 2026\Revit.exe""",
    "        if (Test-Path `$revit2026Exe) {",
    "            New-Item -ItemType Directory -Path `$AddinDir -Force | Out-Null",
    "        }",
    "    }",
    "    ",
    "    if (Test-Path `$AddinDir) {",
    "        Write-Host ""-> Phat hien Autodesk Revit `$ver..."" -ForegroundColor Yellow",
    "        `$ManifestPath = Join-Path `$AddinDir 'RevitQtoTool.addin'",
    "        `$guid = [guid]::NewGuid().ToString()",
    "        ",
    "        # Chon assembly phu hop: Revit 2025-2026 dung .NET 8.0, Revit 2020-2024 dung .NET 4.8",
    "        if ([int]`$ver -ge 2025) {",
    "            `$TargetDll = Join-Path `$CurrentDir 'Revit_2026\RevitQtoTool.dll'",
    "            Write-Host ""   [NET 8.0] Su dung ban bien dich .NET 8 cho Revit `$ver"" -ForegroundColor Gray",
    "        } else {",
    "            `$TargetDll = Join-Path `$CurrentDir 'Revit_2024\RevitQtoTool.dll'",
    "            Write-Host ""   [NET 4.8] Su dung ban bien dich .NET 4.8 cho Revit `$ver"" -ForegroundColor Gray",
    "        }",
    "        ",
    "        if (-not (Test-Path `$TargetDll)) {",
    "            `$TargetDll = Join-Path `$CurrentDir 'RevitQtoTool.dll'",
    "        }",
    "        ",
    "        `$xml = @""",
    "<?xml version=""1.0"" encoding=""utf-8""?>",
    "<RevitAddIns>",
    "  <AddIn Type=""Application"">",
    "    <Name>Revit QTO &amp; Clash Pro</Name>",
    "    <Assembly>`$TargetDll</Assembly>",
    "    <FullClassName>RevitQtoTool.AppRibbon</FullClassName>",
    "    <ClientId>`$guid</ClientId>",
    "    <VendorId>BIM_VN</VendorId>",
    "    <VendorDescription>BIM Structural Suite</VendorDescription>",
    "  </AddIn>",
    "</RevitAddIns>",
    """@",
    "        [System.IO.File]::WriteAllText(`$ManifestPath, `$xml, [System.Text.Encoding]::UTF8)",
    "        Write-Host ""   [OK] Da kich hoat thanh cong cho Revit `$ver tai: `$ManifestPath"" -ForegroundColor Green",
    "        `$InstalledCount++",
    "    }",
    "}",
    "",
    "# Neu chua phat hien thu muc nao, ho tro cai dat thu cong vao Revit 2026",
    "if (`$InstalledCount -eq 0) {",
    "    Write-Host ""[LUA CHON] Khong tim thay thu muc Revit san co trong AppData."" -ForegroundColor Yellow",
    "    Write-Host ""Ban co muon tao va kich hoat Add-in cho Revit 2026 khong? (Y/N): "" -NoNewline -ForegroundColor White",
    "    `$choice = Read-Host",
    "    if (`$choice -eq 'Y' -or `$choice -eq 'y' -or [string]::IsNullOrEmpty(`$choice)) {",
    "        `$AddinDir2026 = Join-Path `$env:APPDATA ""Autodesk\Revit\Addins\2026""",
    "        New-Item -ItemType Directory -Path `$AddinDir2026 -Force | Out-Null",
    "        `$ManifestPath = Join-Path `$AddinDir2026 'RevitQtoTool.addin'",
    "        `$TargetDll = Join-Path `$CurrentDir 'Revit_2026\RevitQtoTool.dll'",
    "        `$guid = [guid]::NewGuid().ToString()",
    "        `$xml = @""",
    "<?xml version=""1.0"" encoding=""utf-8""?>",
    "<RevitAddIns>",
    "  <AddIn Type=""Application"">",
    "    <Name>Revit QTO &amp; Clash Pro</Name>",
    "    <Assembly>`$TargetDll</Assembly>",
    "    <FullClassName>RevitQtoTool.AppRibbon</FullClassName>",
    "    <ClientId>`$guid</ClientId>",
    "    <VendorId>BIM_VN</VendorId>",
    "    <VendorDescription>BIM Structural Suite</VendorDescription>",
    "  </AddIn>",
    "</RevitAddIns>",
    """@",
    "        [System.IO.File]::WriteAllText(`$ManifestPath, `$xml, [System.Text.Encoding]::UTF8)",
    "        Write-Host ""[OK] Da tao va kich hoat Add-in cho Autodesk Revit 2026!"" -ForegroundColor Green",
    "        `$InstalledCount++",
    "    }",
    "}",
    "",
    "Write-Host '----------------------------------------------------------' -ForegroundColor Gray",
    "if (`$InstalledCount -gt 0) {",
    "    Write-Host "" CAI DAT THANH CONG CHO `$InstalledCount PHIEN BAN REVIT!"" -ForegroundColor Green",
    "    Write-Host ""Khoi dong Autodesk Revit 2026 -> Tab 'BIM QTO PRO' se xuat hien tren thanh Ribbon!"" -ForegroundColor White",
    "} else {",
    "    Write-Host ""[HUONG DAN] De cai dat thu cong, hay copy file 'RevitQtoTool.addin' vao:"" -ForegroundColor Yellow",
    "    Write-Host ""%APPDATA%\Autodesk\Revit\Addins\2026\"" -ForegroundColor Cyan",
    "}",
    "Write-Host '==========================================================' -ForegroundColor Cyan"
)
[System.IO.File]::WriteAllLines((Join-Path $distDir "CaiDat_Addin.ps1"), $ps1Lines, [System.Text.Encoding]::UTF8)

# 6. Tạo Huong_Dan_Su_Dung.txt
$guideLines = @(
    "========================================================================================",
    "   HUONG DAN SU DUNG: REVIT QTO & CLASH CHECK PRO (BOQ EXPORTER)",
    "   HO TRO TOAN DIEN AUTODESK REVIT 2026 (.NET 8.0) VA REVIT 2020-2025",
    "   Dành cho Kỹ sư BIM, Kỹ sư Kết cấu và Kỹ sư Dự toán (QS) các Tổng thầu Việt Nam",
    "========================================================================================",
    "",
    "I. DAC DIEM KINH NGHIEM REVIT 2026 (.NET 8.0)",
    "- Tu phien ban Revit 2025 va Revit 2026, Autodesk chuyen doi toan bo nen tang API sang .NET 8.0.",
    "- Bo cai dat da tu dong bien dich rieng ban .NET 8.0 tai thu muc 'Revit_2026' de tuong thich 100%,",
    "  dam bao toc do khoi dong nhanh, xu ly hinh hoc 3D va Boolean toi uu nhat.",
    "",
    "II. 2 CACH KHOI DONG CONG CU",
    "1. CACH 1: Chay truc tiep bang cach nhap dup chuot vao file 'RevitQtoTool.exe'.",
    "   - Cua so giao dien WPF hien len ngay lap tuc!",
    "   - Ho tro xem truoc du lieu mau, kiem tra giao dien, xuat thu file Excel.",
    "",
    "2. CACH 2: Cai dat nut bam truc tiep len thanh Ribbon cua Revit 2026.",
    "   - Nhap dup chuot vao file: 'CaiDat.bat'.",
    "   - Script se tu dong phat hien Revit 2026 va dang ky Add-in vao AppData.",
    "   - Mo Autodesk Revit 2026 -> Tab 'BIM QTO PRO' se xuat hien ngay tren thanh Ribbon!",
    "",
    "III. CAC TINH NANG CHINH",
    "1. Boc tach the tich Be tong (m3): Tu dong tru giao cat giua Cot - Dam - San - Vach.",
    "2. Tinh dien tich Van khuon (m2): Nhan dien huong mat Vector phap tuyen, loai bo mat dinh ho.",
    "3. Boc tach Cot thep (Rebar): Thong ke theo duong kinh Phi, chieu dai cat, trong luong TCVN (D^2/162).",
    "4. Kiem tra Trung lap & Va cham (Clash/Overlap): Phat hien trung lap 100% va tu dong Join.",
    "5. Quet ca cac file Revit Link (Link Kien truc / Ket cau).",
    "6. Xuat file Excel da Sheet chuan doanh nghiep VN: Grouping/Outline theo Tang, SUBTOTAL(9, ...).",
    "========================================================================================"
)
[System.IO.File]::WriteAllLines((Join-Path $distDir "Huong_Dan_Su_Dung.txt"), $guideLines, [System.Text.Encoding]::UTF8)

# 7. Copy manifest mẫu vào dist
Copy-Item "src\RevitQtoTool\RevitQtoTool.addin" (Join-Path $distDir "RevitQtoTool.addin") -Force

# 8. Nén thành file ZIP trong dist\
$zipPath = "dist\RevitQtoTool_Revit.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$distDir\*" -DestinationPath $zipPath -Force

Write-Host "==========================================================" -ForegroundColor Green
Write-Host " DONG GOI THANH CONG CHO REVIT 2026 (.NET 8.0) VAO: $distDir" -ForegroundColor Cyan
Write-Host " FILE THUC THI: dist\RevitQtoTool\RevitQtoTool.exe" -ForegroundColor Magenta
Write-Host " GOI REVIT 2026 (.NET 8): dist\RevitQtoTool\Revit_2026\" -ForegroundColor Yellow
Write-Host " FILE NEN ZIP PHAT HANH: $zipPath" -ForegroundColor Yellow
Write-Host " KICH THUOC: $((Get-Item $zipPath).Length / 1MB) MB" -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Green
