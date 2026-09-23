# =============================================================================
# SCRIPT: Generate_ClashCheck_Presentation.ps1
# DESCRIPTION: Generate professional, native PowerPoint (.pptx) presentation
#              for Tekla Clash Check tool using PowerPoint COM Automation.
# =============================================================================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$RepoRoot = "c:\Users\BIM\Documents\Github\Tekla-Addin"
$ImagesDir = Join-Path $RepoRoot "docs\images"
$OutputPptxDocs = Join-Path $RepoRoot "docs\Huong_Dan_Su_Dung_Clash_Check.pptx"
$OutputPptxDist = Join-Path $RepoRoot "dist\ClashCheck_Tekla2020\Huong_Dan_Su_Dung_Clash_Check.pptx"

# Modern Engineering Dark Theme Color Palette (Ole BGR values)
Add-Type -AssemblyName System.Drawing
function To-OleColor([int]$r, [int]$g, [int]$b) {
    return [System.Drawing.ColorTranslator]::ToOle([System.Drawing.Color]::FromArgb($r, $g, $b))
}

$COLOR_BG_DARK     = To-OleColor 15 23 42    # #0F172A (Deep Slate Navy)
$COLOR_CARD_DARK   = To-OleColor 30 41 59    # #1E293B (Slate 800)
$COLOR_CARD_BORDER = To-OleColor 51 65 85    # #334155 (Slate 700)
$COLOR_CYAN        = To-OleColor 56 189 248  # #38BDF8 (Cyan 400)
$COLOR_BLUE        = To-OleColor 37 99 235   # #2563EB (Blue 600)
$COLOR_EMERALD     = To-OleColor 52 211 153  # #34D399 (Emerald 400)
$COLOR_AMBER       = To-OleColor 251 191 36  # #FBBF24 (Amber Gold)
$COLOR_ROSE        = To-OleColor 244 63 94   # #F43F5E (Rose Pink)
$COLOR_PURPLE      = To-OleColor 168 85 247  # #A855F7 (Purple 500)
$COLOR_TEXT_WHITE  = To-OleColor 248 250 252 # #F8FAFC (Slate 50)
$COLOR_TEXT_MUTED  = To-OleColor 203 213 225 # #CBD5E1 (Slate 300)
$COLOR_TEXT_SUB    = To-OleColor 148 163 184 # #94A3B8 (Slate 400)
$COLOR_BADGE_BG    = To-OleColor 24 33 47    # #18212F

Write-Host ">>> Khoi chay Microsoft PowerPoint Automation..." -ForegroundColor Cyan
$pptApp = New-Object -ComObject PowerPoint.Application
$pptApp.Visible = [Microsoft.Office.Core.MsoTriState]::msoTrue

# Create new presentation (16:9 Widescreen: 960 x 540 points)
$pres = $pptApp.Presentations.Add([Microsoft.Office.Core.MsoTriState]::msoTrue)
$pres.PageSetup.SlideWidth  = 960
$pres.PageSetup.SlideHeight = 540
$ppLayoutBlank = 12

# -----------------------------------------------------------------------------
# Helper: Setup Slide Dark Background & Header
# -----------------------------------------------------------------------------
function Init-SlideBackground {
    param($slide, $categoryTag, $slideTitle, $slideSub)

    # Dark Background
    $bg = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRectangle, 0, 0, 960, 540)
    $bg.Fill.Solid()
    $bg.Fill.ForeColor.RGB = $COLOR_BG_DARK
    $bg.Line.Visible = [Microsoft.Office.Core.MsoTriState]::msoFalse

    # Top Category Badge / Pill
    if ($categoryTag) {
        $tagW = [Math]::Max(220, ($categoryTag.Length * 7.5))
        $tagShape = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, 50, 24, $tagW, 24)
        $tagShape.Fill.Solid()
        $tagShape.Fill.ForeColor.RGB = $COLOR_CARD_DARK
        $tagShape.Line.ForeColor.RGB = $COLOR_CYAN
        $tagShape.Line.Weight = 1.0
        $tagShape.TextFrame.TextRange.Text = $categoryTag
        $tagShape.TextFrame.TextRange.Font.Name = "Segoe UI"
        $tagShape.TextFrame.TextRange.Font.Size = 9.5
        $tagShape.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
        $tagShape.TextFrame.TextRange.Font.Color.RGB = $COLOR_CYAN
        $tagShape.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter
    }

    # Header Title
    if ($slideTitle) {
        $titleBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 52, 860, 36)
        $titleBox.TextFrame.TextRange.Text = $slideTitle
        $titleBox.TextFrame.TextRange.Font.Name = "Segoe UI"
        $titleBox.TextFrame.TextRange.Font.Size = 20
        $titleBox.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
        $titleBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_WHITE
        $titleBox.TextFrame.MarginLeft = 0
        $titleBox.TextFrame.MarginTop = 0
    }

    # Subtitle / Description
    if ($slideSub) {
        $subBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 88, 860, 22)
        $subBox.TextFrame.TextRange.Text = $slideSub
        $subBox.TextFrame.TextRange.Font.Name = "Segoe UI"
        $subBox.TextFrame.TextRange.Font.Size = 11
        $subBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_SUB
        $subBox.TextFrame.MarginLeft = 0
        $subBox.TextFrame.MarginTop = 0
    }

    # Footer Branding
    $footerBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 514, 860, 18)
    $footerBox.TextFrame.TextRange.Text = "TEKLA STRUCTURES ADDIN - CLASH CHECK (REBAR VS IFC) - VIET (BIM & SHOP) 2026"
    $footerBox.TextFrame.TextRange.Font.Name = "Segoe UI"
    $footerBox.TextFrame.TextRange.Font.Size = 8.5
    $footerBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_CARD_BORDER
    $footerBox.TextFrame.MarginLeft = 0
}

