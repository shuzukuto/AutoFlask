# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v1.3.0] - 2026-11-17 15:50:00

### User Request
> cập nhật thêm "Vaal Skill" để tick chọn bên cạnh skill, giúp bot xác định đó là skill thường: có cooldown và Vaal Skill cần Soul để dùng chiêu như skill T hiện tại

### Added
- Đưa tùy chọn tick chọn `Vaal Skill (Cần Soul)` lên ngay dòng đầu tiên cạnh từng thẻ kỹ năng (`[ ] Bật Skill` | `[ ] Vaal Skill (Cần Soul)` | `[ ] Use When Available` | `[Test Cast]`), giúp người dùng chủ động chỉ định và chuyển đổi tức thì giữa chế độ Skill Thường và Vaal Skill.
- Bổ sung huy hiệu phân loại trực quan trên tiêu đề CollapsingHeader: `[⚡ VAAL SOUL]` đối với Vaal Skill hoặc `[⏱️ COOLDOWN]` đối với Skill Thường, kèm tên kỹ năng PoE đọc được từ game.
- Bổ sung biểu ngữ trạng thái hướng dẫn (Mode Banner):
  - Khi tick chọn Vaal Skill: Hiển thị thanh tiến trình Soul Bar (`currSouls / soulsPerUse`) kèm thông điệp hướng dẫn rõ ràng.
  - Khi không tick chọn: Hiển thị chế độ Skill Thường quản lý theo Cooldown kèm thanh trượt thiết lập thời gian hồi chiêu.
- Bổ sung nút 1-click sửa nhanh `[⚡ Chuyển sang Vaal Skill]` khi game phát hiện trên phím gán có kỹ năng Vaal mà người dùng chưa tick chọn.
- Cập nhật cấu hình mặc định trong `AutoFlask_settings.json`: Thiết lập Skill 1 (Phím T) thành Vaal Skill theo đúng thực tế thiết lập của người dùng.

### Changed
- Chuẩn hóa logic `IsSkillReady` và `FindActorSkill`: Khi người dùng không tick chọn Vaal Skill, bot đảm bảo 100% đối xử với kỹ năng đó như Skill Thường (dựa vào Cooldown, `!actorSkill.IsOnCooldown && actorSkill.CanBeUsed`), không bị ép buộc đòi hỏi Souls. Khi người dùng tick chọn Vaal Skill, bot chuyển sang kiểm tra Souls và trạng thái Soul Gain Prevention.
- Nâng cấp phiên bản SemVer từ `v1.2.5` lên `v1.3.0` (Minor release: Bổ sung tính năng điều khiển chế độ Vaal Skill / Skill Thường trực quan).
- Cập nhật hiển thị huy hiệu phiên bản trên giao diện ImGui thành `v1.3.0`.

### Files touched
- AutoFlask.cs
- AutoFlask_settings.json
- rules.md
- changelog.md

## [v1.2.5] - 2026-11-16 21:25:00

### User Request
> bot thực hiện quá nhiều action dẫn đến game kick out

### Fixed
- Khắc phục triệt để lỗi gửi quá nhiều hành động dồn dập (burst actions) trong 1 frame khiến máy chủ Path of Exile ngắt kết nối với thông báo "Too many actions performed, disconnected from server".
- Triển khai bộ điều tiết nhịp hành động toàn cục (Global Action Rate Limiter / Inter-Action Throttler): Đảm bảo khoảng cách giữa 2 lần nhấn phím bất kỳ (xuyên suốt cả 5 bình và 3 kỹ năng) luôn đạt tối thiểu `ActionThrottleMs` (mặc định 80ms, tương đương tối đa ~12.5 actions/giây), loại bỏ hoàn toàn việc xả 5 bình + 3 chiêu trong cùng một mili-giây.
- Phân phối thứ tự hành động thông minh: Mỗi nhịp frame chỉ xử lý duy nhất 1 hành động (ưu tiên bình Máu/Mana khẩn cấp trước, sau đó đến Utility và Kỹ năng), giúp thao tác uống bình piano nhịp nhàng tự nhiên như người chơi thật.
- Bổ sung rào chắn thời gian đệm an toàn cứng (Hard Minimum Cooldown >= 250ms - 350ms) cho mọi ô bình thuốc, ngăn chặn tình trạng spam phím liên tục mỗi frame khi người dùng tắt `Enable Cooldown` hoặc đặt cooldown = 0.
- Sửa lỗi logic `Utility` khi tắt cả `useWhenAvailable` lẫn `cooldownEnable` bị rơi vào vòng lặp nhấn phím liên tục mỗi frame.
- Bổ sung cơ chế Debounce an toàn (250ms - 500ms) cho kỹ năng Vaal và kỹ năng tự dùng để chống spam phím trong thời gian chờ máy chủ PoE cập nhật dữ liệu linh hồn (Souls) và Soul Gain Prevention.

