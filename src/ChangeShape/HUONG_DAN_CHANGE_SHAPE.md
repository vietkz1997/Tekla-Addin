# 🚀 HƯỚNG DẪN SỬ DỤNG CÔNG CỤ CHANGE-SHAPE PRO v2.0
> **Rebar Shape Studio & AutoCAD Live Interactive Controller**  
> *Chuẩn Thống Kê & Bẻ Thép SIDD-SHOP / Hàn Quốc*  
> **Tác giả:** BIM / Antigravity Automation Suite  
> **Vị trí cài đặt:** `d:\VIET\00.INFORMATION\00LISP\cad\My-tool\Change-shape\`

---

## 🌟 1. VÌ SAO CÔNG CỤ NÀY ĐỘC ĐÁO & VƯỢT TRỘI?

Trước đây, khi dùng các Lisp thông thường (như `CF1`), người dùng phải:
- Nhập mù mã shape và kích thước qua dòng lệnh Command Line AutoCAD (`Nhap A: 600`, `Nhap B: 400`, `Nhap C: 120`) mà **không nhìn thấy hình dáng thanh thép**.
- Rất dễ nhầm lẫn giữa cạnh A, cạnh B hay chiều móc C.
- Phải tự nhẩm tính trừ lớp bê tông bảo vệ.

**✨ `CHANGE-SHAPE PRO` giải quyết triệt để và mang lại trải nghiệm hoàn toàn mới:**
1. **📐 Vector Canvas 2D Live Preview thời gian thực:**
   - Khi chọn bất kỳ Shape nào (vd: **Shape 56**), hình vẽ 2D hiển thị trực quan ngay giữa màn hình.
   - Bạn gõ hoặc chỉnh kích thước $A, B, C$, hình vẽ thanh thép **tự động co giãn theo đúng tỷ lệ**, hiển thị cả 2 tai móc 135 độ và **gắn đường kích thước Dimension trực quan** (Dim $A, B, C$).
2. **🔄 Kết nối 2 chiều trực tiếp với AutoCAD (COM Interop):**
   - Nút **`[🖱️ BẮT THANH THÉP TỪ CAD]`**: Click 1 cái vào chữ MText trên CAD (vd thanh thẳng số 1: `2-HD25 6200 @200`), tool lập tức đọc được số lượng, đường kính, chiều dài và tiền tố.
   - Nút **`[📏 Đo CAD]`**: Cần đai vừa khít dầm bê tông? Bấm nút thước đo cạnh ô $A$ hoặc $B$, click 2 điểm trên bản vẽ CAD là số đo tự nhảy vào ô!
   - Nút **`[🚀 ÁP DỤNG ĐỔI SHAPE]`**: 1 Click là MText trên CAD cập nhật ngay chuẩn SIDD-SHOP:
     - Dòng 1: `2-HD25 2240 @200`
     - Dòng 2: `{\H0.8x;\C1;56,600,400,120}`
3. **⚡ Đổi Hàng Loạt (Batch Selection):**
   - Quét chọn hàng chục thanh thép số 1 cùng lúc, tất cả lập tức biến thành Shape 56 với kích thước đã chọn!
4. **✏️ Tùy chọn "Vẽ Polyline Thật Vào CAD":**
   - Không chỉ sửa chữ MText, tool còn cho phép bấm nút để vẽ luôn hình học Polyline thanh thép đai chuẩn xác vào bản vẽ CAD.
5. **💡 Bộ Tính Đai Từ Tiết Diện Bê Tông (Section Converter):**
   - Nhập tiết diện Bê tông $W \times H$ (vd: $300 \times 500$) và Lớp bảo vệ (vd: $30\text{ mm}$), bấm **`[⚡ TÍNH ĐAI]`** là tự động điền ngay $A = 240$, $B = 440$, $C = 120$!
6. **⭐ Thư Viện Mẫu (Presets) 1-Click:**
   - Có sẵn các kích thước chuẩn cho Dầm $200\times350, 300\times500, 300\times600, 400\times700$, Cột $400\times400, 500\times500, 600\times600$...
   - Hỗ trợ nút **`[➕ Lưu Mẫu Này]`** để lưu vĩnh viễn cấu hình riêng của bạn.

---

## 🖥️ 2. CÁCH KHỞI ĐỘNG CÔNG CỤ

### Cách 1: Khởi động trực tiếp từ AutoCAD (Khuyên Dùng)
- Trong AutoCAD, gõ lệnh:
  ```text
  CS
  ```
  *(hoặc `CSH` / `CHANGESHAPE`)*
- Nếu bạn đang quét chọn sẵn 1 thanh thép trước khi gõ `CS`, công cụ sẽ tự động mở lên và nhận diện ngay thanh thép đó!

### Cách 2: Mở trực tiếp file `.exe`
- Nhấp đúp vào file:
  `d:\VIET\00.INFORMATION\00LISP\cad\My-tool\Change-shape\Change-shape.exe`
- Bật nút **`[📌 Ghim Trên Cùng]`** ở góc trên để cửa sổ luôn nổi bên cạnh AutoCAD mà không bị che khuất.

---

## 📖 3. HƯỚNG DẪN TỪNG BƯỚC: ĐỔI THANH THÉP SỐ 1 THÀNH SHAPE 56

### Bước 1: Bắt thanh thép từ CAD
1. Nhấn nút **`[🖱️ BẮT THANH THÉP TỪ CAD]`** (hoặc phím tắt **`Ctrl + P`**).
2. Chuột sẽ chuyển sang màn hình AutoCAD: Click chọn thanh MText số 1 (ví dụ: `2-HD25 6200 @200`).
3. Tool nhận diện ngay: Tiền tố `2-HD25`, đường kính `D25`, chiều dài hiện tại `6200`.

### Bước 2: Chọn Shape 56 & Nhập kích thước
1. Tại danh sách bên trái, chọn **`Shape 56 - Đai Kín 135°`**.
2. Nhập các kích thước mong muốn:
   - **Ô A (Rộng mm):** Nhập chiều rộng đai (ví dụ: `600`).
   - **Ô B (Cao mm):** Nhập chiều cao đai (ví dụ: `400`).
   - **Ô C (Móc 135° mm):** Nhập chiều dài tai móc (ví dụ: `120`) hoặc bấm nút **`⚡`** để tự động tính theo $10d$.
   *(Hoặc chọn nhanh ở danh sách Preset bên phải, hoặc dùng bộ tính từ tiết diện bê tông dầm)*
3. **Quan sát Live Canvas:** Hình đai 56 sẽ co giãn đúng tỷ lệ $600 \times 400 \times 120$ và bảng tổng chiều dài nhảy số tức thì:
   $$L = 2 \times (600 + 400) + 2 \times 120 = 2,240\text{ mm}$$

### Bước 3: Áp dụng vào CAD
- Nhấn nút **`[🚀 ÁP DỤNG ĐỔI SHAPE]`** (hoặc nhấn phím **`Enter`**).
- MText trên AutoCAD sẽ được cập nhật ngay lập tức thành:
  ```text
  2-HD25 2240 @200
  {56,600,400,120}  (dòng đỏ chuẩn SIDD-SHOP)
  ```
- *(Tùy chọn)* Nếu bạn muốn vẽ cả hình học Polyline ra bản vẽ: Bấm nút **`[✏️ Vẽ Polyline]`** (hoặc **`Ctrl + D`**) và click 1 điểm đặt trên bản vẽ CAD!

---

## ⌨️ 4. PHÍM TẮT HỖ TRỢ NHANH
| Phím tắt | Chức năng |
| :--- | :--- |
| **`Enter`** | Áp dụng thay đổi mã Shape và kích thước vào CAD |
| **`Ctrl + P`** | Bắt đối tượng thanh thép từ AutoCAD (Pick rebar) |
| **`Ctrl + D`** | Vẽ đường Polyline thanh thép vào CAD |
| **`F5`** | Làm mới kết nối AutoCAD |