# -----------------------------------------------------------------------------
# Helper: Create Feature Card
# -----------------------------------------------------------------------------
function Add-FeatureCard {
    param($slide, $x, $y, $w, $h, $cmdTag, $tagColor, $title, $bodyText)

    # Card Base Container
    $card = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, $x, $y, $w, $h)
    $card.Fill.Solid()
    $card.Fill.ForeColor.RGB = $COLOR_CARD_DARK
    $card.Line.ForeColor.RGB = $COLOR_CARD_BORDER
    $card.Line.Weight = 1.0

    # Left Accent Strip
    $strip = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, ($x + 3), ($y + 5), 4, ($h - 10))
    $strip.Fill.Solid()
    $strip.Fill.ForeColor.RGB = $tagColor
    $strip.Line.Visible = [Microsoft.Office.Core.MsoTriState]::msoFalse

    # Badge Tag
    $badgeW = [Math]::Max(70, ($cmdTag.Length * 8.5))
    $badge = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, ($x + 14), ($y + 10), $badgeW, 20)
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

    # Card Title
    $titleX = $x + 14 + $badgeW + 8
    $titleBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, $titleX, ($y + 9), ($w - ($badgeW + 28)), 22)
    $titleBox.TextFrame.TextRange.Text = $title
    $titleBox.TextFrame.TextRange.Font.Name = "Segoe UI"
    $titleBox.TextFrame.TextRange.Font.Size = 11.5
    $titleBox.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
    $titleBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_WHITE
    $titleBox.TextFrame.MarginLeft = 0
    $titleBox.TextFrame.MarginTop = 0

    # Body
    $bodyBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, ($x + 14), ($y + 34), ($w - 28), ($h - 38))
    $bodyBox.TextFrame.TextRange.Text = $bodyText
    $bodyBox.TextFrame.TextRange.Font.Name = "Segoe UI"
    $bodyBox.TextFrame.TextRange.Font.Size = 9.5
    $bodyBox.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MUTED
    $bodyBox.TextFrame.WordWrap = [Microsoft.Office.Core.MsoTriState]::msoTrue
    $bodyBox.TextFrame.MarginLeft = 0
    $bodyBox.TextFrame.MarginTop = 0
}

# -----------------------------------------------------------------------------
# Helper: Add Framed Image
# -----------------------------------------------------------------------------
function Add-FramedImage {
    param($slide, $imagePath, $x, $y, $w, $h, $caption)

    if (Test-Path $imagePath) {
        # Outer Card Frame
        $frameH = if ($caption) { $h + 30 } else { $h + 8 }
        $frame = $slide.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, ($x - 4), ($y - 4), ($w + 8), $frameH)
        $frame.Fill.Solid()
        $frame.Fill.ForeColor.RGB = $COLOR_CARD_DARK
        $frame.Line.ForeColor.RGB = $COLOR_CARD_BORDER
        $frame.Line.Weight = 1.0

        # Picture
        $pic = $slide.Shapes.AddPicture($imagePath, [Microsoft.Office.Core.MsoTriState]::msoFalse, [Microsoft.Office.Core.MsoTriState]::msoTrue, $x, $y, $w, $h)
        $pic.Line.ForeColor.RGB = $COLOR_CYAN
        $pic.Line.Weight = 1.0

        if ($caption) {
            $capBox = $slide.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, $x, ($y + $h + 5), $w, 20)
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

# =============================================================================
# SLIDE 1: TRANG BIA (COVER SLIDE)
# =============================================================================
Write-Host ">>> Slide 1: Trang bia tong quan..." -ForegroundColor Yellow
$s1 = $pres.Slides.Add(1, $ppLayoutBlank)
Init-SlideBackground -slide $s1 -categoryTag $null -slideTitle $null -slideSub $null

# Ambient Glow
$glow = $s1.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeOval, 640, 60, 260, 260)
$glow.Fill.Solid()
$glow.Fill.ForeColor.RGB = To-OleColor 20 35 60
$glow.Line.ForeColor.RGB = $COLOR_CYAN
$glow.Line.Weight = 1.5

