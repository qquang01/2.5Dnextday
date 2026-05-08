# Indie Dev PIP — Unity (Lite)

> Mục đích: Quy trình tối giản cho solo indie dev hoặc team 1-2 người, tập trung ship nhanh một vertical slice (2-4 tuần), giữ chất lượng code cơ bản và có test tối thiểu.
> Phạm vi: Unity 2022 LTS+, C#. Không yêu cầu hệ thống versioning phức tạp hay workflow song song.

---

## Tư duy cốt lõi

- Giữ 4 lớp nhưng rút gọn, mỗi lớp chỉ tạo đúng 1 block output trong file này.
- Mỗi lớp phải hoàn thành (Definition of Done) mới sang lớp sau.
- Thay đổi ở lớp trên chỉ tái tạo từ lớp đó trở xuống.
- Ưu tiên “đang chạy được” hơn “hoàn hảo”. Luôn timebox đầu việc.

---

## Lịch khuyến nghị (Vertical Slice ~3 tuần)

- Tuần 1: Lớp 1 (0.5 ngày) + Lớp 2 (0.5 ngày) + Lớp 3 (1-2 ngày)
- Tuần 2: Lớp 4 — Data/Core/Player (3-4 ngày) + 2 unit test cốt lõi
- Tuần 3: Gameplay loop nhỏ + UI tối thiểu + build QA (1 ngày) + polish (1-2 ngày)

---

## Quy tắc code (theo mặc định dự án)

- [SerializeField] cho field cần chỉnh; giữ private, đặt tên `_camelCase`.
- Cache reference ở Awake/Start; không GetComponent trong Update().
- Dùng CompareTag thay vì `.tag ==`.
- Physics trong FixedUpdate() với Time.fixedDeltaTime.
- Input và non-physics trong Update() với Time.deltaTime.
- Ưu tiên TryGetComponent để tránh null.
- Dùng namespace cho tất cả scripts. PascalCase cho methods.
- Comment ngắn gọn với chỗ logic/ toán phức tạp.

---

## Tối giản lưu trữ

- Chỉ dùng 1 file này. Mỗi lớp có một “Output Block” ngay bên dưới template của lớp.
- Khi cập nhật, chèn ngày/giờ vào tiêu đề block. Không cần meta.json.
- Mỗi ngày commit 1 lần: `docs: update indie pip (L1/L2/...)`.

---

## Tổng quan 4 lớp (Lite)

1) Lớp 1 — Game Concept (Lite)
2) Lớp 2 — Technical Approach (Lite)
3) Lớp 3 — Technical Plan (Lite)
4) Lớp 4 — Code & Test (Lite)

Mỗi lớp có: Template → Output Block → DoD (Definition of Done)

---

## 1) Lớp 1 — Game Concept (Lite)

### Template (điền nhanh)
```
=== LAYER 1: GAME CONCEPT (Lite) — <yyyy-mm-dd>
Tên game: <...>
Thể loại: <...>
Phong cách: <...>
Nền tảng: <PC/Mobile/WebGL>
Gameplay cốt lõi (1-3 câu): <...>
Tham khảo: <...>
Scope: solo | team 2 | team 3
Timeline: <2-4 tuần/…>
Mục tiêu vertical slice: <phòng chơi được/combat 1 kẻ địch/boss 1 pha/...>
```

### DoD
- [ ] Có tối thiểu `thể loại` + `gameplay cốt lõi` + `mục tiêu slice`
- [ ] Scope và timeline hợp lý (không tham)

### Output Block (điền tại đây)

> Assets/Docs/Phase1

---

## 2) Lớp 2 — Technical Approach (Lite)

Mặc định khuyến nghị cho solo:
- Kiến trúc: ScriptableObject Event-Driven (Lite) hoặc MonoBehaviour thuần + vài ScriptableObject config.
- Stack: Unity 2D/3D theo game, Cinemachine (nếu cần camera), DOTween (nếu cần tween), TextMeshPro, Input System (tùy chọn).

### Template (chọn 1 phương án, không cần liệt kê 3)
```
=== LAYER 2: TECHNICAL APPROACH (Lite) — <yyyy-mm-dd>
Phương án: <MonoBehaviour thuần | SO Event-Driven (Lite)>
Mô tả ngắn (1 câu): <...>
Stack: <Unity 2022 LTS 2D/3D, ...>
Patterns: <Observer (events)/State/Factory/...>
Asset pipeline (tối thiểu): <Aseprite → Sprite Atlas | Blender FBX | FMOD/AudioSource>
Rủi ro chính (tối đa 2-3): <...>
```

### DoD
- [ ] Phù hợp scope + timeline Lớp 1
- [ ] Không có package thừa (giữ stack gọn)

### Output Block (điền tại đây)

> Assets/Docs/Phase2


---

## 3) Lớp 3 — Technical Plan (Lite)

