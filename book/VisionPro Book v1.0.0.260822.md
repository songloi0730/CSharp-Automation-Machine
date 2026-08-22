% Xử lý ảnh công nghiệp với Cognex VisionPro
% (Bản nháp — đã qua review kỹ thuật độc lập vòng 1 toàn bộ 16 chương)
% 2026-08-22

---

> **LƯU Ý VỀ BẢN GHÉP NÀY:** Đây là bản ghép nội bộ 16 chương + 3 phụ lục để rà soát tổng
> thể, KHÔNG PHẢI bản xuất bản cuối cùng. Còn tồn tại: (1) 3 điểm `<!-- VERIFY -->` chưa xử lý
> được — 2 điểm (Ch9, Ch10) là số liệu minh hoạ cần tác giả tự đo trên trạm thật, 1 điểm (Ch16)
> cần cài thêm module VisionPro Deep Learning riêng để kiểm chứng; (2) 6/28 hình minh hoạ vẫn là
> placeholder chờ tác giả tự chụp screenshot QuickBuild thật (danh sách trong `assets/README.md`),
> 22/28 hình còn lại đã có ảnh thật; (3) toàn bộ 16 chương đã qua review kỹ thuật độc lập ba nguồn
> (ChatGPT + DeepSeek + Gemini) theo quy trình `vision-external-ai-review` — các nhận định
> CRITICAL/IMPORTANT đã được thẩm định độc lập (đối chiếu vật lý/toán/giao thức/ngữ nghĩa C#)
> trước khi áp dụng; các nhận định không kiểm chứng được hoặc chỉ một nguồn nêu mà không có bằng
> chứng cụ thể đã bị loại bỏ. Chi tiết từng đợt xem `CHANGELOG.md`.

## Miễn trừ trách nhiệm về nhãn hiệu

Cognex®, VisionPro®, QuickBuild™ và PatMax® là nhãn hiệu đã đăng ký của Cognex Corporation.
HALCON® là nhãn hiệu đã đăng ký của MVTec Software GmbH. GigE Vision® và GenICam™ là tiêu chuẩn
mở do AIA (Association for Advancing Automation) và EMVA (European Machine Vision Association)
quản lý. Mọi nhãn hiệu, tên sản phẩm được đề cập trong sách thuộc quyền sở hữu của chủ sở hữu
tương ứng.

Cuốn sách này là tài liệu học thuật độc lập do tác giả biên soạn nhằm mục đích giáo dục, dựa trên
tài liệu kỹ thuật công khai và kinh nghiệm triển khai thực tế của tác giả. Sách **không được**
Cognex Corporation, MVTec Software GmbH, hay bất kỳ nhà sản xuất phần cứng/phần mềm nào được đề
cập tài trợ, chứng thực, xem xét trước, hoặc bảo trợ dưới bất kỳ hình thức nào.

Ứng dụng mẫu **MeoVision** và nền tảng máy **MeoFrame** xuyên suốt sách là mô hình hư cấu, được
xây dựng riêng cho mục đích minh hoạ giảng dạy — không đại diện cho bất kỳ hệ thống, khách hàng,
hay dự án thực tế cụ thể nào. Mọi số liệu kỹ thuật (kích thước, dung sai, ngưỡng đo, thời gian chu
kỳ...) đi kèm là ví dụ minh hoạ hợp lý về bậc độ lớn, không phải số đo thực tế của một trạm sản
xuất cụ thể — người đọc cần tự đo đạc, hiệu chỉnh trên hệ thống của mình trước khi áp dụng vào sản
xuất.

\newpage

# Chương 1 — Thị giác máy trong tự động hoá công nghiệp

Ca sáng thứ Hai, chuyền trưởng gọi xuống xưởng: máy lắp nắp trên cho một module điều khiển điện
tử (máy chạy trên khung điều khiển **MeoFrame** — tên gọi chung cho nền tảng phần mềm điều khiển
máy tự động dùng xuyên suốt các ví dụ trong sách) tuần này dừng khẩn ba lần vì tay robot ép nắp lệch,
gãy mất chốt định vị nhựa bên dưới. Xem lại thao tác, mọi công đoạn đều đúng quy trình: người vận hành đặt chi tiết
lên khay gá, liếc nhìn xem có lệch tâm không, rồi bấm nút cho robot xuống ép. Vấn đề nằm đúng ở
chữ "liếc nhìn" đó. Từ khi chuyền tăng tốc để đạt kế hoạch mới, thời gian mắt người còn lại để
nhận ra chi tiết lệch 2-3 mm co xuống chưa đầy nửa giây — ngắn hơn cả một cái chớp mắt bình
thường. Ba trong bốn ca đêm gần nhất, tỉ lệ ép lệch cao gấp đôi ban ngày, không phải vì người vận
hành ca đêm kém tay nghề hơn, mà vì sau nhiều giờ đứng máy, phản xạ "liếc — nhận định — bấm nút"
chậm dần đi một cách rất con người mà không ai có thể trách được.

Câu trả lời quản đốc đưa ra nghe rất hợp lý: "gắn camera lên, cho máy tự nhìn thay người." Nhưng
gắn camera nào, đặt ở đâu, chiếu sáng ra sao để "nhìn" được chi tiết lệch tâm rõ ràng hơn cả mắt
người? Và quan trọng hơn: camera chụp được một tấm ảnh rồi, cái gì đọc tấm ảnh đó ra quyết định
"lệch — dừng lại, đừng ép" trong đúng khoảng thời gian mà con người vừa không kịp làm? Đây chính
là bài toán mà cả một ngành công nghiệp — thị giác máy công nghiệp, hay **machine vision** — tồn
tại để giải quyết. Và đây cũng là điểm khởi đầu của **MeoVision**, ứng dụng mẫu mà chúng ta sẽ
xây dựng dần suốt cuốn sách này: một trạm vision đặt trước máy lắp ráp MeoFrame, đảm nhận đúng
việc mắt người vừa không còn kịp làm — kiểm tra kích thước chi tiết, đọc mã truy vết, và bắt đúng
vị trí để robot ép/gắp không còn đoán mò.

Chương này đặt nền cho toàn bộ phần còn lại của sách: chúng ta sẽ làm rõ machine vision khác gì
với khái niệm "thị giác máy tính" (computer vision) rộng hơn mà nhiều người mới vào nghề hay
nhầm lẫn, và những ràng buộc riêng của môi trường công nghiệp buộc nó phải khác đi (mục 1.1). Kế
đến là bốn dạng bài toán mà gần như mọi ứng dụng vision công nghiệp đều quy về — viết tắt GIGI
(mục 1.2). Rồi chúng ta tháo rời một hệ vision thành các khối phần cứng/phần mềm nối tiếp nhau,
từ đèn đến tín hiệu trả về PLC (mục 1.3), và đặt hệ đó vào đúng vị trí của nó trong một máy tự
động — nó giao tiếp với PLC ra sao, bị ràng buộc thời gian như thế nào (mục 1.4). Cuối chương là
một cái nhìn khái quát về bức tranh công cụ trên thị trường — smart camera hay PC-based, và
VisionPro đứng ở đâu trong đó (mục 1.5).

Vì đây là chương mở đầu, cho phép chúng ta nói trước một lần về lộ trình cả cuốn sách: **Phần I**
(chương này đến Chương 4) xây nền tảng thị giác máy thuần tuý — ánh sáng, ống kính, camera, xử lý
ảnh cơ bản — chưa cần mở phần mềm VisionPro lên. **Phần II** (Chương 5-6) làm quen VisionPro và
QuickBuild, dựng job vision đầu tiên. **Phần III** (Chương 7-12) là bộ công cụ GIGI đầy đủ của
VisionPro, mở đầu bằng calibration/fixturing — nền móng bắt buộc trước khi bất kỳ tool đo/định vị
nào cho ra số đáng tin. **Phần IV** (Chương 13-15) đưa job từ QuickBuild vào một ứng dụng C# thật
sự, chạy độc lập, giao tiếp PLC/robot. **Phần V** (Chương 16) khép lại bằng câu hỏi vận hành: làm
sao biết một hệ "chạy được" thật sự đủ tin cậy để sản xuất 24/7. Xuyên suốt cả năm phần, trạm
MeoVision bồi thêm một lớp mới ở mỗi phần — đến cuối sách, nó là một trạm vision hoàn chỉnh, từ
phần cứng chọn đúng cho đến phần mềm chạy ổn định trong dây chuyền.

## 1.1 Machine vision khác computer vision ở đâu

### 1.1.1 Cùng gốc, khác ràng buộc

Về mặt thuật toán, **machine vision** (thị giác máy công nghiệp) và **computer vision** (thị giác
máy tính, thường viết tắt CV) dùng chung một nền tảng: cùng xử lý ảnh số, cùng các kỹ thuật phát
hiện biên, phân vùng, khớp mẫu, và ngày càng nhiều cùng dùng mạng nơ-ron. Sự khác biệt không nằm
ở thuật toán bên trong, mà ở **ràng buộc của bài toán đặt ra**. Một ứng dụng CV điển hình — nhận
diện khuôn mặt trong ảnh mạng xã hội, phân loại ảnh động vật trong bộ dữ liệu nghiên cứu, xe tự
hành đọc biển báo trên đường phố muôn hình vạn trạng — phải chấp nhận ánh sáng bất kỳ, cảnh vật
bất kỳ, và một câu trả lời sai thỉnh thoảng thường chỉ gây phiền toái chứ không dừng cả dây
chuyền.

Quay lại tình huống mở đầu: trạm vision trước máy MeoFrame không có "cảnh vật bất kỳ" — nó luôn
nhìn đúng một loại chi tiết, đặt trên đúng một khay gá, dưới đúng một nguồn sáng do chính chúng ta
lắp và điều khiển. Đổi lại, nó phải trả lời trong một khoảng thời gian cố định thuộc về nhịp máy
(cycle time), và câu trả lời của nó **kích hoạt hành động vật lý** — robot ép xuống hay dừng lại.
Trả lời sai không phải "phiền toái", mà là chốt nhựa gãy, chi tiết hỏng, hoặc nặng hơn là va chạm
cơ khí. Ba khác biệt đó — ràng buộc thời gian, ánh sáng chủ động, và hậu quả của một quyết định
sai — chính là thứ định nghĩa machine vision, không phải một thuật toán cụ thể nào.

**Bảng 1.1 — Computer vision (nói chung) và machine vision (công nghiệp): cùng gốc, khác ràng buộc.**

| Khía cạnh | Computer vision nói chung | Machine vision công nghiệp |
|---|---|---|
| Cảnh chụp | Đa dạng, không kiểm soát trước | Cố định: một loại chi tiết, một vị trí camera |
| Ánh sáng | Bất kỳ (ánh sáng tự nhiên, trong nhà, ban đêm...) | **Chủ động thiết kế** — ta chọn và lắp đèn (Chương 2) |
| Thời gian phản hồi | Thường không ràng buộc chặt, hoặc "càng nhanh càng tốt" | **Ràng buộc cứng** theo cycle time của máy (mục 1.4) |
| Độ lặp lại yêu cầu | Không đòi hỏi tuyệt đối giống nhau giữa các lần chạy | Phải lặp lại được hàng chục nghìn lần/ca, mọi ca |
| Hậu quả một lần sai | Thường chỉ ảnh hưởng kết quả hiển thị/thống kê | Có thể dừng máy, hỏng chi tiết, va chạm cơ khí |
| Ai/cái gì ra quyết định cuối | Bản thân hệ thống CV | Vision **đề xuất**, PLC/bộ điều khiển máy quyết định |
| Môi trường vận hành | Máy chủ, máy tính cá nhân, cloud | Tủ điện, bụi, rung, nhiệt độ dao động, chạy 24/7 |

### 1.1.2 Vì sao ràng buộc công nghiệp lại quan trọng đến vậy

Ba ràng buộc ở Bảng 1.1 không phải chi tiết vụn vặt — chúng quyết định gần như toàn bộ cách sách
này được viết. Ràng buộc thời gian là lý do Phần IV dành hẳn một chương (Chương 15) cho ngân sách
thời gian và bắt tay PLC-vision. Ràng buộc ánh sáng chủ động là lý do Chương 2 xếp ngay sau chương
này, trước cả camera — trong ngành, người ta hay nói ánh sáng quyết định 80% thành bại của một hệ
vision, và ta sẽ thấy đúng nghĩa đen ở đó. Còn hậu quả của một quyết định sai là lý do nguyên tắc
cuối Bảng 1.1 — "vision đề xuất, PLC quyết định" — sẽ quay lại nhiều lần xuyên suốt sách như một
bất biến an toàn, không phải một câu khẩu hiệu.

> 📌 **Lưu ý:** đừng hiểu "machine vision" là một nhánh công nghệ tách biệt khỏi computer
> vision/deep learning hiện đại. Rất nhiều hệ machine vision ngày nay dùng mạng nơ-ron cho một
> phần bài toán (Chương 16, mục 16.5 nói về VisionPro Deep Learning). Ranh giới nằm ở **ràng
> buộc vận hành**, không nằm ở việc thuật toán "cổ điển" hay "học sâu".

> ⚠️ **Cảnh báo:** nhầm hai khái niệm này là điểm khởi đầu của rất nhiều dự án vision công nghiệp
> thất bại. Một mô hình phân loại ảnh "độ chính xác 99%" luyện trên vài trăm tấm ảnh chụp tay
> trong phòng lab, ánh sáng phòng lab, hoàn toàn có thể tụt xuống độ chính xác 60% khi mang ra
> chuyền vào ca đêm dưới đèn huỳnh quang nhấp nháy — không phải vì mô hình dở, mà vì bài toán đưa
> cho nó chưa từng được ràng buộc theo kiểu công nghiệp.

## 1.2 Bốn bài toán GIGI

### 1.2.1 Một khung phân loại thực dụng

Trước khi chọn camera, chọn đèn, hay nghĩ đến bất kỳ thuật toán nào, câu hỏi đầu tiên và quan
trọng nhất khi tiếp nhận một yêu cầu vision là: **đây thuộc loại bài toán nào trong bốn loại
sau?** Ngành công nghiệp gọi tắt bốn loại này là **GIGI**:

- **Guidance** (dẫn hướng) — cho biết chi tiết đang ở đâu, xoay bao nhiêu, để một cơ cấu chuyển
  động (robot, trục servo) đến đúng vị trí đó. Câu hỏi trả lời: *"nó ở đâu?"*
- **Inspection** (kiểm tra) — xác định chi tiết có khuyết tật, có đúng cấu trúc mong đợi hay
  không (nứt, thiếu chi tiết con, sai màu, biến dạng). Câu hỏi trả lời: *"nó có ổn không?"*
- **Gauging** (đo lường) — đo một kích thước cụ thể và so với dung sai cho phép (đường kính,
  khoảng cách hai cạnh, độ đồng phẳng). Câu hỏi trả lời: *"nó lớn/nhỏ bao nhiêu?"*
- **Identification** (nhận dạng/đọc mã) — đọc một mã hoặc chuỗi ký tự gắn trên chi tiết để truy
  vết hoặc phân loại (mã vạch, DataMatrix, ký tự khắc laser). Câu hỏi trả lời: *"nó là cái nào?"*

Bốn câu hỏi nghe đơn giản, nhưng phân loại đúng ngay từ đầu quyết định gần như mọi lựa chọn kỹ
thuật phía sau: một bài toán Gauging đòi hỏi độ chính xác quang học và calibration nghiêm ngặt
hơn hẳn một bài toán Guidance chỉ cần "đủ gần đúng" để robot gắp không trượt tay. Một bài toán
Identification quan tâm đến độ tương phản của mã nhiều hơn là màu sắc tổng thể của chi tiết.
Nhầm loại bài toán ngay từ đầu là nguyên nhân phổ biến của việc chọn sai phần cứng — ta sẽ quay
lại điều này ở phần "Lỗi thường gặp" cuối chương.

Trạm MeoVision mở đầu chương này hoá ra đã ôm gọn cả bốn bài toán GIGI trong cùng một dây chuyền
kiểm tra: **Guidance** khi báo vị trí/góc xoay chính xác cho robot ép/gắp — chính là bài toán
"lệch tâm" đã làm gãy chốt nhựa ở đầu chương; **Gauging** khi kiểm tra chi tiết có nằm trong dung
sai kích thước cho phép; **Inspection** khi đếm đủ bốn miếng đệm cao su dán trên vỏ nhôm và kiểm
tra bề mặt anodized có vết xước/rỗ hay không — bài toán "nó có ổn không?" thuần tuý, không cần đo
số hay đọc mã; và **Identification** khi đọc mã DataMatrix truy vết in trên chi tiết.

**Bảng 1.2 — Bốn bài toán GIGI: câu hỏi, ví dụ, lớp thuật toán liên quan.**

| Bài toán | Câu hỏi trả lời | Ví dụ (MeoVision) | Lớp thuật toán thường dùng |
|---|---|---|---|
| **G**uidance | Nó ở đâu, xoay bao nhiêu? | Bắt vị trí/góc xoay chi tiết trước khi robot ép/gắp | Định vị mẫu (pattern matching) |
| **I**nspection | Nó có ổn không? | Đếm đủ 4 miếng đệm cao su, kiểm vết xước bề mặt anodized | Phân vùng + đo thuộc tính vùng (blob) |
| **G**auging | Nó lớn/nhỏ bao nhiêu? | Đo khoảng cách hai cạnh, đường kính lỗ định vị | Đo biên cạnh (edge-based measurement) |
| **I**dentification | Nó là cái nào? | Đọc mã DataMatrix truy vết trên chi tiết | Giải mã ký hiệu / nhận dạng ký tự |

> 📌 **Lưu ý:** GIGI là khung phân loại theo *mục đích trả lời*, không phải theo *thuật toán*.
> Một job vision thực tế thường kết hợp nhiều bài toán trong cùng chuỗi xử lý — ví dụ trạm
> MeoVision vừa Guidance (định vị chi tiết) vừa Gauging (đo kích thước) trên cùng một tấm ảnh,
> vì tool đo phía sau cần chi tiết đã được định vị mới đo đúng chỗ (nguyên lý này là trọng tâm
> của Chương 7, mục 7.4 — fixturing).

### 1.2.2 Vì sao phải phân loại trước khi làm gì khác

Xác định đúng bài toán GIGI ngay từ buổi khảo sát đầu tiên giúp trả lời nhanh hàng loạt câu hỏi
kéo theo: cần độ chính xác đến đâu (Gauging đòi hỏi khắt khe hơn Guidance rất nhiều), có cần
calibration hệ mm hay chỉ cần toạ độ tương đối (Guidance cho robot cùng hệ có thể không cần quy
đổi tường minh — xem Chương 7, mục 7.5), có cần huấn luyện mẫu "tốt/xấu" đa dạng hay không
(Inspection thường cần nhiều biến thể mẫu hơn Identification). Phần III của sách (Chương 8-11)
thực chất được tổ chức theo đúng bốn nhóm bài toán này: định vị mẫu là công cụ chủ lực cho
Guidance, Caliper/Blob cho Gauging và Inspection, ID/OCR cho Identification.

> 💡 **Mẹo thực chiến:** khi nhận một yêu cầu vision mới, trước khi hỏi "camera nào, đèn nào",
> hãy dành 5 phút viết ra: bài toán này là G nào trong GIGI (có thể nhiều hơn một), dung sai/độ
> chính xác cần đạt là bao nhiêu, và hậu quả nếu vision báo sai là gì. Ba câu trả lời đó định
> hướng toàn bộ lựa chọn phần cứng ở Phần I, còn hỏi ngược lại — chọn phần cứng trước, tìm bài
> toán sau — là con đường ngắn nhất dẫn đến việc phải mua lại thiết bị.

## 1.3 Giải phẫu một hệ thị giác máy

### 1.3.1 Sáu khối nối tiếp nhau

Bất kể bài toán GIGI nào, mọi hệ machine vision công nghiệp đều được ráp từ cùng một chuỗi khối
chức năng nối tiếp, mỗi khối chuyển thứ nó nhận được thành thứ khối sau cần:

```text
Đèn  →  Ống kính  →  Camera  →  Frame grabber / GigE  →  Phần mềm  →  I/O ra PLC/robot
(chiếu    (hội tụ      (biến      (đưa dữ liệu ảnh vào    (xử lý     (kết quả OK/NG
sáng có   ánh sáng     ánh sáng    bộ nhớ máy tính)        ảnh, ra    hoặc toạ độ,
kiểm      lên cảm      thành                               quyết      qua tín hiệu
soát)     biến)        ảnh số)                             định)      số/mạng)
```

Đèn tạo ra sự tương phản giữa đặc trưng cần thấy và phần còn lại — đây là khối duy nhất chúng ta
*chủ động thiết kế* thay vì chỉ chọn thiết bị có sẵn, và là chủ đề của toàn bộ Chương 2. Ống kính
hội tụ ánh sáng phản xạ từ chi tiết lên bề mặt cảm biến, quyết định vùng nhìn thấy (field of
view, viết tắt FOV) và độ nét; Chương 2 cũng bàn kỹ phần này. Camera chuyển ánh sáng hội tụ được
thành một ma trận số — bức ảnh số theo đúng nghĩa mà phần mềm sẽ xử lý (Chương 3). Khối thứ tư —
"Frame grabber/GigE" — là nhãn gộp cho một trong vài chuẩn giao tiếp có thể có ở đây (GigE Vision,
USB3 Vision, CameraLink, CoaXPress — Chương 3, mục 3.4 so sánh chi tiết); một số chuẩn (GigE, USB3
Vision) đưa dữ liệu ảnh thẳng về bộ nhớ máy tính qua card mạng/cổng thông thường, số khác
(CameraLink, CoaXPress) cần một card frame grabber vật lý riêng. Cùng lúc đó, một tín hiệu **trigger
phần cứng** — thường đi từ PLC hoặc một cảm biến vị trí — được đấu **vào** camera hoặc frame
grabber để ra lệnh bắt đầu phơi sáng đúng thời điểm (chiều tín hiệu ngược với chiều dữ liệu ảnh;
cả hai chủ đề này Chương 3 bàn kỹ). Phần mềm — nơi VisionPro sống, từ Chương 5 trở đi — nhận ảnh
số, chạy chuỗi xử lý phù hợp với bài toán GIGI đã xác định, và ra một kết quả. Cuối cùng, kết quả
đó phải rời khỏi phần mềm để có ý nghĩa với máy: qua tín hiệu số (DI/DO) hoặc mạng (fieldbus —
các giao thức mạng công nghiệp như Profinet, EtherNet/IP — hoặc Ethernet) gửi về PLC hoặc robot
controller — chủ đề của Chương 15.

![Hình 1.1 — Giải phẫu một hệ thị giác máy công nghiệp: sáu khối nối tiếp](../assets/ch01/hinh_1_1.png)
**Hình 1.1 — Giải phẫu một hệ thị giác máy công nghiệp: sáu khối nối tiếp.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ khối ngang (vẽ draw.io), 6 hộp nối tiếp bằng mũi tên
> theo đúng thứ tự Đèn → Ống kính → Camera → Frame grabber/GigE → Phần mềm → I/O ra PLC/robot.
> Mỗi hộp có icon đơn giản tương ứng (bóng đèn, thấu kính, camera công nghiệp, cổng mạng/cáp,
> màn hình máy tính có biểu tượng ảnh, tủ điện PLC + icon robot). Bên dưới mỗi hộp ghi nhỏ số
> chương sách bàn chi tiết khối đó: "Ch.2", "Ch.2", "Ch.3", "Ch.3/6", "Ch.5-12", "Ch.13/15". Phía
> trên cùng, một chi tiết cơ khí mẫu (ví dụ vỏ nhựa MeoVision) đặt bên trái hộp Đèn để ngụ ý đây
> là điểm bắt đầu chuỗi ánh sáng phản xạ.

### 1.3.2 Bốn khối đầu quyết định "nhìn thấy gì", hai khối sau quyết định "làm được gì"

Có một cách nhìn hữu ích khi mới vào nghề: bốn khối đầu tiên (đèn, ống kính, camera, frame
grabber/GigE) hoàn toàn thuộc về vật lý — chúng quyết định tấm ảnh số cuối cùng *chứa* bao nhiêu
thông tin hữu ích. Hai khối cuối (phần mềm, I/O) thuộc về xử lý và tích hợp — chúng quyết định
thông tin có sẵn đó được *khai thác* và *sử dụng* tốt đến đâu. Điều quan trọng cần nhớ: phần mềm,
dù mạnh đến đâu, không thể tạo ra thông tin mà bốn khối đầu chưa từng ghi lại. Một cạnh bị nhoè vì
đèn sai hướng thì không có tool đo biên nào "đoán" lại được cạnh sắc nét — đây là lý do sách dành
trọn Phần I (bốn chương, chưa động đến phần mềm) để xây nền tảng cho phần vật lý đó trước.

> 🔍 **Đào sâu thêm:** hình vẽ ở Hình 1.1 minh hoạ một camera 2D truyền thống. Với một số bài
> toán — bề mặt cong liên tục cần "cuộn" qua camera, hoặc cần đo cả chiều cao/thể tích — chuỗi
> khối trên có biến thể: camera line scan (chụp từng dòng khi vật di chuyển) hoặc camera 3D
> (trả về bản đồ độ sâu thay vì ảnh xám/màu). Sách này nhận diện hai loại bài toán đó ở Chương 3,
> mục 3.6, nhưng không triển khai chi tiết — phần lớn ứng dụng lắp ráp/kiểm tra công nghiệp,
> bao gồm MeoVision, dùng camera 2D diện tích (area scan) là đủ.

## 1.4 Vision đứng đâu trong một máy tự động

### 1.4.1 Vision là một cảm biến "thông minh", không phải một máy độc lập

Phần mềm điều khiển một máy tự động điển hình được tổ chức phân lớp: một bộ điều phối tổng
(master controller) điều phối các trạm (station), mỗi trạm điều phối các cơ cấu (mechanism), và
mỗi cơ cấu mới là nơi gọi xuống phần cứng của riêng nó. Cách dễ nhất để định vị trạm vision trong
bức tranh đó là: **vision đóng vai trò một cảm biến**, chỉ khác cảm biến quang thông thường (chỉ
trả về có/không có vật) ở chỗ nó trả về một khối thông tin phong phú hơn nhiều — toạ độ, kích
thước đo được, chuỗi ký tự đọc được, hoặc kết luận OK/NG. Nó không tự ý quyết định robot chạy hay
băng tải dừng; nó cung cấp dữ liệu để **tầng điều khiển gọi nó đưa ra quyết định**, đúng tinh
thần phân lớp của kiến trúc điều khiển máy.

### 1.4.2 Một chu trình chuẩn: trigger — chụp — xử lý — trả kết quả

Trong vận hành thực tế, một hệ vision hiếm khi tự chụp ảnh liên tục và tự quyết định lúc nào có
kết quả. Nó chờ **tín hiệu trigger** từ PLC — báo hiệu "chi tiết đã vào đúng vị trí, chụp đi" —
rồi chạy đúng bốn bước theo thứ tự: nhận trigger, chụp ảnh, xử lý ảnh theo job đã cấu hình, và trả
kết quả ngược lại PLC. PLC, đến lượt nó, chờ đúng kết quả đó trước khi cho phép bước tiếp theo của
chu trình máy (robot di chuyển, băng tải chạy, cơ cấu ép xuống).

![Hình 1.2 — Chu trình trigger–chụp–xử lý–trả kết quả giữa PLC và hệ vision](../assets/ch01/hinh_1_2.png)
**Hình 1.2 — Chu trình trigger–chụp–xử lý–trả kết quả giữa PLC và hệ vision.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sequence diagram hai làn dọc (vẽ draw.io hoặc PlantUML rồi
> xuất ảnh), làn trái nhãn "PLC", làn phải nhãn "Hệ vision". Mũi tên 1: PLC → Vision, nhãn
> "Trigger (DI)". Bên trong làn Vision, một khối nhỏ nhãn "Chụp ảnh" nối tiếp khối "Xử lý ảnh
> (job GIGI)" — hai khối này nằm trong một dấu ngoặc nhọn ghi chú "Ngân sách thời gian vision
> (mục 1.4.3)". Mũi tên 2: Vision → PLC, nhãn "Kết quả: OK/NG hoặc toạ độ (DO/mạng)". Mũi tên 3
> (nét đứt, quay lại làn PLC): "PLC quyết định bước kế tiếp — chạy robot / dừng / reject". Ghi
> chú nhỏ cuối hình: "Vision đề xuất — PLC quyết định."

### 1.4.3 Ràng buộc thời gian: cycle time không đợi ai

Chu trình ở Hình 1.2 phải hoàn tất trong một phần của **cycle time** — tổng thời gian máy hoàn
thành một chi tiết. Nếu máy chạy 14 chi tiết/phút, cả chu trình cơ khí lẫn vision phải gói gọn
trong xấp xỉ 4.3 giây, và vision thường chỉ được cấp một phần nhỏ trong số đó — phần còn lại dành
cho robot di chuyển, cơ cấu kẹp, băng tải. Vision chạy chậm hơn ngân sách được cấp không "làm hỏng
kết quả" theo nghĩa sai số đo — nó làm **chậm cả dây chuyền**, hoặc buộc PLC phải bỏ qua kết quả
vì đã hết thời gian chờ (timeout). Đây là lý do ràng buộc thời gian xuất hiện ở ngay Bảng 1.1 như
một trong ba đặc điểm cốt lõi phân biệt machine vision với computer vision nói chung — và là lý
do Chương 15 dành hẳn một mục (15.2) để bàn cách đo và cam kết ngân sách này một cách nghiêm túc,
không phải áng chừng.

> ⚠️ **Cảnh báo:** đừng thiết kế hệ vision với giả định "cứ xử lý xong lúc nào thì trả kết quả
> lúc đó". Không có timeout ở phía PLC khi vision xử lý chậm hoặc treo là một trong những lỗi
> thiết kế nguy hiểm nhất khi ghép vision vào máy tự động — máy có thể đứng chờ vô thời hạn, hoặc
> tệ hơn, chạy tiếp với dữ liệu cũ của cycle trước. Chương 15, mục 15.1 quay lại vấn đề này bằng
> một sequence diagram đầy đủ cả hai chiều timeout.

### 1.4.4 Vision chỉ đề xuất — máy mới quyết định

Ghi chú nhỏ ở cuối Hình 1.2 gói gọn một nguyên tắc an toàn sẽ lặp lại nhiều lần trong sách:
**vision không bao giờ trực tiếp ra lệnh cho robot chạy hay cơ cấu chấp hành di chuyển.** Nó gửi
một đề xuất — toạ độ, kết luận OK/NG — kèm cờ báo mức độ tin cậy của chính đề xuất đó (ví dụ có
tìm thấy chi tiết hay không). PLC hoặc robot controller là nơi kiểm tra lại đề xuất đó có hợp lý
không (toạ độ có nằm trong vùng làm việc an toàn, kết quả có đến đúng lúc mong đợi) trước khi
biến nó thành chuyển động vật lý thật sự. Lý do đơn giản: vision là phần mềm chạy trên máy tính
công nghiệp, có thể treo, có thể trả về dữ liệu rác nếu gặp lỗi không lường trước — để một hệ như
vậy có toàn quyền điều khiển trực tiếp cơ cấu chấp hành là đặt cả an toàn máy vào tay một thành
phần chưa từng được thiết kế cho việc đó.

## 1.5 [NÂNG CAO] Bức tranh công cụ trên thị trường

### 1.5.1 Smart camera và PC-based: hai kiến trúc, hai đánh đổi

Nhìn lại sáu khối ở Hình 1.1, thị trường vision công nghiệp giải quyết chúng theo hai kiến trúc
chính. **Smart camera** (camera thông minh) gói camera, bộ xử lý, và phần mềm xử lý ảnh vào chung
một thân thiết bị — cắm điện, cắm mạng, cấu hình qua giao diện web hoặc phần mềm đi kèm là chạy
được, không cần một máy tính công nghiệp riêng. **Hệ PC-based** tách camera (chỉ làm nhiệm vụ thu
ảnh) khỏi phần xử lý — ảnh được truyền qua GigE/USB3 Vision về một máy tính công nghiệp, nơi phần
mềm (như VisionPro) chạy chuỗi xử lý.

**Bảng 1.3 — Smart camera và hệ PC-based: đặc điểm và khi nào chọn phương án nào.**

| Khía cạnh | Smart camera | Hệ PC-based |
|---|---|---|
| Sức mạnh xử lý | Giới hạn theo phần cứng gắn sẵn trong thân camera | Theo cấu hình máy tính — mở rộng được, kể cả GPU |
| Số camera trên một trạm | Thường 1 camera/1 thiết bị độc lập | Nhiều camera cùng chia sẻ một máy tính, một job |
| Độ phức tạp bài toán phù hợp | Bài toán đơn giản, ổn định (đọc mã, kiểm tra hiện diện) | Bài toán phức tạp, nhiều bước, nhiều tool phối hợp |
| Tích hợp vào ứng dụng riêng | Hạn chế — chủ yếu qua giao thức mạng/PLC có sẵn | Sâu — nhúng thẳng vào ứng dụng .NET (Phần IV sách này) |
| Chi phí/thiết bị | Thấp hơn cho bài toán đơn | Cần thêm máy tính công nghiệp, nhưng chia sẻ nhiều camera |
| Bảo trì/thay thế | Đơn giản — thay nguyên khối | Cần quản lý cả phần cứng lẫn máy tính riêng |

Không có phương án nào "hơn" tuyệt đối — smart camera là lựa chọn hợp lý cho một trạm đọc mã đơn
giản, tách biệt; hệ PC-based phù hợp hơn khi bài toán cần nhiều tool phối hợp trên cùng một tấm
ảnh (đúng tình huống của MeoVision — định vị, đo, đọc mã trên cùng một chi tiết) hoặc khi ứng
dụng cần được nhúng sâu vào phần mềm điều khiển máy do chính đội ngũ phát triển, thay vì phụ
thuộc giao diện cấu hình có sẵn của nhà sản xuất camera.

### 1.5.2 VisionPro đứng ở đâu

Trên nhánh PC-based, một vài bộ công cụ (SDK) phổ biến trong công nghiệp gồm **VisionPro** của
Cognex, **HALCON** của MVTec, và thư viện mã nguồn mở **OpenCV**. Cognex cũng có dòng **In-Sight**
theo kiến trúc smart camera, dùng chung một số thuật toán lõi với VisionPro nhưng đóng gói khác
hẳn. Không đi sâu so sánh (sách chủ đích không dạy HALCON hay OpenCV — xem "Ghi chú phạm vi" của
outline sách), điểm khác biệt lớn nhất giữa ba lựa chọn nằm ở mức độ *đóng gói sẵn*: OpenCV là thư
viện hàm xử lý ảnh thuần tuý, mạnh và miễn phí, nhưng không có sẵn khái niệm tool GIGI đóng gói,
không có giao diện thiết kế job trực quan, không có cơ chế calibration/fixturing tích hợp — tất cả
phải tự xây từ đầu. VisionPro và HALCON đi xa hơn: cung cấp sẵn bộ tool đã đóng gói cho từng bài
toán GIGI, một môi trường thiết kế trực quan để dựng và thử job trước khi viết code (với VisionPro
là QuickBuild, chủ đề Chương 5), và cơ chế calibration/fixturing như một phần cốt lõi của kiến
trúc — không phải thứ người dùng tự lắp ghép.

Sách này chọn VisionPro cho ba lý do gắn liền với chính độc giả mà sách hướng đến: nó tích hợp
trực tiếp và tự nhiên vào .NET/C# — đúng nền tảng lập trình mà sách này giả định người đọc đã
vững, không cần học
thêm một ngôn ngữ hay môi trường lập trình riêng; bộ tool của nó phủ đủ cả bốn bài toán GIGI trong
một kiến trúc nhất quán (cùng khái niệm tool, terminal, coordinate space xuyên suốt — sẽ thấy rõ
từ Chương 5); và nó có bề dày sử dụng thực tế lâu năm trong ngành lắp ráp/kiểm tra công nghiệp,
đúng bối cảnh mà MeoVision mô phỏng.

> 🔍 **Đào sâu thêm:** ranh giới giữa rule-based (các tool GIGI "cổ điển", dựa trên đặc trưng hình
> học/tương phản tường minh — toàn bộ Phần III của sách) và deep learning (mạng nơ-ron học đặc
> trưng từ dữ liệu mẫu) đang mờ dần trong ngành. VisionPro có module riêng cho hướng học sâu, gọi
> tắt ViDi (VisionPro Deep Learning), hữu ích khi khuyết tật cần tìm không định nghĩa được rõ
> ràng bằng luật hình học (ví dụ: "vết bẩn bất thường" có hình dạng thiên biến vạn hoá). Sách dành
> một mục khái quát cho hướng này ở Chương 16, mục 16.5 — đủ để nhận diện khi nào rule-based đã
> đuối và cần cân nhắc sang hướng đó, không đi sâu huấn luyện mô hình.

## Tổng kết chương

- **Machine vision** khác **computer vision** không phải ở thuật toán, mà ở ba ràng buộc công
  nghiệp: ánh sáng chủ động thiết kế, thời gian phản hồi ràng buộc cứng theo cycle time, và hậu
  quả vật lý thật sự khi trả lời sai — vision đề xuất, máy/PLC mới quyết định.
- Gần như mọi ứng dụng vision công nghiệp quy về bốn bài toán **GIGI**: Guidance (dẫn hướng),
  Inspection (kiểm tra), Gauging (đo lường), Identification (đọc mã/ký tự). Xác định đúng loại
  bài toán trước khi chọn phần cứng là bước đầu tiên của mọi dự án nghiêm túc.
- Một hệ vision là chuỗi sáu khối nối tiếp: đèn → ống kính → camera → frame grabber/GigE →
  phần mềm → I/O ra PLC/robot. Bốn khối đầu quyết định thông tin *có sẵn* trong ảnh; phần mềm
  không tạo ra được thông tin mà phần cứng chưa từng ghi lại.
- Trong kiến trúc máy tự động, vision đóng vai trò một cảm biến giàu thông tin: chờ trigger từ
  PLC, chụp và xử lý trong một ngân sách thời gian nằm trong cycle time tổng, rồi trả kết quả —
  không bao giờ tự ý điều khiển trực tiếp cơ cấu chấp hành.
- Thị trường có hai kiến trúc chính — smart camera (gọn, đơn giản, phù hợp bài toán đơn) và
  PC-based (mạnh, linh hoạt, phù hợp bài toán nhiều tool phối hợp và tích hợp sâu vào ứng dụng
  riêng). VisionPro thuộc nhánh PC-based, được sách này chọn vì tích hợp tự nhiên vào .NET/C#
  và phủ đủ cả bốn bài toán GIGI trong một kiến trúc nhất quán.
- Từ Chương 2, sách bắt đầu đi sâu vào từng khối của Hình 1.1, bắt đầu từ khối quan trọng nhất
  và thường bị xem nhẹ nhất: ánh sáng.

## Lỗi thường gặp

**Ngộ nhận 1 — Mua camera trước, phân tích bài toán sau.** Hiện tượng: đặt mua một camera độ
phân giải cao, ống kính "đa năng" trước khi biết trạm cần Guidance hay Gauging, dung sai bao
nhiêu; đến lúc dựng job mới phát hiện độ phân giải không đủ cho dung sai đo, hoặc ống kính không
đúng working distance với không gian lắp đặt thật. Nguyên nhân: coi phần cứng vision như phụ kiện
mua sắm thông thường, bỏ qua bước phân tích GIGI + dung sai (mục 1.2). Cách tránh: luôn xuất phát
từ bài toán GIGI, dung sai yêu cầu, và ràng buộc không gian lắp đặt thật trước khi hỏi giá bất kỳ
thiết bị nào — trình tự đúng là Chương 1 → Chương 2 → Chương 3, không phải ngược lại.

**Ngộ nhận 2 — Hứa hẹn "vision thấy được mọi thứ".** Hiện tượng: quản lý dự án cam kết với khách
hàng hệ vision sẽ phát hiện mọi loại khuyết tật có thể xảy ra, kể cả những loại chưa từng gặp
trong mẫu thử; đến khi vận hành, một dạng lỗi mới xuất hiện mà hệ "bỏ lọt". Nguyên nhân: quên rằng
một hệ vision công nghiệp — dù rule-based hay deep learning — chỉ nhận diện tốt những gì nó *đã
được dạy hoặc lập luật để nhận ra*, trên một tập điều kiện ánh sáng/vị trí đã xác định trước.
Cách tránh: phát biểu rõ phạm vi phát hiện ngay từ đầu (loại khuyết tật nào, kích thước tối thiểu
nào, trong điều kiện ánh sáng nào), nghiệm thu bằng bộ mẫu đại diện đủ rộng thay vì vài tấm ảnh
đẹp (Chương 16 bàn kỹ quy trình nghiệm thu này).

**Ngộ nhận 3 — Đánh đồng machine vision với AI/computer vision nói chung.** Hiện tượng: nghĩ rằng
chỉ cần một GPU đủ mạnh và một mô hình deep learning hiện đại là giải quyết được mọi bài toán
vision công nghiệp, bỏ qua hoàn toàn ba ràng buộc ở mục 1.1 — kết quả là một mô hình "chạy tốt
trên tập test" nhưng không đáp ứng nổi cycle time thực tế, hoặc không ổn định khi ánh sáng nhà
xưởng thay đổi theo ca. Nguyên nhân: nhầm bài toán nghiên cứu (tối đa hoá độ chính xác, không
ràng buộc thời gian/môi trường) với bài toán vận hành sản xuất (ổn định, đúng hạn, lặp lại được).
Cách tránh: luôn nhìn một giải pháp vision — dù cổ điển hay học sâu — qua lăng kính ba ràng buộc
công nghiệp của Bảng 1.1 trước khi đánh giá nó "tốt" hay "chưa đủ tốt".

\newpage

# Chương 2 — Ánh sáng và quang học: 80% thành bại

Trạm bắt vị trí của MeoVision chạy ổn định suốt ca đêm — NG dưới 0.5%, đúng như lúc nghiệm thu.
Nhưng từ khoảng 8 giờ sáng, tỉ lệ NG bắt đầu leo lên 4-6%, có những đợt vọt lên gần 10%, rồi tự
nhiên hạ về bình thường lúc chiều muộn. Không ai đổi gì trong job: cùng file `.vpp`, cùng chi
tiết, cùng ca vận hành y hệt ca đêm. Kỹ sư trực ca mở CogRecordDisplay xem lại vài cycle NG —
score PMAlign tụt xuống 0.6-0.7, có lúc dưới ngưỡng chấp nhận 0.8, trong khi ảnh nhìn bằng mắt
vẫn "thấy rõ" chi tiết.

Manh mối nằm ở vị trí lắp đặt: trạm đặt cạnh dãy cửa sổ mái nhà xưởng hướng đông. Ban đêm, nguồn
sáng duy nhất là đèn ring gắn trên camera — ổn định, có thể đoán trước tuyệt đối. Ban ngày, nắng
xuyên qua mái tôn sáng (hoặc mây che bất chợt) cộng thêm vào ánh sáng đó, thay đổi từng phút,
từng đám mây trôi qua. Camera không phân biệt được đâu là ánh sáng "của trạm" và đâu là ánh sáng
"của trời" — nó chỉ ghi lại tổng cường độ ánh sáng phản xạ, và tổng đó dao động đủ để đẩy điểm
tương phản ra ngoài vùng mà pattern đã được train nhận diện tốt.

Câu chuyện này gói gọn chủ đề của cả chương: **một hệ vision không nhìn thấy vật thể, nó nhìn
thấy ánh sáng phản xạ từ vật thể.** Camera tốt nhất, ống kính đắt nhất, thuật toán PatMax tinh vi
nhất đều bất lực nếu tín hiệu ánh sáng đưa vào ống kính không ổn định và không đủ tương phản.
Chương này đi qua nguyên tắc chọn ánh sáng (mục 2.1), sáu kỹ thuật chiếu sáng phổ biến và cách
chọn giữa chúng (mục 2.2), màu sắc/filter/chống nhiễu môi trường — chính là lời giải cho tình
huống mở đầu (mục 2.3), rồi đến quang học: cách tính tiêu cự ống kính từ yêu cầu FOV (mục 2.4),
khi nào bắt buộc dùng telecentric lens (mục 2.5), và các sai số quang học cần biết để không đổ
oan cho tool phần mềm (mục 2.6). Toàn bộ chương không cần mở VisionPro lên — đây là công việc làm
*trước khi* chạm vào phần mềm, và làm sai ở đây thì không tham số tool nào cứu lại được.

## 2.1 Nguyên tắc vàng: tạo tương phản ổn định, triệt tiêu phần còn lại

Quay lại tình huống mở đầu: pattern PMAlign của trạm MeoVision được train để tìm biên dạng vỏ
nhôm dựa trên **tương phản** giữa chi tiết và nền băng tải. Đèn ring tạo ra tương phản đó — nhưng
nắng trời cũng tạo ra tương phản, theo một cách khác, chồng lên tương phản mà đèn ring đang cố
giữ ổn định. Kết quả là tương phản *tổng* mà camera nhìn thấy trôi dạt theo giờ trong ngày, còn
pattern thì chỉ được train cho một kiểu tương phản duy nhất.

Từ đó rút ra hai vế của cùng một nguyên tắc, luôn phải làm đồng thời:

1. **Tạo tương phản đủ mạnh và ổn định cho đúng đặc trưng cần thấy.** "Đặc trưng cần thấy" không
   phải là "cả chi tiết" một cách chung chung — nó là biên ngoài để định vị, là hai cạnh cần đo,
   là các module của mã DataMatrix, là vết xước cần phát hiện. Mỗi bài toán có một đặc trưng khác
   nhau, và ánh sáng phải được chọn *cho đúng đặc trưng đó*, không phải cho "ảnh nhìn rõ nói
   chung".
2. **Triệt tiêu mọi nguồn sáng và mọi chi tiết ảnh không liên quan.** Ánh sáng môi trường (cửa sổ,
   đèn trần nhà xưởng, đèn pha xe nâng đi ngang), phản xạ từ bề mặt kim loại xung quanh, bóng đổ
   của cánh tay robot — tất cả đều là "nhiễu quang học" cần bị áp đảo hoặc chắn hẳn, chứ không chỉ
   "ít ảnh hưởng hơn đèn chính".

> 📌 **Lưu ý:** "tương phản ổn định" không đồng nghĩa với "ảnh đẹp mắt". Một ảnh khiến người vận
> hành thấy dễ chịu trên màn hình (sáng đều, nhiều chi tiết, giống ảnh chụp thường ngày) có thể là
> ảnh tệ cho vision nếu độ sáng đó đến từ nhiều nguồn không kiểm soát được. Ngược lại, ảnh backlight
> chỉ toàn đen-trắng "trông xấu" với người vận hành lại thường là ảnh lý tưởng cho đo lường — vì nó
> tối giản đúng những gì tool cần và không gì khác.

Hệ quả thực dụng của nguyên tắc này: **ánh sáng và ống kính phải được quyết định trước khi bàn
đến camera hay thuật toán**, và quyết định đó dựa trên bài toán cụ thể (định vị, đo lường, đọc
mã, tìm khuyết tật) chứ không phải một công thức "đèn tốt nhất" áp dụng cho mọi trạm. Hai mục tiếp
theo triển khai lần lượt hai vế của nguyên tắc: mục 2.2 chọn *kỹ thuật chiếu sáng* (bố trí đèn thế
nào để tạo tương phản đúng đặc trưng), mục 2.3 xử lý *phần triệt tiêu* (màu sắc, filter, và chặn
ánh sáng môi trường — chính là điều trạm MeoVision ở đầu chương còn thiếu).

## 2.2 Các kỹ thuật chiếu sáng: chọn theo bề mặt và bài toán

Sáu cách bố trí đèn dưới đây không phải sáu "loại đèn" khác nhau về bản chất vật lý (phần lớn đều
dùng LED) — chúng khác nhau ở **vị trí và hướng đặt đèn so với chi tiết và camera**, và chính vị
trí đó quyết định đặc trưng nào được làm nổi bật.

### 2.2.1 Ring light (đèn vòng)

Gắn thành vòng quanh trục ống kính, chiếu gần như thẳng góc xuống bề mặt chi tiết — cách bố trí
"mặc định" quen thuộc nhất, cũng dễ bị lạm dụng nhất vì tính tiện lợi. Phù hợp với bề mặt khuếch
tán đều (nhựa mờ, kim loại phay xước mịn, nhãn giấy) khi bài toán là định vị hình dạng tổng thể
hoặc đọc mã in phẳng. Với bề mặt bóng/phản chiếu gương, ring light tạo ra vệt loé sáng (hot spot)
đúng ngay chỗ camera nhìn vào — nhược điểm cố hữu của việc đặt nguồn sáng quá gần trục quang.

### 2.2.2 Backlight (đèn nền / xuyên sáng)

Đặt phía sau chi tiết, đối diện camera — chi tiết nằm giữa đèn và ống kính, chắn sáng và trở
thành một khối đen tuyệt đối trên nền trắng rực. Đây là kỹ thuật cho **biên dạng ngoài (silhouette)**
sắc nét nhất có thể đạt được bằng ánh sáng thường: không có bóng đổ, không có phản xạ bề mặt gây
nhiễu biên, vì toàn bộ chi tiết bề mặt biến mất — chỉ còn đường viền. Đổi lại, backlight xoá luôn
mọi đặc trưng *trên* bề mặt chi tiết (không dùng được để đọc mã hay phát hiện khuyết tật bề mặt).
Trạm đo kích thước của MeoVision dùng backlight cho chính lý do này (xem Chương 9, mục 9.1 —
caliper đo hai cạnh song song trên biên dạng backlight).

### 2.2.3 Dome light (đèn bán cầu khuếch tán)

Một chụp bán cầu khuếch tán ánh sáng từ mọi hướng vào bên trong, chi tiết đặt ở tâm nhận sáng gần
như đồng đều từ 180°. Đây là lời giải cho bề mặt **cong hoặc phản chiếu mạnh** (vỏ nhôm dập bóng,
vỏ nhựa bóng, linh kiện mạ) — nơi ring light hay bất kỳ nguồn sáng định hướng nào cũng tạo loé
sáng ở đâu đó trên bề mặt cong. Dome "pha loãng" hướng sáng đến mức không còn góc phản xạ gương
nào đủ mạnh để loé.

### 2.2.4 Coaxial light (đèn đồng trục)

Dùng một gương bán mờ đặt 45° trong đường quang, đưa ánh sáng đi **đúng theo trục quang của ống
kính** trước khi tới chi tiết. Kết quả: mọi điểm trên bề mặt phẳng vuông góc với camera phản xạ
sáng thẳng trở lại ống kính với cường độ gần như nhau — cực kỳ hiệu quả cho bề mặt phẳng phản
chiếu cao (kính, chip bán dẫn, kim loại đánh bóng) khi cần phát hiện khuyết tật bề mặt phẳng đó
(vết xước, đốm bẩn phá vỡ tính đồng đều). Nhược điểm: vùng phủ sáng đồng trục bị giới hạn bởi
kích thước gương và cần khoảng công tác vừa phải — không phù hợp FOV lớn.

### 2.2.5 Dark field (trường tối / chiếu góc thấp)

Chiếu từ nhiều hướng ở góc rất thấp, gần như song song với bề mặt, thay vì vuông góc. Trên bề mặt
phẳng nhẵn, phần lớn ánh sáng góc thấp bật ra khỏi camera (không phản xạ ngược lại ống kính) —
nền vì thế tối; nhưng bất kỳ đặc trưng nào **nhô lên hoặc lõm xuống** khỏi mặt phẳng đó (vết xước,
rãnh khắc, cạnh vát) sẽ hắt một phần ánh sáng ngược lại ống kính, hiện ra sáng trên nền tối. Đây
chính là kỹ thuật bắt buộc cho các loại mã và ký tự **khắc trực tiếp lên vật liệu** (Direct Part
Marking — DPM: khắc laser, đột dập, khắc kim cương) — độ tương phản của DPM đến từ chênh lệch độ
nhám bề mặt cực nhỏ, thứ mà chiếu sáng đồng đều (ring, dome) hầu như không làm lộ ra được. Chương
11, mục 11.1.2 quay lại kỹ thuật này khi bàn về đọc mã DataMatrix khắc laser trên vỏ nhôm MeoVision.

### 2.2.6 Bar light (đèn thanh)

Một hoặc vài thanh LED thẳng, đặt một hoặc nhiều phía quanh chi tiết, góc và khoảng cách điều
chỉnh linh hoạt theo không gian lắp đặt thực tế. Không tối ưu riêng cho bài toán nào — giá trị của
bar light nằm ở tính linh hoạt cơ khí: dùng khi không đủ chỗ lắp ring/dome (băng tải hẹp, có cơ cấu
khác che khuất một phía), hoặc khi cần chiếu một phía duy nhất để cố ý tạo bóng đổ có định hướng
(một dạng dark field thô, ít kiểm soát hơn).

![Hình 2.1 — Sơ đồ bố trí sáu kỹ thuật chiếu sáng theo vị trí đèn so với chi tiết và camera](../assets/ch02/hinh_2_1.png)
**Hình 2.1 — Sơ đồ bố trí sáu kỹ thuật chiếu sáng theo vị trí đèn so với chi tiết và camera.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): lưới 2×3 sơ đồ mặt cắt ngang (vẽ draw.io), mỗi ô một kỹ thuật,
> đều có camera (hình thang nhỏ phía trên) và chi tiết (hình chữ nhật ở giữa). Ring: vòng tròn nhỏ
> quanh camera, mũi tên chiếu thẳng xuống. Backlight: dải sáng nằm dưới chi tiết, mũi tên xuyên
> qua chi tiết lên camera. Dome: nửa hình cầu bao quanh chi tiết, nhiều mũi tên toả từ mọi hướng
> bên trong chụp. Coaxial: gương chéo 45° giữa camera và chi tiết, nguồn sáng bên hông chiếu vào
> gương rồi phản xuống. Dark field: các mũi tên chiếu xiên góc thấp (~10-15° so với mặt phẳng) từ
> nhiều phía quanh chi tiết. Bar: một thanh dài nghiêng một góc từ một phía. Mỗi ô ghi tên kỹ
> thuật bằng tiếng Anh phía dưới.

**Bảng 2.1 — Chọn kỹ thuật chiếu sáng theo bề mặt và bài toán.**

| Kỹ thuật | Tạo tương phản tốt cho | Không phù hợp khi |
|---|---|---|
| Ring | Định vị hình dạng tổng thể, đọc mã in/dán trên bề mặt mờ-phẳng | Bề mặt bóng/phản chiếu gương (loé sáng ngay tâm ảnh) |
| Backlight | Đo kích thước biên ngoài, phát hiện lỗ/khuyết ở biên (silhouette) | Cần thấy đặc trưng *trên* bề mặt (chỉ còn bóng đen tuyệt đối) |
| Dome | Bề mặt cong hoặc phản chiếu mạnh (vỏ kim loại dập bóng, linh kiện mạ) | Cần bóng đổ định hướng để làm nổi texture bề mặt |
| Coaxial | Bề mặt phẳng phản chiếu cao, phát hiện khuyết tật bề mặt phẳng (kính, chip, kim loại bóng) | FOV lớn, khoảng công tác thay đổi nhiều |
| Dark field | Khuyết tật bề mặt nhỏ (xước, lõm), mã/ký tự khắc trực tiếp (DPM) — nền phẳng nhẵn hiện tối, đặc trưng nhô/lõm hiện sáng | Bề mặt nhám/texture đều khắp (cả nền tán xạ sáng, mất tương phản nền tối); cần đo kích thước biên dạng chính xác (dùng backlight) |
| Bar | Không gian lắp đặt hạn chế; chiếu một phía có chủ đích | Cần tương phản đồng đều mọi hướng |

Với trạm MeoVision: khâu định vị (PMAlign, xem Chương 8) và đọc mã DataMatrix in/khắc nông dùng
**ring light**; khâu đo kích thước (Chương 9) dùng **backlight** phối hợp telecentric lens (mục
2.5) để đạt độ chính xác sub-pixel cần thiết cho dung sai ± 0.05 mm.

> 💡 **Mẹo thực chiến:** trước khi đặt mua bất kỳ đèn công nghiệp nào, thử nghiệm bằng đèn pin cầm
> tay hoặc đèn LED để bàn rẻ tiền, di chuyển qua các góc/khoảng cách tương ứng với từng kỹ thuật ở
> trên và chụp thử bằng điện thoại hoặc webcam. Không cần chính xác tuyệt đối — mục tiêu là xác
> nhận *hướng đi* đúng (backlight có thật sự cho biên sắc hơn ring cho bài toán này không?) trước
> khi đầu tư đèn công nghiệp đúng model, đúng công suất, đúng cách gá.

### 2.2.7 Tuổi thọ, lão hoá và trôi của nguồn sáng

Sáu kỹ thuật ở trên chọn đúng *hình học* chiếu sáng cho bài toán — nhưng một trạm chạy nhiều năm
còn phải sống chung với việc chính nguồn sáng đó **thay đổi theo thời gian**, dù hình học không hề
đổi. Đây là nguyên nhân vật lý đứng sau phần lớn hiện tượng "job tự nhiên xuống cấp" mà Chương 16
(mục 16.4) sẽ bàn cách giám sát và phát hiện sớm — mục này giải thích *vì sao* nó xảy ra.

LED — nguồn sáng mặc định của gần như mọi trạm vision hiện đại — không hỏng đột ngột như bóng đèn
sợi đốt cháy đứt; nó **mờ dần**. Cường độ phát sáng suy giảm liên tục theo số giờ hoạt động và theo
nhiệt độ vận hành (LED càng nóng, suy giảm càng nhanh — một lý do quan trọng để không che kín tản
nhiệt của đèn công nghiệp khi lắp shroud, mục 2.3.4). Mức suy giảm vài phần trăm mỗi năm nghe nhỏ,
nhưng cộng dồn qua nhiều năm vận hành liên tục 24/7 đủ để đẩy độ tương phản ra khỏi vùng mà
threshold/pattern đã được chỉnh lúc lắp đặt còn hoạt động tốt — đúng cơ chế "score tụt dần, không
đơn lẻ" đã cảnh báo ở Chương 8, mục 8.5.

> 📌 **Lưu ý:** khi một trạm đã loại trừ hết các nguyên nhân "dễ thấy" — ánh sáng môi trường ổn
> định (mục 2.3.4), ống kính sạch, camera không xê dịch — mà tương phản vẫn tụt dần đều qua nhiều
> tháng, **lão hoá đèn LED** là nghi phạm hàng đầu cần kiểm tiếp theo, không phải giả định cuối
> cùng mới nghĩ tới. So công suất đo được của đèn hiện tại với thông số lúc mới lắp (nếu đèn có hỗ
> trợ đọc dòng điện/công suất qua driver) là cách xác nhận nhanh nhất.

Hệ quả thực dụng cho thiết kế trạm: (1) chọn đèn có dự phòng công suất — không thiết kế đúng khít
mức ánh sáng tối thiểu cần thiết lúc mới lắp, để còn biên độ khi đèn xuống cấp; (2) ghi lại thông
số ban đầu (công suất/dòng điện driver, ảnh tham chiếu, chỉ số histogram — Chương 12, mục 12.2) làm
mốc so sánh, đúng nguyên tắc "tài liệu hoá trước khi hiệu chỉnh lại" mà Chương 16, mục 16.4.4 sẽ
hình thức hoá đầy đủ; (3) với hệ thống có driver LED hỗ trợ điều khiển dòng điện, có thể chủ động
**tăng dòng bù** theo thời gian để giữ cường độ ổn định — kỹ thuật này đổi lại tuổi thọ đèn ngắn
hơn, một đánh đổi cần cân nhắc rõ ràng chứ không phải mặc định.

## 2.3 Màu ánh sáng, filter, polarizer; chống nhiễu ánh sáng môi trường

### 2.3.1 Màu ánh sáng: chọn theo phổ phản xạ của vật liệu

Một bề mặt hấp thụ mạnh một màu ánh sáng và phản xạ mạnh màu bổ sung của nó — nguyên tắc quen
thuộc từ bánh xe màu. Chiếu đèn đỏ lên một mảng màu đỏ trên nhãn, mảng đó phản xạ mạnh và hiện ra
**sáng**; chiếu đúng đèn đỏ đó lên mảng màu xanh lá bên cạnh, mảng xanh hấp thụ phần lớn ánh sáng
đỏ và hiện ra **tối**. Chọn đúng màu đèn theo màu của đặc trưng cần làm nổi (hoặc màu cần triệt
tiêu) biến một bài toán "phân biệt hai vùng gần giống nhau về độ sáng" thành một bài toán tương
phản rõ ràng — mà không cần thêm bất kỳ xử lý ảnh nào.

Nguyên tắc này mở rộng tự nhiên ra ngoài dải ánh sáng nhìn thấy: **UV** (tia cực tím) thường không
dùng để "nhìn rõ hơn" mà để kích hoạt **huỳnh quang** — nhiều loại keo/mực công nghiệp trộn sẵn
chất phát quang, phát sáng rực dưới đèn UV dù gần như vô hình dưới ánh sáng thường. Kiểm tra một
đường keo đã bơm đủ/đúng vị trí trở thành bài toán tương phản rất cao (vệt keo phát sáng trên nền
tối) thay vì cố phân biệt hai bề mặt cùng màu dưới ánh sáng trắng. **IR** (hồng ngoại) theo chiều
ngược lại — nhiều vật liệu mờ đục với mắt người (một số loại nhựa, mực in) lại trong suốt hoặc bán
trong suốt với bước sóng hồng ngoại, hữu ích khi cần "nhìn xuyên" một lớp phủ để kiểm tra thứ nằm
bên dưới.

### 2.3.2 Filter quang học (vật lý) — khác hẳn xử lý ảnh phần mềm

Cần phân biệt rõ hai thứ dễ nhầm vì cùng nói về "màu":

- **Filter quang học (vật lý)** — một tấm kính hoặc màng mỏng gắn **trước ống kính**, chỉ cho một
  dải bước sóng ánh sáng nhất định đi qua (band-pass filter) trước khi ánh sáng chạm tới cảm
  biến. Ánh sáng ngoài dải đó bị chặn vật lý — không bao giờ tới được điểm ảnh, dù cường độ mạnh
  đến đâu.
- **Xử lý màu bằng phần mềm** (ví dụ tách kênh màu, chuyển RGB sang xám có trọng số) — xử lý dữ
  liệu **sau khi** cảm biến đã ghi nhận toàn bộ ánh sáng tới, kể cả phần ánh sáng "nhiễu". Phần mềm
  không thể phục hồi thông tin đã bị bão hoà hoặc lẫn nhiễu ngay từ khâu thu nhận. (VisionPro có
  công cụ cho việc này ở tầng phần mềm — `CogImageConvertTool` — xem Chương 12, mục 12.1.1; đó là
  một công cụ hoàn toàn khác về bản chất so với filter vật lý đang bàn ở đây.)

Hệ quả thực dụng: filter vật lý **loại bỏ nhiễu tại nguồn**, phần mềm chỉ **diễn giải lại** những
gì đã bị nhiễu làm bẩn. Với tình huống mở đầu chương — nắng trời (quang phổ trắng, trải rộng mọi
bước sóng) cộng vào đèn ring — một cặp "đèn LED đỏ + filter band-pass đỏ gắn trước lens" chặn được
phần lớn phổ ánh sáng mặt trời không trùng bước sóng đỏ, trong khi vẫn cho ánh sáng của chính đèn
ring đi qua gần như trọn vẹn. Đây là một trong ba tuyến phòng thủ chống nhiễu môi trường sẽ tổng
kết ở mục 2.3.4.

### 2.3.3 Polarizer — khử loé sáng bằng phân cực

Ánh sáng phản xạ **khuếch tán** (từ bề mặt nhám) giữ nguyên trạng thái phân cực hỗn tạp ban đầu;
ánh sáng phản xạ **gương** (từ bề mặt bóng, kim loại, nhựa trong) lại bị phân cực theo một hướng
khá đồng nhất. Đặt một tấm polarizer trước nguồn sáng và một tấm polarizer thứ hai trước ống kính,
xoay lệch 90° so với tấm đầu (bố trí "crossed polarizer"), phần lớn ánh sáng phản xạ gương — vốn
giữ hướng phân cực của nguồn — bị tấm thứ hai chặn lại; ánh sáng khuếch tán (đã mất tính phân cực
định hướng) vẫn lọt qua được một phần đáng kể. Kết quả là các vệt loé sáng trên bề mặt bóng giảm
mạnh, đổi lại ảnh tối đi tổng thể (cần bù bằng công suất đèn hoặc exposure — mục 2.4.4).

### 2.3.4 Ba tuyến phòng thủ chống nhiễu ánh sáng môi trường

Quay lại trọn vẹn tình huống mở đầu chương: đây chính xác là bài toán "ánh sáng môi trường không
kiểm soát được cộng vào ánh sáng chủ động của trạm", và có ba cách xử lý, thường phối hợp cả ba:

1. **Shroud (chụp che cơ khí)** — một khung/hộp che kín vùng nhìn của camera và chi tiết khỏi ánh
   sáng bên ngoài, chỉ để đèn của trạm chiếu sáng bên trong. Giải pháp triệt để nhất vì loại nhiễu
   *trước khi* nó tới gần cảm biến, nhưng đòi hỏi không gian cơ khí và có thể cản trở thao tác bảo
   trì/quan sát bằng mắt.
2. **Filter quang học phù hợp màu đèn** (mục 2.3.2) — khi không thể che kín hoàn toàn (băng tải hở,
   cần người thao tác tiếp cận), lọc bớt phần phổ ánh sáng môi trường không trùng màu đèn chủ động.
3. **Strobe overdrive (chớp sáng vượt công suất định mức)** — thay vì để đèn LED sáng liên tục,
   đèn được cấp một xung dòng điện **cao hơn nhiều lần** dòng định mức liên tục, nhưng chỉ trong
   một khoảng thời gian cực ngắn (cỡ chục đến vài trăm micro giây), đồng bộ chính xác với thời
   điểm trigger và cửa sổ exposure của camera. LED chịu được dòng vượt định mức miễn thời gian đủ
   ngắn (tỉ lệ chiếm dụng — **duty cycle**, tức tỉ lệ thời gian đèn thực sự sáng trên tổng chu kỳ
   trigger, ví dụ exposure 100 µs lặp mỗi 20 ms cho duty cycle 0.5% — cực thấp), và trong khoảnh khắc đó, cường độ đèn có thể
   cao gấp nhiều lần so với chạy liên tục. Kết hợp với việc rút ngắn thời gian exposure xuống bằng
   đúng độ dài xung strobe, ánh sáng môi trường (vốn chiếu liên tục, không đồng bộ với xung strobe)
   chỉ đóng góp một phần năng lượng cực nhỏ vào ảnh — vì nó chỉ có đúng khoảng thời gian exposure
   ngắn ngủi đó để "góp phần", trong khi đèn strobe dồn toàn bộ năng lượng của nó vào chính khoảng
   thời gian đó. Tỉ lệ tín hiệu-trên-nhiễu vì vậy tăng vọt mà không cần che chắn cơ khí. Đánh đổi
   cần biết trước: dòng vượt định mức lặp lại hàng triệu lần trong vòng đời trạm vẫn góp phần vào
   tốc độ lão hoá LED (mục 2.2.7) — tra thông số nhà sản xuất về tuổi thọ ở chế độ strobe/overdrive
   trước khi chốt thiết kế, đừng mặc định nó "miễn phí" chỉ vì mỗi xung rất ngắn.

> ⚠️ **Cảnh báo:** đừng bao giờ coi ánh sáng phòng/nhà xưởng là "đủ dùng" chỉ vì lúc nghiệm thu ảnh
> nhìn "rõ ràng". Ánh sáng môi trường thay đổi theo giờ trong ngày (nắng, đèn huỳnh quang nhấp
> nháy theo tần số lưới điện, đèn xe nâng đi ngang), theo mùa (góc nắng khác nhau), và theo cả việc
> ai đó bật/tắt một dãy đèn trần khác trong xưởng. Một trạm production 24/7 phải triệt tiêu được
> biến số này bằng ít nhất một trong ba tuyến phòng thủ trên — không phải "hy vọng nó không đổi".

![Hình 2.2 — Cùng một cạnh chi tiết dưới ring light thường và dưới backlight](../assets/ch02/hinh_2_2.png)
**Hình 2.2 — Cùng một cạnh chi tiết dưới ring light thường (trái) và dưới backlight (phải).**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): hai ảnh chụp cùng một góc vỏ nhôm MeoVision, đặt cạnh nhau, cùng
> camera/cùng góc chụp. Trái: chiếu ring — bề mặt kim loại loang lổ phản xạ không đều, biên ngoài
> mờ, khó phân biệt với bóng đổ. Phải: chiếu backlight — chi tiết là khối đen tuyệt đối trên nền
> trắng, biên ngoài sắc như một đường kẻ. Overlay đường màu vàng phóng to một đoạn biên ở cả hai
> ảnh để thấy rõ độ "răng cưa"/nhiễu ở ảnh trái so với đường thẳng mượt ở ảnh phải. Chụp thật trên
> mẫu chi tiết bất kỳ có cạnh thẳng, hai kiểu chiếu sáng, cùng thiết lập camera.

## 2.4 Ống kính: tiêu cự, FOV, working distance, depth of field, khẩu độ

### 2.4.1 Bốn đại lượng luôn phải tính cùng nhau

Chọn ống kính không phải tra bảng "lens nào tốt" — nó là giải một bài toán hình học đơn giản với
bốn đại lượng ràng buộc lẫn nhau:

- **FOV** (Field of View — trường nhìn): vùng không gian thực mà camera phải nhìn thấy trọn vẹn.
  Với MeoVision, FOV đã chốt là 100.0 × 83.7 mm (đủ phủ vỏ nhôm 60 × 40 mm cộng biên độ xê dịch
  ± 5 mm của pallet, còn dư lề an toàn).
- **Kích thước cảm biến**: xác định bởi số pixel và kích thước từng pixel — không phải thông số ta
  tự chọn cho lens, mà là ràng buộc đến từ camera đã chọn (Chương 3 bàn cách chọn camera; ở đây ta
  dùng số đã chốt của MeoVision làm dữ liệu đầu vào).
- **Working distance (WD)** — khoảng cách từ mặt trước ống kính đến mặt phẳng chi tiết. Bị ràng
  buộc bởi không gian cơ khí thực tế: cần chừa chỗ cho đèn (đặc biệt dome/coaxial), tránh cánh tay
  robot/băng tải va chạm, và với nhiều lens, WD càng ngắn thì ống kính càng "góc rộng" và distortion
  càng lớn (mục 2.6).
- **Tiêu cự (focal length, f)** — thông số vật lý của chính ống kính, quyết định mối quan hệ giữa
  ba đại lượng trên.

Bốn đại lượng này không độc lập: cố định ba cái, cái thứ tư bị xác định theo. Công thức xấp xỉ
thấu kính mỏng — đủ chính xác cho việc *chọn lens ban đầu*, trước khi tra catalogue nhà sản xuất
để chốt model — như sau: `f ≈ (WD × kích thước cảm biến) / FOV`. Công thức này đến từ tam giác
đồng dạng giữa cảm biến và vùng nhìn thấy trên mặt phẳng vật, với giả thiết WD lớn hơn đáng kể so
với f (đúng với hầu hết trạm công nghiệp, WD hàng trăm mm còn f chỉ vài chục mm).

### 2.4.2 Tính cụ thể cho trạm MeoVision

Cảm biến của MeoVision: 2448 × 2048 px, pixel 3.45 µm. Kích thước vật lý cảm biến:

- Chiều ngang: 2448 × 3.45 µm = 8 445.6 µm ≈ **8.45 mm**
- Chiều dọc: 2048 × 3.45 µm = 7 065.6 µm ≈ **7.07 mm**

FOV yêu cầu: 100.0 × 83.7 mm. Tỉ lệ phóng đại (magnification) của hệ quang: m = kích thước cảm
biến / FOV ≈ 8.45 / 100.0 ≈ **0.0845** (kiểm theo chiều dọc: 7.07 / 83.7 ≈ 0.0845 — khớp, đúng như
kỳ vọng vì FOV và cảm biến có cùng tỉ lệ khung hình).

Giả sử bố trí cơ khí trạm cho phép working distance khoảng **300 mm** (đủ chỗ gắn ring light và
tránh va chạm với cụm gắp của robot). Áp công thức:

f ≈ WD × m ≈ 300 mm × 0.0845 ≈ **25.3 mm**

Đây rơi đúng vào một tiêu cự ống kính công nghiệp tiêu chuẩn: **25 mm**. Tính ngược lại working
distance chính xác cần để đạt đúng FOV 100.0 mm với lens 25 mm chuẩn:

WD = f / m = 25 mm / 0.0845 ≈ **296 mm**

Sai lệch 4 mm so với ước lượng ban đầu (300 mm) là bình thường và chấp nhận được ở bước chọn lens —
công thức xấp xỉ dùng để **chọn đúng tiêu cự chuẩn có bán trên thị trường**, rồi tinh chỉnh khoảng
cách gá thực tế (vài mm) khi lắp đặt để đạt đúng FOV mong muốn, thay vì đi tìm một tiêu cự "lẻ"
không tồn tại.

> 💡 **Mẹo thực chiến:** luôn tính FOV cần thiết **lớn hơn** kích thước chi tiết lớn nhất cộng biên
> độ xê dịch thực tế — không tính khít bằng đúng kích thước chi tiết. MeoVision chốt FOV 100.0 mm
> cho chi tiết dài 60 mm xê dịch ± 5 mm (tức vùng chi tiết có thể xuất hiện rộng tới 70 mm) —
> chừa lề gần 15 mm mỗi bên để chi tiết không bao giờ chạm mép ảnh dù ở vị trí lệch xa nhất. FOV
> tính khít sẽ khiến chi tiết trôi ra ngoài khung hình đúng những lần lệch nhiều nhất — thường
> cũng là những lần cần đo/định vị chính xác nhất.

> 🔍 **Đào sâu thêm:** ví dụ trên "gọn" vì FOV và cảm biến MeoVision tình cờ có cùng tỉ lệ khung
> hình (cả hai ≈ 1.196), nên tỉ lệ phóng đại tính theo chiều ngang và chiều dọc trùng khớp. Khi
> chi tiết/FOV thực tế không cùng tỉ lệ khung hình với cảm biến đã chọn — trường hợp phổ biến hơn
> ví dụ này — tính riêng tỉ lệ phóng đại cho cả hai chiều rồi **lấy giá trị nhỏ hơn** (FOV rộng
> hơn yêu cầu ở chiều còn lại): đảm bảo FOV thực tế phủ đủ cả hai chiều, chấp nhận dư một phần ảnh
> ở chiều không giới hạn thay vì cắt mất chi tiết ở chiều kia.

### 2.4.3 Depth of field (DOF) — vùng nét theo chiều sâu

Ống kính chỉ hội tụ hoàn hảo tại **một** mặt phẳng khoảng cách; càng xa mặt phẳng đó (gần camera
hơn hoặc xa camera hơn), ảnh càng mờ dần. **Depth of field** là bề dày vùng không gian quanh mặt
phẳng lấy nét mà độ mờ đó vẫn được coi là "chấp nhận được" — quan trọng với MeoVision vì chi tiết
cao 8 mm: nếu lấy nét đúng tại mặt pallet, đỉnh chi tiết cách mặt lấy nét 8 mm phải vẫn còn đủ nét
để PMAlign/caliper hoạt động tốt.

Công thức xấp xỉ tổng bề dày DOF (hợp lệ khi WD lớn hơn đáng kể so với f, như phần lớn ống kính
công nghiệp):

`DOF ≈ 2 × N × c × (WD / f)²`

Một điểm sáng lý tưởng nằm ngoài đúng mặt phẳng lấy nét không hội tụ lại thành một điểm trên cảm
biến — nó trải thành một vệt mờ nhỏ hình tròn, đường kính vệt đó lớn dần khi càng lệch xa mặt
phẳng lấy nét. **Circle of confusion (c)** là đường kính vệt mờ tối đa còn được coi là "đủ nét
để chấp nhận" — một khi vệt mờ nhỏ hơn kích thước một pixel, cảm biến không còn cách nào phân
biệt nó với một điểm nét thật sự, nên quy ước phổ biến lấy c bằng khoảng 2 lần kích thước pixel.
Đây là một **tham số thiết kế do ta chọn**, không phải hằng số vật lý cố định — bài toán đo lường
chính xác cao có thể chọn c chặt hơn (gần 1 lần pixel) để đòi hỏi ảnh nét hơn, đổi lại DOF tính
được sẽ hẹp hơn. Với pixel 3.45 µm của MeoVision: c ≈ 2 × 3.45 µm = 6.9 µm = 0.0069 mm.

Với lens 25 mm, WD 296 mm, chọn khẩu độ N = 8 (khẩu độ tương đối nhỏ, phổ biến cho trạm công
nghiệp — mục 2.4.4 giải thích vì sao):

DOF ≈ 2 × 8 × 0.0069 mm × (296/25)² ≈ 2 × 8 × 0.0069 × 140.2 ≈ **15.5 mm**

Tức vùng nét chấp nhận được trải rộng khoảng ± 7.7 mm quanh mặt phẳng lấy nét — đủ bao trọn chiều
cao 8 mm của chi tiết MeoVision nếu lấy nét tại giữa khoảng đó (khoảng 4 mm phía trên mặt pallet),
cộng thêm biên độ dư cho sai số lắp đặt/độ phẳng của pallet.

### 2.4.4 Khẩu độ — đánh đổi giữa DOF và lượng ánh sáng

Khẩu độ (f-number, N — tỉ số giữa tiêu cự và đường kính lỗ mở của ống kính) là tham số duy nhất
trong bốn đại lượng ở mục 2.4.1 chỉnh được ngay trên ống kính mà không đổi cả hệ quang. Khép khẩu
độ (N lớn — lỗ mở nhỏ) tăng DOF, như công thức trên cho thấy tỉ lệ thuận trực tiếp, nhưng đồng thời
giảm lượng ánh sáng đi qua ống kính tới cảm biến — công thức DOF và công thức lượng ánh sáng luôn
kéo ngược chiều nhau.

**Bảng 2.2 — Đánh đổi khẩu độ với cùng lens 25 mm, WD 296 mm (tính theo công thức mục 2.4.3).**

| Khẩu độ (N) | DOF ước tính | Lượng sáng cần | Phù hợp khi |
|---|---|---|---|
| f/2.8 | ≈ 5.4 mm | Thấp (ống kính mở rộng) | Ánh sáng đèn yếu, chi tiết phẳng tuyệt đối, không cần dư DOF |
| f/5.6 | ≈ 10.8 mm | Trung bình | Cân bằng phổ biến khi đèn công suất vừa phải |
| f/8 | ≈ 15.5 mm | Cần đèn công suất khá | Chi tiết có biến thiên độ cao (như MeoVision, 8 mm) |
| f/16 | ≈ 31.0 mm | Cần đèn công suất cao hoặc exposure dài hơn | Chi tiết cao/biến thiên lớn, chấp nhận cần nhiều ánh sáng hơn |

Đây là lý do đèn LED công nghiệp thường có công suất lớn hơn nhiều so với đèn chiếu sáng thông
thường cùng kích thước: khẩu độ khép để lấy đủ DOF ăn bớt phần lớn ánh sáng, và phần thiếu hụt đó
phải được bù lại bằng cường độ nguồn sáng chủ động — không phải bằng cách mở khẩu độ rộng hơn để
"dễ chụp" (đánh đổi ngược lại: mất DOF).

> 🔍 **Đào sâu thêm:** khép khẩu độ không phải "càng nhỏ càng tốt" vô hạn — khi lỗ mở quá nhỏ (N
> rất lớn, ví dụ f/22 trở lên với phần lớn lens công nghiệp cỡ này), hiệu ứng **nhiễu xạ**
> (diffraction) bắt đầu làm giảm độ nét tổng thể của ảnh, bất kể DOF tính theo công thức hình học
> có tăng lên bao nhiêu. Với ứng dụng đo lường chính xác cao, việc chọn N cần cân bằng cả DOF lẫn
> giới hạn nhiễu xạ — thông tin nằm trong thông số MTF (Modulation Transfer Function) của lens, một
> chủ đề nằm ngoài phạm vi CORE của sách này.

![Hình 2.3 — Sơ đồ FOV, working distance và tiêu cự trên hệ quang trạm MeoVision](../assets/ch02/hinh_2_3.png)
**Hình 2.3 — Sơ đồ FOV, working distance và tiêu cự trên hệ quang trạm MeoVision.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ mặt cắt bên (draw.io), từ trái sang phải: mặt phẳng chi
> tiết (đường thẳng đứng, ghi chú "FOV 100.0 × 83.7 mm" với mũi tên hai đầu đo chiều ngang) — sau
> đó một đoạn ghi chú "Working Distance ≈ 296 mm" (mũi tên hai đầu nằm ngang) — tới ống kính (hình
> trụ nhỏ, ghi "f = 25 mm") — tới cảm biến (hình chữ nhật nhỏ, ghi "8.45 × 7.07 mm, 2448×2048 px").
> Vẽ thêm hai tia sáng từ hai mép FOV hội tụ qua tâm lens tới hai mép cảm biến, minh hoạ tam giác
> đồng dạng dùng trong công thức mục 2.4.1. Phía dưới, dải mờ đánh dấu vùng DOF (± 7.7 mm quanh
> mặt phẳng lấy nét) chồng lên vị trí chi tiết cao 8 mm.

## 2.5 Telecentric lens — khi nào bắt buộc

### 2.5.1 Vấn đề: lens thường phóng đại khác nhau theo khoảng cách

Ống kính thường (entocentric) tạo ảnh theo phép chiếu phối cảnh: một vật ở gần ống kính hơn trông
**to hơn** cùng một vật y hệt đặt xa hơn — đúng trực giác nhiếp ảnh thông thường. Hệ quả với đo
lường: nếu calibration được dạy tại một mặt phẳng (ví dụ mặt pallet, xem Chương 7, mục 7.3) nhưng
đặc trưng cần đo nằm ở một độ cao khác (ví dụ đỉnh chi tiết cao 8 mm, gần ống kính hơn mặt pallet
một khoảng đúng bằng chiều cao đó), phép quy đổi pixel → mm đã dạy tại mặt pallet không còn đúng
tại độ cao của đỉnh chi tiết — sai số này gọi là **sai số phối cảnh** (perspective error), tỉ lệ
gần đúng với chênh lệch độ cao chia cho working distance.

Ước lượng cụ thể cho MeoVision: chênh lệch độ cao Δh = 8 mm (chiều cao vỏ nhôm), WD ≈ 296 mm:

Sai số phối cảnh tương đối ≈ Δh / WD ≈ 8 / 296 ≈ **2.7%**

Áp tỉ lệ này lên một kích thước đo 40 mm (chiều rộng vỏ nhôm): sai số tuyệt đối ≈ 40 mm × 2.7% ≈
**1.08 mm** — gấp hơn 20 lần dung sai cho phép ± 0.05 mm của trạm đo MeoVision. Với một trạm chỉ
cần bắt vị trí cho robot (dung sai ± 0.1 mm nhưng đo trên toạ độ tâm chi tiết, không đo kích thước
tuyệt đối trên toàn FOV), sai số cùng bậc này có thể vẫn nằm trong ngân sách chấp nhận được — đây
chính là lý do trạm bắt vị trí của MeoVision dùng lens thường (mục 2.4), còn trạm đo kích thước
bắt buộc phải xử lý vấn đề này bằng lens khác hẳn.

> 📌 **Lưu ý:** khi dạy calibration bằng một tấm chuẩn vật lý (checkerboard, xem Chương 7, mục
> 7.3), mặt phẳng camera thực sự "nhìn thấy" là bề mặt in của tấm chuẩn — cao hơn mặt bàn/pallet
> đặt nó đúng bằng độ dày tấm chuẩn. Với tấm mỏng (dưới 1 mm) sai lệch này thường bỏ qua được,
> nhưng với đế dày (ví dụ đế nhôm vài milimet) cần cộng bề dày đó vào Δh khi ước lượng sai số phối
> cảnh, hoặc đặt tấm chuẩn đúng lên mặt phẳng thật sự cần calibration thay vì suy luận qua độ dày.

### 2.5.2 Telecentric lens giải quyết vấn đề bằng cách nào

**Telecentric lens** được thiết kế sao cho các tia sáng chính đi tới cảm biến gần như **song song
với trục quang**, thay vì hội tụ theo phối cảnh như lens thường. Hệ quả trực tiếp: độ phóng đại
gần như **không đổi** khi khoảng cách vật thay đổi trong một dải nhất định quanh working distance
thiết kế — một vật dịch chuyển gần/xa ống kính vài milimet (như chênh lệch độ cao 8 mm ở trên)
vẫn cho kích thước ảnh gần như y hệt. Sai số phối cảnh vừa tính ở mục 2.5.1 gần như triệt tiêu.

Đổi lại, telecentric lens trả giá bằng ba ràng buộc vật lý đáng kể:

- **Đường kính thấu kính đầu tiên phải lớn hơn hoặc bằng kích thước FOV** — để thu được toàn bộ
  chùm tia song song trong suốt vùng FOV, khác hẳn lens thường (đường kính nhỏ hơn FOV rất nhiều
  nhờ phối cảnh hội tụ). FOV càng lớn, lens telecentric càng to, càng nặng, càng đắt — chi phí
  tăng nhanh hơn nhiều so với lens thường cùng FOV.
- **Working distance gần như cố định**, không điều chỉnh linh hoạt như lens thường (khoảng dịch
  chuyển cho phép rất hẹp, nằm trong đúng phạm vi DOF thiết kế).
- **FOV bị giới hạn theo kích thước lens có sẵn trên thị trường** — không phải cứ cần FOV bao
  nhiêu là có lens telecentric đúng kích cỡ đó với giá hợp lý.

> ⚠️ **Cảnh báo:** đừng chọn telecentric lens theo phản xạ "đo lường thì luôn cần telecentric".
> Với FOV lớn hoặc working distance dài, giá và kích thước lens telecentric có thể vượt xa ngân
> sách/không gian cơ khí cho phép. Luôn tính trước sai số phối cảnh thực tế theo cách ở mục 2.5.1 —
> nếu chênh lệch độ cao đặc trưng cần đo so với mặt phẳng calibration đủ nhỏ (chi tiết mỏng, hoặc
> có thể dạy calibration đúng tại mặt phẳng đo), lens thường kết hợp `CogCalibCheckerboardTool`
> (khử distortion, xem Chương 7, mục 7.3) đôi khi đã đủ — telecentric là công cụ mạnh nhưng không
> miễn phí.

Với trạm đo kích thước của MeoVision — dung sai ± 0.05 mm, chi tiết cao 8 mm, đo trên toàn FOV —
sai số 1.08 mm tính ở trên loại bỏ hoàn toàn khả năng dùng lens thường: **telecentric là bắt buộc**,
phối hợp cùng backlight (mục 2.2.2, vì telecentric phát huy tối đa hiệu quả với ánh sáng song song
song hành, mà backlight tự nhiên tạo ra). Đây chính là cấu hình đã được nêu trước ở Chương 7, mục
7.2.3 và sẽ triển khai chi tiết ở Chương 9.

> 📌 **Lưu ý:** telecentric lens triệt tiêu gần hết sai số phối cảnh, nhưng **không** đồng nghĩa
> "miễn nhiễm mọi sai số quang học" — distortion (mục 2.6) vẫn tồn tại ở mức nhỏ trên hầu hết lens
> telecentric thực tế, và calibration (Chương 7) vẫn cần thiết để đạt độ chính xác cao nhất. DOF
> của telecentric lens cũng vẫn hữu hạn (tính theo cùng công thức mục 2.4.3, chỉ khác N/f đặc thù
> của loại lens này) — không phải "nét vô hạn theo chiều sâu" như đôi khi bị hiểu nhầm.

## 2.6 [NÂNG CAO] Sai số quang học: distortion, vignetting, perspective

> 🔍 **Đào sâu thêm:** ba loại sai số dưới đây tồn tại ở mức độ khác nhau trên mọi ống kính thực tế
> (kể cả lens tốt), và là lý do vì sao "quy đổi pixel sang mm bằng một hệ số duy nhất" luôn là gần
> đúng, không phải chính xác tuyệt đối (Chương 7, mục 7.1.1 đã bàn hệ quả này ở góc độ hệ toạ độ).
>
> - **Distortion** — méo hình học phi tuyến, khiến các đường thẳng trong thực tế cong nhẹ trên
>   ảnh (dạng "gối lồi" barrel hoặc "gối lõm" pincushion). Lens góc rộng/tiêu cự ngắn thường có
>   distortion lớn hơn lens tiêu cự dài. Khắc phục bằng mô hình hiệu chuẩn phi tuyến —
>   `CogCalibCheckerboardTool`, Chương 7, mục 7.3.
> - **Vignetting** — giảm độ sáng dần từ tâm ra mép ảnh, do các tia sáng ở góc rộng bị hạn chế một
>   phần bởi cơ cấu cơ khí bên trong ống kính. Ảnh hưởng trực tiếp đến các tool dựa vào ngưỡng độ
>   sáng cố định (threshold, Chương 4) nếu **ROI** (Region of Interest — vùng ảnh mà một tool được
>   phép xử lý, sẽ dùng lại xuyên suốt các chương công cụ VisionPro từ Chương 7) trải rộng gần hết
>   FOV — đặc trưng giống hệt nhau ở tâm và ở mép ảnh có thể cho giá trị độ sáng đo được khác nhau.
> - **Perspective (sai số phối cảnh)** — như đã phân tích chi tiết ở mục 2.5.1: độ phóng đại thay
>   đổi theo khoảng cách vật, gây sai số hệ thống khi đối tượng có bề dày hoặc calibration/mặt đo
>   không trùng mặt phẳng.
>
> Cả ba đều được xử lý ở tầng phần mềm calibration (Chương 7) khi lens thường được dùng — nhưng
> hiểu rõ nguồn gốc vật lý của chúng giúp phân biệt "lỗi do tham số tool sai" với "giới hạn vật lý
> của chính ống kính đang dùng", một sự nhầm lẫn khiến không ít kỹ sư mới mất hàng giờ chỉnh tham
> số tool cho một vấn đề mà chỉnh tham số không bao giờ giải quyết được.
>
> Một trường hợp đặc biệt đáng biết tên: khi camera phải chụp một **bề mặt nghiêng** (không song
> song với mặt phẳng cảm biến) — ví dụ kiểm mối hàn ở một góc nghiêng cố định — mặt phẳng lấy nét
> thông thường chỉ cắt bề mặt nghiêng đó tại một đường, phần còn lại rơi ra ngoài DOF (mục 2.4.3)
> dù khép khẩu độ đến đâu. **Scheimpflug adapter** (đặt tên theo nguyên lý Scheimpflug) giải quyết
> đúng vấn đề này bằng cách nghiêng mặt phẳng cảm biến so với trục ống kính một góc tính toán trước,
> sao cho mặt phẳng lấy nét xoay theo đúng độ nghiêng của bề mặt cần chụp — toàn bộ bề mặt nghiêng
> nét đều dù DOF hình học không hề tăng. Đây là công cụ chuyên dụng cho đúng một tình huống hẹp
> (chụp góc nghiêng cố định), không phải giải pháp chung cho vấn đề DOF.

## Tổng kết chương

- Hệ vision không nhìn thấy vật thể, nó nhìn thấy ánh sáng phản xạ — nguyên tắc vàng là tạo tương
  phản ổn định cho đúng đặc trưng cần thấy, đồng thời triệt tiêu mọi nguồn sáng/chi tiết không
  liên quan (mục 2.1).
- Sáu kỹ thuật chiếu sáng (ring, backlight, dome, coaxial, dark field, bar) khác nhau ở vị trí đặt
  đèn so với chi tiết và camera, mỗi kỹ thuật tối ưu cho một tổ hợp bề mặt/bài toán riêng — chọn
  sai kỹ thuật thì không tham số camera hay tool nào bù lại được (mục 2.2, Bảng 2.1).
- Màu ánh sáng, filter quang học (vật lý, khác hẳn xử lý màu bằng phần mềm), polarizer, và ba
  tuyến chống nhiễu ánh sáng môi trường (shroud, filter, strobe overdrive) là cách xử lý phần
  "triệt tiêu" của nguyên tắc vàng — và là lời giải trực tiếp cho tình huống mở đầu chương (mục 2.3).
- Bốn đại lượng FOV, kích thước cảm biến, working distance, tiêu cự ràng buộc lẫn nhau qua công
  thức `f ≈ (WD × kích thước cảm biến) / FOV`; MeoVision (FOV 100.0×83.7 mm, cảm biến 8.45×7.07 mm)
  ra lens 25 mm ở working distance ≈ 296 mm. Depth of field và khẩu độ đánh đổi ngược chiều nhau —
  khép khẩu độ để tăng DOF luôn phải trả giá bằng lượng ánh sáng cần nhiều hơn (mục 2.4).
- Lens thường gây sai số phối cảnh tỉ lệ với chênh lệch độ cao chia cho working distance — với
  MeoVision (chi tiết cao 8 mm, WD 296 mm) sai số này (~1 mm trên kích thước 40 mm) vượt xa dung
  sai đo ± 0.05 mm, buộc trạm đo phải dùng telecentric lens; đổi lại telecentric có FOV/working
  distance bị ràng buộc chặt và chi phí cao hơn đáng kể (mục 2.5).
- Distortion, vignetting và sai số phối cảnh là giới hạn vật lý cố hữu của mọi ống kính thực tế,
  xử lý được ở tầng calibration phần mềm (Chương 7) nhưng không phải lỗi tham số tool — phân biệt
  đúng hai loại nguyên nhân này tiết kiệm rất nhiều thời gian debug (mục 2.6).

## Lỗi thường gặp

**Lỗi 1 — Chọn đèn sau cùng thay vì đầu tiên.** Hiện tượng: job dựng xong trên QuickBuild với ánh
sáng phòng/đèn tạm, chạy ổn khi setup nhưng liên tục phải "vá" bằng tham số threshold/contrast
mỗi khi đưa lên máy thật. Nguyên nhân: ánh sáng và ống kính là nền tảng vật lý quyết định tín hiệu
đầu vào — không tham số phần mềm nào bù được một tín hiệu vốn đã kém ngay từ khâu thu nhận. Cách
tránh: chốt kỹ thuật chiếu sáng (mục 2.2) và xử lý nhiễu môi trường (mục 2.3) trước khi mở
QuickBuild dựng job đầu tiên.

**Lỗi 2 — FOV tính khít, không chừa lề cho dao động vị trí thực tế.** Hiện tượng: job chạy đúng
khi chi tiết ở giữa khung hình lúc test, báo lỗi "không tìm thấy" hoặc đo sai khi chi tiết lệch về
phía biên độ xê dịch cho phép trong sản xuất thực tế. Nguyên nhân: FOV chốt bằng đúng kích thước
chi tiết, không cộng thêm biên độ xê dịch/dung sai gá đặt. Cách tránh: luôn tính FOV = kích thước
chi tiết lớn nhất + toàn bộ biên độ xê dịch dự kiến, cộng thêm lề an toàn (mục 2.4.2).

**Lỗi 3 — Coi ánh sáng môi trường là hằng số vì "lúc nghiệm thu ổn".** Hiện tượng: đúng tình huống
mở đầu chương — trạm chạy tốt khi nghiệm thu (thường vào một thời điểm cố định trong ngày), NG
hàng loạt vào ca/giờ khác. Nguyên nhân: không triển khai bất kỳ tuyến phòng thủ nào chống nhiễu
ánh sáng môi trường (mục 2.3.4). Cách tránh: nghiệm thu tối thiểu qua đủ các khung giờ vận hành
thực tế (sáng/trưa/chiều/tối nếu chạy nhiều ca), và mặc định đưa ít nhất một trong ba tuyến phòng
thủ (shroud/filter/strobe) vào thiết kế cho mọi trạm gần nguồn sáng biến động.

**Lỗi 4 — Dùng lens thường cho bài toán đo chính xác trên chi tiết có bề dày.** Hiện tượng: kết
quả đo lệch có hệ thống (không phải nhiễu ngẫu nhiên) — luôn lớn hơn hoặc luôn nhỏ hơn giá trị
thật một lượng gần như cố định. Nguyên nhân: sai số phối cảnh giữa mặt phẳng calibration và mặt
phẳng đặc trưng cần đo (mục 2.5.1), lens thường không triệt tiêu được sai số này. Cách tránh: tính
trước sai số phối cảnh dự kiến theo Δh/WD; nếu vượt đáng kể so với dung sai, chuyển sang telecentric
lens hoặc dạy lại calibration đúng tại mặt phẳng đo (Chương 7, mục 7.3.1).

\newpage

# Chương 3 — Camera và thu nhận ảnh số

Trạm nhận diện & bắt vị trí của MeoVision vừa lắp xong phần cơ khí: pallet trên băng tải index
dừng đúng vị trí, cảm biến báo "part-in-position", PLC gửi trigger cho camera chụp. Ảnh đầu tiên
nhìn ổn — cho đến khi kỹ sư cơ khí, muốn rút ngắn thời gian dừng chờ ổn định của pallet, giảm độ
trễ giữa lúc dừng cơ khí và lúc bắn trigger xuống còn vài chục mili giây. Vật thể lúc đó vẫn còn
rung nhẹ dư chấn sau cú dừng đột ngột. Từ ảnh chụp lần này, biên chi tiết không còn là đường thẳng
sắc nét — nó bị "xé" thành hai nửa lệch nhau vài pixel, như thể ảnh bị cắt ngang rồi ghép lệch.
PMAlign vẫn tìm được vị trí (score tụt nhẹ), nhưng caliper đo chiều rộng chi tiết cho ra con số
dao động ± 0.15 mm giữa các lần chụp cùng một chi tiết đứng yên — gấp ba dung sai cho phép
± 0.05 mm.

Không ai đổi ống kính, không ai đổi ánh sáng, không ai chạm vào job VisionPro. Thủ phạm nằm ở một
dòng thông số kỹ thuật ít người đọc kỹ khi chọn mua camera: loại cảm biến dùng **rolling shutter**
— đọc ảnh theo từng hàng pixel nối tiếp nhau thay vì toàn bộ khung hình cùng một lúc. Khi vật thể
còn chuyển động dù chỉ vài phần trăm milimet trong khoảng thời gian đọc hàng cuối trừ hàng đầu,
mỗi hàng pixel "nhìn thấy" vật thể ở một vị trí hơi khác nhau — kết quả là hình dạng bị bóp méo
theo kiểu không ống kính nào tạo ra được, và không tham số phần mềm nào trong QuickBuild sửa được.

Chương này trả lời đúng những câu hỏi cần trả lời **trước khi bấm nút mua camera**: cảm biến CMOS
hoạt động ra sao, và vì sao gần như toàn bộ camera vision công nghiệp là mono thay vì màu (mục
3.1); cần bao nhiêu pixel là đủ cho một dung sai đo lường cụ thể — và MeoVision đã đứng ở lằn ranh
nào khi chọn cảm biến 5 MP (mục 3.2); exposure, gain, frame rate là gì, và vì sao rolling shutter
với global shutter tạo ra hai loại camera khác hẳn nhau về khả năng chụp vật chuyển động (mục
3.3); các chuẩn giao tiếp phổ biến — GigE Vision, USB3 Vision, CameraLink/CoaXPress — và lớp trừu
tượng chung GenICam phía trên chúng (mục 3.4); cách đồng bộ trigger và đèn strobe đúng thời điểm
(mục 3.5); và hai loại camera đặc biệt — line scan, 3D — dành cho bài toán mà camera diện tích 2D
thông thường không giải quyết được (mục 3.6, NÂNG CAO). Những quyết định trong chương này đứng
**trước** calibration (Chương 7) và mọi tool đo lường (Chương 9) trong chuỗi hệ thống — chọn sai
ở đây, không tham số phần mềm nào cứu được.

## 3.1 Cảm biến CMOS: pixel, mono và màu (Bayer)

### 3.1.1 Pixel là gì — cảm biến vốn dĩ chỉ đo độ sáng

Mỗi điểm ảnh (**pixel**) trên cảm biến CMOS là một **photosite** — một giếng thu ánh sáng nhỏ
chuyển đổi photon tới thành điện tích, rồi mạch đọc số hoá điện tích đó thành một giá trị số
nguyên (phổ biến nhất: 8-bit, 0–255; một số cảm biến xuất 10/12-bit cho dải động rộng hơn). Bản
thân một photosite chỉ đếm được **tổng số photon** rơi vào nó trong thời gian phơi sáng — nó
không phân biệt được bước sóng, tức là không "biết" ánh sáng đó màu gì. Sự thật quan trọng cần
nhớ: cảm biến vốn dĩ là một thiết bị đo **cường độ sáng** (ảnh xám/grayscale); màu sắc là một lớp
thông tin bổ sung được tạo ra bằng phần cứng phụ trợ, không phải khả năng gốc của silicon.

### 3.1.2 Bayer filter — cách máy ảnh "nhìn thấy" màu, và cái giá phải trả

Để có ảnh màu, nhà sản xuất phủ một lưới lọc màu cực nhỏ lên trên từng photosite — phổ biến nhất
là **Bayer filter**: một pattern lặp lại 2×2 gồm 1 pixel đỏ (R), 2 pixel xanh lá (G), 1 pixel xanh
dương (B). Tỉ lệ xanh lá gấp đôi vì mắt người nhạy với dải bước sóng này nhất, và thiết kế Bayer
mô phỏng theo cách mắt người cảm nhận độ sáng. Sau bộ lọc, mỗi pixel chỉ còn nhận đúng **một**
trong ba màu — dữ liệu thô đọc ra từ cảm biến (ảnh Bayer, hay ảnh RAW) là một bức tranh khảm
không có ý nghĩa nếu nhìn trực tiếp từng pixel.

Để có ảnh RGB đầy đủ ba kênh tại **mọi** pixel, phần mềm (hoặc chip xử lý ngay trong thân camera)
phải **nội suy** (demosaic) — ước tính hai kênh màu còn thiếu tại mỗi pixel từ giá trị của các
pixel lân cận cùng kênh. Đây chính là phép toán mà công cụ chuyển đổi ảnh của VisionPro (ví dụ
`CogImageConvertTool` — sẽ học chi tiết ở Chương 12, mục 12.1.1) thực hiện khi nhận ảnh thô từ một
camera màu — và cũng là lý do một ảnh màu **không** mang nhiều thông tin không gian
thật hơn một ảnh mono cùng số pixel danh nghĩa: tại mỗi điểm chỉ một trong ba kênh màu được đo
trực tiếp, hai kênh còn lại (2/3 số giá trị) là suy đoán.

![Hình 3.1 — Bayer filter pattern trên cảm biến CMOS và ảnh RAW trước khi demosaic](../assets/ch03/hinh_3_1.png)
**Hình 3.1 — Bayer filter pattern trên cảm biến CMOS và ảnh RAW trước khi demosaic.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ 2 phần (vẽ draw.io). Trái: lưới 4×4 ô vuông đại diện
> photosite, tô màu theo pattern Bayer chuẩn (hàng lẻ: R-G-R-G xen kẽ, hàng chẵn: G-B-G-B xen
> kẽ), mỗi ô ghi chữ cái kênh màu, chú thích "1 photosite = 1 kênh màu duy nhất, 2 kênh còn lại
> tại mỗi điểm là nội suy". Phải: cùng lưới đó sau bước demosaic — mỗi ô hiển thị màu pha trộn RGB đầy
> đủ; mũi tên nối trái→phải ghi nhãn "nội suy (demosaic) — công cụ chuyển đổi ảnh của VisionPro".
> Chú thích dưới hình: tỉ lệ 1 đỏ : 2 xanh lá : 1 xanh dương trong mỗi ô 2×2.

### 3.1.3 Vì sao vision công nghiệp mặc định chọn mono

Ba lý do khiến camera **mono** (không có Bayer filter) là lựa chọn mặc định cho phần lớn ứng dụng
vision công nghiệp, không phải màu như trực giác "camera tốt phải chụp được ảnh đẹp, có màu" của
người mới vào nghề thường nghĩ:

1. **Độ phân giải không gian hiệu dụng cao hơn.** Mỗi pixel mono là một phép đo cường độ thật; mỗi
   pixel màu (sau demosaic) phần lớn là suy đoán từ hàng xóm. Với các tool dựa trên biên/gradient
   như caliper (Chương 9), phép nội suy màu làm mờ đi chính chi tiết cạnh sắc mà tool cần — cùng
   một cảm biến vật lý, phiên bản mono luôn cho biên "sạch" hơn phiên bản màu.
2. **Độ nhạy sáng cao hơn ở cùng thời gian phơi sáng.** Bộ lọc Bayer chặn phần lớn quang phổ không
   thuộc đúng kênh của từng pixel — trung bình mỗi pixel màu chỉ nhận được một phần ánh sáng tới
   so với pixel mono cùng vị trí. Nhiều ánh sáng hơn đến từng pixel nghĩa là có thể phơi sáng ngắn
   hơn ở cùng độ sáng ảnh — một lợi thế trực tiếp cho bài toán vật chuyển động (mục 3.3).
3. **Pipeline xử lý đơn giản và nhanh hơn.** Hầu hết các tool nền tảng của sách này (PMAlign,
   Caliper, Blob, ID) làm việc trên ảnh xám. Camera mono cho dữ liệu sẵn sàng dùng ngay; camera màu
   luôn cần một bước demosaic/convert trước (Chương 12, mục 12.1.1) — thêm độ trễ, thêm một khâu
   có thể sai.

Lý do gốc rễ nằm ở chính bản chất bốn bài toán GIGI của vision công nghiệp (Guidance, Inspection,
Gauging, Identification — Chương 1, mục 1.2): hình dạng, biên, vị trí, và phần lớn mã vạch/ký tự
đều là thông tin **độ sáng**, không phải màu sắc. Màu chỉ trở thành đặc trưng cần thiết khi bài
toán tự nó là phân loại theo màu — kiểm tra đúng màu linh kiện, đúng màu bao bì (Chương 12, mục
12.6) — một thiểu số các trạm vision thực tế, không phải mặc định.

**Bảng 3.1 — Mono vs màu (Bayer) cho ứng dụng vision công nghiệp.**

| Tiêu chí | Mono | Màu (Bayer) |
|---|---|---|
| Độ phân giải không gian hiệu dụng | Đầy đủ — mỗi pixel là một mẫu cường độ thật | Giảm — phần lớn giá trị màu tại mỗi pixel là nội suy |
| Độ nhạy sáng (cùng thời gian phơi sáng) | Cao — mỗi pixel nhận toàn bộ ánh sáng tới | Thấp hơn — bộ lọc Bayer chặn phần lớn quang phổ ngoài kênh của pixel đó |
| Độ phức tạp xử lý | Thấp — dùng trực tiếp cho hầu hết tool nền tảng | Cao hơn — cần demosaic trước khi phần lớn tool xử lý đúng |
| Khi nào chọn | Mặc định cho Guidance/Inspection/Gauging/Identification | Khi màu bản thân là đặc trưng cần kiểm (Chương 12, mục 12.6) |

> 📌 **Lưu ý:** camera chuẩn của MeoVision (5 MP mono GigE Vision, mục 3.2) là ví dụ điển hình của
> nguyên tắc trên — trạm không có bài toán phân loại theo màu, nên không có lý do trả thêm chi phí
> về độ nhạy sáng và độ phân giải hiệu dụng để đổi lấy một khả năng không dùng đến.

## 3.2 Tính độ phân giải cần thiết: chạy ngược từ dung sai ra pixel

### 3.2.1 Quy tắc pixel trên mỗi đơn vị dung sai

Câu hỏi "camera bao nhiêu MP là đủ?" không có câu trả lời chung — nó chỉ có nghĩa khi gắn với một
**dung sai đo lường cụ thể** trên một **FOV cụ thể** (Chương 2, mục 2.4). Một quy tắc kinh nghiệm
phổ biến trong ngành gauging bằng vision: để một phép đo dựa trên hình ảnh đáng tin cậy, cần tối
thiểu khoảng **3 đến 5 pixel trên mỗi đơn vị dung sai** cần phân biệt. Diễn giải công thức:

```text
độ phân giải cần thiết (mm/px)  =  dung sai (mm) / N        (N = 3 .. 5)
số pixel cần trên FOV            =  FOV (mm) / độ phân giải cần thiết
                                  =  FOV (mm) × N / dung sai (mm)
```

### 3.2.2 Chạy ngược con số cho MeoVision

Trạm đo kích thước của MeoVision có dung sai đo ± 0.05 mm trên FOV 100.0 mm (theo phương đo, xem
`reference/MeoVision_Spec.md`). Áp quy tắc 3–5 pixel/dung sai — dùng ngay giá trị ± 0.05 mm làm
"đơn vị dung sai" (không nhân đôi thành dải 0.1 mm; đây cũng là quy ước Chương 9 dùng khi nói "một
pixel lỗi chiếm 80% dung sai cho phép"):

**Bảng 3.2 — Tính ngược độ phân giải cần thiết cho dung sai ± 0.05 mm, FOV 100 mm.**

| Đại lượng | Quy tắc tối thiểu (N = 3) | Quy tắc thoải mái (N = 5) | MeoVision đã chọn |
|---|---|---|---|
| Độ phân giải cần thiết | 0.05 / 3 ≈ 0.0167 mm/px | 0.05 / 5 = 0.01 mm/px | 0.0409 mm/px |
| Số pixel cần trên FOV 100 mm | 100 / 0.0167 ≈ 6 000 px | 100 / 0.01 = 10 000 px | 2 448 px |
| Cỡ cảm biến tương đương (tỉ lệ ~4:3) | ≈ 30 MP | ≈ 84 MP | 5 MP |
| Pixel "phủ" trên dung sai ± 0.05 mm | 3.0 px (theo định nghĩa) | 5.0 px (theo định nghĩa) | 0.05 / 0.0409 ≈ **1.2 px** |

Con số ở cột cuối là điều đáng dừng lại: theo đúng quy tắc kinh điển, MeoVision "thiếu" pixel —
chỉ đạt 1.2 pixel/dung sai, bằng khoảng 1/3 đến 1/4 mức khuyến nghị tối thiểu. Một cảm biến đủ
theo quy tắc 3–5 pixel sẽ cần từ 30 đến 84 MP — lớn hơn nhiều lần so với cảm biến 5 MP thực tế
đang dùng, kéo theo chi phí cảm biến, băng thông truyền dữ liệu (mục 3.4), và thời gian xử lý mỗi
ảnh tăng vọt.

### 3.2.3 Vì sao 1.2 pixel vẫn "đủ" — và vì sao đây không phải điều nên lặp lại tuỳ tiện

Quy tắc 3–5 pixel/dung sai xuất phát từ một bài toán tổng quát hơn: **phân biệt/đếm một đặc trưng
rời rạc** — ví dụ đếm số vạch trên một pattern, phân biệt hai lỗ gần nhau, đọc một ký tự nhỏ. Ở
bài toán đó, thiếu pixel đồng nghĩa với thiếu thông tin để phân biệt — không thuật toán nội suy
nào bù đắp được việc hai đối tượng chung nhau chưa đến một pixel khác biệt.

Đo **một biên liên tục** bằng caliper (Chương 9, mục 9.1) là bài toán khác hẳn về bản chất. Biên
không phải một đối tượng rời rạc cần "đếm" — nó là một điểm chuyển tiếp cường độ dọc theo một
chiều. Vị trí của điểm chuyển tiếp đó có thể được **nội suy** giữa hai pixel bằng hình dạng của
đường cong gradient cường độ quanh biên (cơ chế cụ thể: Chương 9, mục 9.1.1), miễn tương phản đủ
tốt và nhiễu không áp đảo tín hiệu. Đây là lý do một hệ đo dựa trên caliper có thể hoạt động ở mật
độ pixel/dung sai thấp hơn nhiều so với quy tắc dành cho bài toán phân giải đặc trưng rời rạc —
và cũng là lý do MeoVision "chỉ" 1.2 pixel/dung sai vẫn là một lựa chọn khả thi, không phải một
sai lầm khi thiết kế.

> ⚠️ **Cảnh báo:** "khả thi" không đồng nghĩa với "dư dả". Ở 1.2 pixel/dung sai, mọi nguồn sai số
> khác trong chuỗi đo — chất lượng calibration (RMS ≤ 0.5 px, Chương 7), độ ổn định vật lý của
> chính biên đo (cạnh cắt CNC sắc nét khác hẳn cạnh đúc nhựa có ba-via, Chương 9 mục 9.4.1), tương
> phản ánh sáng (Chương 2), và giới hạn nội suy sub-pixel của bản thân thuật toán caliper — đều
> phải cộng dồn và **vẫn** nằm gọn trong ± 0.05 mm. MeoVision không có biên độ dự phòng lớn để hấp
> thụ một yếu tố xuống cấp bất ngờ (đèn trôi, camera lệch nét — Chương 12, mục 12.1.3). Đây chính
> là lý do Chương 7 đặt ngưỡng RMS calibration khắt khe, và Chương 9 (mục 9.4.2) nhất quyết đòi đo
> **repeatability** thực tế bằng 30 lần chụp thay vì tin vào con số tính trên giấy — với biên độ
> dự phòng mỏng như thế này, "tính đủ trên giấy" không đảm bảo "đo đủ trên máy thật".

> 📌 **Lưu ý:** đừng nhầm dung sai đo ± 0.05 mm (mục này) với dung sai đặt vị trí của robot
> ± 0.1 mm (`MeoVision_Spec.md`). Hai con số phục vụ hai bài toán khác nhau — đo lường (Gauging,
> dùng caliper, phép tính ngược ở trên) và dẫn hướng robot (Guidance, dùng độ chính xác pose của
> PMAlign — Chương 8) — không áp cùng một phép tính pixel/dung sai cho cả hai.

## 3.3 Exposure, gain, frame rate — và vật chuyển động

### 3.3.1 Exposure: đánh đổi giữa ánh sáng và độ mờ chuyển động

**Exposure** (thời gian phơi sáng) là khoảng thời gian mỗi photosite được phép tích luỹ điện tích
trước khi mạch đọc số hoá giá trị đó. Exposure càng dài, ảnh càng sáng (ở cùng điều kiện ánh
sáng) — nhưng nếu vật thể (hoặc camera) còn di chuyển trong khoảng thời gian đó, ảnh sẽ nhoè theo
đúng quãng đường đã di chuyển. Công thức đơn giản đủ dùng cho ước lượng thực địa:

```text
độ nhoè (mm) ≈ vận tốc tương đối (mm/s) × thời gian phơi sáng (s)
```

Ví dụ minh hoạ (một trạm băng tải chạy liên tục, không phải MeoVision — MeoVision dùng pallet
index dừng hẳn khi chụp, xem Chương 7): vật di chuyển ở 300 mm/s, phơi sáng 1 ms (0.001 s) cho độ
nhoè ≈ 0.3 mm — đã lớn gấp nhiều lần một dung sai đo cỡ ± 0.05 mm, và đủ để làm hỏng cả những bài
toán chỉ cần định vị (Guidance) nếu đặc trưng cần bắt nhỏ hơn 0.3 mm. Giảm phơi sáng xuống 100 µs
(0.0001 s) đưa độ nhoè về ≈ 0.03 mm — chấp nhận được cho phần lớn bài toán, nhưng đổi lại: ảnh tối
đi 10 lần ở cùng điều kiện sáng, buộc phải bù bằng cách tăng công suất đèn (Chương 2) hoặc tăng
gain (mục 3.3.2).

### 3.3.2 Gain: khuếch đại tín hiệu — và cả nhiễu

**Gain** là hệ số khuếch đại điện tử áp lên tín hiệu **sau khi** photosite đã thu xong ánh sáng,
trước khi số hoá. Khác exposure — vốn thu thêm ánh sáng thật, mang thêm thông tin — gain chỉ nhân
lên những gì đã có, **bao gồm cả nhiễu nền** đã tồn tại sẵn trong tín hiệu. Tăng gain làm ảnh sáng
hơn nhưng không cải thiện tỉ số tín hiệu/nhiễu (SNR) — thường còn làm SNR xấu đi, biểu hiện thành
ảnh "hạt" (grainy), làm giảm độ tin cậy của các bước tìm biên dựa trên ngưỡng tương phản
(`ContrastThreshold` của caliper — Chương 9, mục 9.1.1).

**Vì sao gain không "miễn phí"** — bản thân tín hiệu ánh sáng trước khi khuếch đại đã mang sẵn
nhiều nguồn nhiễu cộng dồn, mỗi nguồn một nguyên nhân vật lý khác nhau:

- **Nhiễu hạt photon (shot noise)** — ánh sáng đến cảm biến dưới dạng các hạt photon rời rạc, số
  lượng hạt đến trong một khoảng thời gian dao động ngẫu nhiên quanh giá trị trung bình (bản chất
  thống kê của ánh sáng, không phải lỗi thiết kế cảm biến) — nhiễu này **luôn tồn tại**, kể cả với
  cảm biến lý tưởng tuyệt đối, và tăng theo căn bậc hai của cường độ sáng.
- **Nhiễu dòng tối (dark current noise)** — cảm biến vẫn sinh ra một lượng nhỏ điện tích ngay cả
  khi không có ánh sáng nào chiếu vào (do nhiệt), tăng theo nhiệt độ cảm biến và theo thời gian
  exposure — một lý do camera công nghiệp chất lượng cao thường có thiết kế tản nhiệt kỹ.
- **Nhiễu đọc (read noise)** — sai số phát sinh trong chính mạch điện tử đọc và số hoá tín hiệu,
  gần như cố định bất kể cường độ ánh sáng.
- **Nhiễu lượng tử hoá (quantization noise)** — sai số làm tròn khi giá trị tương tự liên tục được
  số hoá thành mức rời rạc (ví dụ 256 mức cho ảnh 8-bit).

Gain khuếch đại **tất cả** các nguồn trên cùng với tín hiệu thật — không có cách nào để mạch khuếch
đại "biết" đâu là ánh sáng thật cần tăng, đâu là nhiễu cần giữ nguyên. Đây là lý do vật lý cốt lõi
đứng sau khuyến nghị "gain là lựa chọn cuối cùng": tăng exposure hoặc tăng công suất đèn đều làm
**tăng lượng tín hiệu thật** thu được (cải thiện SNR vì tín hiệu tăng nhanh hơn nhiễu shot-noise đi
kèm nó), trong khi tăng gain chỉ phóng to nguyên trạng tỉ lệ tín hiệu/nhiễu đã có sẵn — không thêm
thông tin nào, chỉ thêm độ sáng.

> 💡 **Mẹo thực chiến:** thứ tự ưu tiên khi ảnh thiếu sáng: cải thiện ánh sáng vật lý (Chương 2)
> trước, sau đó mới tăng exposure nếu vật đứng yên cho phép, và chỉ tăng gain như **lựa chọn cuối
> cùng** khi hai phương án trên đã hết dư địa. Một trạm sản xuất ổn định hiếm khi cần gain cao —
> gain cao thường trú là dấu hiệu ánh sáng chưa đủ, không phải một cấu hình camera bình thường.

### 3.3.3 Frame rate và ngân sách thời gian acquisition

Thời gian để có một khung hình hoàn chỉnh gồm hai phần nối tiếp: **exposure** (thu sáng) và
**readout** (đọc dữ liệu ra khỏi cảm biến, chuyển thành khung hình số). Ở chế độ đơn giản nhất
(không chồng lấn), frame rate tối đa mà một camera đạt được ở một cấu hình cho trước xấp xỉ nghịch
đảo của tổng hai khoảng thời gian đó (cộng thêm overhead xử lý bên trong camera) — đây là mô hình
gần đúng, dùng an toàn cho ước lượng ban đầu. Exposure càng dài, readout càng chậm (do độ phân giải
lớn hoặc do giao tiếp băng thông thấp — mục 3.4), frame rate tối đa càng giảm.

> 📌 **Lưu ý:** nhiều camera công nghiệp hỗ trợ chế độ **overlapped exposure** (phơi sáng khung kế
> tiếp trong lúc khung hiện tại vẫn đang readout) — khi bật chế độ này, frame rate tối đa có thể
> tiệm cận nghịch đảo của giá trị **lớn hơn** giữa exposure và readout thay vì tổng của chúng. Tính
> theo công thức "exposure + readout" ở trên là cách ước lượng an toàn (không đòi hỏi biết camera
> có hỗ trợ overlap hay không); nếu cần khai thác tối đa nhịp máy, kiểm tra datasheet/GenICam của
> camera cụ thể xem tính năng này có sẵn và đã bật chưa.

Ý nghĩa thực tế của phép cộng "exposure + readout" vượt xa việc chọn thông số camera: đây chính
là phần **acquisition** trong tổng ngân sách thời gian một cycle vision phải hoàn thành. Tổng thời
gian cycle không chỉ là thời gian xử lý thuật toán trong VisionPro (đo được bằng
`ICogRunStatus.ProcessingTime`, Chương 13, mục 13.2) — nó bắt đầu từ lúc trigger được nhận, đi qua
exposure và readout, rồi mới đến xử lý. Ở nhịp máy nhanh, phần acquisition có thể chiếm tỉ trọng
đáng kể trong ngân sách cycle; Chương 15 (mục 15.2) đo và cam kết con số tổng hợp này trên trạm
thật, dựa trực tiếp trên hai đại lượng vừa giới thiệu ở đây.

### 3.3.4 Global shutter vs rolling shutter

Đây là nguyên nhân của sự cố mở đầu chương — và là quyết định camera có ảnh hưởng lớn nhất đến khả
năng chụp vật chuyển động, độc lập với exposure/gain.

**Rolling shutter**: cảm biến đọc ảnh theo từng hàng pixel nối tiếp nhau, từ trên xuống dưới, mỗi
hàng bắt đầu và kết thúc phơi sáng lệch nhau một khoảng thời gian rất nhỏ so với hàng liền kề. Với
cảnh **tĩnh**, độ lệch này vô hại — mọi hàng đều "nhìn thấy" đúng một cảnh không đổi, chỉ khác thời
điểm không quan trọng. Với cảnh có **chuyển động tương đối** trong lúc đọc (vật di chuyển, hoặc
rung động dư chấn như tình huống mở đầu), mỗi hàng bắt được vật thể ở một vị trí hơi khác — kết
quả là hình dạng bị nghiêng, xé, hoặc "trôi" (rolling shutter artifact) tuỳ hướng và tốc độ chuyển
động.

**Global shutter**: toàn bộ pixel trên cảm biến bắt đầu và kết thúc phơi sáng tại **đúng cùng một
thời điểm** — mạch giữ điện tích tại từng pixel trong lúc chờ đọc tuần tự ra ngoài. Vì mọi pixel
"đóng băng" cùng một khoảnh khắc, ảnh thu được luôn đại diện đúng một thời điểm duy nhất, bất kể
vật thể di chuyển nhanh đến đâu trong lúc readout.

**Bảng 3.3 — Rolling shutter vs global shutter cho ứng dụng vision công nghiệp.**

| Tiêu chí | Rolling shutter | Global shutter |
|---|---|---|
| Cách phơi sáng | Từng hàng nối tiếp, lệch thời gian | Toàn bộ khung hình cùng một thời điểm |
| Cảnh vật tĩnh | Không vấn đề | Không vấn đề |
| Vật/camera chuyển động khi chụp | Méo hình (skew, xé, trôi) | Không méo — đại diện đúng một khoảnh khắc |
| Kết hợp với strobe chớp nhanh (mục 3.5) | Khó — các hàng phơi sáng lệch thời điểm nên một xung strobe ngắn không chiếu đủ cho mọi hàng | Tương thích tốt — mọi pixel cùng "chờ" trong một cửa sổ thời gian như nhau |
| Chi phí/độ nhạy (cùng công nghệ, cùng thời điểm) | Thường rẻ hơn, độ nhạy/pixel nhỉnh hơn đôi chút | Thường đắt hơn một chút — thêm mạch giữ điện tích mỗi pixel |
| Khi nào dùng được | Chi tiết chắc chắn đứng yên hoàn toàn lúc chụp (đã hết dư chấn) | Có bất kỳ chuyển động nào khi chụp — mặc định an toàn cho vision công nghiệp |

> 📌 **Lưu ý:** rolling shutter không phải công nghệ "kém" bị global shutter thay thế hoàn toàn —
> nó vẫn tồn tại phổ biến vì mỗi pixel không cần thêm mạch giữ điện tích riêng (điều global shutter
> bắt buộc phải có), nên diện tích thu sáng thật trên mỗi pixel (**fill factor**) lớn hơn, cho độ
> nhạy sáng nhỉnh hơn ở cùng công nghệ và giá thành thấp hơn. Với chi tiết chắc chắn đứng yên tuyệt
> đối (ví dụ đặt cố định trên bàn kiểm, không có dư chấn cơ khí), rolling shutter vẫn là lựa chọn
> hợp lý; global shutter chỉ là bắt buộc khi có khả năng chuyển động lúc chụp.

![Hình 3.2 — So sánh cơ chế phơi sáng rolling shutter và global shutter với vật đang chuyển động](../assets/ch03/hinh_3_2.png)
**Hình 3.2 — So sánh cơ chế phơi sáng rolling shutter và global shutter với vật đang chuyển động.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ 2 hàng (vẽ draw.io). Hàng trên "Rolling shutter": bên
> trái một chuỗi thanh ngang xếp chồng (đại diện các hàng pixel) mỗi thanh có cửa sổ phơi sáng vẽ
> lệch nhau theo trục thời gian (bậc thang chéo); bên phải minh hoạ kết quả — hình chữ nhật đại
> diện chi tiết bị vẽ nghiêng/xé thành các dải lệch ngang, mũi tên ghi "hướng chuyển động". Hàng
> dưới "Global shutter": cùng chuỗi thanh ngang nhưng cửa sổ phơi sáng thẳng hàng (không lệch);
> bên phải hình chữ nhật chi tiết giữ nguyên hình dạng vuông vắn dù cùng hướng chuyển động. Chú
> thích chung dưới hình: "Cùng một vật chuyển động, cùng tốc độ — khác nhau ở cách cảm biến đọc
> ảnh".

> ⚠️ **Cảnh báo:** đừng chỉ dựa vào tín hiệu "chi tiết đã tới vị trí" từ cảm biến để coi là vật đã
> đứng yên hoàn toàn. Dư chấn cơ khí sau một cú dừng đột ngột (pallet, xy-lanh, cơ cấu index) vẫn
> là chuyển động — đúng nguyên nhân của sự cố mở đầu chương. Nếu dùng cảm biến rolling shutter,
> thời gian settle trước khi trigger phải đủ dài để triệt tiêu hoàn toàn dư chấn đó, đo bằng thực
> nghiệm (rung ký hoặc quan sát ảnh liên tiếp), không chỉ đủ để cảm biến vị trí báo "đã tới". Với
> bất kỳ trạm nào không chắc chắn tuyệt đối về việc này, global shutter là lựa chọn an toàn hơn
> nhiều so với việc đặt cược vào thời gian settle.

## 3.4 Giao tiếp: GigE Vision, USB3 Vision, CameraLink/CoaXPress, và GenICam

### 3.4.1 Bốn chuẩn giao tiếp phổ biến

Camera công nghiệp truyền dữ liệu ảnh về máy tính qua một trong bốn chuẩn phổ biến, khác nhau chủ
yếu ở băng thông, chiều dài cáp cho phép, và việc có cần một card phần cứng chuyên dụng (**frame
grabber**) hay không:

**Bảng 3.4 — So sánh các chuẩn giao tiếp camera công nghiệp phổ biến.**

| Chuẩn | Băng thông danh nghĩa | Chiều dài cáp điển hình | Cần frame grabber? | Đặc điểm |
|---|---|---|---|---|
| GigE Vision | ~1 Gbps (~125 MB/s lý thuyết) | Tới ~100 m (cáp mạng Cat5e/6 chuẩn) | Không — cắm thẳng NIC/switch | Cáp phổ biến, rẻ, dễ kéo dài; nhiều camera dùng chung switch nhưng chia sẻ băng thông (mục 3.4.2) |
| USB3 Vision | ~5 Gbps (USB3 SuperSpeed) | Vài mét không có bộ lặp tín hiệu | Không | Băng thông cao hơn GigE trên một kết nối; cắm-chạy đơn giản, hạn chế khoảng cách |
| CameraLink | Vài trăm MB/s đến hơn 1 GB/s (tuỳ cấu hình Base/Medium/Full) | ~10 m (cáp chuyên dụng) | Có, bắt buộc | Băng thông rất cao, độ trễ thấp; phổ biến cho line scan tốc độ cao (mục 3.6) |
| CoaXPress | Tới 12.5 Gbps mỗi link, ghép được nhiều link | Hàng chục mét trên cáp đồng trục | Có, bắt buộc | Băng thông cao nhất trên khoảng cách xa; thay thế CameraLink ở nhiều thiết kế mới |

### 3.4.2 Bài toán băng thông: ví dụ MeoVision

Camera MeoVision (2448 × 2048 px, mono 8-bit, chưa nén) tạo ra mỗi khung hình:

```text
2448 × 2048 = 5 013 504 byte  ≈  5.0 MB / khung hình
```

Một kết nối GigE Vision lý thuyết đạt 125 MB/s, nhưng thực tế sau overhead giao thức (packet
header, GVSP, driver/NIC) thường chỉ đạt bền vững khoảng 100–115 MB/s. Với một khung ~5.0 MB,
thời gian truyền một ảnh mất khoảng 5.0 / 110 ≈ 45 ms — và nếu camera cố streaming liên tục ở băng
thông tối đa, frame rate trần lý thuyết cho **một** camera đã xấp xỉ 20–22 fps, gần như chiếm trọn
một liên kết Gigabit.

Hệ quả trực tiếp: nếu hai camera 5 MP cùng loại chia sẻ **một** cổng uplink của switch về một card
mạng (NIC) duy nhất trên PC, tổng nhu cầu băng thông (~150–200 MB/s ở fps tương đương) vượt xa
~100–115 MB/s khả dụng — dẫn đến nghẽn, rớt gói, hoặc buộc phải giảm fps/độ phân giải của cả hai
camera. Cách xử lý thực tế (cấu hình chi tiết cho VisionPro: Chương 6, mục 6.5) gồm: cắm mỗi
camera vào một cổng NIC vật lý riêng thay vì dùng chung switch, tinh chỉnh packet size (**jumbo
frame** — tăng kích thước gói tin mạng, thường lên MTU 9000 byte thay vì 1500 byte mặc định, để
giảm số gói phải xử lý cho cùng lượng dữ liệu, giảm tải CPU), hoặc giảm **ROI** (Region of
Interest — chỉ đọc và truyền một vùng nhỏ hơn của cảm biến thay vì toàn khung hình, giảm trực tiếp
lượng dữ liệu cần truyền)/fps khi băng thông thực sự là nút thắt.

> 📌 **Lưu ý:** con số ~5.0 MB/khung ở trên tính trên dữ liệu pixel thô 8-bit/pixel, chưa kể
> header giao thức hay overhead phần mềm — dùng để ước lượng bậc độ lớn khi lập kế hoạch mạng, không
> phải con số cam kết chính xác tuyệt đối cho mọi cấu hình camera.

### 3.4.3 GenICam: lớp trừu tượng chung phía trên phần cứng

Bốn chuẩn giao tiếp ở mục 3.4.1 giải quyết vấn đề "dữ liệu đi bằng đường nào" — nhưng còn một vấn
đề khác: mỗi hãng camera có một tập tham số cấu hình riêng (exposure, gain, trigger mode, vùng
đọc...) với tên gọi khác nhau. Nếu phần mềm phải viết riêng cho từng hãng camera, chuyển nhà cung
cấp camera đồng nghĩa viết lại code. **GenICam** (Generic Interface for Cameras) là chuẩn công
nghiệp giải quyết đúng vấn đề đó: mỗi camera tuân thủ GenICam đi kèm một file mô tả (XML) khai báo
toàn bộ tham số cấu hình của nó theo một mô hình chung (`GenApi`) — phần mềm đọc file mô tả này để
biết camera có những tham số gì, kiểu dữ liệu gì, phạm vi giá trị nào, mà không cần biết trước đó
là camera hãng nào.

GenICam độc lập với tầng truyền dữ liệu — cùng một mô hình tham số áp dụng được cho camera GigE
Vision, USB3 Vision, CameraLink, hay CoaXPress (mỗi chuẩn truyền có một lớp "GenTL producer" riêng
làm cầu nối). Đây là lý do một công cụ thu nhận ảnh như `CogAcqFifoTool` của VisionPro (Chương 6,
mục 6.1) có thể nói chuyện với hầu hết camera công nghiệp trên thị trường thông qua cùng một cơ
chế cấu hình, thay vì cần một driver/SDK riêng cho từng hãng — GenICam là chuẩn chung của ngành,
không phải công nghệ riêng của Cognex hay bất kỳ hãng camera nào.

## 3.5 Trigger phần cứng vs phần mềm; đồng bộ đèn strobe

### 3.5.1 Trigger phần cứng

**Trigger phần cứng** là một tín hiệu điện áp thực (thường 24V digital input, cách ly quang) đưa
trực tiếp vào chân trigger của camera — phổ biến nhất là từ ngõ ra PLC hoặc trực tiếp từ một cảm
biến quang/tiệm cận phát hiện chi tiết đã vào đúng vị trí. Độ trễ giữa lúc tín hiệu tới chân
trigger và lúc camera thực sự bắt đầu phơi sáng nằm ở mức micro giây — thấp và **ổn định** (ít
jitter giữa các lần). Đây là lựa chọn bắt buộc khi thời điểm chụp cần đồng bộ chặt với một sự kiện
cơ khí — chi tiết đang chuyển động, hoặc cần đồng bộ với xung strobe (mục 3.5.3).

### 3.5.2 Trigger phần mềm

**Trigger phần mềm** là một lệnh gửi qua chính kênh giao tiếp dữ liệu (ví dụ lệnh `TriggerSoftware`
theo mô hình GenICam, mục 3.4.3) thay vì một dây tín hiệu riêng — không cần đấu nối phần cứng bổ
sung. Đổi lại, độ trễ từ lúc phần mềm gọi lệnh đến lúc camera thực sự phơi sáng phụ thuộc vào hệ
điều hành, driver, và tải hệ thống lúc đó — dao động (jitter) có thể từ dưới một mili giây đến vài
mili giây, không ổn định như trigger phần cứng. Phù hợp cho các tình huống không đòi hỏi đồng bộ
thời gian chặt: chụp thủ công trong QuickBuild lúc phát triển job (Chương 5, mục 5.3), hoặc quy
trình mà chi tiết chắc chắn đã đứng yên từ trước khi lệnh chụp được gửi.

### 3.5.3 Đồng bộ strobe — và vì sao cần global shutter

Đèn **strobe** — chớp sáng cực ngắn và cực mạnh, thay vì chiếu sáng liên tục — là kỹ thuật phổ
biến để "đóng băng" vật chuyển động mà không cần rút ngắn exposure điện tử của camera đến giới hạn
kỹ thuật. Nguyên tắc đồng bộ: xung sáng strobe phải nằm **trọn vẹn bên trong** cửa sổ exposure của
camera — bắt đầu sau khi exposure mở, kết thúc trước khi exposure đóng.

```text
Cảm biến vị trí   ----[phát hiện chi tiết]----------------------------->
                              |
                              v
PLC               -----------[Trigger = 1] (xung vài ms)--------------->
                              |
                              | độ trễ trigger (~µs với trigger phần cứng,
                              | có thể ~ms với trigger phần mềm — mục 3.5.1/3.5.2)
                              v
Camera                        [======== Exposure (t_exp) ========]----->
                                        |
Đèn (strobe)                   [==== xung sáng ====]
                                (nằm TRỌN trong t_exp, không tràn ra ngoài)
                                                    |
Camera                                              [=== Readout ===]-->
                                                                |
                                                                v
                                                    Dữ liệu ảnh qua GigE/USB3
                                                    (mục 3.4) → phần mềm
```

Điểm tinh tế đáng biết: Chương 2 (mục 2.3) đã nhắc đến kỹ thuật "strobe overdrive" khi bàn chống
nhiễu ánh sáng môi trường — đây chính là ứng dụng nguyên lý đó ở tầng timing. Có thể đặt exposure
điện tử tương đối rộng (dễ chỉnh, ít nhạy với jitter trigger), nhưng để đèn strobe là nguồn sáng
**chi phối tuyệt đối** trong một cửa sổ ngắn hơn nhiều, nằm bên trong exposure đó. Khi đó, hiệu
ứng "đóng băng" chuyển động phụ thuộc vào **độ rộng xung strobe**, không phải độ rộng exposure
điện tử — cho phép freeze chuyển động rất nhanh mà không cần một cảm biến có exposure điện tử cực
ngắn (và đắt đỏ) tương ứng.

Điều kiện tiên quyết để kỹ thuật này hoạt động đúng: camera phải dùng **global shutter** (mục
3.3.4). Với rolling shutter, các hàng pixel phơi sáng lệch thời điểm nhau — một xung strobe ngắn
chỉ chiếu sáng đúng lúc cho một số hàng, các hàng còn lại (đang phơi sáng ở thời điểm strobe đã
tắt) sẽ tối hoặc sọc không đều. Đây là lý do bảng 3.3 liệt kê "kết hợp với strobe" là điểm yếu rõ
rệt của rolling shutter, không chỉ là vấn đề lý thuyết.

> 📌 **Lưu ý:** ai bấm nút bật đèn strobe trong sơ đồ trên? **Không phải PLC.** Nếu PLC xuất hai
> tín hiệu độc lập — một dây vào camera, một dây vào đèn — độ trễ cơ điện và chu kỳ quét (scan
> cycle) của PLC khiến hai tín hiệu lệch nhau vài mili giây, đủ để xung sáng lọt ra ngoài cửa sổ
> exposure. Cách làm đúng: PLC chỉ trigger **camera**; chính camera dùng một chân I/O phần cứng
> riêng (thường gọi **Strobe Output**/**Flash Out**, cấu hình qua GenICam — mục 3.4.3) để tự phát
> tín hiệu bật đèn, khoá chặt vào đúng cửa sổ phơi sáng của nó ở độ chính xác micro giây.

## 3.6 [NÂNG CAO] Line scan camera và camera 3D

### 3.6.1 Line scan camera

Camera diện tích (area scan) — loại đã bàn xuyên suốt chương này — chụp một khung hình 2D hoàn
chỉnh mỗi lần trigger. **Line scan camera** khác về nguyên lý: cảm biến chỉ có một (hoặc vài) hàng
pixel duy nhất; ảnh 2D được **dựng dần** bằng cách ghép liên tiếp các dòng quét khi vật thể (hoặc
camera) di chuyển tương đối so với nhau — mỗi dòng ứng với một xung trigger, thường lấy từ
encoder gắn trên trục chuyển động để đảm bảo khoảng cách giữa các dòng đều nhau bất kể tốc độ dao
động.

> 🔍 **Đào sâu thêm:** line scan phù hợp cho vật liệu dạng cuộn/tấm liên tục (băng vải, giấy, kim
> loại cuộn, kính) — nơi chiều dài theo hướng chuyển động về lý thuyết không giới hạn, và độ phân
> giải theo chiều ngang có thể rất cao (hàng nghìn đến hàng chục nghìn pixel một dòng) mà không
> cần ghép nhiều camera diện tích. Đổi lại, hệ thống đòi hỏi đồng bộ chuyển động chính xác (encoder
> — nếu tốc độ không đều mà không bù bằng encoder, ảnh bị co giãn theo hướng quét) và thường dùng
> giao tiếp băng thông cao (CameraLink/CoaXPress, mục 3.4) do tốc độ dữ liệu liên tục lớn. Nhận
> diện bài toán cần line scan: "vật liệu chuyển động liên tục, không có điểm dừng để chụp một khung
> area scan hoàn chỉnh" — nếu chi tiết rời rạc và có thể dừng/index (như MeoVision), area scan vẫn
> là lựa chọn đơn giản hơn.

### 3.6.2 Camera 3D

Mọi camera bàn đến từ đầu chương đến giờ đo được vị trí X, Y trên mặt phẳng ảnh — không đo được
**chiều cao/độ sâu (Z)**. Một số bài toán kiểm tra không thể trả lời chỉ bằng ảnh 2D: chi tiết
đồng phẳng hay bị vênh (coplanarity), chiều cao vệt keo có đủ hay không, chân linh kiện có bị cong
ra khỏi mặt phẳng hay không — hai chi tiết có thể trông **giống hệt nhau** trên một ảnh 2D top-down
dù chiều cao hoàn toàn khác nhau. Đây là lúc cần **camera 3D**, dùng một trong vài kỹ thuật phổ
biến: tam giác hoá bằng laser (chiếu một vệt laser, tính độ lệch để suy ra chiều cao dọc theo một
đường quét — về cơ chế thu ảnh gần với line scan), ánh sáng cấu trúc (structured light — chiếu
pattern để tính profile 3D toàn trường), stereo (hai camera tam giác hoá như mắt người), hoặc
time-of-flight (đo thời gian ánh sáng phản xạ về).

> 🔍 **Đào sâu thêm:** nhận diện bài toán cần camera 3D bằng một câu hỏi đơn giản: "hai chi tiết
> khác nhau chỉ ở chiều cao/độ sâu có cho ra ảnh 2D khác nhau không?" — nếu câu trả lời là không,
> không tổ hợp ánh sáng/ống kính 2D nào (Chương 2) giải quyết được, bài toán cần thêm chiều đo Z.
> Camera 3D và các gói mở rộng 3D chuyên dụng nằm ngoài phạm vi chi tiết của sách này (xem ghi chú
> phạm vi ở Outline); mục đích của mục này chỉ là giúp nhận ra đúng lúc cần tìm đến chúng thay vì
> cố ép một giải pháp 2D vào một bài toán vốn dĩ ba chiều.

## Tổng kết chương

- Cảm biến CMOS vốn dĩ chỉ đo cường độ sáng; ảnh màu cần thêm bộ lọc Bayer và một bước nội suy
  (demosaic) tại phần lớn pixel — đây là lý do vision công nghiệp mặc định chọn **mono**: độ phân
  giải hiệu dụng cao hơn, nhạy sáng hơn, pipeline đơn giản hơn (mục 3.1).
- Độ phân giải camera cần thiết phải được tính **ngược từ dung sai đo lường** trên FOV cụ thể,
  không chọn theo "chấm MP". Với dung sai ± 0.05 mm trên FOV 100 mm, MeoVision chỉ đạt ~1.2
  pixel/dung sai — dưới mức khuyến nghị 3–5 pixel của quy tắc chung, nhưng vẫn khả thi nhờ khả năng
  nội suy sub-pixel của caliper (Chương 9) — với điều kiện mọi khâu khác trong chuỗi đo (calibration,
  ánh sáng, repeatability) được kiểm soát chặt, không có nhiều biên độ dự phòng (mục 3.2).
- Exposure kiểm soát độ nhoè chuyển động và độ sáng bằng cách thu thêm ánh sáng thật; gain chỉ
  khuếch đại những gì đã có, kể cả nhiễu — ưu tiên cải thiện ánh sáng/exposure trước khi tăng gain.
  Exposure + readout là phần acquisition trong ngân sách cycle time (nền tảng cho Chương 15, mục
  15.2) (mục 3.3).
- Rolling shutter đọc ảnh theo hàng, lệch thời điểm — gây méo hình với vật chuyển động; global
  shutter phơi sáng toàn khung cùng lúc, an toàn hơn cho mọi tình huống có khả năng chuyển động
  (mục 3.3.4).
- Bốn chuẩn giao tiếp phổ biến (GigE Vision, USB3 Vision, CameraLink, CoaXPress) khác nhau về băng
  thông và nhu cầu frame grabber; GenICam là lớp trừu tượng chung phía trên chúng, cho phép phần
  mềm cấu hình camera không phụ thuộc hãng sản xuất (mục 3.4).
- Trigger phần cứng cho độ trễ thấp và ổn định, cần thiết khi đồng bộ với chuyển động hoặc strobe;
  trigger phần mềm đơn giản hơn về đấu nối nhưng jitter cao hơn. Strobe phải nằm trọn trong cửa sổ
  exposure, và chỉ hoạt động đúng với global shutter (mục 3.5).
- Line scan giải quyết bài toán vật liệu chuyển động liên tục không có điểm dừng; camera 3D giải
  quyết bài toán mà hai chi tiết chỉ khác nhau ở chiều cao/độ sâu vẫn cho ảnh 2D giống hệt nhau
  (mục 3.6, NÂNG CAO).

## Lỗi thường gặp

**Lỗi 1 — Dùng rolling shutter cho vật còn dư chấn chuyển động.** Hiện tượng: ảnh bị nghiêng, xé,
hoặc kết quả đo dao động lớn giữa các lần chụp cùng một chi tiết, dù chi tiết "đã dừng" theo tín
hiệu cảm biến vị trí. Nguyên nhân: thời gian settle sau khi dừng cơ khí chưa đủ để triệt tiêu dư
chấn, và cảm biến dùng rolling shutter khiến chuyển động dù rất nhỏ cũng gây méo hình (mục 3.3.4).
Cách tránh: đo thực nghiệm thời gian settle cần thiết; với trạm không chắc chắn tuyệt đối về việc
này, chọn global shutter làm phương án an toàn mặc định.

**Lỗi 2 — Nhiều camera chia sẻ một switch/NIC vượt quá băng thông GigE khả dụng.** Hiện tượng:
frame rate tụt, ảnh rớt khung, hoặc lỗi timeout thu ảnh ngẫu nhiên khi thêm camera thứ hai vào
cùng hệ thống mạng. Nguyên nhân: tổng nhu cầu băng thông của các camera vượt mức ~100–115 MB/s
khả dụng của một liên kết Gigabit dùng chung (mục 3.4.2). Cách tránh: cắm mỗi camera vào một cổng
NIC vật lý riêng khi có thể, hoặc giảm fps/ROI/kích thước ảnh để tổng băng thông nằm trong giới
hạn; xem cấu hình chi tiết trong VisionPro ở Chương 6, mục 6.5.

**Lỗi 3 — Chọn camera theo "chấm MP" thay vì tính ngược từ dung sai.** Hiện tượng: mua camera độ
phân giải cao hơn cần thiết (tốn kém, băng thông lớn, xử lý chậm hơn) hoặc thấp hơn cần thiết
(không đo được dung sai yêu cầu), do quyết định dựa trên cảm tính "camera càng nhiều MP càng tốt".
Nguyên nhân: bỏ qua bước tính ngược độ phân giải cần thiết từ dung sai đo và FOV cụ thể (mục 3.2).
Cách tránh: luôn chạy phép tính ở mục 3.2.1–3.2.2 trước khi chọn cảm biến, và đối chiếu con số
pixel/dung sai cuối cùng với việc bài toán là đo biên liên tục (caliper, dư địa thấp vẫn khả thi)
hay phân giải đặc trưng rời rạc (cần bám sát quy tắc 3–5 pixel).

**Lỗi 4 — Tăng gain để bù thiếu sáng thay vì sửa nguồn sáng.** Hiện tượng: ảnh đủ sáng nhưng nhiễu
hạt nhiều, caliper/PMAlign hoạt động không ổn định dù ánh sáng "trông có vẻ đủ" trên màn hình.
Nguyên nhân: gain khuếch đại cả nhiễu nền, không thêm thông tin thật như tăng exposure hay cải
thiện đèn (mục 3.3.2). Cách tránh: ưu tiên sửa ánh sáng vật lý và exposure trước; coi gain cao
thường trực là dấu hiệu cảnh báo cần điều tra lại nguồn sáng, không phải một cấu hình bình thường.

**Lỗi 5 — Strobe không nằm trọn trong cửa sổ exposure, hoặc dùng chung với rolling shutter.** Hiện
tượng: ảnh sáng không đều, một phần khung hình tối hơn phần còn lại, hoặc hiệu ứng "đóng băng"
chuyển động không đạt như kỳ vọng dù đã dùng đèn strobe công suất cao. Nguyên nhân: xung strobe
lệch ra ngoài cửa sổ exposure (bắt đầu sớm/kết thúc muộn), hoặc camera dùng rolling shutter khiến
các hàng pixel phơi sáng không đồng thời với xung strobe ngắn (mục 3.5.3). Cách tránh: kiểm tra
timing bằng dao động ký/tính năng debug I/O của camera trước khi đưa vào sản xuất; mặc định dùng
global shutter cho mọi trạm kết hợp strobe với vật chuyển động.

\newpage

# Chương 4 — Cơ sở xử lý ảnh số

Buổi tối thứ Sáu, một kỹ sư mới của trạm MeoVision ở lại sau giờ để chỉnh cho xong một con số
duy nhất: ngưỡng (threshold) của bước kiểm đếm miếng đệm cao su. Ban ngày job chạy tốt, nhưng cứ
đến khoảng hai giờ chiều là bắt đầu đếm sai — lúc thiếu một miếng, lúc thừa một miếng. Anh mở phần
mềm vision, kéo ô nhập ngưỡng từ 90 lên 100, nạp lại vài tấm ảnh lưu sẵn để chạy thử: có tấm đúng,
có tấm vẫn sai. Kéo xuống 80, thử tấm khác — lần này đúng tấm vừa rồi, sai một tấm khác. Ba tiếng
đồng hồ, hơn hai mươi lần thử, con số ngưỡng nhảy qua nhảy lại trong khoảng 70–110 mà không có
giá trị nào đúng cho *mọi* tấm ảnh cùng lúc.

Vấn đề không nằm ở con số anh chọn. Nó nằm ở việc anh đang dò một tham số bằng thử-sai thuần tuý,
trong khi hoàn toàn không hiểu con số đó đang làm gì với dữ liệu bên dưới. Nếu biết rằng "ngưỡng"
chỉ đơn giản là ranh giới cắt trên một biểu đồ phân bố mức xám, và biểu đồ đó *tự dịch chuyển* theo
độ sáng thực tế mỗi giờ trong ngày, anh sẽ hiểu ngay: không có một con số cố định nào "đúng mãi
mãi" — cái anh cần là một cơ chế tự thích nghi, không phải một lần thử may mắn.

Chương này dạy đúng phần lý thuyết tối thiểu để không rơi vào tình huống trên: ảnh số là gì và
histogram nói lên điều gì (mục 4.1), vì sao có ngưỡng cố định và ngưỡng tự động, và ngưỡng tự động
"tự chọn" ra sao (mục 4.2), cách dọn nhiễu và sửa hình dạng vùng ảnh trước khi đo (mục 4.3), cơ chế
đứng sau việc "tìm cạnh" của một chi tiết (mục 4.4), và một cái nhìn khái quát về so khớp mẫu bằng
mức xám (mục 4.5). Đây là chương lý thuyết thuần tuý — không một dòng code VisionPro nào xuất hiện
ở đây — nhưng mọi khái niệm học được sẽ quay lại nguyên vẹn dưới một cái tên cụ thể trong Phần III:
threshold Otsu là nền tảng của công cụ Blob (Chương 10), gradient/edge là nền tảng của Caliper
(Chương 9), correlation là điểm khởi đầu để hiểu PMAlign thật sự khác gì (Chương 8).

## 4.1 Ảnh số là gì: ma trận, grayscale 8-bit, histogram

### 4.1.1 Một tấm ảnh chỉ là một bảng số

Hãy nhìn một mảnh nhỏ 4×4 pixel, cắt ra đúng chỗ biên giữa một miếng đệm cao su tối và nền nhôm
sáng xung quanh nó:

| | Cột 1 | Cột 2 | Cột 3 | Cột 4 |
|---|---|---|---|---|
| **Hàng 1** | 58 | 61 | 202 | 205 |
| **Hàng 2** | 60 | 59 | 206 | 208 |
| **Hàng 3** | 57 | 63 | 204 | 207 |
| **Hàng 4** | 61 | 58 | 205 | 209 |

Không có gì huyền bí ở đây: mỗi ô là một số nguyên từ 0 đến 255, mỗi số là độ sáng của đúng một
pixel. Toàn bộ tấm ảnh — dù là 4×4 hay 2448×2048 như camera của trạm MeoVision (đã gặp ở Chương 3)
— chỉ là một **ma trận số nguyên** có kích thước bằng số hàng × số cột của cảm biến. "Xử lý ảnh",
xét đến cùng, là các phép toán trên ma trận đó: so sánh, cộng trừ, lọc theo lân cận, tìm cực trị.

Con số 0–255 (256 mức, biểu diễn bằng 8 bit) gọi là **grayscale 8-bit** — thang độ sáng chuẩn của
phần lớn xử lý ảnh công nghiệp. 0 là đen tuyệt đối, 255 là trắng tuyệt đối, các giá trị ở giữa là
các sắc xám. Như đã bàn ở Chương 3, mục 3.1, phần lớn camera dùng trong sách này là **mono** —
cảm biến chỉ đo cường độ sáng, không đo màu — chính vì phần lớn bài toán đo lường/kiểm tra công
nghiệp chỉ cần biết "sáng bao nhiêu" chứ không cần biết "màu gì". Ma trận grayscale 8-bit chính là
định dạng dữ liệu mà mọi kỹ thuật trong chương này thao tác lên.

> 📌 **Lưu ý:** đừng nhầm "8-bit" (số mức xám mỗi pixel, một khái niệm về *độ sâu màu*) với "độ
> phân giải" (số pixel theo chiều rộng/cao, một khái niệm về *mật độ không gian* đã học ở Chương 3,
> mục 3.2). Một ảnh 2448×2048 pixel, mỗi pixel 8-bit, vẫn chỉ có 256 mức sáng khả dĩ — tăng số
> pixel không làm tăng số mức xám, và ngược lại.

### 4.1.2 Histogram: đếm pixel theo mức xám

**Histogram** là một biểu đồ cực kỳ đơn giản: trục hoành là mức xám (0–255), trục tung là *số
lượng pixel* có đúng mức xám đó trong ảnh (hoặc trong một vùng ảnh được chọn). Với mảnh 4×4 ở trên,
histogram chỉ có hai cụm: 8 pixel nằm quanh mức ~60 (phần cao su tối), 8 pixel nằm quanh mức ~206
(phần nhôm sáng), và **không có pixel nào ở khoảng giữa** — một sự tách biệt rất sạch.

Ảnh thật, chụp trên toàn bộ chi tiết chứ không phải một mảnh 4×4 lý tưởng, hiếm khi sạch tuyệt đối
như vậy: sẽ có vài chục pixel rải rác ở vùng biên do hiệu ứng làm mờ tự nhiên của ống kính, vài
pixel nhiễu ngẫu nhiên của cảm biến. Nhưng ý tưởng cốt lõi không đổi — nếu ánh sáng và tương phản
được kiểm soát tốt (Chương 2), histogram của một chi tiết có hai vùng sáng/tối rõ rệt sẽ có dạng
**hai đỉnh tách biệt** (gọi là *bimodal*), với một "thung lũng" ở giữa gần như trống pixel. Hình
dạng hai đỉnh này chính là điều kiện tiên quyết cho kỹ thuật threshold tự động ở mục 4.2.

Ngược lại, hình dạng tổng thể của histogram còn là một công cụ **chẩn đoán ánh sáng bằng mắt** —
đúng điều đã được nhắc sơ lược ở Chương 2 khi bàn về kiểm soát tương phản, giờ phát biểu tường minh:

**Bảng 4.1 — Đọc hình dạng histogram để chẩn đoán ánh sáng.**

| Hình dạng histogram | Chẩn đoán | Liên hệ Chương 2 |
|---|---|---|
| Dồn hẳn về phía trái (phần lớn pixel ở mức 0–60), gần như trống ở nửa phải | Thiếu sáng (under-exposed) — ảnh tối, chi tiết chìm trong vùng gần đen | Cần tăng cường độ đèn, mở khẩu độ, hoặc tăng thời gian phơi sáng (exposure — Chương 3) |
| Dồn hẳn về phía phải, đặc biệt một cột rất cao ngay sát mức 255 (**clipping/bão hoà**) | Thừa sáng (over-exposed) — vùng sáng nhất bị "cắt phẳng" ở 255, mất hết chi tiết trong vùng đó, không thể khôi phục | Giảm cường độ đèn hoặc exposure; kiểm tra phản xạ chói (Chương 2, mục 2.3) |
| Trải rộng, có hai đỉnh tách biệt rõ với thung lũng gần như trống ở giữa | Tương phản tốt — ánh sáng đã làm đúng việc "tạo tương phản ổn định" (nguyên tắc vàng, Chương 2, mục 2.1) | Đây là trạng thái đích khi tinh chỉnh ánh sáng |
| Một khối liên tục không đỉnh rõ, không thung lũng | Tương phản kém — đối tượng và nền gần cùng độ sáng, hoặc ánh sáng không đồng đều tạo gradient liên tục trên toàn ảnh | Xem lại kỹ thuật chiếu sáng (Chương 2, mục 2.2) |

Phần III của sách, khi học nhóm công cụ hỗ trợ (Chương 12, mục 12.2), sẽ biến việc đọc-bằng-mắt
này thành một con số đo được tự động mỗi cycle, dùng để cảnh báo sớm khi ánh sáng xuống cấp dần
theo thời gian — nhưng nguyên lý đọc hình dạng phân bố mức xám mà chúng ta vừa học ở đây không đổi,
chỉ là từ "nhìn biểu đồ" chuyển sang "so sánh vài con số với một dải chuẩn".

## 4.2 Threshold cố định và threshold tự động (Otsu)

### 4.2.1 Threshold cố định: đơn giản nhưng mong manh

**Threshold** — ngưỡng — là phép toán đơn giản nhất biến ảnh xám thành ảnh nhị phân: chọn một số
`T`, mọi pixel có mức xám lớn hơn `T` thuộc lớp A (ví dụ "nền"), mọi pixel nhỏ hơn hoặc bằng `T`
thuộc lớp B (ví dụ "đối tượng"). Với histogram hai đỉnh ở mục 4.1 (đỉnh ~60 và đỉnh ~206), một
ngưỡng cố định đặt ở giữa thung lũng — ví dụ `T = 130` — tách hoàn hảo hai lớp, và tách *mãi mãi*
với chi phí tính toán gần như bằng không: một phép so sánh cho mỗi pixel.

Vấn đề xuất hiện đúng như câu chuyện mở đầu chương: `T = 130` chỉ đúng **khi đỉnh histogram còn
nằm đúng chỗ nó nằm lúc ta chọn con số đó**. Ánh sáng môi trường thay đổi giữa buổi sáng và buổi
chiều (Chương 2) làm toàn bộ histogram — cả hai đỉnh — trôi lên hoặc trôi xuống vài chục mức xám.
Đỉnh cao su từ ~60 trôi lên ~85, đỉnh nhôm từ ~206 trôi lên ~230; ngưỡng cố định 130 vẫn nằm lọt
trong khoảng trống giữa hai đỉnh mới (85 < 130 < 230) nên kết quả phân đoạn không đổi — dù 130
không còn là điểm chính giữa hai đỉnh mới (trung điểm thực tế đã dịch lên ~157), nó vẫn đứng đúng
phía cần đứng. Nhưng nếu hai đỉnh trôi *không đều nhau* (ánh sáng xiên chiếu
mạnh hơn vào riêng vùng nhôm phản xạ, ít ảnh hưởng vùng cao su hấp thụ), thung lũng dịch chuyển
lệch khỏi 130 — và một ngưỡng cố định không có cách nào tự biết điều đó.

### 4.2.2 Otsu: để dữ liệu tự chọn ngưỡng

Ý tưởng của **threshold tự động** là để chính histogram của từng tấm ảnh quyết định ngưỡng, thay
vì dùng một con số đóng cứng từ trước. Thuật toán phổ biến và được dùng rộng rãi nhất cho việc này
là **Otsu**, đặt theo tên người đề xuất năm 1979. Trực giác của Otsu rất gần với điều mắt người
làm khi nhìn một histogram hai đỉnh: tìm điểm nằm trong "thung lũng" — chỗ tách hai đỉnh rõ nhất.

Otsu hình thức hoá trực giác đó bằng cách thử lần lượt mọi ngưỡng ứng viên `t` từ 0 đến 255. Với
mỗi `t`, chia toàn bộ pixel của ảnh thành hai lớp (≤ t và > t), rồi tính:

- `w0(t)`, `w1(t)`: tỉ lệ số pixel rơi vào lớp 0 và lớp 1
- `μ0(t)`, `μ1(t)`: giá trị trung bình mức xám của từng lớp
- **Phương sai giữa hai lớp** (between-class variance):
  `σ²B(t) = w0(t) · w1(t) · (μ0(t) − μ1(t))²`

Otsu chọn ngưỡng `t*` làm cho `σ²B(t)` **lớn nhất**. Nói bằng lời: ngưỡng tốt nhất là ngưỡng chia
ảnh thành hai lớp *đông đảo tương đương* (không phải một lớp chỉ có vài pixel lẻ tẻ) và *cách xa
nhau nhất về giá trị trung bình* — đúng đặc điểm của một điểm nằm giữa thung lũng của một histogram
hai đỉnh cân đối. (Về mặt toán học, tối đa hoá phương sai *giữa* hai lớp tương đương tối thiểu hoá
phương sai *trong* mỗi lớp, vì tổng hai đại lượng này — tổng phương sai toàn ảnh — là một hằng số
không đổi theo `t`; Otsu vì vậy còn được mô tả là thuật toán "làm mỗi lớp càng đồng nhất nội bộ
càng tốt".)

**Bảng 4.2 — Ví dụ số: phương sai giữa hai lớp tại ba ngưỡng ứng viên** (dựa trên một histogram
gần với ảnh thật: khoảng 50% pixel quanh mức 60, 48% pixel quanh mức 206, và ~2% pixel rải rác
trong vùng chuyển tiếp giữa hai đỉnh — mờ biên, nhiễu cảm biến).

| Ngưỡng ứng viên `t` | `w0`, `μ0` (lớp ≤ t) | `w1`, `μ1` (lớp > t) | `σ²B(t)` xấp xỉ | Nhận xét |
|---|---|---|---|---|
| 65 | 0.50, μ0 ≈ 60 | 0.50, μ1 ≈ 203 | ≈ 0.50·0.50·(143)² ≈ 5 112 | Sát rìa đỉnh trái — toàn bộ pixel chuyển tiếp còn nằm bên lớp 1, kéo μ1 xuống |
| 133 (giữa thung lũng) | 0.51, μ0 ≈ 61 | 0.49, μ1 ≈ 205 | ≈ 0.51·0.49·(144)² ≈ **5 182** | Cao nhất — pixel chuyển tiếp chia đều hai bên, hai lớp "sạch" và cách xa nhau nhất |
| 195 | 0.52, μ0 ≈ 63 | 0.48, μ1 ≈ 206 | ≈ 0.52·0.48·(143)² ≈ 5 104 | Sát rìa đỉnh phải — pixel chuyển tiếp dồn hết sang lớp 0, kéo μ0 lên |

(Để ý `w0` tăng dần theo `t` — đúng bản chất: ngưỡng càng cao, lớp "≤ t" chỉ có thể thêm pixel,
không bao giờ bớt.) Nếu histogram "sạch" tuyệt đối — không một pixel nào nằm giữa hai đỉnh —
`σ²B(t)` sẽ không đổi tại mọi `t` trong khoảng trống đó, vì hai lớp không đổi thành viên. Chính
số pixel rải rác trong vùng chuyển tiếp của ảnh thật làm `σ²B(t)` trở thành một hàm có đỉnh, như
ba con số trên cho thấy: cực đại rơi vào giữa thung lũng (5 182 tại t = 133, cao hơn hai ngưỡng
sát rìa), và Otsu chọn đúng điểm đó — khớp với trực giác nhìn bằng mắt.

![Hình 4.1 — Histogram hai đỉnh và ngưỡng Otsu đánh dấu tại đáy thung lũng](../assets/ch04/hinh_4_1.png)
**Hình 4.1 — Histogram hai đỉnh và ngưỡng Otsu đánh dấu tại đáy thung lũng.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): biểu đồ cột (bar chart) trục hoành 0–255 (mức xám), trục tung
> số lượng pixel. Hai cụm cột rõ rệt, chiều cao xấp xỉ nhau (khớp tỉ lệ ~50%/48% ở Bảng 4.2): một
> cụm ở vùng mức xám thấp quanh ~60 (nhãn "cao su — tối"), một cụm ở vùng mức xám cao quanh ~206
> (nhãn "nhôm — sáng"), khoảng trống ở giữa có vài cột rất thấp lác đác (nhiễu biên).
> Một đường thẳng đứng nét đứt màu đỏ tại vị trí ~130 với nhãn "T* (Otsu)"; hai vùng tô màu nhạt
> khác nhau hai bên đường (lớp 0 / lớp 1) để nhấn trực giác "chia đôi ảnh thành hai lớp".

Otsu là **nền tảng lý thuyết** của một trong những cách phân đoạn ảnh phổ biến nhất trong kiểm tra
công nghiệp. Ở Chương 10, khi học công cụ Blob, chúng ta sẽ gặp lại chính xác ý tưởng này dưới tên
một trong các chế độ phân vùng: ngưỡng "tính hoàn toàn tự động từ phân bố mức xám của ảnh hiện tại"
— không gì khác hơn là việc chạy phép tối đa hoá `σ²B(t)` vừa học, mỗi lần chạy tool trên một ảnh
mới.

> ⚠️ **Cảnh báo:** Otsu ngầm giả định histogram có dạng **hai lớp rõ rệt** (bimodal). Khi giả định
> này bị vi phạm — đối tượng cần phân vùng chỉ chiếm một phần rất nhỏ diện tích ảnh (lớp thiểu số
> "chìm" trong nền, `σ²B(t)` gần như bị chi phối hoàn toàn bởi nền), ảnh có ba vùng độ sáng khác
> nhau trở lên, hoặc ánh sáng không đều tạo một gradient liên tục thay vì hai cụm tách biệt —
> Otsu vẫn trả về **một con số**, nhưng con số đó không còn nằm đúng ranh giới vật lý thật của đối
> tượng. Luôn nhìn lại histogram (mục 4.1) trước khi tin tưởng ngưỡng tự động một cách mù quáng.

## 4.3 Lọc ảnh và hình thái học: dọn ảnh trước khi đo

Trước khi một vùng ảnh có thể được đo diện tích, đếm số lượng hay kiểm hình dạng (chủ đề Blob của
Chương 10), gần như luôn cần một bước "dọn dẹp" trung gian. Bước dọn dẹp này chia làm hai loại
khác nhau về bản chất và về **thời điểm áp dụng**: lọc nhiễu trên ảnh xám — áp dụng **trước** khi
threshold — và hình thái học trên ảnh nhị phân — áp dụng **sau** khi threshold, ngay trước khi đo.

### 4.3.1 Lọc ảnh: smoothing và median

**Smoothing** (làm mượt, hay còn gọi là blur trung bình) thay giá trị mỗi pixel bằng **trung bình
cộng** của các pixel lân cận trong một cửa sổ nhỏ (ví dụ 3×3). Đây là cách đơn giản nhất để giảm
nhiễu ngẫu nhiên — nhiễu ngẫu nhiên có xu hướng triệt tiêu lẫn nhau khi lấy trung bình. Cái giá
phải trả: trung bình cộng cũng làm mờ chính những biên thật mà ta cần giữ sắc nét cho mục 4.4.

**Median filter** (lọc trung vị) làm việc khác hẳn: thay vì lấy trung bình, nó sắp xếp các giá trị
trong cửa sổ lân cận và chọn giá trị **ở giữa** (trung vị). Sự khác biệt này quan trọng khi nhiễu
là dạng **xung đột biến** (impulse noise, dân gian gọi "muối tiêu" — một vài pixel lẻ tẻ có giá trị
cực đoan, do hạt bụi trên ống kính hay lỗi cảm biến), ví dụ dãy pixel liền kề sau:

| Vị trí pixel | 1 | 2 | 3 (nhiễu) | 4 | 5 |
|---|---|---|---|---|---|
| Giá trị gốc | 60 | 61 | **255** | 59 | 62 |
| Sau smoothing (trung bình cửa sổ 3, tâm tại vị trí 3) | — | — | (61+255+59)/3 ≈ **125** | — | — |
| Sau median (trung vị cửa sổ 3, tâm tại vị trí 3) | — | — | median(61, 255, 59) = **61** | — | — |

Smoothing kéo giá trị trung tâm lên ~125 — vẫn là một điểm bất thường so với hai bên (~60), chỉ
bớt cực đoan hơn 255 chứ không biến mất. Median loại bỏ hoàn toàn giá trị 255 khỏi kết quả — vì nó
là giá trị ngoại lai, không phải giá trị "ở giữa" — và trả lại đúng 61, khớp với xu hướng thật của
vùng xung quanh. Đây là lý do median filter là lựa chọn mặc định khi nhiễu có dạng đốm/xung rời rạc,
còn smoothing phù hợp hơn cho nhiễu dạng "rung" đều khắp ảnh (nhiễu nhiệt cảm biến).

> 💡 **Mẹo thực chiến:** thứ tự áp dụng có ý nghĩa. Lọc nhiễu (mục này) làm việc trên ảnh xám và
> nên chạy **trước** threshold — dọn nhiễu càng sớm, threshold càng ổn định. Hình thái học (mục
> 4.3.2 dưới đây) làm việc trên ảnh nhị phân và chỉ có ý nghĩa **sau** threshold. Đảo ngược thứ tự
> này (thử lọc nhiễu sau khi đã nhị phân hoá) thường cho kết quả khó đoán. (Hình thái học áp dụng
> trực tiếp trên ảnh xám — *grayscale morphology* — cũng là một kỹ thuật chuẩn trong xử lý ảnh,
> nhưng nằm ngoài phạm vi sách này; ở đây chỉ dùng morphology trên ảnh nhị phân, sau threshold.)

### 4.3.2 Hình thái học: erosion, dilation, open, close

Sau threshold, ảnh chỉ còn hai giá trị: foreground (đối tượng) và background (nền). **Hình thái
học** (morphology) là nhóm bốn phép toán cơ bản thao tác trên ảnh nhị phân này, dùng để sửa hình
dạng vùng foreground trước khi đo đạc — loại bỏ các mảnh vụn nhỏ, hàn lại các vùng bị đứt gãy do
nhiễu cục bộ, mà không cần động đến giá trị mức xám gốc nữa.

Cũng như bộ lọc ở mục 4.3.1 cần một "cửa sổ" để biết lân cận là những pixel nào, mỗi phép toán
hình thái học cần một **structuring element** (phần tử cấu trúc, còn gọi là kernel) — một hình
dạng nhỏ (phổ biến: hình vuông hoặc hình tròn 3×3, 5×5...) định nghĩa "lân cận" được xét quanh mỗi
pixel khi kiểm tra điều kiện erosion/dilation. Kernel càng lớn, hiệu ứng ăn mòn/giãn nở càng mạnh
trong một lần chạy — cùng một phép Open, kernel 3×3 chỉ xoá tua nhiễu 1 pixel, còn kernel 7×7 có
thể xoá luôn cả những chi tiết thật nhỏ hơn 7 pixel. Đây là tham số cụ thể cần chọn khi cấu hình
công cụ hình thái học trong phần mềm vision, không chỉ là một khái niệm lý thuyết.

**Bảng 4.3 — Bốn phép toán hình thái học cơ bản.**

| Phép toán | Cơ chế trực giác | Hiệu ứng |
|---|---|---|
| **Erosion** (ăn mòn) | Một pixel foreground chỉ được *giữ lại* nếu toàn bộ lân cận xung quanh nó cũng là foreground | Thu nhỏ vùng, ăn mòn biên; có thể **tách** hai vùng dính nhau qua một cầu nối mảnh |
| **Dilation** (giãn nở) | Một pixel background *trở thành* foreground nếu có ít nhất một lân cận là foreground | Phình to vùng, giãn nở biên; có thể **nối liền** các vùng nằm gần nhau |
| **Open** (mở, = Erosion rồi Dilation) | Ăn mòn trước để "cắt đứt" các tua/gai/hạt nhiễu nhỏ, rồi giãn nở lại để phục hồi kích thước phần còn lại | Loại bỏ tua nhỏ nhô ra hoặc đốm nhiễu rời rạc, **gần như giữ nguyên kích thước** của vùng chính đủ lớn |
| **Close** (đóng, = Dilation rồi Erosion) | Giãn nở trước để "lấp" các khe/lỗ nhỏ hoặc bắc cầu qua chỗ đứt gãy, rồi ăn mòn lại để phục hồi kích thước | Lấp khe/lỗ nhỏ bên trong vùng hoặc nối các mảnh gần nhau bị đứt đoạn, **gần như giữ nguyên kích thước** |

Điểm mấu chốt để chọn đúng phép toán: **Open** khi vấn đề là nhiễu — các đốm nhỏ *thừa* lẫn vào
nền hoặc bám vào biên vùng thật; **Close** khi vấn đề là đứt gãy — một vùng *đúng là một khối* về
mặt vật lý nhưng bị segmentation cắt rời thành nhiều mảnh do phản xạ cục bộ hay nhiễu. Hai phép
toán đơn lẻ Erosion/Dilation làm thay đổi kích thước thật của vùng, nên hiếm khi dùng một mình khi
mục tiêu cuối là đo diện tích — Open và Close mới là cặp "dọn dẹp mà không làm sai lệch kích thước"
dùng phổ biến nhất.

![Hình 4.2 — Bốn phép toán hình thái học trên một hình mẫu có gai nhiễu và một hình mẫu bị đứt gãy](../assets/ch04/hinh_4_2.png)
**Hình 4.2 — Bốn phép toán hình thái học trên một hình mẫu có gai nhiễu và một hình mẫu bị đứt gãy.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): lưới 2 hàng × 5 cột hình nhị phân đơn giản (đen/trắng), cỡ mỗi
> ô ~15×15 pixel vẽ phóng to dạng lưới ô vuông. Hàng 1 (chủ đề "loại nhiễu"): (a) hình vuông đặc có
> một "tua" nhỏ 1-2 pixel nhô ra một cạnh — nhãn "Gốc"; (b) cùng hình sau Erosion — nhãn "Erosion:
> tua biến mất, hình co lại"; (c) sau Dilation tiếp theo (= Open hoàn chỉnh) — nhãn "Open: tua mất
> hẳn, kích thước khôi phục". Hàng 2 (chủ đề "hàn đứt gãy"): (d) hình vuông bị một khe trắng mảnh
> 1 pixel cắt ngang làm đôi — nhãn "Gốc"; (e) sau Dilation — nhãn "Dilation: khe lấp, hình phình
> to"; (f) sau Erosion tiếp theo (= Close hoàn chỉnh) — nhãn "Close: khe đã hàn, kích thước khôi
> phục". Vẽ bằng công cụ diagram đơn giản (không cần chụp từ phần mềm cụ thể nào).

> ⚠️ **Cảnh báo:** hình thái học áp dụng đều cho **toàn bộ** ảnh nhị phân, kể cả những vùng vốn dĩ
> đã đúng hình dạng, không cần sửa gì. Một phép Close quá mạnh (cửa sổ lớn, hoặc lặp lại nhiều
> lần) có thể vô tình **nối liền hai đối tượng riêng biệt** đặt gần nhau thành một vùng duy nhất —
> biến một lỗi "thiếu một đối tượng, thừa diện tích ở đối tượng bên cạnh" thành kết quả đếm "đủ số
> lượng" giả mạo. Luôn kiểm tra hình thái học bằng ảnh có chủ đích đặt hai đối tượng ở khoảng cách
> tối thiểu cho phép, không chỉ bằng ảnh "đẹp" có khoảng cách rộng rãi.

## 4.4 Gradient và edge

### 4.4.1 Cạnh là nơi độ sáng thay đổi nhanh nhất

Trực giác hàng ngày gọi "cạnh" (edge) của một vật thể là đường viền nhìn thấy được của nó. Về mặt
số học trên ma trận ảnh, cạnh là nơi **giá trị pixel thay đổi nhanh** khi di chuyển qua nó — nhanh
hơn hẳn so với mức dao động ngẫu nhiên (nhiễu) ở những vùng phẳng. Hãy xét một dãy 8 pixel liên
tiếp quét ngang qua một cạnh tối-sang-sáng:

**Bảng 4.4 — Ví dụ số: cường độ pixel và gradient rời rạc qua một cạnh.**

| Vị trí pixel | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
|---|---|---|---|---|---|---|---|---|
| Cường độ `I` | 50 | 52 | 49 | 130 | 208 | 210 | 207 | 209 |
| Gradient `g[i] = I[i+1] − I[i−1]` | — | −1 | 78 | 159 | 80 | −1 | −1 | — |

Ở các vị trí 2, 6, 7 — nằm trong vùng phẳng (toàn tối hoặc toàn sáng) — gradient gần như bằng
không: chỉ dao động ±1, mức nhiễu bình thường. Ở vị trí 4, gradient nhảy vọt lên 159 — lớn hơn hẳn
mọi giá trị khác trong dãy. Đó chính là **vị trí của cạnh**: không phải nơi cường độ tuyệt đối bằng
một con số cụ thể nào, mà là nơi **độ lớn của gradient đạt cực đại cục bộ**.

`g[i] = I[i+1] − I[i−1]` là dạng đơn giản nhất của phép tính gradient — một **sai phân rời rạc**,
xấp xỉ đạo hàm của hàm cường độ theo không gian. Trên ảnh 2 chiều thật, gradient được tính theo cả
hai trục X và Y bằng một cửa sổ nhỏ (kernel) quét qua từng vị trí — về bản chất vẫn là "một phép
cộng-trừ có trọng số trên các pixel lân cận", chỉ tinh vi hơn phép sai phân 1 chiều ở trên. Kết quả
là hai giá trị `Gx`, `Gy` tại mỗi pixel, tạo thành một **vector gradient**: độ lớn
`√(Gx² + Gy²)` cho biết cạnh ở đó "rõ" (tương phản cao) hay "mờ" (tương phản thấp), còn hướng của
vector luôn **vuông góc** với hướng của đường cạnh tại điểm đó.

> 🔍 **Đào sâu thêm:** các thuật toán phát hiện cạnh kinh điển trong xử lý ảnh (kernel Sobel,
> Prewitt, hay thuật toán nhiều bước như Canny) đều dựa trên đúng ý tưởng "tính gradient rồi tìm
> cực trị" vừa trình bày — khác nhau chủ yếu ở cách làm mượt nhiễu trước khi tính gradient (mục
> 4.3.1) và cách xác định cực trị chính xác đến đâu. Sách này không đi sâu vào từng biến thể; nắm
> được trực giác "cạnh = đỉnh của đạo hàm" là đủ để hiểu mọi công cụ đo cạnh ở Phần III.

### 4.4.2 Polarity và độ chính xác dưới-pixel

Gradient còn mang **dấu**: trong ví dụ trên, gradient dương (78, 159, 80) tương ứng với chiều
chuyển từ tối sang sáng; nếu quét ngược chiều, dấu sẽ đảo ngược. Dấu này gọi là **polarity** của
cạnh — tối-sang-sáng hay sáng-sang-tối — và là một tham số cần khai báo tường minh khi một công cụ
đo cạnh thực tế được cấu hình, đặc biệt quan trọng khi trên đường quét có nhiều cạnh song song
(ví dụ một cạnh thật cạnh một bóng đổ) mà chỉ một trong số đó là cạnh cần đo.

Một điểm tinh tế cuối cùng: vì gradient là một hàm số có thể tính tại nhiều điểm rời rạc quanh vị
trí cực đại, người ta có thể **khớp một đường cong** (ví dụ parabol) qua vài điểm gradient gần đỉnh
nhất để nội suy ra vị trí cực đại *chính xác hơn* một pixel nguyên — kỹ thuật gọi là "cạnh dưới-
pixel" (sub-pixel edge). Đây là lý do các phép đo dựa trên cạnh có thể đạt độ chính xác nhỏ hơn
kích thước một pixel thật, một yêu cầu bắt buộc khi dung sai đo lường (Chương 7) nhỏ hơn tỉ lệ
mm/pixel của hệ thống.

Chương 9 sẽ học công cụ Caliper — công cụ đo khoảng cách giữa hai cạnh bằng cách quét dọc theo một
dải hẹp, tính gradient theo phương vuông góc với dải đó, và tìm đúng các cực trị vừa mô tả ở trên.
"Đo khoảng cách hai cạnh", xét đến cùng, chính là "đo khoảng cách giữa hai cực trị đạo hàm" — không
có gì khác về bản chất so với việc tìm vị trí 4 trong Bảng 4.4.

![Hình 4.3 — Cường độ pixel và độ lớn gradient qua một cạnh, cực trị gradient đánh dấu vị trí cạnh](../assets/ch04/hinh_4_3.png)
**Hình 4.3 — Cường độ pixel và độ lớn gradient qua một cạnh, cực trị gradient đánh dấu vị trí cạnh.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): hai biểu đồ đường xếp chồng theo cùng trục hoành (vị trí
> pixel 1-8, khớp với Bảng 4.4). Biểu đồ trên: đường cường độ `I` — dốc bậc thang từ ~50 lên ~208,
> điểm chuyển tại vị trí 4. Biểu đồ dưới: đường độ lớn gradient `|g|` — gần như phẳng sát trục 0 ở
> hai đầu, một đỉnh nhọn rõ rệt tại vị trí 4 (giá trị 159), nhãn mũi tên trỏ vào đỉnh ghi "cực đại
> gradient = vị trí cạnh". Hai biểu đồ dùng cùng màu nền để người đọc dễ đối chiếu theo chiều dọc.

## 4.5 [NÂNG CAO] Correlation và khớp mẫu

Trước khi có các kỹ thuật dựa trên đặc trưng hình học mà Chương 8 sẽ giới thiệu, cách "cổ điển" để
tìm một mẫu (template) nhỏ bên trong một ảnh lớn hơn là **correlation** (tương quan). Ý tưởng đơn
giản: trượt mẫu qua từng vị trí có thể có trên ảnh lớn, tại mỗi vị trí tính một con số đo "độ giống
nhau" giữa mức xám của mẫu và mức xám của vùng ảnh tương ứng ngay dưới nó, rồi chọn vị trí có con
số đó cao nhất.

Con số đo độ giống nhau phổ biến nhất là **tương quan chéo chuẩn hoá** (normalized cross-
correlation): về trực giác, đây là một phép "nhân từng cặp pixel tương ứng rồi cộng lại", sau đó
chuẩn hoá theo độ tương phản của cả mẫu và vùng ảnh để kết quả luôn nằm trong khoảng −1 (ngược hẳn
nhau) đến 1 (khớp tuyệt đối). Việc chuẩn hoá này giúp correlation bớt nhạy với thay đổi độ sáng
*tuyến tính* toàn cục (ảnh sáng hơn hay tối hơn đều một mức), nhưng bản chất phép so khớp vẫn là so
sánh **mức xám từng pixel một** giữa mẫu và ảnh.

Chính vì so khớp ở mức pixel-đối-pixel, correlation cổ điển có ba điểm yếu cố hữu:

- **Nhạy với xoay và co giãn.** Mẫu train ở một góc, chi tiết trên băng tải xoay lệch vài độ —
  từng pixel không còn thẳng hàng với mẫu nữa, điểm correlation tụt nhanh dù hình dạng thực chất
  không đổi.
- **Nhạy với thay đổi tương phản cục bộ và nhiễu.** Chuẩn hoá chỉ xử lý được thay đổi độ sáng
  *đều* trên toàn vùng; bóng đổ cục bộ, phản xạ lệch, hay nhiễu cảm biến vẫn làm lệch từng pixel so
  với mẫu.
- **Chi phí tính toán tăng nhanh** khi cần chịu được xoay và co giãn: mỗi góc xoay, mỗi tỉ lệ scale
  cần thử là một lần trượt riêng biệt qua toàn ảnh — không gian tìm kiếm phình to theo cấp số nhân
  nếu muốn "chắc ăn" với mọi biến thể có thể có.

Ba điểm yếu này chính là lý do các công cụ định vị mẫu công nghiệp hiện đại không dừng lại ở
correlation mức xám thuần tuý. Chương 8 sẽ học công cụ PMAlign, dựa trên công nghệ trích **đặc
trưng hình học** (biên, góc, đường cong) thay vì so khớp trực tiếp mức xám từng pixel — nhờ vậy
chịu được xoay, co giãn, thay đổi ánh sáng và một phần che khuất tốt hơn hẳn correlation cổ điển.
Nhưng trực giác "trượt qua ứng viên, tính điểm số cho mỗi vị trí, chọn vị trí điểm cao nhất" mà
chúng ta vừa học ở đây không mất đi — nó vẫn là bộ khung chung của mọi bài toán so khớp mẫu, chỉ
khác nhau ở việc *cái gì* được so khớp: mức xám thô, hay một tập đặc trưng đã được trích xuất.

> 🔍 **Đào sâu thêm:** correlation mức xám vẫn còn chỗ đứng trong công nghiệp — cho các bài toán mẫu
> không xoay/không co giãn đáng kể, tương phản ổn định, cần tốc độ rất cao và không đòi hỏi độ chắc
> chắn cao trước biến dạng. Phần lớn bài toán định vị chi tiết cơ khí thực tế trong sách này, như
> sẽ thấy ở Chương 8, rơi vào nhóm cần độ chắc chắn cao hơn những gì correlation thuần tuý cung cấp.

## Tổng kết chương

- Ảnh số là một ma trận số nguyên 0–255 (grayscale 8-bit); histogram đếm số pixel theo từng mức
  xám và là công cụ chẩn đoán ánh sáng bằng mắt — hình dạng lệch trái/lệch phải/hai đỉnh tách biệt
  tương ứng thiếu sáng/thừa sáng/tương phản tốt (mục 4.1).
- Threshold cố định đơn giản và nhanh nhưng mong manh trước ánh sáng trôi; Otsu tự động tìm ngưỡng
  bằng cách tối đa hoá phương sai giữa hai lớp pixel — hoạt động tốt khi histogram thật sự có hai
  đỉnh tách biệt, sai lệch khi giả định đó bị vi phạm (mục 4.2).
- Lọc nhiễu (smoothing/median) chạy trên ảnh xám, trước threshold; hình thái học (Erosion/Dilation/
  Open/Close) chạy trên ảnh nhị phân, sau threshold — Open loại nhiễu/tua thừa, Close hàn lại vùng
  bị đứt gãy, cả hai giữ nguyên kích thước tổng thể vùng chính (mục 4.3).
- Cạnh là nơi độ lớn gradient (đạo hàm rời rạc của cường độ theo không gian) đạt cực đại cục bộ,
  không phải nơi cường độ tuyệt đối bằng một số cụ thể; polarity cho biết chiều chuyển sáng-tối,
  nội suy quanh cực trị cho độ chính xác dưới-pixel (mục 4.4).
- Correlation mức xám so khớp mẫu bằng cách trượt và tính độ tương quan pixel-đối-pixel — đơn giản
  nhưng nhạy xoay/co giãn/nhiễu; các công cụ định vị hiện đại dùng đặc trưng hình học thay vì mức
  xám thô để khắc phục, nhưng vẫn giữ nguyên bộ khung "trượt — tính điểm — chọn cực đại" (mục 4.5).
- Bốn khái niệm CORE của chương này — histogram, threshold/Otsu, morphology, gradient/edge — không
  phải lý thuyết trừu tượng: chúng là chính cơ chế bên trong các công cụ mà Phần III của sách sẽ
  dạy cách dùng, dưới những cái tên cụ thể (Blob, Caliper, PMAlign, công cụ histogram).

## Lỗi thường gặp

**Lỗi 1 — Lạm dụng filter thay vì sửa ánh sáng.** Hiện tượng: thêm hết lớp lọc này đến lớp lọc
khác (smoothing mạnh, morphology nhiều lần) mà kết quả vẫn không ổn định, hoặc "ổn định" được
nhưng chi tiết thật cũng bị mờ/biến dạng theo. Nguyên nhân: filter và morphology xử lý phần *ngọn*
— dữ liệu đầu vào (ảnh) đã mang sẵn vấn đề ánh sáng/tương phản chưa được giải quyết tận gốc
(Chương 2). Không có tổ hợp xử lý ảnh phần mềm nào tạo ra tương phản mà bản thân tấm ảnh gốc không
có. Cách tránh: luôn ưu tiên sửa ánh sáng vật lý trước; coi xử lý ảnh trong chương này là bước tinh
chỉnh cuối cùng trên một tấm ảnh đã đủ tốt, không phải một miếng vá cho ánh sáng tồi.

**Lỗi 2 — Tin Otsu một cách mù quáng khi histogram không thật sự hai đỉnh.** Hiện tượng: ngưỡng tự
động ra một con số nằm sai chỗ, cắt vào giữa đối tượng thay vì đúng biên của nó. Nguyên nhân: Otsu
giả định phân bố hai lớp rõ rệt (mục 4.2); đối tượng chiếm diện tích quá nhỏ so với nền, hoặc ảnh
có từ ba vùng độ sáng khác biệt trở lên, phá vỡ giả định đó. Cách tránh: luôn nhìn lại hình dạng
histogram (mục 4.1) trước khi tin ngưỡng tự động; với ảnh có nhiều vùng độ sáng, cân nhắc threshold
theo vùng con thay vì toàn ảnh.

**Lỗi 3 — Dùng Erosion/Dilation đơn lẻ khi mục tiêu là đo diện tích chính xác.** Hiện tượng: kích
thước đo được lệch có hệ thống (luôn nhỏ hơn hoặc luôn lớn hơn thực tế) sau khi thêm bước "dọn
nhiễu" hình thái học. Nguyên nhân: Erosion và Dilation đơn lẻ thay đổi kích thước thật của vùng —
chỉ Open và Close (cặp đôi) mới giữ nguyên kích thước tổng thể (mục 4.3.2). Cách tránh: nếu mục
đích là dọn nhiễu mà không làm sai lệch số đo, luôn dùng Open hoặc Close, không dùng Erosion/
Dilation một mình; nếu cần độ chính xác cao hơn nữa, đo bằng gradient/edge trực tiếp (mục 4.4) thay
vì diện tích sau xử lý hình thái học mạnh.

**Lỗi 4 — Nhầm đốm nhiễu với cạnh thật.** Hiện tượng: phát hiện "cạnh" tại những vị trí không có
biên vật lý nào, thường lẻ tẻ và không lặp lại giữa các lần chụp. Nguyên nhân: gradient nhạy với
mọi thay đổi cường độ đột ngột, bất kể nguồn gốc — một hạt bụi hay nhiễu cảm biến cũng tạo ra biến
thiên cục bộ đủ lớn để bị tính là cực trị gradient. Cách tránh: làm mượt nhẹ (mục 4.3.1) trước khi
tính gradient để loại bớt biến thiên tần số cao của nhiễu, đồng thời đặt một ngưỡng độ lớn gradient
tối thiểu — cực trị yếu hơn ngưỡng đó bị bỏ qua, không được công nhận là cạnh.

\newpage

# Chương 5 — Làm quen VisionPro và QuickBuild

Bốn chương đầu của sách bàn về ánh sáng, ống kính, camera, và những khái niệm xử lý ảnh nền tảng
— không một dòng phần mềm nào. Từ chương này, chúng ta mở phần mềm lên lần đầu tiên. Và điều đầu
tiên đáng nói về VisionPro không phải một tính năng cụ thể, mà là **cách nó được thiết kế**: đây
không phải một thư viện hàm rời rạc để gọi tuần tự như code thông thường — nó là một môi trường
**data-flow**, nơi công việc được biểu diễn bằng cách nối các khối chức năng (tool) vào nhau, dữ
liệu chảy qua các đường nối đó, và toàn bộ cấu trúc được lưu lại thành một file duy nhất. Hiểu
đúng triết lý này từ đầu giúp mọi chương sau — từ PMAlign đến tích hợp C# — trở nên tự nhiên hơn
nhiều so với cố áp tư duy lập trình tuần tự quen thuộc vào một công cụ không được thiết kế theo
cách đó.

Chương này đưa chúng ta từ cài đặt (mục 5.1), qua triết lý nối-dây-dữ-liệu cốt lõi (5.2), chạy
job đầu tiên trong QuickBuild (5.3), đến `CogToolBlock` — đơn vị đóng gói quan trọng nhất, thứ sẽ
xuất hiện lại xuyên suốt Phần III và là cầu nối trực tiếp sang C# ở Chương 13 (5.4) — và công cụ
hiển thị/debug sẽ dùng trong mọi chương còn lại của sách (5.5).

## 5.1 Cài đặt, license, cấu trúc thư mục SDK

### 5.1.1 Phiên bản chuẩn của sách

Sách này dùng **VisionPro 9.0 CR2 x64**, chạy trên **.NET Framework 4.8**. Bộ cài đặt bao gồm cả
thành phần 32-bit và 64-bit song song (vì một số thiết bị/driver phần cứng cũ chỉ có bản 32-bit) —
với ứng dụng phát triển mới, luôn nhắm 64-bit nhất quán (lý do kỹ thuật đầy đủ ở Chương 13, mục
13.1). License có hai hình thức: **dongle** (khoá cứng USB, cắm vật lý vào máy) hoặc **soft
license** (mã kích hoạt gắn với phần cứng máy, không cần khoá vật lý) — chọn hình thức nào tuỳ
thoả thuận mua với Cognex, không phải lựa chọn kỹ thuật của người triển khai. License cần thiết để
**chạy** tool, không chỉ để mở QuickBuild thiết kế — máy dev (thiết kế job) và máy sản xuất (chạy
job đã hoàn thiện) thường dùng loại license khác nhau về mục đích sử dụng, đây là việc cần xác
nhận trước khi triển khai, không phải phát hiện lúc mang máy ra dây chuyền.

> 🔍 **Đào sâu thêm (đã kiểm chứng lại trên SDK thật, VisionPro 9.0 CR2):** ".NET Framework 4.8"
> ở trên là lựa chọn của sách cho mã ví dụ (và cũng là lựa chọn phổ biến nhất trong các dự án
> VisionPro thật khảo sát được), không phải con số Cognex công bố riêng cho đúng bản 9.0 CR2. Bằng
> chứng trực tiếp trên máy cài SDK: phản chiếu (reflection) chính `Cognex.VisionPro.dll` (assembly
> version `59.2.0.0`) cho thấy `TargetFrameworkAttribute` ghi `.NETFramework, Version=v4.0` — DLL
> này biên dịch nhắm nền tối thiểu .NET Framework 4.0. File `VisionProQuickReference.pdf` đóng gói
> ngay trong thư mục cài đặt (`Doc\en\`) xác nhận: "Microsoft .NET 4.0, .NET 4.5, or .NET 4.5.1
> Frameworks" — dù (khá lạ) trang bìa tài liệu này lại tự ghi là "**VisionPro 8.4** Quick
> Reference", không phải 9.0 — có thể Cognex chưa cập nhật lại tài liệu PC-requirements này qua
> nhiều bản lớn. Vì .NET Framework 4.0–4.8 nâng cấp tại chỗ (in-place, chung runtime
> `v4.0.30319`), DLL biên dịch cho 4.0 vẫn chạy tốt trên máy có 4.8 — **đã kiểm chứng bằng runtime
> thật, không chỉ lý luận**: dựng trực tiếp một `CogImage8Grey` từ `Bitmap` thật, roundtrip qua
> `ToBitmap()`, và một `CogToolBlock` rỗng đọc `Inputs`/`Outputs` — cả hai chạy không lỗi dưới
> .NET Framework 4.8.1 (bản cài thật trên máy, xác nhận qua registry `NDP\v4\Full`, mã
> `Release=533509`). Vậy nên chọn 4.8 cho ứng dụng MỚI là
> an toàn và đúng thực tế (bản Framework cuối cùng, luôn có sẵn trên Windows 10/11), chỉ không phải
> "yêu cầu bắt buộc" riêng của bản 9.0 CR2. Với các bản 9.x MỚI HƠN, yêu cầu chính thức của Cognex
> thay đổi rõ theo thời gian (đã đối chiếu trực tiếp tài liệu Quick Reference chính thức tương
> ứng): **9.10** (phát hành 02/2023) vẫn ghi .NET Framework 4.7.2; từ **9.21** (phát hành 02/2024)
> trở đi ghi rõ .NET Framework 4.8 + biên dịch bằng VS2015 Update 3/VS2017. Nếu sau này nâng cấp
> sang dòng **VisionPro Core** (thế hệ mới, từ bản 25.1.0) — dòng này chuyển hẳn sang **.NET 8.0**
> (không còn .NET Framework), giữ nguyên API 2D rule-based/QuickBuild/ToolBlock nhưng KHÔNG mở
> được file `.vpp` chứa tool Edge Learning tạo từ dòng 9.x cũ và ngược lại — cần biết trước khi
> quyết định nâng cấp SDK cho một dự án đang chạy.

### 5.1.2 Năm nơi cần biết trong thư mục cài đặt

Thư mục cài đặt (`C:\Program Files\Cognex\VisionPro\`) chứa nhiều hơn một bộ SDK để lập trình —
năm thư mục con đáng nhớ vị trí ngay từ đầu, vì sách sẽ tham chiếu lại chúng nhiều lần:

**Bảng 5.1 — Năm thư mục quan trọng trong bộ cài VisionPro.**

| Thư mục | Chứa gì | Dùng khi |
|---|---|---|
| `ReferencedAssemblies\` | Toàn bộ DLL để reference vào project C# | Chương 13, mục 13.1 |
| `Doc\en\` | Tài liệu tham khảo chính thức (CHM tổng hợp, PDF chuyên đề) | Tra cứu chi tiết một API/khái niệm |
| `samples\Programming\` | Code mẫu C#/VB chính thức theo từng chủ đề tool | Đối chiếu cách dùng một tool trước khi viết code thật |
| `samples\QuickBuild\` | File `.vpp` mẫu, mở trực tiếp bằng QuickBuild | Học nhanh một kỹ thuật qua job đã dựng sẵn |
| `Images\` | Bộ ảnh mẫu đi kèm SDK | Phát triển/học tool không cần camera thật (Chương 6, mục 6.4) |

## 5.2 Triết lý VisionPro: tool → terminal → nối dây

### 5.2.1 Đơn vị cơ bản: tool và terminal

Mọi thao tác trong VisionPro — định vị, đo lường, đọc mã (Phần III của sách) — đều đóng gói trong
một **tool**. Mỗi tool có **terminal đầu vào** (input) nhận dữ liệu (thường là ảnh, đôi khi là kết
quả từ tool khác) và **terminal đầu ra** (output) trả kết quả. QuickBuild là môi trường thiết kế
trực quan nơi công việc chính là: thêm tool vào job, **nối dây** terminal đầu ra của tool này vào
terminal đầu vào của tool kia, tạo thành một chuỗi xử lý hoàn chỉnh.

![Hình 5.1 — Cửa sổ chính QuickBuild: canvas nối dây tool, bảng tool có sẵn, terminal](../assets/ch05/hinh_5_1.png)
**Hình 5.1 — Cửa sổ chính QuickBuild: canvas nối dây tool, bảng tool có sẵn, terminal.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): screenshot toàn cảnh cửa sổ QuickBuild với một job đơn giản
> đang mở (2-3 tool nối dây, ví dụ Acquisition → một tool bất kỳ). Đánh số/khoanh vùng 4 khu vực
> chính bằng khung màu đỏ + nhãn: (1) canvas trung tâm hiển thị các tool dạng khối và đường nối
> dây giữa terminal đầu ra/đầu vào; (2) bảng danh sách tool có sẵn (bên trái hoặc phải tuỳ theme);
> (3) khu vực xem ảnh/kết quả (nơi CogRecordDisplay tương đương hiện diện trong QuickBuild);
> (4) thanh thuộc tính/tham số của tool đang chọn. Dùng job mẫu bất kỳ trong sdk/samples_quickbuild.

Tư duy này khác căn bản so với viết một hàm C# gọi tuần tự các hàm con — ở đây, cấu trúc chương
trình **là** sơ đồ nối dây, nhìn thấy được toàn bộ trên một canvas, không ẩn trong logic gọi hàm
lồng nhau. Sách sẽ quay lại nguyên tắc này nhiều lần: kiến trúc job chuẩn Acquisition → Calibration
→ Align/Fixture → Inspect (Chương 7, mục 7.4) chính là một chuỗi nối dây theo đúng tinh thần này.

### 5.2.2 File .vpp: toàn bộ job trong một file

Toàn bộ cấu trúc — tool, cách nối dây, tham số từng tool — được lưu trong một file `.vpp`. Đây là
file **nhị phân serialize** (không phải văn bản/XML đọc được bằng text editor), nghĩa là công cụ
kiểm soát phiên bản văn bản thông thường (diff dòng-theo-dòng như với code C#) không áp dụng được
trực tiếp — một chi tiết cần tính đến khi đưa file `.vpp` vào quy trình quản lý phiên bản của dự
án (mục 5.6 bàn kỷ luật backup thay thế). Từ code C#, file này được nạp lại bằng `CogSerializer`
— chi tiết đầy đủ ở Chương 13, mục 13.2.

## 5.3 Chạy job đầu tiên trong QuickBuild

### 5.3.1 CogJob và CogJobManager — bộ máy chạy tự động của QuickBuild

Khi mở QuickBuild và tạo một job mới, đối tượng quản lý toàn bộ vòng đời chạy — thu ảnh, chạy
tool, đếm throughput — là `CogJob`, được điều phối bởi `CogJobManager` (quản lý một hoặc nhiều
`CogJob` cùng lúc, có thể cấu hình đa luồng). Đây là lớp vỏ **tự động hoá** bên trên tool/ToolBlock:
`CogJob` tự lặp vòng thu ảnh → chạy tool hoặc ToolBlock đang gán cho nó → đếm thống kê throughput,
theo chế độ chạy (liên tục, theo trigger, v.v.) đã cấu hình. (Tên property/method chính xác — ví
dụ cách bật đa luồng, cách đọc số liệu accept/reject — nên đối chiếu trực tiếp trong `Doc\en\` hoặc
IntelliSense khi viết code thật ở Chương 13; SDK có thể đặt tên khác đôi chút giữa các phiên bản.)

> 📌 **Lưu ý:** `CogJob`/`CogJobManager` là bộ máy chạy **của QuickBuild** — hữu ích để thử nghiệm,
> quan sát throughput, debug trực quan trong lúc thiết kế. Khi chuyển sang ứng dụng C# sản xuất
> (Chương 13, Chương 14), phần lớn thời gian chúng ta làm việc trực tiếp với `CogToolBlock` (mục
> 5.4) thay vì `CogJob` — ứng dụng C# tự viết vòng lặp trigger/xử lý riêng theo kiến trúc của nó
> (Chương 13, mục 13.7), không cần đến bộ máy tự động của QuickBuild.

### 5.3.2 Ảnh tĩnh từ file — bắt đầu không cần camera

Bài học đầu tiên nên làm trước khi bất kỳ camera nào được cắm vào: cấu hình **image source** của
job trỏ đến một file ảnh tĩnh (dùng bộ ảnh mẫu trong `Images\`, mục 5.1.2) thay vì camera thật.
Bấm nút **Run Once** (chạy một lần) trên thanh công cụ QuickBuild để kích hoạt job chạy qua ảnh
tĩnh đó — hoặc **Run Continuous** (chạy liên tục) nếu muốn job tự lặp lại. Xem ảnh hiện ra trên
canvas QuickBuild — chưa cần tool nào xử lý gì cả, chỉ cần xác nhận: cài đặt đúng, license hợp lệ,
và luồng ảnh → hiển thị hoạt động. Kỹ thuật
phát triển bằng ảnh tĩnh này sẽ trở lại kỹ hơn ở Chương 6, mục 6.4 khi bàn quy trình phát triển
offline đầy đủ (dùng `CogImageFileTool` để nạp cả một thư mục ảnh mẫu thay vì một file đơn lẻ).

## 5.4 CogToolBlock — đơn vị đóng gói quan trọng nhất

### 5.4.1 Vì sao ToolBlock là khái niệm cần nắm chắc nhất chương này

Trong một job thực tế, hàng chục tool nối dây với nhau nhanh chóng trở thành một sơ đồ phức tạp.
`CogToolBlock` giải quyết việc đó bằng cách **đóng gói** một cụm tool + cách nối dây của chúng
thành một khối duy nhất, chỉ lộ ra bên ngoài đúng những gì cần thiết: một tập `Inputs` (terminal
đầu vào của cả khối) và `Outputs` (terminal đầu ra). Bên trong khối có bao nhiêu tool, nối dây ra
sao — người dùng khối đó (kể cả chính chúng ta, khi quay lại sau vài tháng) không cần biết.

Đây chính xác là khái niệm **hợp đồng terminal** sẽ dùng lại liên tục: Chương 7 (mục 7.4) xây
kiến trúc job chuẩn bên trong một ToolBlock; Chương 12 (mục 12.4) viết script gắn vào ToolBlock;
Chương 13 (mục 13.3) gọi thẳng `Inputs`/`Outputs` từ C#. Thiết kế đúng bộ terminal của ToolBlock
**ngay từ khi còn trong QuickBuild** — đặt tên rõ ràng, đúng kiểu dữ liệu, đủ (không thiếu, không
thừa) — là khoản đầu tư trả lãi suốt vòng đời của job, vì đây chính là giao diện mà code C# sau
này sẽ gọi vào.

### 5.4.2 Thiết kế terminal: nghĩ cho người dùng sau này, không chỉ cho lúc thiết kế

Ba nguyên tắc khi đặt terminal cho một ToolBlock, áp dụng ngay cả trước khi biết C# sẽ gọi nó ra
sao:

- **Tên rõ nghĩa, ổn định.** Tên terminal là chuỗi ký tự được code C# tham chiếu trực tiếp (Chương
  13, mục 13.3) — không được compiler kiểm tra. Đặt tên mô tả đúng ý nghĩa (`PartX` chứ không
  `Output1`) và tránh đổi tên tuỳ tiện sau khi code C# đã bắt đầu phụ thuộc vào nó.
- **Kiểu dữ liệu đơn giản, ổn định.** Terminal nên trả kiểu dữ liệu cơ bản (số, chuỗi, bool) hoặc
  kiểu VisionPro chuẩn — tránh để terminal trả về một cấu trúc phức tạp nội bộ mà chỉ chính
  ToolBlock đó hiểu, khiến code C# phía ngoài phải "biết" quá nhiều về bên trong.
- **Đủ, không thừa.** Terminal đầu ra nên là **kết quả cuối cùng** đã tổng hợp (đúng vai trò của
  `CogResultsAnalysisTool`, Chương 12, mục 12.3) — không phơi bày kết quả thô của từng tool con
  nếu code bên ngoài không thực sự cần đến chúng riêng lẻ.

```text
CogToolBlock "TB_Inspect" (ví dụ minh hoạ — nội dung đầy đủ dựng ở Chương 7-12)
  Inputs:  InputImage
  Outputs: Ok, PartX, PartY, PartAngle, Code
```

## 5.5 Hiển thị và debug: CogRecordDisplay

### 5.5.1 Record — gói dữ liệu mang cả ảnh lẫn overlay kết quả

Debug trực quan trong VisionPro xoay quanh một khái niệm duy nhất: **record** (`ICogRecord`).
Một record mang `Content` (thường là ảnh) và có thể chứa `SubRecords` — các record con lồng bên
trong, mỗi record con ứng với kết quả của một tool trong chuỗi. Khi một `CogToolBlock` chạy xong,
gọi `CreateLastRunRecord()` trả về một record tổng hợp: ảnh gốc, cộng với toàn bộ overlay của mọi
tool con — vùng ROI, điểm biên caliper tìm được, khung định vị PMAlign — tất cả trong một cấu
trúc lồng nhau duy nhất.

![Hình 5.2 — CogRecordDisplay hiển thị ảnh với overlay lồng nhau từ SubRecords](../assets/ch05/hinh_5_2.png)
**Hình 5.2 — CogRecordDisplay hiển thị ảnh với overlay lồng nhau từ SubRecords.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): screenshot khu vực hiển thị kết quả của QuickBuild (tương
> đương CogRecordDisplay) trên một job có vài tool, ảnh nền + nhiều lớp overlay chồng lên nhau
> (ví dụ khung ROI màu vàng của một tool, điểm/đường kết quả màu xanh của tool khác). Bên cạnh
> ảnh, vẽ thêm (hoặc chèn) một cây thư mục nhỏ minh hoạ cấu trúc `Record → SubRecords` tương ứng
> — mỗi nhánh cây trỏ mũi tên nét đứt đến đúng lớp overlay nó tạo ra trên ảnh, để người đọc thấy
> rõ mối liên hệ giữa cấu trúc dữ liệu (SubRecords) và những gì nhìn thấy trên màn hình.

`CogRecordDisplay` — control hiển thị sẽ dùng lại từ code C# ở Chương 13, mục 13.5 — chỉ cần một
property duy nhất để hiển thị toàn bộ: gán `Record` của nó bằng record vừa tạo, và control tự vẽ
ảnh cùng mọi overlay lồng bên trong.

```text
Record (ToolBlock)
 ├─ Content: ảnh gốc
 └─ SubRecords:
     ├─ Record (Tool 1) → Content: ảnh + overlay ROI/kết quả của Tool 1
     ├─ Record (Tool 2) → Content: ảnh + overlay ROI/kết quả của Tool 2
     └─ ...
```

### 5.5.2 LastRunRecord vs CurrentRecord

Hai loại record dễ nhầm lẫn khi mới gặp: **LastRunRecord** (`CreateLastRunRecord()`) là ảnh chụp
lại đúng trạng thái **sau khi** tool chạy — cố định, dùng để xem lại kết quả một lần chạy cụ thể.
**CurrentRecord** (`CreateCurrentRecord()`) phản ánh trạng thái **hiện tại** của tool (kể cả khi
chưa chạy, hoặc đang ở giữa quá trình chỉnh tham số) — dùng khi cần xem cấu hình đang có, không
phải kết quả một lần chạy đã qua. Debug một kết quả sai của cycle vừa rồi dùng `LastRunRecord`;
xem tham số hiện tại của một tool đang chỉnh trong QuickBuild dùng `CurrentRecord`.

> 💡 **Mẹo thực chiến:** thói quen debug hiệu quả nhất xuyên suốt sách này — khi một kết quả trông
> "vô lý", việc đầu tiên là mở `CogRecordDisplay` xem `LastRunRecord`, lần theo overlay từng
> `SubRecord` để thấy chính xác tool nào trong chuỗi cho ra kết quả bất thường, thay vì đoán mò
> từ con số cuối cùng. Nguyên tắc này sẽ lặp lại ở gần như mọi chương Phần III.

## 5.6 Tổ chức nhiều job, save/load .vpp có kỷ luật

Một dự án vision thực tế thường có nhiều file `.vpp` — job chính, job debug riêng cho từng tool
lúc phát triển, job phiên bản cũ giữ lại để so sánh. Ba thói quen tổ chức đáng thiết lập từ sớm,
trước khi số lượng file tăng lên gây rối:

- **Quy ước đặt tên nhất quán** — bao gồm thông tin phiên bản/ngày trong tên file hoặc trong một
  file mô tả đi kèm, để không nhầm lẫn "job đang chạy production" với "job đang thử nghiệm".
- **Tách job phát triển khỏi job production** — job dùng để thử nghiệm/debug một tool riêng lẻ
  không nên là cùng file với job chạy sản xuất; sửa đổi thử nghiệm không được vô tình ảnh hưởng
  job đang chạy thật.
- **Giữ lại phiên bản cũ có kỷ luật** — trước khi ghi đè một job đang hoạt động (đặc biệt liên
  quan đến retrain pattern — Chương 8, mục 8.5; Chương 14, mục 14.4), backup phiên bản cũ với tên
  rõ ràng. Vì `.vpp` là file nhị phân (mục 5.2.2), "backup" ở đây là sao chép file vật lý, không
  phải trông chờ vào diff của hệ thống quản lý mã nguồn văn bản thông thường.

## Tổng kết chương

- VisionPro là môi trường **data-flow**: tool nối terminal đầu vào/đầu ra với nhau, toàn bộ cấu
  trúc lưu trong file `.vpp` nhị phân — tư duy khác cơ bản so với code tuần tự.
- `CogJob`/`CogJobManager` là bộ máy chạy tự động **của QuickBuild** (thu ảnh, throughput, đa
  luồng) — hữu ích để thiết kế/thử nghiệm; ứng dụng C# sản xuất (Chương 13-14) thường làm việc
  trực tiếp với `CogToolBlock`, tự viết vòng lặp riêng.
- `CogToolBlock` đóng gói một cụm tool thành một khối có `Inputs`/`Outputs` — đây là **hợp đồng
  terminal** sẽ dùng lại xuyên suốt Phần III và là cầu nối trực tiếp sang C# (Chương 13). Thiết
  kế terminal tốt ngay từ QuickBuild: tên rõ nghĩa ổn định, kiểu dữ liệu đơn giản, đủ không thừa.
- `CogRecordDisplay` hiển thị ảnh + toàn bộ overlay kết quả qua một `Record` (`ICogRecord`) có
  cấu trúc lồng `SubRecords` theo từng tool con. `LastRunRecord` là kết quả một lần chạy cụ thể;
  `CurrentRecord` là trạng thái hiện tại của tool. Debug bằng cách lần theo `SubRecord` là thói
  quen sẽ dùng lại ở hầu hết các chương sau.
- Tổ chức nhiều file `.vpp` cần kỷ luật riêng vì đây là file nhị phân: quy ước tên nhất quán, tách
  job phát triển khỏi production, backup vật lý trước khi ghi đè.

## Lỗi thường gặp

**Lỗi 1 — Dồn toàn bộ logic vào một job khổng lồ, không dùng ToolBlock để đóng gói.** Hiện tượng:
job có hàng chục tool nối dây chằng chịt trên một canvas, khó hiểu lại sau vài tháng, khó tái sử
dụng một phần logic cho job khác. Nguyên nhân: bỏ qua việc đóng gói theo cụm chức năng ngay từ khi
thiết kế. Cách tránh: tách theo giai đoạn xử lý tự nhiên (đúng kiến trúc Acquisition → Calibration
→ Align/Fixture → Inspect sẽ học ở Chương 7) thành các ToolBlock riêng, mỗi khối một trách nhiệm
rõ ràng (mục 5.4.1).

**Lỗi 2 — Đặt tên terminal mơ hồ rồi đổi tuỳ tiện sau khi code đã phụ thuộc vào nó.** Hiện tượng:
lỗi runtime "not found" khi gọi terminal từ C# sau một lần chỉnh sửa job tưởng chừng vô hại. Nguyên
nhân: tên terminal là chuỗi, không được compiler kiểm tra tại thời điểm build (chi tiết đầy đủ ở
Chương 13, mục 13.3). Cách tránh: đặt tên rõ nghĩa ngay từ đầu, coi terminal đã đặt tên là một
phần của hợp đồng ổn định, không đổi tuỳ tiện.

**Lỗi 3 — Không lưu lại ảnh mẫu dùng để train/thiết kế job.** Hiện tượng: cần điều chỉnh lại một
tool sau này nhưng không còn ảnh gốc đã dùng lúc thiết kế ban đầu để so sánh/kiểm chứng. Nguyên
nhân: chỉ giữ file `.vpp`, không lưu riêng bộ ảnh mẫu đã dùng để phát triển nó. Cách tránh: luôn
lưu lại bộ ảnh mẫu đại diện (không chỉ ảnh "đẹp nhất") cùng với job — nền tảng cho kỹ thuật phát
triển offline ở Chương 6, mục 6.4 và cho golden set nghiệm thu ở Chương 16.

**Lỗi 4 — Ghi đè `.vpp` production mà không backup, coi nhẹ vì "chỉ là file cấu hình".** Hiện
tượng: một lần chỉnh sửa/teach lại làm job tệ đi, không có đường lùi. Nguyên nhân: `.vpp` là file
nhị phân, không có lịch sử "undo" qua nhiều phiên như code văn bản dưới quản lý mã nguồn thông
thường. Cách tránh: backup file vật lý có kỷ luật trước mọi lần ghi đè quan trọng (mục 5.6) —
nguyên tắc này sẽ nhắc lại nghiêm túc hơn ở Chương 8 (retrain pattern) và Chương 14 (màn teach).

\newpage

# Chương 6 — Thu nhận ảnh trong VisionPro

Job `TB_Inspect` của MeoVision (Chương 5) chạy mượt trong QuickBuild: nạp một ảnh tĩnh từ
`Images\`, PMAlign định vị đúng, Caliper đo đúng, mọi thứ nhìn "đã xong". Đội lắp máy gắn camera
GigE thật lên trạm, đấu dây trigger từ PLC, bấm Start — và ảnh trả về khi thì cháy sáng trắng
xoá, khi thì đen kịt, khi thì mỗi ảnh lệch nhịp so với lúc chi tiết thực sự nằm dưới camera. Không
tool nào trong job sai cả — PMAlign, Caliper vẫn là chính nó. Vấn đề nằm ở một tầng thấp hơn, tầng
mà ảnh tĩnh nạp từ file chưa bao giờ đụng tới: **camera thật có exposure phải chỉnh, có gain phải
đặt, có trigger phải đúng nhịp với đèn strobe** — những thứ một file `.bmp` nằm yên trên đĩa không
cần quan tâm.

Chương này lấp đúng khoảng trống đó. Chúng ta bắt đầu với `CogAcqFifoTool` — khối thu ảnh đứng
đầu mọi job dùng camera thật (6.1), rồi đi sâu vào cấu hình exposure, gain, trigger, strobe cả từ
QuickBuild lẫn từ code (6.2) — đúng phần lý thuyết exposure/trigger đã học ở Chương 3, mục 3.3 và
3.5, giờ biến thành thao tác cụ thể trên `ICogAcqFifo` thật, và là nền cho Chương 13, mục 13.4 khi
acquisition được gọi trực tiếp từ ứng dụng C#. Mục 6.3 nhìn vào bên trong một tấm ảnh sau khi đã
thu về — `ICogImage` là gì, khi nào gặp `CogImage8Grey`, khi nào gặp `CogImage24PlanarColor`. Mục
6.4 quay lại kỹ thuật ảnh tĩnh của Chương 5 và đào sâu nó thành một quy trình phát triển offline
đầy đủ bằng `CogImageFileTool`. Mục 6.5 khép chương bằng bài toán nâng cao: nhiều camera trên một
máy.

## 6.1 CogAcqFifoTool: khung acquisition

### 6.1.1 Tool không tự chụp ảnh — nó điều khiển một ICogAcqFifo

Điểm dễ hiểu nhầm đầu tiên: `CogAcqFifoTool` (tool ta kéo vào job trong QuickBuild) **không tự
mình** giao tiếp với camera. Nó là một lớp vỏ mỏng bọc quanh một đối tượng thật sự làm việc đó —
`ICogAcqFifo`, truy cập qua property `Operator` của tool. Toàn bộ cấu hình exposure/gain/trigger
(mục 6.2) và thao tác thu ảnh (`StartAcquire`/`CompleteAcquire`, sẽ gặp lại ở Chương 13, mục 13.4)
đều nằm trên `Operator`, không nằm trực tiếp trên tool. `CogAcqFifoTool` chỉ thêm phần "vỏ" quen
thuộc của mọi tool VisionPro: `RunStatus`, `OutputImage`, và khả năng nằm trong một `CogToolBlock`
nối dây cùng PMAlign/Caliper/Blob phía sau.

> 📌 **Lưu ý:** phân biệt hai đối tượng này ngay từ đầu tránh được rất nhiều nhầm lẫn khi đọc code
> mẫu Cognex — một số ví dụ chính thức thao tác thẳng trên `ICogAcqFifo` lấy từ
> `ICogFrameGrabber.CreateAcqFifo(...)`, hoàn toàn không đi qua `CogAcqFifoTool`/`CogToolBlock`.
> Cách đó hợp lệ cho ứng dụng tự viết vòng lặp thu ảnh (Chương 13), nhưng bên trong một job
> QuickBuild nối dây theo kiến trúc chuẩn Acquisition → Calibration → Align/Fixture → Inspect (Chương 7, mục 7.4),
> `CogAcqFifoTool` là điểm vào duy nhất.

### 6.1.2 Ba bước trong QuickBuild: từ ảnh tĩnh sang camera GigE thật

Kéo `CogAcqFifoTool` vào job, mở property grid của nó, ba việc cần làm theo đúng thứ tự:

1. **Chọn frame grabber.** VisionPro liệt kê mọi thiết bị thu ảnh đã cài driver trên máy — với
   camera GigE Vision (chuẩn MeoVision dùng, Chương 3, mục 3.4), danh sách này đến từ driver GigE
   Vision đã cài, không phải "cắm là thấy" như USB Vision.
2. **Chọn video format.** Mỗi frame grabber/camera công bố một danh sách format khả dụng — độ
   phân giải, tốc độ khung hình danh nghĩa, định dạng pixel. Với camera mono 5 MP của MeoVision
   (2448 × 2048 px, Chương 3), format tương ứng luôn ở dạng ảnh xám 8-bit.
3. **Chạy thử một lần acquire** ngay trong QuickBuild để xác nhận cả chuỗi driver → frame grabber
   → tool → hiển thị hoạt động, trước khi nối dây tool xử lý phía sau.

![Hình 6.1 — Property grid của CogAcqFifoTool trong QuickBuild](../assets/ch06/hinh_6_1.png)
**Hình 6.1 — Property grid của CogAcqFifoTool trong QuickBuild.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): screenshot QuickBuild với một `CogAcqFifoTool` đang được chọn
> trên canvas, property grid bên phải hiển thị đầy đủ. Khoanh đỏ + đánh số ba vùng: (1) dropdown
> chọn Frame Grabber (đang chọn camera GigE thật, không phải "Simulated"); (2) dropdown Video
> Format bên dưới, hiển thị chuỗi tên format 8-bit mono; (3) nút "Acquire"/"Live" để chạy thử.
> Panel `CogRecordDisplay`/CogDisplay bên cạnh hiển thị ảnh vừa chụp từ camera thật (không phải
> ảnh tĩnh từ file như Hình 5.1 của Chương 5) — để người đọc thấy rõ khác biệt trực quan.

### 6.1.3 Từ code: chọn đúng camera GigE, đúng video format

Chương 13 (mục 13.4) sẽ gọi lại `CogAcqFifoTool` từ ứng dụng C# để tự viết vòng lặp acquisition
sản xuất. Ở chương này, mục tiêu hẹp hơn: viết đúng đoạn code **tìm và cấu hình** camera — phần
việc tương đương những gì vừa làm bằng chuột ở mục 6.1.2, để hiểu property grid thực chất gọi API
nào bên dưới.

> 📌 **Lưu ý:** từ đây đến hết chương, code minh hoạ đúng cấu trúc và luồng gọi API của VisionPro
> 9.0, nhưng tên chính xác từng property/hằng số (ví dụ `Format8Grey`, tham số đặt tên của
> `CreateAcqFifo`, `CogImageFileTool` có property index nằm trực tiếp trên tool hay trên `Operator`
> của nó) nên đối chiếu lại với IntelliSense hoặc `Doc\en\` (mục 5.1.2) trên máy có cài SDK thật
> trước khi đưa vào code sản xuất — SDK có thể khác đôi chút giữa các bản Cumulative Release.

**Bảng 6.1 — Hai cách liệt kê frame grabber: tổng quát và chuyên cho GigE Vision.**

| Lớp | Namespace / DLL | Liệt kê gì |
|---|---|---|
| `CogFrameGrabbers` | `Cognex.VisionPro` / `Cognex.VisionPro.Core.dll` | Mọi frame grabber đã cài driver — GigE, USB3, CameraLink, 1394... |
| `CogFrameGrabberGigEs` | `Cognex.VisionPro.FGGigE` / `Cognex.VisionPro.FGGigE.dll` | Chỉ camera GigE Vision |

Với MeoVision (camera GigE mono), dùng thẳng `CogFrameGrabberGigEs` tránh phải tự lọc lại danh
sách chung — ý định rõ ràng ngay trong code: đây là trạm chỉ dùng GigE Vision, không có ý định hỗ
trợ loại giao tiếp khác.

**Code 6.1 — Tìm camera GigE đầu tiên, chọn video format 8-bit mono, gán vào CogAcqFifoTool.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.Exceptions;
using Cognex.VisionPro.FGGigE;

var gigeGrabbers = new CogFrameGrabberGigEs();
if (gigeGrabbers.Count < 1)
    throw new CogAcqNoFrameGrabberException("Không tìm thấy camera GigE Vision nào.");

// MeoVision chỉ có một camera ở trạm này — luôn lấy camera GigE đầu tiên tìm được.
ICogFrameGrabber frameGrabber = gigeGrabbers[0];

// Danh sách video format khả dụng — đúng danh sách property grid QuickBuild hiển thị.
CogStringCollection formats = frameGrabber.AvailableVideoFormats;
string monoFormat = formats[0];   // chọn đúng format 8-bit mono của camera MeoVision

var acqTool = new CogAcqFifoTool
{
    Operator = frameGrabber.CreateAcqFifo(
        monoFormat, CogAcqFifoPixelFormatConstants.Format8Grey,
        cameraPort: 0, autoPrepare: false)
};
```

> 📌 **Lưu ý:** `frameGrabber.CreateAcqFifo(...)` tạo một `ICogAcqFifo` **mới, độc lập**, đã gắn
> sẵn video format cụ thể — gán thẳng vào `Operator` (property này ghi được). Cách khác, ngắn hơn
> nhưng ít tường minh về format, là gọi `acqTool.Operator.Connect(frameGrabber)` trên `Operator`
> mặc định đã có sẵn của tool (đúng cách Chương 13, mục 13.4 dùng, ở đó .vpp đã định hình sẵn cấu
> hình từ QuickBuild). Chọn cách nào phụ thuộc việc format đã được chốt trong .vpp hay cần chọn
> động lúc chạy — MeoVision dùng đúng một camera cố định nên chốt tường minh như Code 6.1 rõ ràng
> hơn cho người đọc lại sau này.

## 6.2 Cấu hình exposure/gain/trigger; strobe

### 6.2.1 Vì sao mục này quan trọng hơn vẻ ngoài của nó

Chương 3 (mục 3.3, mục 3.5) đã bàn lý thuyết: exposure quyết định lượng sáng tích luỹ, gain khuếch
đại tín hiệu (đánh đổi bằng nhiễu), trigger đồng bộ thời điểm chụp với thời điểm chi tiết đến đúng
vị trí, strobe đồng bộ đèn với đúng khoảnh khắc exposure. Ở đây, những khái niệm đó trở thành thuộc
tính thật trên `ICogAcqFifo` — và một quan sát quan trọng: VisionPro không cấu hình exposure/gain
bằng cùng một cơ chế. Có phần được VisionPro trừu tượng hoá gọn (exposure), có phần phải chạm
thẳng vào tính năng riêng của camera (gain, một số dòng trigger nâng cao) — biết trước ranh giới
này tránh mất thời gian tìm một property "chuẩn" không tồn tại.

### 6.2.2 Exposure: OwnedExposureParams

`ICogAcqFifo.OwnedExposureParams` trả về `ICogAcqExposure` — property `Exposure` đọc/ghi thời gian
phơi sáng tính bằng **mili-giây** (giá trị `0` nghĩa là dùng thời gian nhỏ nhất camera hỗ trợ).
Đây là property VisionPro chuẩn hoá, hoạt động giống nhau bất kể hãng camera nào đứng sau frame
grabber — không phải mọi camera đều hỗ trợ (`OwnedExposureParams` trả `null` nếu không), nên luôn
kiểm tra trước khi dùng.

**Code 6.2 — Đặt exposure, kiểm tra hỗ trợ trước khi ghi.**

```csharp
ICogAcqExposure exposureParams = acqTool.Operator.OwnedExposureParams;
if (exposureParams != null)
{
    exposureParams.Exposure = 8.0;   // ms — giá trị khởi điểm, tinh theo histogram (Ch4)
    acqTool.Operator.Prepare();      // ghi cấu hình xuống camera — bắt buộc sau khi đổi
}
```

> ⚠️ **Cảnh báo:** đổi bất kỳ tham số acquisition nào (`Exposure`, `Gain`, trigger...) mà quên gọi
> `Prepare()` sau đó, camera vẫn chụp với cấu hình **cũ** — lỗi im lặng, không có exception nào
> báo, chỉ phát hiện được khi ảnh ra không đúng như mong đợi. `Prepare()` là bước bắt buộc, không
> tuỳ chọn, sau mọi lần chỉnh tham số acquisition từ code.

### 6.2.3 Gain: không có chuẩn chung — chạm thẳng GenICam

Khác exposure, VisionPro **không có** một `ICogAcqGain` phổ quát cho mọi camera GigE Vision.
`OwnedDigitalCameraGainParams` (`ICogAcqDigitalCameraGain`) chỉ có đúng một cờ bật/tắt
`DigitalHighGainSelected` — không phải một giá trị gain liên tục. Với đa số camera GigE Vision, đặt
gain (analog/digital, đơn vị dB hoặc raw tuỳ hãng) đi qua đường truy cập tính năng riêng của camera:
`ICogFrameGrabber.OwnedGigEAccess` (`ICogGigEAccess`) — set trực tiếp một GenICam feature bằng tên
chuỗi, tên và đơn vị chính xác nằm trong tài liệu của từng hãng camera (tra bằng công cụ GigE Vision
Configurator đi kèm VisionPro).

**Code 6.3 — Đặt gain qua GenICam feature (tên feature tuỳ hãng camera, tra tài liệu camera).**

```csharp
ICogGigEAccess gigeAccess = frameGrabber.OwnedGigEAccess;
if (gigeAccess != null)
{
    // "GainRaw" là tên feature ví dụ — PHẢI đối chiếu tài liệu camera thật / GigE
    // Vision Configurator trước khi đưa vào code sản xuất.
    gigeAccess.SetIntegerFeature("GainRaw", 200);
    acqTool.Operator.Prepare();
}
```

> 🔍 **Đào sâu thêm (đã kiểm chứng trên SDK thật, VisionPro 9.0 CR2):** phản chiếu `ICogGigEAccess`
> xác nhận `SetIntegerFeature(string, uint)`/`SetDoubleFeature(string, double)`/`SetFeature(string,
> string)` tồn tại đúng như Code 6.3 dùng — nhưng đúng như đã cảnh báo, **không có tên feature
> chuẩn chung cho gain** giữa các hãng GigE Vision (một số theo GenICam SFNC cũ dùng `"GainRaw"`,
> bản mới hơn dùng `"Gain"`). Thay vì chỉ tra tài liệu hãng, `ICogGigEAccess` còn có
> `GetAvailableFeatures(string category)` — gọi hàm này lúc kết nối để liệt kê toàn bộ tên feature
> GenICam camera thật đang có, tự động phát hiện tên đúng thay vì đoán hoặc tra tài liệu giấy.

> 💡 **Mẹo thực chiến:** ưu tiên tăng ánh sáng/exposure trước khi tăng gain (nguyên tắc từ Chương
> 2 và Chương 3, mục 3.3: gain khuếch đại cả tín hiệu lẫn nhiễu). Đặt gain khác 0 mặc định là dấu hiệu nên
> quay lại xem lại thiết kế đèn (Chương 2) trước khi chấp nhận nó như một tham số cố định của job.

### 6.2.4 Trigger: OwnedTriggerParams

`ICogAcqFifo.OwnedTriggerParams` (`ICogAcqTrigger`) có ba property chính: `TriggerEnabled`,
`TriggerLowToHigh` (cạnh kích trigger), và `TriggerModel` (`CogAcqTriggerModelConstants`) — property
quyết định quan hệ giữa lệnh phần mềm (`StartAcquire()`, Chương 13 mục 13.4) và tín hiệu trigger
phần cứng từ PLC/sensor.

**Bảng 6.2 — Các giá trị CogAcqTriggerModelConstants.**

| Giá trị | Ý nghĩa | Khi dùng |
|---|---|---|
| `FreeRun` | Tự động chụp liên tục theo tốc độ khung hình, không chờ trigger nào | Canh nét/canh sáng lúc lắp đặt — KHÔNG dùng khi sản xuất |
| `Auto` | Chụp hoàn toàn theo trigger phần cứng bên ngoài; gọi `StartAcquire()` là lỗi | Trigger cứng nối thẳng camera/frame grabber, không qua điều phối phần mềm |
| `Semi` | `StartAcquire()` "mở sẵn sàng", trigger phần cứng bên ngoài mới thực sự chốt khung hình | Mô hình phổ biến nhất khi PLC vừa phát trigger vừa cần phần mềm điều phối thời điểm (Chương 13 mục 13.4, Chương 15 mục 15.1) |
| `Manual` | Chụp ngay khi gọi `StartAcquire()` — trigger hoàn toàn bằng phần mềm, không chờ tín hiệu ngoài | Test/debug từ code khi chưa có trigger phần cứng đấu nối |
| `Slave` | Giá trị chỉ đọc, VisionPro tự gán cho các FIFO "phụ" đồng bộ theo một FIFO chính, không tự đặt được | Nhiều camera chụp đồng thời theo cùng một trigger gốc (mục 6.5) |

**Code 6.4 — Cấu hình trigger mô hình Semi: PLC ra tín hiệu, phần mềm chủ động StartAcquire trước.**

```csharp
ICogAcqTrigger triggerParams = acqTool.Operator.OwnedTriggerParams;
if (triggerParams != null)
{
    triggerParams.TriggerModel     = CogAcqTriggerModelConstants.Semi;
    triggerParams.TriggerEnabled   = true;
    triggerParams.TriggerLowToHigh = true;   // cạnh lên kích hoạt — xác nhận với đấu nối PLC
    acqTool.Operator.Prepare();
}
```

> 🔍 **Đào sâu thêm:** timing diagram đầy đủ (trigger → exposure → readout) đã vẽ ở Chương 3, mục
> 3.5 — mục này chỉ ánh xạ lý thuyết đó sang đúng property. Chương 15, mục 15.1 sẽ đi tiếp phần PLC
> nhìn thấy handshake này ra sao (busy/done/result).

### 6.2.5 Strobe: đồng bộ đèn với đúng khoảnh khắc exposure

Ba property liên quan: `OwnedStrobeParams` (`ICogAcqStrobe.StrobeEnabled`, bật/tắt strobe),
`OwnedStrobeDelayParams` (`ICogAcqStrobeDelay.StrobeDelay`, độ trễ trước khi bật đèn), và
`OwnedStrobePulseDurationParams` (`ICogAcqStrobePulseDuration.StrobePulseDuration`, thời lượng
xung đèn). Ba con số này cùng nhau định vị chính xác xung sáng nằm ở đâu trên trục thời gian so
với cửa sổ exposure — sai một trong hai (trễ quá sớm/quá muộn, hoặc xung quá ngắn so với exposure)
cho ảnh thiếu sáng dù exposure/gain đặt đúng. Với camera global shutter (mặc định an toàn của
sách này — Chương 3, mục 3.3.4), toàn bộ cảm biến phơi sáng cùng một cửa sổ thời gian, nên hiệu
ứng thiếu sáng do strobe cắt sớm là **đều trên toàn khung hình**, không phải một phần ảnh sáng một
phần tối — kiểu méo cục bộ đó là dấu hiệu của rolling shutter (Chương 3, mục 3.3.4), một vấn đề
khác hẳn về nguyên nhân.

**Code 6.5 — Cấu hình strobe: bật, đặt độ trễ và thời lượng xung.**

```csharp
ICogAcqStrobe strobe = acqTool.Operator.OwnedStrobeParams;
ICogAcqStrobeDelay delay = acqTool.Operator.OwnedStrobeDelayParams;
ICogAcqStrobePulseDuration duration = acqTool.Operator.OwnedStrobePulseDurationParams;

if (strobe != null && delay != null && duration != null)
{
    strobe.StrobeEnabled   = true;
    delay.StrobeDelay      = 0.0;    // đèn bật cùng lúc exposure bắt đầu
    duration.StrobePulseDuration = 8.0;   // >= Exposure (Code 6.2), phủ hết cửa sổ phơi sáng
    acqTool.Operator.Prepare();
}
```

![Hình 6.2 — Timing diagram: trigger, exposure và strobe trên cùng trục thời gian](../assets/ch06/hinh_6_2.png)
**Hình 6.2 — Timing diagram: trigger, exposure và strobe trên cùng trục thời gian.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ vẽ bằng draw.io, trục ngang là thời gian, ba hàng xung
> song song từ trên xuống: (1) "Trigger" — một xung cạnh lên hẹp tại t=0; (2) "Exposure" — một
> cửa sổ hình chữ nhật bắt đầu ngay sau trigger (độ trễ camera, ghi chú nhỏ), độ rộng = giá trị
> Exposure (Code 6.2); (3) "Strobe" — một xung hình chữ nhật, mép trái lệch khỏi mép trái Exposure
> đúng bằng `StrobeDelay`, độ rộng = `StrobePulseDuration`, vẽ chồng lấn hoàn toàn lên cửa sổ
> Exposure (kéo dài bằng hoặc hơn) để minh hoạ nguyên tắc "đèn phải sáng trong suốt lúc cảm biến
> đang phơi sáng". Chú thích dưới sơ đồ: "StrobePulseDuration ngắn hơn Exposure → toàn bộ ảnh thiếu
> sáng đều (global shutter) dù Exposure/Gain đã đặt đúng — không phải chỉ một phần khung hình".

## 6.3 ICogImage, CogImage8Grey, CogImage24PlanarColor

### 6.3.1 ICogImage: hợp đồng tối thiểu, không kế thừa IDisposable

Kết quả của mọi lần acquire (`Operator.Acquire(...)`/`CompleteAcquire(...)`, gặp đầy đủ ở Chương
13 mục 13.4) là một `ICogImage` — interface tối thiểu mà mọi kiểu ảnh trong VisionPro đều thực thi:
`Width`, `Height`, `Allocated` (đã cấp phát bộ nhớ pixel hay chưa), cùng `CoordinateSpaceTree` và
`PixelFromRootTransform` (hệ toạ độ gắn với ảnh — nền tảng của Chương 7). Đúng như Chương 13, mục
13.6.1 đã chỉ ra và nhắc lại ở đây cho nhất quán: **`ICogImage` không kế thừa `IDisposable`** — một
biến khai báo kiểu này không thể `.Dispose()` hay đặt trong khối `using` trực tiếp. Chi tiết đầy đủ
về hệ quả (rò bộ nhớ nếu quên ép kiểu) và cách xử lý đúng nằm ở Chương 13, mục 13.6.1 — mục này chỉ
cần ghi nhớ điều đó khi bắt đầu làm việc với ảnh thu được từ mục 6.1–6.2.

### 6.3.2 Hai lớp cụ thể gặp thường xuyên nhất

Lập trình theo interface — khai báo biến/tham số theo interface thay vì lớp cụ thể — nghĩa là hầu
hết code chỉ cần biết `ICogImage`.
Nhưng khi cần đọc/ghi dữ liệu pixel trực tiếp (ImageProcessing, Chương 12 mục 12.1) hoặc cần Dispose
tường minh (Chương 13, mục 13.6.1), ta chạm đến lớp cụ thể — với MeoVision, gần như luôn là một
trong hai lớp sau.

**Bảng 6.3 — ICogImage vs. hai lớp cụ thể thường gặp.**

| | `ICogImage` | `CogImage8Grey` | `CogImage24PlanarColor` |
|---|---|---|---|
| Vai trò | Interface tối thiểu | Ảnh xám 8-bit/pixel | Ảnh màu 3 mặt phẳng, 8-bit/kênh |
| `IDisposable` | Không | Có | Có |
| Dùng khi | Khai báo biến/tham số chung | Camera mono MeoVision (Chương 3) — trường hợp phổ biến nhất sách này | Có module quan sát màu (Chương 12, mục 12.6) |
| Namespace / DLL | `Cognex.VisionPro` / `Cognex.VisionPro.Core.dll` | như trên | như trên |

> 📌 **Lưu ý:** camera chuẩn của MeoVision là mono (Chương 3, mục 3.1 — lý do chọn mono cho vision
> công nghiệp), nên tuyệt đại đa số ảnh xuất hiện xuyên suốt sách này là `CogImage8Grey` đứng sau
> một biến khai báo `ICogImage`. `CogImage24PlanarColor` chỉ xuất hiện khi job thật sự cần phân
> biệt màu sắc (`CogColorMatchTool`/`CogColorExtractorTool`, mục NÂNG CAO ở Chương 12 mục 12.6) —
> đừng đổi sang camera màu chỉ vì "có thể sau này cần", chi phí (cảm biến đắt hơn, mất chi tiết ở
> ảnh xám do lọc Bayer) không đáng nếu bài toán không đòi hỏi màu.

## 6.4 Nạp ảnh từ file/thư mục (CogImageFileTool)

### 6.4.1 Một file thật sự — nhưng đóng vai trò một kho ảnh mẫu

Chương 5, mục 5.3.2 đã hứa quay lại kỹ thuật phát triển bằng ảnh tĩnh, lần này "dùng
`CogImageFileTool` để nạp cả một thư mục ảnh mẫu thay vì một file đơn lẻ". Cần làm rõ ngay một chi
tiết dễ hiểu nhầm: `CogImageFileTool.Operator` (`CogImageFile`) chỉ mở **một file** qua
`Open(fileName, mode)` — không có API "mở một thư mục Windows". Điều biến một file thành cả một kho
ảnh mẫu là **định dạng file**: phần lớn định dạng (`CogImageFileBMP`, `CogImageFileJPEG`,
`CogImageFilePNG`, `CogImageFileTIFF`) chỉ chứa đúng một ảnh, nhưng **Cognex Image Database**
(đuôi `.idb`/`.cdb`, lớp `CogImageFileCDB`) là một file **chứa nhiều ảnh** — đúng nghĩa "đóng gói cả
một thư mục ảnh mẫu vào một file". Truy cập từng ảnh bên trong qua chỉ số (`Operator[i]`), tổng số
ảnh qua `Operator.Count`.

> 📌 **Lưu ý:** đây chính là hình thức thật của "thư mục ảnh mẫu" mà Chương 5 nhắc tới — về mặt kỹ
> thuật là một file `.cdb` duy nhất đóng gói toàn bộ ảnh trong thư mục gốc, không phải VisionPro
> tự động duyệt một thư mục Windows theo thời gian thực. Bước đóng gói (mục 6.4.2 dưới đây) chỉ làm
> một lần khi chuẩn bị bộ ảnh phát triển.

### 6.4.2 Đóng gói: từ một thư mục ảnh rời sang một file .cdb

Việc đóng gói dùng chính `CogImageFileTool`, chạy hai lượt: lượt 1 mở từng ảnh rời (BMP/PNG/JPEG đã
chụp) ở chế độ `Read` để lấy `ICogImage`; lượt 2 mở file `.cdb` đích ở chế độ `Write` và `Append`
từng ảnh vào đó.

**Code 6.6 — Đóng gói một thư mục ảnh mẫu rời thành một file .cdb.**

```csharp
namespace MeoVision.Tools
{
    using System.IO;
    using Cognex.VisionPro;
    using Cognex.VisionPro.ImageFile;

    /// <summary>Đóng gói một thư mục ảnh mẫu rời thành một file .cdb.</summary>
    public static class SampleImagePackager
    {
        public static void PackFolder(string sourceFolder, string cdbPath)
        {
            using var reader = new CogImageFile();
            using var writer = new CogImageFile();
            writer.Open(cdbPath, CogImageFileModeConstants.Write);

            foreach (string path in Directory.GetFiles(sourceFolder, "*.bmp"))
            {
                reader.Open(path, CogImageFileModeConstants.Read);
                writer.Append(reader[0]);   // ảnh đơn lẻ luôn ở chỉ số 0
                reader.Close();
            }

            writer.Close();
        }
    }
}
```

> 📌 **Lưu ý:** khác `ICogImage` (mục 6.3.1), `CogImageFile` **có** kế thừa `IDisposable` — dùng
> `using var` như Code 6.6 là đúng, không cần ép kiểu thủ công như trường hợp ảnh.

> ⚠️ **Cảnh báo:** cố `Append()` một ảnh thứ hai vào file định dạng chỉ hỗ trợ một ảnh (BMP/JPEG/
> PNG/TIFF) ném `CogImageFileMultipleImagesNotSupportedException` — luôn đóng gói vào `.cdb`/`.idb`
> khi cần nhiều hơn một ảnh trong cùng một file, đừng cố dùng định dạng ảnh đơn lẻ cho việc này.

### 6.4.3 Duyệt qua toàn bộ bộ ảnh: NextImageIndex, CurrentImageIndex, ImageIndexIncrement

Mở lại `.cdb` ở chế độ `Read`, `CogImageFileTool` (đặt trong một `CogToolGroup`/`CogToolBlock` nối
tiếp job xử lý) duyệt qua từng ảnh bằng ba property: `NextImageIndex` (đặt để nhảy đến ảnh cụ thể,
đặt `0` để quay lại ảnh đầu), `CurrentImageIndex` (ảnh vừa chạy xong ở lần `Run()` gần nhất), và
`ImageIndexIncrement` (số ảnh nhảy tới sau mỗi `Run()`, mặc định `1`). Mỗi lần `Run()`, `OutputImage`
được cập nhật thành ảnh hiện tại — nối dây `OutputImage` này vào đúng terminal `InputImage` của
`TB_Inspect` (Chương 5, mục 5.4) là job phát triển offline hoàn chỉnh.

**Code 6.7 — Chạy toàn bộ bộ ảnh mẫu qua job, đếm Accept/Reject (nền tảng golden set, Chương 16).**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;

imageFileTool.NextImageIndex = 0;   // quay về ảnh đầu tiên của bộ .cdb

int accept = 0, reject = 0;
int total = imageFileTool.Operator.Count;

do
{
    imageFileTool.Run();
    toolBlock.Inputs["InputImage"].Value = imageFileTool.OutputImage;
    toolBlock.Run();

    if ((bool)toolBlock.Outputs["Ok"].Value) accept++; else reject++;
}
while (imageFileTool.CurrentImageIndex < total - 1);
```

> 💡 **Mẹo thực chiến:** vòng lặp Code 6.7 chính là hạt nhân của việc "chạy thống kê trên golden
> set" sẽ bàn kỹ ở Chương 16, mục 16.3 — chạy job trên toàn bộ bộ ảnh mẫu đã biết trước kết quả
> đúng, đếm accept/reject, so với kỳ vọng. Xây thói quen này ngay từ lúc phát triển (thay vì chỉ
> thử vài ảnh "đẹp" bằng tay) là cách rẻ nhất phát hiện sớm một thay đổi tham số làm hỏng những
> trường hợp không nằm trong ảnh đang xem trên màn hình lúc đó.

### 6.4.4 Vì sao kỹ thuật này quan trọng hơn một tiện ích debug

Phát triển bằng bộ ảnh mẫu đóng gói không chỉ để "không cần camera lúc code ở bàn làm việc". Nó
tách hai việc vốn hay bị trộn lẫn: **đúng logic job** (PMAlign định vị đúng, Caliper đo đúng — kiểm
được lặp lại trên đúng cùng một bộ ảnh mỗi lần sửa tham số) và **đúng cấu hình acquisition** (mục
6.1–6.2 — exposure/gain/trigger đúng trên phần cứng thật tại hiện trường). Trộn hai việc — vừa
chỉnh tham số tool vừa chụp lại ảnh mới mỗi lần — khiến không thể biết một thay đổi vừa rồi làm kết
quả khác đi vì tool hay vì ảnh đầu vào đã khác. Đây cũng là nền tảng kỹ thuật simulation mode sẽ gặp
lại ở Chương 14, mục 14.6 (bộ ảnh thay camera thật ngay trong ứng dụng sản xuất, phục vụ dev/test
không cần ra máy).

## 6.5 [NÂNG CAO] Nhiều camera trên một máy

### 6.5.1 Một CogAcqFifoTool — một FIFO — một camera

Mô hình VisionPro không có khái niệm "một tool acquisition dùng chung cho nhiều camera" — mỗi
`ICogAcqFifo` gắn cố định với một frame grabber cụ thể (qua `CreateAcqFifo`/`Connect`, mục 6.1.3).
Trạm có 2 camera cần 2 `CogAcqFifoTool` độc lập (thường nằm trong 2 `CogToolBlock` xử lý riêng, mỗi
khối một chuỗi Acquisition → Calibration → Align/Fixture → Inspect của Chương 7), mỗi FIFO cấu hình exposure/gain/
trigger riêng theo đúng mục 6.2 — không có tham số nào dùng chung mặc định giữa hai camera.

### 6.5.2 Băng thông GigE: khi nhiều camera chia sẻ cùng switch

Khác camera đơn, nhiều camera GigE Vision cùng cắm qua một switch mạng chia sẻ băng thông vật lý —
vượt quá băng thông sẵn có gây mất gói ảnh (ảnh lỗi, timeout `CompleteAcquire`, Chương 13 mục
13.4). `ICogAcqFifo.OwnedGigEVisionTransportParams` (`ICogAcqGigEVisionTransport`) có property
`PacketSize` — kích thước gói ảnh gửi từ camera, mặc định do VisionPro tự dò theo card mạng và
camera. Giá trị càng lớn (lý tưởng ≥ 8000, cần bật "jumbo frame" trên card mạng và switch) càng
giảm overhead — đây là tinh chỉnh cho **từng** camera riêng lẻ, không phải cơ chế để cân bằng
băng thông giữa nhiều camera chia sẻ cùng switch.

**Code 6.8 — Đọc/kiểm packet size hiện tại của một FIFO GigE.**

```csharp
ICogAcqGigEVisionTransport transport = acqTool.Operator.OwnedGigEVisionTransportParams;
if (transport != null)
{
    int currentPacketSize = transport.PacketSize;   // thường tự dò — chỉ ghi đè khi có lý do
    // transport.PacketSize = 1500; // ví dụ giảm khi mạng không hỗ trợ jumbo frame
}
```

> ⚠️ **Cảnh báo:** đừng nhầm packet size với công cụ cân bằng băng thông đa camera. **Giảm** packet
> size để "nhường chỗ" cho camera khác là phản tác dụng — gói nhỏ hơn nghĩa là nhiều gói hơn cho
> cùng một ảnh, tăng số ngắt (interrupt) CPU phải xử lý và thực tế làm tăng nguy cơ rớt gói khi tải
> cao, chứ không giảm. Cơ chế đúng để nhiều camera GigE Vision chia sẻ một switch không tranh chấp
> lẫn nhau là giãn tốc độ gửi của từng camera — qua tính năng GenICam **Inter-Packet Delay**
> (`GevSCPD`) hoặc **giới hạn băng thông trên từng link** (`DeviceLinkThroughputLimit`, tuỳ hãng
> camera có hỗ trợ), đặt qua đúng đường truy cập GenICam đã giới thiệu ở mục 6.2.3
> (`ICogGigEAccess.SetIntegerFeature`) — không phải qua `PacketSize`.

![Hình 6.3 — Nhiều camera GigE, mỗi camera một CogAcqFifoTool riêng, chia sẻ một switch mạng](../assets/ch06/hinh_6_3.png)
**Hình 6.3 — Nhiều camera GigE, mỗi camera một CogAcqFifoTool riêng, chia sẻ một switch mạng.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ khối vẽ bằng draw.io. Hai (hoặc ba) hộp "Camera GigE #1",
> "Camera GigE #2"... mỗi hộp nối một đường riêng vào một hộp trung tâm "Switch GigE (jumbo frame
> bật)", từ switch một đường duy nhất nối vào hộp "PC / VisionPro". Bên trong hộp PC, vẽ song song
> hai (hoặc ba) cột nhỏ tương ứng mỗi camera: "CogAcqFifoTool #1" → "CogToolBlock #1 (Acq → Fixture
> → Inspect)", tương tự cột #2. Ghi chú dưới sơ đồ: "Mỗi camera một ICogAcqFifo độc lập — băng
> thông switch chia sẻ giữa tất cả camera, không riêng cho từng camera".

> 🔍 **Đào sâu thêm:** cấu hình chi tiết jumbo frame, packet size, và đo băng thông thực tế nằm
> ngoài phạm vi CORE của chương này — tham khảo `GigEGuide.pdf` đi kèm bộ cài VisionPro khi triển
> khai nhiều camera thật. Về mô hình luồng xử lý khi nhiều camera cùng chạy (mỗi camera một luồng
> acquisition hay dùng chung một luồng xử lý tuần tự), xem lại nguyên tắc "một tool — một luồng"
> đã học ở Chương 13, mục 13.7 — nguyên tắc đó áp dụng cho từng `CogToolBlock` xử lý của từng
> camera, không đổi khi số camera tăng lên.

## Tổng kết chương

- `CogAcqFifoTool` không tự chụp ảnh — nó bọc một `ICogAcqFifo` (property `Operator`), truy cập
  qua frame grabber (`CogFrameGrabbers` tổng quát hoặc `CogFrameGrabberGigEs` chuyên cho GigE
  Vision) và video format cụ thể (mục 6.1).
- Exposure có property chuẩn hoá `OwnedExposureParams.Exposure` (ms); gain không có chuẩn chung,
  thường phải đặt qua GenICam feature (`ICogGigEAccess.SetIntegerFeature`, tên feature tuỳ hãng
  camera); trigger có 5 mô hình (`CogAcqTriggerModelConstants`), phổ biến nhất khi tích hợp PLC là
  `Semi`; strobe cần cả `StrobeDelay` lẫn `StrobePulseDuration` phủ đúng cửa sổ exposure — mọi thay
  đổi tham số acquisition đều cần gọi `Prepare()` sau đó (mục 6.2).
- `ICogImage` là hợp đồng tối thiểu, **không** kế thừa `IDisposable` — nhất quán với phát hiện ở
  Chương 13, mục 13.6.1; `CogImage8Grey` (mono, trường hợp phổ biến của MeoVision) và
  `CogImage24PlanarColor` (màu) mới thực thi `IDisposable` (mục 6.3).
- `CogImageFileTool` mở đúng một file, nhưng file đó có thể là một Cognex Image Database (`.cdb`)
  đóng gói nhiều ảnh — đây là cách kỹ thuật thật đứng sau "nạp cả một thư mục ảnh mẫu" đã hứa ở
  Chương 5, mục 5.3.2. Duyệt qua toàn bộ bộ ảnh bằng `NextImageIndex`/`CurrentImageIndex`/
  `ImageIndexIncrement` là hạt nhân của việc chạy thống kê trên golden set (Chương 16, mục 16.3;
  mục 6.4).
- Nhiều camera trên một máy: mỗi camera một `CogAcqFifoTool`/`ICogAcqFifo` độc lập, không tham số
  dùng chung; băng thông GigE chia sẻ giữa các camera cùng switch cần chủ động cân nhắc packet size
  (`OwnedGigEVisionTransportParams`) khi vượt quá cấu hình mặc định tự dò (mục 6.5).

## Lỗi thường gặp

**Lỗi 1 — Phát triển job bằng ảnh chụp tay thiếu chuẩn.** Hiện tượng: job "chạy tốt" lúc phát
triển nhưng ra hiện trường sai hàng loạt ngay tuần đầu. Nguyên nhân: bộ ảnh mẫu dùng để phát triển
được chụp vội bằng tay (sáng khác, góc khác, thiếu các trường hợp biên) — không phản ánh đúng dải
biến thiên thật của exposure/trigger/vị trí chi tiết sẽ gặp trên dây chuyền. Cách tránh: chụp bộ
ảnh mẫu **sau khi** đã cấu hình acquisition đúng (mục 6.2) trên chính camera/đèn sẽ dùng thật, và
chủ động chụp cả trường hợp xấu/biên, không chỉ ảnh "đẹp".

**Lỗi 2 — Quên tách bộ ảnh "golden set" khỏi ảnh phát triển thông thường.** Hiện tượng: không có
cách nào chứng minh khách quan job đã sẵn sàng nghiệm thu, chỉ có cảm giác "thử vài ảnh thấy ổn".
Nguyên nhân: dùng chung một bộ `.cdb` vừa để mò tham số lúc phát triển vừa để "chứng minh" job hoạt
động — bộ ảnh này bị thiên vị bởi chính quá trình chỉnh tham số dựa trên nó. Cách tránh: giữ riêng
một bộ ảnh **golden set** không dùng để tinh chỉnh tham số, chỉ dùng để chạy thống kê Accept/Reject
(Code 6.7) khi cần đánh giá khách quan — nguyên tắc sẽ hình thức hoá đầy đủ ở Chương 16, mục 16.3.

**Lỗi 3 — Đổi tham số acquisition từ code mà quên gọi Prepare().** Hiện tượng: set `Exposure`/
`Gain`/trigger xong, ảnh vẫn ra như cấu hình cũ, không có lỗi nào báo. Nguyên nhân: các property
trên `ICogAcqFifo` chỉ thay đổi trạng thái trong bộ nhớ; `Prepare()` mới thực sự ghi cấu hình xuống
camera (mục 6.2.2). Cách tránh: coi `Prepare()` là bước bắt buộc ngay sau mọi lần đổi tham số
acquisition, không phải bước tuỳ chọn.

**Lỗi 4 — StrobePulseDuration ngắn hơn Exposure.** Hiện tượng: ảnh thiếu sáng đều trên toàn khung
hình (camera global shutter — mặc định của sách này) dù `Exposure`/`Gain` đặt đúng theo tính toán.
Nguyên nhân: đèn tắt trước khi cảm biến phơi sáng xong (xung strobe ngắn hơn cửa sổ exposure, hoặc
`StrobeDelay` lệch); vì toàn bộ cảm biến phơi sáng chung một cửa sổ thời gian, mọi pixel đều thiếu
đúng phần ánh sáng strobe bị cắt như nhau — không phải hiện tượng "một phần khung hình sáng, một
phần tối" (đó là dấu hiệu của rolling shutter, một nguyên nhân khác — Chương 3, mục 3.3.4). Cách
tránh: luôn đặt `StrobePulseDuration ≥ Exposure − StrobeDelay` và kiểm chứng bằng timing diagram
thật (Hình 6.2), không chỉ đặt theo trực giác.

\newpage

# Chương 7 — Hệ toạ độ, calibration và fixturing

Buổi chạy thử đầu tiên của trạm gắp-đặt, mọi thứ trên màn hình vision đều xanh: PMAlign tìm thấy
chi tiết với score 0.95, toạ độ hiển thị rõ ràng, đèn OK sáng đều mỗi cycle. Nhưng robot cứ gắp
trượt — lúc lệch 1 mm, lúc lệch gần 2 mm, và điều khó hiểu nhất là **càng xa tâm ảnh càng trượt
nhiều**. Kỹ sư robot khẳng định tay máy lặp lại tốt hơn 0.02 mm. Kỹ sư vision khẳng định tool
báo đúng vị trí. Cả hai đều đúng — và hệ thống vẫn sai.

Thủ phạm nằm ở một phép nhân tưởng như vô hại trong code: `mm = pixel * 0.041`. Con số 0.041
được đo tay bằng cách chụp một cây thước, đếm pixel, chia ra. Nó chỉ đúng ở đúng chỗ cây thước
từng nằm. Ống kính có distortion nhẹ nên tỉ lệ ở mép ảnh khác ở tâm; camera lắp nghiêng chưa
đầy 1° nên tỉ lệ theo trục X khác trục Y; và không ai chứng minh được gốc toạ độ của "hệ mm"
đó trùng với gốc mà robot đang dùng. Pixel chưa bao giờ được *dạy* nghĩa là bao nhiêu milimet —
nó chỉ bị *gán* một cách ngây thơ.

Chương này giải quyết tận gốc vấn đề đó. Chúng ta sẽ đi qua cách VisionPro tổ chức các không
gian toạ độ trên một tấm ảnh (mục 7.1), hai công cụ dạy hệ thống quy đổi pixel ↔ đơn vị thực —
CogCalibNPointToNPointTool cho quan hệ tuyến tính (mục 7.2) và CogCalibCheckerboardTool khi cần
khử distortion (mục 7.3) — rồi đến fixturing, kỹ thuật làm ROI của mọi tool kiểm tra "bám theo"
chi tiết đặt lệch (mục 7.4). Cuối chương là bài toán tổng hợp: gửi toạ độ cho robot sao cho
đúng ngay từ nguyên lý (mục 7.5) và một cái nhìn khái quát về hand-eye calibration (mục 7.6).
Đây là chương xương sống của cả cuốn sách: PMAlign (Chương 8), đo lường (Chương 9) hay giao tiếp
robot (Chương 15) đều đứng trên nền các khái niệm ở đây.

## 7.1 Hai thế giới: không gian ảnh và không gian thực

### 7.1.1 Pixel nói dối như thế nào

Hãy bắt đầu bằng ba sự thật khó chịu về tấm ảnh 2448 × 2048 px mà camera của trạm MeoVision
gửi về (thông số trạm: FOV 100.0 × 83.7 mm, tỉ lệ danh nghĩa 0.0409 mm/px):

1. **Tỉ lệ không đều trên toàn ảnh.** Ống kính nào cũng có distortion — thấu kính là mặt cong,
   ảnh là mặt phẳng. Với lens chất lượng tốt, sai lệch giữa tâm và mép ảnh có thể chỉ vài phần
   nghìn; nghe nhỏ, nhưng vài phần nghìn của FOV 100 mm là vài chục micromet — cùng bậc với
   dung sai ± 0.05 mm mà trạm phải đo.
2. **Trục ảnh không trùng trục thế giới.** Camera lắp nghiêng 1° so với mặt phẳng pallet là
   chuyện bình thường của cơ khí. Khi đó một milimet theo phương X của pallet không còn ứng với
   đúng một số pixel cố định theo phương X của ảnh.
3. **Gốc toạ độ ảnh vô nghĩa với thế giới.** Điểm (0, 0) của ảnh là góc trên-trái cảm biến —
   không liên quan gì đến gốc pallet hay gốc robot. Và trục Y của ảnh **hướng xuống**, ngược
   với quy ước toán học thông thường mà đa số robot sử dụng.

Phép nhân `pixel * 0.041` bỏ qua cả ba điều trên. Calibration — hiệu chuẩn — là quá trình đo
và mô hình hoá chúng một cách có phương pháp, để từ đó mọi kết quả vision được phát biểu thẳng
bằng milimet, trong một hệ toạ độ có gốc và hướng do chúng ta *chọn*.

### 7.1.2 Coordinate space tree — cách VisionPro tổ chức các hệ toạ độ

Điều làm VisionPro khác các thư viện xử lý ảnh "thuần" (như OpenCV) là: **hệ toạ độ là công dân
hạng nhất, gắn liền với từng tấm ảnh**. Mỗi `ICogImage` mang theo một *cây không gian toạ độ*
(coordinate space tree, kiểu `CogCoordinateSpaceTree`). Gốc cây là root space; pixel space là
một nút trong cây; và mỗi lần calibration hay fixturing, một không gian mới được *treo thêm*
vào cây kèm phép biến đổi nối nó với nút cha.

Ba tên không gian đặc biệt cần thuộc lòng:

**Bảng 7.1 — Các tên không gian toạ độ đặc biệt trong VisionPro.**

| Tên | Ý nghĩa | Ghi chú |
|---|---|---|
| `#` | **Pixel space** — hệ toạ độ pixel gốc của ảnh | Gốc tại góc trên-trái, X sang phải, Y **hướng xuống**, đơn vị pixel |
| `@` | **Root space** — gốc của cây không gian | Các không gian khác đều nối về đây trực tiếp hoặc gián tiếp |
| `.` | Alias của **selected space** — không gian "đang được chọn" của ảnh | Tên thật nằm trong property `SelectedSpaceName` của ảnh |

*Selected space* là khái niệm then chốt: đó là không gian mà **mọi tool mặc định dùng để nhận
ROI và trả kết quả**. Khi ảnh vừa ra khỏi camera, selected space chính là pixel space — vì thế
kết quả tool là pixel. Sau khi tool calibration chạy, ảnh output có selected space là không gian
đã hiệu chuẩn — và **cùng một tool đo, không đổi một dòng code, giờ trả kết quả bằng milimet**.
Đây là lý do trong VisionPro, calibration không phải một bước "hậu xử lý số liệu" mà là một mắt
xích nằm ngay trong chuỗi ảnh.

![Hình 7.1 — Cây không gian toạ độ của một ảnh sau calibration và fixturing](../assets/ch07/hinh_7_1.png)
**Hình 7.1 — Cây không gian toạ độ của một ảnh sau calibration và fixturing.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ cây (vẽ draw.io), gốc là nút `@ (root)`; con thứ nhất
> `# (pixel)` kèm chú thích "gốc góc trên-trái, Y xuống, đơn vị px"; con thứ hai `PalletMM`
> kèm nhãn mũi tên "transform từ calibration (mục 7.2/7.3)" và chú thích "đơn vị mm, gốc pallet";
> con của `PalletMM` là `PartFixture` kèm nhãn "transform từ CogFixtureTool (mục 7.4)". Bên cạnh
> cây, một hộp nhỏ ghi `SelectedSpaceName = "PartFixture"` với mũi tên trỏ vào nút tương ứng.

Muốn tự tay quy đổi một điểm giữa hai không gian bất kỳ trong cây, chúng ta hỏi cây lấy phép
biến đổi rồi áp nó lên điểm — hai method `GetTransform` và `MapPoint` sẽ xuất hiện trong
Code 7.4 ở mục 7.5. Nhưng phần lớn thời gian không cần làm thủ công: cứ để tool đọc/ghi theo
selected space, cây toạ độ lo phần còn lại.

> 📌 **Lưu ý:** distortion của ống kính là biến dạng *phi tuyến*, nên nghiêm ngặt mà nói không
> thể biểu diễn trọn vẹn bằng một phép biến đổi tuyến tính giữa hai không gian. VisionPro xử lý
> việc này ở tool checkerboard (mục 7.3) bằng mô hình phi tuyến gắn trong không gian hiệu chuẩn.
> Với lens tốt và yêu cầu vừa phải, mô hình tuyến tính của mục 7.2 là đủ — Bảng 7.2 sẽ giúp chọn.

## 7.2 Calibration tỉ lệ: CogCalibNPointToNPointTool

### 7.2.1 Nguyên lý: N cặp điểm

`CogCalibNPointToNPointTool` (namespace `Cognex.VisionPro.CalibFix`) học phép biến đổi tuyến tính
từ **N cặp điểm tương ứng**: mỗi cặp gồm toạ độ *uncalibrated* (pixel — đo được trên ảnh) và toạ
độ *raw calibrated* (mm — biết trước từ bản vẽ cơ khí hoặc do robot dạy). Từ N cặp đó, tool giải
ra bộ tham số: tỉ lệ theo X/Y, góc xoay, tịnh tiến — và cả *đổi chiều tay* (handedness) để xử lý
chuyện trục Y ảnh hướng xuống.

Về số lượng điểm: mô hình tuyến tính đầy đủ chỉ cần 3 cặp điểm không thẳng hàng để giải, nhưng
3 điểm thì "vừa khít" — không dư dữ liệu để phát hiện điểm nhập sai. Thực tế hiện trường dùng
**4 điểm trở lên, trải rộng hết vùng làm việc**, để tool giải theo bình phương tối thiểu và cho
ra một con số quý giá: `ComputedRMSError` — sai số quân phương giữa các điểm sau khi áp mô hình.
RMS lớn nghĩa là hoặc điểm nhập sai, hoặc quan hệ thực tế không còn tuyến tính (distortion lớn) —
cả hai đều phải xử lý trước khi cho trạm chạy.

Với trạm MeoVision, chúng ta dùng 4 lỗ định vị có sẵn trên pallet, toạ độ cơ khí theo bản vẽ là
(10, 10), (90, 10), (90, 70), (10, 70) mm trong hệ pallet. Vị trí pixel của từng lỗ do một tool
tìm tâm lỗ cung cấp (CogFindCircleTool — Chương 9; ở QuickBuild giai đoạn setup có thể click tay
trên ảnh để lấy).

> 📌 **Lưu ý:** Code 7.1 dưới đây minh hoạ bằng toạ độ **bản vẽ**, vì đây là cách dễ tái hiện nhất
> trên giấy. Trạm MeoVision thật lại dùng cách **robot chạm** (mục 7.5.1) — bốn giá trị mm ở
> SetRawCalibratedPointX/Y khi đó không phải số bản vẽ mà là toạ độ robot đọc được lúc chạm từng
> lỗ. Cấu trúc code giống hệt nhau, chỉ khác nguồn của bốn con số mm; mục 7.5.1 giải thích vì sao
> MeoVision chọn cách robot chạm.

![Hình 7.2 — Bốn điểm calibration trải rộng vùng làm việc trên pallet](../assets/ch07/hinh_7_2.png)
**Hình 7.2 — Bốn điểm calibration trải rộng vùng làm việc trên pallet.**
> 🖼 MÔ TẢ HÌNH: ảnh chụp pallet thật (hoặc screenshot QuickBuild) thấy 4 lỗ định vị ở 4 góc
> vùng làm việc; overlay chữ thập đánh dấu tâm từng lỗ kèm nhãn kép "px: (312, 1755) /
> mm: (10, 10)" (giá trị px minh hoạ); vẽ khung nét đứt nối 4 điểm để nhấn "trải rộng — không
> dồn về một góc".

### 7.2.2 Thiết lập bằng code

**Code 7.1 — Dạy CogCalibNPointToNPointTool bằng 4 cặp điểm, kiểm RMS gộp VÀ từng điểm riêng lẻ.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;

// (Trong QuickBuild, các bước dưới đây tương ứng thao tác trên tab Calibration của tool)
var calibTool = new CogCalibNPointToNPointTool();
calibTool.InputImage = rawImage;                   // ICogImage từ camera, selected space = "#"

CogCalibNPointToNPoint calib = calibTool.Calibration;
calib.NumPoints = 4;

// Cặp điểm i: (uncalibrated px) ↔ (raw calibrated mm — toạ độ bản vẽ pallet)
// Giá trị px lấy từ tool tìm tâm lỗ ở bước setup
calib.SetUncalibratedPointX(0, 312.4);  calib.SetUncalibratedPointY(0, 1755.2);
calib.SetRawCalibratedPointX(0, 10.0);  calib.SetRawCalibratedPointY(0, 10.0);

calib.SetUncalibratedPointX(1, 2268.1); calib.SetUncalibratedPointY(1, 1751.8);
calib.SetRawCalibratedPointX(1, 90.0);  calib.SetRawCalibratedPointY(1, 10.0);

calib.SetUncalibratedPointX(2, 2264.7); calib.SetUncalibratedPointY(2, 285.5);
calib.SetRawCalibratedPointX(2, 90.0);  calib.SetRawCalibratedPointY(2, 70.0);

calib.SetUncalibratedPointX(3, 309.0);  calib.SetUncalibratedPointY(3, 289.1);
calib.SetRawCalibratedPointX(3, 10.0);  calib.SetRawCalibratedPointY(3, 70.0);

calib.Calibrate();                                  // giải mô hình từ N cặp điểm

if (!calib.Calibrated || calib.ComputedRMSError > 0.5)
    throw new InvalidOperationException(
        $"Calibration không đạt: RMS = {calib.ComputedRMSError:F3}px");

// RMS GỘP có thể "trốn" một điểm lệch xa nếu các điểm còn lại rất khớp (N ≥ 4 điểm, một điểm tệ
// bị pha loãng trong trung bình). Kiểm TỪNG điểm riêng lẻ trước khi chấp nhận — dùng chính transform
// vừa giải để "dự đoán" lại vị trí uncalibrated từ raw calibrated, so với vị trí đã nhập lúc đầu.
const double maxPerPointResidualPx = 0.5;   // CÙNG ngưỡng với RMS gộp — áp cho từng điểm thay vì
                                             // chỉ áp cho trung bình, xem giải thích dưới
ICogTransform2D uncalFromRawCalib = calib.GetComputedUncalibratedFromRawCalibratedTransform();
for (int i = 0; i < calib.NumPoints; i++)
{
    calib.GetRawCalibratedPoint(i, out double rawX, out double rawY);
    uncalFromRawCalib.MapPoint(rawX, rawY, out double predictedX, out double predictedY);
    calib.GetUncalibratedPoint(i, out double actualX, out double actualY);

    double dx = predictedX - actualX, dy = predictedY - actualY;
    double residualPx = Math.Sqrt(dx * dx + dy * dy);
    logger.LogDebug("Điểm calib {Index}: lệch {ResidualPx:F3}px", i, residualPx);

    if (residualPx > maxPerPointResidualPx)
        throw new InvalidOperationException(
            $"Điểm calib {i} lệch {residualPx:F3}px — vượt {maxPerPointResidualPx}px dù RMS gộp " +
            "có thể vẫn đạt. Kiểm tra lại đúng điểm này (nhập sai toạ độ, hoặc tấm chuẩn bẩn/xước " +
            "đúng vị trí đó) trước khi chấp nhận calibration.");
}

// Từ đây, mỗi lần chạy tool sẽ gắn không gian mm vào ảnh:
calibTool.Run();
ICogImage calibratedImage = calibTool.OutputImage; // selected space = không gian đã hiệu chuẩn
```

`ComputedRMSError` tính theo **uncalibrated space** — tức đơn vị pixel, KHÔNG PHẢI mm (không gian
đã hiệu chuẩn) — xác nhận trực tiếp từ mô tả API của Cognex, không còn là giả định cần kiểm chứng
thêm như bản nháp trước của chương này từng ghi. Chạy với số liệu trên, trạm mẫu đạt RMS 0.18px —
dưới ngưỡng 0.5px mà chúng ta tự đặt cho trạm (quy ước nội bộ: RMS vượt ngưỡng thì từ chối đưa
trạm vào chạy, không "châm chước"). Chú ý cách kiểm tra `Calibrated` và RMS **nằm trong code**,
không phải bước kiểm tay lúc setup rồi thôi: calibration là dữ liệu sống, nó sẽ được làm lại mỗi
lần thay camera, thay lens, va chạm cơ khí — và lần làm lại vội vàng lúc 2 giờ sáng chính là lần
dễ nhập sai điểm nhất.

Vì sao cùng một ngưỡng 0.5px lại cần áp HAI LẦN (một lần cho RMS gộp, một lần cho từng điểm) thay
vì chỉ cần một lần: RMS là căn bậc hai trung bình BÌNH PHƯƠNG của N điểm, nên một điểm lệch hẳn có
thể bị "pha loãng" bởi các điểm còn lại rất khớp. Ví dụ minh hoạ (KHÁC bộ số liệu thật của Code
7.1 ở trên, chỉ để thấy cơ chế "pha loãng"): với cùng N = 4 điểm, nếu 3 điểm gần như hoàn hảo (lệch
~0.05px mỗi điểm) và đúng 1 điểm lệch tới 0.9px, RMS gộp tính ra
`√((3×0.05² + 0.9²)/4) ≈ 0.45px` — **vẫn dưới ngưỡng 0.5px, calibration "đạt"** — trong khi chính
điểm thứ 4 đó lệch gần gấp đôi ngưỡng, đủ tệ để đáng điều tra riêng (nhập sai toạ độ điểm đó, hoặc
tấm chuẩn bẩn/xước đúng vị trí đó). Đây chính xác là điều RMS gộp có thể bỏ lọt mà kiểm per-point
bắt được, và cũng là dạng cụ thể, phát hiện SỚM hơn của tình huống "Lỗi 2" cuối chương (điểm chuẩn
dồn một góc — per-point residual cao bất thường ở đúng những điểm dồn góc đó là dấu hiệu cảnh báo
trước khi vùng ngoại suy kịp gây sai số đo thực tế).

> 📌 **Lưu ý:** tool trong các đoạn mã của chương này là thành viên *sống lâu* của trạm — tạo
> một lần lúc khởi động, tái sử dụng mỗi cycle, và Dispose khi trạm dừng. Ví dụ ở đây lược phần
> quản lý vòng đời để tập trung vào calibration; bức tranh đầy đủ về tài nguyên VisionPro (ảnh,
> tool, display, license) nằm ở Chương 13, mục 13.6.

> 💡 **Mẹo thực chiến:** đừng lấy 4 điểm túm tụm một góc ảnh. Mô hình tuyến tính "đúng nhất"
> quanh vùng có dữ liệu; phần ảnh không có điểm calibration nào là phần mô hình *ngoại suy* —
> và ngoại suy là nơi sai số phình to. Nguyên tắc: vùng làm việc thật của tool nằm gọn trong
> đa giác tạo bởi các điểm calibration.

### 7.2.3 Khi nào tuyến tính là đủ

Mô hình tuyến tính gánh được: tỉ lệ (khác nhau theo X/Y), xoay, tịnh tiến, đổi chiều tay. Nó
**không** gánh được: distortion của lens, hiệu ứng phối cảnh khi camera không vuông góc với mặt
phẳng làm việc. Với trạm bắt vị trí cho robot (dung sai ± 0.1 mm trên FOV 100 mm) và lens tốt,
tuyến tính thường đủ. Với trạm *đo kích thước* dung sai ± 0.05 mm dùng toàn bộ FOV, hãy sang
mục 7.3 — hoặc cân nhắc telecentric lens ngay từ đầu (đã bàn ở Chương 2, mục 2.5).

## 7.3 CogCalibCheckerboardTool — hiệu chuẩn phi tuyến bằng bàn cờ

### 7.3.1 Vì sao lại là bàn cờ

`CogCalibCheckerboardTool` nhận một tấm ảnh chụp **bàn cờ chuẩn** — lưới ô vuông đen trắng có
kích thước ô biết trước — và tự tìm hàng trăm giao điểm ô cờ trên ảnh. Mỗi giao điểm là một cặp
điểm calibration: vị trí pixel (đo trên ảnh) ↔ vị trí mm (suy từ chỉ số hàng-cột × kích thước ô).
Bản chất vẫn là "N cặp điểm" như mục 7.2, nhưng N giờ là hàng trăm và phủ kín FOV — đủ dày để
tool khớp được **mô hình phi tuyến**, mô tả cả distortion của lens lẫn phối cảnh do camera nghiêng.

Bàn cờ của trạm MeoVision dùng ô 5 × 5 mm in trên đế nhôm phẳng — con số này chọn theo đúng FOV
của trạm (không phải mặc định dùng chung cho mọi camera): FOV nhỏ/mm-trên-pixel mịn cần ô nhỏ hơn
để đủ số giao điểm phủ kín ảnh, FOV lớn cần ô lớn hơn để mỗi ô vẫn đủ pixel mà nhận dạng chính xác.
Ba yêu cầu chất lượng quan trọng hơn mọi thông số khác: **phẳng** (bàn cờ giấy dán cong queo phá
hỏng chính thứ ta muốn đo), **đặt đúng cao độ mặt làm việc** (calibration chỉ đúng tại mặt phẳng
nó được chụp — đặt bàn cờ trên mặt băng tải rồi đo chi tiết cao 8 mm phía trên là đã đổi mặt
phẳng), và **phủ kín vùng đo** (vùng không có ô cờ = vùng ngoại suy, như mẹo ở mục 7.2).

Nhiều bàn cờ chuẩn có thêm **fiducial** — dấu mốc xác định gốc và hướng (bàn cờ Cognex dùng
mã DataMatrix ở tâm). Có fiducial, không gian mm sau hiệu chuẩn có gốc/hướng lặp lại được mỗi
lần calibration lại; không có, gốc do tool tự chọn và có thể khác đi giữa hai lần dạy.

![Hình 7.3 — Ảnh bàn cờ calibration và lưới giao điểm tool tìm được](../assets/ch07/hinh_7_3.png)
**Hình 7.3 — Ảnh bàn cờ calibration và lưới giao điểm tool tìm được.**
> 🖼 MÔ TẢ HÌNH: screenshot QuickBuild tab calibration của CogCalibCheckerboardTool sau khi
> Calibrate: ảnh bàn cờ với overlay chữ thập xanh tại các giao điểm tìm được, fiducial
> DataMatrix ở giữa; góc hình chú thích giá trị RMS hiển thị trên tool. Chụp bằng job mẫu
> trong sdk/samples_quickbuild (bộ ảnh bàn cờ đi kèm SDK).

### 7.3.2 Thiết lập và vòng đời hai giai đoạn

Tool checkerboard có hai "chế độ sống" tách bạch — hiểu rõ điều này tránh được một lỗi cấu hình
kinh điển:

- **Giai đoạn DẠY (một lần, lúc setup):** đưa ảnh bàn cờ vào phần calibration của tool, khai báo
  kích thước ô, chọn mô hình, bấm Calibrate. Kết quả (mô hình biến đổi) được lưu cùng tool trong
  file .vpp.
- **Giai đoạn CHẠY (mỗi cycle):** ảnh sản xuất (không còn bàn cờ!) đi qua tool; tool chỉ việc
  gắn không gian đã hiệu chuẩn vào ảnh rồi chuyển tiếp. Chi phí mỗi cycle gần như bằng không.

> 📌 **Đã xác nhận trên SDK thật (VisionPro 9.0 CR2, XML doc chính thức của
> `CogCalibCheckerboardTool.Run`):** *"You must call Calibrate before attempting to run this
> tool"* — `Run()` **không tự động gọi `Calibrate()`**. Nếu gọi `Run()` trước khi `Calibrate()`
> đã chạy thành công, tool ném `CogToolNoOperatorException`. Đúng như mẫu NPoint đã học ở mục 7.2:
> ứng dụng phải tự gọi `cb.Calibrate()` tường minh trước, rồi mới `cbTool.Run()`.

**Code 7.2 — Dạy CogCalibCheckerboardTool từ ảnh bàn cờ.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;

var cbTool = new CogCalibCheckerboardTool();
CogCalibCheckerboard cb = cbTool.Calibration;

cb.CalibrationImage = checkerboardImage;   // ảnh chụp bàn cờ đặt tại mặt làm việc
cb.PhysicalTileSizeX = 5.0;                // mm — kích thước ô theo bản vẽ bàn cờ
cb.PhysicalTileSizeY = 5.0;

// Mô hình: Linear (tuyến tính) hoặc PerspectiveAndRadialWarp (phi tuyến đầy đủ, khử distortion)
cb.ComputationMode = CogCalibFixComputationModeConstants.PerspectiveAndRadialWarp;

cb.Calibrate();                            // BẮT BUỘC — Run() không tự gọi hộ, thiếu dòng này
                                            // Run() ném CogToolNoOperatorException

if (!cb.Calibrated || cb.ComputedRMSError > 0.5)
    throw new InvalidOperationException(
        $"Checkerboard calibration không đạt: RMS = {cb.ComputedRMSError:F3}");

// Sản xuất: mỗi cycle chỉ việc
cbTool.InputImage = productionImage;
cbTool.Run();
ICogImage measurableImage = cbTool.OutputImage;   // selected space đã là mm, đã khử distortion
```

### 7.3.3 Chọn công cụ nào?

**Bảng 7.2 — Chọn phương án calibration cho trạm 2D.**

| Phương án | Gánh được gì | Không gánh được gì | Dùng khi |
|---|---|---|---|
| Hệ số tỉ lệ tay (`px × k`) | Tỉ lệ đều, đúng tại chỗ đo k | Mọi thứ còn lại | Chỉ chấp nhận được cho demo/độ chính xác thấp — sách này coi là anti-pattern |
| NPointToNPoint (7.2) | Tỉ lệ X/Y, xoay, tịnh tiến, handedness; gốc do ta chọn | Distortion, phối cảnh | Bắt vị trí cho robot; lens tốt; điểm chuẩn cơ khí/robot có sẵn — **gốc trùng hệ robot được luôn** |
| Checkerboard (7.3) | Như trên + distortion + phối cảnh; hàng trăm điểm, RMS tin cậy | Đổi mặt phẳng làm việc; không tự cho gốc trùng robot (gốc theo fiducial bàn cờ) | Đo kích thước toàn FOV; lens thường; camera khó lắp vuông góc |

Hai công cụ không loại trừ nhau: một mẫu triển khai phổ biến là **checkerboard để khử distortion
+ NPoint chồng phía trên để xoay gốc về hệ robot**. Với MeoVision, trạm bắt vị trí dùng NPoint
trực tiếp theo 4 lỗ pallet (gốc trùng hệ pallet mà robot cũng được dạy theo); trạm đo kích thước
ở Chương 9 sẽ dùng checkerboard.

> ⚠️ **Cảnh báo:** calibration gắn với **mặt phẳng vật lý** lúc chụp bàn cờ/điểm chuẩn. Đo đỉnh
> chi tiết cao 8 mm bằng calibration dạy tại mặt pallet là đã cộng thêm sai số phối cảnh tỉ lệ
> với 8 mm × khoảng lệch tâm quang học. Chi tiết càng cao, camera càng gần, sai càng lớn. Quy
> tắc: dạy calibration tại **đúng cao độ của đặc trưng sẽ đo**, hoặc dùng telecentric lens.

## 7.4 Fixturing — cho ROI bám theo chi tiết

### 7.4.1 Vấn đề: chi tiết không bao giờ nằm yên một chỗ

Chi tiết của MeoVision đến trên pallet với xê dịch tới ± 5 mm và xoay ± 10°. Trong khi đó mọi
tool kiểm tra đều làm việc trong một ROI — vùng ảnh được phép xử lý. Nếu ROI đứng yên (vẽ chết
lúc setup), chi tiết xê dịch là đặc trưng cần kiểm trôi ra khỏi ROI: caliper đo cạnh vớ phải
bóng đổ, blob đếm linh kiện đếm sang ô bên cạnh. Tăng ROI thật to để "chắc ăn" thì vừa chậm
vừa rước nhiễu nền vào kết quả.

Lời giải của VisionPro thanh lịch hơn nhiều: **fixturing**. Một tool định vị (thường là PMAlign)
tìm chi tiết và trả về *pose* — vị trí + góc xoay. `CogFixtureTool` nhận pose đó và treo thêm
vào cây toạ độ của ảnh một không gian mới — *fixtured space* — có gốc gắn chặt vào chi tiết,
rồi đặt nó làm selected space của ảnh output. Mọi tool phía sau vẽ ROI **trong không gian đó**:
chi tiết trượt đi đâu, xoay bao nhiêu, ROI theo đến đó. Không một dòng code dịch chuyển ROI nào
phải viết.

Từ đây hình thành **kiến trúc job chuẩn** mà chúng ta sẽ gặp lại suốt Phần III — bốn tầng, thứ
tự bất di bất dịch:

```text
Acquisition  →  Calibration  →  Align + Fixture  →  các tool Inspect/Measure/Read
 (ảnh px)       (gắn mm)        (gắn hệ chi tiết)    (ROI vẽ trong hệ chi tiết,
                                                      kết quả tính bằng mm)
```

![Hình 7.4 — ROI tĩnh bị chi tiết "trốn" và ROI fixtured bám theo chi tiết](../assets/ch07/hinh_7_4.png)
**Hình 7.4 — ROI tĩnh bị chi tiết "trốn" (trái) và ROI fixtured bám theo chi tiết (phải).**
> 🖼 MÔ TẢ HÌNH: hai khung hình cùng một cảnh, chi tiết vỏ nhôm đặt lệch + xoay ~8° so với ảnh
> mẫu. Trái: ROI chữ nhật đứng yên (nét đỏ) — mép chi tiết lòi khỏi ROI, caliper trỏ vào nền.
> Phải: cùng ROI nhưng vẽ trong fixtured space (nét xanh) — khung ROI xoay + trượt theo chi
> tiết, đặc trưng cần đo nằm gọn giữa ROI. Chụp từ QuickBuild bằng job mẫu Fixture trong
> sdk/samples_quickbuild với 2 ảnh input khác vị trí.

### 7.4.2 Lắp chuỗi Align → Fixture bằng code

Trong QuickBuild, chuỗi này chỉ là mấy đường nối terminal (kéo `GetPose()` của kết quả PMAlign
thả vào `RunParams.UnfixturedFromFixturedTransform` của fixture tool — trình soạn thảo tự gợi ý).
Nhìn nó dưới dạng code một lần để hiểu điều gì thật sự chảy qua các đường nối đó:

**Code 7.3 — Chuỗi định vị PMAlign → CogFixtureTool (kèm kiểm độ lệch hợp lý vật lý); caliper phía sau đo trong hệ chi tiết.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.PMAlign;

// 1. Định vị chi tiết trên ảnh ĐÃ calibration (selected space = "PalletMM")
pmAlign.InputImage = calibratedImage;
pmAlign.Run();
if (pmAlign.Results.Count == 0)
    return StationResult.NotFound;           // không thấy chi tiết — không có gì để fixture

CogPMAlignResult found = pmAlign.Results[0]; // kết quả tốt nhất (Chương 8 bàn kỹ score/sort)

// 1b. Score cao (Accepted) KHÔNG đồng nghĩa vị trí tìm được hợp lý về vật lý — pose có thể bắt
//     nhầm một đặc trưng tương tự ở chỗ khác trong khung hình. Kiểm độ lệch so với vị trí danh
//     nghĩa nằm trong phạm vi xê dịch cơ khí THẬT đã biết (± 5mm/± 10°, mục 7.4.1) — lớp phòng vệ
//     KHÁC hẳn Score (Score đo "hình dạng khớp không", ngưỡng này đo "vị trí có hợp lý không").
CogTransform2DLinear pose = found.GetPose();
const double maxOffsetMm = 5.0, maxRotationDeg = 10.0;    // đúng dải xê dịch đã nêu ở 7.4.1
double rotationDeg = pose.Rotation * 180.0 / Math.PI;
if (Math.Abs(pose.TranslationX) > maxOffsetMm || Math.Abs(pose.TranslationY) > maxOffsetMm
    || Math.Abs(rotationDeg) > maxRotationDeg)
{
    logger.LogWarning(
        "PMAlign tìm thấy (Score={Score:F3}) nhưng lệch {X:F1}/{Y:F1}mm, xoay {R:F1}° — " +
        "vượt phạm vi cơ khí hợp lý, coi như không tin cậy.", found.Score, pose.TranslationX,
        pose.TranslationY, rotationDeg);
    return StationResult.NotFound;   // KHÔNG dùng pose này để fixture — fallback, không "châm chước"
}

// 2. Gắn fixtured space từ pose của chi tiết
fixtureTool.InputImage = calibratedImage;
fixtureTool.RunParams.UnfixturedFromFixturedTransform = pose;
fixtureTool.Run();
ICogImage fixturedImage = fixtureTool.OutputImage;
// fixturedImage.SelectedSpaceName giờ là fixtured space — gốc nằm trên chi tiết

// 3. Tool đo phía sau nhận ảnh fixtured — ROI của nó đã được vẽ (lúc setup)
//    trong hệ chi tiết, nên tự "bám" theo mọi xê dịch/xoay
caliper.InputImage = fixturedImage;
caliper.Run();
```

Bốn chi tiết đáng dừng lại:

- **Fixture chạy trên ảnh đã calibration.** Pose PMAlign trả về khi đó tính bằng mm trong hệ
  pallet, nên fixtured space cũng "nói tiếng mm". Đo trong fixtured space cho luôn kết quả mm —
  toàn bộ sức mạnh của cây toạ độ nằm ở sự xếp tầng này.
- **Nhánh "không tìm thấy" phải được xử lý tường minh.** Không có kết quả PMAlign mà vẫn cho
  chuỗi chạy tiếp là các tool sau đo trên ROI đặt theo pose *của cycle trước* — kiểu lỗi ra kết
  quả "có vẻ hợp lý" khó phát hiện nhất.
- **"Tìm thấy" (Accepted) không đồng nghĩa "vị trí hợp lý".** Score cao đo hình dạng có khớp
  pattern không — nó không đo được vị trí khớp đó có nằm trong phạm vi vật lý chi tiết THẬT SỰ có
  thể xê dịch tới hay không. Zone tìm kiếm đặt quá rộng, hoặc khung hình có một đặc trưng tương tự
  ở chỗ khác, có thể cho Score cao tại một vị trí vô lý. Bước 1b ở Code 7.3 thêm đúng lớp phòng vệ
  thứ ba này — sau `Results.Count` (có tìm thấy không) và `Score` (hình dạng khớp không) — bằng
  cách kiểm độ lệch trả về có nằm trong dải xê dịch cơ khí đã biết của chi tiết (mục 7.4.1) hay
  không, coi như không tìm thấy nếu vượt dải đó dù `Accepted = true`.
- **Pattern PMAlign nên train với origin tại datum của chi tiết** (giao hai cạnh chuẩn, tâm lỗ
  định vị...). Origin của pattern trở thành gốc của fixtured space; đặt nó vào điểm mà bản vẽ
  lấy làm chuẩn thì mọi ROI, mọi kích thước đo sau này đọc thẳng theo bản vẽ — không cần cộng
  trừ offset thủ công. (Cách train pattern: Chương 8, mục 8.2.)

> 💡 **Mẹo thực chiến:** khi job chạy sai sau fixturing, mở CogRecordDisplay xem record của
> fixture tool (Chương 5, mục 5.5): overlay trục toạ độ fixtured space có nằm đúng datum chi
> tiết không, xoay có khớp chi tiết không. Nhìn một cái biết ngay lỗi ở align hay ở tool đo —
> nhanh hơn mọi phép đoán từ số liệu.

## 7.5 Truyền toạ độ cho robot

### 7.5.1 Hệ quy chiếu chung — quyết định từ trước khi viết dòng code nào

Vision báo "chi tiết ở (57.30, 41.87) mm, xoay 3.2°". Robot cần biết: **trong hệ toạ độ nào?**
Mọi rắc rối giao tiếp vision-robot quy về một câu hỏi đó. Chiến lược sạch nhất — và là chiến
lược MeoVision dùng — là **cho không gian calibrated trùng hẳn với một hệ mà robot biết**:

1. Robot (đã gá tool nhọn) chạm lần lượt 4 lỗ định vị pallet, ghi toạ độ 4 điểm **trong hệ robot**.
2. Bốn toạ độ đó — thay vì toạ độ bản vẽ — được nhập làm *raw calibrated points* trong Code 7.1.
3. Từ đó, không gian calibrated của ảnh **chính là** hệ robot: vision nói (57.30, 41.87) thì
   robot đến (57.30, 41.87). Không ma trận chuyển đổi trung gian, không "hệ số chỉnh tay" nào
   giấu trong PLC.

So với nhập toạ độ bản vẽ: cách này *nuốt* luôn sai số gá đặt pallet-so-với-robot vào trong
calibration, đổi lại hệ toạ độ không còn ý nghĩa cơ khí độc lập (thay robot phải dạy lại).
Trạm nào robot và camera cùng cố định — như MeoVision — thì đây là đánh đổi gần như luôn có lợi.

Còn hai điểm kỹ thuật hay gây lỗi âm thầm, chốt luôn thành quy ước:

- **Chiều tay của hệ.** Trục Y pixel hướng xuống; hệ robot là hệ tay phải thông thường. NPoint
  xử lý được — property `SwapCalibratedHandedness` — miễn là các cặp điểm được nhập nhất quán.
  Sau calibration, *bắt buộc* làm phép thử: đưa chi tiết dịch +10 mm theo X robot, kiểm tra số
  vision báo tăng đúng chiều, lặp cho Y. Ba phút thử tay này bắt được lỗi mà nhìn code không thấy.
- **Đơn vị góc.** Kết quả góc từ pose của VisionPro tính bằng **radian** (xác nhận từ tài liệu
  chính thức: `CogTransform2DLinear.Rotation` luôn ở radian); đa số robot nhận độ. Quên nhân
  `180 / Math.PI` là robot xoay tay gắp đi một góc "ngẫu nhiên" — lỗi có thật, gặp nhiều hơn bạn
  nghĩ.

### 7.5.2 Từ kết quả tool đến bản tin gửi robot

**Code 7.4 — Quy đổi tường minh giữa hai không gian và đóng gói kết quả cho robot.**

```csharp
using Cognex.VisionPro;

// Kết quả pose từ chuỗi Align (Code 7.3) — đã ở không gian calibrated "PalletMM"
// (trùng hệ robot theo cách dạy ở 7.5.1)
CogTransform2DLinear pose = found.GetPose();
double xMm     = pose.TranslationX;
double yMm     = pose.TranslationY;
double angleDeg = pose.Rotation * 180.0 / Math.PI;   // robot nhận độ

// Khi cần tự quy đổi một điểm bất kỳ giữa hai không gian trong cây (ví dụ debug
// một toạ độ pixel): hỏi cây toạ độ của ảnh lấy transform rồi map điểm.
// LƯU Ý thứ tự tham số: GetTransform(toSpaceName, fromSpaceName) — "đến" đứng TRƯỚC "từ",
// ngược trực giác đọc trái-sang-phải thông thường, dễ viết nhầm nếu không nhớ kỹ.
ICogTransform2D pxToMm = fixturedImage.CoordinateSpaceTree.GetTransform(
    "PalletMM",                            // đến: không gian calibrated
    "#");                                  // từ: pixel space
double xChk, yChk;
pxToMm.MapPoint(1224.0, 1024.0, out xChk, out yChk);  // tâm ảnh nằm đâu trong hệ robot?

// Đóng gói cho robot — LUÔN kèm cờ hợp lệ và kiểm tra biên (bàn kỹ ở Chương 15)
// Biên kiểm tra PHẢI khớp đa giác 4 điểm calibration (10,10)-(90,10)-(90,70)-(10,70) ở Code 7.1
// — không được rộng hơn, vì ngoài đa giác đó mô hình là ngoại suy (mục 7.2.2), sai số không
// còn được kiểm soát; gửi valid=true cho vùng ngoại suy là đúng loại lỗi Lỗi 2 mô tả ở cuối chương.
bool insideWork = xMm >= 10.0 && xMm <= 90.0 && yMm >= 10.0 && yMm <= 70.0;
var msg = new PickResultMessage(
    valid: insideWork,
    x: xMm, y: yMm, angle: angleDeg,
    score: found.Score);
```

Nguyên tắc an toàn ở hai dòng cuối đáng một lời riêng: **vision không bao giờ ra lệnh cho robot
chạy — nó chỉ đề xuất toạ độ kèm cờ hợp lệ; PLC/robot controller mới là nơi quyết định** (kiểm
tra biên lần nữa phía họ, xử lý timeout, quyết định retry hay reject). Toạ độ nằm ngoài vùng làm
việc mà vẫn gửi đi với `valid = true` thì lỗi vision đã biến thành va chạm cơ khí. Chương 15
dành riêng cho hợp đồng handshake này.

> 🔍 **Đào sâu thêm:** khi cần *độ chính xác đặt* cao hơn nữa (± 0.02 mm), kỹ thuật phổ biến là
> **chụp lại chi tiết trên tay gắp** (regrip check): camera thứ hai nhìn lên, đo chi tiết lệch
> bao nhiêu so với tâm tay gắp sau khi gắp, robot bù nốt phần lệch đó lúc đặt. Bài toán và các
> tool không đổi — vẫn là calibration + align của chương này, chỉ thêm một hệ toạ độ nữa vào
> chuỗi.

## 7.6 Hand-eye calibration — khái quát để nhận diện bài toán

Toàn bộ chương đến giờ giả định **camera cố định, nhìn xuống vùng làm việc cố định** — cấu hình
của MeoVision và của phần lớn trạm kiểm tra. Khi camera **gắn trên tay robot** (eye-in-hand),
bài toán đổi hẳn tính chất: quan hệ camera-thế giới thay đổi theo từng tư thế tay máy, và thứ
cần tìm trở thành phép biến đổi *cố định* giữa mặt bích robot và camera. Tìm nó đòi hỏi quy
trình chuyên biệt: robot cầm camera (hoặc cầm bàn cờ) di chuyển qua nhiều tư thế đã biết, thu
thập cặp dữ liệu (tư thế robot, ảnh), giải bài toán tối ưu — đó chính là **hand-eye calibration**.

VisionPro có nhóm công cụ riêng cho lớp bài toán này (assembly `CalibFixPlus` với các phép hiệu
chuẩn nhiều tư thế). Sách không triển khai chi tiết — điều cần nắm ở mức nhận diện: (1) thấy
camera trên tay robot là biết ngay bài toán calibration khác về chất, đừng bê Code 7.1 vào dùng;
(2) chi phí dạy và duy trì cao hơn đáng kể, nên nếu bố trí cơ khí cho phép camera cố định thì
đó thường là phương án rẻ và bền hơn; (3) từ khoá để đọc tiếp: *hand-eye calibration,
eye-in-hand vs eye-to-hand* trong tài liệu VisionPro và của hãng robot.

## Tổng kết chương

- Pixel không mang nghĩa vật lý; quy đổi `px × hệ_số` đo tay bỏ qua distortion, camera nghiêng
  và gốc toạ độ — đủ ba lý do khiến robot gắp trượt dù "vision báo đúng".
- Mỗi ảnh VisionPro mang một cây không gian toạ độ: `#` là pixel space, `@` là root space, `.`
  là alias của selected space. Tool đọc ROI và trả kết quả theo **selected space** — calibration
  và fixturing hoạt động bằng cách gắn thêm không gian mới và chọn nó cho ảnh output.
- `CogCalibNPointToNPointTool` học phép biến đổi tuyến tính từ N cặp điểm px ↔ mm; dùng ≥ 4 điểm
  trải rộng vùng làm việc và **kiểm `ComputedRMSError` trong code**, không kiểm tay một lần rồi thôi.
  RMS gộp có thể "trốn" một điểm lệch xa nếu các điểm còn lại rất khớp — kiểm thêm **từng điểm
  riêng lẻ** (Code 7.1) trước khi chấp nhận.
- `CogCalibCheckerboardTool` dùng bàn cờ để khớp mô hình phi tuyến — cần khi đo chính xác toàn
  FOV hoặc lens/góc lắp không lý tưởng. Bàn cờ phải phẳng, đặt đúng cao độ mặt đo, phủ kín vùng đo.
- Fixturing (`CogFixtureTool`) treo hệ toạ độ bám theo chi tiết từ pose của tool định vị; kiến
  trúc job chuẩn: **Acquisition → Calibration → Align + Fixture → Inspect**. Luôn xử lý tường
  minh nhánh "không tìm thấy chi tiết" — và nhớ "tìm thấy" (Accepted, Score cao) không đồng
  nghĩa "vị trí hợp lý": kiểm thêm độ lệch so với phạm vi xê dịch cơ khí đã biết (Code 7.3).
- Gửi toạ độ cho robot: cách sạch nhất là dạy calibration bằng chính các điểm robot chạm để hai
  hệ trùng nhau; kiểm tra chiều tay bằng phép thử dịch chuyển thật; góc VisionPro là radian;
  và vision chỉ **đề xuất** kèm cờ hợp lệ — quyết định cho trục chạy thuộc về PLC/robot.

## Lỗi thường gặp

**Lỗi 1 — Đo trên ảnh chưa calibration rồi nhân hệ số tay.** Hiện tượng: kết quả đo đúng ở giữa
ảnh, sai dần về mép; hai trạm "giống hệt nhau" cho hệ số khác nhau. Nguyên nhân: distortion và
camera nghiêng làm tỉ lệ không đều — một hệ số không mô tả nổi. Cách tránh: mọi phép đo/toạ độ
gửi đi đều lấy từ ảnh đã qua tool calibration; coi `px × k` là anti-pattern.

**Lỗi 2 — Điểm calibration dồn một góc, vùng làm việc nằm ngoài đa giác điểm chuẩn.** Hiện tượng:
RMS đẹp nhưng sai số thực tế lớn ở nửa kia của ảnh. Nguyên nhân: mô hình ngoại suy ngoài vùng có
dữ liệu. Cách tránh: điểm chuẩn trải rộng bao trùm vùng làm việc; nghi ngờ thì thêm điểm kiểm
chứng độc lập và so kết quả.

**Lỗi 3 — Dạy bàn cờ ở cao độ khác mặt đo.** Hiện tượng: đo pallet thì chuẩn, đo mặt trên chi
tiết cao 8 mm thì lệch có hệ thống, càng xa tâm ảnh càng lệch. Nguyên nhân: calibration gắn với
mặt phẳng lúc chụp bàn cờ; phối cảnh đổi theo cao độ. Cách tránh: dạy tại đúng cao độ đặc trưng
cần đo, hoặc telecentric lens cho bài toán đo.

**Lỗi 4 — Fixture theo pose cũ khi tool định vị không tìm thấy chi tiết.** Hiện tượng: thỉnh
thoảng một cycle cho toàn bộ kết quả đo "hợp lý nhưng sai"; log thấy score PMAlign cycle đó bằng
không. Nguyên nhân: chuỗi vẫn chạy tiếp với transform của cycle trước. Cách tránh: kiểm
`Results.Count` ngay sau align, không có kết quả thì dừng chuỗi và báo NG-không-tìm-thấy (phân
biệt với NG-đo-hỏng — Chương 15, mục 15.4).

**Lỗi 5 — Gửi robot góc radian, hoặc hệ trái tay chưa đổi chiều.** Hiện tượng: robot đến đúng
X, Y nhưng xoay sai; hoặc di chuyển ngược chiều Y so với kỳ vọng. Nguyên nhân: quên đổi
radian → độ; `SwapCalibratedHandedness` không khớp cách nhập điểm. Cách tránh: quy ước đơn vị
ghi thành văn trong hợp đồng bản tin (Chương 15); sau mỗi lần calibration làm phép thử dịch
chuyển thật theo từng trục trước khi cho chạy tự động.

**Lỗi 6 — Calibration "làm một lần là xong".** Hiện tượng: trạm chạy tốt nhiều tháng rồi sai số
tăng dần sau một lần bảo trì/va chạm mà không ai để ý. Nguyên nhân: lens xoay nhẹ, camera xê
dịch, pallet mòn — mô hình cũ không còn tả đúng hiện trạng. Cách tránh: đưa "kiểm tra calibration"
vào lịch bảo trì (chụp bàn cờ/điểm chuẩn, so RMS và so với lần trước); lưu ảnh bàn cờ + RMS của
mỗi lần dạy làm hồ sơ (Chương 16 bàn quy trình kiểm soát này).

**Lỗi 7 — "Tìm thấy" chi tiết nhưng ở sai vị trí, vẫn fixture bình thường.** Hiện tượng: PMAlign
trả `Accepted = true`, Score cao (0.85+), nhưng thỉnh thoảng cả lô đo "hợp lý mà sai" — hoá ra
tool bắt nhầm một đặc trưng giống hệt chi tiết thật ở chỗ khác trong khung hình (mép pallet, chi
tiết liền kề, phản chiếu). Nguyên nhân: Score đo "hình dạng khớp mẫu dạy đến đâu", không đo "vị
trí này có hợp lý về mặt cơ khí không" — hai câu hỏi độc lập nhau, pass câu 1 không suy ra pass
câu 2. Cách tránh: sau khi có `Results.Count > 0`, kiểm thêm độ lệch tịnh tiến/xoay của pose so
với vị trí danh nghĩa nằm trong phạm vi xê dịch cơ khí thật đã biết trước khi dùng để fixture
(bước 1b, Code 7.3, §7.4.2) — không tin cậy thì trả `NotFound`, đừng "châm chước" vì Score vẫn cao.

\newpage

# Chương 8 — Định vị mẫu: PMAlign

Trạm MeoVision chạy ổn định suốt ba tuần đầu, cho đến một buổi sáng thứ Hai: score định vị của
gần một phần ba chi tiết tụt xuống dưới ngưỡng, robot liên tục báo "không tìm thấy chi tiết" dù
mắt người nhìn ảnh trên màn hình vẫn thấy rõ vỏ nhôm nằm ngay giữa khung hình. Không ai đổi gì
trong job. Thứ duy nhất đổi là: cuối tuần, nhà cung cấp đổi lô nhôm — bề mặt lô mới có độ bóng
khác một chút, và ba chỗ có vết dập nhẹ do vận chuyển nằm đúng trong vùng đã "dạy" cho tool làm
đặc trưng định vị.

Câu chuyện này hé lộ điều khiến PMAlign — công cụ định vị mẫu chủ lực của VisionPro — vừa mạnh
vừa dễ bị dùng sai: nó không so khớp bức ảnh, nó so khớp *một tập đặc trưng hình học* đã được
trích ra lúc train. Train đúng chỗ, chọn đúng thuật toán, đặt đúng tham số — tool chịu được xoay,
co giãn nhẹ, thay đổi ánh sáng, thậm chí một phần bị che khuất. Train sai chỗ — lỡ tay gồm cả
vết dập ngẫu nhiên của lô hàng cụ thể hôm train — tool trở nên giòn, và giòn theo kiểu không ai
nhận ra cho đến khi lô hàng đổi.

Chương này đi từ nguyên lý (mục 8.1) đến quy trình train đúng (8.2), các tham số chạy quyết định
tốc độ và độ chắc chắn (8.3), cách đọc kết quả (8.4), rồi đến phần thực dụng nhất — vì sao score
tụt và khi nào phải retrain (8.5). Đây là chương đầu tiên của Phần III; kết quả của PMAlign chính
là thứ nuôi cho fixturing đã học ở Chương 7, mục 7.4 — đọc lại chuỗi Align → Fixture ở đó nếu
cần ôn lại bối cảnh trước khi vào chương này.

## 8.1 PatMax là gì

### 8.1.1 Đặc trưng hình học, không phải mức xám

Cách so khớp ảnh "cổ điển" — correlation — trượt một khuôn mẫu (mức xám của từng pixel) qua ảnh,
tính độ tương quan tại mỗi vị trí, chọn chỗ tương quan cao nhất. Cách này nhạy với đúng những
thứ ta không muốn nó nhạy: ánh sáng đổi nhẹ làm mức xám lệch, chi tiết xoay vài độ làm khuôn mẫu
không còn khớp pixel-đối-pixel, độ tương phản thấp làm correlation mù mờ.

**PatMax** (viết tắt của Pattern Max — công nghệ lõi đứng sau `CogPMAlignTool`) đi theo hướng
khác hẳn: lúc train, nó trích ra một **mô hình đặc trưng hình học** — tập hợp các đoạn biên, góc,
đường cong mô tả *hình dạng* của pattern, độc lập phần lớn với mức xám tuyệt đối. Lúc chạy, nó
tìm trong ảnh tập đặc trưng khớp với mô hình đó, cho phép xoay, co giãn (scale), và chịu được
một phần che khuất hay biến dạng — vì nó đang so khớp *hình dạng*, không so khớp từng pixel.

> 🔍 **Đào sâu thêm:** PatMax dùng cách tiếp cận biên (edge-based, boundary matching) khác về
> bản chất với các thuật toán template-matching mức xám thuần tuý. Chi tiết toán học nằm ngoài
> phạm vi sách này; điều quan trọng để dùng tool đúng là nắm được **hệ quả thực dụng**: đặc trưng
> hình học nằm ở *biên có tương phản*, nên vùng phẳng không biên (một mảng màu đồng nhất) đóng
> góp gần như không có gì vào việc định vị, dù mắt người vẫn "nhìn thấy" nó rõ ràng.

### 8.1.2 Ba biến thể: PatMax, PatQuick, PatFlex

VisionPro cho chọn thuật toán lúc **train** (thuộc tính `TrainAlgorithm` của
`CogPMAlignPattern`, tập giá trị định nghĩa trong `CogPMAlignTrainAlgorithmConstants`) và có
thể chọn lại thuật toán chạy trong giới hạn đã train (`RunAlgorithm` của `CogPMAlignRunParams`).
Ba lựa chọn cốt lõi:

**Bảng 8.1 — PatMax, PatQuick, PatFlex: chọn thuật toán train.**

| Thuật toán | Đặc điểm | Đánh đổi | Dùng khi |
|---|---|---|---|
| `PatMax` | Độ chắc chắn cao nhất — chịu xoay, scale, che khuất, biến dạng nhẹ tốt nhất | Chậm hơn PatQuick | Mặc định cho hầu hết bài toán định vị công nghiệp; MeoVision dùng thuật toán này |
| `PatQuick` | Nhanh hơn đáng kể, dựa trên phiên bản rút gọn của cùng công nghệ | Chịu biến dạng/che khuất kém hơn PatMax | Cycle time rất gấp, chi tiết ổn định, ít nhiễu |
| `PatFlex` | Chịu được **biến dạng đàn hồi** — chi tiết mềm, dẻo, không hoàn toàn cứng | Chi phí tính toán cao nhất trong ba loại | Bao bì mềm, vật liệu co giãn — hiếm gặp ở trạm cơ khí cứng như MeoVision |
| `PatMaxAndPatQuick` | Train cả hai, chọn thuật toán chạy sau (`RunAlgorithm`) mà không cần train lại | Tốn thời gian train hơn, file pattern nặng hơn | Chưa chắc chắn cycle time cho phép PatMax hay cần PatQuick — giữ cả hai lựa chọn |

Với trạm MeoVision (vỏ nhôm cứng, không biến dạng), ta train bằng `PatMax` thuần — đủ chắc chắn,
và cycle time của trạm còn dư (Chương 15 sẽ tính ngân sách thời gian chi tiết). `PatMaxPerspective`
và `PatMaxHighSensitivity` là hai biến thể nâng cao hơn (khử méo phối cảnh, tăng độ nhạy khi
tương phản rất thấp) — biết tên để tra cứu khi gặp bài toán khó, sách không đi sâu.

## 8.2 Train pattern đúng cách

### 8.2.1 Ba lựa chọn quyết định chất lượng pattern

Train một `CogPMAlignPattern` xoay quanh ba property: `TrainImage` (ảnh chứa chi tiết mẫu),
`TrainRegion` (vùng trên ảnh được phép trích đặc trưng — một `CogRegion` bất kỳ, thường là hình
chữ nhật hoặc đa giác ôm sát chi tiết), và `Origin` — điểm/hệ toạ độ được chọn làm gốc của pattern.

**`Origin` quan trọng hơn vẻ ngoài của nó.** Đây chính là gốc của không gian mà `GetPose()` trả
về, và — như đã thấy ở Chương 7, mục 7.4 — cũng là gốc của fixtured space khi pose đó được đưa
vào `CogFixtureTool`. Đặt `Origin` tuỳ tiện (mặc định là tâm hình học của `TrainRegion`) thì mọi
ROI phía sau, mọi toạ độ gửi robot, đều mang theo một độ lệch cố định không có ý nghĩa cơ khí gì.
Đặt `Origin` đúng vào **datum của chi tiết** — giao hai cạnh chuẩn theo bản vẽ, tâm một lỗ định
vị — thì toàn bộ chuỗi phía sau đọc thẳng theo hệ quy chiếu mà bản vẽ cơ khí dùng.

> ⚠️ **Lưu ý xác minh SDK:** các tên thuộc tính/method cụ thể dùng xuyên suốt chương này —
> `pattern.Execute()` và `Origin` (mục 8.2), `RunAlgorithm`/`SearchRegionMode` (mục 8.3),
> `GetPose()` và thứ tự sắp xếp mặc định của `Results` theo score (mục 8.4) — được viết theo
> hiểu biết chung về kiến trúc VisionPro, KHÔNG tra trực tiếp trên SDK thật. Trước khi đưa vào
> sản xuất, đối chiếu tên chính xác và hành vi (có sắp xếp không, kiểu trả về property hay
> method) với `Doc\en\` hoặc IntelliSense của đúng phiên bản SDK đang dùng — các chi tiết này có
> thể khác nhau giữa các phiên bản VisionPro.

![Hình 8.1 — Train pattern: TrainRegion ôm sát chi tiết, Origin đặt tại datum bản vẽ](../assets/ch08/hinh_8_1.png)
**Hình 8.1 — Train pattern: TrainRegion ôm sát chi tiết, Origin đặt tại datum bản vẽ.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): screenshot QuickBuild tab train của CogPMAlignTool, ảnh vỏ
> nhôm MeoVision. Vùng train (nét vàng) là hình chữ nhật ôm sát biên ngoài chi tiết, chừa lề nhỏ.
> Gốc pattern (biểu tượng trục toạ độ nhỏ, đỏ-xanh) đặt tại góc trên-trái của chi tiết — trùng
> điểm gốc bản vẽ cơ khí. Có chú thích mũi tên "Origin = datum bản vẽ, KHÔNG phải tâm hình học".

### 8.2.2 Loại trừ nhiễu: mask và ranh giới vùng train

Hai tình huống thường buộc phải "dọn" vùng train trước khi chấp nhận kết quả:

- **Vùng train dính cả nền xung quanh chi tiết.** Nếu nền có hoa văn/vân bề mặt ổn định, không
  sao — nó thậm chí góp thêm đặc trưng ổn định. Nhưng nếu nền là băng tải có thể trôi, có bụi,
  có phản xạ thay đổi theo ca, những đặc trưng "giả" đó bị train vào pattern và trở thành nguồn
  bất ổn. Cách xử lý: co hẹp `TrainRegion` sát biên chi tiết, hoặc dùng `TrainImageMask` (ảnh mặt
  nạ nhị phân) để loại hẳn vùng nền ra khỏi việc trích đặc trưng dù nó vẫn nằm trong `TrainRegion`.
- **Vùng train dính đặc trưng không lặp lại giữa các chi tiết** — đúng như câu chuyện mở đầu
  chương: vết dập ngẫu nhiên, ba-via đúc không đều, nhãn dán lệch. Những đặc trưng này *có thật*
  trên chi tiết mẫu hôm train, nhưng không đại diện cho toàn bộ lô sản xuất. Cách xử lý giống hệt
  trên: mask loại vùng đó ra, hoặc — tốt hơn nếu có thể — chọn ảnh mẫu khác không dính lỗi cục bộ.

> ⚠️ **Cảnh báo:** đừng lấy MỘT tấm ảnh làm chuẩn để đánh giá "pattern tốt". Đặc trưng trông rất
> rõ ràng trên chi tiết mẫu có thể là đặc trưng *riêng của đúng chi tiết đó*. Trước khi chốt
> pattern, chạy thử trên một loạt ảnh khác nhau — nhiều chi tiết, nhiều lô nếu có — quan sát
> score dao động bao nhiêu (mục 8.5 bàn kỹ cách đọc con số này).

**Code 8.1 — Train một CogPMAlignPattern với vùng và gốc đặt tường minh.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;

var pattern = new CogPMAlignPattern();
pattern.TrainImage  = sampleImage;          // ảnh chi tiết mẫu, đã kiểm tra không lỗi cục bộ
pattern.TrainRegion = trainRegion;           // CogRectangleAffine ôm sát biên chi tiết
pattern.Origin       = originAtDatum;        // gốc theo datum bản vẽ, KHÔNG phải mặc định
pattern.TrainAlgorithm = CogPMAlignTrainAlgorithmConstants.PatMax;

pattern.Execute();                           // thực hiện train

if (!pattern.Trained)
    throw new InvalidOperationException("Train pattern thất bại — kiểm TrainRegion/ảnh mẫu");
```

> 📌 **Lưu ý:** `pattern.Execute()` là bước **train**, khác hẳn `tool.Run()` là bước **chạy tìm
> kiếm**. Nhầm hai khái niệm này — tưởng gọi `Run()` sẽ tự train nếu chưa train — là lỗi mới học
> VisionPro hay gặp; hai method thuộc hai đối tượng khác nhau (`CogPMAlignPattern` vs
> `CogPMAlignTool`) chính vì lý do đó.

## 8.3 Run params: cân bằng giữa tốc độ và độ chắc chắn

Train xong pattern một lần; các tham số chạy (`CogPMAlignRunParams`, gán vào `tool.RunParams`)
là thứ được tinh chỉnh nhiều lần trong đời trạm, và là nơi phần lớn "nghệ thuật" tuning PMAlign
nằm ở đó. Bảy nhóm tham số đáng nắm:

**Bảng 8.2 — Các nhóm tham số run params ảnh hưởng tốc độ/độ chắc chắn.**

| Tham số | Ý nghĩa | Ảnh hưởng khi mở rộng |
|---|---|---|
| `AcceptThreshold` | Ngưỡng score tối thiểu để một kết quả được chấp nhận (0–1) | Thấp → dễ chấp nhận kết quả yếu (rủi ro false accept); cao → dễ bỏ sót kết quả hợp lệ nhưng hơi mờ |
| `GrainLimitCoarse`/`GrainLimitFine`, `CoarseAcceptThreshold`(+`Enabled`) | Độ phân giải đặc trưng (granularity) dùng khi tìm thô/tinh, và ngưỡng riêng cho bước thô | Đòn bẩy tốc độ thường **mạnh hơn** cả hai dòng Zone bên dưới — xem giải thích ngay sau bảng |
| `ZoneAngle` (kiểu `CogPMAlignZoneAngle`, đơn vị **radian**) | Phạm vi góc xoay cho phép tìm | Mở rộng → tìm được chi tiết xoay nhiều hơn, **nhưng chậm hơn đáng kể** và dễ bắt nhầm |
| `ZoneScale`, `ZoneScaleX`, `ZoneScaleY` | Phạm vi co giãn cho phép tìm | Tương tự: mở rộng vô tội vạ vừa chậm vừa tăng rủi ro nhầm |
| `ApproximateNumberToFind` | Số kết quả gần đúng mong đợi | Đặt đúng số thực tế giúp tool dừng sớm hơn, không quét cố tìm thêm |
| `TimeoutEnabled` + `Timeout` | Giới hạn thời gian tìm kiếm | Bắt buộc bật cho trạm sản xuất — không có giới hạn, tool có thể "cố tìm" vượt ngân sách cycle |
| `SearchRegionMode`, `SearchRegion` (trên `tool`, không phải `RunParams`) | Vùng ảnh được phép tìm | Thu hẹp vùng tìm vừa nhanh hơn vừa giảm khả năng bắt nhầm đối tượng khác trong khung hình |

PatMax không tìm kiếm ở một độ phân giải duy nhất — nó có tham số **granularity** (độ phân giải
đặc trưng khi tìm), tách riêng cho bước thô và bước tinh: `GrainLimitCoarse`/`GrainLimitFine`
(mặc định 4.0/1.0, hợp lệ 1.0–25.5, và `Coarse` luôn ≥ `Fine`). Đi kèm là
`CoarseAcceptThresholdEnabled` + `CoarseAcceptThreshold` (mặc định tắt; khi bật, ngưỡng này luôn
≤ `AcceptThreshold`): bật lên, chỉ kết quả đạt `CoarseAcceptThreshold` ở bước thô mới được tinh
chỉnh tiếp rồi so lại với `AcceptThreshold` thật — tách "lọc nhanh diện rộng" ra khỏi "đánh giá
chính xác", đúng tinh thần tìm kiếm **thô trước, tinh sau** (coarse-to-fine) phổ biến ở các thuật
toán định vị mẫu công nghiệp. Đây thường là đòn bẩy tốc độ **mạnh hơn hẳn** việc thu hẹp
`ZoneAngle`/`ZoneScale`, vì nó giảm khối lượng tính toán ngay từ vòng lọc đầu tiên thay vì chỉ
giảm phạm vi tìm. Mặc định của VisionPro đã cân bằng hợp lý cho phần lớn bài toán — chỉ chỉnh khi
đã đo tốc độ không đạt VÀ đã thử hết các tham số khác trong Bảng 8.2.

**Code 8.2 — Cấu hình run params: giới hạn góc/scale hợp lý, bật timeout bắt buộc.**

```csharp
using Cognex.VisionPro.PMAlign;

CogPMAlignRunParams rp = tool.RunParams;
rp.AcceptThreshold          = 0.65;   // ngưỡng chấp nhận — xem cách chọn ở mục 8.5
rp.ApproximateNumberToFind  = 1;      // MeoVision: đúng 1 chi tiết mỗi cycle
rp.TimeoutEnabled            = true;
rp.Timeout                   = 200;    // ms — nằm trong ngân sách cycle của trạm (Chương 15)
rp.RunAlgorithm               = CogPMAlignRunAlgorithmConstants.PatMax;

// Phạm vi xoay cho phép: ± 15° quanh nominal — đo thực tế MeoVision là ± 10° (Chương 7),
// chừa biên an toàn. Configuration = LowHigh nghĩa là chỉ định khoảng [Low, High], không
// phải một giá trị Nominal cố định. CHÚ Ý: Low/High tính bằng RADIAN (tài liệu chính thức:
// "Units are in Radians") — gán thẳng -15.0 là -15 radian, tương đương mở toàn dải góc!
rp.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
rp.ZoneAngle.Low  = -15.0 * Math.PI / 180.0;
rp.ZoneAngle.High =  15.0 * Math.PI / 180.0;

tool.SearchRegion = pickAreaRegion;   // giới hạn vùng tìm về đúng khu vực pallet trên băng tải
```

> 💡 **Mẹo thực chiến:** nguyên tắc chung khi tuning zone: **thu hẹp về đúng mức thực tế cần**,
> không hơn. Chi tiết trên pallet MeoVision xoay tối đa ± 10° (thông số trạm, Chương 7) — khai
> báo zone góc ± 15° (chừa biên an toàn vừa phải) chứ không phải ± 180° "cho chắc". Mỗi độ mở
> rộng ngoài nhu cầu thực là mỗi phần trăm cycle time và rủi ro nhầm bị đánh đổi mà không thu
> được gì.

## 8.4 Đọc kết quả: score, pose, nhiều instance

### 8.4.1 CogPMAlignResults — một danh sách, không phải một kết quả

`tool.Results` là một `CogPMAlignResults` — một tập hợp (`Count`, chỉ số qua `Item`/indexer),
không phải một kết quả đơn. Ngay cả khi `ApproximateNumberToFind = 1`, `Count` có thể là 0
(không tìm thấy gì đạt `AcceptThreshold`) hoặc nhiều hơn 1 (nhiều vùng đều đạt ngưỡng — thường
do nhầm chi tiết khác, hoặc pattern có tính đối xứng khiến nhiều vị trí đều "khớp"). Xử lý luôn
phải bắt đầu bằng kiểm tra `Count`, không giả định phần tử đầu tiên luôn tồn tại và luôn đúng.

### 8.4.2 Các thuộc tính của một CogPMAlignResult

**Bảng 8.3 — Các thuộc tính chính của một kết quả PMAlign.**

| Thuộc tính | Ý nghĩa |
|---|---|
| `Score` | Điểm khớp (0–1) — thước đo chính để đánh giá độ tin cậy của kết quả |
| `GetPose()` | Trả về `CogTransform2DLinear` — vị trí (X, Y) và góc xoay của pattern trong ảnh, tính theo `Origin` đã train (mục 8.2) |
| `Contrast` | Độ tương phản đo được tại vị trí khớp |
| `Coverage` | Tỉ lệ đặc trưng của pattern được tìm thấy trong ảnh (thấp → chi tiết bị che một phần) |
| `Clutter` | Mức độ "lộn xộn" — đặc trưng lạ xung quanh vị trí khớp không thuộc pattern |
| `Accepted` | Cờ tổng hợp: kết quả có vượt các ngưỡng chấp nhận đã cấu hình hay không |

**Code 8.3 — Đọc kết quả PMAlign với kiểm tra Count tường minh (nối tiếp Code 7.3).**

```csharp
using Cognex.VisionPro.PMAlign;

tool.InputImage = calibratedImage;   // ảnh đã calibration — xem Chương 7
tool.Run();

if (tool.Results.Count == 0)
{
    return StationResult.NotFound;   // không có gì đạt AcceptThreshold — dừng chuỗi tường minh
}

CogPMAlignResult best = tool.Results[0];   // giá trị khởi tạo — nếu Count > 1, chọn lại theo score dưới đây
if (tool.Results.Count > 1)
{
    // Nhiều kết quả đạt ngưỡng khi ApproximateNumberToFind = 1: đáng ngờ — log để điều tra,
    // KHÔNG âm thầm lấy Results[0] rồi coi như bình thường
    logger.LogWarning("PMAlign trả {Count} kết quả, mong đợi 1", tool.Results.Count);

    // Không giả định Results đã sắp sẵn theo score — chủ động quét lấy điểm cao nhất
    for (int i = 1; i < tool.Results.Count; i++)
    {
        if (tool.Results[i].Score > best.Score)
            best = tool.Results[i];
    }
}

double score = best.Score;
var pose     = best.GetPose();   // dùng tiếp cho CogFixtureTool — xem Chương 7, mục 7.4
```

**Code 8.3b — Trích xuất X, Y, góc cụ thể từ pose (đáp ứng "đọc pose (X, Y, góc)" theo mục tiêu chương).**

```csharp
double xMm      = pose.TranslationX;              // vị trí X theo Origin đã train, đơn vị theo calibration (Chương 7)
double yMm      = pose.TranslationY;               // vị trí Y
double angleDeg = pose.Rotation * 180.0 / Math.PI;  // Rotation trả về radian — đổi sang độ khi log/hiển thị
```

### 8.4.3 Nhiều instance và chi tiết đối xứng

Khi `ApproximateNumberToFind` > 1 (ví dụ trạm cần định vị nhiều chi tiết giống nhau trên cùng một
khay), việc đọc kết quả không dừng ở lấy một `best` — cần duyệt toàn bộ `Results` đã `Accepted`:

```csharp
var accepted = new List<CogPMAlignResult>();
for (int i = 0; i < tool.Results.Count; i++)
{
    if (tool.Results[i].Accepted)
        accepted.Add(tool.Results[i]);
}
// Sắp accepted theo toạ độ X hoặc Y nếu robot cần xử lý các chi tiết theo một thứ tự cố định
```

Một tình huống riêng đáng biết: **chi tiết có tính đối xứng** (vòng đệm tròn, chi tiết đối xứng
180°...) có thể khiến PMAlign trả về nhiều `Results` hợp lệ tại cùng một vị trí vật lý nhưng khác
góc xoay — đây không phải lỗi bắt nhầm, mà là hệ quả tất yếu của hình dạng đối xứng: về mặt hình
học, tool không có cách nào phân biệt hai góc nhìn giống hệt nhau. Nếu góc không quan trọng cho
bước sau (chỉ cần gắp đúng tâm), hiện tượng này vô hại; nếu góc quan trọng (lắp ráp có định
hướng), cần thêm một đặc trưng bất đối xứng vào vùng train (một lỗ lệch tâm, một vết khắc) hoặc
dùng thêm logic/cảm biến khác để phá thế đối xứng.

## 8.5 Tuning thực chiến

### 8.5.1 Đọc con số score như thế nào

Score không phải một hằng số của "chi tiết này có đúng hình dạng không" — nó bị kéo xuống bởi
bất cứ điều gì làm giảm tỉ lệ đặc trưng train khớp được với ảnh chạy: ánh sáng đổi (Chương 2),
chi tiết bị che một phần, bụi bẩn phủ lên đúng vùng đặc trưng, hoặc — như câu chuyện mở đầu —
lô hàng có bề mặt khác đôi chút so với lô lúc train.

Nguyên tắc đặt `AcceptThreshold`: không chọn một con số "nghe hợp lý" như 0.8 rồi thôi. Thu thập
score của một loạt ảnh **hợp lệ thật** (golden set — Chương 16 bàn quy trình đầy đủ), xem phân
bố: nếu toàn bộ ảnh hợp lệ đều cho score trên 0.85 và không có ảnh lỗi nào lọt qua 0.6, ngưỡng
0.65–0.7 là một lựa chọn có cơ sở — còn nếu ảnh hợp lệ dao động rộng từ 0.55 đến 0.95, ngưỡng
cao sẽ tạo ra false reject và vấn đề nằm ở pattern hoặc điều kiện chụp, không phải ở ngưỡng.

### 8.5.2 Vì sao score tụt theo thời gian — và khi nào phải retrain

Bốn nguyên nhân phổ biến nhất khiến một pattern "chạy tốt hàng tháng rồi tự nhiên tệ đi":

1. **Đổi lô nguyên liệu** — bề mặt, độ bóng, dung sai đúc khác đi (đúng tình huống mở chương).
2. **Trôi ánh sáng** — đèn LED xuống cấp theo thời gian, bụi bám ống kính, thay đổi ánh sáng môi
   trường theo mùa (đã bàn nguyên tắc kiểm soát ở Chương 2).
3. **Trôi cơ khí** — camera/lens xê dịch nhẹ do rung động, thay đổi góc nhìn làm đặc trưng train
   không còn khớp hoàn hảo về hình dạng biểu kiến.
4. **Pattern train từ mẫu không đại diện** — vấn đề tiềm ẩn từ mục 8.2 chỉ lộ ra khi lô hàng sau
   này khác đủ nhiều so với mẫu hôm train.

Việc cần làm khi phát hiện score tụt **không phải** là hạ `AcceptThreshold` cho hết báo lỗi —
đó là che triệu chứng, không sửa nguyên nhân, và làm tăng rủi ro false accept đúng lúc hệ thống
đang bất ổn nhất. Thứ tự điều tra hợp lý: kiểm ánh sáng/cơ khí trước (rẻ, nhanh, không cần train
lại) — nếu cả hai ổn mà score vẫn tụt đồng loạt theo lô mới, đó là tín hiệu retrain với mẫu đại
diện cho lô mới, theo đúng quy trình mục 8.2 (không chỉnh riêng threshold để "vá").

> ⚠️ **Cảnh báo:** retrain trực tiếp trên file .vpp đang chạy sản xuất mà không backup là rủi ro
> thật — pattern mới tệ hơn pattern cũ (mẫu chọn không tốt bằng) chỉ lộ ra sau khi đã ghi đè,
> không còn đường lùi. Quy trình retrain có kỷ luật là chủ đề của Chương 14, mục 14.4.

## 8.6 SearchMax và CogPMAlignMultiTool

Hai công cụ mở rộng đáng biết tên, không triển khai chi tiết trong sách này:

- **`CogSearchMaxTool`** (assembly `Cognex.VisionPro.SearchMax`) giải bài toán khác về bản chất:
  tìm kiếm **không cần train hình học trước** — hữu ích khi đối tượng cần tìm không cố định hình
  dạng theo cách PMAlign giả định, hoặc khi bài toán gần với tìm-đối-tượng-tổng-quát hơn là
  định-vị-một-pattern-đã-biết.
- **`CogPMAlignMultiTool`** giải bài toán "nhiều pattern khác nhau, một tool" — hữu ích khi trạm
  cần nhận diện và định vị **nhiều loại chi tiết khác nhau** luân phiên trên cùng một băng tải
  (mỗi loại một pattern đã train riêng), thay vì phải dựng nhiều `CogPMAlignTool` song song.

MeoVision chỉ có một loại chi tiết cố định nên `CogPMAlignTool` đơn là đủ; hai công cụ trên đáng
tra cứu khi bài toán thực tế của bạn khác — từ khoá tìm tài liệu chính thức: *SearchMax*,
*multi-model PMAlign*.

## Tổng kết chương

- PatMax so khớp **đặc trưng hình học** (biên có tương phản), không so khớp mức xám như
  correlation cổ điển — đó là lý do nó chịu được xoay, co giãn, che khuất một phần tốt hơn nhiều.
- Chọn thuật toán train theo Bảng 8.1: `PatMax` là mặc định hợp lý cho chi tiết cứng; `PatQuick`
  khi cần tốc độ và chi tiết ổn định; `PatFlex` cho vật liệu biến dạng đàn hồi.
- Train pattern đúng cách: `TrainRegion` ôm sát chi tiết, `Origin` đặt tại **datum bản vẽ** (không
  phải mặc định hình học) vì nó trở thành gốc của mọi ROI/toạ độ phía sau qua fixturing; dùng
  mask loại nhiễu nền và loại đặc trưng ngẫu nhiên không đại diện cho cả lô.
- Run params: thu hẹp phạm vi góc/scale về **đúng mức thực tế**, bật `TimeoutEnabled` bắt buộc
  cho trạm sản xuất, đặt `ApproximateNumberToFind` đúng số thực tế. Khi cần tăng tốc thật sự,
  `GrainLimitCoarse`/`Fine` + `CoarseAcceptThreshold` (tìm thô trước, tinh sau) thường là đòn bẩy
  mạnh hơn cả thu hẹp Zone.
- Đọc kết quả qua `Results` — một tập hợp, luôn kiểm `Count` trước khi lấy phần tử; các thuộc
  tính `Score`/`Coverage`/`Clutter` cho biết *vì sao* một kết quả yếu, không chỉ *rằng* nó yếu.
- Đặt `AcceptThreshold` dựa trên phân bố score đo được từ golden set, không đoán một con số.
  Score tụt theo thời gian: điều tra ánh sáng/cơ khí trước, chỉ retrain khi xác định nguyên nhân
  là lô hàng/mẫu train không còn đại diện — không hạ threshold để che triệu chứng.

## Lỗi thường gặp

**Lỗi 1 — Train cả bóng đổ hoặc nền không ổn định vào pattern.** Hiện tượng: score cao trong
phòng lab, tụt thất thường ngoài dây chuyền dù chi tiết trông giống hệt mẫu. Nguyên nhân: đặc
trưng "giả" từ bóng đổ/nền được coi là một phần bắt buộc phải khớp. Cách tránh: `TrainRegion` +
mask sát chi tiết thật; kiểm soát ánh sáng nhất quán trước khi train (Chương 2).

**Lỗi 2 — Mở angle range ± 180° "cho chắc".** Hiện tượng: job chạy chậm hẳn so với tính toán,
thỉnh thoảng bắt nhầm hướng đối xứng của chi tiết. Nguyên nhân: mở phạm vi tìm kiếm vượt xa nhu
cầu thực vừa tốn thời gian quét vừa tăng không gian cho kết quả nhầm. Cách tránh: đo phạm vi xoay
thực tế của chi tiết trên pallet, khai báo đúng mức đó cộng biên an toàn vừa phải (mục 8.3).

**Lỗi 3 — Không bật Timeout cho trạm sản xuất.** Hiện tượng: hiếm khi, một cycle "treo" bất
thường lâu hơn hẳn bình thường, làm lệch nhịp cả dây chuyền. Nguyên nhân: điều kiện ảnh xấu khiến
tool "cố tìm" không giới hạn thời gian. Cách tránh: `TimeoutEnabled = true` với `Timeout` nằm
trong ngân sách cycle đã tính (Chương 15).

**Lỗi 4 — Lấy `Results[0]` mà không kiểm `Count`.** Hiện tượng: exception ngẫu nhiên khi chi tiết
vắng mặt một cycle (băng tải trống, cấp liệu trễ). Nguyên nhân: code giả định luôn có ít nhất
một kết quả. Cách tránh: luôn kiểm `Count == 0` trước, xử lý nhánh không tìm thấy tường minh
(đúng nguyên tắc đã nhấn ở Chương 7, mục 7.4).

**Lỗi 5 — Hạ AcceptThreshold để "hết báo lỗi" khi score tụt hàng loạt.** Hiện tượng: false accept
tăng dần, thỉnh thoảng robot gắp vào vị trí sai mà không ai cảnh báo trước. Nguyên nhân: threshold
bị chỉnh để che triệu chứng thay vì điều tra nguyên nhân gốc (ánh sáng, cơ khí, lô hàng). Cách
tránh: theo quy trình điều tra ở mục 8.5.2; xem Chương 16 về giám sát score theo thời gian.

\newpage

# Chương 9 — Đo lường: Caliper và các tool tìm biên

Trạm đo kích thước của MeoVision phải xác nhận chiều rộng vỏ nhôm nằm trong 40 mm ± 0.05 mm —
một dải rộng đúng 100 micromet. Kỹ sư mới vào nghề, quen tư duy "PMAlign tìm được vị trí thì
dùng luôn khoảng cách giữa hai điểm nó trả về" thử cách đó trước: lấy hai lần chạy PMAlign ở hai
cạnh, trừ toạ độ. Kết quả dao động ngẫu nhiên ± 0.3 mm giữa các lần đo cùng một chi tiết đứng
yên — gấp sáu lần dung sai cho phép. PMAlign không sai; nó chỉ không được thiết kế cho việc này.
Định vị mẫu trả lời câu hỏi "vật thể đang ở đâu, xoay bao nhiêu" với độ chính xác đủ cho fixturing
và gắp-đặt; nó không được tối ưu để trả lời "cạnh này cách cạnh kia chính xác bao nhiêu milimet".

Câu hỏi thứ hai đó là việc của một họ công cụ khác hẳn: **caliper**. Ý tưởng của caliper đơn giản
đến mức dễ đánh giá thấp — quét một dải hẹp vuông góc với cạnh cần tìm, tìm điểm mà độ sáng đổi
đột ngột (edge), lặp lại nhiều lần dọc theo cạnh và khớp một đường thẳng/đường tròn qua các điểm
đó. Chính vì chỉ làm một việc hẹp và làm rất kỹ, caliper đạt độ lặp lại dưới pixel (sub-pixel) —
thứ mà một tool định vị đa năng như PMAlign không nhắm tới.

Chương này đi từ viên gạch cơ bản — `CogCaliperTool` tìm một cặp biên (mục 9.1) — đến các tool
tổng hợp nhiều caliper để khớp hình học: đường thẳng và đường tròn (mục 9.2), rồi đến các phép
đo hình học phái sinh — khoảng cách, giao điểm, góc (mục 9.3), cách đưa các con số pixel này
thành milimet có ý nghĩa dung sai thực sự (mục 9.4), và cuối cùng là cách đánh giá cả hệ đo —
không chỉ một phép đo đơn lẻ — như một thiết bị đo lường thực thụ (mục 9.5). Toàn bộ chương giả
định ảnh đầu vào đã đi qua chuỗi Calibration + Fixture của Chương 7 — nếu ROI của caliper không
"bám" theo chi tiết, mọi phép đo trong chương này vô nghĩa trước khi kịp bàn đến độ chính xác.

## 9.1 CogCaliperTool: tìm biên bằng một lát cắt

### 9.1.1 Cơ chế: chiếu ảnh xuống một chiều

`CogCaliperTool` nhận một `Region` — hình chữ nhật xác định dải quét (chiều dài dải là hướng
quét tìm biên, chiều rộng là hướng lấy trung bình để khử nhiễu) — và tham số cấu hình nằm trong
đối tượng `CogCaliper` (property `RunParams` của tool). Về bản chất, tool chiếu (project) toàn bộ
dải xuống một hàm cường độ theo một chiều, rồi tìm vị trí đạo hàm (gradient) đạt cực trị — đó
chính là biên. Việc "lấy trung bình theo chiều rộng dải" là bí quyết khử nhiễu: một cạnh có nhiễu
hạt (grain) ở mức pixel đơn lẻ, nhưng trung bình hoá qua hàng chục dòng quét cho một vị trí biên
ổn định hơn nhiều bậc. Bản thân việc nội suy vị trí cực trị gradient chính xác hơn một pixel
nguyên (kỹ thuật "cạnh dưới-pixel" — đã học cơ chế toán học ở Chương 4, mục 4.4.2) cộng với việc
trung bình hoá qua nhiều dòng quét này là gốc rễ của độ chính xác sub-pixel của caliper.

**Bảng 9.1 — Các tham số cấu hình chính của CogCaliper (`tool.RunParams`).**

| Thuộc tính | Ý nghĩa |
|---|---|
| `EdgeMode` | `SingleEdge` — tìm một biên; `Pair` — tìm một **cặp** biên (ví dụ hai mép của một rãnh) |
| `Edge0Polarity`, `Edge1Polarity` | Chiều chuyển sáng-tối mong đợi tại biên: `LightToDark`, `DarkToLight`, hoặc `DontCare` |
| `ContrastThreshold` | Ngưỡng tương phản tối thiểu để một điểm được công nhận là biên — lọc nhiễu nền phẳng |
| `FilterHalfSizeInPixels` | Độ rộng bộ lọc làm mượt trước khi tính đạo hàm — lớn hơn thì mượt hơn nhưng kém nhạy biên yếu |
| `MaxResults` | Số cặp/biên tối đa trả về khi có nhiều ứng viên trong dải quét |
| `SamplingMode` | Cách lấy mẫu dọc dải (ảnh hưởng tốc độ/độ mượt) |

Chọn `EdgeMode` theo đúng bản chất phép đo: `SingleEdge` khi chỉ cần định vị một biên duy nhất
(mép ngoài chi tiết, một cạnh chuẩn để làm mốc); `Pair` khi cần khoảng cách giữa hai biên đối
diện nhau trong cùng một dải quét (bề rộng một khe, một rãnh, hoặc — như Code 9.1 — bề rộng chi
tiết đo qua backlight).

> 💡 **Mẹo thực chiến — tuning hai tham số hay bị bỏ mặc định:** `ContrastThreshold` đặt quá thấp
> khiến tool bắt cả nhiễu hạt nhỏ làm "biên giả"; đặt quá cao khiến tool bỏ sót đúng cạnh cần đo
> khi tương phản hơi yếu (đèn trôi, bề mặt hơi mờ) — đây là lỗi tuning phổ biến nhất khi đưa job
> ra khỏi phòng lab. `FilterHalfSizeInPixels` lớn hơn làm mượt nhiễu tốt hơn nhưng đồng thời làm
> "béo" biên thật (lệch nhẹ vị trí sub-pixel báo cáo) và có thể xoá mất các đặc trưng hẹp (khe,
> notch nhỏ) nằm gần nhau — không chỉ đơn thuần "mượt hơn thì tốt hơn". Cả hai tham số nên tinh
> chỉnh trên một loạt ảnh thật (không chỉ một ảnh "đẹp"), theo đúng tinh thần đo từ golden set đã
> nhấn mạnh xuyên suốt sách. Một yếu tố khác dễ bị quên: dải quét phải gần **vuông góc** với cạnh
> cần đo — cạnh nghiêng nhiều so với hướng quét làm gradient trải rộng ra thay vì có một đỉnh sắc
> nét, làm giảm cả độ chính xác lẫn độ ổn định của vị trí biên tìm được.

### 9.1.2 Polarity — tham số hay bị bỏ qua nhưng quyết định đúng-sai

`Edge0Polarity`/`Edge1Polarity` chỉ có ý nghĩa khi ta biết trước **chi tiết sáng hơn hay tối hơn
nền**. Trạm MeoVision dùng backlight cho khâu đo (Chương 2, mục 2.2): chi tiết chắn sáng nên xuất
hiện **tối** trên nền **sáng**. Quét từ ngoài (nền) vào trong (chi tiết), biên đầu tiên là chuyển
từ sáng sang tối — `LightToDark`. Đặt `DontCare` "cho chắc" nghe an toàn nhưng thực ra mở khả
năng tool bắt nhầm biên bóng đổ hoặc vết bẩn có chiều tương phản ngược — càng biết trước polarity,
càng nên khai báo tường minh để tool loại bỏ toàn bộ ứng viên sai chiều ngay từ đầu.

**Code 9.1 — Đo khoảng cách hai cạnh bằng một CogCaliperTool cấu hình EdgeMode.Pair.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;

var caliper = new CogCaliperTool();
caliper.Region = measureRegion;        // dải quét vuông góc với chiều rộng chi tiết,
                                        // vẽ trong fixtured space (Chương 7, mục 7.4)

CogCaliper p = caliper.RunParams;
p.EdgeMode          = CogCaliperEdgeModeConstants.Pair;
p.Edge0Polarity      = CogCaliperPolarityConstants.LightToDark;  // vào chi tiết: sáng→tối
p.Edge1Polarity      = CogCaliperPolarityConstants.DarkToLight;  // ra khỏi chi tiết: tối→sáng
p.ContrastThreshold  = 20.0;

caliper.InputImage = fixturedImage;    // ảnh đã calibration + fixture
caliper.Run();

if (caliper.Results.Count == 0)
    return StationResult.MeasureFailed;

// Kết quả cặp biên: Width là khoảng cách giữa Edge0 và Edge1 — đã tính theo
// selected space của ảnh (mm, nhờ calibration ở Chương 7)
double widthMm = caliper.Results[0].Width;
double scoreEdgePair = caliper.Results[0].Score;
```

![Hình 9.1 — Dải quét caliper, hàm cường độ chiếu xuống một chiều, và điểm biên tìm được](../assets/ch09/hinh_9_1.png)
**Hình 9.1 — Dải quét caliper, hàm cường độ chiếu xuống một chiều, và điểm biên tìm được.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): ghép 2 phần. Trái: crop ảnh chi tiết MeoVision với overlay
> Region caliper (hình chữ nhật hẹp, nét vàng) vuông góc với cạnh cần đo, mũi tên chỉ hướng quét.
> Phải: đồ thị hàm cường độ theo chiều quét (trục X = vị trí dọc dải, trục Y = mức xám trung
> bình) — đường cong có bước nhảy rõ tại vị trí biên, chú thích mũi tên "ContrastThreshold" tại
> độ dốc bước nhảy và chấm đỏ đánh dấu vị trí sub-pixel tool báo cáo. Chụp/vẽ từ QuickBuild tab
> kết quả CogCaliperTool (job mẫu trong sdk/samples_quickbuild) hoặc dựng lại bằng draw.io nếu
> QuickBuild không xuất được đồ thị profile trực tiếp.

> ⚠️ **Cảnh báo:** dải quét (`Region`) phải đủ dài để chắc chắn phủ cả hai biên trong **mọi**
> tình huống chi tiết lệch trong phạm vi cho phép của fixturing — nhưng không dài đến mức trùm
> qua các đặc trưng khác gây nhiễu. Đây là lý do caliper luôn đứng **sau** fixture trong chuỗi:
> không có ROI "bám" theo chi tiết, một dải quét đủ ngắn để chính xác sẽ trật khỏi biên khi chi
> tiết xê dịch, còn một dải đủ dài để "chắc ăn" thì mất hết lợi thế về nhiễu và tốc độ.

### 9.1.3 Đọc kết quả: Width và Score

Code 9.1 lấy hai giá trị từ `caliper.Results[0]`: `Width` — khoảng cách đã quy đổi ra selected
space (mm sau calibration) — và `Score`, một con số 0–1 phản ánh **độ tin cậy của chính phép tìm
biên đó**, không phải độ tin cậy của con số `Width`. Score thấp thường đến từ tương phản yếu tại
đúng vị trí biên, nhiễu cục bộ trong dải quét, hoặc biên vật lý không sắc nét (ba-via, bề mặt
đúc thô — mục 9.4.1 bàn kỹ hệ quả của loại biên này lên độ chính xác đo).

> ⚠️ **Cảnh báo:** đừng chỉ kiểm `Results.Count == 0` rồi tin thẳng `Width`. Một cặp biên vẫn có
> thể được tìm thấy (Count > 0) với `Score` rất thấp — biên "gần đúng" thay vì biên thật. Đặt một
> ngưỡng `Score` tối thiểu để chấp nhận kết quả, đo giá trị ngưỡng đó từ golden set theo đúng
> nguyên tắc đã áp dụng cho `AcceptThreshold` của PMAlign ở Chương 8, mục 8.5 — không đoán một
> con số "nghe hợp lý". Không nên so sánh trực tiếp Score giữa hai job hoặc hai loại chi tiết khác
> nhau — nó chỉ có ý nghĩa tương đối trong cùng một cấu hình đo.

## 9.2 Tổng hợp caliper: CogFindLineTool và CogFindCircleTool

### 9.2.1 Vì sao cần nhiều caliper cho một cạnh

Một caliper đơn cho một điểm biên — đủ để đo khoảng cách giữa hai cạnh song song đơn giản
(mục 9.1), nhưng không đủ để trả lời "cạnh này có thẳng không, nghiêng bao nhiêu độ". `CogFindLineTool`
giải bài toán đó bằng cách rải **nhiều caliper** dọc theo một cạnh (thuộc tính `NumCalipers` của
`CogFindLine`, cấu hình caliper dùng chung nằm trong `CaliperRunParams` — chính là một `CogCaliper`
như mục 9.1), thu về nhiều điểm biên, rồi **khớp một đường thẳng** qua các điểm đó bằng bình
phương tối thiểu. `CogFindCircleTool` làm hệt vậy nhưng khớp đường tròn — hữu ích cho lỗ, trục,
biên cong.

Giá trị lớn nhất của cách tiếp cận "nhiều điểm rồi khớp hình" nằm ở khả năng **loại outlier**:
thuộc tính `NumToIgnore` cho phép tool tự loại bỏ một số điểm lệch xa nhất trước khi khớp — một
vết xước cắt ngang cạnh, một hạt bụi bám đúng chỗ quét, không còn kéo lệch cả đường thẳng nếu số
điểm hỏng nằm trong giới hạn `NumToIgnore`. Cờ `DecrementNumToIgnore` (kiểu bool) đổi cách tính
ngân sách đó: khi bật, số điểm được phép loại thực tế là `NumToIgnore` trừ đi số caliper con vốn
đã không tìm thấy biên nào — nói cách khác, một caliper "trắng tay" (không có ứng viên nào) tiêu
tốn luôn một suất trong ngân sách outlier, thay vì được cộng thêm miễn phí vào số điểm lệch còn
được phép bỏ.

Hai nguyên tắc thực dụng khi cấu hình `NumCalipers`/`NumToIgnore`: **nhiều caliper hơn không miễn
phí** — mỗi caliper con là một lần quét-và-tìm-biên riêng, `NumCalipers` tăng kéo thời gian chạy
tăng gần tuyến tính, nên chọn đủ để khớp hình học ổn định (một đường thẳng về lý thuyết chỉ cần 2
điểm, nhưng cần dư ra nhiều hơn để `RMSError` có ý nghĩa thống kê và để `NumToIgnore` có "khoảng
trống" mà loại outlier) trên một cạnh đủ dài, không rải dày đặc "cho chắc". Và **`NumToIgnore` quá
lớn tự đánh mất ý nghĩa của phép khớp**: nếu cho phép loại một tỉ lệ lớn số điểm, đường/tròn khớp
được có thể chỉ còn dựa trên thiểu số điểm "hợp ý" thuật toán, không còn đại diện cho hình dạng
thật của cạnh — luôn kiểm `NumPointsFound`/`RMSError` (mục 9.2.2) để phát hiện tình huống này.

**Bảng 9.2 — Cấu hình chính của CogFindLine/CogFindCircle (tham số dùng chung, khác kiểu geometry khớp).**

| Thuộc tính | Ý nghĩa |
|---|---|
| `NumCalipers` | Số caliper rải dọc cạnh/đường tròn ứng viên |
| `CaliperRunParams` | Cấu hình từng caliper con — cùng kiểu `CogCaliper` như mục 9.1 (polarity, contrast...) |
| `CaliperSearchLength`, `CaliperProjectionLength` | Kích thước dải quét của mỗi caliper con |
| `NumToIgnore`, `DecrementNumToIgnore` | Số điểm lệch xa nhất bị loại trước khi khớp hình học |
| `ExpectedLineSegment` / `ExpectedCircularArc` | Vị trí/hình dạng ước lượng ban đầu — thu hẹp không gian tìm kiếm |
| `RadiusConstraint`, `RadiusConstraintEnabled` (chỉ CogFindCircle) | Ràng buộc bán kính kỳ vọng — loại các đường tròn khớp sai kích cỡ |

### 9.2.2 Đọc kết quả: đường khớp và chất lượng khớp

Kết quả trả về (`tool.Results`, kiểu `CogFindLineResults`/`CogFindCircleResults`) không chỉ là
hình học khớp được — nó còn tự báo cáo **độ tin cậy của phép khớp** qua `RMSError`,
`NumPointsFound`, `NumPointsUsed`. Đây là bộ ba con số quan trọng hơn bản thân toạ độ đường/tâm:
`NumPointsFound` thấp nghĩa là nhiều caliper con không tìm được biên (cạnh đứt đoạn, tương phản
kém cục bộ); `RMSError` cao nghĩa là các điểm tìm được không thật sự nằm trên một đường thẳng/tròn
— cạnh thực tế có thể không thẳng như kỳ vọng, hoặc lẫn outlier vượt quá khả năng lọc của
`NumToIgnore`.

**Code 9.2 — CogFindLineTool: rải caliper dọc cạnh, khớp đường thẳng, kiểm chất lượng khớp.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;

var findLine = new CogFindLineTool();
findLine.Region = edgeSearchRegion;

CogFindLine fl = findLine.RunParams;
fl.NumCalipers       = 12;
fl.NumToIgnore        = 2;             // chịu được tối đa 2 điểm lệch (vết xước, bụi)
fl.CaliperRunParams.Edge0Polarity     = CogCaliperPolarityConstants.LightToDark;
fl.CaliperRunParams.ContrastThreshold = 20.0;

findLine.InputImage = fixturedImage;
findLine.Run();

CogFindLineResults results = findLine.Results;
if (results.NumPointsFound < 8 || results.RMSError > 0.05)   // mm, sau calibration
{
    logger.LogWarning(
        "FindLine yếu: found={Found}, RMS={Rms:F3}",
        results.NumPointsFound, results.RMSError);
    return StationResult.MeasureFailed;
}

CogLine fittedLine = results.GetLine();  // đường khớp — dùng cho Distance/giao điểm (9.3)
```

<!-- VERIFY: ngưỡng RMSError 0.05mm chỉ là ví dụ minh hoạ hợp lý về bậc độ lớn, KHÔNG phải số
đo thực tế — tác giả cần thay bằng ngưỡng đo được trên trạm thật (theo đúng tinh thần mục 9.4:
không đoán ngưỡng, đo từ golden set) -->

> ⚠️ **Cảnh báo:** con số `0.05` trong Code 9.2 chỉ minh hoạ đúng bậc độ lớn (đơn vị mm, sau
> calibration) — **đừng copy nguyên số này vào job thật**. Ngưỡng `RMSError` đúng cho một trạm cụ
> thể phải đo được từ chính trạm đó (golden set — mục 9.4, Chương 16), không có một con số chuẩn
> chung cho mọi bài toán đo.

`CogFindCircleTool` dùng hệt cấu trúc trên (`NumCalipers`, `CaliperRunParams`, `NumToIgnore`),
kết quả tra qua `results.GetCircle()` trả về `CogCircle` (thuộc tính `Radius`, `CenterX`, `CenterY`)
— dùng để đo đường kính lỗ, kiểm tra độ đồng tâm, hoặc làm input hình học cho mục 9.3.

## 9.3 Kết hợp hình học: khoảng cách, giao điểm, góc

Một khi có các đối tượng hình học đã khớp — đường thẳng từ `CogFindLineTool`, đường tròn từ
`CogFindCircleTool` — nhóm công cụ Geometry của VisionPro cho phép tính các đại lượng phái sinh
mà không cần tự viết công thức hình học: `CogDistanceTool` đo khoảng cách giữa hai đối tượng
(điểm-điểm, điểm-đường, đường-đường song song), các tool giao điểm tính điểm cắt giữa hai đường
thẳng hoặc đường thẳng với đường tròn, và công cụ đo góc tính góc hợp bởi hai đường.

Giá trị thực dụng lớn nhất của nhóm này: **kích thước không phải lúc nào cũng là khoảng cách giữa
hai cạnh song song** như mục 9.1. Ví dụ khoảng cách tâm-lỗ-đến-cạnh, hay góc giữa hai cạnh vát,
đòi hỏi trước tiên khớp hình học (đường/tròn — mục 9.2), sau đó tính đại lượng phái sinh bằng
tool geometry — thay vì lấy toạ độ thô rồi tự viết công thức lượng giác trong code C#. Giữ phép
tính hình học trong job VisionPro (thay vì rải rác trong code ứng dụng) giữ cho toàn bộ chuỗi đo
minh bạch và dễ debug bằng `CogRecordDisplay` (Chương 5, mục 5.5) — mọi bước trung gian đều xem
lại được trên overlay, không chỉ có con số cuối cùng.

> 🔍 **Đào sâu thêm:** với bài toán đo phức tạp hơn — nhiều đặc trưng hình học phối hợp, công
> thức dung sai lồng nhau — VisionPro còn có `CogGeomPatternTool` và các tiện ích geometry nâng
> cao khác nằm ngoài phạm vi chương này. Nguyên tắc chọn công cụ không đổi: ưu tiên tool có sẵn
> giải đúng bài toán hình học cần, chỉ rơi về code tự viết khi không có tool phù hợp.

## 9.4 Đo có dung sai: từ pixel đến milimet có ý nghĩa

### 9.4.1 Calibration là điều kiện cần, không phải điều kiện đủ

Chương 7 đã giải quyết việc quy đổi pixel sang milimet đúng nguyên lý. Nhưng "đúng nguyên lý" và
"đủ chính xác cho dung sai ± 0.05 mm" là hai câu hỏi khác nhau. Độ chính xác một phép đo caliper
phụ thuộc bậc thang các yếu tố cộng dồn: độ phân giải quang học (bao nhiêu µm thực ứng với 1 pixel
— Chương 3, mục 3.2), chất lượng calibration (RMS đã kiểm ở Chương 7), độ ổn định của chính biên
vật lý (cạnh cắt CNC sắc nét cho biên "sạch" hơn cạnh đúc nhựa có ba-via), và cuối cùng mới đến
tham số caliper (contrast threshold, số lượng caliper con).

Với trạm đo dung sai ± 0.05 mm trên FOV 100 mm (tỉ lệ ~0.041 mm/px — Chương 7), một pixel lỗi
đã chiếm 80% dung sai cho phép — đây chính là lý do Chương 2 nhấn mạnh telecentric lens cho bài
toán đo lường: distortion phối cảnh của lens thường (dù đã khử một phần bằng checkerboard) vẫn
để lại sai số dư đáng kể ở mức dung sai khắt khe này.

### 9.4.2 Lặp lại (repeatability) — con số phải tự đo, không suy diễn

**Repeatability** — độ dao động của kết quả đo khi đo lặp lại **cùng một chi tiết đứng yên** —
là con số quan trọng hơn độ chính xác tuyệt đối cho phần lớn ứng dụng kiểm tra dung sai. Cách đo:
chụp 30 lần liên tiếp một chi tiết cố định (không di chuyển giữa các lần chụp), chạy phép đo,
tính độ lệch chuẩn của kết quả. Con số này cho biết **sàn nhiễu** của cả hệ thống — dao động do
rung động cơ khí, nhiễu cảm biến, jitter ánh sáng — độc lập với việc chi tiết có đúng kích thước
hay không.

Nguyên tắc thực dụng: repeatability nên nhỏ hơn dung sai cho phép ít nhất một bậc (ví dụ dưới
10 µm cho dung sai ± 50 µm). Nếu repeatability đã chiếm phần lớn dung sai, không tham số caliper
nào cứu được — vấn đề nằm ở tầng thấp hơn: rung cơ khí, ánh sáng không ổn định, hoặc lens/camera
không đủ độ phân giải cho bài toán (quay lại Chương 2, Chương 3).

> 💡 **Mẹo thực chiến:** khi nghiệm thu một trạm đo, đòi hỏi cả hai con số — độ chính xác (so
> với chi tiết đã biết kích thước chuẩn, đo bằng thiết bị đo lường độc lập như CMM) **và** độ
> lặp lại (30 lần trên một chi tiết cố định). Một trạm có độ chính xác tốt nhưng lặp lại kém là
> trạm "may mắn đúng trung bình" — mỗi cycle đơn lẻ vẫn có thể sai đủ để lọt hàng lỗi.

### 9.4.3 Reproducibility — con số khác, đo theo cách khác

Repeatability (mục 9.4.2) đo dao động khi **mọi điều kiện giữ nguyên** — cùng chi tiết, cùng vị
trí, đo liên tiếp không gián đoạn. **Reproducibility** đo một câu hỏi khác: kết quả có ổn định
không khi có một điều kiện **thay đổi** mà lẽ ra không nên ảnh hưởng đến phép đo — chi tiết được
gắp ra rồi đặt lại (thay vì đứng yên tại chỗ), đo vào một ca khác (ánh sáng môi trường khác dù đã
có tuyến phòng thủ chống nhiễu — Chương 2, mục 2.3.4), hoặc sau khi máy đã tắt/khởi động lại. Hai
con số đo hai loại rủi ro khác nhau: dao động repeatability nhỏ (đo tại chỗ rất ổn định) nhưng dao
động reproducibility lớn (gắp ra đặt lại là kết quả khác đi) là dấu hiệu vấn đề nằm ở khâu **định
vị/fixturing** (Chương 7, mục 7.4) hoặc ở tính nhất quán giữa các lần khởi động hệ thống, chứ
không nằm ở bản thân phép đo caliper.

> 📌 **Lưu ý:** với trạm sản xuất thực tế, reproducibility thường quan trọng hơn repeatability
> thuần tuý — chi tiết trong sản xuất luôn "đến rồi đi", không bao giờ đứng yên nhiều lần liên
> tiếp như điều kiện đo repeatability lý tưởng. Một trạm chỉ nghiệm thu bằng repeatability (30 lần
> trên một chi tiết cố định) mà bỏ qua reproducibility có thể bỏ sót đúng loại sai số sẽ gặp nhiều
> nhất trong vận hành thật. Chương 16, mục 16.3 mở rộng nguyên tắc "đo đủ điều kiện thực tế, không
> chỉ đo trong phòng thí nghiệm" này thành quy trình nghiệm thu toàn hệ thống.

### 9.4.4 Hệ số bù hệ thống (systematic bias correction) — đóng vòng lặp sau khi so với CMM

Mục 9.4.2 đã đi 90% quãng đường: đo một chi tiết chuẩn bằng CMM (hoặc thước đo chính xác hơn hệ
vision), so với số vision đo được, ra một con số "độ chính xác". Bước còn thiếu là biến con số đó
thành **hành động khắc phục** thay vì chỉ dùng để nghiệm thu đạt/không đạt: nếu độ lệch so với CMM
**ổn định** qua nhiều lần đo (khác nhiễu ngẫu nhiên mà repeatability ở mục 9.4.2 đã đo), lưu độ
lệch đó làm một hằng số cộng/trừ, áp lại cho mọi phép đo sau này. Đây KHÔNG phải calibration hình
học (Chương 7) — calibration sửa quan hệ pixel↔mm cho *toàn bộ ảnh*; hệ số bù ở đây sửa một độ
lệch còn *sót lại* sau calibration, đặc thù cho đúng phép đo và đúng điều kiện chụp đang dùng
(giống cách một panme được hiệu chỉnh thêm bằng khối chuẩn dù đã có thang đo sẵn).

Hai điều kiện tiên quyết bắt buộc trước khi bù:

1. **Độ lệch phải có tính hệ thống, không phải nhiễu.** Đo lại chi tiết chuẩn vài lần; nếu độ dao
   động giữa các lần đo (repeatability, mục 9.4.2) đã lớn gần bằng độ lệch nghi ngờ, con số đó là
   nhiễu ngẫu nhiên — bù vào sẽ dịch chuyển sai số trung bình chứ không giảm nó. Chỉ bù khi độ lệch
   rõ ràng lớn hơn hẳn sàn nhiễu repeatability.
2. **Đo lại nếu bất kỳ điều kiện nào đổi.** Hệ số bù gắn chặt với đúng ánh sáng, đúng ROI, đúng
   tham số caliper lúc đo mẫu chuẩn — đổi ánh sáng, dạy lại pattern, hoặc chỉnh `ContrastThreshold`
   đều làm hệ số bù cũ mất hiệu lực; phải đo lại CMM và tính lại.

**Code 9.3 — Áp hệ số bù đã đo được vào kết quả caliper thô (nối tiếp Code 9.1).**

```csharp
// Đo một lần, ngoài chu trình sản xuất: so kết quả widthMm (Code 9.1) trên một chi tiết
// chuẩn với giá trị CMM đo được. Ví dụ đo được: vision báo 10.203mm, CMM báo 10.195mm
// → lệch hệ thống +0.008mm, ổn định qua 10 lần đo lặp (dao động repeatability ±0.002mm —
// nhỏ hơn hẳn độ lệch, nên đây là bias thật chứ không phải nhiễu). Ghi lại cùng ngày đo +
// điều kiện ánh sáng/tham số caliper lúc đo, để biết khi nào phải đo lại (Chương 16 — hồ sơ
// chất lượng).
const double systematicBiasMm = -0.008;   // trừ đi để kéo số vision về gần giá trị CMM

double widthMmRaw = caliper.Results[0].Width;              // số đọc trực tiếp từ tool — CHƯA bù
double widthMmCorrected = widthMmRaw + systematicBiasMm;   // số dùng để so dung sai
```

> ⚠️ **Cảnh báo:** đừng nhầm hệ số bù hệ thống này với việc "chỉnh `ContrastThreshold` cho ra số
> đẹp" — bù là một phép cộng/trừ tường minh áp SAU khi tool đã chạy xong với tham số ổn định,
> không phải mò tham số tool cho tới khi số đo trùng khớp giá trị mong muốn. Mò tham số theo cách
> thứ hai che giấu vấn đề thay vì đo nó.

## 9.5 [NÂNG CAO] Gauging GRR — đánh giá hệ đo vision như một thiết bị đo

### 9.5.1 Một chỉ số gộp thay vì hai con số riêng lẻ

Mục 9.4 đã tách repeatability và reproducibility thành hai câu hỏi riêng biệt — hữu ích để chẩn
đoán *nguồn* sai số, nhưng khi cần **một quyết định duy nhất** ("hệ đo này có đủ tốt để đưa vào
sản xuất không?"), ngành đo lường công nghiệp (Measurement System Analysis — MSA, chuẩn phổ biến
nhất là AIAG MSA) gộp cả hai vào một chỉ số: **%GRR** (Gauge Repeatability & Reproducibility).
Ý tưởng cốt lõi: coi *bản thân hệ đo* — camera, đèn, calibration, fixture, thuật toán caliper,
tất cả cộng lại — là một "thiết bị đo" như panme hay đồng hồ so, rồi hỏi thiết bị đó "ăn" bao
nhiêu phần trăm dung sai cho phép, trước khi bàn đến việc chi tiết thật sự khác nhau ra sao.

### 9.5.2 Vision không có "người đo" — thay bằng "điều kiện đo"

GRR truyền thống dùng nhiều **appraiser** (người đo, thường 2–3 người) đo lặp lại cùng một bộ
chi tiết để tách phương sai thiết bị (Equipment Variation — EV, tương ứng repeatability) khỏi
phương sai giữa người đo (Appraiser Variation — AV, tương ứng reproducibility). Hệ vision không
có ai cầm thước, nhưng có nguồn biến thiên đóng đúng vai trò "appraiser": chính là các **điều
kiện thay đổi** đã liệt kê ở mục 9.4.3 — gắp chi tiết ra rồi đặt lại, đo vào ca khác, đo sau khi
khởi động lại hệ thống. Thay "3 người đo" bằng "3 điều kiện tái lập" độc lập là cách chuyển đổi
tự nhiên nhất khi áp GRR cho một trạm vision.

### 9.5.3 Quy trình thực dụng: phương pháp range

Không cần dựng bảng ANOVA đầy đủ (nằm ngoài phạm vi CORE của chương) mới ước lượng được %GRR ở
mức đủ dùng cho quyết định thực tế — **phương pháp range** đơn giản hơn nhiều và vẫn là cách phổ
biến trong ngành cho việc đánh giá nhanh:

1. Chọn *N* chi tiết đại diện, trải hết dải dung sai (đúng tinh thần golden set — mục 9.4.2), ví
   dụ N = 10.
2. Với mỗi chi tiết, đo lặp lại *k* lần liên tiếp (k = 2–3, đúng điều kiện repeatability) dưới
   *M* điều kiện tái lập khác nhau (M = 2–3, đúng điều kiện reproducibility).
3. Với mỗi tổ hợp chi tiết × điều kiện, tính **range** (giá trị lớn nhất trừ nhỏ nhất trong *k*
   lần đo) — trung bình các range này trên toàn bộ N × M tổ hợp ước lượng **EV**.
4. Với mỗi chi tiết, tính range giữa trung bình của *M* điều kiện — trung bình các range này ước
   lượng **AV**.
5. Gộp hai nguồn phương sai theo căn bậc hai tổng bình phương (không cộng thẳng, vì hai nguồn độc
   lập): `GRR ≈ √(EV² + AV²)`.
6. `%GRR = GRR / dung_sai_cho_phép × 100%`.

Ví dụ minh hoạ bằng số cho trạm đo MeoVision (dung sai ± 0.05 mm, tức bề rộng dải 0.10 mm): giả
sử quy trình trên đo được EV ≈ 0.008 mm và AV ≈ 0.006 mm. Khi đó `GRR ≈ √(0.008² + 0.006²) ≈
0.010 mm`, và `%GRR ≈ 0.010 / 0.10 × 100% = 10%` — nằm đúng ở ranh giới giữa "tốt" và "chấp nhận
có điều kiện" theo Bảng 9.3 dưới đây.

**Bảng 9.3 — Ngưỡng chấp nhận %GRR phổ biến trong ngành (theo quy ước AIAG MSA).**

| %GRR | Đánh giá | Hàm ý cho trạm sản xuất |
|---|---|---|
| < 10% | Hệ đo tốt | Chấp nhận không cần bàn thêm |
| 10% – 30% | Chấp nhận có điều kiện | Tuỳ mức rủi ro của ứng dụng (Chương 16, mục 16.2.1 — hậu quả false accept/false reject không đối xứng); cân nhắc cải thiện nếu ứng dụng khắt khe |
| > 30% | Không chấp nhận | Phải cải thiện hệ đo (ánh sáng, độ phân giải, fixture — Chương 2/3/7) trước khi đưa vào sản xuất |

> 📌 **Lưu ý:** N = 10 chi tiết là quy ước phổ biến của MSA, không phải con số bắt buộc — cùng
> logic với việc chọn 30 lần chụp cho repeatability ở mục 9.4.2: đủ mẫu để ước lượng phương sai
> ổn định mà không tốn quá nhiều thời gian nghiệm thu. %GRR nên đo **một lần khi nghiệm thu** và
> đo lại **định kỳ** hoặc sau mỗi lần hiệu chỉnh lớn (Chương 16, mục 16.4.3) — nó bổ sung cho,
> chứ không thay thế, việc giám sát xu hướng score liên tục theo từng cycle đã học ở Chương 16.

## Tổng kết chương

- `CogCaliperTool` tìm biên bằng cách chiếu một dải hẹp xuống một chiều, lấy trung bình theo
  chiều rộng để khử nhiễu — đây là nguồn gốc độ chính xác sub-pixel, và là công cụ khác hẳn về
  bản chất so với định vị mẫu (PMAlign trả lời "ở đâu", caliper trả lời "cách bao xa").
- Khai báo `Edge0Polarity`/`Edge1Polarity` tường minh theo chiều sáng-tối thực tế của trạm, không
  để `DontCare` "cho chắc" — polarity đúng loại bỏ ngay từ đầu các ứng viên biên sai (bóng đổ,
  vết bẩn).
- `CogFindLineTool`/`CogFindCircleTool` rải nhiều caliper rồi khớp hình học bằng bình phương tối
  thiểu, có khả năng loại outlier qua `NumToIgnore`. Luôn kiểm `NumPointsFound` và `RMSError`
  trước khi tin kết quả — chúng cho biết *độ tin cậy* của phép khớp, không chỉ bản thân toạ độ.
- Các đại lượng đo phái sinh (khoảng cách, giao điểm, góc) nên tính bằng tool Geometry của
  VisionPro trên các đối tượng hình học đã khớp, giữ toàn chuỗi đo minh bạch và debug được qua
  CogRecordDisplay — thay vì tự viết công thức lượng giác rải rác trong code ứng dụng.
- Calibration đúng nguyên lý (Chương 7) là điều kiện cần, không đủ cho dung sai khắt khe: độ
  chính xác thực tế phụ thuộc cả độ phân giải quang học, chất lượng biên vật lý, và tham số
  caliper. **Repeatability** đo bằng 30 lần chụp một chi tiết cố định là con số bắt buộc phải đo,
  không suy diễn từ calibration RMS hay từ tài liệu lens. **Reproducibility** (đo khi có điều kiện
  thay đổi — gắp/đặt lại, khác ca, khởi động lại máy) là con số khác, thường quan trọng hơn cho
  trạm sản xuất thực tế, và cần đo riêng chứ không suy ra từ repeatability.
- So với CMM (mục 9.4.2) không chỉ để nghiệm thu đạt/không đạt — nếu độ lệch **ổn định** (lớn hơn
  hẳn nhiễu repeatability), lưu nó làm **hệ số bù hệ thống** áp lại cho mọi phép đo sau này (mục
  9.4.4); đo lại mỗi khi ánh sáng/ROI/tham số caliper đổi, vì hệ số bù gắn chặt với đúng điều kiện
  lúc đo mẫu chuẩn.
- **%GRR** gộp repeatability và reproducibility thành một chỉ số duy nhất để đánh giá cả hệ đo
  như một thiết bị đo lường — vision thay "người đo" bằng "điều kiện tái lập" (gắp/đặt lại, khác
  ca...); ngưỡng phổ biến theo AIAG MSA: < 10% tốt, 10–30% chấp nhận có điều kiện, > 30% phải cải
  thiện hệ đo trước khi sản xuất.

## Lỗi thường gặp

**Lỗi 1 — Dùng khoảng cách toạ độ từ PMAlign để đo dung sai.** Hiện tượng: kết quả đo dao động
lớn giữa các lần đo cùng một chi tiết đứng yên. Nguyên nhân: PMAlign tối ưu cho định vị bền vững
(chịu xoay/scale/nhiễu), không tối ưu cho độ lặp lại sub-pixel. Cách tránh: dùng caliper cho mọi
phép đo có dung sai; dành PMAlign cho định vị và fixturing (Chương 7, Chương 8).

**Lỗi 2 — Polarity `DontCare` khi biết trước chiều sáng-tối.** Hiện tượng: caliper thỉnh thoảng
bắt nhầm biên bóng đổ thay vì biên chi tiết thật, đặc biệt khi ánh sáng môi trường dao động. Nguyên
nhân: bỏ qua thông tin sẵn có (chiều tương phản đã biết từ cách bố trí đèn) khiến tool phải cân
nhắc mọi ứng viên. Cách tránh: khai báo `Edge0Polarity`/`Edge1Polarity` đúng theo cách chiếu sáng
đã chọn (Chương 2).

**Lỗi 3 — Region quá ngắn không phủ hết dải xê dịch của chi tiết.** Hiện tượng: đo đúng khi chi
tiết ở giữa vùng làm việc, báo lỗi "không tìm thấy biên" khi chi tiết lệch về mép phạm vi fixturing
cho phép. Nguyên nhân: dải quét vẽ theo vị trí chi tiết lúc setup, không tính đến toàn bộ phạm vi
xê dịch đã fixture. Cách tránh: vẽ Region đủ dài cho trường hợp lệch xa nhất trong phạm vi cho
phép (Chương 7, mục 7.4), kiểm bằng ảnh mẫu ở các vị trí biên của phạm vi đó.

**Lỗi 4 — Không kiểm `RMSError`/`NumPointsFound`, chỉ lấy toạ độ khớp.** Hiện tượng: thỉnh thoảng
một phép đo "có kết quả" nhưng sai lệch lớn, không có cảnh báo nào trước đó. Nguyên nhân: đường/tròn
vẫn khớp được về mặt toán học ngay cả khi phần lớn điểm biên không tìm thấy hoặc lệch xa — thuật
toán bình phương tối thiểu không tự báo "tôi không chắc" trừ khi ta đọc `RMSError`. Cách tránh:
luôn kiểm ngưỡng `NumPointsFound`/`RMSError` trước khi chấp nhận kết quả (Code 9.2).

**Lỗi 5 — Nghiệm thu trạm đo chỉ bằng độ chính xác, bỏ qua độ lặp lại.** Hiện tượng: trạm "đo
đúng" khi nghiệm thu bằng vài chi tiết mẫu, nhưng sản xuất thực tế có tỉ lệ lọt hàng lỗi không
giải thích được. Nguyên nhân: độ chính xác trung bình tốt có thể che giấu độ lặp lại kém — mỗi
lần đo dao động quanh giá trị đúng với biên độ lớn. Cách tránh: đo repeatability bằng 30 lần chụp
một chi tiết cố định trước khi chấp nhận trạm (mục 9.4.2); xem thêm quy trình nghiệm thu đầy đủ
ở Chương 16.

**Lỗi 6 — Áp hệ số bù hệ thống mà không kiểm đó là bias thật hay chỉ là nhiễu của một lần đo.**
Hiện tượng: sau khi "hiệu chỉnh" theo CMM, sai số trung bình không giảm mà đôi khi còn tệ hơn ở
lô hàng sau. Nguyên nhân: độ lệch đo được lần đó nằm trong biên độ nhiễu ngẫu nhiên (repeatability
chưa đo hoặc bị bỏ qua) chứ không phải độ lệch hệ thống ổn định — bù một con số nhiễu vào chỉ dịch
chuyển sai số trung bình theo hướng khác. Cách tránh: đo chi tiết chuẩn nhiều lần trước khi kết
luận có bias hệ thống (mục 9.4.4); và nhớ đo lại hệ số bù sau bất kỳ thay đổi ánh sáng/ROI/tham số
nào — hệ số bù cũ áp cho điều kiện mới cũng sai như không bù.

\newpage

# Chương 10 — Phân tích Blob và kiểm tra khuyết tật

Trước khi lắp ráp, mỗi vỏ nhôm của MeoVision phải có đúng 4 miếng đệm cao su dán sẵn ở 4 góc mặt
dưới — thiếu một miếng, hoặc một miếng dán rách/gấp mép, chi tiết sẽ lọt xuống trạm kế tiếp và
hỏng cả cụm lắp ráp. Đây là bài toán khác hẳn hai chương trước: không phải "vật thể này ở đâu"
(PMAlign) hay "cạnh này cách cạnh kia bao xa" (Caliper), mà là "**có bao nhiêu vùng thoả một điều
kiện, và từng vùng trông như thế nào**". Đếm, đo diện tích, kiểm hình dạng của các vùng rời rạc
trên ảnh — đó là địa hạt của **blob analysis**.

"Blob" đơn giản là một vùng pixel liền nhau cùng thoả một điều kiện phân loại (thường là: sáng
hơn hay tối hơn một ngưỡng). `CogBlobTool` biến một ảnh xám thành một tập hợp các blob như vậy,
rồi đo hàng loạt thuộc tính hình học trên từng blob — diện tích, chu vi, tâm, độ dài trục chính.
Cùng một tool, hai bài toán tưởng chẳng liên quan trong chương này — đếm miếng đệm cao su (mục
10.3) và bắt vết xước bề mặt (mục 10.4) — thực chất chỉ khác nhau ở cách chuẩn bị ảnh đưa vào
trước khi blob nhìn thấy nó.

## 10.1 CogBlobTool: phân vùng, kết nối, hình thái học

### 10.1.1 Segmentation — bước quyết định blob "nhìn thấy" gì

Bước đầu tiên và quan trọng nhất là **segmentation**: chuyển ảnh xám (256 mức) thành ảnh nhị phân
foreground/background — chỉ sau bước này khái niệm "blob" mới tồn tại. Cấu hình segmentation nằm
trong `tool.RunParams.SegmentationParams` (kiểu `CogBlobSegmentationParams`), thiết lập qua một
trong các method `SetSegmentation...`:

**Bảng 10.1 — Các chế độ segmentation của CogBlobSegmentationParams.**

| Method | Cơ chế | Dùng khi |
|---|---|---|
| `SetSegmentationHardFixedThreshold(threshold, polarity)` | Một ngưỡng cố định, cắt nhị phân dứt khoát | Tương phản ổn định, đã kiểm soát ánh sáng tốt (trường hợp lý tưởng) |
| `SetSegmentationHardRelativeThreshold(tailLow, tailHigh, threshold, polarity)` | Ngưỡng tính tương đối theo phân bố histogram của ảnh, loại bỏ đuôi `tailLow`/`tailHigh`% | Độ sáng nền dao động nhẹ giữa các ảnh, muốn ngưỡng "tự thích nghi" phần nào |
| `SetSegmentationHardDynamicThreshold(tailLow, tailHigh, polarity)` | Ngưỡng tính động hoàn toàn từ ảnh hiện tại (kiểu Otsu — xem Chương 4, mục 4.2) | Độ sáng nền dao động đáng kể, không có một ngưỡng cố định nào "luôn đúng" |
| `SetSegmentationSoftFixedThreshold(low, high, softness, polarity)` | **Soft threshold**: vùng chuyển tiếp `[low, high]` được gán trọng số mờ dần thay vì cắt cứng | Biên đối tượng không sắc nét — mờ, có anti-aliasing, hoặc muốn đo diện tích chính xác hơn ở mức sub-pixel |
| `SetSegmentationSoftRelativeThreshold(...)` | Kết hợp ngưỡng tương đối + biên mờ (soft) | Vừa cần thích nghi độ sáng, vừa cần biên mờ mượt |
| `SetSegmentationSubtractionImage(...)` | Phân vùng dựa trên **hiệu số** so với một ảnh chuẩn, thay vì so với ngưỡng cường độ tuyệt đối | Kiểm khuyết tật — mục 10.4 |

Tham số `polarity` (`CogBlobSegmentationPolarityConstants.DarkBlobs` hoặc `LightBlobs`) khai báo
blob là vùng **tối hơn** hay **sáng hơn** ngưỡng — cùng vai trò với `Edge0Polarity` ở Caliper
(Chương 9): biết trước và khai báo tường minh luôn tốt hơn để tool tự đoán.

> ⚠️ **Cảnh báo:** `SetSegmentationHardDynamicThreshold` (Otsu, Chương 4 mục 4.2.2) tính ngưỡng
> **hoàn toàn từ chính ảnh đang xử lý**, không tham chiếu về ngưỡng đã dạy lúc setup hay ảnh trước
> đó — nếu đúng một ảnh trong hàng chục nghìn ảnh sản xuất bất thường (quá tối/sáng, thiếu vật thể,
> vệt sáng lạ), ngưỡng có thể "đi hoang" xa hẳn vị trí hợp lý, vì Otsu chỉ đi tìm đáy thung lũng
> histogram mà không có ràng buộc nó phải nằm gần đâu. VisionPro không lộ ra ngưỡng Dynamic vừa
> tính được để ứng dụng tự kiểm tra và "kẹp" lại — đây là khác biệt quan trọng so với
> `SetSegmentationHardRelativeThreshold`, nơi ngưỡng tuy cũng tính lại mỗi ảnh nhưng bị RÀNG BUỘC
> nằm đúng tại một vị trí phần trăm cố định giữa hai đuôi histogram đã trim (`hardRelativeThreshold`
> % khoảng cách từ tail thấp đến tail cao) — không "nhảy" tự do đi tìm đáy thung lũng như Otsu, nên
> ít có kiểu bất thường cực đoan hơn. Thực dụng: ưu tiên Relative khi có thể; nếu bắt buộc dùng
> Dynamic (ánh sáng dao động quá mạnh để Relative đủ dùng), giám sát gián tiếp qua số lượng/diện
> tích blob tìm được mỗi cycle — ghi lại **mọi** cycle để theo dõi xu hướng, đúng kỹ thuật ở
> Chương 16, mục 16.4.1 — ngưỡng trôi xa thường kéo theo số đếm hoặc diện tích bất thường, phát
> hiện được dù không đọc trực tiếp con số ngưỡng.

![Hình 10.1 — Segmentation, connectivity và morphology trên 4 miếng đệm cao su MeoVision](../assets/ch10/hinh_10_1.png)
**Hình 10.1 — Segmentation, connectivity và morphology trên 4 miếng đệm cao su MeoVision.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): 3 khung ảnh liền kề cùng kích thước, cùng vùng ảnh mặt dưới
> vỏ nhôm có 4 miếng đệm cao su. (1) "Ảnh gốc": ảnh xám gốc, một miếng đệm có vệt phản xạ sáng
> ở giữa. (2) "Sau segmentation": ảnh nhị phân — miếng đệm có vệt phản xạ hiện ra thành "vành
> khuyên" (lỗ sáng ở giữa vùng tối). (3) "Sau ConnectivityCleanup.Fill + CloseSquare": cùng ảnh
> nhị phân nhưng 4 miếng đệm đã thành khối đặc liền mạch, không còn lỗ. Chú thích dưới mỗi khung.
> Chụp từ QuickBuild tab kết quả CogBlobTool (bật hiển thị segmented image qua
> `SaveSegmentedImage`) hoặc dựng lại minh hoạ nếu chưa có ảnh miếng đệm thật.

> 📌 **Lưu ý:** khác biệt "hard" và "soft" ở đây không phải thuật ngữ marketing — nó là ranh giới
> nhị phân cứng (một pixel là blob hoặc không) so với trọng số mờ liên tục ở vùng biên. Soft
> threshold cho phép các phép đo diện tích/chu vi chính xác hơn ở mức dưới-pixel, đổi lại tính
> toán phức tạp hơn một chút — với hầu hết bài toán đếm/kiểm tra hiện diện, hard threshold là đủ.

**Code 10.1 — Cấu hình segmentation, connectivity, morphology cho đếm miếng đệm cao su.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;

var blobTool = new CogBlobTool();
blobTool.Region = padSearchRegion;      // vẽ trong fixtured space — Chương 7, mục 7.4

CogBlob p = blobTool.RunParams;

// Đệm cao su đen trên nền nhôm sáng (anodized) → blob tối trên nền sáng
p.SegmentationParams.SetSegmentationHardFixedThreshold(
    hardFixedThreshold: 90,
    polarity: CogBlobSegmentationPolarityConstants.DarkBlobs);

// Connectivity: blob chạm nhau ở 1 góc có tính là 1 blob không, và bỏ nhiễu hạt quá nhỏ
p.ConnectivityMode        = CogBlobConnectivityModeConstants.Labeled;
p.ConnectivityMinPixels    = 15;          // loại nhiễu hạt nhỏ hơn 15 px
p.ConnectivityCleanup      = CogBlobConnectivityCleanupConstants.Fill;   // lấp lỗ nhỏ trong blob

// Morphology: đóng (close) để nối liền blob bị đứt đoạn do phản xạ cục bộ trên cao su
p.MorphologyOperations.Add(CogBlobMorphologyConstants.CloseSquare);

blobTool.InputImage = fixturedImage;
blobTool.Run();
```

`ConnectivityCleanup.Fill` đáng chú ý riêng: một miếng đệm cao su có vệt phản xạ sáng nhỏ ở giữa
(do ánh sáng ring) có thể bị segmentation cắt thành "vành khuyên tối bao quanh lõi sáng" thay vì
một blob đặc — `Fill` lấp các lỗ nhỏ hoàn toàn nằm trong một blob, coi cả miếng đệm là một vùng
liền, đúng với thực tế vật lý của nó.

### 10.1.2 Morphology — sửa hình dạng blob trước khi đo

`MorphologyOperations` (kiểu `CogBlobMorphologyCollection`, thêm phần tử qua `.Add(...)`) áp dụng
một chuỗi phép toán hình thái học lên ảnh nhị phân **trước khi** đo thuộc tính — đây là bước "dọn
dẹp" hình dạng, tương tự khái niệm đã giới thiệu ở Chương 4, mục 4.3, giờ áp dụng trực tiếp trong
tool:

**Bảng 10.2 — Bốn phép toán hình thái học cơ bản (biến thể Square/Horizontal/Vertical).**

| Phép toán | Hiệu ứng |
|---|---|
| `Erode*` | Ăn mòn biên blob — thu nhỏ, có thể tách hai blob dính nhau qua một cầu mảnh |
| `Dilate*` | Giãn nở biên blob — phình to, có thể nối liền các blob gần nhau |
| `Open*` (= Erode rồi Dilate) | Loại bỏ các tua/gai nhỏ nhô ra, giữ nguyên kích thước tổng thể |
| `Close*` (= Dilate rồi Erode) | Lấp các khe/lỗ nhỏ bên trong hoặc giữa các blob gần nhau, giữ nguyên kích thước tổng thể |

Thứ tự các phép toán trong `MorphologyOperations` được áp dụng tuần tự — thêm `CloseSquare` rồi
`OpenSquare` cho hiệu ứng khác với thứ tự ngược lại. Nguyên tắc chọn: **Open** khi nhiễu là các
đốm nhỏ rời rạc lẫn vào nền; **Close** khi blob thật bị "đứt gãy" thành nhiều mảnh do nhiễu cục bộ
(đúng tình huống miếng đệm cao su ở Code 10.1).

> ⚠️ **Cảnh báo:** morphology áp dụng đều cho toàn bộ blob, kể cả những blob "đúng" không cần
> sửa. `Close` quá mạnh (nhiều lần liên tiếp) có thể nối liền hai miếng đệm đặt gần nhau thành
> một blob duy nhất — biến lỗi "thiếu 1 miếng, thừa diện tích ở miếng bên cạnh" thành kết quả
> "đủ 4 miếng" giả. Luôn kiểm morphology bằng ảnh có chủ đích đặt hai đối tượng gần nhau ở
> khoảng cách tối thiểu cho phép, không chỉ ảnh "đẹp".

## 10.2 Đo thuộc tính blob và lọc theo thuộc tính

### 10.2.1 Đọc kết quả: từ CogBlobResults đến từng blob

`tool.Results` trả về kiểu `CogBlobResults` — khác với các tool trước, đây **không** phải một
tập hợp có thể duyệt trực tiếp. Phải gọi `GetTopLevelBlobs(filtered: true)` (chỉ lấy blob không
nằm trong blob khác, sau khi đã lọc) hoặc `GetBlobs(filtered: true)` (gồm cả blob con/lỗ nếu có
cấu trúc lồng nhau) để nhận về một `CogBlobResultCollection` — tập hợp này mới có `Count` và chỉ
số `Item`/indexer quen thuộc.

**Bảng 10.3 — Các thuộc tính chính của một CogBlobResult.**

| Thuộc tính | Ý nghĩa |
|---|---|
| `Area` | Diện tích blob — **pixel²** nếu ảnh/tool chưa gắn calibration, **mm²** (hoặc đơn vị vật lý khác) nếu đang làm việc trong calibrated space đã thiết lập ở Chương 7 |
| `Perimeter` | Chu vi biên blob |
| `CenterOfMassX`, `CenterOfMassY` | Toạ độ trọng tâm (centroid) |
| `Elongation` | Tỉ lệ độ dài trục chính/trục phụ — gần 1 là tròn/vuông, lớn là dài-hẹp |
| `Angle` | Góc trục chính của blob |
| `GetBoundingBox(axis)` | Hình chữ nhật bao — theo trục ảnh hoặc theo trục chính của blob |
| `FilteredOut` | Cờ cho biết blob này có bị loại bởi điều kiện lọc (mục 10.2.2) hay không |
| `ID` | Định danh duy nhất của blob trong kết quả — dùng để truy lại qua `GetBlobByID` |

**Code 10.2 — Đếm miếng đệm cao su, kiểm diện tích/hình dạng từng miếng.**

```csharp
using Cognex.VisionPro.Blob;

CogBlobResultCollection pads = blobTool.Results.GetTopLevelBlobs(filtered: true);

if (pads.Count != 4)
{
    logger.LogWarning("Đếm được {Count} miếng đệm, mong đợi 4", pads.Count);
    return StationResult.PadCountMismatch;
}

for (int i = 0; i < pads.Count; i++)
{
    CogBlobResult pad = pads[i];

    // Diện tích/hình dạng bất thường: miếng dán rách (diện tích nhỏ hơn) hoặc
    // hai miếng dính chồng lên nhau (diện tích lớn hơn, Elongation bất thường)
    // Dải 6.0-9.0 giả định pads.Area đang ở mm² — chỉ đúng khi blobTool chạy trên fixturedImage
    // (đã qua Calibration + Fixture ở Chương 7); nếu chạy trên ảnh chưa calibration, Area trả về
    // pixel² và dải số này phải đổi tương ứng theo độ phân giải camera thật.
    bool areaOk = pad.Area >= 6.0 && pad.Area <= 9.0;          // mm² — ví dụ minh hoạ
    bool shapeOk = pad.Elongation <= 1.3;                        // gần tròn/vuông

    if (!areaOk || !shapeOk)
    {
        logger.LogWarning(
            "Miếng đệm ID={Id} bất thường: Area={Area:F2}, Elongation={Elong:F2}",
            pad.ID, pad.Area, pad.Elongation);
        return StationResult.PadShapeMismatch;
    }
}
```

<!-- VERIFY: dải diện tích 6.0-9.0 mm² và ngưỡng Elongation 1.3 chỉ là số minh hoạ hợp lý về bậc
độ lớn cho một miếng đệm cỡ nhỏ — tác giả cần thay bằng số đo thực tế từ miếng đệm và calibration
thật của trạm (đúng tinh thần mục 9.4: không đoán ngưỡng, đo từ mẫu thật) -->

### 10.2.2 Lọc ngay trong tool: RunTimeMeasures

Thay vì đọc hết mọi blob rồi lọc bằng code C# như Code 10.2, VisionPro cho phép khai báo điều
kiện lọc **ngay trong tool** qua `tool.RunParams.RunTimeMeasures` (kiểu `CogBlobMeasureCollection`) —
mỗi phần tử là một `CogBlobMeasure` gắn một phép đo (`CogBlobMeasureConstants.Area`, `.Perimeter`,
`.Elongation`...) với một khoảng lọc (`FilterMode`, `FilterRangeLow`, `FilterRangeHigh`):

```csharp
// Loại thẳng các blob có diện tích ngoài [6.0, 9.0] mm² NGAY TRONG TOOL — không
// cần lọc lại bằng code; kết quả trả về đã chỉ còn blob "hợp lệ về diện tích"
var areaFilter = new CogBlobMeasure(CogBlobMeasureConstants.Area);
areaFilter.FilterMode      = CogBlobFilterModeConstants.IncludeBlobsInRange;
areaFilter.FilterRangeLow   = 6.0;
areaFilter.FilterRangeHigh  = 9.0;
blobTool.RunParams.RunTimeMeasures.Add(areaFilter);
```

Hai cách đều đúng; khác biệt là **nơi** logic "thế nào là hợp lệ" sống. Lọc trong tool phù hợp
khi tiêu chí cố định và đơn giản (một khoảng diện tích); lọc bằng code phù hợp khi tiêu chí phức
tạp hơn, phối hợp nhiều thuộc tính, hoặc cần log chi tiết lý do từ chối như Code 10.2. Không nhất
thiết chọn một — nhiều trạm dùng `RunTimeMeasures` để loại nhiễu rõ ràng (mảnh vụn quá nhỏ) ngay
trong tool, rồi vẫn kiểm chi tiết hơn bằng code cho phần còn lại.

## 10.3 Bài toán đếm và kiểm tra thiếu/thừa linh kiện

Bài toán đếm — như Code 10.2 — là ứng dụng blob phổ biến nhất trong kiểm tra công nghiệp: đếm
đủ linh kiện đã lắp (ốc vít, miếng đệm, chân cắm), đếm đúng số lỗ đã khoan, phát hiện linh kiện
thừa (lẫn từ lô khác). Ba nguyên tắc thực dụng đúc kết từ mục 10.1–10.2:

1. **Kiểm số lượng TRƯỚC, kiểm hình dạng từng đối tượng SAU.** Nếu đếm được sai số lượng ngay từ
   đầu, không cần tốn công kiểm diện tích/elongation của từng blob — trả kết quả lỗi ngay (Code
   10.2 làm đúng thứ tự này: `pads.Count != 4` return trước vòng lặp).
2. **Đừng chỉ khai báo "đủ N blob" — kiểm cả từng blob có đúng NGHĨA LÀ đối tượng cần đếm.** Hai
   tình huống khác hẳn nhau về bản chất có thể cho ra **cùng một con số blob**: (a) đủ cả 4 miếng
   đệm nhưng 2 miếng đặt sát nhau dính thành 1 blob (4 miếng thật → 3 blob đo được), và (b) thật
   sự thiếu 1 miếng, 3 miếng còn lại tách rời hoàn toàn (3 miếng thật → 3 blob đo được). Đếm ra
   "3" không cho biết đang gặp tình huống nào — nếu ngưỡng đếm đặt lỏng lẻo (ví dụ "≥ 3" thay vì
   "= 4") cả hai đều lọt qua như nhau. Luôn kiểm diện tích/hình dạng từng blob để phân biệt "N đối
   tượng đúng" với "N vùng tối bất kỳ" — diện tích blob dính (a) sẽ lớn bất thường so với một
   miếng đệm đơn, ngay cả khi tổng số blob trông "hợp lý".
3. **Blob KHÔNG phân loại được — nó chỉ đo hình học.** Đếm được 4 vùng tối đúng kích cỡ không tự
   động nghĩa là 4 miếng đệm cao su thật — một vệt dầu tròn cùng kích cỡ cũng qua được bộ lọc này.
   Với bài toán cần phân biệt *loại* đối tượng (không chỉ hình dạng/kích thước), PMAlign (Chương
   8) hoặc — khi rule-based không đủ — deep learning (mục 10.5) là công cụ đúng hơn.

## 10.4 Kiểm khuyết tật bề mặt: trừ ảnh chuẩn

### 10.4.1 Vì sao threshold đơn giản không bắt được vết xước

Bề mặt anodized của vỏ nhôm MeoVision có độ sáng gần như đồng đều — một vết xước hay vết rỗ tạo
ra thay đổi độ sáng cục bộ **rất nhỏ**, thường không đủ tương phản để một ngưỡng cố định (mục
10.1) tách nó ra khỏi nhiễu hạt bình thường của bề mặt. Kỹ thuật đứng sau `SetSegmentationSubtractionImage`
giải quyết đúng vấn đề này: thay vì hỏi "pixel này sáng/tối hơn một ngưỡng tuyệt đối", nó hỏi
"pixel này khác bao nhiêu **so với ảnh chuẩn** tại đúng vị trí đó" — và vết xước, dù không đủ
tương phản để vượt ngưỡng tuyệt đối, thường đủ khác biệt so với chính nó lúc "sạch" để vượt ngưỡng
hiệu số.

### 10.4.2 Điều kiện để trừ ảnh hoạt động đúng

Kỹ thuật này chỉ tin cậy khi ba điều kiện được đảm bảo — cả ba đều là hệ quả trực tiếp của những
gì đã học ở các chương trước:

- **Ảnh chuẩn (golden image) và ảnh kiểm phải cùng hệ toạ độ.** Chi tiết phải được fixture về
  cùng một vị trí/góc xoay chuẩn trước khi trừ (Chương 7, mục 7.4) — lệch dù chỉ nửa pixel cũng
  tạo ra hiệu số giả dọc theo mọi biên thật của chi tiết, chứ không chỉ ở chỗ có khuyết tật.
- **Ánh sáng giữa ảnh chuẩn và ảnh kiểm phải nhất quán.** Đây là hệ quả trực tiếp của nguyên tắc
  kiểm soát ánh sáng ở Chương 2 — trừ ảnh khuếch đại mọi khác biệt ánh sáng thành "khuyết tật giả".
- **Ảnh chuẩn phải đại diện cho bề mặt "tốt" thật**, không phải một chi tiết ngẫu nhiên chưa kiểm
  tra kỹ. Sai lầm ở đây tạo hiệu ứng ngược mục 8.2 của Chương 8: một khuyết tật vô tình có mặt
  trên chính ảnh chuẩn sẽ bị "trừ mất", không bao giờ bị phát hiện ở các chi tiết sau.

**Code 10.3 — Phát hiện khuyết tật bề mặt bằng trừ ảnh chuẩn + Blob.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;

var defectTool = new CogBlobTool();
defectTool.Region = surfaceInspectRegion;

// preMap/postMap là bảng tra cứu (lookup table): mỗi giá trị pixel được thay bằng
// map[giá_trị_pixel]. Truyền map "identity" (map[i] = i, không đổi gì) khi chưa cần
// biến đổi độ sáng trước/sau phép trừ — an toàn hơn truyền null cho hai tham số này.
byte[] identityMap = new byte[256];
for (int i = 0; i < 256; i++) identityMap[i] = (byte)i;
// identityMap ở đây chỉ là điểm khởi đầu "không đổi gì" cho preMap/postMap, để ví dụ tập trung
// vào cấu hình trừ ảnh. Với postMap = identity, kết quả sau trừ vẫn là ảnh mức xám (hiệu số thô,
// chưa nhị phân) — muốn postMap tự biến hiệu số thành 0/255 như mô tả ở Mẹo thực chiến dưới, cần
// thay identityMap bằng một lookup table dạng bậc thang (step function) quanh ngưỡng hiệu số mong
// muốn, ví dụ postMap[i] = (i < 128 - nguong || i > 128 + nguong) ? 255 : 0.

CogBlob p = defectTool.RunParams;
p.SegmentationParams.SetSegmentationSubtractionImage(
    preMap: identityMap,
    subtractionImage: goldenSurfaceImage,   // ảnh chuẩn, đã fixture cùng hệ toạ độ
    postMap: identityMap,
    scalingValue: 1,
    subtractionImageOffsetX: 0,
    subtractionImageOffsetY: 0);

p.ConnectivityMinPixels = 5;    // vết xước/rỗ nhỏ vẫn phải đủ lớn để không phải nhiễu hạt

defectTool.InputImage = fixturedImage;  // ảnh kiểm — cùng fixtured space với goldenSurfaceImage
defectTool.Run();

CogBlobResultCollection defects = defectTool.Results.GetTopLevelBlobs(filtered: true);
bool surfaceOk = defects.Count == 0;
```

> 💡 **Mẹo thực chiến:** `preMap` áp dụng lên ảnh **đầu vào** trước khi trừ (hữu ích khi cần bù
> chênh lệch độ sáng đã biết trước giữa ảnh chuẩn và ảnh kiểm); `postMap` áp dụng lên **kết quả**
> sau khi trừ — đây là nơi CÓ THỂ biến độ lớn hiệu số thành 0/255 để Blob nhìn thấy, tương đương
> vai trò một ngưỡng, NẾU postMap được xây dựng thành một lookup table dạng bậc thang thay vì
> identity như trong Code 10.3 ở trên (Code 10.3 giữ postMap = identity để giữ ví dụ đơn giản,
> nghĩa là ảnh sau trừ vẫn ở dạng hiệu số mức xám thô — cần tự thay bằng bậc thang, hoặc dùng thêm
> một bước segmentation ngưỡng riêng, để có blob nhị phân thật sự). Ngưỡng đó (giá trị hiệu số nào
> bắt đầu được coi là khuyết tật) nên được tinh chỉnh bằng một bộ ảnh có khuyết tật **biết trước
> mức độ** (vết xước cạn/sâu đã phân loại bằng mắt hoặc thiết bị đo độ nhám) — cùng nguyên tắc "đo
> từ golden set, không đoán ngưỡng" đã nhấn mạnh ở Chương 8 và Chương 9.

### 10.4.3 Một cách khác: tách bước trừ ảnh ra khỏi Blob

Code 10.3 dùng `SetSegmentationSubtractionImage` để trừ ảnh ngay bên trong `CogBlobTool` — nhanh
gọn, nhưng "giấu" ảnh hiệu số bên trong tool, khó xem trực tiếp lúc debug. Một cách tiếp cận khác,
tách bạch hơn: dùng `CogImageProcessingTool` (assembly `Cognex.VisionPro.ImageProcessing`) để tính
ảnh hiệu số/hiệu số tuyệt đối (`AbsDiff`) giữa ảnh kiểm và ảnh chuẩn thành một `ICogImage` riêng —
bước này chạy độc lập với Blob, trả về đúng một ảnh hiệu số có thể hiển thị trực tiếp lên HMI để
kỹ sư đứng máy xem "ảnh khác biệt" thay vì chỉ thấy kết quả đúng/sai. Đưa ảnh hiệu số đó làm
`InputImage` cho một `CogBlobTool` cấu hình segmentation bình thường (mục 10.1) là bước tiếp theo.
Đánh đổi: nhiều bước hơn (hai tool thay vì một), nhưng dễ debug hơn khi mới triển khai một trạm —
nhiều đội chọn cách này lúc phát triển, rồi gộp lại thành `SetSegmentationSubtractionImage` một
bước khi đã ổn định, để giảm số tool trong job.

## 10.5 Khi rule-based đuối: cầu nối sang deep learning

Blob analysis — như mọi kỹ thuật rule-based trong sách này — hoạt động tốt khi khuyết tật/đối
tượng cần nhận diện có thể mô tả bằng **một công thức hình học/cường độ rõ ràng**: "vùng tối hơn
ngưỡng X, diện tích trong khoảng Y". Ba dấu hiệu cho thấy bài toán đã vượt quá giới hạn đó:

- **Khuyết tật không có đặc trưng cường độ ổn định** — vết nứt tóc mảnh trên bề mặt có vân gỗ tự
  nhiên, biến dạng tinh tế mà mắt người "biết là lỗi" nhưng không chỉ ra được ngưỡng cụ thể nào.
- **Cần phân biệt NHIỀU LOẠI khuyết tật khác nhau**, mỗi loại một cách xử lý khác (phân loại, không
  chỉ nhị phân OK/NG).
- **Biến thể hình dạng/kết cấu quá lớn giữa các mẫu "tốt"** khiến việc định nghĩa một dải ngưỡng
  bao trùm hết các trường hợp tốt mà không lẫn trường hợp xấu trở nên bất khả thi bằng tay.

Khi gặp một trong ba dấu hiệu này, hướng đi hợp lý là chuyển sang **VisionPro Deep Learning
(ViDi)** — bốn công cụ locate/analyze/classify/read học từ dữ liệu thay vì công thức tường minh.
Chương 16, mục 16.5 bàn khái quát công nghệ này và — quan trọng không kém — cái giá thật của nó
(khối lượng dữ liệu cần gán nhãn, hạ tầng GPU, khả năng giải thích kết quả) để đưa ra quyết định
có cân nhắc, không phải "cứ khó là dùng AI".

## Tổng kết chương

- Blob = vùng pixel liền nhau thoả điều kiện phân loại; `CogBlobTool` biến ảnh xám thành tập blob
  qua ba bước: **segmentation** (ngưỡng hard/soft, cố định/tương đối/động), **connectivity**
  (kết nối các pixel thành blob, lấp lỗ nhỏ), **morphology** (Erode/Dilate/Open/Close sửa hình
  dạng trước khi đo). Ngưỡng động (Dynamic/Otsu) thích nghi tốt nhưng không có gì neo nó lại nếu
  đúng một ảnh bất thường — dùng Relative khi đủ, và giám sát xu hướng nếu bắt buộc dùng Dynamic.
- Đọc kết quả qua `GetTopLevelBlobs(filtered: true)` hoặc `GetBlobs(filtered: true)` — không có
  indexer trực tiếp trên `CogBlobResults`. Các thuộc tính `Area`/`Perimeter`/`CenterOfMassX,Y`/
  `Elongation`/`Angle` mô tả hình học từng blob; lọc có thể đặt ngay trong tool qua
  `RunTimeMeasures` hoặc bằng code sau khi đọc kết quả.
- Bài toán đếm: kiểm số lượng trước, kiểm hình dạng từng blob sau; đừng chỉ tin con số — hai đối
  tượng dính nhau có thể nguỵ trang thành đúng số lượng nhưng sai bản chất.
- Kiểm khuyết tật bề mặt bằng trừ ảnh chuẩn đòi hỏi ba điều kiện: cùng hệ toạ độ (fixture đúng —
  Chương 7), ánh sáng nhất quán (Chương 2), và ảnh chuẩn thật sự "sạch".
- Blob chỉ đo hình học, không phân loại bản chất đối tượng. Khi khuyết tật không có đặc trưng
  cường độ/hình học ổn định để viết thành ngưỡng, đó là tín hiệu chuyển sang deep learning
  (Chương 16, mục 16.5) — không phải khiếm khuyết của riêng CogBlobTool.

## Lỗi thường gặp

**Lỗi 1 — Threshold cố định "chết" theo drift ánh sáng.** Hiện tượng: job đếm đúng buổi sáng,
đếm sai buổi chiều dù không ai đổi gì trong cấu hình. Nguyên nhân: `SetSegmentationHardFixedThreshold`
với một số cố định không theo kịp thay đổi độ sáng nền thực tế (đèn xuống cấp, ánh sáng môi
trường — Chương 2). Cách tránh: cân nhắc `SetSegmentationHardRelativeThreshold`/`Dynamic` nếu độ
sáng nền có dao động đo được; giám sát chỉ số ánh sáng theo thời gian (Chương 12, mục 12.2).

**Lỗi 2 — Quên morphology khiến blob thật bị đứt thành nhiều mảnh.** Hiện tượng: đếm được nhiều
hơn số lượng thật (một đối tượng bị tách thành 2-3 blob do phản xạ/nhiễu cục bộ cắt ngang). Nguyên
nhân: segmentation nhạy với nhiễu cục bộ, không có bước "hàn lại" các mảnh vỡ thuộc cùng một đối
tượng. Cách tránh: thêm `CloseSquare` (hoặc biến thể phù hợp hướng nhiễu) trước khi đo — nhưng
kiểm ngưỡng đủ để không nối nhầm hai đối tượng khác nhau (xem Cảnh báo ở mục 10.1.2).

**Lỗi 3 — Đặt ngưỡng đếm quá lỏng ("≥ N" thay vì "= N chính xác").** Hiện tượng: lỗi thiếu linh
kiện lọt qua vì đâu đó vẫn còn "đủ hoặc thừa" blob dù bản chất sai (dính chồng, mảnh vỡ tính
nhầm). Nguyên nhân: kiểm số lượng không đi kèm kiểm hình dạng từng blob. Cách tránh: luôn kiểm cả
hai — số lượng blob hợp lệ VÀ diện tích/hình dạng từng blob nằm trong dải kỳ vọng (Code 10.2).

**Lỗi 4 — Trừ ảnh khi chi tiết chưa fixture đúng.** Hiện tượng: "khuyết tật giả" xuất hiện dọc
theo mọi biên thật của chi tiết, không tập trung ở vị trí lỗi thực. Nguyên nhân: lệch dù rất nhỏ
giữa ảnh chuẩn và ảnh kiểm tạo hiệu số lớn tại mọi cạnh có gradient cao. Cách tránh: đảm bảo chuỗi
Calibration → Fixture chạy trước Blob trong mọi trường hợp trừ ảnh (Chương 7).

**Lỗi 5 — Ảnh chuẩn không thật sự "sạch".** Hiện tượng: một loại khuyết tật cụ thể không bao giờ
bị phát hiện, dù mắt người thấy rõ trên nhiều chi tiết khác nhau. Nguyên nhân: khuyết tật đó vô
tình có mặt trên chính ảnh chuẩn, nên phép trừ luôn coi nó là "bình thường". Cách tránh: kiểm tra
kỹ ảnh chuẩn bằng nhiều phương pháp độc lập trước khi chốt làm golden image; cân nhắc dùng ảnh
chuẩn tổng hợp (trung bình nhiều chi tiết tốt) thay vì một chi tiết đơn lẻ.

**Lỗi 6 — Tin thẳng `SetSegmentationHardDynamicThreshold` mà không giám sát gì thêm.** Hiện tượng:
đa số cycle đếm đúng, nhưng thỉnh thoảng một cycle riêng lẻ đếm sai hẳn rồi tự khỏi ở cycle sau,
không lặp lại theo mẫu nào cả — khó tái hiện khi điều tra. Nguyên nhân: đúng ảnh đó có đặc điểm bất
thường (quá tối/sáng, thiếu vật thể, vệt sáng lạ) khiến Otsu tính ra một ngưỡng lệch hẳn, và vì
Dynamic tính lại hoàn toàn từ mỗi ảnh nên không có gì "kéo" nó về giá trị hợp lý — VisionPro cũng
không lộ ra ngưỡng vừa tính để kiểm tra riêng. Cách tránh: ưu tiên `SetSegmentationHardRelativeThreshold`
nếu đủ dùng (mục 10.1.1); nếu bắt buộc Dynamic, ghi lại số lượng/diện tích blob mọi cycle để giám
sát xu hướng thay vì chỉ nhìn NG tức thời (Chương 16, mục 16.4.1) — một ngưỡng đi hoang thường để
lại dấu vết ở đó dù không thấy trực tiếp.

\newpage

# Chương 11 — Đọc mã và ký tự: ID và OCRMax

Mỗi vỏ nhôm MeoVision rời khỏi trạm gia công CNC đều mang một mã DataMatrix khắc laser ở mặt
đáy — mã này là sợi dây duy nhất nối một sản phẩm cụ thể với lô nguyên liệu, thời điểm gia công,
và chương trình máy đã dùng để làm ra nó. Ba tháng sau khi xuất xưởng, nếu một khách hàng báo lỗi,
mã đó là thứ duy nhất cho phép truy ngược lại: lô nào, máy nào, ca nào — mà không cần thu hồi
toàn bộ lô hàng để kiểm tra. Đọc sai một mã, hoặc tệ hơn, đọc "được" nhưng ra sai ký tự, không
gây lỗi ngay lập tức — nó gây một lỗ hổng trong hồ sơ truy vết mà không ai biết đến cho tới khi
cần dùng đến nó.

Đây là lý do đọc mã (`CogIDTool`) và đọc ký tự in/khắc (`CogOCRMaxTool`) đứng thành một chương
riêng dù về mặt kỹ thuật ảnh, cả hai đều "chỉ" là các bài toán nhận dạng mẫu chuyên biệt. Điều
khác biệt nằm ở hậu quả của việc sai: PMAlign định vị sai một lần, robot gắp trượt, cycle sau
thường tự sửa; đọc mã/ký tự sai một lần có thể đóng dấu vĩnh viễn một dữ liệu sai vào hồ sơ sản
xuất. Chương này vì vậy nhấn mạnh không chỉ *cách đọc* mà còn *cách biết khi nào không nên tin
kết quả đọc được*.

## 11.1 CogIDTool: một tool, nhiều loại mã

### 11.1.1 Kiến trúc: bật/tắt từng symbology trên cùng một tool

Khác với PMAlign hay Caliper — mỗi loại bài toán một tool riêng — VisionPro gom toàn bộ việc đọc
mã vào **một tool duy nhất**, `CogIDTool`. Tham số chạy (`tool.RunParams`, kiểu `CogID`) chứa một
loạt đối tượng con, mỗi đối tượng ứng với một loại symbology — `DataMatrix`, `QRCode`, `Code128`,
`Code39`, `PDF417`, `UpcEan`, `DataBar`, và nhiều loại khác — mỗi đối tượng con có cờ `Enabled`
riêng. Bật đúng những symbology chi tiết thực sự có, tắt hết phần còn lại: tool chỉ tốn công tìm
kiếm những dạng mã đã bật, và tắt bớt symbology không cần thiết vừa nhanh hơn vừa giảm khả năng
đọc nhầm ký tự trên bề mặt thành một mã 1D ảo không tồn tại.

**Code 11.1 — Cấu hình CogIDTool chỉ đọc DataMatrix cho mã truy vết MeoVision.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.ID;

var idTool = new CogIDTool();
idTool.Region = codeSearchRegion;        // vẽ trong fixtured space — Chương 7, mục 7.4

CogID p = idTool.RunParams;
p.DataMatrix.Enabled              = true;
p.DataMatrix.IgnorePolarity        = true;   // laser khắc: không chắc nền sáng hay tối
p.DataMatrix.PerspectiveEnabled    = true;  // camera không hoàn toàn vuông góc vị trí khắc
p.QRCode.Enabled                    = false;  // tắt mọi symbology khác không thực sự dùng
p.Code128.Enabled                   = false;

p.NumToFind        = 1;      // đúng 1 mã mỗi cycle
p.TimeoutEnabled     = true;
p.Timeout             = 150;   // ms — trong ngân sách cycle (Chương 15)

idTool.InputImage = fixturedImage;
idTool.Run();
```

`IgnorePolarity = true` đáng chú ý riêng: mã khắc laser trên nhôm anodized có thể hiện ra sáng
trên nền tối hoặc tối trên nền sáng tuỳ góc chiếu sáng — bỏ giả định về chiều tương phản giúp
tool không bỏ lỡ mã hợp lệ chỉ vì phân cực ngược với kỳ vọng, đổi lại chấp nhận không gian tìm
kiếm rộng hơn đôi chút.

### 11.1.2 Direct Part Marking — khi mã không được in, mà được khắc

**DPM (Direct Part Marking)** là **cách đánh dấu** — khắc trực tiếp lên vật liệu (laser, đột dập,
khắc kim cương) — chứ không phải một loại mã cụ thể: nội dung khắc có thể là DataMatrix (như ở
MeoVision), QR, mã 1D, hay đơn thuần chuỗi ký tự đọc bằng OCR (mục 11.3). Phần này dùng DataMatrix
khắc laser làm ví dụ minh hoạ chính vì đó là trường hợp phổ biến nhất, nhưng nguyên tắc chiếu sáng
dưới đây áp dụng cho DPM nói chung, không riêng DataMatrix. DPM khác về bản chất so với mã in trên
nhãn giấy: độ tương phản thấp hơn nhiều (khắc laser
trên nhôm chỉ đổi độ nhám bề mặt, không đổi màu như mực in), và bề mặt xung quanh mã thường có
phản xạ mạnh, không đồng đều. Đây chính là lý do Chương 2 dành hẳn một mục cho các kỹ thuật
chiếu sáng chuyên biệt: DPM gần như luôn cần **dark field** hoặc **chiếu sáng góc thấp** để biến
sự khác biệt độ nhám cực nhỏ thành tương phản đủ để camera nhìn thấy — chiếu sáng đồng đều tiêu
chuẩn (dome, ring thẳng) thường khiến mã DPM gần như "biến mất" vào nền kim loại xung quanh.

![Hình 11.1 — Cùng một mã DataMatrix khắc laser dưới ánh sáng đồng đều và dưới dark field](../assets/ch11/hinh_11_1.png)
**Hình 11.1 — Cùng một mã DataMatrix khắc laser dưới ánh sáng đồng đều (trái) và dưới dark field/góc thấp (phải).**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): 2 ảnh chụp cùng vị trí khắc laser trên vỏ nhôm MeoVision, đặt
> cạnh nhau. Trái: chiếu ring sáng thẳng — mã gần như biến mất vào nền, module mờ, khó phân biệt
> ranh giới. Phải: chiếu góc thấp/dark field — các module hiện rõ nhờ tương phản từ đổ bóng vi mô
> trên bề mặt nhám do laser tạo ra. Overlay khung vuông đỏ đánh dấu vùng mã ở cả hai ảnh; chú
> thích dưới mỗi ảnh nêu rõ kiểu chiếu sáng dùng. Chụp thật trên trạm hoặc mẫu khắc laser bất kỳ
> có sẵn — đây là hình minh hoạ nguyên lý DPM quan trọng nhất của chương.

"Góc thấp" không phải một con số duy nhất — đây là kinh nghiệm triển khai thực tế trong ngành đọc
mã công nghiệp, không phải một chuẩn quang học bắt buộc; thực hành phổ biến gói gọn quanh bốn
phương án góc chiếu chính, mỗi phương án phù hợp một dạng bề mặt/kiểu khắc khác nhau:
**30°** (gần sát mặt phẳng — tương phản mạnh nhất cho bề mặt phẳng nhẵn, dễ bị che khuất một phần
bởi cấu trúc nổi trên bề mặt cong), **45°** (cân bằng phổ biến nhất, điểm khởi đầu hợp lý khi chưa
biết rõ đặc tính bề mặt), **90°** (gần vuông góc — cho bề mặt cong hoặc có kết cấu phức tạp nơi góc
thấp bị chính hình dạng bề mặt che khuất một phần mã), và **chiếu khuếch tán/dome** (khi bề mặt quá
gồ ghề hoặc phản chiếu để bất kỳ góc chiếu định hướng nào cũng tạo loé sáng cục bộ). Không có một
góc "đúng tuyệt đối" cho mọi bề mặt — với DPM khó, thử nghiệm cả bốn phương án trên chính mẫu vật
liệu và kiểu khắc thật (đúng nguyên tắc "thử trước khi mua đèn công nghiệp" ở Chương 2, mục 2.2) là
cách xác định nhanh nhất, đáng tin hơn suy luận lý thuyết.

> ⚠️ **Cảnh báo:** đừng để mãi đến khi lắp trạm rồi mới phát hiện mã DPM không đọc được ổn định —
> khả năng đọc DPM phụ thuộc trực tiếp vào **cách khắc** (công suất laser, tốc độ, góc khắc) —
> thứ thuộc về công đoạn gia công trước trạm vision, không phải thứ trạm vision sửa được bằng
> tham số phần mềm. Phối hợp với công đoạn khắc để tối ưu chất lượng mã là một phần của việc
> triển khai trạm đọc DPM, không phải việc "để sau tính".

## 11.2 Đánh giá chất lượng mã (grading)

Đọc được một mã (`Status` của kết quả báo thành công) không đồng nghĩa với việc mã đó **đủ chất
lượng để đọc được ổn định về lâu dài**. Một mã DataMatrix có thể đọc thành công hôm nay với vài
module bị mòn/mờ, nhưng mòn thêm một chút nữa — do cọ xát trong quá trình vận chuyển, đóng gói —
là đọc thất bại vào lần sau. **Grading** trả lời câu hỏi "mã này còn bao nhiêu biên độ an toàn",
không chỉ "mã này đọc được không".

Ngành công nghiệp chuẩn hoá việc này qua các chỉ số như ISO/IEC 15415 (mã 2D) và ISO/IEC 15416
(mã 1D) — chấm điểm A đến F dựa trên các thành phần con: độ tương phản, độ đồng đều mô-đun, tỉ lệ
lỗi sửa được, độ méo hình học. Lưu ý phân biệt hai khái niệm hay bị dùng lẫn: **đọc mã** (decode —
tool `CogIDTool` cơ bản trả lời "có giải mã được không") khác với **grading/verification** (chấm
điểm biên độ an toàn theo chuẩn ISO ở trên) — một tool đọc mã cơ bản hoàn toàn có thể decode thành
công một mã mà nếu chấm điểm đầy đủ sẽ chỉ đạt hạng D hoặc F, sắp hỏng. Việc chấm điểm đầy đủ theo
các chuẩn này thường thuộc một **gói
verification/grading riêng** của bộ công cụ đọc mã (có thể yêu cầu license bổ sung so với gói
VisionPro cơ bản) — kiểm tra trong tài liệu cấp phép của bản cài đặt cụ thể xem tính năng này có
sẵn hay không trước khi thiết kế quy trình dựa vào nó.

> 🔍 **Đào sâu thêm (đã kiểm chứng lại trên SDK thật, VisionPro 9.0 CR2):** quét toàn bộ 289 type
> của assembly `Cognex.VisionPro.ID.dll` không thấy type nào tên chứa "Grad"/"Verif"/"Quality"/
> "ISO"/"IEC"/"15415"/"15416". Vào tận `CogIDResult` (kết quả từng symbol đọc được) — properties
> thật chỉ có `DecodedData`, `PixelPerModule`, `BoundsPolygon`, `CenterX`, `CenterY`, `ID`, `Angle`
> — không có property chấm điểm nào tương ứng chuẩn ISO/IEC. Xác nhận lại: gói cài đặt hiện tại
> KHÔNG có tính năng grading/verification chuẩn hoá qua API công khai, đúng như nghi vấn ban đầu —
> tính năng này (nếu tồn tại) nằm trong SKU/license khác. -->

Ngay cả không có công cụ chấm điểm chuẩn hoá đầy đủ, một chỉ số thực dụng luôn tính được: theo dõi
**tỉ lệ đọc thành công theo thời gian** trên cùng một trạm (Chương 16 bàn kỹ việc giám sát này).
Tỉ lệ giảm dần — dù vẫn trên 99% — là tín hiệu sớm về chất lượng khắc đang xuống cấp, đáng điều
tra trước khi nó rơi xuống mức gây dừng dây chuyền.

## 11.3 CogOCRMaxTool: đọc ký tự in/khắc

### 11.3.1 Ba thành phần: Segmenter, Classifier, Fielding

Đọc chuỗi ký tự — số lô, ngày sản xuất in dạng chữ, không phải mã hình học — là bài toán khác hẳn
đọc mã: không có cấu trúc hình học cố định (grid module của DataMatrix) để bám vào, phải tự
**tách từng ký tự ra khỏi chuỗi** trước khi nhận dạng từng ký tự đó là gì. `CogOCRMaxTool` chia
việc này thành ba thành phần rõ ràng:

**Bảng 11.1 — Ba thành phần chính của CogOCRMaxTool.**

| Thành phần | Vai trò |
|---|---|
| `Segmenter` (`CogOCRMaxSegmenter`) | Tách chuỗi ký tự trên ảnh thành từng ô ký tự riêng biệt — dựa trên khoảng cách, chiều cao, độ rộng nét (nhiều tham số: `CharacterMinHeight/Width`, `MinIntercharacterGap`, `Polarity`...) |
| `Classifier` (`CogOCRMaxClassifier`) | Nhận dạng từng ô ký tự đã tách thành một ký tự cụ thể — dựa trên `Font` đã **train** |
| `Fielding` (`CogOCRMaxFielding`) | Ràng buộc định dạng chuỗi kết quả — độ dài, ký tự cho phép ở từng vị trí — để loại bỏ kết quả "đọc được nhưng vô nghĩa" |

### 11.3.2 Train font: dạy tool nhận diện đúng bộ ký tự thực tế

`Classifier.Font` là một tập hợp các `CogOCRMaxChar` — mỗi phần tử là một mẫu ảnh của **một ký
tự cụ thể** kèm mã ký tự tương ứng (`CharacterCode`). Train không phải "chọn font hệ thống có
sẵn" như trên máy tính văn phòng — nó là quá trình dạy tool nhận diện **chính xác hình dạng ký
tự thật xuất hiện trên chi tiết**, vốn khác đáng kể so với font chữ máy tính chuẩn khi khắc bằng
laser (nét có thể đứt quãng, có viền cháy) hay đột dập (nét có ba-via, độ sâu không đều).

**Code 11.2 — Train một font OCRMax từ các mẫu ký tự cắt sẵn.**

```csharp
using Cognex.VisionPro.OCRMax;

var classifier = new CogOCRMaxClassifier();

// Mỗi ký tự cần vài mẫu ảnh đại diện — không chỉ 1 mẫu "đẹp nhất"
foreach (var (charImage, code) in trainingSamples)   // mẫu ('0'..'9', 'A'..'Z') đã cắt sẵn
{
    var sample = new CogOCRMaxChar();
    sample.Image          = charImage;    // CogImage8Grey đã cắt sát 1 ký tự
    sample.CharacterCode   = code;
    classifier.Font.Add(sample);
}

classifier.Train();

if (!classifier.Trained)
    throw new InvalidOperationException("Train font OCRMax thất bại — kiểm lại mẫu ký tự");
```

> 💡 **Mẹo thực chiến:** train với **nhiều biến thể** của cùng một ký tự — khác độ sâu khắc, khác
> góc ánh sáng chụp, khác vị trí trên chi tiết nếu có thể — thay vì một mẫu "đẹp nhất" duy nhất
> cho mỗi ký tự. Giống nguyên tắc train pattern ở Chương 8: một mẫu quá hoàn hảo dạy tool nhận
> diện đúng *một* thể hiện cụ thể, không phải *lớp* ký tự đó nói chung.

### 11.3.3 Fielding: chặn kết quả "đọc được nhưng sai"

Một chuỗi lô sản xuất của MeoVision luôn có định dạng cố định: 2 chữ cái + 6 chữ số (ví dụ
`AB240615`). `Fielding` cho phép khai báo ràng buộc này tường minh — `FieldString` mô tả khuôn
dạng mong đợi bằng các **ký tự alias**, mỗi alias tham chiếu đến một `CogOCRMaxFieldingDefinition`
đã định nghĩa (tập ký tự hợp lệ cho vị trí đó) trong `Fielding.FieldingDefinitions` — **không phải
cú pháp cố định kiểu regex**. VisionPro có sẵn alias `N` nghĩa là "bất kỳ chữ số nào"; alias cho
"bất kỳ chữ cái nào" không có sẵn mặc định, phải tự định nghĩa trước khi dùng trong `FieldString`.
`LengthMin`/`LengthMax` giới hạn độ dài tổng thể. Một kết quả đọc ra 7 hoặc 9 ký tự, hoặc có chữ số
ở vị trí lẽ ra phải là chữ cái, bị `Fielding` loại ngay — dù từng ký tự riêng lẻ classifier vẫn
"tự tin" nhận diện đúng.

```csharp
using Cognex.VisionPro.OCRMax;

// Alias 'A' (chữ cái) chưa có sẵn — tự định nghĩa trước khi FieldString tham chiếu đến nó.
// Alias 'N' (chữ số) đã có sẵn mặc định trong VisionPro, không cần định nghĩa lại.
var letterAlias = new CogOCRMaxFieldingDefinition(
    enabled: true, alias: 'A', characterSet: "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
    wildcard: false, editable: true);
tool.Fielding.FieldingDefinitions.Add(letterAlias);

tool.FieldingEnabled     = true;
tool.Fielding.FieldString = "AANNNNNN";   // A = chữ cái (alias tự định nghĩa), N = chữ số (có sẵn)
tool.Fielding.LengthMin   = 8;
tool.Fielding.LengthMax   = 8;
```

Đây chính là lớp bảo vệ quan trọng nhất chống lại lỗi "đọc được nhưng sai" đã nói ở đầu chương:
nhận dạng ký tự đơn lẻ luôn có một xác suất sai khác không — chữ `O` và số `0`, chữ `I` và số `1`
là các cặp nhầm lẫn kinh điển. Fielding không loại bỏ hoàn toàn khả năng nhầm (một `O` nhầm thành
`0` vẫn có thể lọt qua nếu vị trí đó hợp lệ cho cả hai), nhưng nó chặn đứng phần lớn các trường
hợp nhầm dẫn đến kết quả sai *định dạng* — vốn chiếm đa số lỗi thực tế.

## 11.4 Kịch bản traceability: đọc mã → gửi MES/host

Đọc mã tự nó không tạo ra giá trị — giá trị nằm ở việc gắn kết quả đọc với **một sự kiện sản
xuất cụ thể** và lưu trữ mối liên kết đó theo cách sau này truy vấn lại được. Chuỗi tối thiểu cho
một trạm truy vết:

1. Đọc mã (`CogIDTool`) → nhận `DecodedData.DecodedString` (Mã DataMatrix của MeoVision).
2. Gắn kèm **ngữ cảnh cycle hiện tại**: timestamp, kết quả kiểm tra (Chương 8–10), vị trí trạm.
3. Gửi bản ghi hoàn chỉnh đó lên hệ thống cấp trên — thường gọi chung là **MES** (Manufacturing
   Execution System) hoặc **host** trong tài liệu ngành — qua giao thức đã thống nhất (TCP/IP,
   OPC UA, hoặc SECS/GEM cho ngành bán dẫn/điện tử — Chương 14 bàn chi tiết các lựa chọn giao thức).
4. **Không tiếp tục cycle cho đến khi có xác nhận** bản ghi đã được nhận (hoặc lưu cục bộ chờ gửi
   lại nếu host tạm thời không phản hồi) — mất một bản ghi traceability là mất vĩnh viễn, không
   như một lần đo lường có thể chụp lại ảnh và đo lại.

```csharp
if (idTool.Results.Count == 0 || idTool.Results[0].DecodedData == null)
{
    // Không đọc được mã: đây là NG nghiêm trọng hơn NG đo lường thông thường —
    // chi tiết mất khả năng truy vết nếu vẫn cho qua trạm
    return StationResult.CodeReadFailed;
}

string traceCode = idTool.Results[0].DecodedData.DecodedString;
var record = new TraceabilityRecord(
    code: traceCode,
    timestamp: DateTime.UtcNow,
    station: "MeoVision-01",
    inspectionResult: currentCycleResult);

await _traceabilityClient.SendAsync(record, ct).ConfigureAwait(false);   // xem Chương 14
```

> 📌 **Lưu ý:** quyết định "chi tiết không đọc được mã thì làm gì" (dừng máy, đẩy sang trạm reject,
> hay cho qua kèm cờ cảnh báo) là quyết định **quy trình sản xuất**, không phải quyết định kỹ
> thuật vision — mức độ nghiêm trọng của việc mất truy vết khác nhau tuỳ ngành (dược phẩm/y tế
> thường không được phép cho qua; hàng tiêu dùng có thể chấp nhận với quy trình bù trừ khác).
> Vision chỉ báo cáo trạng thái, quyết định thuộc về PLC/MES theo đúng nguyên tắc đã nhấn ở
> Chương 7, mục 7.5.

## 11.5 Ký tự khắc laser/đột dập khó

Một số bề mặt đẩy bài toán OCR/đọc mã đến giới hạn của kỹ thuật chiếu sáng thông thường: kim loại
đã qua xử lý bề mặt phản xạ mạnh (đánh bóng, mạ), ký tự đột dập sâu có bóng đổ lớn thay đổi theo
góc nhìn, hoặc bề mặt cong khiến một hướng chiếu sáng duy nhất không đủ chiếu đều toàn bộ ký tự.

Ba hướng kỹ thuật khi gặp trường hợp khó:

- **Chiếu sáng đa hướng có kiểm soát** — nhiều đèn LED bố trí xung quanh, chụp nhiều ảnh với các
  tổ hợp đèn bật/tắt khác nhau, chọn hoặc tổng hợp ảnh có tương phản tốt nhất cho từng vùng.
- **Domed/diffuse dome light** (đã giới thiệu ở Chương 2) cho bề mặt cong phản xạ mạnh — ánh sáng
  đến từ mọi hướng loại bỏ phần lớn phản xạ gương cục bộ.
- **Chụp nhiều góc, hợp nhất kết quả** — với ký tự đột dập rất sâu, đôi khi không có góc chiếu
  sáng đơn nào làm rõ toàn bộ ký tự; chụp 2-3 ảnh ở các góc/hướng sáng khác nhau và hợp nhất kết
  quả đọc (lấy kết quả có Fielding hợp lệ, hoặc kết quả nhất quán giữa nhiều lần chụp) là giải
  pháp thực dụng hơn cố tìm "một ảnh hoàn hảo".

Nguyên tắc chung không đổi so với Chương 2: đầu tư thời gian vào chiếu sáng trước khi đổ lỗi cho
tham số Segmenter/Classifier — phần lớn trường hợp "OCR không đọc được ký tự khó" thực chất là
vấn đề ảnh đầu vào không đủ tương phản, không phải giới hạn của thuật toán nhận dạng.

## Tổng kết chương

- `CogIDTool` là một tool duy nhất phủ mọi symbology (DataMatrix, QRCode, các mã 1D) qua các đối
  tượng con có cờ `Enabled` riêng — chỉ bật đúng loại mã thực sự có trên chi tiết.
- DPM (khắc trực tiếp) có độ tương phản tự nhiên thấp, gần như luôn cần chiếu sáng chuyên biệt
  (dark field, góc thấp — Chương 2); đây là vấn đề vật lý của bề mặt, không sửa được bằng tham
  số phần mềm.
- Đọc được mã không đồng nghĩa mã đủ chất lượng để đọc ổn định lâu dài — grading (ISO/IEC 15415/
  15416) đo biên độ an toàn đó; theo dõi tỉ lệ đọc thành công theo thời gian là chỉ số thực dụng
  luôn tính được kể cả khi không có công cụ chấm điểm chuẩn hoá đầy đủ.
- `CogOCRMaxTool` chia việc đọc ký tự thành Segmenter (tách ký tự) → Classifier (nhận dạng, cần
  train bằng mẫu ký tự thật của chi tiết, không phải font hệ thống) → Fielding (ràng buộc định
  dạng chuỗi, chặn phần lớn lỗi "đọc được nhưng sai định dạng").
- Đọc mã chỉ có giá trị khi gắn với ngữ cảnh cycle và gửi lên MES/host có xác nhận nhận thành
  công — mất một bản ghi traceability là mất vĩnh viễn, khác với lỗi đo lường có thể khắc phục ở
  cycle sau.
- Quyết định xử lý khi không đọc được mã là quyết định quy trình sản xuất, không phải quyết định
  kỹ thuật vision — vision báo cáo trạng thái, PLC/MES quyết định hành động tiếp theo.

## Lỗi thường gặp

**Lỗi 1 — Bật tất cả symbology "cho chắc".** Hiện tượng: job chậm hơn cần thiết, thỉnh thoảng
đọc nhầm một cụm nét trên bề mặt thành một mã 1D không tồn tại. Nguyên nhân: mỗi symbology bật
thêm là một không gian tìm kiếm thêm, vừa tốn thời gian vừa tăng khả năng dương tính giả. Cách
tránh: chỉ bật đúng symbology chi tiết thực sự mang (Code 11.1).

**Lỗi 2 — Dùng chiếu sáng tiêu chuẩn cho mã DPM.** Hiện tượng: đọc được trong phòng lab với đèn
thử nghiệm, không đọc được ổn định trên dây chuyền thật. Nguyên nhân: DPM cần tương phản từ chênh
lệch độ nhám bề mặt — ánh sáng đồng đều không tạo ra tương phản đó. Cách tránh: dark field/chiếu
góc thấp cho DPM (Chương 2, mục 2.2.5); kiểm chất lượng khắc phối hợp với công đoạn gia công.

**Lỗi 3 — Train font OCR bằng font hệ thống thay vì mẫu ký tự thật.** Hiện tượng: classifier
nhận diện tốt trên ảnh test tự tạo bằng công cụ vẽ chữ, kém trên chi tiết thật. Nguyên nhân: hình
dạng ký tự khắc/đột dập thực tế khác đáng kể font chữ máy tính chuẩn. Cách tránh: luôn train bằng
ảnh chụp ký tự thật trên chi tiết, nhiều biến thể (Code 11.2, mục 11.3.2).

**Lỗi 4 — Không bật Fielding dù định dạng chuỗi đã biết trước.** Hiện tượng: một tỉ lệ nhỏ bản
ghi traceability có mã sai một ký tự (thường là cặp dễ nhầm như O/0, I/1) lọt qua mà không có
cảnh báo. Nguyên nhân: không có ràng buộc định dạng để loại kết quả sai cấu trúc. Cách tránh: bật
`FieldingEnabled` với `FieldString`/độ dài đúng theo định dạng thực tế (mục 11.3.3).

**Lỗi 5 — Không có cơ chế xác nhận khi gửi bản ghi traceability lên MES.** Hiện tượng: một khoảng
lô hàng "biến mất" khỏi hồ sơ truy vết dù trạm vẫn báo chạy bình thường — thường phát hiện rất
muộn, khi cần tra cứu mới biết thiếu. Nguyên nhân: gửi bản ghi theo kiểu "bắn rồi quên" (fire-and-
forget), không kiểm tra phản hồi từ host. Cách tránh: xác nhận nhận thành công hoặc lưu cục bộ
chờ gửi lại; xem thiết kế giao tiếp bền vững ở Chương 14.

\newpage

# Chương 12 — Công cụ hỗ trợ và kịch bản trong job

Job của trạm MeoVision, sau 11 chương, đã có acquisition, calibration, fixture, PMAlign, caliper,
blob, và đọc mã — nhưng vẫn còn ba việc chưa tool nào đảm nhận: chuyển ảnh màu camera color sang
ảnh xám cho các tool cần nó, gộp kết quả của cả chục tool riêng lẻ thành một quyết định OK/NG
duy nhất, và tự phát hiện khi ánh sáng trạm đang trôi dần trước khi nó gây lỗi thật. Đây là những
"việc vặt" nhưng thiếu chúng, job không thể chạy được trong sản xuất thực tế — chương này gom lại
nhóm công cụ hỗ trợ đó, rồi kết thúc bằng chủ đề khiến nhiều kỹ sư mới dùng VisionPro lúng túng
nhất: viết và **debug** script C# ngay bên trong job.

## 12.1 Nhóm ImageProcessing: chuyển đổi, số học ảnh, độ nét

### 12.1.1 CogImageConvertTool — chuyển màu, tách kênh

`CogImageConvertTool` biến ảnh đầu vào (có thể là ảnh màu từ camera Bayer hoặc RGB) thành dạng
ảnh mà các tool khác trong sách này (PMAlign, Caliper, Blob — tất cả làm việc trên ảnh xám) cần.
Tham số `RunMode` (kiểu `CogImageConvertRunModeConstants`) quyết định phép chuyển đổi cụ thể:

**Bảng 12.1 — Các chế độ chuyển đổi chính của CogImageConvertTool.**

| `RunMode` | Kết quả |
|---|---|
| `Intensity` | Ảnh màu → ảnh xám (độ sáng tổng hợp từ 3 kênh màu) |
| `IntensityFromWeightedRGB` | Ảnh xám, trọng số từng kênh R/G/B tự chọn (`IntensityFromWeightedRGBRedWeight`...) — hữu ích khi một màu cụ thể mang nhiều thông tin hơn màu khác |
| `Plane0`, `Plane1`, `Plane2` | Tách lấy đúng một kênh màu (R, G, hoặc B tuỳ định dạng ảnh gốc) |
| `RGBFromBayer`, `IntensityFromBayer` | Giải mã ảnh thô từ cảm biến Bayer (Chương 3, mục 3.1) thành ảnh màu hoặc ảnh xám trực tiếp |
| `HSI`, `HSIFromBayer` | Chuyển sang không gian màu Hue-Saturation-Intensity — hữu ích khi phân loại theo màu sắc quan trọng hơn độ sáng (mục 12.6) |

```csharp
using Cognex.VisionPro.ImageProcessing;

var convert = new CogImageConvertTool();
convert.RunParams.RunMode = CogImageConvertRunModeConstants.Intensity;
convert.InputImage = colorImageFromCamera;
convert.Run();
ICogImage grayImage = convert.OutputImage;
```

> 📌 **Lưu ý:** nếu camera của trạm vốn đã là **mono** (khuyến nghị mặc định cho vision công
> nghiệp — Chương 3, mục 3.1), phần lớn job không cần `CogImageConvertTool` ở khâu chính; tool
> này giá trị nhất khi cần *tách* một kênh màu cụ thể để tăng tương phản cho một đặc trưng có
> màu riêng biệt (Chương 2, mục 2.3 — filter màu), hoặc khi phải làm việc với ảnh màu vì lý do
> khác (kiểm màu — mục 12.6).

### 12.1.2 Số học ảnh: cộng/trừ/so sánh hai ảnh

Nhóm `CogIPTwoImage*Tool` (`Add`, `Subtract`, `MinMax`) thực hiện phép toán pixel-đối-pixel giữa
hai ảnh cùng kích thước. `CogIPTwoImageSubtractTool` — nhận `InputImageA`, `InputImageB`, trả
`OutputImage` là hiệu số — chính là công cụ đứng sau kỹ thuật trừ ảnh chuẩn đã dùng ở Chương 10,
mục 10.4 (ở đó, phép trừ được cấu hình ngay bên trong `CogBlobSegmentationParams` qua
`SetSegmentationSubtractionImage` — dùng tool `CogIPTwoImageSubtractTool` độc lập khi cần **xem**
hoặc **xử lý tiếp** bản thân ảnh hiệu số, thay vì chỉ dùng nó làm input phân vùng cho Blob).

### 12.1.3 CogImageSharpnessTool — kiểm tra độ nét (autofocus check)

Một câu hỏi hay bị bỏ qua cho đến khi gây lỗi thật: **camera có đang lấy nét đúng không?** Ống
kính lệch tiêu cự do rung động, do ai đó vô tình chạm vào vòng lấy nét khi bảo trì, tạo ra ảnh mờ
đều — không đủ rõ ràng để một người vận hành phát hiện ngay trên màn hình nhỏ, nhưng đủ để làm
score PMAlign và độ chính xác caliper xuống cấp âm thầm. `CogImageSharpnessTool` đo một con số
duy nhất (`tool.Score`) phản ánh độ nét của ảnh, dựa trên một trong bốn thuật toán
(`CogImageSharpnessModeConstants`: `AutoCorrelation`, `AbsDiff`, `BandPass`, `GradientEnergy`) —
mỗi thuật toán nhạy với một dạng đặc trưng tần số khác nhau trong ảnh.

```csharp
using Cognex.VisionPro.ImageProcessing;

var sharpness = new CogImageSharpnessTool();
sharpness.RunParams.Mode = CogImageSharpnessModeConstants.GradientEnergy;
sharpness.InputImage = rawImage;
sharpness.Run();

if (sharpness.Score < minAcceptableSharpness)
    logger.LogWarning("Ảnh có dấu hiệu mất nét: Score={Score:F2}", sharpness.Score);
```

Đặt tool này chạy trên **mỗi ảnh** (hoặc định kỳ, tuỳ ngân sách cycle) và cảnh báo sớm khi độ nét
tụt là một dạng "sensor sức khoẻ" của chính hệ vision — chủ đề được mở rộng đầy đủ ở mục 12.2 tiếp
theo.

## 12.2 Histogram và giám sát ánh sáng tự động

### 12.2.1 CogHistogramTool: từ phân bố mức xám đến con số chẩn đoán

Chương 4 (mục 4.1) đã giới thiệu histogram như công cụ *chẩn đoán bằng mắt* khi tinh chỉnh ánh
sáng. `CogHistogramTool` biến việc đọc-bằng-mắt đó thành các con số đo được tự động mỗi cycle —
`tool.Result` (kiểu `CogHistogramResult`) trả về `Mean`, `StandardDeviation`, `Minimum`, `Maximum`,
`Median` của phân bố mức xám trong vùng đo.

**Code 12.1 — Giám sát ánh sáng: cảnh báo khi Mean/StandardDeviation trôi khỏi dải chuẩn.**

```csharp
using Cognex.VisionPro.ImageProcessing;

var hist = new CogHistogramTool();
hist.Region = referenceBackgroundRegion;  // vùng nền ổn định, KHÔNG phải vùng chứa chi tiết
hist.InputImage = rawImage;
hist.Run();

CogHistogramResult r = hist.Result;

// Dải [meanLow, meanHigh] đo được từ trạm lúc mới hiệu chỉnh ánh sáng (golden set —
// cùng nguyên tắc "đo, không đoán" đã nhấn ở Chương 8/9)
bool lightingOk = r.Mean >= meanLow && r.Mean <= meanHigh
                   && r.StandardDeviation <= maxStdDev;

if (!lightingOk)
{
    logger.LogWarning(
        "Ánh sáng lệch chuẩn: Mean={Mean:F1} (kỳ vọng {Low}-{High}), StdDev={Std:F1}",
        r.Mean, meanLow, meanHigh, r.StandardDeviation);
    // await chỉ hợp lệ khi đoạn này nằm trong một phương thức async Task — ví dụ minh hoạ giả
    // định phương thức bao ngoài đã khai báo async Task xxx(..., CancellationToken ct)
    await _alarmService.RaiseAsync(AlarmCodes.LightingDrift, "MeoVision-01", ct)
        .ConfigureAwait(false);
}
```

Điểm quan trọng: `Region` của histogram giám sát nên là một **vùng nền ổn định** (không có chi
tiết, không đổi giữa các cycle) — đo histogram trên vùng có chi tiết trộn lẫn tín hiệu "ánh sáng
thay đổi" với "chi tiết khác nhau giữa các lần", làm mất tác dụng chẩn đoán.

### 12.2.2 Đưa cảm biến ánh sáng vào job như một "sensor sức khoẻ"

Ba tín hiệu — `CogHistogramTool.Result.Mean/StandardDeviation` (mục này) và
`CogImageSharpnessTool.Score` (mục 12.1.3) — hợp lại tạo thành một bộ giám sát sức khoẻ quang học
liên tục, chạy song song với logic kiểm tra chính của job, không phải để quyết định OK/NG của
từng chi tiết mà để **cảnh báo sớm** khi điều kiện chụp ảnh đang xuống cấp — trước khi nó đủ tệ
để gây false reject hàng loạt. Chương 16 xây dựng tiếp ý tưởng này thành quy trình giám sát đầy
đủ theo thời gian, gắn với xu hướng score của các tool định vị/đo lường khác.

## 12.3 CogResultsAnalysisTool: gộp nhiều kết quả thành một quyết định

### 12.3.1 Bài toán: một chi tiết, nhiều tool, một quyết định

Một cycle kiểm tra MeoVision hoàn chỉnh chạy qua PMAlign (định vị), 2 tool Caliper (đo hai chiều),
CogBlobTool (đếm đệm cao su), CogIDTool (đọc mã) — năm tool, năm kết quả riêng biệt. Câu hỏi cuối
cùng gửi cho PLC chỉ có một: **chi tiết này OK hay NG?** `CogResultsAnalysisTool` là nơi logic
tổng hợp đó sống — thay vì rải rác các câu lệnh `if` nối tiếp trong code C# (dễ sai sót, khó nhìn
thấy toàn cảnh), nó biểu diễn logic tổng hợp như một **cây biểu thức**: các đầu vào (kết quả từ
tool khác, nối qua terminal — đúng khái niệm terminal đã dùng ở CogToolBlock, mục 12.4) được kết
hợp bằng toán tử logic (AND/OR/NOT) và toán tử so sánh (>, <, =...) thành một quyết định cuối.

### 12.3.2 Dựng cây biểu thức: QuickBuild là nơi làm việc chính

Cây biểu thức của `CogResultsAnalysisTool` — dựng bằng cách kéo-thả terminal và toán tử trong
QuickBuild, nối chúng thành sơ đồ logic trực quan — là cách làm việc thực tế phổ biến nhất với
tool này; SDK cũng cho phép dựng cây bằng code (các lớp `CogResultsAnalysisInputExpression`,
`CogResultsAnalysisLogicalBinaryExpression`, `CogResultsAnalysisRelationalBinaryExpression`...),
nhưng viết tay một cây biểu thức lồng nhau bằng code C# rườm rà hơn nhiều so với dựng trực quan,
và hiếm khi là lựa chọn thực tế trừ khi cần **sinh động** cấu hình kiểm tra theo tham số recipe.

> 🔍 **Đào sâu thêm (đã kiểm chứng lại trên SDK thật, VisionPro 9.0 CR2):** lý do sách không đưa
> ví dụ code dựng cây biểu thức, xác nhận bằng reflection sâu hơn — `CogResultsAnalysisLogicalOps`/
> `RelationalOps` chỉ có đúng một constructor không tham số (`ctor()`), không kế thừa/khai báo
> thêm property hay method public nào ngoài `ToString`/`Equals` của `object`. Phương thức thực sự
> tạo ra các toán tử con (ví dụ "AND", "lớn hơn") là `getOps()` — **private**, không gọi được từ
> code ứng dụng. Nói cách khác: constructor `ctor()` tồn tại nhưng không có đường công khai nào
> để lấy một instance toán tử cụ thể từ đó — đúng như nghi vấn ban đầu, mẫu code suy diễn sẽ không
> chạy được. Tham khảo trực tiếp sample chính thức trong `sdk/samples_programming` (thư mục liên
> quan ResultsAnalysis/Verification) nếu cần dựng cây bằng code.

### 12.3.3 Đọc kết quả: bốn mức, không chỉ hai

Điều dễ bị bỏ sót: quyết định trả về không chỉ nhị phân OK/NG. `Result.Decision` có kiểu
`CogToolResultConstants` với **bốn** giá trị: `Accept`, `Warning`, `Reject`, `Error`. Phân biệt
`Warning` khỏi `Accept`/`Reject` cho phép biểu diễn "chấp nhận được nhưng đáng chú ý" — ví dụ
score PMAlign thấp gần ngưỡng, hoặc đo lường sát biên dung sai — một trạng thái trung gian có giá
trị thực dụng lớn cho giám sát xu hướng (Chương 16) mà một quyết định OK/NG thuần nhị phân không
biểu diễn được.

```csharp
using Cognex.VisionPro.ResultsAnalysis;

resultsAnalysis.Run();
CogToolResultConstants decision = resultsAnalysis.Result.Decision;

switch (decision)
{
    case CogToolResultConstants.Accept:  return StationResult.Ok;
    case CogToolResultConstants.Warning: return StationResult.OkWithWarning;
    case CogToolResultConstants.Reject:  return StationResult.Ng;
    default:                              return StationResult.InspectionError;
}
```

## 12.4 Script trong CogToolBlock

### 12.4.1 Khi nào cần script — và khi nào không

`CogToolBlock` đóng gói một chuỗi tool thành một khối có `Inputs`/`Outputs` (hợp đồng terminal
đã nhắc ở Chương 5, mục 5.4 — sẽ dùng trực tiếp từ code ứng dụng ở Chương 13) — phần lớn logic
"tool A chạy xong, đưa kết quả cho tool B" được nối bằng dây (terminal-to-terminal) trong
QuickBuild, không cần một dòng code nào. **Script** chỉ nên xuất hiện khi logic *không* biểu diễn
được bằng cách nối dây và các tool tổng hợp (Chương 10-12) — ví dụ: một vòng lặp xử lý số lượng
đối tượng thay đổi động, một phép tính toán học phức tạp trên nhiều giá trị trung gian, hoặc điều
kiện rẽ nhánh phụ thuộc kết quả runtime mà `CogResultsAnalysisTool` không biểu diễn tự nhiên.

### 12.4.2 Cấu trúc một script: Initialize để "cầm" tham chiếu, override GroupRun để chạy

Script gắn vào `CogToolBlock` là một lớp kế thừa từ `CogToolBlockAdvancedScriptBase`. Điểm dễ
viết sai nhất — vì trực giác hay đoán nhầm có sẵn một property kiểu "ToolBlock" hoặc "Parent" —
là lớp cơ sở **không hề lộ ra property nào để truy cập ngược về khối chứa nó**. Cách đúng: override
`Initialize(CogToolGroup group)` (được gọi một lần khi ToolBlock khởi tạo script), tự khai báo một
field trong lớp con để giữ tham chiếu đó, rồi mới dùng field này trong `GroupRun`:

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.Blob;

public class InspectionScript : CogToolBlockAdvancedScriptBase
{
    private CogToolBlock _toolBlock;   // tự khai báo — lớp cơ sở không có sẵn field/property này

    public override void Initialize(CogToolGroup group)
    {
        base.Initialize(group);
        _toolBlock = group as CogToolBlock;   // CogToolBlock kế thừa CogToolGroup; ép kiểu để
                                                // dùng được Inputs/Outputs (không có trên CogToolGroup)
    }

    public override bool GroupRun(ref string errorMessage, ref CogToolResultConstants result)
    {
        // Truy cập tool trong ToolBlock qua tên đã đặt trong QuickBuild
        var blob = (CogBlobTool)_toolBlock.Tools["BlobPads"];
        var padCount = blob.Results.GetTopLevelBlobs(filtered: true).Count;

        // Ghi kết quả ra Outputs terminal — đây là cách ToolBlock "trả lời" ra ngoài
        _toolBlock.Outputs["PadCount"].Value = padCount;

        if (padCount != 4)
        {
            errorMessage = $"Đếm được {padCount} miếng đệm, mong đợi 4";
            result = CogToolResultConstants.Reject;
            return false;    // false = KHÔNG chạy tiếp các tool còn lại trong block
        }

        result = CogToolResultConstants.Accept;
        return true;         // true = tiếp tục chạy các tool tiếp theo trong block
    }
}
```

> ⚠️ **Cảnh báo:** `CogToolGroup` (kiểu tham số của `Initialize`) là lớp cha của `CogToolBlock`
> và **không có** `Inputs`/`Outputs` — chỉ `CogToolBlock` mới có. Nếu script này được gắn vào một
> `CogToolGroup` thuần (không phải `CogToolBlock`), phép ép kiểu `as CogToolBlock` trả về `null`
> và mọi thao tác `_toolBlock.Outputs[...]` sau đó ném `NullReferenceException` — kiểm tra `null`
> trước khi dùng nếu script có khả năng được tái sử dụng cho cả hai loại khối.

> ⚠️ **Cảnh báo:** `blob.Results` chỉ hợp lệ nếu `BlobPads` **đã chạy xong** trước thời điểm
> `GroupRun` đọc nó. Ví dụ trên giả định `BlobPads` được đặt ở vị trí **trước** script trong thứ
> tự tool đã cấu hình của `CogToolBlock` (script thường được gắn vào một vị trí cụ thể trong
> chuỗi, không phải chạy độc lập ngoài chuỗi) — kiểm tra rõ trong QuickBuild rằng thứ tự này đúng
> trước khi dựa vào `Results` của bất kỳ tool nào trong script; đọc kết quả của một tool chưa chạy
> trong cycle hiện tại là `null` hoặc dữ liệu cũ của cycle trước, tuỳ hành vi cụ thể của SDK — CẦN
> XÁC MINH TRÊN SDK THẬT cơ chế chạy tool chính xác trong `CogToolBlockAdvancedScriptBase`.

`GroupRun` trả về `bool`: `true` cho phép ToolBlock tiếp tục chạy các tool còn lại theo thứ tự
đã cấu hình; `false` dừng chuỗi ngay tại điểm đó. Tham số `ref result` (`CogToolResultConstants`)
là quyết định tổng hợp của cả block — chính là kiểu dữ liệu vừa gặp ở mục 12.3.3.

![Hình 12.1 — Kiến trúc CogToolBlock: Inputs, chuỗi tool nối dây, script GroupRun, Outputs](../assets/ch12/hinh_12_1.png)
**Hình 12.1 — Kiến trúc CogToolBlock: Inputs, chuỗi tool nối dây, script GroupRun, Outputs.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ khối (draw.io). Khung ngoài lớn nhãn "CogToolBlock
> (TB_Inspect)". Bên trái khung: cột "Inputs" với 1 terminal `InputImage`. Bên trong khung: chuỗi
> hộp nhỏ nối tiếp bằng mũi tên đại diện các tool đã cấu hình trong QuickBuild (PMAlign → Fixture
> → Caliper → Blob → ID), mỗi hộp có nhãn tên tool. Phía dưới chuỗi tool: hộp riêng biệt nhãn
> "Script (GroupRun)" với mũi tên nét đứt chỉ vào từng tool phía trên (ngụ ý: script đọc kết quả
> từ Tools[...] theo tên) và mũi tên đi ra tới cột "Outputs" bên phải (`Ok`, `PartX`, `PartY`,
> `PartAngle`, `Code`, `PadCount`). Chú thích nhỏ dưới sơ đồ: "Đường nối dây terminal-to-terminal
> (QuickBuild) xử lý phần lớn luồng dữ liệu; script chỉ can thiệp phần logic không nối dây được".

### 12.4.3 Ranh giới: script không thay thế kiến trúc ứng dụng

> 📌 **Lưu ý:** script trong `.vpp` là một phần **cấu hình job**, không phải một phần **mã nguồn
> ứng dụng** theo nghĩa Chương 13-14 định nghĩa. Nó không nằm trong solution C#, không qua code
> review cùng quy trình, không có unit test tự động đi kèm như code ứng dụng. Logic nghiệp vụ quan trọng
> (quyết định gửi robot chạy hay không, ghi log traceability, xử lý lỗi hệ thống) nên nằm trong
> code ứng dụng gọi ToolBlock từ ngoài — không nhét vào script bên trong `.vpp`. Script chỉ nên
> làm việc **thuộc về job**: tổng hợp/tính toán trực tiếp trên kết quả các tool trong cùng block.

## 12.5 Debug script

### 12.5.1 Lỗi biên dịch — đọc thông báo trong script editor

Script biên dịch ngay khi lưu trong QuickBuild's script editor; lỗi cú pháp hiện trực tiếp tại
dòng gây lỗi, giống trải nghiệm một IDE C# thu nhỏ. Khác biệt lớn nhất so với viết code trong
Visual Studio: **không có IntelliSense đầy đủ cho toàn bộ API VisionPro** trong mọi phiên bản
script editor — tên method/property sai chính tả có thể không được gợi ý sửa ngay, chỉ báo lỗi
biên dịch khi lưu. Thói quen hữu ích: viết đoạn logic phức tạp trong Visual Studio trước (tận
dụng IntelliSense đầy đủ, xem Chương 13, mục 13.1 về cách reference DLL VisionPro), rồi mới dán
vào script editor khi đã chắc chắn cú pháp đúng.

### 12.5.2 Runtime: log, và property ScriptError

Với lỗi xảy ra lúc **chạy** (không phải lúc biên dịch) — exception ném ra trong `GroupRun` —
`CogToolBlock` có property `ScriptError` (kiểu `string`) lưu lại thông báo lỗi gần nhất từ script.
Đây là điểm kiểm tra đầu tiên khi một ToolBlock có script "im lặng thất bại": đọc `toolBlock.ScriptError`
từ code ứng dụng hoặc từ CogRecordDisplay (Chương 5, mục 5.5) ngay sau khi `Run()` trả về, trước
khi nghi ngờ bất cứ tool nào khác trong chuỗi.

```csharp
toolBlockInstance.Run();

if (!string.IsNullOrEmpty(toolBlockInstance.ScriptError))
{
    logger.LogError("Script lỗi runtime: {Error}", toolBlockInstance.ScriptError);
}
```

Ghi log tường minh từ trong script — dùng `System.Diagnostics.Trace`/`Debug.WriteLine`, hoặc gọi
thẳng vào một logger đã đăng ký qua `ScriptData`/`UserData` (kiểu `CogDictionary`, cơ chế chia sẻ
dữ liệu giữa script và ToolBlock chứa nó) — là cách hiệu quả hơn nhiều so với đoán mò khi runtime
có vấn đề mà `ScriptError` không nắm bắt trọn vẹn (ví dụ một điều kiện logic sai nhưng không ném
exception nào).

### 12.5.3 Attach Visual Studio debugger

Khi log không đủ — cần đặt breakpoint, xem giá trị biến giữa chừng — VisionPro hỗ trợ debug script
bằng Visual Studio thật (breakpoint, Watch, Locals, Step Into/Over, Call Stack — đầy đủ như debug
một project C# bình thường), theo đúng 3 bước tài liệu chính thức của Cognex quy định
("Debugging Script with Visual Studio"):

1. **Đặt điểm dừng bằng code, không phải breakpoint UI.** Mỗi script C#/VB.NET mới tạo trong Job,
   ToolGroup hay ToolBlock đều có sẵn một dòng tự sinh (mặc định để dạng comment) ở đầu
   `GroupRun`:
   ```csharp
   // if (System.Diagnostics.Debugger.IsAttached)
   //     System.Diagnostics.Debugger.Break();
   ```
   Bỏ comment dòng này ở nơi muốn debugger dừng lại. Đây **không phải một mẹo dự phòng** — đây
   chính là cơ chế chính thức: `Debugger.Break()` chỉ có tác dụng khi `IsAttached` đã true, nên
   chèn sẵn ở đầu hàm là an toàn (không dừng gì nếu chưa có debugger nào đính kèm).
2. **Chuyển Compilation Mode của script sang Debug.** Ô combo cuối toolbar của Script Editor có
   hai lựa chọn Release/Debug. Ở chế độ Debug, VisionPro biên dịch script kèm thông tin debug đầy
   đủ và **tắt tối ưu hoá** — đây là bước bắt buộc, thiếu bước này Visual Studio không nạp được
   symbol dù attach đúng tiến trình. Đổi lại: hiệu năng script giảm ở chế độ Debug, nên **luôn
   chuyển lại Release** sau khi debug xong trước khi đưa job vào sản xuất.
3. **Attach Visual Studio vào tiến trình đang chạy.** Mở Job trong QuickBuild (đừng Run vội) →
   Visual Studio: Debug (hoặc Tools ở bản VS cũ) → Attach to Process → chọn
   `Cognex.VisionPro.QuickBuild.exe` → Attach. Quay lại QuickBuild, Run job — khi luồng thực thi
   chạy đến `Debugger.Break()`, Visual Studio tự động vào chế độ Break, mở đúng source script (nạp
   từ file tạm được biên dịch riêng cho phiên debug này). Từ đây có thể đặt thêm breakpoint bất kỳ
   dòng nào, Step Into (F11)/Step Over (F10), xem `blob.Results`, `_toolBlock.Outputs[...]` (mục
   12.4.2) qua Watch/Locals — y hệt debug một project C# bình thường.

> 📌 **Đã xác nhận với tài liệu chính thức Cognex** (mục "Debugging Script with Visual Studio",
> CHM đi kèm bản cài VisionPro 9.0 CR2): file tạm (source + assembly + file debug) được tạo trong
> thư mục con `VisionPro` bên dưới thư mục tạm hệ thống (biến môi trường `TEMP`, VisionPro tự dò
> qua `Path.GetTempPath()`) — Visual Studio dùng đúng các file này để khớp source lúc debug.

> ⚠️ **Cảnh báo:** debug bằng `MessageBox.Show(...)` chèn vào script để "xem giá trị" là cám dỗ
> lớn khi mới làm quen — và là cách chắc chắn nhất để **treo job production**: `MessageBox` chặn
> luồng thực thi chờ người dùng bấm OK, và nếu script này chạy trên máy sản xuất không có ai
> đứng cạnh màn hình, cả trạm dừng vô thời hạn. Dùng log (mục 12.5.2) hoặc attach debugger (mục
> này) — không bao giờ để lại `MessageBox`/`Console.ReadLine` hay bất kỳ lời gọi chờ tương tác
> người dùng nào trong script chạy sản xuất. Tương tự, đừng quên chuyển Compilation Mode về
> **Release** trước khi bàn giao — quên bước này không treo job, nhưng âm thầm ăn hiệu năng vì
> script chạy chưa tối ưu.

### 12.5.4 Ba bẫy thường gặp riêng của script

- **Script tham chiếu DLL ngoài không đi kèm `.vpp`.** Nếu script gọi vào một thư viện .NET tự
  viết (không phải DLL VisionPro chuẩn), thư viện đó phải hiện diện đúng vị trí trên **mọi** máy
  chạy job — không tự động đóng gói cùng file `.vpp`. Job chạy tốt trên máy dev, báo lỗi tải DLL
  trên máy sản xuất là hậu quả trực tiếp của việc quên bước này.
- **Script "biến mất" khi lưu sai cách.** Chỉnh sửa script ngoài script editor chính thức (ví dụ
  copy-paste code vào một text editor ngoài rồi paste lại) đôi khi không được QuickBuild ghi nhận
  đúng thay đổi vào state của `.vpp` — luôn lưu và **chạy thử lại ngay** sau khi sửa script để
  xác nhận thay đổi đã thực sự có hiệu lực, trước khi coi như đã sửa xong.
- **Exception trong script bị nuốt lặng lẽ.** Một exception ném ra trong `GroupRun` không phải
  lúc nào cũng dừng cả job theo cách hiển nhiên — tuỳ cấu hình property `AbortRunOnToolFailure`
  của ToolBlock, hành vi có thể là dừng hẳn hoặc tiếp tục với kết quả
  không xác định. Luôn kiểm `ScriptError` tường minh (mục 12.5.2) thay vì giả định "không thấy
  báo lỗi nghĩa là chạy đúng".

## 12.6 Color tools

Hai công cụ đáng biết tên khi bài toán chuyển từ "hình dạng/kích thước" sang "màu sắc là đặc
trưng phân loại chính": `CogColorMatchTool` so khớp màu đo được với một hoặc nhiều mẫu màu chuẩn
đã học (hữu ích cho kiểm tra đúng màu linh kiện/bao bì), và `CogColorExtractorTool` trích xuất
vùng ảnh thoả một điều kiện màu sắc cụ thể (tương tự vai trò `CogBlobTool` — Chương 10 — nhưng
điều kiện phân vùng là màu thay vì độ sáng). MeoVision không có bài toán phân loại theo màu nên
sách không triển khai chi tiết; biết tên hai tool này là đủ để tra cứu khi gặp bài toán như vậy.

## Tổng kết chương

- Nhóm ImageProcessing giải các việc "phụ trợ" mà job sản xuất nào cũng cần: chuyển đổi màu/kênh
  (`CogImageConvertTool`), số học ảnh (`CogIPTwoImage*Tool` — nền tảng của kỹ thuật trừ ảnh ở
  Chương 10), và kiểm độ nét (`CogImageSharpnessTool.Score`) như một cảnh báo sớm mất focus.
- `CogHistogramTool` biến việc đọc histogram bằng mắt (Chương 4) thành số đo tự động
  (`Mean`/`StandardDeviation`) — kết hợp với độ nét tạo thành bộ giám sát sức khoẻ quang học chạy
  song song với logic kiểm tra chính, nền tảng cho giám sát dài hạn ở Chương 16.
- `CogResultsAnalysisTool` gộp kết quả nhiều tool thành một cây biểu thức AND/OR/so sánh — dựng
  chủ yếu bằng QuickBuild, không phải viết tay bằng code. Quyết định trả về có **bốn** mức
  (`Accept`/`Warning`/`Reject`/`Error`), không chỉ nhị phân OK/NG.
- Script trong `CogToolBlock` override `GroupRun`, chỉ nên dùng khi logic không biểu diễn được
  bằng nối dây terminal hoặc tool tổng hợp — và không bao giờ thay thế logic nghiệp vụ quan trọng
  thuộc về code ứng dụng (Chương 13-14).
- Debug script: đọc lỗi compile trong script editor; property `ScriptError` là điểm kiểm tra đầu
  tiên cho lỗi runtime; khi cần breakpoint thật sự — bỏ comment `Debugger.Break()` có sẵn, chuyển
  Compilation Mode sang Debug, rồi attach Visual Studio vào tiến trình QuickBuild (đúng quy trình
  chính thức của Cognex, mục 12.5.3) — luôn trả lại Release trước khi bàn giao. Không bao giờ để
  `MessageBox`/chờ tương tác người dùng trong script chạy sản xuất.

## Lỗi thường gặp

**Lỗi 1 — Đo histogram trên vùng lẫn cả chi tiết.** Hiện tượng: chỉ số giám sát ánh sáng dao động
mạnh giữa các cycle dù ánh sáng thực tế ổn định. Nguyên nhân: `Region` của `CogHistogramTool`
trộn tín hiệu "ánh sáng đổi" với "chi tiết khác nhau". Cách tránh: đo trên vùng nền cố định, không
chứa chi tiết (mục 12.2.1).

**Lỗi 2 — Coi quyết định ResultsAnalysis chỉ có hai mức.** Hiện tượng: code xử lý kết quả chỉ
kiểm `== Accept` rồi coi mọi thứ khác là NG, bỏ lỡ khả năng phân biệt Warning với Reject/Error.
Nguyên nhân: quen tư duy OK/NG nhị phân từ các tool khác. Cách tránh: xử lý đủ 4 nhánh
`CogToolResultConstants` (mục 12.3.3), tận dụng Warning cho giám sát xu hướng (Chương 16).

**Lỗi 3 — Nhét business logic quan trọng vào script trong `.vpp`.** Hiện tượng: một quyết định
nghiệp vụ quan trọng (ví dụ điều kiện gửi robot chạy) nằm sâu trong script, không ai nhớ nó ở đó
khi debug sự cố từ phía code ứng dụng. Nguyên nhân: ranh giới "cái gì thuộc job, cái gì thuộc ứng
dụng" không được tôn trọng. Cách tránh: script chỉ tính toán/tổng hợp trong phạm vi tool block;
quyết định nghiệp vụ nằm trong code C# có version control, review, test (mục 12.4.3).

**Lỗi 4 — Để lại MessageBox/chờ tương tác, hoặc quên trả Compilation Mode về Release, sau khi
debug script.** Hiện tượng (a): trạm sản xuất treo vô thời hạn không rõ nguyên nhân, thường xảy
ra sau một lần sửa script vội. Hiện tượng (b): script chạy chậm hơn bình thường dù logic không
đổi, khó phát hiện vì không có lỗi/exception nào. Nguyên nhân: (a) code debug tạm thời (MessageBox,
breakpoint tương đương chờ người dùng) không được dọn trước khi đưa job vào sản xuất; (b) quên
chuyển Compilation Mode của script từ Debug về lại Release sau khi debug xong — chế độ Debug tắt
tối ưu hoá biên dịch (mục 12.5.3). Cách tránh: dùng log/ScriptError thay MessageBox; rà lại script
VÀ kiểm tra ô Compilation Mode trong Script Editor trước khi chốt chương trình đưa vào sản xuất.

**Lỗi 5 — Script tham chiếu DLL ngoài không triển khai kèm máy đích.** Hiện tượng: job chạy hoàn
hảo trên máy dev, báo lỗi ngay khi mở trên máy sản xuất. Nguyên nhân: `.vpp` không tự đóng gói
DLL phụ thuộc ngoài VisionPro. Cách tránh: liệt kê rõ mọi dependency ngoài của script và đưa
chúng vào danh mục kiểm tra triển khai của trạm (xem checklist triển khai ở Phụ lục B).

\newpage

# Chương 13 — Lập trình VisionPro bằng C#

Job `TB_Inspect` của MeoVision chạy hoàn hảo trong QuickBuild: mở file `.vpp`, bấm Run, xem ảnh
và overlay kết quả ngay trên màn hình. Nhưng máy sản xuất không có ai ngồi mở QuickBuild lúc 2
giờ sáng — nó cần một ứng dụng .NET tự khởi động cùng Windows, tự kết nối camera, tự chạy job mỗi
khi PLC gửi trigger, và tự phục hồi nếu có lỗi. Khoảng cách giữa "job chạy được trong QuickBuild"
và "trạm chạy được trong sản xuất không người" chính là nội dung của chương này.

Đây là chương bản lề của cả cuốn sách: mọi khái niệm đã học — coordinate space (Chương 7), tool
và kết quả (Chương 8–11), ToolBlock và terminal (Chương 12) — giờ được gọi từ code C# thay vì
click chuột. Chúng ta đi theo đúng thứ tự một ứng dụng vision thật được dựng lên: cấu hình project
đúng (mục 13.1), nạp job đã thiết kế (13.2), trao đổi dữ liệu qua terminal (13.3), tự thu ảnh từ
camera (13.4), hiển thị kết quả (13.5), rồi đến phần dễ bị bỏ qua nhất nhưng gây hậu quả nặng nề
nhất khi sai — quản lý tài nguyên (13.6) và luồng thực thi (13.7).

## 13.1 Solution setup

### 13.1.1 Reference đúng DLL, đúng platform, đúng framework

VisionPro không phải một package NuGet — các DLL nằm trong thư mục cài đặt
(`C:\Program Files\Cognex\VisionPro\ReferencedAssemblies\`) và được thêm vào project qua **Add
Reference → Browse** của Visual Studio.
Không có một DLL "tất cả trong một" — mỗi nhóm tool nằm trong assembly riêng, và project chỉ cần
reference đúng những gì thực sự dùng:

**Bảng 13.1 — DLL cần reference cho một ứng dụng MeoVision điển hình.**

| DLL | Chứa gì |
|---|---|
| `Cognex.VisionPro.dll` | Lõi giao diện tool: `ICogTool`, `CogToolBlockTerminal`, các exception chung |
| `Cognex.VisionPro.Core.dll` | `ICogImage` + coordinate space tree, `CogSerializer` (nạp/lưu file), `CogAcqFifoTool` (thu ảnh) |
| `Cognex.VisionPro.Controls.dll` | `CogRecordDisplay` (hiển thị WinForms) |
| `Cognex.VisionPro.ToolGroup.dll` | `CogToolBlock`, `CogToolGroup` |
| `Cognex.VisionPro.PMAlign.dll`, `.Caliper.dll`, `.Blob.dll`, `.ID.dll`, `.OCRMax.dll`, `.CalibFix.dll` | Từng nhóm tool cụ thể — chỉ thêm những nhóm job thực sự dùng |

Hai ràng buộc cứng, sai một trong hai là lỗi build hoặc lỗi runtime khó chẩn đoán:

- **Platform target: x64.** VisionPro 9.x cài cả bin 32-bit và 64-bit (đã thấy ở
  `bin`/`CogPlus` song song trong thư mục cài đặt), nhưng ứng dụng production nên nhắm x64 nhất
  quán — trộn AnyCPU với DLL native 64-bit gây lỗi tải assembly ngay khi khởi động.
- **.NET Framework, không phải .NET hiện đại.** Các DLL VisionPro 9.x biên dịch cho .NET Framework
  4.8 — project gọi trực tiếp API VisionPro phải target .NET Framework. Phần kiến trúc ứng dụng
  hiện đại hơn (tách process, giao tiếp qua IPC) sẽ bàn ở Chương 14, mục 14.2 cho những ai muốn
  phần còn lại của hệ thống dùng .NET hiện đại.

### 13.1.2 Máy dev vs máy sản xuất: license

VisionPro yêu cầu license hợp lệ để **chạy** tool (không chỉ để thiết kế trong QuickBuild) — máy
dev thường dùng license mềm hoặc dongle gắn với máy đó; máy sản xuất cần license runtime riêng,
được cấp phép và cài đặt độc lập với máy dev. Đây là việc cần lên kế hoạch **trước** khi triển
khai, không phải phát hiện lúc mang máy ra dây chuyền mới thấy job không chạy được vì thiếu
license — quy trình cấp phép cụ thể (số lượng, loại license, kích hoạt) nằm ngoài phạm vi kỹ
thuật của sách, nhưng luôn nằm trong checklist triển khai (Phụ lục B).

## 13.2 Nạp .vpp và chạy ToolBlock

### 13.2.1 CogSerializer — cầu nối giữa file thiết kế và object trong bộ nhớ

File `.vpp` do QuickBuild lưu ra là một object đã serialize — nạp lại nó thành một `CogToolBlock`
sống trong bộ nhớ ứng dụng qua `CogSerializer.LoadObjectFromFile`:

**Code 13.1 — Nạp CogToolBlock từ file .vpp và chạy.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;

CogToolBlock toolBlock = (CogToolBlock)CogSerializer.LoadObjectFromFile(
    @"C:\MeoVision\Jobs\TB_Inspect.vpp", typeof(CogToolBlock));

toolBlock.Run();

ICogRunStatus status = toolBlock.RunStatus;
if (status.Result != CogToolResultConstants.Accept &&
    status.Result != CogToolResultConstants.Warning)
{
    logger.LogError(status.Exception, "ToolBlock lỗi: {Message}", status.Message);
}

double processingMs = status.ProcessingTime;   // dùng để giám sát ngân sách cycle (Chương 15)
```

`ICogRunStatus` (trả về từ `RunStatus` của mọi tool — không riêng ToolBlock) là điểm kiểm tra
đầu tiên sau mỗi lần `Run()`: `Result` (bốn mức Accept/Warning/Reject/Error — đã gặp ở Chương 12,
mục 12.3.3), `Message`, `Exception` khi có lỗi, và `ProcessingTime`/`TotalTime` cho việc đo ngân
sách thời gian.

> 📌 **Lưu ý:** `LoadObjectFromFile` trả về `object` — luôn `cast` tường minh sang đúng kiểu
> mong đợi (`CogToolBlock` ở đây). Nạp nhầm file (sai đường dẫn, hoặc file .vpp chứa kiểu object
> khác — ví dụ một `CogJob` thay vì `CogToolBlock`) ném `InvalidCastException` ngay tại dòng này,
> lỗi rõ ràng hơn nhiều so với để lỗi trôi xuống tận lúc gọi `Run()`.

### 13.2.2 Đường dẫn file .vpp: đừng hard-code

Đường dẫn tuyệt đối trong Code 13.1 chỉ để minh hoạ — ứng dụng thật đọc đường dẫn từ cấu hình
(appsettings, hoặc từ recipe hiện hành — Chương 14, mục 14.3 bàn kỹ khái niệm recipe cho vision).
Đây cũng là điểm để **kiểm tra file tồn tại và load thành công** trước khi coi trạm sẵn sàng chạy
sản xuất — job không nạp được là lỗi phải chặn ngay ở bước khởi động, không phải để lộ ra lúc
cycle đầu tiên.

## 13.3 Terminal: hợp đồng giữa vision engineer và software engineer

### 13.3.1 Inputs/Outputs — điểm chạm duy nhất giữa app và job

Đưa ảnh vào và lấy kết quả ra khỏi ToolBlock đi qua đúng một cửa: `Inputs`/`Outputs` (kiểu
`CogToolBlockTerminalCollection`), mỗi terminal (`CogToolBlockTerminal`) truy cập bằng tên đã đặt
trong QuickBuild lúc thiết kế job. Đây chính là "hợp đồng" đã nhắc từ Chương 5, mục 5.4: vision
engineer thiết kế và đặt tên terminal trong QuickBuild, software engineer viết code chỉ cần biết
đúng những cái tên đó — không cần biết bên trong ToolBlock có bao nhiêu tool, nối dây ra sao.

**Code 13.2 — Ghi ảnh vào Inputs, đọc kết quả từ Outputs theo hợp đồng terminal MeoVision.**

```csharp
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;

// Hợp đồng terminal của TB_Inspect (đặt tên khi thiết kế job trong QuickBuild —
// xem reference/MeoVision_Spec.md): Inputs["InputImage"];
// Outputs["Ok"], ["PartX"], ["PartY"], ["PartAngle"], ["Code"]
toolBlock.Inputs["InputImage"].Value = acquiredImage;
toolBlock.Run();

bool ok = (bool)toolBlock.Outputs["Ok"].Value;
if (ok)
{
    double x     = (double)toolBlock.Outputs["PartX"].Value;
    double y     = (double)toolBlock.Outputs["PartY"].Value;
    double angle = (double)toolBlock.Outputs["PartAngle"].Value;
    string code   = (string)toolBlock.Outputs["Code"].Value;
}
```

`Terminal.Value` có kiểu `object` — luôn cần `cast` tường minh về đúng kiểu dữ liệu đã thống nhất
với vision engineer khi thiết kế terminal (`Terminal.ValueType` cho biết kiểu mong đợi, hữu ích để
kiểm tra bằng code thay vì chỉ dựa vào tài liệu). Sai tên terminal (đánh máy nhầm chuỗi) hoặc sai
kiểu cast đều ném exception ngay khi chạy — rõ ràng hơn một lỗi logic âm thầm, nhưng vẫn nên có
một bước kiểm tra tự động xác nhận đúng bộ terminal ngay sau khi nạp job, chạy lúc ứng dụng
khởi động.

> ⚠️ **Cảnh báo:** tên terminal là chuỗi ký tự, không được compiler kiểm tra. Đổi tên terminal
> trong QuickBuild (dù chỉ để "cho rõ nghĩa hơn") mà không cập nhật code gọi nó là nguồn lỗi runtime
> âm thầm phổ biến nhất trong tích hợp VisionPro-C#. Giữ một nơi duy nhất định nghĩa các hằng số
> tên terminal (không rải chuỗi ma thuật khắp code) để đổi tên chỉ cần sửa một chỗ.

## 13.4 Acquisition từ code

### 13.4.1 Khi acquisition nằm ngoài ToolBlock

Chương 6 đã dùng `CogAcqFifoTool` **bên trong** job QuickBuild. Khi ứng dụng C# cần chủ động điều
khiển vòng lặp thu ảnh — chờ trigger từ PLC, quyết định lúc nào chụp, xử lý timeout — thao tác
với `CogAcqFifoTool` trực tiếp từ code linh hoạt hơn để nó "trôi" bên trong một ToolBlock cố định.
Cơ chế cốt lõi là hai bước tách biệt: `StartAcquire()` yêu cầu phần cứng bắt đầu thu (trả về một
ID thao tác), và `CompleteAcquire(id, ...)` chờ và lấy ảnh đã thu xong về dưới dạng `ICogImage`.

**Code 13.3 — Thu một ảnh từ camera theo mô hình Start/Complete, có timeout.**

```csharp
using Cognex.VisionPro;

var acqTool = new CogAcqFifoTool();
acqTool.Operator.Connect(frameGrabber);       // đã cấu hình từ Chương 6
acqTool.Operator.Timeout        = 500;         // ms
acqTool.Operator.TimeoutEnabled  = true;

acqTool.Operator.Prepare();                    // chuẩn bị phần cứng — gọi 1 lần trước vòng lặp

int acqId = acqTool.Operator.StartAcquire();   // kích hoạt thu ảnh (trigger phần cứng/mềm)

int width, height;
ICogImage acquired = acqTool.Operator.CompleteAcquire(acqId, out width, out height);
// acquired là ảnh MỚI mỗi lần gọi — sở hữu bởi caller, PHẢI Dispose khi dùng xong (mục 13.6)
// CẦN XÁC MINH TRÊN SDK THẬT: chữ ký chính xác của CompleteAcquire (tên/số tham số out) theo
// đúng phiên bản VisionPro đang dùng trước khi đưa vào code thật — đối chiếu IntelliSense/tài
// liệu chính thức thay vì copy nguyên văn ví dụ minh hoạ này.
```

### 13.4.2 Sở hữu ảnh: ai chịu trách nhiệm giải phóng

Mỗi lần `CompleteAcquire` (hay `Acquire` — bản gộp cả hai bước) trả về, đó là một **ảnh mới**
trong bộ nhớ — quyền sở hữu (và trách nhiệm giải phóng) thuộc về code gọi nó, không tự động được
VisionPro dọn dẹp. Trong một vòng lặp acquisition chạy liên tục hàng nghìn cycle mỗi ca, quên giải
phóng ảnh cũ là nguyên nhân rò bộ nhớ kinh điển nhất của ứng dụng vision — mục 13.6 đi sâu vào
đúng vấn đề này.

> ⚠️ **Cảnh báo:** "sở hữu bởi caller" không có nghĩa là Dispose ngay lập tức sau
> `toolBlock.Run()` trong mọi trường hợp. Nếu ảnh đó còn cần cho hiển thị (`CreateLastRunRecord()`
> ở mục 13.5.1 tham chiếu ngược lại ảnh đã chạy), Dispose quá sớm có thể làm hỏng overlay hiển
> thị hoặc ném exception khi `CogRecordDisplay` cố vẽ một ảnh đã giải phóng. Nguyên tắc an toàn:
> chỉ Dispose ảnh sau khi **mọi** nơi còn cần đến nó (xử lý, đọc Outputs, tạo Record hiển thị)
> đã dùng xong trong cùng cycle — CẦN XÁC MINH TRÊN SDK THẬT liệu `CreateLastRunRecord()` giữ
> tham chiếu trực tiếp đến ảnh gốc hay tự sao chép riêng, trước khi chốt điểm Dispose chính xác
> trong kiến trúc ứng dụng thật.

## 13.5 Hiển thị: CogRecordDisplay

### 13.5.1 Record — không chỉ là ảnh, mà là ảnh + overlay kết quả

`CogRecordDisplay` (namespace `Cognex.VisionPro`, trong `Cognex.VisionPro.Controls.dll`) là
control WinForms hiển thị không chỉ ảnh thô mà cả **overlay kết quả** — vùng ROI, điểm biên
caliper tìm được, khung định vị PMAlign — tất cả đến từ một property duy nhất: `Record` (kiểu
`ICogRecord`). Đây chính là cơ chế đứng sau debug bằng hình ảnh đã dùng xuyên suốt các chương
trước (Chương 5, mục 5.5; Chương 7, mục 7.4) — giờ gọi trực tiếp từ code ứng dụng:

```csharp
using Cognex.VisionPro;

recordDisplay.Record = toolBlock.CreateLastRunRecord();   // ảnh + toàn bộ overlay kết quả
```

### 13.5.2 WinForms gốc, WPF qua interop

`CogRecordDisplay` là control WinForms thuần — ứng dụng WinForms dùng trực tiếp; ứng dụng WPF
(lựa chọn phổ biến cho HMI công nghiệp hiện đại) nhúng nó qua `WindowsFormsHost`, giống cách bất
kỳ control WinForms bên thứ ba nào được host trong WPF.

> 📌 **Lưu ý:** cập nhật `Record` từ một luồng nền (ví dụ luồng xử lý ảnh chạy song song với UI
> thread — mục 13.7) phải **dispatch về đúng UI thread** trước khi gán — nguyên tắc chung của
> WinForms/WPF: mọi cập nhật control từ callback nền phải đi qua `Control.Invoke` (WinForms) hoặc
> `Dispatcher` (WPF). Gán `Record` trực tiếp từ luồng xử lý ảnh là lỗi cross-thread kinh điển,
> không riêng gì với VisionPro.

## 13.6 Quản lý tài nguyên

### 13.6.1 ICogImage không tự khai báo IDisposable — điều bất ngờ quan trọng nhất chương

Đây là điểm cần dừng lại kỹ nhất trong chương: kiểm tra trực tiếp trên assembly VisionPro cho
thấy interface `ICogImage` (kiểu mà mọi biến ảnh trong sách này khai báo, theo đúng nguyên tắc
lập trình theo interface) **không** kế thừa `IDisposable`. Chỉ các lớp triển khai cụ thể như
`CogImage8Grey` mới thực thi `IDisposable`. Hệ quả trực tiếp: một biến khai báo kiểu `ICogImage`
**không thể** gọi `.Dispose()` hay dùng trong khối `using` mà không ép kiểu:

```csharp
ICogImage img = acqTool.Operator.CompleteAcquire(acqId, out _, out _);

// SAI — không compile: ICogImage không có Dispose()
// img.Dispose();

// ĐÚNG — ép về IDisposable trước khi giải phóng
(img as IDisposable)?.Dispose();

// Hoặc khi biết chắc kiểu cụ thể ngay từ đầu, khai báo đúng kiểu đó để dùng được `using`
using (var grey = (CogImage8Grey)acqTool.Operator.CompleteAcquire(acqId, out _, out _))
{
    // xử lý ảnh trong khối này
}
```

> ⚠️ **Cảnh báo:** đây không phải chi tiết vặt — nó là lý do phổ biến khiến code "trông như đã
> Dispose đầy đủ" (biến `ICogImage` được gán `null` sau khi dùng, tưởng vậy là đủ) vẫn rò bộ nhớ
> trong thực tế. Với mọi biến `ICogImage` sống đủ lâu để cần giải phóng chủ động — không chỉ ảnh
> từ acquisition (mục 13.4) mà cả ảnh trung gian trong xử lý thủ công — luôn ép kiểu về
> `IDisposable` hoặc khai báo bằng kiểu cụ thể ngay từ đầu.

### 13.6.2 Vòng đời tool và ToolBlock

Tool (`CogPMAlignTool`, `CogCaliperTool`...) và `CogToolBlock` cũng triển khai `IDisposable` —
khác `ICogImage`, các đối tượng này thường sống **suốt vòng đời ứng dụng** (tạo một lần lúc khởi
động, tái sử dụng mỗi cycle, `Dispose()` khi ứng dụng đóng), nên không cần khối `using` ngắn hạn
như ảnh — quản lý vòng đời của chúng theo mô hình service/singleton thông thường, chỉ cần đảm bảo
`Dispose()` được gọi đúng một lần khi ứng dụng dừng, theo đúng `IDisposable` pattern chuẩn của
.NET.

### 13.6.3 Ảnh lớn và garbage collector

Ảnh độ phân giải cao (Chương 3: camera MeoVision 5 MP, mỗi ảnh mono ~5 MB chưa nén) chiếm bộ
nhớ đáng kể, và phần dữ liệu pixel thường nằm trong bộ nhớ **không quản lý (unmanaged)** đằng sau
lớp vỏ `.NET` — GC không "nhìn thấy" áp lực bộ nhớ thực sự cho đến khi nó tự quyết định chạy, có
thể trễ hơn nhiều so với tốc độ ảnh mới được tạo ra trong một trạm chạy liên tục. Đây là lý do
Dispose **chủ động, tường minh** (mục 13.6.1) quan trọng hơn hẳn với ảnh so với các object .NET
thông thường — chờ GC dọn dẹp một chuỗi ảnh lớn không được Dispose là con đường chắc chắn dẫn đến
rò bộ nhớ tăng dần trong ca sản xuất kéo dài nhiều giờ.

## 13.7 Đa luồng với VisionPro

### 13.7.1 Tool không thread-safe

Nguyên tắc bắt buộc: **một tool không được gọi `Run()` đồng thời từ hai luồng khác nhau.** Đây
không phải giới hạn của riêng VisionPro — nhiều SDK xử lý ảnh hiệu năng cao chia sẻ ràng buộc
này vì trạng thái nội bộ (bộ nhớ đệm trung gian, kết quả lần chạy trước) không được thiết kế để
an toàn khi truy cập song song. Mô hình an toàn: **một tool/ToolBlock gắn với đúng một luồng xử
lý**, không chia sẻ instance giữa nhiều luồng dù có khoá (`lock`) bao quanh — khoá bảo vệ được
tính đúng đắn của dữ liệu nhưng không loại bỏ được chi phí tranh chấp và không phải lúc nào cũng
đủ để đảm bảo an toàn với mọi trạng thái nội bộ không công khai của tool.

![Hình 13.1 — Luồng dữ liệu: acquisition → hàng đợi → xử lý ToolBlock → hiển thị](../assets/ch13/hinh_13_1.png)
**Hình 13.1 — Luồng dữ liệu: acquisition → hàng đợi → xử lý ToolBlock → hiển thị.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ khối ngang (draw.io). Trái sang phải: hộp "Luồng
> Acquisition" (chứa `CogAcqFifoTool`, `StartAcquire/CompleteAcquire`) → mũi tên vào hộp trụ
> "Channel<ICogImage> (bounded)" → mũi tên ra hộp "Luồng Xử lý" (chứa `CogToolBlock.Run()`,
> `Inputs/Outputs`) → rẽ 2 nhánh: (a) hộp "CogRecordDisplay (UI thread)" ghi rõ "dispatch qua
> Invoke/Dispatcher" (mục 13.5.2), (b) hộp "Gửi kết quả" trỏ sang icon PLC/robot (Chương 15).
> Chú thích dưới sơ đồ: "Không tool nào được gọi Run() từ 2 luồng cùng lúc — acquisition và xử
> lý tách biệt hoàn toàn qua hàng đợi".

### 13.7.2 Mô hình hàng đợi: tách luồng thu ảnh khỏi luồng xử lý

Khi acquisition (chờ trigger, chờ phần cứng) và xử lý ảnh (chạy ToolBlock) cần chạy độc lập —
để acquisition không bị chặn trong lúc xử lý ảnh trước đó còn đang chạy — mô hình phổ biến là
**một luồng thu ảnh, một luồng xử lý, nối nhau bằng hàng đợi** (`System.Threading.Channels.Channel<T>`
— hàng đợi bất đồng bộ chuẩn của .NET): luồng acquisition đẩy ảnh mới vào
channel, luồng xử lý đọc và chạy ToolBlock tuần tự — cả hai luồng không bao giờ cùng gọi `Run()`
trên cùng một tool.

```csharp
var imageChannel = Channel.CreateBounded<ICogImage>(capacity: 2);

// Luồng acquisition
await imageChannel.Writer.WriteAsync(acquiredImage, ct).ConfigureAwait(false);

// Luồng xử lý (riêng biệt) — dùng WaitToReadAsync/TryRead thay vì `await foreach` trên
// IAsyncEnumerable<T>: cú pháp async streams (C# 8+) và kiểu IAsyncEnumerable<T> không có sẵn
// trên .NET Framework 4.8 (mục 13.1.1) nếu không thêm gói Microsoft.Bcl.AsyncInterfaces và tự
// nâng LangVersion — mẫu dưới đây tương thích trực tiếp với .NET Framework 4.8 + C# 7.3, không
// cần gói/cấu hình bổ sung nào ngoài System.Threading.Channels.
while (await imageChannel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
{
    while (imageChannel.Reader.TryRead(out ICogImage img))
    {
        toolBlock.Inputs["InputImage"].Value = img;
        toolBlock.Run();
        (img as IDisposable)?.Dispose();   // giải phóng ngay sau khi dùng xong (mục 13.6.1)
    }
}
```

> 🔍 **Đào sâu thêm:** dung lượng hàng đợi (`capacity: 2` ở trên) là quyết định có chủ đích, không
> phải con số tuỳ tiện — hàng đợi không giới hạn khiến ảnh dồn ứ khi xử lý chậm hơn thu ảnh, làm
> trễ ngày càng tăng thay vì báo lỗi sớm; hàng đợi quá nhỏ khiến acquisition bị chặn (`WriteAsync`
> chờ) khi xử lý tạm thời chậm hơn bình thường. Chọn dung lượng dựa trên ngân sách cycle thực đo
> được của trạm (Chương 15) và mức "gánh được vài cycle chậm" thay vì để trạm tự dồn ứ vô hạn.

## Tổng kết chương

- Project gọi VisionPro: reference đúng DLL cần thiết (Bảng 13.1), platform x64 bắt buộc, .NET
  Framework 4.8 cho phần gọi trực tiếp API; license runtime máy sản xuất là việc lên kế hoạch
  trước, không phải phát hiện lúc triển khai.
- `CogSerializer.LoadObjectFromFile` nạp `.vpp` thành `CogToolBlock` sống trong bộ nhớ; luôn kiểm
  `ICogRunStatus` (`Result`/`Message`/`Exception`/`ProcessingTime`) sau mỗi `Run()`.
- Terminal `Inputs`/`Outputs` là hợp đồng duy nhất giữa app và job — tên terminal là chuỗi không
  được compiler kiểm tra, giữ hằng số tập trung một nơi để tránh lỗi runtime khi đổi tên.
- Acquisition từ code dùng mô hình `StartAcquire()`/`CompleteAcquire()`; mỗi ảnh trả về là ảnh
  mới, quyền sở hữu và trách nhiệm giải phóng thuộc về code gọi.
- **`ICogImage` không kế thừa `IDisposable`** — chỉ lớp cụ thể (`CogImage8Grey`...) mới có; luôn
  ép kiểu `as IDisposable` hoặc khai báo kiểu cụ thể để giải phóng đúng, đây là nguồn rò bộ nhớ
  phổ biến nhất bị bỏ sót. Tool/ToolBlock sống suốt vòng đời ứng dụng, Dispose một lần khi đóng.
- Tool không thread-safe: một instance gắn đúng một luồng xử lý; tách acquisition và xử lý bằng
  hàng đợi có giới hạn dung lượng khi cần chạy song song, không bao giờ chia sẻ tool giữa các luồng.

## Lỗi thường gặp

**Lỗi 1 — Trộn platform AnyCPU với DLL VisionPro 64-bit.** Hiện tượng: lỗi tải assembly ngay khi
khởi động ứng dụng, thông báo không rõ ràng về nguyên nhân thật. Nguyên nhân: VisionPro DLL native
gắn chặt với kiến trúc CPU cụ thể. Cách tránh: đặt Platform Target = x64 tường minh cho toàn bộ
solution (mục 13.1.1).

**Lỗi 2 — Đổi tên terminal trong QuickBuild mà quên cập nhật code.** Hiện tượng: exception "not
found" khi truy cập `Inputs`/`Outputs` sau một lần chỉnh sửa job tưởng chừng vô hại. Nguyên nhân:
tên terminal là chuỗi, không được compiler kiểm tra tại thời điểm build. Cách tránh: hằng số tên
terminal tập trung một nơi; test tự động xác nhận bộ terminal ngay sau khi nạp job (mục 13.3.1).

**Lỗi 3 — Rò bộ nhớ vì tưởng `ICogImage` có sẵn `Dispose()`.** Hiện tượng: bộ nhớ ứng dụng tăng
dần đều trong ca sản xuất kéo dài, không có exception nào báo lỗi. Nguyên nhân: `ICogImage` không
kế thừa `IDisposable`; gán biến về `null` không giải phóng bộ nhớ unmanaged phía sau. Cách tránh:
luôn ép kiểu `as IDisposable` hoặc dùng kiểu cụ thể (mục 13.6.1) — đây là lỗi quan trọng nhất
chương này nhấn mạnh.

**Lỗi 4 — Gán `CogRecordDisplay.Record` từ luồng xử lý ảnh, không dispatch về UI thread.** Hiện
tượng: exception cross-thread ngẫu nhiên hoặc UI treo/vẽ sai. Nguyên nhân: control WinForms/WPF
chỉ được cập nhật an toàn từ UI thread tạo ra nó. Cách tránh: dispatch qua `Control.Invoke`/
`Dispatcher.InvokeAsync` trước khi gán (mục 13.5.2).

**Lỗi 5 — Gọi `Run()` trên cùng một tool từ hai luồng.** Hiện tượng: kết quả sai lệch ngẫu nhiên,
exception hiếm gặp khó tái hiện, đặc biệt dưới tải cao. Nguyên nhân: tool không thread-safe, hai
luồng cùng truy cập trạng thái nội bộ không được bảo vệ. Cách tránh: mô hình một tool/một luồng;
tách acquisition và xử lý bằng hàng đợi thay vì gọi tool từ nhiều luồng (mục 13.7).

\newpage

# Chương 14 — Kiến trúc ứng dụng vision hoàn chỉnh (MeoVision)

Chương 13 cho chúng ta đủ mảnh ghép để "gọi được VisionPro từ C#": nạp job, đưa ảnh vào, đọc kết
quả ra. Nhưng ráp các mảnh ghép đó thẳng vào một `Form1.cs` — nạp `.vpp` trong sự kiện Load, gọi
`Run()` trong sự kiện nhận trigger, vẽ overlay trong sự kiện Paint — cho ra một ứng dụng **chạy
được**, nhưng không **sống được** qua sáu tháng vận hành thực tế: đổi lô hàng cần threshold khác
thì phải sửa code và build lại; camera hỏng cần thay thì không ai dám đụng vào vì không biết code
đụng tới VisionPro ở bao nhiêu chỗ; kỹ sư ca đêm phát hiện job cần train lại pattern nhưng không
có cách nào làm việc đó ngoài việc gọi kỹ sư vision dậy mở QuickBuild.

Đây chính là bài toán kiến trúc phần mềm kinh điển — cùng loại bài toán mà tầng điều khiển máy
tự động vẫn giải bằng các nguyên lý Clean Architecture. Chương này áp dụng đúng những nguyên lý
đó (DIP — Dependency Inversion, tách lớp theo trách nhiệm) cho tầng vision, với một câu hỏi xuyên
suốt: **VisionPro là một chi tiết triển khai (implementation detail), không phải trung tâm kiến
trúc.**

## 14.1 Yêu cầu của một ứng dụng vision sản xuất

Bốn yêu cầu sau đây phân biệt "demo chạy được trên bàn" với "trạm chạy được trong sản xuất" —
mỗi yêu cầu kéo theo một quyết định kiến trúc cụ thể sẽ triển khai trong các mục tiếp theo:

**Bảng 14.1 — Bốn yêu cầu sản xuất và mục giải quyết tương ứng.**

| Yêu cầu | Vì sao quan trọng | Giải quyết ở |
|---|---|---|
| Chạy 24/7 không người can thiệp | Không ai mở QuickBuild lúc 2 giờ sáng (đúng tình huống mở Chương 13) | 14.2 (kiến trúc), 14.6 (nhiều trạm) |
| Đổi recipe không cần build lại code | Đổi lô hàng, đổi model sản phẩm là chuyện xảy ra hàng tuần/hàng ngày | 14.3 |
| Kỹ sư hiện trường teach lại được | Score PMAlign tụt (Chương 8, mục 8.5) cần retrain — không phải lúc nào cũng có kỹ sư vision sẵn tại chỗ | 14.4 |
| Log đầy đủ, có kỷ luật | Truy vết lỗi, phân tích xu hướng (Chương 16) cần dữ liệu lưu lại đúng cách, không tràn đĩa | 14.5 |

## 14.2 Kiến trúc phân lớp: cô lập VisionPro sau một interface

### 14.2.1 IVisionEngine — áp dụng lại DIP cho tầng vision

Nguyên tắc **Interface Over Implementation** — field, tham số constructor, kiểu trả về luôn dùng
interface, không bao giờ để phần còn lại của ứng dụng biết đến kiểu cụ thể — áp dụng nguyên vẹn
ở đây: mọi phần khác của ứng dụng — UI, logic recipe, logic gửi
kết quả cho robot — chỉ nói chuyện với một interface `IVisionEngine`, không bao giờ trực tiếp gọi
`CogToolBlock`, `CogSerializer`, hay bất kỳ kiểu `Cog*` nào.

**Code 14.1 — Interface IVisionEngine: ranh giới duy nhất giữa ứng dụng và VisionPro.**

```csharp
namespace MeoVision.Core.Contracts
{
    public interface IVisionEngine : IDisposable
    {
        string EngineName { get; }       // "Simulated" / "Cognex.VisionPro" — chỉ để hiển thị/log
        bool IsJobLoaded { get; }

        Task LoadRecipeAsync(VisionRecipe recipe, CancellationToken ct = default);
        Task<VisionResult> InspectAsync(VisionJobRequest request, CancellationToken ct = default);
        Task TeachPatternAsync(VisionTrainRegion trainRegion, CancellationToken ct = default);

        event EventHandler<VisionResult>? InspectionCompleted;
    }

    public enum VisionOutcome { Accept, Warning, Reject, Error }
    public enum VisionFailureReason { None, PartNotFound, MeasurementFailed, SystemError }

    public sealed record VisionResult(
        Guid CorrelationId, string JobName, VisionOutcome Outcome, VisionFailureReason FailureReason,
        double Score, double X, double Y, double AngleDeg, string? Code,
        IReadOnlyList<VisionCheckResult> Checks, DateTimeOffset TimestampUtc, double ProcessingTimeMs,
        Bitmap? Image);    // ảnh trung lập dạng Bitmap (BCL, không phải kiểu VisionPro nào)
}
```

`VisionResult` là một **kiểu POCO** (một `record` C# thuần) do `MeoVision.Core` định nghĩa —
không một field nào của nó là kiểu VisionPro, không có ngoại lệ. Ảnh (`Image`) dùng
`System.Drawing.Bitmap`: có sẵn trong .NET Framework, và mọi lớp `ICogImage` cụ thể
(`CogImage8Grey`...) đều có constructor nhận `Bitmap` và method `ToBitmap()` — Engine (lớp duy
nhất được phép `using Cognex.VisionPro.*`) chuyển đổi hai chiều mà không cần viết thêm code
chuyển đổi thủ công nào. `CorrelationId` (Chương 15, mục 15.3.2) giúp truy vết đúng kết quả khớp
đúng lần trigger nào khi có retry; `Outcome` (4 giá trị, không phải một cờ NG duy nhất — đúng tinh
thần Chương 15) phân biệt Accept/Warning/Reject/Error, còn `FailureReason` phân biệt tiếp *vì sao*
Reject — không tìm thấy vật (`PartNotFound`, đáng retry) khác với đo được nhưng sai
(`MeasurementFailed`, retry vô nghĩa, xem mục 15.4.2). Đây là ranh giới quan trọng nhất chương:
implementation của `IVisionEngine` (lớp duy nhất được phép `using Cognex.VisionPro.*`) đọc
terminal (Chương 13, mục 13.3), ép kiểu, đóng gói thành `VisionResult` — mọi thứ phía sau
interface không cần biết VisionPro tồn tại. Nếu muốn hiển thị sâu hơn bằng `CogRecordDisplay`
thật (Chương 13, mục 13.5), đó là một kênh riêng ở tầng UI, không đi qua `VisionResult` (Bảng
14.2) — giữ interface hoàn toàn sạch quan trọng hơn việc nhét thêm một field tiện dụng.

> ⚠️ **Cảnh báo:** phiên bản `LoadJobAsync`/`TeachPatternAsync`/`InspectionResult` ở Chương 13 là
> bản **rút gọn để dạy khái niệm** — chưa có tham số `recipe`, chưa phân biệt `VisionJobRequest`
> (mang `CorrelationId` + ảnh cấp sẵn nếu có) khỏi việc Engine tự thu ảnh, chưa có `Outcome`/
> `FailureReason` tách bạch. Các phần này được bổ sung ở đây vì Code 14.2 (`VisionRecipe`) và
> Code 14.3 (kiểm tra `Outcome`, lưu ảnh NG) đều cần đến chúng — đây cũng chính là hình dạng THẬT
> của `IVisionEngine` trong dự án đồng hành MeoVision (không phải bản giản lược riêng cho sách).

### 14.2.2 Ba lớp, ba trách nhiệm

**Bảng 14.2 — Ba lớp kiến trúc và trách nhiệm.**

| Lớp | Trách nhiệm | Được phép biết VisionPro? |
|---|---|---|
| `MeoVision.Engine` | Triển khai `IVisionEngine`; toàn bộ code gọi trực tiếp API VisionPro (Chương 13) sống ở đây, và **chỉ** ở đây | Có — đây là nơi duy nhất |
| `MeoVision.Application` | Recipe, quy trình cycle (trigger → inspect → gửi kết quả), logging, alarm | Không — chỉ biết `IVisionEngine`/`VisionResult` |
| `MeoVision.UI` | Màn hình vận hành, màn teach (mục 14.4), hiển thị `CogRecordDisplay` | Chỉ phần hiển thị (mục 13.5) cần biết kiểu VisionPro cho control hiển thị — cô lập trong code-behind view, ViewModel vẫn chỉ biết `IVisionEngine` |

Lợi ích cụ thể, không trừu tượng: nâng cấp từ VisionPro 9.x lên phiên bản mới hơn, hay (giả định)
thay thế bằng SDK vision khác cho một trạm cụ thể, chỉ đụng đến `MeoVision.Engine` — `Application`
và phần lớn `UI` không đổi một dòng. Viết unit test cho logic recipe/cycle không cần
VisionPro cài trên máy CI — chỉ cần mock `IVisionEngine`.

> 📌 **Lưu ý:** ranh giới này **không** có nghĩa là "giấu VisionPro đi cho đẹp kiến trúc" — nó
> phản ánh đúng thực tế: kiến thức về coordinate space, terminal, tuning tham số tool (Phần II-III
> của sách) là chuyên môn của **vision engineer**; logic recipe, luồng cycle, giao tiếp PLC là
> chuyên môn của **software engineer**. `IVisionEngine` là đường ranh giới giao tiếp giữa hai vai
> trò đó — đúng tinh thần "hợp đồng terminal" đã nói ở Chương 5, mục 5.4 và Chương 13, mục 13.3,
> giờ nâng lên một tầng trừu tượng cao hơn.

![Hình 14.1 — Ba lớp kiến trúc MeoVision và ranh giới IVisionEngine](../assets/ch14/hinh_14_1.png)
**Hình 14.1 — Ba lớp kiến trúc MeoVision và ranh giới IVisionEngine.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sơ đồ khối phân lớp dọc (draw.io), theo phong cách sơ đồ
> Clean Architecture. Từ trên xuống: "MeoVision.UI" (màn vận hành, màn teach) → mũi tên
> xuống "MeoVision.Application" (recipe, cycle, logging, alarm) → mũi tên xuống một đường kẻ đậm
> nhãn "IVisionEngine (interface)" → dưới đường kẻ là "MeoVision.Engine" chứa icon/nhãn các lớp
> VisionPro thật (CogToolBlock, CogSerializer, CogAcqFifoTool...). Bên phải đường kẻ đậm, chú
> thích: "Chỉ MeoVision.Engine được using Cognex.VisionPro.*". Vẽ thêm nhánh phụ song song với
> Engine thật: hộp "SimulatedVisionEngine" cùng cắm vào đường kẻ IVisionEngine (mục 14.6.2),
> ngụ ý UI/Application không phân biệt được đang chạy engine thật hay giả lập.

## 14.3 Recipe cho vision

### 14.3.1 Hai loại tham số: trong .vpp và ngoài .vpp

File `.vpp` (Chương 13) tự nó đã là một dạng "cấu hình" — nhưng không phải mọi tham số nên nằm
trong đó. Phân biệt rõ hai loại:

- **Tham số thuộc về job** (nằm trong `.vpp`, do vision engineer chỉnh trong QuickBuild): vùng
  ROI, thuật toán train pattern, cấu trúc chuỗi tool. Thay đổi những thứ này đòi hỏi hiểu biết
  chuyên sâu về VisionPro — không nên và không thể expose ra "recipe đổi được từ UI vận hành".
- **Tham số thuộc về recipe** (nằm **ngoài** `.vpp`, trong file cấu hình riêng của
  `MeoVision.Application` — theo đúng khái niệm recipe quen thuộc của máy tự động: bộ tham số
  gắn với model sản phẩm): ngưỡng dung sai đo
  lường (Chương 9, mục 9.4), ngưỡng `AcceptThreshold` của PMAlign (Chương 8, mục 8.5), số lượng
  đối tượng kỳ vọng (Chương 10, mục 10.3). Đây là những con số **kỹ sư dây chuyền** (không nhất
  thiết là vision engineer) cần đổi khi chuyển model sản phẩm, mà không cần mở QuickBuild.

**Code 14.2 — VisionRecipe: tham số ngoài .vpp mà kỹ sư dây chuyền đổi được.**

```csharp
namespace MeoVision.Core.Contracts
{
    public sealed record VisionRecipe(
        string RecipeId, string JobName, string JobFilePath, string SampleImageFolder,
        double MinAcceptScore,       // ghi đè AcceptThreshold của PMAlign lúc load job
        double PartWidthMinMm,
        double PartWidthMaxMm,
        int ExpectedPadCount,
        int MaxRetries = 2,          // Chương 15, mục 15.4 — số lần retry khi PartNotFound
        int CycleTimeoutMs = 2000,
        int JobLoadTimeoutMs = 10_000,
        // Tham số camera/đèn (Chương 6) — chỉ Engine thật đọc, bản giả lập bỏ qua
        CameraTriggerMode TriggerMode = CameraTriggerMode.Semi,
        bool StrobeEnabled = false,
        double StrobeDelayUs = 0,
        double StrobePulseDurationUs = 100);
}
```

`SampleImageFolder` phục vụ `SimulatedVisionEngine` (mục 14.6.2, không cần khi chạy Engine thật);
`MaxRetries`/`CycleTimeoutMs`/`JobLoadTimeoutMs` phục vụ đúng công thức worst-case-cycle-time và
bounded retry đã dạy ở Chương 15 — đặt trên recipe (không hard-code) vì mỗi model sản phẩm có thể
cần ngân sách thời gian khác nhau. `IVisionEngine.LoadRecipeAsync` (mục 14.2.1), sau khi nạp
`.vpp`, áp các tham số recipe vào đúng `RunParams` tương ứng (`AcceptThreshold`, ngưỡng đo,
`RunTimeMeasures`...) trước khi coi job sẵn sàng chạy — cầu nối giữa hai thế giới tham số nằm ở
lớp Engine, đúng như ranh giới đã vạch ở mục 14.2.2.

### 14.3.2 Đổi recipe: dữ liệu, không phải deploy

Đổi recipe là thao tác **đổi dữ liệu cấu hình**, không phải triển khai lại phần mềm — tải file
recipe mới (hoặc chọn từ danh sách đã lưu), gọi lại `LoadJobAsync` với tham số mới, không build
lại solution. Nguyên tắc này giữ cho việc đổi model sản xuất nằm trong tầm với của kỹ sư dây
chuyền theo đúng quy trình vận hành, không phụ thuộc vào có mặt của lập trình viên.

## 14.4 Màn hình vận hành và màn teach

### 14.4.1 Màn vận hành: tối thiểu nhưng đủ

Theo tinh thần ISA-101/High-Performance HMI — giao diện yên tĩnh khi bình thường, chỉ nổi bật
khi cần chú ý — màn vận hành MeoVision cần: ảnh mới nhất kèm overlay kết quả
(`CogRecordDisplay`, Chương 13, mục 13.5), trạng thái OK/NG rõ ràng bằng màu (đúng bảng màu theo
mức nghiêm trọng đã thống nhất trong hệ thống HMI chung), và lịch sử N cycle gần nhất — không cần
hơn. Chi tiết bố cục, màu sắc, kích cỡ theo đúng chuẩn HMI đã áp dụng cho toàn bộ hệ thống
MeoFrame — chương này không lặp lại, chỉ nhấn mạnh **nguồn dữ liệu**: mọi phần tử trên màn hình
đọc từ `VisionResult` (POCO, mục 14.2.1), không bao giờ đọc trực tiếp từ đối tượng VisionPro.

### 14.4.2 Màn teach: đưa việc train pattern ra khỏi tay kỹ sư vision

Score PMAlign tụt theo lô hàng mới (Chương 8, mục 8.5) đòi hỏi retrain — nhưng chờ kỹ sư vision
có mặt để mở QuickBuild là một chi phí vận hành thực tế đáng kể. Màn teach là một màn hình
riêng, quyền hạn giới hạn (chỉ Engineer trở lên — đúng phân quyền `UserLevel` đã thống nhất trong
hệ thống), cho phép:

1. Chụp ảnh chi tiết hiện tại từ camera đang kết nối.
2. Vẽ/điều chỉnh vùng train (`TrainRegion`, Chương 8, mục 8.2) trên chính ảnh vừa chụp.
3. Gọi `IVisionEngine.TeachPatternAsync` — bên trong, Engine gọi `pattern.Execute()` (Chương 8)
   với ảnh và vùng vừa chọn.
4. **Backup file `.vpp` cũ trước khi ghi đè** — nguyên tắc đã cảnh báo ở Chương 8: retrain không
   backup là rủi ro không có đường lùi nếu mẫu mới tệ hơn mẫu cũ.
5. Chạy thử job vừa teach trên một bộ ảnh gần đây (không phải chỉ ảnh vừa chụp) trước khi xác
   nhận thay thế job đang chạy sản xuất.

Đây là quyền năng thực sự của kiến trúc phân lớp: xây một màn hình teach *chỉ gọi được* các thao
tác VisionPro đã được `IVisionEngine` cho phép (train, không phải toàn bộ API VisionPro) — kỹ sư
dây chuyền không thể vô tình phá hỏng cấu trúc job theo cách QuickBuild đầy đủ tính năng có thể
cho phép.

> ⚠️ **Cảnh báo:** màn teach ghi đè trực tiếp lên `.vpp` đang chạy production, không qua backup,
> là kịch bản mất dữ liệu thật đã cảnh báo ở Chương 8, mục 8.5. Bước 4 ở trên không phải tuỳ chọn.

### 14.4.3 Phân quyền chi tiết hơn khi hệ thống lớn dần

`UserLevel` ở trên (2 cấp: Operator/Engineer) đủ cho một trạm với đúng một màn hình cần gác —
Teach. Hệ thống có nhiều màn hình cấu hình hơn (nhiều loại vision, nhiều tham số theo từng bước
recipe — mục 14.3, đa trạm — mục 14.6) thường phải trả lời hai câu hỏi mà mô hình 2 cấp cứng chưa
trả lời được:

- **Ai được vào MÀN NÀO** không nên là một chuỗi `if (level >= X)` rải rác khắp code — mỗi màn
  hình mới lại phải nhớ thêm một chỗ kiểm tra, dễ sót. Thay bằng một **ma trận quyền** (menu/màn
  hình × cấp truy cập → cho phép/không) do người quản trị cấu hình, không phải lập trình viên
  hard-code từng trường hợp. Thêm màn hình mới chỉ cần thêm một dòng vào ma trận, không sửa logic
  kiểm tra quyền ở nơi khác.
- **Ai được sửa THAM SỐ NÀO** bên trong một màn hình — không phải lúc nào quyền cũng dừng ở mức
  "vào được cả màn hay không". Một màn cấu hình recipe (mục 14.3) có thể cho phép Operator xem mọi
  tham số, Engineer sửa ngưỡng đo, nhưng chỉ cấp cao nhất mới được thêm/bớt hạng mục kiểm tra —
  ba mức quyền trên CÙNG một màn hình, không phải ba màn hình riêng.

Một quyết định khác cũng đáng cân nhắc sớm, dù ví dụ đơn giản của chương này chưa cần: **xác thực
lại bao lâu một lần**. Hai chính sách phổ biến, đánh đổi khác nhau giữa an toàn và trải nghiệm:

**Bảng 14.3 — Hai chính sách xác thực lại và đánh đổi.**

| Chính sách | An toàn | Trải nghiệm | Phù hợp khi |
|---|---|---|---|
| Xác thực lại mỗi lần bấm menu cần quyền | Cao nhất — không ai "mượn" phiên đăng nhập của người khác quá lâu | Phiền nếu thao tác lặp lại nhiều lần liên tiếp | Môi trường nhiều người dùng chung một máy, thay ca thường xuyên |
| Chỉ xác thực lại khi đổi từ chế độ Chạy sang chế độ Cấu hình | Vừa đủ — vẫn chặn được thao tác nhầm khi máy đang chạy | Mượt hơn khi cùng một kỹ sư làm nhiều việc cấu hình liên tiếp | Ca làm việc ổn định, ít người dùng chung 1 phiên |

Không có chính sách nào "đúng tuyệt đối" — chọn theo thực tế vận hành (một máy nhiều người thay
phiên, hay một kỹ sư gắn với một ca cố định). Nguyên tắc chung đáng giữ dù chọn chính sách nào:
**chức năng tắt hẳn xác thực** (nếu có, cho môi trường thử nghiệm/demo) tự nó phải cần quyền cao
nhất mới bật/tắt được — nếu không, nó trở thành lối đi vòng qua toàn bộ hệ thống phân quyền.

## 14.5 Lưu ảnh có kỷ luật

### 14.5.1 Cái gì lưu, cái gì không

Nguyên tắc mặc định: **lưu ảnh NG kèm metadata, không lưu mọi ảnh OK.** Một trạm chạy vài nghìn
cycle mỗi ca, lưu toàn bộ ảnh OK lấp đầy ổ đĩa chỉ sau vài ngày — thường lộ rõ nhất sau khoảng
hai tuần vận hành liên tục — trong khi giá trị thông tin gần như bằng không: ảnh OK không cần
điều tra lại.

**Code 14.3 — Chỉ lưu ảnh khi NG/Warning, kèm metadata.**

```csharp
public sealed record InspectionLogEntry(
    Guid CorrelationId, string JobName, VisionOutcome Outcome, VisionFailureReason FailureReason,
    double Score, DateTimeOffset TimestampUtc, int AttemptCount, string? SavedImagePath);

// Chỉ lưu ảnh khi Reject/Warning/Error — Accept không giữ ảnh (Chương 12, mục 12.3.3 đã giới
// thiệu 4 mức, không chỉ nhị phân OK/NG)
bool shouldArchive = result.Outcome is VisionOutcome.Reject or VisionOutcome.Warning or VisionOutcome.Error;
if (shouldArchive)
{
    string path = await _ngImageArchiver.ArchiveIfNeededAsync(
        result.Image, result.Outcome, result.CorrelationId.ToString(), ct).ConfigureAwait(false);
    entry = entry with { SavedImagePath = path };
}
```

> 📌 **Lưu ý:** quyết định "outcome nào cần lưu" (`shouldArchive`) cố tình đặt ở tầng gọi
> (`InspectionCycleService`), không phải bên trong chính lớp lưu ảnh (`INgImageArchiver`) — tách
> **policy** (khi nào lưu — nghiệp vụ, có thể đổi theo yêu cầu) khỏi **cơ chế** (cách lưu + xoá
> vòng khi đầy, mục 14.5.2 — kỹ thuật, ít khi đổi). Gộp hai việc này vào một lớp làm việc unit-test
> "chỉ lưu khi NG" khó viết đúng — phải giả lập cả hệ thống file thay vì chỉ kiểm tra lớp lưu ảnh
> có được GỌI hay không.

Nguyên tắc "chỉ lưu NG" là mặc định hợp lý, không phải luật cứng. Một biến thể thường gặp trong
thực tế: lưu thêm một tỉ lệ nhỏ ảnh OK (ví dụ 1 trên 100 cycle) để phục vụ audit định kỳ hoặc xác
nhận trạm vẫn đo đúng trên chi tiết tốt — không phải để điều tra lỗi (ảnh OK không cần điều tra),
mà để có mẫu đối chiếu khi cần. Vì đây vẫn là quyết định **policy** (lưu bao nhiêu phần trăm, khi
nào), nó thuộc `InspectionCycleService` như `shouldArchive` ở trên, không phải một nhánh rẽ mới
bên trong `INgImageArchiver`.

### 14.5.2 Chính sách xoá vòng

Ngay cả chỉ lưu ảnh NG, dung lượng vẫn tăng theo thời gian nếu không có chính sách xoá. Hai tham
số cần quyết định tường minh, không để mặc định "vô hạn": **thời gian giữ tối đa** (ví dụ 90 ngày,
theo yêu cầu truy vết của ngành — liên hệ Chương 11, mục 11.4) và **dung lượng đĩa tối đa** dành
cho ảnh log, với cơ chế xoá ảnh cũ nhất khi chạm ngưỡng (vòng — FIFO) thay vì để đầy đĩa rồi ứng
dụng crash hoặc ngừng lưu log âm thầm.

> 💡 **Mẹo thực chiến:** đặt cảnh báo (không phải lỗi dừng máy) khi dung lượng còn lại cho ảnh
> log dưới một ngưỡng an toàn — phát hiện sớm một trạm sắp hết chỗ lưu còn tốt hơn nhiều so với
> phát hiện *sau khi* nó đã dừng ghi log được vài ngày mà không ai để ý.

### 14.5.3 Định dạng lưu ảnh: nén và khả năng chạy lại

Khi dung lượng đĩa thật sự là vấn đề (nhiều camera, tần suất cao), nén ảnh log là cách hiển nhiên
để tiết kiệm chỗ — nhưng phải phân biệt rõ hai loại nén trước khi chọn:

- **Không nén hoặc nén không mất dữ liệu** (ví dụ PNG, hoặc TIFF+LZW): ảnh phục hồi lại **chính
  xác từng pixel** như lúc lưu. Chạy lại ảnh này qua `IVisionEngine.InspectAsync` (mục 14.2.1) cho
  kết quả giống hệt lần chạy gốc.
- **Nén mất dữ liệu** (ví dụ JPEG): ảnh phục hồi có nhiễu nén (artifact) không tồn tại trong ảnh
  gốc — nhỏ tới mức mắt thường khó thấy, nhưng đủ để đổi score PMAlign/Caliper vài phần nghìn.
  Với một trạm đặt ngưỡng sát biên (mục 16.2, Chương 16), mức nhiễu đó có thể đổi Accept thành
  Reject khi chạy lại — hoặc ngược lại.

> ⚠️ **Cảnh báo:** ảnh nén mất dữ liệu **không đáng tin cậy để chạy thử lại** (test-run) hay dùng
> làm golden set nghiệm thu (mục 16.3, Chương 16). Được phép dùng cho mục đích xem lại bằng mắt
> (operator kiểm tra nhanh ảnh NG trông thế nào), nhưng không dùng để tái hiện/gỡ lỗi số liệu đo —
> hai mục đích này cần yêu cầu định dạng khác nhau. Nếu dung lượng buộc phải nén, cân nhắc: nén
> mất dữ liệu cho kho lưu dài hạn (xem bằng mắt), giữ bản không nén trong một cửa sổ thời gian ngắn
> hơn (vài ngày, đủ để chạy thử lại khi mới phát hiện lỗi) trước khi nén hoặc xoá.

### 14.5.4 Nhiều lỗi trong một lần kiểm tra: chọn lỗi đại diện

`VisionResult.Checks` (mục 14.2.1) là một **danh sách** — một lần kiểm tra có thể có nhiều mục
trong `Checks` cùng lúc không đạt (ví dụ vừa thiếu một điểm hàn vừa có vết trầy trên cùng một
chi tiết), nhưng `Outcome`/`FailureReason` ở cấp `VisionResult` lại là **một giá trị duy nhất**.
Khi nhiều mục kiểm tra cùng fail, giá trị duy nhất đó phải chọn ĐẠI DIỆN cho lỗi nào — và lựa chọn
này ảnh hưởng trực tiếp tới màu hiển thị trên màn vận hành (mục 14.4.1) và log được ghi lại
(mục 14.5.1), nên không nên để ngẫu nhiên (ví dụ "mục fail đầu tiên tìm thấy trong vòng lặp").

**Bảng 14.4 — Ví dụ xếp hạng độ ưu tiên khi nhiều lỗi xảy ra đồng thời.**

| Hạng | Loại lỗi | Lý do xếp hạng cao |
|---|---|---|
| 1 (cao nhất) | Thiếu chi tiết/không thấy vật (`PartNotFound`) | Không đo được gì thêm — các phép đo khác trên cùng ảnh vô nghĩa |
| 2 | Sai lệch nghiêm trọng vị trí/kích thước | Ảnh hưởng trực tiếp khả năng lắp ráp ở công đoạn sau |
| 3 | Lỗi bề mặt/thẩm mỹ (trầy, bẩn) | Thường không chặn công đoạn sau, nhưng vẫn cần ghi nhận |

Thứ hạng cụ thể phụ thuộc quy trình từng nhà máy — bảng trên chỉ minh hoạ NGUYÊN TẮC (lỗi ảnh
hưởng chức năng xếp trên lỗi thẩm mỹ), không phải giá trị cố định. Điểm thiết kế quan trọng hơn
bảng xếp hạng cụ thể: **giữ nguyên toàn bộ `Checks`** trong log (không chỉ lưu lỗi đại diện) —
lỗi đại diện phục vụ hiển thị nhanh, nhưng điều tra sau này (mục 16.4, Chương 16) thường cần biết
TẤT CẢ những gì đã fail trong lần kiểm tra đó, không chỉ mục được chọn hiển thị.

### 14.5.5 Sao lưu: vị trí vật lý khác, không chỉ thư mục khác

Chính sách xoá vòng (mục 14.5.2) bảo vệ khỏi đầy đĩa; nó không bảo vệ khỏi **hỏng đĩa**. Một thư
mục backup nằm trên cùng ổ vật lý với dữ liệu gốc mất cùng lúc với dữ liệu gốc nếu ổ đó hỏng — bảo
vệ được đúng một loại rủi ro (ghi đè/xoá nhầm bằng thao tác phần mềm), không bảo vệ được loại rủi
ro phổ biến hơn trong môi trường sản xuất (hỏng ổ cứng vật lý, mất điện đột ngột giữa lúc ghi).
Sao lưu tự động, nếu có, nên nhắm tới một ổ đĩa vật lý khác (hoặc vị trí mạng khác) — cùng tinh
thần với nguyên tắc backup trước khi ghi đè đã cảnh báo cho file `.vpp` (Chương 8, mục 8.5): bản
sao lưu chỉ có giá trị nếu nó không thể mất **cùng lúc** với bản gốc.

## 14.6 Nhiều camera, nhiều trạm, và simulation mode

### 14.6.1 Nhiều trạm trong một ứng dụng

Khi một ứng dụng điều khiển nhiều trạm vision (nhiều camera, mỗi trạm một `IVisionEngine` riêng),
kiến trúc phân lớp ở mục 14.2 mở rộng tự nhiên: mỗi trạm là một instance `IVisionEngine` độc lập
(đăng ký DI theo tên/key), `Application` điều phối nhiều
instance đó theo đúng logic pipeline của dây chuyền (`IStationSyncService` nếu các trạm cần đồng
bộ với nhau — khái niệm đã có ở kiến trúc MeoFrame).

### 14.6.2 Simulation mode: phát triển và test không cần camera thật, không cần license VisionPro

Đúng nguyên tắc **Simulation Parity** — mọi thành phần phụ thuộc phần cứng đều có một đối tác
giả lập chạy được không cần thiết bị thật — `IVisionEngine` cũng nên có một triển khai giả lập:
`SimulatedVisionEngine`. Khác với một bản nháp kiến trúc sớm hơn của chương này (từng hình dung
bản giả lập vẫn chạy `CogToolBlock` thật, chỉ giả lập khâu thu ảnh), bản THẬT trong dự án đồng
hành MeoVision giả lập **toàn bộ thuật toán**, không đụng đến Cognex.VisionPro.* dù chỉ một dòng.
Lý do: mục tiêu của simulation mode là "chạy được demo/test trên máy không có license VisionPro"
(nhấn mạnh lại ở Tổng kết chương) — nếu bản giả lập vẫn cần chạy `CogToolBlock` thật, nó vẫn cần
license, không đạt mục tiêu đó.

Việc "đánh giá" (so khớp phép đo với ngưỡng recipe → Accept/Warning/Reject) tách thành một hàm
THUẦN, tất định (`SimulatedEvaluator`, không random) — tách riêng khỏi phần *tạo* số đo giả lập
(có random, đúng tinh thần "không hard-code giá trị trả về" của Simulation Parity). Tách như vậy
để phần đánh giá test được trực tiếp, không cần điều khiển seed ngẫu nhiên.

**Code 14.4 — SimulatedVisionEngine: cùng interface, toàn bộ thuật toán giả lập, không Cognex.**

```csharp
public sealed class SimulatedVisionEngine : IVisionEngine
{
    private readonly Random _random;   // inject được qua constructor — test tự truyền seed cố định

    public async Task<VisionResult> InspectAsync(VisionJobRequest request, CancellationToken ct = default)
    {
        // Không đọc ảnh từ CogAcqFifoTool/file — GenerateReading() lấy ngưỡng từ VisionRecipe hiện
        // hành làm "sự thật nền", jitter ngẫu nhiên quanh đó (rộng hơn dải dung sai một chút, để
        // thỉnh thoảng thật sự tạo ra Reject — nếu luôn nằm trong dải, mọi lần chạy đều Accept và
        // không test được gì)
        SimulatedReading reading = GenerateReading(_activeRecipe);

        // Hàm THUẦN — không random, test trực tiếp được: cùng input luôn cùng output
        (VisionOutcome outcome, VisionFailureReason reason, double score, var checks) =
            SimulatedEvaluator.Evaluate(reading, _activeRecipe);

        Bitmap image = SyntheticPartRenderer.Render(reading, ...); // vẽ thủ tục bằng System.Drawing
        return new VisionResult(/* ... */);
    }
}
```

## Tổng kết chương

- Bốn yêu cầu sản xuất (chạy 24/7, đổi recipe không build lại, teach được tại hiện trường, log
  có kỷ luật) định hình toàn bộ quyết định kiến trúc của chương.
- `IVisionEngine` cô lập toàn bộ API VisionPro sau một interface — áp dụng nguyên tắc DIP cho
  tầng vision; ba lớp Engine/Application/UI phân chia theo đúng ranh giới chuyên môn vision
  engineer/software engineer.
- Recipe tách bạch hai loại tham số: thuộc job (trong `.vpp`, vision engineer chỉnh) và thuộc
  recipe (ngoài `.vpp`, kỹ sư dây chuyền đổi không cần build lại phần mềm).
- Màn teach đưa việc retrain pattern (Chương 8) ra khỏi phụ thuộc vào kỹ sư vision có mặt tại
  chỗ — nhưng bắt buộc backup trước khi ghi đè job production. Hệ thống lớn dần thường cần phân
  quyền chi tiết hơn 2 cấp cứng: ma trận quyền theo từng màn hình/tham số, và một chính sách xác
  thực lại rõ ràng (mỗi lần bấm menu, hay chỉ khi đổi chế độ) thay vì mặc định ngầm.
- Lưu ảnh có kỷ luật: mặc định chỉ lưu NG (và Warning) kèm metadata, có chính sách xoá vòng theo
  thời gian/dung lượng — không lưu vô hạn. Nén mất dữ liệu (JPEG) đánh đổi dung lượng lấy khả năng
  chạy lại đáng tin cậy — không dùng cho ảnh cần test-run/golden set. Khi nhiều mục kiểm tra cùng
  fail một lúc, giữ nguyên toàn bộ `Checks` trong log dù chỉ hiển thị một lỗi đại diện. Sao lưu tự
  động nên nhắm ổ đĩa vật lý khác, không chỉ thư mục khác trên cùng ổ.
- Nhiều trạm mở rộng tự nhiên qua nhiều instance `IVisionEngine`; `SimulatedVisionEngine` giả lập
  **toàn bộ thuật toán** (không chỉ nguồn ảnh) — cho phép phát triển/test không cần camera thật
  VÀ không cần license VisionPro, đánh đổi lấy việc không phát hiện được lỗi riêng của job `.vpp`
  thật (chỉ `CognexVisionEngine` mới lộ ra loại lỗi đó).

## Lỗi thường gặp

**Lỗi 1 — Để kiểu VisionPro rò rỉ ra khỏi Engine.** Hiện tượng: `ViewModel` hoặc logic recipe có
`using Cognex.VisionPro`, tham chiếu trực tiếp `CogToolBlock`/`ICogImage`. Nguyên nhân: tiện tay
lấy giá trị trực tiếp từ terminal thay vì đi qua `VisionResult`. Cách tránh: rà `using` trong
mọi project ngoài `MeoVision.Engine` — không có project nào khác được phép có `using Cognex.*`.

**Lỗi 2 — Trộn tham số job và tham số recipe.** Hiện tượng: đổi model sản phẩm đòi hỏi mở
QuickBuild sửa `.vpp` dù chỉ là đổi một ngưỡng dung sai. Nguyên nhân: ngưỡng đáng ra thuộc recipe
lại bị hard-code cứng trong job lúc thiết kế ban đầu. Cách tránh: phân loại rõ tham số nào "thuộc
job" (cấu trúc, thuật toán) và "thuộc recipe" (ngưỡng, số lượng kỳ vọng) ngay từ lúc thiết kế job
với vision engineer (mục 14.3.1).

**Lỗi 3 — Teach đè `.vpp` sản xuất không backup.** Hiện tượng: retrain làm job tệ đi (mẫu chọn
không đại diện), không có đường lùi về job cũ. Nguyên nhân: bỏ qua bước backup trong màn teach.
Cách tránh: backup tự động, bắt buộc, trước mọi lần ghi đè — không tuỳ chọn tắt được từ UI
(mục 14.4.2).

**Lỗi 4 — Lưu mọi ảnh OK, đầy đĩa sau vài ngày/tuần.** Hiện tượng: ổ đĩa lưu ảnh đầy, ứng dụng
crash hoặc ngừng ghi log âm thầm giữa ca sản xuất. Nguyên nhân: không phân biệt "cần lưu để điều
tra" (NG/Warning) với "không cần lưu" (OK), không có chính sách xoá vòng. Cách tránh: mặc định
chỉ lưu NG/Warning kèm metadata; đặt giới hạn dung lượng + thời gian giữ tường minh (mục 14.5).

**Lỗi 5 — Không có simulation mode, mọi phát triển/test phải có camera thật.** Hiện tượng: không
thể viết unit test tự động cho `Application`/`UI` trong CI; phát triển tính năng mới
bị chặn khi trạm thật đang bận sản xuất. Nguyên nhân: `IVisionEngine` chỉ có một triển khai gắn
chặt phần cứng. Cách tránh: luôn có `SimulatedVisionEngine` giả lập toàn bộ thuật toán (không cần
license VisionPro để chạy), đúng nguyên tắc Simulation Parity (mục 14.6.2).

**Lỗi 6 — Nén ảnh log để tiết kiệm dung lượng, phát hiện muộn là ảnh không dùng lại được.** Hiện
tượng: cần chạy thử lại (test-run) một ảnh NG cũ để gỡ lỗi hoặc đưa vào golden set (Chương 16, mục
16.3), nhưng kết quả chạy lại không khớp log gốc — nghi ngờ sai `IVisionEngine`, mất thời gian điều
tra nhầm hướng. Nguyên nhân thật: ảnh đã lưu bằng định dạng nén mất dữ liệu (JPEG), nhiễu nén đủ để
đổi kết quả đo. Cách tránh: dùng định dạng không mất dữ liệu (PNG/TIFF+LZW) cho bất kỳ ảnh nào có
thể cần chạy lại; nếu bắt buộc nén để tiết kiệm dung lượng, giới hạn chỉ áp dụng cho ảnh đã qua một
khoảng thời gian nhất định, không nén ảnh còn trong cửa sổ có thể cần điều tra (mục 14.5.3).

\newpage

# Chương 15 — Vision trong máy tự động: giao tiếp PLC và robot

Trạm MeoVision từng dừng cả dây chuyền suốt bốn phút vì một lý do không liên quan gì đến thị giác
máy: camera bị rút cáp trong lúc bảo trì ca trước, không ai cắm lại. Vision engine treo ở bước
chờ ảnh — không lỗi, không exception, không log — vì code chờ ảnh vô thời hạn, đúng như nó được
viết. PLC, phía bên kia, chờ tín hiệu "xong" từ vision cũng vô thời hạn, đúng như nó được lập
trình. Hai hệ thống làm đúng những gì được yêu cầu, và kết quả là dây chuyền đứng im cho đến khi
người vận hành đi ngang qua nhận thấy đèn báo vision không sáng.

Sự cố này không phải lỗi thuật toán vision — mọi kỹ thuật từ Chương 7 đến Chương 14 vẫn đúng.
Nó là lỗi **hợp đồng giao tiếp**: không bên nào định nghĩa "chờ bao lâu thì coi là bất thường".
Chương này giải quyết đúng lớp vấn đề đó — không phải cách vision nhìn thấy vật thể, mà cách
vision **nói chuyện** với phần còn lại của máy một cách có kỷ luật, có giới hạn thời gian, và có
ranh giới trách nhiệm rõ ràng.

## 15.1 Handshake chuẩn: trigger → result

### 15.1.1 Bốn tín hiệu tối thiểu

Một giao tiếp vision-PLC bền vững cần tối thiểu bốn tín hiệu, dù truyền qua I/O rời rạc, thanh
ghi PLC, hay bản tin TCP — bản chất logic không đổi theo phương tiện truyền:

**Bảng 15.1 — Bốn tín hiệu tối thiểu của handshake vision-PLC.**

| Tín hiệu | Ai đặt | Ý nghĩa |
|---|---|---|
| `Trigger` | PLC → Vision | Chi tiết đã vào đúng vị trí, yêu cầu vision chụp và kiểm tra |
| `Busy` | Vision → PLC | Vision đang xử lý — PLC không được trigger lần nữa cho đến khi `Busy` hạ |
| `Done` | Vision → PLC | Kết quả đã sẵn sàng để đọc (kèm `Result`, toạ độ nếu có) |
| `Ack` | PLC → Vision | PLC đã đọc xong kết quả — vision an toàn để dọn trạng thái, chờ trigger tiếp theo |

### 15.1.2 Sequence diagram: đường đi bình thường và đường đi khi lỗi

Chuỗi bình thường: PLC bật `Trigger` → vision **bật** `Busy` ngay khi nhận trigger (báo "đang
bận, đừng trigger nữa"), chạy `CogToolBlock.Run()` (Chương 13), rồi hạ `Busy` và bật `Done` kèm
dữ liệu kết quả. PLC đọc kết quả, bật `Ack`. Vision thấy `Ack`,
hạ `Done`, chờ `Trigger` hạ (PLC cũng hạ trigger sau khi thấy `Done`) rồi trở về trạng thái sẵn
sàng cho chu kỳ tiếp theo.

![Hình 15.1 — Trình tự handshake bình thường giữa PLC và Vision](../assets/ch15/hinh_15_1.png)
**Hình 15.1 — Trình tự handshake bình thường giữa PLC và Vision.**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): sequence diagram chuẩn (draw.io hoặc PlantUML), 2 cột dọc
> "PLC" và "Vision", mũi tên ngang theo đúng trình tự văn bản bên dưới (Trigger → Busy → Busy=0/
> Done=1 → Ack → Done=0 → Trigger=0), mỗi mũi tên có nhãn tín hiệu. Overlay khung nét đứt màu đỏ
> quanh từng "khoảng chờ" (PLC chờ Busy/Done; Vision chờ Ack) kèm nhãn "⏱ timeout riêng" — nhấn
> mạnh trực quan nguyên tắc mục 15.1.3. Có thể dùng lại làm khung cho Hình 15.2 (đường đi lỗi)
> nếu muốn vẽ thêm một biến thể ở chương sau.

Phiên bản văn bản rút gọn của cùng trình tự, dùng làm tham chiếu nhanh khi đọc code:

```text
PLC                          Vision
 |--- Trigger = 1 ----------->|
 |                             | (nhận trigger, bắt đầu xử lý)
 |<-------- Busy = 1 ----------|
 |                             | ... CogToolBlock.Run() (Chương 13) ...
 |<----- Busy=0, Done=1 -------|  (kèm Result, PartX, PartY... nếu OK)
 | (đọc kết quả)               |
 |--- Ack = 1 ---------------->|
 |<-------- Done = 0 ----------|
 |--- Trigger = 0 ------------>|
 |--- Ack = 0 ----------------->|
 | (sẵn sàng cho cycle sau)    | (sẵn sàng cho cycle sau)
```

> ⚠️ **Cảnh báo:** bước cuối `Ack = 0` không phải chi tiết vặt — thiếu nó, tín hiệu `Ack` treo ở
> mức 1 sau cycle đầu tiên. Sang cycle kế tiếp, ngay khi vision vừa bật `Done = 1`, nó lập tức
> "thấy" `Ack` đã là 1 (dư từ cycle trước) và hiểu nhầm là PLC đã đọc xong — hạ `Done` và dọn kết
> quả trước khi PLC kịp đọc giá trị mới. Bốn tín hiệu đều phải quay về trạng thái nghỉ (0) trước
> khi cycle tiếp theo bắt đầu — không chỉ ba tín hiệu `Busy`/`Done`/`Trigger`.

Đường đi khi có lỗi mới là phần hay bị bỏ sót lúc thiết kế — chính xác điều gây ra sự cố mở đầu
chương: **mỗi mũi tên chờ ở trên đều cần một giới hạn thời gian tối đa**, không riêng vision mà
cả PLC. Mục 15.1.3 đi vào chi tiết.

### 15.1.3 Timeout hai phía — không chỉ vision timeout, PLC cũng phải timeout

Chương 8 (mục 8.3) đã nhấn `TimeoutEnabled` cho PMAlign; nguyên tắc đó áp dụng ở tầng giao tiếp
với phạm vi rộng hơn: **cả vision và PLC đều phải tự đặt giới hạn chờ cho mọi tín hiệu từ phía
bên kia**, không riêng vision chờ PLC.

- **Vision chờ `Trigger`**: không giới hạn — đây là trạng thái nghỉ bình thường, chờ vô thời hạn
  là đúng thiết kế.
- **Vision xử lý sau khi nhận `Trigger`** (đến khi tự đặt được `Done`): giới hạn bởi ngân sách
  cycle time (mục 15.2) — vision tự dừng và báo lỗi timeout nội bộ nếu vượt quá.
- **Vision chờ `Ack` sau khi đặt `Done`**: đây chính là điểm gây sự cố mở đầu chương nếu không có
  giới hạn — PLC không đọc kết quả (crash, treo, mất kết nối) không được phép làm vision treo vô
  thời hạn. Đặt timeout, và khi hết hạn: log cảnh báo, raise alarm, tự dọn trạng thái để không
  chặn cycle tiếp theo mãi mãi.
- **PLC chờ `Busy`/`Done` sau khi đặt `Trigger`**: đối xứng với vế trên — PLC phải tự timeout nếu
  vision không phản hồi trong thời gian hợp lý, không phụ thuộc hoàn toàn vào vision "tự giác"
  báo lỗi.

> ⚠️ **Cảnh báo:** nguyên tắc "mỗi bên tự bảo vệ mình bằng timeout của chính mình" là bất biến an
> toàn quan trọng nhất của chương — đừng thiết kế theo kiểu "vision luôn phản hồi đúng lúc nên
> PLC không cần timeout". Phần cứng hỏng, cáp đứt, phần mềm treo là những sự kiện *sẽ* xảy ra
> trong vòng đời một trạm sản xuất nhiều năm — thiết kế phải giả định chúng xảy ra, không giả
> định chúng không xảy ra.

### 15.1.4 Một kênh hay nhiều kênh

Mục 15.1.1 nói bốn tín hiệu handshake giữ nguyên bản chất "dù truyền qua I/O rời rạc, thanh ghi
PLC, hay bản tin TCP" — đúng, nhưng không có nghĩa toàn bộ giao tiếp vision-PLC phải dồn vào **một**
kênh duy nhất. Hệ thống nhiều trạm/nhiều thiết bị thường tách hẳn hai loại giao tiếp có đặc tính
khác nhau:

- **Kênh bắt tay** (Trigger/Busy/Done/Ack, mục 15.1.1): cần độ trễ thấp, ổn định, và đúng thứ tự
  tuyệt đối — sai thứ tự dù một tín hiệu cũng đủ gây lỗi dây chuyền (cảnh báo ở mục 15.1.2). Phù
  hợp với I/O rời rạc hoặc một kết nối giữ trạng thái liên tục dành riêng cho việc này.
- **Kênh dữ liệu** (toạ độ chi tiết — mục 15.3, thông tin lô/sản phẩm, log kết quả): cần băng
  thông cao hơn, độ trễ vài chục/vài trăm mili-giây không thành vấn đề, và có thể dùng giao thức
  hướng bản tin (mỗi lần gửi độc lập, không cần giữ kết nối liên tục) như REST/gói tin UDP định kỳ.

Ép cả hai vào một kênh không sai về mặt logic — bốn tín hiệu vẫn hoạt động đúng dù gửi kèm toạ độ
chi tiết trong cùng gói tin. Nhưng tách riêng theo ĐẶC TÍNH cần (độ trễ thấp cho bắt tay, băng
thông cho dữ liệu) thường đơn giản hoá cả hai phía: kênh bắt tay không phải xử lý gói tin lớn làm
trễ tín hiệu thời gian thực; kênh dữ liệu không bị ràng buộc bởi yêu cầu độ trễ khắt khe của
handshake. Với một trạm đơn giản (chương này và MeoVision đang minh hoạ), một kênh là đủ — đáng
biết hướng tách khi hệ thống lớn dần, không cần áp dụng ngay từ đầu.

### 15.1.5 Giám sát tình trạng kết nối — khác với timeout từng lần

Timeout (mục 15.1.3) bảo vệ **một lần** chờ tín hiệu không kéo dài vô hạn. Nó không trả lời được
câu hỏi rộng hơn: **kênh giao tiếp có đang sống hay không**, ngay cả khi chưa có tín hiệu nào đang
chờ. Một cáp mạng bị rút giữa hai chu kỳ sản xuất (không phải giữa lúc đang chờ `Ack`) có thể không
kích hoạt bất kỳ timeout nào cho đến tận chu kỳ *tiếp theo*, khi `WaitForTriggerAsync` gọi ra không
bao giờ trả lời — mất một chu kỳ trọn vẹn chỉ để phát hiện ra sự cố đã xảy ra từ trước đó.

Khi bốn tín hiệu ở mục 15.1.1 truyền qua một phương tiện giữ kết nối liên tục (persistent socket
— ví dụ TCP, thay vì I/O rời rạc vốn không có khái niệm "mất kết nối" theo cùng nghĩa), phần hiện
thực nên tự giám sát tình trạng sống của chính kết nối đó, độc lập với chu kỳ sản xuất:

- **Thuộc tính `IsConnected`** đọc được bất cứ lúc nào, không chỉ suy luận gián tiếp từ việc lần
  gọi gần nhất có timeout hay không.
- **Sự kiện báo mất kết nối** (`ConnectionLost` hoặc tương đương) bắn ra ngay khi phát hiện, không
  đợi tới lần gọi kế tiếp mới lộ ra — tầng ứng dụng có thể nâng thành cảnh báo thật cho vận hành
  viên (ví dụ Chương 12, mục 12.2, đã minh hoạ cách raise một alarm cụ thể) và tạm dừng nhận
  trigger mới, thay vì để chu kỳ tiếp theo tự đâm vào timeout rồi mới biết.
- **Chính sách kết nối lại** khi phát hiện mất kết nối — thử lại có giới hạn số lần, giãn cách thời
  gian tăng dần giữa các lần thử (backoff) thay vì thử lại dồn dập liên tục, và log rõ mỗi lần thử
  (thất bại âm thầm, không log, là kịch bản khó chẩn đoán nhất khi sự cố xảy ra thật).

> 📌 **Lưu ý:** đây là phần mở rộng thực tế, chưa cần thiết cho ví dụ đơn giản của chương này (một
> phiên kết nối, một trạm). Đáng biết trước khi triển khai một trạm chạy liên tục nhiều ca/nhiều
> ngày không giám sát — nơi "mất kết nối lặng lẽ" gây thiệt hại lớn hơn nhiều so với một timeout
> đơn lẻ có log rõ ràng.

## 15.2 Ràng buộc thời gian: ngân sách cycle của vision

### 15.2.1 Đo, không đoán

Đúng tinh thần đã lặp lại xuyên suốt sách ("đo từ golden set, không đoán ngưỡng" — Chương 8, mục
8.5; Chương 9, mục 9.4), ngân sách thời gian cho vision không phải một con số chọn tuỳ ý — nó là
**cam kết đo được** dựa trên tổng thời gian thực tế của: acquisition (Chương 3, mục 3.3 — exposure
+ readout), và xử lý (`ICogRunStatus.ProcessingTime`/`TotalTime`, Chương 13, mục 13.2, đọc được
ngay sau mỗi `Run()`).

**Code 15.1 — Đo thời gian cycle thực tế, tách riêng phần xử lý VisionPro.**

```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

toolBlock.Run();

stopwatch.Stop();
double totalCycleMs = stopwatch.Elapsed.TotalMilliseconds;   // xử lý + overhead của Run() này
double visionProcessingMs = toolBlock.RunStatus.ProcessingTime;   // riêng phần xử lý VisionPro
// CẦN XÁC MINH TRÊN SDK THẬT: tên property chính xác trên ICogRunStatus (ProcessingTime hay
// TotalTime tuỳ phiên bản) — đối chiếu IntelliSense/tài liệu trước khi dùng trong code thật.

// Ghi lại phân bố thời gian qua nhiều cycle — không chỉ 1 lần đo — để biết
// giá trị trung bình VÀ giá trị đỉnh (worst-case), cả hai đều cần cho cam kết
_cycleTimeStats.Record(totalCycleMs);
```

> ⚠️ **Cảnh báo:** `totalCycleMs` ở trên chỉ đo đúng khoảng bọc quanh `toolBlock.Run()` — nó
> **không tự động bao gồm** thời gian acquisition nếu ảnh được thu riêng bằng `CogAcqFifoTool`
> từ code trước khi gán vào `Inputs["InputImage"]` (mô hình `StartAcquire`/`CompleteAcquire` ở
> Chương 13, mục 13.4). Trong mô hình đó, phải bọc Stopwatch quanh **toàn bộ** chuỗi thu ảnh +
> `Run()`, hoặc cộng riêng thời gian acquisition đo được vào `totalCycleMs` trước khi ghi vào
> `_cycleTimeStats` — nếu không, ngân sách cam kết với PLC (mục 15.2.2) sẽ thiếu hẳn phần
> acquisition, một nguồn timeout ngẫu nhiên khó chẩn đoán khác. Chỉ khi `CogAcqFifoTool` nằm
> **bên trong** chính `CogToolBlock` (mô hình QuickBuild gốc — Chương 6) thì `Run()` mới tự bao
> gồm acquisition.

### 15.2.2 Cam kết với máy: worst-case, không phải trung bình

Máy tự động vận hành theo nhịp cố định (takt time) — cam kết với đội cơ khí/PLC phải dựa trên
**thời gian xấu nhất đo được** (percentile cao, ví dụ P99, hoặc giá trị lớn nhất quan sát được
qua đủ nhiều cycle), không phải thời gian trung bình. Một trạm vision trung bình 80 ms nhưng thỉnh
thoảng đạt đỉnh 250 ms (do một cycle cần retry, hoặc GC chạy đúng lúc — Chương 13, mục 13.6.3)
mà máy tự động chỉ chờ 150 ms là công thức cho lỗi timeout ngẫu nhiên khó tái hiện trong sản xuất.

> 💡 **Mẹo thực chiến:** đo ngân sách cycle qua **hàng nghìn** cycle thực tế trước khi cam kết,
> không phải vài chục lần chạy thử trên bàn — điều kiện thực tế (nhiệt độ máy tăng dần, tải hệ
> thống thay đổi, GC) chỉ bộc lộ đầy đủ sau một khoảng thời gian vận hành đủ dài.

### 15.2.3 Khi vision chậm hơn máy: ba lựa chọn, không lựa chọn nào miễn phí

Khi đo được vision không đáp ứng nổi takt time thực tế của máy, ba hướng xử lý — không có hướng
nào "đúng tuyệt đối", chọn theo ràng buộc cụ thể của trạm:

1. **Giảm takt time máy** — chậm cả dây chuyền để vision kịp. Đơn giản nhất về kỹ thuật, tốn kém
   nhất về sản lượng.
2. **Tối ưu vision** — thu hẹp ROI, giảm phạm vi tìm kiếm PMAlign (Chương 8, mục 8.3), giảm độ
   phân giải nếu dung sai cho phép (Chương 3). Đòi hỏi hiểu sâu về tool đang dùng.
3. **Xử lý song song nhiều trạm/nhiều camera** — trong khi trạm A đang xử lý, trạm B đã sẵn sàng
   nhận chi tiết tiếp theo, bù lại thời gian xử lý bằng cách chồng lấp (pipeline). Đòi hỏi thiết
   kế cơ khí/quy trình phù hợp, không phải lúc nào cũng khả thi để thêm vào sau.

## 15.3 Gửi toạ độ cho robot

### 15.3.1 Xây trên nền đã có: hệ toạ độ chung + kiểm biên

Chương 7 (mục 7.5) đã giải quyết phần khó nhất của bài toán này — dạy calibration bằng chính các
điểm robot chạm để không gian calibrated *trùng* hệ toạ độ robot, và đã đưa ra khung `PickResultMessage`
với cờ `valid` + kiểm tra biên. Mục này mở rộng khung đó thành **bản tin hoàn chỉnh** gửi qua kênh
giao tiếp thực (TCP/IP, OPC UA — các giao thức quen thuộc ở tầng điều khiển máy tự động; cách
dùng ở tầng vision không có gì khác biệt):

**Code 15.2 — PickCommand: bản tin hoàn chỉnh gửi robot, mở rộng từ Chương 7.**

```csharp
public record PickCommand(
    bool Valid,           // false = KHÔNG được dùng toạ độ này, robot không di chuyển
    double X, double Y, double AngleDeg,
    string PartCode,       // từ CogIDTool (Chương 11) — gắn kèm để traceability đồng bộ
    int SequenceNumber);   // chống xử lý trùng bản tin cũ (mục 15.3.2)
```

### 15.3.2 Chống trùng lặp: SequenceNumber

Một bản tin bị gửi lại (do lỗi mạng, do PLC đọc chậm rồi đọc lại đúng lúc vision đã ghi đè giá trị
mới) có thể khiến robot dùng toạ độ **cũ** cho chi tiết **mới** — hai bản tin trông giống hệt nhau
về cấu trúc nhưng thuộc hai cycle khác nhau. `SequenceNumber` tăng dần mỗi cycle cho phép bên nhận
phát hiện và từ chối bản tin trùng/cũ trước khi hành động theo nó — một lớp bảo vệ rẻ nhưng thường
bị bỏ qua cho đến khi gặp sự cố thật.

### 15.3.3 Không có gì thay thế được kiểm tra phía robot

> ⚠️ **Cảnh báo — nhắc lại nguyên tắc từ Chương 7, mục 7.5 vì mức độ quan trọng:** vision tính
> toán và **đề xuất** toạ độ; robot controller/PLC **luôn** kiểm tra lại tính hợp lệ (biên vùng
> làm việc, `Valid` flag, `SequenceNumber`) trước khi di chuyển — không bao giờ tin tưởng tuyệt
> đối một bản tin đến từ mạng, dù nguồn gửi là hệ thống của chính mình. Đây là nguyên tắc phòng
> thủ theo lớp (defense in depth), không phải sự thiếu tin tưởng vào vision.

## 15.4 Kết quả NG thì máy làm gì

### 15.4.1 Vision đề xuất, PLC quyết định — áp dụng cho cả nhánh lỗi

Nguyên tắc "vision không ra lệnh, chỉ đề xuất" (Chương 7, mục 7.5) áp dụng đối xứng cho trường
hợp NG: vision báo cáo **loại** kết quả (OK / NG-đo-hỏng / NG-không-tìm-thấy-chi-tiết / Error-hệ-
thống — phân biệt rõ bốn loại này, không gộp chung thành một cờ NG duy nhất, đúng tinh thần bốn
mức `CogToolResultConstants` đã gặp ở Chương 12, mục 12.3.3), còn **quyết định làm gì** với kết
quả đó — dừng máy, đẩy chi tiết sang trạm reject, hay chỉ đánh dấu và tiếp tục — là quyết định
quy trình sản xuất, thuộc thẩm quyền PLC/MES, không phải vision.

**Bảng 15.2 — Phân loại kết quả và hành động điển hình (do PLC quyết định cuối cùng).**

| Loại kết quả | Ý nghĩa | Hành động điển hình |
|---|---|---|
| Accept | Đạt mọi tiêu chí | Cho qua |
| Warning | Đạt nhưng sát ngưỡng (Chương 12, mục 12.3.3) | Cho qua, ghi log để giám sát xu hướng (Chương 16) |
| Reject (NG) | Không đạt tiêu chí đo lường/kiểm tra | Đẩy sang trạm reject, KHÔNG dừng máy |
| Error (hệ thống) | Vision không đọc được kết quả tin cậy (không tìm thấy chi tiết, timeout nội bộ, exception) | Tuỳ chính sách: dừng chờ can thiệp, hoặc reject + cảnh báo nếu tần suất bất thường |

### 15.4.2 Vì sao tách NG-đo-hỏng khỏi NG-không-tìm-thấy

Hai loại thất bại có ý nghĩa vận hành rất khác nhau: **NG-đo-hỏng** (đo được, chi tiết thật sự
lỗi) là tín hiệu về *chất lượng sản phẩm* — đáng để theo dõi tỉ lệ, báo cáo QA. **NG-không-tìm-
thấy** (PMAlign không định vị được — Chương 8, mục 8.4; hoặc caliper không tìm thấy biên — Chương
9) là tín hiệu về *sức khoẻ hệ thống vision* — ánh sáng trôi, camera lệch nét (Chương 12, mục
12.1.3), hay chi tiết đặt sai hoàn toàn ngoài phạm vi fixturing (Chương 7). Gộp hai loại vào một
con số NG duy nhất che mất sự khác biệt quan trọng này — một xu hướng NG-không-tìm-thấy tăng dần
cần điều tra vision, trong khi NG-đo-hỏng tăng dần cần điều tra quy trình gia công phía trước.

## 15.5 Nhiều kết quả một cycle, retry có giới hạn

### 15.5.1 Multi-cavity: một ảnh, nhiều chi tiết

Một số trạm kiểm tra nhiều chi tiết cùng lúc trong một khung hình (khuôn nhiều lòng khuôn —
multi-cavity). Bản tin gửi PLC/robot khi đó cần mở rộng từ một bộ toạ độ đơn thành một **mảng**
kết quả, mỗi phần tử mang đủ thông tin như mục 15.3.1 (bao gồm cả `Valid` riêng cho từng phần tử
— một cavity trống hoặc lỗi không được làm hỏng kết quả của các cavity còn lại trong cùng ảnh).

### 15.5.2 Retry chụp lại — có giới hạn, không phải cố mãi

Khi một cycle không tìm thấy chi tiết (có thể do rung động tức thời, phản xạ bất thường một lần),
retry — chụp lại và thử lại — là hợp lý **với điều kiện có giới hạn số lần rõ ràng** (ví dụ tối
đa 2 lần retry) và **tính vào ngân sách cycle time** (mục 15.2) chứ không phải một vòng lặp "thử
đến khi thành công". Retry vô hạn không giới hạn biến một lỗi thật (chi tiết thực sự không có,
camera thực sự hỏng) thành một cycle treo vô thời hạn — đúng loại sự cố đã mở đầu chương này,
chỉ chuyển từ tầng giao tiếp sang tầng logic retry nội bộ.

> ⚠️ **Cảnh báo — "tính vào ngân sách cycle time" cần một con số, không chỉ một nguyên tắc:**
> worst-case cycle time khi có retry KHÔNG phải worst-case của một lần chạy đơn — nó nhân lên
> theo số lần thử: `Worst_case_với_retry = (maxRetries + 1) × Worst_case_một_lần_chạy`. Ví dụ:
> một lần chạy worst-case đo được 110 ms (30 ms acquisition + 80 ms xử lý), `maxRetries = 2` →
> worst-case thật của cả cycle là `3 × 110 ms = 330 ms`, không phải 110 ms. Nếu cam kết timeout
> với PLC (mục 15.2.2) chỉ dựa trên worst-case một lần chạy mà quên nhân với `(maxRetries + 1)`,
> PLC sẽ timeout và dừng máy đúng vào lúc vision đang retry hợp lệ — biến cơ chế retry (được
> thiết kế để tăng độ ổn định) thành nguyên nhân gây lỗi timeout giả.

**Code 15.3 — Retry có giới hạn khi không tìm thấy chi tiết.**

```csharp
const int maxRetries = 2;   // tổng tối đa maxRetries + 1 = 3 lần chạy (1 lần đầu + 2 lần retry)
VisionResult? result = null;

for (int attempt = 0; attempt <= maxRetries; attempt++)
{
    result = await _visionEngine.InspectAsync(request, ct).ConfigureAwait(false);
    if (result.FailureReason != VisionFailureReason.PartNotFound)
        break;   // Accept/Warning, hoặc Reject loại khác không đáng retry (mục 15.4.2)

    if (attempt < maxRetries)   // còn lượt retry tiếp theo mới log là "sắp thử lại"
        logger.LogWarning("Không tìm thấy chi tiết, thử lại lần {Attempt}", attempt + 1);
}
```

## Tổng kết chương

- Handshake vision-PLC tối thiểu bốn tín hiệu (`Trigger`/`Busy`/`Done`/`Ack`); **mọi** điểm chờ ở
  cả hai phía đều cần timeout riêng — không giả định phía bên kia luôn phản hồi đúng lúc. Hệ thống
  lớn dần có thể tách kênh bắt tay (độ trễ thấp) khỏi kênh dữ liệu (băng thông cao); phương tiện
  giữ kết nối liên tục nên tự giám sát tình trạng sống, không chỉ dựa vào timeout từng lần gọi.
- Ngân sách cycle time là con số đo được qua hàng nghìn cycle thực tế (`ICogRunStatus.ProcessingTime`),
  cam kết dựa trên worst-case (P99/max), không phải trung bình. Ba hướng khi vision không kịp
  takt time: giảm takt time, tối ưu vision, hoặc xử lý song song nhiều trạm.
- Bản tin gửi robot cần cờ hợp lệ (`Valid`) và số thứ tự chống trùng (`SequenceNumber`); robot/PLC
  luôn tự kiểm tra lại, không bao giờ tin tưởng tuyệt đối một bản tin từ mạng.
- Vision đề xuất, PLC quyết định — áp dụng cho cả nhánh lỗi: phân biệt Accept/Warning/Reject/Error
  thay vì một cờ NG duy nhất; tách riêng NG-đo-hỏng (vấn đề chất lượng sản phẩm) khỏi NG-không-
  tìm-thấy (vấn đề sức khoẻ hệ thống vision) vì hai loại đòi hỏi hướng điều tra khác nhau.
- Multi-cavity mở rộng bản tin thành mảng kết quả, mỗi phần tử có cờ hợp lệ riêng. Retry chụp lại
  chỉ hợp lý khi có giới hạn số lần rõ ràng và tính vào ngân sách cycle — không bao giờ retry vô hạn.

## Lỗi thường gặp

**Lỗi 1 — Không có timeout phía PLC khi vision treo.** Hiện tượng: dây chuyền đứng im vô thời
hạn khi vision gặp sự cố (mất kết nối camera, phần mềm treo), không có cảnh báo nào — đúng tình
huống mở đầu chương. Nguyên nhân: thiết kế chỉ đặt timeout phía vision, giả định PLC "chờ vô hạn
là bình thường". Cách tránh: cả hai phía đều tự timeout mọi điểm chờ tín hiệu từ bên kia (mục 15.1.3).

**Lỗi 2 — Cam kết cycle time theo trung bình thay vì worst-case.** Hiện tượng: lỗi timeout ngẫu
nhiên, khó tái hiện, xuất hiện thưa thớt trong sản xuất dù test trên bàn không thấy. Nguyên nhân:
đo và cam kết dựa trên giá trị trung bình, bỏ qua phân bố đuôi dài (GC, retry, tải hệ thống). Cách
tránh: đo qua hàng nghìn cycle, cam kết theo P99/giá trị lớn nhất quan sát được (mục 15.2.2).

**Lỗi 3 — Gửi toạ độ không kèm cờ hợp lệ hoặc không kiểm tra biên.** Hiện tượng: robot di chuyển
đến vị trí vô nghĩa khi vision không tìm thấy chi tiết nhưng vẫn gửi toạ độ mặc định/rác. Nguyên
nhân: bỏ qua bước đóng gói `Valid` + kiểm biên trước khi gửi (đã cảnh báo từ Chương 7, mục 7.5).
Cách tránh: luôn kiểm tra biên + đặt `Valid = false` tường minh khi không có kết quả tin cậy, và
robot/PLC luôn tự kiểm tra lại phía nhận (mục 15.3.3).

**Lỗi 4 — Mất kết nối giữa hai chu kỳ, chỉ phát hiện được ở chu kỳ tiếp theo.** Hiện tượng: cáp
mạng đứt hoặc tiến trình phía bên kia crash trong lúc không có tín hiệu nào đang chờ — không
timeout nào kích hoạt ngay; sự cố chỉ lộ ra khi chu kỳ tiếp theo bắt đầu và treo ở bước chờ đầu
tiên, muộn hơn thời điểm sự cố thật xảy ra một khoảng không xác định. Nguyên nhân: chỉ dựa vào
timeout của từng lần chờ tín hiệu (mục 15.1.3) để phát hiện sự cố, không giám sát tình trạng sống
của bản thân kết nối. Cách tránh: với phương tiện truyền giữ kết nối liên tục, tự theo dõi
`IsConnected`/sự kiện mất kết nối độc lập với chu kỳ sản xuất, nâng thành cảnh báo ngay khi phát
hiện thay vì chờ chu kỳ sau tự đâm vào timeout (mục 15.1.5).

**Lỗi 4 — Gộp mọi loại thất bại thành một cờ NG duy nhất.** Hiện tượng: không thể phân biệt xu
hướng "chất lượng sản phẩm giảm" với xu hướng "vision đang xuống cấp" chỉ từ tỉ lệ NG tổng. Nguyên
nhân: thiết kế bản tin kết quả quá đơn giản ngay từ đầu. Cách tránh: phân loại rõ Accept/Warning/
Reject/Error và tách NG-đo-hỏng khỏi NG-không-tìm-thấy (mục 15.4.2) — sửa lại cấu trúc bản tin
sớm rẻ hơn nhiều so với sửa sau khi đã tích luỹ dữ liệu lịch sử không phân loại được.

**Lỗi 5 — Retry vô hạn khi không tìm thấy chi tiết.** Hiện tượng: một cycle "treo" bất thường lâu
so với bình thường, làm lệch nhịp dây chuyền dù không có lỗi nào được báo rõ ràng. Nguyên nhân:
vòng lặp retry không có giới hạn số lần, cố chụp lại đến khi thành công. Cách tránh: giới hạn số
lần retry rõ ràng, tính thời gian retry vào ngân sách cycle, hết giới hạn thì báo lỗi tường minh
thay vì tiếp tục thử (mục 15.5.2).

\newpage

# Chương 16 — Độ tin cậy, nghiệm thu và deep learning

Trạm MeoVision được nghiệm thu bằng mười chi tiết mẫu, chọn lọc kỹ càng, chạy mười lần đều cho
kết quả OK tuyệt đối. Khách hàng ký nghiệm thu. Ba tuần sau, tỉ lệ NG giả (false reject) leo lên
gần 2% — không có gì "hỏng", chỉ là mười chi tiết mẫu đó chưa từng đại diện cho toàn bộ biến thiên
thực tế của cả một lô sản xuất kéo dài hàng tháng: ánh sáng ca đêm khác ca ngày, lô nguyên liệu
thứ hai có độ bóng khác lô đầu, và một vài chi tiết nằm đúng ở rìa dung sai mà mười mẫu "đẹp" ban
đầu không hề chạm tới.

Đây là khoảng cách giữa **"chạy được"** và **"chạy ổn định"** — chủ đề của chương cuối cùng trong
Phần V. Không có kỹ thuật mới nào ở đây: mọi công cụ đã học đủ (PMAlign, Caliper, Blob — Phần III;
kiến trúc ứng dụng — Chương 14; giao tiếp máy — Chương 15). Điều chương này bổ sung là **phương
pháp** — cách đặt ngưỡng có cơ sở, cách nghiệm thu đúng nghĩa, cách vận hành lâu dài không để một
trạm "chạy được hôm khai trương" âm thầm xuống cấp thành "chạy sai mà không ai biết" — và một cái
nhìn khái quát về deep learning, công cụ dành cho đúng lớp bài toán mà rule-based không giải nổi.

## 16.1 Vì sao "chạy được" khác "chạy ổn định"

### 16.1.1 Ba nguồn biến thiên không xuất hiện trong bài demo

Ba nguồn biến thiên sau đây gần như luôn vắng mặt trong một buổi demo/nghiệm thu vội vàng, nhưng
luôn xuất hiện trong vận hành thực tế kéo dài:

- **Drift ánh sáng** — đèn LED xuống cấp theo thời gian sử dụng, bụi bám ống kính, ánh sáng môi
  trường thay đổi theo ca/theo mùa (Chương 2). Đây chính là nguồn dữ liệu mà `CogHistogramTool`
  (Chương 12, mục 12.2) và `CogImageSharpnessTool` (Chương 12, mục 12.1.3) được thiết kế để phát
  hiện sớm.
- **Dao động chi tiết** — dung sai gia công thực tế trải rộng hơn "chi tiết mẫu đẹp"; một trạm chỉ
  từng thấy chi tiết ở giữa dải dung sai sẽ phản ứng bất ngờ khi gặp chi tiết ở rìa dải — dù chi
  tiết đó vẫn hợp lệ.
- **Thay đổi lô nguyên liệu** — đúng tình huống mở Chương 8 (score PMAlign tụt khi đổi lô nhôm):
  bề mặt, độ bóng, độ phản xạ khác nhau giữa các lô là chuyện xảy ra định kỳ, không phải ngoại lệ.

### 16.1.2 Bài demo chứng minh "có thể chạy", không chứng minh "sẽ chạy ổn định"

Sự khác biệt cốt lõi: một demo chạy N lần liên tiếp trên **cùng một bộ chi tiết** chứng minh tính
**lặp lại** (repeatability — đã định nghĩa ở Chương 9, mục 9.4) trong điều kiện hẹp đó, nhưng
không chứng minh gì về hành vi khi gặp biến thiên thật của sản xuất dài hạn. Đây là lý do mục
16.3 (nghiệm thu) và mục 16.4 (vận hành) tách biệt thành hai giai đoạn rõ ràng — nghiệm thu một
lần không thay thế được giám sát liên tục.

## 16.2 Đặt threshold có phương pháp

### 16.2.1 Hai loại sai, không đối xứng về hậu quả

Mọi ngưỡng trong sách này — `AcceptThreshold` của PMAlign (Chương 8), dải dung sai của Caliper
(Chương 9), ngưỡng diện tích Blob (Chương 10) — đều đối mặt cùng một đánh đổi cơ bản:

**Bảng 16.1 — Hai loại sai khi đặt ngưỡng, và hậu quả điển hình.**

| Loại sai | Tên gọi | Hậu quả |
|---|---|---|
| Ngưỡng quá **lỏng** | False Accept (chấp nhận sai) | Hàng lỗi lọt qua kiểm tra — hậu quả tới tay khách hàng, thường nghiêm trọng và phát hiện muộn |
| Ngưỡng quá **chặt** | False Reject (từ chối sai) | Hàng tốt bị loại oan — tốn chi phí xử lý lại, giảm sản lượng, nhưng phát hiện ngay tại chỗ |

Hai loại sai này **không đối xứng về mức độ nghiêm trọng** trong đa số ngành: false accept trong
ngành y tế/ô tô có thể là sự cố an toàn; false reject trong hàng tiêu dùng "chỉ" là chi phí vận
hành. Ngưỡng đúng không phải ngưỡng "cân bằng hai loại sai" một cách máy móc — nó phản ánh **cái
giá tương đối** của từng loại sai đối với sản phẩm và ngành cụ thể, một quyết định kinh doanh/kỹ
thuật phối hợp, không phải một công thức thuần toán học.

### 16.2.2 Đo phân bố thật, không đoán một con số

Đúng nguyên tắc lặp lại xuyên suốt sách: thu thập **phân bố score/giá trị đo của cả hai nhóm** —
chi tiết hợp lệ đã biết và chi tiết lỗi đã biết (golden set — mục 16.3) — rồi quan sát:

- Nếu hai phân bố **tách biệt rõ ràng** (không chồng lấn, hoặc chồng lấn rất ít), một ngưỡng đặt
  giữa khoảng trống đó cho cả hai loại sai gần bằng không — trường hợp lý tưởng.
- Nếu hai phân bố **chồng lấn đáng kể**, không có ngưỡng nào loại bỏ hoàn toàn cả hai loại sai —
  đây là tín hiệu vấn đề nằm ở **tầng thu ảnh/đo lường** (ánh sáng, độ phân giải, độ nhiễu — Phần
  I-II), không phải vấn đề "chưa tìm đúng ngưỡng". Cố tinh chỉnh threshold khi hai phân bố chồng
  lấn nhiều là tối ưu sai tầng — cải thiện đáng kể chỉ đến từ việc quay lại các chương trước.

![Hình 16.1 — Phân bố score OK/NG tách biệt (trái) và chồng lấn (phải)](../assets/ch16/hinh_16_1.png)
**Hình 16.1 — Phân bố score OK/NG tách biệt (trái) và chồng lấn (phải).**
> 🖼 MÔ TẢ HÌNH (cho tác giả tạo): 2 biểu đồ histogram/mật độ đặt cạnh nhau, trục X là score
> (0-1), trục Y là số lượng mẫu. Trái: 2 đường cong mật độ tách biệt rõ (xanh = golden set OK,
> đỏ = golden set NG), có khoảng trống ở giữa, vạch đứng nét đứt đánh dấu "threshold đặt ở đây"
> nằm gọn trong khoảng trống, chú thích "False Accept ≈ 0, False Reject ≈ 0". Phải: 2 đường cong
> cùng màu nhưng chồng lấn đáng kể ở giữa, vùng chồng lấn tô màu tím nhạt, chú thích "Không có
> threshold nào loại bỏ hết cả 2 loại sai — quay lại Phần I-II thay vì chỉnh số". Dựng bằng
> Excel/Python matplotlib từ dữ liệu score thật của trạm khi có, hoặc số liệu minh hoạ hợp lý.

**Code 16.1 — Minh hoạ khái niệm: so sánh phân bố score để quyết định threshold hoặc điều tra tiếp.**

```csharp
// Minh hoạ khái niệm cho trường hợp "điểm càng cao càng tốt, một phía" (ví dụ score PMAlign,
// Chương 8) — thu thập phân bố score qua golden set để CHỌN ngưỡng, không phải để dùng trực
// tiếp trong vòng lặp sản xuất
var okScores = goldenSetOk.Select(img => RunAndGetScore(img)).ToList();
var ngScores = goldenSetNg.Select(img => RunAndGetScore(img)).ToList();

double minOk = okScores.Min();
double maxNg = ngScores.Max();

if (minOk > maxNg)
{
    // Phân bố tách biệt — đặt ngưỡng ở giữa khoảng trống, ví dụ:
    double threshold = (minOk + maxNg) / 2.0;
}
else
{
    // Chồng lấn — KHÔNG cố tìm ngưỡng "tốt nhất trong tình huống xấu";
    // quay lại điều tra tầng ánh sáng/quang học/đo lường trước
}
```

> ⚠️ **Cảnh báo — phạm vi của ví dụ trên:** so sánh `minOk > maxNg` chỉ đúng nguyên xi cho ngưỡng
> **một phía, giá trị càng cao càng tốt** (đúng dạng score PMAlign — Chương 8). Áp dụng máy móc
> cho Blob (diện tích khuyết tật, thường **càng thấp càng tốt** — chi tiết sạch gần bằng 0, chi
> tiết lỗi cao hơn hẳn — chiều so sánh phải đảo ngược) hoặc Caliper (dung sai **hai phía**, giá
> trị tốt nằm ở giữa một khoảng chứ không phải "cao hơn" hay "thấp hơn" một ngưỡng đơn) sẽ cho
> logic sai. Nguyên tắc chung ở mục 16.2.2 (đo phân bố, không đoán ngưỡng) áp dụng cho cả ba loại
> tool; cách so sánh cụ thể trong code phải viết lại theo đúng chiều "tốt/xấu" của từng loại giá
> trị đo, không copy nguyên văn ví dụ này.

## 16.3 Nghiệm thu hệ vision

### 16.3.1 Golden set: bộ ảnh thử nghiệm phải đại diện, không phải "đẹp"

**Golden set** — bộ ảnh/chi tiết chuẩn dùng để nghiệm thu và làm cơ sở tinh chỉnh ngưỡng — phải
thoả hai tiêu chí thường bị bỏ qua vì sự tiện lợi của "lấy vài chi tiết đẹp trên bàn":

- **Đủ số lượng** để phân bố score/đo lường có ý nghĩa thống kê — con số cụ thể tuỳ ngành và mức
  rủi ro chấp nhận được, nhưng mười chi tiết (đúng tình huống mở chương) gần như luôn là quá ít.
- **Đủ đa dạng**: trải hết dải dung sai (không chỉ chi tiết "đẹp giữa dải"), nhiều lô nguyên liệu
  nếu có thể, nhiều điều kiện ánh sáng thực tế (ca ngày/ca đêm nếu có khác biệt), và **quan
  trọng nhất** — có cả chi tiết lỗi thật (không chỉ chi tiết tốt) để đo được phân bố cả hai phía
  như mục 16.2.2 yêu cầu.

> ⚠️ **Cảnh báo:** golden set chỉ toàn chi tiết tốt không đo được false reject rate — chỉ đo được
> "trạm có nhận ra hàng tốt không", không đo được "trạm có phân biệt được tốt/xấu không". Nghiệm
> thu chỉ bằng chi tiết tốt là nghiệm thu **không đầy đủ**, dù kết quả 100% OK trông rất thuyết
> phục trên giấy.

### 16.3.2 Chạy thống kê, không chạy một lần

Nghiệm thu chạy **toàn bộ golden set nhiều lần** (không chỉ một lượt), ghi lại phân bố kết quả —
tỉ lệ Accept/Warning/Reject/Error (Chương 12, mục 12.3.3; Chương 15, mục 15.4), phân bố score,
phân bố thời gian xử lý (Chương 15, mục 15.2). Một lần chạy hoàn hảo không loại trừ khả năng dao
động ngẫu nhiên (rung động, nhiễu cảm biến) khiến lần chạy thứ hai cho kết quả khác — chính là
khái niệm repeatability (Chương 9) áp dụng ở quy mô toàn hệ thống. Đi xa hơn một bước, chạy lại bộ
thử nghiệm này ở **điều kiện có thay đổi** — sau khi khởi động lại ứng dụng, vào một ca khác, sau
khi tháo lắp lại chi tiết mẫu thay vì để nguyên — kiểm tra **reproducibility** (Chương 9, mục
9.4.3): loại rủi ro thường gặp trong sản xuất thật hơn hẳn kịch bản "đo liên tục không gián đoạn"
của repeatability thuần tuý.

### 16.3.3 Tiêu chí bàn giao: viết thành văn bản, thống nhất trước khi chạy thử

Tiêu chí "đạt nghiệm thu" cần được **thống nhất bằng văn bản trước** khi chạy thử nghiệm thu, không
phải quyết định sau khi đã thấy kết quả (tránh thiên vị xác nhận — confirmation bias). Ví dụ cấu
trúc tiêu chí: tỉ lệ Accept đúng trên golden set OK ≥ X%, tỉ lệ Reject đúng trên golden set NG ≥
Y%, cycle time P99 ≤ Z ms (Chương 15, mục 15.2), không có Error hệ thống nào trong toàn bộ lượt
chạy thử. Phụ lục B của sách cung cấp một checklist đầy đủ hơn cho việc này.

## 16.4 Vận hành: giám sát và retrain có kiểm soát

### 16.4.1 Giám sát score theo thời gian — biến dữ liệu mỗi cycle thành xu hướng

Mỗi cycle sản xuất đã tự nhiên sinh ra dữ liệu quý giá: score PMAlign, kết quả đo Caliper, chỉ số
ánh sáng (Chương 12, mục 12.2), thời gian xử lý. Phần lớn trạm vision **bỏ phí** dữ liệu này —
chỉ dùng nó để quyết định OK/NG tức thời rồi vứt đi. Ghi lại chuỗi thời gian của các con số đó
(kể cả với chi tiết Accept, không chỉ Reject — khác nguyên tắc "chỉ lưu ảnh NG" ở Chương 14, mục
14.5: đây là **số liệu**, nhẹ hơn ảnh nhiều bậc, lưu được lâu dài với chi phí thấp) biến vision
từ một bộ lọc nhị phân thành một **cảm biến sức khoẻ liên tục** của cả trạm.

**Code 16.2 — Ghi lại metric của mọi cycle (không chỉ NG) để phân tích xu hướng.**

```csharp
// Ghi lại MỌI cycle (không chỉ NG) — số liệu, không phải ảnh — để phân tích xu hướng
_metricsStore.Record(new CycleMetrics(
    Timestamp: DateTime.UtcNow,
    PMAlignScore: result.Score,
    LightingMean: lightingCheck.Mean,        // Chương 12, mục 12.2
    Sharpness: sharpnessCheck.Score,          // Chương 12, mục 12.1.3
    ProcessingTimeMs: toolBlock.RunStatus.ProcessingTime));
```

### 16.4.2 Cảnh báo sớm: xu hướng, không chỉ ngưỡng tức thời

Một score 0.68 đơn lẻ (trên ngưỡng chấp nhận 0.65) không đáng báo động. Score trung bình **giảm
dần đều** từ 0.90 xuống 0.70 qua hai tuần — vẫn luôn trên ngưỡng chấp nhận ở mọi thời điểm, nên
không cycle đơn lẻ nào kích hoạt cảnh báo — là dấu hiệu xuống cấp cần điều tra **trước khi** nó
chạm ngưỡng và bắt đầu gây Reject hàng loạt. Đây chính là giá trị của mức `Warning` (Chương 12,
mục 12.3.3) và của việc theo dõi **đường xu hướng**, không chỉ so sánh từng điểm dữ liệu với một
ngưỡng cố định.

> 💡 **Mẹo thực chiến:** một cảnh báo đơn giản nhưng hiệu quả cao — so sánh trung bình trượt (ví
> dụ trung bình 100 cycle gần nhất) với trung bình của cùng chỉ số đo lúc mới nghiệm thu (mục
> 16.3). Lệch quá một ngưỡng phần trăm nhất định là tín hiệu đáng điều tra, không cần mô hình
> thống kê phức tạp để bắt đầu.

### 16.4.3 Retrain có kiểm soát — quy trình, không phải phản xạ

Khi giám sát phát hiện xuống cấp thật (không phải do ánh sáng/cơ khí có thể sửa trực tiếp — đã
bàn thứ tự điều tra ở Chương 8, mục 8.5), retrain pattern qua màn teach (Chương 14, mục 14.4) đi
theo đúng quy trình đã thiết kế: backup job cũ, teach trên mẫu đại diện lô mới, **chạy thử trên
golden set** (không chỉ vài chi tiết vừa teach) trước khi thay thế job đang chạy sản xuất — về
bản chất là lặp lại quy trình nghiệm thu ở quy mô nhỏ hơn, mỗi lần retrain.

### 16.4.4 Bốn loại trôi cần tài liệu hoá trước khi hiệu chỉnh lại

Retrain pattern (mục 16.4.3) chỉ là một trong nhiều hình thức "hiệu chỉnh lại" — trạm còn có thể
cần chỉnh calibration (Chương 7), chỉnh lại ánh sáng, hay đơn giản là xác nhận độ trôi vẫn trong
giới hạn chấp nhận được. Trước khi chạm vào bất kỳ tham số nào, việc đầu tiên đáng làm là **xác
định trôi thuộc loại nào** — bốn loại sau bao trùm phần lớn tình huống thực tế và mỗi loại trỏ
đến một nguyên nhân/cách sửa khác nhau:

**Bảng 16.2 — Bốn loại trôi thường gặp và hướng điều tra tương ứng.**

| Loại trôi | Biểu hiện | Nguyên nhân thường gặp |
|---|---|---|
| **Cường độ** (Intensity) | Score/độ tương phản giảm dần đều dù chi tiết không đổi | Đèn LED lão hoá (Chương 2, mục 2.2.7 — nguyên nhân *có khả năng nhất* khi các yếu tố khác đã loại trừ), bụi bám ống kính, ánh sáng môi trường đổi theo mùa/ca |
| **Tịnh tiến** (Translation) | Toạ độ X/Y kết quả lệch dần một chiều nhất quán qua nhiều cycle | Camera/giá đỡ xê dịch cơ khí do rung động, va chạm nhẹ khi bảo trì |
| **Xoay** (Rotation) | Góc kết quả lệch dần, không do chi tiết thật xoay | Camera xoay nhẹ trong giá đỡ, siết vít không đủ chặt |
| **Tỉ lệ** (Scaling) | Kích thước đo được lệch dần dù chi tiết không đổi kích thước thật | Camera xê dịch theo trục Z (working distance đổi), ống kính lỏng vòng lấy nét |

Bốn loại này không loại trừ lẫn nhau — một va chạm cơ khí có thể gây cả tịnh tiến lẫn xoay cùng
lúc. Nhưng phân loại được đúng loại trôi thu hẹp đáng kể phạm vi điều tra: trôi cường độ hiếm khi
cần đụng đến cơ khí; trôi tịnh tiến/xoay/tỉ lệ hiếm khi do đèn lão hoá.

> 📌 **Lưu ý:** trước khi hiệu chỉnh lại bất kỳ tham số nào, ghi lại **trạng thái hiện tại** làm
> mốc so sánh — working distance và góc giữa camera/đèn/chi tiết, khẩu độ lens, cấu hình strobe
> (Chương 6, mục 6.2.5), exposure/gain đang dùng, và một ảnh tham chiếu chụp ngay tại thời điểm đó.
> Không có mốc này, lần trôi tiếp theo không có gì để so sánh — "trạng thái lúc mới lắp đặt" trở
> thành thông tin không thể khôi phục được sau khi đã chỉnh sửa vài lần. Phụ lục B tổng hợp
> checklist đầy đủ cho việc ghi mốc này.

### 16.4.5 Gói chẩn đoán: chuẩn bị sẵn dữ liệu để xin hỗ trợ

Không phải bất thường nào vận hành viên tại chỗ cũng tự xử lý được — có lúc cần báo lên kỹ sư
vision phụ trách hoặc bộ phận hỗ trợ kỹ thuật, thường không có mặt tại hiện trường. Cách phổ biến
nhất (chụp vài màn hình rời rạc, mô tả triệu chứng qua điện thoại, gửi email đính kèm vài file lẻ)
làm mất đúng thứ quan trọng nhất để chẩn đoán từ xa: **bối cảnh đầy đủ tại thời điểm xảy ra lỗi** —
recipe đang chạy, vài chục cycle gần nhất, ảnh gây lỗi. Đến khi kỹ sư hỗ trợ nhận được thông tin,
ngữ cảnh đã rời rạc hoặc mất hẳn, và một vòng hỏi-đáp qua lại để xin thêm dữ liệu bắt đầu — tốn
đúng thời gian mà tính năng này nên tiết kiệm.

Thiết kế "xuất gói chẩn đoán" như một **tính năng có chủ đích**, không phải việc để vận hành viên
tự xoay xở, giải quyết đúng vấn đề đó — đóng gói mọi thứ cần cho chẩn đoán từ xa vào một hành động
duy nhất:

- **Mô tả sự cố bằng lời** — trường nhập tự do để vận hành viên ghi lại triệu chứng theo cách họ
  quan sát được. Log ghi đủ số liệu nhưng không ghi được "máy có tiếng động lạ trước khi lỗi xuất
  hiện" — hai loại thông tin bổ sung cho nhau, không thay thế được nhau.
- **Recipe/config đang chạy tại thời điểm đó** — mặc định chỉ recipe liên quan, không phải toàn bộ
  danh mục recipe của mọi sản phẩm. Gói hết "phòng khi cần" chỉ tạo ra file khổng lồ, khó gửi và
  khó đọc, không giúp chẩn đoán nhanh hơn.
- **Lịch sử log trong một khoảng giới hạn** — vài chục cycle gần nhất hoặc vài giờ, không phải
  toàn bộ lịch sử vận hành từ lúc lắp máy. Giới hạn cứng (theo số cycle hoặc số ngày) là chủ đích:
  không ai đọc hết một file log vô hạn, và file quá lớn tự nó cản trở việc gửi đi.
- **Ảnh liên quan, có phạm vi rõ ràng** — ảnh gây lỗi chắc chắn cần; ảnh dùng để teach recipe đó
  thì để tuỳ chọn (hữu ích khi cần so sánh "mẫu lúc teach" với "chi tiết lúc lỗi", nhưng không phải
  lúc nào cũng cần, và ảnh vốn nặng hơn số liệu rất nhiều bậc — nguyên tắc đã nói ở mục 16.4.1).

Điểm thiết kế quan trọng nhất không phải là gói được **bao nhiêu**, mà là để vận hành viên **kiểm
soát được phạm vi** trước khi xuất: mặc định hẹp (chỉ recipe hiện hành + log gần nhất + ảnh lỗi),
mở rộng khi thật sự cần (toàn bộ danh mục recipe, toàn bộ ảnh đã teach). Không có phạm vi mặc định
hẹp, tính năng "gói chẩn đoán" tự nó trở thành nguồn tạo ra file khổng lồ không ai gửi/mở nổi —
phản tác dụng so với mục đích ban đầu.

**Code 16.3 — Minh hoạ khái niệm: yêu cầu xuất gói chẩn đoán, phạm vi kiểm soát được từ đầu.**

```csharp
// Minh hoạ khái niệm — thiết kế hợp lý cho tính năng "gói chẩn đoán", chưa phải interface đã có
// sẵn trong MeoVision.Application. Mọi cờ include đều mặc định false/hẹp — mở rộng phạm vi là
// lựa chọn tường minh của người xuất, không phải hành vi ngầm định.
public sealed record DiagnosticBundleRequest(
    string OperatorNote,
    bool IncludeFullRecipeCatalog,   // false = chỉ recipe đang chạy tại thời điểm xuất
    int HistoryWindowCycles,         // giới hạn cứng — không phải "toàn bộ lịch sử"
    bool IncludeTeachImages);        // false = chỉ ảnh gây lỗi, không kèm ảnh dùng để teach

public interface IDiagnosticBundleExporter
{
    Task<string> ExportAsync(DiagnosticBundleRequest request, CancellationToken ct = default);
}
```

> 💡 **Mẹo thực chiến:** đặt sẵn một vị trí lưu mặc định cố định (thư mục cấu hình được, không đổi
> lung tung theo phiên) cho gói chẩn đoán, thay vì để vận hành viên tự chọn nơi lưu mỗi lần — giảm
> một bước quyết định không cần thiết đúng lúc họ đang bối rối vì sự cố, và giúp kỹ sư hỗ trợ (hoặc
> chính vận hành viên lúc bàn giao ca) luôn biết tìm file ở đâu mà không cần hỏi lại.

## 16.5 VisionPro Deep Learning — cái nhìn khái quát

### 16.5.1 Khi rule-based chạm giới hạn

Chương 10 (mục 10.5) đã nêu ba dấu hiệu cho thấy một bài toán vượt quá khả năng rule-based:
khuyết tật không có đặc trưng cường độ/hình học ổn định, cần phân loại nhiều loại khuyết tật khác
nhau, hoặc biến thể giữa các mẫu "tốt" quá lớn để định nghĩa ngưỡng bao trùm. Khi gặp một trong
ba dấu hiệu đó **sau khi** đã thử hết các kỹ thuật rule-based hợp lý (không phải bỏ qua rule-based
vì "deep learning nghe hiện đại hơn") — **VisionPro Deep Learning** là công cụ đúng lớp bài toán.

### 16.5.2 Bốn công cụ, bốn loại bài toán

VisionPro Deep Learning cung cấp bốn nhóm công cụ, mỗi nhóm tương ứng một loại bài toán học từ
dữ liệu thay vì công thức tường minh:

**Bảng 16.3 — Bốn nhóm công cụ VisionPro Deep Learning và bài toán tương ứng.**

| Nhóm | Bài toán | Tương tự rule-based nào trong sách này |
|---|---|---|
| **Locate** | Định vị đối tượng có biến thể hình dạng lớn giữa các mẫu | Tương tự vai trò PMAlign (Chương 8) nhưng cho đối tượng khó mô tả bằng hình học cố định |
| **Analyze** | Phát hiện dị thường/khuyết tật không có đặc trưng tường minh | Tương tự vai trò Blob + trừ ảnh (Chương 10) nhưng học "thế nào là bình thường" từ dữ liệu thay vì công thức |
| **Classify** | Phân loại một ảnh/vùng vào nhiều lớp | Không có tương đương rule-based trực tiếp trong sách — bài toán phân loại đa lớp phức tạp |
| **Read** | Đọc ký tự trong điều kiện khó (biến dạng, font không chuẩn) | Tương tự vai trò OCRMax (Chương 11) nhưng chịu được biến thể lớn hơn nhiều |

<!-- VERIFY: máy tác giả không cài module VisionPro Deep Learning — đã quét lại toàn bộ 89 DLL
trong ReferencedAssemblies (VisionPro 9.0 CR2), không có DLL nào tên chứa DeepLearn/CNN/Neural.
Bảng trên mô tả đúng 4 nhóm công cụ theo tài liệu ngành phổ biến, nhưng tên class/API cụ thể (nếu
sách cần trích dẫn) PHẢI xác minh trên bản cài có module Deep Learning trước khi đưa bất kỳ đoạn
code nào vào chương -->

### 16.5.3 Cái giá thật của deep learning — không miễn phí

Ba chi phí thường bị đánh giá thấp khi quyết định chuyển sang deep learning:

- **Dữ liệu.** Cần một lượng ảnh đã gán nhãn đủ lớn và đủ đa dạng — đúng yêu cầu "đại diện" đã
  nhấn mạnh cho golden set (mục 16.3.1), nhưng với số lượng lớn hơn nhiều bậc, và công sức gán
  nhãn (đặc biệt cho bài toán Analyze/Classify) là chi phí nhân lực thực, không chỉ chi phí tính
  toán.
- **Hạ tầng.** Huấn luyện mô hình thường cần GPU — một yêu cầu phần cứng mới với đa số trạm vision
  công nghiệp truyền thống (Chương 3) chưa từng cần đến.
- **Khả năng giải thích.** Khi một tool rule-based cho kết quả bất ngờ, ta lần theo từng bước
  (ROI → threshold → kết quả — đúng cách debug bằng CogRecordDisplay xuyên suốt sách) để hiểu vì
  sao. Mô hình deep learning khó giải thích hơn nhiều theo cách đó — "tại sao mô hình cho ảnh này
  là lỗi" không có câu trả lời từng bước rõ ràng như một chuỗi tool rule-based.

> 📌 **Lưu ý:** quyết định dùng deep learning nên đến sau khi cân nhắc đủ ba chi phí trên so với
> giá trị nó mang lại cho đúng bài toán — không phải quyết định mặc định "khó thì dùng AI". Với
> phần lớn bài toán công nghiệp, rule-based (Phần I-III của sách) giải quyết được với chi phí thấp
> hơn nhiều và dễ bảo trì/giải thích hơn nhiều; deep learning là công cụ cho đúng lớp bài toán còn
> lại, không phải thay thế mặc định.

## Tổng kết chương

- "Chạy được" (demo trên vài chi tiết mẫu) khác "chạy ổn định" (chịu được drift ánh sáng, dao
  động chi tiết, thay đổi lô — những biến thiên gần như luôn vắng mặt trong một buổi demo).
- Đặt ngưỡng có phương pháp: đo phân bố score của cả nhóm hợp lệ và nhóm lỗi thật; hai phân bố
  chồng lấn là tín hiệu vấn đề nằm ở tầng thu ảnh/đo lường, không phải "chưa tìm đúng ngưỡng".
  False accept và false reject không đối xứng về hậu quả — ngưỡng đúng phản ánh cái giá tương đối
  đó, không phải một điểm cân bằng toán học thuần tuý.
- Nghiệm thu cần golden set **đại diện** (đủ số lượng, đủ đa dạng, có cả chi tiết lỗi thật), chạy
  thống kê nhiều lần, và tiêu chí bàn giao thống nhất bằng văn bản trước khi chạy thử.
- Vận hành: ghi lại số liệu (không chỉ ảnh) của mọi cycle biến vision thành cảm biến sức khoẻ liên
  tục; giám sát xu hướng bắt được xuống cấp trước khi chạm ngưỡng gây lỗi hàng loạt; retrain đi
  theo quy trình có kiểm soát, không phải phản xạ tức thời.
- Khi vận hành viên tại chỗ không tự xử lý được, "xuất gói chẩn đoán" (mô tả sự cố + recipe đang
  chạy + log giới hạn thời gian + ảnh liên quan, phạm vi kiểm soát được) rút ngắn đáng kể thời gian
  chẩn đoán từ xa so với gửi rời rạc từng phần.
- Deep learning (VisionPro ViDi) giải đúng lớp bài toán rule-based không mô tả được bằng công
  thức tường minh — bốn công cụ Locate/Analyze/Classify/Read — nhưng có ba cái giá thật (dữ liệu,
  hạ tầng GPU, khả năng giải thích) cần cân nhắc, không phải lựa chọn mặc định.

## Lỗi thường gặp

**Lỗi 1 — Nghiệm thu chỉ bằng vài chi tiết "đẹp".** Hiện tượng: nghiệm thu đạt tuyệt đối, tỉ lệ
lỗi thực tế tăng dần sau vài tuần vận hành — đúng tình huống mở chương. Nguyên nhân: golden set
không đại diện cho biến thiên thật (dung sai, lô hàng, ánh sáng theo ca). Cách tránh: golden set
đủ số lượng và đa dạng, có cả chi tiết lỗi thật (mục 16.3.1).

**Lỗi 2 — Tinh chỉnh threshold khi phân bố OK/NG chồng lấn nhiều.** Hiện tượng: dành nhiều thời
gian "tối ưu" ngưỡng mà tỉ lệ sai không cải thiện đáng kể. Nguyên nhân: vấn đề thực chất nằm ở
tầng thu ảnh/đo lường (ánh sáng, độ phân giải, độ nhiễu), threshold không sửa được vấn đề ở tầng
khác. Cách tránh: kiểm tra độ chồng lấn phân bố trước khi tối ưu threshold; nếu chồng lấn nhiều,
quay lại Phần I-II thay vì tiếp tục chỉnh số (mục 16.2.2).

**Lỗi 3 — Retrain trực tiếp trên máy sản xuất theo phản xạ, không theo quy trình.** Hiện tượng:
retrain vội để "chữa cháy" khi thấy lỗi tăng, đôi khi làm tình hình tệ hơn vì mẫu train mới chưa
được kiểm chứng. Nguyên nhân: bỏ qua bước backup + chạy thử trên golden set trước khi thay job.
Cách tránh: retrain luôn đi qua quy trình đầy đủ (mục 16.4.3; Chương 14, mục 14.4) dù áp lực thời
gian lớn đến đâu.

**Lỗi 4 — Chỉ giám sát ngưỡng tức thời, không giám sát xu hướng.** Hiện tượng: lỗi hàng loạt xuất
hiện đột ngột dù "không có cảnh báo nào trước đó" — thực chất xu hướng xuống cấp đã diễn ra âm
thầm nhiều ngày/tuần trước, chỉ chưa chạm ngưỡng cảnh báo tức thời. Nguyên nhân: hệ thống chỉ so
sánh từng điểm dữ liệu với ngưỡng cố định, không theo dõi đường xu hướng. Cách tránh: ghi lại
metric mọi cycle, theo dõi trung bình trượt so với baseline lúc nghiệm thu (mục 16.4.1-16.4.2).

**Lỗi 5 — Chuyển sang deep learning mặc định khi rule-based "khó" thay vì "không thể".** Hiện
tượng: đầu tư đáng kể vào hạ tầng và dữ liệu huấn luyện cho một bài toán mà rule-based (chưa được
thử đủ kỹ) có thể giải quyết với chi phí thấp hơn nhiều. Nguyên nhân: xu hướng coi deep learning
là giải pháp "hiện đại hơn" thay vì đánh giá đúng bản chất bài toán. Cách tránh: xác nhận đã chạm
đúng một trong ba dấu hiệu giới hạn rule-based (Chương 10, mục 10.5) và cân nhắc đủ ba chi phí
thật (mục 16.5.3) trước khi quyết định.

**Lỗi 6 — Không có cách chuẩn để xin hỗ trợ từ xa, mỗi lần báo lỗi lại thu thập thủ công.** Hiện
tượng: một sự cố mất nhiều ngày qua lại hỏi-đáp giữa vận hành viên và kỹ sư hỗ trợ chỉ để thu thập
đủ thông tin chẩn đoán, trong khi bản thân sự cố có thể đã tự hết hoặc đổi dạng lúc thông tin thu
thập xong. Nguyên nhân: không có cơ chế đóng gói sẵn recipe + log + ảnh liên quan tại đúng thời
điểm xảy ra lỗi — mỗi lần đều làm thủ công, thiếu nhất quán. Cách tránh: thiết kế "gói chẩn đoán"
như một tính năng có sẵn, phạm vi mặc định hẹp nhưng mở rộng được khi cần (mục 16.4.5).

\newpage

# Phụ lục A — Bảng thuật ngữ

> Nội dung dành cho NGƯỜI ĐỌC — gộp thẳng vào sách khi merge cuối.
> KHÔNG ghi changelog/nhật ký vào file này (ghi vào `Glossary_Changelog_NoiBo.md`).
> Định dạng mỗi mục:
> **Tên thuật ngữ** (tiếng Anh nếu có) — định nghĩa 1-2 câu, súc tích.
> *Xuất hiện đầu tiên: Chương X, mục X.Y.*

## B

**Backlight** (đèn nền / xuyên sáng) — đèn đặt phía sau chi tiết, đối diện camera, biến chi tiết thành khối đen tuyệt đối trên nền sáng; cho biên dạng ngoài (silhouette) sắc nét nhất, nhưng xoá mọi đặc trưng trên bề mặt chi tiết.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.2.*

**Bar light** (đèn thanh) — một hoặc vài thanh LED thẳng, đặt linh hoạt quanh chi tiết; không tối ưu riêng cho bài toán nào, giá trị chính là tính linh hoạt cơ khí khi không đủ chỗ lắp ring/dome.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.6.*

**Bayer filter** — lưới lọc màu lặp 2×2 (1 đỏ, 2 xanh lá, 1 xanh dương) phủ lên cảm biến để tạo ảnh màu; mỗi pixel chỉ nhận đúng một màu, ba giá trị màu còn lại tại mỗi điểm phải nội suy (demosaic).
*Xuất hiện đầu tiên: Chương 3, mục 3.1.2.*

**Blob** — vùng pixel liền nhau cùng thoả điều kiện threshold, đơn vị phân tích của CogBlobTool.
*Xuất hiện đầu tiên: Chương 1, mục 1.2.1.*

## C

**Calibration** (hiệu chuẩn) — quá trình dạy hệ vision quy đổi toạ độ pixel sang đơn vị thực (mm) và khử méo quang học.
*Xuất hiện đầu tiên: Chương 1, mục 1.2.1.*

**Caliper** — kỹ thuật/tool tìm cạnh (edge) dọc theo một vùng quét hẹp, nền tảng của đo lường trong VisionPro.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.2.*

**Camera 3D** — camera đo được cả chiều cao/độ sâu (Z), không chỉ vị trí X-Y trên mặt phẳng ảnh; cần khi hai chi tiết chỉ khác nhau về chiều cao vẫn cho ảnh 2D giống hệt nhau.
*Xuất hiện đầu tiên: Chương 3, mục 3.6.2.*

**CameraLink** — chuẩn giao tiếp camera công nghiệp băng thông rất cao, độ trễ thấp, bắt buộc dùng frame grabber; phổ biến cho camera line scan tốc độ cao.
*Xuất hiện đầu tiên: Chương 3, mục 3.4.1.*

**Close** (đóng, hình thái học) — phép Dilation rồi Erosion; lấp khe/lỗ nhỏ hoặc nối các vùng gần nhau bị đứt đoạn, giữ nguyên kích thước tổng thể.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.2.*

**Coaxial light** (đèn đồng trục) — đưa ánh sáng đi đúng theo trục quang của ống kính qua một gương bán mờ; hiệu quả cho bề mặt phẳng phản chiếu cao khi cần phát hiện khuyết tật bề mặt.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.4.*

**CoaXPress** — chuẩn giao tiếp camera công nghiệp băng thông cao nhất trên khoảng cách xa (cáp đồng trục), bắt buộc dùng frame grabber; thay thế CameraLink ở nhiều thiết kế mới.
*Xuất hiện đầu tiên: Chương 3, mục 3.4.1.*

**CogJob** — đối tượng quản lý vòng đời chạy tự động (thu ảnh, chạy tool, đếm throughput) của QuickBuild; ứng dụng C# sản xuất thường thay bằng vòng lặp tự viết gọi trực tiếp CogToolBlock.
*Xuất hiện đầu tiên: Chương 5, mục 5.3.1.*

**CogRecordDisplay** — control hiển thị ảnh cùng toàn bộ overlay kết quả (ROI, biên caliper, khung PMAlign...) từ một Record; công cụ debug trực quan dùng xuyên suốt sách.
*Xuất hiện đầu tiên: Chương 5, mục 5.5.1.*

**CogToolBlock** (ToolBlock) — đơn vị đóng gói một cụm tool cùng cách nối dây thành một khối có Inputs/Outputs; là "hợp đồng terminal" giữa vision engineer và code C#.
*Xuất hiện đầu tiên: Chương 5, mục 5.4.1.*

**Computer vision** (thị giác máy tính, CV) — lĩnh vực thị giác máy tính nói chung, chấp nhận cảnh vật/ánh sáng bất kỳ và không ràng buộc thời gian cứng; khác machine vision ở ba ràng buộc công nghiệp.
*Xuất hiện đầu tiên: Chương 1, mục 1.1.*

**Connectivity** — bước xác định các pixel foreground liền kề có được coi là cùng một blob hay không, và loại bỏ nhiễu hạt nhỏ hơn một ngưỡng pixel.
*Xuất hiện đầu tiên: Chương 10, mục 10.1.1.*

**Coordinate space** (không gian toạ độ) — mỗi ảnh VisionPro mang một cây không gian toạ độ (coordinate space tree); calibration và fixturing hoạt động bằng cách treo thêm không gian mới vào cây và chọn nó làm selected space.
*Xuất hiện đầu tiên: Chương 7, mục 7.1.2.*

**Correlation** (tương quan chéo chuẩn hoá) — kỹ thuật so khớp mẫu "cổ điển": trượt khuôn mẫu qua ảnh, tính độ tương quan mức xám tại mỗi vị trí; nhạy với xoay, co giãn và nhiễu hơn hẳn PatMax.
*Xuất hiện đầu tiên: Chương 4, mục 4.5.*

**CurrentRecord** — record phản ánh trạng thái hiện tại của tool (kể cả chưa chạy), dùng khi xem cấu hình đang chỉnh, khác với LastRunRecord (kết quả một lần chạy đã qua).
*Xuất hiện đầu tiên: Chương 5, mục 5.5.2.*

**Cycle time** — tổng thời gian máy hoàn thành một chi tiết; vision phải hoàn tất chu trình trigger-chụp-xử lý-trả kết quả trong một phần ngân sách cycle time đó.
*Xuất hiện đầu tiên: Chương 1, mục 1.4.3.*

## D

**Dark field** (trường tối / chiếu góc thấp) — chiếu sáng từ nhiều hướng ở góc rất thấp; bề mặt phẳng nhẵn hiện tối, nhưng đặc trưng nhô/lõm (vết xước, mã khắc DPM) hắt sáng ngược lại ống kính nên hiện sáng trên nền tối.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.5.*

**Data-flow** — triết lý thiết kế của VisionPro: công việc biểu diễn bằng cách nối các tool vào nhau qua terminal, dữ liệu chảy qua đường nối, khác hẳn tư duy lập trình tuần tự.
*Xuất hiện đầu tiên: Chương 5, mục 5.2.*

**DataMatrix** — một loại mã ma trận 2D dùng phổ biến để truy vết chi tiết công nghiệp (mã vạch, ký tự khắc laser).
*Xuất hiện đầu tiên: Chương 1, mục 1.2.1.*

**Deep Learning** (VisionPro Deep Learning / ViDi) — nhóm công cụ VisionPro học từ dữ liệu thay vì công thức tường minh (Locate/Analyze/Classify/Read), dùng khi rule-based không mô tả được đặc trưng bằng ngưỡng/hình học tường minh; có ba cái giá thật: dữ liệu gán nhãn, hạ tầng GPU, khả năng giải thích.
*Xuất hiện đầu tiên: Chương 1, mục 1.1.2.*

**Demosaic** — bước nội suy hai kênh màu còn thiếu tại mỗi pixel từ pixel lân cận cùng kênh, biến ảnh Bayer thô thành ảnh RGB đầy đủ.
*Xuất hiện đầu tiên: Chương 3, mục 3.1.2.*

**Depth of Field** (DOF) — bề dày vùng không gian quanh mặt phẳng lấy nét mà độ mờ vẫn được coi là chấp nhận được; tăng khi khép khẩu độ, đánh đổi bằng lượng ánh sáng cần nhiều hơn.
*Xuất hiện đầu tiên: Chương 2, mục 2.4.3.*

**Dilation** (giãn nở, hình thái học) — pixel background trở thành foreground nếu có ít nhất một lân cận là foreground; phình to vùng, có thể nối liền các vùng gần nhau.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.2.*

**Distortion** — méo hình học phi tuyến của ống kính khiến đường thẳng thực tế cong nhẹ trên ảnh (dạng "gối lồi"/"gối lõm"); khắc phục bằng mô hình hiệu chuẩn phi tuyến (checkerboard).
*Xuất hiện đầu tiên: Chương 2, mục 2.6.*

**Dome light** (đèn bán cầu khuếch tán) — chụp bán cầu khuếch tán ánh sáng từ mọi hướng; giải pháp cho bề mặt cong hoặc phản chiếu mạnh, nơi ánh sáng định hướng luôn tạo loé sáng ở đâu đó.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.3.*

**DPM** (Direct Part Marking) — mã/ký tự khắc trực tiếp lên vật liệu (laser, đột dập, khắc kim cương); độ tương phản thấp, gần như luôn cần chiếu sáng dark field/góc thấp.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.5.*

**Drift** (trôi) — hiện tượng kết quả vision lệch dần theo thời gian dù chi tiết không đổi; bốn loại thường gặp là cường độ (Intensity), tịnh tiến (Translation), xoay (Rotation) và tỉ lệ (Scaling), mỗi loại trỏ đến một nguyên nhân/cách sửa khác nhau.
*Xuất hiện đầu tiên: Chương 16, mục 16.4.4.*

## E

**Edge** (cạnh) — vị trí độ lớn gradient của ảnh đạt cực đại cục bộ, không phải nơi cường độ tuyệt đối bằng một số cụ thể; nền tảng của mọi tool đo biên như Caliper.
*Xuất hiện đầu tiên: Chương 4, mục 4.4.1.*

**Elongation** — tỉ lệ độ dài trục chính/trục phụ của một blob; gần 1 là tròn/vuông, giá trị lớn là hình dạng dài-hẹp, dùng để lọc blob bất thường.
*Xuất hiện đầu tiên: Chương 10, mục 10.2.1.*

**Erosion** (ăn mòn, hình thái học) — một pixel foreground chỉ được giữ lại nếu toàn bộ lân cận cũng là foreground; thu nhỏ vùng, có thể tách hai vùng dính nhau qua cầu nối mảnh.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.2.*

**Exposure** (thời gian phơi sáng) — khoảng thời gian mỗi photosite được phép tích luỹ điện tích trước khi số hoá; càng dài ảnh càng sáng nhưng càng dễ nhoè nếu vật/camera còn chuyển động.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.1.*

## F

**False accept** — lỗi ngưỡng quá lỏng khiến hàng lỗi lọt qua kiểm tra; hậu quả thường nghiêm trọng và phát hiện muộn hơn false reject.
*Xuất hiện đầu tiên: Chương 8, mục 8.3.*

**False reject** — lỗi ngưỡng quá chặt khiến hàng tốt bị loại oan; tốn chi phí xử lý lại nhưng phát hiện ngay tại chỗ.
*Xuất hiện đầu tiên: Chương 8, mục 8.3.*

**Fielding** — thành phần của CogOCRMaxTool ràng buộc định dạng chuỗi ký tự đọc được (độ dài, ký tự cho phép ở từng vị trí), chặn phần lớn kết quả "đọc được nhưng sai định dạng".
*Xuất hiện đầu tiên: Chương 11, mục 11.3.1.*

**File .vpp** — file nhị phân serialize lưu toàn bộ cấu trúc job VisionPro (tool, cách nối dây, tham số); không phải văn bản nên không diff được như code, cần kỷ luật backup vật lý.
*Xuất hiện đầu tiên: Chương 5, mục 5.2.2.*

**Filter quang học** (band-pass filter) — tấm kính/màng mỏng gắn trước ống kính, chỉ cho một dải bước sóng đi qua; loại bỏ nhiễu ánh sáng môi trường tại nguồn, khác hẳn xử lý màu bằng phần mềm.
*Xuất hiện đầu tiên: Chương 2, mục 2.3.2.*

**Fixturing** — kỹ thuật neo ROI của các tool kiểm tra theo vị trí thực tế của chi tiết (tìm bởi tool định vị), để job chịu được chi tiết đặt lệch.
*Xuất hiện đầu tiên: Chương 1, mục 1.2.1.*

**FOV** (Field of View — trường nhìn) — vùng không gian thực mà camera nhìn thấy, quyết định bởi tiêu cự lens và working distance.
*Xuất hiện đầu tiên: Chương 1, mục 1.3.1.*

**Frame grabber** — thiết bị/giao tiếp đưa dữ liệu ảnh từ camera vào bộ nhớ máy tính, kèm tín hiệu đồng bộ trigger; một số chuẩn giao tiếp (CameraLink, CoaXPress) bắt buộc phải có.
*Xuất hiện đầu tiên: Chương 1, mục 1.3.1.*

**Frame rate** — số khung hình camera tạo ra mỗi giây, xấp xỉ nghịch đảo của tổng thời gian exposure cộng readout; là một phần của ngân sách thời gian acquisition trong cycle time.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.3.*

## G

**Gain** — hệ số khuếch đại điện tử áp lên tín hiệu sau khi cảm biến đã thu sáng; khuếch đại cả nhiễu nền, nên là lựa chọn cuối cùng sau khi đã tối ưu ánh sáng và exposure.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.2.*

**GenICam** (Generic Interface for Cameras) — chuẩn công nghiệp cho phần mềm cấu hình camera (exposure, gain, trigger...) qua một mô hình tham số chung, không phụ thuộc hãng camera hay chuẩn truyền dữ liệu.
*Xuất hiện đầu tiên: Chương 3, mục 3.4.3.*

**GIGI** (Guidance, Inspection, Gauging, Identification) — khung phân loại bốn dạng bài toán vision công nghiệp: dẫn hướng, kiểm tra, đo lường, đọc mã/nhận dạng.
*Xuất hiện đầu tiên: Chương 1, mục 1.2.1.*

**GigE Vision** — chuẩn giao tiếp camera công nghiệp qua cáp mạng Gigabit Ethernet, không cần frame grabber chuyên dụng; phổ biến, rẻ, dễ kéo dài nhưng băng thông chia sẻ khi nhiều camera dùng chung switch.
*Xuất hiện đầu tiên: Chương 1, mục 1.3.1.*

**Global shutter** — toàn bộ pixel trên cảm biến bắt đầu và kết thúc phơi sáng tại đúng cùng một thời điểm; an toàn cho mọi tình huống có khả năng chuyển động, bắt buộc khi kết hợp strobe.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.4.*

**Gói chẩn đoán** (Diagnostic bundle) — mô tả sự cố, recipe/cấu hình đang dùng, log giới hạn thời gian và ảnh liên quan đóng gói vào một hành động duy nhất để gửi xin hỗ trợ kỹ thuật từ xa; phạm vi mặc định hẹp, mở rộng tường minh khi cần, tránh tạo file khổng lồ không ai gửi/mở nổi.
*Xuất hiện đầu tiên: Chương 16, mục 16.4.5.*

**Golden image** (ảnh chuẩn) — ảnh tham chiếu "sạch" dùng để trừ ảnh khi kiểm khuyết tật bề mặt; phải cùng hệ toạ độ (đã fixture) và cùng điều kiện ánh sáng với ảnh kiểm.
*Xuất hiện đầu tiên: Chương 10, mục 10.4.2.*

**Golden set** — bộ ảnh/chi tiết chuẩn dùng để nghiệm thu và tinh chỉnh ngưỡng, phải đại diện (đủ số lượng, đủ đa dạng, có cả chi tiết lỗi thật), không chỉ gồm ảnh "đẹp".
*Xuất hiện đầu tiên: Chương 5 (mục Lỗi thường gặp).*

**Gradient** — đạo hàm rời rạc của cường độ ảnh theo không gian; vị trí gradient đạt cực đại cục bộ chính là vị trí một cạnh (edge).
*Xuất hiện đầu tiên: Chương 4, mục 4.4.1.*

**Grading** — đánh giá chất lượng mã đọc được (còn bao nhiêu biên độ an toàn để đọc ổn định lâu dài), không chỉ "đọc được hay không"; chuẩn hoá qua các chỉ số như ISO/IEC 15415/15416.
*Xuất hiện đầu tiên: Chương 11, mục 11.2.*

**Grayscale 8-bit** — thang độ sáng chuẩn của xử lý ảnh công nghiệp, 256 mức (0 đen tuyệt đối, 255 trắng tuyệt đối); là định dạng dữ liệu mà phần lớn tool vision thao tác lên.
*Xuất hiện đầu tiên: Chương 4, mục 4.1.1.*

## H

**Hand-eye calibration** — quy trình hiệu chuẩn khi camera gắn trên tay robot (eye-in-hand): tìm phép biến đổi cố định giữa mặt bích robot và camera qua nhiều tư thế đã biết; khác về chất so với calibration camera cố định.
*Xuất hiện đầu tiên: Chương 7, mục 7.6.*

**Hệ số bù hệ thống** (Systematic bias correction) — hằng số cộng/trừ áp lại cho kết quả đo sau khi phát hiện độ lệch ỔN ĐỊNH (không phải nhiễu ngẫu nhiên) so với thiết bị đo chuẩn ngoại vi (CMM); tách biệt khỏi calibration hình học — sửa phần sai số còn sót lại đặc thù cho đúng phép đo và điều kiện chụp, phải đo lại khi điều kiện đó đổi.
*Xuất hiện đầu tiên: Chương 9, mục 9.4.4.*

**Hình thái học** (Morphology) — nhóm bốn phép toán (Erosion, Dilation, Open, Close) thao tác trên ảnh nhị phân để sửa hình dạng vùng trước khi đo, không động đến giá trị mức xám gốc.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.2.*

**Histogram** — biểu đồ đếm số lượng pixel theo từng mức xám; công cụ chẩn đoán ánh sáng bằng mắt, và là nền tảng của threshold tự động (Otsu).
*Xuất hiện đầu tiên: Chương 4, mục 4.1.2.*

## I

**ISO/IEC 15415 / 15416** — chuẩn ngành chấm điểm chất lượng mã (A đến F) dựa trên các thành phần như độ tương phản, độ đồng đều mô-đun, tỉ lệ lỗi sửa được; 15415 cho mã 2D, 15416 cho mã 1D.
*Xuất hiện đầu tiên: Chương 11, mục 11.2.*

## K

**Khẩu độ** (F-number, N) — tỉ số giữa tiêu cự và đường kính lỗ mở ống kính; khép khẩu độ tăng Depth of Field nhưng giảm lượng ánh sáng đi qua.
*Xuất hiện đầu tiên: Chương 2, mục 2.4.4.*

## L

**LastRunRecord** — record ghi lại đúng trạng thái ảnh và kết quả sau khi tool vừa chạy xong; dùng để xem lại một lần chạy cụ thể, khác CurrentRecord (trạng thái hiện tại).
*Xuất hiện đầu tiên: Chương 5, mục 5.5.2.*

**Line scan camera** — camera có cảm biến chỉ một (hoặc vài) hàng pixel; ảnh 2D được dựng dần bằng ghép các dòng quét khi vật liệu di chuyển liên tục, dùng cho vật liệu dạng cuộn/tấm không có điểm dừng để chụp.
*Xuất hiện đầu tiên: Chương 3, mục 3.6.1.*

## M

**Machine vision** (thị giác máy công nghiệp) — ứng dụng thị giác máy trong môi trường công nghiệp, khác computer vision ở ba ràng buộc: ánh sáng chủ động thiết kế, thời gian phản hồi ràng buộc cứng theo cycle time, và hậu quả vật lý thật khi trả lời sai.
*Xuất hiện đầu tiên: Chương 1, mục 1.1.*

**Median filter** (lọc trung vị) — thay giá trị mỗi pixel bằng giá trị trung vị của cửa sổ lân cận; loại bỏ hoàn toàn nhiễu dạng đốm/xung rời rạc mà không làm mờ biên như smoothing.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.1.*

**MES** (Manufacturing Execution System) — hệ thống cấp trên (còn gọi "host") nhận bản ghi kết quả kiểm tra/đọc mã gắn kèm ngữ cảnh sản xuất để phục vụ truy vết.
*Xuất hiện đầu tiên: Chương 11, mục 11.4.*

**Mono** (camera đơn sắc) — camera không có Bayer filter, chỉ đo cường độ sáng; lựa chọn mặc định cho vision công nghiệp vì độ phân giải hiệu dụng cao hơn, nhạy sáng hơn, pipeline đơn giản hơn.
*Xuất hiện đầu tiên: Chương 3, mục 3.1.3.*

**Multi-cavity** — trạm kiểm tra nhiều chi tiết cùng lúc trong một khung hình (khuôn nhiều lòng khuôn); bản tin gửi PLC/robot khi đó cần mở rộng thành một mảng kết quả, mỗi phần tử có cờ hợp lệ riêng.
*Xuất hiện đầu tiên: Chương 15, mục 15.5.1.*

## O

**Open** (mở, hình thái học) — phép Erosion rồi Dilation; loại bỏ tua/gai/hạt nhiễu nhỏ, giữ nguyên kích thước tổng thể của vùng chính.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.2.*

**Otsu** — thuật toán threshold tự động, chọn ngưỡng làm phương sai giữa hai lớp pixel đạt cực đại; hoạt động tốt khi histogram thật sự có hai đỉnh tách biệt (bimodal).
*Xuất hiện đầu tiên: Chương 4, mục 4.2.2.*

## P

**PatMax** — thuật toán định vị mẫu của Cognex dựa trên đặc trưng hình học thay vì so khớp mức xám, lõi của CogPMAlignTool.
*Xuất hiện đầu tiên: Chương 2 (đoạn mở đầu chương).*

**PC-based** (hệ PC-based) — kiến trúc vision tách camera (chỉ thu ảnh) khỏi phần xử lý, ảnh truyền về máy tính công nghiệp chạy phần mềm (như VisionPro); mạnh, linh hoạt, phù hợp nhiều tool/nhiều camera phối hợp.
*Xuất hiện đầu tiên: Chương 1, mục 1.5.1.*

**Photosite** — giếng thu ánh sáng của một pixel trên cảm biến CMOS, chuyển đổi photon tới thành điện tích; bản thân chỉ đo được tổng số photon, không phân biệt bước sóng (màu).
*Xuất hiện đầu tiên: Chương 3, mục 3.1.1.*

**Polarity** — chiều chuyển sáng-tối tại một biên (tối-sang-sáng hay sáng-sang-tối); khai báo tường minh giúp tool loại bỏ ngay các ứng viên biên sai chiều tương phản.
*Xuất hiện đầu tiên: Chương 4, mục 4.4.2.*

**Polarizer** — tấm kính phân cực đặt trước nguồn sáng và ống kính (bố trí crossed polarizer) để khử loé sáng phản xạ gương trên bề mặt bóng, đổi lại ảnh tối đi tổng thể.
*Xuất hiện đầu tiên: Chương 2, mục 2.3.3.*

**Pose** — vị trí (X, Y) và góc xoay của một chi tiết, thường là kết quả trả về bởi tool định vị (PMAlign); dùng làm đầu vào cho fixturing hoặc gửi robot.
*Xuất hiện đầu tiên: Chương 7, mục 7.4.1.*

## R

**Record** (ICogRecord) — gói dữ liệu mang ảnh (Content) và có thể chứa các record con (SubRecords), mỗi record con ứng với kết quả một tool; nền tảng của hiển thị/debug qua CogRecordDisplay.
*Xuất hiện đầu tiên: Chương 5, mục 5.5.1.*

**Repeatability** (độ lặp lại) — độ dao động của kết quả đo khi đo lặp lại cùng một chi tiết đứng yên; đo bằng 30 lần chụp liên tiếp, cho biết "sàn nhiễu" của cả hệ thống.
*Xuất hiện đầu tiên: Chương 9, mục 9.4.2.*

**Reproducibility** (độ tái lập) — độ ổn định của kết quả đo khi có một điều kiện thay đổi mà lẽ ra không nên ảnh hưởng (gắp-đặt lại chi tiết, khác ca, khởi động lại máy); thường quan trọng hơn repeatability cho trạm sản xuất thực tế.
*Xuất hiện đầu tiên: Chương 9, mục 9.4.3.*

**ResultsAnalysis** — cây biểu thức logic (AND/OR/so sánh) tổng hợp kết quả nhiều tool thành một quyết định cuối (Accept/Warning/Reject/Error), dựng chủ yếu bằng QuickBuild thay vì viết tay bằng code.
*Xuất hiện đầu tiên: Chương 12, mục 12.3.*

**Retrain** — huấn luyện lại pattern định vị khi score tụt do lô hàng/mẫu train không còn đại diện; chỉ nên làm sau khi đã loại trừ nguyên nhân ánh sáng/cơ khí, và luôn backup job cũ trước khi ghi đè.
*Xuất hiện đầu tiên: Chương 8, mục 8.5.2.*

**Ring light** (đèn vòng) — đèn gắn thành vòng quanh trục ống kính, chiếu gần thẳng góc xuống bề mặt; cách bố trí mặc định quen thuộc, phù hợp bề mặt khuếch tán đều nhưng tạo loé sáng trên bề mặt bóng.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.1.*

**RMS** (sai số quân phương) — chỉ số đo độ lệch trung bình bình phương giữa các điểm calibration hoặc điểm khớp hình học sau khi áp mô hình; RMS lớn báo hiệu điểm nhập sai hoặc quan hệ không còn tuyến tính.
*Xuất hiện đầu tiên: Chương 7, mục 7.2.1.*

**ROI** (Region of Interest) — vùng ảnh mà một tool được phép xử lý, thay vì toàn khung hình.
*Xuất hiện đầu tiên: Chương 2, mục 2.6.*

**Rolling shutter** — cảm biến đọc ảnh theo từng hàng pixel nối tiếp, lệch thời điểm phơi sáng; vô hại với cảnh tĩnh nhưng gây méo hình (nghiêng, xé) với vật đang chuyển động.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.4.*

**RunTimeMeasures** — cơ chế khai báo điều kiện lọc thuộc tính blob (ví dụ khoảng diện tích hợp lệ) ngay trong CogBlobTool, thay vì đọc hết kết quả rồi lọc bằng code.
*Xuất hiện đầu tiên: Chương 10, mục 10.2.2.*

## S

**Sai số phối cảnh** (Perspective error) — sai số hệ thống khi độ phóng đại thay đổi theo khoảng cách vật (do lens thường không viễn tâm), gây lệch số đo khi đặc trưng cần đo không cùng mặt phẳng với lúc calibration.
*Xuất hiện đầu tiên: Chương 2, mục 2.5.1.*

**Score** (điểm khớp) — thước đo độ tin cậy chính của một kết quả định vị (0–1); nên đặt ngưỡng chấp nhận dựa trên phân bố score đo từ golden set, không đoán một con số.
*Xuất hiện đầu tiên: Chương 8, mục 8.4.2.*

**Segmentation** (phân vùng) — bước chuyển ảnh xám thành ảnh nhị phân foreground/background; chỉ sau bước này khái niệm "blob" mới tồn tại.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.2.*

**Selected space** — không gian toạ độ "đang được chọn" của một ảnh, nơi mọi tool mặc định đọc ROI và trả kết quả; calibration/fixturing hoạt động bằng cách gắn không gian mới rồi chọn nó làm selected space.
*Xuất hiện đầu tiên: Chương 7, mục 7.1.2.*

**Shot noise** (nhiễu hạt photon) — nhiễu do bản chất thống kê rời rạc của ánh sáng (số photon đến dao động ngẫu nhiên quanh giá trị trung bình); luôn tồn tại kể cả với cảm biến lý tưởng, tăng theo căn bậc hai cường độ sáng.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.2.*

**Shroud** (chụp che cơ khí) — khung/hộp che kín vùng nhìn của camera khỏi ánh sáng môi trường bên ngoài; giải pháp triệt để nhất trong ba tuyến phòng thủ chống nhiễu ánh sáng.
*Xuất hiện đầu tiên: Chương 2, mục 2.3.4.*

**Smart camera** — kiến trúc gói camera, bộ xử lý và phần mềm xử lý ảnh vào chung một thân thiết bị; gọn, đơn giản, phù hợp bài toán đơn nhưng hạn chế khi cần nhiều tool phối hợp hoặc tích hợp sâu.
*Xuất hiện đầu tiên: Chương 1, mục 1.5.1.*

**Smoothing** (làm mượt) — thay giá trị mỗi pixel bằng trung bình cộng của các pixel lân cận; giảm nhiễu ngẫu nhiên nhưng làm mờ cả biên thật.
*Xuất hiện đầu tiên: Chương 4, mục 4.3.1.*

**SNR** (Signal-to-Noise Ratio — tỉ số tín hiệu/nhiễu) — thước đo chất lượng tín hiệu ảnh; tăng gain không cải thiện SNR (khuếch đại cả nhiễu), trong khi tăng exposure hoặc ánh sáng thật sự cải thiện SNR.
*Xuất hiện đầu tiên: Chương 3, mục 3.3.2.*

**Strobe** — đèn chớp sáng cực ngắn và cực mạnh thay vì chiếu liên tục, đồng bộ với cửa sổ exposure để "đóng băng" vật chuyển động; chỉ hoạt động đúng với global shutter.
*Xuất hiện đầu tiên: Chương 2, mục 2.3.4.*

**Sub-pixel** — độ chính xác đo cạnh nhỏ hơn kích thước một pixel, đạt được bằng cách khớp một đường cong qua các điểm gradient gần đỉnh để nội suy vị trí cực đại chính xác hơn một pixel nguyên.
*Xuất hiện đầu tiên: Chương 4, mục 4.4.2.*

**Symbology** — loại/định dạng mã cụ thể (DataMatrix, QR Code, Code128...) mà một tool đọc mã có thể nhận dạng; chỉ nên bật đúng symbology thực sự có trên chi tiết.
*Xuất hiện đầu tiên: Chương 11, mục 11.1.1.*

## T

**Takt time** — nhịp mong muốn của dây chuyền (thời gian tối đa cho phép mỗi chi tiết); cam kết cycle time của vision phải dựa trên thời gian xấu nhất đo được, không phải trung bình, để không vượt takt time.
*Xuất hiện đầu tiên: Chương 15, mục 15.2.2.*

**Telecentric lens** — ống kính thiết kế sao cho tia sáng chính gần như song song với trục quang, giữ độ phóng đại gần như không đổi khi khoảng cách vật thay đổi; triệt tiêu gần hết sai số phối cảnh nhưng đắt và giới hạn FOV/working distance.
*Xuất hiện đầu tiên: Chương 2, mục 2.5.2.*

**Terminal** — điểm vào/ra dữ liệu có tên của một tool hoặc CogToolBlock trong VisionPro; nối terminal đầu ra của tool này vào terminal đầu vào của tool khác tạo thành chuỗi xử lý (data-flow).
*Xuất hiện đầu tiên: Chương 5, mục 5.2.1.*

**Threshold** (ngưỡng) — giá trị mức xám phân tách pixel thành hai nhóm (nền/đối tượng), nền tảng của segmentation.
*Xuất hiện đầu tiên: Chương 2, mục 2.2.7.*

**Tiêu cự** (Focal length, f) — thông số vật lý của ống kính quyết định mối quan hệ giữa FOV, kích thước cảm biến và working distance; công thức xấp xỉ f ≈ (WD × kích thước cảm biến) / FOV.
*Xuất hiện đầu tiên: Chương 2, mục 2.4.1.*

**Tỉ lệ phóng đại** (Magnification) — tỉ số giữa kích thước cảm biến và FOV, dùng để tính tiêu cự cần thiết khi chọn ống kính.
*Xuất hiện đầu tiên: Chương 2, mục 2.4.2.*

**Traceability** (truy vết) — khả năng gắn kết quả đọc mã với một sự kiện sản xuất cụ thể và lưu trữ để truy vấn lại; mất một bản ghi truy vết là mất vĩnh viễn, khác lỗi đo lường có thể khắc phục ở cycle sau.
*Xuất hiện đầu tiên: Chương 11, mục 11.4.*

**Trigger** — tín hiệu báo hiệu camera/hệ vision bắt đầu chụp, thường phát từ PLC khi chi tiết đã vào đúng vị trí.
*Xuất hiện đầu tiên: Chương 1, mục 1.4.2.*

**Trigger phần cứng** (Hardware trigger) — tín hiệu điện áp thực đưa trực tiếp vào chân trigger camera; độ trễ thấp và ổn định, bắt buộc khi cần đồng bộ chặt với chuyển động hoặc đèn strobe.
*Xuất hiện đầu tiên: Chương 3, mục 3.5.1.*

**Trigger phần mềm** (Software trigger) — lệnh chụp gửi qua kênh giao tiếp dữ liệu, không cần dây tín hiệu riêng; đơn giản về đấu nối nhưng độ trễ dao động (jitter) cao hơn trigger phần cứng.
*Xuất hiện đầu tiên: Chương 3, mục 3.5.2.*

**Tương phản** (Contrast) — chênh lệch độ sáng giữa đặc trưng cần thấy và phần còn lại; nguyên tắc vàng của chiếu sáng là tạo tương phản ổn định cho đúng đặc trưng cần thấy, đồng thời triệt tiêu tương phản không liên quan.
*Xuất hiện đầu tiên: Chương 2, mục 2.1.*

## U

**USB3 Vision** — chuẩn giao tiếp camera công nghiệp qua USB3 SuperSpeed, băng thông cao hơn GigE Vision trên một kết nối, cắm-chạy đơn giản nhưng hạn chế khoảng cách cáp.
*Xuất hiện đầu tiên: Chương 3, mục 3.4.1.*

## W

**Working distance** (WD) — khoảng cách từ mặt trước ống kính đến mặt phẳng chi tiết; bị ràng buộc bởi không gian cơ khí thực tế và ảnh hưởng trực tiếp đến tiêu cự cần chọn.
*Xuất hiện đầu tiên: Chương 2, mục 2.4.1.*

\newpage

# Phụ lục B — Checklist khảo sát & triển khai hệ vision

Checklist này đi từ lúc nhận yêu cầu đến lúc nghiệm thu bàn giao, tổng hợp từ nội dung 16 chương
của sách — mỗi mục trỏ về chương/mục gốc để tra cứu chi tiết khi cần. In ra dùng trực tiếp tại
hiện trường; không thay thế việc đọc chương gốc lần đầu triển khai một kỹ thuật chưa quen.

## Phần 1: Khảo sát & xác định bài toán

Trả lời hết các câu hỏi dưới đây **trước khi** nghĩ đến camera/đèn/lens cụ thể nào.

- [ ] Xác định trạm thuộc (các) bài toán GIGI nào — Guidance/Inspection/Gauging/Identification, có
  thể nhiều loại cùng lúc trên một ảnh (xem Chương 1, mục 1.2.1, Bảng 1.2)
- [ ] Ghi rõ dung sai/độ chính xác yêu cầu cho **từng** bài toán riêng biệt — dung sai đo lường
  (Gauging) và dung sai định vị robot (Guidance) là hai con số khác nhau, không dùng chung một
  phép tính (xem Chương 1, mục 1.2.2; Chương 3, mục 3.2.3)
- [ ] Đo kích thước lớn nhất của chi tiết + biên độ xê dịch/lệch tâm/xoay thực tế trên
  pallet/băng tải — dữ liệu đầu vào để tính FOV có lề (xem Chương 2, mục 2.4.2)
- [ ] Xác định tốc độ máy/takt time — đây là ngân sách thời gian mà toàn bộ chu trình vision phải
  nằm gọn trong đó (xem Chương 1, mục 1.4.3)
- [ ] Khảo sát môi trường lắp đặt: ánh sáng xung quanh có đổi theo ca ngày/đêm không, rung động cơ
  khí, bụi/nước/dầu (yêu cầu shroud, cấp bảo vệ IP) (xem Chương 2, mục 2.3.4)
- [ ] Xác định vật liệu, màu sắc, độ bóng bề mặt chi tiết cần "nhìn thấy" — quyết định trực tiếp
  kỹ thuật chiếu sáng ở Phần 2 (xem Chương 2, mục 2.1)
- [ ] Xác nhận chi tiết đứng yên hay đang chuyển động tại thời điểm chụp — ràng buộc exposure và
  loại trigger (xem Chương 3, mục 3.3.1 và mục 3.5)
- [ ] Ghi rõ hậu quả nếu vision báo sai (bỏ lọt hàng lỗi so với báo nhầm hàng tốt) — dữ liệu này
  định hướng cách đặt ngưỡng ở Phần 4 (xem Chương 1, mục 1.2.2)

> ⚠️ **Không bỏ qua dù áp lực thời gian:** đừng chọn mua camera/đèn trước khi hoàn thành đủ 8 mục
> trên. Chọn phần cứng trước, tìm bài toán sau là con đường ngắn nhất dẫn đến việc phải mua lại
> thiết bị (xem Chương 1, mục 1.2.2).

## Phần 2: Thiết kế phần cứng

Dữ liệu khảo sát ở Phần 1 (dung sai, kích thước, biên độ xê dịch, môi trường) là đầu vào bắt buộc
cho mọi tính toán dưới đây.

- [ ] Chọn kỹ thuật chiếu sáng theo bề mặt và bài toán, đối chiếu Bảng 2.1 (ring/backlight/dome/
  coaxial/dark field/bar) (xem Chương 2, mục 2.2)
- [ ] Thử nhanh hướng chiếu sáng bằng đèn pin/LED để bàn trước khi đặt mua đèn công nghiệp đúng
  model (xem Chương 2, mục 2.2.6)
- [ ] Tính FOV cần thiết có chừa lề — bằng kích thước chi tiết lớn nhất cộng biên độ xê dịch thực
  tế, không tính khít theo đúng kích thước chi tiết (xem Chương 2, mục 2.4.2)
- [ ] Tính tiêu cự lens từ FOV, working distance dự kiến và kích thước cảm biến theo công thức
  `f ≈ WD × (kích thước cảm biến / FOV)`, rồi chọn tiêu cự chuẩn gần nhất có bán trên thị trường
  (xem Chương 2, mục 2.4.1–2.4.2)
- [ ] Tính depth of field và chọn khẩu độ đủ để bao trọn biến thiên chiều cao lớn nhất của chi
  tiết trong FOV (xem Chương 2, mục 2.4.3–2.4.4)
- [ ] Tính sai số phối cảnh (Δh/WD) để xác định có **bắt buộc** dùng telecentric lens hay không —
  đừng chọn telecentric theo phản xạ (xem Chương 2, mục 2.5.1)
- [ ] Tính độ phân giải camera cần thiết bằng cách chạy ngược từ dung sai đo (quy tắc pixel/dung
  sai cho đặc trưng rời rạc, hoặc mức thấp hơn nếu đo bằng caliper sub-pixel trên biên liên tục)
  (xem Chương 3, mục 3.2)
- [ ] Chọn giao tiếp camera (GigE Vision/USB3 Vision/CameraLink/CoaXPress) theo băng thông cần
  thiết, đặc biệt khi nhiều camera dùng chung switch (xem Chương 3, mục 3.4)
- [ ] Quyết định trigger phần cứng hay phần mềm, thiết kế đồng bộ đèn strobe theo timing thực tế
  (xem Chương 3, mục 3.5)
- [ ] Kiểm tra global shutter hay rolling shutter phù hợp nếu chi tiết chuyển động lúc chụp (xem
  Chương 3, mục 3.3.4)

## Phần 3: Thiết kế phần mềm & tích hợp

- [ ] Xác nhận loại license VisionPro cần cho máy dev **và** máy sản xuất trước khi mang máy ra
  dây chuyền (xem Chương 5, mục 5.1.1; Chương 13, mục 13.1.2)
- [ ] Reference đúng DLL cần dùng, đặt platform target x64 và .NET Framework 4.8 nhất quán cho
  toàn bộ project gọi API VisionPro (xem Chương 13, mục 13.1.1, Bảng 13.1)
- [ ] Thiết kế terminal của mỗi CogToolBlock rõ nghĩa, kiểu dữ liệu đơn giản ổn định, đủ không
  thừa — trước khi code C# bắt đầu phụ thuộc vào tên terminal đó (xem Chương 5, mục 5.4.2)
- [ ] Thực hiện calibration (NPointToNPoint hoặc Checkerboard tuỳ nhu cầu — xem Bảng 7.2) và kiểm
  tra `ComputedRMSError` không vượt ngưỡng đã tự đặt trước khi chấp nhận, kiểm tra này nằm trong
  code chứ không phải bước kiểm tay lúc setup rồi thôi (xem Chương 7, mục 7.2–7.3)
- [ ] Xây đúng thứ tự chuỗi Acquisition → Calibration → Align + Fixture → Inspect bên trong
  ToolBlock (xem Chương 7, mục 7.4)
- [ ] Cô lập toàn bộ code gọi trực tiếp API VisionPro sau một interface (kiểu `IVisionEngine`) —
  không để kiểu `Cog*` nào rò ra khỏi tầng Engine (xem Chương 14, mục 14.2.1)
- [ ] Tách tham số thuộc job (nằm trong `.vpp`) khỏi tham số thuộc recipe (nằm ngoài `.vpp`) — đổi
  recipe phải là đổi dữ liệu, không phải build lại solution (xem Chương 14, mục 14.3)
- [ ] Thiết kế handshake PLC-Vision đủ bốn tín hiệu tối thiểu (Trigger/Busy/Done/Ack), có timeout
  đặt ở **cả hai phía**, không chỉ phía vision (xem Chương 15, mục 15.1)
- [ ] Đo ngân sách cycle time thực tế qua nhiều cycle bằng `RunStatus.ProcessingTime` — không đoán
  một con số (xem Chương 15, mục 15.2.1)
- [ ] Tổ chức file `.vpp` có kỷ luật: quy ước đặt tên nhất quán, tách job phát triển khỏi job
  production, backup phiên bản cũ trước khi ghi đè (xem Chương 5, mục 5.6)

## Phần 4: Nghiệm thu

- [ ] Xây golden set đủ số lượng và đủ đa dạng — trải hết dải dung sai, nhiều lô nếu có thể, và có
  cả chi tiết lỗi thật chứ không chỉ chi tiết đẹp (xem Chương 16, mục 16.3.1)
- [ ] Viết tiêu chí bàn giao thành văn bản và thống nhất **trước** khi chạy thử nghiệm thu, tránh
  quyết định tiêu chí sau khi đã thấy kết quả (xem Chương 16, mục 16.3.3)
- [ ] Chạy toàn bộ golden set nhiều lượt, ghi lại phân bố kết quả (tỉ lệ Accept/Warning/Reject/
  Error, phân bố score, phân bố thời gian xử lý) — không chấp nhận kết quả của một lượt chạy duy
  nhất (xem Chương 16, mục 16.3.2)
- [ ] Chạy lại ở điều kiện có thay đổi (khởi động lại ứng dụng, ca khác, tháo lắp lại chi tiết mẫu
  thay vì để nguyên) để kiểm tra kết quả không chỉ đúng trong điều kiện đo liên tục (xem Chương
  16, mục 16.3.2)
- [ ] Xác nhận cycle time đạt cam kết theo giá trị **xấu nhất** (P99 hoặc giá trị đỉnh quan sát
  được), không phải theo giá trị trung bình (xem Chương 15, mục 15.2.2)
- [ ] Xác nhận không có Error hệ thống nào phát sinh trong toàn bộ lượt chạy thử nghiệm thu (xem
  Chương 16, mục 16.3.3)
- [ ] Đối chiếu kết quả thực đo với tiêu chí bàn giao đã thống nhất — chỉ ký nghiệm thu khi đạt đủ
  tiêu chí, không châm chước vì áp lực tiến độ (xem Chương 16, mục 16.3.3)

> ⚠️ **Không bỏ qua dù áp lực thời gian:** nghiệm thu chỉ bằng chi tiết tốt (không có mẫu lỗi
> thật) là nghiệm thu không đầy đủ, dù kết quả 100% OK trông rất thuyết phục trên giấy — nó chỉ đo
> được "trạm có nhận ra hàng tốt không", không đo được "trạm có phân biệt được tốt/xấu không" (xem
> Chương 16, mục 16.3.1).

## Phần 5: Bàn giao & bảo trì định kỳ

- [ ] Ghi lại mốc trạng thái hiện tại trước khi hiệu chỉnh lại lần đầu tiên: working distance,
  góc lắp camera/đèn/chi tiết, khẩu độ lens, cấu hình strobe, exposure/gain đang dùng, và một ảnh
  tham chiếu chụp ngay tại thời điểm bàn giao (xem Chương 16, mục 16.4.4)
- [ ] Thiết lập ghi log metric của **mọi** cycle (score, chỉ số ánh sáng, thời gian xử lý), không
  chỉ lưu ảnh của cycle NG (xem Chương 16, mục 16.4.1)
- [ ] Thiết lập cảnh báo sớm theo xu hướng (ví dụ trung bình trượt so với mốc lúc nghiệm thu),
  không chỉ theo ngưỡng tức thời tại từng cycle đơn lẻ (xem Chương 16, mục 16.4.2)
- [ ] Khi phát hiện xuống cấp, xác định đúng loại trôi theo Bảng 16.2 (Cường độ / Tịnh tiến / Xoay
  / Tỉ lệ) trước khi chạm vào bất kỳ tham số nào (xem Chương 16, mục 16.4.4)
- [ ] Retrain pattern theo quy trình có kiểm soát: backup job cũ, teach trên mẫu đại diện lô mới,
  chạy thử lại trên golden set trước khi thay thế job đang chạy sản xuất (xem Chương 16, mục
  16.4.3)
- [ ] Bàn giao màn teach cho kỹ sư hiện trường — train lại pattern được từ ứng dụng vận hành,
  không cần mở QuickBuild (xem Chương 14, mục 14.4.2)
- [ ] Thiết lập chính sách lưu ảnh và xoá vòng để tránh tràn đĩa sau vài tuần vận hành liên tục
  (xem Chương 14, mục 14.5)
- [ ] Chọn đèn có dự phòng công suất và ghi lại thông số driver ban đầu — LED mờ dần theo thời
  gian chứ không hỏng đột ngột, nên "trạng thái lúc mới lắp đặt" phải được ghi lại trước khi nó
  trở thành thông tin không thể khôi phục (xem Chương 2, mục 2.2.7)

\newpage

# Phụ lục C — Troubleshooting theo triệu chứng

Phụ lục này tổng hợp lại toàn bộ mục "Lỗi thường gặp" ở cuối 16 chương, nhưng sắp xếp theo **triệu
chứng người vận hành thực tế nhìn thấy** thay vì theo thứ tự chương. Khi gặp sự cố, tra theo hiện
tượng đang thấy (score tụt, đo nhảy số, đọc mã fail, app rò bộ nhớ...), đọc nhanh các nguyên nhân
khả dĩ được xếp từ **dễ/rẻ kiểm tra trước** đến phức tạp hơn, rồi mở đúng chương/mục được trỏ tới để
đọc giải thích đầy đủ. Một nguyên nhân gốc có thể gây nhiều triệu chứng khác nhau nên xuất hiện ở
nhiều nhóm — đây là lựa chọn có chủ đích để tra cứu nhanh hơn, không phải trùng lặp ngoài ý muốn.

### 1. Định vị sai / không tìm thấy chi tiết

**Triệu chứng: Tool định vị (PMAlign) hoặc caliper báo "không tìm thấy" dù mắt thường vẫn thấy chi
tiết trong ảnh, đặc biệt khi chi tiết lệch về phía biên khung hình hoặc biên vùng tìm kiếm.**
- FOV tính khít đúng bằng kích thước chi tiết, không cộng thêm biên độ xê dịch/dung sai gá đặt thực
  tế (xem Chương 2, mục 2.4.2)
- Region của caliper vẽ theo vị trí chi tiết lúc setup, không phủ hết dải xê dịch đã fixture cho
  phép (xem Chương 9; Chương 7, mục 7.4)
- Code lấy `Results[0]` mà không kiểm `Count` trước — khi chi tiết vắng mặt một cycle (băng tải
  trống, cấp liệu trễ), chương trình văng exception thay vì báo "không tìm thấy" tường minh (xem
  Chương 8, mục 8.4; Chương 7, mục 7.4)

**Triệu chứng: Thỉnh thoảng một cycle cho ra toàn bộ kết quả đo "hợp lý nhưng sai", log cho thấy
score định vị của đúng cycle đó bằng không.**
- Chuỗi xử lý không kiểm `Results.Count` ngay sau align nên vẫn chạy tiếp với transform/pose của
  cycle trước khi align cycle hiện tại thất bại — cần dừng chuỗi và báo NG-không-tìm-thấy, phân
  biệt với NG-đo-hỏng (xem Chương 7, mục 7.4; Chương 15, mục 15.4)

**Triệu chứng: Job định vị chạy chậm hơn tính toán, hoặc thỉnh thoảng bắt nhầm hướng đối xứng của
chi tiết.**
- Angle range mở rộng "cho chắc" (ví dụ ±180°) vượt xa phạm vi xoay thực tế của chi tiết trên
  pallet — vừa tốn thời gian quét vừa tăng khả năng nhầm hướng; đo phạm vi xoay thực tế rồi khai
  báo đúng mức đó cộng biên an toàn vừa phải (xem Chương 8, mục 8.3)

**Triệu chứng: Score PMAlign cao khi thử trong phòng lab nhưng tụt hoặc dao động thất thường ngoài
dây chuyền dù chi tiết trông giống hệt mẫu.**
- Train cả bóng đổ hoặc nền không ổn định vào pattern — `TrainRegion` + mask không sát chi tiết
  thật, ánh sáng lúc train chưa nhất quán (xem Chương 8; Chương 2)
- Đã hạ `AcceptThreshold` để "hết báo lỗi" thay vì điều tra nguyên nhân gốc (ánh sáng, cơ khí, lô
  hàng) — dấu hiệu là false accept tăng dần, đôi khi robot gắp sai vị trí mà không cảnh báo trước
  (xem Chương 8, mục 8.5.2; Chương 16)

### 2. Đo lường sai / nhảy số / không ổn định (Caliper, Calibration)

**Triệu chứng: Kết quả đo dao động lớn giữa các lần đo cùng một chi tiết đứng yên, không lặp lại
được.**
- Polarity của caliper để `DontCare` dù đã biết trước chiều sáng-tối từ cách bố trí đèn — tool
  thỉnh thoảng bắt nhầm biên bóng đổ thay vì biên chi tiết thật; khai báo đúng
  `Edge0Polarity`/`Edge1Polarity` (xem Chương 9; Chương 2)
- Đang dùng khoảng cách toạ độ lấy từ PMAlign để đo dung sai — PMAlign tối ưu cho định vị bền vững
  (chịu xoay/scale/nhiễu), không tối ưu độ lặp lại sub-pixel; dùng caliper cho mọi phép đo có dung
  sai, dành PMAlign cho định vị/fixturing (xem Chương 9; Chương 7, Chương 8)
- Chưa kiểm `RMSError`/`NumPointsFound` trước khi chấp nhận kết quả FindLine/FindCircle — đường/
  tròn vẫn khớp về mặt toán học ngay cả khi phần lớn điểm biên không tìm thấy hoặc lệch xa (xem
  Chương 9, Code 9.2)

**Triệu chứng: Phát hiện "cạnh" tại những vị trí không có biên vật lý nào, thường lẻ tẻ và không
lặp lại giữa các lần chụp.**
- Nhầm đốm nhiễu với cạnh thật — gradient nhạy với mọi thay đổi cường độ đột ngột bất kể nguồn gốc
  (hạt bụi, nhiễu cảm biến); làm mượt nhẹ trước khi tính gradient, đặt ngưỡng độ lớn gradient tối
  thiểu (xem Chương 4, mục 4.3.1)

**Triệu chứng: Kết quả đo lệch có hệ thống — luôn lớn hơn hoặc luôn nhỏ hơn giá trị thật một lượng
gần như cố định, không phải nhiễu ngẫu nhiên.**
- Dùng Erosion/Dilation đơn lẻ để "dọn nhiễu" trước khi đo diện tích — chỉ Open/Close (cặp đôi) mới
  giữ nguyên kích thước thật, Erosion/Dilation đơn lẻ làm sai lệch số đo (xem Chương 4, mục 4.3.2)
- Đo trực tiếp trên ảnh chưa calibration rồi nhân một hệ số tay (px × k) — distortion và camera
  nghiêng làm tỉ lệ không đều theo vị trí trong ảnh, một hệ số không mô tả nổi: đúng ở giữa ảnh, sai
  dần về mép (xem Chương 7)
- Dạy bàn cờ calibration ở cao độ khác mặt phẳng đặc trưng cần đo — phối cảnh đổi theo cao độ, đo
  pallet thì chuẩn nhưng đo mặt trên chi tiết có bề dày thì lệch hệ thống, càng xa tâm ảnh càng lệch
  (xem Chương 7)
- Dùng lens thường (không telecentric) để đo chính xác trên chi tiết có bề dày — sai số phối cảnh
  giữa mặt phẳng calibration và mặt phẳng đo; tính trước sai số dự kiến theo Δh/WD, vượt đáng kể so
  với dung sai thì chuyển sang telecentric (xem Chương 2, mục 2.5.1)

**Triệu chứng: RMS calibration đẹp nhưng sai số đo thực tế lại lớn ở một vùng của ảnh.**
- Điểm calibration dồn về một góc, vùng làm việc thật nằm ngoài đa giác điểm chuẩn — mô hình đang
  ngoại suy ra ngoài vùng có dữ liệu; điểm chuẩn phải trải rộng bao trùm vùng làm việc, nghi ngờ thì
  thêm điểm kiểm chứng độc lập (xem Chương 7)

**Triệu chứng: Nghiệm thu "đo đúng" nhưng sản xuất thực tế có tỉ lệ lọt hàng lỗi không giải thích
được.**
- Nghiệm thu trạm đo chỉ bằng độ chính xác trung bình, bỏ qua độ lặp lại — độ chính xác trung bình
  tốt có thể che giấu độ lặp lại kém; đo repeatability bằng khoảng 30 lần chụp một chi tiết cố định
  trước khi chấp nhận trạm (xem Chương 9, mục 9.4.2; Chương 16)

### 3. Đọc mã / ký tự thất bại hoặc đọc sai (ID Tools, OCRMax)

**Triệu chứng: Job đọc mã 1D/2D chậm hơn cần thiết, thỉnh thoảng "đọc" ra một mã không tồn tại.**
- Bật tất cả symbology "cho chắc" — mỗi symbology bật thêm là một không gian tìm kiếm thêm, vừa
  chậm vừa tăng khả năng dương tính giả; chỉ bật đúng symbology chi tiết thực sự mang (xem Chương
  11, Code 11.1)

**Triệu chứng: Đọc được mã DPM (dot-peen/laser) ổn định trong phòng lab nhưng không đọc được ổn
định trên dây chuyền thật.**
- Dùng chiếu sáng tiêu chuẩn thay vì dark field/chiếu góc thấp — DPM cần tương phản từ chênh lệch
  độ nhám bề mặt, ánh sáng đồng đều không tạo ra tương phản đó (xem Chương 11, mục 11.1.2; Chương 2)

**Triệu chứng: OCR nhận diện tốt trên ảnh test tự tạo bằng công cụ vẽ chữ nhưng kém trên chi tiết
thật.**
- Train font OCR bằng font hệ thống thay vì mẫu ký tự khắc/đột dập thật — hình dạng ký tự thực tế
  khác đáng kể font chữ máy tính chuẩn; luôn train bằng ảnh chụp ký tự thật trên chi tiết, nhiều
  biến thể (xem Chương 11, Code 11.2, mục 11.3.2)

**Triệu chứng: Một tỉ lệ nhỏ bản ghi traceability có mã sai một ký tự (thường là cặp dễ nhầm như
O/0, I/1) lọt qua mà không có cảnh báo.**
- Không bật Fielding dù định dạng chuỗi đã biết trước — không có ràng buộc cấu trúc để loại kết quả
  sai; bật `FieldingEnabled` với `FieldString`/độ dài đúng theo định dạng thực tế (xem Chương 11,
  mục 11.3.3)

**Triệu chứng: Một khoảng lô hàng "biến mất" khỏi hồ sơ truy vết dù trạm vẫn báo chạy bình thường —
thường phát hiện rất muộn, khi cần tra cứu mới biết thiếu.**
- Gửi bản ghi lên MES/host theo kiểu "bắn rồi quên" (fire-and-forget), không kiểm tra phản hồi từ
  host — xác nhận nhận thành công hoặc lưu cục bộ chờ gửi lại (xem Chương 11; Chương 14)

### 4. Đếm / phát hiện khuyết tật sai (Blob)

**Triệu chứng: Ngưỡng nhị phân hoá tự động (Otsu) ra một con số cắt sai vị trí, không đúng biên đối
tượng.**
- Tin Otsu mù quáng khi histogram không thật sự hai đỉnh — Otsu giả định phân bố hai lớp rõ rệt,
  giả định đó vỡ khi đối tượng chiếm diện tích quá nhỏ so với nền hoặc ảnh có từ ba vùng độ sáng trở
  lên; luôn nhìn lại hình dạng histogram trước khi tin ngưỡng tự động, cân nhắc threshold theo vùng
  con (xem Chương 4, mục 4.1-4.2)

**Triệu chứng: Job đếm đúng số lượng vào một thời điểm trong ngày nhưng đếm sai vào ca/giờ khác dù
không ai đổi cấu hình.**
- Dùng `SetSegmentationHardFixedThreshold` với một số cố định — không theo kịp thay đổi độ sáng nền
  thực tế (đèn xuống cấp, ánh sáng môi trường); cân nhắc threshold tương đối/động, giám sát chỉ số
  ánh sáng theo thời gian (xem Chương 10; Chương 2; Chương 12, mục 12.2)

**Triệu chứng: Đếm được nhiều hơn số lượng thật — một đối tượng bị tách thành 2-3 blob.**
- Quên bước morphology "hàn lại" các mảnh vỡ do phản xạ/nhiễu cục bộ cắt ngang blob — thêm
  `CloseSquare` (hoặc biến thể phù hợp hướng nhiễu) trước khi đo, nhưng kiểm ngưỡng đủ để không nối
  nhầm hai đối tượng khác nhau (xem Chương 10, mục 10.1.2)

**Triệu chứng: Lỗi thiếu linh kiện lọt qua dù job báo "đủ" hoặc "thừa" số lượng blob.**
- Đặt ngưỡng đếm quá lỏng kiểu "≥ N" thay vì "= N chính xác", không kiểm hình dạng từng blob — luôn
  kiểm cả số lượng blob hợp lệ VÀ diện tích/hình dạng từng blob nằm trong dải kỳ vọng (xem Chương
  10, Code 10.2)

**Triệu chứng: Dùng phép trừ ảnh, "khuyết tật giả" xuất hiện dọc theo mọi biên thật của chi tiết,
không tập trung ở vị trí lỗi thực.**
- Trừ ảnh khi chi tiết chưa fixture đúng — lệch dù rất nhỏ giữa ảnh chuẩn và ảnh kiểm tạo hiệu số
  lớn tại mọi cạnh có gradient cao; đảm bảo chuỗi Calibration → Fixture chạy trước Blob trong mọi
  trường hợp trừ ảnh (xem Chương 10; Chương 7)

**Triệu chứng: Một loại khuyết tật cụ thể không bao giờ bị phát hiện, dù mắt người thấy rõ trên
nhiều chi tiết khác nhau.**
- Ảnh chuẩn (golden image) dùng để trừ không thật sự "sạch" — khuyết tật đó vô tình có mặt trên
  chính ảnh chuẩn nên phép trừ luôn coi nó là bình thường; kiểm tra kỹ ảnh chuẩn bằng nhiều phương
  pháp độc lập trước khi chốt, cân nhắc ảnh chuẩn tổng hợp (trung bình nhiều chi tiết tốt) thay vì
  một chi tiết đơn lẻ (xem Chương 10)

### 5. Ảnh quá sáng/tối/nhoè/không ổn định theo thời gian, hoặc tham số acquisition "không có tác
dụng"

**Triệu chứng: Thêm hết lớp lọc/morphology này đến lớp khác mà kết quả vẫn không ổn định, hoặc "ổn
định" được nhưng chi tiết thật cũng bị mờ/biến dạng theo.**
- Lạm dụng filter/morphology để xử lý phần ngọn thay vì sửa ánh sáng vật lý ở gốc — không tổ hợp xử
  lý ảnh phần mềm nào tạo ra tương phản mà ảnh gốc không có; ưu tiên sửa ánh sáng vật lý trước, coi
  xử lý ảnh là bước tinh chỉnh cuối cùng trên một ảnh đã đủ tốt (xem Chương 4)

**Triệu chứng: Job chạy ổn định lúc setup nhưng liên tục phải "vá" bằng threshold/contrast mỗi khi
đưa lên máy thật.**
- Ánh sáng dùng lúc dựng job là ánh sáng phòng/đèn tạm, chưa chốt kỹ thuật chiếu sáng — không tham
  số phần mềm nào bù được tín hiệu đầu vào vốn đã kém; chốt chiếu sáng và xử lý nhiễu môi trường
  trước khi mở QuickBuild dựng job đầu tiên (xem Chương 2, mục 2.2, 2.3)

**Triệu chứng: Trạm chạy tốt khi nghiệm thu (thường một thời điểm cố định trong ngày) nhưng NG hàng
loạt vào ca/giờ khác.**
- Coi ánh sáng môi trường là hằng số, không triển khai tuyến phòng thủ nào chống nhiễu — nghiệm thu
  qua đủ các khung giờ vận hành thực tế, mặc định đưa ít nhất một trong ba tuyến phòng thủ (shroud/
  filter/strobe) vào thiết kế cho mọi trạm gần nguồn sáng biến động (xem Chương 2, mục 2.3.4)

**Triệu chứng: Ảnh đủ sáng trên màn hình nhưng nhiễu hạt nhiều, caliper/PMAlign hoạt động không ổn
định.**
- Tăng gain để bù thiếu sáng thay vì sửa nguồn sáng — gain khuếch đại cả nhiễu nền, không thêm
  thông tin thật như tăng exposure hay cải thiện đèn; coi gain cao thường trực là dấu hiệu cảnh báo
  cần điều tra lại nguồn sáng (xem Chương 3, mục 3.3.2)

**Triệu chứng: Ảnh bị nghiêng/xé, hoặc kết quả đo dao động lớn giữa các lần chụp cùng một chi tiết
dù "đã dừng" theo tín hiệu cảm biến vị trí.**
- Dùng camera rolling shutter cho vật còn dư chấn chuyển động, thời gian settle sau khi dừng cơ khí
  chưa đủ để triệt tiêu dư chấn — đo thực nghiệm thời gian settle cần thiết, với trạm không chắc
  chắn tuyệt đối thì chọn global shutter làm phương án an toàn mặc định (xem Chương 3, mục 3.3.4)

**Triệu chứng: Ảnh sáng không đều — một phần khung hình đủ sáng, phần còn lại tối hơn hẳn, dù
Exposure/Gain đặt đúng theo tính toán.**
- Strobe không nằm trọn trong cửa sổ exposure (`StrobePulseDuration` ngắn hơn `Exposure`, hoặc
  `StrobeDelay` lệch), hoặc dùng chung với camera rolling shutter — đặt `StrobePulseDuration ≥
  Exposure`, kiểm chứng bằng timing diagram thật trước khi đưa vào sản xuất, mặc định dùng global
  shutter cho mọi trạm kết hợp strobe với vật chuyển động (xem Chương 3, mục 3.5.3; Chương 6, Hình
  6.2)

**Triệu chứng: Set `Exposure`/`Gain`/trigger từ code xong, ảnh vẫn ra như cấu hình cũ, không có lỗi
nào báo.**
- Quên gọi `Prepare()` sau khi đổi tham số acquisition — các property trên `ICogAcqFifo` chỉ thay
  đổi trạng thái trong bộ nhớ, `Prepare()` mới thực sự ghi cấu hình xuống camera; coi `Prepare()` là
  bước bắt buộc ngay sau mọi lần đổi tham số, không phải bước tuỳ chọn (xem Chương 6, mục 6.2.2)

**Triệu chứng: Chỉ số giám sát ánh sáng (histogram) dao động mạnh giữa các cycle dù ánh sáng thực
tế ổn định.**
- Region của `CogHistogramTool` trộn tín hiệu "ánh sáng đổi" với "chi tiết khác nhau" — đo trên
  vùng nền cố định, không chứa chi tiết (xem Chương 12, mục 12.2.1)

### 6. Ứng dụng C# treo / rò bộ nhớ / crash

**Triệu chứng: Lỗi tải assembly ngay khi khởi động ứng dụng, thông báo không rõ ràng về nguyên
nhân.**
- Trộn Platform Target `AnyCPU` với DLL VisionPro 64-bit — DLL native gắn chặt với kiến trúc CPU cụ
  thể; đặt Platform Target = x64 tường minh cho toàn bộ solution (xem Chương 13, mục 13.1.1)

**Triệu chứng: Trạm sản xuất treo vô thời hạn không rõ nguyên nhân, thường sau một lần sửa script
vội.**
- Để lại MessageBox hoặc điểm chờ tương tác người dùng trong script debug, chưa dọn trước khi đưa
  job vào sản xuất — dùng log/`ScriptError` thay MessageBox, rà lại script trước khi chốt (xem
  Chương 12, mục 12.5.3)

**Triệu chứng: Bộ nhớ ứng dụng tăng dần đều trong ca sản xuất kéo dài, không có exception nào báo
lỗi.**
- Tưởng `ICogImage` có sẵn `Dispose()` — thực tế `ICogImage` không kế thừa `IDisposable`, gán biến
  về `null` không giải phóng bộ nhớ unmanaged phía sau; luôn ép kiểu `as IDisposable` hoặc dùng kiểu
  cụ thể (xem Chương 13, mục 13.6.1)

**Triệu chứng: Ổ đĩa lưu ảnh đầy, ứng dụng crash hoặc ngừng ghi log âm thầm giữa ca sản xuất.**
- Lưu mọi ảnh OK, không phân biệt "cần lưu để điều tra" (NG/Warning) với "không cần lưu" (OK),
  không có chính sách xoá vòng — mặc định chỉ lưu NG/Warning kèm metadata, đặt giới hạn dung lượng
  và thời gian giữ tường minh (xem Chương 14, mục 14.5)

**Triệu chứng: Exception cross-thread ngẫu nhiên, hoặc UI treo/vẽ sai.**
- Gán `CogRecordDisplay.Record` trực tiếp từ luồng xử lý ảnh, không dispatch về UI thread — control
  WinForms/WPF chỉ được cập nhật an toàn từ UI thread tạo ra nó; dispatch qua `Control.Invoke`/
  `Dispatcher.InvokeAsync` trước khi gán (xem Chương 13, mục 13.5.2)

**Triệu chứng: Kết quả sai lệch ngẫu nhiên hoặc exception hiếm gặp khó tái hiện, đặc biệt dưới tải
cao.**
- Gọi `Run()` trên cùng một tool từ hai luồng — tool không thread-safe, hai luồng cùng truy cập
  trạng thái nội bộ không được bảo vệ; giữ mô hình một tool/một luồng, tách acquisition và xử lý
  bằng hàng đợi thay vì gọi tool từ nhiều luồng (xem Chương 13, mục 13.7)

### 7. Giao tiếp PLC/robot treo hoặc timeout

**Triệu chứng: Dây chuyền đứng im vô thời hạn khi vision gặp sự cố (mất kết nối camera, phần mềm
treo), không có cảnh báo nào.**
- Chỉ đặt timeout phía vision, thiết kế giả định PLC "chờ vô hạn là bình thường" — cả hai phía đều
  phải tự timeout mọi điểm chờ tín hiệu từ bên kia (xem Chương 15, mục 15.1.3)

**Triệu chứng: Một cycle "treo" bất thường lâu hơn hẳn bình thường, làm lệch nhịp cả dây chuyền, dù
không có lỗi nào được báo rõ ràng.**
- Không bật `TimeoutEnabled` cho tool định vị (PMAlign) — điều kiện ảnh xấu khiến tool "cố tìm"
  không giới hạn thời gian; bật Timeout với ngân sách nằm trong cycle time đã tính (xem Chương 8,
  mục 8.4; Chương 15)
- Vòng lặp retry khi không tìm thấy chi tiết không có giới hạn số lần, cố chụp lại đến khi thành
  công — giới hạn số lần retry rõ ràng, tính thời gian retry vào ngân sách cycle, hết giới hạn thì
  báo lỗi tường minh thay vì tiếp tục thử (xem Chương 15, mục 15.5.2)

**Triệu chứng: Lỗi timeout ngẫu nhiên, khó tái hiện, xuất hiện thưa thớt trong sản xuất dù test
trên bàn không thấy.**
- Cam kết cycle time theo giá trị trung bình, bỏ qua phân bố đuôi dài (GC, retry, tải hệ thống) —
  đo qua hàng nghìn cycle, cam kết theo P99/giá trị lớn nhất quan sát được, không theo trung bình
  (xem Chương 15, mục 15.2.2)

**Triệu chứng: Robot di chuyển đến vị trí vô nghĩa, hoặc đến đúng X, Y nhưng xoay sai/di chuyển
ngược chiều so với kỳ vọng.**
- Gửi toạ độ không kèm cờ hợp lệ hoặc không kiểm tra biên khi vision không tìm thấy chi tiết — luôn
  kiểm biên và đặt `Valid = false` tường minh khi không có kết quả tin cậy, robot/PLC cũng tự kiểm
  tra lại phía nhận (xem Chương 15, mục 15.3.3; Chương 7, mục 7.5)
- Gửi robot góc theo radian trong khi phía nhận chờ độ, hoặc `SwapCalibratedHandedness` không khớp
  cách nhập điểm — quy ước đơn vị ghi thành văn bản trong hợp đồng bản tin, làm phép thử dịch
  chuyển thật theo từng trục sau mỗi lần calibration trước khi cho chạy tự động (xem Chương 7, mục
  7.5; Chương 15)

**Triệu chứng: Không thể phân biệt xu hướng "chất lượng sản phẩm giảm" với xu hướng "vision đang
xuống cấp" chỉ từ tỉ lệ NG tổng.**
- Gộp mọi loại thất bại thành một cờ NG duy nhất ngay từ thiết kế bản tin — phân loại rõ Accept/
  Warning/Reject/Error, tách NG-đo-hỏng khỏi NG-không-tìm-thấy; sửa cấu trúc bản tin sớm rẻ hơn
  nhiều so với sửa sau khi đã tích luỹ dữ liệu lịch sử không phân loại được (xem Chương 15, mục
  15.4.2)

### 8. Job/trạm chạy chậm hơn dự kiến, không đạt cycle time

**Triệu chứng: Tool định vị/đọc mã chạy chậm hơn tính toán ban đầu.**
- Angle range của PMAlign mở quá rộng "cho chắc" — xem chi tiết ở Nhóm 1, "Triệu chứng: Job định vị
  chạy chậm..." (xem Chương 8, mục 8.3)
- Bật tất cả symbology đọc mã "cho chắc" thay vì đúng loại mã chi tiết mang — xem chi tiết ở Nhóm 3
  (xem Chương 11, Code 11.1)

**Triệu chứng: Frame rate tụt, ảnh rớt khung, hoặc lỗi timeout thu ảnh ngẫu nhiên khi thêm camera
thứ hai vào cùng hệ thống mạng.**
- Nhiều camera chia sẻ một switch/NIC vượt quá băng thông GigE khả dụng (~100–115 MB/s một liên kết
  Gigabit dùng chung) — cắm mỗi camera vào một cổng NIC vật lý riêng khi có thể, hoặc giảm fps/ROI/
  kích thước ảnh (xem Chương 3, mục 3.4.2; Chương 6, mục 6.5)

**Triệu chứng: Một giải pháp "chạy tốt trên tập test" nhưng không đáp ứng nổi cycle time thực tế
khi đưa vào sản xuất.**
- Nhầm bài toán nghiên cứu (tối đa hoá độ chính xác, không ràng buộc thời gian/môi trường) với bài
  toán vận hành sản xuất (ổn định, đúng hạn, lặp lại được) — luôn nhìn giải pháp qua ba ràng buộc
  công nghiệp trước khi đánh giá "tốt" hay "chưa đủ tốt" (xem Chương 1, Bảng 1.1)

### 9. Kết quả tốt lúc nghiệm thu nhưng xấu dần theo thời gian vận hành

**Triệu chứng: Nghiệm thu đạt tuyệt đối, tỉ lệ lỗi thực tế tăng dần sau vài tuần vận hành.**
- Dùng chung một bộ ảnh vừa để mò tham số lúc phát triển vừa để "chứng minh" job hoạt động, không
  tách golden set riêng — bộ ảnh này bị thiên vị bởi chính quá trình chỉnh tham số dựa trên nó; giữ
  riêng một bộ golden set không dùng để tinh chỉnh tham số (xem Chương 6, Code 6.7; Chương 16, mục
  16.3)
- Golden set nghiệm thu chỉ gồm vài chi tiết "đẹp", không đại diện cho biến thiên thật (dung sai, lô
  hàng, ánh sáng theo ca) — golden set phải đủ số lượng, đa dạng, có cả chi tiết lỗi thật (xem
  Chương 16, mục 16.3.1)
- Bộ ảnh mẫu dùng phát triển job được chụp vội bằng tay, không phản ánh đúng dải biến thiên thật của
  exposure/trigger/vị trí chi tiết — chụp bộ ảnh mẫu sau khi đã cấu hình acquisition đúng trên chính
  camera/đèn sẽ dùng thật, chủ động chụp cả trường hợp xấu/biên (xem Chương 6, mục 6.2)
- Đã hứa hẹn hệ vision phát hiện mọi loại khuyết tật có thể xảy ra, kể cả loại chưa từng gặp trong
  mẫu thử — một hệ vision chỉ nhận diện tốt những gì đã được dạy/lập luật để nhận ra; phát biểu rõ
  phạm vi phát hiện ngay từ đầu, nghiệm thu bằng bộ mẫu đại diện đủ rộng (xem Chương 1; Chương 16,
  mục 16.3)

**Triệu chứng: Trạm chạy tốt nhiều tháng rồi sai số đo tăng dần sau một lần bảo trì/va chạm mà
không ai để ý.**
- Coi calibration là "làm một lần là xong" — lens xoay nhẹ, camera xê dịch, pallet mòn khiến mô
  hình cũ không còn tả đúng hiện trạng; đưa "kiểm tra calibration" vào lịch bảo trì (chụp bàn cờ/
  điểm chuẩn, so RMS với lần trước), lưu ảnh + RMS mỗi lần dạy làm hồ sơ (xem Chương 7; Chương 16)

**Triệu chứng: Dành nhiều thời gian "tối ưu" threshold mà tỉ lệ sai không cải thiện đáng kể.**
- Tinh chỉnh threshold khi phân bố OK/NG chồng lấn nhiều — vấn đề thực chất nằm ở tầng thu ảnh/đo
  lường (ánh sáng, độ phân giải, độ nhiễu), threshold không sửa được vấn đề ở tầng khác; kiểm độ
  chồng lấn phân bố trước khi tối ưu threshold, chồng lấn nhiều thì quay lại phần thu ảnh/đo lường
  thay vì tiếp tục chỉnh số (xem Chương 16, mục 16.2.2)

**Triệu chứng: Lỗi hàng loạt xuất hiện đột ngột dù "không có cảnh báo nào trước đó".**
- Code xử lý kết quả chỉ kiểm `== Accept` rồi coi mọi thứ khác là NG, bỏ lỡ khả năng phân biệt
  Warning với Reject/Error — xử lý đủ 4 nhánh `CogToolResultConstants`, tận dụng Warning cho giám
  sát xu hướng (xem Chương 12, mục 12.3.3; Chương 16)
- Hệ thống chỉ so sánh từng điểm dữ liệu với ngưỡng cố định, không theo dõi đường xu hướng — xu
  hướng xuống cấp đã âm thầm diễn ra nhiều ngày/tuần trước, chỉ chưa chạm ngưỡng cảnh báo tức thời;
  ghi metric mọi cycle, theo dõi trung bình trượt so với baseline lúc nghiệm thu (xem Chương 16, mục
  16.4.1-16.4.2)

**Triệu chứng: Retrain vội để "chữa cháy" khi thấy lỗi tăng, đôi khi làm tình hình tệ hơn.**
- Retrain trực tiếp trên máy sản xuất theo phản xạ, bỏ qua bước backup + chạy thử trên golden set
  trước khi thay job — mẫu train mới chưa được kiểm chứng; retrain luôn đi qua quy trình đầy đủ dù
  áp lực thời gian lớn đến đâu (xem Chương 16, mục 16.4.3; Chương 14, mục 14.4)

### 10. Chọn sai/thiếu phần cứng hoặc phạm vi bài toán ngay từ khâu thiết kế

**Triệu chứng: Phát hiện muộn — khi đã dựng job hoặc lắp đặt thật — rằng độ phân giải không đủ cho
dung sai đo, hoặc ống kính sai working distance so với không gian lắp đặt.**
- Chọn camera theo "chấm MP" thay vì tính ngược độ phân giải cần thiết từ dung sai đo và FOV cụ thể
  — quyết định dựa trên cảm tính "càng nhiều MP càng tốt"; luôn chạy phép tính độ phân giải cần
  thiết trước khi chọn cảm biến (xem Chương 3, mục 3.2)
- Mua camera/ống kính "đa năng" trước khi phân tích bài toán GIGI, dung sai yêu cầu và ràng buộc
  không gian lắp đặt thật — coi phần cứng vision như phụ kiện mua sắm thông thường; luôn xuất phát
  từ bài toán trước khi hỏi giá bất kỳ thiết bị nào (xem Chương 1, mục 1.2)

**Triệu chứng: Một mô hình "chạy tốt trên tập test" nhưng không ổn định khi ánh sáng nhà xưởng thay
đổi theo ca, hoặc đầu tư đáng kể vào deep learning cho một bài toán rule-based có thể giải quyết với
chi phí thấp hơn nhiều.**
- Đánh đồng machine vision với AI/computer vision nói chung, bỏ qua ba ràng buộc công nghiệp (ổn
  định, đúng hạn, lặp lại được) — nhìn giải pháp vision qua lăng kính ba ràng buộc trước khi đánh
  giá (xem Chương 1, Bảng 1.1)
- Chuyển sang deep learning mặc định khi rule-based "khó" (chưa được thử đủ kỹ) thay vì thực sự
  "không thể" — xác nhận đã chạm đúng một trong ba dấu hiệu giới hạn rule-based và cân nhắc đủ ba
  chi phí thật trước khi quyết định (xem Chương 10, mục 10.5; Chương 16, mục 16.5.3)

### 11. Lỗi tổ chức file .vpp / project VisionPro / kiến trúc ứng dụng C#

**Triệu chứng: Exception "not found" khi gọi terminal từ C# sau một lần chỉnh sửa job tưởng chừng
vô hại.**
- Tên terminal là chuỗi, không được compiler kiểm tra tại thời điểm build; terminal bị đổi tên
  trong QuickBuild mà quên cập nhật code — đặt tên rõ nghĩa ngay từ đầu, coi terminal đã đặt tên là
  một phần hợp đồng ổn định, dùng hằng số tên terminal tập trung một nơi và test tự động xác nhận
  bộ terminal ngay sau khi nạp job (xem Chương 5, mục 5.4; Chương 13, mục 13.3.1)

**Triệu chứng: Job có hàng chục tool nối dây chằng chịt trên một canvas, khó hiểu lại sau vài
tháng, khó tái sử dụng một phần logic cho job khác.**
- Dồn toàn bộ logic vào một job khổng lồ, không dùng ToolBlock để đóng gói — tách theo giai đoạn xử
  lý tự nhiên (Acquisition → Calibration → Align/Fixture → Inspect) thành các ToolBlock riêng, mỗi
  khối một trách nhiệm rõ ràng (xem Chương 5, mục 5.4.1)

**Triệu chứng: Cần điều chỉnh lại một tool sau này nhưng không còn ảnh gốc đã dùng lúc thiết kế ban
đầu để so sánh/kiểm chứng.**
- Chỉ giữ file `.vpp`, không lưu riêng bộ ảnh mẫu đã dùng để phát triển nó — luôn lưu lại bộ ảnh mẫu
  đại diện (không chỉ ảnh "đẹp nhất") cùng với job (xem Chương 5, mục 5.6)

**Triệu chứng: Một lần chỉnh sửa/teach lại làm job tệ đi, không có đường lùi về job cũ.**
- Ghi đè `.vpp` production (qua QuickBuild hoặc màn teach) mà không backup, coi nhẹ vì "chỉ là file
  cấu hình" — `.vpp` là file nhị phân, không có lịch sử "undo" qua nhiều phiên như code văn bản;
  backup tự động, bắt buộc, trước mọi lần ghi đè quan trọng, không tuỳ chọn tắt được từ UI (xem
  Chương 5, mục 5.6; Chương 14, mục 14.4.2)

**Triệu chứng: Một quyết định nghiệp vụ quan trọng (ví dụ điều kiện gửi robot chạy) không ai nhớ
khi debug sự cố từ phía code ứng dụng.**
- Nhét business logic quan trọng vào script bên trong `.vpp` — ranh giới "cái gì thuộc job, cái gì
  thuộc ứng dụng" không được tôn trọng; script chỉ nên tính toán/tổng hợp trong phạm vi tool block,
  quyết định nghiệp vụ nằm trong code C# có version control, review, test (xem Chương 12, mục
  12.4.3)

**Triệu chứng: Job chạy hoàn hảo trên máy dev, báo lỗi ngay khi mở trên máy sản xuất.**
- Script tham chiếu DLL ngoài không được triển khai kèm máy đích — `.vpp` không tự đóng gói
  dependency ngoài VisionPro; liệt kê rõ mọi dependency ngoài của script, đảm bảo triển khai cùng
  gói cài đặt (xem Chương 12, mục 12.5)

**Triệu chứng: `ViewModel`/logic recipe tham chiếu trực tiếp kiểu VisionPro, hoặc đổi model sản
phẩm đòi hỏi mở QuickBuild sửa `.vpp` dù chỉ là đổi một ngưỡng dung sai.**
- Trộn tham số job và tham số recipe — ngưỡng đáng ra thuộc recipe lại bị hard-code cứng trong job
  lúc thiết kế ban đầu; phân loại rõ tham số nào "thuộc job" (cấu trúc, thuật toán) và "thuộc
  recipe" (ngưỡng, số lượng kỳ vọng) ngay từ lúc thiết kế job (xem Chương 14, mục 14.3.1)
- Để kiểu VisionPro rò rỉ ra khỏi Engine (`using Cognex.VisionPro` xuất hiện ngoài project Engine) —
  tiện tay lấy giá trị trực tiếp từ terminal thay vì đi qua kết quả đã đóng gói; rà `using` trong
  mọi project ngoài Engine, không project nào khác được phép có `using Cognex.*` (xem Chương 14)

**Triệu chứng: Không thể viết unit test tự động cho tầng ứng dụng, phát triển tính năng mới bị
chặn khi trạm thật đang bận sản xuất.**
- Không có simulation mode — `IVisionEngine` chỉ có một triển khai gắn chặt phần cứng; luôn có
  `SimulatedVisionEngine` đọc từ bộ ảnh mẫu (xem Chương 14, mục 14.6.2)