### Added
- Bổ sung cấu hình `ActionThrottleMs` (Action Rate Limiter / Inter-Action Delay) trong `AutoFlaskSettings` (phạm vi từ 20ms đến 250ms, mặc định 80ms) và thanh trượt điều chỉnh trực quan tại tab `🛡️ Safety & Config`.

### Changed
- Nâng cấp phiên bản SemVer từ `v1.2.4` lên `v1.2.5`.
- Cập nhật hiển thị huy hiệu phiên bản trên giao diện ImGui thành `v1.2.5`.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- rules.md
- changelog.md

## [v1.2.4] - 2026-11-16 17:15:00

### User Request
> kiểm tra cấu hình hiện tại để sửa lỗi bot tự động đồng bộ flask trong game chuẩn nhưng flask ultility ở slot 1 luôn bị lỗi và hoạt động không chuẩn như các flask ở slot 2,3

### Fixed
- Sửa triệt để lỗi va chạm hiệu ứng buff ở bình ô 1 (Slot 1 / `index == 0`): Trước đây hàm `IsUtilityFlaskAvailable` kiểm tra điều kiện `b.FlaskSlot == index`. Trong ExileCore / Path of Exile, toàn bộ các buff hào quang (Auras như Determination, Grace, Purity, Blood Rage, v.v.), nội tại Pantheon và Passives đều có giá trị offset `FlaskSlot` mặc định bằng 0. Khi bình Utility đặt ở ô 1 (`index == 0`), điều kiện `b.FlaskSlot == 0` lập tức khớp với buff hào quang đầu tiên của nhân vật, khiến trạng thái bình ô 1 vĩnh viễn bị treo ở `Buff Active (aura_...)` và không bao giờ tự động bấm uống khi bật "Use when available".
- Giới hạn điều kiện kiểm tra `FlaskSlot`: Chỉ khớp `b.FlaskSlot == index` khi tên buff thực sự bắt đầu bằng tiền tố `flask_utility`, loại bỏ hoàn toàn khả năng nhận diện nhầm các buff aura/passive thành buff bình thuốc.
- Bổ sung bảng ánh xạ dự phòng `UtilityFlaskPathToBuff` và `UtilityFlaskNameToBuff` cho toàn bộ các loại bình tiện ích (Quicksilver, Granite, Jade, Diamond, Ruby, Sapphire, Topaz, Amethyst, Quartz, Basalt, Silver, Sulphur, Bismuth, v.v.) đảm bảo luôn trích xuất chính xác 100% ID buff game kể cả khi con trỏ chuỗi bộ nhớ bị chậm.
- Thêm bộ nhớ đệm `_lastDetectedFlaskType` cho từng slot trong `ResolveFlaskType` giúp duy trì loại bình đã nhận diện (Utility) xuyên suốt quá trình chuyển cảnh hoặc tải map thay vì bị reset về fallback mặc định "HP" của ô 1.

### Changed
- Nâng cấp phiên bản SemVer từ `v1.2.3` lên `v1.2.4`.
- Cập nhật hiển thị huy hiệu phiên bản trên giao diện ImGui thành `v1.2.4`.

### Files touched
- AutoFlask.cs
- rules.md
- changelog.md

## [v1.2.3] - 2026-11-16 16:40:00

### User Request
> kiểm tra lại cấu hình của các flask, có vẻ bot nhận nhầm flask ở slot 1 là mana do code cũ còn sót lại

### Fixed
- Khắc phục triệt để lỗi kế thừa file cấu hình cũ (`AutoFlask_settings.json`): Slot 1 bị lưu cứng giá trị `"Mana"` (ngưỡng 15%) và Slot 5 lưu `"HP"` (ngưỡng 60%) từ phiên bản đầu tiên của AutoFlask, khiến bot uống bình ô 1 khi tụt Mana thay vì Máu.
- Tự động di chuyển (auto-migrate) và chuẩn hóa thiết lập khi khởi động plugin nếu phát hiện bố cục ngược cũ (Slot 1 Mana / Slot 5 HP).
- Chuẩn hóa trực tiếp file cấu hình `AutoFlask_settings.json` đưa Slot 1 về đúng loại `HP` (ngưỡng 45%) và Slot 5 về `Utility`.

