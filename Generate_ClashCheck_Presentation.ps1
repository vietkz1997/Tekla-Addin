# =============================================================================
# SCRIPT: Generate_ClashCheck_Presentation.ps1
# DESCRIPTION: Tao slide PowerPoint (.pptx) va PDF huong dan su dung Clash-check
#              voi day du tieng Viet co dau, danh so tung buoc tren giao dien.
# =============================================================================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$RepoRoot = "c:\Users\BIM\Documents\Github\Tekla-Addin"
$ImagesDir = Join-Path $RepoRoot "docs\images"

# Cac vi tri luu file (ra ngoai thu muc goc, Desktop, docs va dist)
$OutputPptxRoot    = Join-Path $RepoRoot "Huong_Dan_Su_Dung_Clash_Check.pptx"
$OutputPdfRoot     = Join-Path $RepoRoot "Huong_Dan_Su_Dung_Clash_Check.pdf"
$DesktopDir        = [Environment]::GetFolderPath("Desktop")
$OutputPptxDesktop = Join-Path $DesktopDir "Huong_Dan_Su_Dung_Clash_Check.pptx"
$OutputPdfDesktop  = Join-Path $DesktopDir "Huong_Dan_Su_Dung_Clash_Check.pdf"
$OutputPptxDocs    = Join-Path $RepoRoot "docs\Huong_Dan_Su_Dung_Clash_Check.pptx"
$OutputPdfDocs     = Join-Path $RepoRoot "docs\Huong_Dan_Su_Dung_Clash_Check.pdf"
$OutputPptxDist    = Join-Path $RepoRoot "dist\ClashCheck_Tekla2020\Huong_Dan_Su_Dung_Clash_Check.pptx"

# He mau chuan Light Theme Executive Engineering
Add-Type -AssemblyName System.Drawing
function To-OleColor([int]$r, [int]$g, [int]$b) {
    return [System.Drawing.ColorTranslator]::ToOle([System.Drawing.Color]::FromArgb($r, $g, $b))
}

$COLOR_BG_LIGHT     = To-OleColor 248 250 252 # #F8FAFC (Slate 50 - Nen trang sang tinh te)
$COLOR_CARD_LIGHT   = To-OleColor 255 255 255 # #FFFFFF (Trang tinh khoi cho Card)
$COLOR_CARD_BORDER  = To-OleColor 203 213 225 # #CBD5E1 (Slate 300 - Vien card ro rang)
$COLOR_CYAN         = To-OleColor 2 132 199   # #0284C7 (Sky/Cyan 600 - Xanh lo dam net tren nen sang)
$COLOR_BLUE         = To-OleColor 29 78 216   # #1D4ED8 (Blue 700 - Xanh duong dam thanh lich)
$COLOR_EMERALD      = To-OleColor 4 120 87    # #047857 (Emerald 700 - Xanh la cay dam)
$COLOR_AMBER        = To-OleColor 180 83 9    # #B45309 (Amber 700 - Vang ho phach dam)
$COLOR_ROSE         = To-OleColor 190 18 60   # #BE123C (Rose 700 - Do crimson sang trong)
$COLOR_PURPLE       = To-OleColor 109 40 217  # #6D28D9 (Purple 700 - Tim dam net)
$COLOR_TEXT_MAIN    = To-OleColor 15 23 42    # #0F172A (Slate 900 - Chu tieu de den than dam)
$COLOR_TEXT_MUTED   = To-OleColor 51 65 85    # #334155 (Slate 700 - Chu noi dung de doc nhat)
$COLOR_TEXT_SUB     = To-OleColor 100 116 139 # #64748B (Slate 500 - Chu phu chu thich)
$COLOR_BADGE_BG     = To-OleColor 241 245 249 # #F1F5F9 (Slate 100 - Nen badge nhe)
$COLOR_FOOTER       = To-OleColor 148 163 184 # #94A3B8 (Slate 400 - Footer)

Write-Host ">>> Khoi chay Microsoft PowerPoint Automation..." -ForegroundColor Cyan
$pptApp = New-Object -ComObject PowerPoint.Application
$pptApp.Visible = [Microsoft.Office.Core.MsoTriState]::msoTrue

# Tao presentation 16:9 Widescreen (960 x 540 pt)
$pres = $pptApp.Presentations.Add([Microsoft.Office.Core.MsoTriState]::msoTrue)
$pres.PageSetup.SlideWidth  = 960
$pres.PageSetup.SlideHeight = 540
$ppLayoutBlank = 12

# -----------------------------------------------------------------------------
# Helper: Background & Headers
# -----------------------------------------------------------------------------
function Init-SlideBackground {
    param($slide, $categoryTag, $slideTitle, $slideSub)

    # Nen sang
    $bg = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRectangle, 0, 0, 960, 540)
    $bg.Fill.Solid()
    $bg.Fill.ForeColor.RGB = $COLOR_BG_LIGHT
    $bg.Line.Visible = [Microsoft.Office.Core.MsoTriState]::msoFalse

    # Top Badge
    if ($categoryTag) {
        $tagW = [Math]::Max(240, ($categoryTag.Length * 7.5))
        $tagShape = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, 50, 18, $tagW, 24)
        $tagShape.Fill.Solid()
        $tagShape.Fill.ForeColor.RGB = $COLOR_CARD_LIGHT
        $tagShape.Line.ForeColor.RGB = $COLOR_BLUE
        $tagShape.Line.Weight = 1.0
        $tagShape.TextFrame.TextRange.Text = $categoryTag
        $tagShape.TextFrame.TextRange.Font.Name = "Segoe UI"
        $tagShape.TextFrame.TextRange.Font.Size = 9.5
        $tagShape.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
        $tagShape.TextFrame.TextRange.Font.Color.RGB = $COLOR_BLUE
        $tagShape.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter
    }

    # Tieu de slide
    if ($slideTitle) {
        $titleBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 46, 860, 32)
        $titleBox.TextFrame.TextRange.Text = $slideTitle
        $titleBox.TextFrame.TextRange.Font.Name = "Segoe UI"
        $titleBox.TextFrame.TextRange.Font.Size = 18.5
        $titleBox.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
        $titleBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MAIN
        $titleBox.TextFrame.MarginLeft = 0
        $titleBox.TextFrame.MarginTop = 0
    }

    # Phu de slide
    if ($slideSub) {
        $subBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 78, 860, 20)
        $subBox.TextFrame.TextRange.Text = $slideSub
        $subBox.TextFrame.TextRange.Font.Name = "Segoe UI"
        $subBox.TextFrame.TextRange.Font.Size = 10.5
        $subBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_SUB
        $subBox.TextFrame.MarginLeft = 0
        $subBox.TextFrame.MarginTop = 0
    }

    # Footer
    $footerBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 516, 860, 18)
    $footerBox.TextFrame.TextRange.Text = "TEKLA ADDIN - CLASH CHECK (REBAR VS IFC) - HƯỚNG DẪN SỬ DỤNG 2026"
    $footerBox.TextFrame.TextRange.Font.Name = "Segoe UI"
    $footerBox.TextFrame.TextRange.Font.Size = 8.5
    $footerBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_FOOTER
    $footerBox.TextFrame.MarginLeft = 0
}