# Top Badge
$badge1 = $s1.Shapes.AddShape([Microsoft.Office.Core.MsoAutoShapeType]::msoShapeRoundedRectangle, 50, 75, 340, 28)
$badge1.Fill.Solid()
$badge1.Fill.ForeColor.RGB = $COLOR_CARD_DARK
$badge1.Line.ForeColor.RGB = $COLOR_CYAN
$badge1.TextFrame.TextRange.Text = "★ TEKLA STRUCTURES ADDIN - V2020 DEN V2026"
$badge1.TextFrame.TextRange.Font.Name = "Segoe UI"
$badge1.TextFrame.TextRange.Font.Size = 10.5
$badge1.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
$badge1.TextFrame.TextRange.Font.Color.RGB = $COLOR_CYAN
$badge1.TextFrame.TextRange.ParagraphFormat.Alignment = [Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment]::ppAlignCenter

# Main Big Title
$tBox1 = $s1.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 115, 660, 120)
$tBox1.TextFrame.TextRange.Text = "TEKLA CLASH CHECK`nREBAR VS CAU KIEN IFC"
$tBox1.TextFrame.TextRange.Font.Name = "Segoe UI"
$tBox1.TextFrame.TextRange.Font.Size = 34
$tBox1.TextFrame.TextRange.Font.Bold = [Microsoft.Office.Core.MsoTriState]::msoTrue
$tBox1.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_WHITE

# Subtitle
$subBox1 = $s1.Shapes.AddTextbox([Microsoft.Office.Core.MsoTextOrientation]::msoTextOrientationHorizontal, 50, 235, 680, 50)
$subBox1.TextFrame.TextRange.Text = "Giai phap kiem tra va cham & khoang ho an toan tu dong giua Cot thep mo hinh Tekla va cac file tham chieu IFC (Navisworks Style toc do cao)."
$subBox1.TextFrame.TextRange.Font.Name = "Segoe UI"
$subBox1.TextFrame.TextRange.Font.Size = 13.5
$subBox1.TextFrame.TextRange.Font.Color.RGB = $COLOR_TEXT_MUTED
$subBox1.TextFrame.WordWrap = [Microsoft.Office.Core.MsoTriState]::msoTrue

# 3 Feature Summary Cards on Cover
$card1Text = "• Thuat toan loc khong gian AABB 2 pha (Broad-phase & Narrow-phase).`n• Tan dung 100% tat ca cac nhan CPU da luong.`n• Quet hang nghin thanh thep chi trong vai giay."
Add-FeatureCard -slide $s1 -x 50 -y 300 -w 270 -h 175 `
    -cmdTag "SPEED" -tagColor $COLOR_CYAN `
    -title "Quet Song Song Da Luong" `
    -bodyText $card1Text

$card2Text = "• Bo qua chi tiet phu (SkipNames): Bu long, lan can, tai cau, moi han.`n• Chi quet doi tuong chi dinh (OnlyNames).`n• Ho tro tich chon dong thoi nhieu file IFC."
Add-FeatureCard -slide $s1 -x 345 -y 300 -w 270 -h 175 `
    -cmdTag "FILTER" -tagColor $COLOR_EMERALD `
    -title "Loc Thong Minh 3 Lop" `
    -bodyText $card2Text

