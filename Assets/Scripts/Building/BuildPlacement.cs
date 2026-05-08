using UnityEngine;

namespace NextDay.Building
{
    /// <summary>
    /// Xử lý validation vị trí đặt building và snap to grid.
    /// </summary>
    public class BuildPlacement : MonoBehaviour
    {
        #region Fields

        [Header("Grid Settings")]
        [Tooltip("Kích thước mỗi ô grid (world units)")]
        [SerializeField] private float _cellSize = 1f;

        [Tooltip("Offset của grid so với world origin")]
        [SerializeField] private Vector3 _gridOffset = Vector3.zero;

        [Header("Validation")]
        [Tooltip("Layer chứa các building đã đặt (để check overlap)")]
        [SerializeField] private LayerMask _buildingLayer;

        [Tooltip("Layer chứa các obstacle không thể xây (nước, đá...)")]
        [SerializeField] private LayerMask _obstacleLayer;

        #endregion

        #region Properties

        public float CellSize => _cellSize;

        #endregion

        #region Public Methods

        /// <summary>
        /// Snap vị trí world về tâm ô grid gần nhất.
        /// </summary>
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            float x = Mathf.Round((worldPosition.x - _gridOffset.x) / _cellSize) * _cellSize + _gridOffset.x;
            float y = Mathf.Round((worldPosition.y - _gridOffset.y) / _cellSize) * _cellSize + _gridOffset.y;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Kiểm tra vị trí có hợp lệ để đặt building không.
        /// Trả về true nếu không có building hoặc obstacle nào overlap.
        /// </summary>
        public bool ValidatePosition(Vector3 gridPosition, Vector2Int gridSize)
        {
            // Tính toán vùng kiểm tra dựa trên gridSize
            Vector2 checkSize = new Vector2(
                gridSize.x * _cellSize * 0.9f,
                gridSize.y * _cellSize * 0.9f
            );

            // Kiểm tra overlap với building đã đặt
            Collider2D buildingOverlap = Physics2D.OverlapBox(
                gridPosition,
                checkSize,
                0f,
                _buildingLayer
            );

            if (buildingOverlap != null)
            {
                Debug.Log("[BuildPlacement] Vị trí đã có building khác!");
                return false;
            }

            // Kiểm tra overlap với obstacle
            Collider2D obstacleOverlap = Physics2D.OverlapBox(
                gridPosition,
                checkSize,
                0f,
                _obstacleLayer
            );

            if (obstacleOverlap != null)
            {
                Debug.Log("[BuildPlacement] Vị trí bị chặn bởi obstacle!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Chuyển đổi vị trí world sang tọa độ grid.
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt((worldPosition.x - _gridOffset.x) / _cellSize);
            int y = Mathf.RoundToInt((worldPosition.y - _gridOffset.y) / _cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Chuyển đổi tọa độ grid sang vị trí world.
        /// </summary>
        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            float x = gridPosition.x * _cellSize + _gridOffset.x;
            float y = gridPosition.y * _cellSize + _gridOffset.y;
            return new Vector3(x, y, 0f);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            // Vẽ grid trong editor
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
            int gridRange = 10;

            for (int x = -gridRange; x <= gridRange; x++)
            {
                for (int y = -gridRange; y <= gridRange; y++)
                {
                    Vector3 pos = new Vector3(
                        x * _cellSize + _gridOffset.x,
                        y * _cellSize + _gridOffset.y,
                        0f
                    );
                    Gizmos.DrawWireCube(pos, new Vector3(_cellSize, _cellSize, 0f));
                }
            }
        }

        #endregion
    }
}
