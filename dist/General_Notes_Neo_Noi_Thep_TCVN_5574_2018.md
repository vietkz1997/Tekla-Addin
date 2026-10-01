# QUY ĐỊNH CHI TIẾT VỀ NEO VÀ NỐI CỐT THÉP (GENERAL NOTES)
## ÁP DỤNG CHO CÔNG TRÌNH BÊ TÔNG CỐT THÉP THEO TCVN 5574:2018 & TCVN 1651:2018

---

## 1. NGUYÊN TẮC CƠ BẢN VỀ NEO CỐT THÉP (ANCHORAGE)

Theo **Điều 10.3.5 - TCVN 5574:2018**, chiều dài neo cơ sở $L_{0,an}$ được tính theo công thức:
$$L_{0,an} = \frac{R_s \cdot A_s}{R_{bond} \cdot u_s} = \frac{R_s \cdot d}{4 \cdot \eta_1 \cdot \eta_2 \cdot R_{bt}}$$

Trong đó:
- $R_s$: Cường độ chịu kéo tính toán của cốt thép (MPa).
- $R_{bt}$: Cường độ chịu kéo dọc trục tính toán của bê tông (MPa).
- $\eta_1$: Hệ số bám dính của cốt thép ($\eta_1 = 2.5$ đối với cốt thép gân cán nóng CB300-V, CB400-V, CB500-V; $\eta_1 = 1.5$ với thép trơn CB240-T).
- $\eta_2$: Hệ số đường kính ($\eta_2 = 1.0$ khi $d \le 32\text{ mm}$; $\eta_2 = 0.9$ khi $d > 32\text{ mm}$).

### Chiều dài đoạn neo tính toán $L_{an}$:
$$L_{an} = \alpha_{an} \cdot L_{0,an} \cdot \frac{A_{s,cal}}{A_{s,ef}}$$
- Thanh thẳng không móc uốn: $\alpha_{an} = 1.0$
- Thanh uốn móc chữ L vuông góc $90^\circ$: $\alpha_{an} = 0.7$
- **Giới hạn tối thiểu:** $L_{an} \ge 0.3 \cdot L_{0,an}$, đồng thời $L_{an} \ge 15d$ và $L_{an} \ge 200\text{ mm}$.

---

## 2. NGUYÊN TẮC VỀ NỐI CHỒNG CỐT THÉP (LAP SPLICE)

Theo **Điều 10.3.6 - TCVN 5574:2018**, chiều dài đoạn nối chồng buộc cốt thép $L_l$ (hoặc $L_{lap}$) được tính:
$$L_{lap} = \alpha_l \cdot L_{0,an} \cdot \frac{A_{s,cal}}{A_{s,ef}}$$

### Hệ số $\alpha_l$ theo vị trí và tỷ lệ nối:
| Trường hợp | Tỷ lệ nối tại 1 mặt cắt | Hệ số $\alpha_l$ |
| :--- | :---: | :---: |
| Vùng chịu kéo | $\le 25\%$ | $1.2$ |
| Vùng chịu kéo | $\le 50\%$ | $1.3$ |
| Vùng chịu kéo | $100\%$ | $2.0$ |
| Vùng chịu nén | $\le 50\%$ | $0.9$ |
| Vùng chịu nén | $100\%$ | $1.2$ |

- **Giới hạn tối thiểu:** $L_{lap} \ge 0.4 \cdot \alpha_l \cdot L_{0,an}$, $L_{lap} \ge 20d$ và $L_{lap} \ge 250\text{ mm}$.
- Khoảng cách so le giữa 2 mặt cắt nối chồng: tối thiểu $\ge 1.3 \cdot L_{lap}$ (hoặc $\ge 500\text{ mm}$).

---

## 3. BẢNG TRA CHIỀU DÀI NEO VÀ NỐI THEO TCVN 5574:2018 (ĐƠN VỊ: MM)

> *Bảng tra áp dụng cho trường hợp an toàn chung (lấy diện tích tính toán $A_{s,cal} = A_{s,ef}$, tỷ lệ nối $\le 50\%$ so le):*