$card3Text = "• Chuot phai hoac phim H / Delete de an dong da duyet.`n• Tu dong nhay xuong dong ke tiep.`n• Zoom can canh va ve hop lap phuong 3D mau do danh dau va cham."
Add-FeatureCard -slide $s1 -x 640 -y 300 -w 270 -h 175 `
    -cmdTag "AUDIT" -tagColor $COLOR_AMBER `
    -title "Kiem Duyet Nhanh & 3D" `
    -bodyText $card3Text

# =============================================================================
# SLIDE 2: KHOI CHAY TU RIBBON TEKLA
# =============================================================================
Write-Host ">>> Slide 2: Khoi chay Ribbon..." -ForegroundColor Yellow
$s2 = $pres.Slides.Add(2, $ppLayoutBlank)
Init-SlideBackground -slide $s2 `
    -categoryTag "BUOC 1: KHOI CHAY & KET NOI" `
    -slideTitle "Khoi chay cong cu tu Ribbon Tekla Structures" `
    -slideSub "Tich hop truc quan vao thanh cong cu Tekla Ribbon hoac chay file thuc thi doc lap"

Add-FramedImage -slide $s2 `
    -imagePath (Join-Path $ImagesDir "media_1790148432146.png") `
    -x 50 -y 125 -w 460 -h 100 `
    -caption "Vi tri nut bam Clash-check tren thanh Ribbon Tekla Structures"

$s2Card1 = "• Mo tab My-tool tren thanh Ribbon Tekla Structures.`n• Nhaps vao nut Clash-check de mo giao dien kiem tra va cham.`n• Cong cu tu dong lien ket voi Tekla Open API cua mo hinh dang mo."
Add-FeatureCard -slide $s2 -x 50 -y 255 -w 460 -h 110 `
    -cmdTag "RIBBON" -tagColor $COLOR_CYAN `
    -title "Goi truc tiep tu Ribbon Tekla" `
    -bodyText $s2Card1

$s2Card2 = "• Co the chay truc tiep file Clash-check.exe trong thu muc dist ma khong can mo Ribbon.`n• Ung dung tu dong phan giai tien trinh TeklaStructures.exe dang hoat dong de ket noi."
Add-FeatureCard -slide $s2 -x 50 -y 380 -w 460 -h 105 `
    -cmdTag "STANDALONE" -tagColor $COLOR_EMERALD `
    -title "Chay doc lap (Standalone EXE)" `
    -bodyText $s2Card2

$s2Card3 = "• Den bao xanh: [Ten Model Tekla]: Cong cu da lien ket thanh cong voi mo hinh, san sang quet.`n• Den bao do: [Chua ket noi Tekla]: Hay mo phan mem Tekla Structures va mo mot du an truoc khi su dung.`n• Tu dong nap thu vien Tekla phien ban hien hanh."
Add-FeatureCard -slide $s2 -x 535 -y 125 -w 375 -h 170 `
    -cmdTag "API STATUS" -tagColor $COLOR_AMBER `
    -title "Den Bao Trang Thai Ket Noi" `
    -bodyText $s2Card3

$s2Card4 = "• Ngay khi ket noi, cong cu tu dong quet danh muc Reference Models dang co trong du an Tekla.`n• Tu dong dien day du danh sach ten file IFC vao menu tha xuong de nguoi dung lua chon.`n• Khong can chon duong dan file thu cong."
Add-FeatureCard -slide $s2 -x 535 -y 310 -w 375 -h 175 `
    -cmdTag "REF MODELS" -tagColor $COLOR_PURPLE `
    -title "Tu Dong Nap File Tham Chieu IFC" `
    -bodyText $s2Card4

# =============================================================================
# SLIDE 3: PHAM VI THEP & CHON DA FILE IFC
# =============================================================================
Write-Host ">>> Slide 3: Pham vi thep & Chon da IFC..." -ForegroundColor Yellow
$s3 = $pres.Slides.Add(3, $ppLayoutBlank)
Init-SlideBackground -slide $s3 `
    -categoryTag "BUOC 2: THIET LAP DAU VAO" `
    -slideTitle "Thiet lap Cot thep muc tieu & Tich chon nhieu File IFC" `
    -slideSub "Linh hoat lua chon pham vi doi tuong quet va quan ly dong thoi nhieu nguon mo hinh IFC"

