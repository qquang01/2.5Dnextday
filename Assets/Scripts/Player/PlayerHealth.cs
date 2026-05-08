using UnityEngine;
using NextDay.Events;

namespace NextDay.Player
{
    /// <summary>
    /// Quản lý HP của player — nhận damage, hồi máu, chết.
    /// Publish event khi bị damage hoặc chết.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        #region Fields

        [Header("Health Settings")]
        [Tooltip("HP tối đa")]
        [SerializeField] private float _maxHealth = 100f;

        [Tooltip("HP hiện tại")]
        [SerializeField] private float _currentHealth;

        [Header("Invincibility")]
        [Tooltip("Thời gian bất tử sau khi bị đánh (giây)")]
        [SerializeField] private float _invincibilityDuration = 0.5f;

        [Header("Event Channels")]
        [Tooltip("Channel phát event game state")]
        [SerializeField] private GameEventChannel _gameEventChannel;

        private float _lastDamageTime;
        private bool _isDead;

        #endregion

        #region Properties

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        /// <summary>Tỷ lệ HP hiện tại (0 → 1).</summary>
        public float HealthPercent => _maxHealth > 0f ? Mathf.Clamp01(_currentHealth / _maxHealth) : 0f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
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
        /// Nhận sát thương. Trừ HP, check chết.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (_isDead)
            {
                return;
            }

            // Kiểm tra invincibility frame
            if (Time.time - _lastDamageTime < _invincibilityDuration)
            {
                return;
            }

            _lastDamageTime = Time.time;
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            Debug.Log($"[PlayerHealth] Nhận {damage} damage. HP: {_currentHealth}/{_maxHealth}");

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Hồi máu. Không vượt quá maxHealth.
        /// </summary>
        public void Heal(float amount)
        {
            if (_isDead)
            {
                return;
            }

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            Debug.Log($"[PlayerHealth] Hồi {amount} HP. HP: {_currentHealth}/{_maxHealth}");
        }

        /// <summary>
        /// Hồi đầy HP — dùng khi respawn hoặc bắt đầu ngày mới.
        /// </summary>
        public void FullHeal()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
        }

        /// <summary>
        /// Tăng HP tối đa (nâng cấp).
        /// </summary>
        public void IncreaseMaxHealth(float amount)
        {
            _maxHealth += amount;
            _currentHealth += amount;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Xử lý chết — publish event, disable player.
        /// </summary>
        private void Die()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            Debug.Log("[PlayerHealth] Player đã chết!");

            if (_gameEventChannel != null)
            {
                _gameEventChannel.RaisePlayerDeath();
            }
        }

        /// <summary>
        /// Khi ngày bắt đầu — hồi một phần HP.
        /// </summary>
        private void HandleDayStart()
        {
            if (_isDead)
            {
                return;
            }

            // Hồi 20% HP khi bắt đầu ngày mới
            float healAmount = _maxHealth * 0.2f;
            Heal(healAmount);
            Debug.Log("[PlayerHealth] Ngày mới — hồi 20% HP.");
        }

        #endregion
    }
}
