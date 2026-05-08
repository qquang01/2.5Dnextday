# LAYER 3: TECHNICAL PLAN — NextDay

> Created: 2026-05-08  
> Input: layer1_output.md + layer2_output.md  
> Status: Draft

---

## Kiến trúc tổng quan

Data-driven architecture với ScriptableObject event channels. GameManager điều phối state machine cho Day/Night cycle. Mỗi hệ thống subscribe/unsubscribe event channels riêng. Không dùng Singleton runtime — dùng ScriptableObject GameStateSO làm shared state.

---

## Folder Structure

```
Assets/
  Scripts/
    Core/
      GameManager.cs
      GameStateSO.cs
      DayNightCycleSO.cs
      SceneLoader.cs
    Player/
      PlayerController.cs
      InputHandler.cs
      PlayerHealth.cs
      Inventory.cs
    Resource/
      ResourceNode.cs
      ResourceCollector.cs
      ResourceDataSO.cs
    Building/
      BuildingSystem.cs
      BuildingPreview.cs
      BuildingDataSO.cs
      BuildPlacement.cs
    Defense/
      EnemySpawner.cs
      EnemyAI.cs
      HealthComponent.cs
      TowerDefense.cs
    UI/
      HUDController.cs
      InventoryUI.cs
      BuildMenuUI.cs
  ScriptableObjects/
    Config/
      GameConfigSO.cs
      PlayerStatsSO.cs
      BuildingConfigSO.cs
      EnemyConfigSO.cs
    Events/
      GameEventChannel.cs
      ResourceEventChannel.cs
      BuildingEventChannel.cs
      DefenseEventChannel.cs
  Art/
    Characters/
      Player/
      Enemies/
    Environment/
      Terrain/
      Vegetation/
    Buildings/
  Audio/
    SFX/
    Music/
```

---

## Modules

### Module 1: Core

**Responsibility:** Điều phối game state, day/night cycle, scene loading.

**Classes:**
- `GameManager.cs` (MonoBehaviour)
  - Key methods: `Initialize()`, `Update()`, `OnDayStart()`, `OnNightStart()`
  - Key fields: `_gameStateSO`, `_dayNightCycleSO`
- `GameStateSO.cs` (ScriptableObject)
  - Key fields: `currentDay`, `isNight`, `isPaused`
  - Purpose: Shared state không Singleton runtime
- `DayNightCycleSO.cs` (ScriptableObject)
  - Key fields: `dayDuration`, `nightDuration`, `currentTime`
  - Key methods: `UpdateCycle()`, `IsNight()`
- `SceneLoader.cs` (MonoBehaviour)
  - Key methods: `LoadScene()`, `UnloadScene()`

**Events Published:**
- `OnDayStart` → Khi ngày bắt đầu
- `OnNightStart` → Khi đêm bắt đầu
- `OnGamePaused` → Khi game pause

**Events Subscribed:**
- `OnPlayerDeath` → Kích hoạt game over screen

**Unity Components:**
- MonoBehaviour (GameManager, SceneLoader)
- ScriptableObject (GameStateSO, DayNightCycleSO)

**AI Asset Spec:**
- Concept art cho UI day/night indicator
- Texture cho skybox (day sky, night sky)
- Icon cho pause menu

---

### Module 2: Player

**Responsibility:** Di chuyển, input, inventory, health.

**Classes:**
- `PlayerController.cs` (MonoBehaviour)
  - Key methods: `Move()`, `Interact()`, `Attack()`
  - Key fields: `_inputHandler`, `_playerHealth`, `_inventory`
- `InputHandler.cs` (MonoBehaviour)
  - Key methods: `GetMovementInput()`, `GetInteractInput()`
  - Purpose: Wrapper cho Unity Input System
- `PlayerHealth.cs` (MonoBehaviour)
  - Key methods: `TakeDamage()`, `Heal()`, `Die()`
  - Key fields: `_currentHealth`, `_maxHealth`
- `Inventory.cs` (MonoBehaviour)
  - Key methods: `AddItem()`, `RemoveItem()`, `GetItemCount()`
  - Key fields: `_items`, `_maxSlots`

**Events Published:**
- `OnResourceCollected` → Khi thu thập tài nguyên
- `OnPlayerDamaged` → Khi player bị damage
- `OnPlayerDeath` → Khi player chết