# -----------------------------------------------------------------------------
# Helper: Feature Card
# -----------------------------------------------------------------------------
function Add-FeatureCard {
    param($slide, $x, $y, $w, $h, $cmdTag, $tagColor, $title, $bodyText)

    $card = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, $x, $y, $w, $h)
    $card.Fill.Solid()
    $card.Fill.ForeColor.RGB = $COLOR_CARD_LIGHT
    $card.Line.ForeColor.RGB = $COLOR_CARD_BORDER
    $card.Line.Weight = 1.0

    $strip = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, ($x + 3), ($y + 5), 4, ($h - 10))
    $strip.Fill.Solid()
    $strip.Fill.ForeColor.RGB = $tagColor
    $strip.Line.Visible = [Microsoft.Office.Core.MsoTriState]::msoFalse

    $badgeW = [Math]::Max(70, ($cmdTag.Length * 8.5))
    $badge = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, ($x + 14), ($y + 9), $badgeW, 20)
    $badge.Fill.Solid()
    $badge.Fill.ForeColor.RGB = $COLOR_BADGE_BG
    $badge.Line.ForeColor.RGB = $tagColor
    $badge.Line.Weight = 1.0
    $badge.TextFrame.TextRange.Text = $cmdTag
    $badge.TextFrame.TextRange.Font.Name = "Segoe UI"
    $badge.TextFrame.TextRange.Font.Size = 9
    $badge.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
    $badge.TextFrame.TextRange.Font.Color.RGB = $tagColor
    $badge.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter

    $titleX = $x + 14 + $badgeW + 8
    $titleBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, $titleX, ($y + 8), ($w - ($badgeW + 28)), 22)
    $titleBox.TextFrame.TextRange.Text = $title
    $titleBox.TextFrame.TextRange.Font.Name = "Segoe UI"
    $titleBox.TextFrame.TextRange.Font.Size = 11.5
    $titleBox.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
    $titleBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MAIN
    $titleBox.TextFrame.MarginLeft = 0
    $titleBox.TextFrame.MarginTop = 0

    $bodyBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, ($x + 14), ($y + 32), ($w - 28), ($h - 36))
    $bodyBox.TextFrame.TextRange.Text = $bodyText
    $bodyBox.TextFrame.TextRange.Font.Name = "Segoe UI"
    $bodyBox.TextFrame.TextRange.Font.Size = 9.5
    $bodyBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MUTED
    $bodyBox.TextFrame.WordWrap = [Microsoft.Office.Core.MsoTriState]::msoTrue
    $bodyBox.TextFrame.MarginLeft = 0
    $bodyBox.TextFrame.MarginTop = 0
}

# -----------------------------------------------------------------------------
# Helper: Framed Image
# -----------------------------------------------------------------------------
function Add-FramedImage {
    param($slide, $imagePath, $x, $y, $w, $h, $caption)

    if (Test-Path $imagePath) {
        $frameH = $h + 6
        if ($caption) { $frameH = $h + 28 }

        $frame = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, ($x - 3), ($y - 3), ($w + 6), $frameH)
        $frame.Fill.Solid()
        $frame.Fill.ForeColor.RGB = $COLOR_CARD_LIGHT
        $frame.Line.ForeColor.RGB = $COLOR_CARD_BORDER
        $frame.Line.Weight = 1.0

        $pic = $slide.Shapes.AddPicture($imagePath, [Microsoft.Office.Core.MsoTriState]::msoFalse, [Microsoft.Office.Core.MsoTriState]::msoTrue, $x, $y, $w, $h)
        $pic.Line.ForeColor.RGB = $COLOR_CARD_BORDER
        $pic.Line.Weight = 1.0

        if ($caption) {
            $capBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, $x, ($y + $h + 4), $w, 18)
            $capBox.TextFrame.TextRange.Text = $caption
            $capBox.TextFrame.TextRange.Font.Name = "Segoe UI"
            $capBox.TextFrame.TextRange.Font.Size = 8.5
            $capBox.TextFrame.TextRange.Font.Italic = [Microsoft.Office.Core.MsoTriState]::msoTrue
            $capBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_SUB
            $capBox.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter
            $capBox.TextFrame.MarginLeft = 0
            $capBox.TextFrame.MarginTop = 0
        }
    }
}

# -----------------------------------------------------------------------------
# Helper: Step Badge Marker
# -----------------------------------------------------------------------------
function Add-StepBadge {
    param($slide, $x, $y, $number, $color, $label)

    $badge = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeOval, $x, $y, 24, 24)
    $badge.Fill.Solid()
    $badge.Fill.ForeColor.RGB = $color
    $badge.Line.ForeColor.RGB = To-OleColor 255 255 255
    $badge.Line.Weight = 1.8
    $badge.TextFrame.TextRange.Text = [string]$number
    $badge.TextFrame.TextRange.Font.Name = "Segoe UI"
    $badge.TextFrame.TextRange.Font.Size = 11.5
    $badge.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
    $badge.TextFrame.TextRange.Font.Color.RGB = To-OleColor 255 255 255
    $badge.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter
    $badge.TextFrame.MarginLeft = 0
    $badge.TextFrame.MarginTop = 0

    if ($label) {
        $lblBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, ($x + 28), ($y + 2), 220, 22)
        $lblBox.TextFrame.TextRange.Text = $label
        $lblBox.TextFrame.TextRange.Font.Name = "Segoe UI"
        $lblBox.TextFrame.TextRange.Font.Size = 10
        $lblBox.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
        $lblBox.TextFrame.TextRange.Font.Color.RGB = $color
        $lblBox.TextFrame.MarginLeft = 0
        $lblBox.TextFrame.MarginTop = 0
    }
}

