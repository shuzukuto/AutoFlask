# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