Tập trung vào module tối thiểu cần để có vertical slice.

### Folder structure (đề xuất)
```
Assets/
  Scripts/
    Core/
    Player/
    Gameplay/
    UI/
  ScriptableObjects/
    Config/
    Events/
  Art/ ... (tùy)
  Audio/ ... (tùy)
```

### Template Module List (tối đa 5 module)
```
=== LAYER 3: TECH PLAN (Lite) — <yyyy-mm-dd>
Kiến trúc tổng quan (1-2 câu): <...>
Modules:
  - Core: GameManager, GameStateSO, SceneLoader
  - Player: PlayerController, InputHandler
  - Gameplay: <ví dụ CombatSystem/DungeonRoom/PuzzleX>
  - UI: HUD, Menu (tối giản)
Shared Events: <OnPlayerDeath/OnRoomClear/...>
Ghi chú: <tránh Singleton runtime; dùng SO cho state chia sẻ>
```

### DoD
- [ ] Danh sách module <= 5, rõ trách nhiệm
- [ ] Folder structure đã chốt

### Output Block (điền tại đây)

> Assets/Docs/Phase3


---

## 4) Lớp 4 — Code & Test (Lite)

Ưu tiên thứ tự:
1. Data layer: ScriptableObject (config, event channels)
2. Core: GameManager, GameStateSO
3. Player: Controller + Input
4. Gameplay loop nhỏ: 1 kẻ địch/1 phòng/1 puzzle
5. UI cơ bản: HUD thanh máu/score

### Checklist code
- [ ] Namespace khớp folder
- [ ] Không dùng `GameObject.Find()` trong runtime loop
- [ ] Subscribe/Unsubscribe event trong OnEnable/OnDisable
- [ ] Không GetComponent trong Update()/FixedUpdate()
- [ ] `[Header]` + `[Tooltip]` cho SerializeField quan trọng
- [ ] Compile sạch, không warning

### Testing (tối thiểu)
- Viết 1-2 Unit Test cho lớp quan trọng nhất (ví dụ: tính sát thương, state chuyển hợp lệ)
- Dùng Unity Test Framework (EditMode)

### Template commit nhỏ (gợi ý)
```
feat(core): add GameStateSO and GameManager (slice ready)
feat(player): basic movement + input
feat(gameplay): enemy hitbox + health component
test(combat): damage calc happy path
```

### Build nhanh (QA)
- PC: Development Build on, bật Profiler, kiểm tra 60 FPS (hoặc 30 FPS mobile)
- Kiểm tra crash, save/load (nếu có), kích thước build

### Output Block (ghi chú tiến độ/ngày)

> Assets/Docs/Phase4

---

## Mini-Validation (Lite)

- L1 → L2: Stack có phù hợp scope/timeline? (Nếu nghi ngờ, bỏ bớt package)
- L2 → L3: Module có đủ để đạt vertical slice chưa? (Nếu >5 module, cắt bớt)
- L3 → L4: Mỗi module có class chủ chốt và SO tương ứng chưa?

> Format trả về khi tự kiểm: `{ "passed": true|false, "issues": ["..."] }`

---

## Khi muốn thay đổi

- Sửa L1 (genre/gameplay): cập nhật L2 ngắn gọn, xem có cần đổi module ở L3 không, sau đó chỉnh L4 tương ứng.
- Thêm tính năng mới: thêm vào L3 (module mới nhỏ), tạo script mới ở L4; không refactor lớn trừ khi cần.
- Lỗi chặn đường: ghi chú ở L4 Output Block, timebox fix (<= 0.5 ngày); quá thời gian thì hạ phạm vi hoặc đổi giải pháp.

---

## Mẫu điền nhanh (gộp 4 lớp — dùng cho dự án siêu nhỏ)
```
# Vertical Slice Plan — <Tên game> (<yyyy-mm-dd>)
L1: Concept → <thể loại>, <gameplay 1-3 câu>, mục tiêu slice <...>
L2: Tech → <MonoBehaviour | SO Event-Driven (Lite)>, Stack <...>
L3: Plan → Modules [Core, Player, Gameplay(1), UI], SharedEvents <...>
L4: Code → Thứ tự Data→Core→Player→Gameplay→UI, Test(2), Build dev
```

---

## Gợi ý cắt giảm nếu quá tải

- Bỏ bớt hệ thống phụ (inventory/crafting) khỏi slice đầu.
- Giảm số animation, dùng placeholder.
- Chỉ 1 enemy/1 phòng/1 puzzle trước; nhân rộng sau khi feel ổn.
- Dời audio/polish sang tuần cuối.

---

## Ghi chú cuối

- Luôn kết thúc ngày với build chạy được và ghi lại tiến độ ở Output Block của Lớp 4.
- Mỗi tuần nên có 1 “chơi thử” trọn vẹn vertical slice để ra quyết định cắt/bổ sung.