# =============================================================================
# SLIDE 1: TRANG BIA (COVER SLIDE)
# =============================================================================
Write-Host ">>> Slide 1: Trang bia tong quan..." -ForegroundColor Yellow
$s1 = $pres.Slides.Add(1, $ppLayoutBlank)
Init-SlideBackground -slide $s1 -categoryTag $null -slideTitle $null -slideSub $null

$glow = $s1.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeOval, 640, 50, 270, 270)
$glow.Fill.Solid()
$glow.Fill.ForeColor.RGB = To-OleColor 224 242 254
$glow.Line.ForeColor.RGB = To-OleColor 186 230 253
$glow.Line.Weight = 1.5

$badge1 = $s1.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, 50, 70, 360, 30)
$badge1.Fill.Solid()
$badge1.Fill.ForeColor.RGB = $COLOR_CARD_LIGHT
$badge1.Line.ForeColor.RGB = $COLOR_BLUE
$badge1.TextFrame.TextRange.Text = "★ TEKLA STRUCTURES ADDIN - 2020 - 2025 - 2026"
$badge1.TextFrame.TextRange.Font.Name = "Segoe UI"
$badge1.TextFrame.TextRange.Font.Size = 10.5
$badge1.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
$badge1.TextFrame.TextRange.Font.Color.RGB = $COLOR_BLUE
$badge1.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter

$tBox1 = $s1.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 110, 700, 120)
$tBox1.TextFrame.TextRange.Text = "HƯỚNG DẪN SỬ DỤNG`nTOOL CLASH-CHECK"
$tBox1.TextFrame.TextRange.Font.Name = "Segoe UI"
$tBox1.TextFrame.TextRange.Font.Size = 34
$tBox1.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
$tBox1.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MAIN

$subBox1 = $s1.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 230, 700, 50)
$subBox1.TextFrame.TextRange.Text = "Kiểm tra va chạm và khoảng hở an toàn tự động giữa Cốt thép (Tekla Model) và Cấu kiện tham chiếu IFC (Navisworks Style tốc độ cao)."
$subBox1.TextFrame.TextRange.Font.Name = "Segoe UI"
$subBox1.TextFrame.TextRange.Font.Size = 13
$subBox1.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MUTED
$subBox1.TextFrame.WordWrap = [Microsoft.Office.Core.MsoTriState]::msoTrue

$s1c1 = "• Thuật toán lọc 2 pha: Broad-phase AABB và Narrow-phase chính xác.`n• Tận dụng 100% tất cả các nhân CPU máy tính.`n• Quét hàng ngàn thanh thép chỉ trong vài giây."
Add-FeatureCard -slide $s1 -x 50 -y 295 -w 270 -h 180 -cmdTag "TỐC ĐỘ" -tagColor $COLOR_CYAN -title "Quét Đa Luồng CPU" -bodyText $s1c1

$s1c2 = "• Lược bỏ chi tiết phụ (SkipNames): Bu lông, lan can, tai cẩu, mối hàn.`n• Chỉ quét cấu kiện chỉ định (OnlyNames).`n• Tích chọn đồng thời nhiều file IFC linh hoạt."
Add-FeatureCard -slide $s1 -x 345 -y 295 -w 270 -h 180 -cmdTag "BỘ LỌC" -tagColor $COLOR_EMERALD -title "Bộ Lọc 3 Tầng Triệt Để" -bodyText $s1c2

$s1c3 = "• Chuột phải hoặc phím H / Delete để ẩn dòng đã duyệt.`n• Tự động nhảy dòng kế tiếp.`n• Zoom cận cảnh và vẽ hộp lập phương 3D màu đỏ đánh dấu va chạm."
Add-FeatureCard -slide $s1 -x 640 -y 295 -w 270 -h 180 -cmdTag "KIỂM DUYỆT" -tagColor $COLOR_AMBER -title "Kiểm Duyệt Nhanh 3D" -bodyText $s1c3

# =============================================================================
# SLIDE 2: SƠ ĐỒ ĐÁNH SỐ TỪNG BƯỚC TRÊN GIAO DIỆN CHÍNH
# =============================================================================
Write-Host ">>> Slide 2: So do danh so tung buoc..." -ForegroundColor Yellow
$s2 = $pres.Slides.Add(2, $ppLayoutBlank)
Init-SlideBackground -slide $s2 -categoryTag "TỔNG QUAN GIAO DIỆN VÀ CÁC BƯỚC THAO TÁC" -slideTitle "Sơ đồ 7 bước thao tác trên giao diện chính của Tool" -slideSub "Hình ảnh trực quan đánh dấu từng vị trí điều khiển từ Bước 1 đến Bước 7 để dễ dàng làm theo"

$imgOverview = Join-Path $ImagesDir "main_form_overview.png"
$imgX = 40
$imgY = 105
$imgW = 600
$imgH = 350
Add-FramedImage -slide $s2 -imagePath $imgOverview -x $imgX -y $imgY -w $imgW -h $imgH -caption "Giao diện chính công cụ Tekla Clash Check với 7 vị trí điều khiển đánh số"

# Danh dau 7 nut tron tren anh chup
Add-StepBadge -slide $s2 -x ($imgX + 110) -y ($imgY + 50)  -number 1 -color $COLOR_CYAN
Add-StepBadge -slide $s2 -x ($imgX + 245) -y ($imgY + 50)  -number 2 -color $COLOR_EMERALD
Add-StepBadge -slide $s2 -x ($imgX + 440) -y ($imgY + 50)  -number 3 -color $COLOR_AMBER
Add-StepBadge -slide $s2 -x ($imgX + 390) -y ($imgY + 95)  -number 4 -color $COLOR_PURPLE
Add-StepBadge -slide $s2 -x ($imgX + 45)  -y ($imgY + 110) -number 5 -color $COLOR_BLUE
Add-StepBadge -slide $s2 -x ($imgX + 180) -y ($imgY + 110) -number 6 -color $COLOR_ROSE
Add-StepBadge -slide $s2 -x ($imgX + 280) -y ($imgY + 230) -number 7 -color $COLOR_CYAN

$panelX = 660
$panelY = 105
$panelW = 260
$panelH = 380

