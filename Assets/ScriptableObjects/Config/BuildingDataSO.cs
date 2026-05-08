using UnityEngine;
using NextDay.Events;

namespace NextDay.Building
{
    /// <summary>
    /// Yêu cầu tài nguyên để xây building.
    /// </summary>
    [System.Serializable]
    public class BuildingCost
    {
        [Tooltip("Loại tài nguyên")]
        public ResourceType ResourceType;

        [Tooltip("Số lượng cần")]
        public int Amount;
    }

    /// <summary>
    /// Config cho từng loại building — chi phí, HP, giá trị phòng thủ.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Config/Building Data")]
    public class BuildingDataSO : ScriptableObject
    {
        #region Fields

        [Header("Thông tin building")]
        [Tooltip("Loại building")]
        [SerializeField] private BuildingType _buildingType;

        [Tooltip("Tên hiển thị")]
        [SerializeField] private string _displayName = "Building";

        [Tooltip("Mô tả ngắn")]
        [SerializeField] private string _description = "";

        [Header("Stats")]
        [Tooltip("HP tối đa của building")]
        [SerializeField] private float _maxHealth = 100f;

        [Tooltip("Giá trị phòng thủ (giảm damage nhận vào)")]
        [SerializeField] private float _defenseValue;

        [Header("Chi phí xây dựng")]
        [Tooltip("Danh sách tài nguyên cần để xây")]
        [SerializeField] private BuildingCost[] _costs;

        [Header("Kích thước")]
        [Tooltip("Kích thước trên grid (cells)")]
        [SerializeField] private Vector2Int _gridSize = Vector2Int.one;

        [Header("Visual")]
        [Tooltip("Sprite hiển thị khi đã xây")]
        [SerializeField] private Sprite _buildingSprite;

        [Tooltip("Sprite preview khi đang chọn vị trí")]
        [SerializeField] private Sprite _previewSprite;

        [Tooltip("Icon trong build menu")]
        [SerializeField] private Sprite _icon;

        [Header("Prefab")]
        [Tooltip("Prefab của building khi đặt vào scene")]
        [SerializeField] private GameObject _buildingPrefab;

        #endregion

        #region Properties

        public BuildingType BuildingType => _buildingType;
        public string DisplayName => _displayName;
        public string Description => _description;
        public float MaxHealth => _maxHealth;
        public float DefenseValue => _defenseValue;
        public BuildingCost[] Costs => _costs;
        public Vector2Int GridSize => _gridSize;
        public Sprite BuildingSprite => _buildingSprite;
        public Sprite PreviewSprite => _previewSprite;
        public Sprite Icon => _icon;
        public GameObject BuildingPrefab => _buildingPrefab;

        #endregion
    }
}