**Events Subscribed:**
- `OnDayStart` → Reset daily stats
- `OnNightStart` → Enable combat mode

**Unity Components:**
- Rigidbody2D (player movement)
- Collider2D (player collision)
- Animator (player animation)

**AI Asset Spec:**
- Concept art cho player character (isometric view)
- Texture cho player model (low-poly 3D)
- Animation sprites (idle, walk, attack, death)
- Icons cho inventory items

---

### Module 3: Resource

**Responsibility:** Tài nguyên, thu thập, resource nodes.

**Classes:**
- `ResourceNode.cs` (MonoBehaviour)
  - Key methods: `Harvest()`, `Deplete()`
  - Key fields: `_resourceDataSO`, `_currentAmount`
- `ResourceCollector.cs` (MonoBehaviour)
  - Key methods: `Collect()`, `GetResourceType()`
  - Key fields: `_playerInventory`, `_range`
- `ResourceDataSO.cs` (ScriptableObject)
  - Key fields: `resourceType`, `amountPerHit`, `harvestTime`
  - Purpose: Config cho từng loại tài nguyên

**Events Published:**
- `OnResourceCollected` → Khi thu thập thành công
- `OnResourceDepleted` → Khi resource node hết

**Events Subscribed:**
- `OnDayStart` → Spawn resource nodes (nếu procedural)

**Unity Components:**
- Collider2D (resource node)
- SpriteRenderer (resource visual)

**AI Asset Spec:**
- Concept art cho resource types (wood, stone, metal)
- Texture cho resource nodes (tree, rock, metal ore)
- Icons cho inventory display
- Particle effect cho harvesting

---

### Module 4: Building

**Responsibility:** Xây dựng căn cứ, placement, building data.

**Classes:**
- `BuildingSystem.cs` (MonoBehaviour)
  - Key methods: `PlaceBuilding()`, `RemoveBuilding()`, `GetBuildingCost()`
  - Key fields: `_buildingDataSO`, `_inventory`
- `BuildingPreview.cs` (MonoBehaviour)
  - Key methods: `ShowPreview()`, `HidePreview()`, `CanPlace()`
  - Key fields: `_buildingDataSO`, `_previewSprite`
- `BuildingDataSO.cs` (ScriptableObject)
  - Key fields: `buildingType`, `cost`, `health`, `defenseValue`
  - Purpose: Config cho từng loại building
- `BuildPlacement.cs` (MonoBehaviour)
  - Key methods: `ValidatePosition()`, `SnapToGrid()`

**Events Published:**
- `OnBuildingPlaced` → Khi đặt building thành công
- `OnBuildingRemoved` → Khi xóa building

**Events Subscribed:**
- `OnResourceCollected` → Check đủ tài nguyên để build
- `OnNightStart` → Enable defensive mode

**Unity Components:**
- Collider2D (building collision)
- SpriteRenderer (building visual)
- Grid (placement grid)

**AI Asset Spec:**
- Concept art cho building types (wall, tower, storage)
- Texture cho buildings (wood, stone, metal materials)
- Icons cho build menu
- Preview sprites cho placement mode

---

### Module 5: Defense

**Responsibility:** Kẻ thù, spawner, health, tower defense.

**Classes:**
- `EnemySpawner.cs` (MonoBehaviour)
  - Key methods: `SpawnWave()`, `SpawnEnemy()`
  - Key fields: `_enemyConfigSO`, `_spawnPoints`
- `EnemyAI.cs` (MonoBehaviour)
  - Key methods: `ChasePlayer()`, `AttackBuilding()`
  - Key fields: `_target`, `_state`
- `HealthComponent.cs` (MonoBehaviour)
  - Key methods: `TakeDamage()`, `Die()`
  - Key fields: `_currentHealth`, `_maxHealth`
- `TowerDefense.cs` (MonoBehaviour)
  - Key methods: `TargetEnemy()`, `Fire()`
  - Key fields: `_range`, `_damage`, `_fireRate`

**Events Published:**
- `OnEnemySpawned` → Khi kẻ thù spawn
- `OnEnemyDeath` → Khi kẻ thù chết
- `OnBuildingDamaged` → Khi building bị damage