$guideCard = $s2.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, $panelX, $panelY, $panelW, $panelH)
$guideCard.Fill.Solid()
$guideCard.Fill.ForeColor.RGB = $COLOR_CARD_LIGHT
$guideCard.Line.ForeColor.RGB = $COLOR_CARD_BORDER

$guideTitle = $s2.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, ($panelX + 12), ($panelY + 10), ($panelW - 24), 24)
$guideTitle.TextFrame.TextRange.Text = "DANH MỤC 7 BƯỚC THAO TÁC"
$guideTitle.TextFrame.TextRange.Font.Name = "Segoe UI"
$guideTitle.TextFrame.TextRange.Font.Size = 10.5
$guideTitle.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
$guideTitle.TextFrame.TextRange.Font.Color.RGB = $COLOR_BLUE

$guideText = "① Rebar Scope: [Selected Rebars] / [All Rebars]`n`n② IFC Model: Tích chọn IFC / Auto scan`n`n③ Tolerance (mm) và Clearance (mm)`n`n④ Filter: SkipNames và OnlyNames`n`n⑤ Bấm [Run Clash Check] (hoặc Stop)`n`n⑥ Zoom Selected, Highlight 3D, Clear 3D, Export Report`n`n⑦ Chuột phải ẩn dòng / Phím H, Del"
$guideBody = $s2.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, ($panelX + 12), ($panelY + 38), ($panelW - 24), ($panelH - 45))
$guideBody.TextFrame.TextRange.Text = $guideText
$guideBody.TextFrame.TextRange.Font.Name = "Segoe UI"
$guideBody.TextFrame.TextRange.Font.Size = 9.5
$guideBody.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MUTED
$guideBody.TextFrame.WordWrap = [Microsoft.Office.Core.MsoTriState]::msoTrue

# =============================================================================
# SLIDE 3: BƯỚC 1 - CHỌN PHẠM VI CỐT THÉP (REBAR SCOPE)
# =============================================================================
Write-Host ">>> Slide 3: Buoc 1 - Rebar Scope..." -ForegroundColor Yellow
$s3 = $pres.Slides.Add(3, $ppLayoutBlank)
Init-SlideBackground -slide $s3 -categoryTag "BƯỚC 1 TRÊN GIAO DIỆN" -slideTitle "Bước 1: Chọn Phạm Vi Cốt Thép Cần Kiểm Tra (Rebar Scope)" -slideSub "Xác định rõ đối tượng mục tiêu để tối ưu thời gian quét và tập trung đúng khu vực cần xử lý"

Add-StepBadge -slide $s3 -x 50 -y 110 -number 1 -color $COLOR_CYAN -label "Rebar Scope"

$s3Text1 = "• Định nghĩa: Chỉ quét những thanh cốt thép hoặc dầm/cột bê tông đang được chọn trực tiếp trong khung nhìn Tekla 3D.`n• Ưu điểm vượt trội:`n  - Tốc độ cực nhanh: Quét xong trong 1-2 giây cho 1 dầm, 1 cột hoặc 1 tầng sàn.`n  - Phù hợp nhất khi bạn đang triển khai và sửa đổi shop rebar cho từng cấu kiện cụ thể.`n• Mẹo thông minh: Khi chọn cấu kiện bê tông cha (Host Part), toàn bộ cốt thép con bên trong sẽ tự động được thu thập để kiểm tra!"
Add-FeatureCard -slide $s3 -x 50 -y 150 -w 420 -h 180 -cmdTag "🎯 SELECTED REBARS" -tagColor $COLOR_CYAN -title "Chế độ: Selected Rebars (Khuyên dùng)" -bodyText $s3Text1

$s3Text2 = "• Định nghĩa: Công cụ sẽ tự động thu thập và kiểm tra toàn bộ cốt thép có trong toàn bộ mô hình Tekla.`n• Ưu điểm:`n  - Kiểm tra tổng thể toàn bộ dự án trước khi phát hành bản vẽ thi công.`n  - Phát hiện các vị trí va chạm ngoài tầm kiểm soát.`n• Tối ưu vượt bậc: Áp dụng thuật toán Bounding Box Spatial Index, thời gian quét toàn bộ mô hình được rút ngắn tối đa."
Add-FeatureCard -slide $s3 -x 490 -y 150 -w 420 -h 180 -cmdTag "🌐 ALL REBARS" -tagColor $COLOR_EMERALD -title "Chế độ: All Rebars (Toàn bộ mô hình)" -bodyText $s3Text2

$s3Text3 = "1. Bật công tắc chọn thanh thép (Select single rebar) hoặc chọn cấu kiện (Select components) trên thanh công cụ Selection của Tekla.`n2. Quét chuột chọn vùng dầm, cột hoặc vách cần kiểm tra va chạm trên không gian 3D.`n3. Trên giao diện Tool, đảm bảo nút [🎯 Selected Rebars] đang sáng màu xanh dương nổi bật."
Add-FeatureCard -slide $s3 -x 50 -y 345 -w 860 -h 130 -cmdTag "THAO TÁC NHANH" -tagColor $COLOR_AMBER -title "Cách thao tác chuẩn trên màn hình Tekla" -bodyText $s3Text3

# =============================================================================
# SLIDE 4: BƯỚC 2 - CHỌN FILE IFC THAM CHIẾU (IFC MODEL)
# =============================================================================
Write-Host ">>> Slide 4: Buoc 2 - Chon file IFC (IFC Model)..." -ForegroundColor Yellow
$s4 = $pres.Slides.Add(4, $ppLayoutBlank)
Init-SlideBackground -slide $s4 -categoryTag "BƯỚC 2 TRÊN GIAO DIỆN" -slideTitle "Bước 2: Lựa Chọn File IFC Tham Chiếu (IFC Model Dropdown)" -slideSub "Hỗ trợ tích chọn đồng thời nhiều file IFC hoặc tự động phát hiện theo phong cách Autodesk Navisworks"

Add-StepBadge -slide $s4 -x 50 -y 110 -number 2 -color $COLOR_EMERALD -label "IFC Model"

Add-FramedImage -slide $s4 -imagePath (Join-Path $ImagesDir "media_1790152101503.png") -x 50 -y 145 -w 270 -h 340 -caption "Menu thả xuống tích chọn nhiều file IFC"

