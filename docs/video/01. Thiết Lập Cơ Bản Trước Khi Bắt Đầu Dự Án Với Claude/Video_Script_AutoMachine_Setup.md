# 🎬 Mô Tả Video: Bộ File .md + Claude Cho Máy Tự Động Hoá C#
## "Thiết kế hệ thống trước khi viết code — Kỹ sư điện làm phần mềm"

---

## 📌 TIÊU ĐỀ VIDEO (chọn 1)

**Ngắn gọn:**
> Bộ file thiết kế máy tự động hoá C# với Claude AI | Setup trước khi viết 1 dòng code

**Keyword-driven:**
> CLAUDE.md + AI Rules cho máy công nghiệp C# | Kỹ sư điện xây dựng phần mềm tự động hoá

**Storytelling:**
> Tôi là kỹ sư điện và đây là cách tôi dùng Claude để thiết kế phần mềm máy trước khi code

---

## 📝 MÔ TẢ VIDEO (copy-paste lên YouTube)

```
Bạn là kỹ sư điện, PLC, hoặc tự động hoá muốn xây dựng phần mềm điều khiển 
máy công nghiệp bằng C# nhưng không biết bắt đầu từ đâu?

Trong video này tôi sẽ giới thiệu bộ file thiết kế hoàn chỉnh — CLAUDE.md, 
AGENTS.md, Rules, Skills, Hooks, Prompt Templates — giúp Claude AI luôn hiểu 
đúng dự án của bạn và viết code đúng chuẩn ngay từ đầu.

─────────────────────────────────────────
⚡ BẠN SẼ HỌC ĐƯỢC GÌ
─────────────────────────────────────────

✅ Tại sao cần setup trước khi code (và hậu quả nếu không setup)
✅ CLAUDE.md là gì và viết như thế nào cho máy công nghiệp
✅ 8 Agents chuyên biệt: hw-driver, seq-engineer, ui-builder, test-writer...
✅ Rules & Skills: AI tự biết dùng giao diện hay concrete class
✅ Hooks tự động: block nguy hiểm, format code, validate architecture
✅ Prompt Templates 11 loại: tạo driver, step, screen, test... chỉ điền chỗ trống
✅ Workflow hàng ngày: plan → confirm → implement → test → commit

─────────────────────────────────────────
📁 CÁC FILE ĐƯỢC GIỚI THIỆU TRONG VIDEO
─────────────────────────────────────────

📄 CLAUDE.md
   → File "bộ nhớ dự án" — Claude đọc tự động mỗi session
   → Chứa: stack, layer rules, 5 non-negotiables, code patterns

📄 AGENTS.md  
   → 8 agents chuyên biệt: mỗi agent = 1 chuyên môn
   → hw-driver | seq-engineer | ui-builder | service-engineer
   → test-writer | code-reviewer | refactor-cleaner | build-fixer

📄 .claude/rules/common/coding-standards.md
   → 14 universal rules: luôn áp dụng cho mọi file
   → R01 Safety-First → R14 Commit convention

📄 .claude/rules/csharp/csharp-patterns.md
   → 14 rules C# cụ thể: naming, async, EF Core, XAML...
   → Chỉ load khi làm việc với file .cs

📄 .claude/skills/ (4 skills)
   → am-hardware-patterns: template driver + simulator đầy đủ
   → am-sequence-patterns: IStep, MachineSequence template
   → am-wpf-mvvm: ViewModel + XAML + Prism Module chuẩn ISA-101
   → am-testing: xUnit + Moq + FluentAssertions với coverage targets

📄 .claude/hooks/ (9 hooks)
   → session-start: inject context tự động
   → bash-guard: block rm -rf, DROP TABLE
   → file-write-guard: block credential, concrete class trong WorkStation
   → stop-validator: force continue nếu arch violation
   → + 5 hooks khác

📄 PROMPT_TEMPLATES.md
   → 11 templates tái sử dụng: điền placeholder → code chuẩn ngay lần đầu

📄 QUICK_REFERENCE.md
   → In ra dán cạnh màn hình: snippets, checklist, alarm ranges

─────────────────────────────────────────
🏗️ KIẾN TRÚC AUTOMACHINE FRAMEWORK
─────────────────────────────────────────

Shell → Modules → Services → Hardware → Infrastructure → Data → Core

⭐ Phần duy nhất thay đổi khi làm máy mới:
   AM.WorkStation.{MachineName}

Phần giữ nguyên 100% qua mọi dự án:
   Hardware drivers | UI Modules | Services | Core

─────────────────────────────────────────
🛠️ STACK KỸ THUẬT
─────────────────────────────────────────

• C# 12 / .NET 8
• WPF + Prism (MVVM)
• DryIoc (Dependency Injection)
• SQLite + Entity Framework Core 8
• CommunityToolkit.Mvvm
• xUnit + Moq + FluentAssertions
• Claude Code / Cursor / GitHub Copilot

─────────────────────────────────────────
📥 TẢI FILES MIỄN PHÍ
─────────────────────────────────────────

🔗 am-ai-rules.zip (rules, agents, snippets, editorconfig)
🔗 ecc-automachine-full.zip (ECC-format CLAUDE.md, skills, commands)
🔗 am-hooks.zip (9 hooks tự động cho Claude Code)

Link tải trong phần bình luận được ghim ⬇️

─────────────────────────────────────────
⏱️ TIMESTAMPS
─────────────────────────────────────────

0:00 - Giới thiệu: Tại sao kỹ sư điện cần setup trước khi code
2:30 - Vấn đề thực tế khi không có hệ thống
5:00 - Tổng quan 12 files trong bộ setup
8:00 - CLAUDE.md: bộ nhớ dự án tự động
14:00 - AGENTS.md: 8 chuyên gia AI
22:00 - Rules & Skills: lazy loading thông minh
30:00 - Hooks: hệ thống bảo vệ tự động
40:00 - Prompt Templates: điền chỗ trống ra code chuẩn
47:00 - Demo workflow thực tế: từ 0 đến Step đầu tiên
55:00 - Q&A thường gặp

─────────────────────────────────────────
🔔 SERIES LIÊN QUAN
─────────────────────────────────────────

📺 [Tập 1] Setup môi trường Visual Studio 2022 cho máy tự động hoá
📺 [Tập 2] BẰNG NÀY FILE là video này ← BẠN ĐANG XEM
📺 [Tập 3] Viết hardware driver đầu tiên với Claude
📺 [Tập 4] Machine sequence: từ SFC đến C# State Machine  
📺 [Tập 5] WPF HMI theo chuẩn ISA-101
📺 [Tập 6] Unit testing cho máy công nghiệp
📺 [Tập 7] CI/CD và deploy lên máy thật

─────────────────────────────────────────
#automachine #csharp #claudeai #tự_động_hoá #wpf #dotnet
#industrial_automation #plc #hmi #kỹ_sư_điện #lập_trình_c_sharp
```

