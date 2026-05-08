# LAYER 4: IMPLEMENTATION PLAN — NextDay

> Created: 2026-05-08  
> Input: layer1_output.md + layer2_output.md + layer3_output.md  
> Status: Draft

---

## Tổng quan

Lớp 4 (Triển khai Code) được chia thành 7 phase để quản lý độ phức tạp. Mỗi phase có thể test độc lập trước khi sang phase tiếp theo.

---

## Phase 1: Foundation (Data Layer + Core Systems)

**Mục tiêu:** Xây dựng nền tảng data và core systems mà các module khác sẽ phụ thuộc vào.

**Thứ tự:**
1. ScriptableObject base classes
2. Event channels
3. GameStateSO
4. DayNightCycleSO
5. GameManager

**Files:**
- `Assets/ScriptableObjects/Events/GameEventChannel.cs`
- `Assets/ScriptableObjects/Events/ResourceEventChannel.cs`
- `Assets/ScriptableObjects/Events/BuildingEventChannel.cs`
- `Assets/ScriptableObjects/Events/DefenseEventChannel.cs`
- `Assets/ScriptableObjects/Config/GameStateSO.cs`
- `Assets/ScriptableObjects/Config/DayNightCycleSO.cs`
- `Assets/Scripts/Core/GameManager.cs`

**DoD (Definition of Done):**
- [ ] Event channels compile sạch, có [CreateAssetMenu]
- [ ] GameStateSO và DayNightCycleSO có đầy đủ field
- [ ] GameManager khởi tạo game state, subscribe events
- [ ] Day/Night cycle chạy được (log ra console khi chuyển ngày/đêm)
- [ ] Không có Singleton runtime

**Ước tính thời gian:** 1-2 ngày

---

## Phase 2: Player + Input

**Mục tiêu:** Player có thể di chuyển và nhận input cơ bản.

**Thứ tự:**
1. InputHandler
2. PlayerController
3. PlayerHealth
4. Inventory (cơ bản)

**Files:**
- `Assets/Scripts/Player/InputHandler.cs`
- `Assets/Scripts/Player/PlayerController.cs`
- `Assets/Scripts/Player/PlayerHealth.cs`
- `Assets/Scripts/Player/Inventory.cs`

**DoD:**
- [ ] Player di chuyển được với WASD/Arrow keys
- [ ] InputHandler wrap Unity Input System
- [ ] PlayerHealth có TakeDamage/Heal, publish OnPlayerDamaged/OnPlayerDeath
- [ ] Inventory có AddItem/RemoveItem cơ bản
- [ ] Player publish OnResourceCollected khi tương tác với resource node (placeholder)

**Ước tính thời gian:** 2-3 ngày

---

## Phase 3: Resource System

**Mục tiêu:** Resource node và thu thập tài nguyên cơ bản.

**Thứ tự:**
1. ResourceDataSO
2. ResourceNode
3. ResourceCollector

**Files:**
- `Assets/ScriptableObjects/Config/ResourceDataSO.cs`
- `Assets/Scripts/Resource/ResourceNode.cs`
- `Assets/Scripts/Resource/ResourceCollector.cs`

**DoD:**
- [ ] ResourceDataSO có đầy đủ field (resourceType, amountPerHit, harvestTime)
- [ ] ResourceNode có collider, visual sprite, publish OnResourceDepleted khi hết
- [ ] ResourceCollector thu thập được tài nguyên, publish OnResourceCollected
- [ ] Player có thể thu thập resource node khi đứng gần và nhấn key
- [ ] Inventory cập nhật khi thu thập

**Ước tính thời gian:** 2-3 ngày

---

## Phase 4: Building System

**Mục tiêu:** Xây dựng căn cứ cơ bản với placement preview.

**Thứ tự:**
1. BuildingDataSO
2. BuildPlacement
3. BuildingPreview
4. BuildingSystem

