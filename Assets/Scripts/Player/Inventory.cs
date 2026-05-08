using UnityEngine;
using System.Collections.Generic;
using NextDay.Events;

namespace NextDay.Player
{
    /// <summary>
    /// Mục trong inventory — loại tài nguyên và số lượng.
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public ResourceType ResourceType;
        public int Amount;

        public InventoryItem(ResourceType type, int amount)
        {
            ResourceType = type;
            Amount = amount;
        }
    }

    /// <summary>
    /// Hệ thống inventory cơ bản — lưu trữ tài nguyên thu thập được.
    /// Publish event khi thêm/xóa item.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        #region Fields

        [Header("Inventory Settings")]
        [Tooltip("Số slot tối đa trong inventory")]
        [SerializeField] private int _maxSlots = 20;

        [Header("Event Channels")]
        [Tooltip("Channel phát event tài nguyên")]
        [SerializeField] private ResourceEventChannel _resourceEventChannel;

        private Dictionary<ResourceType, int> _items = new Dictionary<ResourceType, int>();

        #endregion

        #region Properties

        public int MaxSlots => _maxSlots;

        /// <summary>Số loại tài nguyên đang giữ.</summary>
        public int UsedSlots => _items.Count;

        /// <summary>Còn slot trống hay không.</summary>
        public bool HasFreeSlot => _items.Count < _maxSlots;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _items = new Dictionary<ResourceType, int>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Thêm tài nguyên vào inventory. Trả về true nếu thành công.
        /// </summary>
        public bool AddItem(ResourceType type, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (_items.ContainsKey(type))
            {
                _items[type] += amount;
            }
            else
            {
                // Kiểm tra còn slot trống không
                if (_items.Count >= _maxSlots)
                {
                    Debug.LogWarning("[Inventory] Inventory đầy! Không thể thêm tài nguyên.");
                    return false;
                }

                _items[type] = amount;
            }

            Debug.Log($"[Inventory] +{amount} {type}. Tổng: {_items[type]}");

            if (_resourceEventChannel != null)
            {
                _resourceEventChannel.RaiseResourceCollected(type, amount);
            }

            return true;
        }

        /// <summary>
        /// Xóa tài nguyên khỏi inventory. Trả về true nếu đủ để xóa.
        /// </summary>
        public bool RemoveItem(ResourceType type, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (!_items.ContainsKey(type) || _items[type] < amount)
            {
                Debug.LogWarning($"[Inventory] Không đủ {type}. Có: {GetItemCount(type)}, cần: {amount}");
                return false;
            }

            _items[type] -= amount;

            // Xóa slot nếu hết
            if (_items[type] <= 0)
            {
                _items.Remove(type);
            }

            Debug.Log($"[Inventory] -{amount} {type}. Còn: {GetItemCount(type)}");
            return true;
        }

        /// <summary>
        /// Lấy số lượng tài nguyên theo loại.
        /// </summary>
        public int GetItemCount(ResourceType type)
        {
            return _items.ContainsKey(type) ? _items[type] : 0;
        }

        /// <summary>
        /// Kiểm tra có đủ tài nguyên hay không.
        /// </summary>
        public bool HasEnough(ResourceType type, int amount)
        {
            return GetItemCount(type) >= amount;
        }

        /// <summary>
        /// Lấy danh sách tất cả items trong inventory.
        /// </summary>
        public List<InventoryItem> GetAllItems()
        {
            List<InventoryItem> result = new List<InventoryItem>();
            foreach (KeyValuePair<ResourceType, int> pair in _items)
            {
                result.Add(new InventoryItem(pair.Key, pair.Value));
            }
            return result;
        }

        /// <summary>
        /// Xóa toàn bộ inventory.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            Debug.Log("[Inventory] Đã xóa toàn bộ inventory.");
        }

        #endregion
    }
}
