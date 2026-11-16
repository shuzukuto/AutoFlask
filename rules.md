# AutoFlask Specifications & Architecture Rules

Tài liệu đặc tả kiến trúc và tiêu chuẩn thiết kế còn hiệu lực của plugin **AutoFlask** (dành cho ExileAPI / Path of Exile).

---

## 1. Kiến trúc Cốt lõi (Core Architecture)

### 1.1. Lifecycle & Framework
- Kế thừa `BaseSettingsPlugin<AutoFlaskSettings>` từ `ExileCore`.
- Target Framework: `.NET 10.0-windows` (x64).
- Giao diện đồ họa cấu hình: `Hexa.NET.ImGui` kết hợp `base.DrawSettings()` dự phòng.
- Đồ họa trực quan trong game (HUD Overlay): `IGraphics` (`Graphics.DrawText`, tùy chọn khung nền hộp `Graphics.DrawBox` qua cấu hình `DrawHudBackground`, mặc định tắt).

### 1.2. Cơ chế Gửi Phím (Input System)
- Sử dụng `user32.dll` (`keybd_event`) kết hợp `Task.Delay(duration).ContinueWith(...)`.
- **Tuyệt đối không** dùng `Thread.Sleep` trong vòng lặp game (`Tick()`) để tránh làm drop FPS của client game.
- Toàn bộ phím nóng (Hotkeys) dùng `HotkeyNodeV2` (chuẩn ExileCore hiện hành) và bắt buộc đăng ký `Input.RegisterKey(...)`.

### 1.3. Cơ chế Quét Mục tiêu (Monster Tracing)
- Thuật toán quét $O(N)$ 1 vòng lặp trực tiếp trên `GameController.EntityListWrapper.Entities`.
- Tiêu chí ưu tiên: Quái vật còn sống, thù địch, có thể target được, độ hiếm cao nhất (`Rarity`), sau đó đến khoảng cách gần người chơi nhất.
- Chuyển đổi tọa độ qua `Camera.WorldToScreen(PosNum)` sử dụng `System.Numerics.Vector3` và `System.Numerics.Vector2`.

### 1.4. Cơ chế Kỹ năng Tự động, Vaal Skills & Use When Available
- **Kỹ năng thông thường (Regular Skills)**:
  - Tự động ánh xạ phím gán (`Keys.Q`, `W`, `E`, `R`, `T`) tới chỉ số ô kỹ năng (`SkillSlotIndex`) trong `Actor.ActorSkills`.
  - Khi bật `UseWhenAvailable`: Kiểm tra trạng thái hồi chiêu thực tế trong game (`!actorSkill.IsOnCooldown && actorSkill.CanBeUsed`) để tự động tung chiêu ngay khi hồi xong, đồng thời duy trì bộ đếm dự phòng an toàn.
- **Kỹ năng Vaal (Vaal Skills)**:
  - Tự động nhận diện qua cờ `ActorSkill.IsVaalSkill`, tiền tố tên `"Vaal "`, hoặc tùy chọn `IsVaalSkill` trong cài đặt.
  - Phụ thuộc tích lũy Souls (`CurrVaalSouls` từ `Actor.ActorVaalSkills` so với `SoulsPerUse` / `VaalMaxSouls` hoặc `ActorSkill.HasEnoughSouls`) thay vì thời gian hồi chiêu (`Cooldown`).
  - Bỏ qua ràng buộc cooldown thông thường; kiểm tra `ActorSkill.CanBeUsed` để tôn trọng trạng thái `Soul Gain Prevention` (ngăn tích và dùng soul tạm thời sau khi cast) và debuff.
  - Khi bật `UseWhenAvailable`: Tự động thi triển ngay khi tích đủ Souls và không bị Soul Gain Prevention.
  - Hiển thị thanh tiến trình Souls động trên giao diện ImGui (`CurrSouls / SoulsPerUse`) và rút gọn trên HUD (`S1: 15/30`, `S1: Prev`, `S1: RDY`).