### Bê tông B25 ($R_{bt} = 1.05\text{ MPa}$, $R_b = 14.5\text{ MPa}$):
| Nhóm thép | Trạng thái ứng suất | Công thức kinh nghiệm | d10 | d12 | d14 | d16 | d18 | d20 | d22 | d25 | d28 | d32 |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **CB240-T** | Neo kéo (thẳng có móc U) | $25d$ | 250 | 300 | 350 | 400 | 450 | 500 | 550 | 630 | 700 | 800 |
| | Nối kéo ($\le 50\%$) | $35d$ | 350 | 420 | 490 | 560 | 630 | 700 | 770 | 880 | 980 | 1120 |
| | Nối nén | $25d$ | 250 | 300 | 350 | 400 | 450 | 500 | 550 | 630 | 700 | 800 |
| **CB300-V** | Neo kéo thẳng | $35d$ | 350 | 420 | 490 | 560 | 630 | 700 | 770 | 880 | 980 | 1120 |
| | Neo kéo có móc $90^\circ$ | $25d$ | 250 | 300 | 350 | 400 | 450 | 500 | 550 | 630 | 700 | 800 |
| | Nối kéo ($\le 50\%$) | $40d$ | 400 | 480 | 560 | 640 | 720 | 800 | 880 | 1000 | 1120 | 1280 |
| | Nối kéo ($100\%$) | $50d$ | 500 | 600 | 700 | 800 | 900 | 1000 | 1100 | 1250 | 1400 | 1600 |
| | Nối nén | $30d$ | 300 | 360 | 420 | 480 | 540 | 600 | 660 | 750 | 840 | 960 |
| **CB400-V** | Neo kéo thẳng | $40d$ | 400 | 480 | 560 | 640 | 720 | 800 | 880 | 1000 | 1120 | 1280 |
| | Neo kéo có móc $90^\circ$ | $30d$ | 300 | 360 | 420 | 480 | 540 | 600 | 660 | 750 | 840 | 960 |
| | Nối kéo ($\le 50\%$) | $45d$ | 450 | 540 | 630 | 720 | 810 | 900 | 990 | 1130 | 1260 | 1440 |
| | Nối kéo ($100\%$) | $55d$ | 550 | 660 | 770 | 880 | 990 | 1100 | 1210 | 1380 | 1540 | 1760 |
| | Nối nén | $35d$ | 350 | 420 | 490 | 560 | 630 | 700 | 770 | 880 | 980 | 1120 |

---

### Bê tông B30 ($R_{bt} = 1.15\text{ MPa}$, $R_b = 17.0\text{ MPa}$):
| Nhóm thép | Trạng thái ứng suất | Công thức kinh nghiệm | d10 | d12 | d14 | d16 | d18 | d20 | d22 | d25 | d28 | d32 |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **CB400-V** | Neo kéo thẳng | $35d$ | 350 | 420 | 490 | 560 | 630 | 700 | 770 | 880 | 980 | 1120 |
| | Neo kéo có móc $90^\circ$ | $25d$ | 250 | 300 | 350 | 400 | 450 | 500 | 550 | 630 | 700 | 800 |
| | Nối kéo ($\le 50\%$) | $40d$ | 400 | 480 | 560 | 640 | 720 | 800 | 880 | 1000 | 1120 | 1280 |
| | Nối kéo ($100\%$) | $50d$ | 500 | 600 | 700 | 800 | 900 | 1000 | 1100 | 1250 | 1400 | 1600 |
| | Nối nén | $30d$ | 300 | 360 | 420 | 480 | 540 | 600 | 660 | 750 | 840 | 960 |
| **CB500-V** | Neo kéo thẳng | $45d$ | 450 | 540 | 630 | 720 | 810 | 900 | 990 | 1130 | 1260 | 1440 |
| | Neo kéo có móc $90^\circ$ | $32d$ | 320 | 390 | 450 | 520 | 580 | 640 | 710 | 800 | 900 | 1030 |
| | Nối kéo ($\le 50\%$) | $50d$ | 500 | 600 | 700 | 800 | 900 | 1000 | 1100 | 1250 | 1400 | 1600 |
| | Nối kéo ($100\%$) | $60d$ | 600 | 720 | 840 | 960 | 1080 | 1200 | 1320 | 1500 | 1680 | 1920 |
| | Nối nén | $35d$ | 350 | 420 | 490 | 560 | 630 | 700 | 770 | 880 | 980 | 1120 |

---