**Files:**
- `Assets/ScriptableObjects/Config/BuildingDataSO.cs`
- `Assets/Scripts/Building/BuildPlacement.cs`
- `Assets/Scripts/Building/BuildingPreview.cs`
- `Assets/Scripts/Building/BuildingSystem.cs`

**DoD:**
- [ ] BuildingDataSO có đầy đủ field (buildingType, cost, health, defenseValue)
- [ ] BuildPlacement validate position và snap to grid
- [ ] BuildingPreview hiển thị sprite preview khi chọn building
- [ ] BuildingSystem place building khi click, check đủ tài nguyên
- [ ] Building có collider, visual sprite, publish OnBuildingPlaced
- [ ] Inventory trừ tài nguyên khi place building

**Ước tính thời gian:** 3-4 ngày

---

## Phase 5: Defense System

**Mục tiêu:** Kẻ thù spawn và attack building vào ban đêm.

**Thứ tự:**
1. EnemyConfigSO
2. HealthComponent (tái sử dụng cho cả player và enemy)
3. EnemyAI
4. EnemySpawner
5. TowerDefense (optional cho vertical slice)

**Files:**
- `Assets/ScriptableObjects/Config/EnemyConfigSO.cs`
- `Assets/Scripts/Defense/HealthComponent.cs`
- `Assets/Scripts/Defense/EnemyAI.cs`
- `Assets/Scripts/Defense/EnemySpawner.cs`
- `Assets/Scripts/Defense/TowerDefense.cs` (optional)

**DoD:**
- [ ] EnemyConfigSO có đầy đủ field (enemyType, health, damage, speed)
- [ ] HealthComponent có TakeDamage/Die, publish OnEnemyDeath
- [ ] EnemyAI chase player hoặc attack building
- [ ] EnemySpawner spawn wave khi đêm bắt đầu
- [ ] Enemy publish OnEnemySpawned khi spawn
- [ ] TowerDefense (nếu có) target enemy và fire

**Ước tính thời gian:** 3-4 ngày

---

## Phase 6: UI

**Mục tiêu:** HUD hiển thị health, resources, day/night indicator.

**Thứ tự:**
1. HUDController
2. InventoryUI
3. BuildMenuUI

**Files:**
- `Assets/Scripts/UI/HUDController.cs`
- `Assets/Scripts/UI/InventoryUI.cs`
- `Assets/Scripts/UI/BuildMenuUI.cs`

**DoD:**
- [ ] HUDController hiển thị health bar, update khi OnPlayerDamaged
- [ ] HUDController hiển thị day/night icon, update khi OnDayStart/OnNightStart
- [ ] InventoryUI hiển thị slots, update khi OnResourceCollected
- [ ] BuildMenuUI hiển thị building buttons, select building
- [ ] UI subscribe đúng events, unsubscribe trong OnDisable

**Ước tính thời gian:** 2-3 ngày

---

## Phase 7: Integration + Testing

**Mục tiêu:** Tích hợp tất cả module, test vertical slice hoàn chỉnh.

**Thứ tự:**
1. Tích hợp Day/Night cycle với tất cả hệ thống
2. Test vertical slice: 1 ngày + 1 đêm
3. Fix bugs
4. Optimize (nếu cần)
5. Build QA

**DoD:**
- [ ] Day/Night cycle hoạt động đúng: ngày → thu thập/xây dựng → đêm → spawn enemy/phòng thủ
- [ ] Player có thể thu thập tài nguyên → xây dựng building → phòng thủ đêm
- [ ] UI hiển thị đúng thông tin
- [ ] Không có memory leak (events unsubscribe)
- [ ] FPS ổn định (~60 FPS cho PC)
- [ ] Build Development Build chạy được
- [ ] 1-2 Unit Tests cho class quan trọng (GameStateSO, ResourceCollector)

**Ước tính thời gian:** 3-4 ngày

---

## Tổng thời gian ước tính

- Phase 1: 1-2 ngày
- Phase 2: 2-3 ngày
- Phase 3: 2-3 ngày
- Phase 4: 3-4 ngày
- Phase 5: 3-4 ngày
- Phase 6: 2-3 ngày
- Phase 7: 3-4 ngày