---

## 🎙️ SCRIPT VIDEO CHI TIẾT

---

### ĐOẠN MỞ ĐẦU (0:00 – 2:30)
**[Cảnh: màn hình Visual Studio trống, cursor nhấp nháy]**

> "Bạn là kỹ sư điện, bạn hiểu máy, hiểu PLC, hiểu I/O...
> Nhưng khi mở Visual Studio lên, bạn không biết bắt đầu từ đâu.
>
> Hoặc bạn đã bắt đầu — hỏi Claude, Claude trả lời, code chạy được —
> nhưng ba tuần sau nhìn lại, mỗi file một phong cách khác nhau,
> không có timeout trên hardware call, WorkStation gọi thẳng LtdmcController...
>
> Tôi đã ở đúng vị trí đó.
>
> Video này là bộ file tôi ước mình có từ đầu."

---

### PHẦN 1: Vấn đề thực tế (2:30 – 5:00)
**[Cảnh: code thực tế với các vấn đề được highlight]**

> "Khi bạn hỏi Claude mà không có context, Claude không biết:
> - Dự án bạn dùng kiến trúc gì
> - WorkStation chỉ được dùng interface, không được new() hardware trực tiếp
> - Tất cả async method phải có CancellationToken
> - Alarm phải là AlarmException, không phải Exception thường
>
> Kết quả: mỗi lần hỏi Claude ra một loại code khác nhau.
> Và Claude không bao giờ nhắc bạn những điều này nếu bạn không setup.
>
> Giải pháp: bộ file này."