$s4Text1 = "• Tự động quét Reference Models: Tool tự động nạp toàn bộ danh sách các file IFC đang được đính kèm trong mô hình Tekla.`n• Tích chọn đa file linh hoạt: Người dùng có thể tích chọn đồng thời 1, 2 hoặc nhiều file IFC cùng lúc (ví dụ vừa quét file Kết cấu thép, vừa quét file Cơ điện MEP).`n• Tìm kiếm nhanh: Gõ từ khóa vào ô tìm kiếm để lọc nhanh danh sách hàng chục file IFC."
Add-FeatureCard -slide $s4 -x 345 -y 145 -w 565 -h 120 -cmdTag "TÍCH CHỌN ĐA FILE" -tagColor $COLOR_CYAN -title "Menu Tích Chọn Nhiều File IFC Cùng Lúc" -bodyText $s4Text1

$s4Text2 = "• ⭐ Auto scan all IFC files: Tự động khoanh vùng và quét tất cả các file IFC có đối tượng giao cắt với vùng cốt thép.`n• 🎯 Selected IFC objects / Parts in Tekla only: Chỉ quét va chạm với các Part hoặc Reference Object mà bạn đang pick chọn trên 3D.`n• Phím tắt chọn nhanh: Bấm [☑ Check All] hoặc [☐ Clear All] để thao tác chỉ với 1 cú nhấp chuột."
Add-FeatureCard -slide $s4 -x 345 -y 280 -w 565 -h 120 -cmdTag "CHẾ ĐỘ NÂNG CAO" -tagColor $COLOR_AMBER -title "Các Tùy Chọn Quét IFC Đặc Biệt (Dropdown)" -bodyText $s4Text2

$s4Text3 = "• Lưu ý kỹ thuật: File IFC phải được chèn vào Tekla qua bảng Reference Models và biểu tượng con mắt hiển thị phải ở trạng thái BẬT (Visible)."
Add-FeatureCard -slide $s4 -x 345 -y 415 -w 565 -h 70 -cmdTag "LƯU Ý" -tagColor $COLOR_ROSE -title "Trạng Thái Hiển Thị File IFC" -bodyText $s4Text3

# =============================================================================
# SLIDE 5: BƯỚC 3 - THIẾT LẬP DUNG SAI & KHOẢNG HỞ (TOLERANCE & CLEARANCE)
# =============================================================================
Write-Host ">>> Slide 5: Buoc 3 - Dung sai & Khoang ho..." -ForegroundColor Yellow
$s5 = $pres.Slides.Add(5, $ppLayoutBlank)
Init-SlideBackground -slide $s5 -categoryTag "BƯỚC 3 TRÊN GIAO DIỆN" -slideTitle "Bước 3: Thiết Lập Tolerance (mm) và Clearance (mm)" -slideSub "Cấu hình chuẩn xác để loại bỏ va chạm giả và đồng bộ 100% với tham số trong Autodesk Navisworks Manage"

Add-StepBadge -slide $s5 -x 50 -y 110 -number 3 -color $COLOR_AMBER -label "Tolerance & Clearance"

Add-FramedImage -slide $s5 -imagePath (Join-Path $ImagesDir "media_1790150641284.png") -x 50 -y 145 -w 440 -h 80 -caption "Cụm tham số trên Tool: Tolerance (mm) và Clearance (mm)"
Add-FramedImage -slide $s5 -imagePath (Join-Path $ImagesDir "navisworks_tolerance.png") -x 510 -y 145 -w 400 -h 80 -caption "Tham số trong Navisworks Manage: Tolerance 0.008 m (8 mm)"

$s5Text1 = "• Tương đương trong Navisworks: Tolerance ở chế độ Type: Hard.`n• Ý nghĩa: Bỏ qua độ lấn bề mặt nhỏ hơn hoặc bằng giá trị này.`n• Quy đổi: 0.008 m (Navisworks) = 8 mm (Tool Clash-check).`n• Xử lý thông minh: Tiếp xúc bề mặt (< 0.01mm) hoặc tiếp xúc đầu mút thanh thép (axial end contact) được bỏ qua an toàn để không báo lỗi giả!"
Add-FeatureCard -slide $s5 -x 50 -y 245 -w 420 -h 130 -cmdTag "TOLERANCE (MM)" -tagColor $COLOR_CYAN -title "Dung sai va chạm (Tolerance)" -bodyText $s5Text1

$s5Text2 = "• Tương đương trong Navisworks: Tolerance ở chế độ Type: Clearance.`n• Ý nghĩa: Vùng đệm khoảng cách an toàn tối thiểu giữa mép ngoài cốt thép và cấu kiện IFC.`n• Khuyên dùng: Đặt 0 mm khi chỉ kiểm tra đâm xuyên vật lý; đặt 20-50 mm khi cần đảm bảo lớp bê tông bảo vệ."
Add-FeatureCard -slide $s5 -x 490 -y 245 -w 420 -h 130 -cmdTag "CLEARANCE (MM)" -tagColor $COLOR_AMBER -title "Khoảng hở bảo vệ (Clearance)" -bodyText $s5Text2

$s5Text3 = "• 🔴 Severe (Đỏ): Va chạm nghiêm trọng - cốt thép đâm xuyên sâu vào thân cấu kiện IFC (> 20mm).`n• 🔵/🟠 Medium (Xanh/Cam): Va chạm mức trung bình - độ lấn từ 5mm đến 20mm.`n• 🟡 Minor (Vàng): Va chạm nhẹ mép hoặc xâm phạm khoảng hở an toàn (< 5mm)."
Add-FeatureCard -slide $s5 -x 50 -y 390 -w 860 -h 95 -cmdTag "PHÂN CẤP SEVERITY" -tagColor $COLOR_ROSE -title "Phân Cấp Mức Độ Va Chạm Tự Động (Severity)" -bodyText $s5Text3

# =============================================================================
# SLIDE 6: BƯỚC 4 - THIẾT LẬP BỘ LỌC SKIP FILTER & ONLY FILTER
# =============================================================================
Write-Host ">>> Slide 6: Buoc 4 - Bo loc Skip Filter..." -ForegroundColor Yellow
$s6 = $pres.Slides.Add(6, $ppLayoutBlank)
Init-SlideBackground -slide $s6 -categoryTag "BƯỚC 4 TRÊN GIAO DIỆN" -slideTitle "Bước 4: Thiết Lập Bộ Lọc Skip Filter và Only Filter" -slideSub "Loại bỏ hoàn toàn các chi tiết phụ và tập trung kiểm tra đúng các đối tượng kết cấu chính"