### 1.5. Cơ chế Bình Thuốc & Nhận diện Hiệu ứng Buff (Utility Flask Buff Architecture)
- **Đồng bộ tự động (Auto-Sync)**: Đọc loại bình thực tế từ bộ nhớ `PlayerInventories` (1: HP, 2: Mana, 3: Hybrid, 4: Utility) và lưu vết bộ nhớ tạm (`_lastDetectedFlaskType`) để duy trì đúng loại bình qua các lần chuyển map / màn hình chờ.
- **Quy tắc kiểm tra khả dụng (Availability & Use When Available)**:
  - Kiểm tra Charges thực tế: `chargeComponent.NumCharges >= chargeComponent.ChargesPerUse`.
  - Phân giải Buff chính xác: Đọc chuỗi tên buff gốc từ cấu trúc bộ nhớ `Flask` (`+0x28, 0x18, 0x0`), kết hợp từ điển ánh xạ dự phòng (`UtilityFlaskPathToBuff` / `UtilityFlaskNameToBuff` cho Diamond, Ruby, Sapphire, Topaz, Granite, Quicksilver, Jade, v.v.).
  - **Quy tắc chống va chạm ô Slot 1 (`index == 0`)**: Các buff thông thường của người chơi (Auras như Determination, Grace, Blood Rage, Purity, nội tại Pantheon, Passives) mặc định trả về trường `FlaskSlot == 0` trong cấu trúc bộ nhớ của ExileCore. Do đó, logic kiểm tra buff bình tiện ích **tuyệt đối không** được so sánh trực tiếp `b.FlaskSlot == index` mà bắt buộc phải ràng buộc tiền tố `b.Name.StartsWith("flask_utility")` hoặc khớp chính xác danh sách tên buff `buffNames.Contains(b.Name)`. Quy tắc này đảm bảo Slot 1 hoạt động độc lập, chính xác 100% như các slot 2, 3, 4, 5.

---

## 2. Tiêu chuẩn Giao diện Cài đặt (UI/UX Settings Specification)

### 2.1. Phân cấp Tab Bar (ImGui Tabs)
Giao diện cài đặt phải được tổ chức thành 5 Tab rõ ràng:
1. **🧪 Flasks (1-5)**:
   - Header hiển thị tên vật phẩm trang bị thực tế (`BaseName`), loại bình (`Auto`, `HP`, `Mana`, `Utility`) và trạng thái động (`Ready`, `Buff Active`, `No Charges`, `Empty Slot`).
   - Cung cấp nút `Test Trigger` kiểm tra phím bấm tức thì.
   - Hỗ trợ 4 chế độ: `Auto` (tự động nhận diện loại bình từ bộ nhớ game), `HP` (ngưỡng %), `Mana` (ngưỡng %), `Utility` (Use When Available).
   - Nút `🔄 Tự động đồng bộ loại bình theo trang bị game` quét nhanh toàn bộ 5 slot và cấu hình chuẩn xác.
   - Cảnh báo lệch loại giữa cấu hình và trang bị thực tế trong game kèm nút 1-click sửa nhanh.
2. **⚔️ Skills (1-3)**:
   - Quản lý Cooldown độc lập cho từng chiêu thức kèm tùy chọn **Use When Available** (tự dùng ngay khi hồi chiêu trong game).
   - Tự động nhận diện tên kỹ năng PoE (`ActorSkill.Name`) và hiển thị trạng thái động.
   - Tùy chọn Auto-Aim (Monster Tracing) kèm thanh trượt phạm vi (Range).
   - Nút `Test Cast` trực tiếp.
3. **📊 HUD Overlay**:
   - Tùy chọn vị trí màn hình (X, Y) và 4 nút Preset nhanh góc màn hình.
   - Bật/tắt linh hoạt từng dòng thông tin (Vitals, ES, Area Stopwatch, Skills kèm tùy chọn hiển thị phím tắt `[W]`, `[E]`, `[R]`, Flasks).
4. **🛡️ Safety & Config**:
   - Kiểm tra cửa sổ game có đang ở tiền cảnh (`IsForeground`).
   - Tạm dừng tại thị trấn và Hideout (`IsTown`, `IsHideout`).
   - Ngưỡng Mana tối thiểu trước khi tung chiêu.
5. **⚙️ Classic Tree View**:
   - Hiển thị cây thiết lập gốc của ExileCore (`base.DrawSettings()`) để đảm bảo tính tương thích và linh hoạt.

---

## 3. Quy chuẩn An toàn (Safety Protocols)
1. **Foreground Enforcement**: Khi người dùng alt-tab ra khỏi game, plugin phải ngừng gửi keystroke để tránh gõ phím vào ứng dụng ngoài.
2. **Safe Zones Suppression**: Tự động vô hiệu hóa tự động uống bình và thi triển kỹ năng khi ở Hideout hoặc Town.
3. **Player State Validation**: Kiểm tra trạng thái người chơi (`!GameController.Player.IsAlive`, `InGame`, `IsLoading`) trước khi xử lý bất kỳ hành động nào.
