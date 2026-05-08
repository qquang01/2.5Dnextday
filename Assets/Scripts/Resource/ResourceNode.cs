using UnityEngine;
using NextDay.Events;

namespace NextDay.Resource
{
    /// <summary>
    /// Node tài nguyên trong map — player tương tác để thu thập.
    /// Hết tài nguyên thì publish event và tự disable.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ResourceNode : MonoBehaviour
    {
        #region Fields

        [Header("Resource Config")]
        [Tooltip("ScriptableObject config cho loại tài nguyên này")]
        [SerializeField] private ResourceDataSO _resourceDataSO;

        [Header("Event Channels")]
        [Tooltip("Channel phát event tài nguyên")]
        [SerializeField] private ResourceEventChannel _resourceEventChannel;

        [Tooltip("Channel phát event game state")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        [Header("Visual")]
        [Tooltip("Sprite khi node còn tài nguyên")]
        [SerializeField] private Sprite _fullSprite;

        [Tooltip("Sprite khi node hết tài nguyên")]
        [SerializeField] private Sprite _depletedSprite;

        private int _currentAmount;
        private SpriteRenderer _spriteRenderer;
        private bool _isDepleted;

        #endregion

        #region Properties

        public ResourceDataSO ResourceData => _resourceDataSO;
        public int CurrentAmount => _currentAmount;
        public bool IsDepleted => _isDepleted;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            ResetNode();
        }

        private void OnEnable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnDayStart += HandleDayStart;
            }
        }

        private void OnDisable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnDayStart -= HandleDayStart;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Thu thập tài nguyên từ node. Trả về số lượng thực tế thu được.
        /// </summary>
        public int Harvest()
        {
            if (_isDepleted || _resourceDataSO == null)
            {
                return 0;
            }

            int harvestAmount = Mathf.Min(_resourceDataSO.AmountPerHit, _currentAmount);
            _currentAmount -= harvestAmount;

            Debug.Log($"[ResourceNode] Thu thập {harvestAmount} {_resourceDataSO.ResourceType}. Còn: {_currentAmount}");

            if (_currentAmount <= 0)
            {
                Deplete();
            }

            return harvestAmount;
        }

        /// <summary>
        /// Được gọi khi player tương tác (qua SendMessage từ PlayerController).
        /// </summary>
        public void OnInteract(GameObject interactor)
        {
            if (_isDepleted || _resourceDataSO == null)
            {
                return;
            }

            int amount = Harvest();

            if (amount > 0 && _resourceEventChannel != null)
            {
                _resourceEventChannel.RaiseResourceCollected(_resourceDataSO.ResourceType, amount);
            }
        }

        /// <summary>
        /// Reset node về trạng thái đầy — dùng khi respawn resource.
        /// </summary>
        public void ResetNode()
        {
            if (_resourceDataSO != null)
            {
                _currentAmount = _resourceDataSO.MaxAmount;
            }

            _isDepleted = false;
            UpdateVisual();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Node hết tài nguyên — đổi sprite, publish event.
        /// </summary>
        private void Deplete()
        {
            _isDepleted = true;
            _currentAmount = 0;

            UpdateVisual();

            if (_resourceEventChannel != null)
            {
                _resourceEventChannel.RaiseResourceDepleted(gameObject);
            }

            Debug.Log($"[ResourceNode] {_resourceDataSO.DisplayName} đã hết tài nguyên!");
        }

        /// <summary>
        /// Cập nhật sprite theo trạng thái.
        /// </summary>
        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            if (_isDepleted && _depletedSprite != null)
            {
                _spriteRenderer.sprite = _depletedSprite;
            }
            else if (!_isDepleted && _fullSprite != null)
            {
                _spriteRenderer.sprite = _fullSprite;
            }
        }

        /// <summary>
        /// Khi ngày mới bắt đầu — reset resource node (respawn tài nguyên).
        /// </summary>
        private void HandleDayStart()
        {
            if (_isDepleted)
            {
                ResetNode();
                Debug.Log($"[ResourceNode] {_resourceDataSO.DisplayName} đã được respawn!");
            }
        }

        #endregion
    }
}