---

### PHẦN 2: Tổng quan bộ file (5:00 – 8:00)
**[Cảnh: file explorer với cấu trúc thư mục]**

> "Bộ file gồm 3 nhóm:
>
> Nhóm 1 — AI Memory: CLAUDE.md, AGENTS.md
>   → Claude đọc tự động, luôn biết context dự án
>
> Nhóm 2 — AI Rules & Skills: .claude/rules/, .claude/skills/
>   → Rules luôn áp dụng, Skills lazy-load khi cần
>
> Nhóm 3 — AI Automation: .claude/hooks/, Prompt Templates
>   → Tự động enforce, tự động check, tự động format
>
> Cộng thêm: .editorconfig và Directory.Build.props
>   → Enforce trong Visual Studio, không cần nhắc Claude"

---

### PHẦN 3: CLAUDE.md — Bộ nhớ dự án (8:00 – 14:00)
**[Cảnh: mở file CLAUDE.md, đọc từng section]**

> "CLAUDE.md là file đầu tiên Claude đọc mỗi khi mở dự án.
> Nó trả lời câu hỏi: 'Dự án này là gì? Tôi phải tuân thủ gì?'
>
> Có 5 thứ không thể thiếu trong CLAUDE.md của máy tự động hoá:
>
> Một: Stack — C#, .NET 8, WPF, Prism, DI container.
>
> Hai: Layer Constraint —
>   'WorkStation → Core.Abstractions ONLY'
>   Một dòng này ngăn được hàng chục lỗi architecture.
>
> Ba: 5 Non-negotiables —
>   Interface-only trong WorkStation.
>   CancellationToken trên mọi async method.
>   Timeout trên mọi hardware call.
>   AlarmException cho mọi lỗi phần cứng.
>   BCrypt cho password, không bao giờ MD5.
>
> Bốn: Code patterns — copy-paste trực tiếp.
>   Pattern timeout, pattern sequence loop — viết sẵn ở đây.
>
> Năm: Alarm code ranges —
>   10xxx Motion, 20xxx Vision, 30xxx IO...
>   Claude tra cứu ngay, không cần hỏi."

---

### PHẦN 4: AGENTS.md — 8 chuyên gia AI (14:00 – 22:00)
**[Cảnh: mở AGENTS.md, demo từng agent]**

> "Thay vì nói chuyện với Claude như chat GPT,
> mình dùng agents — mỗi agent là một chuyên gia về một lĩnh vực.
>
> hw-driver: chuyên tạo hardware driver.
>   Biết: luôn tạo interface + implementation + simulator + test.
>   Biết: timeout wrapper, IDisposable, SemaphoreSlim cho thread-safety.
>
> seq-engineer: chuyên viết machine sequence.
>   Biết: WorkStation chỉ dùng interface.
>   Biết: Step phải atomic và idempotent.
>
> ui-builder: chuyên tạo WPF screen.
>   Biết: ISA-101 compliance — màu, font, layout.
>   Biết: không hardcode string, không hardcode color.
>
> Cách dùng: paste agent definition vào đầu chat → Claude theo đúng context đó.
>
> Demo: [mở terminal, paste hw-driver agent, yêu cầu tạo ModbusTCP driver]"

---

### PHẦN 5: Rules & Skills — Lazy loading (22:00 – 30:00)
**[Cảnh: folder .claude/rules và .claude/skills]**

> "Rules và Skills khác nhau ở một điểm quan trọng:
>
> Rules: luôn load, không cần yêu cầu.
>   coding-standards.md: 14 rules luôn áp dụng.
>   csharp-patterns.md: chỉ load khi làm file .cs.
>
> Skills: chỉ load khi task match.
>   Claude đọc name + description (~100 tokens mỗi skill).
>   Khi task cần → load full → tiết kiệm context.
>
> Với 10 skills chi tiết:
>   Không dùng skill = 10 × 100 = 1,000 tokens.
>   Dùng skill = 1,000 + 3,000 (full skill) = 4,000 tokens.
>   So với load tất cả cùng lúc: 30,000 tokens.
>   Tiết kiệm 85% context cho task thực sự.
>
> Demo: [mở am-hardware-patterns/SKILL.md, giải thích template driver]"