Add-FramedImage -slide $s3 `
    -imagePath (Join-Path $ImagesDir "media_1790152101503.png") `
    -x 50 -y 125 -w 270 -h 360 `
    -caption "Menu tha xuong tich chon linh hoat nhieu file IFC"

$s3Card1 = "• Thep dang chon: Chi quet cac thanh cot thep hoac cau kien be tong dang duoc chon truc tiep trong man hinh Tekla 3D. Thich hop kiem tra nhanh tung vung/tung cau kien.`n• Toan bo thep: Tu dong quet toan bo cot thep trong toan bo mo hinh du an."
Add-FeatureCard -slide $s3 -x 345 -y 125 -w 565 -h 110 `
    -cmdTag "REBAR SCOPE" -tagColor $COLOR_CYAN `
    -title "Lua Chon Pham Vi Cot Thep Quet" `
    -bodyText $s3Card1

$s3Card2 = "• Ho tro tich chon dong thoi 1, 2 hoac nhieu file IFC cung luc (vi du: vua quet file MEP, vua quet file Ket cau thep).`n• Tich hop thanh tim kiem go nhanh ten file de loc danh sach.`n• Nut Chon tat ca va Bo chon het giup thao tac cuc ky nhanh chong."
Add-FeatureCard -slide $s3 -x 345 -y 250 -w 565 -h 115 `
    -cmdTag "MULTI IFC" -tagColor $COLOR_EMERALD `
    -title "Tich Chon Dong Thoi Nhieu File IFC" `
    -bodyText $s3Card2

$s3Card3 = "• Tat ca file IFC (Navisworks Auto): Tu dong tim tat ca cac file IFC cat qua vung hop bao AABB cua thep.`n• Chi cau kien IFC dang chon: Chi quet cac Part hoac Reference Object dang duoc pick chon tren Tekla 3D."
Add-FeatureCard -slide $s3 -x 345 -y 380 -w 565 -h 105 `
    -cmdTag "SPECIAL MODES" -tagColor $COLOR_AMBER `
    -title "Cac Che Do Quet IFC Nang Cao" `
    -bodyText $s3Card3

# =============================================================================
# SLIDE 4: DUNG SAI & KHOANG HO AN TOAN
# =============================================================================
Write-Host ">>> Slide 4: Dung sai & Khoang ho..." -ForegroundColor Yellow
$s4 = $pres.Slides.Add(4, $ppLayoutBlank)
Init-SlideBackground -slide $s4 `
    -categoryTag "BUOC 3: THIET LAP THUAT TOAN" `
    -slideTitle "Dung sai va cham (Tolerance) & Khoang ho an toan (Clearance)" `
    -slideSub "Cau hinh chuan xac de loai bo va cham gia va phat hien xam pham khoang cach an toan"

Add-FramedImage -slide $s4 `
    -imagePath (Join-Path $ImagesDir "media_1790150641284.png") `
    -x 50 -y 125 -w 860 -h 100 `
    -caption "Cum tham so Dung sai (mm) va Khoang ho (mm) tren thanh dieu khien"

$s4Card1 = "• Dinh nghia: Do xuyen thau chap nhan duoc giua thanh thep va cau kien IFC.`n• Co che: Neu do lan (overlap) nho hon hoac bang Dung sai, cong cu se KHONG bao va cham.`n• Khuyen dung: Dat 1 mm de loai bo sai so tinh toan lam tron so hoc be mat 3D."
Add-FeatureCard -slide $s4 -x 50 -y 255 -w 420 -h 130 `
    -cmdTag "TOLERANCE" -tagColor $COLOR_CYAN `
    -title "Dung sai (mm) - Bo qua cham nhe mep" `
    -bodyText $s4Card1

$s4Card2 = "• Dinh nghia: Khoang cach toi thieu bat buoc giua mep ngoai cot thep va cau kien IFC.`n• Co che: Neu 2 doi tuong khong cham nhau nhung khoang cach < Khoang ho, van bao va cham.`n• Khuyen dung: Dat khi can dam bao chieu day lop be tong bao ve hoac chong chap ong MEP."
Add-FeatureCard -slide $s4 -x 490 -y 255 -w 420 -h 130 `
    -cmdTag "CLEARANCE" -tagColor $COLOR_AMBER `
    -title "Khoang ho (mm) - Vung dem an toan" `
    -bodyText $s4Card2

$s4Card3 = "• Severe (Mau do): Va cham nghiem trong - do lan sau vao than cau kien (> 20mm).`n• Medium (Mau cam): Va cham muc trung binh (tu 5mm den 20mm).`n• Minor (Mau vang): Va cham nhe mep hoac xam pham khoang ho an toan (< 5mm)."
Add-FeatureCard -slide $s4 -x 50 -y 400 -w 860 -h 90 `
    -cmdTag "SEVERITY" -tagColor $COLOR_ROSE `
    -title "Phan Cap Muc Do Va Cham Truc Quan Theo Mau Sac" `
    -bodyText $s4Card3

