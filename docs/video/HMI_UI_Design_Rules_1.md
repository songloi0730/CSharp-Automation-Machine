# Quy Tắc Thiết Kế Giao Diện HMI
## Phần mềm điều khiển máy công nghiệp — C# / WPF

> **Phạm vi áp dụng:** Tài liệu này là chuẩn thiết kế bắt buộc cho toàn bộ giao diện của các phần mềm điều khiển máy tự động hoá. Mọi màn hình, control, màu sắc, font chữ, và hành vi người dùng phải tuân thủ các quy tắc dưới đây.
>
> **Nguồn tham chiếu:** ISA-101.01-2015 · EEMUA 201 · SEMI S2 / S8 · ASM Consortium · Material Design (chọn lọc)

---

## Mục lục

1. [Triết lý thiết kế](#1-triết-lý-thiết-kế)
2. [Màu sắc](#2-màu-sắc)
3. [Typography — Font chữ](#3-typography--font-chữ)
4. [Phân cấp màn hình (Display Hierarchy)](#4-phân-cấp-màn-hình-display-hierarchy)
5. [Bố cục màn hình (Layout)](#5-bố-cục-màn-hình-layout)
6. [Điều hướng (Navigation)](#6-điều-hướng-navigation)
7. [Hiển thị trạng thái thiết bị](#7-hiển-thị-trạng-thái-thiết-bị)
8. [Quản lý Alarm](#8-quản-lý-alarm)
9. [Hiển thị dữ liệu số và đơn vị](#9-hiển-thị-dữ-liệu-số-và-đơn-vị)
10. [Controls — Nút bấm và Input](#10-controls--nút-bấm-và-input)
11. [Biểu đồ và Trend](#11-biểu-đồ-và-trend)
12. [Phân quyền và bảo mật UI](#12-phân-quyền-và-bảo-mật-ui)
13. [Đa ngôn ngữ](#13-đa-ngôn-ngữ)
14. [Hiệu năng và phản hồi UI](#15-hiệu-năng-và-phản-hồi-ui)
15. [Accessibility và Ergonomics](#15-accessibility-và-ergonomics)
16. [Quy tắc đặc thù cho WPF/XAML](#16-quy-tắc-đặc-thù-cho-wpfxaml)
17. [Checklist trước khi release](#17-checklist-trước-khi-release)

---

## 1. Triết lý thiết kế

### 1.1 Nguyên tắc cốt lõi

**Giao diện phục vụ vận hành, không phải để trưng bày.**

Người vận hành máy làm việc 8–12 giờ mỗi ngày trước màn hình. Mọi quyết định thiết kế phải ưu tiên:

- **Situational Awareness** — Operator biết ngay trạng thái máy mà không cần đọc nhiều
- **Cognitive load tối thiểu** — Thông tin quan trọng nổi bật, không bị chìm trong nhiễu
- **Phòng ngừa lỗi** — Giao diện ngăn thao tác sai, không chỉ thông báo sau khi sai
- **Nhất quán** — Mọi màn hình dùng cùng pattern, operator học một lần dùng mãi

### 1.2 Quy tắc 90/10 (ISA-101)

> **90% màn hình là tông xám trung tính. 10% còn lại dành riêng cho màu có ý nghĩa.**

Khi mọi thứ đều màu sắc, không có gì nổi bật. Khi nền là xám, một indicator đỏ sẽ lập tức thu hút mắt operator — đây là ý đồ thiết kế, không phải do thiếu sáng tạo.

### 1.3 Những thứ KHÔNG được làm

| Cấm | Lý do |
|-----|-------|
| 3D effects, gradient, bóng đổ trên equipment | Tốn tài nguyên, không thêm thông tin |
| Ảnh thực tế (photorealistic) làm nền | Che khuất dữ liệu, gây xao nhãng |
| Animation trang trí (chỉ để đẹp) | Gây mất tập trung khi vận hành |
| Dùng màu tuỳ ý không theo chuẩn | Operator không hiểu ý nghĩa |
| Popup/dialog liên tục cho thao tác thường xuyên | Làm chậm workflow |
| Hiển thị quá nhiều số thập phân | Gây nhầm độ chính xác |
| Dùng màu đỏ/vàng cho trang trí | Mất đi khả năng báo nguy hiểm |

---

## 2. Màu sắc

### 2.1 Bảng màu chuẩn — Semantic Colors

Màu mang **ý nghĩa ngữ nghĩa**, không phải trang trí. Phải nhất quán tuyệt đối trong toàn bộ ứng dụng.

| Trạng thái | Màu | Hex | WPF ResourceKey | Khi nào dùng |
|-----------|-----|-----|-----------------|-------------|
| **Normal / Running** | Xanh lá (muted) | `#4CAF50` | `StatusNormal` | Thiết bị đang chạy đúng |
| **Warning / Advisory** | Vàng hổ phách | `#FFC107` | `StatusWarning` | Tiến đến ngưỡng giới hạn, cần chú ý |
| **Alarm / Error** | Đỏ tươi | `#F44336` | `StatusAlarm` | Lỗi cần xử lý ngay |
| **Critical / E-Stop** | Đỏ đậm nhấp nháy | `#B71C1C` | `StatusCritical` | Dừng khẩn cấp, nguy hiểm an toàn |
| **Disabled / Off** | Xám trung | `#9E9E9E` | `StatusDisabled` | Thiết bị tắt, không khả dụng |
| **Acknowledged Alarm** | Cam nhạt | `#FF8F00` | `StatusAcknowledged` | Alarm đã xác nhận, chưa clear |
| **Manual Mode** | Xanh dương | `#1E88E5` | `StatusManual` | Đang ở chế độ điều khiển tay |
| **Interlock Active** | Tím | `#7B1FA2` | `StatusInterlock` | Điều kiện khoá đang kích hoạt |

### 2.2 Bảng màu nền (Background Palette)

Cả hai theme đều phải hỗ trợ đầy đủ. Người dùng chọn theo môi trường làm việc thực tế.

#### Dark Theme (Mặc định — Operator Station)

```
Token                    Hex        Mô tả
─────────────────────────────────────────────────────
Screen.Background        #1A1A1A   Nền toàn màn hình
Panel.Background         #252525   Nền card / panel
Panel.Background.Alt     #2D2D2D   Nền xen kẽ (zebra rows)
Header.Background        #1F1F1F   Top bar, side menu
Header.Background.Active #2A2A2A   Menu item đang active
Equipment.Normal         #3A3A3A   Thiết bị trạng thái thường
Equipment.Surface        #424242   Bề mặt bên trong equipment
Border.Default           #3D3D3D   Viền panel nhẹ
Border.Strong            #555555   Viền panel nổi bật
Divider                  #333333   Đường kẻ phân cách
Overlay                  #00000080 Nền mờ khi popup mở
```

#### Light Theme (Sàn xưởng sáng — Engineering / Report)

```
Token                    Hex        Mô tả
─────────────────────────────────────────────────────
Screen.Background        #F0F2F5   Nền toàn màn hình (xám xanh nhạt)
Panel.Background         #FFFFFF   Nền card / panel
Panel.Background.Alt     #F8F9FA   Nền xen kẽ (zebra rows)
Header.Background        #E8ECF0   Top bar, side menu
Header.Background.Active #D8DDE3   Menu item đang active
Equipment.Normal         #D0D5DB   Thiết bị trạng thái thường
Equipment.Surface        #BFC5CC   Bề mặt bên trong equipment
Border.Default           #D1D5DB   Viền panel nhẹ
Border.Strong            #9CA3AF   Viền panel nổi bật
Divider                  #E5E7EB   Đường kẻ phân cách
Overlay                  #00000040 Nền mờ khi popup mở
```

> **Lý do chọn `#F0F2F5` (xám-xanh nhạt) thay vì trắng tinh `#FFFFFF` làm nền chính:**  
> Nền trắng tuyệt đối gây glare mạnh dưới ánh đèn công nghiệp. `#F0F2F5` giảm độ chói ~15% trong khi vẫn cảm giác sáng, sạch. Panel card `#FFFFFF` nổi lên rõ ràng trên nền này tạo depth tự nhiên mà không cần shadow.

**Khuyến nghị chọn theme theo môi trường:**

| Môi trường | Theme khuyến nghị | Lý do |
|-----------|------------------|-------|
| Control room, clean room, phòng tối | **Dark** | Giảm mỏi mắt ca dài, màu alarm nổi bật hơn |
| Sàn xưởng có ánh sáng tự nhiên/đèn mạnh | **Light** | Glare từ nền tối gây khó đọc dưới ánh sáng mạnh |
| Màn hình ngoài trời / bán ngoài trời | **Light** | Contrast cao hơn dưới ánh sáng môi trường |
| Màn hình báo cáo, engineering | **Light** | Phù hợp để in ấn và đọc lâu |
| Tablet / touchscreen di động | **Light** | Phản xạ ánh sáng môi trường ít hơn |

### 2.3 Màu chữ

```
Text chính:             #E0E0E0  (dark) / #212121   (light)
Text phụ (label):       #9E9E9E  (dark) / #757575   (light)
Text tiêu đề section:   #BDBDBD  (dark) / #424242   (light)
Text giá trị live:      #FFFFFF  (dark) / #000000   (light)   ← đậm nhất
Text disabled:          #616161  (dark) / #BDBDBD   (light)
```

### 2.4 Quy tắc dùng màu

- **KHÔNG bao giờ** dùng màu đỏ cho thứ gì không phải alarm/error
- **KHÔNG bao giờ** dùng màu vàng cho trang trí
- **KHÔNG bao giờ** dùng đơn thuần màu sắc để phân biệt — luôn kết hợp với **hình dạng hoặc text** (hỗ trợ người mù màu, ~8% nam giới)
- Màu xanh lá **dùng hạn chế**: chỉ khi cần confirm "permissive met", không dùng cho mọi thứ đang chạy (desensitization effect)
- Equipment ở trạng thái bình thường: **xám** — không phải xanh lá

### 2.5 Hiệu ứng nhấp nháy (Blinking)

| Trường hợp | Tần suất | Ghi chú |
|-----------|---------|---------|
| Critical alarm chưa acknowledge | 1 Hz (1 lần/giây) | Dừng khi đã acknowledge |
| E-Stop active | 2 Hz | Luôn nhấp nháy khi còn active |
| Các trạng thái khác | KHÔNG nhấp nháy | Nhấp nháy = khẩn cấp tuyệt đối |

> **Giới hạn an toàn (SEMI S8 / Epilepsy):** Tần suất nhấp nháy KHÔNG được vượt quá 3 Hz và KHÔNG được có vùng nhấp nháy lớn hơn 25% diện tích màn hình để tránh kích thích động kinh quang học.

---

## 3. Typography — Font chữ

### 3.1 Font ưu tiên

```
Font chính:   Segoe UI       (có sẵn trên Windows, hỗ trợ CJK tốt)
Font dự phòng: Roboto, Arial  (không dùng Times New Roman, serif)
Font mono:    Consolas        (hiển thị giá trị số, log, ID)
```

**Lý do chọn sans-serif:** Nghiên cứu ergonomics (SEMI S8) cho thấy sans-serif dễ đọc hơn 15–20% trên màn hình công nghiệp ở khoảng cách > 60 cm.

### 3.2 Cỡ chữ chuẩn

| Thành phần | Size | Weight | Ghi chú |
|-----------|------|--------|---------|
| Tiêu đề màn hình (Level 1-2) | 18–20 pt | Bold | Tên màn hình, station |
| Tiêu đề section/group | 14–16 pt | SemiBold | Tên nhóm control |
| Label field | 12 pt | Regular | Nhãn của giá trị |
| Giá trị live data | 14–16 pt | Bold | Số liệu đang chạy thực |
| Giá trị setpoint | 13 pt | Regular | Giá trị cài đặt |
| Button text | 13 pt | SemiBold | |
| Text phụ, đơn vị | 11–12 pt | Regular | mm/s, °C, kPa... |
| Alarm message | 13 pt | Regular/Bold (Critical) | |
| Tooltip | 11 pt | Regular | |

**Quy tắc:**
- Cỡ chữ tối thiểu trên màn hình vận hành: **11 pt**
- KHÔNG dùng quá 3 cỡ chữ khác nhau trên một màn hình
- Giá trị live data luôn to hơn label tương ứng ít nhất 2 pt

### 3.3 Căn lề

- **Số:** luôn căn phải trong cột để so sánh dễ
- **Text:** căn trái
- **Tiêu đề:** căn giữa trong header, căn trái trong panel
- Dấu thập phân và đơn vị trên cùng một hàng, không xuống dòng

---

## 4. Phân cấp màn hình (Display Hierarchy)

Theo ISA-101 và Rockwell PlantPAx, tổ chức màn hình theo 4 cấp:

```
Level 1 — Overview (Tổng quan toàn máy)
    ↓
Level 2 — Process Area (Khu vực / Workstation)
    ↓
Level 3 — Detail / Faceplate (Chi tiết thiết bị)
    ↓
Level 4 — Diagnostic / Engineering (Cấu hình, debug)
```

### 4.1 Level 1 — Overview Screen

**Mục đích:** Operator nhìn 1 lần biết toàn bộ trạng thái máy.

- Hiển thị tất cả workstation/module với trạng thái tổng quan
- Alarm summary: số lượng Critical / High / Medium / Low
- Machine state hiện tại (Running / Paused / Error / EStop)
- Production counter: UPH, Total OK, Total NG, Yield%
- Thời gian hiện tại, tên recipe đang chạy
- **KHÔNG hiển thị giá trị chi tiết** (đó là Level 2–3)
- Nhấp vào bất kỳ module nào → drill down Level 2

### 4.2 Level 2 — Workstation / Process Area Screen

**Mục đích:** Theo dõi và điều khiển một khu vực cụ thể.

- Sơ đồ thiết bị khu vực (simplified P&ID hoặc machine layout)
- Giá trị live của các điểm đo quan trọng
- Trạng thái I/O chính
- Nút điều khiển thường dùng
- Link đến faceplate chi tiết (Level 3)

### 4.3 Level 3 — Detail / Faceplate

**Mục đích:** Chi tiết một thiết bị đơn lẻ — axis, camera, sensor.

- Tất cả thông số của thiết bị đó
- Lịch sử ngắn (5–10 giá trị gần nhất)
- Nút điều khiển trực tiếp (enable, jog, set, reset)
- Thường xuất hiện dạng **Popup/Flyout** không che Level 2

### 4.4 Level 4 — Engineering / Diagnostic

**Mục đích:** Cấu hình, debug, maintenance.

- Yêu cầu quyền Engineer hoặc Admin
- Cài đặt parameter chi tiết
- Motion tuning, camera calibration
- System log, communication log
- Raw IO monitor
- **Không dùng trong sản xuất bình thường**

---

## 5. Bố cục màn hình (Layout)

### 5.1 Vùng cố định (Fixed Zones)

```
┌─────────────────────────────────────────────────────┐
│  TOP BAR (48px)                                      │
│  [Logo] [Machine Name] [State] [User] [Time] [Lang]  │
├──────────┬──────────────────────────────────────────┤
│          │                                          │
│   SIDE   │          CONTENT AREA                   │
│   MENU   │          (màn hình level 1-4)            │
│  (200px) │                                          │
│          │                                          │
├──────────┴──────────────────────────────────────────┤
│  STATUS BAR (32px)                                   │
│  [Alarm Summary] [Cycle Time] [UPH] [Recipe]         │
└─────────────────────────────────────────────────────┘
```

| Vùng | Chiều cao/rộng | Nội dung |
|------|---------------|---------|
| Top Bar | 48 px | Logo, tên máy, machine state chip, thông tin user, đồng hồ, chọn ngôn ngữ |
| Side Menu | 200 px (có thể collapse còn 60 px) | Navigation các màn hình, icon + text |
| Content Area | Còn lại | Nội dung thay đổi theo màn hình |
| Status Bar | 32 px | Luôn hiển thị: alarm count, cycle time, UPH, recipe |

### 5.2 Nguyên tắc bố cục nội dung

- **Thông tin quan trọng nhất ở trên-trái** (reading pattern tự nhiên)
- **Nút hành động nguy hiểm** (Stop, EStop) ở góc dễ nhìn, cách xa nút thường dùng
- **Padding tối thiểu** giữa các element: 8 px (nhỏ), 16 px (thông thường), 24 px (section)
- **Không để quá 7±2 element** chính trên một màn hình (Miller's Law)
- Group các element liên quan bằng **border hoặc nền nhẹ hơn** — không dùng màu sắc để group
- Căn lề theo **grid 8px** — mọi kích thước và khoảng cách là bội số của 8

### 5.3 Giải phóng không gian

- Thông tin chỉ dùng đôi khi → **Flyout / Collapsible panel** thay vì luôn hiển thị
- Dữ liệu lịch sử → **Tab** hoặc màn hình Level 3/4 riêng
- Thông số ít thay đổi → **Settings screen** không phải màn hình vận hành

---

## 6. Điều hướng (Navigation)

### 6.1 Nguyên tắc

- **Tối đa 3 click** để đến bất kỳ màn hình nào từ màn hình overview
- **Breadcrumb** hoặc chỉ thị màn hình hiện tại luôn hiển thị
- Nút **Back** hoặc **Escape** luôn hoạt động (trừ khi popup yêu cầu xác nhận)
- Không dùng browser-style navigation (tiến/lùi) — gây nhầm lẫn trong môi trường công nghiệp

### 6.2 Side Menu

```
[Home / Overview]         ← luôn ở đầu
[Production]
[Alarm]                   ← với badge số alarm active
[Parameter / Recipe]
[Motion / IO]
[Vision]
[Logging]
──────────────────
[Settings]                ← Engineering level
[User Management]         ← Admin level
```

- Icon + Text khi menu mở đầy đủ
- Chỉ Icon khi menu thu nhỏ — icon phải đủ rõ nghĩa, không cần text
- Menu item active: highlight nền, không chỉ thay màu chữ (hỗ trợ người mù màu)
- Alarm badge: số đỏ góc trên phải icon Alarm

### 6.3 Popup và Dialog

| Loại | Khi nào dùng | Kích thước |
|------|-------------|-----------|
| Confirmation Dialog | Thao tác không hoàn tác được | ≤ 400×200 px |
| Faceplate Popup | Chi tiết thiết bị Level 3 | 500–700 px rộng |
| Input Dialog | Nhập giá trị setpoint | ≤ 400×300 px |
| Full-screen Modal | Không dùng trong vận hành | — |

- Popup **KHÔNG che** Status Bar và alarm area
- Popup có thể drag để di chuyển
- Nút đóng popup rõ ràng (✕), không ẩn
- Sau 30 giây không tương tác → tự đóng popup input (trừ dialog confirm)

---

## 7. Hiển thị trạng thái thiết bị

### 7.1 Equipment State Symbols

Mỗi thiết bị phải hiển thị trạng thái qua **ít nhất 2 cách** (màu + hình hoặc màu + text), không chỉ màu đơn thuần.

| Trạng thái | Màu fill | Icon / Symbol | Text label |
|-----------|---------|--------------|-----------|
| Off / Stopped | Xám `#616161` | ○ (circle rỗng) | OFF |
| Running / On | Xanh lá `#4CAF50` | ● (filled) | RUN |
| Fault / Error | Đỏ `#F44336` | ✕ hoặc ! | ERR |
| Warning | Vàng `#FFC107` | △ | WARN |
| Manual | Xanh dương `#1E88E5` | M | MAN |
| Initializing | Xám nhạt, animation | ◌ (spinning) | INIT |
| Interlock | Tím `#7B1FA2` | 🔒 (ký tự lock) | ILK |

### 7.2 Axis / Motor

```
[Axis Name]   [Position: 123.45 mm]   [●RUN / ○STOP / ✕ERR]
              [Velocity: 50.00 mm/s]  [Home: ✓ / ✗]
```

- Hiển thị **position** luôn, dù đang dừng
- Khi moving: indicator chuyển động nhẹ (không dùng animation giả tốc độ)
- Khi error: cả label lẫn giá trị chuyển sang nền đỏ nhạt

### 7.3 I/O Display

- DI (Digital Input): ○ xám = OFF, ● xanh = ON
- DO (Digital Output): □ xám = OFF, ■ xanh = ON (khác với DI để phân biệt)
- AI (Analog Input): Thanh ngang với min/max, giá trị số bên cạnh
- AO (Analog Output): Tương tự AI nhưng có khả năng chỉnh

**Quy tắc nhóm IO:**
- Group theo chức năng, không theo địa chỉ vật lý
- Đặt tên theo nghĩa: `PART_DETECT_SENSOR` không phải `DI_00_03`
- IO đang active (ON) nổi bật hơn IO đang tắt

---

## 8. Quản lý Alarm

*(Tuân thủ EEMUA 191/201, ISA-18.2, ISA-101)*

### 8.1 Mức độ Alarm (Priority)

| Level | Màu | Nhấp nháy | Ý nghĩa | Thời gian phản hồi yêu cầu |
|-------|-----|---------|---------|--------------------------|
| **Critical** | Đỏ đậm `#B71C1C` | 1 Hz | Nguy hiểm an toàn, E-Stop | Ngay lập tức |
| **High** | Đỏ `#F44336` | Không | Dừng sản xuất, cần xử lý sớm | < 5 phút |
| **Medium** | Vàng `#FFC107` | Không | Ảnh hưởng chất lượng / hiệu suất | < 30 phút |
| **Low** | Xanh dương nhạt `#64B5F6` | Không | Thông tin, nhắc nhở | Cuối ca |

**Phân phối mục tiêu (EEMUA 191):**
- Critical: ≤ 5% tổng alarm
- High: ≤ 15% tổng alarm
- Medium + Low: ≥ 80% tổng alarm

### 8.2 Alarm Rate (EEMUA 191)

- Trạng thái bình thường: **≤ 6 alarm / 10 phút**
- Alarm storm threshold: **> 10 alarm đồng thời** = cần review
- Chattering alarm (on/off > 3 lần / 10 phút): **tự động đánh dấu và giảm ưu tiên**

### 8.3 Alarm Bar — Luôn hiển thị

```
┌──────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: 1  🔴 HIGH: 3  🟡 MED: 5  🔵 LOW: 12  │
│ [Unacked: 4] [Last: AXIS1_FAULT 14:32:05] [→ Details]│
└──────────────────────────────────────────────────────┘
```

- **Luôn hiển thị** ở Status Bar, không bị che
- Khi có Critical alarm: nền Status Bar chuyển đỏ nhạt
- Click → mở Alarm List screen

### 8.4 Alarm List Screen

Cột bắt buộc:

| Cột | Mô tả |
|-----|-------|
| Priority | Icon màu + text level |
| Timestamp | DD/MM/YYYY HH:mm:ss.ms |
| Alarm Code | Mã lỗi cố định (e.g. `10001`) |
| Description | Mô tả rõ ràng bằng ngôn ngữ hiện tại |
| Station/Module | Nguồn gốc alarm |
| State | Active / Acknowledged / Cleared |
| Duration | Thời gian alarm đã active |
| Action | Nút Acknowledge / Goto |

**Quy tắc hiển thị:**
- Alarm đang active: nổi lên đầu danh sách
- Alarm Critical + High: hàng nổi bật (nền đậm hơn)
- Acknowledge tất cả = 1 nút với confirmation
- Lọc theo: level, station, time range, state
- Export ra CSV/Excel

### 8.5 Nội dung Alarm Message

**Cấu trúc chuẩn:** `[Thiết bị] [Vấn đề] — [Hướng giải quyết]`

```
✓ ĐÚNG:   "Axis X: Timeout khi home — Kiểm tra cơ học và home lại"
✗ SAI:    "Error 10001"
✗ SAI:    "Motion error occurred"
✗ SAI:    "Axis X home timeout error has been detected in the system"
```

- Tối đa 80 ký tự cho message chính
- Thêm **Help link** hoặc nút để xem hướng dẫn chi tiết
- Alarm phải có **Cause** và **Action** — không chỉ mô tả triệu chứng

---

## 9. Hiển thị dữ liệu số và đơn vị

### 9.1 Số thập phân

| Loại giá trị | Số chữ số thập phân | Ví dụ |
|-------------|---------------------|-------|
| Position (mm) | 2 | `123.45 mm` |
| Velocity (mm/s) | 1 | `50.0 mm/s` |
| Temperature (°C) | 1 | `25.3 °C` |
| Pressure (kPa) | 2 | `101.32 kPa` |
| Percentage (%) | 1 | `98.7 %` |
| Counter (pcs) | 0 | `1234` |
| Score (0–100) | 1 | `98.5` |

**Quy tắc:** Luôn ít số thập phân hơn độ phân giải thực của cảm biến. Hiển thị `42.3 °C` không phải `42.3456789 °C` — độ chính xác giả tạo làm operator mất tin tưởng.

### 9.2 Đơn vị

- **Luôn hiển thị đơn vị** kế bên giá trị: `123.45 mm`, không phải `123.45`
- Đơn vị nhỏ hơn giá trị 1–2 pt, màu nhạt hơn
- Khoảng cách giữa số và đơn vị: **1 space** (`42.3 °C` không phải `42.3°C`)
- Nhất quán trong toàn bộ ứng dụng — chọn hoặc metric hoặc imperial, không pha trộn

### 9.3 Hiển thị giới hạn

```
┌──────────────────────────────────────────┐
│  Temperature                              │
│  ┌────────────────────────────┐           │
│  │ ████████████░░░░░░░░░░░░  │           │
│  └────────────────────────────┘           │
│  0°C    ↑ Setpoint: 80°C   ↑ Max: 120°C │
│         Current: 75.3°C                  │
└──────────────────────────────────────────┘
```

- Thanh progress/bar: luôn hiển thị min, max, và setpoint
- Vùng cảnh báo (warning band): màu vàng nhạt trên thanh
- Vùng nguy hiểm (alarm band): màu đỏ nhạt trên thanh
- Giá trị hiện tại + setpoint luôn hiển thị cả hai

### 9.4 Thời gian và Ngày tháng

- Format: `DD/MM/YYYY HH:mm:ss` (mặc định, theo ISO 8601 cục bộ)
- Timestamp alarm: thêm milliseconds `HH:mm:ss.fff`
- Tránh format 12h (AM/PM) trong công nghiệp — dùng 24h
- Timezone: hiển thị timezone nếu hệ thống có nhiều máy tính ở múi giờ khác nhau

---

## 10. Controls — Nút bấm và Input

### 10.1 Button

| Loại | Kích thước tối thiểu | Màu | Ví dụ |
|------|---------------------|-----|-------|
| Primary Action | 120 × 40 px | Accent (xanh dương) | Start, Confirm |
| Danger Action | 120 × 40 px | Đỏ `#F44336` | Stop, Delete |
| E-Stop | 80 × 80 px | Đỏ đậm `#B71C1C` | EMERGENCY STOP |
| Secondary | 100 × 36 px | Xám viền | Cancel, Back |
| Icon Button | 40 × 40 px | Transparent | ✏️, 🔄, ⚙️ |

**Quy tắc nút bấm:**
- Khoảng cách tối thiểu giữa hai nút nguy hiểm: **48 px** (ngăn bấm nhầm)
- Nút **E-Stop** phải lớn nhất trên màn hình, vị trí cố định (góc trên phải màn hình vận hành)
- Nút thực hiện hành động không hoàn tác được phải có **Confirmation Dialog**
- Khi đang thực hiện (loading): nút disable + hiện spinner — không cho bấm lại
- Tooltip trên mọi icon button (hiển thị sau 500ms hover)

### 10.2 Input Fields

```
Label (12pt, màu nhạt)
┌──────────────────────┐
│  Giá trị nhập         │  [Đơn vị]
└──────────────────────┘
Min: 0    Max: 1000 mm
```

- Label luôn ở trên input, không phải bên trong (placeholder)
- Hiển thị range hợp lệ (min/max) bên dưới
- Validation realtime: border đỏ ngay khi nhập sai
- Khi nhập giá trị nguy hiểm (> 80% max): cảnh báo màu vàng trước khi submit
- **Numeric keyboard** popup tự động khi focus vào input số (touchscreen friendly)

### 10.3 Toggle và Checkbox

- Toggle (on/off): dùng cho state thay đổi ngay lập tức
- Checkbox: dùng cho lựa chọn trong form (không thực thi ngay)
- Toggle size tối thiểu: 44 × 24 px (touchscreen)
- Label của toggle ở bên phải, mô tả trạng thái **hiện tại** không phải action

### 10.4 Dropdown / ComboBox

- Chiều cao tối thiểu item trong dropdown: 36 px
- Tối đa hiển thị 8 item trước khi scroll
- Khi có > 10 item: thêm search box trong dropdown

---

## 11. Biểu đồ và Trend

### 11.1 Real-time Trend Chart

**Dark theme:**
- Background: `#0D1B2A` (navy) hoặc `#111827` (charcoal) — nền tối tạo contrast cao cho đường data
- Grid lines: `#1E2D3D` — nhạt, không át data lines

**Light theme:**
- Background: `#F8FAFC` (trắng xanh rất nhạt) — không dùng trắng tuyệt đối để giảm glare
- Grid lines: `#E2E8F0` — xám nhạt, vẫn thấy nhưng không lấn data

**Chung cho cả hai theme:**
- Data lines: màu theo semantic (nhiệt độ = `#F97316` cam, áp suất = `#22C55E` xanh lá, v.v.)
- Trục Y: luôn hiển thị đơn vị
- Trục X: timestamp, format `HH:mm:ss`
- **Setpoint line**: đứt nét, cùng màu data nhưng opacity 50%
- **Alarm threshold lines**: `#EF4444` đỏ, đứt nét, opacity 60%
- **Warning threshold lines**: `#F59E0B` vàng, đứt nét, opacity 60%
- Zoom: cuộn chuột để zoom X-axis, drag để pan
- Nền chart **KHÔNG** dùng màu trắng/đen tuyệt đối trong cả hai theme

### 11.2 Production Chart (Bar/Pie)

- Dùng cho: UPH theo giờ, yield theo ngày, NG rate theo loại lỗi
- Màu bars: dùng bảng màu consistent, không random
- OK: `#4CAF50`, NG: `#F44336`, Warning zone: `#FFC107`
- Label giá trị trực tiếp trên bar — không chỉ hiển thị qua tooltip
- Legend đơn giản, max 5 items

### 11.3 Gauge (Đồng hồ)

- **Hạn chế dùng gauge** — thanh ngang hoặc số hiển thị thường rõ hơn
- Nếu dùng gauge: phải có vùng màu alarm/warning rõ ràng
- Không dùng gauge "speedometer" đẹp nhưng không cho thấy trend

---

## 12. Phân quyền và bảo mật UI

### 12.1 User Roles và quyền truy cập UI

| Role | Quyền truy cập |
|------|----------------|
| **Operator** | Xem tất cả màn hình, điều khiển production cơ bản, acknowledge alarm |
| **Technician** | + Jog manual, xem IO/Motion detail, xem log |
| **Engineer** | + Chỉnh parameter, load recipe, calibration, xem tất cả Level 4 |
| **Admin** | + Quản lý user, cài đặt hệ thống, cấu hình alarm |
| **Supervisor** | + Xem report, export data, approve recipe mới |

### 12.2 Hiển thị quyền trong UI

- Element không có quyền: **disabled** (mờ), không hidden (vẫn thấy nhưng không dùng được)
- Tooltip trên element disabled: giải thích lý do và quyền cần thiết
- Nút yêu cầu quyền cao hơn: hiện icon 🔒 nhỏ góc trên phải

### 12.3 Confirmation cho thao tác nguy hiểm

Phải yêu cầu xác nhận khi:
- Dừng máy đột ngột (không phải E-Stop)
- Xoá/reset dữ liệu production
- Load recipe mới khi đang sản xuất
- Thay đổi parameter ảnh hưởng an toàn
- Override interlock

Dialog confirmation phải ghi rõ **hậu quả**: *"Bạn sắp reset counter sản xuất. Dữ liệu batch hiện tại sẽ bị xoá. Không thể hoàn tác."*

### 12.4 Auto-lock

- Idle > 5 phút (configurable): lock screen, yêu cầu nhập PIN để tiếp tục
- Lock screen vẫn hiển thị: alarm status, machine state, thời gian
- Sau lock: giữ nguyên màn hình hiện tại, không tự về Overview

---

## 13. Đa ngôn ngữ

### 13.1 Ngôn ngữ hỗ trợ (ưu tiên)

1. Tiếng Việt (`vi-VN`) — mặc định
2. English (`en-US`)
3. Tiếng Trung giản thể (`zh-CN`)
4. Tiếng Trung phồn thể (`zh-TW`) — optional

### 13.2 Quy tắc i18n

- **Mọi chuỗi hiển thị** đều phải có resource key, không hardcode trong XAML/code
- Key format: `{Screen}.{Section}.{Element}` — ví dụ: `Alarm.List.Title`
- Đổi ngôn ngữ: **không cần restart**, áp dụng ngay lập tức
- Chọn ngôn ngữ: góc trên phải, dropdown, lưu vào user preference
- Font size có thể tự động tăng 10% cho Chinese (mật độ glyph cao hơn)

### 13.3 Thiết kế chứa được text dài

- Nút và label phải co giãn theo nội dung — không cắt text
- Layout cho phép text dài hơn 30% so với tiếng Anh (tiếng Việt, Đức thường dài hơn)
- Test UI với ngôn ngữ dài nhất trước khi hoàn thiện bố cục

### 13.4 Alarm message đa ngôn ngữ

- Tất cả alarm message đều có bản dịch đầy đủ
- Hiển thị ngôn ngữ theo setting của người dùng hiện tại
- Log lưu cả hai: key ngôn ngữ gốc + message đã dịch (để debug)

---

## 14. Hiệu năng và phản hồi UI

### 14.1 Thời gian phản hồi (Response Time)

| Thao tác | Thời gian tối đa | Xử lý |
|---------|----------------|-------|
| Điều hướng màn hình | 200 ms | Instant feel |
| Load dữ liệu nhỏ | 500 ms | Không cần indicator |
| Load dữ liệu lớn | 2 giây | Hiện skeleton loading |
| Lệnh điều khiển hardware | 100 ms (UI response) | Button disable ngay, spinner |
| Export file lớn | > 2 giây | Progress bar, cho phép cancel |

### 14.2 UI Thread Rules

- **Tuyệt đối không** chặn UI thread với I/O hoặc hardware call
- Tất cả data binding đến live data phải qua `INotifyPropertyChanged` + `Dispatcher`
- Update tần suất cao (> 10 Hz): dùng `CompositionTarget.Rendering` hoặc `DispatcherTimer` với throttle
- Màn hình không active (ẩn): tạm dừng update để tiết kiệm CPU

### 14.3 Update rate khuyến nghị

| Loại data | Update rate | Ghi chú |
|---------|------------|---------|
| Machine state, alarm | 100 ms | Ngay lập tức |
| Position, velocity | 100–200 ms | Real-time control |
| Sensor data (nhiệt độ, áp suất) | 500 ms | Đủ responsive |
| Production counter | 1 giây | |
| Statistics, chart history | 5 giây | |
| Report data | On-demand | Chỉ khi mở màn hình |

### 14.4 Startup và Shutdown

- Splash screen với progress bar khi khởi động (hiển thị bước đang load)
- Thời gian khởi động mục tiêu: < 10 giây đến màn hình sử dụng được
- Shutdown: confirmation dialog, đảm bảo sequence dừng an toàn trước khi đóng
- Tự động lưu UI state (màn hình cuối, layout) để restore khi restart

---

## 15. Accessibility và Ergonomics

*(Tham chiếu SEMI S8 — Safety Guideline for Ergonomics Engineering)*

### 15.1 Kích thước target tối thiểu (Touch / Click)

| Môi trường | Click target tối thiểu | Lý do |
|-----------|----------------------|-------|
| Desktop (chuột) | 24 × 24 px | Cursor chính xác |
| Touch (màn hình cảm ứng) | 44 × 44 px | Ngón tay dày hơn cursor |
| Găng tay công nghiệp | 60 × 60 px | SEMI S8 recommendation |

### 15.2 Màu và Contrast

- Tỷ lệ contrast text/nền: tối thiểu **4.5:1** (WCAG AA)
- Text quan trọng (giá trị live, alarm): tối thiểu **7:1** (WCAG AAA)
- Không dùng chỉ màu sắc để phân biệt — thêm icon hoặc text label
- Test với chế độ Grayscale để verify không mất thông tin

### 15.3 Khoảng cách làm việc

- Màn hình vận hành thường xem từ 60–100 cm: font tối thiểu 12 pt
- Màn hình trên máy (operator panel): font tối thiểu 14 pt
- Màn hình trung tâm điều khiển (nhìn từ 1–2 m): font tối thiểu 16 pt

### 15.4 Ánh sáng môi trường

- Dark theme: phù hợp môi trường ánh sáng yếu (clean room, phòng tối)
- Light theme: phù hợp môi trường ánh sáng mạnh (sàn xưởng có đèn fluorescent mạnh)
- Brightness tự động: cân nhắc điều chỉnh độ sáng theo thời gian trong ngày

---

## 16. Quy tắc đặc thù cho WPF/XAML

### 16.1 ResourceDictionary chuẩn hoá

Toàn bộ style phải định nghĩa trong `ResourceDictionary`, không hardcode trong từng control:

```xml
<!-- App.xaml — load thứ tự quan trọng -->
<ResourceDictionary.MergedDictionaries>
  <ResourceDictionary Source="Themes/Colors.Dark.xaml"/>    <!-- Mặc định: dark -->
  <ResourceDictionary Source="Themes/Typography.xaml"/>     <!-- Font styles — dùng chung -->
  <ResourceDictionary Source="Themes/Controls.xaml"/>       <!-- Button, Input styles -->
  <ResourceDictionary Source="Themes/StatusStyles.xaml"/>   <!-- Alarm, state styles — dùng chung -->
  <ResourceDictionary Source="Themes/Icons.xaml"/>          <!-- Icon paths — dùng chung -->
</ResourceDictionary.MergedDictionaries>
```

**Cấu trúc file theme:**
```
Themes/
├── Colors.Dark.xaml         ← Toàn bộ token màu dark theme
├── Colors.Light.xaml        ← Toàn bộ token màu light theme
├── Typography.xaml          ← Font, size — KHÔNG chứa màu
├── Controls.xaml            ← Style dùng {DynamicResource} token màu
├── StatusStyles.xaml        ← Alarm, state styles — KHÔNG chứa màu cứng
└── Icons.xaml               ← Path geometry icons
```

> **Quy tắc quan trọng:** `Controls.xaml` và `StatusStyles.xaml` KHÔNG được hardcode màu hex. Mọi màu phải dùng `{DynamicResource TokenName}` để đổi theme runtime không cần restart.

### 16.2 Định nghĩa Token trong file Colors.xaml

```xml
<!-- Themes/Colors.Dark.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <!-- === BACKGROUND TOKENS === -->
  <Color x:Key="Screen.BackgroundColor">#1A1A1A</Color>
  <SolidColorBrush x:Key="Screen.BackgroundBrush"
                   Color="{StaticResource Screen.BackgroundColor}"/>

  <Color x:Key="Panel.BackgroundColor">#252525</Color>
  <SolidColorBrush x:Key="Panel.BackgroundBrush"
                   Color="{StaticResource Panel.BackgroundColor}"/>

  <Color x:Key="Panel.Background.AltColor">#2D2D2D</Color>
  <SolidColorBrush x:Key="Panel.Background.AltBrush"
                   Color="{StaticResource Panel.Background.AltColor}"/>

  <Color x:Key="Header.BackgroundColor">#1F1F1F</Color>
  <SolidColorBrush x:Key="Header.BackgroundBrush"
                   Color="{StaticResource Header.BackgroundColor}"/>

  <Color x:Key="Header.Background.ActiveColor">#2A2A2A</Color>
  <SolidColorBrush x:Key="Header.Background.ActiveBrush"
                   Color="{StaticResource Header.Background.ActiveColor}"/>

  <Color x:Key="Equipment.NormalColor">#3A3A3A</Color>
  <SolidColorBrush x:Key="Equipment.NormalBrush"
                   Color="{StaticResource Equipment.NormalColor}"/>

  <Color x:Key="Border.DefaultColor">#3D3D3D</Color>
  <SolidColorBrush x:Key="Border.DefaultBrush"
                   Color="{StaticResource Border.DefaultColor}"/>

  <Color x:Key="Border.StrongColor">#555555</Color>
  <SolidColorBrush x:Key="Border.StrongBrush"
                   Color="{StaticResource Border.StrongColor}"/>

  <Color x:Key="Divider.Color">#333333</Color>
  <SolidColorBrush x:Key="Divider.Brush" Color="{StaticResource Divider.Color}"/>

  <!-- === TEXT TOKENS === -->
  <Color x:Key="Text.PrimaryColor">#E0E0E0</Color>
  <SolidColorBrush x:Key="Text.PrimaryBrush"
                   Color="{StaticResource Text.PrimaryColor}"/>

  <Color x:Key="Text.SecondaryColor">#9E9E9E</Color>
  <SolidColorBrush x:Key="Text.SecondaryBrush"
                   Color="{StaticResource Text.SecondaryColor}"/>

  <Color x:Key="Text.LiveValueColor">#FFFFFF</Color>
  <SolidColorBrush x:Key="Text.LiveValueBrush"
                   Color="{StaticResource Text.LiveValueColor}"/>

  <Color x:Key="Text.DisabledColor">#616161</Color>
  <SolidColorBrush x:Key="Text.DisabledBrush"
                   Color="{StaticResource Text.DisabledColor}"/>

  <Color x:Key="Text.HeadingColor">#BDBDBD</Color>
  <SolidColorBrush x:Key="Text.HeadingBrush"
                   Color="{StaticResource Text.HeadingColor}"/>

  <!-- === CHART TOKENS === -->
  <Color x:Key="Chart.BackgroundColor">#0D1B2A</Color>
  <SolidColorBrush x:Key="Chart.BackgroundBrush"
                   Color="{StaticResource Chart.BackgroundColor}"/>

  <Color x:Key="Chart.GridColor">#1E2D3D</Color>
  <SolidColorBrush x:Key="Chart.GridBrush"
                   Color="{StaticResource Chart.GridColor}"/>

</ResourceDictionary>
```

```xml
<!-- Themes/Colors.Light.xaml — CÁC TOKEN GIỐNG HỆT, CHỈ KHÁC GIÁ TRỊ HEX -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <!-- === BACKGROUND TOKENS === -->
  <Color x:Key="Screen.BackgroundColor">#F0F2F5</Color>
  <SolidColorBrush x:Key="Screen.BackgroundBrush"
                   Color="{StaticResource Screen.BackgroundColor}"/>

  <Color x:Key="Panel.BackgroundColor">#FFFFFF</Color>
  <SolidColorBrush x:Key="Panel.BackgroundBrush"
                   Color="{StaticResource Panel.BackgroundColor}"/>

  <Color x:Key="Panel.Background.AltColor">#F8F9FA</Color>
  <SolidColorBrush x:Key="Panel.Background.AltBrush"
                   Color="{StaticResource Panel.Background.AltColor}"/>

  <Color x:Key="Header.BackgroundColor">#E8ECF0</Color>
  <SolidColorBrush x:Key="Header.BackgroundBrush"
                   Color="{StaticResource Header.BackgroundColor}"/>

  <Color x:Key="Header.Background.ActiveColor">#D8DDE3</Color>
  <SolidColorBrush x:Key="Header.Background.ActiveBrush"
                   Color="{StaticResource Header.Background.ActiveColor}"/>

  <Color x:Key="Equipment.NormalColor">#D0D5DB</Color>
  <SolidColorBrush x:Key="Equipment.NormalBrush"
                   Color="{StaticResource Equipment.NormalColor}"/>

  <Color x:Key="Border.DefaultColor">#D1D5DB</Color>
  <SolidColorBrush x:Key="Border.DefaultBrush"
                   Color="{StaticResource Border.DefaultColor}"/>

  <Color x:Key="Border.StrongColor">#9CA3AF</Color>
  <SolidColorBrush x:Key="Border.StrongBrush"
                   Color="{StaticResource Border.StrongColor}"/>

  <Color x:Key="Divider.Color">#E5E7EB</Color>
  <SolidColorBrush x:Key="Divider.Brush" Color="{StaticResource Divider.Color}"/>

  <!-- === TEXT TOKENS === -->
  <Color x:Key="Text.PrimaryColor">#212121</Color>
  <SolidColorBrush x:Key="Text.PrimaryBrush"
                   Color="{StaticResource Text.PrimaryColor}"/>

  <Color x:Key="Text.SecondaryColor">#757575</Color>
  <SolidColorBrush x:Key="Text.SecondaryBrush"
                   Color="{StaticResource Text.SecondaryColor}"/>

  <Color x:Key="Text.LiveValueColor">#000000</Color>
  <SolidColorBrush x:Key="Text.LiveValueBrush"
                   Color="{StaticResource Text.LiveValueColor}"/>

  <Color x:Key="Text.DisabledColor">#BDBDBD</Color>
  <SolidColorBrush x:Key="Text.DisabledBrush"
                   Color="{StaticResource Text.DisabledColor}"/>

  <Color x:Key="Text.HeadingColor">#424242</Color>
  <SolidColorBrush x:Key="Text.HeadingBrush"
                   Color="{StaticResource Text.HeadingColor}"/>

  <!-- === CHART TOKENS === -->
  <Color x:Key="Chart.BackgroundColor">#F8FAFC</Color>
  <SolidColorBrush x:Key="Chart.BackgroundBrush"
                   Color="{StaticResource Chart.BackgroundColor}"/>

  <Color x:Key="Chart.GridColor">#E2E8F0</Color>
  <SolidColorBrush x:Key="Chart.GridBrush"
                   Color="{StaticResource Chart.GridColor}"/>

</ResourceDictionary>
```

### 16.3 Cách dùng token trong Controls.xaml

```xml
<!-- Controls.xaml — dùng DynamicResource để đổi theme runtime -->
<Style x:Key="PanelCardStyle" TargetType="Border">
  <!-- ✅ ĐÚNG: DynamicResource — đổi được khi switch theme -->
  <Setter Property="Background" Value="{DynamicResource Panel.BackgroundBrush}"/>
  <Setter Property="BorderBrush" Value="{DynamicResource Border.DefaultBrush}"/>
  <Setter Property="BorderThickness" Value="1"/>
  <Setter Property="CornerRadius" Value="4"/>
  <Setter Property="Padding" Value="16"/>
</Style>

<Style x:Key="LiveValueTextStyle" TargetType="TextBlock">
  <Setter Property="Foreground" Value="{DynamicResource Text.LiveValueBrush}"/>
  <Setter Property="FontSize" Value="15"/>
  <Setter Property="FontWeight" Value="Bold"/>
  <Setter Property="FontFamily" Value="Segoe UI"/>
</Style>

<Style x:Key="LabelTextStyle" TargetType="TextBlock">
  <Setter Property="Foreground" Value="{DynamicResource Text.SecondaryBrush}"/>
  <Setter Property="FontSize" Value="12"/>
  <Setter Property="FontFamily" Value="Segoe UI"/>
</Style>

<!-- ❌ SAI: Hardcode màu — không đổi được khi switch theme -->
<!-- <Setter Property="Background" Value="#252525"/> -->
```

### 16.4 Theme Switcher — Đổi theme runtime không restart

```csharp
// ThemeService.cs
public static class ThemeService
{
    private const string DarkThemePath  = "Themes/Colors.Dark.xaml";
    private const string LightThemePath = "Themes/Colors.Light.xaml";

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static void SwitchTheme(AppTheme theme)
    {
        if (CurrentTheme == theme) return;

        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // Tìm và xoá theme hiện tại
        var oldTheme = mergedDicts.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains("Colors.") == true);
        if (oldTheme != null)
            mergedDicts.Remove(oldTheme);

        // Load theme mới — WPF tự cập nhật mọi DynamicResource
        var newThemePath = theme == AppTheme.Dark ? DarkThemePath : LightThemePath;
        mergedDicts.Insert(0, new ResourceDictionary
        {
            Source = new Uri(newThemePath, UriKind.Relative)
        });

        CurrentTheme = theme;

        // Lưu preference
        Properties.Settings.Default.Theme = theme.ToString();
        Properties.Settings.Default.Save();
    }

    public static void LoadSavedTheme()
    {
        if (Enum.TryParse<AppTheme>(Properties.Settings.Default.Theme, out var saved))
            SwitchTheme(saved);
    }
}

public enum AppTheme { Dark, Light }
```

```xml
<!-- MainWindow.xaml — Nút đổi theme trong TopBar -->
<ToggleButton x:Name="ThemeToggle"
              IsChecked="{Binding IsLightTheme}"
              Command="{Binding ToggleThemeCommand}"
              ToolTip="Đổi chế độ sáng/tối"
              Width="40" Height="40">
  <Path Data="{StaticResource SunIconPath}"
        Fill="{DynamicResource Text.SecondaryBrush}"
        Width="16" Height="16"/>
</ToggleButton>
```

```csharp
// MainWindowViewModel.cs
[RelayCommand]
private void ToggleTheme()
{
    var next = ThemeService.CurrentTheme == AppTheme.Dark
               ? AppTheme.Light
               : AppTheme.Dark;
    ThemeService.SwitchTheme(next);
    OnPropertyChanged(nameof(IsLightTheme));
}

public bool IsLightTheme => ThemeService.CurrentTheme == AppTheme.Light;
```

### 16.5 DataTrigger cho trạng thái thiết bị

Dùng `DataTrigger` để thay đổi visual theo state — không code-behind, không hardcode màu:

```xml
<!-- Semantic color tokens — dùng chung cả dark lẫn light, KHÔNG thay đổi theo theme -->
<Color x:Key="Status.NormalColor">#4CAF50</Color>
<Color x:Key="Status.WarningColor">#FFC107</Color>
<Color x:Key="Status.AlarmColor">#F44336</Color>
<Color x:Key="Status.CriticalColor">#B71C1C</Color>
<Color x:Key="Status.DisabledColor">#9E9E9E</Color>
<SolidColorBrush x:Key="Status.NormalBrush"    Color="{StaticResource Status.NormalColor}"/>
<SolidColorBrush x:Key="Status.WarningBrush"   Color="{StaticResource Status.WarningColor}"/>
<SolidColorBrush x:Key="Status.AlarmBrush"     Color="{StaticResource Status.AlarmColor}"/>
<SolidColorBrush x:Key="Status.CriticalBrush"  Color="{StaticResource Status.CriticalColor}"/>
<SolidColorBrush x:Key="Status.DisabledBrush"  Color="{StaticResource Status.DisabledColor}"/>

<!-- DeviceStatusStyle — background nền equipment thay đổi theo theme, màu status cố định -->
<Style x:Key="DeviceStatusStyle" TargetType="Border">
  <Setter Property="Background" Value="{DynamicResource Equipment.NormalBrush}"/>
  <Setter Property="BorderThickness" Value="1"/>
  <Setter Property="BorderBrush" Value="{DynamicResource Border.DefaultBrush}"/>
  <Style.Triggers>
    <DataTrigger Binding="{Binding State}" Value="Running">
      <Setter Property="BorderBrush" Value="{StaticResource Status.NormalBrush}"/>
      <Setter Property="BorderThickness" Value="2"/>
    </DataTrigger>
    <DataTrigger Binding="{Binding State}" Value="Warning">
      <Setter Property="BorderBrush" Value="{StaticResource Status.WarningBrush}"/>
      <Setter Property="BorderThickness" Value="2"/>
    </DataTrigger>
    <DataTrigger Binding="{Binding State}" Value="Fault">
      <Setter Property="BorderBrush" Value="{StaticResource Status.AlarmBrush}"/>
      <Setter Property="BorderThickness" Value="2"/>
    </DataTrigger>
  </Style.Triggers>
</Style>
```

### 16.7 Animation chỉ cho semantic state

```xml
<!-- CHỈ dùng animation cho alarm nhấp nháy, không trang trí -->
<Style.Triggers>
  <DataTrigger Binding="{Binding IsCriticalAlarm}" Value="True">
    <DataTrigger.EnterActions>
      <BeginStoryboard>
        <Storyboard RepeatBehavior="Forever">
          <DoubleAnimation Storyboard.TargetProperty="Opacity"
                           From="1" To="0.3" Duration="0:0:0.5"
                           AutoReverse="True"/>
        </Storyboard>
      </BeginStoryboard>
    </DataTrigger.EnterActions>
  </DataTrigger>
</Style.Triggers>
```

### 16.8 Virtualization cho danh sách dài

```xml
<!-- Alarm list, log list: luôn dùng virtualization -->
<ListView VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          ScrollViewer.IsDeferredScrollingEnabled="True">
```

### 16.9 Binding cho live data

```csharp
// ViewModel — KHÔNG update từ non-UI thread trực tiếp
private double _position;
public double Position
{
    get => _position;
    set
    {
        // Nếu update từ hardware callback thread:
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SetProperty(ref _position, value);
        });
    }
}
```

---

## 17. Checklist trước khi release

### 17.1 Kiểm tra Màu sắc và Contrast

- [ ] Screenshot màn hình → convert grayscale → vẫn đọc được thông tin không?
- [ ] Màu đỏ/vàng chỉ dùng cho alarm/warning không?
- [ ] Contrast text/nền ≥ 4.5:1 (dùng tool kiểm tra)
- [ ] Equipment ở trạng thái normal hiển thị màu xám, không phải xanh lá

### 17.2 Kiểm tra Theme Switching

- [ ] Đổi từ Dark → Light: toàn bộ màu nền/chữ/border thay đổi — không có element nào vẫn giữ màu hardcode
- [ ] Đổi từ Light → Dark: tương tự
- [ ] Màu semantic (alarm, status) **giữ nguyên** khi đổi theme — đỏ vẫn là đỏ
- [ ] Preference theme được lưu và restore sau khi restart
- [ ] Biểu đồ/chart dùng đúng nền theo theme: tối trong dark, xám-trắng trong light
- [ ] Không có text nào mất đọc được khi đổi theme (contrast đủ ở cả hai)

### 17.2 Kiểm tra Alarm

- [ ] Mọi alarm có Code, Description rõ ràng, Action hướng dẫn
- [ ] Alarm bar hiển thị trên mọi màn hình
- [ ] Critical alarm nhấp nháy đúng 1 Hz
- [ ] Không có alarm nào thiếu bản dịch

### 17.3 Kiểm tra Navigation

- [ ] Mọi màn hình đến được từ Overview trong ≤ 3 click
- [ ] Breadcrumb / title màn hình hiển thị đúng
- [ ] Popup có nút đóng rõ ràng, không che Status Bar

### 17.4 Kiểm tra Data

- [ ] Mọi giá trị số có đơn vị
- [ ] Số thập phân phù hợp với độ chính xác thực tế
- [ ] Giá trị live update đúng tần suất
- [ ] Không có giá trị nào hiển thị `NaN`, `Infinity`, hoặc exception text

### 17.5 Kiểm tra Ergonomics

- [ ] Tất cả nút bấm ≥ 44 × 44 px (touchscreen) hoặc ≥ 24 × 24 px (mouse only)
- [ ] Font size ≥ 12 pt trên mọi text quan trọng
- [ ] E-Stop button lớn nhất, màu đỏ đậm, vị trí cố định
- [ ] Khoảng cách giữa nút nguy hiểm ≥ 48 px

### 17.6 Kiểm tra Đa ngôn ngữ

- [ ] Đổi sang mọi ngôn ngữ hỗ trợ — không có text bị cắt hoặc overflow
- [ ] Tên nút và label đọc được trong mọi ngôn ngữ
- [ ] Không có hardcoded string nào trong XAML hoặc code

### 17.7 Kiểm tra Hiệu năng

- [ ] Khởi động < 10 giây
- [ ] Điều hướng màn hình < 200 ms
- [ ] CPU < 20% khi idle (màn hình không có animation)
- [ ] Không memory leak sau 8 giờ chạy liên tục (kiểm tra Task Manager)

---

## Phụ lục A — Bảng màu tham chiếu đầy đủ

### Semantic Colors — Dùng chung cả hai theme (KHÔNG thay đổi)

```
Token                   Hex        Mô tả
───────────────────────────────────────────────────────────────
Status.Normal           #4CAF50   Running / On — xanh lá muted
Status.Warning          #FFC107   Warning / Advisory — vàng hổ phách
Status.Alarm            #F44336   Alarm / Error — đỏ
Status.Critical         #B71C1C   Critical / E-Stop — đỏ đậm
Status.Disabled         #9E9E9E   Disabled / Off — xám
Status.Acknowledged     #FF8F00   Alarm acknowledged, chưa clear — cam
Status.Manual           #1E88E5   Manual mode — xanh dương
Status.Interlock        #7B1FA2   Interlock active — tím
```

### Dark Theme Tokens

```
Token                        Hex        Token                  Hex
──────────────────────────────────────────────────────────────────────
Screen.Background            #1A1A1A   Text.Primary           #E0E0E0
Panel.Background             #252525   Text.Secondary         #9E9E9E
Panel.Background.Alt         #2D2D2D   Text.Heading           #BDBDBD
Header.Background            #1F1F1F   Text.LiveValue         #FFFFFF
Header.Background.Active     #2A2A2A   Text.Disabled          #616161
Equipment.Normal             #3A3A3A   Chart.Background       #0D1B2A
Equipment.Surface            #424242   Chart.Grid             #1E2D3D
Border.Default               #3D3D3D
Border.Strong                #555555
Divider                      #333333
```

### Light Theme Tokens

```
Token                        Hex        Token                  Hex
──────────────────────────────────────────────────────────────────────
Screen.Background            #F0F2F5   Text.Primary           #212121
Panel.Background             #FFFFFF   Text.Secondary         #757575
Panel.Background.Alt         #F8F9FA   Text.Heading           #424242
Header.Background            #E8ECF0   Text.LiveValue         #000000
Header.Background.Active     #D8DDE3   Text.Disabled          #BDBDBD
Equipment.Normal             #D0D5DB   Chart.Background       #F8FAFC
Equipment.Surface            #BFC5CC   Chart.Grid             #E2E8F0
Border.Default               #D1D5DB
Border.Strong                #9CA3AF
Divider                      #E5E7EB
```

### So sánh trực quan theo môi trường

```
Môi trường                       Theme      Lý do chính
─────────────────────────────────────────────────────────────────
Control room, clean room         Dark       Giảm mỏi mắt, alarm nổi bật
Sàn xưởng ánh sáng mạnh         Light      Chống glare, contrast tốt hơn
Ngoài trời / bán ngoài trời      Light      Ánh sáng môi trường mạnh
Màn hình báo cáo / engineering   Light      Phù hợp đọc lâu, in ấn
Tablet / touchscreen di động     Light      Phản xạ môi trường ít hơn
```

## Phụ lục B — Tài liệu tham chiếu

| Chuẩn | Áp dụng cho |
|-------|-------------|
| **ANSI/ISA-101.01-2015** | Triết lý HMI, phân cấp màn hình, màu sắc, alarm |
| **EEMUA 191 / 201** | Alarm management, alarm rate, priority distribution |
| **ISA-18.2** | Alarm philosophy, lifecycle management |
| **SEMI S2** | Safety requirements cho equipment software |
| **SEMI S8** | Ergonomics: font size, button size, working distance |
| **ASM Consortium** | High-performance HMI guidelines, grayscale philosophy |
| **WCAG 2.1 AA** | Color contrast, accessibility |
| **Material Design 3** | Spacing grid (8px), animation duration, touch targets |

---

*Phiên bản: 1.1 — Bổ sung Light Theme đầy đủ, ThemeService, token system, và checklist theme switching.*
*Áp dụng cho toàn bộ dự án phần mềm máy tự động hoá.*
*Cập nhật tài liệu này khi có quy tắc mới hoặc phát hiện vấn đề trong thực tế.*
