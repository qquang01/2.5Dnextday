using UnityEngine;
using NextDay.Events;

namespace NextDay.Defense
{
    /// <summary>
    /// Component phòng thủ cho tower — tự động tìm và bắn kẻ thù trong phạm vi.
    /// Gắn vào building có loại Tower.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class TowerDefense : MonoBehaviour
    {
        #region Fields

        [Header("Tower Stats")]
        [Tooltip("Phạm vi phát hiện kẻ thù")]
        [SerializeField] private float _range = 5f;

        [Tooltip("Sát thương mỗi phát bắn")]
        [SerializeField] private float _damage = 15f;

        [Tooltip("Tốc độ bắn (phát/giây)")]
        [SerializeField] private float _fireRate = 1f;

        [Header("Targeting")]
        [Tooltip("Layer chứa kẻ thù")]
        [SerializeField] private LayerMask _enemyLayer;

        [Header("Event Channels")]
        [Tooltip("Channel lắng nghe event game state")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        private HealthComponent _healthComponent;
        private Transform _currentTarget;
        private float _lastFireTime;
        private bool _isActive;

        #endregion

        #region Properties

        public float Range => _range;
        public float Damage => _damage;
        public bool IsActive => _isActive;
        public Transform CurrentTarget => _currentTarget;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnNightStart += HandleNightStart;
                _gameEventChannel.OnDayStart += HandleDayStart;
            }

            if (_healthComponent != null)
            {
                _healthComponent.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnNightStart -= HandleNightStart;
                _gameEventChannel.OnDayStart -= HandleDayStart;
            }

            if (_healthComponent != null)
            {
                _healthComponent.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            if (!_isActive || _healthComponent.IsDead)
            {
                return;
            }

            FindTarget();
            TryFire();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo tower stats — gọi sau khi đặt building.
        /// </summary>
        public void Initialize(float range, float damage, float fireRate)
        {
            _range = range;
            _damage = damage;
            _fireRate = fireRate;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tìm kẻ thù gần nhất trong phạm vi.
        /// </summary>
        private void FindTarget()
        {
            // Kiểm tra target hiện tại còn hợp lệ không
            if (_currentTarget != null)
            {
                float distance = Vector2.Distance(transform.position, _currentTarget.position);
                if (distance > _range || !_currentTarget.gameObject.activeInHierarchy)
                {
                    _currentTarget = null;
                }
            }

            // Nếu không có target, tìm mới
            if (_currentTarget == null)
            {
                _currentTarget = FindNearestEnemy();
            }
        }

        /// <summary>
        /// Tìm enemy gần nhất trong phạm vi.
        /// </summary>
        private Transform FindNearestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                _range,
                _enemyLayer
            );

            Transform nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                // Chỉ target enemy còn sống
                if (hit.TryGetComponent(out HealthComponent health) && !health.IsDead)
                {
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = hit.transform;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Bắn nếu có target và hết cooldown.
        /// </summary>
        private void TryFire()
        {
            if (_currentTarget == null)
            {
                return;
            }

            float fireInterval = _fireRate > 0f ? 1f / _fireRate : 1f;

            if (Time.time - _lastFireTime < fireInterval)
            {
                return;
            }

            _lastFireTime = Time.time;
            Fire();
        }

        /// <summary>
        /// Thực hiện bắn — gửi damage đến target.
        /// </summary>
        private void Fire()
        {
            if (_currentTarget == null)
            {
                return;
            }

            _currentTarget.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[TowerDefense] Bắn {_currentTarget.name} — {_damage} damage");

            // TODO: Spawn projectile visual / particle effect
        }

        /// <summary>
        /// Khi đêm bắt đầu → kích hoạt tower.
        /// </summary>
        private void HandleNightStart()
        {
            _isActive = true;
            Debug.Log("[TowerDefense] Tower kích hoạt — phòng thủ ban đêm!");
        }

        /// <summary>
        /// Khi ngày bắt đầu → tắt tower.
        /// </summary>
        private void HandleDayStart()
        {
            _isActive = false;
            _currentTarget = null;
        }

        /// <summary>
        /// Khi tower bị phá hủy.
        /// </summary>
        private void HandleDeath()
        {
            _isActive = false;
            _currentTarget = null;
            Debug.Log("[TowerDefense] Tower đã bị phá hủy!");
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Vẽ phạm vi bắn (xanh dương)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _range);
        }

        #endregion
    }
}