## 4. QUY ĐỊNH VỊ TRÍ VÀ CẤU TẠO NỐI CỐT THÉP CÁC CẤU KIỆN

### 4.1 Cột (Column):
1. **Vị trí nối:** Bố trí nối ở khoảng giữa tầng (tốt nhất là vùng từ $H/4$ đến $3H/4$ tính từ mép sàn dưới). Không được nối ở chân cột sát mặt sàn (vùng khớp dẻo tiềm năng chịu mô-men uốn lớn).
2. **So le:** Nên nối so le $50\%$ cốt thép. Khoảng cách giữa 2 đầu nối so le $\ge 1.3 \cdot L_{lap}$ (hoặc $\ge 500\text{ mm}$).
3. **Đai gia cường:** Trong toàn bộ phạm vi chiều dài đoạn nối chồng, đai cột phải được bố trí dày hơn: bước đai $s \le 100\text{ mm}$ hoặc $s \le 10d$ (lấy giá trị nhỏ hơn).

### 4.2 Dầm (Beam):
1. **Cốt thép lớp TRÊN:** Chịu kéo lớn nhất tại 2 gối tựa dầm (mô-men âm).
   - **Vùng cấm nối:** Phạm vi $L/3$ tính từ mép cột/gối tựa.
   - **Vị trí nối cho phép:** Khoảng $1/3$ giữa nhịp dầm ($L/3 \div 2L/3$).
2. **Cốt thép lớp DƯỚI:** Chịu kéo lớn nhất tại giữa nhịp dầm (mô-men dương).
   - **Vùng cấm nối:** Phạm vi giữa dầm ($L/3 \div 2L/3$).
   - **Vị trí nối cho phép:** Vùng gần gối tựa (cách mép cột/gối $\le L/4$).
3. **Đai dầm trong vùng nối:** Tăng cường bước đai $s \le 100\text{ mm}$.

### 4.3 Sàn (Slab):
1. **Cốt thép lớp TRÊN (thép gối/thép mũ):** Nối ở khoảng giữa nhịp sàn.
2. **Cốt thép lớp DƯỚI:** Nối trên gối tựa (dầm, tường bê tông).

### 4.4 Vách (Shear Wall):
1. Cốt thép dọc thân vách nối so le tối thiểu cách mặt sàn $\ge 500\text{ mm}$.
2. Vùng biên vách (Boundary Element) tuân thủ quy tắc nối tương tự như cột chịu nén lệch tâm lớn.

---

## 5. QUY CÁCH BẺ MÓC UỐN VÀ ĐAI KHÁNG CHẤN

1. **Móc chữ U $180^\circ$ (cho thép trơn CB240-T):**
   - Đường kính uốn trong: $D \ge 2.5d$ ($d < 20$), $D \ge 5d$ ($d \ge 20$).
   - Đoạn thẳng kéo dài sau khúc uốn: $l_{ext} \ge 3d$ hoặc tối thiểu $50\text{ mm}$.
2. **Móc vuông $90^\circ$ (cho thép gân CB300-V, CB400-V, CB500-V):**
   - Đường kính trục uốn trong: $D \ge 4d$ ($d < 20$), $D \ge 5d$ ($20 \le d \le 25$), $D \ge 7d$ ($d > 25$).
   - Đoạn kéo dài thẳng sau góc uốn: $\ge 12d$ khi neo chịu kéo vào cột/dầm biên.
3. **Móc đai kháng chấn $135^\circ$ (TCVN 9386):**
   - Góc bẻ móc $135^\circ$.
   - Đoạn thẳng kéo dài sau móc uốn: $\ge 10d$ hoặc tối thiểu $\ge 75\text{ mm}$.

---

## 6. QUY ĐỊNH NỐI CỐT THÉP ĐƯỜNG KÍNH LỚN ($d \ge 28\text{ mm}$)
- Khi đường kính thanh $d \ge 28\text{ mm}$, **không khuyến nghị nối chồng buộc**.
- Thay vào đó, áp dụng:
  1. **Nối ren (Coupler cơ học):** Đạt giới hạn phá hủy ngoài vị trí nối theo tiêu chuẩn TCVN 8163:2009.
  2. **Nối hàn đối đầu (Flash butt welding / Shielded metal arc):** Theo TCVN 4453:1995.
