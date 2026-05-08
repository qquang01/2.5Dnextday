using UnityEngine;

namespace NextDay.Building
{
    /// <summary>
    /// Hiển thị preview sprite khi player đang chọn vị trí đặt building.
    /// Đổi màu xanh/đỏ tùy vị trí hợp lệ hay không.
    /// </summary>
    public class BuildingPreview : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [Tooltip("BuildPlacement để validate vị trí")]
        [SerializeField] private BuildPlacement _buildPlacement;

        [Header("Preview Colors")]
        [Tooltip("Màu khi vị trí hợp lệ")]
        [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.5f);

        [Tooltip("Màu khi vị trí không hợp lệ")]
        [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.5f);

        private GameObject _previewObject;
        private SpriteRenderer _previewRenderer;
        private BuildingDataSO _currentBuildingData;
        private bool _isActive;
        private bool _isValidPosition;

        #endregion

        #region Properties

        public bool IsActive => _isActive;
        public bool IsValidPosition => _isValidPosition;
        public BuildingDataSO CurrentBuildingData => _currentBuildingData;

        #endregion

        #region Public Methods

        /// <summary>
        /// Hiển thị preview cho building đã chọn.
        /// </summary>
        public void ShowPreview(BuildingDataSO buildingData)
        {
            if (buildingData == null)
            {
                return;
            }

            _currentBuildingData = buildingData;
            _isActive = true;

            // Tạo preview object nếu chưa có
            if (_previewObject == null)
            {
                _previewObject = new GameObject("BuildingPreview");
                _previewRenderer = _previewObject.AddComponent<SpriteRenderer>();
                _previewRenderer.sortingOrder = 100;
            }

            // Set sprite preview
            if (buildingData.PreviewSprite != null)
            {
                _previewRenderer.sprite = buildingData.PreviewSprite;
            }
            else if (buildingData.BuildingSprite != null)
            {
                _previewRenderer.sprite = buildingData.BuildingSprite;
            }

            _previewObject.SetActive(true);
            Debug.Log($"[BuildingPreview] Hiển thị preview: {buildingData.DisplayName}");
        }

        /// <summary>
        /// Ẩn preview.
        /// </summary>
        public void HidePreview()
        {
            _isActive = false;
            _currentBuildingData = null;

            if (_previewObject != null)
            {
                _previewObject.SetActive(false);
            }
        }

        /// <summary>
        /// Cập nhật vị trí preview theo vị trí chuột (snap to grid).
        /// Trả về true nếu vị trí hợp lệ.
        /// </summary>
        public bool UpdatePreviewPosition(Vector3 worldPosition)
        {
            if (!_isActive || _previewObject == null || _buildPlacement == null)
            {
                return false;
            }

            // Snap to grid
            Vector3 snappedPosition = _buildPlacement.SnapToGrid(worldPosition);
            _previewObject.transform.position = snappedPosition;

            // Validate vị trí
            Vector2Int gridSize = _currentBuildingData != null
                ? _currentBuildingData.GridSize
                : Vector2Int.one;

            _isValidPosition = _buildPlacement.ValidatePosition(snappedPosition, gridSize);

            // Đổi màu theo validation
            if (_previewRenderer != null)
            {
                _previewRenderer.color = _isValidPosition ? _validColor : _invalidColor;
            }

            return _isValidPosition;
        }

        /// <summary>
        /// Lấy vị trí đã snap to grid hiện tại của preview.
        /// </summary>
        public Vector3 GetCurrentPosition()
        {
            return _previewObject != null ? _previewObject.transform.position : Vector3.zero;
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            if (_previewObject != null)
            {
                Destroy(_previewObject);
            }
        }

        #endregion
    }
}