# =============================================================================
# SLIDE 5: BO LOC SKIPNAMES & ONLYNAMES
# =============================================================================
Write-Host ">>> Slide 5: Bo loc thong minh..." -ForegroundColor Yellow
$s5 = $pres.Slides.Add(5, $ppLayoutBlank)
Init-SlideBackground -slide $s5 `
    -categoryTag "BUOC 4: BO LOC CAU KIEN" `
    -slideTitle "Bo loc thong minh 3 lop: SkipNames & OnlyNames" `
    -slideSub "Loai tru triet de cau kien phu va tap trung kiem tra chinh xac cac doi tuong ket cau quan trong"

Add-FramedImage -slide $s5 `
    -imagePath (Join-Path $ImagesDir "media_1790151482316.png") `
    -x 50 -y 125 -w 340 -h 140 `
    -caption "Danh sach tu khoa bo qua (SkipNames) tu dong luu"

$s5Card1 = "• Bo qua cac chi tiet khong can kiem va cham voi thep: bu long, lan can tam, tai cau, thang leo, moi han...`n• Danh sach mac dinh: Bolt assembly, SAFETY_BAR, LUG, LADDER, SAFETY_HOOK, VBRACE, WELD_COUPLER, CHECK_COUPLER.`n• Ho tro tu khoa cach nhau boi dau cach, xuong dong hoac dau phay."
Add-FeatureCard -slide $s5 -x 415 -y 125 -w 495 -h 140 `
    -cmdTag "SKIPNAMES" -tagColor $COLOR_EMERALD `
    -title "Luoc bo cau kien phu (SkipNames)" `
    -bodyText $s5Card1

$s5Card2 = "1. Tang 1: Loc ngay khi trich xuat file IFC trong IfcConvert C++ Engine (tiet kiem 60% bo nho RAM).`n2. Tang 2: Loc o Broad-phase khi phan tich hop bao AABB.`n3. Tang 3: Loc o Narrow-phase dam bao 100% khong sot cau kien nao nam trong SkipNames.`n• Nut Mac dinh: Khoi phuc danh sach tu khoa chuan ban dau."
Add-FeatureCard -slide $s5 -x 50 -y 290 -w 420 -h 190 `
    -cmdTag "3-LAYER FILTER" -tagColor $COLOR_CYAN `
    -title "Co Che Loc 3 Tang Triet De" `
    -bodyText $s5Card2

$s5Card3 = "• Khi tich chon, cong cu CHI kiem tra va cham voi cac cau kien IFC co ten chua tu khoa chi dinh.`n• Vi du dien hinh: Nhap BEAM, COLUMN, SLAB, WALL, PIPE khi chi muon quet voi Dam, Cot, San hoac Duong ong chinh.`n• Nut Xoa trang: De dang lam trong de chuyen ve che do quet tat ca."
Add-FeatureCard -slide $s5 -x 490 -y 290 -w 420 -h 190 `
    -cmdTag "ONLYNAMES" -tagColor $COLOR_PURPLE `
    -title "Chi Quet Cau Kien Chi Dinh (OnlyNames)" `
    -bodyText $s5Card3

# =============================================================================
# SLIDE 6: BANG KET QUA & TUONG TAC 3D TEKLA
# =============================================================================
Write-Host ">>> Slide 6: Bang ket qua & 3D..." -ForegroundColor Yellow
$s6 = $pres.Slides.Add(6, $ppLayoutBlank)
Init-SlideBackground -slide $s6 `
    -categoryTag "BUOC 5: TRUC QUAN HOA & BAO CAO" `
    -slideTitle "Bang ket qua thong minh & Tuong tac 3D hai chieu voi Tekla" `
    -slideSub "Truc quan hoa tuc thi moi thong so va cham va lien ket camera 3D chuan xac"

Add-FramedImage -slide $s6 `
    -imagePath (Join-Path $ImagesDir "media_1790152662775.png") `
    -x 50 -y 125 -w 860 -h 200 `
    -caption "Bang ket qua va cham chi tiet voi phan cap mau sac muc do (Severe / Medium / Minor)"

