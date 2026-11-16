# Auto Flask & Triple Skill Bot for ExileAPI

Plugin tự động sử dụng bình thuốc (Auto Flask) và hỗ trợ tung 3 kỹ năng tự động (Triple Skill Bot) dành cho ExileAPI trên Path of Exile.

---

## 🌟 Tính năng nổi bật (Features)

### 1. Quản lý 5 Bình thuốc (5-Slot Auto Flasks)
- **3 Chế độ linh hoạt**:
  - `HP`: Tự động kích hoạt khi Máu rơi xuống dưới ngưỡng cài đặt (%)
  - `Mana`: Tự động kích hoạt khi Mana rơi xuống dưới ngưỡng cài đặt (%)
  - `Utility`: Tự động duy trì hiệu ứng (Use When Available - tự dùng lại ngay khi buff hết tác dụng và bình đủ Charges)
- **Hệ thống Cooldown độc lập**: Cho phép đặt thời gian giãn cách giữa các lần bấm phím (tránh spam phím).
- **Chẩn đoán thời gian thực (Live Diagnostics)**: Hiển thị ngay trạng thái từng bình (`Ready`, `Buff Active`, `No Charges`, `Empty Slot`) trong giao diện cài đặt.
- **Nút Test Trigger**: Cho phép bấm thử nghiệm trực tiếp từng bình trong UI.

### 2. Tự động tung kỹ năng (Triple Skill Automation)
- Hỗ trợ tới 3 phím kỹ năng độc lập (Skill 1, Skill 2, Skill 3) như Blood Rage, Molten Shell, Enduring Cry, Curse, Totem, v.v.
- **Tính năng "Use when available"**: Tự động phát hiện trạng thái hồi chiêu trong game thông qua `ActorSkill` và tự động tung kỹ năng ngay khi chiêu vừa hồi xong (Off Cooldown).
- **Tự động nhắm mục tiêu (Monster Tracing / Auto-Aim)**: Tự động di chuột và nhắm vào quái vật có độ hiếm cao nhất (Unique > Rare > Magic > Normal) trong phạm vi cho phép trước khi xuất chiêu.
- **Mana Safety Gate**: Không tung chiêu nếu lượng Mana hiện tại thấp hơn ngưỡng an toàn.
- **Chẩn đoán kỹ năng thời gian thực**: Tự động nhận diện tên kỹ năng PoE theo khe phím (`Q`, `W`, `E`, `R`, `T`) và hiển thị trạng thái `Ready`, `On Cooldown` trực tiếp trong UI.
- **Nút Test Cast**: Bấm thử kỹ năng ngay trong giao diện cài đặt.

### 3. Giao diện HUD On-Screen hiện đại
- Hiển thị trạng thái hoạt động: `AutoFlask: [ACTIVE]` / `[PAUSED]`.
- Chỉ số sinh tồn: `HP: % | MP: % | ES: %` đổi màu trực quan theo tình trạng máu.
- Bộ đếm thời gian khu vực: `Area: Name (HH:MM:SS)`.
- Đếm ngược Cooldown kỹ năng và trạng thái từng bình thuốc.
- Tự động đo kích thước và vẽ khung nền bo viền bán trong suốt sang trọng.
- Cho phép bật/tắt từng dòng hiển thị và tùy chỉnh tọa độ (kèm 4 preset góc màn hình).

### 4. An toàn & Tối ưu hiệu năng (Safety & Performance)
- **Keystroke bất đồng bộ (Non-blocking)**: Sử dụng tác vụ nền trên ThreadPool, không sử dụng `Thread.Sleep` gây giật/khựng game (giữ vững 144+ FPS).
- **Monster Tracing $O(N)$ không cấp phát (Zero-allocation)**: Quét quái vật một lượt tối ưu, loại bỏ chi phí sắp xếp mảng của LINQ.
- **Kiểm tra tiêu điểm (Foreground Window Check)**: Tự động tạm dừng gửi phím khi người chơi alt-tab ra ngoài (Discord, trình duyệt web).
- **Tạm dừng tại khu vực an toàn (Town / Hideout Pause)**: Tránh lãng phí bình thuốc và spam kỹ năng khi đang ở làng hoặc Hideout.

### 5. Giao diện cài đặt ImGui Dashboard
- Tab bar phân chia trực quan:
  - 🧪 **Flasks (1-5)**: Quản lý chi tiết 5 bình.
  - ⚔️ **Skills (1-3)**: Cấu hình kỹ năng và tầm nhắm.
  - 📊 **HUD Overlay**: Tùy chỉnh vị trí và nội dung hiển thị.
  - 🛡️ **Safety & Config**: Cấu hình an toàn và thời gian bấm phím.
  - ⚙️ **Classic Tree View**: Giao diện cây mặc định của ExileCore (tương thích 100%).

---

## 🚀 Cài đặt (Installation)

1. Đặt thư mục plugin vào:
   `ExileAPI/Plugins/Source/AutoFlask`
2. Khởi động ExileAPI Loader hoặc biên dịch bằng lệnh:
   ```powershell
   dotnet build
   ```
3. Trong game, mở menu ExileAPI (phím `F12` hoặc phím cấu hình), chọn **Auto Flask & Triple Skill Bot** để cấu hình.
4. Phím tắt chuyển đổi nhanh (Master Toggle): Mặc định là `PageDown` (có thể đổi trong cài đặt).
