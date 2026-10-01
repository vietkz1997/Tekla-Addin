# HƯỚNG DẪN BỘ MÔI TRƯỜNG TEKLA STRUCTURES VIỆT NAM (TCVN)
**Hỗ trợ đa phiên bản:** Tekla Structures **2020.0** và **2025.0**

---

## 1. Cấu trúc thư mục phân phối trong `dist/`

```text
dist/
├── Environments/
│   ├── Vietnam/                      # Thư mục gốc dùng chung
│   ├── Vietnam_2020/                 # Cấu hình tối ưu cho Tekla 2020.0
│   └── Vietnam_2025/                 # Cấu hình tối ưu cho Tekla 2025.0
├── Vietnam_Environment_Tekla2020.zip # File nén cài đặt nhanh cho Tekla 2020.0
├── Vietnam_Environment_Tekla2025.zip # File nén cài đặt nhanh cho Tekla 2025.0
├── install_vietnam_environment.bat   # Script tự động cài đặt 1-Click (hỗ trợ cả 2020 & 2025)
└── README_VIETNAM_ENVIRONMENT.md     # Tài liệu hướng dẫn này
```

---

## 2. Điểm cải tiến và tối ưu riêng cho Tekla Structures 2025

So với phiên bản 2020, gói **Vietnam_2025** được bổ sung các cấu hình tiên tiến nhất của Tekla 2025:
* **Kích hoạt Pour Management:** Tự động quản lý phân đoạn đổ bê tông, Pour Phase, Pour Units phục vụ tiến độ thi công.
* **Tối ưu hóa Rebar Sets:** Bật tính năng ghi nhãn thép uốn tự động (Pull-out picture offset), tương thích hoàn hảo với cơ sở dữ liệu `rebar_database.inp` theo TCVN 1651:2018.
* **DirectX Fast Rendering:** Kích hoạt tính năng dựng hình 3D tốc độ cao bằng card đồ họa GPU rời.
* **Tương thích đường dẫn mới:** Tự động nhận diện cả đường dẫn truyền thống `C:\TeklaStructures\2025.0\` lẫn đường dẫn hệ thống mới `C:\ProgramData\Trimble\Tekla Structures\2025.0\`.

---

## 3. Các quy chuẩn TCVN được tích hợp

### A. Cốt thép theo TCVN 1651:2018 & TCVN 5574:2018
* **Thép tròn trơn CB240-T:** Đường kính $\phi 6, \phi 8, \phi 10$.
* **Thép thanh vằn CB300-V, CB400-V, CB500-V:** Đầy đủ dải đường kính $\phi 10, \phi 12, \phi 14, \phi 16, \phi 18, \phi 20, \phi 22, \phi 25, \phi 28, \phi 32, \phi 36, \phi 40$.
* **Thép dự án FDI:** `SD295`, `SD390` (JIS G3112).
* **Quy cách gia công:** Trọng lượng đơn vị lý thuyết ($kg/m$), diện tích ($mm^2$), bán kính uốn chuẩn TCVN, chiều dài móc đai $135^\circ$ và móc neo $90^\circ, 180^\circ$.
* **Chiều dài thanh thương mại tối đa:** Đã cấu hình `XS_MAX_REBAR_LENGTH=11700` ($11.7m$ theo chuẩn nhà máy Hòa Phát, Pomina, Vina Kyoei).

### B. Mác Bê tông theo TCVN 5574:2018 & Mác truyền thống
* **Cấp độ bền B:** `B10` (bê tông lót), `B15`, `B20`, `B22.5`, `B25`, `B30`, `B35`, `B40`, `B45`, `B50`, `B60`.
* **Mác bê tông M:** `M100`, `M150`, `M200`, `M250`, `M300`, `M350`, `M400`.
* Khối lượng thể tích $2500 kg/m^3$ cho BTCT, $2400 kg/m^3$ cho bê tông lót và Modulus đàn hồi $E_b$ theo TCVN 5574:2018.

### C. Thép kết cấu (Structural Steel)
* Tích hợp các mác thép tấm và thép hình phổ biến tại Việt Nam: `SS400`, `Q235`, `Q345`, `Q355`, `CT3` (TCVN 5709), `SM490`.

---

## 4. Hướng dẫn cài đặt

### Cách 1: Cài đặt tự động bằng Menu (Khuyến nghị)
1. Chạy file [`install_vietnam_environment.bat`](file:///c:/Users/BIM/Documents/Github/Tekla-Addin/dist/install_vietnam_environment.bat) với quyền **Run as administrator**.
2. Chọn phiên bản bạn muốn cài:
   * Nhập `1`: Cài cho **Tekla Structures 2020.0**
   * Nhập `2`: Cài cho **Tekla Structures 2025.0**
   * Nhập `3`: Cài cho **CẢ HAI phiên bản** (2020 & 2025)

### Cách 2: Cài đặt thủ công bằng file ZIP
* Đối với Tekla 2020: Giải nén file [`Vietnam_Environment_Tekla2020.zip`](file:///c:/Users/BIM/Documents/Github/Tekla-Addin/dist/Vietnam_Environment_Tekla2020.zip) vào thư mục:
  `C:\TeklaStructures\2020.0\Environments\Vietnam\`
* Đối với Tekla 2025: Giải nén file [`Vietnam_Environment_Tekla2025.zip`](file:///c:/Users/BIM/Documents/Github/Tekla-Addin/dist/Vietnam_Environment_Tekla2025.zip) vào thư mục:
  `C:\ProgramData\Trimble\Tekla Structures\2025.0\Environments\Vietnam\`
  *(hoặc `C:\TeklaStructures\2025.0\Environments\Vietnam\` tùy cách cài đặt máy của bạn)*

---

## 5. Khởi động Tekla Structures
1. Mở Tekla Structures (2020 hoặc 2025).
2. Tại màn hình chọn cấu hình:
   * **Environment:** Chọn **Vietnam**
   * **Role:** Chọn **Cast-in-Place** (Bê tông), **Steel Detailing** (Thép), **Engineering** (Kỹ thuật), hoặc **All**.
3. Bắt đầu mô hình hóa theo đúng chuẩn TCVN!