Add-StepBadge -slide $s6 -x 50 -y 110 -number 4 -color $COLOR_PURPLE -label "Skip & Only Filter"

Add-FramedImage -slide $s6 -imagePath (Join-Path $ImagesDir "media_1790151482316.png") -x 50 -y 145 -w 340 -h 140 -caption "Danh sách từ khóa Skip Filter được tự động lưu"

$s6Text1 = "• Mục đích: Tự động bỏ qua các đối tượng phụ không cần kiểm tra va chạm với cốt thép.`n• Danh sách mặc định: Bolt assembly, SAFETY_BAR, LUG, LADDER, SAFETY_HOOK, VBRACE, WELD_COUPLER, CHECK_COUPLER.`n• Cú pháp linh hoạt: Hỗ trợ phân tách từ khóa bằng dấu cách, xuống dòng hoặc dấu phẩy.`n• Nút [↺ Default]: Khôi phục lại danh sách từ khóa chuẩn ban đầu chỉ với 1 cú click."
Add-FeatureCard -slide $s6 -x 415 -y 145 -w 495 -h 140 -cmdTag "SKIP FILTER (SKIPNAMES)" -tagColor $COLOR_EMERALD -title "Bỏ Qua Cấu Kiện Phụ (Skip Filter)" -bodyText $s6Text1

$s6Text2 = "1. Tầng 1: Lọc ngay khi nạp file IFC trong C++ IfcConvert Engine (tiết kiệm 60% RAM).`n2. Tầng 2: Native Bounding Box Spatial Index trong Tekla lọc nhanh trong 1-2 giây (thay vì quét cây 3 phút).`n3. Tầng 3: Narrow-phase đa luồng loại trừ chính xác tuyệt đối các chi tiết phụ."
Add-FeatureCard -slide $s6 -x 50 -y 300 -w 420 -h 180 -cmdTag "3 TẦNG LỌC TỐC ĐỘ" -tagColor $COLOR_CYAN -title "Cơ Chế Lọc 3 Tầng Siêu Tốc" -bodyText $s6Text2

$s6Text3 = "• Mục đích: Chỉ quét duy nhất các cấu kiện IFC có tên chứa từ khóa chỉ định.`n• Ví dụ thực tế: Nhập BEAM, COLUMN, GIRDER, SLAB, WALL khi bạn chỉ muốn kiểm tra va chạm với Dầm, Cột hoặc Tường chính.`n• Nút [✖ Clear]: Xóa nhanh ô lọc để quay lại chế độ quét tất cả đối tượng."
Add-FeatureCard -slide $s6 -x 490 -y 300 -w 420 -h 180 -cmdTag "ONLY FILTER (ONLYNAMES)" -tagColor $COLOR_PURPLE -title "Chỉ Quét Cấu Kiện Chỉ Định (Only Filter)" -bodyText $s6Text3

# =============================================================================
# SLIDE 7: BƯỚC 5 - BẤM CHẠY RUN CLASH CHECK & STOP
# =============================================================================
Write-Host ">>> Slide 7: Buoc 5 - Run Clash Check & Stop..." -ForegroundColor Yellow
$s7 = $pres.Slides.Add(7, $ppLayoutBlank)
Init-SlideBackground -slide $s7 -categoryTag "BƯỚC 5 TRÊN GIAO DIỆN" -slideTitle "Bước 5: Khởi Chạy [Run Clash Check] và Giám Sát Tiến Trình" -slideSub "Kích hoạt cỗ máy tính toán song song đa luồng và giám sát tiến độ thời gian thực"

Add-StepBadge -slide $s7 -x 50 -y 110 -number 5 -color $COLOR_BLUE -label "Run Clash Check"

$s7Text1 = "• Cách thực hiện: Sau khi cấu hình xong Rebar Scope, IFC Model và Dung sai, nhấp vào nút [⚡ Run Clash Check].`n• Quy trình tự động diễn ra:`n  1. Khởi tạo tác vụ nền bất đồng bộ (Async Task), form hoàn toàn mượt mà không đơ treo.`n  2. Khoanh vùng IFC thông minh: Tận dụng Bounding Box Spatial Index lọc tức thì các cấu kiện IFC trong phạm vi thép.`n  3. Narrow-phase: Phân tích đa luồng song song trên 100% các nhân CPU (Parallel.ForEach)."
Add-FeatureCard -slide $s7 -x 50 -y 150 -w 550 -h 160 -cmdTag "⚡ RUN CLASH CHECK" -tagColor $COLOR_BLUE -title "Khởi Chạy Quét Va Chạm Song Song" -bodyText $s7Text1

$s7Text2 = "• Trong khi quét, nút [⏹ Stop] màu xám sẽ sáng lên để sẵn sàng hủy tác vụ.`n• Bạn có thể nhấp [⏹ Stop] bất kỳ lúc nào để dừng quét an toàn qua CancellationToken.`n• Kết quả tính toán đến thời điểm dừng vẫn được giữ lại nguyên vẹn để phân tích."
Add-FeatureCard -slide $s7 -x 620 -y 150 -w 290 -h 160 -cmdTag "⏹ STOP AN TOÀN" -tagColor $COLOR_ROSE -title "Nút [⏹ Stop] Tác Vụ" -bodyText $s7Text2

$s7Text3 = "• Thanh trạng thái (Status Strip) ở góc dưới hiển thị thông báo thời gian thực: 'Searching IFC objects...', 'Checking collisions...', 'Done! X clash(es) found'.`n• Bộ đếm ở góc phải hiển thị tổng số va chạm (ví dụ: '2 clashes').`n• Khi hoàn tất, bảng DataGridView tự động hiển thị đầy đủ danh sách các điểm va chạm."
Add-FeatureCard -slide $s7 -x 50 -y 325 -w 860 -h 145 -cmdTag "GIÁM SÁT TIẾN ĐỘ" -tagColor $COLOR_EMERALD -title "Thanh Trạng Thái và Bộ Đếm Tiến Độ Thời Gian Thực" -bodyText $s7Text3