---

### PHẦN 6: Hooks — Hệ thống bảo vệ tự động (30:00 – 40:00)
**[Cảnh: folder .claude/hooks, giải thích từng hook]**

> "Hooks là thứ tôi thích nhất.
> Chúng chạy tự động — bạn không cần nhớ gì.
>
> SessionStart: mỗi lần mở Claude Code →
>   Tự động hiển thị branch, build status, arch warnings.
>   Nếu có violation → cảnh báo ngay từ đầu.
>
> PreToolUse Bash: trước khi Claude chạy lệnh shell →
>   Block rm -rf ở ngoài thư mục an toàn.
>   Block DROP TABLE, disk format.
>   Đây là môi trường thật, lệnh sai = mất data thật.
>
> PreToolUse Write/Edit: trước khi Claude ghi file →
>   Block password plaintext trong code.
>   Block WorkStation reference concrete hardware class.
>   Block XAML hardcode màu, hardcode string.
>
> PostToolUse Write/Edit: sau khi file được lưu →
>   Tự động chạy dotnet format.
>   Scan async method thiếu CancellationToken.
>   Phát hiện Thread.Sleep còn sót.
>
> Stop: khi Claude chuẩn bị dừng →
>   Quét architecture violation.
>   Nếu có vấn đề nghiêm trọng → buộc Claude tiếp tục fix.
>
> Demo: [tạo file .cs trong WorkStation với LtdmcController → hook block ngay]"

---

### PHẦN 7: Prompt Templates (40:00 – 47:00)
**[Cảnh: mở PROMPT_TEMPLATES.md]**

> "11 templates — điền placeholder, paste vào Claude, ra code chuẩn.
>
> Template tạo hardware driver: điền tên thiết bị, SDK, alarm range → Claude tạo
>   interface + driver + simulator + test ngay, đúng pattern.
>
> Template tạo Step: điền tên step, dependencies, timeout, alarm codes →
>   Claude tạo đúng structure, atomic, idempotent, có timeout.
>
> Template tạo WPF screen: điền module, data, actions, permission →
>   Claude tạo View + ViewModel + Module registration, ISA-101 compliant.
>
> Template code review: paste code → Claude báo cáo theo checklist chuẩn.
>
> Không cần nhớ gì. Không cần giải thích lại từ đầu mỗi lần.
> Chỉ cần điền chỗ trống."

---

### PHẦN 8: Demo Workflow Thực Tế (47:00 – 55:00)
**[Cảnh: screen recording làm việc thực tế]**

> "Demo từ 0 đến Step đầu tiên trong 8 phút:
>
> 1. Copy bộ file vào solution root [30 giây]
>
> 2. Mở Claude Code → SessionStart hook chạy tự động
>    Hiển thị: branch main, build OK, không có arch violation
>
> 3. Paste hw-driver agent → yêu cầu:
>    'Tạo driver cho Modbus TCP client.
>     SDK: NModbus4. Alarm: 50001-50003.
>     Plan trước khi implement.'
>
> 4. Claude plan: liệt kê 4 files sẽ tạo, confirm với tôi
>    Tôi confirm → Claude implement
>
> 5. file-write-guard hook kiểm tra trong lúc Claude viết:
>    Không có credential, không có concrete class trong WorkStation → pass
>
> 6. post-file-edit hook chạy sau khi file saved:
>    dotnet format, scan patterns → clean
>
> 7. Paste test-writer agent → 'Viết unit test cho SimulatedModbusTcpClient'
>    Claude tạo test với xUnit + Moq + FluentAssertions
>
> 8. dotnet test → 4/4 pass
>
> Total: 8 phút, 4 files, 4 tests, code chuẩn architecture."

---

### KẾT (55:00 – 60:00)
**[Cảnh: toàn bộ file tree được tạo ra]**

