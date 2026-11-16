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

---

## 2. Tiêu chuẩn Giao diện Cài đặt (UI/UX Settings Specification)

### 2.1. Phân cấp Tab Bar (ImGui Tabs)
Giao diện cài đặt phải được tổ chức thành 5 Tab rõ ràng:
1. **🧪 Flasks (1-5)**:
   - Header hiển thị trạng thái động của bình thuốc (`Ready`, `Buff Active`, `No Charges`, `Empty Slot`).
   - Cung cấp nút `Test Trigger` kiểm tra phím bấm tức thì.
   - Hỗ trợ 3 kiểu bình: HP (ngưỡng %), Mana (ngưỡng %), Utility (Use When Available).
2. **⚔️ Skills (1-3)**:
   - Quản lý Cooldown độc lập cho từng chiêu thức.
   - Tùy chọn Auto-Aim (Monster Tracing) kèm thanh trượt phạm vi (Range).
   - Nút `Test Cast` trực tiếp.
3. **📊 HUD Overlay**:
   - Tùy chọn vị trí màn hình (X, Y) và 4 nút Preset nhanh góc màn hình.
   - Bật/tắt linh hoạt từng dòng thông tin (Vitals, ES, Area Stopwatch, Skills, Flasks).
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