# =============================================================================
# SLIDE 8: BƯỚC 6 - TƯƠNG TÁC 3D VÀ XUẤT BÁO CÁO EXCEL/CSV
# =============================================================================
Write-Host ">>> Slide 8: Buoc 6 - Tuong tac 3D & Export Report..." -ForegroundColor Yellow
$s8 = $pres.Slides.Add(8, $ppLayoutBlank)
Init-SlideBackground -slide $s8 -categoryTag "BƯỚC 6 TRÊN GIAO DIỆN" -slideTitle "Bước 6: Tương Tác 3D Tekla và Xuất Báo Cáo [Export Report]" -slideSub "Xem thông tin chi tiết từng điểm va chạm, lia camera 3D, đánh dấu khối hộp và xuất file Excel nghiệm thu"

Add-StepBadge -slide $s8 -x 50 -y 110 -number 6 -color $COLOR_ROSE -label "Zoom, Highlight 3D & Report"

Add-FramedImage -slide $s8 -imagePath (Join-Path $ImagesDir "media_1790152662775.png") -x 50 -y 145 -w 860 -h 170 -caption "Bảng kết quả va chạm chi tiết và các nút điều khiển tương tác 3D"

$s8Text1 = "• Nhấp đúp chuột vào bất kỳ dòng nào trong bảng (hoặc chọn dòng rồi bấm [🔍 Zoom Selected]).`n• Camera 3D Tekla lập tức lia sát cận cảnh vào vị trí va chạm.`n• Thanh thép bị va chạm được tự động Select chọn nổi bật trên mô hình."
Add-FeatureCard -slide $s8 -x 50 -y 330 -w 270 -h 145 -cmdTag "🔍 ZOOM SELECTED" -tagColor $COLOR_CYAN -title "Zoom và Chọn Cấu Kiện" -bodyText $s8Text1

$s8Text2 = "• Bấm [💡 Highlight 3D]: Tự động vẽ các khối hộp lập phương 3D màu đỏ bao quanh điểm va chạm kèm dấu chéo.`n• Bấm [🧹 Clear 3D]: Tự động xóa sạch toàn bộ khối hộp vẽ tạm và vẽ lại khung nhìn Tekla (RedrawView) tức thì!"
Add-FeatureCard -slide $s8 -x 345 -y 330 -w 270 -h 145 -cmdTag "💡 HIGHLIGHT & CLEAR 3D" -tagColor $COLOR_AMBER -title "Đánh Dấu & Xóa Khối Hộp 3D" -bodyText $s8Text2

$s8Text3 = "• Bấm nút [📊 Export Report (Excel/CSV)].`n• Xuất file bảng mã UTF-8 chuẩn tiếng Việt có dấu.`n• Đầy đủ 12 cột: #, Rebar ID, Rebar Name, Size, Grade, Pos (Mark), Host Part, IFC Entity, Length (mm), Overlap (mm), Severity, Clash Point (X, Y, Z)."
Add-FeatureCard -slide $s8 -x 640 -y 330 -w 270 -h 145 -cmdTag "📊 EXPORT REPORT" -tagColor $COLOR_EMERALD -title "Báo Cáo Excel/CSV Đầy Đủ" -bodyText $s8Text3

# =============================================================================
# SLIDE 9: BƯỚC 7 - CHUỘT PHẢI ẨN DÒNG & PHÍM TẮT DUYỆT SIÊU TỐC
# =============================================================================
Write-Host ">>> Slide 9: Buoc 7 - Chuot phai an dong & Phim tat..." -ForegroundColor Yellow
$s9 = $pres.Slides.Add(9, $ppLayoutBlank)
Init-SlideBackground -slide $s9 -categoryTag "BƯỚC 7 TRÊN GIAO DIỆN (TÍNH NĂNG MỚI)" -slideTitle "Bước 7: Chuột Phải Ẩn Dòng và Phím Tắt H / Delete Duyệt Một Lượt" -slideSub "Quy trình kiểm duyệt mượt mà, loại bỏ điểm đã xử lý và tự động chuyển dòng kế tiếp"

Add-StepBadge -slide $s9 -x 50 -y 110 -number 7 -color $COLOR_CYAN -label "Hide Row & Context Menu"

$s9Text1 = "• Nhấp chuột phải vào bất kỳ ô hoặc dòng va chạm nào trong bảng:`n  - 👁️ Hide selected row(s) [H / Delete]`n  - 🔍 Zoom & Select in Tekla`n  - 🔄 Show all hidden rows ({0} hidden)`n  - 📋 Copy row info`n• Chuột phải lập tức chọn ngay dòng đó, thao tác tự nhiên và chuẩn xác."
Add-FeatureCard -slide $s9 -x 50 -y 150 -w 420 -h 165 -cmdTag "CONTEXT MENU" -tagColor $COLOR_AMBER -title "Menu Ngữ Cảnh Chuột Phải" -bodyText $s9Text1

$s9Text2 = "• Thay vì phải bấm chuột, bạn chỉ cần nhấn phím H (Hide) hoặc phím Delete trên bàn phím.`n• Dòng va chạm vừa kiểm tra xong sẽ lập tức biến mất khỏi bảng.`n• Con trỏ tự động nhảy xuống dòng hiển thị kế tiếp, giúp bạn duyệt 1 lượt 100+ điểm va chạm chỉ trong vài phút mà không cần nhấp chuột lại!"
Add-FeatureCard -slide $s9 -x 490 -y 150 -w 420 -h 165 -cmdTag "PHÍM TẮT H / DEL" -tagColor $COLOR_CYAN -title "Duyệt Siêu Tốc Bằng Phím Tắt" -bodyText $s9Text2

$s9Text3 = "• Thanh trạng thái tự động cập nhật số lượng dòng: 'X clashes (Y hidden)'.`n• Giúp kỹ sư nắm chắc tiến độ xử lý và số lượng va chạm còn tồn đọng.`n• Đã xử lý an toàn CurrencyManager của WinForms, không bao giờ xảy ra lỗi khi ẩn dòng đang chọn."
Add-FeatureCard -slide $s9 -x 50 -y 330 -w 420 -h 150 -cmdTag "TIẾN ĐỘ THỰC" -tagColor $COLOR_EMERALD -title "Bộ Đếm Tiến Độ Thời Gian Thực" -bodyText $s9Text3