> "Bộ file này là nền tảng — bạn setup một lần, dùng cho mọi máy.
>
> Khi làm máy mới:
>   Giữ nguyên: hardware drivers, services, modules, rules, skills, hooks
>   Chỉ tạo mới: AM.WorkStation.{TênMáy}
>
> Claude sẽ luôn biết:
>   Dự án này dùng kiến trúc gì
>   Phải tuân thủ gì
>   Pattern nào cho hardware, sequence, UI, test
>
> Và nếu Claude sai → hooks sẽ catch ngay, không chờ đến lúc review.
>
> Link tải toàn bộ files trong phần bình luận được ghim.
> Tập tiếp theo: viết hardware driver đầu tiên với Claude — thực chiến.
>
> Subscribe để không bỏ lỡ."

---

## 🏷️ TAGS & KEYWORDS

```
Tags chính:
automachine, csharp automation, claude ai coding, wpf industrial, 
plc to csharp, kỹ sư điện lập trình, tự động hoá c#, claude code

Tags phụ:
CLAUDE.md, agents, hooks, skills, prism wpf, dotnet 8,
ISA-101 HMI, industrial software, motion controller, machine vision
ECC framework, agentic coding, automation engineer
```

---

## 📊 THUMBNAIL CONCEPT

```
Layout: Split screen
Trái: Màn hình PLC ladder logic (màu xanh dương)  
Phải: Visual Studio với code C# sạch (màu tím/tối)

Mũi tên ở giữa chỉ từ trái sang phải

Text overlay:
  Trên: "KỸ SƯ ĐIỆN → LẬP TRÌNH VIÊN"
  Dưới (lớn, vàng): "BỘ FILE SETUP"
  Sub: "Trước khi viết 1 dòng code"

Logo nhỏ góc: Claude AI + C#
```

---

## 💬 COMMENT GHIM — LINKS TẢI FILE

```
📥 DOWNLOAD LINKS — TẤT CẢ FILES MIỄN PHÍ

📦 am-ai-rules.zip
   → .cursorrules, AGENTS.md, PROMPT_TEMPLATES.md
   → AutoMachine.snippet (VS Code snippets)
   → .editorconfig, Directory.Build.props
   → QUICK_REFERENCE.md

📦 ecc-automachine-full.zip
   → CLAUDE.md (ECC format)
   → .claude/rules/ (coding standards + C# patterns)
   → .claude/skills/ (4 skills: hardware, sequence, WPF, testing)
   → .claude/commands/ (6 slash commands)

📦 am-hooks.zip
   → hooks.json (config)
   → 9 hook scripts (session-start, bash-guard, file-write-guard...)
   → README hướng dẫn cài đặt

📄 Xem thêm:
   → AutoMachine_Solution_Structure.md (kiến trúc solution)
   → HMI_UI_Design_Rules.md (quy tắc giao diện ISA-101)
   → AutoMachine_Dev_Guide_Complete.md (guide đầy đủ 2337 dòng)
   → Claude_Effective_Usage_AutoMachine.md (tips dùng Claude hiệu quả)

─────────────────────────────────────────
🔧 Cài đặt nhanh:
1. Giải nén ecc-automachine-full.zip vào solution root
2. Giải nén am-hooks.zip vào .claude/hooks/
3. Giải nén am-ai-rules.zip vào solution root
4. Import AutoMachine.snippet vào Visual Studio
   (Tools → Code Snippets Manager → Import)
5. Mở Claude Code → SessionStart tự động chạy

Có câu hỏi? Bình luận bên dưới ⬇️
```

---

## 📋 CHECKLIST SẢN XUẤT VIDEO

```
PRE-PRODUCTION:
□ Cài OBS hoặc screen recorder
□ Chuẩn bị demo project với bộ files đã setup
□ Test tất cả hooks hoạt động đúng
□ Chuẩn bị terminal với syntax highlighting
□ Chuẩn bị VSCode với theme tối, font lớn (16–18px)

RECORDING:
□ Record 1920×1080 tối thiểu
□ Tắt notification trước khi record
□ Hiển thị file tree bên trái khi giải thích cấu trúc
□ Zoom vào code khi giải thích chi tiết
□ Demo thực tế phải chạy được không cut

POST-PRODUCTION:
□ Thêm text overlay cho các thuật ngữ kỹ thuật
□ Thêm annotation khi highlight code quan trọng
□ Chapters/timestamps chính xác
□ Thumbnail A/B test (2 version)
□ Upload links vào comment ghim trước khi publish
```