### Added
- Bổ sung cơ chế tự động nhận diện bình thuốc thông minh từ bộ nhớ game (`GetDetectedFlaskType`): Đọc trực tiếp loại bình (1: HP, 2: Mana, 3: Hybrid, 4: Utility) và tên trang bị (ví dụ `Divine Life Flask`) từ `PlayerInventories`.
- Thêm chế độ `"Auto"` cho danh sách `FlaskTypes` (`"Auto"`, `"HP"`, `"Mana"`, `"Utility"`): Tự động kích hoạt hành vi theo đúng loại bình thực tế đang trang bị trên đai lưng.
- Thêm nút `[🔄 Tự động đồng bộ loại bình theo trang bị game]` trong tab Flasks: Tự động quét và đồng bộ chuẩn xác 100% cả 5 ô bình thuốc chỉ với 1 cú click.
- Hiển thị tên trang bị và cảnh báo lệch loại thời gian thực (`[⚠️ Lệch loại: Cấu hình là Mana]`) kèm nút 1-click sửa nhanh trực tiếp trên từng thẻ bình thuốc.

### Changed
- Nâng cấp phiên bản SemVer từ `v1.2.2` lên `v1.2.3`.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- rules.md
- changelog.md

## [v1.2.2] - 2026-11-16 16:25:40

### User Request
> thêm hiển thị phím tắt của skill trên HUD

### Added
- Hiển thị phím tắt tương ứng của từng kỹ năng trên dòng trạng thái Skills của HUD On-Screen trong game (định dạng trực quan: `S1[W]: RDY | S2[E]: 3.5s | S3[R]: 15/30`).
- Thêm cài đặt `ShowSkillKeysOnHud` (mặc định bật) trong AutoFlaskSettings và checkbox `Show Hotkeys (Hiện phím tắt)` tại tab cài đặt HUD Overlay.
- Hàm chuẩn hóa tên phím `FormatKeyName` giúp hiển thị gọn gàng các phím số (`D1`..`D9` -> `1`..`9`) và ký tự chữ cái trên HUD.

### Changed
- Nâng cấp phiên bản SemVer từ `v1.2.1` lên `v1.2.2`.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- rules.md
- changelog.md

## [v1.2.1] - 2026-11-16 16:21:00

### User Request
> bot không nhận diện được Vaal Skill. Vaal Skill không có thời gian hồi chiêu mà phụ thuộc Soul.

### Added
- Hỗ trợ toàn diện nhận diện và cơ chế vận hành cho Kỹ năng Vaal (Vaal Skills): Tự động phát hiện qua `ActorSkill.IsVaalSkill`, tiền tố `"Vaal "`, hoặc tùy chọn `Is Vaal Skill (Phụ thuộc Soul)` (`Skill1IsVaalSkill`, `Skill2IsVaalSkill`, `Skill3IsVaalSkill`).
- Tích hợp trích xuất và giám sát số lượng linh hồn thời gian thực từ `Actor.ActorVaalSkills` (`CurrVaalSouls`, `SoulsPerUse`, `VaalMaxSouls`, `HasEnoughSouls`).
- Thanh đo tiến trình tích lũy Souls (Souls Progress Bar) trực quan trên giao diện cài đặt ImGui.
- Chẩn đoán trạng thái đặc thù của kỹ năng Vaal: `Souls (X/Y)` khi chưa đủ linh hồn, `Soul Prevention` khi kỹ năng đang trong thời gian hồi tích hồn sau khi sử dụng, và `Ready (X/Y)` khi đã đủ điều kiện kích hoạt.
- Tối ưu tính năng "Use When Available" cho Vaal Skill: Tự động xuất chiêu ngay khi đủ Souls và thoát khỏi Soul Gain Prevention mà không bị chặn bởi bộ đếm Cooldown thông thường.

### Changed
- Cải tiến hàm `FindActorSkill`: Khi bật hoặc phát hiện Vaal Skill, tự động ưu tiên lọc phiên bản Vaal trong slot phím hoặc tìm kiếm kỹ năng Vaal liên kết với gem kỹ năng thông thường.
- Cập nhật hiển thị HUD On-Screen: Hiển thị ngắn gọn thông tin linh hồn `S1: X/Y`, ngăn tích hồn `S1: Prev`, hoặc sẵn sàng `S1: RDY`.
- Xử lý triệt để cảnh báo `ActorVaalSkill.VaalSoulsPerUse` obsolete, đưa toàn bộ dự án về chuẩn biên dịch `0 Warning(s), 0 Error(s)`.
- Nâng cấp phiên bản SemVer từ `v1.2.0` lên `v1.2.1`.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- rules.md
- changelog.md

## [v1.2.0] - 2026-11-16 15:58:56

### User Request
> bổ sung cho Skill: thêm tính năng "Use when available" để tự động dùng khi skill hồi chiêu