$s9Text4 = "• Khi muốn xem lại toàn bộ danh sách va chạm ban đầu, nhấp chuột phải và chọn: [Show all hidden rows].`n• Bảng lập tức khôi phục đầy đủ tất cả các dòng va chạm nguyên vẹn.`n• Menu tự động hiển thị số lượng dòng đang bị ẩn để bạn dễ dàng theo dõi."
Add-FeatureCard -slide $s9 -x 490 -y 330 -w 420 -h 150 -cmdTag "KHÔI PHỤC" -tagColor $COLOR_PURPLE -title "Hiện Lại Toàn Bộ Dòng Đã Ẩn" -bodyText $s9Text4

# =============================================================================
# SLIDE 10: TỰ ĐỘNG GHI NHỚ CẤU HÌNH & ĐÓNG GÓI CHIA SẺ
# =============================================================================
Write-Host ">>> Slide 10: Luu cau hinh & Dong goi..." -ForegroundColor Yellow
$s10 = $pres.Slides.Add(10, $ppLayoutBlank)
Init-SlideBackground -slide $s10 -categoryTag "CẤU HÌNH VÀ CHIA SẺ BỘ CÔNG CỤ" -slideTitle "Tự Động Lưu Cấu Hình (Properties.Settings) và Đóng Gói Sử Dụng" -slideSub "Ghi nhớ vĩnh viễn mọi thiết lập khi tắt mở và dễ dàng chia sẻ trọn gói cho đồng nghiệp"

$s10Text1 = "• Mọi thông số điều khiển đều được tự động lưu vĩnh viễn vào Properties.Settings của Windows:`n  - Rebar Scope: [Selected Rebars] hay [All Rebars].`n  - Tolerance (mm) và Clearance (mm).`n  - Trạng thái Checkbox và danh sách từ khóa Skip Filter & Only Filter.`n  - Danh sách các file IFC cụ thể đã tích chọn.`n  - Kích thước cửa sổ Form (Width/Height) và trạng thái phóng to (Maximized).`n• Khi mở lại Tool, toàn bộ thiết lập được nạp lại nguyên vẹn 100%!"
Add-FeatureCard -slide $s10 -x 50 -y 125 -w 420 -h 210 -cmdTag "AUTO SAVE" -tagColor $COLOR_CYAN -title "Tự Động Ghi Nhớ Cài Đặt (Properties.Settings)" -bodyText $s10Text1

$s10Text2 = "• Bộ cài chạy độc lập (Standalone): Nằm gọn trong thư mục dist\ClashCheck_Tekla2020\.`n• File nén chia sẻ: dist\ClashCheck_Tekla2020.zip (chỉ ~19MB).`n• Chỉ cần giải nén và chạy file Clash-check.exe trên bất kỳ máy tính nào có cài Tekla Structures (không cần cài đặt thêm thư viện ngoài).`n• Tương thích mượt mà: Tekla Structures 2020, 2025, 2026."
Add-FeatureCard -slide $s10 -x 490 -y 125 -w 420 -h 210 -cmdTag "ĐÓNG GÓI CHIA SẺ" -tagColor $COLOR_EMERALD -title "Chia Sẻ Bộ Tool Cho Đồng Nghiệp" -bodyText $s10Text2

$s10Text3 = "• File PowerPoint hướng dẫn ở thư mục gốc: Huong_Dan_Su_Dung_Clash_Check.pptx`n• File PowerPoint đã sao chép ra Desktop: C:\Users\BIM\Desktop\Huong_Dan_Su_Dung_Clash_Check.pptx`n• File PDF xem nhanh trên điện thoại/máy tính: Huong_Dan_Su_Dung_Clash_Check.pdf`n• Bộ công cụ nén chia sẻ gửi đi: dist\ClashCheck_Tekla2020.zip"
Add-FeatureCard -slide $s10 -x 50 -y 350 -w 860 -h 130 -cmdTag "VỊ TRÍ FILE TÀI LIỆU" -tagColor $COLOR_AMBER -title "Vị Trí Các File Tài Liệu và Gói Phân Phối" -bodyText $s10Text3

# =============================================================================
# LƯU VÀ XUẤT FILE RA CÁC VỊ TRÍ (GỐC, DESKTOP, DOCS, DIST)
# =============================================================================
Write-Host ">>> Dang luu file PowerPoint ra ngoai thu muc goc va Desktop..." -ForegroundColor Cyan

# 1. Luu ra ngoai thu muc goc
if (Test-Path $OutputPptxRoot) { Remove-Item $OutputPptxRoot -Force }
$pres.SaveAs($OutputPptxRoot)
Write-Host "[OK] Da luu PPTX ra ngoai thu muc goc: $OutputPptxRoot" -ForegroundColor Green

# 2. Luu ra Desktop
if (Test-Path $DesktopDir) {
    if (Test-Path $OutputPptxDesktop) { Remove-Item $OutputPptxDesktop -Force }
    $pres.SaveCopyAs($OutputPptxDesktop)
    Write-Host "[OK] Da luu PPTX ra ngoai Desktop: $OutputPptxDesktop" -ForegroundColor Green
}

# 3. Luu vao docs va dist
if (Test-Path $OutputPptxDocs) { Remove-Item $OutputPptxDocs -Force }
$pres.SaveCopyAs($OutputPptxDocs)

if (Test-Path $OutputPptxDist) { Remove-Item $OutputPptxDist -Force }
$pres.SaveCopyAs($OutputPptxDist)

# 4. Xuat file PDF
$ppSaveAsPDF = 32
if (Test-Path $OutputPdfRoot) { Remove-Item $OutputPdfRoot -Force }
$pres.SaveAs($OutputPdfRoot, $ppSaveAsPDF)
Write-Host "[OK] Da luu PDF ra ngoai thu muc goc: $OutputPdfRoot" -ForegroundColor Green

if (Test-Path $DesktopDir) {
    Copy-Item $OutputPdfRoot $OutputPdfDesktop -Force
    Write-Host "[OK] Da luu PDF ra ngoai Desktop: $OutputPdfDesktop" -ForegroundColor Green
}

Copy-Item $OutputPdfRoot $OutputPdfDocs -Force

$OutputPdfDist = Join-Path $RepoRoot "dist\ClashCheck_Tekla2020\Huong_Dan_Su_Dung_Clash_Check.pdf"
Copy-Item $OutputPdfRoot $OutputPdfDist -Force


Write-Host ">>> Hoan tat xuat toan bo file PowerPoint va PDF thanh cong!" -ForegroundColor Green

$pres.Close()
$pptApp.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($pres) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($pptApp) | Out-Null
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()

