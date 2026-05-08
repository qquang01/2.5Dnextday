# Game Development AI Agent Pipeline — Unity Engine

> **Mục đích:** Hướng dẫn AI agent vận hành quy trình phát triển game 4 lớp với Unity.  
> Tài liệu này bao gồm cơ chế lưu trữ, validation, iteration, testing, assets, build/deploy, collaboration và error handling.

**Phiên bản:** 2.0 | **Engine:** Unity 2022 LTS+ | **Ngôn ngữ:** C#

---

## Mục lục

1. [Nguyên tắc cốt lõi](#1-nguyên-tắc-cốt-lõi)
2. [Cơ chế lưu trữ & Versioning](#2-cơ-chế-lưu-trữ--versioning)
3. [Sơ đồ luồng pipeline](#3-sơ-đồ-luồng-pipeline)
4. [Lớp 1 — Ý tưởng & Định hướng](#4-lớp-1--ý-tưởng--định-hướng-game)
5. [Lớp 2 — Phân tích Kỹ thuật](#5-lớp-2--phân-tích-kỹ-thuật--lựa-chọn)
6. [Lớp 3 — Tài liệu Chi tiết](#6-lớp-3--tài-liệu-kỹ-thuật-chi-tiết)
7. [Lớp 4 — Triển khai Code](#7-lớp-4--triển-khai-unity-c-scripts)
8. [Testing Pipeline](#8-testing-pipeline)
9. [Asset Pipeline](#9-asset-pipeline)
10. [Build & Deploy](#10-build--deploy)
11. [Collaboration & Parallel Work](#11-collaboration--parallel-work)
12. [Iteration & Incremental Update](#12-iteration--incremental-update)
13. [Validation giữa các lớp](#13-validation-giữa-các-lớp)
14. [Error Handling & Recovery](#14-error-handling--recovery)
15. [Ví dụ context đầy đủ](#15-ví-dụ-context-đầy-đủ)

---

## 1. Nguyên tắc cốt lõi

- **Context tích lũy:** Lớp N luôn nhận toàn bộ output của Lớp 1 → N-1.
- **Không bỏ qua lớp:** Nếu lớp trước chưa có output hợp lệ, dừng và yêu cầu bổ sung.
- **Phản ánh đúng game:** Mọi quyết định kỹ thuật phải truy nguyên được về Lớp 1.
- **Validate trước khi tiếp tục:** Mỗi lớp có validation checklist trước khi chuyển sang lớp kế.
- **Iteration không phá vỡ pipeline:** Thay đổi ở lớp bất kỳ chỉ tái tạo từ lớp đó trở đi, không làm lại toàn bộ.

---

## 2. Cơ chế lưu trữ & Versioning

### Cấu trúc file lưu trữ

Mỗi project dùng một thư mục `pipeline/` với cấu trúc sau:

```
pipeline/
  meta.json                  ← thông tin project, version hiện tại của mỗi lớp
  layer1/
    v1.0.json                ← output Lớp 1 ban đầu
    v1.1.json                ← sau khi chỉnh sửa
    current.json             ← copy của version đang active
  layer2/
    v1.0.json
    current.json
  layer3/
    v1.0.json
    current.json
    modules/                 ← module riêng lẻ khi làm song song
      combat_v1.0.json
      dungeon_v1.0.json
  layer4/
    scripts/                 ← các file .cs đã generate
    v1.0_manifest.json       ← ghi lại script nào tạo từ version nào
  changelog.md               ← log mọi thay đổi theo thời gian
  failed/                    ← output lỗi để phân tích sau
```

### Schema của `meta.json`

```json
{
  "project_id": "shadow-realm-001",
  "game_name": "Shadow Realm",
  "created_at": "2025-01-10",
  "updated_at": "2025-01-15",
  "active_versions": {
    "layer1": "v1.1",
    "layer2": "v1.0",
    "layer3": "v1.0",
    "layer4": "v1.0"
  },
  "pipeline_status": {
    "layer1": "approved",
    "layer2": "approved",
    "layer3": "in_progress",
    "layer4": "locked"
  }
}
```

### Schema output mỗi lớp

Mỗi file `vX.Y.json` có cấu trúc bọc ngoài thống nhất:

```json
{
  "version": "v1.0",
  "layer": 1,
  "created_at": "2025-01-10T10:00:00",
  "created_by": "agent | human",
  "parent_versions": { "layer1": "v1.0" },
  "status": "draft | validated | approved",
  "content": { ... },
  "validation_result": { "passed": true, "issues": [] },
  "changelog": "Mô tả thay đổi so với version trước"
}
```

### Quy tắc versioning

| Loại thay đổi | Bump |
|---|---|
| Sửa nhỏ trong cùng lớp (typo, bổ sung field phụ) | patch: v1.0 → v1.0.1 |
| Thay đổi nội dung quan trọng trong lớp | minor: v1.0 → v1.1 |
| Thay đổi ảnh hưởng toàn bộ lớp dưới | major: v1.0 → v2.0 |

Khi version bump major → agent tự động đánh dấu các lớp phụ thuộc là `stale` trong `meta.json` và yêu cầu human review trước khi tiếp tục.

---

## 3. Sơ đồ luồng pipeline

```
┌──────────────────────────────────────────┐
│  LỚP 1 — Game Concept                    │
│  Input: human                            │
│  Output: layer1/current.json             │
└──────────────────┬───────────────────────┘
                   │
           [VALIDATION L1→L2]
                   │ pass
                   ▼
┌──────────────────────────────────────────┐
│  LỚP 2 — Technical Analysis              │
│  Input: L1                               │
│  Output: layer2/current.json             │
│  Human chọn phương án → approve          │
└──────────────────┬───────────────────────┘
                   │
           [VALIDATION L2→L3]
                   │ pass
                   ▼
┌──────────────────────────────────────────┐
│  LỚP 3 — Technical Docs                  │◄── song song theo module
│  Input: L1 + L2                          │    (Agent A: Player+Combat)
│  Output: layer3/current.json             │    (Agent B: Dungeon+Room)
└──────────────────┬───────────────────────┘    (Agent C: UI+Menu)
                   │
           [VALIDATION L3→L4]             ◄── Asset Pipeline bắt đầu song song
                   │ pass
                   ▼
┌──────────────────────────────────────────┐
│  LỚP 4 — Unity C# Code                  │◄── Testing song song
│  Input: L1 + L2 + L3                    │    (unit test mỗi script)
│  Output: scripts + manifest             │
└──────────────────┬───────────────────────┘
                   │
           [TESTING PIPELINE]
           Unit → Integration → Playtest
                   │
           [BUILD & DEPLOY]
           Optimize → Build → QA → Release

  ↺ ITERATION: thay đổi bất kỳ lớp nào
    → chỉ tái tạo từ lớp đó trở xuống
    → không làm lại toàn bộ
```

---

## 4. Lớp 1 — Ý tưởng & Định hướng Game

**Vai trò:** Single source of truth. Mọi quyết định kỹ thuật đều phải truy nguyên về đây.

**Ai thực hiện:** Human (game designer / product owner). Agent hỗ trợ hỏi và tổng hợp.

### Các trường cần thu thập

| Trường | Bắt buộc | Mô tả | Ví dụ |
|---|---|---|---|
| `game_name` | Có | Tên game | Shadow Realm |
| `genre` | Có | Thể loại | Action RPG, Roguelike |
| `art_style` | Có | Phong cách hình ảnh | 2D Pixel art dark fantasy |
| `platform` | Có | Nền tảng mục tiêu | PC (Steam), Mobile |
| `story` | Khuyến nghị | Cốt truyện, thế giới | Linh hồn vượt thoát bóng tối |
| `gameplay` | Có | Vòng lặp gameplay, cơ chế chính | Procedural dungeon, permadeath |
| `references` | Khuyến nghị | Game tham khảo | Hades + Dead Cells |
| `scope` | Có | Quy mô team | solo indie / small team / studio |
| `timeline` | Có | Thời gian dự kiến | 6 tháng |

### Output chuẩn — lưu vào `layer1/current.json`

```json
{
  "version": "v1.0",
  "layer": 1,
  "status": "approved",
  "content": {
    "game_name": "Shadow Realm",
    "genre": "2D Action Roguelike",
    "art_style": "Pixel art dark fantasy",
    "platform": ["PC"],
    "story": "...",
    "gameplay": "...",
    "references": ["Hades", "Dead Cells"],
    "scope": "solo indie",
    "timeline": "6 tháng"
  }
}
```

### Điều kiện hoàn thành

- [ ] Đủ các trường bắt buộc
- [ ] `scope` và `timeline` đã xác định (ảnh hưởng trực tiếp đến Lớp 2)
- [ ] Human đã review và approve
- [ ] Validation L1 pass trước khi mở khóa Lớp 2

---

## 5. Lớp 2 — Phân tích Kỹ thuật & Lựa chọn

**Vai trò:** Đọc Lớp 1, đề xuất các phương án kỹ thuật phù hợp với đặc thù game và scope team.

**Input bắt buộc:** `layer1/current.json` đã `approved`.

### Prompt mẫu cho agent

```
Bạn là Unity game architect. Đọc toàn bộ game concept và đề xuất 3 phương án kỹ thuật.

[DÁN NỘI DUNG layer1/current.json VÀO ĐÂY]

Yêu cầu:
- Mỗi phương án phải khác biệt rõ ràng (ECS vs OOP, procedural vs hand-crafted...)
- Phải phù hợp với scope và timeline trong Lớp 1
- Bao gồm cả asset pipeline phù hợp với art_style
- Đánh giá rủi ro kỹ thuật cho từng phương án

Trả về JSON thuần (không markdown):
{
  "options": [
    {
      "id": "A",
      "title": "tên phương án",
      "approach": "mô tả cách tiếp cận (1-2 câu)",
      "stack": ["Unity 2022 LTS", "package chính"],
      "patterns": ["design pattern"],
      "asset_pipeline": "công cụ art/sound phù hợp",
      "pros": "ưu điểm chính",
      "cons": "nhược điểm chính",
      "risks": "rủi ro kỹ thuật",
      "difficulty": "Beginner | Intermediate | Advanced",
      "time": "ước tính thời gian",
      "team_size": "solo | 2-3 | 5+"
    }
  ]
}
```

### Tiêu chí chọn phương án

| Scope | Độ khó phù hợp | Kiến trúc gợi ý |
|---|---|---|
| Solo indie / < 6 tháng | Beginner | MonoBehaviour thuần, ít dependency |
| Small team / 6-12 tháng | Intermediate | ScriptableObject architecture, event-driven |
| Studio / 1 năm+ | Advanced | DOTS/ECS, Addressables, CI/CD đầy đủ |

### Output chuẩn — lưu vào `layer2/current.json`

```json
{
  "version": "v1.0",
  "layer": 2,
  "parent_versions": { "layer1": "v1.0" },
  "status": "approved",
  "content": {
    "chosen_option": "B",
    "title": "Event-driven ScriptableObject Architecture",
    "approach": "...",
    "stack": ["Unity 2022 LTS 2D", "ScriptableObject Events", "Cinemachine", "DOTween"],
    "patterns": ["Observer", "Command", "Object Pool"],
    "asset_pipeline": "Aseprite → Unity Sprite Atlas, FMOD",
    "risks": "...",
    "difficulty": "Intermediate",
    "time": "4 tháng"
  }
}
```

### Điều kiện hoàn thành

- [ ] Human đã chọn 1 trong các phương án
- [ ] Validation L1→L2 pass (xem mục 13)
- [ ] Human approve

---

## 6. Lớp 3 — Tài liệu Kỹ thuật Chi tiết

**Vai trò:** Đọc Lớp 1 + 2, soạn tài liệu chi tiết cho từng module. Đây là lớp duy nhất **có thể chạy song song** theo module.

**Input bắt buộc:** `layer1/current.json` + `layer2/current.json` đều đã `approved`.

### Chế độ song song (Parallel mode)

Khi team có nhiều người, Lớp 3 chia theo module:

```
Agent A  →  module: PlayerController + CombatSystem
Agent B  →  module: DungeonGenerator + RoomSystem
Agent C  →  module: RelicSystem + InventoryManager
Agent D  →  module: UIManager + MenuSystem

→ Lưu riêng vào layer3/modules/<name>_v1.0.json
→ Merge vào layer3/current.json khi tất cả xong
→ Conflict check: namespace, event name, ScriptableObject menu path
```

**Quy tắc làm song song:**
1. Đăng ký module trước khi bắt đầu → tránh 2 agent cùng làm 1 module
2. Namespace được phân vùng từ đầu: `GameName.Player | .Dungeon | .Combat | .UI`
3. Shared events thống nhất tên trong `layer3/shared_events.json` trước khi code
4. Class dùng chung → đưa vào `GameName.Core`, chỉ một người viết

### Prompt mẫu cho agent (per module)

```
Bạn là Unity architect. Soạn tài liệu kỹ thuật cho module được giao.

[DÁN layer1/current.json]
[DÁN layer2/current.json]
Module cần soạn: <tên module>

Tài liệu phải nhất quán với kiến trúc và pattern đã chọn ở Lớp 2.

Trả về JSON:
{
  "module_name": "...",
  "responsibility": "module này làm gì",
  "dependencies": ["module khác nó phụ thuộc vào"],
  "unity_components": ["Component Unity cần dùng"],
  "classes": [
    {
      "name": "ClassName",
      "type": "MonoBehaviour | ScriptableObject | Plain C#",
      "purpose": "mục đích",
      "key_fields": ["field quan trọng"],
      "key_methods": ["method quan trọng"]
    }
  ],
  "events_published": ["event channel nó phát ra"],
  "events_subscribed": ["event channel nó lắng nghe"],
  "folder_path": "Assets/Scripts/...",
  "notes": "lưu ý khi implement"
}
```

### Các tài liệu bổ sung agent có thể tạo thêm

| Tài liệu | Khi nào cần |
|---|---|
| GDD đầy đủ | Khi onboard thêm người |
| Sơ đồ dependency giữa modules | Khi có > 5 module |
| Data schema (ScriptableObject fields) | Khi game có nhiều loại entity |
| Event flow diagram | Khi dùng event-driven architecture |
| Animation state machine spec | Khi nhân vật có nhiều state phức tạp |

### Output chuẩn — lưu vào `layer3/current.json` sau merge

```json
{
  "version": "v1.0",
  "layer": 3,
  "parent_versions": { "layer1": "v1.0", "layer2": "v1.0" },
  "status": "approved",
  "content": {
    "architecture_summary": "...",
    "folder_structure": "Assets/Scripts/Core/ | Player/ | ...",
    "global_patterns": ["Observer", "Object Pool"],
    "shared_events": ["OnPlayerDeath", "OnRoomComplete"],
    "modules": [ ... ],
    "notes": "..."
  }
}
```

### Điều kiện hoàn thành

- [ ] Tất cả module đã được soạn tài liệu
- [ ] Không có namespace hoặc event channel trùng lặp
- [ ] Validation L2→L3 pass (xem mục 13)
- [ ] Technical lead approve

---

## 7. Lớp 4 — Triển khai Unity C# Scripts

**Vai trò:** Đọc toàn bộ context pipeline, viết code C# đúng kiến trúc, đúng module, đúng đặc thù game.

**Input bắt buộc:** `layer1` + `layer2` + `layer3` đều đã `approved`.

### Thứ tự viết scripts (bắt buộc)

```
1. Data layer       → ScriptableObject definitions (stats, config, event channels)
2. Core systems     → GameManager, GameStateSO, SceneLoader
3. Player           → PlayerController, InputHandler
4. Gameplay loop    → module cốt lõi của genre (combat, dungeon, puzzle...)
5. Supporting       → Inventory, SaveSystem, AudioManager
6. UI               → HUD, MenuController
7. Utilities        → Extensions, Helpers, Constants
```

### Prompt mẫu cho agent (mỗi script)

```
Bạn là Unity C# developer. Viết script theo đúng context pipeline.

[DÁN layer1/current.json]
[DÁN layer2/current.json]
[DÁN phần module liên quan từ layer3/current.json]

Script cần viết: <ClassName> thuộc module <module_name>

Yêu cầu bắt buộc:
- Namespace: <theo folder_path trong Lớp 3>
- Implement đúng key_fields và key_methods đã định nghĩa ở Lớp 3
- [SerializeField] cho mọi field cần chỉnh Inspector, kèm [Header] và [Tooltip]
- Subscribe/unsubscribe event trong OnEnable/OnDisable
- Không dùng GameObject.Find() hoặc GetComponent() trong Update/FixedUpdate
- Comments tiếng Việt giải thích logic phức tạp
- #region phân chia: Fields | Unity Callbacks | Public Methods | Private Methods
- ScriptableObject phải có [CreateAssetMenu(menuName="...")]
```

### Checklist code trước khi submit

- [ ] Namespace khớp với folder structure Lớp 3
- [ ] Không có `GameObject.Find()` trong runtime loop
- [ ] Events unsubscribe trong `OnDisable` hoặc `OnDestroy`
- [ ] Không có magic numbers — dùng constants hoặc SerializeField
- [ ] Null check trước khi access component lấy từ bên ngoài
- [ ] `[Header]` và `[Tooltip]` cho SerializeField quan trọng
- [ ] `#region` phân chia rõ ràng
- [ ] Compile sạch, không warning Unity

### Lưu manifest sau mỗi script

Cập nhật `layer4/v1.0_manifest.json`:

```json
{
  "scripts": [
    {
      "file": "Assets/Scripts/Player/PlayerController.cs",
      "generated_from": { "layer1": "v1.0", "layer2": "v1.0", "layer3": "v1.0" },
      "module": "Player",
      "status": "generated | reviewed | approved",
      "last_updated": "2025-01-15"
    }
  ]
}
```

---

## 8. Testing Pipeline

Testing chạy **song song với Lớp 4**, không phải sau khi xong toàn bộ.

### 3 cấp độ testing

**Unit Test** (song song Lớp 4)
- Test từng class/method độc lập
- Dùng Unity Test Framework (EditMode tests)
- Agent viết test ngay sau khi viết script tương ứng

**Integration Test** (sau khi xong từng module)
- Test tương tác giữa các module
- Dùng Unity Test Framework (PlayMode tests)
- Tập trung vào event flow và state transition

**Playtest / Manual QA** (sau khi xong toàn bộ)
- Test gameplay loop, feel, balance
- Checklist do human thực hiện

### Prompt mẫu viết unit test

```
Viết Unity NUnit test cho class sau.

[DÁN nội dung file .cs]
[DÁN phần module từ layer3/current.json]

Yêu cầu:
- Dùng Unity Test Framework, EditMode
- Test tất cả public method và happy path
- Test edge cases: null input, giá trị biên, state không hợp lệ
- Tên test: MethodName_Condition_ExpectedResult
- Dùng [SetUp] khởi tạo, [TearDown] dọn dẹp
```

### Checklist trước khi merge vào main

- [ ] Unit tests pass 100%
- [ ] Không có compile error hoặc warning
- [ ] FPS ổn định ở target platform (kiểm bằng Unity Profiler)
- [ ] Memory allocation không có spike bất thường trong Profiler
- [ ] Integration tests pass cho các module tương tác nhau

---

## 9. Asset Pipeline

Chạy song song với Lớp 3 và Lớp 4. Agent hỗ trợ tạo asset specification — không tạo asset trực tiếp.

### Asset được xác định qua từng lớp

- **Lớp 1:** art_style → quyết định tool chain; platform → resolution, compression
- **Lớp 2:** asset_pipeline field → tool cụ thể (Aseprite, Blender, FMOD...)
- **Lớp 3:** agent sinh asset spec cho từng module
- **Lớp 4:** Addressables setup, asset loading strategy trong code

### Asset specification mẫu (agent tạo ở Lớp 3, per module)

```json
{
  "module": "Player",
  "sprites": [
    { "name": "player_idle", "frames": 4, "size": "32x32", "pivot": "bottom_center" },
    { "name": "player_run", "frames": 8, "size": "32x32" },
    { "name": "player_attack", "frames": 6, "size": "48x32" }
  ],
  "animations": [
    { "name": "Idle", "fps": 8, "loop": true },
    { "name": "Run", "fps": 12, "loop": true },
    { "name": "Attack", "fps": 12, "loop": false, "exit_to": "Idle" }
  ],
  "audio_events": [
    { "name": "player_footstep", "type": "sfx", "trigger": "animation frame 2,4" },
    { "name": "player_attack_swing", "type": "sfx", "trigger": "Attack frame 1" }
  ]
}
```

### Cấu trúc thư mục asset chuẩn

```
Assets/
  Art/
    Characters/Player/      ← sprite sheets, slices
    Environment/
    UI/
  Audio/
    SFX/
    Music/
    Ambience/
  Animations/
    Player/
    Enemies/
  Prefabs/
    Characters/
    Environment/
    UI/
  ScriptableObjects/
    Items/
    Events/
    Config/
  AddressableAssets/        ← nếu dùng Addressables
```

---

## 10. Build & Deploy

### Bước 1: Optimization pass

```
□ Unity Profiler: kiểm CPU, GPU, Memory
□ Texture compression theo platform (DXT cho PC, ASTC cho mobile)
□ Audio compression settings
□ Addressables build + catalog update
□ Strip unused code (Managed Stripping Level: Medium hoặc High)
□ Bật IL2CPP cho production build
```

### Bước 2: Build configuration

```
□ Build Settings đúng target platform
□ Player Settings: company name, version, bundle ID
□ Quality Settings theo platform
□ Scripting Backend: IL2CPP (production) / Mono (development)
□ Development Build = false trước khi release
□ Tắt tất cả debug log (#if UNITY_EDITOR hoặc conditional compile)
```

### Bước 3: QA build

```
□ Test trên device thật (không chỉ trong editor)
□ Kiểm tra save/load hoạt động đúng
□ Test crash scenarios
□ Tích hợp crash reporting (Firebase Crashlytics hoặc tương đương)
```

### Bước 4: Release

```
PC (Steam):   upload via steamcmd, set depot, publish branch
Mobile:       .aab lên Google Play / .ipa lên App Store
Web (WebGL):  deploy lên hosting, kiểm tra cross-browser
Cập nhật changelog.md và public release notes
```

### Build config lưu trong pipeline

```json
{
  "build_config": {
    "version": "0.1.0",
    "unity_version": "2022.3.x",
    "targets": [
      {
        "platform": "StandaloneWindows64",
        "scripting_backend": "IL2CPP",
        "development_build": false,
        "compression": "LZ4HC"
      }
    ],
    "defines": ["RELEASE"],
    "addressables_profile": "Release"
  }
}
```

---

## 11. Collaboration & Parallel Work

### Phân công theo lớp

| Lớp | Ai thực hiện | Song song được không |
|---|---|---|
| Lớp 1 | Game Designer + Product Owner | Không — cần đồng thuận |
| Lớp 2 | Technical Lead + Agent | Không — cần quyết định chọn phương án |
| Lớp 3 | Nhiều developer + Agent | **Có** — chia theo module |
| Lớp 4 | Nhiều developer + Agent | **Có** — chia theo module/script |
| Testing | QA Engineer + Agent | **Có** — song song với Lớp 4 |
| Asset Pipeline | Artist + Sound Designer | **Có** — song song từ Lớp 3 |

### Quy trình làm song song ở Lớp 3

```
Bước 1: Lead tạo danh sách module từ Lớp 2, phân công ai làm module nào
Bước 2: Thống nhất shared_events.json và namespace trước khi bắt đầu
Bước 3: Mỗi agent làm module riêng → lưu vào layer3/modules/<name>.json
Bước 4: Lead merge tất cả module → chạy conflict check
Bước 5: Conflict check tìm: class name trùng, event name trùng, menu path trùng
Bước 6: Resolve conflict → merge vào layer3/current.json → validate
```

### Khi 2 module cùng cần một class chung

Ví dụ: `HealthComponent` cần cho cả Player và Enemy:
- Đưa vào namespace `GameName.Core`
- Chỉ một người/agent viết, đặt trong `Assets/Scripts/Core/`
- Module còn lại import từ Core, không duplicate

---

## 12. Iteration & Incremental Update

### Bảng tác động khi thay đổi

| Thay đổi ở | Các lớp bị ảnh hưởng | Hành động |
|---|---|---|
| L1: sửa genre | L2, L3, L4 | Tái tạo L2 → review L3 → tái tạo L4 |
| L1: sửa art_style | Asset pipeline | Chỉ cập nhật asset spec, code không đổi |
| L1: thêm tính năng mới | L2 (đánh giá), L3 (module mới), L4 (scripts mới) | Thêm module mới, không sửa module cũ |
| L1: sửa scope/timeline | L2 | Re-evaluate phương án kỹ thuật |
| L2: đổi phương án | L3, L4 | Tái tạo hoàn toàn L3 và L4 |
| L2: thêm package | L3 (có thể), L4 | Review L3, cập nhật script liên quan |
| L3: sửa một module | Chỉ L4 của module đó | Tái tạo scripts của module đó |
| L4: refactor script | Không ảnh hưởng lớp trên | Cập nhật manifest, chạy lại tests |

### Hot reload — thay đổi không cần tái tạo pipeline

Các thay đổi này **không cần** chạy lại pipeline:
- Sửa giá trị số trong ScriptableObject (balance tuning)
- Thêm comment hoặc debug log
- Đổi tên biến nội bộ private
- Sửa bug logic trong method đã có

Các thay đổi **bắt buộc** tái tạo lớp liên quan:
- Thêm/xóa public method hoặc event
- Thay đổi quan hệ dependency giữa module
- Thêm tính năng mới vào gameplay loop
- Thay đổi data structure ScriptableObject

### Quy trình thay đổi chuẩn

```
1. Xác định thay đổi thuộc lớp nào
2. Tra bảng tác động → biết lớp nào bị ảnh hưởng
3. Tạo version mới cho lớp thay đổi (bump version)
4. Đánh dấu các lớp phụ thuộc là "stale" trong meta.json
5. Tái tạo lớp bị stale theo thứ tự từ trên xuống
6. Chạy lại validation
7. Chạy lại tests liên quan
8. Ghi vào changelog.md
```

---

## 13. Validation giữa các lớp

Agent phải chạy validation và nhận `"passed": true` trước khi chuyển lớp tiếp theo.

### Validation L1 (self-check)

```
Prompt:
"Kiểm tra Lớp 1 đã đủ để tiếp tục chưa.
[DÁN layer1/current.json]

Kiểm tra:
1. Đủ các trường bắt buộc chưa?
2. scope và timeline có hợp lý với genre và art_style không?
3. Có mâu thuẫn nội bộ không? (vd: muốn MMO nhưng solo indie 3 tháng)

Trả về: { 'passed': true/false, 'issues': ['mô tả vấn đề'] }"
```

### Validation L1 → L2

```
Prompt:
"Kiểm tra phương án kỹ thuật Lớp 2 có phù hợp với Lớp 1 không.
[DÁN layer1/current.json]
[DÁN layer2/current.json]

Kiểm tra:
1. Stack có phù hợp với art_style và platform trong Lớp 1?
2. Difficulty có phù hợp với scope và timeline trong Lớp 1?
3. Có package nào trong stack mâu thuẫn nhau không?
4. Design pattern có thực sự phù hợp với genre này không?

Trả về: { 'passed': true/false, 'issues': ['...'] }"
```

### Validation L2 → L3

```
Prompt:
"Kiểm tra tính nhất quán Lớp 3 trước khi viết code.
[DÁN layer2/current.json]
[DÁN layer3/current.json]

Kiểm tra:
1. Tất cả module có dùng đúng pattern đã chọn ở Lớp 2 không?
2. Có module nào bị thiếu so với yêu cầu gameplay Lớp 1 không?
3. Có dependency vòng tròn giữa các module không?
4. Event channel có bị trùng tên không?
5. Namespace có nhất quán với folder structure không?

Trả về: { 'passed': true/false, 'issues': ['...'] }"
```

### Validation L3 → L4 (self-check mỗi script)

```
□ Namespace đúng với folder_path trong Lớp 3
□ Implement đủ key_methods đã định nghĩa Lớp 3
□ Không GetComponent() trong Update()/FixedUpdate()
□ Event unsubscribe trong OnDisable/OnDestroy
□ Không có hardcoded string
□ Compile sạch, không warning
```

---

## 14. Error Handling & Recovery

### Phân loại lỗi và cách xử lý

| Loại lỗi | Ví dụ | Xử lý |
|---|---|---|
| **Input error** | Lớp 1 thiếu field bắt buộc | Dừng, hỏi bổ sung — không tiếp tục |
| **Validation error** | L2 không phù hợp L1 | Hiển thị issue cụ thể, yêu cầu sửa trước khi tiếp tục |
| **AI generation error** | API timeout, JSON parse fail | Retry tối đa 3 lần; nếu vẫn fail → log và báo human |
| **Conflict error** | Hai module trùng class name | Dừng merge, liệt kê conflict, assign người giải quyết |
| **Stale error** | Lớp trên đổi, lớp dưới chưa cập nhật | Warn rõ, đánh dấu "potentially_outdated", không block nhưng yêu cầu review |

### Rollback

Mỗi lớp giữ lịch sử version → rollback đơn giản:

```
Để rollback Lớp 3 về v1.0 (từ v1.1):
1. Copy layer3/v1.0.json → layer3/current.json
2. Cập nhật meta.json: "layer3": "v1.0"
3. Đánh dấu layer4 là "stale"
4. Thông báo team
5. Ghi lý do rollback vào changelog.md
```

### Recovery khi AI sinh output sai liên tục

```
1. Lưu failed output vào pipeline/failed/layer_X_attempt_N.json
2. Agent báo cáo: "Không thể generate output hợp lệ. Lý do: [mô tả]"
3. Human review prompt, bổ sung context còn thiếu
4. Thử lại với prompt đã cải thiện
5. Nếu vẫn fail sau 3 lần → human viết tay phần đó
   → đánh dấu "manually_authored": true trong manifest
```

### 3 câu hỏi agent tự hỏi khi gặp vấn đề

```
1. "Tôi có đủ thông tin từ lớp trước không?"
   → Nếu không: dừng, yêu cầu bổ sung

2. "Output tôi chuẩn bị tạo có nhất quán với toàn bộ pipeline không?"
   → Nếu không: chạy lại validation, tìm mâu thuẫn

3. "Thay đổi này ảnh hưởng đến lớp nào?"
   → Tra bảng tác động, thông báo trước khi tiếp tục
```

---

## 15. Ví dụ context đầy đủ

Context agent nhận khi bắt đầu Lớp 4:

```
=== LAYER 1: GAME CONCEPT (v1.1) ===
Tên game: Shadow Realm
Thể loại: 2D Action Roguelike
Phong cách: Pixel art dark fantasy
Nền tảng: PC (Steam)
Cốt truyện: Người chơi là linh hồn bị kẹt trong vương quốc bóng tối,
  phải vượt qua các tầng ngục để thoát ra.
Gameplay: Procedural dungeon, permadeath, combat real-time,
  hệ thống relic thay đổi playstyle mỗi run.
Tham khảo: Hades + Dead Cells
Scope: solo indie | Timeline: 6 tháng

=== LAYER 2: TECHNICAL APPROACH (v1.0) ===
Phương án đã chọn: B — Event-driven ScriptableObject Architecture
Mô tả: ScriptableObject làm data layer và event channel,
  tách biệt hoàn toàn logic và data.
Stack: Unity 2022 LTS 2D, ScriptableObject Events, Cinemachine, DOTween
Pattern: Observer, Command, Object Pool
Asset pipeline: Aseprite → Unity Sprite Atlas, FMOD
Risks: Learning curve SO events nếu chưa quen
Difficulty: Intermediate | Timeline: 4 tháng

=== LAYER 3: TECHNICAL DOCS (v1.0) ===
Kiến trúc: Data-driven với ScriptableObject event channels.
  GameManager điều phối state machine. Không dùng Singleton runtime.
Shared events: OnPlayerDeath, OnRelicPickup, OnRoomComplete, OnDamageTaken
Modules:
  PlayerController (Rigidbody2D, Animator, CapsuleCollider2D)
  DungeonGenerator (Tilemap, Grid, RuleTile)
  RelicSystem (ScriptableObject, List<RelicSO>)
  CombatSystem (Hitbox, HealthComponent, DamageFlash)
  UIManager (Canvas, TextMeshPro, DOTween)
Folder: Assets/Scripts/Core/ | Player/ | Dungeon/ | Combat/ | Relic/ | UI/
Patterns: Observer (events), Object Pool (enemies, projectiles), Factory (rooms)
Lưu ý: Không dùng Singleton — dùng ScriptableObject GameStateSO làm shared state
```

---

## Phụ lục: Quick Reference cho Agent

### Bắt đầu một lớp mới

```
1. Đọc meta.json → xác nhận lớp trước đã "approved"
2. Load current.json của tất cả lớp trước
3. Chạy validation prompt tương ứng → chờ "passed: true"
4. Generate output → lưu vào vX.Y.json mới
5. Cập nhật meta.json
6. Báo cáo và chờ human approve
```

### Khi nhận yêu cầu thay đổi

```
"Thay đổi này ở lớp nào?
 Theo bảng tác động, các lớp sau sẽ bị ảnh hưởng: [liệt kê]
 Xác nhận để tiếp tục?"
```

### Khi phát hiện mâu thuẫn

```
"Phát hiện mâu thuẫn:
 Lớp X yêu cầu: [...]
 Lớp Y chọn: [...]
 Mâu thuẫn vì: [lý do cụ thể]
 Cần human quyết định trước khi tiếp tục."
```

### Khi AI generation fail

```
"Lần thử [N/3] thất bại. Lý do có thể: [mô tả]
 Đề xuất: [bổ sung thông tin X] để cải thiện kết quả.
 [Nếu lần 3] → Chuyển sang human viết tay, tôi sẽ hỗ trợ review."
```

---

*Phiên bản: 2.0 — Unity Engine — AI Agent Pipeline*  
*Cập nhật: bổ sung storage, versioning, validation, testing, asset pipeline, build/deploy, collaboration, iteration, error handling*