$s6Card1 = "• Nhap dup chuot vao bat ky dong nao trong bang (hoac bam nut Zoom & Chon).`n• Camera 3D Tekla tu dong lia sat vao diem va cham.`n• Thanh thep bi va cham duoc tu dong Select noi bat tren mo hinh."
Add-FeatureCard -slide $s6 -x 50 -y 350 -w 270 -h 135 `
    -cmdTag "DOUBLE CLICK" -tagColor $COLOR_CYAN `
    -title "Zoom & Chon Cau Kien" `
    -bodyText $s6Card1

$s6Card2 = "• Bam nut Danh dau 3D: Ve khoi hop lap phuong 3D mau do noi bat tai tat ca diem va cham.`n• Dau cheo tren nap hop de quan sat tu tren cao.`n• Bam Xoa 3D: Don sach tuc thi ma khong giat lag man hinh."
Add-FeatureCard -slide $s6 -x 345 -y 350 -w 270 -h 135 `
    -cmdTag "GRAPHICS 3D" -tagColor $COLOR_AMBER `
    -title "Danh Dau Hop Lap Phuong" `
    -bodyText $s6Card2

$s6Card3 = "• Bam nut Xuat bao cao Excel/CSV.`n• Xuat bang ma UTF-8 chuan tieng Viet.`n• Day du 14 cot du lieu: ID thep, Mac thep, Chieu dai, Do lan (mm), Cau kien Part, Cau kien IFC, Toa do X, Y, Z."
Add-FeatureCard -slide $s6 -x 640 -y 350 -w 270 -h 135 `
    -cmdTag "REPORT" -tagColor $COLOR_EMERALD `
    -title "Xuat Bao Cao Excel/CSV" `
    -bodyText $s6Card3

# =============================================================================
# SLIDE 7: CHUOT PHAI AN DONG & PHIM TAT KIEM DUYET
# =============================================================================
Write-Host ">>> Slide 7: Chuot phai an dong..." -ForegroundColor Yellow
$s7 = $pres.Slides.Add(7, $ppLayoutBlank)
Init-SlideBackground -slide $s7 `
    -categoryTag "TINH NANG MOI: KIEM DUYET HANG LOAT" `
    -slideTitle "Chuot phai An dong & Phim tat H / Delete kiem tra mot luot" `
    -slideSub "Quy trinh kiem duyet muot ma, loai bo tuc thi cac diem da kiem tra va tu dong chuyen dong"

$s7Card1 = "• Nhap chuot phai vao bat ky o hoac dong nao trong bang de hien thi menu:`n  - An dong nay (Da kiem tra xong) [Phim H / Delete]`n  - Zoom & Chon cau kien tren Tekla`n  - Hien lai tat ca cac dong da an (x dong)`n  - Sao chep thong tin dong va cham (Copy)`n• Chuot phai lap tuc chon ngay dong do, thao tac truc quan."
Add-FeatureCard -slide $s7 -x 50 -y 125 -w 420 -h 170 `
    -cmdTag "RIGHT CLICK" -tagColor $COLOR_AMBER `
    -title "Menu Ngu Canh Chuot Phai" `
    -bodyText $s7Card1

$s7Card2 = "• Thay vi phai click chuot, ban chi can nhan phim H (Hide) hoac phim Delete tren ban phim.`n• Dong va cham vua xem xet se lap tuc duoc an di.`n• Con tro tu dong nhay xuong dong hien thi ke tiep, giup ban duyet 1 luot 100+ diem va cham chi trong vai phut ma khong can click chon lai!"
Add-FeatureCard -slide $s7 -x 490 -y 125 -w 420 -h 170 `
    -cmdTag "HOTKEYS" -tagColor $COLOR_CYAN `
    -title "Phim Tat H & Delete - Duyet Sieu Toc" `
    -bodyText $s7Card2

$s7Card3 = "• Thanh trang thai goc duoi tu dong cap nhat so luong dong con lai:`n  Vi du: '5 con lai / 6 tong (Da an 1)'`n• Giup ky su nam bat chinh xac tien do xu ly va so luong va cham con ton dong.`n• Da xu ly an toan CurrencyManager WinForms, khong bao gio loi khi an dong hien hanh."
Add-FeatureCard -slide $s7 -x 50 -y 310 -w 420 -h 175 `
    -cmdTag "COUNTER" -tagColor $COLOR_EMERALD `
    -title "Bo Dem Tien Do Thoi Gian Thuc" `
    -bodyText $s7Card3

$s7Card4 = "• Khi muon xem lai toan bo ket qua ban dau, nhap chuot phai va chon:`n  Hien lai tat ca cac dong da an (x dong)`n• Bang lap tuc hoan tra day du tat ca cac dong va cham nguyen ven.`n• Menu chuot phai tu dong hien thi so luong dong dang bi an de ban tien theo doi."
Add-FeatureCard -slide $s7 -x 490 -y 310 -w 420 -h 175 `
    -cmdTag "RESTORE" -tagColor $COLOR_PURPLE `
    -title "Khoi Phuc Danh Sach Da An De Dang" `
    -bodyText $s7Card4

# =============================================================================
# SLIDE 8: LUU CAU HINH TU DONG & LUU Y KY THUAT
# =============================================================================
Write-Host ">>> Slide 8: Luu cau hinh & Luu y..." -ForegroundColor Yellow
$s8 = $pres.Slides.Add(8, $ppLayoutBlank)
Init-SlideBackground -slide $s8 `
    -categoryTag "CAU HINH & LUU Y VAN HANH" `
    -slideTitle "Luu cai dat tu dong (Properties.Settings) & Khac phuc tinh huong" `
    -slideSub "Tu dong ghi nho cau hinh khi tat/mo va cac luu y quan trong de quet dat hieu qua cao nhat"

