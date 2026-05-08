using UnityEngine;
using NextDay.Events;
using NextDay.Building;

namespace NextDay.Defense
{
    /// <summary>
    /// Trạng thái AI của kẻ thù.
    /// </summary>
    public enum EnemyState
    {
        Idle,
        ChasePlayer,
        ChaseBuilding,
        AttackPlayer,
        AttackBuilding,
        Dead
    }

    /// <summary>
    /// AI kẻ thù — đuổi player hoặc tấn công building gần nhất.
    /// State machine đơn giản: Idle → Chase → Attack.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyAI : MonoBehaviour
    {
        #region Fields

        [Header("Config")]
        [Tooltip("ScriptableObject config cho kẻ thù này")]
        [SerializeField] private EnemyConfigSO _enemyConfigSO;

        [Header("Event Channels")]
        [Tooltip("Channel phát event defense")]
        [SerializeField] private DefenseEventChannel _defenseEventChannel;

        [Header("Detection")]
        [Tooltip("Layer chứa player")]
        [SerializeField] private LayerMask _playerLayer;

        [Tooltip("Layer chứa buildings")]
        [SerializeField] private LayerMask _buildingLayer;

        private Rigidbody2D _rigidbody;
        private HealthComponent _healthComponent;
        private EnemyState _currentState = EnemyState.Idle;
        private Transform _target;
        private float _lastAttackTime;

        #endregion

        #region Properties

        public EnemyState CurrentState => _currentState;
        public EnemyConfigSO EnemyConfig => _enemyConfigSO;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _healthComponent = GetComponent<HealthComponent>();

            // Cấu hình Rigidbody2D
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
        }

        private void OnEnable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDeath -= HandleDeath;
            }
        }

        private void Start()
        {
            // Khởi tạo stats từ config
            if (_enemyConfigSO != null && _healthComponent != null)
            {
                _healthComponent.Initialize(_enemyConfigSO.MaxHealth);
            }
        }

        private void Update()
        {
            if (_currentState == EnemyState.Dead)
            {
                return;
            }

            UpdateState();
        }

        private void FixedUpdate()
        {
            if (_currentState == EnemyState.Dead)
            {
                return;
            }

            ExecuteState();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo enemy với config — gọi sau khi spawn.
        /// </summary>
        public void Initialize(EnemyConfigSO config, DefenseEventChannel defenseChannel)
        {
            _enemyConfigSO = config;
            _defenseEventChannel = defenseChannel;

            if (_healthComponent != null && config != null)
            {
                _healthComponent.Initialize(config.MaxHealth);
            }

            _currentState = EnemyState.Idle;
        }

        #endregion

        #region Private Methods — State Machine

        /// <summary>
        /// Cập nhật state dựa trên điều kiện hiện tại.
        /// </summary>
        private void UpdateState()
        {
            if (_enemyConfigSO == null)
            {
                return;
            }

            float detectionRange = _enemyConfigSO.DetectionRange;
            float attackRange = _enemyConfigSO.AttackRange;

            // Tìm player
            Collider2D playerHit = Physics2D.OverlapCircle(
                transform.position,
                detectionRange,
                _playerLayer
            );

            // Tìm building gần nhất
            Collider2D buildingHit = Physics2D.OverlapCircle(
                transform.position,
                detectionRange,
                _buildingLayer
            );

            // Ưu tiên: player > building
            if (playerHit != null)
            {
                _target = playerHit.transform;
                float distance = Vector2.Distance(transform.position, _target.position);

                _currentState = distance <= attackRange
                    ? EnemyState.AttackPlayer
                    : EnemyState.ChasePlayer;
            }
            else if (buildingHit != null)
            {
                _target = buildingHit.transform;
                float distance = Vector2.Distance(transform.position, _target.position);

                _currentState = distance <= attackRange
                    ? EnemyState.AttackBuilding
                    : EnemyState.ChaseBuilding;
            }
            else
            {
                _target = null;
                _currentState = EnemyState.Idle;
            }
        }

        /// <summary>
        /// Thực thi hành vi theo state hiện tại — chạy trong FixedUpdate.
        /// </summary>
        private void ExecuteState()
        {
            switch (_currentState)
            {
                case EnemyState.Idle:
                    // Đứng yên, chờ phát hiện mục tiêu
                    break;

                case EnemyState.ChasePlayer:
                case EnemyState.ChaseBuilding:
                    ChaseTarget();
                    break;

                case EnemyState.AttackPlayer:
                    AttackTarget();
                    break;

                case EnemyState.AttackBuilding:
                    AttackTarget();
                    break;
            }
        }

        /// <summary>
        /// Di chuyển về phía mục tiêu.
        /// </summary>
        private void ChaseTarget()
        {
            if (_target == null || _enemyConfigSO == null)
            {
                return;
            }

            Vector2 direction = ((Vector2)_target.position - _rigidbody.position).normalized;
            Vector2 newPosition = _rigidbody.position + direction * _enemyConfigSO.MoveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);
        }

        /// <summary>
        /// Tấn công mục tiêu (player hoặc building).
        /// </summary>
        private void AttackTarget()
        {
            if (_target == null || _enemyConfigSO == null)
            {
                return;
            }

            if (Time.time - _lastAttackTime < _enemyConfigSO.AttackCooldown)
            {
                return;
            }

            _lastAttackTime = Time.time;

            // Gửi damage đến mục tiêu
            _target.SendMessage("TakeDamage", _enemyConfigSO.Damage, SendMessageOptions.DontRequireReceiver);

            // Nếu tấn công building → publish event
            if (_currentState == EnemyState.AttackBuilding && _defenseEventChannel != null)
            {
                BuildingType targetBuildingType = BuildingType.Wall;
                if (_target.TryGetComponent(out BuildingIdentifier identifier))
                {
                    targetBuildingType = identifier.BuildingType;
                }
                _defenseEventChannel.RaiseBuildingDamaged(targetBuildingType, _enemyConfigSO.Damage);
            }

            Debug.Log($"[EnemyAI] {_enemyConfigSO.DisplayName} tấn công {_target.name}!");
        }

        /// <summary>
        /// Xử lý khi enemy chết.
        /// </summary>
        private void HandleDeath()
        {
            _currentState = EnemyState.Dead;

            if (_defenseEventChannel != null && _enemyConfigSO != null)
            {
                _defenseEventChannel.RaiseEnemyDeath(_enemyConfigSO.EnemyType);
            }

            Debug.Log($"[EnemyAI] {(_enemyConfigSO != null ? _enemyConfigSO.DisplayName : "Enemy")} đã chết!");

            // Disable sau 1 khoảng thời gian (cho animation)
            Destroy(gameObject, 0.5f);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (_enemyConfigSO == null)
            {
                return;
            }

            // Vẽ phạm vi phát hiện (vàng)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _enemyConfigSO.DetectionRange);

            // Vẽ phạm vi tấn công (đỏ)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _enemyConfigSO.AttackRange);
        }

        #endregion
    }
}
