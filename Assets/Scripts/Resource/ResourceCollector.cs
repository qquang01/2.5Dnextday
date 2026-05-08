using UnityEngine;
using NextDay.Events;
using NextDay.Player;

namespace NextDay.Resource
{
    /// <summary>
    /// Component gắn vào player — xử lý thu thập tài nguyên từ ResourceNode.
    /// Tìm node gần nhất trong phạm vi, thu thập và thêm vào inventory.
    /// </summary>
    public class ResourceCollector : MonoBehaviour
    {
        #region Fields

        [Header("Collection Settings")]
        [Tooltip("Khoảng cách tối đa để thu thập")]
        [SerializeField] private float _collectRange = 2f;

        [Tooltip("Layer chứa resource nodes")]
        [SerializeField] private LayerMask _resourceLayer;

        [Header("References")]
        [Tooltip("Inventory của player")]
        [SerializeField] private Inventory _playerInventory;

        [Header("Event Channels")]
        [Tooltip("Channel phát event tài nguyên")]
        [SerializeField] private ResourceEventChannel _resourceEventChannel;

        private float _lastCollectTime;

        #endregion

        #region Properties

        public float CollectRange => _collectRange;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            // Tìm Inventory trên cùng GameObject nếu chưa gán
            if (_playerInventory == null)
            {
                TryGetComponent(out _playerInventory);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Thu thập tài nguyên từ node gần nhất trong phạm vi.
        /// Gọi từ PlayerController khi nhấn key tương tác.
        /// </summary>
        public bool Collect()
        {
            ResourceNode nearestNode = FindNearestResourceNode();

            if (nearestNode == null)
            {
                Debug.Log("[ResourceCollector] Không có resource node nào trong phạm vi.");
                return false;
            }

            if (nearestNode.ResourceData == null)
            {
                return false;
            }

            // Kiểm tra cooldown thu thập
            if (Time.time - _lastCollectTime < nearestNode.ResourceData.HarvestTime)
            {
                return false;
            }

            _lastCollectTime = Time.time;

            int amount = nearestNode.Harvest();

            if (amount <= 0)
            {
                return false;
            }

            // Thêm vào inventory
            if (_playerInventory != null)
            {
                ResourceType type = nearestNode.ResourceData.ResourceType;
                bool added = _playerInventory.AddItem(type, amount);

                if (!added)
                {
                    Debug.LogWarning("[ResourceCollector] Inventory đầy, không thể thu thập.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Lấy loại tài nguyên của node gần nhất (để hiển thị UI hint).
        /// </summary>
        public ResourceType? GetNearestResourceType()
        {
            ResourceNode node = FindNearestResourceNode();
            if (node != null && node.ResourceData != null)
            {
                return node.ResourceData.ResourceType;
            }
            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tìm ResourceNode gần nhất trong phạm vi thu thập.
        /// </summary>
        private ResourceNode FindNearestResourceNode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                _collectRange,
                _resourceLayer
            );

            ResourceNode nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out ResourceNode node) && !node.IsDepleted)
                {
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = node;
                    }
                }
            }

            return nearest;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Vẽ phạm vi thu thập (vàng)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _collectRange);
        }

        #endregion
    }
}
