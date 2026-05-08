using UnityEngine;
using NextDay.Events;

namespace NextDay.Resource
{
    /// <summary>
    /// Config cho từng loại tài nguyên — số lượng mỗi lần thu thập, thời gian harvest.
    /// </summary>
    [CreateAssetMenu(menuName = "NextDay/Config/Resource Data")]
    public class ResourceDataSO : ScriptableObject
    {
        #region Fields

        [Header("Thông tin tài nguyên")]
        [Tooltip("Loại tài nguyên")]
        [SerializeField] private ResourceType _resourceType;

        [Tooltip("Tên hiển thị")]
        [SerializeField] private string _displayName = "Resource";

        [Header("Thu thập")]
        [Tooltip("Số lượng mỗi lần thu thập")]
        [SerializeField] private int _amountPerHit = 1;

        [Tooltip("Thời gian chờ giữa các lần thu thập (giây)")]
        [SerializeField] private float _harvestTime = 1f;

        [Tooltip("Tổng lượng tài nguyên trong node trước khi hết")]
        [SerializeField] private int _maxAmount = 10;

        [Header("Visual")]
        [Tooltip("Icon hiển thị trong inventory")]
        [SerializeField] private Sprite _icon;

        #endregion

        #region Properties

        public ResourceType ResourceType => _resourceType;
        public string DisplayName => _displayName;
        public int AmountPerHit => _amountPerHit;
        public float HarvestTime => _harvestTime;
        public int MaxAmount => _maxAmount;
        public Sprite Icon => _icon;

        #endregion
    }
}
