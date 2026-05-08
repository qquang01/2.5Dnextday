# Unity MCP Server Tools Documentation

Tài liệu này liệt kê các công cụ (tools) có sẵn thông qua `anklebreaker-unity-mcp` để điều khiển Unity Editor từ Windsurf.

## 1. Scene & Hierarchy (Quản lý phân cấp)

### `get_hierarchy`
- **Chức năng**: Liệt kê tất cả các GameObject trong Scene hiện tại theo cấu trúc cây.
- **Tham số**: Không có.
- **Sử dụng**: "Liệt kê các object đang có trong scene."

### `get_object_info`
- **Chức năng**: Lấy thông tin chi tiết của một GameObject cụ thể (Components, Transform, Properties).
- **Tham số**: `objectName` (string) hoặc `instanceID` (int).
- **Sử dụng**: "Cho tôi xem chi tiết của object 'Player'."

### `find_objects`
- **Chức năng**: Tìm kiếm các object theo tên hoặc tag.
- **Tham số**: `query` (string), `searchType` (Name/Tag).

## 2. Object Manipulation (Thao tác với Object)

### `create_primitive`
- **Chức năng**: Tạo các khối cơ bản (Cube, Sphere, Capsule, Plane, v.v.).
- **Tham số**: `type` (string), `name` (string), `position` (Vector3).
- **Sử dụng**: "Tạo một khối Cube tại vị trí (0, 0, 0)."

### `set_transform`
- **Chức năng**: Cập nhật Position, Rotation, hoặc Scale của một object.
- **Tham số**: `objectName`, `position`, `rotation`, `scale`.
- **Sử dụng**: "Di chuyển 'Main Camera' sang vị trí (0, 5, -10)."

### `delete_object`
- **Chức năng**: Xóa một GameObject khỏi Scene.
- **Tham số**: `objectName`.

### `instantiate_prefab`
- **Chức năng**: Tạo một instance từ một Prefab trong thư mục Assets.
- **Tham số**: `prefabPath`, `position`, `rotation`.

## 3. Editor Control (Điều khiển Editor)

### `set_play_mode`
- **Chức năng**: Điều khiển trạng thái Play/Stop của Editor.
- **Tham số**: `state` (Play/Stop/Pause).
- **Sử dụng**: "Bật chế độ Play." hoặc "Dừng game."

### `get_console_logs`
- **Chức năng**: Đọc các bản tin từ Unity Console.
- **Tham số**: `count` (int).
- **Sử dụng**: "Lấy 5 dòng log mới nhất từ console."

## 4. Asset Management (Quản lý tài nguyên)

### `list_assets`
- **Chức năng**: Liệt kê các file trong thư mục Assets.
- **Tham số**: `path` (string).

---
*Ghi chú: Đảm bảo Unity Editor đang mở và plugin Unity MCP Bridge đang chạy trên port 7890 để các lệnh này hoạt động.*
