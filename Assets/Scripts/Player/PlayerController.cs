using UnityEngine;
using NextDay.Events;

namespace NextDay.Player
{
    /// <summary>
    /// Điều khiển player — di chuyển, tương tác, tấn công.
    /// Sử dụng Rigidbody2D cho physics-based movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(InputHandler))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields

        [Header("Movement")]
        [Tooltip("Tốc độ di chuyển (units/giây)")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Interaction")]
        [Tooltip("Khoảng cách tối đa để tương tác với object")]
        [SerializeField] private float _interactRange = 1.5f;

        [Tooltip("Layer của các object có thể tương tác")]
        [SerializeField] private LayerMask _interactableLayer;

        [Header("Attack")]
        [Tooltip("Sát thương cơ bản")]
        [SerializeField] private float _baseDamage = 10f;

        [Tooltip("Khoảng cách tấn công")]
        [SerializeField] private float _attackRange = 1.2f;

        [Tooltip("Thời gian cooldown giữa các đòn tấn công (giây)")]
        [SerializeField] private float _attackCooldown = 0.5f;

        [Tooltip("Layer chứa kẻ thù (để attack chỉ target enemy)")]
        [SerializeField] private LayerMask _enemyLayer;

        [Header("Event Channels")]
        [Tooltip("Channel phát event game state")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        private Rigidbody2D _rigidbody;
        private InputHandler _inputHandler;
        private float _lastAttackTime;
        private bool _isCombatMode;

        #endregion

        #region Properties

        public float MoveSpeed => _moveSpeed;
        public float BaseDamage => _baseDamage;
        public bool IsCombatMode => _isCombatMode;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _inputHandler = GetComponent<InputHandler>();

            // Cấu hình Rigidbody2D cho top-down 2.5D
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
        }

        private void OnEnable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnDayStart += HandleDayStart;
                _gameEventChannel.OnNightStart += HandleNightStart;
            }
        }

        private void OnDisable()
        {
            if (_gameEventChannel != null)
            {
                _gameEventChannel.OnDayStart -= HandleDayStart;
                _gameEventChannel.OnNightStart -= HandleNightStart;
            }
        }

        private void Update()
        {
            HandleInteraction();
            HandleAttack();
        }

        private void FixedUpdate()
        {
            Move();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tương tác với object gần nhất trong phạm vi.
        /// </summary>
        public void Interact()
        {
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position,
                _interactRange,
                _interactableLayer
            );

            if (hit != null)
            {
                Debug.Log($"[PlayerController] Tương tác với: {hit.gameObject.name}");
                // Gửi message tương tác đến object
                hit.SendMessage("OnInteract", gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Tấn công trong phạm vi.
        /// </summary>
        public void Attack()
        {
            if (Time.time - _lastAttackTime < _attackCooldown)
            {
                return;
            }

            _lastAttackTime = Time.time;

            // Tìm kẻ thù trong phạm vi tấn công
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                _attackRange,
                _enemyLayer
            );

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject)
                {
                    continue;
                }

                // Gửi damage đến object có HealthComponent
                hit.SendMessage("TakeDamage", _baseDamage, SendMessageOptions.DontRequireReceiver);
            }

            Debug.Log("[PlayerController] Tấn công!");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Di chuyển player theo input — chạy trong FixedUpdate.
        /// </summary>
        private void Move()
        {
            if (_inputHandler == null)
            {
                return;
            }

            Vector2 movement = _inputHandler.MovementInput * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }

        /// <summary>
        /// Xử lý input tương tác.
        /// </summary>
        private void HandleInteraction()
        {
            if (_inputHandler != null && _inputHandler.GetInteractInput())
            {
                Interact();
            }
        }

        /// <summary>
        /// Xử lý input tấn công.
        /// </summary>
        private void HandleAttack()
        {
            if (_inputHandler != null && _inputHandler.GetAttackInput())
            {
                Attack();
            }
        }

        /// <summary>
        /// Khi ngày bắt đầu — reset daily stats, tắt combat mode.
        /// </summary>
        private void HandleDayStart()
        {
            _isCombatMode = false;
            Debug.Log("[PlayerController] Ban ngày — chế độ thu thập/xây dựng.");
        }

        /// <summary>
        /// Khi đêm bắt đầu — bật combat mode.
        /// </summary>
        private void HandleNightStart()
        {
            _isCombatMode = true;
            Debug.Log("[PlayerController] Ban đêm — chế độ chiến đấu!");
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Vẽ phạm vi tương tác (xanh lá)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _interactRange);

            // Vẽ phạm vi tấn công (đỏ)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }

        #endregion
    }
}
