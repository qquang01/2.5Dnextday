using UnityEngine;
using System.Collections.Generic;
using NextDay.Events;
using NextDay.Player;

namespace NextDay.Building
{
    /// <summary>
    /// Hệ thống xây dựng chính — quản lý đặt/xóa building, kiểm tra tài nguyên.
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [Tooltip("Inventory của player để kiểm tra/trừ tài nguyên")]
        [SerializeField] private Inventory _inventory;

        [Tooltip("Component placement để snap to grid và validate")]
        [SerializeField] private BuildPlacement _buildPlacement;

        [Tooltip("Component preview hiển thị vị trí đặt")]
        [SerializeField] private BuildingPreview _buildingPreview;

        [Header("Building Data")]
        [Tooltip("Danh sách building có thể xây")]
        [SerializeField] private BuildingDataSO[] _availableBuildings;

        [Header("Event Channels")]
        [Tooltip("Channel phát event building")]
        [SerializeField] private BuildingEventChannel _buildingEventChannel;

        [Tooltip("Channel phát event game state")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        private int _selectedBuildingIndex = -1;
        private bool _isBuildMode;
        private List<GameObject> _placedBuildings = new List<GameObject>();

        #endregion

        #region Properties

        public bool IsBuildMode => _isBuildMode;
        public int SelectedBuildingIndex => _selectedBuildingIndex;
        public BuildingDataSO[] AvailableBuildings => _availableBuildings;
        public int PlacedBuildingCount => _placedBuildings.Count;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnNightStart += HandleNightStart;
            }
        }

        private void OnDisable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnNightStart -= HandleNightStart;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Bật/tắt chế độ xây dựng.
        /// </summary>
        public void ToggleBuildMode()
        {
            _isBuildMode = !_isBuildMode;

            if (_isBuildMode)
            {
                Debug.Log("[BuildingSystem] Chế độ xây dựng BẬT.");
                if (_selectedBuildingIndex >= 0 && _selectedBuildingIndex < _availableBuildings.Length
                    && _buildingPreview != null)
                {
                    _buildingPreview.ShowPreview(_availableBuildings[_selectedBuildingIndex]);
                }
            }
            else
            {
                Debug.Log("[BuildingSystem] Chế độ xây dựng TẮT.");
                if (_buildingPreview != null)
                {
                    _buildingPreview.HidePreview();
                }
                _selectedBuildingIndex = -1;
            }
        }

        /// <summary>
        /// Chọn building theo index trong danh sách.
        /// </summary>
        public void SelectBuilding(int index)
        {
            if (index < 0 || index >= _availableBuildings.Length)
            {
                Debug.LogWarning("[BuildingSystem] Index building không hợp lệ.");
                return;
            }

            _selectedBuildingIndex = index;
            _isBuildMode = true;

            if (_buildingPreview != null)
            {
                _buildingPreview.ShowPreview(_availableBuildings[index]);
            }

            Debug.Log($"[BuildingSystem] Chọn building: {_availableBuildings[index].DisplayName}");
        }

        /// <summary>
        /// Đặt building tại vị trí preview hiện tại.
        /// Kiểm tra tài nguyên và vị trí hợp lệ trước khi đặt.
        /// </summary>
        public bool PlaceBuilding()
        {
            if (!_isBuildMode || _selectedBuildingIndex < 0)
            {
                return false;
            }

            BuildingDataSO buildingData = _availableBuildings[_selectedBuildingIndex];

            // Kiểm tra vị trí hợp lệ
            if (_buildingPreview != null && !_buildingPreview.IsValidPosition)
            {
                Debug.LogWarning("[BuildingSystem] Vị trí không hợp lệ!");
                return false;
            }

            // Kiểm tra đủ tài nguyên
            if (!HasEnoughResources(buildingData))
            {
                Debug.LogWarning("[BuildingSystem] Không đủ tài nguyên!");
                return false;
            }

            // Trừ tài nguyên
            DeductResources(buildingData);

            // Đặt building
            Vector3 position = _buildingPreview != null
                ? _buildingPreview.GetCurrentPosition()
                : transform.position;

            GameObject building = InstantiateBuilding(buildingData, position);

            if (building != null)
            {
                _placedBuildings.Add(building);

                // Publish event
                if (_buildingEventChannel != null)
                {
                    _buildingEventChannel.RaiseBuildingPlaced(buildingData.BuildingType, position);
                }

                Debug.Log($"[BuildingSystem] Đặt {buildingData.DisplayName} tại {position}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Xóa building tại vị trí chỉ định.
        /// </summary>
        public bool RemoveBuilding(GameObject building)
        {
            if (building == null || !_placedBuildings.Contains(building))
            {
                return false;
            }

            _placedBuildings.Remove(building);
            Vector3 position = building.transform.position;

            // TODO: Hoàn trả một phần tài nguyên nếu cần

            Destroy(building);

            Debug.Log($"[BuildingSystem] Xóa building tại {position}");
            return true;
        }

        /// <summary>
        /// Lấy chi phí xây building (dạng text để hiển thị UI).
        /// </summary>
        public string GetBuildingCostText(BuildingDataSO buildingData)
        {
            if (buildingData == null || buildingData.Costs == null)
            {
                return "Free";
            }

            string costText = "";
            foreach (BuildingCost cost in buildingData.Costs)
            {
                if (costText.Length > 0)
                {
                    costText += ", ";
                }
                costText += $"{cost.ResourceType}: {cost.Amount}";
            }

            return costText;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Kiểm tra inventory có đủ tài nguyên để xây building không.
        /// </summary>
        private bool HasEnoughResources(BuildingDataSO buildingData)
        {
            if (_inventory == null || buildingData.Costs == null)
            {
                return true;
            }

            foreach (BuildingCost cost in buildingData.Costs)
            {
                if (!_inventory.HasEnough(cost.ResourceType, cost.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Trừ tài nguyên từ inventory khi xây building.
        /// </summary>
        private void DeductResources(BuildingDataSO buildingData)
        {
            if (_inventory == null || buildingData.Costs == null)
            {
                return;
            }

            foreach (BuildingCost cost in buildingData.Costs)
            {
                _inventory.RemoveItem(cost.ResourceType, cost.Amount);
            }
        }

        /// <summary>
        /// Tạo building instance từ prefab hoặc tạo mới nếu không có prefab.
        /// </summary>
        private GameObject InstantiateBuilding(BuildingDataSO buildingData, Vector3 position)
        {
            GameObject building;

            if (buildingData.BuildingPrefab != null)
            {
                building = Instantiate(buildingData.BuildingPrefab, position, Quaternion.identity);
            }
            else
            {
                // Tạo building placeholder nếu chưa có prefab
                building = new GameObject($"Building_{buildingData.DisplayName}");
                building.transform.position = position;

                SpriteRenderer sr = building.AddComponent<SpriteRenderer>();
                if (buildingData.BuildingSprite != null)
                {
                    sr.sprite = buildingData.BuildingSprite;
                }

                BuildingIdentifier identifier = building.AddComponent<BuildingIdentifier>();
                identifier.Initialize(buildingData.BuildingType);

                BoxCollider2D collider = building.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(
                    buildingData.GridSize.x,
                    buildingData.GridSize.y
                );
            }

            return building;
        }

        /// <summary>
        /// Khi đêm bắt đầu — tắt build mode, chuyển sang phòng thủ.
        /// </summary>
        private void HandleNightStart()
        {
            if (_isBuildMode)
            {
                _isBuildMode = false;
                if (_buildingPreview != null)
                {
                    _buildingPreview.HidePreview();
                }
                Debug.Log("[BuildingSystem] Đêm bắt đầu — tắt chế độ xây dựng!");
            }
        }

        #endregion
    }
}