**Events Subscribed:**
- `OnNightStart` → Start spawning waves
- `OnBuildingPlaced` → Update defense towers

**Unity Components:**
- Rigidbody2D (enemy movement)
- Collider2D (enemy/building collision)
- SpriteRenderer (enemy visual)

**AI Asset Spec:**
- Concept art cho enemy types (basic, fast, tank)
- Texture cho enemy models (low-poly alien creatures)
- Animation sprites (idle, walk, attack, death)
- Icons cho enemy health bar
- Particle effect cho death

---

## Module 6: UI

**Responsibility:** HUD, inventory UI, build menu.

**Classes:**
- `HUDController.cs` (MonoBehaviour)
  - Key methods: `UpdateHealth()`, `UpdateDayNightIndicator()`
  - Key fields: `_healthBar`, `_dayNightIcon`
- `InventoryUI.cs` (MonoBehaviour)
  - Key methods: `UpdateInventorySlots()`, `ShowTooltip()`
  - Key fields: `_inventorySlots`, `_tooltipPanel`
- `BuildMenuUI.cs` (MonoBehaviour)
  - Key methods: `ShowBuildMenu()`, `SelectBuilding()`
  - Key fields: `_buildingButtons`, `_costDisplay`

**Events Subscribed:**
- `OnPlayerDamaged` → Update health bar
- `OnDayStart` / `OnNightStart` → Update day/night indicator
- `OnResourceCollected` → Update inventory UI
- `OnBuildingPlaced` → Update build menu

**Unity Components:**
- Canvas (UI root)
- TextMeshPro (text display)
- Image (icons, sprites)

**AI Asset Spec:**
- Icons cho HUD elements (health, resources, day/night)
- UI background textures
- Button sprites for build menu
- Tooltip panel design

---

## Shared Events

### GameEventChannel.cs (ScriptableObject)
- `OnDayStart` → void
- `OnNightStart` → void
- `OnGamePaused` → void
- `OnPlayerDeath` → void

### ResourceEventChannel.cs (ScriptableObject)
- `OnResourceCollected` → ResourceType, amount
- `OnResourceDepleted` → ResourceNode

### BuildingEventChannel.cs (ScriptableObject)
- `OnBuildingPlaced` → BuildingType, position
- `OnBuildingRemoved` → BuildingType, position

### DefenseEventChannel.cs (ScriptableObject)
- `OnEnemySpawned` → EnemyType, position
- `OnEnemyDeath` → EnemyType
- `OnBuildingDamaged` → BuildingType, damage

---

## Global Patterns

- **Observer:** ScriptableObject Event Channels
- **State Machine:** Day/Night cycle, Enemy AI states
- **Object Pool:** Enemy projectiles, particle effects
- **Command:** Build/place action, resource collection action

---

## Namespace Structure

- `NextDay.Core` — GameManager, GameStateSO, DayNightCycleSO
- `NextDay.Player` — PlayerController, InputHandler, PlayerHealth
- `NextDay.Resource` — ResourceNode, ResourceCollector
- `NextDay.Building` — BuildingSystem, BuildingPreview
- `NextDay.Defense` — EnemySpawner, EnemyAI, HealthComponent
- `NextDay.UI` — HUDController, InventoryUI, BuildMenuUI

---

## Ghi chú quan trọng

- Không dùng Singleton runtime cho game state — dùng ScriptableObject GameStateSO
- Events phải unsubscribe trong OnDisable/OnDestroy để tránh memory leak
- Use TryGetComponent thay vì GetComponent để tránh null reference
- Physics calculations trong FixedUpdate() với Time.fixedDeltaTime
- Input và non-physics trong Update() với Time.deltaTime
- Comments tiếng Việt giải thích logic phức tạp
- [SerializeField] cho field cần chỉnh Inspector, kèm [Header] và [Tooltip]

---

## Mục tiêu Phase 1 Vertical Slice

- 1 ngày + 1 đêm với:
  - Player di chuyển và thu thập tài nguyên cơ bản
  - Xây dựng 1-2 loại building (wall, tower)
  - Đêm có kẻ thù spawn và attack
  - HUD hiển thị health, resources, day/night indicator
  - Inventory cơ bản (hiển thị, add/remove)