### Added
- Tính năng "Use when available" cho 3 kỹ năng (`Skill1UseWhenAvailable`, `Skill2UseWhenAvailable`, `Skill3UseWhenAvailable`): Tự động phát hiện trạng thái hồi chiêu thực tế trong game qua `ActorSkill` (`!s.IsOnCooldown && s.CanBeUsed`) và xuất chiêu tức thì khi kỹ năng hồi xong.
- Tự động nhận diện tên kỹ năng PoE theo slot phím (`Q`, `W`, `E`, `R`, `T`) và hiển thị trực quan trong giao diện cài đặt (ví dụ: `(Molten Shell)`, `(Steelskin)`).
- Chẩn đoán trạng thái kỹ năng thời gian thực trong giao diện: `Ready`, `On Cooldown`, `No Charges`, `Cannot Use`.

### Changed
- Cập nhật hiển thị HUD On-Screen cho kỹ năng: Đồng bộ hiển thị trạng thái `RDY` hoặc `CD` thời gian thực theo in-game cooldown và `IsSkillReady`.
- Nâng cấp phiên bản SemVer từ `v1.1.1` lên `v1.2.0`.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- README.md
- rules.md
- changelog.md

## [v1.1.1] - 2026-11-16 15:42:48

### User Request
> bỏ tô nền HUD ingame

### Changed
- Mặc định tắt hoàn toàn hộp nền tô (`DrawBox`) và khung viền (`DrawFrame`) của HUD On-Screen trong game, chỉ hiển thị chữ trực tiếp trên màn hình theo yêu cầu người dùng.
- Thêm cài đặt `DrawHudBackground` (mặc định tắt) trong AutoFlaskSettings và giao diện tab HUD Overlay để người chơi có thể chủ động bật/tắt nếu cần.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- rules.md
- changelog.md

## [v1.1.0] - 2026-11-16 15:08:16

### User Request
> kiểm tra và hoàn thiện, tối ưu code, UI cài đặt

### Added
- Giao diện cài đặt hiện đại bằng ImGui TabBar với 5 tabs: Flasks (1-5), Skills (1-3), HUD Overlay, Safety & Config, Classic Tree View.
- Hệ thống chẩn đoán thời gian thực (Live Diagnostics) cho từng bình thuốc: hiển thị trạng thái `Ready`, `Buff Active`, `No Charges`, `Empty Slot`.
- Nút bấm thử nghiệm trực tiếp `Test Trigger` cho bình thuốc và `Test Cast` cho kỹ năng.
- Tùy chọn an toàn: Tự động tạm dừng khi alt-tab (`OnlyRunWhenForeground`) và khi ở Town/Hideout (`PauseInTownAndHideout`).
- Cài đặt thời gian giữ phím bất đồng bộ (`KeyPressDurationMs`) và ngưỡng Mana tối thiểu cho kỹ năng (`MinManaForSkills`).
- Tùy biến hiển thị HUD On-Screen với 4 preset tọa độ góc màn hình nhanh và tùy chọn bật/tắt từng dòng thông tin.
- Tài liệu đặc tả kiến trúc và tiêu chuẩn thiết kế `rules.md`.

### Changed
- Nâng cấp toàn bộ Hotkey từ `HotkeyNode` (obsolete) sang `HotkeyNodeV2`, tự động đăng ký `Input.RegisterKey` giúp phím tắt hoạt động 100% tin cậy.
- Chuyển đổi toàn bộ thao tác đồ họa và tọa độ sang `System.Numerics.Vector2`, `System.Numerics.Vector3` và `Entity.PosNum`, xóa sạch toàn bộ cảnh báo compiler CS0612 và CS0618.
- Chuyển cơ chế gửi phím `SendKeyPress` từ khóa luồng `Thread.Sleep(25)` sang bất đồng bộ trên ThreadPool (`Task.Delay`), loại bỏ hoàn toàn hiện tượng micro-stutter / tụt FPS.
- Tối ưu thuật toán quét quái vật `TraceMonster` từ LINQ OrderByDescending $O(N \log N)$ sang vòng lặp $O(N)$ 1 lượt không cấp phát bộ nhớ (Zero-allocation).
- Thiết lập đường dẫn mặc định `exapiPackage` trong `AutoFlask.csproj` trỏ về `..\..\..` giúp biên dịch trực tiếp không cần biến môi trường.
- Viết lại toàn bộ `README.md` chuẩn hóa tài liệu hướng dẫn cho AutoFlask (thay thế nội dung của plugin Follower cũ).

### Fixed
- Sửa lỗi không nhận phím tắt `ToggleAutoFlask.PressedOnce()` do thiếu đăng ký `Input.RegisterKey`.
- Sửa lỗi gửi phím ra ngoài cửa sổ ứng dụng khác khi alt-tab.
- Khắc phục các cảnh báo CS0105 using trùng lặp, CS0612 API cũ và CS0618 Hotkey cũ.

### Files touched
- AutoFlask.cs
- AutoFlaskSettings.cs
- AutoFlask.csproj
- README.md
- rules.md
- changelog.md
- Errors.txt (đã xóa)
- Mouse.cs (đã xóa)
- TaskNode.cs (đã xóa)