**Tổng:** 16-23 ngày (~3-4 tuần)

---

## Quy tắc khi triển khai

- Mỗi phase phải đạt DoD mới sang phase tiếp theo
- Commit sau mỗi phase hoàn thành
- Unit test cho class quan trọng (GameStateSO, ResourceCollector, BuildingSystem)
- Đừng cố implement tất cả features trong 1 phase — focus vertical slice
- Nếu gặp block > 0.5 ngày → hạ phạm vi hoặc hỏi trợ giúp

---

## AI Asset Generation trong Phase

**Phase 1:** Generate concept cho day/night indicator icons
**Phase 2:** Generate concept cho player character
**Phase 3:** Generate textures cho resource nodes
**Phase 4:** Generate textures cho buildings
**Phase 5:** Generate concept cho enemy types
**Phase 6:** Generate UI icons (buttons, HUD elements)

AI assets có thể song song với coding — không block progress.

---

## Phân công AI Tools

### Devin → Phase 1-5 (Foundation → Defense)

**Nhiệm vụ:**
- Implement toàn bộ code cho Phase 1-5 theo layer4_plan.md
- Tự động tạo folder structure, viết code, compile, fix lỗi
- Chạy build test sau mỗi phase để đảm bảo không break
- Tạo ScriptableObject assets trong Unity Editor (nếu có thể)

**Lý do:**
- Devin tốt cho "tạo từ đầu" (greenfield) với nhiều file
- Có thể tự động implement nhiều file liên tiếp
- Phù hợp các phase nặng cần nhiều file và logic phức tạp
- Có thể tự quản lý task phức tạp

**Input cho Devin:**
- layer1_output.md
- layer2_output.md
- layer3_output.md
- layer4_plan.md (Phase 1-5)
- Unity project path

---

### Windsurf → Phase 6-7 (UI + Integration + Testing)

**Nhiệm vụ:**
- Implement UI (Phase 6) với tweak visual trực tiếp trong IDE
- Integration và Testing (Phase 7) với debug từng lỗi cụ thể
- Review code Devin đã viết, refactor nếu cần
- Fix minor issues, tweak logic, optimize

**Lý do:**
- Windsurf tích hợp IDE, có thể preview và chỉnh trực tiếp
- Tốt cho "tweak và debug" (incremental changes)
- UI cần nhiều visual tweak, Windsurf phù hợp hơn
- Có thể đọc console và fix lỗi nhanh

**Input cho Windsurf:**
- layer1_output.md
- layer2_output.md
- layer3_output.md
- layer4_plan.md (Phase 6-7)
- Code đã implement bởi Devin (Phase 1-5)

---

## Workflow tổng thể

1. **Devin** implement Phase 1 → compile → test
2. **Devin** implement Phase 2 → compile → test
3. **Devin** implement Phase 3 → compile → test
4. **Devin** implement Phase 4 → compile → test
5. **Devin** implement Phase 5 → compile → test
6. **Windsurf** review code Phase 1-5, fix minor issues
7. **Windsurf** implement Phase 6 (UI) → test
8. **Windsurf** implement Phase 7 (Integration + Testing) → build QA
9. **Devin** chạy build QA cuối cùng, optimize nếu cần

---

## Ghi chú cho AI Tools

**Devin:**
- Đọc toàn bộ context pipeline (Lớp 1-3) trước khi implement
- Theo đúng namespace structure trong layer3_output.md
- Sử dụng Unity best practices (không Singleton runtime, events unsubscribe)
- Comments tiếng Việt giải thích logic phức tạp
- Commit sau mỗi phase hoàn thành

**Windsurf:**
- Review code Devin viết trước khi implement UI
- Tweak visual cho UI cho phù hợp với art style 2.5D Isometric
- Debug từng lỗi cụ thể, đọc Unity console log
- Optimize performance nếu cần (Profiler check)