Add-FramedImage -slide $s8 `
    -imagePath (Join-Path $ImagesDir "media_1790134776056.png") `
    -x 50 -y 125 -w 340 -h 240 `
    -caption "Luu y bat hien thi Reference Model IFC tren Tekla"

$s8Card1 = "• Moi thong so dieu khien deu duoc tu dong luu vinh vien vao Properties.Settings:`n  - Pham vi thep (Dang chon / Toan bo).`n  - Dung sai (mm) & Khoang ho (mm).`n  - Bat/tat va noi dung tu khoa SkipNames & OnlyNames.`n  - Danh sach cac file IFC da tich chon.`n  - Kich thuoc cua so Form va trang thai phong to (Maximized).`n• Khi mo lai Form, moi thiet lap duoc khoi phuc nguyen ven 100%."
Add-FeatureCard -slide $s8 -x 415 -y 125 -w 495 -h 170 `
    -cmdTag "AUTO SAVE" -tagColor $COLOR_CYAN `
    -title "Tu Dong Luu & Nap Cau Hinh (Properties.Settings)" `
    -bodyText $s8Card1

$s8Card2 = "• Trang thai Reference Model: File IFC phai duoc chen vao Tekla Structures va bieu tuong con mat hien thi phai o trang thai BAT (Visible).`n• Dong goi gui nguoi khac: Chi can gui tron bo thu muc dist/ClashCheck_Tekla2020 hoac file nen ClashCheck_Tekla2020.zip (chua day du Clash-check.exe va cac file DLL phu thuoc, khong can cai dat them).`n• Tuong thich muot ma tren Tekla Structures 2020, 2025, 2026."
Add-FeatureCard -slide $s8 -x 415 -y 310 -w 495 -h 175 `
    -cmdTag "BEST PRACTICES" -tagColor $COLOR_EMERALD `
    -title "Nhung Luu Y Ky Thuat Quan Trong Khi Su Dung" `
    -bodyText $s8Card2

$s8Card3 = "• File nen chia se: dist\ClashCheck_Tekla2020.zip`n• Slide huong dan: docs\Huong_Dan_Su_Dung_Clash_Check.pptx"
Add-FeatureCard -slide $s8 -x 50 -y 390 -w 340 -h 95 `
    -cmdTag "PACKAGE" -tagColor $COLOR_AMBER `
    -title "Vi Tri File Dong Goi" `
    -bodyText $s8Card3

# =============================================================================
# LUU VÀ XUAT FILE POWERPOINT (.PPTX) VÀ PDF
# =============================================================================
Write-Host ">>> Dang luu file PowerPoint..." -ForegroundColor Cyan
if (Test-Path $OutputPptxDocs) { Remove-Item $OutputPptxDocs -Force }
$pres.SaveAs($OutputPptxDocs)

# Luu them 1 ban vao thu muc dist de di kem bo cong cu gui cho nguoi dung
if (Test-Path $OutputPptxDist) { Remove-Item $OutputPptxDist -Force }
$pres.SaveCopyAs($OutputPptxDist)

# Dong thoi xuat them 1 ban dinh dang PDF tien xem nhanh tren dien thoai/may tinh
$OutputPdf = [System.IO.Path]::ChangeExtension($OutputPptxDocs, ".pdf")
$ppSaveAsPDF = 32
$pres.SaveAs($OutputPdf, $ppSaveAsPDF)

Write-Host ">>> Xuat file hoan tat!" -ForegroundColor Green
Write-Host "PPTX Docs: $OutputPptxDocs" -ForegroundColor Green
Write-Host "PPTX Dist: $OutputPptxDist" -ForegroundColor Green
Write-Host "PDF Docs : $OutputPdf" -ForegroundColor Green

$pres.Close()
$pptApp.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($pres) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($pptApp) | Out-Null
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()
